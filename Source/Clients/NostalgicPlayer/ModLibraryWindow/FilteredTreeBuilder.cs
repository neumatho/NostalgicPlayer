/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.ModLibraryWindow
{
    /// <summary>
    ///     Search mode for filtering files
    /// </summary>
    internal enum SearchMode
    {
        FilenameAndPath = 0,
        FilenameOnly = 1,
        PathOnly = 2
    }

    /// <summary>
    ///     Builds filtered tree in background with cancellation support
    /// </summary>
    internal class FilteredTreeBuilder
    {
        private readonly string filter;
        private readonly bool isFlatView;

        private readonly bool isOfflineMode;
        private readonly IReadOnlyList<ModEntry> localFiles;

        // Cache for fast node lookup during tree building
        private readonly Dictionary<string, TreeNode> nodeCache = new();
        private readonly SearchMode searchMode;
        private readonly List<ModuleService> services;
        private readonly SynchronizationContext syncContext;
        private bool cancelled;


        /********************************************************************/
        /// <summary>
        ///     Constructor
        /// </summary>
        /********************************************************************/
        public FilteredTreeBuilder(List<ModuleService> services, IReadOnlyList<ModEntry> localFiles, string filter, bool isOfflineMode, SearchMode searchMode, bool isFlatView)
        {
            this.services = services;
            this.localFiles = localFiles;
            this.filter = filter;
            this.isOfflineMode = isOfflineMode;
            this.searchMode = searchMode;
            this.isFlatView = isFlatView;
            syncContext = SynchronizationContext.Current;
        }

        /// <summary>
        ///     Event fired when tree building is completed
        /// </summary>
        public event EventHandler<TreeBuildCompletedEventArgs> Completed;


        /********************************************************************/
        /// <summary>
        ///     Add file and parent directories to filtered tree using entry's PathParts
        /// </summary>
        /********************************************************************/
        private void AddFileToFilteredTree(TreeNode serviceNode, ModuleService service, ModEntry entry)
        {
            var currentNode = serviceNode;
            var currentPath = string.Empty;

            // Use the pre-parsed PathParts from ModEntry!
            for (var i = 0; i < entry.PathParts.Count; i++)
            {
                if (cancelled)
                {
                    return;
                }

                var part = entry.PathParts[i];
                currentPath = string.IsNullOrEmpty(currentPath) ? part : currentPath + "/" + part;
                var fullPath = service.RootPath + currentPath;

                // Use cache for O(1) lookup instead of O(n) search
                if (!nodeCache.TryGetValue(fullPath, out var childNode))
                {
                    childNode = new TreeNode
                    {
                        Name = part,
                        FullPath = fullPath,
                        IsDirectory = true,
                        Size = 0,
                        ServiceId = service.Id
                    };
                    currentNode.Children.Add(childNode);
                    nodeCache[fullPath] = childNode;
                }

                currentNode = childNode;
            }

            // Add the file itself
            var fileName = entry.Name;
            var fileFullPath = service.RootPath + entry.FullName;

            // Check cache first
            if (!nodeCache.ContainsKey(fileFullPath))
            {
                var fileNode = new TreeNode
                {
                    Name = fileName,
                    FullPath = fileFullPath,
                    IsDirectory = false,
                    Size = entry.Size,
                    ServiceId = service.Id
                };
                currentNode.Children.Add(fileNode);
                nodeCache[fileFullPath] = fileNode;
            }
        }


        /********************************************************************/
        /// <summary>
        ///     Add file to local tree (without service)
        /// </summary>
        /********************************************************************/
        private void AddFileToLocalTree(TreeNode rootNode, ModEntry entry)
        {
            var currentNode = rootNode;
            var currentPath = string.Empty;

            // Use the pre-parsed PathParts from ModEntry!
            for (var i = 0; i < entry.PathParts.Count; i++)
            {
                if (cancelled)
                {
                    return;
                }

                var part = entry.PathParts[i];
                currentPath = string.IsNullOrEmpty(currentPath) ? part : currentPath + "/" + part;

                // Use cache for O(1) lookup instead of O(n) search
                if (!nodeCache.TryGetValue(currentPath, out var childNode))
                {
                    childNode = new TreeNode
                    {
                        Name = part,
                        FullPath = currentPath,
                        IsDirectory = true,
                        Size = 0,
                        ServiceId = string.Empty
                    };
                    currentNode.Children.Add(childNode);
                    nodeCache[currentPath] = childNode;
                }

                currentNode = childNode;
            }

            // Add file node
            var fileName = entry.Name;
            var fileFullPath = entry.FullName;

            // Check cache first
            if (!nodeCache.ContainsKey(fileFullPath))
            {
                var fileNode = new TreeNode
                {
                    Name = fileName,
                    FullPath = fileFullPath,
                    IsDirectory = false,
                    Size = entry.Size,
                    ServiceId = string.Empty
                };
                currentNode.Children.Add(fileNode);
                nodeCache[fileFullPath] = fileNode;
            }
        }


        /********************************************************************/
        /// <summary>
        ///     Add all matched files for flat view
        /// </summary>
        /********************************************************************/
        private void AddFlatViewFiles(TreeNode root, List<(ModEntry entry, ModuleService service)> allMatchedFiles)
        {
            foreach (var (entry, service) in allMatchedFiles)
            {
                if (cancelled)
                {
                    return;
                }

                var fileNode = new TreeNode
                {
                    Name = entry.Name,
                    FullPath = service != null ? service.RootPath + entry.FullName : entry.FullName,
                    IsDirectory = false,
                    Size = entry.Size,
                    ServiceId = service?.Id ?? string.Empty
                };
                root.Children.Add(fileNode);
            }
        }


        /********************************************************************/
        /// <summary>
        ///     Build filtered tree from services
        /// </summary>
        /********************************************************************/
        private void BuildTree()
        {
            try
            {
                // Clear cache from previous builds
                nodeCache.Clear();

                var root = new TreeNode { Name = "Root", FullPath = string.Empty, IsDirectory = true };

                var showAll = string.IsNullOrEmpty(filter);
                var filterRegex = showAll ? null : ConvertWildcardToRegex(filter);

                // Build tree differently for local vs online mode
                var allMatchedFiles = isOfflineMode
                    ? BuildTreeOfflineMode(root, showAll, filterRegex)
                    : BuildTreeOnlineMode(root, showAll, filterRegex);

                // In flat view, add all files directly under root (sorting happens in GetEntries)
                if (isFlatView && allMatchedFiles.Count > 0)
                {
                    AddFlatViewFiles(root, allMatchedFiles);
                }

                // Fire event with tree (no DisplayCache!)
                NotifyCompletion(root);
            }
            catch (Exception)
            {
                // Silently ignore errors if cancelled
                if (!cancelled)
                {
                    throw;
                }
            }
        }


        /********************************************************************/
        /// <summary>
        ///     Build tree for offline mode (local files)
        /// </summary>
        /********************************************************************/
        private List<(ModEntry entry, ModuleService service)> BuildTreeOfflineMode(TreeNode root, bool showAll, Regex filterRegex)
        {
            var allMatchedFiles = new List<(ModEntry entry, ModuleService service)>();

            // Local mode: Use unified local files list (service-independent)
            foreach (var entry in localFiles)
            {
                if (cancelled)
                {
                    return allMatchedFiles;
                }

                var matchesFilter = showAll;

                if (!showAll)
                {
                    // Use regex for wildcard matching
                    matchesFilter = searchMode switch
                    {
                        SearchMode.FilenameAndPath => filterRegex.IsMatch(entry.FullName),
                        SearchMode.FilenameOnly => filterRegex.IsMatch(entry.Name),
                        SearchMode.PathOnly => filterRegex.IsMatch(entry.FullPath),
                        _ => matchesFilter
                    };
                }

                if (!matchesFilter)
                {
                    continue;
                }

                if (isFlatView)
                {
                    // Flat view: Add to allMatchedFiles for sorting
                    allMatchedFiles.Add((entry, null)); // No service in local mode
                }
                else
                {
                    // Hierarchical view: Build tree structure directly from root
                    AddFileToLocalTree(root, entry);
                }
            }

            return allMatchedFiles;
        }


        /********************************************************************/
        /// <summary>
        ///     Build tree for online mode (service files)
        /// </summary>
        /********************************************************************/
        private List<(ModEntry entry, ModuleService service)> BuildTreeOnlineMode(TreeNode root, bool showAll, Regex filterRegex)
        {
            var allMatchedFiles = new List<(ModEntry entry, ModuleService service)>();

            // Online mode: Use service-specific online files
            foreach (var service in services)
            {
                if (cancelled)
                {
                    return allMatchedFiles;
                }

                // Build display name with status info
                var files = service.OnlineFiles;
                string displayName;

                if (service.IsLoaded && files.Count > 0)
                {
                    // Online mode: Database loaded
                    var fileCount = files.Count;
                    displayName = $"{service.DisplayName} ({service.LastUpdate:yyyy-MM-dd}, {fileCount:N0} files)";
                }
                else
                {
                    // Online mode: Not downloaded
                    displayName = $"{service.DisplayName} (not downloaded)";
                }

                var serviceNode = new TreeNode
                {
                    Name = displayName,
                    FullPath = service.RootPath,
                    IsDirectory = true,
                    ServiceId = service.Id
                };

                // Only add service node if NOT in flat view
                if (!isFlatView)
                {
                    root.Children.Add(serviceNode);
                }

                // If service is loaded, add filtered files
                if (service.IsLoaded)
                {
                    var serviceFiles = ProcessServiceFiles(serviceNode, service, showAll, filterRegex);
                    allMatchedFiles.AddRange(serviceFiles);
                }
            }

            return allMatchedFiles;
        }


        /********************************************************************/
        /// <summary>
        ///     Convert wildcard pattern to regex pattern
        /// </summary>
        /********************************************************************/
        private Regex ConvertWildcardToRegex(string pattern)
        {
            // Check if pattern contains wildcards
            var hasWildcards = pattern.Contains('*') || pattern.Contains('?');

            // If no wildcards, auto-add * around the pattern
            if (!hasWildcards)
            {
                pattern = "*" + pattern + "*";
            }

            // Escape regex special characters except * and ?
            var regexPattern = Regex.Escape(pattern);

            // Convert wildcards to regex
            regexPattern = regexPattern.Replace("\\*", ".*"); // * -> .* (zero or more chars)
            regexPattern = regexPattern.Replace("\\?", "."); // ? -> . (exactly one char)

            // Anchor pattern
            regexPattern = "^" + regexPattern + "$";

            return new Regex(regexPattern, RegexOptions.IgnoreCase);
        }


        /********************************************************************/
        /// <summary>
        ///     Notify completion and fire event
        /// </summary>
        /********************************************************************/
        private void NotifyCompletion(TreeNode root)
        {
            if (!cancelled && Completed != null)
            {
                var args = new TreeBuildCompletedEventArgs(root);
                if (syncContext != null)
                {
                    syncContext.Post(_ => Completed?.Invoke(this, args), null);
                }
                else
                {
                    Completed?.Invoke(this, args);
                }
            }
        }


        /********************************************************************/
        /// <summary>
        ///     Process files for a specific service
        /// </summary>
        /********************************************************************/
        private List<(ModEntry entry, ModuleService service)> ProcessServiceFiles(TreeNode serviceNode, ModuleService service, bool showAll, Regex filterRegex)
        {
            var matchedFiles = new List<(ModEntry entry, ModuleService service)>();

            foreach (var entry in service.OnlineFiles)
            {
                if (cancelled)
                {
                    return matchedFiles;
                }

                // ModEntry only contains files, no directories!
                var matchesFilter = showAll;

                if (!showAll)
                {
                    // Use regex for wildcard matching
                    switch (searchMode)
                    {
                        case SearchMode.FilenameAndPath:
                            matchesFilter = filterRegex.IsMatch(entry.Name) || filterRegex.IsMatch(entry.FullName);
                            break;
                        case SearchMode.FilenameOnly:
                            matchesFilter = filterRegex.IsMatch(entry.Name);
                            break;
                        case SearchMode.PathOnly:
                            matchesFilter = filterRegex.IsMatch(entry.FullPath);
                            break;
                    }
                }

                if (matchesFilter)
                {
                    if (isFlatView)
                    {
                        // Flat view: Collect for later sorting (across all services)
                        matchedFiles.Add((entry, service));
                    }
                    else
                    {
                        // Hierarchical view: Add to tree structure
                        AddFileToFilteredTree(serviceNode, service, entry);
                    }
                }
            }

            return matchedFiles;
        }


        /********************************************************************/
        /// <summary>
        ///     Cancel the build task
        /// </summary>
        /********************************************************************/
        public void Cancel()
        {
            cancelled = true;
            Completed = null; // Remove all event handlers
        }


        /********************************************************************/
        /// <summary>
        ///     Start building the tree in background
        /// </summary>
        /********************************************************************/
        public void Start() => Task.Run(() => BuildTree());
    }
}