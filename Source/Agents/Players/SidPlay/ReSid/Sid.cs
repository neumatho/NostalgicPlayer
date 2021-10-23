/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSid.Containers;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSid
{
	/// <summary>
	/// API interface
	/// </summary>
	internal class Sid
	{
		private readonly Voice[] voice = Helpers.InitializeArray<Voice>(3);
		private readonly Filter filter = new Filter();
		private readonly ExternalFilter extFilt = new ExternalFilter();
		private readonly Potentiometer potX = new Potentiometer();
		private readonly Potentiometer potY = new Potentiometer();

		private uint busValue;
		private int busValueTtl;

		private double clockFrequency;

		// External audio input
		private int extIn;

		// Resampling constants
		// The error in interpolated lookup is bounded by 1.234/L^2,
		// while the error in non-interpolated lookup is bounded by
		// 0.7854/L + 0.4113/L^2, see
		// http://www-ccrma.stanford.edu/~jos/resample/Choice_Table_Size.html
		// For a resolution of 16 bits this yields L >= 285 and L >= 51473,
		// respectively
		private const int FirN = 125;
		private const int FirResInterpolate = 285;
		private const int FirResFast = 51473;
		private const int FirShift = 15;
		private const int RingSize = 16384;

		// Fixpoint constants (16.6 bits)
		private const int FixpShift = 16;
		private const int FixpMask = 0xffff;

		// Sampling variables
		private SamplingMethod sampling;
		private int cyclesPerSample;
		private int sampleOffset;
		private int sampleIndex;
		private short samplePrev;
		private int firN;
		private int firRes;

		// Ring buffer with overflow for contiguous storage of RingSize samples
		private short[] sample;

		// FirRes filter tables (FirN*FirRes)
		private short[] fir;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Sid()
		{
			// Initialize pointers
			sample = null;
			fir = null;

			voice[0].SetSyncSource(voice[2]);
			voice[1].SetSyncSource(voice[0]);
			voice[2].SetSyncSource(voice[1]);

			SetSamplingParameters(985248, SamplingMethod.Fast, 44100);

			busValue = 0;
			busValueTtl = 0;

			extIn = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Set chip model
		/// </summary>
		/********************************************************************/
		public void SetChipModel(ChipModel model)
		{
			for (int i = 0; i < 3; i++)
				voice[i].SetChipModel(model);

			filter.SetChipModel(model);
			extFilt.SetChipModel(model);
		}



		/********************************************************************/
		/// <summary>
		/// SID reset
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			for (int i = 0; i < 3; i++)
				voice[i].Reset();

			filter.Reset();
			extFilt.Reset();

			busValue = 0;
			busValueTtl = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Enable filter
		/// </summary>
		/********************************************************************/
		public void EnableFilter(bool enable)
		{
			filter.EnableFilter(enable);
		}



		/********************************************************************/
		/// <summary>
		/// Return array of default spline interpolation points to map FC to
		/// filter cutoff frequency
		/// </summary>
		/********************************************************************/
		public void FcDefault(Spline.FCPoint[] points, out int count)
		{
			filter.FcDefault(points, out count);
		}



		/********************************************************************/
		/// <summary>
		/// Return FC spline plotter object
		/// </summary>
		/********************************************************************/
		public Spline.PointPlotter FcPlotter()
		{
			return filter.FcPlotter();
		}



		/********************************************************************/
		/// <summary>
		/// Setting of SID sampling parameters
		///
		/// Use a clock frequency of 985248 Hz for PAL C64, 1022730 Hz for
		/// NTSC C64. The default end of passband frequency is
		/// passFreq = 0.9*sampleFreq/2 for sample frequencies up to ~44.1 KHz,
		/// and 20 KHz for higher sample frequencies.
		///
		/// For resampling, the ratio between the clock frequency and the
		/// sample frequency is limited as follows:
		///   125*clockFreq/sampleFreq &lt; 16384
		/// E.g. provided a clock frequency of ~ 1 Mhz, the sample frequency
		/// can not be set lower than ~ 8 KHz. A lower sample frequency
		/// would make the resampling code overfill its 16 K sample ring
		/// buffer.
		///
		/// The end of passband frequency is also limited:
		///   passFreq &lt;= 0.9*sampleFreq/2
		///
		/// E.g. for a 44.1 KHz sampling rate the end of passband frequency
		/// is limited to slightly below 20 KHz. This constraint ensures
		/// that the FIR table is not overfilled
		/// </summary>
		/********************************************************************/
		public bool SetSamplingParameters(double clockFreq, SamplingMethod method, double sampleFreq, double passFreq = -1, double filterScale = 0.97)
		{
			// Check resampling constraints
			if ((method == SamplingMethod.ResampleInterpolate) || (method == SamplingMethod.ResampleFast))
			{
				// Check whether the sample ring buffer would overfill
				if (FirN * clockFreq / sampleFreq >= RingSize)
					return false;

				// The default passband limit is 0.9*sampleFreq/2 for sample
				// frequencies below ~ 44.1 KHz, and 20 KHz for higher sample frequencies
				if (passFreq < 0)
				{
					passFreq = 20000;

					if (2 * passFreq / sampleFreq >= 0.9)
						passFreq = 0.9 * sampleFreq / 2;
				}
				else
				{
					// Check whether the FIR table would overfill
					if (passFreq > 0.9 * sampleFreq / 2)
						return false;
				}

				// The filter scaling is only included to avoid clipping, so keep
				// it sane
				if ((filterScale < 0.9) || (filterScale > 1.0))
					return false;
			}

			clockFrequency = clockFreq;
			sampling = method;

			cyclesPerSample = (int)(clockFreq / sampleFreq * (1 << FixpShift) + 0.5);

			sampleOffset = 0;
			samplePrev = 0;

			// FIR initialization is only necessary for resampling
			if ((method != SamplingMethod.ResampleInterpolate) && (method != SamplingMethod.ResampleFast))
			{
				sample = null;
				fir = null;

				return true;
			}

			// 16 bits -> -96 dB stopband attenuation
			double a = -20 * Math.Log10(1.0 / (1 << 16));

			// A fraction of the bandwidth is allocated to the transition band
			double dw = (1 - 2 * passFreq / sampleFreq) * Math.PI;

			// The cutoff frequency is midway through the transition band
			double wc = (2 * passFreq / sampleFreq + 1) * Math.PI / 2;

			// For calculation of beta and N see the reference for the kaiserord
			// function in the MATLAB Signal Processing Toolbox:
			// http://www.mathworks.com/access/helpdesk/help/toolbox/signal/kaiserord.html
			double beta = 0.1102 * (a - 8.7);
			double i0Beta = I0(beta);

			// The filter order will maximally be 124 with the current constraints.
			// N >= (96.33 - 7.95)/(2.285*0.1*pi) -> N >= 123
			// The filter order is equal to the number of zero crossings, i.e.
			// it should be an even number (sinc is symmetric about x = 0)
			int n = (int)((a - 7.95) / (2.285 * dw) + 0.5);
			n += n & 1;

			double fSamplesPerCycle = sampleFreq / clockFreq;
			double fCyclesPerSample = clockFreq / sampleFreq;

			// The filter length is equal to the filter order + 1.
			// The filter length must be an odd number (sinc is symmetric about x = 0)
			firN = (int)(n * fCyclesPerSample) + 1;
			firN |= 1;

			// We clamp the filter table resolution to 2^n, making the fixpoint
			// sample offset a whole multiple of the filter table resolution
			int res = method == SamplingMethod.ResampleInterpolate ? FirResInterpolate : FirResFast;
			n = (int)Math.Ceiling(Math.Log(res / fCyclesPerSample) / Math.Log(2));
			firRes = 1 << n;

			// Allocate memory for FIR tables
			fir = new short[firN * firRes];

			// Calculate firRes FIR tables for linear interpolation
			for (int i = 0; i < firRes; i++)
			{
				int firOffset = i * firN + firN / 2;
				double jOffset = ((double)i) / firRes;

				// Calculate FIR table. This is the sinc function, weighted by the
				// Kaiser window
				for (int j = -FirN / 2; j <= FirN / 2; j++)
				{
					double jx = j - jOffset;
					double wt = wc * jx / fCyclesPerSample;
					double temp = jx / ((double)firN / 2);
					double kaiser = Math.Abs(temp) <= 1 ? I0(beta * Math.Sqrt(1 - temp * temp)) / i0Beta : 0;
					double sinCwt = Math.Abs(wt) >= 1e-6 ? Math.Sin(wt) / wt : 1;
					double val = (1 << FirShift) * filterScale * fSamplesPerCycle * wc / Math.PI * sinCwt * kaiser;
					fir[firOffset + j] = (short)(val + 0.5);
				}
			}

			// Allocate sample buffer
			if (sample == null)
				sample = new short[RingSize * 2];

			// Clear sample buffer
			Array.Clear(sample, 0, RingSize * 2);

			sampleIndex = 0;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// SID clocking - 1 cycle
		/// </summary>
		/********************************************************************/
		public void Clock()
		{
			// Age bus value
			if (--busValueTtl <= 0)
			{
				busValue = 0;
				busValueTtl = 0;
			}

			// Clock amplitude modulators
			for (int i = 0; i < 3; i++)
				voice[i].envelope.Clock();

			// Clock oscillators
			for (int i = 0; i < 3; i++)
				voice[i].wave.Clock();

			// Synchronize oscillators
			for (int i = 0; i < 3; i++)
				voice[i].wave.Synchronize();

			// Clock filter
			filter.Clock(voice[0].Output(), voice[1].Output(), voice[2].Output(), extIn);

			// Clock external filter
			extFilt.Clock(filter.Output());
		}



		/********************************************************************/
		/// <summary>
		/// SID clocking - delta cycle
		/// </summary>
		/********************************************************************/
		public void Clock(int delta)
		{
			if (delta <= 0)
				return;

			// Age bus value
			busValueTtl -= delta;
			if (busValueTtl <= 0)
			{
				busValue = 0;
				busValueTtl = 0;
			}

			// Clock amplitude modulators
			for (int i = 0; i < 3; i++)
				voice[i].envelope.Clock(delta);

			// Clock and synchronize oscillators
			// Loop until we reach the current cycle
			int deltaOsc = delta;
			while (deltaOsc != 0)
			{
				int deltaMin = deltaOsc;

				// Find minimum number of cycles to an oscillator accumulator MSB toggle.
				// We have to clock on each MSB on / MSB off for hard sync to operate
				// correctly
				for (int i = 0; i < 3; i++)
				{
					WaveformGenerator wave = voice[i].wave;

					// It is only necessary to clock on the MSB of an oscillator that is
					// a sync source and has freq != 0
					if (!((wave.syncDest.sync != 0) && (wave.freq != 0)))
						continue;

					uint freq = wave.freq;
					uint accumulator = wave.accumulator;

					// Clock on MSB off if MSB is on, clock on MSB on if MSB is off
					uint deltaAccumulator = ((accumulator & 0x800000) != 0 ? (uint)0x1000000 : 0x800000) - accumulator;

					int deltaNext = (int)(deltaAccumulator / freq);
					if ((deltaAccumulator % freq) != 0)
						++deltaNext;

					if (deltaNext < deltaMin)
						deltaMin = deltaNext;
				}

				// Clock oscillators
				for (int i = 0; i < 3; i++)
					voice[i].wave.Clock(deltaMin);

				// Synchronize oscillators
				for (int i = 0; i < 3; i++)
					voice[i].wave.Synchronize();

				deltaOsc -= deltaMin;
			}

			// Clock filter
			filter.Clock(delta, voice[0].Output(), voice[1].Output(), voice[2].Output(), extIn);

			// Clock external filter
			extFilt.Clock(delta, filter.Output());
		}



		/********************************************************************/
		/// <summary>
		/// SID clocking with audio sampling.
		/// Fixpoint arithmetics is used.
		///
		/// The example below shows how to clock the SID a specified amount
		/// of cycles while producing audio output:
		///
		/// while (delta_t) {
		///   bufindex += sid.clock(delta_t, buf + bufindex, buflength - bufindex);
		///   write(dsp, buf, bufindex*2);
		///   bufindex = 0;
		/// }
		/// </summary>
		/********************************************************************/
		public int Clock(int delta, short[] buf, int offset, int n, int interleave = 1)
		{
			switch (sampling)
			{
				default:
				case SamplingMethod.Fast:
					return ClockFast(delta, buf, offset, n, interleave);

/*				case SamplingMethod.Interpolate:
					return ClockInterpolate(delta, buf, offset, n, interleave);

				case SamplingMethod.ResampleInterpolate:
					return ClockResampleInterpolate(delta, buf, offset, n, interleave);

				case SamplingMethod.ResampleFast:
					return ClockResampleFast(delta, buf, offset, n, interleave);
*/			}
		}



		/********************************************************************/
		/// <summary>
		/// Write 16-bit sample to audio input.
		/// NB! The caller is responsible for keeping the value within 16
		/// bits. Note that to mix in an external audio signal, the signal
		/// should be resampled to 1MHz first to avoid sampling noise
		/// </summary>
		/********************************************************************/
		public void Input(int sample)
		{
			// Voice outputs are 20 bits. Scale up to match three voices in order
			// to facilitate simulation of the MOS8580 "digi boost" hardware hack
			extIn = (sample << 4) * 3;
		}



		/********************************************************************/
		/// <summary>
		/// Read sample from audio output
		/// </summary>
		/********************************************************************/
		public int Output(int bits)
		{
			int range = 1 << bits;
			int half = range >> 1;

			int sample = extFilt.Output() / ((4095 * 255 >> 7) * 3 * 15 * 2 / range);
			if (sample >= half)
				return half - 1;

			if (sample < -half)
				return -half;

			return sample;
		}



		/********************************************************************/
		/// <summary>
		/// Read registers.
		///
		/// Reading a write only register returns the last byte written to
		/// any SID register. The individual bits in this value starts to
		/// fade down towards zero after a few cycles. All bits reach zero
		/// within approximately $2000 - $4000 cycles.
		/// It has been claimed that this fading happens in an orderly
		/// fashion, however sampling of write only registers reveals that
		/// this is not the case.
		/// NB! This is not correctly modeled.
		///
		/// The actual use of write only registers has largely been made in
		/// the belief that all SID registers are readable. To support this
		/// belief the read would have to be done immediately after a write
		/// to the same register (remember that an intermediate write to
		/// another register would yield that value instead). With this in
		/// mind we return the last value written to any SID registers for
		/// $2000 cycles without modeling the bit fading
		/// </summary>
		/********************************************************************/
		public uint Read(uint offset)
		{
			switch (offset)
			{
				case 0x19:
					return potX.ReadPot();

				case 0x1a:
					return potY.ReadPot();

				case 0x1b:
					return voice[2].wave.ReadOsc();

				case 0x1c:
					return voice[2].envelope.ReadEnv();
			}

			return busValue;
		}



		/********************************************************************/
		/// <summary>
		/// Write registers
		/// </summary>
		/********************************************************************/
		public void Write(uint offset, uint value)
		{
			busValue = value;
			busValueTtl = 0x2000;

			switch (offset)
			{
				case 0x00:
				{
					voice[0].wave.WriteFreqLo(value);
					break;
				}

				case 0x01:
				{
					voice[0].wave.WriteFreqHi(value);
					break;
				}

				case 0x02:
				{
					voice[0].wave.WritePwLo(value);
					break;
				}

				case 0x03:
				{
					voice[0].wave.WritePwHi(value);
					break;
				}

				case 0x04:
				{
					voice[0].WriteControlReg(value);
					break;
				}

				case 0x05:
				{
					voice[0].envelope.WriteAttackDecay(value);
					break;
				}

				case 0x06:
				{
					voice[0].envelope.WriteSustainRelease(value);
					break;
				}

				case 0x07:
				{
					voice[1].wave.WriteFreqLo(value);
					break;
				}

				case 0x08:
				{
					voice[1].wave.WriteFreqHi(value);
					break;
				}

				case 0x09:
				{
					voice[1].wave.WritePwLo(value);
					break;
				}

				case 0x0a:
				{
					voice[1].wave.WritePwHi(value);
					break;
				}

				case 0x0b:
				{
					voice[1].WriteControlReg(value);
					break;
				}

				case 0x0c:
				{
					voice[1].envelope.WriteAttackDecay(value);
					break;
				}

				case 0x0d:
				{
					voice[1].envelope.WriteSustainRelease(value);
					break;
				}

				case 0x0e:
				{
					voice[2].wave.WriteFreqLo(value);
					break;
				}

				case 0x0f:
				{
					voice[2].wave.WriteFreqHi(value);
					break;
				}

				case 0x10:
				{
					voice[2].wave.WritePwLo(value);
					break;
				}

				case 0x11:
				{
					voice[2].wave.WritePwHi(value);
					break;
				}

				case 0x12:
				{
					voice[2].WriteControlReg(value);
					break;
				}

				case 0x13:
				{
					voice[2].envelope.WriteAttackDecay(value);
					break;
				}

				case 0x14:
				{
					voice[2].envelope.WriteSustainRelease(value);
					break;
				}

				case 0x15:
				{
					filter.WriteFcLo(value);
					break;
				}

				case 0x16:
				{
					filter.WriteFcHi(value);
					break;
				}

				case 0x17:
				{
					filter.WriteResFilt(value);
					break;
				}

				case 0x18:
				{
					filter.WriteModeVol(value);
					break;
				}
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// I0() computes the 0th order modified Bessel function of the
		/// first kind. This function is originally from
		/// resample-1.5/filterkit.c by J. O. Smith
		/// </summary>
		/********************************************************************/
		private double I0(double x)
		{
			// Max error acceptable in I0
			const double I0e = 1e-6;

			double u;

			double sum = u = 1;
			int n = 1;
			double halfX = x / 2.0;

			do
			{
				double temp = halfX / n++;
				u *= temp * temp;
				sum += u;
			}
			while (u >= I0e * sum);

			return sum;
		}



		/********************************************************************/
		/// <summary>
		/// SID clocking with audio sampling - delta clocking picking nearest
		/// sample
		/// </summary>
		/********************************************************************/
		private int ClockFast(int delta, short[] buf, int offset, int n, int interleave)
		{
			int s = 0;

			for (;;)
			{
				int nextSampleOffset = sampleOffset + cyclesPerSample + (1 << (FixpShift - 1));
				int deltaSample = nextSampleOffset >> FixpShift;

				if (deltaSample > delta)
					break;

				if (s >= n)
					return s;

				Clock(deltaSample);
				delta -= deltaSample;
				sampleOffset = (nextSampleOffset & FixpMask) - (1 << (FixpShift - 1));
				buf[offset + s++ * interleave] = (short)Output(16);
			}

			Clock(delta);
			sampleOffset -= delta << FixpShift;
			delta = 0;

			return s;
		}
		#endregion
	}
}
