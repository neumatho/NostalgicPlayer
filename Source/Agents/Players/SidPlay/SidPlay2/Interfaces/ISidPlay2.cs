/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Interfaces
{
	/// <summary>
	/// Public SidPlay header
	/// </summary>
	internal interface ISidPlay2 : ISidUnknown
	{
		private static readonly IId iid = new IId(0x25ef79eb, 0x8de6, 0x4076, 0x9c6b, 0xa9f9, 0x570f3a4b);

		/// <summary>
		/// Return an unique ID for this implementation
		/// </summary>
		new static IId IId()
		{
			return iid;
		}

		/// <summary>
		/// Holds the configuration
		/// </summary>
		Sid2Config Configuration { get; set; }

		/// <summary>
		/// Return the emulator information
		/// </summary>
		Sid2Info GetInfo();

		/// <summary>
		/// Load the SID tune into the C64 environment
		/// </summary>
		void Load(SidTune.SidTune sidTune);

		/// <summary>
		/// Run the emulators until the given buffer is filled
		/// </summary>
		uint Play(sbyte[] leftBuffer, sbyte[] rightBuffer, uint length);

		/// <summary>
		/// Stop the emulation
		/// </summary>
		void Stop();
	}
}
