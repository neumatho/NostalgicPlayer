/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Decoder
{
	/// <summary>
	/// State values for a Flac__StreamDecoder
	///
	/// The decoder's state can be obtained by calling Flac__Stream_Decoder_Get_State()
	/// </summary>
	public enum Flac__StreamDecoderState
	{
		/// <summary>
		/// The decoder is ready to search for metadata
		/// </summary>
		Search_For_Metadata,

		/// <summary>
		/// The decoder is ready to or is in the process of reading metadata
		/// </summary>
		Read_Metadata,

		/// <summary>
		/// The decoder is ready to or is in the process of searching for the frame sync code
		/// </summary>
		Search_For_Frame_Sync,

		/// <summary>
		/// The decoder is ready to or is in the process of reading a frame
		/// </summary>
		Read_Frame,

		/// <summary>
		/// The decoder has reached the end of the stream
		/// </summary>
		End_Of_Stream,

		/// <summary>
		/// An error occurred in the underlying Ogg layer
		/// </summary>
		Ogg_Error,

		/// <summary>
		/// An error occurred while seeking. The decoder must be flushed
		/// with Flac__Stream_Decoder_Flush() or reset with
		/// Flac__Stream_Decoder_Reset() before decoding can continue
		/// </summary>
		Seek_Error,

		/// <summary>
		/// The decoder was aborted by the read or write callback
		/// </summary>
		Aborted,

		/// <summary>
		/// An error occurred allocating memory. The decoder is in an invalid
		/// state and can no longer be used
		/// </summary>
		Memory_Allocation_Error,

		/// <summary>
		/// The decoder is in the uninitialized state; one of the
		/// Flac__Stream_Decoder_Init_*() functions must be called before samples
		/// can be processed
		/// </summary>
		Uninitialized,

		/// <summary>
		/// The decoder has reached the end of an Ogg FLAC chain link and a new
		/// link follows; FLAC__stream_decoder_finish_link() has to be called to
		/// progress. This state is only returned when decoding of chained
		/// streams is enabled with
		/// </summary>
		End_Of_Link
	}
}
