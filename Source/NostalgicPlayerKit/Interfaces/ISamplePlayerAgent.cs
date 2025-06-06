﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Agents of this type can play samples
	/// </summary>
	public interface ISamplePlayerAgent : IPlayerAgent, ISampleAgent
	{
		/// <summary>
		/// Return some flags telling what the player supports
		/// </summary>
		SamplePlayerSupportFlag SupportFlags { get; }

		/// <summary>
		/// Will load the header information from the file
		/// </summary>
		AgentResult LoadHeaderInfo(ModuleStream moduleStream, out string errorMessage);

		/// <summary>
		/// Initializes the player
		/// </summary>
		bool InitPlayer(ModuleStream moduleStream, out string errorMessage);

		/// <summary>
		/// Cleanup the player
		/// </summary>
		void CleanupPlayer();

		/// <summary>
		/// Initializes the player to start the sample from start
		/// </summary>
		bool InitSound(out string errorMessage);

		/// <summary>
		/// Cleanup the current song
		/// </summary>
		void CleanupSound();
	}
}
