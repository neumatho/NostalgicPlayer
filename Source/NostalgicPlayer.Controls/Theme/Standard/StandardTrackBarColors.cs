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
	/// Standard colors for track bar
	/// </summary>
	internal class StandardTrackBarColors : ITrackBarColors
	{
		private readonly IButtonColors buttonColors = new StandardButtonColors();

		private static readonly Color backgroundColor = Color.FromArgb(240, 240, 240);

		private static readonly Color normalTrackBorderColor = Color.FromArgb(171, 186, 208);
		private static readonly Color normalTrackBackgroundColor = Color.FromArgb(225, 230, 238);
		private static readonly Color normalTrackFillStartColor = Color.FromArgb(117, 158, 220);
		private static readonly Color normalTrackFillStopColor = Color.FromArgb(73, 113, 185);
		private static readonly Color normalTickColor = Color.FromArgb(117, 158, 220);

		private static readonly Color disabledTrackBorderColor = Color.FromArgb(171, 186, 208);
		private static readonly Color disabledTrackBackgroundColor = Color.FromArgb(225, 230, 238);
		private static readonly Color disabledTrackFillStartColor = Color.FromArgb(200, 200, 200);
		private static readonly Color disabledTrackFillStopColor = Color.FromArgb(200, 200, 200);
		private static readonly Color disabledTickColor = Color.FromArgb(200, 200, 200);

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
		public Color NormalTrackBorderColor => normalTrackBorderColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalTrackBackgroundColor => normalTrackBackgroundColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalTrackFillStartColor => normalTrackFillStartColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalTrackFillStopColor => normalTrackFillStopColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalTickColor => normalTickColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalThumbBorderColor => buttonColors.NormalBorderColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalThumbBackgroundStartColor => buttonColors.NormalBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color NormalThumbBackgroundStopColor => buttonColors.NormalBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color HoverThumbBorderColor => buttonColors.HoverBorderColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color HoverThumbBackgroundStartColor => buttonColors.HoverBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color HoverThumbBackgroundStopColor => buttonColors.HoverBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color PressedThumbBorderColor => buttonColors.PressedBorderColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color PressedThumbBackgroundStartColor => buttonColors.PressedBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color PressedThumbBackgroundStopColor => buttonColors.PressedBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color FocusedThumbBorderColor => buttonColors.FocusedBorderColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color FocusedThumbBackgroundStartColor => buttonColors.FocusedBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color FocusedThumbBackgroundStopColor => buttonColors.FocusedBackgroundStopColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledTrackBorderColor => disabledTrackBorderColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledTrackBackgroundColor => disabledTrackBackgroundColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledTrackFillStartColor => disabledTrackFillStartColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledTrackFillStopColor => disabledTrackFillStopColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledTickColor => disabledTickColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledThumbBorderColor => buttonColors.DisabledBorderColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledThumbBackgroundStartColor => buttonColors.DisabledBackgroundStartColor;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public Color DisabledThumbBackgroundStopColor => buttonColors.DisabledBackgroundStopColor;
	}
}
