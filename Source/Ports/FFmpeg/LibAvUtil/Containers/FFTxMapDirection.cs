/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public enum FFTxMapDirection
	{
		/// <summary>
		/// No map. Make a map up
		/// </summary>
		None = 0,

		/// <summary>
		/// Lookup table must be applied via dst[i] = src[lut[i]];
		/// </summary>
		Gather,

		/// <summary>
		/// Lookup table must be applied via dst[lut[i]] = src[i];
		/// </summary>
		Scatter
	}
}
