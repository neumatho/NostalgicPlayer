/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Containers;

namespace Polycode.NostalgicPlayer.NostalgicPlayerKit.Interfaces
{
	/// <summary>
	/// Agents of this type can play some kind of file
	/// </summary>
	public interface IPlayerAgent : IAgentWorker
	{
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		string[] FileExtensions { get; }

		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		AgentResult Identify(PlayerFileInfo fileInfo);

		/// <summary>
		/// Return the name of the module
		/// </summary>
		string ModuleName { get; }

		/// <summary>
		/// Return the name of the author
		/// </summary>
		string Author { get; }

		/// <summary>
		/// This is the main player method
		/// </summary>
		void Play();

		/// <summary>
		/// Event called when the player has reached the end
		/// </summary>
		event EventHandler EndReached;
	}
}
