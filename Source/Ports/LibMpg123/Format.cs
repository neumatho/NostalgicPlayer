/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Ports.LibMpg123.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123
{
	/// <summary>
	/// Routines to deal with audio (output) format
	/// </summary>
	internal class Format
	{
		private static readonly c_long[] my_Rates = new c_long[Constant.Mpg123_Rates]	// Only the standard rates
		{
			 8000, 11025, 12000,
			16000, 22050, 24000,
			32000, 44100, 48000
		};

		private static readonly Mpg123_Enc_Enum[] my_Encodings = new Mpg123_Enc_Enum[Constant.Mpg123_Encodings]
		{
			Mpg123_Enc_Enum.Enc_Signed_16,
			Mpg123_Enc_Enum.Enc_Unsigned_16,
			Mpg123_Enc_Enum.Enc_Signed_32,
			Mpg123_Enc_Enum.Enc_Unsigned_32,
			Mpg123_Enc_Enum.Enc_Signed_24,
			Mpg123_Enc_Enum.Enc_Unsigned_24,

			// Floating point range, see below
			Mpg123_Enc_Enum.Enc_Float_32,
			Mpg123_Enc_Enum.Enc_Float_64,

			// 8 bit range, see below
			Mpg123_Enc_Enum.Enc_Signed_8,
			Mpg123_Enc_Enum.Enc_Unsigned_8,
			Mpg123_Enc_Enum.Enc_Ulaw_8,
			Mpg123_Enc_Enum.Enc_Alaw_8
		};

		private static readonly c_int[] enc_Float_Range = { 6, 8 };
		private static readonly c_int[] enc_8Bit_Range = { 8, 12 };
		private static readonly c_int[] enc_24Bit_Range = { 2, 6 };
		private static readonly c_int[] enc_16Bit_Range = { 0, 2 };

		/// <summary>
		/// The list of actually possible encodings
		/// </summary>
		private static readonly Mpg123_Enc_Enum[] good_Encodings =
		{
			Mpg123_Enc_Enum.Enc_Signed_32,
			Mpg123_Enc_Enum.Enc_Unsigned_32,
			Mpg123_Enc_Enum.Enc_Signed_24,
			Mpg123_Enc_Enum.Enc_Unsigned_24
		};

		private readonly LibMpg123 lib;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Format(LibMpg123 libMpg123)
		{
			lib = libMpg123;
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
			list = my_Rates;
			number = (size_t)list.Length;
		}



		/********************************************************************/
		/// <summary>
		/// Return the size (in bytes) of one mono sample of the named
		/// encoding
		/// </summary>
		/********************************************************************/
		public c_int Mpg123_EncSize(Mpg123_Enc_Enum encoding)
		{
			return Helpers.Mpg123_SampleSize(encoding);
		}



		/********************************************************************/
		/// <summary>
		/// Match constraints against supported audio formats, store
		/// possible setup in frame.
		/// Return: -1: error; 0: no format change; 1: format change
		/// </summary>
		/********************************************************************/
		public c_int Int123_Frame_Output_Format(Mpg123_Handle fr)
		{
			AudioFormat nf = new AudioFormat();
			c_int f0 = 0;
			c_int f2 = Constant.Mpg123_Encodings + 1;	// Include all encodings by default
			Mpg123_Pars p = fr.P;
			bool try_Float = (p.Flags & Mpg123_Param_Flags.Float_Fallback) != 0 ? false : true;

			// Initialize new format, encoding comes later
			nf.Channels = fr.Stereo;

			// I intended the forcing stuff to be weaved into the format support table,
			// but this probably will never happen, as this would change library behaviour.
			// One could introduce an additional effective format table that takes for
			// forcings into account, but that would have to be updated on any flag
			// change. Tedious
			if ((p.Flags & Mpg123_Param_Flags.Force_8Bit) != 0)
			{
				f0 = enc_8Bit_Range[0];
				f2 = enc_8Bit_Range[1];
			}

			if ((p.Flags & Mpg123_Param_Flags.Force_Float) != 0)
			{
				try_Float = true;
				f0 = enc_Float_Range[0];
				f2 = enc_Float_Range[1];
			}

			// Force stereo is stronger
			if ((p.Flags & Mpg123_Param_Flags.Force_Mono) != 0)
				nf.Channels = 1;

			if ((p.Flags & Mpg123_Param_Flags.Force_Stereo) != 0)
				nf.Channels = 2;

			// Strategy update: Avoid too early triggering of the NtoM decoder.
			// Main target is the native rate, with any encoding.
			// Then, native rate with any channel count and any encoding.
			// Then, it's down_sample from native rate.
			// As last resort: NtoM rate.
			// So the priority is 1. rate 2. channels 3. encoding.
			// As encodings go, 16 bit is tranditionally preferred as efficient choice.
			// Next in line are wider float and integer encodings, then 8 bit as
			// last resort
			if (p.Force_Rate != 0)
			{
				if (Enc_Chan_Fit(p, p.Force_Rate, nf, f0, f2, try_Float))
					goto End;

				// Keep the order consistent if float is considered fallback only
				if (!try_Float && Enc_Chan_Fit(p, p.Force_Rate, nf, f0, f2, true))
					goto End;

				fr.Err = Mpg123_Errors.Bad_OutFormat;
				return -1;
			}

			// Native decoder rate first
			if (Enc_Chan_Fit(p, lib.parse.Int123_Frame_Freq(fr) >> p.Down_Sample, nf, f0, f2, try_Float))
				goto End;

			// Then downsamplings
			if (((p.Flags & Mpg123_Param_Flags.Auto_Resample) != 0) && (p.Down_Sample < 2))
			{
				if (Enc_Chan_Fit(p, lib.parse.Int123_Frame_Freq(fr) >> (p.Down_Sample + 1), nf, f0, f2, try_Float))
					goto End;

				if ((p.Down_Sample < 1) && (Enc_Chan_Fit(p, lib.parse.Int123_Frame_Freq(fr) >> 2, nf, f0, f2, try_Float)))
					goto End;
			}

			// And again the whole deal with float fallback
			if (!try_Float)
			{
				if (Enc_Chan_Fit(p, lib.parse.Int123_Frame_Freq(fr) >> p.Down_Sample, nf, f0, f2, true))
					goto End;

				// Then downsamplings
				if (((p.Flags & Mpg123_Param_Flags.Auto_Resample) != 0) && (p.Down_Sample < 2))
				{
					if (Enc_Chan_Fit(p, lib.parse.Int123_Frame_Freq(fr) >> (p.Down_Sample + 1), nf, f0, f2, true))
						goto End;

					if ((p.Down_Sample < 1) && (Enc_Chan_Fit(p, lib.parse.Int123_Frame_Freq(fr) >> 2, nf, f0, f2, true)))
						goto End;
				}
			}

			// Try to find any rate that works and resample using NtoM hackery
			if (((p.Flags & Mpg123_Param_Flags.Auto_Resample) != 0) && (fr.Down_Sample == 0))
			{
				c_int rn = Rate2Num(p, lib.parse.Int123_Frame_Freq(fr));
				if (rn < 0)
					return 0;

				// Try higher rates first
				for (c_int rrn = rn + 1; rrn < Constant.Mpg123_Rates; ++rrn)
				{
					if (Enc_Chan_Fit(p, my_Rates[rrn], nf, f0, f2, try_Float))
						goto End;
				}

				// Then lower rates
				for (c_int i = f0; i < f2; i++)
				{
					for (c_int rrn = rn - 1; rrn >= 0; --rrn)
					{
						if (Enc_Chan_Fit(p, my_Rates[rrn], nf, f0, f2, try_Float))
							goto End;
					}
				}

				// And again for float fallback
				if (!try_Float)
				{
					// Try higher rates first
					for (c_int rrn = rn + 1; rrn < Constant.Mpg123_Rates; ++rrn)
					{
						if (Enc_Chan_Fit(p, my_Rates[rrn], nf, f0, f2, true))
							goto End;
					}

					// Then lower rates
					for (c_int i = f0; i < f2; i++)
					{
						for (c_int rrn = rn - 1; rrn >= 0; --rrn)
						{
							if (Enc_Chan_Fit(p, my_Rates[rrn], nf, f0, f2, true))
								goto End;
						}
					}
				}
			}

			// Here is the _bad_ end
			fr.Err = Mpg123_Errors.Bad_OutFormat;
			return -1;

			// Here is the _good_ end
			End:
			// We had a successful match, now see if there's a chance
			if ((nf.Rate == fr.Af.Rate) && (nf.Channels == fr.Af.Channels) && (nf.Encoding == fr.Af.Encoding))
			{
				// The same format as before
				return 0;
			}
			else	// A new format
			{
				fr.Af.Rate = nf.Rate;
				fr.Af.Channels = nf.Channels;
				fr.Af.Encoding = nf.Encoding;

				// Cache the size of one sample in bytes, for ease of use
				fr.Af.EncSize = lib.Mpg123_EncSize(fr.Af.Encoding);

				if (fr.Af.EncSize < 1)
				{
					fr.Err = Mpg123_Errors.Bad_OutFormat;
					return -1;
				}

				// Set up the decoder synth format. Might differ
				switch (fr.Af.Encoding)
				{
					case Mpg123_Enc_Enum.Enc_Signed_24:
					case Mpg123_Enc_Enum.Enc_Unsigned_24:
					case Mpg123_Enc_Enum.Enc_Unsigned_32:
					{
						fr.Af.Dec_Enc = Mpg123_Enc_Enum.Enc_Signed_32;
						break;
					}

					default:
					{
						fr.Af.Dec_Enc = fr.Af.Encoding;
						break;
					}
				}

				fr.Af.Dec_EncSize = lib.Mpg123_EncSize(fr.Af.Dec_Enc);
				return 1;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Configure a mpg123 handle to accept no output format at all,
		/// use before specifying supported formats with mpg123_format
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Format_None(Mpg123_Handle mh)
		{
			if (mh == null)
				return Mpg123_Errors.Bad_Handle;

			Mpg123_Errors r = lib.Mpg123_Fmt_None(mh.P);
			if (r != Mpg123_Errors.Ok)
			{
				mh.Err = r;
				r = Mpg123_Errors.Err;
			}

			return r;
		}



		/********************************************************************/
		/// <summary>
		/// Configure mpg123 parameters to accept no output format at all,
		/// use before specifying supported formats with mpg123_format
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Fmt_None(Mpg123_Pars mp)
		{
			if (mp == null)
				return Mpg123_Errors.Bad_Pars;

			Array.Clear(mp.Audio_Caps, 0, mp.Audio_Caps.Length);

			return Mpg123_Errors.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Configure mpg123 parameters to accept all formats (also any
		/// custom rate you may set) -- this is default
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Fmt_All(Mpg123_Pars mp)
		{
			if (mp == null)
				return Mpg123_Errors.Bad_Pars;

			for (size_t ch = 0; ch < Constant.Num_Channels; ++ch)
			{
				for (size_t rate = 0; rate < Constant.Mpg123_Rates + 1; ++rate)
				{
					for (size_t enc = 0; enc < Constant.Mpg123_Encodings; ++enc)
						mp.Audio_Caps[ch, rate, enc] = (c_char)(Good_Enc(my_Encodings[enc]) ? 1 : 0);
				}
			}

			return Mpg123_Errors.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Set the audio format support of a mpg123_handle in detail
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Format(Mpg123_Handle mh, c_long rate, Mpg123_ChannelCount channels, Mpg123_Enc_Enum encodings)
		{
			if (mh == null)
				return Mpg123_Errors.Bad_Handle;

			Mpg123_Errors r = lib.Mpg123_Fmt(mh.P, rate, channels, encodings);
			if (r != Mpg123_Errors.Ok)
			{
				mh.Err = r;
				r = Mpg123_Errors.Err;
			}

			return r;
		}



		/********************************************************************/
		/// <summary>
		/// Set the audio format support of a mpg123_pars in detail
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Fmt2(Mpg123_Pars mp, c_long rate, Mpg123_ChannelCount channels, Mpg123_Enc_Enum encodings)
		{
			c_int[] ch = { 0, 1 };

			if (mp == null)
				return Mpg123_Errors.Bad_Pars;

			if ((channels & (Mpg123_ChannelCount.Mono | Mpg123_ChannelCount.Stereo)) == 0)
				return Mpg123_Errors.Bad_Channel;

			if ((channels &  Mpg123_ChannelCount.Stereo) == 0)
				ch[1] = 0;	// { 0, 0 }
			else if ((channels & Mpg123_ChannelCount.Mono) == 0)
				ch[0] = 1;	// { 1, 1 }

			c_int r1, r2;

			if (rate != 0)
			{
				r1 = Rate2Num(mp, rate);
				r2 = r1 + 1;
			}
			else
			{
				r1 = 0;
				r2 = Constant.Mpg123_Rates + 1;		// Including forced rate
			}

			if (r1 < 0)
				return Mpg123_Errors.Bad_Rate;

			// Now match the encodings
			for (c_int rateI = r1; rateI < r2; ++rateI)
			{
				for (c_int ic = 0; ic < 2; ++ic)
				{
					for (c_int ie = 0; ie < Constant.Mpg123_Encodings; ++ie)
					{
						if (Good_Enc(my_Encodings[ie]) && ((my_Encodings[ie] & encodings) == my_Encodings[ie]))
							mp.Audio_Caps[ch[ic], rateI, ie] = 1;
					}

					if (ch[0] == ch[1])
						break;		// No need to do it again
				}
			}

			return Mpg123_Errors.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Set the audio format support of a mpg123_pars in detail
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Fmt(Mpg123_Pars mp, c_long rate, Mpg123_ChannelCount channels, Mpg123_Enc_Enum encodings)
		{
			return (rate == 0) ? Mpg123_Errors.Bad_Rate : lib.Mpg123_Fmt2(mp, rate, channels, encodings);
		}



		/********************************************************************/
		/// <summary>
		/// Call this one to ensure that any valid format will be something
		/// different than this
		/// </summary>
		/********************************************************************/
		public void Int123_Invalidate_Format(AudioFormat af)
		{
			af.Encoding = 0;
			af.Rate = 0;
			af.Channels = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Number of bytes the decoder produces
		/// </summary>
		/********************************************************************/
		public int64_t Int123_Decoder_Synth_Bytes(Mpg123_Handle fr, int64_t s)
		{
			return s * fr.Af.EncSize * fr.Af.Channels;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int64_t Int123_Bytes_To_Samples(Mpg123_Handle fr, int64_t b)
		{
			return b / fr.Af.EncSize / fr.Af.Channels;
		}



		/********************************************************************/
		/// <summary>
		/// Number of bytes needed for decoding _and_ post-processing
		/// </summary>
		/********************************************************************/
		public int64_t Int123_OutBlock_Bytes(Mpg123_Handle fr, int64_t s)
		{
			c_int encSize = ((fr.Af.Encoding & Mpg123_Enc_Enum.Enc_24) != 0) ? 4 :
				(fr.Af.EncSize > fr.Af.Dec_EncSize ? fr.Af.EncSize : fr.Af.Dec_EncSize);

			return s * encSize * fr.Af.Channels;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Int123_PostProcess_Buffer(Mpg123_Handle fr)
		{
			// This caters for the final output formats that are never produced by
			// decoder synth directly (wide unsigned and 24 bit formats) or that are
			// missing because of limited decoder precision (16 bit synth but 32 or
			// 24 bit output
			switch (fr.Af.Dec_Enc)
			{
				case Mpg123_Enc_Enum.Enc_Signed_32:
				{
					switch (fr.Af.Encoding)
					{
						case Mpg123_Enc_Enum.Enc_Unsigned_32:
						{
							Conv_S32_To_U32(fr.Buffer);
							break;
						}

						case Mpg123_Enc_Enum.Enc_Unsigned_24:
						{
							Conv_S32_To_U32(fr.Buffer);
							Chop_Fourth_Byte(fr.Buffer);
							break;
						}

						case Mpg123_Enc_Enum.Enc_Signed_24:
						{
							Chop_Fourth_Byte(fr.Buffer);
							break;
						}
					}
					break;
				}
			}

			if ((fr.P.Flags & Mpg123_Param_Flags.Force_Endian) != 0)
			{
				if ((fr.P.Flags & Mpg123_Param_Flags.Big_Endian) != 0)
					Swap_Endian(fr.Buffer, lib.Mpg123_EncSize(fr.Af.Encoding));
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Rate2Num(Mpg123_Pars mp, c_long r)
		{
			for (c_int i = 0; i < Constant.Mpg123_Rates; i++)
			{
				if (my_Rates[i] == r)
					return i;
			}

			if ((mp != null) && (mp.Force_Rate != 0) && (mp.Force_Rate == r))
				 return Constant.Mpg123_Rates;

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Cap_Fit(Mpg123_Pars p, AudioFormat nf, c_int f0, c_int f2)
		{
			c_int c = nf.Channels - 1;
			c_int rn = Rate2Num(p, nf.Rate);

			if (rn >= 0)
			{
				for (c_int i = f0; i < f2; i++)
				{
					if (p.Audio_Caps[c, rn, i] != 0)
					{
						nf.Encoding = my_Encodings[i];
						return true;
					}
				}
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Find a possible encoding with given rate and channel count, try
		/// different channel count, too.
		/// This updates the given format and returns true if an encoding
		/// was found
		/// </summary>
		/********************************************************************/
		private bool Enc_Chan_Fit(Mpg123_Pars p, c_long rate, AudioFormat nnf, c_int f0, c_int f2, bool try_Float)
		{
			AudioFormat nf = nnf;
			nf.Rate = rate;

			if (Cap_Fit(p, nf, Math.Max(f0, enc_16Bit_Range[0]), Math.Min(f2, enc_16Bit_Range[1])))
				goto EEnd;

			if (Cap_Fit(p, nf, Math.Max(f0, enc_24Bit_Range[0]), Math.Min(f2, enc_24Bit_Range[1])))
				goto EEnd;
			
			if (try_Float && Cap_Fit(p, nf, Math.Max(f0, enc_Float_Range[0]), Math.Min(f2, enc_Float_Range[1])))
				goto EEnd;

			if (Cap_Fit(p, nf, Math.Max(f0, enc_8Bit_Range[0]), Math.Min(f2, enc_8Bit_Range[1])))
				goto EEnd;

			// Try again with different stereoness
			if ((nf.Channels == 2) && ((p.Flags & Mpg123_Param_Flags.Force_Stereo) == 0))
				nf.Channels = 1;
			else if ((nf.Channels == 1) && ((p.Flags & Mpg123_Param_Flags.Force_Mono) == 0))
				nf.Channels = 2;

			if (Cap_Fit(p, nf, Math.Max(f0, enc_16Bit_Range[0]), Math.Min(f2, enc_16Bit_Range[1])))
				goto EEnd;

			if (Cap_Fit(p, nf, Math.Max(f0, enc_24Bit_Range[0]), Math.Min(f2, enc_24Bit_Range[1])))
				goto EEnd;
			
			if (try_Float && Cap_Fit(p, nf, Math.Max(f0, enc_Float_Range[0]), Math.Min(f2, enc_Float_Range[1])))
				goto EEnd;

			if (Cap_Fit(p, nf, Math.Max(f0, enc_8Bit_Range[0]), Math.Min(f2, enc_8Bit_Range[1])))
				goto EEnd;

			return false;

			EEnd:
				return true;
		}



		/********************************************************************/
		/// <summary>
		/// Check if encoding is a valid one in this build.
		/// ...lazy programming: linear search
		/// </summary>
		/********************************************************************/
		private bool Good_Enc(Mpg123_Enc_Enum enc)
		{
			for (size_t i = 0; i < (size_t)good_Encodings.Length; ++i)
			{
				if (enc == good_Encodings[i])
					return true;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Remove every fourth byte, facilitating conversion from 32 bit to
		/// 24 bit integers. This has to be aware of endianness, of course
		/// </summary>
		/********************************************************************/
		private void Chop_Fourth_Byte(OutBuffer buf)
		{
			size_t wPos = 0;
			size_t rPos = 0;
			size_t blocks = buf.Fill / 4;

			for (size_t i = 0; i < blocks; ++i, wPos += 3, rPos += 4)
				Helpers.Drop4Byte(buf.Data, wPos, rPos);

			buf.Fill = wPos;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Conv_S32_To_U32(OutBuffer buf)
		{
			Span<int32_t> sSamples = MemoryMarshal.Cast<c_uchar, int32_t>(buf.Data);
			Span<uint32_t> uSamples = MemoryMarshal.Cast<c_uchar, uint32_t>(buf.Data);
			size_t count = buf.Fill / sizeof(int32_t);

			for (c_int i = 0; i < (c_int)count; ++i)
				uSamples[i] = Helpers.Conv_SU32(sSamples[i]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Swap_Endian(OutBuffer buf, c_int block)
		{
			if (block >= 2)
			{
				size_t count = buf.Fill / (uint)block;
				Swap_Bytes.Swap_Bytes_(buf.Data, (size_t)block, count);
			}
		}
		#endregion
	}
}
