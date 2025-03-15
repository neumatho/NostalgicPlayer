/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Containers.Flags
{
	/// <summary>
	/// Supported speakers. Used in both player and output agents
	/// </summary>
	[Flags]
	public enum SpeakerFlag
	{
		FrontLeft = 0x00001,
		FrontRight = 0x00002,
		FrontCenter = 0x00004,
		LowFrequency = 0x00008,
		BackLeft = 0x00010,
		BackRight = 0x00020,
		FrontLeftOfCenter = 0x00040,
		FrontRightOfCenter = 0x00080,
		BackCenter = 0x00100,
		SideLeft = 0x00200,
		SideRight = 0x00400,
		TopCenter = 0x00800,
		TopFrontLeft = 0x01000,
		TopFrontCenter = 0x02000,
		TopFrontRight = 0x04000,
		TopBackLeft = 0x08000,
		TopBackCenter = 0x10000,
		TopBackRight = 0x20000
	}
}
