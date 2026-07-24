/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.C.Std.Iterators;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace NostalgicPlayer.Kit.C.Test.Std
{
	/// <summary>
	///
	/// </summary>
	[TestClass]
	public class Algorithm_ : TestStdBase
	{
		/********************************************************************/
		/// <summary>
		/// find must return an iterator to the first element that equals the
		/// given value
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Find_Returns_First_Match()
		{
			CPointer<int> data = new int[] { 1, 3, 4, 4, 6 };
			forward_iterator<int> begin = new forward_iterator<int>(data);
			forward_iterator<int> end = new forward_iterator<int>(data.End());

			forward_iterator<int> result = Algorithm.find(begin, end, 4);

			Assert.IsTrue(result != end);
			Assert.AreEqual(2, result - begin);
			Assert.AreEqual(4, result[0]);
		}



		/********************************************************************/
		/// <summary>
		/// find must return last when no element equals the given value
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Find_No_Match_Returns_Last()
		{
			CPointer<int> data = new int[] { 1, 3, 5, 7 };
			forward_iterator<int> begin = new forward_iterator<int>(data);
			forward_iterator<int> end = new forward_iterator<int>(data.End());

			forward_iterator<int> result = Algorithm.find(begin, end, 4);

			Assert.IsTrue(result == end);
		}



		/********************************************************************/
		/// <summary>
		/// find on an empty range must return last
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Find_Empty_Range_Returns_Last()
		{
			CPointer<int> data = new int[] { 2, 4, 6 };
			forward_iterator<int> first = new forward_iterator<int>(data.Begin());

			forward_iterator<int> result = Algorithm.find(first, first, 2);

			Assert.IsTrue(result == first);
		}



		/********************************************************************/
		/// <summary>
		/// find must work on the [begin(), end()) range of a vector
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Find_On_Vector()
		{
			vector<int> v = new vector<int>(new[] { 10, 20, 30, 40 });

			forward_iterator<int> result = Algorithm.find(v.begin(), v.end(), 30);

			Assert.IsTrue(result != v.end());
			Assert.AreEqual(30, result[0]);
		}



		/********************************************************************/
		/// <summary>
		/// find_if must return an iterator to the first element for which
		/// the predicate returns true
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Find_If_Returns_First_Match()
		{
			CPointer<int> data = new int[] { 1, 3, 4, 5, 6 };
			forward_iterator<int> begin = new forward_iterator<int>(data);
			forward_iterator<int> end = new forward_iterator<int>(data.End());

			forward_iterator<int> result = Algorithm.find_if(begin, end, (int x) => (x % 2) == 0);

			Assert.IsTrue(result != end);
			Assert.AreEqual(2, result - begin);
			Assert.AreEqual(4, result[0]);
		}



		/********************************************************************/
		/// <summary>
		/// find_if must return last when no element satisfies the predicate
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Find_If_No_Match_Returns_Last()
		{
			CPointer<int> data = new int[] { 1, 3, 5, 7 };
			forward_iterator<int> begin = new forward_iterator<int>(data);
			forward_iterator<int> end = new forward_iterator<int>(data.End());

			forward_iterator<int> result = Algorithm.find_if(begin, end, (int x) => (x % 2) == 0);

			Assert.IsTrue(result == end);
		}



		/********************************************************************/
		/// <summary>
		/// find_if on an empty range must return last
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Find_If_Empty_Range_Returns_Last()
		{
			CPointer<int> data = new int[] { 2, 4, 6 };
			forward_iterator<int> first = new forward_iterator<int>(data.Begin());

			forward_iterator<int> result = Algorithm.find_if(first, first, (int x) => (x % 2) == 0);

			Assert.IsTrue(result == first);
		}



		/********************************************************************/
		/// <summary>
		/// find_if must work on the [begin(), end()) range of a vector
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Find_If_On_Vector()
		{
			vector<int> v = new vector<int>(new[] { 10, 20, 30, 40 });

			forward_iterator<int> result = Algorithm.find_if(v.begin(), v.end(), (int x) => x > 25);

			Assert.IsTrue(result != v.end());
			Assert.AreEqual(30, result[0]);
		}



		/********************************************************************/
		/// <summary>
		/// fill must assign the given value to every element in the range
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fill_Assigns_Whole_Range()
		{
			CPointer<int> data = new int[] { 1, 2, 3, 4 };
			forward_iterator<int> begin = new forward_iterator<int>(data);
			forward_iterator<int> end = new forward_iterator<int>(data.End());

			Algorithm.fill(begin, end, 7);

			Assert.AreEqual(7, data[0]);
			Assert.AreEqual(7, data[1]);
			Assert.AreEqual(7, data[2]);
			Assert.AreEqual(7, data[3]);
		}



		/********************************************************************/
		/// <summary>
		/// fill must only touch the elements in [first, last) and leave the
		/// surrounding elements untouched
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fill_Leaves_Elements_Outside_Range_Untouched()
		{
			CPointer<int> data = new int[] { 1, 2, 3, 4, 5 };
			forward_iterator<int> first = new forward_iterator<int>(data + 1);
			forward_iterator<int> last = new forward_iterator<int>(data + 4);

			Algorithm.fill(first, last, 0);

			Assert.AreEqual(1, data[0]);
			Assert.AreEqual(0, data[1]);
			Assert.AreEqual(0, data[2]);
			Assert.AreEqual(0, data[3]);
			Assert.AreEqual(5, data[4]);
		}



		/********************************************************************/
		/// <summary>
		/// fill on an empty range must not change anything
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fill_Empty_Range_Does_Nothing()
		{
			CPointer<int> data = new int[] { 1, 2, 3 };
			forward_iterator<int> first = new forward_iterator<int>(data);

			Algorithm.fill(first, first, 9);

			Assert.AreEqual(1, data[0]);
			Assert.AreEqual(2, data[1]);
			Assert.AreEqual(3, data[2]);
		}



		/********************************************************************/
		/// <summary>
		/// fill must give each element an independent deep clone of the value
		/// when the element type implements IDeepCloneable, so that the
		/// elements do not share the same instance
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fill_Deep_Clones_Each_Element()
		{
			CPointer<Cloneable> data = new Cloneable[2];
			forward_iterator<Cloneable> begin = new forward_iterator<Cloneable>(data);
			forward_iterator<Cloneable> end = new forward_iterator<Cloneable>(data.End());

			Algorithm.fill(begin, end, new Cloneable(5));

			Assert.AreEqual(5, data[0].Value);
			Assert.AreEqual(5, data[1].Value);
			Assert.IsFalse(ReferenceEquals(data[0], data[1]));
		}



		/********************************************************************/
		/// <summary>
		/// copy must copy every element of the source range into the
		/// destination range
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Copy_Copies_Whole_Range()
		{
			CPointer<int> source = new int[] { 1, 2, 3, 4 };
			CPointer<int> dest = new int[4];

			forward_iterator<int> result = Algorithm.copy<CPointer<int>, forward_iterator<int>, int>(source, source + 4, new forward_iterator<int>(dest));

			Assert.AreEqual(1, dest[0]);
			Assert.AreEqual(2, dest[1]);
			Assert.AreEqual(3, dest[2]);
			Assert.AreEqual(4, dest[3]);

			Assert.IsTrue(result == new forward_iterator<int>(dest.End()));
		}



		/********************************************************************/
		/// <summary>
		/// copy must return the destination iterator one past the last
		/// element copied, so a following copy continues where it left off
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Copy_Returns_End_Of_Destination()
		{
			CPointer<int> source = new int[] { 5, 6, 7 };
			CPointer<int> dest = new int[5];

			forward_iterator<int> mid = Algorithm.copy<CPointer<int>, forward_iterator<int>, int>(source, source + 2, new forward_iterator<int>(dest));
			Algorithm.copy<CPointer<int>, forward_iterator<int>, int>(source + 2, source + 3, mid);

			Assert.AreEqual(5, dest[0]);
			Assert.AreEqual(6, dest[1]);
			Assert.AreEqual(7, dest[2]);
			Assert.AreEqual(0, dest[3]);
		}



		/********************************************************************/
		/// <summary>
		/// copy on an empty range must not change the destination
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Copy_Empty_Range_Does_Nothing()
		{
			CPointer<int> source = new int[] { 1, 2, 3 };
			CPointer<int> dest = new int[] { 9, 9, 9 };

			Algorithm.copy<CPointer<int>, forward_iterator<int>, int>(source, source, new forward_iterator<int>(dest));

			Assert.AreEqual(9, dest[0]);
			Assert.AreEqual(9, dest[1]);
			Assert.AreEqual(9, dest[2]);
		}



		/********************************************************************/
		/// <summary>
		/// copy must give each destination element an independent deep clone
		/// of the source element when the element type implements
		/// IDeepCloneable, so that the two ranges do not share instances
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Copy_Deep_Clones_Each_Element()
		{
			CPointer<Cloneable> source = new Cloneable[] { new Cloneable(1), new Cloneable(2) };
			CPointer<Cloneable> dest = new Cloneable[2];

			Algorithm.copy<CPointer<Cloneable>, forward_iterator<Cloneable>, Cloneable>(source, source + 2, new forward_iterator<Cloneable>(dest));

			Assert.AreEqual(1, dest[0].Value);
			Assert.AreEqual(2, dest[1].Value);
			Assert.IsFalse(ReferenceEquals(source[0], dest[0]));
			Assert.IsFalse(ReferenceEquals(source[1], dest[1]));
		}



		/********************************************************************/
		/// <summary>
		/// equal (three iterator form) must return true when the second range
		/// matches the first element-wise, and false otherwise
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Equal_Three_Iterator_Compares_Ranges()
		{
			CPointer<int> a = new int[] { 1, 2, 3, 4 };
			CPointer<int> b = new int[] { 1, 2, 3, 4 };
			CPointer<int> c = new int[] { 1, 2, 9, 4 };

			Assert.IsTrue(Algorithm.equal<CPointer<int>, CPointer<int>, int>(a, a + 4, b));
			Assert.IsFalse(Algorithm.equal<CPointer<int>, CPointer<int>, int>(a, a + 4, c));
		}



		/********************************************************************/
		/// <summary>
		/// equal (three iterator form) must return true on an empty first
		/// range
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Equal_Three_Iterator_Empty_Range_Is_True()
		{
			CPointer<int> a = new int[] { 1, 2, 3 };
			CPointer<int> b = new int[] { 9, 9, 9 };

			Assert.IsTrue(Algorithm.equal<CPointer<int>, CPointer<int>, int>(a, a, b));
		}



		/********************************************************************/
		/// <summary>
		/// equal (four iterator form) must return true only when the two
		/// ranges have the same length and equal elements
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Equal_Four_Iterator_Checks_Length()
		{
			CPointer<int> a = new int[] { 1, 2, 3 };
			CPointer<int> b = new int[] { 1, 2, 3 };
			CPointer<int> c = new int[] { 1, 2, 3, 4 };

			Assert.IsTrue(Algorithm.equal<CPointer<int>, CPointer<int>, int>(a, a + 3, b, b + 3));
			Assert.IsFalse(Algorithm.equal<CPointer<int>, CPointer<int>, int>(a, a + 3, c, c + 4));
			Assert.IsFalse(Algorithm.equal<CPointer<int>, CPointer<int>, int>(c, c + 4, a, a + 3));
		}



		/********************************************************************/
		/// <summary>
		/// equal must work across different iterator types (a CPointer and a
		/// vector's forward_iterator) referring to equal elements
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Equal_Mixed_Iterator_Types()
		{
			CPointer<int> a = new int[] { 10, 20, 30 };
			vector<int> v = new vector<int>(new[] { 10, 20, 30 });

			Assert.IsTrue(Algorithm.equal<CPointer<int>, forward_iterator<int>, int>(a, a + 3, v.begin(), v.end()));
		}



		/********************************************************************/
		/// <summary>
		/// A simple reference type that supports deep cloning, used to verify
		/// the cloning behavior of fill and copy
		/// </summary>
		/********************************************************************/
		private sealed class Cloneable : IDeepCloneable<Cloneable>
		{
			public int Value;

			public Cloneable(int value)
			{
				Value = value;
			}

			public Cloneable MakeDeepClone()
			{
				return new Cloneable(Value);
			}
		}
	}
}
