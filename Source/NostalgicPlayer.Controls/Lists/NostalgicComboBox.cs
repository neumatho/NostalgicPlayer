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

namespace Polycode.NostalgicPlayer.Controls.Lists
{
	/// <summary>
	/// Themed combo box with custom rendering
	/// </summary>
	public class NostalgicComboBox : ComboBox, IThemeControl, IFontConfiguration
	{
		private const int PaddingBetweenTextAndButton = 10;
		private const int DropDownButtonWidth = 16;

		private struct StateColors
		{
			public Color BorderColor { get; init; }
			public Color BackgroundColor { get; init; }
			public Color TextColor { get; init; }
			public Color ArrowButtonBorderColor { get; init; }
			public Color ArrowButtonBackgroundStartColor { get; init; }
			public Color ArrowButtonBackgroundStopColor { get; init; }
			public Color ArrowColor { get; init; }
		}

		private struct DropDownStateColors
		{
			public Color BackgroundStartColor { get; init; }
			public Color BackgroundMiddleColor { get; init; }
			public Color BackgroundStopColor { get; init; }
			public Color TextColor { get; init; }
		}

		private IComboBoxColors colors;
		private IFonts fonts;

		private FontConfiguration fontConfiguration;

		private bool isHovered;
		private bool isButtonHovered;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicComboBox()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
			DrawMode = DrawMode.OwnerDrawFixed;

			// Force DropDownStyle to DropDownList for custom rendering
			DropDownStyle = ComboBoxStyle.DropDownList;
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

				UpdateFont();
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
			base.OnHandleCreated(e);

			if (DesignMode)
				SetTheme(ThemeManagerFactory.GetThemeManager().CurrentTheme);

			UpdateFont();
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
			colors = theme.ComboBoxColors;
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
			isButtonHovered = false;
			Invalidate();

			base.OnMouseLeave(e);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void OnMouseMove(MouseEventArgs e)
		{
			bool wasButtonHovered = isButtonHovered;
			isButtonHovered = GetDropDownButtonRect(ClientRectangle).Contains(e.Location);

			if (wasButtonHovered != isButtonHovered)
				Invalidate();

			base.OnMouseMove(e);
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
		protected override void OnEnabledChanged(EventArgs e)
		{
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

			DrawBackground(g, ClientRectangle, stateColors);
			DrawText(g, ClientRectangle, stateColors);
			DrawDropDownButton(g, ClientRectangle, stateColors);
			DrawFocus(g, ClientRectangle);
		}



		/********************************************************************/
		/// <summary>
		/// Handle dropdown opening
		/// </summary>
		/********************************************************************/
		protected override void OnDropDown(EventArgs e)
		{
			Invalidate();

			base.OnDropDown(e);
		}



		/********************************************************************/
		/// <summary>
		/// Handle dropdown closing
		/// </summary>
		/********************************************************************/
		protected override void OnDropDownClosed(EventArgs e)
		{
			Invalidate();

			base.OnDropDownClosed(e);
		}



		/********************************************************************/
		/// <summary>
		/// Custom draw each item in the dropdown list
		/// </summary>
		/********************************************************************/
		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			if (e.Index < 0)
				return;

			DropDownStateColors stateColors = GetDropDownColors(e.State);

			Graphics g = e.Graphics;

			g.SmoothingMode = SmoothingMode.AntiAlias;

			DrawDropDownBackground(g, e.Bounds, stateColors);
			DrawDropDownItemText(g, e.Bounds, stateColors, e.Index);

			base.OnDrawItem(e);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Set font to use
		/// </summary>
		/********************************************************************/
		private void UpdateFont()
		{
			Font = fontConfiguration?.Font ?? fonts.RegularFont;
		}



		/********************************************************************/
		/// <summary>
		/// Get the rectangle for the dropdown button
		/// </summary>
		/********************************************************************/
		private Rectangle GetDropDownButtonRect(Rectangle rect)
		{
			return new Rectangle(rect.Width - DropDownButtonWidth - 2, 2, DropDownButtonWidth - 1, rect.Height - 5);
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
					BackgroundColor = colors.DisabledBackgroundColor,
					BorderColor = colors.DisabledBorderColor,
					TextColor = colors.DisabledTextColor,
					ArrowButtonBorderColor = colors.DisabledArrowButtonBorderColor,
					ArrowButtonBackgroundStartColor = colors.DisabledArrowButtonBackgroundStartColor,
					ArrowButtonBackgroundStopColor = colors.DisabledArrowButtonBackgroundStopColor,
					ArrowColor = colors.DisabledArrowColor
				};
			}

