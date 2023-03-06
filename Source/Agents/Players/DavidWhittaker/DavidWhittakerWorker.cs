using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Polycode.NostalgicPlayer.Agent.Player.DavidWhittaker.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.DavidWhittaker
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class DavidWhittakerWorker : ModulePlayerAgentBase
	{
		// Information for the loader
		private bool uses32BitPointers;

		private int sampleInfoOffset;
		private int sampleDataOffset;
		private int subSongListOffset;
		private int arpeggioListOffset;
		private int envelopeListOffset;

		private int numberOfSamples;
		private int numberOfChannels;

		private List<SongInfo> songInfoList;
		private Dictionary<uint, uint[]> positionLists;
		private Dictionary<uint, byte[]> tracks;
		private byte[][] arpeggios;
		private byte[][] envelopes;
		private Sample[] samples;

		private bool enableSampleTranspose;
		private bool enableChannelTranspose;
		private bool enableGlobalTranspose;
		private sbyte transpose;

		private byte newSampleCmd;
		private byte newEnvelopeCmd;
		private byte newArpeggioCmd;

		private bool enableArpeggio;
		private bool enableEnvelopes;
		private bool enableVibrato;

		private bool enableVolumeFade;
		private ushort volumeFadeSpeed;
		private bool enableHalfVolume;

		private bool enableGlobalVolumeFade;
		private ushort globalVolume;
		private byte globalVolumeFadeSpeed;
		private byte globalVolumeFadeCounter;

		private bool enableSquareWaveform;
		private int squareWaveformSampleNumber;
		private ushort squareChangePosition;
		private ushort squareChangeMinPosition;
		private ushort squareChangeMaxPosition;
		private byte squareChangeSpeed;
		private bool squareChangeDirection;
		private sbyte squareByte1;
		private sbyte squareByte2;

		private bool useExtraCounter;
		private byte extraCounter;

		private bool enableDelayCounter;
		private bool enableDelayMultiply;
		private byte delayCounterSpeed;
		private ushort delayCounter;

		private ushort[] periods;
		private ChannelInfo[] channels;

		private ushort speed;

		private int currentSong;
		private PositionLength[] positionLengths;

		private const int InfoSpeedLine = 3;

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
			if (moduleStream.Length < 4096)
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
				// Song length
				case 0:
				{
					description = Resources.IDS_DW_INFODESCLINE0;
					value = positionLengths[currentSong].Length.ToString();
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

				// Current speed
				case 3:
				{
					description = Resources.IDS_DW_INFODESCLINE3;
					value = speed.ToString();
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
		public override ModulePlayerSupportFlag SupportFlags => ModulePlayerSupportFlag.SetPosition;



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

				if (!LoadSubSongInfo(moduleStream, out HashSet<uint> trackOffsets))
				{
					errorMessage = Resources.IDS_DW_ERR_LOADING_SUBSONG;
					return AgentResult.Error;
				}

				if (!LoadTracks(moduleStream, trackOffsets, out int numberOfArpeggios, out int numberOfEnvelopes))
				{
					errorMessage = Resources.IDS_DW_ERR_LOADING_TRACKS;
					return AgentResult.Error;
				}

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
		public override bool InitSound(int songNumber, DurationInfo durationInfo, out string errorMessage)
		{
			if (!base.InitSound(songNumber, durationInfo, out errorMessage))
				return false;

			InitializeSound(songNumber);

			// Remember the song number
			currentSong = songNumber;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the duration for all sub-songs
		/// </summary>
		/********************************************************************/
		public override DurationInfo[] CalculateDuration()
		{
			// Find the longest (in time) channel and create
			// position structures. Do it for each sub-song
			int numberOfSubSongs = songInfoList.Count;

			DurationInfo[] result = new DurationInfo[numberOfSubSongs];
			positionLengths = new PositionLength[numberOfSubSongs];

			for (ushort i = 0; i < numberOfSubSongs; i++)
				result[i] = FindLongestChannel(i);

			allDurations = result;

			return result;
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
				delayCounter += delayCounterSpeed;
				if (delayCounter > 255)
				{
					delayCounter -= 256;
					return;
				}
			}

			if (useExtraCounter)
			{
				extraCounter--;
				if (extraCounter == 0)
				{
					extraCounter = 6;
					return;
				}
			}

			if (enableGlobalVolumeFade)
			{
				if (globalVolumeFadeSpeed != 0)
				{
					if (globalVolume > 0)
					{
						globalVolumeFadeCounter--;
						if (globalVolumeFadeCounter == 0)
						{
							globalVolume--;
							if (globalVolume > 0)
								globalVolumeFadeCounter = globalVolumeFadeSpeed;
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
		/// Return the length of the current song
		/// </summary>
		/********************************************************************/
		public override int SongLength => positionLengths[currentSong].Length;



		/********************************************************************/
		/// <summary>
		/// Return the current position of the song
		/// </summary>
		/********************************************************************/
		public override int GetSongPosition()
		{
			return channels[positionLengths[currentSong].Channel].CurrentPosition - 1;
		}



		/********************************************************************/
		/// <summary>
		/// Set a new position of the song
		/// </summary>
		/********************************************************************/
		public override void SetSongPosition(int position, PositionInfo positionInfo)
		{
			// Change the position
			ExtraPositionInfo[] extraInfo = (ExtraPositionInfo[])positionInfo.ExtraInfo;

			for (int i = 0; i < numberOfChannels; i++)
			{
				channels[i].CurrentPosition = extraInfo[i].Position;
				channels[i].TrackData = tracks[channels[i].PositionList[channels[i].CurrentPosition - 1]];
				channels[i].TrackDataPosition = extraInfo[i].TrackPosition;
				channels[i].SpeedCounter = extraInfo[i].SpeedCounter;
			}

			base.SetSongPosition(position, positionInfo);
		}



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

					for (int j = 0; j < Tables.Periods2.Length; j++)
					{
						uint period = (uint)((Tables.Periods2[j] * sample.FineTunePeriod) >> 10);
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

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Test the file to see if its a David Whittaker player and extract
		/// needed information
		/// </summary>
		/********************************************************************/
		private AgentResult TestModule(byte[] buffer)
		{
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
				if ((searchBuffer[index] == 0x47) && (searchBuffer[index + 1] == 0xfa))
					break;
			}

			if (index >= (searchLength - 6))
				return false;

			int offset = (((sbyte)searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;
			if (offset < 0)
				return false;

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

			if (index >= (searchLength - 18))
				return false;

			if (searchBuffer[index + 4] != 0x66)
				return false;

			if (searchBuffer[index + 5] == 0x00)
				index += 2;

			if ((searchBuffer[index + 6] != 0x41) || (searchBuffer[index + 7] != 0xfa) || (searchBuffer[index + 10] != 0x4b) || (searchBuffer[index + 11] != 0xfa) || (searchBuffer[index + 14] != 0x72))
				return false;

			numberOfSamples = (searchBuffer[index + 15] & 0x00ff) + 1;

			sampleInfoOffset = (((sbyte)searchBuffer[index + 12] << 8) | searchBuffer[index + 13]) + index + 12;
			sampleDataOffset = (((sbyte)searchBuffer[index + 8] << 8) | searchBuffer[index + 9]) + index + 8;

			//
			// Extract sub-song information
			//
			for (index = startOfInit; index < searchLength; index += 2)
			{
				if ((searchBuffer[index] == 0xc0) && (searchBuffer[index + 1] == 0xfc))
					break;
			}

			if (index >= (searchLength - 8))
				return false;

			if ((searchBuffer[index + 4] != 0x41) || (searchBuffer[index + 5] != 0xfa))
				return false;

			subSongListOffset = (((sbyte)searchBuffer[index + 6] << 8) | searchBuffer[index + 7]) + index + 6;
			index += 8;

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
					offset = (searchBuffer[index - 4] << 8) | searchBuffer[index - 3];
					useExtraCounter = searchBuffer[offset] != 0;
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
			for (index = startOfPlay; index < searchLength; index += 2)
			{
				if ((searchBuffer[index] == 0x53) && (searchBuffer[index + 1] == 0x68))
					break;
			}

			if (index >= (searchLength - 16))
				return false;

			if (searchBuffer[index + 4] != 0x67)
				return false;

			int readTrackCommandsOffset = searchBuffer[index + 5] + index + 6;

			if (searchBuffer[index + 12] != 0x66)
				return false;

			int doFrameStuffOffset;

			if (searchBuffer[index + 13] == 0x00)
				doFrameStuffOffset = ((searchBuffer[index + 14] << 8) | searchBuffer[index + 15]) + index + 14;
			else
				doFrameStuffOffset = searchBuffer[index + 13] + index + 14;

			for (index = readTrackCommandsOffset; index < searchLength; index += 2)
			{
				if ((searchBuffer[index] == 0x6b) && (searchBuffer[index + 1] == 0x00))
					break;
			}

			int doCommandsOffset = ((searchBuffer[index + 2] << 8) | searchBuffer[index + 3]) + index + 2;

			//
			// Find period table to check which version to use
			//
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
				periods = Tables.Periods1;
			else if ((searchBuffer[offset] == 0x20) && (searchBuffer[offset + 1] == 0x00))
				periods = Tables.Periods2;
			else
				return false;

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
			if ((searchBuffer[doFrameStuffOffset] == 0x10) && (searchBuffer[doFrameStuffOffset + 1] == 0x28))
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

			if (newSampleCmd == 0)
				return false;

			//
			// Check different effects
			//
			int jumpTableOffset;

			if ((searchBuffer[index - 10] == 0x45) && (searchBuffer[index - 9] == 0xfa))
				jumpTableOffset = (((sbyte)searchBuffer[index - 8] << 8) | searchBuffer[index - 7]) + index - 8;
			else if ((searchBuffer[index - 8] == 0x45) && (searchBuffer[index - 7] == 0xfa))
				jumpTableOffset = (((sbyte)searchBuffer[index - 6] << 8) | searchBuffer[index - 5]) + index - 6;
			else
				return false;

			enableVibrato = false;

			int effectOffset = (searchBuffer[jumpTableOffset + 6 * 2] << 8) | searchBuffer[jumpTableOffset + 6 * 2 + 1];
			if ((effectOffset < (searchLength - 6)) && (searchBuffer[effectOffset] == 0x50) && (searchBuffer[effectOffset + 1] == 0xe8) && (searchBuffer[effectOffset + 4] == 0x11) && (searchBuffer[effectOffset + 5] == 0x59))
				enableVibrato = true;

			enableVolumeFade = false;

			effectOffset = (searchBuffer[jumpTableOffset + 8 * 2] << 8) | searchBuffer[jumpTableOffset + 8 * 2 + 1];
			if ((effectOffset < (searchLength - 2)) && (searchBuffer[effectOffset] == 0x17) && (searchBuffer[effectOffset + 1] == 0x59))
				enableVolumeFade = true;

			enableHalfVolume = false;

			effectOffset = (searchBuffer[jumpTableOffset + 8 * 2] << 8) | searchBuffer[jumpTableOffset + 8 * 2 + 1];
			if ((effectOffset < (searchLength - 2)) && (searchBuffer[effectOffset] == 0x50) && (searchBuffer[effectOffset + 1] == 0xe8))
			{
				effectOffset = (searchBuffer[jumpTableOffset + 9 * 2] << 8) | searchBuffer[jumpTableOffset + 9 * 2 + 1];
				if ((effectOffset < (searchLength - 2)) && (searchBuffer[effectOffset] == 0x51) && (searchBuffer[effectOffset + 1] == 0xe8))
					enableHalfVolume = true;
			}

			enableGlobalVolumeFade = false;

			effectOffset = (searchBuffer[jumpTableOffset + 11 * 2] << 8) | searchBuffer[jumpTableOffset + 11 * 2 + 1];
			if ((effectOffset < (searchLength - 2)) && (searchBuffer[effectOffset] == 0x17) && (searchBuffer[effectOffset + 1] == 0x59))
				enableGlobalVolumeFade = true;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load the sub-song information for all sub-songs
		/// </summary>
		/********************************************************************/
		private bool LoadSubSongInfo(ModuleStream moduleStream, out HashSet<uint> trackOffsets)
		{
			songInfoList = new List<SongInfo>();

			positionLists = new Dictionary<uint, uint[]>();
			trackOffsets = new HashSet<uint>();

			moduleStream.Seek(subSongListOffset, SeekOrigin.Begin);

			long minPositionOffset = long.MaxValue;

			for (;;)
			{
				ushort songSpeed;
				byte delaySpeed;

				if (moduleStream.Position == minPositionOffset)
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

				SongInfo info = new SongInfo
				{
					Speed = songSpeed,
					DelayCounterSpeed = delaySpeed
				};

				uint[] positionOffsets = new uint[numberOfChannels];

				if (uses32BitPointers)
				{
					for (int i = 0; i < numberOfChannels; i++)
					{
						positionOffsets[i] = moduleStream.Read_B_UINT32();
						minPositionOffset = Math.Min(minPositionOffset, positionOffsets[i]);
					}
				}
				else
				{
					for (int i = 0; i < numberOfChannels; i++)
					{
						positionOffsets[i] = moduleStream.Read_B_UINT16();
						minPositionOffset = Math.Min(minPositionOffset, positionOffsets[i]);
					}
				}

				if (moduleStream.EndOfStream)
					return false;

				long currentPosition = moduleStream.Position;

				info.PositionLists = positionOffsets;

				for (int i = 0; i < numberOfChannels; i++)
				{
					uint[] list = LoadPositionList(moduleStream, positionOffsets[i], trackOffsets);
					if (list == null)
						return false;

					positionLists[positionOffsets[i]] = list;
				}

				songInfoList.Add(info);

				moduleStream.Position = currentPosition;
			}

			return songInfoList.Count > 0;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single position list and initialize the track offset list
		/// </summary>
		/********************************************************************/
		private uint[] LoadPositionList(ModuleStream moduleStream, uint startPosition, HashSet<uint> trackOffsets)
		{
			if (startPosition == 0)
				return Array.Empty<uint>();

			moduleStream.Seek(startPosition, SeekOrigin.Begin);

			List<uint> positionList = new List<uint>();

			for (;;)
			{
				uint trackOffset = uses32BitPointers ? moduleStream.Read_B_UINT32() : moduleStream.Read_B_UINT16();
				if ((trackOffset == 0) || (trackOffset >= moduleStream.Length) || ((trackOffset & 0x8000) != 0))
					break;

				if (moduleStream.EndOfStream)
					return null;

				trackOffsets.Add(trackOffset);
				positionList.Add(trackOffset);
			}

			return positionList.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Load all the tracks
		/// </summary>
		/********************************************************************/
		private bool LoadTracks(ModuleStream moduleStream, HashSet<uint> trackOffsets, out int numberOfArpeggios, out int numberOfEnvelopes)
		{
			numberOfArpeggios = 0;
			numberOfEnvelopes = 0;

			tracks = new Dictionary<uint, byte[]>();

			foreach (uint offset in trackOffsets)
			{
				moduleStream.Seek(offset, SeekOrigin.Begin);
				List<byte> trackBytes = new List<byte>();

				// Read the track and parse it to find the end + count number of arpeggios
				for (;;)
				{
					byte byt = moduleStream.Read_UINT8();
					if (moduleStream.EndOfStream)
						return false;

					trackBytes.Add(byt);

					if ((byt & 0x80) != 0)
					{
						if (byt >= 0xe0)
							continue;

						if (byt >= newSampleCmd)
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

						if (byt >= 0x80)
						{
							if ((byt == 0x80) || (byt == 0x84))
								break;

							if ((byt == 0x82) || (byt == 0x83) || (byt == 0x87))
								continue;

							if ((byt == 0x85) || (byt == 0x88) || (byt == 0x8a) || (byt == 0x8b) || (byt == 0x8c))
							{
								trackBytes.Add(moduleStream.Read_UINT8());
								continue;
							}

							if ((byt == 0x81) || (byt == 0x86))
							{
								trackBytes.Add(moduleStream.Read_UINT8());
								trackBytes.Add(moduleStream.Read_UINT8());
								continue;
							}

							if (byt == 0x89)
							{
								if (!enableHalfVolume)
								{
									trackBytes.Add(moduleStream.Read_UINT8());
									trackBytes.Add(moduleStream.Read_UINT8());
									break;
								}
								continue;
							}

							throw new Exception($"Invalid track byte detected ({byt})");
						}
					}
				}

				tracks[offset] = trackBytes.ToArray();
			}

			return true;
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
				moduleStream.Seek(offsets[i], SeekOrigin.Begin);

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
				moduleStream.Seek(offsets[i] - 1, SeekOrigin.Begin);

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
		/// Load all the sample information
		/// </summary>
		/********************************************************************/
		private bool LoadSampleInfo(ModuleStream moduleStream)
		{
			samples = new Sample[numberOfSamples];

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

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Scans all the channels to find the channel which take longest
		/// time to play
		/// </summary>
		/********************************************************************/
		private DurationInfo FindLongestChannel(ushort songNum)
		{
			InitializeSound(songNum);

			// Now calculate the time for each channel
			float[] channelTimes = new float[numberOfChannels];
			bool[] doneFlags = new bool[numberOfChannels];
			bool[] hasNote = new bool[numberOfChannels];
			ushort[] positionBeforeRestart = new ushort[numberOfChannels];

			List<PositionInfo>[] positionTimes = Helpers.InitializeArray<List<PositionInfo>>(numberOfChannels);
			ExtraPositionInfo[] extraInfo;

			for (int i = 0; i < numberOfChannels; i++)
			{
				extraInfo = new ExtraPositionInfo[numberOfChannels];

				for (int j = 0; j < numberOfChannels; j++)
					extraInfo[j] = new ExtraPositionInfo(channels[j].CurrentPosition, channels[j].TrackDataPosition, channels[j].SpeedCounter);

				positionTimes[i].Add(new PositionInfo((byte)speed, 125, new TimeSpan(0), songNum, extraInfo));
			}

			for (;;)
			{
				// Check to see if all channels has been parsed
				bool allDone = true;

				for (int i = 0; i < numberOfChannels; i++)
				{
					if (!doneFlags[i])
					{
						allDone = false;
						break;
					}
				}

				if (allDone)
					break;

				extraInfo = new ExtraPositionInfo[numberOfChannels];

				for (int i = 0; i < numberOfChannels; i++)
					extraInfo[i] = new ExtraPositionInfo(channels[i].CurrentPosition, channels[i].TrackDataPosition, channels[i].SpeedCounter);

				if (enableDelayCounter)
				{
					delayCounter += delayCounterSpeed;
					if (delayCounter > 255)
					{
						delayCounter -= 256;

						for (int i = 0; i < numberOfChannels; i++)
							channelTimes[i] += 1000.0f / 50.0f;

						continue;
					}
				}

				if (useExtraCounter)
				{
					extraCounter--;
					if (extraCounter == 0)
					{
						extraCounter = 6;

						for (int i = 0; i < numberOfChannels; i++)
							channelTimes[i] += 1000.0f / 50.0f;

						continue;
					}
				}

				for (int i = 0; i < numberOfChannels; i++)
				{
					ChannelInfo channelInfo = channels[i];
					bool doneFlag = false;

					channelInfo.SpeedCounter--;
					if (channelInfo.SpeedCounter == 0)
					{
						bool channelDone = false;

						do
						{
							bool changePosition = false;

							byte trackByte = channelInfo.TrackData[channelInfo.TrackDataPosition++];

							if ((trackByte & 0x80) != 0)
							{
								if (trackByte >= 0xe0)
								{
									// Set number of rows to wait including the current one
									channelInfo.Speed = (ushort)((trackByte - 0xdf) * speed);
								}
								else if (trackByte < 0x90)
								{
									// Do effects
									switch ((Effect)(trackByte & 0x1f))
									{
										case Effect.EndOfTrack:
										{
											// Move to next position
											if (channelInfo.CurrentPosition >= channelInfo.PositionList.Length)
											{
												positionBeforeRestart[i] = channelInfo.CurrentPosition;
												channelInfo.CurrentPosition = 1;

												channelDone = true;
												doneFlag = true;
											}
											else
												channelInfo.CurrentPosition++;

											channelInfo.TrackData = channelInfo.PositionList.Length == 0 ? Tables.EmptyTrack : tracks[channelInfo.PositionList[channelInfo.CurrentPosition - 1]];
											channelInfo.TrackDataPosition = 0;

											changePosition = true;
											break;
										}

										case Effect.Effect8:
										{
											if (enableChannelTranspose)
												channelInfo.TrackDataPosition++;

											break;
										}

										case Effect.Effect9:
										{
											if (!enableHalfVolume)
											{
												channelInfo.TrackDataPosition += 2;
												positionBeforeRestart[i] = channelInfo.CurrentPosition;
												goto case Effect.StopSong;
											}
											break;
										}

										case Effect.Slide:
										case Effect.StartVibrato:
										{
											channelInfo.TrackDataPosition += 2;
											break;
										}

										case Effect.Mute:
										case Effect.WaitUntilNextRow:
										{
											channelDone = true;
											break;
										}

										case Effect.StopSong:
										{
											for (int j = 0; j < numberOfChannels; j++)
											{
												positionBeforeRestart[j] = channels[j].CurrentPosition;
												doneFlags[j] = true;
											}

											channelDone = true;
											doneFlag = true;
											break;
										}

										case Effect.GlobalTranspose:
										case Effect.GlobalVolumeFade:
										case Effect.SetGlobalVolume:
										{
											channelInfo.TrackDataPosition++;
											break;
										}

										case Effect.SetSpeed:
										{
											speed = channelInfo.TrackData[channelInfo.TrackDataPosition++];
											break;
										}
									}
								}
							}
							else
							{
								hasNote[i] = true;
								channelDone = true;
							}

							if (changePosition)
								positionTimes[i].Add(new PositionInfo((byte)speed, 125, new TimeSpan((long)channelTimes[i] * TimeSpan.TicksPerMillisecond), songNum, extraInfo));
						}
						while (!channelDone);

						channelInfo.SpeedCounter = channelInfo.Speed;
					}

					if (!doneFlags[i])
						channelTimes[i] += 1000.0f / 50.0f;

					if (doneFlag)
						doneFlags[i] = true;
				}
			}

			// Find the channel which takes the longest time to play
			float time = 0.0f;

			for (int i = 0; i < numberOfChannels; i++)
			{
				// Skip channels that does not have any notes
				if (hasNote[i])
				{
					if ((channelTimes[i] > time) || ((channelTimes[i] == time) && (positionBeforeRestart[i] > positionLengths[songNum]?.Length)))
					{
						positionLengths[songNum] = new PositionLength
						{
							Channel = i,
							Length = positionBeforeRestart[i]
						};

						time = channelTimes[i];
					}
				}
			}

			// Some songs may have zero length, which means
			// that the PosLength object hasn't been created
			if (positionLengths[songNum] == null)
				positionLengths[songNum] = new PositionLength();

			// Now return a duration object with the position times
			return new DurationInfo(new TimeSpan((long)time * TimeSpan.TicksPerMillisecond), positionTimes[positionLengths[songNum].Channel].ToArray());
		}



		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound(int subSong)
		{
			SongInfo song = songInfoList[subSong];

			transpose = 0;
			speed = song.Speed;
			volumeFadeSpeed = 0;
			globalVolume = 64;
			globalVolumeFadeSpeed = 0;
			extraCounter = 1;
			delayCounter = 0;
			delayCounterSpeed = song.DelayCounterSpeed;
			if (enableDelayMultiply)
				delayCounterSpeed *= 16;

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
			positionLists = null;
			tracks = null;
			arpeggios = null;
			envelopes = null;
			samples = null;

			channels = null;
			positionLengths = null;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new empty channel info object
		/// </summary>
		/********************************************************************/
		private void InitializeChannelInfo(int subSong)
		{
			SongInfo song = songInfoList[subSong];

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
					PositionList = positionLists[song.PositionLists[i]],
					CurrentPosition = 1
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

				squareChangePosition = squareChangeMinPosition;
				squareChangeDirection = false;
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

				if (squareChangeDirection)
				{
					for (int i = 0; i < squareChangeSpeed; i++)
						squareWaveform[squareChangePosition + i] = squareByte2;

					squareChangePosition -= squareChangeSpeed;

					if (squareChangePosition == squareChangeMinPosition)
						squareChangeDirection = false;
				}
				else
				{
					for (int i = 0; i < squareChangeSpeed; i++)
						squareWaveform[squareChangePosition + i] = squareByte1;

					squareChangePosition += squareChangeSpeed;

					if (squareChangePosition == squareChangeMaxPosition)
						squareChangeDirection = true;
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
						newVolume -= volumeFadeSpeed;
						if (newVolume < 0)
							newVolume = 0;
					}

					newVolume = newVolume * globalVolume / 64;
					channel.SetAmigaVolume((ushort)newVolume);

					if (trackByte >= 128)
						trackByte = 0;
					else if (trackByte >= periods.Length)
						trackByte = (byte)(periods.Length - 1);

					uint period = (uint)((periods[trackByte] * sample.FineTunePeriod) >> 10);
					channel.SetAmigaPeriod(period);

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
				channelInfo.Speed = (ushort)((trackCommand - 0xdf) * speed);
			}
			else if (trackCommand >= newSampleCmd)
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
			switch (effect)
			{
				case Effect.EndOfTrack:
				{
					// Move to next position
					if (channelInfo.CurrentPosition >= channelInfo.PositionList.Length)
					{
						channelInfo.CurrentPosition = 1;

						// Tell NostalgicPlayer that the song has ended
						if (channelInfo.ChannelNumber == positionLengths[currentSong].Channel)
							StopModule();
					}
					else
						channelInfo.CurrentPosition++;

					channelInfo.TrackData = channelInfo.PositionList.Length == 0 ? Tables.EmptyTrack : tracks[channelInfo.PositionList[channelInfo.CurrentPosition - 1]];
					channelInfo.TrackDataPosition = 0;

					// Tell NostalgicPlayer we have changed the position
					if (channelInfo.ChannelNumber == positionLengths[currentSong].Channel)
						OnPositionChanged();

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
						transpose = (sbyte)channelInfo.TrackData[channelInfo.TrackDataPosition++];

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
						volumeFadeSpeed = channelInfo.TrackData[channelInfo.TrackDataPosition++];
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
						byte byt1 = channelInfo.TrackData[channelInfo.TrackDataPosition++];
						byte byt2 = channelInfo.TrackData[channelInfo.TrackDataPosition++];

						uint offset = (uint)((byt1 << 8) | byt2);
						ushort newPosition = 1;

						if (!positionLists.TryGetValue(offset, out uint[] positionList))
						{
							positionList = channelInfo.PositionList;
							KeyValuePair<uint, uint[]>? previousPositionList = null;

							foreach (KeyValuePair<uint, uint[]> pair in positionLists.OrderBy(x => x.Key))
							{
								if (offset < pair.Key)
								{
									positionList = previousPositionList!.Value.Value;
									newPosition = (ushort)(((offset - previousPositionList.Value.Key) / 2) + 1);
									break;
								}

								previousPositionList = pair;
							}
						}

						channelInfo.PositionList = positionList;
						channelInfo.CurrentPosition = newPosition;

						channelInfo.TrackData = tracks[channelInfo.PositionList[newPosition - 1]];
						channelInfo.TrackDataPosition = 0;

						// Tell NostalgicPlayer we have changed the position
						if (channelInfo.ChannelNumber == positionLengths[currentSong].Channel)
						{
							transpose = 0;
							volumeFadeSpeed = 0;
							globalVolume = 64;
							globalVolumeFadeSpeed = 0;

							for (int i = songInfoList.Count - 1; i >= 0; i--)
							{
								SongInfo subSong = songInfoList[i];

								if (subSong.PositionLists[channelInfo.ChannelNumber] == offset)
								{
									currentSong = i;
									ChangeSubSong(i);
									break;
								}
							}

							OnPositionChanged();
							OnEndReached();
						}
					}
					break;
				}

				case Effect.SetSpeed:
				{
					speed = channelInfo.TrackData[channelInfo.TrackDataPosition++];

					// Change the module info
					OnModuleInfoChanged(InfoSpeedLine, speed.ToString());
					break;
				}

				case Effect.GlobalVolumeFade:
				{
					globalVolumeFadeSpeed = channelInfo.TrackData[channelInfo.TrackDataPosition++];
					globalVolumeFadeCounter = globalVolumeFadeSpeed;
					break;
				}

				case Effect.SetGlobalVolume:
				{
					globalVolume = channelInfo.TrackData[channelInfo.TrackDataPosition++];
					break;
				}
			}

			return false;
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
					note += transpose;

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
							newVolume -= volumeFadeSpeed;
							if (newVolume < 0)
								newVolume = 0;
						}

						newVolume = newVolume * globalVolume / 64;
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
			transpose = 0;
			volumeFadeSpeed = 0;
			globalVolume = 64;
			globalVolumeFadeSpeed = 0;

			for (int i = 0; i < numberOfChannels; i++)
				VirtualChannels[i].Mute();

			InitializeChannelInfo(currentSong);
			InitializeSquareWaveform();

			// Tell NostalgicPlayer that the song has ended
			OnEndReached();
		}
		#endregion
	}
}
