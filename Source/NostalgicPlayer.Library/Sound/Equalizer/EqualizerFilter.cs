using Polycode.NostalgicPlayer.Kit.Utility;
using System;

namespace Polycode.NostalgicPlayer.Library.Sound.Equalizer
{
	/// <summary>
	///     10-band graphic equalizer (Winamp-style)
	///     Frequency bands: 60, 170, 310, 600, 1000, 3000, 6000, 12000, 14000, 16000 Hz
	///     Gain range: -12 dB to +12 dB per band (UI), internally scaled
	///     to ±6 dB for cleaner sound
	/// </summary>
	internal class EqualizerFilter
	{
		private const int BandCount = 10;
		private const int Scale = 24;
		private const long ScaleFactor = 1L << Scale;

		// 10 frequency bands (Hz)
		private static readonly double[] Frequencies = {60, 170, 310, 600, 1000, 3000, 6000, 12000, 14000, 16000};

		private readonly BiquadCoefficients[][] coefficients; // [band][channel]
		private readonly FilterState[][][] filterStates; // [band][channel][state]
		private readonly double[] gains; // Current gain for each band in dB
		private double preAmpGain; // Pre-amplifier gain in dB (applied to all frequencies)
		private readonly int outputChannelCount;
		private readonly int sampleRate;

		/********************************************************************/
		/// <summary>
		///     Constructor
		/// </summary>
		/********************************************************************/
		public EqualizerFilter(int sampleRate, int outputChannelCount)
		{
			this.sampleRate = sampleRate;
			this.outputChannelCount = outputChannelCount;
			gains = new double[BandCount];

			// Initialize coefficients for each band
			coefficients = new BiquadCoefficients[BandCount][];
			filterStates = new FilterState[BandCount][][];

			for (int band = 0; band < BandCount; band++)
			{
				coefficients[band] = new BiquadCoefficients[outputChannelCount];
				filterStates[band] = new FilterState[outputChannelCount][];

				for (int ch = 0; ch < outputChannelCount; ch++)
				{
					coefficients[band][ch] = new BiquadCoefficients();
					filterStates[band][ch] = ArrayHelper.InitializeArray<FilterState>(1);
				}
			}

			// Initialize with flat response (0 dB gain on all bands)
			for (int i = 0; i < BandCount; i++) gains[i] = 0.0;

			// Initialize pre-amp with 0 dB (no attenuation/boost)
			preAmpGain = 0.0;

			UpdateCoefficients();
		}

		/********************************************************************/
		/// <summary>
		///     Update biquad filter coefficients based on current gains
		/// </summary>
		/********************************************************************/
		private void UpdateCoefficients()
		{
			for (int band = 0; band < BandCount; band++)
			{
				double frequency = Frequencies[band];
				// Apply 0.5 scaling here when converting to linear gain for cleaner sound
			double gainDb = gains[band] * 0.5;
				double gain = Math.Pow(10.0, gainDb / 20.0); // Convert dB to linear gain

				// Calculate Q factor (bandwidth) - use wider bandwidth for high
				// frequencies to reduce harshness
				// Lower Q = wider bandwidth = smoother response
				double q;
				if (frequency >= 12000)
					// Very wide for ultra-high frequencies (12kHz, 14kHz, 16kHz)
					q = 0.7;
				else if (frequency >= 6000)
					q = 0.85; // Wide for high frequencies (6kHz)
				else if (frequency >= 3000)
					q = 1.0; // Standard for mid-high (3kHz)
				else
					// Slightly narrow for bass/mid (60Hz-1kHz) for more precision
					q = 1.2;

				// Calculate intermediate values
				double omega = 2.0 * Math.PI * frequency / sampleRate;
				double sn = Math.Sin(omega);
				double cs = Math.Cos(omega);
				double alpha = sn / (2.0 * q);

				// Peak EQ coefficients (boost/cut at center frequency)
				double a0 = 1.0 + (alpha / gain);
				double a1Coef = -2.0 * cs;
				double a2Coef = 1.0 - (alpha / gain);
				double b0Coef = 1.0 + (alpha * gain);
				double b1Coef = -2.0 * cs;
				double b2Coef = 1.0 - (alpha * gain);

				// Normalize and convert to fixed-point
				double norm = 1.0 / a0;

				for (int ch = 0; ch < outputChannelCount; ch++)
				{
					coefficients[band][ch].B0 = (long)(b0Coef * norm * ScaleFactor);
					coefficients[band][ch].B1 = (long)(b1Coef * norm * ScaleFactor);
					coefficients[band][ch].B2 = (long)(b2Coef * norm * ScaleFactor);
					coefficients[band][ch].A1 = (long)(a1Coef * norm * ScaleFactor);
					coefficients[band][ch].A2 = (long)(a2Coef * norm * ScaleFactor);
				}
			}
		}

