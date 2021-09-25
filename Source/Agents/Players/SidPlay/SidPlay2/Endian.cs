/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2
{
	/// <summary>
	/// Helper class to do endianess
	/// </summary>
	internal static class Endian
	{
		#region Int16 methods
		/********************************************************************/
		/// <summary>
		/// Set the low byte (8 bit) in a word (16 bit)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort Endian16Lo8(ref ushort word, byte @byte)
		{
			return word = (ushort)((word & 0xff00) | @byte);
		}



		/********************************************************************/
		/// <summary>
		/// Get the low byte (8 bit) in a word (16 bit)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte Endian16Lo8(ushort word)
		{
			return (byte)word;
		}



		/********************************************************************/
		/// <summary>
		/// Set the high byte (8 bit) in a word (16 bit)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort Endian16Hi8(ref ushort word, byte @byte)
		{
			return word = (ushort)((@byte << 8) | Endian16Lo8(word));
		}



		/********************************************************************/
		/// <summary>
		/// Get the high byte (8 bit) in a word (16 bit)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte Endian16Hi8(ushort word)
		{
			return (byte)(word >> 8);
		}



		/********************************************************************/
		/// <summary>
		/// Convert high-byte and low-byte to 16-bit word
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort Endian16(byte hi, byte lo)
		{
			ushort word = 0;

			Endian16Lo8(ref word, lo);
			Endian16Hi8(ref word, hi);

			return word;
		}



		/********************************************************************/
		/// <summary>
		/// Convert high-byte and low-byte to 16-bit little endian word
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Endian16(sbyte[] ptr, uint offset, ushort word)
		{
			ptr[offset] = (sbyte)Endian16Lo8(word);
			ptr[offset + 1] = (sbyte)Endian16Hi8(word);
		}



		/********************************************************************/
		/// <summary>
		/// Convert high-byte and low-byte to 16-bit little endian word
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort EndianLittle16(byte[] ptr, uint offset)
		{
			return Endian16(ptr[offset + 1], ptr[offset]);
		}



		/********************************************************************/
		/// <summary>
		/// Write a little-endian 16-bit word to two bytes in memory
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void EndianLittle16(byte[] ptr, int offset, ushort word)
		{
			ptr[offset] = Endian16Lo8(word);
			ptr[offset + 1] = Endian16Hi8(word);
		}



		/********************************************************************/
		/// <summary>
		/// Convert high-byte and low-byte to 16-bit big endian word
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort EndianBig16(byte[] ptr, int offset)
		{
			return Endian16(ptr[offset], ptr[offset + 1]);
		}
		#endregion

		#region Int32 methods
		/********************************************************************/
		/// <summary>
		/// Set the low word (16 bit) in a dword (32 bit)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint Endian32Lo16(ref uint dword, ushort word)
		{
			return dword = (dword & 0xffff0000) | word;
		}



		/********************************************************************/
		/// <summary>
		/// Get the low word (16 bit) in a dword (32 bit)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort Endian32Lo16(uint dword)
		{
			return (ushort)(dword & 0xffff);
		}



		/********************************************************************/
		/// <summary>
		/// Set the high word (16 bit) in a dword (32 bit)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint Endian32Hi16(ref uint dword, ushort word)
		{
			return dword = (uint)word << 16 | Endian32Lo16(dword);
		}



		/********************************************************************/
		/// <summary>
		/// Get the high word (16 bit) in a dword (32 bit)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort Endian32Hi16(uint dword)
		{
			return (ushort)(dword >> 16);
		}



		/********************************************************************/
		/// <summary>
		/// Set the low byte (8 bit) in a dword (32 bit)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Endian32Lo8(ref uint dword, byte @byte)
		{
			dword &= 0xffffff00;
			dword |= @byte;
		}



		/********************************************************************/
		/// <summary>
		/// Get the low byte (8 bit) in a dword (32 bit)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte Endian32Lo8(uint dword)
		{
			return (byte)dword;
		}



		/********************************************************************/
		/// <summary>
		/// Set the high byte (8 bit) in a dword (32 bit)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Endian32Hi8(ref uint dword, byte @byte)
		{
			dword &= 0xffff00ff;
			dword |= (uint)@byte << 8;
		}



		/********************************************************************/
		/// <summary>
		/// Get the high byte (8 bit) in a dword (32 bit)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte Endian32Hi8(uint dword)
		{
			return (byte)(dword >> 8);
		}



		/********************************************************************/
		/// <summary>
		/// Convert high-byte and low-byte to 32-bit word
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint Endian32(byte hihi, byte hilo, byte hi, byte lo)
		{
			uint dword = 0;
			ushort word = 0;

			Endian32Lo8(ref dword, lo);
			Endian32Hi8(ref dword, hi);
			Endian16Lo8(ref word, hilo);
			Endian16Hi8(ref word, hihi);
			Endian32Hi16(ref dword, word);

			return dword;
		}



		/********************************************************************/
		/// <summary>
		/// Convert high-byte and low-byte to 32-bit big endian word
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint EndianBig32(byte[] ptr, int offset)
		{
			return Endian32(ptr[offset], ptr[offset + 1], ptr[offset + 2], ptr[offset + 3]);
		}
		#endregion
	}
}
