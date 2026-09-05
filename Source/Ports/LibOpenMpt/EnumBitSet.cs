/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt
{
	/// <summary>
	/// Wrapper around bitset so it is possible to use enum instead
	/// of size_t (prevent casting)
	/// </summary>
	internal class EnumBitSet<TEnum> : IDeepCloneable<EnumBitSet<TEnum>>, ICopyTo<EnumBitSet<TEnum>> where TEnum : unmanaged, Enum
	{
		private readonly bitset _set;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public EnumBitSet(TEnum size)
		{
			_set = new bitset(ToSize(size));
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public EnumBitSet(EnumBitSet<TEnum> other)
		{
			_set = new bitset(other._set);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool this[TEnum pos]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _set[ToSize(pos)];
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _set[ToSize(pos)] = value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool any()
		{
			return _set.any();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void set(TEnum pos, bool value = true)
		{
			_set.set(ToSize(pos), value);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void reset()
		{
			_set.reset();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void reset(TEnum pos)
		{
			_set.reset(ToSize(pos));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void flip(TEnum pos)
		{
			_set.flip(ToSize(pos));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static size_t ToSize(TEnum value)
		{
			return (size_t)Unsafe.As<TEnum, int>(ref value);
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public virtual EnumBitSet<TEnum> MakeDeepClone()
		{
			return new EnumBitSet<TEnum>(this);
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object into another object
		/// </summary>
		/********************************************************************/
		public void CopyTo(EnumBitSet<TEnum> destination)
		{
			_set.CopyTo(destination._set);
		}
	}
}
