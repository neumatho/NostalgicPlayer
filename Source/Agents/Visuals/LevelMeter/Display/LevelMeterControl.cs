/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.LevelMeter.Display
{
	/// <summary>
	/// The user control showing the level meter
	/// </summary>
	internal partial class LevelMeterControl : UserControl
	{
		private const int SpaceBetweenMeters = 4;

		private Bitmap backgroundBitmap;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public LevelMeterControl()
		{
			InitializeComponent();
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the visual
		/// </summary>
		/********************************************************************/
		public void InitVisual()
		{
			lock (this)
			{
				DestroyMeters();
				CreateMeters();
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
				DestroyMeters();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will tell each channel to draw itself
		/// </summary>
		/********************************************************************/
		public void SampleData(NewSampleData sampleData)
		{
			// Find the max level of each speaker
			long levelL = 0;
			long levelR = 0;

			if (sampleData.ChannelMapping.Length >= 2)
			{
				int[] sample = sampleData.SampleData;
				int leftChannel = sampleData.ChannelMapping[0];
				int rightChannel = sampleData.ChannelMapping[1];

				int increment = sampleData.ChannelCount;

				for (int i = 0; i < sampleData.SampleData.Length; i += increment)
				{
					levelL = Math.Max(levelL, Math.Abs((long)sample[i + leftChannel]));
					levelR = Math.Max(levelR, Math.Abs((long)sample[i + rightChannel]));
				}
			}
			else
			{
				foreach (int sample in sampleData.SampleData)
					levelL = Math.Max(levelL, Math.Abs((long)sample));

				levelR = levelL;
			}

			lock (this)
			{
				if (levelsPanel.Controls.Count > 0)
				{
					((SpeakerLevelMeterControl)levelsPanel.Controls[0]).UpdateLevel(levelL);
					((SpeakerLevelMeterControl)levelsPanel.Controls[1]).UpdateLevel(levelR);
				}
			}
		}

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called every time the control is resize to recalculate
		/// position and sizes
		/// </summary>
		/********************************************************************/
		private void Control_Resize(object sender, EventArgs e)
		{
			lock (this)
			{
				LayoutPanels();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called 50 times per second and do the animation
		/// </summary>
		/********************************************************************/
		private void PulseTimer_Tick(object sender, EventArgs e)
		{
			lock (this)
			{
				foreach (Control ctrl in levelsPanel.Controls)
				{
					if (ctrl is SpeakerLevelMeterControl speakerLevelMeterControl)
					{
						speakerLevelMeterControl.Animate();
						speakerLevelMeterControl.Invalidate();
					}
				}
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Create all the meters
		/// </summary>
		/********************************************************************/
		private void CreateMeters()
		{
			SpeakerLevelMeterControl ctrl = new SpeakerLevelMeterControl('L');
			levelsPanel.Controls.Add(ctrl);

			ctrl = new SpeakerLevelMeterControl('R');
			levelsPanel.Controls.Add(ctrl);

			// Now layout the panels
			LayoutPanels();
		}



		/********************************************************************/
		/// <summary>
		/// Destroy all the meters
		/// </summary>
		/********************************************************************/
		private void DestroyMeters()
		{
			while (levelsPanel.Controls.Count > 0)
			{
				// Since dispose also removes the control from the
				// collection, we just remove the first item in every
				// iteration
				levelsPanel.Controls[0].Dispose();
			}

			backgroundBitmap?.Dispose();
			backgroundBitmap = null;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the position and sizes for all the meters
		/// </summary>
		/********************************************************************/
		private void LayoutPanels()
		{
			if (levelsPanel.Controls.Count > 0)
			{
				Size clientArea = levelsPanel.ClientSize;
				int width = clientArea.Width;
				int height = (clientArea.Height - SpaceBetweenMeters) / 2;
				Size size = new Size(width, height);

				int yPos = 0;
				Control ctrl = levelsPanel.Controls[0];
				ctrl.Size = size;
				ctrl.Location = new Point(0, yPos);

				yPos = clientArea.Height - height;
				ctrl = levelsPanel.Controls[1];
				ctrl.Size = size;
				ctrl.Location = new Point(0, yPos);

				// Check if the background image need to be recreated
				if ((backgroundBitmap == null) || (backgroundBitmap.Size != size))
				{
					Bitmap oldBackgroundBitmap = backgroundBitmap;

					backgroundBitmap = new Bitmap(width, height);

					using (Graphics bitmapGraphics = Graphics.FromImage(backgroundBitmap))
					{
						ColorBlend blend = new ColorBlend(3);
						blend.Positions = new[] { 0.0f, 0.85f, 1.0f };
						blend.Colors = new[] { Color.Green, Color.Yellow, Color.Red };

						using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(new Point(0, 0), backgroundBitmap.Size), Color.Black, Color.White, LinearGradientMode.Horizontal))
						{
							brush.InterpolationColors = blend;

							bitmapGraphics.FillRectangle(brush, 0, 0, backgroundBitmap.Width, backgroundBitmap.Height);
						}
					}

					((SpeakerLevelMeterControl)levelsPanel.Controls[0]).SetBackgroundBitmap(backgroundBitmap);
					((SpeakerLevelMeterControl)levelsPanel.Controls[1]).SetBackgroundBitmap(backgroundBitmap);

					// Now it is safe to dispose, because the speaker controls has been updated with the new bitmap
					oldBackgroundBitmap?.Dispose();
				}
			}
		}
		#endregion
	}
}
