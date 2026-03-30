/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Components;
using Polycode.NostalgicPlayer.Controls.Designer;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;
using Polycode.NostalgicPlayer.Controls.Theme.Standard;

namespace Polycode.NostalgicPlayer.Controls.Containers
{
	/// <summary>
	/// Themed tab control
	/// </summary>
	public class NostalgicTab : TabControl, IThemeControl, IFontConfiguration, ISupportInitialize
	{
		#region NostalgicTabPageCollection
		/// <summary>
		/// Typed wrapper around TabPages that returns NostalgicTabPage
		/// instances so that the shadowed Visible property is accessible
		/// </summary>
		public sealed class NostalgicTabPageCollection : IList<NostalgicTabPage>
		{
			private readonly NostalgicTab owner;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			internal NostalgicTabPageCollection(NostalgicTab owner)
			{
				this.owner = owner;
			}



			/********************************************************************/
			/// <summary>
			/// Get or set a page by index
			/// </summary>
			/********************************************************************/
			public NostalgicTabPage this[int index]
			{
				get => (NostalgicTabPage)owner.TabPages[index];
				set => throw new NotSupportedException();
			}



			/********************************************************************/
			/// <summary>
			/// Return the number of pages
			/// </summary>
			/********************************************************************/
			public int Count => owner.TabPages.Count;



			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			public bool IsReadOnly => false;



			/********************************************************************/
			/// <summary>
			/// Add a page
			/// </summary>
			/********************************************************************/
			public void Add(NostalgicTabPage page)
			{
				owner.TabPages.Add(page);
			}



			/********************************************************************/
			/// <summary>
			/// Insert a page at the given index
			/// </summary>
			/********************************************************************/
			public void Insert(int index, NostalgicTabPage page)
			{
				owner.TabPages.Insert(index, page);
			}



			/********************************************************************/
			/// <summary>
			/// Remove a page
			/// </summary>
			/********************************************************************/
			public bool Remove(NostalgicTabPage page)
			{
				if (!owner.TabPages.Contains(page))
					return false;

				owner.TabPages.Remove(page);

				return true;
			}



			/********************************************************************/
			/// <summary>
			/// Remove a page at the given index
			/// </summary>
			/********************************************************************/
			public void RemoveAt(int index)
			{
				owner.TabPages.RemoveAt(index);
			}



			/********************************************************************/
			/// <summary>
			/// Remove all pages
			/// </summary>
			/********************************************************************/
			public void Clear()
			{
				owner.TabPages.Clear();
			}



			/********************************************************************/
			/// <summary>
			/// Check if a page is in the collection
			/// </summary>
			/********************************************************************/
			public bool Contains(NostalgicTabPage page)
			{
				return owner.TabPages.Contains(page);
			}



			/********************************************************************/
			/// <summary>
			/// Return the index of a page
			/// </summary>
			/********************************************************************/
			public int IndexOf(NostalgicTabPage page)
			{
				return owner.TabPages.IndexOf(page);
			}



			/********************************************************************/
			/// <summary>
			/// Copy pages to an array
			/// </summary>
			/********************************************************************/
			public void CopyTo(NostalgicTabPage[] array, int arrayIndex)
			{
				for (int i = 0; i < Count; i++)
					array[arrayIndex + i] = this[i];
			}



			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			public IEnumerator<NostalgicTabPage> GetEnumerator()
			{
				for (int i = 0; i < Count; i++)
					yield return this[i];
			}



			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}
		#endregion

		private const int CornerRadius = 3;

		private struct TabStateColors
		{
			public Color BackgroundStartColor { get; init; }
			public Color BackgroundStopColor { get; init; }
			public Color TextColor { get; init; }
		}

		private ITabColors colors;
		private IFonts fonts;

		private FontConfiguration fontConfiguration;

		// Maps visible tab index to actual TabPages index
		private readonly List<int> visibleTabIndices = new List<int>();

		// True between BeginInit/EndInit - suppresses rebuilds
		private bool isInitializing;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicTab()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

			Pages = new NostalgicTabPageCollection(this);
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

		#region ISupportInitialize
		/********************************************************************/
		/// <summary>
		/// Called before batch initialization. Suppresses rebuilds until
		/// EndInit is called
		/// </summary>
		/********************************************************************/
		public void BeginInit()
		{
			isInitializing = true;
		}



