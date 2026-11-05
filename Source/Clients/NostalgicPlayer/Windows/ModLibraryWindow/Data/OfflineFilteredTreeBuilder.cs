/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Data
{
	/// <summary>
	/// Builds filtered tree in background with cancellation support
	/// </summary>
	public class OfflineFilteredTreeBuilder : FilteredTreeBuilder
	{
		private readonly List<ModEntry> files;
		private readonly IModLibraryFavorites favorites;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public OfflineFilteredTreeBuilder(List<ModEntry> files, IModLibraryFavorites favorites, string searchText, SearchMode searchMode) : base(searchText, searchMode)
		{
			this.files = files;
			this.favorites = favorites;
		}

		#region FilteredTreeBuilder overrides
		/********************************************************************/
		/// <summary>
		/// Create tree from loaded data
		/// </summary>
		/********************************************************************/
		protected override void CreateTree(TreeNode root, bool showAll, Regex filterRegex)
		{
			foreach (ModEntry entry in files)
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
				AddFileToFilteredTree(root, entry);
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Add file and parent directories to filtered tree using entry's
		/// PathParts
		/// </summary>
		/********************************************************************/
		private void AddFileToFilteredTree(TreeNode rootNode, ModEntry entry)
		{
			TreeNode currentNode = rootNode;
			string currentPath = string.Empty;

			// Use the pre-parsed PathParts from ModEntry!
			foreach (string part in entry.PathParts)
			{
				if (cancelled)
					return;

				currentPath = string.IsNullOrEmpty(currentPath) ? part : currentPath + "/" + part;

				// Use cache for O(1) lookup instead of O(n) search
				if (!nodeCache.TryGetValue(currentPath, out TreeNode childNode))
				{
					childNode = new TreeNode
					{
						Name = part,
						FullPath = currentPath,
						IsDirectory = true,
						Size = 0,
						SourceId = string.Empty,
						IsFavorite = favorites.IsFavorite(currentPath)
					};

					currentNode.Children.Add(childNode);
					nodeCache[currentPath] = childNode;
				}

				currentNode = childNode;
			}

			// Add the file itself
			string fileName = entry.Name;
			string fileFullPath = entry.FullName;

			// Check cache first
			if (!nodeCache.ContainsKey(fileFullPath))
			{
				TreeNode fileNode = new TreeNode
				{
					Name = fileName,
					FullPath = fileFullPath,
					IsDirectory = false,
					Size = entry.Size,
					SourceId = string.Empty,
					IsFavorite = favorites.IsFavorite(fileFullPath)
				};

				currentNode.Children.Add(fileNode);
				nodeCache[fileFullPath] = fileNode;
			}
		}
		#endregion
	}
}
