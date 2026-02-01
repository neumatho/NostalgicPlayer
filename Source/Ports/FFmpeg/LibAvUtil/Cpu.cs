/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Compat;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class Cpu
	{
		private static c_int cpu_Flags = -1;
		private static c_int cpu_Count = -1;

		private static c_int printed = 0;

		/********************************************************************/
		/// <summary>
		/// Return the flags which specify extensions supported by the CPU.
		/// The returned value is affected by av_force_cpu_flags() if that
		/// was used before. So av_get_cpu_flags() can easily be used in an
		/// application to detect the enabled cpu flags
		/// </summary>
		/********************************************************************/
		public static AvCpuFlag Av_Get_Cpu_Flags()//XX 109
		{
			c_int flags = StdAtomic.Atomic_Load(ref cpu_Flags);

			if (flags == -1)
			{
				flags = (c_int)Get_Cpu_Flags();
				StdAtomic.Atomic_Store(ref cpu_Flags, flags);
			}

			return (AvCpuFlag)flags;
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of logical CPU cores present
		/// </summary>
		/********************************************************************/
		public static c_int Av_Cpu_Count()//XX 221
		{
			c_int nb_Cpus = Environment.ProcessorCount;

			if (StdAtomic.Atomic_Exchange(ref printed, 1) != 0)
				Log.Av_Log(null, Log.Av_Log_Debug, "detected %d logical cores\n", nb_Cpus);

			c_int count = StdAtomic.Atomic_Load(ref cpu_Count);

			if (count > 0)
			{
				nb_Cpus = count;

				Log.Av_Log(null, Log.Av_Log_Debug, "overriding to %d logical cores\n", nb_Cpus);
			}

			return nb_Cpus;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static AvCpuFlag Get_Cpu_Flags()
		{
			return AvCpuFlag.FF_All;
		}
		#endregion
	}
}
