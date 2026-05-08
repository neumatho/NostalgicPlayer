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
	/// Different colors used by module list
	/// </summary>
	internal class StandardModuleListColors : IModuleListColors
	{
		private static readonly Color backgroundColor = Color.FromArgb(255, 255, 255);
		private static readonly Color dropLineColor = Color.FromArgb(0, 120, 215);

		private static readonly Color normalSubSongColor = Color.FromArgb(159, 81, 255);
		private static readonly Color selectedSubSongColor = Color.FromArgb(159, 81, 255);

		private readonly IListItemColors listItemColors = new StandardListItemColors();

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
		public Color DropLineColor => dropLineColor;



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
		public Color NormalItemSubSongColor => normalSubSongColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SelectedItemBackgroundStartColor => listItemColors.SelectedBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SelectedItemBackgroundMiddleColor => listItemColors.SelectedBackgroundMiddleColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SelectedItemBackgroundStopColor => listItemColors.SelectedBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SelectedItemTextColor => listItemColors.SelectedTextColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SelectedItemSubSongColor => selectedSubSongColor;
	}
}
