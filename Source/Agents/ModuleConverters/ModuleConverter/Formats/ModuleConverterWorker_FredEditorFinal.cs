/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
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
	/// Can convert Fred Editor (Final) to Fred Editor format
	/// </summary>
	internal class ModuleConverterWorker_FredEditorFinal : ModuleConverterAgentBase
	{
		private class InstInfo
		{
			public int SampleOffset;
			public ushort SampleLength;
		}

		private int moduleOffset;
		private int offsetDiff;

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
			long fileSize = moduleStream.Length;
			if (fileSize < 0xb0e)
				return AgentResult.Unknown;

			// Check the code to see if it's a Fred module
			moduleStream.Seek(0, SeekOrigin.Begin);

			// First check the JMP instructions
			if (moduleStream.Read_B_UINT16() != 0x4efa)
				return AgentResult.Unknown;

			short initOffset = (short)(moduleStream.Read_B_INT16() + 2);

			if (moduleStream.Read_B_UINT16() != 0x4efa)
				return AgentResult.Unknown;

			moduleStream.Seek(2, SeekOrigin.Current);

			if (moduleStream.Read_B_UINT16() != 0x4efa)
				return AgentResult.Unknown;

			moduleStream.Seek(2, SeekOrigin.Current);

			if (moduleStream.Read_B_UINT16() != 0x4efa)
				return AgentResult.Unknown;

			// Seek to the init routine
			moduleStream.Seek(initOffset, SeekOrigin.Begin);

			// Read the beginning of the routine
			ushort[] initFunc = new ushort[64];

			moduleStream.ReadArray_B_UINT16s(initFunc, 0, 64);

			// Find the place in the routine, where it gets the sub-song number
			int i;
			for (i = 0; i < 4; i++)
			{
				if ((initFunc[i] == 0x123a) && (initFunc[i + 2] == 0xb001))
				{
					// Found it, remember the position to the module data
					moduleOffset = initFunc[i + 1] + initOffset + i * 2 + 1;
					break;
				}
			}

			// Did we find the first piece of code?
			if (i == 4)
				return AgentResult.Unknown;

			// Okay, we need to find some code in the player, so we can
			// calculate the offset difference
			for (; i < 60; i++)
			{
				if ((initFunc[i] == 0x47fa) && (initFunc[i + 2] == 0xd7fa))
				{
					// Found it, now calculate the offset difference
					offsetDiff = initOffset + (i + 1) * 2 + (short)initFunc[i + 1];
					return AgentResult.Ok;
				}
			}

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the module and store the result in the stream given
		/// </summary>
		/********************************************************************/
		public override AgentResult Convert(PlayerFileInfo fileInfo, ConverterStream converterStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			ModuleStream moduleStream = fileInfo.ModuleStream;
			Encoding encoder = EncoderCollection.Amiga;

			// Start to write the ID mark
			byte[] mark = encoder.GetBytes("Fred Editor ");
			converterStream.Write(mark, 0, mark.Length);

			// Write version
			converterStream.Write_B_UINT16(0);

			// Seek to module data
			moduleStream.Seek(moduleOffset, SeekOrigin.Begin);

			// Skip played song
			moduleStream.Seek(1, SeekOrigin.Current);

			// Get the number of sub-songs and write it
			byte subSongs = (byte)(moduleStream.Read_UINT8() + 1);
			converterStream.Write_B_UINT16(subSongs);

			// Skip current tempo
			moduleStream.Seek(1, SeekOrigin.Current);

			// Get sub-song start tempos
			byte[] startTempos = new byte[10];
			moduleStream.Read(startTempos, 0, 10);
			moduleStream.Seek(1, SeekOrigin.Current);

			// Write the start tempos
			converterStream.Write(startTempos, 0, subSongs);

			// Get offset to other structures
			uint instOffset = (uint)(moduleStream.Read_B_UINT32() + offsetDiff);
			uint trackOffset = (uint)(moduleStream.Read_B_UINT32() + offsetDiff);

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_ERR_LOADING_HEADER;
				return AgentResult.Error;
			}

			// Skip replay data
			moduleStream.Seek(100 + 128 * 4, SeekOrigin.Current);

			// Read sub-song start sequence numbers
			ushort[,] startPositions = new ushort[subSongs, 4];

			for (int i = 0; i < subSongs; i++)
			{
				startPositions[i, 0] = (ushort)((moduleStream.Read_B_UINT16() - subSongs * 4 * 2) / 2);
				startPositions[i, 1] = (ushort)((moduleStream.Read_B_UINT16() - subSongs * 4 * 2) / 2);
				startPositions[i, 2] = (ushort)((moduleStream.Read_B_UINT16() - subSongs * 4 * 2) / 2);
				startPositions[i, 3] = (ushort)((moduleStream.Read_B_UINT16() - subSongs * 4 * 2) / 2);
			}

			// Allocate space to hold offsets to the tracks (position list)
			int positionSize = (int)((Math.Min(instOffset, trackOffset) - moduleStream.Position) / 2);
			short[] positionOffsets = new short[positionSize];

			// Load the offsets
			moduleStream.ReadArray_B_INT16s(positionOffsets, 0, positionSize);

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_ERR_LOADING_HEADER;
				return AgentResult.Error;
			}

			// Allocate space to hold the track data
			int trackSize = Math.Abs((int)(instOffset - trackOffset));
			byte[] trackData = new byte[trackSize];

			// Load all the track data
			moduleStream.Seek(trackOffset, SeekOrigin.Begin);
			moduleStream.Read(trackData, 0, trackSize);

			// Create a list holding all the tracks
			List<byte[]> tracks = new List<byte[]>();
			Dictionary<int, int> offsetLookup = new Dictionary<int, int>();

			for (int i = 0, trk = 0; i < trackSize; trk++)
			{
				int startIndex = i;

				while (trackData[i++] != 0x80)
				{
					if (i == trackSize)
						break;
				}

				// If the last track does not contain a "end mark", it
				// is just dummy data, so remove it
				if ((i == trackSize) && (trackData[i - 1] != 0x80))
					break;

				byte[] track = new byte[i - startIndex];
				Array.Copy(trackData, startIndex, track, 0, track.Length);

				offsetLookup[startIndex] = trk;
				tracks.Add(track);
			}

			// Write the position numbers for each sub-song and channel
			byte[] emptyBuffer = new byte[256];

			for (int i = 0; i < subSongs; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					ushort pos = startPositions[i, j];

					while ((pos - startPositions[i, j] < 255) && (positionOffsets[pos] >= 0))
					{
						if (!offsetLookup.TryGetValue(positionOffsets[pos], out int posNum))
						{
							errorMessage = Resources.IDS_ERR_LOADING_TRACKS;
							return AgentResult.Error;
						}

						converterStream.Write_UINT8((byte)posNum);
						pos++;
					}

					// Write end mark
					converterStream.Write_UINT8((byte)(((positionOffsets[pos++] & 0x7fff) / 2) | 0x80));

					// Fill out the rest with zeros
					int emptyLen = 255 - (pos - startPositions[i, j]);

					// Some position table can ofcourse be 256 bytes in length,
					// which is the maximum, so only write padding if less
					if (emptyLen > 0)
					{
						converterStream.Write(emptyBuffer, 0, emptyLen);

						// The last position is always 0x80
						converterStream.Write_UINT8(0x80);
					}
				}
			}

			// Write the track data
			foreach (byte[] track in tracks)
			{
				converterStream.Write_B_INT32(track.Length);
				converterStream.Write(track, 0, track.Length);
			}

			// Seek to the start of the instrument information
			moduleStream.Seek(instOffset, SeekOrigin.Begin);

			// Find out how many instruments are present
			ushort instNum = 0;
			int minSampleOffset = 0x7fffffff;

			for (;;)
			{
				int sampleOffset = moduleStream.Read_B_INT32();
				short testSynth = moduleStream.Read_B_INT16();
				moduleStream.Seek(33, SeekOrigin.Current);
				byte testInst = moduleStream.Read_UINT8();

				// Check to see if we reached the end of the instrument part
				if (moduleStream.EndOfStream || (moduleStream.Position + (63 - 1 - 33 - 2 - 4)) >= minSampleOffset)
					break;	// Well, no more instruments

				// Skip unused instruments
				if (testInst != 0xff)
				{
					if (sampleOffset == 0)
					{
						// !! Gigants-bug1 hack !!
						if ((ushort)testSynth == 0xfcf5)
							break;

						// Synth instrument
						if ((testSynth != -1) && (testInst != 0x00))
						{
							errorMessage = Resources.IDS_ERR_LOADING_INSTRUMENTS;
							return AgentResult.Error;
						}
					}
					else
					{
						// Sample instrument
						sampleOffset += offsetDiff;

						if (sampleOffset < minSampleOffset)
							minSampleOffset = sampleOffset;
					}
				}

				// Seek to next instrument
				moduleStream.Seek(64 - 1 - 33 - 2 - 4, SeekOrigin.Current);

				// Increment the number of instruments counted so far
				instNum++;
			}

			// Now begin to copy the instrument data
			// and count the number as samples as well
			SortedDictionary<ushort, InstInfo> instSampleLength = new SortedDictionary<ushort, InstInfo>();

			converterStream.Write_B_UINT16(instNum);

			moduleStream.Seek(instOffset, SeekOrigin.Begin);

			for (ushort i = 0; i < instNum; i++)
			{
				// Build a name for the instrument
				byte[] instName;

				if (instNum < 100)
					instName = encoder.GetBytes($"instr{i:d2}");
				else
					instName = encoder.GetBytes($"instr{i:d3}");

				converterStream.Write(instName, 0, instName.Length);
				converterStream.Write(emptyBuffer, 0, 32 - instName.Length);

				// Write the instrument number
				converterStream.Write_B_UINT32((uint)(i + 1));

				// Handle the different type of instruments differently
				moduleStream.Seek(39, SeekOrigin.Current);
				byte instType = moduleStream.Read_UINT8();

				if (instType == 0x00)
				{
					// Sample
					moduleStream.Seek(-40, SeekOrigin.Current);

					int sampleOffset = moduleStream.Read_B_INT32();
					ushort repeatLen = moduleStream.Read_B_UINT16();
					ushort length = moduleStream.Read_B_UINT16();

					// !! Exploding Fish hack !!
					if (sampleOffset > moduleStream.Length)
					{
						repeatLen = 0xffff;
						length = 0;
					}
					else
					{
						instSampleLength[(ushort)(i + 1)] = new InstInfo
						{
							SampleOffset = sampleOffset + offsetDiff,
							SampleLength = (ushort)(length * 2)
						};
					}

					converterStream.Write_B_UINT16(repeatLen);
					converterStream.Write_B_UINT16(length);
					Helpers.CopyData(moduleStream, converterStream, 56);
				}
				else
				{
					// Synth
					moduleStream.Seek(-36, SeekOrigin.Current);
					Helpers.CopyData(moduleStream, converterStream, 60);
				}

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_ERR_LOADING_INSTRUMENTS;
					return AgentResult.Error;
				}
			}

			// At last, write the sample data
			converterStream.Write_B_UINT16((ushort)instSampleLength.Count);

			foreach (KeyValuePair<ushort, InstInfo> pair in instSampleLength)
			{
				converterStream.Write_B_UINT16(pair.Key);
				converterStream.Write_B_UINT16(pair.Value.SampleLength);

				moduleStream.Seek(pair.Value.SampleOffset, SeekOrigin.Begin);
				moduleStream.SetSampleDataInfo(pair.Key, pair.Value.SampleLength);

				converterStream.WriteSampleDataMarker(pair.Key, pair.Value.SampleLength);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_ERR_LOADING_SAMPLES;
					return AgentResult.Error;
				}
			}

			// And finally, the end mark
			converterStream.Write_B_UINT32(0x12345678);

			return AgentResult.Ok;
		}
		#endregion
	}
}
