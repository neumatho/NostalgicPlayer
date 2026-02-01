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
	public static class FFTxCodeletPriority
	{
		/// <summary>
		/// Baseline priority
		/// </summary>
		public const c_int Base = 0;

		/// <summary>
		/// For naive implementations
		/// </summary>
		public const c_int Min = -131072;

		/// <summary>
		/// For custom implementations/ASICs
		/// </summary>
		public const c_int Max = 32768;
	}
}
