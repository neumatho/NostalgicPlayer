/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class Float_Dsp
	{
		/********************************************************************/
		/// <summary>
		/// Allocate a float DSP context
		/// </summary>
		/********************************************************************/
		public static AvFloatDspContext AvPriv_Float_Dsp_Alloc(c_int bit_Exact)//XX 135
		{
			AvFloatDspContext fDsp = Mem.Av_MAlloczObj<AvFloatDspContext>();

			if (fDsp == null)
				return null;

			fDsp.Vector_FMul = Vector_FMul_C;
			fDsp.Vector_DMul = Vector_DMul_C;
			fDsp.Vector_FMac_Scalar = Vector_FMac_Scalar_C;
			fDsp.Vector_FMul_Scalar = Vector_FMul_Scalar_C;
			fDsp.Vector_DMac_Scalar = Vector_DMac_Scalar_C;
			fDsp.Vector_DMul_Scalar = Vector_DMul_Scalar_C;
			fDsp.Vector_FMul_Window = Vector_FMul_Window_C;
			fDsp.Vector_FMul_Add = Vector_FMul_Add_C;
			fDsp.Vector_FMul_Reverse = Vector_FMul_Reverse_C;
			fDsp.Butterflies_Float = Butterflies_Float_C;
			fDsp.ScalarProduct_Float = Float_ScalarProduct.FF_ScalarProduct_Float_C;
			fDsp.ScalarProduct_Double = FF_ScalarProduct_Double_C;

			return fDsp;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Vector_FMul_C(CPointer<c_float> dst, CPointer<c_float> src0, CPointer<c_float> src1, c_int len)//XX 27
		{
			for (c_int i = 0; i < len; i++)
				dst[i] = src0[i] * src1[i];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Vector_DMul_C(CPointer<c_double> dst, CPointer<c_double> src0, CPointer<c_double> src1, c_int len)//XX 35
		{
			for (c_int i = 0; i < len; i++)
				dst[i] = src0[i] * src1[i];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Vector_FMac_Scalar_C(CPointer<c_float> dst, CPointer<c_float> src, c_float mul, c_int len)//XX 43
		{
			for (c_int i = 0; i < len; i++)
				dst[i] += src[i] * mul;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Vector_DMac_Scalar_C(CPointer<c_double> dst, CPointer<c_double> src, c_double mul, c_int len)//XX 51
		{
			for (c_int i = 0; i < len; i++)
				dst[i] += src[i] * mul;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Vector_FMul_Scalar_C(CPointer<c_float> dst, CPointer<c_float> src, c_float mul, c_int len)//XX 59
		{
			for (c_int i = 0; i < len; i++)
				dst[i] = src[i] * mul;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Vector_DMul_Scalar_C(CPointer<c_double> dst, CPointer<c_double> src, c_double mul, c_int len)//XX 67
		{
			for (c_int i = 0; i < len; i++)
				dst[i] = src[i] * mul;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Vector_FMul_Window_C(CPointer<c_float> dst, CPointer<c_float> src0, CPointer<c_float> src1, CPointer<c_float> win, c_int len)//XX 75
		{
			dst += len;
			win += len;
			src0 += len;

			for (c_int i = -len, j = len - 1; i < 0; i++, j--)
			{
				c_float s0 = src0[i];
				c_float s1 = src1[j];
				c_float wi = win[i];
				c_float wj = win[j];

				dst[i] = (s0 * wj) - (s1 * wi);
				dst[j] = (s0 * wi) + (s1 * wj);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Vector_FMul_Add_C(CPointer<c_float> dst, CPointer<c_float> src0, CPointer<c_float> src1, CPointer<c_float> src2, c_int len)//XX 94
		{
			for (c_int i = 0; i < len; i++)
				dst[i] = (src0[i] * src1[i]) + src2[i];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Vector_FMul_Reverse_C(CPointer<c_float> dst, CPointer<c_float> src0, CPointer<c_float> src1, c_int len)//XX 102
		{
			src1 += len - 1;

			for (c_int i = 0; i < len; i++)
				dst[i] = src0[i] * src1[-i];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Butterflies_Float_C(CPointer<c_float> v1, CPointer<c_float> v2, c_int len)//XX 112
		{
			for (c_int i = 0; i < len; i++)
			{
				c_float t = v1[i] - v2[i];
				v1[i] += v2[i];
				v2[i] = t;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_double FF_ScalarProduct_Double_C(CPointer<c_double> v1, CPointer<c_double> v2, size_t len)//XX 124
		{
			c_double p = 0.0;

			for (size_t i = 0; i < len; i++)
				p += v1[i] * v2[i];

			return p;
		}
		#endregion
	}
}
