/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format
{
	/// <summary>
	/// An enumeration of the available subframe types
	/// </summary>
	public enum Flac__SubFrameType
	{
		/// <summary>
		/// Constant signal
		/// </summary>
		Constant = 0,

		/// <summary>
		/// Uncompressed signal
		/// </summary>
		Verbatim = 1,

		/// <summary>
		/// Fixed polynomial prediction
		/// </summary>
		Fixed = 2,

		/// <summary>
		/// Linear prediction
		/// </summary>
		Lpc = 3
	}
}
