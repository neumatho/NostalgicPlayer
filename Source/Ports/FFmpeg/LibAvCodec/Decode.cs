/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;
using Buffer = Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Buffer;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// Generic decoding-related code
	/// </summary>
	public static class Decode
	{
		/********************************************************************/
		/// <summary>
		/// Called by decoders to get the next packet for decoding
		/// </summary>
		/********************************************************************/
		public static c_int FF_Decode_Get_Packet(AvCodecContext avCtx, AvPacket pkt)//XX 245
		{
			AvCodecInternal avci = avCtx.Internal;
			DecodeContext dc = Decode_Ctx(avci);

			if (avci.Draining != 0)
				return Error.EOF;

			// If we are a worker thread, get the next packet from the threading
			// context. Otherwise we are the main (user-facing) context, so we get the
			// next packet from the input filterchain
			if (avCtx.Internal.Is_Frame_Mt != 0)
				return PThread_Frame.FF_Thread_Get_Packet(avCtx, pkt);

			while (true)
			{
				c_int ret = Decode_Get_Packet(avCtx, pkt);

				if ((ret == Error.EAGAIN) && (!Packet.AvPacket_Is_Empty(avci.Buffer_Pkt) || (dc.Draining_Started != 0)))
				{
					ret = Bsf.Av_Bsf_Send_Packet(avci.Bsf, avci.Buffer_Pkt);

					if (ret >= 0)
						continue;

					Packet.Av_Packet_Unref(avci.Buffer_Pkt);
				}

				if (ret == Error.EOF)
					avci.Draining = 1;

				return ret;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Do the actual decoding and obtain a decoded frame from the
		/// decoder, if available. When frame threading is used, this is
		/// invoked by the worker threads, otherwise by the top layer
		/// directly
		/// </summary>
		/********************************************************************/
		internal static c_int FF_Decode_Receive_Frame_Internal(AvCodecContext avCtx, AvFrame frame)//XX 608
		{
			AvCodecInternal avci = avCtx.Internal;
			DecodeContext dc = Decode_Ctx(avci);
			FFCodec codec = Codec_Internal.FFCodec(avCtx.Codec);
			c_int ret;

			if (codec.Cb_Type == FFCodecType.Receive_Frame)
			{
				while (true)
				{
					frame.Pict_Type = dc.Initial_Pict_Type;
					frame.Flags |= dc.Intra_Only_Flag;

					ret = codec.Cb.Receive_Frame(avCtx, frame);

					Emms.Emms_C();

					if (ret == 0)
					{
						if (avCtx.Codec.Type == AvMediaType.Audio)
						{
							int64_t discarded_Samples = 0;

							ret = Discard_Samples(avCtx, frame, ref discarded_Samples);
						}

						if ((ret == Error.EAGAIN) || ((frame.Flags & AvFrameFlag.Discard) != 0))
						{
							Frame.Av_Frame_Unref(frame);

							continue;
						}
					}

					break;
				}
			}
			else
				ret = Decode_Simple_Receive_Frame(avCtx, frame);

			if (ret == Error.EOF)
				avci.Draining_Done = 1;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Supply raw packet data as input to a decoder.
		///
		/// Internally, this call will copy relevant AVCodecContext fields,
		/// which can influence decoding per-packet, and apply them when the
		/// packet is actually decoded. (For example
		/// AVCodecContext.skip_frame, which might direct the decoder to drop
		/// the frame contained by the packet sent with this function.)
		///
		/// Warning: The input buffer, avpkt->data must be
		/// AV_INPUT_BUFFER_PADDING_SIZE larger than the actual read bytes
		/// because some optimized bitstream readers read 32 or 64 bits at
		/// once and could read over the end.
		///
		/// Note: The AVCodecContext MUST have been opened with
		/// avcodec_open2() before packets may be fed to the decoder
		/// </summary>
		/********************************************************************/
		public static c_int AvCodec_Send_Packet(AvCodecContext avCtx, AvPacket avPkt)//XX 704
		{
			AvCodecInternal avci = avCtx.Internal;
			DecodeContext dc = Decode_Ctx(avci);
			c_int ret;

			if (!AvCodec_.AvCodec_Is_Open(avCtx) || !Utils_Codec.Av_Codec_Is_Decoder(avCtx.Codec))
				return Error.EINVAL;

			if (dc.Draining_Started != 0)
				return Error.EOF;

			if ((avPkt != null) && (avPkt.Size == 0) && avPkt.Data.IsNotNull)
				return Error.EINVAL;

			if ((avPkt != null) && (avPkt.Data.IsNotNull || (avPkt.Side_Data_Elems != 0)))
			{
				if (!Packet.AvPacket_Is_Empty(avci.Buffer_Pkt))
					return Error.EAGAIN;

				ret = Packet.Av_Packet_Ref(avci.Buffer_Pkt, avPkt);

				if (ret < 0)
					return ret;
			}
			else
				dc.Draining_Started = 1;

			if ((avci.Buffer_Frame.Buf[0] == null) && (dc.Draining_Started == 0))
			{
				ret = Decode_Receive_Frame_Internal(avCtx, avci.Buffer_Frame);

				if ((ret < 0) && (ret != Error.EAGAIN) && (ret != Error.EOF))
					return ret;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// avcodec_receive_frame() implementation for decoders
		/// </summary>
		/********************************************************************/
		internal static c_int FF_Decode_Receive_Frame(AvCodecContext avCtx, AvFrame frame)//XX 791
		{
			AvCodecInternal avci = avCtx.Internal;
			c_int ret;

			if (avci.Buffer_Frame.Buf[0] != null)
				Frame.Av_Frame_Move_Ref(frame, avci.Buffer_Frame);
			else
			{
				ret = Decode_Receive_Frame_Internal(avCtx, frame);

				if (ret < 0)
					return ret;
			}

			ret = Frame_Validate(avCtx, frame);

			if (ret < 0)
				goto Fail;

			if (avCtx.Codec_Type == AvMediaType.Video)
			{
				ret = Apply_Cropping(avCtx, frame);

				if (ret < 0)
					goto Fail;
			}

			avCtx.Frame_Num++;

			return 0;

			Fail:
			Frame.Av_Frame_Unref(frame);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Decode a subtitle message.
		/// Return a negative value on error, otherwise return the number
		/// of bytes used. If no subtitle could be decompressed, got_sub_ptr
		/// is zero. Otherwise, the subtitle is stored in *sub.
		/// Note that AV_CODEC_CAP_DR1 is not available for subtitle codecs.
		/// This is for simplicity, because the performance difference is
		/// expected to be negligible and reusing a get_buffer written for
		/// video codecs would probably perform badly due to a potentially
		/// very different allocation pattern.
		///
		/// Some decoders (those marked with AV_CODEC_CAP_DELAY) have a
		/// delay between input and output. This means that for some packets
		/// they will not immediately produce decoded output and need to be
		/// flushed at the end of decoding to get all the decoded data.
		/// Flushing is done by calling this function with packets with
		/// avpkt->data set to NULL and avpkt->size set to 0 until it stops
		/// returning subtitles. It is safe to flush even those decoders
		/// that are not marked with AV_CODEC_CAP_DELAY, then no subtitles
		/// will be returned.
		///
		/// Note: The AVCodecContext MUST have been opened with
		/// avcodec_open2() before packets may be fed to the decoder
		/// </summary>
		/********************************************************************/
		public static c_int AvCodec_Decode_Subtitle2(AvCodecContext avCtx, out AvSubtitle sub, out c_int got_Sub_Ptr, AvPacket avPkt)//XX 909
		{
			throw new NotImplementedException("AvCodec_Decode_Subtitle2");
		}



		/********************************************************************/
		/// <summary>
		/// Find the best pixel format to convert to given a certain source
		/// pixel format. When converting from one pixel format to another,
		/// information loss may occur. For example, when converting from
		/// RGB24 to GRAY, the color information will be lost. Similarly,
		/// other losses occur when converting from some formats to other
		/// formats. avcodec_find_best_pix_fmt_of_2() searches which of the
		/// given pixel formats should be used to suffer the least amount of
		/// loss. The pixel formats from which it chooses one, are determined
		/// by the pix_fmt_list parameter
		/// </summary>
		/********************************************************************/
		public static AvPixelFormat AvCodec_Default_Get_Format(AvCodecContext avCtx, CPointer<AvPixelFormat> fmt)//XX 980
		{
			AvCodecHwConfig config;
			c_int n;

			// If a device was supplied when the codec was opened, assume that the
			// user wants to use it
			if ((avCtx.Hw_Device_Ctx != null) && (Codec_Internal.FFCodec(avCtx.Codec).Hw_Configs != null))
			{
				AvHwDeviceContext device_Ctx = (AvHwDeviceContext)avCtx.Hw_Device_Ctx.Data;

				for (c_int i = 0; ; i++)
				{
					config = Codec_Internal.FFCodec(avCtx.Codec).Hw_Configs[i].Public;

					if (config == null)
						break;

					if ((config.Methods & AvCodecHwConfigMethod.Hw_Device_Ctx) == 0)
						continue;

					if (device_Ctx.Type != config.Device_Type)
						continue;

					for (n = 0; fmt[n] != AvPixelFormat.None; n++)
					{
						if (config.Pix_Fmt == fmt[n])
							return fmt[n];
					}
				}
			}

			// No device or other setup, so we have to choose from things which
			// don't any other external information

			// If the last element of the list is a software format, choose it
			// (this should be best software format if any exist)
			for (n = 0; fmt[n] != AvPixelFormat.None; n++)
			{
			}

			AVPixFmtDescriptor desc = PixDesc.Av_Pix_Fmt_Desc_Get(fmt[n - 1]);

			if ((desc.Flags & AvPixelFormatFlag.HwAccel) == 0)
				return fmt[n - 1];

			// Finally, traverse the list in order and choose the first entry
			// with no external dependencies (if there is no hardware configuration
			// information available then this just picks the first entry)
			for (n = 0; fmt[n] != AvPixelFormat.None; n++)
			{
				for (c_int i = 0; ; i++)
				{
					config = Utils_Codec.AvCodec_Get_Hw_Config(avCtx.Codec, i);

					if (config == null)
						break;

					if (config.Pix_Fmt == fmt[n])
						break;
				}

				if (config == null)
				{
					// No specific config available, so the decoder must be able
					// to handle this format without any additional setup
					return fmt[n];
				}

				if ((config.Methods & AvCodecHwConfigMethod.Internal) != 0)
				{
					// Usable with only internal setup
					return fmt[n];
				}
			}

			// Nothing is usable, give up
			return AvPixelFormat.None;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void FF_HwAccel_Uninit(AvCodecContext avCtx)//XX 1191
		{
			if ((avCtx.HwAccel != null) && (HwAccel_Internal.FFHwAccel(avCtx.HwAccel).Uninit != null))
				HwAccel_Internal.FFHwAccel(avCtx.HwAccel).Uninit(avCtx);

			Mem.Av_FreeP(ref avCtx.Internal.HwAccel_Priv_Data);

			avCtx.HwAccel = null;

			Buffer.Av_Buffer_Unref(ref avCtx.Hw_Frames_Ctx);
		}



		/********************************************************************/
		/// <summary>
		/// Set various frame properties from the provided packet
		/// </summary>
		/********************************************************************/
		public static c_int FF_Decode_Frame_Props_From_Pkt(AvCodecContext avCtx, AvFrame frame, AvPacket pkt)//XX 1524
		{
			SideDataMap[] sd =
			[
				new SideDataMap(AvPacketSideDataType.A53_CC, AvFrameSideDataType.A53_CC),
				new SideDataMap(AvPacketSideDataType.Afd, AvFrameSideDataType.Afd),
				new SideDataMap(AvPacketSideDataType.Dynamic_Hdr10_Plus, AvFrameSideDataType.Dynamic_Hdr_Plus),
				new SideDataMap(AvPacketSideDataType.S12M_TimeCode, AvFrameSideDataType.S12M_TimeCode),
				new SideDataMap(AvPacketSideDataType.Skip_Samples, AvFrameSideDataType.Skip_Samples),
				new SideDataMap(AvPacketSideDataType.Lcevc, AvFrameSideDataType.Lcevc),
				new SideDataMap(AvPacketSideDataType.Nb, 0)
			];

			frame.Pts = pkt.Pts;
			frame.Duration = pkt.Duration;

			c_int ret = Side_Data_Map(frame, pkt.Side_Data, pkt.Side_Data_Elems, AvCodec_.FF_Sd_Global_Map);

			if (ret < 0)
				return ret;

			ret = Side_Data_Map(frame, pkt.Side_Data, pkt.Side_Data_Elems, sd);

			if (ret < 0)
				return ret;

			Add_Metadata_From_Side_Data(pkt, frame);

			if ((pkt.Flags & AvPktFlag.Discard) != 0)
				frame.Flags |= AvFrameFlag.Discard;

			if ((avCtx.Flags & AvCodecFlag.Copy_Opaque) != 0)
			{
				ret = Buffer.Av_Buffer_Replace(ref frame.Opaque_Ref, pkt.Opaque_Ref);

				if (ret < 0)
					return ret;

				frame.Opaque = pkt.Opaque;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Set various frame properties from the provided packet
		/// </summary>
		/********************************************************************/
		public static c_int FF_Decode_Frame_Props(AvCodecContext avCtx, AvFrame frame)//XX 1566
		{
			c_int ret = Side_Data_Map(frame, avCtx.Coded_Side_Data.Array, (c_int)avCtx.Coded_Side_Data.Count, AvCodec_.FF_Sd_Global_Map);

			if (ret < 0)
				return ret;

			for (c_int i = 0; i < avCtx.Coded_Side_Data.Count; i++)
			{
				AvFrameSideData src = avCtx.Decoded_Side_Data[i];

				if (Frame.Av_Frame_Get_Side_Data(frame, src.Type) != null)
					continue;

				ret = Side_Data.Av_Frame_Side_Data_Clone(ref frame.Side_Data, ref frame.Nb_Side_Data, src, AvFrameSideDataFlag.None);

				if (ret < 0)
					return ret;
			}

			if ((Codec_Internal.FFCodec(avCtx.Codec).Caps_Internal & FFCodecCap.Sets_Frame_Props) == 0)
			{
				AvPacket pkt = avCtx.Internal.Last_Pkt_Props;

				ret = FF_Decode_Frame_Props_From_Pkt(avCtx, frame, pkt);

				if (ret < 0)
					return ret;
			}

			ret = Fill_Frame_Props(avCtx, frame);

			if (ret < 0)
				return ret;

			switch (avCtx.Codec.Type)
			{
				case AvMediaType.Video:
				{
					if ((frame.Width != 0) && (frame.Height != 0) && (ImgUtils.Av_Image_Check_Sar((c_uint)frame.Width, (c_uint)frame.Height, frame.Sample_Aspect_Ratio) < 0))
					{
						Log.Av_Log(avCtx, Log.Av_Log_Warning, "ignoring invalid SAR: %u/%u\n", frame.Sample_Aspect_Ratio.Num, frame.Sample_Aspect_Ratio.Den);

						frame.Sample_Aspect_Ratio = new AvRational(0, 1);
					}

					break;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Make sure avctx.hw_frames_ctx is set. If it's not set, the
		/// function will try to allocate it from hw_device_ctx. If that is
		/// not possible, an error message is printed, and an error code is
		/// returned
		/// </summary>
		/********************************************************************/
		public static c_int FF_Attach_Decode_Data(AvFrame frame)//XX 1643
		{
			RefStruct.Av_RefStruct_Unref(ref frame.Private_Refs);

			FrameDecodeData fdd = RefStruct.Av_RefStruct_Alloc_Ext<FrameDecodeData>(AvRefStructFlag.None, null, Decode_Data_Free);

			if (fdd == null)
				return Error.ENOMEM;

			frame.Private_Refs = fdd;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Get a buffer for a frame. This is a wrapper around
		/// AVCodecContext.get_buffer() and should be used instead calling
		/// get_buffer() directly
		/// </summary>
		/********************************************************************/
		public static c_int FF_Get_Buffer(AvCodecContext avCtx, AvFrame frame, c_int flags)//XX 1720
		{
			FFHwAccel hwAccel = HwAccel_Internal.FFHwAccel(avCtx.HwAccel);
			c_int override_Dimensions = 1;
			c_int ret;

			if (avCtx.Codec_Type == AvMediaType.Video)
			{
				if (((c_uint)avCtx.PictureSize.Width > (c_int.MaxValue - CodecConstants.Stride_Align)) || ((ret = ImgUtils.Av_Image_Check_Size2((c_uint)Macros.FFAlign(avCtx.PictureSize.Width, CodecConstants.Stride_Align), (c_uint)avCtx.PictureSize.Height, avCtx.Max_Pixels, AvPixelFormat.None, 0, avCtx)) < 0) || (avCtx.Pix_Fmt < 0))
				{
					Log.Av_Log(avCtx, Log.Av_Log_Error, "video_get_buffer: image parameters invalid\n");

					ret = Error.EINVAL;

					goto Fail;
				}

				if ((frame.Width <= 0) || (frame.Height <= 0))
				{
					frame.Width = Macros.FFMax(avCtx.PictureSize.Width, Common.Av_Ceil_RShift(avCtx.Coded_Width, avCtx.LowRes));
					frame.Height = Macros.FFMax(avCtx.PictureSize.Height, Common.Av_Ceil_RShift(avCtx.Coded_Height, avCtx.LowRes));

					override_Dimensions = 0;
				}

				if (frame.Data[0].IsNotNull || frame.Data[1].IsNotNull || frame.Data[2].IsNotNull || frame.Data[3].IsNotNull)
				{
					Log.Av_Log(avCtx, Log.Av_Log_Error, "pic->data[*] != null in get_buffer_internal\n");

					ret = Error.EINVAL;

					goto Fail;
				}
			}
			else if (avCtx.Codec_Type == AvMediaType.Audio)
			{
				if ((frame.Nb_Samples * (int64_t)avCtx.Ch_Layout.Nb_Channels) > avCtx.Max_Samples)
				{
					Log.Av_Log(avCtx, Log.Av_Log_Error, "samples per frame %d, exceeds max_samples %lld\n", frame.Nb_Samples, avCtx.Max_Samples);

					ret = Error.EINVAL;

					goto Fail;
				}
			}

			ret = FF_Decode_Frame_Props(avCtx, frame);

			if (ret < 0)
				goto Fail;

			if (hwAccel != null)
			{
				if (hwAccel.Alloc_Frame != null)
				{
					ret = hwAccel.Alloc_Frame(avCtx, frame);

					goto End;
				}
			}
			else
			{
				avCtx.Sw_Pix_Fmt = avCtx.Pix_Fmt;

				Update_Frame_Props(avCtx, frame);
			}

			ret = avCtx.Get_Buffer2(avCtx, frame, flags);

			if (ret < 0)
				goto Fail;

			Validate_AvFrame_Allocation(avCtx, frame);

			ret = FF_Attach_Decode_Data(frame);

			if (ret < 0)
				goto Fail;

			ret = Attach_Post_Process_Data(avCtx, frame);

			if (ret < 0)
				goto Fail;

			End:
			if ((avCtx.Codec_Type == AvMediaType.Video) && (override_Dimensions == 0) && ((Codec_Internal.FFCodec(avCtx.Codec).Caps_Internal & FFCodecCap.Exports_Cropping) == 0))
			{
				frame.Width = avCtx.PictureSize.Width;
				frame.Height = avCtx.PictureSize.Height;
			}

			Fail:
			if (ret < 0)
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "get_buffer() failed\n");

				Frame.Av_Frame_Unref(frame);
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal static c_int FF_Decode_Preinit(AvCodecContext avCtx)//XX 1963
		{
			AvCodecInternal avci = avCtx.Internal;
			DecodeContext dc = Decode_Ctx(avci);
			c_int ret = 0;

			dc.Initial_Pict_Type = AvPictureType.None;

			if ((avCtx.Codec_Descriptor.Props & AvCodecProp.Intra_Only) != 0)
			{
				dc.Intra_Only_Flag = AvFrameFlag.Key;

				if (avCtx.Codec_Type == AvMediaType.Video)
					dc.Initial_Pict_Type = AvPictureType.I;
			}

			// If the decoder init function was already called previously,
			// free the already allocated subtitle_header before overwriting it
			Mem.Av_FreeP(ref avCtx.Subtitle_Header);

			if ((avCtx.Codec.Max_LowRes < avCtx.LowRes) || (avCtx.LowRes < 0))
			{
				Log.Av_Log(avCtx, Log.Av_Log_Warning, "The maximum value for lowres supported by the decoder is %d\n", avCtx.Codec.Max_LowRes);

				avCtx.LowRes = avCtx.Codec.Max_LowRes;
			}

			if (avCtx.Sub_CharEnc.IsNotNull)
			{
				if (avCtx.Codec_Type != AvMediaType.Subtitle)
				{
					Log.Av_Log(avCtx, Log.Av_Log_Error, "Character encoding in only supported with subtitles codecs\n");

					return Error.EINVAL;
				}
				else if ((avCtx.Codec_Descriptor.Props & AvCodecProp.Bitmap_Sub) != 0)
				{
					Log.Av_Log(avCtx, Log.Av_Log_Warning, "Codec '%s' is bitmap-based, subtitles character encoding will be ignored\n", avCtx.Codec_Descriptor.Name);

					avCtx.Sub_CharEnc_Mode = FFSubCharEncMode.Do_Nothing;
				}
				else
				{
					// Input character encoding is set for a text based subtitle
					// codec at this point
					if (avCtx.Sub_CharEnc_Mode == FFSubCharEncMode.Automatic)
						avCtx.Sub_CharEnc_Mode = FFSubCharEncMode.Pre_Decoder;

					if (avCtx.Sub_CharEnc_Mode == FFSubCharEncMode.Pre_Decoder)
					{
						Log.Av_Log(avCtx, Log.Av_Log_Error, "Character encoding subtitles conversion needs a libavcodec built with iconv support for this codec\n");

						return Error.ENOSYS;
					}
				}
			}

			dc.Pts_Correction_Last_Pts = dc.Pts_Correction_Num_Faulty_Dts = 0;
			dc.Pts_Correction_Last_Pts = dc.Pts_Correction_Last_Dts = int64_t.MinValue;

			if (((avCtx.Flags & AvCodecFlag.Gray) != 0) && (avCtx.Codec_Descriptor.Type == AvMediaType.Video))
				Log.Av_Log(avCtx, Log.Av_Log_Warning, "gray decoding requested but not enabled at configuration time\n");

			if ((avCtx.Flags2 & AvCodecFlag2.Export_Mvs) != 0)
				avCtx.Export_Side_Data |= AvCodecExportData.Mvs;

			if ((avCtx.Side_Data_Prefer_Packet.Count == 1) && (avCtx.Side_Data_Prefer_Packet.Array[0] == -1))
				dc.Side_Data_Pref_Mask = ~0UL;
			else
			{
				for (c_uint i = 0; i < avCtx.Side_Data_Prefer_Packet.Count; i++)
				{
					c_int val = avCtx.Side_Data_Prefer_Packet.Array[i];

					if ((val < 0) || (val >= (c_int)AvPacketSideDataType.Nb))
					{
						Log.Av_Log(avCtx, Log.Av_Log_Error, "Invalid side data type: %d\n", val);

						return Error.EINVAL;
					}

					for (c_uint j = 0; AvCodec_.FF_Sd_Global_Map[j].Packet < AvPacketSideDataType.Nb; j++)
					{
						if ((c_int)AvCodec_.FF_Sd_Global_Map[j].Packet == val)
						{
							val = (c_int)AvCodec_.FF_Sd_Global_Map[j].Frame;

							// this code will need to be changed when we have more than
							// 64 frame side data types
							if (val >= 64)
							{
								Log.Av_Log(avCtx, Log.Av_Log_Error, "Side data type too big\n");

								return Error.Bug;
							}

							dc.Side_Data_Pref_Mask |= 1UL << val;
						}
					}
				}
			}

			avci.In_Pkt = Packet.Av_Packet_Alloc();
			avci.Last_Pkt_Props = Packet.Av_Packet_Alloc();

			if ((avci.In_Pkt == null) || (avci.Last_Pkt_Props == null))
				return Error.ENOMEM;

			if ((Codec_Internal.FFCodec(avCtx.Codec).Caps_Internal & FFCodecCap.Uses_ProgressFrames) != 0)
			{
				avci.Progress_Frame_Pool = RefStruct.Av_RefStruct_Pool_Alloc_Ext<ProgressInternal>(AvRefStructPoolFlag.Free_On_Init_Error, avCtx, Progress_Frame_Pool_Init_Cb, Progress_Frame_Pool_Reset_Cb, Progress_Frame_Pool_Free_Entry_Cb, null);

				if (avci.Progress_Frame_Pool == null)
					return Error.ENOMEM;
			}

			ret = Decode_Bsfs_Init(avCtx);

			if (ret < 0)
				return ret;

			if ((avCtx.Export_Side_Data & AvCodecExportData.Enhancements) == 0)
			{
				ret = LcevcDec.FF_Lcevc_Alloc(out dc.Lcevc);

				if ((ret < 0) && ((avCtx.Err_Recognition & AvEf.Explode) != 0))
					return ret;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal static AvCodecInternal FF_Decode_Internal_Alloc()//XX 2313
		{
			return Mem.Av_MAlloczObj<DecodeContext>();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal static void FF_Decode_Internal_Sync(AvCodecContext dst, AvCodecContext src)//XX 2318
		{
			DecodeContext src_Dc = Decode_Ctx(src.Internal);
			DecodeContext dst_Dc = Decode_Ctx(dst.Internal);

			dst_Dc.Initial_Pict_Type = src_Dc.Initial_Pict_Type;
			dst_Dc.Intra_Only_Flag = src_Dc.Intra_Only_Flag;

			RefStruct.Av_RefStruct_Replace(ref dst_Dc.Lcevc, src_Dc.Lcevc);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void FF_Decode_Internal_Uninit(AvCodecContext avCtx)//XX 2328
		{
			AvCodecInternal avci = avCtx.Internal;
			DecodeContext dc = Decode_Ctx(avci);

			RefStruct.Av_RefStruct_Unref(ref dc.Lcevc);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static DecodeContext Decode_Ctx(AvCodecInternal avci)//XX 103
		{
			return (DecodeContext)avci;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Apply_Param_Change(AvCodecContext avCtx, AvPacket avPkt)//XX 108
		{
			c_int ret;

			IDataContext dataContext = Packet.Av_Packet_Get_Side_Data(avPkt, AvPacketSideDataType.Param_Change);

			if (dataContext == null)
				return 0;

			if ((avCtx.Codec.Capabilities & AvCodecCap.Param_Change) == 0)
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "This decoder does not support parameter changes, but PARAM_CHANGE side data was sent to it.\n");

				ret = Error.EINVAL;
				goto Fail2;
			}

			CPointer<uint8_t> data = ((DataBufferContext)dataContext).Data;
			size_t size = ((DataBufferContext)dataContext).Size;

			if (size < 4)
				goto Fail;

			AVSideDataParamChangeFlags flags = (AVSideDataParamChangeFlags)ByteStream.ByteStream_Get_LE32(ref data);
			size -= 4;

			if ((flags & AVSideDataParamChangeFlags.Sample_Rate) != 0)
			{
				if (size < 4)
					goto Fail;

				int64_t val = ByteStream.ByteStream_Get_LE32(ref data);

				if ((val <= 0) || (val > c_int.MaxValue))
				{
					Log.Av_Log(avCtx, Log.Av_Log_Error, "Invalid sample rate");

					ret = Error.InvalidData;

					goto Fail2;
				}

				avCtx.Sample_Rate = (c_int)val;
				size -= 4;
			}

			if ((flags & AVSideDataParamChangeFlags.Dimensions) != 0)
			{
				if (size < 8)
					goto Fail;

				avCtx.PictureSize.Width = (c_int)ByteStream.ByteStream_Get_LE32(ref data);
				avCtx.PictureSize.Height = (c_int)ByteStream.ByteStream_Get_LE32(ref data);
				size -= 8;

				ret = Utils_Codec.FF_Set_Dimensions(avCtx, avCtx.PictureSize.Width, avCtx.PictureSize.Height);

				if (ret < 0)
					goto Fail2;
			}

			return 0;

			Fail:
			Log.Av_Log(avCtx, Log.Av_Log_Error, "PARAM_CHANGE side data too small.\n");
			ret = Error.InvalidData;

			Fail2:
			if (ret < 0)
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "Error applying parameter changes.\n");

				if ((avCtx.Err_Recognition & AvEf.Explode) != 0)
					return ret;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Extract_Packet_Props(AvCodecInternal avci, AvPacket pkt)//XX 169
		{
			c_int ret = 0;

			Packet.Av_Packet_Unref(avci.Last_Pkt_Props);

			if (pkt != null)
				ret = Packet.Av_Packet_Copy_Props(avci.Last_Pkt_Props, pkt);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Decode_Bsfs_Init(AvCodecContext avCtx)//XX 180
		{
			AvCodecInternal avci = avCtx.Internal;
			FFCodec codec = Codec_Internal.FFCodec(avCtx.Codec);

			if (avci.Bsf != null)
				return 0;

			c_int ret = Bsf.Av_Bsf_List_Parse_Str(codec.Bsfs, out avci.Bsf);

			if (ret < 0)
			{
				Log.Av_Log(avCtx, Log.Av_Log_Error, "Error parsing decoder bitstream filters '%s': %s\n", codec.Bsfs, Error.Av_Err2Str(ret));

				if (ret != Error.ENOMEM)
					ret = Error.Bug;

				goto Fail;
			}

			// We do not currently have an API for passing the input timebase into decoders,
			// but no filters used here should actually need it.
			// So we make up some plausible-looking number (the MPEG 90kHz timebase)
			avci.Bsf.Time_Base_In = new AvRational(1, 90000);

			ret = Codec_Par.AvCodec_Parameters_From_Context(avci.Bsf.Par_In, avCtx);

			if (ret < 0)
				goto Fail;

			ret = Bsf.Av_Bsf_Init(avci.Bsf);

			if (ret < 0)
				goto Fail;

			return 0;

			Fail:
			Bsf.Av_Bsf_Free(ref avci.Bsf);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Decode_Get_Packet(AvCodecContext avCtx, AvPacket pkt)//XX 220
		{
			AvCodecInternal avci = avCtx.Internal;

			c_int ret = Bsf.Av_Bsf_Receive_Packet(avci.Bsf, pkt);

			if (ret < 0)
				return ret;

			if ((Codec_Internal.FFCodec(avCtx.Codec).Caps_Internal & FFCodecCap.Sets_Frame_Props) == 0)
			{
				ret = Extract_Packet_Props(avCtx.Internal, pkt);

				if (ret < 0)
					goto Finish;
			}

			ret = Apply_Param_Change(avCtx, pkt);

			if (ret < 0)
				goto Finish;

			return 0;

			Finish:
			Packet.Av_Packet_Unref(pkt);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Attempt to guess proper monotonic timestamps for decoded video
		/// frames which might have incorrect times. Input timestamps may
		/// wrap around, in which case the output will as well
		/// </summary>
		/********************************************************************/
		private static int64_t Guess_Correct_Pts(DecodeContext dc, int64_t reordered_Pts, int64_t dts)//XX 287
		{
			int64_t pts = UtilConstants.Av_NoPts_Value;

			if (dts != UtilConstants.Av_NoPts_Value)
			{
				dc.Pts_Correction_Num_Faulty_Dts += dts <= dc.Pts_Correction_Last_Dts ? 1 : 0;
				dc.Pts_Correction_Last_Dts = dts;
			}
			else if (reordered_Pts != UtilConstants.Av_NoPts_Value)
				dc.Pts_Correction_Last_Dts = reordered_Pts;

			if (reordered_Pts != UtilConstants.Av_NoPts_Value)
			{
				dc.Pts_Correction_Num_Faulty_Pts += reordered_Pts <= dc.Pts_Correction_Last_Pts ? 1 : 0;
				dc.Pts_Correction_Last_Pts = reordered_Pts;
			}
			else if (dts != UtilConstants.Av_NoPts_Value)
				dc.Pts_Correction_Last_Pts = dts;

			if (((dc.Pts_Correction_Num_Faulty_Pts <= dc.Pts_Correction_Num_Faulty_Dts) || (dts == UtilConstants.Av_NoPts_Value)) && (reordered_Pts != UtilConstants.Av_NoPts_Value))
				pts = reordered_Pts;
			else
				pts = dts;

			return pts;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Discard_Samples(AvCodecContext avCtx, AvFrame frame, ref int64_t discarded_Samples)//XX 313
		{
			AvCodecInternal avci = avCtx.Internal;
			uint32_t discard_Padding = 0;
			uint8_t skip_Reason = 0;
			uint8_t discard_Reason = 0;

			AvFrameSideData side = Frame.Av_Frame_Get_Side_Data(frame, AvFrameSideDataType.Skip_Samples);
			DataBufferContext dataBuffer = (DataBufferContext)side?.Data;

			if ((side != null) && (dataBuffer.Size >= 10))
			{
				avci.Skip_Samples = (c_int)IntReadWrite.Av_RL32(dataBuffer.Data);
				avci.Skip_Samples = Macros.FFMax(0, avci.Skip_Samples);
				discard_Padding = IntReadWrite.Av_RL32(dataBuffer.Data + 4);

				Log.Av_Log(avCtx, Log.Av_Log_Debug, "skip %d / discard %d samples due to side data\n", avci.Skip_Samples, (c_int)discard_Padding);

				skip_Reason = IntReadWrite.Av_RL8(dataBuffer.Data + 8);
				discard_Reason = IntReadWrite.Av_RL8(dataBuffer.Data + 9);
			}

			if ((avCtx.Flags2 & AvCodecFlag2.Skip_Manual) != 0)
			{
				if ((side == null) && ((avci.Skip_Samples != 0) || (discard_Padding != 0)))
					side = Frame.Av_Frame_New_Side_Data(frame, AvFrameSideDataType.Skip_Samples, new DataBufferContext(new CPointer<uint8_t>(10), 10));

				if ((side != null) && ((avci.Skip_Samples != 0) || (discard_Padding != 0)))
				{
					IntReadWrite.Av_WL32(dataBuffer.Data, (c_uint)avci.Skip_Samples);
					IntReadWrite.Av_WL32(dataBuffer.Data + 4, discard_Padding);
					IntReadWrite.Av_WL8(dataBuffer.Data + 8, skip_Reason);
					IntReadWrite.Av_WL8(dataBuffer.Data + 9, discard_Reason);

					avci.Skip_Samples = 0;
				}

				return 0;
			}

			Frame.Av_Frame_Remove_Side_Data(frame, AvFrameSideDataType.Skip_Samples);

			if ((frame.Flags & AvFrameFlag.Discard) != 0)
			{
				avci.Skip_Samples = Macros.FFMax(0, avci.Skip_Samples - frame.Nb_Samples);
				discarded_Samples += frame.Nb_Samples;

				return Error.EAGAIN;
			}

			if (avci.Skip_Samples > 0)
			{
				if (frame.Nb_Samples <= avci.Skip_Samples)
				{
					discarded_Samples += frame.Nb_Samples;
					avci.Skip_Samples -= frame.Nb_Samples;

					Log.Av_Log(avCtx, Log.Av_Log_Debug, "skip whole frame, skip left: %d\n", avci.Skip_Samples);

					return Error.EAGAIN;
				}
				else
				{
					SampleFmt.Av_Samples_Copy(frame.Extended_Data, frame.Extended_Data, 0, avci.Skip_Samples, frame.Nb_Samples - avci.Skip_Samples, avCtx.Ch_Layout.Nb_Channels, frame.Format.Sample);

					if ((avCtx.Pkt_TimeBase.Num != 0) && (avCtx.Sample_Rate != 0))
					{
						int64_t diff_Ts = Mathematics.Av_Rescale_Q(avci.Skip_Samples, new AvRational(1, avCtx.Sample_Rate), avCtx.Pkt_TimeBase);

						if (frame.Pts != UtilConstants.Av_NoPts_Value)
							frame.Pts += diff_Ts;

						if (frame.Pkt_Dts != UtilConstants.Av_NoPts_Value)
							frame.Pkt_Dts += diff_Ts;

						if (frame.Duration >= diff_Ts)
							frame.Duration -= diff_Ts;
					}
					else
						Log.Av_Log(avCtx, Log.Av_Log_Warning, "Could not update timestamps for skipped samples.\n");

					Log.Av_Log(avCtx, Log.Av_Log_Debug, "skip %d/%d samples\n", avci.Skip_Samples, frame.Nb_Samples);

					discarded_Samples += avci.Skip_Samples;
					frame.Nb_Samples -= avci.Skip_Samples;
					avci.Skip_Samples = 0;
				}
			}

			if ((discard_Padding > 0) && (discard_Padding <= frame.Nb_Samples))
			{
				if (discard_Padding == frame.Nb_Samples)
				{
					discarded_Samples += frame.Nb_Samples;

					return Error.EAGAIN;
				}
				else
				{
					if ((avCtx.Pkt_TimeBase.Num != 0) && (avCtx.Sample_Rate != 0))
					{
						int64_t diff_Ts = Mathematics.Av_Rescale_Q(frame.Nb_Samples - discard_Padding, new AvRational(1, avCtx.Sample_Rate), avCtx.Pkt_TimeBase);
						frame.Duration = diff_Ts;
					}
					else
						Log.Av_Log(avCtx, Log.Av_Log_Warning, "Could not update timestamps for discarded samples.\n");

					Log.Av_Log(avCtx, Log.Av_Log_Debug, "discard %d/%d samples\n", (c_int)discard_Padding, frame.Nb_Samples);

					frame.Nb_Samples -= (c_int)discard_Padding;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// The core of the receive_frame_wrapper for the decoders
		/// implementing the simple API. Certain decoders might consume
		/// partial packets without returning any output, so this function
		/// needs to be called in a loop until it returns EAGAIN
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_int Decode_Simple_Internal(AvCodecContext avCtx, AvFrame frame, ref int64_t discarded_Samples)//XX 411
		{
			AvCodecInternal avci = avCtx.Internal;
			DecodeContext dc = Decode_Ctx(avci);
			AvPacket pkt = avci.In_Pkt;
			FFCodec codec = Codec_Internal.FFCodec(avCtx.Codec);
			c_int ret = 0;

			if (pkt.Data.IsNull && (avci.Draining == 0))
			{
				Packet.Av_Packet_Unref(pkt);

				ret = FF_Decode_Get_Packet(avCtx, pkt);

				if ((ret < 0) && (ret != Error.EOF))
					return ret;
			}

			// Some codecs (at least wma lossless) will crash when feeding drain packets
			// after EOF was signaled
			if (avci.Draining != 0)
				return Error.EOF;

			if (pkt.Data.IsNull && ((avCtx.Codec.Capabilities & AvCodecCap.Delay) == 0))
				return Error.EOF;

			c_int got_Frame = 0;

			frame.Pict_Type = dc.Initial_Pict_Type;
			frame.Flags |= dc.Intra_Only_Flag;

			c_int consumed = codec.Cb.Decode(avCtx, frame, out got_Frame, pkt);

			if ((codec.Caps_Internal & FFCodecCap.Sets_Pkt_Dts) == 0)
				frame.Pkt_Dts = pkt.Dts;

			Emms.Emms_C();

			if (avCtx.Codec.Type == AvMediaType.Video)
				ret = (got_Frame == 0) || (frame.Flags & AvFrameFlag.Discard) != 0 ? Error.EAGAIN : 0;
			else if (avCtx.Codec.Type == AvMediaType.Audio)
				ret = got_Frame == 0 ? Error.EAGAIN : Discard_Samples(avCtx, frame, ref discarded_Samples);

			if (ret == Error.EAGAIN)
				Frame.Av_Frame_Unref(frame);

			if (consumed < 0)
				ret = consumed;

			if ((consumed >= 0) && (avCtx.Codec.Type == AvMediaType.Video))
				consumed = pkt.Size;

			if (ret == Error.EAGAIN)
				ret = 0;

			// Do not stop draining when got_frame != 0 or ret < 0
			if ((avci.Draining != 0) && (got_Frame == 0))
			{
				if (ret < 0)
				{
					// Prevent infinite loop if a decoder wrongly always return error on draining
					// Reasonable nb_errors_max = maximum b frames + thread count
					c_int nb_Errors_Max = 20 + ((avCtx.Active_Thread_Type & FFThread.Frame) != 0 ? avCtx.Thread_Count : 1);

					if (Decode_Ctx(avci).Nb_Draining_Errors++ >= nb_Errors_Max)
					{
						Log.Av_Log(avCtx, Log.Av_Log_Error, "Too many error when draining, this is a bug. Stop draining and force EOF.\n");

						avci.Draining_Done = 1;
						ret = Error.Bug;
					}
				}
				else
					avci.Draining = 1;
			}

			if ((consumed >= pkt.Size) || (ret < 0))
				Packet.Av_Packet_Unref(pkt);
			else
			{
				pkt.Data += consumed;
				pkt.Size -= consumed;
				pkt.Pts = UtilConstants.Av_NoPts_Value;
				pkt.Dts = UtilConstants.Av_NoPts_Value;

				if ((codec.Caps_Internal & FFCodecCap.Sets_Frame_Props) == 0)
				{
					avci.Last_Pkt_Props.Pts = UtilConstants.Av_NoPts_Value;
					avci.Last_Pkt_Props.Dts = UtilConstants.Av_NoPts_Value;
				}
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Detect_ColorSpace(AvCodecContext avCtx, AvFrame frame)//XX 551
		{
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Fill_Frame_Props(AvCodecContext avCtx, AvFrame frame)//XX 557
		{
			if (frame.Color_Primaries == AvColorPrimaries.Unspecified)
				frame.Color_Primaries = avCtx.Color_Primaries;

			if (frame.Color_Trc == AvColorTransferCharacteristic.Unspecified)
				frame.Color_Trc = avCtx.Color_Trc;

			if (frame.ColorSpace == AvColorSpace.Unspecified)
				frame.ColorSpace = avCtx.ColorSpace;

			if (frame.Color_Range == AvColorRange.Unspecified)
				frame.Color_Range = avCtx.Color_Range;

			if (frame.Chroma_Location == AvChromaLocation.Unspecified)
				frame.Chroma_Location = avCtx.Chroma_Sample_Location;

			if (frame.Alpha_Mode == AvAlphaMode.Unspecified)
				frame.Alpha_Mode = avCtx.Alpha_Mode;

			if (avCtx.Codec_Type == AvMediaType.Video)
			{
				if (frame.Sample_Aspect_Ratio.Num == 0)
					frame.Sample_Aspect_Ratio = avCtx.Sample_Aspect_Ratio;

				if (frame.Format.Pixel == AvPixelFormat.None)
					frame.Format.Pixel = avCtx.Pix_Fmt;
			}
			else if (avCtx.Codec.Type == AvMediaType.Audio)
			{
				if (frame.Format.Sample == AvSampleFormat.None)
					frame.Format.Sample = avCtx.Sample_Fmt;

				if (frame.Ch_Layout.Nb_Channels == 0)
				{
					c_int ret = Channel_Layout.Av_Channel_Layout_Copy(frame.Ch_Layout, avCtx.Ch_Layout);

					if (ret < 0)
						return ret;
				}

				if (frame.Sample_Rate == 0)
					frame.Sample_Rate = avCtx.Sample_Rate;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Decode_Simple_Receive_Frame(AvCodecContext avCtx, AvFrame frame)//XX 592
		{
			int64_t discarded_Samples = 0;

			while (frame.Buf[0] == null)
			{
				if (discarded_Samples > avCtx.Max_Samples)
					return Error.EAGAIN;

				c_int ret = Decode_Simple_Internal(avCtx, frame, ref discarded_Samples);

				if (ret < 0)
					return ret;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Decode_Receive_Frame_Internal(AvCodecContext avCtx, AvFrame frame)//XX 644
		{
			AvCodecInternal avci = avCtx.Internal;
			DecodeContext dc = Decode_Ctx(avci);
			c_int ret;

			if ((avCtx.Active_Thread_Type & FFThread.Frame) != 0)
				ret = PThread_Frame.FF_Thread_Receive_Frame(avCtx, frame);
			else
				ret = FF_Decode_Receive_Frame_Internal(avCtx, frame);

			// Preserve ret
			c_int ok = Detect_ColorSpace(avCtx, frame);

			if (ok < 0)
			{
				Frame.Av_Frame_Unref(frame);

				return ok;
			}

			if (ret == 0)
			{
				if (avCtx.Codec_Type == AvMediaType.Video)
				{
					if (frame.Width == 0)
						frame.Width = avCtx.PictureSize.Width;

					if (frame.Height == 0)
						frame.Height = avCtx.PictureSize.Height;
				}

				ret = Fill_Frame_Props(avCtx, frame);

				if (ret < 0)
				{
					Frame.Av_Frame_Unref(frame);

					return ret;
				}

				frame.Best_Effort_Timestamp = Guess_Correct_Pts(dc, frame.Pts, frame.Pkt_Dts);

				if (frame.Private_Refs != null)
				{
					FrameDecodeData fdd = (FrameDecodeData)frame.Private_Refs;

					if (fdd.Post_Process != null)
					{
						ret = fdd.Post_Process(avCtx, frame);

						if (ret < 0)
						{
							Frame.Av_Frame_Unref(frame);

							return ret;
						}
					}
				}
			}

			// Free the per-frame decode data
			RefStruct.Av_RefStruct_Unref(ref frame.Private_Refs);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Apply_Cropping(AvCodecContext avCtx, AvFrame frame)//XX 737
		{
			// Make sure we are noisy about decoders returning invalid cropping data
			if ((frame.Crop_Left >= (c_int.MaxValue - frame.Crop_Right)) || (frame.Crop_Top >= (c_int.MaxValue - frame.Crop_Bottom)) ||
			    ((frame.Crop_Left + frame.Crop_Right) >= (size_t)frame.Width) || ((frame.Crop_Top + frame.Crop_Bottom) >= (size_t)frame.Height))
			{
				Log.Av_Log(avCtx, Log.Av_Log_Warning, $"Invalid cropping information set by a decoder: %{UtilConstants.Size_Specifier}/%{UtilConstants.Size_Specifier}/%{UtilConstants.Size_Specifier}/%{UtilConstants.Size_Specifier} (frame size %dx%d). This is a bug, please report it\n", frame.Crop_Left, frame.Crop_Right, frame.Crop_Top, frame.Crop_Bottom, frame.Width, frame.Height);

				frame.Crop_Left = 0;
				frame.Crop_Right = 0;
				frame.Crop_Top = 0;
				frame.Crop_Bottom = 0;

				return 0;
			}

			if (avCtx.Apply_Cropping == 0)
				return 0;

			return Frame.Av_Frame_Apply_Cropping(frame, (avCtx.Flags & AvCodecFlag.Unaligned) != 0 ? AvFrameCrop.Unaligned : AvFrameCrop.None);
		}



		/********************************************************************/
		/// <summary>
		/// Make sure frames returned to the caller are valid
		/// </summary>
		/********************************************************************/
		private static c_int Frame_Validate(AvCodecContext avCtx, AvFrame frame)//XX 765
		{
			if ((frame.Buf[0] == null) || ((frame.Format.Pixel < 0) && (frame.Format.Sample < 0)))
				goto Fail;

			switch (avCtx.Codec_Type)
			{
				case AvMediaType.Video:
				{
					if ((frame.Width <= 0) || (frame.Height <= 0))
						goto Fail;

					break;
				}

				case AvMediaType.Audio:
				{
					if ((Channel_Layout.Av_Channel_Layout_Check(frame.Ch_Layout) == 0) || (frame.Sample_Rate <= 0))
						goto Fail;

					break;
				}
			}

			return 0;

			Fail:
			Log.Av_Log(avCtx, Log.Av_Log_Error, "An invalid frame was output by a decoder. This is a bug, please report it.\n");

			return Error.Bug;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static AvPacketSideData Packet_Side_Data_Get(CPointer<AvPacketSideData> sd, c_int nb_Sd, AvPacketSideDataType type)//XX 1342
		{
			for (c_int i = 0; i < nb_Sd; i++)
			{
				if (sd[i].Type == type)
					return sd[i];
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Side_Data_Stereo3D_Merge(AvFrameSideData sd_Frame, AvPacketSideData sd_Pkt)//XX 1358
		{
			c_int ret = Buffer.Av_Buffer_Make_Writable(ref sd_Frame.Buf);

			if (ret < 0)
				return ret;

			sd_Frame.Data = sd_Frame.Buf.Data;

			AvStereo3D dst = (AvStereo3D)sd_Frame.Data;
			AvStereo3D src = (AvStereo3D)sd_Pkt.Data;

			if (dst.Type == AvStereo3DType.Unspec)
				dst.Type = src.Type;

			if (dst.View == AvStereo3DView.Unspec)
				dst.View = src.View;

			if (dst.Primary_Eye == AvStereo3DPrimaryEye.None)
				dst.Primary_Eye = src.Primary_Eye;

			if (dst.Baseline == 0)
				dst.Baseline = src.Baseline;

			if (dst.Horizontal_Disparity_Adjustment.Num == 0)
				dst.Horizontal_Disparity_Adjustment = src.Horizontal_Disparity_Adjustment;

			if (dst.Horizontal_Field_Of_View.Num == 0)
				dst.Horizontal_Field_Of_View = src.Horizontal_Field_Of_View;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Side_Data_Exif_Parse(AvFrame dst, AvPacketSideData sd_Pkt)//XX 1394
		{
			AvFrameSideData sd_Frame;
			AvBufferRef buf = null;

			DataBufferContext dataBuffer = (DataBufferContext)sd_Pkt.Data;

			c_int ret = Exif.Av_Exif_Parse_Buffer(null, dataBuffer.Data, dataBuffer.Size, out AvExifMetadata ifd, AvExifHeaderMode.Tiff_Header);

			if (ret < 0)
				return ret;

			ret = Exif.Av_Exif_Get_Entry(null, ifd, (uint16_t)Exif.Av_Exif_Get_Tag_Id("Orientation".ToCharPointer()), AvExifFlag.None, out AvExifEntry entry);

			if (ret < 0)
				goto End;

			if (entry == null)
			{
				ret = Exif.Av_Exif_Ifd_To_Dict(null, ifd, ref dst.Metadata);

				if (ret < 0)
					goto End;

				sd_Frame = Side_Data.Av_Frame_Side_Data_New(ref dst.Side_Data, ref dst.Nb_Side_Data, AvFrameSideDataType.Exif, dataBuffer.Size, AvFrameSideDataFlag.None);

				if (sd_Frame != null)
					sd_Pkt.Data.CopyTo(sd_Frame.Data);

				ret = sd_Frame != null ? 0 : Error.ENOMEM;

				goto End;
			}
			else if ((entry.Count <= 0) || (entry.Type != AvTiffDataType.Short))
			{
				ret = Error.InvalidData;

				goto End;
			}

			// If a display matrix already exists in the frame, give it priority
			if (Side_Data.Av_Frame_Side_Data_Get(dst.Side_Data, dst.Nb_Side_Data, AvFrameSideDataType.DisplayMatrix) != null)
				goto Finish;

			sd_Frame = Side_Data.Av_Frame_Side_Data_New(ref dst.Side_Data, ref dst.Nb_Side_Data, AvFrameSideDataType.DisplayMatrix, 9 * sizeof(int32_t), AvFrameSideDataFlag.None);

			if (sd_Frame == null)
			{
				ret = Error.ENOMEM;

				goto End;
			}

			ret = Exif.Av_Exif_Orientation_To_Matrix(((DataBufferContext)sd_Frame.Data).Data.Cast<uint8_t, int32_t>(), (c_int)entry.Value.UInt[0]);

			if (ret < 0)
				goto End;

			Finish:
			Exif.Av_Exif_Remove_Entry(null, ifd, entry.Id, AvExifFlag.None);

			ret = Exif.Av_Exif_Ifd_To_Dict(null, ifd, ref dst.Metadata);

			if (ret < 0)
				goto End;

			ret = Exif.Av_Exif_Write(null, ifd, out buf, AvExifHeaderMode.Tiff_Header);

			if (ret < 0)
				goto End;

			if (Side_Data.Av_Frame_Side_Data_Add(ref dst.Side_Data, ref dst.Nb_Side_Data, AvFrameSideDataType.Exif, ref buf, AvFrameSideDataFlag.None) == null)
			{
				ret = Error.ENOMEM;

				goto End;
			}

			ret = 0;

			End:
			Buffer.Av_Buffer_Unref(ref buf);
			Exif.Av_Exif_Free(ifd);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Side_Data_Map(AvFrame dst, CPointer<AvPacketSideData> sd_Src, c_int nb_Sd_Src, CPointer<SideDataMap> map)//XX 1466
		{
			for (c_int i = 0; map[i].Packet < AvPacketSideDataType.Nb; i++)
			{
				AvPacketSideDataType type_Pkt = map[i].Packet;
				AvFrameSideDataType type_Frame = map[i].Frame;

				AvPacketSideData sd_Pkt = Packet_Side_Data_Get(sd_Src, nb_Sd_Src, type_Pkt);

				if (sd_Pkt == null)
					continue;

				AvFrameSideData sd_Frame = Frame.Av_Frame_Get_Side_Data(dst, type_Frame);

				if (sd_Frame != null)
				{
					if (type_Frame == AvFrameSideDataType.Stereo3D)
					{
						c_int ret = Side_Data_Stereo3D_Merge(sd_Frame, sd_Pkt);

						if (ret < 0)
							return ret;
					}

					continue;
				}

				switch (type_Pkt)
				{
					case AvPacketSideDataType.Exif:
					{
						c_int ret = Side_Data_Exif_Parse(dst, sd_Pkt);

						if (ret < 0)
							return ret;

						break;
					}

					default:
					{
						sd_Frame = Frame.Av_Frame_New_Side_Data(dst, type_Frame, sd_Pkt.Data);

						if (sd_Frame == null)
							return Error.ENOMEM;

						sd_Pkt.Data.CopyTo(sd_Frame.Data);
						break;
					}
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Add_Metadata_From_Side_Data(AvPacket avPkt, AvFrame frame)//XX 1512
		{
			AvDictionary frame_Md = frame.Metadata;

			StringBufferContext side_Metadata = (StringBufferContext)Packet.Av_Packet_Get_Side_Data(avPkt, AvPacketSideDataType.Strings_Metadata);

			return Packet.Av_Packet_Unpack_Dictionary(side_Metadata != null ? side_Metadata.String : default, side_Metadata != null ? side_Metadata.Size : 0, ref frame_Md);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Validate_AvFrame_Allocation(AvCodecContext avCtx, AvFrame frame)//XX 1611
		{
			if (avCtx.Codec_Type == AvMediaType.Video)
			{
				c_int num_Planes = PixDesc.Av_Pix_Fmt_Count_Planes(frame.Format.Pixel);
				AVPixFmtDescriptor desc = PixDesc.Av_Pix_Fmt_Desc_Get(frame.Format.Pixel);
				AvPixelFormatFlag flags = desc != null ? desc.Flags : AvPixelFormatFlag.None;

				if ((num_Planes == 1) && ((flags & AvPixelFormatFlag.Pal) != 0))
					num_Planes = 2;

				for (c_int i = 0; i < num_Planes; i++)
				{
				}

				// For formats without data like hwaccel allow unused pointers to be non-NULL
				for (c_int i = num_Planes; (num_Planes > 0) && (i < (c_int)Macros.FF_Array_Elems(frame.Data)); i++)
				{
					if (frame.Data[i].IsNotNull)
						Log.Av_Log(avCtx, Log.Av_Log_Error, "Buffer returned by get_buffer2() did not zero unused plane pointers\n");

					frame.Data[i].SetToNull();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Decode_Data_Free(AvRefStructOpaque unused, IRefCount obj)//XX 1632
		{
			FrameDecodeData fdd = (FrameDecodeData)obj;

			if (fdd.Post_Process_Opaque_Free != null)
				fdd.Post_Process_Opaque_Free(fdd.Post_Process_Opaque);

			if (fdd.HwAccel_Priv != null)
				fdd.HwAccel_Priv_Free(fdd.HwAccel_Priv);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Update_Frame_Props(AvCodecContext avCtx, AvFrame frame)//XX 1659
		{
			AvCodecInternal avci = avCtx.Internal;
			DecodeContext dc = Decode_Ctx(avci);

			dc.Lcevc_Frame = (dc.Lcevc != null) && (avCtx.Codec_Type == AvMediaType.Video) && (Frame.Av_Frame_Get_Side_Data(frame, AvFrameSideDataType.Lcevc) != null) ? 1 : 0;

			if (dc.Lcevc_Frame != 0)
			{
				dc.Width = frame.Width;
				dc.Height = frame.Height;

				frame.Width = frame.Width * 2 / Macros.FFMax(frame.Sample_Aspect_Ratio.Den, 1);
				frame.Height = frame.Height * 2 / Macros.FFMax(frame.Sample_Aspect_Ratio.Num, 1);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Attach_Post_Process_Data(AvCodecContext avCtx, AvFrame frame)//XX 1675
		{
			AvCodecInternal avci = avCtx.Internal;
			DecodeContext dc = Decode_Ctx(avci);

			if (dc.Lcevc_Frame != 0)
			{
				FrameDecodeData fdd = (FrameDecodeData)frame.Private_Refs;

				FFLcevcFrame frame_Ctx = Mem.Av_MAlloczObj<FFLcevcFrame>();

				if (frame_Ctx == null)
					return Error.ENOMEM;

				frame_Ctx.Frame = Frame.Av_Frame_Alloc();

				if (frame_Ctx.Frame == null)
				{
					Mem.Av_Free(frame_Ctx);

					return Error.ENOMEM;
				}

				frame_Ctx.Lcevc = RefStruct.Av_RefStruct_Ref(dc.Lcevc);
				frame_Ctx.Frame.Width = frame.Width;
				frame_Ctx.Frame.Height = frame.Height;
				frame_Ctx.Frame.Format = frame.Format;

				frame.Width = dc.Width;
				frame.Height = dc.Height;

				c_int ret = avCtx.Get_Buffer2(avCtx, frame_Ctx.Frame, 0);

				if (ret < 0)
				{
					LcevcDec.FF_Lcevc_Unref(frame_Ctx);

					return ret;
				}

				Validate_AvFrame_Allocation(avCtx, frame_Ctx.Frame);

				fdd.Post_Process_Opaque = frame_Ctx;
				fdd.Post_Process_Opaque_Free = LcevcDec.FF_Lcevc_Unref;
				fdd.Post_Process = LcevcDec.FF_Lcevc_Process;
			}

			dc.Lcevc_Frame = 0;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Progress_Frame_Pool_Init_Cb(AvRefStructOpaque opaque, IOpaque obj)//XX 1930
		{
			AvCodecContext avCtx = (AvCodecContext)opaque.Nc;
			ProgressInternal progress = (ProgressInternal)obj;

			c_int ret = ThreadProgress_.FF_Thread_Progress_Init(progress.Progress, (avCtx.Active_Thread_Type & FFThread.Frame) != 0 ? 1 : 0);

			if (ret < 0)
				return ret;

			progress.F = Frame.Av_Frame_Alloc();

			if (progress.F == null)
				return Error.ENOMEM;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Progress_Frame_Pool_Reset_Cb(AvRefStructOpaque unused, IOpaque obj)//XX 1947
		{
			ProgressInternal progress = (ProgressInternal)obj;

			ThreadProgress_.FF_Thread_Progress_Reset(progress.Progress);

			Frame.Av_Frame_Unref(progress.F);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Progress_Frame_Pool_Free_Entry_Cb(AvRefStructOpaque opaque, IOpaque obj)//XX 1955
		{
			ProgressInternal progress = (ProgressInternal)obj;

			ThreadProgress_.FF_Thread_Progress_Destroy(progress.Progress);

			Frame.Av_Frame_Free(ref progress.F);
		}
		#endregion
	}
}
