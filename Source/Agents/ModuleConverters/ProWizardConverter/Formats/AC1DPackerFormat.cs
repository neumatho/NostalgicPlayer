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
	/// AC1D Packer
	/// </summary>
	internal class AC1DPackerFormat : ProWizardConverterWorker31SamplesBase
	{
		private byte numberOfPositions;
		private byte restartPosition;
		private uint sampleDataOffset;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT1;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			numberOfPatterns = 0;

			if (moduleStream.Length < 896)
				return false;

			// Get first bytes
			moduleStream.Seek(0, SeekOrigin.Begin);

			byte pos = moduleStream.Read_UINT8();
			byte restartPos = moduleStream.Read_UINT8();

			// Check the ID
			ushort id = moduleStream.Read_B_UINT16();
			if ((id != 0xac1d) && (id != 0xd1ca))
				return false;

			// Check number of positions and restart pos
			if ((pos == 0) || (pos > 127))
				return false;

			if (restartPos > 0x7f)
				return false;

			// Check offset to samples
			uint sampleOffset = moduleStream.Read_B_UINT32();
			if ((sampleOffset == 0) || ((sampleOffset & 0x1) != 0))
				return false;

			// Check the sample information
			uint samplesSize = 0;

			for (int i = 0; i < 31; i++)
			{
				// Get sample size
				uint temp1 = moduleStream.Read_B_UINT16();
				if (temp1 >= 0x8000)
					return false;

				samplesSize += temp1 * 2;

				// Check volume and fine tune
				if (moduleStream.Read_UINT8() > 0x0f)
					return false;

				if (moduleStream.Read_UINT8() > 0x40)
					return false;

				// Skip loop information
				moduleStream.Seek(4, SeekOrigin.Current);
			}

			// Check the calculated length
			if ((sampleOffset + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
				return false;

			// Find highest pattern number
			byte[] positionList = new byte[128];

			moduleStream.Seek(0x300, SeekOrigin.Begin);
			if (moduleStream.Read(positionList, 0, 128) != 128)
				return false;

			numberOfPatterns = 0;

			for (int i = 0; i < 128; i++)
			{
				if (positionList[i] > numberOfPatterns)
					numberOfPatterns = positionList[i];
			}

			// Check number of patterns
			numberOfPatterns++;
			if (numberOfPatterns > 64)
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
			// Get the number of positions and restart position
			numberOfPositions = moduleStream.Read_UINT8();
			restartPosition = moduleStream.Read_UINT8();

			// Skip mark
			moduleStream.Seek(2, SeekOrigin.Current);

			// Read sample data offset
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
			byte[] positionList = new byte[128];

			moduleStream.Seek(0x300, SeekOrigin.Begin);
			moduleStream.ReadInto(positionList, 0, 128);

			return positionList.AsSpan(0, numberOfPositions);
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
			uint[] patternAddresses = new uint[128];
			byte[] pattern = new byte[1024];

			// Read pattern address table
			moduleStream.Seek(0x100, SeekOrigin.Begin);
			moduleStream.ReadArray_B_UINT32s(patternAddresses, 0, 128);

			// Get the start offset
			uint ac1dOffset = patternAddresses[0];

			for (int i = 0; i < numberOfPatterns; i++)
			{
				// Clear the pattern data
				Array.Clear(pattern);

				// Find the pattern offset
				uint patternOffset = patternAddresses[i] - ac1dOffset + 0x380 + 12;
				moduleStream.Seek(patternOffset, SeekOrigin.Begin);

				// Loop each channel
				for (int j = 0; j < 4; j++)
				{
					int writeOffset = j * 4;

					// Loop each line
					for (int k = 0; k < 64; k++)
					{
						byte tempByte = moduleStream.Read_UINT8();
						if (tempByte >= 0x81)
						{
							// Empty lines
							tempByte -= 0x80;
							writeOffset += tempByte * 16;
							k += tempByte - 1;
							continue;
						}

						if (tempByte == 0x7f)
						{
							// Special 0x7f -> No note, only sample
							pattern[writeOffset] = 0x10;
							pattern[writeOffset + 1] = 0x00;
							pattern[writeOffset + 2] = moduleStream.Read_UINT8();
							pattern[writeOffset + 3] = moduleStream.Read_UINT8();

							writeOffset += 16;
							continue;
						}

						if (tempByte == 0x3f)
						{
							// Special 0x3f -> No note, no sample
							pattern[writeOffset] = 0x00;
							pattern[writeOffset + 1] = 0x00;
							pattern[writeOffset + 2] = moduleStream.Read_UINT8();
							pattern[writeOffset + 3] = moduleStream.Read_UINT8();

							writeOffset += 16;
							continue;
						}

						if (tempByte >= 0x4c)
						{
							// Note + sample >= 0x10
							pattern[writeOffset] = 0x10;
							tempByte -= 0x40;
						}

						// Normal note
						tempByte -= 0x0c;

						pattern[writeOffset] |= periods[tempByte, 0];
						pattern[writeOffset + 1] = periods[tempByte, 1];

						tempByte = moduleStream.Read_UINT8();
						if ((tempByte & 0x0f) == 0x07)
						{
							// No effect, only sample
							pattern[writeOffset + 2] = (byte)(tempByte & 0xf0);

							writeOffset += 16;
							continue;
						}

						// Note + sample + effect
						pattern[writeOffset + 2] = tempByte;
						pattern[writeOffset + 3] = moduleStream.Read_UINT8();

						writeOffset += 16;
					}
				}

				// Return the pattern
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
