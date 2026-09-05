/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using Polycode.NostalgicPlayer.Kit.C.Std.Random;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Seed
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static TRng Make_Prng<TRng, TRd_Result>(IEngine_Traits<TRd_Result> rd) where TRng : IEngine_Seed_Traits where TRd_Result : IBinaryInteger<TRd_Result>, IUnsignedNumber<TRd_Result>
		{
			size_t num_Seed_Values = Numeric.Align_Up<size_t>(TRng.Seed_Bits, sizeof(c_uint) * 8) / (sizeof(c_uint) * 8);

			// TNE: The original code uses either the heap or stack. Here we always use the heap,
			// so that's why the two branches are equal
			if (num_Seed_Values > 128)
			{
				Seed_Seq_Values<TRd_Result> values = new Seed_Seq_Values<TRd_Result>(num_Seed_Values, rd);
				Seed_Seq seed = new Seed_Seq(values.Begin(), values.End());

				return (TRng)TRng.Create(seed);
			}
			else
			{
				Seed_Seq_Values<TRd_Result> values = new Seed_Seq_Values<TRd_Result>(num_Seed_Values, rd);
				Seed_Seq seed = new Seed_Seq(values.Begin(), values.End());

				return (TRng)TRng.Create(seed);
			}
		}
	}
}
