/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Rng
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Init_Random(Rng_State rng)
		{
			rng.State = (c_uint)new TimeSpan(DateTime.Now.Ticks).TotalSeconds;
			LibXmp_Get_Random(rng, 0);
			LibXmp_Get_Random(rng, 0);
			LibXmp_Get_Random(rng, 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Set_Random(Rng_State rng, c_uint state)
		{
			rng.State = state;
		}



		/********************************************************************/
		/// <summary>
		/// Returns a pseudo-random unsigned integer between 0 and
		/// (range - 1) and steps the player's internal random state. If
		/// range = 0, returns 0
		/// </summary>
		/********************************************************************/
		public static c_uint LibXmp_Get_Random(Rng_State rng, uint range)
		{
			c_uint state = LibXmp_Random_Step_XorShift32(rng.State);
			rng.State = state;

			return (c_uint)((uint64)range * state >> 32);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_uint LibXmp_Random_Step_XorShift32(c_uint state)
		{
			if (state == 0)
				state = 1;

			state ^= state << 13;
			state ^= state >> 17;

			return state << 5;
		}
	}
}
