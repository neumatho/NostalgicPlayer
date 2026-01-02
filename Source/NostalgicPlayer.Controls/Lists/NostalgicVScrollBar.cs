/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Lists
{
	/// <summary>
	/// Custom vertical scrollbar with theme support
	/// </summary>
	public class NostalgicVScrollBar : NostalgicScrollBar, IThemeControl
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicVScrollBar()
		{
			Width = 17;
		}

		#region Public methods
		/********************************************************************/
		/// <summary>
		/// Calculates the scroll bar new value based on a mouse wheel event
		/// </summary>
		/********************************************************************/
		public void CalculateValueForMouseWheel(MouseEventArgs e)
		{
			int linesToScroll = SystemInformation.MouseWheelScrollLines;

			int delta = e.Delta / 120 * linesToScroll;
			int newValue = Value - delta;

			Value = Math.Max(Minimum, Math.Min(Maximum - LargeChange + 1, newValue));
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			CalculateValueForMouseWheel(e);

			OnScroll(new ScrollEventArgs(ScrollEventType.ThumbPosition, Value));

			base.OnMouseWheel(e);
		}



		/********************************************************************/
		/// <summary>
		/// Return the mouse position to use on mouse moving
		/// </summary>
		/********************************************************************/
		protected override int GetMousePosition(MouseEventArgs e)
		{
			return e.Y;
		}



		/********************************************************************/
		/// <summary>
		/// Return the start position of the rect
		/// </summary>
		/********************************************************************/
		protected override int GetStartPosition(Rectangle rect)
		{
			return rect.Y;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the track moving area size
		/// </summary>
		/********************************************************************/
		protected override int CalculateTrackMovingArea(Rectangle trackRect, Rectangle thumbRect)
		{
			return trackRect.Height - thumbRect.Height;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate all rectangles
		/// </summary>
		/********************************************************************/
		protected override (Rectangle DecrementArrow, Rectangle IncrementArrow, Rectangle Track) CalculateRectangles(int arrowSize)
		{
			return
			(
				new Rectangle(0, 0, Width, arrowSize),
				new Rectangle(0, Height - arrowSize, Width, arrowSize),
				new Rectangle(0, arrowSize, Width, Height - (arrowSize * 2))
			);
		}



		/********************************************************************/
		/// <summary>
		/// Find track position and size
		/// </summary>
		/********************************************************************/
		protected override (int TrackStartPosition, int TrackSize) GetTrackPositionAndSize(Rectangle trackRect)
		{
			return new
			(
				trackRect.Y,
				trackRect.Height
			);
		}



		/********************************************************************/
		/// <summary>
		/// Calculate new thumb rectangle
		/// </summary>
		/********************************************************************/
		protected override Rectangle CalculateThumbRectangle(Rectangle trackRect, int newStartPosition, int newSize)
		{
			return new Rectangle(trackRect.X, newStartPosition, trackRect.Width, newSize);
		}
		#endregion

		#region Drawing
		/********************************************************************/
		/// <summary>
		/// Draw a single arrow button
		/// </summary>
		/********************************************************************/
		protected override void DrawArrowButton(Graphics g, Rectangle rect, ArrowStateColors arrowColors, ArrowDirection direction)
		{
			int centerX = rect.X + (rect.Width / 2);
			int centerY = rect.Y + (rect.Height / 2);

			Point[] triangle;

			if (direction == ArrowDirection.Decrement)
			{
				triangle =
				[
					new Point(centerX, centerY - 3),
					new Point(centerX - 4, centerY + 2),
					new Point(centerX + 4, centerY + 2)
				];
			}
			else
			{
				triangle =
				[
					new Point(centerX - 4, centerY - 2),
					new Point(centerX + 4, centerY - 2),
					new Point(centerX, centerY + 3)
				];
			}

			using (SolidBrush brush = new SolidBrush(arrowColors.ArrowColor))
			{
				g.FillPolygon(brush, triangle);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Adjust the thumb rectangle
		/// </summary>
		/********************************************************************/
		protected override Rectangle AdjustThumbRect(Rectangle thumbRect)
		{
			return new Rectangle(thumbRect.X + 4, thumbRect.Y, thumbRect.Width - 8, thumbRect.Height);
		}
		#endregion
	}
}
