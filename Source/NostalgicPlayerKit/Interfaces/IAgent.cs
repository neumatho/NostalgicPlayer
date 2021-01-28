﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// All agents implements this interface
	/// </summary>
	public interface IAgent
	{
		/// <summary>
		/// Returns an unique ID for this agent
		/// </summary>
		Guid AgentId { get; }

		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Returns the version of this agent
		/// </summary>
		Version Version { get; }

		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		AgentSupportInfo[] AgentInformation { get; }

		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		IAgentWorker CreateInstance(Guid typeId);
	}
}
