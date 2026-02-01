/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class FFStreamInfo
	{
		/// <summary>
		/// 
		/// </summary>
		public int64_t Last_Dts;

		/// <summary>
		/// 
		/// </summary>
		public int64_t Duration_Gcd;

		/// <summary>
		/// 
		/// </summary>
		public c_int Duration_Count;

		/// <summary>
		/// 
		/// </summary>
		public int64_t Rfps_Duration_Sum;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<c_double[][]> Duration_Error;

		/// <summary>
		/// 
		/// </summary>
		public int64_t Codec_Info_Duration;

		/// <summary>
		/// 
		/// </summary>
		public int64_t Codec_Info_Duration_Fields;

		/// <summary>
		/// 
		/// </summary>
		public c_int Frame_Delay_Evidence;

		/// <summary>
		/// 
		/// </summary>
		public c_int Found_Decoder;

		/// <summary>
		/// 
		/// </summary>
		public int64_t Last_Duration;

		/// <summary>
		/// 
		/// </summary>
		public int64_t Fps_First_Dts;

		/// <summary>
		/// 
		/// </summary>
		public c_int Fps_First_Dts_Idx;

		/// <summary>
		/// 
		/// </summary>
		public int64_t Fps_Last_Dts;

		/// <summary>
		/// 
		/// </summary>
		public c_int Fps_Last_Dts_Idx;
	}
}
