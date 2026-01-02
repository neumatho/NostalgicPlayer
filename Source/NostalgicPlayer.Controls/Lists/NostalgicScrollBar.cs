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
using Polycode.NostalgicPlayer.Controls.Theme;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Lists
{
	/// <summary>
	/// Base class with common functionality to both scroll bar types
	/// </summary>
	public abstract class NostalgicScrollBar : Control
	{
		private const int ArrowButtonSize = 17;
		private const int MinThumbSize = 30;

		private IScrollBarColors colors;

		private int minimum;
		private int maximum = 100;
		private int value;
		private int largeChange = 10;
		private int smallChange = 1;

		private Rectangle decrementArrowRect;
		private Rectangle incrementArrowRect;
		private Rectangle trackRect;
		private Rectangle thumbRect;

		private ScrollBarState decrementArrowState = ScrollBarState.Normal;
		private ScrollBarState incrementArrowState = ScrollBarState.Normal;
		private ScrollBarState thumbState = ScrollBarState.Normal;

		private bool decrementArrowPressed;
		private bool incrementArrowPressed;
		private bool thumbPressed;
		private int thumbDragOffset;

		private Timer scrollTimer;

		private enum ScrollBarState
		{
			Normal,
			Hover,
			Pressed,
			Disabled
		}

		/// <summary>
		/// 
		/// </summary>
		protected enum ArrowDirection
		{
			/// <summary>
			/// Top/Left
			/// </summary>
			Decrement,

			/// <summary>
			/// Bottom/Right
			/// </summary>
			Increment
		}

		/// <summary>
		/// 
		/// </summary>
		protected struct ArrowStateColors
		{
			/// <summary>
			/// 
			/// </summary>
			public Color ArrowColor { get; set; }
		}

		/// <summary>
		/// 
		/// </summary>
		private struct ThumbStateColors
		{
			/// <summary>
			/// 
			/// </summary>
			public Color ThumbColor { get; set; }
		}

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected NostalgicScrollBar()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

			scrollTimer = new Timer();
			scrollTimer.Interval = 50;
			scrollTimer.Tick += ScrollTimer_Tick;
		}

		#region Events
		/********************************************************************/
		/// <summary>
		/// 
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
		/// 
		/// </summary>
		/********************************************************************/
		[Category("Action")]
		[Description("Occurs when the user moves the scroll box")]
		public event ScrollEventHandler Scroll;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected virtual void OnScroll(ScrollEventArgs e)
		{
			if (Scroll != null)
				Scroll(this, e);
		}
		#endregion

		#region Designer properties
		/********************************************************************/
		/// <summary>
		/// Gets or sets the minimum value
		/// </summary>
		/********************************************************************/
		[DefaultValue(0)]
		[Category("Behavior")]
		[Description("The lower limit value of the scrollable range")]
		public int Minimum
		{
			get => minimum;

			set
			{
				if (minimum != value)
				{
					minimum = value;

					if (this.value < minimum)
						Value = minimum;

					UpdateThumb();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets or sets the maximum value
		/// </summary>
		/********************************************************************/
		[DefaultValue(100)]
		[Category("Behavior")]
		[Description("The upper limit value of the scrollable range")]
		public int Maximum
		{
			get => maximum;

			set
			{
				if (maximum != value)
				{
					maximum = value;

					if (this.value > maximum)
						Value = maximum;

					UpdateThumb();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets or sets the current value
		/// </summary>
		/********************************************************************/
		[DefaultValue(0)]
		[Category("Behavior")]
		[Description("The value that the scroll box position represents")]
		public int Value
		{
			get => value;

			set
			{
				int newValue = Math.Max(minimum, Math.Min(maximum - largeChange + 1, value));

				if (this.value != newValue)
				{
					this.value = newValue;

					UpdateThumb();

					OnValueChanged(EventArgs.Empty);

					Invalidate();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets or sets the large change value
		/// </summary>
		/********************************************************************/
		[DefaultValue(10)]
		[Category("Behavior")]
		[Description("The amount by which the scroll box position changes when the user clicks in the scroll bar or presses the PAGE UP or PAGE DOWN keys")]
		public int LargeChange
		{
			get => largeChange;

			set
			{
				if (largeChange != value)
				{
					largeChange = Math.Max(1, value);

					UpdateThumb();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets or sets the small change value
		/// </summary>
		/********************************************************************/
		[DefaultValue(1)]
		[Category("Behavior")]
		[Description("The amount by which the scroll box position changes when the user clicks a scroll arrow or presses an arrow key")]
		public int SmallChange
		{
			get => smallChange;

			set
			{
				if (smallChange != value)
					smallChange = Math.Max(1, value);
			}
		}
		#endregion

		#region Initialize
		/********************************************************************/
		/// <summary>
		/// Dispose different stuff
		/// </summary>
		/********************************************************************/
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				scrollTimer?.Dispose();
				scrollTimer = null;
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
			if (DesignMode)
				SetTheme(ThemeManagerFactory.GetThemeManager().CurrentTheme);

			UpdateRectangles();

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
			colors = theme.ScrollBarColors;

			Invalidate();
		}
		#endregion

		#region Handlers
		/********************************************************************/
		/// <summary>
		/// Timer tick for continuous scrolling
		/// </summary>
		/********************************************************************/
		private void ScrollTimer_Tick(object sender, EventArgs e)
		{
			if (decrementArrowPressed)
				DecrementScroll();
			else if (incrementArrowPressed)
				IncrementScroll();
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void OnSizeChanged(EventArgs e)
		{
			UpdateRectangles();

			base.OnSizeChanged(e);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void OnEnabledChanged(EventArgs e)
		{
			decrementArrowState = Enabled ? ScrollBarState.Normal : ScrollBarState.Disabled;
			incrementArrowState = Enabled ? ScrollBarState.Normal : ScrollBarState.Disabled;
			thumbState = Enabled ? ScrollBarState.Normal : ScrollBarState.Disabled;

			Invalidate();

			base.OnEnabledChanged(e);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (!Enabled)
				return;

			if (decrementArrowRect.Contains(e.Location))
			{
				decrementArrowPressed = true;
				decrementArrowState = ScrollBarState.Pressed;

				DecrementScroll();
				scrollTimer.Start();

				Invalidate(decrementArrowRect);
			}
			else if (incrementArrowRect.Contains(e.Location))
			{
				incrementArrowPressed = true;
				incrementArrowState = ScrollBarState.Pressed;

				IncrementScroll();
				scrollTimer.Start();

				Invalidate(incrementArrowRect);
			}
			else if (thumbRect.Contains(e.Location))
			{
				thumbPressed = true;
				thumbState = ScrollBarState.Pressed;

				thumbDragOffset = GetMousePosition(e) - GetStartPosition(thumbRect);

				Invalidate(thumbRect);
			}
			else if (trackRect.Contains(e.Location))
			{
				if (GetMousePosition(e) < GetStartPosition(thumbRect))
					Value -= largeChange;
				else
					Value += largeChange;
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
			if (!Enabled)
				return;

			if (thumbPressed)
			{
				int newPosition = GetMousePosition(e) - thumbDragOffset;
				int range = maximum - minimum - largeChange + 1;
				int trackSize = CalculateTrackMovingArea(trackRect, thumbRect);

				if ((trackSize > 0) && (range > 0))
				{
					int relative = Math.Max(0, Math.Min(trackSize, newPosition - GetStartPosition(trackRect)));
					int newValue = minimum + (int)((float)relative / trackSize * range);

					Value = newValue;

					OnScroll(new ScrollEventArgs(ScrollEventType.ThumbPosition, Value));
				}
			}
			else
				UpdateHoverState(e.Location);

			base.OnMouseMove(e);
		}



		/********************************************************************/
		/// <summary>
		/// Return the mouse position to use on mouse moving
		/// </summary>
		/********************************************************************/
		protected abstract int GetMousePosition(MouseEventArgs e);



		/********************************************************************/
		/// <summary>
		/// Return the start position of the rect
		/// </summary>
		/********************************************************************/
		protected abstract int GetStartPosition(Rectangle rect);



		/********************************************************************/
		/// <summary>
		/// Calculate the track moving area size
		/// </summary>
		/********************************************************************/
		protected abstract int CalculateTrackMovingArea(Rectangle trackRect, Rectangle thumbRect);



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void OnMouseUp(MouseEventArgs e)
		{
			scrollTimer.Stop();

			if (decrementArrowPressed)
			{
				decrementArrowPressed = false;
				decrementArrowState = decrementArrowRect.Contains(e.Location) ? ScrollBarState.Hover : ScrollBarState.Normal;

				Invalidate(decrementArrowRect);
			}

			if (incrementArrowPressed)
			{
				incrementArrowPressed = false;
				incrementArrowState = incrementArrowRect.Contains(e.Location) ? ScrollBarState.Hover : ScrollBarState.Normal;

				Invalidate(incrementArrowRect);
			}

			if (thumbPressed)
			{
				thumbPressed = false;
				thumbState = thumbRect.Contains(e.Location) ? ScrollBarState.Hover : ScrollBarState.Normal;

				Invalidate(thumbRect);
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
			if (!thumbPressed)
			{
				bool needsRedraw = false;

				if (decrementArrowState == ScrollBarState.Hover)
				{
					decrementArrowState = ScrollBarState.Normal;
					needsRedraw = true;
				}

				if (incrementArrowState == ScrollBarState.Hover)
				{
					incrementArrowState = ScrollBarState.Normal;
					needsRedraw = true;
				}

				if (thumbState == ScrollBarState.Hover)
				{
					thumbState = ScrollBarState.Normal;
					needsRedraw = true;
				}

				if (needsRedraw)
					Invalidate();
			}

			base.OnMouseLeave(e);
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
			g.SmoothingMode = SmoothingMode.AntiAlias;

			DrawBackground(g);
			DrawArrows(g);
			DrawThumb(g);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Scroll up by small change
		/// </summary>
		/********************************************************************/
		private void DecrementScroll()
		{
			Value -= smallChange;

			OnScroll(new ScrollEventArgs(ScrollEventType.ThumbPosition, Value));
		}



		/********************************************************************/
		/// <summary>
		/// Scroll down by small change
		/// </summary>
		/********************************************************************/
		private void IncrementScroll()
		{
			Value += smallChange;

			OnScroll(new ScrollEventArgs(ScrollEventType.ThumbPosition, Value));
		}



		/********************************************************************/
		/// <summary>
		/// Update hover state based on mouse position
		/// </summary>
		/********************************************************************/
		private void UpdateHoverState(Point location)
		{
			ScrollBarState newTopState = decrementArrowRect.Contains(location) ? ScrollBarState.Hover : ScrollBarState.Normal;
			ScrollBarState newBottomState = incrementArrowRect.Contains(location) ? ScrollBarState.Hover : ScrollBarState.Normal;
			ScrollBarState newThumbState = thumbRect.Contains(location) ? ScrollBarState.Hover : ScrollBarState.Normal;

			bool needsRedraw = false;

			if (decrementArrowState != newTopState)
			{
				decrementArrowState = newTopState;
				needsRedraw = true;
			}

			if (incrementArrowState != newBottomState)
			{
				incrementArrowState = newBottomState;
				needsRedraw = true;
			}

			if (thumbState != newThumbState)
			{
				thumbState = newThumbState;
				needsRedraw = true;
			}

			if (needsRedraw)
				Invalidate();
		}



		/********************************************************************/
		/// <summary>
		/// Update all rectangles
		/// </summary>
		/********************************************************************/
		private void UpdateRectangles()
		{
			var rects = CalculateRectangles(ArrowButtonSize);

			decrementArrowRect = rects.DecrementArrow;
			incrementArrowRect = rects.IncrementArrow;
			trackRect = rects.Track;

			UpdateThumb();
		}



		/********************************************************************/
		/// <summary>
		/// Calculate all rectangles
		/// </summary>
		/********************************************************************/
		protected abstract (Rectangle DecrementArrow, Rectangle IncrementArrow, Rectangle Track) CalculateRectangles(int arrowSize);



		/********************************************************************/
		/// <summary>
		/// Update thumb rectangle
		/// </summary>
		/********************************************************************/
		private void UpdateThumb()
		{
			int range = maximum - minimum - largeChange + 1;

			if (range <= 0)
			{
				thumbRect = Rectangle.Empty;
				return;
			}

			var trackPosAndSize = GetTrackPositionAndSize(trackRect);

			int trackSize = trackPosAndSize.TrackSize;
			int thumbSize = Math.Max(MinThumbSize, (int)((float)largeChange / (maximum - minimum + 1) * trackSize));
			int availableSize = trackSize - thumbSize;

			int thumbPos = trackPosAndSize.TrackStartPosition + (int)((float)(value - minimum) / range * availableSize);
			thumbRect = CalculateThumbRectangle(trackRect, thumbPos, thumbSize);
		}



		/********************************************************************/
		/// <summary>
		/// Find track position and size
		/// </summary>
		/********************************************************************/
		protected abstract (int TrackStartPosition, int TrackSize) GetTrackPositionAndSize(Rectangle trackRect);



		/********************************************************************/
		/// <summary>
		/// Calculate new thumb rectangle
		/// </summary>
		/********************************************************************/
		protected abstract Rectangle CalculateThumbRectangle(Rectangle trackRect, int newStartPosition, int newSize);
		#endregion

		#region Drawing
		/********************************************************************/
		/// <summary>
		/// Return the colors to use for the arrows in the current state
		/// </summary>
		/********************************************************************/
		private ArrowStateColors GetArrowColors(ScrollBarState state)
		{
			if (state == ScrollBarState.Disabled)
			{
				return new ArrowStateColors
				{
					ArrowColor = colors.DisabledArrowColor
				};
			}

			if (state == ScrollBarState.Pressed)
			{
				return new ArrowStateColors
				{
					ArrowColor = colors.PressedArrowColor
				};
			}

			if (state == ScrollBarState.Hover)
			{
				return new ArrowStateColors
				{
					ArrowColor = colors.HoverArrowColor
				};
			}

			return new ArrowStateColors
			{
				ArrowColor = colors.NormalArrowColor
			};
		}



		/********************************************************************/
		/// <summary>
		/// Return the colors to use for the thumb in the current state
		/// </summary>
		/********************************************************************/
		private ThumbStateColors GetThumbColors()
		{
			if (thumbState == ScrollBarState.Pressed)
			{
				return new ThumbStateColors
				{
					ThumbColor = colors.PressedThumbColor
				};
			}

			if (thumbState == ScrollBarState.Hover)
			{
				return new ThumbStateColors
				{
					ThumbColor = colors.HoverThumbColor
				};
			}

			return new ThumbStateColors
			{
				ThumbColor = colors.NormalThumbColor
			};
		}



		/********************************************************************/
		/// <summary>
		/// Draw the background
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
		/// Draw the arrow buttons
		/// </summary>
		/********************************************************************/
		private void DrawArrows(Graphics g)
		{
			DrawArrowButton(g, decrementArrowRect, GetArrowColors(decrementArrowState), ArrowDirection.Decrement);
			DrawArrowButton(g, incrementArrowRect, GetArrowColors(incrementArrowState), ArrowDirection.Increment);
		}



		/********************************************************************/
		/// <summary>
		/// Draw a single arrow button
		/// </summary>
		/********************************************************************/
		protected abstract void DrawArrowButton(Graphics g, Rectangle rect, ArrowStateColors arrowColors, ArrowDirection direction);



		/********************************************************************/
		/// <summary>
		/// Draw the thumb
		/// </summary>
		/********************************************************************/
		private void DrawThumb(Graphics g)
		{
			if (thumbRect.IsEmpty || !Enabled)
				return;

			ThumbStateColors thumbColors = GetThumbColors();

			using (SolidBrush brush = new SolidBrush(thumbColors.ThumbColor))
			{
				g.FillRectangle(brush, AdjustThumbRect(thumbRect));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Adjust the thumb rectangle
		/// </summary>
		/********************************************************************/
		protected abstract Rectangle AdjustThumbRect(Rectangle thumbRect);
		#endregion
	}
}
