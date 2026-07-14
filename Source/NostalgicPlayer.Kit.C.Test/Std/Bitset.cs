/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.C.Std.Exceptions;

namespace NostalgicPlayer.Kit.C.Test.Std
{
	/// <summary>
	///
	/// </summary>
	[TestClass]
	public class Bitset : TestStdBase
	{
		/********************************************************************/
		/// <summary>
		/// Constructing with a count must create that many bits, all zero
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_With_Count()
		{
			bitset b = new bitset((size_t)10);

			Assert.AreEqual(10UL, b.size());
			Assert.AreEqual(0UL, b.count());
			Assert.IsTrue(b.none());
		}



		/********************************************************************/
		/// <summary>
		/// Constructing from a value must initialize the low order bits
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_From_Value()
		{
			bitset b = new bitset((size_t)8, 0b10110);

			Assert.AreEqual(8UL, b.size());
			Assert.IsTrue(b[1]);
			Assert.IsTrue(b[2]);
			Assert.IsFalse(b[3]);
			Assert.IsTrue(b[4]);
			Assert.AreEqual(0b10110UL, b.to_ulong());
		}



		/********************************************************************/
		/// <summary>
		/// A value must be truncated to the number of bits in the bitset
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_From_Value_Truncated()
		{
			bitset b = new bitset((size_t)3, 0b10110);

			Assert.AreEqual(0b110UL, b.to_ulong());
			Assert.AreEqual(2UL, b.count());
		}



		/********************************************************************/
		/// <summary>
		/// Constructing from a string must set the bits, most significant
		/// character first
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_From_String()
		{
			bitset b = new bitset((size_t)6, "110");

			Assert.IsFalse(b[0]);
			Assert.IsTrue(b[1]);
			Assert.IsTrue(b[2]);
			Assert.AreEqual(6UL, b.size());
			Assert.AreEqual(6UL, b.to_ulong());
		}



		/********************************************************************/
		/// <summary>
		/// Constructing from a string with a bad character must throw
		/// invalid_argument
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_From_Bad_String_Throws()
		{
			Assert.ThrowsExactly<invalid_argument>(() => _ = new bitset((size_t)6, "1210"));
		}



		/********************************************************************/
		/// <summary>
		/// Constructing from a string with pos past the end must throw
		/// out_of_range
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_From_String_Bad_Pos_Throws()
		{
			Assert.ThrowsExactly<out_of_range>(() => _ = new bitset((size_t)6, "110", (size_t)4));
		}



		/********************************************************************/
		/// <summary>
		/// The copy constructor must create an independent copy
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Copy_Construction_Is_Independent()
		{
			bitset original = new bitset((size_t)8, 0b1010);
			bitset copy = new bitset(original);

			copy.set((size_t)0);

			Assert.IsFalse(original[0]);
			Assert.IsTrue(copy[0]);
		}



		/********************************************************************/
		/// <summary>
		/// The indexer must get and set individual bits
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Indexer_Get_And_Set()
		{
			bitset b = new bitset((size_t)8);

			b[3] = true;

			Assert.IsTrue(b[3]);
			Assert.IsFalse(b[2]);

			b[3] = false;
			Assert.IsFalse(b[3]);
		}



		/********************************************************************/
		/// <summary>
		/// test must return the bit when in range
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Test_In_Range()
		{
			bitset b = new bitset((size_t)8, 0b100);

			Assert.IsTrue(b.test((size_t)2));
			Assert.IsFalse(b.test((size_t)1));
		}



		/********************************************************************/
		/// <summary>
		/// test must throw out_of_range when the position is out of range
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Test_Out_Of_Range_Throws()
		{
			bitset b = new bitset((size_t)8);

			Assert.ThrowsExactly<out_of_range>(() => _ = b.test((size_t)8));
		}



		/********************************************************************/
		/// <summary>
		/// count, all, any and none must reflect the number of set bits
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Observers()
		{
			bitset b = new bitset((size_t)4);

			Assert.IsTrue(b.none());
			Assert.IsFalse(b.any());
			Assert.IsFalse(b.all());

			b.set((size_t)1);
			Assert.IsFalse(b.none());
			Assert.IsTrue(b.any());
			Assert.IsFalse(b.all());
			Assert.AreEqual(1UL, b.count());

			b.set();
			Assert.IsTrue(b.all());
			Assert.AreEqual(4UL, b.count());
		}



		/********************************************************************/
		/// <summary>
		/// set with no argument must set every bit, also when the last word
		/// is only partially used
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Set_All_Trims_Unused_Bits()
		{
			bitset b = new bitset((size_t)70);

			b.set();

			Assert.AreEqual(70UL, b.count());
			Assert.IsTrue(b.all());
		}



		/********************************************************************/
		/// <summary>
		/// set, reset and flip with a position must throw out_of_range when
		/// the position is out of range
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Modifiers_Out_Of_Range_Throw()
		{
			bitset b = new bitset((size_t)8);

			Assert.ThrowsExactly<out_of_range>(() => b.set((size_t)8));
			Assert.ThrowsExactly<out_of_range>(() => b.reset((size_t)8));
			Assert.ThrowsExactly<out_of_range>(() => b.flip((size_t)8));
		}



