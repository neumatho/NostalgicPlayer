/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Agents of this type outputs the sound to some device
	/// </summary>
	public interface IOutputAgent : IAgentWorker
	{
		/// <summary>
		/// Return some flags telling what the output agent supports
		/// </summary>
		OutputSupportFlag SupportFlags { get; }

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
		/// Tell the engine to pause playing
		/// </summary>
		void Pause();

		/// <summary>
		/// Will switch the stream to read the sound data from without
		/// interrupting the sound
		/// </summary>
		AgentResult SwitchStream(SoundStream soundStream, string fileName, string moduleName, string author);
	}
}
