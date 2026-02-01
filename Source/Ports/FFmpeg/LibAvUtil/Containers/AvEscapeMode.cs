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
	public enum AvEscapeMode
	{
		/// <summary>
		/// Use auto-selected escaping mode
		/// </summary>
		Auto,

		/// <summary>
		/// Use backslash escaping
		/// </summary>
		Backslash,

		/// <summary>
		/// Use single-quote escaping
		/// </summary>
		Quote,

		/// <summary>
		/// Use XML non-markup character data escaping
		/// </summary>
		Xml
	}
}
