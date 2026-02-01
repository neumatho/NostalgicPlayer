/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal static class AvFormatInternal
	{
		public const int64_t Relative_Ts_Base = int64_t.MaxValue - (1L << 48);

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FormatContextInternal FF_FC_Internal(AvFormatContext s)
		{
			return (FormatContextInternal)s;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Is_Relative(int64_t ts)
		{
			return ts > (Relative_Ts_Base - (1L << 48));
		}
	}
}
