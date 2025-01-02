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
	internal static class LPC_Analysis_Filter
	{
		/********************************************************************/
		/// <summary>
		/// LPC analysis filter
		/// </summary>
		/********************************************************************/
		public static void Silk_LPC_Analysis_Filter(CPointer<opus_int16> _out, CPointer<opus_int16> _in, CPointer<opus_int16> B, opus_int32 len, opus_int32 d, c_int arch)
		{
			for (c_int ix = d; ix < len; ix++)
			{
				CPointer<opus_int16> in_ptr = _in + ix - 1;

				opus_int32 out32_Q12 = Macros.Silk_SMULBB(in_ptr[0], B[0]);

				// Allowing wrap around so that two wraps can cancel each other. The rare
				// cases where the result wraps around can only be triggered by invalid streams
				out32_Q12 = SigProc_Fix.Silk_SMLABB_ovflw((opus_uint32)out32_Q12, (opus_uint32)in_ptr[-1], (opus_uint32)B[1]);
				out32_Q12 = SigProc_Fix.Silk_SMLABB_ovflw((opus_uint32)out32_Q12, (opus_uint32)in_ptr[-2], (opus_uint32)B[2]);
				out32_Q12 = SigProc_Fix.Silk_SMLABB_ovflw((opus_uint32)out32_Q12, (opus_uint32)in_ptr[-3], (opus_uint32)B[3]);
				out32_Q12 = SigProc_Fix.Silk_SMLABB_ovflw((opus_uint32)out32_Q12, (opus_uint32)in_ptr[-4], (opus_uint32)B[4]);
				out32_Q12 = SigProc_Fix.Silk_SMLABB_ovflw((opus_uint32)out32_Q12, (opus_uint32)in_ptr[-5], (opus_uint32)B[5]);

				for (opus_int j = 6; j < d; j += 2)
				{
					out32_Q12 = SigProc_Fix.Silk_SMLABB_ovflw((opus_uint32)out32_Q12, (opus_uint32)in_ptr[-j], (opus_uint32)B[j]);
					out32_Q12 = SigProc_Fix.Silk_SMLABB_ovflw((opus_uint32)out32_Q12, (opus_uint32)in_ptr[-j - 1], (opus_uint32)B[j + 1]);
				}

				// Subtract prediction
				out32_Q12 = SigProc_Fix.Silk_SUB32_ovflw((opus_uint32)SigProc_Fix.Silk_LSHIFT(in_ptr[1], 12), (opus_uint32)out32_Q12);

				// Scale to Q0
				opus_int32 out32 = SigProc_Fix.Silk_RSHIFT_ROUND(out32_Q12, 12);

				// Saturate output
				_out[ix] = (opus_int16)SigProc_Fix.Silk_SAT16(out32);
			}

			// Set first d output samples to zero
			SigProc_Fix.Silk_MemSet<opus_int16>(_out, 0, d);
		}
	}
}
