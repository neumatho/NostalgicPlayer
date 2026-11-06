/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Designer;

namespace Polycode.NostalgicPlayer.Controls.Sliders
{
	/// <summary>
	/// Volume bar variant of NostalgicTrackBar where the track is
	/// rendered as a 90 degree right triangle with the right angle at the
	/// maximum end
	/// </summary>
	public class NostalgicVolumeBar : NostalgicTrackBar
	{
		#region Drawing
		/********************************************************************/
		/// <summary>
		/// Draw the wedge-shaped track plus the filled portion up to the
		/// thumb position
		/// </summary>
		/********************************************************************/
		protected override void DrawTrack(Graphics g, StateColors stateColors, LayoutInfo layoutInfo)
		{
			Rectangle thumbAxisRect = layoutInfo.ThumbAxisRect;

			if ((thumbAxisRect.Width <= 0) || (thumbAxisRect.Height <= 0))
				return;

			bool isHorizontal = Orientation == Orientation.Horizontal;

			// Shrink the wedge in the perpendicular direction so that only the
			// rectangular portion of the thumb (excluding any arrow tips) needs
			// to cover it at max
			bool hasTopLeftArrow = (TickStyle == TickStyle.TopLeft) || (TickStyle == TickStyle.Both);
			bool hasBottomRightArrow = (TickStyle == TickStyle.BottomRight) || (TickStyle == TickStyle.Both);
			int leadingShrink = hasTopLeftArrow ? ThumbArrowSize : 0;
			int trailingShrink = hasBottomRightArrow ? ThumbArrowSize : 0;

			Rectangle wedgeRect = isHorizontal
				? new Rectangle(thumbAxisRect.X, thumbAxisRect.Y + leadingShrink, thumbAxisRect.Width, thumbAxisRect.Height - leadingShrink - trailingShrink)
				: new Rectangle(thumbAxisRect.X + leadingShrink, thumbAxisRect.Y, thumbAxisRect.Width - leadingShrink - trailingShrink, thumbAxisRect.Height);

			if ((wedgeRect.Width <= 0) || (wedgeRect.Height <= 0))
				return;

			// Inset by 1 px so the border pen draws inside the wedge bounds
			int left = wedgeRect.Left;
			int right = wedgeRect.Right - 1;
			int top = wedgeRect.Top;
			int bottom = wedgeRect.Bottom - 1;

			PointF[] wedgePoints = isHorizontal
				?
				[
					new PointF(left, top),		// Apex (min)
					new PointF(right, top),		// Right angle (max)
					new PointF(right, bottom)	// Max with full height
				]
				:
				[
					new PointF(left, bottom),	// Apex (min)
					new PointF(left, top),		// Right angle (max)
					new PointF(right, top)		// Max with full width
				];

			using (GraphicsPath wedgePath = new GraphicsPath())
			{
				wedgePath.AddPolygon(wedgePoints);
				wedgePath.CloseFigure();

				using (SolidBrush brush = new SolidBrush(stateColors.TrackBackgroundColor))
				{
					g.FillPath(brush, wedgePath);
				}

				PointF[] fillPoints = ComputeFillTriangle(layoutInfo, isHorizontal, left, right, top, bottom);

				if (fillPoints != null)
				{
					using (GraphicsPath fillPath = new GraphicsPath())
					{
						fillPath.AddPolygon(fillPoints);
						fillPath.CloseFigure();

						// Gradient runs along the slider axis from min to max:
						// horizontal = left-to-right, vertical = bottom-to-top
						LinearGradientMode gradientMode;
						Color color1, color2;

						if (isHorizontal)
						{
							gradientMode = LinearGradientMode.Horizontal;
							color1 = stateColors.TrackFillStartColor;
							color2 = stateColors.TrackFillStopColor;
						}
						else
						{
							gradientMode = LinearGradientMode.Vertical;
							color1 = stateColors.TrackFillStopColor;		// top = max
							color2 = stateColors.TrackFillStartColor;		// bottom = min
						}

						using (LinearGradientBrush brush = new LinearGradientBrush(wedgeRect, color1, color2, gradientMode))
						{
							g.FillPath(brush, fillPath);
						}
					}
				}

				using (Pen pen = new Pen(stateColors.TrackBorderColor))
				{
					g.DrawPath(pen, wedgePath);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Build the similar right triangle representing the filled portion
		/// of the wedge, from the apex to the thumb center along the axis
		/// </summary>
		/********************************************************************/
		private static PointF[] ComputeFillTriangle(LayoutInfo layoutInfo, bool isHorizontal, int left, int right, int top, int bottom)
		{
			if ((right <= left) || (bottom <= top))
				return null;

			Rectangle thumbRect = layoutInfo.ThumbRect;

			if (isHorizontal)
			{
				float thumbCenter = thumbRect.X + (thumbRect.Width / 2.0f);

				if (thumbCenter <= left)
					return null;

				if (thumbCenter > right)
					thumbCenter = right;

				// Hypotenuse runs from (left, top) to (right, bottom)
				float hypY = top + ((bottom - top) * (thumbCenter - left) / (right - left));

				return
				[
					new PointF(left, top),
					new PointF(thumbCenter, top),
					new PointF(thumbCenter, hypY)
				];
			}
			else
			{
				float thumbCenter = thumbRect.Y + (thumbRect.Height / 2.0f);

				if (thumbCenter >= bottom)
					return null;

				if (thumbCenter < top)
					thumbCenter = top;

				// Hypotenuse runs from (left, bottom) to (right, top)
				float hypX = left + ((right - left) * (bottom - thumbCenter) / (bottom - top));

				return
				[
					new PointF(left, bottom),
					new PointF(left, thumbCenter),
					new PointF(hypX, thumbCenter)
				];
			}
		}
		#endregion

		#region Designer filtering (hide properties from PropertyGrid)
		/********************************************************************/
		/// <summary>
		/// Register a provider that filters properties for the designer
		/// </summary>
		/********************************************************************/
		static NostalgicVolumeBar()
		{
			TypeDescriptor.AddProvider(new NostalgicVolumeBarTypeDescriptionProvider(), typeof(NostalgicVolumeBar));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer
		/// </summary>
		private sealed class NostalgicVolumeBarTypeDescriptionProvider : TypeDescriptionProvider
		{
			private static readonly TypeDescriptionProvider parent = TypeDescriptor.GetProvider(typeof(Control));

			private static readonly string[] propertiesToHide =
			[
				nameof(FlatStyle),
				nameof(BackColor),
				nameof(BackgroundImage),
				nameof(BackgroundImageLayout),
				nameof(Font),
				nameof(ForeColor),
				nameof(RightToLeft),
				nameof(DrawMode)
			];

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public NostalgicVolumeBarTypeDescriptionProvider() : base(parent)
			{
			}



			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
			{
				return new HidingTypeDescriptor(base.GetTypeDescriptor(objectType, instance), propertiesToHide);
			}
		}
		#endregion
	}
}
