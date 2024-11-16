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
	/// StarTrekker Packer
	/// </summary>
	internal class StarTrekkerPackerFormat : ProWizardConverterWorker31SamplesBase
	{
		private byte numberOfPositions;
		private uint[] patternOffsetTable;
		private byte[] positionList;

		private uint sampleOffset;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT35;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x314)
				return false;

			// Check the last 2 samples if they are empty (?!?)
			moduleStream.Seek(0xfc, SeekOrigin.Begin);

			for (int i = 0; i < 2; i++)
			{
				// Check length, fine tune and volume
				if (moduleStream.Read_B_UINT32() != 0)
					return false;

				// Check loop
				if (moduleStream.Read_B_UINT32() != 0x00000001)
					return false;
			}

			// Check the size of the position table
			ushort temp = moduleStream.Read_B_UINT16();
			if ((temp == 0) || ((temp & 0x3) != 0))
				return false;

			// Check module name
			moduleStream.Seek(0, SeekOrigin.Begin);

			for (int i = 0; i < 20; i++)
			{
				byte temp1 = moduleStream.Read_UINT8();
				if ((temp1 != 0x00) && (temp1 < 0x20))
					return false;
			}

			// Check sample information
			uint samplesSize = 0;

			for (int i = 0; i < 31; i++)
			{
				// Check sample size
				temp = moduleStream.Read_B_UINT16();
				if (temp >= 0x8000)
					return false;

				// Check volume and fine tune
				if ((moduleStream.Read_UINT8() > 0x0f) || (moduleStream.Read_UINT8() > 0x40))
					return false;

				// Check loop values
				uint temp1 = moduleStream.Read_B_UINT16();
				uint temp2 = moduleStream.Read_B_UINT16();

				if (temp == 0)
				{
					if ((temp1 + temp2) != 0x0001)
						return false;
				}
				else
				{
					if ((temp1 + temp2) > temp)
						return false;

					samplesSize += temp * 2U;
				}
			}

			// Check sample offset
			moduleStream.Seek(0x310, SeekOrigin.Begin);

			uint temp4 = moduleStream.Read_B_UINT32();
			if (temp4 == 0)
				return false;

			// Check the module length
			if ((0x314 + temp4 + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
				return false;

			// Check the first 64 notes
			for (int i = 0; i < 64; i++)
			{
				if (moduleStream.Read_UINT8() != 0x80)
				{
					// Get the period
					moduleStream.Seek(-1, SeekOrigin.Current);
					temp = (ushort)(moduleStream.Read_B_UINT16() & 0x0fff);
					if ((temp != 0) && ((temp < 0x71) || (temp > 0x358)))
						return false;

					moduleStream.Seek(2, SeekOrigin.Current);
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
			moduleStream.Seek(0x10c, SeekOrigin.Begin);
			numberOfPositions = (byte)(moduleStream.Read_B_UINT16() / 4);

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
			moduleStream.Seek(20, SeekOrigin.Begin);

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
			return positionList;
		}



		/********************************************************************/
		/// <summary>
		/// Return the restart position
		/// </summary>
		/********************************************************************/
		protected override byte GetRestartPosition(ModuleStream moduleStream)
		{
			return 0x00;
		}



		/********************************************************************/
		/// <summary>
		/// Return the ID mark
		/// </summary>
		/********************************************************************/
		protected override uint GetMark()
		{
			return 0x464c5434;		// FLT4
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			moduleStream.Seek(0x310, SeekOrigin.Begin);
			sampleOffset = 0x314 + moduleStream.Read_B_UINT32();

			byte[] pattern = new byte[1024];
			int lastPatternNumber = -1;

			for (int i = 0; i < numberOfPositions; i++)
			{
				if (positionList[i] > lastPatternNumber)
				{
					lastPatternNumber++;

					// Clear the pattern data
					Array.Clear(pattern);

					// Get pattern offset
					uint patternOffset = patternOffsetTable[i];
					moduleStream.Seek(0x314 + patternOffset, SeekOrigin.Begin);

					// Build each channel from the tracks
					for (int j = 0; j < 64 * 4; j++)
					{
						// Out of range?
						if (moduleStream.Position >= sampleOffset)
							break;

						// Empty line?
						byte temp1 = moduleStream.Read_UINT8();
						if (temp1 == 0x80)
							continue;

						// Normal line
						byte temp2 = moduleStream.Read_UINT8();
						byte temp3 = moduleStream.Read_UINT8();
						byte temp4 = moduleStream.Read_UINT8();

						// Divide the sample number with 4
						byte byt3 = (byte)(((temp3 >> 4) | temp1 & 0xf0) / 4);
						byte byt1 = (byte)(byt3 & 0xf0);
						byt3 <<= 4;

						// Copy the pattern data
						pattern[j * 4] = (byte)((temp1 & 0x0f) | byt1);
						pattern[j * 4 + 1] = temp2;
						pattern[j * 4 + 2] = (byte)((temp3 & 0x0f) | byt3);
						pattern[j * 4 + 3] = temp4;
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
			moduleStream.Seek(0x110, SeekOrigin.Begin);

			patternOffsetTable = new uint[numberOfPositions];
			moduleStream.ReadArray_B_UINT32s(patternOffsetTable, 0, numberOfPositions);
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
