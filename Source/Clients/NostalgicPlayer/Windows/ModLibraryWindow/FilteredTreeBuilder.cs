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
		private readonly bool isFlatView;

		private readonly bool isOfflineMode;
		private readonly List<ModEntry> localFiles;

		// Cache for fast node lookup during tree building
		private readonly Dictionary<string, TreeNode> nodeCache = new();
		private readonly SearchMode searchMode;
		private readonly List<ModuleService> services;
		private readonly SynchronizationContext syncContext;
		private bool cancelled;


		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FilteredTreeBuilder(List<ModuleService> services, List<ModEntry> localFiles, string filter,
			bool isOfflineMode, SearchMode searchMode, bool isFlatView)
		{
			this.services = services;
			this.localFiles = localFiles;  // Already a snapshot from ModLibraryData
			this.filter = filter;
			this.isOfflineMode = isOfflineMode;
			this.searchMode = searchMode;
			this.isFlatView = isFlatView;
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
		private void AddFileToFilteredTree(TreeNode serviceNode, ModuleService service, ModEntry entry)
		{
			var currentNode = serviceNode;
			string currentPath = string.Empty;

			// Use the pre-parsed PathParts from ModEntry!
			for (int i = 0; i < entry.PathParts.Count; i++)
			{
				if (cancelled)
				{
					return;
				}

				string part = entry.PathParts[i];
				currentPath = string.IsNullOrEmpty(currentPath) ? part : currentPath + "/" + part;
				string fullPath = service.RootPath + currentPath;

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
			string fileName = entry.Name;
			string fileFullPath = service.RootPath + entry.FullName;

			// Check cache first
			if (!nodeCache.ContainsKey(fileFullPath))
			{
				TreeNode fileNode = new()
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
		/// Add file to local tree (without service)
		/// </summary>
		/********************************************************************/
		private void AddFileToLocalTree(TreeNode rootNode, ModEntry entry)
		{
			var currentNode = rootNode;
			string currentPath = string.Empty;

			// Use the pre-parsed PathParts from ModEntry!
			for (int i = 0; i < entry.PathParts.Count; i++)
			{
				if (cancelled)
				{
					return;
				}

				string part = entry.PathParts[i];
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
					ServiceId = string.Empty
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

				TreeNode root = new() {Name = "Root", FullPath = string.Empty, IsDirectory = true};

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
				{
					throw;
				}
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
			foreach (var entry in localFiles)
			{
				if (cancelled)
				{
					return;
				}

				bool matchesFilter = showAll;

				if (!showAll)
					// Use regex for wildcard matching
				{
					matchesFilter = searchMode switch
					{
						SearchMode.FilenameAndPath => filterRegex.IsMatch(entry.FullName),
						SearchMode.FilenameOnly => filterRegex.IsMatch(entry.Name),
						SearchMode.PathOnly => filterRegex.IsMatch(entry.FullPath),
						_ => false
					};
				}

				if (!matchesFilter)
				{
					continue;
				}

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
			foreach (var service in services)
			{
				if (cancelled)
				{
					return;
				}

				// Build display name with status info
				var files = service.OnlineFiles;
				string displayName;

				if (service.IsLoaded && files.Count > 0)
				{
					// Online mode: Database loaded
					int fileCount = files.Count;
					displayName = $"{service.DisplayName} ({service.LastUpdate:yyyy-MM-dd}, {fileCount:N0} files)";
				}
				else
					// Online mode: Not downloaded
				{
					displayName = $"{service.DisplayName} (not downloaded)";
				}

				TreeNode serviceNode = new() {Name = displayName, FullPath = service.RootPath, IsDirectory = true, ServiceId = service.Id};

				// Always add service node (needed for flat view navigation too)
				root.Children.Add(serviceNode);

				// If service is loaded, add filtered files
				if (service.IsLoaded)
				{
					 ProcessServiceFiles(serviceNode, service, showAll, filterRegex);
				}
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
			{
				pattern = "*" + pattern + "*";
			}

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
				TreeBuildCompletedEventArgs args = new(root);
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
		/// Process files for a specific service
		/// </summary>
		/********************************************************************/
		private void ProcessServiceFiles(TreeNode serviceNode, ModuleService service, bool showAll, Regex filterRegex)
		{
			foreach (var entry in service.OnlineFiles)
			{
				if (cancelled)
				{
					return;
				}

				// ModEntry only contains files, no directories!
				bool matchesFilter = showAll;

				if (!showAll)
					// Use regex for wildcard matching
				{
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
					// Always build tree structure (needed for flat view navigation too)
					AddFileToFilteredTree(serviceNode, service, entry);
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
