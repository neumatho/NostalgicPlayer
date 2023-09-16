/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.SpinningSquares.Display
{
	/// <summary>
	/// The user control holding the squares
	/// </summary>
	internal partial class SpinningSquaresControl : UserControl
	{
		private const int PanelMargin = 4;

		private int channelsInUse = 0;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SpinningSquaresControl()
		{
			InitializeComponent();
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the visual
		/// </summary>
		/********************************************************************/
		public void InitVisual(int channels)
		{
			lock (this)
			{
				channelsInUse = Math.Min(channels, 16);

				DestroyChannelPanels();
				CreateChannelPanels();

				hashPanel.Visible = false;
				squaresPanel.Visible = true;
			}

			pulseTimer.Start();
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the visual
		/// </summary>
		/********************************************************************/
		public void CleanupVisual()
		{
			pulseTimer.Stop();

			lock (this)
			{
				channelsInUse = 0;

				squaresPanel.Visible = false;
				hashPanel.Visible = true;

				DestroyChannelPanels();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Tell the visual about a channel change
		/// </summary>
		/********************************************************************/
		public void ChannelChange(ChannelChanged[] channelChanged)
		{
			lock (this)
			{
				for (int i = 0, cnt = Math.Min(channelChanged.Length, Math.Min(channelsInUse, squaresPanel.Controls.Count)); i < cnt; i++)
				{
					if (channelChanged[i] != null)
					{
						if (squaresPanel.Controls[i].Controls[0] is SingleSpinningSquareControl singleSpinningSquare)
							singleSpinningSquare.ChannelChange(channelChanged[i]);
					}
				}
			}
		}

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called every time the squares panel is resize to recalculate
		/// position and sizes
		/// </summary>
		/********************************************************************/
		private void SquaresPanel_Resize(object sender, EventArgs e)
		{
			LayoutPanels();
		}



		/********************************************************************/
		/// <summary>
		/// Is called 50 times per second and do the animation
		/// </summary>
		/********************************************************************/
		private void PulseTimer_Tick(object sender, EventArgs e)
		{
			foreach (Control ctrl in squaresPanel.Controls)
			{
				if (ctrl.Controls[0] is SingleSpinningSquareControl singleSpinningSquare)
				{
					singleSpinningSquare.Animate();
					singleSpinningSquare.Invalidate();
				}
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Create all the panels for the squares
		/// </summary>
		/********************************************************************/
		private void CreateChannelPanels()
		{
			var rowCols = FindRowsAndColumns();

			int panelsToCreate = rowCols.Rows * rowCols.Columns;

			// First create all the panels
			for (int i = 0; i < panelsToCreate; i++)
			{
				Panel panel = new Panel();
				panel.BorderStyle = BorderStyle.Fixed3D;
				panel.Dock = DockStyle.None;

				Control ctrl;

				if (i < channelsInUse)
					ctrl = new SingleSpinningSquareControl(i + 1);
				else
				{
					ctrl = new Panel();
					ctrl.BackgroundImage = Resources.IDB_HASH;
				}

				ctrl.Dock = DockStyle.Fill;
				panel.Controls.Add(ctrl);

				squaresPanel.Controls.Add(panel);
			}

			// Now layout the panels
			LayoutPanels();
		}



		/********************************************************************/
		/// <summary>
		/// Destroy channel panels
		/// </summary>
		/********************************************************************/
		private void DestroyChannelPanels()
		{
			while (squaresPanel.Controls.Count > 0)
			{
				// Since the dispose also removes the control from the
				// collection, we just remove the first item in every
				// iteration
				squaresPanel.Controls[0].Dispose();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the position and sizes for all the square panels
		/// </summary>
		/********************************************************************/
		private void LayoutPanels()
		{
			var rowCols = FindRowsAndColumns();

			Size clientArea = ParentForm.ClientSize;

			int panelWidth = (clientArea.Width - PanelMargin * 2) / rowCols.Columns;
			int panelHeight = (clientArea.Height - PanelMargin * 2) / rowCols.Rows;

			for (int i = 0; i < rowCols.Rows; i++)
			{
				for (int j = 0; j < rowCols.Columns; j++)
				{
					Panel panel = (Panel)squaresPanel.Controls[i * rowCols.Columns + j];

					panel.Size = new Size(panelWidth - PanelMargin * 2, panelHeight - PanelMargin * 2);
					panel.Location = new Point(PanelMargin * 2 + j * panelWidth, PanelMargin * 2 + i * panelHeight);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the number of rows and columns to use
		/// </summary>
		/********************************************************************/
		private (int Rows, int Columns) FindRowsAndColumns()
		{
			int rows = (int)Math.Round(Math.Sqrt(channelsInUse), MidpointRounding.AwayFromZero);
			int cols = (int)Math.Round((double)channelsInUse / rows, MidpointRounding.AwayFromZero);

			if ((rows * cols) < channelsInUse)
				rows++;

			return (Rows: rows, Columns: cols);
		}
		#endregion
	}
}
