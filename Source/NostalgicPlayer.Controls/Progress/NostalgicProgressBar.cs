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

namespace Polycode.NostalgicPlayer.Controls.Progress
{
	/// <summary>
	/// Themed progress bar with custom rendering
	/// </summary>
	public class NostalgicProgressBar : Control, IThemeControl
	{
		private struct StateColors
		{
			public Color BorderColor { get; init; }
			public Color BackgroundStartColor { get; init; }
			public Color BackgroundStopColor { get; init; }
			public Color FillStartColor { get; init; }
			public Color FillStopColor { get; init; }
		}

		private IProgressBarColors colors;

		private int minimum = 0;
		private int maximum = 100;
		private int currentValue = 0;
		private Orientation orientation = Orientation.Horizontal;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicProgressBar()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
			SetStyle(ControlStyles.Selectable, false);

			Size = new Size(150, 14);
		}

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
		[DefaultValue(100)]
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
		[Description("The current value indicated by the filled portion of the bar")]
		public int Value
		{
			get => currentValue;

			set
			{
				int newValue = Math.Max(minimum, Math.Min(maximum, value));

				if (currentValue != newValue)
				{
					currentValue = newValue;

					Invalidate();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Progress bar orientation
		/// </summary>
		/********************************************************************/
		[DefaultValue(Orientation.Horizontal)]
		[Category("Behavior")]
		[Description("The orientation of the progress bar")]
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
			colors = theme.ProgressBarColors;

			Invalidate();
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override Size DefaultSize => new Size(150, 14);



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnEnabledChanged(EventArgs e)
		{
			Invalidate();

			base.OnEnabledChanged(e);
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

			StateColors stateColors = GetColors();

			DrawBar(g, stateColors);
		}
		#endregion

		#region Drawing
		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		private StateColors GetColors()
		{
			if (!Enabled)
			{
				return new StateColors
				{
					BorderColor = colors.DisabledBorderColor,
					BackgroundStartColor = colors.DisabledBackgroundStartColor,
					BackgroundStopColor = colors.DisabledBackgroundStopColor,
					FillStartColor = colors.DisabledFillStartColor,
					FillStopColor = colors.DisabledFillStopColor
				};
			}

			return new StateColors
			{
				BorderColor = colors.NormalBorderColor,
				BackgroundStartColor = colors.NormalBackgroundStartColor,
				BackgroundStopColor = colors.NormalBackgroundStopColor,
				FillStartColor = colors.NormalFillStartColor,
				FillStopColor = colors.NormalFillStopColor
			};
		}



		/********************************************************************/
		/// <summary>
		/// Draw the track background, the filled portion and the border
		/// </summary>
		/********************************************************************/
		private void DrawBar(Graphics g, StateColors stateColors)
		{
			Rectangle barRect = ClientRectangle;

			if ((barRect.Width <= 0) || (barRect.Height <= 0))
				return;

			Rectangle borderRect = new Rectangle(barRect.X, barRect.Y, barRect.Width - 1, barRect.Height - 1);

			using (LinearGradientBrush brush = new LinearGradientBrush(barRect, stateColors.BackgroundStartColor, stateColors.BackgroundStopColor, LinearGradientMode.Vertical))
			{
				g.FillRectangle(brush, barRect);
			}

			Rectangle fillRect = ComputeFillRect(barRect);

			if ((fillRect.Width > 0) && (fillRect.Height > 0))
			{
				// Gradient runs along the bar axis from min to max:
				// horizontal = left-to-right, vertical = bottom-to-top
				LinearGradientMode gradientMode;
				Color color1, color2;

				if (orientation == Orientation.Horizontal)
				{
					gradientMode = LinearGradientMode.Horizontal;
					color1 = stateColors.FillStartColor;
					color2 = stateColors.FillStopColor;
				}
				else
				{
					gradientMode = LinearGradientMode.Vertical;
					color1 = stateColors.FillStopColor;			// top = max
					color2 = stateColors.FillStartColor;		// bottom = min
				}

				using (LinearGradientBrush brush = new LinearGradientBrush(barRect, color1, color2, gradientMode))
				{
					g.FillRectangle(brush, fillRect);
				}
			}

			using (Pen pen = new Pen(stateColors.BorderColor))
			{
				g.DrawRectangle(pen, borderRect);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Compute the filled portion based on the current value
		/// </summary>
		/********************************************************************/
		private Rectangle ComputeFillRect(Rectangle barRect)
		{
			int range = maximum - minimum;

			if (range <= 0)
				return Rectangle.Empty;

			if (orientation == Orientation.Horizontal)
			{
				int fillWidth = (int)Math.Round((double)(currentValue - minimum) / range * barRect.Width);

				return new Rectangle(barRect.X, barRect.Y, fillWidth, barRect.Height);
			}
			else
			{
				int fillHeight = (int)Math.Round((double)(currentValue - minimum) / range * barRect.Height);

				return new Rectangle(barRect.X, barRect.Bottom - fillHeight, barRect.Width, fillHeight);
			}
		}
		#endregion

		#region Designer filtering (hide properties from PropertyGrid)
		/********************************************************************/
		/// <summary>
		/// Register a provider that filters properties for the designer
		/// </summary>
		/********************************************************************/
		static NostalgicProgressBar()
		{
			TypeDescriptor.AddProvider(new NostalgicProgressBarTypeDescriptionProvider(), typeof(NostalgicProgressBar));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer
		/// </summary>
		private sealed class NostalgicProgressBarTypeDescriptionProvider : TypeDescriptionProvider
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
				nameof(Text)
			];

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public NostalgicProgressBarTypeDescriptionProvider() : base(parent)
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
