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
	/// Digital Illusions
	/// </summary>
	internal class DigitalIllusionsFormat : ProWizardConverterWorker31SamplesBase
	{
		private ushort numberOfSamples;

		private uint positionListOffset;
		private uint sampleDataOffset;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT6;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 17)
				return false;

			// Check number of samples
			moduleStream.Seek(0, SeekOrigin.Begin);
			ushort sampleCount = moduleStream.Read_B_UINT16();
			if (sampleCount > 0x1f)
				return false;

			// Get all the offsets
			uint offset1 = moduleStream.Read_B_UINT32();
			uint offset2 = moduleStream.Read_B_UINT32();
			uint offset3 = moduleStream.Read_B_UINT32();

			// Check upper word on all the offsets
			if (((offset1 & 0xffff0000) != 0x0000) || ((offset2 & 0xffff0000) != 0) || ((offset3 & 0xffff0000) != 0))
				return false;

			// Check lower word on all the offsets
			if (((offset1 & 0x0000ffff) == 0x0000) || ((offset2 & 0x0000ffff) == 0) || ((offset3 & 0x0000ffff) == 0))
				return false;

			// Check pattern offset
			moduleStream.Seek(14 + sampleCount * 8, SeekOrigin.Begin);
			if (moduleStream.Read_B_UINT16() != offset2)
				return false;

			// Check sample information
			moduleStream.Seek(14, SeekOrigin.Begin);
			uint samplesSize = 0;

			for (int i = 0; i < sampleCount; i++)
			{
				// Get sample size
				ushort temp = moduleStream.Read_B_UINT16();
				if (temp >= 0x8000)
					return false;

				samplesSize += temp * 2U;

				// Check volume and fine tune
				if ((moduleStream.Read_UINT8() > 0x0f) || (moduleStream.Read_UINT8() > 0x40))
					return false;

				// Check loop and loop length
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

			// Check "end mark" in the position list
			moduleStream.Seek(offset2 - 1, SeekOrigin.Begin);
			if (moduleStream.Read_UINT8() != 0xff)
				return false;

			// Find number of patterns
			moduleStream.Seek(offset1, SeekOrigin.Begin);

			byte patternNumber;
			numberOfPatterns = 0;

			while ((patternNumber = moduleStream.Read_UINT8()) != 0xff)
			{
				if (patternNumber > numberOfPatterns)
					numberOfPatterns = patternNumber;
			}

			numberOfPatterns++;

			// Check the pattern offsets to see if they are in ascending order
			if (numberOfPatterns > 2)
			{
				moduleStream.Seek(14 + sampleCount * 8, SeekOrigin.Begin);
				ushort temp1 = 0;

				for (int i = 0; i < numberOfPatterns; i++)
				{
					ushort temp2 = moduleStream.Read_B_UINT16();
					if (temp2 < temp1)
						return false;

					temp1 = temp2;
				}
			}

			// Check the module length
			if ((offset3 + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
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
			// Get the number of samples and offsets
			numberOfSamples = moduleStream.Read_B_UINT16();

			positionListOffset = moduleStream.Read_B_UINT32();
			moduleStream.Seek(4, SeekOrigin.Current);		// Pattern offset
			sampleDataOffset = moduleStream.Read_B_UINT32();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			for (int i = 0; i < numberOfSamples; i++)
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
			moduleStream.Seek(positionListOffset, SeekOrigin.Begin);

			byte[] positionList = new byte[128];

			int i;
			for (i = 0; i < 128; i++)
			{
				byte pattern = moduleStream.Read_UINT8();
				if (pattern == 0xff)
					break;

				positionList[i] = pattern;
			}

			return positionList.AsSpan(0, i);
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			ushort[] patternOffsets = new ushort[numberOfPatterns];

			moduleStream.Seek(14 + numberOfSamples * 8, SeekOrigin.Begin);
			moduleStream.ReadArray_B_UINT16s(patternOffsets, 0, numberOfPatterns);

			byte[] pattern = new byte[1024];

			for (int i = 0; i < numberOfPatterns; i++)
			{
				int writeOffset = 0;

				// Seek to the pattern data
				moduleStream.Seek(patternOffsets[i], SeekOrigin.Begin);

				// Clear pattern data
				Array.Clear(pattern);

				for (int j = 0; j < 64; j++)
				{
					bool stopPatternParsing = false;

					for (int k = 0; k < 4; k++)
					{
						// Get first pattern byte
						byte tempByte = moduleStream.Read_UINT8();
						if (tempByte == 0xff)
						{
							// Blank note
							writeOffset += 4;
							continue;
						}

						byte sampleNum, noteNum;

						byte tempByte2 = moduleStream.Read_UINT8();
						switch (tempByte2 & 0x0f)		// Fix by Thomas Neumann
						{
							case 0xb:
							case 0xd:
							{
								stopPatternParsing = true;
								break;
							}
						}

						if (tempByte < 0x80)
						{
							// Sample + note + effect without value
							sampleNum = (byte)((tempByte >> 2) & 0x1f);
							noteNum = (byte)(((tempByte2 & 0xf0) >> 4) | ((tempByte & 0x03) << 4));

							pattern[writeOffset + 2] = (byte)(tempByte2 & 0x0f);
							pattern[writeOffset + 2] |= (byte)(((sampleNum & 0x0f) << 4));

							if (noteNum > 0)
							{
								pattern[writeOffset + 1] = periods[noteNum - 1, 1];
								pattern[writeOffset] = periods[noteNum - 1, 0];
							}

							if (sampleNum >= 0x10)
								pattern[writeOffset] |= 0x10;

							writeOffset += 4;
							continue;
						}

						// Sample + note + effect with value
						sampleNum = (byte)((tempByte >> 2) & 0x1f);
						noteNum = (byte)(((tempByte2 & 0xf0) >> 4) | ((tempByte & 0x03) << 4));

						pattern[writeOffset + 3] = moduleStream.Read_UINT8();
						pattern[writeOffset + 2] = (byte)(tempByte2 & 0x0f);
						pattern[writeOffset + 2] |= (byte)((sampleNum & 0x0f) << 4);

						if (noteNum > 0)
						{
							pattern[writeOffset + 1] = periods[noteNum - 1, 1];
							pattern[writeOffset] = periods[noteNum - 1, 0];
						}

						if (sampleNum >= 0x10)
							pattern[writeOffset] |= 0x10;

						writeOffset += 4;
					}

					if (stopPatternParsing)
						break;
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
			moduleStream.Seek(sampleDataOffset, SeekOrigin.Begin);

			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion
	}
}
