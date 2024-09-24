/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Utility
{
	/// <summary>
	/// This holds a buffer and a start offset. Can be used in C ports, where
	/// there are a lot of pointer calculations. By using this, you don't need
	/// to hold the offset by yourself and pass it around.
	///
	/// It is almost similar to Span, except that with this, you can also use
	/// negative indexes to retrieve the data, which is used by some C programs
	/// </summary>
	public struct Pointer<T> : IEquatable<Pointer<T>>
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Pointer(T[] buffer) : this(buffer, 0)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Pointer(T[] buffer, int offset)
		{
			Buffer = buffer;
			Offset = offset;
		}



		/********************************************************************/
		/// <summary>
		/// Return the whole buffer
		/// </summary>
		/********************************************************************/
		public T[] Buffer { get; private set; }



		/********************************************************************/
		/// <summary>
		/// Return the offset in the buffer where the first item starts
		/// </summary>
		/********************************************************************/
		public int Offset { get; private set; }



		/********************************************************************/
		/// <summary>
		/// Clear the pointer
		/// </summary>
		/********************************************************************/
		public void SetToNull()
		{
			Buffer = null;
			Offset = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Check to see if the pointer is null
		/// </summary>
		/********************************************************************/
		public bool IsNull => Buffer == null;



		/********************************************************************/
		/// <summary>
		/// Return or set the item at the index given
		/// </summary>
		/********************************************************************/
		public T this[int index]
		{
			get => Buffer[Offset + index];
			set => Buffer[Offset + index] = value;
		}



		/********************************************************************/
		/// <summary>
		/// Return a new pointer where the original pointer is incremented by
		/// the value given
		/// </summary>
		/********************************************************************/
		public static Pointer<T> operator + (Pointer<T> ptr, int increment)
		{
			return new Pointer<T>(ptr.Buffer, ptr.Offset + increment);
		}



		/********************************************************************/
		/// <summary>
		/// Return the current pointer with the current offset, but the
		/// offset will be incremented by one afterwards
		/// </summary>
		/********************************************************************/
		public static Pointer<T> operator ++ (Pointer<T> ptr)
		{
			return ptr + 1;
		}



		/********************************************************************/
		/// <summary>
		/// Will calculate the difference between the two pointers. Both
		/// pointers need to use the same buffer
		/// </summary>
		/********************************************************************/
		public static int operator - (Pointer<T> ptr1, Pointer<T> ptr2)
		{
			if (ptr1.Buffer != ptr2.Buffer)
				throw new ArgumentException("Both pointers need to use the same buffer");

			return ptr1.Offset - ptr2.Offset;
		}

		#region IEquatable implementation
		/********************************************************************/
		/// <summary>
		/// Compare two pointers
		/// </summary>
		/********************************************************************/
		public bool Equals(Pointer<T> other)
		{
			return (Buffer == other.Buffer) && (Offset == other.Offset);
		}
		#endregion
	}
}
