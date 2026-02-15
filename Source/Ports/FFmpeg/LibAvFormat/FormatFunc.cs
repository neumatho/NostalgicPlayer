/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvDevice.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Interfaces;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat
{
	/// <summary>
	/// All function delegates
	/// </summary>
	public static class FormatFunc
	{
		/// <summary></summary>
		public delegate c_int Read_Packet_Buffer_Delegate(object opaque, CPointer<c_uchar> buf, c_int buf_Size);
		/// <summary></summary>
		public delegate c_int Write_Packet_Buffer_Delegate(object opaque, CPointer<c_uchar> buf, c_int buf_Size);
		/// <summary></summary>
		public delegate int64_t Seek_Delegate(object opaque, int64_t offset, AvSeek whence);

		/// <summary></summary>
		public delegate c_ulong UpdateChecksum_Delegate(c_ulong checksum, CPointer<uint8_t> buf, c_uint size);
		/// <summary></summary>
		public delegate c_int ReadPause_Delegate(IContext opaque, c_int pause);
		/// <summary></summary>
		public delegate c_int ReadSeek_Delegate(object opaque, c_int stream_Index, int64_t timestamp, AvSeekFlag flags);
		/// <summary></summary>
		public delegate c_int WriteDataType_Delegate(object opaque, CPointer<uint8_t> buf, c_int buf_size, AvIoDataMarkerType type, int64_t time);

		/// <summary></summary>
		public delegate c_int Open_Delegate(AvFormatContext s, out AvIoContext pb, CPointer<char> url, AvIoFlag flags, ref AvDictionary options);
		/// <summary></summary>
		public delegate c_int Close2_Delegate(AvFormatContext s, AvIoContext pb);

		/// <summary></summary>
		public delegate c_int Av_Format_Control_Message(AvFormatContext s, c_int type, IOpaque data, size_t data_Size);

		/// <summary></summary>
		public delegate c_int Callback_Delegate(IOpaque opaque);

		/// <summary></summary>
		public delegate c_int Read_Probe_Delegate(AvProbeData pd);
		/// <summary></summary>
		public delegate c_int Read_Header_Delegate(AvFormatContext s);
		/// <summary></summary>
		public delegate c_int Read_Packet_Delegate(AvFormatContext s, AvPacket pkt);
		/// <summary></summary>
		public delegate c_int Read_Close_Delegate(AvFormatContext s);
		/// <summary></summary>
		public delegate c_int Read_Seek_Delegate(AvFormatContext s, c_int stream_Index, int64_t timestamp, AvSeekFlag flags);
		/// <summary></summary>
		public delegate int64_t Read_Timestamp_Delegate(AvFormatContext s, c_int stream_Index, ref int64_t pos, int64_t pos_Limit);
		/// <summary></summary>
		public delegate c_int Read_Play_Delegate(AvFormatContext s);
		/// <summary></summary>
		public delegate c_int Read_Pause_Delegate(AvFormatContext s);
		/// <summary></summary>
		public delegate c_int Read_Seek2_Delegate(AvFormatContext s, c_int stream_Index, int64_t min_Ts, int64_t ts, int64_t max_Ts, AvSeekFlag flags);
		/// <summary></summary>
		public delegate c_int Get_Device_List_Delegate(AvFormatContext s, out AvDeviceInfoList device_List);

		/// <summary></summary>
		public delegate c_int Write_Header_Delegate(AvFormatContext s);
		/// <summary></summary>
		public delegate c_int Write_Packet_Delegate(AvFormatContext s, AvPacket pkt);
		/// <summary></summary>
		public delegate c_int Write_Trailer_Delegate(AvFormatContext s);
		/// <summary></summary>
		public delegate c_int Interleave_Packet_Delegate(AvFormatContext s, AvPacket pkt, c_int flush, c_int has_Packet);
		/// <summary></summary>
		public delegate c_int Query_Codec_Delegate(AvCodecId id, c_int std_Compliance);
		/// <summary></summary>
		public delegate void Get_Output_Timestamp_Delegate(AvFormatContext s, c_int stream, ref int64_t dts, ref int64_t wall);
		/// <summary></summary>
		public delegate c_int Control_Message_Delegate(AvFormatContext s, c_int type, IOpaque data, size_t data_Size);
		/// <summary></summary>
		public delegate c_int Write_Uncoded_Frame_Delegate(AvFormatContext s, c_int stream_Index, CPointer<AvFrame> frame, c_uint flags);
		/// <summary></summary>
		public delegate c_int Init_Delegate(AvFormatContext s);
		/// <summary></summary>
		public delegate c_int Deinit_Delegate(AvFormatContext s);
		/// <summary></summary>
		public delegate c_int Check_Bitstream_Delegate(AvFormatContext s, AvStream st, AvPacket pkt);

		/// <summary></summary>
		public delegate c_int Short_Seek_Get_Delegate(object opaque);

		/// <summary></summary>
		public delegate c_int InterleavePacket_Delegate(AvFormatContext s, AvPacket pkt, c_int flush, c_int has_Packet);

		/// <summary></summary>
		public delegate c_int Url_Open_Delegate(UrlContext h, CPointer<char> url, AvIoFlag flags);
		/// <summary></summary>
		public delegate c_int Url_Open2_Delegate(UrlContext h, CPointer<char> url, AvIoFlag flags, ref AvDictionary options);
		/// <summary></summary>
		public delegate c_int Url_Accept_Delegate(UrlContext h, out UrlContext c);
		/// <summary></summary>
		public delegate c_int Url_Handshake_Delegate(UrlContext c);
		/// <summary></summary>
		public delegate c_int Url_Read_Delegate(UrlContext h, CPointer<c_uchar> buf, c_int size);
		/// <summary></summary>
		public delegate c_int Url_Write_Delegate(UrlContext h, CPointer<c_uchar> buf, c_int size);
		/// <summary></summary>
		public delegate c_int Url_Seek_Delegate(UrlContext h, int64_t pos, AvSeek whence);
		/// <summary></summary>
		public delegate c_int Url_Close_Delegate(UrlContext h);
		/// <summary></summary>
		public delegate c_int Url_Get_File_Handle_Delegate(UrlContext h);
		/// <summary></summary>
		public delegate c_int Url_Get_Multi_File_Handle_Delegate(UrlContext h, out CPointer<c_int> handles, out c_int numHandles);
		/// <summary></summary>
		public delegate c_int Url_Get_Short_Seek_Delegate(UrlContext h);
		/// <summary></summary>
		public delegate c_int Url_Shutdown_Delegate(UrlContext h, int flags);
		/// <summary></summary>
		public delegate c_int Url_Check_Delegate(UrlContext h, int mask);
		/// <summary></summary>
		public delegate c_int Url_Open_Dir_Delegate(UrlContext h);
		/// <summary></summary>
		public delegate c_int Url_Read_Dir_Delegate(UrlContext h, ref AvIoDirEntry next);
		/// <summary></summary>
		public delegate c_int Url_Close_Dir_Delegate(UrlContext h);
		/// <summary></summary>
		public delegate c_int Url_Delete_Delegate(UrlContext h);
		/// <summary></summary>
		public delegate c_int Url_Move_Delegate(UrlContext h_Src, UrlContext h_Dst);

		/// <summary></summary>
		internal delegate void Id3v2_Read_Delegate(AvFormatContext s, AvIoContext pb, c_int tagLen, CPointer<char> tag, ref ExtraMetaList extra_Meta, bool isV34);
		/// <summary></summary>
		internal delegate void Id3v2_Free_Delegate(IExtraMetadata obj);
		/// <summary></summary>
		internal delegate c_uint Id3v2_Get_Delegate(AvIoContext pb);
	}
}
