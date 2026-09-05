/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundBase.SampleConversion;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Functions for copying ModSample data.
	/// </summary>
	internal static class ModSampleCopy
	{
		/********************************************************************/
		/// <summary>
		/// Copy a mono sample data buffer
		/// </summary>
		/********************************************************************/
		public static size_t CopyMonoSample<TConv, TOut>(ModSample sample, CPointer<uint8> sourceBuffer, size_t sourceSize, TConv conv = default) where TConv : struct, ISampleConversion<TOut> where TOut : unmanaged
		{
			size_t frameSize = TConv.Input_Inc;
			size_t countFrames = Math.Min(sourceSize / frameSize, sample.nLength);
			size_t numFrames = countFrames;

			TConv sampleConv = conv;
			CPointer<byte> inBuf = Memory.Byte_Cast<byte>(sourceBuffer);
			CPointer<TOut> outBuf = sample.SampleEv().Cast<TOut>();

			while (numFrames-- != 0)
			{
				outBuf[0] = sampleConv.Convert(inBuf);
				inBuf += TConv.Input_Inc;
				outBuf++;
			}

			return frameSize * countFrames;
		}



		/********************************************************************/
		/// <summary>
		/// Copy a stereo interleaved sample data buffer
		/// </summary>
		/********************************************************************/
		public static size_t CopyStereoInterleavedSample<TConv, TOut>(ModSample sample, CPointer<uint8> sourceBuffer, size_t sourceSize, TConv conv = default) where TConv : struct, ISampleConversion<TOut> where TOut : unmanaged
		{
			size_t frameSize = 2 * TConv.Input_Inc;
			size_t countFrames = Math.Min(sourceSize / frameSize, sample.nLength);
			size_t numFrames = countFrames;

			TConv sampleConvLeft = conv;
			TConv sampleConvRight = conv;
			CPointer<byte> inBuf = Memory.Byte_Cast<byte>(sourceBuffer);
			CPointer<TOut> outBuf = sample.SampleEv().Cast<TOut>();

			while (numFrames-- != 0)
			{
				outBuf[0] = sampleConvLeft.Convert(inBuf);
				inBuf += TConv.Input_Inc;
				outBuf++;

				outBuf[0] = sampleConvRight.Convert(inBuf);
				inBuf += TConv.Input_Inc;
				outBuf++;
			}

			return frameSize * countFrames;
		}



		/********************************************************************/
		/// <summary>
		/// Copy a stereo split sample data buffer
		/// </summary>
		/********************************************************************/
		public static size_t CopyStereoSplitSample<TConv, TOut>(ModSample sample, CPointer<uint8> sourceBuffer, size_t sourceSize, TConv conv = default) where TConv : struct, ISampleConversion<TOut> where TOut : unmanaged
		{
			size_t sampleSize = TConv.Input_Inc;
			size_t sourceSizeLeft = Math.Min(sample.nLength * TConv.Input_Inc, sourceSize);
			size_t sourceSizeRight = Math.Min(sample.nLength * TConv.Input_Inc, sourceSize - sourceSizeLeft);
			size_t countSamplesLeft = sourceSizeLeft / sampleSize;
			size_t countSamplesRight = sourceSizeRight / sampleSize;

			size_t numSamplesLeft = countSamplesLeft;

			TConv sampleConvLeft = conv;
			CPointer<byte> inBufLeft = Memory.Byte_Cast<byte>(sourceBuffer);
			CPointer<TOut> outBufLeft = sample.SampleEv().Cast<TOut>();

			while (numSamplesLeft-- != 0)
			{
				outBufLeft[0] = sampleConvLeft.Convert(inBufLeft);
				inBufLeft += TConv.Input_Inc;
				outBufLeft += 2;
			}

			size_t numSamplesRight = countSamplesRight;

			TConv sampleConvRight = conv;
			CPointer<byte> inBufRight = Memory.Byte_Cast<byte>(sourceBuffer) + (sample.nLength * TConv.Input_Inc);
			CPointer<TOut> outBufRight = sample.SampleEv().Cast<TOut>() + 1;

			while (numSamplesRight-- != 0)
			{
				outBufRight[0] = sampleConvRight.Convert(inBufRight);
				inBufRight += TConv.Input_Inc;
				outBufRight += 2;
			}

			return (countSamplesLeft + countSamplesRight) * sampleSize;
		}
	}
}
