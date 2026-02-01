/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	internal class Tx_Int32 : Tx_Template<c_float, int32_t, uint32_t, long, AvComplexInt32>
	{
		private static readonly Tx_Int32 mySelf = new Tx_Int32();

		/// <summary>
		/// 
		/// </summary>
		public static readonly FFTxCodelet[] ff_Tx_Codelet_List_Int32_C;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		static Tx_Int32()
		{
			ff_Tx_Codelet_List_Int32_C = mySelf.Build_Codelet_List();
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Tx_Int32() : base("Int32")
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override int32_t Mult(int32_t x, int32_t m)
		{
			return (int32_t)(((((int64_t)x) * (int64_t)m) + 0x40000000) >> 31);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void CMul<T>(out T dre, out T dim, int32_t are, int32_t aim, int32_t bre, int32_t bim)
		{
			int64_t accu = (int64_t)bre * are;
			accu -= (int64_t)bim * aim;
			dre = T.CreateChecked((accu + 0x40000000) >> 31);

			accu = (int64_t)bim * are;
			accu += (int64_t)bre * aim;
			dim = T.CreateChecked((accu + 0x40000000) >> 31);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void SMul(out int32_t dre, out int32_t dim, int32_t are, int32_t aim, int32_t bre, int32_t bim)
		{
			int64_t accu = (int64_t)bre * are;
			accu -= (int64_t)bim * aim;
			dre = (c_int)((accu + 0x40000000) >> 31);

			accu = (int64_t)bim * are;
			accu -= (int64_t)bre * aim;
			dim = (c_int)((accu + 0x40000000) >> 31);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override int32_t Unscale(int32_t x)
		{
			return (int32_t)(x / 2147483648.0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override int32_t Rescale(int32_t x)
		{
			return (int32_t)Common.Av_Clip64(CMath.llrint(x * 2147483648.0), int32_t.MinValue, int32_t.MaxValue);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override int32_t Fold(int32_t x, int32_t y)
		{
			return (int32_t)(x + (c_uint)y + 32) >> 6;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void Bf<T>(out T x, out T y, T a, T b)
		{
			x = T.CreateChecked(c_uint.CreateChecked(a) - uint32_t.CreateChecked(b));
			y = T.CreateChecked(c_uint.CreateChecked(a) + c_uint.CreateChecked(b));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void DoFft3(CPointer<AvComplexInt32> @out, CPointer<AvComplexInt32> @in, AvComplexInt32[] tmp, int32_t[] tab, ptrdiff_t stride)
		{
			int64_t[] mTmp = new int64_t[4];

			@out[0 * stride].Re = (int32_t)((int64_t)tmp[0].Re + tmp[2].Re);
			@out[0 * stride].Im = (int32_t)((int64_t)tmp[0].Im + tmp[2].Im);
			mTmp[0] = (int64_t)tab[8] * tmp[1].Re;
			mTmp[1] = (int64_t)tab[9] * tmp[1].Im;
			mTmp[2] = (int64_t)tab[10] * tmp[2].Re;
			mTmp[3] = (int64_t)tab[10] * tmp[2].Im;
			@out[1 * stride].Re = (int32_t)(tmp[0].Re - (mTmp[2] + mTmp[0] + 0x40000000 >> 31));
			@out[1 * stride].Im = (int32_t)(tmp[0].Im - (mTmp[3] - mTmp[1] + 0x40000000 >> 31));
			@out[2 * stride].Re = (int32_t)(tmp[0].Re - (mTmp[2] - mTmp[0] + 0x40000000 >> 31));
			@out[2 * stride].Im = (int32_t)(tmp[0].Im - (mTmp[3] + mTmp[1] + 0x40000000 >> 31));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void DoFft7(CPointer<AvComplexInt32> @out, CPointer<AvComplexInt32> @in, CPointer<AvComplexInt32> tab, AvComplexInt32[] t, AvComplexInt32[] z)
		{
			int64_t[] mTmp = new int64_t[12];

			// NOTE: it's possible to do this with 16 mults but 72 adds
			mTmp[0] = (((int64_t)tab[0].Re) * t[0].Re) - (((int64_t)tab[2].Re) * t[4].Re);
			mTmp[1] = (((int64_t)tab[0].Re) * t[4].Re) - (((int64_t)tab[1].Re) * t[0].Re);
			mTmp[2] = (((int64_t)tab[0].Re) * t[2].Re) - (((int64_t)tab[2].Re) * t[0].Re);
			mTmp[3] = (((int64_t)tab[0].Re) * t[0].Im) - (((int64_t)tab[1].Re) * t[2].Im);
			mTmp[4] = (((int64_t)tab[0].Re) * t[4].Im) - (((int64_t)tab[1].Re) * t[0].Im);
			mTmp[5] = (((int64_t)tab[0].Re) * t[2].Im) - (((int64_t)tab[2].Re) * t[0].Im);

			mTmp[6] = (((int64_t)tab[2].Im) * t[1].Im) + (((int64_t)tab[1].Im) * t[5].Im);
			mTmp[7] = (((int64_t)tab[0].Im) * t[5].Im) + (((int64_t)tab[2].Im) * t[3].Im);
			mTmp[8] = (((int64_t)tab[2].Im) * t[5].Im) + (((int64_t)tab[1].Im) * t[3].Im);
			mTmp[9] = (((int64_t)tab[0].Im) * t[1].Re) + (((int64_t)tab[1].Im) * t[3].Re);
			mTmp[10] = (((int64_t)tab[2].Im) * t[3].Re) + (((int64_t)tab[0].Im) * t[5].Re);
			mTmp[11] = (((int64_t)tab[2].Im) * t[1].Re) + (((int64_t)tab[1].Im) * t[5].Re);

			z[0].Re = (int32_t)(mTmp[0] - (((int64_t)tab[1].Re) * t[2].Re) + 0x40000000 >> 31);
			z[1].Re = (int32_t)(mTmp[1] - (((int64_t)tab[2].Re) * t[2].Re) + 0x40000000 >> 31);
			z[2].Re = (int32_t)(mTmp[2] - (((int64_t)tab[1].Re) * t[4].Re) + 0x40000000 >> 31);
			z[0].Im = (int32_t)(mTmp[3] - (((int64_t)tab[2].Re) * t[4].Im) + 0x40000000 >> 31);
			z[1].Im = (int32_t)(mTmp[4] - (((int64_t)tab[2].Re) * t[2].Im) + 0x40000000 >> 31);
			z[2].Im = (int32_t)(mTmp[5] - (((int64_t)tab[1].Re) * t[4].Im) + 0x40000000 >> 31);

			t[0].Re = (int32_t)(mTmp[6] - (((int64_t)tab[0].Im) * t[3].Im) + 0x40000000 >> 31);
			t[2].Re = (int32_t)(mTmp[7] - (((int64_t)tab[1].Im) * t[1].Im) + 0x40000000 >> 31);
			t[4].Re = (int32_t)(mTmp[8] + (((int64_t)tab[0].Im) * t[1].Im) + 0x40000000 >> 31);
			t[0].Im = (int32_t)(mTmp[9] + (((int64_t)tab[2].Im) * t[5].Re) + 0x40000000 >> 31);
			t[2].Im = (int32_t)(mTmp[10] - (((int64_t)tab[1].Im) * t[1].Re) + 0x40000000 >> 31);
			t[4].Im = (int32_t)(mTmp[11] - (((int64_t)tab[0].Im) * t[3].Re) + 0x40000000 >> 31);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void DoFft9(CPointer<AvComplexInt32> @out, CPointer<AvComplexInt32> @in, AvComplexInt32 dc, CPointer<AvComplexInt32> tab, AvComplexInt32[] t, AvComplexInt32[] w, AvComplexInt32[] x, AvComplexInt32[] y, AvComplexInt32[] z)
		{
			int64_t[] mTmp = new int64_t[12];

			mTmp[0] = t[1].Re - t[3].Re + t[7].Re;
			mTmp[1] = t[1].Im - t[3].Im + t[7].Im;

			y[3].Re = (int32_t)(((int64_t)tab[0].Im * mTmp[0]) + 0x40000000 >> 31);
			y[3].Im = (int32_t)(((int64_t)tab[0].Im * mTmp[1]) + 0x40000000 >> 31);

			mTmp[0] = (int32_t)(((int64_t)tab[0].Re * z[1].Re) + 0x40000000 >> 31);
			mTmp[1] = (int32_t)(((int64_t)tab[0].Re * z[1].Im) + 0x40000000 >> 31);
			mTmp[2] = (int32_t)(((int64_t)tab[0].Re * t[4].Re) + 0x40000000 >> 31);
			mTmp[3] = (int32_t)(((int64_t)tab[0].Re * t[4].Im) + 0x40000000 >> 31);

			x[3].Re = z[0].Re + (int32_t)mTmp[0];
			x[3].Im = z[0].Im + (int32_t)mTmp[1];
			z[0].Re = @in[0].Re + (int32_t)mTmp[2];
			z[0].Im = @in[0].Im + (int32_t)mTmp[3];

			mTmp[0] = (int64_t)tab[1].Re * w[0].Re;
			mTmp[1] = (int64_t)tab[1].Re * w[0].Im;
			mTmp[2] = (int64_t)tab[2].Im * w[0].Re;
			mTmp[3] = (int64_t)tab[2].Im * w[0].Im;
			mTmp[4] = (int64_t)tab[1].Im * w[2].Re;
			mTmp[5] = (int64_t)tab[1].Im * w[2].Im;
			mTmp[6] = (int64_t)tab[2].Re * w[2].Re;
			mTmp[7] = (int64_t)tab[2].Re * w[2].Im;

			x[1].Re = (int32_t)(mTmp[0] + ((int64_t)tab[2].Im * w[1].Re) + 0x40000000 >> 31);
			x[1].Im = (int32_t)(mTmp[1] + ((int64_t)tab[2].Im * w[1].Im) + 0x40000000 >> 31);
			x[2].Re = (int32_t)(mTmp[2] - ((int64_t)tab[3].Re * w[1].Re) + 0x40000000 >> 31);
			x[2].Im = (int32_t)(mTmp[3] - ((int64_t)tab[3].Re * w[1].Im) + 0x40000000 >> 31);
			y[1].Re = (int32_t)(mTmp[4] + ((int64_t)tab[2].Re * w[3].Re) + 0x40000000 >> 31);
			y[1].Im = (int32_t)(mTmp[5] + ((int64_t)tab[2].Re * w[3].Im) + 0x40000000 >> 31);
			y[2].Re = (int32_t)(mTmp[6] - ((int64_t)tab[3].Im * w[3].Re) + 0x40000000 >> 31);
			y[2].Im = (int32_t)(mTmp[7] - ((int64_t)tab[3].Im * w[3].Im) + 0x40000000 >> 31);

			y[0].Re = (int32_t)(((int64_t)tab[0].Im * t[5].Re) + 0x40000000 >> 31);
			y[0].Im = (int32_t)(((int64_t)tab[0].Im * t[5].Im) + 0x40000000 >> 31);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void DoDctII(AvTxContext s, CPointer<int32_t> dst, CPointer<int32_t> src, ptrdiff_t stride)
		{
			c_int len = s.Len;
			c_int len2 = len >> 1;
			CPointer<int32_t> exp = s.Exp.ToPointer<int32_t>();
			int64_t tmp1, tmp2;

			for (c_int i = 0; i < len2; i++)
			{
				int32_t in1 = src[i];
				int32_t in2 = src[len - i - 1];
				int32_t _s = exp[len + i];

				tmp1 = in1 + in2;
				tmp2 = in1 - in2;

				tmp1 >>= 1;
				tmp2 *= _s;

				tmp2 = (tmp2 + 0x40000000) >> 31;

				src[i] = (int32_t)(tmp1 + tmp2);
				src[len - i - 1] = (int32_t)(tmp1 - tmp2);
			}

			s.Fn[0](s.Sub[0], dst, src, Marshal.SizeOf<AvComplexInt32>());

			int32_t next = dst[len];

			for (c_int i = len - 2; i > 0; i -= 2)
			{
				CMul(out int32_t tmp, out dst[i], exp[len - i], exp[i], dst[i + 0], dst[i + 1]);

				dst[i + 1] = next;

				next += tmp;
			}

			tmp1 = ((int64_t)exp[0]) * ((int64_t)dst[0]);
			dst[0] = (int32_t)((tmp1 + 0x40000000) >> 31);
			dst[1] = next;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void DoDctIII(AvTxContext s, CPointer<int32_t> dst, CPointer<int32_t> src, ptrdiff_t stride)
		{
			c_int len = s.Len;
			c_int len2 = len >> 1;
			CPointer<int32_t> exp = s.Exp.ToPointer<int32_t>();
			int64_t tmp1, tmp2 = src[len - 1];
			tmp2 = ((2 * tmp2) + 0x40000000) >> 31;

			src[len] = (int32_t)tmp2;

			for (c_int i = len - 2; i >= 2; i -= 2)
			{
				int32_t val1 = src[i - 0];
				int32_t val2 = src[i - 1] - src[i + 1];

				CMul(out src[i + 1], out src[i], exp[len - i], exp[i], val1, val2);
			}

			s.Fn[0](s.Sub[0], dst, src, sizeof(c_float));

			for (c_int i = 0; i < len2; i++)
			{
				int32_t in1 = dst[i];
				int32_t in2 = dst[len - i - 1];
				int32_t c = exp[len + i];

				tmp1 = in1 + in2;
				tmp2 = in1 - in2;
				tmp2 *= c;
				tmp2 = (tmp2 + 0x40000000) >> 31;

				dst[i] = (int32_t)(tmp1 + tmp2);
				dst[len -i - 1] = (int32_t)(tmp1 - tmp2);
			}
		}
	}
}
