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

		private readonly IListItemColors listColors = new StandardListColors();

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
		public Color NormalItemBackgroundStartColor => listColors.NormalBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color NormalItemBackgroundMiddleColor => listColors.NormalBackgroundMiddleColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color NormalItemBackgroundStopColor => listColors.NormalBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color NormalItemTextColor => listColors.NormalTextColor;



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
		public Color SelectedItemBackgroundStartColor => listColors.SelectedBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SelectedItemBackgroundMiddleColor => listColors.SelectedBackgroundMiddleColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SelectedItemBackgroundStopColor => listColors.SelectedBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SelectedItemTextColor => listColors.SelectedTextColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SelectedItemSubSongColor => selectedSubSongColor;
	}
}
