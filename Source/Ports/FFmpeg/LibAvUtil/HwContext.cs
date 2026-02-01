/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class HwContext
	{
		/********************************************************************/
		/// <summary>
		/// Get a list of possible source or target formats usable in
		/// av_hwframe_transfer_data()
		/// </summary>
		/********************************************************************/
		public static c_int Av_HwFrame_Transfer_Get_Formats(AvBufferRef hwFrame_Ref, AvHwFrameTransferDirection dir, out CPointer<AvPixelFormat> formats, c_int flags)//XX 386
		{
			formats = null;

			FFHwFramesContext ctxI = (FFHwFramesContext)hwFrame_Ref.Data;

			if (ctxI.Hw_Type.Transfer_Get_Formats == null)
				return Error.ENOSYS;

			return ctxI.Hw_Type.Transfer_Get_Formats(ctxI.P, dir, out formats);
		}



		/********************************************************************/
		/// <summary>
		/// Copy data to or from a hw surface. At least one of dst/src must
		/// have an AVHWFramesContext attached.
		///
		/// If src has an AVHWFramesContext attached, then the format of dst
		/// (if set) must use one of the formats returned by
		/// av_hwframe_transfer_get_formats(src, AV_HWFRAME_TRANSFER_DIRECTION_FROM).
		/// If dst has an AVHWFramesContext attached, then the format of src
		/// must use one of the formats returned by
		/// av_hwframe_transfer_get_formats(dst, AV_HWFRAME_TRANSFER_DIRECTION_TO)
		///
		/// dst may be "clean" (i.e. with data/buf pointers unset), in which
		/// case the data buffers will be allocated by this function using
		/// av_frame_get_buffer(). If dst->format is set, then this format
		/// will be used, otherwise (when dst->format is AV_PIX_FMT_NONE)
		/// the first acceptable format will be chosen.
		///
		/// The two frames must have matching allocated dimensions (i.e.
		/// equal to AVHWFramesContext.width/height), since not all device
		/// types support transferring a sub-rectangle of the whole surface.
		/// The display dimensions (i.e. AVFrame.width/height) may be smaller
		/// than the allocated dimensions, but also have to be equal for both
		/// frames. When the display dimensions are smaller than the
		/// allocated dimensions, the content of the padding in the
		/// destination frame is unspecified
		/// </summary>
		/********************************************************************/
		public static c_int Av_HwFrame_Transfer_Data(AvFrame dst, AvFrame src, c_int flags)//XX 448
		{
			c_int ret;

			if (dst.Buf[0] == null)
				return Transfer_Data_Alloc(dst, src, flags);

			//
			// Hardware -> Hardware Transfer.
			// Unlike Software -> Hardware or Hardware -> Software, the transfer
			// function could be provided by either the src or dst, depending on
			// the specific combination of hardware
			//
			if ((src.Hw_Frames_Ctx != null) && (dst.Hw_Frames_Ctx != null))
			{
				FFHwFramesContext src_Ctx = (FFHwFramesContext)src.Hw_Frames_Ctx.Data;
				FFHwFramesContext dst_Ctx = (FFHwFramesContext)dst.Hw_Frames_Ctx.Data;

				if (src_Ctx.Source_Frames != null)
				{
					Log.Av_Log(src_Ctx, Log.Av_Log_Error, "A device with a derived frame context cannot be used as the source of a HW -> HW transfer.");

					return Error.ENOSYS;
				}

				if (dst_Ctx.Source_Frames != null)
				{
					Log.Av_Log(src_Ctx, Log.Av_Log_Error, "A device with a derived frame context cannot be used as the destination of a HW -> HW transfer.");

					return Error.ENOSYS;
				}

				ret = src_Ctx.Hw_Type.Transfer_Data_From(src_Ctx.P, dst, src);

				if (ret == Error.ENOSYS)
					ret = dst_Ctx.Hw_Type.Transfer_Data_To(dst_Ctx.P, dst, src);

				if (ret < 0)
					return ret;
			}
			else
			{
				if (src.Hw_Frames_Ctx != null)
				{
					FFHwFramesContext ctx = (FFHwFramesContext)src.Hw_Frames_Ctx.Data;

					ret = ctx.Hw_Type.Transfer_Data_From(ctx.P, dst, src);

					if (ret < 0)
						return ret;
				}
				else if (dst.Hw_Frames_Ctx != null)
				{
					FFHwFramesContext ctx = (FFHwFramesContext)dst.Hw_Frames_Ctx.Data;

					ret = ctx.Hw_Type.Transfer_Data_To(ctx.P, dst, src);

					if (ret < 0)
						return ret;
				}
				else
					return Error.ENOSYS;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Allocate a new frame attached to the given AVHWFramesContext
		/// </summary>
		/********************************************************************/
		public static c_int Av_HwFrame_Get_Buffer(AvBufferRef hwFrame_Ref, AvFrame frame, c_int flags)//XX 506
		{
			FFHwFramesContext ctxI = (FFHwFramesContext)hwFrame_Ref.Data;
			AvHwFramesContext ctx = ctxI.P;
			c_int ret;

			if (ctxI.Source_Frames != null)
			{
				// This is a derived frame context, so we allocate in the source
				// and map the frame immediately
				frame.Format.Pixel = ctx.Format;
				frame.Hw_Frames_Ctx = Buffer.Av_Buffer_Ref(hwFrame_Ref);

				if (frame.Hw_Frames_Ctx == null)
					return Error.ENOMEM;

				AvFrame src_Frame = Frame.Av_Frame_Alloc();

				if (src_Frame == null)
					return Error.ENOMEM;

				ret = Av_HwFrame_Get_Buffer(ctxI.Source_Frames, src_Frame, 0);

				if (ret < 0)
				{
					Frame.Av_Frame_Free(ref src_Frame);

					return ret;
				}

				ret = Av_HwFrame_Map(frame, src_Frame, ctxI.Source_Allocation_Map_Flags);

				if (ret != 0)
				{
					Log.Av_Log(ctx, Log.Av_Log_Error, "Failed to map frame into derived frame context: %d\n", ret);

					Frame.Av_Frame_Free(ref src_Frame);

					return ret;
				}

				// Free the source frame immediately - the mapped frame still
				// contains a reference to it
				Frame.Av_Frame_Free(ref src_Frame);

				return 0;
			}

			if (ctxI.Hw_Type.Frames_Get_Buffer == null)
				return Error.ENOSYS;

			if (ctx.Pool == null)
				return Error.EINVAL;

			frame.Hw_Frames_Ctx = Buffer.Av_Buffer_Ref(hwFrame_Ref);

			if (frame.Hw_Frames_Ctx == null)
				return Error.ENOMEM;

			ret = ctxI.Hw_Type.Frames_Get_Buffer(ctx, frame);

			if (ret < 0)
			{
				Buffer.Av_Buffer_Unref(ref frame.Hw_Frames_Ctx);

				return ret;
			}

			frame.Extended_Data = frame.Data;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Map a hardware frame.
		///
		/// This has a number of different possible effects, depending on the
		/// format and origin of the src and dst frames. On input, src should
		/// be a usable frame with valid buffers and dst should be blank
		/// (typically as just created by av_frame_alloc()). src should have
		/// an associated hwframe context, and dst may optionally have a
		/// format and associated hwframe context.
		///
		/// If src was created by mapping a frame from the hwframe context of
		/// dst, then this function undoes the mapping - dst is replaced by a
		/// reference to the frame that src was originally mapped from.
		///
		/// If both src and dst have an associated hwframe context, then this
		/// function attempts to map the src frame from its hardware context
		/// to that of dst and then fill dst with appropriate data to be
		/// usable there. This will only be possible if the hwframe contexts
		/// and associated devices are compatible - given compatible devices,
		/// av_hwframe_ctx_create_derived() can be used to create a hwframe
		/// context for dst in which mapping should be possible.
		///
		/// If src has a hwframe context but dst does not, then the src frame
		/// is mapped to normal memory and should thereafter be usable as a
		/// normal frame. If the format is set on dst, then the mapping will
		/// attempt to create dst with that format and fail if it is not
		/// possible. If format is unset (is AV_PIX_FMT_NONE) then dst will
		/// be mapped with whatever the most appropriate format to use is
		/// (probably the sw_format of the src hwframe context).
		///
		/// A return value of AVERROR(ENOSYS) indicates that the mapping is
		/// not possible with the given arguments and hwframe setup, while
		/// other return values indicate that it failed somehow.
		///
		/// On failure, the destination frame will be left blank, except
		/// for the hw_frames_ctx/format fields they may have been set by
		/// the caller - those will be preserved as they were
		/// </summary>
		/********************************************************************/
		public static c_int Av_HwFrame_Map(AvFrame dst, AvFrame src, c_int flags)//XX 793
		{
			AvBufferRef orig_Dst_Frames = dst.Hw_Frames_Ctx;
			AvPixelFormat orig_Dst_Fmt = dst.Format.Pixel;
			HwMapDescriptor hwMap;
			c_int ret;

			if ((src.Hw_Frames_Ctx != null) && (dst.Hw_Frames_Ctx != null))
			{
				FFHwFramesContext src_Frames = (FFHwFramesContext)src.Hw_Frames_Ctx.Data;
				FFHwFramesContext dst_Frames = (FFHwFramesContext)dst.Hw_Frames_Ctx.Data;

				if (((src_Frames == dst_Frames) && (src.Format.Pixel == dst_Frames.P.Sw_Format) && (dst.Format.Pixel == dst_Frames.P.Format)) ||
					((src_Frames.Source_Frames != null) && (src_Frames.Source_Frames.Data == dst_Frames)))
				{
					// This is an unmap operation.  We don't need to directly
					// do anything here other than fill in the original frame,
					// because the real unmap will be invoked when the last
					// reference to the mapped frame disappears
					if (src.Buf[0] == null)
					{
						Log.Av_Log(src_Frames, Log.Av_Log_Error, "Invalid mapping found when attempting unmap.\n");

						return Error.EINVAL;
					}

					hwMap = (HwMapDescriptor)src.Buf[0].Data;

					return Frame.Av_Frame_Replace(dst, hwMap.Source);
				}
			}

			if (src.Hw_Frames_Ctx != null)
			{
				FFHwFramesContext src_Frames = (FFHwFramesContext)src.Hw_Frames_Ctx.Data;

				if ((src_Frames.P.Format == src.Format.Pixel) && (src_Frames.Hw_Type.Map_From != null))
				{
					ret = src_Frames.Hw_Type.Map_From(src_Frames.P, dst, src, flags);

					if (ret >= 0)
						return ret;
					else if (ret != Error.ENOSYS)
						goto Fail;
				}
			}

			if (dst.Hw_Frames_Ctx != null)
			{
				FFHwFramesContext dst_Frames = (FFHwFramesContext)dst.Hw_Frames_Ctx.Data;

				if ((dst_Frames.P.Format == dst.Format.Pixel) && (dst_Frames.Hw_Type.Map_To != null))
				{
					ret = dst_Frames.Hw_Type.Map_To(dst_Frames.P, dst, src, flags);

					if (ret >= 0)
						return ret;
					else if (ret != Error.ENOSYS)
						goto Fail;
				}
			}

			return Error.ENOSYS;

			Fail:
			// If the caller provided dst frames context, it should be preserved
			// by this function

			// Preserve user-provided dst frame fields, but clean
			// anything we might have set
			dst.Hw_Frames_Ctx = null;
			Frame.Av_Frame_Unref(dst);

			dst.Hw_Frames_Ctx = orig_Dst_Frames;
			dst.Format.Pixel = orig_Dst_Fmt;

			return ret;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Transfer_Data_Alloc(AvFrame dst, AvFrame src, c_int flags)//XX 398
		{
			c_int ret = 0;

			if (src.Hw_Frames_Ctx == null)
				return Error.EINVAL;

			AvHwFramesContext ctx = (AvHwFramesContext)src.Hw_Frames_Ctx.Data;

			AvFrame frame_Tmp = Frame.Av_Frame_Alloc();

			if (frame_Tmp == null)
				return Error.ENOMEM;

			// If the format is set, use that
			// otherwise pick the first supported one
			if (dst.Format.Pixel >= 0)
				frame_Tmp.Format.Pixel = dst.Format.Pixel;
			else
			{
				ret = Av_HwFrame_Transfer_Get_Formats(src.Hw_Frames_Ctx, AvHwFrameTransferDirection.From, out CPointer<AvPixelFormat> formats, 0);

				if (ret < 0)
					goto Fail;

				frame_Tmp.Format.Pixel = formats[0];

				Mem.Av_FreeP(ref formats);
			}

			frame_Tmp.Width = ctx.Width;
			frame_Tmp.Height = ctx.Height;

			ret = Frame.Av_Frame_Get_Buffer(frame_Tmp, 0);

			if (ret < 0)
				goto Fail;

			ret = Av_HwFrame_Transfer_Data(frame_Tmp, src, flags);

			if (ret < 0)
				goto Fail;

			frame_Tmp.Width = src.Width;
			frame_Tmp.Height = src.Height;

			Frame.Av_Frame_Move_Ref(dst, frame_Tmp);

			Fail:
			Frame.Av_Frame_Free(ref frame_Tmp);

			return ret;
		}
		#endregion
	}
}
