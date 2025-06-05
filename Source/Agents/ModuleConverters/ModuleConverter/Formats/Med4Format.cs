/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Formats
{
	/// <summary>
	/// Can convert MED 2.10 (MED4) to MED 2.10 (MMD0) format
	/// </summary>
	internal class Med4Format : ModuleConverterAgentBase
	{
		private static readonly Dictionary<string, ushort> searchDirectories = new Dictionary<string, ushort>
		{
			{ "Instruments", 0x0000 },
			{ "Synthsounds", 0xffff },
			{ "Hybrids", 0xfffe }
		};

		private Dictionary<uint, uint> modulePointersToFix;

		private string[] sampleNames;
		private byte numberOfSamples;
		private ushort numberOfBlocks;
		private byte moduleFlag;

		private string annotation;
		private byte[] holdAndDecay;

		#region IModuleConverterAgent implementation
		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 36)
				return AgentResult.Unknown;

			// Now check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);
			string mark = moduleStream.ReadMark(3);
			byte version = moduleStream.Read_UINT8();

			// Check the mark
			if ((mark == "MED") && (version == 4))
				return AgentResult.Ok;

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the module and store the result in the stream given
		/// </summary>
		/********************************************************************/
		public override AgentResult Convert(PlayerFileInfo fileInfo, ConverterStream converterStream, out string errorMessage)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			modulePointersToFix = new Dictionary<uint, uint>();

			// Skip ID mark
			moduleStream.Seek(4, SeekOrigin.Begin);

			// Write MMD0 header
			WriteHeader(converterStream);

			// Wring song information
			if (!WriteSong(moduleStream, converterStream, out errorMessage))
				return AgentResult.Error;

			// Write block information
			if (!WriteAllBlocks(moduleStream, converterStream, out errorMessage))
				return AgentResult.Error;

			// Read the IFF like structure at the end of the module
			if (!ReadIffStructure(moduleStream, out errorMessage))
				return AgentResult.Error;

			// Write extra information
			WriteExtraInformation(converterStream);

			// Write sample data
			if (!WriteSampleData(fileInfo, moduleStream, converterStream, out errorMessage))
				return AgentResult.Error;

			// Fix all the offset pointers
			FixOffsets(converterStream);

			return AgentResult.Ok;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Write the MMD0 header
		/// </summary>
		/********************************************************************/
		private void WriteHeader(ConverterStream converterStream)
		{
			converterStream.Write_B_UINT32(0x4d4d4430);		// MMD0
			converterStream.Write_B_UINT32(0);				// Length, unknown at the moment. Will be written at the end of the conversion
			converterStream.Write_B_UINT32(0);				// Song pointer
			converterStream.Write_B_UINT16(0);				// Internal player specific information
			converterStream.Write_B_UINT16(0);
			converterStream.Write_B_UINT32(0);				// Block array pointer
			converterStream.Write_UINT8(0);					// Flag
			converterStream.Write_UINT8(0);					// Reserved
			converterStream.Write_UINT8(0);	
			converterStream.Write_UINT8(0);	
			converterStream.Write_B_UINT32(0);				// Instrument array pointer
			converterStream.Write_B_UINT32(0);				// Reserved
			converterStream.Write_B_UINT32(0);				// Expansion pointer
			converterStream.Write_B_UINT32(0);				// Reserved
			converterStream.Write_B_UINT16(0);				// Internal player specific information
			converterStream.Write_B_UINT16(0);
			converterStream.Write_B_UINT16(0);
			converterStream.Write_B_UINT16(0);
			converterStream.Write_B_INT16(-1);
			converterStream.Write_UINT8(0);
			converterStream.Write_UINT8(0);					// Number of songs - 1
		}



		/********************************************************************/
		/// <summary>
		/// Write the song information
		/// </summary>
		/********************************************************************/
		private bool WriteSong(ModuleStream moduleStream, ConverterStream converterStream, out string errorMessage)
		{
			modulePointersToFix[8] = (uint)converterStream.Position;

			if (!WriteSamples(moduleStream, converterStream, out errorMessage))
				return false;

			numberOfBlocks = moduleStream.Read_B_UINT16();
			converterStream.Write_B_UINT16(numberOfBlocks);		// Number of blocks

			ushort temp = moduleStream.Read_B_UINT16();
			converterStream.Write_B_UINT16(temp);				// Song length

			byte[] tempArray = new byte[256];
			moduleStream.ReadInto(tempArray, 0, temp);
			converterStream.Write(tempArray, 0, 256);

			temp = moduleStream.Read_B_UINT16();
			converterStream.Write_B_UINT16(temp);				// Default tempo

			sbyte temp1 = moduleStream.Read_INT8();
			converterStream.Write_INT8(temp1);					// Playing transpose

			moduleFlag = moduleStream.Read_UINT8();
			converterStream.Write_UINT8(moduleFlag);			// Flags
			converterStream.Write_UINT8(0);					// Flags2

			temp = moduleStream.Read_B_UINT16();
			converterStream.Write_UINT8((byte)temp);			// Tempo2

			// Skip jumping mask and colors
			moduleStream.Seek(4 + 16, SeekOrigin.Current);

			moduleStream.ReadInto(tempArray, 0, 16);
			converterStream.Write(tempArray, 0, 16);	// Track volumes

			byte temp2 = moduleStream.Read_UINT8();
			converterStream.Write_UINT8(temp2);					// Master volume

			converterStream.Write_UINT8(numberOfSamples);

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_ERR_LOADING_HEADER;
				return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Write the sample information
		/// </summary>
		/********************************************************************/
		private bool WriteSamples(ModuleStream moduleStream, ConverterStream converterStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			sampleNames = new string[63];

			byte maskMask = moduleStream.Read_UINT8();
			ulong sampleMask = 0;
			int maskCount = 0;

			for (int i = 0; i < 8; i++)
			{
				if ((maskMask & 0x80) != 0)
				{
					sampleMask <<= 8;
					sampleMask |= moduleStream.Read_UINT8();
					maskCount++;
				}

				maskMask <<= 1;
			}

			sampleMask <<= 64 - maskCount * 8;

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_ERR_LOADING_SAMPLEINFO;
				return false;
			}

			Encoding encoder = EncoderCollection.Amiga;

			for (int i = 0; i < 63; i++)
			{
				string name = string.Empty;
				ushort loopStart = 0;
				ushort loopLength = 0;
				byte midiChannel = 0;
				byte midiPreset = 0;
				byte volume = 64;
				sbyte transpose = 0;

				if ((sampleMask & 0x8000000000000000) != 0)
				{
					byte flag = moduleStream.Read_UINT8();

					byte nameLength = moduleStream.Read_UINT8();
					name = moduleStream.ReadString(encoder, nameLength);

					if ((flag & 0x01) == 0)
						loopStart = moduleStream.Read_B_UINT16();

					if ((flag & 0x02) == 0)
						loopLength = moduleStream.Read_B_UINT16();

					if ((flag & 0x04) == 0)
						midiChannel = moduleStream.Read_UINT8();

					if ((flag & 0x08) == 0)
						midiPreset = moduleStream.Read_UINT8();

					if ((flag & 0x30) == 0)
						volume = moduleStream.Read_UINT8();

					if ((flag & 0x40) == 0)
						transpose = moduleStream.Read_INT8();

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_ERR_LOADING_SAMPLEINFO;
						return false;
					}

					if (loopLength <= 2)
					{
						loopStart = 0;
						loopLength = 0;
					}
				}

				sampleNames[i] = name;

				converterStream.Write_B_UINT16(loopStart);
				converterStream.Write_B_UINT16(loopLength);
				converterStream.Write_UINT8(midiChannel);
				converterStream.Write_UINT8(midiPreset);
				converterStream.Write_UINT8(volume);
				converterStream.Write_INT8(transpose);

				sampleMask <<= 1;
			}

			numberOfSamples = 0;

			for (int i = 62; i >= 0; i--)
			{
				if (!string.IsNullOrEmpty(sampleNames[i]))
				{
					numberOfSamples = (byte)(i + 1);
					break;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Write the block information
		/// </summary>
		/********************************************************************/
		private bool WriteAllBlocks(ModuleStream moduleStream, ConverterStream converterStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			uint blockArrayOffset = (uint)converterStream.Position;
			modulePointersToFix[16] = blockArrayOffset;

			for (int i = 0; i < numberOfBlocks; i++)
				converterStream.Write_B_UINT32(0);		// Block pointers

			for (int i = 0; i < numberOfBlocks; i++)
			{
				modulePointersToFix[(uint)(blockArrayOffset + i * 4)] = (uint)converterStream.Position;

				if (!WriteBlock(moduleStream, converterStream))
				{
					errorMessage = Resources.IDS_ERR_LOADING_PATTERNS;
					return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Write a single block
		/// </summary>
		/********************************************************************/
		private bool WriteBlock(ModuleStream moduleStream, ConverterStream converterStream)
		{
			byte blockHeaderLength = moduleStream.Read_UINT8();
			long headerStart = moduleStream.Position;
			byte numberOfTracks = moduleStream.Read_UINT8();
			byte numberOfRows = moduleStream.Read_UINT8();
			ushort blockDataLength = moduleStream.Read_B_UINT16();

			uint blockFlag = 0;
			for (int i = numberOfRows + 1; i > 0; i -= 64)
			{
				blockFlag <<= 8;
				blockFlag |= moduleStream.Read_UINT8();
			}

			blockFlag <<= 32 - ((numberOfRows + 64) / 64) * 8;

			uint[] lineMasks = new uint[8];				// One mask per 32. row, max 256 rows
			uint[] effectMasks = new uint[8];

			for (int i = 0, j = numberOfRows + 1; j > 0; j -= 32, i++)
			{
				if ((blockFlag & 0x80000000) != 0)
					lineMasks[i] = 0xffffffff;
				else if ((blockFlag & 0x40000000) != 0)
					lineMasks[i] = 0x00000000;
				else
					lineMasks[i] = moduleStream.Read_B_UINT32();

				if ((blockFlag & 0x20000000) != 0)
					effectMasks[i] = 0xffffffff;
				else if ((blockFlag & 0x10000000) != 0)
					effectMasks[i] = 0x00000000;
				else
					effectMasks[i] = moduleStream.Read_B_UINT32();

				blockFlag <<= 4;
			}

			if (moduleStream.EndOfStream)
				return false;

			moduleStream.Seek(headerStart + blockHeaderLength, SeekOrigin.Begin);

			byte[] blockData = new byte[blockDataLength];
			int bytesRead = moduleStream.Read(blockData, 0, blockDataLength);

			if (bytesRead < blockDataLength)
				return false;

			// Write block header
			converterStream.Write_UINT8(numberOfTracks);
			converterStream.Write_UINT8(numberOfRows);

			byte[] convertedBlockData = new byte[numberOfTracks * 3 * (numberOfRows + 1)];

			uint currentLineMask = 0;
			uint currentEffectMask = 0;
			ushort nibbleNumber = 0;

			for (int i = 0; i <= numberOfRows; i++)
			{
				if ((i % 32) == 0)
				{
					currentLineMask = lineMasks[i / 32];
					currentEffectMask = effectMasks[i / 32];
				}

				if ((currentLineMask & 0x80000000) != 0)
				{
					ushort channelMask = GetNibbles(blockData, ref nibbleNumber, numberOfTracks / 4);
					channelMask <<= (16 - numberOfTracks);

					for (int j = 0; j < numberOfTracks; j++)
					{
						if ((channelMask & 0x8000) != 0)
						{
							convertedBlockData[i * (numberOfTracks * 3) + j * 3] = (byte)GetNibbles(blockData, ref nibbleNumber, 2);
							convertedBlockData[i * (numberOfTracks * 3) + j * 3 + 1] = (byte)(GetNibble(blockData, ref nibbleNumber) << 4);
						}

						channelMask <<= 1;
					}
				}

				if ((currentEffectMask & 0x80000000) != 0)
				{
					ushort channelMask = GetNibbles(blockData, ref nibbleNumber, numberOfTracks / 4);
					channelMask <<= (16 - numberOfTracks);

					for (int j = 0; j < numberOfTracks; j++)
					{
						if ((channelMask & 0x8000) != 0)
						{
							convertedBlockData[i * (numberOfTracks * 3) + j * 3 + 1] |= GetNibble(blockData, ref nibbleNumber);
							convertedBlockData[i * (numberOfTracks * 3) + j * 3 + 2] = (byte)GetNibbles(blockData, ref nibbleNumber, 2);
						}

						channelMask <<= 1;
					}
				}

				currentLineMask <<= 1;
				currentEffectMask <<= 1;
			}

			if (moduleStream.EndOfStream)
				return false;

			converterStream.Write(convertedBlockData, 0, convertedBlockData.Length);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read a single nibble
		/// </summary>
		/********************************************************************/
		private byte GetNibble(byte[] blockData, ref ushort nibbleNumber)
		{
			byte result;

			int offset = nibbleNumber / 2;

			if ((nibbleNumber & 1) != 0)
				result = (byte)(blockData[offset] & 0x0f);
			else
				result = (byte)(blockData[offset] >> 4);

			nibbleNumber++;

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Read a multiple number of nibbles
		/// </summary>
		/********************************************************************/
		private ushort GetNibbles(byte[] blockData, ref ushort nibbleNumber, int count)
		{
			ushort result = 0;

			while (count-- > 0)
			{
				result <<= 4;
				result |= GetNibble(blockData, ref nibbleNumber);
			}

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Will read the IFF structure at the end of the file
		/// </summary>
		/********************************************************************/
		private bool ReadIffStructure(ModuleStream moduleStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			long originalPosition = moduleStream.Position;

			annotation = null;
			holdAndDecay = null;

			// Start to search after the structure
			byte[] buffer = new byte[1024];

			moduleStream.Seek(-Math.Min(1024, moduleStream.Length), SeekOrigin.End);
			long searchPosition = moduleStream.Position;

			moduleStream.ReadInto(buffer, 0, 1024);

			bool found = false;
			for (int i = 0; i < 1024 - 8; i++)
			{
				if ((buffer[i] == 0x4d) && (buffer[i + 1] == 0x45) && (buffer[i + 2] == 0x44) && (buffer[i + 3] == 0x56))
				{
					moduleStream.Seek(searchPosition + i, SeekOrigin.Begin);
					found = true;
					break;
				}
			}

			if (found)
			{
				Encoding encoder = EncoderCollection.Amiga;

				for (;;)
				{
					string id = moduleStream.ReadMark();
					uint length = moduleStream.Read_B_UINT32();

					if (moduleStream.EndOfStream)
						break;

					switch (id)
					{
						case "MEDV":
						{
							moduleStream.Seek(length, SeekOrigin.Current);
							break;
						}

						case "ANNO":
						{
							moduleStream.ReadInto(buffer, 0, (int)length);
							annotation = encoder.GetString(buffer, 0, (int)length);
							break;
						}

						case "HLDC":
						{
							holdAndDecay = new byte[length];
							moduleStream.ReadInto(holdAndDecay, 0, (int)length);
							break;
						}

						default:
						{
							moduleStream.Seek(length, SeekOrigin.Current);
							break;
						}
					}
				}
			}

			moduleStream.Seek(originalPosition, SeekOrigin.Begin);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Write extra information
		/// </summary>
		/********************************************************************/
		private void WriteExtraInformation(ConverterStream converterStream)
		{
			Encoding encoder = EncoderCollection.Amiga;

			uint annotationOffset = 0, annotationLength = 0;
			uint sampleNameOffset = 0;
			uint sampleExtraOffset = 0;
			ushort sampleExtraCount = 0;

			if (!string.IsNullOrEmpty(annotation))
			{
				annotationOffset = (uint)converterStream.Position;

				byte[] encodedString = encoder.GetBytes(annotation);
				converterStream.Write(encodedString);
				converterStream.Write_UINT8(0);			// Null terminate the string

				if ((converterStream.Position % 2) != 0)
					converterStream.Write_UINT8(0);		// Align the structure

				annotationLength = (uint)encodedString.Length + 1;
			}

			if (numberOfSamples > 0)
			{
				sampleNameOffset = (uint)converterStream.Position;

				for (int i = 0; i < numberOfSamples; i++)
				{
					byte[] encodedString = encoder.GetBytes(sampleNames[i]);
					converterStream.Write(encodedString);

					for (int j = encodedString.Length; j < 42; j++)
						converterStream.Write_UINT8(0);
				}
			}

			if (holdAndDecay != null)
			{
				sampleExtraOffset = (uint)converterStream.Position;
				sampleExtraCount = (ushort)Math.Min(numberOfSamples, holdAndDecay.Length / 2);

				for (int i = 0; i < sampleExtraCount; i++)
				{
					converterStream.Write_UINT8(holdAndDecay[i * 2]);		// Hold
					converterStream.Write_UINT8(holdAndDecay[i * 2 + 1]);	// Decay
					converterStream.Write_UINT8(0);						// Supress MIDI off
					converterStream.Write_INT8(0);						// Fine tune
				}
			}

			modulePointersToFix[32] = (uint)converterStream.Position;

			converterStream.Write_B_UINT32(0);			// Next module
			converterStream.Write_B_UINT32(sampleExtraOffset);// Pointer to InstrExt structure
			converterStream.Write_B_UINT16(sampleExtraCount);// Size of InstrExt array
			converterStream.Write_B_UINT16((ushort)(holdAndDecay != null ? 4 : 0));// Size of each InstrExt structure
			converterStream.Write_B_UINT32(annotationOffset);
			converterStream.Write_B_UINT32(annotationLength);
			converterStream.Write_B_UINT32(sampleNameOffset);
			converterStream.Write_B_UINT16(numberOfSamples);
			converterStream.Write_B_UINT16((ushort)(numberOfSamples > 0 ? 42 : 0));
			converterStream.Write_B_UINT32(0);			// Jump mask
			converterStream.Write_B_UINT32(0);			// Pointer to color table
			converterStream.Write_B_UINT32(0);			// Channel split
			converterStream.Write_B_UINT32(0);			// Pointer to notation info
			converterStream.Write_B_UINT32(0);			// Pointer to song name
			converterStream.Write_B_UINT32(0);			// Song name length
			converterStream.Write_B_UINT32(0);			// Pointer to MIDI dumps
			converterStream.Write_B_UINT32(0);			// Pointer to info
			converterStream.Write_B_UINT32(0);			// Pointer to ARexx
			converterStream.Write_B_UINT32(0);			// Pointer to MIDI command 3x
			converterStream.Write_B_UINT32(0);			// Reserved
			converterStream.Write_B_UINT32(0);
			converterStream.Write_B_UINT32(0);
			converterStream.Write_B_UINT32(0);			// Tag end
		}



		/********************************************************************/
		/// <summary>
		/// Write all the sample data
		/// </summary>
		/********************************************************************/
		private bool WriteSampleData(PlayerFileInfo fileInfo, ModuleStream moduleStream, ConverterStream converterStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			uint sampleDataArrayOffset = (uint)converterStream.Position;
			modulePointersToFix[24] = sampleDataArrayOffset;

			for (int i = 0; i < numberOfSamples; i++)
				converterStream.Write_B_UINT32(0);		// Sample data pointers

			if ((moduleFlag & 0x08) != 0)
			{
				ulong sampleMask = moduleStream.Read_B_UINT64();

				for (int i = 0; i < numberOfSamples; i++)
				{
					sampleMask <<= 1;	// There is no sample #0, so we start to shift

					if ((sampleMask & 0x8000000000000000) != 0)
					{
						modulePointersToFix[(uint)(sampleDataArrayOffset + i * 4)] = (uint)converterStream.Position;

						if (!WriteSingleSample(moduleStream, converterStream, i))
						{
							errorMessage = Resources.IDS_ERR_LOADING_SAMPLES;
							return false;
						}
					}
				}
			}
			else
			{
				for (int i = 0; i < numberOfSamples; i++)
				{
					if (!string.IsNullOrEmpty(sampleNames[i]))
					{
						modulePointersToFix[(uint)(sampleDataArrayOffset + i * 4)] = (uint)converterStream.Position;

						if (!WriteSingleExternalSample(fileInfo, converterStream, i))
						{
							errorMessage = string.Format(Resources.IDS_ERR_LOADING_EXTERNAL_SAMPLE, sampleNames[i]);
							return false;
						}
					}
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Write a single sample data
		/// </summary>
		/********************************************************************/
		private bool WriteSingleSample(ModuleStream moduleStream, ConverterStream converterStream, int sampleNumber)
		{
			uint length = moduleStream.Read_B_UINT32();
			ushort type = moduleStream.Read_B_UINT16();

			switch (type)
			{
				// Synth and Hybrid
				case 0xffff:
				case 0xfffe:
					return WriteSynthInformation(moduleStream, converterStream, sampleNumber, type, false);

				// Normal sample or multiple octave sample
				default:
				{
					converterStream.Write_B_UINT32(length);
					converterStream.Write_B_UINT16(0);

					moduleStream.SetSampleDataInfo(sampleNumber, (int)length);
					converterStream.WriteSampleDataMarker(sampleNumber, (int)length);
					break;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Write a single external sample data
		/// </summary>
		/********************************************************************/
		private bool WriteSingleExternalSample(PlayerFileInfo fileInfo, ConverterStream converterStream, int sampleNumber)
		{
			foreach (KeyValuePair<string, ushort> pair in searchDirectories)
			{
				using (ModuleStream moduleStream = fileInfo.Loader?.TryOpenExternalFile(pair.Key, sampleNames[sampleNumber], out _))
				{
					// Did we get any file at all
					if (moduleStream == null)
						continue;

					moduleStream.Seek(0, SeekOrigin.Begin);

					switch (pair.Value)
					{
						case 0xfffe:
						case 0xffff:
							return WriteSynthInformation(moduleStream, converterStream, sampleNumber, pair.Value, true);

						default:
						{
							converterStream.Write_B_UINT32((uint)moduleStream.Length);
							converterStream.Write_B_UINT16(pair.Value);

							byte[] buffer = new byte[moduleStream.Length];
							moduleStream.ReadInto(buffer, 0, buffer.Length);

							buffer = FixIfIff(buffer);
							converterStream.Write(buffer, 0, buffer.Length);

							return true;
						}
					}
				}
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Write synth information
		/// </summary>
		/********************************************************************/
		private bool WriteSynthInformation(ModuleStream moduleStream, ConverterStream converterStream, int sampleNumber, ushort type, bool copy)
		{
			// Write header
			moduleStream.Seek(6, SeekOrigin.Current);

			converterStream.Write_B_UINT32(0x110);
			converterStream.Write_B_UINT16(type);

			uint readStartOffset = (uint)moduleStream.Position - 6;
			uint startOffset = (uint)converterStream.Position - 6;

			byte temp = moduleStream.Read_UINT8();
			converterStream.Write_UINT8(temp);			// Default decay

			moduleStream.Seek(3, SeekOrigin.Current);
			converterStream.Write_UINT8(0);			// Reserved
			converterStream.Write_UINT8(0);
			converterStream.Write_UINT8(0);

			ushort temp1 = moduleStream.Read_B_UINT16();
			converterStream.Write_B_UINT16(temp1);		// Loop start

			temp1 = moduleStream.Read_B_UINT16();
			converterStream.Write_B_UINT16(temp1);		// Loop length

			ushort volumeSequenceLength = moduleStream.Read_B_UINT16();
			ushort waveformSequenceLength = moduleStream.Read_B_UINT16();
			converterStream.Write_B_UINT16(128);
			converterStream.Write_B_UINT16(128);

			temp = moduleStream.Read_UINT8();
			converterStream.Write_UINT8(temp);			// Volume sequence speed

			temp = moduleStream.Read_UINT8();
			converterStream.Write_UINT8(temp);			// Waveform sequence speed

			ushort waveformCount = moduleStream.Read_B_UINT16();
			converterStream.Write_B_UINT16(waveformCount);

			byte[] buffer = new byte[128];
			moduleStream.ReadInto(buffer, 0, volumeSequenceLength);
			converterStream.Write(buffer, 0, 128);

			Array.Clear(buffer);
			moduleStream.ReadInto(buffer, 0, waveformSequenceLength);
			converterStream.Write(buffer, 0, 128);

			if (moduleStream.EndOfStream)
				return false;

			// Write waveform offset table
			uint arrayStartOffset = (uint)converterStream.Position;
			uint[] readOffsets = new uint[waveformCount];

			for (int i = 0; i < waveformCount; i++)
			{
				readOffsets[i] = moduleStream.Read_B_UINT32();
				converterStream.Write_B_UINT32(0);
			}

			// Write the waveforms
			for (int i = 0; i < waveformCount; i++)
			{
				modulePointersToFix[(uint)(arrayStartOffset + i * 4)] = (uint)converterStream.Position - startOffset;

				moduleStream.Seek(readStartOffset + readOffsets[i], SeekOrigin.Begin);

				if ((i == 0) && (type == 0xfffe))
				{
					uint length = moduleStream.Read_B_UINT32();
					moduleStream.Seek(2, SeekOrigin.Current);

					converterStream.Write_B_UINT32(length);
					converterStream.Write_B_UINT16(0);

					if (copy)
						Helpers.CopyData(moduleStream, converterStream, (int)length);
					else
					{
						moduleStream.SetSampleDataInfo(sampleNumber, (int)length);
						converterStream.WriteSampleDataMarker(sampleNumber, (int)length);
					}
				}
				else
				{
					ushort length = moduleStream.Read_B_UINT16();
					converterStream.Write_B_UINT16(length);

					Helpers.CopyData(moduleStream, converterStream, length * 2);
				}

				if (moduleStream.EndOfStream)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Fix all the offsets
		/// </summary>
		/********************************************************************/
		private void FixOffsets(ConverterStream converterStream)
		{
			// First write the length of the module
			converterStream.Seek(4, SeekOrigin.Begin);
			converterStream.Write_B_UINT32((uint)converterStream.Length);		// I know that this will not include the sample data itself, only the markers

			// Now fix all the offset pointers
			foreach (KeyValuePair<uint, uint> pair in modulePointersToFix)
			{
				converterStream.Seek(pair.Key, SeekOrigin.Begin);
				converterStream.Write_B_UINT32(pair.Value);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Check to see if the loaded sample is in IFF format and if so,
		/// return only the sample part
		/// </summary>
		/********************************************************************/
		private byte[] FixIfIff(byte[] sampleData)
		{
			if ((sampleData[0] == 0x46) && (sampleData[1] == 0x4f) && (sampleData[2] == 0x52) && (sampleData[3] == 0x4d))	// FORM
			{
				if ((sampleData[8] == 0x38) && (sampleData[9] == 0x53) && (sampleData[10] == 0x56) && (sampleData[11] == 0x58))	// 8SVX
				{
					int offset = 12;

					while (offset < sampleData.Length)
					{
						int length = (sampleData[offset + 4] << 24) | (sampleData[offset + 5] << 16) | (sampleData[offset + 6] << 8) | sampleData[offset + 7];

						if ((sampleData[offset] == 0x42) && (sampleData[offset + 1] == 0x4f) && (sampleData[offset + 2] == 0x44) && (sampleData[offset + 3] == 0x59))	// BODY
						{
							if (length > (sampleData.Length - offset))
								return null;

							return sampleData.AsSpan(offset + 8, length).ToArray();
						}

						offset += 8 + length;
					}
				}
			}

			return sampleData;
		}
		#endregion
	}
}
