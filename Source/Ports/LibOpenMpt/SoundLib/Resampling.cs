/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Resampling
	{
		public enum AmigaFilter
		{
			Off = 0,
			A500 = 1,
			A1200 = 2,
			Unfiltered = 3
		}

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ResamplingMode Default()
		{
			return ResamplingMode.Sinc8Lp;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsKnownMode(ResamplingMode mode)
		{
			return (mode >= ResamplingMode.Nearest) && (mode < ResamplingMode.Default);
		}
	}
}
