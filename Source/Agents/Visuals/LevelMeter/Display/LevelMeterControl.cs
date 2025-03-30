/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ABI.System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;

namespace Polycode.NostalgicPlayer.Agent.Visual.LevelMeter.Display
{
	/// <summary>
	/// The user control showing the level meter
	/// </summary>
	internal partial class LevelMeterControl : UserControl
	{
		private const int SpaceBetweenMeters = 4;

		private Bitmap backgroundBitmap;

		private SpeakerFlag speakers;

		private static readonly (SpeakerFlag, string)[] speakerOrder =
		[
			(SpeakerFlag.FrontLeft, "FL"), (SpeakerFlag.FrontCenter, "FC"), (SpeakerFlag.FrontRight, "FR"),
			(SpeakerFlag.SideLeft, "SL"), (SpeakerFlag.SideRight, "SR"),
			(SpeakerFlag.BackLeft, "BL"), (SpeakerFlag.BackRight, "BR"),
			(SpeakerFlag.LowFrequency, "LFE")
		];

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
		public void InitVisual(SpeakerFlag speakersToShow)
		{
			lock (this)
			{
				speakers = speakersToShow;

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
			long[] maxLevels = new long[sampleData.ChannelMapping.Count];

			int[] sample = sampleData.SampleData;
			int increment = sampleData.ChannelCount;

			int controlIndex = 0;

			foreach ((SpeakerFlag speaker, _) in speakerOrder)
			{
				if (speakers.HasFlag(speaker))
				{
					int channelIndex = sampleData.ChannelMapping[speaker];
					long max = 0;

					for (int i = channelIndex; i < sampleData.SampleData.Length; i += increment)
						max = Math.Max(max, Math.Abs((long)sample[i]));

					maxLevels[controlIndex++] = max;
				}
			}

			lock (this)
			{
				for (int i = 0; i < levelsPanel.Controls.Count; i++)
					((SpeakerLevelMeterControl)levelsPanel.Controls[i]).UpdateLevel(maxLevels[i]);
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
			foreach ((SpeakerFlag speaker, string name) in speakerOrder)
			{
				if (speakers.HasFlag(speaker))
				{
					SpeakerLevelMeterControl ctrl = new SpeakerLevelMeterControl(name);
					levelsPanel.Controls.Add(ctrl);
				}
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
				int width = clientArea.Width;
				int height = (clientArea.Height - (numberOfLevels * SpaceBetweenMeters - SpaceBetweenMeters)) / numberOfLevels;
				if (height < 24)
					height = 24;

				Size size = new Size(width, height);
				int yPos = 0;

				foreach (Control ctrl in levelsPanel.Controls)
				{
					ctrl.Size = size;
					ctrl.Location = new Point(0, yPos);

					yPos += height + SpaceBetweenMeters;
				}

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

					foreach (SpeakerLevelMeterControl ctrl in levelsPanel.Controls)
						ctrl.SetBackgroundBitmap(backgroundBitmap);

					// Now it is safe to dispose, because the speaker controls has been updated with the new bitmap
					oldBackgroundBitmap?.Dispose();
				}
			}
		}
		#endregion
	}
}
