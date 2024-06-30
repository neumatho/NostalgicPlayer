/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Loader
{
	/// <summary>
	/// Sample flags
	/// </summary>
	[Flags]
	internal enum Sample_Flag
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Differential
		/// </summary>
		Diff = 0x0001,

		/// <summary>
		/// Unsigned
		/// </summary>
		Uns = 0x0002,

		/// <summary>
		/// 
		/// </summary>
		_8BDiff = 0x0004,

		/// <summary>
		/// 
		/// </summary>
		_7Bit = 0x0008,

		/// <summary>
		/// Get from buffer, don't load
		/// </summary>
		NoLoad = 0x0010,

		/// <summary>
		/// Big-endian
		/// </summary>
		BigEnd = 0x0040,

		/// <summary>
		/// Archimedes VIDC logarithmic
		/// </summary>
		Vidc = 0x0080,

		/// <summary>
		/// Interleaved stereo sample
		/// </summary>
		Interleaved = 0x0100,

		/// <summary>
		/// Play full sample before looping
		/// </summary>
		FullRep = 0x0200,

		/// <summary>
		/// Adlib synth instrument
		/// </summary>
		Adlib = 0x1000,

		/// <summary>
		/// HSC Adlib synth instrument
		/// </summary>
		Hsc = 0x2000,

		/// <summary>
		/// ADPCM4 encoded samples
		/// </summary>
		Adpcm = 0x4000
	}
}
