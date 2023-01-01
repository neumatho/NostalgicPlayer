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
	/// Xann Packer
	/// </summary>
	internal class XannPackerFormat : ProWizardConverterWorker31SamplesBase
	{
		private uint lowPatternOffset;
		private uint origin;

		private byte numberOfPositions;

		private uint[] newPatternOffsets;
		private uint[] newSampleAddresses;
		private uint[] newSampleLoopAddresses;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT47;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x83c)
				return false;

			// Check the first two pattern offsets
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint temp = moduleStream.Read_B_UINT32();
			if (temp > 0xf8000)
				return false;

			uint temp1 = moduleStream.Read_B_UINT32();
			if ((temp1 == 0) || (temp1 > 0xf8000))
				return false;

			if (temp1 > temp)
				temp = temp1 - temp;
			else
				temp -= temp1;

			if ((temp % 1024) != 0)
				return false;

			// Check sample information
			moduleStream.Seek(0x206, SeekOrigin.Begin);

			uint samplesSize = 0;
			uint previousAddress = 0;

			for (int i = 0; i < 31; i++)
			{
				// Check volume and fine tune
				if ((moduleStream.Read_UINT8() > 0x0f) || (moduleStream.Read_UINT8() > 0x40))
					return false;

				// Check sample size and loop length
				temp = moduleStream.Read_B_UINT32();
				ushort temp2 = moduleStream.Read_B_UINT16();
				temp1 = moduleStream.Read_B_UINT32();
				ushort temp3 = moduleStream.Read_B_UINT16();
				moduleStream.Seek(2, SeekOrigin.Current);

				if (temp1 == 0)
					return false;

				if (temp1 < previousAddress)
					return false;

				previousAddress = temp1;

				if (temp3 == 0)
				{
					if (temp2 != 0x0001)
						return false;

					if (temp != temp1)
						return false;
				}
				else
				{
					if (temp3 > 0x8000)
						return false;

					// Loop length > length?
					if (temp2 > temp3)
						return false;

					samplesSize += temp3 * 2U;
				}
			}

			// Check first pattern
			moduleStream.Seek(0x43c, SeekOrigin.Begin);

			for (int i = 0; i < 1024; i++)
			{
				moduleStream.Seek(1, SeekOrigin.Current);

				// Check note
				if (moduleStream.Read_UINT8() > 0x48)
					return false;

				// Check volume command if any
				if (moduleStream.Read_UINT8() == 0x48)
				{
					if (moduleStream.Read_UINT8() > 0x40)
						return false;
				}
				else
					moduleStream.Seek(1, SeekOrigin.Current);
			}

			// Check pattern offsets
			var offsets = FindPatternOffsets(moduleStream);

			temp = offsets.high - offsets.low;
			if (temp == 0)
				return false;

			if ((temp % 1024) != 0)
				return false;

			numberOfPatterns = (ushort)((temp / 1024) + 1);

			// Check sample address
			moduleStream.Seek(0x20e, SeekOrigin.Begin);

			uint temp4 = moduleStream.Read_B_UINT32();
			if ((offsets.high + 1024) != temp4)
				return false;

			if (previousAddress == temp4)
				return false;

			// Check module length
			temp = temp4 - (offsets.low & 0xf000);

			if ((temp + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
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
			// Get pattern offsets
			var offset = FindPatternOffsets(moduleStream);
			lowPatternOffset = offset.low;
			origin = lowPatternOffset & 0xf000;

			numberOfPositions = offset.posCount;

			// Build new pattern offsets
			moduleStream.Seek(0, SeekOrigin.Begin);

			newPatternOffsets = new uint[numberOfPositions];

			for (int i = 0; i < numberOfPositions; i++)
				newPatternOffsets[i] = moduleStream.Read_B_UINT32() - origin;

			// Convert sample addresses
			moduleStream.Seek(0x206, SeekOrigin.Begin);

			newSampleAddresses = new uint[31];
			newSampleLoopAddresses = new uint[31];

			for (int i = 0; i < 31; i++)
			{
				moduleStream.Seek(2, SeekOrigin.Current);
				newSampleLoopAddresses[i] = moduleStream.Read_B_UINT32() - origin;

				moduleStream.Seek(2, SeekOrigin.Current);
				newSampleAddresses[i] = moduleStream.Read_B_UINT32() - origin;

				moduleStream.Seek(4, SeekOrigin.Current);
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			moduleStream.Seek(0x206, SeekOrigin.Begin);

			for (int i = 0; i < 31; i++)
			{
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();
				moduleStream.Seek(4, SeekOrigin.Current);
				ushort loopLength = moduleStream.Read_B_UINT16();
				moduleStream.Seek(4, SeekOrigin.Current);
				ushort length = moduleStream.Read_B_UINT16();
				moduleStream.Seek(2, SeekOrigin.Current);

				yield return new SampleInfo
				{
					Name = null,
					Length = length,
					LoopStart = (ushort)((newSampleLoopAddresses[i] - newSampleAddresses[i]) / 2),
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

			for (int i = 0; i < numberOfPositions; i++)
				positionList[i] = (byte)((newPatternOffsets[i] + origin - lowPatternOffset) / 1024);

			return positionList;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			moduleStream.Seek(lowPatternOffset + origin, SeekOrigin.Begin);

			byte[] pattern = new byte[1024];

			for (int i = 0; i < numberOfPatterns; i++)
			{
				for (int j = 0; j < 64 * 4; j++)
				{
					// Get sample number
					byte temp = (byte)(moduleStream.Read_UINT8() >> 3);
					if (temp >= 0x10)
					{
						pattern[j * 4] = 0x10;		// Hi bit in sample number
						temp -= 0x10;
					}
					else
						pattern[j * 4] = 0x00;

					pattern[j * 4 + 2] = (byte)(temp << 4);

					// Get note number
					temp = moduleStream.Read_UINT8();
					if (temp != 0)
					{
						temp -= 2;
						temp /= 2;
						pattern[j * 4] |= periods[temp, 0];
						pattern[j * 4 + 1] = periods[temp, 1];
					}
					else
						pattern[j * 4 + 1] = 0x00;

					// Get effect + value
					temp = moduleStream.Read_UINT8();
					byte temp1 = moduleStream.Read_UINT8();

					ConvertEffect(ref temp, ref temp1);

					// Store the effect
					pattern[j * 4 + 2] |= temp;
					pattern[j * 4 + 3] = temp1;
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
			moduleStream.Seek(newSampleAddresses[0], SeekOrigin.Begin);

			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Find the lowest and highest pattern offsets
		/// </summary>
		/********************************************************************/
		private (uint low, uint high, byte posCount) FindPatternOffsets(ModuleStream moduleStream)
		{
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint low = moduleStream.Read_B_UINT32();
			uint hi = low;
			byte posCount = 1;

			for (int i = 1; i < 127; i++)
			{
				uint temp = moduleStream.Read_B_UINT32();
				if (temp != 0)
				{
					if (temp < low)
						low = temp;

					if (temp > hi)
						hi = temp;

					posCount++;
				}
			}

			return (low, hi, posCount);
		}



		/********************************************************************/
		/// <summary>
		/// Convert mapped effect back to ProTracker and change the effect
		/// value if needed
		/// </summary>
		/********************************************************************/
		private void ConvertEffect(ref byte effect, ref byte effectVal)
		{
			switch (effect)
			{
				// No effect
				case 0x0:
					break;

				// Effect 0x0??
				case 0x04:
				{
					effect = 0x0;
					break;
				}

				// Effect 0x1??
				case 0x08:
				{
					effect = 0x1;
					break;
				}

				// Effect 0x2??
				case 0x0c:
				{
					effect = 0x2;
					break;
				}

				// Effect 0x300
				case 0x10:
				{
					effect = 0x3;
					effectVal = 0;
					break;
				}

				// Effect 0x3??
				case 0x14:
				{
					effect = 0x3;
					break;
				}

				// Effect 0x400
				case 0x18:
				{
					effect = 0x4;
					effectVal = 0;
					break;
				}

				// Effect 0x4??
				case 0x1c:
				{
					effect = 0x4;
					break;
				}

				// Effect 0x5?0
				case 0x20:
				{
					effect = 0x5;
					effectVal <<= 4;
					break;
				}

				// Effect 0x50?
				case 0x24:
				{
					effect = 0x5;
					break;
				}

				// Effect 0x6?0
				case 0x28:
				{
					effect = 0x6;
					effectVal <<= 4;
					break;
				}

				// Effect 0x60?
				case 0x2c:
				{
					effect = 0x6;
					break;
				}

				// Effect 0x9??
				case 0x38:
				{
					effect = 0x9;
					break;
				}

				// Effect 0xA?0
				case 0x3c:
				{
					effect = 0xa;
					effectVal <<= 4;
					break;
				}

				// Effect 0xA0?
				case 0x40:
				{
					effect = 0xa;
					break;
				}

				// Effect 0xB??
				case 0x44:
				{
					effect = 0xb;
					break;
				}

				// Effect 0xC??
				case 0x48:
				{
					effect = 0xc;
					break;
				}

				// Effect 0xD??
				case 0x4c:
				{
					effect = 0xd;
					break;
				}

				// Effect 0xF??
				case 0x50:
				{
					effect = 0xf;
					break;
				}

				// Effect 0xE01
				case 0x58:
				{
					effect = 0xe;
					effectVal = 0x01;
					break;
				}

				// Effect 0xE1?
				case 0x5c:
				{
					effect = 0xe;
					effectVal |= 0x10;
					break;
				}

				// Effect 0xE2?
				case 0x60:
				{
					effect = 0xe;
					effectVal |= 0x20;
					break;
				}

				// Effect 0xE6?
				case 0x74:
				case 0x78:
				{
					effect = 0xe;
					effectVal |= 0x60;
					break;
				}

				// Effect 0xE9?
				case 0x84:
				{
					effect = 0xe;
					effectVal |= 0x90;
					break;
				}

				// Effect 0xEA?
				case 0x88:
				{
					effect = 0xe;
					effectVal |= 0xa0;
					break;
				}

				// Effect 0xEB?
				case 0x8c:
				{
					effect = 0xe;
					effectVal |= 0xb0;
					break;
				}

				// Effect 0xED?
				case 0x94:
				{
					effect = 0xe;
					effectVal |= 0xd0;
					break;
				}

				// Effect 0xEE?
				case 0x98:
				{
					effect = 0xe;
					effectVal |= 0xe0;
					break;
				}

				// Unknown
				default:
				{
					effect = 0x0;
					effectVal = 0x00;
					break;
				}
			}
		}
		#endregion
	}
}
