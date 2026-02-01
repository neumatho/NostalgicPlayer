/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Demuxer
{
	/// <summary>
	/// 
	/// </summary>
	public static class Asf
	{
		/// <summary>
		/// List of official tags at http://msdn.microsoft.com/en-us/library/dd743066(VS.85).aspx
		/// </summary>
		public static readonly AvMetadataConv[] FF_Asf_Metadata_Conv =
		[
			new AvMetadataConv("WM/AlbumArtist", "album_artist"),
			new AvMetadataConv("WM/AlbumTitle", "album"),
			new AvMetadataConv("Author", "artist"),
			new AvMetadataConv("Description", "comment"),
			new AvMetadataConv("WM/Composer", "composer"),
			new AvMetadataConv("WM/EncodedBy", "encoded_by"),
			new AvMetadataConv("WM/EncodingSettings", "encoder"),
			new AvMetadataConv("WM/Genre", "genre"),
			new AvMetadataConv("WM/Language", "language"),
			new AvMetadataConv("WM/OriginalFilename", "filename"),
			new AvMetadataConv("WM/PartOfSet", "disc"),
			new AvMetadataConv("WM/Publisher", "publisher"),
			new AvMetadataConv("WM/Tool", "encoder"),
			new AvMetadataConv("WM/TrackNumber", "track"),
			new AvMetadataConv("WM/MediaStationCallSign", "service_provider"),
			new AvMetadataConv("WM/MediaStationName", "service_name")
		];

		/********************************************************************/
		/// <summary>
		/// Handles both attached pictures as well as id3 tags
		/// </summary>
		/********************************************************************/
		public static c_int FF_Asf_Handle_Byte_Array(AvFormatContext s, CPointer<char> name, c_int val_Len)//XX 142
		{
			if (CString.strcmp(name, "WM/Picture") == 0)	// Handle cover art
				return Asf_Read_Picture(s, val_Len);
			else if (CString.strcmp(name, "ID3") == 0)		// Handle ID3 tag
				return Get_Id3_Tag(s, val_Len);

			return 1;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// MSDN claims that this should be "compatible with the ID3 frame,
		/// APIC", but in reality this is only loosely similar
		/// </summary>
		/********************************************************************/
		private static c_int Asf_Read_Picture(AvFormatContext s, c_int len)//XX 51
		{
			AvCodecId id = AvCodecId.None;
			CPointer<char> mimeType = new CPointer<char>(64);
			CPointer<char> desc = null;
			AvStream st = null;

			// Type + picSize + mime + desc
			if (len < (1 + 4 + 2 + 2))
			{
				Log.Av_Log(s, Log.Av_Log_Error, "Invalid attached picture size: %d.\n", len);

				return Error.InvalidData;
			}

			// Picture type
			c_int type = AvIoBuf.AvIo_R8(s.Pb);
			len--;

			if ((type >= (c_int)Macros.FF_Array_Elems(Id3v2.ff_Id3v2_Picture_Types)) || (type < 0))
			{
				Log.Av_Log(s, Log.Av_Log_Warning, "Unknown attached picture type: %d.\n", type);
				type = 0;
			}

			c_int picSize = (c_int)AvIoBuf.AvIo_RL32(s.Pb);
			len -= 4;

			// Picture MIME type
			len -= AvIoBuf.AvIo_Get_Str16LE(s.Pb, len, mimeType, mimeType.Length);

			foreach (CodecMime mime in Id3v2.ff_Id3v2_Mime_Tags)
			{
				if (CString.strncmp(mime.Str, mimeType, (size_t)mimeType.Length) == 0)
				{
					id = mime.Id;
					break;
				}
			}

			if (id == AvCodecId.None)
			{
				Log.Av_Log(s, Log.Av_Log_Error, "Unknown attached picture mimetype: %s.\n", mimeType);

				return 0;
			}

			if ((picSize >= len) || ((((int64_t)len - picSize) * 2) + 1) > c_int.MaxValue)
			{
				Log.Av_Log(s, Log.Av_Log_Error, "Invalid attached picture data size: %d  (len = %d).\n", picSize, len);

				return Error.InvalidData;
			}

			// Picture description
			c_int desc_Len = ((len - picSize) * 2) + 1;
			desc = Mem.Av_MAlloc<char>((size_t)desc_Len);

			if (desc.IsNull)
				return Error.ENOMEM;

			len -= AvIoBuf.AvIo_Get_Str16LE(s.Pb, len - picSize, desc, desc_Len);

			c_int ret = Demux_Utils.FF_Add_Attached_Pic(s, null, s.Pb, picSize);

			if (ret < 0)
				goto Fail;

			st = s.Streams[s.Nb_Streams - 1];

			st.CodecPar.Codec_Id = id;

			if (desc[0] != '\0')
			{
				if (Dict.Av_Dict_Set(ref st.Metadata, "title", desc, AvDict.Dont_Strdup_Val) < 0)
					Log.Av_Log(s, Log.Av_Log_Warning, "av_dict_set failed.\n");
				else
					Mem.Av_FreeP(ref desc);
			}

			if (Dict.Av_Dict_Set(ref st.Metadata, "comment", Id3v2.ff_Id3v2_Picture_Types[type], AvDict.None) < 0)
				Log.Av_Log(s, Log.Av_Log_Warning, "av_dict_set failed.\n");

			return 0;

			Fail:
			Mem.Av_FreeP(ref desc);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Get_Id3_Tag(AvFormatContext s, c_int len)//XX 129
		{
			Id3v2.FF_Id3v2_Read(s, Id3v2.Default_Magic, out Id3v2ExtraMeta id3V2_Extra_Meta, (c_uint)len);

			if (id3V2_Extra_Meta != null)
			{
				Id3v2.FF_Id3v2_Parse_Apic(s, id3V2_Extra_Meta);
				Id3v2.FF_Id3v2_Parse_Chapters(s, id3V2_Extra_Meta);
				Id3v2.FF_Id3v2_Free_Extra_Meta(ref id3V2_Extra_Meta);
			}

			return 0;
		}
		#endregion
	}
}
