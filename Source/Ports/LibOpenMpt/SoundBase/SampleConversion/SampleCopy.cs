/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundBase.SampleConversion
{
	/// <summary>
	/// Functions for copying sample data
	/// </summary>
	internal static class SampleCopy
	{
		/********************************************************************/
		/// <summary>
		/// Copy a sample data buffer.
		/// targetBuffer: Buffer in which the sample should be copied into.
		/// numSamples: Number of samples of size T that should be copied.
		///             targetBuffer is expected to be able to hold
		///             "numSamples * incTarget" samples.
		/// incTarget: Number of samples by which the target data pointer is
		///            increased each time.
		/// sourceBuffer: Buffer from which the samples should be read.
		/// sourceSize: Size of source buffer, in bytes.
		/// incSource: Number of samples by which the source data pointer is
		///            increased each time.
		/// </summary>
		/********************************************************************/
		public static size_t CopySample<TConv, TOut>(CPointer<TOut> outBuf, size_t numSamples, size_t incTarget, CPointer<uint8> inBuf, size_t sourceSize, size_t incSource, TConv conv = default) where TConv : struct, ISampleConversion<TOut> where TOut : unmanaged
		{
			size_t sampleSize = incSource * TConv.Input_Inc * 1;
			OpenMpt.LimitMax(ref numSamples, sourceSize / sampleSize);

			size_t copySize = numSamples * sampleSize;

			TConv sampleConv = conv;

			while (numSamples-- != 0)
			{
				outBuf[0] = sampleConv.Convert(inBuf);
				outBuf += incTarget;
				inBuf += incSource * TConv.Input_Inc;
			}

			return copySize;
		}
	}
}
