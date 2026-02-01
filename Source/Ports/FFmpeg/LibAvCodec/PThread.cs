/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Reflection;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// Multithreading support functions
	/// </summary>
	internal static class PThread
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int FF_Thread_Init(AvCodecContext avCtx)//XX 72
		{
			Validate_Thread_Parameters(avCtx);

			if ((avCtx.Active_Thread_Type & FFThread.Slice) != 0)
				return PThread_Slice.FF_Slice_Thread_Init(avCtx);
			else if ((avCtx.Active_Thread_Type & FFThread.Frame) != 0)
				return PThread_Frame.FF_Frame_Thread_Init(avCtx);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void FF_Thread_Free(AvCodecContext avCtx)//XX 84
		{
			if ((avCtx.Active_Thread_Type & FFThread.Frame) != 0)
				PThread_Frame.FF_Frame_Thread_Free(avCtx, avCtx.Thread_Count);
			else
				PThread_Slice.FF_Slice_Thread_Free(avCtx);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void FF_PThread_Free(IContext obj, PThread_ThreadInfo threadInfo)//XX 92
		{
			Type type = obj.GetType();

			FieldInfo fieldInfo = type.GetField(threadInfo.Counter);
			fieldInfo.SetValue(obj, 0U);

			foreach (string mutexName in threadInfo.Mutexes)
			{
				fieldInfo = type.GetField(mutexName);
				CThread.pthread_mutex_destroy((pthread_mutex_t)fieldInfo.GetValue(obj));
			}

			foreach (string condName in threadInfo.Conds)
			{
				fieldInfo = type.GetField(condName);
				CThread.pthread_cond_destroy((pthread_cond_t)fieldInfo.GetValue(obj));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int FF_PThread_Init(IContext obj, PThread_ThreadInfo threadInfo)//XX 105
		{
			FieldInfo fieldInfo;

			c_uint cnt = 0;
			c_int err = 0;

			Type type = obj.GetType();

			foreach (string mutexName in threadInfo.Mutexes)
			{
				fieldInfo = type.GetField(mutexName);

				err = CThread.pthread_mutex_init(out pthread_mutex_t mutex);

				if (err != 0)
					goto Fail;

				fieldInfo.SetValue(obj, mutex);
				cnt++;
			}

			foreach (string condName in threadInfo.Conds)
			{
				fieldInfo = type.GetField(condName);

				err = CThread.pthread_cond_init(out pthread_cond_t cond);

				if (err != 0)
					goto Fail;

				fieldInfo.SetValue(obj, cond);
				cnt++;
			}

			Fail:
			fieldInfo = type.GetField(threadInfo.Counter);
			fieldInfo.SetValue(obj, cnt);

			return err;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Set the threading algorithms used.
		///
		/// Threading requires more than one thread.
		/// Frame threading requires entire frames to be passed to the codec,
		/// and introduces extra decoding delay, so is incompatible with
		/// low_delay
		/// </summary>
		/********************************************************************/
		private static void Validate_Thread_Parameters(AvCodecContext avCtx)
		{
			bool frame_Threading_Supported = ((avCtx.Codec.Capabilities & AvCodecCap.Frame_Threads) != 0) && ((avCtx.Flags & AvCodecFlag.Low_Delay) == 0) && ((avCtx.Flags2 & AvCodecFlag2.Chunks) == 0);

			if (avCtx.Thread_Count == 1)
				avCtx.Active_Thread_Type = FFThread.None;
			else if (frame_Threading_Supported && ((avCtx.Thread_Type & FFThread.Frame) != 0))
				avCtx.Active_Thread_Type = FFThread.Frame;
			else if (((avCtx.Codec.Capabilities & AvCodecCap.Slice_Threads) != 0) && ((avCtx.Thread_Type & FFThread.Slice) != 0))
				avCtx.Active_Thread_Type = FFThread.Slice;
			else if ((Codec_Internal.FFCodec(avCtx.Codec).Caps_Internal & FFCodecCap.Auto_Threads) == 0)
			{
				avCtx.Thread_Count = 1;
				avCtx.Thread_Type = FFThread.None;
			}

			if (avCtx.Thread_Count > UtilConstants.Max_Auto_Threads)
				Log.Av_Log(avCtx, Log.Av_Log_Warning, "Application has requested %d threads. Using a thread count greater than %d is not recommended.\n");
		}
		#endregion
	}
}
