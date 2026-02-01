/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class RcOverride
	{
		/// <summary>
		/// 
		/// </summary>
		public c_int Start_Frame;

		/// <summary>
		/// 
		/// </summary>
		public c_int End_Frame;

		/// <summary>
		/// If this is 0 then quality_factor will be used instead
		/// </summary>
		public c_int QScale;

		/// <summary>
		/// 
		/// </summary>
		public c_float Quality_Factor;
	}
}
