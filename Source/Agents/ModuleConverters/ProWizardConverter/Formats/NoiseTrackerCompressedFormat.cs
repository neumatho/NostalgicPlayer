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
	/// NoiseTracker Compressed
	/// </summary>
	internal class NoiseTrackerCompressedFormat : ProWizardConverterWorker31SamplesBase
	{
		private byte numberOfPositions;
		private byte restartPosition;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT19;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 1080)
				return false;

			// Check the first pattern mark
			moduleStream.Seek(0x1f6, SeekOrigin.Begin);
			if (moduleStream.Read_B_UINT32() != 0x50415454)		// PATT
				return false;

			// Check position list size
			moduleStream.Seek(0x174, SeekOrigin.Begin);

			byte positionListLength = moduleStream.Read_UINT8();
			if ((positionListLength == 0) || (positionListLength >= 128))
				return false;

			// Check highest pattern number
			if (FindHighestPatternNumber(moduleStream, 0x176, positionListLength) > 64)
				return false;

			// Check offset to first sample
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint firstSampleOffset = moduleStream.Read_B_UINT32();
			if ((firstSampleOffset & 0x1) != 0)
				return false;

			// Check number of patterns
			moduleStream.Seek(0x1f6, SeekOrigin.Begin);

			for (int i = 0; i < numberOfPatterns; i++)
			{
				for (;;)
				{
					uint temp4 = moduleStream.Read_B_UINT32();
					moduleStream.Seek(-2, SeekOrigin.Current);

					if (temp4 == 0x50415454)	// PATT
						break;	// Go to next pattern

					if (moduleStream.Position >= firstSampleOffset)
						return false;
				}
			}

			// Check sample information
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint samplesSize = 0;

			for (int i = 0; i < 31; i++)
			{
				// Check pointer to sample
				uint sampleOffset = moduleStream.Read_B_UINT32();
				if ((sampleOffset & 0x1) != 0)
					return false;

				// Check sample length
				ushort temp = moduleStream.Read_B_UINT16();
				if (temp >= 0x8000)
					return false;

				// Get volume + loop + loop length
				ushort temp1 = moduleStream.Read_B_UINT16();
				ushort temp2 = moduleStream.Read_B_UINT16();
				ushort temp3 = moduleStream.Read_B_UINT16();

				// Check volume & fine tune
				if (((temp1 & 0x00ff) > 0x40) || ((temp1 & 0xff00) > 0x0f00))
					return false;

				if (temp == 0)
				{
					// Volume + loop + loop length
					if ((temp1 + temp2 + temp3) > 0x0001)
						return false;
				}
				else
				{
					// Check loop
					if ((temp2 + temp3) > temp)
						return false;

					samplesSize += temp * 2U;
				}
			}

			// Check the module length
			if ((firstSampleOffset + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
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
			moduleStream.Seek(0x174, SeekOrigin.Begin);

			numberOfPositions = moduleStream.Read_UINT8();
			restartPosition = moduleStream.Read_UINT8();

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
				moduleStream.Seek(4, SeekOrigin.Current);

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
					LoopLength = loopLength == 0 ? (ushort)0x0001 : loopLength,
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

			moduleStream.Seek(0x176, SeekOrigin.Begin);
			moduleStream.Read(positionList, 0, numberOfPositions);

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
			byte[] pattern = new byte[1024];

			moduleStream.Seek(0x1f6, SeekOrigin.Begin);

			for (int i = 0; i < numberOfPatterns; i++)
			{
				// Make sure we have an even offset
				if ((moduleStream.Position & 0x01) != 0)
					moduleStream.Seek(1, SeekOrigin.Current);

				// Skip PATT + 3 words
				moduleStream.Seek(4 + 3 * 2, SeekOrigin.Current);

				// Clear the pattern data
				Array.Clear(pattern);

				// Loop the voices
				for (int j = 0; j < 4; j++)
				{
					// Loop the rows
					for (int k = 0; k < 64; k++)
					{
						// Get pattern byte
						byte temp1 = moduleStream.Read_UINT8();

						if (temp1 == 0xff)
						{
							// Empty lines
							k += moduleStream.Read_UINT8() - 1;
							continue;
						}

						if (temp1 >= 0xc0)
						{
							// Sample 0x1x and no effect
							temp1 -= 0xc0;

							// Get note
							if (temp1 != 0)
							{
								temp1--;
								pattern[k * 16 + j * 4] = periods[temp1, 0];
								pattern[k * 16 + j * 4 + 1] = periods[temp1, 1];
							}

							// Set hi sample bit
							pattern[k * 16 + j * 4] |= 0x10;

							// Get sample number
							pattern[k * 16 + j * 4 + 2] = moduleStream.Read_UINT8();
							continue;
						}

						if (temp1 >= 0x80)
						{
							// Sample 0x1x and effect
							temp1 -= 0x80;

							// Get note
							if (temp1 != 0)
							{
								temp1--;
								pattern[k * 16 + j * 4] = periods[temp1, 0];
								pattern[k * 16 + j * 4 + 1] = periods[temp1, 1];
							}

							// Set hi sample bit
							pattern[k * 16 + j * 4] |= 0x10;

							// Get sample number + effect
							pattern[k * 16 + j * 4 + 2] = moduleStream.Read_UINT8();
							pattern[k * 16 + j * 4 + 3] = moduleStream.Read_UINT8();
							continue;
						}

						if (temp1 >= 0x40)
						{
							// Sample 0x0x and no effect
							temp1 -= 0x40;

							// Get note
							if (temp1 != 0)
							{
								temp1--;
								pattern[k * 16 + j * 4] = periods[temp1, 0];
								pattern[k * 16 + j * 4 + 1] = periods[temp1, 1];
							}

							// Get sample number
							pattern[k * 16 + j * 4 + 2] = moduleStream.Read_UINT8();
							continue;
						}

						// Sample 0x0x and effect
						//
						// Get note
						if (temp1 != 0)
						{
							temp1--;
							pattern[k * 16 + j * 4] = periods[temp1, 0];
							pattern[k * 16 + j * 4 + 1] = periods[temp1, 1];
						}

						// Get sample number + effect
						pattern[k * 16 + j * 4 + 2] = moduleStream.Read_UINT8();
						pattern[k * 16 + j * 4 + 3] = moduleStream.Read_UINT8();
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
			moduleStream.Seek(0, SeekOrigin.Begin);
			uint sampleDataOffset = moduleStream.Read_B_UINT32();
			moduleStream.Seek(sampleDataOffset, SeekOrigin.Begin);

			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion
	}
}
