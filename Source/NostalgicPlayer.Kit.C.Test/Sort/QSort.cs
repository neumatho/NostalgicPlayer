/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using Polycode.NostalgicPlayer.Kit.C;

namespace NostalgicPlayer.Kit.C.Test.Sort
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class QSort : TestSortBase
	{
		private class Item
		{
			public c_int Id { get; set; }
			public c_int Score { get; set; }
		}

		private c_int compareCallCounter;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Empty_Array_No_Calls()
		{
			c_int[] a = [];
			compareCallCounter = 0;

			CSort.qsort<c_int>(a, 0, Cmp_Int_Counted);

			Assert.AreEqual(0, compareCallCounter);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Single_Element()
		{
			c_int[] a = [ 42 ];
			compareCallCounter = 0;

			CSort.qsort<c_int>(a, 1, Cmp_Int_Counted);

			Assert.AreEqual(42, a[0]);
			Assert.AreEqual(0, compareCallCounter);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Sorted_Array()
		{
			c_int[] a = [ 1, 2, 3, 4, 5 ];
			c_int[] exp = [ 1, 2, 3, 4, 5 ];

			CSort.qsort<c_int>(a, 5, Cmp);

			Assert.IsTrue(Arrays_Equal(a, exp));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Reverse_Array()
		{
			c_int[] a = [ 5, 4, 3, 2, 1 ];
			c_int[] exp = [ 1, 2, 3, 4, 5 ];

			CSort.qsort<c_int>(a, 5, Cmp);

			Assert.IsTrue(Arrays_Equal(a, exp));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Duplicates()
		{
			c_int[] a = [ 5, 1, 3, 3, 1, 5, 2 ];
			c_int[] exp = [ 1, 1, 2, 3, 3, 5, 5 ];

			CSort.qsort<c_int>(a, 7, Cmp);

			Assert.IsTrue(Arrays_Equal(a, exp));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_All_Equal()
		{
			c_int[] a = [ 7, 7, 7, 7 ];
			c_int[] exp = [ 7, 7, 7, 7 ];

			CSort.qsort<c_int>(a, 4, Cmp);

			Assert.IsTrue(Arrays_Equal(a, exp));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Large_Values()
		{
			c_int[] a = [ c_int.MaxValue, c_int.MinValue, 0, -1, 1 ];
			c_int[] exp = [ c_int.MinValue, -1, 0, 1, c_int.MaxValue ];

			CSort.qsort<c_int>(a, 5, Cmp);

			Assert.IsTrue(Arrays_Equal(a, exp));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Chars()
		{
			char[] a = [ 'z', 'a', 'm', 'a' ];
			char[] exp = [ 'a', 'a', 'm', 'z' ];

			CSort.qsort<char>(a, 4, Cmp);

			Assert.IsTrue(Arrays_Equal(a, exp));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Class_Sort()
		{
			Item[] a =
			[
				new Item { Id = 1, Score = 50 },
				new Item { Id = 2, Score = 10 },
				new Item { Id = 3, Score = 30 }
			];

			CSort.qsort<Item>(a, 3, Cmp_Item);

			Assert.AreEqual(10, a[0].Score);
			Assert.AreEqual(30, a[1].Score);
			Assert.AreEqual(50, a[2].Score);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Arrays_Equal<T>(T[] a, T[] exp) where T : INumber<T>
		{
			for (c_int i = 0; i < exp.Length; i++)
			{
				if (a[i] != exp[i])
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Cmp<T>(T a, T b) where T : INumber<T>
		{
			return (a > b ? 1 : 0) - (a < b ? 1 : 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Cmp_Int_Counted(c_int a, c_int b)
		{
			compareCallCounter++;

			return Cmp(a, b);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Cmp_Item(Item a, Item b)
		{
			return (a.Score > b.Score ? 1 : 0) - (a.Score < b.Score ? 1 : 0);
		}
		#endregion
	}
}
