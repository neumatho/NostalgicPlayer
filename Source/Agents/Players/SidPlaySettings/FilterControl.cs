/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlaySettings
{
	/// <summary>
	/// Show the filter curve
	/// </summary>
	internal partial class FilterControl : UserControl
	{
		private const int CellWidth = 32;
		private const int CellHeight = 32;

		private float filterFs;
		private float filterFm;
		private float filterFt;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FilterControl()
		{
			InitializeComponent();

			SetStyle(ControlStyles.UserPaint, true);

			// Set default values
			filterFs = SidPlay.SidPlaySettings.DefaultFs;
			filterFm = SidPlay.SidPlaySettings.DefaultFm;
			filterFt = SidPlay.SidPlaySettings.DefaultFt;
		}



		/********************************************************************/
		/// <summary>
		/// Update the curve with new values
		/// </summary>
		/********************************************************************/
		public void Update(float fs, float fm, float ft)
		{
			filterFs = fs;
			filterFm = fm;
			filterFt = ft;

			Invalidate();
		}

		#region Event handlers
		/********************************************************************/
		/// <summary> 
		/// Is called every time an update is needed
		/// </summary>
		/********************************************************************/
		private void Control_Paint(object sender, PaintEventArgs e)
		{
			UpdateWindow(e.Graphics);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary> 
		/// Update the window
		/// </summary>
		/********************************************************************/
		private void UpdateWindow(Graphics g)
		{
			// First clear the background
			using (Brush brush = new SolidBrush(BackColor))
			{
				g.FillRectangle(brush, 0, 0, Width, Height);
			}

			// Draw the grid
			using (Pen pen = new Pen(Color.LimeGreen))
			{
				g.DrawRectangle(pen, CellWidth * 0, CellHeight * 0, CellWidth, CellHeight);
				g.DrawRectangle(pen, CellWidth * 1, CellHeight * 0, CellWidth, CellHeight);
				g.DrawRectangle(pen, CellWidth * 2, CellHeight * 0, CellWidth, CellHeight);
				g.DrawRectangle(pen, CellWidth * 3, CellHeight * 0, CellWidth - 1, CellHeight);

				g.DrawRectangle(pen, CellWidth * 0, CellHeight * 1, CellWidth, CellHeight);
				g.DrawRectangle(pen, CellWidth * 1, CellHeight * 1, CellWidth, CellHeight);
				g.DrawRectangle(pen, CellWidth * 2, CellHeight * 1, CellWidth, CellHeight);
				g.DrawRectangle(pen, CellWidth * 3, CellHeight * 1, CellWidth - 1, CellHeight);

				g.DrawRectangle(pen, CellWidth * 0, CellHeight * 2, CellWidth, CellHeight);
				g.DrawRectangle(pen, CellWidth * 1, CellHeight * 2, CellWidth, CellHeight);
				g.DrawRectangle(pen, CellWidth * 2, CellHeight * 2, CellWidth, CellHeight);
				g.DrawRectangle(pen, CellWidth * 3, CellHeight * 2, CellWidth - 1, CellHeight);

				g.DrawRectangle(pen, CellWidth * 0, CellHeight * 3, CellWidth, CellHeight - 1);
				g.DrawRectangle(pen, CellWidth * 1, CellHeight * 3, CellWidth, CellHeight - 1);
				g.DrawRectangle(pen, CellWidth * 2, CellHeight * 3, CellWidth, CellHeight - 1);
				g.DrawRectangle(pen, CellWidth * 3, CellHeight * 3, CellWidth - 1, CellHeight - 1);
			}

			// Draw axis text
			using (Brush brush = new SolidBrush(Color.DarkGray))
			{
				g.TranslateTransform(Width / 2, Height / 2);
				g.RotateTransform(270);
				SizeF size = g.MeasureString(Resources.IDS_SETTINGS_FILTER_YAXIS, Font);
				g.DrawString(Resources.IDS_SETTINGS_FILTER_YAXIS, Font, brush, Height / 2 - size.Width - 4, -Width / 2 + 4);

				g.ResetTransform();
				size = g.MeasureString(Resources.IDS_SETTINGS_FILTER_XAXIS, Font);
				g.DrawString(Resources.IDS_SETTINGS_FILTER_XAXIS, Font, brush, Width - size.Width - 4, Height - size.Height - 4);
			}

			// Draw the curve
			using (Pen pen = new Pen(Color.Blue))
			{
				float oldVX = -1.0f;
				float oldVY = -1.0f;

				for (float x = 0.0f, vx = 0.0f; x < 2048.0f; x += (2048.0f / Width), vx++)
				{
					// Calculate the y position
					double y = -(Math.Exp(x / 2048.0f * Math.Log(filterFs)) / filterFm) - filterFt;

					if (y > 0.0f)
						y = 0.0f;

					if (y < -1.0f)
						y = -1.0f;

					// And transform it to the view y position
					float vy = (float)(-y * (Height - 1));

					if (oldVX < 0.0f)
					{
						oldVX = vx;
						oldVY = vy;
					}

					// Set the point in the view
					g.DrawLine(pen, oldVX, oldVY, vx, vy);

					oldVX = vx;
					oldVY = vy;
				}
			}
		}
		#endregion
	}
}
