/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// This structure stores compressed data. It is typically exported by demuxers
	/// and then passed as input to decoders, or received as output from encoders and
	/// then passed to muxers.
	///
	/// For video, it should typically contain one compressed frame. For audio it may
	/// contain several compressed frames. Encoders are allowed to output empty
	/// packets, with no compressed data, containing only side data
	/// (e.g. to update some stream parameters at the end of encoding).
	///
	/// The semantics of data ownership depends on the buf field.
	/// If it is set, the packet data is dynamically allocated and is
	/// valid indefinitely until a call to av_packet_unref() reduces the
	/// reference count to 0.
	///
	/// If the buf field is not set av_packet_ref() would make a copy instead
	/// of increasing the reference count.
	///
	/// The side data is always allocated with av_malloc(), copied by
	/// av_packet_ref() and freed by av_packet_unref().
	///
	/// sizeof(AVPacket) being a part of the public ABI is deprecated. once
	/// av_init_packet() is removed, new packets will only be able to be allocated
	/// with av_packet_alloc(), and new fields may be added to the end of the struct
	/// with a minor bump
	/// </summary>
	public class AvPacket : IClearable, ICopyTo<AvPacket>
	{
		/********************************************************************/
		/// <summary>
		/// A reference to the reference-counted buffer where the packet data
		/// is stored.
		/// May be NULL, then the packet data is not reference-counted
		/// </summary>
		/********************************************************************/
		public AvBufferRef Buf;



		/********************************************************************/
		/// <summary>
		/// Presentation timestamp in AVStream->time_base units; the time at
		/// which the decompressed packet will be presented to the user.
		/// Can be AV_NOPTS_VALUE if it is not stored in the file.
		/// pts MUST be larger or equal to dts as presentation cannot happen
		/// before decompression, unless one wants to view hex dumps. Some
		/// formats misuse the terms dts and pts/cts to mean something
		/// different. Such timestamps must be converted to true pts/dts
		/// before they are stored in AVPacket
		/// </summary>
		/********************************************************************/
		public int64_t Pts;



		/********************************************************************/
		/// <summary>
		/// Decompression timestamp in AVStream->time_base units; the time
		/// at which the packet is decompressed.
		/// Can be AV_NOPTS_VALUE if it is not stored in the file
		/// </summary>
		/********************************************************************/
		public int64_t Dts;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public CPointer<uint8_t> Data;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Size;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Stream_Index;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public AvPktFlag Flags;



		/********************************************************************/
		/// <summary>
		/// Additional packet data that can be provided by the container.
		/// Packet can contain several types of side information
		/// </summary>
		/********************************************************************/
		public CPointer<AvPacketSideData> Side_Data;



		/********************************************************************/
		/// <summary>
		/// Number of elements in side_data
		/// </summary>
		/********************************************************************/
		public c_int Side_Data_Elems;



		/********************************************************************/
		/// <summary>
		/// Duration of this packet in AVStream->time_base units, 0 if
		/// unknown. Equals next_pts - this_pts in presentation order
		/// </summary>
		/********************************************************************/
		public int64_t Duration;



		/********************************************************************/
		/// <summary>
		/// Byte position in stream, -1 if unknown
		/// </summary>
		/********************************************************************/
		public int64_t Pos;



		/********************************************************************/
		/// <summary>
		/// For some private data of the user
		/// </summary>
		/********************************************************************/
		public IOpaque Opaque;



		/********************************************************************/
		/// <summary>
		/// AVBufferRef for free use by the API user. FFmpeg will never check
		/// the contents of the buffer ref. FFmpeg calls av_buffer_unref() on
		/// it when the packet is unreferenced. av_packet_copy_props() calls
		/// create a new reference with av_buffer_ref() for the target
		/// packet's opaque_ref field.
		///
		/// This is unrelated to the opaque field, although it serves a
		/// similar purpose
		/// </summary>
		/********************************************************************/
		public AvBufferRef Opaque_Ref;



		/********************************************************************/
		/// <summary>
		/// Time base of the packet's timestamps.
		/// In the future, this field may be set on packets output by
		/// encoders or demuxers, but its value will be by default ignored
		/// on input to decoders or muxers
		/// </summary>
		/********************************************************************/
		public AvRational Time_Base;



		/********************************************************************/
		/// <summary>
		/// Clear all fields
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Buf = null;
			Pts = 0;
			Dts = 0;
			Data.SetToNull();
			Size = 0;
			Stream_Index = 0;
			Flags = AvPktFlag.None;
			Side_Data.SetToNull();
			Side_Data_Elems = 0;
			Duration = 0;
			Pos = 0;
			Opaque = null;
			Opaque_Ref = null;
			Time_Base.Clear();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void CopyTo(AvPacket destination)
		{
			destination.Buf = Buf;
			destination.Pts = Pts;
			destination.Dts = Dts;
			destination.Data = Data;
			destination.Size = Size;
			destination.Stream_Index = Stream_Index;
			destination.Flags = Flags;
			destination.Side_Data = Side_Data;
			destination.Side_Data_Elems = Side_Data_Elems;
			destination.Duration = Duration;
			destination.Pos = Pos;
			destination.Opaque = Opaque;
			destination.Opaque_Ref = Opaque_Ref;
			destination.Time_Base = Time_Base;
		}
	}
}
