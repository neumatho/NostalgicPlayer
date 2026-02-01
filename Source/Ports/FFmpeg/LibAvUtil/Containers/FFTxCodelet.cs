/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class FFTxCodelet
	{
		/// <summary>
		/// Codelet name
		/// </summary>
		public CPointer<char> Name;

		/// <summary>
		/// Codelet function, != NULL
		/// </summary>
		public UtilFunc.Av_Tx_Fn Function;

		/// <summary>
		/// Type of codelet transform
		/// </summary>
		public AvTxType Type;

		/// <summary>
		/// A combination of AVTXFlags and codelet
		/// flags that describe its properties
		/// </summary>
		public AvTxFlags Flags;

		/// <summary>
		/// Length factors. MUST be coprime
		/// </summary>
		public c_int[] Factors = new c_int[UtilConstants.Tx_Max_Factors];

		/// <summary>
		/// Minimum number of factors that have to
		/// be a modulo of the length. Must not be 0
		/// </summary>
		public c_int Nb_Factors;

		/// <summary>
		/// Minimum length of transform, must be >= 1
		/// </summary>
		public c_int Min_Len;

		/// <summary>
		/// Maximum length of transform
		/// </summary>
		public c_int Max_Len;

		/// <summary>
		/// Optional callback for current context initialization
		/// </summary>
		public UtilFunc.Tx_Init_Delegate Init;

		/// <summary>
		/// Optional callback for uninitialization
		/// </summary>
		public UtilFunc.Tx_Uninit_Delegate Uninit;

		/// <summary>
		/// CPU flags. If any negative flags like
		/// SLOW are present, will avoid picking.
		/// 0x0 to signal it's a C codelet
		/// </summary>
		public AvCpuFlag Cpu_Flags;

		/// <summary>
		/// ‹ 0 = least, 0 = no pref, › 0 = prefer
		/// </summary>
		public c_int Prio;
	}
}
