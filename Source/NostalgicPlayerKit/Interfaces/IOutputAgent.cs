/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Containers;
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Streams;

namespace Polycode.NostalgicPlayer.NostalgicPlayerKit.Interfaces
{
	/// <summary>
	/// Agents of this type outputs the sound to some device
	/// </summary>
	public interface IOutputAgent : IAgentWorker
	{
		/// <summary>
		/// Will initialize the output driver
		/// </summary>
		AgentResult Initialize(out string errorMessage);

		/// <summary>
		/// Will shutdown the output driver
		/// </summary>
		void Shutdown();

		/// <summary>
		/// Tell the engine to begin playing
		/// </summary>
		void Play();

		/// <summary>
		/// Tell the engine to stop playing
		/// </summary>
		void Stop();

		/// <summary>
		/// Will switch the stream to read the sound data from without
		/// interrupting the sound
		/// </summary>
		void SwitchStream(SoundStream soundStream);
	}
}
