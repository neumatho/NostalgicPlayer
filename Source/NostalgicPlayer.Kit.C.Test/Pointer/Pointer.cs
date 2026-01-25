/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace NostalgicPlayer.Kit.C.Test.Pointer
{
	/// <summary>
	/// Test CPointer functionality
	/// </summary>
	[TestClass]
	public class Pointer : TestPointerBase
	{
		#region Constructor tests
		/********************************************************************/
		/// <summary>
		/// Test constructor with buffer and offset
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Constructor_WithBufferAndOffset()
		{
			int[] buffer = [ 1, 2, 3, 4, 5 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 2);

			Assert.AreEqual(buffer, ptr.GetOriginalArray());
			Assert.AreEqual(2, ptr.Offset);
			Assert.AreEqual(3, ptr.Length);
			Assert.IsFalse(ptr.IsNull);
			Assert.IsTrue(ptr.IsNotNull);
		}



		/********************************************************************/
		/// <summary>
		/// Test constructor with buffer only
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Constructor_WithBufferOnly()
		{
			int[] buffer = [ 1, 2, 3, 4, 5 ];
			CPointer<int> ptr = new CPointer<int>(buffer);

			Assert.AreEqual(buffer, ptr.GetOriginalArray());
			Assert.AreEqual(0, ptr.Offset);
			Assert.AreEqual(5, ptr.Length);
		}



		/********************************************************************/
		/// <summary>
		/// Test constructor with int length
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Constructor_WithIntLength()
		{
			CPointer<int> ptr = new CPointer<int>(10);

			Assert.IsNotNull(ptr.GetOriginalArray());
			Assert.HasCount(10, ptr.GetOriginalArray());
			Assert.AreEqual(0, ptr.Offset);
			Assert.AreEqual(10, ptr.Length);
		}



		/********************************************************************/
		/// <summary>
		/// Test constructor with size_t length
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Constructor_WithSizeTLength()
		{
			size_t length = 15;
			CPointer<int> ptr = new CPointer<int>(length);

			Assert.IsNotNull(ptr.GetOriginalArray());
			Assert.HasCount(15, ptr.GetOriginalArray());
			Assert.AreEqual(0, ptr.Offset);
			Assert.AreEqual(15, ptr.Length);
		}



		/********************************************************************/
		/// <summary>
		/// Test constructor with another pointer and offset
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Constructor_WithPointerAndOffset()
		{
			int[] buffer = [ 1, 2, 3, 4, 5 ];
			CPointer<int> ptr1 = new CPointer<int>(buffer, 1);
			CPointer<int> ptr2 = new CPointer<int>(ptr1, 2);

			Assert.AreEqual(buffer, ptr2.GetOriginalArray());
			Assert.AreEqual(3, ptr2.Offset);
			Assert.AreEqual(2, ptr2.Length);
			Assert.AreEqual(4, ptr2[0]);
		}
		#endregion

		#region Null tests
		/********************************************************************/
		/// <summary>
		/// Test SetToNull method
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_SetToNull()
		{
			CPointer<int> ptr = new CPointer<int>([ 1, 2, 3 ], 1);
			Assert.IsFalse(ptr.IsNull);

			ptr.SetToNull();

			Assert.IsTrue(ptr.IsNull);
			Assert.IsFalse(ptr.IsNotNull);
			Assert.AreEqual(0, ptr.Offset);
		}



		/********************************************************************/
		/// <summary>
		/// Test IsNull property
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_IsNull()
		{
			CPointer<int> ptr = new CPointer<int>();

			Assert.IsTrue(ptr.IsNull);
			Assert.IsFalse(ptr.IsNotNull);
		}
		#endregion

		#region Indexer tests
		/********************************************************************/
		/// <summary>
		/// Test indexer with int
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Indexer_Int()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 2);

			Assert.AreEqual(30, ptr[0]);
			Assert.AreEqual(40, ptr[1]);
			Assert.AreEqual(20, ptr[-1]);

			ptr[0] = 100;
			Assert.AreEqual(100, buffer[2]);
		}



		/********************************************************************/
		/// <summary>
		/// Test indexer with uint
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Indexer_UInt()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 1);

			Assert.AreEqual(20, ptr[0U]);
			Assert.AreEqual(30, ptr[1U]);

			ptr[2U] = 99;
			Assert.AreEqual(99, buffer[3]);
		}



		/********************************************************************/
		/// <summary>
		/// Test indexer with long
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Indexer_Long()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 1);

			Assert.AreEqual(20, ptr[0L]);
			Assert.AreEqual(30, ptr[1L]);

			ptr[2L] = 88;
			Assert.AreEqual(88, buffer[3]);
		}



		/********************************************************************/
		/// <summary>
		/// Test indexer with ulong
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Indexer_ULong()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 1);

			Assert.AreEqual(20, ptr[0UL]);
			Assert.AreEqual(30, ptr[1UL]);

			ptr[2UL] = 77;
			Assert.AreEqual(77, buffer[3]);
		}



		/********************************************************************/
		/// <summary>
		/// Test indexer with increment - get
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Indexer_WithIncrement_Get()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 0);

			int val = ptr[0, 1];
			Assert.AreEqual(10, val);
			Assert.AreEqual(1, ptr.Offset);

			val = ptr[0, 2];
			Assert.AreEqual(20, val);
			Assert.AreEqual(3, ptr.Offset);
		}



		/********************************************************************/
		/// <summary>
		/// Test indexer with increment - set
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Indexer_WithIncrement_Set()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 0);

			ptr[0, 1] = 99;
			Assert.AreEqual(99, buffer[0]);
			Assert.AreEqual(1, ptr.Offset);

			ptr[0, 2] = 88;
			Assert.AreEqual(88, buffer[1]);
			Assert.AreEqual(3, ptr.Offset);
		}
		#endregion

		#region Addition operator tests
		/********************************************************************/
		/// <summary>
		/// Test addition operator with int
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_AdditionOperator_Int()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 1);
			CPointer<int> ptr2 = ptr + 2;

			Assert.AreEqual(buffer, ptr2.GetOriginalArray());
			Assert.AreEqual(3, ptr2.Offset);
			Assert.AreEqual(40, ptr2[0]);
		}



		/********************************************************************/
		/// <summary>
		/// Test addition operator with uint
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_AdditionOperator_UInt()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 1);
			CPointer<int> ptr2 = ptr + 2U;

			Assert.AreEqual(buffer, ptr2.GetOriginalArray());
			Assert.AreEqual(3, ptr2.Offset);
			Assert.AreEqual(40, ptr2[0]);
		}



		/********************************************************************/
		/// <summary>
		/// Test addition operator with long
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_AdditionOperator_Long()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 1);
			CPointer<int> ptr2 = ptr + 2L;

			Assert.AreEqual(buffer, ptr2.GetOriginalArray());
			Assert.AreEqual(3, ptr2.Offset);
			Assert.AreEqual(40, ptr2[0]);
		}



		/********************************************************************/
		/// <summary>
		/// Test addition operator with ulong
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_AdditionOperator_ULong()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 1);
			CPointer<int> ptr2 = ptr + 2UL;

			Assert.AreEqual(buffer, ptr2.GetOriginalArray());
			Assert.AreEqual(3, ptr2.Offset);
			Assert.AreEqual(40, ptr2[0]);
		}
		#endregion

		#region Subtraction operator tests
		/********************************************************************/
		/// <summary>
		/// Test subtraction operator with int
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_SubtractionOperator_Int()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 3);
			CPointer<int> ptr2 = ptr - 2;

			Assert.AreEqual(buffer, ptr2.GetOriginalArray());
			Assert.AreEqual(1, ptr2.Offset);
			Assert.AreEqual(20, ptr2[0]);
		}



		/********************************************************************/
		/// <summary>
		/// Test subtraction operator with uint
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_SubtractionOperator_UInt()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 3);
			CPointer<int> ptr2 = ptr - 2U;

			Assert.AreEqual(buffer, ptr2.GetOriginalArray());
			Assert.AreEqual(1, ptr2.Offset);
			Assert.AreEqual(20, ptr2[0]);
		}



		/********************************************************************/
		/// <summary>
		/// Test subtraction operator with long
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_SubtractionOperator_Long()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 3);
			CPointer<int> ptr2 = ptr - 2L;

			Assert.AreEqual(buffer, ptr2.GetOriginalArray());
			Assert.AreEqual(1, ptr2.Offset);
			Assert.AreEqual(20, ptr2[0]);
		}



		/********************************************************************/
		/// <summary>
		/// Test subtraction operator with ulong
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_SubtractionOperator_ULong()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 3);
			CPointer<int> ptr2 = ptr - 2UL;

			Assert.AreEqual(buffer, ptr2.GetOriginalArray());
			Assert.AreEqual(1, ptr2.Offset);
			Assert.AreEqual(20, ptr2[0]);
		}



		/********************************************************************/
		/// <summary>
		/// Test pointer difference operator
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_PointerDifference()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr1 = new CPointer<int>(buffer, 4);
			CPointer<int> ptr2 = new CPointer<int>(buffer, 1);

			int diff = ptr1 - ptr2;
			Assert.AreEqual(3, diff);

			diff = ptr2 - ptr1;
			Assert.AreEqual(-3, diff);
		}



		/********************************************************************/
		/// <summary>
		/// Test pointer difference with null pointers
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_PointerDifference_WithNull()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr1 = new CPointer<int>(buffer, 2);
			CPointer<int> ptr2 = new CPointer<int>();

			int diff = ptr2 - ptr1;
			Assert.AreEqual(-0x12345678, diff);
		}



		/********************************************************************/
		/// <summary>
		/// Test pointer difference with different buffers
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_PointerDifference_DifferentBuffers()
		{
			CPointer<int> ptr1 = new CPointer<int>([ 1, 2, 3 ], 1);
			CPointer<int> ptr2 = new CPointer<int>([ 4, 5, 6 ], 1);

			bool exceptionThrown = false;

			try
			{
				int diff = ptr1 - ptr2;
			}
			catch (ArgumentException)
			{
				exceptionThrown = true;
			}

			Assert.IsTrue(exceptionThrown);
		}
		#endregion

		#region Increment/Decrement operator tests
		/********************************************************************/
		/// <summary>
		/// Test increment operator
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_IncrementOperator()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 1);
			CPointer<int> ptr2 = ++ptr;

			Assert.AreEqual(buffer, ptr2.GetOriginalArray());
			Assert.AreEqual(2, ptr2.Offset);
			Assert.AreEqual(30, ptr2[0]);
		}



		/********************************************************************/
		/// <summary>
		/// Test decrement operator
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_DecrementOperator()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 3);
			CPointer<int> ptr2 = --ptr;

			Assert.AreEqual(buffer, ptr2.GetOriginalArray());
			Assert.AreEqual(2, ptr2.Offset);
			Assert.AreEqual(30, ptr2[0]);
		}
		#endregion

		#region Comparison operator tests
		/********************************************************************/
		/// <summary>
		/// Test equality operator
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_EqualityOperator()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr1 = new CPointer<int>(buffer, 2);
			CPointer<int> ptr2 = new CPointer<int>(buffer, 2);
			CPointer<int> ptr3 = new CPointer<int>(buffer, 3);

			Assert.IsTrue(ptr1 == ptr2);
			Assert.IsFalse(ptr1 == ptr3);
		}



		/********************************************************************/
		/// <summary>
		/// Test inequality operator
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_InequalityOperator()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr1 = new CPointer<int>(buffer, 2);
			CPointer<int> ptr2 = new CPointer<int>(buffer, 2);
			CPointer<int> ptr3 = new CPointer<int>(buffer, 3);

			Assert.IsFalse(ptr1 != ptr2);
			Assert.IsTrue(ptr1 != ptr3);
		}



		/********************************************************************/
		/// <summary>
		/// Test greater than operator
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_GreaterThanOperator()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr1 = new CPointer<int>(buffer, 3);
			CPointer<int> ptr2 = new CPointer<int>(buffer, 1);

			Assert.IsTrue(ptr1 > ptr2);
			Assert.IsFalse(ptr2 > ptr1);
