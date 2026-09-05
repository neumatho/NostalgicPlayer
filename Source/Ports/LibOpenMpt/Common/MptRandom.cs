/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common
{
	/// <summary>
	/// 
	/// </summary>
	internal static class MptRandom
	{
		private static readonly Random_Device g_Rd = new Random_Device();
		private static readonly Thread_Safe_Prng<Default_Prng, uint64> g_Global_Prng = new Thread_Safe_Prng<Default_Prng, uint64>(Seed.Make_Prng<Default_Prng, c_uint>(Global_Random_Device()));

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Random_Device Global_Random_Device()
		{
			return g_Rd;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Thread_Safe_Prng<Default_Prng, uint64> Global_Prng()
		{
			return g_Global_Prng;
		}
	}
}
