/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/

using System;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.ModLibraryWindow
{
	/// <summary>
	/// Event args for tree build completion
	/// </summary>
	internal class TreeBuildCompletedEventArgs : EventArgs
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public TreeBuildCompletedEventArgs(TreeNode tree)
		{
			ResultTree = tree;
		}

		/********************************************************************/
		/// <summary>
		/// The resulting tree structure
		/// </summary>
		/********************************************************************/
		public TreeNode ResultTree
		{
			get;
		}
	}
}