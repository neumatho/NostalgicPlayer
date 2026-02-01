/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Different constants used in LibAvUtil
	/// </summary>
	public static class UtilConstants
	{
		/********************************************************************/
		/// <summary>
		/// Undefined timestamp value
		///
		/// Usually reported by demuxer that work on containers that do not
		/// provide either pts or dts
		/// </summary>
		/********************************************************************/
		public const int64_t Av_NoPts_Value = unchecked((int64_t)0x8000000000000000);



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public const c_int Av_Time_Base = 1000000;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly AvRational Av_Time_Base_Q = new AvRational(1, Av_Time_Base);



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public const string Size_Specifier = "zu";



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public const c_int AvPalette_Size = 1024;



		/********************************************************************/
		/// <summary>
		/// Maximum number of factors a codelet may have. Arbitrary
		/// </summary>
		/********************************************************************/
		public const c_int Tx_Max_Factors = 16;



		/********************************************************************/
		/// <summary>
		/// Maximum number of returned results for ff_tx_decompose_length.
		/// Arbitrary
		/// </summary>
		/********************************************************************/
		public const c_int Tx_Max_Decompositions = 512;



		/********************************************************************/
		/// <summary>
		/// When used alone, signals that the codelet supports all factors.
		/// Otherwise, if other factors are present, it signals that whatever
		/// remains will be supported, as long as the other factors are a
		/// component of the length
		/// </summary>
		/********************************************************************/
		public const c_int Tx_Factor_Any = -1;



		/********************************************************************/
		/// <summary>
		/// Maximum amount of subtransform functions, subtransforms and
		/// factors. Arbitrary
		/// </summary>
		/********************************************************************/
		public const c_int Tx_Max_Sub = 4;



		/********************************************************************/
		/// <summary>
		/// Special length value to permit all lengths
		/// </summary>
		/********************************************************************/
		public const c_int Tx_Len_Unlimited = -1;



		/********************************************************************/
		/// <summary>
		/// H.264 slice threading seems to be buggy with more than 16
		/// threads, limit the number of threads to 16 for automatic
		/// detection
		/// </summary>
		/********************************************************************/
		public const c_int Max_Auto_Threads = 16;
	}
}
