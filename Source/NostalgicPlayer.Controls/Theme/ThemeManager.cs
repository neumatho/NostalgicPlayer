/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Controls.Events;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;
using Polycode.NostalgicPlayer.Controls.Theme.Standard;

namespace Polycode.NostalgicPlayer.Controls.Theme
{
	/// <summary>
	/// This class controls and manages all the themes and make
	/// sure that forms and controls are updated when the theme
	/// changes
	/// </summary>
	internal class ThemeManager : IThemeManager, IDisposable
	{
		private ITheme theme;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ThemeManager()
		{
			theme = new StandardTheme();
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			if (theme is IDisposable disposable)
				disposable.Dispose();

			theme = null;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the theme changes
		/// </summary>
		/********************************************************************/
		public event ThemeChangedEventHandler ThemeChanged;



		/********************************************************************/
		/// <summary>
		/// Return the current theme
		/// </summary>
		/********************************************************************/
		public ITheme CurrentTheme => theme;



		/********************************************************************/
		/// <summary>
		/// Switch to the new theme given
		/// </summary>
		/********************************************************************/
		public void SwitchTheme(ITheme newTheme)
		{
			if (theme is IDisposable disposable)
				disposable.Dispose();

			theme = newTheme;

			RefreshControls();
		}



		/********************************************************************/
		/// <summary>
		/// Refresh all controls
		/// </summary>
		/********************************************************************/
		public void RefreshControls()
		{
			if (ThemeChanged != null)
				ThemeChanged(this, new ThemeChangedEventArgs(theme));
		}
	}
}
