/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// Enumeration of the mode types of Variable Bitrate
	/// </summary>
	public enum Mpg123_Vbr
	{
		/// <summary>
		/// Constant Bitrate Mode (default)
		/// </summary>
		Cbr,
		/// <summary>
		/// Variable Bitrate Mode
		/// </summary>
		Vbr,
		/// <summary>
		/// Average Bitrate Mode
		/// </summary>
		Abr
	}
}
