/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Endian;
using Utility = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io_Read
{
	/// <summary>
	/// 
	/// </summary>
	internal static class FileReader
	{
		/********************************************************************/
		/// <summary>
		/// Read a "T" object from the stream.
		/// If not enough bytes can be read, false is returned.
		/// If successful, the file cursor is advanced by the size of "T"
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Read<T>(FileCursor f, ref T target) where T : unmanaged
		{
			byte_span2 dst = Memory.As_Raw_Memory(ref target);

			if (dst.Size() != f.GetRaw(dst).Size())
				return false;

			f.Skip(dst.Size());

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read an array of binary-safe T values.
		/// If successful, the file cursor is advanced by the size of the
		/// array. Otherwise, the target is zeroed
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadArray<T>(FileCursor f, T[] destArray) where T : unmanaged
		{
			if (!f.CanRead((size_t)(Marshal.SizeOf<T>() * destArray.Length)))
			{
				Utility.Reset(destArray);
				return false;
			}

			f.ReadRaw(Memory.As_Raw_Memory(destArray));

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read an array of binary-safe T values.
		/// If successful, the file cursor is advanced by the size of the
		/// array. Otherwise, the target is zeroed
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadArray<T>(FileCursor f, array<T> destArray) where T : unmanaged
		{
			if (!f.CanRead((size_t)Marshal.SizeOf<T>() * destArray.size()))
			{
				Utility.Reset(destArray);
				return false;
			}

			f.ReadRaw(Memory.As_Raw_Memory(destArray));

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read an array of binary-safe T values.
		/// If successful, the file cursor is advanced by the size of the
		/// array. Otherwise, the target is zeroed
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadArray<T>(FileCursor f, CPointer<T> destArray) where T : unmanaged
		{
			if (!f.CanRead((size_t)(Marshal.SizeOf<T>() * destArray.Length)))
			{
				Utility.Reset(destArray);
				return false;
			}

			f.ReadRaw(Memory.As_Raw_Memory(destArray));

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read destSize elements of binary-safe T into a vector.
		/// If successful, the file cursor is advanced by the size of the
		/// vector. Otherwise, the vector is resized to destSize, but
		/// possibly existing contents are not cleared
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadVector<T>(FileCursor f, vector<T> destVector, size_t destSize) where T : unmanaged
		{
			destVector.resize(destSize);

			if (!f.CanRead((size_t)Marshal.SizeOf<T>() * destSize))
				return false;

			f.ReadRaw(Memory.As_Raw_Memory(destVector));

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static array<T> ReadArray<T>(FileCursor f, size_t destSize) where T : unmanaged
		{
			array<T> destArray = new array<T>(destSize);

			ReadArray<T>(f, destArray);

			return destArray;
		}



		/********************************************************************/
		/// <summary>
		/// Read some kind of integer in little-endian format.
		/// If successful, the file cursor is advanced by the size of the
		/// integer
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T ReadIntLE<T>(FileCursor f)
		{
			byte_span2 target = stackalloc uint8[Unsafe.SizeOf<T>()];

			if (target.Size() != f.GetRaw(target).Size())
				return default;

			f.Skip(target.Size());

			if (!BitConverter.IsLittleEndian)
				target.Reverse();

			return Bit.Bit_Cast<T>(target);
		}



		/********************************************************************/
		/// <summary>
		/// Read a integer in little-endian format which has some of its
		/// higher bytes not stored in file.
		/// If successful, the file cursor is advanced by the given size
		/// </summary>
		/********************************************************************/
		public static T ReadTruncatedIntLE<T>(FileCursor f, size_t size)
		{
			if (size == 0)
				return default;

			if (!f.CanRead(size))
				return default;

			bool IsSigned(Type t)
			{
				t = Nullable.GetUnderlyingType(t) ?? t;

				if (t.IsEnum)
					t = Enum.GetUnderlyingType(t);

				return t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISignedNumber<>));
			}

			bool isSigned = IsSigned(typeof(T));
			uint8[] buf = new uint8[Unsafe.SizeOf<T>()];
			bool negative = false;

			for (size_t i = 0; i < (size_t)Unsafe.SizeOf<T>(); ++i)
			{
				uint8 @byte = 0;

				if (i < size)
				{
					Read(f, ref @byte);
					negative = isSigned && ((@byte & 0x80) != 0x00);
				}
				else
				{
					// Sign or zero extend
					@byte = (uint8)(negative ? 0xff : 0x00);
				}

				buf[i] = @byte;
			}

			if (!BitConverter.IsLittleEndian)
				Array.Reverse(buf);

			return Bit.Bit_Cast<T>(buf);
		}



		/********************************************************************/
		/// <summary>
		/// Read a supplied-size little endian integer to a fixed size
		/// variable. The data is properly sign-extended when fewer bytes are
		/// stored. If more bytes are stored, higher order bytes are silently
		/// ignored.
		/// If successful, the file cursor is advanced by the given size
		/// </summary>
		/********************************************************************/
		public static T ReadSizedIntLE<T>(FileCursor f, size_t size)
		{
			if (size == 0)
				return default;

			if (!f.CanRead(size))
				return default;

			if (size < (size_t)Unsafe.SizeOf<T>())
				return ReadTruncatedIntLE<T>(f, size);

			T retVal = ReadIntLE<T>(f);
			f.Skip(size - (size_t)Unsafe.SizeOf<T>());

			return retVal;
		}



		/********************************************************************/
		/// <summary>
		/// Read unsigned 32-Bit integer in little-endian format.
		/// If successful, the file cursor is advanced by the size of the
		/// integer
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint32 ReadUInt32LE(FileCursor f)
		{
			uint32le target = new uint32le();

			if (!Read(f, ref target))
				return 0;

			return target;
		}



		/********************************************************************/
		/// <summary>
		/// Read unsigned 16-Bit integer in little-endian format.
		/// If successful, the file cursor is advanced by the size of the
		/// integer
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint16 ReadUInt16LE(FileCursor f)
		{
			uint16le target = new uint16le();

			if (!Read(f, ref target))
				return 0;

			return target;
		}



		/********************************************************************/
		/// <summary>
		/// Read unsigned 8-Bit integer.
		/// If successful, the file cursor is advanced by the size of the
		/// integer
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint8 ReadUInt8(FileCursor f)
		{
			uint8 target = 0;

			if (!Read(f, ref target))
				return 0;

			return target;
		}



		/********************************************************************/
		/// <summary>
		/// Read a struct.
		/// If successful, the file cursor is advanced by the size of the
		/// struct. Otherwise, the target is zeroed
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadStruct<T>(FileCursor f, ref T target) where T : unmanaged, IClearable
		{
			if (!Read(f, ref target))
			{
				target.Clear();
				return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Allow to read a struct partially (if there's less memory
		/// available than the struct's size, fill it up with zeros).
		/// The file cursor is advanced by "partialSize" bytes
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static size_t ReadStructPartial<T>(FileCursor f, ref T target, size_t? partialSize = null) where T : unmanaged
		{
			if (partialSize == null)
				partialSize = (size_t)Marshal.SizeOf<T>();

			size_t copyBytes = Math.Min(partialSize!.Value, (size_t)Marshal.SizeOf<T>());

			if (!f.CanRead(copyBytes))
				copyBytes = size_t.CreateSaturating(f.BytesLeft());

			f.GetRaw(Memory.As_Raw_Memory(ref target).Slice(0, (int)copyBytes));
			Memory.As_Raw_Memory(ref target).Slice((int)copyBytes).Clear();

			f.Skip(partialSize!.Value);

			return copyBytes;
		}



		/********************************************************************/
		/// <summary>
		/// Compare a magic string with the current stream position.
		/// Returns true if they are identical and advances the file cursor
		/// by the length of the "magic" string.
		/// Returns false if the string could not be found. The file cursor
		/// is not advanced in this case
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadMagic(FileCursor f, string magic)
		{
			size_t magicLength = (size_t)magic.Length;
			byte[] buffer = new byte[magicLength];

			if (f.GetRaw(buffer).Size() != magicLength)
				return false;

			if (CMemory.memcmp(buffer, magic, magicLength) != 0)
				return false;

			f.Skip(magicLength);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read variable-length unsigned integer (as found in MIDI files).
		/// If successful, the file cursor is advanced by the size of the
		/// integer and true is returned.
		/// False is returned if not enough bytes were left to finish reading
		/// of the integer or if an overflow happened (source doesn't fit
		/// into target integer).
		/// In case of an overflow, the target is also set to the maximum
		/// value supported by its data type
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadVarInt<T>(FileCursor f, out T target) where T : INumber<T>, IShiftOperators<T, c_int, T>, IBitwiseOperators<T, T, T>, IMinMaxValue<T>
		{
			if (f.NoBytesLeft())
			{
				target = default;
				return false;
			}

			byte[] bytes = new byte[16];	// More than enough for any valid VarInt
			size_t avail = f.GetRaw(bytes).Size();
			size_t readPos = 1;

			uint8 b = bytes[0];
			target = T.CreateTruncating(b & 0x7f);
			size_t writtenBits = (size_t)Bit.Bit_Width(target);	// Bits used in the most significant byte

			while ((readPos < avail) && ((b & 0x80) != 0))
			{
				b = bytes[readPos++];
				target <<= 7;
				target |= T.CreateTruncating(b & 0x7f);
				writtenBits += 7;

				if (readPos == avail)
				{
					f.Skip(readPos);
					avail = f.GetRaw(bytes).Size();
					readPos = 0;
				}
			}

			f.Skip(readPos);

			if (writtenBits > ((size_t)Marshal.SizeOf<T>() * 8U))
			{
				// Overflow
				target = T.MaxValue;
				return false;
			}
			else if ((b & 0x80) != 0)
			{
				// Reached EOF
				return false;
			}

			return true;
		}
	}
}
