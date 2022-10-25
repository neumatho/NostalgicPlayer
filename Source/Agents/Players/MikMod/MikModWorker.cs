/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Agent.Player.MikMod.Containers;
using Polycode.NostalgicPlayer.Agent.Player.MikMod.LibMikMod;
using Polycode.NostalgicPlayer.Agent.Shared.MikMod;
using Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.MikMod
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class MikModWorker : ModulePlayerAgentBase, IDriver
	{
		private Module of;

		private MPlayer player;

		private byte mdNumChn;

		private int subSongCount;

		private const int InfoSpeedLine = 4;
		private const int InfoTempoLine = 5;

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
				// Song length
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

				// Used instruments
				case 2:
				{
					description = Resources.IDS_MIK_INFODESCLINE2;
					value = (of.Flags & ModuleFlag.Inst) != 0 ? of.NumIns.ToString() : "0";
					break;
				}

				// Used samples
				case 3:
				{
					description = Resources.IDS_MIK_INFODESCLINE3;
					value = of.NumSmp.ToString();
					break;
				}

				// Current speed
				case 4:
				{
					description = Resources.IDS_MIK_INFODESCLINE4;
					value = of.SngSpd.ToString();
					break;
				}

				// BPM
				case 5:
				{
					description = Resources.IDS_MIK_INFODESCLINE5;
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
		public override ModulePlayerSupportFlag SupportFlags => ModulePlayerSupportFlag.SetPosition;



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
			player = new MPlayer(of, this);

			mdNumChn = player.mdSngChn;

			return base.InitPlayer(out errorMessage);
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
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the current song
		/// </summary>
		/********************************************************************/
		public override bool InitSound(int songNumber, DurationInfo durationInfo, out string errorMessage)
		{
			player.Init(of, (short)durationInfo.StartPosition);

			// We want to wrap the module
			of.Wrap = true;

			// Tell NostalgicPlayer about the initial BPM tempo
			SetTempo(of.Bpm);
			player.mdBpm = of.Bpm;

			return base.InitSound(songNumber, durationInfo, out errorMessage);
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
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the duration for all sub-songs
		/// </summary>
		/********************************************************************/
		public override DurationInfo[] CalculateDuration()
		{
			DurationInfo[] durations = CalculateDurationBySongPosition();

			subSongCount = (ushort)durations.Length;

			return durations;
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

			if (of.SngSpd != oldSngSpd)
				OnModuleInfoChanged(InfoSpeedLine, of.SngSpd.ToString());

			if (player.mdBpm != oldBpm)
				SetTempo(player.mdBpm);

			if (of.SngPos != oldSngPos)
				OnPositionChanged();

			if (player.endReached)
				OnEndReached();
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
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public override SubSongInfo SubSongs => new SubSongInfo(subSongCount, 0);



		/********************************************************************/
		/// <summary>
		/// Return the length of the current song
		/// </summary>
		/********************************************************************/
		public override int SongLength => of.NumPos;



		/********************************************************************/
		/// <summary>
		/// Return the current position of the song
		/// </summary>
		/********************************************************************/
		public override int GetSongPosition()
		{
			return of.SngPos;
		}



		/********************************************************************/
		/// <summary>
		/// Set a new position of the song
		/// </summary>
		/********************************************************************/
		public override void SetSongPosition(int position, PositionInfo positionInfo)
		{
			// Change the position
			ushort pos = (ushort)position;

			if (pos >= of.NumPos)
				pos = of.NumPos;

			DurationExtraInfo extraInfo = (DurationExtraInfo)positionInfo.ExtraInfo;

			// Change the speed
			of.SngSpd = positionInfo.Speed;
			of.Bpm = positionInfo.Bpm;
			of.Control[0].FarCurTempo = extraInfo.FarCurTempo;
			of.Control[0].FarTempoBend = extraInfo.FarTempoBend;
			SetTempo((ushort)(of.Bpm + of.RelSpd));

			player.mdBpm = of.Bpm;

			of.PosJmp = 2;
			of.PatBrk = 0;
			of.PatPos = 0;
			of.Pat_RepCrazy = false;
			of.PatDly = 0;
			of.PatDly2 = 0;
			of.SngPos = (short)pos;
			of.VbTick = of.SngSpd;

			for (sbyte t = 0; t < player.NumVoices(of); t++)
			{
				VoiceStopInternal(t);
				of.Voice[t].Main.I = null;
				of.Voice[t].Main.S = null;
			}

			for (int t = 0; t < of.NumChn; t++)
			{
				of.Control[t].Main.I = null;
				of.Control[t].Main.S = null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Returns all the instruments available in the module. If none,
		/// null is returned
		/// </summary>
		/********************************************************************/
		public override InstrumentInfo[] Instruments
		{
			get
			{
				// Check to see if there is instruments at all in the module
				if ((of.Flags & ModuleFlag.Inst) == 0)
					return null;

				List<InstrumentInfo> result = new List<InstrumentInfo>();

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

					result.Add(instInfo);
				}

				return result.ToArray();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Returns all the samples available in the module. If none, null
		/// is returned
		/// </summary>
		/********************************************************************/
		public override SampleInfo[] Samples
		{
			get
			{
				List<SampleInfo> result = new List<SampleInfo>();

				for (int i = 0, cnt = of.NumSmp; i < cnt; i++)
				{
					Sample sample = of.Samples[i];

					// Build frequency table
					uint[] frequencies = new uint[10 * 12];

					for (int j = 0; j < 10 * 12; j++)
						frequencies[j] = (uint)Math.Ceiling((double)MlUtil.GetFrequency(of.Flags, player.GetPeriod(of.Flags, (ushort)(j * 2), sample.Speed)));

					SampleInfo sampleInfo = new SampleInfo
					{
						Name = sample.SampleName,
						Flags = SampleInfo.SampleFlags.None,
						Type = SampleInfo.SampleType.Sample,
						BitSize = (byte)((sample.Flags & SampleFlag._16Bits) != 0 ? 16 : 8),
						MiddleC = frequencies[4 * 12],
						Volume = (ushort)(sample.Volume * 4),
						Panning = sample.Panning == SharedConstant.Pan_Surround ? (short)ChannelPanning.Surround : sample.Panning,
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
						sampleInfo.Flags |= SampleInfo.SampleFlags.Loop;

						// Is the loop ping-pong?
						if ((sample.Flags & SampleFlag.Bidi) != 0)
							sampleInfo.Flags |= SampleInfo.SampleFlags.PingPong;
					}

					result.Add(sampleInfo);
				}

				return result.ToArray();
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
				pan = (int)ChannelPanning.Surround;

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

		#region Duration calculation methods
		/********************************************************************/
		/// <summary>
		/// Initialize all internal structures when beginning duration
		/// calculation on a new sub-song
		/// </summary>
		/********************************************************************/
		protected override int InitDurationCalculationByStartPos(int startPosition)
		{
			player.Init(of, (short)startPosition);

			// We want to wrap the module
			of.Wrap = true;

			return startPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Return the current speed
		/// </summary>
		/********************************************************************/
		protected override byte GetCurrentSpeed()
		{
			return (byte)of.SngSpd;
		}



		/********************************************************************/
		/// <summary>
		/// Return the current BPM
		/// </summary>
		/********************************************************************/
		protected override ushort GetCurrentBpm()
		{
			return of.Bpm;
		}



		/********************************************************************/
		/// <summary>
		/// Return extra information for the current position
		/// </summary>
		/********************************************************************/
		protected override object GetExtraPositionInfo()
		{
			return new DurationExtraInfo
			{
				FarCurTempo = of.Control[0].FarCurTempo,
				FarTempoBend = of.Control[0].FarTempoBend
			};
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
		/// Sets the NostalgicPlayer to the right BPM tempo
		/// </summary>
		/********************************************************************/
		private void SetTempo(ushort tempo)
		{
			SetBpmTempo(tempo);
			OnModuleInfoChanged(InfoTempoLine, tempo.ToString());
		}
		#endregion
	}
}
