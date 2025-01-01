/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using Data_Type = System.Single;
global using Reg_Type = System.Single;

using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOgg;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Internal
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Mdct
	{
		private const c_float cPI3_8 = 0.38268343236508977175f;
		private const c_float cPI2_8 = 0.70710678118654752441f;
		private const c_float cPI1_8 = 0.92387953251128675613f;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_float Float_Conv(double x)
		{
			return (c_float)x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_float Mult_Norm(c_float x)
		{
			return x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_float Halve(c_float x)
		{
			return x * 0.5f;
		}



		/********************************************************************/
		/// <summary>
		/// Build lookups for trig functions; also pre-figure scaling and
		/// some window function algebra
		/// </summary>
		/********************************************************************/
		public static void Mdct_Init(MdctLookup lookup, c_int n)
		{
			c_int[] bitrev = new c_int[n / 4];
			Pointer<Data_Type> T = Memory.Ogg_MAlloc<Data_Type>((size_t)(n + n / 4));

			c_int n2 = n >> 1;
			c_int log2n = lookup.log2n = (c_int)Os.Rint(Math.Log(n) / Math.Log(2.0f));
			lookup.n = n;
			lookup.trig = T;
			lookup.bitrev = bitrev;

			// Trig lookups...
			for (c_int i = 0; i < (n / 4); i++)
			{
				T[i * 2] = Float_Conv(Math.Cos((Math.PI / n) * (4 * i)));
				T[i * 2 + 1] = Float_Conv(-Math.Sin((Math.PI / n) * (4 * i)));
				T[n2 + i * 2] = Float_Conv(Math.Cos((Math.PI / (2 * n)) * (2 * i + 1)));
				T[n2 + i * 2 + 1] = Float_Conv(Math.Sin((Math.PI / (2 * n)) * (2 * i + 1)));
			}

			for (c_int i = 0; i < (n / 8); i++)
			{
				T[n + i * 2] = Float_Conv(Math.Cos((Math.PI / n) * (4 * i + 2)) * 0.5);
				T[n + i * 2 + 1] = Float_Conv(-Math.Sin((Math.PI / n) * (4 * i + 2)) * 0.5);
			}

			// Bitreverse lookup...
			{
				c_int mask = (1 << (log2n - 1)) - 1;
				c_int msb = 1 << (log2n - 2);

				for (c_int i = 0; i < (n / 8); i++)
				{
					c_int acc = 0;

					for (c_int j = 0; (msb >> j) != 0; j++)
					{
						if (((msb >> j) & i) != 0)
							acc |= 1 << j;
					}

					bitrev[i * 2] = ((~acc) & mask) - 1;
					bitrev[i * 2 + 1] = acc;
				}
			}

			lookup.scale = Float_Conv(4.0f / n);
		}



		/********************************************************************/
		/// <summary>
		/// 8 point butterfly (in place, 4 register)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Mdct_Butterfly_8(Pointer<Data_Type> x)
		{
			Reg_Type r0 = x[6] + x[2];
			Reg_Type r1 = x[6] - x[2];
			Reg_Type r2 = x[4] + x[0];
			Reg_Type r3 = x[4] - x[0];

			x[6] = r0 + r2;
			x[4] = r0 - r2;

			r0   = x[5] - x[1];
			r2   = x[7] - x[3];
			x[0] = r1 + r0;
			x[2] = r1 - r0;

			r0   = x[5] + x[1];
			r1   = x[7] + x[3];
			x[3] = r2 + r3;
			x[1] = r2 - r3;
			x[7] = r1 + r0;
			x[5] = r1 - r0;
		}



		/********************************************************************/
		/// <summary>
		/// 16 point butterfly (in place, 4 register)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Mdct_Butterfly_16(Pointer<Data_Type> x)
		{
			Reg_Type r0 = x[1] - x[9];
			Reg_Type r1 = x[0] - x[8];

			x[8]  += x[0];
			x[9]  += x[1];
			x[0]   = Mult_Norm((r0 + r1) * cPI2_8);
			x[1]   = Mult_Norm((r0 - r1) * cPI2_8);

			r0     = x[3] - x[11];
			r1     = x[10] - x[2];
			x[10] += x[2];
			x[11] += x[3];
			x[2]   = r0;
			x[3]   = r1;

			r0     = x[12] - x[4];
			r1     = x[13] - x[5];
			x[12] += x[4];
			x[13] += x[5];
			x[4]   = Mult_Norm((r0 - r1) * cPI2_8);
			x[5]   = Mult_Norm((r0 + r1) * cPI2_8);

			r0     = x[14] - x[6];
			r1     = x[15] - x[7];
			x[14] += x[6];
			x[15] += x[7];
			x[6]   = r0;
			x[7]   = r1;

			Mdct_Butterfly_8(x);
			Mdct_Butterfly_8(x + 8);
		}



		/********************************************************************/
		/// <summary>
		/// 32 point butterfly (in place, 4 register)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Mdct_Butterfly_32(Pointer<Data_Type> x)
		{
			Reg_Type r0 = x[30] - x[14];
			Reg_Type r1 = x[31] - x[15];

			x[30] += x[14];
			x[31] += x[15];
			x[14]  = r0;
			x[15]  = r1;

			r0     = x[28] - x[12];
			r1     = x[29] - x[13];
			x[28] += x[12];
			x[29] += x[13];
			x[12]  = Mult_Norm(r0 * cPI1_8 - r1 * cPI3_8);
			x[13]  = Mult_Norm(r0 * cPI3_8 + r1 * cPI1_8);

			r0     = x[26] - x[10];
			r1     = x[27] - x[11];
			x[26] += x[10];
			x[27] += x[11];
			x[10]  = Mult_Norm((r0 - r1) * cPI2_8);
			x[11]  = Mult_Norm((r0 + r1) * cPI2_8);

			r0     = x[24] - x[8];
			r1     = x[25] - x[9];
			x[24] += x[8];
			x[25] += x[9];
			x[8]   = Mult_Norm(r0 * cPI3_8 - r1 * cPI1_8);
			x[9]   = Mult_Norm(r1 * cPI3_8 + r0 * cPI1_8);

			r0     = x[22] - x[6];
			r1     = x[7] - x[23];
			x[22] += x[6];
			x[23] += x[7];
			x[6]   = r1;
			x[7]   = r0;

			r0     = x[4] - x[20];
			r1     = x[5] - x[21];
			x[20] += x[4];
			x[21] += x[5];
			x[4]   = Mult_Norm(r1 * cPI1_8 + r0 * cPI3_8);
			x[5]   = Mult_Norm(r1 * cPI3_8 - r0 * cPI1_8);

			r0     = x[2] - x[18];
			r1     = x[3] - x[19];
			x[18] += x[2];
			x[19] += x[3];
			x[2]   = Mult_Norm((r1 + r0) * cPI2_8);
			x[3]   = Mult_Norm((r1 - r0) * cPI2_8);

			r0     = x[0] - x[16];
			r1     = x[1] - x[17];
			x[16] += x[0];
			x[17] += x[1];
			x[0]   = Mult_Norm(r1 * cPI3_8 + r0 * cPI1_8);
			x[1]   = Mult_Norm(r1 * cPI1_8 - r0 * cPI3_8);

			Mdct_Butterfly_16(x);
			Mdct_Butterfly_16(x + 16);
		}



		/********************************************************************/
		/// <summary>
		/// N point first stage butterfly (in place, 2 register)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Mdct_Butterfly_First(Pointer<Data_Type> T, Pointer<Data_Type> x, c_int points)
		{
			Pointer<Data_Type> x1 = x + points - 8;
			Pointer<Data_Type> x2 = x + (points >> 1) - 8;
			Reg_Type r0;
			Reg_Type r1;

			do
			{
				r0 = x1[6] - x2[6];
				r1 = x1[7] - x2[7];
				x1[6] += x2[6];
				x1[7] += x2[7];
				x2[6] = Mult_Norm(r1 * T[1] + r0 * T[0]);
				x2[7] = Mult_Norm(r1 * T[0] - r0 * T[1]);

				r0 = x1[4] - x2[4];
				r1 = x1[5] - x2[5];
				x1[4] += x2[4];
				x1[5] += x2[5];
				x2[4] = Mult_Norm(r1 * T[5] + r0 * T[4]);
				x2[5] = Mult_Norm(r1 * T[4] - r0 * T[5]);

				r0 = x1[2] - x2[2];
				r1 = x1[3] - x2[3];
				x1[2] += x2[2];
				x1[3] += x2[3];
				x2[2] = Mult_Norm(r1 * T[9] + r0 * T[8]);
				x2[3] = Mult_Norm(r1 * T[8] - r0 * T[9]);

				r0 = x1[0] - x2[0];
				r1 = x1[1] - x2[1];
				x1[0] += x2[0];
				x1[1] += x2[1];
				x2[0] = Mult_Norm(r1 * T[13] + r0 * T[12]);
				x2[1] = Mult_Norm(r1 * T[12] - r0 * T[13]);

				x1 -= 8;
				x2 -= 8;
				T += 16;
			}
			while (x2 >= x);
		}



		/********************************************************************/
		/// <summary>
		/// N/stage point generic N stage butterfly (in place, 2 register)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Mdct_Butterfly_Generic(Pointer<Data_Type> T, Pointer<Data_Type> x, c_int points, c_int trigint)
		{
			Pointer<Data_Type> x1 = x + points - 8;
			Pointer<Data_Type> x2 = x + (points >> 1) - 8;
			Reg_Type r0;
			Reg_Type r1;

			do
			{
				r0 = x1[6] - x2[6];
				r1 = x1[7] - x2[7];
				x1[6] += x2[6];
				x1[7] += x2[7];
				x2[6] = Mult_Norm(r1 * T[1] + r0 * T[0]);
				x2[7] = Mult_Norm(r1 * T[0] - r0 * T[1]);

				T += trigint;

				r0 = x1[4] - x2[4];
				r1 = x1[5] - x2[5];
				x1[4] += x2[4];
				x1[5] += x2[5];
				x2[4] = Mult_Norm(r1 * T[1] + r0 * T[0]);
				x2[5] = Mult_Norm(r1 * T[0] - r0 * T[1]);

				T += trigint;

				r0 = x1[2] - x2[2];
				r1 = x1[3] - x2[3];
				x1[2] += x2[2];
				x1[3] += x2[3];
				x2[2] = Mult_Norm(r1 * T[1] + r0 * T[0]);
				x2[3] = Mult_Norm(r1 * T[0] - r0 * T[1]);

				T += trigint;

				r0 = x1[0] - x2[0];
				r1 = x1[1] - x2[1];
				x1[0] += x2[0];
				x1[1] += x2[1];
				x2[0] = Mult_Norm(r1 * T[1] + r0 * T[0]);
				x2[1] = Mult_Norm(r1 * T[0] - r0 * T[1]);

				T += trigint;

				x1 -= 8;
				x2 -= 8;
			}
			while (x2 >= x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Mdct_Butterflies(MdctLookup init, Pointer<Data_Type> x, c_int points)
		{
			Pointer<Data_Type> T = init.trig;
			c_int stages = init.log2n - 5;

			if (--stages > 0)
				Mdct_Butterfly_First(T, x, points);

			for (c_int i = 1; --stages > 0; i++)
			{
				for (c_int j = 0; j < (1 << i); j++)
					Mdct_Butterfly_Generic(T, x + (points >> i) * j, points >> i, 4 << i);
			}

			for (c_int j = 0; j < points; j += 32)
				Mdct_Butterfly_32(x + j);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Mdct_Clear(MdctLookup l)
		{
			if (l != null)
			{
				if (l.trig.IsNotNull)
					Memory.Ogg_Free(l.trig);

				if (l.bitrev != null)
					l.bitrev = null;

				l.Clear();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Mdct_Bitreverse(MdctLookup init, Pointer<Data_Type> x)
		{
			c_int n = init.n;
			Pointer<c_int> bit = init.bitrev;
			Pointer<Data_Type> w0 = x;
			Pointer<Data_Type> w1 = x = w0 + (n >> 1);
			Pointer<Data_Type> T = init.trig + n;

			do
			{
				Pointer<Data_Type> x0 = x + bit[0];
				Pointer<Data_Type> x1 = x + bit[1];

				Reg_Type r0 = x0[1] - x1[1];
				Reg_Type r1 = x0[0] + x1[0];
				Reg_Type r2 = Mult_Norm(r1 * T[0] + r0 * T[1]);
				Reg_Type r3 = Mult_Norm(r1 * T[1] - r0 * T[0]);

				w1 -= 4;

				r0 = Halve(x0[1] + x1[1]);
				r1 = Halve(x0[0] - x1[0]);

				w0[0] = r0 + r2;
				w1[2] = r0 - r2;
				w0[1] = r1 + r3;
				w1[3] = r3 - r1;

				x0 = x + bit[2];
				x1 = x + bit[3];

				r0 = x0[1] - x1[1];
				r1 = x0[0] + x1[0];
				r2 = Mult_Norm(r1 * T[2] + r0 * T[3]);
				r3 = Mult_Norm(r1 * T[3] - r0 * T[2]);

				r0 = Halve(x0[1] + x1[1]);
				r1 = Halve(x0[0] - x1[0]);

				w0[2] = r0 + r2;
				w1[0] = r0 - r2;
				w0[3] = r1 + r3;
				w1[1] = r3 - r1;

				T += 4;
				bit += 4;
				w0 += 4;
			}
			while (w0 < w1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Mdct_Backward(MdctLookup init, Pointer<Data_Type> @in, Pointer<Data_Type> @out)
		{
			c_int n = init.n;
			c_int n2 = n >> 1;
			c_int n4 = n >> 2;

			// Rotate
			Pointer<Data_Type> iX = @in + n2 - 7;
			Pointer<Data_Type> oX = @out + n2 + n4;
			Pointer<Data_Type> T = init.trig + n4;

			do
			{
				oX -= 4;

				oX[0] = Mult_Norm(-iX[2] * T[3] - iX[0] * T[2]);
				oX[1] = Mult_Norm( iX[0] * T[3] - iX[2] * T[2]);
				oX[2] = Mult_Norm(-iX[6] * T[1] - iX[4] * T[0]);
				oX[3] = Mult_Norm( iX[4] * T[1] - iX[6] * T[0]);

				iX -= 8;
				T += 4;
			}
			while (iX >= @in);

			iX = @in + n2 - 8;
			oX = @out + n2 + n4;
			T = init.trig + n4;

			do
			{
				T -= 4;

				oX[0] = Mult_Norm(iX[4] * T[3] + iX[6] * T[2]);
				oX[1] = Mult_Norm(iX[4] * T[2] - iX[6] * T[3]);
				oX[2] = Mult_Norm(iX[0] * T[1] + iX[2] * T[0]);
				oX[3] = Mult_Norm(iX[0] * T[0] - iX[2] * T[1]);

				iX -= 8;
				oX += 4;
			}
			while (iX >= @in);

			Mdct_Butterflies(init, @out + n2, n2);
			Mdct_Bitreverse(init, @out);

			// Rotate + window
			{
				Pointer<Data_Type> oX1 = @out + n2 + n4;
				Pointer<Data_Type> oX2 = @out + n2 + n4;
				iX = @out;
				T = init.trig + n2;

				do
				{
					oX1 -= 4;

					oX1[3] =  Mult_Norm(iX[0] * T[1] - iX[1] * T[0]);
					oX2[0] = -Mult_Norm(iX[0] * T[0] + iX[1] * T[1]);

					oX1[2] =  Mult_Norm(iX[2] * T[3] - iX[3] * T[2]);
					oX2[1] = -Mult_Norm(iX[2] * T[2] + iX[3] * T[3]);

					oX1[1] =  Mult_Norm(iX[4] * T[5] - iX[5] * T[4]);
					oX2[2] = -Mult_Norm(iX[4] * T[4] + iX[5] * T[5]);

					oX1[0] =  Mult_Norm(iX[6] * T[7] - iX[7] * T[6]);
					oX2[3] = -Mult_Norm(iX[6] * T[6] + iX[7] * T[7]);

					oX2 += 4;
					iX += 8;
					T += 8;
				}
				while (iX < oX1);

				iX = @out + n2 + n4;
				oX1 = @out + n4;
				oX2 = oX1;

				do
				{
					oX1 -= 4;
					iX -= 4;

					oX2[0] = -(oX1[3] = iX[3]);
					oX2[1] = -(oX1[2] = iX[2]);
					oX2[2] = -(oX1[1] = iX[1]);
					oX2[3] = -(oX1[0] = iX[0]);

					oX2 += 4;
				}
				while (oX2 < iX);

				iX = @out + n2 + n4;
				oX1 = @out + n2 + n4;
				oX2 = @out + n2;

				do
				{
					oX1 -= 4;

					oX1[0] = iX[3];
					oX1[1] = iX[2];
					oX1[2] = iX[1];
					oX1[3] = iX[0];

					iX += 4;
				}
				while (oX1 > oX2);
			}
		}
	}
}