		/********************************************************************/
		/// <summary>
		/// reset must clear the whole bitset or a single bit
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Reset()
		{
			bitset b = new bitset((size_t)8).set();

			b.reset((size_t)3);
			Assert.IsFalse(b[3]);
			Assert.AreEqual(7UL, b.count());

			b.reset();
			Assert.AreEqual(0UL, b.count());
		}



		/********************************************************************/
		/// <summary>
		/// flip must invert the whole bitset or a single bit
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Flip()
		{
			bitset b = new bitset((size_t)4, 0b1010);

			b.flip();
			Assert.AreEqual(0b0101UL, b.to_ulong());

			b.flip((size_t)0);
			Assert.AreEqual(0b0100UL, b.to_ulong());
		}



		/********************************************************************/
		/// <summary>
		/// The bitwise operators must combine two bitsets
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Bitwise_Operators()
		{
			bitset a = new bitset((size_t)8, 0b1100);
			bitset b = new bitset((size_t)8, 0b1010);

			Assert.AreEqual(0b1000UL, (a & b).to_ulong());
			Assert.AreEqual(0b1110UL, (a | b).to_ulong());
			Assert.AreEqual(0b0110UL, (a ^ b).to_ulong());
		}



		/********************************************************************/
		/// <summary>
		/// The bitwise operators must throw when the sizes differ
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Bitwise_Operators_Different_Size_Throw()
		{
			bitset a = new bitset((size_t)8);
			bitset b = new bitset((size_t)4);

			Assert.ThrowsExactly<System.ArgumentException>(() => _ = a & b);
		}



		/********************************************************************/
		/// <summary>
		/// The complement operator must flip every bit
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Complement_Operator()
		{
			bitset a = new bitset((size_t)4, 0b1010);

			bitset c = ~a;

			Assert.AreEqual(0b0101UL, c.to_ulong());
			Assert.AreEqual(0b1010UL, a.to_ulong());
		}



		/********************************************************************/
		/// <summary>
		/// The shift operators must move the bits and discard those shifted
		/// past the ends
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Shift_Operators()
		{
			bitset a = new bitset((size_t)4, 0b0011);

			Assert.AreEqual(0b1100UL, (a << 2).to_ulong());
			Assert.AreEqual(0b1000UL, (a << 3).to_ulong());
			Assert.AreEqual(0b0000UL, (a << 4).to_ulong());

			bitset b = new bitset((size_t)4, 0b1100);
			Assert.AreEqual(0b0011UL, (b >> 2).to_ulong());
			Assert.AreEqual(0b0000UL, (b >> 4).to_ulong());
		}



		/********************************************************************/
		/// <summary>
		/// Shifting must work across word boundaries
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Shift_Across_Words()
		{
			bitset a = new bitset((size_t)70);
			a.set((size_t)1);

			bitset shifted = a << 64;

			Assert.IsTrue(shifted[65]);
			Assert.AreEqual(1UL, shifted.count());
		}



		/********************************************************************/
		/// <summary>
		/// Two bitsets with equal size and bits must compare equal
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Equality()
		{
			bitset a = new bitset((size_t)8, 0b1010);
			bitset b = new bitset((size_t)8, 0b1010);
			bitset c = new bitset((size_t)8, 0b1011);

			Assert.IsTrue(a == b);
			Assert.IsFalse(a == c);
			Assert.IsTrue(a != c);
		}



		/********************************************************************/
		/// <summary>
		/// Bitsets of different sizes must never compare equal
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Equality_Different_Size()
		{
			bitset a = new bitset((size_t)8, 0b1010);
			bitset b = new bitset((size_t)4, 0b1010);

			Assert.IsTrue(a != b);
		}



		/********************************************************************/
		/// <summary>
		/// to_string must render the bits, most significant first
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_To_String()
		{
			bitset b = new bitset((size_t)6, 0b101);

			Assert.AreEqual("000101", b.to_string());
			Assert.AreEqual("......", new bitset((size_t)6).to_string('.', '*'));
		}



		/********************************************************************/
		/// <summary>
		/// to_ulong must throw overflow_error when a bit that does not fit is
		/// set
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_To_Ulong_Overflow_Throws()
		{
			bitset b = new bitset((size_t)70);
			b.set((size_t)64);

			Assert.ThrowsExactly<overflow_error>(() => _ = b.to_ulong());
		}



		/********************************************************************/
		/// <summary>
		/// A bitset with no bits must behave consistently
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Zero_Sized()
		{
			bitset b = new bitset((size_t)0);

			Assert.AreEqual(0UL, b.size());
			Assert.AreEqual(0UL, b.count());
			Assert.IsTrue(b.none());
			Assert.IsTrue(b.all());
			Assert.AreEqual(string.Empty, b.to_string());
			Assert.AreEqual(0UL, b.to_ulong());
		}
	}
}
