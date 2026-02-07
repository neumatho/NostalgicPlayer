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
	/// Different colors used by combo box
	/// </summary>
	internal class StandardListColors : IListItemColors
	{
		private static readonly Color normalBackgroundStartColor = Color.FromArgb(255, 255, 255);
		private static readonly Color normalBackgroundMiddleColor = Color.FromArgb(255, 255, 255);
		private static readonly Color normalBackgroundStopColor = Color.FromArgb(255, 255, 255);
		private static readonly Color normalTextColor = Color.FromArgb(0, 0, 0);

		private static readonly Color selectedBackgroundStartColor = Color.FromArgb(255, 225, 112);
		private static readonly Color selectedBackgroundMiddleColor = Color.FromArgb(255, 216, 108);
		private static readonly Color selectedBackgroundStopColor = Color.FromArgb(255, 237, 123);
		private static readonly Color selectedTextColor = Color.FromArgb(0, 0, 0);

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
		public Color NormalBackgroundMiddleColor => normalBackgroundMiddleColor;



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
		public Color SelectedBackgroundStartColor => selectedBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SelectedBackgroundMiddleColor => selectedBackgroundMiddleColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SelectedBackgroundStopColor => selectedBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Color SelectedTextColor => selectedTextColor;
	}
}
