//---------------------------------------------------------------------------------------
// <copyright file="PatternRenderingHelpers.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern
{
	/// <summary>
	/// Result class for rolling pattern info
	/// </summary>
	internal class RollingPatternInfo
	{
		public SongPatternViewData SongPattern
		{
			get;
			set;
		}

		public int Row
		{
			get;
			set;
		}

		public bool IsAtBoundary
		{
			get;
			set;
		}

		public int SongPosition
		{
			get;
			set;
		}
	}

	/// <summary>
	/// Result of note transpose adjustment
	/// </summary>
	internal readonly struct AdjustedNoteResult
	{
		public SongPatternViewNote Note
		{
			get;
			init;
		}

		public byte? Octave
		{
			get;
			init;
		}
	}

	/// <summary>
	/// Static helper methods for pattern rendering - formatting and rolling patterns
	/// </summary>
	internal static class PatternRenderingHelpers
	{
		private const int MinRowNumberDigits = 2;

		/********************************************************************/
		/// <summary>
		/// Get pattern and row for rolling patterns feature
		/// </summary>
		/********************************************************************/
		internal static RollingPatternInfo GetRollingPatternInfo(
			int displaySongPosition,
			int row,
			SongPatternViewData displaySongPattern,
			List<SongPatternViewData> songData,
			bool rollingPatterns)
		{
			if (!rollingPatterns || songData == null || songData.Count == 0)
			{
				return new RollingPatternInfo {SongPattern = displaySongPattern, Row = row, IsAtBoundary = false, SongPosition = displaySongPosition};
			}

			int patternRows = displaySongPattern != null ? displaySongPattern.RowCount : 64;
			int targetSongPos = displaySongPosition;
			int targetRow = row;
			bool isAtBoundary = false;

			// Check if row is before current pattern (negative row)
			while (targetRow < 0 && targetSongPos > 0)
			{
				targetSongPos--;
				if (targetSongPos >= 0 && targetSongPos < songData.Count)
				{
					SongPatternViewData prevPattern = songData[targetSongPos];
					if (prevPattern != null)
					{
						targetRow += prevPattern.RowCount;
						if (targetRow == 0)
						{
							isAtBoundary = true;
						}
					}
				}
				else
				{
					return new RollingPatternInfo {SongPattern = null, Row = 0, IsAtBoundary = false, SongPosition = targetSongPos};
				}
			}

			// Check if row is after current pattern
			while (targetRow >= patternRows && targetSongPos < songData.Count - 1)
			{
				targetRow -= patternRows;
				targetSongPos++;
				if (targetSongPos >= 0 && targetSongPos < songData.Count)
				{
					SongPatternViewData nextPattern = songData[targetSongPos];
					if (nextPattern != null)
					{
						patternRows = nextPattern.RowCount;
						if (targetRow == 0)
						{
							isAtBoundary = true;
						}
					}
				}
				else
				{
					return new RollingPatternInfo {SongPattern = null, Row = 0, IsAtBoundary = false, SongPosition = targetSongPos};
				}
			}

			// Get the target pattern
			if (targetSongPos >= 0 && targetSongPos < songData.Count)
			{
				SongPatternViewData targetPattern = songData[targetSongPos];
				if (targetPattern != null && targetRow >= 0 && targetRow < targetPattern.RowCount)
				{
					return new RollingPatternInfo {SongPattern = targetPattern, Row = targetRow, IsAtBoundary = isAtBoundary, SongPosition = targetSongPos};
				}
			}

			return new RollingPatternInfo {SongPattern = null, Row = 0, IsAtBoundary = false, SongPosition = targetSongPos};
		}

		/********************************************************************/
		/// <summary>
		/// Calculate the number of digits needed for row numbers based on max row count and format
		/// </summary>
		/// <param name="maxRowCount">Maximum row count across all patterns</param>
		/// <param name="format">Number format (hex or decimal)</param>
		/// <returns>Number of digits needed (minimum 2)</returns>
		/********************************************************************/
		internal static int CalculateRowNumberDigits(int maxRowCount, NumberFormat format)
		{
			int maxRowNumber = maxRowCount > 0 ? maxRowCount - 1 : 0;
			int digits = format == NumberFormat.Hexadecimal
				? maxRowNumber.ToString("X").Length
				: maxRowNumber.ToString().Length;
			return Math.Max(MinRowNumberDigits, digits);
		}

		/********************************************************************/
		/// <summary>
		/// Format row number for display
		/// </summary>
		/// <param name="row">Row number to format</param>
		/// <param name="format">Number format (decimal or hex)</param>
		/// <param name="maxRowCount">Maximum row count to determine padding</param>
		/// <param name="padWithSpace">Unused, kept for compatibility</param>
		/********************************************************************/
		internal static string FormatRowNumber(int row, NumberFormat format, int maxRowCount, bool padWithSpace = true)
		{
			int digits = CalculateRowNumberDigits(maxRowCount, format);

			if (format == NumberFormat.Hexadecimal)
			{
				return row.ToString("X").PadLeft(digits, '0');
			}

			return row.ToString().PadLeft(digits, '0');
		}

		/********************************************************************/
		/// <summary>
		/// Format pattern or track number for display
		/// </summary>
		/// <param name="number">Pattern or track number</param>
		/// <param name="trackPatternMode">Display mode for track/pattern numbers</param>
		/// <param name="rowNumberMode">Display mode for row numbers (used when trackPatternMode is Auto)</param>
		/// <param name="digits">Minimum number of digits (default 2)</param>
		/********************************************************************/
		internal static string FormatPatternNumber(int number, NumberDisplayMode trackPatternMode,
			NumberDisplayMode rowNumberMode, int digits = 2)
		{
			// Resolve Auto mode: use row number format
			bool useHex = trackPatternMode == NumberDisplayMode.Hexadecimal ||
			              (trackPatternMode == NumberDisplayMode.Auto && rowNumberMode == NumberDisplayMode.Hexadecimal);

			if (useHex)
			{
				return number.ToString("X").PadLeft(digits, '0');
			}

			return number.ToString().PadLeft(digits, '0');
		}

		/********************************************************************/
		/// <summary>
		/// Format speed display for status bar (speed * skip)
		/// </summary>
		/// <param name="speed">The speed value from player</param>
		/// <param name="skip">Skip factor for compressed patterns (default 1)</param>
		/********************************************************************/
		internal static string FormatSpeed(int speed, int skip = 1)
		{
			return $"{speed * skip:00}";
		}

		/********************************************************************/
		/// <summary>
		/// Format row display for status bar (currentRow/maxRow)
		/// Handles empty patterns and clamps row to valid range
		/// </summary>
		/// <param name="displayRow">Current row number (array index)</param>
		/// <param name="rowCount">Total row count of pattern (0 for empty pattern)</param>
		/// <param name="format">Number format (hex or decimal)</param>
		/// <param name="maxRowCount">Maximum row count across all patterns (for padding)</param>
		/// <param name="skip">Skip factor for compressed patterns (default 1)</param>
		/********************************************************************/
		internal static string FormatRowDisplay(int displayRow, int rowCount, NumberFormat format, int maxRowCount, int skip = 1)
		{
			// Empty pattern - show "---/---"
			if (rowCount <= 0)
			{
				return "---/---";
			}

			// Clamp displayRow to valid range (0 to rowCount-1)
			int safeRow = Math.Max(0, Math.Min(displayRow, rowCount - 1));

			// Apply skip factor for display
			// displayMax = original row count - 1 = (rowCount * skip) - 1
			int displayCurrent = safeRow * skip;
			int displayMax = (rowCount * skip) - 1;

			string currentRowStr = FormatRowNumber(displayCurrent, format, maxRowCount, false);
			string maxRowStr = FormatRowNumber(displayMax, format, maxRowCount, false);
			string skipStr = skip > 1 ? $" [x{(format == NumberFormat.Hexadecimal ? skip.ToString("X") : skip.ToString())}]" : "";
			return $"{currentRowStr}/{maxRowStr}{skipStr}";
		}

		/********************************************************************/
		/// <summary>
		/// Adjust note and octave based on transpose mode and user preference
		/// </summary>
		/********************************************************************/
		internal static AdjustedNoteResult AdjustNoteForTranspose(
			SongPatternViewNote note, byte? octave, sbyte? transpose,
			NoteTransposeMode transposeMode, bool showTransposedNotes)
		{
			// No adjustment needed if:
			// - No transpose value
			// - No transpose support
			// - Note is None or NoteOff
			// - Player already provides what user wants
			if (!transpose.HasValue || transpose.Value == 0 ||
			    transposeMode == NoteTransposeMode.NoTranspose ||
			    note == SongPatternViewNote.None || note == SongPatternViewNote.NoteOff ||
			    !octave.HasValue)
			{
				return new AdjustedNoteResult {Note = note, Octave = octave};
			}

			// Check if adjustment is needed
			bool playerProvidesTransposed = transposeMode == NoteTransposeMode.NotesTransposed;
			if (playerProvidesTransposed == showTransposedNotes)
			{
				// Player provides what user wants - no adjustment needed
				return new AdjustedNoteResult {Note = note, Octave = octave};
			}

			// Calculate adjustment
			int adjustment = showTransposedNotes ? transpose.Value : -transpose.Value;

			// Convert note+octave to semitone value
			int noteIndex = (int)note - 1; // C=0, C#=1, etc.
			int semitone = (octave.Value * 12) + noteIndex + adjustment;

			// Clamp to valid range (C-0 to B-9)
			if (semitone < 0)
			{
				semitone = 0;
			}

			if (semitone > 119) // 10 octaves * 12 semitones - 1
			{
				semitone = 119;
			}

			// Convert back to note+octave
			byte newOctave = (byte)(semitone / 12);
			int newNoteIndex = semitone % 12;
			SongPatternViewNote newNote = (SongPatternViewNote)(newNoteIndex + 1);

			return new AdjustedNoteResult {Note = newNote, Octave = newOctave};
		}

		/********************************************************************/
		/// <summary>
		/// Format note display with optional transpose adjustment
		/// </summary>
		/********************************************************************/
		internal static string FormatNoteDisplayTransposed(SongPatternViewNote note, byte? octave,
			sbyte? transpose, NoteTransposeMode transposeMode, bool showTransposedNotes)
		{
			AdjustedNoteResult adjusted = AdjustNoteForTranspose(note, octave, transpose, transposeMode, showTransposedNotes);
			return FormatNoteDisplay(adjusted.Note, adjusted.Octave);
		}

		/********************************************************************/
		/// <summary>
		/// Format note display from Note enum and Octave
		/// </summary>
		/********************************************************************/
		internal static string FormatNoteDisplay(SongPatternViewNote note, byte? octave)
		{
			if (note == SongPatternViewNote.None || !octave.HasValue)
			{
				return "   ";
			}

			if (note == SongPatternViewNote.NoteOff)
			{
				return "===";
			}

			string[] noteNames = new[] {"C-", "C#", "D-", "D#", "E-", "F-", "F#", "G-", "G#", "A-", "A#", "B-"};

			int noteIndex = (int)note - 1; // SongPatternViewNote.C = 1
			if (noteIndex < 0 || noteIndex >= 12)
			{
				return "???";
			}

			return $"{noteNames[noteIndex]}{octave.Value}";
		}

		/********************************************************************/
		/// <summary>
		/// Format a pattern channel for display
		/// </summary>
		/********************************************************************/
		internal static string FormatPatternEntry(SongPatternViewChannel channel, DisplayMode displayMode,
			bool hasVolumeColumn)
		{
			if (channel == null)
				// Empty channel - return empty string
			{
				return "";
			}

			// Convert Note + Octave to display string
			string note = FormatNoteDisplay(channel.Note, channel.Octave);

			// NotesOnly mode: only note
			if (displayMode == DisplayMode.NotesOnly)
			{
				return note;
			}

			string instrument = channel.Instrument.HasValue ? channel.Instrument.Value.ToString("X2") : "  ";

			// Compact mode: note and instrument
			if (displayMode == DisplayMode.Compact)
			{
				return $"{note} {instrument}";
			}

			// Full mode: all columns with spaces
			string result = $"{note} {instrument}";

			// Volume column (for XM/IT formats)
			if (hasVolumeColumn)
			{
				string volume = channel.Volume.HasValue ? channel.Volume.Value.ToString("X2") : "  ";
				result += $" {volume}";
			}

			// Effect - already formatted by player
			if (channel.EffectText != null && channel.EffectText.Length > 0)
			{
				string effect = RenderEffectHelper.FormatEffectText(channel.EffectText);
				result += $" {effect}";
			}

			return result;
		}

		/********************************************************************/
		/// <summary>
		/// Format the title text with song name, player name, and pause status
		/// </summary>
		/********************************************************************/
		internal static string FormatTitleText(string songTitle, string playerName, bool isPaused,
			string fileName = null, string prefix = null, int subSongCurrent = 1, int subSongTotal = 1)
		{
			string titleText;

			// Build subsong suffix if there are multiple subsongs
			string subSongSuffix = subSongTotal > 1 ? $" [{subSongCurrent}/{subSongTotal}]" : "";

			if (string.IsNullOrEmpty(songTitle))
			{
				if (string.IsNullOrEmpty(fileName))
				{
					titleText = "No Module Loaded";
				}
				else
				{
					// Extract only filename without path
					string fileNameOnly = Path.GetFileName(fileName);
					titleText = string.IsNullOrEmpty(playerName)
						? $"{fileNameOnly}{subSongSuffix}"
						: $"{fileNameOnly}{subSongSuffix} ({playerName})";
				}
			}
			else
			{
				titleText = string.IsNullOrEmpty(playerName)
					? $"{songTitle}{subSongSuffix}"
					: $"{songTitle}{subSongSuffix} ({playerName})";
			}

			if (isPaused)
			{
				titleText += " [PAUSED]";
			}

			// Add optional prefix (e.g., "Name: ")
			if (!string.IsNullOrEmpty(prefix))
			{
				titleText = prefix + titleText;
			}

			return titleText;
		}
	}
}
