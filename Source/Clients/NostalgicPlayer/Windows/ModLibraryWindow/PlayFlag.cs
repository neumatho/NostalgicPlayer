/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow
{
	/// <summary>
	/// Indicate how to reach on the downloaded module
	/// </summary>
	public enum PlayFlag
	{
		/// <summary>
		/// Don't add or play
		/// </summary>
		JustDownload,

		/// <summary>
		/// Just add to play list
		/// </summary>
		AddToPlayList,

		/// <summary>
		/// Add to play list and start playing
		/// </summary>
		AddToPlayListAndPlay
	}
}
