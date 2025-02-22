/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers;
using Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Encoder;
using Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format;
using Polycode.NostalgicPlayer.Ports.LibFlac.Private;
using Polycode.NostalgicPlayer.Ports.LibFlac.Protected;
using Polycode.NostalgicPlayer.Ports.LibFlac.Protected.Containers;
using Polycode.NostalgicPlayer.Ports.LibFlac.Share;

namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac
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
	public class Stream_Encoder
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

		#region PThread_Cond_T class
		private class PThread_Cond_T : IDisposable
		{
			private readonly AutoResetEvent autoResetEvent;
			private readonly ManualResetEvent manualResetEvent;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public PThread_Cond_T()
			{
				autoResetEvent = new AutoResetEvent(false);
				manualResetEvent = new ManualResetEvent(false);
			}



			/********************************************************************/
			/// <summary>
			/// Cleanup
			/// </summary>
			/********************************************************************/
			public void Dispose()
			{
				autoResetEvent.Dispose();
				manualResetEvent.Dispose();
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public void Wait(Mutex mutex)
			{
				mutex.ReleaseMutex();
				WaitHandle.WaitAny([ autoResetEvent, manualResetEvent ]);
				mutex.WaitOne();
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public void Signal()
			{
				autoResetEvent.Set();
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public void Broadcast()
			{
				manualResetEvent.Set();
			}
		}
		#endregion

		private class Verify_Input_Fifo
		{
			public Flac__int32[][] Data = new Flac__int32[Constants.Flac__Max_Channels][];
			public uint32_t Size;	// of each data[] in samples
			public uint32_t Tail;
		}

		private class Apply_Apodization_State_Struct
		{
			public uint32_t A;
			public uint32_t B;
			public uint32_t C;
			public Flac__ApodizationSpecification Current_Apodization;
			public double[] Autoc_Root = new double[Constants.Flac__Max_Lpc_Order + 1];
			public double[] Autoc = new double[Constants.Flac__Max_Lpc_Order + 1];
		}

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

		/// <summary>
		/// Thread-private data
		/// </summary>
		private class Flac__StreamEncoderThreadTask
		{
			public Flac__int32[][] Integer_Signal = new Flac__int32[Constants.Flac__Max_Channels][];	// The integer version of the input signal
			public Flac__int32[][] Integer_Signal_Mid_Size = new Flac__int32[2][];						// The integer version of the mid-side input signal (stereo only)
			public Flac__int64[] Integer_Signal_33Bit_Side;												// 33-bit side for 32-bit stereo decorrelation
			public Flac__real[] Windowed_Signal;														// The Integer_Signal[] * current Window[]
			public uint32_t[] SubFrame_Bps = new uint32_t[Constants.Flac__Max_Channels];				// The effective bits per sample of the input signal (stream bps - wasted bits)
			public uint32_t[] SubFrame_Bps_Mid_Side = new uint32_t[2];									// The effective bits per sample of the mid-side input signal (stream bps - wasted bits + 0/1)
			public Flac__int32[][][] Residual_Workspace = ArrayHelper.InitializeArrayWithArray<Flac__int32>((int)Constants.Flac__Max_Channels, 2);// Each channel has a candidate and best workspace where the subframe residual signals will be stored
			public Flac__int32[][][] Residual_Workspace_Mid_Side = ArrayHelper.InitializeArrayWithArray<Flac__int32>(2, 2);
			public Flac__SubFrame[][] SubFrame_Workspace = ArrayHelper.InitializeArray<Flac__SubFrame>((int)Constants.Flac__Max_Channels, 2);
			public Flac__SubFrame[][] SubFrame_Workspace_Mid_Side = ArrayHelper.InitializeArray<Flac__SubFrame>(2, 2);
			public Flac__SubFrame[][] SubFrame_Workspace_Ptr = ArrayHelper.Initialize2Arrays<Flac__SubFrame>((int)Constants.Flac__Max_Channels, 2);
			public Flac__SubFrame[][] SubFrame_Workspace_Ptr_Mid_Side = ArrayHelper.Initialize2Arrays<Flac__SubFrame>(2, 2);
			public Flac__EntropyCodingMethod_PartitionedRiceContents[][] Partitioned_Rice_Contents_Workspace = ArrayHelper.InitializeArray<Flac__EntropyCodingMethod_PartitionedRiceContents>((int)Constants.Flac__Max_Channels, 2);
			public Flac__EntropyCodingMethod_PartitionedRiceContents[][] Partitioned_Rice_Contents_Workspace_Mid_Side = ArrayHelper.InitializeArray<Flac__EntropyCodingMethod_PartitionedRiceContents>((int)Constants.Flac__Max_Channels, 2);
			public Flac__EntropyCodingMethod_PartitionedRiceContents[][] Partitioned_Rice_Contents_Workspace_Ptr = ArrayHelper.Initialize2Arrays<Flac__EntropyCodingMethod_PartitionedRiceContents>((int)Constants.Flac__Max_Channels, 2);
			public Flac__EntropyCodingMethod_PartitionedRiceContents[][] Partitioned_Rice_Contents_Workspace_Ptr_Mid_Side = ArrayHelper.Initialize2Arrays<Flac__EntropyCodingMethod_PartitionedRiceContents>((int)Constants.Flac__Max_Channels, 2);
			public uint32_t[] Best_SubFrame = new uint32_t[Constants.Flac__Max_Channels];				// Index (0 or 1) into 2nd dimension of the above workspaces
			public uint32_t[] Best_SubFrame_Mid_Side = new uint32_t[2];
			public uint32_t[] Best_SubFrame_Bits = new uint32_t[Constants.Flac__Max_Channels];			// Size in bits of the best subframe for each channel
			public uint32_t[] Best_SubFrame_Bits_Mid_Side = new uint32_t[2];
			public Flac__uint64[] Abs_Residual_Partition_Sums;											// Workspace where the sum of abs(candidate residual) for each partition is stored
			public uint32_t[] Raw_Bits_Per_Partition;													// Workspace where the sum of silog2(candidate residual) for each partition is stored
			public BitWriter Frame;																		// The current frame being worked on
			public uint32_t Current_Frame_Number;
			public Flac__int32[][] Integer_Signal_Unaligned = new Flac__int32[Constants.Flac__Max_Channels][];
			public Flac__int32[][] Integer_Signal_Mid_Side_Unaligned = new Flac__int32[2][];
			public Flac__int64[] Integer_Signal_33Bit_Side_Unaligned;
			public Flac__real[] Windowed_Signal_Unaligned;
			public Flac__int32[][][] Residual_Workspace_Unaligned = ArrayHelper.InitializeArrayWithArray<Flac__int32>((int)Constants.Flac__Max_Channels, 2);
			public Flac__int32[][][] Residual_Workspace_Mid_Side_Unaligned = ArrayHelper.InitializeArrayWithArray<Flac__int32>(2, 2);
			public Flac__uint64[] Abs_Residual_Partition_Sums_Unaligned;
			public Flac__uint64[] Raw_Bits_Per_Partition_Unaligned;
			public Flac__real[][] Lp_Coeff = ArrayHelper.InitializeArray<Flac__real>((int)Constants.Flac__Max_Lpc_Order, (int)Constants.Flac__Max_Lpc_Order);	// From Process_SubFrame()
			public Flac__EntropyCodingMethod_PartitionedRiceContents[] Partitioned_Rice_Contents_Extra = ArrayHelper.InitializeArray<Flac__EntropyCodingMethod_PartitionedRiceContents>(2);	// From Find_Best_Partition_Order()
			public Flac__bool Disable_Constant_SubFrames;
			public Mutex Mutex_This_Task;						// To lock whole threadtask
			public PThread_Cond_T Cond_Task_Done;
			public Flac__bool Task_Done;
			public Flac__bool ReturnValue;
		}

		private class Flac__StreamEncoder
		{
			public Flac__StreamEncoderProtected Protected;
			public Flac__StreamEncoderPrivate Private;
		}

		/// <summary>
		/// Private class data
		/// </summary>
		private class Flac__StreamEncoderPrivate
		{
			public Flac__StreamEncoderThreadTask[] ThreadTask = new Flac__StreamEncoderThreadTask[Constants.Flac__Stream_Encoder_Max_ThreadTasks];
			public Thread[] Thread = new Thread[Constants.Flac__Stream_Encoder_Max_Threads];
			public uint32_t Input_Capacity;																// Current size (in samples) of the signal and residual buffers
			public Flac__real[][] Window = new Flac__real[Constants.Flac__Max_Apodization_Functions][];	// The pre-computed floating-point window for each apodization function
			public Flac__real[][] Window_Unaligned = new Flac__real[Constants.Flac__Max_Apodization_Functions][];
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
			public Stream File;									// Only used when encoding to a file
			public bool Leave_Stream_Open;
			public Flac__uint64 Bytes_Written;
			public Flac__uint64 Samples_Written;
			public uint32_t Frames_Written;
			public uint32_t Total_Frames_Estimate;
			public Flac__bool Is_Being_Deleted;					// If true, call to ..._Finish() from ..._Delete() will not call the callbacks
			public uint32_t Num_ThreadTasks;
			public uint32_t Num_Created_Threads;
			public uint32_t Next_Thread;						// This is the next thread that needs start, or needs to finish and be restarted
			public uint32_t Num_Started_ThreadTasks;
			public uint32_t Num_Available_ThreadTasks;          // Number of threadtasks that are available to work on
			public uint32_t Num_Running_Threads;
			public uint32_t Next_ThreadTask;					// Next threadtask that is available to work on
			public Mutex Mutex_Md5_Fifo;
			public Mutex Mutex_Work_Queue;						// To lock work related variables in this struct
			public PThread_Cond_T Cond_Md5_Emptied;				// To signal to main thread that MD5 queue has been emptied
			public PThread_Cond_T Cond_Work_Available;			// To signal to threads that work is available
			public PThread_Cond_T Cond_Wake_Up_Thread;			// To signal that one sleeping thread can wake up
			public Flac__bool Md5_Active;
			public Flac__bool Finish_Work_Threads;
			public int32_t Overcomitted_Indicator;
			public Verify_Input_Fifo Md5_Fifo = new Verify_Input_Fifo();
		}

		private static readonly CompressionLevels[] compression_Levels =
		[
			new CompressionLevels(false, false, 0, 0, false, false, 0, 3, "tukey(5e-1)"),
			new CompressionLevels(true, true, 0, 0, false, false, 0, 3, "tukey(5e-1)"),
			new CompressionLevels(true, false, 0, 0, false, false, 0, 3, "tukey(5e-1)"),
			new CompressionLevels(false, false, 6, 0, false, false, 0, 4, "tukey(5e-1)"),
			new CompressionLevels(true, true, 8, 0, false, false, 0, 4, "tukey(5e-1)"),
			new CompressionLevels(true, false, 8, 0, false, false, 0, 5, "tukey(5e-1)"),
			new CompressionLevels(true, false, 8, 0, false, false, 0, 6, "subdivide_tukey(2)"),
			new CompressionLevels(true, false, 12, 0, false, false, 0, 6, "subdivide_tukey(2)"),
			new CompressionLevels(true, false, 12, 0, false, false, 0, 6, "subdivide_tukey(3)")
		];

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

			encoder.encoder.Private.ThreadTask[0] = new Flac__StreamEncoderThreadTask();

			encoder.encoder.Private.ThreadTask[0].Frame = BitWriter.Flac__BitWriter_New();
			if (encoder.encoder.Private.ThreadTask[0].Frame == null)
				return null;

			encoder.encoder.Private.File = null;

			encoder.encoder.Protected.State = Flac__StreamEncoderState.Uninitialized;

			Set_Defaults(encoder);

			encoder.encoder.Private.Is_Being_Deleted = false;

			for (uint32_t i = 0; i < Constants.Flac__Max_Channels; i++)
			{
				encoder.encoder.Private.ThreadTask[0].SubFrame_Workspace_Ptr[i][0] = encoder.encoder.Private.ThreadTask[0].SubFrame_Workspace[i][0];
				encoder.encoder.Private.ThreadTask[0].SubFrame_Workspace_Ptr[i][1] = encoder.encoder.Private.ThreadTask[0].SubFrame_Workspace[i][1];
			}

			for (uint32_t i = 0; i < 2; i++)
			{
				encoder.encoder.Private.ThreadTask[0].SubFrame_Workspace_Ptr_Mid_Side[i][0] = encoder.encoder.Private.ThreadTask[0].SubFrame_Workspace_Mid_Side[i][0];
				encoder.encoder.Private.ThreadTask[0].SubFrame_Workspace_Ptr_Mid_Side[i][1] = encoder.encoder.Private.ThreadTask[0].SubFrame_Workspace_Mid_Side[i][1];
			}

			for (uint32_t i = 0; i < Constants.Flac__Max_Channels; i++)
			{
				encoder.encoder.Private.ThreadTask[0].Partitioned_Rice_Contents_Workspace_Ptr[i][0] = encoder.encoder.Private.ThreadTask[0].Partitioned_Rice_Contents_Workspace[i][0];
				encoder.encoder.Private.ThreadTask[0].Partitioned_Rice_Contents_Workspace_Ptr[i][1] = encoder.encoder.Private.ThreadTask[0].Partitioned_Rice_Contents_Workspace[i][1];
			}

			for (uint32_t i = 0; i < 2; i++)
			{
				encoder.encoder.Private.ThreadTask[0].Partitioned_Rice_Contents_Workspace_Ptr_Mid_Side[i][0] = encoder.encoder.Private.ThreadTask[0].Partitioned_Rice_Contents_Workspace_Mid_Side[i][0];
				encoder.encoder.Private.ThreadTask[0].Partitioned_Rice_Contents_Workspace_Ptr_Mid_Side[i][1] = encoder.encoder.Private.ThreadTask[0].Partitioned_Rice_Contents_Workspace_Mid_Side[i][1];
			}

			for (uint32_t i = 0; i < Constants.Flac__Max_Channels; i++)
			{
				Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Init(encoder.encoder.Private.ThreadTask[0].Partitioned_Rice_Contents_Workspace[i][0]);
				Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Init(encoder.encoder.Private.ThreadTask[0].Partitioned_Rice_Contents_Workspace[i][1]);
			}

			for (uint32_t i = 0; i < 2; i++)
			{
				Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Init(encoder.encoder.Private.ThreadTask[0].Partitioned_Rice_Contents_Workspace_Mid_Side[i][0]);
				Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Init(encoder.encoder.Private.ThreadTask[0].Partitioned_Rice_Contents_Workspace_Mid_Side[i][1]);
			}

			for (uint32_t i = 0; i < 2; i++)
				Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Init(encoder.encoder.Private.ThreadTask[0].Partitioned_Rice_Contents_Extra[i]);

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
			Debug.Assert(encoder.Private.ThreadTask[0] != null);
			Debug.Assert(encoder.Private.ThreadTask[0].Frame != null);

			encoder.Private.Is_Being_Deleted = true;

			Flac__Stream_Encoder_Finish();

			for (uint32_t i = 0; i < Constants.Flac__Max_Channels; i++)
			{
				Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Clear(encoder.Private.ThreadTask[0].Partitioned_Rice_Contents_Workspace[i][0]);
				Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Clear(encoder.Private.ThreadTask[0].Partitioned_Rice_Contents_Workspace[i][1]);
			}

			for (uint32_t i = 0; i < 2; i++)
			{
				Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Clear(encoder.Private.ThreadTask[0].Partitioned_Rice_Contents_Workspace_Mid_Side[i][0]);
				Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Clear(encoder.Private.ThreadTask[0].Partitioned_Rice_Contents_Workspace_Mid_Side[i][1]);
			}

			for (uint32_t i = 0; i < 2; i++)
				Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Clear(encoder.Private.ThreadTask[0].Partitioned_Rice_Contents_Extra[i]);

			encoder.Private.ThreadTask[0].Frame.Flac__BitWriter_Delete();
			encoder.Private.ThreadTask[0] = null;
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
			{
				if (encoder.Protected.Metadata != null)	// True in case Flac__Stream_Encoder_Set_Metadata was used but init failed
				{
					encoder.Protected.Metadata = null;
					encoder.Protected.Num_Metadata_Blocks = 0;
				}

				if (encoder.Private.File != null)
				{
					if (!encoder.Private.Leave_Stream_Open)
						encoder.Private.File.Dispose();

					encoder.Private.File = null;
					encoder.Private.Leave_Stream_Open = false;
				}

				return true;
			}

			if ((encoder.Protected.State == Flac__StreamEncoderState.Ok) && !encoder.Private.Is_Being_Deleted)
			{
				Flac__bool ok = true;

				// First finish threads
				if (encoder.Protected.Num_Threads > 1)
				{
					// This is quite complicated, so here is an explanation on what is supposed to happen
					// 
					// Thread no.0 and threadtask no.0 are reserved for non-threaded operation, so counting
					// here starts at 1, which makes things slightly more complicated.
					// 
					// If the file processed was very short compared to the requested number of threadtasks,
					// not all threadtasks have been populated yet. Handling that is easy: threadtask no.1 needs
					// to be processed first, monotonically increasing until the last populated threadtask is
					// processed. This number is stored in encoder->private_->num_started_threadtasks
					// 
					// If the file is longer, the next due frame chronologically might not be in threadtasks
					// number 1, because the threadtasks work like a ringbuffer. To access this, the variable
					// twrap starts counting at the next due frame, and the modulo operator (%) is used to
					// "wrap" the number with the number of threadtasks. So, if the next due task is 3
					// and 4 tasks are started, twrap increases 3, 4, 5, 6, and t follows with values 3, 4, 1, 2
					uint32_t start, end;

					if (encoder.Private.Num_Started_ThreadTasks < encoder.Private.Num_ThreadTasks)
					{
						start = 1;
						end = encoder.Private.Num_Started_ThreadTasks;
					}
					else
					{
						start = encoder.Private.Next_Thread;
						end = encoder.Private.Next_Thread + encoder.Private.Num_ThreadTasks - 1;
					}

					for (uint32_t twrap = start; twrap < end; twrap++)
					{
						Debug.Assert(twrap > 0);

						uint32_t t = (twrap - 1) % (encoder.Private.Num_ThreadTasks - 1) + 1;

						// Lock mutex, if task isn't done yet, wait for condition
						encoder.Private.ThreadTask[t].Mutex_This_Task.WaitOne();

						while (!encoder.Private.ThreadTask[t].Task_Done)
							encoder.Private.ThreadTask[t].Cond_Task_Done.Wait(encoder.Private.ThreadTask[t].Mutex_This_Task);

						if (!encoder.Private.ThreadTask[t].ReturnValue)
							ok = false;

						if (ok && !Write_BitBuffer(encoder.Private.ThreadTask[t], encoder.Protected.BlockSize, false))
							ok = false;

						encoder.Private.ThreadTask[t].Mutex_This_Task.ReleaseMutex();
					}

					// Wait for MD5 calculation to finish
					encoder.Private.Mutex_Work_Queue.WaitOne();

					while (encoder.Private.Md5_Active || encoder.Private.Md5_Fifo.Tail > 0)
						encoder.Private.Cond_Md5_Emptied.Wait(encoder.Private.Mutex_Work_Queue);

					encoder.Private.Mutex_Work_Queue.ReleaseMutex();
				}

				if (ok && encoder.Private.Current_Sample_Number != 0)
				{
					encoder.Protected.BlockSize = encoder.Private.Current_Sample_Number;

					if (!Resize_Buffers(encoder.Protected.BlockSize))
					{
						// The above function sets the state for us in case of an error
						return true;
					}

					if (!Process_Frame(true))
						error = true;
				}
			}

			if (encoder.Protected.Num_Threads > 1)
			{
				// Properly finish all threads
				encoder.Private.Mutex_Work_Queue.WaitOne();

				for (uint32_t t = 1; t < encoder.Private.Num_Created_Threads; t++)
					encoder.Private.Finish_Work_Threads = true;

				encoder.Private.Cond_Wake_Up_Thread.Broadcast();
				encoder.Private.Cond_Work_Available.Broadcast();
				encoder.Private.Mutex_Work_Queue.ReleaseMutex();

				for (uint32_t t = 1; t < encoder.Private.Num_Created_Threads; t++)
					encoder.Private.Thread[t].Join();
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
				else if ((n > 17) && specification.StartsWith("subdivide_tukey("))
				{
					Flac__int32 parts = Helpers.ParseInt(specification.Substring(16));

					if (parts > 1)
					{
						int si_1 = specification.IndexOf('/');
						Flac__real p = si_1 != -1 ? Helpers.ParseFloat(specification.Substring(si_1 + 1)) : 5e-1f;

						if (p > 1)
							p = 1;
						else if (p < 0)
							p = 0;

						encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations].Parameters = new Flac__ApodizationParameter_Subdivide_Tukey { Parts = parts, P = p / parts };
						encoder.Protected.Apodizations[encoder.Protected.Num_Apodizations++].Type = Flac__ApodizationFunction.Subdivide_Tukey;
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
		/// Set the maximum number of threads to use during encoding.
		/// Set to a value different than 1 to enable multithreaded encoding.
		///
		/// Note that enabling multithreading encoding (i.e., setting a value
		/// different than 1) results in the behaviour of
		/// FLAC__stream_encoder_finish(), FLAC__stream_encoder_process(),
		/// FLAC__stream_encoder_process_interleaved() subtly changing.
		/// For example, calling one of the process functions with enough
		/// samples to fill a block might not always result in a call to
		/// the write callback with a frame coding these samples. Instead,
		/// blocks with samples are distributed among worker threads,
		/// meaning that the first few calls to those functions will return
		/// very quickly, while later calls might block if all threads are
		/// occupied. Also, certain calls to the process functions will
		/// results in several calls to the write callback, while subsequent
		/// calls might again return very quickly with no calls to the write
		/// callback.
		///
		/// Also, a call to FLAC__stream_encoder_finish() blocks while
		/// waiting for all threads to finish, and therefore might take much
		/// longer than when not multithreading and result in multiple calls
		/// to the write callback.
		///
		/// Calls to the write callback are guaranteed to be in the correct
		/// order.
		///
		/// Currently, passing a value of 0 is synonymous with a value of 1,
		/// but this might change in the future.
		///
		/// Default 1
		/// <param name="value">See above</param>
		/// <returns>FLAC__STREAM_ENCODER_SET_NUM_THREADS_OK if the number of threads was set correctly, FLAC__STREAM_ENCODER_SET_NUM_THREADS_NOT_COMPILED_WITH_MULTITHREADING_ENABLED when multithreading was not enabled at compilation, FLAC__STREAM_ENCODER_SET_NUM_THREADS_ALREADY_INITIALIZED when the encoder was already initialized, FLAC__STREAM_ENCODER_SET_NUM_THREADS_TOO_MANY_THREADS when the number of threads was larger than the maximum allowed number of threads (currently 128)</returns>
		/// </summary>
		/********************************************************************/
		public Flac__StreamEncoderSetNumThreads Flac__Stream_Encoder_Set_Num_Threads(uint32_t value)
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			if (encoder.Protected.State != Flac__StreamEncoderState.Uninitialized)
				return Flac__StreamEncoderSetNumThreads.Already_Initialized;

			if (value > Constants.Flac__Stream_Encoder_Max_Threads)
				return Flac__StreamEncoderSetNumThreads.Too_Many_Threads;

			if (value == 0)
				encoder.Protected.Num_Threads = 1;
			else
				encoder.Protected.Num_Threads = value;

			return Flac__StreamEncoderSetNumThreads.Ok;
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
		/// Set to true to make the encoder not output frames which contain
		/// only constant subframes. This is beneficial for streaming
		/// applications: very small frames can cause problems with buffering
		/// as bitrates can drop as low 1kbit/s for CDDA audio encoded within
		/// subset. The minimum bitrate for a FLAC file encoded with this
		/// function used is raised to 1bit/sample (i.e. 48kbit/s for 48kHz
		/// material).
		/// <param name="value">Flag value</param>
		/// <returns>False if the encoder is already initialized, else true</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Set_Limit_Min_Bitrate(Flac__bool value)
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			if (encoder.Protected.State != Flac__StreamEncoderState.Uninitialized)
				return false;

			encoder.Protected.Limit_Min_Bitrate = value;

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
		/// Get maximum number of threads setting.
		/// <returns>See Flac__Stream_Encoder_Set_Num_Threads()</returns>
		/// </summary>
		/********************************************************************/
		public uint32_t Flac__Stream_Encoder_Get_Num_Threads()
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			return encoder.Protected.Num_Threads;
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
		/// Get the "limit_min_bitrate" flag
		/// <returns>See Flac__Stream_Encoder_Set_Limit_Min_Bitrate()</returns>
		/// </summary>
		/********************************************************************/
		public Flac__bool Flac__Stream_Encoder_Get_Limit_Min_Bitrate()
		{
			Debug.Assert(encoder != null);
			Debug.Assert(encoder.Private != null);
			Debug.Assert(encoder.Protected != null);

			return encoder.Protected.Limit_Min_Bitrate;
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

			if (encoder.Protected.State != Flac__StreamEncoderState.Ok)
				return false;

			uint32_t j = 0;
			uint32_t channels = encoder.Protected.Channels;
			uint32_t blockSize = encoder.Protected.BlockSize;
			Flac__int32 sample_Max = int32_t.MaxValue >> (int)(32 - encoder.Protected.Bits_Per_Sample);
			Flac__int32 sample_Min = int32_t.MinValue >> (int)(32 - encoder.Protected.Bits_Per_Sample);

			do
			{
				uint32_t n = Math.Min(blockSize + Overread - encoder.Private.Current_Sample_Number, samples - j);

				for (uint32_t channel = 0; channel < channels; channel++)
				{
					if (buffer[channel] == null)
						return false;

					for (uint32_t i = encoder.Private.Current_Sample_Number, k = j; (i <= blockSize) && (k < samples); i++, k++)
					{
						if ((buffer[channel][k] < sample_Min) || (buffer[channel][k] > sample_Max))
						{
							encoder.Protected.State = Flac__StreamEncoderState.Client_Error;
							return false;
						}
					}

					Array.Copy(buffer[channel], j, encoder.Private.ThreadTask[0].Integer_Signal[channel], encoder.Private.Current_Sample_Number, n);
				}

				j += n;
				encoder.Private.Current_Sample_Number += n;

				// We only process if we have a full block + 1 extra sample; final block is always handled by Flac__Stream_Encoder_Finish()
				if (encoder.Private.Current_Sample_Number > blockSize)
				{
					Debug.Assert(encoder.Private.Current_Sample_Number == blockSize + Overread);
					Debug.Assert(Overread == 1);	// Assert we only overread 1 sample which simplifies the rest of the code below

					if (!Process_Frame(false))
						return false;

					// Move unprocessed overread samples to beginnings of arrays
					for (uint32_t channel = 0; channel < channels; channel++)
						encoder.Private.ThreadTask[0].Integer_Signal[channel][0] = encoder.Private.ThreadTask[0].Integer_Signal[channel][blockSize];

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

			if (encoder.Protected.State != Flac__StreamEncoderState.Ok)
				return false;

			uint32_t channels = encoder.Protected.Channels;
			uint32_t blockSize = encoder.Protected.BlockSize;
			Flac__int32 sample_Max = int32_t.MaxValue >> (int)(32 - encoder.Protected.Bits_Per_Sample);
			Flac__int32 sample_Min = int32_t.MinValue >> (int)(32 - encoder.Protected.Bits_Per_Sample);

			uint32_t j = 0;
			uint32_t k = 0;
			uint32_t i;

			do
			{
				// "i <= blockSize" to overread 1 sample; see comment in Overread decl
				for (i = encoder.Private.Current_Sample_Number; (i <= blockSize) && (j < samples); i++, j++)
				{
					for (uint32_t channel = 0; channel < channels; channel++)
					{
						if ((buffer[k] < sample_Min) || (buffer[k] > sample_Max))
						{
							encoder.Protected.State = Flac__StreamEncoderState.Client_Error;
							return false;
						}

						encoder.Private.ThreadTask[0].Integer_Signal[channel][i] = buffer[k++];
					}
				}

				encoder.Private.Current_Sample_Number = i;

				// We only process if we have a full block + 1 extra sample; final block is always handled by Flac__Stream_Encoder_Finish()
				if (i > blockSize)
				{
					if (!Process_Frame(false))
						return false;

					// Move unprocessed overread samples to beginnings of arrays
					Debug.Assert(i == blockSize + Overread);
					Debug.Assert(Overread == 1);	// Assert we only overread 1 sample which simplifies the rest of the code below

					for (uint32_t channel = 0; channel < channels; channel++)
						encoder.Private.ThreadTask[0].Integer_Signal[channel][0] = encoder.Private.ThreadTask[0].Integer_Signal[channel][blockSize];

					encoder.Private.Current_Sample_Number = 1;
				}
			}
			while (j < samples);

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

				if ((encoder.Protected.Bits_Per_Sample != 8) && (encoder.Protected.Bits_Per_Sample != 12) && (encoder.Protected.Bits_Per_Sample != 16) &&
				    (encoder.Protected.Bits_Per_Sample != 20) && (encoder.Protected.Bits_Per_Sample != 24) && (encoder.Protected.Bits_Per_Sample != 32))
				{
					return Flac__StreamEncoderInitStatus.Not_Streamable;
				}

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

			if (encoder.Protected.Num_Threads > 1)
			{
				encoder.Private.Num_ThreadTasks = encoder.Protected.Num_Threads * 2 + 2;	// First threadtask is reserved for main thread

				encoder.Private.Mutex_Md5_Fifo = new Mutex();
				encoder.Private.Mutex_Work_Queue = new Mutex();
				encoder.Private.Cond_Md5_Emptied = new PThread_Cond_T();
				encoder.Private.Cond_Work_Available = new PThread_Cond_T();
				encoder.Private.Cond_Wake_Up_Thread = new PThread_Cond_T();

				if (encoder.Protected.Do_Md5)
				{
					encoder.Private.Md5_Fifo.Size = (encoder.Protected.BlockSize + Overread) * (encoder.Private.Num_ThreadTasks + 2);

					for (uint32_t i = 0; i < encoder.Protected.Channels; i++)
					{
						encoder.Private.Md5_Fifo.Data[i] = Alloc.Safe_MAlloc_Mul_2Op_P<Flac__int32>(1, encoder.Private.Md5_Fifo.Size);
						if (encoder.Private.Md5_Fifo.Data[i] == null)
						{
							encoder.Private.Mutex_Md5_Fifo.Dispose();
							encoder.Private.Mutex_Work_Queue.Dispose();
							encoder.Private.Cond_Md5_Emptied.Dispose();
							encoder.Private.Cond_Work_Available.Dispose();
							encoder.Private.Cond_Wake_Up_Thread.Dispose();

							encoder.Protected.State = Flac__StreamEncoderState.Memory_Allocation_Error;
							return Flac__StreamEncoderInitStatus.Encoder_Error;
						}
					}
				}

				encoder.Private.Md5_Fifo.Tail = 0;

				for (uint32_t t = 1; t < encoder.Private.Num_ThreadTasks; t++)
				{
					encoder.Private.ThreadTask[t] = new Flac__StreamEncoderThreadTask();

					encoder.Private.ThreadTask[t].Frame = BitWriter.Flac__BitWriter_New();
					if (encoder.Private.ThreadTask[t].Frame == null)
					{
						encoder.Private.ThreadTask[t] = null;
						encoder.Protected.State = Flac__StreamEncoderState.Memory_Allocation_Error;

						return Flac__StreamEncoderInitStatus.Encoder_Error;
					}

					encoder.Private.ThreadTask[t].Mutex_This_Task = new Mutex();
					encoder.Private.ThreadTask[t].Cond_Task_Done = new PThread_Cond_T();

					for (uint32_t i = 0; i < Constants.Flac__Max_Channels; i++)
					{
						encoder.Private.ThreadTask[t].SubFrame_Workspace_Ptr[i][0] = encoder.Private.ThreadTask[t].SubFrame_Workspace[i][0];
						encoder.Private.ThreadTask[t].SubFrame_Workspace_Ptr[i][1] = encoder.Private.ThreadTask[t].SubFrame_Workspace[i][1];
					}

					for (uint32_t i = 0; i < 2; i++)
					{
						encoder.Private.ThreadTask[t].SubFrame_Workspace_Ptr_Mid_Side[i][0] = encoder.Private.ThreadTask[t].SubFrame_Workspace_Mid_Side[i][0];
						encoder.Private.ThreadTask[t].SubFrame_Workspace_Ptr_Mid_Side[i][1] = encoder.Private.ThreadTask[t].SubFrame_Workspace_Mid_Side[i][1];
					}

					for (uint32_t i = 0; i < Constants.Flac__Max_Channels; i++)
					{
						encoder.Private.ThreadTask[t].Partitioned_Rice_Contents_Workspace_Ptr[i][0] = encoder.Private.ThreadTask[t].Partitioned_Rice_Contents_Workspace[i][0];
						encoder.Private.ThreadTask[t].Partitioned_Rice_Contents_Workspace_Ptr[i][1] = encoder.Private.ThreadTask[t].Partitioned_Rice_Contents_Workspace[i][1];
					}

					for (uint32_t i = 0; i < 2; i++)
					{
						encoder.Private.ThreadTask[t].Partitioned_Rice_Contents_Workspace_Ptr_Mid_Side[i][0] = encoder.Private.ThreadTask[t].Partitioned_Rice_Contents_Workspace_Mid_Side[i][0];
						encoder.Private.ThreadTask[t].Partitioned_Rice_Contents_Workspace_Ptr_Mid_Side[i][1] = encoder.Private.ThreadTask[t].Partitioned_Rice_Contents_Workspace_Mid_Side[i][1];
					}

					for (uint32_t i = 0; i < Constants.Flac__Max_Channels; i++)
					{
						Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Init(encoder.Private.ThreadTask[t].Partitioned_Rice_Contents_Workspace[i][0]);
						Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Init(encoder.Private.ThreadTask[t].Partitioned_Rice_Contents_Workspace[i][1]);
					}

					for (uint32_t i = 0; i < 2; i++)
					{
						Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Init(encoder.Private.ThreadTask[t].Partitioned_Rice_Contents_Workspace_Mid_Side[i][0]);
						Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Init(encoder.Private.ThreadTask[t].Partitioned_Rice_Contents_Workspace_Mid_Side[i][1]);
					}

					for (uint32_t i = 0; i < 2; i++)
						Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Init(encoder.Private.ThreadTask[t].Partitioned_Rice_Contents_Extra[i]);
				}
			}

			for (uint32_t i = 0; i < encoder.Protected.Num_Apodizations; i++)
				encoder.Private.Window_Unaligned[i] = encoder.Private.Window[i] = null;

			for (uint32_t t = 0; t < encoder.Private.Num_ThreadTasks; t++)
			{
				for (uint32_t i = 0; i < encoder.Protected.Channels; i++)
					encoder.Private.ThreadTask[t].Integer_Signal_Unaligned[i] = encoder.Private.ThreadTask[t].Integer_Signal[i] = null;

				for (uint32_t i = 0; i < 2; i++)
					encoder.Private.ThreadTask[t].Integer_Signal_Mid_Side_Unaligned[i] = encoder.Private.ThreadTask[t].Integer_Signal_Mid_Size[i] = null;

				encoder.Private.ThreadTask[t].Integer_Signal_33Bit_Side_Unaligned = encoder.Private.ThreadTask[t].Integer_Signal_33Bit_Side = null;
				encoder.Private.ThreadTask[t].Windowed_Signal_Unaligned = encoder.Private.ThreadTask[t].Windowed_Signal = null;

				for (uint32_t i = 0; i < encoder.Protected.Channels; i++)
				{
					encoder.Private.ThreadTask[t].Residual_Workspace_Unaligned[i][0] = encoder.Private.ThreadTask[t].Residual_Workspace[i][0] = null;
					encoder.Private.ThreadTask[t].Residual_Workspace_Unaligned[i][1] = encoder.Private.ThreadTask[t].Residual_Workspace[i][1] = null;
					encoder.Private.ThreadTask[t].Best_SubFrame[i] = 0;
				}

				for (uint32_t i = 0; i < 2; i++)
				{
					encoder.Private.ThreadTask[t].Residual_Workspace_Mid_Side_Unaligned[i][0] = encoder.Private.ThreadTask[t].Residual_Workspace_Mid_Side[i][0] = null;
					encoder.Private.ThreadTask[t].Residual_Workspace_Mid_Side_Unaligned[i][1] = encoder.Private.ThreadTask[t].Residual_Workspace_Mid_Side[i][1] = null;
					encoder.Private.ThreadTask[t].Best_SubFrame_Bits_Mid_Side[i] = 0;
				}

				encoder.Private.ThreadTask[t].Abs_Residual_Partition_Sums_Unaligned = encoder.Private.ThreadTask[t].Abs_Residual_Partition_Sums = null;
				encoder.Private.ThreadTask[t].Raw_Bits_Per_Partition_Unaligned = null;
				encoder.Private.ThreadTask[t].Raw_Bits_Per_Partition = null;
			}

			if (!Resize_Buffers(encoder.Protected.BlockSize))
			{
				// The above function sets the state for us in case of an error
				return Flac__StreamEncoderInitStatus.Encoder_Error;
			}

			for (uint32_t t = 0; t < encoder.Private.Num_ThreadTasks; t++)
			{
				if (!encoder.Private.ThreadTask[t].Frame.Flac__BitWriter_Init())
				{
					encoder.Protected.State = Flac__StreamEncoderState.Memory_Allocation_Error;
					return Flac__StreamEncoderInitStatus.Encoder_Error;
				}
			}

			// These must be done before we write any metadata, because that
			// calls the write_Callback, which uses these values
			encoder.Private.First_SeekPoint_To_Check = 0;
			encoder.Private.Samples_Written = 0;
			encoder.Protected.StreamInfo_Offset = 0;
			encoder.Protected.Seekable_Offset = 0;
			encoder.Protected.Audio_Offset = 0;

			// Write the stream header
			if (!encoder.Private.ThreadTask[0].Frame.Flac__BitWriter_Write_Raw_UInt32(Constants.Flac__Stream_Sync, Constants.Flac__Stream_Sync_Len))
			{
				encoder.Protected.State = Flac__StreamEncoderState.Framing_Error;
				return Flac__StreamEncoderInitStatus.Encoder_Error;
			}

			if (!Write_BitBuffer(encoder.Private.ThreadTask[0], 0, false))
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

			if (!Stream_Encoder_Framing.Flac__Add_Metadata_Block(encoder.Private.StreamInfo, encoder.Private.ThreadTask[0].Frame, true))
			{
				encoder.Protected.State = Flac__StreamEncoderState.Framing_Error;
				return Flac__StreamEncoderInitStatus.Encoder_Error;
			}

			if (!Write_BitBuffer(encoder.Private.ThreadTask[0], 0, false))
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

				if (!Stream_Encoder_Framing.Flac__Add_Metadata_Block(vorbis_Comment, encoder.Private.ThreadTask[0].Frame, true))
				{
					encoder.Protected.State = Flac__StreamEncoderState.Framing_Error;
					return Flac__StreamEncoderInitStatus.Encoder_Error;
				}

				if (!Write_BitBuffer(encoder.Private.ThreadTask[0], 0, false))
				{
					// The above function sets the state for us in case of an error
					return Flac__StreamEncoderInitStatus.Encoder_Error;
				}
			}

			// Write the user's metadata blocks
			for (uint32_t i = 0; i < encoder.Protected.Num_Metadata_Blocks; i++)
			{
				encoder.Protected.Metadata[i].Is_Last = i == encoder.Protected.Num_Metadata_Blocks - 1;

				if (!Stream_Encoder_Framing.Flac__Add_Metadata_Block(encoder.Protected.Metadata[i], encoder.Private.ThreadTask[0].Frame, true))
				{
					encoder.Protected.State = Flac__StreamEncoderState.Framing_Error;
					return Flac__StreamEncoderInitStatus.Encoder_Error;
				}

				if (!Write_BitBuffer(encoder.Private.ThreadTask[0], 0, false))
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
			encoder.encoder.Protected.Limit_Min_Bitrate = false;
			encoder.encoder.Protected.Metadata = null;
			encoder.encoder.Protected.Num_Metadata_Blocks = 0;
			encoder.encoder.Protected.Num_Threads = 1;

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
			encoder.encoder.Private.Num_ThreadTasks = 1;
			encoder.encoder.Private.Num_Created_Threads = 1;
			encoder.encoder.Private.Next_Thread = 1;
			encoder.encoder.Private.Num_Running_Threads = 1;
			encoder.encoder.Private.Num_Started_ThreadTasks = 1;
			encoder.encoder.Private.Num_Available_ThreadTasks = 0;
			encoder.encoder.Private.Overcomitted_Indicator = 0;
			encoder.encoder.Private.Next_ThreadTask = 1;
			encoder.encoder.Private.Md5_Active = false;
			encoder.encoder.Private.Finish_Work_Threads = false;

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

			for (uint32_t i = 0; i < encoder.Protected.Num_Apodizations; i++)
			{
				if (encoder.Private.Window_Unaligned[i] != null)
					encoder.Private.Window_Unaligned[i] = null;
			}

			for (uint32_t t = 0; t < encoder.Private.Num_ThreadTasks; t++)
			{
				if (encoder.Private.ThreadTask[t] == null)
					continue;

				for (uint32_t i = 0; i < encoder.Protected.Channels; i++)
				{
					if (encoder.Private.ThreadTask[t].Integer_Signal_Unaligned[i] != null)
						encoder.Private.ThreadTask[t].Integer_Signal_Unaligned[i] = null;
				}

				for (uint32_t i = 0; i < 2; i++)
				{
					if (encoder.Private.ThreadTask[t].Integer_Signal_Mid_Side_Unaligned[i] != null)
						encoder.Private.ThreadTask[t].Integer_Signal_Mid_Side_Unaligned[i] = null;
				}

				if (encoder.Private.ThreadTask[t].Integer_Signal_33Bit_Side_Unaligned != null)
					encoder.Private.ThreadTask[t].Integer_Signal_33Bit_Side_Unaligned = null;

				if (encoder.Private.ThreadTask[t].Windowed_Signal_Unaligned != null)
					encoder.Private.ThreadTask[t].Windowed_Signal_Unaligned = null;

				for (uint32_t channel = 0; channel < encoder.Protected.Channels; channel++)
				{
					for (uint32_t i = 0; i < 2; i++)
					{
						if (encoder.Private.ThreadTask[t].Residual_Workspace_Unaligned[channel][i] != null)
							encoder.Private.ThreadTask[t].Residual_Workspace_Unaligned[channel][i] = null;
					}
				}

				for (uint32_t channel = 0; channel < encoder.Protected.Channels; channel++)
				{
					for (uint32_t i = 0; i < 2; i++)
					{
						if (encoder.Private.ThreadTask[t].Residual_Workspace_Mid_Side_Unaligned[channel][i] != null)
							encoder.Private.ThreadTask[t].Residual_Workspace_Mid_Side_Unaligned[channel][i] = null;
					}
				}

				if (encoder.Private.ThreadTask[t].Abs_Residual_Partition_Sums_Unaligned != null)
					encoder.Private.ThreadTask[t].Abs_Residual_Partition_Sums_Unaligned = null;

				if (encoder.Private.ThreadTask[t].Raw_Bits_Per_Partition_Unaligned != null)
					encoder.Private.ThreadTask[t].Raw_Bits_Per_Partition_Unaligned = null;

				for (uint32_t i = 0; i < Constants.Flac__Max_Channels; i++)
				{
					Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Clear(encoder.Private.ThreadTask[t].Partitioned_Rice_Contents_Workspace[i][0]);
					Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Clear(encoder.Private.ThreadTask[t].Partitioned_Rice_Contents_Workspace[i][1]);
				}

				for (uint32_t i = 0; i < 2; i++)
				{
					Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Clear(encoder.Private.ThreadTask[t].Partitioned_Rice_Contents_Workspace_Mid_Side[i][0]);
					Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Clear(encoder.Private.ThreadTask[t].Partitioned_Rice_Contents_Workspace_Mid_Side[i][1]);
				}

				for (uint32_t i = 0; i < 2; i++)
					Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Clear(encoder.Private.ThreadTask[t].Partitioned_Rice_Contents_Extra[i]);

				if (t > 0)
				{
					encoder.Private.ThreadTask[t].Frame.Flac__BitWriter_Delete();
					encoder.Private.ThreadTask[t].Mutex_This_Task.Dispose();
					encoder.Private.ThreadTask[t].Cond_Task_Done.Dispose();

					encoder.Private.ThreadTask[t] = null;
				}
			}

			if (encoder.Protected.Num_Threads > 1)
			{
				encoder.Private.Mutex_Md5_Fifo.Dispose();
				encoder.Private.Mutex_Md5_Fifo = null;

				encoder.Private.Mutex_Work_Queue.Dispose();
				encoder.Private.Mutex_Work_Queue = null;

				encoder.Private.Cond_Md5_Emptied.Dispose();
				encoder.Private.Cond_Md5_Emptied = null;

				encoder.Private.Cond_Work_Available.Dispose();
				encoder.Private.Cond_Work_Available = null;

				encoder.Private.Cond_Wake_Up_Thread.Dispose();
				encoder.Private.Cond_Wake_Up_Thread = null;

				if (encoder.Protected.Do_Md5)
				{
					for (uint32_t i = 0; i < encoder.Protected.Channels; i++)
					{
						if (encoder.Private.Md5_Fifo.Data[i] != null)
							encoder.Private.Md5_Fifo.Data[i] = null;
					}
				}
			}
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

			Flac__bool ok = true;

			// To avoid excessive malloc'ing, we only grow the buffer; no shrinking
			if (new_BlockSize > encoder.Private.Input_Capacity)
			{
				if (ok && (encoder.Protected.Max_Lpc_Order > 0))
				{
					for (uint32_t i = 0; ok && i < encoder.Protected.Num_Apodizations; i++)
						ok = ok && Memory.Flac__Memory_Alloc_Aligned_Real_Array(new_BlockSize, ref encoder.Private.Window_Unaligned[i], ref encoder.Private.Window[i]);
				}

				for (uint32_t t = 0; t < encoder.Private.Num_ThreadTasks; t++)
				{
					for (uint32_t i = 0; ok && i < encoder.Protected.Channels; i++)
						ok = ok && Memory.Flac__Memory_Alloc_Aligned_Int32_Array(new_BlockSize + 4 + Overread, ref encoder.Private.ThreadTask[t].Integer_Signal_Unaligned[i], ref encoder.Private.ThreadTask[t].Integer_Signal[i]);

					for (uint32_t i = 0; ok && i < 2; i++)
						ok = ok && Memory.Flac__Memory_Alloc_Aligned_Int32_Array(new_BlockSize + 4 + Overread, ref encoder.Private.ThreadTask[t].Integer_Signal_Mid_Side_Unaligned[i], ref encoder.Private.ThreadTask[t].Integer_Signal_Mid_Size[i]);

					ok = ok && Memory.Flac__Memory_Alloc_Aligned_Int64_Array(new_BlockSize + 4 + Overread, ref encoder.Private.ThreadTask[t].Integer_Signal_33Bit_Side_Unaligned, ref encoder.Private.ThreadTask[t].Integer_Signal_33Bit_Side);

					if (ok && (encoder.Protected.Max_Lpc_Order > 0))
						ok = ok && Memory.Flac__Memory_Alloc_Aligned_Real_Array(new_BlockSize, ref encoder.Private.ThreadTask[t].Windowed_Signal_Unaligned, ref encoder.Private.ThreadTask[t].Windowed_Signal);

					for (uint32_t channel = 0; ok && channel < encoder.Protected.Channels; channel++)
					{
						for (uint32_t i = 0; i < 2; i++)
							ok = ok && Memory.Flac__Memory_Alloc_Aligned_Int32_Array(new_BlockSize, ref encoder.Private.ThreadTask[t].Residual_Workspace_Unaligned[channel][i], ref encoder.Private.ThreadTask[t].Residual_Workspace[channel][i]);
					}

					for (uint32_t channel = 0; ok && channel < encoder.Protected.Channels; channel++)
					{
						for (uint32_t i = 0; ok && i < 2; i++)
						{
							ok = ok && Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Ensure_Size(encoder.Private.ThreadTask[t].Partitioned_Rice_Contents_Workspace[channel][i], encoder.Protected.Max_Residual_Partition_Order);
							ok = ok && Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Ensure_Size(encoder.Private.ThreadTask[t].Partitioned_Rice_Contents_Workspace[channel][i], encoder.Protected.Max_Residual_Partition_Order);
						}
					}

					for (uint32_t channel = 0; ok && channel < 2; channel++)
					{
						for (uint32_t i = 0; ok && i < 2; i++)
							ok = ok && Memory.Flac__Memory_Alloc_Aligned_Int32_Array(new_BlockSize, ref encoder.Private.ThreadTask[t].Residual_Workspace_Mid_Side_Unaligned[channel][i], ref encoder.Private.ThreadTask[t].Residual_Workspace_Mid_Side[channel][i]);
					}

					for (uint32_t channel = 0; ok && channel < encoder.Protected.Channels; channel++)
					{
						for (uint32_t i = 0; ok && i < 2; i++)
							ok = ok && Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Ensure_Size(encoder.Private.ThreadTask[t].Partitioned_Rice_Contents_Workspace_Mid_Side[channel][i], encoder.Protected.Max_Residual_Partition_Order);
					}

					for (uint32_t i = 0; ok && i < 2; i++)
						ok = ok && Format.Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Ensure_Size(encoder.Private.ThreadTask[t].Partitioned_Rice_Contents_Extra[i], encoder.Protected.Max_Residual_Partition_Order);

					// The *2 is an approximation to the series 1 + 1/2 + 1/4 + ... that sums tree occupies in a flat array
					// @@@ new_BlockSize*2 is too pessimistic, but to fix, we need smarter logic
					// because a smaller new_BlockSize can actually increase the # of partitions;
					// would require moving this out into a separate function, then checking its
					// capacity against the need of the current blockSize&min/max_Partition_Order
					// (and maybe predictor order)
					ok = ok && Memory.Flac__Memory_Alloc_Aligned_UInt64_Array(new_BlockSize * 2, ref encoder.Private.ThreadTask[t].Abs_Residual_Partition_Sums_Unaligned, ref encoder.Private.ThreadTask[t].Abs_Residual_Partition_Sums);
				}
			}

			if (ok)
				encoder.Private.Input_Capacity = new_BlockSize;
			else
			{
				encoder.Protected.State = Flac__StreamEncoderState.Memory_Allocation_Error;
				return ok;
			}

			// Now adjust the windows if the blockSize has changed
			if ((encoder.Protected.Max_Lpc_Order > 0) && (new_BlockSize > 1))
			{
				for (uint32_t i = 0; i < encoder.Protected.Num_Apodizations; i++)
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

						case Flac__ApodizationFunction.Subdivide_Tukey:
						{
							Window.Flac__Window_Tukey(encoder.Private.Window[i], (Flac__int32)new_BlockSize, ((Flac__ApodizationParameter_Subdivide_Tukey)encoder.Protected.Apodizations[i].Parameters).P);
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

			if (new_BlockSize <= Constants.Flac__Max_Lpc_Order)
			{
				// Intrinsics autocorrelation routines do not all handle cases in which lag might be
				// larger than data_len. Lag is one larger than the LPC order
				encoder.Private.Lpc = new Lpc();
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Write_BitBuffer(Flac__StreamEncoderThreadTask threadTask, uint32_t samples, Flac__bool is_Last_Block)
		{
			Debug.Assert(threadTask.Frame.Flac__BitWriter_Is_Byte_Aligned());

			if (!threadTask.Frame.Flac__BitWriter_Get_Buffer(out Span<Flac__byte> buffer, out size_t bytes))
			{
				encoder.Protected.State = Flac__StreamEncoderState.Memory_Allocation_Error;
				return false;
			}

			if (Write_Frame(buffer, bytes, samples, is_Last_Block) != Flac__StreamEncoderWriteStatus.Ok)
			{
				threadTask.Frame.Flac__BitWriter_Release_Buffer();
				threadTask.Frame.Flac__BitWriter_Clear();

				encoder.Protected.State = Flac__StreamEncoderState.Client_Error;
				return false;
			}

			threadTask.Frame.Flac__BitWriter_Release_Buffer();
			threadTask.Frame.Flac__BitWriter_Clear();

			if (samples > 0)
			{
				Flac__StreamMetadata_StreamInfo metaStreamInfo = (Flac__StreamMetadata_StreamInfo)encoder.Private.StreamInfo.Data;

				metaStreamInfo.Min_FrameSize = (uint32_t)Math.Min(bytes, metaStreamInfo.Min_FrameSize);
				metaStreamInfo.Max_FrameSize = (uint32_t)Math.Max(bytes, metaStreamInfo.Max_FrameSize);
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

				// FLAC__STREAM_ENCODER_TELL_STATUS_UNSUPPORTED just means we didn't get the offset; no error
				if ((encoder.Private.Tell_Callback != null) && (encoder.Private.Tell_Callback(this, out output_Position, encoder.Private.Client_Data) == Flac__StreamEncoderTellStatus.Error))
				{
					encoder.Protected.State = Flac__StreamEncoderState.Client_Error;
					return Flac__StreamEncoderWriteStatus.Fatal_Error;
				}

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
						// FLAC__STREAM_ENCODER_TELL_STATUS_UNSUPPORTED just means we didn't get the offset; no error
						if ((output_Position == 0) && (encoder.Private.Tell_Callback != null) && (encoder.Private.Tell_Callback(this, out output_Position, encoder.Private.Client_Data) == Flac__StreamEncoderTellStatus.Error))
						{
							encoder.Protected.State = Flac__StreamEncoderState.Client_Error;
							return Flac__StreamEncoderWriteStatus.Fatal_Error;
						}

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

				Flac__uint64 samples_uint36 = samples;

				if (samples > (1L << (int)Constants.Flac__Stream_Metadata_StreamInfo_Total_Samples_Len))
					samples_uint36 = 0;

				b[0] = (Flac__byte)(((bps - 1) << 4) | ((samples_uint36 >> 32) & 0x0f));
				b[1] = (Flac__byte)((samples_uint36 >> 24) & 0xff);
				b[2] = (Flac__byte)((samples_uint36 >> 16) & 0xff);
				b[3] = (Flac__byte)((samples_uint36 >> 8) & 0xff);
				b[4] = (Flac__byte)(samples_uint36 & 0xff);

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
				// Convert unused seekpoints to placeholders
				for (uint32_t i = 0; i < encoder.Private.Seek_Table.Num_Points; i++)
				{
					if (encoder.Private.Seek_Table.Points[i].Sample_Number > samples)
						encoder.Private.Seek_Table.Points[i].Sample_Number = Constants.Flac__Stream_Metadata_SeekPoint_Placeholder;
				}

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
		private Flac__bool Process_Frame(Flac__bool is_Last_Block)
		{
			if ((encoder.Protected.Num_Threads < 2) || is_Last_Block)
			{
				Debug.Assert(encoder.Protected.State == Flac__StreamEncoderState.Ok);

				// Accumulate raw signal to the MD5 signature
				if (encoder.Protected.Do_Md5 && !encoder.Private.Md5.Flac__Md5Accumulate(encoder.Private.ThreadTask[0].Integer_Signal, encoder.Protected.Channels, encoder.Protected.BlockSize, (encoder.Protected.Bits_Per_Sample + 7) / 8))
				{
					encoder.Protected.State = Flac__StreamEncoderState.Memory_Allocation_Error;
					return false;
				}

				// Process the frame header and subframes into the frame bitbuffer
				encoder.Private.ThreadTask[0].Current_Frame_Number = encoder.Private.Current_Frame_Number;

				if (!Process_SubFrames(encoder.Private.ThreadTask[0]))
				{
					// The above function sets the state for us in case of an error
					return false;
				}

				// Zero-pad the frame to a byte_boundary
				if (!encoder.Private.ThreadTask[0].Frame.Flac__BitWriter_Zero_Pad_To_Byte_Boundary())
				{
					encoder.Protected.State = Flac__StreamEncoderState.Memory_Allocation_Error;
					return false;
				}

				// CRC-16 the whole thing
				Debug.Assert(encoder.Private.ThreadTask[0].Frame.Flac__BitWriter_Is_Byte_Aligned());

				if (!encoder.Private.ThreadTask[0].Frame.Flac__BitWriter_Get_Write_Crc16(out Flac__uint16 crc) || !encoder.Private.ThreadTask[0].Frame.Flac__BitWriter_Write_Raw_UInt32(crc, Constants.Flac__Frame_Footer_Crc_Len))
				{
					encoder.Protected.State = Flac__StreamEncoderState.Memory_Allocation_Error;
					return false;
				}

				// Write it
				if (!Write_BitBuffer(encoder.Private.ThreadTask[0], encoder.Protected.BlockSize, is_Last_Block))
				{
					// The above function sets the state for us in case of an error
					return false;
				}
			}
			else
			{
				// This bit is quite complicated, so here are some pointers:
				// 
				// When this bit of code is reached for the first time, new threads are spawned and
				// threadtasks are populated until the total number of threads equals the requested number
				// of threads. Next, threadtasks are populated until they there are no more available.
				// Next, this main thread checks whether the threadtask that is due chronologically is
				// done. If it is, the bitbuffer is written and the threadtask memory reused for the next
				// frame. If it is not done, the main thread checks whether there is enough work left in the
				// queue. If there is a lot of work left, the main thread starts on some of it too.
				// If not a lot of work is left, the main thread goes to sleep until the frame due first is
				// finished.
				// 
				// - encoder->private_->next_thread is the number of the next thread to be created or, when
				//    the required number of threads is created, the next threadtask to be populated,
				//    or, when all threadtasks have been populated once, the next threadtask that needs
				//    to finish and thus reused.
				// - encoder->private_->next_threadtask is the number of the next threadtask that a thread
				//    can start work on.
				// 
				// So, in effect, next_thread is (after startup) a pointer considering the chronological
				// order, so input/output isn't shuffled. next_threadtask is a pointer to the next task that
				// hasn't been picked up by a thread yet. This distinction enables threads to work on frames
				// in a non-chronological order
				// 
				// encoder->protected_->num_threads is the max number of threads that can be spawned
				// encoder->private_->num_created_threads is the number of threads that has been spawned
				// encoder->private_->num_threadtasks keeps track of how many threadtasks are available
				// encoder->private_->num_started_threadtasks keeps track of how many threadtasks have been populated
				// 
				// NOTE: thread no. 0 and threadtask no. 0 are reserved for non-threaded operations, so next_thread
				// and next_threadtask start at 1
				if (encoder.Private.Num_Created_Threads < encoder.Protected.Num_Threads)
				{
					// Create a new thread
					encoder.Private.Thread[encoder.Private.Next_Thread] = new Thread(Process_Frame_Thread);
					encoder.Private.Thread[encoder.Private.Next_Thread].Start();

					encoder.Private.Num_Created_Threads++;
				}
				else if (encoder.Private.Num_Started_ThreadTasks == encoder.Private.Num_ThreadTasks)
				{
					// If the first task in the queue is still running, check whether there is enough work
					// left in the queue. If there is, start on some.
					//
					// First, check whether the mutex for the next due task is locked or free. If it is free (and thus acquired now) and
					// the task is done, proceed to the next bit (writing the bitbuffer). If it is either currently locked or not yet
					// processed, choose between starting on some work (if there is enough work in the queue) or waiting for the task
					// to finish. Either way, release the mutex first, so it doesn't get interlocked with the work queue mutex 
					bool mutex_result = !encoder.Private.ThreadTask[encoder.Private.Next_Thread].Mutex_This_Task.WaitOne(0);

					while (mutex_result || !encoder.Private.ThreadTask[encoder.Private.Next_Thread].Task_Done)
					{
						if (!mutex_result)
							encoder.Private.ThreadTask[encoder.Private.Next_Thread].Mutex_This_Task.ReleaseMutex();

						encoder.Private.Mutex_Work_Queue.WaitOne();

						if (encoder.Private.Num_Available_ThreadTasks > (encoder.Protected.Num_Threads - 1))
						{
							Flac__StreamEncoderThreadTask task = encoder.Private.ThreadTask[encoder.Private.Next_ThreadTask];
							encoder.Private.Num_Available_ThreadTasks--;
							encoder.Private.Next_ThreadTask++;

							if (encoder.Private.Next_ThreadTask == encoder.Private.Num_ThreadTasks)
								encoder.Private.Next_ThreadTask = 1;

							encoder.Private.Mutex_Work_Queue.ReleaseMutex();

							task.Mutex_This_Task.WaitOne();
							Process_Frame_Thread_Inner(task);
							mutex_result = !encoder.Private.ThreadTask[encoder.Private.Next_Thread].Mutex_This_Task.WaitOne(0);
						}
						else
						{
							encoder.Private.Mutex_Work_Queue.ReleaseMutex();
							encoder.Private.ThreadTask[encoder.Private.Next_Thread].Mutex_This_Task.WaitOne();

							while (!encoder.Private.ThreadTask[encoder.Private.Next_Thread].Task_Done)
								encoder.Private.ThreadTask[encoder.Private.Next_Thread].Cond_Task_Done.Wait(encoder.Private.ThreadTask[encoder.Private.Next_Thread].Mutex_This_Task);

							mutex_result = false;
						}
					}

					// Task is finished, write bitbuffer
					if (!encoder.Private.ThreadTask[encoder.Private.Next_Thread].ReturnValue)
					{
						encoder.Private.ThreadTask[encoder.Private.Next_Thread].Mutex_This_Task.ReleaseMutex();
						return false;
					}

					if (!Write_BitBuffer(encoder.Private.ThreadTask[encoder.Private.Next_Thread], encoder.Protected.BlockSize, is_Last_Block))
					{
						// The above function sets the state for us in case of an error
						encoder.Private.ThreadTask[encoder.Private.Next_Thread].Mutex_This_Task.ReleaseMutex();
						return false;
					}

					encoder.Private.ThreadTask[encoder.Private.Next_Thread].Mutex_This_Task.ReleaseMutex();
				}

				// Copy input data for MD5 calculation
				if (encoder.Protected.Do_Md5)
				{
					encoder.Private.Mutex_Work_Queue.WaitOne();

					while ((encoder.Private.Md5_Fifo.Tail + encoder.Protected.BlockSize) > encoder.Private.Md5_Fifo.Size)
					{
						encoder.Private.Cond_Md5_Emptied.Wait(encoder.Private.Mutex_Work_Queue);
					}

					encoder.Private.Mutex_Work_Queue.ReleaseMutex();
					encoder.Private.Mutex_Md5_Fifo.WaitOne();

					for (uint32_t i = 0; i < encoder.Protected.Channels; i++)
						Array.Copy(encoder.Private.ThreadTask[0].Integer_Signal[i], 0, encoder.Private.Md5_Fifo.Data[i], encoder.Private.Md5_Fifo.Tail, encoder.Protected.BlockSize);

					encoder.Private.Mutex_Work_Queue.WaitOne();
					encoder.Private.Md5_Fifo.Tail += encoder.Protected.BlockSize;

					encoder.Private.Cond_Work_Available.Signal();
					encoder.Private.Mutex_Work_Queue.ReleaseMutex();
					encoder.Private.Mutex_Md5_Fifo.ReleaseMutex();
				}

				// Copy input data for frame creation
				encoder.Private.ThreadTask[encoder.Private.Next_Thread].Mutex_This_Task.WaitOne();

				for (uint32_t i = 0; i < encoder.Protected.Channels; i++)
					Array.Copy(encoder.Private.ThreadTask[0].Integer_Signal[i], encoder.Private.ThreadTask[encoder.Private.Next_Thread].Integer_Signal[i], encoder.Protected.BlockSize);

				encoder.Private.ThreadTask[encoder.Private.Next_Thread].Current_Frame_Number = encoder.Private.Current_Frame_Number;
				encoder.Private.ThreadTask[encoder.Private.Next_Thread].Mutex_This_Task.ReleaseMutex();

				encoder.Private.Mutex_Work_Queue.WaitOne();

				if (encoder.Private.Num_Started_ThreadTasks < encoder.Private.Num_ThreadTasks)
					encoder.Private.Num_Started_ThreadTasks++;

				encoder.Private.Num_Available_ThreadTasks++;
				encoder.Private.ThreadTask[encoder.Private.Next_Thread].Task_Done = false;
				encoder.Private.Cond_Work_Available.Signal();
				encoder.Private.Mutex_Work_Queue.ReleaseMutex();

				encoder.Private.Next_Thread++;

				if (encoder.Private.Next_Thread == encoder.Private.Num_ThreadTasks)
					encoder.Private.Next_Thread = 1;
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
		private void Process_Frame_Thread()
		{
			encoder.Private.Mutex_Work_Queue.WaitOne();
			encoder.Private.Num_Running_Threads++;
			encoder.Private.Mutex_Work_Queue.ReleaseMutex();

			while (true)
			{
				encoder.Private.Mutex_Work_Queue.WaitOne();

				if (encoder.Private.Finish_Work_Threads)
				{
					encoder.Private.Mutex_Work_Queue.ReleaseMutex();
					return;
				}

				// The code below pauses and restarts threads if it is noticed threads are often put too sleep
				// because of a lack of work. This reduces overhead when too many threads are active. The
				// overcommited indicator is increased when no tasks are available, decreased when more tasks
				// are available then threads are running, and reset when a thread is woken up or put to sleep
				if (encoder.Private.Num_Available_ThreadTasks == 0)
					encoder.Private.Overcomitted_Indicator++;
				else if (encoder.Private.Num_Available_ThreadTasks > encoder.Private.Num_Running_Threads)
					encoder.Private.Overcomitted_Indicator--;

				if (encoder.Private.Overcomitted_Indicator < -20)
				{
					encoder.Private.Overcomitted_Indicator = 0;
					encoder.Private.Cond_Wake_Up_Thread.Signal();
				}
				else if ((encoder.Private.Overcomitted_Indicator > 20) && (encoder.Private.Num_Running_Threads > 2))
				{
					encoder.Private.Overcomitted_Indicator = 0;
					encoder.Private.Num_Running_Threads--;

					encoder.Private.Cond_Wake_Up_Thread.Wait(encoder.Private.Mutex_Work_Queue);
					encoder.Private.Num_Running_Threads++;
				}

				while ((encoder.Private.Num_Available_ThreadTasks == 0) && (encoder.Private.Md5_Active || (encoder.Private.Md5_Fifo.Tail == 0)))
				{
					if (encoder.Private.Finish_Work_Threads)
					{
						encoder.Private.Mutex_Work_Queue.ReleaseMutex();
						return;
					}

					encoder.Private.Cond_Work_Available.Wait(encoder.Private.Mutex_Work_Queue);
				}

				if (encoder.Protected.Do_Md5 && !encoder.Private.Md5_Active && (encoder.Private.Md5_Fifo.Tail > 0))
				{
					uint32_t length = 0;
					encoder.Private.Md5_Active = true;

					while (encoder.Private.Md5_Fifo.Tail > 0)
					{
						length = encoder.Private.Md5_Fifo.Tail;
						encoder.Private.Mutex_Work_Queue.ReleaseMutex();

						if (!encoder.Private.Md5.Flac__Md5Accumulate(encoder.Private.Md5_Fifo.Data, encoder.Protected.Channels, length, (encoder.Protected.Bits_Per_Sample + 7) / 8))
						{
							encoder.Protected.State = Flac__StreamEncoderState.Memory_Allocation_Error;
							return;
						}

						encoder.Private.Mutex_Md5_Fifo.WaitOne();

						for (uint32_t channel = 0; channel < encoder.Protected.Channels; channel++)
							Array.Copy(encoder.Private.Md5_Fifo.Data[channel], length, encoder.Private.Md5_Fifo.Data[channel], 0, encoder.Private.Md5_Fifo.Tail - length);

						encoder.Private.Mutex_Work_Queue.WaitOne();
						encoder.Private.Md5_Fifo.Tail -= length;
						encoder.Private.Cond_Md5_Emptied.Signal();
						encoder.Private.Mutex_Md5_Fifo.ReleaseMutex();
					}

					encoder.Private.Md5_Active = false;
					encoder.Private.Mutex_Work_Queue.ReleaseMutex();
				}
				else if (encoder.Private.Num_Available_ThreadTasks > 0)
				{
					Flac__StreamEncoderThreadTask task = encoder.Private.ThreadTask[encoder.Private.Next_ThreadTask];
					encoder.Private.Num_Available_ThreadTasks--;
					encoder.Private.Next_ThreadTask++;

					if (encoder.Private.Next_ThreadTask == encoder.Private.Num_ThreadTasks)
						encoder.Private.Next_ThreadTask = 1;

					encoder.Private.Mutex_Work_Queue.ReleaseMutex();
					task.Mutex_This_Task.WaitOne();

					if (!Process_Frame_Thread_Inner(task))
						return;
				}
				else
					encoder.Private.Mutex_Work_Queue.ReleaseMutex();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Process_Frame_Thread_Inner(Flac__StreamEncoderThreadTask task)
		{
			Flac__bool ok = true;

			// Process the frame header and subframes into the frame bitbuffer
			if (ok && !Process_SubFrames(task))
			{
				// The above function sets the state for us in case of an error
				ok = false;
			}

			// Zero-pad the frame to a byte_boundary
			if (ok && !task.Frame.Flac__BitWriter_Zero_Pad_To_Byte_Boundary())
			{
				encoder.Protected.State = Flac__StreamEncoderState.Memory_Allocation_Error;
				ok = false;
			}

			// CRC-16 the whole thing
			Debug.Assert(!ok || task.Frame.Flac__BitWriter_Is_Byte_Aligned());

			if (ok && (!task.Frame.Flac__BitWriter_Get_Write_Crc16(out Flac__uint16 crc) || !task.Frame.Flac__BitWriter_Write_Raw_UInt32(crc, Constants.Flac__Frame_Footer_Crc_Len)))
			{
				encoder.Protected.State = Flac__StreamEncoderState.Memory_Allocation_Error;
				ok = false;
			}

			task.ReturnValue = ok;
			task.Task_Done = true;

			task.Cond_Task_Done.Signal();
			task.Mutex_This_Task.ReleaseMutex();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Process_SubFrames(Flac__StreamEncoderThreadTask threadTask)
		{
			uint32_t min_Partition_Order = encoder.Protected.Min_Residual_Partition_Order;
			Flac__bool all_SubFrames_Constant = true;

			threadTask.Disable_Constant_SubFrames = encoder.Private.Disable_Constant_SubFrames;

			// Calculate the min,max Rice partition orders
			uint32_t max_Partition_Order = Format.Flac__Format_Get_Max_Rice_Partition_Order_From_BlockSize(encoder.Protected.BlockSize);
			max_Partition_Order = Math.Min(max_Partition_Order, encoder.Protected.Max_Residual_Partition_Order);

			min_Partition_Order = Math.Min(min_Partition_Order, max_Partition_Order);

			// Setup the frame
			Flac__FrameHeader frame_Header = new Flac__FrameHeader();
			frame_Header.BlockSize = encoder.Protected.BlockSize;
			frame_Header.Sample_Rate = encoder.Protected.Sample_Rate;
			frame_Header.Channels = encoder.Protected.Channels;
			frame_Header.Channel_Assignment = Flac__ChannelAssignment.Independent;	// The default unless the encoder determines otherwise
			frame_Header.Bits_Per_Sample = encoder.Protected.Bits_Per_Sample;
			frame_Header.Number_Type = Flac__FrameNumberType.Frame_Number;
			frame_Header.Frame_Number = threadTask.Current_Frame_Number;

			// Figure out what channel assignments to try
			Flac__bool do_Independent, do_Mid_Side;

			if (encoder.Protected.Do_Mid_Side_Stereo)
			{
				if (encoder.Protected.Loose_Mid_Side_Stereo)
				{
					uint64_t sumAbsLR = 0, sumAbsMS = 0;

					if (encoder.Protected.Bits_Per_Sample < 25)
					{
						for (uint32_t i = 1; i < encoder.Protected.BlockSize; i++)
						{
							int32_t predictionLeft = threadTask.Integer_Signal[0][i] - threadTask.Integer_Signal[0][i - 1];
							int32_t predictionRight = threadTask.Integer_Signal[1][i] - threadTask.Integer_Signal[1][i - 1];
							sumAbsLR += (uint64_t)(Math.Abs(predictionLeft) + Math.Abs(predictionRight));
							sumAbsMS += (uint64_t)(Math.Abs((predictionLeft + predictionRight) >> 1) + Math.Abs(predictionLeft - predictionRight));
						}
					}
					else	// bps 25 or higher
					{
						for (uint32_t i = 1; i < encoder.Protected.BlockSize; i++)
						{
							int64_t predictionLeft = (int64_t)threadTask.Integer_Signal[0][i] - threadTask.Integer_Signal[0][i - 1];
							int64_t predictionRight = (int64_t)threadTask.Integer_Signal[1][i] - threadTask.Integer_Signal[1][i - 1];
							sumAbsLR += (uint64_t)(Math.Abs(predictionLeft) + Math.Abs(predictionRight));
							sumAbsMS += (uint64_t)(Math.Abs((predictionLeft + predictionRight) >> 1) + Math.Abs(predictionLeft - predictionRight));
						}
					}

					if (sumAbsLR < sumAbsMS)
					{
						do_Independent = true;
						do_Mid_Side = false;
						frame_Header.Channel_Assignment = Flac__ChannelAssignment.Independent;
					}
					else
					{
						do_Independent = false;
						do_Mid_Side = true;
						frame_Header.Channel_Assignment = Flac__ChannelAssignment.Mid_Side;
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

			// Prepare mid side signals if applicable
			if (do_Mid_Side)
			{
				Debug.Assert(encoder.Protected.Channels == 2);

				if (encoder.Protected.Bits_Per_Sample < 32)
				{
					for (uint32_t i = 0; i < encoder.Protected.BlockSize; i++)
					{
						threadTask.Integer_Signal_Mid_Size[1][i] = threadTask.Integer_Signal[0][i] - threadTask.Integer_Signal[1][i];
						threadTask.Integer_Signal_Mid_Size[0][i] = (threadTask.Integer_Signal[0][i] + threadTask.Integer_Signal[1][i]) >> 1;	// NOTE: not the same as 'mid = (signal[0][j] + signal[1][j]) / 2' !
					}
				}
				else
				{
					for (uint32_t i = 0; i < encoder.Protected.BlockSize; i++)
					{
						threadTask.Integer_Signal_33Bit_Side[i] = (Flac__int64)threadTask.Integer_Signal[0][i] - threadTask.Integer_Signal[1][i];
						threadTask.Integer_Signal_Mid_Size[0][i] = (Flac__int32)(((Flac__int64)threadTask.Integer_Signal[0][i] + threadTask.Integer_Signal[1][i]) >> 1);	// NOTE: not the same as 'mid = (signal[0][j] + signal[1][j]) / 2' !
					}
				}
			}

			// Check for wasted bits; set effective bps for each subframe
			if (do_Independent)
			{
				for (uint32_t channel = 0; channel < encoder.Protected.Channels; channel++)
				{
					uint32_t w = Get_Wasted_Bits(threadTask.Integer_Signal[channel], encoder.Protected.BlockSize);

					if (w > encoder.Protected.Bits_Per_Sample)
						w = encoder.Protected.Bits_Per_Sample;

					threadTask.SubFrame_Workspace[channel][0].Wasted_Bits = threadTask.SubFrame_Workspace[channel][1].Wasted_Bits = w;
					threadTask.SubFrame_Bps[channel] = encoder.Protected.Bits_Per_Sample - w;
				}
			}

			if (do_Mid_Side)
			{
				Debug.Assert(encoder.Protected.Channels == 2);

				for (uint32_t channel = 0; channel < 2; channel++)
				{
					uint32_t w;

					if ((encoder.Protected.Bits_Per_Sample < 32) || (channel == 0))
						w = Get_Wasted_Bits(threadTask.Integer_Signal_Mid_Size[channel], encoder.Protected.BlockSize);
					else
						w = Get_Wasted_Bits_Wide(threadTask.Integer_Signal_33Bit_Side, threadTask.Integer_Signal_Mid_Size[channel], encoder.Protected.BlockSize);

					if (w > encoder.Protected.Bits_Per_Sample)
						w = encoder.Protected.Bits_Per_Sample;

					threadTask.SubFrame_Workspace_Mid_Side[channel][0].Wasted_Bits = threadTask.SubFrame_Workspace_Mid_Side[channel][1].Wasted_Bits = w;
					threadTask.SubFrame_Bps_Mid_Side[channel] = encoder.Protected.Bits_Per_Sample - w + (channel == 0 ? 0U : 1);
				}
			}

			// First do a normal encoding pass of each independent channel
			if (do_Independent)
			{
				for (uint32_t channel = 0; channel < encoder.Protected.Channels; channel++)
				{
					if (encoder.Protected.Limit_Min_Bitrate && all_SubFrames_Constant && ((channel + 1) == encoder.Protected.Channels))
					{
						// This frame contains only constant subframes at this point.
						// To prevent the frame from becoming too small, make sure
						// the last subframe isn't constant
						threadTask.Disable_Constant_SubFrames = true;
					}

					if (!Process_SubFrame(threadTask, min_Partition_Order, max_Partition_Order, frame_Header, threadTask.SubFrame_Bps[channel], threadTask.Integer_Signal[channel],
						    threadTask.SubFrame_Workspace_Ptr[channel], threadTask.Partitioned_Rice_Contents_Workspace_Ptr[channel], threadTask.Residual_Workspace[channel],
						    out threadTask.Best_SubFrame[channel], out threadTask.Best_SubFrame_Bits[channel]))
					{
						return false;
					}

					if (threadTask.SubFrame_Workspace[channel][threadTask.Best_SubFrame[channel]].Type != Flac__SubFrameType.Constant)
						all_SubFrames_Constant = false;
				}
			}

			// Now do mid and side channels if requested
			if (do_Mid_Side)
			{
				Debug.Assert(encoder.Protected.Channels == 2);

				for (uint32_t channel = 0; channel < 2; channel++)
				{
					Array integer_Signal;

					if (threadTask.SubFrame_Bps_Mid_Side[channel] <= 32)
						integer_Signal = threadTask.Integer_Signal_Mid_Size[channel];
					else
						integer_Signal = threadTask.Integer_Signal_33Bit_Side;

					if (!Process_SubFrame(threadTask, min_Partition_Order, max_Partition_Order, frame_Header, threadTask.SubFrame_Bps_Mid_Side[channel], integer_Signal,
						    threadTask.SubFrame_Workspace_Ptr_Mid_Side[channel], threadTask.Partitioned_Rice_Contents_Workspace_Ptr_Mid_Side[channel], threadTask.Residual_Workspace_Mid_Side[channel],
						    out threadTask.Best_SubFrame_Mid_Side[channel], out threadTask.Best_SubFrame_Bits_Mid_Side[channel]))
					{
						return false;
					}
				}
			}

			// Compose the frame bitbuffer
			if ((do_Independent && do_Mid_Side) || encoder.Protected.Loose_Mid_Side_Stereo)
			{
				uint32_t left_Bps = 0;
				uint32_t right_Bps = 0;
				Flac__SubFrame left_SubFrame = null;
				Flac__SubFrame right_SubFrame = null;
				Flac__ChannelAssignment channel_Assignment;

				Debug.Assert(encoder.Protected.Channels == 2);

				if (!encoder.Protected.Loose_Mid_Side_Stereo)
				{
					uint32_t[] bits = new uint32_t[4];	// WATCHOUT - indexed by Flac__ChannelAssignment

					Debug.Assert((int)Flac__ChannelAssignment.Independent == 0);
					Debug.Assert((int)Flac__ChannelAssignment.Left_Side == 1);
					Debug.Assert((int)Flac__ChannelAssignment.Right_Side == 2);
					Debug.Assert((int)Flac__ChannelAssignment.Mid_Side == 3);

					// We have to figure out which channel assignment results in the smallest frame
					bits[(int)Flac__ChannelAssignment.Independent] = threadTask.Best_SubFrame_Bits[0] + threadTask.Best_SubFrame_Bits[1];
					bits[(int)Flac__ChannelAssignment.Left_Side] = threadTask.Best_SubFrame_Bits[0] + threadTask.Best_SubFrame_Bits_Mid_Side[1];
					bits[(int)Flac__ChannelAssignment.Right_Side] = threadTask.Best_SubFrame_Bits[1] + threadTask.Best_SubFrame_Bits_Mid_Side[1];
					bits[(int)Flac__ChannelAssignment.Mid_Side] = threadTask.Best_SubFrame_Bits_Mid_Side[0] + threadTask.Best_SubFrame_Bits_Mid_Side[1];

					channel_Assignment = Flac__ChannelAssignment.Independent;
					uint32_t min_Bits = bits[(int)channel_Assignment];

					// When doing loose mid side stereo, ignore left side
					// and right side options
					for (uint32_t ca = 1; ca <= 3; ca++)
					{
						if (bits[ca] < min_Bits)
						{
							min_Bits = bits[ca];
							channel_Assignment = (Flac__ChannelAssignment)ca;
						}
					}

					frame_Header.Channel_Assignment = channel_Assignment;
				}

				if (!Stream_Encoder_Framing.Flac__Frame_Add_Header(frame_Header, threadTask.Frame))
				{
					encoder.Protected.State = Flac__StreamEncoderState.Framing_Error;
					return false;
				}

				switch (frame_Header.Channel_Assignment)
				{
					case Flac__ChannelAssignment.Independent:
					{
						left_SubFrame = threadTask.SubFrame_Workspace[0][threadTask.Best_SubFrame[0]];
						right_SubFrame = threadTask.SubFrame_Workspace[1][threadTask.Best_SubFrame[1]];
						break;
					}

					case Flac__ChannelAssignment.Left_Side:
					{
						left_SubFrame = threadTask.SubFrame_Workspace[0][threadTask.Best_SubFrame[0]];
						right_SubFrame = threadTask.SubFrame_Workspace_Mid_Side[1][threadTask.Best_SubFrame_Mid_Side[1]];
						break;
					}

					case Flac__ChannelAssignment.Right_Side:
					{
						left_SubFrame = threadTask.SubFrame_Workspace_Mid_Side[1][threadTask.Best_SubFrame_Mid_Side[1]];
						right_SubFrame = threadTask.SubFrame_Workspace[1][threadTask.Best_SubFrame[1]];
						break;
					}

					case Flac__ChannelAssignment.Mid_Side:
					{
						left_SubFrame = threadTask.SubFrame_Workspace_Mid_Side[0][threadTask.Best_SubFrame_Mid_Side[0]];
						right_SubFrame = threadTask.SubFrame_Workspace_Mid_Side[1][threadTask.Best_SubFrame_Mid_Side[1]];
						break;
					}

					default:
					{
						Debug.Assert(false);
						break;
					}
				}

				switch (frame_Header.Channel_Assignment)
				{
					case Flac__ChannelAssignment.Independent:
					{
						left_Bps = threadTask.SubFrame_Bps[0];
						right_Bps = threadTask.SubFrame_Bps[1];
						break;
					}

					case Flac__ChannelAssignment.Left_Side:
					{
						left_Bps = threadTask.SubFrame_Bps[0];
						right_Bps = threadTask.SubFrame_Bps_Mid_Side[1];
						break;
					}

					case Flac__ChannelAssignment.Right_Side:
					{
						left_Bps = threadTask.SubFrame_Bps_Mid_Side[1];
						right_Bps = threadTask.SubFrame_Bps[1];
						break;
					}

					case Flac__ChannelAssignment.Mid_Side:
					{
						left_Bps = threadTask.SubFrame_Bps_Mid_Side[0];
						right_Bps = threadTask.SubFrame_Bps_Mid_Side[1];
						break;
					}

					default:
					{
						Debug.Assert(false);
						break;
					}
				}

				// Note that encoder_Add_SubFrame sets the state for us in case of an error
				if (!Add_SubFrame(frame_Header.BlockSize, left_Bps, left_SubFrame, threadTask.Frame))
					return false;

				if (!Add_SubFrame(frame_Header.BlockSize, right_Bps, right_SubFrame, threadTask.Frame))
					return false;
			}
			else
			{
				Debug.Assert(do_Independent);

				if (!Stream_Encoder_Framing.Flac__Frame_Add_Header(frame_Header, threadTask.Frame))
				{
					encoder.Protected.State = Flac__StreamEncoderState.Framing_Error;
					return false;
				}

				for (uint32_t channel = 0; channel < encoder.Protected.Channels; channel++)
				{
					if (!Add_SubFrame(frame_Header.BlockSize, threadTask.SubFrame_Bps[channel], threadTask.SubFrame_Workspace[channel][threadTask.Best_SubFrame[channel]], threadTask.Frame))
					{
						// The above function sets the state for us in case of an error
						return false;
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
		private Flac__bool Process_SubFrame(Flac__StreamEncoderThreadTask threadTask, uint32_t min_Partition_Order, uint32_t max_Partition_Order, Flac__FrameHeader frame_Header, uint32_t subFrame_Bps, Array integer_Signal, Flac__SubFrame[] subFrame, Flac__EntropyCodingMethod_PartitionedRiceContents[] partitioned_Rice_Contents, Flac__int32[][] residual, out uint32_t best_SubFrame, out uint32_t best_Bits)
		{
			float[] fixed_Residual_Bits_Per_Sample = new float[Constants.Flac__Max_Fixed_Order + 1];
			Apply_Apodization_State_Struct apply_Apodization_State = new Apply_Apodization_State_Struct();
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

			best_Bits = _best_Bits;

			if (frame_Header.BlockSize > Constants.Flac__Max_Fixed_Order)
			{
				Flac__bool signal_Is_Constant = false;

				// The next formula determines when to use a 64-bit accumulator
				// for the error of a fixed predictor, and when a 32-bit one. As
				// the error of a 4th order predictor for a given sample is the
				// sum of 17 sample values (1+4+6+4+1) and there are blocksize -
				// order error values to be summed, the maximum total error is
				// maximum_sample_value * (blocksize - order) * 17. As ilog2(x)
				// calculates floor(2log(x)), the result must be 31 or lower
				uint32_t guess_Fixed_Order;

				if (subFrame_Bps < 28)
				{
					if ((subFrame_Bps + BitMath.Flac__BitMath_ILog2((frame_Header.BlockSize - Constants.Flac__Max_Fixed_Order) * 17)) < 32)
						guess_Fixed_Order = encoder.Private.Fixed.Compute_Best_Predictor((Flac__int32[])integer_Signal, Constants.Flac__Max_Fixed_Order, frame_Header.BlockSize - Constants.Flac__Max_Fixed_Order, fixed_Residual_Bits_Per_Sample);
					else
						guess_Fixed_Order = encoder.Private.Fixed.Compute_Best_Predictor_Wide((Flac__int32[])integer_Signal, Constants.Flac__Max_Fixed_Order, frame_Header.BlockSize - Constants.Flac__Max_Fixed_Order, fixed_Residual_Bits_Per_Sample);
				}
				else
				{
					if (subFrame_Bps <= 32)
						guess_Fixed_Order = encoder.Private.Fixed.Compute_Best_Predictor_Limit_Residual((Flac__int32[])integer_Signal, Constants.Flac__Max_Fixed_Order, frame_Header.BlockSize - Constants.Flac__Max_Fixed_Order, fixed_Residual_Bits_Per_Sample);
					else
						guess_Fixed_Order = encoder.Private.Fixed.Compute_Best_Predictor_Limit_Residual_33Bit((Flac__int64[])integer_Signal, Constants.Flac__Max_Fixed_Order, frame_Header.BlockSize - Constants.Flac__Max_Fixed_Order, fixed_Residual_Bits_Per_Sample);
				}

				// Check for constant subframe
				if (!threadTask.Disable_Constant_SubFrames && (fixed_Residual_Bits_Per_Sample[1] == 0.0))
				{
					// The above means it's possible all samples are the same value; now double-check it
					signal_Is_Constant = true;

					if (subFrame_Bps <= 32)
					{
						Flac__int32[] integer_Signal_ = (Flac__int32[])integer_Signal;

						for (uint32_t i = 1; i < frame_Header.BlockSize; i++)
						{
							if (integer_Signal_[0] != integer_Signal_[i])
							{
								signal_Is_Constant = false;
								break;
							}
						}
					}
					else
					{
						Flac__int64[] integer_Signal_ = (Flac__int64[])integer_Signal;

						for (uint32_t i = 1; i < frame_Header.BlockSize; i++)
						{
							if (integer_Signal_[0] != integer_Signal_[i])
							{
								signal_Is_Constant = false;
								break;
							}
						}
					}
				}

				if (signal_Is_Constant)
				{
					uint32_t candidate_Bits;

					if (subFrame_Bps <= 32)
						candidate_Bits = Evaluate_Constant_SubFrame(((Flac__int32[])integer_Signal)[0], frame_Header.BlockSize, subFrame_Bps, subFrame[_best_SubFrame == 0 ? 1U : 0]);
					else
						candidate_Bits = Evaluate_Constant_SubFrame(((Flac__int64[])integer_Signal)[0], frame_Header.BlockSize, subFrame_Bps, subFrame[_best_SubFrame == 0 ? 1U : 0]);

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

							uint32_t invertBest_SubFrame = _best_SubFrame == 0 ? 1U : 0;
							uint32_t candidate_Bits = Evaluate_Fixed_SubFrame(threadTask, integer_Signal, residual[invertBest_SubFrame], threadTask.Abs_Residual_Partition_Sums, threadTask.Raw_Bits_Per_Partition, frame_Header.BlockSize, subFrame_Bps, fixed_Order, rice_Parameter_Limit, min_Partition_Order, max_Partition_Order, subFrame[invertBest_SubFrame], partitioned_Rice_Contents[invertBest_SubFrame]);
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
							apply_Apodization_State.A = 0;
							apply_Apodization_State.B = 1;
							apply_Apodization_State.C = 0;

							while (apply_Apodization_State.A < encoder.Protected.Num_Apodizations)
							{
								uint32_t min_Lpc_Order;
								uint32_t max_Lpc_Order_This_Apodization = max_Lpc_Order;

								if (!Apply_Apodization(threadTask, apply_Apodization_State, frame_Header.BlockSize, lpc_Error, ref max_Lpc_Order_This_Apodization, subFrame_Bps, integer_Signal, out uint32_t guess_Lpc_Order))
								{
									// If Apply_Apodization fails, try next apodization
									continue;
								}

								if (encoder.Protected.Do_Exhaustive_Model_Search)
									min_Lpc_Order = 1;
								else
									min_Lpc_Order = max_Lpc_Order_This_Apodization = guess_Lpc_Order;

								for (uint32_t lpc_Order = min_Lpc_Order; lpc_Order <= max_Lpc_Order_This_Apodization; lpc_Order++)
								{
									double lpc_Residual_Bits_Per_Sample = Lpc.Flac__Lpc_Compute_Expected_Bits_Per_Residual_Sample(lpc_Error[lpc_Order - 1], frame_Header.BlockSize - lpc_Order);
									if (lpc_Residual_Bits_Per_Sample >= subFrame_Bps)
										continue;	// Don't even try

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
										uint32_t candidate_Bits = Evaluate_Lpc_SubFrame(threadTask, integer_Signal, residual[invertBest_SubFrame], threadTask.Abs_Residual_Partition_Sums, threadTask.Raw_Bits_Per_Partition, threadTask.Lp_Coeff[lpc_Order - 1], frame_Header.BlockSize, subFrame_Bps, lpc_Order, qlp_Coeff_Precision, rice_Parameter_Limit, min_Partition_Order, max_Partition_Order, subFrame[invertBest_SubFrame], partitioned_Rice_Contents[invertBest_SubFrame]);
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Set_Next_Subdivide_Tukey(Flac__int32 parts, ref uint32_t apodizations, ref uint32_t current_Depth, ref uint32_t current_Part)
		{
			// Current part is interleaved even are partial, odd are punchout
			if (current_Depth == 2)
			{
				// For depth 2, we only do partial, no punchout as that is almost redundant
				if (current_Part == 0)
					current_Part = 2;
				else	// current_Part == 2
				{
					current_Part = 0;
					current_Depth++;
				}
			}
			else if (current_Part < (2 * current_Depth - 1))
				current_Part++;
			else	// current_Part >= (2 * current_Depth - 1)
			{
				current_Part = 0;
				current_Depth++;
			}

			// Now check if we are done with the SUBDIVIDE_TUKEY apodization
			if (current_Depth > (uint32_t)parts)
			{
				apodizations++;
				current_Depth = 1;
				current_Part = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Flac__bool Apply_Apodization(Flac__StreamEncoderThreadTask threadTask, Apply_Apodization_State_Struct apply_Apodization_State, uint32_t blockSize, double[] lpc_Error, ref uint32_t max_Lpc_Order_This_Apodization, uint32_t subFrame_Bps, Array integer_Signal, out uint32_t guess_Lpc_Order)
		{
			apply_Apodization_State.Current_Apodization = encoder.Protected.Apodizations[apply_Apodization_State.A];

			if (apply_Apodization_State.B == 1)
			{
				// Window full subblock
				if (subFrame_Bps <= 32)
					Lpc.Flac__Lpc_Window_Data((Flac__int32[])integer_Signal, encoder.Private.Window[apply_Apodization_State.A], threadTask.Windowed_Signal, blockSize);
				else
					Lpc.Flac__Lpc_Window_Data_Wide((Flac__int64[])integer_Signal, encoder.Private.Window[apply_Apodization_State.A], threadTask.Windowed_Signal, blockSize);

				encoder.Private.Lpc.Compute_Autocorrelation(threadTask.Windowed_Signal, blockSize, max_Lpc_Order_This_Apodization + 1, apply_Apodization_State.Autoc);

				if (apply_Apodization_State.Current_Apodization.Type == Flac__ApodizationFunction.Subdivide_Tukey)
				{
					Array.Copy(apply_Apodization_State.Autoc, apply_Apodization_State.Autoc_Root, max_Lpc_Order_This_Apodization);
					apply_Apodization_State.B++;
				}
				else
					apply_Apodization_State.A++;
			}
			else
			{
				// Window part of subblock
				if ((blockSize / apply_Apodization_State.B) <= Constants.Flac__Max_Lpc_Order)
				{
					// Intrinsics autocorrelation routines do not all handle cases in which lag might be
					// larger than data_len, and some routines round lag up to the nearest multiple of 4.
					// As little gain is expected from using LPC on part of a signal as small as 32 samples
					// and to enable widening this rounding up to larger values in the future, windowing
					// parts smaller than or equal to FLAC__MAX_LPC_ORDER (which is 32) samples is not supported
					Set_Next_Subdivide_Tukey(((Flac__ApodizationParameter_Subdivide_Tukey)apply_Apodization_State.Current_Apodization.Parameters).Parts, ref apply_Apodization_State.A, ref apply_Apodization_State.B, ref apply_Apodization_State.C);

					guess_Lpc_Order = 0;
					return false;
				}

				if ((apply_Apodization_State.C % 2) == 0)
				{
					// On even c, evaluate the (c/2)th partial window of size blocksize/b
					if (subFrame_Bps <= 32)
						Lpc.Flac__Lpc_Window_Data_Partial((int32_t[])integer_Signal, encoder.Private.Window[apply_Apodization_State.A], threadTask.Windowed_Signal, blockSize, blockSize / apply_Apodization_State.B / 2, (apply_Apodization_State.C / 2 * blockSize) / apply_Apodization_State.B);
					else
						Lpc.Flac__Lpc_Window_Data_Partial_Wide((int64_t[])integer_Signal, encoder.Private.Window[apply_Apodization_State.A], threadTask.Windowed_Signal, blockSize, blockSize / apply_Apodization_State.B / 2, (apply_Apodization_State.C / 2 * blockSize) / apply_Apodization_State.B);

					encoder.Private.Lpc.Compute_Autocorrelation(threadTask.Windowed_Signal, blockSize / apply_Apodization_State.B, max_Lpc_Order_This_Apodization + 1, apply_Apodization_State.Autoc);
				}
				else
				{
					// On uneven c, evaluate the root window (over the whole block) minus the previous partial window
					// similar to tukey_punchout apodization but more efficient
					for (uint32_t i = 0; i < max_Lpc_Order_This_Apodization; i++)
						apply_Apodization_State.Autoc[i] = apply_Apodization_State.Autoc_Root[i] - apply_Apodization_State.Autoc[i];
				}

				// Next function sets a, b and c appropriate for next iteration
				Set_Next_Subdivide_Tukey(((Flac__ApodizationParameter_Subdivide_Tukey)apply_Apodization_State.Current_Apodization.Parameters).Parts, ref apply_Apodization_State.A, ref apply_Apodization_State.B, ref apply_Apodization_State.C);
			}

			if (apply_Apodization_State.Autoc[0] == 0.0f)	// Signal seems to be constant, so we can't dp lp. Constant detection is probably disabled
			{
				guess_Lpc_Order = 0;
				return false;
			}

			Lpc.Flac__Lpc_Compute_Lp_Coefficients(apply_Apodization_State.Autoc, ref max_Lpc_Order_This_Apodization, threadTask.Lp_Coeff, lpc_Error);
			guess_Lpc_Order = Lpc.Flac__Lpc_Compute_Best_Order(lpc_Error, max_Lpc_Order_This_Apodization, blockSize, subFrame_Bps + (encoder.Protected.Do_Qlp_Coeff_Prec_Search ? Constants.Flac__Min_Qlp_Coeff_Precision : encoder.Protected.Qlp_Coeff_Precision));

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
		private uint32_t Evaluate_Constant_SubFrame(Flac__int64 signal, uint32_t blockSize, uint32_t subFrame_Bps, Flac__SubFrame subFrame)
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
		private uint32_t Evaluate_Fixed_SubFrame(Flac__StreamEncoderThreadTask threadTask, Array signal, Flac__int32[] residual, Flac__uint64[] abs_Residual_Partition_Sums, uint32_t[] raw_Bits_Per_Partition, uint32_t blockSize, uint32_t subFrame_Bps, uint32_t order, uint32_t rice_Parameter_Limit, uint32_t min_Partition_Order, uint32_t max_Partition_Order, Flac__SubFrame subFrame, Flac__EntropyCodingMethod_PartitionedRiceContents partition_Rice_Contents)
		{
			uint32_t residual_Samples = blockSize - order;

			if ((subFrame_Bps + order) <= 32)
				Fixed.Flac__Fixed_Compute_Residual((Flac__int32[])signal, order, residual_Samples, order, residual);
			else if (subFrame_Bps <= 32)
				Fixed.Flac__Fixed_Compute_Residual_Wide((Flac__int32[])signal, order, residual_Samples, order, residual);
			else
				Fixed.Flac__Fixed_Compute_Residual_Wide_33Bit((Flac__int64[])signal, order, residual_Samples, order, residual);

			subFrame.Type = Flac__SubFrameType.Fixed;

			Flac__SubFrame_Fixed @fixed = new Flac__SubFrame_Fixed();
			subFrame.Data = @fixed;
			@fixed.Entropy_Coding_Method.Type = Flac__EntropyCodingMethodType.Partitioned_Rice;

			Flac__EntropyCodingMethod_PartitionedRice partitionedRice = new Flac__EntropyCodingMethod_PartitionedRice();
			@fixed.Entropy_Coding_Method.Data = partitionedRice;
			partitionedRice.Contents = partition_Rice_Contents;
			@fixed.Residual = residual;

			uint32_t residual_Bits = Find_Best_Partition_Order(threadTask, residual, abs_Residual_Partition_Sums, raw_Bits_Per_Partition, residual_Samples, order, rice_Parameter_Limit, min_Partition_Order, max_Partition_Order, subFrame_Bps, @fixed.Entropy_Coding_Method);

			@fixed.Order = order;

			if (subFrame_Bps <= 32)
			{
				for (uint32_t i = 0; i < order; i++)
					@fixed.Warmup[i] = ((Flac__int32[])signal)[i];
			}
			else
			{
				for (uint32_t i = 0; i < order; i++)
					@fixed.Warmup[i] = ((Flac__int64[])signal)[i];
			}

			uint32_t estimate = Constants.Flac__SubFrame_Zero_Pad_Len + Constants.Flac__SubFrame_Type_Len + Constants.Flac__SubFrame_Wasted_Bits_Flag_Len + subFrame.Wasted_Bits + (order * subFrame_Bps);
			if (residual_Bits < (uint32_t.MaxValue - estimate))	// To make sure estimate doesn't overflow
				estimate += residual_Bits;
			else
				estimate = uint32_t.MaxValue;

			return estimate;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint32_t Evaluate_Lpc_SubFrame(Flac__StreamEncoderThreadTask threadTask, Array signal, Flac__int32[] residual, Flac__uint64[] abs_Residual_Partition_Sums, uint32_t[] raw_Bits_Per_Partition, Flac__real[] lp_Coeff, uint32_t blockSize, uint32_t subFrame_Bps, uint32_t order, uint32_t qlp_Coeff_Precision, uint32_t rice_Parameter_Limit, uint32_t min_Partition_Order, uint32_t max_Partition_Order, Flac__SubFrame subFrame, Flac__EntropyCodingMethod_PartitionedRiceContents partition_Rice_Contents)
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

			if (Lpc.Flac__Lpc_Max_Residual_Bps(subFrame_Bps, qlp_Coeff, order, quantization) > 32)
			{
				if (subFrame_Bps <= 32)
				{
					if (!Lpc.Flac__Lpc_Compute_Residual_From_Qlp_Coefficients_Limit_Residual((Flac__int32[])signal, order, residual_Samples, qlp_Coeff, order, quantization, residual))
						return 0;
				}
				else
				{
					if (!Lpc.Flac__Lpc_Compute_Residual_From_Qlp_Coefficients_Limit_Residual_33Bit((Flac__int64[])signal, order, residual_Samples, qlp_Coeff, order, quantization, residual))
						return 0;
				}
			}
			else
			{
				if (Lpc.Flac__Lpc_Max_Prediction_Before_Shift_Bps(subFrame_Bps, qlp_Coeff, order) <= 32)
				{
					if ((subFrame_Bps <= 16) && (qlp_Coeff_Precision <= 16))
						encoder.Private.Lpc.Compute_Residual_From_Qlp_Coefficients_16Bit((Flac__int32[])signal, order, residual_Samples, qlp_Coeff, order, quantization, residual);
					else
						encoder.Private.Lpc.Compute_Residual_From_Qlp_Coefficients((Flac__int32[])signal, order, residual_Samples, qlp_Coeff, order, quantization, residual);
				}
				else
					encoder.Private.Lpc.Compute_Residual_From_Qlp_Coefficients_Wide((Flac__int32[])signal, order, residual_Samples, qlp_Coeff, order, quantization, residual);
			}

			subFrame.Type = Flac__SubFrameType.Lpc;

			Flac__SubFrame_Lpc lpc = new Flac__SubFrame_Lpc();
			subFrame.Data = lpc;

			Flac__EntropyCodingMethod_PartitionedRice rice = new Flac__EntropyCodingMethod_PartitionedRice();

			lpc.Entropy_Coding_Method.Type = Flac__EntropyCodingMethodType.Partitioned_Rice;
			lpc.Entropy_Coding_Method.Data = rice;
			rice.Contents = partition_Rice_Contents;
			lpc.Residual = residual;

			uint32_t residual_Bits = Find_Best_Partition_Order(threadTask, residual, abs_Residual_Partition_Sums, raw_Bits_Per_Partition, residual_Samples, order, rice_Parameter_Limit, min_Partition_Order, max_Partition_Order, subFrame_Bps, lpc.Entropy_Coding_Method);

			lpc.Order = order;
			lpc.Qlp_Coeff_Precision = qlp_Coeff_Precision;
			lpc.Quantization_Level = quantization;
			Array.Copy(qlp_Coeff, lpc.Qlp_Coeff, Constants.Flac__Max_Lpc_Order);

			if (subFrame_Bps <= 32)
			{
				for (uint32_t i = 0; i < order; i++)
					lpc.Warmup[i] = ((Flac__int32[])signal)[i];
			}
			else
			{
				for (uint32_t i = 0; i < order; i++)
					lpc.Warmup[i] = ((Flac__int64[])signal)[i];
			}

			uint32_t estimate = Constants.Flac__SubFrame_Zero_Pad_Len + Constants.Flac__SubFrame_Type_Len + Constants.Flac__SubFrame_Wasted_Bits_Flag_Len + subFrame.Wasted_Bits + Constants.Flac__SubFrame_Lpc_Qlp_Coeff_Precision_Len + Constants.Flac__SubFrame_Lpc_Qlp_Shift_Len + (order * (qlp_Coeff_Precision + subFrame_Bps));
			if (residual_Bits < (uint32_t.MaxValue - estimate))	// To make sure estimate doesn't overflow
				estimate += residual_Bits;
			else
				estimate = uint32_t.MaxValue;

			return estimate;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint32_t Evaluate_Verbatim_SubFrame(Array signal, uint32_t blockSize, uint32_t subFrame_Bps, Flac__SubFrame subFrame)
		{
			subFrame.Type = Flac__SubFrameType.Verbatim;

			Flac__SubFrame_Verbatim verbatim = new Flac__SubFrame_Verbatim();
			subFrame.Data = verbatim;

			if (subFrame_Bps <= 32)
			{
				verbatim.Data_Type = Flac__VerbatimSubFrameDataType.Int32;
				verbatim.Data32 = (Flac__int32[])signal;
			}
			else
			{
				verbatim.Data_Type = Flac__VerbatimSubFrameDataType.Int64;
				verbatim.Data64 = (Flac__int64[])signal;
			}

			uint32_t estimate = Constants.Flac__SubFrame_Zero_Pad_Len + Constants.Flac__SubFrame_Type_Len + Constants.Flac__SubFrame_Wasted_Bits_Flag_Len + subFrame.Wasted_Bits + (blockSize * subFrame_Bps);

			return estimate;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint32_t Find_Best_Partition_Order(Flac__StreamEncoderThreadTask threadTask, Flac__int32[] residual, Flac__uint64[] abs_Residual_Partition_Sums, uint32_t[] raw_Bits_Per_Partition, uint32_t residual_Samples, uint32_t predictor_Order, uint32_t rice_Parameter_Limit, uint32_t min_Partition_Order, uint32_t max_Partition_Order, uint32_t bps, Flac__EntropyCodingMethod best_Ecm)
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
					if (!Set_Partitioned_Rice(abs_Residual_Partition_Sums, raw_Bits_Per_Partition, sum, residual_Samples, predictor_Order, rice_Parameter_Limit, (uint32_t)partition_Order, threadTask.Partitioned_Rice_Contents_Extra[best_Parameters_Index == 0 ? 1 : 0], out uint32_t residual_Bits))
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

//			Flac__EntropyCodingMethod_PartitionedRice partitionedRice = new Flac__EntropyCodingMethod_PartitionedRice();
//			partitionedRice.Contents = new Flac__EntropyCodingMethod_PartitionedRiceContents();
//			best_Ecm.Data = partitionedRice;
			Flac__EntropyCodingMethod_PartitionedRice partitionedRice = (Flac__EntropyCodingMethod_PartitionedRice)best_Ecm.Data;
			partitionedRice.Order = best_Partition_Order;

			{
				Flac__EntropyCodingMethod_PartitionedRiceContents prc = partitionedRice.Contents;

				// Save best parameters and raw_Bits
				Array.Copy(threadTask.Partitioned_Rice_Contents_Extra[best_Parameters_Index].Parameters, 0, prc.Parameters, 0, 1 << (int)best_Partition_Order);

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
							abs_Residual_Partition_Sum += (Flac__uint32)Math.Abs(residual[residual_Sample]);	// abs(INT_MIN) is undefined, but if the residual is INT_MIN we have bigger problems

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
							abs_Residual_Partition_Sum64 += (Flac__uint64)Math.Abs(residual[residual_Sample]);	// abs(INT_MIN) is undefined, but if the residual is INT_MIN we have bigger problems

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
		private Flac__bool Set_Partitioned_Rice(Flac__uint64[] abs_Residual_Partition_Sums, uint32_t[] raw_Bits_Per_Partition, int offset, uint32_t residual_Samples, uint32_t predictor_Order, uint32_t rice_Parameter_Limit, uint32_t partition_Order, Flac__EntropyCodingMethod_PartitionedRiceContents partitioned_Rice_Contents, out uint32_t bits)
		{
			uint32_t rice_Parameter;
			uint32_t best_Rice_Parameter = 0;
			uint32_t bits_ = Constants.Flac__Entropy_Coding_Method_Type_Len + Constants.Flac__Entropy_Coding_Method_Partitioned_Rice_Order_Len;
			uint32_t partitions = 1U << (int)partition_Order;

			Debug.Assert(rice_Parameter_Limit <= Constants.Flac__Entropy_Coding_Method_Partitioned_Rice2_Escape_Parameter);

			uint32_t[] parameters = partitioned_Rice_Contents.Parameters;
			uint32_t[] raw_Bits = partitioned_Rice_Contents.Raw_Bits;

			uint32_t partition_Samples_Base = (residual_Samples + predictor_Order) >> (int)partition_Order;

			// Integer division is slow. To speed up things, precalculate a fixed point
			// divisor, as all partitions except the first are the same size. 18 bits
			// are taken because maximum block size is 65535, max partition size for
			// partitions other than 0 is 32767 (15 bit), max abs residual is 2^31,
			// which leaves 18 bit
			uint32_t partition_Samples_Fixed_Point_Divisor_Base = 0x40000 / partition_Samples_Base;

			for (uint32_t partition = 0, residual_Sample = 0; partition < partitions; partition++)
			{
				uint32_t partition_Samples = partition_Samples_Base;
				uint32_t partition_Samples_Fixed_Point_Divisor;

				if (partition > 0)
					partition_Samples_Fixed_Point_Divisor = partition_Samples_Fixed_Point_Divisor_Base;
				else
				{
					if (partition_Samples <= predictor_Order)
					{
						bits = 0;
						return false;
					}
					else
						partition_Samples -= predictor_Order;

					partition_Samples_Fixed_Point_Divisor = 0x40000 / partition_Samples;
				}

				Flac__uint64 mean = abs_Residual_Partition_Sums[partition];

				// 'mean' is not a good name for the variable, it is
				// actually the sum of magnitudes of all residual values
				// in the partition, so the actual mean is
				// mean/partition_samples
				if ((mean < 2) || (((mean - 1) * partition_Samples_Fixed_Point_Divisor) >> 18) == 0)
					rice_Parameter = 0;
				else
					rice_Parameter = BitMath.Flac__BitMath_ILog2_Wide(((mean - 1) * partition_Samples_Fixed_Point_Divisor) >> 18) + 1;

				if (rice_Parameter >= rice_Parameter_Limit)
					rice_Parameter = rice_Parameter_Limit - 1;

				uint32_t best_Partition_Bits = uint32_t.MaxValue;
				uint32_t partition_Bits = Count_Rice_Bits_In_Partition(rice_Parameter, partition_Samples, abs_Residual_Partition_Sums[partition]);

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
		private uint32_t Get_Wasted_Bits_Wide(Flac__int64[] signal_Wide, Flac__int32[] signal, uint32_t samples)
		{
			Flac__int64 x = 0;
			uint32_t shift;

			for (uint32_t i = 0; (i < samples) && ((x & 1) == 0); i++)
				x |= signal_Wide[i];

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
					signal[i] = (Flac__int32)(signal_Wide[i] >> (int)shift);
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
