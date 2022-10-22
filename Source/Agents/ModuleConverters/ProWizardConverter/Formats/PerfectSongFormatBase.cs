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
	/// Perfect Song Packer base class
	/// </summary>
	internal abstract class PerfectSongFormatBase : ProWizardConverterWorker31SamplesBase
	{
		protected uint textOffset;
		protected uint sampleStartOffset;

		private uint[] sampleOffsets;
		private byte[] positionList;

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the name of the module if any
		/// </summary>
		/********************************************************************/
		protected override byte[] GetModuleName(ModuleStream moduleStream)
		{
			byte[] moduleName = new byte[20];

			moduleStream.Seek(textOffset, SeekOrigin.Begin);
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
			sampleOffsets = new uint[31];

			for (int i = 0; i < 31; i++)
			{
				byte[] name = new byte[22];

				moduleStream.Seek(textOffset + 20 + i * 22, SeekOrigin.Begin);
				moduleStream.Read(name, 0, 22);

				moduleStream.Seek(12 + i * 16, SeekOrigin.Begin);

				sampleOffsets[i] = moduleStream.Read_B_UINT32() + sampleStartOffset;
				uint temp = moduleStream.Read_B_UINT32() + sampleStartOffset;

				ushort length = moduleStream.Read_B_UINT16();
				ushort loopLength = moduleStream.Read_B_UINT16();
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();
				ushort loopStart = (ushort)((temp - sampleOffsets[i]) / 2);

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
			return positionList;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			moduleStream.Seek(0x3fe, SeekOrigin.Begin);

			byte[] pattern = new byte[1024];

			for (int i = 0; i < numberOfPatterns; i++)
			{
				for (int j = 0; j < 64 * 4; j++)
				{
					byte note = moduleStream.Read_UINT8();
					byte sample = moduleStream.Read_UINT8();
					byte effect = moduleStream.Read_UINT8();
					byte effectVal = moduleStream.Read_UINT8();

					ConvertEffect(ref effect, ref effectVal);

					sample = (byte)(((note & 0x01) << 4) | (sample >> 4));

					pattern[j * 4] = (byte)(sample & 0xf0);
					pattern[j * 4 + 2] = (byte)((sample << 4) & 0xf0);
					pattern[j * 4 + 2] |= effect;
					pattern[j * 4 + 3] = effectVal;

					note /= 2;
					if (note != 0)
					{
						note--;
						pattern[j * 4] |= periods[note, 0];
						pattern[j * 4 + 1] = periods[note, 1];
					}
					else
						pattern[j * 4 + 1] = 0x00;
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
					moduleStream.Seek(sampleOffsets[i], SeekOrigin.Begin);

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

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Convert mapped effect back to ProTracker and change the effect
		/// value if needed
		/// </summary>
		/********************************************************************/
		protected abstract void ConvertEffect(ref byte effect, ref byte effectVal);
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Create position list and find the number of patterns needed
		/// </summary>
		/********************************************************************/
		protected void CreatePositionList(ModuleStream moduleStream)
		{
			moduleStream.Seek(0x1fc, SeekOrigin.Begin);

			ushort length = moduleStream.Read_B_UINT16();
			positionList = new byte[length];

			numberOfPatterns = 0;

			for (int i = 0; i < length; i++)
			{
				uint temp = moduleStream.Read_B_UINT32();
				positionList[i] = (byte)((temp - 0x3fe) / 1024);

				if (positionList[i] > numberOfPatterns)
					numberOfPatterns = positionList[i];
			}

			numberOfPatterns++;
		}
		#endregion
	}
}
