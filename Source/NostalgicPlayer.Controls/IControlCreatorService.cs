/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Controls
{
	/// <summary>
	/// Helper to instance Control objects
	/// </summary>
	public interface IControlCreatorService
	{
		/// <summary>
		/// Create a new control instance. Will call the InitializeControl
		/// method if exists with dependency injections and setup themes
		/// </summary>
		T GetInstance<T>() where T : Control, new();
	}
}
