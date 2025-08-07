/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Agent.Player.SoundControl.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SoundControl.Players.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SoundControl.Players
{
	/// <summary>
	/// Implementation of the SoundControl 3.x player
	/// </summary>
	internal class SoundControl3xPlayer : ISoundControlPlayer
	{
		private static readonly Dictionary<uint, ModuleInfo3x> specificModuleInfo = new Dictionary<uint, ModuleInfo3x>
		{
			// Domination 1
			{ 136612, new ModuleInfo3x
			{
				IsVersion32 = false,
				SongInfoList =
				[
					new SongInfo3x(0, 27),
					new SongInfo3x(31, 70)
				]
			}},
			// Number 9
			{ 126446, new ModuleInfo3x
			{
				IsVersion32 = false,
				SongInfoList =
				[
					new SongInfo3x(0, 40),
					new SongInfo3x(40, 60),
					new SongInfo3x(60, 80)
				]
			}},
			// Dynatsong
			{ 154704, new ModuleInfo3x
			{
				IsVersion32 = true,
				SongInfoList =
				[
					new SongInfo3x(0, 28),
					new SongInfo3x(28, 62)
				]
			}},
			// Eleven6
			{ 103808, new ModuleInfo3x
			{
				IsVersion32 = true
			}}
		};

		private readonly SoundControlWorker worker;

		private ushort[] periods;

		private readonly ModuleData moduleData;
		private readonly SongInfo3x[] songInfoList;
		private readonly bool isVersion32;

		private SongInfo3x currentSongInfo;
		private ushort maxSpeedCounter;

		private GlobalPlayingInfo3x playingInfo;
		private VoiceInfo3x[] voices;

		private bool endReached;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SoundControl3xPlayer(SoundControlWorker worker, ModuleData moduleData, uint totalLength)
		{
			this.worker = worker;
			this.moduleData = moduleData;

			if (specificModuleInfo.TryGetValue(totalLength, out ModuleInfo3x moduleInfo))
			{
				isVersion32 = moduleInfo.IsVersion32;
				songInfoList = moduleInfo.SongInfoList;
			}
			else
				isVersion32 = false;

			songInfoList ??= [ new SongInfo3x(0, (ushort)moduleData.PositionList.Positions[0].Length) ];
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public void InitPlayer()
		{
			CalculatePeriodTable();

			maxSpeedCounter = (ushort)(isVersion32 ? 2 : 3);
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the current song
		/// </summary>
		/********************************************************************/
		public void InitSound(int songNumber)
		{
			currentSongInfo = songInfoList[songNumber];

			playingInfo = new GlobalPlayingInfo3x
			{
				SongPosition = currentSongInfo.StartPosition,
				SpeedCounter = 0,
				SpeedCounter2 = 0
			};

			voices = new VoiceInfo3x[6];

			for (int i = 0; i < 6; i++)
			{
				voices[i] = new VoiceInfo3x
				{
					WaitCounter = 0,
					Track = null,
					TrackPosition = 0,
					Transpose = 0
				};
			}

			SetTracks();
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public bool Play()
		{
			endReached = false;

			playingInfo.SpeedCounter++;

			if (playingInfo.SpeedCounter == maxSpeedCounter)
			{
				ProcessCounter();
				playingInfo.SpeedCounter = 0;
			}

			ProcessCounter();

			return endReached;
		}



		/********************************************************************/
		/// <summary>
		/// Return the calculated period table
		/// </summary>
		/********************************************************************/
		public IEnumerable<ushort> PeriodTable
		{
			get
			{
				for (int i = 0; i < 8; i++)
				{
					for (int j = 0; j < 12; j++)
						yield return periods[i * 16 + j];
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of sub songs
		/// </summary>
		/********************************************************************/
		public int NumberOfSubSongs => songInfoList.Length;



		/********************************************************************/
		/// <summary>
		/// Return the number of positions
		/// </summary>
		/********************************************************************/
		public int NumberOfPositions => currentSongInfo.EndPosition - currentSongInfo.StartPosition;



		/********************************************************************/
		/// <summary>
		/// Return the current song position
		/// </summary>
		/********************************************************************/
		public ushort SongPosition => (ushort)(playingInfo.SongPosition - currentSongInfo.StartPosition);



		/********************************************************************/
		/// <summary>
		/// Get or set a snapshot
		/// </summary>
		/********************************************************************/
		public Snapshot Snapshot
		{
			get => new Snapshot(playingInfo, voices);

			set
			{
				playingInfo = (GlobalPlayingInfo3x)value.PlayingInfo;
				voices = value.Voices.Cast<VoiceInfo3x>().ToArray();
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Calculate the period table
		/// </summary>
		/********************************************************************/
		private void CalculatePeriodTable()
		{
			periods = new ushort[8 * 16];

			int index = 0;

			for (int i = 2; i < 10; i++)
			{
				for (int j = 0; j < 16; j++)
					periods[index++] = (ushort)(Tables.BasePeriod[j] >> i);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Process the next tick
		/// </summary>
		/********************************************************************/
		private void ProcessCounter()
		{
			playingInfo.SpeedCounter2++;

			if (playingInfo.SpeedCounter2 == 2)
			{
				ProcessTrack();
				playingInfo.SpeedCounter2 = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Process the track data
		/// </summary>
		/********************************************************************/
		private void ProcessTrack()
		{
			bool redo;

			do
			{
				redo = false;

				if (isVersion32)
					ProcessVoice6();

				for (int i = 0; i < 4; i++)
				{
					VoiceInfo3x voiceInfo = voices[i];

					if (voiceInfo.WaitCounter == 0)
					{
						byte[] track = voiceInfo.Track;
						int trackPosition = voiceInfo.TrackPosition;

						voiceInfo.WaitCounter = (ushort)(track[trackPosition + 1] - 1);
						voiceInfo.TrackPosition += 4;

						if (track[trackPosition] == 0xff)
						{
							playingInfo.SongPosition++;

							if (playingInfo.SongPosition == currentSongInfo.EndPosition)
							{
								playingInfo.SongPosition = currentSongInfo.StartPosition;
								endReached = true;
							}

							SetTracks();

							worker.ShowSongPosition();
							worker.ShowTracks();

							redo = true;
							break;
						}

						if (track[trackPosition] != 0x00)
						{
							byte note = track[trackPosition];
							byte sampleNumber = track[trackPosition + 2];
							byte volume = track[trackPosition + 3];

							if (isVersion32)
							{
								note = HandleTranspose(voiceInfo, note);

								if (sampleNumber == 0xff)
								{
									sampleNumber = 1;
									volume = 0;
									voiceInfo.WaitCounter = 0;
								}
							}

							if (volume != 0x80)
								PlaySample(worker.VirtualChannels[i], sampleNumber, note, volume);
						}
					}
					else
						voiceInfo.WaitCounter--;
				}
			}
			while (redo);
		}



		/********************************************************************/
		/// <summary>
		/// Processing of voice 6
		/// </summary>
		/********************************************************************/
		private void ProcessVoice6()
		{
			VoiceInfo3x voiceInfo = voices[5];

			byte[] track = voiceInfo.Track;
			int trackPosition = voiceInfo.TrackPosition;

			if (track[trackPosition] == 0xff)
				return;

			if (voiceInfo.WaitCounter == 0)
			{
				voiceInfo.WaitCounter = (ushort)(track[trackPosition + 1] - 1);
				voiceInfo.TrackPosition += 4;

				if (track[trackPosition] != 0x00)
				{
					byte dat1 = (byte)(track[trackPosition + 2] - 1);

					ushort dat2 = (ushort)(track[trackPosition + 3] & 0x3f);
					if ((dat2 & 0x40) != 0)
						dat2 |= 0xff10;

					if (dat1 == 4)
					{
						voices[0].Transpose = (sbyte)dat2;
						voices[1].Transpose = (sbyte)dat2;
						voices[2].Transpose = (sbyte)dat2;
						voices[3].Transpose = (sbyte)dat2;
					}

					int index = (~(dat1 & 3)) & 3;
					voices[index].Transpose = (sbyte)dat2;
				}
			}
			else
				voiceInfo.WaitCounter--;
		}



		/********************************************************************/
		/// <summary>
		/// Handling of transpose
		/// </summary>
		/********************************************************************/
		private byte HandleTranspose(VoiceInfo3x voiceInfo, byte note)
		{
			sbyte transpose = voiceInfo.Transpose;

			while (transpose != 0)
			{
				if (transpose > 0)
				{
					transpose--;
					note++;

					if ((note & 0x0f) == 12)
						note += 4;
				}
				else
				{
					transpose++;
					note--;

					if ((note & 0x0f) == 15)
						note -= 4;
				}
			}

			return note;
		}



		/********************************************************************/
		/// <summary>
		/// Find the tracks at the current song position
		/// </summary>
		/********************************************************************/
		private void SetTracks()
		{
			ushort songPosition = playingInfo.SongPosition;

			for (int i = 0; i < 6; i++)
			{
				VoiceInfo3x voiceInfo = voices[i];

				voiceInfo.WaitCounter = 0;

				Position positionInfo = moduleData.PositionList.Positions[i][songPosition];

				voiceInfo.Track = moduleData.Tracks[positionInfo.TrackNumber];
				voiceInfo.TrackPosition = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Play a new sample
		/// </summary>
		/********************************************************************/
		private void PlaySample(IChannel channel, byte sampleNumber, byte note, byte volume)
		{
			Sample sample = moduleData.Samples[sampleNumber];

			if (sample.NoteTranspose != 0)
			{
				for (int i = 0; i < sample.NoteTranspose; i++)
				{
					note++;

					// Since the first nibble is the octave, we have to check
					// if we are at the end of the octave and add some extra to the note
					if ((note & 0x0f) == 12)
						note += 4;
				}
			}

			ushort period = periods[note];
			channel.SetAmigaPeriod(period);

			channel.SetAmigaVolume(volume);

			channel.PlaySample(sampleNumber, sample.SampleData, 0, sample.Length);

			if (sample.LoopStart != 0)
				channel.SetLoop(sample.LoopStart, (uint)(sample.LoopEnd - sample.LoopStart));
		}
		#endregion
	}
}
