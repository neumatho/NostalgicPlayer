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
	/// The Dark Demon
	/// </summary>
	internal class TheDarkDemonFormat : ProWizardConverterWorker31SamplesBase
	{
		private byte numberOfPositions;
		private byte restartPosition;

		private uint[] sampleAddresses;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT65;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x234)
				return false;

			// Check position list
			moduleStream.Seek(0, SeekOrigin.Begin);

			byte positionListLength = moduleStream.Read_UINT8();
			if ((positionListLength > 0x7f) || (positionListLength == 0x00))
				return false;

			if (FindHighestPatternNumber(moduleStream, 2, positionListLength) > 0x7f)
				return false;

			// Check end of position list
			for (int i = positionListLength; i < 128; i++)
			{
				if (moduleStream.Read_UINT8() != 0)
					return false;
			}

			// Check sample information
			uint samplesSize = 0;

			for (int i = 0; i < 31; i++)
			{
				uint sampleAddress = moduleStream.Read_B_UINT32();
				ushort length = (ushort)(moduleStream.Read_B_UINT16() * 2);
				moduleStream.Seek(1, SeekOrigin.Current);
				byte volume = moduleStream.Read_UINT8();
				uint loopAddress = moduleStream.Read_B_UINT32();
				ushort loopLength = (ushort)(moduleStream.Read_B_UINT16() * 2);

				if (volume > 64)
					return false;

				if (loopAddress < sampleAddress)
					return false;

				if ((sampleAddress < 564) || (loopAddress < 564))
					return false;

				if ((loopAddress - sampleAddress) > length)
					return false;

				if (((loopAddress - sampleAddress) + loopLength) > (length + 2))
					return false;

				samplesSize += length;
			}

			if ((samplesSize <= 2) || (samplesSize > 31 * 65535))
				return false;

			if ((samplesSize + 564 + numberOfPatterns * 1024) > moduleStream.Length)
				return false;

			// Check first pattern
			moduleStream.Seek(564 + samplesSize, SeekOrigin.Begin);

			for (int i = 0; i < 64 * 4; i++)
			{
				// Sample number > 31?
				if (moduleStream.Read_UINT8() > 31)
					return false;

				// Note > 0x48 (36 * 2)
				byte temp = moduleStream.Read_UINT8();
				if ((temp > 0x48) || ((temp & 0x1) != 0))
					return false;

				// Effect = 0xc and value > 64?
				temp = moduleStream.Read_UINT8();
				byte temp1 = moduleStream.Read_UINT8();

				if (((temp & 0x0f) == 0x0c) && (temp1 > 0x40))
					return false;

				// Effect = 0xd and value > 64?
				if (((temp & 0x0f) == 0x0d) && (temp1 > 0x40))
					return false;

				// Effect = 0xb and value > 127?
				if (((temp & 0x0f) == 0x0b) && (temp1 > 0x7f))
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
			moduleStream.Seek(0, SeekOrigin.Begin);

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
			sampleAddresses = new uint[31];

			moduleStream.Seek(130, SeekOrigin.Begin);

			for (int i = 0; i < 31; i++)
			{
				sampleAddresses[i] = moduleStream.Read_B_UINT32();

				ushort length = moduleStream.Read_B_UINT16();
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();
				ushort loopStart = (ushort)((moduleStream.Read_B_UINT32() - sampleAddresses[i]) / 2);
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

			moduleStream.Seek(2, SeekOrigin.Begin);
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

			int sampleSize = sampleLengths.Cast<int>().Sum();
			moduleStream.Seek(564 + sampleSize, SeekOrigin.Begin);

			for (int i = 0; i < numberOfPatterns; i++)
			{
				for (int j = 0; j < 64 * 4; j++)
				{
					byte byt1 = moduleStream.Read_UINT8();
					byte byt2 = moduleStream.Read_UINT8();
					byte byt3 = moduleStream.Read_UINT8();
					byte byt4 = moduleStream.Read_UINT8();

					// Effect value
					pattern[j * 4 + 3] = byt4;

					// Effect
					pattern[j * 4 + 2] = (byte)(byt3 & 0x0f);

					// Sample
					pattern[j * 4] = (byte)(byt1 & 0xf0);
					pattern[j * 4 + 2] |= (byte)((byt1 << 4) & 0xf0);

					// Note
					if (byt2 != 0)
					{
						byt2 /= 2;
						byt2--;

						pattern[j * 4] |= periods[byt2, 0];
						pattern[j * 4 + 1] = periods[byt2, 1];
					}
					else
						pattern[j * 4 + 1] = 0x00;
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
			for (int i = 0; i < 31; i++)
			{
				int length = (int)sampleLengths[i];
				if (length != 0)
				{
					moduleStream.Seek(sampleAddresses[i], SeekOrigin.Begin);

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
	}
}
