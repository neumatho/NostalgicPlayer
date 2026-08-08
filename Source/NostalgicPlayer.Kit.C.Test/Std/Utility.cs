/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace NostalgicPlayer.Kit.C.Test.Std
{
	/// <summary>
	///
	/// </summary>
	[TestClass]
	public class Utility_ : TestStdBase
	{
		/********************************************************************/
		/// <summary>
		/// move must hand the contents of the container over to the
		/// returned one and leave the source empty
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Move_Transfers_The_Contents()
		{
			vector<int> source = new vector<int>([ 1, 2, 3 ]);

			vector<int> moved = Utility.move(source);

			Assert.AreEqual(3UL, moved.size());
			Assert.AreEqual(1, moved[0]);
			Assert.AreEqual(2, moved[1]);
			Assert.AreEqual(3, moved[2]);

			Assert.IsTrue(source.empty());
			Assert.AreEqual(0UL, source.size());
		}



		/********************************************************************/
		/// <summary>
		/// move must return a container that is independent of the source,
		/// which must still be usable after the move
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Moved_From_Container_Is_Still_Usable()
		{
			vector<int> source = new vector<int>([ 1, 2, 3 ]);

			vector<int> moved = Utility.move(source);

			source.push_back(7);

			Assert.AreEqual(1UL, source.size());
			Assert.AreEqual(7, source[0]);

			Assert.AreEqual(3UL, moved.size());
			Assert.AreEqual(1, moved[0]);
		}



		/********************************************************************/
		/// <summary>
		/// move must hand the elements over as they are, without making a
		/// copy of them like the copy operations do
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Move_Does_Not_Copy_The_Elements()
		{
			Cloneable element = new Cloneable(42);

			vector<Cloneable> source = new vector<Cloneable>();
			source.push_back(element);

			Cloneable stored = source[0];

			vector<Cloneable> moved = Utility.move(source);

			Assert.AreEqual(1UL, moved.size());
			Assert.IsTrue(ReferenceEquals(stored, moved[0]));
			Assert.AreEqual(42, moved[0].Value);
		}



		/********************************************************************/
		/// <summary>
		/// move must keep the buffer of the container, so that pointers
		/// into it stay valid and refer to the moved container
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Move_Keeps_The_Buffer()
		{
			vector<int> source = new vector<int>([ 4, 5, 6 ]);
			CPointer<int> pointer = source.data();

			vector<int> moved = Utility.move(source);

			Assert.IsTrue(pointer == moved.data());

			pointer[1] = 9;
			Assert.AreEqual(9, moved[1]);
		}



		/********************************************************************/
		/// <summary>
		/// move must hand the elements of the map over to the returned one
		/// and leave the source empty
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Move_Transfers_The_Elements_Of_A_Map()
		{
			map<int, string> source = new map<int, string>();
			source[1] = "one";
			source[2] = "two";

			map<int, string> moved = Utility.move(source);

			Assert.AreEqual(2UL, moved.size());
			Assert.AreEqual("one", moved[1]);
			Assert.AreEqual("two", moved[2]);

			Assert.IsTrue(source.empty());
			Assert.AreEqual(0UL, source.size());
			Assert.IsTrue(source.begin() == source.end());
		}



		/********************************************************************/
		/// <summary>
		/// A moved-from map must still be usable, and the two maps must be
		/// independent of each other
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Moved_From_Map_Is_Still_Usable()
		{
			map<int, string> source = new map<int, string>();
			source[1] = "one";

			map<int, string> moved = Utility.move(source);

			source[7] = "seven";

			Assert.AreEqual(1UL, source.size());
			Assert.AreEqual("seven", source[7]);

			Assert.AreEqual(1UL, moved.size());
			Assert.AreEqual("one", moved[1]);
		}



		/********************************************************************/
		/// <summary>
		/// move must hand the key ordering over as well, and the moved-from
		/// map must keep using it too
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Move_Keeps_The_Key_Ordering()
		{
			map<int, int> source = new map<int, int>(Comparer<int>.Create((x, y) => y.CompareTo(x)));

			foreach (int k in new[] { 1, 2, 3 })
				source[k] = k;

			map<int, int> moved = Utility.move(source);

			int[] movedKeys = new int[3];
			int i = 0;

			foreach (pair<int, int> kv in moved)
				movedKeys[i++] = kv.first;

			CollectionAssert.AreEqual(new[] { 3, 2, 1 }, movedKeys);

			foreach (int k in new[] { 4, 5 })
				source[k] = k;

			int[] sourceKeys = new int[2];
			i = 0;

			foreach (pair<int, int> kv in source)
				sourceKeys[i++] = kv.first;

			CollectionAssert.AreEqual(new[] { 5, 4 }, sourceKeys);
		}



		/********************************************************************/
		/// <summary>
		/// move on a type that does not support moving must return the
		/// value unchanged, just like a C++ move degrades to a copy
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Move_Without_Move_Support_Returns_The_Value()
		{
			int number = 5;
			Assert.AreEqual(5, Utility.move(number));
			Assert.AreEqual(5, number);

			string text = "hello";
			Assert.AreEqual("hello", Utility.move(text));
			Assert.AreEqual("hello", text);

			Cloneable element = new Cloneable(42);
			Assert.IsTrue(ReferenceEquals(element, Utility.move(element)));
		}



		/********************************************************************/
		/// <summary>
		/// move on a null reference must return null
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Move_Null_Returns_Null()
		{
			vector<int> source = null!;

			Assert.IsNull(Utility.move(source));
		}



		/********************************************************************/
		/// <summary>
		/// move on an empty container must return an empty container
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Move_Empty_Container()
		{
			vector<int> source = new vector<int>();

			vector<int> moved = Utility.move(source);

			Assert.IsTrue(moved.empty());
			Assert.IsTrue(source.empty());
		}



		/********************************************************************/
		/// <summary>
		/// A simple reference type that supports deep cloning, used to
		/// verify that the elements are not copied by a move
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
