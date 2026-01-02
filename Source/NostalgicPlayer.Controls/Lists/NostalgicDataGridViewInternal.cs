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
using Polycode.NostalgicPlayer.Controls.Theme;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Lists
{
	/// <summary>
	/// Custom DataGridView with theme support and custom rendering
	/// </summary>
	internal class NostalgicDataGridViewInternal : DataGridView, IThemeControl
	{
		private const int GlyphWidthSize = 8;
		private const int GlyphHeightSize = 5;

		private IDataGridViewColors colors;
		private IFonts fonts;

		private FontConfiguration fontConfiguration;

		private int pressedHeaderColumnIndex = -1;

		private Panel corner;
		private NostalgicVScrollBar vScrollBar;
		private NostalgicHScrollBar hScrollBar;
		private NostalgicDataGridView parentControl;

		private bool updatingScrollBars;

		private struct HeaderStateColors
		{
			public Color BorderColor { get; init; }
			public Color BackgroundStartColor { get; init; }
			public Color BackgroundStopColor { get; init; }
			public Color TextColor { get; init; }
		}

		private struct CellStateColors
		{
			public Color BackgroundStartColor { get; init; }
			public Color BackgroundMiddleColor { get; init; }
			public Color BackgroundStopColor { get; init; }
			public Color TextColor { get; init; }
		}

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicDataGridViewInternal()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

			BorderStyle = BorderStyle.None;
			CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
			ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
			EnableHeadersVisualStyles = false;
			RowHeadersVisible = false;
			ShowCellErrors = false;
			ShowEditingIcon = false;
			ShowRowErrors = false;
			AllowUserToAddRows = false;
			AllowUserToDeleteRows = false;
			AllowUserToResizeRows = false;
			ReadOnly = true;
			SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			ScrollBars = ScrollBars.None;
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

		#region Public methods
		/********************************************************************/
		/// <summary>
		/// Set the owner of this control
		/// </summary>
		/********************************************************************/
		public void SetControls(NostalgicVScrollBar nostalgicVScrollBar, NostalgicHScrollBar nostalgicHScrollBar, Panel cornerPanel, NostalgicDataGridView parent)
		{
			vScrollBar = nostalgicVScrollBar;
			hScrollBar = nostalgicHScrollBar;
			corner = cornerPanel;
			parentControl = parent;

			parentControl.Resize += ParentControl_Resize;

			InitializeScrollBars();
			UpdateScrollBars();
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
			if (DesignMode || (parentControl?.IsInDesignMode == true))
				SetTheme(ThemeManagerFactory.GetThemeManager().CurrentTheme);

			UpdateFont();

			AutoResizeRows();

			UpdateScrollBars();

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
			colors = theme.DataGridViewColors;
			fonts = theme.StandardFonts;

			UpdateFont();

			Invalidate();
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void OnCellMouseDown(DataGridViewCellMouseEventArgs e)
		{
			if ((e.RowIndex == -1) && (e.ColumnIndex >= 0))
			{
				pressedHeaderColumnIndex = e.ColumnIndex;
				Invalidate(GetColumnDisplayRectangle(e.ColumnIndex, false));
			}

			if ((e.RowIndex == -1) || (e.ColumnIndex == -1))
			{
				// Need to turn off double buffering, when clicking on a row or column
				// header, else the drag'n'drop rectangle or resizing won't be painted
				DoubleBuffered = false;
			}

			base.OnCellMouseDown(e);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (!DoubleBuffered)
					DoubleBuffered = true;

				if (pressedHeaderColumnIndex >= 0)
				{
					int oldIndex = pressedHeaderColumnIndex;
					pressedHeaderColumnIndex = -1;
					Invalidate(GetColumnDisplayRectangle(oldIndex, false));
				}
			}

			base.OnMouseUp(e);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void PaintBackground(Graphics graphics, Rectangle clipBounds, Rectangle gridBounds)
		{
			using (SolidBrush brush = new SolidBrush(colors.BackgroundColor))
			{
				graphics.FillRectangle(brush, gridBounds);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
		{
			Graphics g = e.Graphics;

			if ((e.RowIndex >= 0) && (e.ColumnIndex >= 0))
			{
				DrawColumnCell(g, e.CellBounds, GetCellColors(e.State), e.CellStyle?.Alignment ?? DataGridViewContentAlignment.TopLeft, e.Value);
				e.Handled = true;
			}
			else if ((e.RowIndex == -1) && (e.ColumnIndex >= 0))
			{
				DrawColumnHeader(g, e.ColumnIndex, e.CellBounds, GetHeaderColors(e.ColumnIndex), e.Value);
				e.Handled = true;
			}

			base.OnCellPainting(e);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void OnScroll(ScrollEventArgs e)
		{
			UpdateScrollBars();

			base.OnScroll(e);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void OnResize(EventArgs e)
		{
			UpdateScrollBars();

			base.OnResize(e);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void OnRowsAdded(DataGridViewRowsAddedEventArgs e)
		{
			UpdateScrollBars();

			base.OnRowsAdded(e);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void OnRowsRemoved(DataGridViewRowsRemovedEventArgs e)
		{
			UpdateScrollBars();

			base.OnRowsRemoved(e);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void OnColumnAdded(DataGridViewColumnEventArgs e)
		{
			UpdateScrollBars();

			base.OnColumnAdded(e);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void OnColumnRemoved(DataGridViewColumnEventArgs e)
		{
			UpdateScrollBars();

			base.OnColumnRemoved(e);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			if (vScrollBar.Visible)
				vScrollBar.CalculateValueForMouseWheel(e);

			base.OnMouseWheel(e);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void OnColumnWidthChanged(DataGridViewColumnEventArgs e)
		{
			base.OnColumnWidthChanged(e);

			UpdateScrollBars();
		}
		#endregion

		#region Handlers
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ParentControl_Resize(object sender, EventArgs e)
		{
			UpdateScrollBars();
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
		/// Initialize custom scrollbars
		/// </summary>
		/********************************************************************/
		private void InitializeScrollBars()
		{
			vScrollBar.ValueChanged += VScrollBar_ValueChanged;
			hScrollBar.ValueChanged += HScrollBar_ValueChanged;

			vScrollBar.Visible = false;
			hScrollBar.Visible = false;
		}



		/********************************************************************/
		/// <summary>
		/// Fix sizes and positions on the controls
		/// </summary>
		/********************************************************************/
		private void FixPositionAndSizes()
		{
			int vScrollBarWidth = vScrollBar.Width;
			int hScrollBarHeight = hScrollBar.Height;

			int newWidth = parentControl.Width - (vScrollBar.Visible ? vScrollBarWidth : 0);
			int newHeight = parentControl.Height - (hScrollBar.Visible ? hScrollBarHeight : 0);

			SetBounds(0, 0, newWidth, newHeight);

			vScrollBar.SetBounds(vScrollBar.Left, vScrollBar.Top, vScrollBarWidth, newHeight);
			hScrollBar.SetBounds(hScrollBar.Left, hScrollBar.Top, newWidth, hScrollBarHeight);

			corner.Visible = vScrollBar.Visible && hScrollBar.Visible;
		}



		/********************************************************************/
		/// <summary>
		/// Update scrollbar values based on DataGridView state
		/// </summary>
		/********************************************************************/
		private void UpdateScrollBars()
		{
			if (updatingScrollBars)
				return;

			if ((vScrollBar == null) || (hScrollBar == null))
				return;

			updatingScrollBars = true;

			try
			{
				// Iteratively calculate scrollbar visibility until it stabilizes
				// This handles the chicken-and-egg problem where each scrollbar's
				// visibility affects the other's calculation
				bool previousVScrollBarVisible, previousHScrollBarVisible;
				int iterations = 3;

				do
				{
					previousVScrollBarVisible = vScrollBar.Visible;
					previousHScrollBarVisible = hScrollBar.Visible;

					UpdateHScrollBar();
					UpdateVScrollBar();

					iterations--;
				}
				while ((previousVScrollBarVisible != vScrollBar.Visible || previousHScrollBarVisible != hScrollBar.Visible) && (iterations >= 0));

				FixPositionAndSizes();
			}
			finally
			{
				updatingScrollBars = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Update vertical scrollbar values based on DataGridView state
		/// </summary>
		/********************************************************************/
		private void UpdateVScrollBar()
		{
			int visibleRowCount = 0;

			foreach (DataGridViewRow row in Rows)
			{
				if (row.Visible)
					visibleRowCount++;
			}

			int visibleHeight = Height - ColumnHeadersHeight - (hScrollBar.Visible ? hScrollBar.Height : 0);
			int displayedRows = 0;
			int heightUsed = 0;

			for (int i = 0; (i < Rows.Count) && (heightUsed < visibleHeight); i++)
			{
				if (Rows[i].Visible)
				{
					heightUsed += Rows[i].Height;
					displayedRows++;
				}
			}

			bool needsVScrollBar = visibleRowCount > displayedRows;

			if (needsVScrollBar)
			{
				vScrollBar.Minimum = 0;
				vScrollBar.Maximum = visibleRowCount - 1;
				vScrollBar.LargeChange = Math.Max(1, displayedRows);
				vScrollBar.SmallChange = 1;

				if (FirstDisplayedScrollingRowIndex >= 0)
				{
					int visibleRowIndex = 0;

					for (int i = 0; (i < FirstDisplayedScrollingRowIndex) && (i < Rows.Count); i++)
					{
						if (Rows[i].Visible)
							visibleRowIndex++;
					}

					int newValue = Math.Max(vScrollBar.Minimum, Math.Min(vScrollBar.Maximum - vScrollBar.LargeChange + 1, visibleRowIndex));
					
					if (vScrollBar.Value != newValue)
						vScrollBar.Value = newValue;
				}

				vScrollBar.Visible = true;
			}
			else
				vScrollBar.Visible = false;
		}



		/********************************************************************/
		/// <summary>
		/// Update horizontal scrollbar values based on DataGridView state
		/// </summary>
		/********************************************************************/
		private void UpdateHScrollBar()
		{
			int visibleWidth = Width - (vScrollBar.Visible ? vScrollBar.Width : 0);
			int totalColumnsWidth = 0;

			foreach (DataGridViewColumn column in Columns)
			{
				if (column.Visible)
					totalColumnsWidth += column.Width;
			}

			bool needsHScrollBar = totalColumnsWidth > visibleWidth;

			if (needsHScrollBar)
			{
				hScrollBar.Minimum = 0;
				hScrollBar.Maximum = totalColumnsWidth - 1;
				hScrollBar.LargeChange = visibleWidth;
				hScrollBar.SmallChange = Math.Max(1, visibleWidth / 10);

				if (FirstDisplayedScrollingColumnIndex >= 0)
				{
					int currentScrollPosition = 0;

					for (int i = 0; (i < FirstDisplayedScrollingColumnIndex) && (i < Columns.Count); i++)
					{
						if (Columns[i].Visible)
							currentScrollPosition += Columns[i].Width;
					}

					currentScrollPosition += HorizontalScrollingOffset;
					int newValue = Math.Max(hScrollBar.Minimum, Math.Min(hScrollBar.Maximum - hScrollBar.LargeChange + 1, currentScrollPosition));
					
					if (hScrollBar.Value != newValue)
						hScrollBar.Value = newValue;
				}

				hScrollBar.Visible = true;
			}
			else
				hScrollBar.Visible = false;
		}



		/********************************************************************/
		/// <summary>
		/// Handle vertical scrollbar value changed event
		/// </summary>
		/********************************************************************/
		private void VScrollBar_ValueChanged(object sender, EventArgs e)
		{
			if (updatingScrollBars)
				return;

			updatingScrollBars = true;

			try
			{
				int targetVisibleIndex = vScrollBar.Value;
				int currentVisibleIndex = 0;
				int targetRowIndex = 0;

				for (int i = 0; i < Rows.Count; i++)
				{
					if (!Rows[i].Visible)
						continue;

					if (currentVisibleIndex == targetVisibleIndex)
					{
						targetRowIndex = i;
						break;
					}

					currentVisibleIndex++;
				}

				if (targetRowIndex < Rows.Count)
					FirstDisplayedScrollingRowIndex = targetRowIndex;
			}
			finally
			{
				updatingScrollBars = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle horizontal scrollbar value changed event
		/// </summary>
		/********************************************************************/
		private void HScrollBar_ValueChanged(object sender, EventArgs e)
		{
			if (updatingScrollBars)
				return;

			updatingScrollBars = true;

			try
			{
				int targetPixelPosition = hScrollBar.Value;
				
				// Clamp target position to valid range
				int visibleWidth = ClientSize.Width - (vScrollBar.Visible ? vScrollBar.Width : 0);
				int totalColumnsWidth = 0;

				foreach (DataGridViewColumn column in Columns)
				{
					if (column.Visible)
						totalColumnsWidth += column.Width;
				}
				
				int maxScroll = Math.Max(0, totalColumnsWidth - visibleWidth);
				targetPixelPosition = Math.Min(targetPixelPosition, maxScroll);

				HorizontalScrollingOffset = targetPixelPosition;
			}
			finally
			{
				updatingScrollBars = false;
			}
		}
		#endregion

		#region Drawing
		/********************************************************************/
		/// <summary>
		/// Return the colors to use for the current state
		/// </summary>
		/********************************************************************/
		private HeaderStateColors GetHeaderColors(int columnIndex)
		{
			if (columnIndex == pressedHeaderColumnIndex)
			{
				return new HeaderStateColors
				{
					BorderColor = colors.PressedHeaderBorderColor,
					BackgroundStartColor = colors.PressedHeaderBackgroundStartColor,
					BackgroundStopColor = colors.PressedHeaderBackgroundStopColor,
					TextColor = colors.PressedHeaderTextColor
				};
			}

			return new HeaderStateColors
			{
				BorderColor = colors.NormalHeaderBorderColor,
				BackgroundStartColor = colors.NormalHeaderBackgroundStartColor,
				BackgroundStopColor = colors.NormalHeaderBackgroundStopColor,
				TextColor = colors.NormalHeaderTextColor
			};
		}



		/********************************************************************/
		/// <summary>
		/// Return the colors to use for the current state
		/// </summary>
		/********************************************************************/
		private CellStateColors GetCellColors(DataGridViewElementStates state)
		{
			if ((state & DataGridViewElementStates.Selected) != 0)
			{
				return new CellStateColors
				{
					BackgroundStartColor = colors.SelectedCellBackgroundStartColor,
					BackgroundMiddleColor = colors.SelectedCellBackgroundMiddleColor,
					BackgroundStopColor = colors.SelectedCellBackgroundStopColor,
					TextColor = colors.SelectedCellTextColor
				};
			}

			return new CellStateColors
			{
				BackgroundStartColor = colors.NormalCellBackgroundStartColor,
				BackgroundMiddleColor = colors.NormalCellBackgroundMiddleColor,
				BackgroundStopColor = colors.NormalCellBackgroundStopColor,
				TextColor = colors.NormalCellTextColor
			};
		}



		/********************************************************************/
		/// <summary>
		/// Draw a single column header
		/// </summary>
		/********************************************************************/
		private void DrawColumnHeader(Graphics g, int columnIndex, Rectangle rect, HeaderStateColors headerStateColors, object value)
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;

			DrawColumnHeaderBackground(g, rect, headerStateColors);
			DrawColumnHeaderText(g, rect, headerStateColors, value);
			DrawSortGlyph(g, columnIndex, rect, headerStateColors);
		}



		/********************************************************************/
		/// <summary>
		/// Draw a single column header background
		/// </summary>
		/********************************************************************/
		private void DrawColumnHeaderBackground(Graphics g, Rectangle rect, HeaderStateColors headerStateColors)
		{
			using (LinearGradientBrush brush = new LinearGradientBrush(rect, headerStateColors.BackgroundStartColor, headerStateColors.BackgroundStopColor, LinearGradientMode.Vertical))
			{
				g.FillRectangle(brush, rect);
			}

			using (Pen borderPen = new Pen(headerStateColors.BorderColor))
			{
				g.DrawLine(borderPen, rect.X, rect.Y + rect.Height - 1, rect.X + rect.Width - 1, rect.Y + rect.Height - 1);
				g.DrawLine(borderPen, rect.X + rect.Width - 1, rect.Y + rect.Height - 1, rect.X + rect.Width - 1, rect.Y);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw a single column header text
		/// </summary>
		/********************************************************************/
		private void DrawColumnHeaderText(Graphics g, Rectangle rect, HeaderStateColors headerStateColors, object value)
		{
			if (value != null)
			{
				string text = value.ToString();

				if (!string.IsNullOrEmpty(text))
				{
					Rectangle textRect = new Rectangle(rect.X + 2, rect.Y, rect.Width - 4, rect.Height - 1);

					TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;
					TextRenderer.DrawText(g, text, fonts.RegularFont, textRect, headerStateColors.TextColor, flags);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the sort icon
		/// </summary>
		/********************************************************************/
		private void DrawSortGlyph(Graphics g, int columnIndex, Rectangle rect, HeaderStateColors headerStateColors)
		{
			DataGridViewColumn column = Columns[columnIndex];

			if ((column.SortMode == DataGridViewColumnSortMode.NotSortable) || (column != SortedColumn))
				return;

			if (SortOrder == SortOrder.None)
				return;

			int glyphX = rect.Right - GlyphWidthSize - 8;
			int glyphY = rect.Top + ((rect.Height - 1 - GlyphHeightSize) / 2);

			Point[] triangle;

			if (SortOrder == SortOrder.Ascending)
			{
				triangle =
				[
					new Point(glyphX, glyphY + GlyphHeightSize),
					new Point(glyphX + GlyphWidthSize, glyphY + GlyphHeightSize),
					new Point(glyphX + (GlyphWidthSize / 2), glyphY)
				];
			}
			else
			{
				triangle =
				[
					new Point(glyphX, glyphY),
					new Point(glyphX + GlyphWidthSize, glyphY),
					new Point(glyphX + (GlyphWidthSize / 2), glyphY + GlyphHeightSize)
				];
			}

			using (Brush brush = new SolidBrush(headerStateColors.TextColor))
			{
				g.FillPolygon(brush, triangle);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw a single column cell
		/// </summary>
		/********************************************************************/
		private void DrawColumnCell(Graphics g, Rectangle rect, CellStateColors cellStateColors, DataGridViewContentAlignment alignment, object value)
		{
			DrawColumnCellBackground(g, rect, cellStateColors);
			DrawColumnCellText(g, rect, cellStateColors, alignment, value);
		}



		/********************************************************************/
		/// <summary>
		/// Draw a single column cell background
		/// </summary>
		/********************************************************************/
		private void DrawColumnCellBackground(Graphics g, Rectangle rect, CellStateColors cellStateColors)
		{
			g.SmoothingMode = SmoothingMode.None;

			using (LinearGradientBrush brush = new LinearGradientBrush(rect, cellStateColors.BackgroundStartColor, cellStateColors.BackgroundStopColor, LinearGradientMode.Vertical))
			{
				ColorBlend blend = new ColorBlend
				{
					Colors = [ cellStateColors.BackgroundStartColor, cellStateColors.BackgroundMiddleColor, cellStateColors.BackgroundMiddleColor, cellStateColors.BackgroundStopColor ],
					Positions = [ 0.0f, 0.1f, 0.7f, 1.0f ]
				};

				brush.InterpolationColors = blend;

				g.FillRectangle(brush, rect);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw a single column cell text
		/// </summary>
		/********************************************************************/
		private void DrawColumnCellText(Graphics g, Rectangle rect, CellStateColors cellStateColors, DataGridViewContentAlignment alignment, object value)
		{
			if (value != null)
			{
				string text = value.ToString();

				if (!string.IsNullOrEmpty(text))
				{
					g.SmoothingMode = SmoothingMode.AntiAlias;

					Font font = Font;

					Rectangle textRect = new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 1, rect.Height - 1);

					TextFormatFlags flags = TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;

					switch (alignment)
					{
						case DataGridViewContentAlignment.TopLeft:
						{
							flags |= TextFormatFlags.Top | TextFormatFlags.Left;
							break;
						}

						case DataGridViewContentAlignment.TopCenter:
						{
							flags |= TextFormatFlags.Top | TextFormatFlags.HorizontalCenter;
							break;
						}

						case DataGridViewContentAlignment.TopRight:
						{
							flags |= TextFormatFlags.Top | TextFormatFlags.Right;
							break;
						}

						case DataGridViewContentAlignment.MiddleLeft:
						{
							flags |= TextFormatFlags.Left | TextFormatFlags.VerticalCenter;
							break;
						}

						case DataGridViewContentAlignment.MiddleCenter:
						{
							flags |= TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
							break;
						}

						case DataGridViewContentAlignment.MiddleRight:
						{
							flags |= TextFormatFlags.Right | TextFormatFlags.VerticalCenter;
							break;
						}

						case DataGridViewContentAlignment.BottomLeft:
						{
							flags |= TextFormatFlags.Left | TextFormatFlags.Bottom;
							break;
						}

						case DataGridViewContentAlignment.BottomCenter:
						{
							flags |= TextFormatFlags.HorizontalCenter | TextFormatFlags.Bottom;
							break;
						}

						case DataGridViewContentAlignment.BottomRight:
						{
							flags |= TextFormatFlags.Right | TextFormatFlags.Bottom;
							break;
						}
					}

					TextRenderer.DrawText(g, text, font, textRect, cellStateColors.TextColor, flags);
				}
			}
		}
		#endregion
	}
}
