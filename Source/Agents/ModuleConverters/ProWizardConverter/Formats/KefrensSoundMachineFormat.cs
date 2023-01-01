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
	/// Kefrens Sound Machine
	/// </summary>
	internal class KefrensSoundMachineFormat : ProWizardConverterWorker31SamplesBase
	{
		private int numberOfPositions;
		private byte[] trackTable;
		private byte[] positionList;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT12;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x600)
				return false;

			// Check mark
			moduleStream.Seek(0, SeekOrigin.Begin);

			if (moduleStream.Read_B_UINT16() != 0x4d2e)		// M.
				return false;

			// Check sample information
			moduleStream.Seek(32, SeekOrigin.Begin);

			uint samplesSize = 0;
			uint offset = 0;
			uint temp;

			for (int i = 0; i < 15; i++)
			{
				moduleStream.Seek(16, SeekOrigin.Current);

				// Check offset to sample
				temp = moduleStream.Read_B_UINT32();
				if ((temp == 0) || (temp <= offset))
					return false;

				offset = temp;

				// Check sample size
				ushort temp1 = moduleStream.Read_B_UINT16();
				if ((temp1 == 0) || ((temp1 & 0x1) != 0))
					return false;

				samplesSize += temp1;

				// Check volume
				if (moduleStream.Read_UINT8() > 0x40)
					return false;

				moduleStream.Seek(9, SeekOrigin.Current);
			}

			// Check position list
			for (;;)
			{
				// Did we reach outside of position list table?
				if (moduleStream.Position > 0x400)		// It should be 0x600, since KSM supports up to 256 positions, but we only support 128 positions
					return false;

				// Found the end of position list?
				if (moduleStream.Read_UINT8() == 0xff)
					break;

				moduleStream.Seek(3, SeekOrigin.Current);
			}

			// The first part of the track data should be zeroed. Check it
			moduleStream.Seek(0x600, SeekOrigin.Begin);
			temp = 0;

			for (int i = 0; i < 48; i++)
				temp += moduleStream.Read_B_UINT32();

			if (temp != 0)
				return false;

			// Check notes in the track
			for (int i = 0; i < 48; i++)
			{
				if (moduleStream.Read_UINT8() > 0x24)
					return false;

				moduleStream.Seek(2, SeekOrigin.Current);
			}

			// Check the module length
			moduleStream.Seek(0x30, SeekOrigin.Begin);

			if ((moduleStream.Read_B_UINT32() + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
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
			byte[] moduleName = new byte[20];

			moduleStream.Seek(2, SeekOrigin.Begin);
			moduleStream.Read(moduleName, 0, 13);

			return moduleName;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			moduleStream.Seek(32, SeekOrigin.Begin);

			for (int i = 0; i < 15; i++)
			{
				moduleStream.Seek(20, SeekOrigin.Current);

				ushort length = (ushort)(moduleStream.Read_B_UINT16() / 2);
				byte volume = moduleStream.Read_UINT8();
				moduleStream.Seek(1, SeekOrigin.Current);
				ushort loopStart = moduleStream.Read_B_UINT16();
				ushort loopLength = 1;

				if (loopStart != 0)
				{
					loopStart /= 2;
					loopLength = (ushort)(length - loopStart);
				}

				moduleStream.Seek(6, SeekOrigin.Current);

				yield return new SampleInfo
				{
					Name = null,
					Length = length,
					LoopStart = loopStart,
					LoopLength = loopLength,
					Volume = volume,
					FineTune = 0
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
			return positionList.AsSpan(0, numberOfPositions);
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
				// Get pattern number to build
				if (positionList[i] > lastPatternNumber)
				{
					lastPatternNumber++;

					// Clear the pattern data
					Array.Clear(pattern);

					// Loop voices
					for (int j = 0; j < 4; j++)
					{
						// Get track offset and transpose
						int trackOffset = 0x600 + (trackTable[i * 4 + j] * 192);
						moduleStream.Seek(trackOffset, SeekOrigin.Begin);

						// Loop rows
						for (int k = 0; k < 64; k++)
						{
							byte temp = moduleStream.Read_UINT8();
							if (temp != 0)
							{
								// There is a note, convert it to a period
								temp--;
								pattern[k * 16 + j * 4] = periods[temp, 0];
								pattern[k * 16 + j * 4 + 1] = periods[temp, 1];
							}

							// Copy sample and effect
							byte temp1 = moduleStream.Read_UINT8();
							byte temp2 = moduleStream.Read_UINT8();

							// Convert effect D to A
							if ((temp1 & 0x0f) == 0x0d)
							{
								temp1 &= 0xf0;
								temp1 |= 0x0a;
							}

							pattern[k * 16 + j * 4 + 2] = temp1;
							pattern[k * 16 + j * 4 + 3] = temp2;
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
			moduleStream.Seek(0x30, SeekOrigin.Begin);
			uint sampleDataOffset = moduleStream.Read_B_UINT32();
			moduleStream.Seek(sampleDataOffset, SeekOrigin.Begin);

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
			moduleStream.Seek(0x200, SeekOrigin.Begin);

			trackTable = new byte[0x200];
			moduleStream.Read(trackTable, 0, 0x200);
		}



		/********************************************************************/
		/// <summary>
		/// Create position list and find the number of patterns needed
		/// </summary>
		/********************************************************************/
		private void CreatePositionList()
		{
			positionList = new byte[128];

			// Begin to create the position table
			List<uint> trackCombinations = new List<uint>();
			numberOfPatterns = 0;

			int trackOffset = 0;

			for (numberOfPositions = 0; numberOfPositions < 128; numberOfPositions++)
			{
				// Get offsets
				byte temp1 = trackTable[trackOffset++];
				byte temp2 = trackTable[trackOffset++];
				byte temp3 = trackTable[trackOffset++];
				byte temp4 = trackTable[trackOffset++];

				// Reached end of table?
				if (temp1 == 0xff)
					break;

				uint trackNumbers = ((uint)temp1 << 24) | ((uint)temp2 << 16) | ((uint)temp3 << 8) | temp4;

				// Check to see if this track combination has already been used previously
				bool found = false;

				for (int j = 0; j < trackCombinations.Count; j++)
				{
					if (trackCombinations[j] == trackNumbers)
					{
						// Found an equal track combination
						positionList[numberOfPositions] = (byte)j;
						found = true;
						break;
					}
				}

				if (!found)
				{
					positionList[numberOfPositions] = (byte)numberOfPatterns++;
					trackCombinations.Add(trackNumbers);
				}
			}
		}
		#endregion
	}
}
