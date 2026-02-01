/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public enum AvFmtAvoidNegTs
	{
		/// <summary>
		/// Enabled when required by target format
		/// </summary>
		Auto = -1,

		/// <summary>
		/// Do not shift timestamps even when they are negative
		/// </summary>
		Disabled = 0,

		/// <summary>
		/// Shift timestamps so they are non negative
		/// </summary>
		Make_Non_Negative = 1,

		/// <summary>
		/// Shift timestamps so that they start at 0
		/// </summary>
		Make_Zero = 2
	}
}
