/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.GuiKit.Components;

namespace Polycode.NostalgicPlayer.Agent.Visual.LevelMeter.Display
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

		private readonly float levelDecay;
		private readonly float peakDecay;

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
					speakerLevelDb = GainToDecibels(speakerLevel);
				else
					speakerLevelDb = ClampDb;

				if (speakerLevel > speakerPeak)
				{
					speakerPeak = speakerLevel;
					speakerPeakDb = speakerLevelDb;
					speakerPeakHold = HoldMax;
				}
				else if (speakerPeakHold > 0)
					speakerPeakHold--;
				else if (speakerPeakDb > ClampDb)
				{
					speakerPeak += (speakerLevel - speakerPeak) * peakDecay;

					if (speakerPeak > ClampLevel)
						speakerPeakDb = GainToDecibels(speakerPeak);
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

					// Draw the speaker name
					g.DrawString(speaker, Font, Brushes.Black, 4, (Height - Font.Height) / 2);
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
				int endPos = PositionForLevel(speakerLevelDb);

				// Draw the level
				g.DrawImage(backgroundBitmap, new Rectangle(0, 0, Width - endPos, Height), new Rectangle(0, 0, Width - endPos, Height), GraphicsUnit.Pixel);
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
				g.FillRectangle(Brushes.Black, Width - endPos - 3, 0, 3, Height);
			}
		}



		/********************************************************************/
		/// <summary> 
		/// Convert a dB value to pixel value
		/// </summary>
		/********************************************************************/
		private int PositionForLevel(float dbLevel)
		{
			return (int)Math.Round(Map(dbLevel, MaxDb, MinDb, 0, Width), MidpointRounding.AwayFromZero);
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
