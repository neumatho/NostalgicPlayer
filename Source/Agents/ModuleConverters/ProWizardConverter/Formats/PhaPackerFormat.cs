/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// PhaPacker
	/// </summary>
	internal class PhaPackerFormat : ProWizardConverterWorker31SamplesBase
	{
		private uint endOffset;

		private byte numberOfPositions;
		private List<uint> patternOffsets;
		private byte[] positionList;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT20;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x3c0)
				return false;

			// Check the first sample address
			moduleStream.Seek(8, SeekOrigin.Begin);

			if (moduleStream.Read_B_UINT32() != 0x3c0)
				return false;

			// Check number of positions
			moduleStream.Seek(0x1b4, SeekOrigin.Begin);

			ushort temp = moduleStream.Read_B_UINT16();
			if ((temp == 0) || (temp > 0x200) || ((temp & 0x3) != 0))
				return false;

			// Check sample information
			moduleStream.Seek(0, SeekOrigin.Begin);

			for (int i = 0; i < 31; i++)
			{
				// Get sample size
				temp = moduleStream.Read_B_UINT16();

				// Check volume
				moduleStream.Seek(1, SeekOrigin.Current);
				if (moduleStream.Read_UINT8() > 0x40)
					return false;

				// Check sample size
				ushort temp1 = (ushort)(moduleStream.Read_B_UINT16() + moduleStream.Read_B_UINT16());

				// Check loop
				if (temp == 0)
				{
					if (temp1 != 0x0001)
						return false;
				}
				else
				{
					if ((temp1 == 0) || (temp1 > temp))
						return false;
				}

				// Check fine tune
				moduleStream.Seek(4, SeekOrigin.Current);

				temp = moduleStream.Read_B_UINT16();
				if ((temp % 0x48) != 0)
					return false;
			}

			// Find highest pattern offset
			moduleStream.Seek(0x1c0, SeekOrigin.Begin);

			uint offset = 0;

			for (int i = 0; i < 128; i++)
			{
				uint temp2 = moduleStream.Read_B_UINT32();
				if (temp2 > offset)
					offset = temp2;
			}

			// Check last pattern and find end of module
			short temp3 = 0;

			for (int i = 0; i < 64 * 4; i++)
			{
				temp3++;
				if (temp3 >= 0)
				{
					// Read next row
					offset += 4;

					moduleStream.Seek(offset, SeekOrigin.Begin);
					temp = moduleStream.Read_B_UINT16();

					if (temp >= 0x8000)
					{
						// Get new counter value
						offset += 2;
						temp3 = unchecked((short)temp);

						if ((temp3 & 0xff00) == 0xff00)
							continue;

						if (i != (64 * 4 - 1))
							return false;

						offset -= 2;
					}
				}
			}

			// Check the module length
			if (offset > moduleStream.Length)
				return false;

			// Remember the end offset
			endOffset = offset;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Prepare conversion by initialize what is needed etc.
		/// </summary>
		/********************************************************************/
		protected override bool PrepareConversion(ModuleStream moduleStream)
		{
			CreatePositionList(moduleStream);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			moduleStream.Seek(0, SeekOrigin.Begin);

			for (int i = 0; i < 31; i++)
			{
				ushort length = moduleStream.Read_B_UINT16();
				moduleStream.Seek(1, SeekOrigin.Current);
				byte volume = moduleStream.Read_UINT8();
				ushort loopStart = moduleStream.Read_B_UINT16();
				ushort loopLength = moduleStream.Read_B_UINT16();
				moduleStream.Seek(4, SeekOrigin.Current);
				byte fineTune = (byte)(moduleStream.Read_B_UINT16() / 0x48);

				yield return new SampleInfo
				{
					Name = null,
					Length = length,
					LoopStart = loopStart,
					LoopLength = loopLength,
					Volume = volume,
					FineTune = fineTune
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
			return positionList;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			byte[] pattern = new byte[1024];

			sbyte lastPattern = -1;
			for (int i = 0; i < numberOfPositions; i++)
			{
				// Get pattern to build
				if (positionList[i] > lastPattern)
				{
					lastPattern++;

					// Get pattern offset
					uint offset = patternOffsets[lastPattern];
					if (offset == 0)
						break;

					// Clear pattern
					Array.Clear(pattern);

					// Convert the pattern
					moduleStream.Seek(offset, SeekOrigin.Begin);
					uint writeOffset = 0;

					for (int j = 0; j < 256; j++)
					{
						if (moduleStream.Position >= endOffset)
							break;

						byte byt1, byt2, byt3, byt4;

						byte temp = moduleStream.Read_UINT8();

						// Check for repeat command
						if (temp == 0xff)
						{
							// Get the number of lines to repeat
							sbyte lines = (sbyte)-moduleStream.Read_INT8();

							// Adjust counter
							j--;

							// Get pattern data
							uint tempOffset = writeOffset - 4;
							byt1 = pattern[tempOffset];
							byt2 = pattern[tempOffset + 1];
							byt3 = pattern[tempOffset + 2];
							byt4 = pattern[tempOffset + 3];

							if ((byt1 == 0x00) && (byt2 == 0x00) && (byt3 == 0x00) && (byt4 == 0x00))
							{
								byt1 = 0xee;
								byt2 = 0xee;
								byt3 = 0xee;
								byt4 = 0xee;
							}

							// Write the pattern data
							for (int k = 0; k < lines; k++)
							{
								pattern[tempOffset] = byt1;
								pattern[tempOffset + 1] = byt2;
								pattern[tempOffset + 2] = byt3;
								pattern[tempOffset + 3] = byt4;

								tempOffset += 16;
							}
						}
						else
						{
							// Just a normal note, but convert it first
							//
							// Get sample
							byt3 = temp;
							if (byt3 >= 0x10)
							{
								byt1 = 0x10;
								byt3 -= 0x10;
							}
							else
								byt1 = 0x00;

							byt3 <<= 4;

							// Get note
							byt2 = moduleStream.Read_UINT8();
							if (byt2 != 0)
							{
								byt2 -= 2;
								byt2 /= 2;

								byt1 |= periods[byt2, 0];
								byt2 = periods[byt2, 1];
							}

							// Get effect
							byt3 |= (byte)(moduleStream.Read_UINT8() & 0x0f);
							byt4 = moduleStream.Read_UINT8();

							// Fix position jump
							if ((byt3 & 0x0f) == 0x0b)
								byt4++;

							// Fix for "Raw 3 - Intro"
							if (((byt3 & 0x0f) == 0x0f) && (byt4 == 0x20))
								byt4 = 0x1f;

							// Fix for "What Mag"
							if (((byt3 & 0x0f) == 0x0f) && (byt4 == 0x4f))
							{
								byt3 &= 0xf0;
								byt4 = 0;
							}

							// Find an empty row
							while ((pattern[writeOffset] != 0x00) || (pattern[writeOffset + 1] != 0x00) || (pattern[writeOffset + 2] != 0x00) || (pattern[writeOffset + 3] != 0x00))
							{
								writeOffset += 4;

								j++;
								if (j == 256)
									break;
							}

							if (j == 256)
								break;

							// Write the pattern data
							pattern[writeOffset++] = byt1;
							pattern[writeOffset++] = byt2;
							pattern[writeOffset++] = byt3;
							pattern[writeOffset++] = byt4;
						}
					}

					// Convert special 0xeeeeeeee to empty lines
					for (int j = 0; j < 256; j++)
					{
						if ((pattern[j * 4] == 0xee) && (pattern[j * 4 + 1] == 0xee) && (pattern[j * 4 + 2] == 0xee) && (pattern[j * 4 + 3] == 0xee))
						{
							pattern[j * 4] = 0x00;
							pattern[j * 4 + 1] = 0x00;
							pattern[j * 4 + 2] = 0x00;
							pattern[j * 4 + 3] = 0x00;
						}
					}

					yield return pattern;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Write all the samples
		/// </summary>
		/********************************************************************/
		protected override bool WriteSampleData(ModuleStream moduleStream, ConverterStream converterStream)
		{
			moduleStream.Seek(0x3c0, SeekOrigin.Begin);

			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Create position list and find the number of patterns needed
		/// </summary>
		/********************************************************************/
		private void CreatePositionList(ModuleStream moduleStream)
		{
			moduleStream.Seek(0x1b4, SeekOrigin.Begin);
			numberOfPositions = (byte)(moduleStream.Read_B_UINT16() / 4);

			// Begin to create the position table
			moduleStream.Seek(0x1c0, SeekOrigin.Begin);

			positionList = BuildPositionList(numberOfPositions, out patternOffsets, (pos) => moduleStream.Read_B_UINT32());
		}
		#endregion
	}
}
