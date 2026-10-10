/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Events;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Sources;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow
{
	/// <summary>
	/// Sort order for flat view
	/// </summary>
	internal enum FlatViewSortOrder
	{
		NameThenPath = 0,
		PathThenName = 1
	}

	/// <summary>
	/// Manages module library data, services, and search functionality
	/// </summary>
	internal class ModLibraryData
	{
		// Local files (service-independent)
		private readonly List<ModEntry> localFiles = new List<ModEntry>();
		private readonly Lock localFilesLock = new Lock();
		private FilteredTreeBuilder currentBuilder;

		// Current search results
		private TreeNode currentTree;

		/********************************************************************/
		/// <summary>
		/// Get or set local mode
		/// </summary>
		/********************************************************************/
		public bool IsOfflineMode
		{
			get;
			set;
		}



		/********************************************************************/
		/// <summary>
		/// Get a thread-safe snapshot of local files
		/// </summary>
		/********************************************************************/
		public List<ModEntry> GetLocalFilesSnapshot()
		{
			lock (localFilesLock)
			{
				return localFiles.ToList();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Get current search filter
		/// </summary>
		/********************************************************************/
		public string SearchFilter
		{
			get;
			private set;
		} = string.Empty;



		/********************************************************************/
		/// <summary>
		/// Get all available sources
		/// </summary>
		/********************************************************************/
		public List<IModLibrarySource> Sources
		{
			get;
		} = new();



		/********************************************************************/
		/// <summary>
		/// Event fired when data loading is completed
		/// </summary>
		/********************************************************************/
		public event EventHandler DataLoaded;



		/********************************************************************/
		/// <summary>
		/// Count files recursively in tree
		/// </summary>
		/********************************************************************/
		private int CountFilesRecursive(TreeNode node)
		{
			int count = 0;

			foreach (TreeNode child in node.Children)
			{
				if (!child.IsDirectory)
					count++;
				else
					count += CountFilesRecursive(child);
			}

			return count;
		}



		/********************************************************************/
		/// <summary>
		/// Check if node has any children (recursively)
		/// </summary>
		/********************************************************************/
		private bool HasChildren(TreeNode node)
		{
			if ((node.Children == null) || (node.Children.Count == 0))
				return false;

			// Check if any child is a file
			foreach (TreeNode child in node.Children)
			{
				if (!child.IsDirectory)
					return true;

				// Recursively check subdirectories
				if (HasChildren(child))
					return true;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Called when search is completed
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
		/// Add local file - creates ModEntry from already sorted data
		/// </summary>
		/********************************************************************/
		public void AddLocalFile(string nameWithPath, long size)
		{
			lock (localFilesLock)
			{
				localFiles.Add(new ModEntry(nameWithPath, size));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Add local file if it doesn't already exist (thread-safe)
		/// </summary>
		/********************************************************************/
		public void AddLocalFileIfNotExists(string nameWithPath, long size)
		{
			lock (localFilesLock)
			{
				if (!localFiles.Any(e => e.FullName == nameWithPath))
					localFiles.Add(new ModEntry(nameWithPath, size));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Add a source to the library
		/// </summary>
		/********************************************************************/
		public void AddSource(IModLibrarySource source)
		{
			Sources.Add(source);
		}



		/********************************************************************/
		/// <summary>
		/// Build tree with optional filter (empty filter shows all files)
		/// </summary>
		/********************************************************************/
		public void BuildTree(string filter, SearchMode searchMode = SearchMode.FilenameAndPath)
		{
			SearchFilter = filter.Trim();

			// Cancel previous search if running
			if (currentBuilder != null)
			{
				currentBuilder.Cancel();
				currentBuilder = null;
			}

			// Build tree (empty filter shows everything)
			currentBuilder = new FilteredTreeBuilder(Sources, GetLocalFilesSnapshot(), SearchFilter, IsOfflineMode, searchMode);
			currentBuilder.Completed += OnSearchCompleted;
			currentBuilder.Start();
		}



		/********************************************************************/
		/// <summary>
		/// Clear local files
		/// </summary>
		/********************************************************************/
		public void ClearLocalFiles()
		{
			lock (localFilesLock)
			{
				localFiles.Clear();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Count total files in current filtered tree
		/// </summary>
		/********************************************************************/
		public int CountTotalFilesInFilteredCache()
		{
			if (currentTree == null)
				return 0;

			return CountFilesRecursive(currentTree);
		}



		/********************************************************************/
		/// <summary>
		/// Get display path for breadcrumb
		/// </summary>
		/********************************************************************/
		public string GetDisplayPath(string path)
		{
			if (string.IsNullOrEmpty(path))
				return "Root";

			IModLibrarySource source = GetSourceFromPath(path);
			if (source != null)
			{
				string relativePath = GetRelativePathFromSource(path, source);
				if (string.IsNullOrEmpty(relativePath))
					return source.DisplayName;

				return $"{source.DisplayName}/{relativePath}";
			}

			return path;
		}



		/********************************************************************/
		/// <summary>
		/// Get entries for a specific path with optional flat view sort
		/// order
		/// </summary>
		/********************************************************************/
		public List<TreeNode> GetEntries(string currentPath, bool isFlatView, FlatViewSortOrder sortOrder)
		{
			var node = currentTree?.FindByPath(currentPath);
			if (node == null)
				return new List<TreeNode>();

			List<TreeNode> sortedChildren;

			if (isFlatView)
			{
				// Flat view: Show direct folders + all files recursively
				sortedChildren = new List<TreeNode>();

				// Add direct folders (not recursive)
				foreach (TreeNode child in node.Children)
				{
					if (child.IsDirectory)
						sortedChildren.Add(child);
				}

				// Add all files recursively
				CollectFilesRecursive(node, sortedChildren);
			}
			else
			{
				// Hierarchical view: just use direct children
				sortedChildren = new List<TreeNode>(node.Children);

				// If at root with search filter in hierarchical view, filter out empty services
				if (string.IsNullOrEmpty(currentPath) && !string.IsNullOrEmpty(SearchFilter))
					sortedChildren = sortedChildren.Where(child => HasChildren(child)).ToList();
			}

			if (isFlatView)
			{
				// Flat view: Directories first, then files (with custom sort order for files)
				sortedChildren.Sort((a, b) =>
				{
					// Directories always come before files
					if (a.IsDirectory && !b.IsDirectory)
						return -1;

					if (!a.IsDirectory && b.IsDirectory)
						return 1;

					// Both directories: sort alphabetically by name
					if (a.IsDirectory && b.IsDirectory)
						return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);

					// Both files: sort by selected order
					if (sortOrder == FlatViewSortOrder.NameThenPath)
					{
						// Primary: Name, Secondary: Path
						int nameCompare = string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
						if (nameCompare != 0)
							return nameCompare;

						// If names are equal, sort by full path
						return string.Compare(a.FullPath, b.FullPath, StringComparison.OrdinalIgnoreCase);
					}

					// PathThenName
					// Primary: Path, Secondary: Name
					int pathCompare = string.Compare(a.FullPath, b.FullPath, StringComparison.OrdinalIgnoreCase);
					if (pathCompare != 0)
						return pathCompare;

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
						return -1;

					if (!a.IsDirectory && b.IsDirectory)
						return 1;

					// Both are same type (both dirs or both files), sort alphabetically by name
					return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
				});
			}

			return sortedChildren;
		}



		/********************************************************************/
		/// <summary>
		/// Get relative path without source prefix
		/// </summary>
		/********************************************************************/
		public string GetRelativePathFromSource(string fullPath, IModLibrarySource source)
		{
			if (fullPath.StartsWith(source.RootPath))
				return fullPath.Substring(source.RootPath.Length);

			return string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Get source by ID
		/// </summary>
		/********************************************************************/
		public IModLibrarySource GetSource(string sourceId)
		{
			return Sources.FirstOrDefault(s => s.Id == sourceId);
		}



		/********************************************************************/
		/// <summary>
		/// Get source from full path
		/// </summary>
		/********************************************************************/
		public IModLibrarySource GetSourceFromPath(string path)
		{
			foreach (IModLibrarySource source in Sources)
			{
				if (path.StartsWith(source.RootPath))
					return source;
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Collect all files recursively from a node
		/// </summary>
		/********************************************************************/
		private void CollectFilesRecursive(TreeNode node, List<TreeNode> files)
		{
			foreach (TreeNode child in node.Children)
			{
				if (!child.IsDirectory)
					files.Add(child);
				else
					CollectFilesRecursive(child, files);
			}
		}
	}
}
