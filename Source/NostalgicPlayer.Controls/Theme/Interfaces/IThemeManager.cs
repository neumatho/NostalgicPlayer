/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Controls.Events;

namespace Polycode.NostalgicPlayer.Controls.Theme.Interfaces
{
	/// <summary>
	/// Theme manager interface
	/// </summary>
	public interface IThemeManager
	{
		/// <summary>
		/// Is called before the theme changes
		/// </summary>
		event EventHandler BeforeThemeChange;

		/// <summary>
		/// Is called when the theme changes
		/// </summary>
		event ThemeChangedEventHandler ThemeChanged;

		/// <summary>
		/// Is called after the theme has changed
		/// </summary>
		event EventHandler AfterThemeChange;

		/// <summary>
		/// Return all available themes that can be used
		/// </summary>
		Guid[] GetAvailableThemes();

		/// <summary>
		/// Return the current theme
		/// </summary>
		ITheme CurrentTheme { get; }

		/// <summary>
		/// Switch to the new theme given
		/// </summary>
		void SwitchTheme(Guid themeId);

		/// <summary>
		/// Refresh all controls
		/// </summary>
		void RefreshControls();
	}
}
