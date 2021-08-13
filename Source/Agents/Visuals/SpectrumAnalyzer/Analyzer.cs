/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Diagnostics;
using NAudio.Dsp;

namespace Polycode.NostalgicPlayer.Agent.Visual.SpectrumAnalyzer
{
	/// <summary>
	/// This class is based on the SampleAggregator in NAudio, but
	/// has been modified to only include my needs
	/// </summary>
	internal class Analyzer
	{
		#region FftEventArgs class
		public class FftEventArgs : EventArgs
		{
			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			[DebuggerStepThrough]
			public FftEventArgs(Complex[] result)
			{
				Result = result;
			}



			/********************************************************************/
			/// <summary>
			/// Holds the result from the FFT
			/// </summary>
			/********************************************************************/
			public Complex[] Result
			{
				get;
			}
		}
		#endregion

		private int fftPos;
		private readonly int fftLength;
		private readonly int m;

		private readonly Complex[] fftBuffer;
		private readonly FftEventArgs fftArgs;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Analyzer(int fftLength)
		{
			this.fftLength = fftLength;
			fftPos = 0;

			m = (int)Math.Log(fftLength, 2.0);
			fftBuffer = new Complex[fftLength];
			fftArgs = new FftEventArgs(fftBuffer);
		}



		/********************************************************************/
		/// <summary>
		/// Add the given sound values to the FFT analyzer
		/// </summary>
		/********************************************************************/
		public void AddValues(int[] buffer, bool stereo)
		{
			if (FftCalculated != null)
			{
				if (stereo)
				{
					for (int i = 0, cnt = buffer.Length; i < cnt; i += 2)
					{
						double value = ((long)buffer[i] + buffer[i + 1]) / 2147483648f;

						fftBuffer[fftPos].X = (float)(value * FastFourierTransform.BlackmannHarrisWindow(fftPos, fftLength));
						fftBuffer[fftPos].Y = 0;

						fftPos++;
						if (fftPos >= fftBuffer.Length)
						{
							fftPos = 0;

							FastFourierTransform.FFT(true, m, fftBuffer);
							FftCalculated(this, fftArgs);
						}
					}

				}
				else
				{
					for (int i = 0, cnt = buffer.Length; i < cnt; i++)
					{
						double value = buffer[i] / 2147483648f;

						fftBuffer[fftPos].X = (float)(value * FastFourierTransform.HammingWindow(fftPos, fftLength));
						fftBuffer[fftPos].Y = 0;

						fftPos++;
						if (fftPos >= fftBuffer.Length)
						{
							fftPos = 0;

							FastFourierTransform.FFT(true, m, fftBuffer);
							FftCalculated(this, fftArgs);
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Event called when the FFT buffer has been filled
		/// </summary>
		/********************************************************************/
		public event EventHandler<FftEventArgs> FftCalculated;
	}
}
