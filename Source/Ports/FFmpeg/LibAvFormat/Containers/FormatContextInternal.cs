/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class FormatContextInternal : FFFormatContext
	{
		public FFFormatContext Fc => this;

		public (
			c_int Initialized,										// Whether or not avformat_init_output has already been called
			c_int Streams_Initialized,								// Whether or not avformat_init_output fully initialized streams
			c_int Nb_Interleaved_Streams,							// Number of streams relevant for interleaving. Muxing only
			FormatFunc.InterleavePacket_Delegate Interleave_Packet	// The interleavement function in use. Always set.
		) Muxing = (
			Initialized: 0,
			Streams_Initialized: 0,
			Nb_Interleaved_Streams: 0,
			Interleave_Packet: null
		);

		public (
			PacketList Raw_Packet_Buffer,							// Raw packets from the demuxer, prior to parsing and decoding.
																	// This buffer is used for buffering packets until the codec can
																	// be identified, as parsing cannot be done without knowing the codec
			c_int Raw_Packet_Buffer_Size,							// Sum of the size of packets in raw_packet_buffer, in bytes
			PacketList Parse_Queue,									// Packets split by the parser get queued here
			c_int MetaFree,											// Contexts and child contexts do not contain a metadata option
			c_int Chapter_Ids_Monotonic								// Set if chapter ids are strictly monotonic
		) Demuxing = (
			Raw_Packet_Buffer: new PacketList(),
			Raw_Packet_Buffer_Size: 0,
			Parse_Queue: new PacketList(),
			MetaFree: 0,
			Chapter_Ids_Monotonic: 0
		);
	}
}
