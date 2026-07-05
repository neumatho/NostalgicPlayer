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
	/// C# port of the C++ standard library std::subtract_with_carry_engine
	/// (see the C++ standard, [rand.eng.sub]).
	///
	/// A random number engine that produces unsigned integer values using a
	/// subtract-with-carry (lagged Fibonacci) algorithm.
	///
	/// In C++ the word size (w), short lag (s) and long lag (r) are non-type
	/// template parameters. Since C# does not support those, they are passed
	/// as constructor arguments instead. As a consequence min(), max(),
	/// word_size, short_lag and long_lag are instance members here, where they
	/// are static in C++
	/// </summary>
	public class Subtract_With_Carry_Engine<UIntType> : IRandom_Number_Engine<UIntType> where UIntType : IBinaryInteger<UIntType>, IUnsignedNumber<UIntType>
	{
		// The default seed as defined by the C++ standard
		private const uint_least32_t DefaultSeed = 19780503U;

		// Parameters of the linear_congruential_engine used for seeding, as
		// mandated by the standard: linear_congruential_engine<uint_least32_t, 40014, 0, 2147483563>
		private const c_ulong Lcg_A = 40014UL;
		private const c_ulong Lcg_M = 2147483563UL;

		// Engine parameters (compile-time template arguments in C++)
		private c_int wordSize;		// w
		private c_int shortLag;		// s
		private c_int longLag;		// r

		private UIntType modulus;	// 2^w
		private UIntType[] state;	// The r last values (ring buffer)
		private UIntType carry;		// The carry (0 or 1)
		private c_int pos;			// Current position in the ring buffer

		/********************************************************************/
		/// <summary>
		/// Constructs the engine and seeds it with the default seed
		/// </summary>
		/********************************************************************/
		public Subtract_With_Carry_Engine(c_int w, c_int s, c_int r)
		{
			Setup(w, s, r);
			seed(default_seed);
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the engine and seeds it with the given value
		/// </summary>
		/********************************************************************/
		public Subtract_With_Carry_Engine(c_int w, c_int s, c_int r, UIntType value)
		{
			Setup(w, s, r);
			seed(value);
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the engine and seeds it with the given seed sequence
		/// </summary>
		/********************************************************************/
		public Subtract_With_Carry_Engine(c_int w, c_int s, c_int r, Seed_Seq q)
		{
			Setup(w, s, r);
			seed(q);
		}



		/********************************************************************/
		/// <summary>
		/// The default seed used when the engine is default constructed
		/// (C++ default_seed)
		/// </summary>
		/********************************************************************/
		public static UIntType default_seed => UIntType.CreateTruncating(DefaultSeed);



		/********************************************************************/
		/// <summary>
		/// The type of the values produced by the engine (C++ result_type)
		/// </summary>
		/********************************************************************/
		public static Type result_type => typeof(UIntType);



		/********************************************************************/
		/// <summary>
		/// The number of bits in each generated value (C++ word_size)
		/// </summary>
		/********************************************************************/
		public c_int word_size => wordSize;



		/********************************************************************/
		/// <summary>
		/// The short lag (C++ short_lag)
		/// </summary>
		/********************************************************************/
		public c_int short_lag => shortLag;



		/********************************************************************/
		/// <summary>
		/// The long lag, i.e. the size of the internal state (C++ long_lag)
		/// </summary>
		/********************************************************************/
		public c_int long_lag => longLag;



		/********************************************************************/
		/// <summary>
		/// Returns the smallest value that the engine can produce, which is 0
		/// </summary>
		/********************************************************************/
		public UIntType min()
		{
			return UIntType.Zero;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the largest value that the engine can produce, which is
		/// 2^w - 1
		/// </summary>
		/********************************************************************/
		public UIntType max()
		{
			return modulus - UIntType.One;
		}



		/********************************************************************/
		/// <summary>
		/// Reseeds the engine using the default seed
		/// </summary>
		/********************************************************************/
		public void seed()
		{
			seed(default_seed);
		}



		/********************************************************************/
		/// <summary>
		/// Reseeds the engine using the given value. If the value is 0, the
		/// default seed is used instead
		/// </summary>
		/********************************************************************/
		public void seed(UIntType value)
		{
			// Seed a linear_congruential_engine<uint_least32_t, 40014, 0, 2147483563>
			// with (value == 0 ? default_seed : value), narrowed to uint_least32_t
			uint_least32_t lcgSeed = (value == UIntType.Zero) ? DefaultSeed : uint_least32_t.CreateTruncating(value);
			uint_least32_t lcg = Lcg_Seed(lcgSeed);

			c_int n = (wordSize + 31) / 32;
			UIntType shift32 = UIntType.One << 32;

			for (c_int i = 0; i < longLag; i++)
			{
				UIntType sum = UIntType.Zero;
				UIntType factor = UIntType.One;

				for (c_int j = 0; j < n; j++)
				{
					lcg = Lcg_Next(lcg);
					sum += UIntType.CreateTruncating(lcg) * factor;

					if ((j + 1) < n)
						factor *= shift32;
				}

				state[i] = sum % modulus;
			}

			carry = (state[longLag - 1] == UIntType.Zero) ? UIntType.One : UIntType.Zero;
			pos = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Reseeds the engine using the given seed sequence
		/// </summary>
		/********************************************************************/
		public void seed(Seed_Seq q)
		{
			c_int k = (wordSize + 31) / 32;
			uint_least32_t[] arr = new uint_least32_t[longLag * k];
			q.generate(arr);

			UIntType shift32 = UIntType.One << 32;

			for (c_int i = 0; i < longLag; i++)
			{
				UIntType sum = UIntType.Zero;
				UIntType factor = UIntType.One;

				for (c_int j = 0; j < k; j++)
				{
					sum += UIntType.CreateTruncating(arr[(k * i) + j]) * factor;

					if ((j + 1) < k)
						factor *= shift32;
				}

				state[i] = sum % modulus;
			}

			carry = (state[longLag - 1] == UIntType.Zero) ? UIntType.One : UIntType.Zero;
			pos = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Advances the engine's state and returns the generated value
		/// (C++ operator())
		/// </summary>
		/********************************************************************/
		public UIntType Invoke()
		{
			// Derive the short lag index from the current index
			c_int ps = pos - shortLag;
			if (ps < 0)
				ps += longLag;

			// Calculate the new value without overflow or division.
			// state[pos] holds X(i-r) and state[ps] holds X(i-s)
			UIntType xi;

			if (state[ps] >= (state[pos] + carry))
			{
				xi = state[ps] - state[pos] - carry;
				carry = UIntType.Zero;
			}
			else
			{
				xi = (modulus - state[pos] - carry) + state[ps];
				carry = UIntType.One;
			}

			state[pos] = xi;

			// Advance the current index, looping around the ring buffer
			if (++pos >= longLag)
				pos = 0;

			return xi;
		}



		/********************************************************************/
		/// <summary>
		/// Advances the engine's state by z steps
		/// </summary>
		/********************************************************************/
		public void discard(c_ulong_long z)
		{
			for (; z != 0; z--)
				Invoke();
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Compares two engines for equality. Two engines are equal if they
		/// have the same parameters and internal state, meaning they will
		/// produce the same sequence of values
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			if (obj is not Subtract_With_Carry_Engine<UIntType> other)
				return false;

			if ((wordSize != other.wordSize) || (shortLag != other.shortLag) || (longLag != other.longLag) || (pos != other.pos) || (carry != other.carry))
				return false;

			for (c_int i = 0; i < longLag; i++)
			{
				if (state[i] != other.state[i])
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Returns a hash code for the engine
		/// </summary>
		/********************************************************************/
		public override c_int GetHashCode()
		{
			HashCode hash = new HashCode();

			hash.Add(wordSize);
			hash.Add(shortLag);
			hash.Add(longLag);
			hash.Add(pos);
			hash.Add(carry);

			for (c_int i = 0; i < longLag; i++)
				hash.Add(state[i]);

			return hash.ToHashCode();
		}



		/********************************************************************/
		/// <summary>
		/// Compares two engines for equality
		/// </summary>
		/********************************************************************/
		public static bool operator ==(Subtract_With_Carry_Engine<UIntType> a, Subtract_With_Carry_Engine<UIntType> b)
		{
			if (ReferenceEquals(a, b))
				return true;

			if ((a is null) || (b is null))
				return false;

			return a.Equals(b);
		}



		/********************************************************************/
		/// <summary>
		/// Compares two engines for inequality
		/// </summary>
		/********************************************************************/
		public static bool operator !=(Subtract_With_Carry_Engine<UIntType> a, Subtract_With_Carry_Engine<UIntType> b)
		{
			return !(a == b);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Initializes the engine parameters and allocates the state
		/// </summary>
		/********************************************************************/
		private void Setup(c_int w, c_int s, c_int r)
		{
			wordSize = w;
			shortLag = s;
			longLag = r;

			modulus = UIntType.One << w;
			state = new UIntType[r];
			carry = UIntType.Zero;
			pos = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Seeds the internal linear_congruential_engine. With c == 0 the
		/// state becomes 1 if (s mod m) is 0, otherwise (s mod m)
		/// </summary>
		/********************************************************************/
		private static uint_least32_t Lcg_Seed(uint_least32_t s)
		{
			uint_least32_t mod = (uint_least32_t)(s % Lcg_M);

			return (mod == 0) ? 1U : mod;
		}



		/********************************************************************/
		/// <summary>
		/// Advances the internal linear_congruential_engine and returns the
		/// new state: x = (a * x) mod m
		/// </summary>
		/********************************************************************/
		private static uint_least32_t Lcg_Next(uint_least32_t state)
		{
			return (uint_least32_t)((Lcg_A * state) % Lcg_M);
		}
		#endregion
	}
}
