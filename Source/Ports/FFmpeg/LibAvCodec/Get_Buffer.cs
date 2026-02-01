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
	public static class Get_Buffer
	{
		/********************************************************************/
		/// <summary>
		/// The default callback for AVCodecContext.get_buffer2(). It is made
		/// public so it can be called by custom get_buffer2()
		/// implementations for decoders without AV_CODEC_CAP_DR1 set
		/// </summary>
		/********************************************************************/
		public static c_int AvCodec_Default_Get_Buffer2(AvCodecContext avCtx, AvFrame frame, c_int flags)//XX 253
		{
			c_int ret;

			if (avCtx.Hw_Frames_Ctx != null)
			{
				ret = HwContext.Av_HwFrame_Get_Buffer(avCtx.Hw_Frames_Ctx, frame, 0);

				if (ret == Error.ENOMEM)
				{
					AvHwFramesContext frames_Ctx = (AvHwFramesContext)avCtx.Hw_Frames_Ctx.Data;

					if ((frames_Ctx.Initial_Pool_Size > 0) && (avCtx.Internal.Warned_On_Failed_Allocation_From_Fixed_Pool == 0))
					{
						Log.Av_Log(avCtx, Log.Av_Log_Warning, "Failed to allocate a %s/%s frame from a fixed pool of hardware frames.\n", PixDesc.Av_Get_Pix_Fmt_Name(frames_Ctx.Format), PixDesc.Av_Get_Pix_Fmt_Name(frames_Ctx.Sw_Format));
						Log.Av_Log(avCtx, Log.Av_Log_Warning, "Consider setting extra_hw_frames to a larger value (currently set to %d, giving a pool size of %d).\n", avCtx.Extra_Hw_Frames, frames_Ctx.Initial_Pool_Size);

						avCtx.Internal.Warned_On_Failed_Allocation_From_Fixed_Pool = 1;
					}
				}

				frame.Width = avCtx.Coded_Width;
				frame.Height = avCtx.Coded_Height;

				return ret;
			}

			ret = Update_Frame_Pool(avCtx, frame);

			if (ret < 0)
				return ret;

			switch (avCtx.Codec_Type)
			{
				case AvMediaType.Video:
					return Video_Get_Buffer(avCtx, frame);

				case AvMediaType.Audio:
					return Audio_Get_Buffer(avCtx, frame);

				default:
					return -1;
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Frame_Pool_Free(AvRefStructOpaque unused, IRefCount obj)//XX 56
		{
			FramePool pool = (FramePool)obj;

			for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(pool.Pools); i++)
				Buffer.Av_Buffer_Pool_Uninit(ref pool.Pools[i]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Update_Frame_Pool(AvCodecContext avCtx, AvFrame frame)//XX 65
		{
			FramePool pool = avCtx.Internal.Pool;
			c_int ret;

			if ((pool != null) && ((pool.Format.Pixel == frame.Format.Pixel) || (pool.Format.Sample == frame.Format.Sample)))
			{
				if ((avCtx.Codec_Type == AvMediaType.Video) && (pool.Width == frame.Width) && (pool.Height == frame.Height))
					return 0;

				if ((avCtx.Codec_Type == AvMediaType.Audio) && (pool.Channels == frame.Ch_Layout.Nb_Channels) && (frame.Nb_Samples == pool.Samples))
					return 0;
			}

			pool = RefStruct.Av_RefStruct_Alloc_Ext<FramePool>(AvRefStructFlag.None, null, Frame_Pool_Free);

			if (pool == null)
				return Error.ENOMEM;

			switch (avCtx.Codec_Type)
			{
				case AvMediaType.Video:
				{
					CPointer<c_int> lineSize = new CPointer<c_int>(4);
					c_int w = frame.Width;
					c_int h = frame.Height;
					c_int unaligned;
					CPointer<ptrdiff_t> lineSize1 = new CPointer<ptrdiff_t>(4);
					CPointer<size_t> size = new CPointer<size_t>(4);

					Utils_Codec.AvCodec_Align_Dimensions2(avCtx, ref w, ref h, pool.Stride_Align);

					do
					{
						// NOTE: do not align linesizes individually, this breaks e.g. assumptions
						// that linesize[0] == 2*linesize[1] in the MPEG-encoder for 4:2:2
						ret = ImgUtils.Av_Image_Fill_LineSizes(lineSize, avCtx.Pix_Fmt, w);

						if (ret < 0)
							goto Fail;

						// Increase alignment of w for next try (rhs gives the lowest bit set in w)
						w += w & ~(w - 1);

						unaligned = 0;

						for (c_int i = 0; i < 4; i++)
							unaligned |= lineSize[i] % pool.Stride_Align[i];
					}
					while (unaligned != 0);

					for (c_int i = 0; i < 4; i++)
						lineSize1[i] = lineSize[i];

					ret = ImgUtils.Av_Image_Fill_Plane_Sizes(size, avCtx.Pix_Fmt, h, lineSize1);

					if (ret < 0)
						goto Fail;

					for (c_int i = 0; i < 4; i++)
					{
						pool.LineSize[i] = lineSize[i];

						if (size[i] != 0)
						{
							if (size[i] > (c_int.MaxValue - (16 + CodecConstants.Stride_Align - 1)))
							{
								ret = Error.EINVAL;

								goto Fail;
							}

							pool.Pools[i] = Buffer.Av_Buffer_Pool_Init(size[i] + 16 + CodecConstants.Stride_Align - 1, Buffer.Av_Buffer_Allocz);

							if (pool.Pools[i] == null)
							{
								ret = Error.ENOMEM;

								goto Fail;
							}
						}
					}

					pool.Format = frame.Format;
					pool.Width = frame.Width;
					pool.Height = frame.Height;
					break;
				}

				case AvMediaType.Audio:
				{
					ret = SampleFmt.Av_Samples_Get_Buffer_Size(pool.LineSize, frame.Ch_Layout.Nb_Channels, frame.Nb_Samples, frame.Format.Sample, 0);

					if (ret < 0)
						goto Fail;

					pool.Pools[0] = Buffer.Av_Buffer_Pool_Init((size_t)pool.LineSize[0], Buffer.Av_Buffer_Allocz);

					if (pool.Pools[0] == null)
					{
						ret = Error.ENOMEM;

						goto Fail;
					}

					pool.Format = frame.Format;
					pool.Channels = frame.Ch_Layout.Nb_Channels;
					pool.Samples = frame.Nb_Samples;
					pool.Planes = SampleFmt.Av_Sample_Fmt_Is_Planar(pool.Format.Sample) ? pool.Channels : 1;
					break;
				}
			}

			RefStruct.Av_RefStruct_Unref(ref avCtx.Internal.Pool);
			avCtx.Internal.Pool = pool;

			return 0;

			Fail:
			RefStruct.Av_RefStruct_Unref(ref pool);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Audio_Get_Buffer(AvCodecContext avCtx, AvFrame frame)//XX 172
		{
			FramePool pool = avCtx.Internal.Pool;
			c_int planes = pool.Planes;

			frame.LineSize[0] = pool.LineSize[0];

			if (planes > AvFrame.Av_Num_Data_Pointers)
			{
				frame.Extended_Data = Mem.Av_CAlloc<CPointer<uint8_t>>((size_t)planes);
				frame.Nb_Extended_Buf = planes - AvFrame.Av_Num_Data_Pointers;
				frame.Extended_Buf = Mem.Av_CAllocObj<AvBufferRef>((size_t)frame.Nb_Extended_Buf);

				if (frame.Extended_Data.IsNull || frame.Extended_Buf.IsNull)
				{
					Mem.Av_FreeP(ref frame.Extended_Data);
					Mem.Av_FreeP(ref frame.Extended_Buf);

					return Error.ENOMEM;
				}
			}
			else
				frame.Extended_Data = frame.Data;

			for (c_int i = 0; i < Macros.FFMin(planes, AvFrame.Av_Num_Data_Pointers); i++)
			{
				frame.Buf[i] = Buffer.Av_Buffer_Pool_Get(pool.Pools[0]);

				if (frame.Buf[i] == null)
					goto Fail;

				frame.Extended_Data[i] = frame.Data[i] = ((DataBufferContext)frame.Buf[i].Data).Data;
			}

			for (c_int i = 0; i < frame.Nb_Extended_Buf; i++)
			{
				frame.Extended_Buf[i] = Buffer.Av_Buffer_Pool_Get(pool.Pools[0]);

				if (frame.Extended_Buf[i] == null)
					goto Fail;

				frame.Extended_Data[i + AvFrame.Av_Num_Data_Pointers] = ((DataBufferContext)frame.Extended_Buf[i].Data).Data;
			}

			if ((avCtx.Debug & FFDebug.Buffers) != 0)
				Log.Av_Log(avCtx, Log.Av_Log_Debug, "default_get_buffer called on frame %p", frame);

			return 0;

			Fail:
			Frame.Av_Frame_Unref(frame);

			return Error.ENOMEM;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Video_Get_Buffer(AvCodecContext s, AvFrame pic)//XX 217
		{
			FramePool pool = s.Internal.Pool;
			c_int i;

			if (pic.Data[0].IsNotNull || pic.Data[1].IsNotNull || pic.Data[2].IsNotNull || pic.Data[3].IsNotNull)
			{
				Log.Av_Log(s, Log.Av_Log_Error, "pic->data[*] != null in avcodec_default_get_buffer\n");

				return -1;
			}

			CMemory.memset(pic.Data, null, (size_t)pic.Data.Length);
			pic.Extended_Data = pic.Data;

			for (i = 0; (i < 4) && (pool.Pools[i] != null); i++)
			{
				pic.LineSize[i] = pool.LineSize[i];

				pic.Buf[i] = Buffer.Av_Buffer_Pool_Get(pool.Pools[i]);

				if (pic.Buf[i] == null)
					goto Fail;

				pic.Data[i] = ((DataBufferContext)pic.Buf[i].Data).Data;
			}

			for (; i < AvFrame.Av_Num_Data_Pointers; i++)
			{
				pic.Data[i].SetToNull();
				pic.LineSize[i] = 0;
			}

			if ((s.Debug & FFDebug.Buffers) != 0)
				Log.Av_Log(s, Log.Av_Log_Debug, "default_get_buffer called on pic %p", pic);

			return 0;

			Fail:
			Frame.Av_Frame_Unref(pic);

			return Error.ENOMEM;
		}
		#endregion
	}
}
