/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Mod specifications characterise the features of every editable module format
	/// in OpenMPT, such as the number of supported channels, samples, effects, etc...
	/// </summary>
	internal class CModSpecifications
	{
		/// <summary>
		/// Internal MODTYPE value
		/// </summary>
		public required ModType InternalType { get; init; }

		/// <summary>
		/// File extension without dot (encoded in ASCII)
		/// </summary>
		public required CPointer<char> FileExtension { get; init; }

		/// <summary>
		/// Minimum note index (index starts from 1)
		/// </summary>
		public required ModCommandNote NoteMin { get; init; }

		/// <summary>
		/// Maximum note index (index starts from 1)
		/// </summary>
		public required ModCommandNote NoteMax { get; init; }

		/// <summary>
		/// 
		/// </summary>
		public required PatternIndex PatternsMax { get; init; }

		/// <summary>
		/// 
		/// </summary>
		public required OrderIndex OrdersMax { get; init; }

		/// <summary>
		/// 
		/// </summary>
		public required SequenceIndex SequencesMax { get; init; }

		/// <summary>
		/// Minimum number of editable channels in pattern
		/// </summary>
		public required ChannelIndex ChannelsMin { get; init; }

		/// <summary>
		/// Maximum number of editable channels in pattern
		/// </summary>
		public required ChannelIndex ChannelsMax { get; init; }

		/// <summary>
		/// 
		/// </summary>
		public required uint32 TempoMinInt { get; init; }

		/// <summary>
		/// 
		/// </summary>
		public required uint32 TempoMaxInt { get; init; }

		/// <summary>
		/// Minimum ticks per frame
		/// </summary>
		public required uint32 SpeedMin { get; init; }

		/// <summary>
		/// Maximum ticks per frame
		/// </summary>
		public required uint32 SpeedMax { get; init; }

		/// <summary>
		/// 
		/// </summary>
		public required RowIndex PatternRowsMin { get; init; }

		/// <summary>
		/// 
		/// </summary>
		public required RowIndex PatternRowsMax { get; init; }

		/// <summary>
		/// Meaning 'usable letters', possible null character is not included
		/// </summary>
		public required uint16 ModNameLengthMax { get; init; }

		/// <summary>
		/// Ditto
		/// </summary>
		public required uint16 SampleNameLengthMax { get; init; }

		/// <summary>
		/// Ditto
		/// </summary>
		public required uint16 SampleFileNameLengthMax { get; init; }

		/// <summary>
		/// Ditto
		/// </summary>
		public required uint16 InstrNameLengthMax { get; init; }

		/// <summary>
		/// Ditto
		/// </summary>
		public required uint16 InstrFileNameLengthMax { get; init; }

		/// <summary>
		/// Not including line break, 0 for unlimited
		/// </summary>
		public required uint16 CommentLineLengthMax { get; init; }

		/// <summary>
		/// Max number of samples == Highest possible sample index
		/// </summary>
		public required SampleIndex SamplesMax { get; init; }

		/// <summary>
		/// Max number of instruments == Highest possible instrument index
		/// </summary>
		public required InstrumentIndex InstrumentsMax { get; init; }

		/// <summary>
		/// Default mix levels that are used when creating a new file in this format
		/// </summary>
		public required MixLevels DefaultMixLevels { get; init; }

		/// <summary>
		/// Supported song flags
		/// </summary>
		public required SongFlags SongFlags { get; init; }

		/// <summary>
		/// Number of MIDI Mapping directives that the format can store (0 = none)
		/// </summary>
		public required uint8 MidiMappingDirectivesMax { get; init; }

		/// <summary>
		/// Maximum number of points of each envelope
		/// </summary>
		public required uint8 EnvelopePointsMax { get; init; }

		/// <summary>
		/// True if format has note cut (^^)
		/// </summary>
		public required bool HasNoteCut { get; init; }

		/// <summary>
		/// True if format has note off (==)
		/// </summary>
		public required bool HasNoteOff { get; init; }

		/// <summary>
		/// True if format has note fade (~~)
		/// </summary>
		public required bool HasNoteFade { get; init; }

		/// <summary>
		/// Envelope release node
		/// </summary>
		public required bool HasReleaseNode { get; init; }

		/// <summary>
		/// True if format has a comments field
		/// </summary>
		public required bool HasComments { get; init; }

		/// <summary>
		/// Does "+++" pattern exist?
		/// </summary>
		public required bool HasIgnoreIndex { get; init; }

		/// <summary>
		/// Does "---" pattern exist?
		/// </summary>
		public required bool HasStopIndex { get; init; }

		/// <summary>
		/// Format has an automatic restart order position
		/// </summary>
		public required bool HasRestartPos { get; init; }

		/// <summary>
		/// Format can store plugins
		/// </summary>
		public required bool SupportsPlugins { get; init; }

		/// <summary>
		/// Can patterns have a custom time signature?
		/// </summary>
		public required bool HasPatternSignatures { get; init; }

		/// <summary>
		/// Can patterns have a name?
		/// </summary>
		public required bool HasPatternNames { get; init; }

		/// <summary>
		/// Can artist name be stored in file?
		/// </summary>
		public required bool HasArtistName { get; init; }

		/// <summary>
		/// Can default resampling be saved? (if not, it can still
		/// be modified in the GUI but won't set the module as modified)
		/// </summary>
		public required bool HasDefaultResampling { get; init; }

		/// <summary>
		/// Are fractional tempos allowed?
		/// </summary>
		public required bool HasFractionalTempo { get; init; }

		/// <summary>
		/// An array holding all commands this format supports;
		/// commands that are not supported are marked with "?"
		/// </summary>
		public required CPointer<char> Commands { get; init; }

		/// <summary>
		/// Ditto, but for volume column
		/// </summary>
		public required CPointer<char> VolCommands { get; init; }

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Tempo GetTempoMin()
		{
			return new Tempo(TempoMinInt, 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Tempo GetTempoMax()
		{
			return new Tempo(TempoMaxInt, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Return true if the format supports the given command
		/// </summary>
		/********************************************************************/
		public bool HasCommand(ModCommandCommand cmd)
		{
			return Commands[(c_int)cmd] != '?';
		}
	}
}
