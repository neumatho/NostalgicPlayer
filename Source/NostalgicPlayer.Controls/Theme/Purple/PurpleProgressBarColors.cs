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
	/// Purple theme colors for progress bar
	/// </summary>
	internal class PurpleProgressBarColors : IProgressBarColors
	{
		private readonly IButtonColors buttonColors = new PurpleButtonColors();

		private static readonly Color normalFillStartColor = Color.FromArgb(150, 120, 200);
		private static readonly Color normalFillStopColor = Color.FromArgb(95, 70, 145);

		private static readonly Color disabledFillStartColor = Color.FromArgb(200, 200, 200);
		private static readonly Color disabledFillStopColor = Color.FromArgb(200, 200, 200);

		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalBorderColor => buttonColors.NormalBorderColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalBackgroundStartColor => buttonColors.NormalBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalBackgroundStopColor => buttonColors.NormalBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalFillStartColor => normalFillStartColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalFillStopColor => normalFillStopColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledBorderColor => buttonColors.DisabledBorderColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledBackgroundStartColor => buttonColors.DisabledBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledBackgroundStopColor => buttonColors.DisabledBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledFillStartColor => disabledFillStartColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledFillStopColor => disabledFillStopColor;
	}
}
