/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.CKit;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Sum_Sqr_Shift
	{
		/********************************************************************/
		/// <summary>
		/// Compute number of bits to right shift the sum of squares of a
		/// vector of int16s to make it fit in an int32
		/// </summary>
		/********************************************************************/
		public static void Silk_Sum_Sqr_Shift(out opus_int32 energy, out opus_int shift, CPointer<opus_int16> x, opus_int len)
		{
			opus_int i;
			opus_uint32 nrg_tmp;

			// Do a first run with the maximum shift we could have
			opus_int shft = 31 - Macros.Silk_CLZ32(len);

			// Let's be conservative with rounding and start with nrg=len
			opus_int32 nrg = len;

			for (i = 0; i < (len - 1); i += 2)
			{
				nrg_tmp = (opus_uint32)Macros.Silk_SMULBB(x[i], x[i]);
				nrg_tmp = (opus_uint32)SigProc_Fix.Silk_SMLABB_ovflw(nrg_tmp, (opus_uint32)x[i + 1], (opus_uint32)x[i + 1]);
				nrg = (opus_int32)SigProc_Fix.Silk_ADD_RSHIFT_uint((opus_uint32)nrg, nrg_tmp, shft);
			}

			if (i < len)
			{
				// One sample left to process
				nrg_tmp = (opus_uint32)Macros.Silk_SMULBB(x[i], x[i]);
				nrg = (opus_int32)SigProc_Fix.Silk_ADD_RSHIFT_uint((opus_uint32)nrg, nrg_tmp, shft);
			}

			// Make sure the result will fit in a 32-bit signed integer with two bits
			// of headroom
			shft = SigProc_Fix.Silk_Max_32(0, shft + 3 - Macros.Silk_CLZ32(nrg));
			nrg = 0;

			for (i = 0; i < (len - 1); i += 2)
			{
				nrg_tmp = (opus_uint32)Macros.Silk_SMULBB(x[i], x[i]);
				nrg_tmp = (opus_uint32)SigProc_Fix.Silk_SMLABB_ovflw(nrg_tmp, (opus_uint32)x[i + 1], (opus_uint32)x[i + 1]);
				nrg = (opus_int32)SigProc_Fix.Silk_ADD_RSHIFT_uint((opus_uint32)nrg, nrg_tmp, shft);
			}

			if (i < len)
			{
				// One sample left to process
				nrg_tmp = (opus_uint32)Macros.Silk_SMULBB(x[i], x[i]);
				nrg = (opus_int32)SigProc_Fix.Silk_ADD_RSHIFT_uint((opus_uint32)nrg, nrg_tmp, shft);
			}

			// Output arguments
			shift = shft;
			energy = nrg;
		}
	}
}
