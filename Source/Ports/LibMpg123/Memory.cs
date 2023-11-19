/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123
{
	/// <summary>
	/// Handle different memory stuff
	/// </summary>
	internal static class Memory
	{
		/********************************************************************/
		/// <summary>
		/// Reallocate the array given
		/// </summary>
		/********************************************************************/
		public static T[] Int123_Safe_Realloc<T>(T[] array, size_t newSize)
		{
			Array.Resize(ref array, (int)newSize);

			return array;
		}
	}
}
