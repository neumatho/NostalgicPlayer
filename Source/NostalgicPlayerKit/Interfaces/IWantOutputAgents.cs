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
	/// Derive from this interface, if your agent want to know about loaded
	/// sample converters
	/// </summary>
	public interface IWantOutputAgents
	{
		/// <summary>
		/// Gives a list with all loaded output agents
		/// </summary>
		void SetOutputInfo(AgentInfo[] agents);
	}
}
