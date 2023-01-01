/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Diagnostics;
using System.Text;
using Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac;
using Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Private
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Stream_Encoder_Framing
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Add_Metadata_Block(Flac__StreamMetadata metadata, BitWriter bw)
		{
			uint32_t vendor_String_Length = (uint32_t)Encoding.UTF8.GetByteCount(Format.Flac__Vendor_String);

			if (!bw.Flac__BitWriter_Write_Raw_UInt32(metadata.Is_Last ? 1U : 0, Constants.Flac__Stream_Metadata_Is_Last_Len))
				return false;

			if (!bw.Flac__BitWriter_Write_Raw_UInt32((Flac__uint32)metadata.Type, Constants.Flac__Stream_Metadata_Type_Len))
				return false;

			// First, for VORBIS_COMMENTs, adjust the length to reflect our vendor string
			uint32_t i = metadata.Length;

			if (metadata.Type == Flac__MetadataType.Vorbis_Comment)
			{
				Debug.Assert((((Flac__StreamMetadata_VorbisComment)metadata.Data).Vendor_String.Length == 0) || (((Flac__StreamMetadata_VorbisComment)metadata.Data).Vendor_String.Entry != null));

				i -= ((Flac__StreamMetadata_VorbisComment)metadata.Data).Vendor_String.Length;
				i += vendor_String_Length;
			}

			Debug.Assert(i < (1 << (int)Constants.Flac__Stream_Metadata_Length_Len));

			// Double protection
			if (i >= (1 << (int)Constants.Flac__Stream_Metadata_Length_Len))
				return false;

			if (!bw.Flac__BitWriter_Write_Raw_UInt32(i, Constants.Flac__Stream_Metadata_Length_Len))
				return false;

			switch (metadata.Type)
			{
				case Flac__MetadataType.StreamInfo:
				{
					Flac__StreamMetadata_StreamInfo metaStreamInfo = (Flac__StreamMetadata_StreamInfo)metadata.Data;

					Debug.Assert(metaStreamInfo.Min_BlockSize < (1 << (int)Constants.Flac__Stream_Metadata_StreamInfo_Min_Block_Size_Len));

					if (!bw.Flac__BitWriter_Write_Raw_UInt32(metaStreamInfo.Min_BlockSize, Constants.Flac__Stream_Metadata_StreamInfo_Min_Block_Size_Len))
						return false;

					Debug.Assert(metaStreamInfo.Max_BlockSize < (1 << (int)Constants.Flac__Stream_Metadata_StreamInfo_Max_Block_Size_Len));

					if (!bw.Flac__BitWriter_Write_Raw_UInt32(metaStreamInfo.Max_BlockSize, Constants.Flac__Stream_Metadata_StreamInfo_Max_Block_Size_Len))
						return false;

					Debug.Assert(metaStreamInfo.Min_FrameSize < (1 << (int)Constants.Flac__Stream_Metadata_StreamInfo_Min_Frame_Size_Len));

					if (!bw.Flac__BitWriter_Write_Raw_UInt32(metaStreamInfo.Min_FrameSize, Constants.Flac__Stream_Metadata_StreamInfo_Min_Frame_Size_Len))
						return false;

					Debug.Assert(metaStreamInfo.Max_FrameSize < (1 << (int)Constants.Flac__Stream_Metadata_StreamInfo_Max_Frame_Size_Len));

					if (!bw.Flac__BitWriter_Write_Raw_UInt32(metaStreamInfo.Max_FrameSize, Constants.Flac__Stream_Metadata_StreamInfo_Max_Frame_Size_Len))
						return false;

					Debug.Assert(Format.Flac__Format_Sample_Rate_Is_Valid(metaStreamInfo.Sample_Rate));

					if (!bw.Flac__BitWriter_Write_Raw_UInt32(metaStreamInfo.Sample_Rate, Constants.Flac__Stream_Metadata_StreamInfo_Sample_Rate_Len))
						return false;

					Debug.Assert(metaStreamInfo.Channels > 0);
					Debug.Assert(metaStreamInfo.Channels <= (1 << (int)Constants.Flac__Stream_Metadata_StreamInfo_Channels_Len));

					if (!bw.Flac__BitWriter_Write_Raw_UInt32(metaStreamInfo.Channels - 1, Constants.Flac__Stream_Metadata_StreamInfo_Channels_Len))
						return false;

					Debug.Assert(metaStreamInfo.Bits_Per_Sample > 0);
					Debug.Assert(metaStreamInfo.Bits_Per_Sample <= (1 << (int)Constants.Flac__Stream_Metadata_StreamInfo_Bits_Per_Sample_Len));

					if (!bw.Flac__BitWriter_Write_Raw_UInt32(metaStreamInfo.Bits_Per_Sample - 1, Constants.Flac__Stream_Metadata_StreamInfo_Bits_Per_Sample_Len))
						return false;

					Debug.Assert(metaStreamInfo.Total_Samples <= (1L << (int)Constants.Flac__Stream_Metadata_StreamInfo_Total_Samples_Len));

					if (!bw.Flac__BitWriter_Write_Raw_UInt64(metaStreamInfo.Total_Samples, Constants.Flac__Stream_Metadata_StreamInfo_Total_Samples_Len))
						return false;

					if (!bw.Flac__BitWriter_Write_Byte_Block(metaStreamInfo.Md5Sum, 16))
						return false;

					break;
				}

				case Flac__MetadataType.Padding:
				{
					if (!bw.Flac__BitWriter_Write_Zeroes(metadata.Length * 8))
						return false;

					break;
				}

				case Flac__MetadataType.Application:
				{
					Flac__StreamMetadata_Application metaApplication = (Flac__StreamMetadata_Application)metadata.Data;

					if (!bw.Flac__BitWriter_Write_Byte_Block(metaApplication.Id, Constants.Flac__Stream_Metadata_Application_Id_Len / 8))
						return false;

					if (!bw.Flac__BitWriter_Write_Byte_Block(metaApplication.Data, metadata.Length - (Constants.Flac__Stream_Metadata_Application_Id_Len / 8)))
						return false;

					break;
				}

				case Flac__MetadataType.SeekTable:
				{
					Flac__StreamMetadata_SeekTable metaSeekTable = (Flac__StreamMetadata_SeekTable)metadata.Data;

					for (i = 0; i < metaSeekTable.Num_Points; i++)
					{
						if (!bw.Flac__BitWriter_Write_Raw_UInt64(metaSeekTable.Points[i].Sample_Number, Constants.Flac__Stream_Metadata_SeekPoint_Sample_Number_Len))
							return false;

						if (!bw.Flac__BitWriter_Write_Raw_UInt64(metaSeekTable.Points[i].Stream_Offset, Constants.Flac__Stream_Metadata_SeekPoint_Stream_Offset_Len))
							return false;

						if (!bw.Flac__BitWriter_Write_Raw_UInt32(metaSeekTable.Points[i].Frame_Samples, Constants.Flac__Stream_Metadata_SeekPoint_Frame_Samples_Len))
							return false;
					}
					break;
				}

				case Flac__MetadataType.Vorbis_Comment:
				{
					Flac__StreamMetadata_VorbisComment metaVorbisComment = (Flac__StreamMetadata_VorbisComment)metadata.Data;

					if (!bw.Flac__BitWriter_Write_Raw_UInt32_Little_Endian(vendor_String_Length))
						return false;

					if (!bw.Flac__BitWriter_Write_Byte_Block(Encoding.UTF8.GetBytes(Format.Flac__Vendor_String), vendor_String_Length))
						return false;

					if (!bw.Flac__BitWriter_Write_Raw_UInt32_Little_Endian(metaVorbisComment.Num_Comments))
						return false;

					for (i = 0; i < metaVorbisComment.Num_Comments; i++)
					{
						if (!bw.Flac__BitWriter_Write_Raw_UInt32_Little_Endian(metaVorbisComment.Comments[i].Length))
							return false;

						if (!bw.Flac__BitWriter_Write_Byte_Block(metaVorbisComment.Comments[i].Entry, metaVorbisComment.Comments[i].Length))
							return false;
					}
					break;
				}

				case Flac__MetadataType.CueSheet:
				{
					Flac__StreamMetadata_CueSheet metaCueSheet = (Flac__StreamMetadata_CueSheet)metadata.Data;

					Debug.Assert(Constants.Flac__Stream_Metadata_CueSheet_Media_Catalog_Number_Len % 8 == 0);

					if (!bw.Flac__BitWriter_Write_Byte_Block(metaCueSheet.Media_Catalog_Number, Constants.Flac__Stream_Metadata_CueSheet_Media_Catalog_Number_Len / 8))
						return false;

					if (!bw.Flac__BitWriter_Write_Raw_UInt64(metaCueSheet.Lead_In, Constants.Flac__Stream_Metadata_CueSheet_Lead_In_Len))
						return false;

					if (!bw.Flac__BitWriter_Write_Raw_UInt32(metaCueSheet.Is_Cd ? 1U : 0, Constants.Flac__Stream_Metadata_CueSheet_Is_Cd_Len))
						return false;

					if (!bw.Flac__BitWriter_Write_Zeroes(Constants.Flac__Stream_Metadata_CueSheet_Reserved_Len))
						return false;

					if (!bw.Flac__BitWriter_Write_Raw_UInt32(metaCueSheet.Num_Tracks, Constants.Flac__Stream_Metadata_CueSheet_Num_Tracks_Len))
						return false;

					for (i = 0; i < metaCueSheet.Num_Tracks; i++)
					{
						Flac__StreamMetadata_CueSheet_Track track = metaCueSheet.Tracks[i];

						if (!bw.Flac__BitWriter_Write_Raw_UInt64(track.Offset, Constants.Flac__Stream_Metadata_CueSheet_Track_Offset_Len))
							return false;

						if (!bw.Flac__BitWriter_Write_Raw_UInt32(track.Number, Constants.Flac__Stream_Metadata_CueSheet_Track_Number_Len))
							return false;

						Debug.Assert(Constants.Flac__Stream_Metadata_CueSheet_Track_Isrc_Len % 8 == 0);

						if (!bw.Flac__BitWriter_Write_Byte_Block(track.Isrc, Constants.Flac__Stream_Metadata_CueSheet_Track_Isrc_Len / 8))
							return false;

						if (!bw.Flac__BitWriter_Write_Raw_UInt32(track.Type, Constants.Flac__Stream_Metadata_CueSheet_Track_Type_Len))
							return false;

						if (!bw.Flac__BitWriter_Write_Raw_UInt32(track.Pre_Emphasis, Constants.Flac__Stream_Metadata_CueSheet_Track_Pre_Emphasis_Len))
							return false;

						if (!bw.Flac__BitWriter_Write_Zeroes(Constants.Flac__Stream_Metadata_CueSheet_Track_Reserved_Len))
							return false;

						if (!bw.Flac__BitWriter_Write_Raw_UInt32(track.Num_Indices, Constants.Flac__Stream_Metadata_CueSheet_Track_Num_Indices_Len))
							return false;

						for (uint32_t j = 0; j < track.Num_Indices; j++)
						{
							Flac__StreamMetadata_CueSheet_Index indx = track.Indices[j];

							if (!bw.Flac__BitWriter_Write_Raw_UInt64(indx.Offset, Constants.Flac__Stream_Metadata_CueSheet_Index_Offset_Len))
								return false;

							if (!bw.Flac__BitWriter_Write_Raw_UInt32(indx.Number, Constants.Flac__Stream_Metadata_CueSheet_Index_Number_Len))
								return false;

							if (!bw.Flac__BitWriter_Write_Zeroes(Constants.Flac__Stream_Metadata_CueSheet_Index_Reserved_Len))
								return false;
						}
					}
					break;
				}

				case Flac__MetadataType.Picture:
				{
					Flac__StreamMetadata_Picture metaPicture = (Flac__StreamMetadata_Picture)metadata.Data;

					if (!bw.Flac__BitWriter_Write_Raw_UInt32((Flac__uint32)metaPicture.Type, Constants.Flac__Stream_Metadata_Picture_Type_Len))
						return false;

					size_t len = (size_t)metaPicture.Mime_Type.Length - 1;	// Is zero terminated

					if (!bw.Flac__BitWriter_Write_Raw_UInt32(len, Constants.Flac__Stream_Metadata_Picture_Mime_Type_Length_Len))
						return false;

					if (!bw.Flac__BitWriter_Write_Byte_Block(metaPicture.Mime_Type, len))
						return false;

					len = (size_t)metaPicture.Description.Length - 1;	// Is zero terminated

					if (!bw.Flac__BitWriter_Write_Raw_UInt32(len, Constants.Flac__Stream_Metadata_Picture_Description_Length_Len))
						return false;

					if (!bw.Flac__BitWriter_Write_Byte_Block(metaPicture.Description, len))
						return false;

					if (!bw.Flac__BitWriter_Write_Raw_UInt32(metaPicture.Width, Constants.Flac__Stream_Metadata_Picture_Width_Len))
						return false;

					if (!bw.Flac__BitWriter_Write_Raw_UInt32(metaPicture.Height, Constants.Flac__Stream_Metadata_Picture_Height_Len))
						return false;

					if (!bw.Flac__BitWriter_Write_Raw_UInt32(metaPicture.Depth, Constants.Flac__Stream_Metadata_Picture_Depth_Len))
						return false;

					if (!bw.Flac__BitWriter_Write_Raw_UInt32(metaPicture.Colors, Constants.Flac__Stream_Metadata_Picture_Colors_Len))
						return false;

					if (!bw.Flac__BitWriter_Write_Raw_UInt32(metaPicture.Data_Length, Constants.Flac__Stream_Metadata_Picture_Data_Length_Len))
						return false;

					if (!bw.Flac__BitWriter_Write_Byte_Block(metaPicture.Data, metaPicture.Data_Length))
						return false;

					break;
				}

				default:
				{
					if (!bw.Flac__BitWriter_Write_Byte_Block(((Flac__StreamMetadata_Unknown)metadata.Data).Data, metadata.Length))
						return false;

					break;
				}
			}

			Debug.Assert(bw.Flac__BitWriter_Is_Byte_Aligned());

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Frame_Add_Header(Flac__FrameHeader header, BitWriter bw)
		{
			Debug.Assert(bw.Flac__BitWriter_Is_Byte_Aligned());

			if (!bw.Flac__BitWriter_Write_Raw_UInt32(Constants.Flac__Frame_Header_Sync, Constants.Flac__Frame_Header_Sync_Len))
				return false;

			if (!bw.Flac__BitWriter_Write_Raw_UInt32(0, Constants.Flac__Frame_Header_Reserved_Len))
				return false;

			if (!bw.Flac__BitWriter_Write_Raw_UInt32((header.Number_Type == Flac__FrameNumberType.Frame_Number) ? 0U : 1, Constants.Flac__Frame_Header_Blocking_Strategy_Len))
				return false;

			Debug.Assert((header.BlockSize > 0) && (header.BlockSize <= Constants.Flac__Max_Block_Size));

			// When this assertion holds true, any legal blocksize can be expressed in the frame header
			Debug.Assert(Constants.Flac__Max_Block_Size <= 65535);

			uint32_t blockSize_Hint = 0;
			uint32_t u;

			switch (header.BlockSize)
			{
				case 192:
				{
					u = 1;
					break;
				}

				case 576:
				{
					u = 2;
					break;
				}

				case 1152:
				{
					u = 3;
					break;
				}

				case 2304:
				{
					u = 4;
					break;
				}

				case 4608:
				{
					u = 5;
					break;
				}

				case 256:
				{
					u = 8;
					break;
				}

				case 512:
				{
					u = 9;
					break;
				}

				case 1024:
				{
					u = 10;
					break;
				}

				case 2048:
				{
					u = 11;
					break;
				}

				case 4096:
				{
					u = 12;
					break;
				}

				case 8192:
				{
					u = 13;
					break;
				}

				case 16384:
				{
					u = 14;
					break;
				}

				case 32768:
				{
					u = 15;
					break;
				}

				default:
				{
					if (header.BlockSize <= 0x100)
						blockSize_Hint = u = 6;
					else
						blockSize_Hint = u = 7;

					break;
				}
			}

			if (!bw.Flac__BitWriter_Write_Raw_UInt32(u, Constants.Flac__Frame_Header_Block_Size_Len))
				return false;

			Debug.Assert(Format.Flac__Format_Sample_Rate_Is_Valid(header.Sample_Rate));
			uint32_t sample_Rate_Hint = 0;

			switch (header.Sample_Rate)
			{
				case 88200:
				{
					u = 1;
					break;
				}

				case 176400:
				{
					u = 2;
					break;
				}

				case 192000:
				{
					u = 3;
					break;
				}

				case 8000:
				{
					u = 4;
					break;
				}

				case 16000:
				{
					u = 5;
					break;
				}

				case 22050:
				{
					u = 6;
					break;
				}

				case 24000:
				{
					u = 7;
					break;
				}

				case 32000:
				{
					u = 8;
					break;
				}

				case 44100:
				{
					u = 9;
					break;
				}

				case 48000:
				{
					u = 10;
					break;
				}

				case 96000:
				{
					u = 11;
					break;
				}

				default:
				{
					if ((header.Sample_Rate <= 255000) && ((header.Sample_Rate % 1000) == 0))
						sample_Rate_Hint = u = 12;
					else if ((header.Sample_Rate % 10) == 0)
						sample_Rate_Hint = u = 14;
					else if (header.Sample_Rate <= 0xffff)
						sample_Rate_Hint = u = 13;
					else
						u = 0;

					break;
				}
			}

			if (!bw.Flac__BitWriter_Write_Raw_UInt32(u, Constants.Flac__Frame_Header_Sample_Rate_Len))
				return false;

			Debug.Assert((header.Channels > 0) && (header.Channels <= (1 << (int)Constants.Flac__Stream_Metadata_StreamInfo_Channels_Len)) && (header.Channels <= Constants.Flac__Max_Channels));

			switch (header.Channel_Assignment)
			{
				case Flac__ChannelAssignment.Independent:
				{
					u = header.Channels - 1;
					break;
				}

				case Flac__ChannelAssignment.Left_Side:
				{
					Debug.Assert(header.Channels == 2);
					u = 8;
					break;
				}

				case Flac__ChannelAssignment.Right_Side:
				{
					Debug.Assert(header.Channels == 2);
					u = 9;
					break;
				}

				case Flac__ChannelAssignment.Mid_Side:
				{
					Debug.Assert(header.Channels == 2);
					u = 10;
					break;
				}

				default:
				{
					Debug.Assert(false);
					break;
				}
			}

			if (!bw.Flac__BitWriter_Write_Raw_UInt32(u, Constants.Flac__Frame_Header_Channel_Assignment_Len))
				return false;

			Debug.Assert((header.Bits_Per_Sample > 0) && (header.Bits_Per_Sample <= (1 << (int)Constants.Flac__Stream_Metadata_StreamInfo_Bits_Per_Sample_Len)));

			switch (header.Bits_Per_Sample)
			{
				case 8:
				{
					u = 1;
					break;
				}

				case 12:
				{
					u = 2;
					break;
				}

				case 16:
				{
					u = 4;
					break;
				}

				case 20:
				{
					u = 5;
					break;
				}

				case 24:
				{
					u = 6;
					break;
				}

				default:
				{
					u = 0;
					break;
				}
			}

			if (!bw.Flac__BitWriter_Write_Raw_UInt32(u, Constants.Flac__Frame_Header_Bits_Per_Sample_Len))
				return false;

			if (!bw.Flac__BitWriter_Write_Raw_UInt32(0, Constants.Flac__Frame_Header_Zero_Pad_Len))
				return false;

			if (header.Number_Type == Flac__FrameNumberType.Frame_Number)
			{
				if (!bw.Flac__BitWriter_Write_Utf8_UInt32(header.Frame_Number))
					return false;
			}
			else
			{
				if (!bw.Flac__BitWriter_Write_Utf8_UInt64(header.Sample_Number))
					return false;
			}

			if (blockSize_Hint != 0)
			{
				if (!bw.Flac__BitWriter_Write_Raw_UInt32(header.BlockSize - 1, (blockSize_Hint == 6) ? 8U : 16))
					return false;
			}

			switch (sample_Rate_Hint)
			{
				case 12:
				{
					if (!bw.Flac__BitWriter_Write_Raw_UInt32(header.Sample_Rate / 1000, 8))
						return false;

					break;
				}

				case 13:
				{
					if (!bw.Flac__BitWriter_Write_Raw_UInt32(header.Sample_Rate, 16))
						return false;

					break;
				}

				case 14:
				{
					if (!bw.Flac__BitWriter_Write_Raw_UInt32(header.Sample_Rate / 10, 16))
						return false;

					break;
				}
			}

			// Write the CRC
			if (!bw.Flac__BitWriter_Get_Write_Crc8(out Flac__byte crc))
				return false;

			if (!bw.Flac__BitWriter_Write_Raw_UInt32(crc, Constants.Flac__Frame_Header_Crc_Len))
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__SubFrame_Add_Constant(Flac__SubFrame_Constant subFrame, uint32_t subFrame_Bps, uint32_t wasted_Bits, BitWriter bw)
		{
			Flac__bool ok = bw.Flac__BitWriter_Write_Raw_UInt32(Constants.Flac__SubFrame_Type_Constant_Byte_Aligned_Mask | (wasted_Bits != 0 ? 1U : 0), Constants.Flac__SubFrame_Zero_Pad_Len + Constants.Flac__SubFrame_Type_Len + Constants.Flac__SubFrame_Wasted_Bits_Flag_Len) &&
							(wasted_Bits != 0 ? bw.Flac__BitWriter_Write_Unary_Unsigned(wasted_Bits - 1) : true) &&
							bw.Flac__BitWriter_Write_Raw_Int32(subFrame.Value, subFrame_Bps);

			return ok;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__SubFrame_Add_Fixed(Flac__SubFrame_Fixed subFrame, uint32_t residual_Samples, uint32_t subFrame_Bps, uint32_t wasted_Bits, BitWriter bw)
		{
			if (!bw.Flac__BitWriter_Write_Raw_UInt32(Constants.Flac__SubFrame_Type_Fixed_Byte_Aligned_Mask | (subFrame.Order << 1) | (wasted_Bits != 0 ? 1U : 0), Constants.Flac__SubFrame_Zero_Pad_Len + Constants.Flac__SubFrame_Type_Len + Constants.Flac__SubFrame_Wasted_Bits_Flag_Len))
				return false;

			if (wasted_Bits != 0)
			{
				if (!bw.Flac__BitWriter_Write_Unary_Unsigned(wasted_Bits - 1))
					return false;
			}

			for (uint32_t i = 0; i < subFrame.Order; i++)
			{
				if (!bw.Flac__BitWriter_Write_Raw_Int32(subFrame.Warmup[i], subFrame_Bps))
					return false;
			}

			if (!Add_Entropy_Coding_Method(bw, subFrame.Entropy_Coding_Method))
				return false;

			switch (subFrame.Entropy_Coding_Method.Type)
			{
				case Flac__EntropyCodingMethodType.Partitioned_Rice:
				case Flac__EntropyCodingMethodType.Partitioned_Rice2:
				{
					var data = (Flac__EntropyCodingMethod_PartitionedRice)subFrame.Entropy_Coding_Method.Data;

					if (!Add_Residual_Partitioned_Rice(bw, subFrame.Residual, residual_Samples, subFrame.Order, data.Contents.Parameters, data.Contents.Raw_Bits, data.Order, subFrame.Entropy_Coding_Method.Type == Flac__EntropyCodingMethodType.Partitioned_Rice2))
						return false;

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
		public static Flac__bool Flac__SubFrame_Add_Lpc(Flac__SubFrame_Lpc subFrame, uint32_t residual_Samples, uint32_t subFrame_Bps, uint32_t wasted_Bits, BitWriter bw)
		{
			if (!bw.Flac__BitWriter_Write_Raw_UInt32(Constants.Flac__SubFrame_Type_Lpc_Byte_Aligned_Mask | ((subFrame.Order - 1) << 1) | (wasted_Bits != 0 ? 1U : 0), Constants.Flac__SubFrame_Zero_Pad_Len + Constants.Flac__SubFrame_Type_Len + Constants.Flac__SubFrame_Wasted_Bits_Flag_Len))
				return false;

			if (wasted_Bits != 0)
			{
				if (!bw.Flac__BitWriter_Write_Unary_Unsigned(wasted_Bits - 1))
					return false;
			}

			for (uint32_t i = 0; i < subFrame.Order; i++)
			{
				if (!bw.Flac__BitWriter_Write_Raw_Int32(subFrame.Warmup[i], subFrame_Bps))
					return false;
			}

			if (!bw.Flac__BitWriter_Write_Raw_UInt32(subFrame.Qlp_Coeff_Precision - 1, Constants.Flac__SubFrame_Lpc_Qlp_Coeff_Precision_Len))
				return false;

			if (!bw.Flac__BitWriter_Write_Raw_Int32(subFrame.Quantization_Level, Constants.Flac__SubFrame_Lpc_Qlp_Shift_Len))
				return false;

			for (uint32_t i = 0; i < subFrame.Order; i++)
			{
				if (!bw.Flac__BitWriter_Write_Raw_Int32(subFrame.Qlp_Coeff[i], subFrame.Qlp_Coeff_Precision))
					return false;
			}

			if (!Add_Entropy_Coding_Method(bw, subFrame.Entropy_Coding_Method))
				return false;

			switch (subFrame.Entropy_Coding_Method.Type)
			{
				case Flac__EntropyCodingMethodType.Partitioned_Rice:
				case Flac__EntropyCodingMethodType.Partitioned_Rice2:
				{
					var data = (Flac__EntropyCodingMethod_PartitionedRice)subFrame.Entropy_Coding_Method.Data;

					if (!Add_Residual_Partitioned_Rice(bw, subFrame.Residual, residual_Samples, subFrame.Order, data.Contents.Parameters, data.Contents.Raw_Bits, data.Order, subFrame.Entropy_Coding_Method.Type == Flac__EntropyCodingMethodType.Partitioned_Rice2))
						return false;

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
		public static Flac__bool Flac__SubFrame_Add_Verbatim(Flac__SubFrame_Verbatim subFrame, uint32_t samples, uint32_t subFrame_Bps, uint32_t wasted_Bits, BitWriter bw)
		{
			Flac__int32[] signal = subFrame.Data;

			if (!bw.Flac__BitWriter_Write_Raw_UInt32(Constants.Flac__SubFrame_Type_Verbatim_Byte_Aligned_Mask | (wasted_Bits != 0 ? 1U : 0), Constants.Flac__SubFrame_Zero_Pad_Len + Constants.Flac__SubFrame_Type_Len + Constants.Flac__SubFrame_Wasted_Bits_Flag_Len))
				return false;

			if (wasted_Bits != 0)
			{
				if (!bw.Flac__BitWriter_Write_Unary_Unsigned(wasted_Bits - 1))
					return false;
			}

			for (uint32_t i = 0; i < samples; i++)
			{
				if (!bw.Flac__BitWriter_Write_Raw_Int32(signal[i], subFrame_Bps))
					return false;
			}

			return true;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__bool Add_Entropy_Coding_Method(BitWriter bw, Flac__EntropyCodingMethod method)
		{
			if (!bw.Flac__BitWriter_Write_Raw_UInt32((Flac__uint32)method.Type, Constants.Flac__Entropy_Coding_Method_Type_Len))
				return false;

			switch (method.Type)
			{
				case Flac__EntropyCodingMethodType.Partitioned_Rice:
				case Flac__EntropyCodingMethodType.Partitioned_Rice2:
				{
					if (!bw.Flac__BitWriter_Write_Raw_UInt32(((Flac__EntropyCodingMethod_PartitionedRice)method.Data).Order, Constants.Flac__Entropy_Coding_Method_Partitioned_Rice_Order_Len))
						return false;

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
		private static Flac__bool Add_Residual_Partitioned_Rice(BitWriter bw, Flac__int32[] residual, uint32_t residual_Samples, uint32_t predictor_Order, uint32_t[] rice_Parameters, uint32_t[] raw_Bits, uint32_t partition_Order, Flac__bool is_Extended)
		{
			uint32_t pLen = is_Extended ? Constants.Flac__Entropy_Coding_Method_Partitioned_Rice2_Parameter_Len : Constants.Flac__Entropy_Coding_Method_Partitioned_Rice_Parameter_Len;
			uint32_t pEsc = is_Extended ? Constants.Flac__Entropy_Coding_Method_Partitioned_Rice2_Escape_Parameter : Constants.Flac__Entropy_Coding_Method_Partitioned_Rice_Escape_Parameter;

			if (partition_Order == 0)
			{
				if (raw_Bits[0] == 0)
				{
					if (!bw.Flac__BitWriter_Write_Raw_UInt32(rice_Parameters[0], pLen))
						return false;

					if (!bw.Flac__BitWriter_Write_Rice_Signed_Block(residual, 0, residual_Samples, rice_Parameters[0]))
						return false;
				}
				else
				{
					Debug.Assert(rice_Parameters[0] == 0);

					if (!bw.Flac__BitWriter_Write_Raw_UInt32(pEsc, pLen))
						return false;

					if (!bw.Flac__BitWriter_Write_Raw_UInt32(raw_Bits[0], Constants.Flac__Entropy_Coding_Method_Partitioned_Rice_Raw_Len))
						return false;

					for (uint32_t i = 0; i < residual_Samples; i++)
					{
						if (!bw.Flac__BitWriter_Write_Raw_Int32(residual[i], raw_Bits[0]))
							return false;
					}
				}

				return true;
			}
			else
			{
				uint32_t k = 0, k_Last = 0;
				uint32_t default_Partition_Samples = (residual_Samples + predictor_Order) >> (int)partition_Order;

				for (uint32_t i = 0; i < (1 << (int)partition_Order); i++)
				{
					uint32_t partition_Samples = default_Partition_Samples;

					if (i == 0)
						partition_Samples -= predictor_Order;

					k += partition_Samples;

					if (raw_Bits[i] == 0)
					{
						if (!bw.Flac__BitWriter_Write_Raw_UInt32(rice_Parameters[i], pLen))
							return false;

						if (!bw.Flac__BitWriter_Write_Rice_Signed_Block(residual, k_Last, k - k_Last, rice_Parameters[i]))
							return false;
					}
					else
					{
						if (!bw.Flac__BitWriter_Write_Raw_UInt32(pEsc, pLen))
							return false;

						if (!bw.Flac__BitWriter_Write_Raw_UInt32(raw_Bits[i], Constants.Flac__Entropy_Coding_Method_Partitioned_Rice_Raw_Len))
							return false;

						for (uint32_t j = k_Last; j < k; j++)
						{
							if (!bw.Flac__BitWriter_Write_Raw_Int32(residual[j], raw_Bits[i]))
								return false;
						}
					}

					k_Last = k;
				}

				return true;
			}
		}
		#endregion
	}
}
