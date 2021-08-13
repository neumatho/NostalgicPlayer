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
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using NAudio.Dsp;

namespace Polycode.NostalgicPlayer.Agent.Visual.SpectrumAnalyzer.Display
{
	/// <summary>
	/// The user control showing the audio spectrum
	/// </summary>
	internal partial class SpectrumAnalyzerControl : UserControl
	{
		private const int BarWidth = 8;

		private int[] positions;
		private Bitmap backgroundBitmap;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SpectrumAnalyzerControl()
		{
			InitializeComponent();

			SetStyle(ControlStyles.UserPaint, true);
		}



		/********************************************************************/
		/// <summary>
		/// Update the control with the positions given
		/// </summary>
		/********************************************************************/
		public void Update(Complex[] fftResult)
		{
			lock (this)
			{
				if (fftResult == null)
				{
					// Null means clear the display
					positions = null;

					backgroundBitmap?.Dispose();
					backgroundBitmap = null;
				}
				else
				{
					// Find out how many bars that can be shown
					int width = ClientSize.Width;

					int barCount = width / BarWidth;
					if ((positions == null) || (positions.Length != barCount))
						positions = new int[barCount];

					// Calculate the bar positions
					int b0 = 0;

					for (int x = 0; x < barCount; x++)
					{
						float peak = 0;
						int b1 = (int)Math.Pow(2, x * 10.0 / (barCount - 1));

						if (b1 > 1023)
							b1 = 1023;
						else if (b1 <= b0)
							b1 = b0 + 1;

						for (; b0 < b1; b0++)
						{
							if (peak < fftResult[1 + b0].Y)
								peak = fftResult[1 + b0].Y;
						}

						int y = (int)(Math.Sqrt(peak) * 3 * 255 - 4);
						if (y > 255)
							y = 255;
						else if (y < 0)
							y = 0;

						positions[x] = y;
					}
				}

				// Redraw the control
				Invalidate();
			}
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
			lock (this)
			{
				// Check if the background image need to be recreated
				if ((backgroundBitmap == null) || (backgroundBitmap.Height != Height))
				{
					backgroundBitmap?.Dispose();

					backgroundBitmap = new Bitmap(BarWidth, Height);

					using (Graphics bitmapGraphics = Graphics.FromImage(backgroundBitmap))
					{
						ColorBlend blend = new ColorBlend(3);
						blend.Positions = new[] { 0.0f, 0.4f, 1.0f };
						blend.Colors = new [] {Color.Red, Color.Yellow, Color.Green };

						using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(new Point(0, 0), backgroundBitmap.Size), Color.Black, Color.White, LinearGradientMode.Vertical))
						{
							brush.InterpolationColors = blend;

							bitmapGraphics.FillRectangle(brush, 0, 0, backgroundBitmap.Width, backgroundBitmap.Height);
						}
					}
				}

				// First clear the background
				using (Brush brush = new SolidBrush(BackColor))
				{
					g.FillRectangle(brush, 0, 0, Width, Height);
				}

				if (positions != null)
				{
					// Draw the bars
					int x = (Width - positions.Length * BarWidth) / 2;

					foreach (int barHeight in positions)
					{
						// Find how much that need to be filled
						int startPos = Height - (barHeight * Height / 256);

						g.DrawImage(backgroundBitmap, new Rectangle(x, startPos, BarWidth - 1, Height), new Rectangle(0, startPos, BarWidth - 1, Height), GraphicsUnit.Pixel);

						x += BarWidth;
					}
				}
			}
		}
		#endregion
	}
}
