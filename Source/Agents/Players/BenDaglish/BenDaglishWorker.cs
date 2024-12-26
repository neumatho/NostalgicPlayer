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
using Polycode.NostalgicPlayer.Agent.Player.BenDaglish.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.BenDaglish
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class BenDaglishWorker : ModulePlayerWithSubSongDurationAgentBase
	{
		private int subSongListOffset;
		private int trackOffsetTableOffset;
		private int tracksOffset;
		private int sampleInfoOffsetTableOffset;

		private List<SongInfo> subSongs;
		private Dictionary<ushort, byte[]> positionLists;
		private byte[][] tracks;
		private Sample[] samples;

		private Features features;

		private GlobalPlayingInfo playingInfo;
		private VoiceInfo[] voices;
		private VoicePlaybackInfo[] voicePlaybackInfo;

		private const int InfoPositionLine = 3;
		private const int InfoTrackLine = 4;

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => [ "bd" ];



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 0x1600)
				return AgentResult.Unknown;

			// Read the first part of the file, so it is easier to search
			byte[] buffer = new byte[0x3000];

			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.ReadInto(buffer, 0, buffer.Length);

			return TestModule(buffer);
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
					description = Resources.IDS_BD_INFODESCLINE0;
					value = FormatPositionLengths();
					break;
				}

				// Used tracks
				case 1:
				{
					description = Resources.IDS_BD_INFODESCLINE1;
					value = tracks.Length.ToString();
					break;
				}

				// Used samples
				case 2:
				{
					description = Resources.IDS_BD_INFODESCLINE2;
					value = samples.Length.ToString();
					break;
				}

				// Playing positions
				case 3:
				{
					description = Resources.IDS_BD_INFODESCLINE3;
					value = FormatPositions();
					break;
				}

				// Playing tracks
				case 4:
				{
					description = Resources.IDS_BD_INFODESCLINE4;
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

		#region IModulePlayerAgent implementation
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

				if (!LoadSubSongInfo(moduleStream))
				{
					errorMessage = Resources.IDS_BD_ERR_LOADING_SUBSONG;
					return AgentResult.Error;
				}

				if (!LoadPositionLists(moduleStream))
				{
					errorMessage = Resources.IDS_BD_ERR_LOADING_POSITION_LISTS;
					return AgentResult.Error;
				}

				if (!LoadTracks(moduleStream))
				{
					errorMessage = Resources.IDS_BD_ERR_LOADING_TRACKS;
					return AgentResult.Error;
				}

				if (!LoadSampleInfo(moduleStream, out uint[] sampleDataOffsets))
				{
					errorMessage = Resources.IDS_BD_ERR_LOADING_SAMPLEINFO;
					return AgentResult.Error;
				}

				if (!LoadSampleData(moduleStream, sampleDataOffsets))
				{
					errorMessage = Resources.IDS_BD_ERR_LOADING_SAMPLES;
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



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			HandleEffects();

			if (features.EnableCounter)
			{
				playingInfo.Counter--;

				if (playingInfo.Counter == 0)
				{
					playingInfo.Counter = 6;
					return;
				}
			}

			if (!playingInfo.EnablePlaying)
			{
				OnEndReached();

				playingInfo.EnablePlaying = true;
				RestartSong();

				for (int i = 0; i < 4; i++)
					VirtualChannels[i].Mute();

				return;
			}

			playingInfo.EnablePlaying = voices[0].ChannelEnabled || voices[1].ChannelEnabled || voices[2].ChannelEnabled || voices[3].ChannelEnabled;

			if (playingInfo.EnablePlaying)
			{
				for (int i = 0; i < 4; i++)
				{
					VoiceInfo voiceInfo = voices[i];
					VoicePlaybackInfo playbackInfo = voicePlaybackInfo[i];
					IChannel channel = VirtualChannels[i];

					HandleVoice(voiceInfo, playbackInfo, channel);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public override SubSongInfo SubSongs => new SubSongInfo(subSongs.Count, 0);



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

					for (int j = 0; j < 8 * 12; j++)
					{
						uint period = (ushort)(((((Tables.FineTune[j] & 0xffff) * sample.FineTunePeriod) >> 16) + ((Tables.FineTune[j] >> 16) * sample.FineTunePeriod)) & 0xffff);
						frequencies[8 * 12 - j] = 3546895U / period;
					}

					SampleInfo sampleInfo = new SampleInfo
					{
						Name = string.Empty,
						Flags = SampleInfo.SampleFlag.None,
						Type = SampleInfo.SampleType.Sample,
						Volume = (ushort)(sample.Volume * 4),
						Panning = -1,
						Sample = sample.SampleData,
						Length = sample.Length * 2U,
						NoteFrequencies = frequencies
					};

					if (sample.LoopLength != 0)
					{
						sampleInfo.LoopStart = sample.LoopOffset;
						sampleInfo.LoopLength = sample.LoopLength * 2U;
						sampleInfo.Length += sampleInfo.LoopLength;
						sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;
					}

					yield return sampleInfo;
				}
			}
		}
		#endregion

		#region ModulePlayerWithSubSongDurationAgentBase implementation
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
			return new Snapshot(playingInfo, voices, voicePlaybackInfo);
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
			Snapshot clonedSnapshot = new Snapshot(currentSnapshot.PlayingInfo, currentSnapshot.Voices, currentSnapshot.PlaybackVoices);

			playingInfo = clonedSnapshot.PlayingInfo;
			voices = clonedSnapshot.Voices;
			voicePlaybackInfo = clonedSnapshot.PlaybackVoices;

			UpdateModuleInformation();

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Test the file to see if it's a Ben Daglish player and extract
		/// needed information
		/// </summary>
		/********************************************************************/
		private AgentResult TestModule(byte[] buffer)
		{
			// First check some places in the file for required
			// assembler code
			if ((buffer[0] != 0x60) || (buffer[1] != 0x00) || (buffer[4] != 0x60) || (buffer[5] != 0x00) || (buffer[10] != 0x60) || (buffer[11] != 00))
				return AgentResult.Unknown;

			// Find the init function
			int startOfInit = (((sbyte)buffer[2] << 8) | buffer[3]) + 2;
			if (startOfInit >= (buffer.Length - 14))
				return AgentResult.Unknown;

			if ((buffer[startOfInit] != 0x3f) || (buffer[startOfInit + 1] != 0x00) || (buffer[startOfInit + 2] != 0x61) || (buffer[startOfInit + 3] != 0x00) ||
			    (buffer[startOfInit + 6] != 0x3d) || (buffer[startOfInit + 7] != 0x7c) ||
				(buffer[startOfInit + 12] != 0x41) || (buffer[startOfInit + 13] != 0xfa))
			{
				return AgentResult.Unknown;
			}

			// Find the play function
			int startOfPlay = (((sbyte)buffer[6] << 8) | buffer[7]) + 4 + 2;
			if (startOfPlay >= buffer.Length)
				return AgentResult.Unknown;

			if (!ExtractInfoFromInitFunction(buffer, startOfInit))
				return AgentResult.Unknown;

			if (!ExtractInfoFromPlayFunction(buffer, startOfPlay))
				return AgentResult.Unknown;

			if (!FindFeatures(buffer, startOfPlay))
				return AgentResult.Unknown;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Try to find the init function in the player and extract needed
		/// information from it
		/// </summary>
		/********************************************************************/
		private bool ExtractInfoFromInitFunction(byte[] searchBuffer, int startOfInit)
		{
			int searchLength = searchBuffer.Length;
			int index;

			// Find sub-song information offset
			for (index = startOfInit; index < (searchLength - 6); index += 2)
			{
				if ((searchBuffer[index] == 0x41) && (searchBuffer[index + 1] == 0xfa) && (searchBuffer[index + 4] == 0x22) && (searchBuffer[index + 5] == 0x08))
					break;
			}

			if (index >= (searchLength - 6))
				return false;

			subSongListOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;
			index += 4;

			// Find sample information offset table offset
			for (; index < (searchLength - 6); index += 2)
			{
				if ((searchBuffer[index] == 0x41) && (searchBuffer[index + 1] == 0xfa) && (searchBuffer[index + 4] == 0x23) && (searchBuffer[index + 5] == 0x48))
					break;
			}

			if (index >= (searchLength - 6))
				return false;

			sampleInfoOffsetTableOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Try to find the play function in the player and extract needed
		/// information from it
		/// </summary>
		/********************************************************************/
		private bool ExtractInfoFromPlayFunction(byte[] searchBuffer, int startOfPlay)
		{
			int searchLength = searchBuffer.Length;
			int index;

			// Find track offset table offset
			for (index = startOfPlay; index < (searchLength - 6); index += 2)
			{
				if ((searchBuffer[index] == 0x47) && (searchBuffer[index + 1] == 0xfa) && (((searchBuffer[index + 4] == 0x48) && (searchBuffer[index + 5] == 0x80)) || ((searchBuffer[index + 4] == 0xd0) && (searchBuffer[index + 5] == 0x40))))
					break;
			}

			if (index >= (searchLength - 6))
				return false;

			trackOffsetTableOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;
			index += 4;

			// Find tracks offset
			for (; index < (searchLength - 6); index += 2)
			{
				if ((searchBuffer[index] == 0x47) && (searchBuffer[index + 1] == 0xfa) && (searchBuffer[index + 4] == 0xd6) && (searchBuffer[index + 5] == 0xc0))
					break;
			}

			if (index >= (searchLength - 6))
				return false;

			tracksOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Check the player code to see which features are enabled or
		/// disabled
		/// </summary>
		/********************************************************************/
		private bool FindFeatures(byte[] searchBuffer, int startOfPlay)
		{
			features = new Features();

			if (!FindFeaturesInPlayMethod(searchBuffer, startOfPlay))
				return false;

			if (!FindFeaturesInHandleEffectsMethod(searchBuffer, startOfPlay))
				return false;

			if (!FindFeaturesInParseTrackMethod(searchBuffer, startOfPlay))
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool FindFeaturesInPlayMethod(byte[] searchBuffer, int startOfPlay)
		{
			int searchLength = searchBuffer.Length;
			int index;

			// Check for counter feature
			features.EnableCounter = false;

			if (startOfPlay >= (searchLength - 16))
				return false;

			if ((searchBuffer[startOfPlay + 4] == 0x10) && (searchBuffer[startOfPlay + 5] == 0x3a) && (searchBuffer[startOfPlay + 8] == 0x67) && (searchBuffer[startOfPlay + 14] == 0x53) && (searchBuffer[startOfPlay + 15] == 0x50))
			{
				index = (((sbyte)searchBuffer[startOfPlay + 6] << 8) | searchBuffer[startOfPlay + 7]) + startOfPlay + 6;
				if (index >= searchLength)
					return false;

				features.EnableCounter = searchBuffer[index] != 0;
			}

			// Check effect calls
			features.EnablePortamento = false;
			features.EnableVolumeFade = false;

			for (index = startOfPlay; index < (searchLength - 2); index += 2)
			{
				if ((searchBuffer[index] == 0x53) && (searchBuffer[index + 1] == 0x2c))
					break;
			}

			if (index >= (searchLength - 2))
				return false;

			for (; index >= startOfPlay; index -= 2)
			{
				if ((searchBuffer[index] == 0x49) && (searchBuffer[index + 1] == 0xfa))
					break;

				if ((searchBuffer[index] == 0x61) && (searchBuffer[index + 1] == 0x00))
				{
					int methodIndex = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;

					if (methodIndex >= (searchLength - 14))
						return false;

					if ((searchBuffer[methodIndex] == 0x4a) && (searchBuffer[methodIndex + 1] == 0x2c) && (searchBuffer[methodIndex + 4] == 0x67) &&
					    (searchBuffer[methodIndex + 6] == 0x6a) && (searchBuffer[methodIndex + 8] == 0x30) && (searchBuffer[methodIndex + 9] == 0x29))
					{
						features.EnablePortamento = true;
					}
					else if ((searchBuffer[methodIndex] == 0x4a) && (searchBuffer[methodIndex + 1] == 0x2c) && (searchBuffer[methodIndex + 4] == 0x67) &&
					         (searchBuffer[methodIndex + 6] == 0x4a) && (searchBuffer[methodIndex + 7] == 0x2c) && (searchBuffer[methodIndex + 10] == 0x67))
					{
						features.EnableVolumeFade = true;
					}
					else
						return false;
				}
			}

			// Check for position effects
			features.MaxTrackValue = 0x80;

			for (index = startOfPlay; index < (searchLength - 6); index += 2)
			{
				if ((searchBuffer[index] == 0x10) && (searchBuffer[index + 1] == 0x1b))
					break;
			}

			if (index >= (searchLength - 6))
				return false;

			if (((searchBuffer[index + 2] == 0xb0) && (searchBuffer[index + 3] == 0x3c)) || ((searchBuffer[index + 2] == 0x0c) && (searchBuffer[index + 3] == 0x00)))
				features.MaxTrackValue = searchBuffer[index + 5];

			for (index += 4; index < (searchLength - 6); index += 2)
			{
				if ((((searchBuffer[index] == 0xb0) && (searchBuffer[index + 1] == 0x3c)) || ((searchBuffer[index] == 0x0c) && (searchBuffer[index + 1] == 0x00))) && (searchBuffer[index + 4] == 0x6c))
					break;
			}

			if (index >= (searchLength - 6))
				return false;

			int effect = (searchBuffer[index + 2] << 8) | searchBuffer[index + 3];
			features.EnableC0TrackLoop = effect == 0x00c0;
			features.EnableF0TrackLoop = effect == 0x00f0;

			index = searchBuffer[index + 5] + index + 6;

			if ((searchBuffer[index] == 0x02) && (searchBuffer[index + 1] == 0x40))
				features.SetSampleMappingVersion = 1;
			else if ((searchBuffer[index] == 0x04) && (searchBuffer[index + 1] == 0x00))
				features.SetSampleMappingVersion = 2;
			else
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool FindFeaturesInHandleEffectsMethod(byte[] searchBuffer, int startOfPlay)
		{
			int searchLength = searchBuffer.Length;
			int index;

			if ((searchBuffer[startOfPlay] != 0x61) || (searchBuffer[startOfPlay + 1] != 0x00))
				return false;

			int startOfHandleEffects = (((sbyte)searchBuffer[startOfPlay + 2] << 8) | searchBuffer[startOfPlay + 3]) + startOfPlay + 2;

			// Find call to sample handler callBack method
			for (index = startOfHandleEffects; index < (searchLength - 2); index += 2)
			{
				if ((searchBuffer[index] == 0x4e) && (searchBuffer[index + 1] == 0x90))
					break;
			}

			if (index >= (searchLength - 2))
				return false;

			int callBackIndex = index;

			// Search back after effect method calls
			features.EnableSampleEffects = false;
			features.EnableFinalVolumeSlide = false;

			for (; index >= startOfHandleEffects; index -= 2)
			{
				if ((searchBuffer[index] == 0x4e) && (searchBuffer[index + 1] == 0x75))
					break;

				if ((searchBuffer[index] == 0x61) && (searchBuffer[index + 1] == 0x00))
				{
					int methodIndex = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;

					if (methodIndex >= (searchLength - 14))
						return false;

					if ((searchBuffer[methodIndex] == 0x30) && (searchBuffer[methodIndex + 1] == 0x2b) && (searchBuffer[methodIndex + 4] == 0x67) &&
					    (((searchBuffer[methodIndex + 6] == 0xb0) && (searchBuffer[methodIndex + 7] == 0x7c)) || ((searchBuffer[methodIndex + 6] == 0x0c) && (searchBuffer[methodIndex + 7] == 0x40))) &&
					    (searchBuffer[methodIndex + 8] == 0xff) && (searchBuffer[methodIndex + 9] == 0xff))
					{
						features.EnableSampleEffects = true;
					}
					else if ((searchBuffer[methodIndex] == 0x30) && (searchBuffer[methodIndex + 1] == 0x2b) && (searchBuffer[methodIndex + 4] == 0x67) &&
					         (searchBuffer[methodIndex + 6] == 0x53) && (searchBuffer[methodIndex + 7] == 0x6b))
					{
						features.EnableFinalVolumeSlide = true;
					}
					else
						return false;
				}
			}

			if ((searchBuffer[callBackIndex + 6] != 0x6e) && (searchBuffer[callBackIndex + 6] != 0x66))
				return false;

			index = searchBuffer[callBackIndex + 7] + callBackIndex + 8;
			if (index >= (searchLength - 6))
				return false;

			// Check for setting DMA in sample handlers
			features.SetDmaInSampleHandlers = true;

			for (; index < searchLength; index++)
			{
				if ((searchBuffer[index] == 0x4e) && (searchBuffer[index + 1] == 0x75))
					break;
			}

			if (index >= searchLength)
				return false;

			if ((searchBuffer[index - 2] == 0x00) && (searchBuffer[index - 1] == 0x96))
				features.SetDmaInSampleHandlers = false;

			// Check for master volume fade feature
			if ((searchBuffer[startOfHandleEffects] == 0x61) && (searchBuffer[startOfHandleEffects + 1] == 0x00))
			{
				index = (((sbyte)searchBuffer[startOfHandleEffects + 2] << 8) | searchBuffer[startOfHandleEffects + 3]) + startOfHandleEffects + 2;

				if (index >= (searchLength - 24))
					return false;

				features.MasterVolumeFadeVersion = -1;

				if ((searchBuffer[index] == 0x30) && (searchBuffer[index + 1] == 0x3a) && (searchBuffer[index + 4] == 0x67) && (searchBuffer[index + 5] == 00) &&
				    (searchBuffer[index + 8] == 0x41) && (searchBuffer[index + 9] == 0xfa) && (searchBuffer[index + 18] == 0x30) && (searchBuffer[index + 19] == 0x80))
				{
					features.MasterVolumeFadeVersion = 1;
				}
				else if ((searchBuffer[index] == 0x30) && (searchBuffer[index + 1] == 0x39) && (searchBuffer[index + 6] == 0x67) && (searchBuffer[index + 7] == 00) &&
				    (searchBuffer[index + 10] == 0x41) && (searchBuffer[index + 11] == 0xf9) && (searchBuffer[index + 22] == 0x30) && (searchBuffer[index + 23] == 0x80))
				{
					features.MasterVolumeFadeVersion = 1;
				}
				else if ((searchBuffer[index] == 0x10) && (searchBuffer[index + 1] == 0x3a) && (searchBuffer[index + 4] == 0x67) && (searchBuffer[index + 5] == 00) &&
				         (searchBuffer[index + 8] == 0x41) && (searchBuffer[index + 9] == 0xfa) && (searchBuffer[index + 18] == 0x53) && (searchBuffer[index + 19] == 0x00))
				{
					features.MasterVolumeFadeVersion = 2;
				}
				else
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool FindFeaturesInParseTrackMethod(byte[] searchBuffer, int startOfPlay)
		{
			int searchLength = searchBuffer.Length;
			int index;

			for (index = startOfPlay; index < (searchLength - 4); index += 2)
			{
				if ((searchBuffer[index] == 0x60) && (searchBuffer[index + 1] == 0x00))
					break;
			}

			if (index >= (searchLength - 4))
				return false;

			index = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;

			if (index >= searchLength)
				return false;

			int startOfParseTrack = index;

			for (; index < (searchLength - 8); index += 2)
			{
				if ((searchBuffer[index] == 0x4a) && (searchBuffer[index + 1] == 0x2c) && (searchBuffer[index + 4] == 0x67))
					break;
			}

			if (index >= (searchLength - 8))
				return false;

			features.CheckForTicks = (searchBuffer[index + 6] == 0x4a) && (searchBuffer[index + 7] == 0x2c);

			for (index += 8; index < (searchLength - 6); index += 2)
			{
				if ((searchBuffer[index] == 0x72) && (searchBuffer[index + 1] == 0x00) && (searchBuffer[index + 2] == 0x12) && (searchBuffer[index + 3] == 0x1b))
					break;
			}

			if (index >= (searchLength - 6))
				return false;

			features.ExtraTickArg = searchBuffer[index + 4] == 0x66;

			for (index = startOfParseTrack; index < (searchLength - 4); index += 2)
			{
				if ((searchBuffer[index] == 0x61) && (searchBuffer[index + 1] == 0x00))
					break;
			}

			if (index >= (searchLength - 4))
				return false;

			index = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;

			if (index >= searchLength)
				return false;

			return FindFeaturesInParseTrackEffectMethod(searchBuffer, index);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool FindFeaturesInParseTrackEffectMethod(byte[] searchBuffer, int startOfMethod)
		{
			int searchLength = searchBuffer.Length;
			int index;

			if (((searchBuffer[startOfMethod + 2] != 0xb0) || (searchBuffer[startOfMethod + 3] != 0x3c)) && ((searchBuffer[startOfMethod + 2] != 0x0c) || (searchBuffer[startOfMethod + 3] != 0x00)))
				return false;

			features.MaxSampleMappingValue = searchBuffer[startOfMethod + 5];
			index = startOfMethod + 8;

			if (index >= (searchLength - 4))
				return false;

			if ((searchBuffer[index] != 0x02) || (searchBuffer[index + 1] != 0x40) || (searchBuffer[index + 2] != 0x00))
				return false;

			if (searchBuffer[index + 3] == 0x07)
				features.GetSampleMappingVersion = 1;
			else if (searchBuffer[index + 3] == 0xff)
				features.GetSampleMappingVersion = 2;
			else
				return true;

			for (index += 4; index < (searchLength - 6); index += 2)
			{
				if ((((searchBuffer[index] == 0xb0) && (searchBuffer[index + 1] == 0x3c)) || ((searchBuffer[index] == 0x0c) && (searchBuffer[index + 1] == 0x00))) && (searchBuffer[index + 4] == 0x6c))
					break;
			}

			if (index >= (searchLength - 6))
				return false;

			features.Uses9xTrackEffects = (searchBuffer[index + 3] & 0xf0) == 0x90;
			features.UsesCxTrackEffects = (searchBuffer[index + 3] & 0xf0) == 0xc0;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load the sub-song information for all sub-songs
		/// </summary>
		/********************************************************************/
		private bool LoadSubSongInfo(ModuleStream moduleStream)
		{
			subSongs = new List<SongInfo>();

			// Seek to the sub-song list
			moduleStream.Seek(subSongListOffset, SeekOrigin.Begin);
			int firstPositionList = int.MaxValue;

			do
			{
				SongInfo song = new SongInfo();

				moduleStream.ReadArray_B_UINT16s(song.PositionLists, 0, 4);

				if (moduleStream.EndOfStream)
					return false;

				firstPositionList = Math.Min(firstPositionList, song.PositionLists.Min());

				subSongs.Add(song);
			}
			while (moduleStream.Position < (subSongListOffset + firstPositionList));

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the position lists
		/// </summary>
		/********************************************************************/
		private bool LoadPositionLists(ModuleStream moduleStream)
		{
			positionLists = new Dictionary<ushort, byte[]>();

			foreach (SongInfo song in subSongs)
			{
				for (int i = 0; i < 4; i++)
				{
					ushort positionListOffset = song.PositionLists[i];
					if (positionLists.ContainsKey(positionListOffset))
						continue;

					// Seek to position list
					moduleStream.Seek(subSongListOffset + positionListOffset, SeekOrigin.Begin);

					// Load the position list
					byte[] positionList = LoadSinglePositionList(moduleStream);
					if (positionList == null)
						return false;

					positionLists[positionListOffset] = positionList;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single position list
		/// </summary>
		/********************************************************************/
		private byte[] LoadSinglePositionList(ModuleStream moduleStream)
		{
			List<byte> positionListBytes = new List<byte>();

			for (;;)
			{
				byte cmd = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return null;

				positionListBytes.Add(cmd);

				if (cmd == 0xff)	// End of position list?
					break;

				int argCount = FindPositionCommandArgumentCount(cmd);
				if (argCount == -1)
					return null;

				for (; argCount > 0; argCount--)
					positionListBytes.Add(moduleStream.Read_UINT8());
			}

			return positionListBytes.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Find number of arguments for the given position command
		/// </summary>
		/********************************************************************/
		private int FindPositionCommandArgumentCount(byte cmd)
		{
			if (cmd < features.MaxTrackValue)
				return 0;

			if (features.EnableC0TrackLoop)
			{
				if (cmd < 0xa0)
					return 0;

				if (cmd < 0xc8)
					return 1;
			}

			if (features.EnableF0TrackLoop)
			{
				if (cmd < 0xf0)
					return 0;

				if (cmd < 0xf8)
					return 1;
			}

			if ((cmd == 0xfd) && (features.MasterVolumeFadeVersion > 0))
				return 1;

			if (cmd == 0xfe)
				return 1;

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the tracks
		/// </summary>
		/********************************************************************/
		private bool LoadTracks(ModuleStream moduleStream)
		{
			// Find number of tracks
			int numberOfTracks = (subSongListOffset - trackOffsetTableOffset) / 2;

			tracks = new byte[numberOfTracks][];

			// Read the track offsets
			ushort[] trackOffsetTable = new ushort[numberOfTracks];

			moduleStream.Seek(trackOffsetTableOffset, SeekOrigin.Begin);
			moduleStream.ReadArray_B_UINT16s(trackOffsetTable, 0, numberOfTracks);

			if (moduleStream.EndOfStream)
				return false;

			for (int i = 0; i < numberOfTracks; i++)
			{
				moduleStream.Seek(tracksOffset + trackOffsetTable[i], SeekOrigin.Begin);

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
			List<byte> trackBytes = new List<byte>();

			for (;;)
			{
				byte cmd = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return null;

				trackBytes.Add(cmd);

				if (cmd == 0xff)	// End of track?
					break;

				byte nextByte = moduleStream.Read_UINT8();

				int argCount = FindTrackCommandArgumentCount(cmd, nextByte);
				if (argCount == -1)
					return null;

				if (argCount > 0)
				{
					trackBytes.Add(nextByte);

					for (argCount--; argCount > 0; argCount--)
						trackBytes.Add(moduleStream.Read_UINT8());
				}
				else
					moduleStream.Seek(-1, SeekOrigin.Current);
			}

			return trackBytes.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Find number of arguments for the given track command
		/// </summary>
		/********************************************************************/
		private int FindTrackCommandArgumentCount(byte cmd, byte nextByte)
		{
			if (cmd < 0x7f)
			{
				if (features.ExtraTickArg && (nextByte == 0))
					return 2;

				return 1;
			}

			if (cmd == 0x7f)
				return 1;

			if (cmd <= features.MaxSampleMappingValue)
				return 0;

			if ((features.UsesCxTrackEffects && (cmd < 0xc0)) || (features.Uses9xTrackEffects && (cmd < 0x9b)))
				return 0;

			switch (cmd)
			{
				case 0xc0 when features.UsesCxTrackEffects && features.EnablePortamento:
				case 0x9b when features.Uses9xTrackEffects && features.EnablePortamento:
					return 3;

				case 0xc1 when features.UsesCxTrackEffects && features.EnablePortamento:
				case 0x9c when features.Uses9xTrackEffects && features.EnablePortamento:
					return 0;

				case 0xc2 when features.UsesCxTrackEffects && features.EnableVolumeFade:
				case 0x9d when features.Uses9xTrackEffects && features.EnableVolumeFade:
					return 3;

				case 0xc3 when features.UsesCxTrackEffects && features.EnableVolumeFade:
				case 0x9e when features.Uses9xTrackEffects && features.EnableVolumeFade:
					return 0;

				case 0xc4 when features.UsesCxTrackEffects && features.EnablePortamento:
				case 0x9f when features.Uses9xTrackEffects && features.EnablePortamento:
					return 1;

				case 0xc5 when features.UsesCxTrackEffects && features.EnablePortamento:
				case 0xa0 when features.Uses9xTrackEffects && features.EnablePortamento:
					return 0;

				case 0xc6 when features.UsesCxTrackEffects && features.EnableVolumeFade:
				case 0xa1 when features.Uses9xTrackEffects && features.EnableVolumeFade:
					return features.EnableFinalVolumeSlide ? 3 : 1;

				case 0xc7 when features.UsesCxTrackEffects && features.EnableFinalVolumeSlide:
				case 0xa2 when features.Uses9xTrackEffects && features.EnableFinalVolumeSlide:
					return 0;
			}

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the sample information
		/// </summary>
		/********************************************************************/
		private bool LoadSampleInfo(ModuleStream moduleStream, out uint[] sampleDataOffsets)
		{
			// First read the sample information offset table
			List<uint> offsetTable = new List<uint>();
			uint firstSampleInfo = uint.MaxValue;

			moduleStream.Seek(sampleInfoOffsetTableOffset, SeekOrigin.Begin);

			do
			{
				uint offset = moduleStream.Read_B_UINT32();

				if (moduleStream.EndOfStream)
				{
					sampleDataOffsets = null;
					return false;
				}

				firstSampleInfo = Math.Min(firstSampleInfo, offset);
				offsetTable.Add(offset);
			}
			while (moduleStream.Position < (sampleInfoOffsetTableOffset + firstSampleInfo));

			// Now read the sample information
			samples = new Sample[offsetTable.Count];
			sampleDataOffsets = new uint[offsetTable.Count];

			for (short i = 0; i < samples.Length; i++)
			{
				moduleStream.Seek(sampleInfoOffsetTableOffset + offsetTable[i], SeekOrigin.Begin);

				Sample sample = new Sample();
				sample.SampleNumber = i;

				uint sampleDataOffset = moduleStream.Read_B_UINT32();

				sample.LoopOffset = moduleStream.Read_B_UINT32();
				if (sample.LoopOffset > 0)
					sample.LoopOffset -= sampleDataOffset;

				sample.Length = moduleStream.Read_B_UINT16();
				sample.LoopLength = moduleStream.Read_B_UINT16();

				sample.Volume = moduleStream.Read_B_UINT16();
				sample.VolumeFadeSpeed = moduleStream.Read_B_INT16();

				sample.PortamentoDuration = moduleStream.Read_B_INT16();
				sample.PortamentoAddValue = moduleStream.Read_B_INT16();

				sample.VibratoDepth = moduleStream.Read_B_UINT16();
				sample.VibratoAddValue = moduleStream.Read_B_UINT16();

				sample.NoteTranspose = moduleStream.Read_B_INT16();
				sample.FineTunePeriod = moduleStream.Read_B_UINT16();

				if (moduleStream.EndOfStream)
					return false;

				sampleDataOffsets[i] = sampleDataOffset;
				samples[i] = sample;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the samples
		/// </summary>
		/********************************************************************/
		private bool LoadSampleData(ModuleStream moduleStream, uint[] sampleDataOffsets)
		{
			for (int i = 0; i < samples.Length; i++)
			{
				Sample sample = samples[i];

				moduleStream.Seek(sampleInfoOffsetTableOffset + sampleDataOffsets[i], SeekOrigin.Begin);

				int sampleEnd1 = sample.Length * 2;
				int sampleEnd2 = (int)(sample.LoopOffset + (sample.LoopLength * 2));
				int length = Math.Max(sampleEnd1, sampleEnd2);

				sample.SampleData = moduleStream.ReadSampleData(i, length, out int readBytes);

				if (readBytes != length)
					return false;
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
			SongInfo song = subSongs[subSong];

			playingInfo = new GlobalPlayingInfo
			{
				EnablePlaying = true,

				MasterVolume = 64,
				MasterVolumeFadeSpeed = 0,
				MasterVolumeFadeSpeedCounter = 1,

				Counter = 6
			};

			voices = new VoiceInfo[4];
			voicePlaybackInfo = new VoicePlaybackInfo[4];

			for (int i = 0; i < 4; i++)
			{
				voices[i] = new VoiceInfo
				{
					ChannelEnabled = true,

					PositionList = positionLists[song.PositionLists[i]],
					CurrentPosition = 0,
					NextPosition = 0,

					PlayingTrack = 0,
					Track = null,
					NextTrackPosition = 0,

					SwitchToNextPosition = true,
					TrackLoopCounter = 1,
					TicksLeftForNextTrackCommand = 1,

					Transpose = 0,
					TransposedNote = 0,
					PreviousTransposedNote = 0,
					UseNewNote = true,

					Portamento1Enabled = 0,
					Portamento2Enabled = false,
					PortamentoStartDelay = 0,
					PortamentoDuration = 0,
					PortamentoDeltaNoteNumber = 0,

					PortamentoControlFlag = 0,
					PortamentoStartDelayCounter = 0,
					PortamentoDurationCounter = 0,
					PortamentoAddValue = 0,

					VolumeFadeEnabled = false,
					VolumeFadeInitSpeed = 0,
					VolumeFadeDuration = 0,
					VolumeFadeInitAddValue = 0,

					VolumeFadeRunning = false,
					VolumeFadeSpeed = 0,
					VolumeFadeSpeedCounter = 0,
					VolumeFadeDurationCounter = 0,
					VolumeFadeAddValue = 0,
					VolumeFadeValue = 0,

					ChannelVolume = 0xffff,
					ChannelVolumeSlideSpeed = 0,
					ChannelVolumeSlideAddValue = 0,

					SampleInfo = null,
					SampleInfo2 = null
				};

				voicePlaybackInfo[i] = new VoicePlaybackInfo
				{
					PlayingSample = null,
					SamplePlayTicksCounter = 0,

					NotePeriod = 0,
					FinalVolume = 0,

					FinalVolumeSlideSpeed = 0,
					FinalVolumeSlideSpeedCounter = 0,
					FinalVolumeSlideAddValue = 0,

					LoopDelayCounter = 0,

					PortamentoAddValue = 0,

					SamplePortamentoDuration = 0,
					SamplePortamentoAddValue = 0,

					SampleVibratoDepth = 0,
					SampleVibratoAddValue = 0,

					SamplePeriodAddValue = 0,

					HandleSampleCallback = null,

					DmaEnabled = false,
					SampleNumber = -1,
					SampleData = null,
					SampleLength = 0
				};
			}

			CreateSampleMapping();
		}



		/********************************************************************/
		/// <summary>
		/// Will initialize the sample mapping table
		/// </summary>
		/********************************************************************/
		private void CreateSampleMapping()
		{
			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = voices[i];

				for (byte j = 0; j < voiceInfo.SampleMapping.Length; j++)
					voiceInfo.SampleMapping[j] = j;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			subSongs = null;
			positionLists = null;
			tracks = null;
			samples = null;

			features = null;

			playingInfo = null;
			voices = null;
			voicePlaybackInfo = null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void HandleVoice(VoiceInfo voiceInfo, VoicePlaybackInfo playbackInfo, IChannel channel)
		{
			if (!voiceInfo.ChannelEnabled)
				return;

			if (features.EnablePortamento)
				DoPortamento(voiceInfo, playbackInfo);

			if (features.EnableVolumeFade)
				DoVolumeFade(voiceInfo, playbackInfo, channel);

			for (;;)
			{
				if (voiceInfo.SwitchToNextPosition)
				{
					voiceInfo.TrackLoopCounter--;

					if (voiceInfo.TrackLoopCounter == 0)
					{
						// Go to next position
						voiceInfo.TrackLoopCounter = 1;

						if (TakeNextPosition(voiceInfo, playbackInfo))
							return;
					}
					else
					{
						// Loop track
						voiceInfo.NextTrackPosition = 0;
						voiceInfo.SwitchToNextPosition = false;
					}
				}

				if (!ParseTrack(voiceInfo, playbackInfo, channel))
					break;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse next commands in the position list and setup the track
		/// </summary>
		/********************************************************************/
		private bool TakeNextPosition(VoiceInfo voiceInfo, VoicePlaybackInfo playbackInfo)
		{
			voiceInfo.CurrentPosition = voiceInfo.NextPosition;

			int position = voiceInfo.NextPosition;
			byte cmd;

			for (;;)
			{
				cmd = voiceInfo.PositionList[position++];
				if (cmd < features.MaxTrackValue)
					break;

				switch (cmd)
				{
					case 0xfe:
					{
						voiceInfo.Transpose = (sbyte)voiceInfo.PositionList[position++];
						break;
					}

					case 0xff:
					{
						if ((playbackInfo.LoopDelayCounter == 0) || (playbackInfo.LoopDelayCounter == 0x8000))
							voiceInfo.ChannelEnabled = false;

						return true;
					}

					case 0xfd when (features.MasterVolumeFadeVersion > 0):
					{
						playingInfo.MasterVolumeFadeSpeed = (sbyte)voiceInfo.PositionList[position++];
						break;
					}

					case < 0xf0 when features.EnableF0TrackLoop:
					{
						voiceInfo.TrackLoopCounter = (byte)(cmd - 0xc8);
						break;
					}

					case < 0xc0 when features.EnableC0TrackLoop:
					{
						voiceInfo.TrackLoopCounter = (byte)(cmd & 0x1f);
						break;
					}

					default:
					{
						int index = features.SetSampleMappingVersion == 1 ? cmd & 0x07 : cmd - 0xf0;
						voiceInfo.SampleMapping[index] = (byte)(voiceInfo.PositionList[position++] / 4);
						break;
					}
				}
			}

			voiceInfo.SwitchToNextPosition = false;
			voiceInfo.NextPosition = position;

			voiceInfo.PlayingTrack = cmd;
			voiceInfo.Track = tracks[cmd];
			voiceInfo.NextTrackPosition = 0;

			ShowChannelPositions();
			ShowTracks();

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Parse next commands in the track
		/// </summary>
		/********************************************************************/
		private bool ParseTrack(VoiceInfo voiceInfo, VoicePlaybackInfo playbackInfo, IChannel channel)
		{
			if (voiceInfo.Track == null)
			{
				// Position list have not set any track, so disable the channel
				voiceInfo.ChannelEnabled = false;
				return false;
			}

			int position = voiceInfo.NextTrackPosition;

			voiceInfo.TicksLeftForNextTrackCommand--;

			if (voiceInfo.TicksLeftForNextTrackCommand != 0)
			{
				if (voiceInfo.Track[position] >= 0x80)
					ParseTrackEffect(voiceInfo, ref position);

				voiceInfo.NextTrackPosition = position;
				return false;
			}

			for (;;)
			{
				if (voiceInfo.Track[position] < 0x80)
					break;

				ParseTrackEffect(voiceInfo, ref position);

				if (voiceInfo.SwitchToNextPosition)
				{
					if (features.CheckForTicks && (voiceInfo.TicksLeftForNextTrackCommand == 0))
						voiceInfo.TicksLeftForNextTrackCommand = 1;

					return true;
				}
			}

			byte note = voiceInfo.Track[position++];

			if (note == 0x7f)
			{
				voiceInfo.TicksLeftForNextTrackCommand = voiceInfo.Track[position++];
				voiceInfo.NextTrackPosition = position;

				return false;
			}

			playbackInfo.PortamentoAddValue = 0;

			note = (byte)(note + voiceInfo.Transpose);
			voiceInfo.TransposedNote = note;

			if (features.EnablePortamento)
			{
				voiceInfo.PortamentoControlFlag = voiceInfo.Portamento1Enabled;
				if (voiceInfo.PortamentoControlFlag != 0)
				{
					voiceInfo.PortamentoStartDelayCounter = voiceInfo.PortamentoStartDelay;
					voiceInfo.PortamentoDurationCounter = voiceInfo.PortamentoDuration;
					voiceInfo.PortamentoAddValue = Tables.FineTune[Tables.FineTuneStartIndex - voiceInfo.PortamentoDeltaNoteNumber];
				}
			}

			if (features.EnableVolumeFade)
			{
				voiceInfo.VolumeFadeRunning = voiceInfo.VolumeFadeEnabled;
				if (voiceInfo.VolumeFadeRunning)
				{
					voiceInfo.VolumeFadeSpeed = voiceInfo.VolumeFadeInitSpeed;
					voiceInfo.VolumeFadeSpeedCounter = voiceInfo.VolumeFadeInitSpeed;
					voiceInfo.VolumeFadeDurationCounter = voiceInfo.VolumeFadeDuration;
					voiceInfo.VolumeFadeAddValue = voiceInfo.VolumeFadeInitAddValue;
					voiceInfo.VolumeFadeValue = 0;
				}
			}

			byte ticks = voiceInfo.Track[position++];
			if (features.ExtraTickArg && (ticks == 0))
			{
				voiceInfo.TicksLeftForNextTrackCommand = voiceInfo.Track[position++];
				ticks = 0xff;
			}
			else
				voiceInfo.TicksLeftForNextTrackCommand = ticks;

			if (features.EnablePortamento && voiceInfo.Portamento2Enabled)
			{
				voiceInfo.PortamentoControlFlag = 0xff;
				voiceInfo.PortamentoStartDelayCounter = 0;
				voiceInfo.PortamentoDurationCounter = voiceInfo.PortamentoDuration;

				byte note1 = note;
				if (!voiceInfo.UseNewNote)
					note = voiceInfo.PreviousTransposedNote;

				voiceInfo.TransposedNote = note;
				voiceInfo.PortamentoAddValue = Tables.FineTune[Tables.FineTuneStartIndex - (note1 - note)];
			}

			voiceInfo.PreviousTransposedNote = voiceInfo.TransposedNote;
			voiceInfo.NextTrackPosition = position;
			voiceInfo.UseNewNote = false;

			Sample sample = voiceInfo.SampleInfo;
			voiceInfo.SampleInfo2 = sample;

			short volume = features.EnableVolumeFade ? (short)((sample.Volume * voiceInfo.ChannelVolume) / 16384) : (short)sample.Volume;
			SetupSample(playbackInfo, channel, sample, note, ticks, volume, voiceInfo.ChannelVolumeSlideSpeed, voiceInfo.ChannelVolumeSlideAddValue);

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Parse track effect
		/// </summary>
		/********************************************************************/
		private void ParseTrackEffect(VoiceInfo voiceInfo, ref int position)
		{
			byte cmd = voiceInfo.Track[position++];

			if (cmd <= features.MaxSampleMappingValue)
			{
				int index = features.GetSampleMappingVersion == 1 ? cmd & 0x07 : cmd - 0x80;
				voiceInfo.SampleInfo = samples[voiceInfo.SampleMapping[index]];
			}
			else if (cmd == 0xff)
			{
				voiceInfo.SwitchToNextPosition = true;
				position--;
			}
			else if ((features.UsesCxTrackEffects && (cmd < 0xc0)) || (features.Uses9xTrackEffects && (cmd < 0x9b)))
			{
				// Set control flag, which is not used
			}
			else
			{
				switch (cmd)
				{
					case 0xc0 when features.UsesCxTrackEffects && features.EnablePortamento:
					case 0x9b when features.Uses9xTrackEffects && features.EnablePortamento:
					{
						voiceInfo.Portamento1Enabled = 255;
						voiceInfo.Portamento2Enabled = false;
						voiceInfo.PortamentoStartDelay = voiceInfo.Track[position++];
						voiceInfo.PortamentoDuration = voiceInfo.Track[position++];
						voiceInfo.PortamentoDeltaNoteNumber = (sbyte)voiceInfo.Track[position++];
						break;
					}

					case 0xc1 when features.UsesCxTrackEffects && features.EnablePortamento:
					case 0x9c when features.Uses9xTrackEffects && features.EnablePortamento:
					{
						voiceInfo.Portamento1Enabled = 0;
						break;
					}

					case 0xc2 when features.UsesCxTrackEffects && features.EnableVolumeFade:
					case 0x9d when features.Uses9xTrackEffects && features.EnableVolumeFade:
					{
						voiceInfo.VolumeFadeEnabled = true;
						voiceInfo.VolumeFadeInitSpeed = voiceInfo.Track[position++];
						voiceInfo.VolumeFadeDuration = voiceInfo.Track[position++];
						voiceInfo.VolumeFadeInitAddValue = (sbyte)voiceInfo.Track[position++];
						break;
					}

					case 0xc3 when features.UsesCxTrackEffects && features.EnableVolumeFade:
					case 0x9e when features.Uses9xTrackEffects && features.EnableVolumeFade:
					{
						voiceInfo.VolumeFadeEnabled = false;
						break;
					}

					case 0xc4 when features.UsesCxTrackEffects && features.EnablePortamento:
					case 0x9f when features.Uses9xTrackEffects && features.EnablePortamento:
					{
						voiceInfo.Portamento2Enabled = true;
						voiceInfo.Portamento1Enabled = 0;
						voiceInfo.PortamentoDuration = voiceInfo.Track[position++];
						break;
					}

					case 0xc5 when features.UsesCxTrackEffects && features.EnablePortamento:
					case 0xa0 when features.Uses9xTrackEffects && features.EnablePortamento:
					{
						voiceInfo.Portamento2Enabled = false;
						break;
					}

					case 0xc6 when features.UsesCxTrackEffects && features.EnableVolumeFade:
					case 0xa1 when features.Uses9xTrackEffects && features.EnableVolumeFade:
					{
						voiceInfo.ChannelVolume = (ushort)((voiceInfo.Track[position++] << 8) | 0xff);

						if (features.EnableFinalVolumeSlide)
						{
							voiceInfo.ChannelVolumeSlideSpeed = voiceInfo.Track[position++];
							voiceInfo.ChannelVolumeSlideAddValue = (sbyte)voiceInfo.Track[position++];
						}
						break;
					}

					case 0xc7 when features.UsesCxTrackEffects && features.EnableFinalVolumeSlide:
					case 0xa2 when features.Uses9xTrackEffects && features.EnableFinalVolumeSlide:
					{
						voiceInfo.ChannelVolumeSlideSpeed = 0;
						voiceInfo.ChannelVolume = 0xffff;
						break;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will setup a channel to play a sample
		/// </summary>
		/********************************************************************/
		private void SetupSample(VoicePlaybackInfo playbackInfo, IChannel channel, Sample sample, byte transposedNote, byte playTicks, short volume, ushort volumeSlideSpeed, short volumeSlideAddValue)
		{
			channel.Mute();

			playbackInfo.DmaEnabled = false;
			playbackInfo.SampleNumber = sample.SampleNumber;
			playbackInfo.SampleData = sample.SampleData;
			playbackInfo.SampleLength = sample.Length;

			playbackInfo.PlayingSample = sample;
			playbackInfo.SamplePlayTicksCounter = playTicks;

			int periodIndex = -(transposedNote & 0x7f) + sample.NoteTranspose + Tables.FineTuneStartIndex;
			playbackInfo.NotePeriod = periodIndex >= 0 ? (ushort)(((((Tables.FineTune[periodIndex] & 0xffff) * sample.FineTunePeriod) >> 16) + ((Tables.FineTune[periodIndex] >> 16) * sample.FineTunePeriod)) & 0xffff) : (ushort)0;

			playbackInfo.SamplePortamentoDuration = sample.PortamentoDuration;

			if (playbackInfo.SamplePortamentoDuration >= 0)
			{
				playbackInfo.SampleVibratoDepth = (ushort)(sample.VibratoDepth / 2);
				if ((sample.VibratoDepth & 1) != 0)
					playbackInfo.SampleVibratoDepth++;

				playbackInfo.SamplePortamentoAddValue = (short)((sample.PortamentoAddValue * playbackInfo.NotePeriod) / 32768);
				playbackInfo.SampleVibratoAddValue = (short)((sample.VibratoAddValue * playbackInfo.NotePeriod) / 32768);
			}

			playbackInfo.SamplePeriodAddValue = 0;

			playbackInfo.HandleSampleCallback = sample.VolumeFadeSpeed == 0 ? HandleSamplePlayOnce : HandleSampleLoop;

			playbackInfo.FinalVolume = volume;

			if (volume > playingInfo.MasterVolume)
				volume = playingInfo.MasterVolume;

			channel.SetAmigaVolume((ushort)volume);

			if (features.EnableFinalVolumeSlide)
			{
				playbackInfo.FinalVolumeSlideSpeed = volumeSlideSpeed;
				playbackInfo.FinalVolumeSlideSpeedCounter = volumeSlideSpeed;
				playbackInfo.FinalVolumeSlideAddValue = volumeSlideAddValue;
			}

			playbackInfo.LoopDelayCounter = 2;
		}



		/********************************************************************/
		/// <summary>
		/// Handle realtime effects for all channels
		/// </summary>
		/********************************************************************/
		private void HandleEffects()
		{
			if ((features.MasterVolumeFadeVersion > 0))
				DoMasterVolumeFade();

			for (int i = 0; i < 4; i++)
			{
				VoicePlaybackInfo playbackInfo = voicePlaybackInfo[i];
				IChannel channel = VirtualChannels[i];

				HandleVoiceEffects(playbackInfo, channel);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle realtime effects for a single channel
		/// </summary>
		/********************************************************************/
		private void HandleVoiceEffects(VoicePlaybackInfo playbackInfo, IChannel channel)
		{
			if (playbackInfo.LoopDelayCounter != 0)
			{
				Sample sample = playbackInfo.PlayingSample;

				if (playbackInfo.SamplePlayTicksCounter > 0)
					playbackInfo.SamplePlayTicksCounter--;

				if (features.EnableSampleEffects)
					DoSampleEffects(playbackInfo, sample);

				if (features.EnableFinalVolumeSlide)
					DoFinalVolumeSlide(playbackInfo);

				playbackInfo.HandleSampleCallback(playbackInfo, channel, sample);

				short volume = playbackInfo.FinalVolume;
				if (volume > 0)
				{
					if (volume > playingInfo.MasterVolume)
						volume = playingInfo.MasterVolume;

					if (volume < 0)
						volume = 0;

					channel.SetAmigaVolume((ushort)volume);

					ushort period = (ushort)(playbackInfo.NotePeriod + playbackInfo.SamplePeriodAddValue + playbackInfo.PortamentoAddValue);
					channel.SetAmigaPeriod(period);

					if (!features.SetDmaInSampleHandlers)
						EnableDma(playbackInfo, channel);
				}
				else
				{
					channel.Mute();

					playbackInfo.LoopDelayCounter = 0;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will simulate Amiga Hardware when DMA is enabled
		/// </summary>
		/********************************************************************/
		private void EnableDma(VoicePlaybackInfo playbackInfo, IChannel channel)
		{
			if (!playbackInfo.DmaEnabled)
			{
				uint sampleLength = playbackInfo.SampleLength * 2U;

				channel.PlaySample(playbackInfo.SampleNumber, playbackInfo.SampleData, 0, sampleLength);
				channel.SetLoop(0, sampleLength);       // The Amiga hardware always loops the sample

				playbackInfo.DmaEnabled = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will simulate Amiga Hardware to loop a sample and do volume fade
		/// </summary>
		/********************************************************************/
		private void HandleSampleLoop(VoicePlaybackInfo playbackInfo, IChannel channel, Sample sample)
		{
			if (playbackInfo.LoopDelayCounter < 0x8000)
			{
				if (features.SetDmaInSampleHandlers)
					EnableDma(playbackInfo, channel);

				playbackInfo.LoopDelayCounter--;

				if (playbackInfo.LoopDelayCounter == 0)
				{
					playbackInfo.LoopDelayCounter = (ushort)~playbackInfo.LoopDelayCounter;

					uint loopLength = sample.LoopLength * 2U;
					if (loopLength > 0)
					{
						channel.SetSample(sample.LoopOffset, loopLength);
						channel.SetLoop(sample.LoopOffset, loopLength);
					}
					else
						channel.Mute();
				}
			}
			else
			{
				if (playbackInfo.SamplePlayTicksCounter <= 1)
					playbackInfo.HandleSampleCallback = HandleSampleVolumeFade;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will do a volume fade on a sample
		/// </summary>
		/********************************************************************/
		private void HandleSampleVolumeFade(VoicePlaybackInfo playbackInfo, IChannel channel, Sample sample)
		{
			playbackInfo.LoopDelayCounter = 0x8000;
			playbackInfo.FinalVolume = (short)(playbackInfo.FinalVolume + sample.VolumeFadeSpeed);
		}



		/********************************************************************/
		/// <summary>
		/// Will simulate Amiga Hardware to only play the sample once
		/// </summary>
		/********************************************************************/
		private void HandleSamplePlayOnce(VoicePlaybackInfo playbackInfo, IChannel channel, Sample sample)
		{
			if (playbackInfo.LoopDelayCounter < 0x8000)
			{
				playbackInfo.LoopDelayCounter--;

				if (playbackInfo.LoopDelayCounter == 0)
				{
					playbackInfo.LoopDelayCounter = (ushort)~playbackInfo.LoopDelayCounter;

					// This will simulate stopping the sample when it has played its buffer.
					// The original player has an Audio IRQ which will stop the DMA when done
					channel.SetSample(Tables.EmptySample, 0, (uint)Tables.EmptySample.Length);
				}
				else
				{
					if (features.SetDmaInSampleHandlers)
						EnableDma(playbackInfo, channel);
				}
			}
			else
			{
				if (playbackInfo.SamplePlayTicksCounter == 1)
					playbackInfo.LoopDelayCounter = 0x8000;

				if (!channel.IsActive)
					playbackInfo.FinalVolume = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle realtime effects set from the playing sample itself
		/// </summary>
		/********************************************************************/
		private void DoSampleEffects(VoicePlaybackInfo playbackInfo, Sample sample)
		{
			if (playbackInfo.SamplePortamentoDuration != 0)
			{
				if (playbackInfo.SamplePortamentoDuration == -1)
					return;

				playbackInfo.SamplePeriodAddValue += playbackInfo.SamplePortamentoAddValue;

				playbackInfo.SamplePortamentoDuration--;

				if (playbackInfo.SamplePortamentoDuration != 0)
					return;
			}

			playbackInfo.SamplePeriodAddValue += playbackInfo.SampleVibratoAddValue;

			playbackInfo.SampleVibratoDepth--;

			if (playbackInfo.SampleVibratoDepth == 0)
			{
				if (sample.VibratoDepth != 0)
				{
					playbackInfo.SampleVibratoDepth = sample.VibratoDepth;
					playbackInfo.SampleVibratoAddValue = (short)-playbackInfo.SampleVibratoAddValue;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle final volume slide
		/// </summary>
		/********************************************************************/
		private void DoFinalVolumeSlide(VoicePlaybackInfo playbackInfo)
		{
			if (playbackInfo.FinalVolumeSlideSpeed != 0)
			{
				playbackInfo.FinalVolumeSlideSpeedCounter--;

				if (playbackInfo.FinalVolumeSlideSpeedCounter == 0)
				{
					playbackInfo.FinalVolumeSlideSpeedCounter = playbackInfo.FinalVolumeSlideSpeed;
					playbackInfo.FinalVolume = (short)(playbackInfo.FinalVolume + playbackInfo.FinalVolumeSlideAddValue);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle portamento
		/// </summary>
		/********************************************************************/
		private void DoPortamento(VoiceInfo voiceInfo, VoicePlaybackInfo playbackInfo)
		{
			if (voiceInfo.PortamentoControlFlag != 0)
			{
				if (voiceInfo.PortamentoControlFlag >= 0x80)
				{
					int temp = (short)((playbackInfo.NotePeriod * (voiceInfo.PortamentoAddValue & 0xffff) >> 16) + (playbackInfo.NotePeriod * (voiceInfo.PortamentoAddValue >> 16))) - playbackInfo.NotePeriod;
					voiceInfo.PortamentoAddValue = temp / voiceInfo.PortamentoDurationCounter;

					voiceInfo.PortamentoControlFlag &= 0x7f;
				}

				if (voiceInfo.PortamentoStartDelayCounter == 0)
				{
					if (voiceInfo.PortamentoDurationCounter != 0)
					{
						voiceInfo.PortamentoDurationCounter--;
						playbackInfo.PortamentoAddValue = (short)(playbackInfo.PortamentoAddValue + voiceInfo.PortamentoAddValue);
					}
				}
				else
					voiceInfo.PortamentoStartDelayCounter--;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle volume fade
		/// </summary>
		/********************************************************************/
		private void DoVolumeFade(VoiceInfo voiceInfo, VoicePlaybackInfo playbackInfo, IChannel channel)
		{
			if (voiceInfo.VolumeFadeRunning && (voiceInfo.VolumeFadeDurationCounter != 0))
			{
				voiceInfo.VolumeFadeSpeedCounter--;

				if (voiceInfo.VolumeFadeSpeedCounter == 0)
				{
					voiceInfo.VolumeFadeDurationCounter--;

					voiceInfo.VolumeFadeSpeedCounter = voiceInfo.VolumeFadeSpeed;

					int volume = voiceInfo.VolumeFadeValue + voiceInfo.VolumeFadeAddValue;
					voiceInfo.VolumeFadeValue = (short)volume;

					volume += voiceInfo.SampleInfo2.Volume;
					if (volume < 0)
						voiceInfo.VolumeFadeDurationCounter = 0;
					else
					{
						if (volume > 64)
							volume = 64;

						SetupSample(playbackInfo, channel, voiceInfo.SampleInfo2, voiceInfo.TransposedNote, voiceInfo.VolumeFadeSpeed, (short)volume, 0, 0);
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle master volume fade
		/// </summary>
		/********************************************************************/
		private void DoMasterVolumeFade()
		{
			if (playingInfo.MasterVolumeFadeSpeed != 0)
			{
				playingInfo.MasterVolumeFadeSpeedCounter--;

				if (playingInfo.MasterVolumeFadeSpeedCounter == 0)
				{
					playingInfo.MasterVolumeFadeSpeedCounter = playingInfo.MasterVolumeFadeSpeed;

					if (features.MasterVolumeFadeVersion == 2)
						playingInfo.MasterVolumeFadeSpeedCounter--;

					playingInfo.MasterVolume--;

					if (playingInfo.MasterVolume < 0)
					{
						playingInfo.EnablePlaying = false;

						for (int i = 0; i < 4; i++)
							VirtualChannels[i].Mute();
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
				sb.Append(voices[i].PositionList.Length);
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
				sb.Append(voices[i].CurrentPosition);
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
				sb.Append(voices[i].PlayingTrack);
				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}
		#endregion
	}
}
