/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
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
		/// Test S3M file loading
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_LoadS3M()
		{
			string fileNameBaseSrc = GetTestFileNameBase();
			CSoundFile sndFile = CreateSoundFileContainer(fileNameBaseSrc + "s3m");

			TestLoadS3MFile(sndFile);

			// Test GetLength code, in particular with subsongs
			sndFile.ChnSettings[1].dwFlags.Reset(ChannelFlags.Chn_Mute);

			Assert.AreEqual(19.237, sndFile.GetLength(EnmGetLengthResetMode.eAdjustSamplePositions, new GetLengthTarget(3, 1)).back().Duration, 0.01);
			Assert.IsFalse(sndFile.GetLength(EnmGetLengthResetMode.eAdjustSamplePositions, new GetLengthTarget(2, 0).StartPos(0, 1, 0)).back().TargetReached);

			vector<GetLengthType> allSubSongs = sndFile.GetLength(EnmGetLengthResetMode.eNoAdjust, new GetLengthTarget(true));
			Assert.AreEqual(3U, allSubSongs.size());

			c_double totalDuration = 0.0;
			foreach (GetLengthType subSong in allSubSongs)
				totalDuration += subSong.Duration;

			Assert.AreEqual(3674.38, totalDuration, 1.0);
		}



		/********************************************************************/
		/// <summary>
		/// Check if our test file was loaded correctly
		/// </summary>
		/********************************************************************/
		private void TestLoadS3MFile(CSoundFile sndFile)
		{
			// Global variables
			Assert.AreEqual("S3M_Test__________________X", sndFile.GetTitle().ToString());
			Assert.AreEqual(new Tempo(33, 0), sndFile.Order.Current.GetDefaultTempo());
			Assert.AreEqual(254U, sndFile.Order.Current.GetDefaultSpeed());
			Assert.AreEqual(32 * 4U, sndFile.m_nDefaultGlobalVolume);
			Assert.AreEqual(36U, sndFile.m_nVstiVolume);
			Assert.AreEqual(16U, sndFile.m_nSamplePreAmp);
			Assert.AreEqual(SongFlags.FastVolSlides, sndFile.m_SongFlags);
			Assert.AreEqual(MixLevels.Compatible, sndFile.GetMixLevels());
			Assert.AreEqual(TempoMode.Classic, sndFile.m_nTempoMode);
			Assert.AreEqual(Version.Parse("1.32.00.32"), sndFile.m_dwLastSavedWithVersion);
			Assert.AreEqual(1U, sndFile.GetFileHistory().size());

			// Channels
			Assert.AreEqual(4, sndFile.GetNumChannels());

			Assert.AreEqual(0, sndFile.ChnSettings[0].nPan);
			Assert.AreEqual(ChannelFlags.None, sndFile.ChnSettings[0].dwFlags);

			Assert.AreEqual(256, sndFile.ChnSettings[1].nPan);
			Assert.AreEqual(ChannelFlags.Chn_Mute, sndFile.ChnSettings[1].dwFlags);

			Assert.AreEqual(85, sndFile.ChnSettings[2].nPan);
			Assert.AreEqual(ChannelFlags.None, sndFile.ChnSettings[2].dwFlags);

			Assert.AreEqual(171, sndFile.ChnSettings[3].nPan);
			Assert.AreEqual(ChannelFlags.Chn_Mute, sndFile.ChnSettings[3].dwFlags);

			// Samples
			Assert.AreEqual(4, sndFile.GetNumSamples());
			{
				ModSample sample = sndFile.GetSample(1);

				Assert.AreEqual("Sample_1__________________X", sndFile.m_szNames[1].Str().ToString());
				Assert.AreEqual("Filename_1_X", sample.FileName.Str().ToString());
				Assert.AreEqual(1, sample.GetBytesPerSample());
				Assert.AreEqual(1, sample.GetNumChannels());
				Assert.AreEqual(1, sample.GetElementarySampleSize());
				Assert.AreEqual(60U, sample.GetSampleSizeInBytes());
				Assert.AreEqual(9001U, sample.GetSampleRate(ModType.S3M));
				Assert.AreEqual(32 * 4, sample.nVolume);
				Assert.AreEqual(64, sample.nGlobalVol);
				Assert.AreEqual(ChannelFlags.Chn_Loop, sample.uFlags);

				Assert.AreEqual(16U, sample.nLoopStart);
				Assert.AreEqual(60U, sample.nLoopEnd);

				// Sample data
				for (size_t i = 0; i < 30; i++)
					Assert.AreEqual(127, sample.Sample8()[i]);

				for (size_t i = 31; i < 60; i++)
					Assert.AreEqual(-128, sample.Sample8()[i]);
			}
			{
				ModSample sample = sndFile.GetSample(2);

				Assert.AreEqual("Empty", sndFile.m_szNames[2].Str().ToString());
				Assert.AreEqual(16384U, sample.GetSampleRate(ModType.S3M));
				Assert.AreEqual(2 * 4, sample.nVolume);
			}
			{
				ModSample sample = sndFile.GetSample(3);

				Assert.AreEqual("Stereo / 16-Bit", sndFile.m_szNames[3].Str().ToString());
				Assert.AreEqual("Filename_3_X", sample.FileName.Str().ToString());
				Assert.AreEqual(4, sample.GetBytesPerSample());
				Assert.AreEqual(2, sample.GetNumChannels());
				Assert.AreEqual(2, sample.GetElementarySampleSize());
				Assert.AreEqual(64U, sample.GetSampleSizeInBytes());
				Assert.AreEqual(16000U, sample.GetSampleRate(ModType.S3M));
				Assert.AreEqual(0, sample.nVolume);
				Assert.AreEqual(ChannelFlags.Chn_Loop | ChannelFlags.Chn_16Bit | ChannelFlags.Chn_Stereo, sample.uFlags);

				Assert.AreEqual(0U, sample.nLoopStart);
				Assert.AreEqual(16U, sample.nLoopEnd);

				// Sample Data (Stereo Interleaved)
				for (size_t i = 0; i < 7; i++)
					Assert.AreEqual(-32768, sample.Sample16()[4 + i]);
			}
			{
				ModSample sample = sndFile.GetSample(4);

				Assert.AreEqual("adlib", sndFile.m_szNames[4].Str().ToString());
				Assert.AreEqual(string.Empty, sample.FileName.Str().ToString());
				Assert.AreEqual(8363U, sample.GetSampleRate(ModType.S3M));
				Assert.AreEqual(58 * 4, sample.nVolume);
//XX				Assert.AreEqual(ChannelFlags.Chn_Adlib, sample.uFlags);
//XX				Assert.AreEqual(new OplPatch(0x00, 0x00, 0xc0, 0x00, 0xf0, 0xd2, 0x05, 0xb3, 0x01, 0x00, 0x00, 0x00), sample.Adlib);
			}

			// Orders
			Assert.AreEqual(0, sndFile.Order.Current.GetRestartPos());
			Assert.AreEqual(5, sndFile.Order.Current.GetLengthTailTrimmed());
			Assert.AreEqual(0, sndFile.Order.Current[0]);
			Assert.AreEqual(Snd_Def.PatternIndex_Skip, sndFile.Order.Current[1]);
			Assert.AreEqual(Snd_Def.PatternIndex_Invalid, sndFile.Order.Current[2]);
			Assert.AreEqual(1, sndFile.Order.Current[3]);
			Assert.AreEqual(0, sndFile.Order.Current[4]);

			// Patterns
			Assert.AreEqual(2, sndFile.Patterns.GetNumPatterns());

			Assert.AreEqual(64U, sndFile.Patterns[0].GetNumRows());
			Assert.AreEqual(4, sndFile.Patterns[0].GetNumChannels());
			Assert.IsFalse(sndFile.Patterns[0].GetOverrideSignature());
			Assert.AreEqual(ModCommand.Note_Min + 12, sndFile.Patterns[0].GetpModCommand(0, 0)[0].Note);
			Assert.AreEqual(ModCommand.Note_Min + 107, sndFile.Patterns[0].GetpModCommand(1, 0)[0].Note);
			Assert.AreEqual(ModCommand.Note_NoteCut, sndFile.Patterns[0].GetpModCommand(2, 0)[0].Note);
			Assert.AreEqual(VolumeCommand.Volume, sndFile.Patterns[0].GetpModCommand(0, 1)[0].VolCmd);
			Assert.AreEqual(0, sndFile.Patterns[0].GetpModCommand(0, 1)[0].Vol);
			Assert.AreEqual(VolumeCommand.Volume, sndFile.Patterns[0].GetpModCommand(1, 1)[0].VolCmd);
			Assert.AreEqual(64, sndFile.Patterns[0].GetpModCommand(1, 1)[0].Vol);
			Assert.AreEqual(VolumeCommand.Panning, sndFile.Patterns[0].GetpModCommand(3, 1)[0].VolCmd);
			Assert.AreEqual(64, sndFile.Patterns[0].GetpModCommand(3, 1)[0].Vol);
			Assert.AreEqual(EffectCommand.Speed, sndFile.Patterns[0].GetpModCommand(0, 3)[0].Command);
			Assert.AreEqual(0x11, sndFile.Patterns[0].GetpModCommand(0, 3)[0].Param);
			Assert.IsFalse(sndFile.Patterns[0].GetpModCommand(3, 0)[0].IsEmpty());
			Assert.AreEqual(ModCommand.Note_None, sndFile.Patterns[0].GetpModCommand(3, 0)[0].Note);
			Assert.AreEqual(99, sndFile.Patterns[0].GetpModCommand(3, 0)[0].Instr);
			Assert.IsTrue(sndFile.Patterns[0].GetpModCommand(4, 0)[0].IsEmpty());
			Assert.IsTrue(sndFile.Patterns[0].GetpModCommand(5, 0)[0].IsEmpty());

			Assert.AreEqual(64U, sndFile.Patterns[1].GetNumRows());
			Assert.IsFalse(sndFile.Patterns.IsPatternEmpty(1));
			Assert.AreEqual(0x04, sndFile.Patterns[1].GetpModCommand(63, 3)[0].Param);
		}
	}
}
