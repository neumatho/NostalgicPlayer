/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// Zen Packer
	/// </summary>
	internal class ZenPackerFormat : ProWizardConverterWorker31SamplesBase
	{
		private byte numberOfPositions;
		private byte[] positionList;

		private uint[] newPatternOffsets;
		private uint[] newSampleAddresses;
		private uint[] newSampleLoopAddresses;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT48;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x1f6)
				return false;

			// Check the pattern offset
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint temp = moduleStream.Read_B_UINT32();
			if (((temp & 0x1) != 0) || (temp > 0x200000))
				return false;

			// Check number of patterns
			numberOfPatterns = moduleStream.Read_UINT8();
			if (numberOfPatterns > 0x3f)
				return false;

			numberOfPatterns++;

			// Check number of positions
			byte positionListLength = moduleStream.Read_UINT8();
			if (positionListLength > 0x7f)
				return false;

			// Check the first sample pointer
			temp += positionListLength * 4U + 4U;

			moduleStream.Seek(14, SeekOrigin.Begin);
			if (moduleStream.Read_B_UINT32() != temp)
				return false;

			// Check sample information
			moduleStream.Seek(6, SeekOrigin.Begin);

			uint samplesSize = 0;
			uint previousAddress = 0;

			for (int i = 0; i < 31; i++)
			{
				// Check fine tune
				ushort temp1 = moduleStream.Read_B_UINT16();
				if ((temp1 % 0x48) != 0)
					return false;

				// Check volume
				temp1 = moduleStream.Read_B_UINT16();
				if (temp1 > 0x40)
					return false;

				// Check sample size
				temp1 = moduleStream.Read_B_UINT16();
				if (temp1 >= 0x8000)
					return false;

				if (temp1 == 0)
				{
					// Check loop length
					temp1 = moduleStream.Read_B_UINT16();
					if (temp1 != 0x0001)
						return false;
				}
				else
				{
					samplesSize += temp1 * 2U;
					moduleStream.Seek(2, SeekOrigin.Current);
				}

				// Check addresses
				temp = moduleStream.Read_B_UINT32();

				if (moduleStream.Read_B_UINT32() < temp)
					return false;

				if (previousAddress > temp)
					return false;

				previousAddress = temp;
			}

			// Find 0xffffffff
			uint temp2 = FindEndOfTables(moduleStream);
			if (temp2 == 0)
				return false;

			// Found the end mark, check the last pattern line
			uint tableOffset = temp2 - positionListLength * 4U;

			moduleStream.Seek(tableOffset - 4, SeekOrigin.Begin);
			temp = moduleStream.Read_B_UINT32();
			if (temp != 0xff000000)
				return false;

			// Check module length
			if ((temp2 + 4 + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
				return false;

			// Check position numbers in first pattern
			moduleStream.Seek(0x1f6, SeekOrigin.Begin);

			byte lineNumber = moduleStream.Read_UINT8();
			if (lineNumber != 0xff)
			{
				byte temp3;

				moduleStream.Seek(3, SeekOrigin.Current);

				while ((temp3 = moduleStream.Read_UINT8()) != 0xff)
				{
					if (temp3 <= lineNumber)
						return false;

					lineNumber = temp3;
					moduleStream.Seek(3, SeekOrigin.Current);
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
			moduleStream.Seek(5, SeekOrigin.Begin);
			numberOfPositions = moduleStream.Read_UINT8();

			uint temp = FindEndOfTables(moduleStream);
			uint tableOffset = temp - numberOfPositions * 4U;

			temp = numberOfPatterns;

			CreateOffsetTables(moduleStream, tableOffset);
			CreatePositionList();

			if (numberOfPatterns != temp)
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			moduleStream.Seek(0x6, SeekOrigin.Begin);

			for (int i = 0; i < 31; i++)
			{
				byte fineTune = (byte)(moduleStream.Read_B_UINT16() / 0x48);
				byte volume = (byte)moduleStream.Read_B_UINT16();
				ushort length = moduleStream.Read_B_UINT16();
				ushort loopLength = moduleStream.Read_B_UINT16();
				moduleStream.Seek(8, SeekOrigin.Current);

				yield return new SampleInfo
				{
					Name = null,
					Length = length,
					LoopStart = (ushort)((newSampleLoopAddresses[i] - newSampleAddresses[i]) / 2),
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

			for (int i = 0; i < numberOfPositions; i++)
			{
				if (positionList[i] > lastPatternNumber)
				{
					lastPatternNumber++;

					// Clear the pattern data
					Array.Clear(pattern);

					moduleStream.Seek(newPatternOffsets[i], SeekOrigin.Begin);

					for (;;)
					{
						uint line = moduleStream.Read_UINT8() * 4U;

						// Get note number
						byte temp = moduleStream.Read_UINT8();

						// Is hi bit set in sample number?
						if ((temp & 0x1) != 0)
						{
							pattern[line] = 0x10;
							temp &= 0xfe;
						}

						if (temp != 0)
						{
							temp -= 2;
							temp /= 2;

							pattern[line] |= periods[temp, 0];
							pattern[line + 1] = periods[temp, 1];
						}

						// Copy sample number + effect + effect value
						pattern[line + 2] = moduleStream.Read_UINT8();
						pattern[line + 3] = moduleStream.Read_UINT8();

						if (line == 0xff * 4)
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
			moduleStream.Seek(newSampleAddresses[0], SeekOrigin.Begin);

			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will scan the tables to find the end of them and return that
		/// position
		/// </summary>
		/********************************************************************/
		private uint FindEndOfTables(ModuleStream moduleStream)
		{
			// Find 0xffffffff
			uint temp = 0x1f6;

			for (;;)
			{
				moduleStream.Seek(temp, SeekOrigin.Begin);
				if (moduleStream.Read_B_UINT32() == 0xffffffff)
					break;

				temp += 2;
				if (temp > moduleStream.Length)
					return 0;
			}

			return temp;
		}



		/********************************************************************/
		/// <summary>
		/// Create offset tables
		/// </summary>
		/********************************************************************/
		private void CreateOffsetTables(ModuleStream moduleStream, uint tableOffset)
		{
			uint baseOffset = FindBaseOffset(moduleStream, tableOffset);

			// Build new pattern offset
			moduleStream.Seek(tableOffset, SeekOrigin.Begin);

			newPatternOffsets = new uint[numberOfPositions];

			for (int i = 0; i < numberOfPositions; i++)
				newPatternOffsets[i] = moduleStream.Read_B_UINT32() - baseOffset;

			// Build new sample addresses
			moduleStream.Seek(6, SeekOrigin.Begin);

			newSampleAddresses = new uint[31];
			newSampleLoopAddresses = new uint[31];

			for (int i = 0; i < 31; i++)
			{
				moduleStream.Seek(8, SeekOrigin.Current);

				newSampleAddresses[i] = moduleStream.Read_B_UINT32() - baseOffset;
				newSampleLoopAddresses[i] = moduleStream.Read_B_UINT32() - baseOffset;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Find base offset
		/// </summary>
		/********************************************************************/
		private uint FindBaseOffset(ModuleStream moduleStream, uint tableOffset)
		{
			moduleStream.Seek(tableOffset, SeekOrigin.Begin);

			uint baseOffset = moduleStream.Read_B_UINT32();
			for (int i = 1; i < numberOfPositions; i++)
			{
				uint temp = moduleStream.Read_B_UINT32();
				if (temp < baseOffset)
					baseOffset = temp;
			}

			baseOffset -= 0x1f6;

			return baseOffset;
		}



		/********************************************************************/
		/// <summary>
		/// Create position list and find the number of patterns needed
		/// </summary>
		/********************************************************************/
		private void CreatePositionList()
		{
			positionList = BuildPositionList(numberOfPositions, (pos) => newPatternOffsets[pos]);
		}
		#endregion
	}
}
