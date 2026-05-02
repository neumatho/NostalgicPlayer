/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris.Containers;
using Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris
{
	/// <summary>
	/// 
	/// </summary>
	public partial class TfmxDecoder : Decoder
	{
		private const string Tag = "TFMX";
		private const string Tag_TfmxSong = "TFMX-SONG";
		private const string Tag_TfmxSong_Lc = "tfmxsong";
		private const udword Tfmx_Hex = 0x54464d58;

		private const c_int Tracks_Max = 8;
		private const c_int Track_Steps_Max = 512;

		private const c_int Voices_Max = 8;

		private const c_int Recurse_Limit = 16;		// Way more than needed

		private const c_int Track_Cmd_Max = 4;

		private readonly PaulaVoice[] dummyVoices = ArrayHelper.InitializeArray<PaulaVoice>(Voices_Max);

		private readonly SmartPtr<ubyte> pBuf = new SmartPtr<ubyte>();	// For safe unsigned access

		private readonly List<ubyte> vSongs = new List<ubyte>();
		private udword songPosCurrent;
		private c_int voices;
		private bool triggerRestart;

		private readonly ubyte[] channelToVoiceMap = new ubyte[Voices_Max];

		private readonly ModuleOffsets offsets = new ModuleOffsets();
		private VoiceVars[] voiceVars = ArrayHelper.InitializeArray<VoiceVars>(Voices_Max);
		private readonly PaulaVoice[] paulaVoices = new PaulaVoice[Voices_Max];		// Paula and mixer interface (TNE: Moved this out from VoiceVars into its own array. If not, snapshot does not work)

		private delegate void PattCmdFuncPtr(Track track);
		private readonly PattCmdFuncPtr[] pattCmdFuncs = new PattCmdFuncPtr[(0xff - 0xf0) + 1];

		private delegate void TrackCmdFuncPtr(udword stepOffset);
		private readonly TrackCmdFuncPtr[] trackCmdFuncs = new TrackCmdFuncPtr[Track_Cmd_Max + 1];

		private delegate void MacroCmdFuncPtr(VoiceVars voice);
		private readonly MacroCmdFuncPtr[] macroCmdFuncs = new MacroCmdFuncPtr[0x40];

		private readonly bool[] trackCmdUsed = new bool[Track_Cmd_Max + 1];
		private readonly bool[] patternCmdUsed = new bool[16];
		private readonly bool[] macroCmdUsed = new bool[0x40];
		private HashSet<ubyte> realMacrosUsed;

		private PlayerInfo playerInfo;

		private
		(
			// Format
			bool Compressed,
			// Player variants
			bool FinetuneUnscaled,
			bool VibratoUnscaled,
			bool PortaUnscaled,
			bool PortaOverride,
			bool NoNoteDetune,
			bool BpmSpeed5,
			bool NoAddBeginCount,
			bool NoTrackMute
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
			bool SmplLoaded,

			c_int VersionHint,			// From TFHD flag
			c_int StartSongHint			// From TFMX-MOD struct
		) input;

		private static readonly uword[] periods =
		[
			0x0d5c,

			// -0xf4 extra octave
			0x0c9c, 0x0be8, 0x0b3c, 0x0a9a, 0x0a02, 0x0a02, 0x0972,
			0x08ea, 0x086a, 0x07f2, 0x0780, 0x0718,

			// +0 standard octaves
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
			0x00d6, 0x00ca, 0x00bf, 0x00b4, 0x00aa, 0x00a0, 0x0097, 0x008f,
			0x0087, 0x007f, 0x0078, 0x0071,

			// +0x3c
			0x00d6, 0x00ca, 0x00bf, 0x00b4
		];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public TfmxDecoder()
		{
			input.Buf.SetToNull();
			input.BufLen = input.Len = 0;
			input.SmplLoaded = false;

			// Set up some dummy voices to decouple the decoder from the mixer
			for (ubyte v = 0; v < Voices_Max; v++)
				paulaVoices[v] = dummyVoices[v];

			loopMode = false;
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
		public override c_int GetTracks()
		{
			return (c_int)((offsets.Macros - offsets.Patterns) >> 2);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override c_int GetSpeed()
		{
			return playerInfo.Admin.Speed + 1;
		}



		/********************************************************************/
		/// <summary>
		/// Return a list of samples used
		/// </summary>
		/********************************************************************/
		public override IEnumerable<Sample> GetSamples()
		{
			List<SampleRange> samples = new List<SampleRange>();

			foreach (ubyte index in realMacrosUsed)
			{
				udword macroOffset = GetMacroOffset(index);
				bool endOfMacro = false;

				SampleRange range = new SampleRange();

				for (udword step = 0; !endOfMacro; step += 4)
				{
					udword offset = macroOffset + step;
					ubyte command = pBuf[offset];

					switch (command)
					{
						// Stop
						case 0x07:
						case 0xff:
						{
							endOfMacro = true;
							break;
						}

						// SetBegin
						case 0x02:
						{
							range.Start = MyEndian.MakeDword(0, pBuf[offset + 1], pBuf[offset + 2], pBuf[offset + 3]);
							break;
						}

						// SetLen
						case 0x03:
						{
							range.End = range.Start + (MyEndian.MakeWord(pBuf[offset + 2], pBuf[offset + 3]) * 2U);

							samples.Add(range);
							range = new SampleRange();
							break;
						}

						// SampleLoop
						case 0x18:
						{
							SampleRange prevRange = samples[^1];

							prevRange.LoopStart = prevRange.Start + MyEndian.MakeDword(0, pBuf[offset + 1], pBuf[offset + 2], pBuf[offset + 3]);
							prevRange.LoopLength = prevRange.End - prevRange.LoopStart;
							break;
						}
					}
				}
			}

			if (samples.Count > 0)
			{
				// Sort by Start, and for same Start, keep only the one with highest End
				samples.Sort((a, b) =>
				{
					int startCompare = a.Start.CompareTo(b.Start);
					if (startCompare != 0)
						return startCompare;

					// Same start: sort descending by End so highest End comes first
					return b.End.CompareTo(a.End);
				});

				// Merge overlapping samples
				List<SampleRange> uniqueSamples = new List<SampleRange>();

				foreach (SampleRange r in samples)
				{
					if ((uniqueSamples.Count == 0) || (r.Start >= uniqueSamples[^1].End))
						uniqueSamples.Add(r);
					else if (r.End > uniqueSamples[^1].End)
						uniqueSamples[^1].End = r.End;
				}

				// Fill gaps between samples
				List<SampleRange> finalSamples = new List<SampleRange>();

				// Add filler from 4 to first sample if needed (the first 4 bytes are used as silence sample)
				if (uniqueSamples[0].Start > 4)
				{
					finalSamples.Add(new SampleRange
					{
						Start = 4,
						End = uniqueSamples[0].Start
					});
				}

				for (int i = 0; i < uniqueSamples.Count; i++)
				{
					finalSamples.Add(uniqueSamples[i]);

					// Check if there's a gap to the next sample
					if (i < uniqueSamples.Count - 1)
					{
						udword currentEnd = uniqueSamples[i].End;
						udword nextStart = uniqueSamples[i + 1].Start;

						if (currentEnd < nextStart)
						{
							// Add a filler sample to cover the gap
							finalSamples.Add(new SampleRange
							{
								Start = currentEnd,
								End = nextStart
							});
						}
					}
				}

				// Add filler from last sample end to SmplSize if needed
				udword lastEnd = uniqueSamples[^1].End;

				if (lastEnd < input.SmplSize)
				{
					finalSamples.Add(new SampleRange
					{
						Start = lastEnd,
						End = input.SmplSize
					});
				}

				for (int i = 0; i < finalSamples.Count; i++)
				{
					Sample sample = GetSampleInfo(finalSamples[i], i < finalSamples.Count - 1 ? finalSamples[i + 1] : null);
					if (sample != null)
						yield return sample;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return an initialized sample info object
		/// </summary>
		/********************************************************************/
		private Sample GetSampleInfo(SampleRange current, SampleRange next)
		{
			if ((offsets.SampleData + current.Start) >= pBuf.TellLength())
				return null;

			udword sampleLength = (next == null ? current.End : next.Start) - current.Start;
			sampleLength = Math.Min(sampleLength, (udword)pBuf.TellLength() - current.Start);

			Sample sample = new Sample
			{
				Start = pBuf.TellBegin().Slice((c_int)(offsets.SampleData + current.Start)),
				Length = sampleLength
			};

			if (current.LoopLength != 0)
			{
				sample.LoopStartOffset = current.LoopStart - current.Start;
				sample.LoopLength = current.LoopLength;

				if (sample.LoopStartOffset >= sample.Length)
				{
					sample.LoopStartOffset = 0;
					sample.LoopLength = 0;
				}
				else if ((sample.LoopStartOffset + sample.LoopLength) > sample.Length)
				{
					sample.LoopLength = sample.Length - sample.LoopStartOffset;
				}
			}

			return sample;
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
		public override bool Init(IPointer data, udword length, IPointer sample, udword sampleLength, out string errorMessage)
		{
			errorMessage = string.Empty;

			playerInfo = new PlayerInfo
			{
				Admin = new Admin(),
				Track = ArrayHelper.InitializeArray<Track>(Tracks_Max),
				Cmd = new Cmd()
			};

			realMacrosUsed = new HashSet<ubyte>();

			title = string.Empty;
			author = string.Empty;
			game = string.Empty;
			comment = string.Empty;

			if (data.IsNull || (length == 0))	// Re-init mode
			{
				if (!playerInfo.Admin.Initialized)
				{
					errorMessage = Resources.IDS_ERR_NOT_INITIALIZED;
					return false;
				}

				data = input.Buf;
				length = input.Len;
			}
			else
			{
				// Invalidate what has been found out before
				input.SmplLoaded = false;
				input.MdatSize = input.SmplSize = 0;

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

				if (!Detect(input.Buf, input.BufLen))
				{
					errorMessage = Resources.IDS_ERR_UNKNOWN_FORMAT;
					return false;
				}
			}

			// Check whether it's a single-file format.
			// If it is, this method defines various variables
			if (IsMerged() || LoadSamplesFile(sample, sampleLength))
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

			// --------

			// For convenience, since we don't relocate the module to offset 0
			udword h = offsets.Header;

			// TNE: Added reading the comment block
			Encoding encoder = EncoderCollection.Amiga;

			for (c_int line = 0; line < 6; line++)
			{
				udword lineOffset = (udword)(h + 16 + (line * 40));
				string commentLine = encoder.GetString(pBuf.TellBegin().AsSpan((int)lineOffset, 40)).Replace("\r", string.Empty).TrimEnd();

				if (!string.IsNullOrEmpty(commentLine))
					comment += commentLine + "\n";
			}

			// Remove last \n
			if (!string.IsNullOrEmpty(comment))
				comment = comment.Substring(0, comment.Length - 1);

			udword o1 = MyEndian.ReadBEUdword(pBuf, h + 0x1d0);	// Offset to trackTable
			udword o2 = MyEndian.ReadBEUdword(pBuf, h + 0x1d4);	// Offset to pattern offsets
			udword o3 = MyEndian.ReadBEUdword(pBuf, h + 0x1d8);	// Offset to macro offsets

			if ((o1 | o2 | o3) != 0)
				variant.Compressed = true;
			else
			{
				o1 = 0x800;
				o2 = 0x400;
				o3 = 0x600;
			}

			// Check for out-of-bounds offsets. It may be the rare case of a TFMX PC
			// module conversion like "Tony & Friends in Kellogg's Land", which uses
			// little-endian offsets here
			if ((o1 >= input.BufLen) || (o2 >= input.BufLen) || (o3 >= input.BufLen))
			{
				o1 = MyEndian.ByteSwap(o1);
				o2 = MyEndian.ByteSwap(o2);
				o3 = MyEndian.ByteSwap(o3);
			}

			offsets.TrackTable = h + o1;
			offsets.Patterns = h + o2;		// Offset to array of offsets
			offsets.Macros = h + o3;		// Offset to array of offsets

			// If they are out-of-bounds, reject the module
			if ((offsets.TrackTable >= input.BufLen) || (offsets.Patterns >= input.BufLen) || (offsets.Macros >= input.BufLen))
			{
				errorMessage = Resources.IDS_ERR_CORRUPT;
				return false;
			}

			offsets.SampleData = h + input.MdatSize;

			// TFMX clears the first two words for one-shot samples
			udword o = offsets.SampleData;
			pBuf[o] = pBuf[o + 1] = pBuf[o + 2] = pBuf[o + 3] = 0;

			// Reject Atari ST TFMX files
			bool foundHighMacroCmd = false;

			for (c_int m = 0; m < 7; m++)
			{
				udword macroOffs = GetMacroOffset((ubyte)m);
				udword macroEnd = GetMacroOffset((ubyte)(m + 1));

				if (macroEnd <= macroOffs)
					break;

				bool foundStop = false;

				while (macroOffs < macroEnd)
				{
					ubyte cmd = pBuf[macroOffs];

					if (cmd == 7)
					{
						foundStop = true;
						break;
					}

					if (cmd >= 0x40)
						foundHighMacroCmd = true;

					macroOffs += 4;
				}

				if (foundStop && foundHighMacroCmd)
				{
					errorMessage = Resources.IDS_ERR_ATARI;
					return false;
				}
			}

			offsets.Silence = offsets.SampleData;

			// TFMX clears the first dword here for one shot samples e.g.
			pBuf[offsets.Silence] = pBuf[offsets.Silence + 1] = pBuf[offsets.Silence + 2] = pBuf[offsets.Silence + 3] = 0;

			// Evaluate the compress identification fields at $0A and $0C.
			// In rare cases that part of the header has been overwritten
			// and is invalid
			udword compressHint = MyEndian.ReadBEUdword(pBuf, h + 0x0c);

			if ((compressHint != 0) && (MyEndian.ReadBEUdword(pBuf, h + 0x0a) == 1))
			{
				offsets.TrackTableEnd = offsets.TrackTable + compressHint - (0x800 - 0x10);

				if (offsets.TrackTableEnd > GetPattOffset(0))
					offsets.TrackTableEnd = GetPattOffset(0);
			}
			else
				offsets.TrackTableEnd = GetPattOffset(0);

			// ----------

			// Defaults only. Detection further below
			SetRate(50 << 8);
			voices = 4;

			playerInfo.Sequencer.StepSeenBefore = new bool[Track_Steps_Max];
			playerInfo.Sequencer.Tracks = 8;
			playerInfo.Sequencer.Step.Size = 16;

			for (size_t v = 0; v < (size_t)channelToVoiceMap.Length; v++)
				channelToVoiceMap[v] = (ubyte)(v & 3);

			variant.Compressed = false;
			variant.FinetuneUnscaled = false;
			variant.VibratoUnscaled = false;
			variant.PortaUnscaled = false;
			variant.PortaOverride = false;
			variant.NoNoteDetune = false;
			variant.BpmSpeed5 = false;
			variant.NoAddBeginCount = false;
			variant.NoTrackMute = false;

			pattCmdFuncs[0] = PattCmd_End;
			pattCmdFuncs[1] = PattCmd_Loop;
			pattCmdFuncs[2] = PattCmd_Goto;
			pattCmdFuncs[3] = PattCmd_Wait;
			pattCmdFuncs[4] = PattCmd_Stop;
			pattCmdFuncs[5] = PattCmd_Note;
			pattCmdFuncs[6] = PattCmd_Note;
			pattCmdFuncs[7] = PattCmd_Note;
			pattCmdFuncs[8] = PattCmd_SaveAndGoto;
			pattCmdFuncs[9] = PattCmd_ReturnFromGoto;
			pattCmdFuncs[10] = PattCmd_Fade;
			pattCmdFuncs[11] = PattCmd_Nop;
			pattCmdFuncs[12] = PattCmd_Note;
			pattCmdFuncs[13] = PattCmd_Nop;
			pattCmdFuncs[14] = PattCmd_Stop;
			pattCmdFuncs[15] = PattCmd_Nop;

			trackCmdFuncs[0] = TrackCmd_Stop;
			trackCmdFuncs[1] = TrackCmd_Loop;
			trackCmdFuncs[2] = TrackCmd_Speed;

			// TIMESHARE command not needed.
			// We activate the 7VOICE command by default for more accurate
			// song duration detection
			trackCmdFuncs[3] = TrackCmd_7V;
			trackCmdFuncs[4] = TrackCmd_Fade;

			// Start with mapping all undefined/unknown macro commands to NOP
			for (ubyte m = 0; m < 0x40; m++)
				macroCmdFuncs[m] = MacroFunc_Nop;

			macroCmdFuncs[0] = MacroFunc_StopSound;
			macroCmdFuncs[1] = MacroFunc_StartSample;
			macroCmdFuncs[2] = MacroFunc_SetBegin;
			macroCmdFuncs[3] = MacroFunc_SetLen;
			macroCmdFuncs[4] = MacroFunc_Wait;
			macroCmdFuncs[5] = MacroFunc_Loop;
			macroCmdFuncs[6] = MacroFunc_Cont;
			macroCmdFuncs[7] = MacroFunc_Stop;

			macroCmdFuncs[8] = MacroFunc_AddNote;
			macroCmdFuncs[9] = MacroFunc_SetNote;
			macroCmdFuncs[0xa] = MacroFunc_Reset;
			macroCmdFuncs[0xb] = MacroFunc_Portamento;
			macroCmdFuncs[0xc] = MacroFunc_Vibrato;
			macroCmdFuncs[0xd] = MacroFunc_AddVolNote;
			macroCmdFuncs[0xe] = MacroFunc_SetVolume;
			macroCmdFuncs[0xf] = MacroFunc_Envelope;

			macroCmdFuncs[0x10] = MacroFunc_LoopKeyUp;
			macroCmdFuncs[0x11] = MacroFunc_AddBegin;
			macroCmdFuncs[0x12] = MacroFunc_AddLen;
			macroCmdFuncs[0x13] = MacroFunc_StopSample;
			macroCmdFuncs[0x14] = MacroFunc_WaitKeyUp;
			macroCmdFuncs[0x15] = MacroFunc_Goto;
			macroCmdFuncs[0x16] = MacroFunc_Return;
			macroCmdFuncs[0x17] = MacroFunc_SetPeriod;

			macroCmdFuncs[0x18] = MacroFunc_SampleLoop;
			macroCmdFuncs[0x19] = MacroFunc_OneShot;
			macroCmdFuncs[0x1a] = MacroFunc_WaitOnDma;
			macroCmdFuncs[0x1b] = MacroFunc_RandomPlay;
			macroCmdFuncs[0x1c] = MacroFunc_SplitKey;
			macroCmdFuncs[0x1d] = MacroFunc_SplitVolume;
			macroCmdFuncs[0x1e] = MacroFunc_RandomMask;
			macroCmdFuncs[0x1f] = MacroFunc_SetPrevNote;

			// Macro commands $1F AddChannel and $20 SubChannel are not implemented
			// since no file seems to use them. Also, number $1F is occupied by
			// SetPrevNote already. which would cause a conflict that would need
			// to be detected and prevent somehow.
			// 
			// Macro command $20 Signal and its external write-only registers
			// as added by some TFMX variants is not needed either
			macroCmdFuncs[0x20] = MacroFunc_Nop;

			macroCmdFuncs[0x21] = MacroFunc_PlayMacro;
			macroCmdFuncs[0x22] = MacroFunc_22;
			macroCmdFuncs[0x23] = MacroFunc_23;
			macroCmdFuncs[0x24] = MacroFunc_24;
			macroCmdFuncs[0x25] = MacroFunc_25;
			macroCmdFuncs[0x26] = MacroFunc_26;
			macroCmdFuncs[0x27] = MacroFunc_27;

			macroCmdFuncs[0x28] = MacroFunc_28;
			macroCmdFuncs[0x29] = MacroFunc_29;

			// TFMX v1.x
			if (((MyEndian.ReadBEUdword(pBuf, offsets.Header) == Tfmx_Hex) && (pBuf[offsets.Header + 4] == 0x20)) || (input.VersionHint == 1))
				SetTfmxV1();

			// Last the rare checksum adjustments
			TraitsByChecksum();

			FindSongs();

			// Some files contain SFX only and no valid song definitions
			if (vSongs.Count == 0)
			{
				errorMessage = Resources.IDS_ERR_NO_SUBSONGS;
				return false;
			}

			// If the file specifies a default start song (like TFMX-MOD does),
			// assume it's a single-song file format, and reduce number of songs to 1
			if (input.StartSongHint >= 0)
			{
				vSongs.Clear();
				vSongs.Add((ubyte)input.StartSongHint);
			}

			playerInfo.Admin.Initialized = true;

			// TNE: We need this to be as fast as possible and since I do have my own
			// duration calculation (does a similar thing), we don't need to scan the
			// whole module
			playerInfo.Admin.StartSong = 0;
			Restart();

			bool loopModeBak = loopMode;
			loopMode = false;

			for (int i = 0; i < 50 * 60; i++)	// ~ 1 minute
			{
				Run();

				if (songEnd)
					break;
			}

			loopMode = loopModeBak;

			AdjustTraitsPost();

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
			playerInfo.Admin.StartSong = songNumber < 0 ? 0 : songNumber;

			if (playerInfo.Admin.StartSong > (vSongs.Count - 1))
				playerInfo.Admin.StartSong = 0;

			Restart();
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

			for (ubyte v = 0; v < voices; v++)
			{
				if (!songEnd || loopMode)
				{
					VoiceVars voice = voiceVars[v];
					PaulaVoice paulaVoice = paulaVoices[v];

					// Pretend we have an interrupt handler that has evaluated
					// Paula "audio channel 0-3 block finished" interrupts meanwhile
					if (voice.WaitOnDmaCount >= 0)	// 0 = wait once
					{
						uword x = paulaVoice.GetLoopCount();
						uword y = voice.WaitOnDmaPrevLoops;
						c_int d;

						if (x >= y)
							d = x - y;
						else
							d = x + (0x10000 - y);

						if (d > voice.WaitOnDmaCount)
						{
							voice.Macro.Skip = false;
							voice.WaitOnDmaCount = -1;
						}
						else
						{
							voice.WaitOnDmaCount -= (sword)d;
							voice.WaitOnDmaPrevLoops = paulaVoice.GetLoopCount();
						}
					}

					ProcessMacroMain(voice);
					ProcessModulation(voice);

					paulaVoice.Paula.Period = voice.OutputPeriod;
				}
			}

			if (!songEnd || loopMode)
			{
				if (--playerInfo.Admin.Count < 0)
				{
					playerInfo.Admin.Count = playerInfo.Admin.Speed;	// Reload

					do
					{
						playerInfo.Sequencer.Step.Next = false;
						c_int countInactive = 0;
						c_int countInfinite = 0;

						for (ubyte t = 0; t < playerInfo.Sequencer.Tracks; t++)
						{
							Track tr = playerInfo.Track[t];

							tr.On = GetTrackMute(t);

							if (tr.Pt >= 0x90)
								countInactive++;
							else if (tr.Pattern.InfiniteLoop)
								countInfinite++;

							ProcessPttr(tr);

							if (playerInfo.Sequencer.Step.Next)
								break;
						}	// Next track

						// These are states where track sequencer cannot advance
						if (!playerInfo.Sequencer.Step.Next)
						{
							if ((countInactive == playerInfo.Sequencer.Tracks) || ((countInactive + countInfinite) == playerInfo.Sequencer.Tracks))
							{
								songEnd = true;
								triggerRestart = true;
							}
						}
					}
					while (playerInfo.Sequencer.Step.Next);
				}
			}

			if (songEnd && loopMode)
			{
				songEnd = false;

				if (triggerRestart)
					SoftRestart();

				realSongEnd = true;
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
		public override bool Detect(IPointer data, udword len)
		{
			CPointer<ubyte> d = data.ToPointer<ubyte>();
			bool maybe = false;

			if ((len >= 5) && (CMemory.memcmp(d, Tag, 4) == 0) && (d[4] == 0x20))
				maybe = true;
			else if ((len >= Tag_TfmxSong.Length) && (CMemory.memcmp(d, Tag_TfmxSong, (size_t)Tag_TfmxSong.Length) == 0))
				maybe = true;
			else if ((len >= Tag_TfmxSong_Lc.Length) && (CMemory.memcmp(d, Tag_TfmxSong_Lc, (size_t)Tag_TfmxSong_Lc.Length) == 0))
				maybe = true;
			else if ((len >= Tag_TfmxPak.Length) && (CMemory.memcmp(d, Tag_TfmxPak, (size_t)Tag_TfmxPak.Length) == 0))
				maybe = true;
			else if ((len >= Tag_Tfhd.Length) && (CMemory.memcmp(d, Tag_Tfhd, (size_t)Tag_Tfhd.Length) == 0))
				maybe = true;
			else if ((len >= Tag_TfmxMod.Length) && (CMemory.memcmp(d, Tag_TfmxMod, (size_t)Tag_TfmxMod.Length) == 0))
				maybe = true;

			if (maybe)
			{
				offsets.Header = 0;

				return true;
			}

			return false;
		}

		#region THE: Added extra methods for snapshot support
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override ISnapshot CreateSnapshot()
		{
			return new Snapshot(playerInfo, voiceVars, rate);
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
			Snapshot clonedSnapshot = new Snapshot(currentSnapshot.PlayerInfo, currentSnapshot.Voices, currentSnapshot.Rate);

			playerInfo = clonedSnapshot.PlayerInfo;
			voiceVars = clonedSnapshot.Voices;
			rate = clonedSnapshot.Rate;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Assign default values to essential runtime variables
		/// </summary>
		/********************************************************************/
		private void Reset()
		{
			playerInfo.Cmd.Aa = playerInfo.Cmd.Bb = playerInfo.Cmd.Cd = playerInfo.Cmd.Ee = 0;

			for (ubyte t = 0; t < playerInfo.Sequencer.Tracks; t++)
			{
				Track tr = playerInfo.Track[t];

				tr.On = GetTrackMute(t);
				tr.Pt = 0xff;
				tr.Tr = 0;
				tr.Pattern.Offset = tr.Pattern.Step = 0;
				tr.Pattern.Wait = 0;
				tr.Pattern.Loops = -1;
			}

			for (ubyte v = 0; v < voices; v++)
			{
				VoiceVars voice = voiceVars[v];
				PaulaVoice paulaVoice = paulaVoices[v];

				voice.VoiceNum = v;
				voice.Envelope.Flag = 0;
				voice.Portamento.Speed = 0;
				voice.KeyUp = true;
				voice.Vibrato.Time = 0;
				voice.Vibrato.Delta = 0;
				voice.WaitOnDmaCount = -1;
				voice.WaitOnDmaPrevLoops = 0;

				voice.AddBeginCount = voice.AddBeginArg = 0;
				voice.AddBeginOffset = 0;

				voice.Period = voice.OutputPeriod = 0;
				voice.Detune = 0;
				voice.Volume = 0;

				voice.Note = voice.NotePrevious = 0;
				voice.NoteVolume = 0;

				voice.Macro.Wait = 1;
				voice.Macro.Step = 0;
				voice.Macro.Skip = true;
				voice.Macro.Loop = 0xff;
				voice.Macro.ExtraWait = true;
				voice.Macro.DelayedOff = false;

				voice.Sid.TargetOffset = (0x100U * v) + 4U;
				voice.Sid.TargetLength = 0;
				voice.Sid.LastSample = 0;
				voice.Sid.Op1.InterDelta = 0;

				voice.Rnd.Flag = 0;

				paulaVoice.Off();

				ToPaulaLength(voice, 1);
				ToPaulaStart(voice, offsets.Silence);

				paulaVoice.Paula.Volume = 0;
				paulaVoice.Paula.Period = 0;
			}

			for (c_int m = 0; m <= Track_Cmd_Max; m++)
				trackCmdUsed[m] = false;

			for (c_int m = 0; m < 16; m++)
				patternCmdUsed[m] = false;

			for (c_int m = 0; m < 0x40; m++)
				macroCmdUsed[m] = false;
		}



		/********************************************************************/
		/// <summary>
		/// With an initialized decoder, calling this should (re)start the
		/// decoder currently chosen song
		/// </summary>
		/********************************************************************/
		private void Restart()
		{
			Reset();
			SoftRestart();
		}



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
			triggerRestart = false;

			playerInfo.Sequencer.Step.Next = false;
			playerInfo.Sequencer.Loops = -1;

			for (c_int step = 0; step < Track_Steps_Max; step++)
				playerInfo.Sequencer.StepSeenBefore[step] = false;

			// Not all songs are designed for looping cleanly, so aid them
			for (ubyte v = 0; v < voices; v++)
			{
				VoiceVars voice = voiceVars[v];

				voice.KeyUp = true;
				voice.Volume = 0;
			}

			playerInfo.Fade.Active = false;
			playerInfo.Fade.Volume = playerInfo.Fade.Target = 64;
			playerInfo.Fade.Delta = 0;

			uword so = (uword)(vSongs[playerInfo.Admin.StartSong] << 1);
			playerInfo.Sequencer.Step.First = playerInfo.Sequencer.Step.Current = MyEndian.ReadBEUword(pBuf, offsets.Header + 0x100 + so);
			playerInfo.Sequencer.Step.Last = MyEndian.ReadBEUword(pBuf, offsets.Header + 0x140 + so);
			playerInfo.Admin.Speed = (sword)MyEndian.ReadBEUword(pBuf, offsets.Header + 0x180 + so);

			if (playerInfo.Admin.Speed >= 0x10)
			{
				SetBpm((uword)playerInfo.Admin.Speed);
				playerInfo.Admin.Speed = 0;

				if (variant.BpmSpeed5)
					playerInfo.Admin.Speed = 5;
			}

			playerInfo.Admin.StartSpeed = playerInfo.Admin.Speed;
			playerInfo.Admin.Count = 0;	// Quick start

			ProcessTrackStep();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SetTfmxV1()
		{
			variant.NoAddBeginCount = true;
			variant.VibratoUnscaled = true;
			variant.FinetuneUnscaled = true;
			variant.PortaUnscaled = false;
			variant.PortaOverride = true;

			macroCmdFuncs[0xd] = MacroFunc_AddVolume;

			// Max. macro cmd = $19
			for (ubyte m = 0x1a; m < 0x40; m++)
				macroCmdFuncs[m] = MacroFunc_Nop;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void AdjustTraitsPost()
		{
			// If track command 7VOICE is used, activate 7V mode.
			// Unfortunately, the way TFMX assigns 16 virtual channels
			// to Amiga Paula using a logical AND 3, we cannot rely on
			// searching for the highest channel number
			if (trackCmdUsed[3] || (input.VersionHint == 3))
			{
				voices = 8;

				// Hippel TFMX7V is 7 voices, mapping 3,4,5,6 to 3
				// Huelsbeck TFMX7V implements 8 voices, mapping 4,5,6,7 to 3
				channelToVoiceMap[3] = 7;
				channelToVoiceMap[4] = 3;
				channelToVoiceMap[5] = 4;
				channelToVoiceMap[6] = 5;
				channelToVoiceMap[7] = 6;

				trackCmdFuncs[3] = TrackCmd_7V;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Cache the original pair of Paula start+length parameters, so it
		/// can be checked and adjusted in case of out of bounds access to
		/// sample data area
		/// </summary>
		/********************************************************************/
		private void ToPaulaStart(VoiceVars v, udword offset)
		{
			PaulaVoice paulaVoice = paulaVoices[v.VoiceNum];

			v.PaulaOrig.Offset = offset;
			paulaVoice.Paula.Start = MakeSamplePtr(offset);

			TakeNextBufChecked(v);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ToPaulaLength(VoiceVars v, uword length)
		{
			PaulaVoice paulaVoice = paulaVoices[v.VoiceNum];

			v.PaulaOrig.Length = length;
			paulaVoice.Paula.Length = length;

			TakeNextBufChecked(v);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TakeNextBufChecked(VoiceVars v)
		{
			PaulaVoice paulaVoice = paulaVoices[v.VoiceNum];

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
		private void ProcessPttr(Track tr)
		{
			// PT < 0x80 : current pattern
			// PT >= 0x80 < 0x90 : continue pattern from previous step
			// PT >= 0x90 : track not used
			if (tr.Pt < 0x90)
			{
				if (tr.Pattern.Offset == 0)		// Track didn't set valid PT before
				{
					tr.Pt = 0xff;
					return;
				}

				if (tr.Pattern.Wait == 0)
					ProcessPattern(tr);
				else
					tr.Pattern.Wait--;
			}
			else
			{
				if (tr.Pt == 0xfe)	// Clear track
				{
					tr.Pt = 0xff;

					ubyte vNum = channelToVoiceMap[tr.Tr & (channelToVoiceMap.Length - 1)];
					VoiceVars v = voiceVars[vNum];
					PaulaVoice p = paulaVoices[vNum];
					v.Macro.Skip = true;
					p.Off();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void NoteCmd()
		{
			ubyte vNum = channelToVoiceMap[playerInfo.Cmd.Cd & (channelToVoiceMap.Length - 1)];
			VoiceVars v = voiceVars[vNum];

			if (playerInfo.Cmd.Aa == 0xfc)			// Lock note
			{
			}
			else if (playerInfo.Cmd.Aa == 0xf7)	// Envelope
			{
				v.Envelope.Speed = playerInfo.Cmd.Bb;

				ubyte tmp = (ubyte)((playerInfo.Cmd.Cd >> 4) + 1);
				v.Envelope.Count = v.Envelope.Flag = tmp;
				v.Envelope.Target = playerInfo.Cmd.Ee;
			}
			else if (playerInfo.Cmd.Aa == 0xf6)	// Vibrato
			{
				ubyte tmp = (ubyte)(playerInfo.Cmd.Bb & 0xfe);
				v.Vibrato.Time = tmp;
				v.Vibrato.Count = (ubyte)(tmp >> 1);
				v.Vibrato.Intensity = (sbyte)playerInfo.Cmd.Ee;
				v.Vibrato.Delta = 0;
			}
			else if (playerInfo.Cmd.Aa == 0xf5)	// Key up
				v.KeyUp = true;
			else if (playerInfo.Cmd.Aa < 0xc0)		// Note
			{
				if (variant.NoNoteDetune)
					v.Detune = 0;
				else
					v.Detune = (sbyte)playerInfo.Cmd.Ee;

				v.NoteVolume = (ubyte)(playerInfo.Cmd.Cd >> 4);
				v.NotePrevious = v.Note;
				v.Note = playerInfo.Cmd.Aa;
				v.KeyUp = false;
				v.Macro.Offset = GetMacroOffset((ubyte)(playerInfo.Cmd.Bb & 0x7f));
				v.Macro.Step = 0;
				v.Macro.Wait = 0;
				v.Macro.Loop = 0xff;
				v.Macro.Skip = false;
				v.EffectsMode = 0;
				v.WaitOnDmaCount = 0;
			}
			else						// Portamento note
			{
				v.Portamento.Count = playerInfo.Cmd.Bb;
				v.Portamento.Wait = 1;

				if (v.Portamento.Speed == 0)
					v.Portamento.Period = v.Period;

				v.Portamento.Speed = playerInfo.Cmd.Ee;
				v.Note = (ubyte)(playerInfo.Cmd.Aa & 0x3f);
				v.Period = NoteToPeriod(v.Note);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uword NoteToPeriod(c_int note)
		{
			if (note >= 0)
				note &= 0x3f;
			else if (note < -13)
				note = -13;

			return periods[note + 13];
		}
		#endregion
	}
}
