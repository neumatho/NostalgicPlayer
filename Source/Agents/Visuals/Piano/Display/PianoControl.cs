/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing.Drawing2D;
using Polycode.NostalgicPlayer.Kit.Containers;
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
			public bool Enabled;
			public bool Active;

			public Point KeyPosition;
			public Color Color;
			public int Alpha;

			public int ReservedIndex1;
			public int ReservedIndex2;

			public short SampleNumber;
			public uint Frequency;
			public uint SampleLength;
			public float CalculatedSamplePosition;

			public bool Retrig;
			public bool Pulsing;
			public bool Direction;

			public int Volume0Counter;
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
			Color.FromArgb(221, 222, 26),			// 10
			Color.FromArgb(239, 149, 42),
			Color.FromArgb(255, 164, 182),
			Color.FromArgb(144, 111, 72),
			Color.FromArgb(207, 181, 63),
			Color.FromArgb(204, 202, 178)			// 15
		};

		private VisualChannelInfo[] visualChannels;
		private bool[,] allocatedKeys;
		private uint[][] noteFrequencies;

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
				visualChannels = ArrayHelper.InitializeArray<VisualChannelInfo>(channels);
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
				noteFrequencies = null;
			}

			Invalidate();
		}



		/********************************************************************/
		/// <summary>
		/// Set the frequencies for all the notes
		/// </summary>
		/********************************************************************/
		public void SetNoteFrequencies(uint[][] frequencies)
		{
			lock (this)
			{
				noteFrequencies = frequencies;
			}
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
		public void ChannelChange(ChannelChanged[] channelChanged)
		{
			lock (this)
			{
				if (visualChannels != null)
				{
					for (int i = 0, cnt = channelChanged.Length; i < cnt; i++)
					{
						if (channelChanged[i] != null)
							UpdateChanges(channelChanged[i], visualChannels[i]);
					}
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
						if (visualChannelInfo.Active && visualChannelInfo.Enabled)
						{
							using (Brush brush = new SolidBrush(Color.FromArgb(visualChannelInfo.Alpha, visualChannelInfo.Color)))
							{
								g.FillEllipse(brush, visualChannelInfo.KeyPosition.X, visualChannelInfo.KeyPosition.Y, 7, 7);

								if (visualChannelInfo.KeyPosition.Y <= FirstHalfNoteYPos)
								{
									using (Pen pen = new Pen(Color.FromArgb(visualChannelInfo.Alpha, Color.LightGray)))
									{
										g.DrawEllipse(pen, visualChannelInfo.KeyPosition.X, visualChannelInfo.KeyPosition.Y, 8, 8);
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
				if (visualChannels != null)
				{
					foreach (VisualChannelInfo visualChannelInfo in visualChannels)
					{
						if (visualChannelInfo.Active)
						{
							if (visualChannelInfo.SampleLength == 0)
								Deactivate(visualChannelInfo);
							else
							{
								if (visualChannelInfo.Retrig)
								{
									visualChannelInfo.Alpha += 50;
									if (visualChannelInfo.Alpha >= 255)
									{
										visualChannelInfo.Alpha = 255;
										visualChannelInfo.Retrig = false;
									}
								}
								else
								{
									if (visualChannelInfo.Pulsing)
									{
										if (visualChannelInfo.Direction)
										{
											visualChannelInfo.Alpha += 6;
											if (visualChannelInfo.Alpha >= 255)
											{
												visualChannelInfo.Alpha = 255;
												visualChannelInfo.Direction = false;
											}
										}
										else
										{
											visualChannelInfo.Alpha -= 6;
											if (visualChannelInfo.Alpha <= 128)
											{
												visualChannelInfo.Alpha = 128;
												visualChannelInfo.Direction = true;
											}
										}
									}
								}

								if (!visualChannelInfo.Pulsing)
								{
									int newAlpha = 255 - (int)((visualChannelInfo.CalculatedSamplePosition * 255) / visualChannelInfo.SampleLength);
									if (visualChannelInfo.Alpha > newAlpha)
										visualChannelInfo.Retrig = false;

									if (!visualChannelInfo.Retrig)
										visualChannelInfo.Alpha = Math.Max(0, newAlpha);

									visualChannelInfo.CalculatedSamplePosition += visualChannelInfo.Frequency * pulseTimer.Interval / 1000.0f;
									if (visualChannelInfo.CalculatedSamplePosition > visualChannelInfo.SampleLength)
									{
										visualChannelInfo.CalculatedSamplePosition = visualChannelInfo.SampleLength;
										Deactivate(visualChannelInfo);
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
		/// Will find out where to place dots on the piano
		/// </summary>
		/********************************************************************/
		private void UpdateChanges(ChannelChanged channelChangedInfo, VisualChannelInfo visualChannelInfo)
		{
			visualChannelInfo.Enabled = channelChangedInfo.Enabled;

			if (channelChangedInfo.Muted)
				Deactivate(visualChannelInfo);
			else
			{
				if (channelChangedInfo.NoteKicked)
				{
					Deactivate(visualChannelInfo);

					if (channelChangedInfo.SampleNumber != -1)
					{
						visualChannelInfo.Active = true;
						visualChannelInfo.SampleNumber = channelChangedInfo.SampleNumber;
						visualChannelInfo.Color = FindColor(visualChannelInfo.SampleNumber);
						visualChannelInfo.Alpha = 0;
						visualChannelInfo.Retrig = true;

						visualChannelInfo.SampleLength = channelChangedInfo.SampleLength;
						visualChannelInfo.CalculatedSamplePosition = 0.0f;
						visualChannelInfo.Pulsing = false;
						visualChannelInfo.Direction = false;
						visualChannelInfo.Volume0Counter = 10;

						if (visualChannelInfo.SampleLength == 0)
							Deactivate(visualChannelInfo);
					}
				}

				if (channelChangedInfo.Looping)
					visualChannelInfo.Pulsing = true;

				if (channelChangedInfo.SamplePosition.HasValue)
				{
					if (channelChangedInfo.SamplePositionRelative)
						visualChannelInfo.CalculatedSamplePosition += channelChangedInfo.SamplePosition.Value;
					else
						visualChannelInfo.CalculatedSamplePosition = channelChangedInfo.SamplePosition.Value;
				}

				if (channelChangedInfo.Frequency.HasValue)
				{
					visualChannelInfo.Frequency = channelChangedInfo.Frequency.Value;

					if ((visualChannelInfo.Frequency != 0) && (visualChannelInfo.SampleNumber != -1))
					{
						Deactivate(visualChannelInfo);

						byte noteNumber = FindNoteNumber(visualChannelInfo.SampleNumber, channelChangedInfo.Frequency.Value, channelChangedInfo.Octave, channelChangedInfo.Note);
						if (noteNumber != byte.MaxValue)
						{
							visualChannelInfo.KeyPosition = FindNotePosition(noteNumber, out var reservedIndexes);
							visualChannelInfo.ReservedIndex1 = reservedIndexes.index1;
							visualChannelInfo.ReservedIndex2 = reservedIndexes.index2;

							if (reservedIndexes.index1 == -1)
								visualChannelInfo.Active = false;
							else
								visualChannelInfo.Active = true;
						}
					}
				}

				if (channelChangedInfo.Volume == 0)
				{
					if (visualChannelInfo.Volume0Counter == 0)
						Deactivate(visualChannelInfo);
					else
						visualChannelInfo.Volume0Counter--;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will find the color to use based on the sample number
		/// </summary>
		/********************************************************************/
		private Color FindColor(short sampleNumber)
		{
			return sampleColors[sampleNumber % 16];
		}



		/********************************************************************/
		/// <summary>
		/// Will convert the given frequency to a note number
		/// </summary>
		/********************************************************************/
		private byte FindNoteNumber(short sampleNumber, uint frequency, sbyte octave, sbyte note)
		{
			if (octave == -1)
			{
				if ((noteFrequencies == null) || (sampleNumber >= noteFrequencies.Length))
					return byte.MaxValue;

				uint[] freqTable = noteFrequencies[sampleNumber];
				if (freqTable != null)
				{
					for (int i = 1; i < freqTable.Length; i++)
					{
						if (frequency <= freqTable[i])
							return (byte)i;
					}
				}

				return byte.MaxValue;
			}

			return (byte)(octave * 12 + note);
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
			if (visualChannelInfo.Active)
			{
				visualChannelInfo.Active = false;

				if ((visualChannelInfo.ReservedIndex1 != -1) && (visualChannelInfo.ReservedIndex2 != -1))
					allocatedKeys[visualChannelInfo.ReservedIndex1, visualChannelInfo.ReservedIndex2] = false;
			}
		}
		#endregion
	}
}
