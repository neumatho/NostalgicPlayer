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
using Polycode.NostalgicPlayer.Controls.Containers;
using Polycode.NostalgicPlayer.Controls.Designer;
using Polycode.NostalgicPlayer.Controls.Theme;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Buttons
{
	/// <summary>
	/// Themed button with custom rendering
	/// </summary>
	public class NostalgicButton : Button, IThemeControl, IFontConfiguration
	{
		private const int CornerRadius = 3;

		private struct StateColors
		{
			public Color BorderColor { get; init; }
			public Color BackgroundStartColor { get; init; }
			public Color BackgroundStopColor { get; init; }
			public Color TextColor { get; init; }
		}

		private IButtonColors colors;
		private IFonts fonts;

		private FontConfiguration fontConfiguration;

		private bool isHovered;
		private bool isPressed;			// True while mouse down or space-bar held
		private bool isSpacePressed;	// Track if space currently holds the pressed state

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicButton()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
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
				fontConfiguration = value;

				Invalidate();
			}
		}
		#endregion

		#region Initialize
		/********************************************************************/
		/// <summary>
		/// Initialize the form to use custom rendering
		/// </summary>
		/********************************************************************/
		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			if (DesignMode)
				SetTheme(ThemeManagerFactory.GetThemeManager().CurrentTheme);
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
			colors = theme.ButtonColors;
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
			isSpacePressed = false;
			isPressed = false;
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
		/// Paint the whole control
		/// </summary>
		/********************************************************************/
		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;

			g.SmoothingMode = SmoothingMode.AntiAlias;

			StateColors stateColors = GetColors();

			ClearBackground(g);
			DrawBackground(g, ClientRectangle, stateColors);
			DrawText(g, ClientRectangle, stateColors);
			DrawFocus(g, ClientRectangle);
		}



		/********************************************************************/
		/// <summary>
		/// Keep region updated when size changes
		/// </summary>
		/********************************************************************/
		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			UpdateButtonRegion();
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Update the Region to reflect rounded corners
		/// </summary>
		/********************************************************************/
		private void UpdateButtonRegion()
		{
			if (!IsHandleCreated)
				return;

			using (GraphicsPath path = CreateRoundRectanglePath(new Rectangle(0, 0, Width - 1, Height - 1)))
			{
				Region = new Region(path);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Create a path for a rounded rectangle
		/// </summary>
		/********************************************************************/
		private GraphicsPath CreateRoundRectanglePath(Rectangle rect)
		{
			GraphicsPath path = new GraphicsPath();

			int d = CornerRadius * 2;
			int right = rect.Right;
			int bottom = rect.Bottom;

			path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
			path.AddArc(right - d, rect.Top, d, d, 270, 90);
			path.AddArc(right - d, bottom - d, d, d, 0, 90);
			path.AddArc(rect.Left, bottom - d, d, d, 90, 90);

			path.CloseFigure();

			return path;
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
					TextColor = colors.HoverTextColor
				};
			}

			if (Focused)
			{
				return new StateColors
				{
					BorderColor = colors.FocusedBorderColor,
					BackgroundStartColor = colors.FocusedBackgroundStartColor,
					BackgroundStopColor = colors.FocusedBackgroundStopColor,
					TextColor = colors.FocusedTextColor
				};
			}

			return new StateColors
			{
				BorderColor = colors.NormalBorderColor,
				BackgroundStartColor = colors.NormalBackgroundStartColor,
				BackgroundStopColor = colors.NormalBackgroundStopColor,
				TextColor = colors.NormalTextColor
			};
		}



		/********************************************************************/
		/// <summary>
		/// Clear the background with the parent background to avoid
		/// artifacts in the corners
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
		/// Draw the background and border
		/// </summary>
		/********************************************************************/
		private void DrawBackground(Graphics g, Rectangle rect, StateColors stateColors)
		{
			Rectangle outerRect = new Rectangle(rect.X, rect.Y, rect.Width - 1, rect.Height - 1);

			using (GraphicsPath path = CreateRoundRectanglePath(outerRect))
			{
				using (LinearGradientBrush brush = new LinearGradientBrush(outerRect, stateColors.BackgroundStartColor, stateColors.BackgroundStopColor, LinearGradientMode.Vertical))
				{
					g.FillPath(brush, path);
				}

				using (Pen p = new Pen(stateColors.BorderColor))
				{
					g.DrawPath(p, path);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the button text
		/// </summary>
		/********************************************************************/
		private void DrawText(Graphics g, Rectangle rect, StateColors stateColors)
		{
			Font font = fontConfiguration?.Font ?? fonts.RegularFont;

			int y = ((rect.Height - font.Height) / 2) - 1;

			TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;
			Rectangle drawRect = new Rectangle(rect.X, rect.Y + y, rect.Width, Font.Height);
			TextRenderer.DrawText(g, Text, font, drawRect, stateColors.TextColor, flags);
		}



		/********************************************************************/
		/// <summary>
		/// Draw the focus rectangle
		/// </summary>
		/********************************************************************/
		private void DrawFocus(Graphics g, Rectangle rect)
		{
			if (Focused && ShowFocusCues && Enabled)
			{
				Rectangle focusRect = Rectangle.Inflate(rect, -3, -3);
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
		static NostalgicButton()
		{
			TypeDescriptor.AddProvider(new NostalgicButtonTypeDescriptionProvider(), typeof(NostalgicButton));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer.
		/// This is needed for properties that we cannot override, but we do
		/// it for all our hidden properties to be sure
		/// </summary>
		private sealed class NostalgicButtonTypeDescriptionProvider : TypeDescriptionProvider
		{
			private static readonly TypeDescriptionProvider parent = TypeDescriptor.GetProvider(typeof(Button));

			private static readonly string[] propertiesToHide =
			[
				nameof(FlatStyle),
				nameof(BackColor),
				nameof(BackgroundImage),
				nameof(BackgroundImageLayout),
				nameof(Image),
				nameof(ImageAlign),
				nameof(ImageIndex),
				nameof(ImageKey),
				nameof(ImageList),
				nameof(FlatAppearance),
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
			public NostalgicButtonTypeDescriptionProvider() : base(parent)
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
