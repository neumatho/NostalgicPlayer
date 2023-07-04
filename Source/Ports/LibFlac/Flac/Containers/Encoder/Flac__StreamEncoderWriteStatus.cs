/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Encoder
{
	/// <summary>
	/// Return values for the Flac__StreamEncoder read callback
	/// </summary>
	public enum Flac__StreamEncoderWriteStatus
	{
		/// <summary>
		/// The write was OK and encoding can continue
		/// </summary>
		Ok,

		/// <summary>
		/// An unrecoverable error occurred. The encoder will return from the process call
		/// </summary>
		Fatal_Error
	}
}