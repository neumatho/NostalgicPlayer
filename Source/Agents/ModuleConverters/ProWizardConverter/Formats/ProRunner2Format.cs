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
	/// ProRunner 2
	/// </summary>
	internal class ProRunner2Format : ProWizardConverterWorkerBase
	{
		private byte numberOfPositions;
		private byte restartPosition;

		private ushort[] patternOffsets;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT32;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x284)
				return false;

			// Check sample information
			moduleStream.Seek(8, SeekOrigin.Begin);

			uint samplesSize = 0;

			for (int i = 0; i < 31; i++)
			{
				// Check sample size
				ushort temp1 = moduleStream.Read_B_UINT16();
				if (temp1 > 0x8000)
					return false;

				samplesSize += temp1 * 2U;

				// Check volume and fine tune
				if ((moduleStream.Read_UINT8() > 0x0f) || (moduleStream.Read_UINT8() > 0x40))
					return false;

				// Check loop values
				if (temp1 != 0)
				{
					uint temp2 = moduleStream.Read_B_UINT16();
					uint temp3 = moduleStream.Read_B_UINT16();

					if ((temp2 + temp3) > temp1)
						return false;
				}
				else
					moduleStream.Seek(4, SeekOrigin.Current);
			}

			// Check the module length
			moduleStream.Seek(4, SeekOrigin.Begin);

			uint sampleOffset = moduleStream.Read_B_UINT32();
			if ((sampleOffset + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
				return false;

			// Check number of positions
			moduleStream.Seek(0x100, SeekOrigin.Begin);

			byte positionListLength = moduleStream.Read_UINT8();
			if ((positionListLength > 0x7f) || (positionListLength == 0x00))
				return false;

			// Check NTK byte
			byte temp = moduleStream.Read_UINT8();
			if ((temp > 0x7f) && (temp != 0xff))
				return false;

			numberOfPatterns = 0;
			for (int i = 0; i < positionListLength; i++)
			{
				temp = moduleStream.Read_UINT8();
				if (temp > 0x3f)
					return false;

				if (temp > numberOfPatterns)
					numberOfPatterns = temp;
			}

			numberOfPatterns++;

			// Check pattern pointer table
			moduleStream.Seek(0x282, SeekOrigin.Begin);

			if (moduleStream.Read_B_UINT16() != 0)
				return false;

			if (numberOfPatterns >= 2)
			{
				for (int i = 0; i < numberOfPatterns - 1; i++)
				{
					moduleStream.Seek(-2, SeekOrigin.Current);

					// Get two offsets
					ushort temp1 = moduleStream.Read_B_UINT16();
					ushort temp2 = moduleStream.Read_B_UINT16();

					if (temp2 >= sampleOffset)
						return false;

					if (temp1 >= temp2)
						return false;
				}
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
			// Get number of positions and restart position
			moduleStream.Seek(0x100, SeekOrigin.Begin);

			numberOfPositions = moduleStream.Read_UINT8();
			restartPosition = moduleStream.Read_UINT8();

			// Load pattern offset table
			patternOffsets = new ushort[numberOfPatterns];

			moduleStream.Seek(0x282, SeekOrigin.Begin);
			moduleStream.ReadArray_B_UINT16s(patternOffsets, 0, numberOfPatterns);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			moduleStream.Seek(8, SeekOrigin.Begin);

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
			byte[] positionList = new byte[numberOfPositions];

			moduleStream.Seek(0x102, SeekOrigin.Begin);
			moduleStream.Read(positionList, 0, numberOfPositions);

			return positionList;
		}



		/********************************************************************/
		/// <summary>
		/// Return the restart position
		/// </summary>
		/********************************************************************/
		protected override byte GetRestartPosition(ModuleStream moduleStream)
		{
			return (byte)(restartPosition & 0x7f);
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			byte[] pattern = new byte[1024];
			byte[,] prevNote = new byte[4, 4];

			for (int i = 0; i < numberOfPatterns; i++)
			{
				// Clear pattern data
				Array.Clear(pattern);

				// Find pattern offset
				uint offset = 0x302U + patternOffsets[i];
				moduleStream.Seek(offset, SeekOrigin.Begin);

				for (int j = 0; j < 64; j++)
				{
					for (int k = 0; k < 4; k++)
					{
						// Get first byte
						byte temp = moduleStream.Read_UINT8();

						// Empty note?
						if (temp == 0x80)
							continue;

						// Copy previous row
						if (temp == 0xc0)
						{
							pattern[j * 16 + k * 4] = prevNote[k, 0];
							pattern[j * 16 + k * 4 + 1] = prevNote[k, 1];
							pattern[j * 16 + k * 4 + 2] = prevNote[k, 2];
							pattern[j * 16 + k * 4 + 3] = prevNote[k, 3];
							continue;
						}

						byte byt1 = 0;
						byte byt2 = 0;
						byte byt3 = 0;

						// Normal pattern line
						if ((temp & 0x1) != 0)
						{
							byt3 = 0x10;		// Low bit in sample number
							temp &= 0xfe;
						}

						// Convert note
						if (temp != 0)
						{
							temp -= 2;
							temp /= 2;
							byt1 = periods[temp, 0];
							byt2 = periods[temp, 1];
						}

						// Get sample number
						byte temp1 = moduleStream.Read_UINT8();

						temp = (byte)((temp1 & 0xf0) >> 3);
						if (temp >= 0x10)
						{
							byt1 |= 0x10;		// Hi bit in sample number
							temp -= 0x10;
						}

						byt3 |= (byte)(temp << 4);

						// Get the effect
						byt3 |= (byte)(temp1 & 0x0f);
						byte byt4 = moduleStream.Read_UINT8();

						// Store the line in the pattern
						pattern[j * 16 + k * 4] = byt1;
						pattern[j * 16 + k * 4 + 1] = byt2;
						pattern[j * 16 + k * 4 + 2] = byt3;
						pattern[j * 16 + k * 4 + 3] = byt4;

						// Remember the line
						if ((byt1 != 0) || (byt2 != 0) || (byt3 != 0) || (byt4 != 0))
						{
							prevNote[k, 0] = byt1;
							prevNote[k, 1] = byt2;
							prevNote[k, 2] = byt3;
							prevNote[k, 3] = byt4;
						}
					}
				}

				yield return pattern;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Write all the samples
		/// </summary>
		/********************************************************************/
		protected override bool WriteSampleData(ModuleStream moduleStream, ConverterStream converterStream)
		{
			moduleStream.Seek(4, SeekOrigin.Begin);
			uint sampleDataOffset = moduleStream.Read_B_UINT32();
			moduleStream.Seek(sampleDataOffset, SeekOrigin.Begin);

			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion
	}
}
