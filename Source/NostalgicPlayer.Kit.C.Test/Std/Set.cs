/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace NostalgicPlayer.Kit.C.Test.Std
{
	/// <summary>
	///
	/// </summary>
	[TestClass]
	public class Set : TestStdBase
	{
		/********************************************************************/
		/// <summary>
		/// A default constructed set must be empty and have size zero
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Default_Construction_Is_Empty()
		{
			set<int> s = new set<int>();

			Assert.IsTrue(s.empty());
			Assert.AreEqual(0UL, s.size());
			Assert.IsTrue(s.begin() == s.end());
		}



		/********************************************************************/
		/// <summary>
		/// Constructing from an array must copy all the elements
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_From_Items()
		{
			set<int> s = new set<int>([ 2, 1, 3 ]);

			Assert.AreEqual(3UL, s.size());
			Assert.IsTrue(s.contains(1));
			Assert.IsTrue(s.contains(2));
			Assert.IsTrue(s.contains(3));
		}



		/********************************************************************/
		/// <summary>
		/// When the initializer holds duplicates, only one is kept
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_Skips_Duplicates()
		{
			set<string> s = new set<string>([ "one", "one", "two" ]);

			Assert.AreEqual(2UL, s.size());
			Assert.IsTrue(s.contains("one"));
			Assert.IsTrue(s.contains("two"));
		}



		/********************************************************************/
		/// <summary>
		/// insert must add a new element and report success, and must not
		/// insert a value that is already present
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Insert_Does_Not_Duplicate()
		{
			set<int> s = new set<int>();

			pair<set<int>.iterator, bool> r1 = s.insert(1);
			Assert.IsTrue(r1.second);
			Assert.AreEqual(1, r1.first.Value);

			pair<set<int>.iterator, bool> r2 = s.insert(1);
			Assert.IsFalse(r2.second);
			Assert.AreEqual(1, r2.first.Value);
			Assert.AreEqual(1UL, s.size());
		}



		/********************************************************************/
		/// <summary>
		/// The hint taking insert must return a plain iterator to the
		/// element, so that Next() (C++ std::next) can advance past it
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Insert_With_Hint()
		{
			set<int> s = new set<int>();

			set<int>.iterator hint = s.insert(s.end(), 1);
			Assert.AreEqual(1, hint.Value);

			// Next() on the returned iterator must give the successor (end)
			Assert.IsTrue(s.insert(hint, 2).Next() == s.end());

			// A second call with the same value must not insert
			s.insert(s.end(), 1);
			Assert.AreEqual(2UL, s.size());
		}



		/********************************************************************/
		/// <summary>
		/// insert must add every item of the given array
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Insert_Items()
		{
			set<int> s = new set<int>();
			s.insert([ 3, 1, 2, 1 ]);

			Assert.AreEqual(3UL, s.size());
			CollectionAssert.AreEqual(new[] { 1, 2, 3 }, s.ToList());
		}



		/********************************************************************/
		/// <summary>
		/// emplace and emplace_hint must insert only when the value is
		/// missing
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Emplace_And_Emplace_Hint()
		{
			set<int> s = new set<int>();

			Assert.IsTrue(s.emplace(1).second);
			Assert.IsFalse(s.emplace(1).second);

			Assert.AreEqual(2, s.emplace_hint(s.end(), 2).Value);
			Assert.AreEqual(2, s.emplace_hint(s.end(), 2).Value);

			Assert.AreEqual(2UL, s.size());
		}



		/********************************************************************/
		/// <summary>
		/// find must locate a present value and return end for a missing
		/// value
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Find_And_Contains()
		{
			set<int> s = new set<int>();
			s.insert(10);

			set<int>.iterator it = s.find(10);
			Assert.IsTrue(it != s.end());
			Assert.AreEqual(10, it.Value);

			Assert.IsTrue(s.find(20) == s.end());
			Assert.IsTrue(s.contains(10));
			Assert.IsFalse(s.contains(20));
			Assert.AreEqual(1UL, s.count(10));
			Assert.AreEqual(0UL, s.count(20));
		}



		/********************************************************************/
		/// <summary>
		/// Iterating from begin to end must visit the elements in
		/// ascending order
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Iteration_Is_Ordered()
		{
			set<int> s = new set<int>();

			foreach (int k in new[] { 5, 1, 4, 2, 3, 9, 7, 6, 8, 0 })
				s.insert(k);

			int expected = 0;

			for (set<int>.iterator it = s.begin(); it != s.end(); it++)
			{
				Assert.AreEqual(expected, it.Value);
				expected++;
			}

			Assert.AreEqual(10, expected);
		}



		/********************************************************************/
		/// <summary>
		/// A foreach loop must walk the elements in ascending order
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Foreach_Is_Ordered()
		{
			set<int> s = new set<int>([ 3, 1, 2 ]);

			List<int> values = new List<int>();

			foreach (int value in s)
				values.Add(value);

			CollectionAssert.AreEqual(new[] { 1, 2, 3 }, values);
		}



		/********************************************************************/
		/// <summary>
		/// Decrementing an iterator, including the end iterator, must move
		/// to the previous element
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Iterator_Decrement()
		{
			set<int> s = new set<int>([ 1, 2, 3 ]);

			set<int>.iterator it = s.end();
			it--;
			Assert.AreEqual(3, it.Value);
			it--;
			Assert.AreEqual(2, it.Value);
			it--;
			Assert.AreEqual(1, it.Value);
			Assert.IsTrue(it == s.begin());
			Assert.AreEqual(2, it.Next().Value);
			Assert.IsTrue(s.end().Prev() == s.find(3));
		}



		/********************************************************************/
		/// <summary>
		/// lower_bound, upper_bound and equal_range must return the
		/// expected positions
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Bounds()
		{
			set<int> s = new set<int>([ 10, 20, 30, 40 ]);

			Assert.AreEqual(20, s.lower_bound(20).Value);
			Assert.AreEqual(30, s.lower_bound(21).Value);
			Assert.AreEqual(30, s.upper_bound(20).Value);
			Assert.IsTrue(s.lower_bound(41) == s.end());

			pair<set<int>.iterator, set<int>.iterator> range = s.equal_range(20);
			Assert.AreEqual(20, range.first.Value);
			Assert.AreEqual(30, range.second.Value);

			// A missing value gives an empty range
			pair<set<int>.iterator, set<int>.iterator> empty = s.equal_range(25);
			Assert.IsTrue(empty.first == empty.second);
		}



		/********************************************************************/
		/// <summary>
		/// Erasing by value must remove the element and report the count
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Erase_By_Value()
		{
			set<int> s = new set<int>([ 1, 2, 3 ]);

			Assert.AreEqual(1UL, s.erase(2));
			Assert.AreEqual(0UL, s.erase(2));
			Assert.AreEqual(2UL, s.size());
			Assert.IsFalse(s.contains(2));
			Assert.IsTrue(s.contains(1));
			Assert.IsTrue(s.contains(3));
		}



		/********************************************************************/
		/// <summary>
		/// Erasing by iterator must remove the element and return an
		/// iterator to the following element
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Erase_By_Iterator()
		{
			set<int> s = new set<int>([ 1, 2, 3 ]);

			set<int>.iterator next = s.erase(s.find(2));

			Assert.AreEqual(3, next.Value);
			Assert.AreEqual(2UL, s.size());
			Assert.IsFalse(s.contains(2));

			// Erasing the end iterator must do nothing
			Assert.IsTrue(s.erase(s.end()) == s.end());
			Assert.AreEqual(2UL, s.size());
		}



		/********************************************************************/
		/// <summary>
		/// Erasing a range must remove all elements in the range
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Erase_Range()
		{
			set<int> s = new set<int>();

			for (int i = 0; i < 10; i++)
				s.insert(i);

			s.erase(s.find(3), s.find(7));

			Assert.AreEqual(6UL, s.size());
			Assert.IsTrue(s.contains(2));
			Assert.IsFalse(s.contains(3));
			Assert.IsFalse(s.contains(6));
			Assert.IsTrue(s.contains(7));
		}



		/********************************************************************/
		/// <summary>
		/// clear must remove all elements while keeping the container
		/// usable
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Clear()
		{
			set<int> s = new set<int>([ 1, 2 ]);

			s.clear();

			Assert.IsTrue(s.empty());
			Assert.AreEqual(0UL, s.size());

			s.insert(3);
			Assert.AreEqual(1UL, s.size());
			Assert.IsTrue(s.contains(3));
		}



		/********************************************************************/
		/// <summary>
		/// The copy constructor must create an independent copy
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Copy_Construction_Is_Independent()
		{
			set<int> original = new set<int>([ 1, 2 ]);

			set<int> copy = new set<int>(original);
			copy.insert(3);

			Assert.AreEqual(2UL, original.size());
			Assert.AreEqual(3UL, copy.size());
			Assert.IsFalse(original.contains(3));
		}



		/********************************************************************/
		/// <summary>
		/// CopyTo must replace the contents of the destination, and
		/// MakeDeepClone must give an independent copy
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_CopyTo_And_MakeDeepClone()
		{
			set<int> source = new set<int>([ 1, 2 ]);
			set<int> destination = new set<int>([ 7, 8, 9 ]);

			source.CopyTo(destination);

			Assert.IsTrue(source == destination);
			Assert.IsFalse(destination.contains(7));

			set<int> clone = source.MakeDeepClone();
			clone.insert(3);

			Assert.AreEqual(2UL, source.size());
			Assert.AreEqual(3UL, clone.size());
		}



		/********************************************************************/
		/// <summary>
		/// MoveFrom must hand the contents over and leave the container
		/// empty
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_MoveFrom_Leaves_Source_Empty()
		{
			set<int> source = new set<int>([ 1, 2, 3 ]);

			set<int> moved = Utility.move(source);

			Assert.AreEqual(3UL, moved.size());
			Assert.IsTrue(source.empty());
			Assert.IsTrue(source.begin() == source.end());

			// The emptied container must still be usable
			source.insert(4);
			Assert.AreEqual(1UL, source.size());
			Assert.AreEqual(3UL, moved.size());
		}



		/********************************************************************/
		/// <summary>
		/// swap must exchange the contents of the two containers
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Swap()
		{
			set<int> a = new set<int>([ 1 ]);
			set<int> b = new set<int>([ 2, 3 ]);

			a.swap(b);

			Assert.AreEqual(2UL, a.size());
			Assert.IsTrue(a.contains(2));
			Assert.IsTrue(a.contains(3));

			Assert.AreEqual(1UL, b.size());
			Assert.IsTrue(b.contains(1));
		}



		/********************************************************************/
		/// <summary>
		/// Two containers with the same contents must compare equal
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Equality()
		{
			set<int> a = new set<int>([ 1, 2 ]);
			set<int> b = new set<int>([ 2, 1 ]);

			Assert.IsTrue(a == b);
			Assert.AreEqual(a.GetHashCode(), b.GetHashCode());

			b.insert(3);
			Assert.IsTrue(a != b);
			Assert.IsTrue(a < b);
			Assert.IsTrue(a <= b);
			Assert.IsTrue(b > a);
			Assert.IsTrue(b >= a);
		}



		/********************************************************************/
		/// <summary>
		/// A custom comparer must control the ordering of the values, and
		/// must be given back by key_comp and value_comp
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Custom_Comparer()
		{
			IComparer<int> comparer = Comparer<int>.Create((x, y) => y.CompareTo(x));
			set<int> s = new set<int>(comparer);

			s.insert([ 1, 2, 3 ]);

			CollectionAssert.AreEqual(new[] { 3, 2, 1 }, s.ToList());
			Assert.AreSame(comparer, s.key_comp());
			Assert.AreSame(comparer, s.value_comp());
			Assert.AreEqual(2, s.lower_bound(2).Value);
			Assert.AreEqual(1, s.upper_bound(2).Value);
		}



		/********************************************************************/
		/// <summary>
		/// A large number of insertions and deletions must keep the
		/// container balanced and correct
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Stress_Insert_And_Erase()
		{
			set<int> s = new set<int>();
			const int n = 2000;

			// Insert in a scattered order
			for (int i = 0; i < n; i++)
				s.insert((i * 1103515245 + 12345) & 0x7fff);

			// The container must be sorted and consistent
			int prev = -1;
			int seen = 0;

			for (set<int>.iterator it = s.begin(); it != s.end(); it++)
			{
				Assert.IsTrue(it.Value > prev);
				prev = it.Value;
				seen++;
			}

			Assert.AreEqual((ulong)seen, s.size());

			// Erase every stored value and end up empty
			foreach (int value in s.ToList())
				Assert.AreEqual(1UL, s.erase(value));

			Assert.IsTrue(s.empty());
			Assert.IsTrue(s.begin() == s.end());
		}



		/********************************************************************/
		/// <summary>
		/// A value that only supports copying into an existing instance
		/// must be copied into a new instance of its own type, so that the
		/// container does not share the instance with the caller
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Operations_Use_CopyTo()
		{
			Copyable value = new Copyable(5);

			set<Copyable> s = new set<Copyable>();
			s.insert(value);

			set<Copyable> copy = new set<Copyable>(s);

			Copyable stored = s.begin().Value;
			Copyable copied = copy.begin().Value;

			Assert.IsFalse(ReferenceEquals(value, stored));
			Assert.IsFalse(ReferenceEquals(stored, copied));
			Assert.AreEqual(5, copied.Value);
		}



		/********************************************************************/
		/// <summary>
		/// A simple reference type that can copy itself into an existing
		/// instance, used to verify that the copying falls back to that
		/// way. It is ordered by its value, so that it can be stored in a
		/// set
		/// </summary>
		/********************************************************************/
		private sealed class Copyable : ICopyTo<Copyable>, IComparable<Copyable>
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

			public int CompareTo(Copyable? other)
			{
				return other == null ? 1 : Value.CompareTo(other.Value);
			}
		}
	}
}
