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
	/// 
	/// </summary>
	internal static class EnumExtension
	{
		/********************************************************************/
		/// <summary>
		/// Set one or more flags
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Set<T>(this ref T e, T flags) where T : struct, Enum
		{
			FromBits(ref e, ToBits(e) | ToBits(flags));
		}



		/********************************************************************/
		/// <summary>
		/// Set or clear one or more flags
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Set<T>(this ref T e, T flags, bool set) where T : struct, Enum
		{
			uint64 bits = ToBits(e);
			uint64 flagBits = ToBits(flags);

			FromBits(ref e, set ? (bits | flagBits) : (bits & ~flagBits));
		}



		/********************************************************************/
		/// <summary>
		/// Clear one or more flags
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Reset<T>(this ref T e) where T : struct, Enum
		{
			FromBits(ref e, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Clear one or more flags
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Reset<T>(this ref T e, T flags) where T : struct, Enum
		{
			FromBits(ref e, ToBits(e) & ~ToBits(flags));
		}



		/********************************************************************/
		/// <summary>
		/// Toggle one or more flags
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Flip<T>(this ref T e, T flags) where T : struct, Enum
		{
			FromBits(ref e, ToBits(e) ^ ToBits(flags));
		}



		/********************************************************************/
		/// <summary>
		/// Test if one or more flags are set. Returns true if at least one
		/// of the given flags is set
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Test<T>(this T e, T flags) where T : struct, Enum
		{
			return (ToBits(e) & ToBits(flags)) != 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Read the raw bits of the value, widened to 64 bit
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint64 ToBits<T>(T value) where T : struct, Enum
		{
			switch (Unsafe.SizeOf<T>())
			{
				case 1:
					return Unsafe.As<T, uint8>(ref value);

				case 2:
					return Unsafe.As<T, uint16>(ref value);

				case 4:
					return Unsafe.As<T, uint32>(ref value);

				default:
					return Unsafe.As<T, uint64>(ref value);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Write the raw bits back, truncated to the width of the enum
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void FromBits<T>(ref T target, uint64 bits) where T : struct, Enum
		{
			switch (Unsafe.SizeOf<T>())
			{
				case 1:
				{
					Unsafe.As<T, uint8>(ref target) = (uint8)bits;
					break;
				}

				case 2:
				{
					Unsafe.As<T, uint16>(ref target) = (uint16)bits;
					break;
				}

				case 4:
				{
					Unsafe.As<T, uint32>(ref target) = (uint32)bits;
					break;
				}

				default:
				{
					Unsafe.As<T, uint64>(ref target) = bits;
					break;
				}
			}
		}
        #endregion
	}
}
