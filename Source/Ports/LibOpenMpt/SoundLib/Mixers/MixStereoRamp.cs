/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Mixers
{
	/// <summary>
	/// 
	/// </summary>
	internal struct MixStereoRamp : IMixer
	{
		private Ramp @base;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Init(ModChannel chn)
		{
			@base.Init(chn);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Mix(ReadOnlySpan<mixsample_t> outSample, ModChannel chn, CPointer<mixsample_t> outBuffer)
		{
			@base.lRamp += chn.LeftRamp;
			@base.rRamp += chn.RightRamp;

			outBuffer[0] += outSample[0] * (@base.lRamp >> Mixer.VolumeRampPrecision);
			outBuffer[1] += outSample[1] * (@base.rRamp >> Mixer.VolumeRampPrecision);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Done(ModChannel chn)
		{
			@base.Done(chn);
		}
	}
}
