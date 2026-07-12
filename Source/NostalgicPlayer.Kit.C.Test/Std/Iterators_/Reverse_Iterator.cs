/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.C.Std.Iterators;

namespace NostalgicPlayer.Kit.C.Test.Std.Iterators_
{
	/// <summary>
	///
	/// </summary>
	[TestClass]
	public class Reverse_Iterator_ : TestStdBase
	{
		/********************************************************************/
		/// <summary>
		/// rbegin() must refer to the last element of the container and its
		/// base() must point just after it
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_RBegin_Refers_To_Last_Element()
		{
			vector<int> v = new vector<int>(new[] { 10, 20, 30 });

			reverse_iterator<int> rit = v.rbegin();

			Assert.AreEqual(30, rit[0]);
			Assert.IsTrue(rit.@base() == v.end());
		}



		/********************************************************************/
		/// <summary>
		/// Iterating from rbegin() to rend() must visit the elements in
		/// reverse order
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Reverse_Iteration_Order()
		{
			vector<int> v = new vector<int>(new[] { 1, 2, 3, 4 });

			int[] visited = new int[4];
			int i = 0;

			for (reverse_iterator<int> it = v.rbegin(); it != v.rend(); it++)
				visited[i++] = it[0];

			CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, visited);
		}



		/********************************************************************/
		/// <summary>
		/// find_if on a reverse range must find the last matching element,
		/// and base() must yield the trimmed length (the GetLengthTailTrimmed
		/// pattern)
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Find_If_Reverse_Trimmed_Length()
		{
			// Three real values followed by two trailing -1 markers
			vector<int> v = new vector<int>(new[] { 5, 7, 9, -1, -1 });

			reverse_iterator<int> last = Algorithm.find_if(v.rbegin(), v.rend(), (int x) => x != -1);

			Assert.AreEqual(9, last[0]);
			Assert.AreEqual(3L, Iterator.distance(v.begin(), last.@base()));
		}



		/********************************************************************/
		/// <summary>
		/// When no element matches, reverse find_if must return rend(), whose
		/// base() equals begin() so the trimmed length is zero
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Find_If_Reverse_No_Match()
		{
			vector<int> v = new vector<int>(new[] { -1, -1, -1 });

			reverse_iterator<int> last = Algorithm.find_if(v.rbegin(), v.rend(), (int x) => x != -1);

			Assert.IsTrue(last == v.rend());
			Assert.AreEqual(0L, Iterator.distance(v.begin(), last.@base()));
		}
	}
}
