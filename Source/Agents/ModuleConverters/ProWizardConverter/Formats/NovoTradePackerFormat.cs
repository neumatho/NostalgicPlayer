/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// NovoTrade Packer
	/// </summary>
	internal class NovoTradePackerFormat : ProWizardConverterWorker31SamplesBase
	{
		private ushort bodyOffset;
		private ushort samplesOffset;

		private ushort numberOfSamples;
		private byte numberOfPositions;

		private ushort[] patternOffsets;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT61;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 38)
				return false;

			// Start to check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);
			if (moduleStream.ReadMark() != "MODU")
				return false;

			// Check different offsets
			moduleStream.Seek(20, SeekOrigin.Begin);

			ushort offset1 = (ushort)(moduleStream.Read_B_UINT16() + 4);
			if ((offset1 + 4) > moduleStream.Length)
				return false;

			moduleStream.Seek(26, SeekOrigin.Begin);

			numberOfPatterns = moduleStream.Read_B_UINT16();

			ushort offset2 = (ushort)(moduleStream.Read_B_UINT16() + offset1 + 4);
			if ((offset2 + 2) > moduleStream.Length)
				return false;

			// Lets verify the offsets
			moduleStream.Seek(offset1, SeekOrigin.Begin);

			if (moduleStream.ReadMark() != "BODY")
				return false;

			moduleStream.Seek(offset2, SeekOrigin.Begin);

			if (moduleStream.ReadMark() != "SAMP")
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
			moduleStream.Seek(20, SeekOrigin.Begin);

			bodyOffset = (ushort)(moduleStream.Read_B_UINT16() + 4);
			numberOfSamples = moduleStream.Read_B_UINT16();
			numberOfPositions = (byte)moduleStream.Read_B_UINT16();
			moduleStream.Seek(2, SeekOrigin.Current);
			samplesOffset = (ushort)(moduleStream.Read_B_UINT16() + bodyOffset + 4);

			patternOffsets = new ushort[numberOfPatterns];

			moduleStream.Seek(30 + numberOfSamples * 8 + numberOfPositions * 2, SeekOrigin.Begin);
			moduleStream.ReadArray_B_UINT16s(patternOffsets, 0, numberOfPatterns);

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

			moduleStream.Seek(4, SeekOrigin.Begin);
			moduleStream.ReadInto(moduleName, 0, 16);

			return moduleName;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			moduleStream.Seek(30, SeekOrigin.Begin);

			SampleInfo[] samples = new SampleInfo[31];

			for (int i = 0; i < numberOfSamples; i++)
			{
				byte sampleNumber = moduleStream.Read_UINT8();

				byte volume = moduleStream.Read_UINT8();
				ushort length = moduleStream.Read_B_UINT16();
				ushort loopStart = moduleStream.Read_B_UINT16();
				ushort loopLength = moduleStream.Read_B_UINT16();

				samples[sampleNumber] = new SampleInfo
				{
					Name = null,
					Length = length,
					LoopStart = loopStart,
					LoopLength = loopLength,
					Volume = volume,
					FineTune = 0
				};
			}

			foreach (SampleInfo info in samples)
			{
				if (info == null)
				{
					yield return new SampleInfo
					{
						LoopLength = 0x0001
					};
				}
				else
					yield return info;
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

			for (int i = 0; i < numberOfPositions; i++)
				positionList[i] = (byte)moduleStream.Read_B_UINT16();

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
				// Clear the pattern data
				Array.Clear(pattern);

				moduleStream.Seek(bodyOffset + 4 + patternOffsets[i], SeekOrigin.Begin);

				for (int j = 0; j < 64; j++)
				{
					ushort temp = moduleStream.Read_B_UINT16();

					if (temp == 0x80)
						break;	// End of pattern

					if (temp == 0x00)
						continue;

					if (temp > 0x0f)
						continue;

					if ((temp & 0x01) != 0)
					{
						pattern[j * 16] = moduleStream.Read_UINT8();
						pattern[j * 16 + 1] = moduleStream.Read_UINT8();
						pattern[j * 16 + 2] = moduleStream.Read_UINT8();
						pattern[j * 16 + 3] = moduleStream.Read_UINT8();
					}

					if ((temp & 0x02) != 0)
					{
						pattern[j * 16 + 4] = moduleStream.Read_UINT8();
						pattern[j * 16 + 5] = moduleStream.Read_UINT8();
						pattern[j * 16 + 6] = moduleStream.Read_UINT8();
						pattern[j * 16 + 7] = moduleStream.Read_UINT8();
					}

					if ((temp & 0x04) != 0)
					{
						pattern[j * 16 + 8] = moduleStream.Read_UINT8();
						pattern[j * 16 + 9] = moduleStream.Read_UINT8();
						pattern[j * 16 + 10] = moduleStream.Read_UINT8();
						pattern[j * 16 + 11] = moduleStream.Read_UINT8();
					}

					if ((temp & 0x08) != 0)
					{
						pattern[j * 16 + 12] = moduleStream.Read_UINT8();
						pattern[j * 16 + 13] = moduleStream.Read_UINT8();
						pattern[j * 16 + 14] = moduleStream.Read_UINT8();
						pattern[j * 16 + 15] = moduleStream.Read_UINT8();
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
		protected override bool WriteSampleData(PlayerFileInfo fileInfo, ModuleStream moduleStream, ConverterStream converterStream)
		{
			moduleStream.Seek(samplesOffset + 4, SeekOrigin.Begin);

			return SaveMarksForAllSamples(moduleStream, converterStream);
		}
		#endregion
	}
}
