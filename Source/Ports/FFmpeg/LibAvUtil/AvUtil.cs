/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class AvUtil
	{
		/// <summary>
		/// Factor to convert from H.263 QP to lambda
		/// </summary>
		public const c_int FF_Qp2Lambda = 118;

		/// <summary>
		/// 
		/// </summary>
		public const c_int FF_Lambda_Max = (256 * 128) - 1;

		/// <summary>
		/// 
		/// </summary>
		public const c_int Av_FourCC_Max_String_Size = 32;

		/********************************************************************/
		/// <summary>
		/// Return x default pointer in case p is NULL
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> Av_X_If_Null<T>(CPointer<T> p, CPointer<T> x)
		{
			return p.IsNotNull ? p : x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<char> Av_FourCC2Str(c_uint fourCC)
		{
			return Utils.Av_FourCC_Make_String(new CPointer<char>(Av_FourCC_Max_String_Size), fourCC);
		}
	}
}
