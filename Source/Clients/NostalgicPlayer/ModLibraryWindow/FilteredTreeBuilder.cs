/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.ModLibraryWindow
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
		private bool cancelled = false;
		private List<ModuleService> services;
		private string filter;
		private bool isOfflineMode;
		private SearchMode searchMode;
		private bool isFlatView;

		// Cache for fast node lookup during tree building
		private Dictionary<string, TreeNode> nodeCache = new Dictionary<string, TreeNode>();

		public event EventHandler<TreeBuildCompletedEventArgs> Completed;



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FilteredTreeBuilder(List<ModuleService> services, string filter, bool isOfflineMode, SearchMode searchMode, bool isFlatView)
		{
			this.services = services;
			this.filter = filter;
			this.isOfflineMode = isOfflineMode;
			this.searchMode = searchMode;
			this.isFlatView = isFlatView;
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
			Task.Run(() => BuildTree());
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
			{
				pattern = "*" + pattern + "*";
			}

			// Escape regex special characters except * and ?
			string regexPattern = Regex.Escape(pattern);

			// Convert wildcards to regex
			regexPattern = regexPattern.Replace("\\*", ".*");  // * -> .* (zero or more chars)
			regexPattern = regexPattern.Replace("\\?", ".");   // ? -> . (exactly one char)

			// Anchor pattern
			regexPattern = "^" + regexPattern + "$";

			return new Regex(regexPattern, RegexOptions.IgnoreCase);
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

				TreeNode root = new TreeNode { Name = "Root", FullPath = "", IsDirectory = true };

				bool showAll = string.IsNullOrEmpty(filter);
				Regex filterRegex = showAll ? null : ConvertWildcardToRegex(filter);

				// Collect all matched files across all services for flat view
				List<(ModEntry entry, ModuleService service)> allMatchedFiles = new List<(ModEntry, ModuleService)>();

				// Build tree - create service nodes and add files in one pass
				foreach (var service in services)
				{
					if (cancelled) return;

					// Build display name with status info
					var files = isOfflineMode ? service.OfflineFiles : service.OnlineFiles;
					string displayName;

					if (isOfflineMode)
					{
						// Offline mode: Always show local files count (can be 0)
						int fileCount = files.Count;
						displayName = $"{service.DisplayName} ({fileCount:N0} local files)";
					}
					else if (service.IsLoaded && files.Count > 0)
					{
						// Online mode: Database loaded
						int fileCount = files.Count;
						displayName = $"{service.DisplayName} ({service.LastUpdate:yyyy-MM-dd}, {fileCount:N0} files)";
					}
					else
					{
						// Online mode: Not downloaded
						displayName = $"{service.DisplayName} (not downloaded)";
					}

					TreeNode serviceNode = new TreeNode
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
						foreach (var entry in files)
						{
							if (cancelled) return;

							// ModEntry only contains files, no directories!
							bool matchesFilter = showAll;

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
									allMatchedFiles.Add((entry, service));
								}
								else
								{
									// Hierarchical view: Add to tree structure
									AddFileToFilteredTree(serviceNode, service, entry);
								}
							}
						}
					}
				}

				// In flat view, add all files directly under root (sorting happens in GetEntries)
				if (isFlatView && allMatchedFiles.Count > 0)
				{
					foreach (var (entry, service) in allMatchedFiles)
					{
						if (cancelled) return;

						TreeNode fileNode = new TreeNode
						{
							Name = entry.Name,
							FullPath = service.RootPath + entry.FullName,
							IsDirectory = false,
							Size = entry.Size,
							ServiceId = service.Id
						};
						root.Children.Add(fileNode);
					}
				}

				// Fire event with tree (no DisplayCache!)
				if (!cancelled && Completed != null)
				{
					Completed?.Invoke(this, new TreeBuildCompletedEventArgs(root));
				}
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
		/// Add file and parent directories to filtered tree using entry's PathParts
		/// </summary>
		/********************************************************************/
		private void AddFileToFilteredTree(TreeNode serviceNode, ModuleService service, ModEntry entry)
		{
			TreeNode currentNode = serviceNode;
			string currentPath = "";

			// Use the pre-parsed PathParts from ModEntry!
			for (int i = 0; i < entry.PathParts.Count; i++)
			{
				if (cancelled) return;

				string part = entry.PathParts[i];
				currentPath = string.IsNullOrEmpty(currentPath) ? part : currentPath + "/" + part;
				string fullPath = service.RootPath + currentPath;

				// Use cache for O(1) lookup instead of O(n) search
				if (!nodeCache.TryGetValue(fullPath, out TreeNode childNode))
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
			string fileName = entry.Name;
			string fileFullPath = service.RootPath + entry.FullName;

			// Check cache first
			if (!nodeCache.ContainsKey(fileFullPath))
			{
				TreeNode fileNode = new TreeNode
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
	}



	/// <summary>
	/// Event args for tree build completion
	/// </summary>
	internal class TreeBuildCompletedEventArgs : EventArgs
	{
		public TreeNode ResultTree { get; }

		public TreeBuildCompletedEventArgs(TreeNode tree)
		{
			ResultTree = tree;
		}
	}
}
