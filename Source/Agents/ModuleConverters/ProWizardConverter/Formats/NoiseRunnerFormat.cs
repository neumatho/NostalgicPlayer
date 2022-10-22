/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// NoiseRunner
	/// </summary>
	internal class NoiseRunnerFormat : ProWizardConverterWorker31SamplesBase
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
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT18;
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

			// Check sample information
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint samplesSize = 0;
			ushort temp2;

			for (int i = 0; i < 31; i++)
			{
				// Check volume
				if (moduleStream.Read_B_UINT16() > 0x40)
					return false;

				// Check pointer to sample
				uint temp = moduleStream.Read_B_UINT32();
				if (temp == 0)
					return false;

				// Take sample size
				temp2 = moduleStream.Read_B_UINT16();

				// Check loop pointer
				uint temp1 = moduleStream.Read_B_UINT32();
				if (temp1 == 0)
					return false;

				// Check loop size
				if (temp2 == 0)
				{
					if (temp != temp1)
						return false;

					moduleStream.Seek(4, SeekOrigin.Current);
				}
				else
				{
					temp = (temp1 - temp) / 2 + moduleStream.Read_B_UINT16();
					if (temp > temp2)
						return false;

					moduleStream.Seek(2, SeekOrigin.Current);
				}

				samplesSize += temp2 * 2U;
			}

			// Check position list size
			moduleStream.Seek(0x3b6, SeekOrigin.Begin);

			byte positionListLength = moduleStream.Read_UINT8();
			if ((positionListLength == 0) || (positionListLength > 0x7f))
				return false;

			// Check first two bytes in position list
			moduleStream.Seek(1, SeekOrigin.Current);
			if ((moduleStream.Read_UINT8() > 0x3f) || (moduleStream.Read_UINT8() > 0x3f))
				return false;

			// Check some of the "left-over" in the module
			moduleStream.Seek(0x3ae, SeekOrigin.Begin);

			temp2 = moduleStream.Read_B_UINT16();		// Sample length
			if (temp2 >= 0x8000)
				return false;

			if (temp2 == 0)
			{
				temp2 = moduleStream.Read_B_UINT16();	// Fine tune + volume
				temp2 += moduleStream.Read_B_UINT16();	// Loop start
				temp2 += moduleStream.Read_B_UINT16();	// Loop length

				if (temp2 != 1)
					return false;
			}
			else
			{
				if ((moduleStream.Read_UINT8() > 0x0f) || (moduleStream.Read_UINT8() > 0x40))
					return false;

				if ((moduleStream.Read_B_UINT16() + moduleStream.Read_B_UINT16()) > temp2)
					return false;
			}

			// Check highest pattern number
			if (FindHighestPatternNumber(moduleStream, 0x3b8, positionListLength) > 64)
				return false;

			// Check the module length
			if ((0x43c + numberOfPatterns * 1024 + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
				return false;

			// Check note values
			for (int i = 0; i < 64 * 4; i++)
			{
				moduleStream.Seek(0x43c + i * 4 + 2, SeekOrigin.Begin);
				if (moduleStream.Read_UINT8() > 0x48)
					return false;
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
			// Get number of positions and restart position
			moduleStream.Seek(0x3b6, SeekOrigin.Begin);

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
				ushort volume = moduleStream.Read_B_UINT16();
				uint startPointer = moduleStream.Read_B_UINT32();
				ushort length = moduleStream.Read_B_UINT16();
				uint loopPointer = moduleStream.Read_B_UINT32();
				ushort loopLength = moduleStream.Read_B_UINT16();

				// Fix fine tune
				byte fineTune = 0;
				ushort temp = moduleStream.Read_B_UINT16();
				if (temp != 0)
				{
					if ((temp & 0xf000) == 0xf000)
					{
						// Yes, we have a fine tune
						fineTune = (byte)((0x1000 - temp & 0x0fff) / 0x48);
					}
				}

				// Fix loop start
				ushort loopStart = (ushort)((loopPointer - startPointer) / 2);

				yield return new SampleInfo
				{
					Name = null,
					Length = length,
					LoopStart = loopStart,
					LoopLength = loopLength,
					Volume = (byte)volume,
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

			moduleStream.Seek(0x3b8, SeekOrigin.Begin);
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

			moduleStream.Seek(0x43c, SeekOrigin.Begin);

			for (int i = 0; i < numberOfPatterns; i++)
			{
				// Clear the pattern data
				Array.Clear(pattern);

				// Loop the pattern
				for (int j = 0; j < 64 * 4; j++)
				{
					byte byt1 = moduleStream.Read_UINT8();
					byte byt2 = moduleStream.Read_UINT8();
					byte byt3 = moduleStream.Read_UINT8();
					byte byt4 = moduleStream.Read_UINT8();

					// Convert sample number
					byte temp1 = (byte)(byt4 / 8);
					if (temp1 >= 0x10)
					{
						pattern[j * 4] = 0x10;
						temp1 -= 0x10;
					}

					pattern[j * 4 + 2] = (byte)(temp1 << 4);

					// Convert note
					temp1 = byt3;
					if (temp1 != 0)
					{
						temp1 = (byte)((temp1 - 2) / 2);
						pattern[j * 4] |= periods[temp1, 0];
						pattern[j * 4 + 1] = periods[temp1, 1];
					}

					// Convert effect value
					pattern[j * 4 + 3] = byt2;

					// Convert effect
					temp1 = byt1;

					switch (temp1)
					{
						// Tone portamento (0 -> 3)
						case 0x0:
						{
							temp1 = 0x3;
							break;
						}

						// No effect (C -> 0)
						case 0xc:
						{
							temp1 = 0x0;
							break;
						}

						// All other effects
						default:
						{
							temp1 /= 4;
							break;
						}
					}

					pattern[j * 4 + 2] |= temp1;
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
			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion
	}
}
