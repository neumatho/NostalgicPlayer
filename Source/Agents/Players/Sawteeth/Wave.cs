/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Player.Sawteeth.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.Sawteeth
{
	/// <summary>
	/// Wave generator
	/// </summary>
	internal class Wave
	{
		private const float PwmLim = 0.9f;

		private WaveForm form;

		private bool _pwmLo;
		private float _amp;
		private float fromAmp;

		private float pwm;

		private float currVal;

		private float curr;
		private float step;

		private float noiseVal;

		private uint sinCurrVal;
		private uint sinStep;

		private static bool sIsInit;
		private static readonly float[] sint = new float[513];
		private static readonly float[] trit = new float[513];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Wave()
		{
			SInit();

			fromAmp = 0.0f;
			noiseVal = 0.0f;
			curr = 0.0f;
			pwm = 0.0f;
			step = 0.0f;
			currVal = 0.0f;

			_pwmLo = false;

			SetForm(WaveForm.Saw);
			SetFreq(440);
		}



		/********************************************************************/
		/// <summary>
		/// Set frequency
		/// </summary>
		/********************************************************************/
		public void SetFreq(float freq)
		{
			step = (float)(freq * 0.00002267573696145124);		// 1/44100
			if (step > 1.9f)
				step = 1.9f;

			sinStep = (uint)((1 << 22) * 512.0f * step);
		}



		/********************************************************************/
		/// <summary>
		/// Set PWM
		/// </summary>
		/********************************************************************/
		public void SetPwm(float p)
		{
			if (p > PwmLim)
				pwm = PwmLim;
			else
			{
				if (p < -PwmLim)
					pwm = -PwmLim;
				else
					pwm = p;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set form
		/// </summary>
		/********************************************************************/
		public void SetForm(WaveForm waveForm)
		{
			if (form != waveForm)
			{
				if (currVal > 1.0f)
					currVal = 0.0f;

				if (curr > 1.0f)
					curr = 0.0f;
			}

			form = waveForm;
		}



		/********************************************************************/
		/// <summary>
		/// Set amplitude
		/// </summary>
		/********************************************************************/
		public void SetAmp(float a)
		{
			_amp = a;
		}



		/********************************************************************/
		/// <summary>
		/// Set no interpolation
		/// </summary>
		/********************************************************************/
		public void NoInterp()
		{
			fromAmp = _amp;
		}



		/********************************************************************/
		/// <summary>
		/// Fill buffer with the current wave
		/// </summary>
		/********************************************************************/
		public bool Next(float[] buffer, uint count)
		{
			if (_amp < 0.001f)
				return false;

			switch (form)
			{
				case WaveForm.Saw:
				{
					FillSaw(buffer, count, _amp);
					break;
				}

				case WaveForm.Sqr:
				{
					FillSquare(buffer, count, _amp);
					break;
				}

				case WaveForm.Nos:
				{
					FillNoise(buffer, count, _amp);
					break;
				}

				case WaveForm.Tri:
				{
					FillTri(buffer, count, _amp);
					break;
				}

				case WaveForm.Sin:
				{
					FillSin(buffer, count, _amp);
					break;
				}

				case WaveForm.TriU:
				{
					FillTriU(buffer, count, _amp);
					break;
				}

				case WaveForm.SinU:
				{
					FillSinU(buffer, count, _amp);
					break;
				}

				default:
					return false;
			}

			return true;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Initialize
		/// </summary>
		/********************************************************************/
		private void SInit()
		{
			if (sIsInit)
				return;

			int c;

			// Sine table
			for (c = 0; c < 513; c++)
				sint[c] = (float)Math.Sin(c * (2.0f * Math.PI) / 512.0f);

			// Tri table
			float smp = -1.0f;
			const float Add = 4.0f / 512.0f;

			for (c = 0; c < 256; c++)
			{
				trit[c] = smp;
				smp += Add;
			}

			for (; c < 512; c++)
			{
				trit[c] = smp;
				smp -= Add;
			}

			trit[512] = trit[0];

			sIsInit = true;
		}



		/********************************************************************/
		/// <summary>
		/// Fill buffer with sawtooth wave
		/// </summary>
		/********************************************************************/
		private void FillSaw(float[] o, uint count, float amp)
		{
			float _ampAdd = (amp - fromAmp) / count;

			amp = fromAmp;

			for (uint i = 0; i < count;)
			{
				if (curr >= 1.0f)
				{
					float d = (curr - 1.0f) / step;
					curr -= 2.0f;

					o[i++] = amp * (-2.0f * d + 1.0f);
					amp += _ampAdd;
					curr += step;
				}

				float walkDiff = 1.0f - curr;
				int walkSteps = (int)((walkDiff / step) + 1);

				// Number of samples left in the buffer
				int steps = (int)(count - i);

				if (steps > walkSteps)
					steps = walkSteps;

				while (steps > 8)
				{
					o[i++] = amp * curr;
					amp += _ampAdd;
					curr += step;

					o[i++] = amp * curr;
					amp += _ampAdd;
					curr += step;

					o[i++] = amp * curr;
					amp += _ampAdd;
					curr += step;

					o[i++] = amp * curr;
					amp += _ampAdd;
					curr += step;

					o[i++] = amp * curr;
					amp += _ampAdd;
					curr += step;

					o[i++] = amp * curr;
					amp += _ampAdd;
					curr += step;

					o[i++] = amp * curr;
					amp += _ampAdd;
					curr += step;

					o[i++] = amp * curr;
					amp += _ampAdd;
					curr += step;

					steps -= 8;
				}

				while (steps > 0)
				{
					o[i++] = amp * curr;
					amp += _ampAdd;
					curr += step;

					steps--;
				}
			}

			fromAmp = amp;
		}



		/********************************************************************/
		/// <summary>
		/// Fill buffer with square wave
		/// </summary>
		/********************************************************************/
		private void FillSquare(float[] o, uint count, float amp)
		{
			float _ampAdd = (amp - fromAmp) / count;

			amp = fromAmp;

			for (uint i = 0; i < count;)
			{
				if (curr >= 1.0f)
				{
					float d = (curr - 1.0f) / step;

					curr -= 1.0f;
					if (_pwmLo)
					{
						o[i] = amp * d + ((1.0f - d) * amp);
						curr -= pwm;
					}
					else
					{
						o[i] = -amp * d + ((1.0f - d) * amp);
						curr += pwm;
					}

					_pwmLo = !_pwmLo;
					i++;

					amp += _ampAdd;
					curr += step;
				}

				float walkDiff = 1.0f - curr;
				int walkSteps = (int)((walkDiff / step) + 1);

				// Number of samples left in the buffer
				int steps = (int)(count - i);

				if (steps > walkSteps)
					steps = walkSteps;

				float tmpAmp, tmpAmpAdd;

				if (_pwmLo)
				{
					tmpAmp = -amp;
					tmpAmpAdd = -_ampAdd;
				}
				else
				{
					tmpAmp = amp;
					tmpAmpAdd = _ampAdd;
				}

				for (int counter = steps; counter > 0; counter--)
				{
					o[i++] = tmpAmp;
					tmpAmp += tmpAmpAdd;
				}

				amp += steps * _ampAdd;
				curr += steps * step;
			}

			fromAmp = amp;
		}



		/********************************************************************/
		/// <summary>
		/// Fill buffer with triangle wave
		/// </summary>
		/********************************************************************/
		private void FillTri(float[] o, uint count, float amp)
		{
			float aStep = (amp - fromAmp) / count;

			amp = fromAmp;

			for (uint i = 0; i < count; i++)
			{
				o[i] = amp * (2.0f * ((currVal > 0.0f) ? -currVal : currVal) + 1.0f);
				currVal += step;

				if (currVal > 1.0f)
					currVal -= 2.0f;

				amp += aStep;
			}

			fromAmp = amp;
		}



		/********************************************************************/
		/// <summary>
		/// Fill buffer with noise wave
		/// </summary>
		/********************************************************************/
		private void FillNoise(float[] o, uint count, float amp)
		{
			float _ampAdd = (amp - fromAmp) / count;

			amp = fromAmp;

			for (uint i = 0; i < count;)
			{
				if (curr >= 1.0f)
				{
					curr -= 2.0f;
					noiseVal = amp * ((JngRand() / (64.0f * 256.0f * 256.0f * 256.0f)) - 1.0f);
				}

				float walkDiff = 1.0f - curr;
				int walkSteps = (int)((walkDiff / step) + 1);

				// Number of samples left in the buffer
				int steps = (int)(count - i);

				if (steps > walkSteps)
					steps = walkSteps;

				for (int counter = steps; counter > 0; counter--)
					o[i++] = noiseVal;

				curr += steps * step;
				amp += steps * _ampAdd;
			}

			fromAmp = amp;

			if (curr >= 1.0f)
				curr -= 2.0f;
		}



		/********************************************************************/
		/// <summary>
		/// Fill buffer with sinus wave
		/// </summary>
		/********************************************************************/
		private void FillSin(float[] o, uint count, float amp)
		{
			float aStep = (amp - fromAmp) / count;

			amp = fromAmp;

			for (uint i = 0; i < count; i++)
			{
				sinCurrVal += sinStep;

				uint pos = sinCurrVal >> 23;
				float dec = (sinCurrVal & ((1 << 23) - 1)) / (float)((1 << 23) - 1);

				float val0 = sint[pos];
				float val1 = sint[pos + 1];
				float val = (val1 * dec) + (val0 * (1.0f - dec));

				o[i] = amp * val;
				amp += aStep;
			}

			fromAmp = amp;
		}



		/********************************************************************/
		/// <summary>
		/// Fill buffer with triangle wave
		/// </summary>
		/********************************************************************/
		private void FillTriU(float[] o, uint count, float amp)
		{
			float aStep = (amp - fromAmp) / count;

			amp = fromAmp;

			for (uint i = 0; i < count; i++)
			{
				sinCurrVal += sinStep;

				uint pos = sinCurrVal >> 23;

				o[i] = amp * trit[pos];
				amp += aStep;
			}

			fromAmp = amp;
		}



		/********************************************************************/
		/// <summary>
		/// Fill buffer with sinus wave
		/// </summary>
		/********************************************************************/
		private void FillSinU(float[] o, uint count, float amp)
		{
			float aStep = (amp - fromAmp) / count;

			amp = fromAmp;

			for (uint i = 0; i < count; i++)
			{
				sinCurrVal += sinStep;

				uint pos = sinCurrVal >> 23;

				o[i] = amp * sint[pos];
				amp += aStep;
			}

			fromAmp = amp;
		}



		/********************************************************************/
		/// <summary>
		/// Random number generator
		/// </summary>
		/********************************************************************/
		private uint JngRand()
		{
			const int BigMod = 0x7fffffff;
			const int W = 127773;
			const int C = 2836;

			jngSeed = 16807 * (jngSeed % W) - (jngSeed / W) * C;
			if (jngSeed < 0)
				jngSeed += BigMod;

			return (uint)jngSeed;
		}
		private static int jngSeed = 1;
		#endregion
	}
}
