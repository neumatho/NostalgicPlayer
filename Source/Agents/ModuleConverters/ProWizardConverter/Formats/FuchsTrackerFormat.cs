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
	/// Fuchs Tracker
	/// </summary>
	internal class FuchsTrackerFormat : ProWizardConverterWorker31SamplesBase
	{
		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT52;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 196)
				return false;

			// Start to check the mark
			moduleStream.Seek(192, SeekOrigin.Begin);
			if (moduleStream.Read_B_UINT32() != 0x534f4e47)		// SONG
				return false;

			// All sample size
			moduleStream.Seek(10, SeekOrigin.Begin);

			uint totalSamplesSize = moduleStream.Read_B_UINT32();
			if ((totalSamplesSize <= 2) || (totalSamplesSize >= (65535 * 16)))
				return false;

			// Check sample information
			ushort[] sampleSizes = new ushort[16];
			ushort[] loopStarts = new ushort[16];
			ushort[] volumes = new ushort[16];

			moduleStream.ReadArray_B_UINT16s(sampleSizes, 0, 16);
			moduleStream.ReadArray_B_UINT16s(volumes, 0, 16);
			moduleStream.ReadArray_B_UINT16s(loopStarts, 0, 16);

			uint samplesSize = 0;

			for (int i = 0; i < 16; i++)
			{
				if (volumes[i] > 0x40)
					return false;

				if (sampleSizes[i] < loopStarts[i])
					return false;

				samplesSize += sampleSizes[i];
			}

			if ((samplesSize <= 2) || (samplesSize > totalSamplesSize))
				return false;

			// Find highest pattern number
			moduleStream.Seek(112, SeekOrigin.Begin);

			numberOfPatterns = 0;

			for (int i = 0; i < 40; i++)
			{
				ushort temp = moduleStream.Read_B_UINT16();
				if (temp > 40)
					return false;

				if (temp > numberOfPatterns)
					numberOfPatterns = temp;
			}

			numberOfPatterns++;

			// Check module length
			if ((200 + numberOfPatterns * 1024 + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
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
			moduleStream.ReadInto(moduleName, 0, 10);

			return moduleName;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			ushort[] sampleSizes = new ushort[16];
			ushort[] loopStarts = new ushort[16];
			ushort[] volumes = new ushort[16];

			moduleStream.Seek(14, SeekOrigin.Begin);

			moduleStream.ReadArray_B_UINT16s(sampleSizes, 0, 16);
			moduleStream.ReadArray_B_UINT16s(volumes, 0, 16);
			moduleStream.ReadArray_B_UINT16s(loopStarts, 0, 16);

			for (int i = 0; i < 16; i++)
			{
				ushort length = (ushort)(sampleSizes[i] / 2);
				byte volume = (byte)volumes[i];
				ushort loopStart = (ushort)(loopStarts[i] / 2);

				ushort loopLength = (ushort)(sampleSizes[i] - loopStarts[i]);
				if ((loopLength == 0) || (loopStarts[i] == 0))
					loopLength = 0x0001;
				else
					loopLength /= 2;

				yield return new SampleInfo
				{
					Name = null,
					Length = length,
					LoopStart = loopStart,
					LoopLength = loopLength,
					Volume = volume,
					FineTune = 0
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
			ushort numberOfPositions = moduleStream.Read_B_UINT16();

			byte[] positionList = new byte[numberOfPositions];

			for (int i = 0; i < numberOfPositions; i++)
				positionList[i] = (byte)moduleStream.Read_B_UINT16();

			return positionList;
		}



		/********************************************************************/
		/// <summary>
		/// Return the restart position
		/// </summary>
		/********************************************************************/
		protected override byte GetRestartPosition(ModuleStream moduleStream)
		{
			return 0x00;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			moduleStream.Seek(200, SeekOrigin.Begin);

			byte[] pattern = new byte[1024];

			for (int i = 0; i < numberOfPatterns; i++)
			{
				for (int j = 0; j < 64 * 4; j++)
				{
					byte byt1 = moduleStream.Read_UINT8();
					byte byt2 = moduleStream.Read_UINT8();
					byte byt3 = moduleStream.Read_UINT8();
					byte byt4 = moduleStream.Read_UINT8();

					// Convert effect C arg back to hex value
					if ((byt3 & 0x0f) == 0x0c)
					{
						if (byt4 <= 9)
						{
						}
						else if ((byt4 >= 16) && (byt4 <= 25))
							byt4 -= 6;
						else if ((byt4 >= 32) && (byt4 <= 41))
							byt4 -= 12;
						else if ((byt4 >= 48) && (byt4 <= 57))
							byt4 -= 18;
						else if ((byt4 >= 64) && (byt4 <= 73))
							byt4 -= 24;
						else if ((byt4 >= 80) && (byt4 <= 89))
							byt4 -= 30;
						else if ((byt4 >= 96) && (byt4 <= 100))
							byt4 -= 36;
					}

					pattern[j * 4] = byt1;
					pattern[j * 4 + 1] = byt2;
					pattern[j * 4 + 2] = byt3;
					pattern[j * 4 + 3] = byt4;
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
			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion
	}
}
