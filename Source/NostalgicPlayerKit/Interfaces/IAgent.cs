/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.NostalgicPlayerKit.Interfaces
{
	/// <summary>
	/// All agents implements this interface
	/// </summary>
	public interface IAgent
	{
		/// <summary>
		/// Returns an unique ID for this agent
		/// </summary>
		Guid Id { get; }

		/// <summary>
		/// Returns the version of this agent
		/// </summary>
		Version Version { get; }

		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		string Description { get; }

		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		IAgentWorker CreateInstance();
	}
}
