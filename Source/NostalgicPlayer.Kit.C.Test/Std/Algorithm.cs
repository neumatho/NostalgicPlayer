/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.C.Std.Iterators;

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
	}
}
