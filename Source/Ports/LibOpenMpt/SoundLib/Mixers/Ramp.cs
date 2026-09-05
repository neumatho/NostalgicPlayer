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
	internal struct Ramp
	{
		public int32 lRamp;
		public int32 rRamp;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Init(ModChannel chn)
		{
			lRamp = chn.RampLeftVol;
			rRamp = chn.RampRightVol;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Done(ModChannel channel)
		{
			channel.RampLeftVol = lRamp;
			channel.LeftVol = lRamp >> Mixer.VolumeRampPrecision;
			channel.RampRightVol = rRamp;
			channel.RightVol = rRamp >> Mixer.VolumeRampPrecision;
		}
	}
}
