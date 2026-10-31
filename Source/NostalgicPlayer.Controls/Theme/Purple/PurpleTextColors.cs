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
	/// Purple text colors
	/// </summary>
	internal class PurpleTextColors : ITextColors
	{
		private static readonly Color normalTextColor = Color.FromArgb(55, 30, 85);
		private static readonly Color hoverTextColor = Color.FromArgb(55, 30, 85);
		private static readonly Color pressedTextColor = Color.FromArgb(55, 30, 85);
		private static readonly Color disabledTextColor = Color.FromArgb(168, 168, 168);

		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalTextColor => normalTextColor;



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
		public Color DisabledTextColor => disabledTextColor;
	}
}
