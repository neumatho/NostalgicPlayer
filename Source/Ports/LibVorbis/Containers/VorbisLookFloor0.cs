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
	internal class VorbisLookFloor0 : IVorbisLookFloor
	{
		public c_int ln;
		public c_int m;
		public c_int[][] linearmap;
		public readonly c_int[] n = new c_int[2];

		public VorbisInfoFloor0 vi;

		public c_long bits;
		public c_long frames;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			ln = 0;
			m = 0;
			linearmap = null;
			vi = null;
			bits = 0;
			frames = 0;

			Array.Clear(n);
		}
	}
}
