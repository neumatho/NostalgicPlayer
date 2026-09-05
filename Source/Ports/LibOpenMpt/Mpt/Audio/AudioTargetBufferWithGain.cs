/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundBase.SampleConversion;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Dithers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Audio
{
	/// <summary>
	/// 
	/// </summary>
	internal sealed class AudioTargetBufferWithGain<TAudioSpan, TSampleType, TConvert> : AudioTargetBuffer<TAudioSpan, TSampleType, TConvert> where TAudioSpan : struct, IAudioSpan<TSampleType, TAudioSpan> where TSampleType : unmanaged, INumber<TSampleType> where TConvert : ISampleConvert<TSampleType, MixSampleInt>
	{
		private readonly MixSampleFloat gainFactor;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudioTargetBufferWithGain(TAudioSpan buf, DithersWrapperOpenMpt dithers, c_float gainFactor_) : base(buf, dithers)
		{
			gainFactor = gainFactor_;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override void Process(Audio_Span_Interleaved<MixSampleInt> buffer)
		{
			bool isFloatingPoint = (typeof(TSampleType) == typeof(c_float)) || (typeof(TSampleType) == typeof(c_double));
			size_t countRendered_ = GetRenderedCount();

			if (!isFloatingPoint)
			{
				int32 gainFactor16_16 = SaturateRound.Saturate_Round<int32, MixSampleFloat>(gainFactor * (1 << 16));

				if (gainFactor16_16 != (1 << 16))
				{
					// Only apply gain when != +/- 0dB
					// No clipping prevention is done here
					for (size_t frame = 0; frame < buffer.Size_Frames(); ++frame)
					{
						for (size_t channel = 0; channel < buffer.Size_Channels(); ++channel)
							buffer[channel, frame] = Util.MulDiv(buffer[channel, frame], gainFactor16_16, 1 << 16);
					}
				}
			}

			base.Process(buffer);

			if (isFloatingPoint)
			{
				if (gainFactor != 1.0f)
				{
					// Only apply gain when != +/- 0dB
					TSampleType gain = TSampleType.CreateTruncating(gainFactor);

					for (size_t frame = 0; frame < buffer.Size_Frames(); ++frame)
					{
						for (size_t channel = 0; channel < buffer.Size_Channels(); ++channel)
							outputBuffer[channel, countRendered_ + frame] *= gain;
					}
				}
			}
		}
	}
}
