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
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.Sawteeth.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.Sawteeth
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class SawteethWorker : ModulePlayerAgentBase
	{
		private const int CurrentFileVersion = 1200;
		private const int MaxChan = 12;
		private const int ChnSteps = 8192;

		private bool textVersion;
		public byte posChannel;
		private short songLen;

		private short[][] outBuffers;

		private string name;
		private string author;

		private ushort stVersion;
		public ushort spsPal;

		private byte channelCount;
		private byte partCount;
		private byte instrumentCount;
		private byte breakPCount;

		private ChannelInfo[] chan;
		public Part[] parts;
		public Ins[] ins;
		private BreakPoint[] breakPoints;

		private Player[] p;
		private uint pals;

		private bool looped;

		public float[] n2f;
		public float[] r2f;
		private float[] cMul;

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => new [] { "st" };



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 10)
				return AgentResult.Unknown;

			// Check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint mark = moduleStream.Read_B_UINT32();
			if ((mark != 0x53575444) && (mark != 0x53575454))		// SWTD && SWTT
				return AgentResult.Unknown;

			textVersion = mark == 0x53575454;

			if (textVersion)
			{
				// Read the mark properly
				moduleStream.Seek(0, SeekOrigin.Begin);
				ReadString(moduleStream);
			}

			// Check the version
			ushort ver = Read16Bit(moduleStream);
			if (ver > CurrentFileVersion)
				return AgentResult.Unknown;

			// Check the position length if not a closed beta
			if (ver >= 900)
			{
				if (Read16Bit(moduleStream) < 1)
					return AgentResult.Unknown;
			}

			// Check the number of channels
			byte chan = Read8Bit(moduleStream);
			if ((chan < 1) || (chan > MaxChan))
				return AgentResult.Unknown;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the module
		/// </summary>
		/********************************************************************/
		public override string ModuleName => name;



		/********************************************************************/
		/// <summary>
		/// Return the name of the author
		/// </summary>
		/********************************************************************/
		public override string Author => author;



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
				// Song version
				case 0:
				{
					description = Resources.IDS_SAW_INFODESCLINE0;
					value = (stVersion / 1000.0f).ToString("F2");
					break;
				}

				// Song length
				case 1:
				{
					description = Resources.IDS_SAW_INFODESCLINE1;
					value = songLen.ToString();
					break;
				}

				// Used parts
				case 2:
				{
					description = Resources.IDS_SAW_INFODESCLINE2;
					value = partCount.ToString();
					break;
				}

				// Used instruments
				case 3:
				{
					description = Resources.IDS_SAW_INFODESCLINE3;
					value = instrumentCount.ToString();
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
		public override ModulePlayerSupportFlag SupportFlags => ModulePlayerSupportFlag.BufferMode | ModulePlayerSupportFlag.SetPosition;



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
				if (textVersion)
					ReadString(moduleStream);
				else
					moduleStream.Seek(4, SeekOrigin.Begin);

				// Get the version
				stVersion = Read16Bit(moduleStream);

				if (stVersion < 900)		// Special hack for compatibility with CLOSED_BETA
					spsPal = 882;
				else
					spsPal = Read16Bit(moduleStream);

				//
				// Channels
				//
				channelCount = Read8Bit(moduleStream);
				chan = new ChannelInfo[channelCount];

				for (int i = 0; i < channelCount; i++)
				{
					ChannelInfo chanInfo = new ChannelInfo();

					chanInfo.Left = Read8Bit(moduleStream);
					chanInfo.Right = Read8Bit(moduleStream);
					chanInfo.Len = Read16Bit(moduleStream);

					// Previous we did not have loop points
					if (stVersion < 910)
						chanInfo.LLoop = 0;
					else
						chanInfo.LLoop = Read16Bit(moduleStream);

					// Previous we did not have right loop points
					if (stVersion < 1200)
						chanInfo.RLoop = (ushort)(chanInfo.Len - 1);
					else
						chanInfo.RLoop = Read16Bit(moduleStream);

					// Check the channel length
					if ((chanInfo.Len < 1) || (chanInfo.Len > ChnSteps))
					{
						errorMessage = Resources.IDS_SAW_ERR_LOADING_CHANNELS;
						Cleanup();

						return AgentResult.Error;
					}

					if (chanInfo.RLoop >= chanInfo.Len)
						chanInfo.RLoop = (ushort)(chanInfo.Len - 1);

					// Sequence
					chanInfo.Steps = new ChStep[chanInfo.Len];

					for (int j = 0; j < chanInfo.Len; j++)
					{
						ChStep step = new ChStep();

						step.Part = Read8Bit(moduleStream);
						step.Transp = (sbyte)Read8Bit(moduleStream);
						step.DAmp = Read8Bit(moduleStream);

						chanInfo.Steps[j] = step;
					}

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_SAW_ERR_LOADING_CHANNELS;
						Cleanup();

						return AgentResult.Error;
					}

					chan[i] = chanInfo;
				}

				//
				// Parts
				//
				partCount = Read8Bit(moduleStream);
				if (partCount < 1)
				{
					errorMessage = Resources.IDS_SAW_ERR_LOADING_PARTS;
					Cleanup();

					return AgentResult.Error;
				}

				parts = new Part[partCount];

				// Read the part information
				for (int i = 0; i < partCount; i++)
				{
					Part part = new Part();

					part.Sps = Read8Bit(moduleStream);
					if (part.Sps < 1)
					{
						errorMessage = Resources.IDS_SAW_ERR_LOADING_PARTS;
						Cleanup();

						return AgentResult.Error;
					}

					part.Len = Read8Bit(moduleStream);
					if (part.Len < 1)
					{
						errorMessage = Resources.IDS_SAW_ERR_LOADING_PARTS;
						Cleanup();

						return AgentResult.Error;
					}

					part.Steps = new Step[part.Len];

					for (int j = 0; j < part.Len; j++)
					{
						Step step = new Step();

						step.Ins = Read8Bit(moduleStream);
						step.Eff = Read8Bit(moduleStream);
						step.Note = Read8Bit(moduleStream);

						part.Steps[j] = step;
					}

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_SAW_ERR_LOADING_PARTS;
						Cleanup();

						return AgentResult.Error;
					}

					parts[i] = part;
				}

				// Check to see if any channels points to a part that does not exist
				for (int i = 0; i < channelCount; i++)
				{
					for (int j = 0; j < chan[i].Len; j++)
					{
						if (chan[i].Steps[j].Part >= partCount)
							chan[i].Steps[j].Part = (byte)(partCount - 1);
					}
				}

				//
				// Instruments
				//
				instrumentCount = (byte)(Read8Bit(moduleStream) + 1);
				if (instrumentCount < 2)
				{
					errorMessage = Resources.IDS_SAW_ERR_LOADING_INSTRUMENTS;
					Cleanup();

					return AgentResult.Error;
				}

				ins = new Ins[instrumentCount];

				// Allocate dummy instrument
				ins[0] = new Ins();

				ins[0].FilterPoints = 1;
				ins[0].AmpPoints = 1;
				ins[0].Filter = Helpers.InitializeArray<TimeLev>(1);
				ins[0].Amp = Helpers.InitializeArray<TimeLev>(1);

				ins[0].FilterMode = 0;
				ins[0].ClipMode = 0;
				ins[0].Boost = 1;

				ins[0].Sps = 30;
				ins[0].Res = 0;

				ins[0].VibS = 1;
				ins[0].VibD = 1;

				ins[0].PwmS = 1;
				ins[0].PwmD = 1;

				ins[0].Len = 0;
				ins[0].Loop = 0;

				ins[0].Steps = Helpers.InitializeArray<InsStep>(1);

				ins[0].Filter[0].Time = 0;
				ins[0].Filter[0].Lev = 0;

				ins[0].Amp[0].Time = 0;
				ins[0].Amp[0].Lev = 0;

				ins[0].Steps[0].Note = 0;
				ins[0].Steps[0].Relative = false;
				ins[0].Steps[0].WForm = 0;

				// Read the instruments
				for (int i = 1; i < instrumentCount; i++)
				{
					Ins inst = new Ins();

					inst.FilterPoints = Read8Bit(moduleStream);
					if (inst.FilterPoints < 1)
					{
						errorMessage = Resources.IDS_SAW_ERR_LOADING_INSTRUMENTS;
						Cleanup();

						return AgentResult.Error;
					}

					inst.Filter = Helpers.InitializeArray<TimeLev>(inst.FilterPoints);

					for (int j = 0; j < inst.FilterPoints; j++)
					{
						inst.Filter[j].Time = Read8Bit(moduleStream);
						inst.Filter[j].Lev = Read8Bit(moduleStream);
					}

					inst.AmpPoints = Read8Bit(moduleStream);
					if (inst.AmpPoints < 1)
					{
						errorMessage = Resources.IDS_SAW_ERR_LOADING_INSTRUMENTS;
						Cleanup();

						return AgentResult.Error;
					}

					inst.Amp = Helpers.InitializeArray<TimeLev>(inst.AmpPoints);

					for (int j = 0; j < inst.AmpPoints; j++)
					{
						inst.Amp[j].Time = Read8Bit(moduleStream);
						inst.Amp[j].Lev = Read8Bit(moduleStream);
					}

					inst.FilterMode = Read8Bit(moduleStream);
					inst.ClipMode = Read8Bit(moduleStream);
					inst.Boost = (byte)(inst.ClipMode & 15);
					inst.ClipMode >>= 4;

					inst.VibS = Read8Bit(moduleStream);
					inst.VibD = Read8Bit(moduleStream);
					inst.PwmS = Read8Bit(moduleStream);
					inst.PwmD = Read8Bit(moduleStream);
					inst.Res = Read8Bit(moduleStream);
					inst.Sps = Read8Bit(moduleStream);

					if (inst.Sps < 1)
					{
						errorMessage = Resources.IDS_SAW_ERR_LOADING_INSTRUMENTS;
						Cleanup();

						return AgentResult.Error;
					}

					if (stVersion < 900)
					{
						byte tmp = Read8Bit(moduleStream);
						inst.Len = (byte)(tmp & 127);
						inst.Loop = (byte)((tmp & 1) != 0 ? 0 : inst.Len - 1);
					}
					else
					{
						inst.Len = Read8Bit(moduleStream);
						inst.Loop = Read8Bit(moduleStream);

						if (inst.Loop >= inst.Len)
						{
							errorMessage = Resources.IDS_SAW_ERR_LOADING_INSTRUMENTS;
							Cleanup();

							return AgentResult.Error;
						}
					}

					if (inst.Len < 1)
					{
						errorMessage = Resources.IDS_SAW_ERR_LOADING_INSTRUMENTS;
						Cleanup();

						return AgentResult.Error;
					}

					inst.Steps = Helpers.InitializeArray<InsStep>(inst.Len);

					for (int j = 0; j < inst.Len; j++)
					{
						byte temp = Read8Bit(moduleStream);

						inst.Steps[j].Relative = (temp & 0x80) != 0;
						inst.Steps[j].WForm = (byte)(temp & 0xf);
						inst.Steps[j].Note = Read8Bit(moduleStream);
					}

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_SAW_ERR_LOADING_INSTRUMENTS;
						Cleanup();

						return AgentResult.Error;
					}

					ins[i] = inst;
				}

				//
				// Break points
				//
				breakPCount = Read8Bit(moduleStream);
				breakPoints = Helpers.InitializeArray<BreakPoint>(breakPCount);

				for (int i = 0; i < breakPCount; i++)
				{
					breakPoints[i].Pal = Read32Bit(moduleStream);
					breakPoints[i].Command = Read32Bit(moduleStream);
				}

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_SAW_ERR_LOADING_BREAKPOINTS;
					Cleanup();

					return AgentResult.Error;
				}

				//
				// Names
				//
				name = ReadString(moduleStream);
				author = ReadString(moduleStream);

				for (int i = 0; i < partCount; i++)
					parts[i].Name = ReadString(moduleStream);

				for (int i = 1; i < instrumentCount; i++)
					ins[i].Name = ReadString(moduleStream);
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
			// Create player objects
			p = new Player[channelCount];

			for (byte i = 0; i < channelCount; i++)
				p[i] = new Player(this, chan[i], i);

			// Allocate buffers to hold the output to play
			outBuffers = new short[channelCount][];

			for (int i = 0; i < channelCount; i++)
				outBuffers[i] = new short[spsPal];

			// Initialize the player
			Init();

			// Find the channel with the maximum number of positions
			songLen = 0;
			for (byte i = 0; i < channelCount; i++)
			{
				if (chan[i].RLoop >= songLen)
				{
					songLen = (short)(chan[i].RLoop + 1);
					posChannel = i;
				}
			}

			songLen++;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public override void CleanupPlayer()
		{
			r2f = null;
			n2f = null;
			cMul = null;

			outBuffers = null;
			p = null;

			Cleanup();
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the current song
		/// </summary>
		/********************************************************************/
		public override void InitSound(int songNumber, DurationInfo durationInfo)
		{
			for (int c = 0; c < channelCount; c++)
				p[c].Init();

			InitSong();
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the duration for all sub-songs
		/// </summary>
		/********************************************************************/
		public override DurationInfo[] CalculateDuration()
		{
			ChStep[] step = chan[posChannel].Steps;
			float framePerSec = 44100.0f / spsPal;
			float total = 0.0f;

			PositionInfo[] positionInfo = new PositionInfo[songLen];

			for (int i = 0; i < step.Length; i++)
			{
				positionInfo[i] = new PositionInfo(new TimeSpan((long)(total * TimeSpan.TicksPerMillisecond)));

				// Find the time the current position use
				total += (1000.0f * (parts[step[i].Part].Sps * parts[step[i].Part].Len) / framePerSec);
			}

			// Return the duration
			return new [] { new DurationInfo(new TimeSpan((long)(total * TimeSpan.TicksPerMillisecond)), positionInfo) };
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			// Find the multiply value
			float channelMul = cMul[channelCount] / 255.0f;

			// Get the buffer for each channel and create the output
			for (int c = 0; c < channelCount; c++)
			{
				if (p[c].NextBuffer())
					MemMulMove(outBuffers[c], p[c].Buffer, spsPal, 255.0f * channelMul);
				else
					Array.Clear(outBuffers[c], 0, spsPal);

				// Calculate panning value
				ushort pan = (ushort)((((256 - chan[c].Left) * 256) + (256 * chan[c].Right)) / 512);

				// Tell NostalgicPlayer what to play in this channel
				VirtualChannels[c].PlaySample(outBuffers[c], 0, spsPal, 16);
				VirtualChannels[c].SetFrequency(44100);
				VirtualChannels[c].SetVolume(256);
				VirtualChannels[c].SetPanning(pan);
			}

			// PAL looping
			pals++;

			if (p[0].Looped)
			{
				looped = true;
				pals = 0;

				for (int cnt = 0; cnt < chan[0].LLoop; cnt++)
					pals += (uint)(parts[chan[0].Steps[cnt].Part].Sps * parts[chan[0].Steps[cnt].Part].Len);
			}

			// Break point part
			if (looped)
				looped = false;
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public override int ModuleChannelCount => channelCount;



		/********************************************************************/
		/// <summary>
		/// Return the length of the current song
		/// </summary>
		/********************************************************************/
		public override int SongLength => songLen;



		/********************************************************************/
		/// <summary>
		/// Return the current position of the song
		/// </summary>
		/********************************************************************/
		public override int GetSongPosition()
		{
			return (int)p[posChannel].GetSeqPos();
		}



		/********************************************************************/
		/// <summary>
		/// Set a new position of the song
		/// </summary>
		/********************************************************************/
		public override void SetSongPosition(int position, PositionInfo positionInfo)
		{
			uint pal = 0;

			// Find the pal value for the position to set
			ChStep[] step = chan[posChannel].Steps;
			for (int i = 0; i < position; i++)
				pal += (uint)(parts[step[i].Part].Sps * parts[step[i].Part].Len);

			// Now set the position
			SetPos(pal);
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

				foreach (Ins inst in ins)
				{
					SampleInfo sampleInfo = new SampleInfo
					{
						Name = inst.Name,
						Flags = SampleInfo.SampleFlags.None,
						Type = SampleInfo.SampleType.Synth,
						BitSize = 16,
						MiddleC = 0,
						Volume = 256,
						Panning = -1,
						Sample = null,
						Length = 0,
						LoopStart = 0,
						LoopLength = 0
					};

					result.Add(sampleInfo);
				}

				return result.ToArray();
			}
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Tells that the module has ended
		/// </summary>
		/********************************************************************/
		public void EndReached()
		{
			OnEndReached();
		}



		/********************************************************************/
		/// <summary>
		/// Tells that the position has changed
		/// </summary>
		/********************************************************************/
		public void ChangePosition()
		{
			OnPositionChanged();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player has allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			breakPoints = null;
			ins = null;
			parts = null;
			chan = null;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize all the player structures etc.
		/// </summary>
		/********************************************************************/
		private void Init()
		{
			// Calc multi channels
			{
				const float Floor = 0.1f;

				cMul = new float[MaxChan];
				for (int c = 0; c < MaxChan; c++)
					cMul[c] = ((1.0f - Floor) / c) + Floor;
			}

			// Freq-tables
			{
				const double Mul = 1.0594630943593;

				n2f = new float[22 * 12];
				r2f = new float[22 * 12];

				int count = 0;
				double octBase = 1.02197486445547712033;

				for (int oc = 0; oc < 22; oc++)
				{
					double bas = octBase;
					for (int c = 0; c < 12; c++)
					{
						n2f[count++] = (float)bas;
						bas *= Mul;
					}

					octBase *= 2.0;
				}

				count = 0;
				octBase = 1.0;

				for (int oc = 0; oc < 22; oc++)
				{
					double bas = octBase;
					for (int c = 0; c < 12; c++)
					{
						r2f[count++] = (float)bas;
						bas *= Mul;
					}

					octBase *= 2.0;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Initialize all the song variables
		/// </summary>
		/********************************************************************/
		private void InitSong()
		{
			looped = false;
			pals = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Change the song position to the number of pals given
		/// </summary>
		/********************************************************************/
		private void SetPos(uint newPals)
		{
			pals = newPals;

			for (int c = 0; c < channelCount; c++)
			{
				// Set seq count
				int sum = 0;
				int seqCount = 0;
				int sps = 0;
				int len = 0;

				while (sum <= (int)pals)
				{
					len = parts[chan[c].Steps[seqCount].Part].Len;
					sps = parts[chan[c].Steps[seqCount].Part].Sps;
					sum += sps * len;

					if (seqCount > chan[c].RLoop)
						seqCount = chan[c].LLoop;

					seqCount++;
				}

				if (seqCount > 0)
					seqCount--;
				else
					seqCount = chan[c].RLoop;

				len = parts[chan[c].Steps[seqCount].Part].Len;
				sps = parts[chan[c].Steps[seqCount].Part].Sps;

				sum -= sps * len;

				int stepCount = (int)((pals - sum) / sps);
				int palCount = (int)((pals - sum) % sps);

				p[c].JumpPos((byte)seqCount, (byte)stepCount, (byte)palCount);

				if (c == 0)
				{
					pals = 0;

					for (int cnt = 0; cnt < seqCount; cnt++)
						pals += (uint)(parts[chan[0].Steps[cnt].Part].Sps * parts[chan[0].Steps[cnt].Part].Len);

					pals += (uint)(stepCount * parts[chan[0].Steps[seqCount].Part].Sps);
					pals += (uint)palCount;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Converts the sample output from float to 16-bit for a single
		/// channel
		/// </summary>
		/********************************************************************/
		private void MemMulMove(short[] d, float[] s, uint count, float level)
		{
			level *= 32768.0f;

			int index = 0;

			while (count > 4)
			{
				d[index] = (short)(s[index] * level);
				index++;
				d[index] = (short)(s[index] * level);
				index++;
				d[index] = (short)(s[index] * level);
				index++;
				d[index] = (short)(s[index] * level);
				index++;

				count -= 4;
			}

			while (count > 0)
			{
				d[index] = (short)(s[index] * level);
				index++;
				count--;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will read a string from the stream given
		/// </summary>
		/********************************************************************/
		private string ReadString(ModuleStream moduleStream)
		{
			byte[] buf = new byte[300];

			int i;
			for (i = 0; i < buf.Length - 1; i++)
			{
				byte byt = moduleStream.Read_UINT8();
				if (moduleStream.EndOfStream)
					break;

				if (byt == 0x0d)
				{
					i--;
					continue;
				}

				if ((byt == 0x00) || (byt == 0x0a))
					break;

				buf[i] = byt;
			}

			// Decode the string and return it
			return Encoding.UTF8.GetString(buf, 0, i);
		}



		/********************************************************************/
		/// <summary>
		/// Will read a 8 bit number from the stream given
		/// </summary>
		/********************************************************************/
		private byte Read8Bit(ModuleStream moduleStream)
		{
			if (textVersion)
			{
				string str;

				for (;;)
				{
					str = ReadString(moduleStream);
					if (string.IsNullOrEmpty(str))
						continue;

					if (!str.StartsWith("//"))
						break;
				}

				return Convert.ToByte(str);
			}

			return moduleStream.Read_UINT8();
		}



		/********************************************************************/
		/// <summary>
		/// Will read a 16 bit number from the stream given
		/// </summary>
		/********************************************************************/
		private ushort Read16Bit(ModuleStream moduleStream)
		{
			if (textVersion)
			{
				string str;

				for (;;)
				{
					str = ReadString(moduleStream);
					if (string.IsNullOrEmpty(str))
						continue;

					if (!str.StartsWith("//"))
						break;
				}

				return Convert.ToUInt16(str);
			}

			return moduleStream.Read_B_UINT16();
		}



		/********************************************************************/
		/// <summary>
		/// Will read a 32 bit number from the stream given
		/// </summary>
		/********************************************************************/
		private uint Read32Bit(ModuleStream moduleStream)
		{
			if (textVersion)
			{
				string str;

				for (;;)
				{
					str = ReadString(moduleStream);
					if (string.IsNullOrEmpty(str))
						continue;

					if (!str.StartsWith("//"))
						break;
				}

				return Convert.ToUInt32(str);
			}

			return moduleStream.Read_B_UINT32();
		}
		#endregion
	}
}
