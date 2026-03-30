/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Theme.Standard
{
	/// <summary>
	/// Standard colors for tab control
	/// </summary>
	internal class StandardTabColors : ITabColors
	{
		private static readonly Color borderColor = Color.FromArgb(133, 158, 191);
		private static readonly Color backgroundColor = Color.FromArgb(255, 255, 255);

		private static readonly Color normalTabBackgroundStartColor = Color.FromArgb(225, 237, 250);
		private static readonly Color normalTabBackgroundStopColor = Color.FromArgb(208, 223, 238);
		private static readonly Color normalTabTextColor = Color.FromArgb(30, 57, 91);

		private static readonly Color selectedTabBackgroundStartColor = Color.FromArgb(255, 225, 112);
		private static readonly Color selectedTabBackgroundStopColor = Color.FromArgb(255, 255, 255);
		private static readonly Color selectedTabTextColor = Color.FromArgb(30, 57, 91);

		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color BorderColor => borderColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color BackgroundColor => backgroundColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalTabBackgroundStartColor => normalTabBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalTabBackgroundStopColor => normalTabBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalTabTextColor => normalTabTextColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color SelectedTabBackgroundStartColor => selectedTabBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color SelectedTabBackgroundStopColor => selectedTabBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color SelectedTabTextColor => selectedTabTextColor;
	}
}
