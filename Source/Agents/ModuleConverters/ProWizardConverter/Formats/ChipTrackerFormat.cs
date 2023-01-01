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
	/// ChipTracker / Kris Packer
	/// </summary>
	internal class ChipTrackerFormat : ProWizardConverterWorker31SamplesBase
	{
		// Period table with extra octaves
		private static readonly ushort[] chipPeriods =
		{
			3424, 3232, 3048, 2880, 2712, 2560, 2416, 2280, 2152, 2032, 1920, 1812,
			1712, 1616, 1524, 1440, 1356, 1280, 1208, 1140, 1076, 1016,  960,  906,
			 856,  808,  762,  720,  678,  640,  604,  570,  538,  508,  480,  453,
			 428,  404,  381,  360,  339,  320,  302,  285,  269,  254,  240,  226,
			 214,  202,  190,  180,  170,  160,  151,  143,  135,  127,  120,  113,
			 107,  101,   95,   90,   85,   80,   75,   71,   67,   63,   60,   56,
			  53,   50,   47,   45,   42,   40,   37,   35,   33,   31,   30,   28
		};

		private int numberOfPositions;
		private ushort[] trackOffsetTable;
		private byte[] positionList;

		private int numberOfTracks;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT5;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x7c0)
				return false;

			// Check mark
			moduleStream.Seek(0x3b8, SeekOrigin.Begin);

			if (moduleStream.Read_B_UINT32() != 0x4b524953)		// KRIS
				return false;

			// Check sample information
			moduleStream.Seek(0x16, SeekOrigin.Begin);

			uint samplesSize = 0;
			ushort temp;

			for (int i = 0; i < 31; i++)
			{
				moduleStream.Seek(0x16, SeekOrigin.Current);

				// Check sample size
				temp = moduleStream.Read_B_UINT16();
				if (temp >= 0x8000)
					return false;

				samplesSize += temp * 2U;

				// Check volume
				if (moduleStream.Read_B_UINT16() > 0x40)
					return false;

				moduleStream.Seek(4, SeekOrigin.Current);
			}

			// Find number of positions
			moduleStream.Seek(0x3bc, SeekOrigin.Begin);

			temp = moduleStream.Read_B_UINT16();
			if (temp == 0)
				return false;

			int positionListLength = temp / 256;

			// Find biggest pattern number
			uint trackCount = 0;

			for (int i = 0; i < positionListLength * 4; i++)
			{
				temp = (ushort)(moduleStream.Read_B_UINT16() & 0xff00);
				if (temp > trackCount)
					trackCount = temp;
			}

			trackCount += 0x0100;

			// Check the module length
			if ((0x7c0 + trackCount + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
				return false;

			// Check first 4 tracks
			moduleStream.Seek(0x7c0, SeekOrigin.Begin);

			temp = 0;
			for (int i = 0; i < 4 * 64; i++)
			{
				if (moduleStream.Read_B_UINT16() == 0xa800)
					temp++;		// Count number of empty rows

				moduleStream.Seek(2, SeekOrigin.Current);
			}

			if (temp == 0)
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
		/// Return the name of the module if any
		/// </summary>
		/********************************************************************/
		protected override byte[] GetModuleName(ModuleStream moduleStream)
		{
			byte[] moduleName = new byte[0x16];

			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.Read(moduleName, 0, 0x16);

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
				// Sample name
				byte[] name = null;

				if (moduleStream.Read_UINT8() == 0x01)
					moduleStream.Seek(21, SeekOrigin.Current);	// Has no name
				else
				{
					name = new byte[22];

					moduleStream.Seek(-1, SeekOrigin.Current);
					moduleStream.Read(name, 0, 22);
				}

				ushort length = moduleStream.Read_B_UINT16();
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();
				ushort loopStart = (ushort)(moduleStream.Read_B_UINT16() / 2);
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

					// Loop voices
					for (int j = 0; j < 4; j++)
					{
						// Get track offset and transpose
						int trackOffset = 0x7c0 + (trackOffsetTable[i * 4 + j] & 0xff00);
						int transpose = trackOffsetTable[i * 4 + j] & 0x00ff;

						moduleStream.Seek(trackOffset, SeekOrigin.Begin);

						// Loop rows
						for (int k = 0; k < 64; k++)
						{
							byte chipByte1 = moduleStream.Read_UINT8();
							byte chipByte2 = moduleStream.Read_UINT8();
							byte chipByte3 = moduleStream.Read_UINT8();
							byte chipByte4 = moduleStream.Read_UINT8();

							// Get the sample number
							byte byte1;

							byte byte3 = chipByte2;
							if (byte3 >= 0x10)
							{
								byte1 = 0x10;
								byte3 -= 0x10;
							}
							else
								byte1 = 0x00;

							byte3 <<= 4;

							// Get effect + effect value
							byte3 |= chipByte3;
							byte byte4 = chipByte4;

							// Get note
							byte byte2 = chipByte1;
							if (byte2 == 0xa8)
							{
								// No note
								byte2 = 0x00;
							}
							else
							{
								// Got a note
								byte2 += (byte)(transpose * 2);
								byte2 -= 0x18;
								byte2 /= 2;

								byte1 |= (byte)((chipPeriods[byte2] & 0xff00) >> 8);
								byte2 = (byte)(chipPeriods[byte2] & 0x00ff);
							}

							// Store the new pattern data
							pattern[k * 16 + j * 4] = byte1;
							pattern[k * 16 + j * 4 + 1] = byte2;
							pattern[k * 16 + j * 4 + 2] = byte3;
							pattern[k * 16 + j * 4 + 3] = byte4;
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
			moduleStream.Seek(0x7c0 + numberOfTracks, SeekOrigin.Begin);

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
			moduleStream.Seek(0x3bc, SeekOrigin.Begin);
			numberOfPositions = moduleStream.Read_B_UINT16() / 256;

			trackOffsetTable = new ushort[numberOfPositions * 4];
			moduleStream.ReadArray_B_UINT16s(trackOffsetTable, 0, trackOffsetTable.Length);

			// Find highest track number
			numberOfTracks = 0;
			for (int i = 0; i < trackOffsetTable.Length; i++)
			{
				int temp = trackOffsetTable[i] & 0xff00;
				if (temp > numberOfTracks)
					numberOfTracks = temp;
			}

			numberOfTracks += 0x0100;
		}



		/********************************************************************/
		/// <summary>
		/// Create position list and find the number of patterns needed
		/// </summary>
		/********************************************************************/
		private void CreatePositionList()
		{
			int trackOffset = 0;

			positionList = BuildPositionList(numberOfPositions, (pos) =>
			{
				ushort temp1 = trackOffsetTable[trackOffset++];
				ushort temp2 = trackOffsetTable[trackOffset++];
				ushort temp3 = trackOffsetTable[trackOffset++];
				ushort temp4 = trackOffsetTable[trackOffset++];

				return ((ulong)temp1 << 48) | ((ulong)temp2 << 32) | ((ulong)temp3 << 16) | temp4;
			});
		}
		#endregion
	}
}
