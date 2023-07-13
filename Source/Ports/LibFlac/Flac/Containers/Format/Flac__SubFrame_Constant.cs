/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format
{
	/// <summary>
	/// Constant subframe
	/// </summary>
	public class Flac__SubFrame_Constant : ISubFrame
	{
		/// <summary>
		/// The constant signal value
		/// </summary>
		public Flac__int64 Value;
	}
}
