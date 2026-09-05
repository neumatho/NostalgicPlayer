/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random
{
	/// <summary>
	/// 
	/// </summary>
	internal class Seed_Seq_Values<TRd_Result> where TRd_Result : IBinaryInteger<TRd_Result>, IUnsignedNumber<TRd_Result>
	{
		private readonly c_uint[] seeds;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Seed_Seq_Values(size_t N, IEngine_Traits<TRd_Result> rd)
		{
			seeds = new c_uint[N];

			for (size_t i = 0; i < N; ++i)
				seeds[i] = Random.Random_<c_uint, TRd_Result>(rd);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public CPointer<c_uint> Begin()
		{
			return new CPointer<c_uint>(seeds);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public CPointer<c_uint> End()
		{
			return new CPointer<c_uint>(seeds) + seeds.Length;
		}
	}
}
