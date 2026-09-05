/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Numeric
	{
		/********************************************************************/
		/// <summary>
		/// Returns x % m if m != 0, x otherwise
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TVal Modulo_If_Not_Zero<TMod, TVal>(TMod m, TVal x) where TMod : INumber<TMod> where TVal : INumber<TVal>, IModulusOperators<TVal, TMod, TVal>
		{
			if (m == TMod.Zero)
				return x;

			return x % m;
		}



		/********************************************************************/
		/// <summary>
		/// Rounds x up to multiples of target
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Align_Up<T>(T x, T target) where T : INumber<T>, ISubtractionOperators<T, T, T>
		{
			return ((x + (target - T.One)) / target) * target;
		}



		/********************************************************************/
		/// <summary>
		/// Rounds x down to multiples of target
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Align_Down<T>(T x, T target) where T : INumber<T>
		{
			return (x / target) * target;
		}



		/********************************************************************/
		/// <summary>
		/// Rounds x up to multiples of target or saturation of T
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Saturate_Align_Up<T>(T x, T target) where T : INumber<T>, ISubtractionOperators<T, T, T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
		{
			if (x > (T.MaxValue - (target - T.One)))
				return T.MaxValue;

			return ((x + (target - T.One)) / target) * target;
		}



		/********************************************************************/
		/// <summary>
		/// Returns sign of a number (-1 for negative numbers, 1 for positive
		/// numbers, 0 for 0)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int SigNum<T>(T value) where T : ISignedNumber<T>, ISubtractionOperators<T, T, T>, IComparisonOperators<T, T, bool>
		{
			return c_int.CreateTruncating((value > T.Zero ? 1 : 0) - (value < T.Zero ? 1 : 0));
		}
	}
}
