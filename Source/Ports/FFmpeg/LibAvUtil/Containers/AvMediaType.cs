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
	public enum AvMediaType
	{
		/// <summary>
		/// Usually treated as AVMEDIA_TYPE_DATA
		/// </summary>
		Unknown = -1,

		/// <summary>
		/// 
		/// </summary>
		Video,

		/// <summary>
		/// 
		/// </summary>
		Audio,

		/// <summary>
		/// Opaque data information usually continuous
		/// </summary>
		Data,

		/// <summary>
		/// 
		/// </summary>
		Subtitle,

		/// <summary>
		/// Opaque data information usually sparse
		/// </summary>
		Attachment,

		/// <summary>
		/// 
		/// </summary>
		Nb
	}
}
