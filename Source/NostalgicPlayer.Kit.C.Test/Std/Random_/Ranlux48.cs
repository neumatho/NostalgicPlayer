/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using CRandom = Polycode.NostalgicPlayer.Kit.C.Std.Random;

using Polycode.NostalgicPlayer.Kit.C;

namespace NostalgicPlayer.Kit.C.Test.Std.Random_
{
	/// <summary>
	///
	/// </summary>
	[TestClass]
	public class Ranlux48 : TestRandomBase
	{
		/********************************************************************/
		/// <summary>
		/// The C++ standard mandates that the 10000th consecutive invocation
		/// of a default constructed ranlux48_base produces this value
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Ranlux48_Base_10000th_Value()
		{
			CRandom.Ranlux48_Base engine = new CRandom.Ranlux48_Base();

			ulong value = 0;
			for (int i = 0; i < 10000; i++)
				value = engine.Invoke();

			Assert.AreEqual(61839128582725UL, value);
		}



		/********************************************************************/
		/// <summary>
		/// The C++ standard mandates that the 10000th consecutive invocation
		/// of a default constructed ranlux48 produces this value
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Ranlux48_10000th_Value()
		{
			CRandom.Ranlux48 engine = new CRandom.Ranlux48();

			ulong value = 0;
			for (int i = 0; i < 10000; i++)
				value = engine.Invoke();

			Assert.AreEqual(249142670248501UL, value);
		}



		/********************************************************************/
		/// <summary>
		/// The min/max range of ranlux48 must match its base engine, which
		/// is [0, 2^48 - 1]
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Min_Max()
		{
			CRandom.Ranlux48 engine = new CRandom.Ranlux48();

			Assert.AreEqual(0UL, engine.min());
			Assert.AreEqual((1UL << 48) - 1, engine.max());

			CRandom.Ranlux48_Base baseEngine = new CRandom.Ranlux48_Base();

			Assert.AreEqual(0UL, baseEngine.min());
			Assert.AreEqual((1UL << 48) - 1, baseEngine.max());
		}



		/********************************************************************/
		/// <summary>
		/// Two engines seeded with the same value must produce the same
		/// sequence, while a different seed must (in practice) differ
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Reproducibility()
		{
			CRandom.Ranlux48 a = new CRandom.Ranlux48(12345UL);
			CRandom.Ranlux48 b = new CRandom.Ranlux48(12345UL);
			CRandom.Ranlux48 c = new CRandom.Ranlux48(54321UL);

			bool sameDiffered = false;

			for (int i = 0; i < 1000; i++)
			{
				ulong va = a.Invoke();
				ulong vb = b.Invoke();
				ulong vc = c.Invoke();

				Assert.AreEqual(va, vb);

				if (va != vc)
					sameDiffered = true;
			}

			Assert.IsTrue(sameDiffered);
		}



		/********************************************************************/
		/// <summary>
		/// discard(z) must be equivalent to calling Invoke() z times and
		/// discarding the results
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Discard()
		{
			CRandom.Ranlux48 a = new CRandom.Ranlux48(999UL);
			CRandom.Ranlux48 b = new CRandom.Ranlux48(999UL);

			// Advance 'a' by calling Invoke, and 'b' by discard
			for (int i = 0; i < 777; i++)
				a.Invoke();

			b.discard(777);

			Assert.AreEqual(a.Invoke(), b.Invoke());
		}



		/********************************************************************/
		/// <summary>
		/// Seeding with 0 must be equivalent to seeding with the default
		/// seed
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Seed_Zero_Is_Default()
		{
			CRandom.Ranlux48_Base zero = new CRandom.Ranlux48_Base(0UL);
			CRandom.Ranlux48_Base def = new CRandom.Ranlux48_Base();

			for (int i = 0; i < 100; i++)
				Assert.AreEqual(def.Invoke(), zero.Invoke());
		}



		/********************************************************************/
		/// <summary>
		/// Reseeding an engine must reset it so it produces the same
		/// sequence again
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Reseed_Resets_State()
		{
			CRandom.Ranlux48 engine = new CRandom.Ranlux48();

			ulong[] first = new ulong[50];
			for (int i = 0; i < first.Length; i++)
				first[i] = engine.Invoke();

			engine.seed();

			for (int i = 0; i < first.Length; i++)
				Assert.AreEqual(first[i], engine.Invoke());
		}



		/********************************************************************/
		/// <summary>
		/// Seeding with a seed sequence must be reproducible
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Seed_Seq_Reproducible()
		{
			CRandom.Seed_Seq seq1 = new CRandom.Seed_Seq(1U, 2U, 3U, 4U);
			CRandom.Seed_Seq seq2 = new CRandom.Seed_Seq(1U, 2U, 3U, 4U);

			CRandom.Ranlux48 a = new CRandom.Ranlux48(seq1);
			CRandom.Ranlux48 b = new CRandom.Ranlux48(seq2);

			for (int i = 0; i < 100; i++)
				Assert.AreEqual(a.Invoke(), b.Invoke());
		}



		/********************************************************************/
		/// <summary>
		/// A seed sequence constructed from a [begin, end) range must be
		/// equivalent to one constructed from the same values directly
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Seed_Seq_Range_Constructor()
		{
			uint[] data = [ 1U, 2U, 3U, 4U ];

			CRandom.Seed_Seq fromList = new CRandom.Seed_Seq(1U, 2U, 3U, 4U);
			CRandom.Seed_Seq fromRange = new CRandom.Seed_Seq(new CPointer<uint>(data), new CPointer<uint>(data) + data.Length);

			Assert.AreEqual(fromList.size(), fromRange.size());

			CRandom.Ranlux48 a = new CRandom.Ranlux48(fromList);
			CRandom.Ranlux48 b = new CRandom.Ranlux48(fromRange);

			for (int i = 0; i < 100; i++)
				Assert.AreEqual(a.Invoke(), b.Invoke());
		}
	}
}
