/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibReSidFp.Resample
{
	/// <summary>
	/// Pass through with no resampling
	/// </summary>
	internal sealed class PassThrough : Resampler
	{
		// Last sample
		internal int32_t outputValue = 0;

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override bool Input(int32_t sample)
		{
			outputValue = sample;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override int32_t Output()
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
			outputValue = 0;
		}
		#endregion
	}
}
