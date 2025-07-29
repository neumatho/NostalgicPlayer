/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.ActivisionPro.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.ActivisionPro
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class ActivisionProWorker : ModulePlayerWithSubSongDurationAgentBase
	{
		private enum PortamentoVibratoType
		{
			OnlyOne,
			BothTogether
		}

		private int subSongListOffset;
		private int positionListsOffset;
		private int trackOffsetsOffset;
		private int tracksOffset;
		private int envelopesOffset;
		private int instrumentsOffset;
		private int sampleInfoOffset;
		private int sampleStartOffsetsOffset;
		private int sampleDataOffset;
		private int speedVariationSpeedIncrementOffset;

		private int instrumentFormatVersion;
		private int parseTrackVersion;
		private int speedVariationVersion;
		private byte speedVariationSpeedInit;
		private PortamentoVibratoType portamentoVibratoType;
		private int vibratoVersion;
		private bool haveSeparateSampleInfo;
		private bool haveSetNote;
		private bool haveSetFixedSample;
		private bool haveSetArpeggio;
		private bool haveSetSample;
		private bool haveArpeggio;
		private bool haveEnvelope;
		private bool resetVolume;

		private SongInfo[] songInfoList;
		private byte[][] tracks;
		private Envelope[] envelopes;
		private Instrument[] instruments;
		private Sample[] samples;

		private SongInfo currentSongInfo;

		private GlobalPlayingInfo playingInfo;
		private VoiceInfo[] voices;

		private const int InfoPositionLine = 3;
		private const int InfoTrackLine = 4;

		#region Identify
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => [ "avp", "mw" ];



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 1024)
				return AgentResult.Unknown;

			// Read the first part of the file, so it is easier to search
			byte[] buffer = new byte[4096];

			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.ReadInto(buffer, 0, buffer.Length);

			return TestModule(buffer);
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

				if (!LoadSpeedVariation(moduleStream))
				{
					errorMessage = Resources.IDS_AVP_ERR_LOADING_INFO;
					return AgentResult.Error;
				}

				if (!LoadSubSongInfo(moduleStream, out errorMessage))
					return AgentResult.Error;

				if (!LoadTracks(moduleStream))
				{
					errorMessage = Resources.IDS_AVP_ERR_LOADING_TRACKS;
					return AgentResult.Error;
				}

				if (!LoadEnvelopes(moduleStream))
				{
					errorMessage = Resources.IDS_AVP_ERR_LOADING_ENVELOPES;
					return AgentResult.Error;
				}

				if (!LoadInstruments(moduleStream))
				{
					errorMessage = Resources.IDS_AVP_ERR_LOADING_INSTRUMENTS;
					return AgentResult.Error;
				}

				if (!LoadSampleInfo(moduleStream))
				{
					errorMessage = Resources.IDS_AVP_ERR_LOADING_SAMPLEINFO;
					return AgentResult.Error;
				}

				if (!LoadSampleData(moduleStream))
				{
					errorMessage = Resources.IDS_AVP_ERR_LOADING_SAMPLES;
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
			DoMasterVolumeFade();

			DoSpeedVariation2();

			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = voices[i];
				IChannel channel = VirtualChannels[i];

				if (playingInfo.SpeedVariationCounter == 0)
				{
					voiceInfo.SpeedCounter--;
					voiceInfo.SpeedCounter2++;

					if (voiceInfo.SpeedCounter == 0)
					{
						ParseNextTrackPosition(i, voiceInfo, channel);
						RunEffects2(voiceInfo);
						continue;
					}
				}

				RunEffects1(voiceInfo);
			}

			DoSpeedVariation1();

			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = voices[i];
				IChannel channel = VirtualChannels[i];

				uint period = (uint)(voiceInfo.Period - voiceInfo.FineTune);
				channel.SetAmigaPeriod(period);

				int volume;

				if (haveEnvelope)
					volume = (voiceInfo.Volume * playingInfo.MasterVolume) / 64;
				else
				{
					if ((speedVariationVersion == 2) && (voiceInfo.Mute || ((parseTrackVersion != 5) && (voiceInfo.SpeedCounter <= voiceInfo.MaxSpeedCounter)) || ((parseTrackVersion == 5) && (voiceInfo.SpeedCounter2 >= voiceInfo.MaxSpeedCounter))))
						volume = 0;
					else
						volume = (voiceInfo.TrackVolume * voiceInfo.Volume * playingInfo.MasterVolume) / 4096;
				}

				channel.SetAmigaVolume((ushort)volume);
			}
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

				for (int j = 0; j < Tables.Periods.Length; j++)
				{
					ushort period = Tables.Periods[j];
					freqs[j] = PeriodToFrequency(period);
				}

				foreach (Sample sample in samples)
				{
					SampleInfo sampleInfo = new SampleInfo
					{
						Name = string.Empty,
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
						sampleInfo.LoopStart = sample.LoopStart;
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
					description = Resources.IDS_AVP_INFODESCLINE0;
					value = FormatPositionLengths();
					break;
				}

				// Used tracks
				case 1:
				{
					description = Resources.IDS_AVP_INFODESCLINE1;
					value = tracks.Length.ToString();
					break;
				}

				// Used samples
				case 2:
				{
					description = Resources.IDS_AVP_INFODESCLINE2;
					value = samples.Length.ToString();
					break;
				}

				// Playing position
				case 3:
				{
					description = Resources.IDS_AVP_INFODESCLINE3;
					value = FormatPositions();
					break;
				}

				// Playing tracks
				case 4:
				{
					description = Resources.IDS_AVP_INFODESCLINE4;
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
		/// Test the file to see if it's a Martin Walker player and extract
		/// needed information
		/// </summary>
		/********************************************************************/
		private AgentResult TestModule(byte[] buffer)
		{
			int startOffset = FindStartOffset(buffer);
			if (startOffset < 0)
				return AgentResult.Unknown;

			if (!ExtractInfoFromInitFunction(buffer, startOffset))
				return AgentResult.Unknown;

			if (!ExtractInfoFromPlayFunction(buffer, startOffset))
				return AgentResult.Unknown;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Try to find the start of the player code
		/// </summary>
		/********************************************************************/
		private int FindStartOffset(byte[] searchBuffer)
		{
			for (int i = 0; i < 0x400; i++)
			{
				if ((searchBuffer[i] == 0x48) && (searchBuffer[i + 1] == 0xe7) && (searchBuffer[i + 2] == 0xfc) && (searchBuffer[i + 3] == 0xfe))
					return i;
			}

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// Try to find the init function in the player and extract needed
		/// information from it
		/// </summary>
		/********************************************************************/
		private bool ExtractInfoFromInitFunction(byte[] searchBuffer, int startOffset)
		{
			int searchLength = searchBuffer.Length;
			int index;

			for (index = startOffset; index < (searchLength - 6); index += 2)
			{
				if ((searchBuffer[index] == 0xe9) && (searchBuffer[index + 1] == 0x41) && (searchBuffer[index + 2] == 0x70) && (searchBuffer[index + 3] == 0x00) && (searchBuffer[index + 4] == 0x41) && (searchBuffer[index + 5] == 0xfa))
					break;
			}

			if (index >= (searchLength - 6))
				return false;

			subSongListOffset = (((sbyte)searchBuffer[index + 6] << 8) | searchBuffer[index + 7]) + index + 6;

			for (; index < (searchLength - 4); index += 2)
			{
				if ((searchBuffer[index] == 0x4e) && (searchBuffer[index + 1] == 0x75))
					return false;

				if ((searchBuffer[index] == 0x61) && (searchBuffer[index + 1] == 0x00))
					break;
			}

			if (index >= (searchLength - 4))
				return false;

			index = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;

			if (index >= searchLength)
				return false;

			if ((searchBuffer[index] != 0x7a) || (searchBuffer[index + 1] != 0x00))
				return false;

			if ((searchBuffer[index + 6] != 0x49) || (searchBuffer[index + 7] != 0xfa))
				return false;

			positionListsOffset = (((sbyte)searchBuffer[index + 8] << 8) | searchBuffer[index + 9]) + index + 8;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Try to find the play function in the player and extract needed
		/// information from it
		/// </summary>
		/********************************************************************/
		private bool ExtractInfoFromPlayFunction(byte[] searchBuffer, int startOffset)
		{
			int searchLength = searchBuffer.Length;
			int index;

			// Start to find the play function in the player
			for (index = startOffset; index < (searchLength - 8); index += 2)
			{
				if ((searchBuffer[index] == 0x2c) && (searchBuffer[index + 1] == 0x7c) && (searchBuffer[index + 6] == 0x4a) && (searchBuffer[index + 7] == 0x29))
					break;
			}

			if (index >= (searchLength - 8))
				return false;

			int startOfPlay = index;
			int globalOffset = 0;
			instrumentsOffset = 0;

			index -= 4;

			for (; index >= 0; index -= 2)
			{
				if ((searchBuffer[index] == 0x4b) && (searchBuffer[index + 1] == 0xfa))
					instrumentsOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;
				else if ((searchBuffer[index] == 0x43) && (searchBuffer[index + 1] == 0xfa))
					globalOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;

				if ((instrumentsOffset != 0) && (globalOffset != 0))
					break;
			}

			if ((instrumentsOffset == 0) || (globalOffset == 0))
				return false;

			for (index = startOfPlay; index < (searchLength - 16); index += 2)
			{
				if ((searchBuffer[index] == 0x53) && (searchBuffer[index + 1] == 0x69) && (searchBuffer[index + 4] == 0x67))
					break;
			}

			if (index >= (searchLength - 16))
				return false;

			if ((searchBuffer[index + 6] == 0x70) && (searchBuffer[index + 7] == 0x03))
				speedVariationVersion = 1;
			else if ((searchBuffer[index + 6] == 0x7a) && (searchBuffer[index + 7] == 0x00))
			{
				speedVariationVersion = 2;

				if ((searchBuffer[index + 12] != 0xda) || (searchBuffer[index + 13] != 0x29))
					return false;

				speedVariationSpeedIncrementOffset = globalOffset + (((sbyte)searchBuffer[index + 14] << 8) | searchBuffer[index + 15]);
			}
			else
				return false;

			index += 8;

			for (; index < (searchLength - 12); index += 2)
			{
				if ((searchBuffer[index] == 0x7a) && (searchBuffer[index + 1] == 0x00) &&
				    (searchBuffer[index + 2] == 0x1a) && (searchBuffer[index + 3] == 0x31) &&
				    (searchBuffer[index + 6] == 0xda) && (searchBuffer[index + 7] == 0x45) &&
				    (searchBuffer[index + 8] == 0x49) && (searchBuffer[index + 9] == 0xfa))
				{
					break;
				}
			}

			if (index >= (searchLength - 12))
				return false;

			trackOffsetsOffset = (((sbyte)searchBuffer[index + 10] << 8) | searchBuffer[index + 11]) + index + 10;

			index += 12;

			if (index >= (searchLength - 8))
				return false;

			if ((searchBuffer[index] != 0x3a) || (searchBuffer[index + 1] != 0x34) || (searchBuffer[index + 4] != 0x49) || (searchBuffer[index + 5] != 0xfa))
				return false;

			tracksOffset = (((sbyte)searchBuffer[index + 6] << 8) | searchBuffer[index + 7]) + index + 6;

			index += 8;

			for (; index < (searchLength - 6); index += 2)
			{
				if ((searchBuffer[index] == 0x18) && (searchBuffer[index + 1] == 0x31))
					break;
			}

			if (index >= (searchLength - 6))
				return false;

			resetVolume = searchBuffer[index + 4] == 0x66;

			index += 6;

			for (; index < (searchLength - 10); index += 2)
			{
				if ((searchBuffer[index] == 0x42) && (searchBuffer[index + 1] == 0x31))
					break;
			}

			if (index >= (searchLength - 10))
				return false;

			index += 8;

			if ((searchBuffer[index] == 0x08) && (searchBuffer[index + 1] == 0x31))
				parseTrackVersion = 1;
			else if ((searchBuffer[index] == 0x4a) && (searchBuffer[index + 1] == 0x34))
				parseTrackVersion = 2;
			else if ((searchBuffer[index] == 0x1a) && (searchBuffer[index + 1] == 0x34))
				parseTrackVersion = 3;
			else if ((searchBuffer[index] == 0x42) && (searchBuffer[index + 1] == 0x30))
			{
				parseTrackVersion = 4;

				index += 2;

				for (; index < (searchLength - 4); index += 2)
				{
					if ((searchBuffer[index] == 0x31) && (searchBuffer[index + 1] == 0x85))
						break;

					if ((searchBuffer[index] == 0x0c) && (searchBuffer[index + 1] == 0x05) && (searchBuffer[index + 2] == 0x00) && (searchBuffer[index + 3] == 0x84))
					{
						parseTrackVersion = 5;
						break;
					}
				}

				if (index >= (searchLength - 4))
					return false;

				index -= 2;
			}
			else
				return false;

			index += 2;

			for (; index < (searchLength - 2); index += 2)
			{
				if ((searchBuffer[index] == 0x31) && (searchBuffer[index + 1] == 0x85))
					break;
			}

			if (index >= (searchLength - 2))
				return false;

			index += 4;

			instrumentFormatVersion = 0;

			if (index >= (searchLength - 16))
				return false;

			if ((searchBuffer[index] == 0x13) && (searchBuffer[index + 1] == 0xb5) && (searchBuffer[index + 2] == 0x50) && (searchBuffer[index + 3] == 0x02) &&
				(searchBuffer[index + 6] == 0x13) && (searchBuffer[index + 7] == 0xb5) && (searchBuffer[index + 8] == 0x50) && (searchBuffer[index + 9] == 0x07) &&
				(searchBuffer[index + 12] == 0x13) && (searchBuffer[index + 13] == 0xb5) && (searchBuffer[index + 14] == 0x50) && (searchBuffer[index + 15] == 0x0f))
			{
				instrumentFormatVersion = 1;
			}
			else if ((searchBuffer[index] == 0x11) && (searchBuffer[index + 1] == 0xb5) && (searchBuffer[index + 2] == 0x50) && (searchBuffer[index + 3] == 0x01) &&
				(searchBuffer[index + 6] == 0x13) && (searchBuffer[index + 7] == 0xb5) && (searchBuffer[index + 8] == 0x50) && (searchBuffer[index + 9] == 0x02) &&
				(searchBuffer[index + 12] == 0x13) && (searchBuffer[index + 13] == 0xb5) && (searchBuffer[index + 14] == 0x50) && (searchBuffer[index + 15] == 0x07) &&
				(searchBuffer[index + 18] == 0x13) && (searchBuffer[index + 19] == 0xb5) && (searchBuffer[index + 20] == 0x50) && (searchBuffer[index + 21] == 0x0f))
			{
				instrumentFormatVersion = 2;
			}
			else if ((searchBuffer[index] == 0x11) && (searchBuffer[index + 1] == 0xb5) && (searchBuffer[index + 2] == 0x50) && (searchBuffer[index + 3] == 0x01) &&
				(searchBuffer[index + 6] == 0x13) && (searchBuffer[index + 7] == 0xb5) && (searchBuffer[index + 8] == 0x50) && (searchBuffer[index + 9] == 0x02) &&
				(searchBuffer[index + 12] == 0x13) && (searchBuffer[index + 13] == 0xb5) && (searchBuffer[index + 14] == 0x50) && (searchBuffer[index + 15] == 0x03) &&
				(searchBuffer[index + 18] == 0x31) && (searchBuffer[index + 19] == 0xb5) && (searchBuffer[index + 20] == 0x50) && (searchBuffer[index + 21] == 0x04) &&
				(searchBuffer[index + 24] == 0x33) && (searchBuffer[index + 25] == 0x75) && (searchBuffer[index + 26] == 0x50) && (searchBuffer[index + 27] == 0x06) &&
				(searchBuffer[index + 30] == 0x13) && (searchBuffer[index + 31] == 0xb5) && (searchBuffer[index + 32] == 0x50) && (searchBuffer[index + 33] == 0x08) &&
				(searchBuffer[index + 36] == 0x13) && (searchBuffer[index + 37] == 0xb5) && (searchBuffer[index + 38] == 0x50) && (searchBuffer[index + 39] == 0x0f))
			{
				instrumentFormatVersion = 3;
			}
			else
				return false;

			for (; index < (searchLength - 14); index += 2)
			{
				if ((searchBuffer[index] == 0xe5) && (searchBuffer[index + 1] == 0x45) && (searchBuffer[index + 2] == 0x45) && (searchBuffer[index + 3] == 0xfa))
					break;
			}

			if (index >= (searchLength - 14))
				return false;

			sampleStartOffsetsOffset = (((sbyte)searchBuffer[index + 4] << 8) | searchBuffer[index + 5]) + index + 4;

			if ((searchBuffer[index + 10] != 0x45) || (searchBuffer[index + 11] != 0xfa))
				return false;

			sampleDataOffset = (((sbyte)searchBuffer[index + 12] << 8) | searchBuffer[index + 13]) + index + 12;

			index += 14;

			if (index >= (searchLength - 20))
				return false;

			haveSeparateSampleInfo = false;

			if ((searchBuffer[index + 12] == 0xca) && (searchBuffer[index + 13] == 0xfc) && (searchBuffer[index + 16] == 0x45) && (searchBuffer[index + 17] == 0xfa))
			{
				haveSeparateSampleInfo = true;
				sampleInfoOffset = (((sbyte)searchBuffer[index + 18] << 8) | searchBuffer[index + 19]) + index + 18;

				index += 18;
			}

			for (; index < (searchLength - 12); index += 2)
			{
				if ((searchBuffer[index] == 0x6b) && (searchBuffer[index + 1] == 0x00) && (searchBuffer[index + 4] == 0x4a) && (searchBuffer[index + 5] == 0x31))
					break;
			}

			if (index >= (searchLength - 12))
				return false;

			index += 10;

			if ((searchBuffer[index] == 0x7a) && (searchBuffer[index + 1] == 0x00))
				portamentoVibratoType = PortamentoVibratoType.OnlyOne;
			else if ((searchBuffer[index] == 0x53) && (searchBuffer[index + 1] == 0x31))
				portamentoVibratoType = PortamentoVibratoType.BothTogether;
			else
				return false;

			for (; index < (searchLength - 2); index += 2)
			{
				if ((searchBuffer[index] == 0xda) && (searchBuffer[index + 1] == 0x45))
					break;
			}

			if (index >= (searchLength - 2))
				return false;

			for (; index < (searchLength - 10); index += 2)
			{
				if ((searchBuffer[index] == 0x9b) && (searchBuffer[index + 1] == 0x70))
					break;
			}

			if (index >= (searchLength - 10))
				return false;

			if ((searchBuffer[index + 4] == 0x53) && (searchBuffer[index + 5] == 0x31))
				vibratoVersion = 1;
			else if ((searchBuffer[index + 8] == 0x8a) && (searchBuffer[index + 9] == 0xf1))
				vibratoVersion = 2;
			else
				return false;

			index = FindEnabledEffects(searchBuffer, index + 10);
			if (index == -1)
				return false;

			haveEnvelope = false;

			if (index >= (searchLength - 8))
				return false;

			if ((searchBuffer[index + 4] == 0x6b) && (searchBuffer[index + 6] == 0x4a) && (searchBuffer[index + 7] == 0x31))
			{
				haveEnvelope = true;

				index += 8;

				for (; index < (searchLength - 10); index += 2)
				{
					if ((searchBuffer[index] == 0xe9) && (searchBuffer[index + 1] == 0x44) && ((searchBuffer[index + 2] == 0x31) || (searchBuffer[index + 2] == 0x11)) && (searchBuffer[index + 3] == 0x84) && (searchBuffer[index + 6] == 0x45) && (searchBuffer[index + 7] == 0xfa))
						break;
				}

				if (index >= (searchLength - 10))
					return false;

				envelopesOffset = (((sbyte)searchBuffer[index + 8] << 8) | searchBuffer[index + 9]) + index + 8;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Will check for which effects are available in the player
		/// </summary>
		/********************************************************************/
		private int FindEnabledEffects(byte[] searchBuffer, int startOffset)
		{
			int searchLength = searchBuffer.Length;
			int index = startOffset;

			for (; index < (searchLength - 8); index += 2)
			{
				if ((searchBuffer[index] == 0x08) && (searchBuffer[index + 1] == 0x31) && (searchBuffer[index + 6] == 0x67))
					break;
			}

			if (index >= (searchLength - 8))
				return -1;

			haveSetNote = false;
			haveSetFixedSample = false;
			haveSetArpeggio = false;
			haveSetSample = false;
			haveArpeggio = false;

			do
			{
				switch (searchBuffer[index + 3])
				{
					case 0x01:
					{
						haveSetNote = true;
						break;
					}

					case 0x02:
					{
						haveSetFixedSample = true;
						break;
					}

					case 0x03:
					{
						haveSetArpeggio = true;
						break;
					}

					case 0x04:
					{
						haveSetSample = true;
						break;
					}

					case 0x05:
					{
						haveArpeggio = true;
						break;
					}
				}

				index += (sbyte)searchBuffer[index + 7] + 8;
				if ((index < 0) || (index >= searchLength))
					return -1;
			}
			while ((searchBuffer[index] == 0x08) && (searchBuffer[index + 1] == 0x31) && (searchBuffer[index + 6] == 0x67));

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Load the speed variation if needed
		/// </summary>
		/********************************************************************/
		private bool LoadSpeedVariation(ModuleStream moduleStream)
		{
			if (speedVariationVersion == 2)
			{
				moduleStream.Seek(speedVariationSpeedIncrementOffset, SeekOrigin.Begin);

				speedVariationSpeedInit = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load the sub-song information for all sub-songs including
		/// position lists and tracks
		/// </summary>
		/********************************************************************/
		private bool LoadSubSongInfo(ModuleStream moduleStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			int numberOfSubSongs = (positionListsOffset - subSongListOffset) / 16;
			songInfoList = new SongInfo[numberOfSubSongs];

			for (int i = 0; i < numberOfSubSongs; i++)
			{
				moduleStream.Seek(subSongListOffset + i * 16, SeekOrigin.Begin);

				SongInfo songInfo = new SongInfo();

				ushort[] positionListOffsets = new ushort[4];
				moduleStream.ReadArray_B_UINT16s(positionListOffsets, 0, 4);

				if (moduleStream.ReadSigned(songInfo.SpeedVariation, 0, 8) != 8)
				{
					errorMessage = Resources.IDS_AVP_ERR_LOADING_SUBSONG;
					return false;
				}

				for (int j = 0; j < 4; j++)
				{
					moduleStream.Seek(positionListsOffset + positionListOffsets[j], SeekOrigin.Begin);

					byte[] positionList = LoadPositionList(moduleStream);
					if (positionList == null)
					{
						errorMessage = Resources.IDS_AVP_ERR_LOADING_POSITIONLIST;
						return false;
					}

					songInfo.PositionLists[j] = positionList;
				}

				songInfoList[i] = songInfo;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single position list
		/// </summary>
		/********************************************************************/
		private byte[] LoadPositionList(ModuleStream moduleStream)
		{
			List<byte> positionList = new List<byte>();

			for (;;)
			{
				byte dat = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return null;

				positionList.Add(dat);

				if ((dat >= 0xfd) || ((dat & 0x40) == 0))
					positionList.Add(moduleStream.Read_UINT8());

				if ((dat == 0xfe) || (dat == 0xff))
					break;
			}

			return positionList.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Load all the tracks
		/// </summary>
		/********************************************************************/
		private bool LoadTracks(ModuleStream moduleStream)
		{
			int numberOfTracks = (tracksOffset - trackOffsetsOffset) / 2;
			short[] trackOffsets = new short[numberOfTracks];
			tracks = new byte[numberOfTracks][];

			moduleStream.Seek(trackOffsetsOffset, SeekOrigin.Begin);
			moduleStream.ReadArray_B_INT16s(trackOffsets, 0, numberOfTracks);

			if (moduleStream.EndOfStream)
				return false;

			for (int i = 0; i < numberOfTracks; i++)
			{
				if (trackOffsets[i] < 0)
					continue;

				moduleStream.Seek(tracksOffset + trackOffsets[i], SeekOrigin.Begin);

				byte[] track = LoadSingleTrack(moduleStream);
				if (track == null)
					return false;

				tracks[i] = track;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single track
		/// </summary>
		/********************************************************************/
		private byte[] LoadSingleTrack(ModuleStream moduleStream)
		{
			List<byte> trackData = new List<byte>();

			for (;;)
			{
				byte dat = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return null;

				trackData.Add(dat);

				if (dat == 0xff)
					break;

				if (parseTrackVersion == 3)
				{
					while ((dat & 0x80) != 0)
					{
						trackData.Add(moduleStream.Read_UINT8());

						dat = moduleStream.Read_UINT8();
						trackData.Add(dat);
					}
				}
				else if ((parseTrackVersion == 4) || (parseTrackVersion == 5))
				{
					if (dat != 0x81)
					{
						while ((dat & 0x80) != 0)
						{
							trackData.Add(moduleStream.Read_UINT8());

							dat = moduleStream.Read_UINT8();
							trackData.Add(dat);
						}
					}
				}
				else
				{
					if ((dat & 0x80) != 0)
					{
						trackData.Add(moduleStream.Read_UINT8());

						if (parseTrackVersion == 2)
							trackData.Add(moduleStream.Read_UINT8());
					}
				}

				trackData.Add(moduleStream.Read_UINT8());

				if (moduleStream.EndOfStream)
					return null;
			}

			return trackData.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Load all the envelopes
		/// </summary>
		/********************************************************************/
		private bool LoadEnvelopes(ModuleStream moduleStream)
		{
			if (!haveEnvelope)
				return true;

			int numberOfEnvelopes = (instrumentsOffset- envelopesOffset) / 16;
			envelopes = new Envelope[numberOfEnvelopes];

			moduleStream.Seek(envelopesOffset, SeekOrigin.Begin);

			for (int i = 0; i < numberOfEnvelopes; i++)
			{
				Envelope envelope = new Envelope();

				for (int j = 0; j < 5; j++)
				{
					EnvelopePoint point = new EnvelopePoint
					{
						TicksToWait = moduleStream.Read_UINT8(),
						VolumeIncrementValue = moduleStream.Read_INT8(),
						TimesToRepeat = moduleStream.Read_UINT8()
					};

					if (moduleStream.EndOfStream)
						return false;

					envelope.Points[j] = point;
				}

				// Dragon breed goes outside the 5 envelope points and uses the
				// padding byte as wait counter. We will add an extra envelope point
				// with unlimited repeat
				EnvelopePoint extraPoint = new EnvelopePoint
				{
					TicksToWait = moduleStream.Read_UINT8(),
					VolumeIncrementValue = 0,
					TimesToRepeat = 0xff
				};

				envelope.Points[5] = extraPoint;

				envelopes[i] = envelope;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the instruments
		/// </summary>
		/********************************************************************/
		private bool LoadInstruments(ModuleStream moduleStream)
		{
			int numberOfInstruments = (trackOffsetsOffset - instrumentsOffset) / 16;
			instruments = new Instrument[numberOfInstruments];

			moduleStream.Seek(instrumentsOffset, SeekOrigin.Begin);

			for (int i = 0; i < numberOfInstruments; i++)
			{
				Instrument instrument = new Instrument();

				if (instrumentFormatVersion == 1)
					LoadInstrument1(moduleStream, instrument);
				else if (instrumentFormatVersion == 2)
					LoadInstrument2(moduleStream, instrument);
				else if (instrumentFormatVersion == 3)
					LoadInstrument3(moduleStream, instrument);
				else
					return false;

				if (moduleStream.EndOfStream)
					return false;

				instruments[i] = instrument;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load instrument in version 1 format
		/// </summary>
		/********************************************************************/
		private void LoadInstrument1(ModuleStream moduleStream, Instrument instrument)
		{
			instrument.SampleNumber = moduleStream.Read_UINT8();
			instrument.EnvelopeNumber = moduleStream.Read_UINT8();
			instrument.EnabledEffectFlags = moduleStream.Read_UINT8();

			moduleStream.Seek(1, SeekOrigin.Current);

			instrument.PortamentoAdd = moduleStream.Read_UINT8();

			moduleStream.Seek(2, SeekOrigin.Current);

			instrument.StopResetEffectDelay = moduleStream.Read_UINT8();
			instrument.SampleNumber2 = moduleStream.Read_UINT8();

			moduleStream.ReadSigned(instrument.ArpeggioTable, 0, 4);

			instrument.FixedOrTransposedNote = moduleStream.Read_UINT8();
			instrument.VibratoNumber = moduleStream.Read_UINT8();
			instrument.VibratoDelay = moduleStream.Read_UINT8();
		}



		/********************************************************************/
		/// <summary>
		/// Load instrument in version 2 format
		/// </summary>
		/********************************************************************/
		private void LoadInstrument2(ModuleStream moduleStream, Instrument instrument)
		{
			instrument.SampleNumber = moduleStream.Read_UINT8();
			instrument.Volume = moduleStream.Read_UINT8();
			instrument.EnabledEffectFlags = moduleStream.Read_UINT8();

			moduleStream.Seek(1, SeekOrigin.Current);

			instrument.PortamentoAdd = moduleStream.Read_UINT8();

			moduleStream.Seek(2, SeekOrigin.Current);

			instrument.StopResetEffectDelay = moduleStream.Read_UINT8();
			instrument.SampleNumber2 = moduleStream.Read_UINT8();

			moduleStream.ReadSigned(instrument.ArpeggioTable, 0, 4);

			instrument.FixedOrTransposedNote = moduleStream.Read_UINT8();
			instrument.VibratoNumber = moduleStream.Read_UINT8();
			instrument.VibratoDelay = moduleStream.Read_UINT8();
		}



		/********************************************************************/
		/// <summary>
		/// Load instrument in version 3 format
		/// </summary>
		/********************************************************************/
		private void LoadInstrument3(ModuleStream moduleStream, Instrument instrument)
		{
			instrument.SampleNumber = moduleStream.Read_UINT8();
			instrument.Volume = moduleStream.Read_UINT8();
			instrument.EnabledEffectFlags = moduleStream.Read_UINT8();
			instrument.Transpose = moduleStream.Read_INT8();
			instrument.FineTune = moduleStream.Read_B_INT16();
			instrument.SampleStartOffset = moduleStream.Read_B_UINT16();
			instrument.StopResetEffectDelay = moduleStream.Read_UINT8();

			moduleStream.ReadSigned(instrument.ArpeggioTable, 0, 4);

			instrument.FixedOrTransposedNote = moduleStream.Read_UINT8();
			instrument.VibratoNumber = moduleStream.Read_UINT8();
			instrument.VibratoDelay = moduleStream.Read_UINT8();
		}



		/********************************************************************/
		/// <summary>
		/// Load all the sample information
		/// </summary>
		/********************************************************************/
		private bool LoadSampleInfo(ModuleStream moduleStream)
		{
			samples = ArrayHelper.InitializeArray<Sample>(27);

			if (haveSeparateSampleInfo)
			{
				moduleStream.Seek(sampleInfoOffset, SeekOrigin.Begin);

				foreach (Sample sample in samples)
				{
					sample.Length = moduleStream.Read_B_UINT16();
					sample.LoopStart = moduleStream.Read_B_UINT16();
					sample.LoopLength = moduleStream.Read_B_UINT16();

					if (moduleStream.EndOfStream)
						return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the sample data
		/// </summary>
		/********************************************************************/
		private bool LoadSampleData(ModuleStream moduleStream)
		{
			uint[] startOffsets = new uint[samples.Length + 1];		// One extra for the end of the sample data

			moduleStream.Seek(sampleStartOffsetsOffset, SeekOrigin.Begin);
			moduleStream.ReadArray_B_UINT32s(startOffsets, 0, startOffsets.Length);

			if (moduleStream.EndOfStream)
				return false;

			for (int i = 0; i < samples.Length; i++)
			{
				Sample sample = samples[i];

				uint length = startOffsets[i + 1] - startOffsets[i];

				if (length == 0)
				{
					sample.Length = 0;
					sample.LoopStart = 0;
					sample.LoopLength = 1;
				}
				else
				{
					moduleStream.Seek(sampleDataOffset + startOffsets[i], SeekOrigin.Begin);

					if (!haveSeparateSampleInfo)
					{
						sample.Length = moduleStream.Read_B_UINT16();
						sample.LoopStart = moduleStream.Read_B_UINT16();
						sample.LoopLength = moduleStream.Read_B_UINT16();

						length -= 6;
					}

					sample.SampleData = moduleStream.ReadSampleData(i, (int)length, out int readBytes);
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
				SpeedVariationCounter = 0,
				SpeedIndex = 0,
				SpeedVariation2Counter = 255,
				SpeedVariation2Speed = speedVariationSpeedInit,

				MasterVolumeFadeCounter = 0,
				MasterVolumeFadeSpeed = -1,
				MasterVolume = 64,

				GlobalTranspose = 0
			};

			voices = ArrayHelper.InitializeArray<VoiceInfo>(4);

			InitializeVoiceInfo();
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			songInfoList = null;
			tracks = null;
			envelopes = null;
			instruments = null;
			samples = null;

			currentSongInfo = null;

			playingInfo = null;
			voices = null;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new empty voice info object
		/// </summary>
		/********************************************************************/
		private void InitializeVoiceInfo()
		{
			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = voices[i];

				voiceInfo.SpeedCounter = 1;
				voiceInfo.SpeedCounter2 = 0;
				voiceInfo.MaxSpeedCounter = 0;
				voiceInfo.TickCounter = 0;

				voiceInfo.PositionList = currentSongInfo.PositionLists[i];
				voiceInfo.PositionListPosition = -1;
				voiceInfo.PositionListLoopEnabled = false;
				voiceInfo.PositionListLoopCount = 0;
				voiceInfo.PositionListLoopStart = 0;

				voiceInfo.TrackNumber = 0;
				voiceInfo.TrackPosition = 0;
				voiceInfo.LoopTrackCounter = 0;

				voiceInfo.Note = 0;
				voiceInfo.Transpose = 0;
				voiceInfo.FineTune = 0;
				voiceInfo.NoteAndFlag = 0;
				voiceInfo.Period = 0;

				if ((parseTrackVersion == 3) || (parseTrackVersion == 4) || (parseTrackVersion == 5))
				{
					voiceInfo.InstrumentNumber = 0;
					voiceInfo.Instrument = instruments[0];
				}
				else
				{
					voiceInfo.InstrumentNumber = -1;
					voiceInfo.Instrument = null;
				}

				voiceInfo.SampleNumber = 0;
				voiceInfo.SampleData = null;
				voiceInfo.SampleLength = 0;
				voiceInfo.SampleLoopStart = 0;
				voiceInfo.SampleLoopLength = 0;

				voiceInfo.EnabledEffectsFlag = 0;
				voiceInfo.StopResetEffect = false;
				voiceInfo.StopResetEffectDelay = 0;
					
				voiceInfo.Envelope = null;
				voiceInfo.EnvelopePosition = 0;
				voiceInfo.EnvelopeWaitCounter = 0;
				voiceInfo.EnvelopeLoopCount = 0;

				voiceInfo.Mute = false;
				voiceInfo.Volume = 0;
				voiceInfo.TrackVolume = 64;

				voiceInfo.PortamentoValue = 0;

				voiceInfo.VibratoSpeed = 0;
				voiceInfo.VibratoDelay = 0;
				voiceInfo.VibratoDepth = 0;
				voiceInfo.VibratoDirection = false;
				voiceInfo.VibratoCountDirection = false;
				voiceInfo.VibratoCounterMax = 0;
				voiceInfo.VibratoCounter = 0;

				ParseNextPosition(voiceInfo, false);

				voices[i] = voiceInfo;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Stop and restart the module
		/// </summary>
		/********************************************************************/
		private void StopAndResetModule()
		{
			playingInfo.MasterVolume = 64;
			playingInfo.MasterVolumeFadeSpeed = -1;
			playingInfo.MasterVolumeFadeCounter = 0;
			playingInfo.SpeedVariationCounter = 0;

			for (int i = 0; i < 4; i++)
				VirtualChannels[i].Mute();

			InitializeVoiceInfo();

			// Tell NostalgicPlayer that the song has ended
			OnEndReachedOnAllChannels(0);
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve the period from the note and transpose
		/// </summary>
		/********************************************************************/
		private ushort GetPeriod(byte note, sbyte transpose)
		{
			int index = note + transpose;

			if (index < 0)
				index = 0;
			else if (index >= Tables.Periods.Length)
				index = Tables.Periods.Length - 1;

			return Tables.Periods[index];
		}



		/********************************************************************/
		/// <summary>
		/// Do the master volume fade if needed
		/// </summary>
		/********************************************************************/
		private void DoMasterVolumeFade()
		{
			if (playingInfo.MasterVolumeFadeSpeed >= 0)
			{
				playingInfo.MasterVolumeFadeCounter--;

				if (playingInfo.MasterVolumeFadeCounter < 0)
				{
					playingInfo.MasterVolumeFadeCounter = playingInfo.MasterVolumeFadeSpeed;
					playingInfo.MasterVolume--;

					if (playingInfo.MasterVolume == 0)
						StopAndResetModule();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Do the speed variation version 1
		/// </summary>
		/********************************************************************/
		private void DoSpeedVariation1()
		{
			if (speedVariationVersion == 1)
			{
				playingInfo.SpeedVariationCounter--;

				if (playingInfo.SpeedVariationCounter < 0)
				{
					playingInfo.SpeedIndex--;

					if (playingInfo.SpeedIndex < 0)
						playingInfo.SpeedIndex = 7;

					playingInfo.SpeedVariationCounter = currentSongInfo.SpeedVariation[playingInfo.SpeedIndex];
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Do the speed variation version 2
		/// </summary>
		/********************************************************************/
		private void DoSpeedVariation2()
		{
			if (speedVariationVersion == 2)
			{
				ushort counter = (ushort)(playingInfo.SpeedVariation2Counter + playingInfo.SpeedVariation2Speed);
				if (counter > 255)
					playingInfo.SpeedVariationCounter = 0;
				else
					playingInfo.SpeedVariationCounter = -1;

				playingInfo.SpeedVariation2Counter = (byte)counter;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Do the different counters
		/// </summary>
		/********************************************************************/
		private void DoCounters(VoiceInfo voiceInfo)
		{
			if (voiceInfo.StopResetEffectDelay == 0)
				voiceInfo.StopResetEffect = true;
			else
				voiceInfo.StopResetEffectDelay--;

			voiceInfo.TickCounter++;
		}



		/********************************************************************/
		/// <summary>
		/// Will go to the next position in the position list and parse it
		/// </summary>
		/********************************************************************/
		private void ParseNextPosition(VoiceInfo voiceInfo, bool updateInformation = true)
		{
			byte[] positionList = voiceInfo.PositionList;
			sbyte position = voiceInfo.PositionListPosition;

			bool oneMore;

			do
			{
				oneMore = false;

				position++;

				if (positionList[position] >= 0xfe)
				{
					// Song done
					voiceInfo.TrackNumber = positionList[position];
					position = (sbyte)(positionList[position + 1] - 1);
				}
				else
				{
					if (positionList[position] == 0xfd)
					{
						// Start master volume fade
						position++;

						playingInfo.MasterVolumeFadeSpeed = (sbyte)positionList[position];
						oneMore = true;
					}
					else
					{
						if ((positionList[position] & 0x40) != 0)
						{
							// Position list loop
							if (voiceInfo.PositionListLoopEnabled)
							{
								voiceInfo.PositionListLoopCount--;

								if (voiceInfo.PositionListLoopCount == 0)
								{
									voiceInfo.PositionListLoopEnabled = false;
									oneMore = true;
									continue;
								}

								position = voiceInfo.PositionListLoopStart;
							}
							else
							{
								voiceInfo.PositionListLoopEnabled = true;
								voiceInfo.PositionListLoopCount = (byte)(positionList[position] & 0x3f);
								voiceInfo.PositionListLoopStart = ++position;
							}
						}

						voiceInfo.LoopTrackCounter = positionList[position++];
						voiceInfo.TrackNumber = positionList[position];

						if (parseTrackVersion == 5)
							voiceInfo.MaxSpeedCounter = 255;
					}
				}
			}
			while (oneMore);

			voiceInfo.PositionListPosition = position;

			if (updateInformation)
			{
				ShowChannelPositions();
				ShowTracks();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will go to the next track position and parse it
		/// </summary>
		/********************************************************************/
		private void ParseNextTrackPosition(int channelNumber, VoiceInfo voiceInfo, IChannel channel)
		{
			if (voiceInfo.TrackNumber == 0xfe)
			{
				StopAndResetModule();
				ShowChannelPositions();
				ShowTracks();
			}

			if (voiceInfo.TrackNumber == 0xff)
			{
				voiceInfo.TrackPosition = 0;
				ParseNextPosition(voiceInfo);

				if (voiceInfo.TrackNumber == 0xff)
				{
					OnEndReached(channelNumber);
					return;
				}

				if (playingInfo.MasterVolumeFadeSpeed < 0)
					OnEndReached(channelNumber);
			}

			byte[] track = tracks[voiceInfo.TrackNumber];

			if (resetVolume && (voiceInfo.TrackPosition == 0))
				voiceInfo.TrackVolume = 64;

			sbyte setToInstrumentNumber = -1;

			byte trackByte = track[voiceInfo.TrackPosition];
			voiceInfo.TrackPosition++;

			if (parseTrackVersion == 1)
				voiceInfo.SpeedCounter = (byte)(trackByte & 0x3f);

			voiceInfo.PortamentoValue = 0;
			voiceInfo.StopResetEffect = false;
			voiceInfo.Mute = false;

			if ((parseTrackVersion == 4) || (parseTrackVersion == 5))
				voiceInfo.NoteAndFlag = 0;

			if ((parseTrackVersion == 1) && ((trackByte & 0x40) != 0))
			{
				voiceInfo.EnvelopeWaitCounter = 1;
				voiceInfo.EnvelopeLoopCount = 1;
				voiceInfo.EnvelopePosition = 3;
			}

			bool oneMore;

			do
			{
				oneMore = false;

				if ((trackByte & 0x80) != 0)
				{
					switch (parseTrackVersion)
					{
						case 1:
						{
							if ((track[voiceInfo.TrackPosition] & 0x80) != 0)
								voiceInfo.PortamentoValue = track[voiceInfo.TrackPosition];
							else
								setToInstrumentNumber = (sbyte)track[voiceInfo.TrackPosition];

							voiceInfo.TrackPosition++;
							break;
						}

						case 2:
						{
							setToInstrumentNumber = (sbyte)(trackByte & 0x7f);

							if ((track[voiceInfo.TrackPosition] & 0x80) != 0)
								voiceInfo.PortamentoValue = track[voiceInfo.TrackPosition];
							else
								voiceInfo.TrackVolume = track[voiceInfo.TrackPosition];

							voiceInfo.TrackPosition++;

							trackByte = track[voiceInfo.TrackPosition];
							voiceInfo.TrackPosition++;
							break;
						}

						case 3:
						{
							switch (trackByte)
							{
								case 0x80:
								{
									setToInstrumentNumber = (sbyte)track[voiceInfo.TrackPosition];
									break;
								}

								case 0x81:
								{
									voiceInfo.TrackVolume = track[voiceInfo.TrackPosition];
									break;
								}

								case 0x82:
								{
									voiceInfo.PortamentoValue = track[voiceInfo.TrackPosition];
									break;
								}

								case 0x8a:
								{
									voiceInfo.MaxSpeedCounter = track[voiceInfo.TrackPosition];
									break;
								}

								case 0x8e:
								{
									voiceInfo.TrackVolume += track[voiceInfo.TrackPosition];
									break;
								}
							}

							voiceInfo.TrackPosition++;

							trackByte = track[voiceInfo.TrackPosition];
							voiceInfo.TrackPosition++;

							oneMore = true;
							break;
						}

						case 4:
						{
							oneMore = true;

							switch (trackByte)
							{
								case 0x80:
								{
									setToInstrumentNumber = (sbyte)track[voiceInfo.TrackPosition];
									break;
								}

								case 0x81:
								{
									voiceInfo.Mute = true;
									voiceInfo.Note = 64;
									voiceInfo.Transpose = 0;

									oneMore = false;
									break;
								}

								case 0x82:
								{
									voiceInfo.PortamentoValue = track[voiceInfo.TrackPosition];
									break;
								}

								case 0x83:
								{
									playingInfo.SpeedVariation2Speed = track[voiceInfo.TrackPosition];
									break;
								}

								case 0x87:
								{
									voiceInfo.NoteAndFlag = 0xff;
									break;
								}

								case 0x8a:
								{
									voiceInfo.MaxSpeedCounter = track[voiceInfo.TrackPosition];
									break;
								}

								case 0x8c:
								{
									voiceInfo.TrackVolume = track[voiceInfo.TrackPosition];
									break;
								}

								case 0x8d:
								{
									voiceInfo.TrackVolume += track[voiceInfo.TrackPosition];
									break;
								}
							}

							if (oneMore)
							{
								voiceInfo.TrackPosition++;

								trackByte = track[voiceInfo.TrackPosition];
								voiceInfo.TrackPosition++;
							}
							break;
						}

						case 5:
						{
							oneMore = true;

							switch (trackByte)
							{
								case 0x80:
								{
									setToInstrumentNumber = (sbyte)track[voiceInfo.TrackPosition];
									break;
								}

								case 0x81:
								{
									voiceInfo.Mute = true;
									voiceInfo.Note = 64;
									voiceInfo.Transpose = 0;

									oneMore = false;
									break;
								}

								case 0x82:
								{
									voiceInfo.PortamentoValue = track[voiceInfo.TrackPosition];
									break;
								}

								case 0x83:
								{
									playingInfo.SpeedVariation2Speed = track[voiceInfo.TrackPosition];
									break;
								}

								case 0x84:
								{
									voiceInfo.NoteAndFlag = 0xff;
									break;
								}

								case 0x85:
								{
									voiceInfo.MaxSpeedCounter = track[voiceInfo.TrackPosition];
									break;
								}

								case 0x86:
								{
									playingInfo.GlobalTranspose = (sbyte)track[voiceInfo.TrackPosition];
									break;
								}

								case 0x87:
								{
									voiceInfo.TrackVolume = track[voiceInfo.TrackPosition];
									break;
								}

								case 0x8b:
								{
									voiceInfo.TrackVolume += track[voiceInfo.TrackPosition];
									break;
								}
							}

							if (oneMore)
							{
								voiceInfo.TrackPosition++;

								trackByte = track[voiceInfo.TrackPosition];
								voiceInfo.TrackPosition++;
							}
							break;
						}
					}
				}
				else if ((parseTrackVersion == 4) || (parseTrackVersion == 5))
					voiceInfo.Note = trackByte;
			}
			while (oneMore);

			if ((parseTrackVersion == 4) || (parseTrackVersion == 5))
			{
				voiceInfo.SpeedCounter = track[voiceInfo.TrackPosition];
				voiceInfo.SpeedCounter2 = 0;
				voiceInfo.TrackPosition++;
			}
			else
			{
				if (parseTrackVersion != 1)
					voiceInfo.SpeedCounter = trackByte;

				voiceInfo.NoteAndFlag = track[voiceInfo.TrackPosition];
				voiceInfo.Note = (byte)(voiceInfo.NoteAndFlag & 0x7f);

				voiceInfo.TrackPosition++;
			}

			if ((setToInstrumentNumber >= 0) && (setToInstrumentNumber != voiceInfo.InstrumentNumber))
			{
				voiceInfo.InstrumentNumber = setToInstrumentNumber;
				voiceInfo.NoteAndFlag = 0;
			}

			if ((voiceInfo.NoteAndFlag & 0x80) == 0)
			{
				Instrument instr = instruments[voiceInfo.InstrumentNumber];

				voiceInfo.Instrument = instr;
				voiceInfo.EnabledEffectsFlag = instr.EnabledEffectFlags;
				voiceInfo.StopResetEffectDelay = instr.StopResetEffectDelay;
				voiceInfo.VibratoDelay = instr.VibratoDelay;
				voiceInfo.Transpose = instr.Transpose;
				voiceInfo.FineTune = instr.FineTune;
				voiceInfo.SampleNumber = instr.SampleNumber;

				if (!haveEnvelope)
					voiceInfo.Volume = instr.Volume;

				Sample sampleInfo = samples[voiceInfo.SampleNumber];

				voiceInfo.SampleData = sampleInfo.SampleData;
				voiceInfo.SampleLength = sampleInfo.Length;
				voiceInfo.SampleLoopStart = sampleInfo.LoopStart;
				voiceInfo.SampleLoopLength = sampleInfo.LoopLength;

				channel.SetAmigaPeriod(126);
				channel.PlaySample(voiceInfo.SampleNumber, voiceInfo.SampleData, instr.SampleStartOffset, voiceInfo.SampleLength * 2U);

				if (voiceInfo.SampleLoopLength > 1)
					channel.SetLoop(voiceInfo.SampleLoopStart, voiceInfo.SampleLoopLength * 2U);
			}

			voiceInfo.Note = (byte)(voiceInfo.Note + playingInfo.GlobalTranspose);

			voiceInfo.Period = GetPeriod(voiceInfo.Note, voiceInfo.Transpose);

			if (track[voiceInfo.TrackPosition] == 0xff)
			{
				// End of track
				voiceInfo.TrackPosition = 0;

				voiceInfo.LoopTrackCounter--;
				if (voiceInfo.LoopTrackCounter == 0)
					ParseNextPosition(voiceInfo);
			}

			voiceInfo.TickCounter = 0;
			voiceInfo.VibratoDepth = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Will run effects flow 1
		/// </summary>
		/********************************************************************/
		private void RunEffects1(VoiceInfo voiceInfo)
		{
			switch (portamentoVibratoType)
			{
				case PortamentoVibratoType.OnlyOne:
				{
					if (voiceInfo.PortamentoValue != 0)
						DoPortamento(voiceInfo);
					else
						DoVibrato(voiceInfo);

					break;
				}

				case PortamentoVibratoType.BothTogether:
				{
					DoVibrato(voiceInfo);

					if (voiceInfo.PortamentoValue != 0)
						DoPortamento(voiceInfo);

					break;
				}
			}

			DoSetNote(voiceInfo);
			DoSetFixedSample(voiceInfo);

			if (haveSetArpeggio && ((voiceInfo.EnabledEffectsFlag & 0x08) != 0))
				SetArpeggio(voiceInfo, voiceInfo.TickCounter & 3);
			else
			{
				DoSetSample(voiceInfo);
				DoArpeggio(voiceInfo);
			}

			DoEnvelopes(voiceInfo);
			DoCounters(voiceInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Will run effects flow 2
		/// </summary>
		/********************************************************************/
		private void RunEffects2(VoiceInfo voiceInfo)
		{
			DoSetSample(voiceInfo);
			DoArpeggio(voiceInfo);
			DoEnvelopes(voiceInfo);
			DoCounters(voiceInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Will do the vibrato effect
		/// </summary>
		/********************************************************************/
		private void DoVibrato(VoiceInfo voiceInfo)
		{
			if (voiceInfo.VibratoDelay != 0)
				voiceInfo.VibratoDelay--;
			else
			{
				if (voiceInfo.StopResetEffectDelay == 0)
				{
					Instrument instr = voiceInfo.Instrument;

					if (instr.VibratoNumber != 0)
					{
						if (voiceInfo.VibratoDepth >= 0)
						{
							voiceInfo.VibratoCounterMax = Tables.VibratoCounters[instr.VibratoNumber];
							voiceInfo.VibratoDepth = vibratoVersion == 1 ? Tables.VibratoDepths1[instr.VibratoNumber] : Tables.VibratoDepths2[instr.VibratoNumber];
							voiceInfo.VibratoCountDirection = false;
							voiceInfo.VibratoCounter = 0;

							voiceInfo.Period = GetPeriod(voiceInfo.Note, voiceInfo.Transpose);
							voiceInfo.VibratoSpeed = (ushort)(voiceInfo.Period - GetPeriod((byte)(voiceInfo.Note + 1), voiceInfo.Transpose));

							if (vibratoVersion == 1)
							{
								for (;;)
								{
									voiceInfo.VibratoDepth--;

									if (voiceInfo.VibratoDepth < 0)
										break;

									voiceInfo.VibratoSpeed /= 2;

									if (voiceInfo.VibratoSpeed == 0)
									{
										voiceInfo.VibratoSpeed = 1;
										voiceInfo.VibratoDepth = -1;
										break;
									}
								}
							}
							else
							{
								int newSpeed = voiceInfo.VibratoSpeed / voiceInfo.VibratoDepth;
								if (newSpeed == 0)
									newSpeed = 1;

								voiceInfo.VibratoSpeed = (ushort)newSpeed;
								voiceInfo.VibratoDepth = -1;
							}
						}
						else
						{
							if (voiceInfo.VibratoDirection)
								voiceInfo.Period -= voiceInfo.VibratoSpeed;
							else
								voiceInfo.Period += voiceInfo.VibratoSpeed;

							if (voiceInfo.VibratoCountDirection)
							{
								voiceInfo.VibratoCounter--;

								if (voiceInfo.VibratoCounter == 0)
									voiceInfo.VibratoCountDirection = false;
							}
							else
							{
								voiceInfo.VibratoCounter++;

								if (voiceInfo.VibratoCounter == voiceInfo.VibratoCounterMax)
								{
									voiceInfo.VibratoCountDirection = true;
									voiceInfo.VibratoDirection = !voiceInfo.VibratoDirection;
								}
							}
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will do the portamento effect
		/// </summary>
		/********************************************************************/
		private void DoPortamento(VoiceInfo voiceInfo)
		{
			byte portamentoValue = voiceInfo.PortamentoValue;

			if (portamentoValue >= 0xc0)
				voiceInfo.Period += (ushort)(portamentoValue & 0x3f);
			else
				voiceInfo.Period -= (ushort)(portamentoValue & 0x3f);
		}



		/********************************************************************/
		/// <summary>
		/// Will do the set fixed note effect
		/// </summary>
		/********************************************************************/
		private void DoSetNote(VoiceInfo voiceInfo)
		{
			if (haveSetNote && ((voiceInfo.EnabledEffectsFlag & 0x02) != 0))
			{
				Instrument instr = voiceInfo.Instrument;
				byte note;

				if ((voiceInfo.TickCounter % 2) == 0)
					note = (byte)(voiceInfo.Note + voiceInfo.Transpose);
				else
				{
					note = instr.FixedOrTransposedNote;

					if ((note & 0x80) != 0)
					{
						note &= 0x7f;
						note += voiceInfo.Note;
					}
				}

				voiceInfo.Period = GetPeriod(note, 0);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will do the set fixed sample effect
		/// </summary>
		/********************************************************************/
		private void DoSetFixedSample(VoiceInfo voiceInfo)
		{
			if (haveSetFixedSample && ((voiceInfo.EnabledEffectsFlag & 0x04) != 0))
			{
				Instrument instr = voiceInfo.Instrument;
				byte sample;

				if ((voiceInfo.TickCounter % 2) == 0)
					sample = instr.SampleNumber;
				else
					sample = 2;

				voiceInfo.SampleNumber = sample;

				if ((voiceInfo.Period + instr.PortamentoAdd) < 0x8000)
					voiceInfo.Period += instr.PortamentoAdd;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will do the set sample effect
		/// </summary>
		/********************************************************************/
		private void DoSetSample(VoiceInfo voiceInfo)
		{
			if (haveSetSample && ((voiceInfo.EnabledEffectsFlag & 0x10) != 0))
			{
				Instrument instr = voiceInfo.Instrument;

				if (voiceInfo.StopResetEffectDelay != 0)
				    voiceInfo.SampleNumber = instr.SampleNumber2;
				else if (!voiceInfo.StopResetEffect)
					voiceInfo.SampleNumber = instr.SampleNumber;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will do the arpeggio effect
		/// </summary>
		/********************************************************************/
		private void DoArpeggio(VoiceInfo voiceInfo)
		{
			if (haveArpeggio && ((voiceInfo.EnabledEffectsFlag & 0x20) != 0))
			{
				if (voiceInfo.StopResetEffectDelay != 0)
					SetArpeggio(voiceInfo, voiceInfo.TickCounter);
				else if (!voiceInfo.StopResetEffect)
					voiceInfo.Period = GetPeriod(voiceInfo.Note, voiceInfo.Transpose);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will set the arpeggio note
		/// </summary>
		/********************************************************************/
		private void SetArpeggio(VoiceInfo voiceInfo, int index)
		{
			byte note;

			sbyte arpValue = voiceInfo.Instrument.ArpeggioTable[index];

			if (arpValue >= 0)
				note = (byte)arpValue;
			else
			{
				note = voiceInfo.Note;

				arpValue &= 0x7f;

				if (arpValue < 64)
					note += (byte)arpValue;
				else
					note -= (byte)(arpValue & 0x3f);

				note = (byte)(note + voiceInfo.Transpose);
			}

			voiceInfo.Period = GetPeriod(note, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Will do the envelopes
		/// </summary>
		/********************************************************************/
		private void DoEnvelopes(VoiceInfo voiceInfo)
		{
			if (haveEnvelope)
			{
				if (((voiceInfo.NoteAndFlag & 0x80) == 0) && (voiceInfo.TickCounter == 0))
				{
					Instrument instr = voiceInfo.Instrument;
					Envelope envelope = envelopes[instr.EnvelopeNumber];

					voiceInfo.Envelope = envelope;
					voiceInfo.EnvelopeLoopCount = envelope.Points[0].TimesToRepeat;
					voiceInfo.EnvelopePosition = 0;
					voiceInfo.EnvelopeWaitCounter = 1;
					voiceInfo.Volume = 0;
				}

				if (voiceInfo.EnvelopeWaitCounter >= 0)
				{
					voiceInfo.EnvelopeWaitCounter--;

					if (voiceInfo.EnvelopeWaitCounter == 0)
					{
						Envelope envelope = voiceInfo.Envelope;
						byte position = voiceInfo.EnvelopePosition;

						short volume = (short)voiceInfo.Volume;
						volume += envelope.Points[position].VolumeIncrementValue;

						if (volume > 64)
							volume = 64;
						else if (volume < 0)
							volume = 0;

						voiceInfo.Volume = (ushort)volume;

						voiceInfo.EnvelopeLoopCount--;

						if (voiceInfo.EnvelopeLoopCount == 0)
						{
							position++;

							byte ticks = envelope.Points[position].TicksToWait;

							if (ticks >= 0xc0)
							{
								position = (byte)((ticks & 0x3f) / 3);
								ticks = envelope.Points[position].TicksToWait;
							}

							voiceInfo.EnvelopeWaitCounter = (sbyte)ticks;

							if (position != 5)
								voiceInfo.EnvelopeLoopCount = envelope.Points[position].TimesToRepeat;

							voiceInfo.EnvelopePosition = position;
						}
						else
							voiceInfo.EnvelopeWaitCounter = (sbyte)envelope.Points[position].TicksToWait;
					}
				}
			}
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
		/// Will update the module information with all dynamic values
		/// </summary>
		/********************************************************************/
		private void UpdateModuleInformation()
		{
			ShowChannelPositions();
			ShowTracks();
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
				sb.Append(voices[i].PositionList.Length / 2);
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

			for (int i = 0; i < 4; i++)
			{
				int position = voices[i].PositionListPosition + 1;
				if ((position % 2) != 0)
					position++;

				position /= 2;
				position--;

				if (position < 0)
					position = (voices[i].PositionList.Length / 2) - 1;

				sb.Append(position);
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

			for (int i = 0; i < 4; i++)
			{
				sb.Append(voices[i].TrackNumber >= 254 ? "-" : voices[i].TrackNumber);
				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}
		#endregion
	}
}
