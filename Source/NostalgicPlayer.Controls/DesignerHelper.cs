/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Controls
{
	/// <summary>
	/// Helper class with designer methods
	/// </summary>
	internal static class DesignerHelper
	{
		/********************************************************************/
		/// <summary>
		/// Check if the control is in design mode by walking up the parent
		/// chain. This is needed because DesignMode returns false for child
		/// controls that are not directly sited on the designer
		/// </summary>
		/********************************************************************/
		public static bool IsInDesignMode(Control ctrl)
		{
			while (ctrl != null)
			{
				if (ctrl.Site?.DesignMode == true)
					return true;

				ctrl = ctrl.Parent;
			}

			return false;
		}
	}
}
