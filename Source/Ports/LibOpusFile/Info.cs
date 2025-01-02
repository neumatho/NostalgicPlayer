/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Text;
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Ports.LibOgg;
using Polycode.NostalgicPlayer.Ports.LibOpusFile.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpusFile
{
	/// <summary>
	/// 
	/// </summary>
	public static class Info
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_uint Op_Parse_UInt16LE(CPointer<byte> _data)
		{
			return _data[0] | (c_uint)_data[1] << 8;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Op_Parse_Int16LE(CPointer<byte> _data)
		{
			c_int ret = _data[0] | _data[1] << 8;

			return (ret ^ 0x8000) - 0x8000;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static opus_uint32 Op_Parse_UInt32LE(CPointer<byte> _data)
		{
			return _data[0] | (opus_uint32)_data[1] << 8 | (opus_uint32)_data[2] << 16 | (opus_uint32)_data[3] << 24;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static opus_uint32 Op_Parse_UInt32BE(CPointer<byte> _data)
		{
			return _data[3] | (opus_uint32)_data[2] << 8 | (opus_uint32)_data[1] << 16 | (opus_uint32)_data[0] << 24;
		}



		/********************************************************************/
		/// <summary>
		/// Parses the contents of the ID header packet of an Ogg Opus stream
		/// </summary>
		/********************************************************************/
		public static OpusFileError Opus_Head_Parse(OpusHead _head, CPointer<byte> _data, size_t _len)
		{
			if (_len < 8)
				return OpusFileError.NotFormat;

			if (CMemory.MemCmp(_data, "OpusHead", 8) != 0)
				return OpusFileError.NotFormat;

			if (_len < 9)
				return OpusFileError.BadHeader;

			OpusHead head = new OpusHead();
			head.Version = _data[8];

			if (head.Version > 15)
				return OpusFileError.Version;

			if (_len < 19)
				return OpusFileError.BadHeader;

			head.Channel_Count = _data[9];
			head.Pre_Skip = Op_Parse_UInt16LE(_data + 10);
			head.Input_Sample_Rate = Op_Parse_UInt32LE(_data + 12);
			head.Output_Gain = Op_Parse_Int16LE(_data + 16);
			head.Mapping_Family = _data[18];

			if (head.Mapping_Family == 0)
			{
				if ((head.Channel_Count < 1) || (head.Channel_Count > 2))
					return OpusFileError.BadHeader;

				if ((head.Version <= 1) && (_len > 19))
					return OpusFileError.BadHeader;

				head.Stream_Count = 1;
				head.Coupled_Count = head.Channel_Count - 1;

				if (_head != null)
				{
					_head.Mapping[0] = 0;
					_head.Mapping[1] = 1;
				}
			}
			else if (head.Mapping_Family == 1)
			{
				if ((head.Channel_Count < 1) || (head.Channel_Count > 8))
					return OpusFileError.BadHeader;

				size_t size = (size_t)(21 + head.Channel_Count);

				if ((_len < size) || (head.Version <= 1) && (_len > size))
					return OpusFileError.BadHeader;

				head.Stream_Count = _data[19];

				if (head.Stream_Count < 1)
					return OpusFileError.BadHeader;

				head.Coupled_Count = _data[20];

				if (head.Coupled_Count > head.Stream_Count)
					return OpusFileError.BadHeader;

				for (c_int ci = 0; ci < head.Channel_Count; ci++)
				{
					if ((_data[21 + ci] >= (head.Stream_Count + head.Coupled_Count)) && (_data[21 + ci] != 255))
						return OpusFileError.BadHeader;
				}

				if (_head != null)
					CMemory.MemCpy(_head.Mapping, _data + 21, head.Channel_Count);
			}
			// General purpose players should not attempt to play back content with
			// channel mapping family 255
			else if (head.Mapping_Family == 255)
				return OpusFileError.Impl;
			// No other channel mapping families are currently defined
			else
				return OpusFileError.BadHeader;

			if (_head != null)
			{
				_head.Version = head.Version;
				_head.Channel_Count = head.Channel_Count;
				_head.Pre_Skip = head.Pre_Skip;
				_head.Input_Sample_Rate = head.Input_Sample_Rate;
				_head.Output_Gain = head.Output_Gain;
				_head.Mapping_Family = head.Mapping_Family;
				_head.Stream_Count = head.Stream_Count;
				_head.Coupled_Count = head.Coupled_Count;
			}

			return OpusFileError.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Initializes an OpusTags structure.
		/// This should be called on a freshly allocated #OpusTags structure
		/// before attempting to use it
		/// </summary>
		/********************************************************************/
		public static void Opus_Tags_Init(out OpusTags _tags)
		{
			_tags = new OpusTags();
		}



		/********************************************************************/
		/// <summary>
		/// Clears the OpusTags structure.
		/// This should be called on an OpusTags structure after it is no
		/// longer needed.
		/// It will free all memory used by the structure members
		/// </summary>
		/********************************************************************/
		public static void Opus_Tags_Clear(OpusTags _tags)
		{
			c_int ncomments = _tags.Comments;

			if (_tags.User_Comments != null)
				ncomments++;

			for (c_int ci = ncomments; ci-- > 0;)
				Memory.Ogg_Free(_tags.User_Comments[ci]);

			Memory.Ogg_Free(_tags.User_Comments);
			_tags.User_Comments.SetToNull();

			Memory.Ogg_Free(_tags.Comment_Lengths);
			_tags.Comment_Lengths.SetToNull();

			Memory.Ogg_Free(_tags.Vendor);
			_tags.Vendor.SetToNull();
		}



		/********************************************************************/
		/// <summary>
		/// Ensure there's room for up to _ncomments comments
		/// </summary>
		/********************************************************************/
		private static c_int Op_Tags_Ensure_Capacity(OpusTags _tags, size_t _ncomments)
		{
			if (_ncomments >= int.MaxValue)
				return (c_int)OpusFileError.Fault;

			size_t size = _ncomments + 1;
			c_int cur_ncomments = _tags.Comments;

			// We only support growing.
			// Trimming requires cleaning up the allocated strings in the old space, and
			// is best handled separately if it's ever needed
			CPointer<c_int> comment_lengths = Memory.Ogg_Realloc(_tags.Comment_Lengths, size);
			if (comment_lengths.IsNull)
				return (c_int)OpusFileError.Fault;

			if (_tags.Comment_Lengths.IsNull)
				comment_lengths[cur_ncomments] = 0;

			comment_lengths[(uint)_ncomments] = comment_lengths[cur_ncomments];
			_tags.Comment_Lengths = comment_lengths;

			size = _ncomments + 1;
			CPointer<CPointer<byte>> user_comments = Memory.Ogg_Realloc(_tags.User_Comments, size);
			if (user_comments.IsNull)
				return (c_int)OpusFileError.Fault;

			if (_tags.User_Comments.IsNull)
				user_comments[cur_ncomments].SetToNull();

			user_comments[(uint)_ncomments] = user_comments[cur_ncomments];
			_tags.User_Comments = user_comments;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Duplicate a (possibly non-NUL terminated) string with a known
		/// length
		/// </summary>
		/********************************************************************/
		private static CPointer<byte> Op_Strdup_With_Len(CPointer<byte> _s, size_t _len)
		{
			size_t size = _len + 1;

			CPointer<byte> ret = Memory.Ogg_MAlloc<byte>(size);
			if (ret.IsNotNull)
			{
				CMemory.MemCpy(ret, _s, (int)_len);
				ret[(uint)_len] = 0x00;
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// The actual implementation of opus_tags_parse().
		/// Unlike the public API, this function requires _tags to already
		/// be initialized, modifies its contents before success is
		/// guaranteed, and assumes the caller will clear it on error
		/// </summary>
		/********************************************************************/
		private static c_int Opus_Tags_Parse_Impl(OpusTags _tags, CPointer<byte> _data, size_t _len)
		{
			size_t len = _len;

			if (len < 8)
				return (c_int)OpusFileError.NotFormat;

			if (CMemory.MemCmp(_data, "OpusTags", 8) != 0)
				return (c_int)OpusFileError.NotFormat;

			if (len < 16)
				return (c_int)OpusFileError.BadHeader;

			_data += 8;
			len -= 8;

			opus_uint32 count = Op_Parse_UInt32LE(_data);
			_data += 4;
			len -= 4;

			if (count > len)
				return (c_int)OpusFileError.BadHeader;

			if (_tags != null)
			{
				_tags.Vendor = Op_Strdup_With_Len(_data, count);
				if (_tags.Vendor == null)
					return (c_int)OpusFileError.Fault;
			}

			_data += count;
			len -= count;

			if (len < 4)
				return (c_int)OpusFileError.BadHeader;

			count = Op_Parse_UInt32LE(_data);
			_data += 4;
			len -= 4;

			// Check to make sure there's minimally sufficient data left in the packet
			if (count > (len >> 2))
				return (c_int)OpusFileError.BadHeader;

			// Check for overflow (the API limits this to an int)
			if (count > ((opus_uint32)int.MaxValue - 1))
				return (c_int)OpusFileError.Fault;

			if (_tags != null)
			{
				c_int ret = Op_Tags_Ensure_Capacity(_tags, count);
				if (ret < 0)
					return ret;
			}

			c_int ncomments = (c_int)count;

			for (c_int ci = 0; ci < ncomments; ci++)
			{
				// Check to make sure there's minimally sufficient data left in the packet
				if ((size_t)(ncomments - ci) > (len >> 2))
					return (c_int)OpusFileError.BadHeader;

				count = Op_Parse_UInt32LE(_data);
				_data += 4;
				len -= 4;

				if (count > len)
					return (c_int)OpusFileError.BadHeader;

				// Check for overflow (the API limits this to an int)
				if (count > int.MaxValue)
					return (c_int)OpusFileError.Fault;

				if (_tags != null)
				{
					_tags.User_Comments[ci] = Op_Strdup_With_Len(_data, count);

					if (_tags.User_Comments[ci] == null)
						return (c_int)OpusFileError.Fault;

					_tags.Comment_Lengths[ci] = (c_int)count;
					_tags.Comments = ci + 1;

					// Needed by opus_tags_clear() if we fail before parsing the (optional)
					// binary metadata
					_tags.User_Comments[ci + 1].SetToNull();
				}

				_data += count;
				len -= count;
			}

			if ((len > 0) && ((_data[0] & 1) != 0))
			{
				if (len > int.MaxValue)
					return (c_int)OpusFileError.Fault;

				if (_tags != null)
				{
					_tags.User_Comments[ncomments] = Memory.Ogg_MAlloc<byte>(len);

					if (_tags.User_Comments[ncomments] == null)
						return (c_int)OpusFileError.Fault;

					CMemory.MemCpy(_tags.User_Comments[ncomments], _data, (int)len);
					_tags.Comment_Lengths[ncomments] = (c_int)len;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Parses the contents of the 'comment' header packet of an Ogg Opus
		/// stream
		/// </summary>
		/********************************************************************/
		public static c_int Opus_Tags_Parse(OpusTags _tags, CPointer<byte> _data, size_t _len)
		{
			if (_tags != null)
			{
				Opus_Tags_Init(out OpusTags tags);

				c_int ret = Opus_Tags_Parse_Impl(tags, _data, _len);
				if (ret < 0)
					Opus_Tags_Clear(tags);
				else
				{
					_tags.User_Comments = tags.User_Comments;
					_tags.Comment_Lengths = tags.Comment_Lengths;
					_tags.Comments = tags.Comments;
					_tags.Vendor = tags.Vendor;
				}

				return ret;
			}
			else
				return Opus_Tags_Parse_Impl(null, _data, _len);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Opus_Tagncompare(string _tag_name, CPointer<byte> _comment)
		{
			c_int ret = Internal.Op_Strncasecmp(_tag_name, _comment);

			return ret != 0 ? ret : '=' - _comment[_tag_name.Length];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Opus_Tags_Get_Gain(OpusTags _tags, out c_int _gain_q8, string _tag_name)
		{
			int _tag_len = _tag_name.Length;
			CPointer<CPointer<byte>> comments = _tags.User_Comments;
			c_int ncomments = _tags.Comments;

			// Look for the first valid tag with the name _tag_name and use that
			for (c_int ci = 0; ci < ncomments; ci++)
			{
				if (Opus_Tagncompare(_tag_name, comments[ci]) == 0)
				{
					CPointer<byte> p = comments[ci] + _tag_len + 1;
					c_int negative = 0;

					if (p[0] == '-')
					{
						negative = -1;
						p++;
					}
					else if (p[0] == '+')
						p++;

					opus_int32 gain_q8 = 0;

					while ((p[0] >= '0') && (p[0] <= '9'))
					{
						gain_q8 = 10 * gain_q8 + p[0] - '0';
						if (gain_q8 > (32767 - negative))
							break;

						p++;
					}

					// This didn't look like a signed 16-bit decimal integer.
					// Not a valid gain tag
					if (p[0] != 0x00)
						continue;

					_gain_q8 = gain_q8 + negative ^ negative;

					return 0;
				}
			}

			_gain_q8 = 0;

			return (c_int)OpusFileError.False;
		}



		/********************************************************************/
		/// <summary>
		/// Get the album gain from an R128_ALBUM_GAIN tag, if one was
		/// specified. This searches for the first R128_ALBUM_GAIN tag with
		/// a valid signed, 16-bit decimal integer value and returns the
		/// value. This routine is exposed merely for convenience for
		/// applications which wish to do something special with the album
		/// gain (i.e., display it). If you simply wish to apply the album
		/// gain instead of the header gain, you can use
		/// op_set_gain_offset() with an OP_ALBUM_GAIN type and no offset
		/// </summary>
		/********************************************************************/
		public static c_int Opus_Tags_Get_Album_Gain(OpusTags _tags, out c_int _gain_q8)
		{
			return Opus_Tags_Get_Gain(_tags, out _gain_q8, "R128_ALBUM_GAIN");
		}



		/********************************************************************/
		/// <summary>
		/// Get the track gain from an R128_TRACK_GAIN tag, if one was
		/// specified. This searches for the first R128_TRACK_GAIN tag with
		/// a valid signed, 16-bit decimal integer value and returns the
		/// value. This routine is exposed merely for convenience for
		/// applications which wish to do something special with the track
		/// gain (i.e., display it). If you simply wish to apply the track
		/// gain instead of the header gain, you can use
		/// op_set_gain_offset() with an OP_TRACK_GAIN type and no offset
		/// </summary>
		/********************************************************************/
		public static c_int Opus_Tags_Get_Track_Gain(OpusTags _tags, out c_int _gain_q8)
		{
			return Opus_Tags_Get_Gain(_tags, out _gain_q8, "R128_TRACK_GAIN");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static bool Op_Is_Jpeg(CPointer<byte> _buf, size_t _buf_sz)
		{
			return (_buf_sz >= 3) && (CMemory.MemCmp(_buf, "\xFF\xD8\xFF", 3) == 0);
		}



		/********************************************************************/
		/// <summary>
		/// Tries to extract the width, height, bits per pixel, and palette
		/// size of a JPEG
		/// </summary>
		/********************************************************************/
		private static void Op_Extract_Jpeg_Params(CPointer<byte> _buf, size_t _buf_sz, ref opus_uint32 _width, ref opus_uint32 _height, ref opus_uint32 _depth, ref opus_uint32 _colors, ref c_int _has_palette)
		{
			if (Op_Is_Jpeg(_buf, _buf_sz))
			{
				size_t offs = 2;

				for (;;)
				{
					while ((offs < _buf_sz) && (_buf[(uint)offs] != 0xff))
						offs++;

					while ((offs < _buf_sz) && (_buf[(uint)offs] == 0xff))
						offs++;

					c_int marker = _buf[(uint)offs];
					offs++;

					// If we hit EOI* (end of image), or another SOI* (start of image),
					// or SOS (start of scan), then stop now
					if ((offs >= _buf_sz) || ((marker >= 0xd8) && (marker <= 0xda)))
						break;
					// RST* (restart markers): skip (no segment length)
					else if ((marker >= 0xd0) && (marker <= 0xd7))
						continue;

					// Read the length of the marker segment
					if ((_buf_sz - offs) < 2)
						break;

					size_t segment_len = (size_t)(_buf[(uint)offs] << 8 | _buf[(uint)offs + 1]);

					if ((segment_len < 2) || ((_buf_sz - offs) < segment_len))
						break;

					if ((marker == 0xc0) || ((marker > 0xc0) && (marker < 0xd0) && ((marker & 3) != 0)))
					{
						// Found a SOFn (start of frame) marker segment
						if (segment_len >= 8)
						{
							_height = (opus_uint32)(_buf[(uint)offs + 3] << 8 | _buf[(uint)offs + 4]);
							_width = (opus_uint32)(_buf[(uint)offs + 5] << 8 | _buf[(uint)offs + 6]);
							_depth = (opus_uint32)(_buf[(uint)offs + 2] * _buf[(uint)offs + 7]);
							_colors = 0;
							_has_palette = 0;
						}
						break;
					}

					// Other markers: skip the whole marker segment
					offs += segment_len;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static bool Op_Is_Png(CPointer<byte> _buf, size_t _buf_sz)
		{
			return (_buf_sz >= 8) && (CMemory.MemCmp(_buf, "\x89PNG\x0D\x0A\x1A\x0A", 8) == 0);
		}



		/********************************************************************/
		/// <summary>
		/// Tries to extract the width, height, bits per pixel, and palette
		/// size of a PNG
		/// </summary>
		/********************************************************************/
		private static void Op_Extract_Png_Params(CPointer<byte> _buf, size_t _buf_sz, ref opus_uint32 _width, ref opus_uint32 _height, ref opus_uint32 _depth, ref opus_uint32 _colors, ref c_int _has_palette)
		{
			if (Op_Is_Png(_buf, _buf_sz))
			{
				size_t offs = 8;

				while ((_buf_sz - offs) >= 12)
				{
					ogg_uint32_t chunk_len = Op_Parse_UInt32BE(_buf + (uint)offs);

					if (chunk_len > (_buf_sz - (offs + 12)))
						break;
					else if ((chunk_len == 13) && (CMemory.MemCmp(_buf + (uint)offs + 4, "IHDR", 4) == 0))
					{
						_width = Op_Parse_UInt32BE(_buf + (uint)offs + 8);
						_height = Op_Parse_UInt32BE(_buf + (uint)offs + 12);

						c_int color_type = _buf[(uint)offs + 17];
						if (color_type == 3)
						{
							_depth = 24;
							_has_palette = 1;
						}
						else
						{
							c_int sample_depth = _buf[(uint)offs + 16];

							if (color_type == 0)
								_depth = (opus_uint32)sample_depth;
							else if (color_type == 2)
								_depth = (opus_uint32)sample_depth * 3;
							else if (color_type == 4)
								_depth = (opus_uint32)sample_depth * 2;
							else if (color_type == 6)
								_depth = (opus_uint32)sample_depth * 4;

							_colors = 0;
							_has_palette = 0;
							break;
						}
					}
					else if ((_has_palette > 0) && (CMemory.MemCmp(_buf + (uint)offs + 4, "PLTE", 4) == 0))
					{
						_colors = chunk_len / 3;
						break;
					}

					offs += 12 + chunk_len;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static bool Op_Is_Gif(CPointer<byte> _buf, size_t _buf_sz)
		{
			return (_buf_sz >= 6) && ((CMemory.MemCmp(_buf, "GIF87a", 6) == 0) || (CMemory.MemCmp(_buf, "GIF89a", 6) == 0));
		}



		/********************************************************************/
		/// <summary>
		/// Tries to extract the width, height, bits per pixel, and palette
		/// size of a GIF
		/// </summary>
		/********************************************************************/
		private static void Op_Extract_Gif_Params(CPointer<byte> _buf, size_t _buf_sz, ref opus_uint32 _width, ref opus_uint32 _height, ref opus_uint32 _depth, ref opus_uint32 _colors, ref c_int _has_palette)
		{
			if (Op_Is_Gif(_buf, _buf_sz) && (_buf_sz >= 14))
			{
				_width = (opus_uint32)(_buf[6] | _buf[7] << 8);
				_height = (opus_uint32)(_buf[8] | _buf[9] << 8);

				// libFLAC hard-codes the depth to 24
				_depth = 24;
				_colors = 1U << ((_buf[10] & 7) + 1);
				_has_palette = 1;
			}
		}



		/********************************************************************/
		/// <summary>
		/// The actual implementation of opus_picture_tag_parse().
		/// Unlike the public API, this function requires _pic to already be
		/// initialized, modifies its contents before success is guaranteed,
		/// and assumes the caller will clear it on error
		/// </summary>
		/********************************************************************/
		private static c_int Opus_Picture_Tag_Parse_Impl(OpusPictureTag _pic, string _tag, CPointer<byte> _buf, size_t _buf_sz, size_t _base64_sz)
		{
			size_t i;

			// Decode the BASE64 data
			for (i = 0; i < _base64_sz; i++)
			{
				opus_uint32 value = 0;

				for (c_int j = 0; j < 4; j++)
				{
					c_uint d;
					c_uint c = _tag[4 * (int)i + j];

					if (c == '+')
						d = 62;
					else if (c == '/')
						d = 63;
					else if ((c >= '0') && (c <= '9'))
						d = 52 + c - '0';
					else if ((c >= 'a') && (c <= 'z'))
						d = 26 + c - 'a';
					else if ((c >= 'A') && (c <= 'Z'))
						d = c - 'A';
					else if ((c == '=') && ((size_t)(3 * (int)i + j) > _buf_sz))
						d = 0;
					else
						return (c_int)OpusFileError.NotFormat;

					value = value << 6 | d;
				}

				_buf[(uint)(3U * i)] = (byte)(value >> 16);

				if ((3 * i + 1) < _buf_sz)
				{
					_buf[(uint)(3U * i + 1)] = (byte)(value >> 8);

					if ((3 * i + 2) < _buf_sz)
						_buf[(uint)(3U * i + 2)] = (byte)value;
				}
			}

			i = 0;
			PictureType picture_type = (PictureType)Op_Parse_UInt32BE(_buf + (uint)i);
			i += 4;

			// Extract the MIME type
			opus_uint32 mime_type_length = Op_Parse_UInt32BE(_buf + (uint)i);
			i += 4;

			if (mime_type_length > (_buf_sz - 32))
				return (c_int)OpusFileError.NotFormat;

			CPointer<byte> mime_type = Memory.Ogg_MAlloc<byte>(mime_type_length + 1);
			if (mime_type.IsNull)
				return (c_int)OpusFileError.Fault;

			CMemory.MemCpy(mime_type, _buf + (uint)i, (int)mime_type_length);
			mime_type[mime_type_length] = 0x00;
			_pic.Mime_Type = mime_type;
			i += mime_type_length;

			// Extract the description string
			opus_uint32 description_length = Op_Parse_UInt32BE(_buf + (uint)i);
			i += 4;

			if (description_length > (_buf_sz - mime_type_length - 32))
				return (c_int)OpusFileError.NotFormat;

			CPointer<byte> description = Memory.Ogg_MAlloc<byte>(description_length + 1);
			if (description.IsNull)
				return (c_int)OpusFileError.Fault;

			CMemory.MemCpy(description, _buf + (uint)i, (int)description_length);
			description[description_length] = 0x00;
			_pic.Description = description;
			i += description_length;

			// Extract the remaining fields
			opus_uint32 width = Op_Parse_UInt32BE(_buf + (uint)i);
			i += 4;

			opus_uint32 height = Op_Parse_UInt32BE(_buf + (uint)i);
			i += 4;

			opus_uint32 depth = Op_Parse_UInt32BE(_buf + (uint)i);
			i += 4;

			opus_uint32 colors = Op_Parse_UInt32BE(_buf + (uint)i);
			i += 4;

			// If one of these is set, they all must be, but colors==0 is a valid value
			bool colors_set = (width != 0) || (height != 0) || (depth != 0) || (colors != 0);

			if (((width == 0) || (height == 0) || (depth == 0)) && colors_set)
				return (c_int)OpusFileError.NotFormat;

			opus_uint32 data_length = Op_Parse_UInt32BE(_buf + (uint)i);
			i += 4;

			if (data_length > (_buf_sz - i))
				return (c_int)OpusFileError.NotFormat;

			// Trim extraneous data so we don't copy it below
			_buf_sz = i + data_length;

			// Attempt to determine the image format
			PictureFormat format = PictureFormat.Unknown;

			if ((mime_type_length == 3) && (CMemory.StrCmp(mime_type, "-->") == 0))
			{
				format = PictureFormat.Url;

				// Picture type 1 must be a 32x32 PNG
				if ((picture_type == PictureType.File_Icon_Standard) && ((width != 0) || (height != 0)) && ((width != 32) || (height != 32)))
					return (c_int)OpusFileError.NotFormat;

				// Append a terminating NUL for the convenience of our callers
				_buf[(uint)_buf_sz++] = 0x00;
			}
			else
			{
				string mime = Encoding.ASCII.GetString(mime_type.Buffer, mime_type.Offset, mime_type.Length - 1).ToLower();

				if ((mime_type_length == 10) && (mime == "image/jpeg"))
				{
					if (Op_Is_Jpeg(_buf + (uint)i, data_length))
						format = PictureFormat.Jpeg;
				}
				else if ((mime_type_length == 9) && (mime == "image/png"))
				{
					if (Op_Is_Png(_buf + (uint)i, data_length))
						format = PictureFormat.Png;
				}
				else if ((mime_type_length == 9) && (mime == "image/gif"))
				{
					if (Op_Is_Gif(_buf + (uint)i, data_length))
						format = PictureFormat.Gif;
				}
				else if ((mime_type_length == 0) || ((mime_type_length == 6) && (mime == "image/")))
				{
					if (Op_Is_Jpeg(_buf + (uint)i, data_length))
						format = PictureFormat.Jpeg;
					else if (Op_Is_Png(_buf + (uint)i, data_length))
						format = PictureFormat.Png;
					else if (Op_Is_Gif(_buf + (uint)i, data_length))
						format = PictureFormat.Gif;
				}

				opus_uint32 file_width, file_heigth, file_depth, file_colors;
				file_width = file_heigth = file_depth = file_colors = 0;
				c_int has_palette = -1;

				switch (format)
				{
					case PictureFormat.Jpeg:
					{
						Op_Extract_Jpeg_Params(_buf + (uint)i, data_length, ref file_width, ref file_heigth, ref file_depth, ref file_colors, ref has_palette);
						break;
					}

					case PictureFormat.Png:
					{
						Op_Extract_Png_Params(_buf + (uint)i, data_length, ref file_width, ref file_heigth, ref file_depth, ref file_colors, ref has_palette);
						break;
					}

					case PictureFormat.Gif:
					{
						Op_Extract_Gif_Params(_buf + (uint)i, data_length, ref file_width, ref file_heigth, ref file_depth, ref file_colors, ref has_palette);
						break;
					}
				}

				if (has_palette >= 0)
				{
					// If we sucessfully extracted these parameters from the image, override
					// any declared values
					width = file_width;
					height = file_heigth;
					depth = file_depth;
					colors = file_colors;
				}

				// Picture type 1 must be a 32x32 PNG
				if ((picture_type == PictureType.File_Icon_Standard) && ((format != PictureFormat.Png) || (width != 32) || (height != 32)))
					return (c_int)OpusFileError.NotFormat;
			}

			// Adjust _buf_sz instead of using data_length to capture the terminating NULL
			// for URLs
			_buf_sz -= i;
			CMemory.MemMove(_buf, _buf + (uint)i, (int)_buf_sz);

			_buf = Memory.Ogg_Realloc(_buf, _buf_sz);
			if ((_buf_sz > 0) && _buf.IsNull)
				return (c_int)OpusFileError.Fault;

			_pic.Type = picture_type;
			_pic.Width = width;
			_pic.Height = height;
			_pic.Depth = depth;
			_pic.Colors = colors;
			_pic.Data_Length = data_length;
			_pic.Data = _buf;
			_pic.Format = format;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Parse a single METADATA_BLOCK_PICTURE tag.
		/// This decodes the BASE64-encoded content of the tag and returns a
		/// structure with the MIME type, description, image parameters (if
		/// known), and the compressed image data.
		/// If the MIME type indicates the presence of an image format we
		/// recognize (JPEG, PNG, or GIF) and the actual image data contains
		/// the magic signature associated with that format, then the
		/// OpusPictureTag::format field will be set to the corresponding
		/// format.
		/// This is provided as a convenience to avoid requiring
		/// applications to parse the MIME type and/or do their own format
		/// detection for the commonly used formats.
		/// In this case, we also attempt to extract the image parameters
		/// directly from the image data (overriding any that were present in
		/// the tag, which the specification says applications are not meant
		/// to rely on).
		/// The application must still provide its own support for actually
		/// decoding the image data and, if applicable, retrieving that data
		/// from URLs
		/// </summary>
		/********************************************************************/
		public static c_int Opus_Picture_Tag_Parse(out OpusPictureTag _pic, string _tag)
		{
			_pic = null;

			if (_tag.StartsWith("METADATA_BLOCK_PICTURE="))
				_tag = _tag.Substring(23);

			// Figure out how much BASE64-encoded data we have
			size_t tag_length = (size_t)_tag.Length;

			if ((tag_length & 3) != 0)
				return (c_int)OpusFileError.NotFormat;

			size_t base64_sz = tag_length >> 2;
			size_t buf_sz = 3 * base64_sz;

			if (buf_sz < 32)
				return (c_int)OpusFileError.NotFormat;

			if (_tag[(int)tag_length - 1] == '=')
				buf_sz--;

			if (_tag[(int)tag_length - 2] == '=')
				buf_sz--;

			if (buf_sz < 32)
				return (c_int)OpusFileError.NotFormat;

			// Allocate an extra byte to allow appending a terminating NUL to URL data
			CPointer<byte> buf = CMemory.MAlloc<byte>((int)buf_sz + 1);
			if (buf.IsNull)
				return (c_int)OpusFileError.Fault;

			Opus_Picture_Tag_Init(out OpusPictureTag pic);

			c_int ret = Opus_Picture_Tag_Parse_Impl(pic, _tag, buf, buf_sz, base64_sz);
			if (ret < 0)
			{
				Opus_Picture_Tag_Clear(pic);
				Memory.Ogg_Free(buf);
			}
			else
				_pic = pic;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Initializes an #OpusPictureTag structure.
		/// This should be called on a freshly allocated #OpusPictureTag
		/// structure before attempting to use it
		/// </summary>
		/********************************************************************/
		public static void Opus_Picture_Tag_Init(out OpusPictureTag _pic)
		{
			_pic = new OpusPictureTag();
		}



		/********************************************************************/
		/// <summary>
		/// Initializes an #OpusPictureTag structure.
		/// This should be called on a freshly allocated #OpusPictureTag
		/// structure before attempting to use it
		/// </summary>
		/********************************************************************/
		public static void Opus_Picture_Tag_Clear(OpusPictureTag _pic)
		{
			Memory.Ogg_Free(_pic.Description);
			Memory.Ogg_Free(_pic.Mime_Type);
			Memory.Ogg_Free(_pic.Data);
		}
	}
}
