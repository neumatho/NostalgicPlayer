/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Interfaces;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Buffer = Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Buffer;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat
{
	/// <summary>
	/// ID3v2 header parser
	///
	/// Specifications available at:
	/// http://id3.org/Developer_Information
	/// </summary>
	public static class Id3v2
	{
		/// <summary>
		/// 
		/// </summary>
		public const c_int Header_Size = 10;

		/// <summary>
		/// Default magic bytes for ID3v2 header: "ID3"
		/// </summary>
		public static CPointer<char> Default_Magic = "ID3".ToCharPointer();

		/// <summary>
		/// 
		/// </summary>
		public const string Priv_Metadata_Prefix = "id3v2_priv.";

		private static readonly Id3v2EmFunc[] id3v2_Extra_Meta_Funcs =
		[
			new Id3v2EmFunc("GEO", "GEOB", Read_GeobTag, Free_GeobTag),
			new Id3v2EmFunc("PIC", "APIC", Read_APic, Free_APic),
			new Id3v2EmFunc("CHAP", "CHAP", Read_Chapter, Free_Chapter),
			new Id3v2EmFunc("PRIV", "PRIV", Read_Priv, Free_Priv)
		];

		private static readonly AvMetadataConv[] ff_Id3v2_34_Metadata_Conv =
		[
			new AvMetadataConv("TALB", "album"),
			new AvMetadataConv("TCOM", "composer"),
			new AvMetadataConv("TCON", "genre"),
			new AvMetadataConv("TCOP", "copyright"),
			new AvMetadataConv("TENC", "encoded_by"),
			new AvMetadataConv("TIT2", "title"),
			new AvMetadataConv("TLAN", "language"),
			new AvMetadataConv("TPE1", "artist"),
			new AvMetadataConv("TPE2", "album_artist"),
			new AvMetadataConv("TPE3", "performer"),
			new AvMetadataConv("TPOS", "disc"),
			new AvMetadataConv("TPUB", "publisher"),
			new AvMetadataConv("TRCK", "track"),
			new AvMetadataConv("TSSE", "encoder"),
			new AvMetadataConv("USLT", "lyrics")
		];

		private static readonly AvMetadataConv[] ff_Id3v2_4_Metadata_Conv =
		[
			new AvMetadataConv("TCMP", "compilation"),
			new AvMetadataConv("TDRC", "date"),
			new AvMetadataConv("TDRL", "date"),
			new AvMetadataConv("TDEN", "creation_time"),
			new AvMetadataConv("TSOA", "album-sort"),
			new AvMetadataConv("TSOP", "artist-sort"),
			new AvMetadataConv("TSOT", "title-sort"),
			new AvMetadataConv("TIT1", "grouping")
		];

		private static readonly AvMetadataConv[] ff_Id3v2_2_Metadata_Conv =
		[
			new AvMetadataConv("TAL", "album"),
			new AvMetadataConv("TCO", "genre"),
			new AvMetadataConv("TCP", "compilation"),
			new AvMetadataConv("TT2", "title"),
			new AvMetadataConv("TEN", "encoded_by"),
			new AvMetadataConv("TP1", "artist"),
			new AvMetadataConv("TP2", "album_artist"),
			new AvMetadataConv("TP3", "performer"),
			new AvMetadataConv("TRK", "track")
		];

		internal static readonly string[] ff_Id3v2_Picture_Types =
		[
			"Other",
			"32x32 pixels 'file icon'",
			"Other file icon",
			"Cover (front)",
			"Cover (back)",
			"Leaflet page",
			"Media (e.g. label side of CD)",
			"Lead artist/lead performer/soloist",
			"Artist/performer",
			"Conductor",
			"Band/Orchestra",
			"Composer",
			"Lyricist/text writer",
			"Recording Location",
			"During recording",
			"During performance",
			"Movie/video screen capture",
			"A bright coloured fish",
			"Illustration",
			"Band/artist logotype",
			"Publisher/Studio logotype"
		];

		internal static readonly CodecMime[] ff_Id3v2_Mime_Tags =
		[
			new CodecMime("image/gif", AvCodecId.Gif),
			new CodecMime("image/jpeg", AvCodecId.MJpeg),
			new CodecMime("image/jpg", AvCodecId.MJpeg),
			new CodecMime("image/png", AvCodecId.Png),
			new CodecMime("image/tiff", AvCodecId.Tiff),
			new CodecMime("image/bmp", AvCodecId.Bmp),
			new CodecMime("image/webp", AvCodecId.Webp),
			new CodecMime("JPG", AvCodecId.MJpeg),			// ID3v2.2
			new CodecMime("PNG", AvCodecId.Png),			// ID3v2.2
		];

		/********************************************************************/
		/// <summary>
		/// Detect ID3v2 header
		/// </summary>
		/********************************************************************/
		public static c_int FF_Id3v2_Match(CPointer<uint8_t> buf, CPointer<char> magic)
		{
			return
				(buf[0] == magic[0]) &&
				(buf[1] == magic[1]) &&
				(buf[2] == magic[2]) &&
				(buf[3] != 0xff) &&
				(buf[4] != 0xff) &&
				((buf[6] & 0x80) == 0) &&
				((buf[7] & 0x80) == 0) &&
				((buf[8] & 0x80) == 0) &&
				((buf[9] & 0x80) == 0) ? 1 : 0;
		}



		/********************************************************************/
		/// <summary>
		/// Get the length of an ID3v2 tag
		/// </summary>
		/********************************************************************/
		public static c_int FF_Id3v2_Tag_Len(CPointer<uint8_t> buf)//XX 159
		{
			c_int len =
				((buf[6] & 0x7f) << 21) +
				((buf[7] & 0x7f) << 14) +
				((buf[8] & 0x7f) << 7) +
				(buf[9] & 0x7f) +
				Header_Size;

			if ((buf[5] & 0x10) != 0)
				len += Header_Size;

			return len;
		}



		/********************************************************************/
		/// <summary>
		/// Read an ID3v2 tag into specified dictionary and retrieve
		/// supported extra metadata
		/// </summary>
		/********************************************************************/
		public static void FF_Id3v2_Read_Dict(AvIoContext pb, ref AvDictionary metadata, CPointer<char> magic, out Id3v2ExtraMeta extra_Meta)//XX 1136
		{
			Id3v2_Read_Internal(pb, ref metadata, null, magic, out extra_Meta, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Read an ID3v2 tag, including supported extra metadata.
		///
		/// Data is read from and stored to AVFormatContext
		/// </summary>
		/********************************************************************/
		public static void FF_Id3v2_Read(AvFormatContext s, CPointer<char> magic, out Id3v2ExtraMeta extra_Meta, c_uint max_Search_Size)//XX 1142
		{
			Id3v2_Read_Internal(s.Pb, ref s.Metadata, s, magic, out extra_Meta, max_Search_Size);
		}



		/********************************************************************/
		/// <summary>
		/// Free memory allocated parsing special (non-text) metadata
		/// </summary>
		/********************************************************************/
		public static void FF_Id3v2_Free_Extra_Meta(ref Id3v2ExtraMeta extra_Meta)//XX 1148
		{
			Id3v2ExtraMeta current = extra_Meta;

			while (current != null)
			{
				Id3v2EmFunc extra_Func = Get_Extra_Meta_Func(current.Tag, true);

				if (extra_Func != null)
					extra_Func.Free(current.Data.Data);

				Id3v2ExtraMeta next = current.Next;
				Mem.Av_FreeP(ref current);
				current = next;
			}

			extra_Meta = null;
		}



		/********************************************************************/
		/// <summary>
		/// Create a stream for each APIC (attached picture) extracted from
		/// the ID3v2 header
		/// </summary>
		/********************************************************************/
		public static c_int FF_Id3v2_Parse_Apic(AvFormatContext s, Id3v2ExtraMeta extra_Meta)//XX 1164
		{
			for (Id3v2ExtraMeta cur = extra_Meta; cur != null; cur = cur.Next)
			{
				if (CString.strcmp(cur.Tag, "APIC") != 0)
					continue;

				Id3v2ExtraMetaAPIC apic = cur.Data.APic;

				c_int ret = Demux_Utils.FF_Add_Attached_Pic(s, null, null, ref apic.Buf, 0);

				if (ret < 0)
					return ret;

				AvStream st = s.Streams[s.Nb_Streams - 1];
				st.CodecPar.Codec_Id = apic.Id;

				if (IntReadWrite.Av_RB64(st.Attached_Pic.Data) == Png.PngSig)
					st.CodecPar.Codec_Id = AvCodecId.Png;

				if (apic.Description[0] != '\0')
					Dict.Av_Dict_Set(ref st.Metadata, "title", ConvertToString(apic.Description), AvDict.None);

				Dict.Av_Dict_Set(ref st.Metadata, "comment", apic.Type, AvDict.None);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Create chapters for all CHAP tags found in the ID3v2 header
		/// </summary>
		/********************************************************************/
		public static c_int FF_Id3v2_Parse_Chapters(AvFormatContext s, Id3v2ExtraMeta cur)//XX 1195
		{
			AvRational time_Base = new AvRational(1, 1000);

			for (c_uint i = 0; cur != null; cur = cur.Next)
			{
				if (CString.strcmp(cur.Tag, "CHAP") != 0)
					continue;

				Id3v2ExtraMetaCHAP chap = cur.Data.Chap;

				AvChapter chapter = Demux_Utils.AvPriv_New_Chapter(s, i++, time_Base, chap.Start, chap.End, ConvertToString(chap.Element_Id));

				if (chapter == null)
					continue;

				c_int ret = Dict.Av_Dict_Copy(ref chapter.Metadata, chap.Meta, AvDict.None);

				if (ret < 0)
					return ret;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Parse PRIV tags into a dictionary. The PRIV owner is the metadata
		/// key. The PRIV data is the value, with non-printable characters
		/// escaped
		/// </summary>
		/********************************************************************/
		public static c_int FF_Id3v2_Parse_Priv_Dict(ref AvDictionary metadata, Id3v2ExtraMeta extra_Meta)//XX 1220
		{
			AvDict dict_Flags = AvDict.Dont_Overwrite | AvDict.Dont_Strdup_Key | AvDict.Dont_Strdup_Val;

			for (Id3v2ExtraMeta cur = extra_Meta; cur != null; cur = cur.Next)
			{
				if (CString.strcmp(cur.Tag, "PRIV") == 0)
				{
					Id3v2ExtraMetaPRIV priv = cur.Data.Priv;

					CPointer<char> key = AvString.Av_Asprintf(Priv_Metadata_Prefix + "%s", Encoding.UTF8.GetString(priv.Owner.AsSpan()));

					if (key.IsNull)
						return Error.ENOMEM;

					BPrint.Av_BPrint_Init(out AVBPrint bprint, priv.DataSize + 1, BPrint.Av_BPrint_Size_Unlimited);

					for (c_int i = 0; i < priv.DataSize; i++)
					{
						if ((priv.Data[i] < 32) || (priv.Data[i] > 126) || (priv.Data[i] == '\\'))
							BPrint.Av_BPrintf(bprint, "\\x%02x", priv.Data[i]);
						else
							BPrint.Av_BPrint_Chars(bprint, (char)priv.Data[i], 1);
					}

					c_int ret = BPrint.Av_BPrint_Finalize(bprint, out CPointer<char> escaped);

					if (ret < 0)
					{
						Mem.Av_Free(key);

						return ret;
					}

					ret = Dict.Av_Dict_Set(ref metadata, key, escaped, dict_Flags);

					if (ret < 0)
						return ret;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Add metadata for all PRIV tags in the ID3v2 header. The PRIV
		/// owner is the metadata key. The PRIV data is the value,
		/// with non-printable characters escaped
		/// </summary>
		/********************************************************************/
		public static c_int FF_Id3v2_Parse_Priv(AvFormatContext s, Id3v2ExtraMeta extra_Meta)//XX 1260
		{
			return FF_Id3v2_Parse_Priv_Dict(ref s.Metadata, extra_Meta);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static CPointer<char> ConvertToString(CPointer<uint8_t> utf8)
		{
			return Encoding.UTF8.GetString(utf8.AsSpan()).ToCharPointer();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_uint Get_Size(AvIoContext s, c_int len)//XX 171
		{
			c_int v = 0;

			while (len-- != 0)
				v = (v << 7) + (AvIoBuf.AvIo_R8(s) & 0x7f);

			return (c_uint)v;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_uint Size_To_SyncSafe(c_uint size)//XX 179
		{
			return ((size & (0x7f << 0)) >> 0) +
				   ((size & (0x7f << 8)) >> 1) +
				   ((size & (0x7f << 16)) >> 2) +
				   ((size & (0x7f << 24)) >> 3);
		}



		/********************************************************************/
		/// <summary>
		/// No real verification, only check that the tag consists of a
		/// combination of capital alpha-numerical characters
		/// </summary>
		/********************************************************************/
		private static bool Is_Tag(CPointer<char> buf, c_uint len)//XX 189
		{
			if (len == 0)
				return false;

			while (len-- != 0)
			{
				if (((buf[len] < 'A') || (buf[len] > 'Z')) && ((buf[len] < '0') || (buf[len] > '9')))
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return 1 if the tag of length len at the given offset is valid,
		/// 0 if not, -1 on error
		/// </summary>
		/********************************************************************/
		private static c_int Check_Tag(AvIoContext s, c_int offset, c_uint len)//XX 207
		{
			CPointer<char> tag = new CPointer<char>(4);

			if ((len > 4) || (AvIoBuf.AvIo_Seek(s, offset, AvSeek.Set) < 0) || (AvIoBuf.AvIo_Read(s, tag, (c_int)len) < (c_int)len))
				return -1;
			else if ((IntReadWrite.Av_RB32(tag) == 0) || Is_Tag(tag, len))
				return 1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Free GEOB type extra metadata
		/// </summary>
		/********************************************************************/
		private static void Free_GeobTag(IExtraMetadata obj)//XX 224
		{
			Id3v2ExtraMetaGEOB geob = (Id3v2ExtraMetaGEOB)obj;

			Mem.Av_FreeP(ref geob.Mime_Type);
			Mem.Av_FreeP(ref geob.File_Name);
			Mem.Av_FreeP(ref geob.Description);
			Mem.Av_FreeP(ref geob.Data);
		}



		/********************************************************************/
		/// <summary>
		/// Decode characters to UTF-8 according to encoding type. The
		/// decoded buffer is always null terminated. Stop reading when
		/// either maxread bytes are read from pb or U+0000 character is
		/// found
		/// </summary>
		/********************************************************************/
		private static c_int Decode_Str(AvFormatContext s, AvIoContext pb, Id3v2Encoding encoding, out CPointer<uint8_t> dst, ref c_int maxRead)//XX 245
		{
			dst = null;

			uint32_t ch = 1;
			c_int left = maxRead;
			FormatFunc.Id3v2_Get_Delegate get = AvIoBuf.AvIo_RB16;

			c_int ret = AvIoBuf.AvIo_Open_Dyn_Buf(out AvIoContext dynBuf);

			if (ret < 0)
			{
				Log.Av_Log(s, Log.Av_Log_Error, "Error opening memory stream\n");
				return ret;
			}

			switch (encoding)
			{
				case Id3v2Encoding.Iso8859:
				{
					while ((left != 0) && (ch != 0))
					{
						ch = (uint32_t)AvIoBuf.AvIo_R8(pb);
						Common.Put_Utf8(ch, (x => AvIoBuf.AvIo_W8(dynBuf, x)));
						left--;
					}

					break;
				}

				case Id3v2Encoding.Utf16Bom:
				{
					left -= 2;

					if (left < 0)
					{
						Log.Av_Log(s, Log.Av_Log_Error, "Cannot read BOM value, input too short\n");
						AvIoBuf.FFIo_Free_Dyn_Buf(ref dynBuf);
						dst = null;

						return Error.InvalidData;
					}

					uint16_t bom = (uint16_t)AvIoBuf.AvIo_RB16(pb);

					switch (bom)
					{
						case 0xfffe:
						{
							get = AvIoBuf.AvIo_RL16;
							break;
						}

						case 0xfeff:
							break;

						default:
						{
							Log.Av_Log(s, Log.Av_Log_Error, "Incorrect BOM value: 0x%x\n", bom);
							AvIoBuf.FFIo_Free_Dyn_Buf(ref dynBuf);
							dst = null;
							maxRead = left;

							return Error.InvalidData;
						}
					}

					goto case Id3v2Encoding.Utf16Be;
				}

				case Id3v2Encoding.Utf16Be:
				{
					while ((left > 1) && (ch != 0))
					{
						int l1 = left;
						ch = Common.Get_Utf16(() => (l1 -= 2) >= 0 ? (uint16_t)get(pb) : (uint16_t)0, out bool error);
						left = l1;

						if (error)
							break;

						Common.Put_Utf8(ch, x => AvIoBuf.AvIo_W8(dynBuf, x));
					}

					if (left < 0)
						left += 2;		// Did not read last char from pb

					break;
				}

				case Id3v2Encoding.Utf8:
				{
					while ((left != 0) && (ch != 0))
					{
						ch = (uint32_t)AvIoBuf.AvIo_R8(pb);
						AvIoBuf.AvIo_W8(dynBuf, (c_int)ch);
						left--;
					}

					break;
				}

				default:
				{
					Log.Av_Log(s, Log.Av_Log_Warning, "Unknown encoding %d\n", encoding);
					break;
				}
			}

			if (ch != 0)
				AvIoBuf.AvIo_W8(dynBuf, 0);

			c_int dynSize = AvIoBuf.AvIo_Close_Dyn_Buf(dynBuf, out dst);

			if (dynSize <= 0)
			{
				Mem.Av_FreeP(ref dst);

				return Error.ENOMEM;
			}

			maxRead = left;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Parse a text tag
		/// </summary>
		/********************************************************************/
		private static void Read_TTag(AvFormatContext s, AvIoContext pb, c_int tagLen, ref AvDictionary metadata, CPointer<char> key)//XX 327
		{
			AvDict dict_Flags = AvDict.Dont_Overwrite | AvDict.Dont_Strdup_Val;
			c_uint genre = c_uint.MaxValue;

			if (tagLen < 1)
				return;

			Id3v2Encoding encoding = (Id3v2Encoding)AvIoBuf.AvIo_R8(pb);
			tagLen--;		// Account for encoding type byte

			if (Decode_Str(s, pb, encoding, out CPointer<uint8_t> dst, ref tagLen) < 0)
			{
				Log.Av_Log(s, Log.Av_Log_Error, "Error reading frame %s, skipped\n", key);
				return;
			}

			CSScanF scan = new CSScanF();
			string dstStr = Encoding.UTF8.GetString(dst.AsSpan());

			if ((scan.Parse(dstStr, "(%d)") == 1) || (scan.Parse(dstStr, "%d") == 1))
				genre = (c_uint)scan.Results[0];

			if (!((CString.strcmp(key, "TCON") != 0) && (CString.strcmp(key, "TCO") != 0)) && (genre <= Id3v1.Genre_Max))
			{
				Mem.Av_FreeP(ref dst);
				dst = Encoding.UTF8.GetBytes(Mem.Av_StrDup(Id3v1.ff_Id3v1_Genre_Str[genre].ToCharPointer()).ToString());
			}
			else if (!((CString.strcmp(key, "TXXX") != 0) && (CString.strcmp(key, "TXX") != 0)))
			{
				// dst now contains the key, need to get value
				key = dstStr.ToCharPointer();

				if (Decode_Str(s, pb, encoding, out dst, ref tagLen) < 0)
				{
					Log.Av_Log(s, Log.Av_Log_Error, "Error reading frame %s, skipped\n", key);
					Mem.Av_FreeP(ref key);

					return;
				}

				dict_Flags |= AvDict.Dont_Strdup_Key;
			}
			else if (dst[0] == '\0')
				Mem.Av_FreeP(ref dst);

			if (dst.IsNotNull)
				Dict.Av_Dict_Set(ref metadata, key, ConvertToString(dst), dict_Flags);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Read_Uslt(AvFormatContext s, AvIoContext pb, c_int tagLen, ref AvDictionary metadata)//XX 366
		{
			CPointer<uint8_t> lang = new CPointer<uint8_t>(4);
			CPointer<uint8_t> descriptor = null;
			c_int ok = 0;

			if (tagLen < 4)
				goto Error;

			Id3v2Encoding encoding = (Id3v2Encoding)AvIoBuf.AvIo_R8(pb);
			tagLen--;

			if (AvIoBuf.AvIo_Read(pb, lang, 3) < 3)
				goto Error;

			lang[3] = 0;
			tagLen -= 3;

			if ((Decode_Str(s, pb, encoding, out descriptor, ref tagLen) < 0) || (tagLen < 0))
				goto Error;

			if ((Decode_Str(s, pb, encoding, out CPointer<uint8_t> text, ref tagLen) < 0) || (tagLen < 0))
				goto Error;

			// FFmpeg does not support hierarchical metadata, so concatenate the keys
			CPointer<char> key = AvString.Av_Asprintf("lyrics-%s%s%s", descriptor[0] != 0 ? Encoding.UTF8.GetString(descriptor.AsSpan()) : string.Empty, descriptor[0] != 0 ? "-" : string.Empty, Encoding.UTF8.GetString(lang.AsSpan()));

			if (key.IsNull)
			{
				Mem.Av_Free(text);
				goto Error;
			}

			Dict.Av_Dict_Set(ref metadata, key, ConvertToString(text), AvDict.Dont_Strdup_Key | AvDict.Dont_Strdup_Val);

			ok = 1;

			Error:
			if (ok == 0)
				Log.Av_Log(s, Log.Av_Log_Error, "Error reading lyrics, skipped\n");

			Mem.Av_Free(descriptor);
		}



		/********************************************************************/
		/// <summary>
		/// Parse a comment tag
		/// </summary>
		/********************************************************************/
		private static void Read_Comment(AvFormatContext s, AvIoContext pb, c_int tagLen, ref AvDictionary metadata)//XX 415
		{
			string key = "comment";
			AvDict dict_Flags = AvDict.Dont_Overwrite | AvDict.Dont_Strdup_Val;

			if (tagLen < 4)
				return;

			Id3v2Encoding encoding = (Id3v2Encoding)AvIoBuf.AvIo_R8(pb);
			c_int language = (c_int)AvIoBuf.AvIo_RB24(pb);
			tagLen -= 4;

			if (Decode_Str(s, pb, encoding, out CPointer<uint8_t> dst, ref tagLen) < 0)
			{
				Log.Av_Log(s, Log.Av_Log_Error, "Error reading comment frame, skipped\n");
				return;
			}

			if (dst.IsNotNull && (dst[0] == 0))
				Mem.Av_FreeP(ref dst);

			if (dst.IsNotNull)
			{
				key = Encoding.UTF8.GetString(dst.AsSpan());
				dict_Flags |= AvDict.Dont_Strdup_Key;
			}

			if (Decode_Str(s, pb, encoding, out dst, ref tagLen) < 0)
			{
				Log.Av_Log(s, Log.Av_Log_Error, "Error reading comment frame, skipped\n");

				if ((dict_Flags & AvDict.Dont_Strdup_Key) != 0)
					Mem.Av_FreeP(ref key);

				return;
			}

			if (dst.IsNotNull)
				Dict.Av_Dict_Set(ref metadata, key, Encoding.UTF8.GetString(dst.AsSpan()), dict_Flags);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void List_Append(Id3v2ExtraMeta new_Elem, ExtraMetaList list)//XX 458
		{
			if (list.Tail != null)
				list.Tail.Next = new_Elem;
			else
				list.Head = new_Elem;

			list.Tail = new_Elem;
		}



		/********************************************************************/
		/// <summary>
		/// Parse GEOB tag into a ID3v2ExtraMetaGEOB struct
		/// </summary>
		/********************************************************************/
		private static void Read_GeobTag(AvFormatContext s, AvIoContext pb, c_int tagLen, CPointer<char> tag, ref ExtraMetaList extra_Meta, bool isV34)//XX 470
		{
			if (tagLen < 1)
				return;

			Id3v2ExtraMeta new_Extra = Mem.Av_MAlloczObj<Id3v2ExtraMeta>();

			if (new_Extra == null)
			{
				Log.Av_Log(s, Log.Av_Log_Error, "Failed to alloc Id3v2ExtraMeta\n");
				return;
			}

			new_Extra.Data = new Id3v2ExtraMetadataUnion(new Id3v2ExtraMetaGEOB());

			Id3v2ExtraMetaGEOB geob_Data = new_Extra.Data.Geob;

			// Read encoding type byte
			Id3v2Encoding encoding = (Id3v2Encoding)AvIoBuf.AvIo_R8(pb);
			tagLen--;

			// Read MIME type (always ISO-8859)
			if ((Decode_Str(s, pb, Id3v2Encoding.Iso8859, out geob_Data.Mime_Type, ref tagLen) < 0) || (tagLen <= 0))
				goto Fail;

			// Read file name
			if ((Decode_Str(s, pb, encoding, out geob_Data.File_Name, ref tagLen) < 0) || (tagLen <= 0))
				goto Fail;

			// Read content description
			if ((Decode_Str(s, pb, encoding, out geob_Data.Description, ref tagLen) < 0) || (tagLen <= 0))
				goto Fail;

			if (tagLen != 0)
			{
				// Save encapsulated binary data
				geob_Data.Data = Mem.Av_MAlloc<uint8_t>((size_t)tagLen);

				if (geob_Data.Data.IsNull)
				{
					Log.Av_Log(s, Log.Av_Log_Error, "Failed to alloc %d bytes", tagLen);
					goto Fail;
				}

				c_uint len = (c_uint)AvIoBuf.AvIo_Read(pb, geob_Data.Data, tagLen);

				if (len < tagLen)
					Log.Av_Log(s, Log.Av_Log_Warning, "Error reading GEOB frame, data truncated.\n");

				geob_Data.DataSize = len;
			}
			else
			{
				geob_Data.Data.SetToNull();
				geob_Data.DataSize = 0;
			}

			// Add data to the list
			new_Extra.Tag = "GEOB".ToCharPointer();

			List_Append(new_Extra, extra_Meta);

			return;

			Fail:
			Log.Av_Log(s, Log.Av_Log_Error, "Error reading frame %s, skipped\n", tag);

			Free_GeobTag(geob_Data);
			Mem.Av_Free(new_Extra);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static bool Is_Number(CPointer<char> str)//XX 539
		{
			while ((str[0] >= '0') && (str[0] <= '9'))
				str++;

			return str[0] == '\0';
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static AvDictionaryEntry Get_Date_Tag(AvDictionary m, string tag)//XX 546
		{
			AvDictionaryEntry t = Dict.Av_Dict_Get(m, tag, AvDict.Match_Case).FirstOrDefault();

			if ((t != null) && (CString.strlen(t.Value) == 4) && Is_Number(t.Value))
				return t;

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Merge_Date(ref AvDictionary m)//XX 555
		{
			AvDictionaryEntry t;
			CPointer<char> date = new CPointer<char>(17);

			if (((t = Get_Date_Tag(m, "TYER")) == null) && ((t = Get_Date_Tag(m, "TYE")) == null))
				return;

			AvString.Av_Strlcpy(date, t.Value, 5);

			Dict.Av_Dict_Set(ref m, "TYER", new CPointer<char>(), AvDict.None);
			Dict.Av_Dict_Set(ref m, "TYE", new CPointer<char>(), AvDict.None);

			if (((t = Get_Date_Tag(m, "TDAT")) == null) && ((t = Get_Date_Tag(m, "TDA")) == null))
				goto Finish;

			CString.snprintf(date + 4, (size_t)date.Length - 4, "-%.2s-%.2s", t.Value + 2, t.Value);

			Dict.Av_Dict_Set(ref m, "TDAT", new CPointer<char>(), AvDict.None);
			Dict.Av_Dict_Set(ref m, "TDA", new CPointer<char>(), AvDict.None);

			if (((t = Get_Date_Tag(m, "TIME")) == null) && ((t = Get_Date_Tag(m, "TIM")) == null))
				goto Finish;

			CString.snprintf(date + 10, (size_t)date.Length - 10, " %.2s:%.2s", t.Value, t.Value + 2);

			Dict.Av_Dict_Set(ref m, "TIME", new CPointer<char>(), AvDict.None);
			Dict.Av_Dict_Set(ref m, "TIM", new CPointer<char>(), AvDict.None);

			Finish:
			if (date[0] != '\0')
				Dict.Av_Dict_Set(ref m, "date", date, AvDict.None);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Free_APic(IExtraMetadata obj)//XX 587
		{
			Id3v2ExtraMetaAPIC apic = (Id3v2ExtraMetaAPIC)obj;

			Buffer.Av_Buffer_Unref(ref apic.Buf);
			Mem.Av_FreeP(ref apic.Description);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void RStrip_Spaces(CPointer<uint8_t> buf)//XX 594
		{
			c_int len = (c_int)CString.strlen(buf.Cast<uint8_t, char>());
			if (len < 0)
				len = buf.Length;

			while ((len > 0) && (buf[len - 1] == ' '))
				buf[--len] = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Read_APic(AvFormatContext s, AvIoContext pb, c_int tagLen, CPointer<char> tag, ref ExtraMetaList extra_Meta, bool isV34)//XX 601
		{
			CPointer<char> mimeType = new CPointer<char>(64);
			AvCodecId id = AvCodecId.None;
			Id3v2ExtraMetaAPIC apic = null;
			Id3v2ExtraMeta new_Extra = null;
			int64_t end = AvIoBuf.AvIo_Tell(pb) + tagLen;

			if ((tagLen <= 4) || (!isV34 && (tagLen <= 6)))
				goto Fail;

			new_Extra = Mem.Av_MAlloczObj<Id3v2ExtraMeta>();

			if (new_Extra == null)
				goto Fail;

			new_Extra.Data = new Id3v2ExtraMetadataUnion(new Id3v2ExtraMetaAPIC());

			apic = new_Extra.Data.APic;

			Id3v2Encoding enc = (Id3v2Encoding)AvIoBuf.AvIo_R8(pb);
			tagLen--;

			// Mimetype
			if (isV34)
			{
				c_int ret = AvIoBuf.AvIo_Get_Str(pb, tagLen, mimeType, mimeType.Length);

				if ((ret < 0) || (ret >= tagLen))
					goto Fail;

				tagLen -= ret;
			}
			else
			{
				if (AvIoBuf.AvIo_Read(pb, mimeType, 3) < 0)
					goto Fail;

				mimeType[3] = '\0';
				tagLen -= 3;
			}

			foreach (CodecMime mime in ff_Id3v2_Mime_Tags)
			{
				if (AvString.Av_Strncasecmp(mime.Str, mimeType, (size_t)mimeType.Length) == 0)
				{
					id = mime.Id;
					break;
				}
			}

			if (id == AvCodecId.None)
			{
				Log.Av_Log(s, Log.Av_Log_Warning, "Unknown attached picture mimetype: %s, skipping.\n", mimeType);
				goto Fail;
			}

			apic.Id = id;

			// Picture type
			c_int pic_Type = AvIoBuf.AvIo_R8(pb);
			tagLen--;

			if ((pic_Type < 0) || ((size_t)pic_Type >= Macros.FF_Array_Elems(ff_Id3v2_Picture_Types)))
			{
				Log.Av_Log(s, Log.Av_Log_Warning, "Unknown attached picture type %d.\n", pic_Type);
				pic_Type = 0;
			}

			apic.Type = ff_Id3v2_Picture_Types[pic_Type].ToCharPointer();

			// Description and picture data
			if (Decode_Str(s, pb, enc, out apic.Description, ref tagLen) < 0)
			{
				Log.Av_Log(s, Log.Av_Log_Error, "Error decoding attached picture description.\n");
				goto Fail;
			}

			apic.Buf = Buffer.Av_Buffer_Alloc((size_t)tagLen + Defs.Av_Input_Buffer_Padding_Size);

			if ((apic.Buf == null) || (tagLen == 0) || (AvIoBuf.AvIo_Read(pb, ((DataBufferContext)apic.Buf.Data).Data, tagLen) != tagLen))
				goto Fail;

			CMemory.memset<uint8_t>(((DataBufferContext)apic.Buf.Data).Data + tagLen, 0, Defs.Av_Input_Buffer_Padding_Size);

			new_Extra.Tag = "APIC".ToCharPointer();

			// The description must be unique, and some ID3v2 tag writers add spaces
			// to write several APIC entries with the same description
			RStrip_Spaces(apic.Description);
			List_Append(new_Extra, extra_Meta);

			return;

			Fail:
			if (apic != null)
				Free_APic(apic);

			Mem.Av_FreeP(ref new_Extra);
			AvIoBuf.AvIo_Seek(pb, end, AvSeek.Set);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Free_Chapter(IExtraMetadata obj)//XX 690
		{
			Id3v2ExtraMetaCHAP chap = (Id3v2ExtraMetaCHAP)obj;

			Mem.Av_FreeP(ref chap.Element_Id);
			Dict.Av_Dict_Free(ref chap.Meta);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Read_Chapter(AvFormatContext s, AvIoContext pb, c_int len, CPointer<char> tag, ref ExtraMetaList extra_Meta, bool isV34)//XX 697
		{
			Id3v2ExtraMeta new_Extra = Mem.Av_MAlloczObj<Id3v2ExtraMeta>();

			if (new_Extra == null)
				return;

			new_Extra.Data = new Id3v2ExtraMetadataUnion(new Id3v2ExtraMetaCHAP());

			Id3v2ExtraMetaCHAP chap = new_Extra.Data.Chap;

			if (Decode_Str(s, pb, Id3v2Encoding.Iso8859, out chap.Element_Id, ref len) < 0)
				goto Fail;

			if (len < 16)
				goto Fail;

			chap.Start = AvIoBuf.AvIo_RB32(pb);
			chap.End = AvIoBuf.AvIo_RB32(pb);
			AvIoBuf.AvIo_Skip(pb, 8);

			len -= 16;

			while (len > 10)
			{
				if (AvIoBuf.AvIo_Read(pb, tag, 4) < 4)
					goto Fail;

				tag[4] = '\0';

				c_int tagLen = (c_int)AvIoBuf.AvIo_RB32(pb);
				AvIoBuf.AvIo_Skip(pb, 2);

				len -= 10;

				if ((tagLen < 0) || (tagLen > len))
					goto Fail;

				if (tag[0] == 'T')
					Read_TTag(s, pb, tagLen, ref chap.Meta, tag);
				else
					AvIoBuf.AvIo_Skip(pb, tagLen);

				len -= tagLen;
			}

			Metadata.FF_Metadata_Conv(ref chap.Meta, null, ff_Id3v2_34_Metadata_Conv);
			Metadata.FF_Metadata_Conv(ref chap.Meta, null, ff_Id3v2_4_Metadata_Conv);

			new_Extra.Tag = "CHAP".ToCharPointer();

			List_Append(new_Extra, extra_Meta);

			return;

			Fail:
			Free_Chapter(chap);
			Mem.Av_FreeP(ref new_Extra);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Free_Priv(IExtraMetadata obj)//XX 751
		{
			Id3v2ExtraMetaPRIV priv = (Id3v2ExtraMetaPRIV)obj;

			Mem.Av_FreeP(ref priv.Owner);
			Mem.Av_FreeP(ref priv.Data);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Read_Priv(AvFormatContext s, AvIoContext pb, c_int tagLen, CPointer<char> tag, ref ExtraMetaList extra_Meta, bool isV34)//XX 758
		{
			Id3v2ExtraMeta meta = Mem.Av_MAlloczObj<Id3v2ExtraMeta>();

			if (meta == null)
				return;

			meta.Data = new Id3v2ExtraMetadataUnion(new Id3v2ExtraMetaPRIV());

			Id3v2ExtraMetaPRIV priv = meta.Data.Priv;

			if (Decode_Str(s, pb, Id3v2Encoding.Iso8859, out priv.Owner, ref tagLen) < 0)
				goto Fail;

			priv.Data = Mem.Av_MAlloc<uint8_t>((size_t)tagLen);

			if (priv.Data.IsNull)
				goto Fail;

			priv.DataSize = (uint32_t)tagLen;

			if (AvIoBuf.AvIo_Read(pb, priv.Data, (c_int)priv.DataSize) != priv.DataSize)
				goto Fail;

			meta.Tag = "PRIV".ToCharPointer();

			List_Append(meta, extra_Meta);

			return;

			Fail:
			Free_Priv(priv);
			Mem.Av_FreeP(ref meta);
		}



		/********************************************************************/
		/// <summary>
		/// Get the corresponding ID3v2EMFunc struct for a tag
		/// </summary>
		/********************************************************************/
		private static Id3v2EmFunc Get_Extra_Meta_Func(CPointer<char> tag, bool isV34)//XX 814
		{
			foreach (Id3v2EmFunc f in id3v2_Extra_Meta_Funcs)
			{
				if (tag.IsNotNull && (CMemory.memcmp(tag, isV34 ? f.Tag4 : f.Tag3, isV34 ? 4U : 3U) == 0))
					return f;
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Id3v2_Parse(AvIoContext pb, ref AvDictionary metadata, AvFormatContext s, c_int len, uint8_t version, uint8_t flags, ref ExtraMetaList extra_Meta)//XX 828
		{
			bool isV34;
			c_uint tLen;
			CPointer<char> tag = new CPointer<char>(5);
			int64_t end = AvIoBuf.AvIo_Tell(pb);
			c_int tagHdrLen;
			string reason = null;
			FFIoContext pb_Local = new FFIoContext();
			CPointer<c_uchar> buffer = null;
			c_int buffer_Size = 0;
			Id3v2EmFunc extra_Func = null;
			CPointer<c_uchar> uncompressed_Buffer = null;
			string comm_Frame;

			if (end > (int64_t.MaxValue - len - 10))
				return;

			end += len;

			Log.Av_Log(s, Log.Av_Log_Debug, "id3v2 ver:%d flags:%02X len:%d\n", version, flags, len);

			switch (version)
			{
				case 2:
				{
					if ((flags & 0x40) != 0)
					{
						reason = "compression";
						goto Error;
					}

					isV34 = false;
					tagHdrLen = 6;
					comm_Frame = "COM";
					break;
				}

				case 3:
				case 4:
				{
					isV34 = true;
					tagHdrLen = 10;
					comm_Frame = "COMM";
					break;
				}

				default:
				{
					reason = "version";
					goto Error;
				}
			}

			bool unsync = (flags & 0x80) != 0;

			if (isV34 && ((flags & 0x40) != 0))		// Extended header present, just skip over it
			{
				c_int extLen = (c_int)Get_Size(pb, 4);

				if (version == 4)
				{
					// In v2.4 the length includes the length field we just read
					extLen -= 4;
				}

				if (extLen < 0)
				{
					reason = "invalid extended header length";
					goto Error;
				}

				AvIoBuf.AvIo_Skip(pb, extLen);
				len -= extLen + 4;

				if (len < 0)
				{
					reason = "extended header too long.";
					goto Error;
				}
			}

			while (len >= tagHdrLen)
			{
				Id3v2_Flag tFlags = Id3v2_Flag.None;
				bool tunSync = false;
				bool tComp = false;
				bool tEncr = false;
				c_ulong dLen;

				if (isV34)
				{
					if (AvIoBuf.AvIo_Read(pb, tag, 4) < 4)
						break;

					tag[4] = '\0';

					if (version == 3)
						tLen = AvIoBuf.AvIo_RB32(pb);
					else
					{
						// Some encoders incorrectly uses v3 sizes instead of syncsafe ones
						// so check the next tag to see which one to use
						tLen = AvIoBuf.AvIo_RB32(pb);

						if (tLen > 0x7f)
						{
							if (tLen < len)
							{
								int64_t cur = AvIoBuf.AvIo_Tell(pb);

								if (AvIoBuf.FFIo_Ensure_SeekBack(pb, 2 /* tFlags */ + tLen + 4 /* next_Tag */) != 0)
									break;

								if (Check_Tag(pb, (c_int)(cur + 2 + Size_To_SyncSafe(tLen)), 4) == 1)
									tLen = Size_To_SyncSafe(tLen);
								else if (Check_Tag(pb, (c_int)(cur + 2 + tLen), 4) != 1)
									break;

								AvIoBuf.AvIo_Seek(pb, cur, AvSeek.Set);
							}
							else
								tLen = Size_To_SyncSafe(tLen);
						}
					}

					tFlags = (Id3v2_Flag)AvIoBuf.AvIo_RB16(pb);
					tunSync = (tFlags & Id3v2_Flag.Unsync) != 0;
				}
				else
				{
					if (AvIoBuf.AvIo_Read(pb, tag, 3) < 3)
						break;

					tag[3] = '\0';

					tLen = AvIoBuf.AvIo_RB24(pb);
				}

				if (tLen > (1 << 28))
					break;

				len = (c_int)(len - tagHdrLen + tLen);

				if (len < 0)
					break;

				int64_t next = AvIoBuf.AvIo_Tell(pb) + tLen;

				if (tLen == 0)
				{
					if (tag[0] != '\0')
						Log.Av_Log(s, Log.Av_Log_Debug, "Invalid empty frame %s, skipping.\n", tag);

					continue;
				}

				if ((tFlags & Id3v2_Flag.DataLen) != 0)
				{
					if (tLen < 4)
						break;

					dLen = AvIoBuf.AvIo_RB32(pb);
					tLen -= 4;
				}
				else
					dLen = tLen;

				tComp = (tFlags & Id3v2_Flag.Compression) != 0;
				tEncr = (tFlags & Id3v2_Flag.Encryption) != 0;

				// Skip encrypted tags and, if no zlib, compression tags
				if (tEncr || tComp)
				{
					string type;

					if (!tComp)
						type = "encrypted";
					else if (!tEncr)
						type = "compressed";
					else
						type = "encrypted and compressed";

					Log.Av_Log(s, Log.Av_Log_Warning, "Skipping %s ID3v2 frame %s.\n", type, tag);
					AvIoBuf.AvIo_Skip(pb, tLen);
				}
				// Check for text tag or supported special meta tag
				else if ((tag[0] == 'T') || (CMemory.memcmp(tag, "USLT".ToCharPointer(), 4) == 0) || (CString.strcmp(tag, comm_Frame) == 0) || ((extra_Meta != null) && ((extra_Func = Get_Extra_Meta_Func(tag, isV34)) != null)))
				{
					AvIoContext pbx = pb;

					if (unsync || tunSync || tComp)
					{
						Mem.Av_Fast_MAlloc(ref buffer, ref buffer_Size, tLen);

						if (buffer.IsNull)
						{
							Log.Av_Log(s, Log.Av_Log_Error, "Failed to alloc %d bytes\n", tLen);
							goto Seek;
						}
					}

					if (unsync || tunSync)
					{
						CPointer<uint8_t> b = buffer;
						CPointer<uint8_t> t = buffer;
						CPointer<uint8_t> end1 = t + len;

						if (AvIoBuf.AvIo_Read(pb, buffer, (c_int)tLen) != tLen)
						{
							Log.Av_Log(s, Log.Av_Log_Error, "Failed to read tag data\n");
							goto Seek;
						}

						while (t != end1)
						{
							b[0, 1] = t[0, 1];

							if ((t != end1) && (t[-1] == 0xff) && (t[0] == 0))
								t++;
						}

						AvIoBuf.FFIo_Init_Read_Context(pb_Local, buffer, b - buffer);

						tLen = (c_uint)(b - buffer);
						pbx = pb_Local.Pub;		// Read from sync buffer
					}

					if (tag[0] == 'T')
					{
						// Parse text tag
						Read_TTag(s, pbx, (c_int)tLen, ref metadata, tag);
					}
					else if (CMemory.memcmp(tag, "USLT".ToCharPointer(), 4) == 0)
						Read_Uslt(s, pbx, (c_int)tLen, ref metadata);
					else if (CString.strcmp(tag, comm_Frame) == 0)
						Read_Comment(s, pb, (c_int)tLen, ref metadata);
					else
					{
						// Parse special meta tag
						extra_Func.Read(s, pbx, (c_int)tLen, tag, ref extra_Meta, isV34);
					}
				}
				else if (tag[0] == '\0')
				{
					if (tag[1] != '\0')
						Log.Av_Log(s, Log.Av_Log_Warning, "invalid frame id, assuming padding\n");

					AvIoBuf.AvIo_Skip(pb, tLen);
				}

				// Skip to end of tag
				Seek:
				AvIoBuf.AvIo_Seek(pb, next, AvSeek.Set);
			}

			// Footer preset, alway 10 bytes, skip over it
			if ((version == 4) && ((flags & 0x10) != 0))
				end += 10;

			Error:
			if (reason != null)
				Log.Av_Log(s, Log.Av_Log_Info, "ID3v2.%d tag skipped, cannot handle %s\n", version, reason);

			AvIoBuf.AvIo_Seek(pb, end, AvSeek.Set);

			Mem.Av_Free(buffer);
			Mem.Av_Free(uncompressed_Buffer);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Id3v2_Read_Internal(AvIoContext pb, ref AvDictionary metadata, AvFormatContext s, CPointer<char> magic, out Id3v2ExtraMeta extra_MetaP, int64_t max_Search_Size)//XX 1083
		{
			CPointer<uint8_t> buf = new CPointer<uint8_t>(Header_Size);
			ExtraMetaList extra_Meta = new ExtraMetaList { Head = null };
			c_int found_Header;

			extra_MetaP = null;

			if ((max_Search_Size != 0) && (max_Search_Size < Header_Size))
				return;

			int64_t start = AvIoBuf.AvIo_Tell(pb);

			do
			{
				// Save the current offset in case there's nothing to read/skip
				int64_t off = AvIoBuf.AvIo_Tell(pb);

				if ((max_Search_Size != 0) && ((off - start) >= (max_Search_Size - Header_Size)))
				{
					AvIoBuf.AvIo_Seek(pb, off, AvSeek.Set);
					break;
				}

				c_int ret = AvIoBuf.FFIo_Ensure_SeekBack(pb, Header_Size);

				if (ret >= 0)
					ret = AvIoBuf.AvIo_Read(pb, buf, Header_Size);

				if (ret != Header_Size)
				{
					AvIoBuf.AvIo_Seek(pb, off, AvSeek.Set);
					break;
				}

				found_Header = FF_Id3v2_Match(buf, magic);

				if (found_Header != 0)
				{
					// Parse ID3v2 header
					c_int len = ((buf[6] & 0x7f) << 21) |
								((buf[7] & 0x7f) << 14) |
								((buf[8] & 0x7f) << 7) |
								(buf[9] & 0x7f);

					Id3v2_Parse(pb, ref metadata, s, len, buf[3], buf[5], ref extra_Meta);
				}
				else
					AvIoBuf.AvIo_Seek(pb, off, AvSeek.Set);
			}
			while (found_Header != 0);

			Metadata.FF_Metadata_Conv(ref metadata, null, ff_Id3v2_34_Metadata_Conv);
			Metadata.FF_Metadata_Conv(ref metadata, null, ff_Id3v2_2_Metadata_Conv);
			Metadata.FF_Metadata_Conv(ref metadata, null, ff_Id3v2_4_Metadata_Conv);

			Merge_Date(ref metadata);

			extra_MetaP = extra_Meta.Head;
		}
		#endregion
	}
}
