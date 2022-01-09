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

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow
{
	/// <summary>
	/// Customized implementation of the list box control
	/// </summary>
	public class ModuleListControl : KryptonListBox
	{
		private bool listNumberEnabled;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleListControl()
		{
			ListBox.MeasureItem += ListBox_MeasureItem;
		}



		/********************************************************************/
		/// <summary>
		/// Enable/disable list number viewing
		/// </summary>
		/********************************************************************/
		public void EnableListNumber(bool enable)
		{
			listNumberEnabled = enable;

			BeginUpdate();

			try
			{
				foreach (ModuleListItem listItem in Items)
					listItem.SetListControl(enable ? this : null);
			}
			finally
			{
				EndUpdate();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called for every new item added to the list
		/// </summary>
		/********************************************************************/
		private void ListBox_MeasureItem(object sender, MeasureItemEventArgs e)
		{
			((ModuleListItem)ListBox.Items[e.Index]).SetListControl(listNumberEnabled ? this : null);
		}
	}
}
