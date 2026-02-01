/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Linq;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Interfaces;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Buffer = Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Buffer;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// 
	/// </summary>
	public static class AvCodec_
	{
		/// <summary>
		/// 
		/// </summary>
		internal static SideDataMap[] FF_Sd_Global_Map =
		[
			new SideDataMap(AvPacketSideDataType.ReplayGain, AvFrameSideDataType.ReplayGain),
			new SideDataMap(AvPacketSideDataType.DisplayMatrix, AvFrameSideDataType.DisplayMatrix),
			new SideDataMap(AvPacketSideDataType.Spherical, AvFrameSideDataType.Spherical),
			new SideDataMap(AvPacketSideDataType.Stereo3D, AvFrameSideDataType.Stereo3D),
			new SideDataMap(AvPacketSideDataType.Audio_Service_Type, AvFrameSideDataType.Audio_Service_Type),
			new SideDataMap(AvPacketSideDataType.Mastering_Display_Metadata, AvFrameSideDataType.Mastering_Display_Metadata),
			new SideDataMap(AvPacketSideDataType.Content_Light_Level, AvFrameSideDataType.Content_Light_Level),
			new SideDataMap(AvPacketSideDataType.Icc_Profile, AvFrameSideDataType.Icc_Profile),
			new SideDataMap(AvPacketSideDataType.Ambient_Viewing_Environment, AvFrameSideDataType.Ambient_Viewing_Environment),
			new SideDataMap(AvPacketSideDataType._3D_Reference_Displays, AvFrameSideDataType._3D_Reference_Displays),
			new SideDataMap(AvPacketSideDataType.Exif, AvFrameSideDataType.Exif),
			new SideDataMap(AvPacketSideDataType.Nb, 0)
		];

		private static readonly AvMutex codec_Mutex = new AvMutex();

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int AvCodec_Default_Execute(AvCodecContext c, CodecFunc.Execute_Func_Delegate func, CPointer<IExecuteArg> arg, CPointer<c_int> ret, c_int count)//XX 74
		{
			for (size_t i = 0; i < (size_t)count; i++)
			{
				c_int r = func(c, arg[i]);

				if (ret.IsNotNull)
					ret[i] = r;
			}

			Emms.Emms_C();

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int AvCodec_Default_Execute2(AvCodecContext c, CodecFunc.Execute2_Func_Delegate func, CPointer<IExecuteArg> arg, CPointer<c_int> ret, c_int count)//XX 88
		{
			for (c_int i = 0; i < count; i++)
			{
				c_int r = func(c, arg[0], i, 0);

				if (ret.IsNotNull)
					ret[i] = r;
			}

			Emms.Emms_C();

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the AVCodecContext to use the given AVCodec. Prior to
		/// using this function the context has to be allocated with
		/// avcodec_alloc_context3().
		///
		/// The functions avcodec_find_decoder_by_name(),
		/// avcodec_find_encoder_by_name(), avcodec_find_decoder() and
		/// avcodec_find_encoder() provide an easy way for retrieving a codec.
		///
		/// Depending on the codec, you might need to set options in the
		/// codec context also for decoding (e.g. width, height, or the pixel
		/// or audio sample format in the case the information is not
		/// available in the bitstream, as when decoding raw audio or video).
		///
		/// Options in the codec context can be set either by setting them in
		/// the options AVDictionary, or by setting the values in the
		/// context itself, directly or by using the av_opt_set() API
		/// before calling this function.
		/// </summary>
		/********************************************************************/
		public static c_int AvCodec_Open2(AvCodecContext avCtx, AvCodec codec, ref AvDictionary options)//XX 145
		{
			c_int ret = 0;

			if (AvCodec_Is_Open(avCtx))
				return 0;

			if ((codec == null) && (avCtx.Codec == null))
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "No codec provided to avcodec_open2()\n");

				return Error.EINVAL;
			}

			if ((codec != null) && (avCtx.Codec != null) && (codec != avCtx.Codec))
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "This AVCodecContext was allocated for %s, but %s passed to avcodec_open2()\n", avCtx.Codec.Name, codec.Name);

				return Error.EINVAL;
			}

			if (codec == null)
				codec = avCtx.Codec;

			FFCodec codec2 = Codec_Internal.FFCodec(codec);

			if (((avCtx.Codec_Type != AvMediaType.Unknown) && (avCtx.Codec_Type != codec.Type)) || ((avCtx.Codec_Id != AvCodecId.None) && (avCtx.Codec_Id != codec.Id)))
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "Codec type or id mismatches\n");

				return Error.EINVAL;
			}

			avCtx.Codec_Type = codec.Type;
			avCtx.Codec_Id = codec.Id;
			avCtx.Codec = codec;

			// Set the whitelist from provided options dict,
			// so we can check it immediately
			AvDictionaryEntry e = options != null ? Dict.Av_Dict_Get(options, "codec_whitelist", AvDict.None).FirstOrDefault() : null;

			if (e != null)
			{
				ret = Opt.Av_Opt_Set(avCtx, e.Key, e.Value, AvOptSearch.None);

				if (ret < 0)
					return ret;
			}

			if (avCtx.Codec_Whitelist.IsNotNull && (AvString.Av_Match_List(codec.Name, avCtx.Codec_Whitelist, ',') <= 0))
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "Codec (%s) not on whitelist \'%s\'\n", codec.Name, avCtx.Codec_Whitelist);

				return Error.EINVAL;
			}

			AvCodecInternal avci = Codec_Internal.FF_Codec_Is_Decoder(codec) ? Decode.FF_Decode_Internal_Alloc() : Encode.FF_Encode_Internal_Alloc();

			if (avci == null)
			{
				ret = Error.ENOMEM;

				goto End;
			}

			avCtx.Internal = avci;

			avci.Buffer_Frame = Frame.Av_Frame_Alloc();
			avci.Buffer_Pkt = Packet.Av_Packet_Alloc();

			if ((avci.Buffer_Frame == null) || (avci.Buffer_Pkt == null))
			{
				ret = Error.ENOMEM;

				goto Free_And_End;
			}

			if (codec2.Priv_Data_Alloc != null)
			{
				if (avCtx.Priv_Data == null)
				{
					avCtx.Priv_Data = codec2.Priv_Data_Alloc();

					if (avCtx.Priv_Data == null)
					{
						ret = Error.ENOMEM;

						goto Free_And_End;
					}

					if (codec.Priv_Class != null)
					{
						codec.Priv_Class.CopyTo(avCtx.Priv_Data);
						Opt.Av_Opt_Set_Defaults(avCtx.Priv_Data);
					}
				}
			}
			else
				avCtx.Priv_Data = null;

			ret = Opt.Av_Opt_Set_Dict2(avCtx, ref options, AvOptSearch.Search_Children);

			if (ret < 0)
				goto Free_And_End;

			// Only call ff_set_dimensions() for non H.264/VP6F/DXV codecs so as not to overwrite previously setup dimensions
			if (!((avCtx.Coded_Width != 0) && (avCtx.Coded_Height != 0) && (avCtx.PictureSize.Width != 0) && (avCtx.PictureSize.Height != 0) && ((avCtx.Codec_Id == AvCodecId.H264) || (avCtx.Codec_Id == AvCodecId.Vp6F) || (avCtx.Codec_Id == AvCodecId.Dxv))))
			{
				if ((avCtx.Coded_Width != 0) && (avCtx.Coded_Height != 0))
					ret = Utils_Codec.FF_Set_Dimensions(avCtx, avCtx.Coded_Width, avCtx.Coded_Height);
				else if ((avCtx.PictureSize.Width != 0) && (avCtx.PictureSize.Height != 0))
					ret = Utils_Codec.FF_Set_Dimensions(avCtx, avCtx.PictureSize.Width, avCtx.PictureSize.Height);

				if (ret < 0)
					goto Free_And_End;
			}

			if (((avCtx.Coded_Width != 0) || (avCtx.Coded_Height != 0) || (avCtx.PictureSize.Width != 0) || (avCtx.PictureSize.Height != 0)) && ((ImgUtils.Av_Image_Check_Size2((c_uint)avCtx.Coded_Width, (c_uint)avCtx.Coded_Height, avCtx.Max_Pixels, AvPixelFormat.None, 0, avCtx) < 0) || (ImgUtils.Av_Image_Check_Size2((c_uint)avCtx.PictureSize.Width, (c_uint)avCtx.PictureSize.Height, avCtx.Max_Pixels, AvPixelFormat.None, 0, avCtx) < 0)))
			{
				Log.Av_Log(avCtx, Log.Av_Log_Warning, "Ignoring invalid width/height values\n");

				Utils_Codec.FF_Set_Dimensions(avCtx, 0, 0);
			}

			if ((avCtx.PictureSize.Width > 0) && (avCtx.PictureSize.Height > 0))
			{
				if (ImgUtils.Av_Image_Check_Sar((c_uint)avCtx.PictureSize.Width, (c_uint)avCtx.PictureSize.Height, avCtx.Sample_Aspect_Ratio) < 0)
				{
					Log.Av_Log(avCtx, Log.Av_Log_Warning, "ignoring invalid SAR: %u/%u\n", avCtx.Sample_Aspect_Ratio.Num, avCtx.Sample_Aspect_Ratio.Den);

					avCtx.Sample_Aspect_Ratio = new AvRational(0, 1);
				}
			}

			// AV_CODEC_CAP_CHANNEL_CONF is a decoder-only flag; so the code below
			// in particular checks that sample_rate is set for all audio encoders
			if ((avCtx.Sample_Rate < 0) || ((avCtx.Sample_Rate == 0) && (avCtx.Codec_Type == AvMediaType.Audio) && ((codec.Capabilities & AvCodecCap.Channel_Conf) == 0)))
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "Invalid sample rate: %d\n", avCtx.Sample_Rate);

				ret = Error.EINVAL;

				goto Free_And_End;
			}

			if (avCtx.Block_Align < 0)
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "Invalid block align: %d\n", avCtx.Block_Align);

				ret = Error.EINVAL;

				goto Free_And_End;
			}

			// AV_CODEC_CAP_CHANNEL_CONF is a decoder-only flag; so the code below
			// in particular checks that nb_channels is set for all audio encoders
			if ((avCtx.Codec_Type == AvMediaType.Audio) && (avCtx.Ch_Layout.Nb_Channels == 0) && ((codec.Capabilities & AvCodecCap.Channel_Conf) == 0))
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "%s requires channel layout to be set\n", Codec_Internal.FF_Codec_Is_Decoder(codec) ? "Decoder" : "Encoder");

				ret = Error.EINVAL;

				goto Free_And_End;
			}

			if ((avCtx.Ch_Layout.Nb_Channels != 0) && (Channel_Layout.Av_Channel_Layout_Check(avCtx.Ch_Layout) == 0))
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "Invalid channel layout\n");

				ret = Error.EINVAL;

				goto Free_And_End;
			}

			if (avCtx.Ch_Layout.Nb_Channels > CodecConstants.FF_Sane_Nb_Channels)
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "Too many channels: %d\n", avCtx.Ch_Layout.Nb_Channels);

				ret = Error.EINVAL;

				goto Free_And_End;
			}

			avCtx.Frame_Num = 0;
			avCtx.Codec_Descriptor = Codec_Desc.AvCodec_Descriptor_Get(avCtx.Codec_Id);

			if (((avCtx.Codec.Capabilities & AvCodecCap.Experimental) != 0) && (avCtx.Strict_Std_Compliance > FFCompliance.Experimental))
			{
				string codec_String = Codec_Internal.FF_Codec_Is_Encoder(codec) ? "encoder" : "decoder";
				Log.Av_Log(avCtx, Log.Av_Log_Error, "The %s '%s' is experimental but experimental codecs are not enabled, add '-strict %d' if you want to use it.\n", codec_String, codec.Name, FFCompliance.Experimental);

				AvCodec codec2_ = Codec_Internal.FF_Codec_Is_Encoder(codec) ? AllCodec.AvCodec_Find_Encoder(codec.Id) : AllCodec.AvCodec_Find_Decoder(codec.Id);

				if ((codec2_.Capabilities & AvCodecCap.Experimental) == 0)
					Log.Av_Log(avCtx, Log.Av_Log_Error, "Alternatively use the non experimental %s '%s'.\n", codec_String, codec2_.Name);

				ret = Error.Experimental;

				goto Free_And_End;
			}

			if ((avCtx.Codec_Type == AvMediaType.Audio) && ((avCtx.Time_Base.Num == 0) || (avCtx.Time_Base.Den == 0)))
			{
				avCtx.Time_Base.Num = 1;
				avCtx.Time_Base.Den = avCtx.Sample_Rate;
			}

			if (Codec_Internal.FF_Codec_Is_Encoder(avCtx.Codec))
				ret = Encode.FF_Encode_Preinit(avCtx);
			else
				ret = Decode.FF_Decode_Preinit(avCtx);

			if (ret < 0)
				goto Free_And_End;

			if (avci.Frame_Thread_Encoder == null)
			{
				// Frame-threaded decoders call FFCodec.init for their child contexts
				Lock_AvCodec(codec2);

				ret = PThread.FF_Thread_Init(avCtx);

				Unlock_AvCodec(codec2);

				if (ret < 0)
					goto Free_And_End;
			}

			if ((codec2.Caps_Internal & FFCodecCap.Auto_Threads) == 0)
				avCtx.Thread_Count = 1;

			if (((avCtx.Active_Thread_Type & FFThread.Frame) == 0) || (avci.Frame_Thread_Encoder != null))
			{
				if (codec2.Init != null)
				{
					Lock_AvCodec(codec2);

					ret = codec2.Init(avCtx);

					Unlock_AvCodec(codec2);

					if (ret < 0)
					{
						avci.Needs_Close = (c_int)(codec2.Caps_Internal & FFCodecCap.Init_Cleanup);

						goto Free_And_End;
					}
				}

				avci.Needs_Close = 1;
			}

			ret = 0;

			if (Codec_Internal.FF_Codec_Is_Decoder(avCtx.Codec))
			{
				if (avCtx.Bit_Rate == 0)
					avCtx.Bit_Rate = Get_Bit_Rate(avCtx);

				// Validate channel layout from the decoder
				if (((avCtx.Ch_Layout.Nb_Channels != 0) && (Channel_Layout.Av_Channel_Layout_Check(avCtx.Ch_Layout) == 0)) || (avCtx.Ch_Layout.Nb_Channels > CodecConstants.FF_Sane_Nb_Channels))
				{
					ret = Error.EINVAL;

					goto Free_And_End;
				}

				if (avCtx.Bits_Per_Coded_Sample < 0)
				{
					ret = Error.EINVAL;

					goto Free_And_End;
				}
			}

			End:
			return ret;

			Free_And_End:
			FF_Codec_Close(avCtx);

			goto End;
		}



		/********************************************************************/
		/// <summary>
		/// Free all allocated data in the given subtitle struct
		/// </summary>
		/********************************************************************/
		public static void AvSubtitle_Free(AvSubtitle sub)//XX 412
		{
			for (c_int i = 0; i < sub.Num_Rects; i++)
			{
				AvSubtitleRect rect = sub.Rects[i];

				Mem.Av_FreeP(ref rect.Data[0]);
				Mem.Av_FreeP(ref rect.Data[1]);
				Mem.Av_FreeP(ref rect.Data[2]);
				Mem.Av_FreeP(ref rect.Data[3]);
				Mem.Av_FreeP(ref rect.Text);
				Mem.Av_FreeP(ref rect.Ass);

				Mem.Av_FreeP(ref sub.Rects[i]);
			}

			Mem.Av_FreeP(ref sub.Rects);

			sub.Clear();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void FF_Codec_Close(AvCodecContext avCtx)//XX 434
		{
			if (AvCodec_Is_Open(avCtx))
			{
				AvCodecInternal avci = avCtx.Internal;

				if (avci.Thread_Ctx != null)
					PThread.FF_Thread_Free(avCtx);

				if ((avci.Needs_Close != 0) && (Codec_Internal.FFCodec(avCtx.Codec).Close != null))
					Codec_Internal.FFCodec(avCtx.Codec).Close(avCtx);

				avci.Byte_Buffer_Size = 0;
				Mem.Av_FreeP(ref avci.Byte_Buffer);
				Frame.Av_Frame_Free(ref avci.Buffer_Frame);
				Packet.Av_Packet_Free(ref avci.Buffer_Pkt);
				Packet.Av_Packet_Free(ref avci.Last_Pkt_Props);

				Packet.Av_Packet_Free(ref avci.In_Pkt);
				Frame.Av_Frame_Free(ref avci.In_Frame);
				Frame.Av_Frame_Free(ref avci.Recon_Frame);

				RefStruct.Av_RefStruct_Unref(ref avci.Pool);
				RefStruct.Av_RefStruct_Pool_Uninit(ref avci.Progress_Frame_Pool);

				if (Utils_Codec.Av_Codec_Is_Decoder(avCtx.Codec))
					Decode.FF_Decode_Internal_Uninit(avCtx);

				Decode.FF_HwAccel_Uninit(avCtx);

				Bsf.Av_Bsf_Free(ref avci.Bsf);

				Mem.Av_FreeP(ref avCtx.Internal);
			}

			for (c_int i = 0; i < avCtx.Coded_Side_Data.Count; i++)
				Mem.Av_FreeP(ref avCtx.Coded_Side_Data.Array[i].Data);

			Mem.Av_FreeP(ref avCtx.Coded_Side_Data.Array);
			avCtx.Coded_Side_Data.Count = 0;

			Side_Data.Av_Frame_Side_Data_Free(ref avCtx.Decoded_Side_Data, ref avCtx.Nb_Decoded_Side_Data);

			Buffer.Av_Buffer_Unref(ref avCtx.Hw_Frames_Ctx);
			Buffer.Av_Buffer_Unref(ref avCtx.Hw_Device_Ctx);

			if ((avCtx.Priv_Data != null) && (avCtx.Codec != null) && (avCtx.Codec.Priv_Class != null))
				Opt.Av_Opt_Free(avCtx.Priv_Data);

			Opt.Av_Opt_Free(avCtx);
			Mem.Av_FreeP(ref avCtx.Priv_Data);

			if (Utils_Codec.Av_Codec_Is_Encoder(avCtx.Codec))
				Mem.Av_FreeP(ref avCtx.ExtraData);
			else if (Utils_Codec.Av_Codec_Is_Decoder(avCtx.Codec))
				Mem.Av_FreeP(ref avCtx.Subtitle_Header);

			avCtx.Codec = null;
			avCtx.Active_Thread_Type = FFThread.None;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void AvCodec_String(CPointer<char> buf, c_int buf_Size, AvCodecContext enc, c_int encode)//XX 504
		{
			c_int new_Line = 0;
			CPointer<char> separator = enc.Dump_Separator.IsNotNull ? enc.Dump_Separator : ", ".ToCharPointer();
			CPointer<char> str;

			if (buf.IsNull || (buf_Size <= 0))
				return;

			BPrint.Av_BPrint_Init_For_Buffer(out AVBPrint bPrint, buf, (c_uint)buf_Size);

			CPointer<char> codec_Type = Utils.Av_Get_Media_Type_String(enc.Codec_Type);
			CPointer<char> codec_Name = Utils_Codec.AvCodec_Get_Name(enc.Codec_Id);
			CPointer<char> profile = Utils_Codec.AvCodec_Profile_Name(enc.Codec_Id, enc.Profile);

			BPrint.Av_BPrintf(bPrint, "%s: %s", codec_Type.IsNotNull ? codec_Type : "unknown", codec_Name);

			buf[0] = char.ToUpper(buf[0]);	// First letter in uppercase

			if ((enc.Codec != null) && (CString.strcmp(enc.Codec.Name, codec_Name) != 0))
				BPrint.Av_BPrintf(bPrint, " (%s)", enc.Codec.Name);

			if (profile.IsNotNull)
				BPrint.Av_BPrintf(bPrint, " (%s)", profile);

			if ((enc.Codec_Type == AvMediaType.Video) && (Log.Av_Log_Get_Level() >= Log.Av_Log_Verbose) && (enc.Refs != 0))
				BPrint.Av_BPrintf(bPrint, ", %d reference frame%s", enc.Refs, enc.Refs > 1 ? "s" : CString.Empty);

			if (enc.Codec_Tag != 0)
				BPrint.Av_BPrintf(bPrint, " (%s / 0x%04X)", AvUtil.Av_FourCC2Str(enc.Codec_Tag), enc.Codec_Tag);

			switch (enc.Codec_Type)
			{
				case AvMediaType.Video:
				{
					BPrint.Av_BPrintf(bPrint, "%s%s", separator, enc.Pix_Fmt == AvPixelFormat.None ? "none" : Unknown_If_Null(PixDesc.Av_Get_Pix_Fmt_Name(enc.Pix_Fmt)));
					BPrint.Av_BPrint_Chars(bPrint, '(', 1);

					c_uint len = bPrint.Len;

					// The following check ensures that '(' has been written
					// and therefore allows us to erase it if it turns out
					// to be unnecessary
					if (BPrint.Av_BPrint_Is_Complete(bPrint) == 0)
						return;

					if ((enc.Bits_Per_Raw_Sample != 0) && (enc.Pix_Fmt != AvPixelFormat.None) && (enc.Bits_Per_Raw_Sample < PixDesc.Av_Pix_Fmt_Desc_Get(enc.Pix_Fmt).Comp[0].Depth))
						BPrint.Av_BPrintf(bPrint, "%d bpc, ", enc.Bits_Per_Raw_Sample);

					if ((enc.Color_Range != AvColorRange.Unspecified) && (str = PixDesc.Av_Color_Range_Name(enc.Color_Range)).IsNotNull)
						BPrint.Av_BPrintf(bPrint, "%s, ", str);

					if ((enc.ColorSpace != AvColorSpace.Unspecified) || (enc.Color_Primaries != AvColorPrimaries.Unspecified) || (enc.Color_Trc != AvColorTransferCharacteristic.Unspecified))
					{
						CPointer<char> col = Unknown_If_Null(PixDesc.Av_Color_Space_Name(enc.ColorSpace));
						CPointer<char> pri = Unknown_If_Null(PixDesc.Av_Color_Primaries_Name(enc.Color_Primaries));
						CPointer<char> trc = Unknown_If_Null(PixDesc.Av_Color_Transfer_Name(enc.Color_Trc));

						if ((CString.strcmp(col, pri) != 0) || (CString.strcmp(col, trc) != 0))
						{
							new_Line = 1;
							BPrint.Av_BPrintf(bPrint, "%s/%s/%s, ", col, pri, trc);
						}
						else
							BPrint.Av_BPrintf(bPrint, "%s, ", col);
					}

					if (enc.Field_Order != AvFieldOrder.Unknown)
					{
						CPointer<char> field_Order = "progressive".ToCharPointer();

						if (enc.Field_Order == AvFieldOrder.Tt)
							field_Order = "top first".ToCharPointer();
						else if (enc.Field_Order == AvFieldOrder.Bb)
							field_Order = "bottom first".ToCharPointer();
						else if (enc.Field_Order == AvFieldOrder.Tb)
							field_Order = "top coded first (swapped)".ToCharPointer();
						else if (enc.Field_Order == AvFieldOrder.Bt)
							field_Order = "bottom coded first (swapped)".ToCharPointer();

						BPrint.Av_BPrintf(bPrint, "%s, ", field_Order);
					}

					if ((Log.Av_Log_Get_Level() >= Log.Av_Log_Verbose) && (enc.Chroma_Sample_Location != AvChromaLocation.Unspecified) && (str = PixDesc.Av_Chroma_Location_Name(enc.Chroma_Sample_Location)).IsNotNull)
						BPrint.Av_BPrintf(bPrint, "%s, ", str);

					if (len == bPrint.Len)
					{
						bPrint.Str[len - 1] = '\0';
						bPrint.Len--;
					}
					else
					{
						if ((bPrint.Len - 2) < bPrint.Size)
						{
							// Erase the last ", "
							bPrint.Len -= 2;
							bPrint.Str[bPrint.Len] = '\0';
						}

						BPrint.Av_BPrint_Chars(bPrint, ')', 1);
					}

					if (enc.PictureSize.Width != 0)
					{
						BPrint.Av_BPrintf(bPrint, "%s%dx%d", new_Line != 0 ? separator : ", ", enc.PictureSize.Width, enc.PictureSize.Height);

						if ((Log.Av_Log_Get_Level() >= Log.Av_Log_Verbose) && (enc.Coded_Width != 0) && (enc.Coded_Height != 0) && ((enc.PictureSize.Width != enc.Coded_Width) || (enc.PictureSize.Height != enc.Coded_Height)))
							BPrint.Av_BPrintf(bPrint, " (%dx%d)", enc.Coded_Width, enc.Coded_Height);

						if (enc.Sample_Aspect_Ratio.Num != 0)
						{
							AvRational display_Aspect_Ratio;

							Rational.Av_Reduce(out display_Aspect_Ratio.Num, out display_Aspect_Ratio.Den, enc.PictureSize.Width * (int64_t)enc.Sample_Aspect_Ratio.Num, enc.PictureSize.Height * (int64_t)enc.Sample_Aspect_Ratio.Den, 1024 * 1024);

							BPrint.Av_BPrintf(bPrint, " [SAR %d:%d DAR %d:%d]", enc.Sample_Aspect_Ratio.Num, enc.Sample_Aspect_Ratio.Den, display_Aspect_Ratio.Num, display_Aspect_Ratio.Den);
						}

						if (Log.Av_Log_Get_Level() >= Log.Av_Log_Debug)
						{
							c_int g = (c_int)Mathematics.Av_Gcd(enc.Time_Base.Num, enc.Time_Base.Den);

							BPrint.Av_BPrintf(bPrint, ", %d/%d", enc.Time_Base.Num / g, enc.Time_Base.Den / g);
						}
					}

					if (encode != 0)
						BPrint.Av_BPrintf(bPrint, ", q=%d-%d", enc.QMin, enc.QMax);

					break;
				}

				case AvMediaType.Audio:
				{
					BPrint.Av_BPrintf(bPrint, "%s", separator);

					if (enc.Sample_Rate != 0)
						BPrint.Av_BPrintf(bPrint, "%d Hz, ", enc.Sample_Rate);

					Channel_Layout.Av_Channel_Layout_Describe_BPrint(enc.Ch_Layout, bPrint);

					if ((enc.Sample_Fmt != AvSampleFormat.None) && (str = SampleFmt.Av_Get_Sample_Fmt_Name(enc.Sample_Fmt)).IsNotNull)
						BPrint.Av_BPrintf(bPrint, ", %s", str);

					if ((enc.Bits_Per_Raw_Sample > 0) && (enc.Bits_Per_Raw_Sample != SampleFmt.Av_Get_Bytes_Per_Sample(enc.Sample_Fmt) * 8))
						BPrint.Av_BPrintf(bPrint, " (%d bit)", enc.Bits_Per_Raw_Sample);

					if (Log.Av_Log_Get_Level() >= Log.Av_Log_Verbose)
					{
						if (enc.Initial_Padding != 0)
							BPrint.Av_BPrintf(bPrint, ", delay %d", enc.Initial_Padding);

						if (enc.Trailing_Padding != 0)
							BPrint.Av_BPrintf(bPrint, ", padding %d", enc.Trailing_Padding);
					}

					break;
				}

				case AvMediaType.Data:
				{
					if (Log.Av_Log_Get_Level() >= Log.Av_Log_Debug)
					{
						c_int g = (c_int)Mathematics.Av_Gcd(enc.Time_Base.Num, enc.Time_Base.Den);

						if (g != 0)
							BPrint.Av_BPrintf(bPrint, ", %d/%d", enc.Time_Base.Num / g, enc.Time_Base.Den / g);
					}

					break;
				}

				case AvMediaType.Subtitle:
				{
					if (enc.PictureSize.Width != 0)
						BPrint.Av_BPrintf(bPrint, ", %dx%d", enc.PictureSize.Width, enc.PictureSize.Height);

					break;
				}

				default:
					return;
			}

			if (encode != 0)
			{
				if ((enc.Flags & AvCodecFlag.Pass1) != 0)
					BPrint.Av_BPrintf(bPrint, ", pass 1");

				if ((enc.Flags & AvCodecFlag.Pass2) != 0)
					BPrint.Av_BPrintf(bPrint, ", pass 2");
			}

			int64_t bitRate = Get_Bit_Rate(enc);

			if (bitRate != 0)
				BPrint.Av_BPrintf(bPrint, ", %lld kb/s", bitRate / 1000);
			else if (enc.Rc_Max_Rate > 0)
				BPrint.Av_BPrintf(bPrint, ", max. %lld kb/s", enc.Rc_Max_Rate / 1000);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool AvCodec_Is_Open(AvCodecContext s)//XX 703
		{
			return s.Internal != null;
		}



		/********************************************************************/
		/// <summary>
		/// Return decoded output data from a decoder or encoder (when the
		/// AV_CODEC_FLAG_RECON_FRAME flag is used)
		/// </summary>
		/********************************************************************/
		public static c_int AvCodec_Receive_Frame(AvCodecContext avCtx, AvFrame frame)//XX 708
		{
			Frame.Av_Frame_Unref(frame);

			if (!AvCodec_Is_Open(avCtx) || (avCtx.Codec == null))
				return Error.EINVAL;

			if (Codec_Internal.FF_Codec_Is_Decoder(avCtx.Codec))
				return Decode.FF_Decode_Receive_Frame(avCtx, frame);

			return Encode.FF_Encode_Receive_Frame(avCtx, frame);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Lock_AvCodec(FFCodec codec)//XX 103
		{
			if (((codec.Caps_Internal & FFCodecCap.Not_Init_Threadsafe) != 0) && (codec.Init != null))
				CThread.pthread_mutex_lock(codec_Mutex);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Unlock_AvCodec(FFCodec codec)//XX 109
		{
			if (((codec.Caps_Internal & FFCodecCap.Not_Init_Threadsafe) != 0) && (codec.Init != null))
				CThread.pthread_mutex_unlock(codec_Mutex);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int64_t Get_Bit_Rate(AvCodecContext ctx)//XX 115
		{
			int64_t bit_Rate;

			switch (ctx.Codec_Type)
			{
				case AvMediaType.Video:
				case AvMediaType.Data:
				case AvMediaType.Subtitle:
				case AvMediaType.Attachment:
				{
					bit_Rate = ctx.Bit_Rate;
					break;
				}

				case AvMediaType.Audio:
				{
					c_int bits_Per_Sample = Utils_Codec.Av_Get_Bits_Per_Sample(ctx.Codec_Id);

					if (bits_Per_Sample != 0)
					{
						bit_Rate = ctx.Sample_Rate * (int64_t)ctx.Ch_Layout.Nb_Channels;

						if ((bit_Rate > (int64_t.MaxValue / bits_Per_Sample)))
							bit_Rate = 0;
						else
							bit_Rate *= bits_Per_Sample;
					}
					else
						bit_Rate = ctx.Bit_Rate;

					break;
				}

				default:
				{
					bit_Rate = 0;
					break;
				}
			}

			return bit_Rate;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static CPointer<char> Unknown_If_Null(CPointer<char> str)//XX 499
		{
			return str.IsNotNull ? str : "unknown".ToCharPointer();
		}
		#endregion
	}
}
