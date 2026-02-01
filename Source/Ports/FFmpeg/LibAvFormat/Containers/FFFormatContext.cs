/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class FFFormatContext : AvFormatContext
	{
		/// <summary>
		/// The public context
		/// </summary>
		public AvFormatContext Pub => this;

		// <summary>
		// Whether the timestamp shift offset has already been determined.
		// -1: disabled, 0: not yet determined, 1: determined
		// </summary>
//		public AvoidNegativeTs Avoid_Negative_Ts_Status;

		/// <summary>
		/// This buffer is only needed when packets were already buffered but
		/// not decoded, for example to get the codec parameters in MPEG
		/// streams
		/// </summary>
		public readonly PacketList Packet_Buffer = new PacketList();

		/// <summary>
		/// av_seek_frame() support
		/// </summary>
		public int64_t Data_Offset;

		/// <summary>
		/// The generic code uses this as a temporary packet
		/// to parse packets or for muxing, especially flushing.
		/// For demuxers, it may also be used for other means
		/// for short periods that are guaranteed not to overlap
		/// with calls to av_read_frame() (or ff_read_packet())
		/// or with each other.
		/// It may be used by demuxers as a replacement for
		/// stack packets (unless they call one of the aforementioned
		/// functions with their own AVFormatContext).
		/// Every user has to ensure that this packet is blank
		/// after using it
		/// </summary>
		public AvPacket Parse_Pkt;

		/// <summary>
		/// Used to hold temporary packets for the generic demuxing code.
		/// When muxing, it may be used by muxers to hold packets (even
		/// permanent ones)
		/// </summary>
		public AvPacket Pkt;

		// <summary>
		// 
		// </summary>
//		public c_int Avoid_Negative_Ts_Use_Pts;

		/// <summary>
		/// ID3v2 tag useful for MP3 demuxing
		/// </summary>
		public AvDictionary Id3v2_Meta;

		/// <summary>
		/// 
		/// </summary>
		public c_int Missing_Streams = 0;
	}
}
