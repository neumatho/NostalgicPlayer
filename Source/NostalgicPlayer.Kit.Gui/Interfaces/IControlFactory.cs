/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Kit.Gui.Interfaces
{
	/// <summary>
	/// Helper for agents to create control instances with dependency injections
	/// </summary>
	public interface IControlFactory
	{
		/// <summary>
		/// Create a new control instance. Will call the InitializeControl
		/// method if exists with dependency injections
		/// </summary>
		T GetInstance<T>(params object[] extraArguments) where T : Control, IControl, new();
	}
}
