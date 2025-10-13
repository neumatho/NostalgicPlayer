/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.ModLibraryWindow
{
	/// <summary>
	/// Represents a node in the module library tree structure
	/// </summary>
	internal class TreeNode
	{
		public string Name { get; set; }
		public string FullPath { get; set; }
		public bool IsDirectory { get; set; }
		public long Size { get; set; }
		public string ServiceId { get; set; }
		public List<TreeNode> Children { get; set; }

		public TreeNode()
		{
			Children = new List<TreeNode>();
		}



		/********************************************************************/
		/// <summary>
		/// Find a node by its full path by navigating through the tree
		/// </summary>
		/********************************************************************/
		public TreeNode FindByPath(string path)
		{
			// If this is the path we're looking for, return this node
			if (FullPath == path)
				return this;

			// If the path doesn't start with our path, it's not in our subtree
			// Special case: root has empty FullPath
			if (!string.IsNullOrEmpty(FullPath) && !path.StartsWith(FullPath))
				return null;

			// Find the child that matches the next part of the path
			foreach (var child in Children)
			{
				// Check if this child is on the path
				// For root node (FullPath=""), all paths are valid
				// For service nodes (ending with "://"), check if path starts with it
				// For other nodes, check if path matches or is a subpath
				if (path == child.FullPath ||
				    (child.FullPath.EndsWith("://") && path.StartsWith(child.FullPath)) ||
				    path.StartsWith(child.FullPath + "/"))
				{
					// Recursively search in this child
					return child.FindByPath(path);
				}
			}

			return null;
		}
	}
}
