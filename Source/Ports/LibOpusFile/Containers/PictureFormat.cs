/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.OpusFile.Containers
{
	/// <summary>
	/// Picture format
	/// </summary>
	public enum PictureFormat
	{
		/// <summary>
		/// The MIME type was not recognized, or the image data did not match the
		/// declared MIME type
		/// </summary>
		Unknown = -1,

		/// <summary>
		/// The MIME type indicates the image data is really a URL
		/// </summary>
		Url = 0,

		/// <summary></summary>
		Jpeg,
		/// <summary></summary>
		Png,
		/// <summary></summary>
		Gif
	}
}
