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
using System.Text;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.AudioIff
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class AudioIffWorker : ISampleLoaderAgent, ISampleSaverAgent
	{
		// Loader variables
		private byte[] loadBuffer;
		private int samplesLeft;
		private uint sourceOffset;

		private long ssndStart;
		private uint ssndLength;

		private const int LoadBufferSize = 32768;

		// Saver variables
		private SaveSampleFormatInfo saveFormat;

		private int total;

		private long commPosition;
		private long ssndPosition;

		#region ISampleLoaderAgent implementation
		/********************************************************************/
		/// <summary>
		/// Return the file extensions that is supported by the loader
		/// </summary>
		/********************************************************************/
		public string[] FileExtensions => new [] { "aiff", "aif" };



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public AgentResult Identify(ModuleStream moduleStream)
		{
			// Seek to the start of the file
			moduleStream.Seek(0, SeekOrigin.Begin);

			// Check the chunk names
			if (moduleStream.Read_B_UINT32() == 0x464f524d)			// FORM
			{
				moduleStream.Seek(4, SeekOrigin.Current);		// Skip length
				if (moduleStream.Read_B_UINT32() == 0x41494646)		// AIFF
					return AgentResult.Ok;
			}

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the description and value on the line given. If the line
		/// is out of range, false is returned
		/// </summary>
		/********************************************************************/
		public bool GetInformationString(int line, out string description, out string value)
		{
			description = null;
			value = null;

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the loader
		/// </summary>
		/********************************************************************/
		public bool InitLoader(out string errorMessage)
		{
			errorMessage = string.Empty;

			loadBuffer = new byte[LoadBufferSize];
			samplesLeft = 0;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the loader
		/// </summary>
		/********************************************************************/
		public void CleanupLoader()
		{
			loadBuffer = null;
		}



		/********************************************************************/
		/// <summary>
		/// Load the sample header
		/// </summary>
		/********************************************************************/
		public bool LoadHeader(ModuleStream moduleStream, out LoadSampleFormatInfo formatInfo, out string errorMessage)
		{
			errorMessage = string.Empty;

			// Initialize the format structure
			formatInfo = new LoadSampleFormatInfo();

			// Skip the header
			moduleStream.Seek(12, SeekOrigin.Begin);

			// Set some flags
			bool gotComm = false;
			bool gotSsnd = false;

			for (;;)
			{
				// Read the chunk name and size
				uint chunkName = moduleStream.Read_B_UINT32();
				uint chunkSize = moduleStream.Read_B_UINT32();

				// Check if we reached the end of the file
				if (moduleStream.EndOfStream)
					break;

				// Interpret the known chunks
				switch (chunkName)
				{
					// 'COMM': Common chunk
					case 0x434f4d4d:
					{
						byte[] freq = new byte[10];

						// Begin to read the chunk data
						formatInfo.Channels = moduleStream.Read_B_UINT16();		// Number of channels
						moduleStream.Seek(4, SeekOrigin.Current);			// Skip sample frames
						formatInfo.Bits = moduleStream.Read_B_UINT16();			// Sample size

						moduleStream.Read(freq, 0, 10);				// Extended sample rate
						formatInfo.Frequency = (int)IeeeExtended.ConvertFromIeeeExtended(freq);

						// Skip any extra data
						moduleStream.Seek((chunkSize - 18 + 1) & 0xfffffffe, SeekOrigin.Current);

						gotComm = true;
						break;
					}

					// 'SSND': Sound data chunk
					case 0x53534e44:
					{
						ssndLength = chunkSize;
						ssndStart = moduleStream.Position + 8;

						// Skip any extra data
						moduleStream.Seek((chunkSize + 1) & 0xfffffffe, SeekOrigin.Current);

						gotSsnd = true;
						break;
					}

					// Unknown chunks
					default:
					{
						bool ok = true;
						for (int i = 0; i < 4; i++)
						{
							byte byt = (byte)(chunkName & 0xff);
							if ((byt < 32) || (byt > 127))
							{
								ok = false;
								break;
							}

							chunkName >>= 8;
						}

						if (ok)
							moduleStream.Seek((chunkSize + 1) & 0xfffffffe, SeekOrigin.Current);
						else
							moduleStream.Seek(0, SeekOrigin.End);

						break;
					}
				}
			}

			// Check if we get the chunks needed
			if (!gotComm)
			{
				errorMessage = Resources.IDS_AUDIOIFF_ERR_NOCOMM;
				return false;
			}

			if (!gotSsnd)
			{
				errorMessage = Resources.IDS_AUDIOIFF_ERR_NOSSND;
				return false;
			}

			// Is the number of channels one we support
			if (formatInfo.Channels > 2)
			{
				errorMessage = string.Format(Resources.IDS_AUDIOIFF_ERR_ILLEGALCHANNEL, formatInfo.Channels);
				return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load some part of the sample data
		/// </summary>
		/********************************************************************/
		public int LoadData(ModuleStream moduleStream, int[] buffer, int length, LoadSampleFormatInfo formatInfo)
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
					samplesLeft = moduleStream.Read(loadBuffer, 0, loadBuffer.Length);
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
							buffer[filled + i] = (loadBuffer[sourceOffset++] >> shift) << 24;

						break;
					}

					// 9-16 bits samples
					case 2:
					{
						for (int i = 0; i < todo; i++)
						{
							buffer[filled + i] = ((((loadBuffer[sourceOffset] & 0xff) << 8) | (loadBuffer[sourceOffset + 1] & 0xff)) >> shift) << 16;
							sourceOffset += 2;
						}
						break;
					}

					// 17-24 bits samples
					case 3:
					{
						for (int i = 0; i < todo; i++)
						{
							buffer[filled + i] = ((((loadBuffer[sourceOffset] & 0xff) << 16) | ((loadBuffer[sourceOffset + 1] & 0xff) << 8) | (loadBuffer[sourceOffset + 2] & 0xff)) >> shift) << 8;
							sourceOffset += 3;
						}
						break;
					}

					// 25-32 bits samples
					case 4:
					{
						for (int i = 0; i < todo; i++)
						{
							buffer[filled + i] = (((loadBuffer[sourceOffset] & 0xff) << 24) | ((loadBuffer[sourceOffset + 1] & 0xff) << 16) | ((loadBuffer[sourceOffset + 2] & 0xff) << 8) | (loadBuffer[sourceOffset + 3] & 0xff)) >> shift;
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
		/// Calculates how many samples that will be returned
		/// </summary>
		/********************************************************************/
		public long GetTotalSampleLength(LoadSampleFormatInfo formatInfo)
		{
			return (ssndLength - 8) / ((formatInfo.Bits + 7) / 8);
		}



		/********************************************************************/
		/// <summary>
		/// Sets the file position to the sample position given
		/// </summary>
		/********************************************************************/
		public long SetSamplePosition(ModuleStream moduleStream, long position, LoadSampleFormatInfo formatInfo)
		{
			// Calculate the position in bytes
			long newPosition = position * ((formatInfo.Bits + 7) / 8);

			// Seek to the right position in the SSND chunk
			moduleStream.Seek(ssndStart + newPosition, SeekOrigin.Begin);

			return position;
		}
		#endregion

		#region ISampleSaverAgent implementation
		/********************************************************************/
		/// <summary>
		/// Return some flags telling what the saver supports
		/// </summary>
		/********************************************************************/
		public SampleSaverSupportFlag SaverSupportFlags => SampleSaverSupportFlag.Support8Bit | SampleSaverSupportFlag.Support16Bit | SampleSaverSupportFlag.Support32Bit | SampleSaverSupportFlag.SupportMono | SampleSaverSupportFlag.SupportStereo;



		/********************************************************************/
		/// <summary>
		/// Return the file extension that is used by the saver
		/// </summary>
		/********************************************************************/
		public string FileExtension => "aiff";



		/********************************************************************/
		/// <summary>
		/// Initialize the saver so it is prepared to save the sample
		/// </summary>
		/********************************************************************/
		public virtual bool InitSaver(SaveSampleFormatInfo formatInfo, out string errorMessage)
		{
			errorMessage = string.Empty;

			// Remember arguments
			saveFormat = formatInfo;

			// Initialize variables
			total = 0;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the saver
		/// </summary>
		/********************************************************************/
		public virtual void CleanupSaver()
		{
			saveFormat = null;
		}



		/********************************************************************/
		/// <summary>
		/// Save the header of the sample
		/// </summary>
		/********************************************************************/
		public void SaveHeader(Stream stream)
		{
			using (WriterStream writerStream = new WriterStream(stream))
			{
				Encoding encoder = EncoderCollection.Macintosh;

				// Write the IFF header
				writerStream.Write_B_UINT32(0x464f524d);						// FORM
				writerStream.Write_B_UINT32(0);									// File size
				writerStream.Write_B_UINT32(0x41494646);						// AIFF

				// Write the annotation chunk
				byte[] strBuf = encoder.GetBytes(Resources.IDS_AUDIOIFF_ANNO);
				int strLen = strBuf.Length;

				writerStream.Write_B_UINT32(0x414e4e4f);						// ANNO
				writerStream.Write_B_UINT32((uint)strLen);							// Chunk size
				writerStream.Write(strBuf, 0, strLen);

				if ((strLen % 2) != 0)
					writerStream.Write_UINT8(0);

				// Write the common chunk
				commPosition = writerStream.Position;								// Remember the position of the common chunk

				writerStream.Write_B_UINT32(0x434f4d4d);						// COMM
				writerStream.Write_B_UINT32(18);								// Chunk size
				writerStream.Write_B_UINT16((ushort)saveFormat.Channels);			// Number of channels
				writerStream.Write_B_UINT32(0);									// Sample frames
				writerStream.Write_B_UINT16((ushort)saveFormat.Bits);				// Sample size

				byte[] freq = IeeeExtended.ConvertToIeeeExtended(saveFormat.Frequency);
				writerStream.Write(freq, 0, freq.Length);

				// Write the sound data chunk
				ssndPosition = (uint)writerStream.Position;							// Remember the position of the sound chunk

				writerStream.Write_B_UINT32(0x53534e44);						// SSND
				writerStream.Write_B_UINT32(0);									// Chunk size
				writerStream.Write_B_UINT32(0);									// Offset
				writerStream.Write_B_UINT32(0);									// Block size
			}
		}



		/********************************************************************/
		/// <summary>
		/// Save a part of the sample
		/// </summary>
		/********************************************************************/
		public void SaveData(Stream stream, int[] buffer, int length)
		{
			if (length > 0)
			{
				using (WriterStream writerStream = new WriterStream(stream))
				{
					if (saveFormat.Bits == 8)
					{
						// Convert to signed 8-bit
						for (int i = 0; i < length; i++)
						{
							int sample = buffer[i];
							writerStream.Write_INT8((sbyte)(sample >> 24));
						}

						total += length;
					}
					else if (saveFormat.Bits == 16)
					{
						// Convert to signed 16-bit
						for (int i = 0; i < length; i++)
						{
							int sample = buffer[i];
							writerStream.Write_B_UINT16((ushort)(sample >> 16));
						}

						total += length * 2;
					}
					else if (saveFormat.Bits == 32)
					{
						// Convert to signed 32-bit
						for (int i = 0; i < length; i++)
						{
							int sample = buffer[i];
							writerStream.Write_B_UINT32((uint)sample);
						}

						total += length * 4;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Save the tail of the sample
		/// </summary>
		/********************************************************************/
		public void SaveTail(Stream stream)
		{
			using (WriterStream writerStream = new WriterStream(stream))
			{
				// Change the SSND chunk size
				writerStream.Seek(ssndPosition + 4, SeekOrigin.Begin);
				writerStream.Write_B_UINT32((uint)total + 8);		// Add offset + block size to the total size

				// Change the sample frames in the COMM chunk
				uint sampleFrame = (uint)total;
				if (saveFormat.Bits == 16)
					sampleFrame /= 2;
				else if (saveFormat.Bits == 32)
					sampleFrame /= 4;

				if (saveFormat.Channels == 2)
					sampleFrame /= 2;

				writerStream.Seek(commPosition + 10, SeekOrigin.Begin);
				writerStream.Write_B_UINT32(sampleFrame);

				// Change the FORM size
				writerStream.Seek(4, SeekOrigin.Begin);
				writerStream.Write_B_UINT32((uint)(ssndPosition + (8 + 8 + total) - 8));
			}
		}
		#endregion
	}
}
