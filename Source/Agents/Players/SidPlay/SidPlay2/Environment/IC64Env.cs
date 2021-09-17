/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Event;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Environment
{
	/// <summary>
	/// Interface to C64Env class
	/// </summary>
	internal interface IC64Env
	{
		/// <summary>
		/// Return the event context
		/// </summary>
		IEventContext Context { get; }

		/// <summary></summary>
		void InterruptIrq(bool state);

		/// <summary></summary>
		void InterruptNmi();

		/// <summary></summary>
		void SignalAec(bool state);

		/// <summary></summary>
		byte ReadMemRamByte(ushort addr);

		/// <summary></summary>
		void Sid2Crc(byte data);

		/// <summary></summary>
		void Lightpen();
	}
}
