/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/

using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Agent.Player.DavidWhittaker.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Helpers;

namespace Polycode.NostalgicPlayer.Agent.Player.DavidWhittaker
{
	/// <summary>
	/// Handles pattern recording during duration calculation
	/// </summary>
	internal class DavidWhittakerRecorder : PatternRecorder
	{
		/********************************************************************/
		/// <summary>
		/// Start a new song
		/// </summary>
		/********************************************************************/
		public void StartSong(int songNumber, string songName, int speed, int channelCount)
		{
			StartSong(new StartSongInput
			{
				SubSong = songNumber,
				Format = "David Whittaker",
				Title = songName,
				Speed = speed,
				Bpm = null,
				ChannelCount = channelCount,
				HasVolume = false,
				TransposeMode = NoteTransposeMode.NoTranspose,
				HasTrackNumber = true,
				EffectCharCount = 3,
				HasPatternNumber = false
			});
		}

		/********************************************************************/
		/// <summary>
		/// Record a row
		/// </summary>
		/********************************************************************/
		public void RecordRow(int rowNumber, ChannelInfo[] channels, Dictionary<uint, int> trackNumbers)
		{
			if (!IsRecording)
				return;

			RecordRow(CreateRow(rowNumber, channels, trackNumbers));
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Create a SongPatternViewRow from current playback state
		/// </summary>
		/********************************************************************/
		private SongPatternViewRow CreateRow(int rowNumber, ChannelInfo[] channels, Dictionary<uint, int> trackNumbers)
		{
			int channelCount = channels.Length;

			SongPatternViewRow row = new() {RowNumber = rowNumber, Channels = new SongPatternViewChannel[channelCount]};

			for (int channel = 0; channel < channelCount; channel++)
			{
				var channelInfo = channels[channel];
				byte note = channelInfo.Note;

				// Get instrument number from current sample
				byte? instrument = null;
				if (note != 0 && channelInfo.CurrentSampleInfo != null)
					instrument = (byte)(channelInfo.CurrentSampleInfo.SampleNumber + 1);

				// Get track number for this channel
				int? trackNumber = null;
				int? playingPosition = null;
				if (channelInfo.PositionList != null && channelInfo.CurrentPosition > 0 && channelInfo.CurrentPosition <= channelInfo.PositionList.Length)
				{
					uint trackOffset = channelInfo.PositionList[channelInfo.CurrentPosition - 1];
					if (trackNumbers.ContainsKey(trackOffset))
						trackNumber = trackNumbers[trackOffset];

					playingPosition = channelInfo.CurrentPosition - 1;
				}

				// Build effect text
				List<string> effects = new();

				if (channelInfo.SlideEnabled)
					effects.Add($"S{Math.Abs(channelInfo.SlideSpeed):X2}");

				if (channelInfo.VibratoSpeed > 0)
					effects.Add($"V{channelInfo.VibratoSpeed:X2}");

				if (channelInfo.ArpeggioList != null && channelInfo.ArpeggioListPosition > 0)
					effects.Add("A00");

				(var noteEnum, byte? octave) = NoteToEnum(note);

				row.Channels[channel] = new SongPatternViewChannel
				{
					Note = noteEnum,
					Octave = octave,
					Instrument = instrument,
					Volume = null,
					EffectText = effects.ToArray(),
					TrackNumber = trackNumber,
					PlayingPosition = playingPosition,
					DebugInfo = $"Tick={CurrentTick}"
				};
			}

			return row;
		}

		/********************************************************************/
		/// <summary>
		/// Convert note byte to SongPatternViewNote enum and octave
		/// </summary>
		/********************************************************************/
		private static (SongPatternViewNote, byte?) NoteToEnum(byte note)
		{
			if (note == 0)
				return (SongPatternViewNote.None, null);

			int noteIndex = (note - 1) % 12;
			int octave = (note - 1) / 12;

			var noteEnum = noteIndex switch
			{
				0 => SongPatternViewNote.C,
				1 => SongPatternViewNote.Cis,
				2 => SongPatternViewNote.D,
				3 => SongPatternViewNote.Dis,
				4 => SongPatternViewNote.E,
				5 => SongPatternViewNote.F,
				6 => SongPatternViewNote.Fis,
				7 => SongPatternViewNote.G,
				8 => SongPatternViewNote.Gis,
				9 => SongPatternViewNote.A,
				10 => SongPatternViewNote.Ais,
				11 => SongPatternViewNote.B,
				_ => SongPatternViewNote.None
			};

			return (noteEnum, (byte)octave);
		}
		#endregion
	}
}