			if (DroppedDown)
			{
				return new StateColors
				{
					BackgroundColor = colors.PressedBackgroundColor,
					BorderColor = colors.PressedBorderColor,
					TextColor = colors.PressedTextColor,
					ArrowButtonBorderColor = colors.PressedArrowButtonBorderColor,
					ArrowButtonBackgroundStartColor = colors.PressedArrowButtonBackgroundStartColor,
					ArrowButtonBackgroundStopColor = colors.PressedArrowButtonBackgroundStopColor,
					ArrowColor = colors.PressedArrowColor
				};
			}

			if (isButtonHovered)
			{
				return new StateColors
				{
					BackgroundColor = colors.HoverBackgroundColor,
					BorderColor = colors.HoverBorderColor,
					TextColor = colors.HoverTextColor,
					ArrowButtonBorderColor = colors.HoverArrowButtonBorderColor,
					ArrowButtonBackgroundStartColor = colors.HoverArrowButtonBackgroundStartColor,
					ArrowButtonBackgroundStopColor = colors.HoverArrowButtonBackgroundStopColor,
					ArrowColor = colors.HoverArrowColor
				};
			}

			if (isHovered)
			{
				return new StateColors
				{
					BackgroundColor = colors.HoverBackgroundColor,
					BorderColor = colors.HoverBorderColor,
					TextColor = colors.HoverTextColor,
					ArrowButtonBorderColor = colors.NormalArrowButtonBorderColor,
					ArrowButtonBackgroundStartColor = colors.NormalArrowButtonBackgroundStartColor,
					ArrowButtonBackgroundStopColor = colors.NormalArrowButtonBackgroundStopColor,
					ArrowColor = colors.NormalArrowColor
				};
			}

			if (Focused)
			{
				return new StateColors
				{
					BackgroundColor = colors.FocusedBackgroundColor,
					BorderColor = colors.FocusedBorderColor,
					TextColor = colors.FocusedTextColor,
					ArrowButtonBorderColor = colors.FocusedArrowButtonBorderColor,
					ArrowButtonBackgroundStartColor = colors.FocusedArrowButtonBackgroundStartColor,
					ArrowButtonBackgroundStopColor = colors.FocusedArrowButtonBackgroundStopColor,
					ArrowColor = colors.FocusedArrowColor
				};
			}

			return new StateColors
			{
				BackgroundColor = colors.NormalBackgroundColor,
				BorderColor = colors.NormalBorderColor,
				TextColor = colors.NormalTextColor,
				ArrowButtonBorderColor = colors.NormalArrowButtonBorderColor,
				ArrowButtonBackgroundStartColor = colors.NormalArrowButtonBackgroundStartColor,
				ArrowButtonBackgroundStopColor = colors.NormalArrowButtonBackgroundStopColor,
				ArrowColor = colors.NormalArrowColor
			};
		}



