/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.Loader
{
	/// <summary>
	/// Special loader factory for Audius loader
	/// </summary>
	public interface IAudiusLoaderFactory
	{
		/// <summary>
		/// Create specific loader for Audius
		/// </summary>
		AudiusLoader CreateAudiusLoader();
	}
}
