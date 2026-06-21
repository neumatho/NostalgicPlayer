/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris.Dns.Containers;
using Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris.Dns
{
	/// <summary>
	/// 
	/// </summary>
	public class DnsDecoder : Decoder
	{
		private const c_int Tracks_Max = 6;
		private const c_int Voices_Max = 6;		// Really 4, but can run with 6 voices too

		private readonly ModuleOffsets offsets = new ModuleOffsets();

		private readonly PaulaVoice[] dummyVoices = ArrayHelper.InitializeArray<PaulaVoice>(Voices_Max);

		private c_int voices;
		private udword songPosCurrent;
		private readonly SmartPtr<ubyte> pBuf = new SmartPtr<ubyte>();	// For safe unsigned access

		private VoiceVars[] voiceVars = ArrayHelper.InitializeArray<VoiceVars>(Voices_Max);
		private readonly PaulaVoice[] paulaVoices = new PaulaVoice[Voices_Max];		// Paula and mixer interface (TNE: Moved this out from VoiceVars into its own array. If not, snapshot does not work)

		private PlayerInfo playerInfo;
		private c_int trackCount;
		private c_int sampleCount;

		private
		(
			bool HppIngame,
			bool Starball,
			bool StarballIngame,
			bool Ptc
		) variant;

		private
		(
			CPointer<ubyte> Buf,		// The allocated buffer
			udword BufLen,				// The allocated amount
			udword Len,					// Length of the data we copy

			c_uint MdatSize,
			c_uint SmplSize,
			c_uint MdatSizeCurrent,
			c_uint SmplSizeCurrent,
			bool SmplLoaded
		) input;

		private static readonly uword[] periods =
		[
			0x06ae, 0x064e, 0x05f4, 0x059e, 0x054d, 0x0501, 0x04b9, 0x0475,
			0x0435, 0x03f9, 0x03c0, 0x038c, 

			// +0x0c
			0x0358, 0x032a, 0x02fc, 0x02d0, 0x02a8, 0x0282, 0x025e, 0x023b, 
			0x021b, 0x01fd, 0x01e0, 0x01c6,

			// +0x18
			0x01ac, 0x0194, 0x017d, 0x0168, 0x0154, 0x0140, 0x012f, 0x011e,
			0x010e, 0x00fe, 0x00f0, 0x00e3, 

			// +0x24
			0x00d6, 0x00ca, 0x00bf, 0x00b4, 0x00aa, 0x00a0, 0x0097, 0x008f,
			0x0087, 0x007f, 0x0078, 0x0071,

			// +0x30
		];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DnsDecoder()
		{
			playerInfo = new PlayerInfo
			{
				Track = ArrayHelper.InitializeArray<Track>(Tracks_Max)
			};


			input.Buf.SetToNull();
			input.BufLen = input.Len = 0;
			input.SmplLoaded = false;

			playerInfo.Admin.Initialized = false;

			// Set up some dummy voices to decouple the decoder from the mixer
			for (ubyte v = 0; v < Voices_Max; v++)
				paulaVoices[v] = dummyVoices[v];

			loopMode = false;
		}



		/********************************************************************/
		/// <summary>
		/// Since there are only three publicly released game soundtracks
		/// made with Dynamic Synthesizer and their player machine code and
		/// setup parameters differ quite a bit, we don't overdo with the
		/// probing. There is no header structure, and trying to identify the
		/// music module based on searching for machine code fragments and
		/// data pointers/offsets within it isn't worthwhile. Instead, we
		/// rely on checksums, also during the initialization step
		/// </summary>
		/********************************************************************/
		public override bool Detect(IPointer data, udword len)
		{
			CPointer<ubyte> d = data.ToPointer<ubyte>();
			SmartPtr<ubyte> sBuf = new SmartPtr<ubyte>(d, len);
			bool maybe = false;

			// Invalidate these two essential ones
			playerInfo.Admin.Initialized = false;
			playerInfo.Admin.Checksum = 0;

			// The ripped modules start with player machine code, and at least
			// five BRA main entry points must be present before we take
			// a closer look
			if ((len >= 18) &&
				(d[0] == 0x60) && (d[4] == 0x60) && (d[8] == 0x60) && (d[12] == 0x60) && (d[16] == 0x60) &&
				(d[1] == 0) && (d[5] == 0) && (d[9] == 0) && (d[13] == 0) && (d[17] == 0) &&
				// Also check the branch offset to the "off" routine,
				// which is located fairly near the beginning for all modules
				(MyEndian.MakeWord(d[6], d[7]) < 0x100))
			{
				maybe = true;
			}
			else
				return false;

			// If at least 256 bytes of the music file are available, such as
			// during initialization, calculate a primary checksum
			if (len >= 0x100)
			{
				udword crc1 = CrcLight.Get(sBuf, 0, 0x100);

				udword[] checksum1 =
				[
					0x4f593264,		// Hollywood Poker Pro title
					0x32db24cc,		// Hollywood Poker Pro ingame
					0xf195c217,		// Starball title
					0x48ea5657,		// Starball ingame
					0xd9511944		// PTC
				];

				if (!checksum1.Contains(crc1))
					return false;

				// We use this checksum also in main init()
				playerInfo.Admin.Checksum = crc1;
			}

			// If at least 2048 bytes of the music data are available, such as
			// during initialization, verify a second checksum
			if (len >= 0x800)
			{
				udword crc2 = CrcLight.Get(sBuf, 0x800 - 0x100, 0x100);

				udword[] checksum2 =
				[
					0x04ec1634,		// Hollywood Poker Pro title
					0xb5e99994,		// Hollywood Poker Pro ingame
					0x07ecc48b,		// Starball title
					0xd4c2b312,		// Starball ingame
					0xefb03b1a		// PTC
				];

				if (!checksum2.Contains(crc2))
					return false;
			}

			if (maybe)
			{
				offsets.Header = 0;

				return true;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override bool Init(IPointer data, udword length, IPointer sample, udword sampleLength, out string errorMessage)
		{
			errorMessage = string.Empty;

			if (data.IsNull || (length == 0))	// Re-init mode
			{
				if (!playerInfo.Admin.Initialized)
				{
					errorMessage = Resources.IDS_ERR_NOT_INITIALIZED;
					return false;
				}
			}
			else
			{
				// Invalidate what has been found out before
				input.SmplLoaded = false;
				input.MdatSize = input.SmplSize = 0;

				if (!Detect(data, length))
				{
					errorMessage = Resources.IDS_ERR_UNKNOWN_FORMAT;
					return false;
				}

				// If we still have a sufficiently large buffer, reuse it
				udword newLen = length;

				if (newLen > input.BufLen)
				{
					input.BufLen = 0;
					input.Buf = new CPointer<ubyte>(newLen);
				}

				CMemory.memcpy(input.Buf, data, length);
				input.BufLen = newLen;
				input.Len = length;

				// Set up smart pointer for unsigned input buffer access
				pBuf.SetBuffer(input.Buf, input.BufLen);
			}

			if (LoadSamplesFile(sample, sampleLength))
			{
				input.SmplLoaded = true;

				// Make these the current ones
				input.MdatSizeCurrent = input.MdatSize;
				input.SmplSizeCurrent = input.SmplSize;
			}
			else
			{
				// Can't proceed without samples data
				errorMessage = Resources.IDS_ERR_NO_SAMPLES;
				return false;
			}

			offsets.SampleData = offsets.Header + input.MdatSize;
			offsets.Silence = offsets.SampleData;

			// Clear the first two words for one-shot samples
			udword o = offsets.SampleData;
			pBuf[o] = pBuf[o + 1] = pBuf[o + 2] = pBuf[o + 3] = 0;

			// Defaults
			SetRate(50 << 8);
			voices = 4;

			playerInfo.Sequencer.Tracks = 6;
			playerInfo.Sequencer.Step.Size = 24;
			playerInfo.Sequencer.Tables = 1;
			playerInfo.Sequencer.TableSize = 0;		// Irrelevant, if it's only one table

			// Despite the description "DYNAMIC SYNTHESIZER MODUL V1.34" in all
			// but one of the music modules, there are some differences in the
			// music data format and the machine code players. The features are
			// the same though.
			// 
			// Assuming "Hollywood Poker Pro" title as a default, we handle the
			// differences via some booleans
			variant.HppIngame = false;
			variant.Starball = variant.StarballIngame = false;
			variant.Ptc = false;

			// Hollywood Poker Pro title
			if (playerInfo.Admin.Checksum == 0x4f593264)
			{
				offsets.Base = 0x1f400;
				offsets.TrackTable = 0xdc4;
				offsets.SongDefs = 0x1244;
				offsets.Patterns = 0x1324;
				offsets.SampleHeaders = 0x1fd4;
				playerInfo.Admin.Songs = 1;					// Actually 8 at different start positions
				MyEndian.WriteBEWord(pBuf, offsets.SampleHeaders + (0x2 * 0x60) + 8 + 6, 0x7e2);
				MyEndian.WriteBEWord(pBuf, offsets.SampleHeaders + (0xf * 0x60) + 8 + 6, 0x7e2);
				playerInfo.Sequencer.Tables = 1;
			}
			// Hollywood Poker Pro ingame
			else if (playerInfo.Admin.Checksum == 0x32db24cc)
			{
				offsets.Base = 0x1f400;
				offsets.TrackTable = 0x19b0;
				offsets.SongDefs = 0x27d8;
				offsets.Patterns = 0x28b8;					// The last pattern pointers are filled with the address of the first sample header
				offsets.SampleHeaders = 0x39c0;
				playerInfo.Admin.Songs = 5;
				variant.HppIngame = true;
				playerInfo.Sequencer.Tables = 1;
			}
			// Starball title
			else if (playerInfo.Admin.Checksum == 0xf195c217)
			{
				offsets.Base = 0x7a9e;
				offsets.TrackTable = 0xe56;
				offsets.SongDefs = 0x10d6;
				offsets.Patterns = 0x11b6;
				offsets.SampleHeaders = 0x1c86;
				playerInfo.Sequencer.Tables = 3;			// It uses separate arrays for PT, ST, TR values
				playerInfo.Sequencer.TableSize = 0x20;
				playerInfo.Sequencer.Step.Size = 1;
				playerInfo.Admin.Songs = 1;
				variant.Starball = true;
			}
			// Starball ingame
			else if (playerInfo.Admin.Checksum == 0x48ea5657)
			{
				offsets.Base = 0x4be0;
				offsets.TrackTable = 0x10a2;
				offsets.SongDefs = 0x1562 + 2;				// +2 because first song is silent
				offsets.Patterns = 0x1642;
				offsets.SampleHeaders = 0x2642;
				playerInfo.Sequencer.Tables = 3;			// It uses separate arrays for PT, ST, TR values
				playerInfo.Sequencer.TableSize = 0x40;
				playerInfo.Sequencer.Step.Size = 1;
				playerInfo.Admin.Songs = 6;
				variant.Starball = variant.StarballIngame = true;
			}
			// PTC
			else if (playerInfo.Admin.Checksum == 0xd9511944)
			{
				offsets.Base = 0x0;
				offsets.TrackTable = 0xdac;
				offsets.SongDefs = 0x198c;
				offsets.Patterns = 0x1a8c;					// But no array of pattern offsets
				offsets.SampleHeaders = 0x2e0c;
				playerInfo.Sequencer.Tables = 3;			// It uses separate arrays for PT, ST, TR values
				playerInfo.Sequencer.TableSize = 0xa0;
				playerInfo.Sequencer.Step.Size = 1;
				playerInfo.Admin.Songs = 3;
				variant.Ptc = true;
				SetRate(100 << 8);
			}

			Restart();

			// Find number of tracks used
			trackCount = 0;
			sampleCount = 0;

			HashSet<ubyte> takenTracks = new HashSet<ubyte>();

			for (c_int i = 0; i < playerInfo.Admin.Songs; i++)
			{
				uword so = (uword)(i << 1);
				uword first = MyEndian.ReadBEUword(pBuf, offsets.SongDefs + so);
				uword last = MyEndian.ReadBEUword(pBuf, offsets.SongDefs + 0x40 + so);

				for (c_int j = first; j < last; j++)
				{
					o = (udword)(offsets.TrackTable + (j * playerInfo.Sequencer.Step.Size));

					for (ubyte t = 0; t < playerInfo.Sequencer.Tracks; t++)
					{
						ubyte trackNumber = pBuf[o];
						trackCount = Math.Max(trackCount, trackNumber);

						ubyte sampleTranspose;

						if (playerInfo.Sequencer.Tables == 1)
						{
							sampleTranspose = pBuf[o + 2];
							o += 4;
						}
						else
						{
							uword gap = (uword)(playerInfo.Sequencer.Tracks * playerInfo.Sequencer.TableSize);

							sampleTranspose = pBuf[o + (2U * gap)];
							o += playerInfo.Sequencer.TableSize;
						}

						if ((trackNumber != 0) && takenTracks.Add(trackNumber))
						{
							udword pattOffs = GetPattOffset((ubyte)(trackNumber - 1));
							udword pattPos = 0;

							do
							{
								uword pattVal = MyEndian.ReadBEUword(pBuf, pattOffs + pattPos);

								if ((pattVal != 0) && (pattVal != 0x8000))
								{
									ubyte sampleNumber = (ubyte)((pattVal & 0x0f) + sampleTranspose);
									sampleCount = Math.Max(sampleCount, sampleNumber);
								}

								pattPos += 2;
							}
							while (
								((playerInfo.Sequencer.Pattern.Length == 0) && (MyEndian.ReadBEUword(pBuf, pattOffs + pattPos) != 0xffff)) ||
								((playerInfo.Sequencer.Pattern.Length != 0) && (pattPos < playerInfo.Sequencer.Pattern.Length)));
						}
					}
				}
			}

			trackCount++;

			// TNE: The rest of the Init() function has been moved into InitSong()

			return playerInfo.Admin.Initialized;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override void InitSong(c_int songNumber)
		{
			playerInfo.Admin.StartSong = (songNumber < 0) ? 0 : songNumber;
			Restart();
		}



		/********************************************************************/
		/// <summary>
		/// With an initialized decoder, calling this should (re)start the
		/// decoder currently chosen song
		/// </summary>
		/********************************************************************/
		public override void Restart()
		{
			// Assign default values to essential runtime variables
			for (ubyte t = 0; t < playerInfo.Sequencer.Tracks; t++)
			{
				Track tr = playerInfo.Track[t];

				tr.Num = t;
				tr.Off = false;
				tr.KeyDown = false;
				tr.AssignedVoiceNum = 0xff;
				tr.Pt = 0;
			}

			for (ubyte vNum = 0; vNum < voices; ++vNum)
			{
				VoiceVars v = voiceVars[vNum];
				PaulaVoice paulaVoice = paulaVoices[vNum];

				v.Envelope.Phase = EnvPhase.End;
				v.Envelope.Volume = 0;
				v.Envelope.Duration = 0xffff;
				v.Envelope.KeyUp = true;

				v.SampleHeader = 0;
				v.PipelineState = 0;

				paulaVoice.Off();
				ToPaulaLength(v, paulaVoice, 1);
				ToPaulaStart(v, paulaVoice, offsets.Silence);

				paulaVoice.Paula.Volume = 0;
				paulaVoice.Paula.Period = 0;
			}

			SoftRestart();
			ProcessSequencer();

			playerInfo.Admin.NextVoiceNum = 0;
			playerInfo.Admin.Initialized = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override c_int Run()
		{
			if (!playerInfo.Admin.Initialized)
				return 0;

			realSongEnd = false;

			if (!songEnd || loopMode)
			{
				// The main play loop differs slightly between variants.
				// Not a big deal, but because voices get assigned dynamically,
				// we don't want to deviate from the machine code players much
				if (variant.Starball || variant.Ptc)
				{
					UpdateVoices();

					if (--playerInfo.Admin.Count == 0)
					{
						playerInfo.Admin.Count = playerInfo.Admin.Speed;

						for (ubyte t = 0; t < playerInfo.Sequencer.Tracks; t++)
							ProcessPattern(playerInfo.Track[t]);

						playerInfo.Sequencer.Pattern.Pos += 2;

						if (playerInfo.Sequencer.Pattern.Pos >= playerInfo.Sequencer.Pattern.Length)
							ProcessSequencer();
					}
				}
				else
				{
					if (--playerInfo.Admin.Count <= 0)
					{
						playerInfo.Admin.Count = playerInfo.Admin.Speed;

						if (playerInfo.Sequencer.Step.Next)		// Pattern can trigger next step
							ProcessSequencer();

						for (ubyte t = 0; t < playerInfo.Sequencer.Tracks; t++)
							ProcessPattern(playerInfo.Track[t]);

						playerInfo.Sequencer.Pattern.Pos += 2;
					}

					UpdateVoices();
				}
			}

			tickFpAdd += tickFp;
			c_int tick = (c_int)(tickFpAdd >> 8);
			tickFpAdd &= 0xff;
			songPosCurrent += (uint)tick;

			return tick;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override c_int GetSongs()
		{
			return playerInfo.Admin.Songs;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override ubyte GetVoices()
		{
			return (ubyte)voices;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override void SetPaulaVoice(ubyte v, PaulaVoice p)
		{
			paulaVoices[v] = p;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override c_int GetPositions()
		{
			return playerInfo.Sequencer.Step.Last - playerInfo.Sequencer.Step.First;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override c_int GetTracks()
		{
			return trackCount;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override c_int GetPlayingPosition()
		{
			c_int position = playerInfo.Admin.Looped ? playerInfo.Sequencer.Step.Last : playerInfo.Sequencer.Step.Current;

			return position - playerInfo.Sequencer.Step.First - 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override ubyte[] GetPlayingTracks()
		{
			return playerInfo.Track.Take(4).Select(x => x.Pt).ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override c_int GetSpeed()
		{
			return playerInfo.Admin.Speed;
		}



		/********************************************************************/
		/// <summary>
		/// Return a list of samples used
		/// </summary>
		/********************************************************************/
		public override IEnumerable<Sample> GetSamples()
		{
			for (c_int i = 0; i < sampleCount; i++)
			{
				udword sh = (udword)(offsets.SampleHeaders + (i * 0x60));

				udword start = MyEndian.ReadBEUdword(pBuf, sh);
				udword loopStart = MyEndian.ReadBEUdword(pBuf, sh + 8);

				Sample sample = new Sample
				{
					Start = pBuf.TellBegin() + offsets.SampleData + start,
					Length = MyEndian.ReadBEUdword(pBuf, sh + 4) * 2,
					LoopStartOffset = loopStart - start,
					LoopLength = MyEndian.ReadBEUdword(pBuf, sh + 12) * 2
				};

				yield return sample;
			}
		}

		#region TNE: Added extra methods for snapshot support
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override ISnapshot CreateSnapshot()
		{
			return new Snapshot(playerInfo, voiceVars);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override void SetSnapshot(ISnapshot snapshot)
		{
			// Start to make a clone of the snapshot
			Snapshot currentSnapshot = (Snapshot)snapshot;
			Snapshot clonedSnapshot = new Snapshot(currentSnapshot.PlayerInfo, currentSnapshot.Voices);

			playerInfo = clonedSnapshot.PlayerInfo;
			voiceVars = clonedSnapshot.Voices;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SoftRestart()
		{
			songEnd = false;
			songPosCurrent = 0;
			tickFpAdd = 0;
			playerInfo.Admin.Looped = false;

			uword so = (uword)(playerInfo.Admin.StartSong << 1);
			playerInfo.Sequencer.Step.First = playerInfo.Sequencer.Step.Current = MyEndian.ReadBEUword(pBuf, offsets.SongDefs + so);
			playerInfo.Sequencer.Step.Loop = MyEndian.ReadBEUword(pBuf, offsets.SongDefs + 0x20 + so);
			playerInfo.Sequencer.Step.Last = MyEndian.ReadBEUword(pBuf, offsets.SongDefs + 0x40 + so);
			playerInfo.Sequencer.Step.CurrentOffset = (udword)(playerInfo.Sequencer.Step.Current * playerInfo.Sequencer.Step.Size);

			// Not set by any of the modules/songs
			uword trackMute = MyEndian.ReadBEUword(pBuf, offsets.SongDefs + 0x80 + so);

			for (ubyte t = 0; t < playerInfo.Sequencer.Tracks; t++)
				playerInfo.Track[t].Off = ((trackMute & (1 << t)) != 0);

			// The others end a pattern with a special value
			if (variant.Starball)
				playerInfo.Sequencer.Pattern.Length = (uword)(2 * MyEndian.ReadBEUword(pBuf, offsets.SongDefs + 0xc0 + so));
			else if (variant.Ptc)
				playerInfo.Sequencer.Pattern.Length = MyEndian.ReadBEUword(pBuf, offsets.SongDefs + 0xe0 + so);
			else
				playerInfo.Sequencer.Pattern.Length = 0;	// Means we need to check for pattern end flag

			playerInfo.Admin.Speed = (sword)MyEndian.ReadBEUword(pBuf, offsets.SongDefs + 0x60 + so);
			playerInfo.Admin.Count = 1;		// Quick start
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ProcessSequencer()
		{
			// Since the sequencer advances to next step before tracks and
			// patterns are processed, handling song end must be deferred to
			// next run of sequencer
			if (playerInfo.Admin.Looped)	// End seen before?
			{
				songEnd = true;

				if (songEnd && loopMode)
				{
					songEnd = false;
					playerInfo.Admin.Looped = false;

					realSongEnd = true;
				}
			}

			udword o = offsets.TrackTable + playerInfo.Sequencer.Step.CurrentOffset;

			for (ubyte t = 0; t < playerInfo.Sequencer.Tracks; t++)
			{
				Track tr = playerInfo.Track[t];

				if (playerInfo.Sequencer.Tables == 1)	// Interleaved parameters
				{
					tr.Pt = pBuf[o];
					tr.Tr = (sbyte)pBuf[o + 1];
					tr.St = pBuf[o + 2];

					o += 4;
				}
				else		// Each parameter in its own table
				{
					uword gap = (uword)(playerInfo.Sequencer.Tracks * playerInfo.Sequencer.TableSize);

					tr.Pt = pBuf[o];
					tr.Tr = (sbyte)(pBuf[o + gap]);
					tr.St = pBuf[o + (2U * gap)];

					o += playerInfo.Sequencer.TableSize;
				}
			}

			// Advance to next step
			if (++playerInfo.Sequencer.Step.Current < playerInfo.Sequencer.Step.Last)
				playerInfo.Sequencer.Step.CurrentOffset += playerInfo.Sequencer.Step.Size;
			else	// Sometimes the loop is to a silent step, though
			{
				playerInfo.Sequencer.Step.Current = playerInfo.Sequencer.Step.Loop;
				playerInfo.Sequencer.Step.CurrentOffset = (udword)(playerInfo.Sequencer.Step.Current * playerInfo.Sequencer.Step.Size);

				// Trigger song end next time we run the sequencer
				playerInfo.Admin.Looped = true;
			}

			playerInfo.Sequencer.Step.Next = false;
			playerInfo.Sequencer.Pattern.Pos = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ProcessPattern(Track tr)
		{
			// Check track mute and pattern number
			if (tr.Off || (tr.Pt == 0))
				return;

			udword pattOffs = GetPattOffset((ubyte)(tr.Pt - 1));

			// Check pattern end flag for the variant that doesn't define
			// pattern length
			if ((playerInfo.Sequencer.Pattern.Length == 0) && (MyEndian.ReadBEUword(pBuf, pattOffs + playerInfo.Sequencer.Pattern.Pos + 2) == 0xffff))
				playerInfo.Sequencer.Step.Next = true;

			// First word from pattern
			uword pattVal = MyEndian.ReadBEUword(pBuf, pattOffs + playerInfo.Sequencer.Pattern.Pos);

			if (pattVal == 0)
				return;

			if (pattVal == 0x8000)	// Key up event
			{
				tr.KeyDown = false;

				TriggerVoiceKeyUp(tr);
				return;
			}

			// TODO
			// Pattern value is a note command
			if (tr.KeyDown)
			{
				if (!variant.Ptc)			// Only PTC doesn't do this here
					TriggerVoiceKeyUp(tr);	// for previously used voice
			}

			tr.KeyDown = true;

			// NB! An inconsistency in the original players. Sample number
			// is in lowest four bits, and even if only three bits are
			// used by the published modules because of sound transpose
			// parameter, after this >>2 shift, the lowest bit may be set
			// accidentally. Not a big deal, though
			if (variant.Starball || variant.Ptc)
				tr.Pattern.AddVolume = (ubyte)((pattVal & 0xff) >> 2);
			else if (variant.HppIngame)
				tr.Pattern.AddVolume = (ubyte)((pattVal & 0xf0) >> 2);
			else
				tr.Pattern.AddVolume = (ubyte)((pattVal >> 4) & 0x0f);

			tr.Pattern.Sample = (ubyte)((pattVal & 0x0f) + tr.St);
			tr.Pattern.Note = (ubyte)((((pattVal >> 8) & 0xff) + tr.Tr) + 1);

			TriggerNote(tr);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TriggerVoiceKeyUp(Track tr)
		{
			ubyte voiceNum = tr.AssignedVoiceNum;

			if (voiceNum == 0xff)
				return;

			tr.AssignedVoiceNum = 0xff;		// Track released this voice

			VoiceVars v = voiceVars[voiceNum];

			if (variant.StarballIngame)
			{
				if (v.Envelope.Duration >= 0x7fff)
					return;
			}

			v.Envelope.Duration |= 0x8000;	// Make it much "older"

			// These two together ensure Release phase is started
			v.Envelope.KeyUp = true;
			v.Envelope.Phase = EnvPhase.Decay;

			NextEnvelopePhase(v);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TriggerNote(Track tr)
		{
			ubyte voiceNum;

			if (voices == 4)
				voiceNum = ChooseVoice();
			else	// Just for fun, if increasing voices to 6
				voiceNum = tr.Num;

			tr.AssignedVoiceNum = voiceNum;

			VoiceVars v = voiceVars[voiceNum];
			PaulaVoice paulaVoice = paulaVoices[voiceNum];

			v.SampleHeader = (udword)(offsets.SampleHeaders + (tr.Pattern.Sample * 0x60));

			if (tr.Pattern.Note < 0x30)
				v.Period = periods[tr.Pattern.Note];
			else	// None of the modules/songs cause a note value overflow
				v.Period = 0;

			paulaVoice.Off();

			udword sh = v.SampleHeader;
			ToPaulaStart(v, paulaVoice, MyEndian.ReadBEUdword(pBuf, sh) + offsets.SampleData);
			ToPaulaLength(v, paulaVoice, MyEndian.ReadBEUword(pBuf, sh + 6));

			if (variant.Starball || variant.Ptc)
				v.PipelineState = 2;
			else
				v.PipelineState = 3;

			v.Envelope.Phase = EnvPhase.Attack;
			v.Envelope.KeyUp = false;
			v.Envelope.Duration = 0;

			// Copy envelope parameters
			v.Envelope.Strength = (sword)MyEndian.ReadBEUword(pBuf, sh + 0x1a);
			v.Envelope.Speed = MyEndian.ReadBEUword(pBuf, sh + 0x18);
			v.Envelope.Count = v.Envelope.Speed;
			v.Envelope.Volume = (sword)CapVolume((uword)(MyEndian.ReadBEUword(pBuf, sh + 0x1c) + tr.Pattern.AddVolume));
			v.Envelope.TargetVolume = CapVolume((uword)(MyEndian.ReadBEUword(pBuf, sh + 0x1e) + tr.Pattern.AddVolume));
			v.Envelope.DecaySpeed = MyEndian.ReadBEUword(pBuf, sh + 0x20);
			v.Envelope.DecayStrength = (sword)MyEndian.ReadBEUword(pBuf, sh + 0x22);
			v.Envelope.SetSustain = (MyEndian.ReadBEUword(pBuf, sh + 0x24) == 1);
			v.Envelope.SustainVolume = MyEndian.ReadBEUword(pBuf, sh + 0x26);
			v.Envelope.ReleaseSpeed = MyEndian.ReadBEUword(pBuf, sh + 0x28);
			v.Envelope.ReleaseStrength = (sword)MyEndian.ReadBEUword(pBuf, sh + 0x2a);

			if (variant.Starball || variant.Ptc)
				paulaVoice.Paula.Volume = (uword)v.Envelope.Volume;
			else
				paulaVoice.Paula.Volume = 0x32;		// Overwritten in UpdateVoices() though

			sword detune = (sword)MyEndian.ReadBEUword(pBuf, sh + 0x4a);
			sword period = (sword)(v.Period + detune);

			// Because of the detune value we need to check the resulting
			// period as to avoid going below Paula's "lowest" period
			if (period < 0x71)
				period = 0x71;

			paulaVoice.Paula.Period = (uword)period;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private udword GetPattOffset(ubyte pt)
		{
			if (variant.Ptc)
				return (udword)(offsets.Header + offsets.Patterns + (pt * 0x60) - offsets.Base);
			else
				return offsets.Header + (MyEndian.ReadBEUdword(pBuf, (udword)(offsets.Patterns + (pt << 2))) - offsets.Base);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ubyte ChooseVoice()
		{
			ubyte choseVoiceNum = playerInfo.Admin.NextVoiceNum;
			ubyte tryVoiceNum = choseVoiceNum;
			uword cmpValue = 0;

			for (c_int i = 0; i < voices; ++i)
			{
				VoiceVars v = voiceVars[tryVoiceNum];

				if (cmpValue < v.Envelope.Duration)
				{
					cmpValue = v.Envelope.Duration;
					choseVoiceNum = tryVoiceNum;
				}

				if (tryVoiceNum == 0)
					tryVoiceNum = (ubyte)voices;

				--tryVoiceNum;
			}

			playerInfo.Admin.NextVoiceNum += 2;

			if (playerInfo.Admin.NextVoiceNum >= voices)
				playerInfo.Admin.NextVoiceNum = (ubyte)(playerInfo.Admin.NextVoiceNum - voices);

			// Remove the voice from the track it is assigned to
			for (c_int i = 0; i < playerInfo.Sequencer.Tracks; ++i)
			{
				if (playerInfo.Track[i].AssignedVoiceNum == choseVoiceNum)
					playerInfo.Track[i].AssignedVoiceNum = 0xff;
			}

			return choseVoiceNum;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void UpdateVoices()
		{
			for (c_int i = 0; i < voices; ++i)
			{
				VoiceVars v = voiceVars[i];
				PaulaVoice paulaVoice = paulaVoices[i];

				paulaVoice.Paula.Volume = (uword)v.Envelope.Volume;
				ProcessEnvelope(v);
				ProcessPaulaPipeline(v, paulaVoice);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ProcessPaulaPipeline(VoiceVars v, PaulaVoice paulaVoice)
		{
			if (variant.Starball || variant.Ptc)
				++v.Envelope.Duration;

			switch (v.PipelineState)
			{
				case 3:
				{
					--v.PipelineState;
					break;
				}

				case 2:
				{
					if (variant.Starball || variant.Ptc)
						v.Envelope.Duration = 0;

					paulaVoice.On();

					--v.PipelineState;
					break;
				}

				case 1:
				{
					udword sh = v.SampleHeader;
					ToPaulaStart(v, paulaVoice, MyEndian.ReadBEUdword(pBuf, sh + 0x08) + offsets.SampleData);
					ToPaulaLength(v, paulaVoice, MyEndian.ReadBEUword(pBuf, sh + 0x0e));

					--v.PipelineState;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// ADSR volume envelope including key up/down events
		/// </summary>
		/********************************************************************/
		private void ProcessEnvelope(VoiceVars v)
		{
			if (v.Envelope.Volume == 0)
			{
				if (variant.Starball || variant.Ptc)
					v.Envelope.Duration = 0xfff0;
				else
					v.Envelope.Duration = 0xffff;

				return;
			}

			if (!variant.Starball && !variant.Ptc)
				++v.Envelope.Duration;

			if (v.Envelope.Phase == EnvPhase.End)
				return;
			else if (v.Envelope.Phase != EnvPhase.Release)
				v.Envelope.Duration &= 0x7fff;

			if (--v.Envelope.Count != 0)
				return;

			v.Envelope.Count = v.Envelope.Speed;

			v.Envelope.Volume += v.Envelope.Strength;

			if (v.Envelope.Strength < 0)	// Down?
			{
				if (v.Envelope.Volume > v.Envelope.TargetVolume)
					return;
			}
			else	// Up
			{
				if (v.Envelope.Volume < v.Envelope.TargetVolume)
					return;
			}

			v.Envelope.Volume = (sword)v.Envelope.TargetVolume;

			NextEnvelopePhase(v);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void NextEnvelopePhase(VoiceVars v)
		{
			switch (v.Envelope.Phase)
			{
				case EnvPhase.Attack:
				{
					// Start decay
					v.Envelope.TargetVolume = v.Envelope.SustainVolume;
					v.Envelope.Strength = v.Envelope.DecayStrength;
					v.Envelope.Speed = v.Envelope.DecaySpeed;
					v.Envelope.Count = v.Envelope.Speed;
					v.Envelope.Phase = EnvPhase.Decay;
					break;
				}

				case EnvPhase.Decay:
				{
					// Only on key up start release
					if (!v.Envelope.KeyUp)
						return;

					if (v.Envelope.SetSustain)
						v.Envelope.Volume = (sword)v.Envelope.SustainVolume;

					v.Envelope.TargetVolume = 0;
					v.Envelope.Strength = v.Envelope.ReleaseStrength;
					v.Envelope.Speed = v.Envelope.ReleaseSpeed;
					v.Envelope.Count = v.Envelope.Speed;
					v.Envelope.Phase = EnvPhase.Release;
					break;
				}

				case EnvPhase.Release:
				{
					v.Envelope.Phase = EnvPhase.End;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uword CapVolume(uword volume)
		{
			uword v = (uword)((volume > 64) ? 64 : volume);

			return v;
		}


		/********************************************************************/
		/// <summary>
		/// Cache the original pair of Paula start+length parameters, so it
		/// can be checked and adjusted in case of out of bounds access to
		/// sample data area
		/// </summary>
		/********************************************************************/
		private void ToPaulaStart(VoiceVars v, PaulaVoice paulaVoice, udword offset)
		{
			v.PaulaOrig.Offset = offset;
			paulaVoice.Paula.Start = MakeSamplePtr(offset);

			TakeNextBufChecked(v, paulaVoice);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ToPaulaLength(VoiceVars v, PaulaVoice paulaVoice, uword length)
		{
			v.PaulaOrig.Length = length;
			paulaVoice.Paula.Length = length;

			TakeNextBufChecked(v, paulaVoice);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TakeNextBufChecked(VoiceVars v, PaulaVoice paulaVoice)
		{
			if (v.PaulaOrig.Offset >= input.Len)	// Start outside sample data space
			{
				paulaVoice.Paula.Start = MakeSamplePtr(offsets.Silence);
				paulaVoice.Paula.Length = 1;
			}
			// End outside sample data space
			else if ((v.PaulaOrig.Offset + (v.PaulaOrig.Length << 1)) > input.Len)
			{
				paulaVoice.Paula.Length = (uword)((input.Len - v.PaulaOrig.Offset) >> 1);
				paulaVoice.Paula.Start = MakeSamplePtr(v.PaulaOrig.Offset);
			}
			// Not needed by players that always set start before length
			else
			{
				paulaVoice.Paula.Length = v.PaulaOrig.Length;
				paulaVoice.Paula.Start = MakeSamplePtr(v.PaulaOrig.Offset);
			}

			paulaVoice.TakeNextBuf();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private CPointer<ubyte> MakeSamplePtr(udword offset)
		{
			return pBuf.TellBegin() + offset;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool LoadSamplesFile(IPointer sample, udword sampleLength)
		{
			// Got both the DNS and SMP file?
			if (input.SmplLoaded)	// If loaded before, reuse it
			{
				input.MdatSize = input.MdatSizeCurrent;
				input.SmplSize = input.SmplSizeCurrent;

				return true;
			}

			// TNE: This has been rewritten to use the sample buffer from input
			// instead of loading files
			if (sample.IsNull || (sampleLength == 0))
				return false;

			CPointer<ubyte> newInputBuf = new CPointer<ubyte>(sampleLength + input.BufLen);
			CMemory.memcpy(newInputBuf, input.Buf, input.BufLen);
			input.Buf = newInputBuf;

			CMemory.memcpy(newInputBuf + input.Len, sample, sampleLength);

			input.SmplSize = sampleLength;
			input.MdatSize = input.Len - offsets.Header;
			offsets.SampleData = input.Len;
			input.BufLen += input.SmplSize;
			input.Len += input.SmplSize;

			// Update smart pointers
			pBuf.SetBuffer(input.Buf, input.BufLen);

			return true;
		}
		#endregion
	}
}
