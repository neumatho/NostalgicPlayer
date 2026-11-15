/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibReSidFp.Resample
{
	/// <summary>
	/// Abstraction of a resampling process. Given enough input, produces output.
	/// Constructors take additional arguments that configure these objects
	/// </summary>
	internal abstract class Resampler
	{
		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Input a sample into resampler. Output "true" when resampler is
		/// ready with new sample
		/// </summary>
		/********************************************************************/
		public abstract bool Input(int32_t sample);



		/********************************************************************/
		/// <summary>
		/// Output a sample from resampler
		/// </summary>
		/********************************************************************/
		public abstract int32_t Output();



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract void Reset();
		#endregion
	}
}
