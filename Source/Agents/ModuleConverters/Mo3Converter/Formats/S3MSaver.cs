/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers.Chunks;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Formats
{
	/// <summary>
	/// Will save a S3M file
	/// </summary>
	internal class S3MSaver : IFormatSaver
	{
		#region IFormatSaver implementation
		/********************************************************************/
		/// <summary>
		/// Save the module into S3M format
		/// </summary>
		/********************************************************************/
		public bool SaveModule(Mo3Module module, ModuleStream moduleStream, ConverterStream converterStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			DecodeSampleInfo[] decodeSampleInfo = Mo3SampleWriter.PrepareSamples(module, moduleStream, true);
			if (decodeSampleInfo == null)
			{
				errorMessage = Resources.IDS_ERR_LOADING_SAMPLES;
				return false;
			}

			WriteSongName(module, converterStream);
			WriteHeader(module, converterStream);
			WritePositionList(module, converterStream);
			long paraPointersPosition = ReserveSpaceForParaPointers(module, converterStream);
			WriteChannelPannings(module, converterStream);
			AlignModule(converterStream);

			List<ushort> sampleInfoParaPointers = WriteSampleInfo(module, converterStream);
			List<ushort> patternParaPointers = WritePatterns(module, converterStream);
			List<uint> sampleDataParaPointers = WriteSampleData(converterStream, sampleInfoParaPointers, decodeSampleInfo);

			WriteParaPointers(converterStream, paraPointersPosition, sampleInfoParaPointers, patternParaPointers, sampleDataParaPointers);

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Write back the song name
		/// </summary>
		/********************************************************************/
		private void WriteSongName(Mo3Module module, ConverterStream converterStream)
		{
			converterStream.WriteString(module.Header.SongName, 28);
		}



		/********************************************************************/
		/// <summary>
		/// Write back the header
		/// </summary>
		/********************************************************************/
		private void WriteHeader(Mo3Module module, ConverterStream converterStream)
		{
			FileHeader header = module.Header;

			// Some standard marks
			converterStream.Write_UINT8(0x1a);
			converterStream.Write_UINT8(0x10);
			converterStream.Write_UINT8(0x00);
			converterStream.Write_UINT8(0x00);

			// Order num should be even, so make sure it is
			converterStream.Write_L_UINT16((ushort)((header.NumOrders % 2) != 0 ? header.NumOrders + 1 : header.NumOrders));
			converterStream.Write_L_UINT16(header.NumSamples);
			converterStream.Write_L_UINT16(header.NumPatterns);

			ushort flags = 0x0000;

			if ((header.Flags & HeaderFlag.S3MFastSlides) != 0)
				flags |= 0x0004;

			if ((header.Flags & HeaderFlag.S3MAmigaLimits) != 0)
				flags |= 0x0010;

			converterStream.Write_L_UINT16(flags);

			VersChunk versChunk = module.FindChunk<VersChunk>();
			if (versChunk != null)
				converterStream.Write_L_UINT16(versChunk.Cwtv);
			else
				converterStream.Write_L_UINT16(0x1320);

			converterStream.Write_L_UINT16(2);		// File format = standard (samples unsigned)

			converterStream.Write_B_UINT32(0x5343524d);		// SCRM

			converterStream.Write_UINT8(header.GlobalVol);
			converterStream.Write_UINT8(header.DefaultSpeed);
			converterStream.Write_UINT8(header.DefaultTempo);
			converterStream.Write_UINT8(0xb0);				// Master volume default value (0x30 in volume, 0x80 for stereo)
			converterStream.Write_UINT8(0);					// Ultra click removal
			converterStream.Write_UINT8(0xfc);				// Use default panning

			converterStream.Write(Enumerable.Repeat<byte>(0, 8).ToArray());	// Padding
			converterStream.Write_L_UINT16(0);				// Special custom data pointer (not used)

			if (module.Version >= 5)
			{
				// From version 5, the S3M channel settings array are stored in the
				// last 32 bytes of MO3s channel panning table, so just copy from there
				converterStream.Write(header.ChnPan, 32, header.NumChannels);
			}
			else
			{
				// Earlier versions of MO3 does not save the channel settings, so just
				// write some default values
				byte val = 0x01;

				for (int i = 0; i < header.NumChannels; i++)
				{
					if ((i % 2) == 0)
						converterStream.Write_UINT8(val);
					else
					{
						 converterStream.Write_UINT8((byte)(val + 8));
						 val++;

						 if (val == 0x08)
							 val = 0x01;
					}
				}
			}

			if (header.NumChannels < 32)
				converterStream.Write(Enumerable.Repeat<byte>(0xff, 32 - header.NumChannels).ToArray());	// Disable the rest of the channels
		}



		/********************************************************************/
		/// <summary>
		/// Write the position list and its information
		/// </summary>
		/********************************************************************/
		private void WritePositionList(Mo3Module module, ConverterStream converterStream)
		{
			converterStream.Write(module.PatternInfo.PositionList, 0, module.Header.NumOrders);

			if ((module.Header.NumOrders % 2) != 0)
			{
				// Write end mark
				converterStream.Write_UINT8(0xff);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Reserve space in the S3M file for para pointers
		/// </summary>
		/********************************************************************/
		private long ReserveSpaceForParaPointers(Mo3Module module, ConverterStream converterStream)
		{
			// At this point, we don't know the values of the para pointers, so we just
			// reserve the space in the converted file for them. They will be written later
			int spaceToReserve = module.Header.NumSamples + module.Header.NumPatterns;

			long currentPosition = converterStream.Position;
			converterStream.WriteArray_L_UINT16s(Enumerable.Repeat<ushort>(0, spaceToReserve).ToArray(), spaceToReserve);

			return currentPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Write the channel pannings
		/// </summary>
		/********************************************************************/
		private void WriteChannelPannings(Mo3Module module, ConverterStream converterStream)
		{
			for (int i = 0; i < module.Header.NumChannels; i++)
				converterStream.Write_UINT8((byte)((module.Header.ChnPan[i] >> 4) | 0x20));

			converterStream.Write(Enumerable.Repeat<byte>(0x00, 32 - module.Header.NumChannels).ToArray());	
		}



		/********************************************************************/
		/// <summary>
		/// Make sure the module is aligned
		/// </summary>
		/********************************************************************/
		private void AlignModule(ConverterStream converterStream)
		{
			// Since the instruments pointers points to block of 16 bytes, we need to be sure
			// we are on a position which aligns to 16
			int bytesToPad = (int)(16 - (converterStream.Position % 16));
			if (bytesToPad < 16)
				converterStream.Write(Enumerable.Repeat<byte>(0x00, bytesToPad).ToArray());	
		}



		/********************************************************************/
		/// <summary>
		/// Write all the sample information
		/// </summary>
		/********************************************************************/
		private List<ushort> WriteSampleInfo(Mo3Module module, ConverterStream converterStream)
		{
			List<ushort> paraPointers = new List<ushort>();

			bool frequencyInHertz = (module.Version >= 5) || ((module.Header.Flags & HeaderFlag.LinearSlides) == 0);

			for (int i = 0; i < module.Header.NumSamples; i++)
			{
				paraPointers.Add((ushort)(converterStream.Position / 16));

				Sample sample = module.Samples[i];

				if ((sample.Flags & SampleInfoFlag.OplInstrument) != 0)
				{
					// Adlib instrument
					converterStream.Write_UINT8(2);		// Type -> Adlib
					converterStream.WriteString(sample.FileName, 12);

					converterStream.Write_UINT8(0);
					converterStream.Write_L_UINT16(0);

					// Here the adlib parameters should be written, but we don't
					// have them yet, so just reserve the space. When we got them,
					// they will be written in here
					converterStream.Write(Enumerable.Repeat<byte>(0, 12).ToArray());

					converterStream.Write_UINT8(sample.DefaultVolume);
					converterStream.Write_UINT8(0);
					converterStream.Write_UINT8(0);
					converterStream.Write_UINT8(0);

					converterStream.Write_L_UINT32(frequencyInHertz ? sample.FreqFineTune : (uint)(8363.0 * Math.Pow(2.0, ((int)sample.FreqFineTune + 1408) / 1536.0)));
					converterStream.Write(Enumerable.Repeat<byte>(0, 12).ToArray());	// Padding

					converterStream.WriteString(sample.SampleName, 28);
					converterStream.Write_B_UINT32(0x53435249);		// SCRI
				}
				else
				{
					// Sample
					converterStream.Write_UINT8(1);		// Type -> sample
					converterStream.WriteString(sample.FileName, 12);

					// MemSeq. Don't know the value yet, so just write 0
					converterStream.Write_UINT8(0);
					converterStream.Write_L_UINT16(0);

					converterStream.Write_L_UINT32(sample.Length);

					if ((sample.Flags & SampleInfoFlag.Loop) != 0)
					{
						converterStream.Write_L_UINT32(sample.LoopStart);
						converterStream.Write_L_UINT32(sample.LoopEnd);
					}
					else
					{
						converterStream.Write_L_UINT32(0);
						converterStream.Write_L_UINT32(0);
					}

					converterStream.Write_UINT8(sample.DefaultVolume);
					converterStream.Write_UINT8(0);		// Padding
					converterStream.Write_UINT8(0);		// 0 = Sample data not packed

					byte flags = 0x00;

					if ((sample.Flags & SampleInfoFlag.Loop) != 0)
						flags |= 0x01;

					if ((sample.Flags & SampleInfoFlag.Stereo) != 0)
						flags |= 0x02;

					if ((sample.Flags & SampleInfoFlag._16Bit) != 0)
						flags |= 0x04;

					converterStream.Write_UINT8(flags);

					converterStream.Write_L_UINT32(sample.FreqFineTune);
					converterStream.Write_L_UINT32(0);		// Padding
					converterStream.Write_L_UINT16(0);		// Int:Gp
					converterStream.Write_L_UINT16(0);		// Int:512
					converterStream.Write_L_UINT32(0);		// Int:Last used

					converterStream.WriteString(sample.SampleName, 28);
					converterStream.Write_B_UINT32(0x53435253);		// SCRS
				}
			}

			return paraPointers;
		}



		/********************************************************************/
		/// <summary>
		/// Will recreate all the patterns based on the tracks
		/// </summary>
		/********************************************************************/
		private List<ushort> WritePatterns(Mo3Module module, ConverterStream converterStream)
		{
			List<ushort> paraPointers = new List<ushort>();
			List<byte> packedPatternData = new List<byte>();

			for (int i = 0; i < module.Header.NumPatterns; i++)
			{
				// Make sure a new pattern is aligned at a 16 bytes block
				int bytesToPad = (int)(16 - (converterStream.Position % 16));
				if (bytesToPad < 16)
					converterStream.Write(Enumerable.Repeat<byte>(0x00, bytesToPad).ToArray());	

				paraPointers.Add((ushort)(converterStream.Position / 16));

				packedPatternData.Clear();

				int numberOfRows = module.PatternInfo.RowLengths[i];

				for (int r = 0; r < numberOfRows; r++)
				{
					for (int j = 0; j < module.Header.NumChannels; j++)
					{
						Track trk = module.Tracks[module.PatternInfo.Sequences[i, j]];

						if (r < trk.Rows.Count)
						{
							TrackRow row = trk.Rows[r];

							// If row is empty, skip it
							if ((row.Note == 0) && (row.Instrument == 0) && (row.Effects == null))
								continue;

							byte ctrl = 0x00;

							byte note = 0;
							if (row.Note != 0)
							{
								note = row.Note;
								ctrl |= 0x20;

								if ((note != 0xff) && (note != 0xfe))
								{
									note--;
									note = (byte)(((note / 12) << 4) | (note % 12));
								}
							}

							byte instr = row.Instrument;
							if (instr != 0)
								ctrl |= 0x20;

							var convertedEffect = ConvertEffect(row.Effects);

							if (convertedEffect.VolCmd != 0)
								ctrl |= 0x40;

							if (convertedEffect.Effect != 0)
								ctrl |= 0x80;

							if (ctrl != 0)
							{
								packedPatternData.Add((byte)(ctrl | j));

								if ((ctrl & 0x20) != 0)
								{
									packedPatternData.Add(note);
									packedPatternData.Add(instr);
								}

								if ((ctrl & 0x40) != 0)
									packedPatternData.Add((byte)(convertedEffect.VolCmd - 1));

								if ((ctrl & 0x80) != 0)
								{
									packedPatternData.Add(convertedEffect.Effect);
									packedPatternData.Add(convertedEffect.EffectVal);
								}
							}
						}
					}

					packedPatternData.Add(0x00);		// End of row
				}

				converterStream.Write_L_UINT16((ushort)packedPatternData.Count);
				converterStream.Write(packedPatternData.ToArray());
			}

			return paraPointers;
		}



		/********************************************************************/
		/// <summary>
		/// Will convert the effects to S3M vol + effect
		/// </summary>
		/********************************************************************/
		private (byte VolCmd, byte Effect, byte EffectVal) ConvertEffect(List<(Effect, byte)> effects)
		{
			(byte VolCmd, byte Effect, byte EffectVal) result = (0, 0, 0);

			if (effects != null)
			{
				foreach (var effect in effects)
				{
					// Note that effects TonePortaVol and VibratoVol are coded as two effects:
					// 07 00 22 x
					// 06 00 22 x
					switch (effect.Item1)
					{
						case Effect.Speed:
						{
							result.Effect = 0x01;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.PositionJump:
						{
							result.Effect = 0x02;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.PatternBreak:
						{
							result.Effect = 0x03;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.VolumeSlide2:
						{
							if (result.Effect == 0x07)
								result.Effect = 0x0c;
							else if (result.Effect == 0x08)
								result.Effect = 0x0b;
							else
								result.Effect = 0x04;

							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.PortamentoDown2:
						{
							result.Effect = 0x05;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.PortamentoUp2:
						{
							result.Effect = 0x06;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.TonePortamento:
						{
							result.Effect = 0x07;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.Vibrato:
						{
							result.Effect = 0x08;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.Tremor2:
						{
							result.Effect = 0x09;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.Arpeggio:
						{
							result.Effect = 0x0a;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.ChannelVolSlide:
						{
							result.Effect = 0x0e;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.Offset:
						{
							result.Effect = 0x0f;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.PanningSlide2:
						{
							result.Effect = 0x10;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.Retrig2:
						{
							result.Effect = 0x11;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.Tremolo:
						{
							result.Effect = 0x12;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.S3MCmdEx:
						{
							result.Effect = 0x13;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.Tempo1:
						case Effect.Tempo2:
						{
							result.Effect = 0x14;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.FineVibrato:
						{
							result.Effect = 0x15;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.GlobalVolume:
						{
							result.Effect = 0x16;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.GlobalVolSlide2:
						{
							result.Effect = 0x17;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.Panning8:
						{
							result.Effect = 0x18;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.Midi:
						{
							result.Effect = 0x19;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.Volume:
						{
							if ((result.VolCmd == 0) && (effect.Item2 <= 64))
								result.VolCmd = (byte)(effect.Item2 + 1);	// Increment by one so I can detect if it has been set. Will be decremented again later

							break;
						}

						default:
						{
							throw new NotImplementedException($"Effect {effect.Item1} not implemented for S3M modules");
						}
					}
				}
			}

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Will write all the sample data
		/// </summary>
		/********************************************************************/
		private List<uint> WriteSampleData(ConverterStream converterStream, List<ushort> sampleInfoParaPointers, DecodeSampleInfo[] decodeSampleInfo)
		{
			List<uint> paraPointers = new List<uint>();

			for (int i = 0; i < decodeSampleInfo.Length; i++)
			{
				DecodeSampleInfo sampleInfo = decodeSampleInfo[i];

				if (sampleInfo.SampleData != null)
				{
					// Make sure a new sample is aligned at a 16 bytes block
					int bytesToPad = (int)(16 - (converterStream.Position % 16));
					if (bytesToPad < 16)
						converterStream.Write(Enumerable.Repeat<byte>(0x00, bytesToPad).ToArray());	

					paraPointers.Add((ushort)(converterStream.Position / 16));

					converterStream.Write(sampleInfo.SampleData, 0, sampleInfo.SampleData.Length);
				}
				else
				{
					paraPointers.Add(0);

					if (sampleInfo.OplData != null)
					{
						converterStream.Seek(sampleInfoParaPointers[i] * 16 + 16, SeekOrigin.Begin);
						converterStream.Write(sampleInfo.OplData, 0, sampleInfo.OplData.Length);

						converterStream.Seek(0, SeekOrigin.End);
					}
				}
			}

			return paraPointers;
		}



		/********************************************************************/
		/// <summary>
		/// Will write all the para pointers back
		/// </summary>
		/********************************************************************/
		private void WriteParaPointers(ConverterStream converterStream, long paraPointersPosition, List<ushort> sampleInfoParaPointers, List<ushort> patternParaPointers, List<uint> sampleDataParaPointers)
		{
			converterStream.Seek(paraPointersPosition, SeekOrigin.Begin);

			converterStream.WriteArray_L_UINT16s(sampleInfoParaPointers, sampleInfoParaPointers.Count);
			converterStream.WriteArray_L_UINT16s(patternParaPointers, patternParaPointers.Count);

			for (int i = 0; i < sampleInfoParaPointers.Count; i++)
			{
				converterStream.Seek(sampleInfoParaPointers[i] * 16 + 13, SeekOrigin.Begin);

				uint sampleDataPosition = sampleDataParaPointers[i];
				converterStream.Write_UINT8((byte)((sampleDataPosition & 0xff0000) >> 16));
				converterStream.Write_L_UINT16((ushort)(sampleDataPosition & 0x00ffff));
			}
		}
		#endregion
	}
}
