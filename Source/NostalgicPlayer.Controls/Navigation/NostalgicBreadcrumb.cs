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
using System.Linq;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Components;
using Polycode.NostalgicPlayer.Controls.Designer;
using Polycode.NostalgicPlayer.Controls.Events;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;
using Polycode.NostalgicPlayer.Controls.Theme.Standard;

namespace Polycode.NostalgicPlayer.Controls.Navigation
{
	/// <summary>
	/// Themed breadcrumb control, which shows a list of items separated
	/// by an arrow. All items except the last one (the current one) can
	/// be clicked
	/// </summary>
	[DefaultEvent(nameof(ItemClicked))]
	[DefaultProperty(nameof(Items))]
	public class NostalgicBreadcrumb : Control, IThemeControl, IFontConfiguration
	{
		private enum SegmentKind
		{
			Item,
			Ellipsis,
			Separator
		}

		private readonly record struct Segment(SegmentKind Kind, Rectangle Bounds, int ItemIndex);

		// Width of the area the arrow between two items is drawn in
		private const int SeparatorWidth = 14;

		// Extra space on each side of the item texts, so the click area is a
		// little bigger than the text itself
		private const int ItemPadding = 2;

		private const string EllipsisText = "…";

		private const char PathSeparator = '/';

		private const TextFormatFlags TextFlags = TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter;

		private IBreadcrumbColors colors;
		private IFonts fonts;

		private FontConfiguration fontConfiguration;

		private Font hoverFont;
		private Font currentFont;

		private readonly NostalgicBreadcrumbItemCollection items;
		private readonly List<Segment> segments = new List<Segment>();

		private int hoverIndex = -1;
		private int pressedIndex = -1;

		private int updateCount;
		private bool itemsChangedWhileUpdating;

		private string lastPath = string.Empty;
		private NostalgicBreadcrumbItem[] lastItems = [];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicBreadcrumb()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw, true);
			SetStyle(ControlStyles.Selectable, false);

			items = new NostalgicBreadcrumbItemCollection(this);

			if (DesignerHelper.IsInDesignMode())
				SetTheme(new StandardTheme());
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
				if (fontConfiguration != null)
				{
					fontConfiguration.FontChanged -= FontConfiguration_FontChanged;
					fontConfiguration = null;
				}

