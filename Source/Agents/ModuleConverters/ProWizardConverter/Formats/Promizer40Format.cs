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
	/// Promizer 4.0
	/// </summary>
	internal class Promizer40Format : ProWizardConverterWorker31SamplesBase
	{
		private byte numberOfPositions;
		private byte[] positionList;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT27;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x204)
				return false;

			// Check the ID
			moduleStream.Seek(0, SeekOrigin.Begin);
			if (moduleStream.Read_B_UINT32() != 0x504d3430)		// PM40
				return false;

			// Check size of position table
			if (moduleStream.Read_B_UINT32() > 0x7f)
				return false;

			// Check position table
			for (int i = 0; i < 128; i++)
			{
				if ((moduleStream.Read_B_UINT16() & 0x1) != 0)
					return false;
			}

			// Check sample information
			uint samplesSize = 0;

			for (int i = 0; i < 31; i++)
			{
				// Get sample size
				ushort temp = moduleStream.Read_B_UINT16();
				if (temp >= 0x8000)
					return false;

				if (temp == 0)
				{
					temp += moduleStream.Read_B_UINT16();
					temp += moduleStream.Read_B_UINT16();
					temp += moduleStream.Read_B_UINT16();
					if (temp != 0)
						return false;
				}
				else
				{
					samplesSize += temp * 2U;

					// Check volume and fine tune
					if ((moduleStream.Read_UINT8() > 0x0f) || (moduleStream.Read_UINT8() > 0x40))
						return false;

					moduleStream.Seek(4, SeekOrigin.Current);
				}
			}

			// Check the module length
			uint temp1 = moduleStream.Read_B_UINT32();
			if ((temp1 + 4 + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
				return false;

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
			moduleStream.Seek(0x108, SeekOrigin.Begin);

			for (int i = 0; i < 31; i++)
			{
				ushort length = moduleStream.Read_B_UINT16();
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();
				ushort loopStart = moduleStream.Read_B_UINT16();
				ushort loopLength = moduleStream.Read_B_UINT16();

				yield return new SampleInfo
				{
					Name = null,
					Length = length,
					LoopStart = loopStart,
					LoopLength = loopLength == 0 ? (ushort)0x0001 : loopLength,
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

			// Read offset table
			ushort[] patternOffsets = new ushort[128];
			moduleStream.Seek(0x8, SeekOrigin.Begin);
			moduleStream.ReadArray_B_UINT16s(patternOffsets, 0, 128);

			// Find all the offsets needed
			moduleStream.Seek(0x200, SeekOrigin.Begin);
			uint sampleOffset = moduleStream.Read_B_UINT32() + 4;
			uint realNotesOffset = moduleStream.Read_B_UINT32() + 4;
			uint notesTable = 0x208;
			uint noteEnd = sampleOffset;

			// Read note offset table
			ushort[] singleNoteOffsets = new ushort[(realNotesOffset - notesTable) / 2];
			moduleStream.Seek(notesTable, SeekOrigin.Begin);
			moduleStream.ReadArray_B_UINT16s(singleNoteOffsets, 0, singleNoteOffsets.Length);

			// Convert the pattern data
			sbyte lastPattern = -1;

			for (int i = 0; i < numberOfPositions; i++)
			{
				if (positionList[i] > lastPattern)
				{
					lastPattern++;

					// Clear the pattern data
					Array.Clear(pattern);

					// Find offset to pattern
					ushort offset = (ushort)(patternOffsets[i] / 2);

					for (int j = 0; j < 64; j++)
					{
						bool breakFlag = false;

						for (int k = 0; k < 4; k++)
						{
							if ((notesTable + offset * 2) < noteEnd)
							{
								byte byt1;

								uint patternOffset = singleNoteOffsets[offset++];

								moduleStream.Seek(realNotesOffset + patternOffset * 4, SeekOrigin.Begin);

								// Get sample number
								byte byt3 = moduleStream.Read_UINT8();
								if (byt3 >= 0x10)
								{
									byt3 -= 0x10;
									byt1 = 0x10;
								}
								else
									byt1 = 0x00;

								byt3 <<= 4;

								// Get note
								byte byt2 = moduleStream.Read_UINT8();
								if (byt2 != 0)
								{
									byt2--;

									byt1 |= periods[byt2, 0];
									byt2 = periods[byt2, 1];
								}

								// Get effect
								byt3 |= (byte)(moduleStream.Read_UINT8() & 0x0f);
								byte byt4 = moduleStream.Read_UINT8();

								// Store the pattern data
								pattern[j * 16 + k * 4] = byt1;
								pattern[j * 16 + k * 4 + 1] = byt2;
								pattern[j * 16 + k * 4 + 2] = byt3;
								pattern[j * 16 + k * 4 + 3] = byt4;

								// Have we reached a pattern stop effect
								byt3 &= 0x0f;
								if ((byt3 == 0x0d) || (byt3 == 0x0b))
									breakFlag = true;
							}
						}

						// We have reached the end of the pattern
						if (breakFlag)
							break;
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
			moduleStream.Seek(0x200, SeekOrigin.Begin);
			uint sampleDataOffset = moduleStream.Read_B_UINT32();
			moduleStream.Seek(sampleDataOffset + 4, SeekOrigin.Begin);

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
			moduleStream.Seek(7, SeekOrigin.Begin);
			numberOfPositions = moduleStream.Read_UINT8();

			positionList = BuildPositionList(numberOfPositions, (pos) => moduleStream.Read_B_UINT16());
		}
		#endregion
	}
}
