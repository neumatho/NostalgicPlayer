/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.ModLibraryWindow
{
    /// <summary>
    ///     Sort order for flat view
    /// </summary>
    internal enum FlatViewSortOrder
    {
        NameThenPath = 0,
        PathThenName = 1
    }

    /// <summary>
    ///     Manages module library data, services, and search functionality
    /// </summary>
    internal class ModLibraryData
    {
        // Local files (service-independent)
        private readonly List<ModEntry> localFiles = new();
        private FilteredTreeBuilder currentBuilder;

        // Current search results
        private TreeNode currentTree;


        /********************************************************************/
        /// <summary>
        ///     Get or set local mode
        /// </summary>
        /********************************************************************/
        public bool IsOfflineMode { get; set; }


        /********************************************************************/
        /// <summary>
        ///     Get readonly access to local files
        /// </summary>
        /********************************************************************/
        public IReadOnlyList<ModEntry> LocalFiles => localFiles;


        /********************************************************************/
        /// <summary>
        ///     Get current search filter
        /// </summary>
        /********************************************************************/
        public string SearchFilter { get; private set; } = string.Empty;


        /********************************************************************/
        /// <summary>
        ///     Get all available services
        /// </summary>
        /********************************************************************/
        public List<ModuleService> Services { get; } = new();

        /// <summary>
        ///     Event fired when data loading is completed
        /// </summary>
        public event EventHandler DataLoaded;


        /********************************************************************/
        /// <summary>
        ///     Count files recursively in tree
        /// </summary>
        /********************************************************************/
        private int CountFilesRecursive(TreeNode node)
        {
            var count = 0;

            foreach (var child in node.Children)
            {
                if (!child.IsDirectory)
                {
                    count++;
                }
                else
                {
                    count += CountFilesRecursive(child);
                }
            }

            return count;
        }


        /********************************************************************/
        /// <summary>
        ///     Check if node has any children (recursively)
        /// </summary>
        /********************************************************************/
        private bool HasChildren(TreeNode node)
        {
            if (node.Children == null || node.Children.Count == 0)
            {
                return false;
            }

            // Check if any child is a file
            foreach (var child in node.Children)
            {
                if (!child.IsDirectory)
                {
                    return true;
                }

                // Recursively check subdirectories
                if (HasChildren(child))
                {
                    return true;
                }
            }

            return false;
        }


        /********************************************************************/
        /// <summary>
        ///     Called when search is completed
        /// </summary>
        /********************************************************************/
        private void OnSearchCompleted(object sender, TreeBuildCompletedEventArgs e)
        {
            // Store results
            currentTree = e.ResultTree;

            // Clear builder reference
            currentBuilder = null;

            // Notify UI (already on UI thread via FilteredTreeBuilder)
            DataLoaded?.Invoke(this, EventArgs.Empty);
        }


        /********************************************************************/
        /// <summary>
        ///     Add local file - creates ModEntry from already sorted data
        /// </summary>
        /********************************************************************/
        public void AddLocalFile(string nameWithPath, long size) => localFiles.Add(new ModEntry(nameWithPath, size));


        /********************************************************************/
        /// <summary>
        ///     Add a service to the library
        /// </summary>
        /********************************************************************/
        public void AddService(ModuleService service) => Services.Add(service);


        /********************************************************************/
        /// <summary>
        ///     Build tree with optional filter (empty filter shows all files)
        /// </summary>
        /********************************************************************/
        public void BuildTree(string filter, SearchMode searchMode = SearchMode.FilenameAndPath, bool isFlatView = false)
        {
            SearchFilter = filter.Trim();

            // Cancel previous search if running
            if (currentBuilder != null)
            {
                currentBuilder.Cancel();
                currentBuilder = null;
            }

            // Build tree (empty filter shows everything)
            currentBuilder = new FilteredTreeBuilder(Services, LocalFiles, SearchFilter, IsOfflineMode, searchMode, isFlatView);
            currentBuilder.Completed += OnSearchCompleted;
            currentBuilder.Start();
        }


        /********************************************************************/
        /// <summary>
        ///     Clear local files
        /// </summary>
        /********************************************************************/
        public void ClearLocalFiles() => localFiles.Clear();


        /********************************************************************/
        /// <summary>
        ///     Count total files in current filtered tree
        /// </summary>
        /********************************************************************/
        public int CountTotalFilesInFilteredCache()
        {
            if (currentTree == null)
            {
                return 0;
            }

            return CountFilesRecursive(currentTree);
        }


        /********************************************************************/
        /// <summary>
        ///     Get display path for breadcrumb
        /// </summary>
        /********************************************************************/
        public string GetDisplayPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "Root";
            }

            var service = GetServiceFromPath(path);
            if (service != null)
            {
                var relativePath = GetRelativePathFromService(path, service);
                if (string.IsNullOrEmpty(relativePath))
                {
                    return service.DisplayName;
                }

                return $"{service.DisplayName}/{relativePath}";
            }

            return path;
        }


        /********************************************************************/
        /// <summary>
        ///     Get entries for a specific path with optional flat view sort order
        /// </summary>
        /********************************************************************/
        public List<TreeNode> GetEntries(string currentPath, bool isFlatView, FlatViewSortOrder sortOrder)
        {
            // In flat view, always use root to get all files
            var pathToUse = isFlatView ? string.Empty : currentPath;

            var node = currentTree?.FindByPath(pathToUse);
            if (node == null)
            {
                return new List<TreeNode>();
            }

            // Create a copy
            var sortedChildren = new List<TreeNode>(node.Children);

            // If at root with search filter in hierarchical view, filter out empty services
            if (!isFlatView && string.IsNullOrEmpty(pathToUse) && !string.IsNullOrEmpty(SearchFilter))
            {
                sortedChildren = sortedChildren.Where(child => HasChildren(child)).ToList();
            }

            if (isFlatView)
            {
                // Flat view: Sort by name→path or path→name (all entries are files)
                sortedChildren.Sort((a, b) =>
                {
                    if (sortOrder == FlatViewSortOrder.NameThenPath)
                    {
                        // Primary: Name, Secondary: Path
                        var nameCompare = string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
                        if (nameCompare != 0)
                        {
                            return nameCompare;
                        }

                        // If names are equal, sort by full path
                        return string.Compare(a.FullPath, b.FullPath, StringComparison.OrdinalIgnoreCase);
                    }

                    // PathThenName
                    // Primary: Path, Secondary: Name
                    var pathCompare = string.Compare(a.FullPath, b.FullPath, StringComparison.OrdinalIgnoreCase);
                    if (pathCompare != 0)
                    {
                        return pathCompare;
                    }

                    // If paths are equal, sort by name (shouldn't happen but just in case)
                    return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
                });
            }
            else
            {
                // Hierarchical view: directories first, then files (both alphabetically by name)
                sortedChildren.Sort((a, b) =>
                {
                    // Directories come before files
                    if (a.IsDirectory && !b.IsDirectory)
                    {
                        return -1;
                    }

                    if (!a.IsDirectory && b.IsDirectory)
                    {
                        return 1;
                    }

                    // Both are same type (both dirs or both files), sort alphabetically by name
                    return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
                });
            }

            return sortedChildren;
        }


        /********************************************************************/
        /// <summary>
        ///     Get relative path without service prefix
        /// </summary>
        /********************************************************************/
        public string GetRelativePathFromService(string fullPath, ModuleService service)
        {
            if (fullPath.StartsWith(service.RootPath))
            {
                return fullPath.Substring(service.RootPath.Length);
            }

            return string.Empty;
        }


        /********************************************************************/
        /// <summary>
        ///     Get service by ID
        /// </summary>
        /********************************************************************/
        public ModuleService GetService(string serviceId) => Services.FirstOrDefault(s => s.Id == serviceId);


        /********************************************************************/
        /// <summary>
        ///     Get service from full path
        /// </summary>
        /********************************************************************/
        public ModuleService GetServiceFromPath(string path)
        {
            foreach (var service in Services)
            {
                if (path.StartsWith(service.RootPath))
                {
                    return service;
                }
            }

            return null;
        }
    }
}