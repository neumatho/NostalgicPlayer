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
	/// SKYT Packer
	/// </summary>
	internal class SkytPackerFormat : ProWizardConverterWorker31SamplesBase
	{
		private byte numberOfPositions;
		private ushort[,] trackOffsetTable;
		private byte[] positionList;

		private ushort sampleOffset;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT34;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x106)
				return false;

			// Start to check the ID
			moduleStream.Seek(0x100, SeekOrigin.Begin);
			if (moduleStream.ReadMark() != "SKYT")
				return false;

			// Check sample information
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint samplesSize = 0;

			for (int i = 0; i < 31; i++)
			{
				// Check sample size
				ushort temp = moduleStream.Read_B_UINT16();
				if (temp >= 0x8000)
					return false;

				samplesSize += temp * 2U;

				// Check volume and fine tune
				if ((moduleStream.Read_UINT8() > 0x0f) || (moduleStream.Read_UINT8() > 0x40))
					return false;

				// Check loop values
				if (temp != 0)
				{
					uint temp1 = moduleStream.Read_B_UINT16();
					uint temp2 = moduleStream.Read_B_UINT16();

					if ((temp1 + temp2) > temp)
						return false;
				}
				else
					moduleStream.Seek(4, SeekOrigin.Current);
			}

			// Get number of patterns
			moduleStream.Seek(0x104, SeekOrigin.Begin);
			byte positionListLength = (byte)(moduleStream.Read_UINT8() + 1);

			// Check track numbers
			moduleStream.Seek(0x106, SeekOrigin.Begin);

			for (int i = 0; i < positionListLength; i++)
			{
				if ((moduleStream.Read_B_UINT16() & 0x00ff) != 0x0000)
					return false;
			}

			// Check the module length
			if ((0x106 + positionListLength * 2 * 4 + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
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
			moduleStream.Seek(0x104, SeekOrigin.Begin);
			numberOfPositions = (byte)(moduleStream.Read_UINT8() + 1);

			CreateOffsetTable(moduleStream);
			CreatePositionList();

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
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();
				ushort loopStart = moduleStream.Read_B_UINT16();
				ushort loopLength = moduleStream.Read_B_UINT16();

				yield return new SampleInfo
				{
					Name = null,
					Length = length,
					LoopStart = loopStart,
					LoopLength = loopLength != 0 ? loopLength : (ushort)1,
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
			ushort trackStartOffset = (ushort)(0x106 + numberOfPositions * 2 * 4);
			sampleOffset = 0;

			byte[] pattern = new byte[1024];
			int lastPatternNumber = -1;

			for (int i = 0; i < numberOfPositions; i++)
			{
				if (positionList[i] > lastPatternNumber)
				{
					lastPatternNumber++;

					// Clear the pattern data
					Array.Clear(pattern);

					// Build each channel from the tracks
					for (int j = 0; j < 4; j++)
					{
						ushort trackOffset = trackOffsetTable[j, i];
						if (trackOffset > sampleOffset)
							sampleOffset = trackOffset;

						if (trackOffset != 0)
							trackOffset -= 0x100;

						moduleStream.Seek(trackStartOffset + trackOffset, SeekOrigin.Begin);

						for (int k = 0; k < 64; k++)
						{
							byte byt1 = moduleStream.Read_UINT8();
							byte byt2 = moduleStream.Read_UINT8();
							byte byt3 = moduleStream.Read_UINT8();
							byte byt4 = moduleStream.Read_UINT8();

							// Convert sample number
							byte temp = (byte)(byt2 & 0x1f);
							if (temp >= 0x10)
							{
								pattern[k * 16 + j * 4] = 0x10;
								temp -= 0x10;
							}

							pattern[k * 16 + j * 4 + 2] = (byte)(temp << 4);

							// Convert note
							if (byt1 != 0)
							{
								byt1--;
								pattern[k * 16 + j * 4] |= periods[byt1, 0];
								pattern[k * 16 + j * 4 + 1] = periods[byt1, 1];
							}

							// Convert effect + effect value
							pattern[k * 16 + j * 4 + 2] |= (byte)(byt3 & 0x0f);
							pattern[k * 16 + j * 4 + 3] = byt4;
						}
					}

					yield return pattern;
				}
			}

			sampleOffset += trackStartOffset;
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
			moduleStream.Seek(0x106, SeekOrigin.Begin);
			trackOffsetTable = new ushort[4, numberOfPositions];

			for (int i = 0; i < numberOfPositions; i++)
			{
				for (int j = 0; j < 4; j++)
					trackOffsetTable[j, i] = moduleStream.Read_B_UINT16();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Create position list and find the number of patterns needed
		/// </summary>
		/********************************************************************/
		private void CreatePositionList()
		{
			positionList = BuildPositionList(numberOfPositions, (pos) =>
				((ulong)trackOffsetTable[0, pos] << 48) | ((ulong)trackOffsetTable[1, pos] << 32) | ((ulong)trackOffsetTable[2, pos] << 16) | trackOffsetTable[3, pos]);
		}
		#endregion
	}
}
