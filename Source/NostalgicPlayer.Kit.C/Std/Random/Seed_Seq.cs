/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;

namespace Polycode.NostalgicPlayer.Kit.C.Std.Random
{
	/// <summary>
	/// C# port of the C++ standard library std::seed_seq
	/// (see the C++ standard, [rand.util.seedseq]).
	///
	/// A seed sequence consumes a sequence of integer-valued data and produces
	/// a requested number of unsigned integer values, based on the consumed
	/// data. It is used to seed random number engines in a way that avoids
	/// the pitfalls of seeding an engine with a single value
	/// </summary>
	public class Seed_Seq
	{
		// C++ typedef result_type = uint_least32_t
		private readonly List<uint_least32_t> v;

		/********************************************************************/
		/// <summary>
		/// Constructs an empty seed sequence
		/// </summary>
		/********************************************************************/
		public Seed_Seq()
		{
			v = new List<uint_least32_t>();
		}



		/********************************************************************/
		/// <summary>
		/// Constructs a seed sequence from the given values. Each value is
		/// taken modulo 2^32 (which for a <see cref="uint_least32_t"/> is a
		/// no-op)
		/// </summary>
		/********************************************************************/
		public Seed_Seq(params uint_least32_t[] il)
		{
			v = new List<uint_least32_t>(il);
		}



		/********************************************************************/
		/// <summary>
		/// Constructs a seed sequence from the range [begin, end). Each value
		/// is taken modulo 2^32 (which for a <see cref="uint_least32_t"/> is a
		/// no-op)
		/// </summary>
		/********************************************************************/
		public Seed_Seq(CPointer<uint_least32_t> begin, CPointer<uint_least32_t> end)
		{
			c_int count = end - begin;
			v = new List<uint_least32_t>(count);

			for (c_int i = 0; i < count; i++)
				v.Add(begin[i]);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the number of 32-bit values stored in the sequence
		/// (C++ size())
		/// </summary>
		/********************************************************************/
		public size_t size()
		{
			return (size_t)v.Count;
		}



		/********************************************************************/
		/// <summary>
		/// Copies the stored values into the given destination (C++ param())
		/// </summary>
		/********************************************************************/
		public void param(Span<uint_least32_t> dest)
		{
			for (c_int i = 0; i < v.Count; i++)
				dest[i] = v[i];
		}



		/********************************************************************/
		/// <summary>
		/// Fills the given range with evenly distributed 32-bit values,
		/// derived from the consumed data (C++ generate()).
		///
		/// This follows the exact algorithm mandated by the C++ standard,
		/// with all arithmetic performed modulo 2^32
		/// </summary>
		/********************************************************************/
		public void generate(Span<uint_least32_t> range)
		{
			if (range.Length == 0)
				return;

			range.Fill(0x8b8b8b8bU);

			// Note: The index arithmetic below uses size_t (unsigned 64-bit)
			// just like the reference implementation, so that expressions such
			// as (k - 1) % n produce the same results (in particular for k == 0)
			c_ulong n = (c_ulong)range.Length;
			c_ulong s = (c_ulong)v.Count;
			c_ulong t = (n >= 623) ? 11UL : (n >= 68) ? 7UL : (n >= 39) ? 5UL : (n >= 7) ? 3UL : (n - 1) / 2;
			c_ulong p = (n - t) / 2;
			c_ulong q = p + t;
			c_ulong m = Math.Max(s + 1, n);

			for (c_ulong k = 0; k < m; k++)
			{
				uint_least32_t arg = range[(c_int)(k % n)] ^ range[(c_int)((k + p) % n)] ^ range[(c_int)((k - 1) % n)];
				uint_least32_t r1 = 1664525U * (arg ^ (arg >> 27));

				uint_least32_t r2 = r1;
				if (k == 0)
					r2 += (uint_least32_t)s;
				else if (k <= s)
					r2 += (uint_least32_t)(k % n) + v[(c_int)(k - 1)];
				else
					r2 += (uint_least32_t)(k % n);

				range[(c_int)((k + p) % n)] += r1;
				range[(c_int)((k + q) % n)] += r2;
				range[(c_int)(k % n)] = r2;
			}

			for (c_ulong k = m; k < m + n; k++)
			{
				uint_least32_t arg = range[(c_int)(k % n)] + range[(c_int)((k + p) % n)] + range[(c_int)((k - 1) % n)];
				uint_least32_t r3 = 1566083941U * (arg ^ (arg >> 27));
				uint_least32_t r4 = (uint_least32_t)(r3 - (k % n));

				range[(c_int)((k + p) % n)] ^= r3;
				range[(c_int)((k + q) % n)] ^= r4;
				range[(c_int)(k % n)] = r4;
			}
		}
	}
}
