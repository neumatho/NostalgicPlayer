/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Drawing;
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Agent.Visual.Oscilloscope.Display
{
	/// <summary>
	/// This control draws the sample data for a single speaker
	/// </summary>
	internal partial class SpeakerOscilloscopeControl : UserControl
	{
		public enum ScopeType
		{
			Filled,
			Lines,
			Dots
		}

		private int[] sampleData;
		private int sampleOffset;
		private int sampleIncrement;

		private ScopeType scopeType;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SpeakerOscilloscopeControl()
		{
			InitializeComponent();

			SetStyle(ControlStyles.UserPaint, true);
		}



		/********************************************************************/
		/// <summary>
		/// Draw the given sample data
		/// </summary>
		/********************************************************************/
		public void DrawSample(int[] buffer, int offset, int increment)
		{
			lock (this)
			{
				// Remember the arguments
				sampleData = buffer;
				sampleOffset = offset;
				sampleIncrement = increment;

				// Tell the control to redraw itself
				Invalidate();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will change the type of scope to show
		/// </summary>
		/********************************************************************/
		public void SetScopeType(ScopeType type)
		{
			scopeType = type;
		}



		/********************************************************************/
		/// <summary>
		/// Will rotate the scope type
		/// </summary>
		/********************************************************************/
		public ScopeType RotateScopeType()
		{
			switch (scopeType)
			{
				case ScopeType.Filled:
				{
					scopeType = ScopeType.Lines;
					break;
				}

				case ScopeType.Lines:
				{
					scopeType = ScopeType.Dots;
					break;
				}

				default:
				{
					scopeType = ScopeType.Filled;
					break;
				}
			}

			return scopeType;
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



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicks the mouse button
		/// </summary>
		/********************************************************************/
		private void Control_Click(object sender, System.EventArgs e)
		{
			((OscilloscopeControl)Parent.Parent).SwitchScopeType();
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
				// First clear the background
				using (Brush brush = new SolidBrush(BackColor))
				{
					g.FillRectangle(brush, 0, 0, Width, Height);
				}

				// Find the middle line
				float midPos = Height / 2.0f;

				// Now draw the sample
				using (Pen pen = new Pen(ForeColor))
				{
					if ((sampleData == null) || (sampleData.Length == 0))
					{
						// No sample data assigned, so just draw a straight line
						g.DrawLine(pen, Left, midPos, Right, midPos);
					}
					else
					{
						using (Brush brush = pen.Brush)
						{
							// Find out what to multiply the sample value with
							// to get the right position in the view
							float multiply = midPos / 32768.0f;

							// Now calculate which samples to take from the buffer
							float step = (float)sampleData.Length / sampleIncrement / Width;

							switch (scopeType)
							{
								case ScopeType.Filled:
								{
									DrawFilled(g, pen, brush, midPos, multiply, step);
									break;
								}

								case ScopeType.Lines:
								{
									DrawLines(g, pen, midPos, multiply, step);
									break;
								}

								case ScopeType.Dots:
								{
									DrawDots(g, brush, midPos, multiply, step);
									break;
								}
							}
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary> 
		/// Will draw a single pixel
		/// </summary>
		/********************************************************************/
		private void SetPixel(Graphics g, Brush brush, float x, float y)
		{
			g.FillRectangle(brush, x, y, 1, 1);
		}



		/********************************************************************/
		/// <summary> 
		/// Will show the sample data as a filled block
		/// </summary>
		/********************************************************************/
		private void DrawFilled(Graphics g, Pen pen, Brush brush, float midPos, float multiply, float step)
		{
			float lastY = 0.0f;
			float pos = 0.0f;

			for (int i = 0, cnt = Width; i < cnt; i++, pos += step)
			{
				// Find the position in the view where the sample is
				float y = (sampleData[sampleOffset + (int)pos * sampleIncrement] >> 16) * multiply;

				// Draw the sample
				if (((y > 0) && (lastY > 0)) || ((y < 0) && (lastY < 0)))
					lastY = 0;

				if (lastY == y)
					SetPixel(g, brush, i, y + midPos);
				else
					g.DrawLine(pen, i, lastY + midPos, i, y + midPos);

				lastY = y;
			}
		}



		/********************************************************************/
		/// <summary> 
		/// Will show the sample data as lines
		/// </summary>
		/********************************************************************/
		private void DrawLines(Graphics g, Pen pen, float midPos, float multiply, float step)
		{
			// Initialize start point
			float endX = 0.0f;
			float endY = (sampleData[sampleOffset] >> 16) * multiply + midPos;

			float pos = step;
			for (int i = 1, cnt = Width - 1; i < cnt; i++, pos += step)
			{
				// Find the position in the view where the sample is
				float y = (sampleData[sampleOffset + (int)pos * sampleIncrement] >> 16) * multiply + midPos;

				// Draw the sample
				g.DrawLine(pen, endX, endY, i, y);
				endX = i;
				endY = y;
			}
		}



		/********************************************************************/
		/// <summary> 
		/// Will show the sample data as dots
		/// </summary>
		/********************************************************************/
		private void DrawDots(Graphics g, Brush brush, float midPos, float multiply, float step)
		{
			float pos = 0.0f;

			for (int i = 0, cnt = Width; i < cnt; i++, pos += step)
			{
				// Find the position in the view where the sample is
				float y = (sampleData[sampleOffset + (int)pos * sampleIncrement] >> 16) * multiply + midPos;

				// Draw the sample
				SetPixel(g, brush, i, y);
			}
		}
		#endregion
	}
}
