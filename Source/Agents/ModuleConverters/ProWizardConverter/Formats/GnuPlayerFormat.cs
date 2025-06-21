/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// GnuPlayer
	/// </summary>
	internal class GnuPlayerFormat : ProWizardConverterWorker31SamplesBase
	{
		private ushort period;

		private byte[] track1Data;
		private byte[] track2Data;

		private byte numberOfPositions;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT54;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x96)
				return false;

			// Check the mark
			moduleStream.Seek(0x92, SeekOrigin.Begin);

			if (moduleStream.ReadMark() != "GnPl")
				return false;

			// Check sample information
			moduleStream.Seek(0x14, SeekOrigin.Begin);

			for (int i = 0; i < 31; i++)
			{
				ushort sampleSize = moduleStream.Read_B_UINT16();
				ushort loopStart = moduleStream.Read_B_UINT16();

				if (loopStart > (sampleSize + 1))
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Prepare conversion by initialize what is needed etc.
		/// </summary>
		/********************************************************************/
		protected override bool PrepareConversion(ModuleStream moduleStream)
		{
			// Get the note to play
			moduleStream.Seek(0x90, SeekOrigin.Begin);
			period = moduleStream.Read_B_UINT16();

			// Skip ID mark
			moduleStream.Seek(4, SeekOrigin.Current);

			// Read track data
			track1Data = ReadTrack(moduleStream);
			track2Data = ReadTrack(moduleStream);

			// Find the number of patterns/positions
			int lines1 = CalculateNumberOfLines(track1Data);
			int lines2 = CalculateNumberOfLines(track2Data);

			numberOfPositions = (byte)((Math.Max(lines1, lines2) + 1) / 64);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the module if any
		/// </summary>
		/********************************************************************/
		protected override byte[] GetModuleName(ModuleStream moduleStream)
		{
			byte[] moduleName = new byte[20];

			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.ReadInto(moduleName, 0, 20);

			return moduleName;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			for (int i = 0; i < 31; i++)
			{
				ushort length = moduleStream.Read_B_UINT16();
				ushort loopStart = moduleStream.Read_B_UINT16();
				ushort loopLength = loopStart == 0 ? (ushort)0x0001 : (ushort)(length - loopStart);

				yield return new SampleInfo
				{
					Name = null,
					Length = length,
					LoopStart = loopStart,
					LoopLength = loopLength,
					Volume = 0x40,
					FineTune = 0
				};
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the position list. Note that the returned list must be the
		/// same length as to be played
		/// </summary>
		/********************************************************************/
		protected override Span<byte> GetPositionList(ModuleStream moduleStream)
		{
			// Since this format does not have any patterns, we need to recreate them
			// by ourselves. So each position will use a new pattern
			return Enumerable.Range(0, numberOfPositions).Select(x => (byte)x).ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			byte[] pattern = new byte[1024];

			int track1EmptyLinesLeft = 0, track1Pos = 0;
			int track2EmptyLinesLeft = 0, track2Pos = 0;

			for (int i = 0; i < numberOfPositions; i++)
			{
				// Clear the pattern data
				Array.Clear(pattern);

				FillTrack(pattern, 0, track1Data, ref track1Pos, ref track1EmptyLinesLeft);
				FillTrack(pattern, 8, track2Data, ref track2Pos, ref track2EmptyLinesLeft);

				yield return pattern;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Write all the samples
		/// </summary>
		/********************************************************************/
		protected override bool WriteSampleData(PlayerFileInfo fileInfo, ModuleStream moduleStream, ConverterStream converterStream)
		{
			moduleStream.Seek(0x96 + 2 + track1Data.Length + 2 + track2Data.Length, SeekOrigin.Begin);

			for (int i = 0; i < 31; i++)
			{
				int length = (int)sampleLengths[i];
				if (length != 0)
				{
					// Check to see if we miss too much from the last sample
					if (moduleStream.Length - moduleStream.Position < ((length / 2) - MaxNumberOfMissingBytes))
						return false;

					sbyte[] destinationBuffer = new sbyte[length + 1];

					// Skip sample length
					moduleStream.Seek(2, SeekOrigin.Current);

					// Decompress the sample
					sbyte sample = moduleStream.Read_INT8();
					destinationBuffer[0] = sample;

					for (int j = 1; j < length; )
					{
						byte nib1 = moduleStream.Read_UINT8();
						byte nib2 = (byte)(nib1 & 0x0f);
						nib1 >>= 4;

						if ((nib1 & 0x8) != 0)
							nib1 -= 0x10;

						sample += (sbyte)nib1;
						destinationBuffer[j++] = sample;

						if ((nib2 & 0x8) != 0)
							nib2 -= 0x10;

						sample += (sbyte)nib2;
						destinationBuffer[j++] = sample;
					}

					if ((moduleStream.Position % 2) != 0)
						moduleStream.Seek(1, SeekOrigin.Current);

					converterStream.Write(MemoryMarshal.Cast<sbyte, byte>(destinationBuffer.AsSpan(0, length)));
				}
			}

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Read a single track and return it
		/// </summary>
		/********************************************************************/
		private byte[] ReadTrack(ModuleStream moduleStream)
		{
			ushort length = moduleStream.Read_B_UINT16();

			byte[] track = new byte[length - 2];
			moduleStream.ReadInto(track, 0, track.Length);

			return track;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the total number of patterns lines the given track will
		/// use
		/// </summary>
		/********************************************************************/
		private int CalculateNumberOfLines(byte[] trackData)
		{
			int lines = 0;

			for (int i = 0; i < trackData.Length; i += 2)
			{
				byte cmd = trackData[i];
				if (cmd == 4)
					lines += trackData[i + 1];
			}

			return lines;
		}



		/********************************************************************/
		/// <summary>
		/// Fill out a whole pattern track
		/// </summary>
		/********************************************************************/
		private void FillTrack(byte[] pattern, int patternOffset, byte[] trackData, ref int trackOffset, ref int trackEmptyLinesLeft)
		{
			int effCount = 0;

			for (int i = 0; i < 64;)
			{
				if (trackEmptyLinesLeft > 0)
				{
					patternOffset += 16;
					trackEmptyLinesLeft--;
					i++;
				}
				else
				{
					byte cmd = trackData[trackOffset];
					byte arg = trackData[trackOffset + 1];

					if (cmd == 0)	// End of track
						break;

					trackOffset += 2;

					switch (cmd)
					{
						// Set volume
						case 1:
						{
							pattern[patternOffset + 2] |= 0x0c;
							pattern[patternOffset + 3] = arg;

							pattern[patternOffset + 6] |= 0x0c;
							pattern[patternOffset + 7] = arg;

							effCount++;
							break;
						}

						// Slide volume
						case 2:
						{
							pattern[patternOffset + 2] |= 0x0a;
							pattern[patternOffset + 3] = arg;

							pattern[patternOffset + 6] |= 0x0a;
							pattern[patternOffset + 7] = arg;

							effCount++;
							break;
						}

						// Set speed
						case 3:
						{
							if (effCount == 0)
							{
								pattern[patternOffset + 2] |= 0x0f;
								pattern[patternOffset + 3] = arg;
							}
							else
							{
								pattern[patternOffset + 6] |= 0x0f;
								pattern[patternOffset + 7] = arg;
							}

							effCount++;
							break;
						}

						// Advance rows
						case 4:
						{
							trackEmptyLinesLeft = arg;
							effCount = 0;
							break;
						}

						// Set note
						case 5:
						{
							byte byt1 = (byte)((arg & 0xf0) | ((period >> 8) & 0x0f));
							byte byt2 = (byte)(period & 0x00ff);
							byte byt3 = (byte)(arg << 4);

							pattern[patternOffset] = byt1;
							pattern[patternOffset + 1] = byt2;
							pattern[patternOffset + 2] |= byt3;

							pattern[patternOffset + 4] = byt1;
							pattern[patternOffset + 5] = byt2;
							pattern[patternOffset + 6] |= byt3;
							break;
						}
					}
				}
			}
		}
		#endregion
	}
}
