/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Native;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Controls
{
	/// <summary>
	/// 
	/// </summary>
	public partial class ReadOnlyTextBox : UserControl
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ReadOnlyTextBox()
		{
			InitializeComponent();

			textControl.SetControls(this, textVScrollBar, textHScrollBar);
		}

		#region Properties
		/********************************************************************/
		/// <summary>
		/// Return the item collection
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string[] Lines
		{
			get => textControl.Lines;

			set
			{
				textControl.Lines = value;

				textVScrollBar.Value = 0;
				textHScrollBar.Value = 0;

				FixScrollbars();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the height of a single line
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int LineHeight => (int)Math.Round(Font.GetHeight(), MidpointRounding.AwayFromZero);
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Is called when the control resizes
		/// </summary>
		/********************************************************************/
		protected override void OnResize(EventArgs e)
		{
			FixScrollbars();

			base.OnResize(e);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a mouse wheel message arrives
		/// </summary>
		/********************************************************************/
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			if (textVScrollBar.Visible)
			{
				// Navigate the mouse wheel message to the scrollbar control
				User32.SendMessageW(textVScrollBar.Handle, WM.MOUSEWHEEL, (e.Delta * SystemInformation.MouseWheelScrollLines) << 16, IntPtr.Zero);
			}

			base.OnMouseWheel(e);
		}
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TextVScrollBar_ValueChanged(object sender, EventArgs e)
		{
			textControl.Invalidate();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TextHScrollBar_ValueChanged(object sender, EventArgs e)
		{
			textControl.Invalidate();
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Calculate both scrollbar values
		/// </summary>
		/********************************************************************/
		private void FixScrollbars()
		{
			if (Lines != null)
			{
				int numberOfLines = Lines.Length;
				string longestString = Lines.Aggregate(string.Empty, (max, cur) => cur.Length > max.Length ? cur : max);

				int lineHeight = LineHeight;
				int maxWidth = GetStringWidth(longestString);

				int linesVisible = (textControl.Height - textControl.Margin.Size.Height) / lineHeight;
				int widthVisible = textControl.Width - textControl.Margin.Size.Width;

				bool vScrollBarVisible = linesVisible < numberOfLines;
				bool hScrollBarVisible = widthVisible < maxWidth;

				AdjustControls(vScrollBarVisible, hScrollBarVisible);

				// Because the width and height of the text control depend
				// if the scrollbar should be visible or not, we need need
				// to check if the resizing above affect this
				linesVisible = (textControl.Height - textControl.Margin.Size.Height) / lineHeight;
				widthVisible = textControl.Width - textControl.Margin.Size.Width;

				bool newVScrollBarVisible = linesVisible < numberOfLines;
				bool newHScrollBarVisible = widthVisible < maxWidth;

				if ((newVScrollBarVisible != vScrollBarVisible) || (newHScrollBarVisible != hScrollBarVisible))
				{
					AdjustControls(newVScrollBarVisible, newHScrollBarVisible);

					linesVisible = (textControl.Height - textControl.Margin.Size.Height) / lineHeight;
					widthVisible = textControl.Width - textControl.Margin.Size.Width;
				}

				if (newVScrollBarVisible)
					AdjustScrollBar(textVScrollBar, numberOfLines, linesVisible);

				if (newHScrollBarVisible)
					AdjustScrollBar(textHScrollBar, maxWidth, widthVisible);

				textVScrollBar.Visible = newVScrollBarVisible;
				textHScrollBar.Visible = newHScrollBarVisible;
			}
			else
			{
				textVScrollBar.Visible = false;
				textHScrollBar.Visible = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Resize the text and scrollbars
		/// </summary>
		/********************************************************************/
		private void AdjustControls(bool vScrollBarVisible, bool hScrollBarVisible)
		{
			textControl.Width = Width - (vScrollBarVisible ? textVScrollBar.Width : 0);
			textControl.Height = Height - (hScrollBarVisible ? textHScrollBar.Height : 0);
			textVScrollBar.Height = textControl.Height;
			textHScrollBar.Width = textControl.Width;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the size of the scrollbar thumb
		/// </summary>
		/********************************************************************/
		private void AdjustScrollBar(ScrollBar scrollBar, int max, int visible)
		{
			scrollBar.Maximum = Math.Max(max - 1 - 1, 0);
			scrollBar.LargeChange = Math.Max(visible - 1, 1);

			int maxValue = scrollBar.Maximum - scrollBar.LargeChange + 1;
			if (scrollBar.Value > maxValue)
				scrollBar.Value = maxValue;
		}



		/********************************************************************/
		/// <summary>
		/// Return the width of the string given
		/// </summary>
		/********************************************************************/
		private int GetStringWidth(string str)
		{
			return TextRenderer.MeasureText(str, Font).Width;
		}
		#endregion
	}
}
