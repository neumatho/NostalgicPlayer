/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Do not derive from this interface, but use IAgentGuiDisplay instead
	/// in GuiKit if you want to show a window in your agent
	/// </summary>
	public interface IAgentDisplay
	{
		/// <summary>
		/// Return some flags telling how to set up the display window
		/// </summary>
		DisplayFlag Flags { get; }
	}
}
