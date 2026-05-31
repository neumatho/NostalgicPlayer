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

namespace Polycode.NostalgicPlayer.Controls.Inputs
{
	/// <summary>
	/// Themed text box with custom rendering
	/// </summary>
	internal class NostalgicTextBoxInternal : RichTextBox, IThemeControl, IFontConfiguration
	{
		/// <summary>
		/// Thickness in pixels of the custom border drawn in the non-client area
		/// </summary>
		public const int BorderWidth = 1;

		/// <summary>
		/// Horizontal padding in pixels between the border and the rendered text
		/// </summary>
		public const int HorizontalPadding = 2;

		/// <summary>
		/// Vertical padding in pixels between the border and the rendered text
		/// </summary>
		public const int VerticalPadding = 3;

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

		private int scrollOffset;

		private bool isMouseSelecting;
		private int selectionAnchor;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicTextBoxInternal()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

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



		/********************************************************************/
		/// <summary>
		/// When set, the control behaves as a single-line text box even
		/// though Multiline is enabled (which is needed so it can be made
		/// taller than one line). The Enter key is treated as a dialog key
		/// instead of inserting a new line
		/// </summary>
		/********************************************************************/
		[Category("Behavior")]
		[Description("Behave as a single-line text box (Enter does not insert a new line).")]
		[DefaultValue(false)]
		public bool SingleLine { get; set; }
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
			if (isMouseSelecting && ((e.Button & MouseButtons.Left) != 0))
			{
				int index = GetCharIndexFromX(e.X);

				SelectionStart = Math.Min(selectionAnchor, index);
				SelectionLength = Math.Abs(index - selectionAnchor);

				Invalidate();
			}
			else if (e.Button != MouseButtons.None)
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
			if (isMouseSelecting && (e.Button == MouseButtons.Left))
			{
				isMouseSelecting = false;
				Capture = false;
			}

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
		/// In single-line mode, treat Enter as a dialog key (so it triggers
		/// the form's accept button) instead of letting the multiline
		/// control insert a new line
		/// </summary>
		/********************************************************************/
		protected override bool IsInputKey(Keys keyData)
		{
			if (SingleLine && ((keyData & Keys.KeyCode) == Keys.Return))
				return false;

			return base.IsInputKey(keyData);
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

				case WM.LBUTTONDOWN:
				{
					// The native control hit-tests in its own coordinate
					// space, which does not match our custom layout (padding
					// + scroll offset), so place the caret ourselves and
					// swallow the message to stop the native repositioning
					HandleMouseDown(GetMouseX(m), false);
					return;
				}

				case WM.LBUTTONDBLCLK:
				{
					HandleMouseDown(GetMouseX(m), true);
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
		/// Extract the client X coordinate from a mouse message LParam
		/// </summary>
		/********************************************************************/
		private static int GetMouseX(Message m)
		{
			return (short)(m.LParam.ToInt32() & 0xffff);
		}



		/********************************************************************/
		/// <summary>
		/// Place the caret (or select a word on double-click) at the given
		/// client X coordinate and begin a mouse selection
		/// </summary>
		/********************************************************************/
		private void HandleMouseDown(int x, bool isDoubleClick)
		{
			if (!Focused)
				Focus();

			int index = GetCharIndexFromX(x);

			if (isDoubleClick)
			{
				SelectWordAt(index);

				isMouseSelecting = false;
			}
			else if ((ModifierKeys & Keys.Shift) != 0)
			{
				// Extend the existing selection from its fixed end
				int anchor = SelectionStart;
				if (index < SelectionStart)
					anchor = SelectionStart + SelectionLength;

				selectionAnchor = anchor;
				isMouseSelecting = true;
				Capture = true;

				SelectionStart = Math.Min(anchor, index);
				SelectionLength = Math.Abs(index - anchor);
			}
			else
			{
				selectionAnchor = index;
				isMouseSelecting = true;
				Capture = true;

				SelectionStart = index;
				SelectionLength = 0;
			}

			OnMouseDown(new MouseEventArgs(MouseButtons.Left, isDoubleClick ? 2 : 1, x, Height / 2, 0));

			ResetCaret();
			Invalidate();
		}



		/********************************************************************/
		/// <summary>
		/// Select the whole word surrounding the given character index
		/// </summary>
		/********************************************************************/
		private void SelectWordAt(int index)
		{
			string text = Text;
			if (text.Length == 0)
				return;

			int start = Math.Min(index, text.Length - 1);
			int end = start;

			bool IsWordChar(char c) => char.IsLetterOrDigit(c) || (c == '_');

			if (IsWordChar(text[start]))
			{
				while ((start > 0) && IsWordChar(text[start - 1]))
					start--;

				while ((end < text.Length) && IsWordChar(text[end]))
					end++;
			}
			else
				end = start + 1;

			SelectionStart = start;
			SelectionLength = end - start;
		}



		/********************************************************************/
		/// <summary>
		/// Map a client X coordinate to the nearest character index, taking
		/// the text padding and our own horizontal scroll offset into
		/// account (inverse of GetCharacterX)
		/// </summary>
		/********************************************************************/
		private int GetCharIndexFromX(int x)
		{
			string text = Text;
			if (text.Length == 0)
				return 0;

			using (Graphics g = CreateGraphics())
			{
				Font font = GetFont();

				int prevX = HorizontalPadding - scrollOffset;

				for (int i = 0; i < text.Length; i++)
				{
					int charRight = GetCharacterX(g, font, text, i + 1) - scrollOffset;
					int mid = (prevX + charRight) / 2;

					if (x < mid)
						return i;

					prevX = charRight;
				}

				return text.Length;
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
				return HorizontalPadding;

			if (charIndex >= displayText.Length)
			{
				Size fullSize = TextRenderer.MeasureText(g, displayText, font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
				return HorizontalPadding + fullSize.Width;
			}

			string substring = displayText.Substring(0, charIndex);
			Size size = TextRenderer.MeasureText(g, substring, font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);

			return HorizontalPadding + size.Width;
		}



		/********************************************************************/
		/// <summary>
		/// Update the horizontal scroll offset so the caret stays visible.
		/// We track the offset ourselves rather than asking the native
		/// control, anchoring to the right edge when the caret reaches it
		/// (so a newly typed char is shown in full and the left is clipped)
		/// </summary>
		/********************************************************************/
		private void UpdateScrollOffset(Graphics g, Font font, string displayText)
		{
			int visibleWidth = ClientRectangle.Width - (HorizontalPadding * 2);
			if (visibleWidth <= 0)
				return;

			// Width of the text up to the caret, and of the whole text,
			// both measured from the text start (excluding TextPadding)
			int caretX = GetCharacterX(g, font, displayText, SelectionStart) - HorizontalPadding;
			int totalWidth = GetCharacterX(g, font, displayText, displayText.Length) - HorizontalPadding;

			// Reserve room for the caret line itself at the right edge
			const int CaretWidth = 1;

			// Caret moved past the right edge: scroll left (anchor right)
			if ((caretX - scrollOffset) > (visibleWidth - CaretWidth))
				scrollOffset = caretX - visibleWidth + CaretWidth;

			// Caret moved past the left edge: scroll right (anchor left)
			else if ((caretX - scrollOffset) < 0)
				scrollOffset = caretX;

			// Never scroll further than needed to fill the visible area,
			// and never into negative territory
			int maxOffset = Math.Max(0, totalWidth - visibleWidth);
			if (scrollOffset > maxOffset)
				scrollOffset = maxOffset;

			if (scrollOffset < 0)
				scrollOffset = 0;
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

			// The padded inner area within which text is allowed to render.
			// Used as a clip rectangle by DrawClippedText so chars that have
			// scrolled off (or extend past the right side) don't bleed into
			// the TextPadding stripes
			Rectangle textRect = new Rectangle(rect.Left + HorizontalPadding, rect.Top, rect.Width - (HorizontalPadding * 2), rect.Height);

			UpdateScrollOffset(g, font, displayText);

			if (displayText.Length > 0)
			{
				int textX = HorizontalPadding - scrollOffset;

				if ((SelectionLength > 0) && Focused)
					DrawTextWithSelection(g, font, stateColors, displayText, textX, textY, scrollOffset, textRect);
				else
					DrawPlainText(g, font, stateColors, displayText, textX, textY, textRect);
			}

			if (Focused && caretVisible && (SelectionLength == 0) && Enabled)
				DrawCaret(g, font, stateColors, displayText, textY, textRect);
		}



		/********************************************************************/
		/// <summary>
		/// Draw text without any selection
		/// </summary>
		/********************************************************************/
		private void DrawPlainText(Graphics g, Font font, StateColors stateColors, string displayText, int textX, int textY, Rectangle textRect)
		{
			DrawClippedText(g, displayText, font, textX, textY, stateColors.TextColor, textRect);
		}



		/********************************************************************/
		/// <summary>
		/// Draw text starting at (startX, y), dropping any leading chars
		/// that would fall outside the textRect on the left and clipping
		/// the right side via the bounds passed to TextRenderer.DrawText
		///
		/// TextRenderer.DrawText with a Point overload does not reliably
		/// respect Graphics.Clip, so we must clip ourselves
		/// </summary>
		/********************************************************************/
		private void DrawClippedText(Graphics g, string text, Font font, int startX, int y, Color color, Rectangle textRect)
		{
			if (text.Length == 0)
				return;

			TextFormatFlags flags = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.Left;
			Size measureBounds = new Size(int.MaxValue, int.MaxValue);

			// Drop leading chars whose right edge is still left of textRect.Left
			int x = startX;
			int firstVisibleIdx = 0;
			while ((firstVisibleIdx < text.Length) && (x < textRect.Left))
			{
				Size charSize = TextRenderer.MeasureText(g, text.Substring(firstVisibleIdx, 1), font, measureBounds, flags);
				x += charSize.Width;
				firstVisibleIdx++;
			}

			if (firstVisibleIdx >= text.Length)
				return;

			string visible = text.Substring(firstVisibleIdx);

			Rectangle bounds = new Rectangle(x, y, textRect.Right - x, font.Height);
			if (bounds.Width <= 0)
				return;

			TextRenderer.DrawText(g, visible, font, bounds, color, flags);
		}



		/********************************************************************/
		/// <summary>
		/// Draw text with selection highlight, rendering each segment
		/// separately to avoid subpixel rendering artifacts
		/// </summary>
		/********************************************************************/
		private void DrawTextWithSelection(Graphics g, Font font, StateColors stateColors, string displayText, int textX, int textY, int scrollOffset, Rectangle textRect)
		{
			int selStart = SelectionStart;
			int selEnd = selStart + SelectionLength;

			int selStartX = GetCharacterX(g, font, displayText, selStart) - scrollOffset;
			int selEndX = GetCharacterX(g, font, displayText, selEnd) - scrollOffset;

			// Clip the selection highlight rect to the padded text area
			int bgLeft = Math.Max(selStartX, textRect.Left);
			int bgRight = Math.Min(selEndX, textRect.Right);
			if (bgRight > bgLeft)
			{
				Rectangle selRect = new Rectangle(bgLeft, textY, bgRight - bgLeft, font.Height);

				using (Brush selBrush = new SolidBrush(colors.SelectedTextBackgroundColor))
				{
					g.FillRectangle(selBrush, selRect);
				}
			}

			if (selStart > 0)
			{
				string beforeText = displayText.Substring(0, selStart);
				DrawClippedText(g, beforeText, font, textX, textY, stateColors.TextColor, textRect);
			}

			string selectedText = displayText.Substring(selStart, selEnd - selStart);
			DrawClippedText(g, selectedText, font, selStartX, textY, colors.SelectedTextColor, textRect);

			if (selEnd < displayText.Length)
			{
				string afterText = displayText.Substring(selEnd);
				DrawClippedText(g, afterText, font, selEndX, textY, stateColors.TextColor, textRect);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the blinking caret
		/// </summary>
		/********************************************************************/
		private void DrawCaret(Graphics g, Font font, StateColors stateColors, string displayText, int textY, Rectangle textRect)
		{
			int caretX = GetCharacterX(g, font, displayText, SelectionStart) - scrollOffset;

			// Hide the caret when it has scrolled outside the padded text area
			if ((caretX < textRect.Left) || (caretX >= textRect.Right))
				return;

			using (Pen caretPen = new Pen(stateColors.TextColor))
			{
				g.DrawLine(caretPen, caretX, textY, caretX, textY + font.Height - 1);
			}
		}
		#endregion
	}
}
