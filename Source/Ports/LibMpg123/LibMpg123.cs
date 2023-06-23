/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Ports.LibMpg123.Containers;
using Single = Polycode.NostalgicPlayer.Ports.LibMpg123.Containers.Single;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123
{
	/// <summary>
	/// 
	/// </summary>
	public class LibMpg123
	{
		private static readonly string[] mpg123_Error =
		{
			"No error... (code 0)",
			"Unable to set up output format! (code 1)",
			"Invalid channel number specified. (code 2)",
			"Invalid sample rate specified. (code 3)",
			"Unable to allocate memory for 16 to 8 converter table! (code 4)",
			"Bad parameter id! (code 5)",
			"Bad buffer given -- invalid pointer or too small size. (code 6)",
			"Out of memory -- some malloc() failed. (code 7)",
			"You didn't initialize the library! (code 8)",
			"Invalid decoder choice. (code 9)",
			"Invalid mpg123 handle. (code 10)",
			"Unable to initialize frame buffers (out of memory?)! (code 11)",
			"Invalid RVA mode. (code 12)",
			"This build doesn't support gapless decoding. (code 13)",
			"Not enough buffer space. (code 14)",
			"Incompatible numeric data types. (code 15)",
			"Bad equalizer band. (code 16)",
			"Null pointer given where valid storage address needed. (code 17)",
			"Error reading the stream. (code 18)",
			"Cannot seek from end (end is not known). (code 19)",
			"Invalid 'whence' for seek function. (code 20)",
			"Build does not support stream timeouts. (code 21)",
			"File access error. (code 22)",
			"Seek not supported by stream. (code 23)",
			"No stream opened. (code 24)",
			"Bad parameter handle. (code 25)",
			"Invalid parameter addresses for index retrieval. (code 26)",
			"Lost track in the bytestream and did not attempt resync. (code 27)",
			"Failed to find valid MPEG data within limit on resync. (code 28)",
			"No 8bit encoding possible. (code 29)",
			"Stack alignment is not good. (code 30)",
			"You gave me a NULL buffer? (code 31)",
			"File position is screwed up, please do an absolute seek (code 32)",
			"Inappropriate NULL-pointer provided.",
			"Bad key value given.",
			"There is no frame index (disabled in this build).",
			"Frame index operation failed.",
			"Decoder setup failed (invalid combination of settings?)",
			"Feature not in this build.",
			"Some bad value has been provided.",
			"Low-level seeking has failed (call to lseek(), usually).",
			"Custom I/O obviously not prepared.",
			"Overflow in LFS (large file support) conversion.",
			"Overflow in integer conversion.",
			"Bad IEEE 754 rounding. Re-build libmpg123 properly."
		};

		private readonly Mpg123_Handle handle = new Mpg123_Handle();

		internal readonly Frame frame;
		internal readonly Synth synth;
		internal readonly Synth_Mono synth_Mono;
		internal readonly Synth_S32 synth_s32;
		internal readonly Synth_NToM synth_NToM;
		internal readonly Dct64 dct64;
		internal readonly Icy icy;
		internal readonly Id3 id3;
		internal readonly Format format;
		internal readonly Readers readers;
		internal readonly Index index;
		internal readonly StringBuf stringBuf;
		internal readonly TabInit tabInit;
		internal readonly Optimize optimize;
		internal readonly Parse parse;
		internal readonly GetBits getBits;
		internal readonly Layer1 layer1;
		internal readonly Layer2 layer2;
		internal readonly Layer3 layer3;
		internal readonly NToM nToM;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private LibMpg123()
		{
			frame = new Frame(this);
			synth = new Synth(this);
			synth_Mono = new Synth_Mono();
			synth_s32 = new Synth_S32(this);
			synth_NToM = new Synth_NToM(this);
			dct64 = new Dct64(this);
			icy = new Icy();
			id3 = new Id3(this);
			format = new Format(this);
			readers = new Readers(this);
			index = new Index();
			stringBuf = new StringBuf(this);
			tabInit = new TabInit();
			optimize = new Optimize(this);
			parse = new Parse(this);
			getBits = new GetBits();
			layer1 = new Layer1(this);
			layer2 = new Layer2(this);
			layer3 = new Layer3(this);
			nToM = new NToM(this);
		}



		/********************************************************************/
		/// <summary>
		/// Create a handle with optional choice of decoder (named by a
		/// string, see mpg123_decoders() or mpg123_supported_decoders()).
		/// and optional retrieval of an error code to feed to
		/// mpg123_plain_strerror().
		/// Optional means: Any of or both the parameters may be NULL
		/// </summary>
		/********************************************************************/
		public static LibMpg123 Mpg123_New(string decoder, out Mpg123_Errors error)
		{
			return Mpg123_ParNew(null, decoder, out error);
		}



		/********************************************************************/
		/// <summary>
		/// Delete handle, mh is either a valid mpg123 handle or NULL
		/// </summary>
		/********************************************************************/
		public void Mpg123_Delete()
		{
			if (handle != null)
			{
				Mpg123_Close();
				frame.Frame_Exit(handle);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set a specific parameter on a handle
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Param(Mpg123_Parms key, c_long val, c_double fVal)
		{
			Mpg123_Handle mh = handle;

			if (mh == null)
				return Mpg123_Errors.Bad_Handle;

			Mpg123_Errors r = Mpg123_Par(mh.P, key, val, fVal);
			if (r != Mpg123_Errors.Ok)
			{
				mh.Err = r;
				r = Mpg123_Errors.Err;
			}
			else
			{
				// Special treatment for some settings
				if (key == Mpg123_Parms.Index_Size)
				{
					// Apply frame index size and grow property on the fly
					r = frame.Frame_Index_Setup(mh);
					if (r != Mpg123_Errors.Ok)
						mh.Err = Mpg123_Errors.Index_Fail;
				}

				// Feeder pool size is applied right away, reader will react to that
				if ((key == Mpg123_Parms.FeedPool) || (key == Mpg123_Parms.FeedBuffer))
					readers.Bc_PoolSize(mh.RDat.Buffer, (size_t)mh.P.FeedPool, (size_t)mh.P.FeedBuffer);
			}

			return r;
		}



		/********************************************************************/
		/// <summary>
		/// Look up error strings given integer code
		/// </summary>
		/********************************************************************/
		public string Mpg123_Plain_StrError(Mpg123_Errors errCode)
		{
			if ((errCode >= 0) && ((int)errCode < mpg123_Error.Length))
				return mpg123_Error[(int)errCode];
			else
			{
				switch (errCode)
				{
					case Mpg123_Errors.Err:
						return "A generic mpg123 error.";

					case Mpg123_Errors.Done:
						return "Message: I am done with this track.";

					case Mpg123_Errors.Need_More:
						return "Message: Feed me more input data!";

					case Mpg123_Errors.New_Format:
						return "Message: Prepare for a changed audio format (query the new one)!";

					default:
						return "I have no idea - an unknown error code!";
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Give string describing what error has occurred in the context of
		/// handle mh.
		/// When a function operating on an mpg123 handle returns MPG123_ERR,
		/// you should check for the actual reason via
		///   char *errmsg = mpg123_strerror(mh)
		/// This function will catch mh == NULL and return the message for
		/// MPG123_BAD_HANDLE
		/// </summary>
		/********************************************************************/
		public string Mpg123_StrError()
		{
			return Mpg123_Plain_StrError(Mpg123_ErrCode());
		}



		/********************************************************************/
		/// <summary>
		/// Return the plain errcode instead of a string
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_ErrCode()
		{
			return handle.Err;
		}



		/********************************************************************/
		/// <summary>
		/// An array of supported standard sample rates.
		/// These are possible native sample rates of MPEG audio files.
		/// You can still force mpg123 to resample to a different one, but
		/// by default you will only get audio in one of these samplings.
		/// This list is in ascending order
		/// </summary>
		/********************************************************************/
		public void Mpg123_Rates(out c_long[] list, out size_t number)
		{
			format.Mpg123_Rates(out list, out number);
		}



		/********************************************************************/
		/// <summary>
		/// Return the size (in bytes) of one mono sample of the named
		/// encoding
		/// </summary>
		/********************************************************************/
		public c_int Mpg123_EncSize(Mpg123_Enc_Enum encoding)
		{
			return format.Mpg123_EncSize(encoding);
		}



		/********************************************************************/
		/// <summary>
		/// Configure a mpg123 handle to accept no output format at all,
		/// use before specifying supported formats with mpg123_format
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Format_None()
		{
			return format.Mpg123_Format_None(handle);
		}



		/********************************************************************/
		/// <summary>
		/// Set the audio format support of a mpg123_handle in detail
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Format(c_long rate, Mpg123_ChannelCount channels, Mpg123_Enc_Enum encodings)
		{
			return format.Mpg123_Format(handle, rate, channels, encodings);
		}



		/********************************************************************/
		/// <summary>
		/// Use an already opened stream as the bitstream input.
		/// mpg123_close() will _not_ close the stream
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Open_Fd(Stream fd)
		{
			Mpg123_Handle mh = handle;

			if (mh == null)
				return Mpg123_Errors.Bad_Handle;

			Mpg123_Close();

			return readers.Open_Stream(mh, null, fd);
		}



		/********************************************************************/
		/// <summary>
		/// Closes the source, if libmpg123 opened it
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Close()
		{
			Mpg123_Handle mh = handle;

			if (mh == null)
				return Mpg123_Errors.Bad_Handle;

			if (mh.Rd.Close != null)
				mh.Rd.Close(handle);

			if (mh.New_Format)
			{
				format.Invalidate_Format(handle.Af);
				mh.New_Format = false;
			}

			// Always reset the frame buffers on close, so we cannot forget it in funky
			// opening routines (wrappers, even)
			frame.Frame_Reset(mh);

			return Mpg123_Errors.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Read from stream and decode up to outmemsize bytes
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Read(Span<c_uchar> out_, size_t size, out size_t done)
		{
			return Mpg123_Decode(null, 0, out_, size, out done);
		}



		/********************************************************************/
		/// <summary>
		/// Feed data for a stream that has been opened with
		/// mpg123_open_feed(). It's give and take: You provide the
		/// bytestream, mpg123 gives you the decoded samples
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Feed(Span<c_uchar> in_, size_t size)
		{
			Mpg123_Handle mh = handle;

			if (mh == null)
				return Mpg123_Errors.Bad_Handle;

			if (size > 0)
			{
				if (in_ != null)
				{
					if (readers.Feed_More(mh, in_, (c_long)size) != 0)
						return Mpg123_Errors.Err;
					else
					{
						// The need for more data might have triggered an error.
						// This one is outdated now with the new data
						if (mh.Err == Mpg123_Errors.Err_Reader)
							mh.Err = Mpg123_Errors.Ok;

						return Mpg123_Errors.Ok;
					}
				}
				else
				{
					mh.Err = Mpg123_Errors.Null_Buffer;
					return Mpg123_Errors.Err;
				}
			}

			return Mpg123_Errors.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Decode MPEG Audio from inmemory to outmemory.
		/// This is very close to a drop-in replacement for old mpglib.
		/// When you give zero-sized output buffer the input will be parsed
		/// until decoded data is available. This enables you to get
		/// MPG123_NEW_FORMAT (and query it) without taking decoded data.
		/// Think of this function being the union of mpg123_read() and
		/// mpg123_feed() (which it actually is, sort of;-).
		/// You can actually always decide if you want those specialized
		/// functions in separate steps or one call this one here
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Decode(Span<c_uchar> inMemory, size_t inMemSize, Span<c_uchar> outMem, size_t outMemSize, out size_t done)
		{
			Mpg123_Handle mh = handle;

			Mpg123_Errors ret = Mpg123_Errors.Ok;
			size_t mDone = 0;
			Span<c_uchar> outMemory = outMem;
			int outIndex = 0;

			done = 0;

			if (mh == null)
				return Mpg123_Errors.Bad_Handle;

			if ((inMemSize > 0) && (Mpg123_Feed(inMemory, inMemSize) != Mpg123_Errors.Ok))
			{
				ret = Mpg123_Errors.Err;
				goto DecodeEnd;
			}

			if (outMemory.Length == 0)
				outMemSize = 0;		// Not just give error, give change to get a status message

			while (ret == Mpg123_Errors.Ok)
			{
				// Decode a frame that has been read before
				// This only happens when buffer is empty!
				if (mh.To_Decode)
				{
					if (mh.New_Format)
					{
						mh.New_Format = false;
						ret = Mpg123_Errors.New_Format;
						goto DecodeEnd;
					}

					if ((mh.Buffer.Size - mh.Buffer.Fill) < mh.OutBlock)
					{
						ret = Mpg123_Errors.No_Space;
						goto DecodeEnd;
					}

					if ((mh.Decoder_Change && (Decode_Update(mh) < 0)) || ((mh.State_Flags & Frame_State_Flags.Decoder_Live) == 0))
					{
						ret = Mpg123_Errors.Err;
						goto DecodeEnd;
					}

					Decode_The_Frame(mh);

					mh.To_Decode = mh.To_Ignore = false;
					mh.Buffer.P = mh.Buffer.Data.AsMemory();

					Helpers.Frame_BufferCheck(mh);
				}

				if (mh.Buffer.Fill != 0)	// Copy (part of) the decoded data to the caller's buffer
				{
					// Get what is needed - or just what is there
					c_int a = (c_int)(mh.Buffer.Fill > (outMemSize - mDone) ? outMemSize - mDone : mh.Buffer.Fill);
					mh.Buffer.P.Slice(0, a).Span.CopyTo(outMemory.Slice(outIndex, a));

					// Less data in frame buffer, less needed, output pointer increase, more data given...
					mh.Buffer.Fill -= (size_t)a;
					outIndex += a;
					mDone += (size_t)a;
					mh.Buffer.P = mh.Buffer.P.Slice(a);

					if (!(outMemSize > mDone))
						goto DecodeEnd;
				}
				else
				{
					// If we didn't have data, get a new frame
					int b = Get_Next_Frame(mh);
					if (b < 0)
					{
						ret = (Mpg123_Errors)b;
						goto DecodeEnd;
					}
				}
			}

			DecodeEnd:
			done = mDone;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Find, read and parse the next mp3 frame
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_FrameByFrame_Next()
		{
			// Find, read and parse the next mp3 frame while skipping junk and parsing id3 tags, lame headers, etc.
			// Prepares everything for decoding using mpg123_framebyframe_decode.
			// Returns
			// MPG123_OK -- new frame was read and parsed, call mpg123_framebyframe_decode to actually decode
			// MPG123_NEW_FORMAT -- new frame was read, it results in changed output format, call mpg123_framebyframe_decode to actually decode
			// MPG123_BAD_HANDLE -- mh has not been initialized
			// MPG123_NEED_MORE  -- more input data is needed to advance to the next frame. supply more input data using mpg123_feed
			Mpg123_Handle mh = handle;

			if (mh == null)
				return Mpg123_Errors.Bad_Handle;

			mh.To_Decode = mh.To_Ignore = false;
			mh.Buffer.Fill = 0;

			c_int b = Get_Next_Frame(mh);
			if (b < 0)
				return (Mpg123_Errors)b;

			// Mpg123_FrameByFrame_Decode will return MPG123_OK with 0 bytes decoded if mh.to_decode is 0
			if (!mh.To_Decode)
				return Mpg123_Errors.Ok;

			if (mh.New_Format)
			{
				mh.New_Format = false;
				return Mpg123_Errors.New_Format;
			}

			return Mpg123_Errors.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the current position in samples.
		/// On the next successful read, you'd get audio data with that
		/// offset
		/// </summary>
		/********************************************************************/
		public off_t Mpg123_Tell()
		{
			// Now, where are we? We need to know the last decoded frame... and what's left of it in buffer.
			// The current frame number can mean the last decoded frame or the to-be-decoded frame.
			// If mh->to_decode, then mh->num frames have been decoded, the frame mh->num now coming next.
			// If not, we have the possibility of mh->num+1 frames being decoded or nothing at all.
			// Then, there is firstframe...when we didn't reach it yet, then the next data will come from there.
			//mh->num starts with -1
			Mpg123_Handle mh = handle;

			if (mh == null)
				return (off_t)Mpg123_Errors.Err;

			if (Track_Need_Init(mh))
				return 0;

			// Now we have all the info at hand
			{
				off_t pos = 0;

				if ((mh.Num < mh.FirstFrame) || ((mh.Num == mh.FirstFrame) && mh.To_Decode))
				{
					// We are at the beginning, expect output from firstframe on
					pos = frame.Frame_Outs(mh, mh.FirstFrame);
				}
				else if (mh.To_Decode)
				{
					// We start fresh with this frame. Buffer should be empty, but we make
					// sure to count it in
					pos = frame.Frame_Outs(mh, mh.Num) - format.Bytes_To_Samples(mh, (off_t)mh.Buffer.Fill);
				}
				else
				{
					// We serve what we have in buffer and then the beginning of next frame...
					pos = frame.Frame_Outs(mh, mh.Num + 1) - format.Bytes_To_Samples(mh, (off_t)mh.Buffer.Fill);
				}

				// Subtract padding and delay from the beginning
				pos = Helpers.Sample_Adjust(mh, pos);

				// Negative sample offsets are not right, less than nothing is still nothing
				return pos > 0 ? pos : 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Seek to a desired sample offset
		/// </summary>
		/********************************************************************/
		public off_t Mpg123_Seek(off_t sampleOff, SeekOrigin whence)
		{
			Mpg123_Handle mh = handle;

			off_t pos = Mpg123_Tell();	// Adjusted samples

			// pos < 0 also can mean that simply a former seek failed at the lower levels.
			// In that case, we only allow absolute seeks
			if ((pos < 0) && (whence != SeekOrigin.Begin))
			{
				// Unless we got the obvious error of NULL handle, this is a special seek failure
				if (mh != null)
					mh.Err = Mpg123_Errors.No_RelSeek;

				return (off_t)Mpg123_Errors.Err;
			}

			c_int b = Init_Track(mh);
			if (b < 0)
				return b;

			switch (whence)
			{
				case SeekOrigin.Current:
				{
					pos += sampleOff;
					break;
				}

				case SeekOrigin.Begin:
				{
					pos = sampleOff;
					break;
				}

				case SeekOrigin.End:
				{
					// When we do not know the end already, we can try to find it
					if ((mh.Track_Frames < 1) && ((mh.RDat.Flags & ReaderFlags.Seekable) != 0))
						Mpg123_Scan();

					if (mh.Track_Frames > 0)
						pos = Helpers.Sample_Adjust(mh, frame.Frame_Outs(mh, mh.Track_Frames)) - sampleOff;
					else
					{
						mh.Err = Mpg123_Errors.No_Seek_From_End;
						return (off_t)Mpg123_Errors.Err;
					}
					break;
				}

				default:
				{
					mh.Err = Mpg123_Errors.Bad_Whence;
					return (off_t)Mpg123_Errors.Err;
				}
			}

			if (pos < 0)
				pos = 0;

			// Pos now holds the wanted sample offset in adjusted samples
			frame.Frame_Set_Seek(mh, Helpers.Sample_Unadjust(mh, pos));
			pos = Do_The_Seek(mh);

			if (pos < 0)
				return pos;

			return Mpg123_Tell();
		}



		/********************************************************************/
		/// <summary>
		/// Give access to the frame index table that is managed for seeking.
		/// You are asked not to modify the values... Use mpg123_set_index
		/// to set the seek index
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Index(out off_t[] offsets, out off_t step, out size_t fill)
		{
			offsets = null;
			step = 0;
			fill = 0;

			Mpg123_Handle mh = handle;

			if (mh == null)
				return Mpg123_Errors.Bad_Handle;

			offsets = mh.Index.Data;
			step = mh.Index.Step;
			fill = mh.Index.Fill;

			return Mpg123_Errors.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Reset the 32 Band Audio Equalizer settings to flat
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Reset_Eq()
		{
			return frame.Mpg123_Reset_Eq(handle);
		}



		/********************************************************************/
		/// <summary>
		/// Get frame information about the MPEG audio bitstream and store
		/// it in a mpg123_frameinfo structure
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Info(out Mpg123_FrameInfo mi)
		{
			mi = null;
			Mpg123_Handle mh = handle;

			if (mh == null)
				return Mpg123_Errors.Bad_Handle;

			c_int b = Init_Track(mh);
			if (b < 0)
				return (Mpg123_Errors)b;

			mi = new Mpg123_FrameInfo
			{
				Version = mh.Mpeg25 ? Mpg123_Version._2_5 : (mh.Lsf != 0) ? Mpg123_Version._2_0 : Mpg123_Version._1_0,
				Layer = mh.Lay,
				Rate = parse.Frame_Freq(mh),
				Mode_Ext = mh.Mode_Ext,
				FrameSize = mh.FrameSize + 4,	// Include header
				Emphasis = mh.Emphasis,
				BitRate = parse.Frame_BitRate(mh),
				Abr_Rate = mh.Abr_Rate,
				Vbr = mh.Vbr
			};

			switch (mh.Mode)
			{
				case Mode.Stereo:
				{
					mi.Mode = Mpg123_Mode.Stereo;
					break;
				}

				case Mode.Joint_Stereo:
				{
					mi.Mode = Mpg123_Mode.Joint;
					break;
				}

				case Mode.Dual_Channel:
				{
					mi.Mode = Mpg123_Mode.Dual;
					break;
				}

				case Mode.Mono:
				{
					mi.Mode = Mpg123_Mode.Mono;
					break;
				}

				default:
				{
					mi.Mode = 0;	// Nothing good to do here
					break;
				}
			}

			mi.Flags = 0;

			if (mh.Error_Protection)
				mi.Flags |= Mpg123_Flags.Crc;

			if (mh.Copyright)
				mi.Flags |= Mpg123_Flags.Copyright;

			if (mh.Extension)
				mi.Flags |= Mpg123_Flags.Private;

			if (mh.Original)
				mi.Flags |= Mpg123_Flags.Original;

			return Mpg123_Errors.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Make a full parsing scan of each frame in the file. ID3 tags are
		/// found. An accurate length value is stored. Seek index will be
		/// filled. A seek back to current position is performed. At all,
		/// this function refuses work when stream is not seekable
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Scan()
		{
			off_t track_Frames = 0;
			off_t track_Samples = 0;

			Mpg123_Handle mh = handle;

			if (mh == null)
				return Mpg123_Errors.Bad_Handle;

			if ((mh.RDat.Flags & ReaderFlags.Seekable) == 0)
			{
				mh.Err = Mpg123_Errors.No_Seek;
				return Mpg123_Errors.Err;
			}

			// Scan through the _whole_ file, since the current position is no count but
			// computed assuming constant samples per frame.
			// Also, we can just keep the current buffer and seek settings. Just operate
			// on input frames here
			c_int b = Init_Track(mh);
			if (b < 0)
			{
				if ((Mpg123_Errors)b == Mpg123_Errors.Done)
					return Mpg123_Errors.Ok;
				else
					return Mpg123_Errors.Err;	// Must be error here, NEED_MORE is not for seekable streams
			}

			off_t oldPos = Mpg123_Tell();

			b = mh.Rd.Seek_Frame(mh, 0);
			if ((b < 0) || (mh.Num != 0))
				return Mpg123_Errors.Err;

			// One frame must be there now
			track_Frames = 1;
			track_Samples = mh.Spf;		// Internal samples

			// Do not increment mh->track_frames in the loop as tha would confuse Frankenstein detection
			while (parse.Read_Frame(mh) == 1)
			{
				++track_Frames;
				track_Samples += mh.Spf;
			}

			mh.Track_Frames = track_Frames;
			mh.Track_Samples = track_Samples;

			return Mpg123_Seek(oldPos, SeekOrigin.Begin) >= 0 ? Mpg123_Errors.Ok : Mpg123_Errors.Err;
		}



		/********************************************************************/
		/// <summary>
		/// Return, if possible, the full (expected) length of current track
		/// in MPEG frames
		/// </summary>
		/********************************************************************/
		public off_t Mpg123_FrameLength()
		{
			Mpg123_Handle mh = handle;

			if (mh == null)
				return (off_t)Mpg123_Errors.Err;

			c_int b = Init_Track(mh);
			if (b < 0)
				return b;

			if (mh.Track_Frames > 0)
				return mh.Track_Frames;

			if (mh.RDat.FileLen > 0)
			{
				// Bad estimate. Ignoring tags 'n stuff
				c_double bpf = mh.Mean_FrameSize > 0.0 ? mh.Mean_FrameSize : parse.Compute_Bpf(mh);

				return (off_t)(mh.RDat.FileLen / bpf + 0.5);
			}

			// Last resort: No view of the future, can at least count the frames that
			// were already parsed
			if (mh.Num > -1)
				return mh.Num + 1;

			// Giving up
			return (off_t)Mpg123_Errors.Err;
		}



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
		/********************************************************************/
		public off_t Mpg123_Length()
		{
			Mpg123_Handle mh = handle;

			if (mh == null)
				return (off_t)Mpg123_Errors.Err;

			c_int b = Init_Track(mh);
			if (b < 0)
				return b;

			off_t length;

			if (mh.Track_Samples > -1)
				length = mh.Track_Samples;
			else if (mh.Track_Frames > 0)
				length = mh.Track_Frames * mh.Spf;
			else if (mh.RDat.FileLen > 0)	// Let the case of 0 length just fall through
			{
				// A bad estimate. Ignoring tags 'n stuff
				c_double bpf = mh.Mean_FrameSize != 0 ? mh.Mean_FrameSize : parse.Compute_Bpf(mh);
				length = (off_t)(mh.RDat.FileLen / bpf * mh.Spf);
			}
			else if (mh.RDat.FileLen == 0)
				return Mpg123_Tell();	// We could be in feeder mode
			else
				return (off_t)Mpg123_Errors.Err;

			length = frame.Frame_Ins2Outs(mh, length);
			length = Helpers.Sample_Adjust(mh, length);

			return length;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize an existing mpg123_string structure to {NULL, 0, 0}.
		/// If you hand in a NULL pointer here, your program should crash.
		/// The other string functions are more forgiving, but this one here
		/// is too basic
		/// </summary>
		/********************************************************************/
		public void Mpg123_Init_String(Mpg123_String sb)
		{
			stringBuf.Mpg123_Init_String(sb);
		}



		/********************************************************************/
		/// <summary>
		/// Free-up memory of the contents of an mpg123_string (not the
		/// struct itself). This also calls mpg123_init_string() and hence
		/// is safe to be called repeatedly
		/// </summary>
		/********************************************************************/
		public void Mpg123_Free_String(Mpg123_String sb)
		{
			stringBuf.Mpg123_Free_String(sb);
		}



		/********************************************************************/
		/// <summary>
		/// Change the size of a mpg123_string
		/// </summary>
		/********************************************************************/
		public int Mpg123_Resize_String(Mpg123_String sb, size_t new_)
		{
			return stringBuf.Mpg123_Resize_String(sb, new_);
		}



		/********************************************************************/
		/// <summary>
		/// Increase size of a mpg123_string if necessary (it may stay
		/// larger). Note that the functions for adding and setting in
		/// current libmpg123 use this instead of mpg123_resize_string().
		/// That way, you can preallocate memory and safely work afterwards
		/// with pieces
		/// </summary>
		/********************************************************************/
		public c_int Mpg123_Grow_String(Mpg123_String sb, size_t news)
		{
			return stringBuf.Mpg123_Grow_String(sb, news);
		}



		/********************************************************************/
		/// <summary>
		/// Move the contents of one mpg123_string string to another.
		/// This frees any memory associated with the target and moves over
		/// the pointers from the source, leaving the source without content
		/// after that. The only possible error is that you hand in NULL
		/// pointers. If you handed in a valid source, its contents will be
		/// gone, even if there was no target to move to. If you hand in a
		/// valid target, its original contents will also always be gone, to
		/// be replaced with the source's contents if there was some
		/// </summary>
		/********************************************************************/
		public c_int Mpg123_Move_String(Mpg123_String from, ref Mpg123_String to)
		{
			return stringBuf.Mpg123_Move_String(from, ref to);
		}



		/********************************************************************/
		/// <summary>
		/// Determine if two strings contain the same data.
		/// This only returns 1 if both given handles are non-NULL and if
		/// they are filled with the same bytes
		/// </summary>
		/********************************************************************/
		public c_int Mpg123_Same_String(Mpg123_String a, Mpg123_String b)
		{
			return stringBuf.Mpg123_Same_String(a, b);
		}



		/********************************************************************/
		/// <summary>
		/// Point v1 and v2 to existing data structures wich may change on
		/// any next read/decode function call. v1 and/or v2 can be set to
		/// NULL when there is no corresponding data
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Id3(out Mpg123_Id3V1 v1, out Mpg123_Id3V2 v2)
		{
			Mpg123_Handle mh = handle;

			v1 = null;
			v2 = null;

			if (mh == null)
				return Mpg123_Errors.Bad_Handle;

			if ((mh.MetaFlags & Mpg123_MetaFlags.Id3) != 0)
			{
				id3.Id3_Link(mh);

				if ((mh.RDat.Flags & ReaderFlags.Id3Tag) != 0)
				{
					v1 = new Mpg123_Id3V1();

					Array.Copy(mh.Id3Buf, 0, v1.Tag, 0, 3);
					Array.Copy(mh.Id3Buf, 3, v1.Title, 0, 30);
					Array.Copy(mh.Id3Buf, 33, v1.Artist, 0, 30);
					Array.Copy(mh.Id3Buf, 63, v1.Album, 0, 30);
					Array.Copy(mh.Id3Buf, 93, v1.Year, 0, 4);
					Array.Copy(mh.Id3Buf, 97, v1.Comment, 0, 30);
					v1.Genre = mh.Id3Buf[127];
				}

				if (mh.Id3V2.Version != 0)
					v2 = mh.Id3V2;

				mh.MetaFlags |= Mpg123_MetaFlags.Id3;
				mh.MetaFlags &= ~Mpg123_MetaFlags.New_Id3;
			}

			return Mpg123_Errors.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Create a handle with preset parameters
		/// </summary>
		/********************************************************************/
		public static LibMpg123 Mpg123_ParNew(Mpg123_Pars mp, string decoder, out Mpg123_Errors error)
		{
			Mpg123_Errors err = Mpg123_Errors.Ok;

			LibMpg123 lib = new LibMpg123();
			Mpg123_Handle fr = lib.handle;

			lib.frame.Frame_Init_Par(fr, mp);
			if (!lib.optimize.Frame_Cpu_Opt(fr, decoder))
			{
				err = Mpg123_Errors.Bad_Decoder;
				lib.frame.Frame_Exit(fr);
				fr = null;
			}

			if (fr != null)
				fr.Decoder_Change = true;
			else if (err == Mpg123_Errors.Ok)
				err = Mpg123_Errors.Out_Of_Mem;

			error = err;

			return lib;
		}



		/********************************************************************/
		/// <summary>
		/// Configure mpg123 parameters to accept no output format at all,
		/// use before specifying supported formats with mpg123_format
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Fmt_None(Mpg123_Pars mp)
		{
			return format.Mpg123_Fmt_None(mp);
		}



		/********************************************************************/
		/// <summary>
		/// Configure mpg123 parameters to accept all formats (also any
		/// custom rate you may set) -- this is default
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Fmt_All(Mpg123_Pars mp)
		{
			return format.Mpg123_Fmt_All(mp);
		}



		/********************************************************************/
		/// <summary>
		/// Set the audio format support of a mpg123_pars in detail
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Fmt(Mpg123_Pars mp, c_long rate, Mpg123_ChannelCount channels, Mpg123_Enc_Enum encodings)
		{
			return format.Mpg123_Fmt(mp, rate, channels, encodings);
		}



		/********************************************************************/
		/// <summary>
		/// Set the audio format support of a mpg123_pars in detail
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Fmt2(Mpg123_Pars mp, c_long rate, Mpg123_ChannelCount channels, Mpg123_Enc_Enum encodings)
		{
			return format.Mpg123_Fmt2(mp, rate, channels, encodings);
		}



		/********************************************************************/
		/// <summary>
		/// Set a specific parameter in a par handle
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Par(Mpg123_Pars mp, Mpg123_Parms key, c_long val, c_double fVal)
		{
			Mpg123_Errors ret = Mpg123_Errors.Ok;

			if (mp == null)
				return Mpg123_Errors.Bad_Pars;

			switch (key)
			{
				case Mpg123_Parms.Verbose:
				{
					mp.Verbose = val;
					break;
				}

				case Mpg123_Parms.Flags:
				{
					if (((Mpg123_Param_Flags)val & Mpg123_Param_Flags.Gapless) != 0)
						ret = Mpg123_Errors.No_Gapless;

					if (ret == Mpg123_Errors.Ok)
						mp.Flags = (Mpg123_Param_Flags)val;

					break;
				}

				case Mpg123_Parms.Add_Flags:
				{
					// Enabling of gapless mode doesn't work when it's not there, but
					// disabling (below) is no problem
					if (((Mpg123_Param_Flags)val & Mpg123_Param_Flags.Gapless) != 0)
						ret = Mpg123_Errors.No_Gapless;
					else
						mp.Flags |= (Mpg123_Param_Flags)val;

					break;
				}

				case Mpg123_Parms.Remove_Flags:
				{
					mp.Flags &= ~(Mpg123_Param_Flags)val;
					break;
				}

				case Mpg123_Parms.Force_Rate:
				{
					if (val > 96000)
						ret = Mpg123_Errors.Bad_Rate;
					else
						mp.Force_Rate = val < 0 ? 0 : val;	// > 0 means enable, 0 disable

					break;
				}

				case Mpg123_Parms.Down_Sample:
				{
					if ((val < 0) || (val > 2))
						ret = Mpg123_Errors.Bad_Rate;
					else
						mp.Down_Sample = val;

					break;
				}

				case Mpg123_Parms.Rva:
				{
					if ((val < 0) || ((Mpg123_Param_Rva)val > Mpg123_Param_Rva.Rva_Max))
						ret = Mpg123_Errors.Bad_Rva;
					else
						mp.Rva = (Mpg123_Param_Rva)val;

					break;
				}

				case Mpg123_Parms.DownSpeed:
				{
					mp.HalfSpeed = val < 0 ? 0 : val;
					break;
				}

				case Mpg123_Parms.UpSpeed:
				{
					mp.DoubleSpeed = val < 0 ? 0 : val;
					break;
				}

				case Mpg123_Parms.Icy_Interval:
				{
					mp.Icy_Interval = val > 0 ? val : 0;
					break;
				}

				case Mpg123_Parms.Outscale:
				{
					// Choose the value that is non-zero, if any.
					// Downscaling integers to 1.0
					mp.OutScale = val == 0 ? fVal : (c_double)val / Constant.Short_Scale;
					break;
				}

				case Mpg123_Parms.Timeout:
				{
					if (val > 0)
						ret = Mpg123_Errors.No_Timeout;

					break;
				}

				case Mpg123_Parms.Resync_Limit:
				{
					mp.Resync_Limit = val;
					break;
				}

				case Mpg123_Parms.Index_Size:
				{
					mp.Index_Size = val;
					break;
				}

				case Mpg123_Parms.Preframes:
				{
					if (val >= 0)
						mp.PreFrames = val;
					else
						ret = Mpg123_Errors.Bad_Value;

					break;
				}

				case Mpg123_Parms.FeedPool:
				{
					if (val >= 0)
						mp.FeedPool = val;
					else
						ret = Mpg123_Errors.Bad_Value;

					break;
				}

				case Mpg123_Parms.FeedBuffer:
				{
					if (val > 0)
						mp.FeedBuffer = val;
					else
						ret = Mpg123_Errors.Bad_Value;

					break;
				}

				case Mpg123_Parms.FreeFormat_Size:
				{
					mp.FreeFormat_FrameSize = val;
					break;
				}

				default:
				{
					ret = Mpg123_Errors.Bad_Param;
					break;
				}
			}

			return ret;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private off_t SeekFrame(Mpg123_Handle mh)
		{
			return mh.IgnoreFrame < 0 ? 0 : mh.IgnoreFrame;
		}



		/********************************************************************/
		/// <summary>
		/// Update decoding engine for
		/// a) a new choice of decoder
		/// b) a changed native format of the MPEG stream
		/// ... calls are only valid after parsing some MPEG frame!
		/// </summary>
		/********************************************************************/
		private c_int Decode_Update(Mpg123_Handle mh)
		{
			mh.State_Flags &= ~Frame_State_Flags.Decoder_Live;

			if (mh.Num < 0)
			{
				mh.Err = Mpg123_Errors.Bad_Decoder_Setup;
				return (c_int)Mpg123_Errors.Err;
			}

			mh.State_Flags |= Frame_State_Flags.Fresh_Decoder;
			c_long native_Rate = parse.Frame_Freq(mh);

			c_int b = format.Frame_Output_Format(mh);	// Select the new output format based on given constraints
			if (b < 0)
				return (c_int)Mpg123_Errors.Err;

			if (b == 1)
				mh.New_Format = true;	// Store for later

			if (mh.Af.Rate == native_Rate)
				mh.Down_Sample = 0;
			else if (mh.Af.Rate == (native_Rate >> 1))
				mh.Down_Sample = 1;
			else if (mh.Af.Rate == (native_Rate >> 2))
				mh.Down_Sample = 2;
			else
				mh.Down_Sample = 3;	// Flexible (fixed) rate

			switch (mh.Down_Sample)
			{
				case 0:
				case 1:
				case 2:
				{
					mh.Down_Sample_SbLimit = Constant.SBLimit >> mh.Down_Sample;

					// With downsampling I get less samples per frame
					mh.OutBlock = (size_t)format.OutBlock_Bytes(mh, mh.Spf >> mh.Down_Sample);
					break;
				}

				case 3:
				{
					if (nToM.Synth_NToM_Set_Step(mh) != 0)
						return -1;

					if (parse.Frame_Freq(mh) > mh.Af.Rate)
					{
						mh.Down_Sample_SbLimit = Constant.SBLimit * mh.Af.Rate;
						mh.Down_Sample_SbLimit /= parse.Frame_Freq(mh);

						if (mh.Down_Sample_SbLimit < 1)
							mh.Down_Sample_SbLimit = 1;
					}
					else
						mh.Down_Sample_SbLimit = Constant.SBLimit;

					mh.OutBlock = (size_t)format.OutBlock_Bytes(mh, ((Constant.NToM_Mul - 1 + mh.Spf * ((Constant.NToM_Mul * mh.Af.Rate) / parse.Frame_Freq(mh))) / Constant.NToM_Mul));
					break;
				}
			}

			if ((mh.P.Flags & Mpg123_Param_Flags.Force_Mono) == 0)
			{
				if (mh.Af.Channels == 1)
					mh.Single = Single.Mix;
				else
					mh.Single = Single.Stereo;
			}
			else
				mh.Single = (Single)((mh.P.Flags & Mpg123_Param_Flags.Force_Mono) - 1);

			if (optimize.Set_Synth_Functions(mh) != 0)
				return -1;

			// The needed size of output buffer may have changed
			if (frame.Frame_OutBuffer(mh) != Mpg123_Errors.Ok)
				return -1;

			frame.Do_Rva(mh);

			mh.Decoder_Change = false;
			mh.State_Flags |= Frame_State_Flags.Decoder_Live;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Read in the next frame we actually want for decoding.
		/// This includes skipping/ignoring frames, in addition to skipping
		/// junk in the parser
		/// </summary>
		/********************************************************************/
		private c_int Get_Next_Frame(Mpg123_Handle mh)
		{
			bool change = mh.Decoder_Change;

			// Ensure we got proper decoder for ignoring frames.
			// Header can be changed from seeking around. But be careful: Only after at
			// least one frame got read, decoder update makes sense
			if ((mh.Header_Change > 1) && (mh.Num >= 0))
			{
				change = true;
				mh.Header_Change = 0;

				if (Decode_Update(mh) < 0)
					return (c_int)Mpg123_Errors.Err;
			}

			do
			{
				// Decode & discard some frame(s) before beginning
				if (mh.To_Ignore && (mh.Num < mh.FirstFrame) && (mh.Num >= mh.IgnoreFrame))
				{
					// Decoder structure must be current! decode_update has been called before...
					mh.Do_Layer(mh);
					mh.Buffer.Fill = 0;

					// The ignored decoding may have failed. Make sure ntom stays consistent
					if (mh.Down_Sample == 3)
						nToM.NToM_Set_NToM(mh, mh.Num + 1);

					mh.To_Ignore = mh.To_Decode = false;
				}

				// Read new frame data; possibly breaking out here for MPG123_NEED_MORE
				mh.To_Decode = false;

				c_int b = parse.Read_Frame(mh);	// That sets to_decode only if a full frame was read
				if (b == (c_int)Mpg123_Errors.Need_More)
					return (c_int)Mpg123_Errors.Need_More;	// Need another call with data
				else if (b <= 0)
				{
					// More sophisticated error control?
					if ((b == 0) || ((mh.RDat.FileLen >= 0) && (mh.RDat.FilePos == mh.RDat.FileLen)))
					{
						// We simply reached the end
						mh.Track_Frames = mh.Num + 1;
						return (c_int)Mpg123_Errors.Done;
					}
					else
						return (c_int)Mpg123_Errors.Err;	// Some real error
				}

				// Now, there should be new data to decode ... and also possibly new stream properties
				if ((mh.Header_Change > 1) || mh.Decoder_Change)
				{
					change = true;
					mh.Header_Change = 0;

					// Need to update decoder structure right away since frame might need to
					// be decoded on next loop iteration for properly ignoring its output
					if (Decode_Update(mh) < 0)
						return (c_int)Mpg123_Errors.Err;
				}

				// Now some accounting: Look at the numbers and decide if we want this frame
				++mh.PlayNum;

				// Plain skipping without decoding, only when frame is not ignored on next cycle
				if ((mh.Num < mh.FirstFrame) || ((mh.P.DoubleSpeed != 0) && ((mh.PlayNum % mh.P.DoubleSpeed) != 0)))
				{
					if (!(mh.To_Ignore && (mh.Num < mh.FirstFrame) && (mh.Num >= mh.IgnoreFrame)))
					{
						frame.Frame_Skip(mh);

						// Should one fix NtoM here or not?
						// It is not work the trouble for doublespeed, but what with leading frames?
					}
				}
				else
					break;	// Or, we are finally done and have a new frame
			}
			while (true);

			// If we reach this point, we got a new frame ready to be decoded.
			// All other situations resulted in returns from the loop
			if (change)
			{
				if (mh.Fresh)
					mh.Fresh = false;
			}

			return (c_int)Mpg123_Errors.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Assumption: A buffer full of zero samples can be constructed by
		/// repetition of this byte. Oh, and it handles some format
		/// conversions. Only to be used by decode_the_frame()
		/// </summary>
		/********************************************************************/
		private c_uchar Zero_Byte(Mpg123_Handle fr)
		{
			return 0;	// All normal signed formats have the zero here (even in byte form -- that may be an assumption for your funny machine...)
		}



		/********************************************************************/
		/// <summary>
		/// Not part of the api. This just decodes the frame and fills
		/// missing bits with zeroes.
		/// There can be frames that are broken and thus make do_layer() fail
		/// </summary>
		/********************************************************************/
		private void Decode_The_Frame(Mpg123_Handle fr)
		{
			size_t needed_Bytes = (size_t)format.Decoder_Synth_Bytes(fr, frame.Frame_Expect_OutSamples(fr));
			fr.Clip += fr.Do_Layer(fr);

			if (fr.Buffer.Fill < needed_Bytes)
			{
				// One could do a loop with individual samples instead... but zero is zero.
				// Actually, that is wrong: zero is mostly a series of null bytes,
				// but we have funny 8bit formats that have a different opinion on zero...
				// Unsigned 16 or 32 bit formats are handled later
				Array.Fill(fr.Buffer.Data, Zero_Byte(fr), (int)fr.Buffer.Fill, (int)(needed_Bytes - fr.Buffer.Fill));

				fr.Buffer.Fill = needed_Bytes;

				// ntom_val will be wrong when the decoding wasn't carried out completely
				nToM.NToM_Set_NToM(fr, fr.Num + 1);
			}

			format.PostProcess_Buffer(fr);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Track_Need_Init(Mpg123_Handle mh)
		{
			// Simple: Track needs initialization if no initial frame has been read yet
			return mh.Num < 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Init_Track(Mpg123_Handle mh)
		{
			if (Track_Need_Init(mh))
			{
				// Fresh track, need first frame for basic info
				c_int b = Get_Next_Frame(mh);
				if (b < 0)
					return b;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Do_The_Seek(Mpg123_Handle mh)
		{
			off_t fNum = SeekFrame(mh);
			mh.Buffer.Fill = 0;

			// If we are inside the ignoreframe - firstframe window, we may get away
			// without actual seeking
			if (mh.Num < mh.FirstFrame)
			{
				mh.To_Decode = false;	// In any case, don't decode the current frame, perhaps ignore instead

				if (mh.Num > fNum)
					return (c_int)Mpg123_Errors.Ok;
			}

			// If we are already there, we are fine either for decoding or for ignoring
			if ((mh.Num == fNum) && (mh.To_Decode || (fNum < mh.FirstFrame)))
				return (c_int)Mpg123_Errors.Ok;

			// We have the frame before... just go ahead as normal
			if (mh.Num == (fNum - 1))
			{
				mh.To_Decode = false;
				return (c_int)Mpg123_Errors.Ok;
			}

			// Ok, real seeking follows... clear buffers and go for it
			frame.Frame_Buffers_Reset(mh);

			if (mh.Down_Sample == 3)
				nToM.NToM_Set_NToM(mh, fNum);

			c_int b = mh.Rd.Seek_Frame(mh, fNum);

			if (mh.Header_Change > 1)
			{
				if (Decode_Update(mh) < 0)
					return (c_int)Mpg123_Errors.Err;

				mh.Header_Change = 0;
			}

			if (b < 0)
				return b;

			// Only mh->to_ignore is true
			if (mh.Num < mh.FirstFrame)
				mh.To_Decode = false;

			mh.PlayNum = mh.Num;

			return 0;
		}
		#endregion
	}
}
