/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundBase;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundBase.SampleConversion;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Dithers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Audio
{
	/// <summary>
	/// TNE: The original picks the conversion by specializing
	/// SC::ConvertFixedPoint on the output sample type. C# has no partial
	/// specialization, so TConvert names it instead
	/// </summary>
	internal class AudioTargetBuffer<TAudioSpan, TSampleType, TConvert> : IAudioTarget where TAudioSpan : struct, IAudioSpan<TSampleType, TAudioSpan> where TSampleType : unmanaged, INumber<TSampleType> where TConvert : ISampleConvert<TSampleType, MixSampleInt>
	{
		private size_t countRendered;
		private readonly DithersWrapperOpenMpt dithers;

		protected TAudioSpan outputBuffer;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AudioTargetBuffer(TAudioSpan buf, DithersWrapperOpenMpt dithers_)
		{
			countRendered = 0;
			dithers = dithers_;
			outputBuffer = buf;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public size_t GetRenderedCount()
		{
			return countRendered;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual void Process(Audio_Span_Interleaved<MixSampleInt> buffer)
		{
			dithers.Variant().visit(
				ditherInstance => Convert(ditherInstance, buffer),
				ditherInstance => Convert(ditherInstance, buffer),
				ditherInstance => Convert(ditherInstance, buffer),
				ditherInstance => Convert(ditherInstance, buffer));

			countRendered += buffer.Size_Frames();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// TNE: Plays the part of the generic lambda the original hands to
		/// std::visit. It only exists so the type arguments that cannot be
		/// inferred are written once instead of once per alternative
		/// </summary>
		/********************************************************************/
		private void Convert<TDither, TPrng>(MultiChannelDither<TDither, TPrng> ditherInstance, Audio_Span_Interleaved<MixSampleInt> buffer) where TDither : IDither<TPrng>, new()
		{
			CopyMix.ConvertBufferMixInternalFixedToBuffer<ClipFixed<MixFractionalBits, ClipOutput_False>, TConvert, Audio_Span_With_Offset<TAudioSpan, TSampleType>, TSampleType, Audio_Span_Interleaved<MixSampleInt>, TDither, TPrng>(new Audio_Span_With_Offset<TAudioSpan, TSampleType>(outputBuffer, countRendered), buffer, ditherInstance, buffer.Size_Channels(), buffer.Size_Frames());
		}
		#endregion
	}
}
