/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// All controls need to implement this interface to support theming
	/// </summary>
	internal interface IThemeControl
	{
		/// <summary>
		/// Apply a theme to the control
		/// </summary>
		void SetTheme(ITheme theme);
	}
}
