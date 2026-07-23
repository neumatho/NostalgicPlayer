/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.C.Std.Exceptions;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace NostalgicPlayer.Kit.C.Test.Std
{
	/// <summary>
	///
	/// </summary>
	[TestClass]
	public class Vector : TestStdBase
	{
		/********************************************************************/
		/// <summary>
		/// A default constructed vector must be empty and have size zero
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Default_Construction_Is_Empty()
		{
			vector<int> v = new vector<int>();

			Assert.IsTrue(v.empty());
			Assert.AreEqual(0UL, v.size());
			Assert.IsTrue(v.begin() == v.end());
		}



		/********************************************************************/
		/// <summary>
		/// Constructing with a count must create that many default
		/// initialized elements
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_With_Count()
		{
			vector<int> v = new vector<int>((size_t)3);

			Assert.AreEqual(3UL, v.size());
			Assert.AreEqual(0, v[0]);
			Assert.AreEqual(0, v[1]);
			Assert.AreEqual(0, v[2]);
		}



		/********************************************************************/
		/// <summary>
		/// Constructing with a count and a value must fill the container with
		/// that value
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_With_Count_And_Value()
		{
			vector<int> v = new vector<int>((size_t)4, 7);

			Assert.AreEqual(4UL, v.size());

			for (size_t i = 0; i < v.size(); i++)
				Assert.AreEqual(7, v[i]);
		}



		/********************************************************************/
		/// <summary>
		/// Constructing from an array must copy all the elements
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_From_Array()
		{
			vector<int> v = new vector<int>(new[] { 1, 2, 3, 4 });

			Assert.AreEqual(4UL, v.size());
			Assert.AreEqual(1, v[0]);
			Assert.AreEqual(4, v[3]);
		}



		/********************************************************************/
		/// <summary>
		/// Constructing from a pointer range must copy the range
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_From_Range()
		{
			CPointer<int> source = new CPointer<int>(new[] { 10, 20, 30, 40, 50 });

			vector<int> v = new vector<int>(source + 1, source + 4);

			Assert.AreEqual(3UL, v.size());
			Assert.AreEqual(20, v[0]);
			Assert.AreEqual(30, v[1]);
			Assert.AreEqual(40, v[2]);
		}



		/********************************************************************/
		/// <summary>
		/// The copy constructor must create an independent copy
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Copy_Construction_Is_Independent()
		{
			vector<int> original = new vector<int>(new[] { 1, 2, 3 });
			vector<int> copy = new vector<int>(original);

			copy[0] = 99;

			Assert.AreEqual(1, original[0]);
			Assert.AreEqual(99, copy[0]);
		}



		/********************************************************************/
		/// <summary>
		/// push_back must append elements and grow the container
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Push_Back_Grows()
		{
			vector<int> v = new vector<int>();

			for (int i = 0; i < 100; i++)
				v.push_back(i);

			Assert.AreEqual(100UL, v.size());
			Assert.AreEqual(0, v.front());
			Assert.AreEqual(99, v.back());
			Assert.IsTrue(v.capacity() >= v.size());
		}



		/********************************************************************/
		/// <summary>
		/// emplace_back must append and return a reference to the new element
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Emplace_Back_Returns_Reference()
		{
			vector<int> v = new vector<int>();

			ref int slot = ref v.emplace_back(5);
			slot = 42;

			Assert.AreEqual(1UL, v.size());
			Assert.AreEqual(42, v[0]);
		}



		/********************************************************************/
		/// <summary>
		/// pop_back must remove the last element
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Pop_Back()
		{
			vector<int> v = new vector<int>(new[] { 1, 2, 3 });

			v.pop_back();

			Assert.AreEqual(2UL, v.size());
			Assert.AreEqual(2, v.back());
		}



		/********************************************************************/
		/// <summary>
		/// The indexer must return a modifiable reference to the element
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Indexer_Returns_Modifiable_Reference()
		{
			vector<int> v = new vector<int>(new[] { 1, 2, 3 });

			v[1] = 20;

			Assert.AreEqual(20, v[1]);
		}



		/********************************************************************/
		/// <summary>
		/// at must return the element when in range
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_At_In_Range()
		{
			vector<int> v = new vector<int>(new[] { 1, 2, 3 });

			Assert.AreEqual(2, v.at(1));
		}



		/********************************************************************/
		/// <summary>
		/// at must throw out_of_range when the index is out of range
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_At_Out_Of_Range_Throws()
		{
			vector<int> v = new vector<int>(new[] { 1, 2, 3 });

			Assert.ThrowsExactly<out_of_range>(() => _ = v.at((size_t)3));
		}



		/********************************************************************/
		/// <summary>
		/// A pointer from begin() must be usable as an iterator to walk the
		/// whole container
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Iterate_With_Pointer()
		{
			vector<int> v = new vector<int>(new[] { 1, 2, 3, 4 });

			int sum = 0;

			for (CPointer<int> it = v.begin(); it != v.end(); it++)
				sum += it[0];

			Assert.AreEqual(10, sum);
		}



		/********************************************************************/
		/// <summary>
		/// clear must remove all elements but keep the capacity
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Clear_Keeps_Capacity()
		{
			vector<int> v = new vector<int>(new[] { 1, 2, 3 });
			size_t capacityBefore = v.capacity();

			v.clear();

			Assert.IsTrue(v.empty());
			Assert.AreEqual(0UL, v.size());
			Assert.AreEqual(capacityBefore, v.capacity());
		}



		/********************************************************************/
		/// <summary>
		/// reserve must grow the capacity without changing the size
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Reserve()
		{
			vector<int> v = new vector<int>(new[] { 1, 2, 3 });

			v.reserve((size_t)100);

			Assert.IsTrue(v.capacity() >= 100UL);
			Assert.AreEqual(3UL, v.size());
			Assert.AreEqual(1, v[0]);
		}



		/********************************************************************/
		/// <summary>
		/// shrink_to_fit must reduce the capacity to the size
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Shrink_To_Fit()
		{
			vector<int> v = new vector<int>();
			v.reserve((size_t)100);
			v.push_back(1);
			v.push_back(2);

			v.shrink_to_fit();

			Assert.AreEqual(2UL, v.capacity());
			Assert.AreEqual(2UL, v.size());
		}



		/********************************************************************/
		/// <summary>
		/// resize must grow the container with default initialized elements
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Resize_Grow()
		{
			vector<int> v = new vector<int>(new[] { 1, 2 });

			v.resize((size_t)5);

			Assert.AreEqual(5UL, v.size());
			Assert.AreEqual(1, v[0]);
			Assert.AreEqual(0, v[4]);
		}



		/********************************************************************/
		/// <summary>
		/// resize must shrink the container
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Resize_Shrink()
		{
			vector<int> v = new vector<int>(new[] { 1, 2, 3, 4, 5 });

			v.resize((size_t)2);

			Assert.AreEqual(2UL, v.size());
			Assert.AreEqual(2, v.back());
		}



		/********************************************************************/
		/// <summary>
		/// resize with a value must grow the container with that value
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Resize_Grow_With_Value()
		{
			vector<int> v = new vector<int>(new[] { 1, 2 });

			v.resize((size_t)4, 9);

			Assert.AreEqual(4UL, v.size());
			Assert.AreEqual(9, v[2]);
			Assert.AreEqual(9, v[3]);
		}



		/********************************************************************/
		/// <summary>
		/// insert must insert a single value at the given position
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Insert_Single()
		{
			vector<int> v = new vector<int>(new[] { 1, 2, 4 });

			CPointer<int> result = v.insert(v.begin() + 2, 3);

			Assert.AreEqual(4UL, v.size());
			Assert.AreEqual(1, v[0]);
			Assert.AreEqual(2, v[1]);
			Assert.AreEqual(3, v[2]);
			Assert.AreEqual(4, v[3]);
			Assert.AreEqual(3, result[0]);
		}



		/********************************************************************/
		/// <summary>
		/// insert must insert multiple copies of a value
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Insert_Count()
		{
			vector<int> v = new vector<int>(new[] { 1, 4 });

			v.insert(v.begin() + 1, (size_t)3, 9);

			Assert.AreEqual(5UL, v.size());
			Assert.AreEqual(1, v[0]);
			Assert.AreEqual(9, v[1]);
			Assert.AreEqual(9, v[2]);
			Assert.AreEqual(9, v[3]);
			Assert.AreEqual(4, v[4]);
		}



		/********************************************************************/
		/// <summary>
		/// insert must insert a pointer range at the given position
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Insert_Range()
		{
			vector<int> v = new vector<int>(new[] { 1, 5 });
			CPointer<int> source = new CPointer<int>(new[] { 2, 3, 4 });

			v.insert(v.begin() + 1, source, source + 3);

			Assert.AreEqual(5UL, v.size());
			Assert.AreEqual(1, v[0]);
			Assert.AreEqual(2, v[1]);
			Assert.AreEqual(3, v[2]);
			Assert.AreEqual(4, v[3]);
			Assert.AreEqual(5, v[4]);
		}



		/********************************************************************/
		/// <summary>
		/// insert at end() must append the value
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Insert_At_End()
		{
			vector<int> v = new vector<int>(new[] { 1, 2 });

			v.insert(v.end(), 3);

			Assert.AreEqual(3UL, v.size());
			Assert.AreEqual(3, v.back());
		}



		/********************************************************************/
		/// <summary>
		/// erase must remove a single element and return the following
		/// position
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Erase_Single()
		{
			vector<int> v = new vector<int>(new[] { 1, 2, 3, 4 });

			CPointer<int> next = v.erase(v.begin() + 1);

			Assert.AreEqual(3UL, v.size());
			Assert.AreEqual(1, v[0]);
			Assert.AreEqual(3, v[1]);
			Assert.AreEqual(4, v[2]);
			Assert.AreEqual(3, next[0]);
		}



		/********************************************************************/
		/// <summary>
		/// erase must remove a range of elements
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Erase_Range()
		{
			vector<int> v = new vector<int>(new[] { 1, 2, 3, 4, 5 });

			v.erase(v.begin() + 1, v.begin() + 4);

			Assert.AreEqual(2UL, v.size());
			Assert.AreEqual(1, v[0]);
			Assert.AreEqual(5, v[1]);
		}



		/********************************************************************/
		/// <summary>
		/// assign must replace the contents with copies of a value
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Assign_Count_Value()
		{
			vector<int> v = new vector<int>(new[] { 1, 2, 3 });

			v.assign((size_t)2, 8);

			Assert.AreEqual(2UL, v.size());
			Assert.AreEqual(8, v[0]);
			Assert.AreEqual(8, v[1]);
		}



		/********************************************************************/
		/// <summary>
		/// swap must exchange the contents of two containers
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Swap()
		{
			vector<int> a = new vector<int>(new[] { 1, 2 });
			vector<int> b = new vector<int>(new[] { 9, 8, 7 });

			a.swap(b);

			Assert.AreEqual(3UL, a.size());
			Assert.AreEqual(9, a[0]);
			Assert.AreEqual(2UL, b.size());
			Assert.AreEqual(1, b[0]);
		}



		/********************************************************************/
		/// <summary>
		/// Two containers with equal contents must compare equal
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Equality()
		{
			vector<int> a = new vector<int>(new[] { 1, 2, 3 });
			vector<int> b = new vector<int>(new[] { 1, 2, 3 });
			vector<int> c = new vector<int>(new[] { 1, 2, 4 });

			Assert.IsTrue(a == b);
			Assert.IsFalse(a == c);
			Assert.IsTrue(a != c);
		}



		/********************************************************************/
		/// <summary>
		/// Containers must compare lexicographically
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Lexicographical_Compare()
		{
			vector<int> a = new vector<int>(new[] { 1, 2, 3 });
			vector<int> b = new vector<int>(new[] { 1, 2, 4 });
			vector<int> c = new vector<int>(new[] { 1, 2 });

			Assert.IsTrue(a < b);
			Assert.IsTrue(b > a);
			Assert.IsTrue(c < a);
			Assert.IsTrue(a >= c);
		}



		/********************************************************************/
		/// <summary>
		/// data must return a pointer to the live buffer
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Data_Points_To_Buffer()
		{
			vector<int> v = new vector<int>(new[] { 1, 2, 3 });

			CPointer<int> ptr = v.data();
			ptr[1] = 20;

			Assert.AreEqual(20, v[1]);
		}



		/********************************************************************/
		/// <summary>
		/// A fill construction must deep clone the value into every slot, so
		/// that each element is an independent instance
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fill_Deep_Clones()
		{
			Cloneable prototype = new Cloneable(7);

			vector<Cloneable> v = new vector<Cloneable>((size_t)3, prototype);

			Assert.AreEqual(3UL, v.size());

			for (size_t i = 0; i < v.size(); i++)
			{
				Assert.AreEqual(7, v[i].Value);
				Assert.IsFalse(ReferenceEquals(prototype, v[i]));
			}

			Assert.IsFalse(ReferenceEquals(v[0], v[1]));
			Assert.IsFalse(ReferenceEquals(v[1], v[2]));
		}



		/********************************************************************/
		/// <summary>
		/// The copy constructor must deep clone the elements, so that the two
		/// containers do not share element instances
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Copy_Construction_Deep_Clones()
		{
			vector<Cloneable> original = new vector<Cloneable>();
			original.push_back(new Cloneable(1));
			original.push_back(new Cloneable(2));

			vector<Cloneable> copy = new vector<Cloneable>(original);

			Assert.IsFalse(ReferenceEquals(original[0], copy[0]));
			Assert.AreEqual(1, copy[0].Value);

			// Mutating the copy must not affect the original
			copy[0].Value = 99;
			Assert.AreEqual(1, original[0].Value);
		}



		/********************************************************************/
		/// <summary>
		/// A range insertion must deep clone the inserted elements
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Insert_Range_Deep_Clones()
		{
			Cloneable[] items = [ new Cloneable(1), new Cloneable(2) ];
			CPointer<Cloneable> source = new CPointer<Cloneable>(items);

			vector<Cloneable> v = new vector<Cloneable>();
			v.insert(v.begin(), source, source + 2);

			Assert.IsFalse(ReferenceEquals(items[0], v[0]));
			Assert.AreEqual(1, v[0].Value);
			Assert.AreEqual(2, v[1].Value);
		}



		/********************************************************************/
		/// <summary>
		/// push_back takes a single value and must store the given instance
		/// directly, without cloning
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Push_Back_Does_Not_Clone()
		{
			Cloneable item = new Cloneable(5);

			vector<Cloneable> v = new vector<Cloneable>();
			v.push_back(item);

			Assert.IsTrue(ReferenceEquals(item, v[0]));
		}



		/********************************************************************/
		/// <summary>
		/// A foreach loop must visit every element in order
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Foreach_Visits_All_Elements()
		{
			vector<int> v = new vector<int>(new[] { 10, 20, 30 });

			int sum = 0;
			int visited = 0;

			foreach (int value in v)
			{
				sum += value;
				visited++;
			}

			Assert.AreEqual(3, visited);
			Assert.AreEqual(60, sum);
		}



		/********************************************************************/
		/// <summary>
		/// A foreach loop over a reference must be able to modify the elements
		/// in place
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Foreach_By_Reference_Modifies_Elements()
		{
			vector<int> v = new vector<int>(new[] { 1, 2, 3 });

			foreach (ref int value in v)
				value *= 10;

			Assert.AreEqual(10, v[0]);
			Assert.AreEqual(20, v[1]);
			Assert.AreEqual(30, v[2]);
		}



		/********************************************************************/
		/// <summary>
		/// A foreach loop must only visit the elements up to size(), not the
		/// unused capacity
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Foreach_Ignores_Unused_Capacity()
		{
			vector<int> v = new vector<int>();
			v.reserve((size_t)16);
			v.push_back(1);
			v.push_back(2);

			int visited = 0;

			foreach (int value in v)
				visited++;

			Assert.AreEqual(2, visited);
		}



		/********************************************************************/
		/// <summary>
		/// A foreach loop over an empty container must not visit any element
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Foreach_Empty_Visits_Nothing()
		{
			vector<int> v = new vector<int>();

			int visited = 0;

			foreach (int value in v)
				visited++;

			Assert.AreEqual(0, visited);
		}



		/********************************************************************/
		/// <summary>
		/// A simple reference type that supports deep cloning, used to verify
		/// the cloning behavior of the bulk operations
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
