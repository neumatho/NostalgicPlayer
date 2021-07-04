/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.IO;

namespace Polycode.NostalgicPlayer.Kit.Utility
{
	/// <summary>
	/// Different helper methods
	/// </summary>
	public static class Helpers
	{
		/********************************************************************/
		/// <summary>
		/// Create an array of the type given and fill it will newly created
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
		/// Compare or part of two arrays
		/// objects of same type
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
		/// Copy length bytes from one stream to another
		/// </summary>
		/********************************************************************/
		public static void CopyData(Stream source, Stream destination, int length)
		{
			byte[] buf = new byte[1024];

			while (length >= 1024)
			{
				int len = source.Read(buf, 0, 1024);
				if (len < 1024)
					Array.Clear(buf, len, 1024 - len);

				destination.Write(buf, 0, 1024);

				length -= 1024;
			}

			if (length > 0)
			{
				int len = source.Read(buf, 0, length);
				if (len < length)
					Array.Clear(buf, len, length - len);

				destination.Write(buf, 0, len);
			}
		}
	}
}
