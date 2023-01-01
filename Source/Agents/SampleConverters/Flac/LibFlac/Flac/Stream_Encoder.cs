/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Diagnostics;
using System.Text;
using Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers;
using Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Private;
using Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Protected;
using Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Share;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac
{
	/// <summary>
	/// This module contains the functions which implement the stream
	/// encoder.
	///
	/// The stream encoder can be used to encode complete streams either to the
	/// client via callbacks, or directly to a file, depending on how it is
	/// initialized. When encoding via callbacks, the client provides a write
	/// callback which will be called whenever FLAC data is ready to be written.
	/// If the client also supplies a seek callback, the encoder will also
	/// automatically handle the writing back of metadata discovered while
	/// encoding, like stream info, seek points offsets, etc. When encoding to
	/// a file, the client needs only supply a filename or open FILE* and an
	/// optional progress callback for periodic notification of progress; the
	/// write and seek callbacks are supplied internally.
	///
	/// The stream encoder can encode to native FLAC.
	///
	/// The basic usage of this encoder is as follows:
	///  - The program creates an instance of an encoder using
	///    Flac__Stream_Encoder_New().
	///  - The program overrides the default settings using
	///    Flac__Stream_Encoder_Set_*() functions. At a minimum, the following
	///    functions should be called:
	///    - Flac__Stream_Encoder_Set_Channels()
	///    - Flac__Stream_Encoder_Set_Bits_Per_Sample()
	///    - Flac__Stream_Encoder_Set_Sample_Rate()
	///    - Flac__Stream_Encoder_Set_Total_Samples_Estimate() (if known)
	///  - If the application wants to control the compression level or set its own
	///    metadata, then the following should also be called:
	///    - Flac__Stream_Encoder_Set_Compression_Level()
	///    - Flac__Stream_Encoder_Set_Verify()
	///    - Flac__Stream_Encoder_Set_Metadata()
	///  - The rest of the set functions should only be called if the client needs
	///    exact control over how the audio is compressed; thorough understanding
	///    of the FLAC format is necessary to achieve good results.
	///  - The program initializes the instance to validate the settings and
	///    prepare for encoding using
	///    - Flac__Stream_Encoder_Init_Stream() or Flac__Stream_Encoder_Init_File()
	///      for native FLAC
	///  - The program calls Flac__Stream_Encoder_Process() or
	///    Flac__Stream_Encoder_Process_Interleaved() to encode data, which
	///    subsequently calls the callbacks when there is encoder data ready
	///    to be written.
	///  - The program finishes the encoding with Flac__Stream_Encoder_Finish(),
	///    which causes the encoder to encode any data still in its input pipe,
	///    update the metadata with the final encoding statistics if output
	///    seeking is possible, and finally reset the encoder to the
	///    uninitialized state.
	///  - The instance may be used again or deleted with
	///    Flac__Stream_Encoder_Delete().
	///
	/// In more detail, the stream encoder functions similarly to the
	/// Flac_Stream_Decoder stream decoder, but has fewer
	/// callbacks and more options. Typically the client will create a new
	/// instance by calling Flac__Stream_Encoder_New(), then set the necessary
	/// parameters with Flac__Stream_Encoder_set_*(), and initialize it by
	/// calling one of the Flac__Stream_Encoder_Init_*() functions.
	///
	/// Unlike the decoders, the stream encoder has many options that can
	/// affect the speed and compression ratio. When setting these parameters
	/// you should have some basic knowledge of the format. The
	/// Flac__Stream_Encoder_Set_*() functions themselves do not validate the
	/// values as many are interdependent. The Flac__Stream_Encoder_Init_*()
	/// functions will do this, so make sure to pay attention to the state
	/// returned by Flac__Stream_Encoder_Init_*() to make sure that it is
	/// FLAC__STREAM_ENCODER_INIT_STATUS_OK. Any parameters that are not set
	/// before Flac__Stream_Encoder_Init_*() will take on the defaults from
	/// the constructor.
	///
	/// There are three initialization functions for native FLAC, one for
	/// setting up the encoder to encode FLAC data to the client via
	/// callbacks, and two for encoding directly to a file.
	///
	/// For encoding via callbacks, use Flac__Stream_Encoder_Init_Stream().
	/// You must also supply a write callback which will be called anytime
	/// there is raw encoded data to write. If the client can seek the output
	/// it is best to also supply seek and tell callbacks, as this allows the
	/// encoder to go back after encoding is finished to write back
	/// information that was collected while encoding, like seek point offsets,
	/// frame sizes, etc.
	///
	/// For encoding directly to a file, use Flac__Stream_Encoder_Init_File()
	/// or Flac__Stream_Encoder_Init_File() with a .NET stream. Then you must
	/// only supply a filename or an open Stream; the encoder will handle all
	/// the callbacks internally. You may also supply a progress callback for
	/// periodic notification of the encoding progress.
	///
	/// The call to Flac__Stream_Encoder_Init_*() currently will also immediately
	/// call the write callback several times, once with the fLaC signature,
	/// and once for each encoded metadata block.
	///
	/// After initializing the instance, the client may feed audio data to the
	/// encoder in one of two ways:
	///
	/// - Channel separate, through Flac__Stream_Encoder_Process() - The client
	///   will pass an array of pointers to buffers, one for each channel, to
	///   the encoder, each of the same length. The samples need not be
	///   block-aligned, but each channel should have the same number of samples.
	/// - Channel interleaved, through
	///   Flac__Stream_Encoder_Process_Interleaved() - The client will pass a single
	///   pointer to data that is channel-interleaved (i.e. channel0_sample0,
	///   channel1_sample0, ... , channelN_sample0, channel0_sample1, ...).
	///   Again, the samples need not be block-aligned but they must be
	///   sample-aligned, i.e. the first value should be channel0_sample0 and
	///   the last value channelN_sampleM.
	///
	/// Note that for either process call, each sample in the buffers should be a
	/// signed integer, right-justified to the resolution set by
	/// Flac__Stream_Encoder_Set_Bits_Per_Sample(). For example, if the resolution
	/// is 16 bits per sample, the samples should all be in the range [-32768,32767].
	///
	/// When the client is finished encoding data, it calls
	/// Flac__Stream_Encoder_Finish(), which causes the encoder to encode any
	/// data still in its input pipe, and call the metadata callback with the
	/// final encoding statistics. Then the instance may be deleted with
	/// Flac__Stream_Encoder_Delete() or initialized again to encode another
	/// stream.
	///
	/// For programs that write their own metadata, but that do not know the
	/// actual metadata until after encoding, it is advantageous to instruct
	/// the encoder to write a PADDING block of the correct size, so that
	/// instead of rewriting the whole stream after encoding, the program can
	/// just overwrite the PADDING block. If only the maximum size of the
	/// metadata is known, the program can write a slightly larger padding
	/// block, then split it after encoding.
	///
	/// Make sure you understand how lengths are calculated. All FLAC metadata
	/// blocks have a 4 byte header which contains the type and length. This
	/// length does not include the 4 bytes of the header. See the format page
	/// for the specification of metadata blocks and their lengths.
	///
	/// NOTE:
	/// The "set" functions may only be called when the encoder is in the
	/// state FLAC__STREAM_ENCODER_UNINITIALIZED, i.e. after
	/// Flac__Stream_Encoder_New() or Flac__Stream_Encoder_Finish(), but
	/// before Flac__Stream_Encoder_Init_*(). If this is the case they will
	/// return true, otherwise false.
	///
	/// NOTE:
	/// Flac__Stream_Encoder_Finish() resets all settings to the constructor
	/// defaults.
	/// </summary>
	internal class Stream_Encoder
	{
		#region Delegates
		// NOTE: In general, Flac__StreamEncoder functions which change the
		// state should not be called on the encoder while in the callback

		/********************************************************************/
		/// <summary>
		/// This delegate will be called by the encoder anytime there is raw
		/// encoded data ready to write. It may include metadata mixed with
		/// encoded audio frames and the data is not guaranteed to be aligned
		/// on frame or metadata block boundaries.
		///
		/// The only duty of the callback is to write out the bytes worth of
		/// data in buffer to the current position in the output stream.
		/// The arguments samples and current_Frame are purely informational.
		/// If samples is greater than 0, then current_Frame will hold the
		/// current frame number that is being written; otherwise it indicates
		/// that the write callback is being called to write metadata
		/// </summary>
		/// <param name="encoder">The encoder instance calling the callback</param>
		/// <param name="buffer">An array of encoded data of length bytes.</param>
		/// <param name="bytes">The byte length of buffer</param>
		/// <param name="samples">The number of samples encoded by buffer. 0 has a special meaning; see above</param>
		/// <param name="current_Frame">The number of the current frame being encoded</param>
		/// <param name="client_Data">The callee's client data set through Flac__Stream_Encoder_Init_*()</param>
		/// <returns>The callee's return status</returns>
		/********************************************************************/
		public delegate Flac__StreamEncoderWriteStatus Flac__StreamEncoderWriteCallback(Stream_Encoder encoder, Span<Flac__byte> buffer, size_t bytes, uint32_t samples, uint32_t current_Frame, object client_Data);



		/********************************************************************/
		/// <summary>
		/// This delegate will be called when the encoder needs to seek the
		/// output stream. The encoder will pass the absolute byte offset to
		/// seek to, 0 meaning the beginning of the stream
		/// </summary>
		/// <param name="encoder">The encoder instance calling the callback</param>
		/// <param name="absolute_Byte_Offset">The offset from the beginning of the stream to seek to</param>
		/// <param name="client_Data">The callee's client data set through Flac__Stream_Encoder_Init_*()</param>
		/// <returns>The callee's return status</returns>
		/********************************************************************/
		public delegate Flac__StreamEncoderSeekStatus Flac__StreamEncoderSeekCallback(Stream_Encoder encoder, Flac__uint64 absolute_Byte_Offset, object client_Data);



		/********************************************************************/
		/// <summary>
		/// This delegate will be called when the encoder needs to know the
		/// current position of the output stream.
		///
		/// Warning:
		/// The callback must return the true current byte offset of the
		/// output to which the encoder is writing. If you are buffering the
		/// output, make sure and take this into account
		/// </summary>
		/// <param name="encoder">The encoder instance calling the callback</param>
		/// <param name="absolute_Byte_Offset">Store the current offset from the beginning of the steam here</param>
		/// <param name="client_Data">The callee's client data set through Flac__Stream_Encoder_Init_*()</param>
		/// <returns>The callee's return status</returns>
		/********************************************************************/
		public delegate Flac__StreamEncoderTellStatus Flac__StreamEncoderTellCallback(Stream_Encoder encoder, out Flac__uint64 absolute_Byte_Offset, object client_Data);



		/********************************************************************/
		/// <summary>
		/// This delegate will be called once at the end of encoding with
		/// the populated STREAMINFO structure. This is so the client can
		/// seek back to the beginning of the file and write the STREAMINFO
		/// block with the correct statistics after encoding (like
		/// minimum/maximum frame size and total samples)
		/// </summary>
		/// <param name="encoder">The encoder instance calling the callback</param>
		/// <param name="metadata">The final populated STREAMINFO block</param>
		/// <param name="client_Data">The callee's client data set through Flac__Stream_Encoder_Init_*()</param>
		/********************************************************************/
		public delegate void Flac__StreamEncoderMetadataCallback(Stream_Encoder encoder, Flac__StreamMetadata metadata, object client_Data);



		/********************************************************************/
		/// <summary>
		/// This delegate will be called when the encoder has finished
		/// writing a frame. The total_Frames_Estimate argument to the
		/// callback will be based on the value from
		/// Flac__Stream_Encoder_Set_Total_Samples_Estimate()
		/// </summary>
		/// <param name="encoder">The encoder instance calling the callback</param>
		/// <param name="bytes_Written">Bytes written so far</param>
		/// <param name="samples_Written">Samples written so far</param>
		/// <param name="frames_Written">Frames written so far</param>
		/// <param name="total_Frames_Estimates">The estimate of the total number of frames to be written</param>
		/// <param name="client_Data">The callee's client data set through Flac__Stream_Encoder_Init_*()</param>
		/********************************************************************/
		public delegate void Flac__StreamEncoderProgressCallback(Stream_Encoder encoder, Flac__uint64 bytes_Written, Flac__uint64 samples_Written, uint32_t frames_Written, uint32_t total_Frames_Estimates, object client_Data);
		#endregion

		private class CompressionLevels
		{
			public CompressionLevels(Flac__bool do_Mid_Side_Stereo, Flac__bool loose_Mid_Side_Stereo, uint32_t max_Lpc_Order, uint32_t qlp_Coeff_Precision, Flac__bool do_Qlp_Coeff_Prec_Search, Flac__bool do_Exhaustive_Model_Search, uint32_t min_Residual_Partition_Order, uint32_t max_Residual_Partition_Order, string apodization)
			{
				Do_Mid_Side_Stereo = do_Mid_Side_Stereo;
				Loose_Mid_Side_Stereo = loose_Mid_Side_Stereo;
				Max_Lpc_Order = max_Lpc_Order;
				Qlp_Coeff_Precision = qlp_Coeff_Precision;
				Do_Qlp_Coeff_Prec_Search = do_Qlp_Coeff_Prec_Search;
				Do_Exhaustive_Model_Search = do_Exhaustive_Model_Search;
				Min_Residual_Partition_Order = min_Residual_Partition_Order;
				Max_Residual_Partition_Order = max_Residual_Partition_Order;
				Apodization = apodization;
			}

			public Flac__bool Do_Mid_Side_Stereo;
			public Flac__bool Loose_Mid_Side_Stereo;
			public uint32_t Max_Lpc_Order;
			public uint32_t Qlp_Coeff_Precision;
			public Flac__bool Do_Qlp_Coeff_Prec_Search;
			public Flac__bool Do_Exhaustive_Model_Search;
			public uint32_t Min_Residual_Partition_Order;
			public uint32_t Max_Residual_Partition_Order;
			public string Apodization;
		}

		private class Flac__StreamEncoder
		{
			public Flac__StreamEncoderProtected Protected;
			public Flac__StreamEncoderPrivate Private;
		}

		private class Flac__StreamEncoderPrivate
		{
			public uint32_t Input_Capacity;																// Current size (in samples) of the signal and residual buffers
			public Flac__int32[][] Integer_Signal = new Flac__int32[Constants.Flac__Max_Channels][];	// The integer version of the input signal
			public Flac__int32[][] Integer_Signal_Mid_Size = new Flac__int32[2][];						// The integer version of the mid-side input signal (stereo only)
			public Flac__real[][] Window = new Flac__real[Constants.Flac__Max_Apodization_Functions][];	// The pre-computed floating-point window for each apodization function
			public Flac__real[] Windowed_Signal;														// The Integer_Signal[] * current Window[]
			public uint32_t[] SubFrame_Bps = new uint32_t[Constants.Flac__Max_Channels];				// The effective bits per sample of the input signal (stream bps - wasted bits)
			public uint32_t[] SubFrame_Bps_Mid_Side = new uint32_t[2];									// The effective bits per sample of the mid-side input signal (stream bps - wasted bits + 0/1)
			public Flac__int32[][][] Residual_Workspace = Helpers.InitializeArrayWithArray<Flac__int32>((int)Constants.Flac__Max_Channels, 2);// Each channel has a candidate and best workspace where the subframe residual signals will be stored
			public Flac__int32[][][] Residual_Workspace_Mid_Side = Helpers.InitializeArrayWithArray<Flac__int32>(2, 2);
			public Flac__SubFrame[][] SubFrame_Workspace = Helpers.InitializeArray<Flac__SubFrame>((int)Constants.Flac__Max_Channels, 2);
			public Flac__SubFrame[][] SubFrame_Workspace_Mid_Side = Helpers.InitializeArray<Flac__SubFrame>(2, 2);
			public Flac__SubFrame[][] SubFrame_Workspace_Ptr = Helpers.Initialize2Arrays<Flac__SubFrame>((int)Constants.Flac__Max_Channels, 2);
			public Flac__SubFrame[][] SubFrame_Workspace_Ptr_Mid_Side = Helpers.Initialize2Arrays<Flac__SubFrame>(2, 2);
			public Flac__EntropyCodingMethod_PartitionedRiceContents[][] Partitioned_Rice_Contents_Workspace = Helpers.InitializeArray<Flac__EntropyCodingMethod_PartitionedRiceContents>((int)Constants.Flac__Max_Channels, 2);
			public Flac__EntropyCodingMethod_PartitionedRiceContents[][] Partitioned_Rice_Contents_Workspace_Mid_Side = Helpers.InitializeArray<Flac__EntropyCodingMethod_PartitionedRiceContents>((int)Constants.Flac__Max_Channels, 2);
			public Flac__EntropyCodingMethod_PartitionedRiceContents[][] Partitioned_Rice_Contents_Workspace_Ptr = Helpers.Initialize2Arrays<Flac__EntropyCodingMethod_PartitionedRiceContents>((int)Constants.Flac__Max_Channels, 2);
			public Flac__EntropyCodingMethod_PartitionedRiceContents[][] Partitioned_Rice_Contents_Workspace_Ptr_Mid_Side = Helpers.Initialize2Arrays<Flac__EntropyCodingMethod_PartitionedRiceContents>((int)Constants.Flac__Max_Channels, 2);
			public uint32_t[] Best_SubFrame = new uint32_t[Constants.Flac__Max_Channels];				// Index (0 or 1) into 2nd dimension of the above workspaces
			public uint32_t[] Best_SubFrame_Mid_Side = new uint32_t[2];
			public uint32_t[] Best_SubFrame_Bits = new uint32_t[Constants.Flac__Max_Channels];			// Size in bits of the best subframe for each channel
			public uint32_t[] Best_SubFrame_Bits_Mid_Side = new uint32_t[2];
			public Flac__uint64[] Abs_Residual_Partition_Sums;											// Workspace where the sum of abs(candidate residual) for each partition is stored
			public uint32_t[] Raw_Bits_Per_Partition;													// Workspace where the sum of silog2(candidate residual) for each partition is stored
			public BitWriter Frame;																		// The current frame being worked on
			public uint32_t Loose_Mid_Side_Stereo_Frames;												// Rounded number of frames the encoder will use before trying both independent and mid/side frames again
			public uint32_t Loose_Mid_Side_Stereo_Frame_Count;											// Number of frames using the current channel assignment
			public Flac__ChannelAssignment Last_Channel_Assignment;
			public Flac__StreamMetadata StreamInfo;														// Scratchpad for STREAMINFO as it is built
			public Flac__StreamMetadata_SeekTable Seek_Table;											// Pointer into encoder.Protected.Metadata where the seek table is
			public uint32_t Current_Sample_Number;
			public uint32_t Current_Frame_Number;
			public Md5 Md5;
			public IFixed Fixed;
			public ILpc Lpc;
			public Flac__bool Disable_Constant_SubFrames;
			public Flac__bool Disable_Fixed_SubFrames;
			public Flac__bool Disable_Verbatim_SubFrames;
			public Flac__StreamEncoderSeekCallback Seek_Callback;
			public Flac__StreamEncoderTellCallback Tell_Callback;
			public Flac__StreamEncoderWriteCallback Write_Callback;
			public Flac__StreamEncoderMetadataCallback Metadata_Callback;
			public Flac__StreamEncoderProgressCallback Progress_Callback;
			public object Client_Data;
			public uint32_t First_SeekPoint_To_Check;
			public Stream File;																			// Only used when encoding to a file
			public bool Leave_Stream_Open;
			public Flac__uint64 Bytes_Written;
			public Flac__uint64 Samples_Written;
			public uint32_t Frames_Written;
			public uint32_t Total_Frames_Estimate;
			public Flac__int32[][] Integer_Signal_Unaligned = new Flac__int32[Constants.Flac__Max_Channels][];
			public Flac__int32[][] Integer_Signal_Mid_Side_Unaligned = new Flac__int32[2][];
			public Flac__real[][] Window_Unaligned = new Flac__real[Constants.Flac__Max_Apodization_Functions][];
			public Flac__real[] Windowed_Signal_Unaligned;
			public Flac__int32[][][] Residual_Workspace_Unaligned = Helpers.InitializeArrayWithArray<Flac__int32>((int)Constants.Flac__Max_Channels, 2);
			public Flac__int32[][][] Residual_Workspace_Mid_Side_Unaligned = Helpers.InitializeArrayWithArray<Flac__int32>(2, 2);
			public Flac__uint64[] Abs_Residual_Partition_Sums_Unaligned;
			public Flac__uint64[] Raw_Bits_Per_Partition_Unaligned;
			public Flac__real[][] Lp_Coeff = Helpers.InitializeArray<Flac__real>((int)Constants.Flac__Max_Lpc_Order, (int)Constants.Flac__Max_Lpc_Order);	// From Process_SubFrame()
			public Flac__EntropyCodingMethod_PartitionedRiceContents[] Partitioned_Rice_Contents_Extra = Helpers.InitializeArray<Flac__EntropyCodingMethod_PartitionedRiceContents>(2);	// From Find_Best_Partition_Order()
			public Flac__bool Is_Being_Deleted;		// If true, call to ..._Finish() from ..._Delete() will not call the callbacks
		}

		private static readonly CompressionLevels[] compression_Levels =
		{
			new CompressionLevels(false, false, 0, 0, false, false, 0, 3, "tukey(5e-1)"),
			new CompressionLevels(true, true, 0, 0, false, false, 0, 3, "tukey(5e-1)"),
			new CompressionLevels(true, false, 0, 0, false, false, 0, 3, "tukey(5e-1)"),
			new CompressionLevels(false, false, 6, 0, false, false, 0, 4, "tukey(5e-1)"),
			new CompressionLevels(true, true, 8, 0, false, false, 0, 4, "tukey(5e-1)"),
			new CompressionLevels(true, false, 8, 0, false, false, 0, 5, "tukey(5e-1)"),
			new CompressionLevels(true, false, 8, 0, false, false, 0, 6, "tukey(5e-1);partial_tukey(2)"),
			new CompressionLevels(true, false, 12, 0, false, false, 0, 6, "tukey(5e-1);partial_tukey(2)"),
			new CompressionLevels(true, false, 12, 0, false, false, 0, 6, "tukey(5e-1);partial_tukey(2);punchout_tukey(3)")
		};

		// Number of samples that will be overread to watch for end of stream. By
		// 'overread', we mean that the Flac__Stream_Encoder_Process*() calls will
		// always try to read blockSize+1 samples before encoding a block, so that
		// even if the stream has a total sample count that is an integral multiple
		// of the blockSize, we will still notice when we are encoding the last
		// block.
		//
		// WATCHOUT: Some parts of the code assert that Overread == 1 and there's
		// not really any reason to change it
		private const uint32_t Overread = 1;

		private Flac__StreamEncoder encoder;

		#region Constructor/destructor
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Stream_Encoder()
		{
			encoder = new Flac__StreamEncoder();
			encoder.Protected = new Flac__StreamEncoderProtected();
			encoder.Private = new Flac__StreamEncoderPrivate();
		}



		/********************************************************************/
		/// <summary>
		/// Create a new stream encoder instance. The instance is created
		/// with default settings; see the individual
		/// Flac__Stream_Encoder_Set_*() functions for each setting's default
		/// </summary>
		/********************************************************************/
		public static Stream_Encoder Flac__Stream_Encoder_New()
		{
			Stream_Encoder encoder = new Stream_Encoder();

			encoder.encoder.Private.Frame = BitWriter.Flac__BitWriter_New();
			if (encoder.encoder.Private.Frame == null)
				return null;

			encoder.encoder.Private.File = null;

			encoder.encoder.Protected.State = Flac__StreamEncoderState.Uninitialized;

			Set_Defaults(encoder);

			encoder.encoder.Private.Is_Being_Deleted = false;

			for (uint32_t i = 0; i < Constants.Flac__Max_Channels; i++)
			{
				encoder.encoder.Private.SubFrame_Workspace_Ptr[i][0] = encoder.encoder.Private.SubFrame_Workspace[i][0];
				encoder.encoder.Private.SubFrame_Workspace_Ptr[i][1] = encoder.encoder.Private.SubFrame_Workspace[i][1];
			}

			for (uint32_t i = 0; i < 2; i++)
			{
				encoder.encoder.Private.SubFrame_Workspace_Ptr_Mid_Side[i][0] = encoder.encoder.Private.SubFrame_Workspace_Mid_Side[i][0];
				encoder.encoder.Private.SubFrame_Workspace_Ptr_Mid_Side[i][1] = encoder.encoder.Private.SubFrame_Workspace_Mid_Side[i][1];
			}

			for (uint32_t i = 0; i < Constants.Flac__Max_Channels; i++)
			{
				encoder.encoder.Private.Partitioned_Rice_Contents_Workspace_Ptr[i][0] = encoder.encoder.Private.Partitioned_Rice_Contents_Workspace[i][0];
				encoder.encoder.Private.Partitioned_Rice_Contents_Workspace_Ptr[i][1] = encoder.encoder.Private.Partitioned_Rice_Contents_Workspace[i][1];
			}

			for (uint32_t i = 0; i < 2; i++)
			{
				encoder.encoder.Private.Partitioned_Rice_Contents_Workspace_Ptr_Mid_Side[i][0] = encoder.encoder.Private.Partitioned_Rice_Contents_Workspace_Mid_Side[i][0];
				encoder.encoder.Private.Partitioned_Rice_Contents_Workspace_Ptr_Mid_Side[i][1] = encoder.encoder.Private.Partitioned_Rice_Contents_Workspace_Mid_Side[i][1];
			}

			for (uint32_t i = 0; i < Constants.Flac__Max_Channels; i++)
			{
				Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Init(encoder.encoder.Private.Partitioned_Rice_Contents_Workspace[i][0]);
				Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Init(encoder.encoder.Private.Partitioned_Rice_Contents_Workspace[i][1]);
			}

			for (uint32_t i = 0; i < 2; i++)
			{
				Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Init(encoder.encoder.Private.Partitioned_Rice_Contents_Workspace_Mid_Side[i][0]);
				Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Init(encoder.encoder.Private.Partitioned_Rice_Contents_Workspace_Mid_Side[i][1]);
			}

			for (uint32_t i = 0; i < 2; i++)
				Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Init(encoder.encoder.Private.Partitioned_Rice_Contents_Extra[i]);

			return encoder;
		}



		/********************************************************************/
		/// <summary>
		/// Free an encoder instance. Deletes the object pointed to by
		/// encoder
		/// </summary>
		/********************************************************************/
		public void Flac__Stream_Encoder_Delete()
		{
			if (encoder == null)
				return;

			Debug.Assert(encoder.Protected != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Private.Frame != null);

			encoder.Private.Is_Being_Deleted = true;

			Flac__Stream_Encoder_Finish();

			for (uint32_t i = 0; i < Constants.Flac__Max_Channels; i++)
			{
				Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Clear(encoder.Private.Partitioned_Rice_Contents_Workspace[i][0]);
				Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Clear(encoder.Private.Partitioned_Rice_Contents_Workspace[i][1]);
			}

			for (uint32_t i = 0; i < 2; i++)
			{
				Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Clear(encoder.Private.Partitioned_Rice_Contents_Workspace_Mid_Side[i][0]);
				Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Clear(encoder.Private.Partitioned_Rice_Contents_Workspace_Mid_Side[i][1]);
			}

			for (uint32_t i = 0; i < 2; i++)
				Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Clear(encoder.Private.Partitioned_Rice_Contents_Extra[i]);

			encoder.Private.Frame.Flac__BitWriter_Delete();
			encoder.Private = null;
			encoder.Protected = null;
			encoder = null;
		}
		#endregion

		#region Public methods
		/********************************************************************/
		/// <summary>
		/// Initialize the encoder instance to encode native FLAC streams.
		///
		/// This flavor of initialization sets up the encoder to encode to a
		/// native FLAC stream. I/O is performed via callbacks to the client.
		/// For encoding to a plain file via filename or open Stream,
		/// Flac__Stream_Encoder_Init_File() provide a simpler interface.
		///
		/// This function should be called after Flac__Stream_Encoder_New()
		/// and Flac__Stream_Encoder_Set_*() but before
		/// Flac__Stream_Encoder_Process() or
		/// Flac__Stream_Encoder_Process_Interleaved().
		///
		/// The call to Flac__Stream_Encoder_Init_Stream() currently will
		/// also immediately call the write callback several times, once
		/// with the fLaC signature, and once for each encoded metadata
		/// block
		/// </summary>
		/// <param name="write_Callback">See Flac__StreamEncoderWriteCallback. This delegate must not be NULL</param>
		/// <param name="seek_Callback">See Flac__StreamEncoderSeekCallback. This delegate may be NULL if seeking is not supported. The encoder uses seeking to go back and write some some stream statistics to the STREAMINFO block; this is recommended but not necessary to create a valid FLAC stream. If seek_Callback is not NULL then a tell_Callback must also be supplied. Alternatively, a dummy seek callback that just returns FLAC__STREAM_ENCODER_SEEK_STATUS_UNSUPPORTED may also be supplied, all though this is slightly less efficient for the encoder</param>
		/// <param name="tell_Callback">See Flac__StreamEncoderTellCallback. This delegate may be NULL if not supported by the client. If seek_Callback is NULL then this argument will be ignored. If seek_Callback is not NULL then a tell_Callback must also be supplied. Alternatively, a dummy tell callback that just returns FLAC__STREAM_ENCODER_TELL_STATUS_UNSUPPORTED may also be supplied, all though this is slightly less efficient for the encoder</param>
		/// <param name="metadata_Callback">See Flac__StreamEncoderMetadataCallback. See FLAC__StreamEncoderMetadataCallback. This delegate may be NULL if the callback is not desired. If the client provides a seek Callback, this delegate is not necessary as the encoder will automatically seek back and update the STREAMINFO block. It may also be NULL if the client does not support seeking, since it will have no way of going back to update the STREAMINFO. However the client can still supply a callback if it would like to know the details from the STREAMINFO</param>
		/// <param name="client_Data">This value will be supplied to callbacks in their client_Data argument</param>
		/// <returns>FLAC__STREAM_ENCODER_INIT_STATUS_OK if initialization was successful; see Flac__StreamEncoderInitStatus for the meanings of other return values.</returns>
		/********************************************************************/
		public Flac__StreamEncoderInitStatus Flac__Stream_Encoder_Init_Stream(Flac__StreamEncoderWriteCallback write_Callback, Flac__StreamEncoderSeekCallback seek_Callback, Flac__StreamEncoderTellCallback tell_Callback, Flac__StreamEncoderMetadataCallback metadata_Callback, object client_Data)
		{
			return Init_Stream_Internal(write_Callback, seek_Callback, tell_Callback, metadata_Callback, client_Data);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the encoder instance to encode native FLAC streams.
		///
		/// This flavor of initialization sets up the encoder to encode to a
		/// plain native .NET FLAC stream. For non-stdio streams, you must
		/// use Flac__Stream_Encoder_Init_Stream() and provide callbacks for
		/// the I/O.
		///
		/// This function should be called after Flac__Stream_Encoder_New()
		/// and Flac__Stream_Encoder_Set_*() but before
		/// Flac__Stream_Encoder_Process() or
		/// Flac__Stream_Encoder_Process_Interleaved().
		/// </summary>
		/// <param name="file">An open FLAC file. The file should have been opened with write mode and rewound. The file becomes owned by the encoder and should not be manipulated by the client while encoding</param>
		/// <param name="leave_Open">If false, the stream will be closed when Flac__Stream_Encoder_Finish() is called</param>
		/// <param name="progress_Callback">See Flac__StreamEncoderProgressCallback. This delegate may be NULL if the callback is not desired</param>
		/// <param name="client_Data">This value will be supplied to callbacks in their client_Data argument</param>
		/// <returns>FLAC__STREAM_ENCODER_INIT_STATUS_OK if initialization was successful; see Flac__StreamEncoderInitStatus for the meanings of other return values.</returns>
		/********************************************************************/
		public Flac__StreamEncoderInitStatus Flac__Stream_Encoder_Init_File(Stream file, bool leave_Open, Flac__StreamEncoderProgressCallback progress_Callback, object client_Data)
		{
			return Init_File_Internal(file, leave_Open, progress_Callback, client_Data);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the encoder instance to encode native FLAC files.
		///
		/// This flavor of initialization sets up the encoder to encode to a
		/// plain FLAC file.
		///
		/// This function should be called after Flac__Stream_Encoder_New()
		/// and Flac__Stream_Encoder_Set_*() but before
		/// Flac__Stream_Encoder_Process() or
		/// Flac__Stream_Encoder_Process_Interleaved().
		/// </summary>
		/// <param name="filename">The name of the file to encode to</param>
		/// <param name="progress_Callback">See Flac__StreamEncoderProgressCallback. This delegate may be NULL if the callback is not desired</param>
		/// <param name="client_Data">This value will be supplied to callbacks in their client_Data argument</param>
		/// <returns>FLAC__STREAM_ENCODER_INIT_STATUS_OK if initialization was successful; see Flac__StreamEncoderInitStatus for the meanings of other return values.</returns>
		/********************************************************************/
		public Flac__StreamEncoderInitStatus Flac__Stream_Encoder_Init_File(string filename, Flac__StreamEncoderProgressCallback progress_Callback, object client_Data)
		{
			return Init_File_Internal(filename, progress_Callback, client_Data);
		}



		/********************************************************************/
		/// <summary>
		/// Finish the encoding process.
		/// Flushes the encoding buffer, releases resources, resets the
		/// encoder settings to their defaults, and returns the encoder state
		/// to FLAC__STREAM_ENCODER_UNINITIALIZED. Note that this can
		/// generate one or more write callbacks before returning, and will
		/// generate a metadata callback.
		///
		/// Note that in the course of processing the last frame, errors can
		/// occur, so the caller should be sure to check the return value to
		/// ensure the file was encoded properly.
		///
		/// In the event of a prematurely-terminated encode, it is not
		/// strictly necessary to call this immediately before
		/// Flac__Stream_Encoder_Delete() but it is good practice to match
		/// every Flac__Stream_Encoder_Init_*() with a
		/// Flac__Stream_Encoder_Finish()
		/// <returns>False if an error occurred processing the last frame; else true. If false, caller should check the state with Flac__Stream_Encoder_Get_State() for more information about the error</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Finish()
		{
			Flac__bool error = false;

			if (encoder == null)
				return false;

			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			if (encoder.Protected.State == Flac__StreamEncoderState.Uninitialized)
				return true;

			if ((encoder.Protected.State == Flac__StreamEncoderState.Ok) && !encoder.Private.Is_Being_Deleted)
			{
				if (encoder.Private.Current_Sample_Number != 0)
				{
					Flac__bool is_Fractional_Block = encoder.Protected.BlockSize != encoder.Private.Current_Sample_Number;
					encoder.Protected.BlockSize = encoder.Private.Current_Sample_Number;

					if (!Process_Frame(is_Fractional_Block, true))
						error = true;
				}
			}

			if (encoder.Protected.Do_Md5)
				((Flac__StreamMetadata_StreamInfo)encoder.Private.StreamInfo.Data).Md5Sum = encoder.Private.Md5.Flac__Md5Final();

			if (!encoder.Private.Is_Being_Deleted)
			{
				if (encoder.Protected.State == Flac__StreamEncoderState.Ok)
				{
					if (encoder.Private.Seek_Callback != null)
					{
						Update_Metadata();

						// Check if an error occurred while updating metadata
						if (encoder.Protected.State != Flac__StreamEncoderState.Ok)
							error = true;
					}

					if (encoder.Private.Metadata_Callback != null)
						encoder.Private.Metadata_Callback(this, encoder.Private.StreamInfo, encoder.Private.Client_Data);
				}
			}

			if (encoder.Private.File != null)
			{
				if (!encoder.Private.Leave_Stream_Open)
					encoder.Private.File.Dispose();

				encoder.Private.File = null;
				encoder.Private.Leave_Stream_Open = false;
			}

			Free();
			Set_Defaults(this);

			if (!error)
				encoder.Protected.State = Flac__StreamEncoderState.Uninitialized;

			return !error;
		}



		/********************************************************************/
		/// <summary>
		/// Set the Subset flag. If true, the encoder will comply with the
		/// Subset and will check the settings during
		/// Flac__Stream_Encoder_Init_*() to see if all settings comply.
		/// If false, the settings may take advantage of the full range that
		/// the format allows.
		///
		/// Make sure you know what it entails before setting this to false.
		///
		/// Default true
		/// <param name="value">Flag value (see above)</param>
		/// <returns>false if the encoder is already initialized, else true</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Set_Streamable_Subset(Flac__bool value)
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			if (encoder.Protected.State != Flac__StreamEncoderState.Uninitialized)
				return false;

			encoder.Protected.Streamable_Subset = value;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Set the number of channels to be encoded.
		///
		/// Default 2
		/// <param name="value">See above</param>
		/// <returns>false if the encoder is already initialized, else true</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Set_Channels(uint32_t value)
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			if (encoder.Protected.State != Flac__StreamEncoderState.Uninitialized)
				return false;

			encoder.Protected.Channels = value;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Set the sample resolution of the input to be encoded.
		///
		/// Warning:
		/// Do not feed the encoder data that is wider than the value you
		/// set here or you will generate an invalid stream.
		///
		/// Default 16
		/// <param name="value">See above</param>
		/// <returns>false if the encoder is already initialized, else true</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Set_Bits_Per_Sample(uint32_t value)
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			if (encoder.Protected.State != Flac__StreamEncoderState.Uninitialized)
				return false;

			encoder.Protected.Bits_Per_Sample = value;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Set the sample rate (in Hz) of the input to be encoded.
		///
		/// Default 44100
		/// <param name="value">See above</param>
		/// <returns>false if the encoder is already initialized, else true</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Set_Sample_Rate(uint32_t value)
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			if (encoder.Protected.State != Flac__StreamEncoderState.Uninitialized)
				return false;

			encoder.Protected.Sample_Rate = value;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Set the compression level
		///
		/// The compression level is roughly proportional to the amount of
		/// effort the encoder expends to compress the file. A higher level
		/// usually means more computation but higher compression. The
		/// default level is suitable for most applications.
		///
		/// Currently the levels range from 0 (fastest, least compression)
		/// to 8 (slowest, most compression). A value larger than 8 will be
		/// treated as 8.
		///
		/// This function automatically calls the following other _set_
		/// functions with appropriate values, so the client does not need
		/// to unless it specifically wants to override them:
		///  - Flac__Stream_Encoder_Set_Do_Mid_Side_Stereo()
		///  - Flac__Stream_Encoder_Set_Loose_Mid_Side_Stereo()
		///  - Flac__Stream_Encoder_Set_Apodization()
		///  - Flac__Stream_Encoder_Set_Max_Lpc_Order()
		///  - Flac__Stream_Encoder_Set_Qlp_Coeff_Precision()
		///  - Flac__Stream_Encoder_Set_Do_Qlp_Coeff_Prec_Search()
		///  - Flac__Stream_Encoder_Set_Do_Exhaustive_Model_Search()
		///  - Flac__Stream_Encoder_Set_Min_Residual_Partition_Order()
		///  - Flac__Stream_Encoder_Set_Max_Residual_Partition_Order()
		///
		/// The actual values set for each level are:
		/// +-------+--------------------+-----------------------+-----------------------------------------------+---------------+---------------------+-----------------------+-------------------------+------------------------------+------------------------------+
		/// | level | do mid-side stereo | loose mid-side stereo | apodization                                   | max lpc order | qlp coeff precision | qlp coeff prec search | exhaustive model search | min residual partition order | max residual partition order |
		/// +-------+--------------------+-----------------------+-----------------------------------------------+---------------+---------------------+-----------------------+-------------------------+------------------------------+------------------------------+
		/// | 0     | false              | false                 | tukey(0.5)                                    | 0             | 0                   | false                 | false                   | 0                            | 3                            |
		/// | 1     | true               | true                  | tukey(0.5)                                    | 0             | 0                   | false                 | false                   | 0                            | 3                            |
		/// | 2     | true               | false                 | tukey(0.5)                                    | 0             | 0                   | false                 | false                   | 0                            | 3                            |
		/// | 3     | false              | false                 | tukey(0.5)                                    | 6             | 0                   | false                 | false                   | 0                            | 4                            |
		/// | 4     | true               | true                  | tukey(0.5)                                    | 8             | 0                   | false                 | false                   | 0                            | 4                            |
		/// | 5     | true               | false                 | tukey(0.5)                                    | 8             | 0                   | false                 | false                   | 0                            | 5                            |
		/// | 6     | true               | false                 | tukey(0.5);partial_tukey(2)                   | 8             | 0                   | false                 | false                   | 0                            | 6                            |
		/// | 7     | true               | false                 | tukey(0.5);partial_tukey(2)                   | 12            | 0                   | false                 | false                   | 0                            | 6                            |
		/// | 8     | true               | false                 | tukey(0.5);partial_tukey(2);punchout_tukey(3) | 12            | 0                   | false                 | false                   | 0                            | 6                            |
		/// +-------+--------------------+-----------------------+-----------------------------------------------+---------------+---------------------+-----------------------+-------------------------+------------------------------+------------------------------+
		///
		/// Default is 5
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Set_Compression_Level(uint32_t value)
		{
			Flac__bool ok = true;

			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			if (encoder.Protected.State != Flac__StreamEncoderState.Uninitialized)
				return false;

			if (value >= compression_Levels.Length)
				value = (uint32_t)compression_Levels.Length - 1;

			ok &= Flac__Stream_Encoder_Set_Do_Mid_Side_Stereo(compression_Levels[value].Do_Mid_Side_Stereo);
			ok &= Flac__Stream_Encoder_Set_Loose_Mid_Side_Stereo(compression_Levels[value].Loose_Mid_Side_Stereo);
			ok &= Flac__Stream_Encoder_Set_Apodization(compression_Levels[value].Apodization);
			ok &= Flac__Stream_Encoder_Set_Max_Lpc_Order(compression_Levels[value].Max_Lpc_Order);
			ok &= Flac__Stream_Encoder_Set_Qlp_Coeff_Precision(compression_Levels[value].Qlp_Coeff_Precision);
			ok &= Flac__Stream_Encoder_Set_Do_Qlp_Coeff_Prec_Search(compression_Levels[value].Do_Qlp_Coeff_Prec_Search);
			ok &= Flac__Stream_Encoder_Set_Do_Exhaustive_Model_Search(compression_Levels[value].Do_Exhaustive_Model_Search);
			ok &= Flac__Stream_Encoder_Set_Min_Residual_Partition_Order(compression_Levels[value].Min_Residual_Partition_Order);
			ok &= Flac__Stream_Encoder_Set_Max_Residual_Partition_Order(compression_Levels[value].Max_Residual_Partition_Order);

			return ok;
		}



		/********************************************************************/
		/// <summary>
		/// Set the blocksize to use while encoding.
		///
		/// The number of samples to use per frame. Use 0 to let the encoder
		/// estimate a blocksize; this is usually best.
		///
		/// Default 0
		/// <param name="value">See above</param>
		/// <returns>false if the encoder is already initialized, else true</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Set_BlockSize(uint32_t value)
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			if (encoder.Protected.State != Flac__StreamEncoderState.Uninitialized)
				return false;

			encoder.Protected.BlockSize = value;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Set to true to enable mid-side encoding on stereo input. The
		/// number of channels must be 2 for this to have any effect. Set to
		/// false to use only independent channel coding.
		///
		/// Default true
		/// <param name="value">Flag value (see above)</param>
		/// <returns>false if the encoder is already initialized, else true</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Set_Do_Mid_Side_Stereo(Flac__bool value)
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			if (encoder.Protected.State != Flac__StreamEncoderState.Uninitialized)
				return false;

			encoder.Protected.Do_Mid_Side_Stereo = value;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Set to true to enable adaptive switching between mid-side and
		/// left-right encoding on stereo input. Set to false to use
		/// exhaustive searching. Setting this to true requires
		/// Flac__Stream_Encoder_Set_Do_Mid_Side_Stereo() to also be set to
		/// true in order to have any effect.
		///
		/// Default false
		/// <param name="value">Flag value (see above)</param>
		/// <returns>false if the encoder is already initialized, else true</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Set_Loose_Mid_Side_Stereo(Flac__bool value)
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			if (encoder.Protected.State != Flac__StreamEncoderState.Uninitialized)
				return false;

			encoder.Protected.Loose_Mid_Side_Stereo = value;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Sets the apodization function(s) the encoder will use when
		/// windowing audio data for LPC analysis.
		///
		/// The specification is a plain ASCII string which specifies exactly
		/// which functions to use. There may be more than one (up to 32),
		/// separated by ';' characters. Some functions take one or more
		/// comma-separated arguments in parentheses.
		///
		/// The available functions are bartlett, bartlett_hann, blackman,
		/// blackman_harris_4term_92db, connes, flattop, gauss(STDDEV),
		/// hamming, hann, kaiser_bessel, nuttall, rectangle, triangle,
		/// tukey(P), partial_tukey(n[/ov[/P]]), punchout_tukey(n[/ov[/P]]),
		/// welch.
		///
		/// For gauss(STDDEV), STDDEV specifies the standard deviation
		/// (0 &lt; STDDEV &lt;= 0.5).
		///
		/// For tukey(P), P specifies the fraction of the window that is
		/// tapered (0&lt;=P&lt;=1). P=0 corresponds to rectangle and P=1
		/// corresponds to hann.
		///
		/// Specifying partial_tukey or punchout_tukey works a little
		/// different. These do not specify a single apodization function,
		/// but a series of them with some overlap. partial_tukey specifies
		/// a series of small windows (all treated separately) while
		/// punchout_tukey specifies a series of windows that have a hole in
		/// them. In this way, the predictor is constructed with only a part
		/// of the block, which helps in case a block consists of dissimilar
		/// parts.
		///
		/// The three parameters that can be specified for the functions are
		/// n, ov and P. n is the number of functions to add, ov is the
		/// overlap of the windows in case of partial_tukey and the overlap
		/// in the gaps in case of punchout_tukey. P is the fraction of the
		/// window that is tapered, like with a regular tukey window. The
		/// function can be specified with only a number, a number and an
		/// overlap, or a number an overlap and a P, for example,
		/// partial_tukey(3), partial_tukey(3/0.3) and
		/// partial_tukey(3/0.3/0.5) are all valid. ov should be smaller than
		/// 1 and can be negative.
		///
		/// Example specifications are "blackman" or
		/// "hann;triangle;tukey(0.5);tukey(0.25);tukey(0.125)"
		///
		/// Any function that is specified erroneously is silently dropped.
		/// Up to 32 functions are kept, the rest are dropped. If the
		/// specification is empty the encoder defaults to "tukey(0.5)".
		///
		/// When more than one function is specified, then for every subframe
		/// the encoder will try each of them separately and choose the
		/// window that results in the smallest compressed subframe.
		///
		/// Note that each function specified causes the encoder to occupy a
		/// floating point array in which to store the window. Also note that
		/// the values of P, STDDEV and ov are locale-specific, so if the
		/// comma separator specified by the locale is a comma, a comma
		/// should be used.
		///
		/// default "tukey(0.5)"
		/// <param name="specification">See above</param>
		/// <returns>false if the encoder is already initialized, else true</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Set_Apodization(string specification)
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			if (encoder.Protected.State != Flac__StreamEncoderState.Uninitialized)
				return false;

			encoder.Protected.Num_Apodizations = 0;

			while (true)
			{
				int s = specification.IndexOf(';');
				size_t n = (size_t)(s != -1 ? s : specification.Length);

				if ((n == 8) && specification.StartsWith("bartlett"))
					encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations++].Type = Flac__ApodizationFunction.Bartlett;
				else if ((n == 13) && specification.StartsWith("bartlett_hann"))
					encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations++].Type = Flac__ApodizationFunction.Bartlett_Hann;
				else if ((n == 8) && specification.StartsWith("blackman"))
					encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations++].Type = Flac__ApodizationFunction.Blackman;
				else if ((n == 26) && specification.StartsWith("blackman_harris_4term_92db"))
					encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations++].Type = Flac__ApodizationFunction.Blackman_Harris_4Term_92Db_Sidelobe;
				else if ((n == 6) && specification.StartsWith("connes"))
					encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations++].Type = Flac__ApodizationFunction.Connes;
				else if ((n == 7) && specification.StartsWith("flattop"))
					encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations++].Type = Flac__ApodizationFunction.Flattop;
				else if ((n > 7) && specification.StartsWith("gauss("))
				{
					Flac__real stdDev = Helpers.ParseFloat(specification.Substring(6));
					if ((stdDev > 0.0) && (stdDev <= 0.5))
					{
						encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations].Parameters = new Flac__ApodizationParameter_Gauss { stdDev = stdDev };
						encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations++].Type = Flac__ApodizationFunction.Gauss;
					}
				}
				else if ((n == 7) && specification.StartsWith("hamming"))
					encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations++].Type = Flac__ApodizationFunction.Hamming;
				else if ((n == 4) && specification.StartsWith("hann"))
					encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations++].Type = Flac__ApodizationFunction.Hann;
				else if ((n == 13) && specification.StartsWith("kaiser_bessel"))
					encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations++].Type = Flac__ApodizationFunction.Kaiser_Bessel;
				else if ((n == 7) && specification.StartsWith("nuttall"))
					encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations++].Type = Flac__ApodizationFunction.Nuttall;
				else if ((n == 9) && specification.StartsWith("rectangle"))
					encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations++].Type = Flac__ApodizationFunction.Rectangle;
				else if ((n == 8) && specification.StartsWith("triangle"))
					encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations++].Type = Flac__ApodizationFunction.Triangle;
				else if ((n > 7) && specification.StartsWith("tukey("))
				{
					Flac__real p = Helpers.ParseFloat(specification.Substring(6));
					if ((p >= 0.0) && (p <= 1.0))
					{
						encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations].Parameters = new Flac__ApodizationParameter_Tukey { P = p };
						encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations++].Type = Flac__ApodizationFunction.Tukey;
					}
				}
				else if ((n > 15) && specification.StartsWith("partial_tukey("))
				{
					Flac__int32 tukey_Parts = Helpers.ParseInt(specification.Substring(14));
					int si_1 = specification.IndexOf('/');
					Flac__real overlap = si_1 != -1 ? Math.Min(Helpers.ParseFloat(specification.Substring(si_1 + 1)), 0.99f) : 0.1f;
					Flac__real overlap_Units = 1.0f / (1.0f - overlap) - 1.0f;
					int si_2 = (si_1 != -1 ? specification.Substring(si_1 + 1) : specification).IndexOf('/');
					Flac__real tukey_P = si_2 != -1 ? Helpers.ParseFloat(specification.Substring(si_2 + 1)) : 0.2f;

					if (tukey_Parts <= 1)
					{
						encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations].Parameters = new Flac__ApodizationParameter_Tukey { P = tukey_P };
						encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations++].Type = Flac__ApodizationFunction.Tukey;
					}
					else if ((encoder.Protected.Num_Apodizations + tukey_Parts) < 32)
					{
						for (Flac__int32 m = 0; m < tukey_Parts; m++)
						{
							encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations].Parameters = new Flac__ApodizationParameter_Multiple_Tukey
							{
								P = tukey_P,
								Start = m / (tukey_Parts + overlap_Units),
								End = (m + 1 + overlap_Units) / (tukey_Parts + overlap_Units)
							};
							encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations++].Type = Flac__ApodizationFunction.Partial_Tukey;
						}
					}
				}
				else if ((n > 16) && specification.StartsWith("punchout_tukey("))
				{
					Flac__int32 tukey_Parts = Helpers.ParseInt(specification.Substring(15));
					int si_1 = specification.IndexOf('/');
					Flac__real overlap = si_1 != -1 ? Math.Min(Helpers.ParseFloat(specification.Substring(si_1 + 1)), 0.99f) : 0.2f;
					Flac__real overlap_Units = 1.0f / (1.0f - overlap) - 1.0f;
					int si_2 = (si_1 != -1 ? specification.Substring(si_1 + 1) : specification).IndexOf('/');
					Flac__real tukey_P = si_2 != -1 ? Helpers.ParseFloat(specification.Substring(si_2 + 1)) : 0.2f;

					if (tukey_Parts <= 1)
					{
						encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations].Parameters = new Flac__ApodizationParameter_Tukey { P = tukey_P };
						encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations++].Type = Flac__ApodizationFunction.Tukey;
					}
					else if ((encoder.Protected.Num_Apodizations + tukey_Parts) < 32)
					{
						for (Flac__int32 m = 0; m < tukey_Parts; m++)
						{
							encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations].Parameters = new Flac__ApodizationParameter_Multiple_Tukey
							{
								P = tukey_P,
								Start = m / (tukey_Parts + overlap_Units),
								End = (m + 1 + overlap_Units) / (tukey_Parts + overlap_Units)
							};
							encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations++].Type = Flac__ApodizationFunction.Punchout_Tukey;
						}
					}
				}
				else if ((n == 5) && specification.StartsWith("welch"))
					encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations++].Type = Flac__ApodizationFunction.Welch;

				if (encoder.Protected.Num_Apodizations == 32)
					break;

				if (s != -1)
					specification = specification.Substring(s + 1);
				else
					break;
			}

			if (encoder.Protected.Num_Apodizations == 0)
			{
				encoder.Protected.Num_Apodizations = 1;
				encoder.Protected.Apodizations[0].Type = Flac__ApodizationFunction.Tukey;
				encoder.Protected.Apodizations[0].Parameters = new Flac__ApodizationParameter_Tukey { P = 0.5f };
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Set the maximum LPC order, or 0 to use only the fixed predictors.
		///
		/// Default 8
		/// <param name="value">See above</param>
		/// <returns>false if the encoder is already initialized, else true</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Set_Max_Lpc_Order(uint32_t value)
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			if (encoder.Protected.State != Flac__StreamEncoderState.Uninitialized)
				return false;

			encoder.Protected.Max_Lpc_Order = value;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Set the precision, in bits, of the quantized linear predictor
		/// coefficients, or 0 to let the encoder select it based on the
		/// blocksize.
		///
		/// NOTE:
		/// In the current implementation, qlp_coeff_precision +
		/// bits_per_sample must be less than 32.
		///
		/// Default 0
		/// <param name="value">See above</param>
		/// <returns>false if the encoder is already initialized, else true</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Set_Qlp_Coeff_Precision(uint32_t value)
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			if (encoder.Protected.State != Flac__StreamEncoderState.Uninitialized)
				return false;

			encoder.Protected.Qlp_Coeff_Precision = value;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Set to false to use only the specified quantized linear predictor
		/// coefficient precision, or true to search neighboring precision
		/// values and use the best one.
		///
		/// Default false
		/// <param name="value">See above</param>
		/// <returns>false if the encoder is already initialized, else true</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Set_Do_Qlp_Coeff_Prec_Search(Flac__bool value)
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			if (encoder.Protected.State != Flac__StreamEncoderState.Uninitialized)
				return false;

			encoder.Protected.Do_Qlp_Coeff_Prec_Search = value;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Set to false to let the encoder estimate the best model order
		/// based on the residual signal energy, or true to force the
		/// encoder to evaluate all order models and select the best.
		///
		/// Default false
		/// <param name="value">See above</param>
		/// <returns>false if the encoder is already initialized, else true</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Set_Do_Exhaustive_Model_Search(Flac__bool value)
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			if (encoder.Protected.State != Flac__StreamEncoderState.Uninitialized)
				return false;

			encoder.Protected.Do_Exhaustive_Model_Search = value;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Set the minimum partition order to search when coding the
		/// residual. This is used in tandem with
		/// Flac__Stream_Encoder_Set_Max_Residual_Partition_Order().
		///
		/// The partition order determines the context size in the residual.
		/// The context size will be approximately blocksize / (2 ^ order).
		///
		/// Set both min and max values to 0 to force a single context,
		/// whose Rice parameter is based on the residual signal variance.
		/// Otherwise, set a min and max order, and the encoder will search
		/// all orders, using the mean of each context for its Rice parameter,
		/// and use the best.
		///
		/// Default 0
		/// <param name="value">See above</param>
		/// <returns>false if the encoder is already initialized, else true</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Set_Min_Residual_Partition_Order(uint32_t value)
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			if (encoder.Protected.State != Flac__StreamEncoderState.Uninitialized)
				return false;

			encoder.Protected.Min_Residual_Partition_Order = value;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Set the maximum partition order to search when coding the
		/// residual. This is used in tandem with
		/// Flac__Stream_Encoder_Set_Min_Residual_Partition_Order().
		///
		/// The partition order determines the context size in the residual.
		/// The context size will be approximately blocksize / (2 ^ order).
		///
		/// Set both min and max values to 0 to force a single context,
		/// whose Rice parameter is based on the residual signal variance.
		/// Otherwise, set a min and max order, and the encoder will search
		/// all orders, using the mean of each context for its Rice parameter,
		/// and use the best.
		///
		/// Default 5
		/// <param name="value">See above</param>
		/// <returns>false if the encoder is already initialized, else true</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Set_Max_Residual_Partition_Order(uint32_t value)
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			if (encoder.Protected.State != Flac__StreamEncoderState.Uninitialized)
				return false;

			encoder.Protected.Max_Residual_Partition_Order = value;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Set an estimate of the total samples that will be encoded.
		/// This is merely an estimate and may be set to 0 if unknown.
		/// This value will be written to the STREAMINFO block before
		/// encoding, and can remove the need for the caller to rewrite the
		/// value later if the value is known before encoding.
		///
		/// Default 0
		/// <param name="value">See above</param>
		/// <returns>false if the encoder is already initialized, else true</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Set_Total_Samples_Estimate(uint64_t value)
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			if (encoder.Protected.State != Flac__StreamEncoderState.Uninitialized)
				return false;

			value = Math.Min(value, (1UL << (int)Constants.Flac__Stream_Metadata_StreamInfo_Total_Samples_Len) - 1);
			encoder.Protected.Total_Samples_Estimate = value;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Set the metadata blocks to be emitted to the stream before
		/// encoding. A value of NULL implies no metadata; otherwise, supply
		/// an array of pointers to metadata blocks. The array is non-const
		/// since the encoder may need to change the is_Last flag inside
		/// them, and in some cases update seek point offsets. Otherwise, the
		/// encoder will not modify or free the blocks. It is up to the
		/// caller to free the metadata blocks after encoding finishes.
		///
		/// NOTE:
		/// The encoder stores only copies of the pointers in the metadata
		/// array; the metadata blocks themselves must survive at least until
		/// after Flac__Stream_Encoder_Finish() returns. Do not free the
		/// blocks until then.
		///
		/// NOTE:
		/// The STREAMINFO block is always written and no STREAMINFO block
		/// may occur in the supplied array.
		///
		/// NOTE:
		/// By default the encoder does not create a SEEKTABLE. If one is
		/// supplied in the metadata array, but the client has specified
		/// that it does not support seeking, then the SEEKTABLE will be
		/// written verbatim. However by itself this is not very useful as
		/// the client will not know the stream offsets for the seekpoints
		/// ahead of time. In order to get a proper seektable the client
		/// must support seeking. See next note.
		///
		/// NOTE:
		/// SEEKTABLE blocks are handled specially. Since you will not know
		/// the values for the seek point stream offsets, you should pass in
		/// a SEEKTABLE 'template', that is, a SEEKTABLE object with the
		/// required sample numbers (or placeholder points), with 0 for the
		/// frame_Samples and stream_Offset fields for each point. If the
		/// client has specified that it supports seeking by providing a seek
		/// callback to Flac__Stream_Encoder_Init_Stream() (or by using
		/// Flac__Stream_Encoder_Init*_File()), then while it is encoding
		/// the encoder will fill the stream offsets in for you and when
		/// encoding is finished, it will seek back and write the real
		/// values into the SEEKTABLE block in the stream. There are helper
		/// routines for manipulating seektable template blocks:
		/// Flac__Metadata_Object_Seektable_Template_*(). If the client does
		/// not support seeking, the SEEKTABLE will have inaccurate offsets
		/// which will slow down or remove the ability to seek in the FLAC
		/// stream.
		///
		/// NOTE:
		/// The encoder instance will modify the first SEEKTABLE block as it
		/// transforms the template to a valid seektable while encoding, but
		/// it is still up to the caller to free all metadata blocks after
		/// encoding.
		///
		/// NOTE:
		/// A VORBIS_COMMENT block may be supplied. The vendor string in it
		/// will be ignored. libFLAC will use it's own vendor string. libFLAC
		/// will not modify the passed-in VORBIS_COMMENT's vendor string, it
		/// will simply write it's own into the stream. If no VORBIS_COMMENT
		/// block is present in the metadata array, libFLAC will write an
		/// empty one, containing only the vendor string.
		///
		/// Default null
		/// <param name="metadata">See above</param>
		/// <param name="num_Blocks">See above</param>
		/// <returns>false if the encoder is already initialized, else true</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Set_Metadata(Flac__StreamMetadata[] metadata, uint32_t num_Blocks)
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			if (encoder.Protected.State != Flac__StreamEncoderState.Uninitialized)
				return false;

			if (metadata == null)
				num_Blocks = 0;

			if (num_Blocks == 0)
				metadata = null;

			if (encoder.Protected.Metadata != null)
			{
				encoder.Protected.Metadata = null;
				encoder.Protected.Num_Metadata_Blocks = 0;
			}

			if (num_Blocks != 0)
			{
				Flac__StreamMetadata[] m = Alloc.Safe_MAlloc_Mul_2Op_P<Flac__StreamMetadata>(1, num_Blocks);
				if (m == null)
					return false;

				Array.Copy(metadata, m, num_Blocks);

				encoder.Protected.Metadata = m;
				encoder.Protected.Num_Metadata_Blocks = num_Blocks;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Get the current encoder state
		/// <returns>The current encoder state</returns>
		/// </summary>
		/********************************************************************/
		public Flac__StreamEncoderState Flac__Stream_Encoder_Get_State()
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			return encoder.Protected.State;
		}



		/********************************************************************/
		/// <summary>
		/// Get the Subset flag
		/// <returns>See Flac__Stream_Encoder_Set_Streamable_Subset()</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Get_Streamable_Subset()
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			return encoder.Protected.Streamable_Subset;
		}



		/********************************************************************/
		/// <summary>
		/// Get the number of input channels being processed
		/// <returns>See Flac__Stream_Encoder_Set_Channels()</returns>
		/// </summary>
		/********************************************************************/
		public uint32_t Flac__Stream_Encoder_Get_Channels()
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			return encoder.Protected.Channels;
		}



		/********************************************************************/
		/// <summary>
		/// Get the input sample resolution setting
		/// <returns>See Flac__Stream_Encoder_Set_Bits_Per_Sample()</returns>
		/// </summary>
		/********************************************************************/
		public uint32_t Flac__Stream_Encoder_Get_Bits_Per_Sample()
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			return encoder.Protected.Bits_Per_Sample;
		}



		/********************************************************************/
		/// <summary>
		/// Get the input sample rate setting
		/// <returns>See Flac__Stream_Encoder_Set_Sample_Rate()</returns>
		/// </summary>
		/********************************************************************/
		public uint32_t Flac__Stream_Encoder_Get_Sample_Rate()
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			return encoder.Protected.Sample_Rate;
		}



		/********************************************************************/
		/// <summary>
		/// Get the blocksize setting
		/// <returns>See Flac__Stream_Encoder_Set_BlockSize()</returns>
		/// </summary>
		/********************************************************************/
		public uint32_t Flac__Stream_Encoder_Get_BlockSize()
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			return encoder.Protected.BlockSize;
		}



		/********************************************************************/
		/// <summary>
		/// Get the "mid/side stereo coding" flag
		/// <returns>See Flac__Stream_Encoder_Set_Do_Mid_Side_Stereo()</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Get_Do_Mid_Side_Stereo()
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			return encoder.Protected.Do_Mid_Side_Stereo;
		}



		/********************************************************************/
		/// <summary>
		/// Get the "adaptive mid/side switching" flag
		/// <returns>See Flac__Stream_Encoder_Set_Loose_Mid_Side_Stereo()</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Get_Loose_Mid_Side_Stereo()
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			return encoder.Protected.Loose_Mid_Side_Stereo;
		}



		/********************************************************************/
		/// <summary>
		/// Get the maximum LPC order setting
		/// <returns>See Flac__Stream_Encoder_Set_Max_Lpc_Order()</returns>
		/// </summary>
		/********************************************************************/
		public uint32_t Flac__Stream_Encoder_Get_Max_Lpc_Order()
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			return encoder.Protected.Max_Lpc_Order;
		}



		/********************************************************************/
		/// <summary>
		/// Get the quantized linear predictor coefficient precision setting
		/// <returns>See Flac__Stream_Encoder_Set_Qlp_Coeff_Precision()</returns>
		/// </summary>
		/********************************************************************/
		public uint32_t Flac__Stream_Encoder_Get_Qlp_Coeff_Precision()
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			return encoder.Protected.Qlp_Coeff_Precision;
		}



		/********************************************************************/
		/// <summary>
		/// Get the qlp coefficient precision search flag
		/// <returns>See Flac__Stream_Encoder_Set_Do_Qlp_Coeff_Prec_Search()</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Get_Do_Qlp_Coeff_Prec_Search()
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			return encoder.Protected.Do_Qlp_Coeff_Prec_Search;
		}



		/********************************************************************/
		/// <summary>
		/// Get the exhaustive model search flag
		/// <returns>See Flac__Stream_Encoder_Set_Do_Exhaustive_Model_Search()</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Get_Do_Exhaustive_Model_Search()
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			return encoder.Protected.Do_Exhaustive_Model_Search;
		}



		/********************************************************************/
		/// <summary>
		/// Get the minimum residual partition order setting
		/// <returns>See Flac__Stream_Encoder_Set_Min_Residual_Partition_Order()</returns>
		/// </summary>
		/********************************************************************/
		public uint32_t Flac__Stream_Encoder_Get_Min_Residual_Partition_Order()
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			return encoder.Protected.Min_Residual_Partition_Order;
		}



		/********************************************************************/
		/// <summary>
		/// Get the maximum residual partition order setting
		/// <returns>See Flac__Stream_Encoder_Set_Max_Residual_Partition_Order()</returns>
		/// </summary>
		/********************************************************************/
		public uint32_t Flac__Stream_Encoder_Get_Max_Residual_Partition_Order()
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			return encoder.Protected.Max_Residual_Partition_Order;
		}



		/********************************************************************/
		/// <summary>
		/// Get the previously set estimate of the total samples to be
		/// encoded. The encoder merely mimics back the value given to
		/// Flac__Stream_Encoder_Set_Total_Samples_Estimate() since it has no
		/// other way of knowing how many samples the client will encode
		/// <returns>See Flac__Stream_Encoder_Set_Total_Samples_Estimate()</returns>
		/// </summary>
		/********************************************************************/
		public uint64_t Flac__Stream_Encoder_Get_Total_Samples_Estimate()
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			return encoder.Protected.Total_Samples_Estimate;
		}



		/********************************************************************/
		/// <summary>
		/// Submit data for encoding.
		/// This version allows you to supply the input data via an array of
		/// pointers, each pointer pointing to an array of samples samples
		/// representing one channel. The samples need not be block-aligned,
		/// but each channel should have the same number of samples. Each
		/// sample should be a signed integer, right-justified to the
		/// resolution set by Flac__Stream_Encoder_Set_Bits_Per_Sample().
		/// For example, if the resolution is 16 bits per sample, the
		/// samples should all be in the range [-32768,32767].
		///
		/// For applications where channel order is important, channels must
		/// follow the order as described in the frame header
		/// <param name="buffer">An array of pointers to each channel's signal</param>
		/// <param name="samples">The number of samples in one channel</param>
		/// <returns>True if successful, else false; in this case, check the encoder state with Flac__Stream_Encoder_Get_State() to see what went wrong</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Process(Flac__int32[][] buffer, uint32_t samples)
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);
			Debug.Assert(encoder.Protected.State == Flac__StreamEncoderState.Ok);

			uint32_t j = 0;
			uint32_t channels = encoder.Protected.Channels;
			uint32_t blockSize = encoder.Protected.BlockSize;

			do
			{
				uint32_t n = Math.Min(blockSize + Overread - encoder.Private.Current_Sample_Number, samples - j);

				for (uint32_t channel = 0; channel < channels; channel++)
				{
					if (buffer[channel] == null)
						return false;

					Array.Copy(buffer[channel], j, encoder.Private.Integer_Signal[channel], encoder.Private.Current_Sample_Number, n);
				}

				if (encoder.Protected.Do_Mid_Side_Stereo)
				{
					Debug.Assert(channels == 2);

					// "i <= blockSize" to overread 1 sample; see comment in Overread decl
					for (uint32_t i = encoder.Private.Current_Sample_Number; (i <= blockSize) && (j < samples); i++, j++)
					{
						encoder.Private.Integer_Signal_Mid_Size[1][i] = buffer[0][j] - buffer[1][j];
						encoder.Private.Integer_Signal_Mid_Size[0][i] = (buffer[0][j] + buffer[1][j]) >> 1;	// NOTE: Not the same as 'mid = (buffer[0][j] + buffer[1][j]) / 2' !
					}
				}
				else
					j += n;

				encoder.Private.Current_Sample_Number += n;

				// We only process if we have a full block + 1 extra sample; final block is always handled by Flac__Stream_Encoder_Finish()
				if (encoder.Private.Current_Sample_Number > blockSize)
				{
					Debug.Assert(encoder.Private.Current_Sample_Number == blockSize + Overread);
					Debug.Assert(Overread == 1);	// Assert we only overread 1 sample which simplifies the rest of the code below

					if (!Process_Frame(false, false))
						return false;

					// Move unprocessed overread samples to beginnings of arrays
					for (uint32_t channel = 0; channel < channels; channel++)
						encoder.Private.Integer_Signal[channel][0] = encoder.Private.Integer_Signal[channel][blockSize];

					if (encoder.Protected.Do_Mid_Side_Stereo)
					{
						encoder.Private.Integer_Signal_Mid_Size[0][0] = encoder.Private.Integer_Signal_Mid_Size[0][blockSize];
						encoder.Private.Integer_Signal_Mid_Size[1][0] = encoder.Private.Integer_Signal_Mid_Size[1][blockSize];
					}

					encoder.Private.Current_Sample_Number = 1;
				}
			}
			while (j < samples);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Submit data for encoding.
		/// This version allows you to supply the input data where the
		/// channels are interleaved into a single array (i.e.
		/// channel0_sample0, channel1_sample0, ..., channelN_sample0,
		/// channel0_sample1, ...). The samples need not be block-aligned,
		/// but they must be sample-aligned, i.e. the first value should be
		/// channel0_sample0 and the last value channelN_sampleM. Each
		/// sample should be a signed integer, right-justified to the
		/// resolution set by Flac__Stream_Encoder_Set_Bits_Per_Sample().
		/// For example, if the resolution is 16 bits per sample, the
		/// samples should all be in the range [-32768,32767].
		///
		/// For applications where channel order is important, channels must
		/// follow the order as described in the frame header
		/// <param name="buffer">An array of channel-interleaved data (see above)</param>
		/// <param name="samples">The number of samples in one channel, the same as for Flac__Stream_Encoder_Process(). For example, if encoding two channels, 1000 samples corresponds to a buffer of 2000 values</param>
		/// <returns>True if successful, else false; in this case, check the encoder state with Flac__Stream_Encoder_Get_State() to see what went wrong</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Process_Interleaved(Flac__int32[] buffer, uint32_t samples)
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);
			Debug.Assert(encoder.Protected.State == Flac__StreamEncoderState.Ok);

			uint32_t channels = encoder.Protected.Channels;
			uint32_t blockSize = encoder.Protected.BlockSize;

			uint32_t j = 0;
			uint32_t k = 0;
			uint32_t i;
			Flac__int32 mid, side;

			// We have several flavors of the same basic loop, optimized for
			// different conditions
			if (encoder.Protected.Do_Mid_Side_Stereo && (channels == 2))
			{
				// Stereo coding: unroll channel loop
				do
				{
					// "i <= blockSize" to overread 1 sample; see comment in Overread decl
					for (i = encoder.Private.Current_Sample_Number; (i <= blockSize) && (j < samples); i++, j++)
					{
						encoder.Private.Integer_Signal[0][i] = mid = side = buffer[k++];
						Flac__int32 x = buffer[k++];
						encoder.Private.Integer_Signal[1][i] = x;

						mid += x;
						side -= x;
						mid >>= 1;	// NOTE: Not the same as 'mid = (left + right) / 2' !

						encoder.Private.Integer_Signal_Mid_Size[1][i] = side;
						encoder.Private.Integer_Signal_Mid_Size[0][i] = mid;
					}

					encoder.Private.Current_Sample_Number = i;

					// We only process if we have a full block + 1 extra sample; final block is always handled by Flac__Stream_Encoder_Finish()
					if (i > blockSize)
					{
						if (!Process_Frame(false, false))
							return false;

						// Move unprocessed overread samples to beginnings of arrays
						Debug.Assert(i == blockSize + Overread);
						Debug.Assert(Overread == 1);	// Assert we only overread 1 sample which simplifies the rest of the code below

						encoder.Private.Integer_Signal[0][0] = encoder.Private.Integer_Signal[0][blockSize];
						encoder.Private.Integer_Signal[1][0] = encoder.Private.Integer_Signal[1][blockSize];
						encoder.Private.Integer_Signal_Mid_Size[0][0] = encoder.Private.Integer_Signal_Mid_Size[0][blockSize];
						encoder.Private.Integer_Signal_Mid_Size[1][0] = encoder.Private.Integer_Signal_Mid_Size[1][blockSize];
						encoder.Private.Current_Sample_Number = 1;
					}
				}
				while (j < samples);
			}
			else
			{
				// Independent channel coding: buffer each channel in inner loop
				do
				{
					// "i <= blockSize" to overread 1 sample; see comment in Overread decl
					for (i = encoder.Private.Current_Sample_Number; (i <= blockSize) && (j < samples); i++, j++)
					{
						for (uint32_t channel = 0; channel < channels; channel++)
							encoder.Private.Integer_Signal[channel][i] = buffer[k++];
					}

					encoder.Private.Current_Sample_Number = i;

					// We only process if we have a full block + 1 extra sample; final block is always handled by Flac__Stream_Encoder_Finish()
					if (i > blockSize)
					{
						if (!Process_Frame(false, false))
							return false;

						// Move unprocessed overread samples to beginnings of arrays
						Debug.Assert(i == blockSize + Overread);
						Debug.Assert(Overread == 1);	// Assert we only overread 1 sample which simplifies the rest of the code below

						for (uint32_t channel = 0; channel < channels; channel++)
							encoder.Private.Integer_Signal[channel][0] = encoder.Private.Integer_Signal[channel][blockSize];

						encoder.Private.Current_Sample_Number = 1;
					}
				}
				while (j < samples);
			}

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__StreamEncoderInitStatus Init_Stream_Internal(Flac__StreamEncoderWriteCallback write_Callback, Flac__StreamEncoderSeekCallback seek_Callback, Flac__StreamEncoderTellCallback tell_Callback, Flac__StreamEncoderMetadataCallback metadata_Callback, object client_Data)
		{
			Debug.Assert(encoder != null);

			if (encoder.Protected.State != Flac__StreamEncoderState.Uninitialized)
				return Flac__StreamEncoderInitStatus.Already_Initialized;

			if ((write_Callback == null) || ((seek_Callback != null) && (tell_Callback == null)))
				return Flac__StreamEncoderInitStatus.Invalid_Callbacks;

			if ((encoder.Protected.Channels == 0) || (encoder.Protected.Channels > Constants.Flac__Max_Channels))
				return Flac__StreamEncoderInitStatus.Invalid_Number_Of_Channels;

			if (encoder.Protected.Channels != 2)
			{
				encoder.Protected.Do_Mid_Side_Stereo = false;
				encoder.Protected.Loose_Mid_Side_Stereo = false;
			}
			else if (!encoder.Protected.Do_Mid_Side_Stereo)
				encoder.Protected.Loose_Mid_Side_Stereo = false;

			if (encoder.Protected.Bits_Per_Sample >= 32)
				encoder.Protected.Do_Mid_Side_Stereo = false;	// Since we currently do 32-bit math, the side channel would have 33 bps and overflow

			if ((encoder.Protected.Bits_Per_Sample < Constants.Flac__Min_Bits_Per_Sample) || (encoder.Protected.Bits_Per_Sample > Constants.Flac__Reference_Codec_Max_Bits_Per_Sample))
				return Flac__StreamEncoderInitStatus.Invalid_Bits_Per_Sample;

			if (!Format.Flac__Format_Sample_Rate_Is_Valid(encoder.Protected.Sample_Rate))
				return Flac__StreamEncoderInitStatus.Invalid_Sample_Rate;

			if (encoder.Protected.BlockSize == 0)
			{
				if (encoder.Protected.Max_Lpc_Order == 0)
					encoder.Protected.BlockSize = 1152;
				else
					encoder.Protected.BlockSize = 4096;
			}

			if ((encoder.Protected.BlockSize < Constants.Flac__Min_Block_Size) || (encoder.Protected.BlockSize > Constants.Flac__Max_Block_Size))
				return Flac__StreamEncoderInitStatus.Invalid_Block_Size;

			if (encoder.Protected.Max_Lpc_Order > Constants.Flac__Max_Lpc_Order)
				return Flac__StreamEncoderInitStatus.Invalid_Max_Lpc_Order;

			if (encoder.Protected.BlockSize < encoder.Protected.Max_Lpc_Order)
				return Flac__StreamEncoderInitStatus.Block_Size_Too_Small_For_Lpc_Order;

			if (encoder.Protected.Qlp_Coeff_Precision == 0)
			{
				if (encoder.Protected.Bits_Per_Sample < 16)
				{
					// @@@ Need some data about how to set this here w.r.t. blocksize and sample rate
					// @@@ until then we'll make a guess
					encoder.Protected.Qlp_Coeff_Precision = Math.Max(Constants.Flac__Min_Qlp_Coeff_Precision, 2 + encoder.Protected.Bits_Per_Sample / 2);
				}
				else if (encoder.Protected.Bits_Per_Sample == 16)
				{
					if (encoder.Protected.BlockSize <= 192)
						encoder.Protected.Qlp_Coeff_Precision = 7;
					else if (encoder.Protected.BlockSize <= 384)
						encoder.Protected.Qlp_Coeff_Precision = 8;
					else if (encoder.Protected.BlockSize <= 576)
						encoder.Protected.Qlp_Coeff_Precision = 9;
					else if (encoder.Protected.BlockSize <= 1152)
						encoder.Protected.Qlp_Coeff_Precision = 10;
					else if (encoder.Protected.BlockSize <= 2304)
						encoder.Protected.Qlp_Coeff_Precision = 11;
					else if (encoder.Protected.BlockSize <= 4608)
						encoder.Protected.Qlp_Coeff_Precision = 12;
					else
						encoder.Protected.Qlp_Coeff_Precision = 13;
				}
				else
				{
					if (encoder.Protected.BlockSize <= 384)
						encoder.Protected.Qlp_Coeff_Precision = Constants.Flac__Max_Qlp_Coeff_Precision - 2;
					else if (encoder.Protected.BlockSize <= 1152)
						encoder.Protected.Qlp_Coeff_Precision = Constants.Flac__Max_Qlp_Coeff_Precision - 1;
					else
						encoder.Protected.Qlp_Coeff_Precision = Constants.Flac__Max_Qlp_Coeff_Precision;
				}

				Debug.Assert(encoder.Protected.Qlp_Coeff_Precision <= Constants.Flac__Max_Qlp_Coeff_Precision);
			}
			else if ((encoder.Protected.Qlp_Coeff_Precision < Constants.Flac__Min_Qlp_Coeff_Precision) || (encoder.Protected.Qlp_Coeff_Precision > Constants.Flac__Max_Qlp_Coeff_Precision))
				return Flac__StreamEncoderInitStatus.Invalid_Qlp_Coeff_Precision;

			if (encoder.Protected.Streamable_Subset)
			{
				if (!Format.Flac__Format_BlockSize_Is_Subset(encoder.Protected.BlockSize, encoder.Protected.Sample_Rate))
					return Flac__StreamEncoderInitStatus.Not_Streamable;

				if (!Format.Flac__Format_Sample_Rate_Is_Subset(encoder.Protected.Sample_Rate))
					return Flac__StreamEncoderInitStatus.Not_Streamable;

				if ((encoder.Protected.Bits_Per_Sample != 8) && (encoder.Protected.Bits_Per_Sample != 12) && (encoder.Protected.Bits_Per_Sample != 16) && (encoder.Protected.Bits_Per_Sample != 20) && (encoder.Protected.Bits_Per_Sample != 24))
					return Flac__StreamEncoderInitStatus.Not_Streamable;

				if (encoder.Protected.Max_Residual_Partition_Order > Constants.Flac__Subset_Max_Rice_Partition_Order)
					return Flac__StreamEncoderInitStatus.Not_Streamable;

				if ((encoder.Protected.Sample_Rate <= 48000) && ((encoder.Protected.BlockSize > Constants.Flac__Subset_Max_Block_Size_48000Hz) || (encoder.Protected.Max_Lpc_Order > Constants.Flac__Subset_Max_Lpc_Order_48000Hz)))
					return Flac__StreamEncoderInitStatus.Not_Streamable;
			}

			if (encoder.Protected.Max_Residual_Partition_Order >= (1U << (int)Constants.Flac__Entropy_Coding_Method_Partitioned_Rice_Order_Len))
				encoder.Protected.Max_Residual_Partition_Order = (1U << (int)Constants.Flac__Entropy_Coding_Method_Partitioned_Rice_Order_Len) - 1;

			if (encoder.Protected.Min_Residual_Partition_Order >= encoder.Protected.Max_Residual_Partition_Order)
				encoder.Protected.Min_Residual_Partition_Order = encoder.Protected.Max_Residual_Partition_Order;

			// Keep track of any SEEKTABLE block
			if ((encoder.Protected.Metadata != null) && (encoder.Protected.Num_Metadata_Blocks > 0))
			{
				for (uint32_t i2 = 0; i2 < encoder.Protected.Num_Metadata_Blocks; i2++)
				{
					if ((encoder.Protected.Metadata[i2] != null) && (encoder.Protected.Metadata[i2].Type == Flac__MetadataType.SeekTable))
					{
						encoder.Private.Seek_Table = (Flac__StreamMetadata_SeekTable)encoder.Protected.Metadata[i2].Data;
						break;
					}
				}
			}

			// Validate metadata
			if ((encoder.Protected.Metadata == null) && (encoder.Protected.Num_Metadata_Blocks > 0))
				return Flac__StreamEncoderInitStatus.Invalid_Metadata;

			Flac__bool metadata_Has_SeekTable = false;
			Flac__bool metadata_Has_Vorbis_Comment = false;
			Flac__bool metadata_Picture_Has_Type1 = false;
			Flac__bool metadata_Picture_Has_Type2 = false;

			for (uint32_t i = 0; i < encoder.Protected.Num_Metadata_Blocks; i++)
			{
				Flac__StreamMetadata m = encoder.Protected.Metadata[i];

				if (m.Type == Flac__MetadataType.StreamInfo)
					return Flac__StreamEncoderInitStatus.Invalid_Metadata;
				else if (m.Type == Flac__MetadataType.SeekTable)
				{
					if (metadata_Has_SeekTable)	// Only one is allowed
						return Flac__StreamEncoderInitStatus.Invalid_Metadata;

					metadata_Has_SeekTable = true;

					if (!Format.Flac__Format_SeekTable_Is_Legal((Flac__StreamMetadata_SeekTable)m.Data))
						return Flac__StreamEncoderInitStatus.Invalid_Metadata;
				}
				else if (m.Type == Flac__MetadataType.Vorbis_Comment)
				{
					if (metadata_Has_Vorbis_Comment)	// Only one is allowed
						return Flac__StreamEncoderInitStatus.Invalid_Metadata;

					metadata_Has_Vorbis_Comment = true;
				}
				else if (m.Type == Flac__MetadataType.CueSheet)
				{
					if (!Format.Flac__Format_CueSheet_Is_Legal((Flac__StreamMetadata_CueSheet)m.Data, ((Flac__StreamMetadata_CueSheet)m.Data).Is_Cd, out _))
						return Flac__StreamEncoderInitStatus.Invalid_Metadata;
				}
				else if (m.Type == Flac__MetadataType.Picture)
				{
					Flac__StreamMetadata_Picture picture = (Flac__StreamMetadata_Picture)m.Data;

					if (!Format.Flac__Format_Picture_Is_Legal(picture, out _))
						return Flac__StreamEncoderInitStatus.Invalid_Metadata;

					if (picture.Type == Flac__StreamMetadata_Picture_Type.File_Icon_Standard)
					{
						if (metadata_Picture_Has_Type1)	// There should only be 1 per stream
							return Flac__StreamEncoderInitStatus.Invalid_Metadata;

						metadata_Picture_Has_Type1 = true;

						// Standard icon must be 32x32 pixel PNG
						string mime = Encoding.ASCII.GetString(picture.Mime_Type);

						if ((picture.Type == Flac__StreamMetadata_Picture_Type.File_Icon_Standard) && (((mime != "image/png") && (mime != "-->")) || (picture.Width != 32) || (picture.Height != 32)))
							return Flac__StreamEncoderInitStatus.Invalid_Metadata;
					}
					else if (picture.Type == Flac__StreamMetadata_Picture_Type.File_Icon)
					{
						if (metadata_Picture_Has_Type2)	// There should only be 1 per stream
							return Flac__StreamEncoderInitStatus.Invalid_Metadata;

						metadata_Picture_Has_Type2 = true;
					}
				}
			}

			encoder.Private.Input_Capacity = 0;

			for (uint32_t i = 0; i < encoder.Protected.Channels; i++)
				encoder.Private.Integer_Signal_Unaligned[i] = encoder.Private.Integer_Signal[i] = null;

			for (uint32_t i = 0; i < 2; i++)
				encoder.Private.Integer_Signal_Mid_Side_Unaligned[i] = encoder.Private.Integer_Signal_Mid_Size[i] = null;

			for (uint32_t i = 0; i < encoder.Protected.Num_Apodizations; i++)
				encoder.Private.Window_Unaligned[i] = encoder.Private.Window[i] = null;

			encoder.Private.Windowed_Signal_Unaligned = encoder.Private.Windowed_Signal = null;

			for (uint32_t i = 0; i < encoder.Protected.Channels; i++)
			{
				encoder.Private.Residual_Workspace_Unaligned[i][0] = encoder.Private.Residual_Workspace[i][0] = null;
				encoder.Private.Residual_Workspace_Unaligned[i][1] = encoder.Private.Residual_Workspace[i][1] = null;
				encoder.Private.Best_SubFrame[i] = 0;
			}

			for (uint32_t i = 0; i < 2; i++)
			{
				encoder.Private.Residual_Workspace_Mid_Side_Unaligned[i][0] = encoder.Private.Residual_Workspace_Mid_Side[i][0] = null;
				encoder.Private.Residual_Workspace_Mid_Side_Unaligned[i][1] = encoder.Private.Residual_Workspace_Mid_Side[i][1] = null;
				encoder.Private.Best_SubFrame_Bits_Mid_Side[i] = 0;
			}

			encoder.Private.Abs_Residual_Partition_Sums_Unaligned = encoder.Private.Abs_Residual_Partition_Sums = null;
			encoder.Private.Raw_Bits_Per_Partition_Unaligned = null;
			encoder.Private.Raw_Bits_Per_Partition = null;
			encoder.Private.Loose_Mid_Side_Stereo_Frames = (uint32_t)(encoder.Protected.Sample_Rate * 0.4 / encoder.Protected.BlockSize + 0.5);

			if (encoder.Private.Loose_Mid_Side_Stereo_Frames == 0)
				encoder.Private.Loose_Mid_Side_Stereo_Frames = 1;

			encoder.Private.Loose_Mid_Side_Stereo_Frame_Count = 0;
			encoder.Private.Current_Sample_Number = 0;
			encoder.Private.Current_Frame_Number = 0;

			// First default to the non-asm routines
			encoder.Private.Fixed = new Fixed();
			encoder.Private.Lpc = new Lpc();

			// Since this is a C# port, we do not have any assembler versions of the LPC encoder, so this part has been removed

			// Set state to OK; from here on, errors are fatal and we'll override the state then
			encoder.Protected.State = Flac__StreamEncoderState.Ok;

			encoder.Private.Write_Callback = write_Callback;
			encoder.Private.Seek_Callback = seek_Callback;
			encoder.Private.Tell_Callback = tell_Callback;
			encoder.Private.Metadata_Callback = metadata_Callback;
			encoder.Private.Client_Data = client_Data;

			if (!Resize_Buffers(encoder.Protected.BlockSize))
			{
				// The above function sets the state for us in case of an error
				return Flac__StreamEncoderInitStatus.Encoder_Error;
			}

			if (!encoder.Private.Frame.Flac__BitWriter_Init())
			{
				encoder.Protected.State = Flac__StreamEncoderState.Memory_Allocation_Error;
				return Flac__StreamEncoderInitStatus.Encoder_Error;
			}

			// These must be done before we write any metadata, because that
			// calls the write_Callback, which uses these values
			encoder.Private.First_SeekPoint_To_Check = 0;
			encoder.Private.Samples_Written = 0;
			encoder.Protected.StreamInfo_Offset = 0;
			encoder.Protected.Seekable_Offset = 0;
			encoder.Protected.Audio_Offset = 0;

			// Write the stream header
			if (!encoder.Private.Frame.Flac__BitWriter_Write_Raw_UInt32(Constants.Flac__Stream_Sync, Constants.Flac__Stream_Sync_Len))
			{
				encoder.Protected.State = Flac__StreamEncoderState.Framing_Error;
				return Flac__StreamEncoderInitStatus.Encoder_Error;
			}

			if (!Write_BitBuffer(0, false))
			{
				// The above function sets the state for us in case of an error
				return Flac__StreamEncoderInitStatus.Encoder_Error;
			}

			// Write the STREAMINFO metadata block
			Flac__StreamMetadata_StreamInfo metaStreamInfo = new Flac__StreamMetadata_StreamInfo();
			encoder.Private.StreamInfo = new Flac__StreamMetadata();
			encoder.Private.StreamInfo.Data = metaStreamInfo;

			encoder.Private.StreamInfo.Type = Flac__MetadataType.StreamInfo;
			encoder.Private.StreamInfo.Is_Last = false;	// We will have at a minimum a VORBIS_COMMENT afterwards
			encoder.Private.StreamInfo.Length = Constants.Flac__Stream_Metadata_StreamInfo_Length;
			metaStreamInfo.Min_BlockSize = encoder.Protected.BlockSize;	// This encoder uses the same blocksize for the whole stream
			metaStreamInfo.Max_BlockSize = encoder.Protected.BlockSize;
			metaStreamInfo.Min_FrameSize = 0;	// We don't know this yet; have to fill it in later
			metaStreamInfo.Max_FrameSize = 0;	// We don't know this yet; have to fill it in later
			metaStreamInfo.Sample_Rate = encoder.Protected.Sample_Rate;
			metaStreamInfo.Channels = encoder.Protected.Channels;
			metaStreamInfo.Bits_Per_Sample = encoder.Protected.Bits_Per_Sample;
			metaStreamInfo.Total_Samples = encoder.Protected.Total_Samples_Estimate;	// We will replace this later with the real total
			Array.Clear(metaStreamInfo.Md5Sum);	// We don't know this yet; have to fill it in later

			if (encoder.Protected.Do_Md5)
			{
				encoder.Private.Md5 = new Md5();
				encoder.Private.Md5.Flac__Md5Init();
			}

			if (!Stream_Encoder_Framing.Flac__Add_Metadata_Block(encoder.Private.StreamInfo, encoder.Private.Frame))
			{
				encoder.Protected.State = Flac__StreamEncoderState.Framing_Error;
				return Flac__StreamEncoderInitStatus.Encoder_Error;
			}

			if (!Write_BitBuffer(0, false))
			{
				// The above function sets the state for us in case of an error
				return Flac__StreamEncoderInitStatus.Encoder_Error;
			}

			// Now that the STREAMINFO block is written, we can init this to an
			// absurdly-high value
			metaStreamInfo.Min_FrameSize = (1U << (int)Constants.Flac__Stream_Metadata_StreamInfo_Min_Frame_Size_Len) - 1;

			// ... and clear this to 0
			metaStreamInfo.Total_Samples = 0;

			// Check to see if the supplied metadata contains a VORBIS_COMMENT;
			// if not, we will write an empty one (Flac__Add_Metadata_Block()
			// automatically supplies the vendor string).
			if (!metadata_Has_Vorbis_Comment)
			{
				Flac__StreamMetadata_VorbisComment metaVorbis_Comment = new Flac__StreamMetadata_VorbisComment();
				Flac__StreamMetadata vorbis_Comment = new Flac__StreamMetadata();
				vorbis_Comment.Data = metaVorbis_Comment;

				vorbis_Comment.Type = Flac__MetadataType.Vorbis_Comment;
				vorbis_Comment.Is_Last = encoder.Protected.Num_Metadata_Blocks == 0;
				vorbis_Comment.Length = 4 + 4;	// MAGIC NUMBER
				metaVorbis_Comment.Vendor_String.Length = 0;
				metaVorbis_Comment.Vendor_String.Entry = null;
				metaVorbis_Comment.Num_Comments = 0;
				metaVorbis_Comment.Comments = null;

				if (!Stream_Encoder_Framing.Flac__Add_Metadata_Block(vorbis_Comment, encoder.Private.Frame))
				{
					encoder.Protected.State = Flac__StreamEncoderState.Framing_Error;
					return Flac__StreamEncoderInitStatus.Encoder_Error;
				}

				if (!Write_BitBuffer(0, false))
				{
					// The above function sets the state for us in case of an error
					return Flac__StreamEncoderInitStatus.Encoder_Error;
				}
			}

			// Write the user's metadata blocks
			for (uint32_t i = 0; i < encoder.Protected.Num_Metadata_Blocks; i++)
			{
				encoder.Protected.Metadata[i].Is_Last = i == encoder.Protected.Num_Metadata_Blocks - 1;

				if (!Stream_Encoder_Framing.Flac__Add_Metadata_Block(encoder.Protected.Metadata[i], encoder.Private.Frame))
				{
					encoder.Protected.State = Flac__StreamEncoderState.Framing_Error;
					return Flac__StreamEncoderInitStatus.Encoder_Error;
				}

				if (!Write_BitBuffer(0, false))
				{
					// The above function sets the state for us in case of an error
					return Flac__StreamEncoderInitStatus.Encoder_Error;
				}
			}

			// Now that all the metadata is written, we save the stream offset
			if ((encoder.Private.Tell_Callback != null) && (encoder.Private.Tell_Callback(this, out encoder.Protected.Audio_Offset, encoder.Private.Client_Data) == Flac__StreamEncoderTellStatus.Error))
			{
				// Unsupported just means we didn't get the offset; no error
				encoder.Protected.State = Flac__StreamEncoderState.Client_Error;
				return Flac__StreamEncoderInitStatus.Encoder_Error;
			}

			return Flac__StreamEncoderInitStatus.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__StreamEncoderInitStatus Init_File_Internal(Stream file, bool leave_Open, Flac__StreamEncoderProgressCallback progress_Callback, object client_Data)
		{
			Debug.Assert(encoder != null);
			Debug.Assert(file != null);

			if (encoder.Protected.State != Flac__StreamEncoderState.Uninitialized)
				return Flac__StreamEncoderInitStatus.Already_Initialized;

			// Double protection
			if (file == null)
			{
				encoder.Protected.State = Flac__StreamEncoderState.IO_Error;
				return Flac__StreamEncoderInitStatus.Encoder_Error;
			}

			// To make sure that our file does not go unclosed after an error, we
			// must assign the stream pointer before any further error can occur in
			// this routine
			encoder.Private.File = file;
			encoder.Private.Leave_Stream_Open = leave_Open;

			encoder.Private.Progress_Callback = progress_Callback;
			encoder.Private.Bytes_Written = 0;
			encoder.Private.Samples_Written = 0;
			encoder.Private.Frames_Written = 0;

			Flac__StreamEncoderInitStatus init_Status = Init_Stream_Internal(File_Write_Callback, File_Seek_Callback, File_Tell_Callback, null, client_Data);
			if (init_Status != Flac__StreamEncoderInitStatus.Ok)
			{
				// The above function sets the state for us in case of an error
				return init_Status;
			}

			{
				uint32_t blockSize = Flac__Stream_Encoder_Get_BlockSize();
				Debug.Assert(blockSize != 0);

				encoder.Private.Total_Frames_Estimate = (uint32_t)((Flac__Stream_Encoder_Get_Total_Samples_Estimate() + blockSize - 1) / blockSize);
			}

			return init_Status;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__StreamEncoderInitStatus Init_File_Internal(string filename, Flac__StreamEncoderProgressCallback progress_Callback, object client_Data)
		{
			Debug.Assert(encoder != null);

			// To make sure that our file does not go unclosed after an error, we
			// have to do the same entrance checks here that are later performed
			// in Flac__Stream_Encoder_Init_File() before the File is assigned

			if (encoder.Protected.State != Flac__StreamEncoderState.Uninitialized)
				return Flac__StreamEncoderInitStatus.Already_Initialized;

			try
			{
				Stream file = File.OpenWrite(filename);

				return Init_File_Internal(file, false, progress_Callback, client_Data);
			}
			catch(Exception)
			{
				encoder.Protected.State = Flac__StreamEncoderState.IO_Error;
				return Flac__StreamEncoderInitStatus.Encoder_Error;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Set_Defaults(Stream_Encoder encoder)
		{
			Debug.Assert(encoder != null);

			encoder.encoder.Protected.Streamable_Subset = true;
			encoder.encoder.Protected.Do_Md5 = true;
			encoder.encoder.Protected.Do_Mid_Side_Stereo = false;
			encoder.encoder.Protected.Loose_Mid_Side_Stereo = false;
			encoder.encoder.Protected.Channels = 2;
			encoder.encoder.Protected.Bits_Per_Sample = 16;
			encoder.encoder.Protected.Sample_Rate = 44100;
			encoder.encoder.Protected.BlockSize = 0;
			encoder.encoder.Protected.Num_Apodizations = 1;
			encoder.encoder.Protected.Apodizations[0].Type = Flac__ApodizationFunction.Tukey;
			encoder.encoder.Protected.Apodizations[0].Parameters = new Flac__ApodizationParameter_Tukey { P = 0.5f };
			encoder.encoder.Protected.Max_Lpc_Order = 0;
			encoder.encoder.Protected.Qlp_Coeff_Precision = 0;
			encoder.encoder.Protected.Do_Qlp_Coeff_Prec_Search = false;
			encoder.encoder.Protected.Do_Exhaustive_Model_Search = false;
			encoder.encoder.Protected.Min_Residual_Partition_Order = 0;
			encoder.encoder.Protected.Max_Residual_Partition_Order = 0;
			encoder.encoder.Protected.Total_Samples_Estimate = 0;
			encoder.encoder.Protected.Metadata = null;
			encoder.encoder.Protected.Num_Metadata_Blocks = 0;

			encoder.encoder.Private.Seek_Table = null;
			encoder.encoder.Private.Disable_Constant_SubFrames = false;
			encoder.encoder.Private.Disable_Fixed_SubFrames = false;
			encoder.encoder.Private.Disable_Verbatim_SubFrames = false;
			encoder.encoder.Private.Write_Callback = null;
			encoder.encoder.Private.Seek_Callback = null;
			encoder.encoder.Private.Tell_Callback = null;
			encoder.encoder.Private.Metadata_Callback = null;
			encoder.encoder.Private.Progress_Callback = null;
			encoder.encoder.Private.Client_Data = null;

			encoder.Flac__Stream_Encoder_Set_Compression_Level(5);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Free()
		{
			Debug.Assert(encoder != null);

			if (encoder.Protected.Metadata != null)
			{
				encoder.Protected.Metadata = null;
				encoder.Protected.Num_Metadata_Blocks = 0;
			}

			for (uint32_t i = 0; i < encoder.Protected.Channels; i++)
			{
				if (encoder.Private.Integer_Signal_Unaligned[i] != null)
					encoder.Private.Integer_Signal_Unaligned[i] = null;
			}

			for (uint32_t i = 0; i < 2; i++)
			{
				if (encoder.Private.Integer_Signal_Mid_Side_Unaligned[i] != null)
					encoder.Private.Integer_Signal_Mid_Side_Unaligned[i] = null;
			}

			for (uint32_t i = 0; i < encoder.Protected.Num_Apodizations; i++)
			{
				if (encoder.Private.Window_Unaligned[i] != null)
					encoder.Private.Window_Unaligned[i] = null;
			}

			if (encoder.Private.Windowed_Signal_Unaligned != null)
				encoder.Private.Windowed_Signal_Unaligned = null;

			for (uint32_t channel = 0; channel < encoder.Protected.Channels; channel++)
			{
				for (uint32_t i = 0; i < 2; i++)
				{
					if (encoder.Private.Residual_Workspace_Unaligned[channel][i] != null)
						encoder.Private.Residual_Workspace_Unaligned[channel][i] = null;
				}
			}

			for (uint32_t channel = 0; channel < encoder.Protected.Channels; channel++)
			{
				for (uint32_t i = 0; i < 2; i++)
				{
					if (encoder.Private.Residual_Workspace_Mid_Side_Unaligned[channel][i] != null)
						encoder.Private.Residual_Workspace_Mid_Side_Unaligned[channel][i] = null;
				}
			}

			if (encoder.Private.Abs_Residual_Partition_Sums_Unaligned != null)
				encoder.Private.Abs_Residual_Partition_Sums_Unaligned = null;

			if (encoder.Private.Raw_Bits_Per_Partition_Unaligned != null)
				encoder.Private.Raw_Bits_Per_Partition_Unaligned = null;

			encoder.Private.Frame.Flac__BitWriter_Free();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Resize_Buffers(uint32_t new_BlockSize)
		{
			Debug.Assert(new_BlockSize > 0);
			Debug.Assert(encoder.Protected.State == Flac__StreamEncoderState.Ok);
			Debug.Assert(encoder.Private.Current_Sample_Number == 0);

			// To avoid excessive malloc'ing, we only grow the buffer; no shrinking
			if (new_BlockSize <= encoder.Private.Input_Capacity)
				return true;

			Flac__bool ok = true;

			for (uint32_t i = 0; ok && i < encoder.Protected.Channels; i++)
				ok = ok && Memory.Flac__Memory_Alloc_Aligned_Int32_Array(new_BlockSize + 4 + Overread, ref encoder.Private.Integer_Signal_Unaligned[i], ref encoder.Private.Integer_Signal[i]);

			for (uint32_t i = 0; ok && i < 2; i++)
				ok = ok && Memory.Flac__Memory_Alloc_Aligned_Int32_Array(new_BlockSize + 4 + Overread, ref encoder.Private.Integer_Signal_Mid_Side_Unaligned[i], ref encoder.Private.Integer_Signal_Mid_Size[i]);

			if (ok && (encoder.Protected.Max_Lpc_Order > 0))
			{
				for (uint32_t i = 0; ok && i < encoder.Protected.Num_Apodizations; i++)
					ok = ok && Memory.Flac__Memory_Alloc_Aligned_Real_Array(new_BlockSize, ref encoder.Private.Window_Unaligned[i], ref encoder.Private.Window[i]);

				ok = ok && Memory.Flac__Memory_Alloc_Aligned_Real_Array(new_BlockSize, ref encoder.Private.Windowed_Signal_Unaligned, ref encoder.Private.Windowed_Signal);
			}

			for (uint32_t channel = 0; ok && channel < encoder.Protected.Channels; channel++)
			{
				for (uint32_t i = 0; i < 2; i++)
					ok = ok && Memory.Flac__Memory_Alloc_Aligned_Int32_Array(new_BlockSize, ref encoder.Private.Residual_Workspace_Unaligned[channel][i], ref encoder.Private.Residual_Workspace[channel][i]);
			}

			for (uint32_t channel = 0; ok && channel < 2; channel++)
			{
				for (uint32_t i = 0; i < 2; i++)
					ok = ok && Memory.Flac__Memory_Alloc_Aligned_Int32_Array(new_BlockSize, ref encoder.Private.Residual_Workspace_Mid_Side_Unaligned[channel][i], ref encoder.Private.Residual_Workspace_Mid_Side[channel][i]);
			}

			// The *2 is an approximation to the series 1 + 1/2 + 1/4 + ... that sums tree occupies in a flat array
			// @@@ new_BlockSize*2 is too pessimistic, but to fix, we need smarter logic
			// because a smaller new_BlockSize can actually increase the # of partitions;
			// would require moving this out into a separate function, then checking its
			// capacity against the need of the current blockSize&min/max_Partition_Order
			// (and maybe predictor order)
			ok = ok && Memory.Flac__Memory_Alloc_Aligned_UInt64_Array(new_BlockSize * 2, ref encoder.Private.Abs_Residual_Partition_Sums_Unaligned, ref encoder.Private.Abs_Residual_Partition_Sums);

			// Now adjust the windows if the blockSize has changed
			if (ok && (new_BlockSize != encoder.Private.Input_Capacity) && (encoder.Protected.Max_Lpc_Order > 0))
			{
				for (uint32_t i = 0; ok && i < encoder.Protected.Num_Apodizations; i++)
				{
					switch (encoder.Protected.Apodizations[i].Type)
					{
						case Flac__ApodizationFunction.Bartlett:
						{
							Window.Flac__Window_Bartlett(encoder.Private.Window[i], (Flac__int32)new_BlockSize);
							break;
						}

						case Flac__ApodizationFunction.Bartlett_Hann:
						{
							Window.Flac__Window_Bartlett_Hann(encoder.Private.Window[i], (Flac__int32)new_BlockSize);
							break;
						}

						case Flac__ApodizationFunction.Blackman:
						{
							Window.Flac__Window_Blackman(encoder.Private.Window[i], (Flac__int32)new_BlockSize);
							break;
						}

						case Flac__ApodizationFunction.Blackman_Harris_4Term_92Db_Sidelobe:
						{
							Window.Flac__Window_Blackman_Harris_4Term_92Db_Sidelobe(encoder.Private.Window[i], (Flac__int32)new_BlockSize);
							break;
						}

						case Flac__ApodizationFunction.Connes:
						{
							Window.Flac__Window_Connes(encoder.Private.Window[i], (Flac__int32)new_BlockSize);
							break;
						}

						case Flac__ApodizationFunction.Flattop:
						{
							Window.Flac__Window_Flattop(encoder.Private.Window[i], (Flac__int32)new_BlockSize);
							break;
						}

						case Flac__ApodizationFunction.Gauss:
						{
							Window.Flac__Window_Gauss(encoder.Private.Window[i], (Flac__int32)new_BlockSize, ((Flac__ApodizationParameter_Gauss)encoder.Protected.Apodizations[i].Parameters).stdDev);
							break;
						}

						case Flac__ApodizationFunction.Hamming:
						{
							Window.Flac__Window_Hamming(encoder.Private.Window[i], (Flac__int32)new_BlockSize);
							break;
						}

						case Flac__ApodizationFunction.Hann:
						{
							Window.Flac__Window_Hann(encoder.Private.Window[i], (Flac__int32)new_BlockSize);
							break;
						}

						case Flac__ApodizationFunction.Kaiser_Bessel:
						{
							Window.Flac__Window_Kaiser_Bessel(encoder.Private.Window[i], (Flac__int32)new_BlockSize);
							break;
						}

						case Flac__ApodizationFunction.Nuttall:
						{
							Window.Flac__Window_Nuttall(encoder.Private.Window[i], (Flac__int32)new_BlockSize);
							break;
						}

						case Flac__ApodizationFunction.Rectangle:
						{
							Window.Flac__Window_Rectangle(encoder.Private.Window[i], (Flac__int32)new_BlockSize);
							break;
						}

						case Flac__ApodizationFunction.Triangle:
						{
							Window.Flac__Window_Triangle(encoder.Private.Window[i], (Flac__int32)new_BlockSize);
							break;
						}

						case Flac__ApodizationFunction.Tukey:
						{
							Window.Flac__Window_Tukey(encoder.Private.Window[i], (Flac__int32)new_BlockSize, ((Flac__ApodizationParameter_Tukey)encoder.Protected.Apodizations[i].Parameters).P);
							break;
						}

						case Flac__ApodizationFunction.Partial_Tukey:
						{
							Window.Flac__Window_Partial_Tukey(encoder.Private.Window[i], (Flac__int32)new_BlockSize, ((Flac__ApodizationParameter_Multiple_Tukey)encoder.Protected.Apodizations[i].Parameters).P, ((Flac__ApodizationParameter_Multiple_Tukey)encoder.Protected.Apodizations[i].Parameters).Start, ((Flac__ApodizationParameter_Multiple_Tukey)encoder.Protected.Apodizations[i].Parameters).End);
							break;
						}

						case Flac__ApodizationFunction.Punchout_Tukey:
						{
							Window.Flac__Window_Punchout_Tukey(encoder.Private.Window[i], (Flac__int32)new_BlockSize, ((Flac__ApodizationParameter_Multiple_Tukey)encoder.Protected.Apodizations[i].Parameters).P, ((Flac__ApodizationParameter_Multiple_Tukey)encoder.Protected.Apodizations[i].Parameters).Start, ((Flac__ApodizationParameter_Multiple_Tukey)encoder.Protected.Apodizations[i].Parameters).End);
							break;
						}

						case Flac__ApodizationFunction.Welch:
						{
							Window.Flac__Window_Welch(encoder.Private.Window[i], (Flac__int32)new_BlockSize);
							break;
						}

						default:
						{
							Debug.Assert(false);

							// Double protection
							Window.Flac__Window_Hann(encoder.Private.Window[i], (Flac__int32)new_BlockSize);
							break;
						}
					}
				}
			}

			if (ok)
				encoder.Private.Input_Capacity = new_BlockSize;
			else
				encoder.Protected.State = Flac__StreamEncoderState.Memory_Allocation_Error;

			return ok;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Write_BitBuffer(uint32_t samples, Flac__bool is_Last_Block)
		{
			Debug.Assert(encoder.Private.Frame.Flac__BitWriter_Is_Byte_Aligned());

			if (!encoder.Private.Frame.Flac__BitWriter_Get_Buffer(out Span<Flac__byte> buffer, out size_t bytes))
			{
				encoder.Protected.State = Flac__StreamEncoderState.Memory_Allocation_Error;
				return false;
			}

			if (Write_Frame(buffer, bytes, samples, is_Last_Block) != Flac__StreamEncoderWriteStatus.Ok)
			{
				encoder.Private.Frame.Flac__BitWriter_Release_Buffer();
				encoder.Private.Frame.Flac__BitWriter_Clear();

				encoder.Protected.State = Flac__StreamEncoderState.Client_Error;
				return false;
			}

			encoder.Private.Frame.Flac__BitWriter_Release_Buffer();
			encoder.Private.Frame.Flac__BitWriter_Clear();

			if (samples > 0)
			{
				Flac__StreamMetadata_StreamInfo metaStreamInfo = (Flac__StreamMetadata_StreamInfo)encoder.Private.StreamInfo.Data;

				metaStreamInfo.Min_FrameSize = Math.Min(bytes, metaStreamInfo.Min_FrameSize);
				metaStreamInfo.Max_FrameSize = Math.Max(bytes, metaStreamInfo.Max_FrameSize);
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__StreamEncoderWriteStatus Write_Frame(Span<Flac__byte> buffer, size_t bytes, uint32_t samples, Flac__bool is_Last_Block)
		{
			Flac__uint64 output_Position = 0;

			// Unsupported just means we didn't get the offset; no error
			if ((encoder.Private.Tell_Callback != null) && (encoder.Private.Tell_Callback(this, out output_Position, encoder.Private.Client_Data) == Flac__StreamEncoderTellStatus.Error))
			{
				encoder.Protected.State = Flac__StreamEncoderState.Client_Error;
				return Flac__StreamEncoderWriteStatus.Fatal_Error;
			}

			// Watch for the STREAMINFO block and first SEEKTABLE block to go by and store their offsets
			if (samples == 0)
			{
				Flac__MetadataType type = (Flac__MetadataType)(buffer[0] & 0x7f);

				if (type == Flac__MetadataType.StreamInfo)
					encoder.Protected.StreamInfo_Offset = output_Position;
				else if ((type == Flac__MetadataType.SeekTable) && (encoder.Protected.Seekable_Offset == 0))
					encoder.Protected.Seekable_Offset = output_Position;
			}

			// Mark the current seek point if hit (if audio_Offset == 0 that
			// means we're still writing metadata and haven't hit the first
			// frame yet)
			if ((encoder.Private.Seek_Table != null) && (encoder.Protected.Audio_Offset > 0) && (encoder.Private.Seek_Table.Num_Points > 0))
			{
				uint32_t blockSize = Flac__Stream_Encoder_Get_BlockSize();
				Flac__uint64 frame_First_Sample = encoder.Private.Samples_Written;
				Flac__uint64 frame_Last_Sample = frame_First_Sample + blockSize - 1;

				for (uint32_t i = encoder.Private.First_SeekPoint_To_Check; i < encoder.Private.Seek_Table.Num_Points; i++)
				{
					Flac__uint64 test_Sample = encoder.Private.Seek_Table.Points[i].Sample_Number;
					if (test_Sample > frame_Last_Sample)
						break;
					else if (test_Sample >= frame_First_Sample)
					{
						encoder.Private.Seek_Table.Points[i].Sample_Number = frame_First_Sample;
						encoder.Private.Seek_Table.Points[i].Stream_Offset = output_Position - encoder.Protected.Audio_Offset;
						encoder.Private.Seek_Table.Points[i].Frame_Samples = blockSize;
						encoder.Private.First_SeekPoint_To_Check++;

						// DO NOT: "break;" and here's why:
						// The seektable template may contain more than one target
						// sample for any given frame; we will keep looping, generating
						// duplicate seekpoints for them, and we'll clean it up later,
						// just before writing the seektable back to the metadata
					}
					else
						encoder.Private.First_SeekPoint_To_Check++;
				}
			}

			Flac__StreamEncoderWriteStatus status = encoder.Private.Write_Callback(this, buffer, bytes, samples, encoder.Private.Current_Frame_Number, encoder.Private.Client_Data);

			if (status == Flac__StreamEncoderWriteStatus.Ok)
			{
				encoder.Private.Bytes_Written += bytes;
				encoder.Private.Samples_Written += samples;

				// We keep a high watermark on the number of frames written because
				// when the encoder goes back to write metadata, 'current_Frame'
				// will drop back to 0
				encoder.Private.Frames_Written = Math.Max(encoder.Private.Frames_Written, encoder.Private.Current_Frame_Number + 1);
			}
			else
				encoder.Protected.State = Flac__StreamEncoderState.Client_Error;

			return status;
		}



		/********************************************************************/
		/// <summary>
		/// Gets called when the encoding process has finished so that we can
		/// update the STREAMINFO and SEEKTABLE blocks
		/// </summary>
		/********************************************************************/
		private void Update_Metadata()
		{
			Flac__byte[] b = new Flac__byte[Math.Max(6, Constants.Flac__Stream_Metadata_SeekPoint_Length)];
			Flac__StreamMetadata metadata = encoder.Private.StreamInfo;
			Debug.Assert(metadata.Type == Flac__MetadataType.StreamInfo);

			Flac__StreamMetadata_StreamInfo metaStreamInfo = (Flac__StreamMetadata_StreamInfo)metadata.Data;
			Flac__uint64 samples = metaStreamInfo.Total_Samples;
			uint32_t min_FrameSize = metaStreamInfo.Min_FrameSize;
			uint32_t max_FrameSize = metaStreamInfo.Max_FrameSize;
			uint32_t bps = metaStreamInfo.Bits_Per_Sample;

			// All this is based on intimate knowledge of the stream header
			// layout, but a change to the header format that would break this
			// would also break all streams encoded in the previous format
			Flac__StreamEncoderSeekStatus seek_Status;

			// Write MD5 signature
			{
				uint32_t md5_Offset = Constants.Flac__Stream_Metadata_Header_Length +
									  (
										Constants.Flac__Stream_Metadata_StreamInfo_Min_Block_Size_Len +
										Constants.Flac__Stream_Metadata_StreamInfo_Max_Block_Size_Len +
										Constants.Flac__Stream_Metadata_StreamInfo_Min_Frame_Size_Len +
										Constants.Flac__Stream_Metadata_StreamInfo_Max_Frame_Size_Len +
										Constants.Flac__Stream_Metadata_StreamInfo_Sample_Rate_Len +
										Constants.Flac__Stream_Metadata_StreamInfo_Channels_Len +
										Constants.Flac__Stream_Metadata_StreamInfo_Bits_Per_Sample_Len +
										Constants.Flac__Stream_Metadata_StreamInfo_Total_Samples_Len
				                      ) / 8;

				if ((seek_Status = encoder.Private.Seek_Callback(this, encoder.Protected.StreamInfo_Offset + md5_Offset, encoder.Private.Client_Data)) != Flac__StreamEncoderSeekStatus.Ok)
				{
					if (seek_Status == Flac__StreamEncoderSeekStatus.Error)
						encoder.Protected.State = Flac__StreamEncoderState.Client_Error;

					return;
				}

				if (encoder.Private.Write_Callback(this, metaStreamInfo.Md5Sum, 16, 0, 0, encoder.Private.Client_Data) != Flac__StreamEncoderWriteStatus.Ok)
				{
					encoder.Protected.State = Flac__StreamEncoderState.Client_Error;
					return;
				}
			}

			// Write total samples
			{
				uint32_t total_Samples_Byte_Offset = Constants.Flac__Stream_Metadata_Header_Length +
				                      (
					                      Constants.Flac__Stream_Metadata_StreamInfo_Min_Block_Size_Len +
					                      Constants.Flac__Stream_Metadata_StreamInfo_Max_Block_Size_Len +
					                      Constants.Flac__Stream_Metadata_StreamInfo_Min_Frame_Size_Len +
					                      Constants.Flac__Stream_Metadata_StreamInfo_Max_Frame_Size_Len +
					                      Constants.Flac__Stream_Metadata_StreamInfo_Sample_Rate_Len +
					                      Constants.Flac__Stream_Metadata_StreamInfo_Channels_Len +
					                      Constants.Flac__Stream_Metadata_StreamInfo_Bits_Per_Sample_Len
										  - 4
				                      ) / 8;

				b[0] = (Flac__byte)(((bps - 1) << 4) | ((samples >> 32) & 0x0f));
				b[1] = (Flac__byte)((samples >> 24) & 0xff);
				b[2] = (Flac__byte)((samples >> 16) & 0xff);
				b[3] = (Flac__byte)((samples >> 8) & 0xff);
				b[4] = (Flac__byte)(samples & 0xff);

				if ((seek_Status = encoder.Private.Seek_Callback(this, encoder.Protected.StreamInfo_Offset + total_Samples_Byte_Offset, encoder.Private.Client_Data)) != Flac__StreamEncoderSeekStatus.Ok)
				{
					if (seek_Status == Flac__StreamEncoderSeekStatus.Error)
						encoder.Protected.State = Flac__StreamEncoderState.Client_Error;

					return;
				}

				if (encoder.Private.Write_Callback(this, b, 5, 0, 0, encoder.Private.Client_Data) != Flac__StreamEncoderWriteStatus.Ok)
				{
					encoder.Protected.State = Flac__StreamEncoderState.Client_Error;
					return;
				}
			}

			// Write min/max framesize
			{
				uint32_t min_FrameSize_Offset = Constants.Flac__Stream_Metadata_Header_Length +
				                                     (
					                                     Constants.Flac__Stream_Metadata_StreamInfo_Min_Block_Size_Len +
					                                     Constants.Flac__Stream_Metadata_StreamInfo_Max_Block_Size_Len
				                                     ) / 8;

				b[0] = (Flac__byte)((min_FrameSize >> 16) & 0xff);
				b[1] = (Flac__byte)((min_FrameSize >> 8) & 0xff);
				b[2] = (Flac__byte)(min_FrameSize & 0xff);
				b[3] = (Flac__byte)((max_FrameSize >> 16) & 0xff);
				b[4] = (Flac__byte)((max_FrameSize >> 8) & 0xff);
				b[5] = (Flac__byte)(max_FrameSize & 0xff);

				if ((seek_Status = encoder.Private.Seek_Callback(this, encoder.Protected.StreamInfo_Offset + min_FrameSize_Offset, encoder.Private.Client_Data)) != Flac__StreamEncoderSeekStatus.Ok)
				{
					if (seek_Status == Flac__StreamEncoderSeekStatus.Error)
						encoder.Protected.State = Flac__StreamEncoderState.Client_Error;

					return;
				}

				if (encoder.Private.Write_Callback(this, b, 6, 0, 0, encoder.Private.Client_Data) != Flac__StreamEncoderWriteStatus.Ok)
				{
					encoder.Protected.State = Flac__StreamEncoderState.Client_Error;
					return;
				}
			}

			// Write seektable
			if ((encoder.Private.Seek_Table != null) && (encoder.Private.Seek_Table.Num_Points > 0) && (encoder.Protected.Seekable_Offset > 0))
			{
				Format.Flac__Format_SeekTable_Sort(encoder.Private.Seek_Table);

				Debug.Assert(Format.Flac__Format_SeekTable_Is_Legal(encoder.Private.Seek_Table));

				if ((seek_Status = encoder.Private.Seek_Callback(this, encoder.Protected.Seekable_Offset + Constants.Flac__Stream_Metadata_Header_Length, encoder.Private.Client_Data)) != Flac__StreamEncoderSeekStatus.Ok)
				{
					if (seek_Status == Flac__StreamEncoderSeekStatus.Error)
						encoder.Protected.State = Flac__StreamEncoderState.Client_Error;

					return;
				}

				for (uint32_t i = 0; i < encoder.Private.Seek_Table.Num_Points; i++)
				{
					Flac__uint64 xx = encoder.Private.Seek_Table.Points[i].Sample_Number;
					b[7] = (Flac__byte)xx; xx >>= 8;
					b[6] = (Flac__byte)xx; xx >>= 8;
					b[5] = (Flac__byte)xx; xx >>= 8;
					b[4] = (Flac__byte)xx; xx >>= 8;
					b[3] = (Flac__byte)xx; xx >>= 8;
					b[2] = (Flac__byte)xx; xx >>= 8;
					b[1] = (Flac__byte)xx; xx >>= 8;
					b[0] = (Flac__byte)xx;

					xx = encoder.Private.Seek_Table.Points[i].Stream_Offset;
					b[15] = (Flac__byte)xx; xx >>= 8;
					b[14] = (Flac__byte)xx; xx >>= 8;
					b[13] = (Flac__byte)xx; xx >>= 8;
					b[12] = (Flac__byte)xx; xx >>= 8;
					b[11] = (Flac__byte)xx; xx >>= 8;
					b[10] = (Flac__byte)xx; xx >>= 8;
					b[9] = (Flac__byte)xx; xx >>= 8;
					b[8] = (Flac__byte)xx;

					uint32_t x = encoder.Private.Seek_Table.Points[i].Frame_Samples;
					b[17] = (Flac__byte)x; x >>= 8;
					b[16] = (Flac__byte)x;

					if (encoder.Private.Write_Callback(this, b, 18, 0, 0, encoder.Private.Client_Data) != Flac__StreamEncoderWriteStatus.Ok)
					{
						encoder.Protected.State = Flac__StreamEncoderState.Client_Error;
						return;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Process_Frame(Flac__bool is_Fractional_Block, Flac__bool is_Last_Block)
		{
			Debug.Assert(encoder.Protected.State == Flac__StreamEncoderState.Ok);

			// Accumulate raw signal to the MD5 signature
			if (encoder.Protected.Do_Md5 && !encoder.Private.Md5.Flac__Md5Accumulate(encoder.Private.Integer_Signal, encoder.Protected.Channels, encoder.Protected.BlockSize, (encoder.Protected.Bits_Per_Sample + 7) / 8))
			{
				encoder.Protected.State = Flac__StreamEncoderState.Memory_Allocation_Error;
				return false;
			}

			// Process the frame header and subframes into the frame bitbuffer
			if (!Process_SubFrames(is_Fractional_Block))
			{
				// The above function sets the state for us in case of an error
				return false;
			}

			// Zero-pad the frame to a byte_boundary
			if (!encoder.Private.Frame.Flac__BitWriter_Zero_Pad_To_Byte_Boundary())
			{
				encoder.Protected.State = Flac__StreamEncoderState.Memory_Allocation_Error;
				return false;
			}

			// CRC-16 the whole thing
			Debug.Assert(encoder.Private.Frame.Flac__BitWriter_Is_Byte_Aligned());

			if (!encoder.Private.Frame.Flac__BitWriter_Get_Write_Crc16(out Flac__uint16 crc) || !encoder.Private.Frame.Flac__BitWriter_Write_Raw_UInt32(crc, Constants.Flac__Frame_Footer_Crc_Len))
			{
				encoder.Protected.State = Flac__StreamEncoderState.Memory_Allocation_Error;
				return false;
			}

			// Write it
			if (!Write_BitBuffer(encoder.Protected.BlockSize, is_Last_Block))
			{
				// The above function sets the state for us in case of an error
				return false;
			}

			// Get ready for the next frame
			encoder.Private.Current_Sample_Number = 0;
			encoder.Private.Current_Frame_Number++;
			((Flac__StreamMetadata_StreamInfo)encoder.Private.StreamInfo.Data).Total_Samples += encoder.Protected.BlockSize;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Process_SubFrames(Flac__bool is_Fractional_Block)
		{
			uint32_t min_Partition_Order = encoder.Protected.Min_Residual_Partition_Order;
			uint32_t max_Partition_Order;

			// Calculate the min,max Rice partition orders
			if (is_Fractional_Block)
				max_Partition_Order = 0;
			else
			{
				max_Partition_Order = Format.Flac__Format_Get_Max_Rice_Partition_Order_From_BlockSize(encoder.Protected.BlockSize);
				max_Partition_Order = Math.Min(max_Partition_Order, encoder.Protected.Max_Residual_Partition_Order);
			}

			min_Partition_Order = Math.Min(min_Partition_Order, max_Partition_Order);

			// Setup the frame
			Flac__FrameHeader frame_Header = new Flac__FrameHeader();
			frame_Header.BlockSize = encoder.Protected.BlockSize;
			frame_Header.Sample_Rate = encoder.Protected.Sample_Rate;
			frame_Header.Channels = encoder.Protected.Channels;
			frame_Header.Channel_Assignment = Flac__ChannelAssignment.Independent;	// The default unless the encoder determines otherwise
			frame_Header.Bits_Per_Sample = encoder.Protected.Bits_Per_Sample;
			frame_Header.Number_Type = Flac__FrameNumberType.Frame_Number;
			frame_Header.Frame_Number = encoder.Private.Current_Frame_Number;

			// Figure out what channel assignments to try
			Flac__bool do_Independent, do_Mid_Side;

			if (encoder.Protected.Do_Mid_Side_Stereo)
			{
				if (encoder.Protected.Loose_Mid_Side_Stereo)
				{
					if (encoder.Private.Loose_Mid_Side_Stereo_Frame_Count == 0)
					{
						do_Independent = true;
						do_Mid_Side = true;
					}
					else
					{
						do_Independent = encoder.Private.Last_Channel_Assignment == Flac__ChannelAssignment.Independent;
						do_Mid_Side = !do_Independent;
					}
				}
				else
				{
					do_Independent = true;
					do_Mid_Side = true;
				}
			}
			else
			{
				do_Independent = true;
				do_Mid_Side = false;
			}

			Debug.Assert(do_Independent || do_Mid_Side);

			// Check for wasted bits; set effective bps for each subframe
			if (do_Independent)
			{
				for (uint32_t channel = 0; channel < encoder.Protected.Channels; channel++)
				{
					uint32_t w = Get_Wasted_Bits(encoder.Private.Integer_Signal[channel], encoder.Protected.BlockSize);

					if (w > encoder.Protected.Bits_Per_Sample)
						w = encoder.Protected.Bits_Per_Sample;

					encoder.Private.SubFrame_Workspace[channel][0].Wasted_Bits = encoder.Private.SubFrame_Workspace[channel][1].Wasted_Bits = w;
					encoder.Private.SubFrame_Bps[channel] = encoder.Protected.Bits_Per_Sample - w;
				}
			}

			if (do_Mid_Side)
			{
				Debug.Assert(encoder.Protected.Channels == 2);

				for (uint32_t channel = 0; channel < 2; channel++)
				{
					uint32_t w = Get_Wasted_Bits(encoder.Private.Integer_Signal_Mid_Size[channel], encoder.Protected.BlockSize);

					if (w > encoder.Protected.Bits_Per_Sample)
						w = encoder.Protected.Bits_Per_Sample;

					encoder.Private.SubFrame_Workspace_Mid_Side[channel][0].Wasted_Bits = encoder.Private.SubFrame_Workspace_Mid_Side[channel][1].Wasted_Bits = w;
					encoder.Private.SubFrame_Bps_Mid_Side[channel] = encoder.Protected.Bits_Per_Sample - w + (channel == 0 ? 0U : 1);
				}
			}

			// First do a normal encoding pass of each independent channel
			if (do_Independent)
			{
				for (uint32_t channel = 0; channel < encoder.Protected.Channels; channel++)
				{
					if (!Process_SubFrame(min_Partition_Order, max_Partition_Order, frame_Header, encoder.Private.SubFrame_Bps[channel], encoder.Private.Integer_Signal[channel],
						    encoder.Private.SubFrame_Workspace_Ptr[channel], encoder.Private.Partitioned_Rice_Contents_Workspace_Ptr[channel], encoder.Private.Residual_Workspace[channel],
						    out encoder.Private.Best_SubFrame[channel], out encoder.Private.Best_SubFrame_Bits[channel]))
					{
						return false;
					}
				}
			}

			// Now do mid and side channels if requested
			if (do_Mid_Side)
			{
				Debug.Assert(encoder.Protected.Channels == 2);

				for (uint32_t channel = 0; channel < 2; channel++)
				{
					if (!Process_SubFrame(min_Partition_Order, max_Partition_Order, frame_Header, encoder.Private.SubFrame_Bps_Mid_Side[channel], encoder.Private.Integer_Signal_Mid_Size[channel],
						    encoder.Private.SubFrame_Workspace_Ptr_Mid_Side[channel], encoder.Private.Partitioned_Rice_Contents_Workspace_Ptr_Mid_Side[channel], encoder.Private.Residual_Workspace_Mid_Side[channel],
						    out encoder.Private.Best_SubFrame_Mid_Side[channel], out encoder.Private.Best_SubFrame_Bits_Mid_Side[channel]))
					{
						return false;
					}
				}
			}

			// Compose the frame bitbuffer
			if (do_Mid_Side)
			{
				uint32_t left_Bps = 0;
				uint32_t right_Bps = 0;
				Flac__SubFrame left_SubFrame = null;
				Flac__SubFrame right_SubFrame = null;
				Flac__ChannelAssignment channel_Assignment;

				Debug.Assert(encoder.Protected.Channels == 2);

				if (encoder.Protected.Loose_Mid_Side_Stereo && (encoder.Private.Loose_Mid_Side_Stereo_Frame_Count > 0))
					channel_Assignment = encoder.Private.Last_Channel_Assignment == Flac__ChannelAssignment.Independent ? Flac__ChannelAssignment.Independent : Flac__ChannelAssignment.Mid_Side;
				else
				{
					uint32_t[] bits = new uint32_t[4];	// WATCHOUT - indexed by Flac__ChannelAssignment

					Debug.Assert((int)Flac__ChannelAssignment.Independent == 0);
					Debug.Assert((int)Flac__ChannelAssignment.Left_Side == 1);
					Debug.Assert((int)Flac__ChannelAssignment.Right_Side == 2);
					Debug.Assert((int)Flac__ChannelAssignment.Mid_Side == 3);
					Debug.Assert(do_Independent || do_Mid_Side);

					// We have to figure out which channel assignment results in the smallest frame
					bits[(int)Flac__ChannelAssignment.Independent] = encoder.Private.Best_SubFrame_Bits[0] + encoder.Private.Best_SubFrame_Bits[1];
					bits[(int)Flac__ChannelAssignment.Left_Side] = encoder.Private.Best_SubFrame_Bits[0] + encoder.Private.Best_SubFrame_Bits_Mid_Side[1];
					bits[(int)Flac__ChannelAssignment.Right_Side] = encoder.Private.Best_SubFrame_Bits[1] + encoder.Private.Best_SubFrame_Bits_Mid_Side[1];
					bits[(int)Flac__ChannelAssignment.Mid_Side] = encoder.Private.Best_SubFrame_Bits_Mid_Side[0] + encoder.Private.Best_SubFrame_Bits_Mid_Side[1];

					channel_Assignment = Flac__ChannelAssignment.Independent;
					uint32_t min_Bits = bits[(int)channel_Assignment];

					for (uint32_t ca = 1; ca <= 3; ca++)
					{
						if (bits[ca] < min_Bits)
						{
							min_Bits = bits[ca];
							channel_Assignment = (Flac__ChannelAssignment)ca;
						}
					}
				}

				frame_Header.Channel_Assignment = channel_Assignment;

				if (!Stream_Encoder_Framing.Flac__Frame_Add_Header(frame_Header, encoder.Private.Frame))
				{
					encoder.Protected.State = Flac__StreamEncoderState.Framing_Error;
					return false;
				}

				switch (channel_Assignment)
				{
					case Flac__ChannelAssignment.Independent:
					{
						left_SubFrame = encoder.Private.SubFrame_Workspace[0][encoder.Private.Best_SubFrame[0]];
						right_SubFrame = encoder.Private.SubFrame_Workspace[1][encoder.Private.Best_SubFrame[1]];
						break;
					}

					case Flac__ChannelAssignment.Left_Side:
					{
						left_SubFrame = encoder.Private.SubFrame_Workspace[0][encoder.Private.Best_SubFrame[0]];
						right_SubFrame = encoder.Private.SubFrame_Workspace_Mid_Side[1][encoder.Private.Best_SubFrame_Mid_Side[1]];
						break;
					}

					case Flac__ChannelAssignment.Right_Side:
					{
						left_SubFrame = encoder.Private.SubFrame_Workspace_Mid_Side[1][encoder.Private.Best_SubFrame_Mid_Side[1]];
						right_SubFrame = encoder.Private.SubFrame_Workspace[1][encoder.Private.Best_SubFrame[1]];
						break;
					}

					case Flac__ChannelAssignment.Mid_Side:
					{
						left_SubFrame = encoder.Private.SubFrame_Workspace_Mid_Side[0][encoder.Private.Best_SubFrame_Mid_Side[0]];
						right_SubFrame = encoder.Private.SubFrame_Workspace_Mid_Side[1][encoder.Private.Best_SubFrame_Mid_Side[1]];
						break;
					}

					default:
					{
						Debug.Assert(false);
						break;
					}
				}

				switch (channel_Assignment)
				{
					case Flac__ChannelAssignment.Independent:
					{
						left_Bps = encoder.Private.SubFrame_Bps[0];
						right_Bps = encoder.Private.SubFrame_Bps[1];
						break;
					}

					case Flac__ChannelAssignment.Left_Side:
					{
						left_Bps = encoder.Private.SubFrame_Bps[0];
						right_Bps = encoder.Private.SubFrame_Bps_Mid_Side[1];
						break;
					}

					case Flac__ChannelAssignment.Right_Side:
					{
						left_Bps = encoder.Private.SubFrame_Bps_Mid_Side[1];
						right_Bps = encoder.Private.SubFrame_Bps[1];
						break;
					}

					case Flac__ChannelAssignment.Mid_Side:
					{
						left_Bps = encoder.Private.SubFrame_Bps_Mid_Side[0];
						right_Bps = encoder.Private.SubFrame_Bps_Mid_Side[1];
						break;
					}

					default:
					{
						Debug.Assert(false);
						break;
					}
				}

				// Note that encoder_Add_SubFrame sets the state for us in case of an error
				if (!Add_SubFrame(frame_Header.BlockSize, left_Bps, left_SubFrame, encoder.Private.Frame))
					return false;

				if (!Add_SubFrame(frame_Header.BlockSize, right_Bps, right_SubFrame, encoder.Private.Frame))
					return false;
			}
			else
			{
				if (!Stream_Encoder_Framing.Flac__Frame_Add_Header(frame_Header, encoder.Private.Frame))
				{
					encoder.Protected.State = Flac__StreamEncoderState.Framing_Error;
					return false;
				}

				for (uint32_t channel = 0; channel < encoder.Protected.Channels; channel++)
				{
					if (!Add_SubFrame(frame_Header.BlockSize, encoder.Private.SubFrame_Bps[channel], encoder.Private.SubFrame_Workspace[channel][encoder.Private.Best_SubFrame[channel]], encoder.Private.Frame))
					{
						// The above function sets the state for us in case of an error
						return false;
					}
				}
			}

			if (encoder.Protected.Loose_Mid_Side_Stereo)
			{
				encoder.Private.Loose_Mid_Side_Stereo_Frame_Count++;

				if (encoder.Private.Loose_Mid_Side_Stereo_Frame_Count >= encoder.Private.Loose_Mid_Side_Stereo_Frames)
					encoder.Private.Loose_Mid_Side_Stereo_Frame_Count = 0;
			}

			encoder.Private.Last_Channel_Assignment = frame_Header.Channel_Assignment;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Process_SubFrame(uint32_t min_Partition_Order, uint32_t max_Partition_Order, Flac__FrameHeader frame_Header, uint32_t subFrame_Bps, Flac__int32[] integer_Signal, Flac__SubFrame[] subFrame, Flac__EntropyCodingMethod_PartitionedRiceContents[] partitioned_Rice_Contents, Flac__int32[][] residual, out uint32_t best_SubFrame, out uint32_t best_Bits)
		{
			float[] fixed_Residual_Bits_Per_Sample = new float[Constants.Flac__Max_Fixed_Order + 1];
			Flac__real[] autoc = new Flac__real[Constants.Flac__Max_Lpc_Order + 1];		// WATHOUT: The size is important even though encoder.Protected.Max_Lpc_Order might be less; some asm and x86 intrinsic routines need all the space
			double[] lpc_Error = new double[Constants.Flac__Max_Lpc_Order];

			// Only use RICE2 partitions if stream bps > 16
			uint32_t rice_Parameter_Limit = Flac__Stream_Encoder_Get_Bits_Per_Sample() > 16 ? Constants.Flac__Entropy_Coding_Method_Partitioned_Rice2_Escape_Parameter : Constants.Flac__Entropy_Coding_Method_Partitioned_Rice_Escape_Parameter;

			Debug.Assert(frame_Header.BlockSize > 0);

			// Verbatim subframe is the baseline against which we measure other compressed subframes
			uint32_t _best_Bits;
			uint32_t _best_SubFrame = 0;

			if (encoder.Private.Disable_Verbatim_SubFrames && (frame_Header.BlockSize >= Constants.Flac__Max_Fixed_Order))
				_best_Bits = uint32_t.MaxValue;
			else
				_best_Bits = Evaluate_Verbatim_SubFrame(integer_Signal, frame_Header.BlockSize, subFrame_Bps, subFrame[_best_SubFrame]);

			if (frame_Header.BlockSize >= Constants.Flac__Max_Fixed_Order)
			{
				uint32_t guess_Fixed_Order;
				Flac__bool signal_Is_Constant = false;

				if ((subFrame_Bps + 4 + BitMath.Flac__BitMath_ILog2((frame_Header.BlockSize - Constants.Flac__Max_Fixed_Order) | 1) <= 32))
					guess_Fixed_Order = encoder.Private.Fixed.Compute_Best_Predictor(integer_Signal, Constants.Flac__Max_Fixed_Order, frame_Header.BlockSize - Constants.Flac__Max_Fixed_Order, fixed_Residual_Bits_Per_Sample);
				else
					guess_Fixed_Order = encoder.Private.Fixed.Compute_Best_Predictor_Wide(integer_Signal, Constants.Flac__Max_Fixed_Order, frame_Header.BlockSize - Constants.Flac__Max_Fixed_Order, fixed_Residual_Bits_Per_Sample);

				// Check for constant subframe
				if (!encoder.Private.Disable_Constant_SubFrames && (fixed_Residual_Bits_Per_Sample[1] == 0.0))
				{
					// The above means it's possible all samples are the same value; now double-check it
					signal_Is_Constant = true;

					for (uint32_t i = 1; i < frame_Header.BlockSize; i++)
					{
						if (integer_Signal[0] != integer_Signal[i])
						{
							signal_Is_Constant = false;
							break;
						}
					}
				}

				if (signal_Is_Constant)
				{
					uint32_t candidate_Bits = Evaluate_Constant_SubFrame(integer_Signal[0], frame_Header.BlockSize, subFrame_Bps, subFrame[_best_SubFrame == 0 ? 1U : 0]);
					if (candidate_Bits < _best_Bits)
					{
						_best_SubFrame = _best_SubFrame == 0 ? 1U : 0;
						_best_Bits = candidate_Bits;
					}
				}
				else
				{
					if (!encoder.Private.Disable_Fixed_SubFrames || ((encoder.Protected.Max_Lpc_Order == 0) && (_best_Bits == uint32_t.MaxValue)))
					{
						uint32_t fixed_Order, min_Fixed_Order, max_Fixed_Order;

						// Encode fixed
						if (encoder.Protected.Do_Exhaustive_Model_Search)
						{
							min_Fixed_Order = 0;
							max_Fixed_Order = Constants.Flac__Max_Fixed_Order;
						}
						else
							min_Fixed_Order = max_Fixed_Order = guess_Fixed_Order;

						if (max_Fixed_Order >= frame_Header.BlockSize)
							max_Fixed_Order = frame_Header.BlockSize - 1;

						for (fixed_Order = min_Fixed_Order; fixed_Order <= max_Fixed_Order; fixed_Order++)
						{
							if (fixed_Residual_Bits_Per_Sample[fixed_Order] >= subFrame_Bps)
								continue;	// Don't even try

							uint32_t rice_Parameter = fixed_Residual_Bits_Per_Sample[fixed_Order] > 0.0f ? (uint32_t)(fixed_Residual_Bits_Per_Sample[fixed_Order] + 0.5f) : 0;	// 0.5 is for rounding
							rice_Parameter++;	// To account for the signed->uint32_t conversion during rice coding

							if (rice_Parameter >= rice_Parameter_Limit)
								rice_Parameter = rice_Parameter_Limit - 1;

							uint32_t invertBest_SubFrame = _best_SubFrame == 0 ? 1U : 0;
							uint32_t candidate_Bits = Evaluate_Fixed_SubFrame(integer_Signal, residual[invertBest_SubFrame], encoder.Private.Abs_Residual_Partition_Sums, encoder.Private.Raw_Bits_Per_Partition, frame_Header.BlockSize, subFrame_Bps, fixed_Order, rice_Parameter, rice_Parameter_Limit, min_Partition_Order, max_Partition_Order, subFrame[invertBest_SubFrame], partitioned_Rice_Contents[invertBest_SubFrame]);
							if (candidate_Bits < _best_Bits)
							{
								_best_SubFrame = _best_SubFrame == 0 ? 1U : 0;
								_best_Bits = candidate_Bits;
							}
						}
					}

					// Encode lpc
					if (encoder.Protected.Max_Lpc_Order > 0)
					{
						uint32_t max_Lpc_Order;

						if (encoder.Protected.Max_Lpc_Order >= frame_Header.BlockSize)
							max_Lpc_Order = frame_Header.BlockSize - 1;
						else
							max_Lpc_Order = encoder.Protected.Max_Lpc_Order;

						if (max_Lpc_Order > 0)
						{
							uint32_t min_Lpc_Order;

							for (uint32_t a = 0; a < encoder.Protected.Num_Apodizations; a++)
							{
								Lpc.Flac__Lpc_Window_Data(integer_Signal, encoder.Private.Window[a], encoder.Private.Windowed_Signal, frame_Header.BlockSize);

								encoder.Private.Lpc.Compute_Autocorrelation(encoder.Private.Windowed_Signal, frame_Header.BlockSize, max_Lpc_Order + 1, autoc);

								// If autoc[0] == 0.0, the signal is constant and we usually won't get here, but it can happen
								if (autoc[0] != 0.0)
								{
									Lpc.Flac__Lpc_Compute_Lp_Coefficients(autoc, ref max_Lpc_Order, encoder.Private.Lp_Coeff, lpc_Error);
									if (encoder.Protected.Do_Exhaustive_Model_Search)
										min_Lpc_Order = 1;
									else
									{
										uint32_t guess_Lpc_Order = Lpc.Flac__Lpc_Compute_Best_Order(lpc_Error, max_Lpc_Order, frame_Header.BlockSize, subFrame_Bps + (encoder.Protected.Do_Qlp_Coeff_Prec_Search ? Constants.Flac__Min_Qlp_Coeff_Precision : /* have to guess; use the min possible size to avoid accidentally favoring lower orders*/ encoder.Protected.Qlp_Coeff_Precision));
										min_Lpc_Order = max_Lpc_Order = guess_Lpc_Order;
									}

									if (max_Lpc_Order >= frame_Header.BlockSize)
										max_Lpc_Order = frame_Header.BlockSize - 1;

									for (uint32_t lpc_Order = min_Lpc_Order; lpc_Order <= max_Lpc_Order; lpc_Order++)
									{
										double lpc_Residual_Bits_Per_Sample = Lpc.Flac__Lpc_Compute_Expected_Bits_Per_Residual_Sample(lpc_Error[lpc_Order - 1], frame_Header.BlockSize - lpc_Order);
										if (lpc_Residual_Bits_Per_Sample >= subFrame_Bps)
											continue;	// Don't even try

										uint32_t rice_Parameter = lpc_Residual_Bits_Per_Sample > 0.0f ? (uint32_t)(lpc_Residual_Bits_Per_Sample + 0.5f) : 0;	// 0.5 is for rounding
										rice_Parameter++;	// To account for the signed->uint32_t conversion during rice coding

										if (rice_Parameter >= rice_Parameter_Limit)
											rice_Parameter = rice_Parameter_Limit - 1;

										uint32_t min_Qlp_Coeff_Precision, max_Qlp_Coeff_Precision;

										if (encoder.Protected.Do_Qlp_Coeff_Prec_Search)
										{
											min_Qlp_Coeff_Precision = Constants.Flac__Min_Qlp_Coeff_Precision;

											// Try to keep qlp coeff precision such that only 32-bit math is required for decode of <=16bps(+1bps for side channel) streams
											if (subFrame_Bps <= 17)
											{
												max_Qlp_Coeff_Precision = Math.Min(32 - subFrame_Bps - BitMath.Flac__BitMath_ILog2(lpc_Order), Constants.Flac__Max_Qlp_Coeff_Precision);
												max_Qlp_Coeff_Precision = Math.Max(max_Qlp_Coeff_Precision, min_Qlp_Coeff_Precision);
											}
											else
												max_Qlp_Coeff_Precision = Constants.Flac__Max_Qlp_Coeff_Precision;
										}
										else
											min_Qlp_Coeff_Precision = max_Qlp_Coeff_Precision = encoder.Protected.Qlp_Coeff_Precision;

										for (uint32_t qlp_Coeff_Precision = min_Qlp_Coeff_Precision; qlp_Coeff_Precision <= max_Qlp_Coeff_Precision; qlp_Coeff_Precision++)
										{
											uint32_t invertBest_SubFrame = _best_SubFrame == 0 ? 1U : 0;
											uint32_t candidate_Bits = Evaluate_Lpc_SubFrame(integer_Signal, residual[invertBest_SubFrame], encoder.Private.Abs_Residual_Partition_Sums, encoder.Private.Raw_Bits_Per_Partition, encoder.Private.Lp_Coeff[lpc_Order - 1], frame_Header.BlockSize, subFrame_Bps, lpc_Order, qlp_Coeff_Precision, rice_Parameter, rice_Parameter_Limit, min_Partition_Order, max_Partition_Order, subFrame[invertBest_SubFrame], partitioned_Rice_Contents[invertBest_SubFrame]);
											if (candidate_Bits > 0)	// if == 0m there was a problem quantizing the lpcoeffs
											{
												if (candidate_Bits < _best_Bits)
												{
													_best_SubFrame = _best_SubFrame == 0 ? 1U : 0;
													_best_Bits = candidate_Bits;
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}

			// Under rare circumstances this can happen when all but lpc subframe types are disabled
			if (_best_Bits == uint32_t.MaxValue)
			{
				Debug.Assert(_best_SubFrame == 0);

				_best_Bits = Evaluate_Verbatim_SubFrame(integer_Signal, frame_Header.BlockSize, subFrame_Bps, subFrame[_best_SubFrame]);
			}

			best_SubFrame = _best_SubFrame;
			best_Bits = _best_Bits;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Add_SubFrame(uint32_t blockSize, uint32_t subFrame_Bps, Flac__SubFrame subFrame, BitWriter frame)
		{
			switch (subFrame.Type)
			{
				case Flac__SubFrameType.Constant:
				{
					if (!Stream_Encoder_Framing.Flac__SubFrame_Add_Constant((Flac__SubFrame_Constant)subFrame.Data, subFrame_Bps, subFrame.Wasted_Bits, frame))
					{
						encoder.Protected.State = Flac__StreamEncoderState.Framing_Error;
						return false;
					}
					break;
				}

				case Flac__SubFrameType.Fixed:
				{
					if (!Stream_Encoder_Framing.Flac__SubFrame_Add_Fixed((Flac__SubFrame_Fixed)subFrame.Data, blockSize - ((Flac__SubFrame_Fixed)subFrame.Data).Order, subFrame_Bps, subFrame.Wasted_Bits, frame))
					{
						encoder.Protected.State = Flac__StreamEncoderState.Framing_Error;
						return false;
					}
					break;
				}

				case Flac__SubFrameType.Lpc:
				{
					if (!Stream_Encoder_Framing.Flac__SubFrame_Add_Lpc((Flac__SubFrame_Lpc)subFrame.Data, blockSize - ((Flac__SubFrame_Lpc)subFrame.Data).Order, subFrame_Bps, subFrame.Wasted_Bits, frame))
					{
						encoder.Protected.State = Flac__StreamEncoderState.Framing_Error;
						return false;
					}
					break;
				}

				case Flac__SubFrameType.Verbatim:
				{
					if (!Stream_Encoder_Framing.Flac__SubFrame_Add_Verbatim((Flac__SubFrame_Verbatim)subFrame.Data, blockSize, subFrame_Bps, subFrame.Wasted_Bits, frame))
					{
						encoder.Protected.State = Flac__StreamEncoderState.Framing_Error;
						return false;
					}
					break;
				}

				default:
				{
					Debug.Assert(false);
					break;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint32_t Evaluate_Constant_SubFrame(Flac__int32 signal, uint32_t blockSize, uint32_t subFrame_Bps, Flac__SubFrame subFrame)
		{
			subFrame.Type = Flac__SubFrameType.Constant;

			Flac__SubFrame_Constant constant = new Flac__SubFrame_Constant();
			subFrame.Data = constant;
			constant.Value = signal;

			uint32_t estimate = Constants.Flac__SubFrame_Zero_Pad_Len + Constants.Flac__SubFrame_Type_Len + Constants.Flac__SubFrame_Wasted_Bits_Flag_Len + subFrame.Wasted_Bits + subFrame_Bps;

			return estimate;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint32_t Evaluate_Fixed_SubFrame(Flac__int32[] signal, Flac__int32[] residual, Flac__uint64[] abs_Residual_Partition_Sums, uint32_t[] raw_Bits_Per_Partition, uint32_t blockSize, uint32_t subFrame_Bps, uint32_t order, uint32_t rice_Parameter, uint32_t rice_Parameter_Limit, uint32_t min_Partition_Order, uint32_t max_Partition_Order, Flac__SubFrame subFrame, Flac__EntropyCodingMethod_PartitionedRiceContents partition_Rice_Contents)
		{
			uint32_t residual_Samples = blockSize - order;

			Fixed.Flac__Fixed_Compute_Residual(signal, order, residual_Samples, order, residual);

			subFrame.Type = Flac__SubFrameType.Fixed;

			Flac__SubFrame_Fixed @fixed = new Flac__SubFrame_Fixed();
			subFrame.Data = @fixed;
			@fixed.Entropy_Coding_Method.Type = Flac__EntropyCodingMethodType.Partitioned_Rice;

			Flac__EntropyCodingMethod_PartitionedRice partitionedRice = new Flac__EntropyCodingMethod_PartitionedRice();
			@fixed.Entropy_Coding_Method.Data = partitionedRice;
			partitionedRice.Contents = partition_Rice_Contents;
			@fixed.Residual = residual;

			uint32_t residual_Bits = Find_Best_Partition_Order(residual, abs_Residual_Partition_Sums, raw_Bits_Per_Partition, residual_Samples, order, rice_Parameter, rice_Parameter_Limit, min_Partition_Order, max_Partition_Order, subFrame_Bps, @fixed.Entropy_Coding_Method);

			@fixed.Order = order;

			for (uint32_t i = 0; i < order; i++)
				@fixed.Warmup[i] = signal[i];

			uint32_t estimate = Constants.Flac__SubFrame_Zero_Pad_Len + Constants.Flac__SubFrame_Type_Len + Constants.Flac__SubFrame_Wasted_Bits_Flag_Len + subFrame.Wasted_Bits + (order * subFrame_Bps) + residual_Bits;

			return estimate;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint32_t Evaluate_Lpc_SubFrame(Flac__int32[] signal, Flac__int32[] residual, Flac__uint64[] abs_Residual_Partition_Sums, uint32_t[] raw_Bits_Per_Partition, Flac__real[] lp_Coeff, uint32_t blockSize, uint32_t subFrame_Bps, uint32_t order, uint32_t qlp_Coeff_Precision, uint32_t rice_Parameter, uint32_t rice_Parameter_Limit, uint32_t min_Partition_Order, uint32_t max_Partition_Order, Flac__SubFrame subFrame, Flac__EntropyCodingMethod_PartitionedRiceContents partition_Rice_Contents)
		{
			Flac__int32[] qlp_Coeff = new Flac__int32[Constants.Flac__Max_Lpc_Order];	// WATCHOUT: The size is important; some x86 intrinsic routines need more than lpc order elements
			uint32_t residual_Samples = blockSize - order;

			// Try to keep qlp coeff precision such that only 32-bit math is required for decode of <=16bps(+1bps for side channel) streams
			if (subFrame_Bps <= 17)
			{
				Debug.Assert(order > 0);
				Debug.Assert(order <= Constants.Flac__Max_Lpc_Order);

				qlp_Coeff_Precision = Math.Min(qlp_Coeff_Precision, 32 - subFrame_Bps - BitMath.Flac__BitMath_ILog2(order));
			}

			int ret = Lpc.Flac__Lpc_Quantize_Coefficients(lp_Coeff, order, qlp_Coeff_Precision, qlp_Coeff, out int quantization);
			if (ret != 0)
				return 0;	// This is a hack to indicate to the caller that we can't do lp at this order on this subframe

			if ((subFrame_Bps + qlp_Coeff_Precision + BitMath.Flac__BitMath_ILog2(order)) <= 32)
			{
				if ((subFrame_Bps <= 16) && (qlp_Coeff_Precision <= 16))
					encoder.Private.Lpc.Compute_Residual_From_Qlp_Coefficients_16Bit(signal, order, residual_Samples, qlp_Coeff, order, quantization, residual);
				else
					encoder.Private.Lpc.Compute_Residual_From_Qlp_Coefficients(signal, order, residual_Samples, qlp_Coeff, order, quantization, residual);
			}
			else
				encoder.Private.Lpc.Compute_Residual_From_Qlp_Coefficients_64Bit(signal, order, residual_Samples, qlp_Coeff, order, quantization, residual);

			subFrame.Type = Flac__SubFrameType.Lpc;

			Flac__SubFrame_Lpc lpc = new Flac__SubFrame_Lpc();
			subFrame.Data = lpc;

			Flac__EntropyCodingMethod_PartitionedRice rice = new Flac__EntropyCodingMethod_PartitionedRice();

			lpc.Entropy_Coding_Method.Type = Flac__EntropyCodingMethodType.Partitioned_Rice;
			lpc.Entropy_Coding_Method.Data = rice;
			rice.Contents = partition_Rice_Contents;
			lpc.Residual = residual;

			uint32_t residual_Bits = Find_Best_Partition_Order(residual, abs_Residual_Partition_Sums, raw_Bits_Per_Partition, residual_Samples, order, rice_Parameter, rice_Parameter_Limit, min_Partition_Order, max_Partition_Order, subFrame_Bps, lpc.Entropy_Coding_Method);

			lpc.Order = order;
			lpc.Qlp_Coeff_Precision = qlp_Coeff_Precision;
			lpc.Quantization_Level = quantization;
			Array.Copy(qlp_Coeff, lpc.Qlp_Coeff, Constants.Flac__Max_Lpc_Order);

			for (uint32_t i = 0; i < order; i++)
				lpc.Warmup[i] = signal[i];

			uint32_t estimate = Constants.Flac__SubFrame_Zero_Pad_Len + Constants.Flac__SubFrame_Type_Len + Constants.Flac__SubFrame_Wasted_Bits_Flag_Len + subFrame.Wasted_Bits + Constants.Flac__SubFrame_Lpc_Qlp_Coeff_Precision_Len + Constants.Flac__SubFrame_Lpc_Qlp_Shift_Len + (order * (qlp_Coeff_Precision + subFrame_Bps)) + residual_Bits;

			return estimate;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint32_t Evaluate_Verbatim_SubFrame(Flac__int32[] signal, uint32_t blockSize, uint32_t subFrame_Bps, Flac__SubFrame subFrame)
		{
			subFrame.Type = Flac__SubFrameType.Verbatim;

			Flac__SubFrame_Verbatim verbatim = new Flac__SubFrame_Verbatim();
			subFrame.Data = verbatim;
			verbatim.Data = signal;

			uint32_t estimate = Constants.Flac__SubFrame_Zero_Pad_Len + Constants.Flac__SubFrame_Type_Len + Constants.Flac__SubFrame_Wasted_Bits_Flag_Len + subFrame.Wasted_Bits + (blockSize * subFrame_Bps);

			return estimate;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint32_t Find_Best_Partition_Order(Flac__int32[] residual, Flac__uint64[] abs_Residual_Partition_Sums, uint32_t[] raw_Bits_Per_Partition, uint32_t residual_Samples, uint32_t predictor_Order, uint32_t rice_Parameter, uint32_t rice_Parameter_Limit, uint32_t min_Partition_Order, uint32_t max_Partition_Order, uint32_t bps, Flac__EntropyCodingMethod best_Ecm)
		{
			uint32_t best_Residual_Bits = 0;
			uint32_t best_Parameters_Index = 0;
			uint32_t best_Partition_Order = 0;
			uint32_t blockSize = residual_Samples + predictor_Order;

			max_Partition_Order = Format.Flac__Format_Get_Max_Rice_Partition_Order_From_BlockSize_Limited_Max_And_Predictor_Order(max_Partition_Order, blockSize, predictor_Order);
			min_Partition_Order = Math.Min(min_Partition_Order, max_Partition_Order);

			Precompute_Partition_Info_Sums(residual, abs_Residual_Partition_Sums, residual_Samples, predictor_Order, min_Partition_Order, max_Partition_Order, bps);

			{
				for (int partition_Order = (int)max_Partition_Order, sum = 0; partition_Order >= (int)min_Partition_Order; partition_Order--)
				{
					if (!Set_Partitioned_Rice(abs_Residual_Partition_Sums, raw_Bits_Per_Partition, sum, residual_Samples, predictor_Order, rice_Parameter, rice_Parameter_Limit, (uint32_t)partition_Order, encoder.Private.Partitioned_Rice_Contents_Extra[best_Parameters_Index == 0 ? 1 : 0], out uint32_t residual_Bits))
					{
						Debug.Assert(best_Residual_Bits != 0);
						break;
					}

					sum += 1 << partition_Order;

					if ((best_Residual_Bits == 0) || (residual_Bits < best_Parameters_Index))
					{
						best_Residual_Bits = residual_Bits;
						best_Parameters_Index = best_Parameters_Index == 0 ? 1U : 0;
						best_Partition_Order = (uint32_t)partition_Order;
					}
				}
			}

			Flac__EntropyCodingMethod_PartitionedRice partitionedRice = new Flac__EntropyCodingMethod_PartitionedRice();
			partitionedRice.Contents = new Flac__EntropyCodingMethod_PartitionedRiceContents();
			best_Ecm.Data = partitionedRice;
			partitionedRice.Order = best_Partition_Order;

			{
				Flac__EntropyCodingMethod_PartitionedRiceContents prc = partitionedRice.Contents;

				// Save bes parameters and raw_Bits
				Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Ensure_Size(prc, Math.Max(6, best_Partition_Order));
				Array.Copy(encoder.Private.Partitioned_Rice_Contents_Extra[best_Parameters_Index].Parameters, 0, prc.Parameters, 0, 1 << (int)best_Partition_Order);

				// Now need to check if the type should be changed to
				// Flac__Entropy_Coding_Method_Partitioned_Rice2 based on the
				// size of the rice parameters
				for (uint32_t partition = 0; partition < 1 << (int)best_Partition_Order; partition++)
				{
					if (prc.Parameters[partition] >= Constants.Flac__Entropy_Coding_Method_Partitioned_Rice_Escape_Parameter)
					{
						best_Ecm.Type = Flac__EntropyCodingMethodType.Partitioned_Rice2;
						break;
					}
				}
			}

			return best_Residual_Bits;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected virtual void Precompute_Partition_Info_Sums(Flac__int32[] residual, Flac__uint64[] abs_Residual_Partition_Sums, uint32_t residual_Samples, uint32_t predictor_Order, uint32_t min_Partition_Order, uint32_t max_Partition_Order, uint32_t bps)
		{
			uint32_t default_Partition_Samples = (residual_Samples + predictor_Order) >> (int)max_Partition_Order;
			uint32_t partitions = 1U << (int)max_Partition_Order;

			Debug.Assert(default_Partition_Samples > predictor_Order);

			// First do max_Partition_Order
			{
				uint32_t threshold = 32 - BitMath.Flac__BitMath_ILog2(default_Partition_Samples);
				uint32_t end = (uint32_t)(-(int)predictor_Order);

				// WATCHOUT: "bps + FLAC__MAX_EXTRA_RESIDUAL_BPS" is the maximum assumed size of the average residual magnitude
				if ((bps + Constants.Flac__Max_Extra_Residual_Bps) < threshold)
				{
					for (uint32_t partition = 0, residual_Sample = 0; partition < partitions; partition++)
					{
						Flac__uint32 abs_Residual_Partition_Sum = 0;
						end += default_Partition_Samples;

						for (; residual_Sample < end; residual_Sample++)
							abs_Residual_Partition_Sum += (Flac__uint32)Math.Abs(residual[residual_Sample]);

						abs_Residual_Partition_Sums[partition] = abs_Residual_Partition_Sum;
					}
				}
				else	// Have to pessimistically use 64 bits for accumulator
				{
					for (uint32_t partition = 0, residual_Sample = 0; partition < partitions; partition++)
					{
						Flac__uint64 abs_Residual_Partition_Sum64 = 0;
						end += default_Partition_Samples;

						for (; residual_Sample < end; residual_Sample++)
							abs_Residual_Partition_Sum64 += (Flac__uint64)Math.Abs(residual[residual_Sample]);

						abs_Residual_Partition_Sums[partition] = abs_Residual_Partition_Sum64;
					}
				}
			}

			// Now merge partitions for lower orders
			{
				uint32_t from_Partition = 0;
				uint32_t to_Partition = partitions;

				for (int partition_Order = (int)max_Partition_Order - 1; partition_Order >= min_Partition_Order; partition_Order--)
				{
					partitions >>= 1;

					for (uint32_t i = 0; i < partitions; i++)
					{
						abs_Residual_Partition_Sums[to_Partition++] = abs_Residual_Partition_Sums[from_Partition] + abs_Residual_Partition_Sums[from_Partition + 1];
						from_Partition += 2;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint32_t Count_Rice_Bits_In_Partition(uint32_t rice_Parameter, uint32_t partition_Samples, Flac__uint64 abs_Residual_Partition_Sum)
		{
			return (uint32_t)(Math.Min(		// To make sure the return value doesn't overflow
				Constants.Flac__Entropy_Coding_Method_Partitioned_Rice_Parameter_Len +	// Actually could end up being Flac__Entropy_Coding_Method_Partitioned_Rice2_Parameter_Len but err on side of 16bps
				(1 + rice_Parameter) * partition_Samples +	// 1 for unary stop bit + rice_Parameter for the binary portion
				(
					rice_Parameter != 0 ?
						(abs_Residual_Partition_Sum >> (int)(rice_Parameter - 1))	// rice_Parameter-1 because the real coder sign-folds instead of using a sign bit
						: (abs_Residual_Partition_Sum << 1)		// Can't shift by negative number, so reverse
				)
				- (partition_Samples >> 1), uint32_t.MaxValue));
			// -(partition_Samples>>1) to subtract out extra contributions to the abs_Residual_Partition_Sum.
			// The actual number of bits used is closer to the sum(for all i in the partition) of abs(residual[i])>>(rice_Parameter-1)
			// By using the abs_Residual_Partition sum, we also add in bits in the LSBs that would normally be shifted out.
			// So the subtraction term tries to guess how many extra bits were contributed.
			// If the LSBs are randomly distributed, this should average to 0.5 extra bits per sample
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Set_Partitioned_Rice(Flac__uint64[] abs_Residual_Partition_Sums, uint32_t[] raw_Bits_Per_Partition, int offset, uint32_t residual_Samples, uint32_t predictor_Order, uint32_t suggested_Rice_Parameter, uint32_t rice_Parameter_Limit, uint32_t partition_Order, Flac__EntropyCodingMethod_PartitionedRiceContents partitioned_Rice_Contents, out uint32_t bits)
		{
			uint32_t best_Rice_Parameter = 0;
			uint32_t bits_ = Constants.Flac__Entropy_Coding_Method_Type_Len + Constants.Flac__Entropy_Coding_Method_Partitioned_Rice_Order_Len;

			Debug.Assert(suggested_Rice_Parameter < Constants.Flac__Entropy_Coding_Method_Partitioned_Rice2_Escape_Parameter);
			Debug.Assert(rice_Parameter_Limit <= Constants.Flac__Entropy_Coding_Method_Partitioned_Rice2_Escape_Parameter);

			Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Ensure_Size(partitioned_Rice_Contents, Math.Max(6, partition_Order));
			uint32_t[] parameters = partitioned_Rice_Contents.Parameters;
			uint32_t[] raw_Bits = partitioned_Rice_Contents.Raw_Bits;
			uint32_t rice_Parameter;
			uint32_t best_Partition_Bits;
			uint32_t partition_Bits;

			if (partition_Order == 0)
			{
				best_Partition_Bits = uint32_t.MaxValue;
				rice_Parameter = suggested_Rice_Parameter;
				partition_Bits = Count_Rice_Bits_In_Partition(rice_Parameter, residual_Samples, abs_Residual_Partition_Sums[0]);

				if (partition_Bits < best_Partition_Bits)
				{
					best_Rice_Parameter = rice_Parameter;
					best_Partition_Bits = partition_Bits;
				}

				parameters[0] = best_Rice_Parameter;

				if (best_Partition_Bits < uint32_t.MaxValue - bits_)	// To make sure _bits doesn't overflow
					bits_ += best_Partition_Bits;
				else
					bits_ = uint32_t.MaxValue;
			}
			else
			{
				uint32_t partitions = 1U << (int)partition_Order;

				for (uint32_t partition = 0, residual_Sample = 0; partition < partitions; partition++)
				{
					uint32_t partition_Samples = (residual_Samples + predictor_Order) >> (int)partition_Order;
					if (partition == 0)
					{
						if (partition_Samples <= predictor_Order)
						{
							bits = 0;
							return false;
						}
						else
							partition_Samples -= predictor_Order;
					}

					Flac__uint64 mean = abs_Residual_Partition_Sums[partition];

					// We are basically calculating the size in bits of the
					// average residual magnitude in the partition:
					//   rice_Parameter = floor(log2(mean/partition_Samples))
					// 'mean' is not a good name for the variable, it is
					// actually the sum of magnitudes of all residual values
					// in the partition, so the actual mean is
					// mean/partition_samples
					if (mean <= 0x80000000 / 512)
					{
						Flac__uint32 mean2 = (Flac__uint32)mean;
						rice_Parameter = 0;
						Flac__uint32 k2 = partition_Samples;

						while (k2 * 8 < mean2)	// Requires: mean <= (2^31)/8
						{
							rice_Parameter += 4;
							k2 <<= 4;		// Tuned for 16-bit input
						}

						while (k2 < mean2)	// Requires: mean <= 2^31
						{
							rice_Parameter++;
							k2 <<= 1;
						}
					}
					else
					{
						rice_Parameter = 0;
						Flac__uint64 k = partition_Samples;

						if (mean <= 0x8000000000000000 / 128)	// Usually mean is _much_ smaller than this value
						{
							while (k * 128 < mean)	// Requires: mean <= (2^63)/128
							{
								rice_Parameter += 8;
								k <<= 8;
							}
						}

						while (k < mean)	// Requires: mean <= 2^63
						{
							rice_Parameter++;
							k <<= 1;
						}
					}

					if (rice_Parameter >= rice_Parameter_Limit)
						rice_Parameter = rice_Parameter_Limit - 1;

					best_Partition_Bits = uint32_t.MaxValue;
					partition_Bits = Count_Rice_Bits_In_Partition(rice_Parameter, partition_Samples, abs_Residual_Partition_Sums[partition]);

					if (partition_Bits < best_Partition_Bits)
					{
						best_Rice_Parameter = rice_Parameter;
						best_Partition_Bits = partition_Bits;
					}

					parameters[partition] = best_Rice_Parameter;

					if (best_Partition_Bits < uint32_t.MaxValue - bits_)	// To make sure _bits doesn't overflow
						bits_ += best_Partition_Bits;
					else
						bits_ = uint32_t.MaxValue;

					residual_Sample += partition_Samples;
				}
			}

			bits = bits_;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint32_t Get_Wasted_Bits(Flac__int32[] signal, uint32_t samples)
		{
			Flac__int32 x = 0;
			uint32_t shift;

			for (uint32_t i = 0; (i < samples) && ((x & 1) == 0); i++)
				x |= signal[i];

			if (x == 0)
				shift = 0;
			else
			{
				for (shift = 0; (x & 1) == 0; shift++)
					x >>= 1;
			}

			if (shift > 0)
			{
				for (uint32_t i = 0; i < samples; i++)
					signal[i] >>= (int)shift;
			}

			return shift;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__StreamEncoderSeekStatus File_Seek_Callback(Stream_Encoder streamEncoder, Flac__uint64 absolute_Byte_Offset, object client_Data)
		{
			Flac__StreamEncoder encoder = streamEncoder.encoder;

			try
			{
				encoder.Private.File.Seek((long)absolute_Byte_Offset, SeekOrigin.Begin);

				return Flac__StreamEncoderSeekStatus.Ok;
			}
			catch(NotSupportedException)
			{
				return Flac__StreamEncoderSeekStatus.Unsupported;
			}
			catch(IOException)
			{
				return Flac__StreamEncoderSeekStatus.Error;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__StreamEncoderTellStatus File_Tell_Callback(Stream_Encoder streamEncoder, out Flac__uint64 absolute_Byte_Offset, object client_Data)
		{
			Flac__StreamEncoder encoder = streamEncoder.encoder;

			try
			{
				absolute_Byte_Offset = (Flac__uint64)encoder.Private.File.Position;

				return Flac__StreamEncoderTellStatus.Ok;
			}
			catch(NotSupportedException)
			{
				absolute_Byte_Offset = 0;
				return Flac__StreamEncoderTellStatus.Unsupported;
			}
			catch(IOException)
			{
				absolute_Byte_Offset = 0;
				return Flac__StreamEncoderTellStatus.Error;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__StreamEncoderWriteStatus File_Write_Callback(Stream_Encoder streamEncoder, Span<Flac__byte> buffer, size_t bytes, uint32_t samples, uint32_t current_Frame, object client_Data)
		{
			Flac__StreamEncoder encoder = streamEncoder.encoder;

			try
			{
				encoder.Private.File.Write(buffer.Slice(0, (int)bytes));

				Flac__bool call_it = (encoder.Private.Progress_Callback != null) && (samples > 0);
				if (call_it)
				{
					// NOTE: We have to add +bytes, +samples, and +1 to the stats
					// because at this point in the callback chain, the stats
					// have not been updated. Only after we return and control
					// gets back to Write_Frame() are the stats updated
					encoder.Private.Progress_Callback(streamEncoder, encoder.Private.Bytes_Written + bytes, encoder.Private.Samples_Written + samples, encoder.Private.Frames_Written + (samples != 0 ? 1U : 0), encoder.Private.Total_Frames_Estimate, encoder.Private.Client_Data);
				}

				return Flac__StreamEncoderWriteStatus.Ok;
			}
			catch(IOException)
			{
				return Flac__StreamEncoderWriteStatus.Fatal_Error;
			}
		}
		#endregion
	}
}
