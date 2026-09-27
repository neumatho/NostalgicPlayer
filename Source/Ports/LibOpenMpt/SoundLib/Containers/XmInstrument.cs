/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Endian;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.Arrays;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// XM Instrument Data
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 230)]
	internal struct XmInstrument
	{
		#region XmEnvelopeFlags
		public const uint8 EnvEnabled = 0x01;
		public const uint8 EnvSustain = 0x02;
		public const uint8 EnvLoop = 0x04;
		#endregion

		/// <summary>
		/// Note -> Sample assignment
		/// </summary>
		public Array96 SampleMap;

		/// <summary>
		/// Volume envelope nodes / values (0...64)
		/// </summary>
		public Array24<uint16le> VolEnv;

		/// <summary>
		/// Panning envelope nodes / values (0...63)
		/// </summary>
		public Array24<uint16le> PanEnv;

		/// <summary>
		/// Volume envelope length
		/// </summary>
		public uint8le VolPoints;

		/// <summary>
		/// Panning envelope length
		/// </summary>
		public uint8le PanPoints;

		/// <summary>
		/// Volume envelope sustain point
		/// </summary>
		public uint8le VolSustain;

		/// <summary>
		/// Volume envelope loop start point
		/// </summary>
		public uint8le VolLoopStart;

		/// <summary>
		/// Volume envelope loop end point
		/// </summary>
		public uint8le VolLoopEnd;

		/// <summary>
		/// Panning envelope sustain point
		/// </summary>
		public uint8le PanSustain;

		/// <summary>
		/// Panning envelope loop start point
		/// </summary>
		public uint8le PanLoopStart;

		/// <summary>
		/// Panning envelope loop end point
		/// </summary>
		public uint8le PanLoopEnd;

		/// <summary>
		/// Volume envelope flags
		/// </summary>
		public uint8le VolFlags;

		/// <summary>
		/// Panning envelope flags
		/// </summary>
		public uint8le PanFlags;

		/// <summary>
		/// Sample Auto-Vibrato Type
		/// </summary>
		public uint8le VibType;

		/// <summary>
		/// Sample Auto-Vibrato Sweep
		/// </summary>
		public uint8le VibSweep;

		/// <summary>
		/// Sample Auto-Vibrato Depth
		/// </summary>
		public uint8le VibDepth;

		/// <summary>
		/// Sample Auto-Vibrato Rate
		/// </summary>
		public uint8le VibRate;

		/// <summary>
		/// Volume Fade-Out
		/// </summary>
		public uint16le VolFade;

		/// <summary>
		/// MIDI Out Enabled (0 / 1)
		/// </summary>
		public uint8le MidiEnabled;

		/// <summary>
		/// MIDI Channel (0...15)
		/// </summary>
		public uint8le MidiChannel;

		/// <summary>
		/// MIDI Program (0...127)
		/// </summary>
		public uint16le MidiProgram;

		/// <summary>
		/// MIDI Pitch Wheel Range (0...36 halftones)
		/// </summary>
		public uint16le PitchWheelRange;

		/// <summary>
		/// Mute instrument if MIDI is enabled (0 / 1)
		/// </summary>
		public uint8le MuteComputer;

		/// <summary>
		/// Reserved
		/// </summary>
		public Array15 Reserved;

		/********************************************************************/
		/// <summary>
		/// Convert XM envelope data to an OpenMPT's internal envelope
		/// representation
		/// </summary>
		/********************************************************************/
		public void ConvertEnvelopeToMpt(InstrumentEnvelope mptEnv, uint8 numPoints, uint8 flags, uint8 sustain, uint8 loopStart, uint8 loopEnd, EnvelopeType env)
		{
			mptEnv.resize(Math.Min(numPoints, (uint8)12));

			// Envelope Data
			for (uint32 i = 0; i < mptEnv.size(); i++)
			{
				switch (env)
				{
					case EnvelopeType.Volume:
					{
						mptEnv[i].Tick = VolEnv[(int)i * 2];
						mptEnv[i].Value = (uint8)VolEnv[((int)i * 2) + 1];
						break;
					}

					case EnvelopeType.Panning:
					{
						mptEnv[i].Tick = PanEnv[(int)i * 2];
						mptEnv[i].Value = (uint8)PanEnv[((int)i * 2) + 1];
						break;
					}
				}

				if ((i > 0) && (mptEnv[i].Tick < mptEnv[i - 1].Tick) && ((mptEnv[i].Tick & 0xff00) == 0))
				{
					// libmikmod code says: "Some broken XM editing program will only save the low byte of the position
					// value. Try to compensate by adding the missing high byte."
					// Note: MPT 1.07's XI instrument saver omitted the high byte of envelope nodes.
					// This might be the source for some broken envelopes in IT and XM files
					mptEnv[i].Tick |= (uint16)(mptEnv[i - 1].Tick & 0xff00);

					if (mptEnv[i].Tick < mptEnv[i - 1].Tick)
						mptEnv[i].Tick += 0x100;
				}
			}

			// Envelope Flags
			mptEnv.dwFlags.Reset();

			if (((flags & EnvEnabled) != 0) && !mptEnv.empty())
				mptEnv.dwFlags.Set(EnvelopeFlags.Enabled);

			// Envelope Loops
			if (sustain < 12)
			{
				if ((flags & EnvSustain) != 0)
					mptEnv.dwFlags.Set(EnvelopeFlags.Sustain);

				mptEnv.nSustainStart = mptEnv.nSustainEnd = sustain;
			}

			if ((loopEnd < 12) && (loopEnd >= loopStart))
			{
				if ((flags & EnvLoop) != 0)
					mptEnv.dwFlags.Set(EnvelopeFlags.Loop);

				mptEnv.nLoopStart = loopStart;
				mptEnv.nLoopEnd = loopEnd;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Convert an XMInstrument to OpenMPT's internal instrument
		/// representation
		/// </summary>
		/********************************************************************/
		public void ConvertToMpt(ModInstrument mptIns)
		{
			mptIns.nFadeOut = VolFade;

			// Convert envelopes
			ConvertEnvelopeToMpt(mptIns.VolEnv, VolPoints, VolFlags, VolSustain, VolLoopStart, VolLoopEnd, EnvelopeType.Volume);
			ConvertEnvelopeToMpt(mptIns.PanEnv, PanPoints, PanFlags, PanSustain, PanLoopStart, PanLoopEnd, EnvelopeType.Panning);

			// Create sample assignment table
			array<uint8> sampleMap = SampleMap.ToArray();

			for (size_t i = 0; i < sampleMap.size(); i++)
				mptIns.Keyboard[i + 12] = sampleMap[i];

			if (MidiEnabled != 0)
			{
				mptIns.nMidiChannel = (uint8)(MidiChannel + Snd_Def.MidiFirstChannel);
				OpenMpt.Limit(ref mptIns.nMidiChannel, Snd_Def.MidiFirstChannel, Snd_Def.MidiLastChannel);
				mptIns.nMidiProgram = (uint8)(Math.Min((uint16)MidiProgram, (uint16)127) + 1);
			}

			mptIns.MidiPwd = (int8)(uint8)PitchWheelRange;
		}



		/********************************************************************/
		/// <summary>
		/// Apply auto-vibrato settings from file to a sample
		/// </summary>
		/********************************************************************/
		public void ApplyAutoVibratoToMpt(ModSample mptSmp)
		{
			mptSmp.nVibType = (VibratoType)VibType;
			mptSmp.nVibSweep = VibSweep;
			mptSmp.nVibDepth = VibDepth;
			mptSmp.nVibRate = VibRate;
		}
	}
}
