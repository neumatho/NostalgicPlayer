/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Sources;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Data
{
	/// <summary>
	/// Builds filtered tree in background with cancellation support
	/// </summary>
	public class OnlineFilteredTreeBuilder : FilteredTreeBuilder
	{
		private readonly List<IModLibrarySource> sources;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public OnlineFilteredTreeBuilder(List<IModLibrarySource> sources, string searchText, SearchMode searchMode) : base(searchText, searchMode)
		{
			this.sources = sources;
		}

		#region FilteredTreeBuilder overrides
		/********************************************************************/
		/// <summary>
		/// Create tree from loaded data
		/// </summary>
		/********************************************************************/
		protected override void CreateTree(TreeNode root, bool showAll, Regex filterRegex)
		{
			foreach (IModLibrarySource source in sources)
			{
				if (cancelled)
					return;

				// Build display name with status info
				IReadOnlyList<ModEntry> files = source.Files;
				string displayName;

				if (source.IsLoaded)
				{
					int fileCount = files.Count;
					displayName = string.Format(Resources.IDS_MODLIBRARY_ROOT_DOWNLOADED, source.DisplayName, source.LastUpdate, fileCount);
				}
				else
					displayName = string.Format(Resources.IDS_MODLIBRARY_ROOT_NOTDOWNLOADED, source.DisplayName);

				TreeNode serviceNode = new TreeNode { Name = displayName, FullPath = source.RootPath, IsDirectory = true, SourceId = source.Id };

				// Always add service node (needed for flat view navigation too)
				root.Children.Add(serviceNode);

				// If source is loaded, add filtered files
				if (source.IsLoaded)
					ProcessSourceFiles(serviceNode, source, showAll, filterRegex);
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Process files for a specific source
		/// </summary>
		/********************************************************************/
		private void ProcessSourceFiles(TreeNode serviceNode, IModLibrarySource source, bool showAll, Regex filterRegex)
		{
			foreach (ModEntry entry in source.Files)
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
		/// Add file and parent directories to filtered tree using entry's
		/// PathParts
		/// </summary>
		/********************************************************************/
		private void AddFileToFilteredTree(TreeNode serviceNode, IModLibrarySource source, ModEntry entry)
		{
			TreeNode currentNode = serviceNode;
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
		#endregion
	}
}
