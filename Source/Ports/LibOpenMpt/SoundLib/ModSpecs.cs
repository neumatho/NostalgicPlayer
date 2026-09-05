/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// 
	/// </summary>
	internal static class ModSpecs
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly CModSpecifications mptm = new CModSpecifications
		{
			InternalType = ModType.Mpt,
			FileExtension = "mptm".ToCharPointer(),
			NoteMin = ModCommand.Note_Min,
			NoteMax = ModCommand.Note_Min + 119,
			PatternsMax = 4000,
			OrdersMax = 4000,
			SequencesMax = Snd_Def.Max_Sequences,
			ChannelsMin = 1,
			ChannelsMax = 127,
			TempoMinInt = 32,
			TempoMaxInt = 1000,
			SpeedMin = 1,
			SpeedMax = 255,
			PatternRowsMin = 1,
			PatternRowsMax = 1024,
			ModNameLengthMax = 25,
			SampleNameLengthMax = 25,
			SampleFileNameLengthMax = 12,
			InstrNameLengthMax = 25,
			InstrFileNameLengthMax = 12,
			CommentLineLengthMax = 0,
			SamplesMax = 3999,
			InstrumentsMax = 255,
			DefaultMixLevels = MixLevels.v1_17RC3,
			SongFlags = SongFlags.LinearSlides | SongFlags.ExFilterRange | SongFlags.ItOldEffects | SongFlags.ItCompatGxx,
			MidiMappingDirectivesMax = 200,
			EnvelopePointsMax = Snd_Def.Max_EnvPoints,
			HasNoteCut = true,
			HasNoteOff = true,
			HasNoteFade = true,
			HasReleaseNode = true,
			HasComments = true,
			HasIgnoreIndex = true,
			HasStopIndex = true,
			HasRestartPos = true,
			SupportsPlugins = true,
			HasPatternSignatures = true,
			HasPatternNames = true,
			HasArtistName = true,
			HasDefaultResampling = true,
			HasFractionalTempo = true,
			Commands = @" JFEGHLKRXODB?CQATI?SMNVW?UY?P?Z\:#+*?????????????????????".ToCharPointer(),
			VolCommands = " vpcdabuh??gfe?o".ToCharPointer()
		};



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly CModSpecifications mod = new CModSpecifications
		{
			InternalType = ModType.Mod,
			FileExtension = "mod".ToCharPointer(),
			NoteMin = 25,
			NoteMax = 108,
			PatternsMax = 128,
			OrdersMax = 128,
			SequencesMax = 1,
			ChannelsMin = 1,
			ChannelsMax = 99,
			TempoMinInt = 32,
			TempoMaxInt = 255,
			SpeedMin = 1,
			SpeedMax = 31,
			PatternRowsMin = 64,
			PatternRowsMax = 64,
			ModNameLengthMax = 20,
			SampleNameLengthMax = 22,
			SampleFileNameLengthMax = 0,
			InstrNameLengthMax = 0,
			InstrFileNameLengthMax = 0,
			CommentLineLengthMax = 0,
			SamplesMax = 31,
			InstrumentsMax = 0,
			DefaultMixLevels = MixLevels.Compatible,
			SongFlags = SongFlags.Pt_Mode | SongFlags.AmigaLimits | SongFlags.IsAmiga | SongFlags.Format_No_VolCol,
			MidiMappingDirectivesMax = 0,
			EnvelopePointsMax = 0,
			HasNoteCut = false,
			HasNoteOff = false,
			HasNoteFade = false,
			HasReleaseNode = false,
			HasComments = false,
			HasIgnoreIndex = false,
			HasStopIndex = false,
			HasRestartPos = true,
			SupportsPlugins = false,
			HasPatternSignatures = false,
			HasPatternNames = false,
			HasArtistName = false,
			HasDefaultResampling = false,
			HasFractionalTempo = false,
			Commands = @" 0123456789ABCD?FF?E??????????????????????????????????????".ToCharPointer(),
			VolCommands = " ???????????????".ToCharPointer()
		};



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly CModSpecifications xm = new CModSpecifications
		{
			InternalType = ModType.Xm,
			FileExtension = "xm".ToCharPointer(),
			NoteMin = 13,
			NoteMax = 108,
			PatternsMax = 256,
			OrdersMax = 255,
			SequencesMax = 1,
			ChannelsMin = 1,
			ChannelsMax = 32,
			TempoMinInt = 32,
			TempoMaxInt = 1000,
			SpeedMin = 1,
			SpeedMax = 31,
			PatternRowsMin = 1,
			PatternRowsMax = 256,
			ModNameLengthMax = 20,
			SampleNameLengthMax = 22,
			SampleFileNameLengthMax = 0,
			InstrNameLengthMax = 22,
			InstrFileNameLengthMax = 0,
			CommentLineLengthMax = 0,
			SamplesMax = 128 * 16,
			InstrumentsMax = 128,
			DefaultMixLevels = MixLevels.CompatibleFT2,
			SongFlags = SongFlags.LinearSlides,
			MidiMappingDirectivesMax = 0,
			EnvelopePointsMax = 12,
			HasNoteCut = false,
			HasNoteOff = true,
			HasNoteFade = false,
			HasReleaseNode = false,
			HasComments = false,
			HasIgnoreIndex = false,
			HasStopIndex = false,
			HasRestartPos = true,
			SupportsPlugins = false,
			HasPatternSignatures = false,
			HasPatternNames = false,
			HasArtistName = false,
			HasDefaultResampling = false,
			HasFractionalTempo = false,
			Commands = @" 0123456789ABCDRFFTE???GHK??XPL??????W????????????????????".ToCharPointer(),
			VolCommands = " vpcdabuhlrg????".ToCharPointer()
		};



		/********************************************************************/
		/// <summary>
		/// XM with MPT extensions
		/// </summary>
		/********************************************************************/
		public static readonly CModSpecifications xmEx = new CModSpecifications
		{
			InternalType = ModType.Xm,
			FileExtension = "xm".ToCharPointer(),
			NoteMin = 13,
			NoteMax = 108,
			PatternsMax = 256,
			OrdersMax = 255,
			SequencesMax = 1,
			ChannelsMin = 1,
			ChannelsMax = 128,
			TempoMinInt = 32,
			TempoMaxInt = 1000,
			SpeedMin = 1,
			SpeedMax = 31,
			PatternRowsMin = 1,
			PatternRowsMax = 1024,
			ModNameLengthMax = 20,
			SampleNameLengthMax = 22,
			SampleFileNameLengthMax = 0,
			InstrNameLengthMax = 22,
			InstrFileNameLengthMax = 0,
			CommentLineLengthMax = 0,
			SamplesMax = Snd_Def.Max_Samples - 1,
			InstrumentsMax = 255,
			DefaultMixLevels = MixLevels.CompatibleFT2,
			SongFlags = SongFlags.LinearSlides | SongFlags.ExFilterRange,
			MidiMappingDirectivesMax = 200,
			EnvelopePointsMax = 12,
			HasNoteCut = false,
			HasNoteOff = true,
			HasNoteFade = false,
			HasReleaseNode = false,
			HasComments = true,
			HasIgnoreIndex = false,
			HasStopIndex = false,
			HasRestartPos = true,
			SupportsPlugins = true,
			HasPatternSignatures = false,
			HasPatternNames = true,
			HasArtistName = true,
			HasDefaultResampling = false,
			HasFractionalTempo = false,
			Commands = @" 0123456789ABCDRFFTE???GHK?YXPLZ\?#??W????????????????????".ToCharPointer(),
			VolCommands = " vpcdabuhlrg????".ToCharPointer()
		};



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly CModSpecifications s3m = new CModSpecifications
		{
			InternalType = ModType.S3M,
			FileExtension = "s3m".ToCharPointer(),
			NoteMin = 13,
			NoteMax = 108,
			PatternsMax = 100,
			OrdersMax = 255,
			SequencesMax = 1,
			ChannelsMin = 1,
			ChannelsMax = 32,
			TempoMinInt = 33,
			TempoMaxInt = 255,
			SpeedMin = 1,
			SpeedMax = 255,
			PatternRowsMin = 64,
			PatternRowsMax = 64,
			ModNameLengthMax = 27,
			SampleNameLengthMax = 27,
			SampleFileNameLengthMax = 12,
			InstrNameLengthMax = 0,
			InstrFileNameLengthMax = 0,
			CommentLineLengthMax = 0,
			SamplesMax = 99,
			InstrumentsMax = 0,
			DefaultMixLevels = MixLevels.Compatible,
			SongFlags = SongFlags.FastVolSlides | SongFlags.AmigaLimits | SongFlags.S3MOldVibrato,
			MidiMappingDirectivesMax = 0,
			EnvelopePointsMax = 0,
			HasNoteCut = true,
			HasNoteOff = false,
			HasNoteFade = false,
			HasReleaseNode = false,
			HasComments = false,
			HasIgnoreIndex = true,
			HasStopIndex = true,
			HasRestartPos = false,
			SupportsPlugins = false,
			HasPatternSignatures = false,
			HasPatternNames = false,
			HasArtistName = false,
			HasDefaultResampling = false,
			HasFractionalTempo = false,
			Commands = @" JFEGHLKRXODB?CQATI?SMNVW?U?????????? ????????????????????".ToCharPointer(),
			VolCommands = " vp?????????????".ToCharPointer()
		};



		/********************************************************************/
		/// <summary>
		/// S3M with MPT extensions
		/// </summary>
		/********************************************************************/
		public static readonly CModSpecifications s3mEx = new CModSpecifications
		{
			InternalType = ModType.S3M,
			FileExtension = "s3m".ToCharPointer(),
			NoteMin = 13,
			NoteMax = 108,
			PatternsMax = 100,
			OrdersMax = 255,
			SequencesMax = 1,
			ChannelsMin = 1,
			ChannelsMax = 32,
			TempoMinInt = 33,
			TempoMaxInt = 255,
			SpeedMin = 1,
			SpeedMax = 255,
			PatternRowsMin = 64,
			PatternRowsMax = 64,
			ModNameLengthMax = 27,
			SampleNameLengthMax = 27,
			SampleFileNameLengthMax = 12,
			InstrNameLengthMax = 0,
			InstrFileNameLengthMax = 0,
			CommentLineLengthMax = 0,
			SamplesMax = 99,
			InstrumentsMax = 0,
			DefaultMixLevels = MixLevels.Compatible,
			SongFlags = SongFlags.FastVolSlides | SongFlags.AmigaLimits,
			MidiMappingDirectivesMax = 0,
			EnvelopePointsMax = 0,
			HasNoteCut = true,
			HasNoteOff = false,
			HasNoteFade = false,
			HasReleaseNode = false,
			HasComments = false,
			HasIgnoreIndex = true,
			HasStopIndex = true,
			HasRestartPos = false,
			SupportsPlugins = false,
			HasPatternSignatures = false,
			HasPatternNames = false,
			HasArtistName = false,
			HasDefaultResampling = false,
			HasFractionalTempo = false,
			Commands = @" JFEGHLKRXODB?CQATI?SMNVW?UY?P?Z????? ????????????????????".ToCharPointer(),
			VolCommands = " vp?????????????".ToCharPointer()
		};



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly CModSpecifications it = new CModSpecifications
		{
			InternalType = ModType.It,
			FileExtension = "it".ToCharPointer(),
			NoteMin = 1,
			NoteMax = 120,
			PatternsMax = 200,
			OrdersMax = 256,
			SequencesMax = 1,
			ChannelsMin = 1,
			ChannelsMax = 64,
			TempoMinInt = 32,
			TempoMaxInt = 255,
			SpeedMin = 1,
			SpeedMax = 255,
			PatternRowsMin = 32,
			PatternRowsMax = 200,
			ModNameLengthMax = 25,
			SampleNameLengthMax = 25,
			SampleFileNameLengthMax = 12,
			InstrNameLengthMax = 25,
			InstrFileNameLengthMax = 12,
			CommentLineLengthMax = 75,
			SamplesMax = 99,
			InstrumentsMax = 99,
			DefaultMixLevels = MixLevels.Compatible,
			SongFlags = SongFlags.LinearSlides | SongFlags.ItOldEffects | SongFlags.ItCompatGxx,
			MidiMappingDirectivesMax = 0,
			EnvelopePointsMax = 25,
			HasNoteCut = true,
			HasNoteOff = true,
			HasNoteFade = true,
			HasReleaseNode = false,
			HasComments = true,
			HasIgnoreIndex = true,
			HasStopIndex = true,
			HasRestartPos = false,
			SupportsPlugins = false,
			HasPatternSignatures = false,
			HasPatternNames = false,
			HasArtistName = false,
			HasDefaultResampling = false,
			HasFractionalTempo = false,
			Commands = @" JFEGHLKRXODB?CQATI?SMNVW?UY?P?Z????? ????????????????????".ToCharPointer(),
			VolCommands = " vpcdab?h??gfe??".ToCharPointer()
		};



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly CModSpecifications itEx = new CModSpecifications
		{
			InternalType = ModType.It,
			FileExtension = "it".ToCharPointer(),
			NoteMin = 1,
			NoteMax = 120,
			PatternsMax = 240,
			OrdersMax = 256,
			SequencesMax = 1,
			ChannelsMin = 1,
			ChannelsMax = 127,
			TempoMinInt = 32,
			TempoMaxInt = 512,
			SpeedMin = 1,
			SpeedMax = 255,
			PatternRowsMin = 1,
			PatternRowsMax = 1024,
			ModNameLengthMax = 25,
			SampleNameLengthMax = 25,
			SampleFileNameLengthMax = 12,
			InstrNameLengthMax = 25,
			InstrFileNameLengthMax = 12,
			CommentLineLengthMax = 75,
			SamplesMax = 3999,
			InstrumentsMax = 255,
			DefaultMixLevels = MixLevels.Compatible,
			SongFlags = SongFlags.LinearSlides | SongFlags.ExFilterRange | SongFlags.ItOldEffects | SongFlags.ItCompatGxx,
			MidiMappingDirectivesMax = 200,
			EnvelopePointsMax = 25,
			HasNoteCut = true,
			HasNoteOff = true,
			HasNoteFade = true,
			HasReleaseNode = false,
			HasComments = true,
			HasIgnoreIndex = true,
			HasStopIndex = true,
			HasRestartPos = false,
			SupportsPlugins = true,
			HasPatternSignatures = false,
			HasPatternNames = true,
			HasArtistName = true,
			HasDefaultResampling = false,
			HasFractionalTempo = false,
			Commands = @" JFEGHLKRXODB?CQATI?SMNVW?UY?P?Z\?#?? ????????????????????".ToCharPointer(),
			VolCommands = " vpcdab?h??gfe??".ToCharPointer()
		};



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly array<CModSpecifications> Collection = new array<CModSpecifications>(
		[
			mptm, mod, s3m, s3mEx, xm, xmEx, it, itEx
		]);
	}
}
