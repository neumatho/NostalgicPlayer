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
	internal class VorbisInfoMapping0 : IVorbisInfoMapping
	{
		public c_int submaps;									// <= 16
		public readonly c_int[] chmuxlist = new c_int[256];		// Up to 256 channels in a Vorbis stream

		public readonly c_int[] floorsubmap = new c_int[16];	// [mux] submap to floors
		public readonly c_int[] residuesubmap = new c_int[16];	// [mux] submap to residue

		public c_int coupling_steps;
		public readonly c_int[] coupling_mag = new c_int[256];
		public readonly c_int[] coupling_ang = new c_int[256];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			submaps = 0;
			coupling_steps = 0;

			Array.Clear(chmuxlist);
			Array.Clear(floorsubmap);
			Array.Clear(residuesubmap);
			Array.Clear(coupling_mag);
			Array.Clear(coupling_ang);
		}
	}
}
