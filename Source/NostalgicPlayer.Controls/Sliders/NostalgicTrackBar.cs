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
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;
using Polycode.NostalgicPlayer.Controls.Theme.Standard;

namespace Polycode.NostalgicPlayer.Controls.Sliders
{
	/// <summary>
	/// Themed track bar with custom rendering
	/// </summary>
	public class NostalgicTrackBar : Control, IThemeControl
	{
		private const int ThumbBreadth = 11;		// Size of thumb along slider axis
		private const int ThumbExtent = 20;			// Size of thumb perpendicular to slider axis
		private const int TrackThickness = 4;
		private const int TickAreaSize = 7;			// Space reserved for tick marks (line + small gap)
		private const int TickLineSize = 5;			// Length of a major tick line
		private const int TickMinorLineSize = 3;	// Length of an intermediate tick line
		private const int ThumbArrowSize = 5;		// Length of thumb's arrow tip
		private const int AxisPadding = 1;			// Padding at start/end of slider axis
		private const int PerpPadding = 2;			// Padding perpendicular to axis

		private struct LayoutInfo
		{
			public Rectangle ThumbAxisRect;			// Bounds in which the thumb travels (along axis) and its perpendicular extent
			public Rectangle ThumbRect;				// Current thumb rectangle
			public Rectangle TrackRect;				// Track full rectangle
			public Rectangle TrackFillRect;			// Track filled portion (from min to thumb)
			public Rectangle TopLeftTickArea;
			public Rectangle BottomRightTickArea;
		}

		private struct ThumbStateColors
		{
			public Color BorderColor { get; init; }
			public Color StartColor { get; init; }
			public Color StopColor { get; init; }
		}

		private ITrackBarColors colors;

		private int minimum = 0;
		private int maximum = 10;
		private int currentValue = 0;
		private int smallChange = 1;
		private int largeChange = 5;
		private int tickFrequency = 1;
		private TickStyle tickStyle = TickStyle.BottomRight;
		private Orientation orientation = Orientation.Horizontal;

		private bool isThumbHovered;
		private bool isThumbPressed;
		private bool isFocused;
		private int dragOffset;

		private Timer pageScrollTimer;
		private int pageScrollDirection;
		private Point lastMousePosition;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicTrackBar()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);

			Size = new Size(150, 45);

