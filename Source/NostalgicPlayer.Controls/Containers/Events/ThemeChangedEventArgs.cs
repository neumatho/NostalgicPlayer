/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Containers.Events
{
	/// <summary></summary>
	public delegate void ThemeChangedEventHandler(object sender, ThemeChangedEventArgs e);

	/// <summary>
	/// Event class holding needed information when sending a theme changed event
	/// </summary>
	public class ThemeChangedEventArgs : EventArgs
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		internal ThemeChangedEventArgs(ITheme newTheme)
		{
			NewTheme = newTheme;
		}



		/********************************************************************/
		/// <summary>
		/// Holding the new theme
		/// </summary>
		/********************************************************************/
		public ITheme NewTheme
		{
			get;
		}
	}
}
