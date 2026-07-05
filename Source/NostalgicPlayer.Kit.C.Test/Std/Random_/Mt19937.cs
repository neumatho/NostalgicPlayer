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
	public class Mt19937 : TestRandomBase
	{
		/********************************************************************/
		/// <summary>
		/// The C++ standard mandates that the 10000th consecutive invocation
		/// of a default constructed mt19937 produces this value
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Mt19937_10000th_Value()
		{
			CRandom.Mt19937 engine = new CRandom.Mt19937();

			uint value = 0;
			for (int i = 0; i < 10000; i++)
				value = engine.Invoke();

			Assert.AreEqual(4123659995U, value);
		}



		/********************************************************************/
		/// <summary>
		/// The min/max range of mt19937 is [0, 2^32 - 1]
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Min_Max()
		{
			CRandom.Mt19937 engine = new CRandom.Mt19937();

			Assert.AreEqual(0U, engine.min());
			Assert.AreEqual(uint.MaxValue, engine.max());
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
			CRandom.Mt19937 a = new CRandom.Mt19937(12345U);
			CRandom.Mt19937 b = new CRandom.Mt19937(12345U);
			CRandom.Mt19937 c = new CRandom.Mt19937(54321U);

			bool sameDiffered = false;

			for (int i = 0; i < 1000; i++)
			{
				uint va = a.Invoke();
				uint vb = b.Invoke();
				uint vc = c.Invoke();

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
			CRandom.Mt19937 a = new CRandom.Mt19937(999U);
			CRandom.Mt19937 b = new CRandom.Mt19937(999U);

			// Advance 'a' by calling Invoke, and 'b' by discard
			for (int i = 0; i < 777; i++)
				a.Invoke();

			b.discard(777);

			Assert.AreEqual(a.Invoke(), b.Invoke());
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
			CRandom.Mt19937 engine = new CRandom.Mt19937();

			uint[] first = new uint[50];
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

			CRandom.Mt19937 a = new CRandom.Mt19937(seq1);
			CRandom.Mt19937 b = new CRandom.Mt19937(seq2);

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

			CRandom.Mt19937 a = new CRandom.Mt19937(fromList);
			CRandom.Mt19937 b = new CRandom.Mt19937(fromRange);

			for (int i = 0; i < 100; i++)
				Assert.AreEqual(a.Invoke(), b.Invoke());
		}
	}
}
