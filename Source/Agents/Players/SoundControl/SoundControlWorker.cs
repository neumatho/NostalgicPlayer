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
using Polycode.NostalgicPlayer.Agent.Player.SoundControl.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SoundControl.Players;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.SoundControl
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class SoundControlWorker : ModulePlayerWithSubSongDurationAgentBase
	{
		private readonly ModuleType currentModuleType;
		private ISoundControlPlayer player;

		private string songName;
		private ModuleData moduleData;

		private int numberOfTracks;

		private const int InfoPositionLine = 3;
		private const int InfoTrackLine = 4;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SoundControlWorker(ModuleType moduleType = ModuleType.Unknown)
		{
			currentModuleType = moduleType;
		}

		#region Identify
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => SoundControlIdentifier.FileExtensions;



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			return AgentResult.Unknown;
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
				Encoding encoder = EncoderCollection.Amiga;

				ModuleStream moduleStream = fileInfo.ModuleStream;

				// Read header
				songName = moduleStream.ReadString(encoder, 16).TrimEnd();

				uint tracksLen = moduleStream.Read_B_UINT32();
				uint samplesLen = moduleStream.Read_B_UINT32();
				uint positionListLen = moduleStream.Read_B_UINT32();
				uint sampleCommandsLen = moduleStream.Read_B_UINT32();
				uint totalLength = 64 + tracksLen + samplesLen + positionListLen + sampleCommandsLen;

				moduleData = new ModuleData();

				moduleStream.Seek(2, SeekOrigin.Current);

				moduleData.Speed = moduleStream.Read_B_UINT16();

				if (!LoadTracks(moduleStream))
				{
					errorMessage = Resources.IDS_SC_ERR_LOADING_TRACKS;
					return AgentResult.Error;
				}

				if (!LoadPositionList(moduleStream, 64 + tracksLen + samplesLen, positionListLen))
				{
					errorMessage = Resources.IDS_SC_ERR_LOADING_POSITIONLIST;
					return AgentResult.Error;
				}

				if (!LoadInstruments(moduleStream, 64 + tracksLen + samplesLen + positionListLen, sampleCommandsLen))
				{
					errorMessage = Resources.IDS_SC_ERR_LOADING_INSTRUMENTS;
					return AgentResult.Error;
				}

				if (!LoadSamples(moduleStream, 64 + tracksLen, encoder, out errorMessage))
					return AgentResult.Error;

				player = CreatePlayer(totalLength);
			}
			catch (Exception)
			{
				Cleanup();
				throw;
			}

			// Ok, we're done
			return AgentResult.Ok;
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

			// Initialize structures
			player.InitPlayer();

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

			player.InitSound(songNumber);

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
			if (player.Play())
			{
				// Tell NostalgicPlayer that the song has ended
				OnEndReachedOnAllChannels(0);
			}
		}
		#endregion

		#region Information
		/********************************************************************/
		/// <summary>
		/// Return the title
		/// </summary>
		/********************************************************************/
		public override string Title => songName;



		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public override SubSongInfo SubSongs =>  new SubSongInfo(player.NumberOfSubSongs, 0);



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
				ushort[] periods = player.PeriodTable.ToArray();

				for (int j = 0; j < 8 * 12; j++)
				{
					ushort period = periods[j];
					freqs[j] = PeriodToFrequency(period);
				}

				foreach (Sample sample in moduleData.Samples)
				{
					SampleInfo sampleInfo = new SampleInfo
					{
						Name = sample.Name,
						Flags = SampleInfo.SampleFlag.None,
						Type = SampleInfo.SampleType.Sample,
						Volume = 256,
						Panning = -1,
						Sample = sample.SampleData,
						Length = sample.SampleData != null ? (uint)sample.SampleData.Length : 0U,
						NoteFrequencies = freqs
					};

					if (sample.LoopEnd != 0)
					{
						sampleInfo.LoopStart = sample.LoopStart;
						sampleInfo.LoopLength = sample.LoopEnd - sampleInfo.LoopStart;
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
					description = Resources.IDS_SC_INFODESCLINE0;
					value = player.NumberOfPositions.ToString();
					break;
				}

				// Used tracks
				case 1:
				{
					description = Resources.IDS_SC_INFODESCLINE1;
					value = numberOfTracks.ToString();
					break;
				}

				// Used samples
				case 2:
				{
					description = Resources.IDS_SC_INFODESCLINE2;
					value = moduleData.Samples.Length.ToString();
					break;
				}

				// Playing position
				case 3:
				{
					description = Resources.IDS_SC_INFODESCLINE3;
					value = FormatPosition();
					break;
				}

				// Playing tracks
				case 4:
				{
					description = Resources.IDS_SC_INFODESCLINE4;
					value = FormatTracks();
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
			player.InitSound(subSong);
		}



		/********************************************************************/
		/// <summary>
		/// Create a snapshot of all the internal structures and return it
		/// </summary>
		/********************************************************************/
		protected override ISnapshot CreateSnapshot()
		{
			return player.Snapshot;
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

			player.Snapshot = clonedSnapshot;

			UpdateModuleInformation();

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Create the right player instance based on the module type
		/// </summary>
		/********************************************************************/
		private ISoundControlPlayer CreatePlayer(uint totalLength)
		{
			switch (currentModuleType)
			{
				case ModuleType.SoundControl3x:
					return new SoundControl3xPlayer(this, moduleData, totalLength);

				case ModuleType.SoundControl40:
					return new SoundControl40_50Player(this, moduleData, totalLength, ModuleType.SoundControl40);

				case ModuleType.SoundControl50:
					return new SoundControl40_50Player(this, moduleData, totalLength, ModuleType.SoundControl50);

				default:
					return null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Load all the tracks
		/// </summary>
		/********************************************************************/
		private bool LoadTracks(ModuleStream moduleStream)
		{
			ushort[] offsets = new ushort[256];

			moduleStream.Seek(64, SeekOrigin.Begin);

			moduleStream.ReadArray_B_UINT16s(offsets, 0, 256);
			if (moduleStream.EndOfStream)
				return false;

			moduleData.Tracks = new byte[256][];
			numberOfTracks = 0;

			for (int i = 0; i < 256; i++)
			{
				ushort offset = offsets[i];
				if (offset == 0)
					continue;

				moduleStream.Seek(64 + offset, SeekOrigin.Begin);

				byte[] track = LoadSingleTrack(moduleStream);
				if (track == null)
					return false;

				moduleData.Tracks[i] = track;
				numberOfTracks = i;
			}

			numberOfTracks++;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single track
		/// </summary>
		/********************************************************************/
		private byte[] LoadSingleTrack(ModuleStream moduleStream)
		{
			// Skip track name
			moduleStream.Seek(16, SeekOrigin.Current);

			List<byte> trackData = new List<byte>();

			for (;;)
			{
				byte dat1 = moduleStream.Read_UINT8();
				byte dat2 = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return null;

				trackData.Add(dat1);
				trackData.Add(dat2);

				if (dat1 == 0xff)
					break;

				trackData.Add(moduleStream.Read_UINT8());
				trackData.Add(moduleStream.Read_UINT8());
			}

			return trackData.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Load the position list
		/// </summary>
		/********************************************************************/
		private bool LoadPositionList(ModuleStream moduleStream, uint startOffset, uint positionLength)
		{
			moduleStream.Seek(startOffset, SeekOrigin.Begin);

			positionLength /= 12;

			moduleData.PositionList = new PositionList();

			for (int i = 0; i < 6; i++)
				moduleData.PositionList.Positions[i] = new Position[positionLength];

			for (int i = 0; i < positionLength; i++)
			{
				for (int j = 0; j < 6; j++)
				{
					Position position = new Position();

					position.TrackNumber = moduleStream.Read_UINT8();
					moduleStream.Read_UINT8();

					moduleData.PositionList.Positions[j][i] = position;
				}

				if (moduleStream.EndOfStream)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load the instruments
		/// </summary>
		/********************************************************************/
		private bool LoadInstruments(ModuleStream moduleStream, uint startOffset, uint instrumentsLength)
		{
			if (instrumentsLength == 0)
				return true;

			ushort[] offsets = new ushort[256];

			moduleStream.Seek(startOffset, SeekOrigin.Begin);

			moduleStream.ReadArray_B_UINT16s(offsets, 0, 256);
			if (moduleStream.EndOfStream)
				return false;

			Instrument[] loadedInstruments = new Instrument[256];
			int lastLoaded = 0;

			for (int i = 0; i < 256; i++)
			{
				uint offset = offsets[i];
				if (offset == 0)
					continue;

				moduleStream.Seek(startOffset + offset, SeekOrigin.Begin);

				// Skip name
				moduleStream.Seek(16, SeekOrigin.Current);

				ushort sampleCommandLength = moduleStream.Read_B_UINT16();
				if (moduleStream.EndOfStream)
					return false;

				Instrument instrument = new Instrument();

				instrument.Envelope = LoadEnvelope(moduleStream);
				if (instrument.Envelope == null)
					return false;

				// Skip not used data
				moduleStream.Seek(22, SeekOrigin.Current);

				instrument.SampleCommands = LoadSampleCommandList(moduleStream, sampleCommandLength);
				if (instrument.SampleCommands == null)
					return false;

				lastLoaded = i;

				loadedInstruments[i] = instrument;
			}

			moduleData.Instruments = loadedInstruments
				.Take(lastLoaded + 1)
				.ToArray();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single envelope
		/// </summary>
		/********************************************************************/
		private Envelope LoadEnvelope(ModuleStream moduleStream)
		{
			Envelope envelope = new Envelope
			{
				AttackSpeed = moduleStream.Read_UINT8(),
				AttackIncrement = moduleStream.Read_UINT8(),
				DecaySpeed = moduleStream.Read_UINT8(),
				DecayDecrement = moduleStream.Read_UINT8(),
				DecayValue = moduleStream.Read_B_UINT16(),
				ReleaseSpeed = moduleStream.Read_UINT8(),
				ReleaseDecrement = moduleStream.Read_UINT8()
			};

			if (moduleStream.EndOfStream)
				return null;

			return envelope;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single sample command list
		/// </summary>
		/********************************************************************/
		private SampleCommandInfo[] LoadSampleCommandList(ModuleStream moduleStream, ushort sampleCommandLength)
		{
			SampleCommandInfo[] sampleCommands = new SampleCommandInfo[sampleCommandLength / 6];

			for (int i = 0; i < sampleCommands.Length; i++)
			{
				SampleCommandInfo singleCommand = new SampleCommandInfo
				{
					Command = (SampleCommand)moduleStream.Read_B_UINT16(),
					Argument1 = moduleStream.Read_B_UINT16(),
					Argument2 = moduleStream.Read_B_UINT16()
				};

				if (moduleStream.EndOfStream)
					return null;

				sampleCommands[i] = singleCommand;
			}

			return sampleCommands;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the samples
		/// </summary>
		/********************************************************************/
		private bool LoadSamples(ModuleStream moduleStream, uint startOffset, Encoding encoder, out string errorMessage)
		{
			errorMessage = string.Empty;

			uint[] offsets = new uint[256];

			moduleStream.Seek(startOffset, SeekOrigin.Begin);

			moduleStream.ReadArray_B_UINT32s(offsets, 0, 256);
			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_SC_ERR_LOADING_SAMPLEINFO;
				return false;
			}

			Sample[] loadedSamples = new Sample[256];
			int lastLoaded = 0;

			for (int i = 0; i < 256; i++)
			{
				uint offset = offsets[i];
				if (offset == 0)
					continue;

				moduleStream.Seek(startOffset + offset, SeekOrigin.Begin);

				Sample sample = LoadSingleSample(moduleStream, encoder, i, out errorMessage);
				if (sample == null)
					return false;

				lastLoaded = i;

				loadedSamples[i] = sample;
			}

			moduleData.Samples = loadedSamples
				.Take(lastLoaded + 1)
				.Select(x => x ?? new Sample { Name = string.Empty })
				.ToArray();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single sample
		/// </summary>
		/********************************************************************/
		private Sample LoadSingleSample(ModuleStream moduleStream, Encoding encoder, int sampleNumber, out string errorMessage)
		{
			errorMessage = string.Empty;

			Sample sample = new Sample();

			sample.Name = moduleStream.ReadString(encoder, 16).TrimEnd();
			sample.Length = moduleStream.Read_B_UINT16();
			sample.LoopStart = moduleStream.Read_B_UINT16();
			sample.LoopEnd = moduleStream.Read_B_UINT16();

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_SC_ERR_LOADING_SAMPLEINFO;
				return null;
			}

			moduleStream.Seek(20, SeekOrigin.Current);
			sample.NoteTranspose = moduleStream.Read_B_INT16();

			moduleStream.Seek(16, SeekOrigin.Current);

			uint realSampleLength = moduleStream.Read_B_UINT32();
			realSampleLength -= 64;

			sample.SampleData = moduleStream.ReadSampleData(sampleNumber, (int)realSampleLength, out int readBytes);
			if (readBytes != realSampleLength)
			{
				errorMessage = Resources.IDS_SC_ERR_LOADING_SAMPLES;
				return null;
			}

			return sample;
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			player = null;
			moduleData = null;
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current song position
		/// </summary>
		/********************************************************************/
		public void ShowSongPosition()
		{
			OnModuleInfoChanged(InfoPositionLine, FormatPosition());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with track numbers
		/// </summary>
		/********************************************************************/
		public void ShowTracks()
		{
			OnModuleInfoChanged(InfoTrackLine, FormatTracks());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with all dynamic values
		/// </summary>
		/********************************************************************/
		private void UpdateModuleInformation()
		{
			ShowSongPosition();
			ShowTracks();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing position
		/// </summary>
		/********************************************************************/
		private string FormatPosition()
		{
			return player.SongPosition.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing tracks
		/// </summary>
		/********************************************************************/
		private string FormatTracks()
		{
			StringBuilder sb = new StringBuilder();
			ushort songPosition = player.SongPosition;

			for (int i = 0; i < 4; i++)
			{
				sb.Append(moduleData.PositionList.Positions[i][songPosition].TrackNumber);
				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}
		#endregion
	}
}
