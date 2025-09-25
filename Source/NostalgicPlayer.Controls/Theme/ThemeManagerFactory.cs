/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Theme
{
	/// <summary>
	/// Factory class for theme manager
	/// </summary>
	public static class ThemeManagerFactory
	{
		private static IThemeManager themeManager;

		/********************************************************************/
		/// <summary>
		/// Create a new instance of the theme manager if not created already
		/// and return the instance
		/// </summary>
		/********************************************************************/
		public static IThemeManager GetThemeManager()
		{
			if (themeManager == null)
				themeManager = new ThemeManager();

			return themeManager;
		}
	}
}
