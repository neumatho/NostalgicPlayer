/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSid.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSid
{
	/// <summary>
	/// SID filter emulation
	/// </summary>
	internal class Filter
	{
		// ----------------------------------------------------------------------------
		// The SID filter is modeled with a two-integrator-loop biquadratic filter,
		// which has been confirmed by Bob Yannes to be the actual circuit used in
		// the SID chip.
		//
		// Measurements show that excellent emulation of the SID filter is achieved,
		// except when high resonance is combined with high sustain levels.
		// In this case the SID op-amps are performing less than ideally and are
		// causing some peculiar behavior of the SID filter. This however seems to
		// have more effect on the overall amplitude than on the color of the sound.
		//
		// The theory for the filter circuit can be found in "Microelectric Circuits"
		// by Adel S. Sedra and Kenneth C. Smith.
		// The circuit is modeled based on the explanation found there except that
		// an additional inverter is used in the feedback from the bandpass output,
		// allowing the summer op-amp to operate in single-ended mode. This yields
		// inverted filter outputs with levels independent of Q, which corresponds with
		// the results obtained from a real SID.
		//
		// We have been able to model the summer and the two integrators of the circuit
		// to form components of an IIR filter.
		// Vhp is the output of the summer, Vbp is the output of the first integrator,
		// and Vlp is the output of the second integrator in the filter circuit.
		//
		// According to Bob Yannes, the active stages of the SID filter are not really
		// op-amps. Rather, simple NMOS inverters are used. By biasing an inverter
		// into its region of quasi-linear operation using a feedback resistor from
		// input to output, a MOS inverter can be made to act like an op-amp for
		// small signals centered around the switching threshold.
		//
		// Qualified guesses at SID filter schematics are depicted below.
		//
		// SID filter
		// ----------
		// 
		//     -----------------------------------------------
		//    |                                               |
		//    |            ---Rq--                            |
		//    |           |       |                           |
		//    |  ------------<A]-----R1---------              |
		//    | |                               |             |
		//    | |                        ---C---|      ---C---|
		//    | |                       |       |     |       |
		//    |  --R1--    ---R1--      |---Rs--|     |---Rs--| 
		//    |        |  |       |     |       |     |       |
		//     ----R1--|-----[A>--|--R-----[A>--|--R-----[A>--|
		//             |          |             |             |
		// vi -----R1--           |             |             |
		// 
		//                       vhp           vbp           vlp
		// 
		// 
		// vi  - input voltage
		// vhp - highpass output
		// vbp - bandpass output
		// vlp - lowpass output
		// [A> - op-amp
		// R1  - summer resistor
		// Rq  - resistor array controlling resonance (4 resistors)
		// R   - NMOS FET voltage controlled resistor controlling cutoff frequency
		// Rs  - shunt resitor
		// C   - capacitor
		// 
		// 
		// 
		// SID integrator
		// --------------
		// 
		//                                   V+
		// 
		//                                   |
		//                                   |
		//                              -----|
		//                             |     |
		//                             | ||--
		//                              -||
		//                   ---C---     ||->
		//                  |       |        |
		//                  |---Rs-----------|---- vo
		//                  |                |
		//                  |            ||--
		// vi ----     -----|------------||
		//        |   ^     |            ||->
		//        |___|     |                |
		//        -----     |                |
		//          |       |                |
		//          |---R2--                 |
		//          |
		//          R1                       V-
		//          |
		//          |
		// 
		//          Vw
		//
		// ----------------------------------------------------------------------------

		// Filter enabled
		private bool enabled;

		// Filter cutoff frequency
		private uint fc;

		// Filter resonance
		private uint res;

		// Selects which inputs to route through filter
		private uint filt;

		// Switch voice 3 off
		private uint voice3Off;

		// Highpass, bandpass and lowpass filter modes
		private uint hpBpLp;

		// Output master volume
		private uint vol;

		// Mixer DC offset
		private int mixerDc;

		// State of filter
		private int vhp;		// Highpass
		private int vbp;		// Bandpass
		private int vlp;		// Lowpass
		private int vnf;		// Not filtered

		// Cutoff frequency, resonance
		private int w0;
		private int w0Ceil1;
		private int w0CeilDt;
		private int _1024DivQ;

		// Cutoff frequency tables
		// FC is an 11 bit register
		private readonly int[] f06581 = new int[2048];
		private readonly int[] f08580 = new int[2048];
		private int[] f0;
		private Spline.FCPoint[] f0Points;
		private int f0Count;

		// Maximum cutoff frequency is specified as
		// FCmax = 2.6e-5/C = 2.6e-5/2200e-12 = 11818.
		//
		// Measurements indicate a cutoff frequency range of approximately
		// 220Hz - 18kHz on a MOS6581 fitted with 470pF capacitors. The function
		// mapping FC to cutoff frequency has the shape of the tanh function, with
		// a discontinuity at FCHI = 0x80.
		// In contrast, the MOS8580 almost perfectly corresponds with the
		// specification of a linear mapping from 30Hz to 12kHz.
		// 
		// The mappings have been measured by feeding the SID with an external
		// signal since the chip itself is incapable of generating waveforms of
		// higher fundamental frequency than 4kHz. It is best to use the bandpass
		// output at full resonance to pick out the cutoff frequency at any given
		// FC setting.
		//
		// The mapping function is specified with spline interpolation points and
		// the function values are retrieved via table lookup.
		//
		// NB! Cutoff frequency characteristics may vary, we have modeled two
		// particular Commodore 64s.
		private static readonly Spline.FCPoint[] f0Points6581 =
		{
			//     FC      f        FCHI FCLO
			// -------------------------------
			new (   0,   220),   // 0x00      - repeated end point
			new (   0,   220),   // 0x00
			new ( 128,   230),   // 0x10
			new ( 256,   250),   // 0x20
			new ( 384,   300),   // 0x30
			new ( 512,   420),   // 0x40
			new ( 640,   780),   // 0x50
			new ( 768,  1600),   // 0x60
			new ( 832,  2300),   // 0x68
			new ( 896,  3200),   // 0x70
			new ( 960,  4300),   // 0x78
			new ( 992,  5000),   // 0x7c
			new (1008,  5400),   // 0x7e
			new (1016,  5700),   // 0x7f
			new (1023,  6000),   // 0x7f 0x07
			new (1023,  6000),   // 0x7f 0x07 - discontinuity
			new (1024,  4600),   // 0x80      -
			new (1024,  4600),   // 0x80
			new (1032,  4800),   // 0x81
			new (1056,  5300),   // 0x84
			new (1088,  6000),   // 0x88
			new (1120,  6600),   // 0x8c
			new (1152,  7200),   // 0x90
			new (1280,  9500),   // 0xa0
			new (1408, 12000),   // 0xb0
			new (1536, 14500),   // 0xc0
			new (1664, 16000),   // 0xd0
			new (1792, 17100),   // 0xe0
			new (1920, 17700),   // 0xf0
			new (2047, 18000),   // 0xff 0x07
			new (2047, 18000)    // 0xff 0x07 - repeated end point
		};

		private static readonly Spline.FCPoint[] f0Points8580 =
		{
			//     FC      f        FCHI FCLO
			// -------------------------------
			new (   0,     0),   // 0x00      - repeated end point
			new (   0,     0),   // 0x00
			new ( 128,   800),   // 0x10
			new ( 256,  1600),   // 0x20
			new ( 384,  2500),   // 0x30
			new ( 512,  3300),   // 0x40
			new ( 640,  4100),   // 0x50
			new ( 768,  4800),   // 0x60
			new ( 896,  5600),   // 0x70
			new (1024,  6500),   // 0x80
			new (1152,  7500),   // 0x90
			new (1280,  8400),   // 0xa0
			new (1408,  9200),   // 0xb0
			new (1536,  9800),   // 0xc0
			new (1664, 10500),   // 0xd0
			new (1792, 11000),   // 0xe0
			new (1920, 11700),   // 0xf0
			new (2047, 12500),   // 0xff 0x07
			new (2047, 12500)    // 0xff 0x07 - repeated end point
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Filter()
		{
			fc = 0;

			res = 0;

			filt = 0;

			voice3Off = 0;

			hpBpLp = 0;

			vol = 0;

			// State of filter
			vhp = 0;
			vbp = 0;
			vlp = 0;
			vnf = 0;

			EnableFilter(true);

			// Create mappings from FC to cutoff frequency
			Spline.Interpolate(f0Points6581, 0, f0Points6581.Length - 1, new Spline.PointPlotter(f06581), 1.0);
			Spline.Interpolate(f0Points8580, 0, f0Points8580.Length - 1, new Spline.PointPlotter(f08580), 1.0);

			SetChipModel(ChipModel.Mos6581);
		}



		/********************************************************************/
		/// <summary>
		/// Enable filter
		/// </summary>
		/********************************************************************/
		public void EnableFilter(bool enable)
		{
			enabled = enable;
		}



		/********************************************************************/
		/// <summary>
		/// Return the array of spline interpolation points used to map the
		/// FC register to filter cutoff frequency
		/// </summary>
		/********************************************************************/
		public void FcDefault(Spline.FCPoint[] points, out int count)
		{
			Array.Copy(f0Points, 0, points, 0, f0Count);
			count = f0Count;
		}



		/********************************************************************/
		/// <summary>
		/// Given an array of interpolation points p with n points, the
		/// following statement will specify a new FC mapping:
		/// 
		///   Interpolate(p, p + n - 1, filter.FCPlotter(), 1.0f)
		/// 
		/// Note that the x range of the interpolation points *must* be
		/// [0, 2047], and that additional end points *must* be present since
		/// the end points are not interpolated
		/// </summary>
		/********************************************************************/
		public Spline.PointPlotter FcPlotter()
		{
			return new Spline.PointPlotter(f0);
		}



		/********************************************************************/
		/// <summary>
		/// Set chip model
		/// </summary>
		/********************************************************************/
		public void SetChipModel(ChipModel model)
		{
			if (model == ChipModel.Mos6581)
			{
				// The mixer has a small input DC offset. This is found as follows:
				//
				// The "zero" output level of the mixer measured on the SID audio
				// output pin is 5.50V at zero volume, and 5.44 at full
				// volume. This yields a DC offset of (5.44V - 5.50V) = -0.06V.
				//
				// The DC offset is thus -0.06V/1.05V ~ -1/18 of the dynamic range
				// of one voice. See Voice.cs for measurement of the dynamic
				// range
				mixerDc = -0xfff * 0xff / 18 >> 7;

				f0 = f06581;
				f0Points = f0Points6581;
				f0Count = f0Points6581.Length;
			}
			else
			{
				// No DC offsets in the MOS8580
				mixerDc = 0;

				f0 = f08580;
				f0Points = f0Points8580;
				f0Count = f0Points8580.Length;
			}

			SetW0();
			SetQ();
		}



		/********************************************************************/
		/// <summary>
		/// SID reset
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			fc = 0;

			res = 0;

			filt = 0;

			voice3Off = 0;

			hpBpLp = 0;

			vol = 0;

			// State of filter
			vhp = 0;
			vbp = 0;
			vlp = 0;
			vnf = 0;

			SetW0();
			SetQ();
		}



		/********************************************************************/
		/// <summary>
		/// SID clocking - 1 cycle
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clock(int voice1, int voice2, int voice3, int extIn)
		{
			// Scale each voice down from 20 to 13 bits
			voice1 >>= 7;
			voice2 >>= 7;

			// NB! Voice 3 is not silenced by voice3Off if it is routed though
			// the filter
			if ((voice3Off != 0) && !((filt & 0x04) != 0))
				voice3 = 0;
			else
				voice3 >>= 7;

			extIn >>= 7;

			// This is handy for testing
			if (!enabled)
			{
				vnf = voice1 + voice2 + voice3 + extIn;
				vhp = vbp = vlp = 0;
				return;
			}

			// Route voices into or around filter.
			// The code below is expanded to a switch for faster execution.
			// (filt1 ? vi : vinf) += voice1;
			// (filt2 ? vi : vinf) += voice2;
			// (filt3 ? vi : vinf) += voice3;
			int vi;

			switch (filt)
			{
				default:
				case 0x0:
				{
					vi = 0;
					vnf = voice1 + voice2 + voice3 + extIn;
					break;
				}

				case 0x1:
				{
					vi = voice1;
					vnf = voice2 + voice3 + extIn;
					break;
				}

				case 0x2:
				{
					vi = voice2;
					vnf = voice1 + voice3 + extIn;
					break;
				}

				case 0x3:
				{
					vi = voice1 + voice2;
					vnf = voice3 + extIn;
					break;
				}

				case 0x4:
				{
					vi = voice3;
					vnf = voice1 + voice2 + extIn;
					break;
				}

				case 0x5:
				{
					vi = voice1 + voice3;
					vnf = voice2 + extIn;
					break;
				}

				case 0x6:
				{
					vi = voice2 + voice3;
					vnf = voice1 + extIn;
					break;
				}

				case 0x7:
				{
					vi = voice1 + voice2 + voice3;
					vnf = extIn;
					break;
				}

				case 0x8:
				{
					vi = extIn;
					vnf = voice1 + voice2 + voice3;
					break;
				}

				case 0x9:
				{
					vi = voice1 + extIn;
					vnf = voice2 + voice3;
					break;
				}

				case 0xa:
				{
					vi = voice2 + extIn;
					vnf = voice1 + voice3;
					break;
				}

				case 0xb:
				{
					vi = voice1 + voice2 + extIn;
					vnf = voice3;
					break;
				}

				case 0xc:
				{
					vi = voice3 + extIn;
					vnf = voice1 + voice2;
					break;
				}

				case 0xd:
				{
					vi = voice1 + voice3 + extIn;
					vnf = voice2;
					break;
				}

				case 0xe:
				{
					vi = voice2 + voice3 + extIn;
					vnf = voice1;
					break;
				}

				case 0xf:
				{
					vi = voice1 + voice2 + voice3 + extIn;
					vnf = 0;
					break;
				}
			}

			// delta = 1 is converted to seconds given a 1 MHz clock by dividing
			// with 1 000 000

			// Calculate filter outputs.
			// Vhp = vbp/Q - Vlp - Vi;
			// dVbp = -w0*Vhp*dt;
			// dVlp = -w0*Vbp*dt;
			int dVbp = (w0Ceil1 * vhp >> 20);
			int dVlp = (w0Ceil1 * vbp >> 20);
			vbp -= dVbp;
			vlp -= dVlp;
			vhp = (vbp * _1024DivQ >> 10) - vlp - vi;
		}



		/********************************************************************/
		/// <summary>
		/// SID clocking - delta cycles
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clock(int delta, int voice1, int voice2, int voice3, int extIn)
		{
			// Scale each voice down from 20 to 13 bits
			voice1 >>= 7;
			voice2 >>= 7;

			// NB! Voice 3 is not silenced by voice3Off if it is routed though
			// the filter
			if ((voice3Off != 0) && !((filt & 0x04) != 0))
				voice3 = 0;
			else
				voice3 >>= 7;

			extIn >>= 7;

			// Enable filter on/off.
			// This is not really part of SID, but is useful for testing.
			// On slow CPUs it may be necessary to bypass the filter to lower the CPU
			// load
			if (!enabled)
			{
				vnf = voice1 + voice2 + voice3 + extIn;
				vhp = vbp = vlp = 0;
				return;
			}

			// Route voices into or around filter.
			// The code below is expanded to a switch for faster execution.
			// (filt1 ? vi : vinf) += voice1;
			// (filt2 ? vi : vinf) += voice2;
			// (filt3 ? vi : vinf) += voice3;
			int vi;

			switch (filt)
			{
				default:
				case 0x0:
				{
					vi = 0;
					vnf = voice1 + voice2 + voice3 + extIn;
					break;
				}

				case 0x1:
				{
					vi = voice1;
					vnf = voice2 + voice3 + extIn;
					break;
				}

				case 0x2:
				{
					vi = voice2;
					vnf = voice1 + voice3 + extIn;
					break;
				}

				case 0x3:
				{
					vi = voice1 + voice2;
					vnf = voice3 + extIn;
					break;
				}

				case 0x4:
				{
					vi = voice3;
					vnf = voice1 + voice2 + extIn;
					break;
				}

				case 0x5:
				{
					vi = voice1 + voice3;
					vnf = voice2 + extIn;
					break;
				}

				case 0x6:
				{
					vi = voice2 + voice3;
					vnf = voice1 + extIn;
					break;
				}

				case 0x7:
				{
					vi = voice1 + voice2 + voice3;
					vnf = extIn;
					break;
				}

				case 0x8:
				{
					vi = extIn;
					vnf = voice1 + voice2 + voice3;
					break;
				}

				case 0x9:
				{
					vi = voice1 + extIn;
					vnf = voice2 + voice3;
					break;
				}

				case 0xa:
				{
					vi = voice2 + extIn;
					vnf = voice1 + voice3;
					break;
				}

				case 0xb:
				{
					vi = voice1 + voice2 + extIn;
					vnf = voice3;
					break;
				}

				case 0xc:
				{
					vi = voice3 + extIn;
					vnf = voice1 + voice2;
					break;
				}

				case 0xd:
				{
					vi = voice1 + voice3 + extIn;
					vnf = voice2;
					break;
				}

				case 0xe:
				{
					vi = voice2 + voice3 + extIn;
					vnf = voice1;
					break;
				}

				case 0xf:
				{
					vi = voice1 + voice2 + voice3 + extIn;
					vnf = 0;
					break;
				}
			}

			// Maximum delta cycles for the filter to work satisfactorily under current
			// cutoff frequency and resonance constraints is approximately 8
			int deltaFlt = 8;

			while (delta != 0)
			{
				if (delta < deltaFlt)
					deltaFlt = delta;

				// delta is converted to seconds given a 1 MHz clock by dividing
				// with 1 000 000. This is done in two operations to avoid integer
				// multiplication overflow

				// Calculate filter outputs.
				// Vhp = vbp/Q - Vlp - Vi;
				// dVbp = -w0*Vhp*dt;
				// dVlp = -w0*Vbp*dt;
				int w0Delta = w0CeilDt * deltaFlt >> 6;

				int dVbp = (w0Delta * vhp >> 14);
				int dVlp = (w0Delta * vbp >> 14);
				vbp -= dVbp;
				vlp -= dVlp;
				vhp = (vbp * _1024DivQ >> 10) - vlp - vi;

				delta -= deltaFlt;
			}
		}



		/********************************************************************/
		/// <summary>
		/// SID audio output (20 bits)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Output()
		{
			// This is handy for testing
			if (!enabled)
				return (vnf + mixerDc) * (int)vol;

			// Mix highpass, bandpass, and lowpass outputs. The sum is not
			// weighted, this can be confirmed by sampling sound output for
			// e.g. bandpass, lowpass, and bandpass+lowpass from a SID chip.

			// The code below is expanded to a switch for faster execution.
			// if (hp) Vf += Vhp;
			// if (bp) Vf += Vbp;
			// if (lp) Vf += Vlp;
			int vf;

			switch (hpBpLp)
			{
				default:
				case 0x0:
				{
					vf = 0;
					break;
				}

				case 0x1:
				{
					vf = vlp;
					break;
				}

				case 0x2:
				{
					vf = vbp;
					break;
				}

				case 0x3:
				{
					vf = vlp + vbp;
					break;
				}

				case 0x4:
				{
					vf = vhp;
					break;
				}

				case 0x5:
				{
					vf = vlp + vhp;
					break;
				}

				case 0x6:
				{
					vf = vbp + vhp;
					break;
				}

				case 0x7:
				{
					vf = vlp + vbp + vhp;
					break;
				}
			}

			// Sum non-filtered and filtered output.
			// Multiply the sum with volume
			return (vnf + vf + mixerDc) * (int)vol;
		}

		#region Register methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void WriteFcLo(uint fcLo)
		{
			fc = (fc & 0x7f8) | (fcLo & 0x007);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void WriteFcHi(uint fcHi)
		{
			fc = ((fcHi << 3) & 0x7f8) | (fc & 0x007);
			SetW0();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void WriteResFilt(uint resFilt)
		{
			res = (resFilt >> 4) & 0x0f;
			SetQ();

			filt = resFilt & 0x0f;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void WriteModeVol(uint modeVol)
		{
			voice3Off = modeVol & 0x80;

			hpBpLp = (modeVol >> 4) & 0x07;

			vol = modeVol & 0x0f;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Set filter cutoff frequency
		/// </summary>
		/********************************************************************/
		private void SetW0()
		{
			// Multiply with 1.048576 to facilitate division by 1 000 000 by right-
			// shifting 20 times (2 ^ 20 = 1048576)
			w0 = (int)(2 * Math.PI * f0[fc] * 1.048576);

			// Limit f0 to 16 KHz to keep 1 cycle filter stable
			int w0Max1 = (int)(2 * Math.PI * 16000 * 1.048576);
			w0Ceil1 = w0 <= w0Max1 ? w0 : w0Max1;

			// Limit f0 to 4 KHz to keep delta_t cycle filter stable
			int w0MaxDt = (int)(2 * Math.PI * 4000 * 1.048576);
			w0CeilDt = w0 <= w0MaxDt ? w0 : w0MaxDt;
		}



		/********************************************************************/
		/// <summary>
		/// Set filter resonance
		/// </summary>
		/********************************************************************/
		private void SetQ()
		{
			// Q is controlled linearly by res. Q has approximate range [0.707, 1.7].
			// As resonance is increased, the filter must be clocked more often to keep
			// stable.
			//
			// The coefficient 1024 is dispensed of later by right-shifting 10 times
			// (2 ^ 10 = 1024)
			_1024DivQ = (int)(1024.0 / (0.707 + 1.0 * res / 0x0f));
		}
		#endregion
	}
}
