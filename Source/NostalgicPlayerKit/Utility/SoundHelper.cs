/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Utility
{
	/// <summary>
	/// Different helper methods for sound handling
	/// </summary>
	public static class SoundHelper
	{
		/********************************************************************/
		/// <summary>
		/// Split a pair sample buffer to individual buffers
		/// </summary>
		/********************************************************************/
		public static void SplitBuffer(int numberOfChannels, int[] inputBuffer, int[][] outputBuffers, int framesToCopy)
		{
			if (numberOfChannels == 1)
				Array.Copy(inputBuffer, outputBuffers[0], framesToCopy);
			else if (numberOfChannels == 2)
			{
				int[] leftBuffer = outputBuffers[0];
				int[] rightBuffer = outputBuffers[1];
				int inputOffset = 0;

				for (int j = 0; j < framesToCopy; j++)
				{
					leftBuffer[j] = inputBuffer[inputOffset++];
					rightBuffer[j] = inputBuffer[inputOffset++];
				}
			}
			else
			{
				for (int i = 0; i < numberOfChannels; i++)
				{
					int[] outBuffer = outputBuffers[i];

					for (int j = 0, inputOffset = 0; j < framesToCopy; j++, inputOffset += numberOfChannels)
						outBuffer[j] = inputBuffer[inputOffset];
				}
			}
		}
	}
}
