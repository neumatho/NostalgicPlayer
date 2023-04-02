/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.MikMod.Containers;
using Polycode.NostalgicPlayer.Agent.Player.MikMod.LibMikMod;
using Polycode.NostalgicPlayer.Agent.Shared.MikMod;
using Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Types;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.MikMod
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class MikModWorker : ModulePlayerWithPositionDurationAgentBase, IDriver
	{
		private Module of;

		private MPlayer player;

		private byte mdNumChn;

		private const int InfoPositionLine = 5;
		private const int InfoPatternLine = 6;
		private const int InfoTrackLine = 7;
		private const int InfoSpeedLine = 8;
		private const int InfoTempoLine = 9;

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => new string[0];



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			long fileSize = moduleStream.Length;
			if (fileSize < 25)
				return AgentResult.Unknown;

			// Check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint mark = moduleStream.Read_B_UINT32();
			ushort version = moduleStream.Read_B_UINT16();

			if ((mark != 0x4e50554e) || (version != 0x0100))		// NPUN
				return AgentResult.Unknown;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the module
		/// </summary>
		/********************************************************************/
		public override string ModuleName => of.SongName;



		/********************************************************************/
		/// <summary>
		/// Return the comment separated in lines
		/// </summary>
		/********************************************************************/
		public override string[] Comment => string.IsNullOrWhiteSpace(of.Comment) ? new string[0] : of.Comment.Split('\n');



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
					description = Resources.IDS_MIK_INFODESCLINE0;
					value = of.NumPos.ToString();
					break;
				}

				// Used patterns
				case 1:
				{
					description = Resources.IDS_MIK_INFODESCLINE1;
					value = of.NumPat.ToString();
					break;
				}

				// Used tracks
				case 2:
				{
					description = Resources.IDS_MIK_INFODESCLINE2;
					value = of.NumTrk.ToString();
					break;
				}

				// Used instruments
				case 3:
				{
					description = Resources.IDS_MIK_INFODESCLINE3;
					value = (of.Flags & ModuleFlag.Inst) != 0 ? of.NumIns.ToString() : "0";
					break;
				}

				// Used samples
				case 4:
				{
					description = Resources.IDS_MIK_INFODESCLINE4;
					value = of.NumSmp.ToString();
					break;
				}

				// Playing position
				case 5:
				{
					description = Resources.IDS_MIK_INFODESCLINE5;
					value = of.SngPos.ToString();
					break;
				}

				// Playing pattern
				case 6:
				{
					description = Resources.IDS_MIK_INFODESCLINE6;
					value = of.Positions[of.SngPos].ToString();
					break;
				}

				// Playing tracks
				case 7:
				{
					description = Resources.IDS_MIK_INFODESCLINE7;
					value = FormatTracks();
					break;
				}

				// Current speed
				case 8:
				{
					description = Resources.IDS_MIK_INFODESCLINE8;
					value = of.SngSpd.ToString();
					break;
				}

				// Current tempo (BPM)
				case 9:
				{
					description = Resources.IDS_MIK_INFODESCLINE9;
					value = of.Bpm.ToString();
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
			// Initialize the UniMod structure
			of = new Module();

			// Parse the uni module and create structures to use
			AgentResult retVal = CreateUniStructs(fileInfo.ModuleStream, out errorMessage);
			if (retVal != AgentResult.Ok)
				MLoader.FreeAll(of);

			return retVal;
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

			player = new MPlayer(of, this);

			mdNumChn = player.mdSngChn;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public override void CleanupPlayer()
		{
			MLoader.FreeAll(of);

			player = null;
			of = null;

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



		/********************************************************************/
		/// <summary>
		/// Cleanup the current song
		/// </summary>
		/********************************************************************/
		public override void CleanupSound()
		{
			of.Control = null;
			of.Voice = null;

			base.CleanupSound();
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			short oldSngPos = of.SngPos;
			ushort oldSngSpd = of.SngSpd;
			ushort oldBpm = player.mdBpm;

			player.HandleTick();
			bool endReached = player.endReached;

			if (of.SngSpd != oldSngSpd)
				ShowSpeed();

			if (player.mdBpm != oldBpm)
				SetTempo(player.mdBpm);

			if (of.SngPos != oldSngPos)
			{
				if (HasPositionBeenVisited(of.SngPos))
					endReached = true;

				MarkPositionAsVisited(of.SngPos);

				ShowSongPosition();
				ShowPattern();
				ShowTracks();
			}

			if (endReached)
			{
				OnEndReached(of.SngPos);

				MarkPositionAsVisited(of.SngPos);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module want to reserve
		/// </summary>
		/********************************************************************/
		public override int VirtualChannelCount => of.NumVoices;



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public override int ModuleChannelCount => of.NumChn;



		/********************************************************************/
		/// <summary>
		/// Returns all the instruments available in the module. If none,
		/// null is returned
		/// </summary>
		/********************************************************************/
		public override IEnumerable<InstrumentInfo> Instruments
		{
			get
			{
				// Check to see if there is instruments at all in the module
				if ((of.Flags & ModuleFlag.Inst) == 0)
					yield break;

				for (int i = 0, cnt = of.NumIns; i < cnt; i++)
				{
					Instrument inst = of.Instruments[i];

					InstrumentInfo instInfo = new InstrumentInfo
					{
						Name = inst.InsName,
						Flags = InstrumentInfo.InstrumentFlags.None
					};

					// Fill out the note samples
					for (int j = 0, s = 0; j < InstrumentInfo.Octaves; j++)
					{
						for (int k = 0; k < InstrumentInfo.NotesPerOctave; k++)
							instInfo.Notes[j, k] = (short)inst.SampleNumber[s++];
					}

					yield return instInfo;
				}
			}
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
				for (int i = 0, cnt = of.NumSmp; i < cnt; i++)
				{
					Sample sample = of.Samples[i];

					// Build frequency table
					uint[] frequencies = new uint[10 * 12];

					for (int j = 0; j < 9 * 12; j++)
						frequencies[1 * 12 + j] = (uint)Math.Ceiling((double)MlUtil.GetFrequency(of.Flags, player.GetPeriod(of.Flags, (ushort)(j * 2), sample.Speed)));

					SampleInfo sampleInfo = new SampleInfo
					{
						Name = sample.SampleName,
						Flags = SampleInfo.SampleFlag.None,
						Type = SampleInfo.SampleType.Sample,
						BitSize = (sample.Flags & SampleFlag._16Bits) != 0 ? SampleInfo.SampleSize._16Bit : SampleInfo.SampleSize._8Bit,
						Volume = (ushort)(sample.Volume * 4),
						Panning = sample.Panning == SharedConstant.Pan_Surround ? (short)ChannelPanningType.Surround : sample.Panning,
						Sample = sample.Handle,
						Length = sample.Length,
						LoopStart = sample.LoopStart,
						LoopLength = (sample.LoopEnd - sample.LoopStart),
						NoteFrequencies = frequencies
					};

					// Add extra loop flags if any
					if (((sample.Flags & SampleFlag.Loop) != 0) && (sample.LoopStart < sample.LoopEnd))
					{
						// Set loop flag
						sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;

						// Is the loop ping-pong?
						if ((sample.Flags & SampleFlag.Bidi) != 0)
							sampleInfo.Flags |= SampleInfo.SampleFlag.PingPong;
					}

					yield return sampleInfo;
				}
			}
		}
		#endregion

		#region IDriver methods
		/********************************************************************/
		/// <summary>
		/// Stops the channel
		/// </summary>
		/********************************************************************/
		public void VoiceStopInternal(sbyte voice)
		{
			if ((voice < 0) || (voice >= mdNumChn))
				return;

			VirtualChannels[voice].Mute();
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the voice doesn't play anymore
		/// </summary>
		/********************************************************************/
		public bool VoiceStoppedInternal(sbyte voice)
		{
			if ((voice < 0) || (voice >= mdNumChn))
				return false;

			return !VirtualChannels[voice].IsActive;
		}



		/********************************************************************/
		/// <summary>
		/// Starts to play the sample
		/// </summary>
		/********************************************************************/
		public void VoicePlayInternal(sbyte voice, Sample s, uint start)
		{
			if ((voice < 0) || (voice >= mdNumChn))
				return;

			// Play the sample
			byte bits = (s.Flags & SampleFlag._16Bits) != 0 ? (byte)16 : (byte)8;

			VirtualChannels[voice].PlaySample(s.SampleNumber, s.Handle, start, s.Length, bits);

			// Setup the loop if any
			if (((s.Flags & SampleFlag.Loop) != 0) && (s.LoopStart < s.LoopEnd))
			{
				uint repEnd = s.LoopEnd;

				// repEnd can't be bigger than size
				if (repEnd > s.Length)
					repEnd = s.Length;

				ChannelLoopType type = ((s.Flags & SampleFlag.Bidi) != 0) ? ChannelLoopType.PingPong : ChannelLoopType.Normal;

				VirtualChannels[voice].SetLoop(s.LoopStart, repEnd - s.LoopStart, type);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Changes the volume in the channel
		/// </summary>
		/********************************************************************/
		public void VoiceSetVolumeInternal(sbyte voice, ushort vol)
		{
			if ((voice < 0) || (voice >= mdNumChn))
				return;

			VirtualChannels[voice].SetVolume(vol);
		}



		/********************************************************************/
		/// <summary>
		/// Changes the panning in the channel
		/// </summary>
		/********************************************************************/
		public void VoiceSetPanningInternal(sbyte voice, uint pan)
		{
			if ((voice < 0) || (voice >= mdNumChn))
				return;

			if (pan == SharedConstant.Pan_Surround)
				pan = (int)ChannelPanningType.Surround;

			VirtualChannels[voice].SetPanning((ushort)pan);
		}



		/********************************************************************/
		/// <summary>
		/// Changes the frequency in the channel
		/// </summary>
		/********************************************************************/
		public void VoiceSetFrequencyInternal(sbyte voice, uint frq)
		{
			if ((voice < 0) || (voice >= mdNumChn))
				return;

			VirtualChannels[voice].SetFrequency(frq);
		}
		#endregion

		#region ModulePlayerWithPositionDurationAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Initialize all internal structures when beginning duration
		/// calculation on a new sub-song
		/// </summary>
		/********************************************************************/
		protected override int InitDuration(int startPosition)
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
			return of.NumPos;
		}



		/********************************************************************/
		/// <summary>
		/// Create a snapshot of all the internal structures and return it
		/// </summary>
		/********************************************************************/
		protected override ISnapshot CreateSnapshot()
		{
			return new Snapshot(player);
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
			Snapshot clonedSnapshot = new Snapshot(currentSnapshot.Player);

			player = clonedSnapshot.Player;
			of = player.pf;

			UpdateModuleInformation();

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will create all the structs needed to play the module
		/// </summary>
		/********************************************************************/
		private AgentResult CreateUniStructs(ModuleStream moduleStream, out string errorMessage)
		{
			// Skip mark and version
			moduleStream.Seek(6, SeekOrigin.Begin);

			// Read the header
			of.Flags = (ModuleFlag)moduleStream.Read_B_UINT16();
			of.NumChn = moduleStream.Read_UINT8();
			of.NumVoices = moduleStream.Read_UINT8();
			of.NumPos = moduleStream.Read_B_UINT16();
			of.NumPat = moduleStream.Read_B_UINT16();
			of.NumTrk = moduleStream.Read_B_UINT16();
			of.NumIns = moduleStream.Read_B_UINT16();
			of.NumSmp = moduleStream.Read_B_UINT16();
			of.RepPos = moduleStream.Read_B_UINT16();
			of.InitSpeed = moduleStream.Read_UINT8();
			of.InitTempo = moduleStream.Read_UINT8();
			of.InitVolume = moduleStream.Read_UINT8();
			of.BpmLimit = moduleStream.Read_B_UINT16();

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_MIK_ERR_LOADING_HEADER;
				return AgentResult.Error;
			}

			of.SongName = moduleStream.ReadString();
			of.Comment = moduleStream.ReadString();

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_MIK_ERR_LOADING_HEADER;
				return AgentResult.Error;
			}

			// Allocate memory to hold all the information
			if (!MLoader.AllocSamples(of))
			{
				errorMessage = Resources.IDS_MIK_ERR_LOADING_HEADER;
				return AgentResult.Error;
			}

			if (!MLoader.AllocTracks(of))
			{
				errorMessage = Resources.IDS_MIK_ERR_LOADING_HEADER;
				return AgentResult.Error;
			}

			if (!MLoader.AllocPatterns(of))
			{
				errorMessage = Resources.IDS_MIK_ERR_LOADING_HEADER;
				return AgentResult.Error;
			}

			if (!MLoader.AllocPositions(of, of.NumPos))
			{
				errorMessage = Resources.IDS_MIK_ERR_LOADING_HEADER;
				return AgentResult.Error;
			}

			// Read arrays
			moduleStream.ReadArray_B_UINT16s(of.Positions, 0, of.NumPos);
			moduleStream.ReadArray_B_UINT16s(of.Panning, 0, of.NumChn);
			moduleStream.Read(of.ChanVol, 0, of.NumChn);

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_MIK_ERR_LOADING_HEADER;
				return AgentResult.Error;
			}

			// Load sample headers
			for (int v = 0; v < of.NumSmp; v++)
			{
				Sample s = of.Samples[v];

				s.Flags = (SampleFlag)moduleStream.Read_B_UINT16();
				s.Speed = moduleStream.Read_B_UINT32();
				s.Volume = moduleStream.Read_UINT8();
				s.Panning = (short)moduleStream.Read_B_UINT16();
				s.Length = moduleStream.Read_B_UINT32();
				s.LoopStart = moduleStream.Read_B_UINT32();
				s.LoopEnd = moduleStream.Read_B_UINT32();
				s.SusBegin = moduleStream.Read_B_UINT32();
				s.SusEnd = moduleStream.Read_B_UINT32();

				s.GlobVol = moduleStream.Read_UINT8();
				s.VibFlags = (VibratoFlag)moduleStream.Read_UINT8();
				s.VibType = moduleStream.Read_UINT8();
				s.VibSweep = moduleStream.Read_UINT8();
				s.VibDepth = moduleStream.Read_UINT8();
				s.VibRate = moduleStream.Read_UINT8();
				s.SampleName = moduleStream.ReadString();

				// Reality check for loop settings
				if (s.LoopEnd > s.Length)
					s.LoopEnd = s.Length;

				if (s.LoopStart >= s.LoopEnd)
					s.Flags &= ~SampleFlag.Loop;

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIK_ERR_LOADING_SAMPLEINFO;
					return AgentResult.Error;
				}
			}

			// Load instruments
			if ((of.Flags & ModuleFlag.Inst) != 0)
			{
				if (!MLoader.AllocInstruments(of))
				{
					errorMessage = Resources.IDS_MIK_ERR_LOADING_INSTRUMENTINFO;
					return AgentResult.Error;
				}

				for (int v = 0; v < of.NumIns; v++)
				{
					Instrument i = of.Instruments[v];

					i.Flags = (InstrumentFlag)moduleStream.Read_UINT8();
					i.NnaType = (Nna)moduleStream.Read_UINT8();
					i.Dca = (Dca)moduleStream.Read_UINT8();
					i.Dct = (Dct)moduleStream.Read_UINT8();
					i.GlobVol = moduleStream.Read_UINT8();
					i.Panning = (short)moduleStream.Read_B_UINT16();
					i.PitPanSep = moduleStream.Read_UINT8();
					i.PitPanCenter = moduleStream.Read_UINT8();
					i.RVolVar = moduleStream.Read_UINT8();
					i.RPanVar = moduleStream.Read_UINT8();

					i.VolFade = moduleStream.Read_B_UINT16();

					i.VolFlg = (EnvelopeFlag)moduleStream.Read_UINT8();
					i.VolPts = moduleStream.Read_UINT8();
					i.VolSusBeg = moduleStream.Read_UINT8();
					i.VolSusEnd = moduleStream.Read_UINT8();
					i.VolBeg = moduleStream.Read_UINT8();
					i.VolEnd = moduleStream.Read_UINT8();

					for (int w = 0; w < SharedConstant.EnvPoints; w++)
					{
						i.VolEnv[w].Pos = (short)moduleStream.Read_B_UINT16();
						i.VolEnv[w].Val = (short)moduleStream.Read_B_UINT16();
					}

					i.PanFlg = (EnvelopeFlag)moduleStream.Read_UINT8();
					i.PanPts = moduleStream.Read_UINT8();
					i.PanSusBeg = moduleStream.Read_UINT8();
					i.PanSusEnd = moduleStream.Read_UINT8();
					i.PanBeg = moduleStream.Read_UINT8();
					i.PanEnd = moduleStream.Read_UINT8();

					for (int w = 0; w < SharedConstant.EnvPoints; w++)
					{
						i.PanEnv[w].Pos = (short)moduleStream.Read_B_UINT16();
						i.PanEnv[w].Val = (short)moduleStream.Read_B_UINT16();
					}

					i.PitFlg = (EnvelopeFlag)moduleStream.Read_UINT8();
					i.PitPts = moduleStream.Read_UINT8();
					i.PitSusBeg = moduleStream.Read_UINT8();
					i.PitSusEnd = moduleStream.Read_UINT8();
					i.PitBeg = moduleStream.Read_UINT8();
					i.PitEnd = moduleStream.Read_UINT8();

					for (int w = 0; w < SharedConstant.EnvPoints; w++)
					{
						i.PitEnv[w].Pos = (short)moduleStream.Read_B_UINT16();
						i.PitEnv[w].Val = (short)moduleStream.Read_B_UINT16();
					}

					moduleStream.ReadArray_B_UINT16s(i.SampleNumber, 0, SharedConstant.InstNotes);
					moduleStream.Read(i.SampleNote, 0, SharedConstant.InstNotes);

					i.InsName = moduleStream.ReadString();

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_MIK_ERR_LOADING_INSTRUMENTINFO;
						return AgentResult.Error;
					}
				}
			}

			// Read patterns
			moduleStream.ReadArray_B_UINT16s(of.PattRows, 0, of.NumPat);
			moduleStream.ReadArray_B_UINT16s(of.Patterns, 0, of.NumPat * of.NumChn);

			// Read tracks
			for (int v = 0; v < of.NumTrk; v++)
				of.Tracks[v] = TrkRead(moduleStream);

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_MIK_ERR_LOADING_TRACKS;
				return AgentResult.Error;
			}

			// Calculate the sample addresses and fix the samples
			return FindSamples(moduleStream, out errorMessage);
		}



		/********************************************************************/
		/// <summary>
		/// Allocates and read one track
		/// </summary>
		/********************************************************************/
		private byte[] TrkRead(ModuleStream moduleStream)
		{
			ushort len = moduleStream.Read_B_UINT16();
			byte[] t = new byte[len];

			moduleStream.Read(t, 0, len);

			return t;
		}



		/********************************************************************/
		/// <summary>
		/// Will find the sample addresses and fix them so all samples are
		/// signed and unpacked
		/// </summary>
		/********************************************************************/
		private AgentResult FindSamples(ModuleStream moduleStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			for (int v = 0; v < of.NumSmp; v++)
			{
				Sample s = of.Samples[v];

				uint length = s.Length;
				if (length != 0)
				{
					if ((s.Flags & SampleFlag._16Bits) != 0)
						length *= 2;

					if ((s.Flags & SampleFlag.Stereo) != 0)
						length *= 2;

					// Allocate memory to hold the sample
					s.Handle = new sbyte[length];

					if (!SLoader.Load(moduleStream, v, s.Handle, s.Flags, s.Length, out errorMessage))
						return AgentResult.Error;
				}
			}

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound(int startPosition)
		{
			player.Init(of, (short)startPosition);

			// We want to wrap the module
			of.Wrap = true;

			// Tell NostalgicPlayer about the initial BPM tempo
			SetTempo(of.Bpm);
			player.mdBpm = of.Bpm;
		}



		/********************************************************************/
		/// <summary>
		/// Sets the NostalgicPlayer to the right BPM tempo
		/// </summary>
		/********************************************************************/
		private void SetTempo(ushort tempo)
		{
			SetBpmTempo(tempo);
			ShowTempo();
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current song position
		/// </summary>
		/********************************************************************/
		private void ShowSongPosition()
		{
			OnModuleInfoChanged(InfoPositionLine, of.SngPos.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with pattern number
		/// </summary>
		/********************************************************************/
		private void ShowPattern()
		{
			OnModuleInfoChanged(InfoPatternLine, of.Positions[of.SngPos].ToString());
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
			OnModuleInfoChanged(InfoSpeedLine, of.SngSpd.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with tempo
		/// </summary>
		/********************************************************************/
		private void ShowTempo()
		{
			OnModuleInfoChanged(InfoTempoLine, of.Bpm.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with all dynamic values
		/// </summary>
		/********************************************************************/
		private void UpdateModuleInformation()
		{
			ShowSongPosition();
			ShowPattern();
			ShowTracks();
			ShowSpeed();
			ShowTempo();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing tracks
		/// </summary>
		/********************************************************************/
		private string FormatTracks()
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < of.NumChn; i++)
			{
				sb.Append(of.Patterns[of.Positions[of.SngPos] * of.NumChn + i]);
				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}
		#endregion
	}
}
