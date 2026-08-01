/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Numerics;

namespace Polycode.NostalgicPlayer.Kit.C.Std.Random
{
	/// <summary>
	/// C# port of the C++ standard library std::uniform_int_distribution
	/// (see the C++ standard, [rand.dist.uni.int]).
	///
	/// Produces integer values evenly distributed over the closed range
	/// [a, b], drawing its entropy from a
	/// <see cref="IUniform_Random_Bit_Generator{TResult}"/>.
	///
	/// In C++ the second constructor argument defaults to
	/// numeric_limits‹IntType›::max(). C# does not allow that as a default
	/// argument value, so the constructors are spelled out instead.
	///
	/// C++ does the internal arithmetic in
	/// common_type_t‹generator result, unsigned IntType›. Here it is always
	/// done in uint64_t, which is a safe superset for every supported type,
	/// as none of them are wider than 64 bit. Values are converted by their
	/// two's complement bit pattern, which is what makes the wrapping
	/// (b - a) below produce the correct distance for signed types
	/// </summary>
	public class Uniform_Int_Distribution<IntType> where IntType : IBinaryInteger<IntType>, IMinMaxValue<IntType>
	{
		/// <summary>
		/// C# port of the nested C++ type
		/// std::uniform_int_distribution::param_type.
		///
		/// Holds the two values that define the distribution, so they can be
		/// stored and passed around on their own
		/// </summary>
		public class Param_Type
		{
			private readonly IntType lower;
			private readonly IntType upper;

			/****************************************************************/
			/// <summary>
			/// Constructs the parameters with a = 0 and b = the largest
			/// value of IntType
			/// </summary>
			/****************************************************************/
			public Param_Type() : this(IntType.Zero, IntType.MaxValue)
			{
			}



			/****************************************************************/
			/// <summary>
			/// Constructs the parameters with the given a and b = the
			/// largest value of IntType
			/// </summary>
			/****************************************************************/
			public Param_Type(IntType a) : this(a, IntType.MaxValue)
			{
			}



			/****************************************************************/
			/// <summary>
			/// Constructs the parameters with the given a and b
			/// </summary>
			/****************************************************************/
			public Param_Type(IntType a, IntType b)
			{
				lower = a;
				upper = b;
			}



			/****************************************************************/
			/// <summary>
			/// The distribution these parameters belong to
			/// (C++ distribution_type)
			/// </summary>
			/****************************************************************/
			public static Type distribution_type => typeof(Uniform_Int_Distribution<IntType>);



			/****************************************************************/
			/// <summary>
			/// Returns the smallest value of the range (C++ a())
			/// </summary>
			/****************************************************************/
			public IntType a()
			{
				return lower;
			}



			/****************************************************************/
			/// <summary>
			/// Returns the largest value of the range (C++ b())
			/// </summary>
			/****************************************************************/
			public IntType b()
			{
				return upper;
			}

			#region Overrides
			/****************************************************************/
			/// <summary>
			/// Compares two parameter sets for equality
			/// </summary>
			/****************************************************************/
			public override bool Equals(object obj)
			{
				if (obj is not Param_Type other)
					return false;

				return (lower == other.lower) && (upper == other.upper);
			}



			/****************************************************************/
			/// <summary>
			/// Returns a hash code for the parameters
			/// </summary>
			/****************************************************************/
			public override c_int GetHashCode()
			{
				return HashCode.Combine(lower, upper);
			}



			/****************************************************************/
			/// <summary>
			/// Compares two parameter sets for equality
			/// </summary>
			/****************************************************************/
			public static bool operator ==(Param_Type a, Param_Type b)
			{
				if (ReferenceEquals(a, b))
					return true;

				if ((a is null) || (b is null))
					return false;

				return a.Equals(b);
			}



			/****************************************************************/
			/// <summary>
			/// Compares two parameter sets for inequality
			/// </summary>
			/****************************************************************/
			public static bool operator !=(Param_Type a, Param_Type b)
			{
				return !(a == b);
			}
			#endregion
		}

		private Param_Type parameters;

