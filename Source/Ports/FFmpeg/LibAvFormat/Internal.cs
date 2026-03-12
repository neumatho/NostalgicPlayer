/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Internal
	{
		// Size of probe buffer, for guessing file type from file contents
		public const c_int Probe_Buf_Min = 2048;
		public const c_int Probe_Buf_Max = 1 << 20;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FFFormatContext FFFormatContext(AvFormatContext s)
		{
			return (FFFormatContext)s;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FFStream FFStream(AvStream st)
		{
			return (FFStream)st;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FFStream CFFStream(AvStream st)
		{
			return (FFStream)st;
		}
	}
}
