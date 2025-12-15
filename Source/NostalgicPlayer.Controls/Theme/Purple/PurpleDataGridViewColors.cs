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
	/// Purple theme colors for DataGridView
	/// </summary>
	internal class PurpleDataGridViewColors : IDataGridViewColors
	{
		private static readonly Color backgroundColor = Color.FromArgb(255, 255, 255);
		private static readonly Color normalHeaderBorderColor = Color.FromArgb(140, 125, 160);
		private static readonly Color normalHeaderBackgroundStartColor = Color.FromArgb(240, 235, 250);
		private static readonly Color normalHeaderBackgroundStopColor = Color.FromArgb(220, 210, 235);
		private static readonly Color normalHeaderTextColor = Color.FromArgb(55, 30, 85);
		private static readonly Color pressedHeaderBorderColor = Color.FromArgb(140, 125, 160);
		private static readonly Color pressedHeaderBackgroundStartColor = Color.FromArgb(195, 183, 215);
		private static readonly Color pressedHeaderBackgroundStopColor = Color.FromArgb(240, 235, 250);
		private static readonly Color pressedHeaderTextColor = Color.FromArgb(55, 30, 85);

		private readonly IListItemColors listColors = new PurpleListColors();

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
		public Color NormalHeaderBorderColor => normalHeaderBorderColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color NormalHeaderBackgroundStartColor => normalHeaderBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color NormalHeaderBackgroundStopColor => normalHeaderBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color NormalHeaderTextColor => normalHeaderTextColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color PressedHeaderBorderColor => pressedHeaderBorderColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color PressedHeaderBackgroundStartColor => pressedHeaderBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color PressedHeaderBackgroundStopColor => pressedHeaderBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color PressedHeaderTextColor => pressedHeaderTextColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color NormalCellBackgroundStartColor => listColors.NormalBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color NormalCellBackgroundMiddleColor => listColors.NormalBackgroundMiddleColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color NormalCellBackgroundStopColor => listColors.NormalBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color NormalCellTextColor => listColors.NormalTextColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SelectedCellBackgroundStartColor => listColors.SelectedBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SelectedCellBackgroundMiddleColor => listColors.SelectedBackgroundMiddleColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SelectedCellBackgroundStopColor => listColors.SelectedBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SelectedCellTextColor => listColors.SelectedTextColor;
	}
}
