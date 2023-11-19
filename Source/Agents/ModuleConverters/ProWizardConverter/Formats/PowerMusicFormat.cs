/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// Power Music
	/// </summary>
	internal class PowerMusicFormat : ProWizardConverterWorker31SamplesBase
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
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT22;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 1084)
				return false;

			// Start to check the mark
			moduleStream.Seek(0x438, SeekOrigin.Begin);
			if (moduleStream.Read_B_UINT32() != 0x21504d21)		// !PM!
				return false;

			// Check size of position table
			moduleStream.Seek(0x3b6, SeekOrigin.Begin);

			byte positionListLength = moduleStream.Read_UINT8();
			if ((positionListLength == 0) || (positionListLength > 0x7f))
				return false;

			// Check NTK byte
			byte temp = moduleStream.Read_UINT8();
			if ((temp > 0x7f) && (temp != 0xff))
				return false;

			// Check first two bytes in position table
			if ((moduleStream.Read_UINT8() > 0x3f) || (moduleStream.Read_UINT8() > 0x3f))
				return false;

			// Check sample information
			moduleStream.Seek(20, SeekOrigin.Begin);

			uint samplesSize = 0;
			ushort temp1;

			for (int i = 0; i < 31; i++)
			{
				moduleStream.Seek(22, SeekOrigin.Current);

				// Check sample size
				temp1 = moduleStream.Read_B_UINT16();
				if (temp1 >= 0x8000)
					return false;

				samplesSize += temp1 * 2U;

				// Check volume and fine tune
				if ((moduleStream.Read_UINT8() > 0x0f) || (moduleStream.Read_UINT8() > 0x40))
					return false;

				if (temp != 0)
				{
					int temp2 = temp1 - moduleStream.Read_B_UINT16() + moduleStream.Read_B_UINT16();
					if (temp2 < -4)
						return false;
				}
				else
				{
					int temp2 = moduleStream.Read_B_UINT16() + moduleStream.Read_B_UINT16();
					if (temp2 > 2)
						return false;
				}
			}

			// Find highest pattern offset
			if (FindHighestPatternNumber(moduleStream, 0x3b8, positionListLength) > 64)
				return false;

			// Check module length
			if ((0x43c + numberOfPatterns * 1024 + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
				return false;

			// Check periods in the first pattern
			moduleStream.Seek(0x43c, SeekOrigin.Begin);

			ushort periodNum = 0;
			temp1 = (ushort)(numberOfPatterns * 256);
			if (temp1 > 0x600)
				temp1 = 0x600;

			for (int i = 0; i < temp1; i++)
			{
				// Get period
				ushort temp2 = (ushort)(moduleStream.Read_B_UINT16() & 0x0fff);

				if (temp2 != 0)
				{
					byte hi = (byte)((temp2 & 0xff00) >> 8);
					byte lo = (byte)(temp2 & 0x00ff);
					bool found = false;

					for (int j = 0; j < 36; j++)
					{
						// Try to find the period in the period table
						if ((periods[j, 0] == hi) && (periods[j, 1] == lo))
						{
							found = true;
							break;
						}
					}

					if (!found)
						return false;

					periodNum++;
				}

				moduleStream.Seek(2, SeekOrigin.Current);
			}

			if (periodNum == 0)
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
			moduleStream.Seek(0x3b6, SeekOrigin.Begin);

			numberOfPositions = moduleStream.Read_UINT8();
			restartPosition = moduleStream.Read_UINT8();

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
			moduleStream.Read(moduleName, 0, 20);

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
				moduleStream.Read(name, 0, 22);

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
			byte[] positionList = new byte[numberOfPositions];

			moduleStream.Seek(0x03b8, SeekOrigin.Begin);
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
			return restartPosition == 0xff ? (byte)0x7f : restartPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			moduleStream.Seek(0x43c, SeekOrigin.Begin);

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
			// Convert sample data
			int sampleSize = (int)sampleLengths.Sum(x => x);
			sbyte[] allSamples = new sbyte[sampleSize];
			moduleStream.ReadSigned(allSamples, 0, sampleSize);

			for (int i = 1; i < sampleSize; i++)
				allSamples[i] += allSamples[i - 1];

			converterStream.Write(MemoryMarshal.Cast<sbyte, byte>(allSamples));

			return true;
		}
		#endregion
	}
}
