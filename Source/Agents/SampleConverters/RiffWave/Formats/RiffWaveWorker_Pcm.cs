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
	/// Reader/writer of RIFF-WAVE PCM format
	/// </summary>
	internal class RiffWaveWorker_Pcm : RiffWaveSaverWorkerBase
	{
		private long fileSize;

		private byte[] decodeBuffer;
		private int sourceOffset;

		#region RiffWaveWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the wave format ID
		/// </summary>
		/********************************************************************/
		protected override WaveFormat FormatId => WaveFormat.WAVE_FORMAT_PCM;
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
		protected override int DecodeSampleData(ModuleStream stream, int[] buffer, int length, LoadSampleFormatInfo formatInfo)
		{
			// Calculate the number of bytes used for each sample
			int sampleSize = (formatInfo.Bits + 7) / 8;
			int shift = sampleSize * 8 - formatInfo.Bits;
			int filled = 0;

			while (length > 0)
			{
				// Do we need to load some data from the stream?
				if (samplesLeft == 0)
				{
					// Yes, do it
					samplesLeft = GetFileData(stream, decodeBuffer, decodeBuffer.Length);
					sourceOffset = 0;

					if (samplesLeft == 0)
						break;			// End of file, stop filling
				}

				// Find the number of samples to return
				int todo = Math.Min(length, samplesLeft / sampleSize);

				// Copy the sample data
				switch (sampleSize)
				{
					// 1-8 bits samples
					case 1:
					{
						for (int i = 0; i < todo; i++)
							buffer[filled + i] = ((decodeBuffer[sourceOffset++] >> shift) - 128) << 24;

						break;
					}

					// 9-16 bits samples
					case 2:
					{
						for (int i = 0; i < todo; i++)
						{
							buffer[filled + i] = ((((decodeBuffer[sourceOffset + 1] & 0xff) << 8) | (decodeBuffer[sourceOffset] & 0xff)) >> shift) << 16;
							sourceOffset += 2;
						}
						break;
					}

					// 17-24 bits samples
					case 3:
					{
						for (int i = 0; i < todo; i++)
						{
							buffer[filled + i] = ((((decodeBuffer[sourceOffset + 2] & 0xff) << 16) | ((decodeBuffer[sourceOffset + 1] & 0xff) << 8) | (decodeBuffer[sourceOffset] & 0xff)) >> shift) << 8;
							sourceOffset += 3;
						}
						break;
					}

					// 25-32 bits samples
					case 4:
					{
						for (int i = 0; i < todo; i++)
						{
							buffer[filled + i] = (((decodeBuffer[sourceOffset + 3] & 0xff) << 24) | ((decodeBuffer[sourceOffset + 2] & 0xff) << 16) | ((decodeBuffer[sourceOffset + 1] & 0xff) << 8) | (decodeBuffer[sourceOffset] & 0xff)) >> shift;
							sourceOffset += 4;
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
		public override SampleSaverSupportFlag SaverSupportFlags => SampleSaverSupportFlag.Support8Bit | SampleSaverSupportFlag.Support16Bit | SampleSaverSupportFlag.Support32Bit | SampleSaverSupportFlag.SupportMono | SampleSaverSupportFlag.SupportStereo;
		#endregion

		#region RiffWaveSaverWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Returns the average bytes per second
		/// </summary>
		/********************************************************************/
		protected override uint GetAverageBytesPerSecond()
		{
			return (uint)((format.Channels * format.Bits * format.Frequency + 7) / 8);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the block align
		/// </summary>
		/********************************************************************/
		protected override ushort GetBlockAlign()
		{
			return (ushort)((format.Channels * format.Bits + 7) / 8);
		}



		/********************************************************************/
		/// <summary>
		/// Writes any extra information into the fmt chunk
		/// </summary>
		/********************************************************************/
		protected override void WriteExtraFmtInfo(WriterStream writerStream)
		{
			writerStream.Write_L_UINT16(GetSampleSize(format.Bits));		// Sample size
		}



		/********************************************************************/
		/// <summary>
		/// Writes a block of data
		/// </summary>
		/********************************************************************/
		protected override int WriteData(int[] buffer, int length, byte[] outputBuffer)
		{
			if (format.Bits == 8)
			{
				// Convert to unsigned 8-bit
				for (int i = 0; i < length; i++)
					outputBuffer[i] = (byte)((buffer[i] >> 24) + 128);

				return length;
			}

			if (format.Bits == 16)
			{
				// Convert to signed 16-bit
				for (int i = 0, j = 0; i < length; i++, j += 2)
				{
					int sample = buffer[i];
					byte[] shortArray = BitConverter.GetBytes((short)(sample >> 16));

					outputBuffer[j] = shortArray[0];
					outputBuffer[j + 1] = shortArray[1];
				}

				return length * 2;
			}

			if (format.Bits == 32)
			{
				// Convert to signed 32-bit
				for (int i = 0, j = 0; i < length; i++, j += 4)
				{
					int sample = buffer[i];
					byte[] intArray = BitConverter.GetBytes(sample);

					outputBuffer[j] = intArray[0];
					outputBuffer[j + 1] = intArray[1];
					outputBuffer[j + 2] = intArray[2];
					outputBuffer[j + 3] = intArray[3];
				}

				return length * 4;
			}

			return 0;
		}
		#endregion
	}
}
