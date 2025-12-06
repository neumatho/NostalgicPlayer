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
	/// Different colors used by buttons
	/// </summary>
	internal class StandardButtonColors : IButtonColors
	{
		private static readonly Color normalBorderColor = Color.FromArgb(171, 186, 208);
		private static readonly Color normalBackgroundStartColor = Color.FromArgb(225, 237, 250);
		private static readonly Color normalBackgroundStopColor = Color.FromArgb(208, 223, 238);
		private static readonly Color normalTextColor = Color.FromArgb(30, 57, 91);

		private static readonly Color hoverBorderColor = Color.FromArgb(237, 202, 87);
		private static readonly Color hoverBackgroundStartColor = Color.FromArgb(249, 227, 136);
		private static readonly Color hoverBackgroundStopColor = Color.FromArgb(255, 237, 136);
		private static readonly Color hoverTextColor = Color.FromArgb(30, 57, 91);

		private static readonly Color pressedBorderColor = Color.FromArgb(227, 182, 67);
		private static readonly Color pressedBackgroundStartColor = Color.FromArgb(229, 207, 116);
		private static readonly Color pressedBackgroundStopColor = Color.FromArgb(235, 217, 116);
		private static readonly Color pressedTextColor = Color.FromArgb(30, 57, 91);

		private static readonly Color focusedBorderColor = Color.FromArgb(117, 144, 175);
		private static readonly Color focusedBackgroundStartColor = Color.FromArgb(255, 255, 255);
		private static readonly Color focusedBackgroundStopColor = Color.FromArgb(210, 229, 250);
		private static readonly Color focusedTextColor = Color.FromArgb(30, 57, 91);

		private static readonly Color disabledBorderColor = Color.FromArgb(180, 180, 180);
		private static readonly Color disabledBackgroundStartColor = Color.FromArgb(235, 235, 235);
		private static readonly Color disabledBackgroundStopColor = Color.FromArgb(235, 235, 235);
		private static readonly Color disabledTextColor = Color.FromArgb(168, 168, 168);

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color NormalBorderColor => normalBorderColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color NormalBackgroundStartColor => normalBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color NormalBackgroundStopColor => normalBackgroundStopColor;



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
		public Color HoverBorderColor => hoverBorderColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color HoverBackgroundStartColor => hoverBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color HoverBackgroundStopColor => hoverBackgroundStopColor;



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
		public Color PressedBorderColor => pressedBorderColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color PressedBackgroundStartColor => pressedBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color PressedBackgroundStopColor => pressedBackgroundStopColor;



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
		public Color FocusedBorderColor => focusedBorderColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color FocusedBackgroundStartColor => focusedBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color FocusedBackgroundStopColor => focusedBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color FocusedTextColor => focusedTextColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color DisabledBorderColor => disabledBorderColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color DisabledBackgroundStartColor => disabledBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color DisabledBackgroundStopColor => disabledBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color DisabledTextColor => disabledTextColor;
	}
}
