/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow.ListItem
{
	/// <summary>
	/// All module items need to implement this interface
	/// </summary>
	public interface IModuleListItem
	{
		/// <summary>
		/// Return the name which is shown in the list
		/// </summary>
		string DisplayName { get; }

		/// <summary>
		/// Return the full path to the file
		/// </summary>
		string FullPath { get; }
	}
}
