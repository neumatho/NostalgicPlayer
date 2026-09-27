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
		/// Test XM file loading
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_LoadXm()
		{
			string fileNameBaseSrc = GetTestFileNameBase();
			CSoundFile sndFile = CreateSoundFileContainer(fileNameBaseSrc + "xm");

			TestLoadXmFile(sndFile);
		}



		/********************************************************************/
		/// <summary>
		/// Check if our test file was loaded correctly
		/// </summary>
		/********************************************************************/
		private void TestLoadXmFile(CSoundFile sndFile)
		{
			// Global variables
			Assert.AreEqual("Test Module", sndFile.GetTitle().ToString());
			Assert.AreEqual("OpenMPT Module Loader Test Suite", sndFile.m_SongMessage.substr(0, 32));
			Assert.AreEqual(new Tempo(139, 0), sndFile.Order.Current.GetDefaultTempo());
			Assert.AreEqual(5U, sndFile.Order.Current.GetDefaultSpeed());
			Assert.AreEqual(128U, sndFile.m_nDefaultGlobalVolume);
			Assert.AreEqual(42U, sndFile.m_nVstiVolume);
			Assert.AreEqual(23U, sndFile.m_nSamplePreAmp);
			Assert.AreEqual(SongFlags.LinearSlides | SongFlags.ExFilterRange, sndFile.m_SongFlags);
			Assert.IsTrue(sndFile.m_PlayBehaviour[PlayBehaviour.Msf_Compatible_Play]);
			Assert.IsFalse(sndFile.m_PlayBehaviour[PlayBehaviour.MidiCCBugEmulation]);
			Assert.IsFalse(sndFile.m_PlayBehaviour[PlayBehaviour.MptOldSwingBehaviour]);
			Assert.IsFalse(sndFile.m_PlayBehaviour[PlayBehaviour.OldMidiPitchBends]);
			Assert.AreEqual(MixLevels.Compatible, sndFile.GetMixLevels());
			Assert.AreEqual(TempoMode.Modern, sndFile.m_nTempoMode);
			Assert.AreEqual(6U, sndFile.m_nDefaultRowsPerBeat);
			Assert.AreEqual(12U, sndFile.m_nDefaultRowsPerMeasure);
			Assert.AreEqual(Version.Parse("1.19.02.05"), sndFile.m_dwCreatedWithVersion);
			Assert.AreEqual(1, sndFile.Order.Current.GetRestartPos());

			// Macros
			Assert.AreEqual(ParameteredMacro.SFxReso, sndFile.m_MidiCfg.GetParameteredMacroType(0));
			Assert.AreEqual(ParameteredMacro.SFxDryWet, sndFile.m_MidiCfg.GetParameteredMacroType(1));
			Assert.AreEqual(FixedMacro.ZxxResoFltMode, sndFile.m_MidiCfg.GetFixedMacroType());

			// Channels
			Assert.AreEqual(2, sndFile.GetNumChannels());
			Assert.AreEqual("First Channel", sndFile.ChnSettings[0].szName.Str().ToString());
			Assert.AreEqual("Second Channel", sndFile.ChnSettings[1].szName.Str().ToString());

			// Samples
			Assert.AreEqual(3, sndFile.GetNumSamples());
			Assert.AreEqual("Pulse Sample", sndFile.m_szNames[1].Str().ToString());
			Assert.AreEqual("Empty Sample", sndFile.m_szNames[2].Str().ToString());
			Assert.AreEqual("Unassigned Sample", sndFile.m_szNames[3].Str().ToString());

			ModSample sample = sndFile.GetSample(1);
			Assert.AreEqual(1, sample.GetBytesPerSample());
			Assert.AreEqual(1, sample.GetNumChannels());
			Assert.AreEqual(1, sample.GetElementarySampleSize());
			Assert.AreEqual(16U, sample.GetSampleSizeInBytes());
			Assert.AreEqual(35, sample.nFineTune);
			Assert.AreEqual(1, sample.RelativeTone);
			Assert.AreEqual(32 * 4, sample.nVolume);
			Assert.AreEqual(64, sample.nGlobalVol);
			Assert.AreEqual(160, sample.nPan);
			Assert.AreEqual(ChannelFlags.Chn_Panning | ChannelFlags.Chn_Loop | ChannelFlags.Chn_PingPongLoop, sample.uFlags);

			Assert.AreEqual(1U, sample.nLoopStart);
			Assert.AreEqual(8U, sample.nLoopEnd);

			Assert.AreEqual(VibratoType.Square, sample.nVibType);
			Assert.AreEqual(3, sample.nVibSweep);
			Assert.AreEqual(4, sample.nVibRate);
			Assert.AreEqual(5, sample.nVibDepth);

			// Sample data
			for (size_t i = 0; i < 6; i++)
				Assert.AreEqual(18, sample.Sample8()[i]);

			for (size_t i = 6; i < 16; i++)
				Assert.AreEqual(0, sample.Sample8()[i]);

			// Instruments
			Assert.AreEqual(1, sndFile.GetNumInstruments());

			ModInstrument pIns = sndFile.Instruments[1];
			Assert.AreEqual(1024U, pIns.nFadeOut);
			Assert.AreEqual(128U, pIns.nPan);
			Assert.AreEqual(InstrumentFlags.None, pIns.dwFlags);

			Assert.AreEqual(0, pIns.nPps);
			Assert.AreEqual(ModCommand.Note_MiddleC - 1, pIns.nPpc);

			Assert.AreEqual(1200, pIns.nVolRampUp);
			Assert.AreEqual(ResamplingMode.Sinc8Lp, pIns.Resampling);

			Assert.IsFalse(pIns.IsCutOffEnabled());
			Assert.AreEqual(0, pIns.GetCutOff());
			Assert.IsFalse(pIns.IsResonanceEnabled());
			Assert.AreEqual(0, pIns.GetResonance());
			Assert.AreEqual(FilterMode.Unchanged, pIns.FilterMode);

			Assert.AreEqual(0, pIns.nVolSwing);
			Assert.AreEqual(0, pIns.nPanSwing);
			Assert.AreEqual(0, pIns.nCutSwing);
			Assert.AreEqual(0, pIns.nResSwing);

			Assert.AreEqual(NewNoteAction.NoteCut, pIns.nNna);
			Assert.AreEqual(DuplicateCheckType.None, pIns.nDct);

			Assert.AreEqual(1, pIns.nMixPlug);
			Assert.AreEqual(16, pIns.nMidiChannel);
			Assert.AreEqual(64, pIns.nMidiProgram);
			Assert.AreEqual(2, pIns.wMidiBank);
			Assert.AreEqual(8, pIns.MidiPwd);

			Assert.IsNull(pIns.pTuning);

			Assert.AreEqual(new Tempo(0, 0), pIns.PitchToTempoLock);

			Assert.AreEqual(PlugVelocityHandling.Volume, pIns.PluginVelocityHandling);
			Assert.AreEqual(PlugVolumeHandling.Midi, pIns.PluginVolumeHandling);

			for (size_t i = sndFile.GetModSpecifications().NoteMin; i < sndFile.GetModSpecifications().NoteMax; i++)
				Assert.AreEqual(i == ModCommand.Note_MiddleC - 1 ? 2 : 1, pIns.Keyboard[i]);

			Assert.AreEqual(EnvelopeFlags.Enabled | EnvelopeFlags.Sustain, pIns.VolEnv.dwFlags);
			Assert.AreEqual(3U, pIns.VolEnv.size());
			Assert.AreEqual(Snd_Def.Env_Release_Node_Unset, pIns.VolEnv.nReleaseNode);
			Assert.AreEqual(96, pIns.VolEnv[2].Tick);
			Assert.AreEqual(0, pIns.VolEnv[2].Value);
			Assert.AreEqual(1, pIns.VolEnv.nSustainStart);
			Assert.AreEqual(1, pIns.VolEnv.nSustainEnd);

			Assert.AreEqual(EnvelopeFlags.Loop, pIns.PanEnv.dwFlags);
			Assert.AreEqual(12U, pIns.PanEnv.size());
			Assert.AreEqual(9, pIns.PanEnv.nLoopStart);
			Assert.AreEqual(11, pIns.PanEnv.nLoopEnd);
			Assert.AreEqual(Snd_Def.Env_Release_Node_Unset, pIns.PanEnv.nReleaseNode);
			Assert.AreEqual(46, pIns.PanEnv[9].Tick);
			Assert.AreEqual(23, pIns.PanEnv[9].Value);

			Assert.AreEqual(EnvelopeFlags.None, pIns.PitchEnv.dwFlags);
			Assert.AreEqual(0U, pIns.PitchEnv.size());

			// Sequences
			Assert.AreEqual(1, sndFile.Order.GetNumSequences());
			Assert.AreEqual(0, sndFile.Order.Current[0]);
			Assert.AreEqual(1, sndFile.Order.Current[1]);

			// Patterns
			Assert.AreEqual(2, sndFile.Patterns.GetNumPatterns());

			Assert.AreEqual("First Pattern", sndFile.Patterns[0].GetName().ToString());
			Assert.AreEqual(64U, sndFile.Patterns[0].GetNumRows());
			Assert.AreEqual(2, sndFile.Patterns[0].GetNumChannels());
			Assert.IsFalse(sndFile.Patterns[0].GetOverrideSignature());
			Assert.AreEqual(0U, sndFile.Patterns[0].GetRowsPerBeat());
			Assert.AreEqual(0U, sndFile.Patterns[0].GetRowsPerMeasure());
			Assert.IsTrue(sndFile.Patterns.IsPatternEmpty(0));

			Assert.AreEqual("Second Pattern", sndFile.Patterns[1].GetName().ToString());
			Assert.AreEqual(32U, sndFile.Patterns[1].GetNumRows());
			Assert.AreEqual(2, sndFile.Patterns[1].GetNumChannels());
			Assert.IsFalse(sndFile.Patterns[1].GetOverrideSignature());
			Assert.AreEqual(0U, sndFile.Patterns[1].GetRowsPerBeat());
			Assert.AreEqual(0U, sndFile.Patterns[1].GetRowsPerMeasure());
			Assert.IsFalse(sndFile.Patterns.IsPatternEmpty(1));
			Assert.IsFalse(sndFile.Patterns[1].GetpModCommand(0, 0)[0].IsPcNote());
			Assert.AreEqual(ModCommand.Note_None, sndFile.Patterns[1].GetpModCommand(0, 0)[0].Note);
			Assert.AreEqual(0, sndFile.Patterns[1].GetpModCommand(0, 0)[0].Instr);
			Assert.AreEqual(VolumeCommand.VibratoSpeed, sndFile.Patterns[1].GetpModCommand(0, 0)[0].VolCmd);
			Assert.AreEqual(15, sndFile.Patterns[1].GetpModCommand(0, 0)[0].Vol);
			Assert.AreEqual(ModCommand.Note_Min + 12, sndFile.Patterns[1].GetpModCommand(0, 1)[0].Note);
			Assert.AreEqual(ModCommand.Note_Min + 12 + 95, sndFile.Patterns[1].GetpModCommand(1, 1)[0].Note);
			Assert.AreEqual(ModCommand.Note_KeyOff, sndFile.Patterns[1].GetpModCommand(2, 1)[0].Note);
			Assert.IsTrue(sndFile.Patterns[1].GetpModCommand(30, 0)[0].IsEmpty());	// Test for resaved out-of-range note
			Assert.IsTrue(sndFile.Patterns[1].GetpModCommand(31, 0)[0].IsEmpty());	// Test for resaved out-of-range note
			Assert.IsFalse(sndFile.Patterns[1].GetpModCommand(31, 1)[0].IsEmpty());
			Assert.IsFalse(sndFile.Patterns[1].GetpModCommand(31, 1)[0].IsPcNote());
			Assert.AreEqual(ModCommand.Note_MiddleC + 12, sndFile.Patterns[1].GetpModCommand(31, 1)[0].Note);
			Assert.AreEqual(45, sndFile.Patterns[1].GetpModCommand(31, 1)[0].Instr);
			Assert.AreEqual(VolumeCommand.VolSlideDown, sndFile.Patterns[1].GetpModCommand(31, 1)[0].VolCmd);
			Assert.AreEqual(5, sndFile.Patterns[1].GetpModCommand(31, 1)[0].Vol);
			Assert.AreEqual(EffectCommand.Panning8, sndFile.Patterns[1].GetpModCommand(31, 1)[0].Command);
			Assert.AreEqual(0xff, sndFile.Patterns[1].GetpModCommand(31, 1)[0].Param);

			// Test 4-bit Panning conversion
			for (c_int i = 0; i < 16; i++)
				Assert.AreEqual(i * 4, sndFile.Patterns[1].GetpModCommand((RowIndex)(10 + i), 0)[0].Vol);
		}
	}
}
