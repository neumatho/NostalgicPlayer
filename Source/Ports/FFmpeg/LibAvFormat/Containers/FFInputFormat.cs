/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class FFInputFormat : AvInputFormat
	{
		/// <summary>
		/// The public AVInputFormat. See avformat.h for it
		/// </summary>
		public AvInputFormat P => this;

		/// <summary>
		/// Raw demuxers store their codec ID here
		/// </summary>
		public AvCodecId Raw_Codec_Id;

		/// <summary>
		/// Size of private data so that it can be allocated in the wrapper
		/// </summary>
//		public c_int Priv_Data_Size;
		public CodecFunc.Private_Data_Alloc_Delegate Priv_Data_Alloc;

		/// <summary>
		/// Internal flags. See FF_INFMT_FLAG_* above and FF_FMT_FLAG_* in internal.h
		/// </summary>
		public FFInFmtFlag Flags_Internal;

		/// <summary>
		/// Tell if a given file has a chance of being parsed as this format.
		/// The buffer provided is guaranteed to be AVPROBE_PADDING_SIZE bytes
		/// big so you do not have to check for that unless you need more
		/// </summary>
		public FormatFunc.Read_Probe_Delegate Read_Probe;

		/// <summary>
		/// Read the format header and initialize the AVFormatContext
		/// structure. Return 0 if OK. 'avformat_new_stream' should be
		/// called to create new streams
		/// </summary>
		public FormatFunc.Read_Header_Delegate Read_Header;

		/// <summary>
		/// Read one packet and put it in 'pkt'. pts and flags are also
		/// set. 'avformat_new_stream' can be called only if the flag
		/// AVFMTCTX_NOHEADER is used and only in the calling thread (not in a
		/// background thread)
		/// </summary>
		public FormatFunc.Read_Packet_Delegate Read_Packet;

		/// <summary>
		/// Close the stream. The AVFormatContext and AVStreams are not
		/// freed by this function
		/// </summary>
		public FormatFunc.Read_Close_Delegate Read_Close;

		/// <summary>
		/// Seek to a given timestamp relative to the frames in
		/// stream component stream_index
		/// </summary>
		public FormatFunc.Read_Seek_Delegate Read_Seek;

		/// <summary>
		/// Get the next timestamp in stream[stream_index].time_base units
		/// </summary>
		public FormatFunc.Read_Timestamp_Delegate Read_Timestamp;

		/// <summary>
		/// Start/resume playing - only meaningful if using a network-based format
		/// (RTSP)
		/// </summary>
		public FormatFunc.Read_Play_Delegate Read_Play;

		/// <summary>
		/// Pause playing - only meaningful if using a network-based format
		/// (RTSP)
		/// </summary>
		public FormatFunc.Read_Pause_Delegate Read_Pause;

		/// <summary>
		/// Seek to timestamp ts.
		/// Seeking will be done so that the point from which all active streams
		/// can be presented successfully will be closest to ts and within min/max_ts.
		/// Active streams are all streams that have AVStream.discard ‹ AVDISCARD_ALL
		/// </summary>
		public FormatFunc.Read_Seek2_Delegate Read_Seek2;

		/// <summary>
		/// Returns device list with it properties.
		/// See avdevice_list_devices() for more details
		/// </summary>
		public FormatFunc.Get_Device_List_Delegate Get_Device_List;
	}
}
