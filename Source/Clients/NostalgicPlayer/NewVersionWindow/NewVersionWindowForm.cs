/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Krypton.Toolkit;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.HelpWindow
{
	/// <summary>
	/// This shows the help documentation
	/// </summary>
	public partial class NewVersionWindowForm : KryptonForm
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NewVersionWindowForm()
		{
			InitializeComponent();

			if (!DesignMode)
			{
				// Set the title of the window
				Text = Resources.IDS_NEWVERSION_TITLE;
				urlTextBox.Text = string.Format(Resources.IDS_NEWVERSION_URL, Env.CurrentVersion);
			}
		}
	}
}
