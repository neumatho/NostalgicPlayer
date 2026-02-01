/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// Verify checksums embedded in the bitstream (could be of either encoded or
	/// decoded data, depending on the format) and print an error message on mismatch.
	/// If AV_EF_EXPLODE is also set, a mismatching checksum will result in the
	/// decoder/demuxer returning an error
	/// </summary>
	[Flags]
	public enum AvEf
	{
		/// <summary>
		/// 
		/// </summary>
		CrcCheck = 1 << 0,

		/// <summary>
		/// Detect bitstream specification deviations
		/// </summary>
		Bitstream = 1 << 1,

		/// <summary>
		/// Detect improper bitstream length
		/// </summary>
		Buffer = 1 << 2,

		/// <summary>
		/// Abort decoding on minor error detection
		/// </summary>
		Explode = 1 << 3,

		/// <summary>
		/// Ignore errors and continue
		/// </summary>
		Ignore_Err = 1 << 15,

		/// <summary>
		/// Consider things that violate the spec, are fast to calculate and have not been seen in the wild as errors
		/// </summary>
		Careful = 1 << 16,

		/// <summary>
		/// Consider all spec non compliances as errors
		/// </summary>
		Compliant = 1 << 17,

		/// <summary>
		/// Consider things that a sane encoder/muxer should not do as an error
		/// </summary>
		Aggressive = 1 << 18
	}
}
