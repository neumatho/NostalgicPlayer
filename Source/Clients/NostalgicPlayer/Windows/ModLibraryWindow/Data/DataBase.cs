/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Events;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Data
{
	/// <summary>
	/// Base class to data classes
	/// </summary>
	public abstract class DataBase
	{
		private FilteredTreeBuilder currentBuilder;

		// Current search results
		private TreeNode currentTree;

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
		/// Get the total number of files in the current search result
		/// </summary>
		/********************************************************************/
		public int TotalFileCount
		{
			get;
			private set;
		}



		/********************************************************************/
		/// <summary>
		/// Event fired when data loading is completed
		/// </summary>
		/********************************************************************/
		public event EventHandler DataLoaded;



		/********************************************************************/
		/// <summary>
		/// Build tree with optional filter (empty filter shows all files)
		/// </summary>
		/********************************************************************/
		public void BuildTree(string searchText, SearchMode searchMode)
		{
			SearchFilter = searchText.Trim();

			// Cancel previous search if running
			if (currentBuilder != null)
			{
				currentBuilder.Cancel();
				currentBuilder = null;
			}

			// Build tree (empty filter shows everything)
			currentBuilder = CreateTreeBuilder(SearchFilter, searchMode);
			currentBuilder.Completed += OnSearchCompleted;
			currentBuilder.Start();
		}



		/********************************************************************/
		/// <summary>
		/// Get entries for a specific path with optional flat view sort
		/// order
		/// </summary>
		/********************************************************************/
		public List<TreeNode> GetEntries(string currentPath, bool isFlatView, FlatViewSortOrder sortOrder)
		{
			TreeNode node = currentTree?.FindByPath(currentPath);
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
		/// Create new tree builder
		/// </summary>
		/********************************************************************/
		protected abstract FilteredTreeBuilder CreateTreeBuilder(string searchText, SearchMode searchMode);

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Called when search is completed
		/// </summary>
		/********************************************************************/
		private void OnSearchCompleted(object sender, TreeBuildCompletedEventArgs e)
		{
			// Store results
			currentTree = e.ResultTree;
			TotalFileCount = currentTree != null ? CountFilesRecursive(currentTree) : 0;

			// Clear builder reference
			currentBuilder = null;

			// Notify UI (already on UI thread via FilteredTreeBuilder)
			DataLoaded?.Invoke(this, EventArgs.Empty);
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



		/********************************************************************/
		/// <summary>
		/// Count all files recursively from a node
		/// </summary>
		/********************************************************************/
		private int CountFilesRecursive(TreeNode node)
		{
			int count = 0;

			foreach (TreeNode child in node.Children)
			{
				if (child.IsDirectory)
					count += CountFilesRecursive(child);
				else
					count++;
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
		#endregion
	}
}
