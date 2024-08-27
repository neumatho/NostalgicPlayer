/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibOgg
{
	/// <summary>
	/// Different memory methods
	/// </summary>
	internal static class Memory
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static T[] Ogg_MAlloc<T>(size_t size)
		{
			return new T[size];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static T[] Ogg_Realloc<T>(T[] ptr, size_t newSize)
		{
			Array.Resize(ref ptr, (int)newSize);

			return ptr;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Ogg_Free<T>(T[] ptr)
		{
		}
	}
}
