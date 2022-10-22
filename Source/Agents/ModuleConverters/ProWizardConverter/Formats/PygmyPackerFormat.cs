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
using System.Linq;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// Pygmy Packer
	/// </summary>
	internal class PygmyPackerFormat : ProWizardConverterWorker31SamplesBase
	{
		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT33;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x2a4)
				return false;

			// Check the last values in the period table
			moduleStream.Seek(0x2a0, SeekOrigin.Begin);
			if (moduleStream.Read_B_UINT32() != 0x00780071)
				return false;

			// Check sample addresses
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint firstSampleAddress = moduleStream.Read_B_UINT32();
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint temp, temp1 = 0;

			for (int i = 0; i < 30; i++)
			{
				// Get start address
				temp = moduleStream.Read_B_UINT32();
				if (temp == 0)
					return false;

				// Get the next sample start address
				moduleStream.Seek(12, SeekOrigin.Current);
				temp1 = moduleStream.Read_B_UINT32();

				// The next start address has to be lower than the previous one
				if (temp1 < temp)
					return false;

				moduleStream.Seek(-4, SeekOrigin.Current);
			}

			if (firstSampleAddress == temp1)
				return false;

			// Check sample information
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint samplesSize = 0;

			for (int i = 0; i < 31; i++)
			{
				temp = moduleStream.Read_B_UINT32();

				// Check sample length
				ushort temp2 = moduleStream.Read_B_UINT16();
				if (temp2 != 0)
				{
					samplesSize += temp2 * 2U;

					// Check loop values
					temp = moduleStream.Read_B_UINT32() - temp;
					temp = (temp / 2) + moduleStream.Read_B_UINT16();
					if (temp > temp2)
						return false;
				}
				else
					moduleStream.Seek(6, SeekOrigin.Current);

				// Check volume and fine tune
				if ((moduleStream.Read_UINT8() > 0x0f) || (moduleStream.Read_UINT8() > 0x40))
					return false;

				moduleStream.Seek(2, SeekOrigin.Current);
			}

			// Check sample size
			moduleStream.Seek(0x1f0, SeekOrigin.Begin);
			if (moduleStream.Read_B_UINT32() != samplesSize)
				return false;

			// Check period table
			moduleStream.Seek(0x25c, SeekOrigin.Begin);

			for (int i = 0; i < 36; i++)
			{
				if (moduleStream.Read_B_UINT16() != tuningPeriods[0, i])
					return false;
			}

			// Check offsets to patterns
			moduleStream.Seek(0x59c, SeekOrigin.Begin);

			temp = firstSampleAddress - moduleStream.Read_B_UINT32();
			if ((temp == 0) || ((temp & 0x1) != 0))
				return false;

			// Get number of patterns
			numberOfPatterns = (ushort)(temp / 1024);
			if (numberOfPatterns > 64)
				return false;

			// Check the module length
			if ((0x636 + numberOfPatterns * 1024 + samplesSize + 128) > (moduleStream.Length + MaxNumberOfMissingBytes))
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			// Find first real sample
			moduleStream.Seek(4, SeekOrigin.Begin);

			int j = 0;
			while (moduleStream.Read_B_UINT16() == 0)
			{
				j++;
				moduleStream.Seek(14, SeekOrigin.Current);
			}

			// Write sample information
			moduleStream.Seek(j * 16, SeekOrigin.Begin);

			for (int i = 0; i < 31 - j; i++)
			{
				uint start = moduleStream.Read_B_UINT32();

				ushort length = moduleStream.Read_B_UINT16();
				ushort loopStart = (ushort)((moduleStream.Read_B_UINT32() - start) / 2);
				ushort loopLength = moduleStream.Read_B_UINT16();
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();

				yield return new SampleInfo
				{
					Name = null,
					Length = length,
					LoopStart = loopStart,
					LoopLength = loopLength,
					Volume = volume,
					FineTune = fineTune
				};

				moduleStream.Seek(2, SeekOrigin.Current);
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
			int sampleSize = sampleLengths.Cast<int>().Sum();
			uint offset = (uint)(0x636 + numberOfPatterns * 1024 + sampleSize);
			moduleStream.Seek(offset, SeekOrigin.Begin);

			byte[] positionList = new byte[128];

			int i;
			for (i = 0; i < 128; i++)
			{
				byte temp = moduleStream.Read_UINT8();
				if (temp == 0xff)
					break;

				positionList[i] = temp;
			}

			return positionList.AsSpan(0, i);
		}



		/********************************************************************/
		/// <summary>
		/// Return the restart position
		/// </summary>
		/********************************************************************/
		protected override byte GetRestartPosition(ModuleStream moduleStream)
		{
			// Set to NoiseTracker mode
			return 0x00;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			moduleStream.Seek(0x636, SeekOrigin.Begin);

			byte[] pattern = new byte[1024];

			for (int i = 0; i < numberOfPatterns; i++)
			{
				// Loop the voices
				for (int j = 0; j < 4; j++)
				{
					// Loop the rows
					for (int k = 64; k > 0; k--)
					{
						byte byt2;

						// Get note
						byte byt1 = moduleStream.Read_UINT8();
						if (byt1 != 0)
						{
							if (byt1 >= 0xb8)
								byt1 = unchecked((byte)(-(sbyte)byt1));

							byt1 -= 2;
							byt1 /= 2;
							byt2 = periods[byt1, 1];
							byt1 = periods[byt1, 0];
						}
						else
							byt2 = 0x00;

						// Get sample
						byte byt3 = moduleStream.Read_UINT8();
						if ((byt3 & 0x80) != 0)
							byt1 |= 0x10;

						byt3 <<= 1;

						// Get effect and value
						byte temp = moduleStream.Read_UINT8();
						byte byt4 = moduleStream.Read_UINT8();
						byt3 |= ConvertEffect(temp, ref byt4);

						// Store pattern bytes
						pattern[(k - 1) * 16 + j * 4] = byt1;
						pattern[(k - 1) * 16 + j * 4 + 1] = byt2;
						pattern[(k - 1) * 16 + j * 4 + 2] = byt3;
						pattern[(k - 1) * 16 + j * 4 + 3] = byt4;
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
			moduleStream.Seek(0x636 + numberOfPatterns * 1024, SeekOrigin.Begin);

			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Convert the effect number to a ProTracker number
		/// </summary>
		/********************************************************************/
		private byte ConvertEffect(byte eff, ref byte effVal)
		{
			switch (eff)
			{
				// Arpeggio
				case 0x00:
				case 0x10:
					return 0x00;

				// Portamento up
				case 0x14:
					return 0x01;

				// Portamento down
				case 0x18:
					return 0x02;

				// Tone portamento
				case 0x04:
					return 0x03;

				// Vibrato
				case 0x24:
				{
					// Swap the value
					effVal = (byte)(((effVal & 0xf0) >> 4) | ((effVal & 0x0f) << 4));
					return 0x04;
				}

				// Tone + volume
				case 0x0c:
					return 0x05;

				// Vibrato + volume
				case 0x2c:
					return 0x06;

				// Volume slide up
				case 0x1c:
				{
					effVal <<= 4;
					return 0x0a;
				}

				// Volume slide down
				case 0x20:
					return 0x0a;

				// Set volume
				case 0x34:
					return 0x0c;

				// Pattern break
				case 0x38:
					return 0x0d;

				// Set filter
				case 0x3c:
				{
					if (effVal == 0x02)
						effVal = 0x01;

					return 0x0e;
				}

				// Set speed
				case 0x40:
					return 0x0f;
			}

			// Well, unknown effect, clear it
			effVal = 0x00;
			return 0x00;
		}
		#endregion
	}
}
