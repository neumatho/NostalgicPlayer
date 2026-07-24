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
	public class Iterator_ : TestStdBase
	{
		/********************************************************************/
		/// <summary>
		/// distance must return the number of elements between the two
		/// iterators
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Distance_Counts_Elements()
		{
			CPointer<int> data = new int[] { 1, 2, 3, 4, 5 };
			forward_iterator<int> begin = new forward_iterator<int>(data);
			forward_iterator<int> end = new forward_iterator<int>(data.End());

			Assert.AreEqual(5L, Iterator.distance(begin, end));
		}



		/********************************************************************/
		/// <summary>
		/// distance on an empty range must return zero
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Distance_Empty_Range_Returns_Zero()
		{
			CPointer<int> data = new int[] { 2, 4, 6 };
			forward_iterator<int> first = new forward_iterator<int>(data.Begin());

			Assert.AreEqual(0L, Iterator.distance(first, first));
		}



		/********************************************************************/
		/// <summary>
		/// distance must return a negative value when last comes before
		/// first
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Distance_Reversed_Returns_Negative()
		{
			CPointer<int> data = new int[] { 1, 2, 3, 4, 5 };
			forward_iterator<int> begin = new forward_iterator<int>(data);
			forward_iterator<int> end = new forward_iterator<int>(data.End());

			Assert.AreEqual(-5L, Iterator.distance(end, begin));
		}



		/********************************************************************/
		/// <summary>
		/// distance must work on the [begin(), end()) range of a vector
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Distance_On_Vector()
		{
			vector<int> v = new vector<int>(new[] { 10, 20, 30, 40 });

			Assert.AreEqual(4L, Iterator.distance(v.begin(), v.end()));
		}



		/********************************************************************/
		/// <summary>
		/// size must return the number of elements in the given array
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Size_Returns_Array_Length()
		{
			int[] data = new int[] { 1, 2, 3, 4, 5 };

			Assert.AreEqual(5UL, Iterator.size(data));
		}



		/********************************************************************/
		/// <summary>
		/// size must return zero for an empty array
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Size_Empty_Array_Returns_Zero()
		{
			Assert.AreEqual(0UL, Iterator.size(new int[0]));
		}



		/********************************************************************/
		/// <summary>
		/// size must return the number of elements in an array container
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Size_Returns_Array_Container_Length()
		{
			array<int> data = new array<int>(7);

			Assert.AreEqual(7UL, Iterator.size(data));
		}
	}
}
