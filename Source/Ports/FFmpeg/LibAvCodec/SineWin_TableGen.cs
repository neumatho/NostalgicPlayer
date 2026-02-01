/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// 
	/// </summary>
	public static class SineWin_TableGen
	{
		private static readonly pthread_once_t[] init_Window_Once =
		[
			CThread.pthread_once_init(), CThread.pthread_once_init(), CThread.pthread_once_init(), CThread.pthread_once_init(), CThread.pthread_once_init(),
			CThread.pthread_once_init(), CThread.pthread_once_init(), CThread.pthread_once_init(), CThread.pthread_once_init()
		];

		private static readonly CThread.ThreadOnce_Init_Delegate[] sine_Window_Init_Func_Array =
		[
			() => Init_FF_Sine_Window(5),
			() => Init_FF_Sine_Window(6),
			() => Init_FF_Sine_Window(7),
			() => Init_FF_Sine_Window(8),
			() => Init_FF_Sine_Window(9),
			() => Init_FF_Sine_Window(10),
			() => Init_FF_Sine_Window(11),
			() => Init_FF_Sine_Window(12),
			() => Init_FF_Sine_Window(13)
		];

		private static readonly c_float[] ff_Sine_32 = new c_float[32];
		private static readonly c_float[] ff_Sine_64 = new c_float[64];
		private static readonly c_float[] ff_Sine_128 = new c_float[128];
		private static readonly c_float[] ff_Sine_256 = new c_float[256];
		private static readonly c_float[] ff_Sine_512 = new c_float[512];
		private static readonly c_float[] ff_Sine_1024 = new c_float[1024];
		private static readonly c_float[] ff_Sine_2048 = new c_float[2048];
		private static readonly c_float[] ff_Sine_4096 = new c_float[4096];
		private static readonly c_float[] ff_Sine_8192 = new c_float[8192];

		/// <summary>
		/// 
		/// </summary>
		public static readonly c_float[][] ff_Sine_Windows =
		[
			null, null, null, null, null,	// Unused
			ff_Sine_32, ff_Sine_64, ff_Sine_128,
			ff_Sine_256, ff_Sine_512, ff_Sine_1024,
			ff_Sine_2048, ff_Sine_4096, ff_Sine_8192
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FF_Init_FF_Sine_Windows(c_int index)
		{
			CThread.pthread_once(init_Window_Once[index - 5], sine_Window_Init_Func_Array[index - 5]);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Init_FF_Sine_Window(c_int index)
		{
			FF_Sine_Window_Init(ff_Sine_Windows[index], 1 << index);
		}



		/********************************************************************/
		/// <summary>
		/// Generate a sine window
		/// </summary>
		/********************************************************************/
		private static void FF_Sine_Window_Init(c_float[] window, c_int n)
		{
			for (c_int i = 0; i < n; i++)
				window[i] = (c_float)CMath.sin((i + 0.5) * (Math.PI / (2.0 * n)));
		}
		#endregion
	}
}
