/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// Enumeration of the MPEG Audio flag bits
	/// </summary>
	[Flags]
	public enum Mpg123_Flags
	{
		/// <summary>
		/// The bitstream is error protected using 16-bit CRC
		/// </summary>
		Crc = 0x1,
		/// <summary>
		/// The bitstream is copyrighted
		/// </summary>
		Copyright = 0x2,
		/// <summary>
		/// The private bit has been set
		/// </summary>
		Private = 0x4,
		/// <summary>
		/// The bitstream is an original, not a copy
		/// </summary>
		Original = 0x8
	}
}
