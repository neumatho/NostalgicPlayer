/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers
{
	/// <summary>
	/// Possible return values for the Flac__Stream_Encoder_Init_*() functions
	/// </summary>
	internal enum Flac__StreamEncoderInitStatus
	{
		/// <summary>
		/// Initialization was successful
		/// </summary>
		Ok = 0,

		/// <summary>
		/// General failure to set up encoder; call Flac__Stream_Encoder_Get_State() for cause
		/// </summary>
		Encoder_Error,

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
		/// The encoder has an invalid setting for number of channels
		/// </summary>
		Invalid_Number_Of_Channels,

		/// <summary>
		/// The encoder has an invalid setting for bits-per-sample. FLAC
		/// supports 4-32 bps but the reference encoder currently supports
		/// only up to 24 bps
		/// </summary>
		Invalid_Bits_Per_Sample,

		/// <summary>
		/// The encoder has an invalid setting for the input sample rate
		/// </summary>
		Invalid_Sample_Rate,

		/// <summary>
		/// The encoder has an invalid setting for the block size
		/// </summary>
		Invalid_Block_Size,

		/// <summary>
		/// The encoder has an invalid setting for the maximum LPC order
		/// </summary>
		Invalid_Max_Lpc_Order,

		/// <summary>
		/// The encoder has an invalid setting for the precision of the
		/// quantized linear predictor coefficients
		/// </summary>
		Invalid_Qlp_Coeff_Precision,

		/// <summary>
		/// The specified block size is less than the maximum LPC order
		/// </summary>
		Block_Size_Too_Small_For_Lpc_Order,

		/// <summary>
		/// The encoder is bound to the Subset but other settings violate it
		/// </summary>
		Not_Streamable,

		/// <summary>
		/// The metadata input to the encoder is invalid, in one of the following
		/// ways:
		/// - Flac__Stream_Encoder_Set_Metadata() was called with a null pointer
		///   but a block count > 0
		/// - One of the metadata blocks contains an undefined type
		/// - It contains an illegal CUESHEET as checked by
		///   Flac__Format_CueSheet_Is_Legal()
		/// - It contains an illegal SEEKTABLE as checked by
		///   Flac__Format_SeekTable_Is_Legal()
		/// - It contains more than one SEEKTABLE block or more than one
		///   VORBIS_COMMENT block
		/// </summary>
		Invalid_Metadata,

		/// <summary>
		/// Flac__Stream_Encoder_Init_*() was called when the encoder was
		/// already initialized, usually because
		/// Flac__Stream_Encoder_Finish() was not called
		/// </summary>
		Already_Initialized
	}
}