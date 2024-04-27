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
using Polycode.NostalgicPlayer.Agent.Player.DavidWhittaker.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.DavidWhittaker
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class DavidWhittakerWorker : ModulePlayerWithSubSongDurationAgentBase
	{
		private bool oldPlayer;

		private bool uses32BitPointers;

		private int startOffset;

		private int sampleInfoOffset;
		private int sampleDataOffset;
		private int subSongListOffset;
		private int arpeggioListOffset;
		private int envelopeListOffset;
		private int channelVolumeOffset;

		private int numberOfSamples;
		private int numberOfChannels;

		private List<SongInfo> songInfoList;
		private Dictionary<uint, byte[]> tracks;
		private Dictionary<uint, int> trackNumbers;
		private byte[][] arpeggios;
		private byte[][] envelopes;
		private Sample[] samples;
		private ushort[] channelVolumes;

		private bool enableSampleTranspose;
		private bool enableChannelTranspose;
		private bool enableGlobalTranspose;

		private byte newSampleCmd;
		private byte newEnvelopeCmd;
		private byte newArpeggioCmd;

		private bool enableArpeggio;
		private bool enableEnvelopes;
		private bool enableVibrato;

		private bool enableVolumeFade;
		private bool enableHalfVolume;
		private bool enableGlobalVolumeFade;
		private bool enableSetGlobalVolume;

		private bool enableSquareWaveform;
		private int squareWaveformSampleNumber;
		private uint squareWaveformSampleLength;
		private ushort squareChangeMinPosition;
		private ushort squareChangeMaxPosition;
		private byte squareChangeSpeed;
		private sbyte squareByte1;
		private sbyte squareByte2;

		private bool useExtraCounter;

		private bool enableDelayCounter;
		private bool enableDelayMultiply;
		private bool enableDelaySpeed;

		private ushort[] periods;

		private GlobalPlayingInfo playingInfo;
		private ChannelInfo[] channels;

		private int currentSong;

		private const int InfoPositionLine = 3;
		private const int InfoTrackLine = 4;
		private const int InfoSpeedLine = 5;

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => new [] { "dw", "dwold" };



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 2048)
				return AgentResult.Unknown;

			// Read the first part of the file, so it is easier to search
			byte[] buffer = new byte[16384];

			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.Read(buffer, 0, buffer.Length);

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
					description = Resources.IDS_DW_INFODESCLINE0;
					value = FormatPositionLengths();
					break;
				}

				// Used tracks
				case 1:
				{
					description = Resources.IDS_DW_INFODESCLINE1;
					value = tracks.Count.ToString();
					break;
				}

				// Used samples
				case 2:
				{
					description = Resources.IDS_DW_INFODESCLINE2;
					value = samples.Length.ToString();
					break;
				}

				// Playing positions
				case 3:
				{
					description = Resources.IDS_DW_INFODESCLINE3;
					value = FormatPositions();
					break;
				}

				// Playing tracks
				case 4:
				{
					description = Resources.IDS_DW_INFODESCLINE4;
					value = FormatTracks();
					break;
				}

				// Current speed
				case 5:
				{
					description = Resources.IDS_DW_INFODESCLINE5;
					value = playingInfo.Speed.ToString();
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

				if (!LoadSubSongInfoAndTracks(moduleStream, out int numberOfArpeggios, out int numberOfEnvelopes, out errorMessage))
					return AgentResult.Error;

				if (!LoadArpeggios(moduleStream, numberOfArpeggios))
				{
					errorMessage = Resources.IDS_DW_ERR_LOADING_ARPEGGIOS;
					return AgentResult.Error;
				}

				if (!LoadEnvelopes(moduleStream, numberOfEnvelopes))
				{
					errorMessage = Resources.IDS_DW_ERR_LOADING_ENVELOPES;
					return AgentResult.Error;
				}

				if (!LoadChannelVolumes(moduleStream, numberOfEnvelopes))
				{
					errorMessage = Resources.IDS_DW_ERR_LOADING_CHANNELVOLS;
					return AgentResult.Error;
				}

				if (!LoadSampleInfo(moduleStream))
				{
					errorMessage = Resources.IDS_DW_ERR_LOADING_SAMPLEINFO;
					return AgentResult.Error;
				}

				if (!LoadSampleData(moduleStream))
				{
					errorMessage = Resources.IDS_DW_ERR_LOADING_SAMPLES;
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
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public override bool InitPlayer(out string errorMessage)
		{
			if (!base.InitPlayer(out errorMessage))
				return false;

			trackNumbers = tracks.Keys.Order().Select((key, index) => (key, index)).ToDictionary(x => x.key, x => x.index);

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

			// Remember the song number
			currentSong = songNumber;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			if (enableDelayCounter)
			{
				playingInfo.DelayCounter += playingInfo.DelayCounterSpeed;
				if (playingInfo.DelayCounter > 255)
				{
					playingInfo.DelayCounter -= 256;
					return;
				}
			}

			if (useExtraCounter)
			{
				playingInfo.ExtraCounter--;
				if (playingInfo.ExtraCounter == 0)
				{
					playingInfo.ExtraCounter = 6;
					return;
				}
			}

			if (enableGlobalVolumeFade)
			{
				if (playingInfo.GlobalVolumeFadeSpeed != 0)
				{
					if (playingInfo.GlobalVolume > 0)
					{
						playingInfo.GlobalVolumeFadeCounter--;
						if (playingInfo.GlobalVolumeFadeCounter == 0)
						{
							playingInfo.GlobalVolume--;
							if (playingInfo.GlobalVolume > 0)
								playingInfo.GlobalVolumeFadeCounter = playingInfo.GlobalVolumeFadeSpeed;
						}
					}
				}
			}

			ChangeSquareWaveform();

			for (int i = 0; i < numberOfChannels; i++)
			{
				ChannelInfo channelInfo = channels[i];
				IChannel channel = VirtualChannels[i];

				channelInfo.SpeedCounter--;

				if (channelInfo.SpeedCounter == 0)
					ReadTrackCommands(channelInfo, channel);
				else if (channelInfo.SpeedCounter > 1)
					DoFrameStuff(channelInfo, channel);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public override int ModuleChannelCount => numberOfChannels;



		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public override SubSongInfo SubSongs => new SubSongInfo(songInfoList.Count, 0);



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

					for (int j = 0; j < Tables.Periods3.Length; j++)
					{
						uint period = (uint)((Tables.Periods3[j] * sample.FineTunePeriod) >> 10);
						frequencies[1 * 12 - 3 + j] = 3546895U / period;
					}

					SampleInfo sampleInfo = new SampleInfo
					{
						Name = string.Empty,
						Type = SampleInfo.SampleType.Sample,
						BitSize = SampleInfo.SampleSize._8Bit,
						Volume = (ushort)(sample.Volume * 4),
						Panning = -1,
						Sample = sample.SampleData,
						Length = sample.Length,
						NoteFrequencies = frequencies
					};

					if (sample.LoopStart >= 0)
					{
						sampleInfo.LoopStart = (uint)sample.LoopStart;
						sampleInfo.LoopLength = sampleInfo.Length - sampleInfo.LoopStart;
						sampleInfo.Flags = SampleInfo.SampleFlag.Loop;
					}
					else
						sampleInfo.Flags = SampleInfo.SampleFlag.None;

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
			return new Snapshot(playingInfo, channels);
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
			Snapshot clonedSnapshot = new Snapshot(currentSnapshot.PlayingInfo, currentSnapshot.Channels);

			playingInfo = clonedSnapshot.PlayingInfo;
			channels = clonedSnapshot.Channels;

			UpdateModuleInformation();

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Test the file to see if its a David Whittaker player and extract
		/// needed information
		/// </summary>
		/********************************************************************/
		private AgentResult TestModule(byte[] buffer)
		{
			// SC68 support are handled in a converter, so ignore these modules here
			if ((buffer[0] == 0x53) && (buffer[1] == 0x43) && (buffer[2] == 0x36) && (buffer[3] == 0x38))
				return AgentResult.Unknown;

			if (!ExtractInfoFromInitFunction(buffer))
				return AgentResult.Unknown;

			if (!ExtractInfoFromPlayFunction(buffer))
				return AgentResult.Unknown;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Try to find the init function in the player and extract needed
		/// information from it
		/// </summary>
		/********************************************************************/
		private bool ExtractInfoFromInitFunction(byte[] searchBuffer)
		{
			int searchLength = searchBuffer.Length;
			int index;

			// Start to find the init function in the player
			for (index = 0; index < searchLength; index += 2)
			{
				if ((searchBuffer[index] == 0x47) && (searchBuffer[index + 1] == 0xfa) && ((searchBuffer[index + 2] & 0xf0) == 0xf0))
					break;
			}

			if (index >= (searchLength - 6))
				return false;

			startOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;

			for (; index < searchLength; index += 2)
			{
				if ((searchBuffer[index] == 0x61) && (searchBuffer[index + 1] == 0x00))
					break;
			}

			if (index >= (searchLength - 8))
				return false;

			int startOfInit = index;
			int startOfSampleInit = index;

			if ((searchBuffer[index + 4] == 0x61) && (searchBuffer[index + 5] == 0x00))
				startOfSampleInit = (((sbyte)searchBuffer[index + 6] << 8) | searchBuffer[index + 7]) + index + 6;

			//
			// Extract information about samples
			//
			for (index = startOfSampleInit; index < searchLength; index += 2)
			{
				if ((searchBuffer[index] == 0x4a) && (searchBuffer[index + 1] == 0x2b))
					break;
			}

			if (index >= (searchLength - 36))
				return false;

			if (searchBuffer[index + 4] != 0x66)
			{
				// Maybe this is a format where the sample initializing is not in a sub-function like QBall,
				// so check for this
				for (index = startOfInit; index < searchLength; index += 2)
				{
					if ((searchBuffer[index] == 0x41) && (searchBuffer[index + 1] == 0xeb))
						break;
				}

				if (index >= (searchLength - 36))
					return false;

				sampleDataOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + startOffset;
				index += 4;

				if (searchBuffer[index + 4] != 0x72)
					return false;

				numberOfSamples = (searchBuffer[index + 5] & 0x00ff) + 1;

				for (; index < searchLength - 4; index += 2)
				{
					if ((searchBuffer[index] == 0x41) && (searchBuffer[index + 1] == 0xeb) && (searchBuffer[index + 4] == 0xe3) && (searchBuffer[index + 5] == 0x4f))
						break;
				}

				if (index >= (searchLength - 4))
					return false;

				channelVolumeOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + startOffset;

				//
				// Extract sub-song information
				//
				for (index = startOfInit; index < searchLength; index += 2)
				{
					if ((searchBuffer[index] == 0x41) && (searchBuffer[index + 1] == 0xeb) && (searchBuffer[index + 4] == 0x17))
						break;
				}

				if (index >= (searchLength - 4))
					return false;

				subSongListOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + startOffset;

				uses32BitPointers = true;
				oldPlayer = true;
			}
			else
			{
				oldPlayer = false;

				if (searchBuffer[index + 5] == 0x00)
					index += 2;

				if ((searchBuffer[index + 6] != 0x41) || (searchBuffer[index + 7] != 0xfa))
					return false;

				sampleDataOffset = (((sbyte)searchBuffer[index + 8] << 8) | searchBuffer[index + 9]) + index + 8;
				index += 10;

				if ((searchBuffer[index] == 0x27) && (searchBuffer[index + 1] == 0x48) && (searchBuffer[index + 4] == 0xd0) && (searchBuffer[index + 5] == 0xfc))
				{
					sampleDataOffset += ((searchBuffer[index + 6] << 8) | searchBuffer[index + 7]);
					index += 12;

					if ((searchBuffer[index] != 0xd0) || (searchBuffer[index + 1] != 0xfc))
						return false;

					sampleDataOffset += ((searchBuffer[index + 2] << 8) | searchBuffer[index + 3]);
					index += 4;
				}

				if ((searchBuffer[index] != 0x4b) || (searchBuffer[index + 1] != 0xfa) || (searchBuffer[index + 4] != 0x72))
					return false;

				sampleInfoOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;
				numberOfSamples = (searchBuffer[index + 5] & 0x00ff) + 1;

				index += 8;

				for (; index < searchLength - 4; index += 2)
				{
					if ((searchBuffer[index] == 0x37) && (searchBuffer[index + 1] == 0x7c))
					{
						squareWaveformSampleLength = (uint)(((searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) * 2);
						break;
					}
				}

				//
				// Extract sub-song information
				//
				for (index = startOfInit; index < searchLength; index += 2)
				{
					if ((searchBuffer[index] == 0x41) && (searchBuffer[index + 1] == 0xfa) && (searchBuffer[index + 4] != 0x4b))
						break;
				}

				if (index >= (searchLength - 4))
					return false;

				if (((searchBuffer[index + 4] != 0x12) || (searchBuffer[index + 5] != 0x30)) && ((searchBuffer[index + 4] != 0x37) || (searchBuffer[index + 5] != 0x70)))
					return false;

				subSongListOffset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;
				index += 4;

				//
				// Find out if pointers or offsets are used
				//
				for (; index < searchLength; index += 2)
				{
					if ((searchBuffer[index] == 0x41) && (searchBuffer[index + 1] == 0xfa) && (searchBuffer[index + 4] != 0x23))
						break;
				}

				if (index >= (searchLength - 8))
					return false;

				if ((searchBuffer[index + 4] == 0x20) && (searchBuffer[index + 5] == 0x70))
					uses32BitPointers = true;
				else if ((searchBuffer[index + 4] == 0x30) && (searchBuffer[index + 5] == 0x70))
					uses32BitPointers = false;
				else
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Try to find the play function in the player and extract needed
		/// information from it
		/// </summary>
		/********************************************************************/
		private bool ExtractInfoFromPlayFunction(byte[] searchBuffer)
		{
			int searchLength = searchBuffer.Length;
			int index, offset;

			// Start to find the play function in the player
			for (index = 0; index < searchLength; index += 2)
			{
				if ((searchBuffer[index] == 0x47) && (searchBuffer[index + 1] == 0xfa))
				{
					if (index >= (searchLength - 10))
						return false;

					if ((searchBuffer[index + 4] == 0x4a) && (searchBuffer[index + 5] == 0x2b) && (searchBuffer[index + 8] == 0x67))
					{
						if ((searchBuffer[index + 10] == 0x33) && (searchBuffer[index + 11] == 0xfc))
							continue;

						if ((searchBuffer[index + 10] == 0x17) && (searchBuffer[index + 11] == 0x7c))
							continue;

						if ((searchBuffer[index + 10] == 0x08) && (searchBuffer[index + 11] == 0xb9))
							continue;

						break;
					}
				}
			}

			int startOfPlay = index;

			//
			// Check for delay counter
			//
			enableDelayCounter = false;
			enableDelayMultiply = false;

			for (index = startOfPlay; index < startOfPlay + 100; index += 2)
			{
				if ((searchBuffer[index] == 0x10) && (searchBuffer[index + 1] == 0x3a))
				{
					enableDelayCounter = true;

					if ((searchBuffer[index + 6] == 0xc0) && (searchBuffer[index + 7] == 0xfc))
						enableDelayMultiply = true;

					break;
				}
			}

			//
			// Check for extra counter
			//
			useExtraCounter = false;

			for (index = startOfPlay; index < startOfPlay + 100; index += 2)
			{
				if ((searchBuffer[index] == 0x53) && (searchBuffer[index + 1] == 0x2b) && (searchBuffer[index + 4] == 0x66))
				{
					if (searchBuffer[index + 6] == 0x17)
					{
						offset = (searchBuffer[index - 4] << 8) | searchBuffer[index - 3];
						useExtraCounter = searchBuffer[offset + startOffset] != 0;
					}
					break;
				}
			}

			//
			// Check for square waveform
			//
			enableSquareWaveform = false;

			for (index = startOfPlay; index < startOfPlay + 100; index += 2)
			{
				if ((searchBuffer[index] == 0x20) && (searchBuffer[index + 1] == 0x7a) && (searchBuffer[index + 4] == 0x30) && (searchBuffer[index + 5] == 0x3a))
				{
					enableSquareWaveform = true;

					offset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;
					squareWaveformSampleNumber = (offset - sampleInfoOffset) / 12;

					if (((searchBuffer[index + 14] != 0x31) && (searchBuffer[index + 14] != 0x11)) || (searchBuffer[index + 15] != 0xbc))
						return false;

					squareByte1 = (sbyte)searchBuffer[index + 17];

					if (((searchBuffer[index + 20] & 0xf0) != 0x50) || (searchBuffer[index + 21] != 0x6b))
						return false;

					squareChangeSpeed = (byte)((searchBuffer[index + 20] & 0x0e) >> 1);

					if ((searchBuffer[index + 24] != 0x0c) || (searchBuffer[index + 25] != 0x6b))
						return false;

					squareChangeMaxPosition = (ushort)((searchBuffer[index + 26] << 8) | searchBuffer[index + 27]);

					if (((searchBuffer[index + 38] != 0x31) && (searchBuffer[index + 38] != 0x11)) || (searchBuffer[index + 39] != 0xbc))
						return false;

					squareByte2 = (sbyte)searchBuffer[index + 41];

					if ((searchBuffer[index + 48] != 0x0c) || (searchBuffer[index + 49] != 0x6b))
						return false;

					squareChangeMinPosition = (ushort)((searchBuffer[index + 50] << 8) | searchBuffer[index + 51]);
					break;
				}
			}

			//
			// Get number of channels used
			//
			numberOfChannels = 0;

			for (index = startOfPlay; index < startOfPlay + 200; index += 2)
			{
				if (searchBuffer[index] == 0x7e)
				{
					numberOfChannels = searchBuffer[index + 1];
					if (numberOfChannels == 0)
					{
						for (; index < startOfPlay + 500; index += 2)
						{
							if ((searchBuffer[index] == 0xbe) && ((searchBuffer[index + 1] == 0x7c) || (searchBuffer[index + 1] == 0x3c)))
							{
								numberOfChannels = searchBuffer[index + 3];
								break;
							}
						}
					}
					else
						numberOfChannels++;

					break;
				}
			}

			if (numberOfChannels == 0)
				return false;

			//
			// Find different parts of the player
			//
			int readTrackCommandsOffset, doFrameStuffOffset;

			if (oldPlayer)
			{
				for (index = startOfPlay; index < searchLength; index += 2)
				{
					if ((searchBuffer[index] == 0x70) && (searchBuffer[index + 1] == 0x00))
						break;
				}

				readTrackCommandsOffset = index;

				if (searchBuffer[index + 2] != 0x10)
					return false;

				doFrameStuffOffset = -1;
			}
			else
			{
				for (index = startOfPlay; index < searchLength; index += 2)
				{
					if ((searchBuffer[index] == 0x53) && (searchBuffer[index + 1] == 0x68))
						break;
				}

				if (index >= (searchLength - 16))
					return false;

				if (searchBuffer[index + 4] != 0x67)
					return false;

				readTrackCommandsOffset = searchBuffer[index + 5] + index + 6;

				if (searchBuffer[index + 12] != 0x66)
					return false;

				if (searchBuffer[index + 13] == 0x00)
					doFrameStuffOffset = ((searchBuffer[index + 14] << 8) | searchBuffer[index + 15]) + index + 14;
				else
					doFrameStuffOffset = searchBuffer[index + 13] + index + 14;
			}

			for (index = readTrackCommandsOffset; index < searchLength; index += 2)
			{
				if ((searchBuffer[index] == 0x6b) && (searchBuffer[index + 1] == 0x00))
					break;
			}

			int doCommandsOffset = ((searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;

			//
			// Find period table to check which version to use
			//
			if (oldPlayer)
			{
				periods = Tables.Periods1;
			}
			else
			{
				for (index = readTrackCommandsOffset; index < searchLength; index += 2)
				{
					if ((searchBuffer[index] == 0x45) && (searchBuffer[index + 1] == 0xfa) && (searchBuffer[index + 4] == 0x32) && (searchBuffer[index + 5] == 0x2d))
						break;
				}

				if (index >= (searchLength - 6))
					return false;

				offset =  ((searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;
				if (offset >= (searchLength - 72 * 2))
					return false;

				if ((searchBuffer[offset] == 0x10) && (searchBuffer[offset + 1] == 0x00))
					periods = Tables.Periods2;
				else if ((searchBuffer[offset] == 0x20) && (searchBuffer[offset + 1] == 0x00))
					periods = Tables.Periods3;
				else
					return false;
			}

			//
			// Check for support of different transposes
			//
			for (index = readTrackCommandsOffset; index < searchLength; index += 2)
			{
				if ((searchBuffer[index] == 0x6b) && (searchBuffer[index + 1] == 0x00))
					break;
			}

			if (index >= (searchLength - 6))
				return false;

			enableSampleTranspose = false;
			if ((searchBuffer[index + 4] == 0xd0) && (searchBuffer[index + 5] == 0x2d))
				enableSampleTranspose = true;

			enableGlobalTranspose = false;
			if ((doFrameStuffOffset != -1) && (searchBuffer[doFrameStuffOffset] == 0x10) && (searchBuffer[doFrameStuffOffset + 1] == 0x28))
			{
				if ((searchBuffer[doFrameStuffOffset + 4] == 0xd0) && (searchBuffer[doFrameStuffOffset + 5] == 0x3a))
					enableGlobalTranspose = true;

				if ((searchBuffer[doFrameStuffOffset + 8] == 0xd0) && (searchBuffer[doFrameStuffOffset + 9] == 0x28))
					enableChannelTranspose = true;
			}

			//
			// Check different command ranges
			//
			enableArpeggio = false;
			enableEnvelopes = false;
			newSampleCmd = 0;

			for (index = doCommandsOffset; index < searchLength - 28; index += 2)
			{
				if ((searchBuffer[index] == 0x4e) && ((searchBuffer[index + 1] == 0xd2) || (searchBuffer[index + 1] == 0xf3)))
					break;

				if (((searchBuffer[index] == 0xb0) && (searchBuffer[index + 1] == 0x3c)) || ((searchBuffer[index] == 0x0c) && (searchBuffer[index + 1] == 0x00)))
				{
					if ((searchBuffer[index + 2] == 0x00) && ((searchBuffer[index + 4] == 0x65) || (searchBuffer[index + 4] == 0x6d)))
					{
						if ((searchBuffer[index + 10] == 0xd0) && (searchBuffer[index + 11] == 0x40) && (searchBuffer[index + 12] == 0x45) && (searchBuffer[index + 13] == 0xfa))
						{
							if ((searchBuffer[index + 22] == 0x21) && (searchBuffer[index + 23] == 0x4a) && (searchBuffer[index + 26] == 0x21) && (searchBuffer[index + 27] == 0x4a))
							{
								enableArpeggio = true;
								newArpeggioCmd = searchBuffer[index + 3];
								arpeggioListOffset = (((sbyte)searchBuffer[index + 14] << 8) | searchBuffer[index + 15]) + index + 14;
							}
							else if ((searchBuffer[index + 22] == 0x21) && (searchBuffer[index + 23] == 0x4a) && (searchBuffer[index + 26] == 0x11) && (searchBuffer[index + 27] == 0x6a))
							{
								enableEnvelopes = true;
								newEnvelopeCmd = searchBuffer[index + 3];
								envelopeListOffset = (((sbyte)searchBuffer[index + 14] << 8) | searchBuffer[index + 15]) + index + 14;
							}
						}
						else if ((searchBuffer[index + 10] == 0x4b) && (searchBuffer[index + 11] == 0xfa) && (searchBuffer[index + 14] == 0xc0) && (searchBuffer[index + 15] == 0xfc))
						{
							newSampleCmd = searchBuffer[index + 3];
						}
					}
				}
			}

			if (!oldPlayer && (newSampleCmd == 0))
				return false;

			//
			// Check different effects
			//
			int jumpTableOffset;

			if ((searchBuffer[index - 10] == 0x45) && (searchBuffer[index - 9] == 0xfa))
				jumpTableOffset = (((sbyte)searchBuffer[index - 8] << 8) | searchBuffer[index - 7]) + index - 8;
			else if ((searchBuffer[index - 8] == 0x45) && (searchBuffer[index - 7] == 0xfa))
				jumpTableOffset = (((sbyte)searchBuffer[index - 6] << 8) | searchBuffer[index - 5]) + index - 6;
			else if ((searchBuffer[index - 10] == 0x45) && (searchBuffer[index - 9] == 0xeb))
				jumpTableOffset = (((sbyte)searchBuffer[index - 8] << 8) | searchBuffer[index - 7]) + startOffset;
			else
				return false;

			enableVibrato = false;

			int effectOffset = ((searchBuffer[jumpTableOffset + 6 * 2] << 8) | searchBuffer[jumpTableOffset + 6 * 2 + 1]) + startOffset;
			if ((effectOffset >= 0) && (effectOffset < (searchLength - 6)) && (searchBuffer[effectOffset] == 0x50) && (searchBuffer[effectOffset + 1] == 0xe8) && (searchBuffer[effectOffset + 4] == 0x11) && (searchBuffer[effectOffset + 5] == 0x59))
				enableVibrato = true;

			enableVolumeFade = false;

			effectOffset = ((searchBuffer[jumpTableOffset + 8 * 2] << 8) | searchBuffer[jumpTableOffset + 8 * 2 + 1]) + startOffset;
			if ((effectOffset >= 0) && (effectOffset < (searchLength - 2)) && (searchBuffer[effectOffset] == 0x17) && (searchBuffer[effectOffset + 1] == 0x59))
				enableVolumeFade = true;

			enableHalfVolume = false;

			effectOffset = ((searchBuffer[jumpTableOffset + 8 * 2] << 8) | searchBuffer[jumpTableOffset + 8 * 2 + 1]) + startOffset;
			if ((effectOffset >= 0) && (effectOffset < (searchLength - 2)) && (searchBuffer[effectOffset] == 0x50) && (searchBuffer[effectOffset + 1] == 0xe8))
			{
				effectOffset = ((searchBuffer[jumpTableOffset + 9 * 2] << 8) | searchBuffer[jumpTableOffset + 9 * 2 + 1]) + startOffset;
				if ((effectOffset < (searchLength - 2)) && (searchBuffer[effectOffset] == 0x51) && (searchBuffer[effectOffset + 1] == 0xe8))
					enableHalfVolume = true;
			}

			enableGlobalVolumeFade = false;

			effectOffset = ((searchBuffer[jumpTableOffset + 11 * 2] << 8) | searchBuffer[jumpTableOffset + 11 * 2 + 1]) + startOffset;
			if ((effectOffset >= 0) && (effectOffset < (searchLength - 2)) && (searchBuffer[effectOffset] == 0x17) && (searchBuffer[effectOffset + 1] == 0x59))
				enableGlobalVolumeFade = true;

			enableDelaySpeed = false;

			effectOffset = ((searchBuffer[jumpTableOffset + 10 * 2] << 8) | searchBuffer[jumpTableOffset + 10 * 2 + 1]) + startOffset;
			if ((effectOffset >= 0) && (effectOffset < (searchLength - 4)) && (searchBuffer[effectOffset] == 0x10) && (searchBuffer[effectOffset + 1] == 0x19) && (searchBuffer[effectOffset + 2] == 0x17) && (searchBuffer[effectOffset + 3] == 0x40))
				enableDelaySpeed = true;

			enableSetGlobalVolume = true;

			effectOffset = ((searchBuffer[jumpTableOffset + 12 * 2] << 8) | searchBuffer[jumpTableOffset + 12 * 2 + 1]) + startOffset;
			if ((effectOffset >= 0) && (effectOffset < (searchLength - 4)) && (searchBuffer[effectOffset + 2] == 0x42) && (searchBuffer[effectOffset + 3] == 0x41))
				enableSetGlobalVolume = false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load the sub-song information for all sub-songs including
		/// position lists and tracks
		/// </summary>
		/********************************************************************/
		private bool LoadSubSongInfoAndTracks(ModuleStream moduleStream, out int numberOfArpeggios, out int numberOfEnvelopes, out string errorMessage)
		{
			numberOfArpeggios = 0;
			numberOfEnvelopes = 0;
			errorMessage = string.Empty;

			songInfoList = new List<SongInfo>();
			tracks = new Dictionary<uint, byte[]>();

			moduleStream.Seek(subSongListOffset, SeekOrigin.Begin);

			long minPositionOffset = long.MaxValue;

			for (;;)
			{
				ushort songSpeed;
				byte delaySpeed;

				if ((moduleStream.Position + 8) >= minPositionOffset)
					break;

				if (enableDelayCounter)
				{
					songSpeed = moduleStream.Read_UINT8();
					delaySpeed = moduleStream.Read_UINT8();
				}
				else
				{
					songSpeed = moduleStream.Read_B_UINT16();
					delaySpeed = 0;
				}

				if (songSpeed > 255)
					break;

				SongInfo songInfo = new SongInfo
				{
					Speed = songSpeed,
					DelayCounterSpeed = delaySpeed,
					PositionLists = new PositionList[numberOfChannels]
				};

				uint[] positionOffsets = new uint[numberOfChannels];

				if (uses32BitPointers)
				{
					for (int i = 0; i < numberOfChannels; i++)
					{
						positionOffsets[i] = moduleStream.Read_B_UINT32();
						minPositionOffset = Math.Min(minPositionOffset, positionOffsets[i] + startOffset);
					}
				}
				else
				{
					for (int i = 0; i < numberOfChannels; i++)
					{
						positionOffsets[i] = moduleStream.Read_B_UINT16();
						minPositionOffset = Math.Min(minPositionOffset, positionOffsets[i] + startOffset);
					}
				}

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_DW_ERR_LOADING_SUBSONG;
					return false;
				}

				long currentPosition = moduleStream.Position;

				for (int i = 0; i < numberOfChannels; i++)
				{
					PositionList positionList = LoadPositionList(moduleStream, positionOffsets[i], out int arpeggioCount, out int envelopeCount);
					if (positionList == null)
					{
						errorMessage = Resources.IDS_DW_ERR_LOADING_TRACKS;
						return false;
					}

					songInfo.PositionLists[i] = positionList;

					numberOfArpeggios = Math.Max(numberOfArpeggios, arpeggioCount);
					numberOfEnvelopes = Math.Max(numberOfEnvelopes, envelopeCount);
				}

				songInfoList.Add(songInfo);

				moduleStream.Position = currentPosition;
			}

			return songInfoList.Count > 0;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single position list and initialize the track offset list
		/// </summary>
		/********************************************************************/
		private PositionList LoadPositionList(ModuleStream moduleStream, uint startPosition, out int numberOfArpeggios, out int numberOfEnvelopes)
		{
			numberOfArpeggios = 0;
			numberOfEnvelopes = 0;

			if (startPosition == 0)
				return null;

			moduleStream.Seek(startPosition + startOffset, SeekOrigin.Begin);

			long minPosition = startPosition;
			long maxPosition = startPosition;

			List<uint> positionList = new List<uint>();
			ushort restartPosition = 0;

			for (;;)
			{
				uint trackOffset = uses32BitPointers ? moduleStream.Read_B_UINT32() : moduleStream.Read_B_UINT16();
				if ((trackOffset == 0) || (trackOffset >= moduleStream.Length) || ((trackOffset & 0x8000) != 0))
					break;

				if (moduleStream.EndOfStream)
					return null;

				long currentPosition = moduleStream.Position;

				positionList.Add(trackOffset);

				if (!LoadAndParseTrack(moduleStream, trackOffset, out int newPositionListOffset, out int arpeggioCount, out int envelopeCount))
					return null;

				numberOfArpeggios = Math.Max(numberOfArpeggios, arpeggioCount);
				numberOfEnvelopes = Math.Max(numberOfEnvelopes, envelopeCount);

				if ((newPositionListOffset != 0) && ((newPositionListOffset < minPosition) || (newPositionListOffset > maxPosition)))
				{
					restartPosition = (ushort)positionList.Count;
					currentPosition = newPositionListOffset + startOffset;
				}

				minPosition = Math.Min(minPosition, currentPosition);
				maxPosition = Math.Max(maxPosition, currentPosition);

				moduleStream.Position = currentPosition;
			}

			return new PositionList
			{
				TrackOffsets = positionList.ToArray(),
				RestartPosition = restartPosition
			};
		}



		/********************************************************************/
		/// <summary>
		/// Parse a single track. It will load the track into memory, if not
		/// already loaded
		/// </summary>
		/********************************************************************/
		private bool LoadAndParseTrack(ModuleStream moduleStream, uint trackOffset, out int newPositionListOffset, out int numberOfArpeggios, out int numberOfEnvelopes)
		{
			numberOfArpeggios = 0;
			numberOfEnvelopes = 0;
			newPositionListOffset = 0;

			if (!tracks.TryGetValue(trackOffset, out byte[] trackBytes))
			{
				trackBytes = LoadTrack(moduleStream, trackOffset);
				if (trackBytes == null)
					return false;

				tracks[trackOffset] = trackBytes;
			}

			// Parse the track to count number of arpeggios, envelopes and find restart position
			for (int index = 0; index < trackBytes.Length; )
			{
				byte byt = trackBytes[index++];

				if ((byt & 0x80) != 0)
				{
					if (byt >= 0xe0)
						continue;

					if (!oldPlayer && (byt >= newSampleCmd))
						continue;

					if (enableEnvelopes && (byt >= newEnvelopeCmd))
					{
						numberOfEnvelopes = Math.Max(numberOfEnvelopes, byt - newEnvelopeCmd + 1);
						continue;
					}

					if (enableArpeggio && (byt >= newArpeggioCmd))
					{
						numberOfArpeggios = Math.Max(numberOfArpeggios, byt - newArpeggioCmd + 1);
						continue;
					}

					Effect effect = (Effect)(byt & 0x7f);

					int effectCount = FindEffectByteCount(effect);
					if (effectCount == -1)
						break;

					index += effectCount;

					if ((effect == Effect.Effect9) && !enableHalfVolume)
						newPositionListOffset = (trackBytes[index - 2] << 8) | trackBytes[index - 1];
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single track
		/// </summary>
		/********************************************************************/
		private byte[] LoadTrack(ModuleStream moduleStream, uint trackOffset)
		{
			moduleStream.Seek(trackOffset + startOffset, SeekOrigin.Begin);

			List<byte> trackBytes = new List<byte>();

			// Read the track
			for (;;)
			{
				byte byt = moduleStream.Read_UINT8();
				if (moduleStream.EndOfStream)
					return null;

				trackBytes.Add(byt);

				if ((byt & 0x80) != 0)
				{
					if (byt >= 0xe0)
						continue;

					if (!oldPlayer && (byt >= newSampleCmd))
						continue;

					if (enableEnvelopes && (byt >= newEnvelopeCmd))
						continue;

					if (enableArpeggio && (byt >= newArpeggioCmd))
						continue;

					Effect effect = (Effect)(byt & 0x7f);

					int effectCount = FindEffectByteCount(effect);
					if (effectCount == -1)
						break;

					for (; effectCount > 0; effectCount--)
						trackBytes.Add(moduleStream.Read_UINT8());
				}
			}

			return trackBytes.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Find out how many bytes the given effect uses after the effect
		/// itself. -1 means stop parsing
		/// </summary>
		/********************************************************************/
		private int FindEffectByteCount(Effect effect)
		{
			if (oldPlayer)
			{
				// For the old player (QBall), the effect is in another order
				switch ((int)effect)
				{
					case 0:		// EndOfTrack
					case 1:		// StopSong
						return -1;

					case 2:		// ???
						return 1;
				}
			}
			else
			{
				switch (effect)
				{
					case Effect.EndOfTrack:
					case Effect.StopSong:
						return -1;

					case Effect.Mute:
					case Effect.WaitUntilNextRow:
					case Effect.StopVibrato:
					case Effect.StopSoundFx:
						return 0;

					case Effect.GlobalTranspose:
					case Effect.Effect8:
					case Effect.SetSpeed:
					case Effect.GlobalVolumeFade:
					case Effect.SetGlobalVolume:
						return 1;

					case Effect.Slide:
					case Effect.StartVibrato:
						return 2;

					case Effect.Effect9:
					{
						if (enableHalfVolume)
							return 0;

						return 2;
					}

					case Effect.StartOrStopSoundFx:
					{
						if (enableSetGlobalVolume)
							return 1;

						return 0;
					}
				}
			}

			throw new Exception($"Invalid track byte detected ({(byte)effect})");
		}



		/********************************************************************/
		/// <summary>
		/// Load all the arpeggios
		/// </summary>
		/********************************************************************/
		private bool LoadArpeggios(ModuleStream moduleStream, int numberOfArpeggios)
		{
			if (numberOfArpeggios == 0)
			{
				// We need at least one empty arpeggio
				arpeggios = new byte[1][];
				arpeggios[0] = new byte[] { 0x80 };

				return true;
			}

			arpeggios = new byte[numberOfArpeggios][];

			// Read offset list
			ushort[] offsets = new ushort[numberOfArpeggios];

			moduleStream.Seek(arpeggioListOffset, SeekOrigin.Begin);
			moduleStream.ReadArray_B_UINT16s(offsets, 0, numberOfArpeggios);

			for (int i = 0; i < numberOfArpeggios; i++)
			{
				moduleStream.Seek(offsets[i] + startOffset, SeekOrigin.Begin);

				List<byte> arpBytes = new List<byte>();

				for (;;)
				{
					byte arp = moduleStream.Read_UINT8();
					if (moduleStream.EndOfStream)
						return false;

					arpBytes.Add(arp);

					if ((arp & 0x80) != 0)
						break;
				}

				arpeggios[i] = arpBytes.ToArray();
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the envelopes
		/// </summary>
		/********************************************************************/
		private bool LoadEnvelopes(ModuleStream moduleStream, int numberOfEnvelopes)
		{
			if (numberOfEnvelopes == 0)
				return true;

			envelopes = new byte[numberOfEnvelopes][];

			// Read offset list
			ushort[] offsets = new ushort[numberOfEnvelopes];

			moduleStream.Seek(envelopeListOffset, SeekOrigin.Begin);
			moduleStream.ReadArray_B_UINT16s(offsets, 0, numberOfEnvelopes);

			for (int i = 0; i < numberOfEnvelopes; i++)
			{
				moduleStream.Seek(offsets[i] + startOffset - 1, SeekOrigin.Begin);

				List<byte> envBytes = new List<byte>();
				envBytes.Add(moduleStream.Read_UINT8());		// First byte is the speed

				for (;;)
				{
					byte env = moduleStream.Read_UINT8();
					if (moduleStream.EndOfStream)
						return false;

					envBytes.Add(env);

					if ((env & 0x80) != 0)
						break;
				}

				envelopes[i] = envBytes.ToArray();
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the channel volumes
		/// </summary>
		/********************************************************************/
		private bool LoadChannelVolumes(ModuleStream moduleStream, int numberOfEnvelopes)
		{
			if (!oldPlayer)
				return true;

			channelVolumes = new ushort[numberOfChannels];

			moduleStream.Seek(channelVolumeOffset, SeekOrigin.Begin);
			moduleStream.ReadArray_B_UINT16s(channelVolumes, 0, numberOfChannels);

			return !moduleStream.EndOfStream;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the sample information
		/// </summary>
		/********************************************************************/
		private bool LoadSampleInfo(ModuleStream moduleStream)
		{
			samples = new Sample[numberOfSamples];

			if (oldPlayer)
			{
				for (short i = 0; i < numberOfSamples; i++)
				{
					Sample sample = new Sample();

					sample.SampleNumber = i;
					sample.LoopStart = -1;
					sample.Volume = 64;

					samples[i] = sample;
				}
			}
			else
			{
				moduleStream.Seek(sampleInfoOffset, SeekOrigin.Begin);

				for (short i = 0; i < numberOfSamples; i++)
				{
					Sample sample = new Sample();

					sample.SampleNumber = i;

					moduleStream.Seek(4, SeekOrigin.Current);		// Skip pointer to sample data
					sample.LoopStart = moduleStream.Read_B_INT32();
					sample.Length = moduleStream.Read_B_UINT16() * 2U;

					if (moduleStream.EndOfStream)
						return false;

					// Fix for Jaws
					if ((sample.LoopStart != -1) && (sample.LoopStart > 64 * 1024))
						sample.LoopStart = -1;

					if (uses32BitPointers)
					{
						moduleStream.Seek(2, SeekOrigin.Current);		// Padding

						sample.FineTunePeriod = moduleStream.Read_B_UINT16();
						sample.Volume = moduleStream.Read_B_UINT16();
						sample.Transpose = 0;

						if (moduleStream.EndOfStream)
							return false;
					}
					else
					{
						sample.FineTunePeriod = moduleStream.Read_B_UINT16();

						if (!enableEnvelopes)
						{
							sample.Volume = moduleStream.Read_B_UINT16();
							sample.Transpose = moduleStream.Read_INT8();

							if (moduleStream.EndOfStream)
								return false;

							moduleStream.Seek(1, SeekOrigin.Current);		// Padding
						}
						else
						{
							sample.Volume = 64;
							sample.Transpose = 0;
						}
					}

					samples[i] = sample;
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
			moduleStream.Seek(sampleDataOffset, SeekOrigin.Begin);

			for (int i = 0; i < numberOfSamples; i++)
			{
				Sample sample = samples[i];

				sample.Length = moduleStream.Read_B_UINT32();

				ushort frequency = moduleStream.Read_B_UINT16();
				sample.FineTunePeriod = (ushort)(3579545 / frequency);

				sample.SampleData = moduleStream.ReadSampleData(i + 1, (int)sample.Length, out int readBytes);
				if (readBytes != sample.Length)
					return false;
			}

			if (enableSquareWaveform)
				samples[squareWaveformSampleNumber].Length = squareWaveformSampleLength;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound(int subSong)
		{
			SongInfo song = songInfoList[subSong];

			playingInfo = new GlobalPlayingInfo
			{
				Transpose = 0,
				Speed = song.Speed,
				VolumeFadeSpeed = 0,
				GlobalVolume = 64,
				GlobalVolumeFadeSpeed = 0,
				ExtraCounter = 1,
				DelayCounter = 0,
				DelayCounterSpeed = song.DelayCounterSpeed
			};

			if (enableDelayMultiply)
				playingInfo.DelayCounterSpeed *= 16;

			channels = new ChannelInfo[numberOfChannels];
			InitializeChannelInfo(subSong);

			InitializeSquareWaveform();
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
			trackNumbers = null;
			arpeggios = null;
			envelopes = null;
			samples = null;

			playingInfo = null;
			channels = null;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new empty channel info object
		/// </summary>
		/********************************************************************/
		private void InitializeChannelInfo(int subSong)
		{
			SongInfo songInfo = songInfoList[subSong];

			for (int i = 0; i < numberOfChannels; i++)
			{
				ChannelInfo channelInfo = new ChannelInfo
				{
					ChannelNumber = i,

					SpeedCounter = 1,
					SlideEnabled = false,
					VibratoDirection = 0,
					Transpose = 0,
					EnableHalfVolume = false,
					EnvelopeList = null,
					PositionList = songInfo.PositionLists[i].TrackOffsets,
					CurrentPosition = 1,
					RestartPosition = songInfo.PositionLists[i].RestartPosition,
				};

				if (enableArpeggio)
				{
					channelInfo.ArpeggioList = arpeggios[0];
					channelInfo.ArpeggioListPosition = 0;
				}

				channelInfo.TrackData = channelInfo.PositionList.Length == 0 ? Tables.EmptyTrack : tracks[channelInfo.PositionList[0]];
				channelInfo.TrackDataPosition = 0;

				channels[i] = channelInfo;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the square waveform to its start value
		/// </summary>
		/********************************************************************/
		private void InitializeSquareWaveform()
		{
			if (enableSquareWaveform)
			{
				Sample sample = samples[squareWaveformSampleNumber];
				int halfLength = (int)sample.Length / 2;

				for (int i = 0; i < halfLength; i++)
				{
					sample.SampleData[i] = squareByte1;
					sample.SampleData[i + halfLength] = squareByte2;
				}

				playingInfo.SquareChangePosition = squareChangeMinPosition;
				playingInfo.SquareChangeDirection = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Update the square waveform by expand/minimize the upper and lower
		/// squares
		/// </summary>
		/********************************************************************/
		private void ChangeSquareWaveform()
		{
			if (enableSquareWaveform)
			{
				sbyte[] squareWaveform = samples[squareWaveformSampleNumber].SampleData;

				if (playingInfo.SquareChangeDirection)
				{
					for (int i = 0; i < squareChangeSpeed; i++)
						squareWaveform[playingInfo.SquareChangePosition + i] = squareByte2;

					playingInfo.SquareChangePosition -= squareChangeSpeed;

					if (playingInfo.SquareChangePosition == squareChangeMinPosition)
						playingInfo.SquareChangeDirection = false;
				}
				else
				{
					for (int i = 0; i < squareChangeSpeed; i++)
						squareWaveform[playingInfo.SquareChangePosition + i] = squareByte1;

					playingInfo.SquareChangePosition += squareChangeSpeed;

					if (playingInfo.SquareChangePosition == squareChangeMaxPosition)
						playingInfo.SquareChangeDirection = true;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Read the next track command for a single voice
		/// </summary>
		/********************************************************************/
		private void ReadTrackCommands(ChannelInfo channelInfo, IChannel channel)
		{
			channelInfo.SlideEnabled = false;

			for (;;)
			{
				byte trackByte = channelInfo.TrackData[channelInfo.TrackDataPosition++];

				if ((trackByte & 0x80) != 0)
				{
					if (DoTrackCommand(channelInfo, channel, trackByte))
						break;
				}
				else
				{
					// Play the note
					channelInfo.Note = trackByte;

					if (oldPlayer)
					{
						int sampleNumber = trackByte / 12;
						int note = trackByte % 12;

						Sample sample = samples[sampleNumber];

						if (channelInfo.Note != 0)
						{
							channel.SetAmigaPeriod(periods[note]);
							channel.PlaySample(sample.SampleNumber, sample.SampleData, 0, sample.Length);
							channel.SetAmigaVolume(channelVolumes[channelInfo.ChannelNumber]);
						}
						else
							channel.Mute();
					}
					else
					{
						if (enableSampleTranspose)
							trackByte = (byte)(trackByte + channelInfo.CurrentSampleInfo.Transpose);

						if (enableChannelTranspose)
							trackByte = (byte)(trackByte + channelInfo.Transpose);
						else
						{
							// Old players store the note after it has been transposed in the note field
							channelInfo.Note = trackByte;
						}

						Sample sample = channelInfo.CurrentSampleInfo;

						channel.PlaySample(sample.SampleNumber, sample.SampleData, 0, sample.Length);

						if (sample.LoopStart >= 0)
							channel.SetLoop((uint)sample.LoopStart, sample.Length - (uint)sample.LoopStart);

						int newVolume = sample.Volume;

						if (enableEnvelopes && (channelInfo.EnvelopeList != null))
						{
							newVolume = channelInfo.EnvelopeList[1] & 0x7f;
							channelInfo.EnvelopeListPosition = channelInfo.EnvelopeList.Length > 2 ? 2 : 1;

							channelInfo.EnvelopeCounter = (sbyte)channelInfo.EnvelopeSpeed;
						}

						if (enableHalfVolume && channelInfo.EnableHalfVolume)
							newVolume /= 2;

						if (enableVolumeFade)
						{
							newVolume -= playingInfo.VolumeFadeSpeed;
							if (newVolume < 0)
								newVolume = 0;
						}

						newVolume = newVolume * playingInfo.GlobalVolume / 64;
						channel.SetAmigaVolume((ushort)newVolume);

						if (trackByte >= 128)
							trackByte = 0;
						else if (trackByte >= periods.Length)
							trackByte = (byte)(periods.Length - 1);

						uint period = (uint)((periods[trackByte] * sample.FineTunePeriod) >> 10);
						channel.SetAmigaPeriod(period);
					}
					break;
				}
			}

			channelInfo.SpeedCounter = channelInfo.Speed;
		}



		/********************************************************************/
		/// <summary>
		/// Parse and execute the given track command
		/// </summary>
		/********************************************************************/
		private bool DoTrackCommand(ChannelInfo channelInfo, IChannel channel, byte trackCommand)
		{
			if (trackCommand >= 0xe0)
			{
				// Set number of rows to wait including the current one
				channelInfo.Speed = (ushort)((trackCommand - 0xdf) * playingInfo.Speed);
			}
			else if (!oldPlayer && (trackCommand >= newSampleCmd))
			{
				// Set sample to use
				channelInfo.CurrentSampleInfo = samples[trackCommand - newSampleCmd];
			}
			else if (enableEnvelopes && (trackCommand >= newEnvelopeCmd))
			{
				// Use envelope
				channelInfo.EnvelopeList = envelopes[trackCommand - newEnvelopeCmd];
				channelInfo.EnvelopeListPosition = 1;
				channelInfo.EnvelopeSpeed = channelInfo.EnvelopeList[0];
			}
			else if (enableArpeggio && (trackCommand >= newArpeggioCmd))
			{
				// Use arpeggio
				channelInfo.ArpeggioList = arpeggios[trackCommand - newArpeggioCmd];
				channelInfo.ArpeggioListPosition = 0;
			}
			else
			{
				// Do effects
				return DoEffects(channelInfo, channel, (Effect)(trackCommand & 0x7f));
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Parse and execute effects
		/// </summary>
		/********************************************************************/
		private bool DoEffects(ChannelInfo channelInfo, IChannel channel, Effect effect)
		{
			if (oldPlayer)
			{
				// For the old player (QBall), the effect is in another order
				switch ((int)effect)
				{
					case 0:		// EndOfTrack
					{
						HandleEndOfTrackEffect(channelInfo);
						break;
					}

					case 1:		// StopSong
					{
						StopModule();
						return true;
					}

					case 2:		// ???
						break;
				}
			}
			else
			{
				switch (effect)
				{
					case Effect.EndOfTrack:
					{
						HandleEndOfTrackEffect(channelInfo);
						break;
					}

					case Effect.Slide:
					{
						channelInfo.SlideValue = 0;
						channelInfo.SlideSpeed = (sbyte)channelInfo.TrackData[channelInfo.TrackDataPosition++];
						channelInfo.SlideCounter = channelInfo.TrackData[channelInfo.TrackDataPosition++];
						channelInfo.SlideEnabled = true;
						break;
					}

					case Effect.Mute:
					{
						channel.Mute();
						return true;
					}

					case Effect.WaitUntilNextRow:
						return true;

					case Effect.StopSong:
					{
						StopModule();
						return true;
					}

					case Effect.GlobalTranspose:
					{
						if (enableGlobalTranspose)
							playingInfo.Transpose = (sbyte)channelInfo.TrackData[channelInfo.TrackDataPosition++];

						break;
					}

					case Effect.StartVibrato:
					{
						if (enableVibrato)
						{
							channelInfo.VibratoDirection = -1;
							channelInfo.VibratoSpeed = channelInfo.TrackData[channelInfo.TrackDataPosition++];
							channelInfo.VibratoMaxValue = channelInfo.TrackData[channelInfo.TrackDataPosition++];
							channelInfo.VibratoValue = 0;
						}
						break;
					}

					case Effect.StopVibrato:
					{
						if (enableVibrato)
							channelInfo.VibratoDirection = 0;

						break;
					}

					case Effect.Effect8:
					{
						if (enableVolumeFade)
							playingInfo.VolumeFadeSpeed = channelInfo.TrackData[channelInfo.TrackDataPosition++];
						else if (enableChannelTranspose)
							channelInfo.Transpose = (sbyte)channelInfo.TrackData[channelInfo.TrackDataPosition++];
						else if (enableHalfVolume)
							channelInfo.EnableHalfVolume = true;

						break;
					}

					case Effect.Effect9:
					{
						if (enableHalfVolume)
							channelInfo.EnableHalfVolume = false;
						else
						{
							// Position restart is handled in the loader
							channelInfo.TrackDataPosition += 2;
						}
						break;
					}

					case Effect.SetSpeed:
					{
						if (enableDelaySpeed)
							playingInfo.DelayCounterSpeed = channelInfo.TrackData[channelInfo.TrackDataPosition++];
						else
						{
							playingInfo.Speed = channelInfo.TrackData[channelInfo.TrackDataPosition++];
							ShowSpeed();
						}
						break;
					}

					case Effect.GlobalVolumeFade:
					{
						playingInfo.GlobalVolumeFadeSpeed = channelInfo.TrackData[channelInfo.TrackDataPosition++];
						playingInfo.GlobalVolumeFadeCounter = playingInfo.GlobalVolumeFadeSpeed;
						break;
					}

					case Effect.SetGlobalVolume:
					{
						if (enableSetGlobalVolume)
							playingInfo.GlobalVolume = channelInfo.TrackData[channelInfo.TrackDataPosition++];
						else
						{
							// Start sound effect. This is used in Emlyn Hughes International Soccer to play a whistle sound,
							// but we do not support it
							channelInfo.TrackDataPosition++;
						}
						break;
					}

					case Effect.StartOrStopSoundFx:
					{
						// If effect C if global volume, this is start sound effect. If not, it is stop sound effect
						if (enableSetGlobalVolume)
							channelInfo.TrackDataPosition++;

						break;
					}

					case Effect.StopSoundFx:
						break;
				}
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Start over a track
		/// </summary>
		/********************************************************************/
		private void HandleEndOfTrackEffect(ChannelInfo channelInfo)
		{
			// Move to next position
			if (channelInfo.CurrentPosition >= channelInfo.PositionList.Length)
			{
				channelInfo.CurrentPosition = (ushort)(channelInfo.RestartPosition + 1);
				OnEndReached(channelInfo.ChannelNumber);

				if (HasEndReached)
				{
					playingInfo.Transpose = 0;
					playingInfo.VolumeFadeSpeed = 0;
					playingInfo.GlobalVolumeFadeSpeed = 0;

					if (playingInfo.GlobalVolume == 0)
						playingInfo.GlobalVolume = 64;
				}
			}
			else
			{
				if (channelInfo.CurrentPosition == channelInfo.RestartPosition)
					SetRestartTime();

				channelInfo.CurrentPosition++;
			}

			channelInfo.TrackData = channelInfo.PositionList.Length == 0 ? Tables.EmptyTrack : tracks[channelInfo.PositionList[channelInfo.CurrentPosition - 1]];
			channelInfo.TrackDataPosition = 0;

			ShowChannelPositions();
			ShowTracks();
		}



		/********************************************************************/
		/// <summary>
		/// Run effects for every tick
		/// </summary>
		/********************************************************************/
		private void DoFrameStuff(ChannelInfo channelInfo, IChannel channel)
		{
			if (channelInfo.CurrentSampleInfo != null)
			{
				sbyte note = (sbyte)channelInfo.Note;

				if (enableGlobalTranspose)
					note += playingInfo.Transpose;

				if (enableChannelTranspose)
					note += channelInfo.Transpose;

				if (enableArpeggio)
				{
					// Do arpeggio
					byte arp = channelInfo.ArpeggioList[channelInfo.ArpeggioListPosition++];

					if ((arp & 0x80) != 0)
					{
						channelInfo.ArpeggioListPosition = 0;
						arp &= 0x7f;
					}

					note = (sbyte)(note + arp);
					if (note < 0)
						note = 0;
					else if (note >= periods.Length)
						note = (sbyte)(periods.Length - 1);
				}

				uint period = (uint)((periods[note] * channelInfo.CurrentSampleInfo.FineTunePeriod) >> 10);

				// Do slide
				if (channelInfo.SlideEnabled)
				{
					if (channelInfo.SlideCounter == 0)
					{
						channelInfo.SlideValue += channelInfo.SlideSpeed;
						period = (uint)(period - channelInfo.SlideValue);
					}
					else
						channelInfo.SlideCounter--;
				}

				if (enableVibrato)
				{
					// Do vibrato
					if (channelInfo.VibratoDirection != 0)
					{
						if (channelInfo.VibratoDirection < 0)
						{
							channelInfo.VibratoValue += channelInfo.VibratoSpeed;
							if (channelInfo.VibratoValue == channelInfo.VibratoMaxValue)
								channelInfo.VibratoDirection = (sbyte)-channelInfo.VibratoDirection;
						}
						else
						{
							channelInfo.VibratoValue -= channelInfo.VibratoSpeed;
							if (channelInfo.VibratoValue == 0)
								channelInfo.VibratoDirection = (sbyte)-channelInfo.VibratoDirection;
						}

						if (channelInfo.VibratoValue == 0)
							channelInfo.VibratoDirection ^= 0x01;

						if ((channelInfo.VibratoDirection & 0x01) != 0)
							period += channelInfo.VibratoValue;
						else
							period -= channelInfo.VibratoValue;
					}
				}

				channel.SetAmigaPeriod(period);

				if (enableEnvelopes && (channelInfo.EnvelopeList != null))
				{
					channelInfo.EnvelopeCounter--;
					if (channelInfo.EnvelopeCounter < 0)
					{
						channelInfo.EnvelopeCounter = (sbyte)channelInfo.EnvelopeSpeed;

						int newVolume = channelInfo.EnvelopeList[channelInfo.EnvelopeListPosition];
						if ((newVolume & 0x80) == 0)
							channelInfo.EnvelopeListPosition++;

						newVolume &= 0x7f;

						if (enableHalfVolume && channelInfo.EnableHalfVolume)
							newVolume /= 2;

						if (enableVolumeFade)
						{
							newVolume -= playingInfo.VolumeFadeSpeed;
							if (newVolume < 0)
								newVolume = 0;
						}

						newVolume = newVolume * playingInfo.GlobalVolume / 64;
						channel.SetAmigaVolume((ushort)newVolume);
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Stop and restart the module
		/// </summary>
		/********************************************************************/
		private void StopModule()
		{
			playingInfo.Transpose = 0;
			playingInfo.VolumeFadeSpeed = 0;
			playingInfo.GlobalVolume = 64;
			playingInfo.GlobalVolumeFadeSpeed = 0;

			for (int i = 0; i < numberOfChannels; i++)
				VirtualChannels[i].Mute();

			InitializeChannelInfo(currentSong);
			InitializeSquareWaveform();

			// Tell NostalgicPlayer that the song has ended
			OnEndReached();
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
			OnModuleInfoChanged(InfoSpeedLine, playingInfo.Speed.ToString());
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
				sb.Append(channels[i].PositionList.Length);
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
				sb.Append(channels[i].CurrentPosition - 1);
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
				sb.Append(trackNumbers[channels[i].PositionList[channels[i].CurrentPosition - 1]]);
				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}
		#endregion
	}
}