		/********************************************************************/
		/// <summary>
		/// Called after batch initialization. Rebuilds the visible tab
		/// list once
		/// </summary>
		/********************************************************************/
		public void EndInit()
		{
			isInitializing = false;

			RebuildVisibleTabList();
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
			colors = theme.TabColors;
			fonts = theme.StandardFonts;

			Invalidate();
		}
		#endregion

		#region Public properties
		/********************************************************************/
		/// <summary>
		/// Typed collection that provides access to tab pages as
		/// NostalgicTabPage, so the shadowed Visible property works
		/// correctly
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public NostalgicTabPageCollection Pages { get; }
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public override Rectangle DisplayRectangle
		{
			get
			{
				Rectangle rect = base.DisplayRectangle;
				rect.Inflate(rect.X - 1, 1);

				return rect;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Rebuild visible list when a page is added
		/// </summary>
		/********************************************************************/
		protected override void OnControlAdded(ControlEventArgs e)
		{
			if (!isInitializing && (e.Control is NostalgicTabPage))
				RebuildVisibleTabList();

			base.OnControlAdded(e);
		}



		/********************************************************************/
		/// <summary>
		/// Rebuild visible list when a page is removed
		/// </summary>
		/********************************************************************/
		protected override void OnControlRemoved(ControlEventArgs e)
		{
			if (!isInitializing && (e.Control is NostalgicTabPage))
				RebuildVisibleTabList();

			base.OnControlRemoved(e);
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
		/// Handle tab clicks ourselves so hidden tabs cannot be selected
		/// </summary>
		/********************************************************************/
		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				int clickedVisible = GetVisibleTabIndexAtPoint(e.Location);

				if (clickedVisible >= 0)
				{
					int realIndex = visibleTabIndices[clickedVisible];

					if (realIndex != SelectedIndex)
						SelectedIndex = realIndex;
				}
			}

			base.OnMouseDown(e);
		}



		/********************************************************************/
		/// <summary>
		/// Repaint when the selected tab changes
		/// </summary>
		/********************************************************************/
		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			Invalidate();

			base.OnSelectedIndexChanged(e);
		}



		/********************************************************************/
		/// <summary>
		/// Intercept tab selection to prevent hidden tabs from being
		/// selected
		/// </summary>
		/********************************************************************/
		protected override void OnSelecting(TabControlCancelEventArgs e)
		{
			if ((e.TabPageIndex >= 0) && (e.TabPageIndex < TabCount) && !IsPageVisible(TabPages[e.TabPageIndex]))
				e.Cancel = true;

			base.OnSelecting(e);
		}



		/********************************************************************/
		/// <summary>
		/// Handle arrow keys before the base TabControl processes them
		/// in WndProc. This ensures hidden tabs are skipped
		/// </summary>
		/********************************************************************/
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			Keys key = keyData & Keys.KeyCode;

			if ((key == Keys.Left) || (key == Keys.Right))
			{
				int currentVisible = visibleTabIndices.IndexOf(SelectedIndex);

				if ((currentVisible >= 0) && (visibleTabIndices.Count > 1))
				{
					int newVisible;

					if (key == Keys.Left)
						newVisible = currentVisible > 0 ? currentVisible - 1 : visibleTabIndices.Count - 1;
					else
						newVisible = currentVisible < visibleTabIndices.Count - 1 ? currentVisible + 1 : 0;

					SelectedIndex = visibleTabIndices[newVisible];
				}

				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
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
			if (colors == null)
				return;

			Graphics g = e.Graphics;

			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

			Font font = GetFont();

			ClearBackground(g);
			DrawBackground(g);
			DrawTabs(g, font);
		}
		#endregion