				DisposeDerivedFonts();
			}

			base.Dispose(disposing);
		}

		#region Designer properties
		/********************************************************************/
		/// <summary>
		/// Holds all the items to show
		/// </summary>
		/********************************************************************/
		[Category("Data")]
		[Description("The items to show in the breadcrumb.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public NostalgicBreadcrumbItemCollection Items => items;



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
				if (fontConfiguration != null)
					fontConfiguration.FontChanged -= FontConfiguration_FontChanged;

				fontConfiguration = value;

				if (fontConfiguration != null)
					fontConfiguration.FontChanged += FontConfiguration_FontChanged;

				if (IsHandleCreated)
				{
					UpdateFont();
					Invalidate();
				}
			}
		}
		#endregion

		#region Public properties
		/********************************************************************/
		/// <summary>
		/// Return or set all the items as a path, where the item texts are
		/// separated by a slash. Setting the path will replace all the
		/// items. Empty parts are ignored
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string Path
		{
			get => string.Join(PathSeparator, items.Select(x => x.Text));

			set
			{
				string[] parts = (value ?? string.Empty).Split(PathSeparator, StringSplitOptions.RemoveEmptyEntries);

				BeginUpdate();

				try
				{
					items.Clear();
					items.AddRange(parts.Select(x => new NostalgicBreadcrumbItem(x)));
				}
				finally
				{
					EndUpdate();
				}
			}
		}
		#endregion

		#region Public methods
		/********************************************************************/
		/// <summary>
		/// Stop updating the control when the items change, until EndUpdate
		/// is called. Calls can be nested
		/// </summary>
		/********************************************************************/
		public void BeginUpdate()
		{
			updateCount++;
		}



		/********************************************************************/
		/// <summary>
		/// Resume updating the control. If any items has been changed since
		/// BeginUpdate was called, the control is updated once
		/// </summary>
		/********************************************************************/
		public void EndUpdate()
		{
			if (updateCount == 0)
				return;

			updateCount--;

			if ((updateCount == 0) && itemsChangedWhileUpdating)
			{
				itemsChangedWhileUpdating = false;
				ItemsChanged();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Go one level up by removing the last item. Nothing happens if
		/// already at the root
		/// </summary>
		/********************************************************************/
		public void GotoParent()
		{
			if (items.Count > 1)
				items.RemoveAfter(items.Count - 2);
		}
		#endregion

		#region Events
		/********************************************************************/
		/// <summary>
		/// Fires when an item is clicked
		/// </summary>
		/********************************************************************/
		[Category("Action")]
		[Description("Fires when an item is clicked.")]
		public event BreadcrumbItemClickedEventHandler ItemClicked;



		/********************************************************************/
		/// <summary>
		/// Fires the ItemClicked event
		/// </summary>
		/********************************************************************/
		protected virtual void OnItemClicked(BreadcrumbItemClickedEventArgs e)
		{
			if (ItemClicked != null)
				ItemClicked(this, e);
		}



		/********************************************************************/
		/// <summary>
		/// Fires when the path changes, either by setting the Path property
		/// or by changing the items
		/// </summary>
		/********************************************************************/
		[Category("Property Changed")]
		[Description("Fires when the path changes.")]
		public event BreadcrumbPathChangedEventHandler PathChanged;



		/********************************************************************/
		/// <summary>
		/// Fires the PathChanged event
		/// </summary>
		/********************************************************************/
		protected virtual void OnPathChanged(BreadcrumbPathChangedEventArgs e)
		{
			if (PathChanged != null)
				PathChanged(this, e);
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

			UpdateFont();

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
			colors = theme.BreadcrumbColors;
			fonts = theme.StandardFonts;

			BackColor = Color.Transparent;

			UpdateFont();

			Invalidate();
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// The default size of the control
		/// </summary>
		/********************************************************************/
		protected override Size DefaultSize => new Size(200, 20);



		/********************************************************************/
		/// <summary>
		/// Return the size needed to show all items without an ellipsis
		/// </summary>
		/********************************************************************/
		public override Size GetPreferredSize(Size proposedSize)
		{
			int[] widths = MeasureItemWidths();
			if (widths.Length == 0)
				return new Size(0, Height);

			return new Size(widths.Sum() + ((widths.Length - 1) * SeparatorWidth), Height);
		}



		/********************************************************************/
		/// <summary>
		/// Recreate the derived fonts and recalculate the layout
		/// </summary>
		/********************************************************************/
		protected override void OnFontChanged(EventArgs e)
		{
			CreateDerivedFonts();
			CalculateLayout();
			NotifyPreferredSizeChanged();

			base.OnFontChanged(e);
		}



		/********************************************************************/
		/// <summary>
		/// Recalculate the layout when the size changes
		/// </summary>
		/********************************************************************/
		protected override void OnSizeChanged(EventArgs e)
		{
			CalculateLayout();

			base.OnSizeChanged(e);
		}



		/********************************************************************/
		/// <summary>
		/// Redraw in the new state
		/// </summary>
		/********************************************************************/
		protected override void OnEnabledChanged(EventArgs e)
		{
			ResetMouseState();
			Invalidate();

			base.OnEnabledChanged(e);
		}



		/********************************************************************/
		/// <summary>
		/// Update the hover state
		/// </summary>
		/********************************************************************/
		protected override void OnMouseMove(MouseEventArgs e)
		{
			int index = FindClickableItem(e.Location);
			if (index != hoverIndex)
			{
				hoverIndex = index;
				Cursor = index >= 0 ? Cursors.Hand : Cursors.Default;

				Invalidate();
			}

			base.OnMouseMove(e);
		}



		/********************************************************************/
		/// <summary>
		/// Clear the hover state
		/// </summary>
		/********************************************************************/
		protected override void OnMouseLeave(EventArgs e)
		{
			ResetMouseState();
			Invalidate();

			base.OnMouseLeave(e);
		}



		/********************************************************************/
		/// <summary>
		/// Remember which item is pressed
		/// </summary>
		/********************************************************************/
		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				pressedIndex = FindClickableItem(e.Location);
				Invalidate();
			}

			base.OnMouseDown(e);
		}



		/********************************************************************/
		/// <summary>
		/// Fire the click event if the mouse is released over the same item
		/// as it was pressed on
		/// </summary>
		/********************************************************************/
		protected override void OnMouseUp(MouseEventArgs e)
		{
			if ((e.Button == MouseButtons.Left) && (pressedIndex >= 0))
			{
				int index = pressedIndex;
				pressedIndex = -1;

				Invalidate();

				if (FindClickableItem(e.Location) == index)
					OnItemClicked(new BreadcrumbItemClickedEventArgs(items[index], index));
			}

			base.OnMouseUp(e);
		}



		/********************************************************************/
		/// <summary>
		/// Paint the items and arrows
		/// </summary>
		/********************************************************************/
		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;

			if ((items.Count == 0) && DesignerHelper.IsInDesignMode(this))
			{
				TextRenderer.DrawText(g, Name, Font, ClientRectangle, colors.DisabledTextColor, TextFlags);
				return;
			}

			int lastIndex = items.Count - 1;

			foreach (Segment segment in segments)
			{
				switch (segment.Kind)
				{
					case SegmentKind.Separator:
					{
						DrawArrow(g, segment.Bounds);
						break;
					}

					case SegmentKind.Ellipsis:
					{
						TextRenderer.DrawText(g, EllipsisText, Font, segment.Bounds, Enabled ? colors.TextColor : colors.DisabledTextColor, TextFlags | TextFormatFlags.HorizontalCenter);
						break;
					}

					case SegmentKind.Item:
					{
						NostalgicBreadcrumbItem item = items[segment.ItemIndex];
						Font font = Font;
						Color color;

						if (segment.ItemIndex == lastIndex)
						{
							font = currentFont;
							color = Enabled ? colors.CurrentTextColor : colors.DisabledTextColor;
						}
						else if (!Enabled || !item.Enabled)
							color = colors.DisabledTextColor;
						else if (segment.ItemIndex == pressedIndex)
						{
							font = hoverFont;
							color = colors.PressedTextColor;
						}
						else if (segment.ItemIndex == hoverIndex)
						{
							font = hoverFont;
							color = colors.HoverTextColor;
						}
						else
							color = colors.TextColor;

						Rectangle textRect = Rectangle.Inflate(segment.Bounds, -ItemPadding, 0);
						TextRenderer.DrawText(g, item.Text, font, textRect, color, TextFlags | TextFormatFlags.EndEllipsis);
						break;
					}
				}
			}
		}
		#endregion

		#region Internal methods
		/********************************************************************/
		/// <summary>
		/// Is called when the items or one of the items has changed
		/// </summary>
		/********************************************************************/
		internal void ItemsChanged()
		{
			if (updateCount > 0)
			{
				itemsChangedWhileUpdating = true;
				return;
			}

			ResetMouseState();
			CalculateLayout();
			NotifyPreferredSizeChanged();
			Invalidate();

			// Only fire the event if the path has really changed, e.g. not
			// when an item is enabled or disabled
			string newPath = Path;
			if (newPath != lastPath)
			{
				NostalgicBreadcrumbItem[] oldItems = lastItems;
				NostalgicBreadcrumbItem[] newItems = items.ToArray();

				lastPath = newPath;
				lastItems = newItems;

				OnPathChanged(new BreadcrumbPathChangedEventArgs(oldItems, newItems));
			}
		}
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// React when the attached FontConfiguration recalculates its font
		/// (e.g. theme manager just initialized, or one of FontType /
		/// FontStyle / FontSize changed at runtime)
		/// </summary>
		/********************************************************************/
		private void FontConfiguration_FontChanged(object sender, EventArgs e)
		{
			if (IsHandleCreated)
			{
				UpdateFont();
				Invalidate();
			}
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
			if (fonts == null)
				return;

			Font = fontConfiguration?.Font ?? fonts.RegularFont;

			// Setting the same font as before does not trigger OnFontChanged,
			// so make sure the derived fonts exist
			if (currentFont == null)
			{
				CreateDerivedFonts();
				CalculateLayout();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Create the fonts used for hovered and the current item
		/// </summary>
		/********************************************************************/
		private void CreateDerivedFonts()
		{
			DisposeDerivedFonts();

			hoverFont = new Font(Font, Font.Style | FontStyle.Underline);
			currentFont = new Font(Font, Font.Style | FontStyle.Bold);
		}



		/********************************************************************/
		/// <summary>
		/// Dispose the fonts created from the main font
		/// </summary>
		/********************************************************************/
		private void DisposeDerivedFonts()
		{
			hoverFont?.Dispose();
			hoverFont = null;

			currentFont?.Dispose();
			currentFont = null;
		}



		/********************************************************************/
		/// <summary>
		/// Clear the hover and pressed states
		/// </summary>
		/********************************************************************/
		private void ResetMouseState()
		{
			// Only touch the cursor if we changed it, so it will not be
			// serialized by the designer
			if (hoverIndex >= 0)
				Cursor = Cursors.Default;

			hoverIndex = -1;
			pressedIndex = -1;
		}



		/********************************************************************/
		/// <summary>
		/// Tell the parent that the preferred size may have changed, so it
		/// can arrange its children again
		/// </summary>
		/********************************************************************/
		private void NotifyPreferredSizeChanged()
		{
			Parent?.PerformLayout(this, nameof(PreferredSize));
		}



		/********************************************************************/
		/// <summary>
		/// Measure the width of the given text
		/// </summary>
		/********************************************************************/
		private static int MeasureText(string text, Font font)
		{
			return TextRenderer.MeasureText(text, font, Size.Empty, TextFlags).Width;
		}



		/********************************************************************/
		/// <summary>
		/// Measure the width of each item including padding
		/// </summary>
		/********************************************************************/
		private int[] MeasureItemWidths()
		{
			int count = items.Count;
			int lastIndex = count - 1;

			int[] widths = new int[count];
			for (int i = 0; i < count; i++)
			{
				// Hover uses an underlined font, which has the same width as
				// the normal font, so only the last item needs special care
				Font font = i == lastIndex ? currentFont ?? Font : Font;
				widths[i] = MeasureText(items[i].Text, font) + (ItemPadding * 2);
			}

			return widths;
		}



		/********************************************************************/
		/// <summary>
		/// Find out where each item and arrow should be drawn. If there is
		/// not room for all items, the first items are replaced by an
		/// ellipsis, so the last items are always visible
		/// </summary>
		/********************************************************************/
		private void CalculateLayout()
		{
			segments.Clear();

			int count = items.Count;
			if (count == 0)
				return;

			int lastIndex = count - 1;
			int height = ClientSize.Height;
			int available = ClientSize.Width;

			int[] widths = MeasureItemWidths();

			int ellipsisWidth = MeasureText(EllipsisText, Font) + (ItemPadding * 2);

			// Total width when showing all items from firstVisible and
			// forward, with an ellipsis in front if some items are hidden
			int TotalWidth(int firstVisible)
			{
				int total = firstVisible > 0 ? ellipsisWidth + SeparatorWidth : 0;

				for (int i = firstVisible; i < count; i++)
					total += widths[i];

				return total + ((count - 1 - firstVisible) * SeparatorWidth);
			}

			int first = 0;
			while ((first < lastIndex) && (TotalWidth(first) > available))
				first++;

			int x = 0;

			if (first > 0)
			{
				segments.Add(new Segment(SegmentKind.Ellipsis, new Rectangle(x, 0, ellipsisWidth, height), -1));
				x += ellipsisWidth;

				segments.Add(new Segment(SegmentKind.Separator, new Rectangle(x, 0, SeparatorWidth, height), -1));
				x += SeparatorWidth;
			}

			for (int i = first; i < count; i++)
			{
				int width = widths[i];

				// If even the last item alone cannot be there, shrink it, so
				// it will be drawn with an ellipsis at the end
				if (i == lastIndex)
					width = Math.Max(0, Math.Min(width, available - x));

				segments.Add(new Segment(SegmentKind.Item, new Rectangle(x, 0, width, height), i));
				x += width;

				if (i < lastIndex)
				{
					segments.Add(new Segment(SegmentKind.Separator, new Rectangle(x, 0, SeparatorWidth, height), -1));
					x += SeparatorWidth;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the index of the clickable item at the given point or -1
		/// </summary>
		/********************************************************************/
		private int FindClickableItem(Point point)
		{
			if (!Enabled)
				return -1;

			int lastIndex = items.Count - 1;

			foreach (Segment segment in segments)
			{
				if ((segment.Kind == SegmentKind.Item) && segment.Bounds.Contains(point))
				{
					if ((segment.ItemIndex == lastIndex) || !items[segment.ItemIndex].Enabled)
						return -1;

					return segment.ItemIndex;
				}
			}

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// Draw the arrow between two items
		/// </summary>
		/********************************************************************/
		private void DrawArrow(Graphics g, Rectangle bounds)
		{
			float halfHeight = Math.Max(3, Font.Height / 4);
			float halfWidth = halfHeight / 2;

			float centerX = bounds.Left + (bounds.Width / 2.0f);
			float centerY = bounds.Top + (bounds.Height / 2.0f);

			PointF[] points =
			[
				new PointF(centerX - halfWidth, centerY - halfHeight),
				new PointF(centerX + halfWidth, centerY),
				new PointF(centerX - halfWidth, centerY + halfHeight)
			];

			SmoothingMode oldMode = g.SmoothingMode;
			g.SmoothingMode = SmoothingMode.AntiAlias;

			using (Pen pen = new Pen(colors.SeparatorColor, 1.5f))
			{
				pen.StartCap = LineCap.Round;
				pen.EndCap = LineCap.Round;
				pen.LineJoin = LineJoin.Round;

				g.DrawLines(pen, points);
			}

			g.SmoothingMode = oldMode;
		}
		#endregion

		#region Designer filtering (hide properties from PropertyGrid)
		/********************************************************************/
		/// <summary>
		/// Register a provider that filters properties for the designer
		/// </summary>
		/********************************************************************/
		static NostalgicBreadcrumb()
		{
			TypeDescriptor.AddProvider(new NostalgicBreadcrumbTypeDescriptionProvider(), typeof(NostalgicBreadcrumb));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer.
		/// This is needed for properties that we cannot override, but we do
		/// it for all our hidden properties to be sure
		/// </summary>
		private sealed class NostalgicBreadcrumbTypeDescriptionProvider : TypeDescriptionProvider
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
			public NostalgicBreadcrumbTypeDescriptionProvider() : base(parent)
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
