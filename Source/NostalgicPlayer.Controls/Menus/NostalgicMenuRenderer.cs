/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Components;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Menus
{
	/// <summary>
	/// Custom renderer used to paint a menu strip and its
	/// drop-down panels using the colors of the current theme
	/// </summary>
	internal sealed class NostalgicMenuRenderer : ToolStripRenderer
	{
		private struct MenuBarStateColors
		{
			public Color BorderColor { get; init; }
			public Color BackgroundStartColor { get; init; }
			public Color BackgroundStopColor { get; init; }
			public Color TextColor { get; init; }
		}

		private struct DropDownStateColors
		{
			public Color BackgroundStartColor { get; init; }
			public Color BackgroundMiddleColor { get; init; }
			public Color BackgroundStopColor { get; init; }
			public Color TextColor { get; init; }
		}

		private IMenuStripColors colors;
		private IFonts fonts;
		private Font fontToUse;

		private int imageMarginRightEdge;
		private int textStartXInDropdown;

		/********************************************************************/
		/// <summary>
		/// Set the colors to use for rendering
		/// </summary>
		/********************************************************************/
		public void SetTheme(ITheme theme)
		{
			colors = theme.MenuStripColors;
			fonts = theme.StandardFonts;
		}



		/********************************************************************/
		/// <summary>
		/// Set font to use
		/// </summary>
		/********************************************************************/
		public void UpdateFont(FontConfiguration fontConfiguration)
		{
			fontToUse = fontConfiguration?.Font ?? fonts.RegularFont;
		}

		#region Drawing
		/********************************************************************/
		/// <summary>
		/// Return the colors to use for the current state
		/// </summary>
		/********************************************************************/
		private MenuBarStateColors GetMenuBarColors(ToolStripItem item)
		{
			if (item.Pressed)
			{
				return new MenuBarStateColors
				{
					BorderColor = colors.OpenMenuBarItemBorderColor,
					BackgroundStartColor = colors.OpenMenuBarItemBackgroundStartColor,
					BackgroundStopColor = colors.OpenMenuBarItemBackgroundStopColor,
					TextColor = colors.OpenMenuBarItemTextColor
				};
			}

			if (item.Selected)
			{
				return new MenuBarStateColors
				{
					BorderColor = colors.HoverMenuBarItemBorderColor,
					BackgroundStartColor = colors.HoverMenuBarItemBackgroundStartColor,
					BackgroundStopColor = colors.HoverMenuBarItemBackgroundStopColor,
					TextColor = colors.HoverMenuBarItemTextColor
				};
			}

			return new MenuBarStateColors
			{
				BorderColor = colors.NormalMenuBarItemBorderColor,
				BackgroundStartColor = colors.NormalMenuBarItemBackgroundStartColor,
				BackgroundStopColor = colors.NormalMenuBarItemBackgroundStopColor,
				TextColor = colors.NormalMenuBarItemTextColor
			};
		}



		/********************************************************************/
		/// <summary>
		/// Return the colors to use for the current state
		/// </summary>
		/********************************************************************/
		private DropDownStateColors GetDropDownColors(ToolStripItem item)
		{
			if (!item.Enabled)
			{
				return new DropDownStateColors
				{
					BackgroundStartColor = colors.DisabledDropDownItemBackgroundStartColor,
					BackgroundMiddleColor = colors.DisabledDropDownItemBackgroundMiddleColor,
					BackgroundStopColor = colors.DisabledDropDownItemBackgroundStopColor,
					TextColor = colors.DisabledDropDownItemTextColor
				};
			}

			if (item.Selected)
			{
				return new DropDownStateColors
				{
					BackgroundStartColor = colors.HoverDropDownItemBackgroundStartColor,
					BackgroundMiddleColor = colors.HoverDropDownItemBackgroundMiddleColor,
					BackgroundStopColor = colors.HoverDropDownItemBackgroundStopColor,
					TextColor = colors.HoverDropDownItemTextColor
				};
			}

			return new DropDownStateColors
			{
				BackgroundStartColor = colors.NormalDropDownItemBackgroundStartColor,
				BackgroundMiddleColor = colors.NormalDropDownItemBackgroundMiddleColor,
				BackgroundStopColor = colors.NormalDropDownItemBackgroundStopColor,
				TextColor = colors.NormalDropDownItemTextColor
			};
		}



		/********************************************************************/
		/// <summary>
		/// Paint the background of the strip
		/// </summary>
		/********************************************************************/
		protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
		{
			imageMarginRightEdge = 0;

			Graphics g = e.Graphics;
			Rectangle rect = e.AffectedBounds;

			if (e.ToolStrip is NostalgicMenuBar)
				DrawMenuBarBackground(g, rect);
			else if (e.ToolStrip is ToolStripDropDown)
				DrawDropDownBackground(g, rect);
		}



		/********************************************************************/
		/// <summary>
		/// Paint the border around the strip
		/// </summary>
		/********************************************************************/
		protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
		{
			Graphics g = e.Graphics;

			if (e.ToolStrip is ToolStripDropDown dropDown)
			{
				Rectangle rect = e.ToolStrip.ClientRectangle;
				Rectangle outerRect = new Rectangle(rect.X, rect.Y, rect.Width - 1, rect.Height - 1);

				using (Pen pen = new Pen(colors.DropDownBorderColor))
				{
					ToolStripItem ownerItem = dropDown.OwnerItem;

					if ((ownerItem != null) && !ownerItem.IsOnDropDown)
					{
						g.DrawLine(pen, outerRect.Left, outerRect.Top, outerRect.Left, outerRect.Bottom);
						g.DrawLine(pen, outerRect.Right, outerRect.Top, outerRect.Right, outerRect.Bottom);
						g.DrawLine(pen, outerRect.Left, outerRect.Bottom, outerRect.Right, outerRect.Bottom);
						g.DrawLine(pen, outerRect.Left + ownerItem.Width - 1, outerRect.Top, outerRect.Right, outerRect.Top);
					}
					else
					{
						g.DrawRectangle(pen, outerRect);
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Paint the image margin (the column on the left of drop-down items
		/// where icons are shown)
		/// </summary>
		/********************************************************************/
		protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
		{
			// Just remember the right edge, so it can be used
			// when drawing the separator later on
			Rectangle rect = e.AffectedBounds;
			imageMarginRightEdge = rect.Right;
		}



		/********************************************************************/
		/// <summary>
		/// Paint the background of a menu item. It is both top-level and
		/// drop-down items
		/// </summary>
		/********************************************************************/
		protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
		{
			ToolStripItem item = e.Item;
			Graphics g = e.Graphics;
			Rectangle rect = new Rectangle(Point.Empty, item.Size);

			if (item.IsOnDropDown)
			{
				DropDownStateColors stateColors = GetDropDownColors(item);
				DrawDropDownItemBackground(g, rect, stateColors);
				DrawDropDownImageMarginSeparator(g, rect, item);
			}
			else
			{
				MenuBarStateColors stateColors = GetMenuBarColors(item);
				DrawMenuBarItemBackground(g, rect, stateColors);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set the text color from the current theme before letting the
		/// base class draw the text
		/// </summary>
		/********************************************************************/
		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			ToolStripItem item = e.Item;

			e.TextFont = fontToUse;

			if (item.IsOnDropDown)
			{
				DropDownStateColors stateColors = GetDropDownColors(item);
				e.TextColor = stateColors.TextColor;

				textStartXInDropdown = item.Bounds.X + e.TextRectangle.Left;
			}
			else
			{
				MenuBarStateColors stateColors = GetMenuBarColors(item);
				e.TextColor = stateColors.TextColor;
			}

			base.OnRenderItemText(e);
		}



		/********************************************************************/
		/// <summary>
		/// Draw a horizontal line for separator items
		/// </summary>
		/********************************************************************/
		protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
		{
			Graphics g = e.Graphics;
			ToolStripItem item = e.Item;

			Rectangle rect = new Rectangle(Point.Empty, item.Size);

			int leftX = rect.Left + 4;

			if (item.IsOnDropDown)
			{
				if (textStartXInDropdown > 0)
					leftX = textStartXInDropdown - item.Bounds.X;
				else if (imageMarginRightEdge > 0)
					leftX = (imageMarginRightEdge - item.Bounds.X) + 8;
			}

			using (Pen pen = new Pen(colors.DropDownSeparatorColor))
			{
				int y = rect.Top + (rect.Height / 2);
				g.DrawLine(pen, leftX, y, rect.Right - 4, y);
			}

			DrawDropDownImageMarginSeparator(g, rect, item);
		}



		/********************************************************************/
		/// <summary>
		/// Set the arrow color from the current theme
		/// </summary>
		/********************************************************************/
		protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
		{
			DropDownStateColors stateColors = GetDropDownColors(e.Item);
			e.ArrowColor = stateColors.TextColor;

			base.OnRenderArrow(e);
		}



		/********************************************************************/
		/// <summary>
		/// Draw a check mark for items in checked state
		/// </summary>
		/********************************************************************/
		protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
		{
			Graphics g = e.Graphics;
			Rectangle rect = e.ImageRectangle;

			SmoothingMode prev = g.SmoothingMode;
			g.SmoothingMode = SmoothingMode.AntiAlias;

			DropDownStateColors stateColors = GetDropDownColors(e.Item);

			using (Pen pen = new Pen(stateColors.TextColor, 2f))
			{
				Point[] points =
				[
					new Point(rect.Left + 3, rect.Top + (rect.Height / 2)),
					new Point(rect.Left + (rect.Width / 2), rect.Bottom - 3),
					new Point(rect.Right - 2, rect.Top + 2)
				];

				g.DrawLines(pen, points);
			}

			g.SmoothingMode = prev;
		}
		#endregion

		#region Private drawing helpers
		/********************************************************************/
		/// <summary>
		/// Draw the menu bar background
		/// </summary>
		/********************************************************************/
		private void DrawMenuBarBackground(Graphics g, Rectangle rect)
		{
			using (SolidBrush brush = new SolidBrush(colors.MenuBarBackgroundColor))
			{
				g.FillRectangle(brush, rect);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the drop-down background
		/// </summary>
		/********************************************************************/
		private void DrawDropDownBackground(Graphics g, Rectangle rect)
		{
			using (SolidBrush brush = new SolidBrush(colors.NormalDropDownItemBackgroundStartColor))
			{
				g.FillRectangle(brush, rect);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the background and border of a top-level item
		/// </summary>
		/********************************************************************/
		private void DrawMenuBarItemBackground(Graphics g, Rectangle rect, MenuBarStateColors stateColors)
		{
			Rectangle outerRect = new Rectangle(rect.X, rect.Y, rect.Width - 1, rect.Height - 1);

			using (LinearGradientBrush brush = new LinearGradientBrush(outerRect, stateColors.BackgroundStartColor, stateColors.BackgroundStopColor, LinearGradientMode.Vertical))
			{
				g.FillRectangle(brush, outerRect);
			}

			using (Pen pen = new Pen(stateColors.BorderColor))
			{
				g.DrawRectangle(pen, outerRect);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the background of a drop-down item
		/// </summary>
		/********************************************************************/
		private void DrawDropDownItemBackground(Graphics g, Rectangle rect, DropDownStateColors stateColors)
		{
			SmoothingMode prev = g.SmoothingMode;
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

			g.SmoothingMode = prev;
		}



		/********************************************************************/
		/// <summary>
		/// Draw the image margin separator line
		/// </summary>
		/********************************************************************/
		private void DrawDropDownImageMarginSeparator(Graphics g, Rectangle rect, ToolStripItem item)
		{
			if ((imageMarginRightEdge <= 0) || item.Selected)
				return;

			int x = imageMarginRightEdge - item.Bounds.X - 1;
			if ((x < 0) || (x >= rect.Width))
				return;

			using (Pen pen = new Pen(colors.DropDownSeparatorColor))
			{
				g.DrawLine(pen, x, rect.Top, x, rect.Bottom - 1);
			}
		}
		#endregion
	}
}
