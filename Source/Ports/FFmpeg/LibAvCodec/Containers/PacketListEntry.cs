/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class PacketListEntry
	{
		/// <summary>
		/// 
		/// </summary>
		public PacketListEntry Next;

		/// <summary>
		/// 
		/// </summary>
		public readonly AvPacket Pkt = new AvPacket();
	}
}
