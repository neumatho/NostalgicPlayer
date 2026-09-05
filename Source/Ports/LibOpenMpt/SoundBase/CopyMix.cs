/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Audio;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundBase.SampleConversion;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Dithers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundBase
{
	/// <summary>
	/// TNE: The original is duck typed on its dither argument, so it does
	/// not have to name a soundlib type. C# needs the concrete type to
	/// avoid an interface call per sample, hence the reference to the
	/// dithers from here
	/// </summary>
	internal static class CopyMix
	{
		/********************************************************************/
		/// <summary>
		/// TNE: The original takes fractionalBits and clipOutput as its own
		/// arguments, and builds the clip and the conversion from them.
		/// Here those two are handed in already built, so they carry the
		/// two values instead
		/// </summary>
		/********************************************************************/
		public static void ConvertBufferMixInternalFixedToBuffer<TClip, TConv, TOutBuf, TOutSample, TInBuf, TDither, TPrng>(TOutBuf outBuf, TInBuf inBuf, MultiChannelDither<TDither, TPrng> dither, size_t channels, size_t count) where TClip : ISampleClip<MixSampleInt> where TConv : ISampleConvert<TOutSample, MixSampleInt> where TOutBuf : struct, IAudioSpan<TOutSample, TOutBuf> where TOutSample : unmanaged where TInBuf : struct, IAudioSpan<MixSampleInt, TInBuf> where TDither : IDither<TPrng>, new()
		{
			uint32 ditherBits = (uint32)SampleFormatTraits.GetDitherBits<TOutSample>();

			for (size_t i = 0; i < count; ++i)
			{
				for (size_t channel = 0; channel < channels; ++channel)
					outBuf[channel, i] = TConv.Convert(TClip.Clip(dither.Process(ditherBits, channel, inBuf[channel, i])));
			}
		}
	}
}
