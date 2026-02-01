/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class Frame
	{
		private const c_int Align = 32;

		/********************************************************************/
		/// <summary>
		/// Allocate an AVFrame and set its fields to default values. The
		/// resulting struct must be freed using av_frame_free()
		/// </summary>
		/********************************************************************/
		public static AvFrame Av_Frame_Alloc()//XX 52
		{
			AvFrame frame = Mem.Av_MAllocObj<AvFrame>();

			if (frame == null)
				return null;

			Get_Frame_Defaults(frame);

			return frame;
		}



		/********************************************************************/
		/// <summary>
		/// Free the frame and any dynamically allocated objects in it,
		/// e.g. extended_data. If the frame is reference counted, it will be
		/// unreferenced first
		/// </summary>
		/********************************************************************/
		public static void Av_Frame_Free(ref AvFrame frame)//XX 64
		{
			if (frame == null)
				return;

			Av_Frame_Unref(frame);
			Mem.Av_FreeP(ref frame);
		}



		/********************************************************************/
		/// <summary>
		/// Allocate new buffer(s) for audio or video data.
		///
		/// The following fields must be set on frame before calling this
		/// function:
		/// - format (pixel format for video, sample format for audio)
		/// - width and height for video
		/// - nb_samples and ch_layout for audio
		///
		/// This function will fill AVFrame.data and AVFrame.buf arrays and,
		/// if necessary, allocate and fill AVFrame.extended_data and
		/// AVFrame.extended_buf. For planar formats, one buffer will be
		/// allocated for each plane.
		///
		/// Warning: if frame already has been allocated, calling this
		///          function will leak memory. In addition, undefined
		///          behavior can occur in certain cases
		/// </summary>
		/********************************************************************/
		public static c_int Av_Frame_Get_Buffer(AvFrame frame, c_int align)//XX 206
		{
			if ((frame.Format.Pixel < 0) && (frame.Format.Sample < 0))
				return Error.EINVAL;

			if ((frame.Width > 0) && (frame.Height > 0))
				return Get_Video_Buffer(frame, align);
			else if ((frame.Nb_Samples > 0) && (Channel_Layout.Av_Channel_Layout_Check(frame.Ch_Layout) != 0))
				return Get_Audio_Buffer(frame, align);

			return Error.EINVAL;
		}



		/********************************************************************/
		/// <summary>
		/// Set up a new reference to the data described by the source frame.
		///
		/// Copy frame properties from src to dst and create a new reference
		/// for each AVBufferRef from src.
		///
		/// If src is not reference counted, new buffers are allocated and
		/// the data is copied
		/// </summary>
		/********************************************************************/
		public static c_int Av_Frame_Ref(AvFrame dst, AvFrame src)//XX 278
		{
			dst.Format = src.Format;
			dst.Width = src.Width;
			dst.Height = src.Height;
			dst.Nb_Samples = src.Nb_Samples;

			c_int ret = Frame_Copy_Props(dst, src, 0);

			if (ret < 0)
				goto Fail;

			ret = Channel_Layout.Av_Channel_Layout_Copy(dst.Ch_Layout, src.Ch_Layout);

			if (ret < 0)
				goto Fail;

			// Duplicate the frame data if it's not refcounted
			if (src.Buf[0] == null)
			{
				ret = Av_Frame_Get_Buffer(dst, 0);

				if (ret < 0)
					goto Fail;

				ret = Av_Frame_Copy(dst, src);

				if (ret < 0)
					goto Fail;

				return 0;
			}

			// Ref the buffers
			for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(src.Buf); i++)
			{
				if (src.Buf[i] == null)
					continue;

				dst.Buf[i] = Buffer.Av_Buffer_Ref(src.Buf[i]);

				if (dst.Buf[i] == null)
				{
					ret = Error.ENOMEM;

					goto Fail;
				}
			}

			if (src.Extended_Buf.IsNotNull)
			{
				dst.Extended_Buf = Mem.Av_CAllocObj<AvBufferRef>((size_t)src.Nb_Extended_Buf);

				if (dst.Extended_Buf.IsNull)
				{
					ret = Error.ENOMEM;

					goto Fail;
				}

				dst.Nb_Extended_Buf = src.Nb_Extended_Buf;

				for (c_int i = 0; i < src.Nb_Extended_Buf; i++)
				{
					dst.Extended_Buf[i] = Buffer.Av_Buffer_Ref(src.Extended_Buf[i]);

					if (dst.Extended_Buf[i] == null)
					{
						ret = Error.ENOMEM;

						goto Fail;
					}
				}
			}

			if (src.Hw_Frames_Ctx != null)
			{
				dst.Hw_Frames_Ctx = Buffer.Av_Buffer_Ref(src.Hw_Frames_Ctx);

				if (dst.Hw_Frames_Ctx == null)
				{
					ret = Error.ENOMEM;

					goto Fail;
				}
			}

			// Duplicate extended data
			if (src.Extended_Data != src.Data)
			{
				c_int ch = dst.Ch_Layout.Nb_Channels;

				if ((ch <= 0) || (ch > (c_int)(size_t.MaxValue / (size_t)dst.Extended_Data.Length)))
				{
					ret = Error.EINVAL;

					goto Fail;
				}

				dst.Extended_Data = Mem.Av_MemDup(src.Extended_Data, (size_t)(dst.Extended_Data.Length * ch));

				if (dst.Extended_Data.IsNull)
				{
					ret = Error.ENOMEM;

					goto Fail;
				}
			}
			else
				dst.Extended_Data = dst.Data;

			CMemory.memcpy(dst.Data, src.Data, (size_t)src.Data.Length);
			CMemory.memcpy(dst.LineSize, src.LineSize, (size_t)src.LineSize.Length);

			return 0;

			Fail:
			Av_Frame_Unref(dst);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Ensure the destination frame refers to the same data described by
		/// the source frame, either by creating a new reference for each
		/// AVBufferRef from src if they differ from those in dst, by
		/// allocating new buffers and copying data if src is not reference
		/// counted, or by unrefencing it if src is empty.
		///
		/// Frame properties on dst will be replaced by those from src
		/// </summary>
		/********************************************************************/
		public static c_int Av_Frame_Replace(AvFrame dst, AvFrame src)//XX 376
		{
			c_int ret = 0;

			if (dst == src)
				return Error.EINVAL;

			if (src.Buf[0] == null)
			{
				Av_Frame_Unref(dst);

				// Duplicate the frame data if it's not refcounted
				if (src.Data[0].IsNotNull || src.Data[1].IsNotNull || src.Data[2].IsNotNull || src.Data[3].IsNotNull)
					return Av_Frame_Ref(dst, src);

				ret = Frame_Copy_Props(dst, src, 0);

				if (ret < 0)
					goto Fail;
			}

			dst.Format = src.Format;
			dst.Width = src.Width;
			dst.Height = src.Height;
			dst.Nb_Samples = src.Nb_Samples;

			ret = Channel_Layout.Av_Channel_Layout_Copy(dst.Ch_Layout, src.Ch_Layout);

			if (ret < 0)
				goto Fail;

			Side_Data.Av_Frame_Side_Data_Free(ref dst.Side_Data, ref dst.Nb_Side_Data);
			Dict.Av_Dict_Free(ref dst.Metadata);

			ret = Frame_Copy_Props(dst, src, 0);

			if (ret < 0)
				goto Fail;

			// Replace the buffers
			for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(src.Buf); i++)
			{
				ret = Buffer.Av_Buffer_Replace(ref dst.Buf[i], src.Buf[i]);

				if (ret < 0)
					goto Fail;
			}

			if (src.Extended_Buf.IsNotNull)
			{
				if (dst.Nb_Extended_Buf != src.Nb_Extended_Buf)
				{
					c_int nb_Extended_Buf = Macros.FFMin(dst.Nb_Extended_Buf, src.Nb_Extended_Buf);

					for (c_int i = nb_Extended_Buf; i < dst.Nb_Extended_Buf; i++)
						Buffer.Av_Buffer_Unref(ref dst.Extended_Buf[i]);

					CPointer<AvBufferRef> tmp = Mem.Av_Realloc_ArrayObj(dst.Extended_Buf, (size_t)src.Nb_Extended_Buf);

					if (tmp.IsNull)
					{
						ret = Error.ENOMEM;
						goto Fail;
					}

					dst.Extended_Buf = tmp;
					dst.Nb_Extended_Buf = src.Nb_Extended_Buf;

					for (c_int i = nb_Extended_Buf; i < src.Nb_Extended_Buf; i++)
						dst.Extended_Buf[i].Clear();
				}

				for (c_int i = 0; i < src.Nb_Extended_Buf; i++)
				{
					ret = Buffer.Av_Buffer_Replace(ref dst.Extended_Buf[i], src.Extended_Buf[i]);

					if (ret < 0)
						goto Fail;
				}
			}
			else if (dst.Extended_Buf.IsNotNull)
			{
				for (c_int i = 0; i < dst.Nb_Extended_Buf; i++)
					Buffer.Av_Buffer_Unref(ref dst.Extended_Buf[i]);

				Mem.Av_FreeP(ref dst.Extended_Buf);
			}

			ret = Buffer.Av_Buffer_Replace(ref dst.Hw_Frames_Ctx, src.Hw_Frames_Ctx);

			if (ret < 0)
				goto Fail;

			if (dst.Extended_Data != dst.Data)
				Mem.Av_FreeP(ref dst.Extended_Data);

			if (src.Extended_Data != src.Data)
			{
				c_int ch = dst.Ch_Layout.Nb_Channels;

				if ((ch <= 0) || (ch > (c_int)(size_t.MaxValue / (size_t)dst.Extended_Data.Length)))
				{
					ret = Error.EINVAL;

					goto Fail;
				}

				dst.Extended_Data = Mem.Av_MemDup(src.Extended_Data, (size_t)(dst.Extended_Data.Length * ch));

				if (dst.Extended_Data.IsNull)
				{
					ret = Error.ENOMEM;

					goto Fail;
				}
			}
			else
				dst.Extended_Data = dst.Data;

			CMemory.memcpy(dst.Data, src.Data, (size_t)src.Data.Length);
			CMemory.memcpy(dst.LineSize, src.LineSize, (size_t)src.LineSize.Length);

			return 0;

			Fail:
			Av_Frame_Unref(dst);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Unreference all the buffers referenced by frame and reset the
		/// frame fields
		/// </summary>
		/********************************************************************/
		public static void Av_Frame_Unref(AvFrame frame)//XX 496
		{
			if (frame == null)
				return;

			Side_Data.Av_Frame_Side_Data_Free(ref frame.Side_Data, ref frame.Nb_Side_Data);

			for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(frame.Buf); i++)
				Buffer.Av_Buffer_Unref(ref frame.Buf[i]);

			for (c_int i = 0; i < frame.Nb_Extended_Buf; i++)
				Buffer.Av_Buffer_Unref(ref frame.Extended_Buf[i]);

			Mem.Av_FreeP(ref frame.Extended_Buf);
			Dict.Av_Dict_Free(ref frame.Metadata);

			Buffer.Av_Buffer_Unref(ref frame.Hw_Frames_Ctx);

			Buffer.Av_Buffer_Unref(ref frame.Opaque_Ref);
			RefStruct.Av_RefStruct_Unref(ref frame.Private_Refs);

			if (frame.Extended_Data != frame.Data)
				Mem.Av_FreeP(ref frame.Extended_Data);

			Channel_Layout.Av_Channel_Layout_Uninit(frame.Ch_Layout);

			Get_Frame_Defaults(frame);
		}



		/********************************************************************/
		/// <summary>
		/// Move everything contained in src to dst and reset src
		/// </summary>
		/********************************************************************/
		public static void Av_Frame_Move_Ref(AvFrame dst, AvFrame src)
		{
			src.CopyTo(dst);

			if (src.Extended_Data == src.Data)
				dst.Extended_Data = dst.Data;

			Get_Frame_Defaults(src);
		}



		/********************************************************************/
		/// <summary>
		/// Add a new side data to a frame from an existing AVBufferRef
		/// </summary>
		/********************************************************************/
		public static AvFrameSideData Av_Frame_New_Side_Data_From_Buf(AvFrame frame, AvFrameSideDataType type, AvBufferRef buf)//XX 638
		{
			return Side_Data.FF_Frame_Side_Data_Add_From_Buf(ref frame.Side_Data, ref frame.Nb_Side_Data, type, buf);
		}



		/********************************************************************/
		/// <summary>
		/// Add a new side data to a frame
		/// </summary>
		/********************************************************************/
		public static AvFrameSideData Av_Frame_New_Side_Data(AvFrame frame, AvFrameSideDataType type, IDataContext data)//XX 647
		{
			AvBufferRef buf = Buffer.Av_Buffer_Alloc(data);

			AvFrameSideData ret = Av_Frame_New_Side_Data_From_Buf(frame, type, buf);

			if (ret == null)
				Buffer.Av_Buffer_Unref(ref buf);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Return a pointer to the side data of a given type on success,
		/// NULL if there is no side data with such type in this frame
		/// </summary>
		/********************************************************************/
		public static AvFrameSideData Av_Frame_Get_Side_Data(AvFrame frame, AvFrameSideDataType type)//XX 659
		{
			return Side_Data.Av_Frame_Side_Data_Get(frame.Side_Data, frame.Nb_Side_Data, type);
		}



		/********************************************************************/
		/// <summary>
		/// Copy the frame data from src to dst.
		///
		/// This function does not allocate anything, dst must be already
		/// initialized and allocated with the same parameters as src.
		///
		/// This function only copies the frame data (i.e. the contents of
		/// the data / extended data arrays), not any other properties
		/// </summary>
		/********************************************************************/
		public static c_int Av_Frame_Copy(AvFrame dst, AvFrame src)//XX 711
		{
			if (((dst.Format.Pixel != src.Format.Pixel) && (dst.Format.Sample != src.Format.Sample)) || ((dst.Format.Pixel < 0) && (dst.Format.Sample < 0)))
				return Error.EINVAL;

			if ((dst.Width > 0) && (dst.Height > 0))
				return Frame_Copy_Video(dst, src);
			else if ((dst.Nb_Samples > 0) && (Channel_Layout.Av_Channel_Layout_Check(dst.Ch_Layout) != 0))
				return Frame_Copy_Audio(dst, src);

			return Error.EINVAL;
		}



		/********************************************************************/
		/// <summary>
		/// Remove and free all side data instances of the given type
		/// </summary>
		/********************************************************************/
		public static void Av_Frame_Remove_Side_Data(AvFrame frame, AvFrameSideDataType type)//XX 659
		{
			Side_Data.Av_Frame_Side_Data_Remove(ref frame.Side_Data, ref frame.Nb_Side_Data, type);
		}



		/********************************************************************/
		/// <summary>
		/// Crop the given video AVFrame according to its
		/// crop_left/crop_top/crop_right/crop_bottom fields. If cropping is
		/// successful, the function will adjust the data pointers and the
		/// width/height fields, and set the crop fields to 0.
		///
		/// In all cases, the cropping boundaries will be rounded to the
		/// inherent alignment of the pixel format. In some cases, such as
		/// for opaque hwaccel formats, the left/top cropping is ignored.
		/// The crop fields are set to 0 even if the cropping was rounded or
		/// ignored
		/// </summary>
		/********************************************************************/
		public static c_int Av_Frame_Apply_Cropping(AvFrame frame, AvFrameCrop flags)//XX 760
		{
			CPointer<size_t> offsets = new CPointer<size_t>(4);

			if (!((frame.Width > 0) && (frame.Height > 0)))
				return Error.EINVAL;

			if ((frame.Crop_Left >= (c_int.MaxValue - frame.Crop_Right)) || (frame.Crop_Top >= (c_int.MaxValue - frame.Crop_Bottom)) ||
			    ((frame.Crop_Left + frame.Crop_Right) >= (size_t)frame.Width) || ((frame.Crop_Top + frame.Crop_Bottom) >= (size_t)frame.Height))
			{
				return Error.EINVAL;
			}

			AVPixFmtDescriptor desc = PixDesc.Av_Pix_Fmt_Desc_Get(frame.Format.Pixel);

			if (desc == null)
				return Error.Bug;

			// Apply just the right/bottom cropping for hwaccel formats. Bitstream
			// formats cannot be easily handled here either (and corresponding decoders
			// should not export any cropping anyway), so do the same for those as well
			if ((desc.Flags & (AvPixelFormatFlag.Bitstream | AvPixelFormatFlag.HwAccel)) != 0)
			{
				frame.Width -= (c_int)frame.Crop_Right;
				frame.Height -= (c_int)frame.Crop_Bottom;
				frame.Crop_Right = 0;
				frame.Crop_Bottom = 0;

				return 0;
			}

			// Calculate the offsets for each plane
			c_int ret = Calc_Cropping_Offsets(offsets, frame, desc);

			if (ret < 0)
				return ret;

			// Adjust the offsets to avoid breaking alignment
			if ((flags & AvFrameCrop.Unaligned) == 0)
			{
				c_int log2_Crop_Align = frame.Crop_Left != 0 ? IntMath.FF_Ctz((c_int)frame.Crop_Left) : c_int.MaxValue;
				c_int min_Log2_Align = c_int.MaxValue;

				for (c_int i = 0; frame.Data[i].IsNotNull; i++)
				{
					c_int log2_Align = offsets[i] != 0 ? IntMath.FF_Ctz((c_int)offsets[i]) : c_int.MaxValue;
					min_Log2_Align = Macros.FFMin(log2_Align, min_Log2_Align);
				}

				// We assume, and it should always be true, that the data alignment is
				// related to the cropping alignment by a constant power-of-2 factor
				if (log2_Crop_Align < min_Log2_Align)
					return Error.Bug;

				if ((min_Log2_Align < 5) && (log2_Crop_Align != c_int.MaxValue))
				{
					frame.Crop_Left &= (size_t)~((1 << (5 + log2_Crop_Align - min_Log2_Align)) - 1);

					ret = Calc_Cropping_Offsets(offsets, frame, desc);

					if (ret < 0)
						return ret;
				}
			}

			for (c_int i = 0; frame.Data[i].IsNotNull; i++)
				frame.Data[i] += offsets[i];

			frame.Width -= (c_int)(frame.Crop_Left + frame.Crop_Right);
			frame.Height -= (c_int)(frame.Crop_Top + frame.Crop_Bottom);

			frame.Crop_Left = 0;
			frame.Crop_Right = 0;
			frame.Crop_Top = 0;
			frame.Crop_Bottom = 0;

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Get_Frame_Defaults(AvFrame frame)//XX 31
		{
			frame.Clear();

			frame.Pts = UtilConstants.Av_NoPts_Value;
			frame.Pkt_Dts = UtilConstants.Av_NoPts_Value;
			frame.Best_Effort_Timestamp = UtilConstants.Av_NoPts_Value;
			frame.Duration = 0;
			frame.Time_Base = new AvRational(0, 1);
			frame.Sample_Aspect_Ratio = new AvRational(0, 1);
			frame.Format.Pixel = AvPixelFormat.None;
			frame.Format.Sample = AvSampleFormat.None;
			frame.Extended_Data = frame.Data;
			frame.Color_Primaries = AvColorPrimaries.Unspecified;
			frame.Color_Trc = AvColorTransferCharacteristic.Unspecified;
			frame.ColorSpace = AvColorSpace.Unspecified;
			frame.Color_Range = AvColorRange.Unspecified;
			frame.Chroma_Location = AvChromaLocation.Unspecified;
			frame.Alpha_Mode = AvAlphaMode.Unspecified;
			frame.Flags = AvFrameFlag.None;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Get_Video_Buffer(AvFrame frame, c_int align)//XX 75
		{
			AVPixFmtDescriptor desc = PixDesc.Av_Pix_Fmt_Desc_Get(frame.Format.Pixel);
			CPointer<ptrdiff_t> lineSizes = new CPointer<ptrdiff_t>(4);
			CPointer<size_t> sizes = new CPointer<size_t>(4);

			if (desc == null)
				return Error.EINVAL;

			c_int ret = ImgUtils.Av_Image_Check_Size((c_uint)frame.Width, (c_uint)frame.Height, 0, null);

			if (ret < 0)
				return ret;

			if (align <= 0)
				align = Align;

			c_int plane_Padding = Macros.FFMax(Align, align);

			if (frame.LineSize[0] == 0)
			{
				for (c_int i = 1; i <= align; i += i)
				{
					ret = ImgUtils.Av_Image_Fill_LineSizes(frame.LineSize, frame.Format.Pixel, Macros.FFAlign(frame.Width, i));

					if (ret < 0)
						return ret;

					if ((frame.LineSize[0] & (align - 1)) == 0)
						break;
				}

				for (c_int i = 0; (i < 4) && (frame.LineSize[i] != 0); i++)
					frame.LineSize[i] = Macros.FFAlign(frame.LineSize[i], align);
			}

			for (c_int i = 0; i < 4; i++)
				lineSizes[i] = frame.LineSize[i];

			c_int padded_Height = Macros.FFAlign(frame.Height, 32);

			ret = ImgUtils.Av_Image_Fill_Plane_Sizes(sizes, frame.Format.Pixel, padded_Height, lineSizes);

			if (ret < 0)
				return ret;

			size_t total_Size = (size_t)((4 * plane_Padding) + (4 * align));

			for (c_int i = 0; i < 4; i++)
			{
				if (sizes[i] > (size_t.MaxValue - total_Size))
					return Error.EINVAL;

				total_Size += sizes[i];
			}

			frame.Buf[0] = Buffer.Av_Buffer_Alloc(total_Size);

			if (frame.Buf[0] == null)
			{
				ret = Error.ENOMEM;
				goto Fail;
			}

			ret = ImgUtils.Av_Image_Fill_Pointers(frame.Data, frame.Format.Pixel, padded_Height, ((DataBufferContext)frame.Buf[0].Data).Data, frame.LineSize);

			if (ret < 0)
				goto Fail;

			for (c_int i = 1; i < 4; i++)
			{
				if (frame.Data[i].IsNotNull)
					frame.Data[i] += i * plane_Padding;

				frame.Data[i] += Macros.FFAlign(frame.Data[i].Offset, align);
			}

			frame.Extended_Data = frame.Data;

			return 0;

			Fail:
			Av_Frame_Unref(frame);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Get_Audio_Buffer(AvFrame frame, c_int align)//XX 146
		{
			bool planar = SampleFmt.Av_Sample_Fmt_Is_Planar(frame.Format.Sample);
			c_int ret;

			c_int channels = frame.Ch_Layout.Nb_Channels;
			c_int planes = planar ? channels : 1;

			if (frame.LineSize[0] == 0)
			{
				ret = SampleFmt.Av_Samples_Get_Buffer_Size(frame.LineSize, channels, frame.Nb_Samples, frame.Format.Sample, align);

				if (ret < 0)
					return ret;
			}

			if (align <= 0)
				align = Align;

			if (planes > AvFrame.Av_Num_Data_Pointers)
			{
				frame.Extended_Data = Mem.Av_CAlloc<CPointer<uint8_t>>((size_t)planes);
				frame.Extended_Buf = Mem.Av_CAllocObj<AvBufferRef>((size_t)planes - AvFrame.Av_Num_Data_Pointers);

				if (frame.Extended_Data.IsNull || frame.Extended_Buf.IsNull)
				{
					Mem.Av_FreeP(ref frame.Extended_Data);
					Mem.Av_FreeP(ref frame.Extended_Buf);

					return Error.ENOMEM;
				}

				frame.Nb_Extended_Buf = planes - AvFrame.Av_Num_Data_Pointers;
			}
			else
				frame.Extended_Data = frame.Data;

			if ((size_t)frame.LineSize[0] > (size_t.MaxValue - (size_t)align))
				return Error.EINVAL;

			size_t size = (size_t)frame.LineSize[0] + (size_t)align;

			for (c_int i = 0; i < Macros.FFMin(planes, AvFrame.Av_Num_Data_Pointers); i++)
			{
				frame.Buf[i] = Buffer.Av_Buffer_Alloc(size);

				if (frame.Buf[i] == null)
				{
					Av_Frame_Unref(frame);

					return Error.ENOMEM;
				}

				frame.Extended_Data[i] = frame.Data[i] += Macros.FFAlign(((DataBufferContext)frame.Buf[i].Data).Data.Offset, align);
			}

			for (c_int i = 0; i < (planes - AvFrame.Av_Num_Data_Pointers); i++)
			{
				frame.Extended_Buf[i] = Buffer.Av_Buffer_Alloc(size);

				if (frame.Extended_Buf[i] == null)
				{
					Av_Frame_Unref(frame);

					return Error.ENOMEM;
				}

				frame.Extended_Data[i + AvFrame.Av_Num_Data_Pointers] = ((DataBufferContext)frame.Extended_Buf[i].Data).Data + Macros.FFAlign(((DataBufferContext)frame.Extended_Buf[i].Data).Data.Offset, align);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Frame_Copy_Props(AvFrame dst, AvFrame src, c_int force_Copy)//XX 220
		{
			dst.Pict_Type = src.Pict_Type;
			dst.Sample_Aspect_Ratio = src.Sample_Aspect_Ratio;
			dst.Crop_Top = src.Crop_Top;
			dst.Crop_Bottom = src.Crop_Bottom;
			dst.Crop_Left = src.Crop_Left;
			dst.Crop_Right = src.Crop_Right;
			dst.Pts = src.Pts;
			dst.Duration = src.Duration;
			dst.Repeat_Pict = src.Repeat_Pict;
			dst.Sample_Rate = src.Sample_Rate;
			dst.Opaque = src.Opaque;
			dst.Pkt_Dts = src.Pkt_Dts;
			dst.Time_Base = src.Time_Base;
			dst.Quality = src.Quality;
			dst.Best_Effort_Timestamp = src.Best_Effort_Timestamp;
			dst.Flags = src.Flags;
			dst.Decode_Error_Flags = src.Decode_Error_Flags;
			dst.Color_Primaries = src.Color_Primaries;
			dst.Color_Trc = src.Color_Trc;
			dst.ColorSpace = src.ColorSpace;
			dst.Color_Range = src.Color_Range;
			dst.Chroma_Location = src.Chroma_Location;
			dst.Alpha_Mode = src.Alpha_Mode;

			Dict.Av_Dict_Copy(ref dst.Metadata, src.Metadata, AvDict.None);

			for (c_int i = 0; i < src.Nb_Side_Data; i++)
			{
				AvFrameSideData sd_Src = src.Side_Data[i];
				AvFrameSideData sd_Dst;

				if ((sd_Src.Type == AvFrameSideDataType.PanScan) && ((src.Width != dst.Width) || (src.Height != dst.Height)))
					continue;

				if (force_Copy != 0)
				{
					sd_Dst = Av_Frame_New_Side_Data(dst, sd_Src.Type, sd_Src.Data.MakeDeepClone());

					if (sd_Dst == null)
					{
						Side_Data.Av_Frame_Side_Data_Free(ref dst.Side_Data, ref dst.Nb_Side_Data);

						return Error.ENOMEM;
					}

//					CMemory.memcpy(sd_Dst.Data, sd_Src.Data, sd_Src.Size);
				}
				else
				{
					AvBufferRef @ref = Buffer.Av_Buffer_Ref(sd_Src.Buf);
					sd_Dst = Av_Frame_New_Side_Data_From_Buf(dst, sd_Src.Type, @ref);

					if (sd_Dst == null)
					{
						Buffer.Av_Buffer_Unref(ref @ref);
						Side_Data.Av_Frame_Side_Data_Free(ref dst.Side_Data, ref dst.Nb_Side_Data);

						return Error.ENOMEM;
					}
				}

				Dict.Av_Dict_Copy(ref sd_Dst.Metadata, sd_Src.Metadata, AvDict.None);
			}

			RefStruct.Av_RefStruct_Replace(ref dst.Private_Refs, src.Private_Refs);

			return Buffer.Av_Buffer_Replace(ref dst.Opaque_Ref, src.Opaque_Ref);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Frame_Copy_Video(AvFrame dst, AvFrame src)//XX 668
		{
			if ((dst.Width < src.Width) || (dst.Height < src.Height))
				return Error.EINVAL;

			if ((src.Hw_Frames_Ctx != null) || (dst.Hw_Frames_Ctx != null))
				return HwContext.Av_HwFrame_Transfer_Data(dst, src, 0);

			c_int planes = PixDesc.Av_Pix_Fmt_Count_Planes(dst.Format.Pixel);

			for (c_int i = 0; i < planes; i++)
			{
				if (dst.Data[i].IsNull || src.Data[i].IsNull)
					return Error.EINVAL;
			}

			ImgUtils.Av_Image_Copy2(dst.Data, dst.LineSize, src.Data, src.LineSize, dst.Format.Pixel, src.Width, src.Height);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Frame_Copy_Audio(AvFrame dst, AvFrame src)//XX 691
		{
			bool planar = SampleFmt.Av_Sample_Fmt_Is_Planar(dst.Format.Sample);
			c_int channels = dst.Ch_Layout.Nb_Channels;
			c_int planes = planar ? channels : 1;

			if ((dst.Nb_Samples != src.Nb_Samples) || (Channel_Layout.Av_Channel_Layout_Compare(dst.Ch_Layout, src.Ch_Layout) != 0))
				return Error.EINVAL;

			for (c_int i = 0; i < planes; i++)
			{
				if (dst.Extended_Data[i].IsNull || src.Extended_Data[i].IsNull)
					return Error.EINVAL;
			}

			SampleFmt.Av_Samples_Copy(dst.Extended_Data, src.Extended_Data, 0, 0, dst.Nb_Samples, channels, dst.Format.Sample);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Calc_Cropping_Offsets(CPointer<size_t> offsets, AvFrame frame, AVPixFmtDescriptor desc)//XX 730
		{
			for (c_int i = 0; frame.Data[i].IsNotNull; i++)
			{
				AvComponentDescriptor comp = null;

				c_int shift_X = (i == 1) || (i == 2) ? desc.Log2_Chroma_W : 0;
				c_int shift_Y = (i == 1) || (i == 2) ? desc.Log2_Chroma_H : 0;

				if (((desc.Flags & AvPixelFormatFlag.Pal) != 0) && (i == 1))
				{
					offsets[i] = 0;
					break;
				}

				// Find any component descriptor for this plane
				for (c_int j = 0; j < desc.Nb_Components; j++)
				{
					if (desc.Comp[j].Plane == 1)
					{
						comp = desc.Comp[j];
						break;
					}
				}

				if (comp == null)
					return Error.Bug;

				offsets[i] = ((frame.Crop_Top >> shift_Y) * (size_t)frame.LineSize[i]) + ((frame.Crop_Left >> shift_X) * (size_t)comp.Step);
			}

			return 0;
		}
		#endregion
	}
}
