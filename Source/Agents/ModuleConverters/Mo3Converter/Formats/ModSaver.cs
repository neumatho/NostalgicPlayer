/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Formats
{
	/// <summary>
	/// Will save a MOD file
	/// </summary>
	internal class ModSaver : IFormatSaver
	{
		private static readonly ushort[] periods =
		[
			6848, 6464, 6096, 5760, 5424, 5120, 4832, 4560, 4304, 4064, 3840, 3624,
			3424, 3232, 3048, 2880, 2712, 2560, 2416, 2280, 2152, 2032, 1920, 1812,
			1712, 1616, 1524, 1440, 1356, 1280, 1208, 1140, 1076, 1016,  960,  906,
			 856,  808,  762,  720,  678,  640,  604,  570,  538,  508,  480,  453,
			 428,  404,  381,  360,  339,  320,  302,  285,  269,  254,  240,  226,
			 214,  202,  190,  180,  170,  160,  151,  143,  135,  127,  120,  113,
			 107,  101,   95,   90,   85,   80,   75,   71,   67,   63,   60,   56,
			  53,   50,   47,   45,   42,   40,   37,   35,   33,   31,   30,   28
		];

		#region IFormatSaver implementation
		/********************************************************************/
		/// <summary>
		/// Save the module into MOD format
		/// </summary>
		/********************************************************************/
		public bool SaveModule(Mo3Module module, ModuleStream moduleStream, ConverterStream converterStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			DecodeSampleInfo[] decodeSampleInfo = Mo3SampleWriter.PrepareSamples(module, moduleStream);
			if (decodeSampleInfo == null)
			{
				errorMessage = Resources.IDS_ERR_LOADING_SAMPLES;
				return false;
			}

			ConvertTo8Bit(decodeSampleInfo);

			WriteSongName(module, converterStream);
			WriteSampleInfo(module, converterStream);
			WritePositionList(module, converterStream);
			WriteMark(module, converterStream);
			WritePatterns(module, converterStream);
			Mo3SampleWriter.WriteSamples(decodeSampleInfo, converterStream);

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Make sure that all samples are in 8-bit
		/// </summary>
		/********************************************************************/
		private void ConvertTo8Bit(DecodeSampleInfo[] decodeSampleInfo)
		{
			foreach (DecodeSampleInfo sampleInfo in decodeSampleInfo)
			{
				if ((sampleInfo.SampleData != null) && sampleInfo.SampleHeader.Flags.HasFlag(SampleInfoFlag._16Bit))
				{
					Span<short> source = MemoryMarshal.Cast<byte, short>(sampleInfo.SampleData);
					byte[] dest = new byte[source.Length];

					for (int i = 0; i < source.Length; i++)
						dest[i] = (byte)(source[i] >> 8);

					sampleInfo.SampleData = dest;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Write back the song name
		/// </summary>
		/********************************************************************/
		private void WriteSongName(Mo3Module module, ConverterStream converterStream)
		{
			converterStream.WriteString(module.Header.SongName, 20);
		}



		/********************************************************************/
		/// <summary>
		/// Write all the sample information
		/// </summary>
		/********************************************************************/
		private void WriteSampleInfo(Mo3Module module, ConverterStream converterStream)
		{
			int samplesToTake = Math.Min((int)module.Header.NumSamples, 31);

			for (int i = 0; i < samplesToTake; i++)
			{
				Sample sample = module.Samples[i];

				converterStream.WriteString(sample.SampleName, 22);
				converterStream.Write_B_UINT16((ushort)(sample.Length / 2));
				converterStream.Write_UINT8((byte)(((sample.FreqFineTune - 128) >> 4) & 0x0f));
				converterStream.Write_UINT8(sample.DefaultVolume);

				if ((sample.Flags & SampleInfoFlag.Loop) != 0)
				{
					converterStream.Write_B_UINT16((ushort)(sample.LoopStart / 2));
					converterStream.Write_B_UINT16((ushort)((sample.LoopEnd - sample.LoopStart) / 2));
				}
				else
				{
					converterStream.Write_B_UINT16(0);
					converterStream.Write_B_UINT16(1);
				}
			}

			if (samplesToTake < 31)
			{
				byte[] emptySample = Enumerable.Repeat<byte>(0, 30).ToArray();

				for (int i = samplesToTake; i < 31; i++)
					converterStream.Write(emptySample, 0, emptySample.Length);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Write the position list and its information
		/// </summary>
		/********************************************************************/
		private void WritePositionList(Mo3Module module, ConverterStream converterStream)
		{
			converterStream.Write_UINT8((byte)module.Header.NumOrders);

			if (module.Header.RestartPos != 0)
				converterStream.Write_UINT8((byte)module.Header.RestartPos);
			else
				converterStream.Write_UINT8(0x7f);

			converterStream.Write(module.PatternInfo.PositionList, 0, module.Header.NumOrders);

			if (module.Header.NumOrders < 128)
				converterStream.Write(Enumerable.Repeat<byte>(0, 128 - module.Header.NumOrders).ToArray());
		}



		/********************************************************************/
		/// <summary>
		/// Find the right mark to write
		/// </summary>
		/********************************************************************/
		private void WriteMark(Mo3Module module, ConverterStream converterStream)
		{
			if (module.Header.NumChannels == 4)
				converterStream.WriteMark("M.K.");
			else
			{
				if (module.Header.NumChannels < 10)
				{
					char chn = (char)(module.Header.NumChannels + 0x30);
					converterStream.WriteMark(chn + "CHN");
				}
				else
				{
					string chn = string.Format("{0}{1}", (char)((module.Header.NumChannels / 10) + 0x30), (char)((module.Header.NumChannels % 10) + 0x30));
					converterStream.WriteMark(chn + "CH");
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will recreate all the patterns based on the tracks
		/// </summary>
		/********************************************************************/
		private void WritePatterns(Mo3Module module, ConverterStream converterStream)
		{
			byte[] singlePatternData = new byte[4 * module.Header.NumChannels * 64];

			for (int i = 0; i < module.Header.NumPatterns; i++)
			{
				for (int j = 0; j < module.Header.NumChannels; j++)
				{
					Track trk = module.Tracks[module.PatternInfo.Sequences[i, j]];
					ConvertTrack(singlePatternData, j * 4, 4 * module.Header.NumChannels, trk);
				}

				converterStream.Write(singlePatternData, 0, singlePatternData.Length);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will convert the given track into pattern data
		/// </summary>
		/********************************************************************/
		private void ConvertTrack(byte[] patternData, int offset, int skip, Track trk)
		{
			int rows = Math.Min(trk.Rows.Count, 64);

			for (int i = 0; i < rows; i++)
			{
				TrackRow trkRow = trk.Rows[i];

				patternData[offset] = (byte)(trkRow.Instrument & 0x10);
				patternData[offset + 1] = 0x00;
				patternData[offset + 2] = (byte)((trkRow.Instrument & 0x0f) << 4);
				patternData[offset + 3] = 0x00;

				if (trkRow.Note != 0)
				{
					ushort period = periods[trkRow.Note - 1];
					patternData[offset] |= (byte)((period & 0x0f00) >> 8);
					patternData[offset + 1] = (byte)(period & 0x00ff);
				}

				if (trkRow.Effects != null)
				{
					(Effect, byte) eff = trkRow.Effects[0];

					byte effNum = (byte)(((byte)eff.Item1) - 3);
					byte effVal = eff.Item2;

					patternData[offset + 2] |= effNum;
					patternData[offset + 3] = effVal;
				}

				offset += skip;
			}

			for (int i = rows; i < 64; i++)
			{
				patternData[offset] = 0x00;
				patternData[offset + 1] = 0x00;
				patternData[offset + 2] = 0x00;
				patternData[offset + 3] = 0x00;

				offset += skip;
			}
		}
		#endregion
	}
}
