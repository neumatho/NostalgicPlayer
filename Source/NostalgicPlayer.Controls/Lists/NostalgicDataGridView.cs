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
using Polycode.NostalgicPlayer.Controls.Theme;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Lists
{
	/// <summary>
	/// Custom DataGridView with theme support and custom rendering
	/// </summary>
	public class NostalgicDataGridView : DataGridView, IThemeControl
	{
		private const int GlyphWidthSize = 8;
		private const int GlyphHeightSize = 5;

		private IDataGridViewColors colors;
		private IFonts fonts;

		private FontConfiguration fontConfiguration;

		private int pressedHeaderColumnIndex = -1;

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
		public NostalgicDataGridView()
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
			if (DesignMode)
				SetTheme(ThemeManagerFactory.GetThemeManager().CurrentTheme);

			UpdateFont();

			AutoResizeRows();

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
		protected override void OnCellMouseUp(DataGridViewCellMouseEventArgs e)
		{
			if (!DoubleBuffered)
				DoubleBuffered = true;

			if (pressedHeaderColumnIndex >= 0)
			{
				int oldIndex = pressedHeaderColumnIndex;
				pressedHeaderColumnIndex = -1;
				Invalidate(GetColumnDisplayRectangle(oldIndex, false));
			}

			base.OnCellMouseUp(e);
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

/*			if (CellBorderStyle != DataGridViewCellBorderStyle.None)
			{
				using (Pen gridPen = new Pen(colors.GridLineColor))
				{
					e.Graphics.DrawLine(gridPen, e.CellBounds.Left, e.CellBounds.Bottom - 1,
					                    e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
				}
			}
*/
		}
		#endregion

		#region Designer filtering (hide properties from PropertyGrid)
		/********************************************************************/
		/// <summary>
		/// Register a provider that filters properties for the designer
		/// </summary>
		/********************************************************************/
		static NostalgicDataGridView()
		{
			TypeDescriptor.AddProvider(new NostalgicDataGridViewTypeDescriptionProvider(), typeof(NostalgicDataGridView));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer.
		/// This is needed for properties that we cannot override, but we do
		/// it for all our hidden properties to be sure
		/// </summary>
		private sealed class NostalgicDataGridViewTypeDescriptionProvider : TypeDescriptionProvider
		{
			private static readonly TypeDescriptionProvider parent = TypeDescriptor.GetProvider(typeof(DataGridView));

			private static readonly string[] propertiesToHide =
			[
				nameof(BackgroundColor),
				nameof(BorderStyle),
				nameof(AlternatingRowsDefaultCellStyle),
				nameof(BackColor),
				nameof(CellBorderStyle),
				nameof(ColumnHeadersBorderStyle),
				nameof(ColumnHeadersDefaultCellStyle),
				nameof(ColumnHeadersVisible),
				nameof(DefaultCellStyle),
				nameof(EnableHeadersVisualStyles),
				nameof(GridColor),
				nameof(RowHeadersBorderStyle),
				nameof(RowHeadersDefaultCellStyle),
				nameof(RowHeadersVisible),
				nameof(RowsDefaultCellStyle),
				nameof(ShowCellErrors),
				nameof(ShowEditingIcon),
				nameof(ShowRowErrors),
				nameof(AllowUserToAddRows),
				nameof(AllowUserToDeleteRows),
				nameof(AllowUserToResizeRows),
				nameof(ReadOnly),
				nameof(SelectionMode),
				nameof(BackgroundImage),
				nameof(BackgroundImageLayout),
				nameof(Font),
				nameof(ForeColor),
				nameof(RightToLeft),
				nameof(DrawMode)
			];

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public NostalgicDataGridViewTypeDescriptionProvider() : base(parent)
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
