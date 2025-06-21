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
	/// The Player base class for version 2.2A and 3.0A
	/// </summary>
	internal abstract class ThePlayer2x_3xFormatBase : ProWizardConverterWorker31SamplesBase
	{
		private byte numberOfSamples;
		private byte numberOfPositions;
		private ushort[] trackOffsetTable;
		private byte[] positionList;

		protected uint trackDataOffset;
		private uint trackTableOffset;
		private uint sampleDataOffset;

		private uint[] sampleStartOffsets;

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Prepare conversion by initialize what is needed etc.
		/// </summary>
		/********************************************************************/
		protected override bool PrepareConversion(ModuleStream moduleStream)
		{
			// Get different information
			moduleStream.Seek(5, SeekOrigin.Begin);

			numberOfPositions = (byte)(moduleStream.Read_UINT8() / 2);
			numberOfSamples = moduleStream.Read_UINT8();

			moduleStream.Seek(1, SeekOrigin.Current);
			trackDataOffset = moduleStream.Read_B_UINT32();
			trackTableOffset = moduleStream.Read_B_UINT32();
			sampleDataOffset = moduleStream.Read_B_UINT32();

			CreateOffsetTable(moduleStream);
			CreatePositionList();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			sampleStartOffsets = new uint[numberOfSamples];

			moduleStream.Seek(20, SeekOrigin.Begin);

			for (int i = 0; i < numberOfSamples; i++)
			{
				sampleStartOffsets[i] = moduleStream.Read_B_UINT32();
				ushort length = moduleStream.Read_B_UINT16();
				uint loopStart = moduleStream.Read_B_UINT32();
				ushort loopLength = moduleStream.Read_B_UINT16();
				ushort fineTune = moduleStream.Read_B_UINT16();
				ushort volume = moduleStream.Read_B_UINT16();

				yield return new SampleInfo
				{
					Name = null,
					Length = length,
					LoopStart = (ushort)((loopStart - sampleStartOffsets[i]) / 2),
					LoopLength = loopLength,
					Volume = (byte)volume,
					FineTune = (byte)(fineTune / 74)
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
			return positionList;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			byte[] pattern = new byte[1024];
			int[] voiceRowIndex = new int[4];

			int lastPatternNumber = -1;

			for (int i = 0; i < numberOfPositions; i++)
			{
				if (positionList[i] > lastPatternNumber)
				{
					lastPatternNumber++;

					// Clear the pattern data
					Array.Clear(pattern);

					voiceRowIndex[0] = voiceRowIndex[1] = voiceRowIndex[2] = voiceRowIndex[3] = 0;

					ushort trackOffset = trackOffsetTable[i];
					moduleStream.Seek(trackDataOffset + 4 + trackOffset, SeekOrigin.Begin);

					// Build each channel from the tracks
					for (int k = 0; k < 64; k++)
					{
						for (int j = 0; j < 4; j++)
						{
							if (voiceRowIndex[j] > k)
								continue;

							byte c1 = moduleStream.Read_UINT8();
							byte c2 = moduleStream.Read_UINT8();
							byte c3 = moduleStream.Read_UINT8();
							byte c4 = moduleStream.Read_UINT8();

							ConvertPatternData(moduleStream, c1, c2, c3, c4, j, pattern, voiceRowIndex);
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
		protected override bool WriteSampleData(PlayerFileInfo fileInfo, ModuleStream moduleStream, ConverterStream converterStream)
		{
			uint sampleOffset = sampleDataOffset + 4;

			for (int i = 0; i < numberOfSamples; i++)
			{
				int length = (int)sampleLengths[i];
				if (length != 0)
				{
					moduleStream.Seek(sampleOffset + sampleStartOffsets[i], SeekOrigin.Begin);

					// Check to see if we miss too much from the last sample
					if (moduleStream.Length - moduleStream.Position < (length - MaxNumberOfMissingBytes))
						return false;

					moduleStream.SetSampleDataInfo(i, length);
					converterStream.WriteSampleDataMarker(i, length);
				}
			}

			return true;
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Convert a single row on a single channel and writes it in the
		/// pattern
		/// </summary>
		/********************************************************************/
		protected abstract void ConvertPatternData(ModuleStream moduleStream, byte c1, byte c2, byte c3, byte c4, int channel, byte[] pattern, int[] voiceRowIndex);



		/********************************************************************/
		/// <summary>
		/// Convert the effect 5, 6 and A value
		/// pattern
		/// </summary>
		/********************************************************************/
		protected abstract byte ConvertEffect56AValue(byte c3);
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected bool CheckForThePlayerFormat(ModuleStream moduleStream, string mark)
		{
			if (moduleStream.Length < 20)
				return false;

			// Start to check the ID
			moduleStream.Seek(0, SeekOrigin.Begin);

			if (moduleStream.ReadMark() != mark)
				return false;

			// Get number of patterns
			numberOfPatterns = moduleStream.Read_UINT8();
			if (numberOfPatterns > 0x7f)
				return false;

			// Get number of samples
			moduleStream.Seek(1, SeekOrigin.Current);
			byte sampleCount = moduleStream.Read_UINT8();
			if ((sampleCount > 0x1f) || (sampleCount == 0))
				return false;

			// Check sample information
			moduleStream.Seek(20, SeekOrigin.Begin);
			if (moduleStream.Read_B_UINT32() != 0)
				return false;

			moduleStream.Seek(-4, SeekOrigin.Current);

			uint samplesSize = 0;

			for (int i = 0; i < sampleCount; i++)
			{
				moduleStream.Seek(4, SeekOrigin.Current);
				uint length = moduleStream.Read_B_UINT16() * 2U;

				moduleStream.Seek(4, SeekOrigin.Current);
				uint loopLength = moduleStream.Read_B_UINT16() * 2U;

				if ((length > 0xffff) || (loopLength > 0xffff))
					return false;

				if (loopLength > (length + 2))
					return false;

				samplesSize += length;

				moduleStream.Seek(3, SeekOrigin.Current);
				if (moduleStream.Read_UINT8() > 0x40)
					return false;
			}

			if (samplesSize <= 4)
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Convert a single row on a single channel and writes it in the
		/// pattern
		/// </summary>
		/********************************************************************/
		protected (byte byt1, byte byt2, byte byt3, byte byt4) ConvertData(byte c1, byte c2, byte c3)
		{
			byte sample = (byte)(((c1 << 4) & 0x10) | ((c2 >> 4) & 0x0f));
			byte note = (byte)(c1 & 0x7f);

			byte period1, period2;

			if (note != 0)
			{
				note /= 2;
				note--;

				period1 = periods[note, 0];
				period2 = periods[note, 1];
			}
			else
				period1 = period2 = 0;

			switch (c2 & 0x0f)
			{
				case 0x08:
				{
					c2 -= 0x08;
					break;
				}

				case 0x05:
				case 0x06:
				case 0x0a:
				{
					c3 = ConvertEffect56AValue(c3);
					break;
				}
			}

			return ((byte)((sample & 0xf0) | (period1 & 0x0f)), period2, c2, c3);
		}



		/********************************************************************/
		/// <summary>
		/// Store the given bytes in the pattern
		/// </summary>
		/********************************************************************/
		protected void StoreData(byte byt1, byte byt2, byte byt3, byte byt4, int channel, int row, byte[] pattern)
		{
			pattern[row * 16 + channel * 4] = byt1;
			pattern[row * 16 + channel * 4 + 1] = byt2;
			pattern[row * 16 + channel * 4 + 2] = byt3;
			pattern[row * 16 + channel * 4 + 3] = byt4;
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
			trackOffsetTable = new ushort[numberOfPositions];

			moduleStream.Seek(trackTableOffset + 4, SeekOrigin.Begin);
			moduleStream.ReadArray_B_UINT16s(trackOffsetTable, 0, numberOfPositions);
		}



		/********************************************************************/
		/// <summary>
		/// Create position list and find the number of patterns needed
		/// </summary>
		/********************************************************************/
		private void CreatePositionList()
		{
			positionList = BuildPositionList(numberOfPositions, (pos) => trackOffsetTable[pos]);
		}
		#endregion
	}
}
