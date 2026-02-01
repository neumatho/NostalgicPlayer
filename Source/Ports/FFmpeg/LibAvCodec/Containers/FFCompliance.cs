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
	public enum FFCompliance
	{
		/// <summary>
		/// Strictly conform to an older more strict version of the spec or reference software
		/// </summary>
		Very_Strict = 2,

		/// <summary>
		/// Strictly conform to all the things in the spec no matter what consequences
		/// </summary>
		Strict = 1,

		/// <summary>
		/// 
		/// </summary>
		Normal = 0,

		/// <summary>
		/// Allow unofficial extensions
		/// </summary>
		Unofficial = -1,

		/// <summary>
		/// Allow nonstandardized experimental things
		/// </summary>
		Experimental = -2
	}
}
