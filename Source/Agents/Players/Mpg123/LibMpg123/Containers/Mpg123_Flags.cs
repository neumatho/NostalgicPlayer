/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers
{
	/// <summary>
	/// Enumeration of the MPEG Audio flag bits
	/// </summary>
	[Flags]
	internal enum Mpg123_Flags
	{
		Crc = 0x1,						// < The bitstream is error protected using 16-bit CRC
		Copyright = 0x2,				// < The bitstream is copyrighted
		Private = 0x4,					// < The private bit has been set
		Original = 0x8					// < The bitstream is an original, not a copy
	}
}
