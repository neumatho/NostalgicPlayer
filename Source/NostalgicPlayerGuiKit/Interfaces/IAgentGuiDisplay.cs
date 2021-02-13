/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.GuiKit.Interfaces
{
	/// <summary>
	/// Derive from this interface, if you want to show a window in your agent
	/// </summary>
	public interface IAgentGuiDisplay : IAgentDisplay
	{
		/// <summary>
		/// Return the user control to show
		/// </summary>
		UserControl GetUserControl();
	}
}
