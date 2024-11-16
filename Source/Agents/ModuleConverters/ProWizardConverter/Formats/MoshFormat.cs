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
	/// MOSH
	/// </summary>
	internal class MoshFormat : ProWizardForPcBase
	{
		private byte numberOfPositions;
		private byte restartPosition;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT58;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x17e)
				return false;

			// Start to check the mark
			moduleStream.Seek(0x17a, SeekOrigin.Begin);
			if (moduleStream.Read_B_UINT32() != 0x4d2e4b2e)		// M.K.
				return false;

			// Check sample information
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint samplesSize = 0;

			for (int i = 0; i < 31; i++)
			{
				ushort loopSize = (ushort)(moduleStream.Read_B_UINT16() * 2);
				ushort loopStart = (ushort)(moduleStream.Read_B_UINT16() * 2);
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();
				ushort sampleSize = (ushort)(moduleStream.Read_B_UINT16() * 2);

				if (!TestSample(sampleSize, loopStart, loopSize, volume, fineTune))
					return false;

				samplesSize += sampleSize;
			}

			// Check size of position table
			byte temp = moduleStream.Read_UINT8();
			if ((temp == 0) || (temp > 0x7f))
				return false;

			// Find highest pattern offset
			if (FindHighestPatternNumber(moduleStream, 250, 128) > 64)
				return false;

			// Check module length
			if ((0x17e + numberOfPatterns * 1024 + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
				return false;

			// Check pattern data
			moduleStream.Seek(0x17e, SeekOrigin.Begin);

			for (int i = 0; i < 4 * 64; i++)
			{
				temp = moduleStream.Read_UINT8();
				if (temp > 0x13)
					return false;

				ushort temp1 = (ushort)((temp & 0x0f) * 256 + moduleStream.Read_UINT8());
				if ((temp1 > 0) && (temp1 < 0x1c))
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
			moduleStream.Seek(0xf8, SeekOrigin.Begin);

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
			moduleStream.Seek(0, SeekOrigin.Begin);

			for (int i = 0; i < 31; i++)
			{
				ushort loopLength = moduleStream.Read_B_UINT16();
				ushort loopStart = moduleStream.Read_B_UINT16();
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();
				ushort length = moduleStream.Read_B_UINT16();

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

			moduleStream.Seek(0xfa, SeekOrigin.Begin);
			moduleStream.ReadInto(positionList, 0, numberOfPositions);

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
			moduleStream.Seek(0x17e, SeekOrigin.Begin);

			byte[] pattern = new byte[1024];

			for (int i = 0; i < numberOfPatterns; i++)
			{
				moduleStream.ReadInto(pattern, 0, 1024);

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
