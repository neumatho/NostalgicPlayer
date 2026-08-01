/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using CRandom = Polycode.NostalgicPlayer.Kit.C.Std.Random;

using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.C.Std.Random;

namespace NostalgicPlayer.Kit.C.Test.Std.Random_
{
	/// <summary>
	///
	/// </summary>
	[TestClass]
	public class Uniform_Int_Distribution : TestRandomBase
	{
		/// <summary>
		/// A generator with a range that can be set from the outside, so the
		/// down-scaling, equal-range and up-scaling branches of the
		/// distribution can all be reached on purpose
		/// </summary>
		private class Narrow_Generator : IUniform_Random_Bit_Generator<ulong>
		{
			private readonly ulong lowest;
			private readonly ulong highest;
			private readonly CRandom.Ranlux48 source;

			public Narrow_Generator(ulong lowest, ulong highest, ulong seed = 4711UL)
			{
				this.lowest = lowest;
				this.highest = highest;

				source = new CRandom.Ranlux48(seed);
			}

			public ulong min()
			{
				return lowest;
			}

			public ulong max()
			{
				return highest;
			}

			public ulong Invoke()
			{
				return lowest + (source.Invoke() % ((highest - lowest) + 1));
			}
		}

		/********************************************************************/
		/// <summary>
		/// Every value produced must lie inside the closed range the
		/// distribution was constructed with, for both signed and unsigned
		/// types and for ranges crossing zero
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Values_Are_In_Range()
		{
			CRandom.Ranlux48 engine = new CRandom.Ranlux48(1234UL);

			CRandom.Uniform_Int_Distribution<sbyte> disI8 = new CRandom.Uniform_Int_Distribution<sbyte>(-42, 69);
			CRandom.Uniform_Int_Distribution<byte> disU8 = new CRandom.Uniform_Int_Distribution<byte>(3, 250);
			CRandom.Uniform_Int_Distribution<short> disI16 = new CRandom.Uniform_Int_Distribution<short>(-1000, 1000);
			CRandom.Uniform_Int_Distribution<int> disI32 = new CRandom.Uniform_Int_Distribution<int>(-42, 69);
			CRandom.Uniform_Int_Distribution<long> disI64 = new CRandom.Uniform_Int_Distribution<long>(-42, 69);
			CRandom.Uniform_Int_Distribution<ulong> disU64 = new CRandom.Uniform_Int_Distribution<ulong>(7, 9999);

			for (int i = 0; i < 10000; i++)
			{
				sbyte v8 = disI8.Invoke(engine);
				Assert.IsTrue((v8 >= -42) && (v8 <= 69));

				byte u8 = disU8.Invoke(engine);
				Assert.IsTrue((u8 >= 3) && (u8 <= 250));

				short v16 = disI16.Invoke(engine);
				Assert.IsTrue((v16 >= -1000) && (v16 <= 1000));

				int v32 = disI32.Invoke(engine);
				Assert.IsTrue((v32 >= -42) && (v32 <= 69));

				long v64 = disI64.Invoke(engine);
				Assert.IsTrue((v64 >= -42) && (v64 <= 69));

				ulong u64 = disU64.Invoke(engine);
				Assert.IsTrue((u64 >= 7) && (u64 <= 9999));
			}
		}



		/********************************************************************/
		/// <summary>
		/// A range holding a single value must always produce that value,
		/// also when it is negative
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Single_Value_Range()
		{
			CRandom.Ranlux48 engine = new CRandom.Ranlux48(99UL);

			CRandom.Uniform_Int_Distribution<int> zero = new CRandom.Uniform_Int_Distribution<int>(0, 0);
			CRandom.Uniform_Int_Distribution<int> negative = new CRandom.Uniform_Int_Distribution<int>(-7, -7);

			for (int i = 0; i < 100; i++)
			{
				Assert.AreEqual(0, zero.Invoke(engine));
				Assert.AreEqual(-7, negative.Invoke(engine));
			}
		}



