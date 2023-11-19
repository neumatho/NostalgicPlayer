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
	internal class Buffy
	{
		public c_uchar[] Data;
		public ptrdiff_t Size;
		public ptrdiff_t RealSize;
		public Buffy Next;
	}
}
