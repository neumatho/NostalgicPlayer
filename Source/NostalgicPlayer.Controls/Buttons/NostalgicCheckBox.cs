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
	/// Themed check box with custom rendering
	/// </summary>
	public class NostalgicCheckBox : CheckBox, IThemeControl, IFontConfiguration
	{
		// Gap between the box and the text
		private const int TextGap = 2;

		private struct StateColors
		{
			public Color BorderColor { get; init; }
			public Color BackgroundStartColor { get; init; }
			public Color BackgroundStopColor { get; init; }
			public Color CheckMarkColor { get; init; }
			public Color TextColor { get; init; }
		}

		private ICheckBoxColors colors;
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
		public NostalgicCheckBox()
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
			colors = theme.CheckBoxColors;
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
		protected override void OnCheckStateChanged(EventArgs e)
		{
			Invalidate();

			base.OnCheckStateChanged(e);
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

			Rectangle boxRect = GetBoxRectangle(rect, font);

			ClearBackground(g);
			DrawBox(g, boxRect, stateColors);
			DrawCheckMark(g, boxRect, stateColors);
			DrawText(g, rect, boxRect, font, stateColors, out Rectangle textRect);
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
		/// Return the bounds of the check box glyph, vertically centered
		/// against the text
		/// </summary>
		/********************************************************************/
		private Rectangle GetBoxRectangle(Rectangle rect, Font font)
		{
			int boxSize = Math.Max(13, font.Height - 2);
			boxSize = Math.Min(boxSize, rect.Height - 1);

			int y = rect.Y + ((rect.Height - boxSize) / 2);

			return new Rectangle(rect.X, y, boxSize, boxSize);
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
					CheckMarkColor = colors.DisabledCheckMarkColor,
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
					CheckMarkColor = colors.PressedCheckMarkColor,
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
					CheckMarkColor = colors.HoverCheckMarkColor,
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
					CheckMarkColor = colors.FocusedCheckMarkColor,
					TextColor = colors.FocusedTextColor
				};
			}

			return new StateColors
			{
				BorderColor = colors.NormalBorderColor,
				BackgroundStartColor = colors.NormalBackgroundStartColor,
				BackgroundStopColor = colors.NormalBackgroundStopColor,
				CheckMarkColor = colors.NormalCheckMarkColor,
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
		/// Draw the check box glyph background and border
		/// </summary>
		/********************************************************************/
		private void DrawBox(Graphics g, Rectangle boxRect, StateColors stateColors)
		{
			Rectangle outerRect = new Rectangle(boxRect.X, boxRect.Y, boxRect.Width - 1, boxRect.Height - 1);

			// The border is a sharp rectangle, so keep the edges crisp
			SmoothingMode oldMode = g.SmoothingMode;
			g.SmoothingMode = SmoothingMode.None;

			using (LinearGradientBrush brush = new LinearGradientBrush(outerRect, stateColors.BackgroundStartColor, stateColors.BackgroundStopColor, LinearGradientMode.Vertical))
			{
				g.FillRectangle(brush, outerRect);
			}

			using (Pen p = new Pen(stateColors.BorderColor))
			{
				g.DrawRectangle(p, outerRect);
			}

			g.SmoothingMode = oldMode;
		}



		/********************************************************************/
		/// <summary>
		/// Draw the check mark or the indeterminate marker, depending on the
		/// current check state
		/// </summary>
		/********************************************************************/
		private void DrawCheckMark(Graphics g, Rectangle boxRect, StateColors stateColors)
		{
			if (CheckState == CheckState.Checked)
			{
				// Keep the mark one pixel narrower, so it does not touch the right border
				int markWidth = boxRect.Width - 1;

				PointF p1 = new PointF(boxRect.Left + (markWidth * 0.22f), boxRect.Top + (boxRect.Height * 0.52f));
				PointF p2 = new PointF(boxRect.Left + (markWidth * 0.42f), boxRect.Top + (boxRect.Height * 0.72f));
				PointF p3 = new PointF(boxRect.Left + (markWidth * 0.78f), boxRect.Top + (boxRect.Height * 0.28f));

				float penWidth = Math.Max(2.0f, boxRect.Width / 7.0f);

				using (Pen p = new Pen(stateColors.CheckMarkColor, penWidth))
				{
					p.StartCap = LineCap.Round;
					p.EndCap = LineCap.Round;
					p.LineJoin = LineJoin.Round;

					g.DrawLines(p, [ p1, p2, p3 ]);
				}
			}
			else if (CheckState == CheckState.Indeterminate)
			{
				int inset = (int)Math.Round(boxRect.Width * 0.28f);
				Rectangle fillRect = Rectangle.Inflate(boxRect, -inset, -inset);

				using (SolidBrush brush = new SolidBrush(stateColors.CheckMarkColor))
				{
					g.FillRectangle(brush, fillRect);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the label text and report the rectangle it was drawn into
		/// </summary>
		/********************************************************************/
		private void DrawText(Graphics g, Rectangle rect, Rectangle boxRect, Font font, StateColors stateColors, out Rectangle textRect)
		{
			int x = boxRect.Right + TextGap;

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
		static NostalgicCheckBox()
		{
			TypeDescriptor.AddProvider(new NostalgicCheckBoxTypeDescriptionProvider(), typeof(NostalgicCheckBox));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer.
		/// This is needed for properties that we cannot override, but we do
		/// it for all our hidden properties to be sure
		/// </summary>
		private sealed class NostalgicCheckBoxTypeDescriptionProvider : TypeDescriptionProvider
		{
			private static readonly TypeDescriptionProvider parent = TypeDescriptor.GetProvider(typeof(CheckBox));

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
			public NostalgicCheckBoxTypeDescriptionProvider() : base(parent)
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
