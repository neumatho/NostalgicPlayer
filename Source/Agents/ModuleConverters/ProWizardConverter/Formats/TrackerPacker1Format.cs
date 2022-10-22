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
	/// Tracker Packer 1
	/// </summary>
	internal class TrackerPacker1Format : ProWizardConverterWorker31SamplesBase
	{
		private uint sampleOffset;

		private byte numberOfPositions;
		private uint[] patternOffsetTable;
		private byte[] positionList;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT42;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x11a)
				return false;

			// Start to check the ID
			moduleStream.Seek(0, SeekOrigin.Begin);

			if (moduleStream.Read_B_UINT32() != 0x4d455858)		// MEXX
				return false;

			// Check the module length stored in the module
			uint temp = moduleStream.Read_B_UINT32();
			if ((temp == 0) || ((temp & 0x1) != 0))
				return false;

			// Check the module length
			if (temp > (moduleStream.Length + MaxNumberOfMissingBytes))
				return false;

			// Get the sample offset
			moduleStream.Seek(0x1c, SeekOrigin.Begin);

			temp = moduleStream.Read_B_UINT32();
			if ((temp == 0) || ((temp & 0x1) != 0))
				return false;

			// Check sample information
			uint samplesSize = 0;

			for (int i = 0; i < 31; i++)
			{
				ushort volume = moduleStream.Read_B_UINT16();

				// Check sample size
				ushort temp1 = moduleStream.Read_B_UINT16();
				if (temp1 == 0)
				{
					// Check loop length
					moduleStream.Seek(2, SeekOrigin.Current);

					if (moduleStream.Read_B_UINT16() != 0x0001)
						return false;
				}
				else
				{
					samplesSize += temp1 * 2U;

					// Check volume
					if (volume > 0x40)
						return false;

					moduleStream.Seek(4, SeekOrigin.Current);
				}
			}

			if (samplesSize == 0)
				return false;

			// Check size of pattern list
			if (moduleStream.Read_B_UINT16() == 0)
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
			moduleStream.Seek(0x1c, SeekOrigin.Begin);
			sampleOffset = moduleStream.Read_B_UINT32();

			CreateOffsetTable(moduleStream);
			CreatePositionList();

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

			moduleStream.Seek(8, SeekOrigin.Begin);
			moduleStream.Read(moduleName, 0, 20);

			return moduleName;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			moduleStream.Seek(0x20, SeekOrigin.Begin);

			for (int i = 0; i < 31; i++)
			{
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();
				ushort length = moduleStream.Read_B_UINT16();
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
			int lastPatternNumber = -1;

			for (int i = 0; i < positionList.Length; i++)
			{
				// Get pattern number to build
				if (positionList[i] > lastPatternNumber)
				{
					lastPatternNumber++;

					// Clear the pattern data
					Array.Clear(pattern);

					uint patternOffset = patternOffsetTable[i];
					moduleStream.Seek(0x31a + patternOffset, SeekOrigin.Begin);

					for (int j = 0; j < 64 * 4; j++)
					{
						// Get first byte
						byte temp = moduleStream.Read_UINT8();

						// Is it an empty note?
						if (temp == 0xc0)
							continue;

						// Effect only?
						if (temp >= 0x80)
						{
							pattern[j * 4 + 2] = (byte)((temp >> 2) & 0x0f);
							pattern[j * 4 + 3] = moduleStream.Read_UINT8();
							continue;
						}

						// Note + sample + effect
						if ((temp & 0x01) != 0)
						{
							pattern[j * 4] = 0x10;	// Hi bit in sample number
							temp &= 0xfe;
						}

						if (temp != 0)
						{
							// Got a note, convert it to a period
							temp -= 2;
							temp /= 2;
							pattern[j * 4] |= periods[temp, 0];
							pattern[j * 4 + 1] = periods[temp, 1];
						}

						// Store sample number + effect + effect value
						pattern[j * 4 + 2] = moduleStream.Read_UINT8();
						pattern[j * 4 + 3] = moduleStream.Read_UINT8();
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
			moduleStream.Seek(sampleOffset, SeekOrigin.Begin);

			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Create the track offset table
		/// </summary>
		/********************************************************************/
		private void CreateOffsetTable(ModuleStream moduleStream)
		{
			moduleStream.Seek(0x118, SeekOrigin.Begin);
			numberOfPositions = (byte)(moduleStream.Read_B_UINT16() + 1);

			patternOffsetTable = new uint[numberOfPositions];

			uint startOffset = uint.MaxValue;

			for (int i = 0; i < numberOfPositions; i++)
			{
				uint temp = moduleStream.Read_B_UINT32();
				if (startOffset > temp)
					startOffset = temp;

				patternOffsetTable[i] = temp;
			}

			for (int i = 0; i < numberOfPositions; i++)
				patternOffsetTable[i] -= startOffset;
		}



		/********************************************************************/
		/// <summary>
		/// Create position list and find the number of patterns needed
		/// </summary>
		/********************************************************************/
		private void CreatePositionList()
		{
			positionList = BuildPositionList(numberOfPositions, (pos) => patternOffsetTable[pos]);
		}
		#endregion
	}
}
