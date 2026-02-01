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
	}
}
