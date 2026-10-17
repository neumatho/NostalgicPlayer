/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpenMpt.Test.Tests
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Tests
	{
		/********************************************************************/
		/// <summary>
		/// Test MPTM file loading
		/// </summary>
		/********************************************************************/
//XX		[TestMethod]
		public void Test_LoadMptm()
		{
			string fileNameBaseSrc = GetTestFileNameBase();
			CSoundFile sndFile = CreateSoundFileContainer(fileNameBaseSrc + "mptm");

			TestLoadMptmFile(sndFile);
		}



		/********************************************************************/
		/// <summary>
		/// Check if our test file was loaded correctly
		/// </summary>
		/********************************************************************/
		private void TestLoadMptmFile(CSoundFile sndFile)
		{
			// Global variables
			Assert.AreEqual("Test Module_____________X", sndFile.GetTitle().ToString());
			Assert.AreEqual("OpenMPT Module Loader Test Suite", sndFile.m_SongMessage.substr(0, 32));
			Assert.AreEqual(128U, sndFile.m_nDefaultGlobalVolume);
			Assert.AreEqual(42U, sndFile.m_nVstiVolume);
			Assert.AreEqual(23U, sndFile.m_nSamplePreAmp);
			Assert.AreEqual(SongFlags.LinearSlides | SongFlags.ExFilterRange | SongFlags.ItCompatGxx | SongFlags.ItOldEffects, sndFile.m_SongFlags);
			Assert.IsTrue(sndFile.m_PlayBehaviour[PlayBehaviour.Msf_Compatible_Play]);
			Assert.IsFalse(sndFile.m_PlayBehaviour[PlayBehaviour.MidiCCBugEmulation]);
			Assert.IsFalse(sndFile.m_PlayBehaviour[PlayBehaviour.MptOldSwingBehaviour]);
			Assert.IsFalse(sndFile.m_PlayBehaviour[PlayBehaviour.OldMidiPitchBends]);
			Assert.AreEqual(MixLevels.Compatible, sndFile.GetMixLevels());
			Assert.AreEqual(TempoMode.Modern, sndFile.m_nTempoMode);
			Assert.AreEqual(6U, sndFile.m_nDefaultRowsPerBeat);
			Assert.AreEqual(12U, sndFile.m_nDefaultRowsPerMeasure);
			Assert.AreEqual(Version.Parse("1.19.02.05"), sndFile.m_dwCreatedWithVersion);
			Assert.AreEqual(ResamplingMode.Sinc8Lp, sndFile.m_nResampling);
			Assert.AreEqual("Tester", sndFile.m_SongArtist);
			Assert.AreEqual(6U, sndFile.m_TempoSwing.size());
			Assert.AreEqual(29360125U, sndFile.m_TempoSwing[0]);
			Assert.AreEqual(4194305U, sndFile.m_TempoSwing[1]);
			Assert.AreEqual(29360128U, sndFile.m_TempoSwing[2]);
			Assert.AreEqual(4194305U, sndFile.m_TempoSwing[3]);
			Assert.AreEqual(29360128U, sndFile.m_TempoSwing[4]);
			Assert.AreEqual(4194305U, sndFile.m_TempoSwing[5]);

			// Edit history
			Assert.IsGreaterThan(15U, sndFile.GetFileHistory().size());

			FileHistory fh = sndFile.GetFileHistory().front();
			Assert.AreEqual(2011, fh.LoadDate.Year);
			Assert.AreEqual(6, fh.LoadDate.Month);
			Assert.AreEqual(14, fh.LoadDate.Day);
			Assert.AreEqual(21, fh.LoadDate.Hour);
			Assert.AreEqual(8, fh.LoadDate.Minute);
			Assert.AreEqual(32, fh.LoadDate.Second);

			Assert.AreEqual(31U, (uint32)(fh.OpenTime / FileHistory.History_Timer_Precision));

			// Macros
			Assert.AreEqual(ParameteredMacro.SFxReso, sndFile.m_MidiCfg.GetParameteredMacroType(0));
			Assert.AreEqual(ParameteredMacro.SFxDryWet, sndFile.m_MidiCfg.GetParameteredMacroType(1));
			Assert.AreEqual(ParameteredMacro.SFxPolyAt, sndFile.m_MidiCfg.GetParameteredMacroType(2));
			Assert.AreEqual(FixedMacro.ZxxResoFltMode, sndFile.m_MidiCfg.GetFixedMacroType());

			// Channels
			Assert.AreEqual(70, sndFile.GetNumChannels());
			Assert.AreEqual("First Channel", sndFile.ChnSettings[0].szName.Str().ToString());
			Assert.AreEqual(32, sndFile.ChnSettings[0].nPan);
			Assert.AreEqual(32, sndFile.ChnSettings[0].nVolume);
			Assert.AreEqual(ChannelFlags.Chn_Mute, sndFile.ChnSettings[0].dwFlags);

			Assert.AreEqual("Second Channel", sndFile.ChnSettings[1].szName.Str().ToString());
			Assert.AreEqual(128, sndFile.ChnSettings[1].nPan);
			Assert.AreEqual(16, sndFile.ChnSettings[1].nVolume);
			Assert.AreEqual(ChannelFlags.Chn_Surround, sndFile.ChnSettings[1].dwFlags);

			Assert.AreEqual("Last Channel______X", sndFile.ChnSettings[69].szName.Str().ToString());
			Assert.AreEqual(256, sndFile.ChnSettings[69].nPan);
			Assert.AreEqual(7, sndFile.ChnSettings[69].nVolume);
			Assert.AreEqual(ChannelFlags.None, sndFile.ChnSettings[69].dwFlags);

			// Samples
			Assert.AreEqual(4, sndFile.GetNumSamples());

			{
				ModSample sample = sndFile.GetSample(1);
				Assert.AreEqual(1, sample.GetBytesPerSample());
				Assert.AreEqual(1, sample.GetNumChannels());
				Assert.AreEqual(1, sample.GetElementarySampleSize());
				Assert.AreEqual(16U, sample.GetSampleSizeInBytes());
				Assert.AreEqual(9001U, sample.GetSampleRate(ModType.Mpt));
				Assert.AreEqual(32 * 4, sample.nVolume);
				Assert.AreEqual(16, sample.nGlobalVol);
				Assert.AreEqual(160, sample.nPan);
				Assert.AreEqual(ChannelFlags.Chn_Panning | ChannelFlags.Chn_Loop | ChannelFlags.Chn_SustainLoop | ChannelFlags.Chn_PingPongSustain, sample.uFlags);

				Assert.AreEqual(1U, sample.nLoopStart);
				Assert.AreEqual(8U, sample.nLoopEnd);
				Assert.AreEqual(1U, sample.nSustainStart);
				Assert.AreEqual(7U, sample.nSustainEnd);

				Assert.AreEqual(VibratoType.Square, sample.nVibType);
				Assert.AreEqual(3, sample.nVibSweep);
				Assert.AreEqual(4, sample.nVibRate);
				Assert.AreEqual(5, sample.nVibDepth);

				// Sample data
				for (size_t i = 0; i < 6; i++)
					Assert.AreEqual(18, sample.Sample8()[i]);

				for (size_t i = 6; i < 16; i++)
					Assert.AreEqual(0, sample.Sample8()[i]);

				Assert.AreEqual(2U, sample.Cues[0]);
				Assert.AreEqual(9U, sample.Cues[8]);
			}

			{
				ModSample sample = sndFile.GetSample(2);
				Assert.AreEqual("Stereo / 16-Bit", sndFile.m_szNames[2].Str().ToString());
				Assert.AreEqual(4, sample.GetBytesPerSample());
				Assert.AreEqual(2, sample.GetNumChannels());
				Assert.AreEqual(2, sample.GetElementarySampleSize());
				Assert.AreEqual(16U * 4, sample.GetSampleSizeInBytes());
				Assert.AreEqual(16000U, sample.GetSampleRate(ModType.Mpt));
				Assert.AreEqual(ChannelFlags.Chn_16Bit | ChannelFlags.Chn_Stereo | ChannelFlags.Chn_Loop, sample.uFlags);

				// Sample data (Stereo Interleaved)
				for (size_t i = 0; i < 7; i++)
					Assert.AreEqual(-32768, sample.Sample16()[4 + i]);

				Assert.AreEqual(3U, sample.Cues[0]);
				Assert.AreEqual(14U, sample.Cues[8]);
			}

			// External sample
			{
				ModSample sample = sndFile.GetSample(4);
				Assert.AreEqual("Overridden Name", sndFile.m_szNames[4].Str().ToString());
				Assert.AreEqual("External", sample.FileName.Str().ToString());
				Assert.AreEqual(10101U, sample.GetSampleRate(ModType.Mpt));
				Assert.AreEqual(26 * 4, sample.nVolume);
				Assert.AreEqual(26, sample.nGlobalVol);
				Assert.AreEqual(26 * 4, sample.nPan);

				Assert.AreEqual(VibratoType.Sine, sample.nVibType);
				Assert.AreEqual(37, sample.nVibSweep);
				Assert.AreEqual(42, sample.nVibRate);
				Assert.AreEqual(23, sample.nVibDepth);

				Assert.AreEqual(10U, sample.Cues[0]);
				Assert.AreEqual(50U, sample.Cues[8]);
			}

			// Instruments
			Assert.AreEqual(2, sndFile.GetNumInstruments());

			for (InstrumentIndex ins = 1; ins <= 2; ins++)
			{
				ModInstrument pIns = sndFile.Instruments[ins];

				Assert.AreEqual(32U, pIns.nGlobalVol);
				Assert.AreEqual(ins == 1 ? 1023U : 8992U, pIns.nFadeOut);
				Assert.AreEqual(63U, pIns.nPan);
				Assert.AreEqual(InstrumentFlags.SetPanning, pIns.dwFlags);

				Assert.AreEqual(8, pIns.nPps);
				Assert.AreEqual((ModCommand.Note_MiddleC - ModCommand.Note_Min) + 6, pIns.nPpc);	// F#5

				Assert.AreEqual(1200, pIns.nVolRampUp);
				Assert.AreEqual(ResamplingMode.Sinc8Lp, pIns.Resampling);

				Assert.IsTrue(pIns.IsCutOffEnabled());
				Assert.AreEqual(0x32, pIns.GetCutOff());
				Assert.IsTrue(pIns.IsResonanceEnabled());
				Assert.AreEqual(0x64, pIns.GetResonance());
				Assert.AreEqual(FilterMode.HighPass, pIns.FilterMode);

				Assert.AreEqual(0x30, pIns.nVolSwing);
				Assert.AreEqual(0x18, pIns.nPanSwing);
				Assert.AreEqual(0x0c, pIns.nCutSwing);
				Assert.AreEqual(0x3c, pIns.nResSwing);

				Assert.AreEqual(NewNoteAction.Continue, pIns.nNna);
				Assert.AreEqual(DuplicateCheckType.Note, pIns.nDct);
				Assert.AreEqual(DuplicateNoteAction.NoteFade, pIns.nDna);

				Assert.AreEqual(1, pIns.nMixPlug);
				Assert.AreEqual(16, pIns.nMidiChannel);
				Assert.AreEqual(64, pIns.nMidiProgram);
				Assert.AreEqual(2, pIns.wMidiBank);
				Assert.AreEqual(-1, pIns.MidiPwd);

				if (ins == 1)
					Assert.IsNull(pIns.pTuning);
				else
					Assert.AreEqual("Test Tuning", pIns.pTuning.GetName());

				Assert.AreEqual(new Tempo(130, 2000), pIns.PitchToTempoLock);

				Assert.AreEqual(PlugVelocityHandling.Volume, pIns.PluginVelocityHandling);
				Assert.AreEqual(PlugVolumeHandling.Midi, pIns.PluginVolumeHandling);

				for (size_t i = 0; i < 120; i++)
				{
					Assert.AreEqual(i == ModCommand.Note_MiddleC - 1 ? (ins * 1111) : 1, pIns.Keyboard[i]);
					Assert.AreEqual(i == ModCommand.Note_MiddleC - 1 ? (i + 13) : (i + 1), pIns.NoteMap[i]);
				}

				Assert.AreEqual(EnvelopeFlags.Enabled | EnvelopeFlags.Carry, pIns.VolEnv.dwFlags);
				Assert.AreEqual(3U, pIns.VolEnv.size());
				Assert.AreEqual(1, pIns.VolEnv.nReleaseNode);
				Assert.AreEqual(96, pIns.VolEnv[2].Tick);
				Assert.AreEqual(0, pIns.VolEnv[2].Value);

				Assert.AreEqual(EnvelopeFlags.Loop | EnvelopeFlags.Sustain, pIns.PanEnv.dwFlags);
				Assert.AreEqual(ins == 1 ? 74U : 76U, pIns.PanEnv.size());
				Assert.AreEqual(26, pIns.PanEnv.nLoopStart);
				Assert.AreEqual(29, pIns.PanEnv.nLoopEnd);
				Assert.AreEqual(27, pIns.PanEnv.nSustainStart);
				Assert.AreEqual(28, pIns.PanEnv.nSustainEnd);
				Assert.AreEqual(Snd_Def.Env_Release_Node_Unset, pIns.PanEnv.nReleaseNode);
				Assert.AreEqual(417, pIns.PanEnv[73].Tick);
				Assert.AreEqual(23, pIns.PanEnv[73].Value);

				if (ins == 2)
				{
					Assert.AreEqual(427, pIns.PanEnv[75].Tick);
					Assert.AreEqual(27, pIns.PanEnv[75].Value);
				}

				Assert.AreEqual(EnvelopeFlags.Enabled | EnvelopeFlags.Carry | EnvelopeFlags.Sustain | EnvelopeFlags.Filter, pIns.PitchEnv.dwFlags);
				Assert.AreEqual(3U, pIns.PitchEnv.size());
				Assert.AreEqual(1U, pIns.PitchEnv.nSustainStart);
				Assert.AreEqual(2U, pIns.PitchEnv.nSustainEnd);
				Assert.AreEqual(96, pIns.PitchEnv[1].Tick);
				Assert.AreEqual(64, pIns.PitchEnv[1].Value);
			}

			// Sequences
			Assert.AreEqual(2, sndFile.Order.GetNumSequences());

			Assert.AreEqual(3, sndFile.Order[0].GetLengthTailTrimmed());
			Assert.AreEqual("First Sequence", sndFile.Order[0].GetName());
			Assert.AreEqual(Snd_Def.PatternIndex_Skip, sndFile.Order[0][0]);
			Assert.AreEqual(0, sndFile.Order[0][1]);
			Assert.AreEqual(Snd_Def.PatternIndex_Skip, sndFile.Order[0][2]);
			Assert.AreEqual(1, sndFile.Order[0].GetRestartPos());
			Assert.AreEqual(new Tempo(139, 999), sndFile.Order[0].GetDefaultTempo());
			Assert.AreEqual(5U, sndFile.Order[0].GetDefaultSpeed());

			Assert.AreEqual(3, sndFile.Order[1].GetLengthTailTrimmed());
			Assert.AreEqual("Second Sequence", sndFile.Order[1].GetName());
			Assert.AreEqual(1, sndFile.Order[1][0]);
			Assert.AreEqual(2, sndFile.Order[1][1]);
			Assert.AreEqual(3, sndFile.Order[1][2]);
			Assert.AreEqual(2, sndFile.Order[1].GetRestartPos());
			Assert.AreEqual(new Tempo(123, 4500), sndFile.Order[1].GetDefaultTempo());
			Assert.AreEqual(67U, sndFile.Order[1].GetDefaultSpeed());

			// Patterns
			Assert.AreEqual(2, sndFile.Patterns.GetNumPatterns());

			Assert.AreEqual("First Pattern", sndFile.Patterns[0].GetName().ToString());
			Assert.AreEqual(70U, sndFile.Patterns[0].GetNumRows());
			Assert.AreEqual(70, sndFile.Patterns[0].GetNumChannels());
			Assert.IsTrue(sndFile.Patterns[0].GetOverrideSignature());
			Assert.AreEqual(5U, sndFile.Patterns[0].GetRowsPerBeat());
			Assert.AreEqual(10U, sndFile.Patterns[0].GetRowsPerMeasure());
			Assert.IsTrue(sndFile.Patterns[0].HasTempoSwing());
			Assert.IsTrue(sndFile.Patterns.IsPatternEmpty(0));

			{
				TempoSwing swing = sndFile.Patterns[0].GetTempoSwing();

				Assert.AreEqual(5U, swing.size());
				Assert.AreEqual(16770149U, swing[0]);
				Assert.AreEqual(16803696U, swing[1]);
				Assert.AreEqual(16770157U, swing[2]);
				Assert.AreEqual(29347774U, swing[3]);
				Assert.AreEqual(4194304U, swing[4]);
			}

			Assert.AreEqual("Second Pattern", sndFile.Patterns[1].GetName().ToString());
			Assert.AreEqual(32U, sndFile.Patterns[1].GetNumRows());
			Assert.AreEqual(70, sndFile.Patterns[1].GetNumChannels());
			Assert.IsFalse(sndFile.Patterns[1].GetOverrideSignature());
			Assert.AreEqual(0U, sndFile.Patterns[1].GetRowsPerBeat());
			Assert.AreEqual(0U, sndFile.Patterns[1].GetRowsPerMeasure());
			Assert.IsFalse(sndFile.Patterns[1].HasTempoSwing());
			Assert.IsFalse(sndFile.Patterns.IsPatternEmpty(1));
			Assert.IsTrue(sndFile.Patterns[1].GetpModCommand(0, 0)[0].IsPcNote());
			Assert.AreEqual(ModCommand.Note_Pc, sndFile.Patterns[1].GetpModCommand(0, 0)[0].Note);
			Assert.AreEqual(99, sndFile.Patterns[1].GetpModCommand(0, 0)[0].Instr);
			Assert.AreEqual(1, sndFile.Patterns[1].GetpModCommand(0, 0)[0].GetValueVolCol());
			Assert.AreEqual(200, sndFile.Patterns[1].GetpModCommand(0, 0)[0].GetValueEffectCol());
			Assert.IsTrue(sndFile.Patterns[1].GetpModCommand(1, 0)[0].IsPcNote());
			Assert.AreEqual(ModCommand.Note_Pcs, sndFile.Patterns[1].GetpModCommand(1, 0)[0].Note);
			Assert.AreEqual(250, sndFile.Patterns[1].GetpModCommand(1, 0)[0].Instr);
			Assert.AreEqual(999, sndFile.Patterns[1].GetpModCommand(1, 0)[0].GetValueVolCol());
			Assert.AreEqual(999, sndFile.Patterns[1].GetpModCommand(1, 0)[0].GetValueEffectCol());

			Assert.AreEqual(ModCommand.Note_Min, sndFile.Patterns[1].GetpModCommand(0, 1)[0].Note);
			Assert.AreEqual(ModCommand.Note_Min + 119, sndFile.Patterns[1].GetpModCommand(1, 1)[0].Note);
			Assert.AreEqual(ModCommand.Note_KeyOff, sndFile.Patterns[1].GetpModCommand(2, 1)[0].Note);
			Assert.AreEqual(ModCommand.Note_NoteCut, sndFile.Patterns[1].GetpModCommand(3, 1)[0].Note);
			Assert.AreEqual(ModCommand.Note_Fade, sndFile.Patterns[1].GetpModCommand(4, 1)[0].Note);

			Assert.IsTrue(sndFile.Patterns[1].GetpModCommand(31, 0)[0].IsEmpty());
			Assert.IsFalse(sndFile.Patterns[1].GetpModCommand(31, 1)[0].IsEmpty());
			Assert.IsFalse(sndFile.Patterns[1].GetpModCommand(31, 0)[0].IsPcNote());
			Assert.AreEqual(ModCommand.Note_MiddleC + 12, sndFile.Patterns[1].GetpModCommand(31, 1)[0].Note);
			Assert.AreEqual(45, sndFile.Patterns[1].GetpModCommand(31, 1)[0].Instr);
			Assert.AreEqual(VolumeCommand.VolSlideDown, sndFile.Patterns[1].GetpModCommand(31, 1)[0].VolCmd);
			Assert.AreEqual(5, sndFile.Patterns[1].GetpModCommand(31, 1)[0].Vol);
			Assert.AreEqual(EffectCommand.Panning8, sndFile.Patterns[1].GetpModCommand(31, 1)[0].Command);
			Assert.AreEqual(0xff, sndFile.Patterns[1].GetpModCommand(31, 1)[0].Param);

			// Plugins

			Assert.AreEqual(0, sndFile.FrequencyToCutOff(sndFile.CutOffToFrequency(0)));
			Assert.AreEqual(80, sndFile.FrequencyToCutOff(sndFile.CutOffToFrequency(80)));
			Assert.AreEqual(127, sndFile.FrequencyToCutOff(sndFile.CutOffToFrequency(127)));
		}
	}
}
