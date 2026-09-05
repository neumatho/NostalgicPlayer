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
	internal readonly struct LinearInterpolation : IInterpolator
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Init(ModChannel c, CResampler resampler, c_uint numSamples)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Interpolate<TIn, TTraits>(Span<mixsample_t> outSample, CPointer<TIn> inBuffer, uint32 posLo) where TIn : unmanaged where TTraits : struct, IMixerTraits<TIn>
		{
			mixsample_t fract = (mixsample_t)(posLo >> 18);

			for (c_int i = 0; i < TTraits.NumChannelsIn; i++)
			{
				mixsample_t srcVol = TTraits.Convert(inBuffer[i]);
				mixsample_t destVol = TTraits.Convert(inBuffer[i + TTraits.NumChannelsIn]);

				outSample[i] = srcVol + ((fract * (destVol - srcVol)) / 16384);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Done(ModChannel channel)
		{
		}
	}
}
