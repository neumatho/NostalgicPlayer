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
	public class Map : TestStdBase
	{
		/********************************************************************/
		/// <summary>
		/// A default constructed map must be empty and have size zero
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Default_Construction_Is_Empty()
		{
			map<int, string> m = new map<int, string>();

			Assert.IsTrue(m.empty());
			Assert.AreEqual(0UL, m.size());
			Assert.IsTrue(m.begin() == m.end());
		}



		/********************************************************************/
		/// <summary>
		/// Constructing from an array of pairs must copy all the elements
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_From_Items()
		{
			map<int, string> m = new map<int, string>(new[]
			{
				new pair<int, string>(2, "two"),
				new pair<int, string>(1, "one"),
				new pair<int, string>(3, "three")
			});

			Assert.AreEqual(3UL, m.size());
			Assert.AreEqual("one", m[1]);
			Assert.AreEqual("two", m[2]);
			Assert.AreEqual("three", m[3]);
		}



		/********************************************************************/
		/// <summary>
		/// When the initializer holds duplicate keys, only the first one is
		/// kept
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_Keeps_First_Duplicate()
		{
			map<int, string> m = new map<int, string>(new[]
			{
				new pair<int, string>(1, "first"),
				new pair<int, string>(1, "second")
			});

			Assert.AreEqual(1UL, m.size());
			Assert.AreEqual("first", m[1]);
		}



		/********************************************************************/
		/// <summary>
		/// operator[] must insert a default value when the key is missing and
		/// return a reference that can be assigned through
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Index_Inserts_And_Assigns()
		{
			map<int, string> m = new map<int, string>();

			Assert.IsNull(m[5]);
			Assert.AreEqual(1UL, m.size());

			m[5] = "five";

			Assert.AreEqual("five", m[5]);
			Assert.AreEqual(1UL, m.size());
		}



		/********************************************************************/
		/// <summary>
		/// at must return the value for a present key and throw for a missing
		/// key
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_At_Access_And_Throw()
		{
			map<int, string> m = new map<int, string>();
			m[1] = "one";

			Assert.AreEqual("one", m.at(1));
			Assert.ThrowsExactly<out_of_range>(() => _ = m.at(2));
		}



		/********************************************************************/
		/// <summary>
		/// insert must add a new element and report success, and must not
		/// overwrite an existing key
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Insert_Does_Not_Overwrite()
		{
			map<int, string> m = new map<int, string>();

			pair<map<int, string>.iterator, bool> r1 = m.insert(new pair<int, string>(1, "one"));
			Assert.IsTrue(r1.second);
			Assert.AreEqual("one", r1.first.second);

			pair<map<int, string>.iterator, bool> r2 = m.insert(new pair<int, string>(1, "uno"));
			Assert.IsFalse(r2.second);
			Assert.AreEqual("one", m[1]);
		}



		/********************************************************************/
		/// <summary>
		/// insert_or_assign must overwrite the value of an existing key
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Insert_Or_Assign_Overwrites()
		{
			map<int, string> m = new map<int, string>();
			m.insert_or_assign(1, "one");

			pair<map<int, string>.iterator, bool> r = m.insert_or_assign(1, "uno");

			Assert.IsFalse(r.second);
			Assert.AreEqual("uno", m[1]);
		}



		/********************************************************************/
		/// <summary>
		/// The hint taking insert_or_assign must return a plain iterator to
		/// the element, so that Next() (C++ std::next) can advance past it
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Insert_Or_Assign_With_Hint()
		{
			map<int, string> m = new map<int, string>();

			map<int, string>.iterator hint = m.end();
			hint = m.insert_or_assign(hint, 1, "one");
			Assert.AreEqual(1, hint.first);
			Assert.AreEqual("one", hint.second);

			// Next() on the returned iterator must give the successor (end)
			map<int, string>.iterator next = m.insert_or_assign(hint, 2, "two").Next();
			Assert.IsTrue(next == m.end());

			// A second call with the same key must overwrite, not insert
			m.insert_or_assign(m.end(), 1, "uno");
			Assert.AreEqual(2UL, m.size());
			Assert.AreEqual("uno", m[1]);
		}



		/********************************************************************/
		/// <summary>
		/// emplace and try_emplace must insert only when the key is missing
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Emplace_And_Try_Emplace()
		{
			map<int, string> m = new map<int, string>();

			Assert.IsTrue(m.emplace(1, "one").second);
			Assert.IsFalse(m.emplace(1, "uno").second);

			Assert.IsTrue(m.try_emplace(2, "two").second);
			Assert.IsFalse(m.try_emplace(2, "dos").second);

			Assert.AreEqual("one", m[1]);
			Assert.AreEqual("two", m[2]);
		}



		/********************************************************************/
		/// <summary>
		/// find must locate a present key and return end for a missing key
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Find_And_Contains()
		{
			map<int, string> m = new map<int, string>();
			m[10] = "ten";

			map<int, string>.iterator it = m.find(10);
			Assert.IsTrue(it != m.end());
			Assert.AreEqual(10, it.first);
			Assert.AreEqual("ten", it.second);

			Assert.IsTrue(m.find(20) == m.end());
			Assert.IsTrue(m.contains(10));
			Assert.IsFalse(m.contains(20));
			Assert.AreEqual(1UL, m.count(10));
			Assert.AreEqual(0UL, m.count(20));
		}



		/********************************************************************/
		/// <summary>
		/// Iterating from begin to end must visit the elements in ascending
		/// key order
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Iteration_Is_Ordered()
		{
			map<int, int> m = new map<int, int>();

			foreach (int k in new[] { 5, 1, 4, 2, 3, 9, 7, 6, 8, 0 })
				m[k] = k * 10;

			int expected = 0;

			for (map<int, int>.iterator it = m.begin(); it != m.end(); it++)
			{
				Assert.AreEqual(expected, it.first);
				Assert.AreEqual(expected * 10, it.second);
				expected++;
			}

			Assert.AreEqual(10, expected);
		}



		/********************************************************************/
		/// <summary>
		/// A foreach loop must walk the elements in ascending key order
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Foreach_Is_Ordered()
		{
			map<int, int> m = new map<int, int>();

			foreach (int k in new[] { 3, 1, 2 })
				m[k] = k;

			List<int> keys = new List<int>();

			foreach (pair<int, int> kv in m)
				keys.Add(kv.first);

			CollectionAssert.AreEqual(new[] { 1, 2, 3 }, keys);
		}



		/********************************************************************/
		/// <summary>
		/// Decrementing an iterator, including the end iterator, must move to
		/// the previous element
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Iterator_Decrement()
		{
			map<int, int> m = new map<int, int>();
			m[1] = 1;
			m[2] = 2;
			m[3] = 3;

			map<int, int>.iterator it = m.end();
			it--;
			Assert.AreEqual(3, it.first);
			it--;
			Assert.AreEqual(2, it.first);
			it--;
			Assert.AreEqual(1, it.first);
			Assert.IsTrue(it == m.begin());
		}



		/********************************************************************/
		/// <summary>
		/// lower_bound, upper_bound and equal_range must return the expected
		/// positions
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Bounds()
		{
			map<int, int> m = new map<int, int>();

			foreach (int k in new[] { 10, 20, 30, 40 })
				m[k] = k;

			Assert.AreEqual(20, m.lower_bound(20).first);
			Assert.AreEqual(30, m.lower_bound(21).first);
			Assert.AreEqual(30, m.upper_bound(20).first);
			Assert.IsTrue(m.lower_bound(41) == m.end());

			pair<map<int, int>.iterator, map<int, int>.iterator> range = m.equal_range(20);
			Assert.AreEqual(20, range.first.first);
			Assert.AreEqual(30, range.second.first);
		}



		/********************************************************************/
		/// <summary>
		/// Erasing by key must remove the element and report the count
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Erase_By_Key()
		{
			map<int, int> m = new map<int, int>();
			m[1] = 1;
			m[2] = 2;
			m[3] = 3;

			Assert.AreEqual(1UL, m.erase(2));
			Assert.AreEqual(0UL, m.erase(2));
			Assert.AreEqual(2UL, m.size());
			Assert.IsFalse(m.contains(2));
			Assert.IsTrue(m.contains(1));
			Assert.IsTrue(m.contains(3));
		}



		/********************************************************************/
		/// <summary>
		/// Erasing by iterator must remove the element and return an iterator
		/// to the following element
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Erase_By_Iterator()
		{
			map<int, int> m = new map<int, int>();
			m[1] = 1;
			m[2] = 2;
			m[3] = 3;

			map<int, int>.iterator next = m.erase(m.find(2));

			Assert.AreEqual(3, next.first);
			Assert.AreEqual(2UL, m.size());
			Assert.IsFalse(m.contains(2));
		}



		/********************************************************************/
		/// <summary>
		/// Erasing a range must remove all elements in the range
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Erase_Range()
		{
			map<int, int> m = new map<int, int>();

			for (int i = 0; i < 10; i++)
				m[i] = i;

			m.erase(m.find(3), m.find(7));

			Assert.AreEqual(6UL, m.size());
			Assert.IsTrue(m.contains(2));
			Assert.IsFalse(m.contains(3));
			Assert.IsFalse(m.contains(6));
			Assert.IsTrue(m.contains(7));
		}



		/********************************************************************/
		/// <summary>
		/// clear must remove all elements while keeping the container usable
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Clear()
		{
			map<int, int> m = new map<int, int>();
			m[1] = 1;
			m[2] = 2;

			m.clear();

			Assert.IsTrue(m.empty());
			Assert.AreEqual(0UL, m.size());

			m[3] = 3;
			Assert.AreEqual(1UL, m.size());
			Assert.AreEqual(3, m[3]);
		}



		/********************************************************************/
		/// <summary>
		/// The copy constructor must create an independent copy
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Copy_Construction_Is_Independent()
		{
			map<int, int> original = new map<int, int>();
			original[1] = 10;
			original[2] = 20;

			map<int, int> copy = new map<int, int>(original);
			copy[1] = 99;
			copy[3] = 30;

			Assert.AreEqual(10, original[1]);
			Assert.AreEqual(99, copy[1]);
			Assert.IsFalse(original.contains(3));
			Assert.IsTrue(copy.contains(3));
		}



		/********************************************************************/
		/// <summary>
		/// swap must exchange the contents of the two containers
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Swap()
		{
			map<int, int> a = new map<int, int>();
			a[1] = 1;

			map<int, int> b = new map<int, int>();
			b[2] = 2;
			b[3] = 3;

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
			map<int, int> a = new map<int, int>();
			a[1] = 1;
			a[2] = 2;

			map<int, int> b = new map<int, int>();
			b[2] = 2;
			b[1] = 1;

			Assert.IsTrue(a == b);

			b[3] = 3;
			Assert.IsTrue(a != b);
			Assert.IsTrue(a < b);
		}



		/********************************************************************/
		/// <summary>
		/// A custom comparer must control the ordering of the keys
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Custom_Comparer()
		{
			map<int, int> m = new map<int, int>(Comparer<int>.Create((x, y) => y.CompareTo(x)));

			foreach (int k in new[] { 1, 2, 3 })
				m[k] = k;

			List<int> keys = new List<int>();

			foreach (pair<int, int> kv in m)
				keys.Add(kv.first);

			CollectionAssert.AreEqual(new[] { 3, 2, 1 }, keys);
		}



		/********************************************************************/
		/// <summary>
		/// A large number of insertions and deletions must keep the container
		/// balanced and correct
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Stress_Insert_And_Erase()
		{
			map<int, int> m = new map<int, int>();
			const int n = 2000;

			// Insert in a scattered order
			for (int i = 0; i < n; i++)
			{
				int key = (i * 1103515245 + 12345) & 0x7fff;
				m[key] = key;
			}

			// The container must be sorted and consistent
			int prev = -1;
			int seen = 0;

			for (map<int, int>.iterator it = m.begin(); it != m.end(); it++)
			{
				Assert.IsTrue(it.first > prev);
				Assert.AreEqual(it.first, it.second);
				prev = it.first;
				seen++;
			}

			Assert.AreEqual((ulong)seen, m.size());

			// Erase every stored key and end up empty
			List<int> keys = new List<int>();

			foreach (pair<int, int> kv in m)
				keys.Add(kv.first);

			foreach (int key in keys)
				Assert.AreEqual(1UL, m.erase(key));

			Assert.IsTrue(m.empty());
			Assert.IsTrue(m.begin() == m.end());
		}
	}
}
