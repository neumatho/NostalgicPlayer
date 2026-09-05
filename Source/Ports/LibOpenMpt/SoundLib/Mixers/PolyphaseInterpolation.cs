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
	internal struct PolyphaseInterpolation : IInterpolator
	{
		private CPointer<Sinc_Type> sinc;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Init(ModChannel chn, CResampler resampler, c_uint numSamples)
		{
			sinc = new CPointer<Sinc_Type>(((chn.Increment > new SamplePosition(0x130000000)) || (chn.Increment < new SamplePosition(-0x130000000))) ?
				(((chn.Increment > new SamplePosition(0x180000000)) || (chn.Increment < new SamplePosition(-0x180000000))) ? resampler.gDownSample2x : resampler.gDownSample13x) : resampler.gKaiserSinc);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Interpolate<TIn, TTraits>(Span<mixsample_t> outSample, CPointer<TIn> inBuffer, uint32 posLo) where TIn : unmanaged where TTraits : struct, IMixerTraits<TIn>
		{
			CPointer<Sinc_Type> lut = sinc + ((((c_int)(posLo >> (32 - CResampler.Sinc_Phases_Bits))) & CResampler.Sinc_Mask) * CResampler.Sinc_Width);

			for (c_int i = 0; i < TTraits.NumChannelsIn; i++)
			{
				outSample[i] = ((lut[0] * TTraits.Convert(inBuffer[i - (3 * TTraits.NumChannelsIn)])) +
								(lut[1] * TTraits.Convert(inBuffer[i - (2 * TTraits.NumChannelsIn)])) +
								(lut[2] * TTraits.Convert(inBuffer[i - TTraits.NumChannelsIn])) +
								(lut[3] * TTraits.Convert(inBuffer[i])) +
								(lut[4] * TTraits.Convert(inBuffer[i + TTraits.NumChannelsIn])) +
								(lut[5] * TTraits.Convert(inBuffer[i + (2 * TTraits.NumChannelsIn)])) +
								(lut[6] * TTraits.Convert(inBuffer[i + (3 * TTraits.NumChannelsIn)])) +
								(lut[7] * TTraits.Convert(inBuffer[i + (4 * TTraits.NumChannelsIn)]))) / (1 << CResampler.Sinc_QuantShift);
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
