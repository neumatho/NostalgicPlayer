/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base
{
	/// <summary>
	/// Extension methods for spans, so they can be used with the same method
	/// names as the C++ mpt::span does in the OpenMPT code
	/// </summary>
	public static class SpanExtension
	{
		/********************************************************************/
		/// <summary>
		/// Return the number of elements in the span
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static size_t Size<T>(this ReadOnlySpan<T> span)
		{
			return (size_t)span.Length;
		}



		/********************************************************************/
		/// <summary>
		/// Return a sub-span of the first count elements
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<T> First<T>(this Span<T> span, size_t count)
		{
			return span.Slice(0, (int)count);
		}
	}
}
