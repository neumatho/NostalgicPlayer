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
	internal readonly struct Int16SToIntS : IMixerTraits<int16>
	{
		public static c_int NumChannelsIn => 2;
		public static c_int NumChannelsOut => 2;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static mixsample_t Convert(int16 x)
		{
			return IntToIntTraits.Convert(x);
		}
	}
}
