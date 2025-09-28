/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Controls.Containers.Events;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Theme manager interface
	/// </summary>
	public interface IThemeManager
	{
		/// <summary>
		/// Is called when the theme changes
		/// </summary>
		event ThemeChangedEventHandler ThemeChanged;

		/// <summary>
		/// Return the current theme
		/// </summary>
		public ITheme CurrentTheme { get; }

		/// <summary>
		/// Switch to the new theme given
		/// </summary>
		public void SwitchTheme(ITheme newTheme);

		/// <summary>
		/// Refresh all controls
		/// </summary>
		public void RefreshControls();
	}
}
