/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C.Std;
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
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_MidiMacroParser()
		{
			uint8[] rawData = [ 0x90, 0x40, 0x70, 0x50, 0x70, 0xf5, 0xf6, 0x60, 0x70, 0xf0 ];

			MidiMacroParser rawParser = new MidiMacroParser(new MptSpan<uint8>(rawData));
			MptSpan<uint8> midiMsg;

			rawParser.NextMessage(out midiMsg);
			Assert.IsTrue(midiMsg.Span_Element_Equal(new MptSpan<uint8>([ 0x90, 0x40, 0x70 ])));

			rawParser.NextMessage(out midiMsg);
			Assert.IsTrue(midiMsg.Span_Element_Equal(new MptSpan<uint8>([ 0x90, 0x50, 0x70 ])));

			rawParser.NextMessage(out midiMsg);
			Assert.IsTrue(midiMsg.Span_Element_Equal(new MptSpan<uint8>([ 0xf5 ])));

			rawParser.NextMessage(out midiMsg);
			Assert.IsTrue(midiMsg.Span_Element_Equal(new MptSpan<uint8>([ 0xf6 ])));

			rawParser.NextMessage(out midiMsg);
			Assert.IsTrue(midiMsg.Span_Element_Equal(new MptSpan<uint8>([ 0x90, 0x60, 0x70 ])));

			rawParser.NextMessage(out midiMsg);
			Assert.IsTrue(midiMsg.Span_Element_Equal(new MptSpan<uint8>([ 0xf0 ])));

			Assert.IsFalse(rawParser.NextMessage(out midiMsg));

			CSoundFile sndFile = new CSoundFile();
			PlayState playState = new PlayState();
			ModInstrument instr = new ModInstrument();

			sndFile.Create(ModType.Mpt, 2);

			playState.m_nGlobalVolume = (c_int)(Snd_Def.Max_Global_Volume / 2);
			playState.Chn[3].nMasterChn = 2;
			playState.Chn[3].nLastNote = ModCommand.Note_Min + 0x60;
			playState.Chn[3].nVolume = 193;
			playState.Chn[3].nVolSwing = 32;
			playState.Chn[3].nCalcVolume = 8192;
			playState.Chn[3].nGlobalVol = 32;
			playState.Chn[3].nInsVol = 32;
			playState.Chn[3].nPan = 32;
			playState.Chn[3].nRealPan = 192;
			playState.Chn[3].OldOffset = 0x123456;
			playState.Chn[3].pModInstrument = instr;

			instr.nMidiChannel = Snd_Def.MidiLastChannel;
			instr.nMidiProgram = 1 + 9;
			instr.wMidiBank = 1 + 130;

			char[] macro = "r42 9c z v 50 h F5 F6 n u Bc a b xyop F0 41 10 00 10 12 10 00 04 00 2 s".ToCharArray();
			vector<uint8> @out = new vector<uint8>((size_t)macro.Length + 1);
			MidiMacroParser macroParser = new MidiMacroParser(sndFile, playState, 3, false, new MptSpan<uint8>(Encoding.Latin1.GetBytes(macro)), new MptSpan<uint8>(@out), 64, 0);

			macroParser.NextMessage(out midiMsg);
			Assert.IsTrue(midiMsg.Span_Element_Equal(new MptSpan<uint8>([ 0x9f, 0x40, 0x0e ])));

			macroParser.NextMessage(out midiMsg);
			Assert.IsTrue(midiMsg.Span_Element_Equal(new MptSpan<uint8>([ 0x9f, 0x50, 0x01 ])));

			macroParser.NextMessage(out midiMsg);
			Assert.IsTrue(midiMsg.Span_Element_Equal(new MptSpan<uint8>([ 0xf5 ])));

			macroParser.NextMessage(out midiMsg);
			Assert.IsTrue(midiMsg.Span_Element_Equal(new MptSpan<uint8>([ 0xf6 ])));

			macroParser.NextMessage(out midiMsg);
			Assert.IsTrue(midiMsg.Span_Element_Equal(new MptSpan<uint8>([ 0x9f, 0x60, 0x08 ])));

			macroParser.NextMessage(out midiMsg);
			Assert.IsTrue(midiMsg.Span_Element_Equal(new MptSpan<uint8>([ 0xbf, 0x01, 0x02 ])));

			macroParser.NextMessage(out midiMsg);
			Assert.IsTrue(midiMsg.Span_Element_Equal(new MptSpan<uint8>([ 0xbf, 0x10, 0x60 ])));

			macroParser.NextMessage(out midiMsg);
			Assert.IsTrue(midiMsg.Span_Element_Equal(new MptSpan<uint8>([ 0xbf, 0x34, 0x09 ])));

			macroParser.NextMessage(out midiMsg);
			Assert.IsTrue(midiMsg.Span_Element_Equal(new MptSpan<uint8>([ 0xf0, 0x41, 0x10, 0x00, 0x10, 0x12, 0x10, 0x00, 0x04, 0x00, 0x02, 0x6a, 0xf7 ])));

			Assert.IsFalse(rawParser.NextMessage(out midiMsg));
		}
	}
}
