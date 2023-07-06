/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.ReSidFp.Resample
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
		public static TwoPassSincResampler Create(double clockFrequency, double samplingFrequency, double highestAccurateFrequency)
		{
			// Calculation according to Laurent Ganier. It evaluates to about 120 kHz at typical settings.
			// Some testing around the chosen value seems to confirm that this does work
			double intermediateFrequency = 2.0 * highestAccurateFrequency + Math.Sqrt(2.0 * highestAccurateFrequency * clockFrequency * (samplingFrequency - 2.0 * highestAccurateFrequency) / samplingFrequency);

			return new TwoPassSincResampler(clockFrequency, samplingFrequency, highestAccurateFrequency, intermediateFrequency);
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
