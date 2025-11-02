/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Polycode.NostalgicPlayer.Kit.C
{
	/// <summary>
	/// C like string methods
	/// </summary>
	public static class CString
	{
		/********************************************************************/
		/// <summary>
		/// Calculates the length of a given string
		/// </summary>
		/********************************************************************/
		public static size_t strlen(CPointer<char> str)
		{
			if (str.IsNull)
				throw new ArgumentNullException(nameof(str));

			size_t len = 0;
			size_t maxLength = (size_t)str.Length;

			while ((len < maxLength) && (str[len] != '\0'))
				len++;

			return len;
		}



		/********************************************************************/
		/// <summary>
		/// Copies the C string pointed by source into the array pointed by
		/// destination, including the terminating null character (and
		/// stopping at that point)
		/// </summary>
		/********************************************************************/
		public static CPointer<char> strcpy(CPointer<char> destination, string source)
		{
			return strcpy(destination, source.ToCharPointer());
		}



		/********************************************************************/
		/// <summary>
		/// Copies the C string pointed by source into the array pointed by
		/// destination, including the terminating null character (and
		/// stopping at that point)
		/// </summary>
		/********************************************************************/
		public static CPointer<char> strcpy(CPointer<char> destination, CPointer<char> source)
		{
			if (destination.IsNull)
				throw new ArgumentNullException(nameof(destination));

			if (source.IsNull)
				throw new ArgumentNullException(nameof(source));

			c_int maxLength = Math.Min(source.Length, destination.Length - 1);
			c_int i;

			for (i = 0; (i < maxLength) && (source[i] != '\0'); i++)
				destination[i] = source[i];

			destination[i] = '\0';

			return destination;
		}



		/********************************************************************/
		/// <summary>
		/// Find the first occurrence of a character in a string. It checks
		/// whether the given character is present in the given string. If
		/// the character is found, it returns the pointer to its first
		/// occurrence otherwise, it returns a null pointer, indicating the
		/// character is not found in the string
		/// </summary>
		/********************************************************************/
		public static CPointer<char> strchr(CPointer<char> str, char ch)
		{
			if (str.IsNull)
				throw new ArgumentNullException(nameof(str));

			c_int maxLength = str.Length;
			c_int i = 0;

			while (i < maxLength)
			{
				if (str[i] == ch)
					return new CPointer<char>(str.Buffer, str.Offset + i);

				if (str[i] == '\0')
					break;

				i++;
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Compare two strings lexicographically
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int strcmp(CPointer<char> str1, string str2)
		{
			return strcmp(str1, str2.ToCharPointer());
		}



		/********************************************************************/
		/// <summary>
		/// Compare two strings lexicographically
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int strcmp(string str1, CPointer<char> str2)
		{
			return strcmp(str1.ToCharPointer(), str2);
		}



		/********************************************************************/
		/// <summary>
		/// Compare two strings lexicographically
		/// </summary>
		/********************************************************************/
		public static c_int strcmp(CPointer<char> str1, CPointer<char> str2)
		{
			if (str1.IsNull)
				throw new ArgumentNullException(nameof(str1));

			if (str2.IsNull)
				throw new ArgumentNullException(nameof(str2));

			int maxLength = Math.Min(str1.Length, str2.Length);

			for (int i = 0; i < maxLength; i++)
			{
				bool isStr1End = str1[i] == '\0';
				bool isStr2End = str2[i] == '\0';

				if (isStr1End && isStr2End)
					break;

				if (isStr1End)
					return -1;

				if (isStr2End)
					return 1;

				if (str1[i] != str2[i])
					return str1[i] - str2[i];
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the first n characters of two strings and returns an
		/// integer indicating which one is greater
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int strncmp(CPointer<char> str1, string str2, size_t count)
		{
			return strncmp(str1, str2.ToCharPointer(), count);
		}



		/********************************************************************/
		/// <summary>
		/// Compares the first n characters of two strings and returns an
		/// integer indicating which one is greater
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int strncmp(string str1, CPointer<char> str2, size_t count)
		{
			return strncmp(str1.ToCharPointer(), str2, count);
		}



		/********************************************************************/
		/// <summary>
		/// Compares the first n characters of two strings and returns an
		/// integer indicating which one is greater
		/// </summary>
		/********************************************************************/
		public static c_int strncmp(CPointer<char> str1, CPointer<char> str2, size_t count)
		{
			if (str1.IsNull)
				throw new ArgumentNullException(nameof(str1));

			if (str2.IsNull)
				throw new ArgumentNullException(nameof(str2));

			count = Math.Min(count, (size_t)Math.Min(str1.Length, str2.Length));

			for (size_t i = 0; i < count; i++)
			{
				bool isStr1End = str1[i] == '\0';
				bool isStr2End = str2[i] == '\0';

				if (isStr1End && isStr2End)
					break;

				if (isStr1End)
					return -1;

				if (isStr2End)
					return 1;

				if (str1[i] != str2[i])
					return str1[i] - str2[i];
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Interprets the contents of the string as a floating point number
		/// and return its value as a double. It sets a pointer to point to
		/// the first character after the last valid character of the string,
		/// only if there is any, otherwise it sets the pointer to null
		/// </summary>
		/********************************************************************/
		public static c_double strtod(CPointer<char> str, out CPointer<char> end)
		{
			if (str.IsNull)
				throw new ArgumentNullException(nameof(str));

			c_int i = 0;

			// Skip leading whitespace
			while ((str[i] != '\0') && char.IsWhiteSpace(str[i]))
				i++;

			c_int start = i;
			c_int expDigits;

			// Optional sign
			bool negative = false;

			if ((str[i] == '+') || (str[i] == '-'))
			{
				negative = str[i] == '-';
				i++;
			}

			// Handle INF / INFINITY (case-insensitive)
			if (str[i] != '\0')
			{
				if (MatchIgnoreCase(str, i, "inf"))
				{
					c_int j = i + 3;

					if (MatchIgnoreCase(str, i, "infinity"))
						j = i + 8; // length of "infinity"

					i = j;
					end = new CPointer<char>(str.Buffer, str.Offset + i);

					return negative ? c_double.NegativeInfinity : c_double.PositiveInfinity;
				}

				// Handle NAN (case-insensitive) with optional payload (ignored)
				if (MatchIgnoreCase(str, i, "nan"))
				{
					c_int j = i + 3;

					if (str[j] == '(') // payload
					{
						j++;

						while ((str[j] != '\0') && (str[j] != ')'))
							j++;

						if (str[j] == ')')
							j++;
					}

					i = j;
					end = new CPointer<char>(str.Buffer, str.Offset + i);

					return c_double.NaN;
				}
			}

			// Hexadecimal floating literal:0x[hex][.hex]?p[+/-]?dec
			// Only attempt if next chars are 0x /0X
			if ((str[i] == '0') && (str[i + 1] != '\0') && (str[i + 1] == 'x' || str[i + 1] == 'X'))
			{
				c_int oldI = i;
				i += 2;	// Skip 0x

				BigInteger mantissa = BigInteger.Zero;
				c_int fracDigits = 0;
				c_int digitsTotalHex = 0;
				bool sawDot = false;

				for (;;)
				{
					char c = str[i];
					if (c == '\0')
						break;

					if (c == '.')
					{
						if (sawDot)
							break; // second dot -> stop

						sawDot = true;
						i++;
						continue;
					}

					c_int dv = DigitValue(c);
					if ((dv < 0) || (dv >= 16))
						break;

					mantissa = mantissa * 16 + dv;
					digitsTotalHex++;

					if (sawDot)
						fracDigits++;

					i++;
				}

				if ((digitsTotalHex > 0) && ((str[i] == 'p') || (str[i] == 'P')))
				{
					// Parse binary exponent
					i++;	// skip 'p'
					bool expNeg = false;

					if ((str[i] == '+') || (str[i] == '-'))
					{
						expNeg = str[i] == '-';
						i++;
					}

					expDigits = 0;
					long expVal = 0;

					for (;;)
					{
						char c = str[i];
						if ((c == '\0') || !char.IsDigit(c))
							break;

						expVal = expVal * 10 + (c - '0');
						expDigits++;
						i++;
					}

					if (expDigits == 0)
					{
						// Invalid exponent -> treat as no conversion
						end = str;

						return 0.0;
					}

					if (expNeg)
						expVal = -expVal;

					// Effective binary exponent adjusts for fractional hex digits (each =4 bits)
					long effectiveExp = expVal - fracDigits * 4L;

					c_double value;

					if (mantissa.IsZero)
						value = 0.0;
					else
					{
						// Convert mantissa to double
						c_double mantD = (c_double)mantissa;
						value = mantD * CMath.pow(2.0, effectiveExp);
					}

					end = new CPointer<char>(str.Buffer, str.Offset + i);

					return negative ? -value : value;
				}

				if (digitsTotalHex > 0)
				{
					// Missing required exponent -> invalid hex float -> no conversion
					end = str;

					return 0.0;
				}

				// else no digits after 0x -> fall through to decimal parsing as no conversion
				i = oldI;
			}

			c_int digitsBefore = 0;
			c_int digitsAfter = 0;

			// Integer part digits
			while (char.IsDigit(str[i]))
			{
				digitsBefore++;
				i++;
			}

			// Fractional part
			if (str[i] == '.')
			{
				i++;

				while (char.IsDigit(str[i]))
				{
					digitsAfter++;
					i++;
				}
			}

			c_int digitsTotal = digitsBefore + digitsAfter;

			// Exponent part (only if we already saw digits)
			expDigits = 0;

			if ((digitsTotal > 0) && ((str[i] == 'e') || (str[i] == 'E')))
			{
				c_int expStart = i;	// Remember in case of rollback
				i++;

				if ((str[i] == '+') || (str[i] == '-'))
					i++;

				c_int expScan = i;

				while (char.IsDigit(str[expScan]))
				{
					expDigits++;
					expScan++;
				}

				if (expDigits == 0)
				{
					// Rollback, the 'e' was not part of number
					i = expStart;
				}
				else
					i = expScan;
			}

			if (digitsTotal == 0)
			{
				// No valid conversion
				end = str;

				return 0.0;
			}

			// Build substring for parsing
			c_int numberEnd = i;

			// Actually we want from start (including sign) -> numberEnd
			string numberString = new string(str.AsSpan().Slice(start, numberEnd - start));

			// Parse using invariant culture
			if (!c_double.TryParse(numberString, NumberStyles.Float, CultureInfo.InvariantCulture, out c_double parsed))
				parsed = 0;

			end = new CPointer<char>(str.Buffer, str.Offset + numberEnd);

			return parsed;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the length of the initial substring of the string
		/// pointed to by str1 that is made up of only those character
		/// contained in the string pointed to by str2
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static size_t strspn(CPointer<char> str1, string str2)
		{
			return strspn(str1, str2.ToCharPointer());
		}



		/********************************************************************/
		/// <summary>
		/// Returns the length of the initial substring of the string
		/// pointed to by str1 that is made up of only those character
		/// contained in the string pointed to by str2
		/// </summary>
		/********************************************************************/
		public static size_t strspn(CPointer<char> str1, CPointer<char> str2)
		{
			if (str1.IsNull)
				throw new ArgumentNullException(nameof(str1));

			if (str2.IsNull)
				throw new ArgumentNullException(nameof(str2));

			HashSet<char> acceptSet = new HashSet<char>(str2.AsSpan().ToArray());

			size_t i = 0;
			size_t maxLength = (size_t)str1.Length;

			while ((i < maxLength) && (str1[i] != '\0'))
			{
				if (!acceptSet.Contains(str1[i]))
					break;

				i++;
			}

			return i;
		}



		/********************************************************************/
		/// <summary>
		/// Converts the initial part of the string in str to a value
		/// according to the given base, which must be between 2 and 36
		/// inclusive, or be the special value 0. This function discard any
		/// white space characters until the first non-whitespace character
		/// is found, then takes as many characters as possible to form a
		/// valid base-n unsigned integer number representation and converts
		/// them to an integer value
		/// </summary>
		/********************************************************************/
		public static c_long strtol(CPointer<char> str, out CPointer<char> end, c_int @base, out bool error)
		{
			return strto<c_long>(str, out end, @base, out error);
		}



		/********************************************************************/
		/// <summary>
		/// Converts the initial part of the string in str to a value
		/// according to the given base, which must be between 2 and 36
		/// inclusive, or be the special value 0. This function discard any
		/// white space characters until the first non-whitespace character
		/// is found, then takes as many characters as possible to form a
		/// valid base-n unsigned integer number representation and converts
		/// them to an integer value
		/// </summary>
		/********************************************************************/
		public static c_ulong strtoul(CPointer<char> str, out CPointer<char> end, c_int @base, out bool error)
		{
			return strto<c_ulong>(str, out end, @base, out error);
		}



		/********************************************************************/
		/// <summary>
		/// Converts the initial part of the string in str to a value
		/// according to the given base, which must be between 2 and 36
		/// inclusive, or be the special value 0. This function discard any
		/// white space characters until the first non-whitespace character
		/// is found, then takes as many characters as possible to form a
		/// valid base-n unsigned integer number representation and converts
		/// them to an integer value
		/// </summary>
		/********************************************************************/
		public static c_long_long strtoll(CPointer<char> str, out CPointer<char> end, c_int @base, out bool error)
		{
			return strto<c_long_long>(str, out end, @base, out error);
		}



		/********************************************************************/
		/// <summary>
		/// Converts the initial part of the string in str to a value
		/// according to the given base, which must be between 2 and 36
		/// inclusive, or be the special value 0. This function discard any
		/// white space characters until the first non-whitespace character
		/// is found, then takes as many characters as possible to form a
		/// valid base-n unsigned integer number representation and converts
		/// them to an integer value
		/// </summary>
		/********************************************************************/
		public static c_ulong_long strtoull(CPointer<char> str, out CPointer<char> end, c_int @base, out bool error)
		{
			return strto<c_ulong_long>(str, out end, @base, out error);
		}



		/********************************************************************/
		/// <summary>
		/// Composes a string with the same text that would be printed if
		/// format was used on printf, but instead of being printed, the
		/// content is stored as a C string in the buffer pointed by s
		/// (taking n as the maximum buffer capacity to fill).
		/// 
		/// If the resulting string would be longer than n-1 characters, the
		/// remaining characters are discarded and not stored, but counted
		/// for the value returned by the function.
		///
		/// A terminating null character is automatically appended after the
		/// content written
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int snprintf(CPointer<char> s, size_t n, string format, params object[] args)
		{
			return snprintf(s, n, format.ToCharPointer(), args);
		}



		/********************************************************************/
		/// <summary>
		/// Composes a string with the same text that would be printed if
		/// format was used on printf, but instead of being printed, the
		/// content is stored as a C string in the buffer pointed by s
		/// (taking n as the maximum buffer capacity to fill).
		/// 
		/// If the resulting string would be longer than n-1 characters, the
		/// remaining characters are discarded and not stored, but counted
		/// for the value returned by the function.
		///
		/// A terminating null character is automatically appended after the
		/// content written
		/// </summary>
		/********************************************************************/
		public static c_int snprintf(CPointer<char> s, size_t n, CPointer<char> format, params object[] args)
		{
			if (s.IsNull)
				return 0;

			c_int maxLen = format.Length;	// Includes terminating '\0'
			c_int idx = 0;
			c_int argIndex = 0;

			StringBuilder sb = new StringBuilder();
			CultureInfo inv = CultureInfo.InvariantCulture;

			while (idx < maxLen)
			{
				char c = format[idx];
				if (c == '\0')
					break;

				if (c != '%')
				{
					sb.Append(c);
					idx++;
					continue;
				}

				// Handle %% -> '%'
				if (((idx + 1) < maxLen) && (format[idx + 1] == '%'))
				{
					sb.Append('%');
					idx += 2;
					continue;
				}

				idx++;	// Skip '%'

				// Flags
				bool leftAlign = false;
				bool forceSign = false;
				bool spaceSign = false;
				bool altForm = false;
				bool zeroPad = false;

				for (;;)
				{
					if (idx >= maxLen)
						break;

					char f = format[idx];

					if (f == '-')
					{
						leftAlign = true;
						idx++;
						continue;
					}

					if (f == '+')
					{
						forceSign = true;
						idx++;
						continue;
					}

					if (f == ' ')
					{
						spaceSign = true;
						idx++;
						continue;
					}

					if (f == '#')
					{
						altForm = true;
						idx++;
						continue;
					}

					if (f == '0')
					{
						zeroPad = true;
						idx++;
						continue;
					}

					break;
				}

				c_int? width = null;

				if ((idx < maxLen) && (format[idx] == '*'))
				{
					idx++;
					if (argIndex < args.Length)
						width = Convert.ToInt32(args[argIndex++], inv);
				}
				else
				{
					c_int wVal = 0;
					bool have = false;

					while ((idx < maxLen) && char.IsDigit(format[idx]))
					{
						have = true;
						wVal = (wVal * 10) + (format[idx] - '0');
						idx++;
					}

					if (have)
						width = wVal;
				}

				c_int? precision = null;

				if ((idx < maxLen) && (format[idx] == '.'))
				{
					idx++;	// Skip '.'

					if ((idx < maxLen) && (format[idx] == '*'))
					{
						idx++;

						if (argIndex < args.Length)
							precision = Convert.ToInt32(args[argIndex++], inv);
					}
					else
					{
						c_int pVal = 0;
						bool have = false;

						while ((idx < maxLen) && char.IsDigit(format[idx]))
						{
							have = true;
							pVal = (pVal * 10) + (format[idx] - '0');
						 idx++;
						}

						precision = have ? pVal : 0;	// "%." -> precision = 0
					}
				}

				// Length modifiers (consume only)
				if (idx < maxLen)
				{
					if (format[idx] == 'l')
					{
						idx++;

						if ((idx < maxLen) && (format[idx] == 'l'))
							idx++;	// ll
					}
					else if ((format[idx] == 'h') || (format[idx] == 'z') || (format[idx] == 't'))
						idx++;
				}

				if (idx >= maxLen)
					break;

				char spec = format[idx++];

				object argVal = null;

				if ((spec != 'n') && (spec != '%'))
				argVal = argIndex < args.Length ? args[argIndex++] : null;

				string formattedPart;

				switch (spec)
				{
					case 'd':
					case 'i':
					{
						formattedPart = FmtInt(argVal, true, width, precision, leftAlign, forceSign, spaceSign, zeroPad);
						break;
					}

					case 'u':
					{
						formattedPart = FmtInt(argVal, false, width, precision, leftAlign, false, false, zeroPad);
						break;
					}

					case 'x':
					{
						formattedPart = FmtHex(argVal, false, width, precision, leftAlign, zeroPad, altForm);
						break;
					}

					case 'X':
					{
						formattedPart = FmtHex(argVal, true, width, precision, leftAlign, zeroPad, altForm);
						break;
					}

					case 'o':
					{
						formattedPart = FmtOct(argVal, width, precision, leftAlign, zeroPad, altForm);
						break;
					}

					case 'c':
					{
						char ch = argVal is char chArg ? chArg : (char)Convert.ToInt32(argVal ?? 0, inv);
						formattedPart = ApplyWidth(ch.ToString(), width, leftAlign, ' ', 0);
						break;
					}

					case 's':
					{
						string str = argVal?.ToString() ?? "(null)";

						if (precision.HasValue && precision.Value < str.Length)
							str = str.Substring(0, precision.Value);

						formattedPart = ApplyWidth(str, width, leftAlign, ' ', 0);
						break;
					}

					case 'f':
					case 'F':
					{
						formattedPart = FmtFloat(argVal, width, precision ?? 6, leftAlign, forceSign, spaceSign, zeroPad, altForm, inv, 'f');
						break;
					}

					case 'e':
					case 'E':
					{
						formattedPart = FmtFloat(argVal, width, precision ?? 6, leftAlign, forceSign, spaceSign, zeroPad, altForm, inv, spec);
						break;
					}

					case 'g':
					case 'G':
					{
						// For %g/%G, default precision is 6 significant digits, minimum is 1
						c_int gPrecision = precision ?? 6;
						formattedPart = FmtFloat(argVal, width, gPrecision, leftAlign, forceSign, spaceSign, zeroPad, altForm, inv, spec);
						break;
					}

					case 'p':
					{
						if (argVal is IntPtr ip)
							formattedPart = "0x" + ip.ToInt64().ToString("x", inv);
						else
							formattedPart = "0x" + Convert.ToInt64(argVal ?? 0, inv).ToString("x", inv);

						formattedPart = ApplyWidth(formattedPart, width, leftAlign, zeroPad ? '0' : ' ', 2);
						break;
					}

					case 'n':
					{
						if (argVal is CPointer<int> iptr && iptr.IsNotNull)
							iptr[0] = sb.Length;

						formattedPart = string.Empty;
						break;
					}

					default:
					{
						formattedPart = "%" + spec;
						break;
					}
				}

				sb.Append(formattedPart);
			}

			string result = sb.ToString();
			int totalLen = result.Length;

			if (n > 0)
			{
				int toCopy = (int)Math.Min((ulong)totalLen, (n - 1));
				for (int i = 0; i < toCopy; i++)
					s[i] = result[i];

				s[toCopy] = '\0';
			}

			return totalLen;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int DigitValue(char c)
		{
			if (c >= '0' && c <= '9')
				return c - '0';

			if (c >= 'A' && c <= 'Z')
				return c - 'A' + 10;

			if (c >= 'a' && c <= 'z')
				return c - 'a' + 10;

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static bool MatchIgnoreCase(CPointer<char> str, int offset, string value)
		{
			for (int k = 0; k < value.Length; k++)
			{
				char c = str[offset + k];
				if (c == '\0')
					return false;

				if (char.ToLowerInvariant(c) != value[k])
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Converts the initial part of the string in str to a value
		/// according to the given base, which must be between 2 and 36
		/// inclusive, or be the special value 0. This function discard any
		/// white space characters until the first non-whitespace character
		/// is found, then takes as many characters as possible to form a
		/// valid base-n unsigned integer number representation and converts
		/// them to an integer value
		/// </summary>
		/********************************************************************/
		private static T strto<T>(CPointer<char> str, out CPointer<char> end, c_int @base, out bool error) where T : INumber<T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>, IAdditionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IModulusOperators<T, T, T>
		{
			if (str.IsNull)
				throw new ArgumentNullException(nameof(str));

			if ((@base != 0) && ((@base < 2) || (@base > 36)))
				throw new ArgumentOutOfRangeException(nameof(@base));

			error = false;

			c_int i = 0;

			// Skip leading whitespace
			while ((str[i] != '\0') && char.IsWhiteSpace(str[i]))
				i++;

			// Optional sign
			bool neg = false;

			if ((str[i] == '+') || (str[i] == '-'))
			{
				neg = str[i] == '-';
				i++;
			}

			// Base detection + 0x/0X
			c_int b = @base;

			if (((b == 0) || (b == 16)) && (str[i + 1] != '\0') && (str[i] == '0') && ((str[i + 1] == 'x') || (str[i + 1] == 'X')))
			{
				b = 16;
				i += 2;
			}

			if (b == 0)
				b = str[i] == '0' ? 8 : 10;

			T bType = T.CreateChecked(b);

			// Parse digits
			T acc = T.Zero;
			c_int any = 0;			// 0 = no digits, 1 = digits parsed, -1 = overflow

			// Compute limits for overflow detection
			T limit = T.MaxValue;
			T cutOff = limit / bType;
			T cutLim = limit % bType;

			for (; str[i] != '\0'; i++)
			{
				c_int digit = DigitValue(str[i]);
				if ((digit < 0) || (digit >= b))
					break;

				if (any < 0)
					continue;	// Already overflowed, just move i to proper end

				if ((acc > cutOff) || ((acc == cutOff) && (T.CreateChecked(digit) > cutLim)))
				{
					any = -1;
					acc = limit;	// Store saturated absolute value
				}
				else
				{
					any = 1;
					acc = (acc * bType) + T.CreateChecked(digit);
				}
			}

			if (any == 0)
			{
				end = str;	// No conversion

				return T.Zero;
			}

			T result;

			if (any < 0)
			{
				result = neg ? T.MinValue : T.MaxValue;
				error = true;
			}
			else
				result = neg ? -acc : acc;

			end = new CPointer<char>(str.Buffer, str.Offset + i);

			return result;
		}

		#region Snprintf Helpers
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static string FmtInt(object value, bool signed, c_int? width, c_int? precision, bool leftAlign, bool forceSign, bool spaceSign, bool zeroPad)
		{
			long lVal = value == null ? 0 : Convert.ToInt64(value, CultureInfo.InvariantCulture);
			bool neg = signed && lVal < 0;
			ulong absVal = neg ? (ulong)(-lVal) : (ulong)lVal;
			string digits = absVal.ToString("D", CultureInfo.InvariantCulture);

			if (precision.HasValue)
				digits = precision.Value == 0 ? string.Empty : absVal.ToString("D" + precision.Value, CultureInfo.InvariantCulture);

			string signStr = neg ? "-" : forceSign ? "+" : spaceSign ? " " : string.Empty;
			string composed = signStr + digits;
			char padChar = (zeroPad && !leftAlign && !precision.HasValue) ? '0' : ' ';

			return ApplyWidth(composed, width, leftAlign, padChar, signStr.Length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static string FmtHex(object value, bool upper, c_int? width, c_int? precision, bool leftAlign, bool zeroPad, bool altForm)
		{
			ulong uVal = value == null ? 0UL : Convert.ToUInt64(value, CultureInfo.InvariantCulture);
			string digits = uVal.ToString(upper ? "X" : "x", CultureInfo.InvariantCulture);

			if (precision.HasValue)
				digits = precision.Value == 0 ? string.Empty : uVal.ToString((upper ? "X" : "x") + precision.Value, CultureInfo.InvariantCulture);

			string prefix = altForm && (uVal != 0) ? (upper ? "0X" : "0x") : string.Empty;
			string composed = prefix + digits;
			char padChar = (zeroPad && !leftAlign && !precision.HasValue) ? '0' : ' ';

			return ApplyWidth(composed, width, leftAlign, padChar, prefix.Length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static string FmtOct(object value, c_int? width, c_int? precision, bool leftAlign, bool zeroPad, bool altForm)
		{
			ulong uVal = value == null ? 0UL : Convert.ToUInt64(value, CultureInfo.InvariantCulture);
			string digits = Convert.ToString((long)uVal, 8);

			if (precision.HasValue && (digits.Length < precision.Value))
				digits = new string('0', precision.Value - digits.Length) + digits;

			string prefix = (altForm && (digits.Length > 0) && (digits[0] != '0')) ? "0" : string.Empty;
			string composed = prefix + digits;
			char padChar = (zeroPad && !leftAlign && !precision.HasValue) ? '0' : ' ';

			return ApplyWidth(composed, width, leftAlign, padChar, prefix.Length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static string FmtFloat(object value, c_int? width, c_int precision, bool leftAlign, bool forceSign, bool spaceSign, bool zeroPad, bool altForm, CultureInfo inv, char spec)
		{
			c_double dVal = value == null ? 0.0 : Convert.ToDouble(value, inv);

			string fmt;

			if ((spec == 'g') || (spec == 'G'))
			{
				// For %g, precision is minimum 1 and specifies significant digits
				// Precision 0 is treated as 1
				c_int gPrecision = precision == 0 ? 1 : precision;

				// Use .NET's g format which handles significant digits
				fmt = (spec == 'g') ? dVal.ToString("g" + gPrecision, inv) : dVal.ToString("G" + gPrecision, inv);

				// %g removes trailing zeros and trailing decimal point (unless # flag is set)
				if (!altForm)
				{
					// .NET's "g" format already removes trailing zeros and decimal point
					// So we don't need to do anything extra
				}
				else
				{
					// With # flag, we need to keep the decimal point
					if (!fmt.Contains('.') && !fmt.Contains('e') && !fmt.Contains('E'))
						fmt += ".";
				}
			}
			else
			{
				fmt = spec switch
				{
					'e' => dVal.ToString("e" + precision, inv),
					'E' => dVal.ToString("E" + precision, inv),
					_ => dVal.ToString("f" + precision, inv)
				};

				// Handle alternate form (#) - force decimal point even with .0 precision
				if (altForm && (precision == 0) && !fmt.Contains('.'))
					fmt += ".";
			}

			// Normalize exponent to two digits like C (remove leading zero if three-digit small exponent)
			if ((spec == 'e') || (spec == 'E') || (spec == 'g') || (spec == 'G'))
			{
				c_int ePos = fmt.IndexOf('e');

				if (ePos < 0)
					ePos = fmt.IndexOf('E');

				if ((ePos >= 0) && ((ePos + 2) < fmt.Length))
				{
					string expDigits = fmt.Substring(ePos + 2);

					if ((expDigits.Length == 3) && (expDigits[0] == '0'))
						fmt = fmt.Substring(0, ePos + 2) + expDigits.Substring(1);
				}
			}

			if (dVal >= 0)
				fmt = forceSign ? "+" + fmt : spaceSign ? " " + fmt : fmt;

			char padChar = (zeroPad && !leftAlign) ? '0' : ' ';
			c_int signLen = (fmt.StartsWith("+") || fmt.StartsWith("-") || fmt.StartsWith(" ")) ? 1 : 0;

			return ApplyWidth(fmt, width, leftAlign, padChar, signLen);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static string ApplyWidth(string input, int? width, bool leftAlign, char padChar, int prefixLen)
		{
			if (!width.HasValue || (width.Value <= input.Length))
				return input;

			int padCount = width.Value - input.Length;
			if (leftAlign)
				return input + new string(' ', padCount);

			if ((padChar == '0') && (prefixLen > 0))
				return input.Substring(0, prefixLen) + new string('0', padCount) + input.Substring(prefixLen);

			return new string(padChar, padCount) + input;
		}
		#endregion

		#endregion
	}
}
