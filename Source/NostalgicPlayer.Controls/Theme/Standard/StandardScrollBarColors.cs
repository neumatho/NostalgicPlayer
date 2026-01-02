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
	/// Standard colors for ScrollBar
	/// </summary>
	internal class StandardScrollBarColors : IScrollBarColors
	{
		private static readonly Color backgroundColor = Color.FromArgb(240, 240, 240);

		private static readonly Color normalArrowColor = Color.FromArgb(134, 137, 153);

		private static readonly Color hoverArrowColor = Color.FromArgb(73, 113, 185);

		private static readonly Color pressedArrowColor = Color.FromArgb(30, 57, 91);

		private static readonly Color disabledArrowColor = Color.FromArgb(168, 168, 168);

		private static readonly Color normalThumbColor = Color.FromArgb(194, 195, 201);

		private static readonly Color hoverThumbColor = Color.FromArgb(128, 129, 134);

		private static readonly Color pressedThumbColor = Color.FromArgb(101, 102, 108);

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