		/********************************************************************/
		/// <summary>
		/// The full range of a 64 bit type is wider than what ranlux48 can
		/// produce in one go, so this goes through the up-scaling branch. It
		/// must terminate and must not overflow into a value outside the
		/// range
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Full_64_Bit_Range()
		{
			CRandom.Ranlux48 engine = new CRandom.Ranlux48(2024UL);

			CRandom.Uniform_Int_Distribution<long> signed = new CRandom.Uniform_Int_Distribution<long>(long.MinValue, long.MaxValue);
			CRandom.Uniform_Int_Distribution<ulong> unsigned = new CRandom.Uniform_Int_Distribution<ulong>(ulong.MinValue, ulong.MaxValue);

			bool sawNegative = false;
			bool sawPositive = false;

			for (int i = 0; i < 1000; i++)
			{
				long value = signed.Invoke(engine);

				if (value < 0)
					sawNegative = true;

				if (value > 0)
					sawPositive = true;

				// Only has to terminate and stay inside the type
				unsigned.Invoke(engine);
			}

			Assert.IsTrue(sawNegative);
			Assert.IsTrue(sawPositive);
		}



		/********************************************************************/
		/// <summary>
		/// A generator with a narrower range than the wanted one goes through
		/// the up-scaling branch, where two values are combined into one
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Generator_Narrower_Than_Range()
		{
			// Only 4 bits of entropy per call, against a range of 100000
			Narrow_Generator generator = new Narrow_Generator(0, 15);

			CRandom.Uniform_Int_Distribution<int> dis = new CRandom.Uniform_Int_Distribution<int>(0, 100000);

			HashSet<int> seen = new HashSet<int>();

			for (int i = 0; i < 5000; i++)
			{
				int value = dis.Invoke(generator);

				Assert.IsTrue((value >= 0) && (value <= 100000));
				seen.Add(value);
			}

			// A 4 bit generator must still be able to cover a wide range
			Assert.IsTrue(seen.Count > 1000);
		}



		/********************************************************************/
		/// <summary>
		/// A generator whose range is exactly the wanted one takes the third
		/// branch, where the value is used as it is
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Generator_Matches_Range()
		{
			// Range of the generator is [100, 355], which is the same size as
			// the range asked for below
			Narrow_Generator generator = new Narrow_Generator(100, 355);

			CRandom.Uniform_Int_Distribution<int> dis = new CRandom.Uniform_Int_Distribution<int>(-128, 127);

			for (int i = 0; i < 1000; i++)
			{
				int value = dis.Invoke(generator);

				Assert.IsTrue((value >= -128) && (value <= 127));
			}
		}



