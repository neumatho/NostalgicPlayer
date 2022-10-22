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
	/// Newtron Packer 1.0
	/// </summary>
	internal class NewtronPacker10Format : ProWizardForPcBase
	{
		private byte numberOfPositions;
		private byte numberOfSamples;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT59;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 8)
				return false;

			// Check different counts
			moduleStream.Seek(0, SeekOrigin.Begin);

			ushort sampleCount = moduleStream.Read_B_UINT16();
			if ((sampleCount == 0) || ((sampleCount % 8) != 0))
				return false;

			sampleCount /= 8;

			byte temp = moduleStream.Read_UINT8();
			if (temp != 0x00)
				return false;

			byte positionListLength = moduleStream.Read_UINT8();
			if ((positionListLength == 0) || (positionListLength > 0x7f))
				return false;

			// Check sample information
			moduleStream.Seek(8, SeekOrigin.Begin);

			uint samplesSize = 0;

			for (int i = 0; i < sampleCount; i++)
			{
				ushort sampleSize = (ushort)(moduleStream.Read_B_UINT16() * 2);
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();
				ushort loopStart = (ushort)(moduleStream.Read_B_UINT16() * 2);
				ushort loopSize = (ushort)(moduleStream.Read_B_UINT16() * 2);

				if (!TestSample(sampleSize, loopStart, loopSize, volume, fineTune))
					return false;

				if ((sampleSize == 0) && (loopStart == 0) && (loopSize == 0))
					return false;

				samplesSize += sampleSize;
			}

			if (samplesSize <= 2)
				return false;

			// Find highest pattern offset
			if (FindHighestPatternNumber(moduleStream, positionListLength) > 64)
				return false;

			// Check module length
			if ((8 + sampleCount * 8 + positionListLength + numberOfPatterns * 1024 + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
				return false;

			// Check pattern data
			for (int i = 0; i < 4 * 64; i++)
			{
				temp = moduleStream.Read_UINT8();

				if ((temp & 0x0f) > 0x03)
					return false;

				if ((temp & 0xf0) > 0x10)
					return false;

				ushort temp1 = (ushort)((temp & 0x0f) * 256 + moduleStream.Read_UINT8());
				if ((temp1 > 0) && (temp1 < 0x71))
					return false;

				moduleStream.Seek(2, SeekOrigin.Current);
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

			numberOfSamples = (byte)(moduleStream.Read_B_UINT16() / 8);
			numberOfPositions = (byte)moduleStream.Read_B_UINT16();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			moduleStream.Seek(8, SeekOrigin.Begin);

			for (int i = 0; i < numberOfSamples; i++)
			{
				ushort length = moduleStream.Read_B_UINT16();
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();
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

			moduleStream.Read(positionList, 0, numberOfPositions);

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

			for (int i = 0; i < numberOfPatterns; i++)
			{
				moduleStream.Read(pattern, 0, 1024);
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
