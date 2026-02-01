/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// Bytestream IO Context.
	/// New public fields can be added with minor version bumps.
	/// Removal, reordering and changes to existing public fields require
	/// a major version bump.
	/// sizeof(AVIOContext) must not be used outside libav*.
	///
	/// Note: None of the function pointers in AVIOContext should be called
	///       directly, they should only be set by the client application
	///       when implementing custom I/O. Normally these are set to the
	///       function pointers specified in avio_alloc_context()
	/// </summary>
	public class AvIoContext : AvClass, IOptionContext, IClearable
	{
		/// <summary>
		/// A class for private options.
		///
		/// If this AVIOContext is created by avio_open2(), av_class is set and
		/// passes the options down to protocols.
		///
		/// If this AVIOContext is manually allocated, then av_class may be set by
		/// the caller.
		///
		/// warning -- this field can be NULL, be sure to not pass this AVIOContext
		/// to any av_opt_* functions in that case
		/// </summary>
		public AvClass Av_Class => this;

		// The following shows the relationship between buffer, buf_ptr,
		// buf_ptr_max, buf_end, buf_size, and pos, when reading and when writing
		// (since AVIOContext is used for both):
		//
		// **********************************************************************************
		// *                                   READING
		// **********************************************************************************
		//
		//                             |              buffer_size              |
		//                             |---------------------------------------|
		//                             |                                       |
		// 
		//                          buffer          buf_ptr       buf_end
		//                             +---------------+-----------------------+
		//                             |/ / / / / / / /|/ / / / / / /|         |
		//   read buffer:              |/ / consumed / | to be read /|         |
		//                             |/ / / / / / / /|/ / / / / / /|         |
		//                             +---------------+-----------------------+
		// 
		//                                                          pos
		//               +-------------------------------------------+-----------------+
		//   input file: |                                           |                 |
		//               +-------------------------------------------+-----------------+
		// 
		// 
		// **********************************************************************************
		// *                                   WRITING
		// **********************************************************************************
		// 
		//                              |          buffer_size                 |
		//                              |--------------------------------------|
		//                              |                                      |
		// 
		//                                                 buf_ptr_max
		//                           buffer                 (buf_ptr)       buf_end
		//                              +-----------------------+--------------+
		//                              |/ / / / / / / / / / / /|              |
		//   write buffer:              | / / to be flushed / / |              |
		//                              |/ / / / / / / / / / / /|              |
		//                              +-----------------------+--------------+
		//                                buf_ptr can be in this
		//                                due to a backward seek
		// 
		//                             pos
		//                +-------------+----------------------------------------------+
		//   output file: |             |                                              |
		//                +-------------+----------------------------------------------+

		/// <summary>
		/// Start of the buffer
		/// </summary>
		public CPointer<c_uchar> Buffer;

		/// <summary>
		/// Maximum buffer size
		/// </summary>
		public c_int Buffer_Size;

		/// <summary>
		/// Current position in the buffer
		/// </summary>
		public CPointer<c_uchar> Buf_Ptr;

		/// <summary>
		/// End of the data, may be less than
		/// buffer+buffer_size if the read function returned
		/// less data than requested, e.g. for streams where
		/// no more data has been received yet
		/// </summary>
		public CPointer<c_uchar> Buf_End;

		/// <summary>
		/// A private pointer, passed to the read/write/seek/...
		/// functions
		/// </summary>
		public object Opaque;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.Read_Packet_Buffer_Delegate Read_Packet;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.Write_Packet_Buffer_Delegate Write_Packet;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.Seek_Delegate Seek;

		/// <summary>
		/// Position in the file of the current buffer
		/// </summary>
		public int64_t Pos;

		/// <summary>
		/// True if was unable to read due to error or eof
		/// </summary>
		public c_int Eof_Reached;

		/// <summary>
		/// Contains the error code or 0 if no error happened
		/// </summary>
		public c_int Error;

		/// <summary>
		/// True if open for writing
		/// </summary>
		public c_int Write_Flag;

		/// <summary>
		/// 
		/// </summary>
		public c_int Max_Packet_Size;

		/// <summary>
		/// Try to buffer at least this amount of data
		/// before flushing it
		/// </summary>
		public c_int Min_Packet_Size;

		/// <summary>
		/// 
		/// </summary>
		public c_ulong Checksum;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<c_uchar> Checksum_Ptr;

		/// <summary>
		/// 
		/// </summary>
		public FormatFunc.UpdateChecksum_Delegate Update_Checksum;

		/// <summary>
		/// Pause or resume playback for network streaming protocols - e.g. MMS
		/// </summary>
		public FormatFunc.ReadPause_Delegate Read_Pause;

		/// <summary>
		/// Seek to a given timestamp in stream with the specified stream_index.
		/// Needed for some network streaming protocols which don't support seeking
		/// to byte position
		/// </summary>
		public FormatFunc.ReadSeek_Delegate Read_Seek;

		/// <summary>
		/// A combination of AVIO_SEEKABLE_ flags or 0 when the stream is not seekable
		/// </summary>
		public AvIoSeekable Seekable;

		/// <summary>
		/// avio_read and avio_write should if possible be satisfied directly
		/// instead of going through a buffer, and avio_seek will always
		/// call the underlying seek function directly
		/// </summary>
		public c_int Direct;

		/// <summary>
		/// ',' separated list of allowed protocols
		/// </summary>
		public CPointer<char> Protocol_Whitelist;

		/// <summary>
		/// ',' separated list of disallowed protocols
		/// </summary>
		public CPointer<char> Protocol_Blacklist;

		/// <summary>
		/// A callback that is used instead of write_packet
		/// </summary>
		public FormatFunc.WriteDataType_Delegate Write_Data_Type;

		/// <summary>
		/// If set, don't call write_data_type separately for AVIO_DATA_MARKER_BOUNDARY_POINT,
		/// but ignore them and treat them as AVIO_DATA_MARKER_UNKNOWN (to avoid needlessly
		/// small chunks of data returned from the callback)
		/// </summary>
		public c_int Ignore_Boundary_Point;

		/// <summary>
		/// Maximum reached position before a backward seek in the write buffer,
		/// used keeping track of already written data for a later flush
		/// </summary>
		public CPointer<c_uchar> Buf_Ptr_Max;

		/// <summary>
		/// Read-only statistic of bytes read for this AVIOContext
		/// </summary>
		public int64_t Bytes_Read;

		/// <summary>
		/// Read-only statistic of bytes written for this AVIOContext
		/// </summary>
		public int64_t Bytes_Written;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override void Clear()
		{
			base.Clear();

			Buffer.SetToNull();
			Buffer_Size = 0;
			Buf_Ptr.SetToNull();
			Buf_End.SetToNull();
			Opaque = null;
			Read_Packet = null;
			Write_Packet = null;
			Seek = null;
			Pos = 0;
			Eof_Reached = 0;
			Error = 0;
			Write_Flag = 0;
			Max_Packet_Size = 0;
			Min_Packet_Size = 0;
			Checksum = 0;
			Checksum_Ptr.SetToNull();
			Update_Checksum = null;
			Read_Pause = null;
			Read_Seek = null;
			Seekable = 0;
			Direct = 0;
			Protocol_Whitelist.SetToNull();
			Protocol_Blacklist.SetToNull();
			Write_Data_Type = null;
			Ignore_Boundary_Point = 0;
			Buf_Ptr_Max.SetToNull();
			Bytes_Read = 0;
			Bytes_Written = 0;
		}
	}
}
