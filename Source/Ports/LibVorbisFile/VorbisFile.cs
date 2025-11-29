/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOgg;
using Polycode.NostalgicPlayer.Ports.LibOgg.Containers;
using Polycode.NostalgicPlayer.Ports.LibVorbis;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Containers;
using Polycode.NostalgicPlayer.Ports.LibVorbisFile.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibVorbisFile
{
	/// <summary>
	/// 
	/// </summary>
	public class VorbisFile
	{
		private const int ChunkSize = 65536;		// Greater-than-page-size granularity seeking
		private const int ReadSize = 2048;			// A smaller read size is needed for low-rate streaming

		private readonly OggVorbisFile vf;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private VorbisFile(OggVorbisFile vf)
		{
			this.vf = vf;
		}



		/********************************************************************/
		/// <summary>
		/// Read a little more data from the file/pipe into the ogg_sync
		/// framer
		/// </summary>
		/********************************************************************/
		private c_long Get_Data()
		{
			if (vf.callbacks.Read_Func == null)
				return -1;

			if (vf.datasource != null)
			{
				CPointer<byte> buffer = vf.oy.Buffer(ReadSize);
				c_long bytes = (c_long)vf.callbacks.Read_Func(buffer, 1, ReadSize, vf.datasource);

				if (bytes > 0)
					vf.oy.Wrote(bytes);

				return bytes;
			}
			else
				return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Save a tiny smidge of verbosity to make the code more readable
		/// </summary>
		/********************************************************************/
		private c_int Seek_Helper(ogg_int64_t offset)
		{
			if (vf.datasource != null)
			{
				// Only seek if the file position isn't already there
				if (vf.offset != offset)
				{
					if ((vf.callbacks.Seek_Func == null) || (vf.callbacks.Seek_Func(vf.datasource, offset, SeekOrigin.Begin) == -1))
						return (c_int)VorbisError.Read;

					vf.offset = offset;
					vf.oy.Reset();
				}
			}
			else
			{
				// Shouldn't happen unless someone writes a broken callback
				return (c_int)VorbisError.Fault;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// From the head of the stream, get the next page. Boundary
		/// specifies if the function is allowed to fetch more data from the
		/// stream (and how much) or only use internally buffered data.
		///
		/// boundary: -1) unbounded search
		///            0) read no additional data; use cached only
		///            n) search for a new page beginning for n bytes
		///
		/// return: less than 0) did not find a page (OV_FALSE, OV_EOF, OV_EREAD)
		///         n) found a page at absolute offset n
		/// </summary>
		/********************************************************************/
		private ogg_int64_t Get_Next_Page(ref OggPage og, ogg_int64_t boundary)
		{
			if (boundary > 0)
				boundary += vf.offset;

			while (true)
			{
				if ((boundary > 0) && (vf.offset >= boundary))
					return (ogg_int64_t)VorbisError.False;

				c_long more = vf.oy.PageSeek(out og);

				if (more < 0)
				{
					// Skipped n bytes
					vf.offset -= more;
				}
				else
				{
					if (more == 0)
					{
						// Send more paramedics
						if (boundary == 0)
							return (ogg_int64_t)VorbisError.False;

						{
							c_long ret = Get_Data();

							if (ret == 0)
								return (ogg_int64_t)VorbisError.Eof;

							if (ret < 0)
								return (ogg_int64_t)VorbisError.Read;
						}
					}
					else
					{
						// Got a page. Return the offset at the page beginning,
						// advance the internal offset past the page end
						ogg_int64_t ret = vf.offset;
						vf.offset += more;

						return ret;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Find the latest page beginning before the passed in position.
		/// Much dirtier than the above as Ogg doesn't have any backward
		/// search linkage. No 'readp' as it will certainly have to read.
		/// Returns offset or OV_EREAD, OV_FAULT
		/// </summary>
		/********************************************************************/
		private ogg_int64_t Get_Prev_Page(ogg_int64_t begin, OggPage og)
		{
			ogg_int64_t end = begin;
			ogg_int64_t offset = -1;

			while (offset == -1)
			{
				begin -= ChunkSize;

				if (begin < 0)
					begin = 0;

				ogg_int64_t ret = Seek_Helper(begin);
				if (ret != 0)
					return ret;

				while (vf.offset < end)
				{
					ret = Get_Next_Page(ref og, end - vf.offset);

					if (ret == (ogg_int64_t)VorbisError.Read)
						return (ogg_int64_t)VorbisError.Read;

					if (ret < 0)
						break;
					else
						offset = ret;
				}
			}

			// In a fully compliant, non-multiplexed stream, we'll still be
			// holding the last page. In multiplexed (or noncompliant streams),
			// we will probably have to re-read the last page we saw
			if (og.Page.HeaderLen == 0)
			{
				ogg_int64_t ret = Seek_Helper(offset);
				if (ret != 0)
					return ret;

				ret = Get_Next_Page(ref og, ChunkSize);
				if (ret < 0)
				{
					// This shouldn't be possible
					return (ogg_int64_t)VorbisError.Fault;
				}
			}

			return offset;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Add_Serialno(OggPage og, ref CPointer<c_long> serialno_list, ref c_int n)
		{
			c_long s = og.SerialNo();
			n++;

			if (serialno_list.IsNotNull)
				serialno_list = Memory.Ogg_Realloc(serialno_list, (size_t)n);
			else
				serialno_list = Memory.Ogg_MAlloc<c_long>(1);

			serialno_list[n - 1] = s;
		}



		/********************************************************************/
		/// <summary>
		/// Returns nonzero if found
		/// </summary>
		/********************************************************************/
		private c_int Lookup_Serialno(c_long s, CPointer<c_long> serialno_list, c_int n)
		{
			if (serialno_list.IsNotNull)
			{
				while (n-- != 0)
				{
					if (serialno_list[0] == s)
						return 1;

					serialno_list++;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Lookup_Page_Serialno(OggPage og, CPointer<c_long> serialno_list, c_int n)
		{
			c_long s = og.SerialNo();

			return Lookup_Serialno(s, serialno_list, n);
		}



		/********************************************************************/
		/// <summary>
		/// Performs the same search as _get_prev_page, but prefers pages of
		/// the specified serial number. If a page of the specified serialno
		/// is spotted during the seek-back-and-read-forward, it will return
		/// the info of last page of the matching serial number instead of
		/// the very last page. If no page of the specified serialno is seen,
		/// it will return the info of last page and alter *serialno
		/// </summary>
		/********************************************************************/
		private ogg_int64_t Get_Prev_Page_Serial(ogg_int64_t begin, CPointer<c_long> serial_list, c_int serial_n, ref c_int serialno, ref ogg_int64_t granpos)
		{
			OggPage og = null;
			ogg_int64_t end = begin;

			ogg_int64_t prefoffset = -1;
			ogg_int64_t offset = -1;
			ogg_int64_t ret_serialno = -1;
			ogg_int64_t ret_gran = -1;

			while (offset == -1)
			{
				begin -= ChunkSize;
				if (begin < 0)
					begin = 0;

				ogg_int64_t ret = Seek_Helper(begin);
				if (ret != 0)
					return ret;

				while (vf.offset < end)
				{
					ret = Get_Next_Page(ref og, end - vf.offset);
					if (ret == (ogg_int64_t)VorbisError.Read)
						return (ogg_int64_t)VorbisError.Read;

					if (ret < 0)
						break;
					else
					{
						ret_serialno = og.SerialNo();
						ret_gran = og.GranulePos();
						offset = ret;

						if (ret_serialno == serialno)
						{
							prefoffset = ret;
							granpos = ret_gran;
						}

						if (Lookup_Serialno((c_long)ret_serialno, serial_list, serial_n) == 0)
						{
							// We fell off the end of the link, which means we seeked
							// back too far and shouldn't have been looking in that link
							// to begin with. If we found the preferred serial number,
							// forget that we saw it
							prefoffset = -1;
						}
					}
				}

				// We started from the beginning of the stream and found nothing.
				// This should be impossible unless the contents of the stream changed out
				// from under us after we read from it
				if ((begin == 0) && (vf.offset < 0))
					return (ogg_int64_t)VorbisError.BadLink;
			}

			// We're not interested in the page... just the serialno and granpos
			if (prefoffset >= 0)
				return prefoffset;

			serialno = (c_int)ret_serialno;
			granpos = ret_gran;

			return offset;
		}



		/********************************************************************/
		/// <summary>
		/// Uses the local ogg_stream storage in vf; this is important for
		/// non-streaming input sources
		/// </summary>
		/********************************************************************/
		private c_int Fetch_Headers(VorbisInfo vi, VorbisComment vc, ref CPointer<c_long> serialno_list, ref c_int serialno_n, OggPage og_ptr)
		{
			return Fetch_Headers(vi, vc, true, ref serialno_list, ref serialno_n, og_ptr);
		}



		/********************************************************************/
		/// <summary>
		/// Uses the local ogg_stream storage in vf; this is important for
		/// non-streaming input sources
		/// </summary>
		/********************************************************************/
		private c_int Fetch_Headers(VorbisInfo vi, VorbisComment vc, OggPage og_ptr)
		{
			CPointer<c_long> serialno_list = new CPointer<c_long>();
			c_int serialno_n = 0;

			return Fetch_Headers(vi, vc, false, ref serialno_list, ref serialno_n, og_ptr);
		}



		/********************************************************************/
		/// <summary>
		/// Uses the local ogg_stream storage in vf; this is important for
		/// non-streaming input sources
		/// </summary>
		/********************************************************************/
		private c_int Fetch_Headers(VorbisInfo vi, VorbisComment vc, bool hasSerialnos, ref CPointer<c_long> serialno_list, ref c_int serialno_n, OggPage og_ptr)
		{
			c_int ret;
			bool allbos = false;

			if (og_ptr == null)
			{
				OggPage og = null;

				ogg_int64_t llret = Get_Next_Page(ref og, ChunkSize);
				if (llret == (ogg_int64_t)VorbisError.Read)
					return (c_int)VorbisError.Read;

				if (llret < 0)
					return (c_int)VorbisError.NotVorbis;

				og_ptr = og;
			}

			Info.Vorbis_Info_Init(vi);
			Info.Vorbis_Comment_Init(vc);

			vf.ready_state = State.Opened;

			// Extract the serialnos of all BOS pages + the first set of vorbis
			// headers we see in the link
			while (og_ptr.Bos())
			{
				if (hasSerialnos)
				{
					if (Lookup_Page_Serialno(og_ptr, serialno_list, serialno_n) != 0)
					{
						// A dupe serialnumber in an initial header packet set == invalid stream
						if (serialno_list.IsNotNull)
							Memory.Ogg_Free(serialno_list);

						serialno_list.SetToNull();
						serialno_n = 0;

						ret = (c_int)VorbisError.BadHeader;
						goto Bail_Header;
					}

					Add_Serialno(og_ptr, ref serialno_list, ref serialno_n);
				}

				if (vf.ready_state < State.StreamSet)
				{
					// We don't have a vorbis stream in this link yet, so begin
					// prospective stream setup. We need a stream to get packets
					vf.os.Reset_SerialNo(og_ptr.SerialNo());
					vf.os.PageIn(og_ptr);

					if ((vf.os.PacketOut(out Ogg_Packet op) > 0) && (Info.Vorbis_Synthesis_Idheader(op) != 0))
					{
						// Vorbis header; continue setup
						vf.ready_state = State.StreamSet;

						ret = Info.Vorbis_Synthesis_Headerin(vi, vc, op);
						if (ret != 0)
						{
							ret = (c_int)VorbisError.BadHeader;
							goto Bail_Header;
						}
					}
				}

				// Get next page
				{
					ogg_int64_t llret = Get_Next_Page(ref og_ptr, ChunkSize);

					if (llret == (ogg_int64_t)VorbisError.Read)
					{
						ret = (c_int)VorbisError.Read;
						goto Bail_Header;
					}

					if (llret < 0)
					{
						ret = (c_int)VorbisError.NotVorbis;
						goto Bail_Header;
					}

					// If this page also belongs to our vorbis stream, submit it and break
					if ((vf.ready_state == State.StreamSet) && (vf.os.State.SerialNo == og_ptr.SerialNo()))
					{
						vf.os.PageIn(og_ptr);
						break;
					}
				}
			}

			if (vf.ready_state != State.StreamSet)
			{
				ret = (c_int)VorbisError.NotVorbis;
				goto Bail_Header;
			}

			while (true)
			{
				c_int i = 0;

				while (i < 2)		// Get a page loop
				{
					while (i < 2)	// Get a packet loop
					{
						c_int result = vf.os.PacketOut(out Ogg_Packet op);

						if (result == 0)
							break;

						if (result == -1)
						{
							ret = (c_int)VorbisError.BadHeader;
							goto Bail_Header;
						}

						ret = Info.Vorbis_Synthesis_Headerin(vi, vc, op);
						if (ret != 0)
							goto Bail_Header;

						i++;
					}

					while (i < 2)
					{
						if (Get_Next_Page(ref og_ptr, ChunkSize) < 0)
						{
							ret = (c_int)VorbisError.BadHeader;
							goto Bail_Header;
						}

						// If this page belongs to the correct stream, go parse it
						if (vf.os.State.SerialNo == og_ptr.SerialNo())
						{
							vf.os.PageIn(og_ptr);
							break;
						}

						// If we never see the final vorbis headers before the link
						// ends, abort
						if (og_ptr.Bos())
						{
							if (allbos)
							{
								ret = (c_int)VorbisError.BadHeader;
								goto Bail_Header;
							}
							else
								allbos = true;
						}

						// Otherwise, keep looking
					}
				}

				return 0;
			}

			Bail_Header:
			Info.Vorbis_Info_Clear(vi);
			Info.Vorbis_Comment_Clear(vc);

			vf.ready_state = State.Opened;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Starting from the current cursor position, get initial PCM offset
		/// of next page. Consumes the page in the process without decoding
		/// audio, however this is only called during stream parsing upon
		/// seekable open
		/// </summary>
		/********************************************************************/
		private ogg_int64_t Initial_Pcmoffset(VorbisInfo vi)
		{
			OggPage og = null;
			ogg_int64_t accumulated = 0;
			c_long lastblock = -1;
			c_int serialno = (c_int)vf.os.State.SerialNo;

			while (true)
			{
				if (Get_Next_Page(ref og, -1) < 0)
					break;	// Should not be possible unless the file is truncated/mangled

				if (og.Bos())
					break;

				if (og.SerialNo() != serialno)
					continue;

				// Count blocksizes of all frames in the page
				vf.os.PageIn(og);

				c_int result;
				while ((result = vf.os.PacketOut(out Ogg_Packet op)) != 0)
				{
					if (result > 0)		// Ignore holes
					{
						c_long thisblock = Synthesis.Vorbis_Packet_Blocksize(vi, op);
						if (thisblock >= 0)
						{
							if (lastblock != -1)
								accumulated += (lastblock + thisblock) >> 2;

							lastblock = thisblock;
						}
					}
				}

				if (og.GranulePos() != -1)
				{
					// PCM offset of last packet on the first audio page
					accumulated = og.GranulePos() - accumulated;
					break;
				}
			}

			// Less than zero? Either a corrupt file or a stream with samples
			// trimmed off the beginning, a normal occurrence; in both cases set
			// the offset to zero
			if (accumulated < 0)
				accumulated = 0;

			return accumulated;
		}



		/********************************************************************/
		/// <summary>
		/// Finds each bitstream link one at a time using a bisection search
		/// (has to begin by knowing the offset of the lb's initial page).
		/// Recurses for each link so it can alloc the link storage after
		/// finding them all, then unroll and fill the cache at the same
		/// time
		/// </summary>
		/********************************************************************/
		private c_int Bisect_Forward_Serialno(ogg_int64_t begin, ogg_int64_t searched, ogg_int64_t end, ogg_int64_t endgran, c_int endserial, CPointer<c_long> currentno_list, c_int currentnos, c_long m)
		{
			ogg_int64_t dataoffset = searched;
			ogg_int64_t endsearched = end;
			ogg_int64_t next = end;
			ogg_int64_t searchgran = -1;
			OggPage og = null;
			ogg_int64_t ret;

			c_int serialno = (c_int)vf.os.State.SerialNo;

			// Invariants:
			// We have the headers and serialnos for the link beginning at 'begin'
			// We have the offset and granpos of the last page in the file (potentially
			//   not a page we care about)
			//
			// Is the last page in our list of current serialnumbers?
			if (Lookup_Serialno(endserial, currentno_list, currentnos) != 0)
			{
				// Last page is in the starting serialno list, so we've bisected
				// down to (or just started with) a single link. Now we need to
				// find the last vorbis page belonging to the first vorbis stream
				// for this link
				searched = end;

				while (endserial != serialno)
				{
					endserial = serialno;
					searched = Get_Prev_Page_Serial(searched, currentno_list, currentnos, ref endserial, ref endgran);
				}

				vf.links = (c_int)(m + 1);

				if (vf.offsets.IsNotNull)
					Memory.Ogg_Free(vf.offsets);

				if (vf.serialnos.IsNotNull)
					Memory.Ogg_Free(vf.serialnos);

				if (vf.dataoffsets.IsNotNull)
					Memory.Ogg_Free(vf.dataoffsets);

				vf.offsets = Memory.Ogg_MAlloc<ogg_int64_t>((size_t)vf.links + 1);
				vf.vi = Memory.Ogg_ReallocObj(vf.vi, (size_t)vf.links);
				vf.vc = Memory.Ogg_ReallocObj(vf.vc, (size_t)vf.links);
				vf.serialnos = Memory.Ogg_MAlloc<c_long>((size_t)vf.links);
				vf.dataoffsets = Memory.Ogg_MAlloc<ogg_int64_t>((size_t)vf.links);
				vf.pcmlengths = Memory.Ogg_MAlloc<ogg_int64_t>((size_t)vf.links * 2);

				vf.offsets[m + 1] = end;
				vf.offsets[m] = begin;
				vf.pcmlengths[(m * 2) + 1] = endgran < 0 ? 0 : endgran;
			}
			else
			{
				// Last page is not in the starting stream's serial number list,
				// so we have multiple links. Find where the stream that begins
				// our bisection ends
				CPointer<c_long> next_serialno_list = null;
				c_int next_serialnos = 0;
				VorbisInfo vi = new VorbisInfo();
				VorbisComment vc = new VorbisComment();
				c_int testserial = serialno + 1;

				// The below guards against garbage separating the last and
				// first pages of two links
				while (searched < endsearched)
				{
					ogg_int64_t bisect;

					if ((endsearched - searched) < ChunkSize)
						bisect = searched;
					else
						bisect = (searched + endsearched) / 2;

					ret = Seek_Helper(bisect);
					if (ret != 0)
						return (c_int)ret;

					ogg_int64_t last = Get_Next_Page(ref og, -1);

					if (last == (ogg_int64_t)VorbisError.Read)
						return (c_int)VorbisError.Read;

					if ((last < 0) || (Lookup_Page_Serialno(og, currentno_list, currentnos) == 0))
					{
						endsearched = bisect;

						if (last >= 0)
							next = last;
					}
					else
						searched = vf.offset;
				}

				// Bisection point found
				// For the time being, fetch end PCM offset the simple way
				searched = next;

				while (testserial != serialno)
				{
					testserial = serialno;
					searched = Get_Prev_Page_Serial(searched, currentno_list, currentnos, ref testserial, ref searchgran);
				}

				ret = Seek_Helper(next);
				if (ret != 0)
					return (c_int)ret;

				ret = Fetch_Headers(vi, vc, ref next_serialno_list, ref next_serialnos, null);
				if (ret != 0)
					return (c_int)ret;

				serialno = (c_int)vf.os.State.SerialNo;
				dataoffset = vf.offset;

				// This will consume a page, however the next bisection always
				// starts with a raw seek
				ogg_int64_t pcmoffset = Initial_Pcmoffset(vi);

				ret = Bisect_Forward_Serialno(next, vf.offset, end, endgran, endserial, next_serialno_list, next_serialnos, m + 1);
				if (ret != 0)
					return (c_int)ret;

				if (next_serialno_list.IsNotNull)
					Memory.Ogg_Free(next_serialno_list);

				vf.offsets[m + 1] = next;
				vf.serialnos[m + 1] = serialno;
				vf.dataoffsets[m + 1] = dataoffset;

				vf.vi[m + 1] = vi;
				vf.vc[m + 1] = vc;

				vf.pcmlengths[m * 2 + 1] = searchgran;
				vf.pcmlengths[m * 2 + 2] = pcmoffset;
				vf.pcmlengths[m * 2 + 3] -= pcmoffset;

				if (vf.pcmlengths[m * 2 + 3] < 0)
					vf.pcmlengths[m * 2 + 3] = 0;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Make_Decode_Ready()
		{
			if (vf.ready_state > State.StreamSet)
				return 0;

			if (vf.ready_state < State.StreamSet)
				return (c_int)VorbisError.Fault;

			if (vf.seekable)
			{
				if (Block.Vorbis_Synthesis_Init(vf.vd, vf.vi[vf.current_link]) != 0)
					return (c_int)VorbisError.BadLink;
			}
			else
			{
				if (Block.Vorbis_Synthesis_Init(vf.vd, vf.vi[0]) != 0)
					return (c_int)VorbisError.BadLink;
			}

			Block.Vorbis_Block_Init(vf.vd, vf.vb);

			vf.ready_state = State.InitSet;
			vf.bittrack = 0.0f;
			vf.samptrack = 0.0f;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Open_Seekable2()
		{
			ogg_int64_t dataoffset = vf.dataoffsets[0];
			ogg_int64_t endgran = -1;

			c_int endserial = (c_int)vf.os.State.SerialNo;
			c_int serialno = (c_int)vf.os.State.SerialNo;

			// We're partially open and have a first link header state in
			// storage in vf
			//
			// Fetch initial PCM offset
			ogg_int64_t pcmoffset = Initial_Pcmoffset(vf.vi[0]);

			// We can seek, so set out learning all about this file
			if ((vf.callbacks.Seek_Func != null) && (vf.callbacks.Tell_Func != null))
			{
				vf.callbacks.Seek_Func(vf.datasource, 0, SeekOrigin.End);
				vf.offset = vf.end = vf.callbacks.Tell_Func(vf.datasource);
			}
			else
				vf.offset = vf.end = -1;

			// If seek_func is implemented, tell_func must also be implemented
			if (vf.end == -1)
				return (c_int)VorbisError.Inval;

			// Get the offset of the last page of the physical bitstream, or, if
			// we're lucky the last vorbis page of this link as most OggVorbis
			// files will contain a single logical bitstream
			ogg_int64_t end = Get_Prev_Page_Serial(vf.end, vf.serialnos + 2, (c_int)vf.serialnos[1], ref endserial, ref endgran);
			if (end < 0)
				return (c_int)end;

			// Now determine bitstream structure recursively
			if (Bisect_Forward_Serialno(0, dataoffset, end, endgran, endserial, vf.serialnos + 2, (c_int)vf.serialnos[1], 0) < 0)
				return (c_int)VorbisError.Read;

			vf.offsets[0] = 0;
			vf.serialnos[0] = serialno;
			vf.dataoffsets[0] = dataoffset;
			vf.pcmlengths[0] = pcmoffset;
			vf.pcmlengths[1] -= pcmoffset;

			if (vf.pcmlengths[1] < 0)
				vf.pcmlengths[1] = 0;

			return (c_int)Ov_Raw_Seek(dataoffset);
		}



		/********************************************************************/
		/// <summary>
		/// Clear out the current logical bitstream decoder
		/// </summary>
		/********************************************************************/
		private void Decode_Clear()
		{
			Block.Vorbis_Dsp_Clear(vf.vd);
			Block.Vorbis_Block_Clear(vf.vb);

			vf.ready_state = State.Opened;
		}



		/********************************************************************/
		/// <summary>
		/// Fetch and process a packet. Handles the case where we're at a
		/// bitstream boundary and dumps the decoding machine. If the
		/// decoding machine is unloaded, it loads it. It also keeps
		/// pcm_offset up to date (seek and read both use this. seek uses a
		/// special hack with readp).
		///
		/// return: less than 0) error, OV_HOLE (lost packet) or OV_EOF
		///         0) need more data (only if readp==0)
		///         1) got a packet
		/// </summary>
		/********************************************************************/
		private c_int Fetch_And_Process_Packet(out Ogg_Packet op_in, bool readp, bool spanp)
		{
			op_in = null;

			OggPage og = null;

			// Handle one packet. Try to fetch it from current stream state
			// extract packets from page
			while (true)
			{
				if (vf.ready_state == State.StreamSet)
				{
					c_int ret = Make_Decode_Ready();
					if (ret < 0)
						return ret;
				}

				// Process a packet if we can

				if (vf.ready_state == State.InitSet)
				{
					c_int hs = Synthesis.Vorbis_Synthesis_Halfrate_P(vf.vi[0]);

					while (true)
					{
						c_int result = vf.os.PacketOut(out Ogg_Packet op_ptr);
						op_in = op_ptr;

						if (result == -1)
							return (c_int)VorbisError.Hole;

						if (result > 0)
						{
							// Got a packet. Process it
							ogg_int64_t granulepos = op_ptr.GranulePos;

							if (Synthesis.Vorbis_Synthesis(vf.vb, op_ptr) == 0)	// Lazy check for lazy header handling. The header packets aren't audio, so if/when we submit them, vorbis_synthesis will reject them
							{
								// Suck in the synthesis data and track bitrate
								{
									c_int oldsamples = Block.Vorbis_Synthesis_Pcmout(vf.vd);

									// For proper use of libvorbis within libvorbisfile,
									// oldsamples will always be zero
									if (oldsamples != 0)
										return (c_int)VorbisError.Fault;

									Block.Vorbis_Synthesis_Blockin(vf.vd, vf.vb);
									vf.samptrack += Block.Vorbis_Synthesis_Pcmout(vf.vd) << hs;
									vf.bittrack += op_ptr.Bytes * 8;
								}

								// Update the pcm offset
								if ((granulepos != -1) && !op_ptr.Eos)
								{
									c_int link = vf.seekable ? vf.current_link : 0;

									// This packet has a pcm_offset on it (the last packet
									// completed on a page carries the offset). After processing
									// (above), we know the pcm position of the *last* sample
									// ready to be returned. Find the offset of the *first*
									//
									// As an aside, this trick is inaccurate if we begin
									// reading anew right at the last page; the end-of-stream
									// granulepos declares the last frame in the stream, and the
									// last packet of the last page may be a partial frame.
									// So, we need a previous granulepos from an in-sequence page
									// to have a reference point. Thus the !op_ptr->e_o_s clause
									// above
									if (vf.seekable && (link > 0))
										granulepos -= vf.pcmlengths[link * 2];

									if (granulepos < 0)
										granulepos = 0;		// Actually, this shouldn't be possible here unless the stream is very broken

									c_int samples = Block.Vorbis_Synthesis_Pcmout(vf.vd) << hs;

									granulepos -= samples;

									for (c_int i = 0; i < link; i++)
										granulepos += vf.pcmlengths[i * 2 + 1];

									vf.pcm_offset = granulepos;
								}

								return 1;
							}
						}
						else
							break;
					}
				}

				if (vf.ready_state >= State.Opened)
				{
					while (true)
					{
						// The loop is not strictly necessary, but there's no sense in
						// doing the extra checks of the larger loop for the common
						// case in a multiplexed bistream where the page is simply
						// part of a different logical bitstream; keep reading until
						// we get one with the correct serialno
						if (!readp)
							return 0;

						ogg_int64_t ret = Get_Next_Page(ref og, -1);
						if (ret < 0)
							return (c_int)VorbisError.Eof;	// Eof. Leave uninitialized

						// Bitrate tracking; add the header's bytes here, the body bytes
						// are done by packet above
						vf.bittrack += og.Page.HeaderLen * 8;

						if (vf.ready_state == State.InitSet)
						{
							if (vf.current_serialno != og.SerialNo())
							{
								// Two possibilities:
								// 1) our decoding just traversed a bitstream boundary
								// 2) another stream is multiplexed into this logical section
								if (og.Bos())
								{
									// Boundary case
									if (!spanp)
										return (c_int)VorbisError.Eof;

									Decode_Clear();

									if (!vf.seekable)
									{
										Info.Vorbis_Info_Clear(vf.vi[0]);
										Info.Vorbis_Comment_Clear(vf.vc[0]);
									}

									break;
								}
								else
									continue;	// Possibility #2
							}
						}

						break;
					}
				}

				// Do we need to load a new machine before submitting the page?
				// This is different in the seekable and non-seekable cases.
				//
				// In the seekable case, we already have all the header
				// information loaded and cached; we just initialize the machine
				// with it and continue on our merry way.
				//
				// In the non-seekable (streaming) case, we'll only be at a
				// boundary if we just left the previous logical bitstream and
				// we're now nominally at the header of the next bitstream
				if (vf.ready_state != State.InitSet)
				{
					c_int link;

					if (vf.ready_state < State.StreamSet)
					{
						if (vf.seekable)
						{
							c_long serialno = og.SerialNo();

							// Match the serialno to bitstream section. We use this rather than
							// offset positions to avoid problems near logical bitstream
							// boundaries
							for (link = 0; link < vf.links; link++)
							{
								if (vf.serialnos[link] == serialno)
									break;
							}

							if (link == vf.links)
								continue;	// Not the desired Vorbis bitstream section; keep trying

							vf.current_serialno = serialno;
							vf.current_link = link;

							vf.os.Reset_SerialNo((c_int)vf.current_serialno);
							vf.ready_state = State.StreamSet;
						}
						else
						{
							// We're streaming
							// Fetch the three header packets, build the info struct
							c_int ret = Fetch_Headers(vf.vi[0], vf.vc[0], og);
							if (ret != 0)
								return ret;

							vf.current_serialno = vf.os.State.SerialNo;
							vf.current_link++;
							link = 0;
						}
					}
				}

				// The buffered page is the data we want, and we're ready for it;
				// add it to the stream state
				if (vf.seekable || (vf.ready_state == State.InitSet))	// TNE: Added to fix a bug when streaming
					vf.os.PageIn(og);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Ov_Open1(object f, out VorbisFile vorbisFile, CPointer<byte> initial, c_long ibytes, OvCallbacks callbacks)
		{
			c_int offsettest = ((f != null) && (callbacks.Seek_Func != null)) ? callbacks.Seek_Func(f, 0, SeekOrigin.Current) : -1;
			CPointer<c_long> serialno_list = null;
			c_int serialno_list_size = 0;

			OggVorbisFile vf = new OggVorbisFile();
			vorbisFile = new VorbisFile(vf);
			vf.datasource = f;
			vf.callbacks = callbacks;

			// Init frame state
			OggSync.Init(out vf.oy);

			// Perhaps some data was previously read into a buffer for testing
			// against other stream types. Allow initialization from this
			// previously read data (as we may be reading from a non-seekable
			// stream)
			if (initial.IsNotNull)
			{
				CPointer<byte> buffer = vf.oy.Buffer(ibytes);
				CMemory.memcpy(buffer, initial, (size_t)ibytes);
				vf.oy.Wrote(ibytes);
			}

			// Can we seek? Stevens suggests the seek test was portable
			if (offsettest != -1)
				vf.seekable = true;

			// No seeking yet; Set up a 'single' (current) logical bitstream
			// entry for partial open
			vf.links = 1;
			vf.vi = Memory.Ogg_CAllocObj<VorbisInfo>((size_t)vf.links);
			vf.vc = Memory.Ogg_CAllocObj<VorbisComment>((size_t)vf.links);

			OggStream.Init(out vf.os, -1); // Fill in the serialno later

			// Fetch all BOS pages, store the vorbis header and all seen serial
			// numbers, load subsequent vorbis setup headers
			c_int ret = vorbisFile.Fetch_Headers(vf.vi[0], vf.vc[0], ref serialno_list, ref serialno_list_size, null);
			if (ret < 0)
			{
				vf.datasource = null;
				vorbisFile.Ov_Clear();
			}
			else
			{
				// Serial number list for first link needs to be held somewhere
				// for second stage of seekable stream open; this saves having to
				// seek/reread first link's serialnumber data then
				vf.serialnos = Memory.Ogg_CAlloc<c_long>((size_t)serialno_list_size + 2);
				vf.serialnos[0] = vf.current_serialno = vf.os.State.SerialNo;
				vf.serialnos[1] = serialno_list_size;

				CMemory.memcpy(vf.serialnos + 2, serialno_list, (size_t)serialno_list_size);

				vf.offsets = Memory.Ogg_CAlloc<ogg_int64_t>(1);
				vf.dataoffsets = Memory.Ogg_CAlloc<ogg_int64_t>(1);
				vf.offsets[0] = 0;
				vf.dataoffsets[0] = vf.offset;

				vf.ready_state = State.PartOpen;
			}

			if (serialno_list.IsNotNull)
				Memory.Ogg_Free(serialno_list);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Ov_Open2()
		{
			if (vf.ready_state != State.PartOpen)
				return (c_int)VorbisError.Inval;

			vf.ready_state = State.Opened;

			if (vf.seekable)
			{
				c_int ret = Open_Seekable2();
				if (ret != 0)
				{
					vf.datasource = null;
					Ov_Clear();
				}

				return ret;
			}
			else
				vf.ready_state = State.StreamSet;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Clear out the OggVorbis_File struct
		/// </summary>
		/********************************************************************/
		public VorbisError Ov_Clear()
		{
			Block.Vorbis_Block_Clear(vf.vb);
			Block.Vorbis_Dsp_Clear(vf.vd);
			vf.os.Clear();

			if (vf.vi.IsNotNull && (vf.links != 0))
			{
				for (c_int i = 0; i < vf.links; i++)
				{
					Info.Vorbis_Info_Clear(vf.vi[i]);
					Info.Vorbis_Comment_Clear(vf.vc[i]);
				}

				Memory.Ogg_Free(vf.vi);
				Memory.Ogg_Free(vf.vc);
			}

			if (vf.dataoffsets.IsNotNull)
				Memory.Ogg_Free(vf.dataoffsets);

			if (vf.pcmlengths.IsNotNull)
				Memory.Ogg_Free(vf.pcmlengths);

			if (vf.serialnos.IsNotNull)
				Memory.Ogg_Free(vf.serialnos);

			if (vf.offsets.IsNotNull)
				Memory.Ogg_Free(vf.offsets);

			vf.oy.Clear();

			if ((vf.datasource != null) && (vf.callbacks.Close_Func != null))
				vf.callbacks.Close_Func(vf.datasource);

			return VorbisError.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Inspects the OggVorbis file and finds/documents all the logical
		/// bitstreams contained in it. Tries to be tolerant of logical
		/// bitstream sections that are truncated/woogie
		/// </summary>
		/********************************************************************/
		public static VorbisError Ov_Open_Callbacks(object f, out VorbisFile vorbisFile, CPointer<byte> initial, c_long ibytes, OvCallbacks callbacks)
		{
			c_int ret = Ov_Open1(f, out vorbisFile, initial, ibytes, callbacks);
			if (ret != 0)
				return (VorbisError)ret;

			return (VorbisError)vorbisFile.Ov_Open2();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static VorbisError Ov_Open(Stream f, bool leaveOpen, out VorbisFile vorbisFile, CPointer<byte> initial, c_long ibytes)
		{
			OvCallbacks callbacks = new OvCallbacks
			{
				Read_Func = StreamCallback.Read,
				Seek_Func = StreamCallback.Seek,
				Close_Func = leaveOpen ? null : StreamCallback.Close,
				Tell_Func = StreamCallback.Tell
			};

			return Ov_Open_Callbacks(f, out vorbisFile, initial, ibytes, callbacks);
		}



		/********************************************************************/
		/// <summary>
		/// Only partially open the vorbis file; test for Vorbisness, and
		/// load the headers for the first chain. Do not seek (although test
		/// for seekability). Use ov_test_open to finish opening the file,
		/// else ov_clear to close/free it. Same return codes as open.
		/// 
		/// Note that vorbisfile does _not_ take ownership of the file if the
		/// call fails; the calling application is responsible for closing
		/// the file if this call returns an error
		/// </summary>
		/********************************************************************/
		public static VorbisError Ov_Test_Callbacks(object f, out VorbisFile vorbisFile, CPointer<byte> initial, c_long ibytes, OvCallbacks callbacks)
		{
			return (VorbisError)Ov_Open1(f, out vorbisFile, initial, ibytes, callbacks);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static VorbisError Ov_Test(Stream f, bool leaveOpen, out VorbisFile vorbisFile, CPointer<byte> initial, c_long ibytes)
		{
			OvCallbacks callbacks = new OvCallbacks
			{
				Read_Func = StreamCallback.Read,
				Seek_Func = StreamCallback.Seek,
				Close_Func = leaveOpen ? null : StreamCallback.Close,
				Tell_Func = StreamCallback.Tell
			};

			return Ov_Test_Callbacks(f, out vorbisFile, initial, ibytes, callbacks);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the actual bitrate since last call. Returns -1 if no
		/// additional data to offer since last call (or at beginning of
		/// stream), EINVAL if stream is only partially open
		/// </summary>
		/********************************************************************/
		public c_long Ov_Bitrate_Instant()
		{
			c_int link = vf.seekable ? vf.current_link : 0;

			if (vf.ready_state < State.Opened)
				return (c_long)VorbisError.Inval;

			if (vf.samptrack == 0)
				return (c_long)VorbisError.False;

			c_long ret = (c_long)(vf.bittrack / vf.samptrack * vf.vi[link].rate + 0.5f);

			vf.bittrack = 0.0f;
			vf.samptrack = 0.0f;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Returns: total PCM length (samples) of content if i==-1 PCM
		/// length (samples) of that logical bitstream for i==0 to n
		/// OV_EINVAL if the stream is not seekable (we can't know the
		/// length) or only partially open
		/// </summary>
		/********************************************************************/
		public ogg_int64_t Ov_Pcm_Total(c_int i)
		{
			if (vf.ready_state < State.Opened)
				return (ogg_int64_t)VorbisError.Inval;

			if (!vf.seekable || (i >= vf.links))
				return (ogg_int64_t)VorbisError.Inval;

			if (i < 0)
			{
				ogg_int64_t acc = 0;

				for (c_int j = 0; j < vf.links; j++)
					acc += Ov_Pcm_Total(j);

				return acc;
			}
			else
				return vf.pcmlengths[i * 2 + 1];
		}



		/********************************************************************/
		/// <summary>
		/// Returns: total seconds of content if i==-1
		///          seconds in that logical bitstream for i==0 to n
		///          OV_EINVAL if the stream is not seekable (we can't know
		///          the length) or only partially open
		/// </summary>
		/********************************************************************/
		public c_double Ov_Time_Total(c_int i)
		{
			if (vf.ready_state < State.Opened)
				return (c_double)VorbisError.Inval;

			if (!vf.seekable || (i >= vf.links))
				return (c_double)VorbisError.Inval;

			if (i < 0)
			{
				c_double acc = 0;

				for (c_int j = 0; j < vf.links; j++)
					acc += Ov_Time_Total(j);

				return acc;
			}
			else
				return (c_double)(vf.pcmlengths[i * 2 + 1]) / vf.vi[i].rate;
		}



		/********************************************************************/
		/// <summary>
		/// Seek to an offset relative to the *compressed* data. This also
		/// scans packets to update the PCM cursor. It will cross a logical
		/// bitstream boundary, but only if it can't get any packets out of
		/// the tail of the bitstream we seek to (so no surprises).
		/// </summary>
		/********************************************************************/
		public VorbisError Ov_Raw_Seek(ogg_int64_t pos)
		{
			OggStream work_os = null;

			if (vf.ready_state < State.Opened)
				return VorbisError.Inval;

			if (!vf.seekable)
				return VorbisError.NoSeek;	// Don't dump machine if we can't seek

			if ((pos < 0) || (pos > vf.end))
				return VorbisError.Inval;

			// Is the seek position outside our current link [if any]?
			if (vf.ready_state >= State.StreamSet)
			{
				if ((pos < vf.offsets[vf.current_link]) || (pos >= vf.offsets[vf.current_link + 1]))
					Decode_Clear();		// Clear out stream state
			}

			// Don't yet clear out decoding machine (if it's initialized), in
			// the case we're in the same link. Restart the decode lapping, and
			// let _fetch_and_process_packet deal with a potential bitstream
			// boundary
			vf.pcm_offset = -1;
			vf.os.Reset_SerialNo((c_int)vf.current_serialno);
			Block.Vorbis_Synthesis_Restart(vf.vd);

			if (Seek_Helper(pos) != 0)
			{
				// Dump the machine so we're in a known state
				vf.pcm_offset = -1;
				Decode_Clear();

				return VorbisError.BadLink;
			}

			// We need to make sure the pcm_offset is set, but we don't want to
			// advance the raw cursor past good packets just to get to the first
			// with a granulepos. That's not equivalent behavior to beginning
			// decoding as immediately after the seek position as possible.
			//
			// So, a hack. We use two stream states; a local scratch state and
			// the shared vf->os stream state. We use the local state to
			// scan, and the shared state as a buffer for later decode.
			//
			// Unfortuantely, on the last page we still advance to last packet
			// because the granulepos on the last page is not necessarily on a
			// packet boundary, and we need to make sure the granpos is
			// correct
			{
				OggPage og = null;
				c_int lastblock = 0;
				c_int accblock = 0;
				c_int thisblock = 0;
				bool lastflag = false;
				bool firstflag = false;
				ogg_int64_t pagepos = -1;

				OggStream.Init(out work_os, (c_int)vf.current_serialno);	// Get the memory ready
				work_os.Reset();	// Eliminate the spurious OV_HOLE
									// return from not necessarily
									// starting from the beginning

				while (true)
				{
					if (vf.ready_state >= State.StreamSet)
					{
						// Snarf/scan a packet if we can
						c_int result = work_os.PacketOut(out Ogg_Packet op);

						if (result > 0)
						{
							if (vf.vi[vf.current_link].codec_setup != null)
							{
								thisblock = (c_int)Synthesis.Vorbis_Packet_Blocksize(vf.vi[vf.current_link], op);

								if (thisblock < 0)
								{
									vf.os.PacketOut(out _);
									thisblock = 0;
								}
								else
								{
									// We can't get a guaranteed correct pcm position out of the
									// last page in a stream because it might have a 'short'
									// granpos, which can only be detected in the presence of a
									// preceding page. However, if the last page is also the first
									// page, the granpos rules of a first page take precedence. Not
									// only that, but for first==last, the EOS page must be treated
									// as if its a normal first page for the stream to open/play
									if (lastflag && !firstflag)
										vf.os.PacketOut(out _);
									else
									{
										if (lastblock != 0)
											accblock += (lastblock + thisblock) >> 2;
									}
								}

								if (op.GranulePos != -1)
								{
									c_int link = vf.current_link;
									ogg_int64_t granulepos = op.GranulePos - vf.pcmlengths[link * 2];

									if (granulepos < 0)
										granulepos = 0;

									for (c_int i = 0; i < link; i++)
										granulepos += vf.pcmlengths[i * 2 + 1];

									vf.pcm_offset = granulepos - accblock;

									if (vf.pcm_offset < 0)
										vf.pcm_offset = 0;

									break;
								}

								lastblock = thisblock;
								continue;
							}
							else
								vf.os.PacketOut(out _);
						}
					}

					if (lastblock == 0)
					{
						pagepos = Get_Next_Page(ref og, -1);
						if (pagepos < 0)
						{
							vf.pcm_offset = Ov_Pcm_Total(-1);
							break;
						}
					}
					else
					{
						// Huh? Bogus stream with packets but no granulepos
						vf.pcm_offset = -1;
						break;
					}

					// Has our decoding just traversed a bitstream boundary?
					if (vf.ready_state >= State.StreamSet)
					{
						if (vf.current_serialno != og.SerialNo())
						{
							// Two possibilities:
							// 1) our decoding just traversed a bitstream boundary
							// 2) another stream is multiplexed into this logical section?
							if (og.Bos())
							{
								// We traversed
								Decode_Clear();		// Clear out stream state
								work_os.Clear();
							}	// Else, do nothing; next loop will scoop another page
						}
					}

					if (vf.ready_state < State.StreamSet)
					{
						c_int link;
						c_long serialno = og.SerialNo();

						for (link = 0; link < vf.links; link++)
						{
							if (vf.serialnos[link] == serialno)
								break;
						}

						if (link == vf.links)
							continue;	// Not the desired Vorbis bitstream section; keep trying

						vf.current_link = link;
						vf.current_serialno = serialno;

						vf.os.Reset_SerialNo((c_int)serialno);
						work_os.Reset_SerialNo((c_int)serialno);

						vf.ready_state = State.StreamSet;
						firstflag = pagepos <= vf.dataoffsets[link];
					}

					vf.os.PageIn(og);
					work_os.PageIn(og);
					lastflag = og.Eos();
				}
			}

			work_os.Clear();

			vf.bittrack = 0.0f;
			vf.samptrack = 0.0f;

			return VorbisError.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Page granularity seek (faster than sample granularity because we
		/// don't do the last bit of decode to find a specific sample).
		///
		/// Seek to the last [granule marked] page preceding the specified
		/// pos location, such that decoding past the returned point will
		/// quickly arrive at the requested position
		/// </summary>
		/********************************************************************/
		public VorbisError Ov_Pcm_Seek_Page(ogg_int64_t pos)
		{
			c_int link = -1;
			ogg_int64_t result = 0;
			ogg_int64_t total = Ov_Pcm_Total(-1);

			if (vf.ready_state < State.Opened)
				return VorbisError.Inval;

			if (!vf.seekable)
				return VorbisError.NoSeek;

			if ((pos < 0) || (pos > total))
				return VorbisError.Inval;

			// Which bitstream section does this pcm offset occur in?
			for (link = vf.links - 1; link >= 0; link--)
			{
				total -= vf.pcmlengths[link * 2 + 1];

				if (pos >= total)
					break;
			}

			// Search within the logical bitstream for the page with the highest
			// pcm_pos preceding pos. If we're looking for a position on the
			// first page, bisection will halt without finding our position as
			// it's before the first explicit granulepos fencepost. That case is
			// handled separately below.
			//
			// There is a danger here; missing pages or incorrect frame number
			// information in the bitstream could make our task impossible.
			// Account for that (it would be an error condition)
			//
			// New search algorithm originally by HB (Nicholas Vinen)
			{
				ogg_int64_t end = vf.offsets[link + 1];
				ogg_int64_t begin = vf.dataoffsets[link];
				ogg_int64_t begintime = vf.pcmlengths[link * 2];
				ogg_int64_t endtime = vf.pcmlengths[link * 2 + 1] + begintime;
				ogg_int64_t target = pos - total + begintime;
				ogg_int64_t best = -1;
				bool got_page = false;

				OggPage og = null;

				// If we have only one page, there will be no bisection. Grab the page here
				if (begin == end)
				{
					result = Seek_Helper(begin);
					if (result != 0)
						goto SeekError;

					result = Get_Next_Page(ref og, 1);
					if (result < 0)
						goto SeekError;

					got_page = true;
				}

				// Bisection loop
				while (begin < end)
				{
					ogg_int64_t bisect;

					if ((end - begin) < ChunkSize)
						bisect = begin;
					else
					{
						// Take a (pretty decent) guess
						bisect = begin + (ogg_int64_t)((c_double)(target - begintime) * (end - begin) / (endtime - begintime)) - ChunkSize;

						if (bisect < (begin + ChunkSize))
							bisect = begin;
					}

					result = Seek_Helper(bisect);
					if (result != 0)
						goto SeekError;

					// Read loop within the bisection loop
					while (begin < end)
					{
						result = Get_Next_Page(ref og, end - vf.offset);

						if (result == (ogg_int64_t)VorbisError.Read)
							goto SeekError;

						if (result < 0)
						{
							// There is no next page!
							if (bisect <= (begin + 1))
							{
								// No bisection left to perform. We've either found the
								// best candidate already or failed. Exit loop
								end = begin;
							}
							else
							{
								// We tried to load a fraction of the last page; back up a
								// bit and try to get the whole last page
								if (bisect == 0)
									goto SeekError;

								bisect -= ChunkSize;

								// Don't repeat/loop on a read we've already performed
								if (bisect <= begin)
									bisect = begin + 1;

								// Seek and continue bisection
								result = Seek_Helper(bisect);
								if (result != 0)
									goto SeekError;
							}
						}
						else
						{
							got_page = true;

							// Got a page. Analyze it
							// Only consider pages from primary vorbis stream
							if (og.SerialNo() != vf.serialnos[link])
								continue;

							// Only consider pages with the granulepos set
							ogg_int64_t granulepos = og.GranulePos();
							if (granulepos == -1)
								continue;

							if (granulepos < target)
							{
								// This page is a successful candicate! Set state
								best = result;	// Raw offset of packet with granulepos
								begin = vf.offset;	// Raw offset of next page
								begintime = granulepos;

								// If we're before our target but within a short distance,
								// don't bisect; read forward
								if ((target - begintime) > 44100)
									break;

								bisect = begin;	// *not* begin + 1 as above
							}
							else
							{
								// This is one of our pages, but the granpos is
								// post-target; it is not a bisection return
								// candidate. (The only way we'd use it is if it's the
								// first page in the stream; we handle that case later
								// outside the bisection)
								if (bisect <= (begin + 1))
								{
									// No bisection left to perform. We've either found the
									// best candidate already or failed. Exit loop
									end = begin;
								}
								else
								{
									if (end == vf.offset)
									{
										// bisection read to the end; use the known page
										// boundary (result) to update bisection, back up a
										// little bit, and try again
										end = result;
										bisect -= ChunkSize;

										if (bisect <= begin)
											bisect = begin + 1;

										result = Seek_Helper(bisect);
										if (result != 0)
											goto SeekError;
									}
									else
									{
										// Normal bisection
										end = bisect;
										endtime = granulepos;
										break;
									}
								}
							}
						}
					}
				}

				// Out of bisection: did it 'fail?'
				if (best == -1)
				{
					// Check the 'looking for data in first page' special case;
					// bisection would 'fail' because our search target was before the
					// first PCM granule position fencepost
					if (got_page && (begin == vf.dataoffsets[link]) && (og.SerialNo() == vf.serialnos[link]))
					{
						// Yes, this is the beginning-of-stream case. We already have
						// our page, right at the beginning of PCM data. Set state
						// and return
						vf.pcm_offset = total;

						if (link != vf.current_link)
						{
							// Different link; dump entire decode machine
							Decode_Clear();

							vf.current_link = link;
							vf.current_serialno = vf.serialnos[link];
							vf.ready_state = State.StreamSet;
						}
						else
							Block.Vorbis_Synthesis_Restart(vf.vd);

						vf.os.Reset_SerialNo((c_int)vf.current_serialno);
						vf.os.PageIn(og);
					}
					else
						goto SeekError;
				}
				else
				{
					// Bisection found our page. seek to it, update pcm offset. Easier case than
					// raw_seek, don't keep packets preceding granulepos
					OggPage og1 = null;

					// Seek
					result = Seek_Helper(best);
					vf.pcm_offset = -1;

					if (result != 0)
						goto SeekError;

					result = Get_Next_Page(ref og1, -1);
					if (result < 0)
						goto SeekError;

					if (link != vf.current_link)
					{
						// Different link; dump entire decode machine
						Decode_Clear();

						vf.current_link = link;
						vf.current_serialno = vf.serialnos[link];
						vf.ready_state = State.StreamSet;
					}
					else
						Block.Vorbis_Synthesis_Restart(vf.vd);

					vf.os.Reset_SerialNo((c_int)vf.current_serialno);
					vf.os.PageIn(og1);

					// Pull out all but last packet; the one with granulepos
					while (true)
					{
						result = vf.os.PacketPeek(out Ogg_Packet op);

						if (result == 0)
						{
							// No packet returned; we exited the bisection with 'best'
							// pointing to a page with a granule position, so the packet
							// finishing this page ('best') originated on a preceding
							// page. Keep fetching previous pages until we get one with
							// a granulepos or without the 'continued' flag set. Then
							// just use raw_seek for simplicity.
							//
							// Do not rewind past the beginning of link data; if we do,
							// it's either a bug or a broken stream
							result = best;

							while (result > vf.dataoffsets[link])
							{
								result = Get_Prev_Page(result, og1);
								if (result < 0)
									goto SeekError;

								if ((og1.SerialNo() == vf.current_serialno) && ((og1.GranulePos() > -1) || !og1.Continued()))
									return Ov_Raw_Seek(result);
							}
						}

						if (result < 0)
						{
							result = (ogg_int64_t)VorbisError.BadPacket;
							goto SeekError;
						}

						if (op.GranulePos != -1)
						{
							vf.pcm_offset = op.GranulePos - vf.pcmlengths[vf.current_link * 2];

							if (vf.pcm_offset < 0)
								vf.pcm_offset = 0;

							vf.pcm_offset += total;
							break;
						}
						else
							result = vf.os.PacketOut(out _);
					}
				}
			}

			// Verify result
			if ((vf.pcm_offset > pos) || (pos > Ov_Pcm_Total(-1)))
			{
				result = (ogg_int64_t)VorbisError.Fault;
				goto SeekError;
			}

			vf.bittrack = 0.0f;
			vf.samptrack = 0.0f;

			return VorbisError.Ok;

			SeekError:
			// Dump machine so we're in a known state
			vf.pcm_offset = -1;
			Decode_Clear();

			return (VorbisError)result;
		}



		/********************************************************************/
		/// <summary>
		/// Seek to a sample offset relative to the decompressed pcm stream
		/// </summary>
		/********************************************************************/
		public VorbisError Ov_Pcm_Seek(ogg_int64_t pos)
		{
			c_int thisblock;
			c_int lastblock = 0;

			VorbisError ret = Ov_Pcm_Seek_Page(pos);
			if (ret != VorbisError.Ok)
				return ret;

			c_int ret1 = Make_Decode_Ready();
			if (ret1 != 0)
				return (VorbisError)ret1;

			// Discard leading packets we don't need for the lapping of the
			// position we want; don't decode them
			while (true)
			{
				OggPage og = null;

				ret1 = vf.os.PacketPeek(out Ogg_Packet op);
				if (ret1 > 0)
				{
					thisblock = (c_int)Synthesis.Vorbis_Packet_Blocksize(vf.vi[vf.current_link], op);
					if (thisblock < 0)
					{
						vf.os.PacketOut(out _);
						continue;	// Non audio packet
					}

					if (lastblock != 0)
						vf.pcm_offset += (lastblock + thisblock) >> 2;

					if ((vf.pcm_offset + ((thisblock + Info.Vorbis_Info_Blocksize(vf.vi[0], 1)) >> 2)) >= pos)
						break;

					// Remove the packet from packet queue and track its granulepos
					vf.os.PacketOut(out _);
					Synthesis.Vorbis_Synthesis_Trackonly(vf.vb, op);	// Set up a vb with only tracking, no pcm_decode
					Block.Vorbis_Synthesis_Blockin(vf.vd, vf.vb);

					// End of logical stream case is hard, especially with exact
					// length positioning
					if (op.GranulePos > -1)
					{
						// Always believe the stream markers
						vf.pcm_offset = op.GranulePos - vf.pcmlengths[vf.current_link * 2];

						if (vf.pcm_offset < 0)
							vf.pcm_offset = 0;

						for (c_int i = 0; i < vf.current_link; i++)
							vf.pcm_offset += vf.pcmlengths[i * 2 + 1];
					}

					lastblock = thisblock;
				}
				else
				{
					if ((ret1 < 0) && (ret1 != (c_int)VorbisError.Hole))
						break;

					// Suck in a new page
					if (Get_Next_Page(ref og, -1) < 0)
						break;

					if (og.Bos())
						Decode_Clear();

					if (vf.ready_state < State.StreamSet)
					{
						c_long serialno = og.SerialNo();
						c_int link;

						for (link = 0; link < vf.links; link++)
						{
							if (vf.serialnos[link] == serialno)
								break;
						}

						if (link == vf.links)
							continue;

						vf.current_link = link;

						vf.ready_state = State.StreamSet;
						vf.current_serialno = og.SerialNo();
						vf.os.Reset_SerialNo((c_int)serialno);

						ret1 = Make_Decode_Ready();
						if (ret1 != 0)
							return (VorbisError)ret1;

						lastblock = 0;
					}

					vf.os.PageIn(og);
				}
			}

			vf.bittrack = 0.0f;
			vf.samptrack = 0.0f;

			// Discard samples until we reach the desired position. Crossing a
			// logical bitstream boundary with abanon is OK
			{
				// Note that halfrate could be set differently in each link, but
				// vorbisfile encoforces all links are set or unset
				c_int hs = Synthesis.Vorbis_Synthesis_Halfrate_P(vf.vi[0]);

				while (vf.pcm_offset < ((pos >> hs) << hs))
				{
					ogg_int64_t target = (pos - vf.pcm_offset) >> hs;
					c_long samples = Block.Vorbis_Synthesis_Pcmout(vf.vd);

					if (samples > target)
						samples = (c_long)target;

					Block.Vorbis_Synthesis_Read(vf.vd, (c_int)samples);
					vf.pcm_offset += samples << hs;

					if (samples < target)
					{
						if (Fetch_And_Process_Packet(out _, true, true) <= 0)
							vf.pcm_offset = Ov_Pcm_Total(-1);		// eof
					}
				}
			}

			return VorbisError.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Seek to a playback time relative to the decompressed pcm stream
		/// </summary>
		/********************************************************************/
		public VorbisError Ov_Time_Seek(c_double seconds)
		{
			// Translate time to PCM position and call ov_pcm_seek
			c_int link = -1;
			ogg_int64_t pcm_total = 0;
			c_double time_total = 0.0;

			if (vf.ready_state < State.Opened)
				return VorbisError.Inval;

			if (!vf.seekable)
				return VorbisError.NoSeek;

			if (seconds < 0)
				return VorbisError.Inval;

			// Which bitstream section does this time offset occur in?
			for (link = 0; link < vf.links; link++)
			{
				c_double addsec = Ov_Time_Total(link);

				if (seconds < (time_total + addsec))
					break;

				time_total += addsec;
				pcm_total += vf.pcmlengths[link * 2 + 1];
			}

			if (link == vf.links)
				return VorbisError.Inval;

			// Enough information to convert time offset to pcm offset
			{
				ogg_int64_t target = (ogg_int64_t)(pcm_total + (seconds - time_total) * vf.vi[link].rate);

				return Ov_Pcm_Seek(target);
			}
		}



		/********************************************************************/
		/// <summary>
		/// link:   -1) return the vorbis_info struct for the bitstream
		///             section currently being decoded
		///        0-n) to request information for a specific bitstream
		///             section
		///
		/// In the case of a non-seekable bitstream, any call returns the
		/// current bitstream. NULL in the case that the machine is not
		/// initialized
		/// </summary>
		/********************************************************************/
		public VorbisInfo Ov_Info(c_int link)
		{
			if (vf.seekable)
			{
				if (link < 0)
				{
					if (vf.ready_state >= State.StreamSet)
						return vf.vi[vf.current_link];
					else
						return vf.vi[0];
				}
				else
				{
					if (link >= vf.links)
						return null;
					else
						return vf.vi[link];
				}
			}
			else
				return vf.vi[0];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public VorbisComment Ov_Comment(c_int link)
		{
			if (vf.seekable)
			{
				if (link < 0)
				{
					if (vf.ready_state >= State.StreamSet)
						return vf.vc[vf.current_link];
					else
						return vf.vc[0];
				}
				else
				{
					if (link >= vf.links)
						return null;
					else
						return vf.vc[link];
				}
			}
			else
				return vf.vc[0];
		}



		/********************************************************************/
		/// <summary>
		/// Input values: pcm_channels) a float vector per channel of output
		///               length) the sample length being read by the app
		///
		/// Return values: less than 0) error/hole in data (OV_HOLE), partial
		///                   open (OV_EINVAL)
		///                0) EOF
		///                n) number of samples of PCM actually returned.
		///                   The below works on a packet-by-packet basis,
		///                   so the return length is not related to the
		///                   'length' passed in, just guaranteed to fit.
		///
		/// *section) set to the logical bitstream number
		/// </summary>
		/********************************************************************/
		public c_long Ov_Read_Float(out CPointer<c_float>[] pcm_channels, c_int length, out c_int bitstream)
		{
			pcm_channels = null;
			bitstream = 0;

			if (vf.ready_state < State.Opened)
				return (c_long)VorbisError.Inval;

			while (true)
			{
				if (vf.ready_state == State.InitSet)
				{
					c_long samples = Block.Vorbis_Synthesis_Pcmout(vf.vd, out CPointer<c_float>[] pcm);

					if (samples != 0)
					{
						c_int hs = Synthesis.Vorbis_Synthesis_Halfrate_P(vf.vi[0]);

						pcm_channels = pcm;

						if (samples > length)
							samples = length;

						Block.Vorbis_Synthesis_Read(vf.vd, (c_int)samples);

						vf.pcm_offset += samples << hs;

						bitstream = vf.current_link;

						return samples;
					}
				}

				// Suck in another packet
				{
					c_int ret = Fetch_And_Process_Packet(out _, true, true);

					if (ret == (c_int)VorbisError.Eof)
						return 0;

					if (ret <= 0)
						return ret;
				}
			}
		}
	}
}
