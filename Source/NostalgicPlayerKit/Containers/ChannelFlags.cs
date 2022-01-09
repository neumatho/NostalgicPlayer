/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Channel flags
	/// </summary>
	[Flags]
	public enum ChannelFlags : uint
	{
		/// <summary>
		/// No flags
		/// </summary>
		None = 0,

		/// <summary>
		/// Mute the channel
		/// </summary>
		MuteIt = 0x00000001,

		/// <summary>
		/// Trig the sample (start over)
		/// </summary>
		TrigIt = 0x00000002,

		/// <summary>
		/// Set this if the sample is 16 bit
		/// </summary>
		_16Bit = 0x00000004,

		/// <summary>
		/// The sample loops
		/// </summary>
		Loop = 0x00000008,

		/// <summary>
		/// Set this together with the Loop flag for ping-pong loop
		/// </summary>
		PingPong = 0x00000010,

		/// <summary>
		/// Set this to trigger the sample when setting looping information
		/// </summary>
		TrigLoop = 0x00000020,

		/// <summary>
		/// Volume changed
		/// </summary>
		Volume = 0x00000100,

		/// <summary>
		/// Panning changed
		/// </summary>
		Panning = 0x00000200,

		/// <summary>
		/// New frequency
		/// </summary>
		Frequency = 0x0000400,

		/// <summary>
		/// Release the sample
		/// </summary>
		Release = 0x00000800,

		/// <summary>
		/// This is a read-only bit. When a sample is playing in the channel, it's set
		/// </summary>
		Active = 0x80000000
	}
}
