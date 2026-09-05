/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Helper functions for Module Instrument handling
	/// </summary>
	internal class ModInstrument : ICopyTo<ModInstrument>
	{
		/// <summary>
		/// Instrument fadeout speed
		/// </summary>
		public uint32 nFadeOut = 256;

		/// <summary>
		/// Global volume (0...64, all sample volumes are multiplied with this - TODO: This is 0...128 in Impulse Tracker)
		/// </summary>
		public uint32 nGlobalVol = 64;

		/// <summary>
		/// Default pan (0...256), if the appropriate flag is set. Sample panning overrides instrument panning
		/// </summary>
		public uint32 nPan = 32 * 4;

		/// <summary>
		/// Default sample ramping up, 0 = use global default
		/// </summary>
		public uint16 nVolRampUp = 0;

		/// <summary>
		/// Resampling mode
		/// </summary>
		public ResamplingMode Resampling = ResamplingMode.Default;

		/// <summary>
		/// Instrument flags
		/// </summary>
		public InstrumentFlags dwFlags;

		/// <summary>
		/// New note action
		/// </summary>
		public NewNoteAction nNna = NewNoteAction.NoteCut;

		/// <summary>
		/// Duplicate check type (i.e. which condition will trigger the duplicate note action)
		/// </summary>
		public DuplicateCheckType nDct = DuplicateCheckType.None;

		/// <summary>
		/// Duplicate note action
		/// </summary>
		public DuplicateNoteAction nDna = DuplicateNoteAction.NoteCut;

		/// <summary>
		/// Random panning factor (0...64)
		/// </summary>
		public uint8 nPanSwing = 0;

		/// <summary>
		/// Random volume factor (0...100)
		/// </summary>
		public uint8 nVolSwing = 0;

		/// <summary>
		/// Default filter cutoff (0...127). Used if the high bit is set
		/// </summary>
		public uint8 nIfc = 0;

		/// <summary>
		/// Default filter resonance (0...127). Used if the high bit is set
		/// </summary>
		public uint8 nIfr = 0;

		/// <summary>
		/// Random cutoff factor (0...64)
		/// </summary>
		public uint8 nCutSwing = 0;

		/// <summary>
		/// Random resonance factor (0...64)
		/// </summary>
		public uint8 nResSwing = 0;

		/// <summary>
		/// Default filter mode
		/// </summary>
		public FilterMode FilterMode = FilterMode.Unchanged;

		/// <summary>
		/// 
		/// </summary>
		public int8 nPps = 0;

		/// <summary>
		/// 
		/// </summary>
		public uint8 nPpc = ModCommand.Note_MiddleC - ModCommand.Note_Min;

		/// <summary>
		/// MIDI Bank (1...16384). 0 = Don't send
		/// </summary>
		public uint16 wMidiBank = 0;

		/// <summary>
		/// MIDI Program (1...128). 0 = Don't send
		/// </summary>
		public uint8 nMidiProgram = 0;

		/// <summary>
		/// MIDI Channel (1...16). 0 = Don't send. 17 = Mapped (Send to tracker channel modulo 16)
		/// </summary>
		public uint8 nMidiChannel = 0;

		/// <summary>
		/// Drum set note mapping (currently only used by the .MID loader)
		/// </summary>
		public uint8 nMidiDrumKey = 0;

		/// <summary>
		/// MIDI Pitch Wheel Depth and CMD_FINETUNE depth in semitones
		/// </summary>
		public int8 MidiPwd = 2;

		/// <summary>
		/// Plugin assigned to this instrument (0 = no plugin, 1 = first plugin)
		/// </summary>
		public PlugIndex nMixPlug = 0;

		/// <summary>
		/// How to deal with plugin velocity
		/// </summary>
		public PlugVelocityHandling PluginVelocityHandling = PlugVelocityHandling.Channel;

		/// <summary>
		/// How to deal with plugin volume
		/// </summary>
		public PlugVolumeHandling PluginVolumeHandling = PlugVolumeHandling.Ignore;

		/// <summary>
		/// BPM at which the samples assigned to this instrument loop correctly (0 = unset)
		/// </summary>
		public Tempo PitchToTempoLock = new Tempo();

		/// <summary>
		/// Sample tuning assigned to this instrument
		/// </summary>
		public CTuning pTuning = null;

		/// <summary>
		/// Synth scripts for this instrument
		/// </summary>
		public readonly InstrumentSynth Synth = new InstrumentSynth();

		/// <summary>
		/// Volume envelope data
		/// </summary>
		public readonly InstrumentEnvelope VolEnv = new InstrumentEnvelope();

		/// <summary>
		/// Panning envelope data
		/// </summary>
		public readonly InstrumentEnvelope PanEnv = new InstrumentEnvelope();

		/// <summary>
		/// Pitch / filter envelope data
		/// </summary>
		public readonly InstrumentEnvelope PitchEnv = new InstrumentEnvelope();

		/// <summary>
		/// Note mapping, e.g. C-5 => D-5
		/// </summary>
		public readonly array<uint8> NoteMap = new array<uint8>(128);

		/// <summary>
		/// Sample mapping, e.g. C-5 => Sample 1
		/// </summary>
		public readonly array<SampleIndex> Keyboard = new array<SampleIndex>(128);

		/// <summary>
		/// 
		/// </summary>
		public readonly CharBuf Name = new CharBuf(Snd_Def.Max_InstrumentName);

		/// <summary>
		/// 
		/// </summary>
		public readonly CharBuf FileName = new CharBuf(Snd_Def.Max_InstrumentFileName);

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModInstrument() : this(0)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModInstrument(SampleIndex sample)
		{
			for (size_t i = 0; i < NoteMap.size(); i++)
				NoteMap[i] = (uint8)(ModCommand.Note_Min + i);

			Keyboard.fill(sample);
		}



		/********************************************************************/
		/// <summary>
		/// Assign all notes to a given sample
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AssignSample(SampleIndex sample)
		{
			Keyboard.fill(sample);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsCutOffEnabled()
		{
			return (nIfc & 0x80) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsResonanceEnabled()
		{
			return (nIfr & 0x80) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint8 GetCutOff()
		{
			return (uint8)(nIfc & 0x7f);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint8 GetResonance()
		{
			return (uint8)(nIfr & 0x7f);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasValidMidiChannel()
		{
			return (nMidiChannel >= 1) && (nMidiChannel <= 17);
		}



		/********************************************************************/
		/// <summary>
		/// Get a reference to a specific envelope of this instrument
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public InstrumentEnvelope GetEnvelope(EnvelopeType envType)
		{
			switch (envType)
			{
				case EnvelopeType.Volume:
				default:
					return VolEnv;

				case EnvelopeType.Panning:
					return PanEnv;

				case EnvelopeType.Pitch:
					return PitchEnv;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Sanitize(ModType modType)//XX 266
		{
			OpenMpt.LimitMax(ref nFadeOut, 65536U);
			OpenMpt.LimitMax(ref nGlobalVol, 64U);
			OpenMpt.LimitMax(ref nPan, 256U);

			OpenMpt.LimitMax(ref wMidiBank, (uint16)16384);
			OpenMpt.LimitMax(ref nMidiProgram, (uint8)128);
			OpenMpt.LimitMax(ref nMidiChannel, (uint8)17);

			if (nNna > NewNoteAction.NoteFade)
				nNna = NewNoteAction.NoteCut;

			if (nDct > DuplicateCheckType.Plugin)
				nDct = DuplicateCheckType.None;

			if (nDna > DuplicateNoteAction.NoteFade)
				nDna = DuplicateNoteAction.NoteCut;

			OpenMpt.LimitMax(ref nPanSwing, (uint8)64);
			OpenMpt.LimitMax(ref nVolSwing, (uint8)100);

			OpenMpt.Limit(ref nPps, (int8)(-32), (int8)32);

			OpenMpt.LimitMax(ref nCutSwing, (uint8)64);
			OpenMpt.LimitMax(ref nResSwing, (uint8)64);

			uint8 range = modType == ModType.Ams ? uint8.MaxValue : Snd_Def.Envelope_Max;

			VolEnv.Sanitize();
			PanEnv.Sanitize();
			PitchEnv.Sanitize(range);
			Synth.Sanitize();

			for (size_t i = 0; i < NoteMap.size(); i++)
			{
				if ((NoteMap[i] < ModCommand.Note_Min) || (NoteMap[i] > ModCommand.Note_Max))
					NoteMap[i] = (uint8)(i + ModCommand.Note_Min);
			}

			if (!SoundLib.Resampling.IsKnownMode(Resampling))
				Resampling = ResamplingMode.Default;

			if (nMixPlug > Snd_Def.Max_MixPlugins)
				nMixPlug = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint8 GetMidiChannel(ModChannel channel, ChannelIndex chn)//XX 351
		{
			// For mapped channels, return their pattern channel, modulo 16 (because there are only 16 MIDI channels)
			if (nMidiChannel == Snd_Def.MidiMappedChannel)
				return (uint8)((channel.nMasterChn != 0 ? (channel.nMasterChn - 1U) : chn) % 16U);
			else if (HasValidMidiChannel())
				return (uint8)((nMidiChannel - Snd_Def.MidiFirstChannel) % 16U);
			else
				return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object into another object
		/// </summary>
		/********************************************************************/
		public void CopyTo(ModInstrument destination)
		{
			destination.nFadeOut = nFadeOut;
			destination.nGlobalVol = nGlobalVol;
			destination.nPan = nPan;
			destination.nVolRampUp = nVolRampUp;
			destination.Resampling = Resampling;
			destination.dwFlags = dwFlags;
			destination.nNna = nNna;
			destination.nDct = nDct;
			destination.nDna = nDna;
			destination.nPanSwing = nPanSwing;
			destination.nVolSwing = nVolSwing;
			destination.nIfc = nIfc;
			destination.nIfr = nIfr;
			destination.nCutSwing = nCutSwing;
			destination.nResSwing = nResSwing;
			destination.FilterMode = FilterMode;
			destination.nPps = nPps;
			destination.nPpc = nPpc;
			destination.wMidiBank = wMidiBank;
			destination.nMidiProgram = nMidiProgram;
			destination.nMidiChannel = nMidiChannel;
			destination.nMidiDrumKey = nMidiDrumKey;
			destination.MidiPwd = MidiPwd;
			destination.nMixPlug = nMixPlug;
			destination.PluginVelocityHandling = PluginVelocityHandling;
			destination.PluginVolumeHandling = PluginVolumeHandling;
			destination.PitchToTempoLock = PitchToTempoLock;
			destination.pTuning = pTuning;

			Synth.CopyTo(destination.Synth);
			VolEnv.CopyTo(destination.VolEnv);
			PanEnv.CopyTo(destination.PanEnv);
			PitchEnv.CopyTo(destination.PitchEnv);
			NoteMap.CopyTo(destination.NoteMap);
			Keyboard.CopyTo(destination.Keyboard);

			destination.Name.Assign(Name);
			destination.FileName.Assign(FileName);
		}
	}
}
