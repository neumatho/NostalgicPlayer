/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Resampler_Private_AR2
	{
		/********************************************************************/
		/// <summary>
		/// Second order AR filter with single delay elements
		/// </summary>
		/********************************************************************/
		public static void Silk_Resampler_Private_AR2(Pointer<opus_int32> S, Pointer<opus_int32> out_Q8, Pointer<opus_int16> _in, Pointer<opus_int16> A_Q14, opus_int32 len)
		{
			for (opus_int32 k = 0; k < len; k++)
			{
				opus_int32 out32 = SigProc_Fix.Silk_ADD_LSHIFT32(S[0], _in[k], 8);
				out_Q8[k] = out32;
				out32 = SigProc_Fix.Silk_LSHIFT(out32, 2);

				S[0] = Macros.Silk_SMLAWB(S[1], out32, A_Q14[0]);
				S[1] = Macros.Silk_SMULWB(out32, A_Q14[1]);
			}
		}
	}
}
