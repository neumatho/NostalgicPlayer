/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Components;
using Polycode.NostalgicPlayer.Controls.Designer;
using Polycode.NostalgicPlayer.Controls.Inputs.RichText;
using Polycode.NostalgicPlayer.Controls.Lists;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;
using Polycode.NostalgicPlayer.Controls.Theme.Standard;

namespace Polycode.NostalgicPlayer.Controls.Inputs
{
	/// <summary>
	/// A themed, read-only multi-line rich text view. Content is added with the
	/// same SelectedText/SelectionFont/SelectionIndent/SelectionHangingIndent
	/// API as a RichTextBox
	/// </summary>
	public class NostalgicRichTextView : Control, IThemeControl, IFontConfiguration
	{
		private const int BorderWidth = 1;
		private const int HorizontalPadding = 3;
		private const int VerticalPadding = 3;

		private struct StateColors
		{
			public Color BorderColor { get; init; }
			public Color BackgroundColor { get; init; }
			public Color TextColor { get; init; }
		}

		private IRichTextViewColors colors;
		private IFonts fonts;

		private FontConfiguration fontConfiguration;

		private readonly RichTextDocument document = new RichTextDocument();
		private RichTextLayout layout;
		private bool layoutDirty = true;

		private readonly NostalgicVScrollBar vScrollBar;

		// Pen state used when appending text
		private Font selectionFont;
		private Color? selectionColor;
		private int selectionIndent;
		private int selectionHangingIndent;

		// Selection (global character indices into the plain text)
		private int selectionStart;
		private int selectionEnd;
		private int selectionAnchor;
		private bool isMouseSelecting;

		private bool isHovered;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicRichTextView()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
			SetStyle(ControlStyles.Selectable | ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);

			vScrollBar = new NostalgicVScrollBar
			{
				Visible = false
			};
			vScrollBar.ValueChanged += VScrollBar_ValueChanged;

			Controls.Add(vScrollBar);
		}

		#region Designer properties
		/********************************************************************/
		/// <summary>
		/// Set the FontConfiguration component to use for the default font
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

