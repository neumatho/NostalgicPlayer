/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace NostalgicPlayer.Kit.C.Test.Array_
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class BSearch : TestArrayBase
	{
		private struct Pair
		{
			public Pair(c_int key, c_int value)
			{
				Key = key;
				Value = value;
			}

			public c_int Key { get; }
			public c_int Value { get; }
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
			c_int key = 7;
			compareCallCounter = 0;

			CPointer<c_int> r = CArray.bsearch<c_int, c_int>(key, null, 0, Cmp_Int_Asc_Counted);
			Assert.IsTrue(r.IsNull);
			Assert.AreEqual(0, compareCallCounter);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Singleton_Found_And_NotFound()
		{
			c_int[] arr = [ 42 ];
			c_int k1 = 42, k2 = 41;

			CPointer<c_int> r1 = CArray.bsearch<c_int, c_int>(k1, arr, 1, Cmp_Int_Asc);
			CPointer<c_int> r2 = CArray.bsearch<c_int, c_int>(k2, arr, 1, Cmp_Int_Asc);

			Assert.AreEqual(arr[0], r1[0]);
			Assert.IsTrue(r2.IsNull);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Found_All_In_Ordered_Ints()
		{
			CPointer<c_int> arr = new CPointer<c_int>(100);

			for (c_int i = 0; i < 100; i++)
				arr[i] = i;

			for (c_int i = 0; i < 100; i++)
			{
				c_int key = i;

				CPointer<c_int> r = CArray.bsearch<c_int, c_int>(key, arr, 100, Cmp_Int_Asc);

				Assert.IsTrue(r.IsNotNull);
				Assert.IsTrue(Ptr_In_Array(r, arr, 100));
				Assert.AreEqual(key, r[0]);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Not_Found_In_Gaps()
		{
			c_int[] arr = new c_int[10];

			for (c_int i = 0; i < 10; i++)
				arr[i] = 2 * i;

			for (c_int miss = 1; miss < 20; miss += 2)
			{
				c_int key = miss;

				CPointer<c_int> r = CArray.bsearch<c_int, c_int>(key, arr, 10, Cmp_Int_Asc);
				Assert.IsTrue(r.IsNull);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Duplicates_Any_Match_Is_Ok()
		{
			c_int[] arr = [ 1, 2, 2, 2, 3, 4, 4, 5 ];

			c_int key = 2;
			c_int first = -1, last = -1;

			for (c_int i = 0; i < arr.Length; i++)
			{
				if (arr[i] == key)
				{
					if (first < 0)
						first = i;

					last = i;
				}
			}

			CPointer<c_int> arrPtr = arr;
			CPointer<c_int> r = CArray.bsearch(key, arrPtr, (size_t)arr.Length, Cmp_Int_Asc);

			Assert.IsTrue(r.IsNotNull);
			Assert.IsTrue(Ptr_In_Array(r, arrPtr, (size_t)arr.Length));

			c_int idx = r - arrPtr;
			Assert.IsGreaterThanOrEqualTo(first, idx);
			Assert.IsLessThanOrEqualTo(last, idx);
			Assert.AreEqual(key, r[0]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Struct_Key_Lookup()
		{
			Pair[] a =
			[
				new Pair(1, 10), new Pair(2, 20), new Pair(2, 21), new Pair(4, 40), new Pair(7, 70), new Pair(9, 90)
			];

			Pair key = new Pair(2, 0);

			CPointer<Pair> r = CArray.bsearch<Pair, Pair>(key, a, (size_t)a.Length, Cmp_Pair_Key_Asc);

			Assert.IsTrue(r.IsNotNull);
			Assert.AreEqual(2, r[0].Key);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Descending_Order_With_Desc_Comparator()
		{
			c_int[] arr = [ 9, 7, 7, 5, 3, 1, 0, -2, -5 ];

			c_int key = 7;

			CPointer<c_int> r = CArray.bsearch<c_int, c_int>(key, arr, (size_t)arr.Length, Cmp_Int_Desc);

			Assert.IsTrue(r.IsNotNull);
			Assert.AreEqual(key, r[0]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Strings()
		{
			string[] arr = [ "apple", "banana", "carrot", "date", "elderberry", "fig", "grape" ];

			string key1 = "banana";
			string key2 = "coconut";

			CPointer<string> r1 = CArray.bsearch<string, string>(key1, arr, (size_t)arr.Length, Cmp_Str_Asc);
			CPointer<string> r2 = CArray.bsearch<string, string>(key2, arr, (size_t)arr.Length, Cmp_Str_Asc);

			Assert.IsTrue(r1.IsNotNull);
			Assert.AreEqual(key1, r1[0]);
			Assert.IsTrue(r2.IsNull);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Pointer_Alignment_And_Bounds()
		{
			CPointer<c_int> arr = new CPointer<c_int>(32);

			for (c_int i = 0; i < 32; i++)
				arr[i] = i * 3;

			c_int key = arr[17];

			CPointer<c_int> r = CArray.bsearch(key, arr, 32, Cmp_Int_Asc);

			Assert.IsTrue(Ptr_In_Array(r, arr, 32));
			c_int idx = r - arr;
			Assert.IsLessThan(32, idx);
			Assert.AreEqual(key, r[0]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Comparison_Calls_Are_Logarithmic()
		{
			c_int n = 1000;

			CPointer<c_int> arr = new CPointer<c_int>(n);

			for (c_int i = 0; i < n; i++)
				arr[i] = i * 2;

			c_int key = arr[n / 3];
			compareCallCounter = 0;

			CPointer<c_int> r = CArray.bsearch(key, arr, (size_t)n, Cmp_Int_Asc_Counted);

			Assert.IsTrue(r.IsNotNull);
			Assert.AreEqual(key, r[0]);

			c_int limit = (2 * Ceil_Log2_Size(n)) + 5;
			Assert.IsLessThanOrEqualTo(limit, compareCallCounter);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Nmemb_Zero_With_Nonnull_Base()
		{
			c_int[] arr = [ 1, 2, 3, 4 ];

			c_int key = 3;
			compareCallCounter = 0;

			CPointer<c_int> r = CArray.bsearch<c_int, c_int>(key, arr, 0, Cmp_Int_Asc_Counted);

			Assert.IsTrue(r.IsNull);
			Assert.AreEqual(0, compareCallCounter);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_First_And_Last_Element()
		{
			c_int[] arr = [ -10, -5, 0, 5, 9, 12, 18, 21 ];
			c_int k1 = -10, k2 = 21;

			CPointer<c_int> r1 = CArray.bsearch<c_int, c_int>(k1, arr, (size_t)arr.Length, Cmp_Int_Asc);
			CPointer<c_int> r2 = CArray.bsearch<c_int, c_int>(k2, arr, (size_t)arr.Length, Cmp_Int_Asc);

			Assert.AreEqual(arr[0], r1[0]);
			Assert.AreEqual(arr[^1], r2[0]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Large_Gap_NotFound()
		{
			c_int n = 4096;

			CPointer<c_int> arr = new CPointer<c_int>(n);

			for (c_int i = 0; i < n; i++)
				arr[i] = i * 4;

			c_int key = -3;

			CPointer<c_int> r = CArray.bsearch(key, arr, (size_t)n, Cmp_Int_Asc);
			Assert.IsTrue(r.IsNull);

			key = n * 4 + 1;

			r = CArray.bsearch(key, arr, (size_t)n, Cmp_Int_Asc);
			Assert.IsTrue(r.IsNull);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Ptr_In_Array<T>(CPointer<T> ret, CPointer<T> @base, size_t nMemb)
		{
			if (ret.IsNull)
				return false;

			if (ret.Buffer != @base.Buffer)
				return false;

			if ((ret.Offset < @base.Offset) || (ret.Offset >= (@base.Offset + (c_int)nMemb)))
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Ceil_Log2_Size(c_int n)
		{
			c_int v = n, lg = 0;

			v--;
			while (v != 0)
			{
				v >>= 1;
				lg++;
			}

			return lg;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Cmp_Int_Asc(c_int a, c_int b)
		{
			return (a > b ? 1 : 0) - (a < b ? 1 : 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Cmp_Int_Desc(c_int a, c_int b)
		{
			return (b > a ? 1 : 0) - (b < a ? 1 : 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Cmp_Int_Asc_Counted(c_int a, c_int b)
		{
			compareCallCounter++;

			return Cmp_Int_Asc(a, b);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Cmp_Pair_Key_Asc(Pair a, Pair b)
		{
			c_int ka = a.Key;
			c_int kb = b.Key;

			return (ka > kb ? 1 : 0) - (ka < kb ? 1 : 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Cmp_Str_Asc(string a, string b)
		{
			return CString.strcmp(a.ToCharPointer(), b);
		}
		#endregion
	}
}
