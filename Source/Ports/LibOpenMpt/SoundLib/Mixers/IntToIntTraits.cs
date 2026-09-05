/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Mixers
{
	/// <summary>
	/// 
	/// </summary>
	internal class IntToIntTraits
	{
		private const c_int MixPrecision = 16;

		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static mixsample_t Convert(int8 x)
		{
			return x * (1 << (MixPrecision - (sizeof(int8) * 8)));
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static mixsample_t Convert(int16 x)
		{
			return x * (1 << (MixPrecision - (sizeof(int16) * 8)));
		}
	}
}
