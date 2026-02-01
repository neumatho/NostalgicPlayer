/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// Options for behavior on timestamp wrap detection
	/// </summary>
	public enum AvPtsWrap
	{
		/// <summary>
		/// Ignore the wrap
		/// </summary>
		Ignore = 0,

		/// <summary>
		/// Add the format specific offset on wrap detection
		/// </summary>
		Add_Offset = 1,

		/// <summary>
		/// Subtract the format specific offset on wrap detection
		/// </summary>
		Sub_Offset = -1
	}
}
