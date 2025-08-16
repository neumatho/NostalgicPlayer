using System;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Library.Sound.Mixer
{
	/// <summary>
	/// This class is based on the information that the Milkytracker
	/// co-authors (Christopher O'Neill and Antti S. Lankila) has found out.
	///
	/// The Amiga contains an analog low-pass filter (reconstruction filter) which
	/// is external to Paula. The filter is a 12 dB/oct Butterworth low-pass filter
	/// at approximately 3.3 kHz. The filter can only be applied globally to all
	/// four channels. In models after the Amiga 1000, the brightness of the power
	/// LED is used to indicate the status of the filter. The filter is active when
	/// the LED is at normal brightness, and deactivated when dimmed (on early
	/// Amiga 500 models the LED went completely off). Models released before
	/// Amiga 1200 also have a static "tone knob" type low-pass filter that is
	/// enabled regardless of the optional "LED filter". This filter is a 6
	/// dB/oct low-pass filter with cutoff frequency at 4.5 or 5 kHz.
	///
	/// -----------------+- Rolloff ---+- Cutoff -------------
	/// Amiga 500        | 6 dB/oct    | 4900 Hz , RC-filter
	/// Amiga 500 (LED)  | 12 dB/oct   | 3275 Hz , Butterworth
	/// Amiga 1200       | 6 dB/oct    | 28867 Hz , As is
	/// Amiga 1200 (LED) | 2x12 dB/oct | 3275 Hz , Butterworth
	///
	/// | 2nd-order Butterworth s-domain coefficients are: |
	/// |                                                  |
	/// | b0 = 1.0  b1 = 0        b2 = 0                   |
	/// | a0 = 1    a1 = sqrt(2)  a2 = 1                   |
	/// |                                                  |
	/// ````````````````````````````````````````````````````
	///
	/// Only the LED filter is implemented here
	/// </summary>
	internal class AmigaFilter
	{
		private const double CutoffFrequency = 3275.0;
		private const int Scale = 24;
		private const long ScaleFactor = 1L << Scale;

		private class FilterValues
		{
			public long X1 { get; set; }
			public long X2 { get; set; }
			public long Y1 { get; set; }
			public long Y2 { get; set; }
		}

		private readonly long a1;
		private readonly long a2;
		private readonly long b0;
		private readonly long b1;
		private readonly long b2;

		private readonly FilterValues[] previousFilterValues;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AmigaFilter(int sampleRate, int outputChannelCount)
		{
			// S-domain coefficients
			double a0Coef = 1.0;
			double a1Coef = Math.Sqrt(2.0);
			double a2Coef = 1.0;
			double b0Coef = 1.0;
//			double b1Coef = 0.0;
//			double b2Coef = 0.0;

			// Bilinear transformation precalculations
			double omega = 2.0 * Math.PI * CutoffFrequency / sampleRate;
			double tanOmega = Math.Tan(omega / 2.0);

			// Z-domain coefficients (bilinear transformation)
			double norm = 1.0 / (a0Coef + a1Coef * tanOmega + a2Coef * tanOmega * tanOmega);
			b0 = (long)(b0Coef * tanOmega * tanOmega * norm * ScaleFactor);
			b1 = 2 * b0;
			b2 = b0;
			a1 = (long)(2.0 * (a2Coef * tanOmega * tanOmega - a0Coef) * norm * ScaleFactor);
			a2 = (long)((a0Coef - a1Coef * tanOmega + a2Coef * tanOmega * tanOmega) * norm * ScaleFactor);

			previousFilterValues = ArrayHelper.InitializeArray<FilterValues>(outputChannelCount);
		}



		/********************************************************************/
		/// <summary>
		/// Apply the filter to the buffer
		/// </summary>
		/********************************************************************/
		public void Apply(Span<int> buffer, int framesTodo)
		{
			int numberOfChannels = previousFilterValues.Length;
			int todoInSamples = framesTodo * numberOfChannels;

			for (int i = 0; i < numberOfChannels; i++)
			{
				FilterValues prevValues = previousFilterValues[i];

				for (int j = i; j < todoInSamples; j += numberOfChannels)
				{
					long input = buffer[j];
					long output = (b0 * input + b1 * prevValues.X1 + b2 * prevValues.X2 - a1 * prevValues.Y1 - a2 * prevValues.Y2) >> Scale;

					prevValues.X2 = prevValues.X1;
					prevValues.X1 = input;
					prevValues.Y2 = prevValues.Y1;
					prevValues.Y1 = output;

					if (output < int.MinValue)
						output = int.MinValue;
					else if (output > int.MaxValue)
						output = int.MaxValue;

					buffer[j] = (int)output;
				}
			}
		}
	}
}
