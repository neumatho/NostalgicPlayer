/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// Mode definition (opaque)
	/// </summary>
	internal class OpusCustomMode
	{
		public opus_int32 Fs;
		public c_int overlap;

		public c_int nbEBands;
		public c_int effEBands;
		public opus_val16[] preemph = new opus_val16[4];

		/// <summary>
		/// Definition for each "pseudo-critical band"
		/// </summary>
		public Pointer<opus_int16> eBands;

		public c_int maxLM;
		public c_int nbShortMdcts;
		public c_int shortMdctSize;

		/// <summary>
		/// Number of lines in the matrix below
		/// </summary>
		public c_int nbAllocVectors;

		/// <summary>
		/// Number of bits in each band for several rates
		/// </summary>
		public Pointer<byte> allocVectors;
		public Pointer<opus_int16> logN;

		public Pointer<opus_val16> window;
		public Mdct_Lookup mdct = new Mdct_Lookup();
		public PulseCache cache = new PulseCache();
	}
}
