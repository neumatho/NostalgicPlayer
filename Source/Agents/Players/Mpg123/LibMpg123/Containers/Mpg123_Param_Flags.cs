/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers
{
	/// <summary>
	/// Flag bits for MPG123_FLAGS, use the usual binary or to combine
	/// </summary>
	[Flags]
	internal enum Mpg123_Param_Flags : c_long
	{
		None = 0,
		Force_Mono = 0x7,				// <     0111 Force some mono mode: This is a test bitmask for seeing if any mono forcing is active
		Mono_Left = 0x1,				// <     0001 Force playback of left channel only
		Mono_Right = 0x2,				// <     0010 Force playback of right channel only
		Mono_Mix = 0x4,					// <     0100 Force playback of mixed mono
		Force_Stereo = 0x8,				// <     1000 Force stereo output
		Force_8Bit = 0x10,				// < 00010000 Force 8bit formats
		Quiet = 0x20,					// < 00100000 Suppress any printouts (overrules verbose)
		Gapless = 0x40,					// < 01000000 Enable gapless decoding (default on if libmpg123 has support)
		No_Resync = 0x80,				// < 10000000 Disable resync stream after error
		SeekBuffer = 0x100,				// < 000100000000 Enable small buffer on non-seekable streams to allow some peek-ahead (for better MPEG sync)
		Fuzzy = 0x200,					// < 001000000000 Enable fuzzy seeks (guessing byte offsets or using approximate seek points from Xing TOC)
		Force_Float = 0x400,			// < 010000000000 Force floating point output (32 or 64 bits depends on mpg123 internal precision)
		Plain_Id3Text = 0x800,			// < 100000000000 Do not translate ID3 text data to UTF-8. ID3 strings will contain the raw text data, with the first byte containing the ID3 encoding code
		Ignore_StreamLength = 0x1000,	// < 1000000000000 Ignore any stream length information contained in the stream, which can be contained in a 'TLEN' frame of an ID3v2 tag or a Xing tag
		Skip_Id3V2 = 0x2000,			// < 10 0000 0000 0000 Do not parse ID3v2 tags, just skip them
		Ignore_InfoFrame = 0x4000,		// < 100 0000 0000 0000 Do not parse the LAME/Xing info frame, treat it as normal MPEG data
		Auto_Resample = 0x8000,			// < 1000 0000 0000 0000 Allow automatic internal resampling of any kind (default on if supported). Especially when going lowlevel with replacing output buffer, you might want to unset this flag. Setting MPG123_DOWNSAMPLE or MPG123_FORCE_RATE will override this
		Picture = 0x10000,				// < 17th bit: Enable storage of pictures from tags (ID3v2 APIC)
		No_Peek_End = 0x20000,			// < 18th bit: Do not seek to the end of
										//   the stream in order to probe
										//   the stream length and search for the id3v1 field. This also means
										//   the file size is unknown unless set using mpg123_set_filesize() and
										//   the stream is assumed as non-seekable unless overridden
		Force_Seekable = 0x40000,		// < 19th bit: Force the stream to be seekable
		Store_Raw_Id3 = 0x80000,		// < Store raw ID3 data (even if skipping)
		Force_Endian = 0x100000,		// < Enforce endianess of output samples.
										//   This is not reflected in the format codes. If this flag is set along with
										//   MPG123_BIG_ENDIAN, MPG123_ENC_SIGNED16 means s16be, without
										//   MPG123_BIG_ENDIAN, it means s16le. Normal operation without
										//   MPG123_FORCE_ENDIAN produces output in native byte order
		Big_Endian = 0x200000,			// < Choose big endian instead of little
		No_Readahead = 0x400000,		// < Disable read-ahead in parser. If
										//   you know you provide full frames to the feeder API, this enables
										//   decoder output from the first one on, instead of having to wait for
										//   the next frame to confirm that the stream is healthy. It also disables
										//   free format support unless you provide a frame size using
										//   MPG123_FREEFORMAT_SIZE
		Float_Fallback = 0x800000,		// < Consider floating point output encoding only after
										//   trying other (possibly downsampled) rates and encodings first. This is to
										//   support efficient playback where floating point output is only configured for
										//   an external resampler, bypassing that resampler when the desired rate can
										//   be produced directly. This is enabled by default to be closer to older versions
										//   of libmpg123 which did not enable float automatically at all. If disabled,
										//   float is considered after the 16 bit default and higher-bit integer encodings
										//   for any rate
		No_Frankenstein = 0x1000000		// < Disable support for Frankenstein streams
										//   (different MPEG streams stiched together). Do not accept serious change of MPEG
										//   header inside a single stream. With this flag, the audio output format cannot
										//   change during decoding unless you open a new stream. This also stops decoding
										//   after an announced end of stream (Info header contained a number of frames
										//   and this number has been reached). This makes your MP3 files behave more like
										//   ordinary media files with defined structure, rather than stream dumps with
										//   some sugar
	}
}
