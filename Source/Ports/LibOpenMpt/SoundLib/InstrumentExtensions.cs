/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.FileFormat_Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Instrument properties I/O
	/// Welcome to the absolutely horrible abominations that are the "extended instrument properties"
	/// which are some of the earliest additions OpenMPT did to the IT / XM format. They are ugly,
	/// and the way they work even differs between IT/XM/ITI/XI and ITI/XI/ITP.
	/// Yes, the world would be a better place without this stuff
	/// </summary>
	internal partial class CSoundFile
	{
		/********************************************************************/
		/// <summary>
		/// Convert instrument flags which were read from 'dF..' extension to
		/// proper internal representation
		/// </summary>
		/********************************************************************/
		public static void ConvertInstrumentFlags(ModInstrument ins, uint32 flags)//XX 394
		{
			ins.VolEnv.dwFlags.Set(EnvelopeFlags.Enabled, (flags & 0x0001) != 0);
			ins.VolEnv.dwFlags.Set(EnvelopeFlags.Sustain, (flags & 0x0002) != 0);
			ins.VolEnv.dwFlags.Set(EnvelopeFlags.Loop, (flags & 0x0004) != 0);
			ins.VolEnv.dwFlags.Set(EnvelopeFlags.Carry, (flags & 0x0800) != 0);

			ins.PanEnv.dwFlags.Set(EnvelopeFlags.Enabled, (flags & 0x0008) != 0);
			ins.PanEnv.dwFlags.Set(EnvelopeFlags.Sustain, (flags & 0x0010) != 0);
			ins.PanEnv.dwFlags.Set(EnvelopeFlags.Loop, (flags & 0x0020) != 0);
			ins.PanEnv.dwFlags.Set(EnvelopeFlags.Carry, (flags & 0x1000) != 0);

			ins.PitchEnv.dwFlags.Set(EnvelopeFlags.Enabled, (flags & 0x0040) != 0);
			ins.PitchEnv.dwFlags.Set(EnvelopeFlags.Sustain, (flags & 0x0080) != 0);
			ins.PitchEnv.dwFlags.Set(EnvelopeFlags.Loop, (flags & 0x0100) != 0);
			ins.PitchEnv.dwFlags.Set(EnvelopeFlags.Carry, (flags & 0x2000) != 0);
			ins.PitchEnv.dwFlags.Set(EnvelopeFlags.Filter, (flags & 0x0400) != 0);

			ins.dwFlags.Set(InstrumentFlags.SetPanning, (flags & 0x0200) != 0);
			ins.dwFlags.Set(InstrumentFlags.Mute, (flags & 0x4000) != 0);
		}



		/********************************************************************/
		/// <summary>
		/// Convert VFLG / PFLG / AFLG
		/// </summary>
		/********************************************************************/
		public static void ConvertEnvelopeFlags(ModInstrument instr, uint32 flags, EnvelopeType envType)//XX 418
		{
			InstrumentEnvelope env = instr.GetEnvelope(envType);

			env.dwFlags.Set(EnvelopeFlags.Enabled, (flags & 0x01) != 0);
			env.dwFlags.Set(EnvelopeFlags.Loop, (flags & 0x02) != 0);
			env.dwFlags.Set(EnvelopeFlags.Sustain, (flags & 0x04) != 0);
			env.dwFlags.Set(EnvelopeFlags.Carry, (flags & 0x08) != 0);
			env.dwFlags.Set(EnvelopeFlags.Filter, (envType == EnvelopeType.Pitch) && ((flags & 0x10) != 0));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void ReadInstrumentHeaderField(ModInstrument ins, uint32 fCode, FileReader file)//XX 429
		{
			size_t size = file.GetLength();

			// Note: Various int / enum members have changed their size over the past.
			// Hence we use ReadSizedIntLE everywhere to allow reading both truncated and oversized values
			void ReadInt<T>(FileReader file_, size_t size_, out T member)
			{
				member = file_.ReadSizedIntLE<T>(size_);
			}

			void ReadEnum<T>(FileReader file_, size_t size_, out T member) where T : Enum
			{
				member = file_.ReadSizedIntLE<T>(size_);
			}

			void ReadEnvelopeTicks(FileReader file_, size_t size_, InstrumentEnvelope env)
			{
				uint32 points = Math.Min((uint32)env.size(), (uint32)(size_ / 2));

				for (uint32 i = 0; i < points; i++)
					env[i].Tick = file_.ReadUInt16LE();
			}

			void ReadEnvelopeValues(FileReader file_, size_t size_, InstrumentEnvelope env)
			{
				uint32 points = (uint32)Math.Min(env.size(), size_);

				for (uint32 i = 0; i < points; i++)
					env[i].Value = file_.ReadUInt8();
			}

			// Members which can be found in this table but not in the write table are only required in the legacy ITP format
			switch (Magic.MagicToStringBE(fCode))
			{
				case "FO..":
				{
					ReadInt(file, size, out ins.nFadeOut);
					break;
				}

				case "GV..":
				{
					ReadInt(file, size, out ins.nGlobalVol);
					break;
				}

				case "P...":
				{
					ReadInt(file, size, out ins.nPan);
					break;
				}

				case "VLS.":
				{
					ReadInt(file, size, out ins.VolEnv.nLoopStart);
					break;
				}

				case "VLE.":
				{
					ReadInt(file, size, out ins.VolEnv.nLoopEnd);
					break;
				}

				case "VSB.":
				{
					ReadInt(file, size, out ins.VolEnv.nSustainStart);
					break;
				}

				case "VSE.":
				{
					ReadInt(file, size, out ins.VolEnv.nSustainEnd);
					break;
				}

				case "PLS.":
				{
					ReadInt(file, size, out ins.PanEnv.nLoopStart);
					break;
				}

				case "PLE.":
				{
					ReadInt(file, size, out ins.PanEnv.nLoopEnd);
					break;
				}

				case "PSB.":
				{
					ReadInt(file, size, out ins.PanEnv.nSustainStart);
					break;
				}

				case "PSE.":
				{
					ReadInt(file, size, out ins.PanEnv.nSustainEnd);
					break;
				}

				case "PiLS":
				{
					ReadInt(file, size, out ins.PitchEnv.nLoopStart);
					break;
				}

				case "PiLE":
				{
					ReadInt(file, size, out ins.PitchEnv.nLoopEnd);
					break;
				}

				case "PiSB":
				{
					ReadInt(file, size, out ins.PitchEnv.nSustainStart);
					break;
				}

				case "PiSE":
				{
					ReadInt(file, size, out ins.PitchEnv.nSustainEnd);
					break;
				}

				case "NNA.":
				{
					ReadEnum(file, size, out ins.nNna);
					break;
				}

				case "DCT.":
				{
					ReadEnum(file, size, out ins.nDct);
					break;
				}

				case "DNA.":
				{
					ReadEnum(file, size, out ins.nDna);
					break;
				}

				case "PS..":
				{
					ReadInt(file, size, out ins.nPanSwing);
					break;
				}

				case "VS..":
				{
					ReadInt(file, size, out ins.nVolSwing);
					break;
				}

				case "IFC.":
				{
					ReadInt(file, size, out ins.nIfc);
					break;
				}

				case "IFR.":
				{
					ReadInt(file, size, out ins.nIfr);
					break;
				}

				case "MB..":
				{
					ReadInt(file, size, out ins.wMidiBank);
					break;
				}

				case "MP..":
				{
					ReadInt(file, size, out ins.nMidiProgram);
					break;
				}

				case "MC..":
				{
					ReadInt(file, size, out ins.nMidiChannel);
					break;
				}

				case "PPS.":
				{
					ReadInt(file, size, out ins.nPps);
					break;
				}

				case "PPC.":
				{
					ReadInt(file, size, out ins.nPpc);
					break;
				}

				case "VP[.":
				{
					ReadEnvelopeTicks(file, size, ins.VolEnv);
					break;
				}

				case "PP[.":
				{
					ReadEnvelopeTicks(file, size, ins.PanEnv);
					break;
				}

				case "PiP[":
				{
					ReadEnvelopeTicks(file, size, ins.PitchEnv);
					break;
				}

				case "VE[.":
				{
					ReadEnvelopeValues(file, size, ins.VolEnv);
					break;
				}

				case "PE[.":
				{
					ReadEnvelopeValues(file, size, ins.PanEnv);
					break;
				}

				case "PiE[":
				{
					ReadEnvelopeValues(file, size, ins.PitchEnv);
					break;
				}

				case "MiP.":
				{
					ReadInt(file, size, out ins.nMixPlug);
					break;
				}

				case "VR..":
				{
					ReadInt(file, size, out ins.nVolRampUp);
					break;
				}

				case "CS..":
				{
					ReadInt(file, size, out ins.nCutSwing);
					break;
				}

				case "RS..":
				{
					ReadInt(file, size, out ins.nResSwing);
					break;
				}

				case "FM..":
				{
					ReadEnum(file, size, out ins.FilterMode);
					break;
				}

				case "PVEH":
				{
					ReadEnum(file, size, out ins.PluginVelocityHandling);
					break;
				}

				case "PVOH":
				{
					ReadEnum(file, size, out ins.PluginVolumeHandling);
					break;
				}

				case "PERN":
				{
					ReadInt(file, size, out ins.PitchEnv.nReleaseNode);
					break;
				}

				case "AERN":
				{
					ReadInt(file, size, out ins.PanEnv.nReleaseNode);
					break;
				}

				case "VERN":
				{
					ReadInt(file, size, out ins.VolEnv.nReleaseNode);
					break;
				}

				case "MPWD":
				{
					ReadInt(file, size, out ins.MidiPwd);
					break;
				}

				case "dF..":
				{
					ConvertInstrumentFlags(ins, file.ReadSizedIntLE<uint32>(size));
					break;
				}

				case "VFLG":
				{
					ConvertEnvelopeFlags(ins, file.ReadSizedIntLE<uint32>(size), EnvelopeType.Volume);
					break;
				}

				case "AFLG":
				{
					ConvertEnvelopeFlags(ins, file.ReadSizedIntLE<uint32>(size), EnvelopeType.Panning);
					break;
				}

				case "PFLG":
				{
					ConvertEnvelopeFlags(ins, file.ReadSizedIntLE<uint32>(size), EnvelopeType.Pitch);
					break;
				}

				case "NM[.":
				{
					for (size_t i = 0; i < Math.Min(size, ins.NoteMap.size()); i++)
						ins.NoteMap[i] = file.ReadUInt8();

					break;
				}

				case "n[..":
				{
					CharBuf name = new CharBuf(32);

					file.ReadString(ReadWriteMode.MaybeNullTerminated, name, size);
					ins.Name.Assign(name);
					break;
				}

				case "fn[.":
				{
					CharBuf fileName = new CharBuf(32);

					file.ReadString(ReadWriteMode.MaybeNullTerminated, fileName, size);
					ins.FileName.Assign(fileName);
					break;
				}

				case "R...":
				{
					// Resampling has been written as various sizes including uint16 and uint32 in the past
					uint32 resampling = file.ReadSizedIntLE<uint32>(size);

					if (Resampling.IsKnownMode((ResamplingMode)resampling))
						ins.Resampling = (ResamplingMode)resampling;

					break;
				}

				case "PTTL":
				{
					// Integer part of pitch/tempo lock
					ins.PitchToTempoLock.Set(file.ReadSizedIntLE<uint16>(size), ins.PitchToTempoLock.GetFract());
					break;
				}

				case "FTTP":	// TNE: This case is in little endian and not big endian as the rest. That's why the string has been reversed
				{
					// Fractional part of pitch/tempo lock
					ins.PitchToTempoLock.Set(ins.PitchToTempoLock.GetInt(), file.ReadSizedIntLE<uint16>(size));
					break;
				}

				case "VE..":
				{
					ins.VolEnv.resize(Math.Min(Snd_Def.Max_EnvPoints, file.ReadSizedIntLE<uint32>(size)));
					break;
				}

				case "PE..":
				{
					ins.PanEnv.resize(Math.Min(Snd_Def.Max_EnvPoints, file.ReadSizedIntLE<uint32>(size)));
					break;
				}

				case "PiE.":
				{
					ins.PitchEnv.resize(Math.Min(Snd_Def.Max_EnvPoints, file.ReadSizedIntLE<uint32>(size)));
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// For ITP and internal usage
		/// </summary>
		/********************************************************************/
		public void ReadExtendedInstrumentProperty(MptSpan<ModInstrument> instruments, uint32 code, FileReader file)//XX 569
		{
			uint16 size = file.ReadUInt16LE();

			foreach (ModInstrument ins in instruments)
			{
				FileReader chunk = new FileReader(file.ReadChunk(size));

				if ((ins != null) && (chunk.GetLength() == size))
					ReadInstrumentHeaderField(ins, code, chunk);
			}
		}



		/********************************************************************/
		/// <summary>
		/// For IT / XM / MO3 / ITI / XI
		/// </summary>
		/********************************************************************/
		public bool LoadExtendedInstrumentProperties(MptSpan<ModInstrument> instruments, FileReader file)//XX 582
		{
			if (!file.ReadMagic("XTPM"))	// 'MPTX'
				return false;

			while (file.CanRead(6))
			{
				uint32 code = file.ReadUInt32LE();

				if ((code == Magic.MagicBE("MPTS")) ||						// Reached song extensions, break out of this loop
					(code == Magic.MagicLE("228\x04")) ||					// Reached MPTM extensions (in case there are no song extensions)
					((code & 0x80808080) != 0) || ((code & 0x60606060) == 0))	// Non-ASCII chunk ID
				{
					file.SkipBack(4);
					break;
				}

				ReadExtendedInstrumentProperty(instruments, code, file);
			}

			return true;
		}
	}
}
