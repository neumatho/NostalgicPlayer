/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.RiffWave
{
	/// <summary>
	/// Derive from this class, if your format supports loading
	/// </summary>
	internal abstract class RiffWaveLoaderWorkerBase : RiffWaveWorkerBase, ISampleLoaderAgent
	{
		/// <summary>
		/// Average bytes per second
		/// </summary>
		protected uint bytesPerSecond;

		/// <summary>
		/// Block align
		/// </summary>
		protected ushort blockAlign;

		private long dataStarts;

		private byte[] loadBuffer;
		private int loadBufferFillCount;
		private int loadBufferOffset;
		protected int samplesLeft;

		protected const int LoadBufferSize = 32768;
		protected const int DecodeBufferSize = 16384;

		#region ISampleLoaderAgent implementation
		/********************************************************************/
		/// <summary>
		/// Return the file extensions that is supported by the loader
		/// </summary>
		/********************************************************************/
		public string[] FileExtensions => new [] { "wav" };



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
			if (moduleStream.Read_B_UINT32() == 0x52494646)			// RIFF
			{
				moduleStream.Seek(4, SeekOrigin.Current);		// Skip length
				if (moduleStream.Read_B_UINT32() == 0x57415645)		// WAVE
				{
					// See if we can find the 'fmt ' chunk
					for (;;)
					{
						// Read the chunk name and size
						uint chunkName = moduleStream.Read_B_UINT32();
						uint chunkSize = moduleStream.Read_L_UINT32();

						// Check if we reached the end of the file
						if (moduleStream.EndOfStream)
							return AgentResult.Unknown;

						if (chunkName == 0x666d7420)				// fmt 
						{
							// Got it, check the format
							WaveFormat format = (WaveFormat)moduleStream.Read_L_UINT16();
							if (format == WaveFormat.WAVE_FORMAT_EXTENSIBLE)
							{
								// Check for extended format
								moduleStream.Seek(22, SeekOrigin.Current);

								Guid formatGuid = moduleStream.ReadGuid();
								if (formatGuid == new Guid((int)FormatId, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71))
									return AgentResult.Ok;
							}
							else
							{
								if (format == FormatId)
									return AgentResult.Ok;
							}

							// Not a known format :-(
							return AgentResult.Unknown;
						}

						// Skip the chunk
						moduleStream.Seek((chunkSize + 1) & 0xfffffffe, SeekOrigin.Current);
					}
				}
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
		public virtual bool InitLoader(out string errorMessage)
		{
			errorMessage = string.Empty;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the loader
		/// </summary>
		/********************************************************************/
		public virtual void CleanupLoader()
		{
			// Call cleanup methods
			LoaderCleanup();
			CleanupBasicLoader();
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
			bool gotFmt = false;
			bool gotData = false;
			bool gotFact = false;

			for (;;)
			{
				// Read the chunk name and size
				uint chunkName = moduleStream.Read_B_UINT32();
				uint chunkSize = moduleStream.Read_L_UINT32();

				// Check if we reached the end of the file
				if (moduleStream.EndOfStream)
					break;

				// Interpret the known chunks
				switch (chunkName)
				{
					// 'fmt ': Format chunk
					case 0x666d7420:
					{
						// Begin to read the chunk data
						moduleStream.Seek(2, SeekOrigin.Current);				// Skip sample format (we know what it is)
						formatInfo.Channels = moduleStream.Read_L_UINT16();			// Number of channels
						formatInfo.Frequency = (int)moduleStream.Read_L_UINT32();	// Sample rate
						bytesPerSecond = moduleStream.Read_L_UINT32();				// Average bytes per second
						blockAlign = moduleStream.Read_L_UINT16();					// Block align
						formatInfo.Bits = moduleStream.Read_L_UINT16();				// Sample size

						// Initialize the loader
						InitBasicLoader();

						// Read extra header information
						int extraData = LoadExtraHeaderInfo(moduleStream, formatInfo, out errorMessage);
						if (!string.IsNullOrEmpty(errorMessage))
							return false;

						// Skip any extra data
						moduleStream.Seek((chunkSize - 16 - extraData + 1) & 0xfffffffe, SeekOrigin.Current);

						gotFmt = true;
						break;
					}

					// 'fact': Fact chunk
					case 0x66616374:
					{
						// Load the fact chunk
						int factSize = LoadFactChunk(moduleStream);

						// Skip any extra data
						moduleStream.Seek((chunkSize - factSize + 1) & 0xfffffffe, SeekOrigin.Current);

						gotFact = true;
						break;
					}

					// 'data': Data chunk
					case 0x64617461:
					{
						// Remember the position where the data starts
						dataStarts = moduleStream.Position;

						// Initialize the loader
						LoaderInitialize(dataStarts, chunkSize, moduleStream.Length, formatInfo);

						// Skip any extra data
						moduleStream.Seek((chunkSize + 1) & 0xfffffffe, SeekOrigin.Current);

						gotData = true;
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
			if (!gotFmt)
			{
				errorMessage = Resources.IDS_RIFFWAVE_ERR_NOFMT;
				return false;
			}

			if (!gotFact && NeedFact)
			{
				errorMessage = Resources.IDS_RIFFWAVE_ERR_NOFACT;
				return false;
			}

			if (!gotData)
			{
				errorMessage = Resources.IDS_RIFFWAVE_ERR_NODATA;
				return false;
			}

			// Is the number of channels one we support
			if (formatInfo.Channels > 2)
			{
				errorMessage = string.Format(Resources.IDS_RIFFWAVE_ERR_ILLEGALCHANNEL, formatInfo.Channels);
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
			return DecodeSampleData(moduleStream, buffer, length, formatInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Calculates how many samples that will be returned
		/// </summary>
		/********************************************************************/
		public abstract long GetTotalSampleLength(LoadSampleFormatInfo formatInfo);



		/********************************************************************/
		/// <summary>
		/// Sets the file position to the sample position given
		/// </summary>
		/********************************************************************/
		public long SetSamplePosition(ModuleStream moduleStream, long position, LoadSampleFormatInfo formatInfo)
		{
			// Reset the loader
			ResetBasicLoader();

			// Calculate the position in bytes
			position = CalcFilePosition(position, formatInfo);
			long samplePosition = CalcSamplePosition(position, formatInfo);

			// Seek to the right position in the data chunk
			moduleStream.Seek(dataStarts + position, SeekOrigin.Begin);

			return samplePosition;
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Tells whether the fact chunk is required or not
		/// </summary>
		/********************************************************************/
		protected abstract bool NeedFact { get; }



		/********************************************************************/
		/// <summary>
		/// Initialize the loader
		/// </summary>
		/********************************************************************/
		protected abstract void LoaderInitialize(long dataStart, long dataLength, long totalFileSize, LoadSampleFormatInfo formatInfo);



		/********************************************************************/
		/// <summary>
		/// Cleanup the loader
		/// </summary>
		/********************************************************************/
		protected abstract void LoaderCleanup();



		/********************************************************************/
		/// <summary>
		/// Loads any extra header information from the 'fmt ' chunk
		/// </summary>
		/********************************************************************/
		protected virtual int LoadExtraHeaderInfo(ModuleStream moduleStream, LoadSampleFormatInfo formatInfo, out string errorMessage)
		{
			errorMessage = string.Empty;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Loads the 'fact' chunk
		/// </summary>
		/********************************************************************/
		protected virtual int LoadFactChunk(ModuleStream moduleStream)
		{
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Load and decode a block of sample data
		/// </summary>
		/********************************************************************/
		protected abstract int DecodeSampleData(ModuleStream moduleStream, int[] buffer, int length, LoadSampleFormatInfo formatInfo);



		/********************************************************************/
		/// <summary>
		/// Calculates the number of bytes to go into the file to reach the
		/// position given
		/// </summary>
		/********************************************************************/
		protected abstract long CalcFilePosition(long position, LoadSampleFormatInfo formatInfo);



		/********************************************************************/
		/// <summary>
		/// Calculates the number of samples from the byte position given
		/// </summary>
		/********************************************************************/
		protected abstract long CalcSamplePosition(long position, LoadSampleFormatInfo formatInfo);

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Loads some part of the sample data from the stream and return
		/// them
		/// </summary>
		/********************************************************************/
		protected int GetFileData(ModuleStream moduleStream, byte[] buffer, int length)
		{
			int total = 0;

			while (length > 0)
			{
				// If the number of bytes taken from the buffer is equal to the fill
				// count, load some new data from the file
				if (loadBufferOffset == loadBufferFillCount)
				{
					// Load the data
					loadBufferFillCount = moduleStream.Read(loadBuffer, 0, loadBuffer.Length);
					loadBufferOffset = 0;

					// Well, there isn't any data left to read
					if (loadBufferFillCount == 0)
						break;
				}

				// Find out how many bytes to copy
				int todo = Math.Min(length, loadBufferFillCount - loadBufferOffset);

				// Copy the data
				Array.Copy(loadBuffer, loadBufferOffset, buffer, total, todo);

				// Adjust the variables
				loadBufferOffset += todo;
				length -= todo;
				total += todo;
			}

			return total;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Initialize the loader part
		/// </summary>
		/********************************************************************/
		private void InitBasicLoader()
		{
			loadBuffer = new byte[LoadBufferSize];

			// Initialize load variables
			ResetBasicLoader();
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the loader part
		/// </summary>
		/********************************************************************/
		private void CleanupBasicLoader()
		{
			loadBuffer = null;
		}



		/********************************************************************/
		/// <summary>
		/// Resets loader variables
		/// </summary>
		/********************************************************************/
		private void ResetBasicLoader()
		{
			loadBufferFillCount = 0;
			loadBufferOffset = 0;
			samplesLeft = 0;
		}
		#endregion
	}
}
