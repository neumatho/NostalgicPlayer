/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Demuxer.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class AsfStream
	{
		// <summary>
		// 
		// </summary>
//		public c_int Num;

		/// <summary>
		/// 
		/// </summary>
		public c_uchar Seq;

		// Use for reading

		/// <summary>
		/// 
		/// </summary>
		public readonly AvPacket Pkt = new AvPacket();

		/// <summary>
		/// 
		/// </summary>
		public c_int Frag_Offset;

		/// <summary>
		/// 
		/// </summary>
		public c_int Packet_Obj_Size;

		// <summary>
		// 
		// </summary>
//		public c_int Timestamp;

		// <summary>
		// 
		// </summary>
//		public int64_t Duration;

		/// <summary>
		/// 
		/// </summary>
		public c_int Skip_To_Key;

		/// <summary>
		/// 
		/// </summary>
		public c_int Pkt_Clean;

		/// <summary>
		/// Descrambling
		/// </summary>
		public c_int Ds_Span;

		/// <summary>
		/// 
		/// </summary>
		public c_int Ds_Packet_Size;

		/// <summary>
		/// 
		/// </summary>
		public c_int Ds_Chunk_Size;

		/// <summary>
		/// 
		/// </summary>
		public int64_t Packet_Pos;

		/// <summary>
		/// 
		/// </summary>
		public uint16_t Stream_Language_Index;

		/// <summary>
		/// 
		/// </summary>
		public c_int Palette_Changed;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<uint32_t> Palette = new CPointer<uint32_t>(256);

		/// <summary>
		/// 
		/// </summary>
		public c_int Payload_Ext_Ct;

		/// <summary>
		/// 
		/// </summary>
		public readonly AsfPayload[] Payload = ArrayHelper.InitializeArray<AsfPayload>(8);
	}
}
