/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Lists;
using Polycode.NostalgicPlayer.Platform.Native;

namespace Polycode.NostalgicPlayer.Controls.Containers
{
	/// <summary>
	/// This is the inner flow layout panel that holds the content.
	///
	/// The panel is sized to fit its whole content and is then moved around inside
	/// its parent (which clips it), so the visible part follows the scroll bars
	/// </summary>
	internal class NostalgicFlowLayoutPanelInternal : FlowLayoutPanel, IMessageFilter
	{
		private const int WheelLineStep = 16;

		private NostalgicVScrollBar vScrollBar;
		private NostalgicHScrollBar hScrollBar;
		private Panel corner;
		private NostalgicFlowLayoutPanel parentControl;

		private bool updatingScrollBars;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicFlowLayoutPanelInternal()
		{
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);

			AutoScroll = false;
			BackColor = Color.Transparent;
		}

		#region Properties
		/********************************************************************/
		/// <summary>
		/// Return the size of the visible content area (excluding any
		/// visible scroll bars)
		/// </summary>
		/********************************************************************/
		public Size ViewportSize
		{
			get
			{
				if (Parent == null)
					return ClientSize;

				Size available = Parent.ClientSize;

				int width = available.Width - ((vScrollBar != null) && vScrollBar.Visible ? vScrollBar.Width : 0);
				int height = available.Height - ((hScrollBar != null) && hScrollBar.Visible ? hScrollBar.Height : 0);

				return new Size(Math.Max(0, width), Math.Max(0, height));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the current scroll position in pixels (0, 0 is the top
		/// left)
		/// </summary>
		/********************************************************************/
		public Point ScrollPosition => new Point(hScrollBar.Visible ? hScrollBar.Value : 0, vScrollBar.Visible ? vScrollBar.Value : 0);
		#endregion

		#region Events
		/********************************************************************/
		/// <summary>
		/// Is raised when the content is scrolled
		/// </summary>
		/********************************************************************/
		public event ScrollEventHandler ContentScrolled;



		/********************************************************************/
		/// <summary>
		/// Raise the ContentScrolled event
		/// </summary>
		/********************************************************************/
		private void OnContentScrolled(ScrollEventArgs e)
		{
			if (ContentScrolled != null)
				ContentScrolled(this, e);
		}
		#endregion

		#region Public methods
		/********************************************************************/
		/// <summary>
		/// Set the owner and scroll bars of this control
		/// </summary>
		/********************************************************************/
		public void SetControls(NostalgicVScrollBar nostalgicVScrollBar, NostalgicHScrollBar nostalgicHScrollBar, Panel cornerPanel, NostalgicFlowLayoutPanel parent)
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

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Update the scroll bars once the handle is ready, and start
		/// listening for mouse wheel messages
		/// </summary>
		/********************************************************************/
		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			Application.AddMessageFilter(this);

			UpdateScrollBars();
		}



		/********************************************************************/
		/// <summary>
		/// Stop listening for mouse wheel messages
		/// </summary>
		/********************************************************************/
		protected override void OnHandleDestroyed(EventArgs e)
		{
			Application.RemoveMessageFilter(this);

			base.OnHandleDestroyed(e);
		}



		/********************************************************************/
		/// <summary>
		/// Recalculate the scroll bars whenever the content is laid out (items
		/// added, removed or resized)
		/// </summary>
		/********************************************************************/
		protected override void OnLayout(LayoutEventArgs e)
		{
			base.OnLayout(e);

			UpdateScrollBars();
		}
		#endregion

		#region IMessageFilter implementation
		/********************************************************************/
		/// <summary>
		/// Catch mouse wheel messages so the content scrolls even when the
		/// mouse is over one of the hosted child controls (a normal
		/// OnMouseWheel would never reach this panel in that case)
		/// </summary>
		/********************************************************************/
		public bool PreFilterMessage(ref Message m)
		{
			if (m.Msg != (int)WM.MOUSEWHEEL)
				return false;

			if ((Parent == null) || !Parent.IsHandleCreated)
				return false;

			if (((vScrollBar == null) || !vScrollBar.Visible) && ((hScrollBar == null) || !hScrollBar.Visible))
				return false;

			// The mouse position is in screen coordinates in the low/high word of lParam
			int lParam = (int)(m.LParam.ToInt64() & 0xffffffff);
			Point screenPosition = new Point((short)(lParam & 0xffff), (short)((lParam >> 16) & 0xffff));

			Rectangle viewportOnScreen = Parent.RectangleToScreen(new Rectangle(Point.Empty, ViewportSize));
			if (!viewportOnScreen.Contains(screenPosition))
				return false;

			// When panels are nested, the cursor is over more than one viewport
			// at once. Only the innermost panel under the cursor should scroll
			if (FindOwningPanel(GetDeepestControlAtPoint(screenPosition)) != this)
				return false;

			int delta = (short)((m.WParam.ToInt64() >> 16) & 0xffff);
			ScrollByWheel(delta);

			return true;
		}
		#endregion

		#region Handlers
		/********************************************************************/
		/// <summary>
		/// React when the parent control is resized
		/// </summary>
		/********************************************************************/
		private void ParentControl_Resize(object sender, EventArgs e)
		{
			UpdateScrollBars();
		}



		/********************************************************************/
		/// <summary>
		/// React when the vertical scroll bar changes its value
		/// </summary>
		/********************************************************************/
		private void VScrollBar_ValueChanged(object sender, EventArgs e)
		{
			if (updatingScrollBars)
				return;

			ApplyScrollPosition();

			OnContentScrolled(new ScrollEventArgs(ScrollEventType.ThumbPosition, vScrollBar.Value, ScrollOrientation.VerticalScroll));
		}



		/********************************************************************/
		/// <summary>
		/// React when the horizontal scroll bar changes its value
		/// </summary>
		/********************************************************************/
		private void HScrollBar_ValueChanged(object sender, EventArgs e)
		{
			if (updatingScrollBars)
				return;

			ApplyScrollPosition();

			OnContentScrolled(new ScrollEventArgs(ScrollEventType.ThumbPosition, hScrollBar.Value, ScrollOrientation.HorizontalScroll));
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Scroll the content by the given mouse wheel delta
		/// </summary>
		/********************************************************************/
		private void ScrollByWheel(int delta)
		{
			if (vScrollBar.Visible)
			{
				int linesToScroll = SystemInformation.MouseWheelScrollLines;
				vScrollBar.Value -= delta / 120 * linesToScroll * Math.Max(1, vScrollBar.SmallChange);
			}
			else if (hScrollBar.Visible)
				hScrollBar.Value -= delta / 120 * Math.Max(1, hScrollBar.SmallChange);
		}



		/********************************************************************/
		/// <summary>
		/// Find the deepest child control at the given screen position
		/// </summary>
		/********************************************************************/
		private Control GetDeepestControlAtPoint(Point screenPosition)
		{
			Control root = FindForm();

			if (root == null)
			{
				root = this;

				while (root.Parent != null)
					root = root.Parent;
			}

			Control current = root;

			while (true)
			{
				Control child = current.GetChildAtPoint(current.PointToClient(screenPosition), GetChildAtPointSkip.Invisible | GetChildAtPointSkip.Disabled);
				if ((child == null) || (child == current))
					break;

				current = child;
			}

			return current;
		}



		/********************************************************************/
		/// <summary>
		/// Walk up the parent chain and return the first flow layout panel
		/// found (or null)
		/// </summary>
		/********************************************************************/
		private NostalgicFlowLayoutPanelInternal FindOwningPanel(Control control)
		{
			while (control != null)
			{
				if (control is NostalgicFlowLayoutPanelInternal panel)
					return panel;

				control = control.Parent;
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the custom scroll bars
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
		/// Recalculate which scroll bars are needed, size everything and apply
		/// the current scroll position
		/// </summary>
		/********************************************************************/
		private void UpdateScrollBars()
		{
			if (updatingScrollBars)
				return;

			if ((vScrollBar == null) || (hScrollBar == null) || (Parent == null))
				return;

			updatingScrollBars = true;

			try
			{
				Size available = Parent.ClientSize;

				int vScrollBarWidth = vScrollBar.Width;
				int hScrollBarHeight = hScrollBar.Height;

				// Iteratively calculate scroll bar visibility until it stabilizes.
				// This handles the chicken-and-egg problem where showing one bar
				// steals space from the other dimension, which can make the other
				// bar needed too
				bool needsVScrollBar = false;
				bool needsHScrollBar = false;

				for (int iterations = 0; iterations < 3; iterations++)
				{
					int viewWidth = available.Width - (needsVScrollBar ? vScrollBarWidth : 0);
					int viewHeight = available.Height - (needsHScrollBar ? hScrollBarHeight : 0);

					Size preferred = MeasureContent(viewWidth);

					bool newNeedsVScrollBar = preferred.Height > viewHeight;
					bool newNeedsHScrollBar = preferred.Width > viewWidth;

					if ((newNeedsVScrollBar == needsVScrollBar) && (newNeedsHScrollBar == needsHScrollBar))
						break;

					needsVScrollBar = newNeedsVScrollBar;
					needsHScrollBar = newNeedsHScrollBar;
				}

				int finalViewWidth = Math.Max(0, available.Width - (needsVScrollBar ? vScrollBarWidth : 0));
				int finalViewHeight = Math.Max(0, available.Height - (needsHScrollBar ? hScrollBarHeight : 0));

				Size content = MeasureContent(finalViewWidth);
				int contentWidth = Math.Max(finalViewWidth, content.Width);
				int contentHeight = Math.Max(finalViewHeight, content.Height);

				// Size the panel to fit its whole content
				SetBounds(Left, Top, contentWidth, contentHeight);

				ConfigureScrollBar(vScrollBar, needsVScrollBar, contentHeight, finalViewHeight, WheelLineStep);
				ConfigureScrollBar(hScrollBar, needsHScrollBar, contentWidth, finalViewWidth, Math.Max(1, finalViewWidth / 10));

				FixPositionAndSizes(available, finalViewWidth, finalViewHeight, needsVScrollBar, needsHScrollBar);
				ApplyScrollPosition();
			}
			finally
			{
				updatingScrollBars = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Measure the size needed by the content given the available width
		/// </summary>
		/********************************************************************/
		private Size MeasureContent(int availableWidth)
		{
			// When wrapping, the content has to wrap within the available width.
			// When not wrapping, leave the width unconstrained so we get the full
			// content width
			Size constraint = WrapContents ? new Size(Math.Max(0, availableWidth), 0) : Size.Empty;

			return GetPreferredSize(constraint);
		}



		/********************************************************************/
		/// <summary>
		/// Setup a single scroll bar
		/// </summary>
		/********************************************************************/
		private void ConfigureScrollBar(NostalgicScrollBar scrollBar, bool visible, int contentSize, int viewSize, int smallChange)
		{
			if (visible)
			{
				scrollBar.Minimum = 0;
				scrollBar.Maximum = contentSize;
				scrollBar.LargeChange = Math.Max(1, viewSize);
				scrollBar.SmallChange = smallChange;
			}

			scrollBar.Visible = visible;
		}



		/********************************************************************/
		/// <summary>
		/// Fix sizes and positions on the scroll bars and corner
		/// </summary>
		/********************************************************************/
		private void FixPositionAndSizes(Size available, int viewWidth, int viewHeight, bool needsVScrollBar, bool needsHScrollBar)
		{
			vScrollBar.SetBounds(available.Width - vScrollBar.Width, 0, vScrollBar.Width, viewHeight);
			hScrollBar.SetBounds(0, available.Height - hScrollBar.Height, viewWidth, hScrollBar.Height);

			corner.Visible = needsVScrollBar && needsHScrollBar;

			if (corner.Visible)
				corner.SetBounds(viewWidth, viewHeight, vScrollBar.Width, hScrollBar.Height);
		}



		/********************************************************************/
		/// <summary>
		/// Move the content according to the current scroll bar values
		/// </summary>
		/********************************************************************/
		private void ApplyScrollPosition()
		{
			Size viewport = ViewportSize;

			int maxX = Math.Max(0, Width - viewport.Width);
			int maxY = Math.Max(0, Height - viewport.Height);

			int x = (hScrollBar != null) && hScrollBar.Visible ? Math.Min(hScrollBar.Value, maxX) : 0;
			int y = (vScrollBar != null) && vScrollBar.Visible ? Math.Min(vScrollBar.Value, maxY) : 0;

			Location = new Point(-x, -y);
		}
		#endregion
	}
}
