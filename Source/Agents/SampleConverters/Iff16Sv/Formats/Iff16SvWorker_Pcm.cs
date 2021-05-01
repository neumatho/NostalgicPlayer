/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Iff16Sv.Formats
{
	/// <summary>
	/// Reader/writer of IFF-16SV PCM format
	/// </summary>
	internal class Iff16SvWorker_Pcm : Iff16SvSaverWorkerBase
	{
		// Loader variables
		private long fileSize;

		private byte[] decodeBuffer;
		private int sourceOffset;

		// Saver variables
		private byte[] saveBuffer;

		#region Iff16SvWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the format ID
		/// </summary>
		/********************************************************************/
		protected override Format FormatId => Format.Pcm;
		#endregion

		#region ISampleLoaderAgent implementation
		/********************************************************************/
		/// <summary>
		/// Calculates how many samples that will be returned
		/// </summary>
		/********************************************************************/
		public override long GetTotalSampleLength(LoadSampleFormatInfo formatInfo)
		{
			return fileSize / 2;
		}
		#endregion

		#region Iff16SvLoaderWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Initialize the loader
		/// </summary>
		/********************************************************************/
		protected override void LoaderInitialize(long dataLength)
		{
			decodeBuffer = new byte[DecodeBufferSize];

			// Initialize member variables
			fileSize = dataLength;
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
		protected override int DecodeSampleData(ModuleStream moduleStream, ModuleStream moduleStream2, int[] buffer, int length)
		{
			int filled = 0;

			int bufSize = moduleStream2 == null ? decodeBuffer.Length : decodeBuffer.Length / 2;

			while (length > 0)
			{
				// Do we need to load some data from the file?
				if (samplesLeft == 0)
				{
					// Yes, do it
					if (moduleStream2 != null)
					{
						samplesLeft = GetFileData2(moduleStream2, decodeBuffer, bufSize, bufSize);

						if (samplesLeft == 0)
							break;			// End of file, stop filling

						samplesLeft += GetFileData1(moduleStream, decodeBuffer, 0, samplesLeft);
					}
					else
					{
						samplesLeft = GetFileData1(moduleStream, decodeBuffer, 0, bufSize);

						if (samplesLeft == 0)
							break;			// End of file, stop filling
					}

					sourceOffset = 0;
				}

				// Copy the sample data
				int todo;

				if (moduleStream2 == null)
				{
					// Mono sample
					//
					// Find the number of samples to return
					todo = Math.Min(length, samplesLeft / 2);

					for (int i = 0; i < todo; i++)
					{
						buffer[filled + i] = (decodeBuffer[sourceOffset] << 24) | (decodeBuffer[sourceOffset + 1] << 16);
						sourceOffset += 2;
					}
				}
				else
				{
					// Stereo sample
					//
					// Find the number of samples to return
					todo = Math.Min(length, samplesLeft / 2) / 2;

					for (int i = 0, j = 0; i < todo; i++, j += 2)
					{
						buffer[filled + j] = (decodeBuffer[sourceOffset] << 24) | (decodeBuffer[sourceOffset + 1] << 16);
						buffer[filled + j + 1] = (decodeBuffer[bufSize + sourceOffset] << 24) | (decodeBuffer[bufSize + sourceOffset + 1] << 16);

						sourceOffset += 2;
					}

					todo *= 2;
				}

				// Update the counter variables
				length -= todo;
				filled += todo;
				samplesLeft -= todo * 2;
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
			if (formatInfo.Channels == 2)
				return position / 2 * 2;

			return position * 2;
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the number of samples from the byte position given
		/// </summary>
		/********************************************************************/
		protected override long CalcSamplePosition(long position, LoadSampleFormatInfo formatInfo)
		{
			return position;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the position where the right channel starts relative to
		/// the left channel
		/// </summary>
		/********************************************************************/
		protected override long GetRightChannelPosition(long totalLength)
		{
			return totalLength / 2;
		}
		#endregion

		#region ISampleSaverAgent implementation
		/********************************************************************/
		/// <summary>
		/// Return some flags telling what the saver supports
		/// </summary>
		/********************************************************************/
		public override SampleSaverSupportFlag SaverSupportFlags => SampleSaverSupportFlag.Support16Bit | SampleSaverSupportFlag.SupportMono | SampleSaverSupportFlag.SupportStereo;
		#endregion

		#region Iff16SvSaverWorkerBase implementation
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
		/// Writes a block of data
		/// </summary>
		/********************************************************************/
		protected override uint WriteData(WriterStream stream, Stream stereoStream, int[] buffer, int length)
		{
			if (saveFormat.Channels == 2)
			{
				// Stereo sample
				//
				// Split the data up in two parts
				byte[] newBuf1 = new byte[length];
				byte[] newBuf2 = new byte[length];

				for (int i = 0, j = 0; i < length; i += 2, j += 2)
				{
					newBuf1[i] = (byte)(buffer[j] >> 24);
					newBuf1[i + 1] = (byte)(buffer[j] >> 16);
					newBuf2[i] = (byte)(buffer[j + 1] >> 24);
					newBuf2[i + 1] = (byte)(buffer[j + 1] >> 16);
				}

				// Write the data in the two files
				stream.Write(newBuf1, 0, length);
				stereoStream.Write(newBuf2, 0, length);

				return (uint)length / 2;
			}

			// Mono sample
			//
			// Do we need to reallocate the buffer?
			length *= 2;

			if ((saveBuffer == null) || (length > saveBuffer.Length))
			{
				// Allocate new buffer to store the converted samples into
				saveBuffer = new byte[length];
			}

			// Convert to 16-bit
			for (int i = 0, j = 0; i < length; i += 2, j++)
			{
				saveBuffer[i] = (byte)(buffer[j] >> 24);
				saveBuffer[i + 1] = (byte)(buffer[j] >> 16);
			}

			// Write the data
			stream.Write(saveBuffer, 0, length);

			return (uint)length / 2;
		}
		#endregion
	}
}
