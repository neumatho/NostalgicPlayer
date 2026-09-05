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
	internal readonly struct NoInterpolation : IInterpolator
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Init(ModChannel c, CResampler resampler, c_uint numSamples)
		{
			// Adding 0.5 to the sample position before the interpolation loop starts
			// effectively gives us nearest-neighbour with rounding instead of truncation.
			// This gives us more consistent behaviour between forward and reverse playing of a sample
			c.Position += SamplePosition.Ratio(1, 2);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Interpolate<TIn, TTraits>(Span<mixsample_t> outSample, CPointer<TIn> inBuffer, uint32 posLo) where TIn : unmanaged where TTraits : struct, IMixerTraits<TIn>
		{
			for (c_int i = 0; i < TTraits.NumChannelsIn; i++)
				outSample[i] = TTraits.Convert(inBuffer[i]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Done(ModChannel channel)
		{
			channel.Position -= SamplePosition.Ratio(1, 2);
		}
	}
}
