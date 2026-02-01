/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class FFTxCodeletOptions : IOptionContext
	{
		/// <summary>
		/// Request a specific lookup table direction. Codelets MUST put the
		/// direction in AVTXContext. If the codelet does not respect this, a
		/// conversion will be performed
		/// </summary>
		public FFTxMapDirection Map_Dir;
	}
}
