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
	/// Different colors used by text box
	/// </summary>
	internal class PurpleTextBoxColors : ITextBoxColors
	{
		private static readonly Color selectedTextBackgroundColor = Color.FromArgb(180, 125, 185);
		private static readonly Color selectedTextColor = Color.FromArgb(255, 255, 255);

		private readonly IInputControlColors inputControlColors = new PurpleInputControlColors();

		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalBorderColor => inputControlColors.NormalBorderColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalBackgroundColor => inputControlColors.NormalBackgroundColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalTextColor => inputControlColors.NormalTextColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color HoverBorderColor => inputControlColors.HoverBorderColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color HoverBackgroundColor => inputControlColors.HoverBackgroundColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color HoverTextColor => inputControlColors.HoverTextColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color FocusedBorderColor => inputControlColors.FocusedBorderColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color FocusedBackgroundColor => inputControlColors.FocusedBackgroundColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color FocusedTextColor => inputControlColors.FocusedTextColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledBorderColor => inputControlColors.DisabledBorderColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledBackgroundColor => inputControlColors.DisabledBackgroundColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledTextColor => inputControlColors.DisabledTextColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color SelectedTextBackgroundColor => selectedTextBackgroundColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color SelectedTextColor => selectedTextColor;
	}
}
