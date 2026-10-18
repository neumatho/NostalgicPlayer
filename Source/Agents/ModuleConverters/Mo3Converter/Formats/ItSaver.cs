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
	/// Will save an IT file
	/// </summary>
	internal class ItSaver : IFormatSaver
	{
		private static readonly byte[] portaVolCmd =
		[
			0x00, 0x01, 0x04, 0x08, 0x10, 0x20, 0x40, 0x60, 0x80, 0xff
		];

		/// <summary>
		/// Holds an envelope in OpenMPT's internal format (values 0-64)
		/// </summary>
		private class WorkEnvelope
		{
			public EnvelopeFlag Flags;
			public int SustainStart;
			public int SustainEnd;
			public int LoopStart;
			public int LoopEnd;
			public List<(int Tick, int Value)> Nodes = new List<(int Tick, int Value)>();
		}

		#region IFormatSaver implementation
		/********************************************************************/
		/// <summary>
		/// Save the module into IT format
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

			DecodeSampleInfo[] decodeSampleInfo = Mo3SampleWriter.PrepareSamples(module, moduleStream);
			if (decodeSampleInfo == null)
			{
				errorMessage = Resources.IDS_ERR_LOADING_SAMPLES;
				return false;
			}

			WriteHeader(module, converterStream);
			WriteChannelPannings(module, converterStream);
			WriteChannelVolumes(module, converterStream);
			WritePositionList(module, converterStream);
			long offsetsPosition = ReserveSpaceForOffsets(module, converterStream);
			WriteEditHistory(converterStream);
			WriteMidiConfig(module, converterStream);
			OpenMptExtensions.WriteOmptHead(module, converterStream);
			OpenMptExtensions.WritePlugins(module, converterStream);
			WriteComment(module, converterStream);

			List<uint> instrumentInfoOffsets = WriteInstrumentInfo(module, converterStream);
			List<uint> sampleInfoOffsets = WriteSampleInfo(module, converterStream);
			List<uint> patternOffsets = WritePatterns(module, converterStream);

			List<uint> sampleDataOffsets = WriteSampleData(converterStream, decodeSampleInfo);
			if (sampleDataOffsets == null)
			{
				errorMessage = Resources.IDS_ERR_LOADING_SAMPLES;
				return false;
			}

			// Instrument and song extensions are stored after the last sample
			OpenMptExtensions.WriteOmptTail(module, converterStream);

			WriteOffsets(converterStream, offsetsPosition, instrumentInfoOffsets, sampleInfoOffsets, patternOffsets, sampleDataOffsets);

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Check if the module is in OpenMPT format
		/// </summary>
		/********************************************************************/
		private bool IsOpenMpt(Mo3Module module)
		{
			// OpenMPT uses this version range for MPTM files
			VersChunk versChunk = module.FindChunk<VersChunk>();

			return (versChunk != null) && (module.FindChunk<OmptChunk>() != null) && (versChunk.Cwtv > 0x0889) && (versChunk.Cwtv <= 0x08ff);
		}



		/********************************************************************/
		/// <summary>
		/// Write back the header
		/// </summary>
		/********************************************************************/
		private void WriteHeader(Mo3Module module, ConverterStream converterStream)
		{
			FileHeader header = module.Header;

			converterStream.WriteMark("IMPM");
			converterStream.WriteString(header.SongName, 26);

			PrhiChunk prhiChunk = module.FindChunk<PrhiChunk>();
			if (prhiChunk != null)
			{
				converterStream.Write_UINT8(prhiChunk.RowsPerBeat);
				converterStream.Write_UINT8(prhiChunk.RowsPerMeasure);
			}
			else
				converterStream.Write_L_UINT16(0);

			converterStream.Write_L_UINT16((ushort)(header.NumOrders + 1));		// Reserved space for end marker
			converterStream.Write_L_UINT16(header.NumInstruments);
			converterStream.Write_L_UINT16(header.NumSamples);
			converterStream.Write_L_UINT16(header.NumPatterns);

			// Without a version, use the same as the original UNMO3
			ushort cwtv = 0x0214;
			ushort cmwt = 0x0214;

			VersChunk versChunk = module.FindChunk<VersChunk>();
			if (versChunk != null)
			{
				cwtv = versChunk.Cwtv;
				cmwt = versChunk.Cmwt;
			}

			// Instruments are always written in the new format
			if (cmwt < 0x0200)
				cmwt = 0x0200;

			// Stereo samples are ignored by players in files made with older versions
			if ((cwtv < 0x0214) && module.Samples.Any(x => (x.Flags & SampleInfoFlag.Stereo) != 0))
				cwtv = 0x0214;

			converterStream.Write_L_UINT16(cwtv);
			converterStream.Write_L_UINT16(cmwt);

			ushort flags = 0x0001;		// Always enable stereo

			if ((header.Flags & HeaderFlag.InstrumentMode) != 0)
				flags |= 0x0004;

			if ((header.Flags & HeaderFlag.LinearSlides) != 0)
				flags |= 0x0008;

			if ((header.Flags & HeaderFlag.ItOldFx) == 0)
				flags |= 0x0010;

			if ((header.Flags & HeaderFlag.ItCompactGxx) == 0)
				flags |= 0x0020;

			flags |= 0x0080;			// Request embedded MIDI configuration

			if ((header.Flags & HeaderFlag.ExtFilterRange) != 0)
				flags |= 0x1000;

			converterStream.Write_L_UINT16(flags);

			flags = 0x0002 | 0x0008;	// Edit history and MIDI configuration are always embedded

			if (header.SongMessage.Length > 0)
				flags |= 0x0001;

			if (prhiChunk != null)
				flags |= 0x0004;

			converterStream.Write_L_UINT16(flags);

			converterStream.Write_UINT8(header.GlobalVol);

			int mixVol = header.SampleVolume < 0 ? header.SampleVolume + 52 : (int)Math.Exp(header.SampleVolume * 3.1 / 20.0) + 51;
			converterStream.Write_UINT8((byte)Math.Min(mixVol, 128));

			converterStream.Write_UINT8(header.DefaultSpeed);
			converterStream.Write_UINT8(header.DefaultTempo);
			converterStream.Write_UINT8(header.PanSeparation);
			converterStream.Write_UINT8(FindPitchWheelDepth(module));

			converterStream.Write_L_UINT16((ushort)(header.SongMessage.Length + 1));	// Reserve space for null terminator
			converterStream.Write_L_UINT32(0);		// Message offset. Will be written later
			converterStream.Write_L_UINT32(0);		// Reserved
		}



		/********************************************************************/
		/// <summary>
		/// The pitch wheel depth is global in IT, so take it from the first
		/// instrument that sends MIDI data
		/// </summary>
		/********************************************************************/
		private byte FindPitchWheelDepth(Mo3Module module)
		{
			foreach (Instrument instr in module.Instruments)
			{
				if (GetMidiChannel(instr) != 0)
					return instr.MidiBend;
			}

			// OpenMPT default value
			return 2;
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
					converterStream.Write_UINT8(100);		// Surround
				else if (panning == 255)
					converterStream.Write_UINT8(64);		// 256 -> full right
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
			for (int i = 0; i < module.Header.NumChannels; i++)
				converterStream.Write_UINT8(Math.Min(module.Header.ChnVolume[i], (byte)64));

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

			return currentPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Write an empty edit history, as Impulse Tracker does
		/// </summary>
		/********************************************************************/
		private void WriteEditHistory(ConverterStream converterStream)
		{
			converterStream.Write_L_UINT16(0);
		}



		/********************************************************************/
		/// <summary>
		/// Write the MIDI macro configuration
		/// </summary>
		/********************************************************************/
		private void WriteMidiConfig(Mo3Module module, ConverterStream converterStream)
		{
			byte[] midiConfig = OpenMptExtensions.BuildMidiConfig(module);
			converterStream.Write(midiConfig, 0, midiConfig.Length);
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
		/// Return the MIDI channel in IT format (0 = none, 1-16)
		/// </summary>
		/********************************************************************/
		private byte GetMidiChannel(Instrument instr)
		{
			if (instr.MidiChannel >= 17)
				return 0;

			// IT encoded with MO3 version prior to 2.4.1 use channel 0 as "no channel"
			if (((instr.Flags & InstrumentFlag.PlayOnMidi) != 0) || (instr.MidiChannel > 0))
				return (byte)(instr.MidiChannel + 1);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Write all the instrument information
		/// </summary>
		/********************************************************************/
		private List<uint> WriteInstrumentInfo(Mo3Module module, ConverterStream converterStream)
		{
			List<uint> offsets = new List<uint>();

			// OpenMPT fixes some old ModPlug behaviour, if the version is not known
			bool modPlugFixes = OpenMptExtensions.NeedModPlugBehaviour(module);

			for (int i = 0; i < module.Header.NumInstruments; i++)
			{
				offsets.Add((uint)converterStream.Position);

				Instrument instr = module.Instruments[i];

				converterStream.WriteMark("IMPI");
				converterStream.WriteString(instr.FileName, 12);
				converterStream.Write_UINT8(0);

				converterStream.Write_UINT8(instr.Nna);
				converterStream.Write_UINT8(instr.Dct);
				converterStream.Write_UINT8(instr.Dca);
				converterStream.Write_L_UINT16((ushort)(instr.FadeOut / 32));
				converterStream.Write_UINT8(instr.Pps);
				converterStream.Write_UINT8(instr.Ppc);
				converterStream.Write_UINT8(Math.Min(instr.GlobalVol, (byte)128));

				// The MO3 encoder stores full right as 255, so round to nearest
				if (instr.Panning <= 256)
					converterStream.Write_UINT8((byte)Math.Min((instr.Panning + 2) / 4, 64));
				else
					converterStream.Write_UINT8(0x80 | 32);		// Don't use panning

				int panSwing = Math.Min(instr.PanSwing, (ushort)256) / 4;
				if (modPlugFixes)
				{
					// Fix excessive pan swing range (for files before v1.26)
					panSwing = (panSwing + 3) / 4;
				}

				converterStream.Write_UINT8((byte)Math.Min(instr.VolSwing, (ushort)100));	// Random volume variation
				converterStream.Write_UINT8((byte)panSwing);								// Random panning variation
				converterStream.Write_L_UINT16(0);		// Tracker version (only used in instrument files)
				converterStream.Write_UINT8(0);			// Number of samples associated (only used in instrument files)
				converterStream.Write_UINT8(0);

				converterStream.WriteString(instr.InstrumentName, 26);

				converterStream.Write_UINT8(instr.CutOff);
				converterStream.Write_UINT8(instr.Resonance);

				// MIDI channel. Plugins are stored as 128 + plugin number
				byte midiChannel = GetMidiChannel(instr);

				if (instr.MidiChannel >= 128)
					converterStream.Write_UINT8(instr.MidiChannel < 255 ? (byte)(instr.MidiChannel + 1) : (byte)0);
				else
					converterStream.Write_UINT8(midiChannel);

				// MIDI program and bank are only used if a MIDI channel is set
				if ((midiChannel != 0) && (instr.MidiPatch < 128))
					converterStream.Write_UINT8(instr.MidiPatch);
				else
					converterStream.Write_UINT8(0xff);

				if ((midiChannel != 0) && (instr.MidiBank < 128))
				{
					converterStream.Write_UINT8(instr.MidiBank);
					converterStream.Write_UINT8(0);
				}
				else
				{
					converterStream.Write_UINT8(0xff);
					converterStream.Write_UINT8(0xff);
				}

				for (int j = 0; j < 240; j += 2)
				{
					converterStream.Write_UINT8((byte)instr.SampleMap[j]);

					ushort samp = instr.SampleMap[j + 1];
					converterStream.Write_UINT8(samp >= 255 ? (byte)0 : (byte)(samp + 1));
				}

				WriteEnvelope(ConvertEnvelope(instr.VolEnv, 0), converterStream, 0);
				WriteEnvelope(ConvertEnvelope(instr.PanEnv, 0), converterStream, 32);

				WorkEnvelope pitchEnv = ConvertEnvelope(instr.PitchEnv, 5);
				if (modPlugFixes)
				{
					// Fix pitch / filter envelope being shortened by one tick (for files before v1.20)
					ConvertXmEnvelopeToIt(pitchEnv);
				}

				WriteEnvelope(pitchEnv, converterStream, 32);

				// Some padding
				converterStream.Write_L_UINT32(0);
			}

			return offsets;
		}



		/********************************************************************/
		/// <summary>
		/// Convert a MO3 envelope into OpenMPT's internal representation
		/// </summary>
		/********************************************************************/
		private WorkEnvelope ConvertEnvelope(Envelope env, int shift)
		{
			WorkEnvelope result = new WorkEnvelope
			{
				Flags = env.Flags,
				SustainStart = env.SustainStart,
				SustainEnd = env.SustainEnd,
				LoopStart = env.LoopStart,
				LoopEnd = env.LoopEnd
			};

			int numNodes = Math.Min((int)env.NumNodes, 25);

			for (int i = 0; i < numNodes; i++)
			{
				int tick = env.Points[i * 2];
				if ((i > 0) && (tick < result.Nodes[i - 1].Tick))
					tick = result.Nodes[i - 1].Tick + 1;

				int value = Math.Clamp(env.Points[i * 2 + 1] >> shift, 0, 64);

				result.Nodes.Add((tick, value));
			}

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Do the same as OpenMPT's InstrumentEnvelope::Convert() from XM to
		/// IT
		/// </summary>
		/********************************************************************/
		private void ConvertXmEnvelopeToIt(WorkEnvelope env)
		{
			if ((env.SustainStart > env.LoopEnd) && ((env.Flags & EnvelopeFlag.Loop) != 0))
			{
				// In the IT format, the sustain loop is always considered before the envelope loop.
				// In the XM format, whichever of the two is encountered first is considered.
				// So we have to disable the sustain loop if it was behind the normal loop
				env.Flags &= ~EnvelopeFlag.Sustain;
			}

			if ((env.Flags & (EnvelopeFlag.Loop | EnvelopeFlag.Sustain)) == 0)
			{
				// XM has no automatic fade-out behaviour at the end of the envelope
				env.Flags |= EnvelopeFlag.Sustain;
				env.SustainStart = env.SustainEnd = Math.Max(env.Nodes.Count, 1) - 1;
			}

			// Shorten loop by one tick by inserting bogus point
			if ((env.LoopEnd > env.LoopStart) && ((env.Flags & EnvelopeFlag.Loop) != 0) && (env.LoopEnd < env.Nodes.Count))
			{
				if ((env.Nodes[env.LoopEnd].Tick - 1) > env.Nodes[env.LoopEnd - 1].Tick)
				{
					// Insert an interpolated point just before the loop point
					int tick = env.Nodes[env.LoopEnd].Tick - 1;
					env.Nodes.Insert(env.LoopEnd, (tick, GetValueFromPosition(env, tick)));
				}
				else
				{
					// There is already a point before the loop point: Use it as new loop end
					env.LoopEnd--;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the envelope value at the given position. Same calculation
		/// as InstrumentEnvelope::GetValueFromPosition() in OpenMPT
		/// </summary>
		/********************************************************************/
		private int GetValueFromPosition(WorkEnvelope env, int position)
		{
			const int EnvPrecision = 1 << 16;
			const int RangeIn = 64;
			const int RangeOut = 64;

			if (env.Nodes.Count == 0)
				return 0;

			int lastPoint = env.Nodes.Count - 1;
			int pt = lastPoint;

			for (int i = 0; i < lastPoint; i++)
			{
				if (position <= env.Nodes[i].Tick)
				{
					pt = i;
					break;
				}
			}

			int x2 = env.Nodes[pt].Tick;
			int value = 0;

			if (position >= x2)
				value = env.Nodes[pt].Value * EnvPrecision / RangeIn;
			else
			{
				int x1 = 0;

				if (pt != 0)
				{
					value = env.Nodes[pt - 1].Value * EnvPrecision / RangeIn;
					x1 = env.Nodes[pt - 1].Tick;
				}

				if ((x2 > x1) && (position > x1))
					value += (int)((long)(position - x1) * (env.Nodes[pt].Value * EnvPrecision / RangeIn - value) / (x2 - x1));
			}

			value = Math.Clamp(value, 0, EnvPrecision);

			return (value * RangeOut + EnvPrecision / 2) / EnvPrecision;
		}



		/********************************************************************/
		/// <summary>
		/// Write a single envelope structure
		/// </summary>
		/********************************************************************/
		private void WriteEnvelope(WorkEnvelope env, ConverterStream converterStream, int subtract)
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

			int numNodes = Math.Min(env.Nodes.Count, 25);

			converterStream.Write_UINT8((byte)numNodes);
			converterStream.Write_UINT8((byte)env.LoopStart);
			converterStream.Write_UINT8((byte)env.LoopEnd);
			converterStream.Write_UINT8((byte)env.SustainStart);
			converterStream.Write_UINT8((byte)env.SustainEnd);

			for (int i = 0; i < numNodes; i++)
			{
				converterStream.Write_UINT8((byte)(env.Nodes[i].Value - subtract));
				converterStream.Write_L_UINT16((ushort)env.Nodes[i].Tick);
			}

			if (numNodes < 25)
				converterStream.Write(Enumerable.Repeat<byte>(0, (25 - numNodes) * 3).ToArray());

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

			for (int i = 0; i < module.Header.NumSamples; i++)
			{
				offsets.Add((uint)converterStream.Position);

				Sample sample = module.Samples[i];

				converterStream.WriteMark("IMPS");
				converterStream.WriteString(sample.FileName, 12);

				converterStream.Write_UINT8(0);
				converterStream.Write_UINT8(Math.Min(sample.GlobalVol, (byte)64));

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

				converterStream.Write_UINT8(Math.Min(sample.DefaultVolume, (byte)64));
				converterStream.WriteString(sample.SampleName, 26);
				converterStream.Write_UINT8(0x01);			// Samples are signed

				// Bit 7 set means that the panning should be used. The MO3 encoder
				// stores full right as 255, so round to nearest
				if (sample.Panning <= 256)
					converterStream.Write_UINT8((byte)(Math.Min((sample.Panning + 2) / 4, 64) | 0x80));
				else
					converterStream.Write_UINT8(32);

				converterStream.Write_L_UINT32(sample.Length);
				converterStream.Write_L_UINT32(sample.LoopStart);
				converterStream.Write_L_UINT32(sample.LoopEnd);
				converterStream.Write_L_UINT32(FormatHelper.GetC5Speed(module, sample));
				converterStream.Write_L_UINT32(sample.SustainStart);
				converterStream.Write_L_UINT32(sample.SustainEnd);
				converterStream.Write_L_UINT32(0);			// Sample pointer. Not known yet as this point, will be written back later

				converterStream.Write_UINT8(sample.VibRate);
				converterStream.Write_UINT8(sample.VibDepth);
				converterStream.Write_UINT8(sample.VibSweep);
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

			int numChannels = module.Header.NumChannels;

			byte[] previousMasks = new byte[numChannels];
			bool[] hasPreviousMask = new bool[numChannels];
			byte[] previousNote = new byte[numChannels];
			bool[] hasPreviousNote = new bool[numChannels];
			byte[] previousInstrument = new byte[numChannels];
			byte[] previousVolume = new byte[numChannels];
			bool[] hasPreviousVolume = new bool[numChannels];
			byte[] previousEffect = new byte[numChannels * 2];
			bool[] hasPreviousEffect = new bool[numChannels];

			for (int i = 0; i < module.Header.NumPatterns; i++)
			{
				offsets.Add((uint)converterStream.Position);

				packedPatternData.Clear();
				Array.Clear(hasPreviousMask);
				Array.Clear(hasPreviousNote);
				Array.Clear(previousInstrument);
				Array.Clear(hasPreviousVolume);
				Array.Clear(hasPreviousEffect);

				ushort numberOfRows = module.PatternInfo.RowLengths[i];

				for (int r = 0; r < numberOfRows; r++)
				{
					for (int j = 0; j < numChannels; j++)
					{
						Track trk = module.Tracks[module.PatternInfo.Sequences[i, j]];

						if (r < trk.Rows.Count)
						{
							TrackRow row = trk.Rows[r];

							// If row is empty, skip it
							if ((row.Note == 0) && (row.Instrument == 0) && (row.Effects == null))
								continue;

							var convertedEffect = ConvertEffect(row.Effects);

							bool hasNote = row.Note != 0;
							bool hasInstrument = row.Instrument != 0;
							bool hasVolume = convertedEffect.VolCmd != 0;
							bool hasEffect = (convertedEffect.Effect != 0) || (convertedEffect.EffectVal != 0);

							if (!hasNote && !hasInstrument && !hasVolume && !hasEffect)
								continue;

							byte mask = 0x00;
							bool writeNote = false, writeInstrument = false, writeVolume = false, writeEffect = false;

							// Notes 1-120 are normal notes. Note fade is written the same
							// way as OpenMPT does, since 0xfd is read as no note
							byte note = row.Note <= 120 ? (byte)(row.Note - 1) : row.Note == 0xfd ? (byte)0xf6 : row.Note;
							byte volume = (byte)(convertedEffect.VolCmd - 1);

							if (hasNote)
							{
								if (hasPreviousNote[j] && (note == previousNote[j]))
									mask |= 0x10;
								else
								{
									previousNote[j] = note;
									hasPreviousNote[j] = true;
									mask |= 0x01;
									writeNote = true;
								}
							}

							if (hasInstrument)
							{
								if (row.Instrument == previousInstrument[j])
									mask |= 0x20;
								else
								{
									previousInstrument[j] = row.Instrument;
									mask |= 0x02;
									writeInstrument = true;
								}
							}

							if (hasVolume)
							{
								if (hasPreviousVolume[j] && (volume == previousVolume[j]))
									mask |= 0x40;
								else
								{
									previousVolume[j] = volume;
									hasPreviousVolume[j] = true;
									mask |= 0x04;
									writeVolume = true;
								}
							}

							if (hasEffect)
							{
								if (hasPreviousEffect[j] && (convertedEffect.Effect == previousEffect[j * 2]) && (convertedEffect.EffectVal == previousEffect[j * 2 + 1]))
									mask |= 0x80;
								else
								{
									previousEffect[j * 2] = convertedEffect.Effect;
									previousEffect[j * 2 + 1] = convertedEffect.EffectVal;
									hasPreviousEffect[j] = true;
									mask |= 0x08;
									writeEffect = true;
								}
							}

							byte channel = (byte)(j + 1);

							if (!hasPreviousMask[j] || (mask != previousMasks[j]))
							{
								previousMasks[j] = mask;
								hasPreviousMask[j] = true;
								packedPatternData.Add((byte)(channel | 0x80));
								packedPatternData.Add(mask);
							}
							else
								packedPatternData.Add(channel);

							if (writeNote)
								packedPatternData.Add(note);

							if (writeInstrument)
								packedPatternData.Add(row.Instrument);

							if (writeVolume)
								packedPatternData.Add(volume);

							if (writeEffect)
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
		/// Will convert the effects to IT vol + effect
		/// </summary>
		/********************************************************************/
		private (byte VolCmd, byte Effect, byte EffectVal) ConvertEffect(List<(Effect, byte)> effects)
		{
			(byte VolCmd, byte Effect, byte EffectVal) result = (0, 0, 0);

			// Remember the effect, if an effect has been placed in the volume column,
			// so it can be moved back to the effect column if needed
			(byte Effect, byte EffectVal)? volumeColumnEffect = null;

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
										volumeColumnEffect = (0x07, effect.Item2);
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
							{
								result.VolCmd = (byte)(203 + effect.Item2 + 1);
								volumeColumnEffect = (0x08, effect.Item2);
							}
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
									volumeColumnEffect = (0x18, effect.Item2);
									break;
								}

								if ((effect.Item2 & 0x03) == 0)
								{
									result.VolCmd = (byte)(128 + (effect.Item2 / 4) + 1);
									volumeColumnEffect = (0x18, effect.Item2);
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
							if (effect.Item2 <= 64)
							{
								if (result.VolCmd == 0)
									result.VolCmd = (byte)(effect.Item2 + 1);	// Increment by one so I can detect if it has been set. Will be decremented again later
								else if ((volumeColumnEffect != null) && (result.Effect == 0) && (result.EffectVal == 0))
								{
									// The MO3 encoder stores the effect before the volume, so the
									// effect has been placed in the volume column. IT does not have
									// a volume effect, so move the effect back to the effect column
									result.Effect = volumeColumnEffect.Value.Effect;
									result.EffectVal = volumeColumnEffect.Value.EffectVal;
									result.VolCmd = (byte)(effect.Item2 + 1);

									volumeColumnEffect = null;
								}

								// Otherwise the command is dropped, as OpenMPT does when saving IT files
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
							volumeColumnEffect = null;
							break;
						}

						case Effect.Vol_PortaDown:
						{
							result.VolCmd = (byte)(105 + effect.Item2 + 1);
							volumeColumnEffect = null;
							break;
						}

						case Effect.Vol_PortaUp:
						{
							result.VolCmd = (byte)(115 + effect.Item2 + 1);
							volumeColumnEffect = null;
							break;
						}

						case Effect.Vol_ItOther:
						{
							result.VolCmd = (byte)(effect.Item2 + 1);	// Increment by one so I can detect if it has been set. Will be decremented again later
							volumeColumnEffect = null;
							break;
						}

						// Effects not supported by IT are ignored
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
		private List<uint> WriteSampleData(ConverterStream converterStream, DecodeSampleInfo[] decodeSampleInfo)
		{
			List<uint> offsets = new List<uint>();

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

			converterStream.Seek(0, SeekOrigin.End);
		}
		#endregion
	}
}
