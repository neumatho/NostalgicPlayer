/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
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
	internal class PcmFormat : RiffWaveSaverWorkerBase
	{
		// Loader variables
		private long fileSize;

		private byte[] decodeBuffer;
		private int sourceOffset;

		// Saver variables
		private byte[] saveBuffer;

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
		protected override int DecodeSampleData(ModuleStream moduleStream, int[] buffer, int length, LoadSampleFormatInfo formatInfo)
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
					samplesLeft = GetFileData(moduleStream, decodeBuffer, decodeBuffer.Length);
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
			return (uint)((saveFormat.Channels * saveFormat.Bits * saveFormat.Frequency + 7) / 8);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the block align
		/// </summary>
		/********************************************************************/
		protected override ushort GetBlockAlign()
		{
			return (ushort)((saveFormat.Channels * saveFormat.Bits + 7) / 8);
		}



		/********************************************************************/
		/// <summary>
		/// Writes a block of data
		/// </summary>
		/********************************************************************/
		protected override int WriteData(WriterStream stream, int[] buffer, int length)
		{
			if (saveFormat.Bits == 8)
			{
				// Do we need to reallocate the buffer?
				if ((saveBuffer == null) || (length > saveBuffer.Length))
				{
					// Allocate new buffer to store the converted samples into
					saveBuffer = new byte[length];
				}

				// Convert to unsigned 8-bit
				for (int i = 0; i < length; i++)
					saveBuffer[i] = (byte)((buffer[i] >> 24) + 128);

				// Write the data
				stream.Write(saveBuffer, 0, length);

				return length;
			}

			if (saveFormat.Bits == 16)
			{
				// Convert to signed 16-bit
				for (int i = 0; i < length; i++)
				{
					int sample = buffer[i];
					stream.Write_L_UINT16((ushort)(sample >> 16));
				}

				return length * 2;
			}

			if (saveFormat.Bits == 32)
			{
				// Convert to signed 32-bit
				for (int i = 0; i < length; i++)
				{
					int sample = buffer[i];
					stream.Write_L_UINT32((uint)sample);
				}

				return length * 4;
			}

			return 0;
		}
		#endregion
	}
}
