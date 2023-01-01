/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Diagnostics;
using Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers;
using Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Private;
using Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Protected;
using Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Share;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac
{
	/// <summary>
	/// This module describes the decoder layers provided by libFLAC.
	///
	/// The stream decoder can be used to decode complete streams either from
	/// the client via callbacks, or directly from a file, depending on how
	/// it is initialized. When decoding via callbacks, the client provides
	/// callbacks for reading FLAC data and writing decoded samples, and
	/// handling metadata and errors. If the client also supplies seek-related
	/// callback, the decoder function for sample-accurate seeking within the
	/// FLAC input is also available. When decoding from a file, the client
	/// needs only supply a filename or open FILE* and write/metadata/error
	/// callbacks; the rest of the callbacks are supplied internally.
	///
	/// The stream decoder can decode native FLAC streams and files.
	///
	/// The basic usage of this decoder is as follows:
	///  - The program creates an instance of a decoder using
	///    Flac__Stream_Decoder_New().
	///  - The program overrides the default settings using
	///    Flac__Stream_Decoder_Set_*() functions.
	///  - The program initializes the instance to validate the settings and
	///    prepare for decoding using
	///    Flac__Stream_Decoder_Init_Stream() or Flac__Stream_Decoder_Init_File()
	///    for native FLAC.
	///  - The program calls the Flac__Stream_Decoder_Process_*() functions
	///    to decode data, which subsequently calls the callbacks.
	///  - The program finishes the decoding with Flac__Stream_Decoder_Finish(),
	///    which flushes the input and output and resets the decoder to the
	///    uninitialized state.
	///  - The instance may be used again or deleted with
	///    Flac__Stream_Decoder_Delete().
	///
	/// In more detail, the program will create a new instance by calling
	/// Flac__Stream_Decoder_New(), then call Flac__Stream_Decoder_Set_*()
	/// functions to override the default decoder options, and call
	/// one of the Flac__Stream_Decoder_Init_*() functions.
	///
	/// There are three initialization functions for native FLAC, one for
	/// setting up the decoder to decode FLAC data from the client via
	/// callbacks, and two for decoding directly from a FLAC file.
	///
	/// For decoding via callbacks, use Flac__Stream_Decoder_Init_Stream().
	/// You must also supply several callbacks for handling I/O. Some (like
	/// seeking) are optional, depending on the capabilities of the input.
	///
	/// For decoding directly from a file, use Flac__Stream_Decoder_Init_File()
	/// or Flac__Stream_Decoder_Init_File() with a .NET stream. Then you must
	/// only supply an open Stream or filename and fewer callbacks; the decoder
	/// will handle the other callbacks internally.
	///
	/// Once the decoder is initialized, your program will call one of several
	/// functions to start the decoding process:
	///
	///  - Flac__Stream_Decoder_Process_Single() - Tells the decoder to process at
	///    most one metadata block or audio frame and return, calling either the
	///    metadata callback or write callback, respectively, once. If the decoder
	///    loses sync it will return with only the error callback being called.
	///  - Flac__Stream_Decoder_Process_Until_End_Of_Metadata() - Tells the decoder
	///    to process the stream from the current location and stop upon reaching
	///    the first audio frame. The client will get one metadata, write, or error
	///    callback per metadata block, audio frame, or sync error, respectively.
	///  - Flac__Stream_Decoder_Process_Until_End_Of_Stream() - Tells the decoder
	///    to process the stream from the current location until the read callback
	///    returns FLAC__STREAM_DECODER_READ_STATUS_END_OF_STREAM or
	///    FLAC__STREAM_DECODER_READ_STATUS_ABORT. The client will get one metadata,
	///    write, or error callback per metadata block, audio frame, or sync error,
	///    respectively.
	///
	/// When the decoder has finished decoding (normally or through an abort),
	/// the instance is finished by calling Flac__Stream_Decoder_Finish(), which
	/// ensures the decoder is in the correct state and frees memory. Then the
	/// instance may be deleted with Flac__Stream_Decoder_Delete() or initialized
	/// again to decode another stream.
	///
	/// Seeking is exposed through the Flac__Stream_Decoder_Seek_Absolute() method.
	/// At any point after the stream decoder has been initialized, the client can
	/// call this function to seek to an exact sample within the stream.
	/// Subsequently, the first time the write callback is called it will be
	/// passed a (possibly partial) block starting at that sample.
	///
	/// If the client cannot seek via the callback interface provided, but still
	/// has another way of seeking, it can flush the decoder using
	/// Flac__Stream_Decoder_Flush() and start feeding data from the new position
	/// through the read callback.
	///
	/// The stream decoder also provides MD5 signature checking. If this is
	/// turned on before initialization, Flac__Stream_Decoder_Finish() will
	/// report when the decoded MD5 signature does not match the one stored
	/// in the STREAMINFO block. MD5 checking is automatically turned off
	/// (until the next Flac__Stream_Decoder_Reset()) if there is no signature
	/// in the STREAMINFO block or when a seek is attempted.
	///
	/// The Flac__Stream_Decoder_Set_Metadata_*() functions deserve special
	/// attention. By default, the decoder only calls the metadata_callback for
	/// the STREAMINFO block. These functions allow you to tell the decoder
	/// explicitly which blocks to parse and return via the metadata_callback
	/// and/or which to skip. Use a Flac__Stream_Decoder_Set_Metadata_Respond_All(),
	/// Flac__Stream_Decoder_Set_Metadata_Ignore() ... or Flac__Stream_Decoder_Set_Metadata_Ignore_All(),
	/// Flac__Stream_Decoder_Set_Metadata_Respond() ... sequence to exactly specify
	/// which blocks to return. Remember that metadata blocks can potentially
	/// be big (for example, cover art) so filtering out the ones you don't
	/// use can reduce the memory requirements of the decoder. Also note the
	/// special forms Flac__Stream_Decoder_Set_Metadata_Respond_Application(id)
	/// and Flac__Stream_Decoder_Set_Metadata_Ignore_Application(id) for
	/// filtering APPLICATION blocks based on the application ID.
	///
	/// STREAMINFO and SEEKTABLE blocks are always parsed and used internally, but
	/// they still can legally be filtered from the metadata_callback.
	///
	/// NOTE:
	/// The "set" functions may only be called when the decoder is in the
	/// state Flac__STREAM_DECODER_UNINITIALIZED, i.e. after
	/// Flac__Stream_Decoder_New() or Flac__Stream_Decoder_Finish(), but
	/// before Flac__Stream_Decoder_Init_*(). If this is the case they will
	/// return true, otherwise false.
	///
	/// NOTE:
	/// Flac__Stream_Decoder_Finish() resets all settings to the constructor
	/// defaults, including the callbacks.
	/// </summary>
	internal class Stream_Decoder
	{
		private static readonly Flac__byte[] id3V2_Tag = { 0x49, 0x44, 0x33 };	// ID3

		#region Delegates
		// NOTE: In general, Flac__StreamDecoder functions which change the
		// state should not be called on the decoder while in the callback

		/********************************************************************/
		/// <summary>
		/// This delegate will be called when the decoder needs more input
		/// data. The address of the buffer to be filled is supplied, along
		/// with the number of bytes the buffer can hold. The callback may
		/// choose to supply less data and modify the byte count but must be
		/// careful not to overflow the buffer. The callback then returns a
		/// status code chosen from Flac__StreamDecoderReadStatus
		/// </summary>
		/// <param name="decoder">The decoder instance calling the callback</param>
		/// <param name="buffer">An array for the callee to store data to be decoded</param>
		/// <param name="bytes">On entry to the callback, it contains the maximum number of bytes that may be stored in buffer. The callee must set it to the actual number of bytes stored (0 in case of error or end-of-stream) before returning</param>
		/// <param name="client_Data">The callee's client data set through Flac__Stream_Decoder_Init_*()</param>
		/// <returns>The callee's return status. Note that the callback should return End_Of_Stream if and only if zero bytes were read and there is no more data to be read</returns>
		/********************************************************************/
		public delegate Flac__StreamDecoderReadStatus Flac__StreamDecoderReadCallback(Stream_Decoder decoder, Span<Flac__byte> buffer, ref size_t bytes, object client_Data);



		/********************************************************************/
		/// <summary>
		/// This delegate will be called when the decoder needs to seek the
		/// input stream. The decoder will pass the absolute byte offset to
		/// seek to, 0 meaning the beginning of the stream
		/// </summary>
		/// <param name="decoder">The decoder instance calling the callback</param>
		/// <param name="absolute_Byte_Offset">The offset from the beginning of the stream to seek to</param>
		/// <param name="client_Data">The callee's client data set through Flac__Stream_Decoder_Init_*()</param>
		/// <returns>The callee's return status</returns>
		/********************************************************************/
		public delegate Flac__StreamDecoderSeekStatus Flac__StreamDecoderSeekCallback(Stream_Decoder decoder, Flac__uint64 absolute_Byte_Offset, object client_Data);



		/********************************************************************/
		/// <summary>
		/// This delegate will be called when the decoder wants to know the
		/// current position of the stream. The callback should return the
		/// byte offset from the beginning of the stream
		/// </summary>
		/// <param name="decoder">The decoder instance calling the callback</param>
		/// <param name="absolute_Byte_Offset">Store the current offset from the beginning of the steam here</param>
		/// <param name="client_Data">The callee's client data set through Flac__Stream_Decoder_Init_*()</param>
		/// <returns>The callee's return status</returns>
		/********************************************************************/
		public delegate Flac__StreamDecoderTellStatus Flac__StreamDecoderTellCallback(Stream_Decoder decoder, out Flac__uint64 absolute_Byte_Offset, object client_Data);



		/********************************************************************/
		/// <summary>
		/// This delegate will be called when the decoder wants to know the
		/// total length of the stream in bytes
		/// </summary>
		/// <param name="decoder">The decoder instance calling the callback</param>
		/// <param name="stream_Length">Store the length of the stream in bytes</param>
		/// <param name="client_Data">The callee's client data set through Flac__Stream_Decoder_Init_*()</param>
		/// <returns>The callee's return status</returns>
		/********************************************************************/
		public delegate Flac__StreamDecoderLengthStatus Flac__StreamDecoderLengthCallback(Stream_Decoder decoder, out Flac__uint64 stream_Length, object client_Data);



		/********************************************************************/
		/// <summary>
		/// This delegate will be called when the decoder needs to know the
		/// if the end of the stream has been reached
		/// </summary>
		/// <param name="decoder">The decoder instance calling the callback</param>
		/// <param name="client_Data">The callee's client data set through Flac__Stream_Decoder_Init_*()</param>
		/// <returns>True if the currently at the end of the stream, else false</returns>
		/********************************************************************/
		public delegate Flac__bool Flac__StreamDecoderEofCallback(Stream_Decoder decoder, object client_Data);



		/********************************************************************/
		/// <summary>
		/// This delegate will be called when the decoder has decoded a
		/// single audio frame. The decoder will pass the frame metadata as
		/// well as an array of pointers (one for each channel) pointing to
		/// the decoded audio
		/// </summary>
		/// <param name="decoder">The decoder instance calling the callback</param>
		/// <param name="frame">The description of the decoded frame</param>
		/// <param name="buffer">An array of pointers to decoded channels of data. Each pointer will point to an array of signed samples of length frame.Header.BlockSize. Channels will be ordered according to the FLAC specification</param>
		/// <param name="client_Data">The callee's client data set through Flac__Stream_Decoder_Init_*()</param>
		/// <returns>The callee's return status</returns>
		/********************************************************************/
		public delegate Flac__StreamDecoderWriteStatus Flac__StreamDecoderWriteCallback(Stream_Decoder decoder, Flac__Frame frame, Flac__int32[][] buffer, object client_Data);



		/********************************************************************/
		/// <summary>
		/// This delegate will be called when the decoder has decoded a
		/// metadata block. In a valid FLAC file there will always be one
		/// STREAMINFO block, followed by zero or more other metadata blocks.
		/// These will be supplied by the decoder in the same order as they
		/// appear in the stream and always before the first audio frame (i.e.
		/// write callback). The metadata block that is passed in must not be
		/// modified, and it doesn't live beyond the callback, so you should make
		/// a copy of it with Flac__Metadata_Object_Clone() if you will need it
		/// elsewhere. Since metadata blocks can potentially be large, by
		/// default the decoder only calls the metadata callback for the
		/// STREAMINFO block; you can instruct the decoder to pass or filter
		/// other blocks with Flac__Stream_Decoder_Set_Metadata_*() calls
		/// </summary>
		/// <param name="decoder">The decoder instance calling the callback</param>
		/// <param name="metadata">The decoded metadata block</param>
		/// <param name="client_Data">The callee's client data set through Flac__Stream_Decoder_Init_*()</param>
		/********************************************************************/
		public delegate void Flac__StreamDecoderMetadataCallback(Stream_Decoder decoder, Flac__StreamMetadata metadata, object client_Data);



		/********************************************************************/
		/// <summary>
		/// This delegate will be called whenever an error occurs during
		/// decoding
		/// </summary>
		/// <param name="decoder">The decoder instance calling the callback</param>
		/// <param name="status">The error encountered by the decoder</param>
		/// <param name="client_Data">The callee's client data set through Flac__Stream_Decoder_Init_*()</param>
		/********************************************************************/
		public delegate void Flac__StreamDecoderErrorCallback(Stream_Decoder decoder, Flac__StreamDecoderErrorStatus status, object client_Data);
		#endregion

		private class Flac__StreamDecoder
		{
			public Flac__StreamDecoderProtected Protected;
			public Flac__StreamDecoderPrivate Private;
		}

		private class Flac__StreamDecoderPrivate
		{
			public Flac__StreamDecoderReadCallback Read_Callback;
			public Flac__StreamDecoderSeekCallback Seek_Callback;
			public Flac__StreamDecoderTellCallback Tell_Callback;
			public Flac__StreamDecoderLengthCallback Length_Callback;
			public Flac__StreamDecoderEofCallback Eof_Callback;
			public Flac__StreamDecoderWriteCallback Write_Callback;
			public Flac__StreamDecoderMetadataCallback Metadata_Callback;
			public Flac__StreamDecoderErrorCallback Error_Callback;
			public object Client_Data;
			public ILpc Lpc;
			public Stream File;											// Only used if Flac__Stream_Decoder_Init_File() is called, else null
			public bool Leave_Stream_Open;
			public BitReader Input;
			public Flac__int32[][] Output = new Flac__int32[Constants.Flac__Max_Channels][];
			public Flac__int32[][] Residual = new Flac__int32[Constants.Flac__Max_Channels][];	// WATCHOUT: These are the aligned pointers; the real pointers that should be free()'d are residual_unaligned[] below
			public Flac__EntropyCodingMethod_PartitionedRiceContents[] Partitioned_Rice_Contents = Helpers.InitializeArray<Flac__EntropyCodingMethod_PartitionedRiceContents>((int)Constants.Flac__Max_Channels);
			public uint32_t Output_Capacity, Output_Channels;
			public Flac__uint32 Fixed_Block_Size, Next_Fixed_Block_Size;
			public Flac__uint64 Samples_Decoded;
			public Flac__bool Has_Stream_Info, Has_Seek_Table;
			public Flac__StreamMetadata Stream_Info = new Flac__StreamMetadata();
			public Flac__StreamMetadata Seek_Table = new Flac__StreamMetadata();
			public Flac__bool[] Metadata_Filter = new Flac__bool[128];	// MAGIC number 128 == total number of metadata block types == 1 << 7
			public Flac__byte[] Metadata_Filter_Ids;
			public size_t Metadata_Filter_Ids_Count, Metadata_Filter_Ids_Capacity;	// Units for both are IDs, not bytes
			public Flac__Frame Frame;
			public Flac__bool Cached;									// True if there is a byte in lookahead
			public Flac__byte[] Header_Warmup = new Flac__byte[2];		// Contains the sync code and reserved bits
			public Flac__byte Lookahead;								// Temp storage when we need to look ahead one byte in the stream
			public Flac__int32[][] Residual_Unaligned = new Flac__int32[Constants.Flac__Max_Channels][];		// Unaligned (original) pointers to allocated data
			public Flac__bool Do_Md5_Checking;							// Initially gets Protected.Md5_Checking but is turned off after a seek or if the metadata has a zero MD5
			public Flac__bool Internal_Reset_Hack;						// Used only during init() so we can call reset to set up the decoder without rewinding the input
			public Flac__bool Is_Seeking;
			public Md5 Md5;
			public Flac__byte[] Computed_Md5Sum;						// This is the sum we computed from the decoded data

			// The rese of these are only used for seeking
			public Flac__Frame Last_Frame;				// Holds the info of the last frame we seeked to
			public Flac__uint64 First_Frame_Offset;		// Hint to the seek routine of where in the stream the first audio frame starts
			public Flac__uint64 Target_Sample;
			public uint32_t Unparseable_Frame_Count;	// Used to tell whether we're decoding a future version of FLAC or just got a bad sync
		}

		private Flac__StreamDecoder decoder;

		#region Constructor/destructor
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Stream_Decoder()
		{
			decoder = new Flac__StreamDecoder();
			decoder.Protected = new Flac__StreamDecoderProtected();
			decoder.Private = new Flac__StreamDecoderPrivate();
		}



		/********************************************************************/
		/// <summary>
		/// Create a new stream decoder instance. The instance is created
		/// with default settings; see the individual
		/// Flac__Stream_Decoder_Set_*() functions for each setting's default
		/// </summary>
		/********************************************************************/
		public static Stream_Decoder Flac__Stream_Decoder_New()
		{
			Stream_Decoder decoder = new Stream_Decoder();

			decoder.decoder.Private.Input = BitReader.Flac__BitReader_New();
			if (decoder.decoder.Private.Input == null)
				return null;

			decoder.decoder.Private.Metadata_Filter_Ids_Capacity = 16;
			decoder.decoder.Private.Metadata_Filter_Ids = new Flac__byte[(Constants.Flac__Stream_Metadata_Application_Id_Len / 8) * decoder.decoder.Private.Metadata_Filter_Ids_Capacity];

			for (uint32_t i = 0; i < Constants.Flac__Max_Channels; i++)
			{
				decoder.decoder.Private.Output[i] = null;
				decoder.decoder.Private.Residual_Unaligned[i] = null;
				decoder.decoder.Private.Residual[i] = null;
			}

			decoder.decoder.Private.Output_Capacity = 0;
			decoder.decoder.Private.Output_Channels = 0;
			decoder.decoder.Private.Has_Seek_Table = false;

			for (uint32_t i = 0; i < Constants.Flac__Max_Channels; i++)
				Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Init(decoder.decoder.Private.Partitioned_Rice_Contents[i]);

			decoder.decoder.Private.File = null;

			Set_Defaults(decoder.decoder);

			decoder.decoder.Protected.State = Flac__StreamDecoderState.Uninitialized;

			return decoder;
		}



		/********************************************************************/
		/// <summary>
		/// Free a decoder instance
		/// </summary>
		/********************************************************************/
		public void Flac__Stream_Decoder_Delete()
		{
			if (decoder == null)
				return;

			Debug.Assert(decoder.Protected != null);
			Debug.Assert(decoder.Private != null);
			Debug.Assert(decoder.Private.Input != null);

			Flac__Stream_Decoder_Finish();

			if (decoder.Private.Metadata_Filter_Ids != null)
				decoder.Private.Metadata_Filter_Ids = null;

			decoder.Private.Input.Flac__BitReader_Delete();

			for (uint i = 0; i < Constants.Flac__Max_Channels; i++)
				Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Clear(decoder.Private.Partitioned_Rice_Contents[i]);

			decoder.Private = null;
			decoder.Protected = null;
			decoder = null;
		}
		#endregion

		#region Public methods
		/********************************************************************/
		/// <summary>
		/// Initialize the decoder instance to decode native FLAC streams.
		///
		/// This flavor of initialization sets up the decoder to decode from
		/// a native FLAC stream. I/O is performed via callbacks to the
		/// client. For decoding from a plain file via filename or open
		/// Stream, Flac__Stream_Decoder_Init_File() provide a simpler
		/// interface.
		///
		/// This function should be called after Flac__Stream_Decoder_New()
		/// and Flac__Stream_Decoder_Set_*() but before any of the
		/// Flac__Stream_Decoder_Process_*() functions. Will set and return
		/// the decoder state, which will be
		/// FLAC__STREAM_DECODER_SEARCH_FOR_METADATA if initialization
		/// succeeded
		/// </summary>
		/// <param name="read_Callback">See Flac__StreamDecoderReadCallback. This delegate must not be NULL</param>
		/// <param name="seek_Callback">See Flac__StreamDecoderSeekCallback. This delegate may be NULL if seeking is not supported. If seek_Callback is not NULL then a tell_Callback, length_Callback, and eof_Callback must also be supplied. Alternatively, a dummy seek callback that just returns FLAC__STREAM_DECODER_SEEK_STATUS_UNSUPPORTED may also be supplied, all though this is slightly less efficient for the decoder</param>
		/// <param name="tell_Callback">See Flac__StreamDecoderTellCallback. This delegate may be NULL if not supported by the client. If seek_Callback is not NULL then a tell_Callback must also be supplied. Alternatively, a dummy tell callback that just returns FLAC__STREAM_DECODER_TELL_STATUS_UNSUPPORTED may also be supplied, all though this is slightly less efficient for the decoder</param>
		/// <param name="length_Callback">See Flac__StreamDecoderLengthCallback. This delegate may be NULL if not supported by the client. If seek_Callback is not NULL then a Length_callback must also be supplied. Alternatively, a dummy length callback that just returns FLAC__STREAM_DECODER_LENGTH_STATUS_UNSUPPORTED may also be supplied, all though this is slightly less efficient for the decoder</param>
		/// <param name="eof_Callback">See Flac__StreamDecoderEofCallback. This delegate may be NULL if not supported by the client. If seek_Callback is not NULL then a eof_Callback must also be supplied. Alternatively, a dummy length callback that just returns false may also be supplied, all though this is slightly less efficient for the decoder</param>
		/// <param name="write_Callback">See Flac__StreamDecoderWriteCallback. This delegate must not be NULL</param>
		/// <param name="metadata_Callback">See Flac__StreamDecoderMetadataCallback. This delegate may be NULL if the callback is not desired</param>
		/// <param name="error_Callback">See Flac__StreamDecoderErrorCallback. This delegate must not be NULL</param>
		/// <param name="client_Data">This value will be supplied to callbacks in their client_Data argument</param>
		/// <returns>FLAC__STREAM_DECODER_INIT_STATUS_OK if initialization was successful; see Flac__StreamDecoderInitStatus for the meanings of other return values.</returns>
		/********************************************************************/
		public Flac__StreamDecoderInitStatus Flac__Stream_Decoder_Init_Stream(Flac__StreamDecoderReadCallback read_Callback, Flac__StreamDecoderSeekCallback seek_Callback, Flac__StreamDecoderTellCallback tell_Callback, Flac__StreamDecoderLengthCallback length_Callback, Flac__StreamDecoderEofCallback eof_Callback, Flac__StreamDecoderWriteCallback write_Callback, Flac__StreamDecoderMetadataCallback metadata_Callback, Flac__StreamDecoderErrorCallback error_Callback, object client_Data)
		{
			return Init_Stream_Internal(read_Callback, seek_Callback, tell_Callback, length_Callback, eof_Callback, write_Callback, metadata_Callback, error_Callback, client_Data);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the decoder instance to decode native FLAC streams.
		///
		/// This flavor of initialization sets up the decoder to decode from
		/// a plain native .NET FLAC stream. For non-stdio streams, you must
		/// use Flac__Stream_Decoder_Init_Stream() and provide callbacks for
		/// the I/O.
		///
		/// This function should be called after Flac__Stream_Decoder_New()
		/// and Flac__Stream_Decoder_Set_*() but before any of the
		/// Flac__Stream_Decoder_Process_*() functions. Will set and return
		/// the decoder state, which will be
		/// FLAC__STREAM_DECODER_SEARCH_FOR_METADATA if initialization
		/// succeeded
		/// </summary>
		/// <param name="file">An open FLAC file. The file should have been opened with read mode and rewound. The file becomes owned by the decoder and should not be manipulated by the client while decoding</param>
		/// <param name="leave_Open">If false, the stream will be closed when Flac__Stream_Decoder_Finish() is called</param>
		/// <param name="write_Callback">See Flac__StreamDecoderWriteCallback. This delegate must not be NULL</param>
		/// <param name="metadata_Callback">See Flac__StreamDecoderMetadataCallback. This delegate may be NULL if the callback is not desired</param>
		/// <param name="error_Callback">See Flac__StreamDecoderErrorCallback. This delegate must not be NULL</param>
		/// <param name="client_Data">This value will be supplied to callbacks in their client_Data argument</param>
		/// <returns>FLAC__STREAM_DECODER_INIT_STATUS_OK if initialization was successful; see Flac__StreamDecoderInitStatus for the meanings of other return values.</returns>
		/********************************************************************/
		public Flac__StreamDecoderInitStatus Flac__Stream_Decoder_Init_File(Stream file, bool leave_Open, Flac__StreamDecoderWriteCallback write_Callback, Flac__StreamDecoderMetadataCallback metadata_Callback, Flac__StreamDecoderErrorCallback error_Callback, object client_Data)
		{
			return Init_File_Internal(file, leave_Open, write_Callback, metadata_Callback, error_Callback, client_Data);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the decoder instance to decode native FLAC streams.
		///
		/// This flavor of initialization sets up the decoder to decode from
		/// a plain native FLAC file.
		///
		/// This function should be called after Flac__Stream_Decoder_New()
		/// and Flac__Stream_Decoder_Set_*() but before any of the
		/// Flac__Stream_Decoder_Process_*() functions. Will set and return
		/// the decoder state, which will be
		/// FLAC__STREAM_DECODER_SEARCH_FOR_METADATA if initialization
		/// succeeded
		/// </summary>
		/// <param name="filename">The name of the file to decode from</param>
		/// <param name="write_Callback">See Flac__StreamDecoderWriteCallback. This delegate must not be NULL</param>
		/// <param name="metadata_Callback">See Flac__StreamDecoderMetadataCallback. This delegate may be NULL if the callback is not desired</param>
		/// <param name="error_Callback">See Flac__StreamDecoderErrorCallback. This delegate must not be NULL</param>
		/// <param name="client_Data">This value will be supplied to callbacks in their client_Data argument</param>
		/// <returns>FLAC__STREAM_DECODER_INIT_STATUS_OK if initialization was successful; see Flac__StreamDecoderInitStatus for the meanings of other return values.</returns>
		/********************************************************************/
		public Flac__StreamDecoderInitStatus Flac__Stream_Decoder_Init_File(string filename, Flac__StreamDecoderWriteCallback write_Callback, Flac__StreamDecoderMetadataCallback metadata_Callback, Flac__StreamDecoderErrorCallback error_Callback, object client_Data)
		{
			return Init_File_Internal(filename, write_Callback, metadata_Callback, error_Callback, client_Data);
		}



		/********************************************************************/
		/// <summary>
		/// Finish the decoding process.
		/// Flushes the decoding buffer, releases resources, resets the
		/// decoder settings to their defaults, and returns the decoder state
		/// to FLAC__STREAM_DECODER_UNINITIALIZED.
		///
		/// In the event of a prematurely-terminated decode, it is not
		/// strictly necessary to call this immediately before
		/// Flac__Stream_Decoder_Delete() but it is good practice to match
		/// every Flac__Stream_Decoder_Init_*() with a
		/// Flac__Stream_Decoder_Finish().
		/// </summary>
		/// <returns>False if MD5 checking is on AND a STREAMINFO block was available AND the MD5 signature in the STREAMINFO block was non-zero AND the signature does not match the one computed by the decoder; else true</returns>
		/********************************************************************/
		public Flac__bool Flac__Stream_Decoder_Finish()
		{
			Flac__bool md5_Failed = false;

			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Private != null);
			Debug.Assert(decoder.Protected != null);

			if (decoder.Protected.State == Flac__StreamDecoderState.Uninitialized)
				return true;

			// See the comment in Flac__Stream_Decoder_Reset() as to why we
			// always call Flac__Md5Final()
			decoder.Private.Computed_Md5Sum = decoder.Private.Md5.Flac__Md5Final();

			if ((Flac__StreamMetadata_SeekTable)decoder.Private.Seek_Table.Data != null)
				((Flac__StreamMetadata_SeekTable)decoder.Private.Seek_Table.Data).Points = null;

			decoder.Private.Has_Seek_Table = false;

			decoder.Private.Input.Flac__BitReader_Free();

			for (uint i = 0; i < Constants.Flac__Max_Channels; i++)
			{
				if (decoder.Private.Output[i] != null)
					decoder.Private.Output[i] = null;

				if (decoder.Private.Residual_Unaligned[i] != null)
				{
					decoder.Private.Residual_Unaligned[i] = null;
					decoder.Private.Residual[i] = null;
				}
			}

			decoder.Private.Output_Capacity = 0;
			decoder.Private.Output_Channels = 0;

			if (decoder.Private.File != null)
			{
				if (!decoder.Private.Leave_Stream_Open)
					decoder.Private.File.Dispose();

				decoder.Private.File = null;
				decoder.Private.Leave_Stream_Open = false;
			}

			if (decoder.Private.Do_Md5_Checking)
			{
				if (!decoder.Private.Computed_Md5Sum.AsSpan().SequenceEqual(((Flac__StreamMetadata_StreamInfo)decoder.Private.Stream_Info.Data)?.Md5Sum))
					md5_Failed = true;
			}

			decoder.Private.Is_Seeking = false;

			Set_Defaults(decoder);

			decoder.Protected.State = Flac__StreamDecoderState.Uninitialized;

			return !md5_Failed;
		}



		/********************************************************************/
		/// <summary>
		/// Set the "MD5 signature checking" flag. If true, the decoder will
		/// compute the MD5 signature of the unencoded audio data while
		/// decoding and compare it to the signature from the STREAMINFO
		/// block, if it exists, during Flac__Stream_Decoder_Finish().
		///
		/// MD5 signature checking will be turned off (until the next
		/// Flac__Stream_Decoder_Reset()) if there is no signature in the
		/// STREAMINFO block or when a seek is attempted.
		///
		/// Clients that do not use the MD5 check should leave this off to
		/// speed up decoding.
		/// </summary>
		/// <param name="value">Flag value (see above)</param>
		/// <returns>False if the decoder is already initialized, else true</returns>
		/********************************************************************/
		public Flac__bool Flac__Stream_Decoder_Set_Md5_Checking(Flac__bool value)
		{
			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Protected != null);

			if (decoder.Protected.State != Flac__StreamDecoderState.Uninitialized)
				return false;

			decoder.Protected.Md5_Checking = value;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Direct the decoder to pass on all metadata blocks of the given
		/// type
		/// </summary>
		/// <param name="type">See above</param>
		/// <returns>False if the decoder is already initialized, else true</returns>
		/********************************************************************/
		public Flac__bool Flac__Stream_Decoder_Set_Metadata_Respond(Flac__MetadataType type)
		{
			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Private != null);
			Debug.Assert(decoder.Protected != null);
			Debug.Assert(type <= Flac__MetadataType.Max_Metadata_Type);

			// Double protection
			if (type > Flac__MetadataType.Max_Metadata_Type)
				return false;

			if (decoder.Protected.State != Flac__StreamDecoderState.Uninitialized)
				return false;

			decoder.Private.Metadata_Filter[(int32_t)type] = true;

			if (type == Flac__MetadataType.Application)
				decoder.Private.Metadata_Filter_Ids_Count = 0;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Direct the decoder to pass on all APPLICATION metadata blocks
		/// of the given id
		/// </summary>
		/// <param name="id">See above</param>
		/// <returns>False if the decoder is already initialized, else true</returns>
		/********************************************************************/
		public Flac__bool Flac__Stream_Decoder_Set_Metadata_Respond_Application(Flac__byte[] id)
		{
			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Private != null);
			Debug.Assert(decoder.Protected != null);
			Debug.Assert(id != null);

			if (decoder.Protected.State != Flac__StreamDecoderState.Uninitialized)
				return false;

			if (decoder.Private.Metadata_Filter[(int32_t)Flac__MetadataType.Application])
				return true;

			Debug.Assert(decoder.Private.Metadata_Filter_Ids != null);

			if (decoder.Private.Metadata_Filter_Ids_Count == decoder.Private.Metadata_Filter_Ids_Capacity)
			{
				decoder.Private.Metadata_Filter_Ids = Alloc.Safe_Realloc_Mul_2Op(decoder.Private.Metadata_Filter_Ids, decoder.Private.Metadata_Filter_Ids_Capacity, 2);
				if (decoder.Private.Metadata_Filter_Ids == null)
				{
					decoder.Protected.State = Flac__StreamDecoderState.Memory_Allocation_Error;
					return false;
				}

				decoder.Private.Metadata_Filter_Ids_Capacity *= 2;
			}

			Array.Copy(id, 0, decoder.Private.Metadata_Filter_Ids, decoder.Private.Metadata_Filter_Ids_Count * (Constants.Flac__Stream_Metadata_Application_Id_Len / 8), (Constants.Flac__Stream_Metadata_Application_Id_Len / 8));
			decoder.Private.Metadata_Filter_Ids_Count++;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Direct the decoder to pass on all metadata blocks of any type
		/// </summary>
		/// <returns>False if the decoder is already initialized, else true</returns>
		/********************************************************************/
		public Flac__bool Flac__Stream_Decoder_Set_Metadata_Respond_All()
		{
			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Private != null);
			Debug.Assert(decoder.Protected != null);

			if (decoder.Protected.State != Flac__StreamDecoderState.Uninitialized)
				return false;

			for (uint32_t i = 0; i < decoder.Private.Metadata_Filter.Length; i++)
				decoder.Private.Metadata_Filter[i] = true;

			decoder.Private.Metadata_Filter_Ids_Count = 0;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Direct the decoder to filter out all metadata blocks of the
		/// given type
		/// </summary>
		/// <param name="type">See above</param>
		/// <returns>False if the decoder is already initialized, else true</returns>
		/********************************************************************/
		public Flac__bool Flac__Stream_Decoder_Set_Metadata_Ignore(Flac__MetadataType type)
		{
			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Private != null);
			Debug.Assert(decoder.Protected != null);
			Debug.Assert(type <= Flac__MetadataType.Max_Metadata_Type);

			// Double protection
			if (type > Flac__MetadataType.Max_Metadata_Type)
				return false;

			if (decoder.Protected.State != Flac__StreamDecoderState.Uninitialized)
				return false;

			decoder.Private.Metadata_Filter[(int32_t)type] = false;

			if (type == Flac__MetadataType.Application)
				decoder.Private.Metadata_Filter_Ids_Count = 0;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Direct the decoder to filter out all APPLICATION metadata blocks
		/// of the given id
		/// </summary>
		/// <param name="id">See above</param>
		/// <returns>False if the decoder is already initialized, else true</returns>
		/********************************************************************/
		public Flac__bool Flac__Stream_Decoder_Set_Metadata_Ignore_Application(Flac__byte[] id)
		{
			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Private != null);
			Debug.Assert(decoder.Protected != null);
			Debug.Assert(id != null);

			if (decoder.Protected.State != Flac__StreamDecoderState.Uninitialized)
				return false;

			if (!decoder.Private.Metadata_Filter[(int32_t)Flac__MetadataType.Application])
				return true;

			Debug.Assert(decoder.Private.Metadata_Filter_Ids != null);

			if (decoder.Private.Metadata_Filter_Ids_Count == decoder.Private.Metadata_Filter_Ids_Capacity)
			{
				decoder.Private.Metadata_Filter_Ids = Alloc.Safe_Realloc_Mul_2Op(decoder.Private.Metadata_Filter_Ids, decoder.Private.Metadata_Filter_Ids_Capacity, 2);
				if (decoder.Private.Metadata_Filter_Ids == null)
				{
					decoder.Protected.State = Flac__StreamDecoderState.Memory_Allocation_Error;
					return false;
				}

				decoder.Private.Metadata_Filter_Ids_Capacity *= 2;
			}

			Array.Copy(id, 0, decoder.Private.Metadata_Filter_Ids, decoder.Private.Metadata_Filter_Ids_Count * (Constants.Flac__Stream_Metadata_Application_Id_Len / 8), (Constants.Flac__Stream_Metadata_Application_Id_Len / 8));
			decoder.Private.Metadata_Filter_Ids_Count++;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Direct the decoder to filter out all metadata blocks of any type
		/// </summary>
		/// <returns>False if the decoder is already initialized, else true</returns>
		/********************************************************************/
		public Flac__bool Flac__Stream_Decoder_Set_Metadata_Ignore_All()
		{
			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Private != null);
			Debug.Assert(decoder.Protected != null);

			if (decoder.Protected.State != Flac__StreamDecoderState.Uninitialized)
				return false;

			Array.Clear(decoder.Private.Metadata_Filter);
			decoder.Private.Metadata_Filter_Ids_Count = 0;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Get the current decoder state
		/// </summary>
		/// <returns>The current decoder state</returns>
		/********************************************************************/
		public Flac__StreamDecoderState Flac__Stream_Decoder_Get_State()
		{
			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Protected != null);

			return decoder.Protected.State;
		}



		/********************************************************************/
		/// <summary>
		/// Get the "MD5 signature checking" flag.
		/// This is the value of the setting, not whether or not the decoder
		/// is currently checking the MD5 (remember, it can be turned off
		/// automatically by a seek). When the decoder is reset the flag will
		/// be restored to the value returned by this function
		/// </summary>
		/// <returns>See above</returns>
		/********************************************************************/
		public Flac__bool Flac__Stream_Decoder_Get_Md5_Checking()
		{
			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Protected != null);

			return decoder.Protected.Md5_Checking;
		}



		/********************************************************************/
		/// <summary>
		/// Get the total number of samples in the stream being decoded.
		/// Will only be valid after decoding has started and will contain
		/// the value from the STREAMINFO block. A value of 0 means "unknown"
		/// </summary>
		/// <returns>See above</returns>
		/********************************************************************/
		public Flac__uint64 Flac__Stream_Decoder_Get_Total_Samples()
		{
			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Protected != null);

			return decoder.Private.Has_Stream_Info ? ((Flac__StreamMetadata_StreamInfo)decoder.Private.Stream_Info.Data).Total_Samples : 0;
		}



		/********************************************************************/
		/// <summary>
		/// Get the current number of channels in the stream being decoded.
		/// Will only be valid after decoding has started and will contain
		/// the value from the most recently decoded frame header
		/// </summary>
		/// <returns>See above</returns>
		/********************************************************************/
		public Flac__uint32 Flac__Stream_Decoder_Get_Channels()
		{
			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Protected != null);

			return decoder.Protected.Channels;
		}



		/********************************************************************/
		/// <summary>
		/// Get the current channel assignment in the stream being decoded.
		/// Will only be valid after decoding has started and will contain
		/// the value from the most recently decoded frame header
		/// </summary>
		/// <returns>See above</returns>
		/********************************************************************/
		public Flac__ChannelAssignment Flac__Stream_Decoder_Get_Channel_Assignment()
		{
			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Protected != null);

			return decoder.Protected.Channel_Assignment;
		}



		/********************************************************************/
		/// <summary>
		/// Get the current sample resolution in the stream being decoded.
		/// Will only be valid after decoding has started and will contain
		/// the value from the most recently decoded frame header
		/// </summary>
		/// <returns>See above</returns>
		/********************************************************************/
		public Flac__uint32 Flac__Stream_Decoder_Get_Bits_Per_Sample()
		{
			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Protected != null);

			return decoder.Protected.Bits_Per_Sample;
		}



		/********************************************************************/
		/// <summary>
		/// Get the current sample rate in Hz of the stream being decoded.
		/// Will only be valid after decoding has started and will contain
		/// the value from the most recently decoded frame header
		/// </summary>
		/// <returns>See above</returns>
		/********************************************************************/
		public Flac__uint32 Flac__Stream_Decoder_Get_Sample_Rate()
		{
			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Protected != null);

			return decoder.Protected.Sample_Rate;
		}



		/********************************************************************/
		/// <summary>
		/// Get the current block size of the stream being decoded.
		/// Will only be valid after decoding has started and will contain
		/// the value from the most recently decoded frame header
		/// </summary>
		/// <returns>See above</returns>
		/********************************************************************/
		public Flac__uint32 Flac__Stream_Decoder_Get_BlockSize()
		{
			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Protected != null);

			return decoder.Protected.BlockSize;
		}



		/********************************************************************/
		/// <summary>
		/// Flush the stream input.
		/// The decoder's input buffer will be cleared and the state set to
		/// FLAC__STREAM_DECODER_SEARCH_FOR_FRAME_SYNC. This will also turn
		/// off MD5 checking
		/// </summary>
		/// <returns>True if successful, else false if a memory allocation error occurs (in which case the state will be set to FLAC__STREAM_DECODER_MEMORY_ALLOCATION_ERROR)</returns>
		/********************************************************************/
		public Flac__bool Flac__Stream_Decoder_Flush()
		{
			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Private != null);
			Debug.Assert(decoder.Protected != null);

			if (!decoder.Private.Internal_Reset_Hack && (decoder.Protected.State == Flac__StreamDecoderState.Uninitialized))
				return false;

			decoder.Private.Samples_Decoded = 0;
			decoder.Private.Do_Md5_Checking = false;

			if (!decoder.Private.Input.Flac__BitReader_Clear())
			{
				decoder.Protected.State = Flac__StreamDecoderState.Memory_Allocation_Error;
				return false;
			}

			decoder.Protected.State = Flac__StreamDecoderState.Search_For_Frame_Sync;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the decoder's current read position within the stream.
		/// The position is the byte offset from the start of the stream.
		/// Bytes before this position have been fully decoded. Note that
		/// there may still be undecoded bytes in the decoder's read FIFO.
		/// The returned position is correct even after a seek.
		/// </summary>
		/// <param name="position">The current position</param>
		/// <returns>True if successful, false if the stream is not native FLAC, or there was an error from the 'tell' callback or it returned</returns>
		/********************************************************************/
		public Flac__bool Flac__Stream_Decoder_Get_Decode_Position(out Flac__uint64 position)
		{
			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Private != null);

			if (decoder.Private.Tell_Callback == null)
			{
				position = 0;
				return false;
			}

			if (decoder.Private.Tell_Callback(this, out position, decoder.Private.Client_Data) != Flac__StreamDecoderTellStatus.Ok)
				return false;

			// Should never happen since all FLAC frames and metadata blocks are byte aligned, but check just in case
			if (!decoder.Private.Input.Flac__BitReader_Is_Consumed_Byte_Aligned())
				return false;

			Debug.Assert(position >= Flac__Stream_Decoder_Get_Input_Bytes_Unconsumed());
			position -= Flac__Stream_Decoder_Get_Input_Bytes_Unconsumed();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Reset the decoding process.
		/// The decoder's input buffer will be cleared and the state set to
		/// FLAC__STREAM_DECODER_SEARCH_FOR_METADATA. This is similar to
		/// Flac__Stream_Decoder_Finish() except that the settings are
		/// preserved; there is no need to call Flac__Stream_Decoder_Init_*()
		/// before decoding again. MD5 checking will be restored to its
		/// original setting.
		///
		/// If the decoder is seekable, or was initialized with
		/// Flac__Stream_Decoder_Init*_File(), the decoder will also attempt
		/// to seek to the beginning of the file. If this rewind fails, this
		/// function will return false.
		///
		/// If the decoder was initialized with
		/// Flac__Stream_Decoder_Init*_Stream() and is not seekable (i.e. no
		/// seek callback was provided or the seek callback returns
		/// FLAC__STREAM_DECODER_SEEK_STATUS_UNSUPPORTED), it is the duty of
		/// the client to start feeding data from the beginning of the stream
		/// on the next FLAC__Stream_Decoder_Process_*() call.
		/// </summary>
		/// <returns>True if successful, else false if a memory allocation occurs (in which case the state will be set to FLAC__STREAM_DECODER_MEMORY_ALLOCATION_ERROR) or a seek error occurs (the state will be unchanged)</returns>
		/********************************************************************/
		public Flac__bool Flac__Stream_Decoder_Reset()
		{
			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Private != null);
			Debug.Assert(decoder.Protected != null);

			if (!Flac__Stream_Decoder_Flush())
			{
				// Above call sets the state for us
				return false;
			}

			// Rewind if necessary. If Flac__Stream_Decoder_Init() is calling us,
			// (internal reset hack) don't try to rewind since we are already at
			// the beginning of the stream and don't want to fail if the input is
			// not seekable
			if (!decoder.Private.Internal_Reset_Hack)
			{
				if ((decoder.Private.Seek_Callback != null) && (decoder.Private.Seek_Callback(this, 0, decoder.Private.Client_Data) == Flac__StreamDecoderSeekStatus.Error))
					return false;	// Seekable and seek fails, reset fails
			}
			else
				decoder.Private.Internal_Reset_Hack = false;

			decoder.Protected.State = Flac__StreamDecoderState.Search_For_Metadata;

			decoder.Private.Has_Stream_Info = false;

			if ((Flac__StreamMetadata_SeekTable)decoder.Private.Seek_Table.Data != null)
				((Flac__StreamMetadata_SeekTable)decoder.Private.Seek_Table.Data).Points = null;

			decoder.Private.Has_Seek_Table = false;

			decoder.Private.Do_Md5_Checking = decoder.Protected.Md5_Checking;

			// This goes in Reset() and not Flush() because according to the spec, a
			// fixed-blocksize stream must stay that way through the whole stream
			decoder.Private.Fixed_Block_Size = decoder.Private.Next_Fixed_Block_Size = 0;

			// We initialize the Flac__Md5Context even though we may never use it. This
			// is because MD5 checking may be turned on to start and then turned off if
			// a seek occurs. So we init the context here and finalize it in
			// Flac__Stream_Decoder_Finish() to make sure things are always cleaned up
			// properly
			decoder.Private.Md5 = new Md5();
			decoder.Private.Md5.Flac__Md5Init();

			decoder.Private.First_Frame_Offset = 0;
			decoder.Private.Unparseable_Frame_Count = 0;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Decode one metadata block or audio frame.
		/// This version instructs the decoder to decode a either a single
		/// metadata block or a single frame and stop, unless the callbacks
		/// return a fatal error or the read callback returns
		/// FLAC__STREAM_DECODER_READ_STATUS_END_OF_STREAM.
		///
		/// As the decoder needs more input it will call the read callback.
		/// Depending on what was decoded, the metadata or write callback
		/// will be called with the decoded metadata block or audio frame.
		///
		/// Unless there is a fatal read error or end of stream, this function
		/// will return once one whole frame is decoded. In other words, if
		/// the stream is not synchronized or points to a corrupt frame
		/// header, the decoder will continue to try and resync until it
		/// gets to a valid frame, then decode one frame, then return. If
		/// the decoder points to a frame whose frame CRC in the frame
		/// footer does not match the computed frame CRC, this function
		/// will issue a FLAC__STREAM_DECODER_ERROR_STATUS_FRAME_CRC_MISMATCH
		/// error to the error callback, and return, having decoded one
		/// complete, although corrupt, frame. (Such corrupted frames are
		/// sent as silence of the correct length to the write callback.)
		/// </summary>
		/// <returns>False if any fatal read, write, or memory allocation error occurred (meaning decoding must stop), else true; for more information about the decoder, check the decoder state with Flac__Stream_Decoder_Get_State()</returns>
		/********************************************************************/
		public Flac__bool Flac__Stream_Decoder_Process_Single()
		{
			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Protected != null);

			while (true)
			{
				switch (decoder.Protected.State)
				{
					case Flac__StreamDecoderState.Search_For_Metadata:
					{
						if (!Find_Metadata())
							return false;	// Above function sets the status for us

						break;
					}

					case Flac__StreamDecoderState.Read_Metadata:
					{
						if (!Read_Metadata())
							return false;	// Above function sets the status for us

						break;
					}

					case Flac__StreamDecoderState.Search_For_Frame_Sync:
					{
						if (!Frame_Sync())
							return true;	// Above function sets the status for us

						break;
					}

					case Flac__StreamDecoderState.Read_Frame:
					{
						if (!Read_Frame(out Flac__bool got_A_Frame, true))
							return false;	// Above function sets the status for us

						if (got_A_Frame)
							return true;	// Above function sets the status for us

						break;
					}

					case Flac__StreamDecoderState.End_Of_Stream:
					case Flac__StreamDecoderState.Aborted:
						return true;

					default:
					{
						Debug.Assert(false);
						return false;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Decode until the end of the metadata.
		/// This version instructs the decoder to decode from the current
		/// position and continue until all the metadata has been read, or
		/// until the callbacks return a fatal error or the read callback
		/// returns FLAC__STREAM_DECODER_READ_STATUS_END_OF_STREAM.
		///
		/// As the decoder needs more input it will call the read callback.
		/// As each metadata block is decoded, the metadata callback will be
		/// called with the decoded metadata
		/// </summary>
		/// <returns>False if any fatal read, write, or memory allocation error occurred (meaning decoding must stop), else true; for more information about the decoder, check the decoder state with Flac__Stream_Decoder_Get_State()</returns>
		/********************************************************************/
		public Flac__bool Flac__Stream_Decoder_Process_Until_End_Of_Metadata()
		{
			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Protected != null);

			while (true)
			{
				switch (decoder.Protected.State)
				{
					case Flac__StreamDecoderState.Search_For_Metadata:
					{
						if (!Find_Metadata())
							return false;	// Above function sets the status for us

						break;
					}

					case Flac__StreamDecoderState.Read_Metadata:
					{
						if (!Read_Metadata())
							return false;	// Above function sets the status for us

						break;
					}

					case Flac__StreamDecoderState.Search_For_Frame_Sync:
					case Flac__StreamDecoderState.Read_Frame:
					case Flac__StreamDecoderState.End_Of_Stream:
					case Flac__StreamDecoderState.Aborted:
						return true;

					default:
					{
						Debug.Assert(false);
						return false;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Decode until the end of the stream.
		/// This version instructs the decoder to decode from the current
		/// position and continue until the end of stream (the read callback
		/// returns FLAC__STREAM_DECODER_READ_STATUS_END_OF_STREAM), or until
		/// the callbacks return a fatal error.
		///
		/// As the decoder needs more input it will call the read callback.
		/// As each metadata block and frame is decoded, the metadata or
		/// write callback will be called with the decoded metadata or frame.
		/// </summary>
		/// <returns>False if any fatal read, write, or memory allocation error occurred (meaning decoding must stop), else true; for more information about the decoder, check the decoder state with Flac__Stream_Decoder_Get_State()</returns>
		/********************************************************************/
		public Flac__bool Flac__Stream_Decoder_Process_Until_End_Of_Stream()
		{
			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Protected != null);

			while (true)
			{
				switch (decoder.Protected.State)
				{
					case Flac__StreamDecoderState.Search_For_Metadata:
					{
						if (!Find_Metadata())
							return false;	// Above function sets the status for us

						break;
					}

					case Flac__StreamDecoderState.Read_Metadata:
					{
						if (!Read_Metadata())
							return false;	// Above function sets the status for us

						break;
					}

					case Flac__StreamDecoderState.Search_For_Frame_Sync:
					{
						if (!Frame_Sync())
							return true;	// Above function sets the status for us

						break;
					}
					case Flac__StreamDecoderState.Read_Frame:
					{
						if (!Read_Frame(out _, true))
							return false;	// Above function sets the status for us

						break;
					}

					case Flac__StreamDecoderState.End_Of_Stream:
					case Flac__StreamDecoderState.Aborted:
						return true;

					default:
					{
						Debug.Assert(false);
						return false;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Skip one audio frame.
		/// This version instructs the decoder to 'skip' a single frame and
		/// stop, unless the callbacks return a fatal error or the read
		/// callback returns FLAC__STREAM_DECODER_READ_STATUS_END_OF_STREAM.
		///
		/// The decoding flow is the same as what occurs when
		/// Flac__Stream_Decoder_Process_Single() is called to process an
		/// audio frame, except that this function does not decode the parsed
		/// data into PCM or call the write callback. The integrity of the
		/// frame is still checked the same way as in the other process
		/// functions.
		///
		/// This function will return once one whole frame is skipped, in
		/// the same way that Flac__Stream_Decoder_Process_Single() will
		/// return once one whole frame is decoded.
		///
		/// This function can be used in more quickly determining FLAC frame
		/// boundaries when decoding of the actual data is not needed, for
		/// example when an application is separating a FLAC stream into
		/// frames for editing or storing in a container. To do this, the
		/// application can use Flac__Stream_Decoder_Skip_Single_Frame() to
		/// quickly advance to the next frame, then use
		/// Flac__Stream_Decoder_Get_Decode_Position() to find the new frame
		/// boundary.
		///
		/// This function should only be called when the stream has advanced
		/// past all the metadata, otherwise it will return false.
		/// </summary>
		/// <returns>False if any fatal read, write, or memory allocation error occurred (meaning decoding must stop), or if the decoder is in the FLAC__STREAM_DECODER_SEARCH_FOR_METADATA or FLAC__STREAM_DECODER_READ_METADATA state, else true; for more information about the decoder, check the decoder state with Flac__Stream_Decoder_Get_State()</returns>
		/********************************************************************/
		public Flac__bool Flac__Stream_Decoder_Skip_Single_Frame()
		{
			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Protected != null);

			while (true)
			{
				switch (decoder.Protected.State)
				{
					case Flac__StreamDecoderState.Search_For_Metadata:
					case Flac__StreamDecoderState.Read_Metadata:
						return false;

					case Flac__StreamDecoderState.Search_For_Frame_Sync:
					{
						if (!Frame_Sync())
							return true;	// Above function sets the status for us

						break;
					}

					case Flac__StreamDecoderState.Read_Frame:
					{
						if (!Read_Frame(out Flac__bool got_A_Frame, false))
							return false;	// Above function sets the status for us

						if (got_A_Frame)
							return true;	// Above function sets the status for us

						break;
					}

					case Flac__StreamDecoderState.End_Of_Stream:
					case Flac__StreamDecoderState.Aborted:
						return true;

					default:
					{
						Debug.Assert(false);
						return false;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Flush the input and seek to an absolute sample.
		/// Decoding will resume at the given sample. Note that because of
		/// this, the next write callback may contain a partial block. The
		/// client must support seeking the input or this function will fail
		/// and return false. Furthermore, if the decoder state is
		/// FLAC__STREAM_DECODER_SEEK_ERROR, then the decoder must be flushed
		/// with Flac__Stream_Decoder_Flush() or reset with
		/// Flac__Stream_Decoder_Reset() before decoding can continue.
		/// </summary>
		/// <param name="sample">The target sample number to seek to</param>
		/// <returns>True for successful, else false</returns>
		/********************************************************************/
		public Flac__bool Flac__Stream_Decoder_Seek_Absolute(Flac__uint64 sample)
		{
			Debug.Assert(decoder != null);

			if ((decoder.Protected.State != Flac__StreamDecoderState.Search_For_Metadata) &&
			    (decoder.Protected.State != Flac__StreamDecoderState.Read_Metadata) &&
			    (decoder.Protected.State != Flac__StreamDecoderState.Search_For_Frame_Sync) &&
			    (decoder.Protected.State != Flac__StreamDecoderState.Read_Frame) &&
			    (decoder.Protected.State != Flac__StreamDecoderState.End_Of_Stream))
			{
				return false;
			}

			if (decoder.Private.Seek_Callback == null)
				return false;

			Debug.Assert(decoder.Private.Seek_Callback != null);
			Debug.Assert(decoder.Private.Tell_Callback != null);
			Debug.Assert(decoder.Private.Length_Callback != null);
			Debug.Assert(decoder.Private.Eof_Callback != null);

			if ((Flac__Stream_Decoder_Get_Total_Samples() > 0) && (sample >= Flac__Stream_Decoder_Get_Total_Samples()))
				return false;

			decoder.Private.Is_Seeking = true;

			// Turn off MD5 checking if a seek is attempted
			decoder.Private.Do_Md5_Checking = false;

			// Get the file length (currently our algorithm needs to know the length so it's also an error to get FLAC__STREAM_DECODER_LENGTH_STATUS_UNSUPPORTED)
			if (decoder.Private.Length_Callback(this, out Flac__uint64 length, decoder.Private.Client_Data) != Flac__StreamDecoderLengthStatus.Ok)
			{
				decoder.Private.Is_Seeking = false;
				return false;
			}

			// If we haven't finished processing the metadata yet, do that so we have the STREAMINFO, SEEK_TABLE, and first_Frame_Offset
			if ((decoder.Protected.State == Flac__StreamDecoderState.Search_For_Metadata) || (decoder.Protected.State == Flac__StreamDecoderState.Read_Metadata))
			{
				if (!Flac__Stream_Decoder_Process_Until_End_Of_Metadata())
				{
					// Above call sets the state for us
					decoder.Private.Is_Seeking = false;
					return false;
				}

				// Check this again in case we didn't know total samples the first time
				if ((Flac__Stream_Decoder_Get_Total_Samples() > 0) && (sample >= Flac__Stream_Decoder_Get_Total_Samples()))
				{
					decoder.Private.Is_Seeking = false;
					return false;
				}
			}

			{
				Flac__bool ok = Seek_To_Absolute_Sample(length, sample);

				decoder.Private.Is_Seeking = false;

				return ok;
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__StreamDecoderInitStatus Init_Stream_Internal(Flac__StreamDecoderReadCallback read_Callback, Flac__StreamDecoderSeekCallback seek_Callback, Flac__StreamDecoderTellCallback tell_Callback, Flac__StreamDecoderLengthCallback length_Callback, Flac__StreamDecoderEofCallback eof_Callback, Flac__StreamDecoderWriteCallback write_Callback, Flac__StreamDecoderMetadataCallback metadata_Callback, Flac__StreamDecoderErrorCallback error_Callback, object client_Data)
		{
			Debug.Assert(decoder != null);

			if (decoder.Protected.State != Flac__StreamDecoderState.Uninitialized)
				return Flac__StreamDecoderInitStatus.Already_Initialized;

			if ((read_Callback == null) || (write_Callback == null) || (error_Callback == null) || ((seek_Callback != null) && ((tell_Callback == null) || (length_Callback == null) || (eof_Callback == null))))
				return Flac__StreamDecoderInitStatus.Invalid_Callbacks;

			// First default to the non-asm routines
			decoder.Private.Lpc = new Lpc();

			// Since this is a C# port, we do not have any assembler versions of the LPC decoder, so this part has been removed

			// From here on, errors are fatal
			if (!decoder.Private.Input.Flac__BitReader_Init(Read_Callback, this))
			{
				decoder.Protected.State = Flac__StreamDecoderState.Memory_Allocation_Error;
				return Flac__StreamDecoderInitStatus.Memory_Allocation_Error;
			}

			decoder.Private.Read_Callback = read_Callback;
			decoder.Private.Seek_Callback = seek_Callback;
			decoder.Private.Tell_Callback = tell_Callback;
			decoder.Private.Length_Callback = length_Callback;
			decoder.Private.Eof_Callback = eof_Callback;
			decoder.Private.Write_Callback = write_Callback;
			decoder.Private.Metadata_Callback = metadata_Callback;
			decoder.Private.Error_Callback = error_Callback;
			decoder.Private.Client_Data = client_Data;
			decoder.Private.Fixed_Block_Size = decoder.Private.Next_Fixed_Block_Size = 0;
			decoder.Private.Samples_Decoded = 0;
			decoder.Private.Has_Stream_Info = false;
			decoder.Private.Cached = false;

			decoder.Private.Do_Md5_Checking = decoder.Protected.Md5_Checking;
			decoder.Private.Is_Seeking = false;

			decoder.Private.Internal_Reset_Hack = true;		// So the following reset does not try to rewind the input
			if (!Flac__Stream_Decoder_Reset())
			{
				// Above call sets the state for us
				return Flac__StreamDecoderInitStatus.Memory_Allocation_Error;
			}

			return Flac__StreamDecoderInitStatus.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__StreamDecoderInitStatus Init_File_Internal(Stream file, bool leave_Open, Flac__StreamDecoderWriteCallback write_Callback, Flac__StreamDecoderMetadataCallback metadata_Callback, Flac__StreamDecoderErrorCallback error_Callback, object client_Data)
		{
			Debug.Assert(decoder != null);
			Debug.Assert(file != null);

			if (decoder.Protected.State != Flac__StreamDecoderState.Uninitialized)
				return decoder.Protected.InitState = Flac__StreamDecoderInitStatus.Already_Initialized;

			if ((write_Callback == null) || (error_Callback == null))
				return decoder.Protected.InitState = Flac__StreamDecoderInitStatus.Invalid_Callbacks;

			// To make sure that our file does not go unclosed after an error, we
			// must assign the stream pointer before any further error can occur in
			// this routine
			decoder.Private.File = file;
			decoder.Private.Leave_Stream_Open = leave_Open;

			return Init_Stream_Internal(File_Read_Callback, file.CanSeek ? File_Seek_Callback : null, file.CanSeek ? File_Tell_Callback : null, file.CanSeek ? File_Length_Callback : null, File_Eof_Callback, write_Callback, metadata_Callback, error_Callback, client_Data);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__StreamDecoderInitStatus Init_File_Internal(string filename, Flac__StreamDecoderWriteCallback write_Callback, Flac__StreamDecoderMetadataCallback metadata_Callback, Flac__StreamDecoderErrorCallback error_Callback, object client_Data)
		{
			Debug.Assert(decoder != null);

			// To make sure that our file does not go unclosed after an error, we
			// have to do the same entrance checks here that are later performed
			// in Flac__Stream_Decoder_Init_File() before the File is assigned

			if (decoder.Protected.State != Flac__StreamDecoderState.Uninitialized)
				return decoder.Protected.InitState = Flac__StreamDecoderInitStatus.Already_Initialized;

			if ((write_Callback == null) || (error_Callback == null))
				return decoder.Protected.InitState = Flac__StreamDecoderInitStatus.Invalid_Callbacks;

			try
			{
				Stream file = File.OpenRead(filename);

				return Init_File_Internal(file, false, write_Callback, metadata_Callback, error_Callback, client_Data);
			}
			catch(Exception)
			{
				return Flac__StreamDecoderInitStatus.Error_Opening_File;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of input bytes consumed
		/// </summary>
		/********************************************************************/
		private uint32_t Flac__Stream_Decoder_Get_Input_Bytes_Unconsumed()
		{
			Debug.Assert(decoder != null);

			Debug.Assert(decoder.Private.Input.Flac__BitReader_Is_Consumed_Byte_Aligned());

			Debug.Assert((decoder.Private.Input.Flac__BitReader_Get_Input_Bits_Unconsumed() & 7) == 0);

			return decoder.Private.Input.Flac__BitReader_Get_Input_Bits_Unconsumed() / 8;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Set_Defaults(Flac__StreamDecoder decoder)
		{
			decoder.Private.Read_Callback = null;
			decoder.Private.Seek_Callback = null;
			decoder.Private.Tell_Callback = null;
			decoder.Private.Length_Callback = null;
			decoder.Private.Eof_Callback = null;
			decoder.Private.Write_Callback = null;
			decoder.Private.Metadata_Callback = null;
			decoder.Private.Error_Callback = null;
			decoder.Private.Client_Data = null;

			Array.Clear(decoder.Private.Metadata_Filter);
			decoder.Private.Metadata_Filter[(int)Flac__MetadataType.StreamInfo] = true;
			decoder.Private.Metadata_Filter_Ids_Count = 0;

			decoder.Protected.Md5_Checking = false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Allocate_Output(uint32_t size, uint32_t channels)
		{
			if ((size <= decoder.Private.Output_Capacity) && (channels <= decoder.Private.Output_Channels))
				return true;

			// Simply using realloc() is not practical because the number of channels may change mid-stream
			for (uint32_t i = 0; i < Constants.Flac__Max_Channels; i++)
			{
				if (decoder.Private.Output[i] != null)
					decoder.Private.Output[i] = null;

				if (decoder.Private.Residual_Unaligned[i] != null)
				{
					decoder.Private.Residual_Unaligned[i] = null;
					decoder.Private.Residual[i] = null;
				}
			}

			for (uint32_t i = 0; i < channels; i++)
			{
				Flac__int32[] tmp = Alloc.Safe_MAlloc_Mul_2Op<Flac__int32>(size, 1);
				if (tmp == null)
				{
					decoder.Protected.State = Flac__StreamDecoderState.Memory_Allocation_Error;
					return false;
				}

				decoder.Private.Output[i] = tmp;

				if (!Memory.Flac__Memory_Alloc_Aligned_Int32_Array(size, ref decoder.Private.Residual_Unaligned[i], ref decoder.Private.Residual[i]))
				{
					decoder.Protected.State = Flac__StreamDecoderState.Memory_Allocation_Error;
					return false;
				}
			}

			decoder.Private.Output_Capacity = size;
			decoder.Private.Output_Channels = channels;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Has_Id_Filtered(Flac__byte[] id)
		{
			Debug.Assert(decoder != null);
			Debug.Assert(decoder.Private != null);

			for (size_t i = 0; i < decoder.Private.Metadata_Filter_Ids_Count; i++)
			{
				if (decoder.Private.Metadata_Filter_Ids.AsSpan((int)(i * (Constants.Flac__Stream_Metadata_Application_Id_Len / 8)), 4).SequenceEqual(id))
					return true;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Find_Metadata()
		{
			Flac__bool first = true;

			Debug.Assert(decoder.Private.Input.Flac__BitReader_Is_Consumed_Byte_Aligned());

			for (uint32_t i = 0, id = 0; i < 4; )
			{
				Flac__uint32 x;

				if (decoder.Private.Cached)
				{
					x = decoder.Private.Lookahead;
					decoder.Private.Cached = false;
				}
				else
				{
					if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out x, 8))
						return false;	// Read_Callback sets the state for us
				}

				if (x == Format.Flac__Stream_Sync_String[i])
				{
					first = true;
					i++;
					id = 0;
					continue;
				}

				if (id >= 3)
					return false;

				if (x == id3V2_Tag[id])
				{
					id++;
					i = 0;

					if (id == 3)
					{
						if (!Skip_Id3V2_Tag())
							return false;	// Skip_Id3V2_Tag sets the state for us
					}
					continue;
				}

				id = 0;
				if (x == 0xff)	// MAGIC NUMBER for the first 8 frame sync bits
				{
					decoder.Private.Header_Warmup[0] = (Flac__byte)x;

					if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out x, 8))
						return false;	// Read_Callback sets the state for us

					// We have to check if we just read two 0xff in a row; the second may actually be the beginning of the sync code
					// else we have to check if the second byte is the end of a sync code
					if (x == 0xff)	// MAGIC NUMBER for the first 8 frame sync bits
					{
						decoder.Private.Lookahead = (Flac__byte)x;
						decoder.Private.Cached = true;
					}
					else if (x >> 1 == 0x7c)	// MAGIC NUMBER for the last 6 sync bits and reserved 7th bit
					{
						decoder.Private.Header_Warmup[1] = (Flac__byte)x;
						decoder.Protected.State = Flac__StreamDecoderState.Read_Frame;
						return true;
					}
				}

				i = 0;
				if (first)
				{
					Send_Error_To_Client(Flac__StreamDecoderErrorStatus.Lost_Sync);
					first = false;
				}
			}

			decoder.Protected.State = Flac__StreamDecoderState.Read_Metadata;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Read_Metadata()
		{
			Debug.Assert(decoder.Private.Input.Flac__BitReader_Is_Consumed_Byte_Aligned());

			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out Flac__uint32 x, Constants.Flac__Stream_Metadata_Is_Last_Len))
				return false;	// Read_Callback sets the state for us

			Flac__bool is_Last = x != 0;

			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out x, Constants.Flac__Stream_Metadata_Type_Len))
				return false;	// Read_Callback sets the state for us

			Flac__MetadataType type = (Flac__MetadataType)x;

			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out Flac__uint32 length, Constants.Flac__Stream_Metadata_Length_Len))
				return false;	// Read_Callback sets the state for us

			if (type == Flac__MetadataType.StreamInfo)
			{
				if (!Read_Metadata_StreamInfo(is_Last, length))
					return false;

				decoder.Private.Has_Stream_Info = true;

				if (((Flac__StreamMetadata_StreamInfo)decoder.Private.Stream_Info.Data).Md5Sum.All(i => i == 0))
					decoder.Private.Do_Md5_Checking = false;

				if (!decoder.Private.Is_Seeking && decoder.Private.Metadata_Filter[(int)Flac__MetadataType.StreamInfo] && (decoder.Private.Metadata_Callback != null))
					decoder.Private.Metadata_Callback(this, decoder.Private.Stream_Info, decoder.Private.Client_Data);
			}
			else if (type == Flac__MetadataType.SeekTable)
			{
				// Just in case we already have a seek table, and reading the next one fails:
				decoder.Private.Has_Seek_Table = false;

				if (!Read_Metadata_SeekTable(is_Last, length))
					return false;

				decoder.Private.Has_Seek_Table = true;

				if (!decoder.Private.Is_Seeking && decoder.Private.Metadata_Filter[(int)Flac__MetadataType.SeekTable] && (decoder.Private.Metadata_Callback != null))
					decoder.Private.Metadata_Callback(this, decoder.Private.Seek_Table, decoder.Private.Client_Data);
			}
			else
			{
				Flac__bool skip_It = !decoder.Private.Metadata_Filter[(int)type];
				uint32_t real_Length = length;

				Flac__StreamMetadata block = new Flac__StreamMetadata();
				block.Is_Last = is_Last;
				block.Type = type;
				block.Length = length;

				if (type == Flac__MetadataType.Application)
				{
					Flac__StreamMetadata_Application metaApplication = new Flac__StreamMetadata_Application();
					block.Data = metaApplication;

					if (!decoder.Private.Input.Flac__BitReader_Read_Byte_Block_Aligned_No_Crc(metaApplication.Id, Constants.Flac__Stream_Metadata_Application_Id_Len / 8))
						return false;	// Read_Callback sets the state for us

					if (real_Length < Constants.Flac__Stream_Metadata_Application_Id_Len / 8)	// Underflow check
					{
						decoder.Protected.State = Flac__StreamDecoderState.Memory_Allocation_Error;	// @@@@@@ maybe wrong error? Need to resync?
						return false;
					}

					real_Length -= Constants.Flac__Stream_Metadata_Application_Id_Len / 8;

					if ((decoder.Private.Metadata_Filter_Ids_Count > 0) && Has_Id_Filtered(metaApplication.Id))
						skip_It = !skip_It;
				}

				if (skip_It)
				{
					if (!decoder.Private.Input.Flac__BitReader_Skip_Byte_Block_Aligned_No_Crc(real_Length))
						return false;	// Read_Callback sets the state for us
				}
				else
				{
					Flac__bool ok = true;

					switch (type)
					{
						case Flac__MetadataType.Padding:
						{
							block.Data = new Flac__StreamMetadata_Padding();

							// Skip padding bytes
							if (!decoder.Private.Input.Flac__BitReader_Skip_Byte_Block_Aligned_No_Crc(real_Length))
								ok = false;	// Read_Callback sets the state for us

							break;
						}

						case Flac__MetadataType.Application:
						{
							// Remember, we read the ID already
							Flac__StreamMetadata_Application metaApplication = (Flac__StreamMetadata_Application)block.Data;

							if (real_Length > 0)
							{
								metaApplication.Data = new Flac__byte[real_Length];

								if (!decoder.Private.Input.Flac__BitReader_Read_Byte_Block_Aligned_No_Crc(metaApplication.Data, real_Length))
									ok = false;	// Read_Callback sets the state for us
							}
							else
								metaApplication.Data = null;

							break;
						}

						case Flac__MetadataType.Vorbis_Comment:
						{
							Flac__StreamMetadata_VorbisComment metaVorbisComment = new Flac__StreamMetadata_VorbisComment();
							block.Data = metaVorbisComment;

							if (!Read_Metadata_VorbisComment(metaVorbisComment, real_Length))
								ok = false;

							break;
						}

						case Flac__MetadataType.CueSheet:
						{
							Flac__StreamMetadata_CueSheet metaCueSheet = new Flac__StreamMetadata_CueSheet();
							block.Data = metaCueSheet;

							if (!Read_Metadata_CueSheet(metaCueSheet))
								ok = false;

							break;
						}

						case Flac__MetadataType.Picture:
						{
							Flac__StreamMetadata_Picture metaPicture = new Flac__StreamMetadata_Picture();
							block.Data = metaPicture;

							if (!Read_Metadata_Picture(metaPicture))
								ok = false;

							break;
						}

						case Flac__MetadataType.StreamInfo:
						case Flac__MetadataType.SeekTable:
						{
							Debug.Assert(false);
							break;
						}

						default:
						{
							Flac__StreamMetadata_Unknown metaUnknown = new Flac__StreamMetadata_Unknown();
							block.Data = metaUnknown;

							if (real_Length > 0)
							{
								metaUnknown.Data = new Flac__byte[real_Length];

								if (!decoder.Private.Input.Flac__BitReader_Read_Byte_Block_Aligned_No_Crc(metaUnknown.Data, real_Length))
									ok = false;	// Read_Callback sets the state for us
							}
							else
								metaUnknown.Data = null;

							break;
						}
					}

					if (ok && !decoder.Private.Is_Seeking && (decoder.Private.Metadata_Callback != null))
						decoder.Private.Metadata_Callback(this, block, decoder.Private.Client_Data);

					// Now we have to free any allocated data in the block
					switch (type)
					{
						case Flac__MetadataType.Padding:
							break;

						case Flac__MetadataType.Application:
						{
							Flac__StreamMetadata_Application metaApplication = (Flac__StreamMetadata_Application)block.Data;

							if (metaApplication.Data != null)
								metaApplication.Data = null;

							break;
						}

						case Flac__MetadataType.Vorbis_Comment:
						{
							Flac__StreamMetadata_VorbisComment metaVorbisComment = (Flac__StreamMetadata_VorbisComment)block.Data;

							if (metaVorbisComment.Vendor_String.Entry != null)
								metaVorbisComment.Vendor_String.Entry = null;

							if (metaVorbisComment.Num_Comments > 0)
							{
								for (Flac__uint32 i = 0; i < metaVorbisComment.Num_Comments; i++)
								{
									if (metaVorbisComment.Comments[i].Entry != null)
										metaVorbisComment.Comments[i].Entry = null;
								}
							}

							if (metaVorbisComment.Comments != null)
								metaVorbisComment.Comments = null;

							break;
						}

						case Flac__MetadataType.CueSheet:
						{
							Flac__StreamMetadata_CueSheet metaCueSheet = (Flac__StreamMetadata_CueSheet)block.Data;

							if (metaCueSheet.Num_Tracks > 0)
							{
								for (Flac__uint32 i = 0; i < metaCueSheet.Num_Tracks; i++)
								{
									if (metaCueSheet.Tracks[i].Indices != null)
										metaCueSheet.Tracks[i].Indices = null;
								}
							}

							if (metaCueSheet.Tracks != null)
								metaCueSheet.Tracks = null;

							break;
						}

						case Flac__MetadataType.Picture:
						{
							Flac__StreamMetadata_Picture metaPicture = (Flac__StreamMetadata_Picture)block.Data;

							if (metaPicture.Mime_Type != null)
								metaPicture.Mime_Type = null;

							if (metaPicture.Description != null)
								metaPicture.Description = null;

							if (metaPicture.Data != null)
								metaPicture.Data = null;

							break;
						}

						case Flac__MetadataType.StreamInfo:
						case Flac__MetadataType.SeekTable:
						{
							Debug.Assert(false);
							break;
						}

						default:
						{
							Flac__StreamMetadata_Unknown metaUnknown = (Flac__StreamMetadata_Unknown)block.Data;

							if (metaUnknown.Data != null)
								metaUnknown.Data = null;

							break;
						}
					}

					if (!ok)	// Anything that unsets "ok" should also make sure decoder.Protected.State is updated
						return false;
				}
			}

			if (is_Last)
			{
				// If this fails, it's OK, it's just a hint for the seek routine
				if (!Flac__Stream_Decoder_Get_Decode_Position(out decoder.Private.First_Frame_Offset))
					decoder.Private.First_Frame_Offset = 0;

				decoder.Protected.State = Flac__StreamDecoderState.Search_For_Frame_Sync;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Read_Metadata_StreamInfo(Flac__bool is_Last, uint32_t length)
		{
			uint32_t used_Bits = 0;

			Debug.Assert(decoder.Private.Input.Flac__BitReader_Is_Consumed_Byte_Aligned());

			decoder.Private.Stream_Info.Type = Flac__MetadataType.StreamInfo;
			decoder.Private.Stream_Info.Is_Last = is_Last;
			decoder.Private.Stream_Info.Length = length;

			uint32_t bits = Constants.Flac__Stream_Metadata_StreamInfo_Min_Block_Size_Len;
			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out Flac__uint32 x, bits))
				return false;	// Read_Callback sets the state for us

			Flac__StreamMetadata_StreamInfo metaStreamInfo = new Flac__StreamMetadata_StreamInfo();
			decoder.Private.Stream_Info.Data = metaStreamInfo;

			metaStreamInfo.Min_BlockSize = x;
			used_Bits += bits;

			bits = Constants.Flac__Stream_Metadata_StreamInfo_Max_Block_Size_Len;
			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out x, bits))
				return false;	// Read_Callback sets the state for us

			metaStreamInfo.Max_BlockSize = x;
			used_Bits += bits;

			bits = Constants.Flac__Stream_Metadata_StreamInfo_Min_Frame_Size_Len;
			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out x, bits))
				return false;	// Read_Callback sets the state for us

			metaStreamInfo.Min_FrameSize = x;
			used_Bits += bits;

			bits = Constants.Flac__Stream_Metadata_StreamInfo_Max_Frame_Size_Len;
			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out x, bits))
				return false;	// Read_Callback sets the state for us

			metaStreamInfo.Max_FrameSize = x;
			used_Bits += bits;

			bits = Constants.Flac__Stream_Metadata_StreamInfo_Sample_Rate_Len;
			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out x, bits))
				return false;	// Read_Callback sets the state for us

			metaStreamInfo.Sample_Rate = x;
			used_Bits += bits;

			bits = Constants.Flac__Stream_Metadata_StreamInfo_Channels_Len;
			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out x, bits))
				return false;	// Read_Callback sets the state for us

			metaStreamInfo.Channels = x + 1;
			used_Bits += bits;

			bits = Constants.Flac__Stream_Metadata_StreamInfo_Bits_Per_Sample_Len;
			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out x, bits))
				return false;	// Read_Callback sets the state for us

			metaStreamInfo.Bits_Per_Sample = x + 1;
			used_Bits += bits;

			bits = Constants.Flac__Stream_Metadata_StreamInfo_Total_Samples_Len;
			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt64(out metaStreamInfo.Total_Samples, bits))
				return false;	// Read_Callback sets the state for us

			used_Bits += bits;

			if (!decoder.Private.Input.Flac__BitReader_Read_Byte_Block_Aligned_No_Crc(metaStreamInfo.Md5Sum, 16))
				return false;

			used_Bits += 16 * 8;

			// Skip the rest of the block
			Debug.Assert(used_Bits % 8 == 0);

			if (length < (used_Bits / 8))
				return false;	// Read_Callback sets the state for us

			length -= (used_Bits / 8);

			if (!decoder.Private.Input.Flac__BitReader_Skip_Byte_Block_Aligned_No_Crc(length))
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Read_Metadata_SeekTable(Flac__bool is_Last, uint32_t length)
		{
			Debug.Assert(decoder.Private.Input.Flac__BitReader_Is_Consumed_Byte_Aligned());

			decoder.Private.Seek_Table.Type = Flac__MetadataType.SeekTable;
			decoder.Private.Seek_Table.Is_Last = is_Last;
			decoder.Private.Seek_Table.Length = length;

			Flac__StreamMetadata_SeekTable metaSeekTable = new Flac__StreamMetadata_SeekTable();
			decoder.Private.Seek_Table.Data = metaSeekTable;

			metaSeekTable.Num_Points = length / Constants.Flac__Stream_Metadata_SeekPoint_Length;

			// Use realloc since we may pass through here several times (e.g. after seeking)
			metaSeekTable.Points = Alloc.Safe_Realloc_Mul_2Op(metaSeekTable.Points, metaSeekTable.Num_Points, 1);
			if (metaSeekTable.Points == null)
			{
				decoder.Protected.State = Flac__StreamDecoderState.Memory_Allocation_Error;
				return false;
			}

			for (Flac__uint32 i = 0; i < metaSeekTable.Num_Points; i++)
			{
				if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt64(out Flac__uint64 xx, Constants.Flac__Stream_Metadata_SeekPoint_Sample_Number_Len))
					return false;	// Read_Callback sets the state for us

				metaSeekTable.Points[i].Sample_Number = xx;

				if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt64(out xx, Constants.Flac__Stream_Metadata_SeekPoint_Stream_Offset_Len))
					return false;	// Read_Callback sets the state for us

				metaSeekTable.Points[i].Stream_Offset = xx;

				if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out Flac__uint32 x, Constants.Flac__Stream_Metadata_SeekPoint_Frame_Samples_Len))
					return false;	// Read_Callback sets the state for us

				metaSeekTable.Points[i].Frame_Samples = x;
			}

			length -= (metaSeekTable.Num_Points * Constants.Flac__Stream_Metadata_SeekPoint_Length);

			// If there is a partial point left, skip over it
			if (length > 0)
			{
				// @@@ Do a Send_Error_To_Client() here? There's an argument for either way
				if (!decoder.Private.Input.Flac__BitReader_Skip_Byte_Block_Aligned_No_Crc(length))
					return false;	// Read_Callback sets the state for us
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Read_Metadata_VorbisComment(Flac__StreamMetadata_VorbisComment obj, uint32_t length)
		{
			Debug.Assert(decoder.Private.Input.Flac__BitReader_Is_Consumed_Byte_Aligned());

			// Read vendor string
			if (length >= 8)
			{
				length -= 8;	// Vendor string length + num comments entries alone take 8 bytes
				Debug.Assert(Constants.Flac__Stream_Metadata_Vorbis_Comment_Entry_Length_Len == 32);

				if (!decoder.Private.Input.Flac__BitReader_Read_UInt32_Little_Endian(out obj.Vendor_String.Length))
					return false;	// Read_Callback sets the state for us

				if (obj.Vendor_String.Length > 0)
				{
					if (length < obj.Vendor_String.Length)
					{
						obj.Vendor_String.Length = 0;
						obj.Vendor_String.Entry = null;
						goto Skip;
					}
					else
						length -= obj.Vendor_String.Length;

					if ((obj.Vendor_String.Entry = Alloc.Safe_MAlloc_Add_2Op<Flac__byte>(obj.Vendor_String.Length, 1)) == null)
					{
						decoder.Protected.State = Flac__StreamDecoderState.Memory_Allocation_Error;
						return false;
					}

					if (!decoder.Private.Input.Flac__BitReader_Read_Byte_Block_Aligned_No_Crc(obj.Vendor_String.Entry, obj.Vendor_String.Length))
						return false;	// Read_Callback sets the state for us

					obj.Vendor_String.Entry[obj.Vendor_String.Length] = 0x00;
				}
				else
					obj.Vendor_String.Entry = null;

				// Read num comments
				Debug.Assert(Constants.Flac__Stream_Metadata_Vorbis_Comment_Num_Comments_Len == 32);

				if (!decoder.Private.Input.Flac__BitReader_Read_UInt32_Little_Endian(out obj.Num_Comments))
					return false;	// Read_Callback sets the state for us

				// Read comments
				if (obj.Num_Comments > 100000)
				{
					// Possible malicious file
					obj.Num_Comments = 0;
					return false;
				}

				if (obj.Num_Comments > 0)
				{
					if ((obj.Comments = Alloc.Safe_MAlloc_Mul_2Op_P<Flac__StreamMetadata_VorbisComment_Entry>(obj.Num_Comments, 1)) == null)
					{
						obj.Num_Comments = 0;
						decoder.Protected.State = Flac__StreamDecoderState.Memory_Allocation_Error;
						return false;
					}

					for (Flac__uint32 i = 0; i < obj.Num_Comments; i++)
					{
						// Initialize here just to make sure
						obj.Comments[i].Length = 0;
						obj.Comments[i].Entry = null;

						Debug.Assert(Constants.Flac__Stream_Metadata_Vorbis_Comment_Entry_Length_Len == 32);

						if (length < 4)
						{
							obj.Num_Comments = i;
							goto Skip;
						}
						else
							length -= 4;

						if (!decoder.Private.Input.Flac__BitReader_Read_UInt32_Little_Endian(out obj.Comments[i].Length))
						{
							obj.Num_Comments = i;
							return false;	// Read_Callback sets the state for us
						}

						if (obj.Comments[i].Length > 0)
						{
							if (length < obj.Comments[i].Length)
							{
								obj.Num_Comments = i;
								goto Skip;
							}
							else
								length -= obj.Comments[i].Length;

							if ((obj.Comments[i].Entry = Alloc.Safe_MAlloc_Add_2Op<Flac__byte>(obj.Comments[i].Length, 1)) == null)
							{
								decoder.Protected.State = Flac__StreamDecoderState.Memory_Allocation_Error;
								obj.Num_Comments = i;
								return false;
							}

							Array.Clear(obj.Comments[i].Entry);

							if (!decoder.Private.Input.Flac__BitReader_Read_Byte_Block_Aligned_No_Crc(obj.Comments[i].Entry, obj.Comments[i].Length))
							{
								// Current i-th entry is bad, so we delete it
								obj.Comments[i].Entry = null;
								obj.Num_Comments = i;
								goto Skip;
							}

							obj.Comments[i].Entry[obj.Comments[i].Length] = 0x00;
						}
						else
							obj.Comments[i].Entry = null;
					}
				}
			}
Skip:
			if (length > 0)
			{
				// length > 0 can only happen on files with invalid data in comments
				if (obj.Num_Comments < 1)
					obj.Comments = null;

				if (!decoder.Private.Input.Flac__BitReader_Skip_Byte_Block_Aligned_No_Crc(length))
					return false;	// Read_Callback sets the state for us
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Read_Metadata_CueSheet(Flac__StreamMetadata_CueSheet obj)
		{
			Debug.Assert(decoder.Private.Input.Flac__BitReader_Is_Consumed_Byte_Aligned());

			Debug.Assert(Constants.Flac__Stream_Metadata_CueSheet_Media_Catalog_Number_Len % 8 == 0);

			if (!decoder.Private.Input.Flac__BitReader_Read_Byte_Block_Aligned_No_Crc(obj.Media_Catalog_Number, Constants.Flac__Stream_Metadata_CueSheet_Media_Catalog_Number_Len / 8))
				return false;	// Read_Callback sets the state for us

			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt64(out obj.Lead_In, Constants.Flac__Stream_Metadata_CueSheet_Lead_In_Len))
				return false;	// Read_Callback sets the state for us

			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out Flac__uint32 x, Constants.Flac__Stream_Metadata_CueSheet_Is_Cd_Len))
				return false;	// Read_Callback sets the state for us

			obj.Is_Cd = x != 0;

			if (!decoder.Private.Input.Flac__BitReader_Skip_Bits_No_Crc(Constants.Flac__Stream_Metadata_CueSheet_Reserved_Len))
				return false;	// Read_Callback sets the state for us

			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out x, Constants.Flac__Stream_Metadata_CueSheet_Num_Tracks_Len))
				return false;	// Read_Callback sets the state for us

			obj.Num_Tracks = x;

			if (obj.Num_Tracks > 0)
			{
				if ((obj.Tracks = Alloc.Safe_CAlloc<Flac__StreamMetadata_CueSheet_Track>(obj.Num_Tracks)) == null)
				{
					decoder.Protected.State = Flac__StreamDecoderState.Memory_Allocation_Error;
					return false;
				}

				for (Flac__uint32 i = 0; i < obj.Num_Tracks; i++)
				{
					Flac__StreamMetadata_CueSheet_Track track = obj.Tracks[i];

					if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt64(out track.Offset, Constants.Flac__Stream_Metadata_CueSheet_Track_Offset_Len))
						return false;	// Read_Callback sets the state for us

					if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out x, Constants.Flac__Stream_Metadata_CueSheet_Track_Number_Len))
						return false;	// Read_Callback sets the state for us

					track.Number = (Flac__byte)x;

					Debug.Assert(Constants.Flac__Stream_Metadata_CueSheet_Track_Isrc_Len % 8 == 0);

					if (!decoder.Private.Input.Flac__BitReader_Read_Byte_Block_Aligned_No_Crc(track.Isrc, Constants.Flac__Stream_Metadata_CueSheet_Track_Isrc_Len / 8))
						return false;	// Read_Callback sets the state for us

					if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out x, Constants.Flac__Stream_Metadata_CueSheet_Track_Type_Len))
						return false;	// Read_Callback sets the state for us

					track.Type = x;

					if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out x, Constants.Flac__Stream_Metadata_CueSheet_Track_Pre_Emphasis_Len))
						return false;	// Read_Callback sets the state for us

					track.Pre_Emphasis = x;

					if (!decoder.Private.Input.Flac__BitReader_Skip_Bits_No_Crc(Constants.Flac__Stream_Metadata_CueSheet_Track_Reserved_Len))
						return false;	// Read_Callback sets the state for us

					if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out x, Constants.Flac__Stream_Metadata_CueSheet_Track_Num_Indices_Len))
						return false;	// Read_Callback sets the state for us

					track.Num_Indices = (Flac__byte)x;

					if (track.Num_Indices > 0)
					{
						if ((track.Indices = Alloc.Safe_CAlloc<Flac__StreamMetadata_CueSheet_Index>(track.Num_Indices)) == null)
						{
							decoder.Protected.State = Flac__StreamDecoderState.Memory_Allocation_Error;
							return false;
						}

						for (Flac__uint32 j = 0; j < track.Num_Indices; j++)
						{
							Flac__StreamMetadata_CueSheet_Index indx = track.Indices[j];

							if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt64(out indx.Offset, Constants.Flac__Stream_Metadata_CueSheet_Index_Offset_Len))
								return false;	// Read_Callback sets the state for us

							if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out x, Constants.Flac__Stream_Metadata_CueSheet_Index_Number_Len))
								return false;	// Read_Callback sets the state for us

							indx.Number = (Flac__byte)x;

							if (!decoder.Private.Input.Flac__BitReader_Skip_Bits_No_Crc(Constants.Flac__Stream_Metadata_CueSheet_Index_Reserved_Len))
								return false;	// Read_Callback sets the state for us
						}
					}
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Read_Metadata_Picture(Flac__StreamMetadata_Picture obj)
		{
			Debug.Assert(decoder.Private.Input.Flac__BitReader_Is_Consumed_Byte_Aligned());

			// Read type
			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out Flac__uint32 x, Constants.Flac__Stream_Metadata_Picture_Type_Len))
				return false;	// Read_Callback sets the state for us

			obj.Type = (Flac__StreamMetadata_Picture_Type)x;

			// Read MIME type
			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out x, Constants.Flac__Stream_Metadata_Picture_Mime_Type_Length_Len))
				return false;	// Read_Callback sets the state for us

			if ((obj.Mime_Type = Alloc.Safe_MAlloc_Add_2Op<byte>(x, 1)) == null)
			{
				decoder.Protected.State = Flac__StreamDecoderState.Memory_Allocation_Error;
				return false;
			}

			if (x > 0)
			{
				if (!decoder.Private.Input.Flac__BitReader_Read_Byte_Block_Aligned_No_Crc(obj.Mime_Type, x))
					return false;	// Read_Callback sets the state for us
			}

			obj.Mime_Type[x] = 0x00;

			// Read description
			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out x, Constants.Flac__Stream_Metadata_Picture_Description_Length_Len))
				return false;	// Read_Callback sets the state for us

			if ((obj.Description = Alloc.Safe_MAlloc_Add_2Op<byte>(x, 1)) == null)
			{
				decoder.Protected.State = Flac__StreamDecoderState.Memory_Allocation_Error;
				return false;
			}

			if (x > 0)
			{
				if (!decoder.Private.Input.Flac__BitReader_Read_Byte_Block_Aligned_No_Crc(obj.Description, x))
					return false;	// Read_Callback sets the state for us
			}

			obj.Description[x] = 0x00;

			// Read width
			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out obj.Width, Constants.Flac__Stream_Metadata_Picture_Width_Len))
				return false;	// Read_Callback sets the state for us

			// Read height
			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out obj.Height, Constants.Flac__Stream_Metadata_Picture_Height_Len))
				return false;	// Read_Callback sets the state for us

			// Read depth
			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out obj.Depth, Constants.Flac__Stream_Metadata_Picture_Depth_Len))
				return false;	// Read_Callback sets the state for us

			// Read colors
			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out obj.Colors, Constants.Flac__Stream_Metadata_Picture_Colors_Len))
				return false;	// Read_Callback sets the state for us

			// Read data
			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out obj.Data_Length, Constants.Flac__Stream_Metadata_Picture_Data_Length_Len))
				return false;	// Read_Callback sets the state for us

			if ((obj.Data = Alloc.Safe_MAlloc<byte>(obj.Data_Length)) == null)
			{
				decoder.Protected.State = Flac__StreamDecoderState.Memory_Allocation_Error;
				return false;
			}

			if (obj.Data_Length > 0)
			{
				if (!decoder.Private.Input.Flac__BitReader_Read_Byte_Block_Aligned_No_Crc(obj.Data, obj.Data_Length))
					return false;	// Read_Callback sets the state for us
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Skip_Id3V2_Tag()
		{
			// Skip the version and flag bytes
			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out _, 24))
				return false;	// Read_Callback sets the state for us

			// Get the size (in bytes) to skip
			uint32_t skip = 0;

			for (uint32_t i = 0; i < 4; i++)
			{
				if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out Flac__uint32 x, 8))
					return false;	// Read_Callback sets the state for us

				skip <<= 7;
				skip |= (x & 0x7f);
			}

			// Skip the rest of the tag
			if (!decoder.Private.Input.Flac__BitReader_Skip_Byte_Block_Aligned_No_Crc(skip))
				return false;	// Read_Callback sets the state for us

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Frame_Sync()
		{
			Flac__bool first = true;

			// If we know the total number of samples in the stream, stop if we're read that many.
			// This will stop us, for example, from wasting time trying to sync on an ID3v1 tag
			if (Flac__Stream_Decoder_Get_Total_Samples() > 0)
			{
				if (decoder.Private.Samples_Decoded >= Flac__Stream_Decoder_Get_Total_Samples())
				{
					decoder.Protected.State = Flac__StreamDecoderState.End_Of_Stream;
					return true;
				}
			}

			// Make sure we're byte aligned
			if (!decoder.Private.Input.Flac__BitReader_Is_Consumed_Byte_Aligned())
			{
				if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out _, decoder.Private.Input.Flac__BitReader_Bits_Left_For_Byte_Alignment()))
					return false;	// Read_Callback sets the state for us
			}

			while (true)
			{
				Flac__uint32 x;

				if (decoder.Private.Cached)
				{
					x = decoder.Private.Lookahead;
					decoder.Private.Cached = false;
				}
				else
				{
					if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out x, 8))
						return false;	// Read_Callback sets the state for us
				}

				if (x == 0xff)	// MAGIC NUMBER for the first 8 frame sync bits
				{
					decoder.Private.Header_Warmup[0] = (Flac__byte)x;

					if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out x, 8))
						return false;	// Read_Callback sets the state for us

					// We have to check if we just read two 0xff's in a row; the second may actually be the beginning of the sync code
					// else we have to check if the second byte is the end of a sync code
					if (x == 0xff)	// MAGIC NUMBER for the first 8 frame sync bits
					{
						decoder.Private.Lookahead = (Flac__byte)x;
						decoder.Private.Cached = true;
					}
					else if (x >> 1 == 0x7c)	// MAGIC NUMBER for the last 6 sync bits and reserved 7th bit
					{
						decoder.Private.Header_Warmup[1] = (Flac__byte)x;
						decoder.Protected.State = Flac__StreamDecoderState.Read_Frame;

						return true;
					}
				}

				if (first)
				{
					Send_Error_To_Client(Flac__StreamDecoderErrorStatus.Lost_Sync);
					first = false;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Read_Frame(out Flac__bool got_A_Frame, Flac__bool do_Full_Decode)
		{
			got_A_Frame = false;

			// Init the CRC
			uint32_t frame_Crc = 0;
			frame_Crc = Crc.Flac__Crc16_Update(decoder.Private.Header_Warmup[0], (Flac__uint16)frame_Crc);
			frame_Crc = Crc.Flac__Crc16_Update(decoder.Private.Header_Warmup[1], (Flac__uint16)frame_Crc);
			decoder.Private.Input.Flac__BitReader_Reset_Read_Crc16((Flac__uint16)frame_Crc);

			if (!Read_Frame_Header())
				return false;

			if (decoder.Protected.State == Flac__StreamDecoderState.Search_For_Frame_Sync)	// Means we didn't sync on a valid header
				return true;

			if (!Allocate_Output(decoder.Private.Frame.Header.BlockSize, decoder.Private.Frame.Header.Channels))
				return false;

			for (uint32_t channel = 0; channel < decoder.Private.Frame.Header.Channels; channel++)
			{
				// First figure the correct bits-per-sample of the subframe
				uint32_t bps = decoder.Private.Frame.Header.Bits_Per_Sample;

				switch (decoder.Private.Frame.Header.Channel_Assignment)
				{
					case Flac__ChannelAssignment.Independent:
					{
						// No adjustment needed
						break;
					}

					case Flac__ChannelAssignment.Left_Side:
					{
						Debug.Assert(decoder.Private.Frame.Header.Channels == 2);

						if (channel == 1)
							bps++;

						break;
					}

					case Flac__ChannelAssignment.Right_Side:
					{
						Debug.Assert(decoder.Private.Frame.Header.Channels == 2);

						if (channel == 0)
							bps++;

						break;
					}

					case Flac__ChannelAssignment.Mid_Side:
					{
						Debug.Assert(decoder.Private.Frame.Header.Channels == 2);

						if (channel == 1)
							bps++;

						break;
					}

					default:
					{
						Debug.Assert(false);
						break;
					}
				}

				// Now read it
				if (!Read_SubFrame(channel, bps, do_Full_Decode))
					return false;

				if (decoder.Protected.State == Flac__StreamDecoderState.Search_For_Frame_Sync)	// Means bad sync or got corruption
					return true;
			}

			if (!Read_Zero_Padding())
				return false;

			if (decoder.Protected.State == Flac__StreamDecoderState.Search_For_Frame_Sync)	// Means bad sync or got corruption (i.e. "zero bits" were not all zeroes)
				return true;

			// Read the frame CRC-16 from the footer and check
			frame_Crc = decoder.Private.Input.Flac__BitReader_Get_Read_Crc16();

			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out Flac__uint32 x, Constants.Flac__Frame_Footer_Crc_Len))
				return false;	// Read_Callback sets the state for us

			if (frame_Crc == x)
			{
				if (do_Full_Decode)
				{
					// Undo any special channel coding
					switch (decoder.Private.Frame.Header.Channel_Assignment)
					{
						case Flac__ChannelAssignment.Independent:
						{
							// Do nothing
							break;
						}

						case Flac__ChannelAssignment.Left_Side:
						{
							Debug.Assert(decoder.Private.Frame.Header.Channels == 2);

							for (uint32_t i = 0; i < decoder.Private.Frame.Header.BlockSize; i++)
								decoder.Private.Output[1][i] = decoder.Private.Output[0][i] - decoder.Private.Output[1][i];

							break;
						}

						case Flac__ChannelAssignment.Right_Side:
						{
							Debug.Assert(decoder.Private.Frame.Header.Channels == 2);

							for (uint32_t i = 0; i < decoder.Private.Frame.Header.BlockSize; i++)
								decoder.Private.Output[0][i] += decoder.Private.Output[1][i];

							break;
						}

						case Flac__ChannelAssignment.Mid_Side:
						{
							Debug.Assert(decoder.Private.Frame.Header.Channels == 2);

							for (uint32_t i = 0; i < decoder.Private.Frame.Header.BlockSize; i++)
							{
								Flac__int32 mid = decoder.Private.Output[0][i];
								Flac__int32 side = decoder.Private.Output[1][i];
								mid = (Flac__int32)(((uint32_t)mid) << 1);
								mid |= (side & 1);	// I.e. if 'side' is odd...
								decoder.Private.Output[0][i] = (mid + side) >> 1;
								decoder.Private.Output[1][i] = (mid - side) >> 1;
							}
							break;
						}

						default:
						{
							Debug.Assert(false);
							break;
						}
					}
				}
			}
			else
			{
				// Bad frame, emit error and zero the output signal
				Send_Error_To_Client(Flac__StreamDecoderErrorStatus.Crc_Mismatch);

				if (do_Full_Decode)
				{
					for (uint32_t channel = 0; channel < decoder.Private.Frame.Header.Channels; channel++)
						Array.Clear(decoder.Private.Output[channel]);
				}
			}

			got_A_Frame = true;

			// We wait to update Fixed_Block_Size until here, when we're sure we've got a proper frame and hence a correct blocksize
			if (decoder.Private.Next_Fixed_Block_Size != 0)
				decoder.Private.Fixed_Block_Size = decoder.Private.Next_Fixed_Block_Size;

			// Put the latest values into the public section of the decoder instance
			decoder.Protected.Channels = decoder.Private.Frame.Header.Channels;
			decoder.Protected.Channel_Assignment = decoder.Private.Frame.Header.Channel_Assignment;
			decoder.Protected.Bits_Per_Sample = decoder.Private.Frame.Header.Bits_Per_Sample;
			decoder.Protected.Sample_Rate = decoder.Private.Frame.Header.Sample_Rate;
			decoder.Protected.BlockSize = decoder.Private.Frame.Header.BlockSize;

			Debug.Assert(decoder.Private.Frame.Header.Number_Type == Flac__FrameNumberType.Sample_Number);

			decoder.Private.Samples_Decoded = decoder.Private.Frame.Header.Sample_Number + decoder.Private.Frame.Header.BlockSize;

			// Write it
			if (do_Full_Decode)
			{
				if (Write_Audio_Frame_To_Client(decoder.Private.Frame, decoder.Private.Output) != Flac__StreamDecoderWriteStatus.Continue)
				{
					decoder.Protected.State = Flac__StreamDecoderState.Aborted;
					return false;
				}
			}

			decoder.Protected.State = Flac__StreamDecoderState.Search_For_Frame_Sync;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Read_Frame_Header()
		{
			uint32_t blockSize_Hint = 0;
			uint32_t sample_Rate_Hint = 0;
			Flac__byte[] raw_Header = new Flac__byte[16];	// MAGIC NUMBER based on the maximum frame header size, including CRC
			Flac__bool is_Unparseable = false;

			Debug.Assert(decoder.Private.Input.Flac__BitReader_Is_Consumed_Byte_Aligned());

			// Init the raw header with the saved bits from synchronization
			raw_Header[0] = decoder.Private.Header_Warmup[0];
			raw_Header[1] = decoder.Private.Header_Warmup[1];
			uint32_t raw_Header_Len = 2;

			// Check to make sure that reserved bit is 0
			if ((raw_Header[1] & 0x02) != 0)	// MAGIC NUMBER
				is_Unparseable = true;

			// Note that along the way as we read the header, we look for a sync
			// code inside. If we find one it would indicate that our original
			// sync was bad since there cannot be a sync code in a valid header.
			//
			// Three kinds of things can go wrong when reading the frame header:
			//  1) We may have sync'ed incorrectly and not landed on a frame header.
			//     If we don't find a sync code, it can end up looking like we read
			//     a valid but unparseable header, until getting to the frame header
			//     CRC. Even then we could get a false positive on the CRC.
			//  2) We may have sync'ed correctly but on an unparseable frame (from a
			//     future encoder).
			//  3) We may be on a damaged frame which appears valid but unparseable.
			//
			// For all these reasons, we try and read a complete frame header as
			// long as it seems valid, even if unparseable, up until the frame
			// header CRC
			Flac__uint32 x;

			// Read in the raw header as bytes so we can CRC it, and parse it on the way
			for (uint32_t i = 0; i < 2; i++)
			{
				if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out x, 8))
					return false;	// Read_Callback sets the state for us

				if (x == 0xff)	// MAGIC NUMBER for the first 8 frame sync bits
				{
					// If we get here it means our original sync was erroneous since the sync code cannot appear in the header
					decoder.Private.Lookahead = (Flac__byte)x;
					decoder.Private.Cached = true;

					Send_Error_To_Client(Flac__StreamDecoderErrorStatus.Bad_Header);

					decoder.Protected.State = Flac__StreamDecoderState.Search_For_Frame_Sync;

					return true;
				}

				raw_Header[raw_Header_Len++] = (Flac__byte)x;
			}

			decoder.Private.Frame = new Flac__Frame();

			switch (x = (uint)raw_Header[2] >> 4)
			{
				case 0:
				{
					is_Unparseable = true;
					break;
				}

				case 1:
				{
					decoder.Private.Frame.Header.BlockSize = 192;
					break;
				}

				case 2:
				case 3:
				case 4:
				case 5:
				{
					decoder.Private.Frame.Header.BlockSize = 576U << ((int)x - 2);
					break;
				}

				case 6:
				case 7:
				{
					blockSize_Hint = x;
					break;
				}

				case 8:
				case 9:
				case 10:
				case 11:
				case 12:
				case 13:
				case 14:
				case 15:
				{
					decoder.Private.Frame.Header.BlockSize = 256U << ((int)x - 8);
					break;
				}

				default:
				{
					Debug.Assert(false);
					break;
				}
			}

			switch (x = (uint)raw_Header[2] & 0x0f)
			{
				case 0:
				{
					if (decoder.Private.Has_Stream_Info)
						decoder.Private.Frame.Header.Sample_Rate = ((Flac__StreamMetadata_StreamInfo)decoder.Private.Stream_Info.Data).Sample_Rate;
					else
						is_Unparseable = true;

					break;
				}

				case 1:
				{
					decoder.Private.Frame.Header.Sample_Rate = 88200;
					break;
				}

				case 2:
				{
					decoder.Private.Frame.Header.Sample_Rate = 176400;
					break;
				}

				case 3:
				{
					decoder.Private.Frame.Header.Sample_Rate = 192000;
					break;
				}

				case 4:
				{
					decoder.Private.Frame.Header.Sample_Rate = 8000;
					break;
				}

				case 5:
				{
					decoder.Private.Frame.Header.Sample_Rate = 16000;
					break;
				}

				case 6:
				{
					decoder.Private.Frame.Header.Sample_Rate = 22050;
					break;
				}

				case 7:
				{
					decoder.Private.Frame.Header.Sample_Rate = 24000;
					break;
				}

				case 8:
				{
					decoder.Private.Frame.Header.Sample_Rate = 32000;
					break;
				}

				case 9:
				{
					decoder.Private.Frame.Header.Sample_Rate = 44100;
					break;
				}

				case 10:
				{
					decoder.Private.Frame.Header.Sample_Rate = 48000;
					break;
				}

				case 11:
				{
					decoder.Private.Frame.Header.Sample_Rate = 96000;
					break;
				}

				case 12:
				case 13:
				case 14:
				{
					sample_Rate_Hint = x;
					break;
				}

				case 15:
				{
					Send_Error_To_Client(Flac__StreamDecoderErrorStatus.Bad_Header);
					decoder.Protected.State = Flac__StreamDecoderState.Search_For_Frame_Sync;

					return true;
				}

				default:
				{
					Debug.Assert(false);
					break;
				}
			}

			x = (uint32_t)(raw_Header[3] >> 4);
			if ((x & 8) != 0)
			{
				decoder.Private.Frame.Header.Channels = 2;

				switch (x & 7)
				{
					case 0:
					{
						decoder.Private.Frame.Header.Channel_Assignment = Flac__ChannelAssignment.Left_Side;
						break;
					}

					case 1:
					{
						decoder.Private.Frame.Header.Channel_Assignment = Flac__ChannelAssignment.Right_Side;
						break;
					}

					case 2:
					{
						decoder.Private.Frame.Header.Channel_Assignment = Flac__ChannelAssignment.Mid_Side;
						break;
					}

					default:
					{
						is_Unparseable = true;
						break;
					}
				}
			}
			else
			{
				decoder.Private.Frame.Header.Channels = x + 1;
				decoder.Private.Frame.Header.Channel_Assignment = Flac__ChannelAssignment.Independent;
			}

			switch (x = (uint32_t)(raw_Header[3] & 0x0e) >> 1)
			{
				case 0:
				{
					if (decoder.Private.Has_Stream_Info)
						decoder.Private.Frame.Header.Bits_Per_Sample = ((Flac__StreamMetadata_StreamInfo)decoder.Private.Stream_Info.Data).Bits_Per_Sample;
					else
						is_Unparseable = true;

					break;
				}

				case 1:
				{
					decoder.Private.Frame.Header.Bits_Per_Sample = 8;
					break;
				}

				case 2:
				{
					decoder.Private.Frame.Header.Bits_Per_Sample = 12;
					break;
				}

				case 4:
				{
					decoder.Private.Frame.Header.Bits_Per_Sample = 16;
					break;
				}

				case 5:
				{
					decoder.Private.Frame.Header.Bits_Per_Sample = 20;
					break;
				}

				case 6:
				{
					decoder.Private.Frame.Header.Bits_Per_Sample = 24;
					break;
				}

				case 3:
				case 7:
				{
					is_Unparseable = true;
					break;
				}

				default:
				{
					Debug.Assert(false);
					break;
				}
			}

			if ((decoder.Private.Frame.Header.Bits_Per_Sample == 32) && (decoder.Private.Frame.Header.Channel_Assignment != Flac__ChannelAssignment.Independent))
			{
				// Decoder isn't equipped for 33-bit side frame
				is_Unparseable = true;
			}

			// Check to make sure that reserved bit is 0
			if ((raw_Header[3] & 0x01) != 0)	// MAGIC NUMBER
				is_Unparseable = true;

			// Read the frame's starting sample number (or frame number as the case may be)
			if (((raw_Header[1] & 0x01) != 0) ||
			    // @@@ This clause is a concession on the old way of doing variable blocksize; the only known implementation is flake and can probably be removed without inconveniencing anyone
			    (decoder.Private.Has_Stream_Info && (((Flac__StreamMetadata_StreamInfo)decoder.Private.Stream_Info.Data).Min_BlockSize != ((Flac__StreamMetadata_StreamInfo)decoder.Private.Stream_Info.Data).Max_BlockSize)))
			{
				// Variable block size
				if (!decoder.Private.Input.Flac__BitReader_Read_Utf8_UInt64(out Flac__uint64 xx, raw_Header, ref raw_Header_Len))
					return false;	// Read_Callback sets the state for us

				if (xx == uint64_t.MaxValue)	// I.e. non-UTF8 code...
				{
					decoder.Private.Lookahead = raw_Header[raw_Header_Len - 1];	// Back up as much as we can
					decoder.Private.Cached = true;

					Send_Error_To_Client(Flac__StreamDecoderErrorStatus.Bad_Header);

					decoder.Protected.State = Flac__StreamDecoderState.Search_For_Frame_Sync;

					return true;
				}

				decoder.Private.Frame.Header.Number_Type = Flac__FrameNumberType.Sample_Number;
				decoder.Private.Frame.Header.Sample_Number = xx;
			}
			else
			{
				// Fixed block size
				if (!decoder.Private.Input.Flac__BitReader_Read_Utf8_UInt32(out x, raw_Header, ref raw_Header_Len))
					return false;	// Read_Callback sets the state for us

				if (x == uint32_t.MaxValue)	// I.e. non-UTF8 code...
				{
					decoder.Private.Lookahead = raw_Header[raw_Header_Len - 1];	// Back up as much as we can
					decoder.Private.Cached = true;

					Send_Error_To_Client(Flac__StreamDecoderErrorStatus.Bad_Header);

					decoder.Protected.State = Flac__StreamDecoderState.Search_For_Frame_Sync;

					return true;
				}

				decoder.Private.Frame.Header.Number_Type = Flac__FrameNumberType.Frame_Number;
				decoder.Private.Frame.Header.Frame_Number = x;
			}

			if (blockSize_Hint != 0)
			{
				if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out x, 8))
					return false;	// Read_Callback sets the state for us

				raw_Header[raw_Header_Len++] = (Flac__byte)x;

				if (blockSize_Hint == 7)
				{
					if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out uint32_t _x, 8))
						return false;	// Read_Callback sets the state for us

					raw_Header[raw_Header_Len++] = (Flac__byte)_x;
					x = (x << 8) | _x;
				}

				decoder.Private.Frame.Header.BlockSize = x + 1;
			}

			if (sample_Rate_Hint != 0)
			{
				if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out x, 8))
					return false;	// Read_Callback sets the state for us

				raw_Header[raw_Header_Len++] = (Flac__byte)x;

				if (sample_Rate_Hint != 12)
				{
					if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out uint32_t _x, 8))
						return false;	// Read_Callback sets the state for us

					raw_Header[raw_Header_Len++] = (Flac__byte)_x;
					x = (x << 8) | _x;
				}

				if (sample_Rate_Hint == 12)
					decoder.Private.Frame.Header.Sample_Rate = x * 1000;
				else if (sample_Rate_Hint == 13)
					decoder.Private.Frame.Header.Sample_Rate = x;
				else
					decoder.Private.Frame.Header.Sample_Rate = x * 10;
			}

			// Read the CRC-8 byte
			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out x, 8))
				return false;	// Read_Callback sets the state for us

			Flac__byte crc8 = (Flac__byte)x;

			if (Crc.Flac__Crc8(raw_Header, raw_Header_Len) != crc8)
			{
				Send_Error_To_Client(Flac__StreamDecoderErrorStatus.Bad_Header);
				decoder.Protected.State = Flac__StreamDecoderState.Search_For_Frame_Sync;

				return true;
			}

			// Calculate the sample number from the frame number if needed
			decoder.Private.Next_Fixed_Block_Size = 0;

			if (decoder.Private.Frame.Header.Number_Type == Flac__FrameNumberType.Frame_Number)
			{
				x = decoder.Private.Frame.Header.Frame_Number;
				decoder.Private.Frame.Header.Number_Type = Flac__FrameNumberType.Sample_Number;

				if (decoder.Private.Fixed_Block_Size != 0)
					decoder.Private.Frame.Header.Sample_Number = (Flac__uint64)decoder.Private.Fixed_Block_Size * x;
				else if (decoder.Private.Has_Stream_Info)
				{
					if (((Flac__StreamMetadata_StreamInfo)decoder.Private.Stream_Info.Data).Min_BlockSize == ((Flac__StreamMetadata_StreamInfo)decoder.Private.Stream_Info.Data).Max_BlockSize)
					{
						decoder.Private.Frame.Header.Sample_Number = (Flac__uint64)((Flac__StreamMetadata_StreamInfo)decoder.Private.Stream_Info.Data).Min_BlockSize * x;
						decoder.Private.Next_Fixed_Block_Size = ((Flac__StreamMetadata_StreamInfo)decoder.Private.Stream_Info.Data).Max_BlockSize;
					}
					else
						is_Unparseable = true;
				}
				else if (x == 0)
				{
					decoder.Private.Frame.Header.Sample_Number = 0;
					decoder.Private.Next_Fixed_Block_Size = decoder.Private.Frame.Header.BlockSize;
				}
				else
				{
					// Can only get here if the stream has invalid frame numbering and no STREAMINFO, so assume it's not the last (possible short) frame
					decoder.Private.Frame.Header.Sample_Number = (Flac__uint64)decoder.Private.Frame.Header.BlockSize * x;
				}
			}

			if (is_Unparseable)
			{
				Send_Error_To_Client(Flac__StreamDecoderErrorStatus.Unparseable_Stream);
				decoder.Protected.State = Flac__StreamDecoderState.Search_For_Frame_Sync;

				return true;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Read_SubFrame(uint32_t channel, uint32_t bps, Flac__bool do_Full_Decode)
		{
			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out Flac__uint32 x, 8))	// MAGIC NUMBER
				return false;	// Read_Callback sets the state for us

			Flac__bool wasted_Bits = (x & 1) != 0;
			x &= 0xfe;

			if (wasted_Bits)
			{
				if (!decoder.Private.Input.Flac__BitReader_Read_Unary_Unsigned(out uint32_t u))
					return false;	// Read_Callback sets the state for us

				decoder.Private.Frame.SubFrames[channel].Wasted_Bits = u + 1;

				if (decoder.Private.Frame.SubFrames[channel].Wasted_Bits >= bps)
					return false;

				bps -= decoder.Private.Frame.SubFrames[channel].Wasted_Bits;
			}
			else
				decoder.Private.Frame.SubFrames[channel].Wasted_Bits = 0;

			// Lots of magic numbers here
			if ((x & 0x80) != 0)
			{
				Send_Error_To_Client(Flac__StreamDecoderErrorStatus.Lost_Sync);
				decoder.Protected.State = Flac__StreamDecoderState.Search_For_Frame_Sync;

				return true;
			}
			else if (x == 0)
			{
				if (!Read_SubFrame_Constant(channel, bps, do_Full_Decode))
					return false;
			}
			else if (x == 2)
			{
				if (!Read_SubFrame_Verbatim(channel, bps, do_Full_Decode))
					return false;
			}
			else if (x < 16)
			{
				Send_Error_To_Client(Flac__StreamDecoderErrorStatus.Unparseable_Stream);
				decoder.Protected.State = Flac__StreamDecoderState.Search_For_Frame_Sync;

				return true;
			}
			else if (x <= 24)
			{
				uint32_t predictor_Order = (x >> 1) & 7;

				if (decoder.Private.Frame.Header.Bits_Per_Sample > 24)
				{
					// Decoder isn't equipped for fixed subframes with more than 24 bps
					Send_Error_To_Client(Flac__StreamDecoderErrorStatus.Unparseable_Stream);
					decoder.Protected.State = Flac__StreamDecoderState.Search_For_Frame_Sync;

					return true;
				}

				if (decoder.Private.Frame.Header.BlockSize <= predictor_Order)
				{
					Send_Error_To_Client(Flac__StreamDecoderErrorStatus.Lost_Sync);
					decoder.Protected.State = Flac__StreamDecoderState.Search_For_Frame_Sync;

					return true;
				}

				if (!Read_SubFrame_Fixed(channel, bps, predictor_Order, do_Full_Decode))
					return false;

				if (decoder.Protected.State == Flac__StreamDecoderState.Search_For_Frame_Sync)	// Means bad sync or got corruption
					return true;
			}
			else if (x < 64)
			{
				Send_Error_To_Client(Flac__StreamDecoderErrorStatus.Unparseable_Stream);
				decoder.Protected.State = Flac__StreamDecoderState.Search_For_Frame_Sync;

				return true;
			}
			else
			{
				uint32_t predictor_Order = ((x >> 1) & 31) + 1;

				if (decoder.Private.Frame.Header.BlockSize <= predictor_Order)
				{
					Send_Error_To_Client(Flac__StreamDecoderErrorStatus.Lost_Sync);
					decoder.Protected.State = Flac__StreamDecoderState.Search_For_Frame_Sync;

					return true;
				}

				if (!Read_SubFrame_Lpc(channel, bps, predictor_Order, do_Full_Decode))
					return false;

				if (decoder.Protected.State == Flac__StreamDecoderState.Search_For_Frame_Sync)	// Means bad sync or got corruption
					return true;
			}

			if (wasted_Bits && do_Full_Decode)
			{
				x = decoder.Private.Frame.SubFrames[channel].Wasted_Bits;

				for (uint32_t i = 0; i < decoder.Private.Frame.Header.BlockSize; i++)
				{
					uint32_t val = (uint32_t)decoder.Private.Output[channel][i];
					decoder.Private.Output[channel][i] = (int32_t)(val << (int32_t)x);
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Read_SubFrame_Fixed(uint32_t channel, uint32_t bps, uint32_t order, Flac__bool do_Full_Decode)
		{
			Flac__SubFrame_Fixed subFrame = new Flac__SubFrame_Fixed();
			decoder.Private.Frame.SubFrames[channel].Data = subFrame;

			decoder.Private.Frame.SubFrames[channel].Type = Flac__SubFrameType.Fixed;

			subFrame.Residual = decoder.Private.Residual[channel];
			subFrame.Order = order;

			// Read warm-up samples
			for (uint32_t u = 0; u < order; u++)
			{
				if (!decoder.Private.Input.Flac__BitReader_Read_Raw_Int32(out Flac__int32 i32, bps))
					return false;	// Read_Callback sets the state for us

				subFrame.Warmup[u] = i32;
			}

			// Read entropy coding method info
			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out Flac__uint32 u32, Constants.Flac__Entropy_Coding_Method_Type_Len))
				return false;	// Read_Callback sets the state for us

			subFrame.Entropy_Coding_Method.Type = (Flac__EntropyCodingMethodType)u32;

			switch (subFrame.Entropy_Coding_Method.Type)
			{
				case Flac__EntropyCodingMethodType.Partitioned_Rice:
				case Flac__EntropyCodingMethodType.Partitioned_Rice2:
				{
					if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out u32, Constants.Flac__Entropy_Coding_Method_Partitioned_Rice_Order_Len))
						return false;	// Read_Callback sets the state for us

					if (((decoder.Private.Frame.Header.BlockSize >> (int32_t)u32) < order) || ((decoder.Private.Frame.Header.BlockSize % (1 << (int32_t)u32) > 0)))
					{
						Send_Error_To_Client(Flac__StreamDecoderErrorStatus.Lost_Sync);
						decoder.Protected.State = Flac__StreamDecoderState.Search_For_Frame_Sync;

						return true;
					}

					Flac__EntropyCodingMethod_PartitionedRice data = new Flac__EntropyCodingMethod_PartitionedRice();
					subFrame.Entropy_Coding_Method.Data = data;

					data.Order = u32;
					data.Contents = decoder.Private.Partitioned_Rice_Contents[channel];
					break;
				}

				default:
				{
					Send_Error_To_Client(Flac__StreamDecoderErrorStatus.Unparseable_Stream);
					decoder.Protected.State = Flac__StreamDecoderState.Search_For_Frame_Sync;

					return true;
				}
			}

			// Read residual
			switch (subFrame.Entropy_Coding_Method.Type)
			{
				case Flac__EntropyCodingMethodType.Partitioned_Rice:
				case Flac__EntropyCodingMethodType.Partitioned_Rice2:
				{
					if (!Read_Residual_Partitioned_Rice(order, ((Flac__EntropyCodingMethod_PartitionedRice)subFrame.Entropy_Coding_Method.Data).Order, decoder.Private.Partitioned_Rice_Contents[channel], decoder.Private.Residual[channel], subFrame.Entropy_Coding_Method.Type == Flac__EntropyCodingMethodType.Partitioned_Rice2))
						return false;

					break;
				}

				default:
				{
					Debug.Assert(false);
					break;
				}
			}

			// Decode the subframe
			if (do_Full_Decode)
			{
				Array.Copy(subFrame.Warmup, decoder.Private.Output[channel], order);
				Fixed.Flac__Fixed_Restore_Signal(decoder.Private.Residual[channel], decoder.Private.Frame.Header.BlockSize - order, order, decoder.Private.Output[channel], order);
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Read_SubFrame_Lpc(uint32_t channel, uint32_t bps, uint32_t order, Flac__bool do_Full_Decode)
		{
			Flac__SubFrame_Lpc subFrame = new Flac__SubFrame_Lpc();
			decoder.Private.Frame.SubFrames[channel].Data = subFrame;

			decoder.Private.Frame.SubFrames[channel].Type = Flac__SubFrameType.Lpc;

			subFrame.Residual = decoder.Private.Residual[channel];
			subFrame.Order = order;

			Flac__int32 i32;

			// Read warm-up samples
			for (uint32_t u = 0; u < order; u++)
			{
				if (!decoder.Private.Input.Flac__BitReader_Read_Raw_Int32(out i32, bps))
					return false;	// Read_Callback sets the state for us

				subFrame.Warmup[u] = i32;
			}

			// Read glp coeff precision
			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out Flac__uint32 u32, Constants.Flac__SubFrame_Lpc_Qlp_Coeff_Precision_Len))
				return false;	// Read_Callback sets the state for us

			if (u32 == (1U << (int)Constants.Flac__SubFrame_Lpc_Qlp_Coeff_Precision_Len) - 1)
			{
				Send_Error_To_Client(Flac__StreamDecoderErrorStatus.Lost_Sync);
				decoder.Protected.State = Flac__StreamDecoderState.Search_For_Frame_Sync;

				return true;
			}

			subFrame.Qlp_Coeff_Precision = u32 + 1;

			// Read qlp shift
			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_Int32(out i32, Constants.Flac__SubFrame_Lpc_Qlp_Shift_Len))
				return false;	// Read_Callback sets the state for us

			if (i32 < 0)
			{
				Send_Error_To_Client(Flac__StreamDecoderErrorStatus.Lost_Sync);
				decoder.Protected.State = Flac__StreamDecoderState.Search_For_Frame_Sync;

				return true;
			}

			subFrame.Quantization_Level = i32;

			// Read quantized lp coefficients
			for (uint32_t u = 0; u < order; u++)
			{
				if (!decoder.Private.Input.Flac__BitReader_Read_Raw_Int32(out i32, subFrame.Qlp_Coeff_Precision))
					return false;	// Read_Callback sets the state for us

				subFrame.Qlp_Coeff[u] = i32;
			}

			// Read entropy coding method info
			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out u32, Constants.Flac__Entropy_Coding_Method_Type_Len))
				return false;	// Read_Callback sets the state for us

			subFrame.Entropy_Coding_Method.Type = (Flac__EntropyCodingMethodType)u32;

			switch (subFrame.Entropy_Coding_Method.Type)
			{
				case Flac__EntropyCodingMethodType.Partitioned_Rice:
				case Flac__EntropyCodingMethodType.Partitioned_Rice2:
				{
					Flac__EntropyCodingMethod_PartitionedRice partitionedRice = new Flac__EntropyCodingMethod_PartitionedRice();
					subFrame.Entropy_Coding_Method.Data = partitionedRice;

					if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out u32, Constants.Flac__Entropy_Coding_Method_Partitioned_Rice_Order_Len))
						return false;	// Read_Callback sets the state for us

					if (((decoder.Private.Frame.Header.BlockSize >> (int)u32) < order) || ((decoder.Private.Frame.Header.BlockSize % (1 << (int)u32)) > 0))
					{
						Send_Error_To_Client(Flac__StreamDecoderErrorStatus.Lost_Sync);
						decoder.Protected.State = Flac__StreamDecoderState.Search_For_Frame_Sync;

						return true;
					}

					partitionedRice.Order = u32;
					partitionedRice.Contents = decoder.Private.Partitioned_Rice_Contents[channel];
					break;
				}

				default:
				{
					Send_Error_To_Client(Flac__StreamDecoderErrorStatus.Unparseable_Stream);
					decoder.Protected.State = Flac__StreamDecoderState.Search_For_Frame_Sync;

					return true;
				}
			}

			// Read residual
			switch (subFrame.Entropy_Coding_Method.Type)
			{
				case Flac__EntropyCodingMethodType.Partitioned_Rice:
				case Flac__EntropyCodingMethodType.Partitioned_Rice2:
				{
					if (!Read_Residual_Partitioned_Rice(order, ((Flac__EntropyCodingMethod_PartitionedRice)subFrame.Entropy_Coding_Method.Data).Order, decoder.Private.Partitioned_Rice_Contents[channel], decoder.Private.Residual[channel], subFrame.Entropy_Coding_Method.Type == Flac__EntropyCodingMethodType.Partitioned_Rice2))
						return false;

					break;
				}

				default:
				{
					Debug.Assert(false);
					break;
				}
			}

			// Decode the subframe
			if (do_Full_Decode)
			{
				Array.Copy(subFrame.Warmup, decoder.Private.Output[channel], order);

				if ((bps + subFrame.Qlp_Coeff_Precision + BitMath.Flac__BitMath_ILog2(order)) <= 32)
				{
					if ((bps <= 16) && (subFrame.Qlp_Coeff_Precision <= 16))
						decoder.Private.Lpc.Restore_Signal_16Bit(decoder.Private.Residual[channel], decoder.Private.Frame.Header.BlockSize - order, subFrame.Qlp_Coeff, order, subFrame.Quantization_Level, decoder.Private.Output[channel], order);
					else
						decoder.Private.Lpc.Restore_Signal(decoder.Private.Residual[channel], decoder.Private.Frame.Header.BlockSize - order, subFrame.Qlp_Coeff, order, subFrame.Quantization_Level, decoder.Private.Output[channel], order);
				}
				else
					decoder.Private.Lpc.Restore_Signal_64Bit(decoder.Private.Residual[channel], decoder.Private.Frame.Header.BlockSize - order, subFrame.Qlp_Coeff, order, subFrame.Quantization_Level, decoder.Private.Output[channel], order);
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Read_SubFrame_Constant(uint32_t channel, uint32_t bps, Flac__bool do_Full_Decode)
		{
			Flac__SubFrame_Constant subFrame = new Flac__SubFrame_Constant();
			decoder.Private.Frame.SubFrames[channel].Data = subFrame;

			Flac__int32[] output = decoder.Private.Output[channel];

			decoder.Private.Frame.SubFrames[channel].Type = Flac__SubFrameType.Constant;

			if (!decoder.Private.Input.Flac__BitReader_Read_Raw_Int32(out Flac__int32 x, bps))
				return false;	// Read_Callback sets the state for us

			subFrame.Value = x;

			// Decode the subframe
			if (do_Full_Decode)
			{
				for (uint32_t i = 0; i < decoder.Private.Frame.Header.BlockSize; i++)
					output[i] = x;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Read_SubFrame_Verbatim(uint32_t channel, uint32_t bps, Flac__bool do_Full_Decode)
		{
			Flac__SubFrame_Verbatim subFrame = new Flac__SubFrame_Verbatim();
			decoder.Private.Frame.SubFrames[channel].Data = subFrame;

			Flac__int32[] residual = decoder.Private.Residual[channel];

			decoder.Private.Frame.SubFrames[channel].Type = Flac__SubFrameType.Verbatim;

			subFrame.Data = residual;

			for (uint32_t i = 0; i < decoder.Private.Frame.Header.BlockSize; i++)
			{
				if (!decoder.Private.Input.Flac__BitReader_Read_Raw_Int32(out Flac__int32 x, bps))
					return false;	// Read_Callback sets the state for us

				residual[i] = x;
			}

			// Decode the subframe
			if (do_Full_Decode)
				Array.Copy(subFrame.Data, decoder.Private.Output[channel], decoder.Private.Frame.Header.BlockSize);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Read_Residual_Partitioned_Rice(uint32_t predictor_Order, uint32_t partition_Order, Flac__EntropyCodingMethod_PartitionedRiceContents partitioned_Rice_Contents, Flac__int32[] residual, Flac__bool is_Extended)
		{
			uint32_t partitions = 1U << (int32_t)partition_Order;
			uint32_t partition_Samples = decoder.Private.Frame.Header.BlockSize >> (int32_t)partition_Order;
			uint32_t pLen = is_Extended ? Constants.Flac__Entropy_Coding_Method_Partitioned_Rice2_Parameter_Len : Constants.Flac__Entropy_Coding_Method_Partitioned_Rice_Parameter_Len;
			uint32_t pEsc = is_Extended ? Constants.Flac__Entropy_Coding_Method_Partitioned_Rice2_Escape_Parameter : Constants.Flac__Entropy_Coding_Method_Partitioned_Rice_Escape_Parameter;

			// Invalid predictor and partition orders much be handled in the callers
			Debug.Assert(partition_Order > 0 ? partition_Samples >= predictor_Order : decoder.Private.Frame.Header.BlockSize >= predictor_Order);

			if (!Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Ensure_Size(partitioned_Rice_Contents, Math.Max(6, partition_Order)))
			{
				decoder.Protected.State = Flac__StreamDecoderState.Memory_Allocation_Error;
				return false;
			}

			uint32_t sample = 0;
			for (uint32_t partition = 0; partition < partitions; partition++)
			{
				if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out uint32_t rice_Parameter, pLen))
					return false;	// Read_Callback sets the state for us

				partitioned_Rice_Contents.Parameters[partition] = rice_Parameter;

				if (rice_Parameter < pEsc)
				{
					partitioned_Rice_Contents.Raw_Bits[partition] = 0;
					uint32_t u = (partition == 0) ? partition_Samples - predictor_Order : partition_Samples;

					if (!decoder.Private.Input.Flac__BitReader_Read_Rice_Signed_Block(residual, sample, u, rice_Parameter))
						return false;	// Read_Callback sets the state for us

					sample += u;
				}
				else
				{
					if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out rice_Parameter, Constants.Flac__Entropy_Coding_Method_Partitioned_Rice_Raw_Len))
						return false;	// Read_Callback sets the state for us

					partitioned_Rice_Contents.Raw_Bits[partition] = rice_Parameter;

					for (uint32_t u = (partition == 0) ? predictor_Order : 0; u < partition_Samples; u++, sample++)
					{
						if (!decoder.Private.Input.Flac__BitReader_Read_Raw_Int32(out int32_t i, rice_Parameter))
							return false;	// Read_Callback sets the state for us

						residual[sample] = i;
					}
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Read_Zero_Padding()
 		{
			if (!decoder.Private.Input.Flac__BitReader_Is_Consumed_Byte_Aligned())
			{
				if (!decoder.Private.Input.Flac__BitReader_Read_Raw_UInt32(out Flac__uint32 zero, decoder.Private.Input.Flac__BitReader_Bits_Left_For_Byte_Alignment()))
					return false;	// Read_Callback sets the state for us

				if (zero != 0)
				{
					Send_Error_To_Client(Flac__StreamDecoderErrorStatus.Lost_Sync);
					decoder.Protected.State = Flac__StreamDecoderState.Search_For_Frame_Sync;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__bool Read_Callback(Span<Flac__byte> buffer, ref size_t bytes, object client_Data)
		{
			Stream_Decoder streamDecoder = (Stream_Decoder)client_Data;
			Flac__StreamDecoder decoder = streamDecoder.decoder;

			if ((decoder.Private.Eof_Callback != null) && decoder.Private.Eof_Callback(streamDecoder, decoder.Private.Client_Data))
			{
				bytes = 0;
				decoder.Protected.State = Flac__StreamDecoderState.End_Of_Stream;
				return false;
			}
			else if (bytes > 0)
			{
				// While seeking, it is possible for our seek to land in the
				// middle of audio data that looks exactly like a frame header
				// from a future version of an encoder. When that happens, our
				// error callback will get an FLAC__STREAM_DECODER_UNPARSEABLE_STREAM and increment its
				// unparseable_frame_count. But there is a remote possibility
				// that it is properly synced at such a "future-codec frame",
				// so to make sure, we wait to see many "unparseable" errors in
				// a row before bailing out
				if (decoder.Private.Is_Seeking && (decoder.Private.Unparseable_Frame_Count > 20))
				{
					decoder.Protected.State = Flac__StreamDecoderState.Aborted;
					return false;
				}
				else
				{
					Flac__StreamDecoderReadStatus status = decoder.Private.Read_Callback(streamDecoder, buffer, ref bytes, decoder.Private.Client_Data);
					if (status == Flac__StreamDecoderReadStatus.Abort)
					{
						decoder.Protected.State = Flac__StreamDecoderState.Aborted;
						return false;
					}
					else if (bytes == 0)
					{
						if ((status == Flac__StreamDecoderReadStatus.End_Of_Stream) || ((decoder.Private.Eof_Callback != null) && decoder.Private.Eof_Callback(streamDecoder, decoder.Private.Client_Data)))
						{
							decoder.Protected.State = Flac__StreamDecoderState.End_Of_Stream;
							return false;
						}
						else
							return true;
					}
					else
						return true;
				}
			}
			else
			{
				// Abort to avoid a deadlock
				decoder.Protected.State = Flac__StreamDecoderState.Aborted;
				return false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__StreamDecoderWriteStatus Write_Audio_Frame_To_Client(Flac__Frame frame, Flac__int32[][] buffer)
		{
			if (decoder.Private.Is_Seeking)
			{
				Flac__uint64 this_Frame_Sample = frame.Header.Sample_Number;
				Flac__uint64 next_Frame_Sample = this_Frame_Sample + frame.Header.BlockSize;
				Flac__uint64 target_Sample = decoder.Private.Target_Sample;

				Debug.Assert(frame.Header.Number_Type == Flac__FrameNumberType.Sample_Number);

				decoder.Private.Last_Frame = frame;	// Save the frame

				if ((this_Frame_Sample <= target_Sample) && (target_Sample < next_Frame_Sample))	// We hit our target frame
				{
					uint32_t delta = (uint32_t)(target_Sample - this_Frame_Sample);

					// Kick out of seek mode
					decoder.Private.Is_Seeking = false;

					// Shift out the samples before target_Sample
					if (delta > 0)
					{
						Flac__int32[][] newBuffer = new Flac__int32[Constants.Flac__Max_Channels][];

						for (uint32_t channel = 0; channel < frame.Header.Channels; channel++)
							newBuffer[channel] = buffer[channel].AsSpan((int)delta).ToArray();

						decoder.Private.Last_Frame.Header.BlockSize -= delta;
						decoder.Private.Last_Frame.Header.Sample_Number += delta;

						// Write the relevant samples
						return decoder.Private.Write_Callback(this, decoder.Private.Last_Frame, newBuffer, decoder.Private.Client_Data);
					}
					else
					{
						// Write the relevant samples
						return decoder.Private.Write_Callback(this, frame, buffer, decoder.Private.Client_Data);
					}
				}
				else
					return Flac__StreamDecoderWriteStatus.Continue;
			}
			else
			{
				// If we never got STREAMINFO, turn off MD5 checking to save
				// cycles since we don't have a sum to compare to anyway
				if (!decoder.Private.Has_Stream_Info)
					decoder.Private.Do_Md5_Checking = false;

				if (decoder.Private.Do_Md5_Checking)
				{
					if (!decoder.Private.Md5.Flac__Md5Accumulate(buffer, frame.Header.Channels, frame.Header.BlockSize, (frame.Header.Bits_Per_Sample + 7) / 8))
						return Flac__StreamDecoderWriteStatus.Abort;
				}

				return decoder.Private.Write_Callback(this, frame, buffer, decoder.Private.Client_Data);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Send_Error_To_Client(Flac__StreamDecoderErrorStatus status)
		{
			if (!decoder.Private.Is_Seeking)
				decoder.Private.Error_Callback(this, status, decoder.Private.Client_Data);
			else if (status == Flac__StreamDecoderErrorStatus.Unparseable_Stream)
				decoder.Private.Unparseable_Frame_Count++;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Seek_To_Absolute_Sample(Flac__uint64 stream_Length, Flac__uint64 target_Sample)
		{
			Flac__uint64 first_Frame_Offset = decoder.Private.First_Frame_Offset;
			Flac__bool first_Seek = true;
			Flac__uint64 total_Samples = Flac__Stream_Decoder_Get_Total_Samples();
			uint32_t min_BlockSize = ((Flac__StreamMetadata_StreamInfo)decoder.Private.Stream_Info.Data).Min_BlockSize;
			uint32_t max_BlockSize = ((Flac__StreamMetadata_StreamInfo)decoder.Private.Stream_Info.Data).Max_BlockSize;
			uint32_t max_FrameSize = ((Flac__StreamMetadata_StreamInfo)decoder.Private.Stream_Info.Data).Max_FrameSize;
			uint32_t min_FrameSize = ((Flac__StreamMetadata_StreamInfo)decoder.Private.Stream_Info.Data).Min_FrameSize;

			// Take these from the current frame in case they've changed mid-stream
			uint32_t channels = Flac__Stream_Decoder_Get_Channels();
			uint32_t bps = Flac__Stream_Decoder_Get_Bits_Per_Sample();
			Flac__StreamMetadata_SeekTable seek_Table = decoder.Private.Has_Seek_Table ? (Flac__StreamMetadata_SeekTable)decoder.Private.Seek_Table.Data : null;

			// Use values from stream info if we didn't decode a frame
			if (channels == 0)
				channels = ((Flac__StreamMetadata_StreamInfo)decoder.Private.Stream_Info.Data).Channels;

			if (bps == 0)
				bps = ((Flac__StreamMetadata_StreamInfo)decoder.Private.Stream_Info.Data).Bits_Per_Sample;

			// We are just guessing here
			uint32_t approx_Bytes_Per_Frame;

			if (max_FrameSize > 0)
				approx_Bytes_Per_Frame = (max_FrameSize + min_FrameSize) / 2 + 1;
			// Check if it's a know fixed-blocksize stream. Note that though
			// the spec doesn't allow zeroes in the STREAMINFO block, we may
			// never get a STREAMINFO block when decoding so the value of
			// min_BlockSize might be zero
			else if ((min_BlockSize == max_BlockSize) && (min_BlockSize > 0))
			{
				// Note there are no () around 'bps/8' to keep precision up since it's an integer calculation
				approx_Bytes_Per_Frame = min_BlockSize * channels * bps / 8 + 64;
			}
			else
				approx_Bytes_Per_Frame = 4096 * channels * bps / 8 + 64;

			// First, we set an upper and lower bound on where in the
			// stream we will search. For now we take the current position
			// as one bound and, depending on where the target position lies,
			// the beginning of the first frame or the end of the stream as
			// the other bound
			Flac__uint64 lower_Bound = first_Frame_Offset;
			Flac__uint64 lower_Bound_Sample = 0;
			Flac__uint64 upper_Bound = stream_Length;
			Flac__uint64 upper_Bound_Sample = total_Samples > 0 ? total_Samples : target_Sample;	// Estimate it

			if (decoder.Protected.State == Flac__StreamDecoderState.Read_Frame)
			{
				if (target_Sample < decoder.Private.Samples_Decoded)
				{
					if (Flac__Stream_Decoder_Get_Decode_Position(out upper_Bound))
						upper_Bound_Sample = decoder.Private.Samples_Decoded;
				}
				else
				{
					if (Flac__Stream_Decoder_Get_Decode_Position(out lower_Bound))
						lower_Bound_Sample = decoder.Private.Samples_Decoded;
				}
			}

			// Now we refine the bounds if we have a seektable with
			// suitable points. Note that according to the spec they
			// must be ordered by ascending sample number.
			//
			// Note: to protect against invalid seek tables we will ignore points
			// that have frame_Samples==0 or sample_Number>=total_Samples
			if (seek_Table != null)
			{
				Flac__uint64 new_Lower_Bound = lower_Bound;
				Flac__uint64 new_Upper_Bound = upper_Bound;
				Flac__uint64 new_Lower_Bound_Sample = lower_Bound_Sample;
				Flac__uint64 new_Upper_Bound_Sample = upper_Bound_Sample;

				// Find the closest seek point <= target_Sample, if it exists
				int i;
				for (i = (int)seek_Table.Num_Points - 1; i >= 0; i--)
				{
					if ((seek_Table.Points[i].Sample_Number != Constants.Flac__Stream_Metadata_SeekPoint_Placeholder) && (seek_Table.Points[i].Frame_Samples > 0) && ((total_Samples <= 0) || (seek_Table.Points[i].Sample_Number < total_Samples)) && seek_Table.Points[i].Sample_Number <= target_Sample)
						break;
				}

				if (i >= 0)		// I.e. we found a suitable seek point
				{
					new_Lower_Bound = first_Frame_Offset + seek_Table.Points[i].Stream_Offset;
					new_Lower_Bound_Sample = seek_Table.Points[i].Sample_Number;
				}

				// Find the closest seek point > target_Sample, if it exists
				for (i = 0; i < (int)seek_Table.Num_Points; i++)
				{
					if ((seek_Table.Points[i].Sample_Number != Constants.Flac__Stream_Metadata_SeekPoint_Placeholder) && (seek_Table.Points[i].Frame_Samples > 0) && ((total_Samples <= 0) || (seek_Table.Points[i].Sample_Number < total_Samples)) && seek_Table.Points[i].Sample_Number > target_Sample)
						break;
				}

				if (i < seek_Table.Num_Points)	// I.e. we found a suitable seek point
				{
					new_Upper_Bound = first_Frame_Offset + seek_Table.Points[i].Stream_Offset;
					new_Upper_Bound_Sample = seek_Table.Points[i].Sample_Number;
				}

				// Final protection against unsorted seek tables; keep original values if bogus
				if (new_Upper_Bound >= new_Lower_Bound)
				{
					lower_Bound = new_Lower_Bound;
					upper_Bound = new_Upper_Bound;
					lower_Bound_Sample = new_Lower_Bound_Sample;
					upper_Bound_Sample = new_Upper_Bound_Sample;
				}
			}

			Debug.Assert(upper_Bound_Sample >= lower_Bound_Sample);

			// There are 2 insidious ways that the following equality occurs, which we need to fix:
			//  1) total_Samples is 0 (unknown) and target_Sample is 0
			//  2) total_Samples is 0 (unknown) and target_Sample happens to be
			//     exactly equal to the last seek point in the seek table; this
			//     means there is no seek point above it, and upper_Bound_Samples
			//     remains equal to the estimate (of target_Samples) we made above
			// In either case it does not hurt to move upper_Bound_Sample up by 1
			if (upper_Bound_Sample == lower_Bound_Sample)
				upper_Bound_Sample++;

			decoder.Private.Target_Sample = target_Sample;

			while (true)
			{
				// Check if the bounds are still ok
				if ((lower_Bound_Sample >= upper_Bound_Sample) || (lower_Bound > upper_Bound))
				{
					decoder.Protected.State = Flac__StreamDecoderState.Seek_Error;
					return false;
				}

				Flac__int64 pos = (Flac__int64)lower_Bound + (Flac__int64)((double)(target_Sample - lower_Bound_Sample) / (upper_Bound_Sample - lower_Bound_Sample) * (upper_Bound - lower_Bound)) - approx_Bytes_Per_Frame;

				if (pos >= (Flac__int64)upper_Bound)
					pos = (Flac__int64)upper_Bound - 1;

				if (pos < (Flac__int64)lower_Bound)
					pos = (Flac__int64)lower_Bound;

				if (decoder.Private.Seek_Callback(this, (Flac__uint64)pos, decoder.Private.Client_Data) != Flac__StreamDecoderSeekStatus.Ok)
				{
					decoder.Protected.State = Flac__StreamDecoderState.Seek_Error;
					return false;
				}

				if (!Flac__Stream_Decoder_Flush())
				{
					// Above call sets the state for us
					return false;
				}

				// Now we need to get a frame. First we need to reset our
				// unparseable_Frame_Count; if we get too many unparseable
				// frames in a row, the read callback will return
				// FLAC__STREAM_DECODER_READ_STATUS_ABORT, causing
				// Flac__Stream_Decoder_Process_Single() to return false
				decoder.Private.Unparseable_Frame_Count = 0;

				if (!Flac__Stream_Decoder_Process_Single() || (decoder.Protected.State == Flac__StreamDecoderState.Aborted))
				{
					decoder.Protected.State = Flac__StreamDecoderState.Seek_Error;
					return false;
				}

				if (!decoder.Private.Is_Seeking)
					break;

				Debug.Assert(decoder.Private.Last_Frame.Header.Number_Type == Flac__FrameNumberType.Sample_Number);
				Flac__uint64 this_Frame_Sample = decoder.Private.Last_Frame.Header.Sample_Number;

				if ((decoder.Private.Samples_Decoded == 0) || ((this_Frame_Sample + decoder.Private.Last_Frame.Header.BlockSize >= upper_Bound_Sample) && !first_Seek))
				{
					if (pos == (Flac__int64)lower_Bound)
					{
						// Can't move back any more than the first frame, something is fatally wrong
						decoder.Protected.State = Flac__StreamDecoderState.Seek_Error;
						return false;
					}

					// Our last move backwards wasn't big enough, try again
					approx_Bytes_Per_Frame = approx_Bytes_Per_Frame != 0 ? approx_Bytes_Per_Frame * 2 : 16;
					continue;
				}

				// Allow one seek over upper bound, so we can get a correct upper_Bound_Sample for streams with unknown total_Samples
				first_Seek = false;

				// Make sure we are not seeking in corrupted streams
				if (this_Frame_Sample < lower_Bound_Sample)
				{
					decoder.Protected.State = Flac__StreamDecoderState.Seek_Error;
					return false;
				}

				// We need to narrow the search
				if (target_Sample < this_Frame_Sample)
				{
					upper_Bound_Sample = this_Frame_Sample + decoder.Private.Last_Frame.Header.BlockSize;

					// @@@@@@ What will decode position be if at end of stream?
					if (!Flac__Stream_Decoder_Get_Decode_Position(out upper_Bound))
					{
						decoder.Protected.State = Flac__StreamDecoderState.Seek_Error;
						return false;
					}

					approx_Bytes_Per_Frame = (uint32_t)(2 * (upper_Bound - (uint64_t)pos) / 3 + 16);
				}
				else	// target_Sample >= this_Frame_Sample + this frame's blocksize
				{
					lower_Bound_Sample = this_Frame_Sample + decoder.Private.Last_Frame.Header.BlockSize;

					if (!Flac__Stream_Decoder_Get_Decode_Position(out lower_Bound))
					{
						decoder.Protected.State = Flac__StreamDecoderState.Seek_Error;
						return false;
					}

					approx_Bytes_Per_Frame = (uint32_t)(2 * (lower_Bound - (uint64_t)pos) / 3 + 16);
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__StreamDecoderReadStatus File_Read_Callback(Stream_Decoder streamDecoder, Span<Flac__byte> buffer, ref size_t bytes, object _)
		{
			Flac__StreamDecoder decoder = streamDecoder.decoder;

			if (bytes > 0)
			{
				try
				{
					bytes = (size_t)decoder.Private.File.Read(buffer.Slice(0, (int)bytes));
					if (bytes == 0)
						return Flac__StreamDecoderReadStatus.End_Of_Stream;
					else
						return Flac__StreamDecoderReadStatus.Continue;
				}
				catch(IOException)
				{
					return Flac__StreamDecoderReadStatus.Abort;
				}
			}
			else
				return Flac__StreamDecoderReadStatus.Abort;	// Abort to avoid a deadlock
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__StreamDecoderSeekStatus File_Seek_Callback(Stream_Decoder streamDecoder, Flac__uint64 absolute_Byte_Offset, object _)
		{
			Flac__StreamDecoder decoder = streamDecoder.decoder;

			try
			{
				decoder.Private.File.Seek((long)absolute_Byte_Offset, SeekOrigin.Begin);

				return Flac__StreamDecoderSeekStatus.Ok;
			}
			catch(NotSupportedException)
			{
				return Flac__StreamDecoderSeekStatus.Unsupported;
			}
			catch(IOException)
			{
				return Flac__StreamDecoderSeekStatus.Error;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__StreamDecoderTellStatus File_Tell_Callback(Stream_Decoder streamDecoder, out Flac__uint64 absolute_Byte_Offset, object _)
		{
			Flac__StreamDecoder decoder = streamDecoder.decoder;

			try
			{
				absolute_Byte_Offset = (Flac__uint64)decoder.Private.File.Position;

				return Flac__StreamDecoderTellStatus.Ok;
			}
			catch(NotSupportedException)
			{
				absolute_Byte_Offset = 0;
				return Flac__StreamDecoderTellStatus.Unsupported;
			}
			catch(IOException)
			{
				absolute_Byte_Offset = 0;
				return Flac__StreamDecoderTellStatus.Error;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__StreamDecoderLengthStatus File_Length_Callback(Stream_Decoder streamDecoder, out Flac__uint64 stream_Length, object _)
		{
			Flac__StreamDecoder decoder = streamDecoder.decoder;

			try
			{
				stream_Length = (Flac__uint64)decoder.Private.File.Length;

				return Flac__StreamDecoderLengthStatus.Ok;
			}
			catch(NotSupportedException)
			{
				stream_Length = 0;
				return Flac__StreamDecoderLengthStatus.Unsupported;
			}
			catch(IOException)
			{
				stream_Length = 0;
				return Flac__StreamDecoderLengthStatus.Error;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__bool File_Eof_Callback(Stream_Decoder streamDecoder, object _)
		{
			Flac__StreamDecoder decoder = streamDecoder.decoder;

			try
			{
				return decoder.Private.File.Position >= decoder.Private.File.Length;
			}
			catch(NotSupportedException)
			{
				return false;
			}
		}
		#endregion
	}
}
