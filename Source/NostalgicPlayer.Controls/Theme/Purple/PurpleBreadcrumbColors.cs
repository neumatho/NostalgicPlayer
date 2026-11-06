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
	/// Different colors used by breadcrumbs
	/// </summary>
	internal class PurpleBreadcrumbColors : IBreadcrumbColors
	{
		private static readonly Color hoverTextColor = Color.FromArgb(100, 60, 150);
		private static readonly Color pressedTextColor = Color.FromArgb(35, 15, 60);

		private static readonly Color separatorColor = Color.FromArgb(140, 120, 165);

		private readonly ITextColors textColors = new PurpleTextColors();

		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color TextColor => textColors.NormalTextColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color HoverTextColor => hoverTextColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color PressedTextColor => pressedTextColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color CurrentTextColor => textColors.NormalTextColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledTextColor => textColors.DisabledTextColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color SeparatorColor => separatorColor;
	}
}
