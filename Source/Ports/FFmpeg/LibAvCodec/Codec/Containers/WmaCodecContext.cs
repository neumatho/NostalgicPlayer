/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using WmaCoef = System.Single;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Codec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class WmaCodecContext : IPrivateData
	{
		/// <summary>
		/// 
		/// </summary>
		public AvCodecContext AvCtx;

		/// <summary>
		/// 
		/// </summary>
		public readonly GetBitContext Gb = new GetBitContext();

		/// <summary>
		/// 
		/// </summary>
		public readonly PutBitContext Pb = new PutBitContext();

		/// <summary>
		/// 1 = 0x160 (WMAV1), 2 = 0x161 (WMAV2)
		/// </summary>
		public c_int Version;

		/// <summary>
		/// 
		/// </summary>
		public c_int Use_Bit_Reservoir;

		/// <summary>
		/// 
		/// </summary>
		public c_int Use_Variable_Block_Len;

		/// <summary>
		/// Exponent coding: 0 = lsp, 1 = vlc + delta
		/// </summary>
		public c_int Use_Exp_Vlc;

		/// <summary>
		/// True if perceptual noise is added
		/// </summary>
		public c_int Use_Noise_Coding;

		/// <summary>
		/// 
		/// </summary>
		public c_int Byte_Offset_Bits;

		/// <summary>
		/// 
		/// </summary>
		public readonly Vlc Exp_Vlc = new Vlc();

		/// <summary>
		/// 
		/// </summary>
		public readonly c_int[] Exponent_Sizes = new c_int[WmaConstants.Block_Nb_Sizes];

		/// <summary>
		/// 
		/// </summary>
		public readonly uint16_t[][] Exponent_Bands = ArrayHelper.Initialize2Arrays<uint16_t>(WmaConstants.Block_Nb_Sizes, 25);

		/// <summary>
		/// Index of first coef in high band
		/// </summary>
		public readonly c_int[] High_Band_Start = new c_int[WmaConstants.Block_Nb_Sizes];

		/// <summary>
		/// First coded coef
		/// </summary>
		public c_int Coefs_Start;

		/// <summary>
		/// Max number of coded coefficients
		/// </summary>
		public readonly c_int[] Coefs_End = new c_int[WmaConstants.Block_Nb_Sizes];

		/// <summary>
		/// 
		/// </summary>
		public readonly c_int[] Exponent_High_Sizes = new c_int[WmaConstants.Block_Nb_Sizes];

		/// <summary>
		/// 
		/// </summary>
		public readonly c_int[][] Exponent_High_Bands = ArrayHelper.Initialize2Arrays<c_int>(WmaConstants.Block_Nb_Sizes, WmaConstants.High_Band_Max_Size);

		/// <summary>
		/// 
		/// </summary>
		public readonly Vlc Hgain_Vlc = new Vlc();

		/// <summary>
		/// 
		/// </summary>
		public readonly c_int[][] High_Band_Coded = ArrayHelper.Initialize2Arrays<c_int>(WmaConstants.Max_Channels, WmaConstants.High_Band_Max_Size);

		/// <summary>
		/// 
		/// </summary>
		public readonly c_int[][] High_Band_Values = ArrayHelper.Initialize2Arrays<c_int>(WmaConstants.Max_Channels, WmaConstants.High_Band_Max_Size);

		// There are two possible tables for spectral coefficients

		/// <summary>
		/// 
		/// </summary>
		public readonly Vlc[] Coef_Vlc = ArrayHelper.InitializeArray<Vlc>(2);

		/// <summary>
		/// 
		/// </summary>
		public readonly CPointer<uint16_t>[] Run_Table = new CPointer<uint16_t>[2];

		/// <summary>
		/// 
		/// </summary>
		public readonly CPointer<c_float>[] Level_Table = new CPointer<c_float>[2];

		/// <summary>
		/// 
		/// </summary>
		public readonly CPointer<uint16_t>[] Int_Table = new CPointer<uint16_t>[2];

		/// <summary>
		/// 
		/// </summary>
		public readonly CoefVlcTable[] Coef_Vlcs = new CoefVlcTable[2];

		// Frame info

		/// <summary>
		/// Frame length in samples
		/// </summary>
		public c_int Frame_Len;

		/// <summary>
		/// Frame_len = 1 ‹‹ frame_len_bits
		/// </summary>
		public c_int Frame_Len_Bits;

		/// <summary>
		/// Number of block sizes
		/// </summary>
		public c_int Nb_Block_Sizes;

		// Block info

		/// <summary>
		/// 
		/// </summary>
		public c_int Reset_Block_Lengths;

		/// <summary>
		/// Log2 of current block length
		/// </summary>
		public c_int Block_Len_Bits;

		/// <summary>
		/// Log2 of next block length
		/// </summary>
		public c_int Next_Block_Len_Bits;

		/// <summary>
		/// Log2 of prev block length
		/// </summary>
		public c_int Prev_Block_Len_Bits;

		/// <summary>
		/// Block length in samples
		/// </summary>
		public c_int Block_Len;

		/// <summary>
		/// Block number in current frame
		/// </summary>
		public c_int Block_Num;

		/// <summary>
		/// Current position in frame
		/// </summary>
		public c_int Block_Pos;

		/// <summary>
		/// True if mid/side stereo mode
		/// </summary>
		public uint8_t Ms_Stereo;

		/// <summary>
		/// True if channel is coded
		/// </summary>
		public readonly uint8_t[] Channel_Coded = new uint8_t[WmaConstants.Max_Channels];

		/// <summary>
		/// Log2 ratio frame/exp. length
		/// </summary>
		public readonly c_int[] Exponents_BSize = new c_int[WmaConstants.Max_Channels];

		/// <summary>
		/// 
		/// </summary>
		public readonly c_float[][] Exponents = ArrayHelper.Initialize2Arrays<c_float>(WmaConstants.Max_Channels, WmaConstants.Block_Max_Size);

		/// <summary>
		/// 
		/// </summary>
		public readonly c_float[] Max_Exponent = new c_float[WmaConstants.Max_Channels];

		/// <summary>
		/// 
		/// </summary>
		public readonly WmaCoef[][] Coefs1 = ArrayHelper.Initialize2Arrays<WmaCoef>(WmaConstants.Max_Channels, WmaConstants.Block_Max_Size);

		/// <summary>
		/// 
		/// </summary>
		public readonly c_float[][] Coefs = ArrayHelper.Initialize2Arrays<c_float>(WmaConstants.Max_Channels, WmaConstants.Block_Max_Size);

		/// <summary>
		/// 
		/// </summary>
		public readonly c_float[] Output = new c_float[WmaConstants.Block_Max_Size * 2];

		/// <summary>
		/// 
		/// </summary>
		public readonly AvTxContext[] Mdct_Ctx = new AvTxContext[WmaConstants.Block_Nb_Sizes];

		/// <summary>
		/// 
		/// </summary>
		public readonly UtilFunc.Av_Tx_Fn[] Mdct_Fn = new UtilFunc.Av_Tx_Fn[WmaConstants.Block_Nb_Sizes];

		/// <summary>
		/// 
		/// </summary>
		public readonly c_float[][] Windows = new c_float[WmaConstants.Block_Nb_Sizes][];

		/// <summary>
		/// Output buffer for one frame and the last for IMDCT windowing
		/// </summary>
		public readonly c_float[][] Frame_Out = ArrayHelper.Initialize2Arrays<c_float>(WmaConstants.Max_Channels, WmaConstants.Block_Max_Size * 2);

		// Last frame info

		/// <summary>
		/// 
		/// </summary>
		public readonly CPointer<uint8_t> Last_SuperFrame = new CPointer<uint8_t>(WmaConstants.Max_Coded_SuperFrame_Size + Defs.Av_Input_Buffer_Padding_Size);

		/// <summary>
		/// 
		/// </summary>
		public c_int Last_BitOffset;

		/// <summary>
		/// 
		/// </summary>
		public c_int Last_SuperFrame_Len;

		/// <summary>
		/// 
		/// </summary>
		public readonly c_int[] Exponents_Initialized = new c_int[WmaConstants.Max_Channels];

		/// <summary>
		/// 
		/// </summary>
		public readonly c_float[] Noise_Table = new c_float[WmaConstants.Noise_Tab_Size];

		/// <summary>
		/// 
		/// </summary>
		public c_int Noise_Index;

		/// <summary>
		/// XXX: Suppress that and integrate it in the noise array
		/// </summary>
		public c_float Noise_Mult;

		// Lsp_To_Curve tables

		/// <summary>
		/// 
		/// </summary>
		public readonly c_float[] Lsp_Cos_Table = new c_float[WmaConstants.Block_Max_Size];

		/// <summary>
		/// 
		/// </summary>
		public readonly c_float[] Lsp_Pow_E_Table = new c_float[256];

		/// <summary>
		/// 
		/// </summary>
		public readonly c_float[] Lsp_Pow_M_Table1 = new c_float[1 << WmaConstants.Lsp_Pow_Bits];

		/// <summary>
		/// 
		/// </summary>
		public readonly c_float[] Lsp_Pow_M_Table2 = new c_float[1 << WmaConstants.Lsp_Pow_Bits];

		/// <summary>
		/// 
		/// </summary>
		public AvFloatDspContext fDsp;

		/// <summary>
		/// Decode flag to output remaining samples after EOF
		/// </summary>
		public c_int Eof_Done;
	}
}
