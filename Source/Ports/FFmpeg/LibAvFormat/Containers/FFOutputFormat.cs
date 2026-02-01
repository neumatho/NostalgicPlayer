/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class FFOutputFormat : AvOutputFormat
	{
		/// <summary>
		/// The public AVOutputFormat. See avformat.h for it
		/// </summary>
		public AvOutputFormat P => this;

		/// <summary>
		/// Size of private data so that it can be allocated in the wrapper
		/// </summary>
		public c_int Priv_Data_Size;

		/// <summary>
		/// Internal flags. See FF_OFMT_FLAG_* above and FF_FMT_FLAG_* in internal.h
		/// </summary>
		public FFOFmtFlag Flags_Internal;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.Write_Header_Delegate Write_Header;

		/// <summary>
		/// Write a packet. If FF_OFMT_FLAG_ALLOW_FLUSH is set in flags_internal,
		/// pkt can be NULL in order to flush data buffered in the muxer.
		/// When flushing, return 0 if there still is more data to flush,
		/// or 1 if everything was flushed and there is no more buffered
		/// data
		/// </summary>
		public FormatFunc.Write_Packet_Delegate Write_Packet;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.Write_Trailer_Delegate Write_Trailer;

		/// <summary>
		/// A format-specific function for interleavement.
		/// If unset, packets will be interleaved by dts
		/// </summary>
		public FormatFunc.Interleave_Packet_Delegate Interleave_Packet;

		/// <summary>
		/// Test if the given codec can be stored in this container
		/// </summary>
		public FormatFunc.Query_Codec_Delegate Query_Codec;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.Get_Output_Timestamp_Delegate Get_Output_Timestamp;

		/// <summary>
		/// Allows sending messages from application to device
		/// </summary>
		public FormatFunc.Control_Message_Delegate Control_Message;

		/// <summary>
		/// Write an uncoded AVFrame.
		///
		/// See av_write_uncoded_frame() for details.
		///
		/// The library will free *frame afterwards, but the muxer can prevent it
		/// by setting the pointer to NULL
		/// </summary>
		public FormatFunc.Write_Uncoded_Frame_Delegate Write_Uncoded_Frame;

		/// <summary>
		/// Returns device list with it properties.
		/// See avdevice_list_devices() for more details.
		/// </summary>
		public FormatFunc.Get_Device_List_Delegate Get_Device_List;

		/// <summary>
		/// Initialize format. May allocate data here, and set any AVFormatContext or
		/// AVStream parameters that need to be set before packets are sent.
		/// This method must not write output.
		///
		/// Any allocations made here must be freed in deinit()
		/// </summary>
		public FormatFunc.Init_Delegate Init;

		/// <summary>
		/// Deinitialize format. If present, this is called whenever the muxer is being
		/// destroyed, regardless of whether or not the header has been written.
		///
		/// If a trailer is being written, this is called after write_trailer().
		///
		/// This is called if init() fails as well
		/// </summary>
		public FormatFunc.Deinit_Delegate Deinit;

		/// <summary>
		/// Set up any necessary bitstream filtering and extract any extra data needed
		/// for the global header.
		///
		/// Note pkt might have been directly forwarded by a meta-muxer; therefore
		///      pkt->stream_index as well as the pkt's timebase might be invalid
		/// </summary>
		public FormatFunc.Check_Bitstream_Delegate Check_Bitstream;
	}
}
