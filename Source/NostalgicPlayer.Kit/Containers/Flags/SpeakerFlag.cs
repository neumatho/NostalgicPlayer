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
		/// <summary></summary>
		FrontLeft = 0x00001,
		/// <summary></summary>
		FrontRight = 0x00002,
		/// <summary></summary>
		FrontCenter = 0x00004,
		/// <summary></summary>
		LowFrequency = 0x00008,
		/// <summary></summary>
		BackLeft = 0x00010,
		/// <summary></summary>
		BackRight = 0x00020,
		/// <summary></summary>
		FrontLeftOfCenter = 0x00040,
		/// <summary></summary>
		FrontRightOfCenter = 0x00080,
		/// <summary></summary>
		BackCenter = 0x00100,
		/// <summary></summary>
		SideLeft = 0x00200,
		/// <summary></summary>
		SideRight = 0x00400,
		/// <summary></summary>
		TopCenter = 0x00800,
		/// <summary></summary>
		TopFrontLeft = 0x01000,
		/// <summary></summary>
		TopFrontCenter = 0x02000,
		/// <summary></summary>
		TopFrontRight = 0x04000,
		/// <summary></summary>
		TopBackLeft = 0x08000,
		/// <summary></summary>
		TopBackCenter = 0x10000,
		/// <summary></summary>
		TopBackRight = 0x20000
	}
}
