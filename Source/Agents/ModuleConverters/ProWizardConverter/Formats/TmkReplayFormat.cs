/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// TMK Replay
	/// </summary>
	internal class TmkReplayFormat : ProWizardConverterWorker31SamplesBase
	{
		private byte numberOfPositions;
		private byte numberOfSamples;

		private bool sampleDelta;
		private long sampleStartOffset;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT69;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 100)
				return false;

			// Start to check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);
			if (moduleStream.Read_B_UINT32() != 0x544d4b01)		// TMK\1
				return false;

			// Check position list length
			byte positionListLength = moduleStream.Read_UINT8();
			if (positionListLength > 0x80)
				return false;

			// Check number of samples
			byte sampleCount = (byte)(moduleStream.Read_UINT8() & 0x7f);
			if (sampleCount > 0x1f)
				return false;

			if ((6 + sampleCount * 8 + positionListLength) > moduleStream.Length)
				return false;

			// Check sample information
			uint samplesSize = 0;

			for (int i = 0; i < sampleCount; i++)
			{
				ushort temp = moduleStream.Read_B_UINT16();
				samplesSize += temp * 2U;

				moduleStream.Seek(4, SeekOrigin.Current);

				if (moduleStream.Read_UINT8() > 0x0f)
					return false;

				if (moduleStream.Read_UINT8() > 0x40)
					return false;
			}

			FindHighestPatternNumber(moduleStream, positionListLength);
			numberOfPatterns = (ushort)((numberOfPatterns - 1) / 2 + 1);

			// Check module length
			if ((6 + sampleCount * 8 + positionListLength + numberOfPatterns * 64 * 2 + samplesSize) > moduleStream.Length)
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
			moduleStream.Seek(4, SeekOrigin.Begin);

			numberOfPositions = moduleStream.Read_UINT8();
			numberOfSamples = moduleStream.Read_UINT8();

			sampleDelta = (numberOfSamples & 0x80) != 0;
			numberOfSamples &= 0x7f;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			moduleStream.Seek(6, SeekOrigin.Begin);

			for (int i = 0; i < numberOfSamples; i++)
			{
				ushort length = moduleStream.Read_B_UINT16();
				ushort loopStart = moduleStream.Read_B_UINT16();
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
				positionList[i] = (byte)(moduleStream.Read_UINT8() / 2);

			// If number of positions is odd, there is a padding byte we need to skip
			if ((numberOfPositions % 2) != 0)
				moduleStream.Seek(1, SeekOrigin.Current);

			return positionList;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			sampleStartOffset = 0;

			long referenceStartOffset = moduleStream.Position;
			ushort[,] referenceOffsets = ReadReferenceOffsets(moduleStream);

			byte[] pattern = new byte[1024];

			for (int i = 0; i < numberOfPatterns; i++)
			{
				// Clear pattern
				Array.Clear(pattern);

				for (int j = 0; j < 64; j++)
				{
					int offset = referenceOffsets[i, j];
					moduleStream.Seek(referenceStartOffset + offset, SeekOrigin.Begin);

					for (int k = 0; k < 4; k++)
					{
						byte temp = moduleStream.Read_UINT8();

						if (temp == 0x80)
							continue;

						if ((temp & 0xc0) == 0xc0)
						{
							// Only effects
							pattern[j * 16 + k * 4 + 2] = (byte)(temp & 0x0f);
							pattern[j * 16 + k * 4 + 3] = moduleStream.Read_UINT8();
							continue;
						}

						if ((temp & 0xc0) == 0x80)
						{
							// Note + sample
							temp &= 0x7f;

							if (temp != 0)
							{
								temp--;

								pattern[j * 16 + k * 4] = periods[temp, 0];
								pattern[j * 16 + k * 4 + 1] = periods[temp, 1];
							}

							temp = moduleStream.Read_UINT8();

							pattern[j * 16 + k * 4] |= (byte)(temp & 0xf0);
							pattern[j * 16 + k * 4 + 2] = (byte)(temp << 4);
							continue;
						}

						pattern[j * 16 + k * 4] = (byte)((temp & 0x01) << 4);
						pattern[j * 16 + k * 4 + 2] = moduleStream.Read_UINT8();
						pattern[j * 16 + k * 4 + 3] = moduleStream.Read_UINT8();

						temp &= 0xfe;
						if (temp != 0)
						{
							temp /= 2;
							temp--;

							pattern[j * 16 + k * 4] |= periods[temp, 0];
							pattern[j * 16 + k * 4 + 1] = periods[temp, 1];
						}
					}

					if (moduleStream.Position > sampleStartOffset)
						sampleStartOffset = moduleStream.Position;
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

			if (sampleDelta)
			{
				for (int i = 0; i < numberOfSamples; i++)
				{
					int length = (int)sampleLengths[i];
					if (length != 0)
					{
						// Check to see if we miss too much from the last sample
						if (moduleStream.Length - moduleStream.Position < (length - MaxNumberOfMissingBytes))
							return false;

						sbyte[] sampleData = new sbyte[length];

						sbyte c1 = moduleStream.Read_INT8();
						sampleData[0] = c1;

						for (int j = 1; j < length; j++)
						{
							sampleData[j] = (sbyte)(c1 - moduleStream.Read_INT8());
							c1 = sampleData[j];
						}

						converterStream.Write(MemoryMarshal.Cast<sbyte, byte>(sampleData));
					}
				}

				return true;
			}

			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Read all the reference offsets
		/// </summary>
		/********************************************************************/
		private ushort[,] ReadReferenceOffsets(ModuleStream moduleStream)
		{
			ushort[,] referenceOffsets = new ushort[numberOfPatterns, 64];

			for (int i = 0; i < numberOfPatterns; i++)
			{
				for (int j = 0; j < 64; j++)
					referenceOffsets[i, j] = moduleStream.Read_B_UINT16();
			}

			return referenceOffsets;
		}
		#endregion
	}
}
