/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// List of possible view types
	/// </summary>
	public enum AvStereo3DView
	{
		/// <summary>
		/// Frame contains two packed views
		/// </summary>
		Packed,

		/// <summary>
		/// Frame contains only the left view
		/// </summary>
		Left,

		/// <summary>
		/// Frame contains only the right view
		/// </summary>
		Right,

		/// <summary>
		/// Content is unspecified
		/// </summary>
		Unspec
	}
}
