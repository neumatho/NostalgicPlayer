/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Codec.Containers
{
	/// <summary>
	/// Different constants used by WMA codecs
	/// </summary>
	internal static class WmaConstants
	{
		/// <summary>
		/// Size of blocks
		/// </summary>
		public const c_int Block_Min_Bits = 7;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Block_Max_Bits = 11;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Block_Max_Size = 1 << Block_Max_Bits;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Block_Nb_Sizes = Block_Max_Bits - Block_Min_Bits + 1;

		/// <summary>
		/// 
		/// </summary>
		public const c_int High_Band_Max_Size = 16;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Nb_Lsp_Coefs = 10;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Max_Coded_SuperFrame_Size = 32768;

		/// <summary>
		/// Max number of subframes per channel
		/// </summary>
		public const c_int Max_Subframes = 32;

		/// <summary>
		/// Max number of scale factor bands
		/// </summary>
		public const c_int Max_Bands = 29;

		/// <summary>
		/// Maximum compressed frame size
		/// </summary>
		public const c_int Max_FrameSize = 32768;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Max_Order = 256;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Max_Channels = 2;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Noise_Tab_Size = 8192;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Lsp_Pow_Bits = 7;

		/// <summary>
		/// 
		/// </summary>
		public const c_int VlcBits = 9;

		/// <summary>
		/// 
		/// </summary>
		public const c_int VlcMax = (22 + VlcBits - 1) / VlcBits;

		/// <summary>
		/// 
		/// </summary>
		public const c_int ExpVlcBits = 8;

		/// <summary>
		/// 
		/// </summary>
		public const c_int ExpMax = (19 + ExpVlcBits - 1) / ExpVlcBits;

		/// <summary>
		/// 
		/// </summary>
		public const c_int HGainVlcBits = 9;

		/// <summary>
		/// 
		/// </summary>
		public const c_int HGainMax = (13 + HGainVlcBits - 1) / HGainVlcBits;

		/// <summary>
		/// 
		/// </summary>
		public const c_int ScaleVlcBits = 8;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Huff_Scale_Size = 121;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Huff_Scale_MaxBits = 19;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Huff_Scale_Rl_Size = 120;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Huff_Scale_Rl_MaxBits = 21;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Huff_Coef0_Size = 272;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Huff_Coef1_Size = 244;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Huff_Vec4_Size = 127;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Huff_Vec4_MaxBits = 14;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Huff_Vec2_Size = 137;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Huff_Vec2_MaxBits = 12;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Huff_Vec1_Size = 101;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Huff_Vec1_MaxBits = 11;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Vec4MaxDepth = (Huff_Vec4_MaxBits + VlcBits - 1) / VlcBits;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Vec2MaxDepth = (Huff_Vec2_MaxBits + VlcBits - 1) / VlcBits;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Vec1MaxDepth = (Huff_Vec1_MaxBits + VlcBits - 1) / VlcBits;

		/// <summary>
		/// 
		/// </summary>
		public const c_int ScaleMaxDepth = (Huff_Scale_MaxBits + ScaleVlcBits - 1) / ScaleVlcBits;

		/// <summary>
		/// 
		/// </summary>
		public const c_int ScaleRlMaxDepth = (Huff_Scale_Rl_MaxBits + VlcBits - 1) / VlcBits;

		/// <summary>
		/// Max number of handled channels
		/// </summary>
		public const c_int WmaPro_Max_Channels = 8;

		/// <summary>
		/// log2 of min block size
		/// </summary>
		public const c_int WmaPro_Block_Min_Bits = 6;

		/// <summary>
		/// log2 of max block size
		/// </summary>
		public const c_int WmaPro_Block_Max_Bits = 13;

		/// <summary>
		/// Minimum block size
		/// </summary>
		public const c_int WmaPro_Block_Min_Size = 1 << WmaPro_Block_Min_Bits;

		/// <summary>
		/// Maximum block size
		/// </summary>
		public const c_int WmaPro_Block_Max_Size = 1 << WmaPro_Block_Max_Bits;

		/// <summary>
		/// Possible block sizes
		/// </summary>
		public const c_int WmaPro_Block_Sizes = WmaPro_Block_Max_Bits - WmaPro_Block_Min_Bits + 1;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Xma_Max_Channels_Stream = 2;

		/// <summary>
		/// Max number of handled channels
		/// </summary>
		public const c_int WmaLL_Max_Channels = 8;

		/// <summary>
		/// Log2 of max block size
		/// </summary>
		public const c_int WmaLL_Block_Max_Bits = 14;

		/// <summary>
		/// Maximum block size
		/// </summary>
		public const c_int WmaLL_Block_Max_Size = 1 << WmaLL_Block_Max_Bits;

		/// <summary>
		/// Pad coef buffers with 0 for use with SIMD
		/// </summary>
		public const c_int WmaLL_Coeff_Pad_Size = 16;
	}
}
