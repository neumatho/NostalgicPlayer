/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Sources
{
	/// <summary>
	/// Creates the different sources
	/// </summary>
	public interface ISourceFactory
	{
		/// <summary>
		/// Get a ModLand source
		/// </summary>
		IModLibrarySource GetModLandSource();
	}
}
