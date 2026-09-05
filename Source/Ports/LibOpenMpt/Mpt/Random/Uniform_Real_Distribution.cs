/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random
{
	/// <summary>
	/// 
	/// </summary>
	internal class Uniform_Real_Distribution<T> where T : IFloatingPoint<T>
	{
		private readonly T a;
		private readonly T b;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Uniform_Real_Distribution(T a_, T b_)
		{
			a = a_;
			b = b_;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public T Invoke<TValue>(IEngine_Traits<TValue> rng) where TValue : IBinaryInteger<TValue>, IUnsignedNumber<TValue>
		{
			// TNE: The original reads this from numeric_limits‹T›::digits,
			// which is the number of mantissa bits including the implicit one
			c_int mantissa_Bits = T.Zero.GetSignificandBitLength();

			return ((b - a) * T.CreateTruncating(Random.Random_<uint64, TValue>(rng, (size_t)mantissa_Bits)) / T.CreateTruncating((uint64)1 << mantissa_Bits)) + a;
		}
	}
}
