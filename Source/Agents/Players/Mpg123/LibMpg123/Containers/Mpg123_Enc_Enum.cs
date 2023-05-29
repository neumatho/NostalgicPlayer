/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers
{
	/// <summary>
	/// An enum over all sample types possibly known to mpg123.
	/// The values are designed as bit flags to allow bitmasking for encoding
	/// families.
	/// This is also why the enum is not used as type for actual encoding variables,
	/// plain integers (at least 16 bit, 15 bit being used) cover the possible
	/// combinations of these flags.
	///
	/// Note that (your build of) libmpg123 does not necessarily support all these.
	/// Usually, you can expect the 8bit encodings and signed 16 bit.
	/// Also 32bit float will be usual beginning with mpg123-1.7.0.
	/// What you should bear in mind is that (SSE, etc) optimized routines may be
	/// absent for some formats. We do have SSE for 16, 32 bit and float, though.
	/// 24 bit integer is done via postprocessing of 32 bit output -- just cutting
	/// the last byte, no rounding, even. If you want better, do it yourself.
	///
	/// All formats are in native byte order. If you need different endinaness, you
	/// can simply postprocess the output buffers (libmpg123 wouldn't do anything
	/// else). The macro MPG123_SAMPLESIZE() can be helpful there
	/// </summary>
	[Flags]
	internal enum Mpg123_Enc_Enum
	{
		// 0000 0000 0000 1111 Some 8 bit  integer encoding
		Enc_8 = 0x00f,
		// 0000 0000 0100 0000 Some 16 bit integer encoding
		Enc_16 = 0x040,
		// 0100 0000 0000 0000 Some 24 bit integer encoding
		Enc_24 = 0x4000,
		// 0000 0001 0000 0000 Some 32 bit integer encoding
		Enc_32 = 0x100,
		// 0000 0000 1000 0000 Some signed integer encoding
		Enc_Signed = 0x080,
		// 0000 1110 0000 0000 Some float encoding
		Enc_Float = 0xe00,
		// 0000 0000 1101 0000 signed 16 bit
		Enc_Signed_16 = (Enc_16 | Enc_Signed | 0x10),
		// 0000 0000 0110 0000 unsigned 16 bit
		Enc_Unsigned_16 = (Enc_16 | 0x20),
		// 0000 0000 0000 0001 unsigned 8 bit
		Enc_Unsigned_8 = 0x01,
		// 0000 0000 1000 0010 signed 8 bit
		Enc_Signed_8 = (Enc_Signed | 0x02),
		// 0000 0000 0000 0100 ulaw 8 bit
		Enc_Ulaw_8 = 0x04,
		// 0000 0000 0000 1000 alaw 8 bit
		Enc_Alaw_8 = 0x08,
		// 0001 0001 1000 0000 signed 32 bit
		Enc_Signed_32 = Enc_32 | Enc_Signed | 0x1000,
		// 0010 0001 0000 0000 unsigned 32 bit
		Enc_Unsigned_32 = Enc_32 | 0x2000,
		// 0101 0000 1000 0000 signed 24 bit
		Enc_Signed_24 = Enc_24 | Enc_Signed | 0x1000,
		// 0110 0000 0000 0000 unsigned 24 bit
		Enc_Unsigned_24 = Enc_24 | 0x2000,
		// 0000 0010 0000 0000 32bit float
		Enc_Float_32 = 0x200,
		// 0000 0100 0000 0000 64bit float
		Enc_Float_64 = 0x400,
		// Any possibly known encoding from the list above
		Enc_Any = Enc_Signed_16 | Enc_Unsigned_16 |
					Enc_Unsigned_8 | Enc_Signed_8 |
					Enc_Ulaw_8 | Enc_Alaw_8 |
					Enc_Signed_32 | Enc_Unsigned_32 |
					Enc_Signed_24 | Enc_Unsigned_24 |
					Enc_Float_32 | Enc_Float_64
	}
}
