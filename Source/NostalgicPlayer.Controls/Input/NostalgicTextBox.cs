/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Components;
using Polycode.NostalgicPlayer.Controls.Designer;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;
using Polycode.NostalgicPlayer.Controls.Theme.Standard;
using Polycode.NostalgicPlayer.Platform.Native;

namespace Polycode.NostalgicPlayer.Controls.Input
{
	/// <summary>
	/// Themed text box with custom rendering
	/// </summary>
	public class NostalgicTextBox : RichTextBox, IThemeControl, IFontConfiguration
	{
		private const int BorderWidth = 1;
		private const int TextPadding = 3;

		private struct StateColors
		{
			public Color BorderColor { get; init; }
			public Color BackgroundColor { get; init; }
			public Color TextColor { get; init; }
		}

		private ITextBoxColors colors;
		private IFonts fonts;

		private FontConfiguration fontConfiguration;

		private bool isHovered;

		private readonly Timer caretTimer;
		private bool caretVisible;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicTextBox()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

			// Make the RichTextBox behave as a single-line text box
			Multiline = false;

			caretTimer = new Timer();
			caretTimer.Interval = SystemInformation.CaretBlinkTime > 0 ? SystemInformation.CaretBlinkTime : 530;
			caretTimer.Tick += CaretTimer_Tick;
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
		/// Initialize the control to use custom rendering
		/// </summary>
		/********************************************************************/
		protected override void OnHandleCreated(EventArgs e)
		{
			if (DesignerHelper.IsInDesignMode(this))
				SetTheme(new StandardTheme());

			base.OnHandleCreated(e);
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup
		/// </summary>
		/********************************************************************/
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				caretTimer.Stop();
				caretTimer.Dispose();
			}

			base.Dispose(disposing);
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
			colors = theme.TextBoxColors;
			fonts = theme.StandardFonts;

			Invalidate();
			RefreshNonClientArea();
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
			RefreshNonClientArea();

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
			RefreshNonClientArea();

