/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Iff8Svx.Formats
{
	/// <summary>
	/// Reader/writer of IFF-8SVX PCM format
	/// </summary>
	internal class PcmFormat : Iff8SvxSaverWorkerBase
	{
		// Loader variables
		private long fileSize;

		private byte[] decodeBuffer;
		private int sourceOffset;

		// Saver variables
		private byte[] saveBuffer;

		#region Iff8SvxWorkerBase implementation
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
			return fileSize;
		}
		#endregion

		#region Iff8SvxLoaderWorkerBase implementation
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
					todo = Math.Min(length, samplesLeft);

					for (int i = 0; i < todo; i++)
						buffer[filled + i] = decodeBuffer[sourceOffset++] << 24;
				}
				else
				{
					// Stereo sample
					//
					// Find the number of samples to return
					todo = Math.Min(length, samplesLeft);

					for (int i = 0; i < todo; i += 2)
					{
						buffer[filled + i] = decodeBuffer[sourceOffset] << 24;
						buffer[filled + i + 1] = decodeBuffer[bufSize + sourceOffset++] << 24;
					}
				}

				// Update the counter variables
				length -= todo;
				filled += todo;
				samplesLeft -= todo;
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
				return position / 2;

			return position;
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
		public override SampleSaverSupportFlag SaverSupportFlags => SampleSaverSupportFlag.Support8Bit | SampleSaverSupportFlag.SupportMono | SampleSaverSupportFlag.SupportStereo;
		#endregion

		#region Iff8SvxSaverWorkerBase implementation
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
				int count = length / 2;

				byte[] newBuf1 = new byte[count];
				byte[] newBuf2 = new byte[count];

				for (int i = 0, j = 0; i < count; i++, j += 2)
				{
					newBuf1[i] = (byte)(buffer[j] >> 24);
					newBuf2[i] = (byte)(buffer[j + 1] >> 24);
				}

				// Write the data in the two files
				stream.Write(newBuf1, 0, count);
				stereoStream.Write(newBuf2, 0, count);

				return (uint)count;
			}

			// Mono sample
			//
			// Do we need to reallocate the buffer?
			if ((saveBuffer == null) || (length > saveBuffer.Length))
			{
				// Allocate new buffer to store the converted samples into
				saveBuffer = new byte[length];
			}

			// Convert to 8-bit
			for (int i = 0; i < length; i++)
				saveBuffer[i] = (byte)(buffer[i] >> 24);

			// Write the data
			stream.Write(saveBuffer, 0, length);

			return (uint)length;
		}
		#endregion
	}
}
