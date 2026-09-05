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
	internal struct FirFilterInterpolation : IInterpolator
	{
		private CPointer<int16> wFirLut;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Init(ModChannel c, CResampler resampler, c_uint numSamples)
		{
			wFirLut = new CPointer<int16>(resampler.m_WindowedFir.Lut);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Interpolate<TIn, TTraits>(Span<mixsample_t> outSample, CPointer<TIn> inBuffer, uint32 posLo) where TIn : unmanaged where TTraits : struct, IMixerTraits<TIn>
		{
			CPointer<int16> lut = wFirLut + (c_int)((((posLo >> 16) + WindowedFir.WFir_FracHalve) >> WindowedFir.WFir_FracShift) & WindowedFir.WFir_FracMask);

			for (c_int i = 0; i < TTraits.NumChannelsIn; i++)
			{
				mixsample_t vol1 = (lut[0] * TTraits.Convert(inBuffer[i - (3 * TTraits.NumChannelsIn)])) +
								   (lut[1] * TTraits.Convert(inBuffer[i - (2 * TTraits.NumChannelsIn)])) +
								   (lut[2] * TTraits.Convert(inBuffer[i - TTraits.NumChannelsIn])) +
								   (lut[3] * TTraits.Convert(inBuffer[i]));
				mixsample_t vol2 = (lut[4] * TTraits.Convert(inBuffer[i + (1 * TTraits.NumChannelsIn)])) +
								   (lut[5] * TTraits.Convert(inBuffer[i + (2 * TTraits.NumChannelsIn)])) +
								   (lut[6] * TTraits.Convert(inBuffer[i + (3 * TTraits.NumChannelsIn)])) +
								   (lut[7] * TTraits.Convert(inBuffer[i + (4 * TTraits.NumChannelsIn)]));

				outSample[i] = ((vol1 / 2) + (vol2 / 2)) / (1 << (WindowedFir.WFir_16BitShift - 1));
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
