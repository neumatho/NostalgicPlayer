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
	/// Different purple inspired colors used by menu strips
	/// </summary>
	internal class PurpleMenuStripColors : IMenuStripColors
	{
		private static readonly Color normalMenuBarItemTextColor = Color.FromArgb(55, 30, 85);

		private static readonly Color hoverMenuBarItemBorderColor = Color.FromArgb(167, 171, 176);
		private static readonly Color hoverMenuBarItemBackgroundStartColor = Color.FromArgb(190, 170, 230);
		private static readonly Color hoverMenuBarItemBackgroundStopColor = Color.FromArgb(250, 248, 252);
		private static readonly Color hoverMenuBarItemTextColor = Color.FromArgb(55, 30, 85);

		private static readonly Color openMenuBarItemBorderColor = Color.FromArgb(167, 171, 176);
		private static readonly Color openMenuBarItemBackgroundStartColor = Color.FromArgb(250, 248, 252);
		private static readonly Color openMenuBarItemBackgroundStopColor = Color.FromArgb(250, 248, 252);
		private static readonly Color openMenuBarItemTextColor = Color.FromArgb(55, 30, 85);

		private readonly IFormColors formColors = new PurpleFormColors();
		private readonly IMenuColors menuColors = new PurpleMenuColors();

		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color MenuBarBackgroundColor => formColors.ActivatedFormInnerStopColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalMenuBarItemBorderColor => formColors.ActivatedFormInnerStopColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalMenuBarItemBackgroundStartColor => formColors.ActivatedFormInnerStopColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalMenuBarItemBackgroundStopColor => formColors.ActivatedFormInnerStopColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalMenuBarItemTextColor => normalMenuBarItemTextColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color HoverMenuBarItemBorderColor => hoverMenuBarItemBorderColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color HoverMenuBarItemBackgroundStartColor => hoverMenuBarItemBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color HoverMenuBarItemBackgroundStopColor => hoverMenuBarItemBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color HoverMenuBarItemTextColor => hoverMenuBarItemTextColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color OpenMenuBarItemBorderColor => openMenuBarItemBorderColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color OpenMenuBarItemBackgroundStartColor => openMenuBarItemBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color OpenMenuBarItemBackgroundStopColor => openMenuBarItemBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color OpenMenuBarItemTextColor => openMenuBarItemTextColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DropDownBorderColor => menuColors.BorderColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DropDownSeparatorColor => menuColors.DropDownSeparatorColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalDropDownItemBackgroundStartColor => menuColors.NormalItemBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalDropDownItemBackgroundMiddleColor => menuColors.NormalItemBackgroundMiddleColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalDropDownItemBackgroundStopColor => menuColors.NormalItemBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalDropDownItemTextColor => menuColors.NormalItemTextColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color HoverDropDownItemBackgroundStartColor => menuColors.HoverItemBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color HoverDropDownItemBackgroundMiddleColor => menuColors.HoverItemBackgroundMiddleColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color HoverDropDownItemBackgroundStopColor => menuColors.HoverItemBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color HoverDropDownItemTextColor => menuColors.HoverItemTextColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledDropDownItemBackgroundStartColor => menuColors.DisabledItemBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledDropDownItemBackgroundMiddleColor => menuColors.DisabledItemBackgroundMiddleColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledDropDownItemBackgroundStopColor => menuColors.DisabledItemBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledDropDownItemTextColor => menuColors.DisabledItemTextColor;
	}
}
