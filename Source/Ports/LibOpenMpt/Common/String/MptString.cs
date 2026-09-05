/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String
{
	/// <summary>
	/// 
	/// </summary>
	internal static class MptString
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static StringModeBufRefImpl ReadBuf(ReadWriteMode mode, array<uint8> buf)
		{
			return new StringModeBufRefImpl(buf.data(), buf.size(), mode);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static StringModeBufRefImpl WriteBuf(ReadWriteMode mode, CPointer<uint8> buf)
		{
			return new StringModeBufRefImpl(buf, buf.Size(), mode);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static StringBufRefImpl ReadAutoBuf(CPointer<uint8> buf)
		{
			return new StringBufRefImpl(buf, buf.Size());
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static StringBufRefImpl ReadAutoBuf(CPointer<uint8> buf, size_t size)
		{
			return new StringBufRefImpl(buf, size);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static StringBufRefImpl WriteAutoBuf(CPointer<uint8> buf)
		{
			return new StringBufRefImpl(buf, buf.Size());
		}



		/********************************************************************/
		/// <summary>
		/// Sets last character to null in the given buffer
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetNullTerminator(CPointer<uint8> buffer, size_t size)
		{
			buffer[size - 1] = 0x00;
		}



		/********************************************************************/
		/// <summary>
		/// Return default whitespaces
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static StdString Default_Whitespace()
		{
			StdString result = new StdString();
			result.reserve(4);

			result.push_back((uint8)' ');
			result.push_back((uint8)'\n');
			result.push_back((uint8)'\r');
			result.push_back((uint8)'\t');

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Remove whitespace at start of string
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static StdString Trim_Left(StdString str, StdString whitespace = null)
		{
			if (whitespace == null)
				whitespace = Default_Whitespace();

			// C++ takes the string by value, so work on a copy of it
			str = new StdString(str);

			size_t pos = str.find_first_not_of(whitespace);

			if (pos != StdString.npos)
				str.erase(str.begin(), str.begin() + pos);
			else if ((pos == StdString.npos) && (str.length() > 0) && (str.find_last_of(whitespace) == (str.length() - 1)))
				return new StdString();

			return str;
		}



		/********************************************************************/
		/// <summary>
		/// Remove whitespace at end of string
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static StdString Trim_Right(StdString str, StdString whitespace = null)
		{
			if (whitespace == null)
				whitespace = Default_Whitespace();

			// C++ takes the string by value, so work on a copy of it
			str = new StdString(str);

			size_t pos = str.find_last_not_of(whitespace);

			if (pos != StdString.npos)
				str.erase(str.begin() + pos + 1, str.end());
			else if ((pos == StdString.npos) && (str.length() > 0) && (str.find_first_of(whitespace) == 0))
				return new StdString();

			return str;
		}



		/********************************************************************/
		/// <summary>
		/// Remove whitespace at start and end of string
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static StdString Trim(StdString str, StdString whitespace = null)
		{
			return Trim_Right(Trim_Left(str, whitespace), whitespace);
		}
	}
}
