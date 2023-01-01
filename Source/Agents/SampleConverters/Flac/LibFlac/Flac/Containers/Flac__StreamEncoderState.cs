/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers
{
	/// <summary>
	/// State values for a FLAC__StreamEncoder.
	///
	/// The encoder's state can be obtained by calling Flac__Stream_Encoder_Get_State().
	///
	/// If the encoder gets into any other state besides FLAC__STREAM_ENCODER_OK
	/// or FLAC__STREAM_ENCODER_UNINITIALIZED, it becomes invalid for encoding and
	/// must be deleted with Flac__Stream_Encoder_Delete()
	/// </summary>
	internal enum Flac__StreamEncoderState
	{
		/// <summary>
		/// The encoder is in the normal OK state and samples can be processed
		/// </summary>
		Ok,

		/// <summary>
		/// The encoder is in the uninitialized state; one of the
		/// Flac__Stream_Encoder_Init_*() functions must be called before samples
		/// can be processed
		/// </summary>
		Uninitialized,

		/// <summary>
		/// An error occurred in the underlying verify stream decoder;
		/// check Flac__Stream_Encoder_Get_Verify_Decoder_State()
		/// </summary>
		Verify_Decoder_Error,

		/// <summary>
		/// The verify decoder detected a mismatch between the original
		/// audio signal and the decoded audio signal
		/// </summary>
		Verify_Mismatch_In_Audio_Data,

		/// <summary>
		/// One of the callbacks returned a fatal error
		/// </summary>
		Client_Error,

		/// <summary>
		/// An I/O error occurred while opening/reading/writing a file
		/// </summary>
		IO_Error,

		/// <summary>
		/// An error occurred while writing the stream; usually, the
		/// write_callback returned an error
		/// </summary>
		Framing_Error,

		/// <summary>
		/// Memory allocation failed
		/// </summary>
		Memory_Allocation_Error
	}
}
