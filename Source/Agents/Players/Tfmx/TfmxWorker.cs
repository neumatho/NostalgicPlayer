/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.Tfmx.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Types;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris;
using Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Containers;
using Decoder = Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Decoder;

namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class TfmxWorker : ModulePlayerWithSubSongDurationAgentBase
	{
		private readonly ModuleType currentModuleType;
		private readonly bool isSingleFile;

		private byte[] musicData;
		private int musicLen;

		private sbyte[] sampleData;
		private int sampleLen;

		private string moduleName;
		private string author;

		private string[] comment;

		private int songCount;
		private int voiceCount;
		private int trackCount;

		private Decoder decoder;

		private uint currentRate;
		private int currentSpeed;

		private int positionCount;
		private int currentPosition;
		private byte[] currentTracks;

		private TfmxVoice[] voices;

		private SampleInfo[] sampleInfo;

		private const int InfoPositionLine = 2;
		private const int InfoTrackLine = 3;
		private const int InfoSpeedLine = 4;
		private const int InfoTempoLine = 5;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public TfmxWorker(ModuleType moduleType = ModuleType.Unknown, bool singleFile = false)
		{
			currentModuleType = moduleType;
			isSingleFile = singleFile;
		}

		#region Identify
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => TfmxIdentifier.FileExtensions;



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
				ModuleStream moduleStream = fileInfo.ModuleStream;

				// Just load the whole file into memory
				musicLen = (int)moduleStream.Length;
				musicData = new byte[musicLen];

				if (moduleStream.Read(musicData) != musicLen)
				{
					errorMessage = Resources.IDS_TFMX_ERR_LOADING;
					Cleanup();

					return AgentResult.Error;
				}

				if (!isSingleFile)
				{
					// Two file format. Find the other file and read the samples from it
					ModuleStream sampleStream = fileInfo.Loader?.OpenExtraFileByExtension("smpl");
					if (sampleStream == null)
						sampleStream = fileInfo.Loader?.OpenExtraFileByExtension("sam");

					if (sampleStream == null)
					{
						errorMessage = Resources.IDS_TFMX_ERR_LOADING_SAMPLE;
						Cleanup();

						return AgentResult.Error;
					}

					try
					{
						// Read the samples
						sampleLen = (int)sampleStream.Length;
						sampleData = sampleStream.ReadSampleData(0, sampleLen, out int readBytes);

						if (readBytes != sampleLen)
						{
							errorMessage = Resources.IDS_TFMX_ERR_LOADING_SAMPLE;
							Cleanup();

							return AgentResult.Error;
						}
					}
					finally
					{
						sampleStream.Dispose();
					}
				}
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

			decoder = new TfmxDecoder();

			if (!decoder.Init(musicData.ToPointer(), (uint)musicLen, sampleData.ToPointer(), (uint)sampleLen, out errorMessage))
				return false;

			// Data has been copied above, so we don't need our own buffers anymore,
			// so set them to null to save memory
			musicData = null;
			sampleData = null;

			// Get number of songs and voices
			songCount = decoder.GetSongs();
			voiceCount = decoder.GetVoices();

			if (voiceCount == 8)
				voiceCount--;

			if (((currentModuleType == ModuleType.Tfmx7V) && (voiceCount != 7)) || ((currentModuleType != ModuleType.Tfmx7V) && (voiceCount != 4)))
			{
				errorMessage = Resources.IDS_TFMX_ERR_VOICE_COUNT;
				return false;
			}

			trackCount = decoder.GetTracks();

			// Get information strings
			moduleName = decoder.GetInfoString("game");
			if (!string.IsNullOrEmpty(moduleName))
				moduleName += " - " + decoder.GetInfoString("title");

			author = decoder.GetInfoString("artist");

			string commentStr = decoder.GetInfoString("comment");
			if (!string.IsNullOrEmpty(commentStr))
				comment = commentStr.Split('\n');

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
			decoder.Run();

			// Update voices
			foreach (TfmxVoice voice in voices)
				voice.UpdateRegisters();

			RefreshRate();
			RefreshModuleInformation();

			if (decoder.GetSongEndFlag())
				OnEndReachedOnAllChannels(0);
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
		public override string[] Comment => comment;



		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public override SubSongInfo SubSongs
		{
			get
			{
				return new SubSongInfo(songCount, 0);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public override int ModuleChannelCount => voiceCount;



		/********************************************************************/
		/// <summary>
		/// Returns all the samples available in the module. If none, null
		/// is returned
		/// </summary>
		/********************************************************************/
		public override IEnumerable<SampleInfo> Samples => sampleInfo;



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
					description = Resources.IDS_TFMX_INFODESCLINE0;
					value = positionCount.ToString();
					break;
				}

				// Used tracks
				case 1:
				{
					description = Resources.IDS_TFMX_INFODESCLINE1;
					value = trackCount.ToString();
					break;
				}

				// Playing position
				case 2:
				{
					description = Resources.IDS_TFMX_INFODESCLINE2;
					value = FormatPosition();
					break;
				}

				// Playing tracks
				case 3:
				{
					description = Resources.IDS_TFMX_INFODESCLINE3;
					value = FormatTracks();
					break;
				}

				// Current speed
				case 4:
				{
					description = Resources.IDS_TFMX_INFODESCLINE4;
					value = FormatSpeed();
					break;
				}

				// Current tempo (Hz)
				case 5:
				{
					description = Resources.IDS_TFMX_INFODESCLINE5;
					value = FormatTempo();
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
		/// Is called when the duration calculation is done for all sub-songs
		/// </summary>
		/********************************************************************/
		protected override void DurationDone()
		{
			// At this point, it is possible to extract the sample data.
			// We cannot do it in InitPlayer(), because we need to know
			// which macros has been used
			ExtractSampleInfo();
		}



		/********************************************************************/
		/// <summary>
		/// Create a snapshot of all the internal structures and return it
		/// </summary>
		/********************************************************************/
		protected override ISnapshot CreateSnapshot()
		{
			return decoder.CreateSnapshot();
		}



		/********************************************************************/
		/// <summary>
		/// Initialize internal structures based on the snapshot given
		/// </summary>
		/********************************************************************/
		protected override bool SetSnapshot(ISnapshot snapshot, out string errorMessage)
		{
			errorMessage = string.Empty;

			decoder.SetSnapshot(snapshot);

			SetModuleInformation();
			UpdateModuleInformation();

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Refresh the playing frequency
		/// </summary>
		/********************************************************************/
		private void RefreshRate()
		{
			uint newRate = decoder.GetRate();
			if (newRate != currentRate)
			{
				currentRate = newRate;
				PlayingFrequency = (float)newRate / 256;
				ShowTempo();
			}

			int newSpeed = decoder.GetSpeed();
			if (newSpeed != currentSpeed)
			{
				currentSpeed = newSpeed;
				ShowSpeed();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Refresh the different module information
		/// </summary>
		/********************************************************************/
		private void RefreshModuleInformation()
		{
			int newPosition = decoder.GetPlayingPosition();
			if (newPosition != currentPosition)
			{
				currentPosition = newPosition;
				ShowSongPosition();
			}

			byte[] newTracks = decoder.GetPlayingTracks();
			bool changed = false;

			for (int i = 0; i < newTracks.Length; i++)
			{
				if (newTracks[i] != currentTracks[i])
				{
					if ((newTracks[i] >= 0x80) && (newTracks[i] < 0x90))
						continue;

					currentTracks[i] = newTracks[i];
					changed = true;
				}
			}

			if (changed)
				ShowTracks();
		}



		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound(int songNumber)
		{
			currentRate = (uint)(PlayingFrequency * 256);
			currentSpeed = 0;

			decoder.InitSong(songNumber);
			decoder.SetLoopMode(true);

			positionCount = decoder.GetPositions();
			currentPosition = 0;
			currentTracks = Enumerable.Repeat((byte)0xff, voiceCount).ToArray();

			voices = new TfmxVoice[voiceCount];

			ChannelPanningType[] pannings = voiceCount == 7 ? Tables.Pan7 : Tables.Pan4;

			for (byte i = 0; i < voices.Length; i++)
			{
				TfmxVoice voice = new TfmxVoice(VirtualChannels[i]);

				voices[i] = voice;
				decoder.SetPaulaVoice(i, voice);
				VirtualChannels[i].SetPanning((ushort)pannings[i]);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			musicData = null;
			sampleData = null;

			decoder = null;
			voices = null;
		}



		/********************************************************************/
		/// <summary>
		/// Extract the sample information by parsing the instrument macros
		/// </summary>
		/********************************************************************/
		private void ExtractSampleInfo()
		{
			// Build frequency table
			uint[] frequencies = new uint[10 * 12];

			for (int j = 0; j < Tables.Periods.Length; j++)
				frequencies[(2 * 12) - 1 + j] = PeriodToFrequency(Tables.Periods[j]);

			List<SampleInfo> samples = new List<SampleInfo>();

			foreach (Sample sample in decoder.GetSamples())
			{
				ReadOnlyMemory<byte> sampleBuffer = sample.Start.AsMemory();

				if (MemoryMarshal.TryGetArray(sampleBuffer, out ArraySegment<byte> segment))
				{
					SampleInfo info = new SampleInfo
					{
						Name = string.Empty,
						Flags = SampleInfo.SampleFlag.None,
						Type = SampleInfo.SampleType.Sample,
						Panning = -1,
						Volume = 256,
						Sample = segment.Array,
						SampleOffset = (uint)segment.Offset,
						Length = sample.Length,
						NoteFrequencies = frequencies
					};

					if (sample.LoopLength != 0)
					{
						info.Flags |= SampleInfo.SampleFlag.Loop;
						info.LoopStart = sample.LoopStartOffset;
						info.LoopLength = sample.LoopLength;
					}

					samples.Add(info);
				}
			}

			sampleInfo = samples.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Will set local variables to hold different information shown to
		/// the user
		/// </summary>
		/********************************************************************/
		private void SetModuleInformation()
		{
			currentRate = decoder.GetRate();
			currentSpeed = decoder.GetSpeed();
			currentPosition = decoder.GetPlayingPosition();
			currentTracks = decoder.GetPlayingTracks();

			PlayingFrequency = (float)currentRate / 256;
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current song position
		/// </summary>
		/********************************************************************/
		private void ShowSongPosition()
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
			OnModuleInfoChanged(InfoSpeedLine, FormatSpeed());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current tempo
		/// </summary>
		/********************************************************************/
		private void ShowTempo()
		{
			OnModuleInfoChanged(InfoTempoLine, FormatTempo());
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
			ShowSpeed();
			ShowTempo();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the current position
		/// </summary>
		/********************************************************************/
		private string FormatPosition()
		{
			return currentPosition.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing tracks
		/// </summary>
		/********************************************************************/
		private string FormatTracks()
		{
			StringBuilder sb = new StringBuilder();

			foreach (byte track in decoder.GetPlayingTracks())
			{
				if (track >= 0x90)
					sb.Append("-, ");
				else
				{
					sb.Append(track);
					sb.Append(", ");
				}
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the current speed
		/// </summary>
		/********************************************************************/
		private string FormatSpeed()
		{
			return currentSpeed.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the current tempo
		/// </summary>
		/********************************************************************/
		private string FormatTempo()
		{
			return PlayingFrequency.ToString("F2", CultureInfo.InvariantCulture);
		}
		#endregion
	}
}
