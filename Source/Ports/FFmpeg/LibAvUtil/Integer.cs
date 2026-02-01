/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// Arbitrary precision integers
	/// </summary>
	public static unsafe class Integer
	{
		/// <summary></summary>
		public const c_int Av_Integer_Size = 8;

		private static readonly AvInteger zero_I = new AvInteger();

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static AvInteger Av_Add_I(AvInteger a, AvInteger b)//XX 36
		{
			c_int carry = 0;

			for (c_int i = 0; i < Av_Integer_Size; i++)
			{
				carry = (carry >> 16) + a.V[i] + b.V[i];
				a.V[i] = (uint16_t)carry;
			}

			return a;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static AvInteger Av_Sub_I(AvInteger a, AvInteger b)//XX 46
		{
			c_int carry = 0;

			for (c_int i = 0; i < Av_Integer_Size; i++)
			{
				carry = (carry >> 16) + a.V[i] - b.V[i];
				a.V[i] = (uint16_t)carry;
			}

			return a;
		}



		/********************************************************************/
		/// <summary>
		/// Return the rounded-down value of the base 2 logarithm of the
		/// given AVInteger. This is simply the index of the most significant
		/// bit which is 1, or 0 if all bits are 0
		/// </summary>
		/********************************************************************/
		public static c_int Av_Log2_I(AvInteger a)//XX 56
		{
			for (c_int i = Av_Integer_Size - 1; i >= 0; i--)
			{
				if (a.V[i] != 0)
					return IntMath.Av_Log2_16Bit(a.V[i]) + (16 * i);
			}

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static AvInteger Av_Mul_I(AvInteger a, AvInteger b)//XX 66
		{
			AvInteger @out = new AvInteger();

			c_int na = (Av_Log2_I(a) + 16) >> 4;
			c_int nb = (Av_Log2_I(b) + 16) >> 4;

			@out.Clear();

			for (c_int i = 0; i < na; i++)
			{
				c_uint carry = 0;

				if (a.V[i] != 0)
				{
					for (c_int j = i; (j < Av_Integer_Size) && ((j - i) <= nb); j++)
					{
						carry = (carry >> 16) + @out.V[j] + (a.V[i] * (c_uint)b.V[j - i]);
						@out.V[j] = (uint16_t)carry;
					}
				}
			}

			return @out;
		}



		/********************************************************************/
		/// <summary>
		/// Return 0 if a==b, 1 if a›b and -1 if a‹b
		/// </summary>
		/********************************************************************/
		public static c_int Av_Cmp_I(AvInteger a, AvInteger b)//XX 87
		{
			c_int v = (int16_t)a.V[Av_Integer_Size - 1] - (int16_t)b.V[Av_Integer_Size - 1];

			if (v != 0)
				return (v >> 16) | 1;

			for (c_int i = Av_Integer_Size - 2; i >= 0; i--)
			{
				v = a.V[i] - b.V[i];

				if (v != 0)
					return (v >> 16) | 1;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Bitwise shift
		/// </summary>
		/********************************************************************/
		public static AvInteger Av_Shr_I(AvInteger a, c_int s)//XX 99
		{
			AvInteger @out = new AvInteger();

			for (c_int i = 0; i < Av_Integer_Size; i++)
			{
				c_uint index = (c_uint)(i + (s >> 4));
				c_uint v = 0;

				if ((index + 1) < Av_Integer_Size)
					v = a.V[index + 1] * (1U << 16);

				if (index < Av_Integer_Size)
					v |= a.V[index];

				@out.V[i] = (uint16_t)(v >> (s & 15));
			}

			return @out;
		}



		/********************************************************************/
		/// <summary>
		/// Return a % b
		/// </summary>
		/********************************************************************/
		public static AvInteger Av_Mod_I(out AvInteger quot, AvInteger a, AvInteger b)//XX 113
		{
			quot = new AvInteger();

			c_int i = Av_Log2_I(a) - Av_Log2_I(b);

			if ((int16_t)a.V[Av_Integer_Size - 1] < 0)
			{
				a = Av_Mod_I(out quot, Av_Sub_I(zero_I, a), b);
				quot = Av_Sub_I(zero_I, quot);

				return Av_Sub_I(zero_I, a);
			}

			if (i > 0)
				b = Av_Shr_I(b, -i);

			quot.Clear();

			while (i-- >= 0)
			{
				quot = Av_Shr_I(quot, -1);

				if (Av_Cmp_I(a, b) >= 0)
				{
					a = Av_Sub_I(a, b);
					quot.V[0] += 1;
				}

				b = Av_Shr_I(b, 1);
			}

			return a;
		}



		/********************************************************************/
		/// <summary>
		/// Return a/b
		/// </summary>
		/********************************************************************/
		public static AvInteger Av_Div_I(AvInteger a, AvInteger b)//XX 143
		{
			Av_Mod_I(out AvInteger quot, a, b);

			return quot;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the given int64_t to an AVInteger
		/// </summary>
		/********************************************************************/
		public static AvInteger Av_Int2I(int64_t a)//XX 149
		{
			AvInteger @out = new AvInteger();

			for (c_int i = 0; i < Av_Integer_Size; i++)
			{
				@out.V[i] = (uint16_t)a;
				a >>= 16;
			}

			return @out;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the given AVInteger to an int64_t.
		/// If the AVInteger is too large to fit into an int64_t, then only
		/// the least significant 64 bits will be used
		/// </summary>
		/********************************************************************/
		public static int64_t Av_I2Int(AvInteger a)//XX 160
		{
			uint64_t @out = a.V[3];

			for (c_int i = 2; i >= 0; i--)
				@out = (@out << 16) | a.V[i];

			return (int64_t)@out;
		}
	}
}
