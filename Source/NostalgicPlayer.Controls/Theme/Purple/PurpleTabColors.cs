/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Theme.Purple
{
	/// <summary>
	/// Purple theme colors for tab control
	/// </summary>
	internal class PurpleTabColors : ITabColors
	{
		private static readonly Color borderColor = Color.FromArgb(140, 125, 160);
		private static readonly Color backgroundColor = Color.FromArgb(250, 248, 252);

		private static readonly Color normalTabBackgroundStartColor = Color.FromArgb(230, 225, 235);
		private static readonly Color normalTabBackgroundStopColor = Color.FromArgb(210, 200, 230);
		private static readonly Color normalTabTextColor = Color.FromArgb(55, 30, 85);

		private static readonly Color selectedTabBackgroundStartColor = Color.FromArgb(190, 170, 230);
		private static readonly Color selectedTabBackgroundStopColor = Color.FromArgb(250, 248, 252);
		private static readonly Color selectedTabTextColor = Color.FromArgb(55, 30, 85);

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
