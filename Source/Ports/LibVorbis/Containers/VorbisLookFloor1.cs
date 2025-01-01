/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class VorbisLookFloor1 : IVorbisLookFloor
	{
		public readonly c_int[] sorted_index = new c_int[Constants.Vif_Posit + 2];
		public readonly c_int[] forward_index = new c_int[Constants.Vif_Posit + 2];
		public readonly c_int[] reverse_index = new c_int[Constants.Vif_Posit + 2];

		public readonly c_int[] hineighbor = new c_int[Constants.Vif_Posit];
		public readonly c_int[] loneighbor = new c_int[Constants.Vif_Posit];
		public c_int posts;

		public c_int n;
		public c_int quant_q;
		public VorbisInfoFloor1 vi;

		public c_long phrasebits;
		public c_long postbits;
		public c_long frames;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			posts = 0;
			n = 0;
			quant_q = 0;
			vi = null;
			phrasebits = 0;
			postbits = 0;
			frames = 0;

			Array.Clear(sorted_index);
			Array.Clear(forward_index);
			Array.Clear(reverse_index);
			Array.Clear(hineighbor);
			Array.Clear(loneighbor);
		}
	}
}
