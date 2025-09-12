/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.Actionamics.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.Actionamics
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class ActionamicsWorker : ModulePlayerWithSubSongDurationAgentBase
	{
		private ushort tempo;

		private SongInfo[] songInfoList;

		private SinglePositionInfo[][] positions;
		private Instrument[] instruments;
		private Sample[] samples;
		private SampleExtra[] sampleExtras;

		private sbyte[][] sampleNumberList;
		private sbyte[][] arpeggioList;
		private sbyte[][] frequencyList;

		private byte[][] tracks;

		private SongInfo currentSongInfo;

		private GlobalPlayingInfo playingInfo;
		private VoiceInfo[] voices;

		private const int InfoPositionLine = 3;
		private const int InfoTrackLine = 4;
		private const int InfoSpeedLine = 5;

		#region Identify
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => [ "ast" ];



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 90)
				return AgentResult.Unknown;

			// Check signature
			moduleStream.Seek(62, SeekOrigin.Begin);

			string signature = moduleStream.ReadMark(22, false);
			if (signature != "ACTIONAMICS SOUND TOOL")
				return AgentResult.Unknown;

			return AgentResult.Ok;
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
			errorMessage = string.Empty;

			try
			{
				ModuleStream moduleStream = fileInfo.ModuleStream;
				Encoding encoder = EncoderCollection.Amiga;

				tempo = moduleStream.Read_B_UINT16();

				uint[] lengths = new uint[15];
				moduleStream.ReadArray_B_UINT32s(lengths, 0, 15);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_AST_ERR_LOADING_INFO;
					return AgentResult.Error;

				}

				long startOffset = moduleStream.Position;
				startOffset += lengths[0];

				if (!LoadModuleInformation(moduleStream, startOffset, out uint totalLength) || (totalLength > moduleStream.Length))
				{
					errorMessage = Resources.IDS_AST_ERR_LOADING_INFO;
					return AgentResult.Error;
				}

				startOffset += lengths[1];

				if (!LoadPositionLists(moduleStream, startOffset, lengths[2], lengths[3], lengths[4]))
				{
					errorMessage = Resources.IDS_AST_ERR_LOADING_POSITIONLIST;
					return AgentResult.Error;
				}

				startOffset += lengths[2] + lengths[3] + lengths[4];

				if (!LoadInstruments(moduleStream, startOffset, lengths[5]))
				{
					errorMessage = Resources.IDS_AST_ERR_LOADING_INSTRUMENTS;
					return AgentResult.Error;
				}

				startOffset += lengths[5];

				sampleNumberList = LoadList(moduleStream, startOffset, lengths[6]);
				if (sampleNumberList == null)
				{
					errorMessage = Resources.IDS_AST_ERR_LOADING_LISTS;
					return AgentResult.Error;
				}

				startOffset += lengths[6];

				arpeggioList = LoadList(moduleStream, startOffset, lengths[7]);
				if (sampleNumberList == null)
				{
					errorMessage = Resources.IDS_AST_ERR_LOADING_LISTS;
					return AgentResult.Error;
				}

				startOffset += lengths[7];

				frequencyList = LoadList(moduleStream, startOffset, lengths[8]);
				if (sampleNumberList == null)
				{
					errorMessage = Resources.IDS_AST_ERR_LOADING_LISTS;
					return AgentResult.Error;
				}

				startOffset += lengths[8] + lengths[9] + lengths[10];

				if (!LoadSubSongs(moduleStream, startOffset, lengths[11]))
				{
					errorMessage = Resources.IDS_AST_ERR_LOADING_SUBSONG;
					return AgentResult.Error;
				}

				startOffset += lengths[11] + lengths[12];

				if (!LoadSampleInfo(moduleStream, startOffset, lengths[13], encoder))
				{
					errorMessage = Resources.IDS_AST_ERR_LOADING_SAMPLEINFO;
					return AgentResult.Error;
				}

				startOffset += lengths[13];

				if (!LoadTracks(moduleStream, startOffset, lengths[14]))
				{
					errorMessage = Resources.IDS_AST_ERR_LOADING_TRACKS;
					return AgentResult.Error;
				}

				if (!LoadSampleData(moduleStream, totalLength))
				{
					errorMessage = Resources.IDS_AST_ERR_LOADING_SAMPLES;
					return AgentResult.Error;
				}
			}
			catch (Exception)
			{
				Cleanup();
				throw;
			}

			// Everything is loaded alright
			return AgentResult.Ok;
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
			playingInfo.MeasureCounter++;

			if (playingInfo.MeasureCounter == 3)
				playingInfo.MeasureCounter = 0;

			playingInfo.SpeedCounter++;

			if (playingInfo.SpeedCounter == playingInfo.CurrentSpeed)
			{
				playingInfo.SpeedCounter = 0;
				playingInfo.MeasureCounter = 0;

				for (int i = 0; i < 4; i++)
				{
					VoiceInfo voice = voices[i];

					ReadNextRow(voice);
					SetupNoteAndSample(voice);
				}

				DoSampleInversionEffect();

				playingInfo.CurrentRowPosition++;

				if (playingInfo.CurrentRowPosition == playingInfo.NumberOfRows)
				{
					playingInfo.CurrentRowPosition = 0;

					byte position = playingInfo.CurrentPosition;
					playingInfo.CurrentPosition++;

					if (position == playingInfo.EndPosition)
					{
						playingInfo.CurrentPosition = playingInfo.LoopPosition;

						OnEndReachedOnAllChannels(0);
					}

					for (int i = 0; i < 4; i++)
						SetupTrack(voices[i]);

					ShowPosition();
					ShowTracks();
				}
			}

			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = voices[i];
				IChannel channel = VirtualChannels[i];

				if (voiceInfo.RestartDelayCounter != 0)
				{
					voiceInfo.RestartDelayCounter--;

					if (voiceInfo.RestartDelayCounter == 0)
					{
						voiceInfo.SampleData = voiceInfo.RestartSampleData;
						voiceInfo.SampleOffset = voiceInfo.RestartSampleOffset;
						voiceInfo.SampleLength = voiceInfo.RestartSampleLength;
						voiceInfo.TrigSample = true;
					}
				}

				DoEffects(voiceInfo, channel);

				if (voiceInfo.NoteDelayCounter == 0)
				{
					if (voiceInfo.TrigSample)
					{
						voiceInfo.TrigSample = false;

						channel.PlaySample((short)voiceInfo.SampleNumber, voiceInfo.SampleData, voiceInfo.SampleOffset, voiceInfo.SampleLength * 2U);

						if (voiceInfo.SampleLoopLength > 1)
							channel.SetLoop(voiceInfo.SampleLoopStart, voiceInfo.SampleLoopLength * 2U);
					}

					channel.SetAmigaPeriod(voiceInfo.FinalPeriod);
				}
				else
					voiceInfo.NoteDelayCounter--;
			}

			if (playingInfo.SpeedCounter != 0)
				DoSampleInversionEffect();
		}
		#endregion

		#region Information
		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public override SubSongInfo SubSongs => new SubSongInfo(songInfoList.Length, 0);



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
				uint[] freqs = new uint[10 * 12];

				for (int j = 1; j < Tables.Periods.Length; j++)
				{
					ushort period = Tables.Periods[j];
					freqs[1 * 12 + 3 + j - 1] = PeriodToFrequency(period);
				}

				foreach (Sample sample in samples)
				{
					SampleInfo sampleInfo = new SampleInfo
					{
						Name = sample.Name,
						Flags = SampleInfo.SampleFlag.None,
						Type = SampleInfo.SampleType.Sample,
						Volume = 256,
						Panning = -1,
						Sample = sample.SampleData,
						Length = sample.Length * 2U,
						NoteFrequencies = freqs
					};

					if (sample.LoopLength > 1)
					{
						sampleInfo.LoopStart = sample.LoopStart * 2U;
						sampleInfo.LoopLength = sample.LoopLength * 2U;
						sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;
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
					description = Resources.IDS_AST_INFODESCLINE0;
					value = FormatPositionLength();
					break;
				}

				// Used tracks
				case 1:
				{
					description = Resources.IDS_AST_INFODESCLINE1;
					value = tracks.Length.ToString();
					break;
				}

				// Used samples
				case 2:
				{
					description = Resources.IDS_AST_INFODESCLINE2;
					value = samples.Length.ToString();
					break;
				}

				// Playing position
				case 3:
				{
					description = Resources.IDS_AST_INFODESCLINE3;
					value = FormatPosition();
					break;
				}

				// Playing tracks
				case 4:
				{
					description = Resources.IDS_AST_INFODESCLINE4;
					value = FormatTracks();
					break;
				}

				// Current speed
				case 5:
				{
					description = Resources.IDS_AST_INFODESCLINE5;
					value = playingInfo.CurrentSpeed.ToString();
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
			return new Snapshot(playingInfo, voices, sampleExtras);
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
			Snapshot clonedSnapshot = new Snapshot(currentSnapshot.PlayingInfo, currentSnapshot.Voices, currentSnapshot.SampleExtras);

			playingInfo = clonedSnapshot.PlayingInfo;
			voices = clonedSnapshot.Voices;
			sampleExtras = clonedSnapshot.SampleExtras;

			UpdateModuleInformation();

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Load module information
		/// </summary>
		/********************************************************************/
		private bool LoadModuleInformation(ModuleStream moduleStream, long startOffset, out uint totalLength)
		{
			if (startOffset > moduleStream.Length)
			{
				totalLength = 0;
				return false;
			}

			moduleStream.Seek(startOffset, SeekOrigin.Begin);

			totalLength = moduleStream.Read_B_UINT32();

			if (moduleStream.EndOfStream)
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load position lists
		/// </summary>
		/********************************************************************/
		private bool LoadPositionLists(ModuleStream moduleStream, long startOffset, uint trackNumberLength, uint instrumentTransposeLength, uint noteTransposeLength)
		{
			if (startOffset > moduleStream.Length)
				return false;

			moduleStream.Seek(startOffset, SeekOrigin.Begin);

			if ((trackNumberLength != instrumentTransposeLength) || (trackNumberLength != noteTransposeLength))
				return false;

			int count = (int)(trackNumberLength / 4);
			positions = ArrayHelper.Initialize2Arrays<SinglePositionInfo>(4, count);

			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < count; j++)
				{
					positions[i][j] = new SinglePositionInfo();
					positions[i][j].TrackNumber = moduleStream.Read_UINT8();
				}

				if (moduleStream.EndOfStream)
					return false;
			}

			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < count; j++)
					positions[i][j].NoteTranspose = moduleStream.Read_INT8();

				if (moduleStream.EndOfStream)
					return false;
			}

			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < count; j++)
					positions[i][j].InstrumentTranspose = moduleStream.Read_INT8();

				if (moduleStream.EndOfStream)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load instruments
		/// </summary>
		/********************************************************************/
		private bool LoadInstruments(ModuleStream moduleStream, long startOffset, uint instrumentLength)
		{
			if (startOffset > moduleStream.Length)
				return false;

			moduleStream.Seek(startOffset, SeekOrigin.Begin);

			int count = (int)(instrumentLength / 32);
			instruments = new Instrument[count];

			for (int i = 0; i < count; i++)
			{
				Instrument instr = new Instrument();

				instr.SampleNumberList = LoadInstrumentListInfo(moduleStream);
				instr.ArpeggioList = LoadInstrumentListInfo(moduleStream);
				instr.FrequencyList = LoadInstrumentListInfo(moduleStream);

				if (moduleStream.EndOfStream)
					return false;

				instr.PortamentoIncrement = moduleStream.Read_INT8();
				instr.PortamentoDelay = moduleStream.Read_UINT8();

				instr.NoteTranspose = moduleStream.Read_INT8();

				if (moduleStream.EndOfStream)
					return false;

				moduleStream.Seek(1, SeekOrigin.Current);

				instr.AttackEndVolume = moduleStream.Read_UINT8();
				instr.AttackSpeed = moduleStream.Read_UINT8();
				instr.DecayEndVolume = moduleStream.Read_UINT8();
				instr.DecaySpeed = moduleStream.Read_UINT8();
				instr.SustainDelay = moduleStream.Read_UINT8();
				instr.ReleaseEndVolume = moduleStream.Read_UINT8();
				instr.ReleaseSpeed = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return false;

				moduleStream.Seek(9, SeekOrigin.Current);

				instruments[i] = instr;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load instrument list info
		/// </summary>
		/********************************************************************/
		private InstrumentList LoadInstrumentListInfo(ModuleStream moduleStream)
		{
			return new InstrumentList
			{
				ListNumber = moduleStream.Read_UINT8(),
				NumberOfValuesInList = moduleStream.Read_UINT8(),
				StartCounterDeltaValue = moduleStream.Read_UINT8(),
				CounterEndValue = moduleStream.Read_UINT8()
			};
		}



		/********************************************************************/
		/// <summary>
		/// Load a single list
		/// </summary>
		/********************************************************************/
		private sbyte[][] LoadList(ModuleStream moduleStream, long startOffset, uint listLength)
		{
			if (startOffset > moduleStream.Length)
				return null;

			moduleStream.Seek(startOffset, SeekOrigin.Begin);

			int count = (int)(listLength / 16);
			sbyte[][] list = ArrayHelper.Initialize2Arrays<sbyte>(count, 16);

			for (int i = 0; i < count; i++)
			{
				if (moduleStream.ReadSigned(list[i], 0, 16) != 16)
					return null;
			}

			return list;
		}



		/********************************************************************/
		/// <summary>
		/// Load sub-song information
		/// </summary>
		/********************************************************************/
		private bool LoadSubSongs(ModuleStream moduleStream, long startOffset, uint subSongLength)
		{
			if (startOffset > moduleStream.Length)
				return false;

			moduleStream.Seek(startOffset, SeekOrigin.Begin);

			int count = (int)(subSongLength / 4);
			SongInfo[] tempList = new SongInfo[count];

			for (int i = 0; i < count; i++)
			{
				SongInfo song = new SongInfo();

				song.StartPosition = moduleStream.Read_UINT8();
				song.EndPosition = moduleStream.Read_UINT8();
				song.LoopPosition = moduleStream.Read_UINT8();
				song.Speed = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return false;

				tempList[i] = song;
			}

			songInfoList = tempList.Where(x => (x.StartPosition != 0) || (x.EndPosition != 0) || (x.LoopPosition != 0)).ToArray();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load sample information
		/// </summary>
		/********************************************************************/
		private bool LoadSampleInfo(ModuleStream moduleStream, long startOffset, uint sampleLength, Encoding encoder)
		{
			if (startOffset > moduleStream.Length)
				return false;

			moduleStream.Seek(startOffset, SeekOrigin.Begin);

			int count = (int)(sampleLength / 64);
			samples = new Sample[count];
			sampleExtras = new SampleExtra[count];

			for (int i = 0; i < count; i++)
			{
				Sample sample = new Sample();
				SampleExtra sampleExtra = new SampleExtra();

				// Skip pointer to data
				moduleStream.Seek(4, SeekOrigin.Current);

				sample.Length = moduleStream.Read_B_UINT16();
				sample.LoopStart = moduleStream.Read_B_UINT16();
				sample.LoopLength = moduleStream.Read_B_UINT16();

				sample.EffectStartPosition = moduleStream.Read_B_UINT16();
				sample.EffectLength = moduleStream.Read_B_UINT16();

				// Bug in original player. Hi-byte is also used as arpeggio list number
				sample.ArpeggioListNumber = (byte)(sample.EffectLength >> 8);

				sample.EffectSpeed = moduleStream.Read_B_UINT16();
				sample.EffectMode = moduleStream.Read_B_UINT16();

				sampleExtra.EffectIncrementValue = moduleStream.Read_B_INT16();
				sampleExtra.EffectPosition = moduleStream.Read_B_INT32();
				sampleExtra.EffectSpeedCounter = moduleStream.Read_B_UINT16();
				sampleExtra.AlreadyTaken = moduleStream.Read_B_UINT16() != 0;

				if (moduleStream.EndOfStream)
					return false;

				moduleStream.Seek(4, SeekOrigin.Current);

				sample.Name = moduleStream.ReadString(encoder, 32);

				if (moduleStream.EndOfStream)
					return false;

				samples[i] = sample;
				sampleExtras[i] = sampleExtra;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all tracks
		/// </summary>
		/********************************************************************/
		private bool LoadTracks(ModuleStream moduleStream, long startOffset, uint trackOffsetLength)
		{
			if (startOffset > moduleStream.Length)
				return false;

			moduleStream.Seek(startOffset, SeekOrigin.Begin);

			int count = (int)(trackOffsetLength / 2);
			tracks = new byte[count - 1][];

			ushort[] offsets = new ushort[count];
			moduleStream.ReadArray_B_UINT16s(offsets, 0, count);

			if (moduleStream.EndOfStream)
				return false;

			long trackStartOffset = moduleStream.Position;

			for (int i = 0; i < count - 1; i++)
			{
				moduleStream.Seek(trackStartOffset + offsets[i], SeekOrigin.Begin);

				int trackLength = offsets[i + 1] - offsets[i];
				tracks[i] = new byte[trackLength];

				if (moduleStream.Read(tracks[i], 0, trackLength) != trackLength)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load sample data
		/// </summary>
		/********************************************************************/
		private bool LoadSampleData(ModuleStream moduleStream, uint totalLength)
		{
			int totalSampleLength = samples.Sum(s => s.Length) * 2;
			uint sampleDataStartPosition = totalLength - (uint)totalSampleLength;

			if (sampleDataStartPosition > moduleStream.Length)
				return false;

			moduleStream.Seek(sampleDataStartPosition, SeekOrigin.Begin);

			for (int i = 0; i < samples.Length; i++)
			{
				Sample sample = samples[i];

				if (sample.Length > 0)
				{
					int length = sample.Length * 2;

					sample.SampleData = moduleStream.ReadSampleData(i, length, out int readBytes);
					if (readBytes != length)
						return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound(int subSong)
		{
			currentSongInfo = songInfoList[subSong];

			playingInfo = new GlobalPlayingInfo
			{
				SpeedCounter = 0,
				CurrentSpeed = currentSongInfo.Speed,
				MeasureCounter = 0,
				CurrentPosition = currentSongInfo.StartPosition,
				LoopPosition = currentSongInfo.LoopPosition,
				EndPosition = currentSongInfo.EndPosition,
				CurrentRowPosition = 0,
				NumberOfRows = 64
			};

			voices = ArrayHelper.InitializeArray<VoiceInfo>(4);

			for (int i = 0; i < 4; i++)
			{
				voices[i] = new VoiceInfo
				{
					PositionList = positions[i],
					TrackData = null,
					TrackPosition = 0,
					DelayCounter = 0,

					InstrumentNumber = 0,
					Instrument = null,
					InstrumentTranspose = 0,

					SampleNumber = 0,
					SampleData = null,
					SampleOffset = 0,
					SampleLength = 0,
					SampleLoopStart = 0,
					SampleLoopLength = 0,

					Note = 0,
					NoteTranspose = 0,
					NotePeriod = 0,

					FinalNote = 0,
					FinalPeriod = 0,
					FinalVolume = 0,
					GlobalVoiceVolume = 0x4041,

					EnvelopeState = EnvelopeState.Done,
					SustainCounter = 0,

					SampleNumberListSpeedCounter = 0,
					SampleNumberListPosition = 0,

					ArpeggioListSpeedCounter = 0,
					ArpeggioListPosition = 0,

					FrequencyListSpeedCounter = 0,
					FrequencyListPosition = 0,

					Effect = Effect.None,
					EffectArgument = 0,

					PortamentoDelayCounter = 0,
					PortamentoValue = 0,

					TonePortamentoEndPeriod = 0,
					TonePortamentoIncrementValue = 0,

					VibratoEffectArgument = 0,
					VibratoTableIndex = 0,

					TremoloEffectArgument = 0,
					TremoloTableIndex = 0,
					TremoloVolume = 0,

					SampleOffsetEffectArgument = 0,
					NoteDelayCounter = 0,

					RestartDelayCounter = 0,
					RestartSampleData = null,
					RestartSampleOffset = 0,
					RestartSampleLength = 0,

					TrigSample = false
				};

				SetupTrack(voices[i]);
			}

			for (int i = samples.Length - 1; i >= 0; i--)
			{
				Sample sample = samples[i];

				if (sample.EffectMode != 0)
					sampleExtras[i].ModifiedSampleData = ArrayHelper.CloneArray(sample.SampleData);
			}

			SetBpmTempo(tempo);
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			songInfoList = null;

			positions = null;
			instruments = null;
			samples = null;
			sampleExtras = null;

			sampleNumberList = null;
			arpeggioList = null;
			frequencyList = null;

			tracks = null;

			currentSongInfo = null;

			playingInfo = null;
			voices = null;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize voice to new track
		/// </summary>
		/********************************************************************/
		private void SetupTrack(VoiceInfo voiceInfo)
		{
			SinglePositionInfo positionInfo = voiceInfo.PositionList[playingInfo.CurrentPosition];

			voiceInfo.TrackData = tracks[positionInfo.TrackNumber];
			voiceInfo.TrackPosition = 0;

			voiceInfo.NoteTranspose = positionInfo.NoteTranspose;
			voiceInfo.InstrumentTranspose = positionInfo.InstrumentTranspose;

			voiceInfo.DelayCounter = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Read the next row for the given voice
		/// </summary>
		/********************************************************************/
		private void ReadNextRow(VoiceInfo voiceInfo)
		{
			ReadTrackData(voiceInfo);

			if (voiceInfo.Note != 0)
			{
				voiceInfo.Note = (ushort)(voiceInfo.Note + voiceInfo.NoteTranspose);
				voiceInfo.TrigSample = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Read the track data for the given voice
		/// </summary>
		/********************************************************************/
		private void ReadTrackData(VoiceInfo voiceInfo)
		{
			voiceInfo.Note = 0;
			voiceInfo.InstrumentNumber = 0;
			voiceInfo.Effect = 0;
			voiceInfo.EffectArgument = 0;

			if (voiceInfo.DelayCounter == 0)
			{
				byte[] trackData = voiceInfo.TrackData;

				byte data = trackData[voiceInfo.TrackPosition++];
				if ((data & 0x80) != 0)
				{
					voiceInfo.DelayCounter = (byte)~data;
					return;
				}

				if (data >= 0x70)
				{
					voiceInfo.Effect = (Effect)data;
					voiceInfo.EffectArgument = trackData[voiceInfo.TrackPosition++];
					return;
				}

				voiceInfo.Note = data;

				data = trackData[voiceInfo.TrackPosition++];
				if ((data & 0x80) != 0)
				{
					voiceInfo.DelayCounter = (byte)~data;
					return;
				}

				if (data >= 0x70)
				{
					voiceInfo.Effect = (Effect)data;
					voiceInfo.EffectArgument = trackData[voiceInfo.TrackPosition++];
					return;
				}

				voiceInfo.InstrumentNumber = data;

				data = trackData[voiceInfo.TrackPosition++];
				if ((data & 0x80) != 0)
				{
					voiceInfo.DelayCounter = (byte)~data;
					return;
				}

				voiceInfo.Effect = (Effect)data;
				voiceInfo.EffectArgument = trackData[voiceInfo.TrackPosition++];
			}
			else
				voiceInfo.DelayCounter--;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize voice with note and sample information
		/// </summary>
		/********************************************************************/
		private void SetupNoteAndSample(VoiceInfo voiceInfo)
		{
			voiceInfo.PortamentoValue = 0;

			if (voiceInfo.Note != 0)
			{
				voiceInfo.FinalVolume = 0;
				voiceInfo.SampleNumberListSpeedCounter = 0;
				voiceInfo.SampleNumberListPosition = 0;
				voiceInfo.ArpeggioListSpeedCounter = 0;
				voiceInfo.ArpeggioListPosition = 0;
				voiceInfo.FrequencyListSpeedCounter = 0;
				voiceInfo.FrequencyListPosition = 0;
				voiceInfo.PortamentoDelayCounter = 0;
				voiceInfo.TonePortamentoIncrementValue = 0;
				voiceInfo.EnvelopeState = EnvelopeState.Attack;
				voiceInfo.SustainCounter = 0;

				if (voiceInfo.InstrumentNumber != 0)
					voiceInfo.Instrument = instruments[voiceInfo.InstrumentNumber - 1 + voiceInfo.InstrumentTranspose];

				voiceInfo.FinalNote = (ushort)(voiceInfo.Note + voiceInfo.Instrument.NoteTranspose);

				sbyte sampleNumber = sampleNumberList[voiceInfo.Instrument.SampleNumberList.ListNumber][0];
				voiceInfo.SampleNumber = (ushort)sampleNumber;

				Sample sample = samples[sampleNumber];

				voiceInfo.SampleData = sample.SampleData;
				voiceInfo.SampleOffset = 0;
				voiceInfo.SampleLength = sample.Length;
				voiceInfo.SampleLoopStart = sample.LoopStart * 2U;
				voiceInfo.SampleLoopLength = sample.LoopLength;

				int note = voiceInfo.FinalNote + arpeggioList[sample.ArpeggioListNumber][0];
				voiceInfo.NotePeriod = Tables.Periods[note];
				voiceInfo.FinalPeriod = voiceInfo.NotePeriod;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Do special synth effect on samples
		/// </summary>
		/********************************************************************/
		private void DoSampleInversionEffect()
		{
			SampleExtra[] samplesTaken = new SampleExtra[4];

			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = voices[i];

				Sample sample = samples[voiceInfo.SampleNumber];
				SampleExtra sampleExtra = sampleExtras[voiceInfo.SampleNumber];

				samplesTaken[i] = sampleExtra;

				if (!sampleExtra.AlreadyTaken)
				{
					sampleExtra.AlreadyTaken = true;

					if (sampleExtra.EffectSpeedCounter == 0)
					{
						sampleExtra.EffectSpeedCounter = sample.CounterInitValue;

						if (sample.EffectMode != 0)
						{
							int endPosition = sample.EffectLength * 2 - 1;

							int position = sample.EffectStartPosition * 2 + sampleExtra.EffectPosition;
							sampleExtra.ModifiedSampleData[position] = (sbyte)~sampleExtra.ModifiedSampleData[position];

							sampleExtra.EffectPosition += sampleExtra.EffectIncrementValue;

							if (sampleExtra.EffectPosition < 0)
							{
								if (sample.EffectMode == 2)
									sampleExtra.EffectPosition = endPosition;
								else
								{
									sampleExtra.EffectPosition -= sampleExtra.EffectIncrementValue;
									sampleExtra.EffectIncrementValue = (short)-sampleExtra.EffectIncrementValue;
								}
							}
							else
							{
								if (sampleExtra.EffectPosition <= endPosition)
								{
									if (sample.EffectMode == 1)
										sampleExtra.EffectPosition = 0;
									else
									{
										sampleExtra.EffectPosition -= sampleExtra.EffectIncrementValue;
										sampleExtra.EffectIncrementValue = (short)-sampleExtra.EffectIncrementValue;
									}
								}
							}
						}
					}
					else
					{
						sampleExtra.EffectSpeedCounter--;
						sampleExtra.EffectSpeedCounter &= 0x1f;
					}
				}
			}

			foreach (SampleExtra sampleExtra in samplesTaken)
				sampleExtra.AlreadyTaken = false;
		}



		/********************************************************************/
		/// <summary>
		/// Handle and processing all real-time effects
		/// </summary>
		/********************************************************************/
		private void DoEffects(VoiceInfo voiceInfo, IChannel channel)
		{
			SetVolume(voiceInfo, channel);
			DoSampleNumberList(voiceInfo, channel);
			HandleTrackEffects(voiceInfo, channel);
			DoArpeggioList(voiceInfo);
			DoFrequencyList(voiceInfo);
			DoPortamento(voiceInfo);
			DoTonePortamento(voiceInfo);

			int period = voiceInfo.FinalPeriod + voiceInfo.PortamentoValue;

			if (period < 95)
				period = 95;
			else if (period > 5760)
				period = 5760;

			voiceInfo.FinalPeriod = (ushort)period;

			channel.SetAmigaPeriod(voiceInfo.FinalPeriod);
		}



		/********************************************************************/
		/// <summary>
		/// Set the volume on the given channel
		/// </summary>
		/********************************************************************/
		private void SetVolume(VoiceInfo voiceInfo, IChannel channel)
		{
			DoEnvelope(voiceInfo);

			voiceInfo.TremoloEffectArgument = 0;
			voiceInfo.TremoloTableIndex = (sbyte)voiceInfo.FinalVolume;

			ushort volume = (ushort)((voiceInfo.FinalVolume * voiceInfo.GlobalVoiceVolume) >> 16);
			channel.SetAmigaVolume(volume);
		}



		/********************************************************************/
		/// <summary>
		/// Handle the envelope for the given voice
		/// </summary>
		/********************************************************************/
		private void DoEnvelope(VoiceInfo voiceInfo)
		{
			Instrument instr = voiceInfo.Instrument;

			if (instr != null)
			{
				switch (voiceInfo.EnvelopeState)
				{
					case EnvelopeState.Attack:
					{
						voiceInfo.FinalVolume += instr.AttackSpeed;

						if (voiceInfo.FinalVolume >= instr.AttackEndVolume)
						{
							voiceInfo.FinalVolume = instr.AttackEndVolume;
							voiceInfo.EnvelopeState = EnvelopeState.Decay;
						}
						break;
					}

					case EnvelopeState.Decay:
					{
						if (instr.DecaySpeed != 0)
						{
							voiceInfo.FinalVolume -= instr.DecaySpeed;

							if (voiceInfo.FinalVolume <= instr.DecayEndVolume)
							{
								voiceInfo.FinalVolume = instr.DecayEndVolume;
								voiceInfo.EnvelopeState = EnvelopeState.Sustain;
							}
						}
						else
							voiceInfo.EnvelopeState = EnvelopeState.Sustain;

						break;
					}

					case EnvelopeState.Sustain:
					{
						if (voiceInfo.SustainCounter != instr.SustainDelay)
							voiceInfo.SustainCounter++;
						else
							voiceInfo.EnvelopeState = EnvelopeState.Release;

						break;
					}

					case EnvelopeState.Release:
					{
						if (instr.ReleaseSpeed != 0)
						{
							voiceInfo.FinalVolume -= instr.ReleaseSpeed;

							if (voiceInfo.FinalVolume <= instr.ReleaseEndVolume)
							{
								voiceInfo.FinalVolume = instr.ReleaseEndVolume;
								voiceInfo.EnvelopeState = EnvelopeState.Done;
							}
						}
						else
							voiceInfo.EnvelopeState = EnvelopeState.Done;

						break;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Switch sample number to the next one in the list
		/// </summary>
		/********************************************************************/
		private void DoSampleNumberList(VoiceInfo voiceInfo, IChannel channel)
		{
			Instrument instr = voiceInfo.Instrument;

			if (instr != null)
			{
				InstrumentList list = instr.SampleNumberList;

				if (list.NumberOfValuesInList != 0)
				{
					if (voiceInfo.SampleNumberListSpeedCounter == list.CounterEndValue)
					{
						voiceInfo.SampleNumberListSpeedCounter = (byte)(list.CounterEndValue - list.StartCounterDeltaValue);

						if (voiceInfo.SampleNumberListPosition == list.NumberOfValuesInList)
							voiceInfo.SampleNumberListPosition = -1;

						voiceInfo.SampleNumberListPosition++;

						sbyte sampleNumber = sampleNumberList[list.ListNumber][voiceInfo.SampleNumberListPosition];

						if (sampleNumber < 0)
							voiceInfo.SampleNumberListPosition--;
						else
						{
							voiceInfo.SampleNumber = (ushort)sampleNumber;

							Sample sample = samples[sampleNumber];

							voiceInfo.SampleData = sample.SampleData;
							voiceInfo.SampleOffset = 0;
							voiceInfo.SampleLoopStart = 0;
							voiceInfo.SampleLoopLength = sample.Length;

							channel.SetSampleNumber(sampleNumber);
							channel.SetSample(voiceInfo.SampleData, voiceInfo.SampleLoopStart, voiceInfo.SampleLoopLength * 2U);
							channel.SetLoop(voiceInfo.SampleLoopStart, voiceInfo.SampleLoopLength * 2U);
						}
					}
					else
						voiceInfo.SampleNumberListSpeedCounter++;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Switch to the next arpeggio value
		/// </summary>
		/********************************************************************/
		private void DoArpeggioList(VoiceInfo voiceInfo)
		{
			Instrument instr = voiceInfo.Instrument;

			if (instr != null)
			{
				InstrumentList list = instr.ArpeggioList;

				if (list.NumberOfValuesInList != 0)
				{
					if (voiceInfo.ArpeggioListSpeedCounter == list.CounterEndValue)
					{
						voiceInfo.ArpeggioListSpeedCounter = (byte)(list.CounterEndValue - list.StartCounterDeltaValue);

						if (voiceInfo.ArpeggioListPosition == list.NumberOfValuesInList)
							voiceInfo.ArpeggioListPosition = -1;

						voiceInfo.ArpeggioListPosition++;

						sbyte arpValue = arpeggioList[list.ListNumber][voiceInfo.ArpeggioListPosition];
						voiceInfo.FinalPeriod = Tables.Periods[arpValue + voiceInfo.FinalNote];
					}
					else
						voiceInfo.ArpeggioListSpeedCounter++;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Switch to the next frequency value
		/// </summary>
		/********************************************************************/
		private void DoFrequencyList(VoiceInfo voiceInfo)
		{
			Instrument instr = voiceInfo.Instrument;

			if (instr != null)
			{
				InstrumentList list = instr.FrequencyList;

				if (list.NumberOfValuesInList != 0)
				{
					if (voiceInfo.FrequencyListSpeedCounter == list.CounterEndValue)
					{
						voiceInfo.FrequencyListSpeedCounter = (byte)(list.CounterEndValue - list.StartCounterDeltaValue);

						if (voiceInfo.FrequencyListPosition == list.NumberOfValuesInList)
							voiceInfo.FrequencyListPosition = -1;

						voiceInfo.FrequencyListPosition++;

						sbyte freqValue = frequencyList[list.ListNumber][voiceInfo.FrequencyListPosition];
						voiceInfo.FinalPeriod = (ushort)(voiceInfo.FinalPeriod + freqValue);
					}
					else
						voiceInfo.FrequencyListSpeedCounter++;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Calculate new portamento value
		/// </summary>
		/********************************************************************/
		private void DoPortamento(VoiceInfo voiceInfo)
		{
			Instrument instr = voiceInfo.Instrument;

			if (instr != null)
			{
				if (instr.PortamentoIncrement != 0)
				{
					if (voiceInfo.PortamentoDelayCounter == instr.PortamentoDelay)
						voiceInfo.PortamentoValue += instr.PortamentoIncrement;
					else
						voiceInfo.PortamentoDelayCounter++;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Do the tone portamento
		/// </summary>
		/********************************************************************/
		private void DoTonePortamento(VoiceInfo voiceInfo)
		{
			if ((voiceInfo.Effect != Effect.None) && (voiceInfo.Effect < Effect.Arpeggio) && (voiceInfo.EffectArgument != 0))
			{
				Instrument instr = voiceInfo.Instrument;

				byte endNote = (byte)(((sbyte)voiceInfo.Effect) + voiceInfo.NoteTranspose + instr.NoteTranspose);
				voiceInfo.TonePortamentoEndPeriod = Tables.Periods[endNote];

				int speed = voiceInfo.EffectArgument;

				int delta = voiceInfo.TonePortamentoEndPeriod - voiceInfo.FinalPeriod;
				if (delta != 0)
				{
					if (delta < 0)
						speed = -speed;

					voiceInfo.TonePortamentoIncrementValue = (short)speed;
				}
			}

			if (voiceInfo.TonePortamentoIncrementValue != 0)
			{
				if (voiceInfo.TonePortamentoIncrementValue < 0)
				{
					voiceInfo.FinalPeriod = (ushort)(voiceInfo.FinalPeriod + voiceInfo.TonePortamentoIncrementValue);

					if (voiceInfo.FinalPeriod <= voiceInfo.TonePortamentoEndPeriod)
					{
						voiceInfo.TonePortamentoIncrementValue = 0;
						voiceInfo.FinalPeriod = voiceInfo.TonePortamentoEndPeriod;
						voiceInfo.NotePeriod = voiceInfo.FinalPeriod;
					}
				}
				else
				{
					voiceInfo.FinalPeriod = (ushort)(voiceInfo.FinalPeriod + voiceInfo.TonePortamentoIncrementValue);

					if (voiceInfo.FinalPeriod >= voiceInfo.TonePortamentoEndPeriod)
					{
						voiceInfo.TonePortamentoIncrementValue = 0;
						voiceInfo.FinalPeriod = voiceInfo.TonePortamentoEndPeriod;
						voiceInfo.NotePeriod = voiceInfo.FinalPeriod;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle all the track effects
		/// </summary>
		/********************************************************************/
		private void HandleTrackEffects(VoiceInfo voiceInfo, IChannel channel)
		{
			if (playingInfo.SpeedCounter != 0)
			{
				switch (voiceInfo.Effect)
				{
					case Effect.Arpeggio:
					{
						DoEffectArpeggio(voiceInfo);
						break;
					}

					case Effect.SlideUp:
					{
						DoEffectSlideUp(voiceInfo);
						break;
					}

					case Effect.SlideDown:
					{
						DoEffectSlideDown(voiceInfo);
						break;
					}

					case Effect.VolumeSlideAfterEnvelope:
					{
						DoEffectVolumeSlideAfterEnvelope(voiceInfo);
						break;
					}

					case Effect.Vibrato:
					{
						DoEffectVibrato(voiceInfo);
						break;
					}
				}
			}

			switch (voiceInfo.Effect)
			{
				case Effect.SetRows:
				{
					DoEffectSetRows(voiceInfo);
					break;
				}

				case Effect.SetSampleOffset:
				{
					DoEffectSetSampleOffset(voiceInfo);
					break;
				}

				case Effect.NoteDelay:
				{
					DoEffectNoteDelay(voiceInfo);
					break;
				}

				case Effect.Mute:
				{
					DoEffectMute(voiceInfo, channel);
					break;
				}

				case Effect.SampleRestart:
				{
					DoEffectSampleRestart(voiceInfo);
					break;
				}

				case Effect.Tremolo:
				{
					DoEffectTremolo(voiceInfo, channel);
					break;
				}

				case Effect.Break:
				{
					DoEffectBreak();
					break;
				}

				case Effect.SetVolume:
				{
					DoEffectSetVolume(voiceInfo, channel);
					break;
				}

				case Effect.VolumeSlide:
				{
					DoEffectVolumeSlide(voiceInfo);
					break;
				}

				case Effect.VolumeSlideAndVibrato:
				{
					DoEffectVolumeSlideAndVibrato(voiceInfo);
					break;
				}

				case Effect.SetSpeed:
				{
					DoEffectSetSpeed(voiceInfo);
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 70: Arpeggio
		/// </summary>
		/********************************************************************/
		private void DoEffectArpeggio(VoiceInfo voiceInfo)
		{
			ushort arg = voiceInfo.EffectArgument;

			byte[] arpList =
			[
				(byte)(arg >> 4),
				0,
				(byte)(arg & 0x0f),
				0
			];

			byte arpVal = arpList[playingInfo.MeasureCounter];
			voiceInfo.FinalPeriod = Tables.Periods[voiceInfo.FinalNote + arpVal];
		}



		/********************************************************************/
		/// <summary>
		/// 71: Slide up
		/// </summary>
		/********************************************************************/
		private void DoEffectSlideUp(VoiceInfo voiceInfo)
		{
			voiceInfo.PortamentoValue = (short)-voiceInfo.EffectArgument;
			voiceInfo.NotePeriod = (ushort)(voiceInfo.NotePeriod + voiceInfo.PortamentoValue);
		}



		/********************************************************************/
		/// <summary>
		/// 72: Slide down
		/// </summary>
		/********************************************************************/
		private void DoEffectSlideDown(VoiceInfo voiceInfo)
		{
			voiceInfo.PortamentoValue = (short)voiceInfo.EffectArgument;
			voiceInfo.NotePeriod = (ushort)(voiceInfo.NotePeriod + voiceInfo.PortamentoValue);
		}



		/********************************************************************/
		/// <summary>
		/// 73: Volume slide after envelope
		/// </summary>
		/********************************************************************/
		private void DoEffectVolumeSlideAfterEnvelope(VoiceInfo voiceInfo)
		{
			if (voiceInfo.EnvelopeState == EnvelopeState.Done)
			{
				if ((playingInfo.SpeedCounter == 0) && (voiceInfo.InstrumentNumber != 0))
				{
					if (voiceInfo.Instrument == null)
						return;

					voiceInfo.FinalVolume = voiceInfo.Instrument.AttackSpeed;
				}

				DoVolumeSlide(voiceInfo);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 74: Vibrato
		/// </summary>
		/********************************************************************/
		private void DoEffectVibrato(VoiceInfo voiceInfo)
		{
			ushort arg = voiceInfo.EffectArgument;

			if (arg != 0)
			{
				byte newArg = voiceInfo.VibratoEffectArgument;

				if ((arg & 0x0f) != 0)
					newArg = (byte)((newArg & 0xf0) | (arg & 0x0f));

				if ((arg & 0xf0) != 0)
					newArg = (byte)((newArg & 0x0f) | (arg & 0xf0));

				voiceInfo.VibratoEffectArgument = newArg;
			}

			DoVibrato(voiceInfo);
		}



		/********************************************************************/
		/// <summary>
		/// 75: Set rows
		/// </summary>
		/********************************************************************/
		private void DoEffectSetRows(VoiceInfo voiceInfo)
		{
			playingInfo.NumberOfRows = (byte)voiceInfo.EffectArgument;
		}



		/********************************************************************/
		/// <summary>
		/// 76: Set sample offset
		/// </summary>
		/********************************************************************/
		private void DoEffectSetSampleOffset(VoiceInfo voiceInfo)
		{
			if (voiceInfo.EffectArgument != 0)
				voiceInfo.SampleOffsetEffectArgument = voiceInfo.EffectArgument;

			ushort offset = (ushort)(voiceInfo.SampleOffsetEffectArgument << 7);

			if (offset < voiceInfo.SampleLength)
			{
				voiceInfo.SampleLength -= offset;
				voiceInfo.SampleOffset += offset * 2U;
			}
			else
				voiceInfo.SampleLength = 1;
		}



		/********************************************************************/
		/// <summary>
		/// 77: Note delay
		/// </summary>
		/********************************************************************/
		private void DoEffectNoteDelay(VoiceInfo voiceInfo)
		{
			if (voiceInfo.EffectArgument != 0)
			{
				voiceInfo.NoteDelayCounter = voiceInfo.EffectArgument;

				voiceInfo.Effect = Effect.None;
				voiceInfo.EffectArgument = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 78: Mute
		/// </summary>
		/********************************************************************/
		private void DoEffectMute(VoiceInfo voiceInfo, IChannel channel)
		{
			if ((voiceInfo.EffectArgument != 0) && (playingInfo.SpeedCounter == 0))
			{
				voiceInfo.FinalVolume = 0;
				voiceInfo.TremoloEffectArgument = 0;
				voiceInfo.TremoloTableIndex = 0;

				channel.SetAmigaVolume(0);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 79: Sample restart
		/// </summary>
		/********************************************************************/
		private void DoEffectSampleRestart(VoiceInfo voiceInfo)
		{
			if (voiceInfo.EffectArgument != 0)
			{
				voiceInfo.RestartDelayCounter = voiceInfo.EffectArgument;
				voiceInfo.RestartSampleData = voiceInfo.SampleData;
				voiceInfo.RestartSampleOffset = voiceInfo.SampleOffset;
				voiceInfo.RestartSampleLength = voiceInfo.SampleLength;

				voiceInfo.Effect = Effect.None;
				voiceInfo.EffectArgument = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 7A: Tremolo
		/// </summary>
		/********************************************************************/
		private void DoEffectTremolo(VoiceInfo voiceInfo, IChannel channel)
		{
			ushort arg = voiceInfo.EffectArgument;

			if (arg != 0)
			{
				byte newArg = voiceInfo.TremoloEffectArgument;

				if ((arg & 0x0f) != 0)
					newArg = (byte)((newArg & 0xf0) | (arg & 0x0f));

				if ((arg & 0xf0) != 0)
					newArg = (byte)((newArg & 0x0f) | (arg & 0xf0));

				voiceInfo.TremoloEffectArgument = newArg;
			}

			byte val = Tables.Sinus[(voiceInfo.TremoloTableIndex >> 2) & 0x1f];
			int vibVal = (((voiceInfo.TremoloEffectArgument & 0x0f) * val) >> 6);

			if (voiceInfo.TremoloTableIndex >= 0)
				vibVal = -vibVal;

			short volume = (short)(voiceInfo.TremoloVolume - vibVal);

			if (volume < 0)
				volume = 0;

			if (volume > 64)
				volume = 64;

			volume *= 4;

			if (volume == 256)
				volume = 255;

			voiceInfo.FinalVolume = volume;

			channel.SetAmigaVolume((ushort)((volume * voiceInfo.GlobalVoiceVolume) >> 16));

			voiceInfo.TremoloTableIndex += (sbyte)((voiceInfo.TremoloEffectArgument >> 2) & 0x3c);
		}



		/********************************************************************/
		/// <summary>
		/// 7B: Break
		/// </summary>
		/********************************************************************/
		private void DoEffectBreak()
		{
			playingInfo.CurrentRowPosition = (byte)(playingInfo.NumberOfRows - 1);
		}



		/********************************************************************/
		/// <summary>
		/// 7C: Set volume
		/// </summary>
		/********************************************************************/
		private void DoEffectSetVolume(VoiceInfo voiceInfo, IChannel channel)
		{
			short volume = (short)(voiceInfo.EffectArgument * 4);

			if (volume > 255)
				volume = 255;

			voiceInfo.FinalVolume = volume;

			channel.SetAmigaVolume((ushort)((volume * voiceInfo.GlobalVoiceVolume) >> 16));
		}



		/********************************************************************/
		/// <summary>
		/// 7D: Volume slide
		/// </summary>
		/********************************************************************/
		private void DoEffectVolumeSlide(VoiceInfo voiceInfo)
		{
			if (playingInfo.SpeedCounter == 0)
			{
				if (voiceInfo.Instrument == null)
					return;

				voiceInfo.FinalVolume = voiceInfo.Instrument.AttackSpeed;
			}

			DoVolumeSlide(voiceInfo);
		}



		/********************************************************************/
		/// <summary>
		/// 7E: Volume slide and vibrato
		/// </summary>
		/********************************************************************/
		private void DoEffectVolumeSlideAndVibrato(VoiceInfo voiceInfo)
		{
			DoEffectVolumeSlideAfterEnvelope(voiceInfo);
			DoVibrato(voiceInfo);
		}



		/********************************************************************/
		/// <summary>
		/// 7F: Set speed
		/// </summary>
		/********************************************************************/
		private void DoEffectSetSpeed(VoiceInfo voiceInfo)
		{
			if (voiceInfo.EffectArgument < 31)
			{
				playingInfo.CurrentSpeed = (byte)voiceInfo.EffectArgument;

				ShowSpeed();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Volume slide
		/// </summary>
		/********************************************************************/
		private void DoVolumeSlide(VoiceInfo voiceInfo)
		{
			ushort arg = voiceInfo.EffectArgument;

			if ((arg & 0x0f) != 0)
			{
				int volume = voiceInfo.FinalVolume - (arg * 4);
				if (volume < 0)
					volume = 0;

				voiceInfo.FinalVolume = (short)volume;

				voiceInfo.TremoloEffectArgument = 0;
				voiceInfo.TremoloTableIndex = (sbyte)volume;
			}
			else
			{
				int volume = voiceInfo.FinalVolume + ((arg >> 4) * 4);
				if (volume > 255)
					volume = 255;

				voiceInfo.FinalVolume = (short)volume;

				voiceInfo.TremoloEffectArgument = 0;
				voiceInfo.TremoloTableIndex = (sbyte)volume;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Vibrato
		/// </summary>
		/********************************************************************/
		private void DoVibrato(VoiceInfo voiceInfo)
		{
			byte val = Tables.Sinus[(voiceInfo.VibratoTableIndex >> 2) & 0x1f];
			int vibVal = (((voiceInfo.VibratoEffectArgument & 0x0f) * val) >> 7);

			if (voiceInfo.VibratoTableIndex >= 0)
				vibVal = -vibVal;

			voiceInfo.FinalPeriod = (ushort)(voiceInfo.NotePeriod - vibVal);
			voiceInfo.VibratoTableIndex += (sbyte)(voiceInfo.VibratoEffectArgument >> 2);
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current position
		/// </summary>
		/********************************************************************/
		private void ShowPosition()
		{
			OnModuleInfoChanged(InfoPositionLine, FormatPosition());
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
		/// Will update the module information with all dynamic values
		/// </summary>
		/********************************************************************/
		private void UpdateModuleInformation()
		{
			ShowPosition();
			ShowTracks();
			ShowSpeed();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the position length
		/// </summary>
		/********************************************************************/
		private string FormatPositionLength()
		{
			return (currentSongInfo.EndPosition - currentSongInfo.StartPosition + 1).ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the current position
		/// </summary>
		/********************************************************************/
		private string FormatPosition()
		{
			return (playingInfo.CurrentPosition - currentSongInfo.StartPosition).ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing tracks
		/// </summary>
		/********************************************************************/
		private string FormatTracks()
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < 4; i++)
			{
				sb.Append(voices[i].PositionList[playingInfo.CurrentPosition].TrackNumber);
				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}
		#endregion
	}
}
