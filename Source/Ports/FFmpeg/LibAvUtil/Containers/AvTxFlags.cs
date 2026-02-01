/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Flags for av_tx_init()
	/// </summary>
	[Flags]
	public enum AvTxFlags : uint64_t
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Allows for in-place transformations, where input == output.
		/// May be unsupported or slower for some transform types
		/// </summary>
		Inplace = 1UL << 0,

		/// <summary>
		/// Relaxes alignment requirement for the in and out arrays of av_tx_fn().
		/// May be slower with certain transform types
		/// </summary>
		Unaligned = 1UL << 1,

		/// <summary>
		/// Performs a full inverse MDCT rather than leaving out samples that can be
		/// derived through symmetry. Requires an output array of 'len' floats,
		/// rather than the usual 'len/2' floats.
		/// Ignored for all transforms but inverse MDCTs
		/// </summary>
		Full_Imdct = 1UL << 2,

		/// <summary>
		/// Perform a real to half-complex RDFT.
		/// Only the real, or imaginary coefficients will
		/// be output, depending on the flag used. Only available for forward RDFTs.
		/// Output array must have enough space to hold N complex values
		/// (regular size for a real to complex transform)
		/// </summary>
		Real_To_Real = 1UL << 3,

		/// <summary>
		/// 
		/// </summary>
		Real_To_Imaginary = 1UL << 4,

		// Private flags

		/// <summary>
		/// Can be OR'd with AV_TX_INPLACE
		/// </summary>
		FF_Out_Of_Place = 1UL << 63,

		/// <summary>
		/// Cannot be OR'd with AV_TX_UNALIGNED
		/// </summary>
		FF_Aligned = 1UL << 62,

		/// <summary>
		/// Codelet expects permuted coeffs
		/// </summary>
		FF_Preshuffle = 1UL << 61,

		/// <summary>
		/// For non-orthogonal inverse-only transforms
		/// </summary>
		FF_Inverse_Only = 1UL << 60,

		/// <summary>
		/// For non-orthogonal forward-only transforms
		/// </summary>
		FF_Forward_Only = 1UL << 59,

		/// <summary>
		/// For asm->asm functions only
		/// </summary>
		FF_Asm_Call = 1UL << 58
	}
}