			base.OnMouseLeave(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnMouseDown(MouseEventArgs e)
		{
			Invalidate();

			base.OnMouseDown(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (e.Button != MouseButtons.None)
				Invalidate();

			base.OnMouseMove(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnMouseUp(MouseEventArgs e)
		{
			Invalidate();

			base.OnMouseUp(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnGotFocus(EventArgs e)
		{
			caretVisible = true;
			caretTimer.Start();

			Invalidate();
			RefreshNonClientArea();

			base.OnGotFocus(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnLostFocus(EventArgs e)
		{
			caretTimer.Stop();
			caretVisible = false;

			Invalidate();
			RefreshNonClientArea();

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
			RefreshNonClientArea();

			base.OnEnabledChanged(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnTextChanged(EventArgs e)
		{
			ResetCaret();
			Invalidate();

			base.OnTextChanged(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			ResetCaret();
			Invalidate();
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);

			Invalidate();
		}



		/********************************************************************/
		/// <summary>
		/// Intercept non-client paint to draw themed border
		/// </summary>
		/********************************************************************/
		protected override void WndProc(ref Message m)
		{
			switch ((WM)m.Msg)
			{
				case WM.NCCALCSIZE:
				{
					RecalculateClientArea(m);

					m.Result = IntPtr.Zero;
					return;
				}

				case WM.NCPAINT:
				{
					DrawCustomNonClient();

					m.Result = IntPtr.Zero;
					return;
				}
			}

			base.WndProc(ref m);

			// The native richedit control creates/shows its caret in
			// response to various messages. Hide it after every message,
			// since we draw our own caret in OnPaint
			if (Focused)
				User32.HideCaret(Handle);
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

			Rectangle rect = ClientRectangle;
			Font font = GetFont();
			StateColors stateColors = GetColors();

			DrawBackground(g, rect, stateColors);
			DrawTextContent(g, rect, font, stateColors);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will recalculate the client area of the form
		/// </summary>
		/********************************************************************/
		private void RecalculateClientArea(Message m)
		{
			if (m.WParam != IntPtr.Zero)
			{
				// wParam is TRUE: using NCCALCSIZE_PARAMS
				NCCALCSIZE_PARAMS ncParams = Marshal.PtrToStructure<NCCALCSIZE_PARAMS>(m.LParam);
				
				// Shrink client area by border width on all sides
				ncParams.rgrc0.Left += BorderWidth;
				ncParams.rgrc0.Top += BorderWidth;
				ncParams.rgrc0.Right -= BorderWidth;
				ncParams.rgrc0.Bottom -= BorderWidth;
				
				Marshal.StructureToPtr(ncParams, m.LParam, false);
			}
			else
			{
				// wParam is FALSE: using simple RECT
				RECT rect = Marshal.PtrToStructure<RECT>(m.LParam);
				
				// Shrink client area by border width on all sides
				rect.Left += BorderWidth;
				rect.Top += BorderWidth;
				rect.Right -= BorderWidth;
				rect.Bottom -= BorderWidth;
				
				Marshal.StructureToPtr(rect, m.LParam, false);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Force a repaint of the title bar and border of the form
		/// </summary>
		/********************************************************************/
		private void RefreshNonClientArea()
		{
			if (!IsHandleCreated)
				return;

			User32.RedrawWindow(Handle, IntPtr.Zero, IntPtr.Zero, (uint)(RDW.INVALIDATE | RDW.FRAME | RDW.NOCHILDREN));
		}



		/********************************************************************/
		/// <summary>
		/// Reset caret to visible state
		/// </summary>
		/********************************************************************/
		private void ResetCaret()
		{
			if (Focused)
			{
				caretVisible = true;
				caretTimer.Stop();
				caretTimer.Start();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Get the pixel position for a character index, accounting for
		/// the native edit control's internal scroll position
		/// </summary>
		/********************************************************************/
		private int GetCharacterX(Graphics g, Font font, string displayText, int charIndex)
		{
			if (charIndex <= 0)
				return TextPadding;

			if (charIndex >= displayText.Length)
			{
				Size fullSize = TextRenderer.MeasureText(g, displayText, font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
				return TextPadding + fullSize.Width;
			}

			string substring = displayText.Substring(0, charIndex);
			Size size = TextRenderer.MeasureText(g, substring, font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);

			return TextPadding + size.Width;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the scroll offset so the caret stays visible
		/// </summary>
		/********************************************************************/
		private int GetScrollOffset(Graphics g, Font font, string displayText)
		{
			// Use the native edit control's first visible character to determine scroll
			int firstVisibleChar = GetCharIndexFromPosition(new Point(TextPadding + 1, Height / 2));

			if (firstVisibleChar > 0)
			{
				string beforeVisible = displayText.Substring(0, Math.Min(firstVisibleChar, displayText.Length));
				Size beforeSize = TextRenderer.MeasureText(g, beforeVisible, font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);

				return beforeSize.Width;
			}

			return 0;
		}
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Caret blink timer
		/// </summary>
		/********************************************************************/
		private void CaretTimer_Tick(object sender, EventArgs e)
		{
			caretVisible = !caretVisible;
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
					BackgroundColor = colors.DisabledBackgroundColor,
					TextColor = colors.DisabledTextColor
				};
			}

			if (Focused)
			{
				return new StateColors
				{
					BorderColor = colors.FocusedBorderColor,
					BackgroundColor = colors.FocusedBackgroundColor,
					TextColor = colors.FocusedTextColor
				};
			}

			if (isHovered)
			{
				return new StateColors
				{
					BorderColor = colors.HoverBorderColor,
					BackgroundColor = colors.HoverBackgroundColor,
					TextColor = colors.HoverTextColor
				};
			}

			return new StateColors
			{
				BorderColor = colors.NormalBorderColor,
				BackgroundColor = colors.NormalBackgroundColor,
				TextColor = colors.NormalTextColor
			};
		}



		/********************************************************************/
		/// <summary>
		/// Will draw the title bar and border of the form
		/// </summary>
		/********************************************************************/
		private void DrawCustomNonClient()
		{
			if (!IsHandleCreated || (colors == null))
				return;

			// Draw border in non-client area
			IntPtr hdc = IntPtr.Zero;
			
			try
			{
				hdc = User32.GetWindowDC(Handle);
				if (hdc != IntPtr.Zero)
				{
					using (Graphics g = Graphics.FromHdc(hdc))
					{
						DrawBorder(g, GetColors());
					}
				}
			}
			finally
			{
				if (hdc != IntPtr.Zero)
					User32.ReleaseDC(Handle, hdc);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the themed border in the non-client area
		/// </summary>
		/********************************************************************/
		private void DrawBorder(Graphics g, StateColors stateColors)
		{
			using (Pen borderPen = new Pen(stateColors.BorderColor, BorderWidth))
			{
				g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the background
		/// </summary>
		/********************************************************************/
		private void DrawBackground(Graphics g, Rectangle rect, StateColors stateColors)
		{
			using (Brush brush = new SolidBrush(stateColors.BackgroundColor))
			{
				g.FillRectangle(brush, rect);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the text content including selection and caret
		/// </summary>
		/********************************************************************/
		private void DrawTextContent(Graphics g, Rectangle rect, Font font, StateColors stateColors)
		{
			string displayText = Text;
			int textY = (rect.Height - font.Height + 1) / 2;

			// Clip to the text area. The border is in the non-client area,
			// so the full client rectangle is available for text
			Region oldClip = g.Clip;
			g.SetClip(rect);

			int scrollOffset = 0;

			if (displayText.Length > 0)
				scrollOffset = GetScrollOffset(g, font, displayText);

			if (displayText.Length > 0)
			{
				int textX = TextPadding - scrollOffset;

				if ((SelectionLength > 0) && Focused)
					DrawTextWithSelection(g, font, stateColors, displayText, textX, textY, scrollOffset);
				else
					DrawPlainText(g, font, stateColors, displayText, textX, textY);
			}

			if (Focused && caretVisible && (SelectionLength == 0) && Enabled)
				DrawCaret(g, font, stateColors, displayText, textY, scrollOffset);

			g.Clip = oldClip;
		}



		/********************************************************************/
		/// <summary>
		/// Draw text without any selection
		/// </summary>
		/********************************************************************/
		private void DrawPlainText(Graphics g, Font font, StateColors stateColors, string displayText, int textX, int textY)
		{
			TextRenderer.DrawText(g, displayText, font, new Point(textX, textY), stateColors.TextColor, TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
		}



		/********************************************************************/
		/// <summary>
		/// Draw text with selection highlight, rendering each segment
		/// separately to avoid subpixel rendering artifacts
		/// </summary>
		/********************************************************************/
		private void DrawTextWithSelection(Graphics g, Font font, StateColors stateColors, string displayText, int textX, int textY, int scrollOffset)
		{
			int selStart = SelectionStart;
			int selEnd = selStart + SelectionLength;

			int selStartX = GetCharacterX(g, font, displayText, selStart) - scrollOffset;
			int selEndX = GetCharacterX(g, font, displayText, selEnd) - scrollOffset;

			Rectangle selRect = new Rectangle(selStartX, textY, selEndX - selStartX, font.Height);

			// Draw selection highlight background
			using (Brush selBrush = new SolidBrush(colors.SelectedTextBackgroundColor))
			{
				g.FillRectangle(selBrush, selRect);
			}

			// Draw text before selection
			if (selStart > 0)
			{
				string beforeText = displayText.Substring(0, selStart);
				TextRenderer.DrawText(g, beforeText, font, new Point(textX, textY), stateColors.TextColor, TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
			}

			// Draw selected text
			string selectedText = displayText.Substring(selStart, selEnd - selStart);
			TextRenderer.DrawText(g, selectedText, font, new Point(selStartX, textY), colors.SelectedTextColor, TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);

			// Draw text after selection
			if (selEnd < displayText.Length)
			{
				string afterText = displayText.Substring(selEnd);
				TextRenderer.DrawText(g, afterText, font, new Point(selEndX, textY), stateColors.TextColor, TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the blinking caret
		/// </summary>
		/********************************************************************/
		private void DrawCaret(Graphics g, Font font, StateColors stateColors, string displayText, int textY, int scrollOffset)
		{
			int caretX = GetCharacterX(g, font, displayText, SelectionStart) - scrollOffset;

			using (Pen caretPen = new Pen(stateColors.TextColor))
			{
				g.DrawLine(caretPen, caretX, textY, caretX, textY + font.Height - 1);
			}
		}
		#endregion

		#region Designer filtering (hide properties from PropertyGrid)
		/********************************************************************/
		/// <summary>
		/// Register a provider that filters properties for the designer
		/// </summary>
		/********************************************************************/
		static NostalgicTextBox()
		{
			TypeDescriptor.AddProvider(new NostalgicTextBoxTypeDescriptionProvider(), typeof(NostalgicTextBox));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer.
		/// This is needed for properties that we cannot override, but we do
		/// it for all our hidden properties to be sure
		/// </summary>
		private sealed class NostalgicTextBoxTypeDescriptionProvider : TypeDescriptionProvider
		{
			private static readonly TypeDescriptionProvider parent = TypeDescriptor.GetProvider(typeof(RichTextBox));

			private static readonly string[] propertiesToHide =
			[
				nameof(BackColor),
				nameof(BackgroundImage),
				nameof(BackgroundImageLayout),
				nameof(BorderStyle),
				nameof(Font),
				nameof(ForeColor),
				nameof(RightToLeft)
			];

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public NostalgicTextBoxTypeDescriptionProvider() : base(parent)
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
