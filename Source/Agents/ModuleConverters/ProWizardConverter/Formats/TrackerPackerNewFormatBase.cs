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
	/// Tracker Packer base class for version 2 and 3
	/// </summary>
	internal abstract class TrackerPackerNewFormatBase : ProWizardConverterWorker31SamplesBase
	{
		private ushort numberOfSamples;

		private byte numberOfPositions;
		private ushort[,] trackOffsetTable;

		private uint noteStartOffset;
		private uint sampleOffset;

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Prepare conversion by initialize what is needed etc.
		/// </summary>
		/********************************************************************/
		protected override bool PrepareConversion(ModuleStream moduleStream)
		{
			moduleStream.Seek(0x1c, SeekOrigin.Begin);
			numberOfSamples = (ushort)(moduleStream.Read_B_UINT16() / 8);

			CreateOffsetTable(moduleStream);

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

			moduleStream.Seek(8, SeekOrigin.Begin);
			moduleStream.Read(moduleName, 0, 20);

			return moduleName;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			moduleStream.Seek(0x1e, SeekOrigin.Begin);

			for (int i = 0; i < numberOfSamples; i++)
			{
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();
				ushort length = moduleStream.Read_B_UINT16();
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
			byte[] positionList = new byte[numberOfPositions];

			moduleStream.Seek(0x1e + numberOfSamples * 8 + 2, SeekOrigin.Begin);

			for (int i = 0; i < numberOfPositions; i++)
				positionList[i] = (byte)(moduleStream.Read_B_UINT16() / 8);

			return positionList;
		}



		/********************************************************************/
		/// <summary>
		/// Write all the samples
		/// </summary>
		/********************************************************************/
		protected override bool WriteSampleData(ModuleStream moduleStream, ConverterStream converterStream)
		{
			moduleStream.Seek(sampleOffset, SeekOrigin.Begin);

			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected bool CheckForTrackerPackerFormat(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x1e)
				return false;

			// Get number of samples
			moduleStream.Seek(0x1c, SeekOrigin.Begin);

			ushort temp = moduleStream.Read_B_UINT16();
			if (temp == 0)
				return false;

			temp /= 8;

			// Check sample information
			uint samplesSize = 0;

			for (int i = 0; i < temp; i++)
			{
				moduleStream.Seek(2, SeekOrigin.Current);

				// Check sample size
				ushort temp1 = moduleStream.Read_B_UINT16();
				if (temp1 == 0)
					return false;

				samplesSize += temp1 * 2U;

				moduleStream.Seek(4, SeekOrigin.Current);
			}

			// Get size of position table
			uint offset = 0x1e + temp * 8U;
			moduleStream.Seek(offset, SeekOrigin.Begin);
			ushort positionListLength = moduleStream.Read_B_UINT16();

			// Find highest pattern number
			numberOfPatterns = 0;

			for (int i = 0; i < positionListLength; i++)
			{
				temp = (ushort)(moduleStream.Read_B_UINT16() / 8);
				if (temp > numberOfPatterns)
					numberOfPatterns = temp;
			}

			numberOfPatterns++;

			offset = (uint)moduleStream.Position;
			offset += (uint)(numberOfPatterns * 8 + 2);
			moduleStream.Seek(offset - 2, SeekOrigin.Begin);
			offset += moduleStream.Read_B_UINT16();

			// Check module length
			if ((offset + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected IEnumerable<byte[]> GetAllPatterns(ModuleStream moduleStream, int version)
		{
			byte[] pattern = new byte[1024];

			for (int i = 0; i < numberOfPatterns; i++)
			{
				// Clear the pattern data
				Array.Clear(pattern);

				for (int j = 0; j < 4; j++)
				{
					uint noteOffset = trackOffsetTable[j, i];
					moduleStream.Seek(noteStartOffset + noteOffset, SeekOrigin.Begin);

					for (int k = 0; k < 64; k++)
					{
						// Get first byte
						byte temp1 = moduleStream.Read_UINT8();

						// Is it empty lines?
						if (temp1 >= 0xc0)
						{
							k += unchecked((-(sbyte)temp1) - 1);
							continue;
						}

						// Effect only?
						if (temp1 >= 0x80)
						{
							temp1 -= 0x80;

							if (version == 2)
								temp1 >>= 2;
							else
								temp1 >>= 1;

							temp1 &= 0x0f;
							byte temp2 = moduleStream.Read_UINT8();
							ConvertEffect(ref temp1, ref temp2);

							pattern[k * 16 + j * 4 + 2] = temp1;
							pattern[k * 16 + j * 4 + 3] = temp2;
							continue;
						}

						// Find out if the sample is >= 0x10
						if (version == 2)
						{
							if ((temp1 & 0x01) != 0)
							{
								pattern[k * 16 + j * 4] = 0x10;		// Hi bit in sample number
								temp1 &= 0xfe;
							}
						}
						else
						{
							if (temp1 >= 0x5b)
							{
								pattern[k * 16 + j * 4] = 0x10;		// Hi bit in sample number
								temp1 = (byte)unchecked(-(sbyte)temp1 - 0x81);
							}
						}

						if (temp1 != 0)
						{
							// Got a note, convert it to a period
							if (version == 2)
							{
								temp1 -= 2;
								temp1 /= 2;
							}
							else
								temp1--;

							pattern[k * 16 + j * 4] |= periods[temp1, 0];
							pattern[k * 16 + j * 4 + 1] = periods[temp1, 1];
						}

						// Store sample number + effect + effect value
						temp1 = moduleStream.Read_UINT8();
						pattern[k * 16 + j * 4 + 2] = (byte)(temp1 & 0xf0);
						temp1 &= 0x0f;

						if (temp1 != 0)
						{
							byte temp2 = moduleStream.Read_UINT8();
							ConvertEffect(ref temp1, ref temp2);

							pattern[k * 16 + j * 4 + 2] |= temp1;
							pattern[k * 16 + j * 4 + 3] = temp2;
						}
					}
				}

				yield return pattern;
			}
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
			moduleStream.Seek(0x1e + numberOfSamples * 8, SeekOrigin.Begin);
			numberOfPositions = (byte)moduleStream.Read_B_UINT16();

			moduleStream.Seek(numberOfPositions * 2, SeekOrigin.Current);

			trackOffsetTable = new ushort[4, numberOfPatterns];

			for (int i = 0; i < numberOfPatterns; i++)
			{
				for (int j = 0; j < 4; j++)
					trackOffsetTable[j, i] = moduleStream.Read_B_UINT16();
			}

			noteStartOffset = (uint)moduleStream.Position + 2;
			sampleOffset = noteStartOffset + moduleStream.Read_B_UINT16();
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
				// Arpeggio
				case 0x8:
				{
					effect = 0;
					break;
				}

				// Volume slide commands
				case 0x5:
				case 0x6:
				case 0xa:
				{
					if ((sbyte)effectVal < 0)
						effectVal = (byte)-(sbyte)effectVal;
					else
						effectVal <<= 4;

					break;
				}
			}
		}
		#endregion
	}
}
