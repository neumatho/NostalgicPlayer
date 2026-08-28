/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/

using System.Collections.Generic;

namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	///     How transpose is handled in the pattern data
	/// </summary>
	public enum NoteTransposeMode
	{
		/// <summary>
		///     No transpose support in this format
		/// </summary>
		NoTranspose,

		/// <summary>
		///     Has transpose, notes are raw (not transposed)
		/// </summary>
		NotesRaw,

		/// <summary>
		///     Has transpose, notes are already transposed
		/// </summary>
		NotesTransposed
	}

	/// <summary>
	///     Contains information about a module's pattern structure
	/// </summary>
	public class SongPatterns
	{
		/// <summary>
		///     Length of the song (number of positions)
		/// </summary>
		public required int SongLength
		{
			get;
			set;
		}

		/// <summary>
		///     Start position offset for subsongs (0-based absolute position in module).
		///     Default is 0 for modules without subsongs or first subsong.
		///     For subsongs that start at a different position, this indicates the offset.
		/// </summary>
		public int StartPosition
		{
			get;
			set;
		} = 0;

		/// <summary>
		///     Module format name (e.g., "ProTracker", "FastTracker", etc.)
		/// </summary>
		public required string ModuleFormat
		{
			get;
			set;
		}

		/// <summary>
		///     Module title
		/// </summary>
		public required string ModuleTitle
		{
			get;
			set;
		}

		/// <summary>
		///     Initial speed (ticks per row) - default 6
		/// </summary>
		public required int InitialSpeed
		{
			get;
			set;
		}

		/// <summary>
		///     Initial BPM (tempo) - default 125
		/// </summary>
		public required int? InitialBpm
		{
			get;
			set;
		}

		/// <summary>
		///     Whether this module format has a volume column
		/// </summary>
		public required bool HasVolumeColumn
		{
			get;
			set;
		}

		/// <summary>
		///     How transpose is handled in the pattern data
		/// </summary>
		public required NoteTransposeMode TransposeMode
		{
			get;
			set;
		} = NoteTransposeMode.NoTranspose;

		/// <summary>
		///     Whether this module format has track numbers per channel
		/// </summary>
		public required bool HasTrackNumber
		{
			get;
			set;
		}

		/// <summary>
		///     Number of characters per effect (e.g., 3 for "C40", 4 for "10FF").
		///     Must be consistent between calculated and recorded patterns.
		/// </summary>
		public required int EffectCharCount
		{
			get;
			set;
		}

		/// <summary>
		///     Song-Daten für jede Position
		///     Index = Song-Position, Wert = Pattern-Daten für diese Position
		/// </summary>
		public required List<SongPatternViewData> SongData
		{
			get;
			set;
		}

		/// <summary>
		///     Error message if pattern data could not be loaded.
		///     If set, the pattern viewer will display this error instead of pattern data.
		/// </summary>
		public string ErrorMessage
		{
			get;
			set;
		}

		/// <summary>
		///     Current subsong number (1-based for display)
		/// </summary>
		public int SubSongCurrent
		{
			get;
			set;
		} = 1;

		/// <summary>
		///     Total number of subsongs in the module
		/// </summary>
		public int SubSongTotal
		{
			get;
			set;
		} = 1;

		/// <summary>
		///     Whether the pattern viewer should automatically compress patterns
		///     for this format when CompressPatterns setting is Auto.
		///     Set to true for formats where patterns often have empty rows
		///     that can be skipped (e.g., SoundControl).
		/// </summary>
		public bool AutoCompress
		{
			get;
			set;
		} = false;

		/// <summary>
		///     Whether these are real patterns from the player. False for the
		///     empty placeholder the viewer builds for formats that provide no
		///     patterns (only channel columns and VU meters are shown).
		/// </summary>
		public bool HasPatterns
		{
			get;
			set;
		} = true;
	}
}