/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Kit.C.Std.Iterators
{
	/// <summary>
	/// The enumerator returned by the GetEnumerator() of the contiguous
	/// containers in this namespace (vector‹T› and array‹T›), so that they can
	/// be used in a C# foreach loop.
	///
	/// This has no equivalent in C++, where a range based for loop uses the
	/// begin()/end() iterators directly. In C# a foreach instead needs a type
	/// that exposes Current and MoveNext(), which is what this provides.
	///
	/// Current returns the element by reference (ref T), so a foreach loop can
	/// both read and write the elements, just like a C++ range based for over
	/// auto＆. It also means the loop variable itself may be taken by reference
	/// (foreach (ref T element in container))
	/// </summary>
	public struct Buffer_Enumerator<T>
	{
		private readonly T[] buffer;
		private readonly int count;
		private int index;

		/********************************************************************/
		/// <summary>
		/// Constructs an enumerator over the first count elements of the
		/// given buffer, positioned before the first element
		/// </summary>
		/********************************************************************/
		public Buffer_Enumerator(T[] buffer, int count)
		{
			this.buffer = buffer;
			this.count = count;
			index = -1;
		}



		/********************************************************************/
		/// <summary>
		/// Returns a reference to the element the enumerator currently
		/// refers to. Only valid after a call to <see cref="MoveNext"/> that
		/// returned true
		/// </summary>
		/********************************************************************/
		public ref T Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref buffer[index];
		}



		/********************************************************************/
		/// <summary>
		/// Advances the enumerator to the next element. Returns true if
		/// there is such an element, or false if the end has been reached
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			int next = index + 1;

			if (next < count)
			{
				index = next;
				return true;
			}

			return false;
		}
	}
}
