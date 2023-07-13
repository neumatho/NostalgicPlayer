/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Linq;
using System.Text;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers;
using Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibFlac.Test.Common
{
	/// <summary>
	/// 
	/// </summary>
	internal class Metadata_Utils
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Init_Metadata_Blocks(out Flac__StreamMetadata streamInfo, out Flac__StreamMetadata padding, out Flac__StreamMetadata seekTable, out Flac__StreamMetadata application1, out Flac__StreamMetadata application2, out Flac__StreamMetadata vorbisComment, out Flac__StreamMetadata cueSheet, out Flac__StreamMetadata picture, out Flac__StreamMetadata unknown)
		{
			// Most of the actual numbers and data in the blocks don't matter,
			// we just want to make sure the decoder parses them correctly.
			//
			// Remember, the metadata interface gets tested after the decoders,
			// so we do all the metadata manipulation here without it

			// Min/max framesize and md5sum don't get written at first, so we have to leave them 0
			streamInfo = new Flac__StreamMetadata();
			streamInfo.Is_Last = false;
			streamInfo.Type = Flac__MetadataType.StreamInfo;
			streamInfo.Length = Constants.Flac__Stream_Metadata_StreamInfo_Length;

			Flac__StreamMetadata_StreamInfo metaStreamInfo = new Flac__StreamMetadata_StreamInfo();
			streamInfo.Data = metaStreamInfo;

			metaStreamInfo.Min_BlockSize = 576;
			metaStreamInfo.Max_BlockSize = 576;
			metaStreamInfo.Min_FrameSize = 0;
			metaStreamInfo.Max_FrameSize = 0;
			metaStreamInfo.Sample_Rate = 44100;
			metaStreamInfo.Channels = 1;
			metaStreamInfo.Bits_Per_Sample = 8;
			metaStreamInfo.Total_Samples = 0;
			Array.Clear(metaStreamInfo.Md5Sum);

			padding = new Flac__StreamMetadata();
			padding.Is_Last = false;
			padding.Type = Flac__MetadataType.Padding;
			padding.Length = 1234;

			Flac__StreamMetadata_Padding metaPadding = new Flac__StreamMetadata_Padding();
			padding.Data = metaPadding;

			seekTable = new Flac__StreamMetadata();
			seekTable.Is_Last = false;
			seekTable.Type = Flac__MetadataType.SeekTable;

			Flac__StreamMetadata_SeekTable metaSeekTable = new Flac__StreamMetadata_SeekTable();
			seekTable.Data = metaSeekTable;

			metaSeekTable.Num_Points = 2;
			seekTable.Length = metaSeekTable.Num_Points * Constants.Flac__Stream_Metadata_SeekPoint_Length;
			metaSeekTable.Points = ArrayHelper.InitializeArray<Flac__StreamMetadata_SeekPoint>((int)metaSeekTable.Num_Points);
			metaSeekTable.Points[0].Sample_Number = 0;
			metaSeekTable.Points[0].Stream_Offset = 0;
			metaSeekTable.Points[0].Frame_Samples = metaStreamInfo.Min_BlockSize;
			metaSeekTable.Points[1].Sample_Number = Constants.Flac__Stream_Metadata_SeekPoint_Placeholder;
			metaSeekTable.Points[1].Stream_Offset = 1000;
			metaSeekTable.Points[1].Frame_Samples = metaStreamInfo.Min_BlockSize;

			application1 = new Flac__StreamMetadata();
			application1.Is_Last = false;
			application1.Type = Flac__MetadataType.Application;
			application1.Length = 8;

			Flac__StreamMetadata_Application metaApplication = new Flac__StreamMetadata_Application();
			application1.Data = metaApplication;

			Array.Copy(Encoding.ASCII.GetBytes("This"), metaApplication.Id, 4);
			metaApplication.Data = new byte[] { 0xf0, 0xe1, 0xd2, 0xc3 };

			application2 = new Flac__StreamMetadata();
			application2.Is_Last = false;
			application2.Type = Flac__MetadataType.Application;
			application2.Length = 4;

			metaApplication = new Flac__StreamMetadata_Application();
			application2.Data = metaApplication;

			Array.Copy(Encoding.ASCII.GetBytes("Here"), metaApplication.Id, 4);
			metaApplication.Data = null;

			byte[] vendor_String = Encoding.UTF8.GetBytes("reference libFLAC 1.3.4 20220220");
			uint32_t vendor_String_Length = (uint32_t)vendor_String.Length;

			vorbisComment = new Flac__StreamMetadata();
			vorbisComment.Is_Last = false;
			vorbisComment.Type = Flac__MetadataType.Vorbis_Comment;
			vorbisComment.Length = (4 + vendor_String_Length) + 4 + (4 + 5) + (4 + 0);

			Flac__StreamMetadata_VorbisComment metaVorbisComment = new Flac__StreamMetadata_VorbisComment();
			vorbisComment.Data = metaVorbisComment;

			metaVorbisComment.Vendor_String.Length = vendor_String_Length;
			metaVorbisComment.Vendor_String.Entry = new Flac__byte[vendor_String_Length + 1];
			Array.Copy(vendor_String, metaVorbisComment.Vendor_String.Entry, vendor_String_Length);
			metaVorbisComment.Num_Comments = 2;
			metaVorbisComment.Comments = ArrayHelper.InitializeArray<Flac__StreamMetadata_VorbisComment_Entry>((int)metaVorbisComment.Num_Comments);
			metaVorbisComment.Comments[0].Length = 5;
			metaVorbisComment.Comments[0].Entry = new Flac__byte[5 + 1];
			Array.Copy(Encoding.UTF8.GetBytes("ab=cd"), metaVorbisComment.Comments[0].Entry, 5);
			metaVorbisComment.Comments[1].Length = 0;
			metaVorbisComment.Comments[1].Entry = new Flac__byte[1];
			metaVorbisComment.Comments[1].Entry[0] = 0;

			cueSheet = new Flac__StreamMetadata();
			cueSheet.Is_Last = false;
			cueSheet.Type = Flac__MetadataType.CueSheet;
			cueSheet.Length =
				// Cuesheet guts
				(
					Constants.Flac__Stream_Metadata_CueSheet_Media_Catalog_Number_Len +
					Constants.Flac__Stream_Metadata_CueSheet_Lead_In_Len + 
					Constants.Flac__Stream_Metadata_CueSheet_Is_Cd_Len + 
					Constants.Flac__Stream_Metadata_CueSheet_Reserved_Len +
					Constants.Flac__Stream_Metadata_CueSheet_Num_Tracks_Len
				) / 8 +
				// 2 tracks
				3 * (
					Constants.Flac__Stream_Metadata_CueSheet_Track_Offset_Len +
					Constants.Flac__Stream_Metadata_CueSheet_Track_Number_Len +
					Constants.Flac__Stream_Metadata_CueSheet_Track_Isrc_Len +
					Constants.Flac__Stream_Metadata_CueSheet_Track_Type_Len +
					Constants.Flac__Stream_Metadata_CueSheet_Track_Pre_Emphasis_Len +
					Constants.Flac__Stream_Metadata_CueSheet_Track_Reserved_Len +
					Constants.Flac__Stream_Metadata_CueSheet_Track_Num_Indices_Len
				) / 8 +
				// 3 index points
				3 * (
					Constants.Flac__Stream_Metadata_CueSheet_Index_Offset_Len +
					Constants.Flac__Stream_Metadata_CueSheet_Index_Number_Len +
					Constants.Flac__Stream_Metadata_CueSheet_Index_Reserved_Len
				) / 8;

			Flac__StreamMetadata_CueSheet metaCueSheet = new Flac__StreamMetadata_CueSheet();
			cueSheet.Data = metaCueSheet;

			metaCueSheet.Media_Catalog_Number[0] = (byte)'j';
			metaCueSheet.Media_Catalog_Number[1] = (byte)'C';
			metaCueSheet.Lead_In = 2 * 44100;
			metaCueSheet.Is_Cd = true;
			metaCueSheet.Num_Tracks = 3;
			metaCueSheet.Tracks = ArrayHelper.InitializeArray<Flac__StreamMetadata_CueSheet_Track>((int)metaCueSheet.Num_Tracks);
			metaCueSheet.Tracks[0].Offset = 0;
			metaCueSheet.Tracks[0].Number = 1;
			Array.Copy(Encoding.ASCII.GetBytes("ACBDE1234567"), metaCueSheet.Tracks[0].Isrc, 12);
			metaCueSheet.Tracks[0].Type = 0;
			metaCueSheet.Tracks[0].Pre_Emphasis = 1;
			metaCueSheet.Tracks[0].Num_Indices = 2;
			metaCueSheet.Tracks[0].Indices = ArrayHelper.InitializeArray<Flac__StreamMetadata_CueSheet_Index>(metaCueSheet.Tracks[0].Num_Indices);
			metaCueSheet.Tracks[0].Indices[0].Offset = 0;
			metaCueSheet.Tracks[0].Indices[0].Number = 0;
			metaCueSheet.Tracks[0].Indices[1].Offset = 123 * 588;
			metaCueSheet.Tracks[0].Indices[1].Number = 1;
			metaCueSheet.Tracks[1].Offset = 1234 * 588;
			metaCueSheet.Tracks[1].Number = 2;
			Array.Copy(Encoding.ASCII.GetBytes("ACBDE7654321"), metaCueSheet.Tracks[1].Isrc, 12);
			metaCueSheet.Tracks[1].Type = 1;
			metaCueSheet.Tracks[1].Pre_Emphasis = 0;
			metaCueSheet.Tracks[1].Num_Indices = 1;
			metaCueSheet.Tracks[1].Indices = ArrayHelper.InitializeArray<Flac__StreamMetadata_CueSheet_Index>(metaCueSheet.Tracks[1].Num_Indices);
			metaCueSheet.Tracks[1].Indices[0].Offset = 0;
			metaCueSheet.Tracks[1].Indices[0].Number = 1;
			metaCueSheet.Tracks[2].Offset = 12345 * 588;
			metaCueSheet.Tracks[2].Number = 170;
			metaCueSheet.Tracks[2].Num_Indices = 0;

			picture = new Flac__StreamMetadata();
			picture.Is_Last = false;
			picture.Type = Flac__MetadataType.Picture;
			picture.Length =
				(
					Constants.Flac__Stream_Metadata_Picture_Type_Len +
					Constants.Flac__Stream_Metadata_Picture_Mime_Type_Length_Len + // Will add the length for the string later
					Constants.Flac__Stream_Metadata_Picture_Description_Length_Len + // Will add the length for the string later
					Constants.Flac__Stream_Metadata_Picture_Width_Len +
					Constants.Flac__Stream_Metadata_Picture_Height_Len +
					Constants.Flac__Stream_Metadata_Picture_Depth_Len +
					Constants.Flac__Stream_Metadata_Picture_Colors_Len +
					Constants.Flac__Stream_Metadata_Picture_Data_Length_Len // Will add the length for the data later
				) / 8;

			Flac__StreamMetadata_Picture metaPicture = new Flac__StreamMetadata_Picture();
			picture.Data = metaPicture;

			metaPicture.Type = Flac__StreamMetadata_Picture_Type.Front_Cover;
			picture.Length += Strdup("image/jpeg", Encoding.ASCII, out metaPicture.Mime_Type);
			picture.Length += Strdup("desc", Encoding.UTF8, out metaPicture.Description);
			metaPicture.Width = 300;
			metaPicture.Height = 300;
			metaPicture.Depth = 24;
			metaPicture.Colors = 0;
			metaPicture.Data_Length = 12;
			metaPicture.Data = new byte[metaPicture.Data_Length];
			Array.Copy(Encoding.ASCII.GetBytes("SOMEJPEGDATA"), metaPicture.Data, 12);
			picture.Length += metaPicture.Data_Length;
			
			unknown = new Flac__StreamMetadata();
			unknown.Is_Last = true;
			unknown.Type = (Flac__MetadataType)126;
			unknown.Length = 8;

			Flac__StreamMetadata_Unknown metaUnknown = new Flac__StreamMetadata_Unknown();
			unknown.Data = metaUnknown;

			metaUnknown.Data = new byte[] { 0xfe, 0xdc, 0xba, 0x98, 0xf0, 0xe1, 0xd2, 0xc3 };
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Flac__bool Compare_Block(Flac__StreamMetadata block, Flac__StreamMetadata blockCopy)
		{
			if (blockCopy.Type != block.Type)
			{
				Console.WriteLine("FAILED, Type mismatch, expected {0}, got {1}", block.Type, blockCopy.Type);
				return false;
			}

			if (blockCopy.Is_Last != block.Is_Last)
			{
				Console.WriteLine("FAILED, Is_Last mismatch, expected {0}, got {1}", block.Is_Last, blockCopy.Is_Last);
				return false;
			}

			if (blockCopy.Length != block.Length)
			{
				Console.WriteLine("FAILED, Length mismatch, expected {0}, got {1}", block.Length, blockCopy.Length);
				return false;
			}

			switch (block.Type)
			{
				case Flac__MetadataType.StreamInfo:
					return Compare_Block_Data_StreamInfo((Flac__StreamMetadata_StreamInfo)block.Data, (Flac__StreamMetadata_StreamInfo)blockCopy.Data);

				case Flac__MetadataType.Padding:
					return Compare_Block_Data_Padding((Flac__StreamMetadata_Padding)block.Data, (Flac__StreamMetadata_Padding)blockCopy.Data, block.Length);

				case Flac__MetadataType.Application:
					return Compare_Block_Data_Application((Flac__StreamMetadata_Application)block.Data, (Flac__StreamMetadata_Application)blockCopy.Data, block.Length);

				case Flac__MetadataType.SeekTable:
					return Compare_Block_Data_SeekTable((Flac__StreamMetadata_SeekTable)block.Data, (Flac__StreamMetadata_SeekTable)blockCopy.Data);

				case Flac__MetadataType.Vorbis_Comment:
					return Compare_Block_Data_VorbisComment((Flac__StreamMetadata_VorbisComment)block.Data, (Flac__StreamMetadata_VorbisComment)blockCopy.Data);

				case Flac__MetadataType.CueSheet:
					return Compare_Block_Data_CueSheet((Flac__StreamMetadata_CueSheet)block.Data, (Flac__StreamMetadata_CueSheet)blockCopy.Data);

				case Flac__MetadataType.Picture:
					return Compare_Block_Data_Picture((Flac__StreamMetadata_Picture)block.Data, (Flac__StreamMetadata_Picture)blockCopy.Data);

				default:
					return Compare_Block_Data_Unknown((Flac__StreamMetadata_Unknown)block.Data, (Flac__StreamMetadata_Unknown)blockCopy.Data, block.Length);
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static uint32_t Strdup(string str, Encoding encoding, out byte[] dup)
		{
			byte[] bytes = encoding.GetBytes(str);
			dup = new byte[bytes.Length + 1];
			Array.Copy(bytes, dup, bytes.Length);

			return (uint32_t)bytes.Length;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__bool Compare_Block_Data_StreamInfo(Flac__StreamMetadata_StreamInfo block, Flac__StreamMetadata_StreamInfo blockCopy)
		{
			if (blockCopy.Min_BlockSize != block.Min_BlockSize)
			{
				Console.WriteLine("FAILED, Min_BlockSize mismatch, expected {0}, got {1}", block.Min_BlockSize, blockCopy.Min_BlockSize);
				return false;
			}

			if (blockCopy.Max_BlockSize != block.Max_BlockSize)
			{
				Console.WriteLine("FAILED, Max_BlockSize mismatch, expected {0}, got {1}", block.Max_BlockSize, blockCopy.Max_BlockSize);
				return false;
			}

			if (blockCopy.Min_FrameSize != block.Min_FrameSize)
			{
				Console.WriteLine("FAILED, Min_FrameSize mismatch, expected {0}, got {1}", block.Min_FrameSize, blockCopy.Min_FrameSize);
				return false;
			}

			if (blockCopy.Max_FrameSize != block.Max_FrameSize)
			{
				Console.WriteLine("FAILED, Max_FrameSize mismatch, expected {0}, got {1}", block.Max_FrameSize, blockCopy.Max_FrameSize);
				return false;
			}

			if (blockCopy.Sample_Rate != block.Sample_Rate)
			{
				Console.WriteLine("FAILED, Sample_Rate mismatch, expected {0}, got {1}", block.Sample_Rate, blockCopy.Sample_Rate);
				return false;
			}

			if (blockCopy.Channels != block.Channels)
			{
				Console.WriteLine("FAILED, Channels mismatch, expected {0}, got {1}", block.Channels, blockCopy.Channels);
				return false;
			}

			if (blockCopy.Bits_Per_Sample != block.Bits_Per_Sample)
			{
				Console.WriteLine("FAILED, Bits_Per_Sample mismatch, expected {0}, got {1}", block.Bits_Per_Sample, blockCopy.Bits_Per_Sample);
				return false;
			}

			if (blockCopy.Total_Samples != block.Total_Samples)
			{
				Console.WriteLine("FAILED, Total_Samples mismatch, expected {0}, got {1}", block.Total_Samples, blockCopy.Total_Samples);
				return false;
			}

			if (!blockCopy.Md5Sum.AsSpan().SequenceEqual(block.Md5Sum))
			{
				Console.WriteLine("FAILED, Md5Sum mismatch, expected {0}, got {1}", BitConverter.ToString(block.Md5Sum), BitConverter.ToString(blockCopy.Md5Sum));
				return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__bool Compare_Block_Data_Padding(Flac__StreamMetadata_Padding block, Flac__StreamMetadata_Padding blockCopy, uint32_t block_Length)
		{
			// We don't compare the padding guts
			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__bool Compare_Block_Data_Application(Flac__StreamMetadata_Application block, Flac__StreamMetadata_Application blockCopy, uint32_t block_Length)
		{
			if (block_Length < block.Id.Length)
			{
				Console.WriteLine("FAILED, bad block length = {0}", block_Length);
				return false;
			}

			if (!blockCopy.Id.AsSpan().SequenceEqual(block.Id))
			{
				Console.WriteLine("FAILED, id mismatch, expected {0:X}{1:X}{2:X}{3:X}, got {4:X}{5:X}{6:X}{7:X}", block.Id[0], block.Id[1], block.Id[2], block.Id[3], blockCopy.Id[0], blockCopy.Id[1], blockCopy.Id[2], blockCopy.Id[3]);
				return false;
			}

			if ((block.Data == null) || (blockCopy.Data == null))
			{
				if (block.Data != blockCopy.Data)
				{
					Console.WriteLine("FAILED, data mismatch ({0}'s data pointer is null)", block.Data == null ? "original" : "copy");
					return false;
				}
				else if ((block_Length - block.Id.Length) > 0)
				{
					Console.WriteLine("FAILED, data pointer is null but block length is not 0");
					return false;
				}
			}
			else
			{
				if ((block_Length - block.Id.Length) == 0)
				{
					Console.WriteLine("FAILED, data pointer is not null but block length is 0");
					return false;
				}
				else if (!blockCopy.Data.AsSpan().SequenceEqual(block.Data))
				{
					Console.WriteLine("FAILED, data mismatch");
					return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__bool Compare_Block_Data_SeekTable(Flac__StreamMetadata_SeekTable block, Flac__StreamMetadata_SeekTable blockCopy)
		{
			if (blockCopy.Num_Points != block.Num_Points)
			{
				Console.WriteLine("FAILED, Num_Points mismatch, expected {0}, got {1}", block.Num_Points, blockCopy.Num_Points);
				return false;
			}

			for (uint32_t i = 0; i < block.Num_Points; i++)
			{
				if (blockCopy.Points[i].Sample_Number != block.Points[i].Sample_Number)
				{
					Console.WriteLine("FAILED, Points[{0}].Sample_Number mismatch, expected {1}, got {2}", i, block.Points[i].Sample_Number, blockCopy.Points[i].Sample_Number);
					return false;
				}

				if (blockCopy.Points[i].Stream_Offset != block.Points[i].Stream_Offset)
				{
					Console.WriteLine("FAILED, Points[{0}].Stream_Offset mismatch, expected {1}, got {2}", i, block.Points[i].Stream_Offset, blockCopy.Points[i].Stream_Offset);
					return false;
				}

				if (blockCopy.Points[i].Frame_Samples != block.Points[i].Frame_Samples)
				{
					Console.WriteLine("FAILED, Points[{0}].Frame_Samples mismatch, expected {1}, got {2}", i, block.Points[i].Frame_Samples, blockCopy.Points[i].Frame_Samples);
					return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__bool Compare_Block_Data_VorbisComment(Flac__StreamMetadata_VorbisComment block, Flac__StreamMetadata_VorbisComment blockCopy)
		{
			if (blockCopy.Vendor_String.Length != block.Vendor_String.Length)
			{
				Console.WriteLine("FAILED, Vendor_String.Length mismatch, expected {0}, got {1}", block.Vendor_String.Length, blockCopy.Vendor_String.Length);
				return false;
			}

			if ((block.Vendor_String.Entry == null) || (blockCopy.Vendor_String.Entry == null))
			{
				if (block.Vendor_String.Entry != blockCopy.Vendor_String.Entry)
				{
					Console.WriteLine("FAILED, Vendor_String.Entry mismatch");
					return false;
				}
			}
			else if (!blockCopy.Vendor_String.Entry.AsSpan().SequenceEqual(block.Vendor_String.Entry))
			{
				Console.WriteLine("FAILED, Vendor_String.Entry data mismatch");
				return false;
			}

			if (blockCopy.Num_Comments != block.Num_Comments)
			{
				Console.WriteLine("FAILED, Num_Comments mismatch, expected {0}, got {1}", block.Num_Comments, blockCopy.Num_Comments);
				return false;
			}

			for (uint32_t i = 0; i < block.Num_Comments; i++)
			{
				if (blockCopy.Comments[i].Length != block.Comments[i].Length)
				{
					Console.WriteLine("FAILED, Comments[{0}].Length mismatch, expected {1}, got {2}", i, block.Comments[i].Length, blockCopy.Comments[i].Length);
					return false;
				}

				if ((block.Comments[i].Entry == null) || (blockCopy.Comments[i].Entry == null))
				{
					if (block.Comments[i].Entry != blockCopy.Comments[i].Entry)
					{
						Console.WriteLine("FAILED, Comments[{0}].Entry mismatch", i);
						return false;
					}
				}
				else if (!blockCopy.Comments[i].Entry.AsSpan().SequenceEqual(block.Comments[i].Entry))
				{
					Console.WriteLine("FAILED, Comments[{0}].Entry data mismatch", i);
					return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__bool Compare_Block_Data_CueSheet(Flac__StreamMetadata_CueSheet block, Flac__StreamMetadata_CueSheet blockCopy)
		{
			if (!blockCopy.Media_Catalog_Number.AsSpan().SequenceEqual(block.Media_Catalog_Number))
			{
				Console.WriteLine("FAILED, Media_Catalog_Number mismatch, expected {0}, got {1}", Encoding.ASCII.GetString(block.Media_Catalog_Number, 0, block.Media_Catalog_Number.Count(b => b != 0)), Encoding.ASCII.GetString(blockCopy.Media_Catalog_Number, 0, blockCopy.Media_Catalog_Number.Count(b => b != 0)));
				return false;
			}

			if (blockCopy.Lead_In != block.Lead_In)
			{
				Console.WriteLine("FAILED, Lead_In mismatch, expected {0}, got {1}", block.Lead_In, blockCopy.Lead_In);
				return false;
			}

			if (blockCopy.Is_Cd != block.Is_Cd)
			{
				Console.WriteLine("FAILED, Is_Cd mismatch, expected {0}, got {1}", block.Is_Cd, blockCopy.Is_Cd);
				return false;
			}

			if (blockCopy.Num_Tracks != block.Num_Tracks)
			{
				Console.WriteLine("FAILED, Num_Tracks mismatch, expected {0}, got {1}", block.Num_Tracks, blockCopy.Num_Tracks);
				return false;
			}

			for (uint32_t i = 0; i < block.Num_Tracks; i++)
			{
				if (blockCopy.Tracks[i].Offset != block.Tracks[i].Offset)
				{
					Console.WriteLine("FAILED, Tracks[{0}].Offset mismatch, expected {1}, got {2}", i, block.Tracks[i].Offset, blockCopy.Tracks[i].Offset);
					return false;
				}

				if (blockCopy.Tracks[i].Number != block.Tracks[i].Number)
				{
					Console.WriteLine("FAILED, Tracks[{0}].Number mismatch, expected {1}, got {2}", i, block.Tracks[i].Number, blockCopy.Tracks[i].Number);
					return false;
				}

				if (blockCopy.Tracks[i].Num_Indices != block.Tracks[i].Num_Indices)
				{
					Console.WriteLine("FAILED, Tracks[{0}].Num_Indices mismatch, expected {1}, got {2}", i, block.Tracks[i].Num_Indices, blockCopy.Tracks[i].Num_Indices);
					return false;
				}

				// Num_Indices == 0 means lead-out track so only the track offset and number are valid
				if (block.Tracks[i].Num_Indices > 0)
				{
					if (!blockCopy.Tracks[i].Isrc.AsSpan().SequenceEqual(block.Tracks[i].Isrc))
					{
						Console.WriteLine("FAILED, Tracks[{0}].Isrc mismatch, expected {1}, got {2}", i, Encoding.ASCII.GetString(block.Tracks[i].Isrc, 0, block.Tracks[i].Isrc.Count(b => b != 0)), Encoding.ASCII.GetString(blockCopy.Tracks[i].Isrc, 0, blockCopy.Tracks[i].Isrc.Count(b => b != 0)));
						return false;
					}

					if (blockCopy.Tracks[i].Type != block.Tracks[i].Type)
					{
						Console.WriteLine("FAILED, Tracks[{0}].Type mismatch, expected {1}, got {2}", i, block.Tracks[i].Type, blockCopy.Tracks[i].Type);
						return false;
					}

					if (blockCopy.Tracks[i].Pre_Emphasis != block.Tracks[i].Pre_Emphasis)
					{
						Console.WriteLine("FAILED, Tracks[{0}].Pre_Emphasis mismatch, expected {1}, got {2}", i, block.Tracks[i].Pre_Emphasis, blockCopy.Tracks[i].Pre_Emphasis);
						return false;
					}

					if ((block.Tracks[i].Indices == null) || (blockCopy.Tracks[i].Indices == null))
					{
						if (block.Tracks[i].Indices != blockCopy.Tracks[i].Indices)
						{
							Console.WriteLine("FAILED, Tracks[{0}].Indices mismatch", i);
							return false;
						}
					}
					else
					{
						for (uint32_t j = 0; j < block.Tracks[i].Num_Indices; j++)
						{
							if (blockCopy.Tracks[i].Indices[j].Offset != block.Tracks[i].Indices[j].Offset)
							{
								Console.WriteLine("FAILED, Tracks[{0}].Indices[{1}].Offset mismatch, expected {2}, got {3}", i, j, block.Tracks[i].Indices[j].Offset, blockCopy.Tracks[i].Indices[j].Offset);
								return false;
							}

							if (blockCopy.Tracks[i].Indices[j].Number != block.Tracks[i].Indices[j].Number)
							{
								Console.WriteLine("FAILED, Tracks[{0}].Indices[{1}].Number mismatch, expected {2}, got {3}", i, j, block.Tracks[i].Indices[j].Number, blockCopy.Tracks[i].Indices[j].Number);
								return false;
							}
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
		private static Flac__bool Compare_Block_Data_Picture(Flac__StreamMetadata_Picture block, Flac__StreamMetadata_Picture blockCopy)
		{
			if (blockCopy.Type != block.Type)
			{
				Console.WriteLine("FAILED, Type mismatch, expected {0}, got {1}", block.Type, blockCopy.Type);
				return false;
			}

			int len = block.Mime_Type.Count(b => b != 0);
			int lenCopy = blockCopy.Mime_Type.Count(b => b != 0);

			if (lenCopy != len)
			{
				Console.WriteLine("FAILED, Mime_Type length mismatch, expected {0}, got {1}", len, lenCopy);
				return false;
			}

			if (!blockCopy.Mime_Type.AsSpan().SequenceEqual(block.Mime_Type))
			{
				Console.WriteLine("FAILED, Mime_Type mismatch, expected {0}, got {1}", Encoding.ASCII.GetString(block.Mime_Type, 0, len), Encoding.ASCII.GetString(blockCopy.Mime_Type, 0, lenCopy));
				return false;
			}

			len = block.Description.Count(b => b != 0);
			lenCopy = blockCopy.Description.Count(b => b != 0);

			if (lenCopy != len)
			{
				Console.WriteLine("FAILED, Description length mismatch, expected {0}, got {1}", len, lenCopy);
				return false;
			}

			if (!blockCopy.Description.AsSpan().SequenceEqual(block.Description))
			{
				Console.WriteLine("FAILED, Description mismatch, expected {0}, got {1}", Encoding.UTF8.GetString(block.Description, 0, len), Encoding.UTF8.GetString(blockCopy.Description, 0, lenCopy));
				return false;
			}

			if (blockCopy.Width != block.Width)
			{
				Console.WriteLine("FAILED, Width mismatch, expected {0}, got {1}", block.Width, blockCopy.Width);
				return false;
			}

			if (blockCopy.Height != block.Height)
			{
				Console.WriteLine("FAILED, Height mismatch, expected {0}, got {1}", block.Height, blockCopy.Height);
				return false;
			}

			if (blockCopy.Depth != block.Depth)
			{
				Console.WriteLine("FAILED, Depth mismatch, expected {0}, got {1}", block.Depth, blockCopy.Depth);
				return false;
			}

			if (blockCopy.Colors != block.Colors)
			{
				Console.WriteLine("FAILED, Colors mismatch, expected {0}, got {1}", block.Colors, blockCopy.Colors);
				return false;
			}

			if (blockCopy.Data_Length != block.Data_Length)
			{
				Console.WriteLine("FAILED, Data_Length mismatch, expected {0}, got {1}", block.Data_Length, blockCopy.Data_Length);
				return false;
			}

			if ((block.Data_Length > 0) && !blockCopy.Data.AsSpan().SequenceEqual(block.Data))
			{
				Console.WriteLine("FAILED, Data mismatch");
				return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__bool Compare_Block_Data_Unknown(Flac__StreamMetadata_Unknown block, Flac__StreamMetadata_Unknown blockCopy, uint32_t block_Length)
		{
			if ((block.Data == null) || (blockCopy.Data == null))
			{
				if (block.Data != blockCopy.Data)
				{
					Console.WriteLine("FAILED, Data mismatch ({0}'s data pointer is null", block.Data == null ? "original" : "copy");
					return false;
				}
				else if (block_Length > 0)
				{
					Console.WriteLine("FAILED, Data pointer is null but block length is not 0");
					return false;
				}
			}
			else
			{
				if (block_Length == 0)
				{
					Console.WriteLine("FAILED, Data pointer is not null but block length is 0");
					return false;
				}
				else if (!blockCopy.Data.AsSpan().SequenceEqual(block.Data))
				{
					Console.WriteLine("FAILED, data mismatch");
					return false;
				}
			}

			return true;
		}
		#endregion
	}
}
