/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Controls
{
	/// <summary>
	/// This control will render some text lines tight together
	/// </summary>
	public partial class TextControl : UserControl
	{
		private readonly Color textColor = Color.FromArgb(30, 57, 91);

		private ReadOnlyTextBox owner;
		private VScrollBar vScrollBar;
		private HScrollBar hScrollBar;

		private string[] lines;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public TextControl()
		{
			InitializeComponent();

			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
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
			get => lines;

			set
			{
				lines = value;
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
		public void SetControls(ReadOnlyTextBox readOnlyTextBox, VScrollBar vScrollBar, HScrollBar hScrollBar)
		{
			owner = readOnlyTextBox;
			this.vScrollBar = vScrollBar;
			this.hScrollBar = hScrollBar;
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Paint the whole control
		/// </summary>
		/********************************************************************/
		protected override void OnPaint(PaintEventArgs e)
		{
			DrawBackground(e.Graphics);
			DrawLines(e.Graphics);

			base.OnPaint(e);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a key is pressed in the list control
		/// </summary>
		/********************************************************************/
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			// Check for different keyboard shortcuts
			Keys modifiers = keyData & Keys.Modifiers;
			Keys key = keyData & Keys.KeyCode;

			switch (key)
			{
				case Keys.Up:
				{
					if (vScrollBar.Visible)
					{
						if (vScrollBar.Value > 0)
							vScrollBar.Value--;
					}

					return true;
				}

				case Keys.Down:
				{
					if (vScrollBar.Visible)
					{
						if (vScrollBar.Value <= (vScrollBar.Maximum - vScrollBar.LargeChange))
							vScrollBar.Value++;
					}

					return true;
				}

				case Keys.Left:
				{
					if (hScrollBar.Visible)
					{
						if (hScrollBar.Value > 0)
							hScrollBar.Value = Math.Max(hScrollBar.Value - 4, 0);
					}

					return true;
				}

				case Keys.Right:
				{
					if (hScrollBar.Visible)
					{
						if (hScrollBar.Value <= (hScrollBar.Maximum - hScrollBar.LargeChange))
							hScrollBar.Value = Math.Min(hScrollBar.Value + 4, hScrollBar.Maximum - hScrollBar.LargeChange);
					}

					return true;
				}
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Draw the background
		/// </summary>
		/********************************************************************/
		private void DrawBackground(Graphics g)
		{
			g.FillRectangle(Brushes.White, 0, 0, Width, Height);
		}



		/********************************************************************/
		/// <summary>
		/// Draw all the items
		/// </summary>
		/********************************************************************/
		private void DrawLines(Graphics g)
		{
			if ((lines != null) && (lines.Length > 0))
			{
				Font font = Font;
				int lineHeight = owner.LineHeight;

				int count = lines.Length;
				int height = Height - Margin.Size.Height;

				int top = Margin.Top;
				int left = Margin.Left;

				for (int i = vScrollBar.Value, y = 0; i < count; i++)
				{
					DrawSingleItem(g, font, left, top + y, i);

					y += lineHeight;
					if (y >= height)
						break;
				}

				g.FillRectangle(Brushes.White, 0, 0, Margin.Left, Height);
				g.FillRectangle(Brushes.White, Width - Margin.Right, 0, Margin.Right, Height);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will draw a single item
		/// </summary>
		/********************************************************************/
		private void DrawSingleItem(Graphics g, Font font, int x, int y, int lineIndex)
		{
			string line = lines[lineIndex];

			TextRenderer.DrawText(g, line, font, new Point(x - hScrollBar.Value, y), textColor, TextFormatFlags.NoPrefix);
		}
		#endregion
	}
}
