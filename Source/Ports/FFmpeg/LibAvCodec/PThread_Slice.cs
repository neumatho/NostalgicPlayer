/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Interfaces;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// Slice multithreading support functions
	/// </summary>
	internal static class PThread_Slice
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void FF_Slice_Thread_Free(AvCodecContext avCtx)//XX 69
		{
			SliceThreadContext c = (SliceThreadContext)avCtx.Internal.Thread_Ctx;

			SliceThread.AvPriv_SliceThread_Free(ref c.Thread);

			Mem.Av_FreeP(ref avCtx.Internal.Thread_Ctx);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int FF_Slice_Thread_Init(AvCodecContext avCtx)//XX 112
		{
			SliceThreadContext c;

			c_int thread_Count = avCtx.Thread_Count;

			if (thread_Count == 0)
			{
				c_int nb_Cpus = Cpu.Av_Cpu_Count();

				if (avCtx.PictureSize.Height != 0)
					nb_Cpus = Macros.FFMin(nb_Cpus, (avCtx.PictureSize.Height + 15) / 16);

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

			avCtx.Internal.Thread_Ctx = c = Mem.Av_MAlloczObj<SliceThreadContext>();

			if (c == null)
				return Error.ENOMEM;

			UtilFunc.Thread_Main_Func_Delegate mainFunc = (Codec_Internal.FFCodec(avCtx.Codec).Caps_Internal & FFCodecCap.Slice_Thread_Has_Mf) != 0 ? Main_Function : null;
			thread_Count = SliceThread.AvPriv_SliceThread_Create(out c.Thread, avCtx, Worker_Func, mainFunc, thread_Count);

			if (thread_Count <= 1)
			{
				FF_Slice_Thread_Free(avCtx);

				avCtx.Thread_Count = 1;
				avCtx.Active_Thread_Type = FFThread.None;

				return thread_Count < 0 ? thread_Count : 0;
			}

			avCtx.Thread_Count = thread_Count;

			avCtx.Execute = Thread_Execute;
			avCtx.Execute2 = Thread_Execute2;

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Main_Function(IOpaque priv)//XX 51
		{
			AvCodecContext avCtx = (AvCodecContext)priv;
			SliceThreadContext c = (SliceThreadContext)avCtx.Internal.Thread_Ctx;

			c.MainFunc(avCtx);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Worker_Func(IOpaque priv, c_int jobNr, c_int threadNr, c_int nb_Jobs, c_int nb_Threads)//XX 57
		{
			AvCodecContext avCtx = (AvCodecContext)priv;
			SliceThreadContext c = (SliceThreadContext)avCtx.Internal.Thread_Ctx;

			c_int ret = c.Func != null ? c.Func(avCtx, c.Args[jobNr]) : c.Func2(avCtx, c.Args[0], jobNr, threadNr);

			if (c.Rets.IsNotNull)
				c.Rets[jobNr] = ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Thread_Execute(AvCodecContext avCtx, CodecFunc.Execute_Func_Delegate func, CPointer<IExecuteArg> arg, CPointer<c_int> ret, c_int job_Count)//XX 78
		{
			SliceThreadContext c = (SliceThreadContext)avCtx.Internal.Thread_Ctx;

			if (((avCtx.Active_Thread_Type & FFThread.Slice) == 0) || (avCtx.Thread_Count <= 1))
				return AvCodec_.AvCodec_Default_Execute(avCtx, func, arg, ret, job_Count);

			if (job_Count <= 0)
				return 0;

			c.Args = arg;
			c.Func = func;
			c.Rets = ret;

			SliceThread.AvPriv_SliceThread_Execute(c.Thread, job_Count, c.MainFunc != null ? 1 : 0);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Thread_Execute2(AvCodecContext avCtx, CodecFunc.Execute2_Func_Delegate func2, CPointer<IExecuteArg> arg, CPointer<c_int> ret, c_int job_Count)//XX 97
		{
			SliceThreadContext c = (SliceThreadContext)avCtx.Internal.Thread_Ctx;

			c.Func2 = func2;

			return Thread_Execute(avCtx, null, arg, ret, job_Count);
		}
		#endregion
	}
}
