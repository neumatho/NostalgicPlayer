/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Io_
	{
		/********************************************************************/
		/// <summary>
		/// Read the given number of bytes into the given span
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte_span2 ReadRaw(Stream f, CPointer<byte> data, size_t size)
		{
			return f.ReadRawImpl(data.AsSpan(size));
		}



		/********************************************************************/
		/// <summary>
		/// Read the given number of bytes into the given span
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte_span2 ReadRaw(Stream f, byte_span2 data)
		{
			return f.ReadRawImpl(data);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Read<TBinary>(Stream f, ref TBinary v) where TBinary : unmanaged
		{
			return ReadRaw(f, Memory.As_Raw_Memory(ref v)).Size() == Memory.As_Raw_Memory(ref v).Size();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Read(Stream f, array<byte> v)
		{
			return ReadRaw(f, v.data(), v.size()).Size() == v.size();
		}



		/********************************************************************/
		/// <summary>
		/// Read only the given number of bytes of an integer in
		/// little-endian format. The rest of the bytes are zero
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadBinaryTruncatedLE<T>(Stream f, out T v, size_t size)
		{
			array<uint8> bytes = new array<uint8>((size_t)Marshal.SizeOf<T>());

			size_t readResult = ReadRaw(f, bytes.data(), Math.Min(size, (size_t)Marshal.SizeOf<T>())).Size();
			bool result = readResult == Math.Min(size, (size_t)Marshal.SizeOf<T>());

			if (!BitConverter.IsLittleEndian)
				bytes.data().AsSpan().Reverse();

			v = Bit.Bit_Cast<T>(bytes.data().AsSpan());

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Read some kind of integer in little-endian format. If not all of
		/// the bytes could be read, false is returned and the missing high
		/// bytes are zero
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadIntLE<T>(Stream f, out T v)
		{
			array<uint8> bytes = new array<uint8>((size_t)Marshal.SizeOf<T>());

			size_t readResult = ReadRaw(f, bytes.data().AsSpan()).Size();
			bool result = readResult == (size_t)Marshal.SizeOf<T>();

			if (!BitConverter.IsLittleEndian)
				bytes.data().AsSpan().Reverse();

			v = Bit.Bit_Cast<T>(bytes.data().AsSpan());

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Read a 16-bit integer stored in adaptive little-endian format.
		/// The lowest bit of the first byte tells how many bytes in total
		/// are used (1 or 2)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadAdaptiveInt16LE(Stream f, out uint16 v)
		{
			bool result = true;
			v = 0;

			if (!ReadIntLE(f, out uint8 @byte))
				result = false;

			size_t additionalBytes = (size_t)(@byte & 0x01);
			v = (uint16)(@byte >> 1);

			for (size_t i = 0; i < additionalBytes; ++i)
			{
				if (!ReadIntLE(f, out @byte))
					result = false;

				v |= (uint16)(@byte << (int)(((i + 1) * 8) - 1));
			}

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Read a 32-bit integer stored in adaptive little-endian format.
		/// The two lowest bits of the first byte tell how many bytes in
		/// total are used (1 to 4)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadAdaptiveInt32LE(Stream f, out uint32 v)
		{
			bool result = true;
			v = 0;

			if (!ReadIntLE(f, out uint8 @byte))
				result = false;

			size_t additionalBytes = (size_t)(@byte & 0x03);
			v = (uint32)(@byte >> 2);

			for (size_t i = 0; i < additionalBytes; ++i)
			{
				if (!ReadIntLE(f, out @byte))
					result = false;

				v |= ((uint32)@byte << (int)(((i + 1) * 8) - 2));
			}

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Read a 64-bit integer stored in adaptive little-endian format.
		/// The two lowest bits of the first byte tell how many bytes in
		/// total are used (1, 2, 4 or 8)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadAdaptiveInt64LE(Stream f, out uint64 v)
		{
			bool result = true;
			v = 0;

			if (!ReadIntLE(f, out uint8 @byte))
				result = false;

			size_t additionalBytes = (size_t)((1 << (@byte & 0x03)) - 1);
			v = (uint64)(@byte >> 2);

			for (size_t i = 0; i < additionalBytes; ++i)
			{
				if (!ReadIntLE(f, out @byte))
					result = false;

				v |= ((uint64)@byte << (int)(((i + 1) * 8) - 2));
			}

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Read a string prefixed with its length stored as a little-endian
		/// integer of type Tsize
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadSizedStringLE<TSize>(Stream f, out StdString str) where TSize : IBinaryInteger<TSize>, IMinMaxValue<TSize>
		{
			return ReadSizedStringLE(f, out str, TSize.MaxValue);
		}



		/********************************************************************/
		/// <summary>
		/// Read a string prefixed with its length stored as a little-endian
		/// integer of type Tsize
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadSizedStringLE<TSize>(Stream f, out StdString str, TSize maxSize) where TSize : IBinaryInteger<TSize>
		{
			str = new StdString();

			if (!ReadIntLE(f, out TSize size))
				return false;

			if (size > maxSize)
				return false;

			for (TSize i = TSize.Zero; i != size; ++i)
			{
				if (!ReadIntLE(f, out uint8 c))
					return false;

				str.push_back(c);
			}

			return true;
		}
	}
}