#pragma warning disable CS1718 // Comparison made to same variable
			Assert.IsFalse(ptr1 > ptr1);
#pragma warning restore CS1718 // Comparison made to same variable
		}



		/********************************************************************/
		/// <summary>
		/// Test less than operator
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_LessThanOperator()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr1 = new CPointer<int>(buffer, 1);
			CPointer<int> ptr2 = new CPointer<int>(buffer, 3);

			Assert.IsTrue(ptr1 < ptr2);
			Assert.IsFalse(ptr2 < ptr1);
#pragma warning disable CS1718 // Comparison made to same variable
			Assert.IsFalse(ptr1 < ptr1);
#pragma warning restore CS1718 // Comparison made to same variable
		}



		/********************************************************************/
		/// <summary>
		/// Test greater than or equal operator
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_GreaterThanOrEqualOperator()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr1 = new CPointer<int>(buffer, 3);
			CPointer<int> ptr2 = new CPointer<int>(buffer, 1);
			CPointer<int> ptr3 = new CPointer<int>(buffer, 3);

			Assert.IsTrue(ptr1 >= ptr2);
			Assert.IsTrue(ptr1 >= ptr3);
			Assert.IsFalse(ptr2 >= ptr1);
		}



		/********************************************************************/
		/// <summary>
		/// Test less than or equal operator
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_LessThanOrEqualOperator()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr1 = new CPointer<int>(buffer, 1);
			CPointer<int> ptr2 = new CPointer<int>(buffer, 3);
			CPointer<int> ptr3 = new CPointer<int>(buffer, 1);

			Assert.IsTrue(ptr1 <= ptr2);
			Assert.IsTrue(ptr1 <= ptr3);
			Assert.IsFalse(ptr2 <= ptr1);
		}
		#endregion

		#region Method tests
		/********************************************************************/
		/// <summary>
		/// Test AsSpan method
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_AsSpan()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 2);

			Span<int> span = ptr.AsSpan();

			Assert.AreEqual(3, span.Length);
			Assert.AreEqual(30, span[0]);
			Assert.AreEqual(40, span[1]);
			Assert.AreEqual(50, span[2]);
		}



		/********************************************************************/
		/// <summary>
		/// Test AsSpan method with length parameter
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_AsSpan_WithLength()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 1);

			Span<int> span = ptr.AsSpan(3);

			Assert.AreEqual(3, span.Length);
			Assert.AreEqual(20, span[0]);
			Assert.AreEqual(30, span[1]);
			Assert.AreEqual(40, span[2]);
		}



		/********************************************************************/
		/// <summary>
		/// Test AsSpan method with offset and length parameters
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_AsSpan_WithOffsetAndLength()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 1);

			Span<int> span = ptr.AsSpan(1, 2);

			Assert.AreEqual(2, span.Length);
			Assert.AreEqual(30, span[0]);
			Assert.AreEqual(40, span[1]);
		}



		/********************************************************************/
		/// <summary>
		/// Test AsMemory method
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_AsMemory()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 2);

			Memory<int> memory = ptr.AsMemory();

			Assert.AreEqual(3, memory.Length);
			Assert.AreEqual(30, memory.Span[0]);
			Assert.AreEqual(40, memory.Span[1]);
			Assert.AreEqual(50, memory.Span[2]);
		}



		/********************************************************************/
		/// <summary>
		/// Test AsMemory method with length parameter
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_AsMemory_WithLength()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 1);

			Memory<int> memory = ptr.AsMemory(3);

			Assert.AreEqual(3, memory.Length);
			Assert.AreEqual(20, memory.Span[0]);
			Assert.AreEqual(30, memory.Span[1]);
			Assert.AreEqual(40, memory.Span[2]);
		}



		/********************************************************************/
		/// <summary>
		/// Test AsMemory method with offset and length parameters
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_AsMemory_WithOffsetAndLength()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 1);

			Memory<int> memory = ptr.AsMemory(1, 2);

			Assert.AreEqual(2, memory.Length);
			Assert.AreEqual(30, memory.Span[0]);
			Assert.AreEqual(40, memory.Span[1]);
		}



		/********************************************************************/
		/// <summary>
		/// Test Clear method without parameters
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Clear()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 2);

			ptr.Clear();

			Assert.AreEqual(10, buffer[0]);
			Assert.AreEqual(20, buffer[1]);
			Assert.AreEqual(0, buffer[2]);
			Assert.AreEqual(0, buffer[3]);
			Assert.AreEqual(0, buffer[4]);
		}



		/********************************************************************/
		/// <summary>
		/// Test Clear method with length parameter
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Clear_WithLength()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 1);

			ptr.Clear(2);

			Assert.AreEqual(10, buffer[0]);
			Assert.AreEqual(0, buffer[1]);
			Assert.AreEqual(0, buffer[2]);
			Assert.AreEqual(40, buffer[3]);
			Assert.AreEqual(50, buffer[4]);
		}



		/********************************************************************/
		/// <summary>
		/// Test ToString method with char pointer
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_ToString_CharPointer()
		{
			char[] buffer = [ 'H', 'e', 'l', 'l', 'o', '\0', 'W', 'o', 'r', 'l', 'd' ];
			CPointer<char> ptr = new CPointer<char>(buffer, 0);

			string result = ptr.ToString();

			Assert.AreEqual("Hello", result);
		}



		/********************************************************************/
		/// <summary>
		/// Test ToString method with null char pointer
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_ToString_NullCharPointer()
		{
			CPointer<char> ptr = new CPointer<char>();

			string result = ptr.ToString();

			Assert.AreEqual(string.Empty, result);
		}



		/********************************************************************/
		/// <summary>
		/// Test Equals method
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Equals()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr1 = new CPointer<int>(buffer, 2);
			CPointer<int> ptr2 = new CPointer<int>(buffer, 2);
			CPointer<int> ptr3 = new CPointer<int>(buffer, 3);

			Assert.IsTrue(ptr1.Equals(ptr2));
			Assert.IsFalse(ptr1.Equals(ptr3));
		}



		/********************************************************************/
		/// <summary>
		/// Test CompareTo method
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_CompareTo()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr1 = new CPointer<int>(buffer, 1);
			CPointer<int> ptr2 = new CPointer<int>(buffer, 3);
			CPointer<int> ptr3 = new CPointer<int>(buffer, 1);

			Assert.IsLessThan(0, ptr1.CompareTo(ptr2));
			Assert.IsGreaterThan(0, ptr2.CompareTo(ptr1));
			Assert.AreEqual(0, ptr1.CompareTo(ptr3));
		}



		/********************************************************************/
		/// <summary>
		/// Test CompareTo with null pointer
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_CompareTo_WithNull()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr1 = new CPointer<int>(buffer, 1);
			CPointer<int> ptr2 = new CPointer<int>();

			Assert.IsGreaterThan(0, ptr1.CompareTo(ptr2));
			Assert.IsLessThan(0, ptr2.CompareTo(ptr1));
		}



		/********************************************************************/
		/// <summary>
		/// Test CompareTo with different buffers
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_CompareTo_DifferentBuffers()
		{
			CPointer<int> ptr1 = new CPointer<int>([ 1, 2, 3 ], 1);
			CPointer<int> ptr2 = new CPointer<int>([ 4, 5, 6 ], 1);

			bool exceptionThrown = false;

			try
			{
				ptr1.CompareTo(ptr2);
			}
			catch (ArgumentException)
			{
				exceptionThrown = true;
			}

			Assert.IsTrue(exceptionThrown);
		}



		/********************************************************************/
		/// <summary>
		/// Test MakeDeepClone method
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_MakeDeepClone()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 2);

			CPointer<int> clonedPtr = ptr.MakeDeepClone();

			Assert.AreNotEqual(ptr.GetOriginalArray(), clonedPtr.GetOriginalArray());
			Assert.AreEqual(ptr.Offset, clonedPtr.Offset);
			Assert.AreEqual(ptr.Length, clonedPtr.Length);
			Assert.AreEqual(30, clonedPtr[0]);

			buffer[2] = 99;
			Assert.AreEqual(99, ptr[0]);
			Assert.AreEqual(30, clonedPtr[0]);
		}



		/********************************************************************/
		/// <summary>
		/// Test MakeDeepClone with null pointer
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_MakeDeepClone_Null()
		{
			CPointer<int> ptr = new CPointer<int>();

			CPointer<int> clonedPtr = ptr.MakeDeepClone();

			Assert.IsTrue(clonedPtr.IsNull);
		}



		/********************************************************************/
		/// <summary>
		/// Test implicit conversion from array
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_ImplicitConversion_FromArray()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = buffer;

			Assert.AreEqual(buffer, ptr.GetOriginalArray());
			Assert.AreEqual(0, ptr.Offset);
			Assert.AreEqual(5, ptr.Length);
			Assert.AreEqual(10, ptr[0]);
		}



		/********************************************************************/
		/// <summary>
		/// Test Length property with offset
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Length_Property()
		{
			int[] buffer = [ 10, 20, 30, 40, 50 ];
			CPointer<int> ptr = new CPointer<int>(buffer, 0);
			Assert.AreEqual(5, ptr.Length);

			ptr = new CPointer<int>(buffer, 2);
			Assert.AreEqual(3, ptr.Length);

			ptr = new CPointer<int>(buffer, 5);
			Assert.AreEqual(0, ptr.Length);
		}
		#endregion
	}
}
