/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp
{
	/// <summary>
	/// Helper class to do endianess
	/// </summary>
	internal static class SidEndian
	{
		#region Int16 methods
		/********************************************************************/
		/// <summary>
		/// Set the low byte (8 bit) in a word (16 bit)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Endian_16Lo8(ref uint_least16_t word, uint8_t @byte)
		{
			word &= 0xff00;
			word |= @byte;
		}



		/********************************************************************/
		/// <summary>
		/// Get the low byte (8 bit) in a word (16 bit)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint8_t Endian_16Lo8(uint_least16_t word)
		{
			return (uint8_t)word;
		}



		/********************************************************************/
		/// <summary>
		/// Set the high byte (8 bit) in a word (16 bit)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Endian_16Hi8(ref uint_least16_t word, uint8_t @byte)
		{
			word &= 0x00ff;
			word |= (uint_least16_t)(@byte << 8);
		}



		/********************************************************************/
		/// <summary>
		/// Get the high byte (8 bit) in a word (16 bit)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint8_t Endian_16Hi8(uint_least16_t word)
		{
			return (uint8_t)(word >> 8);
		}



		/********************************************************************/
		/// <summary>
		/// Convert high-byte and low-byte to 16-bit word
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint_least16_t Endian_16(uint8_t hi, uint8_t lo)
		{
			uint_least16_t word = 0;

			Endian_16Lo8(ref word, lo);
			Endian_16Hi8(ref word, hi);

			return word;
		}



		/********************************************************************/
		/// <summary>
		/// Convert high-byte and low-byte to 16-bit little endian word
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint_least16_t Endian_Little16(uint8_t[] ptr, uint offset)
		{
			return Endian_16(ptr[offset + 1], ptr[offset]);
		}



		/********************************************************************/
		/// <summary>
		/// Write a little-endian 16-bit word to two bytes in memory
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Endian_Little16(uint8_t[] ptr, uint offset, uint_least16_t word)
		{
			ptr[offset] = Endian_16Lo8(word);
			ptr[offset + 1] = Endian_16Hi8(word);
		}



		/********************************************************************/
		/// <summary>
		/// Convert high-byte and low-byte to 16-bit big endian word
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint_least16_t Endian_Big16(uint8_t[] ptr, uint offset)
		{
			return Endian_16(ptr[offset], ptr[offset + 1]);
		}
		#endregion

		#region Int32 methods
		/********************************************************************/
		/// <summary>
		/// Set the high word (16 bit) in a dword (32 bit)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Endian_32Hi16(ref uint_least32_t dword, uint_least16_t word)
		{
			dword &= 0x0000ffff;
			dword |= (uint_least32_t)word << 16;
		}



		/********************************************************************/
		/// <summary>
		/// Set the low byte (8 bit) in a dword (32 bit)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Endian_32Lo8(ref uint_least32_t dword, uint8_t @byte)
		{
			dword &= 0xffffff00;
			dword |= @byte;
		}



		/********************************************************************/
		/// <summary>
		/// Set the high byte (8 bit) in a dword (32 bit)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Endian_32Hi8(ref uint_least32_t dword, uint8_t @byte)
		{
			dword &= 0xffff00ff;
			dword |= (uint_least32_t)@byte << 8;
		}



		/********************************************************************/
		/// <summary>
		/// Convert high-byte and low-byte to 32-bit word
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint_least32_t Endian_32(uint8_t hihi, uint8_t hilo, uint8_t hi, uint8_t lo)
		{
			uint_least32_t dword = 0;
			uint_least16_t word = 0;

			Endian_32Lo8(ref dword, lo);
			Endian_32Hi8(ref dword, hi);
			Endian_16Lo8(ref word, hilo);
			Endian_16Hi8(ref word, hihi);
			Endian_32Hi16(ref dword, word);

			return dword;
		}



		/********************************************************************/
		/// <summary>
		/// Convert high-byte and low-byte to 32-bit big endian word
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint_least32_t Endian_Big32(uint8_t[] ptr, int offset)
		{
			return Endian_32(ptr[offset], ptr[offset + 1], ptr[offset + 2], ptr[offset + 3]);
		}
		#endregion
	}
}
