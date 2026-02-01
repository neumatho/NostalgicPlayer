/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public enum FFSubCharEncMode
	{
		/// <summary>
		/// Do nothing (demuxer outputs a stream supposed to be already in UTF-8, or the codec is bitmap for instance)
		/// </summary>
		Do_Nothing = -1,

		/// <summary>
		/// libavcodec will select the mode itself
		/// </summary>
		Automatic = 0,

		/// <summary>
		/// The AVPacket data needs to be recoded to UTF-8 before being fed to the decoder, requires iconv
		/// </summary>
		Pre_Decoder = 1,

		/// <summary>
		/// Neither convert the subtitles, nor check them for valid UTF-8
		/// </summary>
		Ignore = 2
	}
}
