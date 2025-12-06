/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Theme.Purple
{
	internal class PurpleInputControlColors : IInputControlColors
	{
		private static readonly Color normalBorderColor = Color.FromArgb(140, 125, 160);
		private static readonly Color normalBackgroundColor = Color.FromArgb(230, 225, 235);
		private static readonly Color normalTextColor = Color.FromArgb(55, 30, 85);

		private static readonly Color hoverBorderColor = Color.FromArgb(90, 100, 205);
		private static readonly Color hoverBackgroundColor = normalBackgroundColor;
		private static readonly Color hoverTextColor = normalTextColor;

		private static readonly Color focusedBorderColor = Color.FromArgb(255, 255, 255);
		private static readonly Color focusedBackgroundColor = normalBackgroundColor;
		private static readonly Color focusedTextColor = normalTextColor;

		private static readonly Color disabledBorderColor = Color.FromArgb(180, 180, 180);
		private static readonly Color disabledBackgroundColor = Color.FromArgb(235, 235, 235);
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
		public Color NormalBackgroundColor => normalBackgroundColor;



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
		public Color HoverBackgroundColor => hoverBackgroundColor;



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
		public Color FocusedBorderColor => focusedBorderColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color FocusedBackgroundColor => focusedBackgroundColor;



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
		public Color DisabledBackgroundColor => disabledBackgroundColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color DisabledTextColor => disabledTextColor;
	}
}
