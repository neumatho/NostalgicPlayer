/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class Kiss_Fft_State
	{
		public c_int nfft;
		public opus_val16 scale;
		public c_int shift;
		public opus_int16[] factors = new opus_int16[2 * Constants.MaxFactors];
		public Pointer<opus_int16> bitrev;
		public Pointer<Kiss_Twiddle_Cpx> twiddles;
		public Pointer<Arch_Fft_State> arch_fft;
	}
}
