/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp.Resample
{
	/// <summary>
	/// Return sample with linear interpolation
	/// </summary>
	internal sealed class ZeroOrderResampler : Resampler
	{
		/// <summary>
		/// Last sample
		/// </summary>
		private int cachedSample;

		/// <summary>
		/// Number of cycles per sample
		/// </summary>
		private readonly int cyclesPerSample;

		private int sampleOffset;

		/// <summary>
		/// Calculated sample
		/// </summary>
		private int outputValue;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ZeroOrderResampler(double clockFrequency, double samplingFrequency)
		{
			cachedSample = 0;
			cyclesPerSample = (int)(clockFrequency / samplingFrequency * 1024.0);
			sampleOffset = 0;
			outputValue = 0;
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override bool Input(int sample)
		{
			bool ready = false;

			if (sampleOffset < 1024)
			{
				outputValue = cachedSample + (sampleOffset * (sample - cachedSample) >> 10);
				ready = true;
				sampleOffset += cyclesPerSample;
			}

			sampleOffset -= 1024;

			cachedSample = sample;

			return ready;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override int Output()
		{
			return outputValue;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override void Reset()
		{
			sampleOffset = 0;
			cachedSample = 0;
		}
		#endregion
	}
}
