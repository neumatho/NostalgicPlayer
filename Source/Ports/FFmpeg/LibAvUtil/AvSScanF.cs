/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class AvSScanF
	{
		private enum Size
		{
			hh = -2,
			h = -1,
			def = 0,
			l = 1,
			L = 2,
			ll = 3,
		}

		private const c_int Eof = -1;

		private const c_int Flt_Mant_Dig = 24;
		private const c_int Flt_Min_Exp = -125;

		private const c_int Dbl_Mant_Dig = 53;
		private const c_int Dbl_Min_Exp = -1021;

		private const c_int Ld_B1B_Dig = 2;
		private const c_int Ld_B1B_Max1 = 9007199;
		private const c_int Ld_B1B_Max2 = 254740991;
		private const c_int KMax = 128;
		private const c_int Mask = KMax - 1;

		private static readonly c_uchar[] table =
		[
			255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 255, 255, 255, 255, 255, 255,
			255, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
			25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 255, 255, 255, 255, 255,
			255, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
			25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Av_SScanF(CPointer<char> @string, string format, object[] results)//XX 961
		{
			return Av_SScanF(@string, format.ToCharPointer(), results);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Av_SScanF(CPointer<char> @string, CPointer<char> format, object[] results)//XX 961
		{
			c_int ret = FF_VSScanF(@string, format, results);

			return ret;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ptrdiff_t ShCnt(FFFile f)//XX 54
		{
			return f.ShCnt + (f.RPos - f.Buf);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int FFToRead(FFFile f)//XX 56
		{
			f.RPos = f.REnd = f.Buf + f.Buf_Size;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static size_t FFString_Read(FFFile f, CPointer<char> buf, size_t len)//XX 62
		{
			CPointer<char> src = (CPointer<char>)f.Cookie;
			size_t k = len + 256;
			CPointer<char> end = CMemory.memchr(src, '\0', k);

			if (end.IsNotNull)
				k = (size_t)(end - src);

			if (k < len)
				len = k;

			CMemory.memcpy(buf, src, len);

			f.RPos = src + len;
			f.REnd = src + k;
			f.Cookie = src + k;

			return len;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int FFUFlow(FFFile f)//XX 78
		{
			CPointer<char> c = new CPointer<char>(1);

			if ((FFToRead(f) == 0) && (f.Read(f, c, 1) == 1))
				return c[0];

			return Eof;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void FFShLim(FFFile f, c_int lim)//XX 85
		{
			f.ShLim = lim;
			f.ShCnt = f.Buf - f.RPos;

			// If lim is nonzero, rend must be a valid pointer
			if ((lim != 0) && ((f.REnd - f.RPos) > lim))
				f.ShEnd = f.RPos + lim;
			else
				f.ShEnd = f.REnd;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int FFShGetC(FFFile f)//XX 96
		{
			c_int c;

			ptrdiff_t cnt = ShCnt(f);

			if (((f.ShLim != 0) && (cnt >= f.ShLim)) || ((c = FFUFlow(f)) < 0))
			{
				f.ShCnt = f.Buf - f.RPos + cnt;
				f.ShEnd.SetToNull();

				return Eof;
			}

			cnt++;

			if ((f.ShLim != 0) && ((f.REnd - f.RPos) > (f.ShLim - cnt)))
				f.ShEnd = f.RPos + (f.ShLim - cnt);
			else
				f.ShEnd = f.REnd;

			f.ShCnt = f.Buf - f.RPos + cnt;

			if (f.RPos[-1] != c)
				f.RPos[-1] = (char)c;

			return c;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ShLim(FFFile f, c_int lim)//XX 115
		{
			FFShLim(f, lim);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_int ShGetC(FFFile f)//XX 116
		{
			return f.RPos < f.ShEnd ? f.RPos[0, 1] : FFShGetC(f);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ShUnget(FFFile f)//XX 117
		{
			if (f.ShEnd.IsNotNull)
				f.RPos--;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_ulong_long FFIntScan(FFFile f, c_uint @base, c_int pok, c_ulong_long lim)//XX 138
		{
			CPointer<c_uchar> val = new CPointer<c_uchar>(table, 1);
			c_int c, neg = 0;
			c_uint x;
			c_ulong_long y;
			c_int[] bsTable = [ 0, 1, 2, 4, 7, 3, 6, 5 ];

			if ((@base > 36) || (@base == 1))
				return 0;

			while (AvString.Av_IsSpace((c = ShGetC(f))))
			{
			}

			if ((c == '+') || (c == '-'))
			{
				neg = -(c == '-' ? 1 : 0);
				c = ShGetC(f);
			}

			if (((@base == 0) || (@base == 16)) && (c == '0'))
			{
				c = ShGetC(f);

				if ((c | 32) == 'x')
				{
					c = ShGetC(f);

					if (val[c] >= 16)
					{
						ShUnget(f);

						if (pok != 0)
							ShUnget(f);
						else
							ShLim(f, 0);

						return 0;
					}

					@base = 16;
				}
				else if (@base == 0)
					@base = 8;
			}
			else
			{
				if (@base == 0)
					@base = 10;

				if (val[c] >= @base)
				{
					ShUnget(f);
					ShLim(f, 0);

					return 0;
				}
			}

			if (@base == 10)
			{
				for (x = 0; ((c_uint)(c - '0') < 10U) && (x <= ((c_uint.MaxValue / 10) - 1)); c = ShGetC(f))
					x = (c_uint)((x * 10) + (c - '0'));

				for (y = x; ((c_uint)(c - '0') < 10U) && (y <= (c_ulong_long.MaxValue / 10)) && ((10 * y) <= (c_ulong_long.MaxValue - (c_ulong_long)(c - '0'))); c = ShGetC(f))
					y = (y * 10U) + (c_ulong_long)(c - '0');

				if ((c_uint)(c - '0') >= 10U)
					goto Done;
			}
			else if ((@base & (@base - 1)) == 0)
			{
				c_int bs = bsTable[((0x17 * @base) >> 5) & 7];

				for (x = 0; (val[c] < @base) && (x <= (c_uint.MaxValue / 32)); c = ShGetC(f))
					x = (x << bs) | val[c];

				for (y = x; (val[c] < @base) && (y <= (c_ulong_long.MaxValue >> bs)); c = ShGetC(f))
					y = (y << bs) | val[c];
			}
			else
			{
				for (x = 0; (val[c] < @base) && (x <= ((c_uint.MaxValue / 36) - 1)); c = ShGetC(f))
					x = (x * @base) + val[c];

				for (y = x; (val[c] < @base) && (y <= (c_ulong_long.MaxValue / @base)) && ((@base * y) <= (c_ulong_long.MaxValue - val[c])); c = ShGetC(f))
					y = (y * @base) + val[c];
			}

			if (val[c] < @base)
			{
				for (; val[c] < @base; c = ShGetC(f))
				{
				}

				y = lim;

				if ((lim & 1) != 0)
					neg = 0;
			}

			Done:
			ShUnget(f);

			if (y >= lim)
			{
				if (((lim & 1) == 0) && (neg == 0))
					return lim - 1;
				else if (y > lim)
					return lim;
			}

			return (y ^ (c_ulong_long)neg) - (c_ulong_long)neg;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_long_long ScanExp(FFFile f, c_int pok)//XX 214
		{
			c_int x;
			c_long_long y;
			c_int neg = 0;

			c_int c = ShGetC(f);

			if ((c == '+') || (c == '-'))
			{
				neg = (c == '-' ? 1 : 0);
				c = ShGetC(f);

				if (((c - '0') >= 10) && (pok != 0))
					ShUnget(f);
			}

			if ((c - '0') >= 10)
			{
				ShUnget(f);

				return c_long_long.MinValue;
			}

			for (x = 0; ((c - '0') < 10) && (x < (c_int.MaxValue / 10)); c = ShGetC(f))
				x = 10 * x + (c - '0');

			for (y = x; ((c - '0') < 10) && (y < (c_long_long.MaxValue / 100)); c = ShGetC(f))
				y = 10 * y + (c - '0');

			for (; (c - '0') < 10; c = ShGetC(f))
			{
			}

			ShUnget(f);

			return neg != 0 ? -y : y;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_double DecFloat(FFFile f, c_int c, c_int bits, c_int eMin, c_int sign, c_int pok)//XX 245
		{
			uint32_t[] x = new uint32_t[KMax];
			uint32_t[] th = [ Ld_B1B_Max1, Ld_B1B_Max2 ];
			c_int i;
			c_long_long lrp = 0, dc = 0;
			c_long_long e10 = 0;
			c_int lnz = 0;
			c_int gotDig = 0, gotRad = 0;
			c_int eMax = -eMin - bits + 3;
			c_int denormal = 0;
			c_double y;
			c_double frac = 0;
			c_double bias = 0;
			c_int[] p10s = [ 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000 ];

			c_int j = 0;
			c_int k = 0;

			// Don't let leading zeros consume buffer space
			for (; c == '0'; c = ShGetC(f))
				gotDig = 1;

			if (c == '.')
			{
				gotRad = 1;
				for (c = ShGetC(f); c == '0'; c = ShGetC(f))
				{
					gotDig = 1;
					lrp--;
				}
			}

			x[0] = 0;

			for (; ((c - '0') < 10) || (c == '.'); c = ShGetC(f))
			{
				if (c == '.')
				{
					if (gotRad != 0)
						break;

					gotRad = 1;
					lrp = dc;
				}
				else if (k < (KMax - 3))
				{
					dc++;

					if (c != '0')
						lnz = (c_int)dc;

					if (j != 0)
						x[k] = (uint32_t)(x[k] * 10 + c - '0');
					else
						x[k] = (uint32_t)(c - '0');

					if (++j == 9)
					{
						k++;
						j = 0;
					}

					gotDig = 1;
				}
				else
				{
					dc++;

					if (c != '0')
					{
						lnz = (KMax - 4) * 9;
						x[KMax - 4] |= 1;
					}
				}
			}

			if (gotRad == 0)
				lrp = dc;

			if ((gotDig != 0) && ((c | 32) == 'e'))
			{
				e10 = ScanExp(f, pok);

				if (e10 == c_long_long.MinValue)
				{
					if (pok != 0)
						ShUnget(f);
					else
					{
						ShLim(f, 0);

						return 0;
					}

					e10 = 0;
				}

				lrp += 10;
			}
			else if (c >= 0)
				ShUnget(f);

			if (gotDig == 0)
			{
				ShLim(f, 0);

				return 0;
			}

			// Handle zero specially to avoid nasty special cases later
			if (x[0] == 0)
				return sign * 0.0;

			// Optimize small integers (w/no exponent) and over/under-flow
			if ((lrp == dc) && (dc < 10) && ((bits > 30) || ((x[0] >> bits) == 0)))
				return sign * (c_double)x[0];

			if (lrp > (-eMin / 2))
				return sign * CMath.DBL_MAX * CMath.DBL_MAX;

			if (lrp < (eMin - (2 * Dbl_Mant_Dig)))
				return sign * CMath.DBL_MIN * CMath.DBL_MIN;

			// Align incomplete final B1B digit
			if (j != 0)
			{
				for (; j < 9; j++)
					x[k] *= 10;

				k++;
				j = 0;
			}

			c_int a = 0;
			c_int z = k;
			c_int e2 = 0;
			c_int rp = (c_int)lrp;

			// Optimize small to mid-size integers (even in exp. notation)
			if ((lnz < 9) && (lnz <= rp) && (rp < 18))
			{
				if (rp == 9)
					return sign * (c_double)x[0];

				if (rp < 9)
					return sign * (c_double)x[0] / p10s[8 - rp];

				c_int bitLim = bits - 3 * (rp - 9);

				if ((bitLim > 30) || ((x[0] >> bitLim) == 0))
					return sign * (c_double)x[0] * p10s[rp - 10];
			}

			// Drop trailing zeros
			for (; x[z - 1] == 0; z--)
			{
			}

			// Align radix point to B1B digit boundary
			if ((rp % 9) != 0)
			{
				c_int rpm9 = rp >= 0 ? rp % 9 : rp % 9 + 9;
				c_int p10 = p10s[8 - rpm9];
				uint32_t carry = 0;

				for (k = a; k != z; k++)
				{
					uint32_t tmp = (uint32_t)(x[k] % p10);
					x[k] = (uint32_t)(x[k] / p10 + carry);
					carry = (uint32_t)(1000000000 / p10 * tmp);

					if ((k == a) && (x[k] == 0))
					{
						a = (a + 1 & Mask);
						rp -= 9;
					}
				}

				if (carry != 0)
					x[z++] = carry;

				rp += 9 - rpm9;
			}

			// Upscale until desired number of bits are left of radix point
			while ((rp < (9 * Ld_B1B_Dig)) || ((rp == (9 * Ld_B1B_Dig)) && (x[a] < th[0])))
			{
				uint32_t carry = 0;
				e2 -= 29;

				for (k = (z - 1 & Mask); ; k = (k - 1 & Mask))
				{
					uint64_t tmp = ((uint64_t)x[k] << 29) + carry;

					if (tmp > 1000000000)
					{
						carry = (uint32_t)(tmp / 1000000000);
						x[k] = (uint32_t)(tmp % 1000000000);
					}
					else
					{
						carry = 0;
						x[k] = (uint32_t)tmp;
					}

					if ((k == ((z - 1) & Mask)) && (k != a) && (x[k] == 0))
						z = k;

					if (k == a)
						break;
				}

				if (carry != 0)
				{
					rp += 9;
					a = (a - 1 & Mask);

					if (a == z)
					{
						z = (z - 1 & Mask);
						x[z - 1 & Mask] |= x[z];
					}

					x[a] = carry;
				}
			}

			// Downscale until exactly number of bits are left of radix point
			for (;;)
			{
				uint32_t carry = 0;
				c_int sh = 1;

				for (i = 0; i < Ld_B1B_Dig; i++)
				{
					k = (a + i & Mask);

					if ((k == z) || (x[k] < th[i]))
					{
						i = Ld_B1B_Dig;
						break;
					}

					if (x[a + i & Mask] > th[i])
						break;
				}

				if ((i == Ld_B1B_Dig) && (rp == (9 * Ld_B1B_Dig)))
					break;

				// FIXME: Find a way to compute optimal sh
				if (rp > (9 + 9 * Ld_B1B_Dig))
					sh = 9;

				e2 += sh;

				for (k = a; k != z; k = (k + 1 & Mask))
				{
					uint32_t tmp = (uint32_t)(x[k] & (1 << sh) - 1);
					x[k] = (x[k] >> sh) + carry;
					carry = (uint32_t)((1000000000 >> sh) * tmp);

					if ((k == a) && (x[k] == 0))
					{
						a = (a + 1 & Mask);
						i--;
						rp -= 9;
					}
				}

				if (carry != 0)
				{
					if (((z + 1) & Mask) == a)
					{
						x[z] = carry;
						z = (z + 1 & Mask);
					}
					else
						x[z - 1 & Mask] |= 1;
				}
			}

			// Assemble desired bits into floating point variable
			for (y = i = 0; i < Ld_B1B_Dig; i++)
			{
				if ((a + i & Mask) == z)
					x[(z = (z + 1 & Mask)) - 1] = 0;

				y = 1000000000.0 * y + x[a + 1 & Mask];
			}

			y *= sign;

			// Limit precision for denormal results
			if (bits > (Dbl_Mant_Dig + e2 - eMin))
			{
				bits = Dbl_Mant_Dig + e2 - eMin;

				if (bits < 0)
					bits = 0;

				denormal = 1;
			}

			// Calculate bias term to force rounding, move out lower bits
			if (bits < Dbl_Mant_Dig)
			{
				bias = CMath.copysign(CMath.scalbn(1, 2 * Dbl_Mant_Dig - bits - 1), y);
				frac = CMath.fmod(y, CMath.scalbn(1, Dbl_Mant_Dig - bits));
				y -= frac;
				y += bias;
			}

			// Process tail of decimal input so it can affect rounding
			if ((a + i & Mask) != z)
			{
				uint32_t t = x[a + i & Mask];

				if ((t < 500000000) && ((t != 0) || ((a + i + 1 & Mask) != z)))
					frac += 0.25 * sign;
				else if (t > 500000000)
					frac += 0.75 * sign;
				else if (t == 500000000)
				{
					if ((a + i + 1 & Mask) == z)
						frac += 0.5 * sign;
					else
						frac += 0.75 * sign;
				}

				if (((Dbl_Mant_Dig - bits) >= 2) && (CMath.fmod(frac, 1) == 0))
					frac++;
			}

			y += frac;
			y -= bias;

			if ((e2 + Dbl_Mant_Dig & c_int.MaxValue) > (eMax - 5))
			{
				if (CMath.fabs(y) >= CMath.pow(2, Dbl_Mant_Dig))
				{
					if ((denormal != 0) && (bits == (Dbl_Mant_Dig + e2 - eMin)))
						denormal = 0;

					y *= 0.5;
					e2++;
				}

				if (((e2 + Dbl_Mant_Dig) > eMax) || ((denormal != 0) && (frac != 0)))
				{
				}
			}

			return CMath.scalbn(y, e2);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_double HexFloat(FFFile f, c_int bits, c_int eMin, c_int sign, c_int pok)//XX 497
		{
			uint32_t x = 0;
			c_double y = 0;
			c_double scale = 1;
			c_double bias = 0;
			c_int gotTail = 0, gotRad = 0, gotDig = 0;
			c_long_long rp = 0;
			c_long_long dc = 0;
			c_long_long e2 = 0;
			c_int d;

			c_int c = ShGetC(f);

			// Skip leading zeros
			for (; c == '0'; c = ShGetC(f))
				gotDig = 1;

			if (c == '.')
			{
				gotRad = 1;
				c = ShGetC(f);

				// Count zeros after the radix point before significand
				for (rp = 0; c == '0'; c = ShGetC(f), rp--)
					gotDig = 1;
			}

			for (; ((c - '0') < 10) || (((c | 32) - 'a') < 6) || (c == '.'); c = ShGetC(f))
			{
				if (c == '.')
				{
					if (gotRad != 0)
						break;

					rp = dc;
					gotRad = 1;
				}
				else
				{
					gotDig = 1;

					if (c > '9')
						d = (c | 32) + 10 - 'a';
					else
						d = c - '0';

					if (dc < 8)
						x = (uint32_t)(x * 16 + d);
					else if (dc < (Dbl_Mant_Dig / 4 + 1))
						y += d * (scale /= 16);
					else if ((d != 0) && (gotTail == 0))
					{
						y += 0.5 * scale;
						gotTail = 1;
					}

					dc++;
				}
			}

			if (gotDig == 0)
			{
				ShUnget(f);

				if (pok != 0)
				{
					ShUnget(f);

					if (gotRad != 0)
						ShUnget(f);
				}
				else
					ShLim(f, 0);

				return sign * 0.0;
			}

			if (gotRad == 0)
				rp = dc;

			while (dc < 8)
			{
				x *= 16;
				dc++;
			}

			if ((c | 32) == 'p')
			{
				e2 = ScanExp(f, pok);

				if (e2 == c_long_long.MinValue)
				{
					if (pok != 0)
						ShUnget(f);
					else
					{
						ShLim(f, 0);

						return 0;
					}

					e2 = 0;
				}
			}
			else
				ShUnget(f);

			e2 += 4 * rp - 32;

			if (x == 0)
				return sign * 0.0;

			if (e2 > -eMin)
				return sign * CMath.DBL_MAX * CMath.DBL_MAX;

			if (e2 < (eMin - (2 * Dbl_Mant_Dig)))
				return sign * CMath.DBL_MIN * CMath.DBL_MIN;

			while (x < 0x80000000)
			{
				if (y >= 0.5)
				{
					x += x + 1;
					y += y - 1;
				}
				else
				{
					x += x;
					y += y;
				}

				e2--;
			}

			if (bits > (32 + e2 - eMin))
			{
				bits = (c_int)(32 + e2 - eMin);

				if (bits < 0)
					bits = 0;
			}

			if (bits < Dbl_Mant_Dig)
				bias = CMath.copysign(CMath.scalbn(1, 32 + Dbl_Mant_Dig - bits - 1), sign);

			if ((bits < 32) && (y != 0) && ((x & 1) == 0))
			{
				x++;
				y = 0;
			}

			y = bias + sign * (c_double)x + sign * y;
			y -= bias;

			return CMath.scalbn(y, (c_int)e2);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_double FFFloatScan(FFFile f, c_int prec, c_int pok)//XX 610
		{
			c_int sign = 1;
			size_t i;
			c_int bits;
			c_int eMin;
			c_int c;

			switch (prec)
			{
				case 0:
				{
					bits = Flt_Mant_Dig;
					eMin = Flt_Min_Exp - bits;
					break;
				}

				case 1:
				{
					bits = Dbl_Mant_Dig;
					eMin = Dbl_Min_Exp - bits;
					break;
				}

				case 2:
				{
					bits = Dbl_Mant_Dig;
					eMin = Dbl_Min_Exp - bits;
					break;
				}

				default:
					return 0;
			}

			while (AvString.Av_IsSpace((c = ShGetC(f))))
			{
			}

			if ((c == '+') || (c == '-'))
			{
				sign -= 2 * (c == '-' ? 1 : 0);
				c = ShGetC(f);
			}

			const string inf = "infinity";
			const string nan = "nan";

			for (i = 0; (i < 8) && ((c | 32) == inf[(c_int)i]); i++)
			{
				if (i < 7)
					c = ShGetC(f);
			}

			if ((i == 3) || (i == 8) || ((i > 3 && (pok != 0))))
			{
				if (i != 8)
				{
					ShUnget(f);

					if (pok != 0)
					{
						for (; i > 3; i--)
							ShUnget(f);
					}
				}

				return sign * c_double.PositiveInfinity;
			}

			if (i == 0)
			{
				for (i = 0; (i < 3) && ((c |32) == nan[(c_int)i]); i++)
				{
					if (i < 2)
						c = ShGetC(f);
				}
			}

			if (i == 3)
			{
				if (ShGetC(f) != '(')
				{
					ShUnget(f);

					return c_double.NaN;
				}

				for (i = 1; ; i++)
				{
					c = ShGetC(f);

					if (((c - '0') < 10) || ((c - 'A') < 26) || ((c - 'a') < 26) || (c == '_'))
						continue;

					if (c == ')')
						return c_double.NaN;

					ShUnget(f);

					if (pok == 0)
					{
						ShLim(f, 0);

						return 0;
					}

					while (i-- != 0)
						ShUnget(f);

					return c_double.NaN;
				}
			}

			if (i != 0)
			{
				ShUnget(f);
				ShLim(f, 0);

				return 0;
			}

			if (c == '0')
			{
				c = ShGetC(f);

				if ((c | 32) == 'x')
					return HexFloat(f, bits, eMin, sign, pok);

				ShUnget(f);
				c = '0';
			}

			return DecFloat(f, c, bits, eMin, sign, pok);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Store_Int(object[] results, c_int destIndex, Size size, c_ulong_long i)//XX 704
		{
			if (destIndex == -1)
				return;

			switch (size)
			{
				case Size.hh:
				{
					results[destIndex] = (c_char)i;
					break;
				}

				case Size.h:
				{
					results[destIndex] = (c_short)i;
					break;
				}

				case Size.def:
				{
					results[destIndex] = (c_int)i;
					break;
				}

				case Size.l:
				{
					results[destIndex] = (c_long)i;
					break;
				}

				case Size.ll:
				{
					results[destIndex] = (c_long_long)i;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int FF_VFScanF(FFFile f, CPointer<char> fmt, object[] results)//XX 726
		{
			c_int width;
			Size size;
			c_int @base;
			CPointer<char> p;
			c_int c, t;
			CPointer<char> s = new CPointer<char>();
			c_int nextResultIndex = 0;
			c_int destIndex = -1;
			c_int invert;
			c_int matches = 0;
			c_ulong_long x;
			c_double y;
			ptrdiff_t pos = 0;
			CPointer<c_uchar> scanSet = new CPointer<c_uchar>(257);
			size_t i;

			for (p = fmt; p[0] != 0; p++)
			{
				if (AvString.Av_IsSpace(p[0]))
				{
					while (AvString.Av_IsSpace(p[1]))
						p++;

					ShLim(f, 0);

					while (AvString.Av_IsSpace(ShGetC(f)))
					{
					}

					ShUnget(f);
					pos += ShCnt(f);
					continue;
				}

				if ((p[0] != '%') || (p[1] == '%'))
				{
					ShLim(f, 0);

					if (p[0] == '%')
					{
						p++;

						while (AvString.Av_IsSpace((c = ShGetC(f))))
						{
						}
					}
					else
						c = ShGetC(f);

					if (c != p[0])
					{
						ShUnget(f);

						if (c < 0)
							goto Input_Fail;

						goto Match_Fail;
					}

					pos += ShCnt(f);
					continue;
				}

				p++;

				if (p[0] == '*')
				{
					destIndex = -1;
					p++;
				}
				else if (AvString.Av_IsDigit(p[0]) && (p[1] == 'S'))
				{
					destIndex = p[0] - '0';
					p += 2;
				}
				else
					destIndex = nextResultIndex++;

				for (width = 0; AvString.Av_IsDigit(p[0]); p++)
					width = (width * 10) + p[0] - '0';

				if (p[0] == 'm')
				{
					s.SetToNull();
					p++;
				}

				size = Size.def;

				switch (p[0, 1])
				{
					case 'h':
					{
						if (p[0] == 'h')
						{
							p++;
							size = Size.hh;
						}
						else
							size = Size.h;

						break;
					}

					case 'l':
					{
						if (p[0] == 'l')
						{
							p++;
							size = Size.ll;
						}
						else
							size = Size.l;

						break;
					}

					case 'j':
					{
						size = Size.ll;
						break;
					}

					case 'z':
					case 't':
					{
						size = Size.l;
						break;
					}

					case 'L':
					{
						size = Size.L;
						break;
					}

					case 'd': case 'i': case 'o': case 'u': case 'x':
					case 'a': case 'e': case 'f': case 'g':
					case 'A': case 'E': case 'F': case 'G': case 'X':
					case 's': case 'c': case '[':
					case 'S': case 'C':
					case 'p': case 'n':
					{
						p--;
						break;
					}

					default:
						goto Fmt_Fail;
				}

				t = p[0];

				// C or S
				if ((t & 0x2f) == 3)
				{
					t |= 32;
					size = Size.l;
				}

				switch (t)
				{
					case 'c':
					{
						if (width < 1)
							width = 1;

						break;
					}

					case '[':
						break;

					case 'n':
					{
						Store_Int(results, destIndex, size, (c_ulong_long)pos);

						// Do not increment match count, etc!
						continue;
					}

					default:
					{
						ShLim(f, 0);

						while (AvString.Av_IsSpace(ShGetC(f)))
						{
						}

						ShUnget(f);
						pos += ShCnt(f);
						break;
					}
				}

				ShLim(f, width);

				if (ShGetC(f) < 0)
					goto Input_Fail;

				ShUnget(f);

				bool Int_Common()
				{
					x = FFIntScan(f, (c_uint)@base, 0, c_ulong_long.MaxValue);

					if (ShCnt(f) == 0)
						return true;

					if ((t == 'p') && (destIndex != -1))
						results[destIndex] = x;
					else
						Store_Int(results, destIndex, size, x);

					return false;
				}

				switch (t)
				{
					case 's':
					case 'c':
					case '[':
					{
						if ((t == 'c') || (t == 's'))
						{
							CMemory.memset<c_uchar>(scanSet, 0xff, (size_t)scanSet.Length);
							scanSet[0] = 0;

							if (t == 's')
							{
								scanSet[1 + '\t'] = 0;
								scanSet[1 + '\n'] = 0;
								scanSet[1 + '\v'] = 0;
								scanSet[1 + '\f'] = 0;
								scanSet[1 + '\r'] = 0;
								scanSet[1 + ' '] = 0;
							}
						}
						else
						{
							if (p[1, 1] == '^')
							{
								p++;
								invert = 1;
							}
							else
								invert = 0;

							CMemory.memset(scanSet, (c_uchar)invert, (size_t)scanSet.Length);
							scanSet[0] = 0;

							if (p[0] == '-')
							{
								p++;
								scanSet[1 + '-'] = (c_uchar)(1 - invert);
							}
							else if (p[0] == ']')
							{
								p++;
								scanSet[1 + ']'] = (c_uchar)(1 - invert);
							}

							for (; p[0] != ']'; p++)
							{
								if (p[0] == 0)
									goto Fmt_Fail;

								if ((p[0] == '-') && (p[1] != 0) && (p[1] != ']'))
								{
									for (c = p[0, 1]; c < p[0]; c++)
										scanSet[1 + c] = (c_uchar)(1 - invert);
								}

								scanSet[1 + p[0]] = (c_uchar)(1 - invert);
							}
						}

						s.SetToNull();
						i = 0;

						if (destIndex != -1)
						{
							s = (CPointer<char>)results[destIndex];

							while (scanSet[(c = ShGetC(f)) + 1] != 0)
								s[i++] = (char)c;
						}
						else
						{
							while (scanSet[(c = ShGetC(f)) + 1] != 0)
							{
							}
						}

						ShUnget(f);

						if (ShCnt(f) == 0)
							goto Match_Fail;

						if ((t == 'c') && (ShCnt(f) != width))
							goto Match_Fail;

						if (t != 'c')
						{
							if (s.IsNotNull)
								s[i] = '\0';
						}

						break;
					}

					case 'p':
					case 'X':
					case 'x':
					{
						@base = 16;
						if (Int_Common())
							goto Match_Fail;

						break;
					}

					case 'o':
					{
						@base = 8;
						if (Int_Common())
							goto Match_Fail;

						break;
					}

					case 'd':
					case 'u':
					{
						@base = 10;
						if (Int_Common())
							goto Match_Fail;

						break;
					}

					case 'i':
					{
						@base = 0;
						if (Int_Common())
							goto Match_Fail;

						break;
					}

					case 'a': case 'A':
					case 'e': case 'E':
					case 'f': case 'F':
					case 'g': case 'G':
					{
						y = FFFloatScan(f, (c_int)size, 0);

						if (ShCnt(f) == 0)
							goto Match_Fail;

						if (destIndex != -1)
						{
							switch (size)
							{
								case Size.def:
								{
									results[destIndex] = (c_float)y;
									break;
								}

								case Size.l:
								{
									results[destIndex] = (c_double)y;
									break;
								}

								case Size.L:
								{
									results[destIndex] = (c_double)y;
									break;
								}
							}
						}

						break;
					}
				}

				pos += ShCnt(f);

				if (destIndex != -1)
					matches++;
			}

			return matches;

			Fmt_Fail:
			Input_Fail:
			if (matches == 0)
				matches--;

			Match_Fail:
			return matches;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int FF_VSScanF(CPointer<char> s, CPointer<char> fmt, object[] results)//XX 951
		{
			FFFile f = new FFFile
			{
				Buf = s,
				Cookie = s,
				Read = FFString_Read
			};

			return FF_VFScanF(f, fmt, results);
		}
		#endregion
	}
}
