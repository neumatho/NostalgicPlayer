/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Containers.Flags
{
	/// <summary>
	/// Different flags indicating what a module player supports
	/// </summary>
	[Flags]
	public enum SampleSaverSupportFlag
	{
		/// <summary>
		/// Nothing
		/// </summary>
		None = 0,

		/// <summary>
		/// Converter can write in 8-bit
		/// </summary>
		Support8Bit = 0x0001,

		/// <summary>
		/// Converter can write in 16-bit
		/// </summary>
		Support16Bit = 0x0002,

		/// <summary>
		/// Converter can write in 32-bit
		/// </summary>
		Support32Bit = 0x0004,

		/// <summary>
		/// Converter can write in mono
		/// </summary>
		SupportMono = 0x0100,

		/// <summary>
		/// Converter can write in stereo
		/// </summary>
		SupportStereo = 0x0200
	}
}
