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
	public class StdString_ : TestStdBase
	{
		#region Construction
		/********************************************************************/
		/// <summary>
		/// A default constructed container must be empty and null
		/// terminated
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Default_Construction_Is_Empty()
		{
			StdString s = new StdString();

			Assert.IsTrue(s.empty());
			Assert.AreEqual(0UL, s.size());
			Assert.AreEqual(0UL, s.length());
			Assert.IsTrue(s.begin() == s.end());
			Assert.AreEqual((uint8_t)0, s.c_str()[0]);
		}



		/********************************************************************/
		/// <summary>
		/// Constructing with a count and a character must fill the
		/// container with that character
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_With_Count_And_Character()
		{
			StdString s = new StdString((size_t)4, (uint8_t)'x');

			Assert.AreEqual(4UL, s.size());
			Assert.AreEqual("xxxx", Text(s));
		}



		/********************************************************************/
		/// <summary>
		/// Constructing from a null terminated character array must stop at
		/// the null character
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_From_C_String()
		{
			StdString s = new StdString(CStr("hello"));

			Assert.AreEqual(5UL, s.size());
			Assert.AreEqual("hello", Text(s));
		}



		/********************************************************************/
		/// <summary>
		/// Constructing from a character array and a count must keep the
		/// null characters that the array holds
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_From_Array_And_Count()
		{
			CPointer<uint8_t> source = new CPointer<uint8_t>(new uint8_t[] { (uint8_t)'a', 0, (uint8_t)'b' });

			StdString s = new StdString(source, (size_t)3);

			Assert.AreEqual(3UL, s.size());
			Assert.AreEqual((uint8_t)0, s[1]);
			Assert.AreEqual((uint8_t)'b', s[2]);
		}



		/********************************************************************/
		/// <summary>
		/// Constructing from a pointer range must copy the range
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_From_Range()
		{
			CPointer<uint8_t> source = CStr("abcdef");

			StdString s = new StdString(source + 1, source + 4);

			Assert.AreEqual("bcd", Text(s));
		}



		/********************************************************************/
		/// <summary>
		/// Constructing from a byte array must copy all the characters
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_From_Byte_Array()
		{
			StdString s = new StdString(Bytes("test"));

			Assert.AreEqual("test", Text(s));
		}



		/********************************************************************/
		/// <summary>
		/// The copy constructor must create an independent copy
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Copy_Construction_Is_Independent()
		{
			StdString original = Str("abc");
			StdString copy = new StdString(original);

			copy[0] = (uint8_t)'z';

			Assert.AreEqual("abc", Text(original));
			Assert.AreEqual("zbc", Text(copy));
		}



		/********************************************************************/
		/// <summary>
		/// Constructing from a part of another container must take the
		/// wanted characters only
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_From_Substring()
		{
			StdString original = Str("abcdef");

			Assert.AreEqual("cdef", Text(new StdString(original, (size_t)2)));
			Assert.AreEqual("cd", Text(new StdString(original, (size_t)2, (size_t)2)));
			Assert.AreEqual("cdef", Text(new StdString(original, (size_t)2, (size_t)100)));
			Assert.AreEqual(string.Empty, Text(new StdString(original, (size_t)6)));
		}



		/********************************************************************/
		/// <summary>
		/// Constructing from a position after the end must throw
		/// out_of_range
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_From_Substring_Out_Of_Range()
		{
			StdString original = Str("abc");

			Assert.ThrowsExactly<out_of_range>(() => new StdString(original, (size_t)4));
		}
		#endregion

		#region assign
		/********************************************************************/
		/// <summary>
		/// assign must replace the previous characters
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Assign_Replaces_Content()
		{
			StdString s = Str("original");

			s.assign((size_t)3, (uint8_t)'q');
			Assert.AreEqual("qqq", Text(s));

			s.assign(Str("other"));
			Assert.AreEqual("other", Text(s));

			s.assign(Str("abcdef"), (size_t)2, (size_t)3);
			Assert.AreEqual("cde", Text(s));

			s.assign(CStr("cstring"));
			Assert.AreEqual("cstring", Text(s));

			s.assign(CStr("cstring"), (size_t)2);
			Assert.AreEqual("cs", Text(s));

			s.assign(Bytes("items"));
			Assert.AreEqual("items", Text(s));

			CPointer<uint8_t> range = CStr("range");
			s.assign(range + 1, range + 3);
			Assert.AreEqual("an", Text(s));
		}



		/********************************************************************/
		/// <summary>
		/// Assigning a container to itself must leave it unchanged
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Assign_Self()
		{
			StdString s = Str("self");

			s.assign(s);

			Assert.AreEqual("self", Text(s));
		}



		/********************************************************************/
		/// <summary>
		/// assign must return the container itself, so that the calls can
		/// be chained
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Assign_Returns_Itself()
		{
			StdString s = new StdString();

			Assert.AreSame(s, s.assign(Str("abc")));
		}
		#endregion

		#region Element access
		/********************************************************************/
		/// <summary>
		/// The element access methods must give the wanted characters
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Element_Access()
		{
			StdString s = Str("abcd");

			Assert.AreEqual((uint8_t)'a', s.front());
			Assert.AreEqual((uint8_t)'d', s.back());
			Assert.AreEqual((uint8_t)'b', s[1]);
			Assert.AreEqual((uint8_t)'c', s.at((size_t)2));
			Assert.AreEqual((uint8_t)'c', s.at(2));
		}



		/********************************************************************/
		/// <summary>
		/// Writing through the indexer must change the character
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Indexer_Is_A_Reference()
		{
			StdString s = Str("abc");

			s[1] = (uint8_t)'X';

			Assert.AreEqual("aXc", Text(s));
		}



		/********************************************************************/
		/// <summary>
		/// The indexer must give the null character when the position is
		/// size()
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Indexer_At_Size_Is_Null_Character()
		{
			StdString s = Str("abc");

			Assert.AreEqual((uint8_t)0, s[s.size()]);
		}



		/********************************************************************/
		/// <summary>
		/// at must throw out_of_range when the position is not within the
		/// container
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_At_Out_Of_Range()
		{
			StdString s = Str("abc");

			Assert.ThrowsExactly<out_of_range>(() => s.at((size_t)3));
			Assert.ThrowsExactly<out_of_range>(() => s.at(-1));
		}



		/********************************************************************/
		/// <summary>
		/// data() and c_str() must give the same null terminated buffer
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Data_And_C_Str_Are_Null_Terminated()
		{
			StdString s = Str("abc");

			CPointer<uint8_t> data = s.data();
			CPointer<uint8_t> cStr = s.c_str();

			Assert.IsTrue(data == cStr);
			Assert.AreEqual((uint8_t)'a', data[0]);
			Assert.AreEqual((uint8_t)0, data[3]);
		}



		/********************************************************************/
		/// <summary>
		/// The null character must still be there after the container has
		/// been changed
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Null_Terminator_Is_Maintained()
		{
			StdString s = new StdString();

			for (int i = 0; i < 50; i++)
			{
				s.push_back((uint8_t)'a');
				Assert.AreEqual((uint8_t)0, s.c_str()[s.size()]);
			}

			s.erase((size_t)10);
			Assert.AreEqual((uint8_t)0, s.c_str()[10]);

			s.pop_back();
			Assert.AreEqual((uint8_t)0, s.c_str()[9]);

			s.clear();
			Assert.AreEqual((uint8_t)0, s.c_str()[0]);
		}
		#endregion

		#region Iterators
		/********************************************************************/
		/// <summary>
		/// The iterators must walk through all the characters
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Iterators()
		{
			StdString s = Str("abc");

			Assert.AreEqual(3, s.end() - s.begin());
			Assert.AreEqual((uint8_t)'a', s.begin()[0]);
			Assert.AreEqual((uint8_t)'c', s.rbegin()[0]);
			Assert.AreEqual(3, s.rend() - s.rbegin());
		}



		/********************************************************************/
		/// <summary>
		/// A foreach loop must give all the characters and be able to
		/// change them
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Foreach()
		{
			StdString s = Str("abc");
			string collected = string.Empty;

			foreach (uint8_t ch in s)
				collected += (char)ch;

			Assert.AreEqual("abc", collected);

			foreach (ref uint8_t ch in s)
				ch = (uint8_t)'z';

			Assert.AreEqual("zzz", Text(s));
		}
		#endregion

		#region Capacity
		/********************************************************************/
		/// <summary>
		/// reserve must grow the capacity without changing the characters,
		/// and never shrink it
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Reserve()
		{
			StdString s = Str("abc");

			s.reserve((size_t)100);

			Assert.IsTrue(s.capacity() >= 100UL);
			Assert.AreEqual(3UL, s.size());
			Assert.AreEqual("abc", Text(s));

			size_t before = s.capacity();
			s.reserve((size_t)1);

			Assert.AreEqual(before, s.capacity());
		}



		/********************************************************************/
		/// <summary>
		/// shrink_to_fit must make the capacity equal to the size
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Shrink_To_Fit()
		{
			StdString s = Str("abc");
			s.reserve((size_t)100);

			s.shrink_to_fit();

			Assert.AreEqual(3UL, s.capacity());
			Assert.AreEqual("abc", Text(s));
			Assert.AreEqual((uint8_t)0, s.c_str()[3]);
		}



		/********************************************************************/
		/// <summary>
		/// The capacity must never be smaller than the size
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Capacity_Grows_With_Size()
		{
			StdString s = new StdString();

			for (int i = 0; i < 200; i++)
			{
				s.push_back((uint8_t)'a');
				Assert.IsTrue(s.capacity() >= s.size());
			}

			Assert.AreEqual(200UL, s.size());
		}
		#endregion

		#region Modifiers
		/********************************************************************/
		/// <summary>
		/// clear must remove all the characters, but keep the capacity
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Clear()
		{
			StdString s = Str("abcdef");
			size_t before = s.capacity();

			s.clear();

			Assert.IsTrue(s.empty());
			Assert.AreEqual(0UL, s.size());
			Assert.AreEqual(before, s.capacity());
		}



		/********************************************************************/
		/// <summary>
		/// The index based insert must put the characters at the wanted
		/// position
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Insert_At_Index()
		{
			Assert.AreEqual("abXXcd", Text(Str("abcd").insert((size_t)2, (size_t)2, (uint8_t)'X')));
			Assert.AreEqual("abXcd", Text(Str("abcd").insert((size_t)2, CStr("X"))));
			Assert.AreEqual("abXYcd", Text(Str("abcd").insert((size_t)2, CStr("XYZ"), (size_t)2)));
			Assert.AreEqual("abXYcd", Text(Str("abcd").insert((size_t)2, Str("XY"))));
			Assert.AreEqual("abYZcd", Text(Str("abcd").insert((size_t)2, Str("XYZ"), (size_t)1)));
			Assert.AreEqual("abYcd", Text(Str("abcd").insert((size_t)2, Str("XYZ"), (size_t)1, (size_t)1)));
			Assert.AreEqual("abcdX", Text(Str("abcd").insert((size_t)4, CStr("X"))));
		}



		/********************************************************************/
		/// <summary>
		/// Inserting at a position after the end must throw out_of_range
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Insert_Out_Of_Range()
		{
			StdString s = Str("abc");

			Assert.ThrowsExactly<out_of_range>(() => s.insert((size_t)4, CStr("X")));
		}



		/********************************************************************/
		/// <summary>
		/// The pointer based insert must put the characters before the
		/// given position and return a pointer to the first of them
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Insert_At_Pointer()
		{
			StdString s = Str("abcd");

			CPointer<uint8_t> result = s.insert(s.data() + 2, (uint8_t)'X');

			Assert.AreEqual("abXcd", Text(s));
			Assert.AreEqual((uint8_t)'X', result[0]);

			s = Str("abcd");
			s.insert(s.data() + 1, (size_t)3, (uint8_t)'Y');
			Assert.AreEqual("aYYYbcd", Text(s));

			s = Str("abcd");
			CPointer<uint8_t> source = CStr("12345");
			s.insert(s.data() + 4, source + 1, source + 3);
			Assert.AreEqual("abcd23", Text(s));
		}



		/********************************************************************/
		/// <summary>
		/// The index based erase must remove the wanted characters
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Erase_At_Index()
		{
			Assert.AreEqual(string.Empty, Text(Str("abcdef").erase()));
			Assert.AreEqual("ab", Text(Str("abcdef").erase((size_t)2)));
			Assert.AreEqual("aef", Text(Str("abcdef").erase((size_t)1, (size_t)3)));
			Assert.AreEqual("abcdef", Text(Str("abcdef").erase((size_t)2, (size_t)0)));
			Assert.AreEqual("ab", Text(Str("abcdef").erase((size_t)2, (size_t)100)));

			StdString s = Str("abc");
			Assert.ThrowsExactly<out_of_range>(() => s.erase((size_t)4));
		}



		/********************************************************************/
		/// <summary>
		/// The pointer based erase must remove the wanted characters and
		/// return a pointer to the one that followed them
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Erase_At_Pointer()
		{
			StdString s = Str("abcdef");

			CPointer<uint8_t> result = s.erase(s.data() + 1);

			Assert.AreEqual("acdef", Text(s));
			Assert.AreEqual((uint8_t)'c', result[0]);

			s = Str("abcdef");
			result = s.erase(s.data() + 1, s.data() + 4);

			Assert.AreEqual("aef", Text(s));
			Assert.AreEqual((uint8_t)'e', result[0]);
		}



		/********************************************************************/
		/// <summary>
		/// push_back and pop_back must add and remove a character at the
		/// end
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Push_Back_And_Pop_Back()
		{
			StdString s = new StdString();

			s.push_back((uint8_t)'a');
			s.push_back((uint8_t)'b');

			Assert.AreEqual("ab", Text(s));

			s.pop_back();

			Assert.AreEqual("a", Text(s));
		}



		/********************************************************************/
		/// <summary>
		/// append must add the characters to the end
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Append()
		{
			Assert.AreEqual("abXXX", Text(Str("ab").append((size_t)3, (uint8_t)'X')));
			Assert.AreEqual("abcd", Text(Str("ab").append(Str("cd"))));
			Assert.AreEqual("abde", Text(Str("ab").append(Str("cde"), (size_t)1)));
			Assert.AreEqual("abd", Text(Str("ab").append(Str("cde"), (size_t)1, (size_t)1)));
			Assert.AreEqual("abcd", Text(Str("ab").append(CStr("cd"))));
			Assert.AreEqual("abc", Text(Str("ab").append(CStr("cde"), (size_t)1)));
			Assert.AreEqual("abcd", Text(Str("ab").append(Bytes("cd"))));

			CPointer<uint8_t> range = CStr("12345");
			Assert.AreEqual("ab23", Text(Str("ab").append(range + 1, range + 3)));
		}



		/********************************************************************/
		/// <summary>
		/// Appending a container to itself must work, even when the buffer
		/// has to grow
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Append_Self()
		{
			StdString s = Str("abc");
			s.shrink_to_fit();

			s.append(s);

			Assert.AreEqual("abcabc", Text(s));
		}



		/********************************************************************/
		/// <summary>
		/// replace must change the wanted range, no matter if the new
		/// characters are fewer or more than the old ones
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Replace()
		{
			Assert.AreEqual("aXYZef", Text(Str("abcdef").replace((size_t)1, (size_t)3, Str("XYZ"))));
			Assert.AreEqual("aXef", Text(Str("abcdef").replace((size_t)1, (size_t)3, Str("X"))));
			Assert.AreEqual("aXYZZZef", Text(Str("abcdef").replace((size_t)1, (size_t)3, Str("XYZZZ"))));
			Assert.AreEqual("aef", Text(Str("abcdef").replace((size_t)1, (size_t)3, Str(string.Empty))));
			Assert.AreEqual("aYef", Text(Str("abcdef").replace((size_t)1, (size_t)3, Str("XYZ"), (size_t)1, (size_t)1)));
			Assert.AreEqual("aYZef", Text(Str("abcdef").replace((size_t)1, (size_t)3, Str("XYZ"), (size_t)1)));
			Assert.AreEqual("aXYef", Text(Str("abcdef").replace((size_t)1, (size_t)3, CStr("XYZ"), (size_t)2)));
			Assert.AreEqual("aXYZef", Text(Str("abcdef").replace((size_t)1, (size_t)3, CStr("XYZ"))));
			Assert.AreEqual("aXXef", Text(Str("abcdef").replace((size_t)1, (size_t)3, (size_t)2, (uint8_t)'X')));
			Assert.AreEqual("abX", Text(Str("abcdef").replace((size_t)2, (size_t)100, CStr("X"))));
		}



		/********************************************************************/
		/// <summary>
		/// The pointer based replace must change the given range
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Replace_At_Pointer()
		{
			StdString s = Str("abcdef");
			s.replace(s.data() + 1, s.data() + 4, Str("XYZZ"));
			Assert.AreEqual("aXYZZef", Text(s));

			s = Str("abcdef");
			s.replace(s.data() + 1, s.data() + 4, CStr("X"));
			Assert.AreEqual("aXef", Text(s));

			s = Str("abcdef");
			s.replace(s.data() + 1, s.data() + 4, CStr("XYZ"), (size_t)2);
			Assert.AreEqual("aXYef", Text(s));

			s = Str("abcdef");
			s.replace(s.data() + 1, s.data() + 4, (size_t)4, (uint8_t)'X');
			Assert.AreEqual("aXXXXef", Text(s));
		}



		/********************************************************************/
		/// <summary>
		/// resize must grow the container with the wanted character and cut
		/// it when it is made smaller
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Resize()
		{
			StdString s = Str("abc");

			s.resize((size_t)5);
			Assert.AreEqual(5UL, s.size());
			Assert.AreEqual((uint8_t)0, s[4]);

			s = Str("abc");
			s.resize((size_t)5, (uint8_t)'x');
			Assert.AreEqual("abcxx", Text(s));

			s.resize((size_t)2);
			Assert.AreEqual("ab", Text(s));
			Assert.AreEqual((uint8_t)0, s.c_str()[2]);
		}



		/********************************************************************/
		/// <summary>
		/// swap must exchange the characters of the two containers
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Swap()
		{
			StdString s1 = Str("abc");
			StdString s2 = Str("123456");

			s1.swap(s2);

			Assert.AreEqual("123456", Text(s1));
			Assert.AreEqual("abc", Text(s2));
		}
		#endregion

		#region Operations
		/********************************************************************/
		/// <summary>
		/// compare must give the lexicographical order of the characters
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Compare()
		{
			Assert.AreEqual(0, Str("abc").compare(Str("abc")));
			Assert.IsTrue(Str("abc").compare(Str("abd")) < 0);
			Assert.IsTrue(Str("abd").compare(Str("abc")) > 0);
			Assert.IsTrue(Str("abc").compare(Str("abcd")) < 0);
			Assert.IsTrue(Str("abcd").compare(Str("abc")) > 0);

			Assert.AreEqual(0, Str("abc").compare(CStr("abc")));
			Assert.AreEqual(0, Str("xabcx").compare((size_t)1, (size_t)3, Str("abc")));
			Assert.AreEqual(0, Str("xabcx").compare((size_t)1, (size_t)3, CStr("abc")));
			Assert.AreEqual(0, Str("xabcx").compare((size_t)1, (size_t)3, CStr("abcd"), (size_t)3));
			Assert.AreEqual(0, Str("xabcx").compare((size_t)1, (size_t)3, Str("yabcy"), (size_t)1, (size_t)3));
			Assert.AreEqual(0, Str("xabc").compare((size_t)1, (size_t)100, Str("yabc"), (size_t)1));
		}



		/********************************************************************/
		/// <summary>
		/// A character must be compared as an unsigned byte
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Compare_Uses_Unsigned_Characters()
		{
			StdString low = new StdString(new uint8_t[] { 0x20 });
			StdString high = new StdString(new uint8_t[] { 0x80 });

			Assert.IsTrue(low.compare(high) < 0);
			Assert.IsTrue(high.compare(low) > 0);
			Assert.IsTrue(low < high);
		}



		/********************************************************************/
		/// <summary>
		/// starts_with, ends_with and contains must check the characters at
		/// the beginning, at the end and anywhere
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Starts_Ends_Contains()
		{
			StdString s = Str("hello world");

			Assert.IsTrue(s.starts_with(Str("hello")));
			Assert.IsTrue(s.starts_with(CStr("hello")));
			Assert.IsTrue(s.starts_with((uint8_t)'h'));
			Assert.IsFalse(s.starts_with(Str("world")));

			Assert.IsTrue(s.ends_with(Str("world")));
			Assert.IsTrue(s.ends_with(CStr("world")));
			Assert.IsTrue(s.ends_with((uint8_t)'d'));
			Assert.IsFalse(s.ends_with(Str("hello")));

			Assert.IsTrue(s.contains(Str("lo wo")));
			Assert.IsTrue(s.contains(CStr("lo wo")));
			Assert.IsTrue(s.contains((uint8_t)'w'));
			Assert.IsFalse(s.contains(Str("xyz")));
		}



		/********************************************************************/
		/// <summary>
		/// substr must give an independent container with the wanted
		/// characters
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Substr()
		{
			StdString s = Str("abcdef");

			Assert.AreEqual("abcdef", Text(s.substr()));
			Assert.AreEqual("cdef", Text(s.substr((size_t)2)));
			Assert.AreEqual("cd", Text(s.substr((size_t)2, (size_t)2)));
			Assert.AreEqual("cdef", Text(s.substr((size_t)2, (size_t)100)));
			Assert.AreEqual(string.Empty, Text(s.substr((size_t)6)));

			Assert.ThrowsExactly<out_of_range>(() => s.substr((size_t)7));

			StdString part = s.substr((size_t)2, (size_t)2);
			part[0] = (uint8_t)'X';
			Assert.AreEqual("abcdef", Text(s));
		}



		/********************************************************************/
		/// <summary>
		/// copy must move the wanted characters to the given array without
		/// null terminating them
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Copy()
		{
			StdString s = Str("abcdef");
			CPointer<uint8_t> destination = new CPointer<uint8_t>(new uint8_t[8]);

			size_t copied = s.copy(destination, (size_t)3, (size_t)2);

			Assert.AreEqual(3UL, copied);
			Assert.AreEqual((uint8_t)'c', destination[0]);
			Assert.AreEqual((uint8_t)'e', destination[2]);
			Assert.AreEqual((uint8_t)0, destination[3]);

			copied = s.copy(destination, (size_t)100);

			Assert.AreEqual(6UL, copied);
			Assert.AreEqual((uint8_t)'a', destination[0]);
		}
		#endregion

		#region Search
		/********************************************************************/
		/// <summary>
		/// find must give the position of the first occurrence
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Find()
		{
			StdString s = Str("abcabc");

			Assert.AreEqual(0UL, s.find(Str("abc")));
			Assert.AreEqual(3UL, s.find(Str("abc"), (size_t)1));
			Assert.AreEqual(StdString.npos, s.find(Str("abc"), (size_t)4));
			Assert.AreEqual(StdString.npos, s.find(Str("xyz")));

			Assert.AreEqual(1UL, s.find(CStr("bc")));
			Assert.AreEqual(4UL, s.find(CStr("bc"), (size_t)2));
			Assert.AreEqual(1UL, s.find(CStr("bcx"), (size_t)0, (size_t)2));

			Assert.AreEqual(2UL, s.find((uint8_t)'c'));
			Assert.AreEqual(5UL, s.find((uint8_t)'c', (size_t)3));
			Assert.AreEqual(StdString.npos, s.find((uint8_t)'z'));
		}



		/********************************************************************/
		/// <summary>
		/// Searching for no characters at all must give the position that
		/// the search began at, as in C++
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Find_Empty()
		{
			StdString s = Str("abc");
			StdString empty = new StdString();

			Assert.AreEqual(0UL, s.find(empty));
			Assert.AreEqual(2UL, s.find(empty, (size_t)2));
			Assert.AreEqual(3UL, s.find(empty, (size_t)3));
			Assert.AreEqual(StdString.npos, s.find(empty, (size_t)4));

			Assert.AreEqual(3UL, s.rfind(empty));
			Assert.AreEqual(2UL, s.rfind(empty, (size_t)2));
		}



		/********************************************************************/
		/// <summary>
		/// rfind must give the position of the last occurrence that begins
		/// at or before the given position
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_RFind()
		{
			StdString s = Str("abcabc");

			Assert.AreEqual(3UL, s.rfind(Str("abc")));
			Assert.AreEqual(0UL, s.rfind(Str("abc"), (size_t)2));
			Assert.AreEqual(0UL, s.rfind(Str("abc"), (size_t)0));
			Assert.AreEqual(StdString.npos, s.rfind(Str("xyz")));

			Assert.AreEqual(4UL, s.rfind(CStr("bc")));
			Assert.AreEqual(1UL, s.rfind(CStr("bc"), (size_t)3));
			Assert.AreEqual(4UL, s.rfind(CStr("bcx"), StdString.npos, (size_t)2));

			Assert.AreEqual(5UL, s.rfind((uint8_t)'c'));
			Assert.AreEqual(2UL, s.rfind((uint8_t)'c', (size_t)4));
			Assert.AreEqual(StdString.npos, s.rfind((uint8_t)'z'));
		}



		/********************************************************************/
		/// <summary>
		/// find_first_of and find_last_of must give the position of a
		/// character that is one of the given ones
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Find_Of()
		{
			StdString s = Str("hello world");

			Assert.AreEqual(2UL, s.find_first_of(Str("lo")));
			Assert.AreEqual(4UL, s.find_first_of(Str("lo"), (size_t)4));
			Assert.AreEqual(2UL, s.find_first_of(CStr("lo")));
			Assert.AreEqual(2UL, s.find_first_of(CStr("lox"), (size_t)0, (size_t)2));
			Assert.AreEqual(2UL, s.find_first_of((uint8_t)'l'));
			Assert.AreEqual(StdString.npos, s.find_first_of(Str("xyz")));

			Assert.AreEqual(9UL, s.find_last_of(Str("lo")));
			Assert.AreEqual(4UL, s.find_last_of(Str("lo"), (size_t)6));
			Assert.AreEqual(9UL, s.find_last_of(CStr("lo")));
			Assert.AreEqual(9UL, s.find_last_of((uint8_t)'l'));
			Assert.AreEqual(StdString.npos, s.find_last_of(Str("xyz")));
		}



		/********************************************************************/
		/// <summary>
		/// find_first_not_of and find_last_not_of must give the position of
		/// a character that is not one of the given ones
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Find_Not_Of()
		{
			StdString s = Str("aaabaaa");

			Assert.AreEqual(3UL, s.find_first_not_of(Str("a")));
			Assert.AreEqual(3UL, s.find_first_not_of(CStr("a")));
			Assert.AreEqual(3UL, s.find_first_not_of((uint8_t)'a'));
			Assert.AreEqual(StdString.npos, s.find_first_not_of(Str("ab")));

			Assert.AreEqual(3UL, s.find_last_not_of(Str("a")));
			Assert.AreEqual(3UL, s.find_last_not_of(CStr("a")));
			Assert.AreEqual(3UL, s.find_last_not_of((uint8_t)'a'));
			Assert.AreEqual(StdString.npos, s.find_last_not_of(Str("ab")));

			Assert.AreEqual(0UL, s.find_first_not_of(Str("xyz")));
			Assert.AreEqual(6UL, s.find_last_not_of(Str("xyz")));
		}



		/********************************************************************/
		/// <summary>
		/// Searching in an empty container must always give npos, except
		/// when nothing is searched for
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Search_In_Empty()
		{
			StdString s = new StdString();

			Assert.AreEqual(StdString.npos, s.find(Str("a")));
			Assert.AreEqual(StdString.npos, s.rfind(Str("a")));
			Assert.AreEqual(StdString.npos, s.find_first_of(Str("a")));
			Assert.AreEqual(StdString.npos, s.find_last_of(Str("a")));
			Assert.AreEqual(StdString.npos, s.find_first_not_of(Str("a")));
			Assert.AreEqual(StdString.npos, s.find_last_not_of(Str("a")));
			Assert.AreEqual(0UL, s.find(new StdString()));
		}
		#endregion

		#region Operators
		/********************************************************************/
		/// <summary>
		/// The + operator must give a new container holding the characters
		/// of both operands
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Concatenation()
		{
			StdString left = Str("ab");
			StdString right = Str("cd");

			Assert.AreEqual("abcd", Text(left + right));
			Assert.AreEqual("abcd", Text(left + CStr("cd")));
			Assert.AreEqual("cdab", Text(CStr("cd") + left));
			Assert.AreEqual("abX", Text(left + (uint8_t)'X'));
			Assert.AreEqual("Xab", Text((uint8_t)'X' + left));

			Assert.AreEqual("ab", Text(left));
			Assert.AreEqual("cd", Text(right));
		}



		/********************************************************************/
		/// <summary>
		/// C# must derive the += operator from the + operator
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Concatenation_Assignment()
		{
			StdString s = Str("ab");

			s += Str("cd");

			Assert.AreEqual("abcd", Text(s));
		}



		/********************************************************************/
		/// <summary>
		/// The comparison operators must compare the characters
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Comparison_Operators()
		{
			Assert.IsTrue(Str("abc") == Str("abc"));
			Assert.IsFalse(Str("abc") == Str("abd"));
			Assert.IsTrue(Str("abc") != Str("abd"));

			Assert.IsTrue(Str("abc") < Str("abd"));
			Assert.IsTrue(Str("abc") <= Str("abc"));
			Assert.IsTrue(Str("abd") > Str("abc"));
			Assert.IsTrue(Str("abc") >= Str("abc"));

			Assert.IsTrue(Str("abc") < Str("abcd"));
			Assert.IsFalse(Str("abcd") < Str("abc"));
		}



		/********************************************************************/
		/// <summary>
		/// Comparing against null must work and never throw
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Comparison_With_Null()
		{
			StdString s = Str("abc");
			StdString? nothing = null;

			Assert.IsFalse(s == nothing);
			Assert.IsTrue(s != nothing);
			Assert.IsTrue(nothing == null);
		}



		/********************************************************************/
		/// <summary>
		/// Two containers holding the same characters must be equal and
		/// give the same hash code
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Equals_And_Hash_Code()
		{
			StdString s1 = Str("abc");
			StdString s2 = Str("abc");
			StdString s3 = Str("abd");

			Assert.IsTrue(s1.Equals(s2));
			Assert.IsFalse(s1.Equals(s3));
			Assert.IsFalse(s1.Equals((object?)null));
			Assert.AreEqual(s1.GetHashCode(), s2.GetHashCode());
		}
		#endregion

		#region Cloning and moving
		/********************************************************************/
		/// <summary>
		/// MakeDeepClone must give an independent copy
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Make_Deep_Clone()
		{
			StdString original = Str("abc");
			StdString clone = ((IDeepCloneable<StdString>)original).MakeDeepClone();

			clone[0] = (uint8_t)'z';

			Assert.AreEqual("abc", Text(original));
			Assert.AreEqual("zbc", Text(clone));
		}



		/********************************************************************/
		/// <summary>
		/// CopyTo must replace the characters of the destination with an
		/// independent copy
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Copy_To()
		{
			StdString source = Str("abc");
			StdString destination = Str("123456");

			source.CopyTo(destination);

			Assert.AreEqual("abc", Text(destination));

			destination[0] = (uint8_t)'z';
			Assert.AreEqual("abc", Text(source));
		}



		/********************************************************************/
		/// <summary>
		/// A move must hand the characters over and leave the source empty
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Move()
		{
			StdString source = Str("abc");

			StdString moved = Utility.move(source);

			Assert.AreEqual("abc", Text(moved));
			Assert.IsTrue(source.empty());
			Assert.AreEqual(0UL, source.size());
			Assert.AreEqual((uint8_t)0, source.c_str()[0]);

			source.push_back((uint8_t)'x');
			Assert.AreEqual("x", Text(source));
			Assert.AreEqual("abc", Text(moved));
		}
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Returns a container holding the characters of the given text,
		/// one byte for each character
		/// </summary>
		/********************************************************************/
		private static StdString Str(string text)
		{
			return new StdString(Bytes(text));
		}



		/********************************************************************/
		/// <summary>
		/// Returns the characters of the given text as a byte array
		/// </summary>
		/********************************************************************/
		private static uint8_t[] Bytes(string text)
		{
			uint8_t[] bytes = new uint8_t[text.Length];

			for (int i = 0; i < text.Length; i++)
				bytes[i] = (uint8_t)text[i];

			return bytes;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the characters of the given text as a null terminated
		/// character array
		/// </summary>
		/********************************************************************/
		private static CPointer<uint8_t> CStr(string text)
		{
			uint8_t[] bytes = new uint8_t[text.Length + 1];

			for (int i = 0; i < text.Length; i++)
				bytes[i] = (uint8_t)text[i];

			return new CPointer<uint8_t>(bytes);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the characters of the given container as a text, one
		/// character for each byte
		/// </summary>
		/********************************************************************/
		private static string Text(StdString str)
		{
			char[] chars = new char[str.size()];

			for (size_t i = 0; i < str.size(); i++)
				chars[i] = (char)str[i];

			return new string(chars);
		}
		#endregion
	}
}
