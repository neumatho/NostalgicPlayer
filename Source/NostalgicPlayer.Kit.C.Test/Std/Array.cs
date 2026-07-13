/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.C.Std.Exceptions;
using Polycode.NostalgicPlayer.Kit.C.Std.Iterators;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace NostalgicPlayer.Kit.C.Test.Std
{
	/// <summary>
	///
	/// </summary>
	[TestClass]
	public class Array : TestStdBase
	{
		/********************************************************************/
		/// <summary>
		/// Constructing with a count must create that many default
		/// initialized elements
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_With_Count()
		{
			array<int> a = new array<int>((size_t)3);

			Assert.AreEqual(3UL, a.size());
			Assert.AreEqual(0, a[0]);
			Assert.AreEqual(0, a[1]);
			Assert.AreEqual(0, a[2]);
		}



		/********************************************************************/
		/// <summary>
		/// Constructing from an array must copy all the elements and set the
		/// size to the number of items
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_From_Array()
		{
			array<int> a = new array<int>(new[] { 1, 2, 3, 4 });

			Assert.AreEqual(4UL, a.size());
			Assert.AreEqual(1, a[0]);
			Assert.AreEqual(4, a[3]);
		}



		/********************************************************************/
		/// <summary>
		/// The copy constructor must create an independent copy
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Copy_Construction_Is_Independent()
		{
			array<int> original = new array<int>(new[] { 1, 2, 3 });
			array<int> copy = new array<int>(original);

			copy[0] = 99;

			Assert.AreEqual(1, original[0]);
			Assert.AreEqual(99, copy[0]);
		}



		/********************************************************************/
		/// <summary>
		/// The size must always equal N and never change
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Size_Equals_Max_Size()
		{
			array<int> a = new array<int>((size_t)5);

			Assert.AreEqual(5UL, a.size());
			Assert.AreEqual(5UL, a.max_size());
			Assert.IsFalse(a.empty());
		}



		/********************************************************************/
		/// <summary>
		/// A container constructed with N zero must be empty
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Zero_Sized_Is_Empty()
		{
			array<int> a = new array<int>((size_t)0);

			Assert.IsTrue(a.empty());
			Assert.AreEqual(0UL, a.size());
			Assert.IsTrue(a.begin() == a.end());
		}



		/********************************************************************/
		/// <summary>
		/// The indexer must return a modifiable reference to the element
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Indexer_Returns_Modifiable_Reference()
		{
			array<int> a = new array<int>(new[] { 1, 2, 3 });

			a[1] = 20;

			Assert.AreEqual(20, a[1]);
		}



		/********************************************************************/
		/// <summary>
		/// front and back must return the first and last elements
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Front_And_Back()
		{
			array<int> a = new array<int>(new[] { 1, 2, 3 });

			Assert.AreEqual(1, a.front());
			Assert.AreEqual(3, a.back());
		}



		/********************************************************************/
		/// <summary>
		/// at must return the element when in range
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_At_In_Range()
		{
			array<int> a = new array<int>(new[] { 1, 2, 3 });

			Assert.AreEqual(2, a.at(1));
		}



		/********************************************************************/
		/// <summary>
		/// at must throw out_of_range when the index is out of range
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_At_Out_Of_Range_Throws()
		{
			array<int> a = new array<int>(new[] { 1, 2, 3 });

			Assert.ThrowsExactly<out_of_range>(() => _ = a.at((size_t)3));
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
			array<int> a = new array<int>(new[] { 1, 2, 3, 4 });

			int sum = 0;

			for (CPointer<int> it = a.begin(); it != a.end(); it++)
				sum += it[0];

			Assert.AreEqual(10, sum);
		}



		/********************************************************************/
		/// <summary>
		/// A reverse iterator must walk the container in reverse order
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Reverse_Iterate()
		{
			array<int> a = new array<int>(new[] { 1, 2, 3 });

			int[] visited = new int[3];
			int index = 0;

			for (reverse_iterator<int> it = a.rbegin(); it != a.rend(); it++)
				visited[index++] = it[0];

			Assert.AreEqual(3, visited[0]);
			Assert.AreEqual(2, visited[1]);
			Assert.AreEqual(1, visited[2]);
		}



		/********************************************************************/
		/// <summary>
		/// data must return a pointer to the live buffer
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Data_Points_To_Buffer()
		{
			array<int> a = new array<int>(new[] { 1, 2, 3 });

			CPointer<int> ptr = a.data();
			ptr[1] = 20;

			Assert.AreEqual(20, a[1]);
		}



		/********************************************************************/
		/// <summary>
		/// fill must assign the value to every element
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fill()
		{
			array<int> a = new array<int>((size_t)4);

			a.fill(7);

			for (size_t i = 0; i < a.size(); i++)
				Assert.AreEqual(7, a[i]);
		}



		/********************************************************************/
		/// <summary>
		/// swap must exchange the contents of two equally sized containers
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Swap()
		{
			array<int> a = new array<int>(new[] { 1, 2, 3 });
			array<int> b = new array<int>(new[] { 9, 8, 7 });

			a.swap(b);

			Assert.AreEqual(9, a[0]);
			Assert.AreEqual(7, a[2]);
			Assert.AreEqual(1, b[0]);
			Assert.AreEqual(3, b[2]);
		}



		/********************************************************************/
		/// <summary>
		/// swap must throw when the two containers have different sizes
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Swap_Different_Size_Throws()
		{
			array<int> a = new array<int>((size_t)3);
			array<int> b = new array<int>((size_t)2);

			Assert.ThrowsExactly<System.ArgumentException>(() => a.swap(b));
		}



		/********************************************************************/
		/// <summary>
		/// Two containers with equal contents must compare equal
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Equality()
		{
			array<int> a = new array<int>(new[] { 1, 2, 3 });
			array<int> b = new array<int>(new[] { 1, 2, 3 });
			array<int> c = new array<int>(new[] { 1, 2, 4 });

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
			array<int> a = new array<int>(new[] { 1, 2, 3 });
			array<int> b = new array<int>(new[] { 1, 2, 4 });

			Assert.IsTrue(a < b);
			Assert.IsTrue(b > a);
			Assert.IsTrue(a <= b);
			Assert.IsTrue(b >= a);
		}



		/********************************************************************/
		/// <summary>
		/// Constructing from an array must deep clone the elements, so that
		/// the container does not share element instances with the source
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_Deep_Clones()
		{
			Cloneable[] items = [ new Cloneable(1), new Cloneable(2) ];

			array<Cloneable> a = new array<Cloneable>(items);

			Assert.IsFalse(ReferenceEquals(items[0], a[0]));
			Assert.AreEqual(1, a[0].Value);

			// Mutating the container must not affect the source
			a[0].Value = 99;
			Assert.AreEqual(1, items[0].Value);
		}



		/********************************************************************/
		/// <summary>
		/// fill must deep clone the value into every slot, so that each
		/// element is an independent instance
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fill_Deep_Clones()
		{
			Cloneable prototype = new Cloneable(7);

			array<Cloneable> a = new array<Cloneable>((size_t)3);
			a.fill(prototype);

			for (size_t i = 0; i < a.size(); i++)
			{
				Assert.AreEqual(7, a[i].Value);
				Assert.IsFalse(ReferenceEquals(prototype, a[i]));
			}

			Assert.IsFalse(ReferenceEquals(a[0], a[1]));
			Assert.IsFalse(ReferenceEquals(a[1], a[2]));
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
			array<Cloneable> original = new array<Cloneable>([ new Cloneable(1), new Cloneable(2) ]);

			array<Cloneable> copy = new array<Cloneable>(original);

			Assert.IsFalse(ReferenceEquals(original[0], copy[0]));
			Assert.AreEqual(1, copy[0].Value);

			// Mutating the copy must not affect the original
			copy[0].Value = 99;
			Assert.AreEqual(1, original[0].Value);
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
