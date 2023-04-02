/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Player.Sawteeth.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.Sawteeth.Implementation
{
	/// <summary>
	/// Instrument player
	/// </summary>
	internal class InsPly : IDeepCloneable<InsPly>
	{
		// Clipping
		private Lfo vib;
		private Lfo pwm;
		private float vAmp;
		private float pAmp;
		private float pwmOffs;

		private readonly SawteethWorker song;
		private Wave w;
		private readonly Ins[] ins;
		private InsStep currStep;
		private byte stepC;					// Instrument step
		private int nextS;					// Next ins

		private Ins currIns;
		private InsStep[] steps;

		// Amp ADSR
		private bool trigged;
		private float currAmp;
		private float ampStep;
		private sbyte adsr;
		private int nextAdsr;

		// Filter ADSR
		private float currF;
		private float fStep;
		private sbyte fAdsr;
		private int nextFAdsr;

		// From ins pattern
		private float insFreq;

		// From player
		private float currPartFreq;
		private float currPartAmp;
		private float currPartCo;

		// Curr
		private float res;
		private float amp;
		private float cutOff;

		// For filter
		private float lo;
		private float hi;
		private float bp;
		private float bs;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public InsPly(SawteethWorker s)
		{
			vib = new Lfo();
			pwm = new Lfo();
			w = new Wave();

			song = s;
			ins = song.ins;
			currIns = ins[0];
			steps = currIns.Steps;

			insFreq = 440.0f;
			currPartFreq = 440.0f;

			pwmOffs = 0.0f;
			amp = 0.0f;
			cutOff = 0.0f;
			res = 0.0f;

			currPartAmp = 0.0f;
			currPartCo = 0.0f;
			currAmp = 0.0f;
			currF = 0.0f;

			lo = 0.0f;
			hi = 0.0f;
			bp = 0.0f;
			bs = 0.0f;
			ampStep = 0.0f;
			fStep = 0.0f;

			TrigAdsr(0);
		}



		/********************************************************************/
		/// <summary>
		/// Trigger ADSR
		/// </summary>
		/********************************************************************/
		public void TrigAdsr(byte i)
		{
			pwmOffs = 0.0f;
			trigged = true;
			currIns = ins[i];
			steps = currIns.Steps;

			vib.SetFreq(currIns.VibS * 50.0f);
			vAmp = currIns.VibD / 2000.0f;

			pwm.SetFreq(currIns.PwmS * 5.0f);
			pAmp = currIns.PwmD / 255.1f;

			SetReso(currIns.Res / 255.0f);

			// Amp ADSR
			adsr = 0;
			nextAdsr = 0;

			// Filter ADSR
			fAdsr = 0;
			nextFAdsr = 0;

			// Step
			stepC = 0;
			nextS = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Set PWM offset
		/// </summary>
		/********************************************************************/
		public void SetPwmOffs(float a)
		{
			pwmOffs = a;
		}



		/********************************************************************/
		/// <summary>
		/// Set resonance
		/// </summary>
		/********************************************************************/
		public void SetReso(float a)
		{
			res = 1.0f - a;
			res *= res;
			res = 1.0f - res;
		}



		/********************************************************************/
		/// <summary>
		/// Set amplitude
		/// </summary>
		/********************************************************************/
		public void SetAmp(float a)
		{
			currPartAmp = a;
		}



		/********************************************************************/
		/// <summary>
		/// Set frequency
		/// </summary>
		/********************************************************************/
		public void SetFreq(float f)
		{
			currPartFreq = f;
		}



		/********************************************************************/
		/// <summary>
		/// Set cut off
		/// </summary>
		/********************************************************************/
		public void SetCutOff(float co)
		{
			currPartCo = co;
		}



		/********************************************************************/
		/// <summary>
		/// Fill buffer with playing instrument samples
		/// </summary>
		/********************************************************************/
		public bool Next(float[] buffer, uint count)
		{
			// Check ins step
			if (nextS < 1)
			{
				currStep = steps[stepC];
				nextS = currIns.Sps;

				if (currStep.Note != 0)
				{
					if (currStep.Relative)
						insFreq = song.r2f[currStep.Note];
					else
						insFreq = song.n2f[currStep.Note];
				}

				if (currStep.WForm != 0)
					w.SetForm((WaveForm)currStep.WForm);

				stepC++;

				if (stepC >= currIns.Len)
					stepC = currIns.Loop;
			}

			// Check ADSR
			if (nextAdsr < 1)
			{
				if (adsr < currIns.AmpPoints)
				{
					nextAdsr = 1 + currIns.Amp[adsr].Time;
					ampStep = ((currIns.Amp[adsr].Lev / 257.0f) - currAmp) / nextAdsr;
					adsr++;
				}
				else
					ampStep = 0.0f;
			}

			// Check filter ADSR
			if (nextFAdsr < 1)
			{
				if (fAdsr < currIns.FilterPoints)
				{
					nextFAdsr = 1 + currIns.Filter[fAdsr].Time;
					float target = currIns.Filter[fAdsr].Lev / 257.0f;
					target *= target * target;
					fStep = (target - currF) / nextFAdsr;
					fAdsr++;
				}
				else
					fStep = 0.0f;
			}

			// ADSR
			nextAdsr--;
			nextFAdsr--;

			// Step
			nextS--;

			// Freq
			if (currStep.Relative)
				w.SetFreq(currPartFreq * insFreq * (1.0f + vAmp * vib.Next()));
			else
				w.SetFreq(insFreq * (1.0f + vAmp * vib.Next()));

			// PWM
			w.SetPwm(pwmOffs + pAmp * pwm.Next());

			// Amp
			currAmp += ampStep;
			amp = currAmp * currPartAmp;

			// Filter
			currF += fStep;
			cutOff = currF * currPartCo;

			// Filter part
			w.SetAmp(amp);

			if (trigged)
			{
				trigged = false;
				w.NoInterp();
			}

			if (!w.Next(buffer, count))
				return false;

			switch (currIns.FilterMode)
			{
				case 1:
				{
					Slp(buffer, count);
					break;
				}

				case 2:
				{
					Olp(buffer, count);
					break;
				}

				case 3:
				{
					Lp(buffer, count);
					break;
				}

				case 4:
				{
					Hp(buffer, count);
					break;
				}

				case 5:
				{
					Bp(buffer, count);
					break;
				}

				case 6:
				{
					Bs(buffer, count);
					break;
				}
			}

			switch (currIns.ClipMode)
			{
				case 0x1:
				{
					VanillaClip(buffer, count, 2.0f * (1 + currIns.Boost));
					break;
				}

				case 0x2:
				{
					SinusClip(buffer, count, 0.7f * (1.3f + currIns.Boost));
					break;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public InsPly MakeDeepClone()
		{
			InsPly clone = (InsPly)MemberwiseClone();

			clone.vib = vib.MakeDeepClone();
			clone.pwm = pwm.MakeDeepClone();
			clone.w = w.MakeDeepClone();

			return clone;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// SLP
		/// </summary>
		/********************************************************************/
		private void Slp(float[] b, uint count)
		{
			for (uint i = 0; i < count; i++)
			{
				float f = b[i];
				lo = (cutOff * f) + (lo * (1.0f - cutOff));
				b[i] = lo;
			}
		}



		/********************************************************************/
		/// <summary>
		/// OLP
		/// </summary>
		/********************************************************************/
		private void Olp(float[] b, uint count)
		{
			for (uint i = 0; i < count; i++)
			{
				lo = cutOff * b[i] + (1.0f - cutOff) * hi;
				lo += (lo - bp) * res;

				bp = hi;
				hi = lo;

				b[i] = lo;
			}
		}



		/********************************************************************/
		/// <summary>
		/// LP
		/// </summary>
		/********************************************************************/
		private void Lp(float[] b, uint count)
		{
			for (uint i = 0; i < count; i++)
			{
				float f = b[i];
				float t = lo + cutOff * bp;
				hi = f - lo - (1.8f - res * 1.8f) * bp;
				bp += cutOff * hi;

				if (t < -amp)
					lo = -amp;
				else
				{
					if (t > amp)
						lo = amp;
					else
						lo = t;
				}

				bs = lo + hi;
				b[i] = lo;
			}
		}



		/********************************************************************/
		/// <summary>
		/// HP
		/// </summary>
		/********************************************************************/
		private void Hp(float[] b, uint count)
		{
			for (uint i = 0; i < count; i++)
			{
				float f = b[i];
				float t = lo + cutOff * bp;
				hi = f - lo - (1.8f - res * 1.8f) * bp;
				bp += cutOff * hi;

				if (t < -amp)
					lo = -amp;
				else
				{
					if (t > amp)
						lo = amp;
					else
						lo = t;
				}

				bs = lo + hi;
				b[i] = hi;
			}
		}



		/********************************************************************/
		/// <summary>
		/// BP
		/// </summary>
		/********************************************************************/
		private void Bp(float[] b, uint count)
		{
			for (uint i = 0; i < count; i++)
			{
				float f = b[i];
				float t = lo + cutOff * bp;
				hi = f - lo - (1.8f - res * 1.8f) * bp;
				bp += cutOff * hi;

				if (t < -amp)
					lo = -amp;
				else
				{
					if (t > amp)
						lo = amp;
					else
						lo = t;
				}

				bs = lo + hi;
				b[i] = bp;
			}
		}



		/********************************************************************/
		/// <summary>
		/// BS
		/// </summary>
		/********************************************************************/
		private void Bs(float[] b, uint count)
		{
			for (uint i = 0; i < count; i++)
			{
				float f = b[i];
				float t = lo + cutOff * bp;
				hi = f - lo - (1.8f - res * 1.8f) * bp;
				bp += cutOff * hi;

				if (t < -amp)
					lo = -amp;
				else
				{
					if (t > amp)
						lo = amp;
					else
						lo = t;
				}

				bs = lo + hi;
				b[i] = bs;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Vanilla clip
		/// </summary>
		/********************************************************************/
		private void VanillaClip(float[] b, uint count, float mul)
		{
			if (Math.Abs(mul - 1.0f) < 0.1f)
			{
				for (uint i = 0; i < count; i++)
				{
					if (b[i] > 1.0f)
						b[i] = 1.0f;
					else
					{
						if (b[i] < -1.0f)
							b[i] = -1.0f;
					}
				}
			}
			else
			{
				for (uint i = 0; i < count; i++)
				{
					b[i] *= mul;

					if (b[i] > 1.0f)
						b[i] = 1.0f;
					else
					{
						if (b[i] < -1.0f)
							b[i] = -1.0f;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Sinus clip
		/// </summary>
		/********************************************************************/
		private void SinusClip(float[] b, uint count, float mul)
		{
			if (Math.Abs(mul - 1.0f) < 0.1f)
			{
				for (uint i = 0; i < count; i++)
					b[i] = (float)Math.Sin(b[i]);
			}
			else
			{
				for (uint i = 0; i < count; i++)
				{
					b[i] *= mul;
					b[i] = (float)Math.Sin(b[i] * mul);
				}
			}
		}
		#endregion
	}
}
