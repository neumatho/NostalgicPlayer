/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris.Dns;
using Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Containers;
using Decoder = Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Decoder;

namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class DynamicSynthesizerWorker : ModulePlayerWithSubSongDurationAgentBase
	{
		private byte[] musicData;
		private int musicLen;

		private sbyte[] sampleData;
		private int sampleLen;

		private int songCount;
		private int trackCount;

		private Decoder decoder;

		private int positionCount;
		private int currentPosition;
		private byte[] currentTracks;

		private TfmxVoice[] voices;

		private const int InfoPositionLine = 2;
		private const int InfoTrackLine = 3;

		#region Identify
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => DynamicSynthesizerIdentifier.FileExtensions;



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

				// Find the other file and read the samples from it
				ModuleStream sampleStream = fileInfo.Loader?.OpenExtraFileByExtension("smp");
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

			decoder = new DnsDecoder();

			if (!decoder.Init(musicData.ToPointer(), (uint)musicLen, sampleData.ToPointer(), (uint)sampleLen, out errorMessage))
				return false;

			// Data has been copied above, so we don't need our own buffers anymore,
			// so set them to null to save memory
			musicData = null;
			sampleData = null;

			songCount = decoder.GetSongs();
			trackCount = decoder.GetTracks();

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

			RefreshModuleInformation();

			if (decoder.GetSongEndFlag())
				OnEndReachedOnAllChannels(decoder.GetPlayingPosition());
		}
		#endregion

		#region Information
		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public override SubSongInfo SubSongs => new SubSongInfo(songCount, 0);



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

				for (int j = 0; j < 4 * 12; j++)
					frequencies[36 + j] = PeriodToFrequency(Tables.Periods[13 + j]);

				foreach (Sample sample in decoder.GetSamples())
				{
					ReadOnlyMemory<byte> sampleBuffer = sample.Start.AsMemory();

					if (MemoryMarshal.TryGetArray(sampleBuffer, out ArraySegment<byte> segment))
					{
						SampleInfo sampleInfo = new SampleInfo
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

						if (sample.LoopLength > 2)
						{
							sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;
							sampleInfo.LoopStart = sample.LoopStartOffset;
							sampleInfo.LoopLength = sample.LoopLength;
						}

						yield return sampleInfo;
					}
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
					value = decoder.GetSpeed().ToString();
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
		/// Return the total number of positions. You only need to derive
		/// from this method, if your player have one position list for all
		/// channels and can restart on another position than 0
		/// </summary>
		/********************************************************************/
		protected override int GetTotalNumberOfPositions()
		{
			return decoder.GetPositions();
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

				MarkPositionAsVisited(newPosition);
			}

			byte[] newTracks = decoder.GetPlayingTracks();
			bool changed = false;

			for (int i = 0; i < newTracks.Length; i++)
			{
				if (newTracks[i] != currentTracks[i])
				{
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
			decoder.InitSong(songNumber);
			decoder.SetLoopMode(true);

			positionCount = decoder.GetPositions();
			currentPosition = 0;
			currentTracks = new byte[4];

			voices = new TfmxVoice[4];
			SampleInfo[] samples = Samples.ToArray();

			for (byte i = 0; i < voices.Length; i++)
			{
				TfmxVoice voice = new TfmxVoice(VirtualChannels[i], samples);

				voices[i] = voice;
				decoder.SetPaulaVoice(i, voice);
			}

			MarkPositionAsVisited(0);
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
		/// Will set local variables to hold different information shown to
		/// the user
		/// </summary>
		/********************************************************************/
		private void SetModuleInformation()
		{
			currentPosition = decoder.GetPlayingPosition();
			currentTracks = decoder.GetPlayingTracks();
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
				if (track == 0)
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
		#endregion
	}
}
