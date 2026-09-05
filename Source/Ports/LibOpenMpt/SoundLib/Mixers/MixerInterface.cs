/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Mixers
{
	/// <summary>
	/// 
	/// </summary>
	internal static class MixerInterface
	{
		/********************************************************************/
		/// <summary>
		/// Main sample render loop template
		/// </summary>
		/********************************************************************/
		public static void SampleLoop<TIn, TTraits, TInterp, TFilter, TMix>(ModChannel chn, CResampler resampler, CPointer<mixsample_t> outBuffer, c_uint numSamples) where TIn : unmanaged where TTraits : struct, IMixerTraits<TIn> where TInterp : struct, IInterpolator where TFilter : struct, IFilter where TMix : struct, IMixer
		{
			ModChannel c = chn;
			CPointer<TIn> inSample = c.pCurrentSample.Cast<TIn>();

			TInterp interpolate = default;
			interpolate.Init(c, resampler, numSamples);

			TFilter filter = default;
			filter.Init(c, TTraits.NumChannelsIn);

			TMix mix = default;
			mix.Init(c);

			c_uint samples = numSamples;
			SamplePosition smpPos = c.Position;		// Fixed-point sample position
			SamplePosition increment = c.Increment;	// Fixed-point sample increment

			Span<mixsample_t> outSample = stackalloc mixsample_t[TTraits.NumChannelsOut];

			while (samples-- != 0)
			{
				interpolate.Interpolate<TIn, TTraits>(outSample, inSample + (smpPos.GetInt() * TTraits.NumChannelsIn), smpPos.GetFract());
				filter.Filter(outSample, c, TTraits.NumChannelsIn);
				mix.Mix(outSample, c, outBuffer);

				outBuffer += TTraits.NumChannelsOut;
				smpPos += increment;
			}

			c.Position = smpPos;

			mix.Done(c);
			filter.Done(c, TTraits.NumChannelsIn);
			interpolate.Done(c);
		}
	}
}
