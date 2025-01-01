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
	/// Block-partitioned VQ coded straight residue
	/// </summary>
	internal class VorbisInfoResidue0 : IVorbisInfoResidue
	{
		public c_long begin;
		public c_long end;

		// First stage (lossless partitioning)
		public c_int grouping;									// Group n vectors per partition
		public c_int partitions;								// Possible codebooks for a partition
		public c_int partvals;									// Partitions ^ groupbook dim
		public c_int groupbook;									// Huffbook for partitioning
		public readonly c_int[] secondstages = new c_int[64];	// Expanded out to pointers in lookup
		public readonly c_int[] booklist = new c_int[512];		// List of second stage books

		public readonly c_int[] classmetric1 = new c_int[64];
		public readonly c_int[] classmetric2 = new c_int[64];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			begin = 0;
			end = 0;
			grouping = 0;
			partitions = 0;
			partvals = 0;
			groupbook = 0;

			Array.Clear(secondstages);
			Array.Clear(booklist);
			Array.Clear(classmetric1);
			Array.Clear(classmetric2);
		}
	}
}
