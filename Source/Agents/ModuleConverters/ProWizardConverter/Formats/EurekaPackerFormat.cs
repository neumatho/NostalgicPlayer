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
	/// Eureka Packer
	/// </summary>
	internal class EurekaPackerFormat : ProWizardConverterWorker31SamplesBase
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
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT7;
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

			// Check position length
			moduleStream.Seek(950, SeekOrigin.Begin);

			byte positionListLength = moduleStream.Read_UINT8();
			if ((positionListLength > 0x7f) || (positionListLength == 0x00))
				return false;

			// Check NTK byte
			byte temp = moduleStream.Read_UINT8();
			if (temp > 0x7f)
				return false;

			// Check first two patterns in the position table
			if ((moduleStream.Read_UINT8() > 0x3f) || (moduleStream.Read_UINT8() > 0x3f))
				return false;

			// No mark is allowed
			moduleStream.Seek(1080, SeekOrigin.Begin);

			uint sampleOffset = moduleStream.Read_B_UINT32();
			if ((sampleOffset == 0x4d2e4b2e) || (sampleOffset == 0x534e542e))	// M.K. and SNT.
				return false;

			// Check sample information
			moduleStream.Seek(20, SeekOrigin.Begin);

			ushort temp1;
			uint samplesSize = 0;

			for (int i = 0; i < 31; i++)
			{
				moduleStream.Seek(22, SeekOrigin.Current);

				// Get sample size
				temp1 = moduleStream.Read_B_UINT16();
				if (temp1 >= 0x8000)
					return false;

				samplesSize += temp1 * 2U;

				// Check volume and fine tune
				if ((moduleStream.Read_UINT8() > 0x0f) || (moduleStream.Read_UINT8() > 0x40))
					return false;

				// Check loop and loop length
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
			if ((sampleOffset + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
				return false;

			// Check number of patterns
			if (FindHighestPatternNumber(moduleStream, 952, positionListLength) > 64)
				return false;

			// Check the pattern offsets to see if they are in ascending order
			moduleStream.Seek(1084, SeekOrigin.Begin);

			temp1 = 0;

			for (int i = 0; i < numberOfPatterns * 4; i++)
			{
				ushort temp2 = moduleStream.Read_B_UINT16();
				if (temp2 < temp1)
					return false;

				temp1 = temp2;
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
			moduleStream.Seek(950, SeekOrigin.Begin);

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
			ushort[] patternOffsets = new ushort[numberOfPatterns * 4];

			moduleStream.Seek(1084, SeekOrigin.Begin);
			moduleStream.ReadArray_B_UINT16s(patternOffsets, 0, numberOfPatterns * 4);

			byte[] pattern = new byte[1024];

			for (int i = 0; i < numberOfPatterns; i++)
			{
				// Clear pattern data
				Array.Clear(pattern);

				// Loop each channel
				for (int j = 0; j < 4; j++)
				{
					// Seek to the track data
					moduleStream.Seek(patternOffsets[i * 4 + j], SeekOrigin.Begin);

					// Initialize the write offset
					int writeOffset = j * 4;

					// Loop each line
					for (int k = 0; k < 64; k++)
					{
						byte byt = moduleStream.Read_UINT8();
						byte tempByte = (byte)(byt & 0xf0);

						if ((tempByte == 0x00) || (tempByte == 0x10))
						{
							// Entire note
							pattern[writeOffset] = byt;
							pattern[writeOffset + 1] = moduleStream.Read_UINT8();
							pattern[writeOffset + 2] = moduleStream.Read_UINT8();
							pattern[writeOffset + 3] = moduleStream.Read_UINT8();

							writeOffset += 16;
							continue;
						}

						if (tempByte == 0x40)
						{
							// Only effect
							pattern[writeOffset + 2] = (byte)(byt & 0x0f);
							pattern[writeOffset + 3] = moduleStream.Read_UINT8();

							writeOffset += 16;
							continue;
						}

						if (tempByte == 0x80)
						{
							// Only note
							pattern[writeOffset] = moduleStream.Read_UINT8();
							pattern[writeOffset + 1] = moduleStream.Read_UINT8();
							pattern[writeOffset + 2] = (byte)((byt & 0x0f) << 4);

							writeOffset += 16;
							continue;
						}

						// Empty lines
						tempByte = (byte)(byt - 0xc0);
						writeOffset += ((tempByte + 1) * 16);
						k += tempByte;
					}
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
			moduleStream.Seek(1080, SeekOrigin.Begin);
			uint sampleDataOffset = moduleStream.Read_B_UINT32();
			moduleStream.Seek(sampleDataOffset, SeekOrigin.Begin);

			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion
	}
}