			pageScrollTimer = new Timer();
			pageScrollTimer.Interval = 100;
			pageScrollTimer.Tick += PageScrollTimer_Tick;
		}

		#region Events
		/********************************************************************/
		/// <summary>
		/// Occurs when the value changes
		/// </summary>
		/********************************************************************/
		[Category("Action")]
		[Description("Occurs when the value of the control changes")]
		public event EventHandler ValueChanged;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected virtual void OnValueChanged(EventArgs e)
		{
			if (ValueChanged != null)
				ValueChanged(this, e);
		}



		/********************************************************************/
		/// <summary>
		/// Occurs when the user moves the thumb
		/// </summary>
		/********************************************************************/
		[Category("Action")]
		[Description("Occurs when the user moves the thumb of the control")]
		public event EventHandler Scroll;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected virtual void OnScroll(EventArgs e)
		{
			if (Scroll != null)
				Scroll(this, e);
		}
		#endregion

		#region Designer properties
		/********************************************************************/
		/// <summary>
		/// Lower bound of the value range
		/// </summary>
		/********************************************************************/
		[DefaultValue(0)]
		[Category("Behavior")]
		[Description("The lower limit of the range")]
		public int Minimum
		{
			get => minimum;

			set
			{
				if (minimum != value)
				{
					minimum = value;

					if (maximum < minimum)
						maximum = minimum;

					if (currentValue < minimum)
						Value = minimum;
					else
						Invalidate();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Upper bound of the value range
		/// </summary>
		/********************************************************************/
		[DefaultValue(10)]
		[Category("Behavior")]
		[Description("The upper limit of the range")]
		public int Maximum
		{
			get => maximum;

			set
			{
				if (maximum != value)
				{
					maximum = value;

					if (minimum > maximum)
						minimum = maximum;

					if (currentValue > maximum)
						Value = maximum;
					else
						Invalidate();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// The current value
		/// </summary>
		/********************************************************************/
		[DefaultValue(0)]
		[Category("Behavior")]
		[Description("The current value indicated by the thumb")]
		public int Value
		{
			get => currentValue;

			set
			{
				int newValue = Math.Max(minimum, Math.Min(maximum, value));

				if (currentValue != newValue)
				{
					currentValue = newValue;

					OnValueChanged(EventArgs.Empty);

					Invalidate();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Amount the value changes when arrow keys or mouse wheel is used
		/// </summary>
		/********************************************************************/
		[DefaultValue(1)]
		[Category("Behavior")]
		[Description("The amount the value changes when arrow keys or mouse wheel is used")]
		public int SmallChange
		{
			get => smallChange;

			set
			{
				if (smallChange != value)
					smallChange = Math.Max(1, value);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Amount the value changes when clicking the track or pressing
		/// PageUp/PageDown
		/// </summary>
		/********************************************************************/
		[DefaultValue(5)]
		[Category("Behavior")]
		[Description("The amount the value changes when clicking the track or pressing PageUp/PageDown")]
		public int LargeChange
		{
			get => largeChange;

			set
			{
				if (largeChange != value)
					largeChange = Math.Max(1, value);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Distance between the tick marks
		/// </summary>
		/********************************************************************/
		[DefaultValue(1)]
		[Category("Appearance")]
		[Description("The distance between tick marks")]
		public int TickFrequency
		{
			get => tickFrequency;

			set
			{
				int newValue = Math.Max(1, value);

				if (tickFrequency != newValue)
				{
					tickFrequency = newValue;

					Invalidate();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Where the tick marks are drawn relative to the track. When set
		/// to None, the thumb is drawn as a rectangle
		/// </summary>
		/********************************************************************/
		[DefaultValue(TickStyle.BottomRight)]
		[Category("Appearance")]
		[Description("Where the tick marks are drawn. When set to None, the thumb is drawn as a rectangle")]
		public TickStyle TickStyle
		{
			get => tickStyle;

			set
			{
				if (tickStyle != value)
				{
					tickStyle = value;

					Invalidate();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Slider orientation
		/// </summary>
		/********************************************************************/
		[DefaultValue(Orientation.Horizontal)]
		[Category("Behavior")]
		[Description("The orientation of the track bar")]
		public Orientation Orientation
		{
			get => orientation;

			set
			{
				if (orientation != value)
				{
					orientation = value;

					Invalidate();
				}
			}
		}
		#endregion

		#region Initialize
		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				pageScrollTimer?.Dispose();
				pageScrollTimer = null;
			}

			base.Dispose(disposing);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the control to use custom rendering
		/// </summary>
		/********************************************************************/
		protected override void OnHandleCreated(EventArgs e)
		{
			if (DesignerHelper.IsInDesignMode(this))
				SetTheme(new StandardTheme());

			base.OnHandleCreated(e);
		}
		#endregion

		#region Theme
		/********************************************************************/
		/// <summary>
		/// Will setup the theme for the control
		/// </summary>
		/********************************************************************/
		public void SetTheme(ITheme theme)
		{
			colors = theme.TrackBarColors;

			Invalidate();
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override Size DefaultSize => new Size(150, 45);



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override bool IsInputKey(Keys keyData)
		{
			switch (keyData & Keys.KeyCode)
			{
				case Keys.Left:
				case Keys.Right:
				case Keys.Up:
				case Keys.Down:
				case Keys.PageUp:
				case Keys.PageDown:
				case Keys.Home:
				case Keys.End:
					return true;
			}

			return base.IsInputKey(keyData);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnEnabledChanged(EventArgs e)
		{
			if (!Enabled)
			{
				isThumbPressed = false;
				isThumbHovered = false;
				pageScrollTimer.Stop();
			}

			Invalidate();

			base.OnEnabledChanged(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnGotFocus(EventArgs e)
		{
			isFocused = true;

			Invalidate();

			base.OnGotFocus(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnLostFocus(EventArgs e)
		{
			isFocused = false;

			Invalidate();

			base.OnLostFocus(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (Enabled && (e.Button == MouseButtons.Left))
			{
				Focus();

				LayoutInfo layoutInfo = ComputeLayout();

				if (layoutInfo.ThumbRect.Contains(e.Location))
				{
					isThumbPressed = true;

					if (orientation == Orientation.Horizontal)
						dragOffset = e.X - layoutInfo.ThumbRect.X;
					else
						dragOffset = e.Y - layoutInfo.ThumbRect.Y;

					Invalidate();
				}
				else
				{
					int thumbCenter = orientation == Orientation.Horizontal ? layoutInfo.ThumbRect.X + (layoutInfo.ThumbRect.Width / 2) : layoutInfo.ThumbRect.Y + (layoutInfo.ThumbRect.Height / 2);
					int mouseAxis = orientation == Orientation.Horizontal ? e.X : e.Y;

					if (orientation == Orientation.Vertical)
						pageScrollDirection = mouseAxis < thumbCenter ? 1 : -1;
					else
						pageScrollDirection = mouseAxis < thumbCenter ? -1 : 1;

					lastMousePosition = e.Location;

					DoPageScroll();

					pageScrollTimer.Start();
				}
			}

			base.OnMouseDown(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (Enabled)
			{
				lastMousePosition = e.Location;

				if (isThumbPressed)
				{
					UpdateValueFromDrag(e.Location);
				}
				else if (!pageScrollTimer.Enabled)
				{
					LayoutInfo layoutInfo = ComputeLayout();
					bool nowHovered = layoutInfo.ThumbRect.Contains(e.Location);

					if (nowHovered != isThumbHovered)
					{
						isThumbHovered = nowHovered;

						Invalidate();
					}
				}
			}

			base.OnMouseMove(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				pageScrollTimer.Stop();

				if (isThumbPressed)
				{
					isThumbPressed = false;

					LayoutInfo layoutInfo = ComputeLayout();
					isThumbHovered = layoutInfo.ThumbRect.Contains(e.Location);

					Invalidate();
				}
			}

			base.OnMouseUp(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnMouseLeave(EventArgs e)
		{
			if (!isThumbPressed && isThumbHovered)
			{
				isThumbHovered = false;

				Invalidate();
			}

			base.OnMouseLeave(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			if (Enabled)
			{
				int delta = e.Delta / 120 * smallChange;
				int oldValue = currentValue;

				Value = currentValue + delta;

				if (currentValue != oldValue)
					OnScroll(EventArgs.Empty);
			}

			base.OnMouseWheel(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (Enabled)
			{
				int oldValue = currentValue;

				switch (e.KeyData & Keys.KeyCode)
				{
					case Keys.Left:
					case Keys.Down:
					{
						Value = currentValue - smallChange;
						break;
					}

					case Keys.Right:
					case Keys.Up:
					{
						Value = currentValue + smallChange;
						break;
					}

					case Keys.PageDown:
					{
						Value = currentValue - largeChange;
						break;
					}

					case Keys.PageUp:
					{
						Value = currentValue + largeChange;
						break;
					}

					case Keys.Home:
					{
						Value = minimum;
						break;
					}

					case Keys.End:
					{
						Value = maximum;
						break;
					}
				}

				if (currentValue != oldValue)
				{
					OnScroll(EventArgs.Empty);

					e.Handled = true;
				}
			}

			base.OnKeyDown(e);
		}



		/********************************************************************/
		/// <summary>
		/// All painting is done in OnPaint
		/// </summary>
		/********************************************************************/
		protected override void OnPaintBackground(PaintEventArgs e)
		{
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnPaint(PaintEventArgs e)
		{
			if (colors == null)
				return;

			Graphics g = e.Graphics;

			DrawBackground(g);

			LayoutInfo layoutInfo = ComputeLayout();

			g.SmoothingMode = SmoothingMode.AntiAlias;

			DrawTicks(g, layoutInfo);
			DrawTrack(g, layoutInfo);
			DrawThumb(g, layoutInfo);
			DrawFocus(g, layoutInfo);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Page scroll timer tick
		/// </summary>
		/********************************************************************/
		private void PageScrollTimer_Tick(object sender, EventArgs e)
		{
			DoPageScroll();
		}



		/********************************************************************/
		/// <summary>
		/// Move the thumb by LargeChange toward the click position. Stop
		/// when the thumb has reached or passed the click position
		/// </summary>
		/********************************************************************/
		private void DoPageScroll()
		{
			LayoutInfo layoutInfo = ComputeLayout();

			int thumbCenter = orientation == Orientation.Horizontal ? layoutInfo.ThumbRect.X + (layoutInfo.ThumbRect.Width / 2) : layoutInfo.ThumbRect.Y + (layoutInfo.ThumbRect.Height / 2);
			int mouseAxis = orientation == Orientation.Horizontal ? lastMousePosition.X : lastMousePosition.Y;

			bool stillNeedsToMove;

			if (orientation == Orientation.Horizontal)
				stillNeedsToMove = pageScrollDirection > 0 ? thumbCenter < mouseAxis : thumbCenter > mouseAxis;
			else
				stillNeedsToMove = pageScrollDirection > 0 ? thumbCenter > mouseAxis : thumbCenter < mouseAxis;

			if (!stillNeedsToMove)
			{
				pageScrollTimer.Stop();
				return;
			}

			int oldValue = currentValue;

			Value = currentValue + (pageScrollDirection * largeChange);

			if (currentValue != oldValue)
				OnScroll(EventArgs.Empty);
			else
				pageScrollTimer.Stop();
		}



		/********************************************************************/
		/// <summary>
		/// Update value while dragging the thumb
		/// </summary>
		/********************************************************************/
		private void UpdateValueFromDrag(Point mousePosition)
		{
			int range = maximum - minimum;

			if (range <= 0)
				return;

			LayoutInfo layoutInfo = ComputeLayout();
			Rectangle thumbAxisRect = layoutInfo.ThumbAxisRect;

			int newAxisPos;
			int travel;

			if (orientation == Orientation.Horizontal)
			{
				travel = thumbAxisRect.Width - ThumbBreadth;
				newAxisPos = Math.Max(thumbAxisRect.Left, Math.Min(thumbAxisRect.Right - ThumbBreadth, mousePosition.X - dragOffset));

				int relative = newAxisPos - thumbAxisRect.Left;
				int newValue = (travel <= 0) ? minimum : minimum + (int)Math.Round((double)relative / travel * range);

				int oldValue = currentValue;
				Value = newValue;

				if (currentValue != oldValue)
					OnScroll(EventArgs.Empty);
			}
			else
			{
				travel = thumbAxisRect.Height - ThumbBreadth;
				newAxisPos = Math.Max(thumbAxisRect.Top, Math.Min(thumbAxisRect.Bottom - ThumbBreadth, mousePosition.Y - dragOffset));

				int relative = newAxisPos - thumbAxisRect.Top;

				// Vertical: top = maximum, bottom = minimum
				int newValue = (travel <= 0) ? minimum : maximum - (int)Math.Round((double)relative / travel * range);

				int oldValue = currentValue;
				Value = newValue;

				if (currentValue != oldValue)
					OnScroll(EventArgs.Empty);
			}
		}
		#endregion

		#region Layout
		/********************************************************************/
		/// <summary>
		/// Compute layout rectangles for the current state
		/// </summary>
		/********************************************************************/
		private LayoutInfo ComputeLayout()
		{
			Rectangle client = ClientRectangle;

			LayoutInfo layoutInfo = new LayoutInfo();
			bool isHorizontal = orientation == Orientation.Horizontal;

			int axisStart = isHorizontal ? client.Left + AxisPadding : client.Top + AxisPadding;
			int axisEnd = isHorizontal ? client.Right - AxisPadding : client.Bottom - AxisPadding;
			int perpStart = isHorizontal ? client.Top + PerpPadding : client.Left + PerpPadding;
			int perpEnd = isHorizontal ? client.Bottom - PerpPadding : client.Right - PerpPadding;

			// Always reserve space for tick areas first, then size the thumb to fit in
			// the remainder. This keeps the layout symmetric between TopLeft and
			// BottomRight and prevents tick lines from being clipped on small controls
			bool hasTopLeftTicks = (tickStyle == TickStyle.TopLeft) || (tickStyle == TickStyle.Both);
			bool hasBottomRightTicks = (tickStyle == TickStyle.BottomRight) || (tickStyle == TickStyle.Both);

			int tickReservation = (hasTopLeftTicks ? TickAreaSize : 0) + (hasBottomRightTicks ? TickAreaSize : 0);

			int totalAvailable = perpEnd - perpStart;
			int actualThumbExtent = Math.Max(1, Math.Min(ThumbExtent, totalAvailable - tickReservation));

			int leadingPad = Math.Max(0, (totalAvailable - actualThumbExtent - tickReservation) / 2);

			int regionStart = perpStart + leadingPad;
			int topLeftTickStart = regionStart;
			int thumbPerpStart = regionStart + (hasTopLeftTicks ? TickAreaSize : 0);
			int thumbPerpEnd = thumbPerpStart + actualThumbExtent;
			int bottomRightTickStart = thumbPerpEnd;

			int axisLength = axisEnd - axisStart;

			if (isHorizontal)
				layoutInfo.ThumbAxisRect = new Rectangle(axisStart, thumbPerpStart, axisLength, thumbPerpEnd - thumbPerpStart);
			else
				layoutInfo.ThumbAxisRect = new Rectangle(thumbPerpStart, axisStart, thumbPerpEnd - thumbPerpStart, axisLength);

			int travel = axisLength - ThumbBreadth;
			int range = maximum - minimum;
			int relative;

			if ((travel <= 0) || (range <= 0))
				relative = 0;
			else
				relative = (int)Math.Round((double)(currentValue - minimum) / range * travel);

			int thumbAxisPos;

			if (isHorizontal)
				thumbAxisPos = axisStart + relative;
			else
				thumbAxisPos = axisEnd - ThumbBreadth - relative;	// vertical: top = max, bottom = min

			if (isHorizontal)
				layoutInfo.ThumbRect = new Rectangle(thumbAxisPos, thumbPerpStart, ThumbBreadth, thumbPerpEnd - thumbPerpStart);
			else
				layoutInfo.ThumbRect = new Rectangle(thumbPerpStart, thumbAxisPos, thumbPerpEnd - thumbPerpStart, ThumbBreadth);

			int trackPerpCenter = (thumbPerpStart + thumbPerpEnd) / 2;
			int trackPerpStart = trackPerpCenter - (TrackThickness / 2);

			int thumbAxisCenter = thumbAxisPos + (ThumbBreadth / 2);

			if (isHorizontal)
			{
				layoutInfo.TrackRect = new Rectangle(axisStart, trackPerpStart, axisLength, TrackThickness);
				layoutInfo.TrackFillRect = new Rectangle(axisStart, trackPerpStart, Math.Max(0, thumbAxisCenter - axisStart), TrackThickness);
			}
			else
			{
				layoutInfo.TrackRect = new Rectangle(trackPerpStart, axisStart, TrackThickness, axisLength);

				// Vertical fill: from thumb center down to bottom (minimum side)
				int fillStart = thumbAxisCenter;
				int fillEnd = axisEnd;
				layoutInfo.TrackFillRect = new Rectangle(trackPerpStart, fillStart, TrackThickness, Math.Max(0, fillEnd - fillStart));
			}

			if (hasTopLeftTicks)
			{
				if (isHorizontal)
					layoutInfo.TopLeftTickArea = new Rectangle(axisStart, topLeftTickStart, axisLength, TickAreaSize);
				else
					layoutInfo.TopLeftTickArea = new Rectangle(topLeftTickStart, axisStart, TickAreaSize, axisLength);
			}

			if (hasBottomRightTicks)
			{
				if (isHorizontal)
					layoutInfo.BottomRightTickArea = new Rectangle(axisStart, bottomRightTickStart, axisLength, TickAreaSize);
				else
					layoutInfo.BottomRightTickArea = new Rectangle(bottomRightTickStart, axisStart, TickAreaSize, axisLength);
			}

			return layoutInfo;
		}
		#endregion

		#region Drawing
		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		private ThumbStateColors GetThumbColors()
		{
			if (!Enabled)
			{
				return new ThumbStateColors
				{
					BorderColor = colors.DisabledThumbBorderColor,
					StartColor = colors.DisabledThumbBackgroundStartColor,
					StopColor = colors.DisabledThumbBackgroundStopColor
				};
			}

			if (isThumbPressed)
			{
				return new ThumbStateColors
				{
					BorderColor = colors.PressedThumbBorderColor,
					StartColor = colors.PressedThumbBackgroundStartColor,
					StopColor = colors.PressedThumbBackgroundStopColor
				};
			}

			if (isThumbHovered)
			{
				return new ThumbStateColors
				{
					BorderColor = colors.HoverThumbBorderColor,
					StartColor = colors.HoverThumbBackgroundStartColor,
					StopColor = colors.HoverThumbBackgroundStopColor
				};
			}

			if (isFocused)
			{
				return new ThumbStateColors
				{
					BorderColor = colors.FocusedThumbBorderColor,
					StartColor = colors.FocusedThumbBackgroundStartColor,
					StopColor = colors.FocusedThumbBackgroundStopColor
				};
			}

			return new ThumbStateColors
			{
				BorderColor = colors.NormalThumbBorderColor,
				StartColor = colors.NormalThumbBackgroundStartColor,
				StopColor = colors.NormalThumbBackgroundStopColor
			};
		}



		/********************************************************************/
		/// <summary>
		/// Draw the control background
		/// </summary>
		/********************************************************************/
		private void DrawBackground(Graphics g)
		{
			using (SolidBrush brush = new SolidBrush(colors.BackgroundColor))
			{
				g.FillRectangle(brush, ClientRectangle);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw track background plus the filled portion up to the thumb
		/// </summary>
		/********************************************************************/
		private void DrawTrack(Graphics g, LayoutInfo layoutInfo)
		{
			Rectangle trackRect = layoutInfo.TrackRect;

			if ((trackRect.Width <= 0) || (trackRect.Height <= 0))
				return;

			Rectangle borderRect = new Rectangle(trackRect.X, trackRect.Y, trackRect.Width - 1, trackRect.Height - 1);

			using (SolidBrush brush = new SolidBrush(colors.TrackBackgroundColor))
			{
				g.FillRectangle(brush, trackRect);
			}

			Rectangle fillRect = layoutInfo.TrackFillRect;

			if ((fillRect.Width > 0) && (fillRect.Height > 0))
			{
				if (Enabled)
				{
					LinearGradientMode gradientMode = orientation == Orientation.Horizontal ? LinearGradientMode.Vertical : LinearGradientMode.Horizontal;

					using (LinearGradientBrush brush = new LinearGradientBrush(trackRect, colors.TrackFillStartColor, colors.TrackFillStopColor, gradientMode))
					{
						g.FillRectangle(brush, fillRect);
					}
				}
				else
				{
					using (SolidBrush brush = new SolidBrush(colors.TrackDisabledFillColor))
					{
						g.FillRectangle(brush, fillRect);
					}
				}
			}

			using (Pen pen = new Pen(colors.TrackBorderColor))
			{
				g.DrawRectangle(pen, borderRect);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the tick marks
		/// </summary>
		/********************************************************************/
		private void DrawTicks(Graphics g, LayoutInfo layoutInfo)
		{
			if (tickStyle == TickStyle.None)
				return;

			Color tickColor = Enabled ? colors.TickColor : colors.DisabledTickColor;
			int range = maximum - minimum;

			if (range <= 0)
				return;

			Rectangle thumbAxisRect = layoutInfo.ThumbAxisRect;
			bool isHorizontal = orientation == Orientation.Horizontal;
			int axisStart = isHorizontal ? thumbAxisRect.Left : thumbAxisRect.Top;
			int axisEnd = isHorizontal ? thumbAxisRect.Right : thumbAxisRect.Bottom;
			int travel = (axisEnd - axisStart) - ThumbBreadth;

			if (travel <= 0)
				return;

			using (Pen pen = new Pen(tickColor))
			{
				int freq = Math.Max(1, tickFrequency);

				for (int v = 0; v <= range; v += freq)
				{
					int actualValue = minimum + v;
					bool isEnd = (v == 0) || (actualValue == maximum);
					int lineLength = isEnd ? TickLineSize : TickMinorLineSize;

					int relative = (int)Math.Round((double)v / range * travel);
					int axisPos;

					if (isHorizontal)
						axisPos = axisStart + relative + (ThumbBreadth / 2);
					else
						axisPos = axisEnd - relative - (ThumbBreadth / 2);	// top = max, bottom = min

					DrawSingleTick(g, pen, layoutInfo, axisPos, lineLength, isHorizontal);
				}

				// Make sure the maximum end tick is always drawn even if range is not divisible by frequency
				if ((range % freq) != 0)
				{
					int axisPos;

					if (isHorizontal)
						axisPos = axisStart + travel + (ThumbBreadth / 2);
					else
						axisPos = axisStart + (ThumbBreadth / 2);

					DrawSingleTick(g, pen, layoutInfo, axisPos, TickLineSize, isHorizontal);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		private void DrawSingleTick(Graphics g, Pen pen, LayoutInfo layoutInfo, int axisPos, int lineLength, bool isHorizontal)
		{
			if ((tickStyle == TickStyle.TopLeft) || (tickStyle == TickStyle.Both))
			{
				Rectangle area = layoutInfo.TopLeftTickArea;

				if (isHorizontal)
				{
					int yEnd = area.Bottom - 1;
					int yStart = yEnd - lineLength + 1;

					g.DrawLine(pen, axisPos, yStart, axisPos, yEnd);
				}
				else
				{
					int xEnd = area.Right - 1;
					int xStart = xEnd - lineLength + 1;

					g.DrawLine(pen, xStart, axisPos, xEnd, axisPos);
				}
			}

			if ((tickStyle == TickStyle.BottomRight) || (tickStyle == TickStyle.Both))
			{
				Rectangle area = layoutInfo.BottomRightTickArea;

				if (isHorizontal)
				{
					int yStart = area.Top;
					int yEnd = yStart + lineLength - 1;

					g.DrawLine(pen, axisPos, yStart, axisPos, yEnd);
				}
				else
				{
					int xStart = area.Left;
					int xEnd = xStart + lineLength - 1;

					g.DrawLine(pen, xStart, axisPos, xEnd, axisPos);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the thumb
		/// </summary>
		/********************************************************************/
		private void DrawThumb(Graphics g, LayoutInfo layoutInfo)
		{
			Rectangle thumbRect = layoutInfo.ThumbRect;

			if ((thumbRect.Width <= 0) || (thumbRect.Height <= 0))
				return;

			ThumbStateColors thumbColors = GetThumbColors();

			using (GraphicsPath path = CreateThumbPath(thumbRect))
			{
				LinearGradientMode gradientMode = orientation == Orientation.Horizontal ? LinearGradientMode.Vertical : LinearGradientMode.Horizontal;

				using (LinearGradientBrush brush = new LinearGradientBrush(thumbRect, thumbColors.StartColor, thumbColors.StopColor, gradientMode))
				{
					g.FillPath(brush, path);
				}

				using (Pen pen = new Pen(thumbColors.BorderColor))
				{
					g.DrawPath(pen, path);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Build a graphics path for the thumb based on the current
		/// TickStyle and Orientation
		/// </summary>
		/********************************************************************/
		private GraphicsPath CreateThumbPath(Rectangle rect)
		{
			GraphicsPath path = new GraphicsPath();

			// Use rect.Width-1 / rect.Height-1 so the bounding box matches what
			// DrawPath expects (pen draws on the right/bottom edge)
			int left = rect.Left;
			int right = rect.Right - 1;
			int top = rect.Top;
			int bottom = rect.Bottom - 1;

			bool isHorizontal = orientation == Orientation.Horizontal;
			bool arrowFirst = (tickStyle == TickStyle.TopLeft) || (tickStyle == TickStyle.Both);
			bool arrowSecond = (tickStyle == TickStyle.BottomRight) || (tickStyle == TickStyle.Both);

			if (tickStyle == TickStyle.None)
			{
				path.AddRectangle(new Rectangle(left, top, right - left, bottom - top));
				path.CloseFigure();

				return path;
			}

			if (isHorizontal)
			{
				int centerX = left + ((right - left) / 2);
				int arrowTopY = top + ThumbArrowSize;
				int arrowBottomY = bottom - ThumbArrowSize;

				if (arrowFirst && !arrowSecond)
				{
					// Arrow pointing up
					path.AddPolygon(new Point[]
					{
						new Point(centerX, top),
						new Point(right, arrowTopY),
						new Point(right, bottom),
						new Point(left, bottom),
						new Point(left, arrowTopY)
					});
				}
				else if (!arrowFirst && arrowSecond)
				{
					// Arrow pointing down
					path.AddPolygon(new Point[]
					{
						new Point(left, top),
						new Point(right, top),
						new Point(right, arrowBottomY),
						new Point(centerX, bottom),
						new Point(left, arrowBottomY)
					});
				}
				else
				{
					// Both arrows
					path.AddPolygon(new Point[]
					{
						new Point(centerX, top),
						new Point(right, arrowTopY),
						new Point(right, arrowBottomY),
						new Point(centerX, bottom),
						new Point(left, arrowBottomY),
						new Point(left, arrowTopY)
					});
				}
			}
			else
			{
				int centerY = top + ((bottom - top) / 2);
				int arrowLeftX = left + ThumbArrowSize;
				int arrowRightX = right - ThumbArrowSize;

				if (arrowFirst && !arrowSecond)
				{
					// Arrow pointing left
					path.AddPolygon(new Point[]
					{
						new Point(arrowLeftX, top),
						new Point(right, top),
						new Point(right, bottom),
						new Point(arrowLeftX, bottom),
						new Point(left, centerY)
					});
				}
				else if (!arrowFirst && arrowSecond)
				{
					// Arrow pointing right
					path.AddPolygon(new Point[]
					{
						new Point(left, top),
						new Point(arrowRightX, top),
						new Point(right, centerY),
						new Point(arrowRightX, bottom),
						new Point(left, bottom)
					});
				}
				else
				{
					// Both arrows
					path.AddPolygon(new Point[]
					{
						new Point(arrowLeftX, top),
						new Point(arrowRightX, top),
						new Point(right, centerY),
						new Point(arrowRightX, bottom),
						new Point(arrowLeftX, bottom),
						new Point(left, centerY)
					});
				}
			}

			path.CloseFigure();

			return path;
		}



		/********************************************************************/
		/// <summary>
		/// Draw the focus rectangle around the thumb
		/// </summary>
		/********************************************************************/
		private void DrawFocus(Graphics g, LayoutInfo layoutInfo)
		{
			if (Focused && ShowFocusCues && Enabled)
			{
				Rectangle focusRect = Rectangle.Inflate(layoutInfo.ThumbRect, 1, 1);
				ControlPaint.DrawFocusRectangle(g, focusRect);
			}
		}
		#endregion

		#region Designer filtering (hide properties from PropertyGrid)
		/********************************************************************/
		/// <summary>
		/// Register a provider that filters properties for the designer
		/// </summary>
		/********************************************************************/
		static NostalgicTrackBar()
		{
			TypeDescriptor.AddProvider(new NostalgicTrackBarTypeDescriptionProvider(), typeof(NostalgicTrackBar));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer.
		/// This is needed for properties that we cannot override, but we do
		/// it for all our hidden properties to be sure
		/// </summary>
		private sealed class NostalgicTrackBarTypeDescriptionProvider : TypeDescriptionProvider
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
			public NostalgicTrackBarTypeDescriptionProvider() : base(parent)
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
