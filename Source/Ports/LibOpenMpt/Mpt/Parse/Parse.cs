/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Parse
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Parse_
	{
		/********************************************************************/
		/// <summary>
		/// The original is an if constexpr chain over T, where bool and
		/// the two character types read a wider integer and narrow it,
		/// while every other type is read straight out of the stream.
		/// The cases below mirror that one to one. Note that the value
		/// has to be converted to T before it is boxed, or the unboxing
		/// back to T throws
		/// </summary>
		/********************************************************************/
		public static T Parse_Or<T>(string str, T def)
		{
			switch (Type.GetTypeCode(typeof(T)))
			{
				case TypeCode.Boolean:
					return c_int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out c_int b) ? (T)(object)(b != 0) : def;

				case TypeCode.SByte:
					return c_int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out c_int sc) ? (T)(object)(c_char)sc : def;

				case TypeCode.Byte:
					return c_uint.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out c_uint uc) ? (T)(object)(c_uchar)uc : def;

				case TypeCode.Int16:
					return int16.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int16 s) ? (T)(object)s : def;

				case TypeCode.UInt16:
					return uint16.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint16 us) ? (T)(object)us : def;

				case TypeCode.Int32:
					return int32.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int32 i) ? (T)(object)i : def;

				case TypeCode.UInt32:
					return uint32.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint32 ui) ? (T)(object)ui : def;

				case TypeCode.Int64:
					return int64.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int64 l) ? (T)(object)l : def;

				case TypeCode.UInt64:
					return uint64.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint64 ul) ? (T)(object)ul : def;

				case TypeCode.Single:
					return c_float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out c_float f) ? (T)(object)f : def;

				case TypeCode.Double:
					return c_double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out c_double d) ? (T)(object)d : def;

				case TypeCode.String:
					return (T)(object)str;

				default:
					return def;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Parse<T>(string str)//XX 113
		{
			return Parse_Or<T>(str, default);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint32 Parse_Hex(string str)
		{
			c_int i = 0;

			// Skip leading white space, as the classic locale defines it
			while ((i < str.Length) && ((str[i] == ' ') || ((str[i] >= '\t') && (str[i] <= '\r'))))
				i++;

			// Skip the optional 0x prefix
			if (((i + 1) < str.Length) && (str[i] == '0') && ((str[i + 1] == 'x') || (str[i + 1] == 'X')))
				i += 2;

			uint64 value = 0;
			bool anyDigit = false;

			for (; i < str.Length; i++)
			{
				char c = str[i];
				uint32 digit;

				if ((c >= '0') && (c <= '9'))
					digit = (uint32)(c - '0');
				else if ((c >= 'a') && (c <= 'f'))
					digit = (uint32)(c - 'a' + 10);
				else if ((c >= 'A') && (c <= 'F'))
					digit = (uint32)(c - 'A' + 10);
				else
					break;

				anyDigit = true;
				value = (value * 16) + digit;

				// Overflow makes the istream fail, which leaves the value at zero
				if (value > uint32.MaxValue)
					return 0;
			}

			return anyDigit ? (uint32)value : 0;
		}
	}
}
