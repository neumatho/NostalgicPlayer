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
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers.Plugins;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Formats
{
	/// <summary>
	/// Will save an IT file
	/// </summary>
	internal class ItSaver : IFormatSaver
	{
		private static readonly byte[] portaVolCmd =
		[
			0x00, 0x01, 0x04, 0x08, 0x10, 0x20, 0x40, 0x60, 0x80, 0xff
		];

		#region IFormatSaver implementation
		/********************************************************************/
		/// <summary>
		/// Save the module into S3M format
		/// </summary>
		/********************************************************************/
		public bool SaveModule(Mo3Module module, ModuleStream moduleStream, ConverterStream converterStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			if (IsOpenMpt(module))
			{
				errorMessage = Resources.IDS_ERR_OPENMPT_NOT_SUPPORT;
				return false;
			}

			VersChunk versChunk = WriteHeader(module, converterStream);
			WriteChannelPannings(module, converterStream);
			WriteChannelVolumes(module, converterStream);
			WritePositionList(module, converterStream);
			long offsetsPosition = ReserveSpaceForOffsets(module, converterStream);
			WriteMidiChunk(module, converterStream);
			WriteOmptChunk(module, converterStream);
			WriteFxPlugin(module, converterStream);
			WriteComment(module, converterStream);
			List<uint> instrumentInfoOffsets = WriteInstrumentInfo(module, converterStream, versChunk);
			List<uint> sampleInfoOffsets = WriteSampleInfo(module, converterStream);
			List<uint> patternOffsets = WritePatterns(module, converterStream);

			List<uint> sampleDataOffsets = WriteSampleData(module, moduleStream, converterStream);
			if (sampleDataOffsets == null)
			{
				errorMessage = Resources.IDS_ERR_LOADING_SAMPLES;
				return false;
			}

			WriteOffsets(converterStream, offsetsPosition, instrumentInfoOffsets, sampleInfoOffsets, patternOffsets, sampleDataOffsets);

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Check if MIDI is used
		/// </summary>
		/********************************************************************/
		private void CheckMidi(Mo3Module module)
		{
			if (module.Header.FixedMacros.SelectMany(x => x).Any(x => x != 0))
				throw new NotImplementedException("Macros not implemented yet");

			if (module.Header.SfxMacros.Any(x => x != 0))
				throw new NotImplementedException("Macros not implemented yet");
		}



		/********************************************************************/
		/// <summary>
		/// Check if the module is in OpenMPT format
		/// </summary>
		/********************************************************************/
		private bool IsOpenMpt(Mo3Module module)
		{
			OmptChunk omptChunk = module.FindChunk<OmptChunk>();
			if (omptChunk == null)
				return false;

			for (int i = 0; i < omptChunk.Data.Length - 2; i += 2)
			{
				if ((omptChunk.Data[i] == 0x32) && (omptChunk.Data[i + 1] == 0x32) && (omptChunk.Data[i + 2] == 0x38))
					return true;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Write back the header
		/// </summary>
		/********************************************************************/
		private VersChunk WriteHeader(Mo3Module module, ConverterStream converterStream)
		{
			FileHeader header = module.Header;

			converterStream.Write_B_UINT32(0x494d504d);			// IMPM
			converterStream.WriteString(header.SongName, 26);

			converterStream.Write_L_UINT16(0);					// PHiligt
			converterStream.Write_L_UINT16((ushort)(header.NumOrders + 1));		// Reserved space for end marker
			converterStream.Write_L_UINT16(header.NumInstruments);
			converterStream.Write_L_UINT16(header.NumSamples);
			converterStream.Write_L_UINT16(header.NumPatterns);

			VersChunk versChunk = module.FindChunk<VersChunk>();
			if (versChunk == null)
			{
				versChunk = new VersChunk
				{
					Cwtv = 0x200,
					Cmwt = 0x200
				};
			}

			converterStream.Write_L_UINT16(versChunk.Cwtv);
			converterStream.Write_L_UINT16(versChunk.Cmwt);

			ushort flags = 0x0001;		// Always enable stereo

			if ((header.Flags & HeaderFlag.InstrumentMode) != 0)
				flags |= 0x0004;

			if ((header.Flags & HeaderFlag.LinearSlides) != 0)
				flags |= 0x0008;

			if ((header.Flags & HeaderFlag.ItOldFx) == 0)
				flags |= 0x0010;

			if ((header.Flags & HeaderFlag.ItCompactGxx) == 0)
				flags |= 0x0020;

			if ((header.Flags & HeaderFlag.ExtFilterRange) != 0)
				flags |= 0x1000;

			converterStream.Write_L_UINT16(flags);

			flags = 0x0000;

			if (header.SongMessage.Length > 0)
				flags |= 0x0001;

			converterStream.Write_L_UINT16(flags);

			converterStream.Write_UINT8(header.GlobalVol);

			int mixVol = header.SampleVolume < 0 ? header.SampleVolume + 52 : (int)(Math.Exp(header.SampleVolume * 3.1 / 20.0) + 51);
			converterStream.Write_UINT8((byte)mixVol);

			converterStream.Write_UINT8(header.DefaultSpeed);
			converterStream.Write_UINT8(header.DefaultTempo);
			converterStream.Write_UINT8(header.PanSeparation);
			converterStream.Write_UINT8(0);			// Pitch wheel depth

			converterStream.Write_L_UINT16((ushort)(header.SongMessage.Length + 1));	// Reserve space for null terminator
			converterStream.Write_L_UINT32(0);		// Message offset. Will be written later
			converterStream.Write_L_UINT32(0);		// Reserved

			return versChunk;
		}



		/********************************************************************/
		/// <summary>
		/// Write back the channel panning table
		/// </summary>
		/********************************************************************/
		private void WriteChannelPannings(Mo3Module module, ConverterStream converterStream)
		{
			for (int i = 0; i < module.Header.NumChannels; i++)
			{
				byte panning = module.Header.ChnPan[i];
				if (panning == 127)
					converterStream.Write_UINT8(100);
				else
					converterStream.Write_UINT8((byte)(panning / 4));
			}

			if (module.Header.NumChannels < 64)
				converterStream.Write(Enumerable.Repeat<byte>(255, 64 - module.Header.NumChannels).ToArray());
		}



		/********************************************************************/
		/// <summary>
		/// Write back the channel volume table
		/// </summary>
		/********************************************************************/
		private void WriteChannelVolumes(Mo3Module module, ConverterStream converterStream)
		{
			converterStream.Write(module.Header.ChnVolume, 0, module.Header.NumChannels);

			if (module.Header.NumChannels < 64)
				converterStream.Write(Enumerable.Repeat<byte>(64, 64 - module.Header.NumChannels).ToArray());
		}



		/********************************************************************/
		/// <summary>
		/// Write the position list and its information
		/// </summary>
		/********************************************************************/
		private void WritePositionList(Mo3Module module, ConverterStream converterStream)
		{
			converterStream.Write(module.PatternInfo.PositionList, 0, module.Header.NumOrders);

			// Write end mark
			converterStream.Write_UINT8(0xff);
		}



		/********************************************************************/
		/// <summary>
		/// Reserve space in the IT file offsets
		/// </summary>
		/********************************************************************/
		private long ReserveSpaceForOffsets(Mo3Module module, ConverterStream converterStream)
		{
			// At this point, we don't know the values of the offsets yet, so we just
			// reserve the space in the converted file for them. They will be written later
			int spaceToReserve = module.Header.NumInstruments + module.Header.NumSamples + module.Header.NumPatterns;

			long currentPosition = converterStream.Position;
			converterStream.WriteArray_L_UINT32s(Enumerable.Repeat<uint>(0, spaceToReserve).ToArray(), spaceToReserve);

			// For some reason, it seems like 2 extra zero bytes are written after this
			converterStream.Write_L_UINT16(0);

			return currentPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Write back the MIDI chunk if any
		/// </summary>
		/********************************************************************/
		private void WriteMidiChunk(Mo3Module module, ConverterStream converterStream)
		{
			MidiChunk midiChunk = module.FindChunk<MidiChunk>();
			if (midiChunk != null)
				converterStream.Write(midiChunk.Data, 0, midiChunk.Data.Length);
		}



		/********************************************************************/
		/// <summary>
		/// Write back the OMPT chunk if any
		/// </summary>
		/********************************************************************/
		private void WriteOmptChunk(Mo3Module module, ConverterStream converterStream)
		{
			OmptChunk omptChunk = module.FindChunk<OmptChunk>();
			if (omptChunk != null)
				converterStream.Write(omptChunk.Data, 0, omptChunk.Data.Length);
		}



		/********************************************************************/
		/// <summary>
		/// Write back the FX plugin if any
		/// </summary>
		/********************************************************************/
		private void WriteFxPlugin(Mo3Module module, ConverterStream converterStream)
		{
			foreach (FxPlugin fxPlugin in module.FindPlugin<FxPlugin>())
			{
				converterStream.Write_B_UINT16(0x4658);		// FX

				byte number = 0x30;
				if (fxPlugin.Plugin >= 10)
					number = (byte)(number + (fxPlugin.Plugin / 10));

				converterStream.Write_UINT8(number);
				converterStream.Write_UINT8((byte)(0x30 + (fxPlugin.Plugin % 10)));

				converterStream.Write_L_UINT32((uint)fxPlugin.Data.Length);

				converterStream.Write(fxPlugin.Data, 0, fxPlugin.Data.Length);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will write the comment
		/// </summary>
		/********************************************************************/
		private void WriteComment(Mo3Module module, ConverterStream converterStream)
		{
			if (module.Header.SongMessage.Length > 0)
			{
				long currentPosition = converterStream.Position;

				// Write the offset to the message
				converterStream.Seek(0x38, SeekOrigin.Begin);
				converterStream.Write_L_UINT32((uint)currentPosition);
				converterStream.Seek(0, SeekOrigin.End);

				// Then write the comment
				converterStream.Write(module.Header.SongMessage, 0, module.Header.SongMessage.Length);

				// And the null terminator
				converterStream.Write_UINT8(0x00);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Write all the instrument information
		/// </summary>
		/********************************************************************/
		private List<uint> WriteInstrumentInfo(Mo3Module module, ConverterStream converterStream, VersChunk versChunk)
		{
			List<uint> offsets = new List<uint>();

			if (versChunk.Cmwt < 0x200)
				throw new NotImplementedException("Old instruments not implemented yet");

			for (int i = 0; i < module.Header.NumInstruments; i++)
			{
				offsets.Add((uint)converterStream.Position);

				Instrument instr = module.Instruments[i];

				converterStream.Write_B_UINT32(0x494d5049);		// IMPI
				converterStream.WriteString(instr.FileName, 12);
				converterStream.Write_UINT8(0);

				converterStream.Write_UINT8(instr.Nna);
				converterStream.Write_UINT8(instr.Dct);
				converterStream.Write_UINT8(instr.Dca);
				converterStream.Write_L_UINT16((ushort)(instr.FadeOut / 32));
				converterStream.Write_UINT8(instr.Pps);
				converterStream.Write_UINT8(instr.Ppc);
				converterStream.Write_UINT8(instr.GlobalVol);
				converterStream.Write_UINT8((byte)(instr.Panning / 4));

				converterStream.Write_UINT8(0);			// Random volume variation
				converterStream.Write_UINT8(0);			// Random panning variation
				converterStream.Write_L_UINT16(0);		// Tracker version (only used in instrument files)
				converterStream.Write_UINT8(0);			// Number of samples associated (only used in instrument files)
				converterStream.Write_UINT8(0);

				converterStream.WriteString(instr.InstrumentName, 26);

				converterStream.Write_UINT8(instr.CutOff);
				converterStream.Write_UINT8(instr.Resonance);
				converterStream.Write_UINT8(instr.MidiChannel);
				converterStream.Write_UINT8(instr.MidiPatch);
				converterStream.Write_L_UINT16(instr.MidiBank);

				for (int j = 0; j < 240; j += 2)
				{
					converterStream.Write_UINT8((byte)instr.SampleMap[j]);

					byte samp = (byte)instr.SampleMap[j + 1];
					if (samp == 0xff)
						samp = 0x00;
					else if (samp == 0x00)
						samp = (byte)(i + 1);
					else
						samp++;

					converterStream.Write_UINT8(samp);
				}

				WriteEnvelope(instr.VolEnv, converterStream, 1, 0);
				WriteEnvelope(instr.PanEnv, converterStream, 1, 32);
				WriteEnvelope(instr.PitchEnv, converterStream, 32, 32);

				// Some padding
				converterStream.Write_L_UINT32(0);
			}

			return offsets;
		}



		/********************************************************************/
		/// <summary>
		/// Write a single envelope structure
		/// </summary>
		/********************************************************************/
		private void WriteEnvelope(Envelope env, ConverterStream converterStream, int divide, int substract)
		{
			byte flag = 0x00;

			if ((env.Flags & EnvelopeFlag.Enabled) != 0)
				flag |= 0x01;

			if ((env.Flags & EnvelopeFlag.Loop) != 0)
				flag |= 0x02;

			if ((env.Flags & EnvelopeFlag.Sustain) != 0)
				flag |= 0x04;

			if ((env.Flags & EnvelopeFlag.Filter) != 0)
				flag |= 0x80;

			if ((env.Flags & EnvelopeFlag.Carry) != 0)
				flag |= 0x08;

			converterStream.Write_UINT8(flag);

			converterStream.Write_UINT8(env.NumNodes);
			converterStream.Write_UINT8(env.LoopStart);
			converterStream.Write_UINT8(env.LoopEnd);
			converterStream.Write_UINT8(env.SustainStart);
			converterStream.Write_UINT8(env.SustainEnd);

			for (int i = 0; i < env.NumNodes; i++)
			{
				converterStream.Write_UINT8((byte)((env.Points[i * 2 + 1] / divide) - substract));
				converterStream.Write_L_INT16(env.Points[i * 2]);
			}

			if (env.NumNodes < 25)
				converterStream.Write(Enumerable.Repeat<byte>(0, (25 - env.NumNodes) * 3).ToArray());

			converterStream.Write_UINT8(0);		// Padding
		}



		/********************************************************************/
		/// <summary>
		/// Write all the sample information
		/// </summary>
		/********************************************************************/
		private List<uint> WriteSampleInfo(Mo3Module module, ConverterStream converterStream)
		{
			List<uint> offsets = new List<uint>();

			bool frequencyInHertz = (module.Version >= 5) || ((module.Header.Flags & HeaderFlag.LinearSlides) == 0);

			for (int i = 0; i < module.Header.NumSamples; i++)
			{
				offsets.Add((uint)converterStream.Position);

				Sample sample = module.Samples[i];

				converterStream.Write_B_UINT32(0x494d5053);		// IMPS
				converterStream.WriteString(sample.FileName, 12);

				converterStream.Write_UINT8(0);
				converterStream.Write_UINT8(sample.GlobalVol);

				byte flag = 0x01;		// Always associated with header

				if ((sample.Flags & SampleInfoFlag._16Bit) != 0)
					flag |= 0x02;

				if ((sample.Flags & SampleInfoFlag.Stereo) != 0)
					flag |= 0x04;

				if ((sample.Flags & SampleInfoFlag.Loop) != 0)
					flag |= 0x10;

				if ((sample.Flags & SampleInfoFlag.Sustain) != 0)
					flag |= 0x20;

				if ((sample.Flags & SampleInfoFlag.PingPongLoop) != 0)
					flag |= 0x40;

				if ((sample.Flags & SampleInfoFlag.SustainPingPong) != 0)
					flag |= 0x80;

				converterStream.Write_UINT8(flag);

				converterStream.Write_UINT8(sample.DefaultVolume);
				converterStream.WriteString(sample.SampleName, 26);
				converterStream.Write_UINT8(0x01);			// Samples are signed
				converterStream.Write_UINT8((byte)(sample.Panning == 0xffff ? 0x00 : sample.Panning / 4));

				converterStream.Write_L_UINT32(sample.Length);
				converterStream.Write_L_UINT32(sample.LoopStart);
				converterStream.Write_L_UINT32(sample.LoopEnd);
				converterStream.Write_L_UINT32(frequencyInHertz ? sample.FreqFineTune : (uint)(8363.0 * Math.Pow(2.0, ((int)sample.FreqFineTune + 1408) / 1536.0)));
				converterStream.Write_L_UINT32(sample.SustainStart);
				converterStream.Write_L_UINT32(sample.SustainEnd);
				converterStream.Write_L_UINT32(0);			// Sample pointer. Not known yet as this point, will be written back later

				converterStream.Write_UINT8(sample.VibSweep);
				converterStream.Write_UINT8(sample.VibDepth);
				converterStream.Write_UINT8(sample.VibRate);
				converterStream.Write_UINT8(sample.VibType);
			}

			return offsets;
		}



		/********************************************************************/
		/// <summary>
		/// Will recreate all the patterns based on the tracks
		/// </summary>
		/********************************************************************/
		private List<uint> WritePatterns(Mo3Module module, ConverterStream converterStream)
		{
			List<uint> offsets = new List<uint>();
			List<byte> packedPatternData = new List<byte>();

			byte[] previousMasks = new byte[module.Header.NumChannels];
			byte[] previousNote = new byte[module.Header.NumChannels];
			byte[] previousInstrument = new byte[module.Header.NumChannels];
			byte[] previousVolume = new byte[module.Header.NumChannels];
			byte[] previousEffect = new byte[module.Header.NumChannels * 2];

			for (int i = 0; i < module.Header.NumPatterns; i++)
			{
				offsets.Add((uint)converterStream.Position);

				packedPatternData.Clear();
				Array.Clear(previousMasks);
				Array.Clear(previousNote);
				Array.Clear(previousInstrument);
				Array.Clear(previousVolume);
				Array.Clear(previousEffect);

				ushort numberOfRows = module.PatternInfo.RowLengths[i];

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

							byte channel = (byte)(j + 1);

							byte mask = 0x00;

							byte note = 0;
							if (row.Note != 0)
							{
								note = row.Note;

								if (note < 120)
									note--;

								if (note != previousNote[j])
								{
									previousNote[j] = note;
									mask |= 01;
								}
								else
								{
									mask |= 0x10;
									note = 0;
								}
							}

							byte instr = row.Instrument;
							if (instr != 0)
							{
								if (instr != previousInstrument[j])
								{
									previousInstrument[j] = instr;
									mask |= 0x02;
								}
								else
								{
									mask |= 0x20;
									instr = 0;
								}
							}

							var convertedEffect = ConvertEffect(row.Effects);

							if (convertedEffect.VolCmd != 0)
							{
								if (convertedEffect.VolCmd != previousVolume[j])
								{
									previousVolume[j] = convertedEffect.VolCmd;
									mask |= 0x04;
								}
								else
								{
									mask |= 0x40;
									convertedEffect.VolCmd = 0;
								}
							}

							if ((convertedEffect.Effect != 0) || (convertedEffect.EffectVal != 0))
							{
								if ((convertedEffect.Effect != previousEffect[j * 2]) || (convertedEffect.EffectVal != previousEffect[j * 2 + 1]))
								{
									previousEffect[j * 2] = convertedEffect.Effect;
									previousEffect[j * 2 + 1] = convertedEffect.EffectVal;
									mask |= 0x08;
								}
								else
								{
									mask |= 0x80;
									convertedEffect.Effect = 0;
									convertedEffect.EffectVal = 0;
								}
							}

							if (mask != previousMasks[j])
							{
								previousMasks[j] = mask;
								packedPatternData.Add((byte)(channel | 0x80));
								packedPatternData.Add(mask);
							}
							else
								packedPatternData.Add(channel);

							if (note != 0)
								packedPatternData.Add(note);

							if (instr != 0)
								packedPatternData.Add(instr);

							if (convertedEffect.VolCmd != 0)
								packedPatternData.Add((byte)(convertedEffect.VolCmd - 1));

							if ((convertedEffect.Effect != 0) || (convertedEffect.EffectVal != 0))
							{
								packedPatternData.Add(convertedEffect.Effect);
								packedPatternData.Add(convertedEffect.EffectVal);
							}
						}
					}

					packedPatternData.Add(0x00);		// End of row
				}

				converterStream.Write_L_UINT16((ushort)packedPatternData.Count);
				converterStream.Write_L_UINT16(numberOfRows);
				converterStream.Write_L_UINT32(0);		// Padding
				converterStream.Write(packedPatternData.ToArray());
			}

			return offsets;
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
							if (result.VolCmd == 0)
							{
								for (int i = 0; i < 10; i++)
								{
									if (portaVolCmd[i] == effect.Item2)
									{
										result.VolCmd = (byte)(193 + i + 1);
										break;
									}
								}

								if (result.VolCmd != 0)
									break;
							}

							result.Effect = 0x07;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.Vibrato:
						{
							if ((result.VolCmd == 0) && (effect.Item2 < 10))
								result.VolCmd = (byte)(203 + effect.Item2 + 1);
							else
							{
								result.Effect = 0x08;
								result.EffectVal = effect.Item2;
							}
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

						case Effect.ChannelVolume:
						{
							result.Effect = 0x0d;
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
							if (result.VolCmd == 0)
							{
								if (effect.Item2 == 0xff)
								{
									result.VolCmd = 128 + 64 + 1;
									break;
								}

								if ((effect.Item2 & 0x03) == 0)
								{
									result.VolCmd = (byte)(128 + (effect.Item2 / 4) + 1);
									break;
								}
							}

							result.Effect = 0x18;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.Panbrello:
						{
							result.Effect = 0x19;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.Midi:
						{
							result.Effect = 0x1a;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.XParam:
						{
							result.Effect = 0x1b;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.SmoothMidi:
						{
							result.Effect = 0x1c;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.DelayCut:
						{
							result.Effect = 0x1d;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.FineTune:
						{
							result.Effect = 0x1e;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.FineTuneSmooth:
						{
							result.Effect = 0x1f;
							result.EffectVal = effect.Item2;
							break;
						}

						case Effect.Volume:
						{
							if ((result.VolCmd == 0) && (effect.Item2 <= 64))
								result.VolCmd = (byte)(effect.Item2 + 1);	// Increment by one so I can detect if it has been set. Will be decremented again later
							else
							{
								result.Effect = 0x0d;
								result.EffectVal = effect.Item2;
							}
							break;
						}

						case Effect.Vol_VolSlide2:
						{
							byte vol = (byte)((effect.Item2 % 10) + 1);

							if (effect.Item2 < 10)
								vol += 65;
							else if (effect.Item2 < 20)
								vol += 75;
							else if (effect.Item2 < 30)
								vol += 85;
							else if (effect.Item2 < 40)
								vol += 95;

							result.VolCmd = vol;
							break;
						}

						case Effect.Vol_PortaDown:
						{
							result.VolCmd = (byte)(105 + effect.Item2 + 1);
							break;
						}

						case Effect.Vol_PortaUp:
						{
							result.VolCmd = (byte)(115 + effect.Item2 + 1);
							break;
						}

						case Effect.Vol_ItOther:
						{
							result.VolCmd = (byte)(effect.Item2 + 1);	// Increment by one so I can detect if it has been set. Will be decremented again later
							break;
						}

						default:
						{
							throw new NotImplementedException($"Effect {effect.Item1} not implemented for IT modules");
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
		private List<uint> WriteSampleData(Mo3Module module, ModuleStream moduleStream, ConverterStream converterStream)
		{
			List<uint> offsets = new List<uint>();

			DecodeSampleInfo[] decodeSampleInfo = Mo3SampleWriter.PrepareSamples(module, moduleStream);
			if (decodeSampleInfo == null)
				return null;

			foreach (DecodeSampleInfo sampleInfo in decodeSampleInfo)
			{
				offsets.Add((uint)converterStream.Position);

				if (sampleInfo.SampleData != null)
					converterStream.Write(sampleInfo.SampleData, 0, sampleInfo.SampleData.Length);
			}

			return offsets;
		}



		/********************************************************************/
		/// <summary>
		/// Will write all the offsets back
		/// </summary>
		/********************************************************************/
		private void WriteOffsets(ConverterStream converterStream, long offsetsPosition, List<uint> instrumentInfoOffsets, List<uint> sampleInfoOffsets, List<uint> patternOffsets, List<uint> sampleDataOffsets)
		{
			converterStream.Seek(offsetsPosition, SeekOrigin.Begin);

			converterStream.WriteArray_L_UINT32s(instrumentInfoOffsets, instrumentInfoOffsets.Count);
			converterStream.WriteArray_L_UINT32s(sampleInfoOffsets, sampleInfoOffsets.Count);
			converterStream.WriteArray_L_UINT32s(patternOffsets, patternOffsets.Count);

			for (int i = 0; i < sampleInfoOffsets.Count; i++)
			{
				converterStream.Seek(sampleInfoOffsets[i] + 0x48, SeekOrigin.Begin);
				converterStream.Write_L_UINT32(sampleDataOffsets[i]);
			}
		}
		#endregion
	}
}
