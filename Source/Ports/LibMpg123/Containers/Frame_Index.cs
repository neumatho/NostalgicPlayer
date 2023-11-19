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
	internal class Frame_Index
	{
		public int64_t[] Data;			// Actual data, the frame positions
		public int64_t Step;			// Advancement in frame number per index point
		public int64_t Next;			// Frame offset supposed to come next into the index
		public size_t Size;				// Total number of possible entries
		public size_t Fill;				// Number of used entries
		public size_t Grow_Size;		// if > 0: index allowed to grow on need with these steps, instead of lowering resolution
	}
}
