/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// 
	/// </summary>
	public static class Utils_Codec
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool Av_Codec_Is_Encoder(AvCodec avCodec)//XX 79
		{
			FFCodec codec = Codec_Internal.FFCodec(avCodec);

			return (codec != null) && !codec.Is_Decoder;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool Av_Codec_Is_Decoder(AvCodec avCodec)//XX 85
		{
			FFCodec codec = Codec_Internal.FFCodec(avCodec);

			return (codec != null) && codec.Is_Decoder;
		}



		/********************************************************************/
		/// <summary>
		/// Check that the provided frame dimensions are valid and set them
		/// on the codec context
		/// </summary>
		/********************************************************************/
		public static c_int FF_Set_Dimensions(AvCodecContext s, c_int width, c_int height)//XX 91
		{
			c_int ret = ImgUtils.Av_Image_Check_Size2((c_uint)width, (c_uint)height, s.Max_Pixels, AvPixelFormat.None, 0, s);

			if (ret < 0)
				width = height = 0;

			s.Coded_Width = width;
			s.Coded_Height = height;
			s.PictureSize.Width = Common.Av_Ceil_RShift(width, s.LowRes);
			s.PictureSize.Height = Common.Av_Ceil_RShift(height, s.LowRes);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Modify width and height values so that they will result in a
		/// memory buffer that is acceptable for the codec if you also ensure
		/// that all line sizes are a multiple of the respective
		/// linesize_align[i].
		///
		/// May only be used if a codec with AV_CODEC_CAP_DR1 has been opened
		/// </summary>
		/********************************************************************/
		public static void AvCodec_Align_Dimensions2(AvCodecContext s, ref c_int width, ref c_int height, CPointer<c_int> lineSize_Align)//XX 141
		{
			c_int w_Align = 1;
			c_int h_Align = 1;
			AVPixFmtDescriptor desc = PixDesc.Av_Pix_Fmt_Desc_Get(s.Pix_Fmt);

			if (desc != null)
			{
				w_Align = 1 << desc.Log2_Chroma_W;
				h_Align = 1 << desc.Log2_Chroma_H;
			}

			switch (s.Pix_Fmt)
			{
				case AvPixelFormat.YUV420P:
				case AvPixelFormat.YUYV422:
				case AvPixelFormat.YVYU422:
				case AvPixelFormat.UYVY422:
				case AvPixelFormat.YUV422P:
				case AvPixelFormat.YUV440P:
				case AvPixelFormat.YUV444P:
				case AvPixelFormat.GBRP:
				case AvPixelFormat.GBRAP:
				case AvPixelFormat.GRAY8:
				case AvPixelFormat.GRAY16BE:
				case AvPixelFormat.GRAY16LE:
				case AvPixelFormat.YUVJ420P:
				case AvPixelFormat.YUVJ422P:
				case AvPixelFormat.YUVJ440P:
				case AvPixelFormat.YUVJ444P:
				case AvPixelFormat.YUVA420P:
				case AvPixelFormat.YUVA422P:
				case AvPixelFormat.YUVA444P:
				case AvPixelFormat.YUV420P9LE:
				case AvPixelFormat.YUV420P9BE:
				case AvPixelFormat.YUV420P10LE:
				case AvPixelFormat.YUV420P10BE:
				case AvPixelFormat.YUV420P12LE:
				case AvPixelFormat.YUV420P12BE:
				case AvPixelFormat.YUV420P14LE:
				case AvPixelFormat.YUV420P14BE:
				case AvPixelFormat.YUV420P16LE:
				case AvPixelFormat.YUV420P16BE:
				case AvPixelFormat.YUVA420P9LE:
				case AvPixelFormat.YUVA420P9BE:
				case AvPixelFormat.YUVA420P10LE:
				case AvPixelFormat.YUVA420P10BE:
				case AvPixelFormat.YUVA420P16LE:
				case AvPixelFormat.YUVA420P16BE:
				case AvPixelFormat.YUV422P9LE:
				case AvPixelFormat.YUV422P9BE:
				case AvPixelFormat.YUV422P10LE:
				case AvPixelFormat.YUV422P10BE:
				case AvPixelFormat.YUV422P12LE:
				case AvPixelFormat.YUV422P12BE:
				case AvPixelFormat.YUV422P14LE:
				case AvPixelFormat.YUV422P14BE:
				case AvPixelFormat.YUV422P16LE:
				case AvPixelFormat.YUV422P16BE:
				case AvPixelFormat.YUVA422P9LE:
				case AvPixelFormat.YUVA422P9BE:
				case AvPixelFormat.YUVA422P10LE:
				case AvPixelFormat.YUVA422P10BE:
				case AvPixelFormat.YUVA422P12LE:
				case AvPixelFormat.YUVA422P12BE:
				case AvPixelFormat.YUVA422P16LE:
				case AvPixelFormat.YUVA422P16BE:
				case AvPixelFormat.YUV440P10LE:
				case AvPixelFormat.YUV440P10BE:
				case AvPixelFormat.YUV440P12LE:
				case AvPixelFormat.YUV440P12BE:
				case AvPixelFormat.YUV444P9LE:
				case AvPixelFormat.YUV444P9BE:
				case AvPixelFormat.YUV444P10LE:
				case AvPixelFormat.YUV444P10BE:
				case AvPixelFormat.YUV444P12LE:
				case AvPixelFormat.YUV444P12BE:
				case AvPixelFormat.YUV444P14LE:
				case AvPixelFormat.YUV444P14BE:
				case AvPixelFormat.YUV444P16LE:
				case AvPixelFormat.YUV444P16BE:
				case AvPixelFormat.YUVA444P9LE:
				case AvPixelFormat.YUVA444P9BE:
				case AvPixelFormat.YUVA444P10LE:
				case AvPixelFormat.YUVA444P10BE:
				case AvPixelFormat.YUVA444P12LE:
				case AvPixelFormat.YUVA444P12BE:
				case AvPixelFormat.YUVA444P16LE:
				case AvPixelFormat.YUVA444P16BE:
				case AvPixelFormat.GBRP9LE:
				case AvPixelFormat.GBRP9BE:
				case AvPixelFormat.GBRP10LE:
				case AvPixelFormat.GBRP10BE:
				case AvPixelFormat.GBRP12LE:
				case AvPixelFormat.GBRP12BE:
				case AvPixelFormat.GBRP14LE:
				case AvPixelFormat.GBRP14BE:
				case AvPixelFormat.GBRP16LE:
				case AvPixelFormat.GBRP16BE:
				case AvPixelFormat.GBRAP12LE:
				case AvPixelFormat.GBRAP12BE:
				case AvPixelFormat.GBRAP16LE:
				case AvPixelFormat.GBRAP16BE:
				{
					w_Align = 16;		// FIXME assume 16 pixel per macroblock
					h_Align = 16 * 2;	// Interlaced needs 2 macroblocks height

					if (s.Codec_Id == AvCodecId.BinkVideo)
						w_Align = 16 * 2;

					break;
				}

				case AvPixelFormat.YUV411P:
				case AvPixelFormat.YUVJ411P:
				case AvPixelFormat.UYYVYY411:
				{
					w_Align = 32;
					h_Align = 16 * 2;
					break;
				}

				case AvPixelFormat.YUV410P:
				{
					if (s.Codec_Id == AvCodecId.Svq1)
					{
						w_Align = 64;
						h_Align = 64;
					}
					else if (s.Codec_Id == AvCodecId.Snow)
					{
						w_Align = 16;
						h_Align = 16;
					}

					break;
				}

				case AvPixelFormat.RGB555BE:
				case AvPixelFormat.RGB555LE:
				{
					if (s.Codec_Id == AvCodecId.Rpza)
					{
						w_Align = 4;
						h_Align = 4;
					}

					if (s.Codec_Id == AvCodecId.Interplay_Video)
					{
						w_Align = 8;
						h_Align = 8;
					}

					break;
				}

				case AvPixelFormat.PAL8:
				case AvPixelFormat.BGR8:
				case AvPixelFormat.RGB8:
				{
					if ((s.Codec_Id == AvCodecId.Smc) || (s.Codec_Id == AvCodecId.Cinepak))
					{
						w_Align = 4;
						h_Align = 4;
					}

					if ((s.Codec_Id == AvCodecId.Jv) || (s.Codec_Id == AvCodecId.Argo) || (s.Codec_Id == AvCodecId.Interplay_Video))
					{
						w_Align = 8;
						h_Align = 8;
					}

					if ((s.Codec_Id == AvCodecId.MJpeg) || (s.Codec_Id == AvCodecId.MJpegB) || (s.Codec_Id == AvCodecId.LJpeg) ||
						(s.Codec_Id == AvCodecId.SmvJpeg) || (s.Codec_Id == AvCodecId.Amv) || (s.Codec_Id == AvCodecId.SP5X) || (s.Codec_Id == AvCodecId.JpegLs))
					{
						w_Align = 8;
						h_Align = 2 * 8;
					}

					break;
				}

				case AvPixelFormat.BGR24:
				{
					if ((s.Codec_Id == AvCodecId.Mszh) || (s.Codec_Id == AvCodecId.ZLib))
					{
						w_Align = 4;
						h_Align = 4;
					}

					break;
				}

				case AvPixelFormat.RGB24:
				{
					if (s.Codec_Id == AvCodecId.Cinepak)
					{
						w_Align = 4;
						h_Align = 4;
					}

					break;
				}

				case AvPixelFormat.BGR0:
				{
					if (s.Codec_Id == AvCodecId.Argo)
					{
						w_Align = 8;
						h_Align = 8;
					}

					break;
				}
			}

			if (s.Codec_Id == AvCodecId.Iff_Ilbm)
				w_Align = Macros.FFMax(w_Align, 16);

			width = Macros.FFAlign(width, w_Align);
			height = Macros.FFAlign(height, h_Align);

			if ((s.Codec_Id == AvCodecId.H264) || (s.LowRes != 0) || (s.Codec_Id == AvCodecId.Vc1) || (s.Codec_Id == AvCodecId.Wmv3) ||
			    (s.Codec_Id == AvCodecId.Vp5) || (s.Codec_Id == AvCodecId.Vp6) || (s.Codec_Id == AvCodecId.Vp6F) || (s.Codec_Id == AvCodecId.Vp6A))
			{
				// Some of the optimized chroma MC reads one line too much
				// which is also done in mpeg decoders with lowres > 0
				height += 2;

				// H.264 uses edge emulation for out of frame motion vectors, for this
				// it requires a temporary area large enough to hold a 21x21 block,
				// increasing width ensure that the temporary area is large enough,
				// the next rounded up width is 32
				width = Macros.FFMax(width, 32);
			}

			if (s.Codec_Id == AvCodecId.Svq3)
				width = Macros.FFMax(width, 32);

			for (c_int i = 0; i < 4; i++)
				lineSize_Align[i] = CodecConstants.Stride_Align;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int AvPriv_Codec_Get_Cap_Skip_Frame_Fill_Param(AvCodec codec)//XX 402
		{
			return (Codec_Internal.FFCodec(codec).Caps_Internal & FFCodecCap.Skip_Frame_Fill_Param) != 0 ? 1 : 0;
		}



		/********************************************************************/
		/// <summary>
		/// Get the name of a codec
		/// </summary>
		/********************************************************************/
		public static CPointer<char> AvCodec_Get_Name(AvCodecId id)//XX 406
		{
			if (id == AvCodecId.None)
				return "none".ToCharPointer();

			AvCodecDescriptor cd = Codec_Desc.AvCodec_Descriptor_Get(id);

			if (cd != null)
				return cd.Name;

			Log.Av_Log(null, Log.Av_Log_Warning, "Codec 0x%x is not in the full list.\n", id);

			AvCodec codec = AllCodec.AvCodec_Find_Decoder(id);

			if (codec != null)
				return codec.Name;

			codec = AllCodec.AvCodec_Find_Encoder(id);

			if (codec != null)
				return codec.Name;

			return "unknown_codec".ToCharPointer();
		}



		/********************************************************************/
		/// <summary>
		/// Return a name for the specified profile, if available
		/// </summary>
		/********************************************************************/
		public static CPointer<char> AvCodec_Profile_Name(AvCodecId codec_Id, AvProfileType profile)//XX 439
		{
			AvCodecDescriptor desc = Codec_Desc.AvCodec_Descriptor_Get(codec_Id);

			if ((profile == AvProfileType.Unknown) || (desc == null) || desc.Profiles.IsNull)
				return null;

			for (c_int i = 0; i < desc.Profiles.Length; i++)
			{
				AvProfile p = desc.Profiles[i];

				if (p.Profile == profile)
					return p.Name;
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Return codec bits per sample.
		/// Only return non-zero if the bits per sample is exactly correct,
		/// not an approximation
		/// </summary>
		/********************************************************************/
		public static c_int Av_Get_Exact_Bits_Per_Sample(AvCodecId codec_Id)//XX 454
		{
			switch (codec_Id)
			{
				case AvCodecId.Dfpwm:
					return 1;

				case AvCodecId._8Svx_Exp:
				case AvCodecId._8Svx_Fib:
				case AvCodecId.Adpcm_Argo:
				case AvCodecId.Adpcm_Ct:
				case AvCodecId.Adpcm_Ima_Alp:
				case AvCodecId.Adpcm_Ima_Amv:
				case AvCodecId.Adpcm_Ima_Apc:
				case AvCodecId.Adpcm_Ima_Apm:
				case AvCodecId.Adpcm_Ima_Ea_Sead:
				case AvCodecId.Adpcm_Ima_Magix:
				case AvCodecId.Adpcm_Ima_Oki:
				case AvCodecId.Adpcm_Ima_Ws:
				case AvCodecId.Adpcm_Ima_Ssi:
				case AvCodecId.Adpcm_G722:
				case AvCodecId.Adpcm_Yamaha:
				case AvCodecId.Adpcm_Aica:
					return 4;

				case AvCodecId.Dsd_Lsbf:
				case AvCodecId.Dsd_Msbf:
				case AvCodecId.Dsd_Lsbf_Planar:
				case AvCodecId.Dsd_Msbf_Planar:
				case AvCodecId.Pcm_Alaw:
				case AvCodecId.Pcm_Mulaw:
				case AvCodecId.Pcm_Vidc:
				case AvCodecId.Pcm_S8:
				case AvCodecId.Pcm_S8_Planar:
				case AvCodecId.Pcm_Sga:
				case AvCodecId.Pcm_U8:
				case AvCodecId.Sdx2_Dpcm:
				case AvCodecId.Cbd2_Dpcm:
				case AvCodecId.Derf_Dpcm:
				case AvCodecId.Wady_Dpcm:
				case AvCodecId.Adpcm_Circus:
					return 8;

				case AvCodecId.Pcm_S16Be:
				case AvCodecId.Pcm_S16Be_Planar:
				case AvCodecId.Pcm_S16Le:
				case AvCodecId.Pcm_S16Le_Planar:
				case AvCodecId.Pcm_U16Be:
				case AvCodecId.Pcm_U16Le:
					return 16;

				case AvCodecId.Pcm_S24Daud:
				case AvCodecId.Pcm_S24Be:
				case AvCodecId.Pcm_S24Le:
				case AvCodecId.Pcm_S24Le_Planar:
				case AvCodecId.Pcm_U24Be:
				case AvCodecId.Pcm_U24Le:
					return 24;

				case AvCodecId.Pcm_S32Be:
				case AvCodecId.Pcm_S32Le:
				case AvCodecId.Pcm_S32Le_Planar:
				case AvCodecId.Pcm_U32Be:
				case AvCodecId.Pcm_U32Le:
				case AvCodecId.Pcm_F32Be:
				case AvCodecId.Pcm_F32Le:
				case AvCodecId.Pcm_F24Le:
				case AvCodecId.Pcm_F16Le:
					return 32;

				case AvCodecId.Pcm_F64Be:
				case AvCodecId.Pcm_F64Le:
				case AvCodecId.Pcm_S64Be:
				case AvCodecId.Pcm_S64Le:
					return 64;

				default:
					return 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return codec bits per sample
		/// </summary>
		/********************************************************************/
		public static c_int Av_Get_Bits_Per_Sample(AvCodecId codec_Id)//XX 549
		{
			switch (codec_Id)
			{
				case AvCodecId.Dfpwm:
					return 1;

				case AvCodecId.Adpcm_SbPro_2:
				case AvCodecId.G728:
					return 2;

				case AvCodecId.Adpcm_SbPro_3:
					return 3;

				case AvCodecId.Adpcm_SbPro_4:
				case AvCodecId.Adpcm_Ima_Wav:
				case AvCodecId.Adpcm_Ima_Xbox:
				case AvCodecId.Adpcm_Ima_Qt:
				case AvCodecId.Adpcm_Swf:
				case AvCodecId.Adpcm_Ms:
					return 4;

				default:
					return Av_Get_Exact_Bits_Per_Sample(codec_Id);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return audio frame duration
		/// </summary>
		/********************************************************************/
		public static c_int Av_Get_Audio_Frame_Duration(AvCodecContext avCtx, c_int frame_Bytes)//XX 803
		{
			c_int channels = avCtx.Ch_Layout.Nb_Channels;

			c_int duration = Get_Audio_Frame_Duration(avCtx.Codec_Id, avCtx.Sample_Rate, channels, avCtx.Block_Align, avCtx.Codec_Tag, avCtx.Bits_Per_Coded_Sample, avCtx.Bit_Rate, avCtx.ExtraData, avCtx.Frame_Size, frame_Bytes);

			return Macros.FFMax(0, duration);
		}



		/********************************************************************/
		/// <summary>
		/// This function is the same as av_get_audio_frame_duration(),
		/// except it works with AVCodecParameters instead of an
		/// AVCodecContext
		/// </summary>
		/********************************************************************/
		public static c_int Av_Get_Audio_Frame_Duration2(AvCodecParameters par, c_int frame_Bytes)//XX 816
		{
			c_int channels = par.Ch_Layout.Nb_Channels;

			c_int duration = Get_Audio_Frame_Duration(par.Codec_Id, par.Sample_Rate, channels, par.Block_Align, par.Codec_Tag, par.Bits_Per_Coded_Sample, par.Bit_Rate, par.ExtraData, par.Frame_Size, frame_Bytes);

			return Macros.FFMax(0, duration);
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve supported hardware configurations for a codec.
		///
		/// Values of index from zero to some maximum return the indexed
		/// configuration descriptor; all other values return NULL. If the
		/// codec does not support any hardware configurations then it will
		/// always return NULL
		/// </summary>
		/********************************************************************/
		public static AvCodecHwConfig AvCodec_Get_Hw_Config(AvCodec avCodec, c_int index)//XX 850
		{
			FFCodec codec = Codec_Internal.FFCodec(avCodec);

			if ((codec.Hw_Configs == null) || (index < 0))
				return null;

			for (c_int i = 0; i <= index; i++)
			{
				if (codec.Hw_Configs[i] == null)
					return null;
			}

			return codec.Hw_Configs[index].Public;
		}



		/********************************************************************/
		/// <summary>
		/// Allocate a CPB properties structure and initialize its fields to
		/// default values
		/// </summary>
		/********************************************************************/
		public static AvCpbProperties Av_Cpb_Properties_Alloc()//XX 968
		{
			AvCpbProperties props = Mem.Av_MAlloczObj<AvCpbProperties>();

			if (props == null)
				return null;

			return props;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Get_Audio_Frame_Duration(AvCodecId id, c_int sr, c_int ch, c_int ba, uint32_t tag, c_int bits_Per_Coded_Sample, int64_t bitRate, IDataContext extraData, c_int frame_Size, c_int frame_Bytes)//XX 571
		{
			c_int bps = Av_Get_Exact_Bits_Per_Sample(id);
			c_int frameCount = (ba > 0) && ((frame_Bytes / ba) > 0) ? frame_Bytes / ba : 1;

			// Codecs with an exact constant bits per sample
			if ((bps > 0) && (ch > 0) && (frame_Bytes > 0) && (ch < 32768) && (bps < 32768))
				return (c_int)((frame_Bytes * 8L) / (bps * ch));

			bps = bits_Per_Coded_Sample;

			// Codecs with a fixed packet duration
			switch (id)
			{
				case AvCodecId.Adpcm_Adx:
					return 32;

				case AvCodecId.Adpcm_Ima_Qt:
					return 64;

				case AvCodecId.Adpcm_Ea_Xas:
					return 128;

				case AvCodecId.Amr_Nb:
				case AvCodecId.Evrc:
				case AvCodecId.Gsm:
				case AvCodecId.Qcelp:
				case AvCodecId.Ra_288:
					return 160;

				case AvCodecId.Amr_Wb:
				case AvCodecId.Gsm_Ms:
					return 320;

				case AvCodecId.Mp1:
					return 384;

				case AvCodecId.Atrac1:
					return 512;

				case AvCodecId.Atrac9:
				case AvCodecId.Atrac3:
				{
					if (frameCount > (c_int.MaxValue / 1024))
						return 0;

					return 1024 * frameCount;
				}

				case AvCodecId.Atrac3P:
					return 2048;

				case AvCodecId.Mp2:
				case AvCodecId.MusePack7:
					return 1152;

				case AvCodecId.Ac3:
					return 1536;

				case AvCodecId.Ftr:
					return 1024;
			}

			if (sr > 0)
			{
				// Calc from sample rate
				if (id == AvCodecId.Tta)
					return 25611 * sr / 245;
				else if (id == AvCodecId.Dst)
					return 58811 * sr / 44100;
				else if (id == AvCodecId.BinkAudio_Dct)
				{
					if ((sr / 22050) > 22)
						return 0;

					return 480 << (sr / 22050);
				}

				if (id == AvCodecId.Mp3)
					return sr <= 24000 ? 576 : 1152;
			}

			if (ba > 0)
			{
				// Calc from block align
				if (id == AvCodecId.Sipr)
				{
					switch (ba)
					{
						case 20:
							return 160;

						case 19:
							return 144;

						case 29:
							return 288;

						case 37:
							return 480;
					}
				}
				else if (id == AvCodecId.Ilbc)
				{
					switch (ba)
					{
						case 38:
							return 160;

						case 50:
							return 240;
					}
				}
			}

			if (frame_Bytes > 0)
			{
				// Calc from frame_bytes only
				if (id == AvCodecId.TrueSpeech)
					return 240 * (frame_Bytes / 32);

				if (id == AvCodecId.Nellymoser)
					return 256 * (frame_Bytes / 64);

				if (id == AvCodecId.Ra_144)
					return 160 * (frame_Bytes / 20);

				if (id == AvCodecId.Aptx)
					return 4 * (frame_Bytes / 4);

				if (id == AvCodecId.Aptx_Hd)
					return 4 * (frame_Bytes / 6);

				if (bps > 0)
				{
					// Calc from frame_bytes and bits_per_coded_sample
					if ((id == AvCodecId.Adpcm_G726) || (id == AvCodecId.Adpcm_G726Le))
						return frame_Bytes * 8 / bps;
				}

				if ((ch > 0) && (ch < (c_int.MaxValue / 16)))
				{
					// Calc from frame_bytes and channels
					switch (id)
					{
						case AvCodecId.FastAudio:
							return frame_Bytes / (40 * ch) * 256;

						case AvCodecId.Adpcm_Ima_Moflex:
							return (frame_Bytes - (4 * ch)) / (128 * ch) * 256;

						case AvCodecId.Adpcm_Afc:
							return frame_Bytes / (9 * ch) * 16;

						case AvCodecId.Adpcm_N64:
						{
							frame_Bytes /= 9 * ch;

							if (frame_Bytes > (c_int.MaxValue / 16))
								return 0;

							return frame_Bytes * 16;
						}

						case AvCodecId.Adpcm_Psx:
						case AvCodecId.Adpcm_Dtk:
						{
							frame_Bytes /= 16 * ch;

							if (frame_Bytes > (c_int.MaxValue / 28))
								return 0;

							return frame_Bytes * 28;
						}

						case AvCodecId.Adpcm_Psxc:
						{
							frame_Bytes = (frame_Bytes - 1) / ch;

							if (frame_Bytes > (c_int.MaxValue / 2))
								return 0;

							return frame_Bytes * 2;
						}

						case AvCodecId._4Xm:
						case AvCodecId.Adpcm_Ima_Acorn:
						case AvCodecId.Adpcm_Ima_Dat4:
						case AvCodecId.Adpcm_Ima_Iss:
						case AvCodecId.Adpcm_Ima_Pda:
							return (frame_Bytes - (4 * ch)) * 2 / ch;

						case AvCodecId.Adpcm_Ima_SmJpeg:
							return (frame_Bytes - 4) * 2 / ch;

						case AvCodecId.Adpcm_Ima_Amv:
							return (frame_Bytes - 8) * 2;

						case AvCodecId.Adpcm_Thp:
						case AvCodecId.Adpcm_Thp_Le:
						{
							if (extraData != null)
								return (c_int)(frame_Bytes * 14L / (8 * ch));

							break;
						}

						case AvCodecId.Adpcm_Xa:
							return (frame_Bytes / 128) * 224 / ch;

						case AvCodecId.Interplay_Dpcm:
							return (frame_Bytes - 6 - ch) / ch;

						case AvCodecId.Roq_Dpcm:
							return (frame_Bytes - 8) / ch;

						case AvCodecId.Xan_Dpcm:
							return (frame_Bytes - (2 * ch)) / ch;

						case AvCodecId.Mace3:
							return 3 * frame_Bytes / ch;

						case AvCodecId.Mace6:
							return 6 * frame_Bytes / ch;

						case AvCodecId.Pcm_Lxf:
							return 2 * (frame_Bytes / (5 * ch));

						case AvCodecId.Iac:
						case AvCodecId.Imc:
							return 4 * frame_Bytes / ch;
					}

					if (tag != 0)
					{
						// Calc from frame_bytes, channels, and codec_tag
						if (id == AvCodecId.Sol_Dpcm)
						{
							if (tag == 3)
								return frame_Bytes / ch;
							else
								return frame_Bytes * 2 / ch;
						}
					}

					if (ba > 0)
					{
						// Calc from frame_bytes, channels, and block_align
						c_int blocks = frame_Bytes / ba;
						int64_t tmp = 0;

						switch (id)
						{
							case AvCodecId.Adpcm_Ima_Xbox:
							{
								if (bps != 4)
									return 0;

								tmp = blocks * ((ba - (4 * ch)) / (bps * ch) * 8);
								break;
							}

							case AvCodecId.Adpcm_Ima_Wav:
							{
								if ((bps < 2) || (bps > 5))
									return 0;

								tmp = blocks * ((1L + (ba - (4 * ch)) / (bps * ch)) * 8L);
								break;
							}

							case AvCodecId.Adpcm_Ima_Dk3:
							{
								tmp = blocks * (((ba - 16L) * 2 / 3 * 4) / ch);
								break;
							}

							case AvCodecId.Adpcm_Ima_Dk4:
							{
								tmp = blocks * (1 + (((ba - (4L * ch)) * 2) / ch));
								break;
							}

							case AvCodecId.Adpcm_Ima_Rad:
							{
								tmp = blocks * ((ba - ((4L * ch) * 2)) / ch);
								break;
							}

							case AvCodecId.Adpcm_Ms:
							{
								tmp = blocks * (2 + (((ba - (7L * ch)) * 2L) / ch));
								break;
							}
	
							case AvCodecId.Adpcm_Mtaf:
							{
								tmp = blocks * (ba - 16L) * 2 / ch;
								break;
							}

							case AvCodecId.Adpcm_Xmd:
							{
								tmp = blocks * 32;
								break;
							}
						}

						if (tmp != 0)
						{
							if (tmp != (c_int)tmp)
								return 0;

							return (c_int)tmp;
						}
					}

					if (bps > 0)
					{
						// Calc from frame_bytes, channels, and bits_per_coded_sample
						switch (id)
						{
							case AvCodecId.Pcm_Dvd:
							{
								if ((bps < 4) || (frame_Bytes < 3))
									return 0;

								return 2 * ((frame_Bytes - 3) / ((bps * 2 / 8) * ch));
							}

							case AvCodecId.Pcm_Bluray:
							{
								if ((bps < 4) || (frame_Bytes < 4))
									return 0;

								return (frame_Bytes - 4) / ((Macros.FFAlign(ch, 2) * bps) / 8);
							}

							case AvCodecId.S302M:
								return 2 * (frame_Bytes / ((bps + 4) / 4)) / ch;
						}
					}
				}
			}

			// Fall back on using frame_size
			if ((frame_Size > 1) && (frame_Bytes != 0))
				return frame_Size;

			// For WMA we currently have no other means to calculate duration thus we
			// do it here by assuming CBR, which is true for all known cases
			if ((bitRate > 0) && (frame_Bytes > 0) && (sr > 0) && (ba > 1))
			{
				if ((id == AvCodecId.WmaV1) || (id == AvCodecId.WmaV2))
					return (c_int)((frame_Bytes * 8L * sr) / bitRate);
			}

			return 0;
		}
		#endregion
	}
}
