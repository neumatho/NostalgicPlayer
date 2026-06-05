/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal partial class MusiclineEditorWorker : ModulePlayerWithSubSongDurationAgentBase
	{
		private static readonly Dictionary<Guid, ModuleType> moduleTypeLookup = new Dictionary<Guid, ModuleType>
		{
			{ MusiclineEditor.Agent1Id, ModuleType._4Channels },
			{ MusiclineEditor.Agent2Id, ModuleType._8Channels }
		};

		private readonly ModuleType currentModuleType;

		private List<Tune> tunes;
		private Part[] parts;
		private Arpeggio[] arpeggios;
		private Instrument[] instruments;
		private Sample[] samples;

		private int numberOfChannels;
		private int numberOfParts;
		private int numberOfInstruments;
		private int numberOfSamples;

		private string moduleName;
		private string author;
		private string[] comments;

		private SubSongInfo subSongs;

		private Tune currentSong;

		private GlobalPlayingInfo playingInfo;
		private VoiceInfo[] voices;

		private Dictionary<byte, TimeSpan>[] positionTimes;
		private TimeSpan[] channelLoopLengths;

		private const int InfoPositionLine = 4;
		private const int InfoTrackLine = 5;
		private const int InfoSpeedLine = 6;
		private const int InfoTempoLine = 7;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MusiclineEditorWorker(Guid typeId)
		{
			if (!moduleTypeLookup.TryGetValue(typeId, out currentModuleType))
				currentModuleType = ModuleType.Unknown;
		}

		#region Identify
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => [ "ml" ];



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

			subSongs = new SubSongInfo(tunes.Count, 0, tunes.Select(x => x.Title));

			positionTimes = ArrayHelper.InitializeArray<Dictionary<byte, TimeSpan>>(8);

			for (int i = 0; i < positionTimes.Length; i++)
				positionTimes[i][0] = new TimeSpan(0);

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

			if (HasEndReached)
			{
				if (voices.All(x => x.VoiceOff))
					RestartSong();
				else
				{
					TimeSpan currentTime = GetCurrentTime() ?? TimeSpan.Zero;
					TimeSpan maxLoop = TimeSpan.Zero;

					for (int i = 0; i < numberOfChannels; i++)
					{
						if (channelLoopLengths[i] > maxLoop)
							maxLoop = channelLoopLengths[i];
					}

					TimeSpan restartTime = currentTime - maxLoop;
					if (restartTime < TimeSpan.Zero)
						restartTime = TimeSpan.Zero;

					SetRestartTime(restartTime);
				}
			}
		}
		#endregion

		#region Information
		/********************************************************************/
		/// <summary>
		/// Return the title
		/// </summary>
		/********************************************************************/
		public override string Title => moduleName;



		/********************************************************************/
		/// <summary>
		/// Return the name of the author
		/// </summary>
		/********************************************************************/
		public override string Author => author;



		/********************************************************************/
		/// <summary>
		/// Return the comment separated in lines
		/// </summary>
		/********************************************************************/
		public override string[] Comment => comments;



		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public override SubSongInfo SubSongs => subSongs;



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public override int ModuleChannelCount => currentModuleType == ModuleType._8Channels ? 8 : 4;



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
				for (int i = 0; i < numberOfSamples; i++)
				{
					Sample sample = samples[i + 1];

					// Build frequency table.
					// PalPitchTable has 32 entries per halftone; index 1210 holds
					// period 428 (the standard MOD C-2 reference), which we anchor
					// at frequencies[60]. The sample's SemiTone (whole halftones,
					// hence << 5) and FineTune (1/32 halftones) are folded in the
					// same way the player does at runtime.
					uint[] frequencies = new uint[10 * 12];

					int baseIndex = 1210 - (60 * 32) + (sample.SemiTone << 5) + sample.FineTune;
					for (int j = 0; j < frequencies.Length; j++)
					{
						int idx = baseIndex + (j * 32);
						if ((idx >= 0) && (idx < Tables.PalPitchTable.Length))
							frequencies[j] = PeriodToFrequency(Tables.PalPitchTable[idx]);
					}

					SampleInfo sampleInfo = new SampleInfo
					{
						Name = sample.Title,
						Flags = SampleInfo.SampleFlag.None,
						Volume = 256,
						Panning = -1,
						LoopStart = 0,
						LoopLength = 0,
						NoteFrequencies = frequencies
					};

					if (sample.SampleType == SampleType.Sample)
					{
						sampleInfo.Type = SampleInfo.SampleType.Sample;
						sampleInfo.Sample = sample.SampleData;
						sampleInfo.Length = sample.SampleLength * 2U;

						if (sample.SampleRepeatLength != 0)
						{
							sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;
							sampleInfo.LoopStart = sample.SampleRepeatPointer!.Value.StartOffset;
							sampleInfo.LoopLength = sample.SampleRepeatLength * 2U;
						}
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
				// Number of positions
				case 0:
				{
					description = Resources.IDS_MLE_INFODESCLINE0;
					value = FormatPositionLengths();
					break;
				}

				// Used tracks
				case 1:
				{
					description = Resources.IDS_MLE_INFODESCLINE1;
					value = numberOfParts.ToString();
					break;
				}

				// Used instruments
				case 2:
				{
					description = Resources.IDS_MLE_INFODESCLINE2;
					value = numberOfInstruments.ToString();
					break;
				}

				// Used samples
				case 3:
				{
					description = Resources.IDS_MLE_INFODESCLINE3;
					value = numberOfSamples.ToString();
					break;
				}

				// Playing positions
				case 4:
				{
					description = Resources.IDS_MLE_INFODESCLINE4;
					value = FormatPositions();
					break;
				}

				// Playing tracks
				case 5:
				{
					description = Resources.IDS_MLE_INFODESCLINE5;
					value = FormatTracks();
					break;
				}

				// Current speed
				case 6:
				{
					description = Resources.IDS_MLE_INFODESCLINE6;
					value = playingInfo.CurrentSpeed.ToString();
					break;
				}

				// Current tempo (BPM)
				case 7:
				{
					description = Resources.IDS_MLE_INFODESCLINE7;
					value = playingInfo.TuneTempo.ToString();
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
			currentSong = tunes[subSong];

			numberOfChannels = currentModuleType == ModuleType._8Channels ? 8 : 4;
			numberOfChannels = Math.Min(numberOfChannels, currentSong.Channels);

			// Some modules uses fewer channels than the format defines,
			// so mark those channels as already ended, else the duration
			// calculation will run forever
			for (int i = numberOfChannels; i < ModuleChannelCount; i++)
				OnEndReached(i);

			HasEndReached = false;

			playingInfo = new GlobalPlayingInfo
			{
				TuneTempo = currentSong.Tempo,
				TuneSpeed = currentSong.Speed,
				TuneGroove = currentSong.Groove,
				TunePosition = 0,
				MasterVolume = 64 * 16
			};

			voices = new VoiceInfo[numberOfChannels];
			channelLoopLengths = new TimeSpan[numberOfChannels];

			for (int i = 0; i < numberOfChannels; i++)
			{
				VoiceInfo voiceInfo = new VoiceInfo
				{
					ChannelNumber = i,

					VoiceOff = false,
					Speed = playingInfo.TuneSpeed,
					Groove = playingInfo.TuneGroove,
					SpeedPart = 0,
					GroovePart = 0,
					TuneSpeed = 0,
					TuneGroove = 0,
					SpeedCounter = 1,
					ArpeggioSpeedCounter = 0,
					PartGroove = playingInfo.TuneGroove != 0,
					ArpeggioGroove = false,
					WaveSampleNumber = 0,
					WaveSampleNumberOld = 0,
					Instrument = null,
					WaveSample = null,
					PartNote = 0,
					PartInstrument = 0,
					Arpeggio = ArpeggioFlag.None,
					ArpeggioPosition = 0,
					ArpeggioTable = 0,
					ArpeggioWait = false,
					ArpeggioNote = 0,
					ArpeggioVolumeSlide = ArpeggioEffect.None,
					ArpeggioPitchSlide = ArpeggioEffect.None,
					ArpeggioPitchSlideType = Direction.Upward,
					ArpeggioCalculatedNote = 0,

					TunePosition = 0,
					PartPosition = 0,
					PartPositionWork = 0,
					TuneJumpCounter = 0,
					TuneJumpPosition = -1,
					PartJumpCounter = 0,

					WaveSampleRepeatPointerOriginal = null,
					WaveSamplePointer = null,
					WaveSampleLength = 0,
					WaveSampleRepeatPointer = null,
					WaveSampleRepeatLength = 0,

					Volume1 = 0,
					Volume2 = 0,
					Volume3 = 0,

					Note = 0,
					Period1 = 0,
					Period2 = 0,

					Transpose = 0,
					SemiTone = 0,
					FineTune = 0,

					SampleOffsetActive = false,
					SampleOffset = 0,
					OldInstrument = 0,
					Restart = RestartFlag.None,

					VolumeAdd = PartEffect.None,
					VolumeSlide = PartEffect.None,
					ChannelVolumeSlide = PartEffect.None,
					MasterVolumeSlide = PartEffect.None,
					VolumeSet = 0,
					ChannelVolume = 64 * 16,
					VolumeAddNumber = 0,
					ChannelVolumeAddNumber = 0,
					MasterVolumeAddNumber = 0,
					VolumeSlideSpeed = 0,
					ChannelVolumeSlideSpeed = 0,
					MasterVolumeSlideSpeed = 0,

					VolumeSlideVolume = 0,
					ChannelVolumeSlideVolume = 0,
					MasterVolumeSlideVolume = 0,
					VolumeSlideToVolume = 0,
					ChannelVolumeSlideToVolume = 0,
					MasterVolumeSlideToVolume = 0,
					VolumeSlideType = Direction.Upward,
					ChannelVolumeSlideType = Direction.Upward,
					MasterVolumeSlideType = Direction.Upward,
					VolumeSlideToVolumeOff = false,
					ChannelVolumeSlideToVolumeOff = false,
					MasterVolumeSlideToVolumeOff = false,

					Volume = PartEffect.None,

					InstrumentPitchSlide = PartEffect.None,
					MixResFilBoost = BoostFlag.None,

					TransposeNumber = 0,
					PartNumber = 0,
					PitchSlide = 0,
					PitchSlideType = Direction.Upward,
					PitchSlideSpeed = 0,
					PitchSlideNote = 0,
					PitchSlideToNote = -1,
					PitchAdd = 0,
					ArpeggioVolumeSlideSpeed = 0,
					ArpeggioPitchSlideSpeed = 0,
					ArpeggioPitchSlideToNote = 0,
					ArpeggioPitchSlideNote = 0,

					PtPitchSlide = 0,
					PtPitchSlideType = Direction.Upward,
					PtPitchSlideSpeed = 0,
					PtPitchSlideSpeed2 = 0,
					PtPitchSlideNote = 0,
					PtPitchSlideToNote = 0,
					PtPitchAdd = 0,

					EnvelopeVolume = 0,
					Play = PlayFlag.None,
					WaveOrSample = SampleType.Sample,
					PhaseInit = 0,
					FilterInit = 0,
					TransformInit = 0,
					TuneWait = 0,

					Vibrato = PartEffect.None,
					VibratoDirection = Direction.Upward,
					VibratoWaveType = WaveType.Sine,
					PartVibratoWaveType = WaveType.Sine,
					VibratoCounter = 0,
					VibratoCommandSpeed = 0,
					VibratoCommandDepth = 0,
					VibratoCommandDelay = 0,
					VibratoAttackSpeed = 0,
					VibratoAttackLength = 0,
					VibratoDepth = 0,
					VibratoNote = 0,

					PtTremoloPosition = 0,
					PtTremoloCommand = 0,
					PtTremoloWaveType = PtWaveType.Sine,
					PtVibratoPosition = 0,
					PtVibratoCommand = 0,
					PtVibratoWaveType = PtWaveType.Sine,
					PtVibratoNote = 0,

					Tremolo = PartEffect.None,
					TremoloDirection = Direction.Upward,
					TremoloWaveType = WaveType.Sine,
					PartTremoloWaveType = WaveType.Sine,
					TremoloCounter = 0,
					TremoloCommandSpeed = 0,
					TremoloCommandDepth = 0,
					TremoloCommandDelay = 0,
					TremoloAttackSpeed = 0,
					TremoloAttackLength = 0,
					TremoloDepth = 0,

					FilterLastSample = 0,
					ResonanceLastSample = 0,
					FilterLastInit = false,
					ResonanceLastInit = false,
					ResonanceAmp = 0,
					ResonanceInit = 0,
					PhaseType = PhaseType.Old,
					FilterType = FilterType.Normal,
					TransformSpeed = 0,
					PhaseSpeed = 0,
					MixSpeed = 0,
					ResonanceSpeed = 0,
					FilterSpeed = 0,
					MixWaveNumber = 0,
					MixInit = 0,
					LoopInit = 0,
					LoopRepeat = 0,
					LoopRepeatEnd = 0,
					LoopLength = 0,
					LoopStep = 0,
					LoopWait = 0,
					LoopWaitCounter = 0,
					LoopDelay = 0,
					LoopTurns = 0,
					LoopCounter = 0,
					LoopCounterSave = 0,
					LoopWaveSampleCounterMax = 0,
					LoopSpeed = 0,
					LoopWaveSample = null
				};

				voices[i] = voiceInfo;
			}

			playingEnabled = true;

			SetBpmTempo(currentSong.Tempo);
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player has allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			tunes = null;
			parts = null;
			arpeggios = null;
			instruments = null;
			samples = null;

			zeroChannel = null;
			zeroInstrument = null;

			subSongs = null;

			playingInfo = null;
			voices = null;

			positionTimes = null;
			channelLoopLengths = null;
		}



		/********************************************************************/
		/// <summary>
		/// Update the position lookup list with a new time
		/// </summary>
		/********************************************************************/
		private void SetPositionTime(VoiceInfo voiceInfo)
		{
			TimeSpan? currentTime = GetCurrentTime();
			if (currentTime.HasValue && !positionTimes[voiceInfo.ChannelNumber].ContainsKey(voiceInfo.TunePosition))
				positionTimes[voiceInfo.ChannelNumber][voiceInfo.TunePosition] = currentTime.Value;
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current channel positions
		/// </summary>
		/********************************************************************/
		private void ShowChannelPositions()
		{
			OnModuleInfoChanged(InfoPositionLine, FormatPositions());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with track numbers
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
			OnModuleInfoChanged(InfoSpeedLine, playingInfo.CurrentSpeed.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current tempo
		/// </summary>
		/********************************************************************/
		private void ShowTempo()
		{
			OnModuleInfoChanged(InfoTempoLine, playingInfo.TuneTempo.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with all dynamic values
		/// </summary>
		/********************************************************************/
		private void UpdateModuleInformation()
		{
			ShowChannelPositions();
			ShowTracks();
			ShowSpeed();
			ShowTempo();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the songs position lengths
		/// </summary>
		/********************************************************************/
		private string FormatPositionLengths()
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < numberOfChannels; i++)
			{
				int length = currentSong.Sequences[i]
					.Select((val, index) => new { val, index })
					// Find control command where the command is either end or jump and the argument is 0
					.Where(x => ((x.val & 0x20) != 0) && (((x.val & 0xc0) >> 6) <= 2) && ((x.val & 0x1f) == 0))
					.Select(x => x.index + 1)
					.FirstOrDefault() - 1;

				if (length == -1)
					length = currentSong.Sequences[i].Length;
				else
					length++;

				sb.Append(length);
				sb.Append(", ");
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

			for (int i = 0; i < numberOfChannels; i++)
			{
				sb.Append(voices[i].TunePosition);
				sb.Append(", ");
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
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < numberOfChannels; i++)
			{
				sb.Append(voices[i].PartNumber);
				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}
		#endregion
	}
}
