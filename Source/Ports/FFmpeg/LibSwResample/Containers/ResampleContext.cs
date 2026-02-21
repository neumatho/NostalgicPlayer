/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class ResampleContext : AvClass
	{
		/// <summary>
		/// AVClass used for AVOption and av_log()
		/// </summary>
		public AvClass Av_Class => this;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<uint8_t> Filter_Bank;

		/// <summary>
		/// 
		/// </summary>
		public c_int Filter_Length;

		/// <summary>
		/// 
		/// </summary>
		public c_int Filter_Alloc;

		/// <summary>
		/// 
		/// </summary>
		public c_int Ideal_Dst_Incr;

		/// <summary>
		/// 
		/// </summary>
		public c_int Dst_Incr;

		/// <summary>
		/// 
		/// </summary>
		public c_int Dst_Incr_Div;

		/// <summary>
		/// 
		/// </summary>
		public c_int Dst_Incr_Mod;

		/// <summary>
		/// 
		/// </summary>
		public c_int Index;

		/// <summary>
		/// 
		/// </summary>
		public c_int Frac;

		/// <summary>
		/// 
		/// </summary>
		public c_int Src_Incr;

		/// <summary>
		/// 
		/// </summary>
		public c_int Compensation_Distance;

		/// <summary>
		/// 
		/// </summary>
		public c_int Phase_Count;

		/// <summary>
		/// 
		/// </summary>
		public c_int Linear;

		/// <summary>
		/// 
		/// </summary>
		public SwrFilterType Filter_Type;

		/// <summary>
		/// 
		/// </summary>
		public c_double Kaiser_Beta;

		/// <summary>
		/// 
		/// </summary>
		public c_double Factor;

		/// <summary>
		/// 
		/// </summary>
		public AvSampleFormat Format;

		/// <summary>
		/// 
		/// </summary>
		public c_int FElem_Size;

		/// <summary>
		/// 
		/// </summary>
		public c_int Filter_Shift;

		/// <summary>
		/// Desired phase_count when compensation is enabled
		/// </summary>
		public c_int Phase_Count_Compensation;

		// <summary>
		// 
		// </summary>
/*		(
			SwrFunc.Resample_One_Delegate Resample_One,
			SwrFunc.Resample_Common_Delegate Resample_Common,
			SwrFunc.Resample_Linear_Delegate Resample_Linear
		) Dsp;*/
	}
}
