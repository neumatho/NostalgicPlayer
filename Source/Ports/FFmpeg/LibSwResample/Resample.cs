/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Resample
	{
		/// <summary>
		/// 
		/// </summary>
		public static Resampler swri_Resampler = new Resampler
		{
			Init = Resample_Init,
			Free = Resample_Free,
			Multiple_Resample = Multiple_Resample,
			Flush = Resample_Flush,
			Set_Compensation = Set_Compensation,
			Get_Delay = Get_Delay,
			Invert_Initial_Buffer = Invert_Initial_Buffer,
			Get_Out_Samples = Get_Out_Samples
		};

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Resample_Free(ref ResampleContext cc)//XX 176
		{
			ResampleContext c = cc;

			if (c == null)
				return;

			Mem.Av_FreeP(ref c.Filter_Bank);
			Mem.Av_FreeP(ref cc);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static ResampleContext Resample_Init(ResampleContext c, c_int out_Rate, c_int in_Rate, c_int filter_Size, c_int phase_Shift, c_int linear, c_double cutoff0, AvSampleFormat format, SwrFilterType filter_Type, c_double kaiser_Beta, c_double precision, c_int cheby, c_int exact_Rational)//XX 184
		{
			throw new NotImplementedException();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Set_Compensation(ResampleContext c, c_int sample_Delta, c_int compensation_Distance)//XX 328
		{
			throw new NotImplementedException();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Multiple_Resample(ResampleContext c, AudioData dst, c_int dst_Size, AudioData src, c_int src_Size, out c_int consumed)//XX 349
		{
			throw new NotImplementedException();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static int64_t Get_Delay(SwrContext s, int64_t @base)//XX 408
		{
			throw new NotImplementedException();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static int64_t Get_Out_Samples(SwrContext s, c_int in_Samples)//XX 418
		{
			throw new NotImplementedException();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Resample_Flush(SwrContext s)//XX 437
		{
			throw new NotImplementedException();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Invert_Initial_Buffer(ResampleContext c, AudioData dst, AudioData src, c_int in_Count, ref c_int out_Idx, ref c_int out_Sz)//XX 457
		{
			throw new NotImplementedException();
		}
	}
}
