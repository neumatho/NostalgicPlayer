/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Windows.Forms;
using Krypton.Toolkit;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SampleInfoWindow
{
	/// <summary>
	/// This class overrides some method in the DataGrid control
	/// to implement wanted features
	/// </summary>
	public class SampleInfoSamplesListControl : KryptonDataGridView
	{
		#region Keyboard shortcuts
		/********************************************************************/
		/// <summary>
		/// Is called when a key is pressed in the sample list. Handle the
		/// playing of samples.
		///
		/// It is done here, because we want to use the raw scan code from
		/// the keyboard, so it doesn't matter how your keyboard is mapped
		/// </summary>
		/********************************************************************/
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			int scanCode = ((int)msg.LParam >> 16) & 0xff;
			int extended = ((int)msg.LParam >> 24) & 0x1;

			if (extended == 0)
			{
				if (((SampleInfoWindowForm)FindForm()).ProcessKey(scanCode, keyData & Keys.KeyCode))
					return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}
		#endregion
	}
}
