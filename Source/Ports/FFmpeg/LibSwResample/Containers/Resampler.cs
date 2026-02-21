/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class Resampler
	{
		/// <summary>
		/// 
		/// </summary>
		public SwrFunc.Resample_Init_Func_Delegate Init;

		/// <summary>
		/// 
		/// </summary>
		public SwrFunc.Resample_Free_Func_Delegate Free;

		/// <summary>
		/// 
		/// </summary>
		public SwrFunc.Multiple_Resample_Func_Delegate Multiple_Resample;

		/// <summary>
		/// 
		/// </summary>
		public SwrFunc.Resample_Flush_Func_Delegate Flush;

		/// <summary>
		/// 
		/// </summary>
		public SwrFunc.Set_Compensation_Func_Delegate Set_Compensation;

		/// <summary>
		/// 
		/// </summary>
		public SwrFunc.Get_Delay_Func_Delegate Get_Delay;

		/// <summary>
		/// 
		/// </summary>
		public SwrFunc.Invert_Initial_Buffer_Func_Delegate Invert_Initial_Buffer;

		/// <summary>
		/// 
		/// </summary>
		public SwrFunc.Get_Out_Samples_Func_Delegate Get_Out_Samples;
	}
}
