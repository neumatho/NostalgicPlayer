/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibReSidFp.Resample
{
	/// <summary>
	/// Compose a more efficient SINC from chaining two other SINCs
	/// </summary>
	internal sealed class TwoPassSincResampler : Resampler
	{
		private readonly SincResampler s1;
		private readonly SincResampler s2;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private TwoPassSincResampler(double clockFrequency, double samplingFrequency, double highestAccurateFrequency, double intermediateFrequency)
		{
			s1 = new SincResampler(clockFrequency, intermediateFrequency, highestAccurateFrequency);
			s2 = new SincResampler(intermediateFrequency, samplingFrequency, highestAccurateFrequency);
		}



		/********************************************************************/
		/// <summary>
		/// Named constructor
		/// </summary>
		/********************************************************************/
		public static TwoPassSincResampler Create(double clockFrequency, double samplingFrequency)
		{
			// Set the passband frequency slightly below half sampling frequency
			//   pass_freq <= 0.9*sample_freq/2
			//
			// This constraint ensures that the FIR table is not overfilled.
			// For higher sampling frequencies we're fine with 20KHz
			double halfFreq = (samplingFrequency > 44000) ? 20000 : samplingFrequency * 0.45;

			// Calculation according to Laurent Ganier.
			// It evaluates to about 120 kHz at typical settings.
			// Some testing around the chosen value seems to confirm that this does work
			double intermediateFrequency = 2.0 * halfFreq + Math.Sqrt(2.0 * halfFreq * clockFrequency * (samplingFrequency - 2.0 * halfFreq) / samplingFrequency);

			return new TwoPassSincResampler(clockFrequency, samplingFrequency, halfFreq, intermediateFrequency);
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override bool Input(int sample)
		{
			return s1.Input(sample) && s2.Input(s1.Output());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override int Output()
		{
			return s2.Output();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override void Reset()
		{
			s1.Reset();
			s2.Reset();
		}
		#endregion
	}
}
