/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/

using System.Collections.Generic;
using Polycode.NostalgicPlayer.Agent.Player.ModTracker.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.ModTracker
{
	/// <summary>
	/// Helper class for converting ModTracker patterns to SongPatternViewData
	/// </summary>
	internal static class ModTrackerPatternHelper
	{
		private const byte NumberOfNotes = 5 * 12; // 5 octaves

		/********************************************************************/
		/// <summary>
		/// Create ModulePatternInfo with all song data
		/// </summary>
		/********************************************************************/
		public static SongPatterns CreateSongPatterns(byte[] positions, int songLength,
			ushort[,] sequences, TrackLine[][] tracks, int maxPattern, int patternLength,
			int channelNum, int trackNum, ModuleType currentModuleType, string songName,
			ushort initTempo, bool showTracks)
		{
			// Erstelle Song-Daten für jede Position
			List<SongPatternViewData> songData = new();
			for (int songPos = 0; songPos < songLength; songPos++)
			{
				// For MultiTracker, after conversion positions[i] = i, so we use songPos directly
				// when songPos exceeds the positions array bounds
				int patternNumber = songPos < positions.Length ? positions[songPos] : songPos;
				var songPatternData = GetPatternData(patternNumber, sequences, tracks,
					patternLength, channelNum, trackNum, showTracks);
				songData.Add(songPatternData);
			}

			return new SongPatterns
			{
				SongLength = songLength,
				ModuleFormat = currentModuleType.ToString(),
				ModuleTitle = songName,
				InitialSpeed = 6, // Default MOD speed
				InitialBpm = initTempo,
				HasVolumeColumn = false, // ProTracker MODs don't have volume column
				TransposeMode = NoteTransposeMode.NoTranspose,
				HasTrackNumber = showTracks,
				EffectCharCount = 3,
				SongData = songData
			};
		}

		/********************************************************************/
		/// <summary>
		/// Get pattern data by pattern number
		/// </summary>
		/********************************************************************/
		public static SongPatternViewData GetPatternData(int patternNumber, ushort[,] sequences,
			TrackLine[][] tracks, int patternLength, int channelNum, int trackNum, bool showTracks)
		{
			if (sequences == null || patternNumber < 0)
				return null;

			// For multitrack formats (IceTracker, SoundTracker 2.6), use per-row ChannelTracks
			// in SongRowChangeInfo instead of a single PatternNumber
			int? patternNum = showTracks ? null : patternNumber;

			SongPatternViewData patternData = new()
			{
				PatternNumber = patternNum,
				RowCount = patternLength,
				ChannelCount = channelNum,
				Rows = new SongPatternViewRow[patternLength]
			};

			for (int row = 0; row < patternLength; row++)
			{
				patternData.Rows[row] = new SongPatternViewRow
				{
					RowNumber = row, Channels = new SongPatternViewChannel[channelNum]
				};

				for (int channel = 0; channel < channelNum; channel++)
				{
					ushort trackNumber = sequences[channel, patternNumber];
					if (trackNumber < trackNum && tracks[trackNumber] != null && row < tracks[trackNumber].Length)
					{
						var trackLine = tracks[trackNumber][row];
						patternData.Rows[row].Channels[channel] =
							ConvertTrackLineToPatternEntry(trackLine, showTracks ? trackNumber : null);
					}
					else
						// Empty entry
						patternData.Rows[row].Channels[channel] = new SongPatternViewChannel
						{
							Note = SongPatternViewNote.None,
							Octave = null,
							Instrument = null,
							Volume = null,
							EffectText = [],
							TrackNumber = null,
							Transpose = null,
							PlayingPosition = null
						};
				}
			}

			return patternData;
		}

		/********************************************************************/
		/// <summary>
		/// Convert a track line to a pattern view entry
		/// </summary>
		/********************************************************************/
		private static SongPatternViewChannel ConvertTrackLineToPatternEntry(TrackLine trackLine, int? trackNumber)
		{
			// Note
			(var note, byte? octave) = NoteToEnum(trackLine.Note);

			// Instrument (ProTracker: 0 = no instrument)
			byte? instrument = trackLine.Sample == 0 ? null : trackLine.Sample;

			// Effect
			// Arpeggio (effect 0) with parameter 00 is not an effect
			string[] effects;
			if (trackLine.Effect == Effect.Arpeggio && trackLine.EffectArg == 0)
				effects = [];
			else
			{
				string effectChar = GetEffectChar(trackLine.Effect);
				string paramStr = trackLine.EffectArg.ToString("X2");
				effects = [$"{effectChar}{paramStr}"];
			}

			return new SongPatternViewChannel
			{
				Note = note,
				Octave = octave,
				Instrument = instrument,
				Volume = null,
				EffectText = effects,
				TrackNumber = trackNumber,
				Transpose = null,
				PlayingPosition = null
			};
		}

		/********************************************************************/
		/// <summary>
		/// Convert a note number to SongPatternViewNote enum and octave
		/// </summary>
		/********************************************************************/
		private static (SongPatternViewNote, byte?) NoteToEnum(byte note)
		{
			if (note == 0 || note > NumberOfNotes)
				return (SongPatternViewNote.None, null);

			SongPatternViewNote[] notes =
			[
				SongPatternViewNote.C, SongPatternViewNote.Cis, SongPatternViewNote.D, SongPatternViewNote.Dis,
				SongPatternViewNote.E, SongPatternViewNote.F, SongPatternViewNote.Fis, SongPatternViewNote.G,
				SongPatternViewNote.Gis, SongPatternViewNote.A, SongPatternViewNote.Ais, SongPatternViewNote.B
			];

			int octave = ((note - 1) / 12) + 1;
			int noteIndex = (note - 1) % 12;

			return (notes[noteIndex], (byte?)octave);
		}

		/********************************************************************/
		/// <summary>
		/// Get the effect character from an effect value
		/// </summary>
		/********************************************************************/
		private static string GetEffectChar(Effect effect)
		{
			return effect switch
			{
				Effect.Arpeggio => "0",
				Effect.SlideUp => "1",
				Effect.SlideDown => "2",
				Effect.TonePortamento => "3",
				Effect.Vibrato => "4",
				Effect.TonePort_VolSlide => "5",
				Effect.Vibrato_VolSlide => "6",
				Effect.Tremolo => "7",
				Effect.SampleOffset => "9",
				Effect.VolumeSlide => "A",
				Effect.PosJump => "B",
				Effect.SetVolume => "C",
				Effect.PatternBreak => "D",
				Effect.ExtraEffect => "E",
				Effect.SetSpeed => "F",
				_ => ((int)effect).ToString("X")
			};
		}

		/********************************************************************/
		/// <summary>
		/// Create SongRowChangeInfo for current playback state
		/// </summary>
		/********************************************************************/
		public static SongRowChangeInfo CreateRowChangeInfo(int songPos, int patternPos,
			byte[] positions, int speedEven, int speedOdd, int tempo, ushort[,] sequences, int channelNum,
			bool showTracks)
		{
			// For formats with per-channel tracks (IceTracker, SoundTracker 2.6), get track numbers
			uint?[] channelTracks = null;
			if (showTracks && sequences != null && songPos < positions.Length)
			{
				int patternNumber = positions[songPos];
				channelTracks = new uint?[channelNum];
				for (int channel = 0; channel < channelNum; channel++)
					channelTracks[channel] = sequences[channel, patternNumber];
			}

			return new SongRowChangeInfo
			{
				SongPosition = songPos,
				Row = patternPos,
				Speed = (patternPos & 1) == 0 ? speedEven : speedOdd,
				Bpm = tempo,
				ChannelTracks = channelTracks,
				ChannelPositions = null
			};
		}
	}
}