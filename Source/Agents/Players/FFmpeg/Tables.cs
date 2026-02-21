/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers.Flags;

namespace Polycode.NostalgicPlayer.Agent.Player.FFmpeg
{
	/// <summary>
	/// Different tables needed
	/// </summary>
	internal static class Tables
	{
		/********************************************************************/
		/// <summary>
		/// Channel to speaker mapping
		/// </summary>
		/********************************************************************/
		public static readonly SpeakerFlag[] ChannelToSpeaker =
		[
			SpeakerFlag.FrontCenter,
			SpeakerFlag.FrontLeft | SpeakerFlag.FrontRight,
			SpeakerFlag.FrontLeft | SpeakerFlag.FrontCenter | SpeakerFlag.FrontRight,
			SpeakerFlag.FrontLeft | SpeakerFlag.FrontRight | SpeakerFlag.BackLeft | SpeakerFlag.BackRight,
			SpeakerFlag.FrontLeft | SpeakerFlag.FrontCenter | SpeakerFlag.FrontRight | SpeakerFlag.BackLeft | SpeakerFlag.BackRight,
			SpeakerFlag.FrontLeft | SpeakerFlag.FrontCenter | SpeakerFlag.FrontRight | SpeakerFlag.BackLeft | SpeakerFlag.BackRight | SpeakerFlag.LowFrequency,
			SpeakerFlag.FrontLeft | SpeakerFlag.FrontCenter | SpeakerFlag.FrontRight | SpeakerFlag.SideLeft | SpeakerFlag.SideRight | SpeakerFlag.BackCenter | SpeakerFlag.LowFrequency,
			SpeakerFlag.FrontLeft | SpeakerFlag.FrontCenter | SpeakerFlag.FrontRight | SpeakerFlag.SideLeft | SpeakerFlag.SideRight | SpeakerFlag.BackLeft | SpeakerFlag.BackRight | SpeakerFlag.LowFrequency
		];
	}
}
