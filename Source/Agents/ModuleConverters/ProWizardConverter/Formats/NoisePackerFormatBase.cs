/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// NoisePacker base class for all versions
	/// </summary>
	internal abstract class NoisePackerFormatBase : ProWizardConverterWorker31SamplesBase
	{
		protected int numberOfSamples;
		private int numberOfPositions;

		private int positionListOffset;
		protected int trackDataOffset;
		private int sampleDataOffset;

		protected ushort[] trackOffsetTable;

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Prepare conversion by initialize what is needed etc.
		/// </summary>
		/********************************************************************/
		protected override bool PrepareConversion(ModuleStream moduleStream)
		{
			// Get different information
			numberOfSamples = (moduleStream.Read_B_UINT16() & 0x0ff0) >> 4;
			numberOfPositions = moduleStream.Read_B_UINT16() / 2;
			ushort trackLength = moduleStream.Read_B_UINT16();
			ushort trackDataLength = moduleStream.Read_B_UINT16();

			// Initialize offsets
			positionListOffset = 8 + numberOfSamples * 16 + 4;
			int trackListOffset = positionListOffset + numberOfPositions * 2;
			trackDataOffset = trackListOffset + trackLength;
			sampleDataOffset = trackDataOffset + trackDataLength;

			// Read track offset table
			moduleStream.Seek(trackListOffset, SeekOrigin.Begin);

			trackOffsetTable = new ushort[numberOfPatterns * 4];
			moduleStream.ReadArray_B_UINT16s(trackOffsetTable, 0, trackOffsetTable.Length);

			return true;
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

			moduleStream.Seek(positionListOffset, SeekOrigin.Begin);

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
			moduleStream.Seek(sampleDataOffset, SeekOrigin.Begin);

			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Check the sample information and return the total size of all
		/// the samples
		/// </summary>
		/********************************************************************/
		protected abstract uint CheckSampleInfo(ModuleStream moduleStream, ushort sampleCount, out int formatVersion);



		/********************************************************************/
		/// <summary>
		/// Check the pattern data
		/// </summary>
		/********************************************************************/
		protected abstract bool CheckPatternData(ModuleStream moduleStream, ushort sampleCount, ushort trackLength, ref int formatVersion);
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format and return
		/// its version. -1 for unknown
		/// </summary>
		/********************************************************************/
		protected int CheckForNoisePackerFormat(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 15)
				return -1;

			// Check the extra 'C' in number of samples
			moduleStream.Seek(0, SeekOrigin.Begin);

			ushort temp = moduleStream.Read_B_UINT16();
			if ((temp & 0xf00f) != 0x000c)
				return -1;

			// Check the number of samples
			temp &= 0x0ff0;
			if ((temp == 0) || (temp > 0x01f0))
				return -1;

			ushort sampleCount = (ushort)(temp >> 4);

			// Check the number of positions
			moduleStream.Seek(8 + sampleCount * 16, SeekOrigin.Begin);

			temp = moduleStream.Read_B_UINT16();
			if ((temp == 0) || (temp > 0xfe) || ((temp & 0x01) != 0))
				return -1;

			// The "number of positions" is stored twice, check it
			moduleStream.Seek(2, SeekOrigin.Begin);
			ushort length1 = moduleStream.Read_B_UINT16();
			if (length1 != temp)
				return -1;

			// Check track offset length
			ushort length2 = moduleStream.Read_B_UINT16();
			if ((length2 == 0) || ((length2 & 0x01) != 0))
				return -1;

			// Check track data length
			ushort length3 = moduleStream.Read_B_UINT16();
			if ((length3 == 0) || ((length3 & 0x01) != 0))
				return -1;

			// Check sample information
			uint samplesSize = CheckSampleInfo(moduleStream, sampleCount, out int formatVersion);
			if (samplesSize == 0)
				return -1;

			// Check position table and find highest pattern number
			int offset = 8 + sampleCount * 16;
			moduleStream.Seek(offset, SeekOrigin.Begin);

			numberOfPatterns = 0;
			ushort positionListLength = (ushort)(moduleStream.Read_B_UINT16() / 2);
			moduleStream.Seek(2, SeekOrigin.Current);

			for (int i = 0; i < positionListLength; i++)
			{
				temp = moduleStream.Read_B_UINT16();
				if ((temp % 8) != 0)
					return -1;

				if (temp > numberOfPatterns)
					numberOfPatterns = temp;
			}

			numberOfPatterns = (ushort)(numberOfPatterns / 8 + 1);

			// Skip track offset table
			moduleStream.Seek(numberOfPatterns * 8, SeekOrigin.Current);

			// Check pattern data
			if (!CheckPatternData(moduleStream, sampleCount, length3, ref formatVersion))
				return -1;

			// Check the module length
			uint temp1 = (uint)(8 + sampleCount * 16 + 4);
			temp1 += length1;
			temp1 += length2;
			temp1 += length3;
			temp1 += samplesSize;

			if (temp1 > (moduleStream.Length + MaxNumberOfMissingBytes))
				return -1;

			return formatVersion;
		}



		/********************************************************************/
		/// <summary>
		/// Create the note and sample number byte pair
		/// </summary>
		/********************************************************************/
		protected ValueTuple<byte, byte> BuildNoteAndSamplePair(byte dat1)
		{
			byte byt1 = 0x00;
			byte byt2 = 0x00;

			byte note = (byte)(dat1 & 0xfe);
			if (note != dat1)
				byt1 = 0x10;

			// Is there any note?
			if (dat1 > 0x01)
			{
				// Yes, find the period and "or" it together with the other bits
				note -= 2;
				note /= 2;
				byt1 |= periods[note, 0];
				byt2 = periods[note, 1];
			}

			return (byt1, byt2);
		}



		/********************************************************************/
		/// <summary>
		/// Create the effect and argument
		/// </summary>
		/********************************************************************/
		protected ValueTuple<byte, byte> BuildEffect(byte dat2, byte dat3, byte patternBreakOffset)
		{
			byte byt3 = (byte)(dat2 & 0xf0);

			// Extract the effect
			dat2 &= 0x0f;

			// Should the effect be converted?
			switch (dat2)
			{
				// Pattern break
				case 0xb:
				{
					dat3 += patternBreakOffset;
					dat3 /= 2;
					break;
				}

				// Volume slide
				case 0x7:
				{
					dat2 = 0xa;
					goto case 0x6;
				}

				// (Vibrato + volume slide) + (portamento + volume slide)
				case 0x6:
				case 0x5:
				{
					if ((sbyte)dat3 < 0)
						dat3 = (byte)(-(sbyte)dat3);
					else
						dat3 <<= 4;

					break;
				}

				// Arpeggio
				case 0x8:
				{
					dat2 = 0x0;
					break;
				}

				// Filter
				case 0xe:
				{
					if (((dat3 & 0xf0) == 0x00) && (dat3 != 0x00))
						dat3 = 0x01;

					break;
				}
			}

			return ((byte)(byt3 | dat2), dat3);
		}
		#endregion
	}
}
