/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.Sawteeth.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.Sawteeth.Implementation
{
	/// <summary>
	/// Main player
	/// </summary>
	internal class Player
	{
		private readonly byte myChannel;

		private bool looped;
		private bool tmpLoop;

		private readonly SawteethWorker song;
		private readonly ChStep[] step;
		private readonly ChannelInfo ch;
		private readonly InsPly ip;

		private Step currStep;
		private Part currPart;
		private uint seqCount;				// Step in sequencer
		private byte stepC;					// Step in part

		private uint nexts;					// PAL-screen counter;

		private float dAmp;
		private float amp;
		private float ampStep;

		private float freq;
		private float freqStep;
		private float targetFreq;

		private float cutOff;
		private float cutOffStep;

		private readonly float[] buffer;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Player(SawteethWorker s, ChannelInfo chn, byte chanNum)
		{
			myChannel = chanNum;

			song = s;
			step = chn.Steps;
			ch = chn;

			ip = new InsPly(s);
			buffer = new float[s.spsPal];
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the class to begin over again
		/// </summary>
		/********************************************************************/
		public void Init()
		{
			looped = false;
			tmpLoop = false;

			amp = 1.0f;
			ampStep = 0.0f;

			freq = 1.0f;
			freqStep = 1.0f;

			cutOff = 1.0f;
			cutOffStep = 1.0f;

			JumpPos(0, 0, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Generates the next buffer of sound
		/// </summary>
		/********************************************************************/
		public bool NextBuffer()
		{
			if (nexts < 1)
			{
				dAmp = (255.0f - step[seqCount].DAmp) / 255.0f;
				nexts = currPart.Sps;

				currStep = currPart.Steps[stepC];
				if (currStep.Ins != 0)
				{
					ip.TrigAdsr(currStep.Ins);

					amp = 1.0f;
					ampStep = 0.0f;
					cutOff = 1.0f;
					cutOffStep = 1.0f;
				}

				if (currStep.Note != 0)
				{
					targetFreq = song.n2f[currStep.Note + ch.Steps[seqCount].Transp];
					freqStep = 1.0f;

					if ((currStep.Eff & 0xf0) != 0x30)
						freq = targetFreq;
				}

				//
				// Effects
				//
				const float Divider = 50.0f;

				if (currStep.Eff != 0)
				{
					switch (currStep.Eff & 0xf0)
					{
						// Pitch
						case 0x10:
						{
							targetFreq = 44100.0f;
							freqStep = 1 + (((currStep.Eff & 0x0f) + 1) / (Divider * 3));
							break;
						}

						case 0x20:
						{
							targetFreq = 1.0f;
							freqStep = 1 - (((currStep.Eff & 0x0f) + 1) / (Divider * 3));
							break;
						}

						case 0x30:
						{
							if (targetFreq > freq)
								freqStep = 1 + (((currStep.Eff & 0x0f) + 1) / Divider);
							else
								freqStep = 1 - (((currStep.Eff & 0x0f) + 1) / Divider);

							break;
						}

						// PWM
						case 0x40:
						{
							ip.SetPwmOffs((currStep.Eff & 0x0f) / 16.1f);
							break;
						}

						// Resonance
						case 0x50:
						{
							ip.SetReso((currStep.Eff & 0x0f) / 15.0f);
							break;
						}

						// Filter
						case 0x70:
						{
							cutOffStep = 1.0f - ((currStep.Eff & 0x0f) + 1) / 256.0f;
							break;
						}

						case 0x80:
						{
							cutOffStep = 1.0f + ((currStep.Eff & 0x0f) + 1) / 256.0f;
							break;
						}

						case 0x90:
						{
							cutOffStep = 1.0f;
							cutOff = ((currStep.Eff & 0x0f) + 1) / 16.0f;
							cutOff *= cutOff;
							break;
						}

						// Amp
						case 0xa0:
						{
							ampStep = -((currStep.Eff & 0x0f) + 1) / 256.0f;
							break;
						}

						case 0xb0:
						{
							ampStep = ((currStep.Eff & 0x0f) + 1) / 256.0f;
							break;
						}

						case 0xc0:
						{
							ampStep = 0.0f;
							amp = (currStep.Eff & 0x0f) / 15.0f;
							break;
						}
					}
				}

				// Increase counters
				if (tmpLoop)
				{
					looped = true;
					tmpLoop = false;

					// Tell NostalgicPlayer that the song has ended
					if (song.posChannel == myChannel)
						song.EndReached();
				}
				else
					looped = false;

				stepC++;
				if (stepC >= currPart.Len)
				{
					stepC = 0;
					seqCount++;

					// Do only tell NostalgicPlayer about a position change,
					// if we are the position teller channel
					if (song.posChannel == myChannel)
						song.ChangePosition();

					if (seqCount > ch.RLoop)
					{
						seqCount = ch.LLoop;	// Limit
						tmpLoop = true;
					}

					currPart = song.parts[step[seqCount].Part];
				}
			}

			nexts--;

			cutOff *= cutOffStep;
			if (cutOff < 0.0f)
			{
				cutOff = 0.0f;
				cutOffStep = 1.0f;
			}
			else
			{
				if (cutOff > 1.0f)
				{
					cutOff = 1.0f;
					cutOffStep = 1.0f;
				}
			}

			freq *= freqStep;
			if (freqStep > 1.0001f)
			{
				if (freq > targetFreq)
				{
					freq = targetFreq;
					freqStep = 1.0f;
				}
			}
			else
			{
				if (freqStep < 0.9999f)
				{
					if (freq < targetFreq)
					{
						freq = targetFreq;
						freqStep = 1.0f;
					}
				}
			}

			amp += ampStep;
			if (amp < 0.0f)
			{
				amp = 0.0f;
				ampStep = 0.0f;
			}
			else
			{
				if (amp > 1.0f)
				{
					amp = 1.0f;
					ampStep = 0.0f;
				}
			}

			ip.SetAmp(amp * dAmp);
			ip.SetFreq(freq);
			ip.SetCutOff(cutOff);

			return ip.Next(buffer, song.spsPal);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the pointer to the buffer holding the sound
		/// </summary>
		/********************************************************************/
		public float[] Buffer => buffer;



		/********************************************************************/
		/// <summary>
		/// Returns whether the sound has looped or not
		/// </summary>
		/********************************************************************/
		public bool Looped
		{
			get
			{
				if (looped)
				{
					looped = false;
					return true;
				}

				return false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Returns the current sequencer position
		/// </summary>
		/********************************************************************/
		public uint GetSeqPos()
		{
			return seqCount;
		}



		/********************************************************************/
		/// <summary>
		/// Jumps to the specific position given
		/// </summary>
		/********************************************************************/
		public void JumpPos(byte seqPos, byte stepPos, byte pal)
		{
			stepC = stepPos;
			seqCount = seqPos;
			nexts = 0;

			currPart = song.parts[step[seqCount].Part];
			currStep = currPart.Steps[stepC];

			dAmp = (255.0f - step[seqCount].DAmp) / 255.0f;

			if (pal > 0)
			{
				// Increase counters
				stepC++;

				if (stepC >= currPart.Len)
				{
					stepC = 0;
					seqCount++;

					if (seqCount >= ch.Len)
						seqCount = 0;

					currPart = song.parts[step[seqCount].Part];
				}

				nexts = (uint)(currPart.Sps - pal);
				currStep = currPart.Steps[stepC];
			}
		}
	}
}
