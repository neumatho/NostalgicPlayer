/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.IO;
using System.Linq;
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Bases;
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Containers;
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Exceptions;
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Mixer;
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.JamCracker
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class JamCrackerWorker : ModulePlayerAgentBase
	{
		#region Structures

		#region Note info structure
		private class NoteInfo
		{
			public byte period;
			public sbyte instr;
			public byte speed;
			public byte arpeggio;
			public byte vibrato;
			public byte phase;
			public byte volume;
			public byte porta;
		}
		#endregion

		#region Instrument info structure
		private class InstInfo
		{
			public byte[] name = new byte[32];
			public byte flags;
			public uint size;
			public sbyte[] address;
		}
		#endregion

		#region Pattern info structure
		private class PattInfo
		{
			public ushort size;
			public NoteInfo[] address;
		}
		#endregion

		#region Voice info structure
		private class VoiceInfo
		{
			public ushort waveOffset = 0;
			public ushort dmacon;
			public Channel channel;
			public ushort insLen;
			public sbyte[] insAddress;
			public sbyte[] realInsAddress;
			public int perIndex;
			public ushort[] pers = new ushort[3];
			public short por;
			public short deltaPor;
			public short porLevel;
			public short vib;
			public short deltaVib;
			public short vol;
			public short deltaVol;
			public ushort volLevel;
			public ushort phase;
			public short deltaPhase;
			public byte vibCnt;
			public byte vibMax;
			public byte flags;
		}
		#endregion

		#region Position info structure
		private class PosInfo
		{
			public byte speed;
			public TimeSpan time;
		}
		#endregion

		#endregion

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

		private ushort tmpDMACON;

		private ushort songPos;
		private ushort songCnt;
		private ushort noteCnt;

		private byte wait;
		private byte waitCnt;

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions
		{
			get
			{
				return new [] { "jam" };
			}
		}



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream stream = fileInfo.ModuleStream;

			// Check the module size
			if (stream.Length < 6)
				return AgentResult.Unknown;

			// Check the mark
			stream.Seek(0, SeekOrigin.Begin);

			if (stream.Read_B_UINT32() == 0x42654570)	// BeEp
				return AgentResult.Ok;

			return AgentResult.Unknown;
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
		#endregion

		#region IModulePlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Will load the file into memory
		/// </summary>
		/********************************************************************/
		public override ModulePlayerSupportFlag SupportFlags
		{
			get
			{
				return ModulePlayerSupportFlag.SetPosition;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will load the file into memory
		/// </summary>
		/********************************************************************/
		public override AgentResult Load(PlayerFileInfo fileInfo, out string errorMessage)
		{
			errorMessage = string.Empty;
			AgentResult agentResult = AgentResult.Error;

			try
			{
				ModuleStream stream = fileInfo.ModuleStream;

				// Skip the module mark
				stream.Read_B_UINT32();

				// Get the number of instruments
				samplesNum = stream.Read_B_UINT16();

				// Allocate the instrument structures
				instTable = new InstInfo[samplesNum];

				// Read the instrument info
				for (int i = 0; i < samplesNum; i++)
				{
					InstInfo instInfo = new InstInfo();

					stream.ReadString(instInfo.name, 31);

					instInfo.flags = stream.Read_UINT8();
					instInfo.size = stream.Read_B_UINT32();
					stream.Read_B_UINT32();		// Skip the address

					instTable[i] = instInfo;
				}

				// Get the number of patterns
				patternNum = stream.Read_B_UINT16();

				// Allocate pattern structures
				pattTable = new PattInfo[patternNum];

				// Read the pattern information
				for (int i = 0; i < patternNum; i++)
				{
					PattInfo pattInfo = new PattInfo();

					pattInfo.size = stream.Read_B_UINT16();
					stream.Read_B_UINT32();		// Skip the address

					pattTable[i] = pattInfo;
				}

				// Get the song length
				songLen = stream.Read_B_UINT16();

				// Allocate and read the position array
				songTable = new ushort[songLen];

				stream.ReadArray_B_UINT16s(songTable, songLen);

				if (stream.EndOfStream)
				{
					errorMessage = Properties.Resources.IDS_JAM_ERR_LOADING_HEADER;
					throw new StopException();
				}

				// Get the pattern data
				for (int i = 0; i < patternNum; i++)
				{
					// Allocate the pattern data
					NoteInfo[] note = new NoteInfo[pattTable[i].size * 4];
					pattTable[i].address = note;

					// Read the data from the stream
					for (int j = 0; j < pattTable[i].size * 4; j++)
					{
						NoteInfo noteInfo = new NoteInfo();

						noteInfo.period = stream.Read_UINT8();
						noteInfo.instr = stream.Read_INT8();
						noteInfo.speed = stream.Read_UINT8();
						noteInfo.arpeggio = stream.Read_UINT8();
						noteInfo.vibrato = stream.Read_UINT8();
						noteInfo.phase = stream.Read_UINT8();
						noteInfo.volume = stream.Read_UINT8();
						noteInfo.porta = stream.Read_UINT8();

						note[j] = noteInfo;
					}

					if (stream.EndOfStream)
					{
						errorMessage = Properties.Resources.IDS_JAM_ERR_LOADING_PATTERNS;
						throw new StopException();
					}
				}

				// Read the samples
				for (int i = 0; i < samplesNum; i++)
				{
					InstInfo instInfo = instTable[i];

					if (instInfo.size != 0)
					{
						instInfo.address = new sbyte[instInfo.size];

						stream.ReadSigned(instInfo.address, 0, (int)instInfo.size);

						if (stream.EndOfStream && (i != samplesNum - 1))
						{
							errorMessage = Properties.Resources.IDS_JAM_ERR_LOADING_SAMPLES;
							throw new StopException();
						}
					}
				}

				// Ok, we're done
				agentResult = AgentResult.Ok;
			}
			catch (StopException)
			{
				Cleanup();
			}
			catch (Exception)
			{
				Cleanup();
				throw;
			}

			return agentResult;
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

				posInfo.speed = curSpeed;
				posInfo.time = new TimeSpan((long)(total * TimeSpan.TicksPerMillisecond));

				posInfoList[i] = posInfo;

				// Get next pattern
				PattInfo pattInfo = pattTable[songTable[i]];
				ushort noteCount = pattInfo.size;
				NoteInfo[] noteInfo = pattInfo.address;

				for (int j = 0; j < noteCount; j++)
				{
					for (int k = 0; k < 4; k++)
					{
						// Should the speed be changed
						if ((noteInfo[j + k].speed & 15) != 0)
							curSpeed = (byte)(noteInfo[j + k].speed & 15);
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
			noteCnt = pattInfo.size;
			address = pattInfo.address;

			addressIndex = 0;

			wait = 6;
			waitCnt = 1;

			// Initialize channel variables
			ushort waveOff = 0x80;

			variables = new VoiceInfo[4];
			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = new VoiceInfo();

				voiceInfo.waveOffset = waveOff;
				voiceInfo.dmacon = (ushort)(1 << i);
				voiceInfo.channel = Channels[i];
				voiceInfo.insLen = 0;
				voiceInfo.insAddress = null;
				voiceInfo.realInsAddress = null;
				voiceInfo.perIndex = 0;
				voiceInfo.pers[0] = 1019;
				voiceInfo.pers[1] = 0;
				voiceInfo.pers[2] = 0;
				voiceInfo.por = 0;
				voiceInfo.deltaPor = 0;
				voiceInfo.porLevel = 0;
				voiceInfo.vib = 0;
				voiceInfo.deltaVib = 0;
				voiceInfo.vol = 0;
				voiceInfo.deltaVol = 0;
				voiceInfo.volLevel = 0x40;
				voiceInfo.phase = 0;
				voiceInfo.deltaPhase = 0;
				voiceInfo.vibCnt = 0;
				voiceInfo.vibMax = 0;
				voiceInfo.flags = 0;

				variables[i] = voiceInfo;

				waveOff += 0x40;
			}
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
				noteCnt = pattInfo.size;
				address = pattInfo.address;

				addressIndex = 0;

				// Change the speed
				waitCnt = 1;
				wait = posInfoList[value].speed;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the position time for each position
		/// </summary>
		/********************************************************************/
		public override TimeSpan GetPositionTimeTable(int songNumber, out TimeSpan[] positionTimes)
		{
			positionTimes = posInfoList.Select(pi => pi.time).ToArray();

			return totalTime;
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
					description = Properties.Resources.IDS_JAM_INFODESCLINE0;
					value = songLen.ToString();
					break;
				}

				// Used patterns
				case 1:
				{
					description = Properties.Resources.IDS_JAM_INFODESCLINE1;
					value = patternNum.ToString();
					break;
				}

				// Used instruments
				case 2:
				{
					description = Properties.Resources.IDS_JAM_INFODESCLINE2;
					value = samplesNum.ToString();
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
					EndReached = true;
				}

				// Next position
				OnPositionChanged();

				PattInfo pattInfo = pattTable[songTable[songPos]];
				noteCnt = pattInfo.size;
				address = pattInfo.address;

				addressIndex = 0;
			}

			tmpDMACON = 0;

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

			if (adr.period != 0)
			{
				perIndex = adr.period - 1;

				if ((adr.speed & 64) != 0)
					voice.porLevel = (short)periods[perIndex];
				else
				{
					tmpDMACON |= voice.dmacon;

					voice.perIndex = perIndex;
					voice.pers[0] = periods[perIndex];
					voice.pers[1] = periods[perIndex];
					voice.pers[2] = periods[perIndex];

					voice.por = 0;

					if (adr.instr > samplesNum)
					{
						voice.insAddress = null;
						voice.realInsAddress = null;
						voice.insLen = 0;
						voice.flags = 0;
					}
					else
					{
						InstInfo instInfo = instTable[adr.instr];
						if (instInfo.address == null)
						{
							voice.insAddress = null;
							voice.realInsAddress = null;
							voice.insLen = 0;
							voice.flags = 0;
						}
						else
						{
							if ((instInfo.flags & 2) == 0)
							{
								voice.insAddress = instInfo.address;
								voice.realInsAddress = instInfo.address;
								voice.insLen = (ushort)(instInfo.size / 2);
							}
							else
							{
								voice.realInsAddress = instInfo.address;
								voice.insAddress = new sbyte[0x40];
								Array.Copy(instInfo.address, voice.waveOffset, voice.insAddress, 0, 0x40);
								voice.insLen = 0x20;
							}

							voice.flags = instInfo.flags;
							voice.vol = (short)voice.volLevel;
						}
					}
				}
			}

			if ((adr.speed & 15) != 0)
				wait = (byte)(adr.speed & 15);

			// Do arpeggio
			perIndex = voice.perIndex;

			if (adr.arpeggio != 0)
			{
				if (adr.arpeggio == 255)
				{
					voice.pers[0] = periods[perIndex];
					voice.pers[1] = periods[perIndex];
					voice.pers[2] = periods[perIndex];
				}
				else
				{
					voice.pers[2] = periods[perIndex + adr.arpeggio & 15];
					voice.pers[1] = periods[perIndex + adr.arpeggio >> 4];
					voice.pers[0] = periods[perIndex];
				}
			}

			// Do vibrato
			if (adr.vibrato != 0)
			{
				if (adr.vibrato == 255)
				{
					voice.vib = 0;
					voice.deltaVib = 0;
					voice.vibCnt = 0;
				}
				else
				{
					voice.vib = 0;
					voice.deltaVib = (short)(adr.vibrato & 15);
					voice.vibMax = (byte)(adr.vibrato >> 4);
					voice.vibCnt = (byte)(adr.vibrato >> 5);
				}
			}

			// Do phase
			if (adr.phase != 0)
			{
				if (adr.phase == 255)
				{
					voice.phase = 0;
					voice.deltaPhase = -1;
				}
				else
				{
					voice.phase = 0;
					voice.deltaPhase = (short)(adr.phase & 15);
				}
			}

			// Do volume
			short temp;
			if ((temp = adr.volume) == 0)
			{
				if ((adr.speed & 128) != 0)
				{
					voice.vol = temp;
					voice.volLevel = (ushort)temp;
					voice.deltaVol = 0;
				}
			}
			else
			{
				if (temp == 255)
					voice.deltaVol = 0;
				else
				{
					if ((adr.speed & 128) != 0)
					{
						voice.vol = temp;
						voice.volLevel = (ushort)temp;
						voice.deltaVol = 0;
					}
					else
					{
						temp &= 0x7f;
						if ((adr.volume & 128) != 0)
							temp = (short)-temp;

						voice.deltaVol = temp;
					}
				}
			}

			// Do portamento
			if ((temp = adr.porta) != 0)
			{
				if (temp == 255)
				{
					voice.por = 0;
					voice.deltaPor = 0;
				}
				else
				{
					voice.por = 0;
					if ((adr.speed & 64) != 0)
					{
						if (voice.porLevel <= voice.pers[0])
							temp = (short)-temp;
					}
					else
					{
						temp &= 0x7f;
						if ((adr.porta & 128) == 0)
						{
							temp = (short)-temp;
							voice.porLevel = 135;
						}
						else
							voice.porLevel = 1019;
					}

					voice.deltaPor = temp;
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
			if ((tmpDMACON & voice.dmacon) != 0)
			{
				Channel chan = voice.channel;

				// Setup the start sample
				if (voice.insAddress == null)
					chan.Mute();
				else
				{
					chan.PlaySample(voice.insAddress, 0, (uint)(voice.insLen * 2));
					chan.SetAmigaPeriod(voice.pers[0]);
				}

				// Check to see if sample loops
				if ((voice.flags & 1) == 0)
				{
					voice.insAddress = null;
					voice.realInsAddress = null;
					voice.insLen = 0;
				}

				// Setup loop
				if (voice.insAddress != null)
					chan.SetLoop(0, (uint)(voice.insLen * 2));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will setup the channel structure
		/// </summary>
		/********************************************************************/
		private void SetChannel(VoiceInfo voice)
		{
			Channel chan = voice.channel;

			while (voice.pers[0] == 0)
				RotatePeriods(voice);

			short per = (short)(voice.pers[0] + voice.por);
			if (voice.por < 0)
			{
				if (per < voice.porLevel)
					per = voice.porLevel;
			}
			else
			{
				if ((voice.por != 0) && (per > voice.porLevel))
					per = voice.porLevel;
			}

			// Add vibrato
			per += voice.vib;

			if (per < 135)
				per = 135;
			else if (per > 1019)
				per = 1019;

			chan.SetAmigaPeriod((uint)per);
			RotatePeriods(voice);

			voice.por += voice.deltaPor;
			if (voice.por < -1019)
				voice.por = -1019;
			else if (voice.por > 1019)
				voice.por = 1019;

			if (voice.vibCnt != 0)
			{
				voice.vib += voice.deltaVib;
				if (--voice.vibCnt == 0)
				{
					voice.deltaVib = (short)-voice.deltaVib;
					voice.vibCnt = voice.vibMax;
				}
			}

			chan.SetVolume((ushort)(voice.vol * 4));

			voice.vol += voice.deltaVol;
			if (voice.vol < 0)
				voice.vol = 0;
			else if (voice.vol > 64)
				voice.vol = 64;

			if (((voice.flags & 1) != 0) && (voice.deltaPhase != 0))
			{
				if (voice.deltaPhase < 0)
					voice.deltaPhase = 0;

				sbyte[] instData = voice.insAddress;
				sbyte[] wave = voice.realInsAddress;
				int wavePhase = voice.phase / 4;

				for (int i = 0; i < 64; i++)
				{
					short temp = wave[i];
					temp += wave[wavePhase++];
					temp >>= 1;
					instData[i] = (sbyte)temp;
				}

				voice.phase = (ushort)(voice.phase + voice.deltaPhase);
				if (voice.phase >= 256)
					voice.phase -= 256;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Rotates the periods in the pers array
		/// </summary>
		/********************************************************************/
		private void RotatePeriods(VoiceInfo voice)
		{
			ushort temp = voice.pers[0];
			voice.pers[0] = voice.pers[1];
			voice.pers[1] = voice.pers[2];
			voice.pers[2] = temp;
		}
		#endregion
	}
}
