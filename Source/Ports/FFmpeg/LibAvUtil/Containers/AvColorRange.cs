/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Visual content value range.
	///
	/// These values are based on definitions that can be found in multiple
	/// specifications, such as ITU-T BT.709 (3.4 - Quantization of RGB, luminance
	/// and colour-difference signals), ITU-T BT.2020 (Table 5 - Digital
	/// Representation) as well as ITU-T BT.2100 (Table 9 - Digital 10- and 12-bit
	/// integer representation). At the time of writing, the BT.2100 one is
	/// recommended, as it also defines the full range representation.
	///
	/// Common definitions:
	///   - For RGB and luma planes such as Y in YCbCr and I in ICtCp,
	///     'E' is the original value in range of 0.0 to 1.0.
	///   - For chroma planes such as Cb,Cr and Ct,Cp, 'E' is the original
	///     value in range of -0.5 to 0.5.
	///   - 'n' is the output bit depth.
	///   - For additional definitions such as rounding and clipping to valid n
	///     bit unsigned integer range, please refer to BT.2100 (Table 9)
	/// </summary>
	public enum AvColorRange
	{
		/// <summary>
		/// 
		/// </summary>
		Unspecified = 0,

		/// <summary>
		/// Narrow or limited range content.
		///
		/// - For luma planes:
		///
		///       (219 * E + 16) * 2^(n-8)
		///
		///   F.ex. the range of 16-235 for 8 bits
		///
		/// - For chroma planes:
		///
		///       (224 * E + 128) * 2^(n-8)
		///
		///   F.ex. the range of 16-240 for 8 bits
		/// </summary>
		Mpeg = 1,

		/// <summary>
		/// Full range content.
		///
		/// - For RGB and luma planes:
		///
		///       (2^n - 1) * E
		///
		///   F.ex. the range of 0-255 for 8 bits
		///
		/// - For chroma planes:
		///       (2^n - 1) * E + 2^(n - 1)
		///
		///   F.ex. the range of 1-255 for 8 bits
		/// </summary>
		Jpeg = 2,

		/// <summary>
		/// Not part of ABI
		/// </summary>
		Nb
	}
}
