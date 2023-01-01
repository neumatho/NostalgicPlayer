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
	/// STIM (SlamTilt)
	/// </summary>
	internal class SlamTiltFormat : ProWizardConverterWorker31SamplesBase
	{
		private uint sampleInfoOffset;
		private uint[] sampleStartOffsets;
		private uint[] patternStartOffsets;

		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT64;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 406)
				return false;

			// Start to check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);
			if (moduleStream.Read_B_UINT32() != 0x5354494d)		// STIM
				return false;

			// Check sample address
			uint infoOffset = moduleStream.Read_B_UINT32();
			if (infoOffset < 406)
				return false;

			// Check size of the position list
			moduleStream.Seek(18, SeekOrigin.Begin);

			ushort temp = moduleStream.Read_B_UINT16();
			if (temp > 128)
				return false;

			// Check number of patterns saved
			numberOfPatterns = moduleStream.Read_B_UINT16();
			if ((numberOfPatterns > 64) || (numberOfPatterns == 0))
				return false;

			// Check position list
			for (int i = 0; i < 128; i++)
			{
				if (moduleStream.Read_UINT8() > numberOfPatterns)
					return false;
			}

			// Check sample sizes
			moduleStream.Seek(infoOffset, SeekOrigin.Begin);

			uint[] sampleOffsets = new uint[31];
			moduleStream.ReadArray_B_UINT32s(sampleOffsets, 0, 31);

			uint samplesSize = 0;

			for (int i = 0; i < 31; i++)
			{
				moduleStream.Seek(infoOffset + sampleOffsets[i], SeekOrigin.Begin);
				samplesSize += moduleStream.Read_B_UINT16() * 2U;
			}

			if (samplesSize < 4)
				return false;

			// Check the module length
			if ((sampleInfoOffset + 31 * 4 + 31 * 8 + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
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
			moduleStream.Seek(4, SeekOrigin.Begin);

			sampleInfoOffset = moduleStream.Read_B_UINT32();

			sampleStartOffsets = new uint[31];
			moduleStream.Seek(sampleInfoOffset, SeekOrigin.Begin);
			moduleStream.ReadArray_B_UINT32s(sampleStartOffsets, 0, 31);

			patternStartOffsets = new uint[64];
			moduleStream.Seek(0x96, SeekOrigin.Begin);

			for (int i = 0; i < 64; i++)
				patternStartOffsets[i] = moduleStream.Read_B_UINT32() + 12;

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
				moduleStream.Seek(sampleInfoOffset + sampleStartOffsets[i], SeekOrigin.Begin);

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
			moduleStream.Seek(18, SeekOrigin.Begin);
			ushort numberOfPositions = moduleStream.Read_B_UINT16();

			moduleStream.Seek(2, SeekOrigin.Current);

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
			ushort[] trackOffsets = new ushort[4];

			byte[] pattern = new byte[1024];

			for (int i = 0; i < numberOfPatterns; i++)
			{
				uint offset = patternStartOffsets[i];

				moduleStream.Seek(offset, SeekOrigin.Begin);
				moduleStream.ReadArray_B_UINT16s(trackOffsets, 0, 4);

				// Clear pattern data
				Array.Clear(pattern);

				for (int j = 0; j < 4; j++)
				{
					moduleStream.Seek(offset + trackOffsets[j], SeekOrigin.Begin);

					for (int k = 0; k < 64; k++)
					{
						byte c1 = moduleStream.Read_UINT8();
						if ((c1 & 0x80) != 0)
						{
							k += (c1 & 0x7f);
							continue;
						}

						byte c2 = moduleStream.Read_UINT8();
						byte c3 = moduleStream.Read_UINT8();

						byte smp = (byte)(c1 & 0x1f);
						byte note = (byte)(c2 & 0x3f);
						byte fx = (byte)((c1 >> 5) & 0x03);

						byte c4 = (byte)((c2 >> 4) & 0x0c);
						fx |= c4;
						byte fxVal = c3;

						pattern[k * 16 + j * 4] = (byte)(smp & 0xf0);

						if (note != 0)
						{
							note--;

							pattern[k * 16 + j * 4] |= periods[note, 0];
							pattern[k * 16 + j * 4 + 1] = periods[note, 1];
						}

						pattern[k * 16 + j * 4 + 2] = (byte)((smp << 4) & 0xf0);
						pattern[k * 16 + j * 4 + 2] |= fx;
						pattern[k * 16 + j * 4 + 3] = fxVal;
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
			for (int i = 0; i < 31; i++)
			{
				int length = (int)sampleLengths[i];
				if (length != 0)
				{
					moduleStream.Seek(sampleInfoOffset + sampleStartOffsets[i] + 8, SeekOrigin.Begin);

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
	}
}
