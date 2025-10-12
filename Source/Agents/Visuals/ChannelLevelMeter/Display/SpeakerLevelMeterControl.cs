/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Kit.Gui.Components;

namespace Polycode.NostalgicPlayer.Agent.Visual.ChannelLevelMeter.Display
{
	/// <summary>
	/// This control shows the volume level for a single speaker
	/// </summary>
	internal partial class SpeakerLevelMeterControl : UserControl
	{
		private const int RefreshRate = 50;
		private const int HoldMax = RefreshRate * 2;

		private const float ClampDb = -120.0f;
		private const float ClampLevel = 0.000001f;	// -120 dB

		private const float DefaultMinusInfinityDb = -100.0f;

		private const float MaxDb = 0.0f;
		private const float MinDb = -60.0f;

		private Bitmap backgroundBitmap;
		private readonly string speaker;
		private bool isVerticalMode;  // true = bars fill from bottom to top, false = bars fill from left to right

		private readonly float levelDecay;
		private readonly float peakDecay;

		// Text size caching
		private float textWidthFull;   // Width of "CH01"
		private float textWidthShort;  // Width of "01"
		private float textHeight;      // Height of text

		private float measuredLevel;

		private float speakerLevel;
		private float speakerLevelDb;
		private float speakerPeak;
		private float speakerPeakDb;
		private int speakerPeakHold;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SpeakerLevelMeterControl(string speakerName)
		{
			InitializeComponent();

			Font = FontPalette.GetRegularFont();

			SetStyle(ControlStyles.UserPaint, true);

			speaker = speakerName;

			levelDecay = 1.0f - (float)Math.Exp(-1.0f / (RefreshRate * 0.2f));
			peakDecay = 1.0f - (float)Math.Exp(-1.0f / (RefreshRate * 0.5f));

			measuredLevel = 0.0f;

			speakerLevel = 0.0f;
			speakerLevelDb = ClampDb;
			speakerPeak = 0.0f;
			speakerPeakDb = ClampDb;
			speakerPeakHold = 0;

			// Pre-calculate text sizes
			using (Graphics g = CreateGraphics())
			{
				SizeF sizeFull = g.MeasureString(speaker, Font);
				textWidthFull = sizeFull.Width;
				textHeight = sizeFull.Height;

				string shortText = speaker.Length > 2 ? speaker.Substring(2) : speaker;
				textWidthShort = g.MeasureString(shortText, Font).Width;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set the background image to use
		/// </summary>
		/********************************************************************/
		public void SetBackgroundBitmap(Bitmap bitmap)
		{
			lock (this)
			{
				backgroundBitmap = bitmap;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set whether bars fill vertically (bottom to top) or horizontally (left to right)
		/// </summary>
		/********************************************************************/
		public void SetVerticalMode(bool vertical)
		{
			lock (this)
			{
				isVerticalMode = vertical;
			}
		}



		/********************************************************************/
		/// <summary> 
		/// Update to a new level
		/// </summary>
		/********************************************************************/
		public void UpdateLevel(long level)
		{
			if (level > int.MaxValue)
				level = int.MaxValue;

			float newLevel = (float)level / int.MaxValue;

			lock (this)
			{
				if (newLevel > measuredLevel)
					measuredLevel = newLevel;
			}
		}



		/********************************************************************/
		/// <summary> 
		/// Animate internal variables to the next frame
		/// </summary>
		/********************************************************************/
		public void Animate()
		{
			lock (this)
			{
				float newLevel = measuredLevel;
				measuredLevel = 0.0f;

				if (newLevel > speakerLevel)
					speakerLevel = newLevel;
				else
					speakerLevel += (newLevel - speakerLevel) * levelDecay;

				if (speakerLevel > ClampLevel)
					speakerLevelDb = Math.Min(GainToDecibels(speakerLevel), MaxDb);  // Clamp to 0 dB max
				else
					speakerLevelDb = ClampDb;

				if (speakerLevel > speakerPeak)
				{
					speakerPeak = speakerLevel;
					speakerPeakDb = Math.Min(speakerLevelDb, MaxDb);  // Clamp to 0 dB max
					speakerPeakHold = HoldMax;
				}
				else if (speakerPeakHold > 0)
					speakerPeakHold--;
				else if (speakerPeakDb > ClampDb)
				{
					speakerPeak += (speakerLevel - speakerPeak) * peakDecay;

					if (speakerPeak > ClampLevel)
						speakerPeakDb = Math.Min(GainToDecibels(speakerPeak), MaxDb);  // Clamp to 0 dB max
					else
						speakerPeakDb = ClampDb;
				}
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
				// First clear the background
				using (Brush brush = new SolidBrush(BackColor))
				{
					g.FillRectangle(brush, 0, 0, Width, Height);
				}

				if (backgroundBitmap != null)
				{
					DrawLevel(g);
					DrawPeak(g);

					// Draw the speaker name - decide what fits
					string displayText = null;
					float textWidth = 0;

					if (isVerticalMode)
					{
						// isVerticalMode = true means Horizontal layout (meters side by side)
						// Each meter has small width, bars fill vertically (bottom to top)
						// Text is horizontal in middle - check if WIDTH is enough
						if (textWidthFull + 8 <= Width)  // 8px padding (4px each side)
						{
							displayText = speaker;
							textWidth = textWidthFull;
						}
						else if (textWidthShort + 8 <= Width)
						{
							displayText = speaker.Length > 2 ? speaker.Substring(2) : speaker;
							textWidth = textWidthShort;
						}
						// else: don't draw text
					}
					else
					{
						// isVerticalMode = false means Vertical layout (meters stacked top to bottom)
						// Each meter has full width but small height
						// Width is always full, so text always fits horizontally - only check HEIGHT
						if (Height >= textHeight + 4)  // Need at least text height + padding
						{
							displayText = speaker;
							textWidth = textWidthFull;
						}
						// else: don't draw text (height too small)
					}

					// Draw text if it fits
					if (displayText != null)
					{
						if (isVerticalMode)
						{
							// Vertical mode: centered horizontally at bottom
							float x = (Width - textWidth) / 2;
							float y = Height - Font.Height - 2;
							g.DrawString(displayText, Font, Brushes.Black, x, y);
						}
						else
						{
							// Horizontal mode: centered horizontally in middle
							float x = (Width - textWidth) / 2;
							float y = (Height - Font.Height) / 2;
							g.DrawString(displayText, Font, Brushes.Black, x, y);
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the level
		/// </summary>
		/********************************************************************/
		private void DrawLevel(Graphics g)
		{
			if (speakerLevel > 0.0f)
			{
				// Find how much that need to be filled
				// endPos is the distance from max: 0 = max volume, size = silence
				int endPos = PositionForLevel(speakerLevelDb);

				if (isVerticalMode)
				{
					// Vertical mode: bars fill from bottom to top
					// Draw from bottom: Height - (Height - endPos) = endPos pixels from bottom
					int fillHeight = Height - endPos;
					int yStart = Height - fillHeight;
					g.DrawImage(backgroundBitmap, new Rectangle(0, yStart, Width, fillHeight), new Rectangle(0, yStart, Width, fillHeight), GraphicsUnit.Pixel);
				}
				else
				{
					// Horizontal mode: bars fill from left to right
					int fillWidth = Width - endPos;
					g.DrawImage(backgroundBitmap, new Rectangle(0, 0, fillWidth, Height), new Rectangle(0, 0, fillWidth, Height), GraphicsUnit.Pixel);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the peak
		/// </summary>
		/********************************************************************/
		private void DrawPeak(Graphics g)
		{
			if (speakerPeakDb > ClampDb)
			{
				int endPos = PositionForLevel(speakerPeakDb);

				if (isVerticalMode)
				{
					// Vertical mode: peak bar at top of level (horizontal bar)
					int fillHeight = Height - endPos;
					int yPos = Height - fillHeight;
					g.FillRectangle(Brushes.Black, 0, yPos, Width, 3);
				}
				else
				{
					// Horizontal mode: peak bar at right of level (vertical bar)
					int fillWidth = Width - endPos;
					g.FillRectangle(Brushes.Black, fillWidth - 3, 0, 3, Height);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Convert a dB value to pixel value
		/// </summary>
		/********************************************************************/
		private int PositionForLevel(float dbLevel)
		{
			int size = isVerticalMode ? Height : Width;
			return (int)Math.Round(Map(dbLevel, MaxDb, MinDb, 0, size), MidpointRounding.AwayFromZero);
		}



		/********************************************************************/
		/// <summary> 
		/// Converts a gain level into a dBFS value.
		///
		/// A gain of 1.0 = 0 dB, and lower gains map onto negative decibel
		/// values. If the gain is 0 (or negative), then the method will
		/// return the value provided as minusInfinityDb
		/// </summary>
		/********************************************************************/
		private float GainToDecibels(float gain, float minusInfinityDb = DefaultMinusInfinityDb)
		{
			return gain > 0.0f ? Math.Max(minusInfinityDb, (float)(Math.Log10(gain)) * 20.0f) : minusInfinityDb;
		}



		/********************************************************************/
		/// <summary> 
		/// Remaps a value from a source range to a target range
		/// </summary>
		/********************************************************************/
		private float Map(float sourceValue, float sourceRangeMin, float sourceRangeMax, float targetRangeMin, float targetRangeMax)
		{
			return targetRangeMin + ((targetRangeMax - targetRangeMin) * (sourceValue - sourceRangeMin)) / (sourceRangeMax - sourceRangeMin);
		}
		#endregion
	}
}
