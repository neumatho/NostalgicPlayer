/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.C.Std.Containers;
using Polycode.NostalgicPlayer.Kit.C.Std.Exceptions;

namespace NostalgicPlayer.Kit.C.Test.Std
{
	/// <summary>
	///
	/// </summary>
	[TestClass]
	public class Variant : TestStdBase
	{
		/********************************************************************/
		/// <summary>
		/// A variant constructed from a value must report the correct index
		/// and hold the corresponding alternative
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_Holds_First_Alternative()
		{
			variant<int, string> v = new variant<int, string>(42);

			Assert.AreEqual(0UL, v.index());
			Assert.IsTrue(v.holds_alternative<int>());
			Assert.IsFalse(v.holds_alternative<string>());
			Assert.AreEqual(42, v.get<int>());
		}



		/********************************************************************/
		/// <summary>
		/// A variant constructed from the second alternative must report
		/// index 1
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Construction_Holds_Second_Alternative()
		{
			variant<int, string> v = new variant<int, string>("hello");

			Assert.AreEqual(1UL, v.index());
			Assert.IsTrue(v.holds_alternative<string>());
			Assert.AreEqual("hello", v.get<string>());
		}



		/********************************************************************/
		/// <summary>
		/// Implicit conversion must select the matching alternative, both on
		/// construction and on reassignment
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Implicit_Conversion()
		{
			variant<int, string> v = 7;
			Assert.AreEqual(0UL, v.index());
			Assert.AreEqual(7, v.get<int>());

			v = "world";
			Assert.AreEqual(1UL, v.index());
			Assert.AreEqual("world", v.get<string>());
		}



		/********************************************************************/
		/// <summary>
		/// get with the wrong alternative type must throw bad_variant_access
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Get_Wrong_Type_Throws()
		{
			variant<int, string> v = 5;

			bool exceptionThrown = false;

			try
			{
				v.get<string>();
			}
			catch (bad_variant_access)
			{
				exceptionThrown = true;
			}

			Assert.IsTrue(exceptionThrown);
		}



		/********************************************************************/
		/// <summary>
		/// get_if must return the value when the alternative is held, and
		/// fail without throwing otherwise
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Get_If()
		{
			variant<int, string> v = "text";

			Assert.IsTrue(v.get_if<string>(out string s));
			Assert.AreEqual("text", s);

			Assert.IsFalse(v.get_if<int>(out int i));
			Assert.AreEqual(0, i);
		}



		/********************************************************************/
		/// <summary>
		/// emplace must replace the held value and switch the active
		/// alternative
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Emplace()
		{
			variant<int, string> v = 1;

			string result = v.emplace<string>("replaced");

			Assert.AreEqual("replaced", result);
			Assert.AreEqual(1UL, v.index());
			Assert.AreEqual("replaced", v.get<string>());
		}



		/********************************************************************/
		/// <summary>
		/// emplace with a type that is not one of the alternatives must
		/// throw bad_variant_access
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Emplace_Invalid_Type_Throws()
		{
			variant<int, string> v = 1;

			bool exceptionThrown = false;

			try
			{
				v.emplace<double>(2.0);
			}
			catch (bad_variant_access)
			{
				exceptionThrown = true;
			}

			Assert.IsTrue(exceptionThrown);
		}



		/********************************************************************/
		/// <summary>
		/// The value returning visit must invoke the function that matches
		/// the held alternative
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Visit_With_Result()
		{
			variant<int, string> v = 10;
			Assert.AreEqual("int:10", v.visit(i => $"int:{i}", s => $"str:{s}"));

			v = "abc";
			Assert.AreEqual("str:abc", v.visit(i => $"int:{i}", s => $"str:{s}"));
		}



		/********************************************************************/
		/// <summary>
		/// The void visit must invoke the action that matches the held
		/// alternative
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Visit_Void()
		{
			variant<int, string> v = "only";

			int intCalls = 0;
			string seenString = null!;

			v.visit(_ => intCalls++, s => seenString = s);

			Assert.AreEqual(0, intCalls);
			Assert.AreEqual("only", seenString);
		}



		/********************************************************************/
		/// <summary>
		/// swap must exchange the contents of two variants
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Swap()
		{
			variant<int, string> a = 99;
			variant<int, string> b = "swapped";

			a.swap(ref b);

			Assert.AreEqual(1UL, a.index());
			Assert.AreEqual("swapped", a.get<string>());
			Assert.AreEqual(0UL, b.index());
			Assert.AreEqual(99, b.get<int>());
		}



		/********************************************************************/
		/// <summary>
		/// Two variants are equal only when they hold the same alternative
		/// with equal values
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Equality()
		{
			variant<int, string> a = 1;
			variant<int, string> b = 1;
			variant<int, string> c = 2;
			variant<int, string> d = "1";

			Assert.IsTrue(a == b);
			Assert.IsTrue(a.Equals(b));
			Assert.AreEqual(a.GetHashCode(), b.GetHashCode());

			Assert.IsTrue(a != c);
			Assert.IsFalse(a == c);

			// Same textual value but a different alternative must not be equal
			Assert.IsTrue(a != d);
		}



		/********************************************************************/
		/// <summary>
		/// Being a reference type, copying a variant shares the same instance,
		/// so mutating the copy also affects the original
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Reference_Semantics()
		{
			variant<int, string> original = 5;
			variant<int, string> copy = original;

			copy.emplace<string>("changed");

			Assert.IsTrue(ReferenceEquals(original, copy));
			Assert.AreEqual(1UL, original.index());
			Assert.AreEqual("changed", original.get<string>());
		}



		/********************************************************************/
		/// <summary>
		/// An empty (parameterless) constructed variant reports its first
		/// alternative and is not valueless
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Empty_Is_Not_Valueless()
		{
			variant<monostate, int> v = new variant<monostate, int>();

			Assert.AreEqual(0UL, v.index());
			Assert.IsTrue(v.holds_alternative<monostate>());
			Assert.IsFalse(v.valueless_by_exception());
		}



		/********************************************************************/
		/// <summary>
		/// variant_npos must be the largest possible size_t value
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Variant_Npos()
		{
			size_t npos = variant.variant_npos;

			Assert.AreEqual(ulong.MaxValue, npos);
		}



		/********************************************************************/
		/// <summary>
		/// The three alternative variant must dispatch to the correct
		/// alternative
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Three_Alternatives()
		{
			variant<int, string, double> v = 3.5;

			Assert.AreEqual(2UL, v.index());
			Assert.IsTrue(v.holds_alternative<double>());
			Assert.AreEqual(3.5, v.get<double>());
			Assert.AreEqual("d", v.visit(i => "i", s => "s", d => "d"));
		}



		/********************************************************************/
		/// <summary>
		/// The four alternative variant must dispatch to the correct
		/// alternative
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Four_Alternatives()
		{
			variant<int, string, double, bool> v = true;

			Assert.AreEqual(3UL, v.index());
			Assert.IsTrue(v.holds_alternative<bool>());
			Assert.IsTrue(v.get<bool>());
			Assert.AreEqual("b", v.visit(i => "i", s => "s", d => "d", b => "b"));
		}
	}
}
