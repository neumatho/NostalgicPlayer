/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// Context used by codec threads and stored in their AVCodecInternal thread_ctx
	/// </summary>
	internal class PerThreadContext : IContext
	{
		/// <summary>
		/// 
		/// </summary>
		public FrameThreadContext Parent;

		/// <summary>
		/// 
		/// </summary>
		public pthread_t Thread;

		/// <summary>
		/// 
		/// </summary>
		public ThreadFlag Thread_Init;

		/// <summary>
		/// Number of successfully initialized mutexes/conditions
		/// </summary>
		public c_uint PThread_Init_Cnt = 0;

		/// <summary>
		/// Used to wait for a new packet from the main thread
		/// </summary>
		public readonly pthread_cond_t Input_Cond = new pthread_cond_t();

		/// <summary>
		/// Used by child threads to wait for progress to change
		/// </summary>
		public readonly pthread_cond_t Progress_Cond = new pthread_cond_t();

		/// <summary>
		/// Used by the main thread to wait for frames to finish
		/// </summary>
		public readonly pthread_cond_t Output_Cond = new pthread_cond_t();

		/// <summary>
		/// Mutex used to protect the contents of the PerThreadContext
		/// </summary>
		public readonly pthread_mutex_t Mutex = new pthread_mutex_t();

		/// <summary>
		/// Mutex used to protect frame progress values and progress_cond
		/// </summary>
		public readonly pthread_mutex_t Progress_Mutex = new pthread_mutex_t();

		/// <summary>
		/// Context used to decode packets passed to this thread
		/// </summary>
		public AvCodecContext AvCtx;

		/// <summary>
		/// Input packet (for decoding) or output (for encoding)
		/// </summary>
		public AvPacket AvPkt;

		/// <summary>
		/// Decoded frames from a single decode iteration
		/// </summary>
		public readonly DecodedFrames Df = new DecodedFrames();

		/// <summary>
		/// The result of the last codec decode/encode() call
		/// </summary>
		public c_int Result;

		/// <summary>
		/// 
		/// </summary>
		public PThreadState State;

		/// <summary>
		/// Set when the thread should exit
		/// </summary>
		public c_int Die;

		/// <summary>
		/// 
		/// </summary>
		public c_int HwAccel_Serializing;

		/// <summary>
		/// 
		/// </summary>
		public c_int Async_Serializing;

		/// <summary>
		/// Set to 1 in ff_thread_finish_setup() when a threadsafe hwaccel is used;
		/// cannot check hwaccel caps directly, because
		/// worked threads clear hwaccel state for thread-unsafe hwaccels
		/// after each decode call
		/// </summary>
		public c_int HwAccel_Threadsafe;
	}
}
