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
	/// Polka Packer
	/// </summary>
	internal class PolkaPackerFormat : WantonPackerFormat
	{
		private int sampleLoopDivision;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT21;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x43c)
				return false;

			// Start to check the ID
			moduleStream.Seek(0x438, SeekOrigin.Begin);

			string temp = moduleStream.ReadMark();
			if (temp == "PWR.")
				sampleLoopDivision = 2;
			else if (temp == "PSUX")
				sampleLoopDivision = 1;
			else
				return false;

			// Check some of the pattern data in first pattern
			for (int i = 0; i < 64 * 4; i++)
			{
				moduleStream.Seek(1, SeekOrigin.Current);

				// Check sample number
				if (moduleStream.Read_UINT8() > 0x1f)
					return false;

				moduleStream.Seek(2, SeekOrigin.Current);
			}

			// Check sample information
			moduleStream.Seek(20, SeekOrigin.Begin);

			uint samplesSize = 0;

			for (int i = 0; i < 31; i++)
			{
				moduleStream.Seek(18, SeekOrigin.Current);

				// Check start address
				if (moduleStream.Read_B_UINT32() == 0)
					return false;

				// Check sample size
				ushort temp1 = moduleStream.Read_B_UINT16();
				if (temp1 > 0x8000)
					return false;

				samplesSize += temp1 * 2U;

				// Check volume and fine tune
				if ((moduleStream.Read_UINT8() > 0x0f) || (moduleStream.Read_UINT8() > 0x40))
					return false;

				// Check loop values
				if (temp1 != 0)
				{
					uint temp2 = (ushort)(moduleStream.Read_B_UINT16() / sampleLoopDivision);
					uint temp3 = moduleStream.Read_B_UINT16();

					if ((temp2 + temp3) > temp1)
						return false;
				}
				else
					moduleStream.Seek(4, SeekOrigin.Current);
			}

			// Check highest pattern number
			moduleStream.Seek(0x3b6, SeekOrigin.Begin);

			byte positionListLength = moduleStream.Read_UINT8();

			if (FindHighestPatternNumber(moduleStream, 0x3b8, positionListLength) > 64)
				return false;

			// Check the module length
			if ((0x43c + numberOfPatterns * 1024 + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
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
			for (int i = 0; i < 31; i++)
			{
				byte[] name = new byte[22];
				moduleStream.ReadInto(name, 0, 18);

				moduleStream.Seek(4, SeekOrigin.Current);

				ushort length = moduleStream.Read_B_UINT16();
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();
				ushort loopStart = (ushort)(moduleStream.Read_B_UINT16() / sampleLoopDivision);
				ushort loopLength = moduleStream.Read_B_UINT16();

				yield return new SampleInfo
				{
					Name = name,
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
			byte numberOfPositions = moduleStream.Read_UINT8();

			restartPosition = moduleStream.Read_UINT8();

			byte[] positionList = new byte[numberOfPositions];
			moduleStream.ReadInto(positionList, 0, numberOfPositions);

			moduleStream.Seek(128 - numberOfPositions, SeekOrigin.Current);
			moduleStream.Seek(4, SeekOrigin.Current);

			return positionList;
		}
		#endregion
	}
}
