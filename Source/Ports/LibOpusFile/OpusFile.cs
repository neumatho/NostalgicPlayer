/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOgg;
using Polycode.NostalgicPlayer.Ports.LibOgg.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpus;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;
using Polycode.NostalgicPlayer.Ports.OpusFile.Containers;

namespace Polycode.NostalgicPlayer.Ports.OpusFile
{
	/// <summary>
	/// 
	/// </summary>
	public class OpusFile
	{
		/// <summary>
		/// 
		/// </summary>
		public delegate c_int Op_Decode_Cb_Func(object _ctx, OpusDecoder _decoder, Array _pcm, Ogg_Packet _op, c_int _nsamples, c_int _nchannels, DecodeFormat _format, c_int _li);

		/// <summary>
		/// The maximum number of bytes in a page (including page headers)
		/// </summary>
		private const int Op_Page_Size_Max = 65307;

		/// <summary>
		/// The default amount to seek backwards per step when trying to find the
		/// previous page.
		/// This must be at least as large as the maximum size of a page
		/// </summary>
		private const int Op_Chunk_Size = 65536;

		/// <summary>
		/// The maximum amount to seek backwards per step when trying to find the
		/// previous page
		/// </summary>
		private const int Op_Chunk_Size_Max = 1024 * 1024;

		/// <summary>
		/// A smaller read size is needed for low-rate streaming
		/// </summary>
		private const int Op_Read_Size = 2048;

		/// <summary>
		/// The minimum granule position spacing allowed for making predictions.
		/// This corresponds to about 1 second of audio at 48 kHz for both Opus and
		/// Vorbis, or one keyframe interval in Theora with the default keyframe spacing
		/// of 256
		/// </summary>
		private const int Op_Gp_Spacing_Min = 48000;

		/// <summary>
		/// This controls how close the target has to be to use the current stream
		/// position to subdivide the initial range.
		/// Two minutes seems to be a good default
		/// </summary>
		private const int Op_Cur_Time_Thresh = 120 * 48 * 1000;

		private OggOpusFile _of;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private OpusFile(OggOpusFile of)
		{
			_of = of;
		}



		/********************************************************************/
		/// <summary>
		/// Test to see if this is an Opus stream.
		/// For good results, you will need at least 57 bytes (for a pure
		/// Opus-only stream).
		/// Something like 512 bytes will give more reliable results for
		/// multiplexed streams.
		/// This function is meant to be a quick-rejection filter.
		///
		/// Its purpose is not to guarantee that a stream is a valid Opus
		/// stream, but to ensure that it looks enough like Opus that it
		/// isn't going to be recognized as some other format (except
		/// possibly an Opus stream that is also multiplexed with other
		/// codecs, such as video)
		/// </summary>
		/********************************************************************/
		public static OpusFileError Op_Test(OpusHead _head, Pointer<byte> _initial_data, size_t _initial_bytes)
		{
			// The first page of a normal Opus file will be at most 57 bytes (27 Ogg
			// page header bytes + 1 lacing value + 21 Opus header bytes + 8 channel
			// mapping bytes).
			//
			// It will be at least 47 bytes (27 Ogg page header bytes + 1 lacing value +
			// 19 Opus header bytes using channel mapping family 0).
			// If we don't have at least that much data, give up now
			if (_initial_bytes < 47)
				return OpusFileError.False;

			// Only proceed if we start with the magic OggS string.
			// This is to prevent us spending a lot of time allocating memory and looking
			// for Ogg pages in non-Ogg files
			if (CMemory.MemCmp(_initial_data, "OggS", 4) != 0)
				return OpusFileError.NotFormat;

			if (_initial_bytes > long.MaxValue)
				return OpusFileError.Fault;

			OpusFileError err;
			OggSync.Init(out OggSync oy);
			Pointer<byte> data = oy.Buffer((c_long)_initial_bytes);

			if (!data.IsNull)
			{
				CMemory.MemCpy(data, _initial_data, (int)_initial_bytes);
				oy.Wrote((c_long)_initial_bytes);

				OggStream.Init(out OggStream os, -1);
				err = OpusFileError.False;

				do
				{
					c_int ret = oy.PageOut(out OggPage og);

					// Ignore holes
					if (ret < 0)
						continue;

					// Stop if we run out of data
					if (ret == 0)
						break;

					os.Reset_SerialNo(og.SerialNo());
					os.PageIn(og);

					// Only process the first packet on this page (if it's a BOS packet,
					// it's required to be the only one)
					if (os.PacketOut(out Ogg_Packet op) == 1)
					{
						if (op.Bos)
						{
							OpusFileError ret1 = Info.Opus_Head_Parse(_head, op.Packet, (size_t)op.Bytes);

							// If this didn't look like Opus, keep going
							if (ret1 == OpusFileError.NotFormat)
								continue;

							// Otherwise we're done, one way or another
							err = ret1;
						}
						else
						{
							// We finished parsing the headers.
							// There is no Opus to be found
							err = OpusFileError.NotFormat;
						}
					}
				}
				while (err == OpusFileError.False);

				os.Clear();
			}
			else
				err = OpusFileError.Fault;

			oy.Clear();

			return err;
		}



		/********************************************************************/
		/// <summary>
		/// Read a little more data from the file/pipe into the ogg_sync
		/// framer
		/// </summary>
		/********************************************************************/
		private c_int Op_Get_Data(c_int _nbytes)
		{
			Pointer<byte> buffer = _of.oy.Buffer(_nbytes);
			c_int nbytes = _of.callbacks.Read(_of.stream, buffer, _nbytes);

			if (nbytes > 0)
				_of.oy.Wrote(nbytes);

			return nbytes;
		}



