/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Demuxer.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;
using Buffer = Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Buffer;
using Version = Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Version;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Demuxer
{
	/// <summary>
	/// ASF compatible demuxer
	/// </summary>
	internal static class AsfDec_F
	{
		private const c_int Len = 22;
		private const c_int Asf_Max_Streams = 127;
		private const c_int Frame_Header_Size = 6;

		private static readonly AvOption[] options =//XX 121
		[
			new AvOption("no_resync_search", "Don't try to resynchronize by looking for a certain optional start code", nameof(AsfContext.No_Resync_Search), AvOptionType.Bool, new AvOption.DefaultValueUnion { I64 = 0 }, 0, 1, AvOptFlag.Decoding_Param),
			new AvOption("export_xmp", "Export full XMP metadata", nameof(AsfContext.Export_Xmp), AvOptionType.Bool, new AvOption.DefaultValueUnion { I64 = 0 }, 0, 1, AvOptFlag.Decoding_Param),
			new AvOption()
		];

		private static readonly AvClass asf_Class = new AvClass//XX 127
		{
			Class_Name = "asf_demuxer".ToCharPointer(),
			Item_Name = Log.Av_Default_Item_Name,
			Option = options,
			Version = Version.Version_Int
		};

		/// <summary>
		/// 
		/// </summary>
		public static readonly FFInputFormat FF_Asf_Demuxer = new FFInputFormat//XX 1624
		{
			Name = "asf".ToCharPointer(),
			Long_Name = "ASF (Advanced / Active Streaming Format)".ToCharPointer(),
			Flags = AvFmt.NoBinSearch | AvFmt.NoGenSearch,
			Priv_Class = asf_Class,
			Priv_Data_Alloc = Alloc_Priv_Data,
			Read_Probe = Asf_Probe,
			Read_Header = Asf_Read_Header,
			Read_Packet = Asf_Read_Packet,
			Read_Close = Asf_Read_Close,
			Read_Seek = Asf_Read_Seek,
			Read_Timestamp = Asf_Read_Pts
		};

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static IPrivateData Alloc_Priv_Data()
		{
			return new AsfContext();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Print_Guid(FF_Asf_Guid g)//XX 157
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Asf_Probe(AvProbeData pd)//XX 196
		{
			// Check file header
			if (RiffDec.FF_GuidCmp(pd.Buf, Asf_Tags.FF_Asf_Header) == 0)
				return AvProbe.Score_Max;
			else
				return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Size of type 2 (BOOL) is 32bit for
		/// "Extended Content Description Object" but 16 bit for
		/// "Metadata Object" and "Metadata Library Object"
		/// </summary>
		/********************************************************************/
		private static c_int Get_Value(AvIoContext pb, AsfDataType type, c_int type2_Size)//XX 207
		{
			switch (type)
			{
				case AsfDataType.Bool:
					return (type2_Size == 32) ? (c_int)AvIoBuf.AvIo_RL32(pb) : (c_int)AvIoBuf.AvIo_RL16(pb);

				case AsfDataType.DWord:
					return (c_int)AvIoBuf.AvIo_RL32(pb);

				case AsfDataType.QWord:
					return (c_int)AvIoBuf.AvIo_RL64(pb);

				case AsfDataType.Word:
					return (c_int)AvIoBuf.AvIo_RL16(pb);

				default:
					return c_int.MinValue;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Get_Tag(AvFormatContext s, CPointer<char> key, AsfDataType type, c_int len, c_int type2_Size)//XX 223
		{
			AsfContext asf = (AsfContext)s.Priv_Data;
			CPointer<char> value = null;
			int64_t off = AvIoBuf.AvIo_Tell(s.Pb);
			const c_int Len = 22;

			if ((asf.Export_Xmp == 0) && (CString.strncmp(key, "xmp", 3) == 0))
				goto Finish;

			value = Mem.Av_MAlloc<char>((size_t)(2 * len) + Len);

			if (value.IsNull)
				goto Finish;

			switch (type)
			{
				case AsfDataType.Unicode:
				{
					AvIoBuf.AvIo_Get_Str16LE(s.Pb, len, value, (2 * len) + 1);
					break;
				}

				case AsfDataType.Ascii:
				{
					CPointer<c_uchar> val = new CPointer<c_uchar>(len);

					c_int ret = AvIoBuf.FFIo_Read_Size(s.Pb, val, len);

					if (ret < 0)
						goto Finish;

					for (c_int i = 0; i < len; i++)
						value[i] = (char)val[i];

					value[len] = '\0';
					break;
				}

				case AsfDataType.Byte_Array:
				{
					if (Asf.FF_Asf_Handle_Byte_Array(s, key, len) > 0)
						Log.Av_Log(s, Log.Av_Log_Verbose, "Unsupported byte array in tag %s.\n", key);

					goto Finish;
				}

				case AsfDataType.Bool:
				case AsfDataType.DWord:
				case AsfDataType.QWord:
				case AsfDataType.Word:
				{
					uint64_t num = (uint16_t)Get_Value(s.Pb, type, type2_Size);
					CString.snprintf(value, Len, "%lld", num);
					break;
				}

				case AsfDataType.Guid:
				{
					Log.Av_Log(s, Log.Av_Log_Debug, "Unsupported GUID value in tag %s.\n", key);
					goto Finish;
				}
			}

			if (value[0] != '\0')
				Dict.Av_Dict_Set(ref s.Metadata, key, value, AvDict.None);

			Finish:
			Mem.Av_FreeP(ref value);
			AvIoBuf.AvIo_Seek(s.Pb, off + len, AvSeek.Set);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Asf_Read_File_Properties(AvFormatContext s)//XX 277
		{
			AsfContext asf = (AsfContext)s.Priv_Data;
			AvIoContext pb = s.Pb;

			RiffDec.FF_Get_Guid(pb, out asf.Hdr.Guid);
			asf.Hdr.File_Size = AvIoBuf.AvIo_RL64(pb);
			asf.Hdr.Create_Time = AvIoBuf.AvIo_RL64(pb);
			AvIoBuf.AvIo_RL64(pb);							// Number of packets
			asf.Hdr.Play_Time = AvIoBuf.AvIo_RL64(pb);
			asf.Hdr.Send_Time = AvIoBuf.AvIo_RL64(pb);
			asf.Hdr.Preroll = AvIoBuf.AvIo_RL32(pb);
			asf.Hdr.Ignore = AvIoBuf.AvIo_RL32(pb);
			asf.Hdr.Flags = AvIoBuf.AvIo_RL32(pb);
			asf.Hdr.Min_PktSize = AvIoBuf.AvIo_RL32(pb);
			asf.Hdr.Max_PktSize = AvIoBuf.AvIo_RL32(pb);

			if (asf.Hdr.Min_PktSize >= (1U << 29))
				return Error.InvalidData;

			asf.Hdr.Max_BitRate = AvIoBuf.AvIo_RL32(pb);

			s.Packet_Size = asf.Hdr.Max_PktSize;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Asf_Read_Stream_Properties(AvFormatContext s, int64_t size)//XX 301
		{
			AsfContext asf = (AsfContext)s.Priv_Data;
			AvIoContext pb = s.Pb;

			AvMediaType type;
			bool is_Dvr_Ms_Audio = false;
			int64_t pos2;

			if (s.Nb_Streams == Asf_Max_Streams)
			{
				Log.Av_Log(s, Log.Av_Log_Error, "too many streams\n");

				return Error.EINVAL;
			}

			int64_t pos1 = AvIoBuf.AvIo_Tell(pb);

			AvStream st = Options_Format.AvFormat_New_Stream(s, null);

			if (st == null)
				return Error.ENOMEM;

			FFStream sti = Internal.FFStream(st);

			AvFormat.AvPriv_Set_Pts_Info(st, 32, 1, 1000);	// 32 bit pts in ms

			int64_t startTime = asf.Hdr.Preroll;

			if ((asf.Hdr.Flags & 0x01) == 0)	// If we aren't streaming...
			{
				int64_t fSize = AvIoBuf.AvIo_Size(pb);

				if ((fSize <= 0) || ((int64_t)asf.Hdr.File_Size <= 0) || (Common.FFAbs(fSize - (int64_t)asf.Hdr.File_Size) < (Macros.FFMin(fSize, (int64_t)asf.Hdr.File_Size) / 20)))
					st.Duration = (int64_t)(asf.Hdr.Play_Time / (10000000 / 1000)) - startTime;
			}

			RiffDec.FF_Get_Guid(pb, out FF_Asf_Guid g);

			bool test_For_Ext_Stream_Audio = false;

			if (RiffDec.FF_GuidCmp(g, Asf_Tags.FF_Asf_Audio_Stream) == 0)
				type = AvMediaType.Audio;
			else if (RiffDec.FF_GuidCmp(g, Asf_Tags.FF_Asf_Video_Stream) == 0)
				type = AvMediaType.Video;
			else if (RiffDec.FF_GuidCmp(g, Asf_Tags.FF_Asf_Jfif_Media) == 0)
			{
				type = AvMediaType.Video;
				st.CodecPar.Codec_Id = AvCodecId.MJpeg;
			}
			else if (RiffDec.FF_GuidCmp(g, Asf_Tags.FF_Asf_Command_Stream) == 0)
				type = AvMediaType.Data;
			else if (RiffDec.FF_GuidCmp(g, Asf_Tags.FF_Asf_Ext_Stream_Embed_Stream_Header) == 0)
			{
				test_For_Ext_Stream_Audio = true;
				type = AvMediaType.Unknown;
			}
			else
				return -1;

			RiffDec.FF_Get_Guid(pb, out g);

			AvIoBuf.AvIo_Skip(pb, 8);		// Total size
			c_int type_Specific_Size = (c_int)AvIoBuf.AvIo_RL32(pb);
			AvIoBuf.AvIo_RL32(pb);

			st.Id = (c_int)AvIoBuf.AvIo_RL16(pb) & 0x7f;    // Stream id

			// Mapping of asf ID to AV stream ID
			asf.AsfId2AvId[st.Id] = (c_int)s.Nb_Streams - 1;
			AsfStream asf_St = asf.Streams[st.Id];

			AvIoBuf.AvIo_RL32(pb);

			if (test_For_Ext_Stream_Audio)
			{
				RiffDec.FF_Get_Guid(pb, out g);

				if (RiffDec.FF_GuidCmp(g, Asf_Tags.FF_Asf_Ext_Stream_Audio_Stream) == 0)
				{
					type = AvMediaType.Audio;
					is_Dvr_Ms_Audio = true;

					RiffDec.FF_Get_Guid(pb, out g);
					AvIoBuf.AvIo_RL32(pb);
					AvIoBuf.AvIo_RL32(pb);
					AvIoBuf.AvIo_RL32(pb);
					RiffDec.FF_Get_Guid(pb, out g);
					AvIoBuf.AvIo_RL32(pb);
				}
			}

			st.CodecPar.Codec_Type = type;

			if (type == AvMediaType.Audio)
			{
				c_int ret = RiffDec.FF_Get_Wav_Header(s, pb, st.CodecPar, type_Specific_Size, 0);

				if (ret < 0)
					return ret;

				if (is_Dvr_Ms_Audio)
				{
					// codec_id and codec_tag are unreliable in dvr_ms
					// files. Set them later by probing stream
					sti.Request_Probe = 1;
					st.CodecPar.Codec_Tag = 0;
				}

				if (st.CodecPar.Codec_Id == AvCodecId.Aac)
					sti.Need_Parsing = AvStreamParseType.None;
				else
					sti.Need_Parsing = AvStreamParseType.Full;

				// We have to init the frame size at some point ....
				pos2 = AvIoBuf.AvIo_Tell(pb);

				if (size >= (pos2 + 8 - pos1 + 24))
				{
					asf_St.Ds_Span = AvIoBuf.AvIo_R8(pb);
					asf_St.Ds_Packet_Size = (c_int)AvIoBuf.AvIo_RL16(pb);
					asf_St.Ds_Chunk_Size = (c_int)AvIoBuf.AvIo_RL16(pb);
					AvIoBuf.AvIo_RL16(pb);	// ds_data_size
					AvIoBuf.AvIo_R8(pb);	// ds_silence_data
				}

				if (asf_St.Ds_Span > 1)
				{
					if ((asf_St.Ds_Chunk_Size == 0) || ((asf_St.Ds_Packet_Size / asf_St.Ds_Chunk_Size) <= 1) || ((asf_St.Ds_Packet_Size % asf_St.Ds_Chunk_Size) != 0))
						asf_St.Ds_Span = 0;		// Disable descrambling
				}
			}
			else if ((type == AvMediaType.Video) && ((size - (AvIoBuf.AvIo_Tell(pb) - pos1 + 24)) >= 51))
			{
				AvIoBuf.AvIo_RL32(pb);
				AvIoBuf.AvIo_RL32(pb);
				AvIoBuf.AvIo_R8(pb);
				AvIoBuf.AvIo_RL16(pb);	// size
				c_int sizeX = (c_int)AvIoBuf.AvIo_RL32(pb);	// size
				st.CodecPar.Width = (c_int)AvIoBuf.AvIo_RL32(pb);
				st.CodecPar.Height = (c_int)AvIoBuf.AvIo_RL32(pb);

				// Not available for asf
				AvIoBuf.AvIo_RL16(pb);	// panes
				st.CodecPar.Bits_Per_Coded_Sample = (c_int)AvIoBuf.AvIo_RL16(pb);	// depth
				c_uint tag1 = AvIoBuf.AvIo_RL32(pb);
				AvIoBuf.AvIo_Skip(pb, 20);

				if (sizeX > 40)
				{
					if ((size < (sizeX - 40)) || ((sizeX - 40) > (c_int.MaxValue - Defs.Av_Input_Buffer_Padding_Size)))
						return Error.InvalidData;

					size_t extraData_Size = (size_t)AvIoBuf.FFIo_Limit(pb, sizeX - 40);
					CPointer<uint8_t> extraData = Mem.Av_MAllocz<uint8_t>(extraData_Size + Defs.Av_Input_Buffer_Padding_Size);

					if (extraData.IsNull)
						return Error.ENOMEM;

					st.CodecPar.ExtraData = new DataBufferContext(extraData, extraData_Size);

					AvIoBuf.AvIo_Read(pb, extraData, (c_int)extraData_Size);
				}

				// Extract palette from extradata if bpp <= 8
				// This code assumes that extradata contains only palette
				// This is true for all paletted codecs implemented in libavcodec
				if ((st.CodecPar.ExtraData != null) && (st.CodecPar.Bits_Per_Coded_Sample <= 8))
				{
					DataBufferContext extraData = (DataBufferContext)st.CodecPar.ExtraData;
					var src = MemoryMarshal.Cast<uint8_t, uint32_t>(extraData.Data.AsSpan());

					if (!BitConverter.IsLittleEndian)
					{
						for (c_int i = 0; i < (Macros.FFMin((c_int)extraData.Size, UtilConstants.AvPalette_Size) / 4); i++)
							asf_St.Palette[i] = BSwap.Av_BSwap32(src[i]);
					}
					else
						CMemory.memcpy(asf_St.Palette, src.ToArray(), (size_t)Macros.FFMin((c_int)extraData.Size, UtilConstants.AvPalette_Size));

					asf_St.Palette_Changed = 1;
				}

				st.CodecPar.Codec_Tag = tag1;
				st.CodecPar.Codec_Id = Utils_Format.FF_Codec_Get_Id(Riff.FF_Codec_Bmp_Tags, tag1);

				if (st.CodecPar.Codec_Id == 0)
					st.CodecPar.Codec_Id = Utils_Format.FF_Codec_Get_Id(Riff.FF_Codec_Bmp_Tags_Unofficial, tag1);

				if (tag1 == Macros.MkTag('D', 'V', 'R', ' '))
				{
					sti.Need_Parsing = AvStreamParseType.Full;

					// issue658 contains wrong w/h and MS even puts a fake seq header
					// with wrong w/h in extradata while a correct one is in the stream.
					// maximum lameness
					st.CodecPar.Width = st.CodecPar.Height = 0;

					Mem.Av_FreeP(ref st.CodecPar.ExtraData);
				}

				if (st.CodecPar.Codec_Id == AvCodecId.H264)
					sti.Need_Parsing = AvStreamParseType.Full_Once;

				if (st.CodecPar.Codec_Id == AvCodecId.Mpeg4)
					sti.Need_Parsing = AvStreamParseType.Full;

				if (st.CodecPar.Codec_Id == AvCodecId.Hevc)
					sti.Need_Parsing = AvStreamParseType.Full;
			}

			pos2 = AvIoBuf.AvIo_Tell(pb);
			AvIoBuf.AvIo_Skip(pb, size - (pos2 - pos1 + 24));

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Asf_Read_Ext_Stream_Properties(AvFormatContext s)//XX 476
		{
			AsfContext asf = (AsfContext)s.Priv_Data;
			AvIoContext pb = s.Pb;

			AvIoBuf.AvIo_RL64(pb);		// starttime
			AvIoBuf.AvIo_RL64(pb);		// endtime
			uint32_t leak_Rate = AvIoBuf.AvIo_RL32(pb);	// leak-datarate
			AvIoBuf.AvIo_RL32(pb);		// bucket-datasize
			AvIoBuf.AvIo_RL32(pb);		// init-bucket-fullness
			AvIoBuf.AvIo_RL32(pb);		// alt-leak-datarate
			AvIoBuf.AvIo_RL32(pb);		// alt-bucket-datasize
			AvIoBuf.AvIo_RL32(pb);		// alt-init-bucket-fullness
			AvIoBuf.AvIo_RL32(pb);		// max-object-size
			AvIoBuf.AvIo_RL32(pb);		// flags (reliable,seekable,no_cleanpoints?,resend-live-cleanpoints, rest of bits reserved)
			uint32_t stream_Num = AvIoBuf.AvIo_RL16(pb);	// stream-num

			c_uint stream_LanguageId_Index = AvIoBuf.AvIo_RL16(pb);	// stream-language-id-index

			if (stream_Num < 128)
				asf.Streams[stream_Num].Stream_Language_Index = (uint16_t)stream_LanguageId_Index;

			AvIoBuf.AvIo_RL64(pb);		// avg frametime in 100ns units
			c_int stream_Ct = (c_int)AvIoBuf.AvIo_RL16(pb);			// stream-name-count;
			c_int payload_Ext_Ct = (c_int)AvIoBuf.AvIo_RL16(pb);	// payload-extension-system-count

			if (stream_Num < 128)
			{
				asf.Stream_BitRates[stream_Num] = leak_Rate;
				asf.Streams[stream_Num].Payload_Ext_Ct = 0;
			}

			for (c_int i = 0; i < stream_Ct; i++)
			{
				AvIoBuf.AvIo_RL16(pb);
				c_int ext_Len = (c_int)AvIoBuf.AvIo_RL16(pb);
				AvIoBuf.AvIo_Skip(pb, ext_Len);
			}

			for (c_int i = 0; i < payload_Ext_Ct; i++)
			{
				RiffDec.FF_Get_Guid(pb, out FF_Asf_Guid g);
				c_int size = (c_int)AvIoBuf.AvIo_RL16(pb);
				c_int ext_Len = (c_int)AvIoBuf.AvIo_RL32(pb);

				if (ext_Len < 0)
					return Error.InvalidData;

				AvIoBuf.AvIo_Skip(pb, ext_Len);

				if ((stream_Num < 128) && (i < (c_int)Macros.FF_Array_Elems(asf.Streams[stream_Num].Payload)))
				{
					AsfPayload p = asf.Streams[stream_Num].Payload[i];

					p.Type = g.Data[0];
					p.Size = (uint16_t)size;

					Log.Av_Log(s, Log.Av_Log_Debug, "Payload extension %x %d\n", g.Data[0], p.Size);

					asf.Streams[stream_Num].Payload_Ext_Ct++;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Asf_Read_Content_Desc(AvFormatContext s)//XX 536
		{
			AvIoContext pb = s.Pb;

			c_int len1 = (c_int)AvIoBuf.AvIo_RL16(pb);
			c_int len2 = (c_int)AvIoBuf.AvIo_RL16(pb);
			c_int len3 = (c_int)AvIoBuf.AvIo_RL16(pb);
			c_int len4 = (c_int)AvIoBuf.AvIo_RL16(pb);
			c_int len5 = (c_int)AvIoBuf.AvIo_RL16(pb);

			Get_Tag(s, "title".ToCharPointer(), AsfDataType.Unicode, len1, 32);
			Get_Tag(s, "author".ToCharPointer(), AsfDataType.Unicode, len2, 32);
			Get_Tag(s, "copyright".ToCharPointer(), AsfDataType.Unicode, len3, 32);
			Get_Tag(s, "comment".ToCharPointer(), AsfDataType.Unicode, len4, 32);
			AvIoBuf.AvIo_Skip(pb, len5);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Asf_Read_Ext_Content_Desc(AvFormatContext s)//XX 555
		{
			AvIoContext pb = s.Pb;
			AsfContext asf = (AsfContext)s.Priv_Data;

			c_int desc_Count = (c_int)AvIoBuf.AvIo_RL16(pb);

			for (c_int i = 0; i < desc_Count; i++)
			{
				CPointer<char> name = new CPointer<char>(1024);

				c_int name_Len = (c_int)AvIoBuf.AvIo_RL16(pb);

				if ((name_Len % 2) != 0)	// Must be even, broken lavf versions wrote len-1
					name_Len += 1;

				c_int ret = AvIoBuf.AvIo_Get_Str16LE(pb, name_Len, name, name.Length);

				if (ret < name_Len)
					AvIoBuf.AvIo_Skip(pb, name_Len - ret);

				AsfDataType value_Type = (AsfDataType)AvIoBuf.AvIo_RL16(pb);
				c_int value_Len = (c_int)AvIoBuf.AvIo_RL16(pb);

				if ((value_Type == AsfDataType.Unicode) && ((value_Len % 2) != 0))
					value_Len += 1;

				// My sample has that stream set to 0 maybe that mean the container.
				// ASF stream count starts at 1. I am using 0 to the container value
				// since it's unused
				if (CString.strcmp(name, "AspectRatioX") == 0)
					asf.Dar[0].Num = Get_Value(s.Pb, value_Type, 32);
				else if (CString.strcmp(name, "AspectRatioY") == 0)
					asf.Dar[0].Den = Get_Value(s.Pb, value_Type, 32);
				else
					Get_Tag(s, name, value_Type, value_Len, 32);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Asf_Read_Language_List(AvFormatContext s)//XX 589
		{
			AvIoContext pb = s.Pb;
			AsfContext asf = (AsfContext)s.Priv_Data;

			c_int stream_Count = (c_int)AvIoBuf.AvIo_RL16(pb);

			for (c_int j = 0; j < stream_Count; j++)
			{
				CPointer<char> lang = new CPointer<char>(6);

				c_uint lang_Len = (c_uint)AvIoBuf.AvIo_R8(pb);

				c_int ret = AvIoBuf.AvIo_Get_Str16LE(pb, (c_int)lang_Len, lang, lang.Length);

				if (ret < lang_Len)
					AvIoBuf.AvIo_Skip(pb, lang_Len - ret);

				if (j < 128)
					AvString.Av_Strlcpy(asf.Stream_Languages[j], lang, (size_t)asf.Stream_Languages[0].Length);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Asf_Read_Metadata(AvFormatContext s)//XX 609
		{
			AvIoContext pb = s.Pb;
			AsfContext asf = (AsfContext)s.Priv_Data;

			c_int n = (c_int)AvIoBuf.AvIo_RL16(pb);

			for (c_int i = 0; i < n; i++)
			{
				AvIoBuf.AvIo_RL16(pb);		// lang_list_index
				c_int stream_Num = (c_int)AvIoBuf.AvIo_RL16(pb);
				c_int name_Len_Utf16 = (c_int)AvIoBuf.AvIo_RL16(pb);
				AsfDataType value_Type = (AsfDataType)AvIoBuf.AvIo_RL16(pb);	// value_type
				c_uint value_Len = AvIoBuf.AvIo_RL32(pb);

				if (value_Len >= ((c_int.MaxValue - Len) / 2))
					return Error.InvalidData;

				c_int name_Len_Utf8 = (2 * name_Len_Utf16) + 1;
				CPointer<char> name = Mem.Av_MAlloc<char>((size_t)name_Len_Utf8);

				if (name.IsNull)
					return Error.ENOMEM;

				c_int ret = AvIoBuf.AvIo_Get_Str16LE(pb, name_Len_Utf16, name, name_Len_Utf8);

				if (ret < name_Len_Utf16)
					AvIoBuf.AvIo_Skip(pb, name_Len_Utf16 - ret);

				Log.Av_Log(s, Log.Av_Log_Trace, "%d stream %d name_len %2d type %d len %4d <%s>\n", i, stream_Num, name_Len_Utf16, value_Type, value_Len, name);

				if ((CString.strcmp(name, "AspectRatioX") == 0))
				{
					c_int aspect_X = Get_Value(s.Pb, value_Type, 16);

					if (stream_Num < 128)
						asf.Dar[stream_Num].Num = aspect_X;
				}
				else if ((CString.strcmp(name, "AspectRatioY") == 0))
				{
					c_int aspect_Y = Get_Value(s.Pb, value_Type, 16);

					if (stream_Num < 128)
						asf.Dar[stream_Num].Den = aspect_Y;
				}
				else
					Get_Tag(s, name, value_Type, (c_int)value_Len, 16);

				Mem.Av_FreeP(ref name);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Asf_Read_Marker(AvFormatContext s)//XX 658
		{
			AvIoContext pb = s.Pb;
			AsfContext asf = (AsfContext)s.Priv_Data;
			CPointer<char> name = new CPointer<char>(1024);

			AvIoBuf.AvIo_RL64(pb);							// Reserved 16 bytes
			AvIoBuf.AvIo_RL64(pb);							// ...
			c_int count = (c_int)AvIoBuf.AvIo_RL32(pb);		// Markers count
			AvIoBuf.AvIo_RL16(pb);							// Reserved 2 bytes
			c_int name_Len = (c_int)AvIoBuf.AvIo_RL16(pb);	// Name length
			AvIoBuf.AvIo_Skip(pb, name_Len);

			for (c_int i = 0; i < count; i++)
			{
				if (AvIoBuf.AvIo_FEof(pb) != 0)
					return Error.InvalidData;

				AvIoBuf.AvIo_RL64(pb);								// Offset, 8 bytes
				int64_t pres_Time = (int64_t)AvIoBuf.AvIo_RL64(pb);	// Presentation time
				pres_Time = Common.Av_Sat_Sub64(pres_Time, asf.Hdr.Preroll * 10000L);
				AvIoBuf.AvIo_RL16(pb);								// Entry length
				AvIoBuf.AvIo_RL32(pb);								// Send time
				AvIoBuf.AvIo_RL32(pb);								// Flags
				name_Len = (c_int)AvIoBuf.AvIo_RL32(pb);			// Name length

				if ((c_uint)name_Len > (c_int.MaxValue / 2))
					return Error.InvalidData;

				c_int ret = AvIoBuf.AvIo_Get_Str16LE(pb, name_Len * 2, name, name.Length);

				if (ret < name_Len)
					AvIoBuf.AvIo_Skip(pb, name_Len - ret);

				Demux_Utils.AvPriv_New_Chapter(s, i, new AvRational(1, 10000000), pres_Time, UtilConstants.Av_NoPts_Value, name);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Asf_Read_Header(AvFormatContext s)//XX 698
		{
			AsfContext asf = (AsfContext)s.Priv_Data;
			AvIoContext pb = s.Pb;

			RiffDec.FF_Get_Guid(pb, out FF_Asf_Guid g);

			if (RiffDec.FF_GuidCmp(g, Asf_Tags.FF_Asf_Header) != 0)
				return Error.InvalidData;

			AvIoBuf.AvIo_RL64(pb);
			AvIoBuf.AvIo_RL32(pb);
			AvIoBuf.AvIo_R8(pb);
			AvIoBuf.AvIo_R8(pb);

			CMemory.memset(asf.AsfId2AvId, -1, (size_t)asf.AsfId2AvId.Length);

			for (c_int i = 0; i < 128; i++)
				asf.Streams[i].Stream_Language_Index = 128;		// Invalid stream index means no language info

			for (;;)
			{
				uint64_t gPos = (uint64_t)AvIoBuf.AvIo_Tell(pb);
				c_int ret = 0;

				RiffDec.FF_Get_Guid(pb, out g);
				int64_t gSize = (int64_t)AvIoBuf.AvIo_RL64(pb);
				Print_Guid(g);

				if (RiffDec.FF_GuidCmp(g, Asf_Tags.FF_Asf_Data_Header) == 0)
				{
					asf.Data_Object_Offset = (uint64_t)AvIoBuf.AvIo_Tell(pb);

					// If not streaming, gsize is not unlimited (how?),
					// and there is enough space in the file
					if (((asf.Hdr.Flags & 0x01) == 0) && (gSize >= 100))
						asf.Data_Object_Size = (uint64_t)gSize - 24;
					else
						asf.Data_Object_Size = uint64_t.MaxValue;

					break;
				}

				if (gSize < 24)
					return Error.InvalidData;

				if (RiffDec.FF_GuidCmp(g, Asf_Tags.FF_Asf_File_Header) == 0)
					ret = Asf_Read_File_Properties(s);
				else if (RiffDec.FF_GuidCmp(g, Asf_Tags.FF_Asf_Stream_Header) == 0)
					ret = Asf_Read_Stream_Properties(s, gSize);
				else if (RiffDec.FF_GuidCmp(g, Asf_Tags.FF_Asf_Comment_Header) == 0)
					Asf_Read_Content_Desc(s);
				else if (RiffDec.FF_GuidCmp(g, Asf_Tags.FF_Asf_Language_Guid) == 0)
					Asf_Read_Language_List(s);
				else if (RiffDec.FF_GuidCmp(g, Asf_Tags.FF_Asf_Extended_Content_Header) == 0)
					Asf_Read_Ext_Content_Desc(s);
				else if (RiffDec.FF_GuidCmp(g, Asf_Tags.FF_Asf_Metadata_Header) == 0)
					Asf_Read_Metadata(s);
				else if (RiffDec.FF_GuidCmp(g, Asf_Tags.FF_Asf_Metadata_Library_Header) == 0)
					Asf_Read_Metadata(s);
				else if (RiffDec.FF_GuidCmp(g, Asf_Tags.FF_Asf_Ext_Stream_Header) == 0)
				{
					Asf_Read_Ext_Stream_Properties(s);

					// There could be an optional stream properties object to follow
					// if so the next iteration will pick it up
					continue;
				}
				else if (RiffDec.FF_GuidCmp(g, Asf_Tags.FF_Asf_Head1_Guid) == 0)
				{
					RiffDec.FF_Get_Guid(pb, out g);
					AvIoBuf.AvIo_Skip(pb, 6);
					continue;
				}
				else if (RiffDec.FF_GuidCmp(g, Asf_Tags.FF_Asf_Marker_Header) == 0)
					Asf_Read_Marker(s);
				else if (AvIoBuf.AvIo_FEof(pb) != 0)
					return Error.EOF;
				else
				{
					if (s.Key.Len == 0)
					{
						if (RiffDec.FF_GuidCmp(g, Asf_Tags.FF_Asf_Content_Encryption) == 0)
						{
							AvPacket pkt = Internal.FFFormatContext(s).Parse_Pkt;

							Log.Av_Log(s, Log.Av_Log_Warning, "DRM protected stream detected, decoding will likely fail!\n");

							c_uint len = AvIoBuf.AvIo_RL32(pb);
							Log.Av_Log(s, Log.Av_Log_Debug, "Secret data:\n");

							ret = Utils_Format.Av_Get_Packet(pb, pkt, (c_int)len);

							if (ret < 0)
								return ret;

							Dump.Av_Hex_Dump_Log(s, Log.Av_Log_Debug, pkt.Data, pkt.Size);
							Packet.Av_Packet_Unref(pkt);

							len = AvIoBuf.AvIo_RL32(pb);

							if (len > uint16_t.MaxValue)
								return Error.InvalidData;

							Get_Tag(s, "ASF_Protection_Type".ToCharPointer(), AsfDataType.Ascii, (c_int)len, 32);

							len = AvIoBuf.AvIo_RL32(pb);

							if (len > uint16_t.MaxValue)
								return Error.InvalidData;

							Get_Tag(s, "ASF_Key_ID".ToCharPointer(), AsfDataType.Ascii, (c_int)len, 32);

							len = AvIoBuf.AvIo_RL32(pb);

							if (len > uint16_t.MaxValue)
								return Error.InvalidData;

							Get_Tag(s, "ASF_License_URL".ToCharPointer(), AsfDataType.Ascii, (c_int)len, 32);
						}
						else if (RiffDec.FF_GuidCmp(g, Asf_Tags.FF_Asf_Ext_Content_Encryption) == 0)
						{
							Log.Av_Log(s, Log.Av_Log_Warning, "Ext DRM protected stream detected, decoding will likely fail!\n");

							Dict.Av_Dict_Set(ref s.Metadata, "encryption", "ASF Extended Content Encryption", AvDict.None);
						}
						else if (RiffDec.FF_GuidCmp(g, Asf_Tags.FF_Asf_Digital_Signature) == 0)
							Log.Av_Log(s, Log.Av_Log_Info, "Digital signature detected!\n");
					}
				}

				if (ret < 0)
					return ret;

				if ((uint64_t)AvIoBuf.AvIo_Tell(pb) != (gPos + (uint64_t)gSize))
					Log.Av_Log(s, Log.Av_Log_Debug, "gpos mismatch our pos=%lld, end=%lld\n", (uint64_t)AvIoBuf.AvIo_Tell(pb) - gPos, gSize);

				AvIoBuf.AvIo_Seek(pb, (int64_t)gPos + gSize, AvSeek.Set);
			}

			RiffDec.FF_Get_Guid(pb, out g);
			AvIoBuf.AvIo_RL64(pb);
			AvIoBuf.AvIo_R8(pb);
			AvIoBuf.AvIo_R8(pb);

			if (AvIoBuf.AvIo_FEof(pb) != 0)
				return Error.EOF;

			asf.Data_Offset = (uint64_t)AvIoBuf.AvIo_Tell(pb);
			asf.Packet_Size_Left = 0;

			for (c_int i = 0; i < 128; i++)
			{
				c_int stream_Num = asf.AsfId2AvId[i];

				if (stream_Num >= 0)
				{
					AvStream st = s.Streams[stream_Num];

					if (st.CodecPar.Bit_Rate == 0)
						st.CodecPar.Bit_Rate = asf.Stream_BitRates[i];

					if ((asf.Dar[i].Num > 0) && (asf.Dar[i].Den > 0))
						Rational.Av_Reduce(out st.Sample_Aspects_Ratio.Num, out st.Sample_Aspects_Ratio.Den, asf.Dar[i].Num, asf.Dar[i].Den, c_int.MaxValue);
					else if ((asf.Dar[0].Num > 0) && (asf.Dar[0].Den > 0) &&
					         // Use ASF container value if the stream doesn't set AR
					         (st.CodecPar.Codec_Type == AvMediaType.Video))
					{
						Rational.Av_Reduce(out st.Sample_Aspects_Ratio.Num, out st.Sample_Aspects_Ratio.Den, asf.Dar[0].Num, asf.Dar[0].Den, c_int.MaxValue);
					}

					Log.Av_Log(s, Log.Av_Log_Trace, "i=%d, st->codecpar->codec_type:%d, asf->dar %d:%d sar=%d:%d\n", i, st.CodecPar.Codec_Type, asf.Dar[i].Num, asf.Dar[i].Den, st.Sample_Aspects_Ratio.Num, st.Sample_Aspects_Ratio.Den);

					// Copy and convert language codes to the frontend
					if (asf.Streams[i].Stream_Language_Index < 128)
					{
						CPointer<char> rfc1766 = asf.Stream_Languages[asf.Streams[i].Stream_Language_Index];

						if (rfc1766.IsNotNull && (CString.strlen(rfc1766) > 1))
						{
							char[] primary_Tag = [ rfc1766[0], rfc1766[1], '\0' ];

							// Ignore country code if any
							CPointer<char> iso6392 = AvLanguage.FF_Convert_Lang_To(primary_Tag, AvLangCodespace.Iso639_2_Bibl);

							if (iso6392.IsNotNull)
								Dict.Av_Dict_Set(ref st.Metadata, "language", iso6392, AvDict.None);
						}
					}
				}
			}

			Metadata.FF_Metadata_Conv(ref s.Metadata, null, Asf.FF_Asf_Metadata_Conv);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Do_2Bits<T>(AvIoContext pb, ref c_int rSize, c_int bits, out T var, T defVal) where T : INumber<T> //XX 860
		{
			switch (bits & 3)
			{
				case 3:
				{
					var = T.CreateTruncating(AvIoBuf.AvIo_RL32(pb));
					rSize += 4;
					break;
				}

				case 2:
				{
					var = T.CreateTruncating(AvIoBuf.AvIo_RL16(pb));
					rSize += 2;
					break;
				}

				case 1:
				{
					var = T.CreateTruncating(AvIoBuf.AvIo_R8(pb));
					rSize++;
					break;
				}

				default:
				{
					var = defVal;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Load a single ASF packet into the demuxer
		/// </summary>
		/********************************************************************/
		private static c_int Asf_Get_Packet(AvFormatContext s, AvIoContext pb)//XX 885
		{
			AsfContext asf = (AsfContext)s.Priv_Data;
			uint32_t packet_Length, padSize;
			c_int rSize = 8;
			c_int c, d = 0, e = 0;

			if (asf.Uses_Std_Ecc > 0)
			{
				// If we do not know packet size, allow skipping up to 32 kB
				c_int off = 32768;

				if (asf.No_Resync_Search != 0)
					off = 3;

				c = d = e = -1;

				while (off-- > 0)
				{
					c = d;
					d = e;
					e = AvIoBuf.AvIo_R8(pb);

					if ((c == 0x82) && (d == 0) && (e == 0))
						break;
				}

				if (c != 0x82)
				{
					// This code allows handling of -EAGAIN at packet boundaries (i.e.
					// if the packet sync code above triggers -EAGAIN). This does not
					// imply complete -EAGAIN handling support at random positions in
					// the stream
					if (pb.Error == Error.EAGAIN)
						return Error.EAGAIN;

					if (AvIoBuf.AvIo_FEof(pb) == 0)
						Log.Av_Log(s, Log.Av_Log_Error, "ff asf bad header %x  at:%lld\n", c, AvIoBuf.AvIo_Tell(pb));
				}

				if ((c & 0x8f) == 0x82)
				{
					if ((d != 0) || (e != 0))
					{
						if (AvIoBuf.AvIo_FEof(pb) == 0)
							Log.Av_Log(s, Log.Av_Log_Error, "ff asf bad non zero\n");

						return Error.InvalidData;
					}

					c = AvIoBuf.AvIo_R8(pb);
					d = AvIoBuf.AvIo_R8(pb);

					rSize += 3;
				}
				else if (AvIoBuf.AvIo_FEof(pb) == 0)
					AvIoBuf.AvIo_Seek(pb, -1, AvSeek.Cur);
			}
			else
			{
				c = AvIoBuf.AvIo_R8(pb);

				if ((c & 0x80) != 0)
				{
					rSize++;

					if ((c & 0x60) == 0)
					{
						d = AvIoBuf.AvIo_R8(pb);
						e = AvIoBuf.AvIo_R8(pb);

						AvIoBuf.AvIo_Seek(pb, (c & 0xf) - 2, AvSeek.Cur);

						rSize += c & 0xf;
					}

					if (c != 0x82)
						Log.AvPriv_Request_Sample(s, "Invalid ECC byte");

					if (asf.Uses_Std_Ecc == 0)
						asf.Uses_Std_Ecc = (c == 0x82) && (d == 0) && (e == 0) ? 1 : -1;

					c = AvIoBuf.AvIo_R8(pb);
				}
				else
					asf.Uses_Std_Ecc = -1;

				d = AvIoBuf.AvIo_R8(pb);
			}

			asf.Packet_Flags = c;
			asf.Packet_Property = d;

			Do_2Bits(pb, ref rSize, asf.Packet_Flags >> 5, out packet_Length, s.Packet_Size);
			Do_2Bits(pb, ref rSize, asf.Packet_Flags >> 1, out padSize, 0U);	// Sequence ignored
			Do_2Bits(pb, ref rSize, asf.Packet_Flags >> 3, out padSize, 0U);	// Padding length

			// The following checks prevent overflows and infinite loops
			if ((packet_Length == 0) || (packet_Length >= (1U << 29)))
			{
				Log.Av_Log(s, Log.Av_Log_Error, "invalid packet length %lld at:%lld\n", packet_Length, AvIoBuf.AvIo_Tell(pb));

				return Error.InvalidData;
			}

			if (padSize >= packet_Length)
			{
				Log.Av_Log(s, Log.Av_Log_Error, "invalid padsize %lld at:%lld\n", padSize, AvIoBuf.AvIo_Tell(pb));

				return Error.InvalidData;
			}

			asf.Packet_Timestamp = (c_int)AvIoBuf.AvIo_RL32(pb);
			AvIoBuf.AvIo_RL16(pb);	// Duration

			// rsize has at least 11 bytes which have to be present

			if ((asf.Packet_Flags & 0x01) != 0)
			{
				asf.Packet_SegSizeType = AvIoBuf.AvIo_R8(pb);
				rSize++;

				asf.Packet_Segments = asf.Packet_SegSizeType & 0x3f;
			}
			else
			{
				asf.Packet_Segments = 1;
				asf.Packet_SegSizeType = 0x80;
			}

			if (rSize > (packet_Length - padSize))
			{
				asf.Packet_Size_Left = 0;

				Log.Av_Log(s, Log.Av_Log_Error, "invalid packet header length %d for pktlen %lld-%lld at %lld\n", rSize, packet_Length, padSize, AvIoBuf.AvIo_Tell(pb));

				return Error.InvalidData;
			}

			asf.Packet_Size_Left = (c_int)(packet_Length - padSize - rSize);

			if (packet_Length < asf.Hdr.Min_PktSize)
				padSize += asf.Hdr.Min_PktSize - packet_Length;

			asf.Packet_PadSize = (c_int)padSize;

			Log.Av_Log(s, Log.Av_Log_Trace, "packet: size=%d padsize=%d  left=%d\n", s.Packet_Size, asf.Packet_PadSize, asf.Packet_Size_Left);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Asf_Read_Frame_Header(AvFormatContext s, AvIoContext pb)//XX 1007
		{
			AsfContext asf = (AsfContext)s.Priv_Data;
			c_int rSize = 1;
			c_int num = AvIoBuf.AvIo_R8(pb);

			asf.Packet_Segments--;
			asf.Packet_Key_Frame = num >> 7;
			asf.Stream_Index = asf.AsfId2AvId[num & 0x7f];
			AsfStream asfSt = asf.Streams[num & 0x7f];

			// Sequence should be ignored!
			Do_2Bits(pb, ref rSize, asf.Packet_Property >> 4, out asf.Packet_Seq, 0);
			Do_2Bits(pb, ref rSize, asf.Packet_Property >> 2, out asf.Packet_Frag_Offset, 0U);
			Do_2Bits(pb, ref rSize, asf.Packet_Property, out asf.Packet_Replic_Size, 0);

			Log.Av_Log(asf, Log.Av_Log_Trace, "key:%d stream:%d seq:%d offset:%d replic_size:%d num:%X packet_property %X\n", asf.Packet_Key_Frame, asf.Stream_Index, asf.Packet_Seq, asf.Packet_Frag_Offset, asf.Packet_Replic_Size, num, asf.Packet_Property);

			if ((rSize + (int64_t)asf.Packet_Replic_Size) > asf.Packet_Size_Left)
			{
				Log.Av_Log(s, Log.Av_Log_Error, "packet_replic_size %d is invalid\n", asf.Packet_Replic_Size);

				return Error.InvalidData;
			}

			if (asf.Packet_Replic_Size >= 8)
			{
				int64_t end = AvIoBuf.AvIo_Tell(pb) + asf.Packet_Replic_Size;
				asfSt.Packet_Obj_Size = (c_int)AvIoBuf.AvIo_RL32(pb);

				AvRational aspect = new AvRational();

				if ((asfSt.Packet_Obj_Size >= (1 << 24)) || (asfSt.Packet_Obj_Size < 0))
				{
					Log.Av_Log(s, Log.Av_Log_Error, "packet_obj_size %d invalid\n", asfSt.Packet_Obj_Size);

					asfSt.Packet_Obj_Size = 0;

					return Error.InvalidData;
				}

				asf.Packet_Frag_Timestamp = AvIoBuf.AvIo_RL32(pb);	// timestamp

				for (c_int i = 0; i < asfSt.Payload_Ext_Ct; i++)
				{
					AsfPayload p = asfSt.Payload[i];
					c_int size = p.Size;

					if (size == 0xffff)
						size = (c_int)AvIoBuf.AvIo_RL16(pb);

					int64_t payEnd = AvIoBuf.AvIo_Tell(pb) + size;

					if (payEnd > end)
					{
						Log.Av_Log(s, Log.Av_Log_Error, "too long payload\n");
						break;
					}

					switch (p.Type)
					{
						case 0x50:
							break;

						case 0x54:
						{
							aspect.Num = AvIoBuf.AvIo_R8(pb);
							aspect.Den = AvIoBuf.AvIo_R8(pb);

							if ((aspect.Num > 0) && (aspect.Den > 0) && (asf.Stream_Index >= 0))
								s.Streams[asf.Stream_Index].Sample_Aspects_Ratio = aspect;

							break;
						}

						case 0x2a:
						{
							AvIoBuf.AvIo_Skip(pb, 8);

							int64_t ts0 = (int64_t)AvIoBuf.AvIo_RL64(pb);
							int64_t ts1 = (int64_t)AvIoBuf.AvIo_RL64(pb);

							if (ts0 != -1)
								asf.Packet_Frag_Timestamp = ts0 / 10000;
							else
								asf.Packet_Frag_Timestamp = UtilConstants.Av_NoPts_Value;

							asf.Ts_Is_Pts = 1;
							break;
						}

						case 0x5b:
						case 0xb7:
						case 0xcc:
						case 0xc0:
						case 0xa0:
						{
							// Unknown
							break;
						}
					}

					AvIoBuf.AvIo_Seek(pb, payEnd, AvSeek.Set);
				}

				AvIoBuf.AvIo_Seek(pb, end, AvSeek.Set);
				rSize += asf.Packet_Replic_Size;	// FIXME - check validity
			}
			else if (asf.Packet_Replic_Size == 1)
			{
				// Multipacket - frag_offset is beginning timestamp
				asf.Packet_Time_Start = asf.Packet_Frag_Offset;
				asf.Packet_Frag_Offset = 0;
				asf.Packet_Frag_Timestamp = asf.Packet_Timestamp;

				asf.Packet_Time_Delta = AvIoBuf.AvIo_R8(pb);
				rSize++;
			}
			else if (asf.Packet_Replic_Size != 0)
			{
				Log.Av_Log(s, Log.Av_Log_Error, "unexpected packet_replic_size of %d\n", asf.Packet_Replic_Size);

				return Error.InvalidData;
			}

			if ((asf.Packet_Flags & 0x01) != 0)
			{
				Do_2Bits(pb, ref rSize, asf.Packet_SegSizeType >> 6, out asf.Packet_Frag_Size, 0U);	// 0 is illegal

				if (rSize > asf.Packet_Size_Left)
				{
					Log.Av_Log(s, Log.Av_Log_Error, "packet_replic_size is invalid\n");

					return Error.InvalidData;
				}
				else if (asf.Packet_Frag_Size > (asf.Packet_Size_Left - rSize))
				{
					if (asf.Packet_Frag_Size > (asf.Packet_Size_Left - rSize + asf.Packet_PadSize))
					{
						Log.Av_Log(s, Log.Av_Log_Error, "packet_frag_size is invalid (%d>%d-%d+%d)\n", asf.Packet_Frag_Size, asf.Packet_Size_Left, rSize, asf.Packet_PadSize);

						return Error.InvalidData;
					}
					else
					{
						c_int diff = (c_int)(asf.Packet_Frag_Size - (asf.Packet_Size_Left - rSize));

						asf.Packet_Size_Left += diff;
						asf.Packet_PadSize -= diff;
					}
				}
			}
			else
				asf.Packet_Frag_Size = (c_uint)(asf.Packet_Size_Left - rSize);

			if (asf.Packet_Replic_Size == 1)
			{
				asf.Packet_Multi_Size = (c_int)asf.Packet_Frag_Size;

				if (asf.Packet_Multi_Size > asf.Packet_Size_Left)
					return Error.InvalidData;
			}

			asf.Packet_Size_Left -= rSize;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Parse data from individual ASF packets (which were previously
		/// loaded with asf_get_packet())
		/// </summary>
		/********************************************************************/
		private static c_int Asf_Parse_Packet(AvFormatContext s, AvIoContext pb, AvPacket pkt)//XX 1136
		{
			AsfContext asf = (AsfContext)s.Priv_Data;
			AsfStream asf_St = null;

			for (;;)
			{
				c_int ret;

				if (AvIoBuf.AvIo_FEof(pb) != 0)
					return Error.EOF;

				if ((asf.Packet_Size_Left < Frame_Header_Size) || (asf.Packet_Segments < 1) && (asf.Packet_Time_Start == 0))
				{
					ret = asf.Packet_Size_Left + asf.Packet_PadSize;

					if ((asf.Packet_Size_Left != 0) && (asf.Packet_Size_Left < Frame_Header_Size))
						Log.Av_Log(s, Log.Av_Log_Warning, "Skip due to FRAME_HEADER_SIZE\n");

					// Fail safe
					AvIoBuf.AvIo_Skip(pb, ret);

					asf.Packet_Pos = AvIoBuf.AvIo_Tell(pb);

					if ((asf.Data_Object_Size != uint64_t.MaxValue) && ((((uint64_t)asf.Packet_Pos - asf.Data_Object_Offset) >= asf.Data_Object_Size)))
						return Error.EOF;

					return 1;
				}

				if (asf.Packet_Time_Start == 0)
				{
					if (Asf_Read_Frame_Header(s, pb) < 0)
					{
						asf.Packet_Time_Start = asf.Packet_Segments = 0;
						continue;
					}

					if ((asf.Stream_Index < 0) || (s.Streams[asf.Stream_Index].Discard >= AvDiscard.All) || ((asf.Packet_Key_Frame == 0) && ((s.Streams[asf.Stream_Index].Discard >= AvDiscard.NonKey) || (asf.Streams[s.Streams[asf.Stream_Index].Id].Skip_To_Key != 0))))
					{
						asf.Packet_Time_Start = 0;

						// Unhandled packet (should not happend)
						AvIoBuf.AvIo_Skip(pb, asf.Packet_Frag_Size);

						asf.Packet_Size_Left -= (c_int)asf.Packet_Frag_Size;

						if (asf.Stream_Index < 0)
							Log.Av_Log(s, Log.Av_Log_Error, "ff asf skip %d (unknown stream)\n", asf.Packet_Frag_Size);

						continue;
					}

					asf.Asf_St = asf.Streams[s.Streams[asf.Stream_Index].Id];

					if (asf.Packet_Frag_Offset == 0)
						asf.Asf_St.Skip_To_Key = 0;
				}

				asf_St = asf.Asf_St;

				if ((asf_St.Frag_Offset == 0) && (asf.Packet_Frag_Offset != 0))
				{
					Log.Av_Log(s, Log.Av_Log_Trace, "skipping asf data pkt with fragment offset for stream:%d, expected:%d but got %d from pkt)\n", asf.Stream_Index, asf_St.Frag_Offset, asf.Packet_Frag_Offset);

					AvIoBuf.AvIo_Skip(pb, asf.Packet_Frag_Size);

					asf.Packet_Size_Left -= (c_int)asf.Packet_Frag_Size;
					continue;
				}

				if (asf.Packet_Replic_Size == 1)
				{
					// frag_offset is here used as the beginning timestamp
					asf.Packet_Frag_Timestamp = asf.Packet_Time_Start;
					asf.Packet_Time_Start += asf.Packet_Time_Delta;

					asf_St.Packet_Obj_Size = AvIoBuf.AvIo_R8(pb);
					asf.Packet_Frag_Size = (c_uint)asf_St.Packet_Obj_Size;

					asf.Packet_Size_Left--;
					asf.Packet_Multi_Size--;

					if (asf.Packet_Multi_Size < asf_St.Packet_Obj_Size)
					{
						asf.Packet_Time_Start = 0;

						AvIoBuf.AvIo_Skip(pb, asf.Packet_Multi_Size);

						asf.Packet_Size_Left -= asf.Packet_Multi_Size;
						continue;
					}

					asf.Packet_Multi_Size -= asf_St.Packet_Obj_Size;
				}

				if ((asf_St.Pkt.Size != asf_St.Packet_Obj_Size) || ((asf_St.Frag_Offset + asf.Packet_Frag_Size) > asf_St.Pkt.Size))
				{
					if (asf_St.Pkt.Data.IsNotNull)
					{
						Log.Av_Log(s, Log.Av_Log_Info, "freeing incomplete packet size %d, new %d\n", asf_St.Pkt.Size, asf_St.Packet_Obj_Size);

						asf_St.Frag_Offset = 0;

						Packet.Av_Packet_Unref(asf_St.Pkt);
					}

					// New packet
					ret = Packet.Av_New_Packet(asf_St.Pkt, asf_St.Packet_Obj_Size);

					if (ret < 0)
						return ret;

					asf_St.Seq = (c_uchar)asf.Packet_Seq;

					if (asf.Packet_Frag_Timestamp != UtilConstants.Av_NoPts_Value)
					{
						if (asf.Ts_Is_Pts != 0)
							asf_St.Pkt.Pts = asf.Packet_Frag_Timestamp - asf.Hdr.Preroll;
						else
							asf_St.Pkt.Dts = asf.Packet_Frag_Timestamp - asf.Hdr.Preroll;
					}

					asf_St.Pkt.Stream_Index = asf.Stream_Index;
					asf_St.Pkt.Pos = asf_St.Packet_Pos = asf.Packet_Pos;
					asf_St.Pkt_Clean = 0;

					if (asf_St.Pkt.Data.IsNotNull && (asf_St.Palette_Changed != 0))
					{
						DataBufferContext pal = Packet.Av_Packet_New_Side_Data(asf_St.Pkt, AvPacketSideDataType.Palette, UtilConstants.AvPalette_Size);

						if (pal == null)
							Log.Av_Log(s, Log.Av_Log_Error, "Cannot append palette to packet\n");
						else
						{
							CMemory.memcpy(pal.Data, MemoryMarshal.Cast<uint32_t, uint8_t>(asf_St.Palette.AsSpan()), UtilConstants.AvPalette_Size);
							asf_St.Palette_Changed = 0;
						}
					}

					Log.Av_Log(asf, Log.Av_Log_Trace, "new packet: stream:%d key:%d packet_key:%d audio:%d size:%d\n", asf.Stream_Index, asf.Packet_Key_Frame, asf_St.Pkt.Flags & AvPktFlag.Key, s.Streams[asf.Stream_Index].CodecPar.Codec_Type == AvMediaType.Audio ? 1 : 0, asf_St.Packet_Obj_Size);

					if (s.Streams[asf.Stream_Index].CodecPar.Codec_Type == AvMediaType.Audio)
						asf.Packet_Key_Frame = 1;

					if (asf.Packet_Key_Frame != 0)
						asf_St.Pkt.Flags |= AvPktFlag.Key;
				}

				// Read data
				Log.Av_Log(asf, Log.Av_Log_Trace, "READ PACKET s:%d  os:%d  o:%d,%d  l:%d   DATA:%p\n", s.Packet_Size, asf_St.Pkt.Size, asf.Packet_Frag_Offset, asf_St.Frag_Offset, asf.Packet_Frag_Size, asf_St.Pkt.Data);

				asf.Packet_Size_Left -= (c_int)asf.Packet_Frag_Size;

				if (asf.Packet_Size_Left < 0)
					continue;

				if ((asf.Packet_Frag_Offset >= asf_St.Pkt.Size) || (asf.Packet_Frag_Size > (asf_St.Pkt.Size - asf.Packet_Frag_Offset)))
				{
					Log.Av_Log(s, Log.Av_Log_Error, "packet fragment position invalid %u,%u not in %u\n", asf.Packet_Frag_Offset, asf.Packet_Frag_Size, asf_St.Pkt.Size);

					continue;
				}

				if ((asf.Packet_Frag_Offset != asf_St.Frag_Offset) && (asf_St.Pkt_Clean == 0))
				{
					CMemory.memset<uint8_t>(asf_St.Pkt.Data + asf_St.Frag_Offset, 0, (size_t)(asf_St.Pkt.Size - asf_St.Frag_Offset));

					asf_St.Pkt_Clean = 1;
				}

				ret = AvIoBuf.AvIo_Read(pb, asf_St.Pkt.Data + asf.Packet_Frag_Offset, (c_int)asf.Packet_Frag_Size);

				if (ret != asf.Packet_Frag_Size)
				{
					if ((ret < 0) || ((asf.Packet_Frag_Size + ret) == 0))
						return ret < 0 ? ret : Error.EOF;

					if (asf_St.Ds_Span > 1)
					{
						// Scrambling, we can either drop it completely or fill the remainder
						// TODO: should we fill the whole packet instead of just the current
						// fragment?
						CMemory.memset<uint8_t>(asf_St.Pkt.Data + asf.Packet_Frag_Offset + ret, 0, (size_t)(asf.Packet_Frag_Size - ret));

						ret = (c_int)asf.Packet_Frag_Size;
					}
					else
					{
						// No scrambling, so we can return partial packets
						Packet.Av_Shrink_Packet(asf_St.Pkt, (c_int)asf.Packet_Frag_Offset + ret);
					}
				}

				if (s.Key.Ptr.IsNotNull && (s.Key.Len == 20))
					AsfCrypt.FF_AsfCrypt_Dec(s.Key.Ptr, asf_St.Pkt.Data + asf.Packet_Frag_Offset, ret);

				asf_St.Frag_Offset += ret;

				// Test if whole packet is read
				if (asf_St.Frag_Offset == asf_St.Pkt.Size)
				{
					// Workaround for macroshit radio DVR-MS files
					if ((s.Streams[asf.Stream_Index].CodecPar.Codec_Id == AvCodecId.Mpeg2Video) && (asf_St.Pkt.Size > 100))
					{
						c_int i;

						for (i = 0; (i < asf_St.Pkt.Size) && (asf_St.Pkt.Data[i] == 0); i++)
						{
						}

						if (i == asf_St.Pkt.Size)
						{
							Log.Av_Log(s, Log.Av_Log_Debug, "discarding ms fart\n");

							asf_St.Frag_Offset = 0;

							Packet.Av_Packet_Unref(asf_St.Pkt);

							continue;
						}
					}

					// Return packet
					if (asf_St.Ds_Span > 1)
					{
						if (asf_St.Pkt.Size != (asf_St.Ds_Packet_Size * asf_St.Ds_Span))
							Log.Av_Log(s, Log.Av_Log_Error, "pkt.size != ds_packet_size * ds_span (%d %d %d)\n", asf_St.Pkt.Size, asf_St.Ds_Packet_Size, asf_St.Ds_Span);
						else
						{
							// Packet descrambling
							AvBufferRef buf = Buffer.Av_Buffer_Alloc((size_t)asf_St.Pkt.Size + Defs.Av_Input_Buffer_Padding_Size);

							if (buf != null)
							{
								CPointer<uint8_t> newData = ((DataBufferContext)buf.Data).Data;
								c_int offset = 0;

								CMemory.memset<uint8_t>(newData + asf_St.Pkt.Size, 0, Defs.Av_Input_Buffer_Padding_Size);

								while (offset < asf_St.Pkt.Size)
								{
									c_int off = offset / asf_St.Ds_Chunk_Size;
									c_int row = off / asf_St.Ds_Span;
									c_int col = off % asf_St.Ds_Span;
									c_int idx = row + ((col * asf_St.Ds_Packet_Size) / asf_St.Ds_Chunk_Size);

									CMemory.memcpy(newData + offset, asf_St.Pkt.Data + idx * asf_St.Ds_Chunk_Size, (size_t)asf_St.Ds_Chunk_Size);

									offset += asf_St.Ds_Chunk_Size;
								}

								Buffer.Av_Buffer_Unref(ref asf_St.Pkt.Buf);

								asf_St.Pkt.Buf = buf;
								asf_St.Pkt.Data = ((DataBufferContext)buf.Data).Data;
							}
						}
					}

					asf_St.Frag_Offset = 0;

					asf_St.Pkt.CopyTo(pkt);

					asf_St.Pkt.Buf = null;
					asf_St.Pkt.Size = 0;
					asf_St.Pkt.Data.SetToNull();
					asf_St.Pkt.Side_Data_Elems = 0;
					asf_St.Pkt.Side_Data.SetToNull();
					break;	// Packet completed
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Asf_Read_Packet(AvFormatContext s, AvPacket pkt)//XX 1367
		{
			AsfContext asf = (AsfContext)s.Priv_Data;

			for (;;)
			{
				// Parse cached packets, if any
				c_int ret = Asf_Parse_Packet(s, s.Pb, pkt);

				if (ret <= 0)
					return ret;

				Asf_Get_Packet(s, s.Pb);

				asf.Packet_Time_Start = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Added to support seeking after packets have been read.
		/// If information is not reset, read_packet fails due to leftover
		/// information from previous reads
		/// </summary>
		/********************************************************************/
		private static void Asf_Reset_Header(AvFormatContext s)//XX 1387
		{
			AsfContext asf = (AsfContext)s.Priv_Data;

			asf.Packet_Size_Left = 0;
			asf.Packet_Flags = 0;
			asf.Packet_Property = 0;
			asf.Packet_Frag_Timestamp = 0;
			asf.Packet_SegSizeType = 0;
			asf.Packet_Segments = 0;
			asf.Packet_Seq = 0;
			asf.Packet_Replic_Size = 0;
			asf.Packet_Key_Frame = 0;
			asf.Packet_PadSize = 0;
			asf.Packet_Frag_Offset = 0;
			asf.Packet_Frag_Size = 0;
			asf.Packet_Frag_Timestamp = 0;
			asf.Packet_Multi_Size = 0;
			asf.Packet_Time_Delta = 0;
			asf.Packet_Time_Start = 0;

			for (c_int i = 0; i < 128; i++)
			{
				AsfStream asf_St = asf.Streams[i];

				Packet.Av_Packet_Unref(asf_St.Pkt);

				asf_St.Packet_Obj_Size = 0;
				asf_St.Frag_Offset = 0;
				asf_St.Seq = 0;
			}

			asf.Asf_St = null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Skip_To_Key(AvFormatContext s)//XX 1420
		{
			AsfContext asf = (AsfContext)s.Priv_Data;

			for (c_int i = 0; i < 128; i++)
			{
				c_int j = asf.AsfId2AvId[i];
				AsfStream asf_St = asf.Streams[i];

				if ((j < 0) || (s.Streams[j].CodecPar.Codec_Type != AvMediaType.Video))
					continue;

				asf_St.Skip_To_Key = 1;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Asf_Read_Close(AvFormatContext s)//XX 1435
		{
			Asf_Reset_Header(s);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int64_t Asf_Read_Pts(AvFormatContext s, c_int stream_Index, ref int64_t pPos, int64_t pos_Limit)//XX 1442
		{
			FFFormatContext si = Internal.FFFormatContext(s);
			AsfContext asf = (AsfContext)s.Priv_Data;
			AvPacket pkt1 = new AvPacket();
			AvPacket pkt = pkt1;
			int64_t pts;
			int64_t pos = pPos;
			int64_t[] start_Pos = new int64_t[Asf_Max_Streams];

			for (c_int i = 0; i < s.Nb_Streams; i++)
				start_Pos[i] = pos;

			if (s.Packet_Size > 0)
				pos = ((pos + s.Packet_Size - 1 - si.Data_Offset) / s.Packet_Size * s.Packet_Size) + si.Data_Offset;

			pPos = pos;

			if (AvIoBuf.AvIo_Seek(s.Pb, pos, AvSeek.Set) < 0)
				return UtilConstants.Av_NoPts_Value;

			Seek.FF_Read_Frame_Flush(s);
			Asf_Reset_Header(s);

			for (;;)
			{
				if (Demux.Av_Read_Frame(s, pkt) < 0)
				{
					Log.Av_Log(s, Log.Av_Log_Info, "asf_read_pts failed\n");

					return UtilConstants.Av_NoPts_Value;
				}

				pts = pkt.Dts;

				if ((pkt.Flags & AvPktFlag.Key) != 0)
				{
					c_int i = pkt.Stream_Index;

					AsfStream asf_St = asf.Streams[s.Streams[i].Id];

					pos = asf_St.Packet_Pos;

					Seek.Av_Add_Index_Entry(s.Streams[i], pos, pts, pkt.Size, (c_int)(pos - start_Pos[i] + 1), AvIndex.KeyFrame);
					start_Pos[i] = asf_St.Packet_Pos + 1;

					if (pkt.Stream_Index == stream_Index)
					{
						Packet.Av_Packet_Unref(pkt);
						break;
					}
				}

				Packet.Av_Packet_Unref(pkt);
			}

			pPos = pos;

			return pts;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Asf_Build_Simple_Index(AvFormatContext s, c_int stream_Index)//XX 1500
		{
			AsfContext asf = (AsfContext)s.Priv_Data;
			int64_t current_Pos = AvIoBuf.AvIo_Tell(s.Pb);

			int64_t ret = AvIoBuf.AvIo_Seek(s.Pb, (int64_t)(asf.Data_Object_Offset + asf.Data_Object_Size), AvSeek.Set);

			if (ret < 0)
				return (c_int)ret;

			ret = RiffDec.FF_Get_Guid(s.Pb, out FF_Asf_Guid g);

			if (ret < 0)
				goto End;

			// The data object can be followed by other top-level objects,
			// skip them until the simple index object is reached
			while (RiffDec.FF_GuidCmp(g, Asf_Tags.FF_Asf_Simple_Index_Header) != 0)
			{
				int64_t gSize = (int64_t)AvIoBuf.AvIo_RL64(s.Pb);

				if ((gSize < 24) || (AvIoBuf.AvIo_FEof(s.Pb) != 0))
					goto End;

				AvIoBuf.AvIo_Skip(s.Pb, gSize - 24);

				ret = RiffDec.FF_Get_Guid(s.Pb, out g);

				if (ret < 0)
					goto End;
			}

			{
				int64_t last_Pos = -1;
				int64_t gSize = (int64_t)AvIoBuf.AvIo_RL64(s.Pb);

				ret = RiffDec.FF_Get_Guid(s.Pb, out g);

				if (ret < 0)
					goto End;

				int64_t iTime = (int64_t)AvIoBuf.AvIo_RL64(s.Pb);
				c_int pct = (c_int)AvIoBuf.AvIo_RL32(s.Pb);
				c_int ict = (c_int)AvIoBuf.AvIo_RL32(s.Pb);

				Log.Av_Log(s, Log.Av_Log_Debug, "itime:0x%llx, pct:%d, ict:%d\n", iTime, pct, ict);

				for (c_int i = 0; i < ict; i++)
				{
					c_int pktNum = (c_int)AvIoBuf.AvIo_RL32(s.Pb);
					c_int pktCt = (c_int)AvIoBuf.AvIo_RL16(s.Pb);

					int64_t pos = Internal.FFFormatContext(s).Data_Offset + (s.Packet_Size * pktNum);
					int64_t index_Pts = Macros.FFMax(Mathematics.Av_Rescale(iTime, i, 10000) - asf.Hdr.Preroll, 0);

					if (AvIoBuf.AvIo_FEof(s.Pb) != 0)
					{
						ret = Error.InvalidData;

						goto End;
					}

					if (pos != last_Pos)
					{
						Log.Av_Log(s, Log.Av_Log_Debug, "pktnum:%d, pktct:%d  pts: %lld\n", pktNum, pktCt, index_Pts);

						Seek.Av_Add_Index_Entry(s.Streams[stream_Index], pos, index_Pts, (c_int)s.Packet_Size, 0, AvIndex.KeyFrame);

						last_Pos = pos;
					}
				}

				asf.Index_Read = ict > 1 ? 1 : 0;
			}

			End:
			AvIoBuf.AvIo_Seek(s.Pb, current_Pos, AvSeek.Set);

			return (c_int)ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Asf_Read_Seek(AvFormatContext s, c_int stream_Index, int64_t pts, AvSeekFlag flags)//XX 1568
		{
			AsfContext asf = (AsfContext)s.Priv_Data;
			AvStream st = s.Streams[stream_Index];
			FFStream sti = Internal.FFStream(st);
			c_int ret = 0;

			if (s.Packet_Size <= 0)
				return -1;

			// Try using the protocol's read_seek if available
			if (s.Pb != null)
			{
				int64_t ret1 = AvIoBuf.AvIo_Seek_Time(s.Pb, stream_Index, pts, flags);

				if (ret1 >= 0)
					Asf_Reset_Header(s);

				if (ret1 != Error.ENOSYS)
					return (c_int)ret1;
			}

			// Explicitly handle the case of seeking to 0
			if (pts == 0)
			{
				Asf_Reset_Header(s);
				AvIoBuf.AvIo_Seek(s.Pb, Internal.FFFormatContext(s).Data_Offset, AvSeek.Set);

				return 0;
			}

			if (asf.Index_Read == 0)
			{
				ret = Asf_Build_Simple_Index(s, stream_Index);

				if (ret < 0)
					asf.Index_Read = -1;
			}

			if ((asf.Index_Read > 0) && sti.Index_Entries.IsNotNull)
			{
				c_int index = Seek.Av_Index_Search_Timestamp(st, pts, flags);

				if (index >= 0)
				{
					// Find the position
					uint64_t pos = (uint64_t)sti.Index_Entries[index].Pos;

					// Do the seek
					Log.Av_Log(s, Log.Av_Log_Debug, "SEEKTO: %lld\n", pos);

					if (AvIoBuf.AvIo_Seek(s.Pb, (int64_t)pos, AvSeek.Set) < 0)
						return -1;

					Asf_Reset_Header(s);
					Skip_To_Key(s);

					return 0;
				}
			}

			// No index or seeking by index failed
			if (Seek.FF_Seek_Frame_Binary(s, stream_Index, pts, flags) < 0)
				return -1;

			Asf_Reset_Header(s);
			Skip_To_Key(s);

			return 0;
		}
		#endregion
	}
}
