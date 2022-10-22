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
	/// Promizer 0.1
	/// </summary>
	internal class Promizer01Format : ProWizardConverterWorker31SamplesBase
	{
		protected byte[] fineTunes;

		private ushort realNumberOfPatterns;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT23;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x2fe)
				return false;

			// Check the size of position list
			moduleStream.Seek(0xf8, SeekOrigin.Begin);
			ushort temp = moduleStream.Read_B_UINT16();
			if ((temp > 0x200) || ((temp & 0x1) != 0))
				return false;

			// Check the first 4 pattern numbers
			uint temp1 = moduleStream.Read_B_UINT32();
			temp1 |= moduleStream.Read_B_UINT32();
			temp1 |= moduleStream.Read_B_UINT32();
			temp1 |= moduleStream.Read_B_UINT32();
			temp1 &= 0xffff03ff;
			if (temp1 != 0)
				return false;

			// Check sample information
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint samplesSize = 0;

			for (int i = 0; i < 31; i++)
			{
				// Check sample size
				temp = moduleStream.Read_B_UINT16();
				if (temp >= 0x8000)
					return false;

				if (temp == 0)
				{
					temp += moduleStream.Read_B_UINT16();
					temp += moduleStream.Read_B_UINT16();
					temp += moduleStream.Read_B_UINT16();
					if (temp != 0x0001)
						return false;
				}
				else
				{
					// Check volume & fine tune
					if ((moduleStream.Read_UINT8() > 0x0f) || (moduleStream.Read_UINT8() > 0x40))
						return false;

					// Check loop
					if ((moduleStream.Read_B_UINT16() + moduleStream.Read_B_UINT16()) > temp)
						return false;

					samplesSize += temp * 2U;
				}
			}

			// Check position list and find highest pattern number
			moduleStream.Seek(0xfa, SeekOrigin.Begin);
			uint patternCount = 0;

			for (int i = 0; i < 128; i++)
			{
				temp1 = moduleStream.Read_B_UINT32();
				if ((temp1 % 1024) != 0)
					return false;

				if (temp1 > patternCount)
					patternCount = temp1;
			}

			numberOfPatterns = (ushort)((patternCount / 1024) + 1);
			if (numberOfPatterns > 64)
				return false;

			// Check the size of all patterns
			moduleStream.Seek(0x2fa, SeekOrigin.Begin);
			temp1 = moduleStream.Read_B_UINT32();
			if ((temp1 == 0) || ((temp1 & 0x1) != 0) || ((temp1 & 0x2) != 0))
				return false;

			if (temp1 != (numberOfPatterns * 1024))
				return false;

			// Check the first pattern
			for (int i = 0; i < 64 * 4; i++)
			{
				temp = (ushort)(moduleStream.Read_B_UINT16() ^ 0xffff);
				temp &= 0x0fff;
				if (temp != 0)
				{
					if ((temp < 0x6c) || (temp > 0x38b))
						return false;
				}

				moduleStream.Seek(2, SeekOrigin.Current);
			}

			// Check the module length
			if ((0x2fe + numberOfPatterns * 1024 + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
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
			fineTunes = new byte[31];

			for (int i = 0; i < 31; i++)
			{
				ushort length = moduleStream.Read_B_UINT16();
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();
				ushort loopStart = moduleStream.Read_B_UINT16();
				ushort loopLength = moduleStream.Read_B_UINT16();

				fineTunes[i] = fineTune;

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
			moduleStream.Seek(0xf8, SeekOrigin.Begin);
			int numberOfPositions = moduleStream.Read_B_UINT16() / 4;

			realNumberOfPatterns = numberOfPatterns;
			numberOfPatterns = 0;

			byte[] positionList = new byte[numberOfPositions];
			for (int i = 0; i < numberOfPositions; i++)
			{
				positionList[i] = (byte)(moduleStream.Read_B_UINT32() / 1024);
				if (positionList[i] > numberOfPatterns)
					numberOfPatterns = positionList[i];
			}

			numberOfPatterns++;

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

			moduleStream.Seek(0x2fe, SeekOrigin.Begin);

			for (int i = 0; i < numberOfPatterns; i++)
			{
				for (int j = 0; j < 1024; j += 4)
				{
					pattern[j] = (byte)(moduleStream.Read_UINT8() ^ 0xff);
					pattern[j + 1] = (byte)(moduleStream.Read_UINT8() ^ 0xff);
					pattern[j + 2] = (byte)(moduleStream.Read_UINT8() ^ 0xff);
					pattern[j + 3] = (byte)(moduleStream.Read_UINT8() ^ 0xf0);
				}

				AdjustFineTunes(pattern);

				yield return pattern;
			}

			// If extra patterns are stored, skip them
			if (numberOfPatterns < realNumberOfPatterns)
				moduleStream.Seek((realNumberOfPatterns - numberOfPatterns) * 1024, SeekOrigin.Current);
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

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Will scan the pattern to adjust the periods
		/// </summary>
		/********************************************************************/
		protected void AdjustFineTunes(byte[] pattern)
		{
			for (int i = 0; i < 1024; i += 4)
			{
				int sampleNumber = (pattern[i] & 0x10) | ((pattern[i + 2] & 0xf0) >> 4);
				if ((sampleNumber != 0) && (fineTunes[sampleNumber - 1] != 0))
				{
					// Sample use a fine tune, so convert the period
					byte fineTune = fineTunes[sampleNumber - 1];
					ushort period = (ushort)(((pattern[i] & 0x0f) << 8) | pattern[i + 1]);

					for (int j = 0; j < 36; j++)
					{
						if (tuningPeriods[fineTune, j] == period)
						{
							pattern[i] = (byte)((pattern[i] & 0xf0) | periods[j, 0]);
							pattern[i + 1] = periods[j, 1];
							break;
						}
					}
				}
			}
		}
		#endregion
	}
}
