/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;

namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123
{
	/// <summary>
	/// Wrapper class to Mpg123. Note that only the API that I need are wrapped
	/// and not the whole library
	/// </summary>
	internal static class Native
	{
#if X64
		const string Mpg123Dll = @"libs\libmpg123-x64.dll";
#else
		const string Mpg123Dll = @"libs\libmpg123-x86.dll";
#endif

		/********************************************************************/
		/// <summary>
		/// Some of the methods below return a string as an IntPtr.
		/// Use this method to convert the IntPtr to a string
		/// </summary>
		/********************************************************************/
		public static string ConvertToString(IntPtr p)
		{
			return Marshal.PtrToStringAnsi(p);
		}



		/********************************************************************/
		/// <summary>
		/// Helper method to get the string of an error
		/// </summary>
		/********************************************************************/
		public static string GetErrorString(IntPtr mh, int error)
		{
			if (error == mpg123_errors.MPG123_ERR)
				return ConvertToString(mpg123_strerror(mh));

			return ConvertToString(mpg123_plain_strerror(error));
		}



		/********************************************************************/
		/// <summary>
		/// Create a handle with optional choice of decoder (named by a
		/// string, see mpg123_decoders() or mpg123_supported_decoders()) and
		/// optional retrieval of an error code to feed to
		/// mpg123_plain_strerror().
		/// Optional means: Any of or both the parameters may be NULL
		/// </summary>
		/// <param name="decoder">Optional choice of decoder variant (NULL for default)</param>
		/// <param name="error">Optional address to store error codes</param>
		/// <returns>Non-NULL pointer to fresh handle when successful</returns>
		/********************************************************************/
		[DllImport(Mpg123Dll, CharSet = CharSet.Ansi)]
		public static extern IntPtr mpg123_new([MarshalAs(UnmanagedType.LPStr)]string decoder, out int error);



		/********************************************************************/
		/// <summary>
		/// Delete handle, mh is either a valid mpg123 handle or NULL
		/// </summary>
		/// <param name="mh">Handle</param>
		/********************************************************************/
		[DllImport(Mpg123Dll)]
		public static extern void mpg123_delete(IntPtr mh);



		/********************************************************************/
		/// <summary>
		/// Look up error strings given integer code
		/// </summary>
		/// <param name="errcode">Integer error code</param>
		/// <returns>String describing what that error code means</returns>
		/********************************************************************/
		[DllImport(Mpg123Dll)]
		public static extern IntPtr mpg123_plain_strerror(int errcode);



		/********************************************************************/
		/// <summary>
		/// Give string describing what error has occurred in the context of
		/// handle mh. When a function operating on an Mpg123 handle returns
		/// MPG123_ERR, you should check for the actual reason via
		/// char *errmsg = mpg123_strerror(mh)
		/// This function will catch mh == NULL and return the message for
		/// MPG123_BAD_HANDLE
		/// </summary>
		/// <param name="mh">Handle</param>
		/// <returns>Error message</returns>
		/********************************************************************/
		[DllImport(Mpg123Dll)]
		public static extern IntPtr mpg123_strerror(IntPtr mh);



		/********************************************************************/
		/// <summary>
		/// An array of supported standard sample rates.
		/// These are possible native sample rates of MPEG audio files.
		/// You can still force Mpg123 to resample to a different one, but by
		/// default you will only get audio in one of these samplings.
		/// The list is in ascending order
		/// </summary>
		/// <param name="list">Store a pointer to the sample rates array there</param>
		/// <param name="number">Store the number of sample rates there</param>
		/********************************************************************/
		[DllImport(Mpg123Dll)]
		public static extern void mpg123_rates(out IntPtr list, out uint number);



		/********************************************************************/
		/// <summary>
		/// Configure a Mpg123 handle to accept no output format at all,
		/// use before specifying supported formats with mpg123_format
		/// </summary>
		/// <param name="mh">Handle</param>
		/// <returns>MPG123_OK on success</returns>
		/********************************************************************/
		[DllImport(Mpg123Dll)]
		public static extern int mpg123_format_none(IntPtr mh);



		/********************************************************************/
		/// <summary>
		/// Set the audio format support of a mpg123_handle in detail:
		/// </summary>
		/// <param name="mh">Handle</param>
		/// <param name="rate">The sample rate value (in Hertz)</param>
		/// <param name="channels">A combination of MPG123_STEREO and MPG123_MONO</param>
		/// <param name="encodings">A combination of accepted encodings for rate and channels,
		/// p.ex MPG123_ENC_SIGNED16 | MPG123_ENC_ULAW_8 (or 0 for no support). Please note
		/// that some encodings may not be supported in the library build and thus will be
		/// ignored here</param>
		/// <returns>MPG123_OK on success, MPG123_ERR if there was an error</returns>
		/********************************************************************/
		[DllImport(Mpg123Dll)]
		public static extern int mpg123_format(IntPtr mh, int rate, int channels, int encodings);



		/********************************************************************/
		/// <summary>
		/// Open a new bitstream and prepare for direct feeding.
		/// This works together with mpg123_decode(); you are responsible for
		/// reading and feeding the input bitstream.
		/// Also, you are expected to handle ICY metadata extraction
		/// yourself. This input method does not handle MPG123_ICY_INTERVAL.
		/// It does parse ID3 frames though
		/// </summary>
		/// <param name="mh">Handle</param>
		/// <returns>MPG123_OK on success</returns>
		/********************************************************************/
		[DllImport(Mpg123Dll)]
		public static extern int mpg123_open_feed(IntPtr mh);



		/********************************************************************/
		/// <summary>
		/// Closes the source, if libmpg123 opened it
		/// </summary>
		/// <param name="mh">Handle</param>
		/// <returns>MPG123_OK on success</returns>
		/********************************************************************/
		[DllImport(Mpg123Dll)]
		public static extern int mpg123_close(IntPtr mh);



		/********************************************************************/
		/// <summary>
		/// Read from stream and decode up to outmemsize bytes.
		/// </summary>
		/// <param name="mh">Handle</param>
		/// <param name="outmemory">Address of output buffer to write to</param>
		/// <param name="outmemsize">Maximum number of bytes to write</param>
		/// <param name="done">Address to store the number of actually decoded bytes to</param>
		/// <returns>MPG123_OK or error/message code</returns>
		/********************************************************************/
		[DllImport(Mpg123Dll)]
		public static extern int mpg123_read(IntPtr mh, IntPtr outmemory, int outmemsize, out int done);



		/********************************************************************/
		/// <summary>
		/// Feed data for a stream that has been opened with
		/// mpg123_open_feed().
		/// It's give and take: You provide the byte stream, Mpg123 gives
		/// you the decoded samples
		/// </summary>
		/// <param name="mh">Handle</param>
		/// <param name="input">Input buffer</param>
		/// <param name="size">Size number of input bytes</param>
		/// <returns>MPG123_OK or error/message code</returns>
		/********************************************************************/
		[DllImport(Mpg123Dll)]
		public static extern int mpg123_feed(IntPtr mh, IntPtr input, int size);



		/********************************************************************/
		/// <summary>
		/// Seek to desired sample offset in data feeding mode.
		/// This just prepares things to be right only if you ensure that the
		/// next chunk of input data will be from input_offset byte position
		/// </summary>
		/// <param name="mh">Handle</param>
		/// <param name="sampleoff">Offset in samples (PCM frames)</param>
		/// <param name="whence">One of SEEK_SET, SEEK_CUR or SEEK_END</param>
		/// <param name="input_offset">The position it expects to be at the next time data is fed to mpg123_decode()</param>
		/// <returns>The resulting offset >= 0 or error/message code</returns>
		/********************************************************************/
		[DllImport(Mpg123Dll)]
		public static extern int mpg123_feedseek(IntPtr mh, int sampleoff, int whence, out int input_offset);



		/********************************************************************/
		/// <summary>
		/// Get frame information about the MPEG audio bitstream and store it
		/// in a mpg123_frameinfo2 structure
		/// </summary>
		/// <param name="mh">Handle</param>
		/// <param name="mi">Address of existing frameinfo structure to write to</param>
		/// <returns>MPG123_OK on success</returns>
		/********************************************************************/
		[DllImport(Mpg123Dll)]
		public static extern int mpg123_info2(IntPtr mh, out mpg123_frameinfo2 mi);



		/********************************************************************/
		/// <summary>
		/// Return, if possible, the full (expected) length of current track
		/// in MPEG frames
		/// </summary>
		/// <param name="mh">Handle</param>
		/// <returns>length >= 0 or MPG123_ERR if there is no length guess possible</returns>
		/********************************************************************/
		[DllImport(Mpg123Dll)]
		public static extern int mpg123_framelength(IntPtr mh);



		/********************************************************************/
		/// <summary>
		/// Return, if possible, the full (expected) length of current track
		/// in samples (PCM frames).
		///
		/// This relies either on an Info frame at the beginning or a
		/// previous call to mpg123_scan() to get the real number of MPEG
		/// frames in a file. It will guess based on file size if neither
		/// Info frame nor scan data are present. In any case, there is no
		/// guarantee that the decoder will not give you more data, for
		/// example in case the open file gets appended to during decoding
		/// </summary>
		/// <param name="mh">Handle</param>
		/// <returns>length >= 0 or MPG123_ERR if there is no length guess possible</returns>
		/********************************************************************/
		[DllImport(Mpg123Dll)]
		public static extern int mpg123_length(IntPtr mh);



		/********************************************************************/
		/// <summary>
		/// Override the value for file size in bytes.
		/// Useful for getting sensible track length values in feed mode or
		/// for HTTP streams
		/// </summary>
		/// <param name="mh">Handle</param>
		/// <param name="size">File size in bytes</param>
		/// <returns>MPG123_OK on success</returns>
		/********************************************************************/
		[DllImport(Mpg123Dll)]
		public static extern int mpg123_set_filesize(IntPtr mh, int size);



		/********************************************************************/
		/// <summary>
		/// Query if there is (new) meta info, be it ID3 or ICY (or something
		/// new in future)
		/// </summary>
		/// <param name="mh">Handle</param>
		/// <returns>Combination of flags, 0 on error (same as "nothing new")</returns>
		/********************************************************************/
		[DllImport(Mpg123Dll)]
		public static extern int mpg123_meta_check(IntPtr mh);



		/********************************************************************/
		/// <summary>
		/// Point v1 and v2 to existing data structures which may change on
		/// any next read/decode function call.
		/// v1 and/or v2 can be set to NULL when there is no corresponding
		/// data
		/// </summary>
		/// <param name="mh">Handle</param>
		/// <param name="v1">Where to store pointer to ID3v1 structure</param>
		/// <param name="v2">Where to store pointer to ID3v2 structure</param>
		/// <returns>MPG123_OK on success</returns>
		/********************************************************************/
		[DllImport(Mpg123Dll)]
		public static extern int mpg123_id3(IntPtr mh, out IntPtr v1, out IntPtr v2);

		// ReSharper disable InconsistentNaming

		public const int MPG123_ID3 = 0x3;			// 0011 There is some ID3 info. Also matches 0010 or NEW_ID3
		public const int MPG123_NEW_ID3 = 0x1;		// 0001 There is ID3 info that changed since last call to mpg123_id3
		public const int MPG123_ICY = 0xc;			// 1100 There is some ICY info. Also matches 0100 or NEW_ICY
		public const int MPG123_NEW_ICY = 0x4;		// 0100 There is ICY info that changed since last call to mpg123_icy

		/// <summary>
		/// The different error codes that can be returned
		/// </summary>
		public static class mpg123_errors
		{
			public const int MPG123_DONE = -12;					// Message: Track ended. Stop decoding
			public const int MPG123_NEW_FORMAT = -11;			// Message: Output format will be different on next call. Note that some libmpg123 versions between 1.4.3 and 1.8.0 insist on you calling mpg123_getformat() after getting this message code. Newer versions behave like advertised: You have the chance to call mpg123_getformat(), but you can also just continue decoding and get your data
			public const int MPG123_NEED_MORE = -10;			// Message: For feed reader: "Feed me more!" (call mpg123_feed() or mpg123_decode() with some new input data)
			public const int MPG123_ERR = -1;					// Generic error
			public const int MPG123_OK = 0;						// Success
			public const int MPG123_BAD_OUTFORMAT = 1;			// Unable to set up output format!
			public const int MPG123_BAD_CHANNEL = 2;			// Invalid channel number specified
			public const int MPG123_BAD_RATE = 3;				// Invalid sample rate specified
			public const int MPG123_ERR_16TO8TABLE = 4;			// Unable to allocate memory for 16 to 8 converter table!
			public const int MPG123_BAD_PARAM = 5;				// Bad parameter id!
			public const int MPG123_BAD_BUFFER = 6;				// Bad buffer given -- invalid pointer or too small size
			public const int MPG123_OUT_OF_MEM = 7;				// Out of memory -- some malloc() failed
			public const int MPG123_NOT_INITIALIZED = 8;		// You didn't initialize the library!
			public const int MPG123_BAD_DECODER = 9;			// Invalid decoder choice
			public const int MPG123_BAD_HANDLE = 10;			// Invalid mpg123 handle
			public const int MPG123_NO_BUFFERS = 11;			// Unable to initialize frame buffers (out of memory?)
			public const int MPG123_BAD_RVA = 12;				// Invalid RVA mode
			public const int MPG123_NO_GAPLESS = 13;			// This build doesn't support gapless decoding
			public const int MPG123_NO_SPACE = 14;				// Not enough buffer space
			public const int MPG123_BAD_TYPES = 15;				// Incompatible numeric data types
			public const int MPG123_BAD_BAND = 16;				// Bad equalizer band
			public const int MPG123_ERR_NULL = 17;				// Null pointer given where valid storage address needed
			public const int MPG123_ERR_READER = 18;			// Error reading the stream
			public const int MPG123_NO_SEEK_FROM_END = 19;		// Cannot seek from end (end is not known)
			public const int MPG123_BAD_WHENCE = 20;			// Invalid 'whence' for seek function
			public const int MPG123_NO_TIMEOUT = 21;			// Build does not support stream timeouts
			public const int MPG123_BAD_FILE = 22;				// File access error
			public const int MPG123_NO_SEEK = 23;				// Seek not supported by stream
			public const int MPG123_NO_READER = 24;				// No stream opened
			public const int MPG123_BAD_PARS = 25;				// Bad parameter handle
			public const int MPG123_BAD_INDEX_PAR = 26;			// Bad parameters to mpg123_index() and mpg123_set_index()
			public const int MPG123_OUT_OF_SYNC = 27;			// Lost track in byte stream and did not try to resync
			public const int MPG123_RESYNC_FAIL = 28;			// Resync failed to find valid MPEG data
			public const int MPG123_NO_8BIT = 29;				// No 8-bit encoding possible
			public const int MPG123_BAD_ALIGN = 30;				// Stack alignment error
			public const int MPG123_NULL_BUFFER = 31;			// NULL input buffer with non-zero size...
			public const int MPG123_NO_RELSEEK = 32;			// Relative seek not possible (screwed up file offset)
			public const int MPG123_NULL_POINTER = 33;			// You gave a null pointer somewhere where you shouldn't have
			public const int MPG123_BAD_KEY = 34;				// Bad key value given
			public const int MPG123_NO_INDEX = 35;				// No frame index in this build
			public const int MPG123_INDEX_FAIL = 36;			// Something with frame index went wrong
			public const int MPG123_BAD_DECODER_SETUP = 37;		// Something prevents a proper decoder setup
			public const int MPG123_MISSING_FEATURE = 38;		// This feature has not been built into libmpg123
			public const int MPG123_BAD_VALUE = 39;				// A bad value has been given, somewhere
			public const int MPG123_LSEEK_FAILED = 40;			// Low-level seek failed
			public const int MPG123_BAD_CUSTOM_IO = 41;			// Custom I/O not prepared
			public const int MPG123_LFS_OVERFLOW = 42;			// Offset value overflow during translation of large file API calls -- your client program cannot handle that large file
			public const int MPG123_INT_OVERFLOW = 43;			// Some integer overflow
			public const int MPG123_BAD_FLOAT = 44;				// Floating-point computations work not as expected
		}

		/// <summary>
		/// They can be combined into one number (3) to indicate mono and stereo...
		/// </summary>
		public static class mpg123_channelcount
		{
			public const int MPG123_MONO = 1;					// Mono
			public const int MPG123_STEREO = 2;					// Stereo
		}

		/// <summary>
		/// An enum over all sample types possibly known to Mpg123.
		/// The values are designed as bit flags to allow bitmasking for encoding
		/// families.
		/// This is also why the enum is not used as type for actual encoding variables,
		/// plain integers (at least 16 bit, 15 bit being used) cover the possible
		/// combinations of these flags.
		///
		/// Note that (your build of) libmpg123 does not necessarily support all these.
		/// Usually, you can expect the 8bit encodings and signed 16 bit.
		/// Also 32bit float will be usual beginning with mpg123-1.7.0.
		/// What you should bear in mind is that (SSE, etc) optimized routines may be
		/// absent for some formats. We do have SSE for 16, 32 bit and float, though.
		/// 24 bit integer is done via postprocessing of 32 bit output -- just cutting
		/// the last byte, no rounding, even. If you want better, do it yourself.
		///
		/// All formats are in native byte order. If you need different endinaness, you
		/// can simply postprocess the output buffers (libmpg123 wouldn't do anything
		/// else). The macro MPG123_SAMPLESIZE() can be helpful there.
		/// </summary>
		public static class mpg123_enc_enum
		{
			public const int MPG123_ENC_8 = 0x00f;													// 0000 0000 0000 1111 Some 8 bit integer encoding
			public const int MPG123_ENC_16 = 0x040;													// 0000 0000 0100 0000 Some 16 bit integer encoding
			public const int MPG123_ENC_24 = 0x4000;												// 0100 0000 0000 0000 Some 24 bit integer encoding
			public const int MPG123_ENC_32 = 0x0100;												// 0000 0001 0000 0000 Some 32 bit integer encoding
			public const int MPG123_ENC_SIGNED = 0x080;												// 0000 0000 1000 0000 Some signed integer encoding
			public const int MPG123_ENC_FLOAT = 0xe00;												// 0000 1110 0000 0000 Some float encoding
			public const int MPG123_ENC_SIGNED_16 = MPG123_ENC_16 | MPG123_ENC_SIGNED | 0x10;		// 0000 0000 1101 0000 signed 16 bit
			public const int MPG123_ENC_UNSIGNED_16 = MPG123_ENC_16 | 0x20;							// 0000 0000 0110 0000 unsigned 16 bit
			public const int MPG123_ENC_UNSIGNED_8 = 0x01;											// 0000 0000 0000 0001 unsigned 8 bit
			public const int MPG123_ENC_SIGNED_8 = MPG123_ENC_SIGNED | 0x02;						// 0000 0000 1000 0010 signed 8 bit
			public const int MPG123_ENC_ULAW_8 = 0x04;												// 0000 0000 0000 0100 ulaw 8 bit
			public const int MPG123_ENC_ALAW_8 = 0x08;												// 0000 0000 0000 1000 alaw 8 bit
			public const int MPG123_ENC_SIGNED_32 = MPG123_ENC_32 | MPG123_ENC_SIGNED | 0x1000;		// 0001 0001 1000 0000 signed 32 bit
			public const int MPG123_ENC_UNSIGNED_32 = MPG123_ENC_32 | 0x2000;						// 0010 0001 0000 0000 unsigned 32 bit
			public const int MPG123_ENC_SIGNED_24 = MPG123_ENC_24 | MPG123_ENC_SIGNED | 0x1000;		// 0101 0000 1000 0000 signed 24 bit
			public const int MPG123_ENC_UNSIGNED_24 = MPG123_ENC_24 | 0x2000;						// 0110 0000 0000 0000 unsigned 24 bit
			public const int MPG123_ENC_FLOAT_32 = 0x200;											// 0000 0010 0000 0000 32 bit float
			public const int MPG123_ENC_FLOAT_64 = 0x400;											// 0000 0100 0000 0000 64 bit float
			public const int MPG123_ENC_ANY = MPG123_ENC_SIGNED_16 | MPG123_ENC_UNSIGNED_16 |		// Any possibly known encoding from the list above
			                                  MPG123_ENC_UNSIGNED_8 | MPG123_ENC_SIGNED_8 |
			                                  MPG123_ENC_ULAW_8 | MPG123_ENC_ALAW_8 |
			                                  MPG123_ENC_SIGNED_32 | MPG123_ENC_UNSIGNED_32 |
			                                  MPG123_ENC_SIGNED_24 | MPG123_ENC_UNSIGNED_24 |
			                                  MPG123_ENC_FLOAT_32 | MPG123_ENC_FLOAT_64; 
		}

		public static class mpg123_vbr
		{
			public const int MPG123_CBR = 0;				// Constant Bitrate Mode (default)
			public const int MPG123_VBR = 1;				// Variable Bitrate Mode
			public const int MPG123_ABR = 2;				// Average Bitrate Mode
		}

		/// <summary>
		/// Data structure for storing information about a frame of MPEG Audio without enums
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct mpg123_frameinfo2
		{
			public int version;								// The MPEG version (1.0/2.0/2.5), enum mpg123_version
			public int layer;								// The MPEG Layer (MP1/MP2/MP3)
			public int rate;								// The sampling rate in Hz
			public int mode;								// The audio mode (enum mpg123_mode, Mono, Stereo, Joint-Stereo, Dual Channel)
			public int mode_ext;							// The mode extension bit flag
			public int framesize;							// The size of the frame (in bytes, including header)
			public int flags;								// MPEG Audio flag bits. Bitwise combination of enum mpg123_flags values
			public int emphasis;							// The emphasis type
			public int bitrate;								// Bit rate of the frame (kbps)
			public int abr_rate;							// The target average bit rate
			public int vbr;									// The VBR mode, enum mpg123_vbr
		}

		/// <summary>
		/// Data structure for storing ID3v2 tags.
		/// This structure is not a direct binary mapping with the file contents.
		/// The ID3v2 text frames are allowed to contain multiple strings.
		/// So check for null bytes until you reach the mpg123_string fill.
		/// All text is encoded in UTF-8
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct mpg123_id3v2
		{
			public byte version;							// 3 or 4 for ID3v2.3 or ID3v2.4
			public IntPtr/*mpg123_string*/ title;			// Title string (pointer into text_list)
			public IntPtr/*mpg123_string*/ artist;			// Artist string (pointer into text_list)
			public IntPtr/*mpg123_string*/ album;			// Album string (pointer into text_list)
			public IntPtr/*mpg123_string*/ year;			// The year as a string (pointer into text_list)
			public IntPtr/*mpg123_string*/ genre;			// Genre string (pointer into text_list). The genre string(s) may very well need postprocessing, esp. for ID3v2.3
			public IntPtr/*mpg123_string*/ comment;			// Pointer to last encountered comment text with empty description

			// Encountered ID3v2 fields are appended to these lists.
			// There can be multiple occurrences, the pointers above always point to the
			// last encountered data
			public IntPtr/*mpg123_text[]*/ comment_list;	// Array of comments
			public UIntPtr comments;						// Number of comments
			public IntPtr/*mpg123_text[]*/ text;			// Array of ID3v2 text fields (including USLT)
			public UIntPtr texts;							// Number of text fields
			public IntPtr/*mpg123_text[]*/ extra;			// The array of extra (TXXX) fields
			public UIntPtr extras;							// Number of extra text (TXXX) fields
			public IntPtr picture;							// Array of ID3v2 pictures fields (APIC). Only populated if MPG123_PICTURE flag is set!
			public UIntPtr pictures;						// Number of picture (APIC) fields
		}

		/// <summary>
		/// Data structure for storing strings in a safer way than a standard C-String.
		/// Can also hold a number of null-terminated strings
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct mpg123_string
		{
			public IntPtr p;								// The string data
			public UIntPtr size;							// Raw number of bytes allocated
			public UIntPtr fill;							// Number of used bytes (including closing zero byte)
		}

		/// <summary>
		/// Data structure for storing strings in a safer way than a standard C-String.
		/// Can also hold a number of null-terminated strings
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct mpg123_text
		{
			[MarshalAs(UnmanagedType.ByValArray, ArraySubType=UnmanagedType.I1, SizeConst=3)]
			public byte[] lang;								// Three-letter language code (not terminated)
			[MarshalAs(UnmanagedType.ByValArray, ArraySubType=UnmanagedType.I1, SizeConst=4)]
			public byte[] id;								// The ID3v2 text field id, like TALB, TPE2, ... (4 characters, no string termination)
			public IntPtr/*mpg123_string*/ description;		// Empty for the generic comment...
			public IntPtr/*mpg123_string*/ text;			// ...
		}
		// ReSharper restore InconsistentNaming
	}
}
