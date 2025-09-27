/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibReSidFp
{
	/// <summary>
	/// 
	/// </summary>
	internal class ExternalFilter
	{
		// The audio output stage in a Commodore 64 consists of two STC networks, a
		// low-pass RC filter with 3 dB frequency 16kHz followed by a DC-blocker which
		// acts as a high-pass filter with a cutoff dependent on the attached audio
		// equipment impedance. Here we suppose an impedance of 10kOhm resulting
		// in a 3 dB attenuation at 1.6Hz.
		//
		// ~~~
		//                                 9/12V
		// -----+
		// audio|       10k                  |
		//      +---o----R---o--------o-----(K)          +-----
		//  out |   |        |        |      |           |audio
		// -----+   R 1k     C 1000   |      |    10 uF  |
		//          |        |  pF    +-C----o-----C-----+ 10k
		//                             470   |           |
		//         GND      GND         pF   R 1K        | amp
		//          *                   **   |           +-----
		//
		//                                  GND
		// ~~~
		//
		// The STC networks are connected with a [BJT] based [common collector]
		// used as a voltage follower (featuring a 2SC1815 NPN transistor).
		//
		// * To operate properly the 6581 audio output needs a pull-down resistor
		//   (1KOhm recommended, not needed on 8580)
		// ** The C64c board additionally includes a [bootstrap] condenser to increase
		//    the input impedance of the common collector.
		//
		// [BJT]: https://en.wikipedia.org/wiki/Bipolar_junction_transistor
		// [common collector]: https://en.wikipedia.org/wiki/Common_collector
		// [bootstrap]: https://en.wikipedia.org/wiki/Bootstrapping_(electronics)

		/// <summary>
		/// Lowpass filter voltage
		/// </summary>
		private int vlp;

		/// <summary>
		/// Highpass filter voltage
		/// </summary>
		private int vhp;

		private int w0lp_1_s7 = 0;

		private int w0hp_1_s17 = 0;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ExternalFilter()
		{
			Reset();
		}



		/********************************************************************/
		/// <summary>
		/// Setup of the external filter sampling parameters
		/// </summary>
		/********************************************************************/
		public void SetClockFrequency(double frequency)
		{
			double dt = 1.0 / frequency;

			// Low-pass:  R = 10kOhm, C = 1000pF; w0l = dt/(dt+RC) = 1e-6/(1e-6+1e4*1e-9) = 0.091
			// Cutoff 1/2*PI*RC = 1/2*PI*1e4*1e-9 = 15915.5 Hz
			w0lp_1_s7 = (int)((dt / (dt + GetRc(10e3, 1000e-12))) * (1 << 7) + 0.5);

			// High-pass: R = 10kOhm, C = 10uF;   w0h = dt/(dt+RC) = 1e-6/(1e-6+1e4*1e-5) = 0.00000999
			// Cutoff 1/2*PI*RC = 1/2*PI*1e4*1e-5 = 1.59155 Hz
			w0hp_1_s17 = (int)((dt / (dt + GetRc(10e3, 10e-6))) * (1 << 17) + 0.5);
		}



		/********************************************************************/
		/// <summary>
		/// SID reset
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			// State of filter
			vlp = 0;	// 1 << (15 + 11)
			vhp = 0;
		}



		/********************************************************************/
		/// <summary>
		/// SID clocking
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Clock(int input)
		{
			int vi = input << 11;
			int dVlp = (w0lp_1_s7 * (vi - vlp) >> 7);
			int dVhp = (w0hp_1_s17 * (vlp - vhp) >> 17);

			vlp += dVlp;
			vhp += dVhp;

			return (vlp - vhp) >> 11;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Get the 3 dB attenuation point
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private double GetRc(double res, double cap)
		{
			return res * cap;
		}
		#endregion
	}
}
