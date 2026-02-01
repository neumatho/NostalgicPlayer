/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// Context stored in the client AVCodecInternal thread_ctx
	/// </summary>
	internal class FrameThreadContext : IContext
	{
		/// <summary>
		/// The contexts for each thread
		/// </summary>
		public CPointer<PerThreadContext> Threads;

		/// <summary>
		/// The last thread submit_packet() was called on
		/// </summary>
		public PerThreadContext Prev_Thread;

		/// <summary>
		/// Number of successfully initialized mutexes/conditions
		/// </summary>
		public c_uint PThread_Init_Cnt = 0;

		/// <summary>
		/// Mutex used to protect get/release_buffer()
		/// </summary>
		public readonly pthread_mutex_t Buffer_Mutex = new pthread_mutex_t();

		/// <summary>
		/// This lock is used for ensuring threads run in serial when thread-unsafe
		/// hwaccel is used
		/// </summary>
		public readonly pthread_mutex_t HwAccel_Mutex = new pthread_mutex_t();

		/// <summary>
		/// 
		/// </summary>
		public readonly pthread_mutex_t Async_Mutex = new pthread_mutex_t();

		/// <summary>
		/// 
		/// </summary>
		public readonly pthread_cond_t Async_Cond = new pthread_cond_t();

		/// <summary>
		/// 
		/// </summary>
		public c_int Async_Lock;

		/// <summary>
		/// 
		/// </summary>
		public readonly DecodedFrames Df = new DecodedFrames();

		/// <summary>
		/// 
		/// </summary>
		public c_int Result;

		/// <summary>
		/// Packet to be submitted to the next thread for decoding
		/// </summary>
		public AvPacket Next_Pkt;

		/// <summary>
		/// 
		/// </summary>
		public c_int Next_Decoding;

		/// <summary>
		/// 
		/// </summary>
		public c_int Next_Finished;

		/// <summary>
		/// hwaccel state for thread-unsafe hwaccels is temporarily stored here in
		/// order to transfer its ownership to the next decoding thread without the
		/// need for extra synchronization
		/// </summary>
		public AvHwAccel Stash_HwAccel;

		/// <summary>
		/// 
		/// </summary>
		public IContext Stash_HwAccel_Context;

		/// <summary>
		/// 
		/// </summary>
		public IPrivateData Stash_HwAccel_Priv;
	}
}
