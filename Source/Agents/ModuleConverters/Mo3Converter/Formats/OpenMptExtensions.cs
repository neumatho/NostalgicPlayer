/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers.Chunks;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers.Plugins;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Formats
{
	/// <summary>
	/// Helper methods to write OpenMPT specific extensions into IT and XM files
	/// </summary>
	internal static class OpenMptExtensions
	{
		private const int MacroLength = 32;
		private const int NumberOfGlobalMacros = 9;
		private const int NumberOfSfxMacros = 16;
		private const int NumberOfZxxMacros = 128;

		/// <summary>
		/// Size of the MIDI macro configuration structure
		/// </summary>
		public const int MidiConfigSize = (NumberOfGlobalMacros + NumberOfSfxMacros + NumberOfZxxMacros) * MacroLength;

		// Indexes into OpenMPT's PlayBehaviour enum (see Snd_defs.h). These
		// are stored in files, so the numbers never change
		private static readonly int[] itPlayBehaviours =
		[
			0,		// MSF_COMPATIBLE_PLAY
			7,		// kPeriodsAreHertz
			8,		// kTempoClamp
			9,		// kPerChannelGlobalVolSlide
			10,		// kPanOverride
			11,		// kITInstrWithoutNote
			12,		// kITVolColFinePortamento
			13,		// kITArpeggio
			14,		// kITOutOfRangeDelay
			15,		// kITPortaMemoryShare
			16,		// kITPatternLoopTargetReset
			17,		// kITFT2PatternLoop
			18,		// kITPingPongNoReset
			19,		// kITEnvelopeReset
			20,		// kITClearOldNoteAfterCut
			21,		// kITVibratoTremoloPanbrello
			22,		// kITTremor
			23,		// kITRetrigger
			24,		// kITMultiSampleBehaviour
			25,		// kITPortaTargetReached
			26,		// kITPatternLoopBreak
			28,		// kITSwingBehaviour
			29,		// kITNNAReset
			30,		// kITSCxStopsSample
			31,		// kITEnvelopePositionHandling
			32,		// kITPortamentoInstrument
			33,		// kITPingPongMode
			34,		// kITRealNoteMapping
			35,		// kITHighOffsetNoRetrig
			36,		// kITFilterBehaviour
			37,		// kITNoSurroundPan
			38,		// kITShortSampleRetrig
			39,		// kITPortaNoNote
			40,		// kITFT2DontResetNoteOffOnPorta
			41,		// kITVolColMemory
			42,		// kITPortamentoSwapResetsPos
			43,		// kITEmptyNoteMapSlot
			44,		// kITFirstTickHandling
			45,		// kITSampleAndHoldPanbrello
			46,		// kITClearPortaTarget
			47,		// kITPanbrelloHold
			48,		// kITPanningReset
			87,		// kITMultiSampleInstrumentNumber
			88,		// kRowDelayWithNoteDelay
			100,	// kITInstrWithNoteOffOldEffects
			102,	// kITDoNotOverrideChannelPan
			103,	// kITPatternLoopWithJumps
			104,	// kITDCTBehaviour
			115,	// kITPitchPanSeparation
			119,	// kITResetFilterOnPortaSmpChange
			120,	// kITInitialNoteMemory
			122,	// kITNoSustainOnPortamento
			123,	// kITEmptyNoteMapSlotIgnoreCell
			124,	// kITOffsetWithInstrNumber
			127,	// kITDoublePortamentoSlides
			132,	// kITCarryAfterNoteOff
			134,	// kITNoteCutWithPorta
			135,	// kITVolColNoSlidePropagation
			136,	// kITStoppedFilterEnvAtStart
			137		// kITCompatGxxCarryPortaWithIns

			// Not set in ModPlug mode: 27 = kITOffset, 50 = kITInstrWithNoteOff
		];

		private static readonly int[] xmPlayBehaviours =
		[
			0,		// MSF_COMPATIBLE_PLAY
			8,		// kTempoClamp
			9,		// kPerChannelGlobalVolSlide
			10,		// kPanOverride
			17,		// kITFT2PatternLoop
			40,		// kITFT2DontResetNoteOffOnPorta
			51,		// kFT2Arpeggio
			52,		// kFT2Retrigger
			53,		// kFT2VolColVibrato
			54,		// kFT2PortaNoNote
			55,		// kFT2KeyOff
			56,		// kFT2PanSlide
			58,		// kFT2RestrictXCommand
			59,		// kFT2RetrigWithNoteDelay
			60,		// kFT2SetPanEnvPos
			61,		// kFT2PortaIgnoreInstr
			62,		// kFT2VolColMemory
			63,		// kFT2LoopE60Restart
			64,		// kFT2ProcessSilentChannels
			65,		// kFT2ReloadSampleSettings
			66,		// kFT2PortaDelay
			67,		// kFT2Transpose
			68,		// kFT2PatternLoopWithJumps
			69,		// kFT2PortaTargetNoReset
			70,		// kFT2EnvelopeEscape
			71,		// kFT2Tremor
			72,		// kFT2OutOfRangeDelay
			74,		// kFT2PanWithDelayedNoteOff
			75,		// kFT2VolColDelay
			76,		// kFT2FinetunePrecision
			86,		// kFT2NoteOffFlags
			88,		// kRowDelayWithNoteDelay
			89,		// kFT2MODTremoloRampWaveform
			90,		// kFT2PortaUpDownMemory
			93,		// kFT2PanSustainRelease
			98,		// kFT2NoteDelayWithoutInstr
			112,	// kFT2PortaResetDirection
			129,	// kFT2AutoVibratoAbortSweep
			133		// kFT2OffsetMemoryRequiresNote

			// Not set: 4 = kFT2VolumeRamping (only for FT2 made files)
			// Not set in ModPlug mode: 57 = kFT2ST3OffsetOutOfRange, 73 = kFT2Periods
		];

		/********************************************************************/
		/// <summary>
		/// Split the OMPT chunk into the part which is stored before the
		/// instruments (pattern and channel names) and the part stored
		/// after the sample data (instrument and song extensions)
		/// </summary>
		/********************************************************************/
		public static (byte[] Head, byte[] Tail) SplitOmptChunk(OmptChunk omptChunk)
		{
			if (omptChunk == null)
				return ([], []);

			byte[] data = omptChunk.Data;
			int offset = 0;

			while ((offset + 8) <= data.Length)
			{
				string id = Encoding.Latin1.GetString(data, offset, 4);
				if ((id != "PNAM") && (id != "CNAM"))
					break;

				uint len = BitConverter.ToUInt32(data, offset + 4);
				if (len > data.Length - offset - 8)
					break;

				offset += 8 + (int)len;
			}

			return (data.AsSpan(0, offset).ToArray(), data.AsSpan(offset).ToArray());
		}



		/********************************************************************/
		/// <summary>
		/// Write all the mixer plugins and channel plugin assignments
		/// </summary>
		/********************************************************************/
		public static void WritePlugins(Mo3Module module, ConverterStream converterStream)
		{
			foreach (FxPlugin fxPlugin in module.FindPlugin<FxPlugin>())
			{
				int plugin = fxPlugin.Plugin;

				converterStream.Write_UINT8((byte)'F');
				converterStream.Write_UINT8(plugin >= 100 ? (byte)('0' + plugin / 100) : (byte)'X');
				converterStream.Write_UINT8((byte)('0' + (plugin / 10) % 10));
				converterStream.Write_UINT8((byte)('0' + plugin % 10));

				converterStream.Write_L_UINT32((uint)fxPlugin.Data.Length);
				converterStream.Write(fxPlugin.Data, 0, fxPlugin.Data.Length);
			}

			ChannelPlugin channelPlugin = module.FindPlugin<ChannelPlugin>().FirstOrDefault();
			if (channelPlugin != null)
			{
				converterStream.WriteMark("CHFX");
				converterStream.Write_L_UINT32((uint)(channelPlugin.Plugins.Length * 4));
				converterStream.WriteArray_L_UINT32s(channelPlugin.Plugins, channelPlugin.Plugins.Length);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Build the MIDI macro configuration the same way as OpenMPT does
		/// when loading the MO3 file
		/// </summary>
		/********************************************************************/
		public static byte[] BuildMidiConfig(Mo3Module module)
		{
			byte[] config = new byte[MidiConfigSize];

			MidiChunk midiChunk = module.FindChunk<MidiChunk>();
			if (midiChunk != null)
			{
				Array.Copy(midiChunk.Data, config, Math.Min(midiChunk.Data.Length, config.Length));
				return config;
			}

			const int SfxStart = NumberOfGlobalMacros;
			const int ZxxStart = NumberOfGlobalMacros + NumberOfSfxMacros;

			// OpenMPT default configuration
			SetMacro(config, 0, "FF");			// MIDI start
			SetMacro(config, 1, "FC");			// MIDI stop
			SetMacro(config, 3, "9c n v");		// Note on
			SetMacro(config, 4, "9c n 0");		// Note off
			SetMacro(config, 8, "Cc p");		// Program change

			SetMacro(config, SfxStart, "F0F000z");		// Z00-Z7F controls cutoff

			for (int i = 0; i < 16; i++)
				SetMacro(config, ZxxStart + i, $"F0F001{i * 8:X2}");	// Z80-Z8F controls resonance

			FileHeader header = module.Header;

			if (header.SfxMacros.Any(x => x != 0) || header.FixedMacros[1].Any(x => x != 0))
			{
				for (int i = 0; i < NumberOfSfxMacros; i++)
					SetMacro(config, SfxStart + i, header.SfxMacros[i] != 0 ? $"F0F0{header.SfxMacros[i] - 1:X2}z" : string.Empty);

				for (int i = 0; i < NumberOfZxxMacros; i++)
					SetMacro(config, ZxxStart + i, header.FixedMacros[1][i] != 0 ? $"F0F0{header.FixedMacros[1][i] - 1:X2}{header.FixedMacros[0][i]:X2}" : string.Empty);
			}

			return config;
		}



		/********************************************************************/
		/// <summary>
		/// Check if the play behaviour of old ModPlug versions need to be
		/// written in a song extension
		/// </summary>
		/********************************************************************/
		public static bool NeedModPlugBehaviour(Mo3Module module)
		{
			return ((module.Header.Flags & HeaderFlag.ModPlugMode) != 0) && (module.FindChunk<OmptChunk>() == null);
		}



		/********************************************************************/
		/// <summary>
		/// Write a song extension block with the play behaviour OpenMPT uses
		/// for MO3 files in ModPlug mode where the version is not known
		/// </summary>
		/********************************************************************/
		public static void WriteModPlugBehaviour(ModuleType moduleType, ConverterStream converterStream)
		{
			int[] behaviours = moduleType == ModuleType.Xm ? xmPlayBehaviours : itPlayBehaviours;

			byte[] bits = new byte[behaviours.Max() / 8 + 1];
			foreach (int behaviour in behaviours)
				bits[behaviour >> 3] |= (byte)(1 << (behaviour & 0x07));

			converterStream.WriteMark("STPM");

			// Field codes are stored reversed
			converterStream.WriteMark(".FSM");
			converterStream.Write_L_UINT16((ushort)bits.Length);
			converterStream.Write(bits, 0, bits.Length);

			if (moduleType == ModuleType.Xm)
			{
				// The XM loader marks a file with song extensions as made with
				// OpenMPT 1.17 and upgrades the play behaviour, unless a newer
				// version is stored. Use the version of OpenMPT this has been
				// modelled after
				converterStream.WriteMark("VWSL");
				converterStream.Write_L_UINT16(4);
				converterStream.Write_L_UINT32(0x01321102);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Write all the pattern and channel names
		/// </summary>
		/********************************************************************/
		public static void WriteOmptHead(Mo3Module module, ConverterStream converterStream)
		{
			byte[] head = SplitOmptChunk(module.FindChunk<OmptChunk>()).Head;
			converterStream.Write(head, 0, head.Length);
		}



		/********************************************************************/
		/// <summary>
		/// Write all the instrument and song extensions. If none is stored
		/// and the module is in ModPlug mode, the play behaviour is written
		/// </summary>
		/********************************************************************/
		public static void WriteOmptTail(Mo3Module module, ConverterStream converterStream)
		{
			if (NeedModPlugBehaviour(module))
				WriteModPlugBehaviour(module.ModuleType, converterStream);
			else
			{
				byte[] tail = SplitOmptChunk(module.FindChunk<OmptChunk>()).Tail;
				converterStream.Write(tail, 0, tail.Length);
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Store a single macro string
		/// </summary>
		/********************************************************************/
		private static void SetMacro(byte[] config, int index, string macro)
		{
			int offset = index * MacroLength;

			Array.Clear(config, offset, MacroLength);

			byte[] bytes = Encoding.ASCII.GetBytes(macro);
			Array.Copy(bytes, 0, config, offset, Math.Min(bytes.Length, MacroLength - 1));
		}
		#endregion
	}
}
