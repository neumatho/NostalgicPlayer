/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Demuxer.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class AsfContext : AvClass, IPrivateData
	{
		/// <summary>
		/// A class for logging and AVOptions
		/// </summary>
		public AvClass Class => this;

		/// <summary>
		/// Conversion table from asf ID 2 AVStream ID
		/// </summary>
		public readonly c_int[] AsfId2AvId = new c_int[128];

		/// <summary>
		/// It's max number and it's not that big
		/// </summary>
		public readonly AsfStream[] Streams = ArrayHelper.InitializeArray<AsfStream>(128);

		/// <summary>
		/// Max number of streams, bitrate for each (for streaming)
		/// </summary>
		public readonly uint32_t[] Stream_BitRates = new uint32_t[128];

		/// <summary>
		/// 
		/// </summary>
		public readonly AvRational[] Dar = new AvRational[128];

		/// <summary>
		/// Max number of streams, language for each (RFC1766, e.g. en-US)
		/// </summary>
		public readonly char[][] Stream_Languages = ArrayHelper.Initialize2Arrays<char>(128, 6);

		// Non streamed additonnal info
		// Packet filling

		/// <summary>
		/// 
		/// </summary>
		public c_int Packet_Size_Left;

		// Only for reading

		/// <summary>
		/// Beginning of the first data packet
		/// </summary>
		public uint64_t Data_Offset;

		/// <summary>
		/// Data object offset (excl. GUID and size)
		/// </summary>
		public uint64_t Data_Object_Offset;

		/// <summary>
		/// Size of the data object
		/// </summary>
		public uint64_t Data_Object_Size;

		// <summary>
		// 
		// </summary>
		public c_int Index_Read;

		/// <summary>
		/// 
		/// </summary>
		public readonly AsfMainHeader Hdr = new AsfMainHeader();

		/// <summary>
		/// 
		/// </summary>
		public c_int Packet_Flags;

		/// <summary>
		/// 
		/// </summary>
		public c_int Packet_Property;

		/// <summary>
		/// 
		/// </summary>
		public c_int Packet_Timestamp;

		/// <summary>
		/// 
		/// </summary>
		public c_int Packet_SegSizeType;

		/// <summary>
		/// 
		/// </summary>
		public c_int Packet_Segments;

		/// <summary>
		/// 
		/// </summary>
		public c_int Packet_Seq;

		/// <summary>
		/// 
		/// </summary>
		public c_int Packet_Replic_Size;

		/// <summary>
		/// 
		/// </summary>
		public c_int Packet_Key_Frame;

		/// <summary>
		/// 
		/// </summary>
		public c_int Packet_PadSize;

		/// <summary>
		/// 
		/// </summary>
		public c_uint Packet_Frag_Offset;

		/// <summary>
		/// 
		/// </summary>
		public c_uint Packet_Frag_Size;

		/// <summary>
		/// 
		/// </summary>
		public int64_t Packet_Frag_Timestamp;

		/// <summary>
		/// 
		/// </summary>
		public c_int Ts_Is_Pts;

		/// <summary>
		/// 
		/// </summary>
		public c_int Packet_Multi_Size;

		/// <summary>
		/// 
		/// </summary>
		public c_int Packet_Time_Delta;

		/// <summary>
		/// 
		/// </summary>
		public int64_t Packet_Time_Start;

		/// <summary>
		/// 
		/// </summary>
		public int64_t Packet_Pos;

		/// <summary>
		/// 
		/// </summary>
		public c_int Stream_Index;

		/// <summary>
		/// Currently decoded stream
		/// </summary>
		public AsfStream Asf_St;

		/// <summary>
		/// 
		/// </summary>
		public c_int No_Resync_Search = 0;

		/// <summary>
		/// 
		/// </summary>
		public c_int Export_Xmp = 0;

		/// <summary>
		/// 
		/// </summary>
		public c_int Uses_Std_Ecc;
	}
}
