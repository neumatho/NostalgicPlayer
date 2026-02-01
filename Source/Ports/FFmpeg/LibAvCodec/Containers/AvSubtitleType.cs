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
	public enum AvSubtitleType
	{

		/// <summary>
		/// 
		/// </summary>
		None,

		/// <summary>
		/// A bitmap, pict will be set
		/// </summary>
		Bitmap,

		/// <summary>
		/// Plain text, the text field must be set by the decoder and is
		/// authoritative. ass and pict fields may contain approximations
		/// </summary>
		Text,

		/// <summary>
		/// Formatted text, the ass field must be set by the decoder and is
		/// authoritative. pict and text fields may contain approximations
		/// </summary>
		Ass
	}
}
