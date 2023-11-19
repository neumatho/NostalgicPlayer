/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class BufferChain
	{
		public Buffy First;				// The beginning of the chain
		public Buffy Last;				// The end...    of the chain
		public ptrdiff_t Size;			// Aggregated size of all buffies

		// These positions are relative to buffer chain beginning
		public ptrdiff_t Pos;			// Position in whole chain
		public ptrdiff_t FirstPos;		// The point of return on non-forget()

		// The "real" filepos is fileoff + pos
		public int64_t FileOff;			// Beginning of chain is at this file offset
		// Unsigned since no direct arithmetic with offsets. Overflow of overall
		// size needs to be checked anyway
		public size_t BufBlock;			// Default (minimum) size of buffers
		public size_t Pool_Size;		// Keep that many buffers in storage
		public size_t Pool_Fill;		// That many buffers are there

		// A pool of buffers to re-use, if activated. It's a linked list that is worked on from the front
		public Buffy Pool;
	}
}
