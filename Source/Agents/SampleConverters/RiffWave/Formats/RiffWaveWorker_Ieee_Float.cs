/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.RiffWave.Formats
{
	/// <summary>
	/// Reader/writer of RIFF-WAVE IEEE Float format
	/// </summary>
	internal class RiffWaveWorker_Ieee_Float : RiffWaveSaverWorkerBase
	{
		// Loader variables
		private long fileSize;

		private byte[] decodeBuffer;
		private int sourceOffset;

		// Saver variables
		private byte[] saveBuffer;
		private float normalize;

		#region RiffWaveWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the wave format ID
		/// </summary>
		/********************************************************************/
		protected override WaveFormat FormatId => WaveFormat.WAVE_FORMAT_IEEE_FLOAT;
		#endregion

		#region ISampleLoaderAgent implementation
		/********************************************************************/
		/// <summary>
		/// Calculates how many samples that will be returned
		/// </summary>
		/********************************************************************/
		public override long GetTotalSampleLength(LoadSampleFormatInfo formatInfo)
		{
			return fileSize / ((formatInfo.Bits + 7) / 8);
		}
		#endregion

		#region RiffWaveLoaderWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Tells whether the fact chunk is required or not
		/// </summary>
		/********************************************************************/
		protected override bool NeedFact => false;




		/********************************************************************/
		/// <summary>
		/// Initialize the loader
		/// </summary>
		/********************************************************************/
		protected override void LoaderInitialize(long dataStart, long dataLength, long totalFileSize, LoadSampleFormatInfo formatInfo)
		{
			// Allocate buffer to hold the loaded data
			int sampleSize = (formatInfo.Bits + 7) / 8;
			decodeBuffer = new byte[DecodeBufferSize * sampleSize];

			// Initialize member variables
			fileSize = Math.Min(dataLength, totalFileSize - dataStart);

			normalize = 1.0f;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the loader
		/// </summary>
		/********************************************************************/
		protected override void LoaderCleanup()
		{
			decodeBuffer = null;
		}



		/********************************************************************/
		/// <summary>
		/// Load and decode a block of sample data
		/// </summary>
		/********************************************************************/
		protected override int DecodeSampleData(ModuleStream moduleStream, int[] buffer, int length, LoadSampleFormatInfo formatInfo)
		{
			// Calculate the number of bytes used for each sample
			int sampleSize = (formatInfo.Bits + 7) / 8;
			int filled = 0;

			while (length > 0)
			{
				// Do we need to load some data from the stream?
				if (samplesLeft == 0)
				{
					// Yes, do it
					samplesLeft = GetFileData(moduleStream, decodeBuffer, decodeBuffer.Length);
					sourceOffset = 0;

					if (samplesLeft == 0)
						break;			// End of file, stop filling
				}

				// Find the number of samples to return
				int todo = Math.Min(length, samplesLeft / sampleSize);
				if (todo == 0)
					break;

				// Copy the sample data
				switch (sampleSize)
				{
					case 4:
					{
						for (int i = 0; i < todo; i++)
						{
							float f = BitConverter.ToSingle(decodeBuffer, sourceOffset);
							if ((f > 1.0f) || (f < -1.0f))
								normalize = 0.9f / Math.Abs(f);

							buffer[filled + i] = (int)(f * normalize * 2147483647.0f);
							sourceOffset += 4;
						}
						break;
					}

					case 8:
					{
						for (int i = 0; i < todo; i++)
						{
							double d = BitConverter.ToDouble(decodeBuffer, sourceOffset);
							if ((d > 1.0f) || (d < -1.0f))
								normalize = (float)(0.9f / Math.Abs(d));

							buffer[filled + i] = (int)(d * normalize * 2147483647.0f);
							sourceOffset += 8;
						}
						break;
					}
				}

				// Update the counter variables
				length -= todo;
				filled += todo;
				samplesLeft -= (todo * sampleSize);
			}

			return filled;
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the number of bytes to go into the file to reach the
		/// position given
		/// </summary>
		/********************************************************************/
		protected override long CalcFilePosition(long position, LoadSampleFormatInfo formatInfo)
		{
			return position * ((formatInfo.Bits + 7) / 8);
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the number of samples from the byte position given
		/// </summary>
		/********************************************************************/
		protected override long CalcSamplePosition(long position, LoadSampleFormatInfo formatInfo)
		{
			return position / ((formatInfo.Bits + 7) / 8);
		}
		#endregion

		#region ISampleSaverAgent implementation
		/********************************************************************/
		/// <summary>
		/// Return some flags telling what the saver supports
		/// </summary>
		/********************************************************************/
		public override SampleSaverSupportFlag SaverSupportFlags => SampleSaverSupportFlag.Support32Bit | SampleSaverSupportFlag.SupportMono | SampleSaverSupportFlag.SupportStereo;
		#endregion

		#region RiffWaveSaverWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Cleanup the saver
		/// </summary>
		/********************************************************************/
		public override void CleanupSaver()
		{
			saveBuffer = null;

			base.CleanupSaver();
		}



		/********************************************************************/
		/// <summary>
		/// Returns the average bytes per second
		/// </summary>
		/********************************************************************/
		protected override uint GetAverageBytesPerSecond()
		{
			return (uint)((saveFormat.Channels * 32 * saveFormat.Frequency + 7) / 8);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the block align
		/// </summary>
		/********************************************************************/
		protected override ushort GetBlockAlign()
		{
			return (ushort)((saveFormat.Channels * 32 + 7) / 8);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the bit size of each sample
		/// </summary>
		/********************************************************************/
		protected override ushort GetSampleSize(int sampleSize)
		{
			return 32;
		}



		/********************************************************************/
		/// <summary>
		/// Writes a block of data
		/// </summary>
		/********************************************************************/
		protected override int WriteData(WriterStream stream, int[] buffer, int length)
		{
			// Do we need to reallocate the buffer?
			if ((saveBuffer == null) || (length > saveBuffer.Length))
			{
				// Allocate new buffer to store the converted samples into
				saveBuffer = new byte[length * (GetSampleSize(saveFormat.Bits) / 8)];
			}

			// Convert to 32-bit float
			for (int i = 0, j = 0; i < length; i++, j += 4)
			{
				float sample = buffer[i] / 2147483647.0f;
				byte[] floatArray = BitConverter.GetBytes(sample);

				saveBuffer[j] = floatArray[0];
				saveBuffer[j + 1] = floatArray[1];
				saveBuffer[j + 2] = floatArray[2];
				saveBuffer[j + 3] = floatArray[3];
			}

			// Write the data
			stream.Write(saveBuffer, 0, length * 4);

			return length * 4;
		}
		#endregion
	}
}
