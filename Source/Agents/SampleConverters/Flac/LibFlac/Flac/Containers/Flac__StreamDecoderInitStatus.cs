/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers
{
	/// <summary>
	/// Possible return values for the Flac__Stream_Decoder_Init_*() functions
	/// </summary>
	internal enum Flac__StreamDecoderInitStatus
	{
		/// <summary>
		/// Initialization was successful
		/// </summary>
		Ok = 0,

		/// <summary>
		/// The library was not compiled with support for the given container
		/// format
		/// </summary>
		Unsupported_Container,

		/// <summary>
		/// A required callback was not supplied
		/// </summary>
		Invalid_Callbacks,

		/// <summary>
		/// An error occurred allocating memory
		/// </summary>
		Memory_Allocation_Error,

		/// <summary>
		/// File open failed in Flac__Stream_Decoder_Init_File()
		/// </summary>
		Error_Opening_File,

		/// <summary>
		/// Flac__Stream_Decoder_Init_*() was called when the decoder was
		/// already initialized, usually because
		/// Flac__Stream_Decoder_Finish() was not called
		/// </summary>
		Already_Initialized
	}
}
