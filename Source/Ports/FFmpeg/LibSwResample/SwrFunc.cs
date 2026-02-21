/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample
{
	/// <summary>
	/// 
	/// </summary>
	public static class SwrFunc
	{
		/// <summary></summary>
		internal delegate ResampleContext Resample_Init_Func_Delegate(ResampleContext c, c_int out_Rate, c_int in_Rate, c_int filter_Size, c_int phase_Shift, c_int linear, c_double cutoff, AvSampleFormat format, SwrFilterType filter_Type, c_double kaiser_Beta, c_double precision, c_int cheby, c_int exact_Rational);
		/// <summary></summary>
		internal delegate void Resample_Free_Func_Delegate(ref ResampleContext c);
		/// <summary></summary>
		internal delegate c_int Multiple_Resample_Func_Delegate(ResampleContext c, AudioData dst, c_int dst_Size, AudioData src, c_int src_Size, out c_int consumed);
		/// <summary></summary>
		internal delegate c_int Resample_Flush_Func_Delegate(SwrContext c);
		/// <summary></summary>
		internal delegate c_int Set_Compensation_Func_Delegate(ResampleContext c, c_int sample_Delta, c_int compensation_Distance);
		/// <summary></summary>
		internal delegate int64_t Get_Delay_Func_Delegate(SwrContext s, int64_t @base);
		/// <summary></summary>
		internal delegate c_int Invert_Initial_Buffer_Func_Delegate(ResampleContext c, AudioData dst, AudioData src, c_int src_Size, ref c_int dst_Idx, ref c_int dst_Count);
		/// <summary></summary>
		internal delegate int64_t Get_Out_Samples_Func_Delegate(SwrContext s, c_int in_Samples);

		/// <summary></summary>
		public delegate void Conv_Func_Type_Delegate(CPointer<uint8_t> po, CPointer<uint8_t> pi, c_int @is, c_int os, CPointer<uint8_t> end);
		/// <summary></summary>
		public delegate void Simd_Func_Type_Delegate(CPointer<uint8_t> dst, CPointer<uint8_t> src, c_int len);

		/// <summary></summary>
		public delegate void Resample_One_Delegate(IPointer dst, IPointer src, c_int n, int64_t index, int64_t incr);
		/// <summary></summary>
		public delegate c_int Resample_Common_Delegate(ResampleContext c, IPointer dst, IPointer src, c_int n, c_int update_Ctx);
		/// <summary></summary>
		public delegate c_int Resample_Linear_Delegate(ResampleContext c, IPointer dst, IPointer src, c_int n, c_int update_Ctx);
	}
}
