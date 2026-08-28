/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Lists
{
	/// <summary>
	/// Custom horizontal scrollbar with theme support
	/// </summary>
	public class NostalgicHScrollBar : NostalgicScrollBar, IThemeControl
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicHScrollBar()
		{
			Height = 17;
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			int delta = e.Delta / 120 * SmallChange;
			Value -= delta;

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
			return e.X;
		}



		/********************************************************************/
		/// <summary>
		/// Return the start position of the rect
		/// </summary>
		/********************************************************************/
		protected override int GetStartPosition(Rectangle rect)
		{
			return rect.X;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the track moving area size
		/// </summary>
		/********************************************************************/
		protected override int CalculateTrackMovingArea(Rectangle trackRect, Rectangle thumbRect)
		{
			return trackRect.Width - thumbRect.Width;
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
				new Rectangle(0, 0, arrowSize, Height),
				new Rectangle(Width - arrowSize, 0, arrowSize, Height),
				new Rectangle(arrowSize, 0, Width - (arrowSize * 2), Height)
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
				trackRect.X,
				trackRect.Width
			);
		}



		/********************************************************************/
		/// <summary>
		/// Calculate new thumb rectangle
		/// </summary>
		/********************************************************************/
		protected override Rectangle CalculateThumbRectangle(Rectangle trackRect, int newStartPosition, int newSize)
		{
			return new Rectangle(newStartPosition, trackRect.Y, newSize, trackRect.Height);
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
					new Point(centerX - 3, centerY),
					new Point(centerX + 2, centerY - 4),
					new Point(centerX + 2, centerY + 4)
				];
			}
			else
			{
				triangle =
				[
					new Point(centerX - 2, centerY - 4),
					new Point(centerX - 2, centerY + 4),
					new Point(centerX + 3, centerY)
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
			return new Rectangle(thumbRect.X, thumbRect.Y + 4, thumbRect.Width, thumbRect.Height - 8);
		}
		#endregion
	}
}
