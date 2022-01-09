/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Krypton.Toolkit;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages
{
	/// <summary>
	/// This class overrides some method in the DataGrid control
	/// to implement wanted features
	/// </summary>
	public class DescriptionListControl : KryptonDataGridView
	{
		/********************************************************************/
		/// <summary>
		/// This is overridden and do not call the base method, which means
		/// nothing can be selected
		/// </summary>
		/********************************************************************/
		protected override void SetSelectedRowCore(int rowIndex, bool selected)
		{
		}
	}
}
