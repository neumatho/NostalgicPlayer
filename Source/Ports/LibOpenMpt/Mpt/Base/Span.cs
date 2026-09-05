/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base
{
	/// <summary>
	/// 
	/// </summary>
	internal class MptSpan<T>
	{
		private const size_t dynamic_extent = size_t.MaxValue;

		private readonly CPointer<T> m_Data;
		private readonly size_t m_Size;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MptSpan()
		{
			m_Data.SetToNull();
			m_Size = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MptSpan(CPointer<T> arr)
		{
			m_Data = arr;
			m_Size = arr.Size();
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MptSpan(CPointer<T> arr, size_t n)
		{
			m_Data = arr;
			m_Size = n;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MptSpan(T[] arr)
		{
			m_Data = arr.ToPointer();
			m_Size = (size_t)arr.Length;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MptSpan(array<T> arr)
		{
			m_Data = arr.data();
			m_Size = arr.size();
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MptSpan(vector<T> vec)
		{
			m_Data = vec.data();
			m_Size = vec.size();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public CPointer<T> Begin()
		{
			return m_Data;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public CPointer<T> End()
		{
			return m_Data + m_Size;
		}



		/********************************************************************/
		/// <summary>
		/// Return or set the item at the index given
		/// </summary>
		/********************************************************************/
		public T this[size_t index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => m_Data[index];

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => m_Data[index] = value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CPointer<T> Data()
		{
			return m_Data;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public size_t Size()
		{
			return m_Size;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MptSpan<T> SubSpan(size_t offset, size_t count = dynamic_extent)
		{
			return new MptSpan<T>(Data() + offset, (count == dynamic_extent) ? (Size() - offset) : count);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MptSpan<T> First(size_t count)
		{
			return new MptSpan<T>(Data(), count);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool Span_Element_Equal(MptSpan<T> other)
		{
			if (Size() != other.Size())
				return false;

			if (Data() == other.Data())
				return true;

			for (size_t i = 0; i < m_Size; i++)
			{
				if (!m_Data[i].Equals(other.m_Data[i]))
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return an enumerator over the elements of the span
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerator GetEnumerator()
		{
			return new Enumerator(m_Data, m_Size);
		}

		#region Enumerator class
		/// <summary>
		/// The enumerator returned by <see cref="GetEnumerator"/>.
		/// </summary>
		public struct Enumerator
		{
			private readonly CPointer<T> m_Data;
			private readonly size_t m_Size;

			// Number of elements returned so far, which means that the
			// element currently referred to is the one just before it
			private size_t index;

			/********************************************************************/
			/// <summary>
			/// Constructs an enumerator over the first n elements of the given
			/// data, positioned before the first element
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Enumerator(CPointer<T> arr, size_t n)
			{
				m_Data = arr;
				m_Size = n;
				index = 0;
			}



			/********************************************************************/
			/// <summary>
			/// Return the element the enumerator currently refers to. Only valid
			/// after a call to <see cref="MoveNext"/> that returned true
			/// </summary>
			/********************************************************************/
			public T Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => m_Data[index - 1];
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
				if (index < m_Size)
				{
					index++;
					return true;
				}

				return false;
			}
		}
		#endregion
	}
}
