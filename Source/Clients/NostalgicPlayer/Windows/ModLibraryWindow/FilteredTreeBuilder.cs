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
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Events;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Sources;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow
{
	/// <summary>
	/// Search mode for filtering files
	/// </summary>
	internal enum SearchMode
	{
		FilenameAndPath = 0,
		FilenameOnly = 1,
		PathOnly = 2
	}

	/// <summary>
	/// Builds filtered tree in background with cancellation support
	/// </summary>
	internal class FilteredTreeBuilder
	{
		private readonly string filter;

		private readonly bool isOfflineMode;
		private readonly List<ModEntry> localFiles;

		// Cache for fast node lookup during tree building
		private readonly Dictionary<string, TreeNode> nodeCache = new();
		private readonly SearchMode searchMode;
		private readonly List<IModLibrarySource> sources;
		private readonly SynchronizationContext syncContext;
		private bool cancelled;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FilteredTreeBuilder(List<IModLibrarySource> sources, List<ModEntry> localFiles, string filter, bool isOfflineMode, SearchMode searchMode)
		{
			this.sources = sources;
			this.localFiles = localFiles;  // Already a snapshot from ModLibraryData
			this.filter = filter;
			this.isOfflineMode = isOfflineMode;
			this.searchMode = searchMode;
			syncContext = SynchronizationContext.Current;
		}



		/********************************************************************/
		/// <summary>
		/// Event fired when tree building is completed
		/// </summary>
		/********************************************************************/
		public event EventHandler<TreeBuildCompletedEventArgs> Completed;



		/********************************************************************/
		/// <summary>
		/// Add file and parent directories to filtered tree using entry's
		/// PathParts
		/// </summary>
		/********************************************************************/
		private void AddFileToFilteredTree(TreeNode serviceNode, IModLibrarySource source, ModEntry entry)
		{
			var currentNode = serviceNode;
			string currentPath = string.Empty;

			// Use the pre-parsed PathParts from ModEntry!
			foreach (string part in entry.PathParts)
			{
				if (cancelled)
					return;

				currentPath = string.IsNullOrEmpty(currentPath) ? part : currentPath + "/" + part;
				string fullPath = source.RootPath + currentPath;

				// Use cache for O(1) lookup instead of O(n) search
				if (!nodeCache.TryGetValue(fullPath, out TreeNode childNode))
				{
					childNode = new TreeNode
					{
						Name = part,
						FullPath = fullPath,
						IsDirectory = true,
						Size = 0,
						SourceId = source.Id
					};

					currentNode.Children.Add(childNode);
					nodeCache[fullPath] = childNode;
				}

				currentNode = childNode;
			}

			// Add the file itself
			string fileName = entry.Name;
			string fileFullPath = source.RootPath + entry.FullName;

			// Check cache first
			if (!nodeCache.ContainsKey(fileFullPath))
			{
				TreeNode fileNode = new TreeNode
				{
					Name = fileName,
					FullPath = fileFullPath,
					IsDirectory = false,
					Size = entry.Size,
					SourceId = source.Id
				};

				currentNode.Children.Add(fileNode);
				nodeCache[fileFullPath] = fileNode;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Add file to local tree (without service)
		/// </summary>
		/********************************************************************/
		private void AddFileToLocalTree(TreeNode rootNode, ModEntry entry)
		{
			var currentNode = rootNode;
			string currentPath = string.Empty;

			// Use the pre-parsed PathParts from ModEntry!
			foreach (string part in entry.PathParts)
			{
				if (cancelled)
					return;

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
						SourceId = string.Empty
					};

					currentNode.Children.Add(childNode);
					nodeCache[currentPath] = childNode;
				}

				currentNode = childNode;
			}

			// Add file node
			string fileName = entry.Name;
			string fileFullPath = entry.FullName;

			// Check cache first
			if (!nodeCache.ContainsKey(fileFullPath))
			{
				TreeNode fileNode = new()
				{
					Name = fileName,
					FullPath = fileFullPath,
					IsDirectory = false,
					Size = entry.Size,
					SourceId = string.Empty
				};

				currentNode.Children.Add(fileNode);
				nodeCache[fileFullPath] = fileNode;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Build filtered tree from services
		/// </summary>
		/********************************************************************/
		private void BuildTree()
		{
			try
			{
				// Clear cache from previous builds
				nodeCache.Clear();

				TreeNode root = new TreeNode { Name = "Root", FullPath = string.Empty, IsDirectory = true };

				bool showAll = string.IsNullOrEmpty(filter);
				var filterRegex = showAll ? null : ConvertWildcardToRegex(filter);

				// Build tree differently for local vs online mode
				if (isOfflineMode)
					BuildTreeOfflineMode(root, showAll, filterRegex);
				else
					BuildTreeOnlineMode(root, showAll, filterRegex);

				// Fire event with tree (no DisplayCache!)
				NotifyCompletion(root);
			}
			catch (Exception)
			{
				// Silently ignore errors if cancelled
				if (!cancelled)
					throw;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Build tree for offline mode (local files)
		/// </summary>
		/********************************************************************/
		private void BuildTreeOfflineMode(TreeNode root, bool showAll, Regex filterRegex)
		{
			// Local mode: Use unified local files list (service-independent)
			foreach (ModEntry entry in localFiles)
			{
				if (cancelled)
					return;

				bool matchesFilter = showAll;

				if (!showAll)
				{
					// Use regex for wildcard matching
					matchesFilter = searchMode switch
					{
						SearchMode.FilenameAndPath => filterRegex.IsMatch(entry.FullName),
						SearchMode.FilenameOnly => filterRegex.IsMatch(entry.Name),
						SearchMode.PathOnly => filterRegex.IsMatch(entry.FullPath),
						_ => false
					};
				}

				if (!matchesFilter)
					continue;

				// Always build tree structure (needed for flat view navigation too)
				AddFileToLocalTree(root, entry);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Build tree for online mode (service files)
		/// </summary>
		/********************************************************************/
		private void BuildTreeOnlineMode(TreeNode root, bool showAll, Regex filterRegex)
		{
			// Online mode: Use service-specific online files
			foreach (IModLibrarySource source in sources)
			{
				if (cancelled)
					return;

				// Build display name with status info
				IReadOnlyList<ModEntry> files = source.OnlineFiles;
				string displayName;

				if (source.IsLoaded && (files.Count > 0))
				{
					// Online mode: Database loaded
					int fileCount = files.Count;
					displayName = string.Format(Resources.IDS_MODLIBRARY_ROOT_DOWNLOADED, source.DisplayName, source.LastUpdate, fileCount);
				}
				else
				{
					// Online mode: Not downloaded
					displayName = string.Format(Resources.IDS_MODLIBRARY_ROOT_NOTDOWNLOADED, source.DisplayName);
				}

				TreeNode serviceNode = new TreeNode { Name = displayName, FullPath = source.RootPath, IsDirectory = true, SourceId = source.Id };

				// Always add service node (needed for flat view navigation too)
				root.Children.Add(serviceNode);

				// If service is loaded, add filtered files
				if (source.IsLoaded)
					 ProcessServiceFiles(serviceNode, source, showAll, filterRegex);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Convert wildcard pattern to regex pattern
		/// </summary>
		/********************************************************************/
		private Regex ConvertWildcardToRegex(string pattern)
		{
			// Check if pattern contains wildcards
			bool hasWildcards = pattern.Contains('*') || pattern.Contains('?');

			// If no wildcards, auto-add * around the pattern
			if (!hasWildcards)
				pattern = "*" + pattern + "*";

			// Escape regex special characters except * and ?
			string regexPattern = Regex.Escape(pattern);

			// Convert wildcards to regex
			regexPattern = regexPattern.Replace("\\*", ".*"); // * -> .* (zero or more chars)
			regexPattern = regexPattern.Replace("\\?", "."); // ? -> . (exactly one char)

			// Anchor pattern
			regexPattern = "^" + regexPattern + "$";

			return new Regex(regexPattern, RegexOptions.IgnoreCase);
		}



		/********************************************************************/
		/// <summary>
		/// Notify completion and fire event
		/// </summary>
		/********************************************************************/
		private void NotifyCompletion(TreeNode root)
		{
			if (!cancelled && Completed != null)
			{
				TreeBuildCompletedEventArgs args = new TreeBuildCompletedEventArgs(root);

				if (syncContext != null)
					syncContext.Post(_ => Completed?.Invoke(this, args), null);
				else
					Completed?.Invoke(this, args);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Process files for a specific service
		/// </summary>
		/********************************************************************/
		private void ProcessServiceFiles(TreeNode serviceNode, IModLibrarySource source, bool showAll, Regex filterRegex)
		{
			foreach (var entry in source.OnlineFiles)
			{
				if (cancelled)
					return;

				// ModEntry only contains files, no directories!
				bool matchesFilter = showAll;

				if (!showAll)
				{
					// Use regex for wildcard matching
					switch (searchMode)
					{
						case SearchMode.FilenameAndPath:
						{
							matchesFilter = filterRegex.IsMatch(entry.Name) || filterRegex.IsMatch(entry.FullName);
							break;
						}

						case SearchMode.FilenameOnly:
						{
							matchesFilter = filterRegex.IsMatch(entry.Name);
							break;
						}

						case SearchMode.PathOnly:
						{
							matchesFilter = filterRegex.IsMatch(entry.FullPath);
							break;
						}
					}
				}

				if (matchesFilter)
				{
					// Always build tree structure (needed for flat view navigation too)
					AddFileToFilteredTree(serviceNode, source, entry);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Cancel the build task
		/// </summary>
		/********************************************************************/
		public void Cancel()
		{
			cancelled = true;
			Completed = null; // Remove all event handlers
		}



		/********************************************************************/
		/// <summary>
		/// Start building the tree in background
		/// </summary>
		/********************************************************************/
		public void Start()
		{
			Task.Run(BuildTree);
		}
	}
}
