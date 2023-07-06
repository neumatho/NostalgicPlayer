﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.ReSidFp
{
	/// <summary>
	/// MOS6581 filter emulation
	/// </summary>
	internal sealed class Filter6581 : Filter
	{
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
		// filter outputs with levels independent of Q, which corresponds with the
		// results obtained from a real SID.
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
		// In 2008, Michael Huth facilitated closer investigation of the SID 6581
		// filter circuit by publishing high quality microscope photographs of the die.
		// Tommi Lempinen has done an impressive work on re-vectorizing and annotating
		// the die photographs, substantially simplifying further analysis of the
		// filter circuit.
		//
		// The filter schematics below are reverse engineered from these re-vectorized
		// and annotated die photographs. While the filter first depicted in reSID 0.9
		// is a correct model of the basic filter, the schematics are now completed
		// with the audio mixer and output stage, including details on intended
		// relative resistor values. Also included are schematics for the NMOS FET
		// voltage controlled resistors (VCRs) used to control cutoff frequency, the
		// DAC which controls the VCRs, the NMOS op-amps, and the output buffer.
		//
		//
		// SID filter / mixer / output
		// ---------------------------
		//
		//               +---------------------------------------------------+
		//               |                                                   |
		//               |                        +--1R1-- \--+ D7           |
		//               |             +---R1--+  |           |              |
		//               |             |       |  o--2R1-- \--o D6           |
		//               |   +---------o--<A]--o--o           |     $17      |
		//               |   |                    o--4R1-- \--o D5  1=open   | (3.5R1)
		//               |   |                    |           |              |
		//               |   |                    +--8R1-- \--o D4           | (7.0R1)
		//               |   |                                |              |
		// $17           |   |                    (CAP2B)     |  (CAP1B)     |
		// 0=to mixer    |   +--R8--+  +---R8--+      +---C---o      +---C---o
		// 1=to filter   |          |  |       |      |       |      |       |
		//                ------R8--o--o--[A>--o--Rw--o--[A>--o--Rw--o--[A>--o
		//     ve (EXT IN)          |          |              |              |
		// D3  \ ---------------R8--o          |              | (CAP2A)      | (CAP1A)
		//     |   v3               |          | vhp          | vbp          | vlp
		// D2  |   \ -----------R8--o    +-----+              |              |
		//     |   |   v2           |    |                    |              |
		// D1  |   |   \ -------R8--o    |   +----------------+              |
		//     |   |   |   v1       |    |   |                               |
		// D0  |   |   |   \ ---R8--+    |   |   +---------------------------+
		//     |   |   |   |             |   |   |
		//     R6  R6  R6  R6            R6  R6  R6
		//     |   |   |   | $18         |   |   |  $18
		//     |    \  |   | D7: 1=open   \   \   \ D6 - D4: 0=open
		//     |   |   |   |             |   |   |
		//     +---o---o---o-------------o---o---+                         12V
		//                 |
		//                 |               D3 +--/ --1R2--+                 |
		//                 |   +---R8--+      |           |  +---R2--+      |
		//                 |   |       |   D2 o--/ --2R2--o  |       |  ||--+
		//                 +---o--[A>--o------o           o--o--[A>--o--||
		//                                 D1 o--/ --4R2--o (4.25R2)    ||--+
		//                        $18         |           |                 |
		//                        0=open   D0 +--/ --8R2--+ (8.75R2)        |
		//
		//                                                                  vo (AUDIO
		//                                                                      OUT)
		//
		//
		// v1  - voice 1
		// v2  - voice 2
		// v3  - voice 3
		// ve  - ext in
		// vhp - highpass output
		// vbp - bandpass output
		// vlp - lowpass output
		// vo  - audio out
		// [A> - single ended inverting op-amp (self-biased NMOS inverter)
		// Rn  - "resistors", implemented with custom NMOS FETs
		// Rw  - cutoff frequency resistor (VCR)
		// C   - capacitor
		// ~~~
		// Notes:
		//
		//     R2  ~  2.0*R1
		//     R6  ~  6.0*R1
		//     R8  ~  8.0*R1
		//     R24 ~ 24.0*R1
		//
		// The Rn "resistors" in the circuit are implemented with custom NMOS FETs,
		// probably because of space constraints on the SID die. The silicon substrate
		// is laid out in a narrow strip or "snake", with a strip length proportional
		// to the intended resistance. The polysilicon gate electrode covers the entire
		// silicon substrate and is fixed at 12V in order for the NMOS FET to operate
		// in triode mode (a.k.a. linear mode or ohmic mode).
		//
		// Even in "linear mode", an NMOS FET is only an approximation of a resistor,
		// as the apparant resistance increases with increasing drain-to-source
		// voltage. If the drain-to-source voltage should approach the gate voltage
		// of 12V, the NMOS FET will enter saturation mode (a.k.a. active mode), and
		// the NMOS FET will not operate anywhere like a resistor.
		//
		//
		//
		// NMOS FET voltage controlled resistor (VCR)
		// ------------------------------------------
		// ~~~
		//                Vw
		//
		//                |
		//                |
		//                R1
		//                |
		//         +--R1--o
		//         |    __|__
		//         |    -----
		//         |    |   |
		// vi -----o----+   +--o----- vo
		//         |           |
		//         +----R24----+
		//
		//
		// vi  - input
		// vo  - output
		// Rn  - "resistors", implemented with custom NMOS FETs
		// Vw  - voltage from 11-bit DAC (frequency cutoff control)
		// ~~~
		// Notes:
		//
		// An approximate value for R24 can be found by using the formula for the
		// filter cutoff frequency:
		//
		//     FCmin = 1/(2*pi*Rmax*C)
		//
		// Assuming that a the setting for minimum cutoff frequency in combination with
		// a low level input signal ensures that only negligible current will flow
		// through the transistor in the schematics above, values for FCmin and C can
		// be substituted in this formula to find Rmax.
		// Using C = 470pF and FCmin = 220Hz (measured value), we get:
		//
		//     FCmin = 1/(2*pi*Rmax*C)
		//     Rmax = 1/(2*pi*FCmin*C) = 1/(2*pi*220*470e-12) ~ 1.5MOhm
		//
		// From this it follows that:
		//     R24 =  Rmax   ~ 1.5MOhm
		//     R1  ~  R24/24 ~  64kOhm
		//     R2  ~  2.0*R1 ~ 128kOhm
		//     R6  ~  6.0*R1 ~ 384kOhm
		//     R8  ~  8.0*R1 ~ 512kOhm
		//
		// Note that these are only approximate values for one particular SID chip,
		// due to process variations the values can be substantially different in
		// other chips.
		//
		//
		//
		// Filter frequency cutoff DAC
		// ---------------------------
		//
		// ~~~
		//    12V  10   9   8   7   6   5   4   3   2   1   0   VGND
		//      |   |   |   |   |   |   |   |   |   |   |   |     |   Missing
		//     2R  2R  2R  2R  2R  2R  2R  2R  2R  2R  2R  2R    2R   termination
		//      |   |   |   |   |   |   |   |   |   |   |   |     |
		// Vw --o-R-o-R-o-R-o-R-o-R-o-R-o-R-o-R-o-R-o-R-o-R-o-   -+
		//
		//
		// Bit on:  12V
		// Bit off:  5V (VGND)
		// ~~~
		// As is the case with all MOS 6581 DACs, the termination to (virtual) ground
		// at bit 0 is missing.
		//
		// Furthermore, the control of the two VCRs imposes a load on the DAC output
		// which varies with the input signals to the VCRs. This can be seen from the
		// VCR figure above.
		//
		//
		// 
		// "Op-amp" (self-biased NMOS inverter)
		// ------------------------------------
		// ~~~
		//
		//                        12V
		//
		//                         |
		//             +-----------o
		//             |           |
		//             |    +------o
		//             |    |      |
		//             |    |  ||--+
		//             |    +--||
		//             |       ||--+
		//         ||--+           |
		// vi -----||              o---o----- vo
		//         ||--+           |   |
		//             |       ||--+   |
		//             |-------||      |
		//             |       ||--+   |
		//         ||--+           |   |
		//      +--||              |   |
		//      |  ||--+           |   |
		//      |      |           |   |
		//      |      +-----------o   |
		//      |                  |   |
		//      |                      |
		//      |                 GND  |
		//      |                      |
		//      +----------------------+
		//
		//
		// vi  - input
		// vo  - output
		// ~~~
		// Notes:
		//
		// The schematics above are laid out to show that the "op-amp" logically
		// consists of two building blocks; a saturated load NMOS inverter (on the
		// right hand side of the schematics) with a buffer / bias input stage
		// consisting of a variable saturated load NMOS inverter (on the left hand
		// side of the schematics).
		//
		// Provided a reasonably high input impedance and a reasonably low output
		// impedance, the "op-amp" can be modeled as a voltage transfer function
		// mapping input voltage to output voltage.
		//
		//
		//
		// Output buffer (NMOS voltage follower)
		// -------------------------------------
		// ~~~
		//
		//            12V
		//
		//             |
		//             |
		//         ||--+
		// vi -----||
		//         ||--+
		//             |
		//             o------ vo
		//             |     (AUDIO
		//            Rext    OUT)
		//             |
		//             |
		//
		//            GND
		//
		// vi   - input
		// vo   - output
		// Rext - external resistor, 1kOhm
		// ~~~
		// Notes:
		//
		// The external resistor Rext is needed to complete the NMOS voltage follower,
		// this resistor has a recommended value of 1kOhm.
		//
		// Die photographs show that actually, two NMOS transistors are used in the
		// voltage follower. However the two transistors are coupled in parallel (all
		// terminals are pairwise common), which implies that we can model the two
		// transistors as one.

		private readonly ushort[] f0_dac;

		private readonly ushort[][] mixer;
		private readonly ushort[][] summer;
		private readonly ushort[][] gain_res;
		private readonly ushort[][] gain_vol;

		private readonly int voiceScaleS11;
		private readonly int voiceDc;

		/// <summary>
		/// VCR + associated capacitor connected to highpass output
		/// </summary>
		private Integrator6581 hpIntegrator;

		/// <summary>
		/// VCR + associated capacitor connected to bandpass output
		/// </summary>
		private Integrator6581 bpIntegrator;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Filter6581()
		{
			f0_dac = FilterModelConfig6581.GetInstance().GetDac(0.5);
			mixer = FilterModelConfig6581.GetInstance().GetMixer();
			summer = FilterModelConfig6581.GetInstance().GetSummer();
			gain_res = FilterModelConfig6581.GetInstance().GetGainRes();
			gain_vol = FilterModelConfig6581.GetInstance().GetGainVol();
			voiceScaleS11 = FilterModelConfig6581.GetInstance().GetVoiceScaleS11();
			voiceDc = FilterModelConfig6581.GetInstance().GetNormalizedVoiceDc();
			hpIntegrator = FilterModelConfig6581.GetInstance().BuildIntegrator();
			bpIntegrator = FilterModelConfig6581.GetInstance().BuildIntegrator();

			Input(0);
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Set filter cutoff frequency
		/// </summary>
		/********************************************************************/
		protected override void UpdatedCenterFrequency()
		{
			ushort vw = f0_dac[fc];
			hpIntegrator.SetVw(vw);
			bpIntegrator.SetVw(vw);
		}



		/********************************************************************/
		/// <summary>
		/// Set filter resonance.
		///
		/// In the MOS 6581, 1/Q is controlled linearly by res
		/// </summary>
		/********************************************************************/
		protected override void UpdateResonance(byte res)
		{
			currentResonance = gain_res[res];
		}



		/********************************************************************/
		/// <summary>
		/// Mixing configuration modified (offsets change)
		/// </summary>
		/********************************************************************/
		protected override void UpdateMixing()
		{
			currentGain = gain_vol[vol];

			uint ni = 0;
			uint no = 0;

			if (filt1)
				ni++;
			else
				no++;

			if (filt2)
				ni++;
			else
				no++;

			if (filt3)
				ni++;
			else if (!voice3Off)
				no++;

			if (filtE)
				ni++;
			else
				no++;

			currentSummer = summer[ni];

			if (lp)
				no++;

			if (bp)
				no++;

			if (hp)
				no++;

			currentMixer = mixer[no];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override void Input(int sample)
		{
			ve = (sample * voiceScaleS11 * 3 >> 11) + mixer[0][0];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override ushort Clock(int voice1, int voice2, int voice3)
		{
			voice1 = (voice1 * voiceScaleS11 >> 15) + voiceDc;
			voice2 = (voice2 * voiceScaleS11 >> 15) + voiceDc;

			// Voice 3 is silenced by voice3Off if it is not routed through the filter
			voice3 = (filt3 || !voice3Off) ? (voice3 * voiceScaleS11 >> 15) + voiceDc : 0;

			int vi = 0;
			int vo = 0;

			if (filt1)
				vi += voice1;
			else
				vo += voice1;

			if (filt2)
				vi += voice2;
			else
				vo += voice2;

			if (filt3)
				vi += voice3;
			else
				vo += voice3;

			if (filtE)
				vi += ve;
			else
				vo += ve;

			vhp = currentSummer[currentResonance[vbp] + vlp + vi];
			vbp = hpIntegrator.Solve(vhp);
			vlp = bpIntegrator.Solve(vbp);

			if (lp)
				vo += vlp;

			if (bp)
				vo += vbp;

			if (hp)
				vo += vhp;

			return currentGain[currentMixer[vo]];
		}
		#endregion
	}
}
