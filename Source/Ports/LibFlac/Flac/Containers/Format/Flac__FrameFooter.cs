/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format
{
	/// <summary>
	/// FLAC frame footer structure
	/// </summary>
	public class Flac__FrameFooter
	{
		/// <summary>
		/// CRC-16 (polynomial = x^16 + x^15 + x^2 + x^0, initialized with 0)
		/// of the bytes before the crc, back to and including the frame header
		/// sync code
		/// </summary>
		public Flac__uint16 Crc;
	}
}
