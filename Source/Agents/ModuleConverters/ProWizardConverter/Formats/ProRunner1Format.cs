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
	/// ProRunner 1
	/// </summary>
	internal class ProRunner1Format : ProWizardConverterWorker31SamplesBase
	{
		private byte numberOfPositions;
		private byte restartPosition;
		private ushort realNumberOfPatterns;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT31;
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

			// Check sample information
			moduleStream.Seek(20, SeekOrigin.Begin);

			uint samplesSize = 0;

			for (int i = 0; i < 31; i++)
			{
				moduleStream.Seek(22, SeekOrigin.Current);

				// Check sample size
				ushort temp = moduleStream.Read_B_UINT16();
				if (temp > 0x8000)
					return false;

				samplesSize += temp * 2U;

				// Check volume and fine tune
				if ((moduleStream.Read_UINT8() > 0x0f) || (moduleStream.Read_UINT8() > 0x40))
					return false;

				// Check loop values
				if (temp != 0)
				{
					uint temp1 = moduleStream.Read_B_UINT16();
					uint temp2 = moduleStream.Read_B_UINT16();

					if ((temp1 + temp2) > temp)
						return false;
				}
				else
					moduleStream.Seek(4, SeekOrigin.Current);
			}

			// Check position table and find highest pattern number
			moduleStream.Seek(2, SeekOrigin.Current);

			numberOfPatterns = 0;
			for (int i = 0; i < 128; i++)
			{
				byte temp = moduleStream.Read_UINT8();
				if (temp > 0x3f)
					return false;

				if (temp > numberOfPatterns)
					numberOfPatterns = temp;
			}

			numberOfPatterns++;
			uint biggestPattern = numberOfPatterns * 256U;

			// Check the module length
			if ((0x43c + biggestPattern * 4 + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
				return false;

			// Check the pattern data
			moduleStream.Seek(0x43c, SeekOrigin.Begin);

			bool gotNotes = false;
			for (int i = 0; i < biggestPattern; i++)
			{
				uint temp1 = moduleStream.Read_B_UINT32();
				if (temp1 != 0)
				{
					// The unused nibble has to be zero
					if ((temp1 & 0x0000f000) != 0)
						return false;

					// Check note
					ushort temp = (ushort)((temp1 & 0x00ff0000) >> 16);
					if (temp > 0x24)
						return false;

					// Check sample number
					temp = (ushort)((temp1 & 0xff000000) >> 24);
					if (temp > 0x1f)
						return false;

					gotNotes = true;
				}
			}

			// Did we get any notes?
			if (!gotNotes)
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
			moduleStream.Seek(950, SeekOrigin.Begin);

			numberOfPositions = moduleStream.Read_UINT8();
			restartPosition = moduleStream.Read_UINT8();

			// We don't want to save unused patterns
			realNumberOfPatterns = numberOfPatterns;
			FindHighestPatternNumber(moduleStream, 0x3b8, numberOfPositions);

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

			moduleStream.Seek(952, SeekOrigin.Begin);
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
			return GetAllPatterns(moduleStream, 1);
		}



		/********************************************************************/
		/// <summary>
		/// Write all the samples
		/// </summary>
		/********************************************************************/
		protected override bool WriteSampleData(ModuleStream moduleStream, ConverterStream converterStream)
		{
			moduleStream.Seek(0x43c + realNumberOfPatterns * 1024, SeekOrigin.Begin);

			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected IEnumerable<byte[]> GetAllPatterns(ModuleStream moduleStream, byte divisor)
		{
			moduleStream.Seek(0x43c, SeekOrigin.Begin);

			byte[] pattern = new byte[1024];

			for (int i = 0; i < numberOfPatterns; i++)
			{
				// Clear pattern data
				Array.Clear(pattern);

				// Loop each channel
				for (int j = 0; j < 64 * 4; j++)
				{
					byte temp = moduleStream.Read_UINT8();
					temp /= divisor;

					if (temp >= 0x10)
					{
						pattern[j * 4] = 0x10;		// Hi bit in sample number
						temp -= 0x10;
					}

					// Store sample number
					pattern[j * 4 + 2] = (byte)(temp << 4);

					// Convert note
					temp = moduleStream.Read_UINT8();
					temp /= divisor;

					if (temp != 0)
					{
						temp--;
						pattern[j * 4] |= periods[temp, 0];
						pattern[j * 4 + 1] = periods[temp, 1];
					}

					// Copy effect
					pattern[j * 4 + 2] |= moduleStream.Read_UINT8();
					pattern[j * 4 + 3] = moduleStream.Read_UINT8();
				}

				yield return pattern;
			}
		}
		#endregion
	}
}
