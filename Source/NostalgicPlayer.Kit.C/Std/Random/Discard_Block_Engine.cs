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
	/// C# port of the C++ standard library std::discard_block_engine
	/// (see the C++ standard, [rand.adapt.disc]).
	///
	/// A random number engine adaptor that produces random numbers from a base
	/// engine, discarding some of the values it produces: from each block of
	/// p (block_size) values produced by the base engine, the adaptor keeps
	/// only the first r (used_block) values and discards the rest.
	///
	/// In C++ the block size (p) and used block (r) are non-type template
	/// parameters. Since C# does not support those, they are passed as
	/// constructor arguments instead
	/// </summary>
	public class Discard_Block_Engine<TEngine, TResult> : IRandom_Number_Engine<TResult> where TEngine : IRandom_Number_Engine<TResult>, new() where TResult : IBinaryInteger<TResult>, IUnsignedNumber<TResult>
	{
		private c_int blockSize;			// p
		private c_int usedBlock;			// r
		private TEngine baseEngine;			// The wrapped base engine
		private c_int used;					// Number of values used from the current block

		/********************************************************************/
		/// <summary>
		/// Constructs the adaptor with a default constructed base engine
		/// </summary>
		/********************************************************************/
		public Discard_Block_Engine(c_int p, c_int r)
		{
			Setup(p, r);
			used = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the adaptor and seeds the base engine with the given
		/// value
		/// </summary>
		/********************************************************************/
		public Discard_Block_Engine(c_int p, c_int r, TResult value)
		{
			Setup(p, r);
			baseEngine.seed(value);
			used = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the adaptor and seeds the base engine with the given
		/// seed sequence
		/// </summary>
		/********************************************************************/
		public Discard_Block_Engine(c_int p, c_int r, Seed_Seq q)
		{
			Setup(p, r);
			baseEngine.seed(q);
			used = 0;
		}



		/********************************************************************/
		/// <summary>
		/// The type of the values produced by the engine (C++ result_type)
		/// </summary>
		/********************************************************************/
		public static Type result_type => typeof(TResult);



		/********************************************************************/
		/// <summary>
		/// The number of values produced by the base engine in each block
		/// (C++ block_size)
		/// </summary>
		/********************************************************************/
		public c_int block_size => blockSize;



		/********************************************************************/
		/// <summary>
		/// The number of values used from each block (C++ used_block)
		/// </summary>
		/********************************************************************/
		public c_int used_block => usedBlock;



		/********************************************************************/
		/// <summary>
		/// The wrapped base engine (C++ base())
		/// </summary>
		/********************************************************************/
		public TEngine Base()
		{
			return baseEngine;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the smallest value that the engine can produce, which is
		/// the smallest value of the base engine
		/// </summary>
		/********************************************************************/
		public TResult min()
		{
			return baseEngine.min();
		}



		/********************************************************************/
		/// <summary>
		/// Returns the largest value that the engine can produce, which is
		/// the largest value of the base engine
		/// </summary>
		/********************************************************************/
		public TResult max()
		{
			return baseEngine.max();
		}



		/********************************************************************/
		/// <summary>
		/// Reseeds the base engine using the default seed
		/// </summary>
		/********************************************************************/
		public void seed()
		{
			baseEngine.seed();
			used = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Reseeds the base engine using the given value
		/// </summary>
		/********************************************************************/
		public void seed(TResult value)
		{
			baseEngine.seed(value);
			used = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Reseeds the base engine using the given seed sequence
		/// </summary>
		/********************************************************************/
		public void seed(Seed_Seq q)
		{
			baseEngine.seed(q);
			used = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Advances the engine's state and returns the generated value
		/// (C++ operator())
		/// </summary>
		/********************************************************************/
		public TResult Invoke()
		{
			if (used >= usedBlock)
			{
				baseEngine.discard((c_ulong_long)(blockSize - usedBlock));
				used = 0;
			}

			used++;

			return baseEngine.Invoke();
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
		/// have the same parameters, the same base engine state and have used
		/// the same amount of the current block
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			if (obj is not Discard_Block_Engine<TEngine, TResult> other)
				return false;

			return (used == other.used) && (blockSize == other.blockSize) && (usedBlock == other.usedBlock) && baseEngine.Equals(other.baseEngine);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a hash code for the engine
		/// </summary>
		/********************************************************************/
		public override c_int GetHashCode()
		{
			return HashCode.Combine(used, blockSize, usedBlock, baseEngine);
		}



		/********************************************************************/
		/// <summary>
		/// Compares two engines for equality
		/// </summary>
		/********************************************************************/
		public static bool operator ==(Discard_Block_Engine<TEngine, TResult> a, Discard_Block_Engine<TEngine, TResult> b)
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
		public static bool operator !=(Discard_Block_Engine<TEngine, TResult> a, Discard_Block_Engine<TEngine, TResult> b)
		{
			return !(a == b);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Initializes the adaptor parameters and default constructs the base
		/// engine
		/// </summary>
		/********************************************************************/
		private void Setup(c_int p, c_int r)
		{
			blockSize = p;
			usedBlock = r;
			baseEngine = new TEngine();
		}
		#endregion
	}
}
