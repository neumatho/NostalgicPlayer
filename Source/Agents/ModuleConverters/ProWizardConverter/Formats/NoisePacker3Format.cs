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
	/// NoisePacker 3
	/// </summary>
	internal class NoisePacker3Format : NoisePackerFormatBase
	{
		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT17;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			return CheckForNoisePackerFormat(moduleStream) == 3;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			moduleStream.Seek(8, SeekOrigin.Begin);

			for (int i = 0; i < numberOfSamples; i++)
			{
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();
				moduleStream.Seek(4, SeekOrigin.Current);
				ushort length = moduleStream.Read_B_UINT16();
				moduleStream.Seek(4, SeekOrigin.Current);
				ushort loopLength = moduleStream.Read_B_UINT16();
				ushort loopStart = moduleStream.Read_B_UINT16();

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
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			byte[] pattern = new byte[1024];

			int trackOffsetTableIndex = 0;

			for (int i = 0; i < numberOfPatterns; i++)
			{
				// Clear the pattern data
				Array.Clear(pattern);

				// Loop voices
				for (int j = 0; j < 4; j++)
				{
					// Find offset to the track to copy
					int trackOffset = trackOffsetTable[trackOffsetTableIndex++];
					moduleStream.Seek(trackDataOffset + trackOffset, SeekOrigin.Begin);

					// Loop rows
					for (int k = 0; k < 64; k++)
					{
						// Get pattern data
						byte dat1 = moduleStream.Read_UINT8();

						if (dat1 >= 0xc1)
						{
							// Empty lines
							k += unchecked((-(sbyte)dat1) - 1);
							continue;
						}

						if (dat1 == 0xc0)
							break;
						
						byte dat2 = moduleStream.Read_UINT8();
						byte dat3 = moduleStream.Read_UINT8();

						// Convert the pattern data
						ValueTuple<byte, byte> pair1 = BuildNoteAndSamplePair(dat1);
						ValueTuple<byte, byte> pair2 = BuildEffect(dat2, dat3, 4);

						// Store the converted values
						pattern[k * 16 + (3 - j) * 4] = pair1.Item1;
						pattern[k * 16 + (3 - j) * 4 + 1] = pair1.Item2;
						pattern[k * 16 + (3 - j) * 4 + 2] = pair2.Item1;
						pattern[k * 16 + (3 - j) * 4 + 3] = pair2.Item2;

						// Pattern stop?
						int temp = pair2.Item1 & 0x0f;
						if ((temp == 0x0b) || (temp == 0x0d))
							break;
					}
				}

				yield return pattern;
			}
		}
		#endregion

		#region ChannelPlayerFormatBase implementation
		/********************************************************************/
		/// <summary>
		/// Check the sample information and return the total size of all
		/// the samples
		/// </summary>
		/********************************************************************/
		protected override uint CheckSampleInfo(ModuleStream moduleStream, ushort sampleCount, out int formatVersion)
		{
			formatVersion = 3;
			uint samplesSize = 0;

			moduleStream.Seek(8, SeekOrigin.Begin);

			for (int i = 0; i < sampleCount; i++)
			{
				// Check volume and fine tune
				if ((moduleStream.Read_UINT8() > 0x0f) || (moduleStream.Read_UINT8() > 0x40))
					return 0;

				// Check pointer to sample
				uint temp1 = moduleStream.Read_B_UINT32();
				if (temp1 > 0x1fe000)
					return 0;

				// Check sample size
				ushort temp = moduleStream.Read_B_UINT16();
				if (temp > 0x8000)
					return 0;

				samplesSize += temp * 2U;

				// Check loop pointer
				temp1 = moduleStream.Read_B_UINT32();
				if (temp1 > 0x1fe000)
					return 0;

				// Check loop
				ushort temp2 = moduleStream.Read_B_UINT16();
				ushort temp3 = moduleStream.Read_B_UINT16();

				temp1 = (uint)temp2 + temp3;
				if (temp1 > temp)
					return 0;
			}

			return samplesSize;
		}



		/********************************************************************/
		/// <summary>
		/// Check the pattern data
		/// </summary>
		/********************************************************************/
		protected override bool CheckPatternData(ModuleStream moduleStream, ushort sampleCount, ushort trackLength, ref int formatVersion)
		{
			for (int i = 0; i < trackLength; i++)
			{
				byte temp1 = moduleStream.Read_UINT8();
				if ((temp1 & 0x80) == 0x80)
					continue;

				// Check note
				if (temp1 > 0x49)
					return false;

				byte temp2 = moduleStream.Read_UINT8();
				byte temp3 = moduleStream.Read_UINT8();

				// Check different invalid cases on effects
				if ((temp2 & 0x0f) == 0x0a)
					return false;

				if (((temp2 & 0x0f) == 0x0d) && (temp3 > 0x64))
					return false;

				i += 2;
			}

			return true;
		}
		#endregion
	}
}