		/********************************************************************/
		/// <summary>
		/// A generator that does not start at zero must not shift the result
		/// away from the wanted range
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Generator_With_Non_Zero_Min()
		{
			Narrow_Generator generator = new Narrow_Generator(1000, 1000000);

			CRandom.Uniform_Int_Distribution<int> dis = new CRandom.Uniform_Int_Distribution<int>(-5, 5);

			for (int i = 0; i < 5000; i++)
			{
				int value = dis.Invoke(generator);

				Assert.IsTrue((value >= -5) && (value <= 5));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Every value of the range must come up, and none of them may
		/// dominate. This is a coarse check, not a real statistical test
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Distribution_Is_Even()
		{
			CRandom.Ranlux48 engine = new CRandom.Ranlux48(555UL);

			CRandom.Uniform_Int_Distribution<int> dis = new CRandom.Uniform_Int_Distribution<int>(0, 9);

			const int Draws = 100000;
			int[] buckets = new int[10];

			for (int i = 0; i < Draws; i++)
				buckets[dis.Invoke(engine)]++;

			// With an even distribution every bucket holds a tenth of the
			// draws. Allow a wide margin, so this does not turn flaky
			foreach (int count in buckets)
			{
				Assert.IsTrue(count > ((Draws / 10) / 2));
				Assert.IsTrue(count < ((Draws / 10) * 2));
			}
		}



		/********************************************************************/
		/// <summary>
		/// The same seed must give the same sequence of values
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Reproducibility()
		{
			CRandom.Ranlux48 a = new CRandom.Ranlux48(31337UL);
			CRandom.Ranlux48 b = new CRandom.Ranlux48(31337UL);

			CRandom.Uniform_Int_Distribution<int> disA = new CRandom.Uniform_Int_Distribution<int>(-1000, 1000);
			CRandom.Uniform_Int_Distribution<int> disB = new CRandom.Uniform_Int_Distribution<int>(-1000, 1000);

			for (int i = 0; i < 1000; i++)
				Assert.AreEqual(disA.Invoke(a), disB.Invoke(b));
		}



		/********************************************************************/
		/// <summary>
		/// a(), b(), min() and max() must report what the distribution was
		/// constructed with, and the default constructors must fill in the
		/// largest value of the type
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Range_Accessors()
		{
			CRandom.Uniform_Int_Distribution<int> dis = new CRandom.Uniform_Int_Distribution<int>(-17, 4711);

			Assert.AreEqual(-17, dis.a());
			Assert.AreEqual(4711, dis.b());
			Assert.AreEqual(-17, dis.min());
			Assert.AreEqual(4711, dis.max());

			CRandom.Uniform_Int_Distribution<int> byDefault = new CRandom.Uniform_Int_Distribution<int>();

			Assert.AreEqual(0, byDefault.a());
			Assert.AreEqual(int.MaxValue, byDefault.b());

			CRandom.Uniform_Int_Distribution<int> onlyA = new CRandom.Uniform_Int_Distribution<int>(42);

			Assert.AreEqual(42, onlyA.a());
			Assert.AreEqual(int.MaxValue, onlyA.b());
		}



		/********************************************************************/
		/// <summary>
		/// param() must give back the parameters, param(parm) must replace
		/// them, and the overload taking parameters must leave the ones of
		/// the distribution untouched
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Param()
		{
			CRandom.Ranlux48 engine = new CRandom.Ranlux48(8080UL);

			CRandom.Uniform_Int_Distribution<int> dis = new CRandom.Uniform_Int_Distribution<int>(0, 10);

			CRandom.Uniform_Int_Distribution<int>.Param_Type other = new CRandom.Uniform_Int_Distribution<int>.Param_Type(500, 600);

			for (int i = 0; i < 100; i++)
			{
				int value = dis.Invoke(engine, other);

				Assert.IsTrue((value >= 500) && (value <= 600));
			}

			// The distribution itself must be unchanged
			Assert.AreEqual(0, dis.a());
			Assert.AreEqual(10, dis.b());

			dis.param(other);

			Assert.AreEqual(500, dis.a());
			Assert.AreEqual(600, dis.b());
			Assert.AreEqual(other, dis.param());

			// reset() keeps no state, so it must change nothing
			dis.reset();

			Assert.AreEqual(500, dis.a());
			Assert.AreEqual(600, dis.b());
		}



		/********************************************************************/
		/// <summary>
		/// Two distributions are equal when they have the same parameters
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Equality()
		{
			CRandom.Uniform_Int_Distribution<int> a = new CRandom.Uniform_Int_Distribution<int>(1, 100);
			CRandom.Uniform_Int_Distribution<int> b = new CRandom.Uniform_Int_Distribution<int>(1, 100);
			CRandom.Uniform_Int_Distribution<int> c = new CRandom.Uniform_Int_Distribution<int>(1, 101);

			Assert.IsTrue(a == b);
			Assert.IsFalse(a != b);
			Assert.IsTrue(a != c);
			Assert.IsFalse(a == c);

			Assert.AreEqual(a.GetHashCode(), b.GetHashCode());

			CRandom.Uniform_Int_Distribution<int>.Param_Type pa = new CRandom.Uniform_Int_Distribution<int>.Param_Type(1, 100);
			CRandom.Uniform_Int_Distribution<int>.Param_Type pb = new CRandom.Uniform_Int_Distribution<int>.Param_Type(1, 100);
			CRandom.Uniform_Int_Distribution<int>.Param_Type pc = new CRandom.Uniform_Int_Distribution<int>.Param_Type(2, 100);

			Assert.IsTrue(pa == pb);
			Assert.IsTrue(pa != pc);
		}
	}
}
