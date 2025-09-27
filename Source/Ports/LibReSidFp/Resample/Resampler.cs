/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibReSidFp.Resample
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public short GetOutput(int scaleFactor)
		{
			int @out = (scaleFactor * Output()) / 2;
			return SoftClip(@out);
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
		/// Soft clipping into 16 bit range [-32768,32767]
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected short SoftClip(int x)
		{
			return (short)SoftClipImpl(x);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Soft clipping implementation, splitted for test
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static int SoftClipImpl(int x)
		{
			return x < 0 ? -Clipper(-x, 32768) : Clipper(x, 32767);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int Clipper(int x, int m)
		{
			int threshold = 28000;
			if (x < threshold)
				return (short)x;

			double max_val = m;
			double t = threshold / max_val;
			double a = 1.0 - t;
			double b = 1.0 / a;

			double value = (x - threshold) / max_val;
			value = t + a * Math.Tanh(b * value);

			return (int)(value * max_val);
		}
		#endregion
	}
}