		/********************************************************************/
		/// <summary>
		/// Save a tiny smidge of verbosity to make the code more readable
		/// </summary>
		/********************************************************************/
		private c_int Op_Seek_Helper(opus_int64 _offset)
		{
			if (_offset == _of.offset)
				return 0;

			if ((_of.callbacks.Seek == null) || (_of.callbacks.Seek(_of.stream, _offset, SeekOrigin.Begin) != 0))
				return (c_int)OpusFileError.Read;

			_of.offset = _offset;
			_of.oy.Reset();

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Get the current position indicator of the underlying stream.
		/// This should be the same as the value reported by tell()
		/// </summary>
		/********************************************************************/
		private opus_int64 Op_Position()
		{
			// The current position indicator is _not_ simply offset.
			// We may also have unprocessed, buffered data in the sync state
			return _of.offset + _of.oy.State.Fill - _of.oy.State.Returned;
		}



		/********************************************************************/
		/// <summary>
		/// From the head of the stream, get the next page.
		/// _boundary specifies if the function is allowed to fetch more data
		/// from the stream (and how much) or only use internally buffered
		/// data
		/// </summary>
		/********************************************************************/
		private opus_int64 Op_Get_Next_Page(ref OggPage _og, opus_int64 _boundary)
		{
			while ((_boundary <= 0) || (_of.offset < _boundary))
			{
				c_int more = _of.oy.PageSeek(out _og);

				// Skipped (-more) bytes
				if (more < 0)
					_of.offset -= more;
				else if (more == 0)
				{
					// Send more paramedics
					if (_boundary == 0)
						return (opus_int64)OpusFileError.False;

					c_int read_nbytes;

					if (_boundary < 0)
						read_nbytes = Op_Read_Size;
					else
					{
						opus_int64 position = Op_Position();
						if (position >= _boundary)
							return (opus_int64)OpusFileError.False;

						read_nbytes = Internal.Op_Min((c_int)(_boundary - position), Op_Read_Size);
					}

					c_int ret = Op_Get_Data(read_nbytes);
					if (ret < 0)
						return (opus_int64)OpusFileError.Read;

					if (ret == 0)
					{
						// Only fail cleanly on EOF if we didn't have a known boundary.
						// Otherwise, we should have been able to reach that boundary, and this
						// is a fatal error
						return (opus_int64)(_boundary < 0 ? OpusFileError.False : OpusFileError.BadLink);
					}
				}
				else
				{
					// Got a page.
					// Return the page start offset and advance the internal offset past the
					// page end
					opus_int64 page_offset = _of.offset;
					_of.offset += more;

					return page_offset;
				}
			}

			return (opus_int64)OpusFileError.False;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Op_Add_Serialno(OggPage _og, ref Pointer<ogg_uint32_t> _serialnos, ref c_int _nserialnos, ref c_int _cserialnos)
		{
			ogg_uint32_t s = (ogg_uint32_t)_og.SerialNo();

			Pointer<ogg_uint32_t> serialnos = _serialnos;
			c_int nserialnos = _nserialnos;
			c_int cserialnos = _cserialnos;

			if (nserialnos >= cserialnos)
			{
				if (cserialnos > (c_int.MaxValue / sizeof(ogg_uint32_t) - 1 >> 1))
					return (c_int)OpusFileError.Fault;

				cserialnos = 2 * cserialnos + 1;
				serialnos = Memory.Ogg_Realloc(serialnos, (size_t)cserialnos);
				if (serialnos.IsNull)
					return (c_int)OpusFileError.Fault;
			}

			serialnos[nserialnos++] = s;

			_serialnos = serialnos;
			_nserialnos = nserialnos;
			_cserialnos = cserialnos;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Returns nonzero if found
		/// </summary>
		/********************************************************************/
		private c_int Op_Lookup_Serialno(ogg_uint32_t _s, Pointer<ogg_uint32_t> _serialnos, c_int _nserialnos)
		{
			c_int i;

			for (i = 0; (i < _nserialnos) && (_serialnos[i] != _s); i++)
			{
			}

			return i < _nserialnos ? 1 : 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Op_Lookup_Page_Serialno(OggPage _og, Pointer<ogg_uint32_t> _serialnos, c_int _nserialnos)
		{
			return Op_Lookup_Serialno((ogg_uint32_t)_og.SerialNo(), _serialnos, _nserialnos);
		}



		/********************************************************************/
		/// <summary>
		/// Find the last page beginning before _offset with a valid granule
		/// position. There is no '_boundary' parameter as it will always
		/// have to read more data. This is much dirtier than the above, as
		/// Ogg doesn't have any backward search linkage.
		/// This search prefers pages of the specified serial number.
		/// If a page of the specified serial number is spotted during the
		/// seek-back-and-read-forward, it will return the info of last page
		/// of the matching serial number, instead of the very last page,
		/// unless the very last page belongs to a different link than
		/// preferred serial number.
		/// If no page of the specified serial number is seen, it will return
		/// the info of the last page
		/// </summary>
		/********************************************************************/
		private c_int Op_Get_Prev_Page_Serial(out OpusSeekRecord _sr, opus_int64 _offset, ogg_uint32_t _serialno, Pointer<ogg_uint32_t> _serialnos, c_int _nserialnos)
		{
			_sr = null;

			OpusSeekRecord preferred_sr = null;
			OggPage og = null;
			opus_int64 begin;
			opus_int64 end;
			opus_int64 original_end;

			original_end = end = begin = _offset;
			bool preferred_found = false;
			_offset = -1;
			opus_int32 chunk_size = Op_Chunk_Size;

			do
			{
				begin = Internal.Op_Max(begin - chunk_size, 0);

				c_int ret = Op_Seek_Helper(begin);
				if (ret < 0)
					return ret;

				opus_int64 search_start = begin;

				while (_of.offset < end)
				{
					opus_int64 llret = Op_Get_Next_Page(ref og, end);
					if (llret < (opus_int64)OpusFileError.False)
						return (c_int)llret;
					else if (llret == (opus_int64)OpusFileError.False)
						break;

					ogg_uint32_t serialno = (ogg_uint32_t)og.SerialNo();

					// Save the information for this page.
					// We're not interested in the page itself... just the serial number, byte
					// offset, page size, and granule position
					_sr = new OpusSeekRecord();

					_sr.search_start = search_start;
					_sr.offset = _offset = llret;
					_sr.serialno = serialno;
					_sr.size = (opus_int32)(_of.offset - _offset);
					_sr.gp = og.GranulePos();

					// If this page is from the stream we're looking for, remember it
					if (serialno == _serialno)
					{
						preferred_found = true;
						preferred_sr = _sr;
					}

					if (Op_Lookup_Serialno(serialno, _serialnos, _nserialnos) == 0)
					{
						// We fell off the end of the link, which means we seeked back too far
						// and shouldn't have been looking in that link to begin with.
						// If we found the preferred serial number, forget that we saw it
						preferred_found = false;
					}

					search_start = llret + 1;
				}

				// We started from the beginning of the stream and found nothing.
				// This should be impossible unless the contents of the stream changed out
				// from under us after we read from it
				if ((begin == 0) && (_offset < 0))
					return (c_int)OpusFileError.BadLink;

				// Bump up the chunk size.
				// This is mildly helpful when seeks are very expensive (http)
				chunk_size = Internal.Op_Min(2 * chunk_size, Op_Chunk_Size_Max);

				// Avoid quadratic complexity if we hit an invalid patch of the file
				end = Internal.Op_Min(begin + Op_Page_Size_Max - 1, original_end);
			}
			while (_offset < 0);

			if (preferred_found)
				_sr = preferred_sr;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Find the last page beginning before _offset with the given serial
		/// number and a valid granule position.
		/// Unlike the above search, this continues until it finds such a
		/// page, but does not stray outside the current link.
		/// We could implement it (inefficiently) by calling
		/// op_get_prev_page_serial() repeatedly until it returned a page
		/// that had both our preferred serial number and a valid granule
		/// position, but doing it with a separate function allows us to
		/// avoid repeatedly re-scanning valid pages from other streams as
		/// we seek-back-and-read-forward
		/// </summary>
		/********************************************************************/
		private opus_int64 Op_Get_Last_Page(out ogg_int64_t _gp, opus_int64 _offset, ogg_uint32_t _serialno, Pointer<ogg_uint32_t> _serialnos, c_int _nserialnos)
		{
			_gp = 0;

			OggPage og = null;
			opus_int64 begin;
			opus_int64 end;
			opus_int64 original_end;

			original_end = end = begin = _offset;
			_offset = -1;

			ogg_int64_t gp = -1;
			opus_int32 chunk_size = Op_Chunk_Size;

			do
			{
				begin = Internal.Op_Max(begin - chunk_size, 0);

				c_int ret = Op_Seek_Helper(begin);
				if (ret < 0)
					return ret;

				c_int left_link = 0;

				while (_of.offset < end)
				{
					opus_int64 llret = Op_Get_Next_Page(ref og, end);
					if (llret < (opus_int64)OpusFileError.False)
						return llret;
					else if (llret == (opus_int64)OpusFileError.False)
						break;

					ogg_uint32_t serialno = (ogg_uint32_t)og.SerialNo();

					if (serialno == _serialno)
					{
						// The page is from the right stream
						ogg_int64_t page_gp = og.GranulePos();

						if (page_gp != -1)
						{
							// And has a valid granule position.
							// Let's remember it
							_offset = llret;
							gp = page_gp;
						}
					}
					else if (Op_Lookup_Serialno(serialno, _serialnos, _nserialnos) == 0)
					{
						// We fell off the start of the link, which means we don't need to keep
						// seeking any farther back
						left_link = 1;
					}
				}

				// We started from at or before the beginning of the link and found nothing.
				// This should be impossible unless the contents of the stream changed out
				// from under us after we read from it
				if (((left_link != 0) || (begin == 0)) && (_offset < 0))
					return (opus_int64)OpusFileError.BadLink;

				// Bump up the chunk size.
				// This is mildly helpful when seeks are very expensive (http)
				chunk_size = Internal.Op_Min(2 * chunk_size, Op_Chunk_Size_Max);

				// Avoid quadratic complexity if we hit an invalid patch of the file
				end = Internal.Op_Min(begin + Op_Page_Size_Max - 1, original_end);
			}
			while (_offset < 0);

			_gp = gp;

			return _offset;
		}



		/********************************************************************/
		/// <summary>
		/// Uses the local ogg_stream storage in _of.
		/// This is important for non-streaming input sources
		/// </summary>
		/********************************************************************/
		private c_int Op_Fetch_Headers_Impl(OpusHead _head, OpusTags _tags, bool hasSerialnos, ref Pointer<ogg_uint32_t> _serialnos, ref c_int _nserialnos, ref c_int _cserialnos, ref OggPage _og)
		{
			c_int ret;

			if (hasSerialnos)
				_nserialnos = 0;

			// Extract the serialnos of all BOS pages plus the first set of Opus headers
			// we see in the link
			while (_og.Bos())
			{
				if (hasSerialnos)
				{
					if (Op_Lookup_Page_Serialno(_og, _serialnos, _nserialnos) != 0)
					{
						// A dupe serialnumber in an initial header packet set==invalid stream
						return (c_int)OpusFileError.BadHeader;
					}

					ret = Op_Add_Serialno(_og, ref _serialnos, ref _nserialnos, ref _cserialnos);
					if (ret < 0)
						return ret;
				}

				if (_of.ready_state < State.StreamSet)
				{
					// We don't have an Opus stream in this link yet, so begin prospective
					// stream setup.
					// We need a stream to get packets
					_of.os.Reset_SerialNo(_og.SerialNo());
					_of.os.PageIn(_og);

					if (_of.os.PacketOut(out Ogg_Packet op) > 0)
					{
						ret = (c_int)Info.Opus_Head_Parse(_head, op.Packet, (size_t)op.Bytes);

						// Found a valid Opus header
						// Continue setup
						if (ret >= 0)
							_of.ready_state = State.StreamSet;
						// If it's just a stream type we don't recognize, ignore it.
						// Everything else is fatal
						else if (ret != (c_int)OpusFileError.NotFormat)
							return ret;
					}
				}

				// Get the next page.
				// No need to clamp the boundary offset against _of->end, as all errors
				// become OP_ENOTFORMAT or OP_EBADHEADER
				if (Op_Get_Next_Page(ref _og, Internal.Op_Adv_Offset(_of.offset, Op_Chunk_Size)) < 0)
					return (c_int)(_of.ready_state < State.StreamSet ? OpusFileError.NotFormat : OpusFileError.BadHeader);
			}

			if (_of.ready_state != State.StreamSet)
				return (c_int)OpusFileError.NotFormat;

			// If the first non-header page belonged to our Opus stream, submit it
			if (_of.os.State.SerialNo == _og.SerialNo())
				_of.os.PageIn(_og);

			// Loop getting packets
			for (;;)
			{
				switch (_of.os.PacketOut(out Ogg_Packet op))
				{
					case 0:
					{
						// Loop getting pages
						for (;;)
						{
							// No need to clamp the boundary offset against _of->end, as all
							// errors become OP_EBADHEADER
							if (Op_Get_Next_Page(ref _og, Internal.Op_Adv_Offset(_of.offset, Op_Chunk_Size)) < 0)
								return (c_int)OpusFileError.BadHeader;

							// If this page belongs to the correct stream, go parse it
							if (_of.os.State.SerialNo == _og.SerialNo())
							{
								_of.os.PageIn(_og);
								break;
							}

							// If the link ends before we see the Opus comment header, abort
							if (_og.Bos())
								return (c_int)OpusFileError.BadHeader;

							// Otherwise, keep looking
						}
						break;
					}

					// We shouldn't get a hole in the headers
					case -1:
						return (c_int)OpusFileError.BadHeader;

					default:
					{
						// Got a packet.
						// It should be the comment header
						ret = Info.Opus_Tags_Parse(_tags, op.Packet, (size_t)op.Bytes);
						if (ret < 0)
							return ret;

						// Make sure the page terminated at the end of the comment header.
						// If there is another packet on the page, or part of a packet, then
						// reject the stream.
						// Otherwise seekable sources won't be able to seek back to the start
						// properly
						ret = _of.os.PacketOut(out op);
						if ((ret != 0) || (_og.Page.Header[_og.Page.HeaderLen - 1] == 255))
						{
							// If we fail, the caller assumes our tags are uninitialized
							Info.Opus_Tags_Clear(_tags);

							return (c_int)OpusFileError.BadHeader;
						}

						return 0;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Op_Fetch_Headers(OpusHead _head, OpusTags _tags, OggPage _og)
		{
			Pointer<ogg_uint32_t> serialnos = new Pointer<ogg_uint32_t>();
			c_int nserialnos = 0;
			c_int cserialnos = 0;

			return Op_Fetch_Headers(_head, _tags, false, ref serialnos, ref nserialnos, ref cserialnos, _og);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Op_Fetch_Headers(OpusHead _head, OpusTags _tags, ref Pointer<ogg_uint32_t> _serialnos, ref c_int _nserialnos, ref c_int _cserialnos, OggPage _og)
		{
			return Op_Fetch_Headers(_head, _tags, true, ref _serialnos, ref _nserialnos, ref _cserialnos, _og);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Op_Fetch_Headers(OpusHead _head, OpusTags _tags, bool hasSerialnos, ref Pointer<ogg_uint32_t> _serialnos, ref c_int _nserialnos, ref c_int _cserialnos, OggPage _og)
		{
			if (_og == null)
			{
				OggPage og = null;

				// No need to clamp the boundary offset against _of->end, as all errors
				// become OP_ENOTFORMAT
				if (Op_Get_Next_Page(ref og, Internal.Op_Adv_Offset(_of.offset, Op_Chunk_Size)) < 0)
					return (c_int)OpusFileError.NotFormat;

				_og = og;
			}

			_of.ready_state = State.Opened;

			c_int ret = Op_Fetch_Headers_Impl(_head, _tags, hasSerialnos, ref _serialnos, ref _nserialnos, ref _cserialnos, ref _og);

			// Revert back from OP_STREAMSET to OP_OPENED on failure, to prevent
			// double-free of the tags in an unseekable stream
			if (ret < 0)
				_of.ready_state = State.Opened;

			return ret;
		}

		// Granule position manipulation routines.
		//
		// A granule position is defined to be an unsigned 64-bit integer, with the
		// special value -1 in two's complement indicating an unset or invalid granule
		// position.
		// We are not guaranteed to have an unsigned 64-bit type, so we construct the
		// following routines that
		//  a) Properly order negative numbers as larger than positive numbers, and
		//  b) Check for underflow or overflow past the special -1 value.
		// This lets us operate on the full, valid range of granule positions in a
		// consistent and safe manner.
		// This full range is organized into distinct regions:
		//  [ -1 (invalid) ][ 0 ... OP_INT64_MAX ][ OP_INT64_MIN ... -2 ][-1 (invalid) ]
		//
		// No one should actually use granule positions so large that they're negative,
		// even if they are technically valid, as very little software handles them
		// correctly (including most of Xiph.Org's).
		// This library also refuses to support durations so large they won't fit in a
		// signed 64-bit integer (to avoid exposing this mess to the application, and
		// to simplify a good deal of internal arithmetic), so the only way to use them
		// successfully is if pcm_start is very large.
		// This means there isn't anything you can do with negative granule positions
		// that you couldn't have done with purely non-negative ones.
		// The main purpose of these routines is to allow us to think very explicitly
		// about the possible failure cases of all granule position manipulations

		/********************************************************************/
		/// <summary>
		/// Safely adds a small signed integer to a valid (not -1) granule
		/// position. The result can use the full 64-bit range of values
		/// (both positive and negative), but will fail on overflow
		/// (wrapping past -1; wrapping past OP_INT64_MAX is explicitly okay)
		/// </summary>
		/********************************************************************/
		private c_int Op_Granpos_Add(out ogg_int64_t _dst_gp, ogg_int64_t _src_gp, opus_int32 _delta)
		{
			_dst_gp = 0;

			if (_delta > 0)
			{
				// Adding this amount to the granule position would overflow its 64-bit
				// range
				if ((_src_gp < 0) && (_src_gp >= (-1 - _delta)))
					return (c_int)OpusFileError.Inval;

				if (_src_gp > Internal.Op_Int64_Max - _delta)
				{
					// Adding this amount to the granule position would overflow the positive
					// half of its 64-bit range.
					// Since signed overflow is undefined in C, do it in a way the compiler
					// isn't allowed to screw up
					_delta -= (opus_int32)(Internal.Op_Int64_Max - _src_gp) + 1;
					_src_gp = Internal.Op_Int64_Min;
				}
			}
			else if (_delta < 0)
			{
				// Subtracting this amount from the granule position would underflow its
				// 64-bit range
				if ((_src_gp >= 0) && (_src_gp < -_delta))
					return (c_int)OpusFileError.Inval;

				if (_src_gp < (Internal.Op_Int64_Min - _delta))
				{
					// Subtracting this amount from the granule position would underflow the
					// negative half of its 64-bit range.
					// Since signed underflow is undefined in C, do it in a way the compiler
					// isn't allowed to screw up
					_delta += (opus_int32)(_src_gp - Internal.Op_Int64_Min) + 1;
					_src_gp = Internal.Op_Int64_Max;
				}
			}

			_dst_gp = _src_gp + _delta;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Safely computes the difference between two granule positions.
		/// The difference must fit in a signed 64-bit integer, or the
		/// function fails. It correctly handles the case where the granule
		/// position has wrapped around from positive values to negative ones
		/// </summary>
		/********************************************************************/
		private c_int Op_Granpos_Diff(out ogg_int64_t _delta, ogg_int64_t _gp_a, ogg_int64_t _gp_b)
		{
			bool gp_a_negative = _gp_a < 0;
			bool gp_b_negative = _gp_b < 0;

			if (gp_a_negative ^ gp_b_negative)
			{
				ogg_int64_t da;
				ogg_int64_t db;

				_delta = 0;

				if (gp_a_negative)
				{
					// _gp_a has wrapped to a negative value but _gp_b hasn't: the difference
					// should be positive.
					//
					// Step 1: Handle wrapping
					// _gp_a < 0 => da < 0
					da = (Internal.Op_Int64_Min - _gp_a) - 1;

					// _gp_b >= 0  => db >= 0
					db = Internal.Op_Int64_Max - _gp_b;

					// Step 2: Check for overflow
					if ((Internal.Op_Int64_Max + da) < db)
						return (c_int)OpusFileError.Inval;

					_delta = db - da;
				}
				else
				{
					// _gp_b has wrapped to a negative value but _gp_a hasn't: the difference
					// should be negative.
					//
					// Step 1: Handle wrapping
					// _gp_a >= 0 => da <= 0
					da = _gp_a + Internal.Op_Int64_Min;

					// _gp_b < 0  => db <= 0
					db = Internal.Op_Int64_Min - _gp_b;

					// Step 2: Check for overflow
					if (da < (Internal.Op_Int64_Min - db))
						return (c_int)OpusFileError.Inval;

					_delta = da + db;
				}
			}
			else
				_delta = _gp_a - _gp_b;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Op_Granpos_Cmp(ogg_int64_t _gp_a, ogg_int64_t _gp_b)
		{
			// Handle the wrapping cases
			if (_gp_a < 0)
			{
				if (_gp_b >= 0)
					return 1;

				// Else fall through
			}
			else if (_gp_b < 0)
				return -1;

			return (_gp_a > _gp_b ? 1 : 0) - (_gp_b > _gp_a ? 1 : 0);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the duration of the packet (in samples at 48 kHz), or a
		/// negative value on error
		/// </summary>
		/********************************************************************/
		private c_int Op_Get_Packet_Duration(Pointer<byte> _data, c_int _len)
		{
			c_int nframes = OpusDecoder.Packet_Get_Nb_Frames(_data, _len);
			if (nframes < 0)
				return (c_int)OpusFileError.BadPacket;

			c_int frame_size = OpusDecoder.Packet_Get_Samples_Per_Frame(_data, 48000);
			c_int nsamples = nframes * frame_size;

			if (nsamples > (120 * 48))
				return (c_int)OpusFileError.BadPacket;

			return nsamples;
		}



		/********************************************************************/
		/// <summary>
		/// Grab all the packets currently in the stream state, and compute
		/// their durations.
		/// _of.op_count is set to the number of packets collected
		/// </summary>
		/********************************************************************/
		private opus_int32 Op_Collect_Audio_Packets(Pointer<c_int> _durations)
		{
			// Count the durations of all packets in the page
			c_int op_count = 0;
			opus_int32 total_duration = 0;

			for (;;)
			{
				// This takes advantage of undocumented libogg behavior that returned
				// ogg_packet buffers are valid at least until the next page is
				// submitted.
				// Relying on this is not too terrible, as _none_ of the Ogg memory
				// ownership/lifetime rules are well-documented.
				// But I can read its code and know this will work
				c_int ret = _of.os.PacketOut(out _of.op[op_count]);
				if (ret == 0)
					break;

				if (ret < 0)
				{
					// Set the return value and break out of the loop.
					// We want to make sure op_count gets set to 0, because we've ingested a
					// page, so any previously loaded packets are now invalid
					total_duration = (opus_int32)OpusFileError.Hole;
					break;
				}

				_durations[op_count] = Op_Get_Packet_Duration(_of.op[op_count].Packet, _of.op[op_count].Bytes);
				if (_durations[op_count] > 0)
				{
					// With at most 255 packets on a page, this can't overflow
					total_duration += _durations[op_count++];
				}
				// Ignore packets with an invalid TOC sequence
				else if (op_count > 0)
				{
					// But save the granule position, if there was one
					_of.op[op_count - 1].GranulePos = _of.op[op_count].GranulePos;
				}
			}

			_of.op_pos = 0;
			_of.op_count = op_count;

			return total_duration;
		}



		/********************************************************************/
		/// <summary>
		/// Starting from current cursor position, get the initial PCM offset
		/// of the next page.
		/// This also validates the granule position on the first page with a
		/// completed audio data packet, as required by the spec.
		/// If this link is completely empty (no pages with completed
		/// packets), then this function sets pcm_start=pcm_end=0 and returns
		/// the BOS page of the next link (if any).
		/// In the seekable case, we initialize pcm_end=-1 before calling
		/// this function, so that later we can detect that the link was
		/// empty before calling op_find_final_pcm_offset()
		/// </summary>
		/********************************************************************/
		private c_int Op_Find_Initial_Pcm_Offset(OggOpusLink _link, out OggPage _og)
		{
			_og = null;

			opus_int64 page_offset;
			c_int[] durations = new c_int[255];

			ogg_uint32_t serialno = (ogg_uint32_t)_of.os.State.SerialNo;
			c_int op_count = 0;
			opus_int32 total_duration = 0;

			do
			{
				page_offset = Op_Get_Next_Page(ref _og, _of.end);

				// We should get a page unless the file is truncated or mangled.
				// Otherwise there are no audio data packets in the whole logical stream
				if (page_offset < 0)
				{
					// Fail if there was a read error
					if (page_offset < (opus_int64)OpusFileError.False)
						return (c_int)page_offset;

					// Fail if the pre-skip is non-zero, since it's asking us to skip more
					// samples than exists
					if (_link.head.Pre_Skip > 0)
						return (c_int)OpusFileError.BadTimestamp;

					_link.pcm_file_offset = 0;

					// Set pcm_end and end_offset so we can skip the call to
					// op_find_final_pcm_offset()
					_link.pcm_start = _link.pcm_end = 0;
					_link.end_offset = _link.data_offset;

					return 0;
				}

				// Similarly, if we hit the next link in the chain, we've gone too far
				if (_og.Bos())
				{
					if (_link.head.Pre_Skip > 0)
						return (c_int)OpusFileError.BadTimestamp;

					// Set pcm_end and end_offset so we can skip the call to
					// op_find_final_pcm_offset()
					_link.pcm_file_offset = 0;
					_link.pcm_start = _link.pcm_end = 0;
					_link.end_offset = _link.data_offset;

					// Tell the caller we've got a buffered page for them
					return 1;
				}

				// Ignore pages from other streams (not strictly necessary, because of the
				// checks in ogg_stream_pagein(), but saves some work)
				if (serialno != (ogg_uint32_t)_og.SerialNo())
					continue;

				_of.os.PageIn(_og);

				// Bitrate tracking: add the header's bytes here.
				// The body bytes are counted when we consume the packets
				_of.bytes_tracked += _og.Page.HeaderLen;

				// Count the durations of all packets in the page
				do
				{
					total_duration = Op_Collect_Audio_Packets(durations);
				}
				// Ignore holes
				while (total_duration < 0);

				op_count = _of.op_count;
			}
			while (op_count <= 0);

			// We found the first page with a completed audio data packet: actually look
			// at the granule position.
			// RFC 3533 says, "A special value of -1 (in two's complement) indicates that
			// no packets finish on this page," which does not say that a granule
			// position that is NOT -1 indicates that some packets DO finish on that page
			// (even though this was the intention, libogg itself violated this intention
			// for years before we fixed it).
			// The Ogg Opus specification only imposes its start-time requirements
			// on the granule position of the first page with completed packets,
			// so we ignore any set granule positions until then
			ogg_int64_t cur_page_gp = _of.op[op_count - 1].GranulePos;

			// But getting a packet without a valid granule position on the page is not
			// okay
			if (cur_page_gp == -1)
				return (c_int)OpusFileError.BadTimestamp;

			ogg_int64_t pcm_start;
			bool cur_page_eos = _of.op[op_count - 1].Eos;

			if (!cur_page_eos)
			{
				// The EOS flag wasn't set.
				// Work backwards from the provided granule position to get the starting PCM
				// offset
				if (Op_Granpos_Add(out pcm_start, cur_page_gp, -total_duration) < 0)
				{
					// The starting granule position MUST not be smaller than the amount of
					// audio on the first page with completed packets
					return (c_int)OpusFileError.BadTimestamp;
				}
			}
			else
			{
				// The first page with completed packets was also the last
				if (Op_Granpos_Add(out pcm_start, cur_page_gp, -total_duration) < 0)
				{
					// If there's less audio on the page than indicated by the granule
					// position, then we're doing end-trimming, and the starting PCM offset
					// is zero by spec mandate
					pcm_start = 0;

					// However, the end-trimming MUST not ask us to trim more samples than
					// exist after applying the pre-skip
					if (Op_Granpos_Cmp(cur_page_gp, _link.head.Pre_Skip) < 0)
						return (c_int)OpusFileError.BadTimestamp;
				}
			}

			// Timestamp the individual packets
			ogg_int64_t prev_packet_gp = pcm_start;
			c_int pi;

			for (pi = 0; pi < op_count; pi++)
			{
				if (cur_page_eos)
				{
					Op_Granpos_Diff(out ogg_int64_t diff, cur_page_gp, prev_packet_gp);
					diff = durations[pi] - diff;

					// If we have samples to trim...
					if (diff > 0)
					{
						// If we trimmed the entire packet, stop (the spec says encoders
						// shouldn't do this, but we support it anyway)
						if (diff > durations[pi])
							break;

						_of.op[pi].GranulePos = prev_packet_gp = cur_page_gp;

						// Move the EOS flag to this packet, if necessary, so we'll trim the
						// samples
						_of.op[pi].Eos = true;
						continue;
					}
				}

				// Update the granule position as normal
				Op_Granpos_Add(out _of.op[pi].GranulePos, prev_packet_gp, durations[pi]);
				prev_packet_gp = _of.op[pi].GranulePos;
			}

			// Update the packet count after end-trimming
			_of.op_count = pi;
			_of.cur_discard_count = (opus_int32)_link.head.Pre_Skip;
			_link.pcm_file_offset = 0;
			_of.prev_packet_gp = _link.pcm_start = pcm_start;
			_of.prev_page_offset = page_offset;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Starting from current cursor position, get the final PCM offset
		/// of the previous page.
		/// This also validates the duration of the link, which, while not
		/// strictly required by the spec, we need to ensure duration
		/// calculations don't overflow.
		/// This is only done for seekable sources.
		/// We must validate that op_find_initial_pcm_offset() succeeded for
		/// this link before calling this function, otherwise it will scan
		/// the entire stream backwards until it reaches the start, and then
		/// fail
		/// </summary>
		/********************************************************************/
		private c_int Op_Find_Final_Pcm_Offset(Pointer<ogg_uint32_t> _serialnos, c_int _nserialnos, OggOpusLink _link, opus_int64 _offset, ogg_uint32_t _end_serialno, ogg_int64_t _end_gp, ref ogg_int64_t _total_duration)
		{
			// For the time being, fetch end PCM offset the simple way
			ogg_uint32_t cur_serialno = _link.serialno;

			if ((_end_serialno != cur_serialno) || (_end_gp == -1))
			{
				_offset = Op_Get_Last_Page(out _end_gp, _offset, cur_serialno, _serialnos, _nserialnos);
				if (_offset < 0)
					return (c_int)_offset;
			}

			// At worst we should have found the first page with completed packets
			if (_offset < _link.data_offset)
				return (c_int)OpusFileError.BadLink;

			// This implementation requires that the difference between the first and last
			// granule positions in each link be representable in a signed, 64-bit
			// number, and that each link also have at least as many samples as the
			// pre-skip requires
			if ((Op_Granpos_Diff(out ogg_int64_t duration, _end_gp, _link.pcm_start) < 0) || (duration < _link.head.Pre_Skip))
				return (c_int)OpusFileError.BadTimestamp;

			// We also require that the total duration be representable in a signed,
			// 64-bit number
			duration -= _link.head.Pre_Skip;

			ogg_int64_t total_duration = _total_duration;

			if ((Internal.Op_Int64_Max - duration) < total_duration)
				return (c_int)OpusFileError.BadTimestamp;

			_total_duration = total_duration + duration;

			_link.pcm_end = _end_gp;
			_link.end_offset = _offset;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Rescale the number _x from the range [0, _from] to [0,_to].
		/// _from and _to must be positive
		/// </summary>
		/********************************************************************/
		private opus_int64 Op_Rescale64(opus_int64 _x, opus_int64 _from, opus_int64 _to)
		{
			if (_x >= _from)
				return _to;

			if (_x <= 0)
				return 0;

			opus_int64 frac = 0;

			for (c_int i = 0; i < 63; i++)
			{
				frac <<= 1;

				if (_x >= (_from >> 1))
				{
					_x -= _from - _x;
					frac |= 1;
				}
				else
					_x <<= 1;
			}

			opus_int64 ret = 0;

			for (c_int i = 0; i < 63; i++)
			{
				if ((frac & 1) != 0)
					ret = (ret & _to & 1) + (ret >> 1) + (_to >> 1);
				else
					ret >>= 1;

				frac >>= 1;
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Try to estimate the location of the next link using the current
		/// seek records, assuming the initial granule position of any
		/// streams we've found is 0
		/// </summary>
		/********************************************************************/
		private opus_int64 Op_Predict_Link_Start(Pointer<OpusSeekRecord> _sr, c_int _nsr, opus_int64 _searched, opus_int64 _end_searched, opus_int32 _bias)
		{
			// Require that we be at least OP_CHUNK_SIZE from the end.
			// We don't require that we be at least OP_CHUNK_SIZE from the beginning,
			// because if we are we'll just scan forward without seeking
			_end_searched -= Op_Chunk_Size;

			if (_searched >= _end_searched)
				return -1;

			opus_int64 bisect = _end_searched;

			for (c_int sri = 0; sri < _nsr; sri++)
			{
				// If the granule position is negative, either it's invalid or we'd cause
				// overflow
				ogg_int64_t gp1 = _sr[sri].gp;
				if (gp1 < 0)
					continue;

				// We require some minimum distance between granule positions to make an
				// estimate.
				// We don't actually know what granule position scheme is being used,
				// because we have no idea what kind of stream these came from.
				// Therefore we require a minimum spacing between them, with the
				// expectation that while bitrates and granule position increments might
				// vary locally in quite complex ways, they are globally smooth
				if (Op_Granpos_Add(out ogg_int64_t gp2_min, gp1, Op_Gp_Spacing_Min) < 0)
				{
					// No granule position would satisfy us
					continue;
				}

				opus_int64 offset1 = _sr[sri].offset;
				ogg_uint32_t serialno1 = _sr[sri].serialno;

				for (c_int srj = sri; srj-- > 0;)
				{
					ogg_int64_t gp2 = _sr[srj].gp;
					if (gp2 < gp2_min)
						continue;

					// Oh, and also make sure these came from the same stream
					if (_sr[srj].serialno != serialno1)
						continue;

					opus_int64 offset2 = _sr[srj].offset;

					// For once, we can subtract with impunity
					ogg_int64_t den = gp2 - gp1;
					ogg_int64_t ipart = gp2 / den;
					opus_int64 num = offset2 - offset1;

					if ((ipart > 0) && (((offset2 - _searched) / ipart) < num))
						continue;

					offset2 -= ipart * num;
					gp2 -= ipart * den;
					offset2 -= Op_Rescale64(gp2, den, num) - _bias;

					if (offset2 < _searched)
						continue;

					bisect = Internal.Op_Min(bisect, offset2);
					break;
				}
			}

			return bisect >= _end_searched ? -1 : bisect;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Op_Bisect_Forward_Serialno(opus_int64 _searched, Pointer<OpusSeekRecord> _sr, c_int _csr, ref Pointer<ogg_uint32_t> _serialnos, ref c_int _nserialnos, ref c_int _cserialnos)
		{
			OggPage og = null;
			c_int nlinks;
			c_int clinks;
			Pointer<ogg_uint32_t> serialnos;
			c_int nserialnos;
			c_int ret;

			Pointer<OggOpusLink> links = _of.links;
			nlinks = clinks = _of.nlinks;
			ogg_int64_t total_duration = 0;

			// We start with one seek record, for the last page in the file.
			// We build up a list of records for places we seek to during link
			// enumeration.
			// This list is kept sorted in reverse order.
			// We only care about seek locations that were _not_ in the current link,
			// therefore we can add them one at a time to the end of the list as we
			// improve the lower bound on the location where the next link starts
			c_int nsr = 1;

			for (;;)
			{
				ogg_int64_t end_offset = 0;

				serialnos = _serialnos;
				nserialnos = _nserialnos;

				if (nlinks >= clinks)
				{
					if (clinks > (int.MaxValue - 1 >> 1))
						return (c_int)OpusFileError.Fault;

					clinks = 2 * clinks + 1;
					links = Memory.Ogg_Realloc(links, (size_t)clinks);
					if (links == null)
						return (c_int)OpusFileError.Fault;

					_of.links = links;
				}

				// Invariants:
				//   We have the headers and serial numbers for the link beginning at 'begin'.
				//   We have the offset and granule position of the last page in the file
				//    (potentially not a page we care about)
				//
				// Scan the seek records we already have to save us some bisection
				c_int sri;
				for (sri = 0; sri < nsr; sri++)
				{
					if (Op_Lookup_Serialno(_sr[sri].serialno, serialnos, nserialnos) != 0)
						break;
				}

				// Is the last page in our current list of serial numbers?
				if (sri <= 0)
					break;

				// Last page wasn't found.
				// We have at least one more link
				opus_int64 last = -1;
				opus_int64 end_searched = _sr[sri - 1].search_start;
				opus_int64 next = _sr[sri - 1].offset;
				ogg_int64_t end_gp = -1;

				if (sri < nsr)
				{
					_searched = _sr[sri].offset + _sr[sri].size;

					if (_sr[sri].serialno == links[nlinks - 1].serialno)
					{
						end_gp = _sr[sri].gp;
						end_offset = _sr[sri].offset;
					}
				}

				nsr = sri;
				opus_int64 bisect = -1;

				// If we've already found the end of at least one link, try to pick the
				// first bisection point at twice the average link size.
				// This is a good choice for files with lots of links that are all about the
				// same size
				if (nlinks > 1)
				{
					opus_int64 last_offset = links[nlinks - 1].offset;
					opus_int64 avg_link_size = last_offset / (nlinks - 1);
					opus_int64 upper_limit = end_searched - Op_Chunk_Size - avg_link_size;

					if ((last_offset > (_searched - avg_link_size)) && (last_offset < upper_limit))
					{
						bisect = last_offset + avg_link_size;

						if (bisect < upper_limit)
							bisect += avg_link_size;
					}
				}

				// We guard against garbage separating the last and first pages of two
				// links below
				while (_searched < end_searched)
				{
					// If we don't have a better estimate, use simple bisection
					if (bisect == -1)
						bisect = _searched + (end_searched - _searched >> 1);

					// If we're within OP_CHUNK_SIZE of the start, scan forward
					if ((bisect - _searched) < Op_Chunk_Size)
						bisect = _searched;
					// Otherwise we're skipping data.
					// Forget the end page, if we saw one, as we might miss a later one
					else
						end_gp = -1;

					ret = Op_Seek_Helper(bisect);
					if (ret < 0)
						return ret;

					last = Op_Get_Next_Page(ref og, _sr[nsr - 1].offset);
					if (last < (opus_int64)OpusFileError.False)
						return (c_int)last;

					opus_int32 next_bias = 0;

					if (last == (opus_int64)OpusFileError.False)
						end_searched = bisect;
					else
					{
						ogg_uint32_t serialno = (ogg_uint32_t)og.SerialNo();
						ogg_int64_t gp = og.GranulePos();

						if (Op_Lookup_Serialno(serialno, serialnos, nserialnos) == 0)
						{
							end_searched = bisect;
							next = last;

							// In reality we should always have enough room, but be paranoid
							if (nsr < _csr)
							{
								_sr[nsr] = new OpusSeekRecord();

								_sr[nsr].search_start = bisect;
								_sr[nsr].offset = last;
								_sr[nsr].size = (opus_int32)(_of.offset - last);
								_sr[nsr].serialno = serialno;
								_sr[nsr].gp = gp;

								nsr++;
							}
						}
						else
						{
							_searched = _of.offset;
							next_bias = Op_Chunk_Size;

							if (serialno == links[nlinks - 1].serialno)
							{
								// This page was from the stream we want, remember it.
								// If it's the last such page in the link, we won't have to go back
								// looking for it later
								end_gp = gp;
								end_offset = last;
							}
						}
					}

					bisect = Op_Predict_Link_Start(_sr, nsr, _searched, end_searched, next_bias);
				}

				// Bisection point found.
				// Get the final granule position of the previous link, assuming
				// op_find_initial_pcm_offset() didn't already determine the link was
				// empty
				if (links[nlinks - 1].pcm_end == -1)
				{
					if (end_gp == -1)
					{
						// If we don't know where the end page is, we'll have to seek back and
						// look for it, starting from the end of the link
						end_offset = next;

						// Also forget the last page we read.
						// It won't be available after the seek
						last = -1;
					}

					ret = Op_Find_Final_Pcm_Offset(serialnos, nserialnos, links[nlinks - 1], end_offset, links[nlinks - 1].serialno, end_gp, ref total_duration);
					if (ret < 0)
						return ret;
				}

				if (last != next)
				{
					// The last page we read was not the first page the next link.
					// Move the cursor position to the offset of that first page.
					// This only performs an actual seek if the first page of the next link
					// does not start at the end of the last page from the current Opus
					// stream with a valid granule position
					ret = Op_Seek_Helper(next);
					if (ret < 0)
						return ret;
				}

				ret = Op_Fetch_Headers(links[nlinks].head, links[nlinks].tags, ref _serialnos, ref _nserialnos, ref _cserialnos, last != next ? null : og);
				if (ret < 0)
					return ret;

				links[nlinks].offset = next;
				links[nlinks].data_offset = _of.offset;
				links[nlinks].serialno = (ogg_uint32_t)_of.os.State.SerialNo;
				links[nlinks].pcm_end = -1;

				// This might consume a page from the next link, however the next bisection
				// always starts with a seek
				ret = Op_Find_Initial_Pcm_Offset(links[nlinks], out _);
				if (ret < 0)
					return ret;

				links[nlinks].pcm_file_offset = total_duration;
				_searched = _of.offset;

				// Mark the current link count so it can be cleaned up on error
				_of.nlinks = ++nlinks;
			}

			// Last page is in the starting serialno list, so we've reached the last link.
			// Now find the last granule position for it (if we didn't the first time we
			// looked at the end of the stream, and if op_find_initial_pcm_offset()
			// didn't already determine the link was empty)
			if (links[nlinks - 1].pcm_end == -1)
			{
				ret = Op_Find_Final_Pcm_Offset(serialnos, nserialnos, links[nlinks - 1], _sr[0].offset, _sr[0].serialno, _sr[0].gp, ref total_duration);
				if (ret < 0)
					return ret;
			}

			// Trim back the links array if necessary
			links = Memory.Ogg_Realloc(links, (size_t)nlinks);
			if (links != null)
				_of.links = links;

			// We also don't need these anymore
			_serialnos.SetToNull();
			_cserialnos = _nserialnos = 0;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Op_Update_Gain()
		{
			// If decode isn't ready, then we'll apply the gain when we initialize the
			// decoder
			if (_of.ready_state < State.InitSet)
				return;

			opus_int32 gain_q8 = _of.gain_offset_q8;
			c_int li = _of.seekable ? _of.cur_link : 0;
			OpusHead head = _of.links[li].head;

			// We don't have to worry about overflow here because the header gain and
			// track gain must lie in the range [-32768,32767], and the user-supplied
			// offset has been pre-clamped to [-98302,98303]
			switch (_of.gain_type)
			{
				case GainType.Album:
				{
					Info.Opus_Tags_Get_Album_Gain(_of.links[li].tags, out c_int album_gain_q8);
					gain_q8 += album_gain_q8;
					gain_q8 += head.Output_Gain;
					break;
				}

				case GainType.Track:
				{
					Info.Opus_Tags_Get_Track_Gain(_of.links[li].tags, out c_int track_gain_q8);
					gain_q8 += track_gain_q8;
					gain_q8 += head.Output_Gain;
					break;
				}

				case GainType.Header:
				{
					gain_q8 += head.Output_Gain;
					break;
				}

				case GainType.Absolute:
				{
					break;
				}
			}

			gain_q8 = Internal.Op_Clamp(-32768, gain_q8, 32767);
//TNE			Opus_Multistream_Decoder_Ctl(_of.od, OpusControlSetRequest.Opus_Set_Gain, gain_q8);
			_of.od.Decoder_Ctl_Set(OpusControlSetRequest.Opus_Set_Gain, gain_q8);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Op_Make_Decode_Ready()
		{
			if (_of.ready_state > State.StreamSet)
				return 0;

			if (_of.ready_state < State.StreamSet)
				return (c_int)OpusFileError.Fault;

			c_int li = _of.seekable ? _of.cur_link : 0;
			OpusHead head = _of.links[li].head;

			c_int stream_count = head.Stream_Count;
			c_int coupled_count = head.Coupled_Count;
			c_int channel_count = head.Channel_Count;

			// Check to see if the current decoder is compatible with the current link
			if ((_of.od != null) && (_of.od_stream_count == stream_count) && (_of.od_coupled_count == coupled_count) && (_of.od_channel_count == channel_count) && (CMemory.MemCmp(_of.od_mapping, head.Mapping, channel_count) == 0))
//TNE				Opus_Multistream_Decoder_Ctl(_of.od, OpusControlSetRequest.Opus_Reset_State);
				_of.od.Decoder_Ctl_Set(OpusControlSetRequest.Opus_Reset_State);
			else
			{
//TNE				Opus_Multistream_Decoder_Destroy(_of.od);
//				_of.od = Opus_Multistream_Decoder_Create(48000, channel_count, stream_count, coupled_count, head.mapping, out c_int err);
				_of.od = OpusDecoder.Create(48000, channel_count, out _);
				if (_of.od == null)
					return (c_int)OpusFileError.Fault;

				_of.od_stream_count = stream_count;
				_of.od_coupled_count = coupled_count;
				_of.od_channel_count = channel_count;

				CMemory.MemCpy(_of.od_mapping, (Pointer<byte>)head.Mapping, channel_count);
			}

			_of.ready_state = State.InitSet;
			_of.bytes_tracked = 0;
			_of.samples_tracked = 0;
			_of.state_channel_count = 0;

			// Use the serial number for the PRNG seed to get repeatable output for
			// straight play-throughts
			_of.dither_seed = _of.links[li].serialno;

			Op_Update_Gain();

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Op_Open_Seekable2_Impl()
		{
			// 64 seek records should be enough for anybody.
			// Actually, with a bisection search in a 63-bit range down to OP_CHUNK_SIZE
			// granularity, much more than enough
			OpusSeekRecord[] sr = new OpusSeekRecord[64];

			// We can seek, so set out learning all about this file
			_of.callbacks.Seek(_of.stream, 0, SeekOrigin.End);
			_of.offset = _of.end = _of.callbacks.Tell(_of.stream);

			if (_of.end < 0)
				return (c_int)OpusFileError.Read;

			opus_int64 data_offset = _of.links[0].data_offset;
			if (_of.end < data_offset)
				return (c_int)OpusFileError.BadLink;

			// Get the offset of the last page of the physical bitstream, or, if we're
			// lucky, the last Opus page of the first link, as most Ogg Opus files will
			// contain a single logical bitstream
			c_int ret = Op_Get_Prev_Page_Serial(out sr[0], _of.end, _of.links[0].serialno, _of.serialnos, _of.nserialnos);
			if (ret < 0)
				return ret;

			// If there's any trailing junk, forget about it
			_of.end = sr[0].offset + sr[0].size;
			if (_of.end < data_offset)
				return (c_int)OpusFileError.BadLink;

			// Now enumerate the bitstream structure
			return Op_Bisect_Forward_Serialno(data_offset, sr, sr.Length, ref _of.serialnos, ref _of.nserialnos, ref _of.cserialnos);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Op_Open_Seekable2()
		{
			// We're partially open and have a first link header state in storage in _of.
			// Save off that stream state so we can come back to it.
			// It would be simpler to just dump all this state and seek back to
			// links[0].data_offset when we're done.
			// But we do the extra work to allow us to seek back to _exactly_ the same
			// stream position we're at now.
			// This allows, e.g., the HTTP backend to continue reading from the original
			// connection (if it's still available), instead of opening a new one.
			// This means we can open and start playing a normal Opus file with a single
			// link and reasonable packet sizes using only two HTTP requests
			c_int start_op_count = _of.op_count;

			// This is a bit too large to put on the stack unconditionally
			Ogg_Packet[] op_start = new Ogg_Packet[start_op_count];
			if (op_start == null)
				return (c_int)OpusFileError.Fault;

			OggSync oy_start = _of.oy;
			OggStream os_start = _of.os;
			opus_int64 prev_page_offset = _of.prev_page_offset;
			opus_int64 start_offset = _of.offset;

			for (int i = 0; i < start_op_count; i++)
				op_start[i] = _of.op[i].MakeDeepClone();

			OggSync.Init(out _of.oy);
			OggStream.Init(out _of.os, -1);

			c_int ret = Op_Open_Seekable2_Impl();

			// Restore the old stream state
			_of.os.Clear();
			_of.oy.Clear();

			_of.oy = oy_start;
			_of.os = os_start;
			_of.offset = start_offset;
			_of.op_count = start_op_count;

			for (int i = 0; i < start_op_count; i++)
				_of.op[i] = op_start[i];

			_of.prev_packet_gp = _of.links[0].pcm_start;
			_of.prev_page_offset = prev_page_offset;
			_of.cur_discard_count = (opus_int32)_of.links[0].head.Pre_Skip;

			if (ret < 0)
				return ret;

			// And restore the position indicator
			ret = _of.callbacks.Seek(_of.stream, Op_Position(), SeekOrigin.Begin);

			return ret < 0 ? (c_int)OpusFileError.Read : 0;
		}



		/********************************************************************/
		/// <summary>
		/// Clear out the current logical bitstream decoder
		/// </summary>
		/********************************************************************/
		private void Op_Decode_Clear()
		{
			// We don't actually free the decoder.
			// We might be able to re-use it for the next link
			_of.op_count = 0;
			_of.od_buffer_size = 0;
			_of.prev_packet_gp = -1;
			_of.prev_page_offset = -1;

			if (!_of.seekable)
				Info.Opus_Tags_Clear(_of.links[0].tags);

			_of.ready_state = State.Opened;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Op_Clear()
		{
			Memory.Ogg_Free(_of.od_buffer);
			_of.od_buffer.SetToNull();

		if (_of.od != null)
//TNE				Opus_Multistream_Decoder_Destroy(_of.od);
			_of.od.Destroy();

			Pointer<OggOpusLink> links = _of.links;

			if (!_of.seekable)
			{
				if ((_of.ready_state > State.Opened) || (_of.ready_state == State.PartOpen))
					Info.Opus_Tags_Clear(links[0].tags);
			}
			else if (links != null)
			{
				c_int nlinks = _of.nlinks;

				for (c_int link = 0; link < nlinks; link++)
					Info.Opus_Tags_Clear(links[link].tags);
			}

			Memory.Ogg_Free(_of.links);
			_of.links.SetToNull();

			Memory.Ogg_Free(_of.serialnos);
			_of.serialnos.SetToNull();

			_of.os.Clear();
			_of.oy.Clear();

			if (_of.callbacks.Close != null)
				_of.callbacks.Close(_of.stream);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Op_Open1(object _stream, OpusFileCallbacks _cb, Pointer<byte> _initial_data, size_t _initial_bytes)
		{
			_of.Clear();

			if (_initial_bytes > c_long.MaxValue)
				return (c_int)OpusFileError.Fault;

			_of.end = -1;
			_of.stream = _stream;
			_of.callbacks = _cb.MakeDeepClone();

			// At a minimum, we need to be able to read data
			if (_of.callbacks.Read == null)
				return (c_int)OpusFileError.Read;

			// Initialize the framing state
			OggSync.Init(out _of.oy);

			// Perhaps some data was previously read into a buffer for testing against
			// other stream types.
			// Allow initialization from this previously read data (especially as we may
			// be reading from a non-seekable stream).
			// This requires copying it into a buffer allocated by ogg_sync_buffer() and
			// doesn't support seeking, so this is not a good mechanism to use for
			// decoding entire files from RAM
			if (_initial_bytes > 0)
			{
				Pointer<byte> buffer = _of.oy.Buffer((c_long)_initial_bytes);
				CMemory.MemCpy(buffer, _initial_data, (int)_initial_bytes);
				_of.oy.Wrote((c_long)_initial_bytes);
			}

			// Can we seek?
			bool seekable = (_cb.Seek != null) && (_cb.Seek(_stream, 0, SeekOrigin.Current) != -1);

			// If seek is implemented, tell must also be implemented
			if (seekable)
			{
				if (_of.callbacks.Tell == null)
					return (c_int)OpusFileError.Inval;

				opus_int64 pos = _of.callbacks.Tell(_stream);

				// If the current position is not equal to the initial bytes consumed,
				// absolute seeking will not work
				if (pos != (opus_int64)_initial_bytes)
					return (c_int)OpusFileError.Inval;
			}

			_of.seekable = seekable;

			// Don't seek yet.
			// Set up a 'single' (current) logical bitstream entry for partial open
			_of.links = Memory.Ogg_MAlloc<OggOpusLink>(1);
			_of.links[0] = new OggOpusLink();

			// The serialno gets filled in later by op_fetch_headers()
			OggStream.Init(out _of.os, -1);
			OggPage pog = null;
			c_int ret;

			for (;;)
			{
				// Fetch all BOS pages, store the Opus header and all seen serial numbers,
				// and load subsequent Opus setup headers
				ret = Op_Fetch_Headers(_of.links[0].head, _of.links[0].tags, ref _of.serialnos, ref _of.nserialnos, ref _of.cserialnos, pog);
				if (ret < 0)
					break;

				_of.nlinks = 1;
				_of.links[0].offset = 0;
				_of.links[0].data_offset = _of.offset;
				_of.links[0].pcm_end = -1;
				_of.links[0].serialno = (ogg_uint32_t)_of.os.State.SerialNo;

				// Fetch the initial PCM offset
				ret = Op_Find_Initial_Pcm_Offset(_of.links[0], out OggPage og);

				if (seekable || (ret <= 0))
					break;

				// This link was empty, but we already have the BOS page for the next one in
				// og.
				// We can't seek, so start processing the next link right now
				Info.Opus_Tags_Clear(_of.links[0].tags);
				_of.nlinks = 0;

				if (!seekable)
					_of.cur_link++;

				pog = og;
			}

			if (ret >= 0)
				_of.ready_state = State.PartOpen;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Op_Open2()
		{
			c_int ret;

			if (_of.seekable)
			{
				_of.ready_state = State.Opened;

				ret = Op_Open_Seekable2();
			}
			else
				ret = 0;

			if (ret >= 0)
			{
				// We have buffered packets from op_find_initial_pcm_offset().
				// Move to OP_INITSET so we can use them
				_of.ready_state = State.StreamSet;

				ret = Op_Make_Decode_Ready();
				if (ret >= 0)
					return 0;
			}

			// Don't auto-close the stream on failure
			_of.callbacks.Close = null;
			Op_Clear();

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Partially open a stream using the given set of callbacks to
		/// access it. This tests for Opusness and loads the headers for the
		/// first link. It does not seek (although it tests for seekability).
		/// You can query a partially open stream for the few pieces of basic
		/// information returned by op_serialno(), op_channel_count(),
		/// op_head(), and op_tags() (but only for the first link).
		/// You may also determine if it is seekable via a call to
		/// op_seekable(). You cannot read audio from the stream, seek, get
		/// the size or duration, get information from links other than the
		/// first one, or even get the total number of links until you
		/// finish opening the stream with op_test_open().
		/// If you do not need to do any of these things, you can dispose of
		/// it with op_free() instead.
		/// 
		/// This function is provided mostly to simplify porting existing
		/// code that used libvorbisfile.
		/// For new code, you are likely better off using op_test() instead,
		/// which is less resource-intensive, requires less data to succeed,
		/// and imposes a hard limit on the amount of data it examines
		/// (important for unseekable streams, where all such data must be
		/// buffered until you are sure of the stream type)
		/// </summary>
		/********************************************************************/
		public static OpusFile Op_Test_Callbacks(object _stream, OpusFileCallbacks _cb, Pointer<byte> _initial_data, size_t _initial_bytes, out OpusFileError _error)
		{
			OpusFile of = new OpusFile(new OggOpusFile());
			c_int ret = (c_int)OpusFileError.Fault;

			if (of != null)
			{
				ret = of.Op_Open1(_stream, _cb, _initial_data, _initial_bytes);
				if (ret >= 0)
				{
					_error = OpusFileError.Ok;
					return of;
				}

				// Don't auto-close the stream on failure
				of._of.callbacks.Close = null;
				of.Op_Clear();
			}

			_error = (OpusFileError)ret;

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Open a stream using the given set of callbacks to access it
		/// </summary>
		/********************************************************************/
		public static OpusFile Op_Open_Callbacks(object _stream, OpusFileCallbacks _cb, Pointer<byte> _initial_data, size_t _initial_bytes, out OpusFileError _error)
		{
			OpusFile of = Op_Test_Callbacks(_stream, _cb, _initial_data, _initial_bytes, out _error);
			if (of != null)
			{
				c_int ret = of.Op_Open2();
				if (ret >= 0)
					return of;

				_error = (OpusFileError)ret;
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Convenience routine to clean up from failure for the open
		/// functions that create their own streams
		/// </summary>
		/********************************************************************/
		private static OpusFile Op_Open_Close_On_Failure(System.IO.Stream _stream, OpusFileCallbacks _cb, out OpusFileError _error)
		{
			if (_stream == null)
			{
				_error = OpusFileError.Fault;
				return null;
			}

			OpusFile of = Op_Open_Callbacks(_stream, _cb, null, 0, out _error);
			if (of == null)
			{
				if (_cb.Close != null)
					_cb.Close(_stream);
			}

			return of;
		}



		/********************************************************************/
		/// <summary>
		/// Open a stream from the given .NET stream
		/// </summary>
		/********************************************************************/
		public static OpusFile Op_Open_Stream(System.IO.Stream _stream, bool _leaveOpen, out OpusFileError _error)
		{
			return Op_Open_Close_On_Failure(Stream.Op_IO_Stream_Create(out OpusFileCallbacks cb, _stream, _leaveOpen), cb, out _error);
		}



		/********************************************************************/
		/// <summary>
		/// Release all memory used by an OggOpusFile
		/// </summary>
		/********************************************************************/
		public void Op_Free()
		{
			if (_of != null)
			{
				Op_Clear();
				_of = null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Get the channel count of the given link in a (possibly-chained)
		/// Ogg Opus stream.
		/// This is equivalent to op_head(_of,_li)->channel_count, but is
		/// provided for convenience.
		/// This function may be called on partially-opened streams, but it
		/// will always return the channel count of the Opus stream in the
		/// first link
		/// </summary>
		/********************************************************************/
		public c_int Op_Channel_Count(c_int _li)
		{
			return Op_Head(_li).Channel_Count;
		}



		/********************************************************************/
		/// <summary>
		/// Get the total PCM length (number of samples at 48 kHz) of the
		/// stream, or of an individual link in a (possibly-chained) Ogg
		/// Opus stream
		/// </summary>
		/********************************************************************/
		public ogg_int64_t Op_Pcm_Total(c_int _li)
		{
			c_int nlinks = _of.nlinks;

			if ((_of.ready_state < State.Opened) || !_of.seekable || (_li >= nlinks))
				return (ogg_int64_t)OpusFileError.Inval;

			Pointer<OggOpusLink> links = _of.links;

			// We verify that the granule position differences are larger than the
			// pre-skip and that the total duration does not overflow during link
			// enumeration, so we don't have to check here
			ogg_int64_t pcm_total = 0;

			if (_li < 0)
			{
				pcm_total = links[nlinks - 1].pcm_file_offset;
				_li = nlinks - 1;
			}

			Op_Granpos_Diff(out ogg_int64_t diff, links[_li].pcm_end, links[_li].pcm_start);

			return pcm_total + diff - links[_li].head.Pre_Skip;
		}



		/********************************************************************/
		/// <summary>
		/// Get the ID header information for the given link in a (possibly
		/// chained) Ogg Opus stream.
		/// This function may be called on partially-opened streams, but it
		/// will always return the ID header information of the Opus stream
		/// in the first link
		/// </summary>
		/********************************************************************/
		public OpusHead Op_Head(c_int _li)
		{
			if (_li >= _of.nlinks)
				_li = _of.nlinks - 1;

			if (!_of.seekable)
				_li = 0;

			return _of.links[_li < 0 ? _of.cur_link : _li].head;
		}



		/********************************************************************/
		/// <summary>
		/// Get the comment header information for the given link in a
		/// (possibly chained) Ogg Opus stream.
		/// This function may be called on partially-opened streams, but it
		/// will always return the tags from the Opus stream in the first
		/// link
		/// </summary>
		/********************************************************************/
		public OpusTags Op_Tags(c_int _li)
		{
			if (_li >= _of.nlinks)
				_li = _of.nlinks - 1;

			if (!_of.seekable)
			{
				if ((_of.ready_state < State.StreamSet) && (_of.ready_state != State.PartOpen))
					return null;

				_li = 0;
			}
			else if (_li < 0)
				_li = _of.ready_state >= State.StreamSet ? _of.cur_link : 0;

			return _of.links[_li].tags;
		}



		/********************************************************************/
		/// <summary>
		/// Compute an average bitrate given a byte and sample count
		/// </summary>
		/********************************************************************/
		private opus_int32 Op_Calc_Bitrate(opus_int64 _bytes, ogg_int64_t _samples)
		{
			if (_samples <= 0)
				return Internal.Op_Int32_Max;

			// These rates are absurd, but let's handle them anyway
			if (_bytes > ((Internal.Op_Int64_Max - (_samples >> 1)) / (48000 * 8)))
			{
				if ((_bytes / (Internal.Op_Int32_Max / (48000 * 8))) >= _samples)
					return Internal.Op_Int32_Max;

				ogg_int64_t den = _samples / (48000 * 8);

				return (opus_int32)((_bytes + (den >> 1)) / den);
			}

			// This can't actually overflow in normal operation: even with a pre-skip of
			// 545 2.5 ms frames with 8 streams running at 1282*8+1 bytes per packet
			// (1275 byte frames + Opus framing overhead + Ogg lacing values), that all
			// produce a single sample of decoded output, we still don't top 45 Mbps.
			// The only way to get bitrates larger than that is with excessive Opus
			// padding, more encoded streams than output channels, or lots and lots of
			// Ogg pages with no packets on them
			return (opus_int32)Internal.Op_Min((_bytes * 48000 * 8 + (_samples >> 1)) / _samples, Internal.Op_Int32_Max);
		}



		/********************************************************************/
		/// <summary>
		/// Compute the instantaneous bitrate, measured as the ratio of bits
		/// to playable samples decoded since
		///  a) the last call to op_bitrate_instant(),
		///  b) the last seek, or
		///  c) the start of playback, whichever was most recent.
		/// This will spike somewhat after a seek or at the start/end of a
		/// chain boundary, as pre-skip, pre-roll, and end-trimming causes
		/// samples to be decoded but not played
		/// </summary>
		/********************************************************************/
		public opus_int32 Op_Bitrate_Instant()
		{
			if (_of.ready_state < State.Opened)
				return (opus_int32)OpusFileError.Inval;

			ogg_int64_t samples_tracked = _of.samples_tracked;
			if (samples_tracked == 0)
				return (opus_int32)OpusFileError.False;

			opus_int32 ret = Op_Calc_Bitrate(_of.bytes_tracked, samples_tracked);

			_of.bytes_tracked = 0;
			_of.samples_tracked = 0;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Given a serialno, find a link with a corresponding Opus stream,
		/// if it exists
		/// </summary>
		/********************************************************************/
		private c_int Op_Get_Link_From_Serialno(c_int _cur_link, opus_int64 _page_offset, ogg_uint32_t _serialno)
		{
			Pointer<OggOpusLink> links = _of.links;
			c_int nlinks = _of.nlinks;
			c_int li_lo = 0;

			// Start off by guessing we're just a multiplexed page in the current link
			c_int li_hi = (_cur_link + 1 < nlinks) && (_page_offset < links[_cur_link + 1].offset) ? _cur_link + 1 : nlinks;

			do
			{
				if (_page_offset >= links[_cur_link].offset)
					li_lo = _cur_link;
				else
					li_hi = _cur_link;

				_cur_link = li_lo + (li_hi - li_lo >> 1);
			}
			while ((li_hi - li_lo) > 1);

			// We've identified the link that should contain this page.
			// Make sure it's a page we care about
			if (links[_cur_link].serialno != _serialno)
				return (c_int)OpusFileError.False;

			return _cur_link;
		}



		/********************************************************************/
		/// <summary>
		/// Fetch and process a page.
		/// This handles the case where we're at a bitstream boundary and
		/// dumps the decoding machine.
		/// If the decoding machine is unloaded, it loads it.
		/// It also keeps prev_packet_gp up to date (seek and read both use
		/// this).
		/// </summary>
		/********************************************************************/
		private c_int Op_Fetch_And_Process_Page(OggPage _og, opus_int64 _page_offset, bool _spanp, bool _ignore_holes)
		{
			c_int ret;

			bool seekable = _of.seekable;
			Pointer<OggOpusLink> links = _of.links;
			c_int cur_link = seekable ? _of.cur_link : 0;
			ogg_uint32_t cur_serialno = links[cur_link].serialno;

			// Handle one page
			for (;;)
			{
				OggPage og;

				// If we were given a page to use, use it
				if (_og != null)
				{
					og = _og;
					_og = null;
				}
				// Keep reading until we get a page with the correct serialno
				else
				{
					og = null;
					_page_offset = Op_Get_Next_Page(ref og, _of.end);
				}

				// EOF: Leave uninitialized
				if (_page_offset < 0)
					return _page_offset < (opus_int64)OpusFileError.False ? (c_int)_page_offset : (c_int)OpusFileError.EOF;

				if ((_of.ready_state >= State.StreamSet) && (cur_serialno != (ogg_uint32_t)og.SerialNo()))
				{
					// Two possibilities:
					//  1) Another stream is multiplexed into this logical section, or
					if (!og.Bos())
						continue;

					//  2) Our decoding just traversed a bitstream boundary
					if (!_spanp)
						return (c_int)OpusFileError.EOF;

					if (_of.ready_state >= State.InitSet)
						Op_Decode_Clear();
				}
				// Bitrate tracking: add the header's bytes here.
				// The body bytes are counted when we consume the packets
				else
					_of.bytes_tracked += og.Page.HeaderLen;

				// Do we need to load a new machine before submitting the page?
				// This is different in the seekable and non-seekable cases.
				// In the seekable case, we already have all the header information loaded
				// and cached.
				// We just initialize the machine with it and continue on our merry way.
				// In the non-seekable (streaming) case, we'll only be at a boundary if we
				// just left the previous logical bitstream, and we're now nominally at the
				// header of the next bitstream
				if (_of.ready_state < State.StreamSet)
				{
					if (seekable)
					{
						ogg_uint32_t serialno = (ogg_uint32_t)og.SerialNo();

						// Match the serialno to bitstream section
						if (links[cur_link].serialno != serialno)
						{
							// It wasn't a page from the current link.
							// Is it from the next one?
							if (((cur_link + 1) < _of.nlinks) && (links[cur_link + 1].serialno == serialno))
								cur_link++;
							else
							{
								c_int new_link = Op_Get_Link_From_Serialno(cur_link, _page_offset, serialno);

								// Not a desired Opus bitstream section
								// Keep trying
								if (new_link < 0)
									continue;

								cur_link = new_link;
							}
						}

						cur_serialno = serialno;
						_of.cur_link = cur_link;

						_of.os.Reset_SerialNo((c_int)serialno);
						_of.ready_state = State.StreamSet;

						// If we're at the start of this link, initialize the granule position
						// and pre-skip tracking
						if (_page_offset <= links[cur_link].data_offset)
						{
							_of.prev_packet_gp = links[cur_link].pcm_start;
							_of.prev_page_offset = -1;
							_of.cur_discard_count = (c_int)links[cur_link].head.Pre_Skip;

							// Ignore a hole at the start of a new link (this is common for
							// streams joined in the middle) or after seeking
							_ignore_holes = true;
						}
					}
					else
					{
						do
						{
							// We're streaming.
							// Fetch the two header packets, build the info struct
							ret = Op_Fetch_Headers(links[0].head, links[0].tags, og);
							if (ret < 0)
								return ret;

							// op_find_initial_pcm_offset() will suppress any initial hole for us,
							// so no need to set _ignore_holes
							ret = Op_Find_Initial_Pcm_Offset(links[0], out og);
							if (ret < 0)
								return ret;

							_of.links[0].serialno = cur_serialno = (ogg_uint32_t)_of.os.State.SerialNo;
							_of.cur_link++;
						}
						// If the link was empty, keep going, because we already have the
						// BOS page of the next one in og
						while (ret > 0);

						// If we didn't get any packets out of op_find_initial_pcm_offset(),
						// keep going (this is possible if end-trimming trimmed them all)
						if (_of.op_count <= 0)
							continue;

						// Otherwise, we're done.
						// TODO: This resets bytes_tracked, which misses the header bytes
						// already processed by op_find_initial_pcm_offset()
						ret = Op_Make_Decode_Ready();
						if (ret < 0)
							return ret;

						return 0;
					}
				}

				// The buffered page is the data we want, and we're ready for it.
				// Add it to the stream state
				if (_of.ready_state == State.StreamSet)
				{
					ret = Op_Make_Decode_Ready();
					if (ret < 0)
						return ret;
				}

				// Extract all the packets from the current page
				_of.os.PageIn(og);

				if (_of.ready_state >= State.InitSet)
				{
					c_int[] durations = new c_int[255];

					bool report_hole = false;
					opus_int32 total_duration = Op_Collect_Audio_Packets(durations);

					if (total_duration < 0)
					{
						// libogg reported a hole (a gap in the page sequence numbers).
						// Drain the packets from the page anyway.
						// If we don't, they'll still be there when we fetch the next page.
						// Then, when we go to pull out packets, we might get more than 255,
						// which would overrun our packet buffer.
						// We repeat this call until we get any actual packets, since we might
						// have buffered multiple out-of-sequence pages with no packets on
						// them
						do
						{
							total_duration = Op_Collect_Audio_Packets(durations);
						}
						while (total_duration > 0);

						if (!_ignore_holes)
						{
							// Report the hole to the caller after we finish timestamping the packets
							report_hole = true;

							// We had lost or damaged pages, so reset our granule position
							// tracking.
							// This makes holes behave the same as a small raw seek.
							// If the next page is the EOS page, we'll discard it (because we
							// can't perform end trimming properly), and we'll always discard at
							// least 80 ms of audio (to allow decoder state to re-converge).
							// We could try to fill in the gap with PLC by looking at timestamps
							// in the non-EOS case, but that's complicated and error prone and we
							// can't rely on the timestamps being valid
							_of.prev_packet_gp = -1;
						}
					}

					c_int op_count = _of.op_count;

					// If we found at least one audio data packet, compute per-packet granule
					// positions for them
					if (op_count > 0)
					{
						ogg_int64_t diff;
						ogg_int64_t cur_packet_gp;
						c_int pi;

						ogg_int64_t cur_page_gp = _of.op[op_count - 1].GranulePos;
						bool cur_page_eos = _of.op[op_count - 1].Eos;
						ogg_int64_t prev_packet_gp = _of.prev_packet_gp;

						if (prev_packet_gp == -1)
						{
							// This is the first call after a raw seek.
							// Try to reconstruct prev_packet_gp from scratch
							if (cur_page_eos)
							{
								// If the first page we hit after our seek was the EOS page, and
								// we didn't start from data_offset or before, we don't have
								// enough information to do end-trimming.
								// Proceed to the next link, rather than risk playing back some
								// samples that shouldn't have been played
								_of.op_count = 0;

								if (report_hole)
									return (c_int)OpusFileError.Hole;

								continue;
							}

							// By default discard 80 ms of data after a seek, unless we seek
							// into the pre-skip region
							opus_int32 cur_discard_count = 80 * 48;
							cur_page_gp = _of.op[op_count - 1].GranulePos;

							// Try to initialize prev_packet_gp.
							// If the current page had packets but didn't have a granule
							// position, or the granule position it had was too small (both
							// illegal), just use the starting granule position for the link
							prev_packet_gp = links[cur_link].pcm_start;

							if (cur_page_gp != -1)
								Op_Granpos_Add(out prev_packet_gp, cur_page_gp, -total_duration);

							if (Op_Granpos_Diff(out diff, prev_packet_gp, links[cur_link].pcm_start) == 0)
							{
								// If we start at the beginning of the pre-skip region, or we're
								// at least 80 ms from the end of the pre-skip region, we discard
								// to the end of the pre-skip region.
								// Otherwise, we still use the 80 ms default, which will discard
								// past the end of the pre-skip region
								opus_int32 pre_skip = (opus_int32)links[cur_link].head.Pre_Skip;

								if ((diff >= 0) && (diff <= Internal.Op_Max(0, pre_skip - 80 * 48)))
									cur_discard_count = pre_skip - (c_int)diff;
							}

							_of.cur_discard_count = cur_discard_count;
						}

						if (cur_page_gp == -1)
						{
							// This page had completed packets but didn't have a valid granule
							// position.
							// This is illegal, but we'll try to handle it by continuing to count
							// forwards from the previous page
							if (Op_Granpos_Add(out cur_page_gp, prev_packet_gp, total_duration) < 0)
							{
								// The timestamp for this page overflowed
								cur_page_gp = links[cur_link].pcm_end;
							}
						}

						// If we hit the last page, handle end-trimming
						if (cur_page_eos && (Op_Granpos_Diff(out diff, cur_page_gp, prev_packet_gp) == 0) && (diff < total_duration))
						{
							cur_packet_gp = prev_packet_gp;

							for (pi = 0; pi < op_count; pi++)
							{
								// Check for overflow
								if ((diff < 0) && ((Internal.Op_Int64_Max + diff) < durations[pi]))
									diff = durations[pi] + 1;
								else
									diff = durations[pi] - diff;

								// If we have samples to trim...
								if (diff > 0)
								{
									// If we trimmed the entire packet, stop (the spec says encoders
									// shouldn't do this, but we support it anyway)
									if (diff > durations[pi])
										break;

									cur_packet_gp = cur_page_gp;

									// Move the EOS flag to this packet, if necessary, so we'll trim
									// the samples during decode
									_of.op[pi].Eos = true;
								}
								else
								{
									// Update the granule position as normal
									Op_Granpos_Add(out cur_packet_gp, cur_packet_gp, durations[pi]);
								}

								_of.op[pi].GranulePos = cur_packet_gp;
								Op_Granpos_Diff(out diff, cur_page_gp, cur_packet_gp);
							}
						}
						else
						{
							// Propagate timestamps to earlier packets.
							// op_granpos_add(&prev_packet_gp,prev_packet_gp,total_duration)
							// should succeed and give prev_packet_gp==cur_page_gp.
							// But we don't bother to check that, as there isn't much we can do
							// if it's not true, and it actually will not be true on the first
							// page after a seek, if there was a continued packet.
							// The only thing we guarantee is that the start and end granule
							// positions of the packets are valid, and that they are monotonic
							// within a page.
							// They might be completely out of range for this link (we'll check
							// that elsewhere), or non-monotonic between pages
							if (Op_Granpos_Add(out prev_packet_gp, cur_page_gp, -total_duration) < 0)
							{
								// The starting timestamp for the first packet on this page
								// underflowed.
								// This is illegal, but we ignore it
								prev_packet_gp = 0;
							}

							for (pi = 0; pi < op_count; pi++)
							{
								if (Op_Granpos_Add(out cur_packet_gp, cur_page_gp, -total_duration) < 0)
								{
									// The start timestamp for this packet underflowed.
									// This is illegal, but we ignore it
									cur_packet_gp = 0;
								}

								total_duration -= durations[pi];

								Op_Granpos_Add(out cur_packet_gp, cur_packet_gp, durations[pi]);

								_of.op[pi].GranulePos = cur_packet_gp;
							}
						}

						_of.prev_packet_gp = prev_packet_gp;
						_of.prev_page_offset = _page_offset;
						_of.op_count = op_count = pi;
					}

					if (report_hole)
						return (c_int)OpusFileError.Hole;

					// If end-trimming didn't trim all packets, we're done
					if (op_count > 0)
						return 0;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// A small helper to determine if an Ogg page contains data that
		/// continues onto a subsequent page
		/// </summary>
		/********************************************************************/
		private bool Op_Page_Continues(OggPage _og)
		{
			c_int nlacing = _og.Page.Header[26];

			// This also correctly handles the (unlikely) case of nlacing==0, because
			// 0!=255
			return _og.Page.Header[27 + nlacing - 1] == 255;
		}



		/********************************************************************/
		/// <summary>
		/// A small helper to buffer the continued packet data from a page
		/// </summary>
		/********************************************************************/
		private void Op_Buffer_Continued_Data(OggPage _og)
		{
			_of.os.PageIn(_og);

			// Drain any packets that did end on this page (and ignore holes).
			// We only care about the continued packet data
			while (_of.os.PacketOut(out Ogg_Packet _) != 0)
			{
			}
		}



		/********************************************************************/
		/// <summary>
		/// Search within link _li for the page with the highest granule
		/// position preceding (or equal to) _target_gp.
		/// There is a danger here: missing pages or incorrect frame number
		/// information in the bitstream could make our task impossible.
		/// Account for that (and report it as an error condition)
		/// </summary>
		/********************************************************************/
		private c_int Op_Pcm_Seek_Page(ogg_int64_t _target_gp, c_int _li)
		{
			OggPage og = null;
			ogg_int64_t pcm_start;
			ogg_int64_t best_gp;
			ogg_int64_t diff;
			ogg_int64_t begin;
			ogg_int64_t end;
			ogg_int64_t boundary;
			ogg_int64_t best;
			ogg_int64_t best_start;
			c_int ret;

			_of.bytes_tracked = 0;
			_of.samples_tracked = 0;

			OggOpusLink link = _of.links[_li];
			best_gp = pcm_start = link.pcm_start;
			ogg_int64_t pcm_end = link.pcm_end;
			ogg_uint32_t serialno = link.serialno;
			best = best_start = begin = link.data_offset;
			ogg_int64_t page_offset = -1;
			bool buffering = false;

			// We discard the first 80 ms of data after a seek, so seek back that much
			// farther.
			// If we can't, simply seek to the beginning of the link
			if ((Op_Granpos_Add(out _target_gp, _target_gp, -80 * 48) < 0) || (Op_Granpos_Cmp(_target_gp, pcm_start) < 0))
				_target_gp = pcm_start;

			// Special case seeking to the start of the link
			opus_int32 pre_skip = (opus_int32)link.head.Pre_Skip;

			Op_Granpos_Add(out ogg_int64_t pcm_pre_skip, pcm_start, pre_skip);

			if (Op_Granpos_Cmp(_target_gp, pcm_pre_skip) < 0)
				end = boundary = begin;
			else
			{
				end = boundary = link.end_offset;

				// If we were decoding from this link, we can narrow the range a bit
				if ((_li == _of.cur_link) && (_of.ready_state >= State.InitSet))
				{
					c_int op_count = _of.op_count;

					// The only way the offset can be invalid _and_ we can fail the granule
					// position checks below is if someone changed the contents of the last
					// page since we read it.
					// We'd be within our rights to just return OP_EBADLINK in that case, but
					// we'll simply ignore the current position instead
					opus_int64 offset = _of.offset;

					if ((op_count > 0) && (offset <= end))
					{
						// Make sure the timestamp is valid.
						// The granule position might be -1 if we collected the packets from a
						// page without a granule position after reporting a hole
						ogg_int64_t gp = _of.op[op_count - 1].GranulePos;

						if ((gp != -1) && (Op_Granpos_Cmp(pcm_start, gp) < 0) && (Op_Granpos_Cmp(pcm_end, gp) > 0))
						{
							Op_Granpos_Diff(out diff, gp, _target_gp);

							// We only actually use the current time if either
							//  a) We can cut off at least half the range, or
							//  b) We're seeking sufficiently close to the current position that
							//     it's likely to be informative.
							// Otherwise it appears using the whole link range to estimate the
							// first seek location gives better results, on average
							if (diff < 0)
							{
								if (((offset - begin) >= (end - begin >> 1)) || (diff > -Op_Cur_Time_Thresh))
								{
									best = begin = offset;
									best_gp = pcm_start = gp;

									// If we have buffered data from a continued packet, remember the
									// offset of the previous page's start, so that if we do wind up
									// having to seek back here later, we can prime the stream with
									// the continued packet data.
									// With no continued packet, we remember the end of the page
									best_start = _of.os.State.BodyReturned < _of.os.State.BodyFill ? _of.prev_page_offset : best;

									// Buffer any continued packet data starting from here
									buffering = true;
								}
							}
							else
							{
								// We might get lucky and already have the packet with the target
								// buffered.
								// Worth checking.
								// For very small files (with all of the data in a single page,
								// generally 1 second or less), we can loop them continuously
								// without seeking at all
								Op_Granpos_Add(out ogg_int64_t prev_page_gp, _of.op[0].GranulePos, -Op_Get_Packet_Duration(_of.op[0].Packet, _of.op[0].Bytes));

								if (Op_Granpos_Cmp(prev_page_gp, _target_gp) <= 0)
								{
									// Don't call op_decode_clear(), because it will dump our
									// packets
									_of.op_pos = 0;
									_of.od_buffer_size = 0;
									_of.prev_packet_gp = prev_page_gp;

									// _of->prev_page_offset already points to the right place
									_of.ready_state = State.StreamSet;

									return Op_Make_Decode_Ready();
								}

								// No such luck.
								// Check if we can cut off at least half the range, though
								if (((offset - begin) <= (end - begin >> 1)) || (diff < Op_Cur_Time_Thresh))
								{
									// We really want the page start here, but this will do
									end = boundary = offset;
									pcm_end = gp;
								}
							}
						}
					}
				}
			}

			// This code was originally based on the "new search algorithm by HB (Nicholas
			// Vinen)" from libvorbisfile.
			// It has been modified substantially since
			Op_Decode_Clear();

			if (!buffering)
				_of.os.Reset_SerialNo((c_int)serialno);

			_of.cur_link = _li;
			_of.ready_state = State.StreamSet;

			// Initialize the interval size history
			opus_int64 d0;
			opus_int64 d1;
			opus_int64 d2;

			d2 = d1 = d0 = end - begin;
			bool force_bisect = false;

			while (begin < end)
			{
				opus_int64 bisect;

				if ((end - begin) < Op_Chunk_Size)
					bisect = begin;
				else
				{
					// Update the interval size history
					d0 = d1 >> 1;
					d1 = d2 >> 1;
					d2 = end - begin >> 1;

					if (force_bisect)
						bisect = begin + (end - begin >> 1);
					else
					{
						Op_Granpos_Diff(out diff, _target_gp, pcm_start);
						Op_Granpos_Diff(out ogg_int64_t diff2, pcm_end, pcm_start);

						// Take a (pretty decent) guess
						bisect = begin + Op_Rescale64(diff, diff2, end - begin) - Op_Chunk_Size;
					}

					if ((bisect - Op_Chunk_Size) < begin)
						bisect = begin;

					force_bisect = false;
				}

				if (bisect != _of.offset)
				{
					// Discard any buffered continued packet data
					if (buffering)
						_of.os.Reset();

					buffering = false;
					page_offset = -1;

					ret = Op_Seek_Helper(bisect);
					if (ret < 0)
						return ret;
				}

				opus_int32 chunk_size = Op_Chunk_Size;
				opus_int64 next_boundary = boundary;

				// Now scan forward and figure out where we landed.
				// In the ideal case, we will see a page with a granule position at or
				// before our target, followed by a page with a granule position after our
				// target (or the end of the search interval).
				// Then we can just drop out and will have all of the data we need with no
				// additional seeking.
				// If we landed too far before, or after, we'll break out and do another
				// bisection
				while (begin < end)
				{
					page_offset = Op_Get_Next_Page(ref og, boundary);
					if (page_offset < 0)
					{
						if (page_offset < (ogg_int64_t)OpusFileError.False)
							return (c_int)page_offset;

						// There are no more pages in our interval from our stream with a valid
						// timestamp that start at position bisect or later.
						// If we scanned the whole interval, we're done
						if (bisect <= (begin + 1))
							end = begin;
						else
						{
							// Otherwise, back up one chunk.
							// First, discard any data from a continued packet
							if (buffering)
								_of.os.Reset();

							buffering = false;
							bisect = Internal.Op_Max(bisect - chunk_size, begin);

							ret = Op_Seek_Helper(bisect);
							if (ret < 0)
								return ret;

							// Bump up the chunk size
							chunk_size = Internal.Op_Min(2 * chunk_size, Op_Chunk_Size_Max);

							// If we did find a page from another stream or without a timestamp,
							// don't read past it
							boundary = next_boundary;
						}
					}
					else
					{
						// Save the offset of the first page we found after the seek, regardless
						// of the stream it came from or whether or not it has a timestamp
						next_boundary = Internal.Op_Min(page_offset, next_boundary);

						if (serialno != (ogg_uint32_t)og.SerialNo())
							continue;

						bool has_packets = og.Packets() > 0;

						// Force the gp to -1 (as it should be per spec) if no packets end on
						// this page.
						// Otherwise we might get confused when we try to pull out a packet
						// with that timestamp and can't find it
						ogg_int64_t gp = has_packets ? og.GranulePos() : -1;

						if (gp == -1)
						{
							if (buffering)
							{
								if (!has_packets)
									_of.os.PageIn(og);
								else
								{
									// If packets did end on this page, but we still didn't have a
									// valid granule position (in violation of the spec!), stop
									// buffering continued packet data.
									// Otherwise we might continue past the packet we actually
									// wanted
									_of.os.Reset();
									buffering = false;
								}
							}
							continue;
						}

						if (Op_Granpos_Cmp(gp, _target_gp) < 0)
						{
							// We found a page that ends before our target.
							// Advance to the raw offset of the next page
							begin = _of.offset;

							if ((Op_Granpos_Cmp(pcm_start, gp) > 0) || (Op_Granpos_Cmp(pcm_end, gp) < 0))
							{
								// Don't let pcm_start get out of range!
								// That could happen with an invalid timestamp
								break;
							}

							// Save the byte offset of the end of the page with this granule
							// position
							best = best_start = begin;

							// Buffer any data from a continued packet, if necessary.
							// This avoids the need to seek back here if the next timestamp we
							// encounter while scanning forward lies after our target
							if (buffering)
								_of.os.Reset();

							if (Op_Page_Continues(og))
							{
								Op_Buffer_Continued_Data(og);

								// If we have a continued packet, remember the offset of this
								// page's start, so that if we do wind up having to seek back here
								// later, we can prime the stream with the continued packet data.
								// With no continued packet, we remember the end of the page
								best_start = page_offset;
							}

							// Then force buffering on, so that if a packet starts (but does not
							// end) on the next page, we still avoid the extra seek back
							buffering = true;
							best_gp = pcm_start = gp;

							Op_Granpos_Diff(out diff, _target_gp, pcm_start);

							// If we're more than a second away from our target, break out and
							// do another bisection
							if (diff > 48000)
								break;

							// Otherwise, keep scanning forward (do NOT use begin+1)
							bisect = begin;
						}
						else
						{
							// We found a page that ends after our target.
							// If we scanned the whole interval before we found it, we're done
							if (bisect <= (begin + 1))
								end = begin;
							else
							{
								end = bisect;

								// In later iterations, don't read past the first page we found
								boundary = next_boundary;

								// If we're not making much progress shrinking the interval size,
								// start forcing straight bisection to limit the worst case
								force_bisect = end - begin > d0 * 2;

								// Don't let pcm_end get out of range!
								// That could happen with an invalid timestamp
								if ((Op_Granpos_Cmp(pcm_end, gp) > 0) && (Op_Granpos_Cmp(pcm_start, gp) <= 0))
									pcm_end = gp;

								break;
							}
						}
					}
				}
			}

			// Found our page
			//
			// Seek, if necessary.
			// If we were buffering data from a continued packet, we should be able to
			// continue to scan forward to get the rest of the data (even if
			// page_offset==-1).
			// Otherwise, we need to seek back to best_start
			if (!buffering)
			{
				if (best_start != page_offset)
				{
					page_offset = -1;

					ret = Op_Seek_Helper(best_start);
					if (ret < 0)
						return ret;
				}

				if (best_start < best)
				{
					// Retrieve the page at best_start, if we do not already have it
					if (page_offset < 0)
					{
						page_offset = Op_Get_Next_Page(ref og, link.end_offset);

						if (page_offset < (ogg_int64_t)OpusFileError.False)
							return (c_int)page_offset;

						if (page_offset != best_start)
							return (c_int)OpusFileError.BadLink;
					}

					Op_Buffer_Continued_Data(og);

					page_offset = -1;
				}
			}

			// Update prev_packet_gp to allow per-packet granule position assignment
			_of.prev_packet_gp = best_gp;
			_of.prev_page_offset = best_start;

			ret = Op_Fetch_And_Process_Page(page_offset < 0 ? null : og, page_offset, false, true);
			if (ret < 0)
				return (c_int)OpusFileError.BadLink;

			// Verify result
			if (Op_Granpos_Cmp(_of.prev_packet_gp, _target_gp) > 0)
				return (c_int)OpusFileError.BadLink;

			// Our caller will set cur_discard_count to handle pre-roll
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Convert a PCM offset relative to the start of the whole stream to
		/// a granule position in an individual link
		/// </summary>
		/********************************************************************/
		private ogg_int64_t Op_Get_Granulepos(ogg_int64_t _pcm_offset, out c_int _li)
		{
			c_int nlinks = _of.nlinks;
			Pointer<OggOpusLink> links = _of.links;
			c_int li_lo = 0;
			c_int li_hi = nlinks;

			do
			{
				c_int li = li_lo + (li_hi - li_lo >> 1);

				if (links[li].pcm_file_offset <= _pcm_offset)
					li_lo = li;
				else
					li_hi = li;
			}
			while ((li_hi - li_lo) > 1);

			_pcm_offset -= links[li_lo].pcm_file_offset;
			ogg_int64_t pcm_start = links[li_lo].pcm_start;
			opus_int32 pre_skip = (opus_int32)links[li_lo].head.Pre_Skip;

			Op_Granpos_Diff(out ogg_int64_t duration, links[li_lo].pcm_end, pcm_start);
			duration -= pre_skip;

			if (_pcm_offset >= duration)
			{
				_li = 0;
				return -1;
			}

			_pcm_offset += pre_skip;

			if (pcm_start > (Internal.Op_Int64_Max - _pcm_offset))
			{
				// Adding this amount to the granule position would overflow the positive
				// half of its 64-bit range
				_pcm_offset -= Internal.Op_Int64_Max - pcm_start + 1;
				pcm_start = Internal.Op_Int64_Min;
			}

			pcm_start += _pcm_offset;
			_li = li_lo;

			return pcm_start;
		}



		/********************************************************************/
		/// <summary>
		/// Seek to the specified PCM offset, such that decoding will begin
		/// at exactly the requested position
		/// </summary>
		/********************************************************************/
		public c_int Op_Pcm_Seek(ogg_int64_t _pcm_offset)
		{
			if (_of.ready_state < State.Opened)
				return (c_int)OpusFileError.Inval;

			if (!_of.seekable)
				return (c_int)OpusFileError.NoSeek;

			if (_pcm_offset < 0)
				return (c_int)OpusFileError.Inval;

			ogg_int64_t target_gp = Op_Get_Granulepos(_pcm_offset, out c_int li);
			if (target_gp == -1)
				return (c_int)OpusFileError.Inval;

			OggOpusLink link = _of.links[li];
			ogg_int64_t pcm_start = link.pcm_start;

			Op_Granpos_Diff(out _pcm_offset, target_gp, pcm_start);

			// For small (90 ms or less) forward seeks within the same link, just decode
			// forward.
			// This also optimizes the case of seeking to the current position
			if ((li == _of.cur_link) && (_of.ready_state >= State.InitSet))
			{
				ogg_int64_t gp = _of.prev_packet_gp;

				if (gp != -1)
				{
					c_int nbuffered = Internal.Op_Max(_of.od_buffer_size - _of.od_buffer_pos, 0);
					Op_Granpos_Add(out gp, gp, -nbuffered);

					// We do _not_ add cur_discard_count to gp.
					// Otherwise the total amount to discard could grow without bound, and it
					// would be better just to do a full seek
					if (Op_Granpos_Diff(out ogg_int64_t discard_count, target_gp, gp) == 0)
					{
						// We use a threshold of 90 ms instead of 80, since 80 ms is the
						// _minimum_ we would have discarded after a full seek.
						// Assuming 20 ms frames (the default), we'd discard 90 ms on average
						if ((discard_count >= 0) && (discard_count < 90 * 48))
						{
							_of.cur_discard_count = (opus_int32)discard_count;

							return 0;
						}
					}
				}
			}

			c_int ret = Op_Pcm_Seek_Page(target_gp, li);
			if (ret < 0)
				return ret;

			// Now skip samples until we actually get to our target.
			// Figure out where we should skip to
			ogg_int64_t skip;

			if (_pcm_offset <= link.head.Pre_Skip)
				skip = 0;
			else
				skip = Internal.Op_Max(_pcm_offset - 80 * 48, 0);

			// Skip packets until we find one with samples past our skip target
			ogg_int64_t prev_packet_gp;
			ogg_int64_t diff;

			for (;;)
			{
				c_int op_pos;

				c_int op_count = _of.op_count;
				prev_packet_gp = _of.prev_packet_gp;

				for (op_pos = _of.op_pos; op_pos < op_count; op_pos++)
				{
					ogg_int64_t cur_packet_gp = _of.op[op_pos].GranulePos;

					if ((Op_Granpos_Diff(out diff, cur_packet_gp, pcm_start) == 0) && (diff > skip))
						break;

					prev_packet_gp = cur_packet_gp;
				}

				_of.prev_packet_gp = prev_packet_gp;
				_of.op_pos = op_pos;

				if (op_pos < op_count)
					break;

				// We skipped all the packets on this page.
				// Fetch another
				ret = Op_Fetch_And_Process_Page(null, -1, false, true);
				if (ret < 0)
					return (c_int)OpusFileError.BadLink;
			}

			// We skipped too far, or couldn't get within 2 billion samples of the target.
			// Either the timestamps were illegal or there was a hole in the data
			if ((Op_Granpos_Diff(out diff, prev_packet_gp, pcm_start) != 0) || (diff > skip) || ((_pcm_offset - diff) >= Internal.Op_Int32_Max))
				return (c_int)OpusFileError.BadLink;

			// TODO: If there are further holes/illegal timestamps, we still won't decode
			// to the correct sample.
			// However, at least op_pcm_tell() will report the correct value immediately
			// after returning
			_of.cur_discard_count = (opus_int32)(_pcm_offset - diff);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Allocate the decoder scratch buffer.
		/// This is done lazily, since if the user provides large enough
		/// buffers, we'll never need it
		/// </summary>
		/********************************************************************/
		private c_int Op_Init_Buffer()
		{
			c_int nchannels_max;

			if (_of.seekable)
			{
				Pointer<OggOpusLink> links = _of.links;
				c_int nlinks = _of.nlinks;
				nchannels_max = 1;

				for (c_int li = 0; li < nlinks; li++)
					nchannels_max = Internal.Op_Max(nchannels_max, links[li].head.Channel_Count);
			}
			else
				nchannels_max = Constants.Op_NChannels_Max;

			_of.od_buffer = Memory.Ogg_MAlloc<op_sample>((size_t)nchannels_max * 120 * 48);
			if (_of.od_buffer.IsNull)
				return (c_int)OpusFileError.Fault;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Decode a single packet into the target buffer
		/// </summary>
		/********************************************************************/
		private c_int Op_Decode(Pointer<op_sample> _pcm, Ogg_Packet _op, c_int _nsamples, c_int _nchannels)
		{
			c_int ret;

			// First we try using the application-provided decode callback
			if (_of.decode_cb != null)
				ret = _of.decode_cb(_of.decode_cb_ctx, _of.od, _pcm.Buffer, _op, _nsamples, _nchannels, DecodeFormat.Float, _of.cur_link);
			else
				ret = Constants.Op_Dec_Use_Default;

			// If the application didn't want to handle decoding, do it ourselves
			if (ret == Constants.Op_Dec_Use_Default)
//TNE				ret = Opus_Multistream_Decode(_of.od, _op.Packet, _op.Bytes, _pcm, _nsamples, false);
				ret = _of.od.Decode_Float(_op.Packet, _op.Bytes, _pcm, _nsamples, false);
			// If the application returned a positive value other than 0 or
			// OP_DEC_USE_DEFAULT, fail
			else if (ret > 0)
				return (c_int)OpusFileError.BadPacket;

			if (ret < 0)
				return (c_int)OpusFileError.BadPacket;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Read more samples from the stream, using the same API as
		/// op_read() or op_read_float()
		/// </summary>
		/********************************************************************/
		private c_int Op_Read_Native(Pointer<op_sample> _pcm, c_int _buf_size, out c_int _li)
		{
			_li = 0;

			if (_of.ready_state < State.Opened)
				return (c_int)OpusFileError.Inval;

			for (;;)
			{
				c_int ret;

				if (_of.ready_state >= State.InitSet)
				{
					c_int nchannels = _of.links[_of.seekable ? _of.cur_link : 0].head.Channel_Count;
					c_int od_buffer_pos = _of.od_buffer_pos;
					c_int nsamples = _of.od_buffer_size - od_buffer_pos;

					// If we have buffered samples, return them
					if (nsamples > 0)
					{
						if ((nsamples * nchannels) > _buf_size)
							nsamples = _buf_size / nchannels;

						// Check nsamples again so we don't pass NULL to memcpy() if _buf_size
						// is zero.
						// That would technically be undefined behavior, even if the number of
						// bytes to copy were zero
						if (nsamples > 0)
						{
							CMemory.MemCpy(_pcm, _of.od_buffer + nchannels * od_buffer_pos, nchannels * nsamples);

							od_buffer_pos += nsamples;
							_of.od_buffer_pos = od_buffer_pos;
						}

						_li = _of.cur_link;

						return nsamples;
					}

					// If we have buffered packets, decode one
					c_int op_pos = _of.op_pos;

					if (op_pos < _of.op_count)
					{
						Ogg_Packet pop = _of.op[op_pos++];
						_of.op_pos = op_pos;

						opus_int32 cur_discard_count = _of.cur_discard_count;
						c_int duration = Op_Get_Packet_Duration(pop.Packet, pop.Bytes);
						c_int trimmed_duration = duration;

						// Perform end-trimming
						if (pop.Eos)
						{
							if (Op_Granpos_Cmp(pop.GranulePos, _of.prev_packet_gp) <= 0)
								trimmed_duration = 0;
							else if (Op_Granpos_Diff(out ogg_int64_t diff, pop.GranulePos, _of.prev_packet_gp) == 0)
								trimmed_duration = (c_int)Internal.Op_Min(diff, trimmed_duration);
						}

						_of.prev_packet_gp = pop.GranulePos;

						if ((duration * nchannels) > _buf_size)
						{
							// If the user's buffer is too small, decode into a scratch buffer
							Pointer<op_sample> buf = _of.od_buffer;

							if (buf.IsNull)
							{
								ret = Op_Init_Buffer();
								if (ret < 0)
									return ret;

								buf = _of.od_buffer;
							}

							ret = Op_Decode(buf, pop, duration, nchannels);
							if (ret < 0)
								return ret;

							// Perform pre-skip/pre-roll
							od_buffer_pos = Internal.Op_Min(trimmed_duration, cur_discard_count);
							cur_discard_count -= od_buffer_pos;

							_of.cur_discard_count = cur_discard_count;
							_of.od_buffer_pos = od_buffer_pos;
							_of.od_buffer_size = trimmed_duration;

							// Update bitrate tracking based on the actual samples we used from
							// what was decoded
							_of.bytes_tracked += pop.Bytes;
							_of.samples_tracked += trimmed_duration - od_buffer_pos;
						}
						else
						{
							// Otherwise decode directly into the user's buffer
							ret = Op_Decode(_pcm, pop, duration, nchannels);
							if (ret < 0)
								return ret;

							if (trimmed_duration > 0)
							{
								// Perform pre-skip/pre-roll
								od_buffer_pos = Internal.Op_Min(trimmed_duration, cur_discard_count);
								cur_discard_count -= od_buffer_pos;
								_of.cur_discard_count = cur_discard_count;
								trimmed_duration -= od_buffer_pos;

								if ((trimmed_duration > 0) && (od_buffer_pos > 0))
									CMemory.MemMove(_pcm, _pcm + od_buffer_pos * nchannels, trimmed_duration * nchannels);

								// Update bitrate tracking based on the actual samples we used from
								// what was decoded
								_of.bytes_tracked += pop.Bytes;
								_of.samples_tracked += trimmed_duration;

								if (trimmed_duration > 0)
								{
									_li = _of.cur_link;

									return trimmed_duration;
								}
							}
						}

						// Don't grab another page yet.
						// This one might have more packets, or might have buffered data now
						continue;
					}
				}

				// Suck in another page
				ret = Op_Fetch_And_Process_Page(null, -1, true, false);
				if (ret == (c_int)OpusFileError.EOF)
				{
					_li = _of.cur_link;

					return 0;
				}

				if (ret < 0)
					return ret;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Reads more samples from the stream.
		/// Although _buf_size must indicate the total number of values that
		/// can be stored in _pcm, the return value is the number of samples
		/// per channel.
		///
		/// - The channel count cannot be known a priori (reading more
		///   samples might advance us into the next link, with a different
		///   channel count), so _buf_size cannot also be in units of samples
		///   per channel,
		/// - Returning the samples per channel matches the libopus API as
		///   closely as we're able,
		/// - Returning the total number of values instead of samples per
		///   channel would mean the caller would need a division to
		///   compute the samples per channel, and might worry about the
		///   possibility of getting back samples for some channels and not
		///   others, and
		/// - This approach is relatively fool-proof: if an application
		///   passes too small a value to _buf_size, they will simply get
		///   fewer samples back, and if they assume the return value is the
		///   total number of values, then they will simply read too few
		///   (rather than reading too many and going off the end of the
		///   buffer)
		/// </summary>
		/********************************************************************/
		public c_int Op_Read_Float(Pointer<c_float> _pcm, c_int _buf_size, out c_int _li)
		{
			_of.state_channel_count = 0;

			return Op_Read_Native(_pcm, _buf_size, out _li);
		}
	}
}
