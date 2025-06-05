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
	/// Module-Patterncompressor
	/// </summary>
	internal class ModulePatterncompressorFormat : ProWizardForPcBase
	{
		private bool eightChannels;

		private byte numberOfPositions;
		private byte restartPosition;

		private uint[,,] patternOffsets;

		private int controlStartOffset;
		private int dataStartOffset;
		private int sampleStartOffset;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT57;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 1092)
				return false;

			// Start to check the mark
			moduleStream.Seek(1080, SeekOrigin.Begin);

			string mark = moduleStream.ReadMark();
			if ((mark != "PMD3") && (mark != "PMd3"))
				return false;

			// Check sample information
			moduleStream.Seek(20, SeekOrigin.Begin);

			uint samplesSize = 0;

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

				samplesSize += sampleSize;
			}

			// Check size of position table
			byte temp1 = moduleStream.Read_UINT8();
			if (temp1 > 127)
				return false;

			// Find highest pattern offset
			if (FindHighestPatternNumber(moduleStream, 952, 128) > 127)
				return false;

			// Check module length
			moduleStream.Seek(1084, SeekOrigin.Begin);

			uint temp = moduleStream.Read_B_UINT32();
			uint temp2 = moduleStream.Read_B_UINT32();

			if ((1092 + temp + temp2 + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
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
			moduleStream.Seek(950, SeekOrigin.Begin);

			numberOfPositions = moduleStream.Read_UINT8();
			restartPosition = moduleStream.Read_UINT8();

			moduleStream.Seek(1080, SeekOrigin.Begin);

			uint temp = moduleStream.Read_B_UINT32();
			eightChannels = (temp & 0x0000ff00) == 0x00006400;

			uint controlLen = moduleStream.Read_B_UINT32();
			uint dataLen = moduleStream.Read_B_UINT32();

			int channelCount = eightChannels ? 8 : 4;
			patternOffsets = new uint[numberOfPatterns, channelCount, 2];

			for (int i = 0; i < numberOfPatterns; i++)
			{
				for (int j = 0; j < channelCount; j++)
				{
					patternOffsets[i, j, 0] = moduleStream.Read_B_UINT32();
					patternOffsets[i, j, 1] = moduleStream.Read_B_UINT32();
				}
			}

			controlStartOffset = 1092 + numberOfPatterns * channelCount * 4 * 2;
			dataStartOffset = (int)(controlStartOffset + controlLen);
			sampleStartOffset = (int)(dataStartOffset + dataLen);

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

			moduleStream.Seek(952, SeekOrigin.Begin);
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
		/// Return the ID mark
		/// </summary>
		/********************************************************************/
		protected override uint GetMark()
		{
			return eightChannels ? 0x43443831U : 0x4d2e4b2e;		// CD81 / M.K.
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			byte[] pattern = new byte[eightChannels ? 2048 : 1024];
			int channelCount = eightChannels ? 8 : 4;
			int writeIncrement = eightChannels ? 32 : 16;

			for (int i = 0; i < numberOfPatterns; i++)
			{
				// Clear the pattern data
				Array.Clear(pattern);

				for (int j = 0; j < channelCount; j++)
				{
					uint controlOffset = patternOffsets[i, j, 0];
					uint dataOffset = patternOffsets[i, j, 1];

					for (int k = 0, m = 0; k < 64; m++)
					{
						moduleStream.Seek(controlStartOffset + controlOffset + m, SeekOrigin.Begin);
						byte c = moduleStream.Read_UINT8();

						byte c1 = (byte)((c & 0x03) + 1);
						byte c2 = (byte)(c & 0xfc);

						moduleStream.Seek(dataStartOffset + dataOffset + c2, SeekOrigin.Begin);

						byte byt1 = moduleStream.Read_UINT8();
						byte byt2 = moduleStream.Read_UINT8();
						byte byt3 = moduleStream.Read_UINT8();
						byte byt4 = moduleStream.Read_UINT8();

						int writeOffset = (j * 4) + (k * writeIncrement);

						for (int repeat = 0; repeat < c1; repeat++, writeOffset += writeIncrement)
						{
							pattern[writeOffset] = byt1;
							pattern[writeOffset + 1] = byt2;
							pattern[writeOffset + 2] = byt3;
							pattern[writeOffset + 3] = byt4;
						}

						k += c1;
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
			moduleStream.Seek(sampleStartOffset, SeekOrigin.Begin);

			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion
	}
}
