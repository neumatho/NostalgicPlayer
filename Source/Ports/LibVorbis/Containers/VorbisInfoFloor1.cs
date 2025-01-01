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
	internal class VorbisInfoFloor1 : IVorbisInfoFloor
	{
		public c_int partitions;													// 0 to 31
		public readonly c_int[] partitionclass = new c_int[Constants.Vif_Parts];	// 0 to 15

		public readonly c_int[] class_dim = new c_int[Constants.Vif_Class];			// 1 to 8
		public readonly c_int[] class_subs = new c_int[Constants.Vif_Class];		// 0,1,2,3 (bits: 1<<n poss)
		public readonly c_int[] class_book = new c_int[Constants.Vif_Class];		// subs ^ dim entries
		public readonly c_int[,] class_subbook = new c_int[Constants.Vif_Class, 8];	// [VifClass][subs]

		public c_int mult;															// 1 2 3 or 4
		public readonly c_int[] postlist = new c_int[Constants.Vif_Posit + 2];		// First two implicit

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			partitions = 0;
			mult = 0;

			Array.Clear(partitionclass);
			Array.Clear(class_dim);
			Array.Clear(class_subs);
			Array.Clear(class_book);
			Array.Clear(class_subbook);
			Array.Clear(postlist);
		}
	}
}