		#region Internal methods
		/********************************************************************/
		/// <summary>
		/// Called by NostalgicTabPage when its Visible property changes
		/// </summary>
		/********************************************************************/
		internal void NotifyTabPageVisibilityChanged()
		{
			if (isInitializing)
				return;

			RebuildVisibleTabList();

			// If the currently selected tab was hidden, select the first
			// visible tab instead
			if ((SelectedIndex >= 0) && (SelectedIndex < TabCount) && !IsPageVisible(TabPages[SelectedIndex]))
			{
				if (visibleTabIndices.Count > 0)
					SelectedIndex = visibleTabIndices[0];
			}

			Invalidate();
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Return whether a page should be shown in the tab strip
		/// </summary>
		/********************************************************************/
		private bool IsPageVisible(TabPage page)
		{
			if (page is NostalgicTabPage ntp)
				return ntp.Visible;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Rebuild the list of visible tab indices
		/// </summary>
		/********************************************************************/
		private void RebuildVisibleTabList()
		{
			visibleTabIndices.Clear();

			for (int i = 0; i < TabCount; i++)
			{
				if (IsPageVisible(TabPages[i]))
					visibleTabIndices.Add(i);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the visible tab index at a given point, or -1 if none
		/// </summary>
		/********************************************************************/
		private int GetVisibleTabIndexAtPoint(Point pt)
		{
			for (int i = 0; i < visibleTabIndices.Count; i++)
			{
				if (GetVisibleTabRect(i).Contains(pt))
					return i;
			}

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the rectangle for a visible tab by its visible index
		/// </summary>
		/********************************************************************/
		private Rectangle GetVisibleTabRect(int visibleIndex)
		{
			if ((visibleIndex < 0) || (visibleIndex >= visibleTabIndices.Count))
				return Rectangle.Empty;

			// Find the start on the first tab
			Rectangle rect = Rectangle.Empty;

			int tabTop = 2;
			int tabHeight = DisplayRectangle.Y - tabTop;
			int x = 2;

			using (Graphics g = CreateGraphics())
			{
				Font font = GetFont();

				for (int i = 0; i <= visibleIndex; i++)
				{
					int realIndex = visibleTabIndices[i];
					string text = TabPages[realIndex].Text;

					int textWidth = TextRenderer.MeasureText(g, text, font).Width;
					int tabWidth = textWidth + 5;

					if (i == visibleIndex)
					{
						rect = new Rectangle(x, tabTop, tabWidth, tabHeight);
						break;
					}

					x += tabWidth + 1;
				}
			}

			return rect;
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
			return fontConfiguration?.Font ?? fonts.TabFont;
		}



		/********************************************************************/
		/// <summary>
		/// Return the colors for a tab based on its state
		/// </summary>
		/********************************************************************/
		private TabStateColors GetTabColors(int visibleIndex)
		{
			int realIndex = visibleTabIndices[visibleIndex];

			if (realIndex == SelectedIndex)
			{
				return new TabStateColors
				{
					BackgroundStartColor = colors.SelectedTabBackgroundStartColor,
					BackgroundStopColor = colors.SelectedTabBackgroundStopColor,
					TextColor = colors.SelectedTabTextColor
				};
			}

			return new TabStateColors
			{
				BackgroundStartColor = colors.NormalTabBackgroundStartColor,
				BackgroundStopColor = colors.NormalTabBackgroundStopColor,
				TextColor = colors.NormalTabTextColor
			};
		}



		/********************************************************************/
		/// <summary>
		/// Clear the background with the parent background to avoid
		/// artifacts
		/// </summary>
		/********************************************************************/
		private void ClearBackground(Graphics g)
		{
			if (Parent != null)
			{
				GraphicsState state = g.Save();

				try
				{
					g.TranslateTransform(-Left, -Top);

					Rectangle parentRect = new Rectangle(Point.Empty, Parent.ClientSize);

					using (PaintEventArgs e = new PaintEventArgs(g, parentRect))
					{
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
		private void DrawBackground(Graphics g)
		{
			Rectangle rect = DisplayRectangle;
			rect.Inflate(1, 1);

			using (SolidBrush bgBrush = new SolidBrush(colors.BackgroundColor))
			{
				g.FillRectangle(bgBrush, rect);
			}

			using (Pen borderPen = new Pen(colors.BorderColor))
			{
				g.DrawRectangle(borderPen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw all the tab headers (only visible ones)
		/// </summary>
		/********************************************************************/
		private void DrawTabs(Graphics g, Font font)
		{
			int selectedVisible = visibleTabIndices.IndexOf(SelectedIndex);

			// Draw non-selected visible tabs first
			for (int i = 0; i < visibleTabIndices.Count; i++)
			{
				if (i != selectedVisible)
					DrawTab(g, i, font);
			}

			// Draw selected tab last so it paints on top
			if (selectedVisible >= 0)
				DrawTab(g, selectedVisible, font);
		}



		/********************************************************************/
		/// <summary>
		/// Draw a single tab header by its visible index
		/// </summary>
		/********************************************************************/
		private void DrawTab(Graphics g, int visibleIndex, Font font)
		{
			Rectangle rect = GetVisibleTabRect(visibleIndex);

			if (rect.IsEmpty)
				return;

			int realIndex = visibleTabIndices[visibleIndex];
			bool isSelected = (realIndex == SelectedIndex);

			// Selected tab is a little bit bigger than the rest
			if (isSelected)
			{
				rect.X -= 2;
				rect.Width += 4;
				rect.Height += rect.Y;
				rect.Y = 0;
			}

			TabStateColors tabStateColors = GetTabColors(visibleIndex);

			using (GraphicsPath tabPath = CreateTabPath(new Rectangle(rect.X, rect.Y, rect.Width, rect.Height - 1)))
			{
				if (isSelected)
				{
					using (GraphicsPath fullPath = CreateTabPath(rect))
					{
						DrawTabBackground(g, rect, fullPath, tabStateColors);
					}
				}
				else
					DrawTabBackground(g, rect, tabPath, tabStateColors);

				DrawTabBorder(g, tabPath);
			}

			DrawTabText(g, TabPages[realIndex].Text, rect, font, tabStateColors);

			// Draw focus rectangle on the selected tab
			if (isSelected && Focused)
			{
				Rectangle focusRect = rect;
				focusRect.Inflate(-3, -3);

				if ((focusRect.Width > 0) && (focusRect.Height > 0))
					ControlPaint.DrawFocusRectangle(g, focusRect);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw a single tab background
		/// </summary>
		/********************************************************************/
		private void DrawTabBackground(Graphics g, Rectangle rect, GraphicsPath path, TabStateColors tabStateColors)
		{
			g.SmoothingMode = SmoothingMode.None;

			using (LinearGradientBrush brush = new LinearGradientBrush(rect, tabStateColors.BackgroundStartColor, tabStateColors.BackgroundStopColor, LinearGradientMode.Vertical))
			{
				g.FillPath(brush, path);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw a single tab border
		/// </summary>
		/********************************************************************/
		private void DrawTabBorder(Graphics g, GraphicsPath path)
		{
			using (Pen borderPen = new Pen(colors.BorderColor))
			{
				g.DrawPath(borderPen, path);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw a single tab text
		/// </summary>
		/********************************************************************/
		private void DrawTabText(Graphics g, string text, Rectangle rect, Font font, TabStateColors tabStateColors)
		{
			TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;

			TextRenderer.DrawText(g, text, font, rect, tabStateColors.TextColor, flags);
		}



		/********************************************************************/
		/// <summary>
		/// Create a GraphicsPath for a tab shape with rounded top corners
		/// and a flat bottom edge
		/// </summary>
		/********************************************************************/
		private GraphicsPath CreateTabPath(Rectangle rect)
		{
			GraphicsPath path = new GraphicsPath();

			int diameter = CornerRadius * 2;
			int right = rect.Right - 1;
			int bottom = rect.Bottom;

			// Start at bottom-left and draw up the left side
			path.AddLine(rect.Left, bottom, rect.Left, rect.Top + CornerRadius);

			// Top-left arc (rounded corner)
			path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);

			// Top-right arc (rounded corner) - this also creates the top line
			path.AddArc(right - diameter, rect.Top, diameter, diameter, 270, 90);

			// Right side down to bottom
			path.AddLine(right, rect.Top + CornerRadius, right, bottom);

			// Path is not closed, so no bottom line is drawn

			return path;
		}
		#endregion

		#region Designer filtering (hide properties from PropertyGrid)
		/********************************************************************/
		/// <summary>
		/// Register a provider that filters properties for the designer
		/// </summary>
		/********************************************************************/
		static NostalgicTab()
		{
			TypeDescriptor.AddProvider(new NostalgicTabTypeDescriptionProvider(), typeof(NostalgicTab));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer
		/// </summary>
		private sealed class NostalgicTabTypeDescriptionProvider : TypeDescriptionProvider
		{
			private static readonly TypeDescriptionProvider parent = TypeDescriptor.GetProvider(typeof(TabControl));

			private static readonly string[] propertiesToHide =
			[
				nameof(BackColor),
				nameof(BackgroundImage),
				nameof(BackgroundImageLayout),
				nameof(Font),
				nameof(ForeColor),
				nameof(RightToLeft),
				nameof(DrawMode),
				nameof(Appearance),
				nameof(HotTrack),
			];

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public NostalgicTabTypeDescriptionProvider() : base(parent)
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
