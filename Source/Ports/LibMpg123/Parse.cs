/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Ports.LibMpg123.Containers;
using Polycode.NostalgicPlayer.Kit;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123
{
	/// <summary>
	/// Spawned from common; clustering around stream/frame parsing
	/// </summary>
	internal class Parse
	{
		// 11111111 11100000 00000000 00000000
		private const c_uint Hdr_Sync = 0xffe00000;
		private const c_int Hdr_Sync_Shift = 21;
		// 00000000 00011000 00000000 00000000
		private const c_uint Hdr_Version = 0x00180000;
		private const c_int Hdr_Version_Shift = 19;
		// 00000000 00000110 00000000 00000000
		private const c_uint Hdr_Layer = 0x00060000;
		private const c_int Hdr_Layer_Shift = 17;
		// 00000000 00000001 00000000 00000000
		private const c_uint Hdr_Crc = 0x00010000;
		private const c_int Hdr_Crc_Shift = 16;
		// 00000000 00000000 11110000 00000000
		private const c_uint Hdr_BitRate = 0x0000f000;
		private const c_int Hdr_BitRate_Shift = 12;
		// 00000000 00000000 00001100 00000000
		private const c_uint Hdr_SampleRate = 0x00000c00;
		private const c_int Hdr_SampleRate_Shift = 10;
		// 00000000 00000000 00000010 00000000
		private const c_uint Hdr_Padding = 0x00000200;
		private const c_int Hdr_Padding_Shift = 9;
		// 00000000 00000000 00000001 00000000
		private const c_uint Hdr_Private = 0x00000100;
		private const c_int Hdr_Private_Shift = 8;
		// 00000000 00000000 00000000 11000000
		private const c_uint Hdr_Channel = 0x000000c0;
		private const c_int Hdr_Channel_Shift = 6;
		// 00000000 00000000 00000000 00110000
		private const c_uint Hdr_Chanex = 0x00000030;
		private const c_int Hdr_Chanex_Shift = 4;
		// 00000000 00000000 00000000 00001000
		private const c_uint Hdr_Copyright = 0x00000008;
		private const c_int Hdr_Copyright_Shift = 3;
		// 00000000 00000000 00000000 00000100
		private const c_uint Hdr_Original = 0x00000004;
		private const c_int Hdr_Original_Shift = 2;
		// 00000000 00000000 00000000 00000011
		private const c_uint Hdr_Emphasis = 0x00000003;
		private const c_int Hdr_Emphasis_Shift = 0;

		// A generic mask for telling if a header is somewhat valid for the current stream.
		// Meaning: Most basic info is not allowed to change.
		// Checking of channel count needs to be done, too, though. So,
		// if channel count matches, frames are decoded the same way: frame buffers and decoding
		// routines can stay the same, especially frame buffers (think spf * channels!)
		private const c_uint Hdr_CmpMask = Hdr_Sync | Hdr_Version | Hdr_Layer | Hdr_SampleRate;

		// A stricter mask, for matching free format headers
		private const c_uint Hdr_SameMask = Hdr_Sync | Hdr_Version | Hdr_Layer | Hdr_BitRate | Hdr_SampleRate | Hdr_Channel;

		private const c_int Parse_More = (int)Mpg123_Errors.Need_More;
		private const c_int Parse_Err = (int)Mpg123_Errors.Err;
		private const c_int Parse_End = 10;		// No more audio data to find
		private const c_int Parse_Good = 1;		// Everything's fine
		private const c_int Parse_Bad = 0;		// Not fine (invalid data)
		private const c_int Parse_Resync = 2;		// Header not good, go into resync
		private const c_int Parse_Again = 3;		// Really start over, throw away and read a new header, again

		private const c_int Forget_Interval = 1024;	// Used by callers to set forget flag each <n> bytes

		private const c_ulong Track_Max_Frames = c_ulong.MaxValue / 4 / 1152;

		// Bitrates for [mpeg1/2][layer]
		private static readonly c_int[,,] tabSel_123 = new c_int[2, 3, 16]
		{
			{
				{ 0, 32, 64, 96, 128, 160, 192, 224, 256, 288, 320, 352, 384, 416, 448, 0 },
				{ 0, 32, 48, 56,  64,  80,  96, 112, 128, 160, 192, 224, 256, 320, 384, 0 },
				{ 0, 32, 40, 48,  56,  64,  80,  96, 112, 128, 160, 192, 224, 256, 320, 0 }
			},

			{
				{ 0, 32, 48, 56, 64, 80, 96, 112, 128, 144, 160, 176, 192, 224, 256, 0 },
				{ 0,  8, 16, 24, 32, 40, 48,  56,  64,  80,  96, 112, 128, 144, 160, 0 },
				{ 0,  8, 16, 24, 32, 40, 48,  56,  64,  80,  96, 112, 128, 144, 160, 0 }
			}
		};

		private static readonly c_long[] freqs = { 44100, 48000, 32000, 22050, 24000, 16000, 11025, 12000, 8000 };

		private readonly LibMpg123 lib;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Parse(LibMpg123 libMpg123)
		{
			lib = libMpg123;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Frame_BitRate(Mpg123_Handle fr)
		{
			return tabSel_123[fr.Hdr.Lsf, fr.Hdr.Lay - 1, fr.Hdr.BitRate_Index];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_long Int123_Frame_Freq(Mpg123_Handle fr)
		{
			return freqs[fr.Hdr.Sampling_Frequency];
		}



		/********************************************************************/
		/// <summary>
		/// That's a big one: read the next frame. 1 is success, negative or
		/// 0 is some error.
		/// Special error READER_MORE means: Please feed more data and try
		/// again
		/// </summary>
		/********************************************************************/
		public c_int Int123_Read_Frame(Mpg123_Handle fr)
		{
			// TODO: Rework this thing
			c_int freeFormat_Count = 0;

			// Start with current frame header state as copy for roll-back ability
			Frame_Header nhdr = fr.Hdr;

			// Stuff that needs resetting if complete frame reading fails
			c_int oldPhase = fr.HalfPhase;

			// The counter for the search-first-header loop.
			// It is persistent outside the loop to prevent seemingly endless loops
			// when repeatedly headers are found that do not have valid followup headers
			c_long headCount = 0;

			fr.FSizeOld = fr.Hdr.FrameSize;		// For layer 3

			if (HalfSpeed_Do(fr) == 1)
				return 1;

			// From now on, old frame data is tainted by parsing attempts.
			// Handling premature effects of decode header now, more decoupling would be welcome
			fr.To_Decode = fr.To_Ignore = false;

			if (((fr.P.Flags & Mpg123_Param_Flags.No_Frankenstein) != 0) && (((fr.Track_Frames > 0) && (fr.Num >= fr.Track_Frames - 1))))
				return 0;

			Read_Again:
			// In case we are looping to find a valid frame, discard any buffered data
			// before the current position.
			// This is essential to prevent endless looping, always going back to the
			// beginning when feeder buffer is exhausted
			if (fr.Rd.Forget != null)
				fr.Rd.Forget(fr);

			c_int ret = fr.Rd.Head_Read(fr, out c_ulong newHead);
			if (ret <= 0)
				goto Read_Frame_Bad;

			Init_Resync:
			if ((fr.FirstHead == 0) && (Head_Check(newHead) == 0))
			{
				ret = Skip_Junk(fr, ref newHead, ref headCount, ref nhdr);

				// JUMP_CONCLUSION
				if (ret < 0)
					goto Read_Frame_Bad;
				else if (ret == Parse_Again)
					goto Read_Again;
				else if (ret == Parse_Resync)
					goto Init_Resync;
				else if (ret == Parse_End)
				{
					ret = 0;
					goto Read_Frame_Bad;
				}
			}

			ret = Head_Check(newHead);
			if (ret != 0)
				ret = Decode_Header(fr, ref nhdr, newHead, ref freeFormat_Count);

			// JUMP_CONCLUSION
			if (ret < 0)
				goto Read_Frame_Bad;
			else if (ret == Parse_Again)
				goto Read_Again;
			else if (ret == Parse_Resync)
				goto Init_Resync;
			else if (ret == Parse_End)
			{
				ret = 0;
				goto Read_Frame_Bad;
			}

			if (ret == Parse_Bad)
			{
				// Header was not good
				ret = WetWork(fr, ref newHead);		// Messy stuff, handle junk, resync ...

				// JUMP_CONCLUSION
				if (ret < 0)
					goto Read_Frame_Bad;
				else if (ret == Parse_Again)
					goto Read_Again;
				else if (ret == Parse_Resync)
					goto Init_Resync;
				else if (ret == Parse_End)
				{
					ret = 0;
					goto Read_Frame_Bad;
				}

				// Normally, we jumped already. If for some reason everything's fine to continue, do continue
				if (ret != Parse_Good)
					goto Read_Frame_Bad;
			}

			if (fr.FirstHead == 0)
			{
				ret = (fr.P.Flags & Mpg123_Param_Flags.No_Readahead) != 0 ? Parse_Good : Do_ReadAhead(fr, ref nhdr, newHead);

				// Readahead can fail with NEED_MORE, in which case we must also make the
				// just read header available again for next go
				if (ret < 0)
					fr.Rd.Back_Bytes(fr, 4);

				// JUMP_CONCLUSION
				if (ret < 0)
					goto Read_Frame_Bad;
				else if (ret == Parse_Again)
					goto Read_Again;
				else if (ret == Parse_Resync)
					goto Init_Resync;
				else if (ret == Parse_End)
				{
					ret = 0;
					goto Read_Frame_Bad;
				}
			}

			// Now we should have our valid header and proceed to reading the frame
			if ((fr.P.Flags & Mpg123_Param_Flags.No_Frankenstein) != 0)
			{
				if ((fr.FirstHead != 0) && !Head_Compatible(fr.FirstHead, newHead))
					return 0;
			}

			// If filepos is invalid, so is framepos
			int64_t framePos = fr.Rd.Tell(fr) - 4;

			// Flip/init buffer for layer 3
			{
				c_uchar[] newBuf = fr.BsSpace[fr.BsNum];

				// Read main data into memory
				ret = fr.Rd.Read_Frame_Body(fr, newBuf.AsMemory(512), nhdr.FrameSize);
				if (ret < 0)
				{
					// If failed, flip back
					goto Read_Frame_Bad;
				}

				fr.BsBufOld = fr.BsBuf;
				fr.BsBufOldIndex = fr.BsBufIndex;
				fr.BsBuf = newBuf;
				fr.BsBufIndex = 512;
			}

			fr.BsNum = (fr.BsNum + 1) & 1;

			// We read the frame body, time to apply the matching header.
			// Even if erroring out later, the header state needs to match the body
			Apply_Header(fr, ref nhdr);

			if (fr.FirstHead == 0)
			{
				fr.FirstHead = newHead;		// _Now_ it's time to store it... the first real header

				// This is the first header of our current stream segment.
				// It is only the actual first header of the whole stream when fr.num is still below zero!
				// Think of resyncs where firsthead has been reset for format flexibility
				if (fr.Num < 0)
				{
					fr.Audio_Start = framePos;

					// Only check for LAME tag at beginning of whole stream
					// ... when there indeed is one in between, it's the user's problem
					if ((fr.Hdr.Lay == 3) && (Check_Lame_Tag(fr) == 1))
					{
						// ... In practice, Xing/LAME tags are layer 3 only
						if (fr.Rd.Forget != null)
							fr.Rd.Forget(fr);

						fr.OldHead = 0;
						goto Read_Again;
					}

					// Now adjust volume
					lib.frame.Int123_Do_Rva(fr);
				}
			}

			Int123_Set_Pointer(fr, false, 0);

			// No use of nhdr from here on. It is fr.Hdr now!

			// Question: How bad does the floating point value get with repeated recomputation?
			// Also, considering that we can play the file or parts of many times
			if (++fr.Mean_Frames != 0)
				fr.Mean_FrameSize = ((fr.Mean_Frames - 1) * fr.Mean_FrameSize + Int123_Compute_Bpf(fr)) / fr.Mean_Frames;

			++fr.Num;	// 0 for first frame!

			if (((fr.State_Flags & Frame_State_Flags.Frankenstein) == 0) && (((fr.Track_Frames > 0) && (fr.Num >= fr.Track_Frames))))
				fr.State_Flags |= Frame_State_Flags.Frankenstein;

			HalfSpeed_Prepare(fr);

			// Index the position
			fr.Input_Offset = framePos;

			// Keep track of true frame positions in our frame index.
			// But only do so when we are sure that the frame number is accurate...
			if (((fr.State_Flags & Frame_State_Flags.Accurate) != 0) && ((fr.Index.Size != 0) && (fr.Num == fr.Index.Next)))
				lib.index.Int123_Fi_Add(fr.Index, framePos);

			if (fr.Silent_Resync > 0)
				--fr.Silent_Resync;

			if (fr.Rd.Forget != null)
				fr.Rd.Forget(fr);

			fr.To_Decode = fr.To_Ignore = true;

			if (fr.Hdr.Error_Protection)
				fr.Crc = lib.getBits.GetBits_(fr, 16);		// Skip crc

			// Let's check for header change after deciding that the new one is good
			// and actually having read a frame.
			//
			// header_change > 1: decoder structure has to be updated.
			// Preserve header change value from previous runs if it is serious.
			// If we still have a big change pending, it should be dealt with outside,
			// fr.header_change set to zero afterwards
			if (fr.Header_Change < 2)
			{
				fr.Header_Change = 2;	// Output format change is possible

				if (fr.OldHead != 0)	// Check a following header for change
				{
					if (fr.OldHead == newHead)
						fr.Header_Change = 0;
					else
					{
						// Headers that match in this test behave the same for the outside world.
						// Namely: same decoding routines, same amount of decoded data
						if (Head_Compatible(fr.OldHead, newHead))
							fr.Header_Change = 1;
						else
							fr.State_Flags |= Frame_State_Flags.Frankenstein;
					}
				}
				else if ((fr.FirstHead != 0) && !Head_Compatible(fr.FirstHead, newHead))
					fr.State_Flags |= Frame_State_Flags.Frankenstein;
			}

			fr.OldHead = newHead;

			return 1;

			Read_Frame_Bad:
			// Also if we searched for valid data in vein, we can forget skipped data.
			// Otherwise, the feeder would hold every dead old byte in memory until the
			// first valid frame!
			if (fr.Rd.Forget != null)
				fr.Rd.Forget(fr);

			fr.Silent_Resync = 0;

			if (fr.Err == Mpg123_Errors.Ok)
				fr.Err = Mpg123_Errors.Err_Reader;

			fr.HalfPhase = oldPhase;

			// That return code might be inherited from some feeder action, or reader error
			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Prepare for bit reading. Two stages:
		/// 0. Layers 1 and 2, side info for layer 3
		/// 1. Second call for possible bit reservoir for layer 3 part 2, 3.
		///    This overwrites side info needed for stage 0.
		///
		/// Continuing to read bits after layer 3 side info shall fail unless
		/// INT123_Set_Pointer() is called to refresh things
		/// </summary>
		/********************************************************************/
		public void Int123_Set_Pointer(Mpg123_Handle fr, bool part2, c_long backStep)
		{
			fr.BitIndex = 0;

			if (fr.Hdr.Lay == 3)
			{
				if (part2)
				{
					fr.WordPointer = fr.BsBuf;
					fr.WordPointerIndex = fr.BsBufIndex + fr.Hdr.SSize - backStep;

					if (backStep != 0)
						Array.Copy(fr.BsBufOld, fr.BsBufOldIndex + fr.FSizeOld - backStep, fr.WordPointer, fr.WordPointerIndex, backStep);

					fr.Bits_Avail = (fr.Hdr.FrameSize - fr.Hdr.SSize + backStep) * 8;
				}
				else
				{
					fr.WordPointer = fr.BsBuf;
					fr.WordPointerIndex = fr.BsBufIndex;
					fr.Bits_Avail = fr.Hdr.SSize * 8;
				}
			}
			else
			{
				fr.WordPointer = fr.BsBuf;
				fr.WordPointerIndex = fr.BsBufIndex;
				fr.Bits_Avail = fr.Hdr.FrameSize * 8;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_double Int123_Compute_Bpf(Mpg123_Handle fr)
		{
			return (fr.Hdr.FrameSize > 0) ? fr.Hdr.FrameSize + 4.0 : 1.0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private c_int Head_Check(c_ulong head)
		{
			if 
			(
				((head & Hdr_Sync) != Hdr_Sync)
				||
				// Version: 00, 10, 11 is 2.5,2,1; 01 is reserved (added by Thomas Neumann)
				(((head & Hdr_Version) >> Hdr_Version_Shift) == 1)
				||
				// Layer: 01,10,11 is 1,2,3; 00 is reserved
				(((head & Hdr_Layer) >> Hdr_Layer_Shift) == 0)
				||
				// 1111 means bad bitrate
				(((head & Hdr_BitRate) >> Hdr_BitRate_Shift) == 0xf)
				||
				// Sampling freq: 11 is reserved
				(((head & Hdr_SampleRate) >> Hdr_SampleRate_Shift) == 0x3)
//				||
				// Emphasis: 10 is reserved (added by Thomas Neumann)
//				(((head & Hdr_Emphasis) >> Hdr_Emphasis_Shift) == 0x2)
				// Here used to be a mpeg 2.5 check... re-enabled 2.5 decoding due to lack
				// of evidence that it is really not good
			)
			{
				return 0;
			}

			// If no check failed, the header is valid (hopefully)
			return 1;
		}



		/********************************************************************/
		/// <summary>
		/// This is moderately sized buffers. Int offset is enough
		/// </summary>
		/********************************************************************/
		private c_ulong Bit_Read_Long(Span<c_uchar> buf, ref c_int offset)
		{
			c_ulong val =     // 32 bit value
				  (((c_ulong)buf[offset]) << 24)
				| (((c_ulong)buf[offset + 1]) << 16)
				| (((c_ulong)buf[offset + 2]) << 8)
				| (buf[offset + 3]);
			offset += 4;

			return val;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_ushort Bit_Read_Short(Span<c_uchar> buf, ref c_int offset)
		{
			c_ushort val = (c_ushort)    // 16 bit value
				  ((buf[offset] << 8)
				| (buf[offset + 3]));
			offset += 2;

			return val;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Check_Lame_Tag(Mpg123_Handle fr)
		{
			// Going to look for Xing or Info at some position after the header
			//                                    MPEG 1  MPEG 2/2.5 (LSF)
			// Stereo, Joint Stereo, Dual Channel  32      17
			// Mono                                17       9
			c_int lame_Offset = (fr.Stereo == 2) ? (fr.Hdr.Lsf != 0 ? 17 : 32) : (fr.Hdr.Lsf != 0 ? 9 : 17);

			if ((fr.P.Flags & Mpg123_Param_Flags.Ignore_InfoFrame) != 0)
				goto Check_Lame_Tag_No;

			// Note: CRC or not, that does not matter here.
			// But, there is any combination of Xing flags in the wild. There are headers
			// without the search index table! I cannot assume a reasonable minimal size
			// for the actual data, have to check if each byte of information is present.
			// But: 4 B Info/Xing + 4 B flags is bare minimum
			if (fr.Hdr.FrameSize < (lame_Offset + 8))
				goto Check_Lame_Tag_No;

			// Only search for tag when all zero before it (apart from checksum)
			Span<c_uchar> bsBufSpan = fr.BsBuf.AsSpan(fr.BsBufIndex);

			for (c_int i = 2; i < lame_Offset; ++i)
			{
				if (bsBufSpan[i] != 0)
					goto Check_Lame_Tag_No;
			}

			if ((bsBufSpan[lame_Offset] == 'I') && (bsBufSpan[lame_Offset + 1] == 'n') && (bsBufSpan[lame_Offset + 2] == 'f') && (bsBufSpan[lame_Offset + 3] == 'o'))
			{
				// We still have to see what there is
			}
			else if ((bsBufSpan[lame_Offset] == 'X') && (bsBufSpan[lame_Offset + 1] == 'i') && (bsBufSpan[lame_Offset + 2] == 'n') && (bsBufSpan[lame_Offset + 3] == 'g'))
			{
				fr.Vbr = Mpg123_Vbr.Vbr;	// Xing header means always VBR
			}
			else
				goto Check_Lame_Tag_No;

			// We have one of these headers...
			lame_Offset += 4;
			c_ulong xing_Flags = Bit_Read_Long(bsBufSpan, ref lame_Offset);

			// From now on, I have to carefully check if the announced data is actually
			// there! I'm always returning 'yes', though
			if ((xing_Flags & 1) != 0)	// Total bitstram frames
			{
				if (fr.Hdr.FrameSize < (lame_Offset + 4))
					goto Check_Lame_Tag_Yes;

				c_ulong long_Tmp = Bit_Read_Long(bsBufSpan, ref lame_Offset);

				if ((fr.P.Flags & Mpg123_Param_Flags.Ignore_StreamLength) == 0)
				{
					// Check for endless stream, but: TRACK_MAX_FRAMES sensible at all?
					fr.Track_Frames = long_Tmp > Track_Max_Frames ? 0 : long_Tmp;
				}
			}

			if ((xing_Flags & 0x2) != 0)	// Total bitstream bytes
			{
				if (fr.Hdr.FrameSize < (lame_Offset + 4))
					goto Check_Lame_Tag_Yes;

				c_ulong long_Tmp = Bit_Read_Long(bsBufSpan, ref lame_Offset);

				if ((fr.P.Flags & Mpg123_Param_Flags.Ignore_StreamLength) == 0)
				{
					// The Xing bitstream length, at least as interpreted by the Lame
					// encoder, encompasses all data from the Xing header frame on,
					// ignoring leading ID3v2 data. Trailing tags (ID3v1) seem to be
					// included, though
					if (fr.RDat.FileLen < 1)
						fr.RDat.FileLen = long_Tmp + fr.Audio_Start;	// Overflow?
				}
			}

			if ((xing_Flags & 0x4) != 0)	// TOC
			{
				if (fr.Hdr.FrameSize < (lame_Offset + 100))
					goto Check_Lame_Tag_Yes;

				lib.frame.Int123_Frame_Fill_Toc(fr, fr.BsBuf.AsMemory(fr.BsBufIndex + lame_Offset));
				lame_Offset += 100;
			}

			if ((xing_Flags & 0x8) != 0)	// VBR quality
			{
				if (fr.Hdr.FrameSize < (lame_Offset + 4))
					goto Check_Lame_Tag_Yes;

				Bit_Read_Long(bsBufSpan, ref lame_Offset);
			}

			// Either zeros/nothing, or:
			//     0-8: LAME3.90a
			//     9: Revision/VBR method
			//     10: Lowpass
			//     11-18: ReplayGain
			//     19: Encoder flags
			//     20: ABR
			//     21-23: Encoder delays
			if (fr.Hdr.FrameSize < (lame_Offset + 24))	// I'm interested in 24 B of extra info
				goto Check_Lame_Tag_Yes;

			if (bsBufSpan[lame_Offset] != 0)
			{
				c_float[] replay_Gain = { 0.0f, 0.0f };
				c_float gain_Offset = 0;  // Going to be +6 for old lame that used 83dB
				c_uchar[] nb = new c_uchar[10];

				Array.Copy(fr.BsBuf, fr.BsBufIndex + lame_Offset, nb, 0, 9);
				nb[9] = 0;

				if ((nb[0] == 0x4c) && (nb[1] == 0x41) && (nb[2] == 0x4d) && (nb[3] == 0x45))		// LAME
				{
					// Lame versions before 3.95.1 used 83 dB reference level, later
					// versions 89 dB. We stick with 89 dB as being "normal", adding
					// 6 dB
					CSScanF csscanf = new CSScanF();
					string nbStr = EncoderCollection.Win1252.GetString(nb.AsSpan(4));

					if (csscanf.Parse(nbStr, "%u.%u%s") >= 2)
					{
						// We cannot detect LAME 3.95 reliably (same version string as
						// 3.95.1), so this is a blind spot. Everything < 3.95 is safe,
						// though
						c_uint major = (c_uint)csscanf.Results[0];
						c_uint minor = (c_uint)csscanf.Results[1];

						if ((major < 3) || ((major == 3) && (minor < 95)))
							gain_Offset = 6;
					}
				}

				lame_Offset += 9;	// 9 in

				// The 4 big bits are tag revision, the small bits vbr method
				c_uchar lame_Vbr = (c_uchar)(bsBufSpan[lame_Offset] & 15);
				lame_Offset += 1;	// 10 in

				switch (lame_Vbr)
				{
					// From rev1 proposal... not sure if all good in practice
					case 1:
					case 8:
					{
						fr.Vbr = Mpg123_Vbr.Cbr;
						break;
					}

					case 2:
					case 9:
					{
						fr.Vbr = Mpg123_Vbr.Abr;
						break;
					}

					default:
					{
						fr.Vbr = Mpg123_Vbr.Vbr;	// 00=unknown is taken as VBR
						break;
					}
				}

				lame_Offset += 1;	// 11 in, skipping lowpass filter value

				// ReplayGain peak ampitude, 32 bit float -- why did I parse it as int
				// before?? Ah, yes, Lame seems to store it as int since some day in 2003;
				// I've only seen zeros anyway until now, bah!
				if ((bsBufSpan[lame_Offset] != 0) || (bsBufSpan[lame_Offset + 1] != 0) || (bsBufSpan[lame_Offset + 2] != 0) && (bsBufSpan[lame_Offset + 3] != 0))
				{
					;
				}

				lame_Offset += 4;	// 15 in

				// ReplayGain values - lame only writes radio mode gain...
				// 16bit gain, 3 bits name, 3 bits originator, sign (1=-, 0=+),
				// dB value*10 in 9 bits (fixed point) ignore the setting if name or
				// originator == 000!
				// radio      0 0 1 0 1 1 1 0 0 1 1 1 1 1 0 1
				// audiophile 0 1 0 0 1 0 0 0 0 0 0 1 0 1 0 0
				for (c_int i = 0; i < 2; ++i)
				{
					c_uchar gt = (c_uchar)(bsBufSpan[lame_Offset] >> 5);
					c_uchar origin = (c_uchar)((bsBufSpan[lame_Offset] >> 2) & 0x7);
					c_float factor = (bsBufSpan[lame_Offset] & 0x2) != 0 ? -0.1f : 0.1f;
					c_ushort gain = (c_ushort)(Bit_Read_Short(bsBufSpan, ref lame_Offset) & 0x1ff);

					// 19 in (2 cycles)
					if ((origin == 0) || (gt < 1) || (gt > 2))
						continue;

					--gt;
					replay_Gain[gt] = factor * gain;

					// Apply gain offset for automatic origin
					if (origin == 3)
						replay_Gain[gt] += gain_Offset;
				}

				for (c_int i = 0; i < 2; ++i)
				{
					if (fr.Rva.Level[i] <= 0)
					{
						fr.Rva.Peak[i] = 0;     // TODO: use parsed peak?
						fr.Rva.Gain[i] = replay_Gain[i];
						fr.Rva.Level[i] = 0;
					}
				}

				lame_Offset += 1;	// 20 in, skipping encoding flag byte

				// ABR rate
				if (fr.Vbr == Mpg123_Vbr.Abr)
					fr.Abr_Rate = bsBufSpan[lame_Offset];

				lame_Offset += 1;	// 21 in

				// Encoder delay and padding, two 12 bit values
				// ... lame does write them from int
				int64_t pad_In = (((bsBufSpan[lame_Offset]) << 4) | ((bsBufSpan[lame_Offset + 1]) >> 4));
				int64_t pad_Out = (((bsBufSpan[lame_Offset + 1]) << 8) | (bsBufSpan[lame_Offset + 2])) & 0xfff;

				lame_Offset += 3;	// 24 in

				// Store even if libmpg123 does not do gapless decoding itself
				fr.Enc_Delay = (c_int)pad_In;
				fr.Enc_Padding = (c_int)pad_Out;

				// Final: 24 B LAME data
			}

			Check_Lame_Tag_Yes:
			// Switch buffer back
			fr.BsBuf = fr.BsSpace[fr.BsNum];
			fr.BsBufIndex = 512;
			fr.BsNum = (fr.BsNum + 1) & 1;

			return 1;

			Check_Lame_Tag_No:
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Just tell if the header is some mono
		/// </summary>
		/********************************************************************/
		private bool Header_Mono(c_ulong newHead)
		{
			return ((newHead & Hdr_Channel) >> Hdr_Channel_Shift) == (c_ulong)Mode.Mono;
		}



		/********************************************************************/
		/// <summary>
		/// True if the two headers will work with the same decoding routines
		/// </summary>
		/********************************************************************/
		private bool Head_Compatible(c_ulong fred, c_ulong bret)
		{
			return ((fred & Hdr_CmpMask) == (bret & Hdr_CmpMask) && (Header_Mono(fred) == Header_Mono(bret)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void HalfSpeed_Prepare(Mpg123_Handle fr)
		{
			// Save for repetition
			if ((fr.P.HalfSpeed != 0) && (fr.Hdr.Lay == 3))
				Array.Copy(fr.BsBuf, fr.BsBufIndex, fr.SSave, 0, fr.Hdr.SSize);
		}



		/********************************************************************/
		/// <summary>
		/// If this returns 1, the next frame is the repetition
		/// </summary>
		/********************************************************************/
		private c_int HalfSpeed_Do(Mpg123_Handle fr)
		{
			// Speed-down hack: Play it again, Sam (the frame, I mean)
			if (fr.P.HalfSpeed != 0)
			{
				if (fr.HalfPhase != 0)	// Repeat last frame
				{
					fr.To_Decode = fr.To_Ignore = true;
					--fr.HalfPhase;

					Int123_Set_Pointer(fr, false, 0);

					if (fr.Hdr.Lay == 3)
						Array.Copy(fr.SSave, 0, fr.BsBuf, fr.BsBufIndex, fr.Hdr.SSize);

					if (fr.Hdr.Error_Protection)
						fr.Crc = lib.getBits.GetBits_(fr, 16);	// Skip crc

					return 1;
				}
				else
					fr.HalfPhase = fr.P.HalfSpeed - 1;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Read ahead and find the next MPEG header, to guess framesize.
		/// Return value: Success code
		/// 
		/// PARSE_GOOD: Found a valid frame size (stored in the handle).
		/// Negative: Error codes, possibly from feeder buffer (NEED_MORE)
		/// PARSE_BAD: Cannot get the framesize for some reason and shall
		/// silentry try the next possible header (if this is no free format
		/// stream after all...)
		/// </summary>
		/********************************************************************/
		private c_int Guess_FreeFormat_FrameSize(Mpg123_Handle fr, c_ulong oldHead, ref c_int frameSize)
		{
			if ((fr.RDat.Flags & (ReaderFlags.Seekable | ReaderFlags.Buffered)) == 0)
				return Parse_Bad;

			c_int ret = fr.Rd.Head_Read(fr, out c_ulong head);
			if (ret <= 0)
				return ret;

			// We are already 4 bytes into it
			c_int i;
			for (i = 4; i < Constant.MaxFrameSize + 4; i++)
			{
				ret = fr.Rd.Head_Shift(fr, ref head);
				if (ret <= 0)
					return ret;

				// No head_check needed, the mask contains all relevant bits
				if ((head & Hdr_SameMask) == (oldHead & Hdr_SameMask))
				{
					fr.Rd.Back_Bytes(fr, i + 1);
					frameSize = i - 3;

					return Parse_Good;	// Success!
				}
			}

			fr.Rd.Back_Bytes(fr, i);

			return Parse_Bad;
		}



		/********************************************************************/
		/// <summary>
		/// Decode a header and write the information into the frame
		/// structure. Return values are compatible with those of
		/// INT123_read_frame, namely:
		/// 
		/// 1: Success
		/// 0: No valid header
		/// Negative: Some error
		/// 
		/// You are required to do a head_check() before calling!
		///
		/// This now only operates on a frame header struct, not the full frame
		/// structure. The scope is limited to parsing header information and
		/// determining the size of the frame body to read. Everything else
		/// belongs into a later stage of applying header information to the
		/// main decoder frame structure
		/// </summary>
		/********************************************************************/
		private c_int Decode_Header(Mpg123_Handle fr, ref Frame_Header fh, c_ulong newHead, ref c_int freeFormat_Count)
		{
			// For some reason, the layer and sampling freq settings used to be wrapped
			// in a weird conditional including MPG123_NO_RESYNC. What was I thinking?
			// This information has to be consistent
			fh.Lay = (c_int)(4U - ((newHead & Hdr_Layer) >> Hdr_Layer_Shift));

			if ((((newHead & Hdr_Version) >> Hdr_Version_Shift) & 0x2) != 0)
			{
				fh.Lsf = (((newHead & Hdr_Version) >> Hdr_Version_Shift) & 0x1) != 0 ? 0 : 1;
				fh.Mpeg25 = false;
				fh.Sampling_Frequency = (c_int)(((newHead & Hdr_SampleRate) >> Hdr_SampleRate_Shift) + (c_ulong)(fh.Lsf * 3U));
			}
			else
			{
				fh.Lsf = 1;
				fh.Mpeg25 = true;
				fh.Sampling_Frequency = (c_int)(6U + ((newHead & Hdr_SampleRate) >> Hdr_SampleRate_Shift));
			}

			fh.Error_Protection = (((newHead & Hdr_Crc) >> Hdr_Crc_Shift) ^ 0x1) != 0;
			fh.BitRate_Index = (c_int)((newHead & Hdr_BitRate) >> Hdr_BitRate_Shift);
			fh.Padding = (c_int)((newHead & Hdr_Padding) >> Hdr_Padding_Shift);
			fh.Extension = ((newHead & Hdr_Private) >> Hdr_Private_Shift) != 0;
			fh.Mode = (Mode)((newHead & Hdr_Channel) >> Hdr_Channel_Shift);
			fh.Mode_Ext = (c_int)((newHead & Hdr_Chanex) >> Hdr_Chanex_Shift);
			fh.Copyright = ((newHead & Hdr_Copyright) >> Hdr_Copyright_Shift) != 0;
			fh.Original = ((newHead & Hdr_Original) >> Hdr_Original_Shift) != 0;
			fh.Emphasis = (c_int)((newHead & Hdr_Emphasis) >> Hdr_Emphasis_Shift);
			fh.FreeFormat = (newHead & Hdr_BitRate) == 0;

			// We can't use tabsel_123 for freeformat, so trying to guess framesize...
			if (fh.FreeFormat)
			{
				// When we first encounter the frame with freeformat, guess framesize
				if (fh.FreeFormat_FrameSize < 0)
				{
					if ((fr.P.Flags & Mpg123_Param_Flags.No_Readahead) != 0)
						return Parse_Bad;

					freeFormat_Count += 1;

					if (freeFormat_Count > 5)
						return Parse_Bad;

					int ret = Guess_FreeFormat_FrameSize(fr, newHead, ref fh.FrameSize);
					if (ret == Parse_Good)
						fh.FreeFormat_FrameSize = fh.FrameSize - fh.Padding;
					else
						return ret;
				}
				else	// Freeformat should be CBR, so the same framesize can be used at the 2nd reading or later
					fh.FrameSize = fh.FreeFormat_FrameSize + fh.Padding;
			}

			switch (fh.Lay)
			{
				case 1:
				{
					if (!fh.FreeFormat)
					{
						c_long fs = tabSel_123[fh.Lsf, 0, fh.BitRate_Index] * 12000;
						fs /= freqs[fh.Sampling_Frequency];
						fs = ((fs + fh.Padding) << 2) - 4;
						fh.FrameSize = fs;
					}
					break;
				}

				case 2:
				{
					if (!fh.FreeFormat)
					{
						c_long fs = tabSel_123[fh.Lsf, 1, fh.BitRate_Index] * 144000;
						fs /= freqs[fh.Sampling_Frequency];
						fs += fh.Padding - 4;
						fh.FrameSize = fs;
					}
					break;
				}

				case 3:
				{
					if (fh.Lsf != 0)
						fh.SSize = (fh.Mode == Mode.Mono) ? 9 : 17;
					else
						fh.SSize = (fh.Mode == Mode.Mono) ? 17 : 32;

					if (fh.Error_Protection)
						fh.SSize += 2;

					if (!fh.FreeFormat)
					{
						c_long fs = tabSel_123[fh.Lsf, 2, fh.BitRate_Index] * 144000;
						fs /= freqs[fh.Sampling_Frequency] << fh.Lsf;
						fs += fh.Padding - 4;
						fh.FrameSize = fs;
					}

					if (fh.FrameSize < fh.SSize)
						return Parse_Bad;

					break;
				}

				default:
					return Parse_Bad;
			}

			if (fh.FrameSize > Constant.MaxFrameSize)
				return Parse_Bad;

			return Parse_Good;
		}



		/********************************************************************/
		/// <summary>
		/// Apply decoded header structure to frame struct, including main
		/// decoder function pointer
		/// </summary>
		/********************************************************************/
		private void Apply_Header(Mpg123_Handle fr, ref Frame_Header hdr)
		{
			// Copy whole struct, do some postprocessing
			fr.Hdr = hdr;
			fr.Stereo = (fr.Hdr.Mode == Mode.Mono) ? 1 : 2;

			switch (fr.Hdr.Lay)
			{
				case 1:
				{
					fr.Spf = 384;
					fr.Do_Layer = lib.layer1.Int123_Do_Layer1;
					break;
				}

				case 2:
				{
					fr.Spf = 1152;
					fr.Do_Layer = lib.layer2.Int123_Do_Layer2;
					break;
				}

				case 3:
				{
					fr.Spf = fr.Hdr.Lsf != 0 ? 576 : 1152;	// MPEG 2.5 implies LSF
					fr.Do_Layer = lib.layer3.Int123_Do_Layer3;
					break;
				}

				default:
				{
					// No error checking/message here, been done in Decode_Header()
					fr.Spf = 0;
					fr.Do_Layer = null;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// First attempt of read ahead check to find the real first header;
		/// cannot believe what junk is out there!
		/// </summary>
		/********************************************************************/
		private c_int Do_ReadAhead(Mpg123_Handle fr, ref Frame_Header nhdr, c_ulong newHead)
		{
			c_ulong nextHead = 0;
			c_int hd = 0;

			if (!((fr.FirstHead == 0) && ((fr.RDat.Flags & (ReaderFlags.Seekable | ReaderFlags.Buffered)) != 0)))
				return Parse_Good;

			int64_t start = fr.Rd.Tell(fr);

			// Step framesize bytes forward and read next possible header
			int64_t oRet = fr.Rd.Skip_Bytes(fr, nhdr.FrameSize);
			if (oRet < 0)
				return oRet == (int64_t)Mpg123_Errors.Need_More ? Parse_More : Parse_Err;

			// Read header, seek back
			hd = fr.Rd.Head_Read(fr, out nextHead);

			if (fr.Rd.Back_Bytes(fr, fr.Rd.Tell(fr) - start) < 0)
				return Parse_Err;

			if (hd == (c_int)Mpg123_Errors.Need_More)
				return Parse_More;

			if (hd == 0)
				return Parse_End;

			if ((Head_Check(nextHead) == 0) || !Head_Compatible(newHead, nextHead))
			{
				fr.OldHead = 0;		// Start over

				// Try next byte for valid header
				c_int ret = fr.Rd.Back_Bytes(fr, 3);
				if (ret < 0)
					return Parse_Err;

				return Parse_Again;
			}
			else
				return Parse_Good;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Handle_Id3V2(Mpg123_Handle fr, c_ulong newHead)
		{
			fr.OldHead = 0;		// Think about that. Used to be present only for skipping of junk, not resync-style wetwork

			c_int ret = lib.id3.Int123_Parse_New_Id3(fr, newHead);
			if (ret < 0)
				return ret;
			else if (ret > 0)
				fr.MetaFlags |= Mpg123_MetaFlags.New_Id3 | Mpg123_MetaFlags.Id3;

			return Parse_Again;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Handle_ApeTag(Mpg123_Handle fr, c_ulong newHead)
		{
			c_uchar[] apeBuf = new c_uchar[28];

			// How many bytes to backpedal to get back to just after the first byte of
			// the supposed header
			c_int back_Bytes = 3;
			fr.OldHead = 0;

			// Apetag headers are 32 bytes, newhead contains 4, read the rest
			c_int ret = (c_int)fr.Rd.FullRead(fr, apeBuf, 28);
			if (ret < 0)
				return ret;

			back_Bytes += ret;
			if (ret < 28)
				goto ApeTag_Bad;

			// Apetags starts with "APETAGEX", "APET" is already tested
			if ((apeBuf[0] != 0x41) || (apeBuf[1] != 0x47) || (apeBuf[2] != 0x45) || (apeBuf[3] != 0x58))
				goto ApeTag_Bad;

			// Version must be 2.000 / 2000
			c_ulong val = ((c_ulong)apeBuf[11] << 24) | ((c_ulong)apeBuf[10] << 16) | ((c_ulong)apeBuf[9] << 8) | apeBuf[8];

			// If encountering EOF here, things are just at an end
			ret = (c_int)fr.Rd.Skip_Bytes(fr, val);
			if (ret < 0)
				return ret;

			return Parse_Again;

			ApeTag_Bad:
			fr.Rd.Back_Bytes(fr, back_Bytes);

			return Parse_Again;		// Give the resync code a chance to fix things
		}



		/********************************************************************/
		/// <summary>
		/// Advance a byte in stream to get next possible header and forget
		/// buffered data if possible (for feed reader)
		/// </summary>
		/********************************************************************/
		private c_int Forget_Head_Shift(Mpg123_Handle fr, ref c_ulong newHeadP, bool forget)
		{
			c_int ret = fr.Rd.Head_Shift(fr, ref newHeadP);
			if (ret <= 0)
				return ret;

			// Try to forget buffered data as early as possible to speed up parsing where
			// new data needs to be added for resync (and things would be re-parsed again
			// and again because of the start from beginning after hitting end)
			if (forget && (fr.Rd.Forget != null))
			{
				// Ensure that the last 4 bytes stay in buffers for reading the header
				// anew
				if (fr.Rd.Back_Bytes(fr, 4) == 0)
				{
					fr.Rd.Forget(fr);
					fr.Rd.Back_Bytes(fr, -4);
				}
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Watch out for junk/tags on beginning of stream by invalid header
		/// </summary>
		/********************************************************************/
		private c_int Skip_Junk(Mpg123_Handle fr, ref c_ulong newHeadP, ref c_long headCount, ref Frame_Header nhdr)
		{
			c_int ret;
			c_int freeFormat_Count = 0;
			c_long limit = 65536;
			c_ulong newHead = newHeadP;
			c_uint forgetCount = 0;

			// Check for id3v2; first three bytes (of 4) are "ID3"
			if ((newHead & 0xffffff00) == 0x49443300)
				return Handle_Id3V2(fr, newHead);

			// I even saw RIFF headers at the beginning of MPEG streams ;(
			if (newHead == (('R' << 24) + ('I' << 16) + ('F' << 8) + 'F'))
			{
				ret = fr.Rd.Head_Read(fr, out newHead);
				if (ret <= 0)
					return ret;

				while (newHead != (('d' << 24) + ('a' << 16) + ('t' << 8) + 'a'))
				{
					if (++forgetCount > Forget_Interval)
						forgetCount = 0;

					ret = Forget_Head_Shift(fr, ref newHead, forgetCount == 0);
					if (ret <= 0)
						return ret;
				}

				ret = fr.Rd.Head_Read(fr, out newHead);
				if (ret <= 0)
					return ret;

				fr.OldHead = 0;
				newHeadP = newHead;

				return Parse_Again;
			}

			// Unhandled junk... just continue search for a header, stepping in single
			// bytes through next 64K.
			// This is rather identical to the resync loop
			newHeadP = 0;	// Invalid the external value
			ret = 0;		// We will check the value after the loop

			// We prepare for at least the 64K bytes as usual, unless
			// user explicitly wanted more (even infinity). Never less
			if ((fr.P.Resync_Limit < 0) || (fr.P.Resync_Limit > limit))
				limit = fr.P.Resync_Limit;

			do
			{
				++headCount;

				if ((limit >= 0) && (headCount >= limit))
					break;

				if (++forgetCount > Forget_Interval)
					forgetCount = 0;

				ret = Forget_Head_Shift(fr, ref newHead, forgetCount == 0);
				if (ret <= 0)
					return ret;

				if ((Head_Check(newHead) != 0) && ((ret = Decode_Header(fr, ref nhdr, newHead, ref freeFormat_Count)) != 0))
					break;
			}
			while (true);

			if (ret < 0)
				return ret;

			if ((limit >= 0) && (headCount >= limit))
			{
				fr.Err = Mpg123_Errors.Resync_Fail;
				return Parse_Err;
			}

			// If the new header is good, it is already decoded
			newHeadP = newHead;

			return Parse_Good;
		}



		/********************************************************************/
		/// <summary>
		/// The newhead is bad, so let's check if it is something special,
		/// otherwise just resync
		/// </summary>
		/********************************************************************/
		private c_int WetWork(Mpg123_Handle fr, ref c_ulong newHeadP)
		{
			c_int ret = Parse_Err;
			c_ulong newHead = newHeadP;
			newHeadP = 0;

			// Classic ID3 tags. Read, then start parsing again
			if ((newHead & 0xffffff00) == (('T' << 24) + ('A' << 16) + ('G' << 8)))
			{
				fr.Id3Buf[0] = (c_uchar)((newHead >> 24) & 0xff);
				fr.Id3Buf[1] = (c_uchar)((newHead >> 16) & 0xff);
				fr.Id3Buf[2] = (c_uchar)((newHead >> 8) & 0xff);
				fr.Id3Buf[3] = (c_uchar)(newHead & 0xff);

				ret = (c_int)fr.Rd.FullRead(fr, fr.Id3Buf.AsMemory(4), 124);
				if (ret < 0)
					return ret;

				fr.MetaFlags |= Mpg123_MetaFlags.New_Id3 | Mpg123_MetaFlags.Id3;
				fr.RDat.Flags |= ReaderFlags.Id3Tag;	// That marks id3v1

				return Parse_Again;
			}

			// This is similar to initial junk skipping code...
			// Check for id3v2; first three bytes (of 4) are "ID3"
			if ((newHead & 0xffffff00) == 0x49443300)
				return Handle_Id3V2(fr, newHead);

			// Check for an apetag header
			if (newHead == (('A' << 24) + ('P' << 16) + ('E' << 8) + 'T'))
				return Handle_ApeTag(fr, newHead);

			// Now we got something bad at hand, try to recover
			if ((fr.P.Flags & Mpg123_Param_Flags.No_Resync) == 0)
			{
				c_long try_ = 0;
				c_long limit = fr.P.Resync_Limit;
				c_uint forgetCount = 0;

				// If a resync is needed the bitreservoir of previous frames is no longer valid
				fr.BitReservoir = 0;

				do	// ... Shift the header with additional single bytes until be found something that could be a header
				{
					++try_;

					if ((limit >= 0) && (try_ >= limit))
						break;

					if (++forgetCount > Forget_Interval)
						forgetCount = 0;

					ret = Forget_Head_Shift(fr, ref newHead, forgetCount == 0);
					if (ret <= 0)
					{
						newHeadP = newHead;

						return ret != 0 ? ret : Parse_End;
					}
				}
				while (Head_Check(newHead) == 0);

				newHeadP = newHead;

				// Now we either got something that could be a header, or we gave up
				if ((limit >= 0) && (try_ >= limit))
				{
					fr.Err = Mpg123_Errors.Resync_Fail;

					return Parse_Err;
				}
				else
				{
					fr.OldHead = 0;

					return Parse_Resync;
				}
			}
			else
			{
				fr.Err = Mpg123_Errors.Out_Of_Sync;

				return Parse_Err;
			}
		}
		#endregion
	}
}
