/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format
{
	/// <summary>
	/// Verbatim subframe
	/// </summary>
	public class Flac__SubFrame_Verbatim : ISubFrame
	{
		/// <summary>
		/// A pointer to verbatim signal
		/// </summary>
		public Flac__int32[] Data;
	}
}