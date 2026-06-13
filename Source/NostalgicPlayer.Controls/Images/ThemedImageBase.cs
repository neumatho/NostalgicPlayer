/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Controls.Events;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Images
{
	/// <summary>
	/// Base class to all image classes which used themed colors
	/// </summary>
	internal abstract class ThemedImageBase : ImageBase
	{
		private readonly IThemeManager themeManager;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected ThemedImageBase(IThemeManager themeManager)
		{
			this.themeManager = themeManager;

			themeManager.ThemeChanged += ThemeChanged;
		}



		/********************************************************************/
		/// <summary>
		/// Dispose all the images
		/// </summary>
		/********************************************************************/
		public override void Dispose()
		{
			themeManager.ThemeChanged -= ThemeChanged;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the theme changes. Update all controls and redraw
		/// itself
		/// </summary>
		/********************************************************************/
		private void ThemeChanged(object sender, ThemeChangedEventArgs e)
		{
			FlushImages();
		}



		/********************************************************************/
		/// <summary>
		/// Flush images
		/// </summary>
		/********************************************************************/
		protected abstract void FlushImages();



		/********************************************************************/
		/// <summary>
		/// Return current themed colors
		/// </summary>
		/********************************************************************/
		protected IImageColors CurrentColors => themeManager.CurrentTheme.ImageColors;
	}
}