		/********************************************************************/
		/// <summary>
		///     Apply equalizer to audio buffer
		/// </summary>
		/********************************************************************/
		public void Apply(Span<int> buffer, int framesTodo)
		{
			int todoInSamples = framesTodo * outputChannelCount;

			// Process each band sequentially (cascade of filters)
			for (int band = 0; band < BandCount; band++)
			{
				// Skip bands with 0 dB gain (optimization)
				if (Math.Abs(gains[band]) < 0.01) continue;

				for (int ch = 0; ch < outputChannelCount; ch++)
				{
					BiquadCoefficients coef = coefficients[band][ch];
					FilterState state = filterStates[band][ch][0];

					for (int i = ch; i < todoInSamples; i += outputChannelCount)
					{
						long input = buffer[i];
						long output = ((coef.B0 * input) + (coef.B1 * state.X1) + (coef.B2 * state.X2)
						               - (coef.A1 * state.Y1) - (coef.A2 * state.Y2)) >> Scale;

						state.X2 = state.X1;
						state.X1 = input;
						state.Y2 = state.Y1;
						state.Y1 = output;

						// Clamp to prevent overflow
						if (output < int.MinValue)
							output = int.MinValue;
						else if (output > int.MaxValue) output = int.MaxValue;

						buffer[i] = (int)output;
					}
				}
			}

			// Apply pre-amp gain if set
			if (Math.Abs(preAmpGain) > 0.01)
			{
				double preAmpLinear = Math.Pow(10.0, preAmpGain / 20.0);
				long preAmpFixed = (long)(preAmpLinear * ScaleFactor);

				for (int i = 0; i < todoInSamples; i++)
				{
					long output = (buffer[i] * preAmpFixed) >> Scale;

					// Clamp to prevent overflow
					if (output < int.MinValue)
						output = int.MinValue;
					else if (output > int.MaxValue)
						output = int.MaxValue;

					buffer[i] = (int)output;
				}
			}
		}

		/********************************************************************/
		/// <summary>
		///     Get current band gains
		/// </summary>
		/********************************************************************/
		public double[] GetGains() => (double[])gains.Clone();

		/********************************************************************/
		/// <summary>
		///     Get current pre-amp gain
		/// </summary>
		/********************************************************************/
		public double GetPreAmpGain() => preAmpGain;

		/********************************************************************/
		/// <summary>
		///     Set pre-amp gain
		/// </summary>
		/// <param name="gainDb">Gain in dB (-12.0 to +12.0)</param>
		/********************************************************************/
		public void SetPreAmpGain(double gainDb)
		{
			// Clamp to ±12 dB
			preAmpGain = Math.Max(-12.0, Math.Min(12.0, gainDb));
		}

		/********************************************************************/
		/// <summary>
		///     Reset equalizer to flat response
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			for (int i = 0; i < BandCount; i++) gains[i] = 0.0;
			preAmpGain = 0.0;

			UpdateCoefficients();

			// Clear filter states
			for (int band = 0; band < BandCount; band++)
			for (int ch = 0; ch < outputChannelCount; ch++)
			{
				filterStates[band][ch][0].X1 = 0;
				filterStates[band][ch][0].X2 = 0;
				filterStates[band][ch][0].Y1 = 0;
				filterStates[band][ch][0].Y2 = 0;
			}
		}

		/********************************************************************/
		/// <summary>
		///     Set all band gains at once
		/// </summary>
		/********************************************************************/
		public void SetAllGains(double[] bandGains)
		{
			if (bandGains == null || bandGains.Length != BandCount) return;

			for (int i = 0; i < BandCount; i++)
				// Clamp to ±12 dB, then scale by 0.5 for cleaner sound
				gains[i] = Math.Max(-12.0, Math.Min(12.0, bandGains[i]));

			UpdateCoefficients();
		}

		/********************************************************************/
		/// <summary>
		///     Set gain for a specific band
		/// </summary>
		/// <param name="band">Band index (0-9)</param>
		/// <param name="gainDb">Gain in dB (-12.0 to +12.0)</param>
		/********************************************************************/
		public void SetBandGain(int band, double gainDb)
		{
			if (band < 0 || band >= BandCount) return;

			// Clamp to ±12 dB, then scale by 0.5 for cleaner sound
			gainDb = Math.Max(-12.0, Math.Min(12.0, gainDb));
			gains[band] = gainDb;

			UpdateCoefficients();
		}

		// Biquad filter coefficients for each band
		private class BiquadCoefficients
		{
			public long A1
			{
				get;
				set;
			}

			public long A2
			{
				get;
				set;
			}

			public long B0
			{
				get;
				set;
			}

			public long B1
			{
				get;
				set;
			}

			public long B2
			{
				get;
				set;
			}
		}

		// Filter state for each band and channel
		private class FilterState
		{
			public long X1
			{
				get;
				set;
			}

			public long X2
			{
				get;
				set;
			}

			public long Y1
			{
				get;
				set;
			}

			public long Y2
			{
				get;
				set;
			}
		}
	}
}