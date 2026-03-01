/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Designer;
using Polycode.NostalgicPlayer.Controls.Events;
using Polycode.NostalgicPlayer.Controls.Native;
using Polycode.NostalgicPlayer.Controls.Theme;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Forms
{
	/// <summary>
	/// Themed form with custom rendering
	/// </summary>
	public class NostalgicForm : Form
	{
		private const int FrameCornerRadius = 8;
		private const int FrameBorderThickness = 8;
		private const int TitleBarPadding = 10;
		private const int CaptionButtonWidth = 24;
		private const int IconPosition = FrameBorderThickness + 3;

		private enum CaptionButton
		{
			None,
			Minimize,
			Maximize,
			Close
		}

		private enum CaptionButtonStatus
		{
			Hidden,
			Normal,
			Disabled,
			Hovered,
			Pressed
		}

		private struct CaptionButtonColors
		{
			public Color HoverStartColor { get; init; }
			public Color HoverStopColor { get; init; }
			public Color PressStartColor { get; init; }
			public Color PressStopColor { get; init; }
		}

		private IFormColors colors;
		private IFonts fonts;

		private int titleBarFontSize;
		private int titleBarHeight;

		private Icon smallIcon;
		private bool isActive;

		private CaptionButton hoverButton = CaptionButton.None;
		private CaptionButton pressButton = CaptionButton.None;

		#region Initialize
		/********************************************************************/
		/// <summary>
		/// Dispose different stuff
		/// </summary>
		/********************************************************************/
		protected override void Dispose(bool disposing)
		{
			CleanupTheme();

			if (smallIcon != null)
			{
				smallIcon.Dispose();
				smallIcon = null;
			}

			base.Dispose(disposing);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the form to use custom rendering
		/// </summary>
		/********************************************************************/
		protected override void OnHandleCreated(EventArgs e)
		{
			InitializeTheme();

			isActive = ActiveForm == this;

			SetFormProperties();

			CalculateTitleHeight();
			ApplyWindowRegion();
			RefreshNonClientArea();

			base.OnHandleCreated(e);
		}
		#endregion

		#region Theme
		/********************************************************************/
		/// <summary>
		/// Initialize form with the current theme
		/// </summary>
		/********************************************************************/
		private void InitializeTheme()
		{
			IThemeManager themeManager = ThemeManagerFactory.GetThemeManager();
			themeManager.ThemeChanged += ThemeChanged;

			SetupTheme(themeManager.CurrentTheme);
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup theme from the form
		/// </summary>
		/********************************************************************/
		private void CleanupTheme()
		{
			IThemeManager themeManager = ThemeManagerFactory.GetThemeManager();
			themeManager.ThemeChanged -= ThemeChanged;
		}



		/********************************************************************/
		/// <summary>
		/// Will setup the theme for the form and all its controls
		/// </summary>
		/********************************************************************/
		private void SetupTheme(ITheme theme)
		{
			colors = theme.FormColors;
			fonts = theme.StandardFonts;

			foreach (IThemeControl control in FindThemedControls(Controls))
				control.SetTheme(theme);
		}



		/********************************************************************/
		/// <summary>
		/// Return controls to set new theme on
		/// </summary>
		/********************************************************************/
		private IEnumerable<IThemeControl> FindThemedControls(Control.ControlCollection controls)
		{
			List<IThemeControl> result = new List<IThemeControl>();

			foreach (Control control in controls)
			{
				if (control is IThemeControl themedControl)
					result.Add(themedControl);

				result.AddRange(FindThemedControls(control.Controls));
			}

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the theme changes. Update all controls and redraw
		/// itself
		/// </summary>
		/********************************************************************/
		private void ThemeChanged(object sender, ThemeChangedEventArgs e)
		{
			SetupTheme(e.NewTheme);

			RefreshNonClientArea();
			Refresh();
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Make sure the form is rendered when resized
		/// </summary>
		/********************************************************************/
		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			ApplyWindowRegion();
			RefreshNonClientArea();
			Invalidate();
		}



		/********************************************************************/
		/// <summary>
		/// Process different Windows messages
		/// </summary>
		/********************************************************************/
		protected override void WndProc(ref Message m)
		{
			switch ((WM)m.Msg)
			{
				case WM.NCCALCSIZE:
				{
					if (titleBarHeight > 0)
					{
						RecalculateClientArea(m);

						m.Result = IntPtr.Zero;
						return;
					}

					break;
				}

				case WM.NCPAINT:
				{
					DrawCustomNonClient();

					m.Result = IntPtr.Zero;
					return;
				}

				case WM.NCACTIVATE:
				{
					// Prevent default non-client painting
					isActive = m.WParam != IntPtr.Zero;

					DrawCustomNonClient();

					m.Result = 1;	// TRUE: we handled activation change
					return;
				}

				case WM.SETTEXT:
				case WM.SETICON:
				{
					// Let default change the value, then redraw our non-client area
					base.WndProc(ref m);

					DrawCustomNonClient();
					return;
				}

				case WM.ACTIVATE:
				case WM.SIZE:
				case WM.SYSCOLORCHANGE:
				{
					base.WndProc(ref m);

					if ((WM)m.Msg == WM.ACTIVATE)
						isActive = m.WParam != IntPtr.Zero;

					DrawCustomNonClient();
					return;
				}

				// Suppress themed caption/frame drawing done by UxTheme
				case WM.NCUAHDRAWCAPTION:
				case WM.NCUAHDRAWFRAME:
				{
					m.Result = IntPtr.Zero;
					return;
				}

				case WM.NCMOUSEMOVE:
				{
					// From Copilot:
					//
					// • Windows doesn’t send a WM_NCMOUSEENTER. The first indication the cursor is in the
					//   non‑client area is WM_NCMOUSEMOVE.
					//
					// • You must call TrackMouseEvent(TME_LEAVE | TME_NONCLIENT) while the cursor is in
					//   NC to receive a single WM_NCMOUSELEAVE later.
					//
					// • After WM_NCMOUSELEAVE fires, tracking automatically turns off. Re‑arming it on
					//   each WM_NCMOUSEMOVE is a common, safe pattern to ensure you always get the leave,
					//   even if the system cancels tracking due to capture, modal loops, etc.
					//   The overhead is negligible.
					TrackNonClientMouseLeave();

					CaptionButton button = FindCaptionButton((HT)m.WParam);

					if (button != hoverButton)
					{
						hoverButton = button;
						DrawCustomNonClient();
					}

					return;
				}

				case WM.NCMOUSELEAVE:
				{
					if (hoverButton != CaptionButton.None)
					{
						hoverButton = CaptionButton.None;
						DrawCustomNonClient();
					}

					return;
				}

				case WM.NCHITTEST:
				{
					Point screen = GetLParamPoint(m.LParam);

					m.Result = (IntPtr)HitTestNonClient(screen);
					return;
				}

				case WM.NCLBUTTONDOWN:
				{
					HT hitTest = (HT)m.WParam;
					CaptionButton button = FindCaptionButton(hitTest);

					if (button != pressButton)
					{
						pressButton = button;
						DrawCustomNonClient();
					}

					// Let Windows handle resize operations and caption dragging
					if (IsResizeHitTest(hitTest) || (hitTest == HT.CAPTION))
						base.WndProc(ref m);

					return;
				}

				case WM.NCLBUTTONUP:
				{
					CaptionButton button = FindCaptionButton((HT)m.WParam);

					if (HandleCaptionButtonClick(button))
					{
						m.Result = IntPtr.Zero;
						return;
					}

					break;
				}
			}

			base.WndProc(ref m);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Set different properties on the form
		/// </summary>
		/********************************************************************/
		private void SetFormProperties()
		{
			BackColor = colors.FormBackgroundColor;
			AutoScaleMode = AutoScaleMode.None;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the height of the title bar
		/// </summary>
		/********************************************************************/
		private void CalculateTitleHeight()
		{
			if (!IsHandleCreated)
				return;

			using (Graphics g = CreateGraphics())
			{
				Size size = TextRenderer.MeasureText(g, "Ag", fonts.FormTitleFont, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);

				titleBarFontSize = size.Height;
				titleBarHeight = titleBarFontSize + TitleBarPadding;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will set rounded corners on the form
		/// </summary>
		/********************************************************************/
		private void ApplyWindowRegion()
		{
			if (!IsHandleCreated)
				return;

			int width = Width;
			int height = Height;

			if ((width <= 0) || (height <= 0))
				return;

			IntPtr hRgn = Gdi32.CreateRoundRectRgn(0, 0, width + 1, height + 1, FrameCornerRadius * 2, FrameCornerRadius * 2);

			// After SetWindowRgn, system owns hRgn
			User32.SetWindowRgn(Handle, hRgn, true);
		}



		/********************************************************************/
		/// <summary>
		/// Convert a lParam to a Point structure
		/// </summary>
		/********************************************************************/
		private Point GetLParamPoint(IntPtr lParam)
		{
			int lp = lParam.ToInt32();
			int x = (short)(lp & 0xffff);
			int y = (short)((lp >> 16) & 0xffff);

			return new Point(x, y);
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
		/// Will recalculate the client area of the form
		/// </summary>
		/********************************************************************/
		private void RecalculateClientArea(Message m)
		{
			// See https://learn.microsoft.com/en-us/windows/win32/winmsg/wm-nccalcsize for more information
			if (m.WParam != IntPtr.Zero)
			{
				NCCALCSIZE_PARAMS nccsp = Marshal.PtrToStructure<NCCALCSIZE_PARAMS>(m.LParam);

				nccsp.rgrc0.Left += FrameBorderThickness;
				nccsp.rgrc0.Top += FrameBorderThickness + titleBarHeight;
				nccsp.rgrc0.Right -= FrameBorderThickness;
				nccsp.rgrc0.Bottom -= FrameBorderThickness;

				Marshal.StructureToPtr(nccsp, m.LParam, false);
			}
			else
			{
				RECT rect = Marshal.PtrToStructure<RECT>(m.LParam);

				rect.Left += FrameBorderThickness;
				rect.Top += FrameBorderThickness + titleBarHeight;
				rect.Right -= FrameBorderThickness;
				rect.Bottom -= FrameBorderThickness;

				Marshal.StructureToPtr(rect, m.LParam, false);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Test if the mouse is over "action" parts of the non-client area
		/// </summary>
		/********************************************************************/
		private HT HitTestNonClient(Point screen)
		{
			User32.GetWindowRect(Handle, out RECT windowRect);

			int x = screen.X - windowRect.Left;
			int y = screen.Y - windowRect.Top;
			int width = Math.Max(0, windowRect.Right - windowRect.Left);
			int height = Math.Max(0, windowRect.Bottom - windowRect.Top);

			// Test for resize positions
			bool atTopBorder = y < FrameBorderThickness;
			bool atLeftBorder = x < FrameBorderThickness;
			bool atRightBorder = x >= (width - FrameBorderThickness);
			bool atBottomBorder = y >= (height - FrameBorderThickness);

			if (atTopBorder && atLeftBorder)
				return HT.TOPLEFT;

			if (atTopBorder && atRightBorder)
				return HT.TOPRIGHT;

			if (atBottomBorder && atLeftBorder)
				return HT.BOTTOMLEFT;

			if (atBottomBorder && atRightBorder)
				return HT.BOTTOMRIGHT;

			if (atTopBorder)
				return HT.TOP;

			if (atLeftBorder)
				return HT.LEFT;

			if (atRightBorder)
				return HT.RIGHT;

			if (atBottomBorder)
				return HT.BOTTOM;

			int titleTop = FrameBorderThickness;
			int titleBottom = titleTop + titleBarHeight;

			if ((y >= titleTop) && (y < titleBottom) && (x >= FrameBorderThickness) && (x < (width - FrameBorderThickness)))
			{
				// Test for caption buttons
				if (ControlBox)
				{
					int buttonPosition = width - FrameBorderThickness + 1;

					CaptionButtonStatus status = GetCloseCaptionButtonStatus();
					if (status != CaptionButtonStatus.Hidden)
					{
						buttonPosition -= CaptionButtonWidth;

						if ((x >= buttonPosition) && (x < (buttonPosition + CaptionButtonWidth)))
							return HT.CLOSE;
					}

					status = GetMaximizeCaptionButtonStatus();
					if (status != CaptionButtonStatus.Hidden)
					{
						buttonPosition -= CaptionButtonWidth;

						if ((x >= buttonPosition) && (x < (buttonPosition + CaptionButtonWidth)))
							return HT.MAXBUTTON;
					}

					status = GetMinimizeCaptionButtonStatus();
					if (status != CaptionButtonStatus.Hidden)
					{
						buttonPosition -= CaptionButtonWidth;

						if ((x >= buttonPosition) && (x < (buttonPosition + CaptionButtonWidth)))
							return HT.MINBUTTON;
					}
				}

				// Test for icon
				if (smallIcon != null)
				{
					int iconTop = FrameBorderThickness + ((titleBarHeight - smallIcon.Height) / 2);
					int iconLeft = IconPosition;

					if ((x >= iconLeft) && (x < (iconLeft + smallIcon.Width)) && (y >= iconTop) && (y < (iconTop + smallIcon.Height)))
						return HT.SYSMENU;
				}

				return HT.CAPTION;
			}

			return HT.NOWHERE;
		}



		/********************************************************************/
		/// <summary>
		/// Make sure we will get a WM_NCMOUSELEAVE when the mouse leaves
		/// </summary>
		/********************************************************************/
		private void TrackNonClientMouseLeave()
		{
			TRACKMOUSEEVENT tme = new TRACKMOUSEEVENT
			{
				cbSize = (uint)Marshal.SizeOf<TRACKMOUSEEVENT>(),
				dwFlags = (uint)(TME.LEAVE | TME.NONCLIENT),
				hwndTrack = Handle,
				dwHoverTime = 0
			};

			User32.TrackMouseEvent(ref tme);
		}



		/********************************************************************/
		/// <summary>
		/// Check if the hit test corresponds to a resize area
		/// </summary>
		/********************************************************************/
		private static bool IsResizeHitTest(HT hitTest)
		{
			return hitTest switch
			{
				HT.LEFT or HT.RIGHT or HT.TOP or HT.BOTTOM or
				HT.TOPLEFT or HT.TOPRIGHT or HT.BOTTOMLEFT or HT.BOTTOMRIGHT => true,
				_ => false
			};
		}



		/********************************************************************/
		/// <summary>
		/// Will check to see if the mouse is over one of the caption buttons
		/// </summary>
		/********************************************************************/
		private CaptionButton FindCaptionButton(HT hitButton)
		{
			CaptionButton button = hitButton switch
			{
				HT.MINBUTTON => CaptionButton.Minimize,
				HT.MAXBUTTON => CaptionButton.Maximize,
				HT.CLOSE => CaptionButton.Close,
				_ => CaptionButton.None
			};

			return button;
		}



		/********************************************************************/
		/// <summary>
		/// Will check to see if the mouse is over one of the caption buttons
		/// </summary>
		/********************************************************************/
		private bool HandleCaptionButtonClick(CaptionButton hitButton)
		{
			bool handled = false;

			if ((pressButton != CaptionButton.None) && (pressButton == hitButton))
			{
				switch (hitButton)
				{
					case CaptionButton.Minimize:
					{
						if (GetMinimizeCaptionButtonStatus() == CaptionButtonStatus.Pressed)
						{
							WindowState = FormWindowState.Minimized;
							handled = true;
						}

						break;
					}

					case CaptionButton.Maximize:
					{
						if (GetMaximizeCaptionButtonStatus() == CaptionButtonStatus.Pressed)
						{
							WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
							handled = true;
						}

						break;
					}

					case CaptionButton.Close:
					{
						Close();

						handled = true;
						break;
					}
				}
			}

			pressButton = CaptionButton.None;
			DrawCustomNonClient();

			return handled;
		}
		#endregion

		#region Drawing
		/********************************************************************/
		/// <summary>
		/// Will draw the title bar and border of the form
		/// </summary>
		/********************************************************************/
		private void DrawCustomNonClient()
		{
			if (!IsHandleCreated)
				return;

			IntPtr hdc = IntPtr.Zero;

			try
			{
				hdc = User32.GetWindowDC(Handle);
				if (hdc == IntPtr.Zero)
					return;

				// Window and client geometry
				User32.GetClientRect(Handle, out RECT clientRect);
				User32.MapWindowPoints(Handle, IntPtr.Zero, ref clientRect, 2);

				int clientRight = clientRect.Right - clientRect.Left;
				int clientBottom = clientRect.Bottom - clientRect.Top;

				int windowWidth = clientRight + FrameBorderThickness * 2 - 1;
				int windowHeight = clientBottom + FrameBorderThickness * 2 + titleBarHeight - 1;

				int innerBorderThickness = FrameBorderThickness - 2;
				int totalTitleBarHeight = innerBorderThickness + titleBarHeight + 1;

				using (Graphics gWin = Graphics.FromHdc(hdc))
				{
					// Draw full border to the window DC
					DrawBorder(gWin, windowWidth, windowHeight, innerBorderThickness);

					// Double buffer the title bar area to avoid flicking when mouse
					// is moving around on the title bar
					Rectangle titleRect = new Rectangle(0, 0, windowWidth, totalTitleBarHeight + 1);
					BufferedGraphicsContext ctx = BufferedGraphicsManager.Current;

					using (BufferedGraphics buffer = ctx.Allocate(gWin, titleRect))
					{
						Graphics g = buffer.Graphics;

						g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

						g.TranslateTransform(-titleRect.X, -titleRect.Y);       // Draw using window coordinates

						// Redraw the border, but only the parts that intersects the title bar
						GraphicsState state = g.Save();

						g.SetClip(titleRect);
						DrawBorder(g, windowWidth, windowHeight, innerBorderThickness);

						g.Restore(state);

						// Now draw the title bar
						DrawTitleBar(g, windowWidth, totalTitleBarHeight);

						// And finally, render it to the window
						buffer.Render(gWin);
					}
				}
			}
			catch
			{
				// Ignore any exceptions
			}
			finally
			{
				if (hdc != IntPtr.Zero)
					User32.ReleaseDC(Handle, hdc);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will draw the border around the window
		/// </summary>
		/********************************************************************/
		private void DrawBorder(Graphics g, int windowWidth, int windowHeight, int innerBorderThickness)
		{
			Color outerColor = isActive ? colors.ActivatedFormOuterColor : colors.DeactivatedFormOuterColor;
			Color middleColor = isActive ? colors.ActivatedFormMiddleColor : colors.DeactivatedFormMiddleColor;
			Color innerStartColor = isActive ? colors.ActivatedFormInnerStartColor : colors.DeactivatedFormInnerStartColor;
			Color innerStopColor = isActive ? colors.ActivatedFormInnerStopColor : colors.DeactivatedFormInnerStopColor;

			using (Pen pen = new Pen(outerColor, 1))
			{
				g.DrawRectangle(pen, 0, 0, windowWidth, windowHeight);
			}

			using (Pen pen = new Pen(middleColor, 1))
			{
				g.DrawRectangle(pen, 1, 1, windowWidth - 2, windowHeight - 2);
			}

			using (Brush startBrush = new SolidBrush(innerStartColor))
			{
				using (Brush stopBrush = new SolidBrush(innerStopColor))
				{
					g.FillRectangle(startBrush, 2, 2, windowWidth - 3, innerBorderThickness);
					g.FillRectangle(stopBrush, windowWidth - FrameBorderThickness + 1, FrameBorderThickness, innerBorderThickness, windowHeight - FrameBorderThickness - 1);
					g.FillRectangle(stopBrush, 2, windowHeight - FrameBorderThickness + 1, windowWidth - 3, innerBorderThickness);
					g.FillRectangle(stopBrush, 2, FrameBorderThickness, innerBorderThickness, windowHeight - FrameBorderThickness - 1);
				}
			}

			// Corner-only anti-aliased arcs to “dither” the rounded corners, leaving straight edges unchanged
			DrawCornerOutlineArcs(g, windowWidth, windowHeight, outerColor, middleColor);
		}



		/********************************************************************/
		/// <summary>
		/// Draws the corner arcs for the window
		/// </summary>
		/********************************************************************/
		private void DrawCornerOutlineArcs(Graphics g, int windowWidth, int windowHeight, Color outer, Color middle)
		{
			SmoothingMode oldSmooth = g.SmoothingMode;
			PixelOffsetMode oldPixel = g.PixelOffsetMode;

			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.PixelOffsetMode = PixelOffsetMode.Half;	// Crisper 1px arcs

			using (Pen penOuter = new Pen(outer, 1))
			{
				using (Pen penMiddle = new Pen(middle, 1))
				{
					// Outer-most 1px outline follows the window region radius
					int rOuter = FrameCornerRadius;
					DrawCornerArc(g, penOuter, windowWidth, windowHeight, rOuter, 0);

					// Second 1px outline is inset by 1px
					int rMiddle = Math.Max(0, FrameCornerRadius - 1);
					DrawCornerArc(g, penMiddle, windowWidth, windowHeight, rMiddle, 1);
				}
			}

			g.SmoothingMode = oldSmooth;
			g.PixelOffsetMode = oldPixel;
		}



		/********************************************************************/
		/// <summary>
		/// Draw a single color for the corner arcs
		/// </summary>
		/********************************************************************/
		private static void DrawCornerArc(Graphics g, Pen pen, int windowWidth, int windowHeight, int radius, int inset)
		{
			if (radius <= 0)
				return;

			// Top-left
			Rectangle tl = new Rectangle(inset, inset, radius * 2, radius * 2);
			g.DrawArc(pen, tl, 180f, 90f);

			// Top-right
			Rectangle tr = new Rectangle(windowWidth - (inset + radius * 2), inset, radius * 2, radius * 2);
			g.DrawArc(pen, tr, 270f, 90f);

			// Bottom-right
			Rectangle br = new Rectangle(windowWidth - (inset + radius * 2), windowHeight - (inset + radius * 2), radius * 2, radius * 2);
			g.DrawArc(pen, br, 0f, 90f);

			// Bottom-left
			Rectangle bl = new Rectangle(inset, windowHeight - (inset + radius * 2), radius * 2, radius * 2);
			g.DrawArc(pen, bl, 90f, 90f);
		}



		/********************************************************************/
		/// <summary>
		/// Will draw the whole title bar
		/// </summary>
		/********************************************************************/
		private void DrawTitleBar(Graphics g, int windowWidth, int totalTitleBarHeight)
		{
			DrawTitleBarBackground(g, windowWidth);

			int left = DrawIcon(g);
			int right = DrawCaptionButtons(g, windowWidth, totalTitleBarHeight);
			DrawTitle(g, left, right);
		}



		/********************************************************************/
		/// <summary>
		/// Will draw the title bar background
		/// </summary>
		/********************************************************************/
		private void DrawTitleBarBackground(Graphics g, int windowWidth)
		{
			Color startColor = isActive ? colors.ActivatedFormInnerStartColor : colors.DeactivatedFormInnerStartColor;
			Color stopColor = isActive ? colors.ActivatedFormInnerStopColor : colors.DeactivatedFormInnerStopColor;

			using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, windowWidth, FrameBorderThickness + titleBarHeight), startColor, stopColor, LinearGradientMode.Vertical))
			{
				g.FillRectangle(brush, 2, FrameBorderThickness, windowWidth - 3, titleBarHeight);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will draw the title bar icon
		/// </summary>
		/********************************************************************/
		private int DrawIcon(Graphics g)
		{
			int left = IconPosition;

			if (ShowIcon && (Icon != null))
			{
				if (smallIcon == null)
					smallIcon = new Icon(Icon, SystemInformation.SmallIconSize);

				int top = FrameBorderThickness + ((titleBarHeight - smallIcon.Height) / 2);

				g.DrawIcon(smallIcon, left, top);

				left += smallIcon.Width;
			}

			return left;
		}



		/********************************************************************/
		/// <summary>
		/// Will draw the title text
		/// </summary>
		/********************************************************************/
		private void DrawTitle(Graphics g, int left, int right)
		{
			Rectangle rect = new Rectangle(left, FrameBorderThickness + ((titleBarHeight - titleBarFontSize) / 2) - 1, right - left, titleBarFontSize);

			Color titleColor = isActive ? colors.ActivatedWindowTitleColor : colors.DeactivatedWindowTitleColor;

			using (Brush brush = new SolidBrush(titleColor))
			{
				using (StringFormat sf = new StringFormat())
				{
					sf.LineAlignment = StringAlignment.Center;
					sf.Trimming = StringTrimming.EllipsisCharacter;

					g.DrawString(Text, fonts.FormTitleFont, brush, rect, sf);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will draw all the caption buttons
		/// </summary>
		/********************************************************************/
		private int DrawCaptionButtons(Graphics g, int windowWidth, int totalTitleBarHeight)
		{
			int right = windowWidth - FrameBorderThickness + 1;

			if (ControlBox)
			{
				CaptionButtonStatus status = GetCloseCaptionButtonStatus();
				if (status != CaptionButtonStatus.Hidden)
				{
					right -= CaptionButtonWidth;
					DrawSingleCaptionButton(g, Resources.IDB_CAPTION_CLOSE, status, right, totalTitleBarHeight, GetCloseCaptionButtonColors());
				}

				CaptionButtonColors captionColors = GetCaptionButtonColors();

				status = GetMaximizeCaptionButtonStatus();
				if (status != CaptionButtonStatus.Hidden)
				{
					right -= CaptionButtonWidth;
					DrawSingleCaptionButton(g, WindowState == FormWindowState.Maximized ? Resources.IDB_CAPTION_NORMALIZE : Resources.IDB_CAPTION_MAXIMIZE, status, right, totalTitleBarHeight, captionColors);
				}

				status = GetMinimizeCaptionButtonStatus();
				if (status != CaptionButtonStatus.Hidden)
				{
					right -= CaptionButtonWidth;
					DrawSingleCaptionButton(g, Resources.IDB_CAPTION_MINIMIZE, status, right, totalTitleBarHeight, captionColors);
				}
			}

			return right;
		}



		/********************************************************************/
		/// <summary>
		/// Will draw a single caption button
		/// </summary>
		/********************************************************************/
		private void DrawSingleCaptionButton(Graphics g, Bitmap bitmap, CaptionButtonStatus status, int x, int totalTitleBarHeight, CaptionButtonColors captionColors)
		{
			switch (status)
			{
				case CaptionButtonStatus.Hidden:
				default:
					break;

				case CaptionButtonStatus.Normal:
				{
					DrawCaptionButtonImage(g, bitmap, x, totalTitleBarHeight);
					break;
				}

				case CaptionButtonStatus.Disabled:
				{
					DrawCaptionButtonImageDisabled(g, bitmap, x, totalTitleBarHeight);
					break;
				}

				case CaptionButtonStatus.Hovered:
				{
					DrawCaptionButtonBackground(g, x, totalTitleBarHeight, captionColors.HoverStartColor, captionColors.HoverStopColor);
					DrawCaptionButtonImage(g, bitmap, x, totalTitleBarHeight);
					break;
				}

				case CaptionButtonStatus.Pressed:
				{
					DrawCaptionButtonBackground(g, x, totalTitleBarHeight, captionColors.PressStartColor, captionColors.PressStopColor);
					DrawCaptionButtonImage(g, bitmap, x, totalTitleBarHeight);
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the background of a caption button
		/// </summary>
		/********************************************************************/
		private void DrawCaptionButtonBackground(Graphics g, int x, int totalTitleBarHeight, Color startColor, Color stopColor)
		{
			using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, CaptionButtonWidth, FrameBorderThickness + titleBarHeight), startColor, stopColor, LinearGradientMode.Vertical))
			{
				g.FillRectangle(brush, x, 2, CaptionButtonWidth, totalTitleBarHeight - 2);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the caption button image
		/// </summary>
		/********************************************************************/
		private void DrawCaptionButtonImage(Graphics g, Bitmap bitmap, int x, int totalTitleBarHeight)
		{
			int y = (totalTitleBarHeight - bitmap.Height) / 2;

			if (WindowState == FormWindowState.Maximized)
				y += 3;

			g.DrawImage(bitmap, x + ((CaptionButtonWidth - bitmap.Width) / 2), y);
		}



		/********************************************************************/
		/// <summary>
		/// Draw the caption button image as disabled
		/// </summary>
		/********************************************************************/
		private void DrawCaptionButtonImageDisabled(Graphics g, Bitmap bitmap, int x, int totalTitleBarHeight)
		{
			int imgX = x + ((CaptionButtonWidth - bitmap.Width) / 2);
			int imgY = (totalTitleBarHeight - bitmap.Height) / 2;

			if (WindowState == FormWindowState.Maximized)
				imgY += 3;

			// sRGB luminance weights + reduced alpha
			const float rw = 0.2126f;
			const float gw = 0.7152f;
			const float bw = 0.0722f;
			const float alpha = 0.40f;	// Tweak to taste

			ColorMatrix cm = new ColorMatrix(
			[
				[ rw, rw, rw, 0, 0 ],
				[ gw, gw, gw, 0, 0 ],
				[ bw, bw, bw, 0, 0 ],
				[  0,  0,  0, alpha, 0 ],
				[  0,  0,  0, 0, 1 ]
			]);

			using (ImageAttributes ia = new ImageAttributes())
			{
				ia.SetColorMatrix(cm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

				Rectangle dest = new Rectangle(imgX, imgY, bitmap.Width, bitmap.Height);
				g.DrawImage(bitmap, dest, 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, ia);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the status for the close button
		/// </summary>
		/********************************************************************/
		private CaptionButtonStatus GetCloseCaptionButtonStatus()
		{
			if (pressButton == CaptionButton.Close)
				return CaptionButtonStatus.Pressed;

			if (hoverButton == CaptionButton.Close)
				return CaptionButtonStatus.Hovered;

			return CaptionButtonStatus.Normal;
		}



		/********************************************************************/
		/// <summary>
		/// Return the status for the maximize button
		/// </summary>
		/********************************************************************/
		private CaptionButtonStatus GetMaximizeCaptionButtonStatus()
		{
			if (MaximizeBox)
			{
				if (pressButton == CaptionButton.Maximize)
					return CaptionButtonStatus.Pressed;

				if (hoverButton == CaptionButton.Maximize)
					return CaptionButtonStatus.Hovered;

				return CaptionButtonStatus.Normal;
			}

			return MinimizeBox ? CaptionButtonStatus.Disabled : CaptionButtonStatus.Hidden;
		}



		/********************************************************************/
		/// <summary>
		/// Return the status for the minimize button
		/// </summary>
		/********************************************************************/
		private CaptionButtonStatus GetMinimizeCaptionButtonStatus()
		{
			if (MinimizeBox)
			{
				if (pressButton == CaptionButton.Minimize)
					return CaptionButtonStatus.Pressed;

				if (hoverButton == CaptionButton.Minimize)
					return CaptionButtonStatus.Hovered;

				return CaptionButtonStatus.Normal;
			}

			return MaximizeBox ? CaptionButtonStatus.Disabled : CaptionButtonStatus.Hidden;
		}



		/********************************************************************/
		/// <summary>
		/// Return the colors for the close button
		/// </summary>
		/********************************************************************/
		private CaptionButtonColors GetCloseCaptionButtonColors()
		{
			return new CaptionButtonColors
			{
				HoverStartColor = colors.CloseCaptionButtonHoverStartColor,
				HoverStopColor = colors.CloseCaptionButtonHoverStopColor,
				PressStartColor = colors.CloseCaptionButtonPressStartColor,
				PressStopColor = colors.CloseCaptionButtonPressStopColor
			};
		}



		/********************************************************************/
		/// <summary>
		/// Return the colors for the normal caption buttons
		/// </summary>
		/********************************************************************/
		private CaptionButtonColors GetCaptionButtonColors()
		{
			return new CaptionButtonColors
			{
				HoverStartColor = colors.CaptionButtonHoverStartColor,
				HoverStopColor = colors.CaptionButtonHoverStopColor,
				PressStartColor = colors.CaptionButtonPressStartColor,
				PressStopColor = colors.CaptionButtonPressStopColor
			};
		}
		#endregion

		#region Designer filtering (hide properties from PropertyGrid)
		/********************************************************************/
		/// <summary>
		/// Register a provider that filters properties for the designer
		/// </summary>
		/********************************************************************/
		static NostalgicForm()
		{
			TypeDescriptor.AddProvider(new NostalgicFormTypeDescriptionProvider(), typeof(NostalgicForm));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer.
		/// This is needed for properties that we cannot override, but we do
		/// it for all our hidden properties to be sure
		/// </summary>
		private sealed class NostalgicFormTypeDescriptionProvider : TypeDescriptionProvider
		{
			private static readonly TypeDescriptionProvider parent = TypeDescriptor.GetProvider(typeof(Form));

			private static readonly string[] propertiesToHide =
			[
				nameof(AutoScaleDimensions),
				nameof(AutoScaleMode),
				nameof(BackColor),
				nameof(Font),
				nameof(ForeColor)
			];

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public NostalgicFormTypeDescriptionProvider() : base(parent)
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
