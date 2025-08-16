/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// Decoder control
	/// </summary>
	internal class Silk_Decoder_Control
	{
		// Prediction and coding parameters
		public readonly opus_int[] pitchL = new opus_int[Constants.Max_Nb_Subfr];
		public readonly CPointer<opus_int32> Gains_Q16 = new CPointer<opus_int32>(Constants.Max_Nb_Subfr);

		// Holds interpolated and final coefficients
		public readonly opus_int16[][] PredCoef_Q12 = ArrayHelper.Initialize2Arrays<opus_int16>(2, Constants.Max_Lpc_Order);
		public readonly CPointer<opus_int16> LTPCoef_Q14 = new CPointer<opus_int16>(Constants.Ltp_Order * Constants.Max_Nb_Subfr);
		public opus_int LTP_scale_Q14;
	}
}
