/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.ReSidFp.Resample
{
	/// <summary>
	/// Abstraction of a resampling process. Given enough input, produces output.
	/// Constructors take additional arguments that configure these objects
	/// </summary>
	internal abstract class Resampler
	{
		/********************************************************************/
		/// <summary>
		/// Output a sample from resampler
		/// </summary>
		/********************************************************************/
		public short GetOutput()
		{
			return SoftClip(Output());
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Input a sample into resampler. Output "true" when resampler is
		/// ready with new sample
		/// </summary>
		/********************************************************************/
		public abstract bool Input(int sample);



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract void Reset();



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract int Output();
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected short SoftClip(int x)
		{
			int threshold = 28000;
			if (x < threshold)
				return (short)x;

			double t = threshold / 32768.0;
			double a = 1.0 - t;
			double b = 1.0 / a;

			double value = (x - threshold) / 32768.0;
			value = t + a * Math.Tanh(b * value);

			return (short)(value * 32768.0);
		}
		#endregion
	}
}
