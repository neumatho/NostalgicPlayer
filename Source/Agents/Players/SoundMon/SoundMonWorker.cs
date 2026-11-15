/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.SoundMon.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.SoundMon
{
	/// <summary>
	/// Main worker class.
	///
	/// The player mimics the original players, which have small
	/// differences between each version:
	///
	/// 1.0 - SoundMon_v1_0.S
	/// 1.1/2.0 - SoundMon_PCRel.s and SoundMon_2_0.s, which plays exactly the same
	/// 2.1 - SoundMon_v2_1.S
	/// 2.2 - SoundMon_v2_2.s
	///
	/// Note that all the original players play voice 0 in the step list in
	/// channel 3, voice 1 in channel 2 and so on. This is done in this
	/// player too, by storing the steps in reverse order when loading
	/// </summary>
	internal class SoundMonWorker : ModulePlayerWithPositionDurationAgentBase
	{
		private readonly ModuleType currentModuleType;

		private string moduleName;
		private byte waveNum;
		private ushort stepNum;
		private ushort trackNum;

		private Instrument[] instruments;
		private Step[][] steps;
		private Track[][] tracks;
		private sbyte[] waveTables;

		private sbyte byteBeforeWaveTables;
		private sbyte[] dataAfterWaveTables;

		private GlobalPlayingInfo playingInfo;
		private BpCurrent[] bpCurrent;

		private bool endReached;

		private const int InfoPositionLine = 4;
		private const int InfoTrackLine = 5;
		private const int InfoSpeedLine = 6;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SoundMonWorker(ModuleType moduleType = ModuleType.Unknown)
		{
			currentModuleType = moduleType;
		}

		#region Identify
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => SoundMonIdentifier.FileExtensions;



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
				Encoding encoder = EncoderCollection.Amiga;

				// Start to read the module name
				moduleStream.Seek(0, SeekOrigin.Begin);
				moduleName = moduleStream.ReadString(encoder, 26);

				if (currentModuleType == ModuleType.SoundMon10)
				{
					// Skip the mark
					moduleStream.Seek(4, SeekOrigin.Current);

					// Version 1.0 don't have waveforms
					waveNum = 0;
				}
				else
				{
					// Skip the mark
					moduleStream.Seek(3, SeekOrigin.Current);

					// Get the number of waveforms
					waveNum = moduleStream.Read_UINT8();
				}

				// Get the number of positions
				stepNum = moduleStream.Read_B_UINT16();

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_BP_ERR_LOADING_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				// Read the sample information
				instruments = new Instrument[15];

				for (int i = 0; i < 15; i++)
				{
					// First find out the instrument kind
					if (currentModuleType != ModuleType.SoundMon10)
					{
						if (moduleStream.Read_UINT8() == 0xff)
						{
							// Synth instrument
							SynthInstrument inst = new SynthInstrument();
							instruments[i] = inst;

							if (currentModuleType == ModuleType.SoundMon22)
							{
								inst.WaveTable = moduleStream.Read_UINT8();
								inst.WaveLength = (ushort)(moduleStream.Read_B_UINT16() * 2);
								inst.AdsrControl = moduleStream.Read_UINT8();
								inst.AdsrTable = moduleStream.Read_UINT8();
								inst.AdsrLength = moduleStream.Read_B_UINT16();
								inst.AdsrSpeed = moduleStream.Read_UINT8();
								inst.LfoControl = moduleStream.Read_UINT8();
								inst.LfoTable = moduleStream.Read_UINT8();
								inst.LfoDepth = moduleStream.Read_UINT8();
								inst.LfoLength = moduleStream.Read_B_UINT16();
								inst.LfoDelay = moduleStream.Read_UINT8();
								inst.LfoSpeed = moduleStream.Read_UINT8();
								inst.EgControl = moduleStream.Read_UINT8();
								inst.EgTable = moduleStream.Read_UINT8();
								inst.EgInitValue = 0;
								inst.EgLength = moduleStream.Read_B_UINT16();
								inst.EgDelay = moduleStream.Read_UINT8();
								inst.EgSpeed = moduleStream.Read_UINT8();
								inst.FxControl = moduleStream.Read_UINT8();
								inst.FxSpeed = moduleStream.Read_UINT8();
								inst.FxDelay = moduleStream.Read_UINT8();
								inst.ModControl = moduleStream.Read_UINT8();
								inst.ModTable = moduleStream.Read_UINT8();
								inst.ModSpeed = moduleStream.Read_UINT8();
								inst.ModDelay = moduleStream.Read_UINT8();
								inst.Volume = moduleStream.Read_UINT8();
								inst.ModLength = moduleStream.Read_B_UINT16();
							}
							else
							{
								inst.WaveTable = moduleStream.Read_UINT8();
								inst.WaveLength = (ushort)(moduleStream.Read_B_UINT16() * 2);
								inst.AdsrControl = moduleStream.Read_UINT8();
								inst.AdsrTable = moduleStream.Read_UINT8();
								inst.AdsrLength = moduleStream.Read_B_UINT16();
								inst.AdsrSpeed = moduleStream.Read_UINT8();
								inst.LfoControl = moduleStream.Read_UINT8();
								inst.LfoTable = moduleStream.Read_UINT8();
								inst.LfoDepth = moduleStream.Read_UINT8();
								inst.LfoLength = moduleStream.Read_B_UINT16();
								inst.LfoDelay = moduleStream.Read_B_UINT16();
								inst.LfoSpeed = moduleStream.Read_UINT8();
								inst.EgControl = moduleStream.Read_UINT8();
								inst.EgTable = moduleStream.Read_UINT8();
								inst.EgInitValue = moduleStream.Read_UINT8();
								inst.EgLength = moduleStream.Read_B_UINT16();
								inst.EgDelay = moduleStream.Read_B_UINT16();
								inst.EgSpeed = moduleStream.Read_UINT8();
								inst.FxControl = 0;
								inst.FxSpeed = 1;
								inst.FxDelay = 0;
								inst.ModControl = 0;
								inst.ModTable = 0;
								inst.ModSpeed = 1;
								inst.ModDelay = 0;
								inst.Volume = moduleStream.Read_UINT8();
								inst.ModLength = 0;
								moduleStream.Seek(6, SeekOrigin.Current);
							}

							// Check for "end-of-file"
							if (moduleStream.EndOfStream)
							{
								errorMessage = Resources.IDS_BP_ERR_LOADING_INSTINFO;
								Cleanup();

								return AgentResult.Error;
							}

							continue;
						}

						// Sample, seek back
						moduleStream.Seek(-1, SeekOrigin.Current);
					}

					// Sample instrument
					SampleInstrument sampInst = new SampleInstrument();
					instruments[i] = sampInst;

					sampInst.Name = moduleStream.ReadString(encoder, 24).TrimEnd();
					sampInst.Length = (ushort)(moduleStream.Read_B_UINT16() * 2);
					sampInst.LoopStart = moduleStream.Read_B_UINT16();
					sampInst.LoopLength = (ushort)(moduleStream.Read_B_UINT16() * 2);
					sampInst.Volume = moduleStream.Read_B_UINT16();
					sampInst.Adr = null;

					// Fix for Karate.bp
					if ((sampInst.LoopStart + sampInst.LoopLength) > sampInst.Length)
						sampInst.LoopLength = (ushort)(sampInst.Length - sampInst.LoopStart);

					// Check for "end-of-file"
					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_BP_ERR_LOADING_INSTINFO;
						Cleanup();

						return AgentResult.Error;
					}
				}

				// Allocate step structures
				steps = new Step[4][];

				for (int i = 0; i < 4; i++)
					steps[i] = ArrayHelper.InitializeArray<Step>(stepNum);

				// Read the step data
				trackNum = 0;

				for (int i = 0; i < stepNum; i++)
				{
					// The first voice in the step is played in the last
					// channel and so on, so store them in reverse order
					for (int j = 3; j >= 0; j--)
					{
						// Read track
						steps[j][i].TrackNumber = moduleStream.Read_B_UINT16();
						steps[j][i].SoundTranspose = moduleStream.Read_INT8();
						steps[j][i].Transpose = moduleStream.Read_INT8();

						if (steps[j][i].TrackNumber > trackNum)
							trackNum = steps[j][i].TrackNumber;
					}

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_BP_ERR_LOADING_HEADER;
						Cleanup();

						return AgentResult.Error;
					}
				}

				// Allocate space to hold the tracks
				tracks = new Track[trackNum][];

				// Read the tracks
				for (int i = 0; i < trackNum; i++)
				{
					// Allocate one track
					tracks[i] = ArrayHelper.InitializeArray<Track>(16);

					// Read it
					for (int j = 0; j < 16; j++)
					{
						tracks[i][j].Note = moduleStream.Read_INT8();
						tracks[i][j].Instrument = moduleStream.Read_UINT8();
						tracks[i][j].Optional = (Optional)(tracks[i][j].Instrument & 0x0f);
						tracks[i][j].Instrument = (byte)((tracks[i][j].Instrument & 0xf0) >> 4);
						tracks[i][j].OptionalData = moduleStream.Read_UINT8();
					}

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_BP_ERR_LOADING_TRACKS;
						Cleanup();

						return AgentResult.Error;
					}
				}

				// The original players read the wave tables directly in the
				// module memory, which means that e.g. the averaging effect
				// reads the byte just before the first wave table, which is
				// the last byte in the tracks
				byteBeforeWaveTables = trackNum > 0 ? (sbyte)tracks[trackNum - 1][15].OptionalData : (sbyte)0;

				if (currentModuleType != ModuleType.SoundMon10)
				{
					// Allocate space to hold the wave tables
					waveTables = new sbyte[waveNum * 64];

					// Read the wave tables
					moduleStream.ReadSigned(waveTables, 0, waveNum * 64);

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_BP_ERR_LOADING_SAMPLES;
						Cleanup();

						return AgentResult.Error;
					}
				}

				// Okay, finally we read the samples
				List<sbyte> afterWaveTables = new List<sbyte>();

				for (int i = 0; i < 15; i++)
				{
					// Is the instrument a sample?
					if (instruments[i] is SampleInstrument inst)
					{
						// Yes, check the length
						if (inst.Length != 0)
						{
							// Read the sample data
							inst.Adr = moduleStream.ReadSampleData(inst.Length, out int _);

							if (moduleStream.EndOfStream)
							{
								errorMessage = Resources.IDS_BP_ERR_LOADING_SAMPLES;
								Cleanup();

								return AgentResult.Error;
							}

							afterWaveTables.AddRange(inst.Adr);
						}
					}
				}

				// Remember the data after the wave tables, if some
				// synth instrument read beyond the last wave table
				dataAfterWaveTables = afterWaveTables.ToArray();
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

			InitializeSound(0);

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
			// Call the player
			if ((currentModuleType == ModuleType.SoundMon10) || (currentModuleType == ModuleType.SoundMon11))
				BpMusic11();
			else
				BpMusic21();

			if (endReached)
			{
				OnEndReached(playingInfo.BpStep);
				endReached = false;

				MarkPositionAsVisited(playingInfo.BpStep);
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

				for (int j = 0; j < 7 * 12; j++)
					frequencies[(1 * 12) + j] = PeriodToFrequency(Tables.Periods[j]);

				for (int i = 0; i < 15; i++)
				{
					Instrument inst = instruments[i];

					SampleInfo sampleInfo = new SampleInfo
					{
						Flags = SampleInfo.SampleFlag.None,
						Volume = (ushort)(Math.Min((int)inst.Volume, 64) * 4),
						Panning = -1,
						NoteFrequencies = frequencies
					};

					if (inst is SampleInstrument sampleInst)
					{
						sampleInfo.Type = SampleInfo.SampleType.Sample;
						sampleInfo.Name = sampleInst.Name;
						sampleInfo.Sample = sampleInst.Adr;
						sampleInfo.Length = sampleInst.Length > 2 ? sampleInst.Length : 0U;

						if (sampleInst.LoopLength <= 2)
						{
							// No loop
							sampleInfo.LoopStart = 0;
							sampleInfo.LoopLength = 0;
						}
						else
						{
							// Sample loops
							sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;
							sampleInfo.LoopStart = sampleInst.LoopStart;
							sampleInfo.LoopLength = sampleInst.LoopLength;
						}
					}
					else
					{
						sampleInfo.Type = SampleInfo.SampleType.Synthesis;
						sampleInfo.Name = string.Empty;
						sampleInfo.Sample = null;
						sampleInfo.Length = 0;
						sampleInfo.LoopStart = 0;
						sampleInfo.LoopLength = 0;
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
					description = Resources.IDS_BP_INFODESCLINE0;
					value = stepNum.ToString();
					break;
				}

				// Used tracks
				case 1:
				{
					description = Resources.IDS_BP_INFODESCLINE1;
					value = trackNum.ToString();
					break;
				}

				// Supported / used samples
				case 2:
				{
					description = Resources.IDS_BP_INFODESCLINE2;
					value = "15";
					break;
				}

				// Used wave tables
				case 3:
				{
					description = Resources.IDS_BP_INFODESCLINE3;
					value = waveNum.ToString();
					break;
				}

				// Playing position
				case 4:
				{
					description = Resources.IDS_BP_INFODESCLINE4;
					value = playingInfo.BpStep.ToString();
					break;
				}

				// Playing tracks
				case 5:
				{
					description = Resources.IDS_BP_INFODESCLINE5;
					value = FormatTracks();
					break;
				}

				// Current speed
				case 6:
				{
					description = Resources.IDS_BP_INFODESCLINE6;
					value = playingInfo.BpDelay.ToString();
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
		protected override int InitDuration(int songNumber, int startPosition)
		{
			InitializeSound(startPosition);
			MarkPositionAsVisited(startPosition);

			return startPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Return the total number of positions
		/// </summary>
		/********************************************************************/
		protected override int GetTotalNumberOfPositions()
		{
			return stepNum;
		}



		/********************************************************************/
		/// <summary>
		/// Create a snapshot of all the internal structures and return it
		/// </summary>
		/********************************************************************/
		protected override ISnapshot CreateSnapshot()
		{
			return new Snapshot(playingInfo, bpCurrent);
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
			bpCurrent = clonedSnapshot.Channels;

			UpdateModuleInformation();

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			instruments = null;
			steps = null;
			tracks = null;
			waveTables = null;
			dataAfterWaveTables = null;

			playingInfo = null;
			bpCurrent = null;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound(int startPosition)
		{
			// Initialize member variables
			playingInfo = new GlobalPlayingInfo
			{
				ArpCount = 1,
				BpCount = 1,
				BpDelay = 6,
				BpRepCount = 1,
				RepeatActive = false,
				VibIndex = 0,
				BpStep = (ushort)startPosition,
				BpPatCount = 0,
				St = 0,
				Tr = 0,
				NextStepFlag = false,
				NextStep = 0
			};

			endReached = false;

			// Initialize the BpCurrent structure for each channel
			bpCurrent = ArrayHelper.InitializeArray<BpCurrent>(4);

			for (int i = 0; i < 4; i++)
			{
				bpCurrent[i].SynthOffset = -1;
				bpCurrent[i].PlayLength = 1;
			}

			if (currentModuleType != ModuleType.SoundMon10)
			{
				// Allocate the temporary synth data buffer
				playingInfo.SynthBuffer = new sbyte[4][];
				playingInfo.SynthBuffer[0] = new sbyte[32];
				playingInfo.SynthBuffer[1] = new sbyte[32];
				playingInfo.SynthBuffer[2] = new sbyte[32];
				playingInfo.SynthBuffer[3] = new sbyte[32];

				// Make a copy of the wave tables
				playingInfo.WaveTables = ArrayHelper.CloneArray(waveTables);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Plays the module the SoundMon 1.0 and 1.1/2.0 way
		/// </summary>
		/********************************************************************/
		private void BpMusic11()
		{
			// Version 1.1/2.0 runs the synth effects before everything else
			if (currentModuleType == ModuleType.SoundMon11)
				BpSynth();

			playingInfo.ArpCount--;

			for (int i = 0; i < 4; i++)
			{
				BpCurrent cur = bpCurrent[i];

				// Auto slide
				cur.Period = (ushort)(cur.Period + cur.AutoSlide);

				// When the LFO is running, it has already set the period
				if (cur.LfoControl == 0)
					VirtualChannels[i].SetAmigaPeriod(cur.Period);

				// Arpeggio
				if ((cur.ArpValue != 0) || (cur.AutoArp != 0))
					DoArpeggio(i, cur);
			}

			// The arpeggio runs in a cycle of 3
			if (playingInfo.ArpCount == 0)
				playingInfo.ArpCount = 3;

			playingInfo.BpCount--;
			if (playingInfo.BpCount == 0)
			{
				playingInfo.BpCount = playingInfo.BpDelay;
				BpNext();

				for (int i = 0; i < 4; i++)
				{
					if (bpCurrent[i].Restart)
						PlayIt(i);
				}

				StopVoicesWithoutRetrig();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Plays the module the SoundMon 2.1 and 2.2 way
		/// </summary>
		/********************************************************************/
		private void BpMusic21()
		{
			// The arpeggio runs in a cycle of 4
			playingInfo.ArpCount = (byte)((playingInfo.ArpCount - 1) & 3);

			// Adjust the vibrato table index
			playingInfo.VibIndex = (byte)((playingInfo.VibIndex + 1) & 7);

			for (int i = 0; i < 4; i++)
			{
				BpCurrent cur = bpCurrent[i];

				// Auto slide
				cur.Period = (ushort)(cur.Period + cur.AutoSlide);

				// Vibrato
				if (cur.Vibrato != 0)
					VirtualChannels[i].SetAmigaPeriod((ushort)(cur.Period + (Tables.VibratoTable[playingInfo.VibIndex] / cur.Vibrato)));
				else
					VirtualChannels[i].SetAmigaPeriod(cur.Period);

				// Arpeggio
				if ((cur.ArpValue != 0) || (cur.AutoArp != 0))
					DoArpeggio(i, cur);
			}

			// The synth effects are running after the period has been set,
			// which means the LFO overrules the period only on the ticks it
			// is updated
			BpSynth();

			playingInfo.BpCount--;
			if (playingInfo.BpCount == 0)
			{
				playingInfo.BpCount = playingInfo.BpDelay;
				BpNext();

				// Copy temporary synth data back for all retrigged voices
				// before any of the voices are retrigged
				for (int i = 0; i < 4; i++)
				{
					BpCurrent cur = bpCurrent[i];

					if (cur.Restart && (cur.SynthOffset >= 0))
					{
						RestoreSynthBuffer(i);

						if (currentModuleType == ModuleType.SoundMon22)
							cur.SynthOffset = -1;
					}
				}

				for (int i = 0; i < 4; i++)
				{
					if (bpCurrent[i].Restart)
						PlayIt(i);
				}

				StopVoicesWithoutRetrig();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Change the pattern position and/or step position
		/// </summary>
		/********************************************************************/
		private void BpNext()
		{
			bool isVersion1 = (currentModuleType == ModuleType.SoundMon10) || (currentModuleType == ModuleType.SoundMon11);
			bool positionChanged = false;

			// Find out from which effect the voice is not retrigged
			Optional noRetrigFrom = currentModuleType == ModuleType.SoundMon22 ? Optional.ChangeInversion : currentModuleType == ModuleType.SoundMon21 ? Optional.ResetAdsr : (Optional)0x10;

			for (int i = 0; i < 4; i++)
			{
				BpCurrent cur = bpCurrent[i];

				// Get the step information. Note that in version 1.0 and 1.1/2.0, the
				// step can be changed in the middle of the loop
				Step step = steps[i][playingInfo.BpStep];
				playingInfo.St = step.SoundTranspose;
				playingInfo.Tr = step.Transpose;

				// Find the track
				Track curTrack = tracks[step.TrackNumber - 1][playingInfo.BpPatCount];

				// Is there any note?
				sbyte note = curTrack.Note;
				if (note != 0)
				{
					// Stop the effects
					cur.AutoSlide = 0;
					cur.AutoArp = 0;

					if (currentModuleType == ModuleType.SoundMon22)
						cur.Vibrato = 0;

					// Find the note number and period
					if ((curTrack.Optional != Optional.Transpose) || ((curTrack.OptionalData & 0xf0) == 0))
						note += playingInfo.Tr;

					cur.Note = (byte)note;
					cur.Period = GetPeriod(note);

					// Should the voice be retrigged?
					if (curTrack.Optional < noRetrigFrom)
					{
						cur.Restart = true;
						cur.Volume = 0xff;
					}

					// Find the instrument
					byte inst = curTrack.Instrument;
					if (inst == 0)
						inst = cur.Instrument;

					if ((inst != 0) && ((curTrack.Optional != Optional.Transpose) || ((curTrack.OptionalData & 0x0f) == 0)))
					{
						inst += (byte)playingInfo.St;
						if ((inst < 1) || (inst > 15))
							inst -= (byte)playingInfo.St;
					}

					// Should the sample be restarted?
					bool restartSample;

					if (isVersion1)
						restartSample = (cur.PlayLength == 1) || (cur.Instrument != inst);
					else if ((currentModuleType == ModuleType.SoundMon22) && (curTrack.Optional >= Optional.ChangeInversion))
						restartSample = false;
					else
						restartSample = !cur.SynthMode || (cur.Instrument != inst);

					if (restartSample)
					{
						cur.Instrument = inst;
						cur.DmaRestart = true;
					}
				}

				if (DoOptionals(i, curTrack.Optional, curTrack.OptionalData))
					positionChanged = true;
			}

			// Change the position
			if (playingInfo.NextStepFlag)
			{
				playingInfo.NextStepFlag = false;
				playingInfo.BpPatCount = 0;

				// Only the low byte of the step is changed
				playingInfo.BpStep = (ushort)((playingInfo.BpStep & 0xff00) | playingInfo.NextStep);

				// The original player does not check the new position, so
				// a jump beyond the end of the module (e.g. Super Silly.bp)
				// will play garbage. Start over instead
				if (playingInfo.BpStep >= stepNum)
					playingInfo.BpStep = 0;

				positionChanged = true;
			}
			else
			{
				// Next row in the pattern
				playingInfo.BpPatCount++;
				if (playingInfo.BpPatCount == 16)
				{
					// Done with the pattern, now go to the next step
					playingInfo.BpPatCount = 0;
					playingInfo.BpStep++;

					if (playingInfo.BpStep == stepNum)
					{
						// Done with the module, repeat it
						playingInfo.BpStep = 0;
					}

					positionChanged = true;
				}
			}

			if (positionChanged)
			{
				ShowSongPosition();
				ShowTracks();

				// While a repeat loop is running, the same positions are played
				// several times, so do not check for the end of the module
				if (!playingInfo.RepeatActive)
				{
					if (HasPositionBeenVisited(playingInfo.BpStep))
						endReached = true;

					MarkPositionAsVisited(playingInfo.BpStep);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parses the track optionals. Returns true if the position has
		/// been changed directly
		/// </summary>
		/********************************************************************/
		private bool DoOptionals(int voice, Optional optional, byte optionalData)
		{
			BpCurrent cur = bpCurrent[voice];
			bool isVersion1 = (currentModuleType == ModuleType.SoundMon10) || (currentModuleType == ModuleType.SoundMon11);

			switch (optional)
			{
				// Arpeggio once
				case Optional.ArpeggioOnce:
				{
					cur.ArpValue = optionalData;
					break;
				}

				// Set volume
				case Optional.SetVolume:
				{
					cur.Volume = optionalData;

					// Version 2.2 does not change the volume on synth sounds,
					// since the ADSR takes care of it
					if ((currentModuleType != ModuleType.SoundMon22) || !cur.SynthMode)
						SetVolume(voice, optionalData);

					break;
				}

				// Set speed
				case Optional.SetSpeed:
				{
					playingInfo.BpCount = optionalData;
					playingInfo.BpDelay = optionalData;

					ShowSpeed();
					break;
				}

				// Filter control
				case Optional.Filter:
				{
					AmigaFilter = optionalData != 0;
					break;
				}

				// Period lift up
				case Optional.PortUp:
				{
					cur.Period -= optionalData;
					cur.ArpValue = 0;

					// Version 1.0 set the new period right away
					if (currentModuleType == ModuleType.SoundMon10)
						VirtualChannels[voice].SetAmigaPeriod(cur.Period);

					break;
				}

				// Period lift down
				case Optional.PortDown:
				{
					cur.Period += optionalData;
					cur.ArpValue = 0;

					// Version 1.0 set the new period right away
					if (currentModuleType == ModuleType.SoundMon10)
						VirtualChannels[voice].SetAmigaPeriod(cur.Period);

					break;
				}

				// Set repeat count / Vibrato
				case Optional.SetRepCount:
				{
					if (isVersion1)
					{
						// If the count is set inside a running loop, the loop
						// will never end
						if (playingInfo.RepeatActive)
						{
							playingInfo.RepeatActive = false;
							endReached = true;
						}

						playingInfo.BpRepCount = optionalData;
					}
					else
						cur.Vibrato = optionalData;

					break;
				}

				// DBRA repeat count / Jump to step
				case Optional.DbraRepCount:
				{
					if (isVersion1)
					{
						// The step is changed right away, so the rest of the
						// voices will read the new step. The row counter is not
						// reset, and if this is the last row in the track, the
						// step will be incremented afterwards
						playingInfo.BpRepCount--;

						if (playingInfo.BpRepCount != 0)
						{
							playingInfo.BpStep = optionalData;

							// A small count is a loop, so the end check should
							// be disabled while it runs. Larger counts are seen
							// as an endless jump
							playingInfo.RepeatActive = playingInfo.BpRepCount < 15;

							return true;
						}

						playingInfo.RepeatActive = false;
					}
					else
					{
						playingInfo.NextStep = optionalData;
						playingInfo.NextStepFlag = true;
					}

					break;
				}

				// Set auto slide
				case Optional.SetAutoSlide:
				{
					cur.AutoSlide = (sbyte)optionalData;
					break;
				}

				// Set continuous arpeggio
				case Optional.SetArpeggio:
				{
					cur.AutoArp = optionalData;

					if (!isVersion1)
						ResetAdsr(cur);

					break;
				}

				// Change fx type
				case Optional.ChangeFx:
				{
					if (currentModuleType == ModuleType.SoundMon22)
						cur.FxControl = optionalData;

					break;
				}

				// Changes from inversion to backward inversion (or vice versa) or
				// from transform to backward transform
				case Optional.ChangeInversion:
				{
					if (currentModuleType == ModuleType.SoundMon22)
					{
						cur.AutoArp = optionalData;
						cur.FxControl = (byte)(cur.FxControl ^ 1);

						ResetAdsr(cur);
					}

					break;
				}

				// Reset ADSR on synth sound, but not EG, averaging, transform etc.
				case Optional.ResetAdsr:
				{
					if (!isVersion1)
					{
						cur.AutoArp = optionalData;
						ResetAdsr(cur);
					}

					break;
				}

				// Same as above, but does not reset ADSR (just changes note)
				case Optional.ChangeNote:
				{
					if (!isVersion1)
						cur.AutoArp = optionalData;

					break;
				}
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Restart the ADSR
		/// </summary>
		/********************************************************************/
		private void ResetAdsr(BpCurrent cur)
		{
			cur.AdsrPtr = 0;

			if (cur.AdsrControl == 0)
				cur.AdsrControl = 1;
		}



		/********************************************************************/
		/// <summary>
		/// Handle the arpeggio
		/// </summary>
		/********************************************************************/
		private void DoArpeggio(int voice, BpCurrent cur)
		{
			int note = cur.Note;

			if (playingInfo.ArpCount == 0)
				note += ((cur.ArpValue & 0xf0) >> 4) + ((cur.AutoArp & 0xf0) >> 4);
			else if (playingInfo.ArpCount == 1)
				note += (cur.ArpValue & 0x0f) + (cur.AutoArp & 0x0f);

			ushort period = GetPeriod((sbyte)note);
			VirtualChannels[voice].SetAmigaPeriod(period);

			// Version 2.x also stores the period, so the auto slide and
			// vibrato continue from the arpeggio note
			if ((currentModuleType == ModuleType.SoundMon21) || (currentModuleType == ModuleType.SoundMon22))
				cur.Period = period;
		}



		/********************************************************************/
		/// <summary>
		/// Mute all voices that have been stopped, but not restarted
		/// </summary>
		/********************************************************************/
		private void StopVoicesWithoutRetrig()
		{
			for (int i = 0; i < 4; i++)
			{
				BpCurrent cur = bpCurrent[i];

				if (cur.DmaRestart)
				{
					// Only happens in version 2.1, when using effect E or F
					// on a sample or together with a new instrument. The
					// original player stops the voice, but never starts it
					// again
					cur.DmaRestart = false;
					VirtualChannels[i].Mute();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Retrigs a sample or synth sound
		/// </summary>
		/********************************************************************/
		private void PlayIt(int voice)
		{
			BpCurrent cur = bpCurrent[voice];

			// Reset the retrig flag
			cur.Restart = false;

			// Version 1.1/2.0 copies the temporary synth data back here
			if ((currentModuleType == ModuleType.SoundMon11) && (cur.SynthOffset >= 0))
				RestoreSynthBuffer(voice);

			VirtualChannels[voice].SetAmigaPeriod(cur.Period);

			// Get the instrument address
			if (cur.Instrument == 0)
				return;

			Instrument inst = instruments[cur.Instrument - 1];

			if (inst is SynthInstrument synthInst)
			{
				if (currentModuleType == ModuleType.SoundMon22)
					PlaySynth22(voice, cur, synthInst);
				else
					PlaySynth(voice, cur, synthInst);
			}
			else
				PlaySample(voice, cur, (SampleInstrument)inst);

			cur.DmaRestart = false;
		}



		/********************************************************************/
		/// <summary>
		/// Retrigs a sample
		/// </summary>
		/********************************************************************/
		private void PlaySample(int voice, BpCurrent cur, SampleInstrument sampleInst)
		{
			IChannel channel = VirtualChannels[voice];

			if (currentModuleType != ModuleType.SoundMon10)
			{
				cur.SynthMode = false;
				cur.LfoControl = 0;

				if (currentModuleType != ModuleType.SoundMon22)
				{
					cur.AdsrControl = 0;
					cur.SynthOffset = -1;
				}
			}

			bool hasLoop = sampleInst.LoopLength > 2;
			cur.PlayLength = hasLoop ? (ushort)(sampleInst.LoopLength / 2) : (ushort)1;

			if (sampleInst.Adr == null)
			{
				channel.Mute();
				return;
			}

			// Only start the sample, if the voice has been stopped. If not,
			// the sample just continues
			if (cur.DmaRestart)
			{
				short sampleNumber = (short)(cur.Instrument - 1);

				if (!hasLoop)
					channel.PlaySample(sampleNumber, sampleInst.Adr, 0, sampleInst.Length);
				else
				{
					if ((currentModuleType == ModuleType.SoundMon10) || (currentModuleType == ModuleType.SoundMon11))
					{
						// Version 1.0 and 1.1/2.0 only plays the loop part of the sample
						channel.PlaySample(sampleNumber, sampleInst.Adr, sampleInst.LoopStart, sampleInst.LoopLength);
					}
					else
					{
						// Version 2.x plays from the start of the sample
						// to the end of the loop
						channel.PlaySample(sampleNumber, sampleInst.Adr, 0, (uint)(sampleInst.LoopStart + sampleInst.LoopLength));
					}

					channel.SetLoop(sampleInst.LoopStart, sampleInst.LoopLength);
				}
			}

			// Set the volume
			SetVolume(voice, cur.Volume == 0xff ? sampleInst.Volume : cur.Volume);
		}



		/********************************************************************/
		/// <summary>
		/// Retrigs a synth sound the SoundMon 1.1/2.0 and 2.1 way
		/// </summary>
		/********************************************************************/
		private void PlaySynth(int voice, BpCurrent cur, SynthInstrument synthInst)
		{
			cur.SynthMode = true;
			cur.EgPtr = 0;
			cur.LfoPtr = 0;
			cur.AdsrPtr = 0;

			cur.EgCount = (ushort)(synthInst.EgDelay + 1);
			cur.LfoCount = (ushort)(synthInst.LfoDelay + 1);
			cur.AdsrCount = 1;							// Start immediate

			cur.EgControl = synthInst.EgControl;
			cur.LfoControl = synthInst.LfoControl;
			cur.AdsrControl = synthInst.AdsrControl;
			cur.OldEgValue = synthInst.EgInitValue;

			// Play the synth sound
			int waveOffset = synthInst.WaveTable * 64;
			StartSynthSound(voice, cur, synthInst, waveOffset);

			// Initialize ADSR
			InitializeSynthVolume(voice, cur, synthInst);

			// Version 1.1/2.0 always saves the waveform, while version 2.1
			// only saves it when the EG is used
			if ((currentModuleType == ModuleType.SoundMon11) || (synthInst.EgControl != 0))
				SaveSynthBuffer(voice, waveOffset);

			if (synthInst.EgControl != 0)
			{
				// Is there an initial value for the EG?
				if (synthInst.EgInitValue != 0)
				{
					int count = synthInst.EgInitValue / 8;
					cur.OldEgValue = (byte)count;

					for (int i = 0; i < count; i++)
						playingInfo.WaveTables[waveOffset + i] = (sbyte)-GetTableValue(waveOffset + i);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Retrigs a synth sound the SoundMon 2.2 way
		/// </summary>
		/********************************************************************/
		private void PlaySynth22(int voice, BpCurrent cur, SynthInstrument synthInst)
		{
			cur.SynthMode = true;
			cur.EgPtr = 0;
			cur.LfoPtr = 0;
			cur.AdsrPtr = 0;
			cur.ModPtr = 0;

			cur.EgCount = (byte)(synthInst.EgDelay + 1);
			cur.LfoCount = (byte)(synthInst.LfoDelay + 1);
			cur.AdsrCount = 1;							// Start immediate
			cur.ModCount = (byte)(synthInst.ModDelay + 1);
			cur.FxCount = (byte)(synthInst.FxDelay + 1);

			cur.FxControl = synthInst.FxControl;
			cur.EgControl = synthInst.EgControl;
			cur.LfoControl = synthInst.LfoControl;
			cur.AdsrControl = synthInst.AdsrControl;
			cur.ModControl = synthInst.ModControl;
			cur.OldEgValue = 0;

			// Play the synth sound
			int waveOffset = synthInst.WaveTable * 64;
			StartSynthSound(voice, cur, synthInst, waveOffset);

			// Initialize ADSR
			InitializeSynthVolume(voice, cur, synthInst);

			// Initialize the other effects
			if ((cur.EgControl != 0) || (cur.ModControl != 0) || (cur.FxControl != 0))
				SaveSynthBuffer(voice, waveOffset);
		}



		/********************************************************************/
		/// <summary>
		/// Start to play the synth waveform, if the voice has been stopped
		/// </summary>
		/********************************************************************/
		private void StartSynthSound(int voice, BpCurrent cur, SynthInstrument synthInst, int waveOffset)
		{
			cur.PlayLength = (ushort)(synthInst.WaveLength / 2);

			if (cur.DmaRestart)
			{
				IChannel channel = VirtualChannels[voice];

				channel.PlaySample((short)(cur.Instrument - 1), playingInfo.WaveTables, (uint)waveOffset, synthInst.WaveLength);
				channel.SetLoop((uint)waveOffset, synthInst.WaveLength);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set the start volume of a synth sound
		/// </summary>
		/********************************************************************/
		private void InitializeSynthVolume(int voice, BpCurrent cur, SynthInstrument synthInst)
		{
			if (synthInst.AdsrControl != 0)
			{
				// Get table value
				int tableValue = (byte)(GetTableValue(synthInst.AdsrTable * 64) + 128) >> 2;

				if (cur.Volume == 0xff)
					cur.Volume = (byte)synthInst.Volume;

				SetVolume(voice, (tableValue * cur.Volume) >> 6);
			}
			else
				SetVolume(voice, cur.Volume == 0xff ? synthInst.Volume : cur.Volume);
		}



		/********************************************************************/
		/// <summary>
		/// Save the waveform in the synth buffer, so it can be restored
		/// </summary>
		/********************************************************************/
		private void SaveSynthBuffer(int voice, int waveOffset)
		{
			bpCurrent[voice].SynthOffset = waveOffset;

			sbyte[] buffer = playingInfo.SynthBuffer[voice];

			for (int i = 0; i < 32; i++)
				buffer[i] = GetTableValue(waveOffset + i);
		}



		/********************************************************************/
		/// <summary>
		/// Copy the saved waveform back
		/// </summary>
		/********************************************************************/
		private void RestoreSynthBuffer(int voice)
		{
			int waveOffset = bpCurrent[voice].SynthOffset;
			sbyte[] buffer = playingInfo.SynthBuffer[voice];

			for (int i = 0; i < 32; i++)
				playingInfo.WaveTables[waveOffset + i] = buffer[i];
		}



		/********************************************************************/
		/// <summary>
		/// Calculate and run synth effects
		/// </summary>
		/********************************************************************/
		private void BpSynth()
		{
			for (int i = 0; i < 4; i++)
			{
				BpCurrent cur = bpCurrent[i];

				// Do the current voice play a synth sample?
				if (cur.SynthMode)
				{
					// Yes, begin to do the synth effects
					//
					// Get the instrument
					if (instruments[cur.Instrument - 1] is SynthInstrument synthInst)
					{
						DoAdsr(i, cur, synthInst);
						DoLfo(i, cur, synthInst);

						if (currentModuleType == ModuleType.SoundMon22)
						{
							// Do we have a synth buffer?
							if (cur.SynthOffset >= 0)
							{
								DoEg(i, cur, synthInst);
								DoFx(i, cur, synthInst);
								DoMod(cur, synthInst);
							}
						}
						else
							DoEg(i, cur, synthInst);
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Decrement one of the synth counters. In version 2.2, they are
		/// bytes, while they are words in the older versions
		/// </summary>
		/********************************************************************/
		private ushort DecrementCounter(ushort counter)
		{
			if (currentModuleType == ModuleType.SoundMon22)
				return (byte)(counter - 1);

			return (ushort)(counter - 1);
		}



		/********************************************************************/
		/// <summary>
		/// Handle the ADSR
		/// </summary>
		/********************************************************************/
		private void DoAdsr(int voice, BpCurrent cur, SynthInstrument synthInst)
		{
			if (cur.AdsrControl != 0)
			{
				cur.AdsrCount = DecrementCounter(cur.AdsrCount);
				if (cur.AdsrCount == 0)
				{
					// Reset counter
					cur.AdsrCount = synthInst.AdsrSpeed;

					// Calculate new volume
					int tableValue = (byte)(GetTableValue((synthInst.AdsrTable * 64) + cur.AdsrPtr) + 128) >> 2;
					SetVolume(voice, (tableValue * cur.Volume) >> 6);

					// Update the ADSR pointer
					cur.AdsrPtr++;
					if (cur.AdsrPtr == synthInst.AdsrLength)
					{
						cur.AdsrPtr = 0;

						// Only do the ADSR once?
						if (cur.AdsrControl == 1)
							cur.AdsrControl = 0;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle the LFO
		/// </summary>
		/********************************************************************/
		private void DoLfo(int voice, BpCurrent cur, SynthInstrument synthInst)
		{
			if (cur.LfoControl != 0)
			{
				cur.LfoCount = DecrementCounter(cur.LfoCount);
				if (cur.LfoCount == 0)
				{
					// Reset counter
					cur.LfoCount = synthInst.LfoSpeed;

					// Get the wave table value
					int tableValue = GetTableValue((synthInst.LfoTable * 64) + cur.LfoPtr);

					// Adjust the value by the LFO depth
					if (synthInst.LfoDepth != 0)
						tableValue /= synthInst.LfoDepth;

					// Calculate and set the new period
					VirtualChannels[voice].SetAmigaPeriod((ushort)(cur.Period + tableValue));

					// Update the LFO pointer
					cur.LfoPtr++;
					if (cur.LfoPtr == synthInst.LfoLength)
					{
						cur.LfoPtr = 0;

						// Only do the LFO once?
						if (cur.LfoControl == 1)
							cur.LfoControl = 0;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle the EG
		/// </summary>
		/********************************************************************/
		private void DoEg(int voice, BpCurrent cur, SynthInstrument synthInst)
		{
			if (cur.EgControl != 0)
			{
				cur.EgCount = DecrementCounter(cur.EgCount);
				if ((cur.EgCount == 0) && (cur.SynthOffset >= 0))
				{
					// Reset counter
					cur.EgCount = synthInst.EgSpeed;

					// Calculate new EG value
					int oldValue = cur.OldEgValue;
					int newValue = (byte)(GetTableValue((synthInst.EgTable * 64) + cur.EgPtr) + 128) >> 3;
					cur.OldEgValue = (byte)newValue;

					// Do we need to do the EG thing at all?
					if (newValue != oldValue)
					{
						// Find the source and destination offset
						sbyte[] source = playingInfo.SynthBuffer[voice];
						int sourceOffset = oldValue;
						int destOffset = cur.SynthOffset + oldValue;

						if (newValue < oldValue)
						{
							// Copy the original waveform back
							for (int j = oldValue - newValue; j > 0; j--)
								playingInfo.WaveTables[--destOffset] = source[--sourceOffset];
						}
						else
						{
							// Negate the waveform
							for (int j = newValue - oldValue; j > 0; j--)
								playingInfo.WaveTables[destOffset++] = (sbyte)-source[sourceOffset++];
						}
					}

					// Update the EG pointer
					cur.EgPtr++;
					if (cur.EgPtr == synthInst.EgLength)
					{
						cur.EgPtr = 0;

						// Only do the EG once?
						if (cur.EgControl == 1)
							cur.EgControl = 0;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle the synth effects
		/// </summary>
		/********************************************************************/
		private void DoFx(int voice, BpCurrent cur, SynthInstrument synthInst)
		{
			switch (cur.FxControl)
			{
				case 1:
				{
					cur.FxCount--;
					if (cur.FxCount == 0)
					{
						// Reset counter
						cur.FxCount = synthInst.FxSpeed;
						Averaging(voice);
					}

					break;
				}

				case 2:
				{
					Transform2(voice, (sbyte)synthInst.FxSpeed);
					break;
				}

				case 3:
				case 5:
				{
					Transform3(voice, (sbyte)synthInst.FxSpeed);
					break;
				}

				case 4:
				{
					Transform4(voice, (sbyte)synthInst.FxSpeed);
					break;
				}

				case 6:
				{
					cur.FxCount--;
					if (cur.FxCount == 0)
					{
						cur.FxControl = 0;
						cur.FxCount = 1;

						for (int i = 0; i < 32; i++)
							playingInfo.WaveTables[cur.SynthOffset + i] = GetTableValue(cur.SynthOffset + 64 + i);
					}

					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle the MOD
		/// </summary>
		/********************************************************************/
		private void DoMod(BpCurrent cur, SynthInstrument synthInst)
		{
			if (cur.ModControl != 0)
			{
				cur.ModCount--;
				if (cur.ModCount == 0)
				{
					// Reset counter
					cur.ModCount = synthInst.ModSpeed;

					// Get the table value and store it in the synth sample
					playingInfo.WaveTables[cur.SynthOffset + 32] = GetTableValue((synthInst.ModTable * 64) + cur.ModPtr);

					// Update the MOD pointer
					cur.ModPtr++;
					if (cur.ModPtr == synthInst.ModLength)
					{
						cur.ModPtr = 0;

						// Only do the MOD once?
						if (cur.ModControl == 1)
							cur.ModControl = 0;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Averages the synth sample
		/// </summary>
		/********************************************************************/
		private void Averaging(int voice)
		{
			int synthOffset = bpCurrent[voice].SynthOffset;
			int lastVal = GetTableValue(synthOffset - 1);

			for (int i = 0; i < 32 / 2; i++)
			{
				int newVal = (GetTableValue(synthOffset + 1) + lastVal) >> 1;
				playingInfo.WaveTables[synthOffset++] = (sbyte)newVal;

				lastVal = (GetTableValue(synthOffset + 1) + newVal) >> 1;
				playingInfo.WaveTables[synthOffset++] = (sbyte)lastVal;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Transform the synth sample using method 2
		/// </summary>
		/********************************************************************/
		private void Transform2(int voice, sbyte delta)
		{
			sbyte[] source = playingInfo.SynthBuffer[voice];
			int sourceOffset = 31;
			int destOffset = bpCurrent[voice].SynthOffset;

			for (int i = 0; i < 32; i++)
			{
				TransformByte(source[sourceOffset], destOffset, delta);

				sourceOffset--;
				destOffset++;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Transform the synth sample using method 3
		/// </summary>
		/********************************************************************/
		private void Transform3(int voice, sbyte delta)
		{
			sbyte[] source = playingInfo.SynthBuffer[voice];
			int sourceOffset = 0;
			int destOffset = bpCurrent[voice].SynthOffset;

			for (int i = 0; i < 32; i++)
			{
				TransformByte(source[sourceOffset], destOffset, delta);

				sourceOffset++;
				destOffset++;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Transform the synth sample using method 4
		/// </summary>
		/********************************************************************/
		private void Transform4(int voice, sbyte delta)
		{
			int sourceOffset = bpCurrent[voice].SynthOffset + 64;
			int destOffset = bpCurrent[voice].SynthOffset;

			for (int i = 0; i < 32; i++)
			{
				TransformByte(GetTableValue(sourceOffset), destOffset, delta);

				sourceOffset++;
				destOffset++;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Move a single byte in the waveform towards the source value
		/// </summary>
		/********************************************************************/
		private void TransformByte(sbyte sourceVal, int destOffset, sbyte delta)
		{
			sbyte destVal = GetTableValue(destOffset);

			if (sourceVal < destVal)
				playingInfo.WaveTables[destOffset] = (sbyte)(destVal - delta);
			else if (sourceVal > destVal)
				playingInfo.WaveTables[destOffset] = (sbyte)(destVal + delta);
		}



		/********************************************************************/
		/// <summary>
		/// Return a value from the wave tables. The original players read
		/// directly in the module memory, so reading outside the tables is
		/// emulated as good as possible
		/// </summary>
		/********************************************************************/
		private sbyte GetTableValue(int offset)
		{
			sbyte[] tables = playingInfo.WaveTables;

			// The averaging effect on the first wave table reads the byte
			// before it (e.g. Aztec Challenge - Christmas.bp)
			if (offset < 0)
				return byteBeforeWaveTables;

			// The transform effect on the last wave table reads the table
			// after it, which is the sample data (e.g. Summertime.bp)
			if (offset >= tables.Length)
				return dataAfterWaveTables[offset - tables.Length];

			return tables[offset];
		}



		/********************************************************************/
		/// <summary>
		/// Return the period for the given note
		/// </summary>
		/********************************************************************/
		private ushort GetPeriod(int note)
		{
			return Tables.Periods[Math.Max(0, Math.Min(note + 36 - 1, Tables.Periods.Length - 1))];
		}



		/********************************************************************/
		/// <summary>
		/// Set the volume on the channel. The volume is the same as the
		/// Amiga hardware volume, which means it is clamped to 64
		/// </summary>
		/********************************************************************/
		private void SetVolume(int voice, int volume)
		{
			VirtualChannels[voice].SetAmigaVolume((ushort)Math.Min(volume, 64));
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current song position
		/// </summary>
		/********************************************************************/
		private void ShowSongPosition()
		{
			OnModuleInfoChanged(InfoPositionLine, playingInfo.BpStep.ToString());
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
			OnModuleInfoChanged(InfoSpeedLine, playingInfo.BpDelay.ToString());
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
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing tracks
		/// </summary>
		/********************************************************************/
		private string FormatTracks()
		{
			return FormatChannelTracks(4, i => steps[i][playingInfo.BpStep].TrackNumber);
		}
		#endregion
	}
}
