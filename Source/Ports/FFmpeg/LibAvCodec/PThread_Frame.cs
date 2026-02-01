/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Compat;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// Frame multithreading support functions
	/// </summary>
	internal static class PThread_Frame
	{
		private static readonly PThread_ThreadInfo thread_Ctx_Fields = new PThread_ThreadInfo
		(
			nameof(FrameThreadContext.PThread_Init_Cnt),
			[
				nameof(FrameThreadContext.Buffer_Mutex),
				nameof(FrameThreadContext.HwAccel_Mutex),
				nameof(FrameThreadContext.Async_Mutex)
			],
			[
				nameof(FrameThreadContext.Async_Cond)
			]
		);

		private static readonly PThread_ThreadInfo per_Thread_Fields = new PThread_ThreadInfo
		(
			nameof(PerThreadContext.PThread_Init_Cnt),
			[
				nameof(PerThreadContext.Progress_Mutex),
				nameof(PerThreadContext.Mutex)
			],
			[
				nameof(PerThreadContext.Input_Cond),
				nameof(PerThreadContext.Progress_Cond),
				nameof(PerThreadContext.Output_Cond)
			]
		);

		/********************************************************************/
		/// <summary>
		/// Submit available packets for decoding to worker threads, return a
		/// decoded frame if available. Returns AVERROR(EAGAIN) if none is
		/// available.
		///
		/// Parameters are the same as FFCodec.receive_frame
		/// </summary>
		/********************************************************************/
		public static c_int FF_Thread_Receive_Frame(AvCodecContext avCtx, AvFrame frame)//XX 561
		{
			FrameThreadContext fCtx = (FrameThreadContext)avCtx.Internal.Thread_Ctx;
			c_int ret = 0;

			// Release the async lock, permitting blocked hwaccel threads to
			// go forward while we are in this function
			Async_Unlock(fCtx);

			// Submit packets to threads while there are no buffered results to return
			while ((fCtx.Df.Nb_F == 0) && (fCtx.Result == 0))
			{
				// Get a packet to be submitted to the next thread
				Packet.Av_Packet_Unref(fCtx.Next_Pkt);

				ret = Decode.FF_Decode_Get_Packet(avCtx, fCtx.Next_Pkt);

				if ((ret < 0) && (ret != Error.EOF))
					goto Finish;

				ret = Submit_Packet(fCtx.Threads[fCtx.Next_Decoding], avCtx, fCtx.Next_Pkt);

				if (ret < 0)
					goto Finish;

				// Do not return any frames until all threads have something to do
				if ((fCtx.Next_Decoding != fCtx.Next_Finished) && (avCtx.Internal.Draining == 0))
					continue;

				PerThreadContext p = fCtx.Threads[fCtx.Next_Finished];
				fCtx.Next_Finished = (fCtx.Next_Finished + 1) % avCtx.Thread_Count;

				if (StdAtomic.Atomic_Load(ref p.State) != PThreadState.Input_Ready)
				{
					CThread.pthread_mutex_lock(p.Progress_Mutex);

					while (StdAtomic.Atomic_Load(ref p.State) != PThreadState.Input_Ready)
						CThread.pthread_cond_wait(p.Output_Cond, p.Progress_Mutex);

					CThread.pthread_mutex_unlock(p.Progress_Mutex);
				}

				Update_Context_From_Thread(avCtx, p.AvCtx, 1);

				fCtx.Result = p.Result;
				p.Result = 0;

				if (p.Df.Nb_F != 0)
					Macros.FFSwapObj(fCtx.Df, p.Df);
			}

			// A thread may return multiple frames AND an error
			// we first return all the frames, then the error
			if (fCtx.Df.Nb_F != 0)
			{
				Decoded_Frames_Pop(fCtx.Df, frame);

				ret = 0;
			}
			else
			{
				ret = fCtx.Result;
				fCtx.Result = 0;
			}

			Finish:
			Async_Lock(fCtx);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// If the codec defines update_thread_context(), call this when
		/// they are ready for the next thread to start decoding the next
		/// frame. After calling it, do not change any variables read by the
		/// update_thread_context() method, or call ff_thread_get_buffer()
		/// </summary>
		/********************************************************************/
		public static void FF_Thread_Finish_Setup(AvCodecContext avCtx)//XX 666
		{
			if ((avCtx.Active_Thread_Type & FFThread.Frame) == 0)
				return;

			PerThreadContext p = (PerThreadContext)avCtx.Internal.Thread_Ctx;

			p.HwAccel_Threadsafe = (avCtx.HwAccel != null) && ((HwAccel_Internal.FFHwAccel(avCtx.HwAccel).Caps_Internal & HwAccelCap.Thread_Safe) != 0) ? 1 : 0;

			if ((HwAccel_Serial(avCtx) != 0) && (p.HwAccel_Serializing == 0))
			{
				CThread.pthread_mutex_lock(p.Parent.HwAccel_Mutex);

				p.HwAccel_Serializing = 1;
			}

			// This assumes that no hwaccel calls happen before ff_thread_finish_setup()
			if ((avCtx.HwAccel != null) && ((HwAccel_Internal.FFHwAccel(avCtx.HwAccel).Caps_Internal & HwAccelCap.Async_Safe) == 0))
			{
				p.Async_Serializing = 1;

				Async_Lock(p.Parent);
			}

			// Thread-unsafe hwaccels share a single private data instance, so we
			// save hwaccel state for passing to the next thread;
			// this is done here so that this worker thread can wipe its own hwaccel
			// state after decoding, without requiring synchronization
			if (HwAccel_Serial(avCtx) != 0)
			{
				p.Parent.Stash_HwAccel = avCtx.HwAccel;
				p.Parent.Stash_HwAccel_Context = avCtx.HwAccel_Context;
				p.Parent.Stash_HwAccel_Priv = avCtx.Internal.HwAccel_Priv_Data;
			}

			CThread.pthread_mutex_lock(p.Progress_Mutex);

			if (StdAtomic.Atomic_Load(ref p.State) == PThreadState.Setup_Finished)
				Log.Av_Log(avCtx, Log.Av_Log_Warning, "Multiple ff_thread_finish_setup() calls\n");

			StdAtomic.Atomic_Store(ref p.State, PThreadState.Setup_Finished);

			CThread.pthread_cond_broadcast(p.Progress_Cond);
			CThread.pthread_mutex_unlock(p.Progress_Mutex);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void FF_Frame_Thread_Free(AvCodecContext avCtx, c_int thread_Count)//XX 744
		{
			FrameThreadContext fCtx = (FrameThreadContext)avCtx.Internal.Thread_Ctx;
			FFCodec codec = Codec_Internal.FFCodec(avCtx.Codec);

			Park_Frame_Worker_Threads(fCtx, thread_Count);

			for (c_int i = 0; i < thread_Count; i++)
			{
				PerThreadContext p = fCtx.Threads[i];
				AvCodecContext ctx = p.AvCtx;

				if (ctx.Internal != null)
				{
					if (p.Thread_Init == ThreadFlag.Initialized)
					{
						CThread.pthread_mutex_lock(p.Mutex);
						p.Die = 1;
						CThread.pthread_cond_signal(p.Input_Cond);
						CThread.pthread_mutex_unlock(p.Mutex);

						CThread.pthread_join(p.Thread);
					}

					if ((codec.Close != null) && (p.Thread_Init != ThreadFlag.Uninitialized))
						codec.Close(ctx);

					// When using a threadsafe hwaccell, this is where
					// each thread's context is uninit'd and freed
					Decode.FF_HwAccel_Uninit(ctx);

					if (ctx.Priv_Data != null)
					{
						if (codec.P.Priv_Class != null)
							Opt.Av_Opt_Free(ctx.Priv_Data);

						Mem.Av_FreeP(ref ctx.Priv_Data);
					}

					RefStruct.Av_RefStruct_Unref(ref ctx.Internal.Pool);
					Packet.Av_Packet_Free(ref ctx.Internal.In_Pkt);
					Packet.Av_Packet_Free(ref ctx.Internal.Last_Pkt_Props);
					Decode.FF_Decode_Internal_Uninit(ctx);
					Mem.Av_FreeP(ref ctx.Internal);
					Buffer.Av_Buffer_Unref(ref ctx.Hw_Frames_Ctx);
					Side_Data.Av_Frame_Side_Data_Free(ref ctx.Decoded_Side_Data, ref ctx.Nb_Decoded_Side_Data);
				}

				Decoded_Frames_Free(p.Df);

				PThread.FF_PThread_Free(p, per_Thread_Fields);
				Packet.Av_Packet_Free(ref p.AvPkt);

				Mem.Av_FreeP(ref p.AvCtx);
			}

			Decoded_Frames_Free(fCtx.Df);
			Packet.Av_Packet_Free(ref fCtx.Next_Pkt);

			Mem.Av_FreeP(ref fCtx.Threads);
			PThread.FF_PThread_Free(fCtx, thread_Ctx_Fields);

			// If we have stashed hwaccel state, move it to the user-facing context,
			// so it will be freed in ff_codec_close()
			Macros.FFSwap(ref avCtx.HwAccel, ref fCtx.Stash_HwAccel);
			Macros.FFSwap(ref avCtx.HwAccel_Context, ref fCtx.Stash_HwAccel_Context);
			Macros.FFSwap(ref avCtx.Internal.HwAccel_Priv_Data, ref fCtx.Stash_HwAccel_Priv);

			Mem.Av_FreeP(ref avCtx.Internal.Thread_Ctx);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int FF_Frame_Thread_Init(AvCodecContext avCtx)//XX 909
		{
			c_int thread_Count = avCtx.Thread_Count;
			FFCodec codec = Codec_Internal.FFCodec(avCtx.Codec);
			FrameThreadContext fCtx;
			c_int i = 0;

			if (thread_Count == 0)
			{
				c_int nb_Cpus = Cpu.Av_Cpu_Count();

				// Use number of cores + 1 as thread count if there is more than one
				if (nb_Cpus > 1)
					thread_Count = avCtx.Thread_Count = Macros.FFMin(nb_Cpus + 1, UtilConstants.Max_Auto_Threads);
				else
					thread_Count = avCtx.Thread_Count = 1;
			}

			if (thread_Count <= 1)
			{
				avCtx.Active_Thread_Type = FFThread.None;

				return 0;
			}

			avCtx.Internal.Thread_Ctx = fCtx = Mem.Av_MAlloczObj<FrameThreadContext>();

			if (fCtx == null)
				return Error.ENOMEM;

			c_int err = PThread.FF_PThread_Init(fCtx, thread_Ctx_Fields);

			if (err < 0)
			{
				PThread.FF_PThread_Free(fCtx, thread_Ctx_Fields);

				Mem.Av_FreeP(ref avCtx.Internal.Thread_Ctx);

				return err;
			}

			fCtx.Next_Pkt = Packet.Av_Packet_Alloc();

			if (fCtx.Next_Pkt == null)
				return Error.ENOMEM;

			fCtx.Async_Lock = 1;

			if (codec.P.Type == AvMediaType.Video)
				avCtx.Delay = avCtx.Thread_Count - 1;

			fCtx.Threads = Mem.Av_CAllocObj<PerThreadContext>((size_t)thread_Count);

			if (fCtx.Threads == null)
			{
				err = Error.ENOMEM;

				goto Error;
			}

			for (; i < thread_Count;)
			{
				PerThreadContext p = fCtx.Threads[i];
				c_int first = i == 0 ? 1 : 0;

				err = Init_Thread(p, ref i, fCtx, avCtx, codec, first);

				if (err < 0)
					goto Error;
			}

			return 0;

			Error:
			FF_Frame_Thread_Free(avCtx, i);

			return err;
		}



		/********************************************************************/
		/// <summary>
		/// Get a packet for decoding. This gets invoked by the worker
		/// threads
		/// </summary>
		/********************************************************************/
		public static c_int FF_Thread_Get_Packet(AvCodecContext avCtx, AvPacket pkt)//XX 1094
		{
			PerThreadContext p = (PerThreadContext)avCtx.Internal.Thread_Ctx;

			if (!Packet.AvPacket_Is_Empty(p.AvPkt))
			{
				Packet.Av_Packet_Move_Ref(pkt, p.AvPkt);

				return 0;
			}

			return avCtx.Internal.Draining != 0 ? Error.EOF : Error.EAGAIN;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int HwAccel_Serial(AvCodecContext avCtx)//XX 157
		{
			return (avCtx.HwAccel != null) && ((HwAccel_Internal.FFHwAccel(avCtx.HwAccel).Caps_Internal & HwAccelCap.Thread_Safe) == 0) ? 1 : 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Async_Lock(FrameThreadContext fCtx)//XX 162
		{
			CThread.pthread_mutex_lock(fCtx.Async_Mutex);

			while (fCtx.Async_Lock != 0)
				CThread.pthread_cond_wait(fCtx.Async_Cond, fCtx.Async_Mutex);

			fCtx.Async_Lock = 1;

			CThread.pthread_mutex_unlock(fCtx.Async_Mutex);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Async_Unlock(FrameThreadContext fCtx)//XX 171
		{
			CThread.pthread_mutex_lock(fCtx.Async_Mutex);

			fCtx.Async_Lock = 0;
			CThread.pthread_cond_broadcast(fCtx.Async_Cond);

			CThread.pthread_mutex_unlock(fCtx.Async_Mutex);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Thread_Set_Name(PerThreadContext p)//XX 180
		{
		}



		/********************************************************************/
		/// <summary>
		/// Get a free frame to decode into
		/// </summary>
		/********************************************************************/
		private static AvFrame Decoded_Frames_Get_Free(DecodedFrames df)//XX 192
		{
			if (df.Nb_F == df.Nb_F_Allocated)
			{
				CPointer<AvFrame> tmp = Mem.Av_Realloc_ArrayObj(df.F, df.Nb_F + 1);

				if (tmp.IsNull)
					return null;

				df.F = tmp;

				df.F[df.Nb_F] = Frame.Av_Frame_Alloc();

				if (df.F[df.Nb_F] == null)
					return null;

				df.Nb_F_Allocated++;
			}

			return df.F[df.Nb_F];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Decoded_Frames_Pop(DecodedFrames df, AvFrame dst)//XX 213
		{
			AvFrame tmp_Frame = df.F[0];

			Frame.Av_Frame_Move_Ref(dst, tmp_Frame);
			CMemory.memmove(df.F, df.F + 1, df.Nb_F - 1);

			df.F[--df.Nb_F] = tmp_Frame;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Decoded_Frames_Free(DecodedFrames df)//XX 228
		{
			for (size_t i = 0; i < df.Nb_F_Allocated; i++)
				Frame.Av_Frame_Free(ref df.F[i]);

			Mem.Av_FreeP(ref df.F);
			df.Nb_F = 0;
			df.Nb_F_Allocated = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Codec worker thread.
		///
		/// Automatically calls ff_thread_finish_setup() if the codec does
		/// not provide an update_thread_context method, or if the codec
		/// returns before calling it
		/// </summary>
		/********************************************************************/
		private static void Frame_Worker_Thread(object arg)//XX 244
		{
			PerThreadContext p = (PerThreadContext)arg;
			AvCodecContext avCtx = p.AvCtx;
			FFCodec codec = Codec_Internal.FFCodec(avCtx.Codec);

			Thread_Set_Name(p);

			CThread.pthread_mutex_lock(p.Mutex);

			while (true)
			{
				while ((StdAtomic.Atomic_Load(ref p.State) == PThreadState.Input_Ready) && (p.Die == 0))
					CThread.pthread_cond_wait(p.Input_Cond, p.Mutex);

				if (p.Die != 0)
					break;

				if (codec.Update_Thread_Context == null)
					FF_Thread_Finish_Setup(avCtx);

				// If the previous thread uses thread-unsafe hwaccel then we take the
				// lock to ensure the threads don't run concurrently
				if (HwAccel_Serial(avCtx) != 0)
				{
					CThread.pthread_mutex_lock(p.Parent.HwAccel_Mutex);

					p.HwAccel_Serializing = 1;
				}

				c_int ret = 0;

				while (ret >= 0)
				{
					// Get the frame which will store the output
					AvFrame frame = Decoded_Frames_Get_Free(p.Df);

					if (frame == null)
					{
						p.Result = Error.ENOMEM;

						goto Alloc_Fail;
					}

					// Do the actual decoding
					ret = Decode.FF_Decode_Receive_Frame_Internal(avCtx, frame);

					if (ret == 0)
						p.Df.Nb_F++;
					else if ((ret < 0) && (frame.Buf[0] != null))
						Frame.Av_Frame_Unref(frame);

					p.Result = (ret == Error.EAGAIN) ? 0 : ret;
				}

				if (StdAtomic.Atomic_Load(ref p.State) == PThreadState.Setting_Up)
					FF_Thread_Finish_Setup(avCtx);

				Alloc_Fail:
				if (p.HwAccel_Serializing != 0)
				{
					// Wipe hwaccel state for thread-unsafe hwaccels to avoid stale
					// pointers lying around;
					// the state was transferred to FrameThreadContext in
					// ff_thread_finish_setup(), so nothing is leaked
					avCtx.HwAccel = null;
					avCtx.HwAccel_Context = null;
					avCtx.Internal.HwAccel_Priv_Data = null;

					p.HwAccel_Serializing = 0;

					CThread.pthread_mutex_unlock(p.Parent.HwAccel_Mutex);
				}

				if (p.Async_Serializing != 0)
				{
					p.Async_Serializing = 0;

					Async_Unlock(p.Parent);
				}

				CThread.pthread_mutex_lock(p.Progress_Mutex);

				StdAtomic.Atomic_Store(ref p.State, PThreadState.Input_Ready);

				CThread.pthread_cond_broadcast(p.Progress_Cond);
				CThread.pthread_cond_signal(p.Output_Cond);
				CThread.pthread_mutex_unlock(p.Progress_Mutex);
			}

			CThread.pthread_mutex_unlock(p.Mutex);
		}



		/********************************************************************/
		/// <summary>
		/// Update the next thread's AVCodecContext with values from the
		/// reference thread's context
		/// </summary>
		/********************************************************************/
		private static c_int Update_Context_From_Thread(AvCodecContext dst, AvCodecContext src, c_int for_User)//XX 346
		{
			FFCodec codec = Codec_Internal.FFCodec(dst.Codec);
			c_int err = 0;

			if ((dst != src) && ((for_User != 0) || (codec.Update_Thread_Context != null)))
			{
				dst.Time_Base = src.Time_Base;
				dst.FrameRate = src.FrameRate;
				dst.PictureSize.Width = src.PictureSize.Width;
				dst.PictureSize.Height = src.PictureSize.Height;
				dst.Pix_Fmt = src.Pix_Fmt;
				dst.Sw_Pix_Fmt = src.Sw_Pix_Fmt;

				dst.Coded_Width = src.Coded_Width;
				dst.Coded_Height = src.Coded_Height;

				dst.Has_B_Frames = src.Has_B_Frames;
				dst.Idct_Algo = src.Idct_Algo;

				dst.Bits_Per_Coded_Sample = src.Bits_Per_Coded_Sample;
				dst.Sample_Aspect_Ratio = src.Sample_Aspect_Ratio;

				dst.Profile = src.Profile;
				dst.Level = src.Level;

				dst.Bits_Per_Raw_Sample = src.Bits_Per_Raw_Sample;
				dst.Color_Primaries = src.Color_Primaries;

				dst.Color_Trc = src.Color_Trc;
				dst.ColorSpace = src.ColorSpace;
				dst.Color_Range = src.Color_Range;
				dst.Chroma_Sample_Location = src.Chroma_Sample_Location;

				dst.Sample_Rate = src.Sample_Rate;
				dst.Sample_Fmt = src.Sample_Fmt;

				err = Channel_Layout.Av_Channel_Layout_Copy(dst.Ch_Layout, src.Ch_Layout);

				if (err < 0)
					return err;

				if (((dst.Hw_Frames_Ctx != null) != (src.Hw_Frames_Ctx != null)) || ((dst.Hw_Frames_Ctx != null) && (dst.Hw_Frames_Ctx.Data != src.Hw_Frames_Ctx.Data)))
				{
					Buffer.Av_Buffer_Unref(ref dst.Hw_Frames_Ctx);

					if (src.Hw_Frames_Ctx != null)
					{
						dst.Hw_Frames_Ctx = Buffer.Av_Buffer_Ref(src.Hw_Frames_Ctx);

						if (dst.Hw_Frames_Ctx == null)
							return Error.ENOMEM;
					}
				}

				dst.HwAccel_Flags = src.HwAccel_Flags;

				RefStruct.Av_RefStruct_Replace(ref dst.Internal.Pool, src.Internal.Pool);
				Decode.FF_Decode_Internal_Sync(dst, src);
			}

			if (for_User != 0)
			{
				if (codec.Update_Thread_Context_For_User != null)
					err = codec.Update_Thread_Context_For_User(dst, src);
			}
			else
			{
				PerThreadContext p_Src = (PerThreadContext)src.Internal.Thread_Ctx;
				PerThreadContext p_Dst = (PerThreadContext)dst.Internal.Thread_Ctx;

				if (codec.Update_Thread_Context != null)
				{
					err = codec.Update_Thread_Context(dst, src);

					if (err < 0)
						return err;
				}

				// Reset dst hwaccel state if needed
				if ((p_Dst.HwAccel_Threadsafe != 0) && ((p_Src.HwAccel_Threadsafe == 0) || (dst.HwAccel != src.HwAccel)))
				{
					Decode.FF_HwAccel_Uninit(dst);

					p_Dst.HwAccel_Threadsafe = 0;
				}

				// Propagate hwaccel state for threadsafe hwaccels
				if (p_Src.HwAccel_Threadsafe != 0)
				{
					FFHwAccel hwAccel = HwAccel_Internal.FFHwAccel(src.HwAccel);

					if (dst.HwAccel == null)
					{
						if (hwAccel.Priv_Data_Alloc != null)
						{
							dst.Internal.HwAccel_Priv_Data = hwAccel.Priv_Data_Alloc();

							if (dst.Internal.HwAccel_Priv_Data == null)
								return Error.ENOMEM;
						}

						dst.HwAccel = src.HwAccel;
					}

					if (hwAccel.Update_Thread_Context != null)
					{
						err = hwAccel.Update_Thread_Context(dst, src);

						if (err < 0)
						{
							Log.Av_Log(dst, Log.Av_Log_Error, "Error propagating hwaccel state\n");

							Decode.FF_HwAccel_Uninit(dst);

							return err;
						}
					}

					p_Dst.HwAccel_Threadsafe = 1;
				}
			}

			return err;
		}



		/********************************************************************/
		/// <summary>
		/// Update the next thread's AVCodecContext with values set by the
		/// user
		/// </summary>
		/********************************************************************/
		private static c_int Update_Context_From_User(AvCodecContext dst, AvCodecContext src)//XX 467
		{
			dst.Flags = src.Flags;

			dst.Draw_Horiz_Band = src.Draw_Horiz_Band;
			dst.Get_Buffer2 = src.Get_Buffer2;

			dst.Opaque = src.Opaque;
			dst.Debug = src.Debug;

			dst.Slice_Flags = src.Slice_Flags;
			dst.Flags2 = src.Flags2;
			dst.Export_Side_Data = src.Export_Side_Data;

			dst.Skip_Loop_Filter = src.Skip_Loop_Filter;
			dst.Skip_Idct = src.Skip_Idct;
			dst.Skip_Frame = src.Skip_Frame;

			dst.Frame_Num = src.Frame_Num;

			Packet.Av_Packet_Unref(dst.Internal.Last_Pkt_Props);

			c_int err = Packet.Av_Packet_Copy_Props(dst.Internal.Last_Pkt_Props, src.Internal.Last_Pkt_Props);

			if (err < 0)
				return err;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Submit_Packet(PerThreadContext p, AvCodecContext user_AvCtx, AvPacket in_Pkt)//XX 497
		{
			FrameThreadContext fCtx = p.Parent;
			PerThreadContext prev_Thread = fCtx.Prev_Thread;
			AvCodec codec = p.AvCtx.Codec;

			CThread.pthread_mutex_lock(p.Mutex);

			Packet.Av_Packet_Unref(p.AvPkt);
			Packet.Av_Packet_Move_Ref(p.AvPkt, in_Pkt);

			if (Packet.AvPacket_Is_Empty(p.AvPkt))
				p.AvCtx.Internal.Draining = 1;

			c_int ret = Update_Context_From_User(p.AvCtx, user_AvCtx);

			if (ret != 0)
			{
				CThread.pthread_mutex_unlock(p.Mutex);

				return ret;
			}

			if (prev_Thread != null)
			{
				if (StdAtomic.Atomic_Load(ref prev_Thread.State) == PThreadState.Setting_Up)
				{
					CThread.pthread_mutex_lock(prev_Thread.Progress_Mutex);

					while (StdAtomic.Atomic_Load(ref prev_Thread.State) == PThreadState.Setting_Up)
						CThread.pthread_cond_wait(prev_Thread.Progress_Cond, prev_Thread.Progress_Mutex);

					CThread.pthread_mutex_unlock(prev_Thread.Progress_Mutex);
				}

				// Codecs without delay might not be prepared to be called repeatedly here during
				// flushing (vp3/theora), and also don't need to be, since from this point on, they
				// will always return EOF anyway
				if ((p.AvCtx.Internal.Draining == 0) || ((codec.Capabilities & AvCodecCap.Delay) != 0))
				{
					ret = Update_Context_From_Thread(p.AvCtx, prev_Thread.AvCtx, 0);

					if (ret != 0)
					{
						CThread.pthread_mutex_unlock(p.Mutex);

						return ret;
					}
				}
			}

			// Transfer the stashed hwaccel state, if any
			if (p.HwAccel_Threadsafe == 0)
			{
				Macros.FFSwap(ref p.AvCtx.HwAccel, ref fCtx.Stash_HwAccel);
				Macros.FFSwap(ref p.AvCtx.HwAccel_Context, ref fCtx.Stash_HwAccel_Context);
				Macros.FFSwap(ref p.AvCtx.Internal.HwAccel_Priv_Data, ref fCtx.Stash_HwAccel_Priv);
			}

			StdAtomic.Atomic_Store(ref p.State, PThreadState.Setting_Up);

			CThread.pthread_cond_signal(p.Input_Cond);
			CThread.pthread_mutex_unlock(p.Mutex);

			fCtx.Prev_Thread = p;
			fCtx.Next_Decoding = (fCtx.Next_Decoding + 1) % p.AvCtx.Thread_Count;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Waits for all threads to finish
		/// </summary>
		/********************************************************************/
		private static void Park_Frame_Worker_Threads(FrameThreadContext fCtx, c_int thread_Count)//XX 712
		{
			Async_Unlock(fCtx);

			for (c_int i = 0; i < thread_Count; i++)
			{
				PerThreadContext p = fCtx.Threads[i];

				if (StdAtomic.Atomic_Load(ref p.State) != PThreadState.Input_Ready)
				{
					CThread.pthread_mutex_lock(p.Progress_Mutex);

					while (StdAtomic.Atomic_Load(ref p.State) != PThreadState.Input_Ready)
						CThread.pthread_cond_wait(p.Output_Cond, p.Progress_Mutex);

					CThread.pthread_mutex_unlock(p.Progress_Mutex);
				}
			}

			Async_Lock(fCtx);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Init_Thread(PerThreadContext p, ref c_int threads_To_Free, FrameThreadContext fCtx, AvCodecContext avCtx, FFCodec codec, c_int first)//XX 812
		{
			c_int err;

			StdAtomic.Atomic_Init(ref p.State, PThreadState.Input_Ready);

			AvCodecContext copy = Mem.Av_MemDupObj(avCtx);

			if (copy == null)
				return Error.ENOMEM;

			copy.Priv_Data = null;
			copy.Decoded_Side_Data.SetToNull();
			copy.Nb_Decoded_Side_Data = 0;

			// From now on, this PerThreadContext will be cleaned up by
			// ff_frame_thread_free in case of errors
			threads_To_Free++;

			p.Parent = fCtx;
			p.AvCtx = copy;

			copy.Internal = Decode.FF_Decode_Internal_Alloc();

			if (copy.Internal == null)
				return Error.ENOMEM;

			Decode.FF_Decode_Internal_Sync(copy, avCtx);

			copy.Internal.Thread_Ctx = p;
			copy.Internal.Progress_Frame_Pool = avCtx.Internal.Progress_Frame_Pool;

			copy.Delay = avCtx.Delay;

			if (codec.Priv_Data_Alloc != null)
			{
				copy.Priv_Data = codec.Priv_Data_Alloc();

				if (copy.Priv_Data == null)
					return Error.ENOMEM;

				if (codec.P.Priv_Class != null)
				{
					codec.P.Priv_Class.CopyTo(copy.Priv_Data);

					err = Opt.Av_Opt_Copy(copy.Priv_Data, avCtx.Priv_Data);

					if (err < 0)
						return err;
				}
			}

			err = PThread.FF_PThread_Init(p, per_Thread_Fields);

			if (err < 0)
				return err;

			p.AvPkt = Packet.Av_Packet_Alloc();

			if (p.AvPkt == null)
				return Error.ENOMEM;

			copy.Internal.Is_Frame_Mt = 1;

			if (first == 0)
				copy.Internal.Is_Copy = 1;

			copy.Internal.In_Pkt = Packet.Av_Packet_Alloc();

			if (copy.Internal.In_Pkt == null)
				return Error.ENOMEM;

			copy.Internal.Last_Pkt_Props = Packet.Av_Packet_Alloc();

			if (copy.Internal.Last_Pkt_Props == null)
				return Error.ENOMEM;

			if (codec.Init != null)
			{
				err = codec.Init(copy);

				if (err < 0)
				{
					if ((codec.Caps_Internal & FFCodecCap.Init_Cleanup) != 0)
						p.Thread_Init = ThreadFlag.Needs_Close;

					return err;
				}
			}

			p.Thread_Init = ThreadFlag.Needs_Close;

			if (first != 0)
			{
				Update_Context_From_Thread(avCtx, copy, 1);

				Side_Data.Av_Frame_Side_Data_Free(ref avCtx.Decoded_Side_Data, ref avCtx.Nb_Decoded_Side_Data);

				for (c_int i = 0; i < copy.Nb_Decoded_Side_Data; i++)
				{
					err = Side_Data.Av_Frame_Side_Data_Clone(ref avCtx.Decoded_Side_Data, ref avCtx.Nb_Decoded_Side_Data, copy.Decoded_Side_Data[i], AvFrameSideDataFlag.None);

					if (err < 0)
						return err;
				}
			}

			err = CThread.pthread_create(out p.Thread, Frame_Worker_Thread, p);

			if (err < 0)
				return err;

			p.Thread_Init = ThreadFlag.Initialized;

			return 0;
		}
		#endregion
	}
}
