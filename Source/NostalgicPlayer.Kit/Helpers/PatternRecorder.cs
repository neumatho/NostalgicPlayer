/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Kit.Helpers
{
	/// <summary>
	/// Input parameters for starting a new song recording
	/// </summary>
	public struct StartSongInput
	{
		/// <summary>Subsong number</summary>
		public required int SubSong { get; init; }

		/// <summary>Module format name (e.g., "ProTracker", "TFMX")</summary>
		public required string Format { get; init; }

		/// <summary>Song title</summary>
		public string Title { get; init; }

		/// <summary>Initial speed (ticks per row)</summary>
		public required int Speed { get; init; }

		/// <summary>Initial BPM (null if not supported)</summary>
		public int? Bpm { get; init; }

		/// <summary>Number of channels</summary>
		public required int ChannelCount { get; init; }

		/// <summary>Whether format has volume column</summary>
		public bool HasVolume { get; init; }

		/// <summary>How transpose is handled in the pattern data</summary>
		public NoteTransposeMode TransposeMode { get; init; }

		/// <summary>Whether format has track numbers</summary>
		public bool HasTrackNumber { get; init; }

		/// <summary>Characters per effect (e.g., 3 for "C40", 4 for "10FF", 0 for no effects)</summary>
		public required int EffectCharCount { get; init; }

		/// <summary>Whether format has pattern numbers (false for Rec only players)</summary>
		public required bool HasPatternNumber { get; init; }
	}

	/// <summary>
	/// Helper class for recording pattern data during duration calculation.
	/// Used by players that need to capture actual playback data rather than
	/// pre-calculating patterns (e.g., DavidWhittaker, TFMX).
	/// </summary>
	public class PatternRecorder
	{
		private class RecordedRow
		{
			public int PatternNumber { get; set; }
			public SongPatternViewRow Row { get; set; }
			public int Tick { get; set; }
		}

		private class SubSongData
		{
			public List<RecordedRow> Rows { get; } = new();
			public int CurrentPattern { get; set; }

			public string Format { get; set; }
			public string Title { get; set; }
			public int Speed { get; set; }
			public int? Bpm { get; set; }
			public int ChannelCount { get; set; }
			public bool HasVolume { get; set; }
			public NoteTransposeMode TransposeMode { get; set; }
			public bool HasTrackNumber { get; set; }
			public int EffectCharCount { get; set; }
			public bool HasPatternNumber { get; set; }
		}

		private readonly Dictionary<int, SubSongData> subSongs = new();
		private int currentSubSong;
		private bool isRecording;
		private string moduleFilePath;
		private int currentTick;

		/********************************************************************/
		/// <summary>
		/// Whether recording is currently active
		/// </summary>
		/********************************************************************/
		public bool IsRecording => isRecording;



		/********************************************************************/
		/// <summary>
		/// The module file path for writing comparison files
		/// </summary>
		/********************************************************************/
		public string ModuleFilePath
		{
			get => moduleFilePath;
			set => moduleFilePath = value;
		}

		/********************************************************************/
		/// <summary>
		/// Number of recorded rows for current subsong
		/// </summary>
		/********************************************************************/
		public int RecordedRowCount => subSongs.TryGetValue(currentSubSong, out var data) ? data.Rows.Count : 0;

		/********************************************************************/
		/// <summary>
		/// Current tick value
		/// </summary>
		/********************************************************************/
		public int CurrentTick => currentTick;

		/********************************************************************/
		/// <summary>
		/// Start recording mode
		/// </summary>
		/********************************************************************/
		public void StartRecording()
		{
			if (isRecording)
				throw new InvalidOperationException("Recording is already active. Call StopRecording() first.");

			isRecording = true;
			currentTick = 0;
		}

		/********************************************************************/
		/// <summary>
		/// Start a new subsong for recording
		/// </summary>
		/********************************************************************/
		public void StartSong(StartSongInput input)
		{
			if (!isRecording)
				return;

			currentSubSong = input.SubSong;
			currentTick = 0;

			subSongs[input.SubSong] = new SubSongData
			{
				Format = input.Format,
				Title = input.Title,
				Speed = input.Speed,
				Bpm = input.Bpm,
				ChannelCount = input.ChannelCount,
				HasVolume = input.HasVolume,
				TransposeMode = input.TransposeMode,
				HasTrackNumber = input.HasTrackNumber,
				EffectCharCount = input.EffectCharCount,
				HasPatternNumber = input.HasPatternNumber
			};
		}

		/********************************************************************/
		/// <summary>
		/// Start a new pattern (increments pattern counter)
		/// </summary>
		/********************************************************************/
		public void NewPattern()
		{
			if (!isRecording)
				return;

			if (!subSongs.Any())
				throw new InvalidOperationException("StartSong must be called before recording patterns");

			subSongs[currentSubSong].CurrentPattern++;
		}

		/********************************************************************/
		/// <summary>
		/// Record a pattern row with current tick
		/// </summary>
		/// <param name="row">The row data to record</param>
		/********************************************************************/
		public void RecordRow(SongPatternViewRow row)
		{
			if (!isRecording || row == null)
				return;

			if (!subSongs.Any())
				throw new InvalidOperationException("StartSong must be called before recording patterns");

			var data = subSongs[currentSubSong];
			data.Rows.Add(new RecordedRow
			{
				PatternNumber = data.CurrentPattern,
				Row = row,
				Tick = currentTick
			});

		}

		/********************************************************************/
		/// <summary>
		/// Stop recording
		/// </summary>
		/********************************************************************/
		public void StopRecording()
		{
			if (!isRecording)
				throw new InvalidOperationException("Recording is not active. Call StartRecording() first.");

			isRecording = false;
			currentTick = 0;
		}

		/********************************************************************/
		/// <summary>
		/// Get the recorded song patterns for a subsong
		/// </summary>
		/********************************************************************/
		public SongPatterns GetSongPatterns(int subSong)
		{
			if (isRecording)
				throw new InvalidOperationException("Cannot retrieve patterns while recording is active. Call StopRecording() first.");

			if (!subSongs.TryGetValue(subSong, out var data))
				return null;

			// Group rows by pattern number
			var patternGroups = data.Rows
				.GroupBy(r => r.PatternNumber)
				.ToDictionary(g => g.Key, g => g.Select(r => r.Row).ToArray());

			List<SongPatternViewData> songData = new();

			// Iterate from 0 to CurrentPattern to include empty patterns
			for (int i = 0; i <= data.CurrentPattern; i++)
			{
				if (patternGroups.TryGetValue(i, out var rows))
				{
					// Pattern has rows
					songData.Add(new SongPatternViewData
					{
						PatternNumber = data.HasPatternNumber ? i : null,
						RowCount = rows.Length,
						ChannelCount = data.ChannelCount,
						Rows = rows
					});
				}
				else
				{
					// Empty pattern (e.g., EFFE command)
					songData.Add(new SongPatternViewData
					{
						PatternNumber = data.HasPatternNumber ? i : null,
						RowCount = 0,
						ChannelCount = data.ChannelCount,
						Rows = Array.Empty<SongPatternViewRow>()
					});
				}
			}

			if (songData.Count == 0)
				return null;

			return new SongPatterns
			{
				SongLength = songData.Count,
				StartPosition = 0,
				ModuleFormat = data.Format ?? string.Empty,
				ModuleTitle = data.Title ?? string.Empty,
				InitialSpeed = data.Speed,
				InitialBpm = data.Bpm,
				HasVolumeColumn = data.HasVolume,
				TransposeMode = data.TransposeMode,
				HasTrackNumber = data.HasTrackNumber,
				EffectCharCount = data.EffectCharCount,
				SongData = songData
			};
		}

		/********************************************************************/
		/// <summary>
		/// Increment the tick counter. Call this in Play()
		/// </summary>
		/********************************************************************/
		public void Tick()
		{
			currentTick++;
		}

		/********************************************************************/
		/// <summary>
		/// Reset the tick counter. Call this when song loops
		/// </summary>
		/********************************************************************/
		public void ResetTick()
		{
			currentTick = 0;
		}

		/********************************************************************/
		/// <summary>
		/// Set the tick counter. Call this when seeking/spooling
		/// </summary>
		/********************************************************************/
		public void SetTick(int tick)
		{
			currentTick = tick;
		}


		/********************************************************************/
		/// <summary>
		/// Get row index from current playback tick.
		/// Returns 0 if no data recorded or tick is before first row.
		/// </summary>
		/********************************************************************/
		public int GetRowFromTick(int subSong)
		{
			if (!subSongs.TryGetValue(subSong, out var data) || data.Rows.Count == 0)
				return 0;

			// Find last row with tick <= currentTick
			for (int i = data.Rows.Count - 1; i >= 0; i--)
			{
				if (data.Rows[i].Tick <= currentTick)
					return i;
			}

			return 0;
		}

		/********************************************************************/
		/// <summary>
		/// Check if tick-based row lookup is available for a subsong
		/// </summary>
		/********************************************************************/
		public bool HasTickMapping(int subSong)
		{
			return subSongs.TryGetValue(subSong, out var data) && data.Rows.Count > 0;
		}

		/********************************************************************/
		/// <summary>
		/// Clear all recorded data
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			isRecording = false;
			subSongs.Clear();
		}

	}
}
