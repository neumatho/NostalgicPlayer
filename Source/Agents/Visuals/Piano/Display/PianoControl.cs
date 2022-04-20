/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Drawing.Drawing2D;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Visual.Piano.Display
{
	/// <summary>
	/// User control that shows the piano
	/// </summary>
	public partial class PianoControl : UserControl
	{
		private class VisualChannelInfo
		{
			public bool enabled;
			public bool active;

			public Point keyPosition;
			public Color color;
			public int alpha;

			public int reservedIndex1;
			public int reservedIndex2;

			public uint frequency;
			public uint sampleLength;
			public float calculatedSamplePosition;

			public bool pulsing;
			public bool direction;
		}

		private const int NumberOfOctaves = 10;

		private const int FirstNoteYPos = 124;
		private const int FirstHalfNoteYPos = 96;

		private static readonly Point[] octavePositions =
		{
			new Point(8, FirstNoteYPos), new Point(14, FirstHalfNoteYPos),
			new Point(23, FirstNoteYPos), new Point(32, FirstHalfNoteYPos),
			new Point(38, FirstNoteYPos),
			new Point(53, FirstNoteYPos), new Point(59, FirstHalfNoteYPos),
			new Point(68, FirstNoteYPos), new Point(75, FirstHalfNoteYPos),
			new Point(83, FirstNoteYPos), new Point(91, FirstHalfNoteYPos),
			new Point(97, FirstNoteYPos)
		};

		private static readonly Color[] sampleColors =
		{
			Color.FromArgb(246, 15, 66),			// 0
			Color.FromArgb(207, 64, 84),
			Color.FromArgb(246, 20, 162),
			Color.FromArgb(138, 87, 184),
			Color.FromArgb(190, 190, 190),
			Color.FromArgb(46, 117, 188),			// 5
			Color.FromArgb(63, 173, 238),
			Color.FromArgb(60, 167, 156),
			Color.FromArgb(52, 165, 81),
			Color.FromArgb(145, 197, 63),
			Color.FromArgb(251, 242, 26),			// 10
			Color.FromArgb(239, 149, 42),
			Color.FromArgb(255, 164, 182),
			Color.FromArgb(144, 111, 72),
			Color.FromArgb(207, 181, 63),
			Color.FromArgb(204, 202, 178)			// 15
		};

		private VisualChannelInfo[] visualChannels;
		private bool[,] allocatedKeys;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PianoControl()
		{
			InitializeComponent();

			SetStyle(ControlStyles.UserPaint, true);
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
				visualChannels = Helpers.InitializeArray<VisualChannelInfo>(channels);
				allocatedKeys = new bool[NumberOfOctaves * 12, 3];
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
				visualChannels = null;
				allocatedKeys = null;
			}

			Invalidate();
		}



		/********************************************************************/
		/// <summary>
		/// Set the pause state
		/// </summary>
		/********************************************************************/
		public void SetPauseState(bool paused)
		{
			if (paused)
				pulseTimer.Stop();
			else
				pulseTimer.Start();
		}



		/********************************************************************/
		/// <summary>
		/// Tell the visual about a channel change
		/// </summary>
		/********************************************************************/
		public void ChannelChange(ChannelChanged channelChanged)
		{
			lock (this)
			{
				if (visualChannels != null)
				{
					for (int i = 0, cnt = channelChanged.VirtualChannels.Length; i < cnt; i++)
						UpdateChanges(channelChanged.Flags[i], channelChanged.VirtualChannels[i], i >= channelChanged.EnabledChannels.Length ? true : channelChanged.EnabledChannels[i], visualChannels[i]);
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



		/********************************************************************/
		/// <summary>
		/// Is called 50 times per second and do the animation
		/// </summary>
		/********************************************************************/
		private void PulseTimer_Tick(object sender, EventArgs e)
		{
			Animate();
			Invalidate();
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
				// First draw the top of the piano
				using (LinearGradientBrush brush = new LinearGradientBrush(new Point(0, 0), new Point(0, 60), Color.FromArgb(0x55, 0x55, 0x55), Color.Black))
				{
					g.FillRectangle(brush, 4, 4, Width - 8, 50);
				}

				// Now draw the keys for all the octaves
				int toAdd = Resources.IDB_OCTAVE.Width - 2;
				for (int i = 0, x = 4; i < NumberOfOctaves; i++, x += toAdd)
					g.DrawImage(Resources.IDB_OCTAVE, new Point(x, 54));

				if (visualChannels != null)
				{
					foreach (VisualChannelInfo visualChannelInfo in visualChannels)
					{
						if (visualChannelInfo.active && visualChannelInfo.enabled)
						{
							using (Brush brush = new SolidBrush(Color.FromArgb(visualChannelInfo.alpha, visualChannelInfo.color)))
							{
								g.FillEllipse(brush, visualChannelInfo.keyPosition.X, visualChannelInfo.keyPosition.Y, 7, 7);

								if (visualChannelInfo.keyPosition.Y <= FirstHalfNoteYPos)
								{
									using (Pen pen = new Pen(Color.FromArgb(visualChannelInfo.alpha, Color.LightGray)))
									{
										g.DrawEllipse(pen, visualChannelInfo.keyPosition.X, visualChannelInfo.keyPosition.Y, 8, 8);
									}
								}
							}
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will animate the dots
		/// </summary>
		/********************************************************************/
		private void Animate()
		{
			lock (this)
			{
				foreach (VisualChannelInfo visualChannelInfo in visualChannels)
				{
					if (visualChannelInfo.active)
					{
						if (visualChannelInfo.pulsing)
						{
							if (visualChannelInfo.direction)
							{
								visualChannelInfo.alpha += 6;
								if (visualChannelInfo.alpha >= 255)
									visualChannelInfo.direction = false;
							}
							else
							{
								visualChannelInfo.alpha -= 6;
								if (visualChannelInfo.alpha < 128)
									visualChannelInfo.direction = true;
							}
						}
						else
						{
							if (visualChannelInfo.sampleLength == 0)
								Deactivate(visualChannelInfo);
							else
							{
								visualChannelInfo.alpha = 255 - (int)((visualChannelInfo.calculatedSamplePosition * 255) / visualChannelInfo.sampleLength);

								visualChannelInfo.calculatedSamplePosition += visualChannelInfo.frequency * pulseTimer.Interval / 1000.0f;
								if (visualChannelInfo.calculatedSamplePosition > visualChannelInfo.sampleLength)
								{
									visualChannelInfo.calculatedSamplePosition = visualChannelInfo.sampleLength;
									Deactivate(visualChannelInfo);
								}
							}
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will find out where to place dots on the piano
		/// </summary>
		/********************************************************************/
		private void UpdateChanges(ChannelFlags flags, IChannel chan, bool enabled, VisualChannelInfo visualChannelInfo)
		{
			visualChannelInfo.enabled = enabled;

			if ((flags & ChannelFlags.MuteIt) != 0)
				Deactivate(visualChannelInfo);
			else
			{
				if ((flags & ChannelFlags.Visual) != 0)
				{
					Deactivate(visualChannelInfo);

					VisualInfo visualInfo = chan.GetVisualInfo();

					visualChannelInfo.active = true;
					visualChannelInfo.color = FindColor(visualInfo.SampleNumber);
					visualChannelInfo.alpha = 255;
					visualChannelInfo.direction = false;
					visualChannelInfo.keyPosition = FindNotePosition(visualInfo.NoteNumber, out var reservedIndexes);
					visualChannelInfo.reservedIndex1 = reservedIndexes.index1;
					visualChannelInfo.reservedIndex2 = reservedIndexes.index2;

					if (reservedIndexes.index1 == -1)
						visualChannelInfo.active = false;
				}

				if ((flags & ChannelFlags.TrigIt) != 0)
				{
					visualChannelInfo.sampleLength = chan.GetSampleLength();
					visualChannelInfo.calculatedSamplePosition = 0.0f;
					visualChannelInfo.pulsing = false;
					visualChannelInfo.direction = false;

					if (visualChannelInfo.sampleLength == 0)
						Deactivate(visualChannelInfo);
				}

				if ((flags & ChannelFlags.Loop) != 0)
					visualChannelInfo.pulsing = true;

				if ((flags & ChannelFlags.ChangePosition) != 0)
				{
					if ((flags & ChannelFlags.Relative) != 0)
						visualChannelInfo.calculatedSamplePosition += chan.GetSamplePosition();
					else
						visualChannelInfo.calculatedSamplePosition = chan.GetSamplePosition();
				}

				if ((flags & ChannelFlags.Frequency) != 0)
					visualChannelInfo.frequency = chan.GetFrequency();

				if (chan.GetVolume() == 0)
					Deactivate(visualChannelInfo);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will find the color to use based on the sample number
		/// </summary>
		/********************************************************************/
		private Color FindColor(ushort sampleNumber)
		{
			return sampleColors[sampleNumber % 16];
		}



		/********************************************************************/
		/// <summary>
		/// Will find the position to use based on the note number
		/// </summary>
		/********************************************************************/
		private Point FindNotePosition(byte noteNumber, out (int index1, int index2) reservedIndexes)
		{
			for (int i = 0; i < 3; i++)
			{
				if (!allocatedKeys[noteNumber, i])
				{
					allocatedKeys[noteNumber, i] = true;
					reservedIndexes = (noteNumber, i);

					int octave = noteNumber / 12;
					int note = noteNumber % 12;

					Point octavePoint = octavePositions[note];

					return new Point(octavePoint.X + octave * 104, octavePoint.Y - (i * 9));
				}
			}

			reservedIndexes = (-1, -1);

			return Point.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Deactivate channel
		/// </summary>
		/********************************************************************/
		private void Deactivate(VisualChannelInfo visualChannelInfo)
		{
			if (visualChannelInfo.active)
			{
				visualChannelInfo.active = false;

				allocatedKeys[visualChannelInfo.reservedIndex1, visualChannelInfo.reservedIndex2] = false;
			}
		}
		#endregion
	}
}
