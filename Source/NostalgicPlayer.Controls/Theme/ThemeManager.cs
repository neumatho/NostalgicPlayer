/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Controls.Events;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;
using Polycode.NostalgicPlayer.Controls.Theme.Purple;
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
		private readonly ITheme[] embeddedThemes =
		[
			new StandardTheme(),
			new PurpleTheme()
		];

		private ITheme currentTheme;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ThemeManager()
		{
			currentTheme = embeddedThemes[0];
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			foreach (ITheme theme in embeddedThemes)
			{
				if (theme is IDisposable disposable)
					disposable.Dispose();
			}

			currentTheme = null;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the theme changes
		/// </summary>
		/********************************************************************/
		public event ThemeChangedEventHandler ThemeChanged;



		/********************************************************************/
		/// <summary>
		/// Return all available themes that can be used
		/// </summary>
		/********************************************************************/
		public Guid[] GetAvailableThemes()
		{
			return GetThemes().Select(x => x.Id).ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Return the current theme
		/// </summary>
		/********************************************************************/
		public ITheme CurrentTheme => currentTheme;



		/********************************************************************/
		/// <summary>
		/// Switch to the new theme given
		/// </summary>
		/********************************************************************/
		public void SwitchTheme(Guid themeId)
		{
			currentTheme = GetThemes().FirstOrDefault(x => x.Id == themeId);
			if (currentTheme == null)
				currentTheme = embeddedThemes[0];

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
				ThemeChanged(this, new ThemeChangedEventArgs(currentTheme));
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Return all available themes that can be used
		/// </summary>
		/********************************************************************/
		private IEnumerable<ITheme> GetThemes()
		{
			foreach (ITheme theme in embeddedThemes)
				yield return theme;
		}
		#endregion
	}
}
