/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C.Std;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Algorithm
	{
		/********************************************************************/
		/// <summary>
		/// Grows x with an exponential factor suitable for increasing buffer
		/// sizes.
		/// Clamps the result at limit.
		/// And avoids integer overflows while doing its business.
		/// The growth factor is 1.5, rounding down, except for the initial
		/// x==1 case
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Exponential_Grow<T>(T x, T limit) where T : INumber<T>, IMinMaxValue<T>, IShiftOperators<T, c_int, T>
		{
			if (x <= T.One)
				return T.CreateTruncating(2);

			T add = T.Min(x >> 1, T.MaxValue - x);

			return T.Min(x + add, T.CreateSaturating(limit));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Exponential_Grow<T>(T x) where T : INumber<T>, IMinMaxValue<T>, IShiftOperators<T, c_int, T>
		{
			return Exponential_Grow(x, T.MaxValue);
		}



		/********************************************************************/
		/// <summary>
		/// Check if val is in [lo,hi]
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Is_In_Range<T, C>(T val, C lo, C hi) where T : INumber<T>, IComparisonOperators<T, C, bool> where C : INumber<C>, IComparisonOperators<C, T, bool>
		{
			return (lo <= val) && (val <= hi);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Contains<TVal>(vector<TVal> container, TVal value)
		{
			return Kit.C.Std.Algorithm.find(container.begin(), container.end(), value) != container.end();
		}
	}
}
