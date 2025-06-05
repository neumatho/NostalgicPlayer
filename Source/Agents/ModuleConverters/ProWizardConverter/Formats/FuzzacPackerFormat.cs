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
	/// Fuzzac Packer
	/// </summary>
	internal class FuzzacPackerFormat : ProWizardConverterWorker31SamplesBase
	{
		private byte numberOfPositions;
		private ushort[,] trackOffsetTable;
		private byte[] positionList;

		private uint notesOffset;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT9;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x84a)
				return false;

			// Check mark
			moduleStream.Seek(0, SeekOrigin.Begin);

			if (moduleStream.ReadMark() != "M1.0")
				return false;

			// Check sample information
			moduleStream.Seek(6, SeekOrigin.Begin);

			uint samplesSize = 0;

			for (int i = 0; i < 31; i++)
			{
				moduleStream.Seek(60, SeekOrigin.Current);

				uint temp = moduleStream.Read_B_UINT16();
				if (temp >= 0x8000)
					return false;

				if (temp != 0)
				{
					samplesSize += temp * 2;

					// Check loop and loop length
					uint temp1 = moduleStream.Read_B_UINT16();
					uint temp2 = moduleStream.Read_B_UINT16();

					if ((temp1 + temp2) > temp)
						return false;

					// Check volume
					if (moduleStream.Read_B_UINT16() > 0x40)
						return false;
				}
				else
					moduleStream.Seek(6, SeekOrigin.Current);
			}

			// Check position list size
			byte positionListLength = moduleStream.Read_UINT8();
			if ((positionListLength == 0x00) || (positionListLength > 0x7f))
				return false;

			// Get max track offset
			ushort offset = moduleStream.Read_B_UINT16();
			if (offset == 0)
				return false;

			// Scan all the track offsets and find the highest one
			moduleStream.Seek(0x846, SeekOrigin.Begin);
			ushort maxOffset = 0;

			for (int i = 0; i < positionListLength * 4; i++)
			{
				ushort temp1 = moduleStream.Read_B_UINT16();
				if (temp1 > maxOffset)
					maxOffset = temp1;

				moduleStream.Seek(2, SeekOrigin.Current);
			}

			// Do the offset table have the same highest offset as stored in the module?
			if ((maxOffset + 0x100) != offset)
				return false;

			uint notes = (uint)moduleStream.Position - 0x40;
			uint samples = notes + offset + 4;

			// Check the end mark
			moduleStream.Seek(samples - 4, SeekOrigin.Begin);
			if (moduleStream.ReadMark() != "SEnd")
				return false;

			// Check first pattern
			moduleStream.Seek(notes + 0x40, SeekOrigin.Begin);

			for (int i = 0; i < 64; i++)
			{
				uint temp1 = (moduleStream.Read_B_UINT32() >> 16) & 0x0fff;
				if (temp1 == 0)
					continue;

				if ((temp1 < 0x71) || (temp1 > 0x358))
					return false;
			}

			// Check the module length
			if ((samples + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
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
			moduleStream.Seek(6, SeekOrigin.Begin);

			for (int i = 0; i < 31; i++)
			{
				byte[] name = new byte[60];
				moduleStream.ReadInto(name, 0, 60);

				ushort length = moduleStream.Read_B_UINT16();
				ushort loopStart = moduleStream.Read_B_UINT16();
				ushort loopLength = moduleStream.Read_B_UINT16();
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();

				yield return new SampleInfo
				{
					Name = name,
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
			byte[] pattern = new byte[1024];
			int lastPatternNumber = -1;

			for (int i = 0; i < numberOfPositions; i++)
			{
				if (positionList[i] > lastPatternNumber)
				{
					lastPatternNumber++;

					// Build each channel from the tracks
					for (int j = 0; j < 4; j++)
					{
						moduleStream.Seek(notesOffset + trackOffsetTable[j, i], SeekOrigin.Begin);

						// Copy the track data
						for (int k = 0; k < 64; k++)
						{
							pattern[j * 4 + k * 16] = moduleStream.Read_UINT8();
							pattern[j * 4 + k * 16 + 1] = moduleStream.Read_UINT8();
							pattern[j * 4 + k * 16 + 2] = moduleStream.Read_UINT8();
							pattern[j * 4 + k * 16 + 3] = moduleStream.Read_UINT8();
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
			moduleStream.Seek(0x843, SeekOrigin.Begin);
			uint sampleDataOffset = moduleStream.Read_B_UINT16();
			moduleStream.Seek(notesOffset + sampleDataOffset + 4, SeekOrigin.Begin);

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
			moduleStream.Seek(0x842, SeekOrigin.Begin);
			numberOfPositions = moduleStream.Read_UINT8();

			moduleStream.Seek(0x846, SeekOrigin.Begin);
			trackOffsetTable = new ushort[4, numberOfPositions];

			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < numberOfPositions; j++)
				{
					trackOffsetTable[i, j] = moduleStream.Read_B_UINT16();
					moduleStream.Seek(2, SeekOrigin.Current);
				}
			}

			notesOffset = (uint)moduleStream.Position - 0x40;
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
