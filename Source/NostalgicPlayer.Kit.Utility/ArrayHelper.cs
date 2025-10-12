/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.Utility
{
	/// <summary>
	/// Array helper methods
	/// </summary>
	public static class ArrayHelper
	{
		/********************************************************************/
		/// <summary>
		/// Create an array of the type given and fill it with newly created
		/// objects of same type
		/// </summary>
		/********************************************************************/
		public static T[] InitializeArray<T>(int length) where T : new()
		{
			T[] array = new T[length];

			for (int i = 0; i < length; i++)
				array[i] = new T();

			return array;
		}



		/********************************************************************/
		/// <summary>
		/// Create an array where each element holds an array of the type
		/// given. The second array is filled with newly created objects of
		/// same type
		/// </summary>
		/********************************************************************/
		public static T[][] InitializeArray<T>(int length1, int length2) where T : new()
		{
			T[][] array = new T[length1][];

			for (int i = 0; i < length1; i++)
				array[i] = InitializeArray<T>(length2);

			return array;
		}



		/********************************************************************/
		/// <summary>
		/// Create an array where each element holds an array of the type
		/// given. The second array is not initialized
		/// </summary>
		/********************************************************************/
		public static T[][] Initialize2Arrays<T>(int length1, int length2) where T : new()
		{
			T[][] array = new T[length1][];

			for (int i = 0; i < length1; i++)
				array[i] = new T[length2];

			return array;
		}



		/********************************************************************/
		/// <summary>
		/// Create an array where each element holds an array to an array of
		/// the type given. The first and second arrays are initialized, the
		/// third is not
		/// </summary>
		/********************************************************************/
		public static T[][][] Initialize3Arrays<T>(int length1, int length2, int length3) where T : new()
		{
			T[][][] array = new T[length1][][];

			for (int i = 0; i < length1; i++)
				array[i] = Initialize2Arrays<T>(length2, length3);

			return array;
		}



		/********************************************************************/
		/// <summary>
		/// Create an array where each element holds an array to an array of
		/// the type given. The last array is not initialized
		/// </summary>
		/********************************************************************/
		public static T[][][] InitializeArrayWithArray<T>(int length1, int length2) where T : new()
		{
			T[][][] array = new T[length1][][];

			for (int i = 0; i < length1; i++)
				array[i] = new T[length2][];

			return array;
		}



		/********************************************************************/
		/// <summary>
		/// Create a 2D array where each element holds an array of the type
		/// given. The second array is filled with newly created objects of
		/// same type
		/// </summary>
		/********************************************************************/
		public static T[,] Initialize2DArray<T>(int length1, int length2) where T : new()
		{
			T[,] array = new T[length1, length2];

			for (int i = 0; i < length1; i++)
			{
				for (int j = 0; j < length2; j++)
					array[i, j] = new T();
			}

			return array;
		}



		/********************************************************************/
		/// <summary>
		/// Compare two arrays having objects of same type
		/// </summary>
		/********************************************************************/
		public static bool ArrayCompare<T>(T[] array1, int offset1, T[] array2, int offset2, int length) where T : IComparable
		{
			int len1 = Math.Min(length, array1.Length - offset1);
			int len2 = Math.Min(length, array2.Length - offset2);

			if (len1 != len2)
				return false;

			if (len1 > 0)
			{
				for (int i = 0; i < len1; i++, offset1++, offset2++)
				{
					if (array1[offset1].CompareTo(array2[offset2]) != 0)
						return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Clone an array and return the new one
		/// </summary>
		/********************************************************************/
		public static T[] CloneArray<T>(T[] array)
		{
			T[] newArray = new T[array.Length];
			Array.Copy(array, newArray, array.Length);

			return newArray;
		}



		/********************************************************************/
		/// <summary>
		/// Clone an array and return the new one
		/// </summary>
		/********************************************************************/
		public static T[][] CloneArray<T>(T[][] array)
		{
			T[][] newArray = new T[array.Length][];

			for (int i = array.Length - 1; i >= 0; i--)
				newArray[i] = CloneArray(array[i]);

			return newArray;
		}



		/********************************************************************/
		/// <summary>
		/// Clone an array and return the new one
		/// </summary>
		/********************************************************************/
		public static T[] CloneObjectArray<T>(T[] array) where T : IDeepCloneable<T>
		{
			T[] newArray = new T[array.Length];

			for (int i = array.Length - 1; i >= 0; i--)
			{
				if (array[i] != null)
					newArray[i] = array[i].MakeDeepClone();
			}

			return newArray;
		}



		/********************************************************************/
		/// <summary>
		/// Clone a two dimension array and return the new one
		/// </summary>
		/********************************************************************/
		public static T[,] CloneObjectArray<T>(T[,] array) where T : IDeepCloneable<T>
		{
			int dimension0Count = array.GetLength(0);
			int dimension1Count = array.GetLength(1);

			T[,] newArray = new T[dimension0Count, dimension1Count];

			for (int i = 0; i < dimension0Count; i++)
			{
				for (int j = 0; j < dimension1Count; j++)
					newArray[i, j] = array[i, j].MakeDeepClone();
			}

			return newArray;
		}



		/********************************************************************/
		/// <summary>
		/// Clear all elements of the given array
		/// </summary>
		/********************************************************************/
		public static void ClearArray<T>(T[] array) where T : IClearable
		{
			for (int i = array.Length - 1; i >= 0; i--)
				array[i].Clear();
		}
	}
}
