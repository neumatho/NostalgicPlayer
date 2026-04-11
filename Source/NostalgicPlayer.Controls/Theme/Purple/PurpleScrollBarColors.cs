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
	/// Purple theme colors for ScrollBar
	/// </summary>
	internal class PurpleScrollBarColors : IScrollBarColors
	{
		private static readonly Color backgroundColor = Color.FromArgb(245, 240, 250);

		private static readonly Color normalArrowColor = Color.FromArgb(140, 125, 160);

		private static readonly Color hoverArrowColor = Color.FromArgb(90, 100, 205);

		private static readonly Color pressedArrowColor = Color.FromArgb(55, 30, 85);

		private static readonly Color disabledArrowColor = Color.FromArgb(180, 180, 180);

		private static readonly Color normalThumbColor = Color.FromArgb(210, 195, 225);

		private static readonly Color hoverThumbColor = Color.FromArgb(190, 170, 210);

		private static readonly Color pressedThumbColor = Color.FromArgb(165, 150, 190);

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
		public Color NormalArrowColor => normalArrowColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color HoverArrowColor => hoverArrowColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color PressedArrowColor => pressedArrowColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color DisabledArrowColor => disabledArrowColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color NormalThumbColor => normalThumbColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color HoverThumbColor => hoverThumbColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color PressedThumbColor => pressedThumbColor;
	}
}
