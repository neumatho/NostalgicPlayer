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
	/// Titanics Player
	/// </summary>
	internal class TitanicsPlayerFormat : ProWizardConverterWorker15SamplesBase
	{
		private byte numberOfPositions;
		private ushort[] patternOffsets;
		private byte[] positionList;

		private uint[] sampleAddresses;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT68;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 180)
				return false;

			// Check sample information
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint samplesSize = 0;

			for (int i = 0; i < 15; i++)
			{
				uint temp = moduleStream.Read_B_UINT32();
				if ((temp > moduleStream.Length) || ((temp < 180) && (temp != 0)))
					return false;

				ushort length = moduleStream.Read_B_UINT16();
				ushort volume = moduleStream.Read_B_UINT16();
				ushort loopStart = moduleStream.Read_B_UINT16();
				ushort loopLength = moduleStream.Read_B_UINT16();

				if (volume > 0x40)
					return false;

				if ((loopStart > length) || (loopLength > (length + 1)) || (length > 32768))
					return false;

				if (loopLength == 0)
					return false;

				if ((length == 0) && ((loopStart != 0) || (loopLength != 1)))
					return false;

				if ((length != 0) && (length == loopStart))
					return false;

				samplesSize += length;
			}

			if (samplesSize < 2)
				return false;

			// Test pattern addresses
			for (int i = 0; i < 256; i += 2)
			{
				if (moduleStream.EndOfStream)
					return false;

				ushort temp = moduleStream.Read_B_UINT16();
				if (temp == 0xffff)
					return true;

				if (temp < 180)
					return false;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Prepare conversion by initialize what is needed etc.
		/// </summary>
		/********************************************************************/
		protected override bool PrepareConversion(ModuleStream moduleStream)
		{
			CreatePositionList(moduleStream);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			sampleAddresses = new uint[15];

			moduleStream.Seek(0, SeekOrigin.Begin);

			for (int i = 0; i < 15; i++)
			{
				sampleAddresses[i] = moduleStream.Read_B_UINT32();

				ushort length = moduleStream.Read_B_UINT16();
				ushort volume = moduleStream.Read_B_UINT16();
				ushort loopStart = moduleStream.Read_B_UINT16();
				ushort loopLength = moduleStream.Read_B_UINT16();

				yield return new SampleInfo
				{
					Name = null,
					Length = length,
					LoopStart = loopStart,
					LoopLength = loopLength,
					Volume = (byte)volume,
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
			return this.positionList;
		}



		/********************************************************************/
		/// <summary>
		/// Return the restart position
		/// </summary>
		/********************************************************************/
		protected override byte GetRestartPosition(ModuleStream moduleStream)
		{
			return 0x78;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			byte[] pattern = new byte[1024];

			sbyte lastPattern = -1;
			for (int i = 0; i < numberOfPositions; i++)
			{
				// Get pattern to build
				if (positionList[i] > lastPattern)
				{
					lastPattern++;

					// Clear pattern
					Array.Clear(pattern);

					// Get pattern offset
					moduleStream.Seek(patternOffsets[i], SeekOrigin.Begin);

					// Convert the pattern
					byte c1;
					byte nextC1 = moduleStream.Read_UINT8();
					int row = 0;

					do
					{
						c1 = nextC1;
						byte c2 = moduleStream.Read_UINT8();
						byte c3 = moduleStream.Read_UINT8();
						byte c4 = moduleStream.Read_UINT8();

						byte voice = (byte)(((c2 >> 6) & 0x03) * 4);
						byte note = (byte)(c2 & 0x3f);

						if ((note > 0) && (note <= 36))
						{
							pattern[row * 16 + voice] = periods[note, 0];
							pattern[row * 16 + voice + 1] = periods[note, 1];
						}

						pattern[row * 16 + voice + 2] = c3;
						pattern[row * 16 + voice + 3] = c4;

						nextC1 = moduleStream.Read_UINT8();

						if ((nextC1 & 0x7f) != 0)
							row += nextC1 & 0x7f;
					}
					while ((row < 64) && ((c1 & 0x80) == 0));

					yield return pattern;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Write all the samples
		/// </summary>
		/********************************************************************/
		protected override bool WriteSampleData(ModuleStream moduleStream, ConverterStream converterStream)
		{
			for (int i = 0; i < 15; i++)
			{
				int length = (int)sampleLengths[i];
				if (length != 0)
				{
					moduleStream.Seek(sampleAddresses[i], SeekOrigin.Begin);

					// Check to see if we miss too much from the last sample
					if (moduleStream.Length - moduleStream.Position < (length - MaxNumberOfMissingBytes))
						return false;

					moduleStream.SetSampleDataInfo(i, length);
					converterStream.WriteSampleDataMarker(i, length);
				}
			}

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Create position list and find the number of patterns needed
		/// </summary>
		/********************************************************************/
		private void CreatePositionList(ModuleStream moduleStream)
		{
			moduleStream.Seek(180, SeekOrigin.Begin);

			patternOffsets = new ushort[128];

			for (numberOfPositions = 0; numberOfPositions < 128; numberOfPositions++)
			{
				ushort offset = moduleStream.Read_B_UINT16();
				if (offset == 0xffff)
					break;

				patternOffsets[numberOfPositions] = offset;
			}

			positionList = BuildPositionList(numberOfPositions, (pos) => patternOffsets[pos]);
		}
		#endregion
	}
}
