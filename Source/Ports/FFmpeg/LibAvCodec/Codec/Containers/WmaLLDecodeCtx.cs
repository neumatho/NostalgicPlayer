/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Codec.Containers
{
	/// <summary>
	/// Main decoder context
	/// </summary>
	internal class WmaLLDecodeCtx : IPrivateData
	{
		// Generic decoder variables

		/// <summary>
		/// 
		/// </summary>
		public AvCodecContext AvCtx;

		/// <summary>
		/// 
		/// </summary>
		public AvFrame Frame;

		/// <summary>
		/// Accelerated DSP functions
		/// </summary>
		public readonly LLAudDspContext Dsp = new LLAudDspContext();

		/// <summary>
		/// Compressed frame data
		/// </summary>
		public CPointer<uint8_t> Frame_Data;

		/// <summary>
		/// Max bitstream size
		/// </summary>
		public c_int Max_Frame_Size;

		/// <summary>
		/// Context for filling the frame_data buffer
		/// </summary>
		public readonly PutBitContext Pb = new PutBitContext();

		// Frame size dependent frame information (set during initialization)

		/// <summary>
		/// Used compression features
		/// </summary>
		public uint32_t Decode_Flags;

		/// <summary>
		/// Frame is prefixed with its length
		/// </summary>
		public c_int Len_Prefix;

		/// <summary>
		/// Frame contains DRC data
		/// </summary>
		public c_int Dynamic_Range_Compression;

		/// <summary>
		/// Integer audio sample size for the unscaled IMDCT output (used to scale to [-1.0, 1.0])
		/// </summary>
		public uint8_t Bits_Per_Sample;

		/// <summary>
		/// Number of samples to output
		/// </summary>
		public uint16_t Samples_Per_Frame;

		/// <summary>
		/// 
		/// </summary>
		public uint16_t Log2_Frame_Size;

		/// <summary>
		/// Number of channels in the stream (same as AVCodecContext.num_channels)
		/// </summary>
		public int8_t Num_Channels;

		/// <summary>
		/// Lfe channel index
		/// </summary>
		public int8_t Lfe_Channel;

		/// <summary>
		/// 
		/// </summary>
		public uint8_t Max_Num_Subframes;

		/// <summary>
		/// Number of bits used for the subframe length
		/// </summary>
		public uint8_t Subframe_Len_Bits;

		/// <summary>
		/// Flag indicating that the subframe is of maximum size when the first subframe length bit is 1
		/// </summary>
		public uint8_t Max_Subframe_Len_Bit;

		/// <summary>
		/// 
		/// </summary>
		public uint16_t Min_Samples_Per_Subframe;

		// Packet decode state

		/// <summary>
		/// Bitstream reader context for the packet
		/// </summary>
		public readonly GetBitContext Pgb = new GetBitContext();

		/// <summary>
		/// Start offset of the next WMA packet in the demuxer packet
		/// </summary>
		public c_int Next_Packet_Start;

		/// <summary>
		/// Offset to the frame in the packet
		/// </summary>
		public uint8_t Packet_Offset;

		/// <summary>
		/// Current packet number
		/// </summary>
		public uint8_t Packet_Sequence_Number;

		/// <summary>
		/// Saved number of bits
		/// </summary>
		public c_int Num_Saved_Bits;

		/// <summary>
		/// Frame offset in the bit reservoir
		/// </summary>
		public c_int Frame_Offset;

		/// <summary>
		/// Subframe offset in the bit reservoir
		/// </summary>
		public c_int Subframe_Offset;

		/// <summary>
		/// Set in case of bitstream error
		/// </summary>
		public uint8_t Packet_Loss;

		/// <summary>
		/// Set when a packet is fully decoded
		/// </summary>
		public uint8_t Packet_Done;

		// Frame decode state

		/// <summary>
		/// Current frame number (not used for decoding)
		/// </summary>
		public uint32_t Frame_Num;

		/// <summary>
		/// Bitstream reader context
		/// </summary>
		public readonly GetBitContext Gb = new GetBitContext();

		/// <summary>
		/// Buffer size in bits
		/// </summary>
		public c_int Buf_Bit_Size;

		/// <summary>
		/// Current sample buffer pointer (16-bit)
		/// </summary>
		public readonly CPointer<int16_t>[] Samples_16 = new CPointer<int16_t>[WmaConstants.WmaLL_Max_Channels];

		/// <summary>
		/// Current sample buffer pointer (24-bit)
		/// </summary>
		public readonly CPointer<int32_t>[] Samples_32 = new CPointer<int32_t>[WmaConstants.WmaLL_Max_Channels];

		/// <summary>
		/// Gain for the DRC tool
		/// </summary>
		public uint8_t Drc_Gain;

		/// <summary>
		/// Skip output step
		/// </summary>
		public int8_t Skip_Frame;

		/// <summary>
		/// All subframes decoded?
		/// </summary>
		public int8_t Parsed_All_Subframes;

		// Subframe/block decode state

		/// <summary>
		/// Number of channels that contain the subframe
		/// </summary>
		public int8_t Channels_For_Cur_Subframe;

		/// <summary>
		/// 
		/// </summary>
		public readonly int8_t[] Channel_Indexes_For_Cur_Subframe = new int8_t[WmaConstants.WmaLL_Max_Channels];

		/// <summary>
		/// Per channel data
		/// </summary>
		public readonly WmaLLChannelCtx[] Channel = ArrayHelper.InitializeArray<WmaLLChannelCtx>(WmaConstants.WmaLL_Max_Channels);

		// WMA Lossless-specific

		/// <summary>
		/// 
		/// </summary>
		public uint8_t Do_Arith_Coding;

		/// <summary>
		/// 
		/// </summary>
		public uint8_t Do_Ac_Filter;

		/// <summary>
		/// 
		/// </summary>
		public uint8_t Do_Inter_Ch_Decorr;

		/// <summary>
		/// 
		/// </summary>
		public uint8_t Do_Mclms;

		/// <summary>
		/// 
		/// </summary>
		public uint8_t Do_Lpc;

		/// <summary>
		/// 
		/// </summary>
		public int8_t AcFilter_Order;

		/// <summary>
		/// 
		/// </summary>
		public int8_t AcFilter_Scaling;

		/// <summary>
		/// 
		/// </summary>
		public readonly int16_t[] AcFilter_Coeffs = new int16_t[16];

		/// <summary>
		/// 
		/// </summary>
		public readonly c_int[][] AcFilter_PrevValues = ArrayHelper.Initialize2Arrays<c_int>(WmaConstants.WmaLL_Max_Channels, 16);

		/// <summary>
		/// 
		/// </summary>
		public int8_t Mclms_Order;

		/// <summary>
		/// 
		/// </summary>
		public int8_t Mclms_Scaling;

		/// <summary>
		/// 
		/// </summary>
		public readonly int16_t[] Mclms_Coeffs = new int16_t[WmaConstants.WmaLL_Max_Channels * WmaConstants.WmaLL_Max_Channels * 32];

		/// <summary>
		/// 
		/// </summary>
		public readonly int16_t[] Mclms_Coeffs_Cur = new int16_t[WmaConstants.WmaLL_Max_Channels * WmaConstants.WmaLL_Max_Channels];

		/// <summary>
		/// 
		/// </summary>
		public CPointer<int32_t> Mclms_PrevValues = new CPointer<int32_t>(WmaConstants.WmaLL_Max_Channels * 2 * 32);

		/// <summary>
		/// 
		/// </summary>
		public CPointer<int32_t> Mclms_Updates = new CPointer<int32_t>(WmaConstants.WmaLL_Max_Channels * 2 * 32);

		/// <summary>
		/// 
		/// </summary>
		public c_int Mclms_Recent;

		/// <summary>
		/// 
		/// </summary>
		public c_int Movave_Scaling;

		/// <summary>
		/// 
		/// </summary>
		public c_int Quant_StepSize;

		/// <summary>
		/// 
		/// </summary>
		public readonly
		(
			c_int Order,
			c_int Scaling,
			c_int CoefsEnd,
			c_int BitsEnd,
			CPointer<int16_t> Coefs,
			CPointer<int32_t> Lms_PrevValues,
			CPointer<int16_t> Lms_Updates,
			c_int Recent
		)[][] Cdlms = ArrayHelper.Initialize2Arrays<(c_int, c_int, c_int, c_int, CPointer<int16_t>, CPointer<int32_t>, CPointer<int16_t>, c_int)>(WmaConstants.WmaLL_Max_Channels, 9);

		/// <summary>
		/// 
		/// </summary>
		public readonly c_int[] Cdlms_Ttl = new c_int[WmaConstants.WmaLL_Max_Channels];

		/// <summary>
		/// 
		/// </summary>
		public c_int BV3Rtm;

		/// <summary>
		/// 
		/// </summary>
		public readonly c_int[] Is_Channel_Coded = new c_int[WmaConstants.WmaLL_Max_Channels];

		/// <summary>
		/// 
		/// </summary>
		public readonly c_int[] Update_Speed = new c_int[WmaConstants.WmaLL_Max_Channels];

		/// <summary>
		/// 
		/// </summary>
		public readonly c_int[] Transient = new c_int[WmaConstants.WmaLL_Max_Channels];

		/// <summary>
		/// 
		/// </summary>
		public readonly c_int[] Transient_Pos = new c_int[WmaConstants.WmaLL_Max_Channels];

		/// <summary>
		/// 
		/// </summary>
		public c_int Seekable_Tile;

		/// <summary>
		/// 
		/// </summary>
		public readonly c_uint[] Ave_Sum = new c_uint[WmaConstants.WmaLL_Max_Channels];

		/// <summary>
		/// 
		/// </summary>
		public readonly c_int[][] Channel_Residues = ArrayHelper.Initialize2Arrays<c_int>(WmaConstants.WmaLL_Max_Channels, WmaConstants.WmaLL_Block_Max_Size);

		/// <summary>
		/// 
		/// </summary>
		public readonly c_int[][] Lpc_Coefs = ArrayHelper.Initialize2Arrays<c_int>(WmaConstants.WmaLL_Max_Channels, 40);

		/// <summary>
		/// 
		/// </summary>
		public c_int Lpc_Order;

		/// <summary>
		/// 
		/// </summary>
		public c_int Lpc_Scaling;

		/// <summary>
		/// 
		/// </summary>
		public c_int Lpc_IntBits;
	}
}
