/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Theme.Office2010Blue
{
	internal class Office2010BlueInputControlColors : IInputControlColors
	{
		private static readonly Color normalBorderColor = Color.FromArgb(177, 192, 214);
		private static readonly Color normalBackgroundColor = Color.FromArgb(255, 255, 255);
		private static readonly Color normalTextColor = Color.FromArgb(0, 0, 0);

		private static readonly Color hoverBorderColor = Color.FromArgb(147, 162, 184);
		private static readonly Color hoverBackgroundColor = normalBackgroundColor;
		private static readonly Color hoverTextColor = normalTextColor;

		private static readonly Color focusedBorderColor = Color.FromArgb(147, 162, 184);
		private static readonly Color focusedBackgroundColor = normalBackgroundColor;
		private static readonly Color focusedTextColor = normalTextColor;

		private static readonly Color disabledBorderColor = Color.FromArgb(177, 187, 198);
		private static readonly Color disabledBackgroundColor = Color.FromArgb(240, 240, 240);
		private static readonly Color disabledTextColor = Color.FromArgb(172, 168, 153);

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
