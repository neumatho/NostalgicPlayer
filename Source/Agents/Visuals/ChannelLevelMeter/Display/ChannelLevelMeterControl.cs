/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.ChannelLevelMeter.Display
{
	/// <summary>
	/// Orientation for the channel level meter
	/// </summary>
	internal enum Orientation
	{
		Horizontal,
		Vertical
	}

	/// <summary>
	/// The user control showing the channel level meter
	/// </summary>
	internal partial class ChannelLevelMeterControl : UserControl
	{
		private const int SpaceBetweenMeters = 4;

		private Bitmap backgroundBitmap;
		private int channelCount;
		private Orientation currentOrientation = Orientation.Horizontal;
		private ChannelLevelMeterSettings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ChannelLevelMeterControl()
		{
			InitializeComponent();

			// Load settings
			settings = new ChannelLevelMeterSettings();

			// Apply loaded settings
			currentOrientation = settings.Orientation;
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
				channelCount = channels;

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
		public void ChannelChange(ChannelChanged[] channelChanged)
		{
			if (channelChanged == null)
				return;

			lock (this)
			{
				// Update each channel meter with the volume
				for (int i = 0; i < channelChanged.Length && i < levelsPanel.Controls.Count; i++)
				{
					if (channelChanged[i] != null && channelChanged[i].Volume.HasValue)
					{
						ushort vol = channelChanged[i].Volume.Value;

						// Volume is in 0-256 range
						long maxLevel = ((long)vol * int.MaxValue) / 256;

						((SpeakerLevelMeterControl)levelsPanel.Controls[i]).UpdateLevel(maxLevel);
					}
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



		/********************************************************************/
		/// <summary>
		/// Is called when the context menu is opening
		/// </summary>
		/********************************************************************/
		private void ContextMenu_Opening(object sender, CancelEventArgs e)
		{
			// Update checkmarks based on current orientation
			horizontalItem.Checked = (currentOrientation == Orientation.Horizontal);
			verticalItem.Checked = (currentOrientation == Orientation.Vertical);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the horizontal menu item is clicked
		/// </summary>
		/********************************************************************/
		private void HorizontalItem_Click(object sender, EventArgs e)
		{
			if (currentOrientation != Orientation.Horizontal)
			{
				currentOrientation = Orientation.Horizontal;

				lock (this)
				{
					LayoutPanels();
				}

				SaveSettings();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the vertical menu item is clicked
		/// </summary>
		/********************************************************************/
		private void VerticalItem_Click(object sender, EventArgs e)
		{
			if (currentOrientation != Orientation.Vertical)
			{
				currentOrientation = Orientation.Vertical;

				lock (this)
				{
					LayoutPanels();
				}

				SaveSettings();
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Save current settings
		/// </summary>
		/********************************************************************/
		private void SaveSettings()
		{
			if (settings != null)
			{
				settings.Orientation = currentOrientation;
				settings.Settings.SaveSettings();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Create all the meters
		/// </summary>
		/********************************************************************/
		private void CreateMeters()
		{
			for (int i = 0; i < channelCount; i++)
			{
				string channelName = $"CH{i + 1:00}";
				SpeakerLevelMeterControl ctrl = new SpeakerLevelMeterControl(channelName);
				levelsPanel.Controls.Add(ctrl);
			}

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
			int numberOfLevels = levelsPanel.Controls.Count;

			if (numberOfLevels > 0)
			{
				Size clientArea = levelsPanel.ClientSize;
				Size size;
				LinearGradientMode gradientMode;

				if (currentOrientation == Orientation.Vertical)
				{
					// Vertical layout: meters stacked top to bottom
					int width = clientArea.Width;
					float height = (clientArea.Height - (numberOfLevels * SpaceBetweenMeters - SpaceBetweenMeters)) / (float)numberOfLevels;
					if (height < 1)
						height = 1;

					size = new Size(width, (int)height);
					float yPos = clientArea.Height - height;

					for (int i = numberOfLevels - 1; i >= 0; i--)
					{
						if (i == 0)
							yPos = 0.0f;

						Control ctrl = levelsPanel.Controls[i];
						ctrl.Size = size;
						ctrl.Location = new Point(0, (int)yPos);

						yPos -= height + SpaceBetweenMeters;
					}

					gradientMode = LinearGradientMode.Horizontal;
				}
				else
				{
					// Horizontal layout: meters side by side left to right
					int height = clientArea.Height;
					float width = (clientArea.Width - (numberOfLevels * SpaceBetweenMeters - SpaceBetweenMeters)) / (float)numberOfLevels;
					if (width < 1)
						width = 1;

					size = new Size((int)width, height);
					float xPos = 0.0f;

					for (int i = 0; i < numberOfLevels; i++)
					{
						if (i == numberOfLevels - 1)
							xPos = clientArea.Width - width;

						Control ctrl = levelsPanel.Controls[i];
						ctrl.Size = size;
						ctrl.Location = new Point((int)xPos, 0);

						xPos += width + SpaceBetweenMeters;
					}

					gradientMode = LinearGradientMode.Vertical;
				}

				// Check if the background image need to be recreated
				if ((backgroundBitmap == null) || (backgroundBitmap.Size != size))
				{
					Bitmap oldBackgroundBitmap = backgroundBitmap;

					backgroundBitmap = new Bitmap(size.Width, size.Height);

					using (Graphics bitmapGraphics = Graphics.FromImage(backgroundBitmap))
					{
						ColorBlend blend;

						if (currentOrientation == Orientation.Horizontal)
						{
							// Horizontal mode: Green at bottom (0.0), Red at top (1.0)
							// Gradient goes left to right but will be drawn bottom to top
							blend = new ColorBlend(3)
							{
								Positions = [ 0.0f, 0.15f, 1.0f ],
								Colors = [ Color.Red, Color.Yellow, Color.Green ]
							};
						}
						else
						{
							// Vertical mode: Green at left (0.0), Red at right (1.0)
							blend = new ColorBlend(3)
							{
								Positions = [ 0.0f, 0.85f, 1.0f ],
								Colors = [ Color.Green, Color.Yellow, Color.Red ]
							};
						}

						using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(new Point(0, 0), backgroundBitmap.Size), Color.Black, Color.White, gradientMode))
						{
							brush.InterpolationColors = blend;

							bitmapGraphics.FillRectangle(brush, 0, 0, backgroundBitmap.Width, backgroundBitmap.Height);
						}
					}

					foreach (SpeakerLevelMeterControl ctrl in levelsPanel.Controls)
					{
						ctrl.SetBackgroundBitmap(backgroundBitmap);
						ctrl.SetVerticalMode(currentOrientation == Orientation.Horizontal);  // Horizontal orientation = vertical bars
					}

					// Now it is safe to dispose, because the speaker controls has been updated with the new bitmap
					oldBackgroundBitmap?.Dispose();
				}
			}
		}
		#endregion
	}
}
