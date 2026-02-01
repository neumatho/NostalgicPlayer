/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Compat;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// 
	/// </summary>
	public static class ThreadProgress_
	{
		private static readonly PThread_ThreadInfo thread_Progress_Fields = new PThread_ThreadInfo
		(
			nameof(ThreadProgress.Init),
			[
				nameof(ThreadProgress.Progress_Mutex)
			],
			[
				nameof(ThreadProgress.Progress_Cond)
			]
		);

		/********************************************************************/
		/// <summary>
		/// Reset the ::ThreadProgress.progress counter; must only be called
		/// if the ThreadProgress is not in use in any way (e.g. no thread
		/// may wait on it via ff_thread_progress_await())
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FF_Thread_Progress_Reset(ThreadProgress pro)
		{
			StdAtomic.Atomic_Init(ref pro.Progress, pro.Init != 0 ? -1 : c_int.MaxValue);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize a ThreadProgress
		/// </summary>
		/********************************************************************/
		public static c_int FF_Thread_Progress_Init(ThreadProgress pro, c_int init_Mode)//XX 33
		{
			StdAtomic.Atomic_Init(ref pro.Progress, init_Mode != 0 ? -1 : c_int.MaxValue);

			if (init_Mode != 0)
				return PThread.FF_PThread_Init(pro, thread_Progress_Fields);

			pro.Init = (c_uint)init_Mode;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Destroy a ThreadProgress. Can be called on a ThreadProgress that
		/// has never been initialized provided that the ThreadProgress
		/// struct has been initially zeroed. Must be called even if
		/// ff_thread_progress_init() failed
		/// </summary>
		/********************************************************************/
		public static void FF_Thread_Progress_Destroy(ThreadProgress pro)//XX 44
		{
			PThread.FF_PThread_Free(pro, thread_Progress_Fields);
		}
	}
}
