/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// The output buffer, used to be pcm_sample, pcm_point and audiobufsize
	/// </summary>
	internal class OutBuffer
	{
		public c_uchar[] Data;			// Main data pointer, aligned
		public Memory<c_uchar> P;		// Read pointer
		public size_t Fill;				// Fill from read pointer
		public size_t Size;
		public c_uchar[] RData;			// Unaligned base pointer
	}
}
