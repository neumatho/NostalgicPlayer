/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class Mdct_Lookup
	{
		public c_int n;
		public c_int maxshift;
		public Kiss_Fft_State[] kfft = new Kiss_Fft_State[4];
		public CPointer<kiss_twiddle_scalar> trig;
	}
}