		/********************************************************************/
		/// <summary>
		/// Constructs the distribution with a = 0 and b = the largest value
		/// of IntType
		/// </summary>
		/********************************************************************/
		public Uniform_Int_Distribution() : this(IntType.Zero, IntType.MaxValue)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the distribution with the given a and b = the largest
		/// value of IntType
		/// </summary>
		/********************************************************************/
		public Uniform_Int_Distribution(IntType a) : this(a, IntType.MaxValue)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the distribution with the given a and b
		/// </summary>
		/********************************************************************/
		public Uniform_Int_Distribution(IntType a, IntType b)
		{
			parameters = new Param_Type(a, b);
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the distribution from the given parameters
		/// </summary>
		/********************************************************************/
		public Uniform_Int_Distribution(Param_Type parm)
		{
			parameters = parm;
		}



		/********************************************************************/
		/// <summary>
		/// The type of the values produced by the distribution
		/// (C++ result_type)
		/// </summary>
		/********************************************************************/
		public static Type result_type => typeof(IntType);



		/********************************************************************/
		/// <summary>
		/// Resets the distribution, so the next value does not depend on any
		/// previous one. This distribution keeps no such state, so as in C++
		/// it does nothing (C++ reset())
		/// </summary>
		/********************************************************************/
		public void reset()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Returns the smallest value of the range (C++ a())
		/// </summary>
		/********************************************************************/
		public IntType a()
		{
			return parameters.a();
		}



		/********************************************************************/
		/// <summary>
		/// Returns the largest value of the range (C++ b())
		/// </summary>
		/********************************************************************/
		public IntType b()
		{
			return parameters.b();
		}



		/********************************************************************/
		/// <summary>
		/// Returns the parameters of the distribution (C++ param())
		/// </summary>
		/********************************************************************/
		public Param_Type param()
		{
			return parameters;
		}



		/********************************************************************/
		/// <summary>
		/// Sets the parameters of the distribution (C++ param(param_type))
		/// </summary>
		/********************************************************************/
		public void param(Param_Type parm)
		{
			parameters = parm;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the smallest value the distribution can produce, which is
		/// a() (C++ min())
		/// </summary>
		/********************************************************************/
		public IntType min()
		{
			return parameters.a();
		}



		/********************************************************************/
		/// <summary>
		/// Returns the largest value the distribution can produce, which is
		/// b() (C++ max())
		/// </summary>
		/********************************************************************/
		public IntType max()
		{
			return parameters.b();
		}



		/********************************************************************/
		/// <summary>
		/// Returns a value evenly distributed over the range the
		/// distribution was constructed with (C++ operator())
		/// </summary>
		/********************************************************************/
		public IntType Invoke<TValue>(IUniform_Random_Bit_Generator<TValue> g) where TValue : IBinaryInteger<TValue>, IUnsignedNumber<TValue>
		{
			return Invoke(g, parameters);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a value evenly distributed over the given range, leaving
		/// the parameters of the distribution untouched
		/// (C++ operator()(g, param_type))
		/// </summary>
		/********************************************************************/
		public IntType Invoke<TValue>(IUniform_Random_Bit_Generator<TValue> g, Param_Type parm) where TValue : IBinaryInteger<TValue>, IUnsignedNumber<TValue>
		{
			uint64_t urngMin = uint64_t.CreateTruncating(g.min());
			uint64_t urngRange = uint64_t.CreateTruncating(g.max()) - urngMin;
			uint64_t range = To_Unsigned(parm.b()) - To_Unsigned(parm.a());

			uint64_t result;

			if (urngRange > range)
			{
				// The generator has a wider range than needed. Split it into
				// equally sized buckets and throw away the values falling in
				// the incomplete bucket at the end, as they would make the
				// lowest values come up slightly too often
				uint64_t rangeSize = range + 1;
				uint64_t scaling = urngRange / rangeSize;
				uint64_t past = rangeSize * scaling;

				do
				{
					result = uint64_t.CreateTruncating(g.Invoke()) - urngMin;
				}
				while (result >= past);

				result /= scaling;
			}
			else if (urngRange < range)
			{
				// The generator has a narrower range than needed, so two
				// values are combined into one. The result is retried when
				// the combination lands outside the wanted range
				uint64_t low;

				do
				{
					uint64_t urngRangeSize = urngRange + 1;

					low = urngRangeSize * To_Unsigned(Invoke(g, new Param_Type(IntType.Zero, IntType.CreateTruncating(range / urngRangeSize))));
					result = low + (uint64_t.CreateTruncating(g.Invoke()) - urngMin);
				}
				while ((result > range) || (result < low));
			}
			else
				result = uint64_t.CreateTruncating(g.Invoke()) - urngMin;

			return IntType.CreateTruncating(To_Unsigned(parm.a()) + result);
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Compares two distributions for equality. Two distributions are
		/// equal if they have the same parameters, meaning they will produce
		/// the same values from the same generator state
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			if (obj is not Uniform_Int_Distribution<IntType> other)
				return false;

			return parameters == other.parameters;
		}



		/********************************************************************/
		/// <summary>
		/// Returns a hash code for the distribution
		/// </summary>
		/********************************************************************/
		public override c_int GetHashCode()
		{
			return parameters.GetHashCode();
		}



		/********************************************************************/
		/// <summary>
		/// Compares two distributions for equality
		/// </summary>
		/********************************************************************/
		public static bool operator ==(Uniform_Int_Distribution<IntType> a, Uniform_Int_Distribution<IntType> b)
		{
			if (ReferenceEquals(a, b))
				return true;

			if ((a is null) || (b is null))
				return false;

			return a.Equals(b);
		}



		/********************************************************************/
		/// <summary>
		/// Compares two distributions for inequality
		/// </summary>
		/********************************************************************/
		public static bool operator !=(Uniform_Int_Distribution<IntType> a, Uniform_Int_Distribution<IntType> b)
		{
			return !(a == b);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Returns the two's complement bit pattern of the value, widened to
		/// 64 bit. Negative values are taken through int64_t first, so the
		/// sign is extended instead of the value being clamped away. That is
		/// what lets the wrapping (b - a) above give the correct distance
		/// </summary>
		/********************************************************************/
		private static uint64_t To_Unsigned(IntType value)
		{
			if (IntType.IsNegative(value))
				return (uint64_t)int64_t.CreateTruncating(value);

			return uint64_t.CreateTruncating(value);
		}
		#endregion
	}
}