				layoutDirty = true;
				Invalidate();
			}
		}
		#endregion

		#region RichTextBox-like content API
		/********************************************************************/
		/// <summary>
		/// The plain text of the whole content
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string Text
		{
			get => document.GetPlainText();

			set
			{
				document.Clear();

				if (!string.IsNullOrEmpty(value))
					document.Append(value, selectionFont, selectionColor, 0, 0);

				ResetSelection();

				layoutDirty = true;
				Invalidate();
			}
		}



		/********************************************************************/
		/// <summary>
		/// The font used for text appended next
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Font SelectionFont
		{
			get => selectionFont;
			set => selectionFont = value;
		}



		/********************************************************************/
		/// <summary>
		/// The color used for text appended next, or null for the theme's
		/// default text color
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color SelectionColor
		{
			get => selectionColor ?? Color.Empty;
			set => selectionColor = value;
		}



		/********************************************************************/
		/// <summary>
		/// The left indentation in pixels used for paragraphs appended next
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectionIndent
		{
			get => selectionIndent;
			set => selectionIndent = value;
		}



		/********************************************************************/
		/// <summary>
		/// The hanging indentation in pixels used for paragraphs appended
		/// next
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectionHangingIndent
		{
			get => selectionHangingIndent;
			set => selectionHangingIndent = value;
		}



		/********************************************************************/
		/// <summary>
		/// Append text using the current pen state (font, color, indent)
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string SelectedText
		{
			get => string.Empty;

			set
			{
				document.Append(value, selectionFont, selectionColor, selectionIndent, selectionHangingIndent);

				layoutDirty = true;
				Invalidate();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Remove all content
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			document.Clear();

			ResetSelection();

			vScrollBar.Value = 0;

			layoutDirty = true;
			Invalidate();
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
			colors = theme.RichTextViewColors;
			fonts = theme.StandardFonts;

			vScrollBar.SetTheme(theme);

			layoutDirty = true;
			Invalidate();
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

			layoutDirty = true;
			Invalidate();
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Recalculate the layout when resized
		/// </summary>
		/********************************************************************/
		protected override void OnResize(EventArgs e)
		{
			layoutDirty = true;

			base.OnResize(e);
		}



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
		/// Begin a selection
		/// </summary>
		/********************************************************************/
		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				Focus();

				EnsureLayout();
				int index = HitTest(e.Location);

				selectionAnchor = index;
				selectionStart = index;
				selectionEnd = index;

				isMouseSelecting = true;
				Capture = true;

				Invalidate();
			}

			base.OnMouseDown(e);
		}



		/********************************************************************/
		/// <summary>
		/// Extend the selection
		/// </summary>
		/********************************************************************/
		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (isMouseSelecting && ((e.Button & MouseButtons.Left) != 0))
			{
				EnsureLayout();
				int index = HitTest(e.Location);

				selectionStart = Math.Min(selectionAnchor, index);
				selectionEnd = Math.Max(selectionAnchor, index);

				AutoScrollDuringSelection(e.Location);

				Invalidate();
			}

			base.OnMouseMove(e);
		}



		/********************************************************************/
		/// <summary>
		/// Finish a selection
		/// </summary>
		/********************************************************************/
		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (isMouseSelecting && (e.Button == MouseButtons.Left))
			{
				isMouseSelecting = false;
				Capture = false;
			}

			base.OnMouseUp(e);
		}



		/********************************************************************/
		/// <summary>
		/// Select the word under the cursor
		/// </summary>
		/********************************************************************/
		protected override void OnMouseDoubleClick(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				EnsureLayout();
				int index = HitTest(e.Location);

				SelectWordAt(index);

				isMouseSelecting = false;
				Capture = false;

				Invalidate();
			}

			base.OnMouseDoubleClick(e);
		}



		/********************************************************************/
		/// <summary>
		/// Scroll with the mouse wheel
		/// </summary>
		/********************************************************************/
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			if (vScrollBar.Visible)
			{
				int linesToScroll = SystemInformation.MouseWheelScrollLines;
				int delta = e.Delta / 120 * linesToScroll * Math.Max(1, vScrollBar.SmallChange);

				vScrollBar.Value -= delta;
			}

			base.OnMouseWheel(e);
		}



		/********************************************************************/
		/// <summary>
		/// Treat the navigation keys as input keys so we receive them
		/// </summary>
		/********************************************************************/
		protected override bool IsInputKey(Keys keyData)
		{
			switch (keyData & Keys.KeyCode)
			{
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
		/// Handle keyboard shortcuts (select all, copy) and scrolling
		/// </summary>
		/********************************************************************/
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Control && (e.KeyCode == Keys.A))
			{
				SelectAll();
				e.Handled = true;
			}
			else if (e.Control && (e.KeyCode == Keys.C))
			{
				CopySelectionToClipboard();
				e.Handled = true;
			}
			else if (vScrollBar.Visible)
			{
				switch (e.KeyCode)
				{
					case Keys.Up:
					{
						vScrollBar.Value -= vScrollBar.SmallChange;
						e.Handled = true;
						break;
					}

					case Keys.Down:
					{
						vScrollBar.Value += vScrollBar.SmallChange;
						e.Handled = true;
						break;
					}

					case Keys.PageUp:
					{
						vScrollBar.Value -= vScrollBar.LargeChange;
						e.Handled = true;
						break;
					}

					case Keys.PageDown:
					{
						vScrollBar.Value += vScrollBar.LargeChange;
						e.Handled = true;
						break;
					}

					case Keys.Home:
					{
						vScrollBar.Value = vScrollBar.Minimum;
						e.Handled = true;
						break;
					}

					case Keys.End:
					{
						vScrollBar.Value = vScrollBar.Maximum;
						e.Handled = true;
						break;
					}
				}
			}

			base.OnKeyDown(e);
		}



		/********************************************************************/
		/// <summary>
		/// Don't do anything, all painting is done in OnPaint
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
			EnsureLayout();

			Graphics g = e.Graphics;
			StateColors stateColors = GetColors();

			DrawBackground(g, stateColors);
			DrawBorder(g, stateColors);

			if ((colors == null) || (layout == null))
				return;

			DrawContent(g, stateColors);
		}
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Repaint when the scroll position changes
		/// </summary>
		/********************************************************************/
		private void VScrollBar_ValueChanged(object sender, EventArgs e)
		{
			Invalidate();
		}



		/********************************************************************/
		/// <summary>
		/// React when the attached FontConfiguration recalculates its font
		/// </summary>
		/********************************************************************/
		private void FontConfiguration_FontChanged(object sender, EventArgs e)
		{
			layoutDirty = true;
			Invalidate();
		}
		#endregion

		#region Geometry helpers
		/********************************************************************/
		/// <summary>
		/// The left edge of the text area
		/// </summary>
		/********************************************************************/
		private int TextLeft => BorderWidth + HorizontalPadding;



		/********************************************************************/
		/// <summary>
		/// The top edge of the text area
		/// </summary>
		/********************************************************************/
		private int TextTop => BorderWidth + VerticalPadding;



		/********************************************************************/
		/// <summary>
		/// The right edge of the text area (excludes the scrollbar when
		/// visible)
		/// </summary>
		/********************************************************************/
		private int TextRight
		{
			get
			{
				int innerRight = Width - BorderWidth - (vScrollBar.Visible ? vScrollBar.Width : 0);
				return innerRight - HorizontalPadding;
			}
		}



		/********************************************************************/
		/// <summary>
		/// The bottom edge of the text area
		/// </summary>
		/********************************************************************/
		private int TextBottom => Height - BorderWidth - VerticalPadding;



		/********************************************************************/
		/// <summary>
		/// The visible text height
		/// </summary>
		/********************************************************************/
		private int ViewportHeight => Math.Max(0, TextBottom - TextTop);



		/********************************************************************/
		/// <summary>
		/// The current vertical scroll offset in pixels
		/// </summary>
		/********************************************************************/
		private int ScrollOffset => vScrollBar.Visible ? vScrollBar.Value : 0;
		#endregion

		#region Layout
		/********************************************************************/
		/// <summary>
		/// Rebuild the layout if it is out of date, and update the scrollbar
		/// </summary>
		/********************************************************************/
		private void EnsureLayout()
		{
			if (!layoutDirty || !IsHandleCreated)
				return;

			Font defaultFont = GetDefaultFont();
			if (defaultFont == null)
				return;

			using (Graphics g = CreateGraphics())
			{
				layout = RichTextLayout.Build(document, defaultFont, TextLeft, TextRight, g);

				int viewport = ViewportHeight;
				bool needScrollBar = layout.ContentHeight > viewport;

				// The available width changes when the scrollbar appears or
				// disappears, so rebuild once if the visibility changed
				if (needScrollBar != vScrollBar.Visible)
				{
					vScrollBar.Visible = needScrollBar;
					layout = RichTextLayout.Build(document, defaultFont, TextLeft, TextRight, g);
				}
			}

			PositionScrollBar();

			if (vScrollBar.Visible)
			{
				vScrollBar.Maximum = layout.ContentHeight;
				vScrollBar.LargeChange = Math.Max(1, ViewportHeight);
				vScrollBar.SmallChange = Math.Max(1, GetDefaultFont().Height);
			}

			layoutDirty = false;
		}



		/********************************************************************/
		/// <summary>
		/// Position the scrollbar at the right edge inside the border
		/// </summary>
		/********************************************************************/
		private void PositionScrollBar()
		{
			vScrollBar.Bounds = new Rectangle(Width - BorderWidth - vScrollBar.Width, BorderWidth, vScrollBar.Width, Math.Max(0, Height - (BorderWidth * 2)));
		}
		#endregion

		#region Selection
		/********************************************************************/
		/// <summary>
		/// Reset the selection to empty
		/// </summary>
		/********************************************************************/
		private void ResetSelection()
		{
			selectionStart = 0;
			selectionEnd = 0;
			selectionAnchor = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Select all the text
		/// </summary>
		/********************************************************************/
		private void SelectAll()
		{
			selectionStart = 0;
			selectionEnd = document.GetTextLength();
			selectionAnchor = 0;

			Invalidate();
		}



		/********************************************************************/
		/// <summary>
		/// Select the whole word surrounding the given character index
		/// </summary>
		/********************************************************************/
		private void SelectWordAt(int index)
		{
			string text = document.GetPlainText();
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

			selectionStart = start;
			selectionEnd = end;
			selectionAnchor = start;
		}



		/********************************************************************/
		/// <summary>
		/// Copy the selected text to the clipboard
		/// </summary>
		/********************************************************************/
		private void CopySelectionToClipboard()
		{
			if (selectionEnd <= selectionStart)
				return;

			string text = document.GetPlainText();
			int end = Math.Min(selectionEnd, text.Length);
			if (end <= selectionStart)
				return;

			try
			{
				Clipboard.SetText(text.Substring(selectionStart, end - selectionStart));
			}
			catch
			{
				// Ignore clipboard failures (it can be locked by other apps)
			}
		}



		/********************************************************************/
		/// <summary>
		/// Map a client point to the nearest character index
		/// </summary>
		/********************************************************************/
		private int HitTest(Point location)
		{
			if ((layout == null) || (layout.Lines.Count == 0))
				return 0;

			int contentY = (location.Y - TextTop) + ScrollOffset;

			LayoutLine line = layout.FindLineAtY(contentY);
			if (line == null)
				return 0;

			using (Graphics g = CreateGraphics())
			{
				return layout.HitTestLine(line, location.X, g);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Scroll the view when the mouse is dragged outside the text area
		/// during a selection
		/// </summary>
		/********************************************************************/
		private void AutoScrollDuringSelection(Point location)
		{
			if (!vScrollBar.Visible)
				return;

			if (location.Y < TextTop)
				vScrollBar.Value -= vScrollBar.SmallChange;
			else if (location.Y > TextBottom)
				vScrollBar.Value += vScrollBar.SmallChange;
		}
		#endregion

		#region Drawing
		/********************************************************************/
		/// <summary>
		/// Return the default font for text that has none of its own
		/// </summary>
		/********************************************************************/
		private Font GetDefaultFont()
		{
			return fontConfiguration?.Font ?? fonts?.RegularFont;
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
		/// Draw the background
		/// </summary>
		/********************************************************************/
		private void DrawBackground(Graphics g, StateColors stateColors)
		{
			using (Brush brush = new SolidBrush(stateColors.BackgroundColor))
			{
				g.FillRectangle(brush, ClientRectangle);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the themed border
		/// </summary>
		/********************************************************************/
		private void DrawBorder(Graphics g, StateColors stateColors)
		{
			using (Pen pen = new Pen(stateColors.BorderColor, BorderWidth))
			{
				g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the visible lines and the selection
		/// </summary>
		/********************************************************************/
		private void DrawContent(Graphics g, StateColors stateColors)
		{
			int textLeft = TextLeft;
			int textTop = TextTop;
			int textRight = TextRight;
			int textBottom = TextBottom;

			Rectangle clip = Rectangle.FromLTRB(textLeft, textTop, Math.Max(textLeft, textRight), Math.Max(textTop, textBottom));

			g.SetClip(clip);

			try
			{
				int scroll = ScrollOffset;
				Font defaultFont = GetDefaultFont();

				foreach (LayoutLine line in layout.Lines)
				{
					int screenY = (textTop + line.Top) - scroll;

					if ((screenY + line.Height) <= textTop)
						continue;

					if (screenY >= textBottom)
						break;

					DrawLine(g, line, screenY, stateColors, defaultFont);
				}
			}
			finally
			{
				g.ResetClip();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw a single line including its part of the selection
		/// </summary>
		/********************************************************************/
		private void DrawLine(Graphics g, LayoutLine line, int screenY, StateColors stateColors, Font defaultFont)
		{
			// Selection background
			if (selectionEnd > selectionStart)
			{
				int a = Math.Max(selectionStart, line.FirstIndex);
				int b = Math.Min(selectionEnd, line.EndIndex);

				if (b > a)
				{
					int xA = layout.IndexToX(line, a, g);
					int xB = layout.IndexToX(line, b, g);

					// When the selection continues onto the next line, show a
					// little extra to indicate the selected line break
					if (selectionEnd > line.EndIndex)
						xB += RichTextLayout.MeasureWidth(g, " ", defaultFont);

					if (xB > xA)
					{
						using (Brush selBrush = new SolidBrush(colors.SelectedTextBackgroundColor))
						{
							g.FillRectangle(selBrush, xA, screenY, xB - xA, line.Height);
						}
					}
				}
			}

			foreach (LayoutFragment fragment in line.Fragments)
				DrawFragment(g, fragment, screenY, line.Height, stateColors);
		}



		/********************************************************************/
		/// <summary>
		/// Draw a single fragment, splitting it into selected and unselected
		/// parts so the selected part uses the themed selection color
		/// </summary>
		/********************************************************************/
		private void DrawFragment(Graphics g, LayoutFragment fragment, int screenY, int lineHeight, StateColors stateColors)
		{
			Color normalColor = fragment.Color ?? stateColors.TextColor;

			int fragmentStart = fragment.StartIndex;
			int fragmentEnd = fragmentStart + fragment.Text.Length;

			int selStart = Math.Max(selectionStart, fragmentStart);
			int selEnd = Math.Min(selectionEnd, fragmentEnd);

			if (selEnd <= selStart)
			{
				DrawClippedText(g, fragment.Text, fragment.Font, fragment.X, screenY, lineHeight, normalColor);
				return;
			}

			int x = fragment.X;

			if (selStart > fragmentStart)
			{
				string before = fragment.Text.Substring(0, selStart - fragmentStart);
				DrawClippedText(g, before, fragment.Font, x, screenY, lineHeight, normalColor);
				x += RichTextLayout.MeasureWidth(g, before, fragment.Font);
			}

			string selected = fragment.Text.Substring(selStart - fragmentStart, selEnd - selStart);
			DrawClippedText(g, selected, fragment.Font, x, screenY, lineHeight, colors.SelectedTextColor);
			x += RichTextLayout.MeasureWidth(g, selected, fragment.Font);

			if (selEnd < fragmentEnd)
			{
				string after = fragment.Text.Substring(selEnd - fragmentStart);
				DrawClippedText(g, after, fragment.Font, x, screenY, lineHeight, normalColor);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw text at (x, screenY), clipped to the text area.
		///
		/// TextRenderer.DrawText with a Point overload does not respect
		/// Graphics.Clip, so we clip using the bounds rectangle overload
		/// instead (which does clip reliably). For a line that is partly
		/// scrolled off the top we bottom-align within a rectangle whose top
		/// is the text area top, so the glyphs keep their position but the
		/// part above the top border is clipped away. Otherwise we top-align
		/// within a rectangle whose bottom is the text area bottom
		/// </summary>
		/********************************************************************/
		private void DrawClippedText(Graphics g, string text, Font font, int x, int screenY, int lineHeight, Color color)
		{
			if (string.IsNullOrEmpty(text))
				return;

			int right = TextRight;
			if (x >= right)
				return;

			TextFormatFlags flags = RichTextLayout.TextFlags;
			int top;
			int bottom;

			if (screenY < TextTop)
			{
				flags |= TextFormatFlags.Bottom;
				top = TextTop;
				bottom = screenY + lineHeight;
			}
			else
			{
				flags |= TextFormatFlags.Top;
				top = screenY;
				bottom = TextBottom;
			}

			if (bottom <= top)
				return;

			Rectangle bounds = Rectangle.FromLTRB(x, top, right, bottom);
			TextRenderer.DrawText(g, text, font, bounds, color, flags);
		}
		#endregion

		#region Designer filtering (hide properties from PropertyGrid)
		/********************************************************************/
		/// <summary>
		/// Register a provider that filters properties for the designer
		/// </summary>
		/********************************************************************/
		static NostalgicRichTextView()
		{
			TypeDescriptor.AddProvider(new NostalgicRichTextViewTypeDescriptionProvider(), typeof(NostalgicRichTextView));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer.
		/// This is needed for properties that we cannot override, but we do
		/// it for all our hidden properties to be sure
		/// </summary>
		private sealed class NostalgicRichTextViewTypeDescriptionProvider : TypeDescriptionProvider
		{
			private static readonly TypeDescriptionProvider parent = TypeDescriptor.GetProvider(typeof(Control));

			private static readonly string[] propertiesToHide =
			[
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
			public NostalgicRichTextViewTypeDescriptionProvider() : base(parent)
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
