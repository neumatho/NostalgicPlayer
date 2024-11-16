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
	/// Wanton Packer
	/// </summary>
	internal class WantonPackerFormat : ProWizardConverterWorker31SamplesBase
	{
		protected byte restartPosition;
		protected ushort realNumberOfPatterns;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT46;
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

			uint temp = moduleStream.Read_B_UINT32();
			if ((temp & 0xffffff00) != 0x574e0000)		// WN\0
				return false;

			// Check number of patterns
			numberOfPatterns = (ushort)(temp & 0x000000ff);
			if ((numberOfPatterns == 0) || (numberOfPatterns > 64))
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
				moduleStream.Seek(22, SeekOrigin.Current);

				// Check sample size
				ushort temp1 = moduleStream.Read_B_UINT16();
				if (temp1 >= 0x8000)
					return false;

				samplesSize += temp1 * 2U;

				// Check volume and fine tune
				if ((moduleStream.Read_UINT8() > 0x0f) || (moduleStream.Read_UINT8() > 0x40))
					return false;

				// Check loop values
				if (temp1 != 0)
				{
					uint temp2 = moduleStream.Read_B_UINT16();
					uint temp3 = moduleStream.Read_B_UINT16();

					if ((temp2 + temp3) > temp1)
						return false;
				}
				else
					moduleStream.Seek(4, SeekOrigin.Current);
			}

			// Check the module length
			if ((0x43c + numberOfPatterns * 1024 + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
				return false;

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

			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.ReadInto(moduleName, 0, 20);

			return moduleName;
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
				moduleStream.ReadInto(name, 0, 22);

				ushort length = moduleStream.Read_B_UINT16();
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();
				ushort loopStart = moduleStream.Read_B_UINT16();
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

			// We don't want to save unused patterns
			moduleStream.Seek(3, SeekOrigin.Current);
			realNumberOfPatterns = moduleStream.Read_UINT8();

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

			for (int i = 0; i < numberOfPatterns; i++)
			{
				for (int j = 0; j < 64 * 4; j++)
				{
					byte byt1, byt2;

					// Convert note
					byte temp = moduleStream.Read_UINT8();
					if (temp != 0)
					{
						temp -= 2;
						temp /= 2;
						byt1 = periods[temp, 0];
						byt2 = periods[temp, 1];
					}
					else
					{
						byt1 = 0x00;
						byt2 = 0x00;
					}

					// Get sample number
					temp = moduleStream.Read_UINT8();

					if (temp >= 0x10)
					{
						byt1 |= 0x10;
						temp -= 0x10;
					}

					byte byt3 = (byte)(temp << 4);

					// Get effect
					byt3 |= moduleStream.Read_UINT8();
					byte byt4 = moduleStream.Read_UINT8();

					// Copy the pattern data
					pattern[j * 4] = byt1;
					pattern[j * 4 + 1] = byt2;
					pattern[j * 4 + 2] = byt3;
					pattern[j * 4 + 3] = byt4;
				}

				yield return pattern;
			}

			// If extra patterns are stored, skip them
			if (numberOfPatterns < realNumberOfPatterns)
				moduleStream.Seek((realNumberOfPatterns - numberOfPatterns) * 0x300, SeekOrigin.Current);
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
