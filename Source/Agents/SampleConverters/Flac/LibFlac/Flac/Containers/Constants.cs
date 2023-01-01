/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers
{
	/// <summary>
	/// Different constants
	/// </summary>
	internal static class Constants
	{
		/// <summary></summary>
		public const double M_LN2 = 0.69314718055994530942;

		/// <summary></summary>
		public const uint32_t Flac__Bytes_Per_Word = 8;

		/// <summary></summary>
		public const uint32_t Flac__Bits_Per_Word = 64;

		/// <summary></summary>
		public const uint64_t Flac__Word_All_Ones = 0xffffffffffffffff;

		/// <summary></summary>
		public const uint32_t Flac__Max_Extra_Residual_Bps = 4;

		/// <summary></summary>
		public const uint32_t Flac__Max_Apodization_Functions = 32;

		/// <summary>
		/// The largest legal metadata type code
		/// </summary>
		public const int32_t Flac__Max_Metadata_Type_Code = 126;

		/// <summary>
		/// The minimum block size, in samples, permitted by the format
		/// </summary>
		public const uint32_t Flac__Min_Block_Size = 16;

		/// <summary>
		/// The maximum block size, in samples, permitted by the format
		/// </summary>
		public const uint32_t Flac__Max_Block_Size = 65535;

		/// <summary>
		/// The maximum block size, in samples, permitted by the FLAC subset for
		/// sample rates up to 48kHz
		/// </summary>
		public const uint32_t Flac__Subset_Max_Block_Size_48000Hz = 4608;

		/// <summary>
		/// The maximum number of channels permitted by the format
		/// </summary>
		public const uint32_t Flac__Max_Channels = 8;

		/// <summary>
		/// The minimum sample resolution permitted by the format
		/// </summary>
		public const uint32_t Flac__Min_Bits_Per_Sample = 4;

		/// <summary>
		/// The maximum sample resolution permitted by the format
		/// </summary>
		public const uint32_t Flac__Max_Bits_Per_Sample = 32;

		/// <summary>
		/// The maximum sample resolution permitted by libFLAC.
		///
		/// WARNING:
		/// Flac__Max_Bits_Per_Sample is the limit of the FLAC format. However,
		/// the reference encoder/decoder is currently limited to 24 bits because
		/// of prevalent 32-bit math, so make sure and use this value when
		/// appropriate
		/// </summary>
		public const uint32_t Flac__Reference_Codec_Max_Bits_Per_Sample = 24;

		/// <summary>
		/// The maximum sample rate permitted by the format. The value is
		/// ((2 ^ 16) - 1) * 10
		/// </summary>
		public const uint32_t Flac__Max_Sample_Rate = 655350;

		/// <summary>
		/// The maximum LPC order permitted by the format
		/// </summary>
		public const uint32_t Flac__Max_Lpc_Order = 32;

		/// <summary>
		/// The maximum LPC order permitted by the FLAC subset for sample rates
		/// up to 48kHz
		/// </summary>
		public const uint32_t Flac__Subset_Max_Lpc_Order_48000Hz = 12;

		/// <summary>
		/// The minimum quantized linear predictor coefficient precision
		/// permitted by the format
		/// </summary>
		public const uint32_t Flac__Min_Qlp_Coeff_Precision = 5;

		/// <summary>
		/// The maximum quantized linear predictor coefficient precision
		/// permitted by the format
		/// </summary>
		public const uint32_t Flac__Max_Qlp_Coeff_Precision = 15;

		/// <summary>
		/// The maximum order of the fixed predictors permitted by the format
		/// </summary>
		public const uint32_t Flac__Max_Fixed_Order = 4;

		/// <summary>
		/// The maximum Rice partition order permitted by the format
		/// </summary>
		public const uint32_t Flac__Max_Rice_Partition_Order = 15;

		/// <summary>
		/// The maximum Rice partition order permitted by the FLAC Subset
		/// </summary>
		public const uint32_t Flac__Subset_Max_Rice_Partition_Order = 8;

		/// <summary>
		/// The total stream length of the STREAMINFO block in bytes
		/// </summary>
		public const uint32_t Flac__Stream_Metadata_StreamInfo_Length = 34;

		/// <summary>
		/// The total stream length of a metadata block header in bytes
		/// </summary>
		public const uint32_t Flac__Stream_Metadata_Header_Length = 4;

		/// <summary>
		/// The total stream length of a seek point in bytes
		/// </summary>
		public const uint32_t Flac__Stream_Metadata_SeekPoint_Length = 18;

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_StreamInfo_Min_Block_Size_Len = 16;	// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_StreamInfo_Max_Block_Size_Len = 16;	// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_StreamInfo_Min_Frame_Size_Len = 24;	// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_StreamInfo_Max_Frame_Size_Len = 24;	// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_StreamInfo_Sample_Rate_Len = 20;	// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_StreamInfo_Channels_Len = 3;		// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_StreamInfo_Bits_Per_Sample_Len = 5;	// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_StreamInfo_Total_Samples_Len = 36;	// Bits

		/// <summary>
		/// The maximum order of the fixed predictors permitted by the format
		/// </summary>
		public const uint32_t Flac__Stream_Metadata_Application_Id_Len = 32;			// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_SeekPoint_Sample_Number_Len = 64;	// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_SeekPoint_Stream_Offset_Len = 64;	// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_SeekPoint_Frame_Samples_Len = 16;	// Bits

		/// <summary></summary>
		public const uint64_t Flac__Stream_Metadata_SeekPoint_Placeholder = uint64_t.MaxValue;

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_Vorbis_Comment_Entry_Length_Len = 32;// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_Vorbis_Comment_Num_Comments_Len = 32;// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_CueSheet_Index_Offset_Len = 64;		// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_CueSheet_Index_Number_Len = 8;		// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_CueSheet_Index_Reserved_Len = 3 * 8;// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_CueSheet_Track_Offset_Len = 64;		// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_CueSheet_Track_Number_Len = 8;		// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_CueSheet_Track_Isrc_Len = 12 * 8;	// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_CueSheet_Track_Type_Len = 1;		// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_CueSheet_Track_Pre_Emphasis_Len = 1;// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_CueSheet_Track_Reserved_Len = 6 + 13 * 8;// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_CueSheet_Track_Num_Indices_Len = 8;	// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_CueSheet_Media_Catalog_Number_Len = 128 * 8;// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_CueSheet_Lead_In_Len = 64;			// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_CueSheet_Is_Cd_Len = 1;				// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_CueSheet_Reserved_Len = 7 + 258 * 8;// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_CueSheet_Num_Tracks_Len = 8;		// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_Picture_Type_Len = 32;				// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_Picture_Mime_Type_Length_Len = 32;	// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_Picture_Description_Length_Len = 32;// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_Picture_Width_Len = 32;				// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_Picture_Height_Len = 32;			// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_Picture_Depth_Len = 32;				// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_Picture_Colors_Len = 32;			// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_Picture_Data_Length_Len = 32;		// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_Is_Last_Len = 1;					// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_Type_Len = 7;						// Bits

		/// <summary></summary>
		public const uint32_t Flac__Stream_Metadata_Length_Len = 24;					// Bits

		/// <summary></summary>
		public const uint32_t Flac__Frame_Header_Sync = 0x3ffe;

		/// <summary></summary>
		public const uint32_t Flac__Frame_Header_Sync_Len = 14;							// Bits

		/// <summary></summary>
		public const uint32_t Flac__Frame_Header_Reserved_Len = 1;						// Bits

		/// <summary></summary>
		public const uint32_t Flac__Frame_Header_Blocking_Strategy_Len = 1;				// Bits

		/// <summary></summary>
		public const uint32_t Flac__Frame_Header_Block_Size_Len = 4;					// Bits

		/// <summary></summary>
		public const uint32_t Flac__Frame_Header_Sample_Rate_Len = 4;					// Bits

		/// <summary></summary>
		public const uint32_t Flac__Frame_Header_Channel_Assignment_Len = 4;			// Bits

		/// <summary></summary>
		public const uint32_t Flac__Frame_Header_Bits_Per_Sample_Len = 3;				// Bits

		/// <summary></summary>
		public const uint32_t Flac__Frame_Header_Zero_Pad_Len = 1;						// Bits

		/// <summary></summary>
		public const uint32_t Flac__Frame_Header_Crc_Len = 8;							// Bits

		/// <summary></summary>
		public const uint32_t Flac__Frame_Footer_Crc_Len = 16;							// Bits

		/// <summary></summary>
		public const uint32_t Flac__Entropy_Coding_Method_Type_Len = 2;					// Bits

		/// <summary></summary>
		public const uint32_t Flac__Entropy_Coding_Method_Partitioned_Rice_Order_Len = 4;// Bits

		/// <summary></summary>
		public const uint32_t Flac__Entropy_Coding_Method_Partitioned_Rice_Parameter_Len = 4;// Bits

		/// <summary></summary>
		public const uint32_t Flac__Entropy_Coding_Method_Partitioned_Rice2_Parameter_Len = 5;// Bits

		/// <summary></summary>
		public const uint32_t Flac__Entropy_Coding_Method_Partitioned_Rice_Raw_Len = 5;	// Bits

		/// <summary></summary>
		public const uint32_t Flac__Entropy_Coding_Method_Partitioned_Rice_Escape_Parameter = 15;// == (1 << Flac__Entropy_Coding_Method_Partitioned_Rice_Parameter_Len) - 1

		/// <summary></summary>
		public const uint32_t Flac__Entropy_Coding_Method_Partitioned_Rice2_Escape_Parameter = 31;// == (1 << Flac__Entropy_Coding_Method_Partitioned_Rice2_Parameter_Len) - 1

		/// <summary></summary>
		public const uint32_t Flac__SubFrame_Lpc_Qlp_Coeff_Precision_Len = 4;			// Bits

		/// <summary></summary>
		public const uint32_t Flac__SubFrame_Lpc_Qlp_Shift_Len = 5;						// Bits

		/// <summary></summary>
		public const uint32_t Flac__SubFrame_Zero_Pad_Len = 1;							// Bits

		/// <summary></summary>
		public const uint32_t Flac__SubFrame_Type_Len = 6;								// Bits

		/// <summary></summary>
		public const uint32_t Flac__SubFrame_Wasted_Bits_Flag_Len = 1;					// Bits

		/// <summary></summary>
		public const uint32_t Flac__SubFrame_Type_Constant_Byte_Aligned_Mask = 0x00;

		/// <summary></summary>
		public const uint32_t Flac__SubFrame_Type_Verbatim_Byte_Aligned_Mask = 0x02;

		/// <summary></summary>
		public const uint32_t Flac__SubFrame_Type_Fixed_Byte_Aligned_Mask = 0x10;

		/// <summary></summary>
		public const uint32_t Flac__SubFrame_Type_Lpc_Byte_Aligned_Mask = 0x40;

		/// <summary>
		/// The 32-bit integer big-endian representation of the beginning of
		/// a FLAC stream
		/// </summary>
		public const uint32_t Flac__Stream_Sync = 0x664c6143;

		/// <summary>
		/// The length of the FLAC signature in bits
		/// </summary>
		public const uint32_t Flac__Stream_Sync_Len = 32;								// Bits
	}
}