		/********************************************************************/
		/// <summary>
		/// Draw the background and border
		/// </summary>
		/********************************************************************/
		private void DrawBackground(Graphics g, Rectangle rect, StateColors stateColors)
		{
			Rectangle outerRect = new Rectangle(rect.X, rect.Y, rect.Width - 1, rect.Height - 1);

			using (Brush b = new SolidBrush(stateColors.BackgroundColor))
			{
				g.FillRectangle(b, outerRect);
			}

			using (Pen p = new Pen(stateColors.BorderColor))
			{
				g.DrawRectangle(p, outerRect);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the selected item text
		/// </summary>
		/********************************************************************/
		private void DrawText(Graphics g, Rectangle rect, StateColors stateColors)
		{
			if (SelectedIndex < 0)
				return;

			Font font = Font;

			string text = GetItemText(SelectedItem);

			TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;
			Rectangle drawRect = new Rectangle(rect.X, rect.Y, rect.Width - DropDownButtonWidth - PaddingBetweenTextAndButton, rect.Height);
			TextRenderer.DrawText(g, text, font, drawRect, stateColors.TextColor, flags);
		}



		/********************************************************************/
		/// <summary>
		/// Draw the dropdown button with arrow
		/// </summary>
		/********************************************************************/
		private void DrawDropDownButton(Graphics g, Rectangle rect, StateColors stateColors)
		{
			Rectangle buttonRect = GetDropDownButtonRect(rect);

			using (LinearGradientBrush brush = new LinearGradientBrush(buttonRect, stateColors.ArrowButtonBackgroundStartColor, stateColors.ArrowButtonBackgroundStopColor, LinearGradientMode.Vertical))
			{
				g.FillRectangle(brush, buttonRect);
			}

			using (Pen p = new Pen(stateColors.ArrowButtonBorderColor))
			{
				g.DrawRectangle(p, buttonRect);
			}

			// Draw arrow
			int arrowSize = 4;
			int centerX = buttonRect.Left + (buttonRect.Width / 2);
			int centerY = buttonRect.Top + (buttonRect.Height / 2);

			Point[] arrowPoints =
			[
				new Point(centerX - arrowSize, centerY - 2),
				new Point(centerX + arrowSize, centerY - 2),
				new Point(centerX, centerY + 2)
			];

			using (SolidBrush brush = new SolidBrush(stateColors.ArrowColor))
			{
				g.FillPolygon(brush, arrowPoints);
			}
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
				Rectangle buttonRect = GetDropDownButtonRect(rect);
				Rectangle focusRect = Rectangle.Inflate(buttonRect, -3, -3);
				ControlPaint.DrawFocusRectangle(g, focusRect);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the colors to use for the dropdown list in the current
		/// state
		/// </summary>
		/********************************************************************/
		private DropDownStateColors GetDropDownColors(DrawItemState state)
		{
			if ((state & DrawItemState.Selected) == DrawItemState.Selected)
			{
				return new DropDownStateColors
				{
					BackgroundStartColor = colors.SelectedDropDownBackgroundStartColor,
					BackgroundMiddleColor = colors.SelectedDropDownBackgroundMiddleColor,
					BackgroundStopColor = colors.SelectedDropDownBackgroundStopColor,
					TextColor = colors.SelectedDropDownTextColor
				};
			}

			return new DropDownStateColors
			{
				BackgroundStartColor = colors.NormalDropDownBackgroundStartColor,
				BackgroundMiddleColor = colors.NormalDropDownBackgroundMiddleColor,
				BackgroundStopColor = colors.NormalDropDownBackgroundStopColor,
				TextColor = colors.NormalDropDownTextColor
			};
		}



		/********************************************************************/
		/// <summary>
		/// Draw the dropdown background and border
		/// </summary>
		/********************************************************************/
		private void DrawDropDownBackground(Graphics g, Rectangle rect, DropDownStateColors stateColors)
		{
			g.SmoothingMode = SmoothingMode.None;

			using (LinearGradientBrush brush = new LinearGradientBrush(rect, stateColors.BackgroundStartColor, stateColors.BackgroundStopColor, LinearGradientMode.Vertical))
			{
				ColorBlend blend = new ColorBlend
				{
					Colors = [ stateColors.BackgroundStartColor, stateColors.BackgroundMiddleColor, stateColors.BackgroundMiddleColor, stateColors.BackgroundStopColor ],
					Positions = [ 0.0f, 0.1f, 0.7f, 1.0f ]
				};

				brush.InterpolationColors = blend;

				g.FillRectangle(brush, rect);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the dropdown item text
		/// </summary>
		/********************************************************************/
		private void DrawDropDownItemText(Graphics g, Rectangle rect, DropDownStateColors stateColors, int itemIndex)
		{
			Font font = Font;

			string text = GetItemText(Items[itemIndex]);

			TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;
			TextRenderer.DrawText(g, text, font, rect, stateColors.TextColor, flags);
		}
		#endregion

		#region Designer filtering (hide properties from PropertyGrid)
		/********************************************************************/
		/// <summary>
		/// Register a provider that filters properties for the designer
		/// </summary>
		/********************************************************************/
		static NostalgicComboBox()
		{
			TypeDescriptor.AddProvider(new NostalgicComboBoxTypeDescriptionProvider(), typeof(NostalgicComboBox));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer.
		/// This is needed for properties that we cannot override, but we do
		/// it for all our hidden properties to be sure
		/// </summary>
		private sealed class NostalgicComboBoxTypeDescriptionProvider : TypeDescriptionProvider
		{
			private static readonly TypeDescriptionProvider parent = TypeDescriptor.GetProvider(typeof(ComboBox));

			private static readonly string[] propertiesToHide =
			[
				nameof(FlatStyle),
				nameof(BackColor),
				nameof(BackgroundImage),
				nameof(BackgroundImageLayout),
				nameof(Font),
				nameof(ForeColor),
				nameof(RightToLeft),
				nameof(DropDownStyle),
				nameof(DrawMode)
			];

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public NostalgicComboBoxTypeDescriptionProvider() : base(parent)
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
