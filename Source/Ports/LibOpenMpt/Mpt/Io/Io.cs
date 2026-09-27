/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
		public static byte_span2 ReadRaw(Stream f, byte_span2 data)//XX 42
		{
			return f.ReadRawImpl(data);
		}



		/********************************************************************/
		/// <summary>
		/// Read some kind of integer in little-endian format. If not all of
		/// the bytes could be read, false is returned and the missing high
		/// bytes are zero
		/// </summary>
		/********************************************************************/
		public static bool ReadIntLE<T>(Stream f, out T v)//XX 101
		{
			byte_span2 bytes = stackalloc uint8[Marshal.SizeOf<T>()];
			bytes.Clear();

			bool result = ReadRaw(f, bytes).Size() == (size_t)Marshal.SizeOf<T>();

			if (!BitConverter.IsLittleEndian)
				bytes.Reverse();

			v = Bit.Bit_Cast<T>(bytes);

			return result;
		}
	}
}
