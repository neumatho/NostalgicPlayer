/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.ManiacsOfNoise.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.ManiacsOfNoise
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal partial class ManiacsOfNoiseWorker : ModulePlayerWithSubSongDurationAgentBase
	{
		private int playerVersion;

		private byte numberOfSubSongs;
		private int subSongsOffset;
		private int sampleSequenceTableOffset;

		private SongInfo[] subSongs;
		private Dictionary<ushort, ushort[]> positionLists;
		private SortedList<ushort, byte[]> trackLists;
		private Dictionary<ushort, byte[]> envelopes;
		private Dictionary<ushort, sbyte[]> vibratos;
		private Dictionary<ushort, byte[]> arpeggios;
		private Sample[] samples;
		private Dictionary<uint, sbyte[]> sampleData;
		private Dictionary<uint, ushort[]> waveforms;
		private Dictionary<uint, byte[]> sampleSequences;
		private ushort[] sampleSequenceTable;

		private GlobalPlayingInfo playingInfo;
		private VoiceInfo[] voices;

		private const int InfoPositionLine = 4;
		private const int InfoTrackLine = 5;
		private const int InfoSpeedLine = 6;

		#region Identify
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => [ "mon" ];



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			return CheckModule(fileInfo.ModuleStream);
		}
		#endregion

		#region Loading
		/********************************************************************/
		/// <summary>
		/// Will load the file into memory
		/// </summary>
		/********************************************************************/
		public override AgentResult Load(PlayerFileInfo fileInfo, out string errorMessage)
		{
			try
			{
				return LoadModule(fileInfo.ModuleStream, out errorMessage) ? AgentResult.Ok : AgentResult.Error;
			}
			catch (Exception)
			{
				Cleanup();
				throw;
			}
		}
		#endregion

		#region Initialization and cleanup
		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public override void CleanupPlayer()
		{
			Cleanup();

			base.CleanupPlayer();
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the current song
		/// </summary>
		/********************************************************************/
		public override bool InitSound(int songNumber, out string errorMessage)
		{
			if (!base.InitSound(songNumber, out errorMessage))
				return false;

			InitializeSound(songNumber);

			return true;
		}
		#endregion

		#region Playing
		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			PlayModule();

			if (!playingInfo.EnablePlaying)
			{
				OnEndReached();

				playingInfo.EnablePlaying = true;
				RestartSong();

				for (int i = 0; i < 4; i++)
					VirtualChannels[i].Mute();
			}
		}
		#endregion

		#region Information
		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public override SubSongInfo SubSongs => new SubSongInfo(subSongs.Length, 0);



		/********************************************************************/
		/// <summary>
		/// Returns all the samples available in the module. If none, null
		/// is returned
		/// </summary>
		/********************************************************************/
		public override IEnumerable<SampleInfo> Samples
		{
			get
			{
				foreach (Sample sample in samples)
				{
					// Build frequency table
					uint[] frequencies = new uint[10 * 12];

					for (int j = 0; j < Tables.Periods.Length; j++)
					{
						ushort period = (ushort)((Tables.Periods[j] * sample.PeriodMultiplier) / 1024);
						frequencies[(2 * 12) + j] = PeriodToFrequency(period);
					}

					SampleInfo sampleInfo = new SampleInfo
					{
						Name = string.Empty,
						Flags = SampleInfo.SampleFlag.None,
						Volume = (ushort)(sample.Volume * 4),
						Panning = -1,
						NoteFrequencies = frequencies
					};

					if (sample.Type == SampleType.Sample)
					{
						sampleInfo.Type = SampleInfo.SampleType.Sample;
						sampleInfo.Length = sample.Length * 2U;

						if (sampleInfo.Length > 0)
						{
							sampleInfo.Sample = sampleData[sample.DataOffset];

							if (sample.LoopStart == 0xffff)
							{
								sampleInfo.LoopStart = 0;
								sampleInfo.LoopLength = 0;
							}
							else
							{
								sampleInfo.LoopStart = sample.LoopStart;
								sampleInfo.LoopLength = sampleInfo.Length - sampleInfo.LoopStart;
								sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;
							}
						}
					}
					else
					{
						sampleInfo.Type = SampleInfo.SampleType.Synthesis;
						sampleInfo.Sample = null;
						sampleInfo.Length = 0;
						sampleInfo.LoopStart = 0;
						sampleInfo.LoopLength = 0;
					}

					yield return sampleInfo;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Returns the description and value on the line given. If the line
		/// is out of range, false is returned
		/// </summary>
		/********************************************************************/
		public override bool GetInformationString(int line, out string description, out string value)
		{
			// Find out which line to take
			switch (line)
			{
				// Number of positions
				case 0:
				{
					description = Resources.IDS_MON_INFODESCLINE0;
					value = FormatPositionLengths();
					break;
				}

				// Used tracks
				case 1:
				{
					description = Resources.IDS_MON_INFODESCLINE1;
					value = trackLists.Count.ToString();
					break;
				}

				// Used samples
				case 2:
				{
					description = Resources.IDS_MON_INFODESCLINE2;
					value = samples.Length.ToString();
					break;
				}

				// Used wave tables
				case 3:
				{
					description = Resources.IDS_MON_INFODESCLINE3;
					value = waveforms.Count.ToString();
					break;
				}

				// Playing positions
				case 4:
				{
					description = Resources.IDS_MON_INFODESCLINE4;
					value = FormatPositions();
					break;
				}

				// Playing tracks
				case 5:
				{
					description = Resources.IDS_MON_INFODESCLINE5;
					value = FormatTracks();
					break;
				}

				// Current speed
				case 6:
				{
					description = Resources.IDS_MON_INFODESCLINE6;
					value = FormatSpeed();
					break;
				}

				default:
				{
					description = null;
					value = null;

					return false;
				}
			}

			return true;
		}
		#endregion

		#region Duration calculation
		/********************************************************************/
		/// <summary>
		/// Initialize all internal structures when beginning duration
		/// calculation on a new sub-song
		/// </summary>
		/********************************************************************/
		protected override void InitDuration(int subSong)
		{
			InitializeSound(subSong);
		}



		/********************************************************************/
		/// <summary>
		/// Create a snapshot of all the internal structures and return it
		/// </summary>
		/********************************************************************/
		protected override ISnapshot CreateSnapshot()
		{
			return new Snapshot(playingInfo, voices);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize internal structures based on the snapshot given
		/// </summary>
		/********************************************************************/
		protected override bool SetSnapshot(ISnapshot snapshot, out string errorMessage)
		{
			errorMessage = string.Empty;

			// Start to make a clone of the snapshot
			Snapshot currentSnapshot = (Snapshot)snapshot;
			Snapshot clonedSnapshot = new Snapshot(currentSnapshot.PlayingInfo, currentSnapshot.Voices);

			playingInfo = clonedSnapshot.PlayingInfo;
			voices = clonedSnapshot.Voices;

			UpdateModuleInformation();

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound(int subSong)
		{
			SongInfo song = subSongs[subSong];

			AmigaFilter = song.UseFilter;

			playingInfo = new GlobalPlayingInfo
			{
				EnablePlaying = true,

				MasterVolume = song.MasterVolume,
				FadeStartVolume = song.MasterVolume,
				FadeIncrement = 0,

				CurrentSpeed = song.StartSpeed
			};


			voices = new VoiceInfo[4];

			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = new VoiceInfo
				{
					ChannelNumber = i,

					Playing = true,

					PositionList = song.PositionListOffsets[i] != 0 ? positionLists[song.PositionListOffsets[i]] : null,
					PositionListIndex = 0,

					TrackRepeatCount = 1,
					TrackNumber = 0,
					Track = null,
					TrackIndex = 0,

					FirstTickFlag = true,
					NoteTickCounter = 0,

					CurrentSample = null,
					SampleNumber = 0,
					EffectFlag = false,

					Volume = 0,
					FinalVolume = 0,

					Transpose = 0,
					CurrentNote = 0,
					NotePeriod = 0,
					FinalPeriod = 0,
					NoteDelay = 0,

					PortamentoTickCounter = 0,
					PortamentoDuration = 0,
					PortamentoStartPeriod = 0,
					PortamentoTick = 0,
					PortamentoCurrentPeriod = 0,
					PortamentoStep = 0,
					PortamentoStepLeftOver = 0,
					PortamentoDifference = 0,

					EnvelopeSpeedCounter = 0,
					EnvelopeIndex = 0,
					EnvelopeFlag = false,

					ArpeggioOffset = 0,
					ArpeggioSpeedCounter = 0,
					ArpeggioIndex = 0,
					ArpeggioDifference = 0,

					VibratoDelay = 0,
					VibratoSpeedCounter = 0,
					VibratoIndex = 0,
					VibratoValue = 0,

					WaveformSpeedCounter = 0,
					WaveformIndex = 0,
					WaveformLength = 0,

					SampleSequence = null,
					SampleSequenceToPlay = null,
					SampleSequenceLength = 0,
					SampleSequenceSpeedCounter = 0,
					SampleSequenceSpeed = 0,
					SampleSequenceIndex = 0,

					VoiceState = 0
				};

				voices[i] = voiceInfo;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			playingInfo = null;

			subSongs = null;
			positionLists = null;
			trackLists = null;
			envelopes = null;
			vibratos = null;
			arpeggios = null;
			samples = null;
			sampleData = null;
			waveforms = null;
			sampleSequences = null;
			sampleSequenceTable = null;
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current song position
		/// </summary>
		/********************************************************************/
		private void ShowPositions()
		{
			OnModuleInfoChanged(InfoPositionLine, FormatPositions());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current tracks
		/// </summary>
		/********************************************************************/
		private void ShowTracks()
		{
			OnModuleInfoChanged(InfoTrackLine, FormatTracks());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current speed
		/// </summary>
		/********************************************************************/
		private void ShowSpeed()
		{
			OnModuleInfoChanged(InfoSpeedLine, FormatSpeed());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with all dynamic values
		/// </summary>
		/********************************************************************/
		private void UpdateModuleInformation()
		{
			ShowPositions();
			ShowTracks();
			ShowSpeed();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the songs position lengths
		/// </summary>
		/********************************************************************/
		private string FormatPositionLengths()
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < 4; i++)
			{
				if (IsVoiceMuted(voices[i]))
					sb.Append("-, ");
				else
				{
					sb.Append(voices[i].PositionList.Length);
					sb.Append(", ");
				}
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing positions
		/// </summary>
		/********************************************************************/
		private string FormatPositions()
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < 4; i++)
			{
				if (IsVoiceMuted(voices[i]))
					sb.Append("-, ");
				else
				{
					sb.Append(voices[i].PositionListIndex);
					sb.Append(", ");
				}
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing tracks
		/// </summary>
		/********************************************************************/
		private string FormatTracks()
		{
			return FormatChannelTracks(4, i => IsVoiceMuted(voices[i]) ? "-" : voices[i].TrackNumber);
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the current speed
		/// </summary>
		/********************************************************************/
		private string FormatSpeed()
		{
			return playingInfo.CurrentSpeed.ToString();
		}
		#endregion
	}
}
