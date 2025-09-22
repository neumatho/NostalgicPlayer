/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Agent.Player.SoundFactory.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.SoundFactory
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class SoundFactoryWorker : ModulePlayerWithSubSongDurationAgentBase
	{
		private SongInfo[] songInfoList;
		private byte[] opcodes;

		private SortedDictionary<uint, Instrument> originalInstruments;

		private SongInfo currentSongInfo;

		private GlobalPlayingInfo playingInfo;
		private VoiceInfo[] voices;

		private Dictionary<uint, TimeSpan> opcodeTimes;

		#region Identify
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => [ "psf" ];



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 276)
				return AgentResult.Unknown;

			moduleStream.Seek(0, SeekOrigin.Begin);

			uint moduleLength = moduleStream.Read_B_UINT32();
			if (moduleLength > moduleStream.Length)
				return AgentResult.Unknown;

			// Check enabled channels for each sub-song
			for (int i = 0; i < 16; i++)
			{
				if (moduleStream.Read_UINT8() > 15)
					return AgentResult.Unknown;
			}

			// Check offsets
			uint minOffset = uint.MaxValue;

			for (int i = 0; i < 4 * 16; i++)
			{
				uint offset = moduleStream.Read_B_UINT32();
				if (offset > moduleStream.Length)
					return AgentResult.Unknown;

				minOffset = Math.Min(minOffset, offset);
			}

			if (minOffset != 276)
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

				uint moduleLength = moduleStream.Read_B_UINT32();

				if (!LoadSubSongs(moduleStream))
				{
					errorMessage = Resources.IDS_PSF_ERR_LOADING_SUBSONG;
					return AgentResult.Error;
				}

				if (!LoadOpcodes(moduleStream, moduleLength))
				{
					errorMessage = Resources.IDS_PSF_ERR_LOADING_OPCODES;
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
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public override bool InitPlayer(out string errorMessage)
		{
			if (!base.InitPlayer(out errorMessage))
				return false;

			opcodeTimes = new Dictionary<uint, TimeSpan>();

			FindSamples();

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
			HandleFade();

			for (int i = 3; i >= 0; i--)	// Need to take the voices backward, else will short.psf won't play correctly
			{
				VoiceInfo voiceInfo = voices[i];
				IChannel channel = VirtualChannels[i];

				if (voiceInfo.VoiceEnabled)
					PlayVoice(voiceInfo, channel);
				else
					OnEndReached(i);
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
				foreach (Instrument instr in originalInstruments.Select(x => x.Value))
				{
					SampleInfo sampleInfo = new SampleInfo
					{
						Name = string.Empty,
						Flags = SampleInfo.SampleFlag.None,
						Type = SampleInfo.SampleType.Sample,
						Volume = 256,
						Panning = -1,
						Sample = instr.SampleData,
						Length = instr.SampleLength * 2U
					};

					// Build frequency table
					uint[] freqs = new uint[10 * 12];

					if (instr.SamplingPeriod != 0)
					{
						for (int i = 0; i < 12 * 8; i++)
						{
							ushort period = CalculatePeriodFromSamplingPeriod(instr, i / 12, i % 12);
							freqs[1 * 12 + i] = PeriodToFrequency(period);
						}
					}
					else
					{
						for (int i = 0; i < 12 * 8; i++)
						{
							ushort period = CalculatePeriodFromMultipleOctaveSample(instr, i / 12, i % 12);
							freqs[1 * 12 + i] = PeriodToFrequency(period);
						}
					}

					sampleInfo.NoteFrequencies = freqs;

					if (!instr.EffectByte.HasFlag(InstrumentFlag.OneShot))
					{
						sampleInfo.LoopStart = 0;
						sampleInfo.LoopLength = sampleInfo.Length;
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
				// Used samples
				case 0:
				{
					description = Resources.IDS_PSF_INFODESCLINE0;
					value = originalInstruments.Count.ToString();
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

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Load sub-song information
		/// </summary>
		/********************************************************************/
		private bool LoadSubSongs(ModuleStream moduleStream)
		{
			List<SongInfo> songs = new List<SongInfo>();

			for (int i = 0; i < 16; i++)
			{
				byte channels = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return false;

				if (channels == 0)
					continue;

				SongInfo songInfo = new SongInfo
				{
					EnabledChannels = channels
				};

				songs.Add(songInfo);
			}

			if (songs.Count == 0)
				return false;

			moduleStream.Seek(20, SeekOrigin.Begin);

			foreach (SongInfo songInfo in songs)
			{
				for (int i = 0; i < 4; i++)
					songInfo.OpcodeStartOffsets[i] = moduleStream.Read_B_UINT32() - 276;

				if (moduleStream.EndOfStream)
					return false;
			}

			songInfoList = songs.ToArray();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the opcodes
		/// </summary>
		/********************************************************************/
		private bool LoadOpcodes(ModuleStream moduleStream, uint moduleLength)
		{
			moduleStream.Seek(276, SeekOrigin.Begin);

			// Just read all the opcodes into one big block
			opcodes = new byte[moduleLength - 276];

			if (moduleStream.Read(opcodes, 0, opcodes.Length) != opcodes.Length)
				return false;

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
				DefaultInstrument = Tables.DefaultInstrument.MakeDeepClone(),
				InstrumentLookup = originalInstruments.Select(x => new KeyValuePair<uint, Instrument>(x.Key, x.Value.MakeDeepClone())).ToDictionary(),
				SoundTable = new Instrument[32],

				FadeOutFlag = false,
				FadeInFlag = false,
				FadeOutVolume = 0,
				FadeOutCounter = 0,
				FadeOutSpeed = 0,

				RequestCounter = 0,

				TakenOpcodes = new HashSet<uint>()
			};

			// Set default instrument in sound bank

			for (int i = 0; i < 32; i++)
				playingInfo.SoundTable[i] = playingInfo.DefaultInstrument;

			voices = ArrayHelper.InitializeArray<VoiceInfo>(4);

			for (int i = 0; i < 4; i++)
			{
				voices[i] = new VoiceInfo
				{
					ChannelNumber = i,

					VoiceEnabled = (currentSongInfo.EnabledChannels & (1 << i)) != 0,

					StartPosition = currentSongInfo.OpcodeStartOffsets[i],
					CurrentPosition = currentSongInfo.OpcodeStartOffsets[i],

					CurrentInstrument = 0,

					NoteDuration = 1,
					NoteDuration2 = 0,
					Note = 0,
					Transpose = 0,

					FineTune = 0,
					Period = 0,

					CurrentVolume = 0,
					Volume = 0,

					ActivePeriod = 0,
					PortamentoCounter = 0,

					ArpeggioFlag = false,
					ArpeggioCounter = 0,

					VibratoDelay = 0,
					VibratoCounter = 0,
					VibratoCounter2 = 0,
					VibratoRelative = 0,
					VibratoStep = 0,

					TremoloCounter = 0,
					TremoloStep = 0,
					TremoloVolume = 0,

					EnvelopeState = EnvelopeState.Attack,
					EnvelopeCounter = 0,

					PhasingCounter = 0,
					PhasingStep = 0,
					PhasingRelative = 0,

					FilterCounter = 0,
					FilterStep = 0,
					FilterRelative = 0,

					Stack = new Stack<uint>(),

					NoteStartFlag = 0,
					NoteStartFlag1 = 0,

					PhasingBuffer = new sbyte[256],
				};
			}
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			currentSongInfo = null;

			playingInfo = null;
			voices = null;

			originalInstruments = null;

			songInfoList = null;
			opcodes = null;

			opcodeTimes = null;
		}



		/********************************************************************/
		/// <summary>
		/// Scan all the opcodes to find the samples
		/// </summary>
		/********************************************************************/
		private void FindSamples()
		{
			originalInstruments = new SortedDictionary<uint, Instrument>();

			HashSet<uint> takenOpcodes = new HashSet<uint>();

			foreach (SongInfo songInfo in songInfoList)
			{
				for (int i = 0; i < 4; i++)
					FindSamplesInList(songInfo.OpcodeStartOffsets[i], takenOpcodes);
			}

			// Set instrument numbers
			short index = 0;
			foreach (KeyValuePair<uint, Instrument> pair in originalInstruments)
				pair.Value.InstrumentNumber = index++;
		}



		/********************************************************************/
		/// <summary>
		/// Scan all the opcodes from the given offset to find the samples
		/// </summary>
		/********************************************************************/
		private void FindSamplesInList(uint offset, HashSet<uint> takenOpcodes)
		{
			bool stop = false;

			do
			{
				takenOpcodes.Add(offset);

				Opcode opcode = (Opcode)opcodes[offset++];
				uint bytesToSkip;

				switch (opcode)
				{
					case Opcode.Next:
					case Opcode.Nop:
					case Opcode.Request:
					case Opcode.OneShot:
					case Opcode.Looping:
					{
						bytesToSkip = 0;
						break;
					}

					case Opcode.SetVolume:
					case Opcode.SetFineTune:
					case Opcode.UseInstrument:
					case Opcode.For:
					case Opcode.FadeOut:
					case Opcode.FadeIn:
					case Opcode.Led:
					case Opcode.WaitForRequest:
					case Opcode.SetTranspose:
					{
						bytesToSkip = 1;
						break;
					}

					case Opcode.Pause:
					case Opcode.StopAndPause:
					{
						bytesToSkip = 2;
						break;
					}

					case Opcode.Portamento:
					case Opcode.Tremolo:
					case Opcode.Filter:
					{
						bytesToSkip = 0;

						bool enable = opcodes[offset++] != 0;
						if (enable)
							bytesToSkip = 3;

						break;
					}

					case Opcode.Arpeggio:
					{
						bytesToSkip = 0;

						bool enable = opcodes[offset++] != 0;
						if (enable)
							bytesToSkip = 1;

						break;
					}

					case Opcode.Vibrato:
					case Opcode.Phasing:
					{
						bytesToSkip = 0;

						bool enable = opcodes[offset++] != 0;
						if (enable)
							bytesToSkip = 4;

						break;
					}

					case Opcode.SetAdsr:
					{
						bytesToSkip = 4;
						offset += 3;

						bool releaseEnabled = opcodes[offset++] != 0;
						if (releaseEnabled)
							bytesToSkip++;

						break;
					}

					case Opcode.DefineInstrument:
					{
						offset++;
						ushort instrumentLength = FetchWord(ref offset);

						Instrument instr = FetchInstrument(offset);
						originalInstruments[offset] = instr;

						bytesToSkip = instrumentLength * 2U - 4U;
						break;
					}

					case Opcode.Return:
					{
						bytesToSkip = 0;
						stop = true;
						break;
					}

					case Opcode.GoSub:
					{
						int gotoOffset = FetchLong(ref offset);
						uint newOffset = (uint)(offset + gotoOffset);

						if (!takenOpcodes.Contains(newOffset))
							FindSamplesInList(newOffset, takenOpcodes);

						bytesToSkip = 0;
						break;
					}

					case Opcode.Goto:
					{
						int gotoOffset = FetchLong(ref offset);

						offset = (uint)(offset + gotoOffset);

						if (takenOpcodes.Contains(offset))
							stop = true;

						bytesToSkip = 0;
						break;
					}

					case Opcode.Loop:
					case Opcode.End:
					{
						bytesToSkip = 0;
						stop = true;
						break;
					}

					default:
					{
						// Notes
						bytesToSkip = 2;
						break;
					}
				}

				offset += bytesToSkip;
			}
			while (!stop);
		}



		/********************************************************************/
		/// <summary>
		/// Read a whole instrument and return it
		/// </summary>
		/********************************************************************/
		private Instrument FetchInstrument(uint offset)
		{
			Instrument instr = new Instrument();

			instr.SampleLength = FetchWord(ref offset);
			instr.SamplingPeriod = FetchWord(ref offset);

			instr.EffectByte = (InstrumentFlag)opcodes[offset++];

			instr.TremoloSpeed = opcodes[offset++];
			instr.TremoloStep = opcodes[offset++];
			instr.TremoloRange = opcodes[offset++];

			instr.PortamentoStep = FetchWord(ref offset);
			instr.PortamentoSpeed = opcodes[offset++];

			instr.ArpeggioSpeed = opcodes[offset++];

			instr.VibratoDelay = opcodes[offset++];
			instr.VibratoSpeed = opcodes[offset++];
			instr.VibratoStep = (sbyte)opcodes[offset++];
			instr.VibratoAmount = opcodes[offset++];

			instr.AttackTime = opcodes[offset++];
			instr.DecayTime = opcodes[offset++];
			instr.SustainLevel = opcodes[offset++];
			instr.ReleaseTime = opcodes[offset++];

			instr.PhasingStart = opcodes[offset++];
			instr.PhasingEnd = opcodes[offset++];
			instr.PhasingSpeed = opcodes[offset++];
			instr.PhasingStep = (sbyte)opcodes[offset++];

			instr.WaveCount = opcodes[offset++];
			instr.Octave = opcodes[offset++];

			instr.FilterFrequency = opcodes[offset++];
			instr.FilterEnd = opcodes[offset++];
			instr.FilterSpeed = opcodes[offset++];
			offset++;

			instr.DASR_SustainOffset = FetchWord(ref offset);
			instr.DASR_ReleaseOffset = FetchWord(ref offset);

			instr.SampleData = new sbyte[instr.SampleLength * 2];
			MemoryMarshal.Cast<byte, sbyte>(opcodes.AsSpan((int)offset, instr.SampleData.Length)).CopyTo(instr.SampleData);

			return instr;
		}



		/********************************************************************/
		/// <summary>
		/// Read a single byte from the opcode list and return it
		/// </summary>
		/********************************************************************/
		private byte FetchCode(VoiceInfo voiceInfo)
		{
			return opcodes[voiceInfo.CurrentPosition++];
		}



		/********************************************************************/
		/// <summary>
		/// Read 2 bytes from the opcode list and return it
		/// </summary>
		/********************************************************************/
		private ushort FetchWord(VoiceInfo voiceInfo)
		{
			return (ushort)((FetchCode(voiceInfo) << 8) | FetchCode(voiceInfo));
		}



		/********************************************************************/
		/// <summary>
		/// Read 2 bytes from the opcode list and return it
		/// </summary>
		/********************************************************************/
		private ushort FetchWord(ref uint offset)
		{
			ushort value = (ushort)((opcodes[offset] << 8) | opcodes[offset + 1]);
			offset += 2;

			return value;
		}



		/********************************************************************/
		/// <summary>
		/// Read 4 bytes from the opcode list and return it
		/// </summary>
		/********************************************************************/
		private int FetchLong(ref uint offset)
		{
			int value = ((sbyte)opcodes[offset] << 24) | (opcodes[offset + 1] << 16) | (opcodes[offset + 2] << 8) | opcodes[offset + 3];
			offset += 4;

			return value;
		}



		/********************************************************************/
		/// <summary>
		/// Store a byte into the stack
		/// </summary>
		/********************************************************************/
		private void PushByte(VoiceInfo voiceInfo, byte value)
		{
			voiceInfo.Stack.Push(value);
		}



		/********************************************************************/
		/// <summary>
		/// Store an uint into the stack
		/// </summary>
		/********************************************************************/
		private void PushLong(VoiceInfo voiceInfo, uint value)
		{
			voiceInfo.Stack.Push(value);
		}



		/********************************************************************/
		/// <summary>
		/// Read a byte from the stack
		/// </summary>
		/********************************************************************/
		private byte PullByte(VoiceInfo voiceInfo)
		{
			return (byte)voiceInfo.Stack.Pop();
		}



		/********************************************************************/
		/// <summary>
		/// Read an uint from the stack
		/// </summary>
		/********************************************************************/
		private uint PullLong(VoiceInfo voiceInfo)
		{
			return voiceInfo.Stack.Pop();
		}



		/********************************************************************/
		/// <summary>
		/// Return the current instrument for the voice
		/// </summary>
		/********************************************************************/
		private Instrument GetInstrument(VoiceInfo voiceInfo)
		{
			return playingInfo.SoundTable[voiceInfo.CurrentInstrument];
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the period from the sampling period, octave and note
		/// </summary>
		/********************************************************************/
		private ushort CalculatePeriod(VoiceInfo voiceInfo, Instrument instr)
		{
			int octave = voiceInfo.Note / 12;
			int note = voiceInfo.Note % 12;

			if (instr.SamplingPeriod != 0)
				return CalculatePeriodFromSamplingPeriod(instr, octave, note);

			return CalculatePeriodFromMultipleOctaveSample(instr, octave, note);
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the period from a multi-octave sample
		/// </summary>
		/********************************************************************/
		private ushort CalculatePeriodFromMultipleOctaveSample(Instrument instr, int octave, int note)
		{
			uint period = Tables.SampleTable[note];

			if (instr.WaveCount != 1)
				period *= instr.WaveCount;

			if (instr.SampleLength != 1)
				period /= instr.SampleLength;

			while (octave != 0)
			{
				period /= 2;
				octave--;
			}

			return (ushort)(period * 2);
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the period from the sampling period, octave and note
		/// </summary>
		/********************************************************************/
		private ushort CalculatePeriodFromSamplingPeriod(Instrument instr, int octave, int note)
		{
			ushort multiplier = Tables.MultiplyTable[note];
			int period = instr.SamplingPeriod * multiplier / 32768;

			int currentOctave = instr.Octave;

			while (currentOctave != octave)
			{
				if (currentOctave < octave)
				{
					period /= 2;
					currentOctave++;
				}
				else
				{
					period *= 2;
					currentOctave--;
				}
			}

			return (ushort)period;
		}



		/********************************************************************/
		/// <summary>
		/// Handle the fade in and out
		/// </summary>
		/********************************************************************/
		private void HandleFade()
		{
			if (playingInfo.FadeInFlag || playingInfo.FadeOutFlag)
			{
				byte newFadeCounter = (byte)(playingInfo.FadeOutCounter + playingInfo.FadeOutSpeed);
				playingInfo.FadeOutCounter = (byte)(newFadeCounter & 3);

				newFadeCounter >>= 2;
				int fadeOutVolume = playingInfo.FadeOutVolume;

				if (playingInfo.FadeInFlag)
				{
					fadeOutVolume += newFadeCounter;

					if (fadeOutVolume >= 256)
					{
						playingInfo.FadeInFlag = false;
						playingInfo.FadeOutVolume = 0;
					}
					else
						playingInfo.FadeOutVolume = (byte)fadeOutVolume;
				}
				else
				{
					if (fadeOutVolume == 0)
						fadeOutVolume -= newFadeCounter;
					else
					{
						fadeOutVolume -= newFadeCounter;

						if (fadeOutVolume <= 0)
						{
							// Stop playing
							playingInfo.FadeOutFlag = false;
							playingInfo.FadeInFlag = false;

							for (int i = 0; i < 4; i++)
							{
								voices[i].VoiceEnabled = false;
								VirtualChannels[i].Mute();
							}

							OnEndReachedOnAllChannels(0);
							RestartSong();
							return;
						}
					}

					playingInfo.FadeOutVolume = (byte)fadeOutVolume;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Play a single voice
		/// </summary>
		/********************************************************************/
		private void PlayVoice(VoiceInfo voiceInfo, IChannel channel)
		{
			Instrument instr = GetInstrument(voiceInfo);

			voiceInfo.NoteDuration--;

			if (voiceInfo.NoteDuration == 0)
				NextNote(voiceInfo, channel, instr);
			else
				Modulator(voiceInfo, channel, instr);
		}



		/********************************************************************/
		/// <summary>
		/// Parse the next note/opcode for the voice
		/// </summary>
		/********************************************************************/
		private void NextNote(VoiceInfo voiceInfo, IChannel channel, Instrument instr)
		{
			byte opcode;

			do
			{
				playingInfo.TakenOpcodes.Add(voiceInfo.CurrentPosition);

				TimeSpan? currentTime = GetCurrentTime();
				if (currentTime.HasValue)
					opcodeTimes[voiceInfo.CurrentPosition] = currentTime.Value;

				opcode = FetchCode(voiceInfo);

				if ((opcode & 0x80) == 0)
					opcode = NewNote(voiceInfo, channel, instr, opcode) ? (byte)0 : (byte)1;
				else
				{
					switch ((Opcode)opcode)
					{
						case Opcode.Pause:
						{
							DoOpcodePause(voiceInfo);
							break;
						}

						case Opcode.SetVolume:
						{
							DoOpcodeSetVolume(voiceInfo);
							break;
						}

						case Opcode.SetFineTune:
						{
							DoOpcodeSetFineTune(voiceInfo);
							break;
						}

						case Opcode.UseInstrument:
						{
							instr = DoOpcodeUseInstrument(voiceInfo);
							break;
						}

						case Opcode.DefineInstrument:
						{
							DoOpcodeDefineInstrument(voiceInfo);
							break;
						}

						case Opcode.Return:
						{
							DoOpcodeReturn(voiceInfo);
							break;
						}

						case Opcode.GoSub:
						{
							DoOpcodeGoSub(voiceInfo);
							break;
						}

						case Opcode.Goto:
						{
							DoOpcodeGoto(voiceInfo);

							if (playingInfo.TakenOpcodes.Contains(voiceInfo.CurrentPosition))
								EndChannel(voiceInfo.ChannelNumber);

							break;
						}

						case Opcode.For:
						{
							DoOpcodeFor(voiceInfo);
							break;
						}

						case Opcode.Next:
						{
							DoOpcodeNext(voiceInfo);
							break;
						}

						case Opcode.FadeOut:
						{
							DoOpcodeFadeOut(voiceInfo);
							break;
						}

						case Opcode.Nop:
						{
							break;
						}

						case Opcode.Request:
						{
							DoOpcodeRequest();
							break;
						}

						case Opcode.Loop:
						{
							DoOpcodeLoop(voiceInfo);

							EndChannel(voiceInfo.ChannelNumber);
							break;
						}

						case Opcode.End:
						{
							DoOpcodeEnd(voiceInfo, channel);

							if (!playingInfo.FadeOutFlag)
							{
								OnEndReached(voiceInfo.ChannelNumber);

								if (HasEndReached)
									RestartSong();
							}

							opcode = 0;
							break;
						}

						case Opcode.FadeIn:
						{
							DoOpcodeFadeIn(voiceInfo);
							break;
						}

						case Opcode.SetAdsr:
						{
							DoOpcodeSetAdsr(voiceInfo, instr);
							break;
						}

						case Opcode.OneShot:
						{
							DoOpcodeOneShot(instr);
							break;
						}

						case Opcode.Looping:
						{
							DoOpcodeLooping(instr);
							break;
						}

						case Opcode.Vibrato:
						{
							DoOpcodeVibrato(voiceInfo, instr);
							break;
						}

						case Opcode.Arpeggio:
						{
							DoOpcodeArpeggio(voiceInfo, instr);
							break;
						}

						case Opcode.Phasing:
						{
							DoOpcodePhasing(voiceInfo, instr);
							break;
						}

						case Opcode.Portamento:
						{
							DoOpcodePortamento(voiceInfo, instr);
							break;
						}

						case Opcode.Tremolo:
						{
							DoOpcodeTremolo(voiceInfo, instr);
							break;
						}

						case Opcode.Filter:
						{
							DoOpcodeFilter(voiceInfo, instr);
							break;
						}

						case Opcode.StopAndPause:
						{
							DoOpcodeStopAndPause(voiceInfo, channel);

							opcode = 0;
							break;
						}

						case Opcode.Led:
						{
							DoOpcodeLed(voiceInfo);
							break;
						}

						case Opcode.WaitForRequest:
						{
							if (!DoOpcodeWaitForRequest(voiceInfo))
								opcode = 0;

							break;
						}

						case Opcode.SetTranspose:
						{
							DoOpcodeSetTranspose(voiceInfo);
							break;
						}
					}
				}
			}
			while ((opcode & 0x7f) != 0);
		}



		/********************************************************************/
		/// <summary>
		/// 80: Pause
		/// </summary>
		/********************************************************************/
		private void DoOpcodePause(VoiceInfo voiceInfo)
		{
			voiceInfo.NoteDuration = FetchWord(voiceInfo);
		}



		/********************************************************************/
		/// <summary>
		/// 81: Set volume
		/// </summary>
		/********************************************************************/
		private void DoOpcodeSetVolume(VoiceInfo voiceInfo)
		{
			voiceInfo.Volume = FetchCode(voiceInfo);
		}



		/********************************************************************/
		/// <summary>
		/// 82: Set finetune
		/// </summary>
		/********************************************************************/
		private void DoOpcodeSetFineTune(VoiceInfo voiceInfo)
		{
			voiceInfo.FineTune = FetchCode(voiceInfo);
		}



		/********************************************************************/
		/// <summary>
		/// 83: Use instrument
		/// </summary>
		/********************************************************************/
		private Instrument DoOpcodeUseInstrument(VoiceInfo voiceInfo)
		{
			voiceInfo.CurrentInstrument = FetchCode(voiceInfo);

			return GetInstrument(voiceInfo);
		}



		/********************************************************************/
		/// <summary>
		/// 84: Define instrument
		/// </summary>
		/********************************************************************/
		private void DoOpcodeDefineInstrument(VoiceInfo voiceInfo)
		{
			byte instrumentNumber = FetchCode(voiceInfo);
			ushort instrumentLength = FetchWord(voiceInfo);

			playingInfo.SoundTable[instrumentNumber] = playingInfo.InstrumentLookup[voiceInfo.CurrentPosition];

			// Skip instrument data, since we already have it
			voiceInfo.CurrentPosition += (instrumentLength * 2U - 4);
		}



		/********************************************************************/
		/// <summary>
		/// 85: Return from sub-routine
		/// </summary>
		/********************************************************************/
		private void DoOpcodeReturn(VoiceInfo voiceInfo)
		{
			voiceInfo.CurrentPosition = PullLong(voiceInfo);
		}



		/********************************************************************/
		/// <summary>
		/// 86: go to sub-routine
		/// </summary>
		/********************************************************************/
		private void DoOpcodeGoSub(VoiceInfo voiceInfo)
		{
			int newPosition = (FetchWord(voiceInfo) << 16) | FetchWord(voiceInfo);

			PushLong(voiceInfo, voiceInfo.CurrentPosition);

			voiceInfo.CurrentPosition = (uint)(voiceInfo.CurrentPosition + newPosition);
		}



		/********************************************************************/
		/// <summary>
		/// 87: goto
		/// </summary>
		/********************************************************************/
		private void DoOpcodeGoto(VoiceInfo voiceInfo)
		{
			int newPosition = (FetchWord(voiceInfo) << 16) | FetchWord(voiceInfo);

			voiceInfo.CurrentPosition = (uint)(voiceInfo.CurrentPosition + newPosition);
		}



		/********************************************************************/
		/// <summary>
		/// 88: Start for loop
		/// </summary>
		/********************************************************************/
		private void DoOpcodeFor(VoiceInfo voiceInfo)
		{
			PushByte(voiceInfo, FetchCode(voiceInfo));
			PushLong(voiceInfo, voiceInfo.CurrentPosition);
		}



		/********************************************************************/
		/// <summary>
		/// 89: Iterate loop
		/// </summary>
		/********************************************************************/
		private void DoOpcodeNext(VoiceInfo voiceInfo)
		{
			uint loopPosition = PullLong(voiceInfo);
			byte loopCount = PullByte(voiceInfo);

			loopCount--;

			if (loopCount != 0)
			{
				voiceInfo.CurrentPosition = loopPosition;

				PushByte(voiceInfo, loopCount);
				PushLong(voiceInfo, loopPosition);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 8A: Start to fade out
		/// </summary>
		/********************************************************************/
		private void DoOpcodeFadeOut(VoiceInfo voiceInfo)
		{
			playingInfo.FadeOutSpeed = FetchCode(voiceInfo);
			playingInfo.FadeInFlag = false;
			playingInfo.FadeOutCounter = 0;
			playingInfo.FadeOutFlag = true;
		}



		/********************************************************************/
		/// <summary>
		/// 8C: Increment request counter
		/// </summary>
		/********************************************************************/
		private void DoOpcodeRequest()
		{
			playingInfo.RequestCounter++;
		}



		/********************************************************************/
		/// <summary>
		/// 8D: Loop voice
		/// </summary>
		/********************************************************************/
		private void DoOpcodeLoop(VoiceInfo voiceInfo)
		{
			voiceInfo.CurrentPosition = voiceInfo.StartPosition;
		}



		/********************************************************************/
		/// <summary>
		/// 8E: End voice
		/// </summary>
		/********************************************************************/
		private void DoOpcodeEnd(VoiceInfo voiceInfo, IChannel channel)
		{
			channel.Mute();

			voiceInfo.VoiceEnabled = false;
		}



		/********************************************************************/
		/// <summary>
		/// 8F: Start fade in
		/// </summary>
		/********************************************************************/
		private void DoOpcodeFadeIn(VoiceInfo voiceInfo)
		{
			playingInfo.FadeOutSpeed = FetchCode(voiceInfo);
			playingInfo.FadeInFlag = true;

			if (playingInfo.FadeOutVolume == 0)
				playingInfo.FadeOutVolume = 1;

			playingInfo.FadeOutCounter = 0;
			playingInfo.FadeOutFlag = false;
		}



		/********************************************************************/
		/// <summary>
		/// 90: Set ADSR values
		/// </summary>
		/********************************************************************/
		private void DoOpcodeSetAdsr(VoiceInfo voiceInfo, Instrument instr)
		{
			instr.AttackTime = FetchCode(voiceInfo);
			instr.DecayTime = FetchCode(voiceInfo);
			instr.SustainLevel = FetchCode(voiceInfo);
			bool releaseEnabled = FetchCode(voiceInfo) != 0;

			if (releaseEnabled)
			{
				instr.EffectByte &= ~InstrumentFlag.Release;
				instr.ReleaseTime = FetchCode(voiceInfo);
			}
			else
				instr.EffectByte |= InstrumentFlag.Release;
		}



		/********************************************************************/
		/// <summary>
		/// 91: Play sample without loop
		/// </summary>
		/********************************************************************/
		private void DoOpcodeOneShot(Instrument instr)
		{
			instr.EffectByte |= InstrumentFlag.OneShot;
		}



		/********************************************************************/
		/// <summary>
		/// 92: Play sample with loop
		/// </summary>
		/********************************************************************/
		private void DoOpcodeLooping(Instrument instr)
		{
			instr.EffectByte &= ~InstrumentFlag.OneShot;
		}



		/********************************************************************/
		/// <summary>
		/// 93: Set vibrato
		/// </summary>
		/********************************************************************/
		private void DoOpcodeVibrato(VoiceInfo voiceInfo, Instrument instr)
		{
			bool enable = FetchCode(voiceInfo) != 0;

			if (enable)
			{
				instr.EffectByte |= InstrumentFlag.Vibrato;

				instr.VibratoDelay = FetchCode(voiceInfo);
				instr.VibratoSpeed = FetchCode(voiceInfo);
				instr.VibratoStep = (sbyte)FetchCode(voiceInfo);
				instr.VibratoAmount = FetchCode(voiceInfo);
			}
			else
				instr.EffectByte &= ~InstrumentFlag.Vibrato;
		}



		/********************************************************************/
		/// <summary>
		/// 94: Set arpeggio
		/// </summary>
		/********************************************************************/
		private void DoOpcodeArpeggio(VoiceInfo voiceInfo, Instrument instr)
		{
			bool enable = FetchCode(voiceInfo) != 0;

			if (enable)
			{
				instr.EffectByte |= InstrumentFlag.Arpeggio;

				instr.ArpeggioSpeed = FetchCode(voiceInfo);
			}
			else
				instr.EffectByte &= ~InstrumentFlag.Arpeggio;
		}



		/********************************************************************/
		/// <summary>
		/// 95: Set phasing
		/// </summary>
		/********************************************************************/
		private void DoOpcodePhasing(VoiceInfo voiceInfo, Instrument instr)
		{
			bool enable = FetchCode(voiceInfo) != 0;

			if (enable)
			{
				instr.EffectByte |= InstrumentFlag.Phasing;

				instr.PhasingStart = FetchCode(voiceInfo);
				instr.PhasingEnd = FetchCode(voiceInfo);
				instr.PhasingSpeed = FetchCode(voiceInfo);
				instr.PhasingStep = (sbyte)FetchCode(voiceInfo);
			}
			else
				instr.EffectByte &= ~InstrumentFlag.Phasing;
		}



		/********************************************************************/
		/// <summary>
		/// 96: Set portamento
		/// </summary>
		/********************************************************************/
		private void DoOpcodePortamento(VoiceInfo voiceInfo, Instrument instr)
		{
			bool enable = FetchCode(voiceInfo) != 0;

			if (enable)
			{
				instr.EffectByte |= InstrumentFlag.Portamento;

				instr.PortamentoSpeed = FetchCode(voiceInfo);
				instr.PortamentoStep = FetchWord(voiceInfo);
			}
			else
				instr.EffectByte &= ~InstrumentFlag.Portamento;
		}



		/********************************************************************/
		/// <summary>
		/// 97: Set tremolo
		/// </summary>
		/********************************************************************/
		private void DoOpcodeTremolo(VoiceInfo voiceInfo, Instrument instr)
		{
			bool enable = FetchCode(voiceInfo) != 0;

			if (enable)
			{
				instr.EffectByte |= InstrumentFlag.Tremolo;

				instr.TremoloSpeed = FetchCode(voiceInfo);
				instr.TremoloStep = FetchCode(voiceInfo);
				instr.TremoloRange = FetchCode(voiceInfo);
			}
			else
				instr.EffectByte &= ~InstrumentFlag.Tremolo;
		}



		/********************************************************************/
		/// <summary>
		/// 98: Set filter
		/// </summary>
		/********************************************************************/
		private void DoOpcodeFilter(VoiceInfo voiceInfo, Instrument instr)
		{
			bool enable = FetchCode(voiceInfo) != 0;

			if (enable)
			{
				instr.EffectByte |= InstrumentFlag.Filter;

				instr.FilterFrequency = FetchCode(voiceInfo);
				instr.FilterEnd = FetchCode(voiceInfo);
				instr.FilterSpeed = FetchCode(voiceInfo);
			}
			else
				instr.EffectByte &= ~InstrumentFlag.Filter;
		}



		/********************************************************************/
		/// <summary>
		/// 99: Stop and pause voice
		/// </summary>
		/********************************************************************/
		private void DoOpcodeStopAndPause(VoiceInfo voiceInfo, IChannel channel)
		{
			voiceInfo.NoteDuration = FetchWord(voiceInfo);

			channel.Mute();
		}



		/********************************************************************/
		/// <summary>
		/// 9A: LED
		/// </summary>
		/********************************************************************/
		private void DoOpcodeLed(VoiceInfo voiceInfo)
		{
			bool enable = FetchCode(voiceInfo) != 0;

			AmigaFilter = enable;
		}



		/********************************************************************/
		/// <summary>
		/// 9B: Wait for request
		/// </summary>
		/********************************************************************/
		private bool DoOpcodeWaitForRequest(VoiceInfo voiceInfo)
		{
			byte value = FetchCode(voiceInfo);

			if (value == playingInfo.RequestCounter)
				return true;

			voiceInfo.CurrentPosition -= 2;
			voiceInfo.NoteDuration = 1;

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// 9C: Set transpose
		/// </summary>
		/********************************************************************/
		private void DoOpcodeSetTranspose(VoiceInfo voiceInfo)
		{
			voiceInfo.Transpose = (sbyte)FetchCode(voiceInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Set a new note
		/// </summary>
		/********************************************************************/
		private bool NewNote(VoiceInfo voiceInfo, IChannel channel, Instrument instr, byte note)
		{
			voiceInfo.Note = (byte)((note + voiceInfo.Transpose) & 127);

			InstrumentFlag instrFlag = instr.EffectByte;

			if (instrFlag.HasFlag(InstrumentFlag.Portamento))
			{
				voiceInfo.ActivePeriod = voiceInfo.Period;
				voiceInfo.PortamentoCounter = 1;
			}

			voiceInfo.Period = CalculatePeriod(voiceInfo, instr);

			ushort originalNoteDuration = FetchWord(voiceInfo);
			ushort noteDuration = (ushort)(originalNoteDuration & 0x7fff);

			if (noteDuration == 0)
			{
				channel.Mute();
				return false;
			}

			voiceInfo.NoteDuration = noteDuration;
			voiceInfo.NoteDuration2 = (ushort)(noteDuration / 2);
			voiceInfo.NoteStartFlag = 1;

			if (instrFlag.HasFlag(InstrumentFlag.Arpeggio))
			{
				voiceInfo.ArpeggioFlag = false;
				voiceInfo.ArpeggioCounter = instr.ArpeggioSpeed;
			}

			if (instrFlag.HasFlag(InstrumentFlag.Vibrato))
			{
				voiceInfo.VibratoDelay = instr.VibratoDelay;

				if (voiceInfo.VibratoDelay == 0)
				{
					voiceInfo.VibratoStep = instr.VibratoStep;
					voiceInfo.VibratoRelative = 0;
					voiceInfo.VibratoCounter = instr.VibratoSpeed;
					voiceInfo.VibratoCounter2 = instr.VibratoAmount;
				}
			}

			if (instrFlag.HasFlag(InstrumentFlag.Tremolo))
			{
				voiceInfo.TremoloCounter = 1;
				voiceInfo.TremoloStep = (sbyte)(-instr.TremoloStep);
				voiceInfo.TremoloVolume = 0;
			}

			if ((originalNoteDuration & 0x8000) == 0)
			{
				voiceInfo.EnvelopeCounter = 0;

				if (instr.AttackTime != 0)
				{
					voiceInfo.CurrentVolume = 0;
					voiceInfo.EnvelopeState = EnvelopeState.Attack;
				}
				else
				{
					if ((instr.DecayTime == 0) || (instr.SustainLevel == 64))
					{
						voiceInfo.EnvelopeState = EnvelopeState.Sustain;
						voiceInfo.CurrentVolume = instr.SustainLevel;
					}
					else
					{
						voiceInfo.CurrentVolume = 64;
						voiceInfo.EnvelopeState = EnvelopeState.Decay;
					}
				}
			}

			sbyte[] sampleData;

			if (instrFlag.HasFlag(InstrumentFlag.Phasing))
			{
				voiceInfo.PhasingCounter = instr.PhasingSpeed;
				voiceInfo.PhasingStep = instr.PhasingStep;
				voiceInfo.PhasingRelative = (sbyte)instr.PhasingStart;

				Mix(voiceInfo, instr);
				sampleData = voiceInfo.PhasingBuffer;
			}
			else
				sampleData = instr.SampleData;

			if (instrFlag.HasFlag(InstrumentFlag.Filter))
			{
				voiceInfo.FilterCounter = instr.FilterSpeed;
				voiceInfo.FilterRelative = instr.FilterFrequency;
				voiceInfo.FilterStep = 1;

				Filter(voiceInfo, instr);
				sampleData = voiceInfo.PhasingBuffer;
			}

			if (instr.DASR_SustainOffset != 0)
			{
				channel.PlaySample(instr.InstrumentNumber, sampleData, 0, instr.DASR_ReleaseOffset * 2U);
				channel.SetLoop(instr.DASR_SustainOffset * 2U, ((uint)instr.DASR_ReleaseOffset - instr.DASR_SustainOffset) * 2U);
			}
			else
			{
				channel.PlaySample(instr.InstrumentNumber, sampleData, 0, instr.SampleLength * 2U);

				if (!instrFlag.HasFlag(InstrumentFlag.OneShot))
					channel.SetLoop(0, instr.SampleLength * 2U);
			}

			InHardware(voiceInfo, channel, instrFlag);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Mix audio data for phasing
		/// </summary>
		/********************************************************************/
		private void Mix(VoiceInfo voiceInfo, Instrument instr)
		{
			ushort sampleLength = (ushort)(instr.SampleLength * 2);
			sbyte[] sampleData = instr.SampleData;
			sbyte[] phasingBuffer = voiceInfo.PhasingBuffer;

			int index = sampleLength - voiceInfo.PhasingRelative;

			for (int i = 0; i < sampleLength; i++)
			{
				short sample1 = index >= sampleLength ? (short)0 : sampleData[index];
				short sample2 = sampleData[i];

				phasingBuffer[i] = (sbyte)((sample1 + sample2) / 2);

				index++;
				if (index == sampleLength)
					index = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Filter(VoiceInfo voiceInfo, Instrument instr)
		{
			ushort sampleLength = (ushort)(instr.SampleLength * 2);
			sbyte[] sampleData = instr.SampleData;
			sbyte[] phasingBuffer = voiceInfo.PhasingBuffer;

			if (instr.EffectByte.HasFlag(InstrumentFlag.Phasing))
				sampleData = phasingBuffer;

			if (voiceInfo.FilterRelative == 1)	// 1 means copy original
			{
				if (sampleData != phasingBuffer)
				{
					int toCopy = sampleLength == 256 ? 256 : sampleLength + 4;
					Array.Copy(sampleData, phasingBuffer, toCopy);
				}
			}
			else
			{
				ushort samplePosition = (ushort)(voiceInfo.FilterRelative / 2);
				short average = Average(sampleData, sampleLength, 0, voiceInfo.FilterRelative);

				bool finished = false;

				do
				{
					short previousAverage = average;
					
					ushort position = (ushort)((voiceInfo.FilterRelative / 2) + samplePosition);
					if (position >= sampleLength)
						position -= sampleLength;

					average = Average(sampleData, sampleLength, position, voiceInfo.FilterRelative);

					short difference = (short)(average - previousAverage);
					short step = 1;

					if (difference < 0)
					{
						step = -1;
						difference = (short)-difference;
					}

					short counter = 0;
					for (int i = voiceInfo.FilterRelative; i > 0; i--)
					{
						phasingBuffer[samplePosition] = (sbyte)previousAverage;
						counter += difference;

						while (counter >= voiceInfo.FilterRelative)
						{
							counter -= voiceInfo.FilterRelative;
							previousAverage += step;
						}

						samplePosition++;

						if (samplePosition >= sampleLength)
						{
							samplePosition -= sampleLength;
							finished = true;
						}
					}
				}
				while (!finished);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private short Average(sbyte[] sampleData, ushort sampleLength, ushort offset, short count)
		{
			short sampleSum = 0;

			for (int i = count; i > 0; i--)
			{
				sampleSum += sampleData[offset];

				offset++;

				if (offset == sampleLength)
					offset = 0;
			}

			return (short)(sampleSum / count);
		}



		/********************************************************************/
		/// <summary>
		/// Apply real-time effects
		/// </summary>
		/********************************************************************/
		private void Modulator(VoiceInfo voiceInfo, IChannel channel, Instrument instr)
		{
			InstrumentFlag instrFlag = instr.EffectByte;

			voiceInfo.NoteStartFlag1 = voiceInfo.NoteStartFlag;
			voiceInfo.NoteStartFlag = 0;

			if (instrFlag.HasFlag(InstrumentFlag.Tremolo))
			{
				voiceInfo.TremoloCounter--;

				if (voiceInfo.TremoloCounter == 0)
				{
					voiceInfo.TremoloCounter = instr.TremoloSpeed;

					voiceInfo.TremoloVolume = (byte)(voiceInfo.TremoloVolume + voiceInfo.TremoloStep);

					if (voiceInfo.TremoloVolume <= instr.TremoloRange)
						voiceInfo.TremoloStep = (sbyte)(-voiceInfo.TremoloStep);
				}
			}

			if (instrFlag.HasFlag(InstrumentFlag.Portamento))
			{
				if (voiceInfo.Period != voiceInfo.ActivePeriod)
				{
					voiceInfo.PortamentoCounter--;

					if (voiceInfo.PortamentoCounter == 0)
					{
						voiceInfo.PortamentoCounter = instr.PortamentoSpeed;

						if (voiceInfo.Period < voiceInfo.ActivePeriod)
						{
							voiceInfo.ActivePeriod -= instr.PortamentoStep;

							if (voiceInfo.ActivePeriod < voiceInfo.Period)
								voiceInfo.ActivePeriod = voiceInfo.Period;
						}
						else
						{
							voiceInfo.ActivePeriod += instr.PortamentoStep;

							if (voiceInfo.ActivePeriod > voiceInfo.Period)
								voiceInfo.ActivePeriod = voiceInfo.Period;
						}
					}
				}
			}

			if (instrFlag.HasFlag(InstrumentFlag.Arpeggio))
			{
				voiceInfo.ArpeggioCounter--;

				if (voiceInfo.ArpeggioCounter == 0)
				{
					voiceInfo.ArpeggioCounter = instr.ArpeggioSpeed;
					voiceInfo.ArpeggioFlag = !voiceInfo.ArpeggioFlag;
				}
			}

			if (instrFlag.HasFlag(InstrumentFlag.Vibrato))
			{
				if (voiceInfo.VibratoDelay == 0)
				{
					voiceInfo.VibratoCounter--;

					if (voiceInfo.VibratoCounter == 0)
					{
						voiceInfo.VibratoCounter = instr.VibratoSpeed;

						voiceInfo.VibratoRelative += voiceInfo.VibratoStep;

						voiceInfo.VibratoCounter2--;

						if (voiceInfo.VibratoCounter2 == 0)
						{
							voiceInfo.VibratoCounter2 = (byte)(instr.VibratoAmount * 2);
							voiceInfo.VibratoStep = (sbyte)(-voiceInfo.VibratoStep);
						}
					}
				}
				else
				{
					voiceInfo.VibratoDelay--;

					if (voiceInfo.VibratoDelay == 0)
					{
						voiceInfo.VibratoCounter = 1;
						voiceInfo.VibratoRelative = 0;
						voiceInfo.VibratoStep = instr.VibratoStep;
						voiceInfo.VibratoCounter2 = instr.VibratoAmount;
					}
				}
			}

			if (voiceInfo.EnvelopeState == EnvelopeState.Release)
			{
				if (voiceInfo.CurrentVolume != 0)
				{
					voiceInfo.EnvelopeCounter += instr.SustainLevel;

					for (;;)
					{
						if (voiceInfo.EnvelopeCounter < instr.ReleaseTime)
							break;

						voiceInfo.EnvelopeCounter -= instr.ReleaseTime;
						voiceInfo.CurrentVolume--;

						if (voiceInfo.CurrentVolume == 0)
							break;
					}
				}
			}
			else
			{
				bool doReleaseSkip = false;

				if (instrFlag.HasFlag(InstrumentFlag.Release))
					doReleaseSkip = true;
				else
				{
					 if (voiceInfo.NoteDuration2 != 0)
						 voiceInfo.NoteDuration2--;

					 if (voiceInfo.NoteDuration2 == 0)
					 {
						if (voiceInfo.EnvelopeState != EnvelopeState.Sustain)
							doReleaseSkip = true;
						else
						{
							voiceInfo.EnvelopeState = EnvelopeState.Release;
							voiceInfo.EnvelopeCounter = 0;

							if (instrFlag.HasFlag(InstrumentFlag.OneShot) && (instr.DASR_SustainOffset != 0))
							{
								channel.SetSample(instr.DASR_SustainOffset * 2U, ((uint)instr.DASR_ReleaseOffset - instr.DASR_SustainOffset) * 2U);
								voiceInfo.NoteStartFlag = 2;
							}
						}
					 }
					 else
						 doReleaseSkip = true;
				}

				if (doReleaseSkip)
				{
					if (voiceInfo.EnvelopeState == EnvelopeState.Decay)
					{
						voiceInfo.EnvelopeCounter += (byte)(64 - instr.SustainLevel);

						for (;;)
						{
							if (voiceInfo.EnvelopeCounter < instr.DecayTime)
								break;

							voiceInfo.EnvelopeCounter -= instr.DecayTime;
							voiceInfo.CurrentVolume--;

							if (voiceInfo.CurrentVolume == 0)
								break;
						}

						if (voiceInfo.CurrentVolume <= instr.SustainLevel)
							voiceInfo.EnvelopeState = EnvelopeState.Sustain;
					}
					else if (voiceInfo.EnvelopeState == EnvelopeState.Attack)
					{
						byte level = 64;

						if (instr.DecayTime == 0)
							level = instr.SustainLevel;

						voiceInfo.EnvelopeCounter += level;

						if (instr.AttackTime != 0)
						{
							while (voiceInfo.EnvelopeCounter >= instr.AttackTime)
							{
								voiceInfo.EnvelopeCounter -= instr.AttackTime;
								voiceInfo.CurrentVolume++;
							}
						}

						if (voiceInfo.CurrentVolume == level)
						{
							if (instr.DecayTime == 0)
								voiceInfo.EnvelopeState = EnvelopeState.Sustain;
							else
							{
								voiceInfo.EnvelopeCounter = 0;

								if (instr.SustainLevel == 64)
									voiceInfo.EnvelopeState = EnvelopeState.Sustain;
								else
									voiceInfo.EnvelopeState = EnvelopeState.Decay;
							}
						}
					}
				}
			}

			bool mixFlag = false;

			if (instrFlag.HasFlag(InstrumentFlag.Phasing))
			{
				voiceInfo.PhasingCounter--;

				if (voiceInfo.PhasingCounter == 0)
				{
					voiceInfo.PhasingCounter = instr.PhasingSpeed;

					short relative = (short)(voiceInfo.PhasingRelative + voiceInfo.PhasingStep);
					voiceInfo.PhasingRelative = (sbyte)relative;

					if ((relative < 0) || (relative >= instr.PhasingEnd) || (relative <= instr.PhasingStart))
						voiceInfo.PhasingStep = (sbyte)(-voiceInfo.PhasingStep);

					mixFlag = true;
				}
			}

			if (instrFlag.HasFlag(InstrumentFlag.Filter))
			{
				voiceInfo.FilterCounter--;

				if (voiceInfo.FilterCounter == 0)
				{
					voiceInfo.FilterCounter = instr.FilterSpeed;

					voiceInfo.FilterRelative = (byte)(voiceInfo.FilterRelative + voiceInfo.FilterStep);

					if ((voiceInfo.FilterRelative == instr.FilterFrequency) || (voiceInfo.FilterRelative == instr.FilterEnd))
						voiceInfo.FilterStep = (sbyte)(-voiceInfo.FilterStep);

					mixFlag = true;
				}
			}

			if (mixFlag)
			{
				if (instrFlag.HasFlag(InstrumentFlag.Phasing))
					Mix(voiceInfo, instr);

				if (instrFlag.HasFlag(InstrumentFlag.Filter))
					Filter(voiceInfo, instr);
			}

			InHardware(voiceInfo, channel, instrFlag);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the audio "hardware"
		/// </summary>
		/********************************************************************/
		private void InHardware(VoiceInfo voiceInfo, IChannel channel, InstrumentFlag instrFlag)
		{
			ushort volume = voiceInfo.CurrentVolume;

			if (instrFlag.HasFlag(InstrumentFlag.Tremolo) && (voiceInfo.TremoloVolume != 0))
				volume = (ushort)((volume * voiceInfo.TremoloVolume) / 256);

			if (voiceInfo.Volume != 0)
				volume = (ushort)((volume * voiceInfo.Volume) / 256);

			if ((playingInfo.FadeOutFlag || playingInfo.FadeInFlag) && (playingInfo.FadeOutVolume != 0))
				volume = (ushort)((volume * playingInfo.FadeOutVolume) / 256);

			channel.SetAmigaVolume(volume);

			ushort period = instrFlag.HasFlag(InstrumentFlag.Portamento) ? voiceInfo.ActivePeriod : voiceInfo.Period;

			if (instrFlag.HasFlag(InstrumentFlag.Vibrato) && (voiceInfo.VibratoDelay == 0))
				period = (ushort)(period + voiceInfo.VibratoRelative);

			if (instrFlag.HasFlag(InstrumentFlag.Arpeggio) && voiceInfo.ArpeggioFlag)
				period /= 2;

			period += voiceInfo.FineTune;

			channel.SetAmigaPeriod(period);
		}



		/********************************************************************/
		/// <summary>
		/// Tell NostalgicPlayer that the given channel has ended
		/// </summary>
		/********************************************************************/
		private void EndChannel(int channelNumber)
		{
			OnEndReached(channelNumber);

			if (HasEndReached)
			{
				long restartTime = long.MaxValue;

				for (int i = 0; i < 4; i++)
					restartTime = Math.Min(restartTime, opcodeTimes[voices[i].CurrentPosition].Ticks);

				SetRestartTime(new TimeSpan(restartTime));
			}
		}
		#endregion
	}
}
