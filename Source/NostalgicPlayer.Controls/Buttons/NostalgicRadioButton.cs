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
using Polycode.NostalgicPlayer.Controls.Components;
using Polycode.NostalgicPlayer.Controls.Designer;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;
using Polycode.NostalgicPlayer.Controls.Theme.Standard;

namespace Polycode.NostalgicPlayer.Controls.Buttons
{
	/// <summary>
	/// Themed radio button with custom rendering
	/// </summary>
	public class NostalgicRadioButton : RadioButton, IThemeControl, IFontConfiguration
	{
		// Gap between the circle and the text
		private const int TextGap = 2;

		private struct StateColors
		{
			public Color BorderColor { get; init; }
			public Color BackgroundStartColor { get; init; }
			public Color BackgroundStopColor { get; init; }
			public Color DotColor { get; init; }
			public Color TextColor { get; init; }
		}

		private IRadioButtonColors colors;
		private IFonts fonts;

		private FontConfiguration fontConfiguration;

		private bool isHovered;
		private bool isPressed;			// True while mouse down or space-bar held
		private bool isSpacePressed;	// Track if space currently holds the pressed state
		private bool isFocused;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicRadioButton()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw, true);
		}

		#region Designer properties
		/********************************************************************/
		/// <summary>
		/// Set the FontConfiguration component to use for this control
		/// </summary>
		/********************************************************************/
		[Category("Appearance")]
		[Description("Which font configuration to use if you want to change the default font.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(null)]
		public FontConfiguration UseFont
		{
			get => fontConfiguration;

			set
			{
				if (fontConfiguration != null)
					fontConfiguration.FontChanged -= FontConfiguration_FontChanged;

				fontConfiguration = value;

				if (fontConfiguration != null)
					fontConfiguration.FontChanged += FontConfiguration_FontChanged;

				Invalidate();
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
			colors = theme.RadioButtonColors;
			fonts = theme.StandardFonts;

			Invalidate();
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnMouseEnter(EventArgs e)
		{
			isHovered = true;
			Invalidate();

			base.OnMouseEnter(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnMouseLeave(EventArgs e)
		{
			isHovered = false;
			Invalidate();

			base.OnMouseLeave(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				isPressed = true;
				Invalidate();
			}

			base.OnMouseDown(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (isPressed && !isSpacePressed)
			{
				isPressed = false;
				Invalidate();
			}

			base.OnMouseUp(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnMouseMove(MouseEventArgs e)
		{
			// While captured (mouse button down) Windows keeps sending us mouse moves even outside.
			// We must manually track hover state when pressed
			bool inside = ClientRectangle.Contains(e.Location);

			if (inside)
			{
				if (!isHovered)
				{
					isHovered = true;
					Invalidate();
				}
				else if (!isPressed && (e.Button == MouseButtons.Left) && Focused)
				{
					isPressed = true;
					Invalidate();
				}
			}
			else
			{
				if (isPressed)
				{
					isPressed = false;
					Invalidate();
				}
			}

			base.OnMouseMove(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnMouseCaptureChanged(EventArgs e)
		{
			// If capture is lost unexpectedly while pressed, normalize state
			if (!Capture && isPressed && !isSpacePressed)
			{
				isPressed = false;
				Invalidate();
			}

			base.OnMouseCaptureChanged(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnEnter(EventArgs e)
		{
			isFocused = true;
			Invalidate();

			base.OnEnter(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnLeave(EventArgs e)
		{
			isFocused = false;
			isSpacePressed = false;
			isPressed = false;
			Invalidate();

			base.OnLeave(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnGotFocus(EventArgs e)
		{
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
			Invalidate();

			base.OnLostFocus(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if ((e.KeyCode == Keys.Space) && !isSpacePressed)
			{
				isSpacePressed = true;
				isPressed = true;
				Invalidate();
			}

			base.OnKeyDown(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnKeyUp(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Space && isSpacePressed)
			{
				isSpacePressed = false;
				isPressed = false;
				Invalidate();
			}

			base.OnKeyUp(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnCheckedChanged(EventArgs e)
		{
			Invalidate();

			base.OnCheckedChanged(e);
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
				isSpacePressed = false;
				isPressed = false;
			}

			Invalidate();

			base.OnEnabledChanged(e);
		}



		/********************************************************************/
		/// <summary>
		/// Don't do anything, we have all painting in OnPaint
		/// </summary>
		/********************************************************************/
		protected override void OnPaintBackground(PaintEventArgs e)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Paint the whole control
		/// </summary>
		/********************************************************************/
		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;

			g.SmoothingMode = SmoothingMode.AntiAlias;

			Rectangle rect = ClientRectangle;
			Font font = GetFont();
			StateColors stateColors = GetColors();

			Rectangle circleRect = GetCircleRectangle(rect, font);

			ClearBackground(g);
			DrawCircle(g, circleRect, stateColors);
			DrawDot(g, circleRect, stateColors);
			DrawText(g, rect, circleRect, font, stateColors, out Rectangle textRect);
			DrawFocus(g, font, textRect);
		}
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// React when the attached FontConfiguration recalculates its font
		/// (e.g. theme manager just initialized, or one of FontType /
		/// FontStyle / FontSize changed at runtime)
		/// </summary>
		/********************************************************************/
		private void FontConfiguration_FontChanged(object sender, EventArgs e)
		{
			Invalidate();
		}
		#endregion

		#region Drawing
		/********************************************************************/
		/// <summary>
		/// Return the font to use
		/// </summary>
		/********************************************************************/
		private Font GetFont()
		{
			return fontConfiguration?.Font ?? fonts.RegularFont;
		}



		/********************************************************************/
		/// <summary>
		/// Return the bounds of the radio circle glyph, vertically centered
		/// against the text
		/// </summary>
		/********************************************************************/
		private Rectangle GetCircleRectangle(Rectangle rect, Font font)
		{
			int circleSize = Math.Max(13, font.Height - 2);
			circleSize = Math.Min(circleSize, rect.Height - 1);

			int y = rect.Y + ((rect.Height - circleSize) / 2);

			return new Rectangle(rect.X, y, circleSize, circleSize);
		}



		/********************************************************************/
		/// <summary>
		/// Return the colors to use for the current state
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
					DotColor = colors.DisabledDotColor,
					TextColor = colors.DisabledTextColor
				};
			}

			if (isPressed)
			{
				return new StateColors
				{
					BorderColor = colors.PressedBorderColor,
					BackgroundStartColor = colors.PressedBackgroundStartColor,
					BackgroundStopColor = colors.PressedBackgroundStopColor,
					DotColor = colors.PressedDotColor,
					TextColor = colors.PressedTextColor
				};
			}

			if (isHovered)
			{
				return new StateColors
				{
					BorderColor = colors.HoverBorderColor,
					BackgroundStartColor = colors.HoverBackgroundStartColor,
					BackgroundStopColor = colors.HoverBackgroundStopColor,
					DotColor = colors.HoverDotColor,
					TextColor = colors.HoverTextColor
				};
			}

			if (isFocused)
			{
				return new StateColors
				{
					BorderColor = colors.FocusedBorderColor,
					BackgroundStartColor = colors.FocusedBackgroundStartColor,
					BackgroundStopColor = colors.FocusedBackgroundStopColor,
					DotColor = colors.FocusedDotColor,
					TextColor = colors.FocusedTextColor
				};
			}

			return new StateColors
			{
				BorderColor = colors.NormalBorderColor,
				BackgroundStartColor = colors.NormalBackgroundStartColor,
				BackgroundStopColor = colors.NormalBackgroundStopColor,
				DotColor = colors.NormalDotColor,
				TextColor = colors.NormalTextColor
			};
		}



		/********************************************************************/
		/// <summary>
		/// Clear the background with the parent background, so the control
		/// blends in with whatever sits behind it
		/// </summary>
		/********************************************************************/
		private void ClearBackground(Graphics g)
		{
			if (Parent != null)
			{
				GraphicsState state = g.Save();

				try
				{
					// Shift so we paint the part of the parent that sits "under" us
					g.TranslateTransform(-Left, -Top);

					Rectangle parentRect = new Rectangle(Point.Empty, Parent.ClientSize);

					using (PaintEventArgs e = new PaintEventArgs(g, parentRect))
					{
						// Ask parent to paint its background + foreground (foreground needed if it draws custom stuff behind children)
						InvokePaintBackground(Parent, e);
						InvokePaint(Parent, e);
					}
				}
				finally
				{
					g.Restore(state);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the radio circle glyph background and border
		/// </summary>
		/********************************************************************/
		private void DrawCircle(Graphics g, Rectangle circleRect, StateColors stateColors)
		{
			Rectangle outerRect = new Rectangle(circleRect.X, circleRect.Y, circleRect.Width - 1, circleRect.Height - 1);

			using (LinearGradientBrush brush = new LinearGradientBrush(outerRect, stateColors.BackgroundStartColor, stateColors.BackgroundStopColor, LinearGradientMode.Vertical))
			{
				g.FillEllipse(brush, outerRect);
			}

			using (Pen p = new Pen(stateColors.BorderColor))
			{
				g.DrawEllipse(p, outerRect);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the filled dot when the radio button is checked
		/// </summary>
		/********************************************************************/
		private void DrawDot(Graphics g, Rectangle circleRect, StateColors stateColors)
		{
			if (Checked)
			{
				// Center the dot on the same ellipse DrawCircle fills, so the two are concentric
				Rectangle outerRect = new Rectangle(circleRect.X, circleRect.Y, circleRect.Width - 1, circleRect.Height - 1);

				float diameter = outerRect.Width * 0.5f;
				float x = outerRect.X + ((outerRect.Width - diameter) / 2.0f);
				float y = outerRect.Y + ((outerRect.Height - diameter) / 2.0f);

				using (SolidBrush brush = new SolidBrush(stateColors.DotColor))
				{
					g.FillEllipse(brush, x, y, diameter, diameter);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the label text and report the rectangle it was drawn into
		/// </summary>
		/********************************************************************/
		private void DrawText(Graphics g, Rectangle rect, Rectangle circleRect, Font font, StateColors stateColors, out Rectangle textRect)
		{
			int x = circleRect.Right + TextGap;

			textRect = new Rectangle(x, rect.Y, rect.Width - x, rect.Height);

			TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;
			TextRenderer.DrawText(g, Text, font, textRect, stateColors.TextColor, flags);
		}



		/********************************************************************/
		/// <summary>
		/// Draw the focus rectangle around the text
		/// </summary>
		/********************************************************************/
		private void DrawFocus(Graphics g, Font font, Rectangle textRect)
		{
			if (Focused && ShowFocusCues && Enabled)
			{
				Size textSize = TextRenderer.MeasureText(g, Text, font, textRect.Size, TextFormatFlags.SingleLine);

				int height = Math.Min(textSize.Height, textRect.Height);
				int y = textRect.Y + ((textRect.Height - height) / 2);

				Rectangle focusRect = new Rectangle(textRect.X - 1, y, Math.Min(textSize.Width, textRect.Width) + 2, height);

				// Keep the focus rectangle inside the control, so the right edge is not clipped
				// when the text fills the available width
				focusRect.Intersect(ClientRectangle);

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
		static NostalgicRadioButton()
		{
			TypeDescriptor.AddProvider(new NostalgicRadioButtonTypeDescriptionProvider(), typeof(NostalgicRadioButton));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer.
		/// This is needed for properties that we cannot override, but we do
		/// it for all our hidden properties to be sure
		/// </summary>
		private sealed class NostalgicRadioButtonTypeDescriptionProvider : TypeDescriptionProvider
		{
			private static readonly TypeDescriptionProvider parent = TypeDescriptor.GetProvider(typeof(RadioButton));

			private static readonly string[] propertiesToHide =
			[
				nameof(Appearance),
				nameof(FlatStyle),
				nameof(FlatAppearance),
				nameof(BackColor),
				nameof(BackgroundImage),
				nameof(BackgroundImageLayout),
				nameof(CheckAlign),
				nameof(Image),
				nameof(ImageAlign),
				nameof(ImageIndex),
				nameof(ImageKey),
				nameof(ImageList),
				nameof(Font),
				nameof(ForeColor),
				nameof(RightToLeft),
				nameof(TextAlign),
				nameof(TextImageRelation),
				nameof(UseVisualStyleBackColor),
				nameof(UseCompatibleTextRendering),
			];

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public NostalgicRadioButtonTypeDescriptionProvider() : base(parent)
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
