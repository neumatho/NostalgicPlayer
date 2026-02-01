/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public enum AvClassCategory
	{
		/// <summary>
		/// 
		/// </summary>
		Na = 0,

		/// <summary>
		/// 
		/// </summary>
		Input,

		/// <summary>
		/// 
		/// </summary>
		Output,

		/// <summary>
		/// 
		/// </summary>
		Muxer,

		/// <summary>
		/// 
		/// </summary>
		Demuxer,

		/// <summary>
		/// 
		/// </summary>
		Encoder,

		/// <summary>
		/// 
		/// </summary>
		Decoder,

		/// <summary>
		/// 
		/// </summary>
		Filter,

		/// <summary>
		/// 
		/// </summary>
		Bitstream_Filter,

		/// <summary>
		/// 
		/// </summary>
		SwScaler,

		/// <summary>
		/// 
		/// </summary>
		SwResample,

		/// <summary>
		/// 
		/// </summary>
		HwDevice,

		/// <summary>
		/// 
		/// </summary>
		Device_Video_Output = 40,

		/// <summary>
		/// 
		/// </summary>
		Device_Video_Input,

		/// <summary>
		/// 
		/// </summary>
		Device_Audio_Output,

		/// <summary>
		/// 
		/// </summary>
		Device_Audio_Input,

		/// <summary>
		/// 
		/// </summary>
		Device_Output,

		/// <summary>
		/// 
		/// </summary>
		Device_Input,

		/// <summary>
		/// Not part of ABI/API
		/// </summary>
		Nb
	}
}
