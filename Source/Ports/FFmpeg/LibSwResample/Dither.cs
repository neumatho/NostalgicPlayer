/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Dither
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Swri_Get_Dither(SwrContext s, IPointer dst, c_int len, c_uint seed, AvSampleFormat noise_Fmt)//XX 27
		{
			throw new NotImplementedException();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Swri_Dither_Init(SwrContext s, AvSampleFormat out_Fmt, AvSampleFormat in_Fmt)//XX 80
		{
			c_double scale = 0;

			if ((s.Dither.Method > SwrDitherType.Triangular_HighPass) && (s.Dither.Method <= SwrDitherType.Ns))
				return Error.EINVAL;

			out_Fmt = SampleFmt.Av_Get_Packed_Sample_Fmt(out_Fmt);
			in_Fmt = SampleFmt.Av_Get_Packed_Sample_Fmt(in_Fmt);

			if ((in_Fmt == AvSampleFormat.Flt) || (in_Fmt == AvSampleFormat.Dbl))
			{
				if (out_Fmt == AvSampleFormat.S32)
					scale = 1.0 / (1L << 31);

				if (out_Fmt == AvSampleFormat.S16)
					scale = 1.0 / (1L << 15);

				if (out_Fmt == AvSampleFormat.U8)
					scale = 1.0 / (1L << 7);
			}

			if ((in_Fmt == AvSampleFormat.S32) && (out_Fmt == AvSampleFormat.S32) && ((s.Dither.Output_Sample_Bits & 31) != 0))
				scale = 1;

			if ((in_Fmt == AvSampleFormat.S32) && (out_Fmt == AvSampleFormat.S16))
				scale = 1 << 16;

			if ((in_Fmt == AvSampleFormat.S32) && (out_Fmt == AvSampleFormat.U8))
				scale = 1 << 24;

			if ((in_Fmt == AvSampleFormat.S16) && (out_Fmt == AvSampleFormat.U8))
				scale = 1 << 8;

			scale *= s.Dither.Scale;

			if ((out_Fmt == AvSampleFormat.S32) && (s.Dither.Output_Sample_Bits != 0))
				scale *= 1 << (32 - s.Dither.Output_Sample_Bits);

			if (scale == 0)
			{
				s.Dither.Method = SwrDitherType.None;

				return 0;
			}

			s.Dither.Ns_Pos = 0;
			s.Dither.Noise_Scale = (c_float)scale;
			s.Dither.Ns_Scale = (c_float)scale;
			s.Dither.Ns_Scale_1 = scale != 0 ? (c_float)(1 / scale) : 0.0f;

			Array.Clear(s.Dither.Ns_Errors);

			c_int i;
			for (i = 0; i < Noise_Shaping_Data.Filters.Length; i++)
			{
				Filter_T f = Noise_Shaping_Data.Filters[i];

				if (((CMath.llabs(s.Out_Sample_Rate - f.Rate) * 20) <= f.Rate) && (f.Name == s.Dither.Method))
				{
					s.Dither.Ns_Taps = (c_int)f.Len;

					for (c_int j = 0; j < (c_int)f.Len; j++)
						s.Dither.Ns_Coeffs[j] = f.Coefs[j];

					s.Dither.Ns_Scale_1 *= (c_float)(1 - (CMath.exp(f.Gain_cB * Mathematics.M_Ln10 * 0.005) * 2 / (1 << (8 * SampleFmt.Av_Get_Bytes_Per_Sample(out_Fmt)))));
					break;
				}
			}

			if ((i == Noise_Shaping_Data.Filters.Length) && (s.Dither.Method > SwrDitherType.Ns))
			{
				Log.Av_Log(s, Log.Av_Log_Warning, "Requested noise shaping dither not available at this sampling rate, using triangular hp dither\n");

				s.Dither.Method = SwrDitherType.Triangular_HighPass;
			}

			return 0;
		}
	}
}
