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
	/// Standard colors for progress bar
	/// </summary>
	internal class StandardProgressBarColors : IProgressBarColors
	{
		private readonly IButtonColors buttonColors = new StandardButtonColors();

		private static readonly Color normalFillStartColor = Color.FromArgb(140, 210, 110);
		private static readonly Color normalFillStopColor = Color.FromArgb(70, 150, 50);

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
