/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Codec.Containers
{
	/// <summary>
	/// Main decoder context
	/// </summary>
	internal class WmaProDecodeCtx : IPrivateData
	{
		// Generic decoder variables

		/// <summary>
		/// Codec context for av_log
		/// </summary>
		public AvCodecContext AvCtx;

		/// <summary>
		/// 
		/// </summary>
		public AvFloatDspContext fDsp;

		/// <summary>
		/// Compressed frame data
		/// </summary>
		public readonly uint8_t[] Frame_Data = new uint8_t[WmaConstants.Max_FrameSize + Defs.Av_Input_Buffer_Padding_Size];

		/// <summary>
		/// Context for filling the frame_data buffer
		/// </summary>
		public readonly PutBitContext Pb = new PutBitContext();

		/// <summary>
		/// MDCT context per block size
		/// </summary>
		public readonly AvTxContext[] Tx = new AvTxContext[WmaConstants.WmaPro_Block_Sizes];

		/// <summary>
		/// 
		/// </summary>
		public readonly UtilFunc.Av_Tx_Fn[] Tx_Fn = new UtilFunc.Av_Tx_Fn[WmaConstants.WmaPro_Block_Sizes];

		/// <summary>
		/// IMDCT output buffer
		/// </summary>
		public readonly CPointer<c_float> Tmp = new CPointer<c_float>(WmaConstants.WmaPro_Block_Max_Size);

		/// <summary>
		/// Windows for the different block sizes
		/// </summary>
		public readonly c_float[][] Windows = new c_float[WmaConstants.WmaPro_Block_Sizes][];

		// Frame size dependent frame information (set during initialization)

		/// <summary>
		/// Used compression features
		/// </summary>
		public uint32_t Decode_Flags;

		/// <summary>
		/// Frame is prefixed with its length
		/// </summary>
		public uint8_t Len_Prefix;

		/// <summary>
		/// Frame contains DRC data
		/// </summary>
		public uint8_t Dynamic_Range_Compression;

		/// <summary>
		/// Integer audio sample size for the unscaled IMDCT output (used to scale to [-1.0, 1.0])
		/// </summary>
		public uint8_t Bits_Per_Sample;

		/// <summary>
		/// Number of samples to output
		/// </summary>
		public uint16_t Samples_Per_Frame;

		/// <summary>
		/// Number of samples to skip at start
		/// </summary>
		public uint16_t Trim_Start;

		/// <summary>
		/// Number of samples to skip at end
		/// </summary>
		public uint16_t Trim_End;

		/// <summary>
		/// 
		/// </summary>
		public uint16_t Log2_Frame_Size;

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

		/// <summary>
		/// Scale factor bands per block size
		/// </summary>
		public readonly int8_t[] Num_Sfb = new int8_t[WmaConstants.WmaPro_Block_Sizes];

		/// <summary>
		/// Scale factor band offsets (multiples of 4)
		/// </summary>
		public readonly int16_t[][] Sfb_Offsets = ArrayHelper.Initialize2Arrays<int16_t>(WmaConstants.WmaPro_Block_Sizes, WmaConstants.Max_Bands);

		/// <summary>
		/// Scale factor resample matrix
		/// </summary>
		public readonly int8_t[][][] Sf_Offsets = ArrayHelper.Initialize3Arrays<int8_t>(WmaConstants.WmaPro_Block_Sizes, WmaConstants.WmaPro_Block_Sizes, WmaConstants.Max_Bands);

		/// <summary>
		/// Subwoofer cutoff values
		/// </summary>
		public readonly int16_t[] Subwoofer_CutOffs = new int16_t[WmaConstants.WmaPro_Block_Sizes];

		// Packet decode state

		/// <summary>
		/// Bitstream reader context for the packet
		/// </summary>
		public readonly GetBitContext Pgb = new GetBitContext();

		/// <summary>
		/// Start offset of the next wma packet in the demuxer packet
		/// </summary>
		public c_int Next_Packet_Start;

		/// <summary>
		/// Frame offset in the packet
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

		/// <summary>
		/// Set when EOF reached and extra subframe is written (XMA1/2)
		/// </summary>
		public uint8_t Eof_Done;

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

		/// <summary>
		/// Packets to skip to find next packet in a stream (XMA1/2)
		/// </summary>
		public uint8_t Skip_Packets;

		// Subframe/block decode state

		/// <summary>
		/// Current subframe length
		/// </summary>
		public int16_t Subframe_Len;

		/// <summary>
		/// Number of channels in stream (XMA1/2)
		/// </summary>
		public int8_t Nb_Channels;

		/// <summary>
		/// Number of channels that contain the subframe
		/// </summary>
		public int8_t Channels_For_Cur_Subframe;

		/// <summary>
		/// 
		/// </summary>
		public readonly int8_t[] Channel_Indexes_For_Cur_Subframe = new int8_t[WmaConstants.WmaPro_Max_Channels];

		/// <summary>
		/// Number of scale factor bands
		/// </summary>
		public int8_t Num_Bands;

		/// <summary>
		/// Number of vector coded coefficients is part of the bitstream
		/// </summary>
		public int8_t Transmit_Num_Vec_Coeffs;

		/// <summary>
		/// Sfb offsets for the current block
		/// </summary>
		public CPointer<int16_t> Cur_Sfb_Offsets;

		/// <summary>
		/// Index for the num_sfb, sfb_offsets, sf_offsets and subwoofer_cutoffs tables
		/// </summary>
		public uint8_t Table_Idx;

		/// <summary>
		/// Length of escaped coefficients
		/// </summary>
		public int8_t Esc_Len;

		/// <summary>
		/// Number of channel groups
		/// </summary>
		public uint8_t Num_ChGroups;

		/// <summary>
		/// Channel group information
		/// </summary>
		public readonly WmaProChannelGrp[] ChGroup = ArrayHelper.InitializeArray<WmaProChannelGrp>(WmaConstants.WmaPro_Max_Channels);

		/// <summary>
		/// Per channel data
		/// </summary>
		public readonly WmaProChannelCtx[] Channel = ArrayHelper.InitializeArray<WmaProChannelCtx>(WmaConstants.WmaPro_Max_Channels);
	}
}
