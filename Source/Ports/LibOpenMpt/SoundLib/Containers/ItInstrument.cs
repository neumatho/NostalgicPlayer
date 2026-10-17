/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Endian;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.Arrays;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.Strings;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Impulse Instrument Format
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 554)]
	internal struct ItInstrument
	{
		#region ItInstrumentFlags
		public const uint8 IgnorePanning = 0x80;
		public const uint8 EnableCutOff = 0x80;
		public const uint8 EnableResonance = 0x80;
		#endregion

		/// <summary>
		/// Magic Bytes (IMPI)
		/// </summary>
		public Array4 Id;

		/// <summary>
		/// DOS Filename, null-terminated
		/// </summary>
		public String13 FileName;

		/// <summary>
		/// New Note Action
		/// </summary>
		public uint8le Nna;

		/// <summary>
		/// Duplicate Note Check Type
		/// </summary>
		public uint8le Dct;

		/// <summary>
		/// Duplicate Note Check Action
		/// </summary>
		public uint8le Dca;

		/// <summary>
		/// Instrument Fadeout (0...256, although values up to 1024 would be sensible. Up to IT2.07, the limit was 0...128)
		/// </summary>
		public uint16le FadeOut;

		/// <summary>
		/// Pitch/Pan Separation
		/// </summary>
		public int8le Pps;

		/// <summary>
		/// Pitch/Pan Centre
		/// </summary>
		public uint8le Ppc;

		/// <summary>
		/// Global Volume
		/// </summary>
		public uint8le Gbv;

		/// <summary>
		/// Panning
		/// </summary>
		public uint8le Dfp;

		/// <summary>
		/// Vol Swing
		/// </summary>
		public uint8le Rv;

		/// <summary>
		/// Pan Swing
		/// </summary>
		public uint8le Rp;

		/// <summary>
		/// Tracker ID
		/// </summary>
		public uint16le TrkVers;

		/// <summary>
		/// Number of embedded samples
		/// </summary>
		public uint8le Nos;

		/// <summary>
		/// Reserved
		/// </summary>
		public uint8 Reserved1;

		/// <summary>
		/// Instrument Name, null-terminated (but may also contain nulls)
		/// </summary>
		public String26 Name;

		/// <summary>
		/// Filter Cutoff
		/// </summary>
		public uint8le Ifc;

		/// <summary>
		/// Filter Resonance
		/// </summary>
		public uint8le Ifr;

		/// <summary>
		/// MIDI Channel
		/// </summary>
		public uint8le Mch;

		/// <summary>
		/// MIDI Program
		/// </summary>
		public uint8le Mpr;

		/// <summary>
		/// MIDI Bank
		/// </summary>
		public Array2 MBank;

		/// <summary>
		/// Sample / Transpose map
		/// </summary>
		public Array240 Keyboard;

		/// <summary>
		/// Volume Envelope
		/// </summary>
		public ItEnvelope VolEnv;

		/// <summary>
		/// Pan Envelope
		/// </summary>
		public ItEnvelope PanEnv;

		/// <summary>
		/// Pitch / Filter Envelope
		/// </summary>
		public ItEnvelope PitchEnv;

		/// <summary>
		/// IT saves some additional padding bytes to match the size of the old instrument format for simplified loading. We use them for some hacks
		/// </summary>
		public Array4 Dummy;

		/********************************************************************/
		/// <summary>
		/// Convert an ITInstrument to OpenMPT's internal instrument
		/// representation. Returns size of the instrument data that has
		/// been read
		/// </summary>
		/********************************************************************/
		public uint32 ConvertToMpt(ModInstrument mptIns, ModType modFormat)
		{
			if (CMemory.memcmp(Id.ToArray().data(), "IMPI", 4) != 0)
				return 0;

			mptIns.Name.Assign(MptString.ReadBuf(ReadWriteMode.SpacePadded, Name.ToArray()));
			mptIns.FileName.Assign(MptString.ReadBuf(ReadWriteMode.NullTerminated, FileName.ToArray()));

			// Volume / Panning
			mptIns.nFadeOut = (uint32)(FadeOut << 5);
			mptIns.nGlobalVol = Gbv / 2U;
			OpenMpt.LimitMax(ref mptIns.nGlobalVol, 64U);
			mptIns.nPan = (uint32)((Dfp & 0x7f) * 4);

			if (mptIns.nPan > 256)
				mptIns.nPan = 128;

			mptIns.dwFlags.Set(InstrumentFlags.SetPanning, (Dfp & IgnorePanning) == 0);

			// Random Variation
			mptIns.nVolSwing = Math.Min(Rv, (uint8)100);
			mptIns.nPanSwing = Math.Min(Rp, (uint8)64);

			// NNA Stuff
			mptIns.nNna = (NewNoteAction)Nna;
			mptIns.nDct = (DuplicateCheckType)Dct;
			mptIns.nDna = (DuplicateNoteAction)Dca;

			// Pitch / Pan Separation
			mptIns.nPps = Pps;
			mptIns.nPpc = Ppc;

			// Filter Stuff
			mptIns.SetCutOff((uint8)(Ifc & 0x7f), (Ifc & EnableCutOff) != 0);
			mptIns.SetResonance((uint8)(Ifr & 0x7f), (Ifr & EnableResonance) != 0);

			// MIDI Setup

			// MPT used to have a slightly different encoding of MIDI program and banks which we are trying to fix here.
			// Impulse Tracker / Schism Tracker will set trkvers to 0 in IT files,
			// and we won't care about correctly importing MIDI programs and banks in ITI files.
			// Chibi Tracker sets trkvers to 0x214, but always writes mpr=mbank=0 anyway.
			// Old BeRoTracker versions set trkvers to 0x214 or 0x217.
			//          <= MPT 1.07              <= MPT 1.16            OpenMPT 1.17-?           <= OpenMPT 1.26         definitely not MPT
			if (((TrkVers == 0x0202) || (TrkVers == 0x0211) || (TrkVers == 0x0220) || (TrkVers == 0x0214)) && (Mpr != 0xff))
			{
				if (Mpr <= 128)
					mptIns.nMidiProgram = Mpr;

				array<uint8> mbank = MBank.ToArray();
				uint16 bank = (uint16)(mbank[0] | (mbank[1] << 8));

				// These versions also ignored the high bank nibble (was only handled correctly in OpenMPT instrument extensions)
				if (bank <= 128)
					mptIns.wMidiBank = bank;
			}
			else
			{
				if (Mpr < 128)
					mptIns.nMidiProgram = (uint8)(Mpr + 1);

				array<uint8> mbank = MBank.ToArray();
				uint16 bank = 0;

				if (mbank[0] < 128)
					bank = (uint16)(mbank[0] + 1);

				if (mbank[1] < 128)
					bank += (uint16)(mbank[1] << 7);

				mptIns.wMidiBank = bank;
			}

			mptIns.nMidiChannel = Mch;

			if (mptIns.nMidiChannel >= 128)
			{
				// Handle old format where MIDI channel and Plugin index are stored in the same variable
				mptIns.nMixPlug = (uint8)(mptIns.nMidiChannel - 128);
				mptIns.nMidiChannel = 0;
			}

			// Envelope point count. Limited to 25 in IT format
			uint8 maxNodes = (uint8)((modFormat & ModType.Mpt) != 0 ? Snd_Def.Max_EnvPoints : 25);

			// Volume Envelope
			VolEnv.ConvertToMpt(mptIns.VolEnv, 0, maxNodes);

			// Panning Envelope
			PanEnv.ConvertToMpt(mptIns.PanEnv, 32, maxNodes);

			// Pitch Envelope
			PitchEnv.ConvertToMpt(mptIns.PitchEnv, 32, maxNodes);
			mptIns.PitchEnv.dwFlags.Set(EnvelopeFlags.Filter, (PitchEnv.Flags & ItEnvelope.EnvFilter) != 0);

			// Sample Map
			array<uint8> keyboard = Keyboard.ToArray();
			for (c_int i = 0; i < 120; i++)
			{
				uint8 note = keyboard[i * 2];
				SampleIndex ins = keyboard[(i * 2) + 1];

				if (ins < Snd_Def.Max_Samples)
					mptIns.Keyboard[i] = ins;

				if (note < 120)
					mptIns.NoteMap[i] = (uint8)(note + ModCommand.Note_Min);
				else
					mptIns.NoteMap[i] = (uint8)(i + ModCommand.Note_Min);
			}

			return (uint32)Marshal.SizeOf<ItInstrument>();
		}
	}
}
