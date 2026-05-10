/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
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
			return Limiter.SoftClip(@out);
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
	}
}
