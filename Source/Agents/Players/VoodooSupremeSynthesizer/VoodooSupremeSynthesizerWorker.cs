/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Agent.Player.VoodooSupremeSynthesizer.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Sound;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.VoodooSupremeSynthesizer
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal partial class VoodooSupremeSynthesizerWorker : ModulePlayerWithSubSongDurationAgentBase
	{
		private SongInfo[] subSongs;
		private Waveform[] allSamples;

		private int numberOfTracks;
		private int numberOfSamples;
		private int numberOfWaveforms;

		private SongInfo currentSong;
		private VoiceInfo[] voices;
		private PaulaChannel[] channels;

		private Dictionary<int, TimeSpan>[] trackTimes;
		private bool endReached;

		#region Identify
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => [ "vss" ];



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
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public override bool InitPlayer(out string errorMessage)
		{
			if (!base.InitPlayer(out errorMessage))
				return false;

			allSamples = subSongs.SelectMany(x => x.Data).OfType<Waveform>().OrderBy(x => x.Offset).ToArray();

			numberOfTracks = subSongs.SelectMany(x => x.Data).OfType<TrackData>().Count();
			numberOfSamples = allSamples.OfType<Sample>().Count();
			numberOfWaveforms = allSamples.Length - numberOfSamples;

			trackTimes = ArrayHelper.InitializeArray<Dictionary<int, TimeSpan>>(4);

			return true;
		}



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

			if (endReached)
			{
				OnEndReachedOnAllChannels(0);
				SetRestartTime(voices.Select(x => x.RestartTime).Min()!.Value);
				endReached = false;
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
				// Build frequency table
				uint[] frequencies = new uint[10 * 12];

				for (int j = 0; j < 8 * 12; j++)
					frequencies[9 + j] = PeriodToFrequency(Tables.Periods[j]);

				foreach (Waveform waveform in allSamples)
				{
					SampleInfo sampleInfo = new SampleInfo
					{
						Name = string.Empty,
						Flags = SampleInfo.SampleFlag.None,
						Volume = 256,
						Panning = -1,
						LoopStart = 0,
						LoopLength = 0,
						NoteFrequencies = frequencies
					};

					if (waveform is Sample sample)
					{
						sampleInfo.Type = SampleInfo.SampleType.Sample;
						sampleInfo.Sample = sample.Data;
						sampleInfo.Length = sample.Length;
					}
					else
					{
						sampleInfo.Type = SampleInfo.SampleType.Synthesis;
						sampleInfo.Length = 0;
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
				// Used tracks
				case 0:
				{
					description = Resources.IDS_VSS_INFODESCLINE0;
					value = numberOfTracks.ToString();
					break;
				}

				// Used samples
				case 1:
				{
					description = Resources.IDS_VSS_INFODESCLINE1;
					value = numberOfSamples.ToString();
					break;
				}

				// Used wave tables
				case 2:
				{
					description = Resources.IDS_VSS_INFODESCLINE2;
					value = numberOfWaveforms.ToString();
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
			return new Snapshot(voices);
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
			Snapshot clonedSnapshot = new Snapshot(currentSnapshot.Voices);

			voices = clonedSnapshot.Voices;

			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = voices[i];
				PaulaChannel channel = channels[i];

				if (voiceInfo.SynthesisMode.HasFlag(SynthesisFlag.FrequencyMapped))
					channel.Length = 0x40;
				else
					channel.Length = 0x10;

				channel.SetAddress(-1, Tables.EmptySample.Data, 0, false);
			}

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
			currentSong = subSongs[subSong];

			voices = new VoiceInfo[4];
			channels = new PaulaChannel[4];

			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = new VoiceInfo
				{
					ChannelNumber = i,
					RestartTime = null,

					Sample1 = null,
					Sample1Offset = 0,
					Sample1Number = -1,
					Sample2 = null,
					Sample2Number = -1,

					AudioBuffer = ArrayHelper.Initialize2Arrays<sbyte>(2, 32),
					UseAudioBuffer = 0,

					Track = currentSong.Tracks[i],
					TrackPosition = 0,
					TickCounter = 1,

					NewNote = false,
					Transpose = 0,
					NotePeriod = 0,
					TargetPeriod = 0,
					CurrentVolume = 0,
					FinalVolume = 0,
					MasterVolume = 64,
					ResetFlags = ResetFlag.None,

					PortamentoTickCounter = 0,
					PortamentoIncrement = 0,
					PortamentoDirection = false,
					PortamentoDelay = 0,
					PortamentoDuration = 0,

					VolumeEnvelope = null,
					VolumeEnvelopePosition = 0,
					VolumeEnvelopeTickCounter = 0,
					VolumeEnvelopeDelta = 0,

					PeriodTable = null,
					PeriodTablePosition = 0,
					PeriodTableTickCounter = 0,
					PeriodTableCommand = 0,

					WaveformTable = null,
					WaveformTablePosition = 0,

					WaveformStartPosition = 0,
					WaveformPosition = 0,
					WaveformTickCounter = 0,
					WaveformIncrement = 1,
					WaveformMask = 0,
					SynthesisMode = SynthesisFlag.None,
					MorphSpeed = 0
				};

				voices[i] = voiceInfo;

				PaulaChannel channel = new PaulaChannel(VirtualChannels[i]);
				channel.Length = 0x10;
				channel.Volume = 0;
				channel.SetAddress(-1, Tables.EmptySample.Data, 0, false);
				channel.SetDma(true);

				channels[i] = channel;

				int index = i;
				VirtualChannels[i].Interrupt = () => AudioInterrupt(index);
			}

			endReached = false;
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player has allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			voices = null;
			channels = null;

			subSongs = null;
			allSamples = null;

			trackTimes = null;
		}
		#endregion
	}
}
