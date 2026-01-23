/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Kit.C
{
	/// <summary>
	/// Extension methods for arrays
	/// </summary>
	public static class ArrayExtension
	{
		/********************************************************************/
		/// <summary>
		/// Convert the enumerable to a C pointer
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> ToPointer<T>(this T[] e)
		{
			return new CPointer<T>(e);
		}
	}
}
