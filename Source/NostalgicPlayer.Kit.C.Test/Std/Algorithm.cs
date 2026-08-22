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
			vector<int> v = new vector<int>([ 10, 20, 30, 40 ]);

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
			vector<int> v = new vector<int>([ 10, 20, 30, 40 ]);

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
		/// replace must replace every element that equals the old value and
		/// leave the other elements untouched
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Replace_Replaces_All_Matches()
		{
			CPointer<int> data = new int[] { 1, 4, 3, 4, 5 };
			forward_iterator<int> begin = new forward_iterator<int>(data);
			forward_iterator<int> end = new forward_iterator<int>(data.End());

			Algorithm.replace(begin, end, 4, 8);

			Assert.AreEqual(1, data[0]);
			Assert.AreEqual(8, data[1]);
			Assert.AreEqual(3, data[2]);
			Assert.AreEqual(8, data[3]);
			Assert.AreEqual(5, data[4]);
		}



		/********************************************************************/
		/// <summary>
		/// replace must not change anything when no element equals the old
		/// value
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Replace_No_Match_Does_Nothing()
		{
			CPointer<int> data = new int[] { 1, 3, 5 };
			forward_iterator<int> begin = new forward_iterator<int>(data);
			forward_iterator<int> end = new forward_iterator<int>(data.End());

			Algorithm.replace(begin, end, 4, 8);

			Assert.AreEqual(1, data[0]);
			Assert.AreEqual(3, data[1]);
			Assert.AreEqual(5, data[2]);
		}



		/********************************************************************/
		/// <summary>
		/// replace must only touch the elements in [first, last) and leave
		/// the surrounding elements untouched
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Replace_Leaves_Elements_Outside_Range_Untouched()
		{
			CPointer<int> data = new int[] { 4, 4, 2, 4, 4 };
			forward_iterator<int> first = new forward_iterator<int>(data + 1);
			forward_iterator<int> last = new forward_iterator<int>(data + 4);

			Algorithm.replace(first, last, 4, 0);

			Assert.AreEqual(4, data[0]);
			Assert.AreEqual(0, data[1]);
			Assert.AreEqual(2, data[2]);
			Assert.AreEqual(0, data[3]);
			Assert.AreEqual(4, data[4]);
		}



		/********************************************************************/
		/// <summary>
		/// replace on an empty range must not change anything
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Replace_Empty_Range_Does_Nothing()
		{
			CPointer<int> data = new int[] { 1, 2, 3 };
			forward_iterator<int> first = new forward_iterator<int>(data);

			Algorithm.replace(first, first, 1, 9);

			Assert.AreEqual(1, data[0]);
			Assert.AreEqual(2, data[1]);
			Assert.AreEqual(3, data[2]);
		}



		/********************************************************************/
		/// <summary>
		/// replace must work on the [begin(), end()) range of a vector
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Replace_On_Vector()
		{
			vector<int> v = new vector<int>([ 10, 20, 30, 20 ]);

			Algorithm.replace(v.begin(), v.end(), 20, 40);

			Assert.AreEqual(10, v[0]);
			Assert.AreEqual(40, v[1]);
			Assert.AreEqual(30, v[2]);
			Assert.AreEqual(40, v[3]);
		}



		/********************************************************************/
		/// <summary>
		/// replace must give each replaced element an independent deep clone
		/// of the new value when the element type implements IDeepCloneable,
		/// so that the elements do not share the same instance
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Replace_Deep_Clones_Each_Element()
		{
			Cloneable old_value = new Cloneable(1);
			CPointer<Cloneable> data = new Cloneable[] { old_value, new Cloneable(2), old_value };
			forward_iterator<Cloneable> begin = new forward_iterator<Cloneable>(data);
			forward_iterator<Cloneable> end = new forward_iterator<Cloneable>(data.End());

			Algorithm.replace(begin, end, old_value, new Cloneable(5));

			Assert.AreEqual(5, data[0].Value);
			Assert.AreEqual(2, data[1].Value);
			Assert.AreEqual(5, data[2].Value);
			Assert.IsFalse(ReferenceEquals(data[0], data[2]));
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
			vector<int> v = new vector<int>([ 10, 20, 30 ]);

			Assert.IsTrue(Algorithm.equal<CPointer<int>, forward_iterator<int>, int>(a, a + 3, v.begin(), v.end()));
		}



		/********************************************************************/
		/// <summary>
		/// partial_sort must put the smallest elements of the range in
		/// ascending order at the beginning of it
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Partial_Sort_Sorts_The_Smallest_Elements()
		{
			CPointer<int> data = new int[] { 5, 1, 9, 3, 7, 2 };
			forward_iterator<int> begin = new forward_iterator<int>(data);
			forward_iterator<int> end = new forward_iterator<int>(data.End());

			Algorithm.partial_sort<forward_iterator<int>, int>(begin, begin + 3, end);

			Assert.AreEqual(1, data[0]);
			Assert.AreEqual(2, data[1]);
			Assert.AreEqual(3, data[2]);

			// The rest of the range holds the remaining elements in an
			// unspecified order
			int[] expected = [ 5, 7, 9 ];
			int[] rest = [ data[3], data[4], data[5] ];

			CollectionAssert.AreEquivalent(expected, rest);
		}



		/********************************************************************/
		/// <summary>
		/// partial_sort must sort the whole range when middle is last
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Partial_Sort_Whole_Range_Sorts_Everything()
		{
			CPointer<int> data = new int[] { 4, 8, 1, 1, 6, 3, 9, 2 };
			forward_iterator<int> begin = new forward_iterator<int>(data);
			forward_iterator<int> end = new forward_iterator<int>(data.End());

			Algorithm.partial_sort<forward_iterator<int>, int>(begin, end, end);

			int[] expected = [ 1, 1, 2, 3, 4, 6, 8, 9 ];
			int[] actual = [ data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7] ];

			CollectionAssert.AreEqual(expected, actual);
		}



		/********************************************************************/
		/// <summary>
		/// partial_sort must do nothing when middle is first
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Partial_Sort_Empty_Sort_Range_Does_Nothing()
		{
			CPointer<int> data = new int[] { 5, 1, 9 };
			forward_iterator<int> begin = new forward_iterator<int>(data);
			forward_iterator<int> end = new forward_iterator<int>(data.End());

			Algorithm.partial_sort<forward_iterator<int>, int>(begin, begin, end);

			int[] expected = [ 5, 1, 9 ];
			int[] actual = [ data[0], data[1], data[2] ];

			CollectionAssert.AreEqual(expected, actual);
		}



		/********************************************************************/
		/// <summary>
		/// partial_sort on an empty range must do nothing
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Partial_Sort_Empty_Range_Does_Nothing()
		{
			CPointer<int> data = new int[] { 5, 1, 9 };
			forward_iterator<int> first = new forward_iterator<int>(data.Begin());

			Algorithm.partial_sort<forward_iterator<int>, int>(first, first, first);

			int[] expected = [ 5, 1, 9 ];
			int[] actual = [ data[0], data[1], data[2] ];

			CollectionAssert.AreEqual(expected, actual);
		}



		/********************************************************************/
		/// <summary>
		/// partial_sort must leave the elements outside the given range
		/// untouched
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Partial_Sort_Leaves_Elements_Outside_Range_Untouched()
		{
			CPointer<int> data = new int[] { 8, 8, 5, 1, 3, 8, 8 };

			Algorithm.partial_sort<CPointer<int>, int>(data + 2, data + 4, data + 5);

			Assert.AreEqual(8, data[0]);
			Assert.AreEqual(8, data[1]);
			Assert.AreEqual(1, data[2]);
			Assert.AreEqual(3, data[3]);
			Assert.AreEqual(5, data[4]);
			Assert.AreEqual(8, data[5]);
			Assert.AreEqual(8, data[6]);
		}



		/********************************************************************/
		/// <summary>
		/// partial_sort must keep every duplicate of the values it moves to
		/// the beginning of the range
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Partial_Sort_Handles_Duplicates()
		{
			CPointer<int> data = new int[] { 2, 1, 2, 1, 2, 1 };

			Algorithm.partial_sort<CPointer<int>, int>(data, data + 4, data + 6);

			int[] expected = [ 1, 1, 1, 2, 2, 2 ];
			int[] actual = [ data[0], data[1], data[2], data[3], data[4], data[5] ];

			CollectionAssert.AreEqual(expected, actual);
		}



		/********************************************************************/
		/// <summary>
		/// partial_sort must use the given comparer, so that the largest
		/// elements can be moved to the beginning of the range instead
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Partial_Sort_With_Comparer()
		{
			CPointer<int> data = new int[] { 5, 1, 9, 3, 7, 2 };

			Algorithm.partial_sort(data, data + 2, data + 6, (int a, int b) => a > b);

			Assert.AreEqual(9, data[0]);
			Assert.AreEqual(7, data[1]);

			int[] expected = [ 1, 2, 3, 5 ];
			int[] rest = [ data[2], data[3], data[4], data[5] ];

			CollectionAssert.AreEquivalent(expected, rest);
		}



		/********************************************************************/
		/// <summary>
		/// partial_sort must work on the [begin(), end()) range of a vector
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Partial_Sort_On_Vector()
		{
			vector<int> v = new vector<int>([ 40, 10, 30, 20 ]);

			Algorithm.partial_sort<forward_iterator<int>, int>(v.begin(), v.begin() + 2, v.end());

			Assert.AreEqual(10, v[0]);
			Assert.AreEqual(20, v[1]);

			int[] expected = [ 30, 40 ];
			int[] rest = [ v[2], v[3] ];

			CollectionAssert.AreEquivalent(expected, rest);
		}



		/********************************************************************/
		/// <summary>
		/// partial_sort must work on a reverse_iterator range, which sorts
		/// the elements from the end of the range and backwards
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Partial_Sort_On_Reverse_Iterators()
		{
			CPointer<int> data = new int[] { 5, 1, 9, 3 };
			reverse_iterator<int> rbegin = new reverse_iterator<int>(new forward_iterator<int>(data.End()));
			reverse_iterator<int> rend = new reverse_iterator<int>(new forward_iterator<int>(data.Begin()));

			Algorithm.partial_sort<reverse_iterator<int>, int>(rbegin, rbegin + 2, rend);

			Assert.AreEqual(1, data[3]);
			Assert.AreEqual(3, data[2]);

			int[] expected = [ 5, 9 ];
			int[] rest = [ data[0], data[1] ];

			CollectionAssert.AreEquivalent(expected, rest);
		}



		/********************************************************************/
		/// <summary>
		/// binary_search must find every element of a sorted range
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Binary_Search_Finds_Every_Element()
		{
			CPointer<int> data = new int[] { 1, 3, 5, 7, 9, 11 };
			forward_iterator<int> begin = new forward_iterator<int>(data);
			forward_iterator<int> end = new forward_iterator<int>(data.End());

			for (int i = 0; i < 6; i++)
				Assert.IsTrue(Algorithm.binary_search(begin, end, data[i]));
		}



		/********************************************************************/
		/// <summary>
		/// binary_search must return false when no element equals the given
		/// value
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Binary_Search_No_Match_Returns_False()
		{
			CPointer<int> data = new int[] { 1, 3, 5, 7, 9, 11 };
			forward_iterator<int> begin = new forward_iterator<int>(data);
			forward_iterator<int> end = new forward_iterator<int>(data.End());

			Assert.IsFalse(Algorithm.binary_search(begin, end, 0));
			Assert.IsFalse(Algorithm.binary_search(begin, end, 4));
			Assert.IsFalse(Algorithm.binary_search(begin, end, 12));
		}



		/********************************************************************/
		/// <summary>
		/// binary_search must find a value which is present more than once
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Binary_Search_Handles_Duplicates()
		{
			CPointer<int> data = new int[] { 1, 2, 2, 2, 5 };

			Assert.IsTrue(Algorithm.binary_search(data, data + 5, 2));
			Assert.IsFalse(Algorithm.binary_search(data, data + 5, 3));
		}



		/********************************************************************/
		/// <summary>
		/// binary_search on an empty range must return false
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Binary_Search_Empty_Range_Returns_False()
		{
			CPointer<int> data = new int[] { 2, 4, 6 };
			forward_iterator<int> first = new forward_iterator<int>(data.Begin());

			Assert.IsFalse(Algorithm.binary_search(first, first, 2));
		}



		/********************************************************************/
		/// <summary>
		/// binary_search must work on the [begin(), end()) range of a vector
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Binary_Search_On_Vector()
		{
			vector<int> v = new vector<int>([ 10, 20, 30, 40 ]);

			Assert.IsTrue(Algorithm.binary_search(v.begin(), v.end(), 40));
			Assert.IsFalse(Algorithm.binary_search(v.begin(), v.end(), 35));
		}



		/********************************************************************/
		/// <summary>
		/// binary_search must use the given comparer, so that a range sorted
		/// in descending order can be searched
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Binary_Search_With_Comparer()
		{
			CPointer<int> data = new int[] { 11, 9, 7, 5, 3, 1 };

			Assert.IsTrue(Algorithm.binary_search(data, data + 6, 7, (int a, int b) => a > b));
			Assert.IsFalse(Algorithm.binary_search(data, data + 6, 8, (int a, int b) => a > b));
		}



		/********************************************************************/
		/// <summary>
		/// binary_search must work on a reverse_iterator range, which visits
		/// a descending range in ascending order
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Binary_Search_On_Reverse_Iterators()
		{
			CPointer<int> data = new int[] { 11, 9, 7, 5, 3, 1 };
			reverse_iterator<int> rbegin = new reverse_iterator<int>(new forward_iterator<int>(data.End()));
			reverse_iterator<int> rend = new reverse_iterator<int>(new forward_iterator<int>(data.Begin()));

			Assert.IsTrue(Algorithm.binary_search(rbegin, rend, 9));
			Assert.IsFalse(Algorithm.binary_search(rbegin, rend, 10));
		}



		/********************************************************************/
		/// <summary>
		/// fill must copy into the elements that the range already holds, so
		/// that everything referring to them sees the new value
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fill_Keeps_Existing_Elements()
		{
			CPointer<Copyable> data = new Copyable[] { new Copyable(1), new Copyable(2) };
			forward_iterator<Copyable> begin = new forward_iterator<Copyable>(data);
			forward_iterator<Copyable> end = new forward_iterator<Copyable>(data.End());

			Copyable existing = data[0];

			Algorithm.fill(begin, end, new Copyable(5));

			Assert.IsTrue(ReferenceEquals(existing, data[0]));
			Assert.AreEqual(5, data[0].Value);
			Assert.AreEqual(5, data[1].Value);
			Assert.IsFalse(ReferenceEquals(data[0], data[1]));
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



		/********************************************************************/
		/// <summary>
		/// A simple reference type that can copy itself into an existing
		/// instance, used to verify that the copying falls back to that way
		/// </summary>
		/********************************************************************/
		private sealed class Copyable : ICopyTo<Copyable>
		{
			public int Value;

			public Copyable()
			{
			}

			public Copyable(int value)
			{
				Value = value;
			}

			public void CopyTo(Copyable destination)
			{
				destination.Value = Value;
			}
		}
	}
}
