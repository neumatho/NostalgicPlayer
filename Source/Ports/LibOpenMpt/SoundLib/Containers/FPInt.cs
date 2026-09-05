/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Aaaand another fixed-point type, e.g. used for fractional tempos
	/// Note that this doesn't use classical bit shifting for the fixed point part.
	/// This is mostly for the clarity of stored values and to be able to represent any value .0000 to .9999 properly
	/// </summary>
	internal struct FPInt<T> : IEquatable<FPInt<T>> where T : IBinaryInteger<T>, IMinMaxValue<T>
	{
		private T v;
		private readonly size_t _fractFact;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private FPInt(size_t fractFact, T rawValue)
		{
			_fractFact = fractFact;
			v = rawValue;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FPInt(size_t fractFact)
		{
			_fractFact = fractFact;
			v = T.Zero;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FPInt(size_t fractFact, T intPart, T fractPart)
		{
			_fractFact = fractFact;

			T ff = T.CreateTruncating(fractFact);
			v = (intPart * ff) + (fractPart % ff);
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FPInt(size_t fractFact, c_double f)
		{
			_fractFact = fractFact;

			v = SaturateRound.Saturate_Round<T, c_double>(f * fractFact);
		}



		/********************************************************************/
		/// <summary>
		/// Set the integer and fractional part
		///
		/// TNE: Return type changed to void, since it is not possible to
		/// return a reference to a struct in C#
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(T intPart, T fractPart = default)
		{
			T ff = T.CreateTruncating(_fractFact);
			v = (intPart * ff) + (fractPart % ff);
		}



		/********************************************************************/
		/// <summary>
		/// Set the raw internal representation directly
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetRaw(T value)
		{
			v = value;
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve the integer part of the stored value
		/// </summary>
		/********************************************************************/
		public T GetInt()
		{
			return v / T.CreateTruncating(_fractFact);
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve the raw internal representation of the stored value
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly T GetRaw()
		{
			return v;
		}



		/********************************************************************/
		/// <summary>
		/// Formats the stored value as a floating-point value
		/// </summary>
		/********************************************************************/
		public readonly c_double ToDouble()
		{
			return c_double.CreateTruncating(v) / _fractFact;
		}



		/********************************************************************/
		/// <summary>
		/// Add two values together
		/// </summary>
		/********************************************************************/
		public static FPInt<T> operator +(FPInt<T> a, FPInt<T> b)
		{
			return new FPInt<T>(a._fractFact, a.v + b.v);
		}



		/********************************************************************/
		/// <summary>
		/// Subtract one value from another
		/// </summary>
		/********************************************************************/
		public static FPInt<T> operator -(FPInt<T> a, FPInt<T> b)
		{
			return new FPInt<T>(a._fractFact, a.v - b.v);
		}



		/********************************************************************/
		/// <summary>
		/// Compare two values for equality
		/// </summary>
		/********************************************************************/
		public static bool operator ==(FPInt<T> a, FPInt<T> b)
		{
			return a.v == b.v;
		}



		/********************************************************************/
		/// <summary>
		/// Compare two values for inequality
		/// </summary>
		/********************************************************************/
		public static bool operator !=(FPInt<T> a, FPInt<T> b)
		{
			return !(a == b);
		}



		/********************************************************************/
		/// <summary>
		/// Check if one value is less than another
		/// </summary>
		/********************************************************************/
		public static bool operator <(FPInt<T> a, FPInt<T> b)
		{
			return a.v < b.v;
		}



		/********************************************************************/
		/// <summary>
		/// Check if one value is greater than another
		/// </summary>
		/********************************************************************/
		public static bool operator >(FPInt<T> a, FPInt<T> b)
		{
			return a.v > b.v;
		}



		/********************************************************************/
		/// <summary>
		/// Check if one value is less than or equal to another
		/// </summary>
		/********************************************************************/
		public static bool operator <=(FPInt<T> a, FPInt<T> b)
		{
			return a.v <= b.v;
		}



		/********************************************************************/
		/// <summary>
		/// Check if one value is greater than or equal to another
		/// </summary>
		/********************************************************************/
		public static bool operator >=(FPInt<T> a, FPInt<T> b)
		{
			return a.v >= b.v;
		}



		/********************************************************************/
		/// <summary>
		/// Compare this value with another one
		/// </summary>
		/********************************************************************/
		public bool Equals(FPInt<T> other)
		{
			return this == other;
		}



		/********************************************************************/
		/// <summary>
		/// Compare this value with another object
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			return (obj is FPInt<T> other) && (this == other);
		}



		/********************************************************************/
		/// <summary>
		/// Return a hash code for this value
		/// </summary>
		/********************************************************************/
		public override int GetHashCode()
		{
			return HashCode.Combine(v, _fractFact);
		}
	}
}
