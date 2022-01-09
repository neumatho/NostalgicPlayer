/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Implement this interface in your worker to tell about which settings
	/// agent that can show the settings. It can be yourself or a separate library
	/// </summary>
	public interface IAgentSettingsRegistrar
	{
		/// <summary>
		/// Return the agent ID for the agent showing the settings
		/// </summary>
		Guid GetSettingsAgentId();
	}
}
