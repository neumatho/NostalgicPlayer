/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class VorbisInfoFloor0 : IVorbisInfoFloor, IClearable
	{
		public c_int order;
		public c_long rate;
		public c_long barkmap;

		public c_int ampbits;
		public c_int ampdB;

		public c_int numbooks;		// <= 16
		public readonly c_int[] books = new c_int[16];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			order = 0;
			rate = 0;
			barkmap = 0;
			ampbits = 0;
			ampdB = 0;
			numbooks = 0;

			Array.Clear(books);
		}
	}
}
