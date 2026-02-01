/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Correlation between the alpha channel and color values
	/// </summary>
	public enum AvAlphaMode
	{
		/// <summary>
		/// Unknown alpha handling, or no alpha channel
		/// </summary>
		Unspecified = 0,

		/// <summary>
		/// Alpha channel is multiplied into color values
		/// </summary>
		Premultiplied = 1,

		/// <summary>
		/// Alpha channel is independent of color values
		/// </summary>
		Straight = 2,

		/// <summary>
		/// Not part of ABI
		/// </summary>
		Nb
	}
}
