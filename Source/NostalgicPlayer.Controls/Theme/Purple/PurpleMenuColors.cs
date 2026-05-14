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
	/// Different purple inspired colors used by menus
	/// </summary>
	internal class PurpleMenuColors : IMenuColors
	{
		private static readonly Color dropDownBorderColor = Color.FromArgb(167, 171, 176);
		private static readonly Color dropDownSeparatorColor = Color.FromArgb(227, 229, 230);

		private readonly IListItemColors listItemColors = new PurpleListItemColors();

		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color BorderColor => dropDownBorderColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DropDownSeparatorColor => dropDownSeparatorColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalItemBackgroundStartColor => listItemColors.NormalBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalItemBackgroundMiddleColor => listItemColors.NormalBackgroundMiddleColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalItemBackgroundStopColor => listItemColors.NormalBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalItemTextColor => listItemColors.NormalTextColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color HoverItemBackgroundStartColor => listItemColors.SelectedBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color HoverItemBackgroundMiddleColor => listItemColors.SelectedBackgroundMiddleColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color HoverItemBackgroundStopColor => listItemColors.SelectedBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color HoverItemTextColor => listItemColors.SelectedTextColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledItemBackgroundStartColor => listItemColors.DisabledBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledItemBackgroundMiddleColor => listItemColors.DisabledBackgroundMiddleColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledItemBackgroundStopColor => listItemColors.DisabledBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledItemTextColor => listItemColors.DisabledTextColor;
	}
}
