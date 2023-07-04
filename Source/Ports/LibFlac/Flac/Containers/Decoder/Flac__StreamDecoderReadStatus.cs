/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Decoder
{
	/// <summary>
	/// Return values for the Flac__StreamDecoder read callback
	/// </summary>
	public enum Flac__StreamDecoderReadStatus
	{
		/// <summary>
		/// The read was OK and decoding can continue
		/// </summary>
		Continue,

		/// <summary>
		/// The read was attempted while at the end of the stream. Note that
		/// the client must only return this value when the read callback was
		/// called when already at the end of the stream. Otherwise, if the read
		/// itself moves to the end of the stream, the client should still return
		/// the data and Continue, and then on the next read callback it should return
		/// End_Of_Stream with a byte count of 0
		/// </summary>
		End_Of_Stream,

		/// <summary>
		/// An unrecoverable error occurred. The decoder will return from the process call
		/// </summary>
		Abort
	}
}
