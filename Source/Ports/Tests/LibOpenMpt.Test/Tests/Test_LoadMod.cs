/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C.Std;
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
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_LoadMod()
		{
			string fileNameBaseSrc = GetTestFileNameBase();
			CSoundFile sndFile = CreateSoundFileContainer(fileNameBaseSrc + "mod");

			TestLoadModFile(sndFile);
		}



		/********************************************************************/
		/// <summary>
		/// Check if our test file was loaded correctly
		/// </summary>
		/********************************************************************/
		private void TestLoadModFile(CSoundFile sndFile)
		{
			// Global variables
			Assert.AreEqual("MOD_Test___________X", sndFile.GetTitle().ToString());
			Assert.AreEqual(SongFlags.Pt_Mode | SongFlags.AmigaLimits | SongFlags.IsAmiga | SongFlags.Format_No_VolCol, sndFile.m_SongFlags);
			Assert.AreEqual(MixLevels.Compatible, sndFile.GetMixLevels());
			Assert.AreEqual(TempoMode.Classic, sndFile.m_nTempoMode);
			Assert.AreEqual(4, sndFile.GetNumChannels());
			Assert.IsTrue(sndFile.m_PlayBehaviour[PlayBehaviour.ModOneShotLoops]);
			Assert.IsTrue(sndFile.m_PlayBehaviour[PlayBehaviour.ModSampleSwap]);
			Assert.IsTrue(sndFile.m_PlayBehaviour[PlayBehaviour.ModIgnorePanning]);
			Assert.IsFalse(sndFile.m_PlayBehaviour[PlayBehaviour.ModVBlankTiming]);

			// Test GetLength code, in particular with subsongs
			Assert.IsFalse(sndFile.GetLength(EnmGetLengthResetMode.eNoAdjust, new GetLengthTarget(0, 4)).back().TargetReached);

			vector<GetLengthType> allSubSongs = sndFile.GetLength(EnmGetLengthResetMode.eNoAdjust, new GetLengthTarget(true));
			Assert.AreEqual(2U, allSubSongs.size());
			Assert.AreEqual(2.04, allSubSongs[0].Duration, 0.1);
			Assert.AreEqual(118.84, allSubSongs[1].Duration, 0.1);
			Assert.AreEqual(0, allSubSongs[0].RestartOrder);
			Assert.AreEqual(1U, allSubSongs[0].RestartRow);
			Assert.AreEqual(2, allSubSongs[1].RestartOrder);
			Assert.AreEqual(61U, allSubSongs[1].RestartRow);
			Assert.AreEqual(2, allSubSongs[1].StartOrder);
			Assert.AreEqual(0U, allSubSongs[1].StartRow);

			// Samples
			Assert.AreEqual(31, sndFile.GetNumSamples());
			{
				ModSample sample = sndFile.GetSample(1);

				Assert.AreEqual("Sample_1_____________X", sndFile.m_szNames[1].Str().ToString());
				Assert.AreEqual(1, sample.GetBytesPerSample());
				Assert.AreEqual(1, sample.GetNumChannels());
				Assert.AreEqual(1, sample.GetElementarySampleSize());
				Assert.AreEqual(1244U, sample.GetSampleSizeInBytes());
				Assert.AreEqual(0x70, sample.nFineTune);
				Assert.AreEqual(0, sample.RelativeTone);
				Assert.AreEqual(256, sample.nVolume);
				Assert.AreEqual(64, sample.nGlobalVol);
				Assert.AreEqual(ChannelFlags.Chn_Loop, sample.uFlags);

				Assert.AreEqual(0U, sample.nLoopStart);
				Assert.AreEqual(128U, sample.nLoopEnd);

				// Sample data
				Assert.AreEqual(0, sample.Sample8()[0]);
				Assert.AreEqual(0, sample.Sample8()[1]);
				Assert.AreEqual(-29, sample.Sample8()[2]);
			}
			{
				ModSample sample = sndFile.GetSample(3);

				Assert.AreEqual("OpenMPT Module Loader", sndFile.m_szNames[3].Str().ToString());
				Assert.AreEqual(0U, sample.GetSampleSizeInBytes());
				Assert.AreEqual(-0x80, sample.nFineTune);
				Assert.AreEqual(0, sample.RelativeTone);
				Assert.AreEqual(4, sample.nVolume);
				Assert.AreEqual(64, sample.nGlobalVol);
			}

			// Orders
			Assert.AreEqual(0, sndFile.Order.Current.GetRestartPos());
			Assert.AreEqual(4, sndFile.Order.Current.GetLengthTailTrimmed());
			Assert.AreEqual(0, sndFile.Order.Current[0]);
			Assert.AreEqual(1, sndFile.Order.Current[1]);
			Assert.AreEqual(2, sndFile.Order.Current[2]);
			Assert.AreEqual(0, sndFile.Order.Current[3]);

			// Patterns
			Assert.AreEqual(3, sndFile.Patterns.GetNumPatterns());
			Assert.AreEqual(64U, sndFile.Patterns[2].GetNumRows());
			Assert.AreEqual(ModCommand.Note_MiddleC + 12, sndFile.Patterns[2].GetpModCommand(1, 0)[0].Note);
			Assert.AreEqual(1, sndFile.Patterns[2].GetpModCommand(16, 3)[0].Instr);
			Assert.AreEqual(EffectCommand.Panning8, sndFile.Patterns[2].GetpModCommand(19, 3)[0].Command);
			Assert.AreEqual(0x28, sndFile.Patterns[2].GetpModCommand(19, 3)[0].Param);
			Assert.AreEqual(EffectCommand.Tempo, sndFile.Patterns[2].GetpModCommand(20, 0)[0].Command);
			Assert.AreEqual(EffectCommand.Speed, sndFile.Patterns[2].GetpModCommand(20, 1)[0].Command);
		}
	}
}
