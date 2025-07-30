/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Linq;
using System.Text;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Iff16Sv
{
	/// <summary>
	/// Derive from this class, if your format supports loading
	/// </summary>
	internal abstract class Iff16SvLoaderWorkerBase : Iff16SvWorkerBase, ISampleLoaderAgent
	{
		private class LoadInfo
		{
			public byte[] LoadBuffer { get; set; }
			public int LoadBufferFillCount { get; set; }
			public int LoadBufferOffset { get; set; }
		}

		private ModuleStream moduleStream2;

		private long dataStarts1;
		private long dataStarts2;

		private string copyright;
		private string annotation;
		private byte octaves;

		private uint totalLength;

		private LoadInfo loadInfo1;
		private LoadInfo loadInfo2;

		protected int samplesLeft;

		protected const int LoadBufferSize = 32768;
		protected const int DecodeBufferSize = 16384;

		#region ISampleLoaderAgent implementation
		/********************************************************************/
		/// <summary>
		/// Return the file extensions that is supported by the loader
		/// </summary>
		/********************************************************************/
		public string[] FileExtensions => [ "16sv", "iff16" ];



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
			if (moduleStream.ReadMark() == "FORM")
			{
				moduleStream.Seek(4, SeekOrigin.Current);		// Skip length
				if (moduleStream.ReadMark() == "16SV")
				{
					// See if we can find the 'VHDR' chunk
					for (;;)
					{
						// Read the chunk name and size
						string chunkName = moduleStream.ReadMark();
						uint chunkSize = moduleStream.Read_B_UINT32();

						// Check if we reached the end of the file
						if (moduleStream.EndOfStream)
							return AgentResult.Unknown;

						if (chunkName == "VHDR")
						{
							// Got it, check the format
							moduleStream.Seek(15, SeekOrigin.Current);
							Format format = (Format)moduleStream.Read_UINT8();
							if (format == FormatId)
								return AgentResult.Ok;

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
			// Find out which line to take
			switch (line)
			{
				// Octaves
				case 0:
				{
					description = Resources.IDS_IFF16SV_INFODESCLINE0;
					value = octaves.ToString();
					break;
				}

				// Copyright
				case 1:
				{
					description = Resources.IDS_IFF16SV_INFODESCLINE1;
					value = string.IsNullOrEmpty(copyright) ? Resources.IDS_IFF16SV_NA : copyright;
					break;
				}

				// Annotation
				case 2:
				{
					description = Resources.IDS_IFF16SV_INFODESCLINE2;
					value = string.IsNullOrEmpty(annotation) ? Resources.IDS_IFF16SV_NA : annotation;
					break;
				}

				default:
				{
					description = null;
					value = null;

					return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the loader
		/// </summary>
		/********************************************************************/
		public virtual bool InitLoader(out string errorMessage)
		{
			errorMessage = string.Empty;

			moduleStream2 = null;

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

			// Delete other file handle if any
			moduleStream2?.Dispose();
			moduleStream2 = null;
		}



		/********************************************************************/
		/// <summary>
		/// Load the sample header
		/// </summary>
		/********************************************************************/
		public bool LoadHeader(ModuleStream moduleStream, out LoadSampleFormatInfo formatInfo, out string errorMessage)
		{
			errorMessage = string.Empty;

			Encoding encoder = EncoderCollection.Amiga;

			// Initialize the format structure
			formatInfo = new LoadSampleFormatInfo
			{
				Bits = 16,
				Channels = 1
			};

			// Skip the header
			moduleStream.Seek(12, SeekOrigin.Begin);

			// Set some flags
			bool gotVhdr = false;
			bool gotBody = false;

			for (;;)
			{
				// Read the chunk name and size
				string chunkName = moduleStream.ReadMark();
				uint chunkSize = moduleStream.Read_B_UINT32();

				// Check if we reached the end of the file
				if (moduleStream.EndOfStream)
					break;

				// Interpret the known chunks
				switch (chunkName)
				{
					// Name chunk
					case "NAME":
					{
						// Read the string
						byte[] strBuf = new byte[chunkSize + 1];
						moduleStream.ReadString(strBuf, (int)chunkSize);

						formatInfo.Name = encoder.GetString(strBuf);

						// Skip any pad bytes
						if ((chunkSize % 2) != 0)
							moduleStream.Seek(1, SeekOrigin.Current);

						break;
					}

					// Author chunk
					case "AUTH":
					{
						// Read the string
						byte[] strBuf = new byte[chunkSize + 1];
						moduleStream.ReadString(strBuf, (int)chunkSize);

						formatInfo.Author = encoder.GetString(strBuf);

						// Skip any pad bytes
						if ((chunkSize % 2) != 0)
							moduleStream.Seek(1, SeekOrigin.Current);

						break;
					}

					// Copyright chunk
					case "(c) ":
					{
						// Read the string
						byte[] strBuf = new byte[chunkSize + 1];
						moduleStream.ReadString(strBuf, (int)chunkSize);

						copyright = encoder.GetString(strBuf);

						// Skip any pad bytes
						if ((chunkSize % 2) != 0)
							moduleStream.Seek(1, SeekOrigin.Current);

						break;
					}

					// Annotation chunk
					case "ANNO":
					{
						// Read the string
						byte[] strBuf = new byte[chunkSize + 1];
						moduleStream.ReadString(strBuf, (int)chunkSize);

						annotation = encoder.GetString(strBuf);

						// Skip any pad bytes
						if ((chunkSize % 2) != 0)
							moduleStream.Seek(1, SeekOrigin.Current);

						break;
					}

					// Channel chunk
					case "CHAN":
					{
						uint chanVal = moduleStream.Read_B_UINT32();

						// 2 = Left channel
						// 4 = Right channel
						// 6 = Stereo
						//
						// We do only check for stereo. Mono samples
						// will be played in both speakers
						if (chanVal == 6)
							formatInfo.Channels = 2;

						// Skip any extra data
						moduleStream.Seek((chunkSize - 4 + 1) & 0xfffffffe, SeekOrigin.Current);
						break;
					}

					// Voice header chunk
					case "VHDR":
					{
						// Begin to read the chunk data
						uint oneShotLength = moduleStream.Read_B_UINT32();		// Number of samples in the one-shot part
						uint repeatLength = moduleStream.Read_B_UINT32();		// Number of samples in the repeat part
						moduleStream.Seek(4, SeekOrigin.Current);			// Skip the samples in high octave
						formatInfo.Frequency = moduleStream.Read_B_UINT16();	// Sample frequency
						octaves = moduleStream.Read_UINT8();					// Number of octaves in the file
						moduleStream.Seek(1 + 4, SeekOrigin.Current);		// Skip compression (we know what it is already) and the volume

						if (octaves == 1)
						{
							// Only one octave, set the loop points if looping
							if (repeatLength != 0)
							{
								formatInfo.LoopStart = oneShotLength;
								formatInfo.LoopLength = repeatLength;
								formatInfo.Flags |= LoadSampleFormatInfo.SampleFlag.Loop;
							}

							totalLength = oneShotLength + repeatLength;
						}
						else
						{
							// More than one octave, so fix the total length
							uint length = oneShotLength + repeatLength;
							totalLength = length;

							for (int i = 1; i < octaves; i++)
							{
								length *= 2;
								totalLength += length;
							}
						}

						// Initialize the loader
						InitBasicLoader();

						// Skip any extra data
						moduleStream.Seek((chunkSize - 20 + 1) & 0xfffffffe, SeekOrigin.Current);

						gotVhdr = true;
						break;
					}

					// Body chunk
					case "BODY":
					{
						// Convert the total length from samples-pair to samples
						if (formatInfo.Channels == 2)
							totalLength *= 2;

						// Check for save bugs in old versions of SoundBox
						if (totalLength == chunkSize)
							totalLength /= 2;

						// Remember the position where the data starts
						dataStarts1 = moduleStream.Position;

						// Initialize the loader
						LoaderInitialize(totalLength * 2);

						// Skip any extra data
						moduleStream.Seek((chunkSize + 1) & 0xfffffffe, SeekOrigin.Current);

						gotBody = true;
						break;
					}

					// Unknown chunks
					default:
					{
						bool ok = chunkName.Select(t => (byte)t).All(byt => (byt >= 32) && (byt <= 127));

						if (ok)
							moduleStream.Seek((chunkSize + 1) & 0xfffffffe, SeekOrigin.Current);
						else
							moduleStream.Seek(0, SeekOrigin.End);

						break;
					}
				}
			}

			// Check if we get the chunks needed
			if (!gotVhdr)
			{
				errorMessage = Resources.IDS_IFF16SV_ERR_NOVHDR;
				return false;
			}

			if (!gotBody)
			{
				errorMessage = Resources.IDS_IFF16SV_ERR_NOBODY;
				return false;
			}

			// Okay, now we got all the chunks. If the file is a stereo
			// file, we need to open the file with another file handle,
			// because the file format will store the whole sample for
			// the left channel and then the right channel
			if (formatInfo.Channels == 2)
			{
				moduleStream2 = moduleStream.Duplicate();
				dataStarts2 = dataStarts1 + GetRightChannelPosition(totalLength * 2);
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
			return DecodeSampleData(moduleStream, moduleStream2, buffer, length);
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
		public void SetSamplePosition(ModuleStream moduleStream, long position, LoadSampleFormatInfo formatInfo)
		{
			// Reset the loader
			ResetBasicLoader();
			ResetLoader(position);

			// Calculate the position in bytes
			long filePosition = CalcFilePosition(position, formatInfo);

			// Seek to the right position in the data chunk
			moduleStream.Seek(dataStarts1 + filePosition, SeekOrigin.Begin);
			moduleStream2?.Seek(dataStarts2 + filePosition, SeekOrigin.Begin);
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Initialize the loader
		/// </summary>
		/********************************************************************/
		protected abstract void LoaderInitialize(long dataLength);



		/********************************************************************/
		/// <summary>
		/// Cleanup the loader
		/// </summary>
		/********************************************************************/
		protected abstract void LoaderCleanup();



		/********************************************************************/
		/// <summary>
		/// Reset the loader
		/// </summary>
		/********************************************************************/
		protected virtual void ResetLoader(long position)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Load and decode a block of sample data
		/// </summary>
		/********************************************************************/
		protected abstract int DecodeSampleData(ModuleStream moduleStream, ModuleStream moduleStream2, int[] buffer, int length);



		/********************************************************************/
		/// <summary>
		/// Calculates the number of bytes to go into the file to reach the
		/// position given
		/// </summary>
		/********************************************************************/
		protected abstract long CalcFilePosition(long position, LoadSampleFormatInfo formatInfo);



		/********************************************************************/
		/// <summary>
		/// Returns the position where the right channel starts relative to
		/// the left channel
		/// </summary>
		/********************************************************************/
		protected abstract long GetRightChannelPosition(long totalLength);

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Loads some part of the sample data from the stream and return
		/// them
		/// </summary>
		/********************************************************************/
		protected int GetFileData1(ModuleStream moduleStream, byte[] buffer, int offset, int length)
		{
			return GetFileData(moduleStream, buffer, offset, length, loadInfo1);
		}



		/********************************************************************/
		/// <summary>
		/// Loads some part of the sample data from the stream and return
		/// them
		/// </summary>
		/********************************************************************/
		protected int GetFileData2(ModuleStream moduleStream, byte[] buffer, int offset, int length)
		{
			return GetFileData(moduleStream, buffer, offset, length, loadInfo2);
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
			loadInfo1 = new LoadInfo
			{
				LoadBuffer = new byte[LoadBufferSize]
			};

			loadInfo2 = new LoadInfo
			{
				LoadBuffer = new byte[LoadBufferSize]
			};

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
			loadInfo1 = null;
			loadInfo2 = null;
		}



		/********************************************************************/
		/// <summary>
		/// Resets loader variables
		/// </summary>
		/********************************************************************/
		private void ResetBasicLoader()
		{
			loadInfo1.LoadBufferFillCount = 0;
			loadInfo1.LoadBufferOffset = 0;

			loadInfo2.LoadBufferFillCount = 0;
			loadInfo2.LoadBufferOffset = 0;

			samplesLeft = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Loads some part of the sample data from the stream and return
		/// them
		/// </summary>
		/********************************************************************/
		private int GetFileData(ModuleStream moduleStream, byte[] buffer, int offset, int length, LoadInfo loadInfo)
		{
			int total = 0;

			while (length > 0)
			{
				// If the number of bytes taken from the buffer is equal to the fill
				// count, load some new data from the file
				if (loadInfo.LoadBufferOffset == loadInfo.LoadBufferFillCount)
				{
					// Load the data
					loadInfo.LoadBufferFillCount = moduleStream.Read(loadInfo.LoadBuffer, 0, loadInfo1.LoadBuffer.Length);
					loadInfo.LoadBufferOffset = 0;

					// Well, there isn't any data left to read
					if (loadInfo.LoadBufferFillCount == 0)
						break;
				}

				// Find out how many bytes to copy
				int todo = Math.Min(length, loadInfo.LoadBufferFillCount - loadInfo.LoadBufferOffset);

				// Copy the data
				Array.Copy(loadInfo.LoadBuffer, loadInfo.LoadBufferOffset, buffer, offset + total, todo);

				// Adjust the variables
				loadInfo.LoadBufferOffset += todo;
				length -= todo;
				total += todo;
			}

			return total;
		}
		#endregion
	}
}
