/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Polycode.NostalgicPlayer.Agent.Player.JamCracker.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Mixer;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.JamCracker
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class JamCrackerWorker : ModulePlayerAgentBase
	{
		private static readonly ushort[] periods =
		{
			1019, 962, 908, 857, 809, 763, 720, 680, 642, 606, 572, 540, 509,
			 481, 454, 428, 404, 381, 360, 340, 321, 303, 286, 270, 254, 240,
			 227, 214, 202, 190, 180, 170, 160, 151, 143, 135, 135, 135, 135,
			 135, 135, 135, 135, 135, 135, 135, 135, 135, 135, 135, 135
		};

		private TimeSpan totalTime;
		private PosInfo[] posInfoList;

		private ushort samplesNum;
		private ushort patternNum;
		private ushort songLen;

		private InstInfo[] instTable;
		private PattInfo[] pattTable;
		private ushort[] songTable;

		private NoteInfo[] address;
		private VoiceInfo[] variables;

		private int addressIndex;

		private ushort tmpDmacon;

		private ushort songPos;
		private ushort songCnt;
		private ushort noteCnt;

		private byte wait;
		private byte waitCnt;

		private const int InfoSpeedLine = 3;

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => new [] { "jam" };



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 6)
				return AgentResult.Unknown;

			// Check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);

			if (moduleStream.Read_B_UINT32() == 0x42654570)	// BeEp
				return AgentResult.Ok;

			return AgentResult.Unknown;
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
					description = Resources.IDS_JAM_INFODESCLINE0;
					value = songLen.ToString();
					break;
				}

				// Used patterns
				case 1:
				{
					description = Resources.IDS_JAM_INFODESCLINE1;
					value = patternNum.ToString();
					break;
				}

				// Used instruments
				case 2:
				{
					description = Resources.IDS_JAM_INFODESCLINE2;
					value = samplesNum.ToString();
					break;
				}

				// Current speed
				case 3:
				{
					description = Resources.IDS_JAM_INFODESCLINE3;
					value = wait.ToString();
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

				// Skip the module mark
				moduleStream.Read_B_UINT32();

				// Get the number of instruments
				samplesNum = moduleStream.Read_B_UINT16();

				// Allocate the instrument structures
				instTable = new InstInfo[samplesNum];

				// Read the instrument info
				for (int i = 0; i < samplesNum; i++)
				{
					InstInfo instInfo = new InstInfo();

					moduleStream.ReadString(instInfo.Name, 31);

					instInfo.Flags = moduleStream.Read_UINT8();
					instInfo.Size = moduleStream.Read_B_UINT32();
					moduleStream.Read_B_UINT32();		// Skip the address

					instTable[i] = instInfo;
				}

				// Get the number of patterns
				patternNum = moduleStream.Read_B_UINT16();

				// Allocate pattern structures
				pattTable = new PattInfo[patternNum];

				// Read the pattern information
				for (int i = 0; i < patternNum; i++)
				{
					PattInfo pattInfo = new PattInfo();

					pattInfo.Size = moduleStream.Read_B_UINT16();
					moduleStream.Read_B_UINT32();		// Skip the address

					pattTable[i] = pattInfo;
				}

				// Get the song length
				songLen = moduleStream.Read_B_UINT16();

				// Allocate and read the position array
				songTable = new ushort[songLen];

				moduleStream.ReadArray_B_UINT16s(songTable, 0, songLen);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_JAM_ERR_LOADING_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				// Get the pattern data
				for (int i = 0; i < patternNum; i++)
				{
					// Allocate the pattern data
					NoteInfo[] note = new NoteInfo[pattTable[i].Size * 4];
					pattTable[i].Address = note;

					// Read the data from the stream
					for (int j = 0; j < pattTable[i].Size * 4; j++)
					{
						NoteInfo noteInfo = new NoteInfo();

						noteInfo.Period = moduleStream.Read_UINT8();
						noteInfo.Instr = moduleStream.Read_INT8();
						noteInfo.Speed = moduleStream.Read_UINT8();
						noteInfo.Arpeggio = moduleStream.Read_UINT8();
						noteInfo.Vibrato = moduleStream.Read_UINT8();
						noteInfo.Phase = moduleStream.Read_UINT8();
						noteInfo.Volume = moduleStream.Read_UINT8();
						noteInfo.Porta = moduleStream.Read_UINT8();

						note[j] = noteInfo;
					}

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_JAM_ERR_LOADING_PATTERNS;
						Cleanup();

						return AgentResult.Error;
					}
				}

				// Read the samples
				for (int i = 0; i < samplesNum; i++)
				{
					InstInfo instInfo = instTable[i];

					if (instInfo.Size != 0)
					{
						instInfo.Address = moduleStream.ReadSampleData(i, (int)instInfo.Size);

						if (moduleStream.EndOfStream && (i != samplesNum - 1))
						{
							errorMessage = Resources.IDS_JAM_ERR_LOADING_SAMPLES;
							Cleanup();

							return AgentResult.Error;
						}
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



		/********************************************************************/
		/// <summary>
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public override bool InitPlayer()
		{
			byte curSpeed = 6;
			float total = 0.0f;

			// Calculate the position times
			posInfoList = new PosInfo[songLen];

			for (int i = 0; i < songLen; i++)
			{
				// Add the position information to the list
				PosInfo posInfo = new PosInfo();

				posInfo.Speed = curSpeed;
				posInfo.Time = new TimeSpan((long)(total * TimeSpan.TicksPerMillisecond));

				posInfoList[i] = posInfo;

				// Get next pattern
				PattInfo pattInfo = pattTable[songTable[i]];
				ushort noteCount = pattInfo.Size;
				NoteInfo[] noteInfo = pattInfo.Address;

				for (int j = 0; j < noteCount; j++)
				{
					for (int k = 0; k < 4; k++)
					{
						// Should the speed be changed
						if ((noteInfo[j + k].Speed & 15) != 0)
							curSpeed = (byte)(noteInfo[j + k].Speed & 15);
					}

					// Add the row time
					total += 1000.0f * curSpeed / 50.0f;
				}
			}

			// Set the total time
			totalTime = new TimeSpan((long)(total * TimeSpan.TicksPerMillisecond));

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
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the current song
		/// </summary>
		/********************************************************************/
		public override void InitSound(int songNumber)
		{
			// Initialize other variables
			songPos = 0;
			songCnt = songLen;

			PattInfo pattInfo = pattTable[songTable[0]];
			noteCnt = pattInfo.Size;
			address = pattInfo.Address;

			addressIndex = 0;

			wait = 6;
			waitCnt = 1;

			// Initialize channel variables
			ushort waveOff = 0x80;

			variables = new VoiceInfo[4];
			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = new VoiceInfo();

				voiceInfo.WaveOffset = waveOff;
				voiceInfo.Dmacon = (ushort)(1 << i);
				voiceInfo.Channel = VirtualChannels[i];
				voiceInfo.InsLen = 0;
				voiceInfo.InsAddress = null;
				voiceInfo.RealInsAddress = null;
				voiceInfo.PerIndex = 0;
				voiceInfo.Pers[0] = 1019;
				voiceInfo.Pers[1] = 0;
				voiceInfo.Pers[2] = 0;
				voiceInfo.Por = 0;
				voiceInfo.DeltaPor = 0;
				voiceInfo.PorLevel = 0;
				voiceInfo.Vib = 0;
				voiceInfo.DeltaVib = 0;
				voiceInfo.Vol = 0;
				voiceInfo.DeltaVol = 0;
				voiceInfo.VolLevel = 0x40;
				voiceInfo.Phase = 0;
				voiceInfo.DeltaPhase = 0;
				voiceInfo.VibCnt = 0;
				voiceInfo.VibMax = 0;
				voiceInfo.Flags = 0;

				variables[i] = voiceInfo;

				waveOff += 0x40;
			}
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			if (--waitCnt == 0)
			{
				NewNote();
				waitCnt = wait;
			}

			SetChannel(variables[0]);
			SetChannel(variables[1]);
			SetChannel(variables[2]);
			SetChannel(variables[3]);
		}



		/********************************************************************/
		/// <summary>
		/// Return the length of the current song
		/// </summary>
		/********************************************************************/
		public override int SongLength
		{
			get
			{
				return songLen;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Holds the current position of the song
		/// </summary>
		/********************************************************************/
		public override int SongPosition
		{
			get
			{
				return songLen - songCnt;
			}

			set
			{
				// Change the position
				songCnt = (ushort)(songLen - value);
				songPos = (ushort)value;

				PattInfo pattInfo = pattTable[songTable[songPos]];
				noteCnt = pattInfo.Size;
				address = pattInfo.Address;

				addressIndex = 0;

				// Change the speed
				waitCnt = 1;
				wait = posInfoList[value].Speed;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the position time for each position
		/// </summary>
		/********************************************************************/
		public override TimeSpan GetPositionTimeTable(int songNumber, out TimeSpan[] positionTimes)
		{
			positionTimes = posInfoList.Select(pi => pi.Time).ToArray();

			return totalTime;
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

				foreach (InstInfo instInfo in instTable)
				{
					SampleInfo sampleInfo = new SampleInfo
					{
						Name = EncoderCollection.Amiga.GetString(instInfo.Name),
						BitSize = 8,
						MiddleC = 8287,
						Volume = 256,
						Panning = -1
					};

					if ((instInfo.Flags & 2) != 0)
					{
						// AM sample
						sampleInfo.Type = SampleInfo.SampleType.Synth;
						sampleInfo.Flags = SampleInfo.SampleFlags.None;
						sampleInfo.Sample = null;
						sampleInfo.Length = 0;
						sampleInfo.LoopStart = 0;
						sampleInfo.LoopLength = 0;
					}
					else
					{
						// Normal sample
						sampleInfo.Type = SampleInfo.SampleType.Sample;
						sampleInfo.Sample = instInfo.Address;
						sampleInfo.Length = (int)instInfo.Size;

						if ((instInfo.Flags & 1) != 0)
						{
							// Sample loops
							sampleInfo.Flags = SampleInfo.SampleFlags.Loop;
							sampleInfo.LoopStart = 0;
							sampleInfo.LoopLength = (int)instInfo.Size;
						}
						else
						{
							// No loop
							sampleInfo.Flags = SampleInfo.SampleFlags.None;
							sampleInfo.LoopStart = 0;
							sampleInfo.LoopLength = 0;
						}
					}

					result.Add(sampleInfo);
				}

				return result.ToArray();
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player has allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			posInfoList = null;

			songTable = null;
			pattTable = null;
			instTable = null;
		}



		/********************************************************************/
		/// <summary>
		/// Will get a new note from the current pattern + skip to the next
		/// pattern if necessary
		/// </summary>
		/********************************************************************/
		private void NewNote()
		{
			NoteInfo[] adr = address;
			int adrIndex = addressIndex;
			addressIndex += 4;				// Go to the next row

			if (--noteCnt == 0)
			{
				songPos++;
				if (--songCnt == 0)
				{
					songPos = 0;
					songCnt = songLen;

					// Next module
					OnEndReached();
				}

				// Next position
				OnPositionChanged();

				PattInfo pattInfo = pattTable[songTable[songPos]];
				noteCnt = pattInfo.Size;
				address = pattInfo.Address;

				addressIndex = 0;
			}

			tmpDmacon = 0;

			NwNote(adr[adrIndex], variables[0]);
			NwNote(adr[++adrIndex], variables[1]);
			NwNote(adr[++adrIndex], variables[2]);
			NwNote(adr[++adrIndex], variables[3]);

			SetVoice(variables[0]);
			SetVoice(variables[1]);
			SetVoice(variables[2]);
			SetVoice(variables[3]);
		}



		/********************************************************************/
		/// <summary>
		/// Parse the given pattern and set up the voice structure given
		/// </summary>
		/********************************************************************/
		private void NwNote(NoteInfo adr, VoiceInfo voice)
		{
			int perIndex;

			if (adr.Period != 0)
			{
				perIndex = adr.Period - 1;

				if ((adr.Speed & 64) != 0)
					voice.PorLevel = (short)periods[perIndex];
				else
				{
					tmpDmacon |= voice.Dmacon;

					voice.PerIndex = perIndex;
					voice.Pers[0] = periods[perIndex];
					voice.Pers[1] = periods[perIndex];
					voice.Pers[2] = periods[perIndex];

					voice.Por = 0;

					if (adr.Instr > samplesNum)
					{
						voice.InsAddress = null;
						voice.RealInsAddress = null;
						voice.InsLen = 0;
						voice.Flags = 0;
					}
					else
					{
						InstInfo instInfo = instTable[adr.Instr];
						if (instInfo.Address == null)
						{
							voice.InsAddress = null;
							voice.RealInsAddress = null;
							voice.InsLen = 0;
							voice.Flags = 0;
						}
						else
						{
							if ((instInfo.Flags & 2) == 0)
							{
								voice.InsAddress = instInfo.Address;
								voice.RealInsAddress = instInfo.Address;
								voice.InsLen = (ushort)(instInfo.Size / 2);
							}
							else
							{
								voice.RealInsAddress = instInfo.Address;
								voice.InsAddress = voice.WaveBuffer;
								Array.Copy(instInfo.Address, voice.WaveOffset, voice.InsAddress, 0, 0x40);
								voice.InsLen = 0x20;
							}

							voice.Flags = instInfo.Flags;
							voice.Vol = (short)voice.VolLevel;
						}
					}
				}
			}

			if ((adr.Speed & 15) != 0)
			{
				wait = (byte)(adr.Speed & 15);

				// Change the module info
				OnModuleInfoChanged(InfoSpeedLine, wait.ToString());
			}

			// Do arpeggio
			perIndex = voice.PerIndex;

			if (adr.Arpeggio != 0)
			{
				if (adr.Arpeggio == 255)
				{
					voice.Pers[0] = periods[perIndex];
					voice.Pers[1] = periods[perIndex];
					voice.Pers[2] = periods[perIndex];
				}
				else
				{
					voice.Pers[2] = periods[perIndex + adr.Arpeggio & 15];
					voice.Pers[1] = periods[perIndex + adr.Arpeggio >> 4];
					voice.Pers[0] = periods[perIndex];
				}
			}

			// Do vibrato
			if (adr.Vibrato != 0)
			{
				if (adr.Vibrato == 255)
				{
					voice.Vib = 0;
					voice.DeltaVib = 0;
					voice.VibCnt = 0;
				}
				else
				{
					voice.Vib = 0;
					voice.DeltaVib = (short)(adr.Vibrato & 15);
					voice.VibMax = (byte)(adr.Vibrato >> 4);
					voice.VibCnt = (byte)(adr.Vibrato >> 5);
				}
			}

			// Do phase
			if (adr.Phase != 0)
			{
				if (adr.Phase == 255)
				{
					voice.Phase = 0;
					voice.DeltaPhase = -1;
				}
				else
				{
					voice.Phase = 0;
					voice.DeltaPhase = (short)(adr.Phase & 15);
				}
			}

			// Do volume
			short temp;
			if ((temp = adr.Volume) == 0)
			{
				if ((adr.Speed & 128) != 0)
				{
					voice.Vol = temp;
					voice.VolLevel = (ushort)temp;
					voice.DeltaVol = 0;
				}
			}
			else
			{
				if (temp == 255)
					voice.DeltaVol = 0;
				else
				{
					if ((adr.Speed & 128) != 0)
					{
						voice.Vol = temp;
						voice.VolLevel = (ushort)temp;
						voice.DeltaVol = 0;
					}
					else
					{
						temp &= 0x7f;
						if ((adr.Volume & 128) != 0)
							temp = (short)-temp;

						voice.DeltaVol = temp;
					}
				}
			}

			// Do portamento
			if ((temp = adr.Porta) != 0)
			{
				if (temp == 255)
				{
					voice.Por = 0;
					voice.DeltaPor = 0;
				}
				else
				{
					voice.Por = 0;
					if ((adr.Speed & 64) != 0)
					{
						if (voice.PorLevel <= voice.Pers[0])
							temp = (short)-temp;
					}
					else
					{
						temp &= 0x7f;
						if ((adr.Porta & 128) == 0)
						{
							temp = (short)-temp;
							voice.PorLevel = 135;
						}
						else
							voice.PorLevel = 1019;
					}

					voice.DeltaPor = temp;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will setup the voice in the channel structure
		/// </summary>
		/********************************************************************/
		private void SetVoice(VoiceInfo voice)
		{
			if ((tmpDmacon & voice.Dmacon) != 0)
			{
				Channel chan = voice.Channel;

				// Setup the start sample
				if (voice.InsAddress == null)
					chan.Mute();
				else
				{
					chan.PlaySample(voice.InsAddress, 0, (uint)(voice.InsLen * 2));
					chan.SetAmigaPeriod(voice.Pers[0]);
				}

				// Check to see if sample loops
				if ((voice.Flags & 1) == 0)
				{
					voice.InsAddress = null;
					voice.RealInsAddress = null;
					voice.InsLen = 0;
				}

				// Setup loop
				if (voice.InsAddress != null)
					chan.SetLoop(0, (uint)(voice.InsLen * 2));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will setup the channel structure
		/// </summary>
		/********************************************************************/
		private void SetChannel(VoiceInfo voice)
		{
			Channel chan = voice.Channel;

			while (voice.Pers[0] == 0)
				RotatePeriods(voice);

			short per = (short)(voice.Pers[0] + voice.Por);
			if (voice.Por < 0)
			{
				if (per < voice.PorLevel)
					per = voice.PorLevel;
			}
			else
			{
				if ((voice.Por != 0) && (per > voice.PorLevel))
					per = voice.PorLevel;
			}

			// Add vibrato
			per += voice.Vib;

			if (per < 135)
				per = 135;
			else if (per > 1019)
				per = 1019;

			chan.SetAmigaPeriod((uint)per);
			RotatePeriods(voice);

			voice.Por += voice.DeltaPor;
			if (voice.Por < -1019)
				voice.Por = -1019;
			else if (voice.Por > 1019)
				voice.Por = 1019;

			if (voice.VibCnt != 0)
			{
				voice.Vib += voice.DeltaVib;
				if (--voice.VibCnt == 0)
				{
					voice.DeltaVib = (short)-voice.DeltaVib;
					voice.VibCnt = voice.VibMax;
				}
			}

			chan.SetVolume((ushort)(voice.Vol * 4));

			voice.Vol += voice.DeltaVol;
			if (voice.Vol < 0)
				voice.Vol = 0;
			else if (voice.Vol > 64)
				voice.Vol = 64;

			if (((voice.Flags & 1) != 0) && (voice.DeltaPhase != 0))
			{
				if (voice.DeltaPhase < 0)
					voice.DeltaPhase = 0;

				sbyte[] instData = voice.InsAddress;
				sbyte[] wave = voice.RealInsAddress;
				int wavePhase = voice.Phase / 4;

				for (int i = 0; i < 64; i++)
				{
					short temp = wave[i];
					temp += wave[wavePhase++];
					temp >>= 1;
					instData[i] = (sbyte)temp;
				}

				voice.Phase = (ushort)(voice.Phase + voice.DeltaPhase);
				if (voice.Phase >= 256)
					voice.Phase -= 256;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Rotates the periods in the pers array
		/// </summary>
		/********************************************************************/
		private void RotatePeriods(VoiceInfo voice)
		{
			ushort temp = voice.Pers[0];
			voice.Pers[0] = voice.Pers[1];
			voice.Pers[1] = voice.Pers[2];
			voice.Pers[2] = temp;
		}
		#endregion
	}
}
