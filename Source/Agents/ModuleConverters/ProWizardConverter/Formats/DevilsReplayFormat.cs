/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// Devils Replay
	/// </summary>
	internal class DevilsReplayFormat : ProWizardForPcBase
	{
		private byte numberOfPositions;
		private byte restartPosition;

		private ushort patternSourceSize;
		private ushort referenceTableSize;
		private byte[] referenceTable;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT51;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x440)
				return false;

			// Start to check the mark
			moduleStream.Seek(0x438, SeekOrigin.Begin);
			if (moduleStream.ReadMark() != "M.K.")
				return false;

			// Check sample information
			moduleStream.Seek(20, SeekOrigin.Begin);

			for (int i = 0; i < 31; i++)
			{
				moduleStream.Seek(22, SeekOrigin.Current);

				ushort sampleSize = (ushort)(moduleStream.Read_B_UINT16() * 2);
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();
				ushort loopStart = (ushort)(moduleStream.Read_B_UINT16() * 2);
				ushort loopSize = (ushort)(moduleStream.Read_B_UINT16() * 2);

				if (!TestSample(sampleSize, loopStart, loopSize, volume, fineTune))
					return false;
			}

			// Find highest pattern offset
			if (FindHighestPatternNumber(moduleStream, 0x3b8, 128) > 64)
				return false;

			// Check the pattern data and reference table sizes
			moduleStream.Seek(0x43c, SeekOrigin.Begin);

			ushort temp = moduleStream.Read_B_UINT16();
			if ((temp == 0) || ((temp + 2 + 1084) > moduleStream.Length))
				return false;

			if ((numberOfPatterns * 192 + 4) != temp)
				return false;

			// Check the reference table size
			moduleStream.Seek(temp, SeekOrigin.Current);

			ushort temp1 = moduleStream.Read_B_UINT16();
			if ((temp1 == 0) || ((temp + temp1 + 2 + 1084 + 2) > moduleStream.Length))
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
			// Get number of positions and restart position
			moduleStream.Seek(0x3b6, SeekOrigin.Begin);

			numberOfPositions = moduleStream.Read_UINT8();
			restartPosition = moduleStream.Read_UINT8();

			// Get pattern sizes and reference table
			moduleStream.Seek(0x43c, SeekOrigin.Begin);
			patternSourceSize = moduleStream.Read_B_UINT16();

			moduleStream.Seek(patternSourceSize, SeekOrigin.Current);
			referenceTableSize = moduleStream.Read_B_UINT16();

			referenceTable = new byte[referenceTableSize];
			moduleStream.ReadInto(referenceTable, 0, referenceTableSize);

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
				byte[] name = new byte[22];
				moduleStream.ReadInto(name, 0, 22);

				ushort length = moduleStream.Read_B_UINT16();
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();
				ushort loopStart = moduleStream.Read_B_UINT16();
				ushort loopLength = moduleStream.Read_B_UINT16();

				yield return new SampleInfo
				{
					Name = name,
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

			moduleStream.Seek(0x03b8, SeekOrigin.Begin);
			moduleStream.ReadInto(positionList, 0, numberOfPositions);

			return positionList;
		}



		/********************************************************************/
		/// <summary>
		/// Return the restart position
		/// </summary>
		/********************************************************************/
		protected override byte GetRestartPosition(ModuleStream moduleStream)
		{
			return restartPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			moduleStream.Seek(0x43e, SeekOrigin.Begin);

			byte[] pattern = new byte[1024];
			int endOffset = 1084 + 2 + patternSourceSize;

			for (int i = 0; i < numberOfPatterns; i++)
			{
				// Clear the pattern data
				Array.Clear(pattern);

				for (int j = 0; j <  64; j++)
				{
					if (moduleStream.Position >= endOffset)
						break;

					// Read filter + address to reference table
					byte filter = moduleStream.Read_UINT8();
					ushort referenceOffset = moduleStream.Read_B_UINT16();

					// No note
					if (filter == 0x00)
						continue;

					// Go through all possible values of filter
					if ((filter & 0x80) != 0)
					{
						pattern[j * 16] = referenceTable[referenceOffset++];
						pattern[j * 16 + 1] = referenceTable[referenceOffset++];
					}

					if ((filter & 0x40) != 0)
					{
						pattern[j * 16 + 2] = referenceTable[referenceOffset++];
						pattern[j * 16 + 3] = referenceTable[referenceOffset++];
					}

					if ((filter & 0x20) != 0)
					{
						pattern[j * 16 + 4] = referenceTable[referenceOffset++];
						pattern[j * 16 + 5] = referenceTable[referenceOffset++];
					}

					if ((filter & 0x10) != 0)
					{
						pattern[j * 16 + 6] = referenceTable[referenceOffset++];
						pattern[j * 16 + 7] = referenceTable[referenceOffset++];
					}

					if ((filter & 0x08) != 0)
					{
						pattern[j * 16 + 8] = referenceTable[referenceOffset++];
						pattern[j * 16 + 9] = referenceTable[referenceOffset++];
					}

					if ((filter & 0x04) != 0)
					{
						pattern[j * 16 + 10] = referenceTable[referenceOffset++];
						pattern[j * 16 + 11] = referenceTable[referenceOffset++];
					}

					if ((filter & 0x02) != 0)
					{
						pattern[j * 16 + 12] = referenceTable[referenceOffset++];
						pattern[j * 16 + 13] = referenceTable[referenceOffset++];
					}

					if ((filter & 0x01) != 0)
					{
						pattern[j * 16 + 14] = referenceTable[referenceOffset];
						pattern[j * 16 + 15] = referenceTable[referenceOffset + 1];
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
		protected override bool WriteSampleData(PlayerFileInfo fileInfo, ModuleStream moduleStream, ConverterStream converterStream)
		{
			moduleStream.Seek(1084 + 2 + patternSourceSize + 2 + referenceTableSize, SeekOrigin.Begin);

			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion
	}
}
