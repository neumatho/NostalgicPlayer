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
	/// C# port of the C++ standard library std::mersenne_twister_engine
	/// (see the C++ standard, [rand.eng.mers]).
	///
	/// A random number engine that produces unsigned integer values using a
	/// Mersenne twister algorithm.
	///
	/// In C++ the word size (w), state size (n), shift size (m), mask bits (r),
	/// the tempering/twist parameters and the initialization multiplier (f) are
	/// non-type template parameters. Since C# does not support those, they are
	/// passed as constructor arguments instead. As a consequence min(), max(),
	/// word_size, state_size and so on are instance members here, where they are
	/// static in C++
	/// </summary>
	public class Mersenne_Twister_Engine<UIntType> : IRandom_Number_Engine<UIntType> where UIntType : IBinaryInteger<UIntType>, IUnsignedNumber<UIntType>
	{
		// The default seed as defined by the C++ standard
		private const uint_least32_t DefaultSeed = 5489U;

		// Engine parameters (compile-time template arguments in C++)
		private c_int wordSize;			// w
		private c_int stateSize;		// n
		private c_int shiftSize;		// m
		private c_int maskBits;			// r
		private UIntType xorMask;		// a
		private c_int temperingU;		// u
		private UIntType temperingD;	// d
		private c_int temperingS;		// s
		private UIntType temperingB;	// b
		private c_int temperingT;		// t
		private UIntType temperingC;	// c
		private c_int temperingL;		// l
		private UIntType initMult;		// f

		private UIntType wordMask;		// 2^w - 1, used to keep values within w bits
		private UIntType upperMask;		// The most significant w-r bits
		private UIntType lowerMask;		// The least significant r bits

		private UIntType[] state;		// The n last values (X)
		private c_int pos;				// Current position in the state

		/********************************************************************/
		/// <summary>
		/// Constructs the engine and seeds it with the default seed
		/// </summary>
		/********************************************************************/
		public Mersenne_Twister_Engine(c_int w, c_int n, c_int m, c_int r, UIntType a, c_int u, UIntType d, c_int s, UIntType b, c_int t, UIntType c, c_int l, UIntType f)
		{
			Setup(w, n, m, r, a, u, d, s, b, t, c, l, f);
			seed(default_seed);
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the engine and seeds it with the given value
		/// </summary>
		/********************************************************************/
		public Mersenne_Twister_Engine(c_int w, c_int n, c_int m, c_int r, UIntType a, c_int u, UIntType d, c_int s, UIntType b, c_int t, UIntType c, c_int l, UIntType f, UIntType value)
		{
			Setup(w, n, m, r, a, u, d, s, b, t, c, l, f);
			seed(value);
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the engine and seeds it with the given seed sequence
		/// </summary>
		/********************************************************************/
		public Mersenne_Twister_Engine(c_int w, c_int n, c_int m, c_int r, UIntType a, c_int u, UIntType d, c_int s, UIntType b, c_int t, UIntType c, c_int l, UIntType f, Seed_Seq q)
		{
			Setup(w, n, m, r, a, u, d, s, b, t, c, l, f);
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
		/// The size of the internal state (C++ state_size)
		/// </summary>
		/********************************************************************/
		public c_int state_size => stateSize;



		/********************************************************************/
		/// <summary>
		/// The shift size (C++ shift_size)
		/// </summary>
		/********************************************************************/
		public c_int shift_size => shiftSize;



		/********************************************************************/
		/// <summary>
		/// The number of mask bits (C++ mask_bits)
		/// </summary>
		/********************************************************************/
		public c_int mask_bits => maskBits;



		/********************************************************************/
		/// <summary>
		/// The initialization multiplier (C++ initialization_multiplier)
		/// </summary>
		/********************************************************************/
		public UIntType initialization_multiplier => initMult;



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
			return wordMask;
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
		/// Reseeds the engine using the given value
		/// </summary>
		/********************************************************************/
		public void seed(UIntType value)
		{
			state[0] = value & wordMask;

			for (c_int i = 1; i < stateSize; i++)
			{
				UIntType x = state[i - 1];
				x ^= x >> (wordSize - 2);
				x *= initMult;
				x += UIntType.CreateTruncating(i);

				state[i] = x & wordMask;
			}

			pos = stateSize;
		}



		/********************************************************************/
		/// <summary>
		/// Reseeds the engine using the given seed sequence
		/// </summary>
		/********************************************************************/
		public void seed(Seed_Seq q)
		{
			c_int k = (wordSize + 31) / 32;
			uint_least32_t[] arr = new uint_least32_t[stateSize * k];
			q.generate(arr);

			UIntType shift32 = UIntType.One << 32;

			bool zero = true;

			for (c_int i = 0; i < stateSize; i++)
			{
				UIntType sum = UIntType.Zero;
				UIntType factor = UIntType.One;

				for (c_int j = 0; j < k; j++)
				{
					sum += UIntType.CreateTruncating(arr[(k * i) + j]) * factor;

					if ((j + 1) < k)
						factor *= shift32;
				}

				state[i] = sum & wordMask;

				if (zero)
				{
					if (i == 0)
					{
						if ((state[0] & upperMask) != UIntType.Zero)
							zero = false;
					}
					else if (state[i] != UIntType.Zero)
						zero = false;
				}
			}

			// Avoid an all-zero state, which would be a fixed point
			if (zero)
				state[0] = UIntType.One << (wordSize - 1);

			pos = stateSize;
		}



		/********************************************************************/
		/// <summary>
		/// Advances the engine's state and returns the generated value
		/// (C++ operator())
		/// </summary>
		/********************************************************************/
		public UIntType Invoke()
		{
			// Reload the state vector when it is exhausted. This has a cost of
			// O(n) that is amortized over the n following calls
			if (pos >= stateSize)
				Generate_Numbers();

			// Calculate the tempered value of state[pos]
			UIntType z = state[pos++];
			z ^= (z >> temperingU) & temperingD;
			z ^= (z << temperingS) & temperingB;
			z ^= (z << temperingT) & temperingC;
			z ^= z >> temperingL;

			return z;
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
			if (obj is not Mersenne_Twister_Engine<UIntType> other)
				return false;

			if ((wordSize != other.wordSize) || (stateSize != other.stateSize) || (shiftSize != other.shiftSize) || (maskBits != other.maskBits) || (pos != other.pos))
				return false;

			if ((xorMask != other.xorMask) || (temperingU != other.temperingU) || (temperingD != other.temperingD) || (temperingS != other.temperingS) || (temperingB != other.temperingB) || (temperingT != other.temperingT) || (temperingC != other.temperingC) || (temperingL != other.temperingL) || (initMult != other.initMult))
				return false;

			for (c_int i = 0; i < stateSize; i++)
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
			hash.Add(stateSize);
			hash.Add(shiftSize);
			hash.Add(maskBits);
			hash.Add(pos);

			for (c_int i = 0; i < stateSize; i++)
				hash.Add(state[i]);

			return hash.ToHashCode();
		}



		/********************************************************************/
		/// <summary>
		/// Compares two engines for equality
		/// </summary>
		/********************************************************************/
		public static bool operator ==(Mersenne_Twister_Engine<UIntType> a, Mersenne_Twister_Engine<UIntType> b)
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
		public static bool operator !=(Mersenne_Twister_Engine<UIntType> a, Mersenne_Twister_Engine<UIntType> b)
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
		private void Setup(c_int w, c_int n, c_int m, c_int r, UIntType a, c_int u, UIntType d, c_int s, UIntType b, c_int t, UIntType c, c_int l, UIntType f)
		{
			wordSize = w;
			stateSize = n;
			shiftSize = m;
			maskBits = r;
			xorMask = a;
			temperingU = u;
			temperingD = d;
			temperingS = s;
			temperingB = b;
			temperingT = t;
			temperingC = c;
			temperingL = l;
			initMult = f;

			// The number of bits in the underlying type
			c_int typeBits = c_int.CreateTruncating(UIntType.PopCount(~UIntType.Zero));

			// 2^w - 1. When w equals the width of the type the shift would
			// overflow, so all bits are set instead
			wordMask = (w >= typeBits) ? ~UIntType.Zero : (UIntType.One << w) - UIntType.One;

			// The most significant w-r bits and the least significant r bits
			upperMask = (~UIntType.Zero << r) & wordMask;
			lowerMask = ~upperMask & wordMask;

			state = new UIntType[n];
			pos = n;
		}



		/********************************************************************/
		/// <summary>
		/// Regenerates the whole state vector (C++ internal twist step)
		/// </summary>
		/********************************************************************/
		private void Generate_Numbers()
		{
			for (c_int k = 0; k < (stateSize - shiftSize); k++)
			{
				UIntType y = (state[k] & upperMask) | (state[k + 1] & lowerMask);
				state[k] = state[k + shiftSize] ^ (y >> 1) ^ (((y & UIntType.One) != UIntType.Zero) ? xorMask : UIntType.Zero);
			}

			for (c_int k = stateSize - shiftSize; k < (stateSize - 1); k++)
			{
				UIntType y = (state[k] & upperMask) | (state[k + 1] & lowerMask);
				state[k] = state[k + (shiftSize - stateSize)] ^ (y >> 1) ^ (((y & UIntType.One) != UIntType.Zero) ? xorMask : UIntType.Zero);
			}

			UIntType yl = (state[stateSize - 1] & upperMask) | (state[0] & lowerMask);
			state[stateSize - 1] = state[shiftSize - 1] ^ (yl >> 1) ^ (((yl & UIntType.One) != UIntType.Zero) ? xorMask : UIntType.Zero);

			pos = 0;
		}
		#endregion
	}
}
