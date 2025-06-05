/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers.Chunks;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Formats
{
	/// <summary>
	/// Will save a XM file
	/// </summary>
	internal class XmSaver : IFormatSaver
	{
		#region IFormatSaver implementation
		/********************************************************************/
		/// <summary>
		/// Save the module into XM format
		/// </summary>
		/********************************************************************/
		public bool SaveModule(Mo3Module module, ModuleStream moduleStream, ConverterStream converterStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			if (module.Chunks.Length > 1)
				throw new NotImplementedException("Chunks in XM");

			if (module.Plugins.Length > 0)
				throw new NotImplementedException("Plugins in XM");

			WriteHeader(module, converterStream);
			WritePositionList(module, converterStream);
			WritePatterns(module, converterStream);
			return WriteInstrumentAndSampleInfoAndSampleData(module, moduleStream, converterStream);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Write back the header
		/// </summary>
		/********************************************************************/
		private void WriteHeader(Mo3Module module, ConverterStream converterStream)
		{
			FileHeader header = module.Header;

			converterStream.WriteMark("Extended Module: ");
			converterStream.WriteString(module.Header.SongName, 20, 0x20);
			converterStream.Write_UINT8(0x1a);

			VersChunk versChunk = module.FindChunk<VersChunk>();
			if (versChunk != null)
				converterStream.WriteString(versChunk.CreatedWithTracker, 20, 0x20);
			else
				converterStream.WriteMark("FastTracker v2.00   ");

			converterStream.Write_L_UINT16(0x0104);		// Version
			converterStream.Write_L_UINT32(0x0114);		// Header size

			converterStream.Write_L_UINT16(header.NumOrders);
			converterStream.Write_L_UINT16(header.RestartPos);
			converterStream.Write_L_UINT16(header.NumChannels);
			converterStream.Write_L_UINT16(header.NumPatterns);
			converterStream.Write_L_UINT16(header.NumInstruments);

			ushort flags = 0x0000;

			if ((header.Flags & HeaderFlag.LinearSlides) != 0)
				flags |= 0x0001;

			converterStream.Write_L_UINT16(flags);

			converterStream.Write_L_UINT16(header.DefaultSpeed);
			converterStream.Write_L_UINT16(header.DefaultTempo);
		}



		/********************************************************************/
		/// <summary>
		/// Write the position list and its information
		/// </summary>
		/********************************************************************/
		private void WritePositionList(Mo3Module module, ConverterStream converterStream)
		{
			converterStream.Write(module.PatternInfo.PositionList, 0, module.Header.NumOrders);

			if (module.Header.NumOrders < 256)
				converterStream.Write(Enumerable.Repeat<byte>(0, 256 - module.Header.NumOrders).ToArray());
		}



		/********************************************************************/
		/// <summary>
		/// Will recreate all the patterns based on the tracks
		/// </summary>
		/********************************************************************/
		private void WritePatterns(Mo3Module module, ConverterStream converterStream)
		{
			List<byte> packedPatternData = new List<byte>();

			for (int i = 0; i < module.Header.NumPatterns; i++)
			{
				converterStream.Write_L_UINT32(9);		// Header size
				converterStream.Write_UINT8(0);			// Packing type

				ushort numberOfRows = module.PatternInfo.RowLengths[i];
				converterStream.Write_L_UINT16(numberOfRows);

				packedPatternData.Clear();

				for (int r = 0; r < numberOfRows; r++)
				{
					for (int j = 0; j < module.Header.NumChannels; j++)
					{
						Track trk = module.Tracks[module.PatternInfo.Sequences[i, j]];

						if (r < trk.Rows.Count)
						{
							TrackRow row = trk.Rows[r];

							var convertedEffect = ConvertEffect(row.Effects);

							// If there is a value in all fields, just store them
							if ((row.Note != 0) && (row.Instrument != 0) && (convertedEffect.VolCmd != 0) && (convertedEffect.Effect != 0) && (convertedEffect.EffectVal != 0))
							{
								packedPatternData.Add(row.Note);
								packedPatternData.Add(row.Instrument);
								packedPatternData.Add(convertedEffect.VolCmd);
								packedPatternData.Add(convertedEffect.Effect);
								packedPatternData.Add(convertedEffect.EffectVal);
							}
							else
							{
								byte ctrl = 0x80;

								byte note = 0;
								if (row.Note != 0)
								{
									note = row.Note;
									ctrl |= 0x01;

									if (note == 0xff)
										note = 0x61;	// Key off
								}

								byte instr = row.Instrument;
								if (instr != 0)
									ctrl |= 0x02;

								if (convertedEffect.VolCmd != 0)
									ctrl |= 0x04;

								if (convertedEffect.Effect != 0)
									ctrl |= 0x08;

								if (convertedEffect.EffectVal != 0)
									ctrl |= 0x10;

								packedPatternData.Add(ctrl);

								if ((ctrl & 0x01) != 0)
									packedPatternData.Add(note);

								if ((ctrl & 0x02) != 0)
									packedPatternData.Add(instr);

								if ((ctrl & 0x04) != 0)
									packedPatternData.Add(convertedEffect.VolCmd);

								if ((ctrl & 0x08) != 0)
									packedPatternData.Add(convertedEffect.Effect);

								if ((ctrl & 0x10) != 0)
									packedPatternData.Add(convertedEffect.EffectVal);
							}
						}
						else
							packedPatternData.Add(0x80);
					}
				}

				converterStream.Write_L_UINT16((ushort)packedPatternData.Count);
				converterStream.Write(packedPatternData.ToArray());
			}
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
					switch (effect.Item1)
					{
						case Effect.Arpeggio:
						{
							result.Effect = 0x00;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.PortamentoUp1:
						{
							result.Effect = 0x01;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.PortamentoDown1:
						{
							result.Effect = 0x02;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.TonePortamento:
						{
							if ((result.VolCmd == 0x00) && ((effect.Item2 & 0x0f) == 0))
								result.VolCmd = (byte)(0xf0 | (effect.Item2 >> 4));
							else
							{
								result.Effect = 0x03;
								result.EffectVal = effect.Item2;
							}
							break;
						}

						case Effect.Vibrato:
						{
							result.Effect = 0x04;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.TonePortaVol:
						{
							result.Effect = 0x05;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.VibratoVol:
						{
							result.Effect = 0x06;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.Tremolo:
						{
							result.Effect = 0x07;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.Panning8:
						{
							if ((result.VolCmd == 0x00) && ((effect.Item2 & 0x0f) == 0))
								result.VolCmd = (byte)(0xc0 | (effect.Item2 >> 4));
							else
							{
								result.Effect = 0x08;
								result.EffectVal = effect.Item2;
							}
							break;
						}

						case Effect.Offset:
						{
							result.Effect = 0x09;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.VolumeSlide1:
						{
							result.Effect = 0x0a;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.PositionJump:
						{
							result.Effect = 0x0b;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.Volume:
						{
							if ((result.VolCmd == 0x00) && (effect.Item2 <= 64))
								result.VolCmd = (byte)(0x10 + effect.Item2);
							else
							{
								result.Effect = 0x0c;
								result.EffectVal = effect.Item2;
							}
							break;
						}

						case Effect.PatternBreak:
						{
							result.Effect = 0x0d;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.ModCmdEx:
						{
							result.Effect = 0x0e;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.Tempo1:
						{
							result.Effect = 0x0f;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.GlobalVolume:
						{
							result.Effect = 0x10;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.GlobalVolSlide1:
						{
							result.Effect = 0x11;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.KeyOff:
						{
							result.Effect = 0x14;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.SetEnvPosition:
						{
							result.Effect = 0x15;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.PanningSlide1:
						{
							result.Effect = 0x19;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.Retrig1:
						{
							result.Effect = 0x1b;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.Tremor1:
						{
							result.Effect = 0x1d;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.UnusedW:
						{
							result.Effect = 0x20;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.XFinePortaUp:
						{
							result.Effect = 0x21;
							result.EffectVal = (byte)(0x10 | effect.Item2);
							break;
						}

						case Effect.XFinePortaDown:
						{
							result.Effect = 0x21;
							result.EffectVal = (byte)(0x20 | effect.Item2);
							break;
						}

						case Effect.Panbrello:
						{
							result.Effect = 0x22;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.Midi:
						{
							result.Effect = 0x23;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.Vol_VolSlide1:
						{
							if ((effect.Item2 & 0xf0) != 0)
								result.VolCmd = (byte)(0x70 | (effect.Item2 >> 4));
							else
								result.VolCmd = (byte)(0x60 | (effect.Item2 & 0x0f));

							break;
						}

						case Effect.Vol_FineVol:
						{
							if ((effect.Item2 & 0xf0) != 0)
								result.VolCmd = (byte)(0x90 | (effect.Item2 >> 4));
							else
								result.VolCmd = (byte)(0x80 | (effect.Item2 & 0x0f));

							break;
						}

						case Effect.Vol_PanSlide:
						{
							if ((effect.Item2 & 0xf0) != 0)
								result.VolCmd = (byte)(0xe0 | (effect.Item2 >> 4));
							else
								result.VolCmd = (byte)(0xd0 | (effect.Item2 & 0x0f));

							break;
						}

						case Effect.Vol_VibratoSpeed:
						{
							result.VolCmd = (byte)(0xa0 | effect.Item2);
							break;
						}

						case Effect.Vol_VibratoDepth:
						{
							result.VolCmd = (byte)(0xb0 | effect.Item2);
							break;
						}
					}
				}
			}

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Write all the instrument and sample information
		/// </summary>
		/********************************************************************/
		private bool WriteInstrumentAndSampleInfoAndSampleData(Mo3Module module, ModuleStream moduleStream, ConverterStream converterStream)
		{
			// Decode all the samples
			DecodeSampleInfo[] decodeSampleInfo = Mo3SampleWriter.PrepareSamples(module, moduleStream);
			if (decodeSampleInfo == null)
				return false;

			// Start to find minimum sample number for each instrument.
			// This is needed, because sometimes in the sample maps,
			// there are gaps between instruments. E.g, instrument 1
			// may reference to the first 4 samples in the sample map.
			// Instrument 2 reference to sample 7. Therefore sample
			// 5 to 6 are stored in instrument 1
			int[] firstSampleNumbers = new int[module.Header.NumInstruments];

			for (int i = 0; i < module.Header.NumInstruments; i++)
			{
				Instrument instr = module.Instruments[i];

				// Only first 96 values are used in XM. Also skip
				// note values and only use sample numbers
				int minSampleNum = int.MaxValue;

				for (int j = 1; j < 96 * 2; j += 2)
				{
					if ((instr.SampleMap[j] != 0xffff) && (instr.SampleMap[j] < minSampleNum))
						minSampleNum = instr.SampleMap[j];
				}

				firstSampleNumbers[i] = minSampleNum == int.MaxValue ? -1 : minSampleNum;
			}

			for (int i = 0; i < module.Header.NumInstruments; i++)
			{
				int numberOfSamples = 0;

				if (firstSampleNumbers[i] != -1)
				{
					int nextInstrumentSampleNumber = -1;

					for (int j = i + 1; j < module.Header.NumInstruments; j++)
					{
						if (firstSampleNumbers[j] != -1)
						{
							nextInstrumentSampleNumber = firstSampleNumbers[j];
							break;
						}
					}

					if (nextInstrumentSampleNumber == -1)
						nextInstrumentSampleNumber = module.Header.NumSamples;

					numberOfSamples = nextInstrumentSampleNumber - firstSampleNumbers[i];
				}

				uint headerSize = 29;

				if (numberOfSamples > 0)
					headerSize += 234;

				Instrument instr = module.Instruments[i];

				// Write instrument header
				converterStream.Write_L_UINT32(headerSize);
				converterStream.WriteString(instr.InstrumentName, 22);
				converterStream.Write_UINT8(0);		// Type
				converterStream.Write_L_UINT16((ushort)numberOfSamples);

				if (numberOfSamples > 0)
				{
					// Write rest of instrument header
					converterStream.Write_L_UINT32(0x28);		// Sample header size. This number does not make sense at all, but it seems to be a common value. LibXmp ignore this value anyway

					for (int j = 1; j < 96 * 2; j += 2)
						converterStream.Write_UINT8((byte)(instr.SampleMap[j] - firstSampleNumbers[i]));

					converterStream.WriteArray_L_INT16s(instr.VolEnv.Points, 24);
					converterStream.WriteArray_L_INT16s(instr.PanEnv.Points, 24);

					converterStream.Write_UINT8(instr.VolEnv.NumNodes);
					converterStream.Write_UINT8(instr.PanEnv.NumNodes);
					converterStream.Write_UINT8(instr.VolEnv.SustainStart);
					converterStream.Write_UINT8(instr.VolEnv.LoopStart);
					converterStream.Write_UINT8(instr.VolEnv.LoopEnd);
					converterStream.Write_UINT8(instr.PanEnv.SustainStart);
					converterStream.Write_UINT8(instr.PanEnv.LoopStart);
					converterStream.Write_UINT8(instr.PanEnv.LoopEnd);

					byte flag = 0x00;

					if ((instr.VolEnv.Flags & EnvelopeFlag.Enabled) != 0)
						flag |= 0x01;

					if ((instr.VolEnv.Flags & EnvelopeFlag.Sustain) != 0)
						flag |= 0x02;

					if ((instr.VolEnv.Flags & EnvelopeFlag.Loop) != 0)
						flag |= 0x04;

					converterStream.Write_UINT8(flag);

					flag = 0x00;

					if ((instr.PanEnv.Flags & EnvelopeFlag.Enabled) != 0)
						flag |= 0x01;

					if ((instr.PanEnv.Flags & EnvelopeFlag.Sustain) != 0)
						flag |= 0x02;

					if ((instr.PanEnv.Flags & EnvelopeFlag.Loop) != 0)
						flag |= 0x04;

					converterStream.Write_UINT8(flag);

					converterStream.Write_UINT8(instr.Vibrato.Type);
					converterStream.Write_UINT8(instr.Vibrato.Sweep);
					converterStream.Write_UINT8(instr.Vibrato.Depth);
					converterStream.Write_UINT8(instr.Vibrato.Rate);

					converterStream.Write_L_UINT16(instr.FadeOut);

					converterStream.Write(Enumerable.Repeat<byte>(0, 22).ToArray());	// Reserved

					// Now take the sample headers
					int firstSample = firstSampleNumbers[i];

					for (int j = 0; j < numberOfSamples; j++)
					{
						Sample sample = module.Samples[firstSample + j];

						uint multiply = (sample.Flags & SampleInfoFlag._16Bit) != 0 ? 2U : 1;
						multiply *= (sample.Flags & SampleInfoFlag.Stereo) != 0 ? 2U : 1;

						converterStream.Write_L_UINT32(sample.Length * multiply);

						if ((sample.Flags & SampleInfoFlag.Loop) != 0)
						{
							converterStream.Write_L_UINT32(sample.LoopStart * multiply);
							converterStream.Write_L_UINT32((sample.LoopEnd - sample.LoopStart) * multiply);
						}
						else
						{
							converterStream.Write_L_UINT32(0);
							converterStream.Write_L_UINT32(0);
						}

						converterStream.Write_UINT8(sample.DefaultVolume);
						converterStream.Write_INT8((sbyte)(sample.FreqFineTune - 128));

						flag = 0x00;

						if ((sample.Flags & SampleInfoFlag.Loop) != 0)
							flag |= 0x01;

						if ((sample.Flags & SampleInfoFlag.PingPongLoop) != 0)
							flag |= 0x02;

						if ((sample.Flags & SampleInfoFlag._16Bit) != 0)
							flag |= 0x10;

						if ((sample.Flags & SampleInfoFlag.Stereo) != 0)
							flag |= 0x20;

						converterStream.Write_UINT8(flag);

						converterStream.Write_UINT8((byte)sample.Panning);
						converterStream.Write_INT8(sample.Transpose);
						converterStream.Write_UINT8(0);		// Reserved

						converterStream.WriteString(sample.SampleName, 22, 0x20);
					}

					// Write sample data
					for (int j = 0; j < numberOfSamples; j++)
					{
						DecodeSampleInfo sampleInfo = decodeSampleInfo[firstSample + j];

						if ((sampleInfo.SampleData != null) && (sampleInfo.SampleData.Length > 0))
						{
							if ((sampleInfo.SampleHeader.Flags & SampleInfoFlag._16Bit) != 0)
							{
								Span<ushort> sampleData = MemoryMarshal.Cast<byte, ushort>(sampleInfo.SampleData);

								ushort previousSample = sampleData[0];

								for (int k = 1; k < sampleData.Length; k++)
								{
									ushort newSample = (ushort)(sampleData[k] - previousSample);
									previousSample = sampleData[k];
									sampleData[k] = newSample;
								}
							}
							else
							{
								byte[] sampleData = sampleInfo.SampleData;
								byte previousSample = sampleData[0];

								for (int k = 1; k < sampleData.Length; k++)
								{
									byte newSample = (byte)(sampleData[k] - previousSample);
									previousSample = sampleData[k];
									sampleData[k] = newSample;
								}
							}

							converterStream.Write(sampleInfo.SampleData, 0, sampleInfo.SampleData.Length);
						}
					}
				}
			}

			return true;
		}
		#endregion
	}
}
