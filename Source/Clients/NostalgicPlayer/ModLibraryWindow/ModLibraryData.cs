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
		private List<ModuleService> services = new List<ModuleService>();
		private FilteredTreeBuilder currentBuilder = null;

		// Current search results
		private TreeNode currentTree = null;
		private string searchFilter = "";

		// Offline mode support
		private bool isOfflineMode = false;

		public event EventHandler DataLoaded;



		/********************************************************************/
		/// <summary>
		/// Get or set offline mode
		/// </summary>
		/********************************************************************/
		public bool IsOfflineMode
		{
			get => isOfflineMode;
			set => isOfflineMode = value;
		}



		/********************************************************************/
		/// <summary>
		/// Get all available services
		/// </summary>
		/********************************************************************/
		public List<ModuleService> Services => services;



		/********************************************************************/
		/// <summary>
		/// Get current search filter
		/// </summary>
		/********************************************************************/
		public string SearchFilter => searchFilter;



		/********************************************************************/
		/// <summary>
		/// Add a service to the library
		/// </summary>
		/********************************************************************/
		public void AddService(ModuleService service)
		{
			services.Add(service);
		}



		/********************************************************************/
		/// <summary>
		/// Get service by ID
		/// </summary>
		/********************************************************************/
		public ModuleService GetService(string serviceId)
		{
			return services.FirstOrDefault(s => s.Id == serviceId);
		}



		/********************************************************************/
		/// <summary>
		/// Get service from full path
		/// </summary>
		/********************************************************************/
		public ModuleService GetServiceFromPath(string path)
		{
			foreach (var service in services)
			{
				if (path.StartsWith(service.RootPath))
					return service;
			}
			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Get relative path without service prefix
		/// </summary>
		/********************************************************************/
		public string GetRelativePathFromService(string fullPath, ModuleService service)
		{
			if (fullPath.StartsWith(service.RootPath))
				return fullPath.Substring(service.RootPath.Length);
			return "";
		}



		/********************************************************************/
		/// <summary>
		/// Build tree with optional filter (empty filter shows all files)
		/// </summary>
		/********************************************************************/
		public void BuildTree(string filter, SearchMode searchMode = SearchMode.FilenameAndPath, bool isFlatView = false)
		{
			searchFilter = filter.Trim();

			// Cancel previous search if running
			if (currentBuilder != null)
			{
				currentBuilder.Cancel();
				currentBuilder = null;
			}

			// Build tree (empty filter shows everything)
			currentBuilder = new FilteredTreeBuilder(services, searchFilter, isOfflineMode, searchMode, isFlatView);
			currentBuilder.Completed += OnSearchCompleted;
			currentBuilder.Start();
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

			// Notify UI
			DataLoaded?.Invoke(this, EventArgs.Empty);
		}



		/********************************************************************/
		/// <summary>
		/// Get entries for a specific path with optional flat view sort order
		/// </summary>
		/********************************************************************/
		public List<TreeNode> GetEntries(string currentPath, bool isFlatView, FlatViewSortOrder sortOrder)
		{
			TreeNode node = currentTree?.FindByPath(currentPath);
			if (node == null)
				return new List<TreeNode>();

			// Create a copy and sort
			var sortedChildren = new List<TreeNode>(node.Children);

			if (isFlatView)
			{
				// Flat view: Sort by name→path or path→name (all entries are files)
				sortedChildren.Sort((a, b) =>
				{
					if (sortOrder == FlatViewSortOrder.NameThenPath)
					{
						// Primary: Name, Secondary: Path
						int nameCompare = string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
						if (nameCompare != 0)
							return nameCompare;

						// If names are equal, sort by full path
						return string.Compare(a.FullPath, b.FullPath, StringComparison.OrdinalIgnoreCase);
					}
					else // PathThenName
					{
						// Primary: Path, Secondary: Name
						int pathCompare = string.Compare(a.FullPath, b.FullPath, StringComparison.OrdinalIgnoreCase);
						if (pathCompare != 0)
							return pathCompare;

						// If paths are equal, sort by name (shouldn't happen but just in case)
						return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
					}
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
		/// Get display path for breadcrumb
		/// </summary>
		/********************************************************************/
		public string GetDisplayPath(string path)
		{
			if (string.IsNullOrEmpty(path))
				return "Root";

			var service = GetServiceFromPath(path);
			if (service != null)
			{
				string relativePath = GetRelativePathFromService(path, service);
				if (string.IsNullOrEmpty(relativePath))
					return service.DisplayName;
				return $"{service.DisplayName}/{relativePath}";
			}

			return path;
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
		/// Count files recursively in tree
		/// </summary>
		/********************************************************************/
		private int CountFilesRecursive(TreeNode node)
		{
			int count = 0;

			foreach (var child in node.Children)
			{
				if (!child.IsDirectory)
					count++;
				else
					count += CountFilesRecursive(child);
			}

			return count;
		}
	}
}
