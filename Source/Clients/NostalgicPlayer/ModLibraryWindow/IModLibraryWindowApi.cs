/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/

using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.ModLibraryWindow
{
	/// <summary>
	/// API for the Module Library window
	/// </summary>
	public interface IModLibraryWindowApi
	{
		/********************************************************************/
		/// <summary>
		/// Return the form of the Module Library window
		/// </summary>
		/********************************************************************/
		Form Form
		{
			get;
		}
	}
}