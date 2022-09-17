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
	/// Channel Player base class for all versions
	/// </summary>
	internal abstract class ChannelPlayerFormatBase : ProWizardConverterWorkerBase
	{
		protected ushort sampleInfoLength;
		protected ushort positionListLength;
		private ushort patternLength;

		protected byte[] positionList;
		protected int numberOfPositions;

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Prepare conversion by initialize what is needed etc.
		/// </summary>
		/********************************************************************/
		protected override bool PrepareConversion(ModuleStream moduleStream)
		{
			// Get header information
			sampleInfoLength = moduleStream.Read_B_UINT16();
			positionListLength = moduleStream.Read_B_UINT16();
			patternLength = moduleStream.Read_B_UINT16();

			CreatePositionTable(moduleStream);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			moduleStream.Seek(10, SeekOrigin.Begin);

			int numberOfSamples = (sampleInfoLength & 0xfff0) >> 4;
			for (int i = 0; i < numberOfSamples; i++)
			{
				uint address = moduleStream.Read_B_UINT32();
				ushort length = moduleStream.Read_B_UINT16();
				moduleStream.Seek(1, SeekOrigin.Current);
				byte volume = moduleStream.Read_UINT8();
				uint loopAddress = moduleStream.Read_B_UINT32();
				ushort loopLength = moduleStream.Read_B_UINT16();
				ushort fineTune = moduleStream.Read_B_UINT16();

				yield return new SampleInfo
				{
					Name = null,
					Length = length,
					LoopStart = (ushort)((loopAddress - address) / 2),
					LoopLength = loopLength,
					Volume = volume,
					FineTune = (byte)(fineTune / 0x48)
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
			return positionList.AsSpan(0, numberOfPositions);
		}



		/********************************************************************/
		/// <summary>
		/// Write all the samples
		/// </summary>
		/********************************************************************/
		protected override bool WriteSampleData(ModuleStream moduleStream, ConverterStream converterStream)
		{
			moduleStream.Seek(sampleInfoLength + positionListLength + patternLength, SeekOrigin.Begin);

			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Build the position table structure
		/// </summary>
		/********************************************************************/
		protected abstract void CreatePositionTable(ModuleStream moduleStream);
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format and return
		/// its version. -1 for unknown
		/// </summary>
		/********************************************************************/
		protected int CheckForChannelPlayerFormat(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 26)
				return -1;

			// Get first bytes
			moduleStream.Seek(0, SeekOrigin.Begin);

			// Check number of samples (each sample is 0x10 bytes in length and start at offset 0xa)
			ushort sampleInfoLen = moduleStream.Read_B_UINT16();
			if ((sampleInfoLen & 0xf00f) != 0x000a)
				return -1;

			ushort temp = (ushort)(sampleInfoLen & 0x0ff0);
			if ((temp == 0) || (temp > 0x01f0))
				return -1;

			// Check size of position table
			ushort positionListLen = moduleStream.Read_B_UINT16();
			if (positionListLen == 0)
				return -1;

			// Check size of patterns
			ushort patternLen = moduleStream.Read_B_UINT16();
			if (patternLen == 0)
				return -1;

			// Check sample information and length
			uint sampleLength = moduleStream.Read_B_UINT32();

			int numberOfSamples = sampleInfoLen >> 4;
			uint calculatedSampleLength = 0;

			for (int i = 0; i < numberOfSamples; i++)
			{
				// Check "memory address" to sample data
				uint sampleAddress = moduleStream.Read_B_UINT32();
				if (sampleAddress > 0x1fe000)
					return -1;

				// Get sample size
				ushort singleSampleLength = moduleStream.Read_B_UINT16();
				if (singleSampleLength >= 0x8000)
					return -1;

				calculatedSampleLength += (uint)singleSampleLength * 2;

				// Check volume
				if (moduleStream.Read_B_UINT16() > 0x40)
					return -1;

				// Check "memory address" to loop start
				uint loopAddress = moduleStream.Read_B_UINT32();
				if (loopAddress > 0x1fe000)
					return -1;

				if (singleSampleLength == 0)
				{
					// Sample start - loop start should be 0
					if ((loopAddress - sampleAddress) != 0)
						return -1;
				}

				// Skip loop length
				moduleStream.Seek(2, SeekOrigin.Current);

				// Check fine tune
				temp = moduleStream.Read_B_UINT16();
				if ((temp % 0x48) != 0)
					return -1;
			}

			// Check sample length
			if (calculatedSampleLength != sampleLength)
				return -1;

			// Check module length
			if ((sampleInfoLen + positionListLen + patternLen + calculatedSampleLength) > (moduleStream.Length + MaxNumberOfMissingBytes))
				return -1;

			// Get offsets to patterns and samples
			int patternOffset = sampleInfoLen + positionListLen;
			int sampleOffset = patternOffset + patternLen - 4;		// -4 because of the test

			// v1 note test
			bool found = false;
			int foundVersion;

			moduleStream.Seek(patternOffset, SeekOrigin.Begin);

			while (moduleStream.Position < sampleOffset)
			{
				// Find any 0x80 commands. They are not possible in v1
				if ((moduleStream.Read_UINT8() == 0x80) && (moduleStream.Read_UINT8() <= 0x0f))
				{
					found = true;
					break;
				}
			}

			if (!found)
				foundVersion = 1;
			else
			{
				// v2 or v3
				foundVersion = 2;

				// Test for v3
				moduleStream.Seek(patternOffset, SeekOrigin.Begin);

				while (moduleStream.Position < sampleOffset)
				{
					// Only v3 can have 4 0x80 right after each other
					if (moduleStream.Read_B_UINT32() == 0x80808080)
					{
						foundVersion = 3;
						break;
					}
				}
			}

			return foundVersion;
		}
		#endregion
	}
}
