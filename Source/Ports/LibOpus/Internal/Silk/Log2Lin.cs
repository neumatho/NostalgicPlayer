/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Log2Lin
	{
		/********************************************************************/
		/// <summary>
		/// Approximation of 2^() (very close inverse of silk_lin2log())
		/// Convert input to a linear scale
		/// </summary>
		/********************************************************************/
		public static opus_int32 Silk_Log2Lin(opus_int32 inLog_Q7)
		{
			if (inLog_Q7 < 0)
				return 0;
			else if (inLog_Q7 >= 3967)
				return opus_int32.MaxValue;

			opus_int32 _out = SigProc_Fix.Silk_LSHIFT(1, SigProc_Fix.Silk_RSHIFT(inLog_Q7, 7));
			opus_int32 frac_Q7 = inLog_Q7 & 0x7f;

			if (inLog_Q7 < 2048)
			{
				// Piece-wise parabolic approximation
				_out = SigProc_Fix.Silk_ADD_RSHIFT32(_out, SigProc_Fix.Silk_MUL(_out, Macros.Silk_SMLAWB(frac_Q7, Macros.Silk_SMULBB(frac_Q7, 128 - frac_Q7), -174)), 7);
			}
			else
			{
				// Piece-wise parabolic approximation
				_out = SigProc_Fix.Silk_MLA(_out, SigProc_Fix.Silk_RSHIFT(_out, 7), Macros.Silk_SMLAWB(frac_Q7, Macros.Silk_SMULBB(frac_Q7, 128 - frac_Q7), -174));
			}

			return _out;
		}
	}
}
