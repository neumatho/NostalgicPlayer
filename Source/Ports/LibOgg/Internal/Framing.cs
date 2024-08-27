/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Linq;
using System.Text;
using Polycode.NostalgicPlayer.Ports.LibOgg.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOgg.Internal
{
	/// <summary>
	/// Code raw packets into framed OggSquish stream and
	/// decode Ogg streams back into raw packets
	/// </summary>
	internal static class Framing
	{
		#region Ogg_Stream
		/********************************************************************/
		/// <summary>
		/// Init the encode/decode logical stream state
		/// </summary>
		/********************************************************************/
		public static c_int Ogg_Stream_Init(out Ogg_Stream_State os, c_int serialNo)
		{
			os = new Ogg_Stream_State();

			os.BodyStorage = 16 * 1024;
			os.LacingStorage = 1024;

			os.BodyData = Memory.Ogg_MAlloc<byte>((size_t)os.BodyStorage);
			os.LacingVals = Memory.Ogg_MAlloc<c_int>((size_t)os.LacingStorage);
			os.GranuleVals = Memory.Ogg_MAlloc<ogg_int64_t>((size_t)os.LacingStorage);

			if ((os.BodyData == null) || (os.LacingVals == null) || (os.GranuleVals == null))
			{
				os = null;
				return -1;
			}

			os.SerialNo = serialNo;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Async/delayed error detection for the ogg_stream_state
		/// </summary>
		/********************************************************************/
		public static c_int Ogg_Stream_Check(Ogg_Stream_State os)
		{
			if ((os == null) || (os.BodyData == null))
				return -1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Clear does not free os, only the non-flat storage within
		/// </summary>
		/********************************************************************/
		public static c_int Ogg_Stream_Clear(Ogg_Stream_State os)
		{
			if (os != null)
			{
				if (os.BodyData != null)
					Memory.Ogg_Free(os.BodyData);

				if (os.LacingVals != null)
					Memory.Ogg_Free(os.LacingVals);

				if (os.GranuleVals != null)
					Memory.Ogg_Free(os.GranuleVals);

				os.Clear();
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Submit data to the internal buffer of the framing engine
		/// </summary>
		/********************************************************************/
		public static c_int Ogg_Stream_IoVecIn(Ogg_Stream_State os, Ogg_IoVec[] iov, c_int count, bool eos, ogg_int64_t granulePos)
		{
			c_long bytes = 0;
			c_int i;

			if (Ogg_Stream_Check(os) != 0)
				return -1;

			if (iov == null)
				return 0;

			for (i = 0; i < count; ++i)
			{
				if (iov[i].Len > c_long.MaxValue)
					return -1;

				if (bytes > (c_long.MaxValue - (c_long)iov[i].Len))
					return -1;

				bytes += (c_long)iov[i].Len;
			}

			c_long lacingVals = bytes / 255 + 1;

			if (os.BodyReturned != 0)
			{
				// Advance packet data according to the body_returned pointer. We
				// had to keep it around to return a pointer into the buffer last
				// call
				os.BodyFill -= os.BodyReturned;

				if (os.BodyFill != 0)
					Array.Copy(os.BodyData, os.BodyReturned, os.BodyData, 0, os.BodyFill);

				os.BodyReturned = 0;
			}

			// Make sure we have the buffer storage
			if ((Os_Body_Expand(os, bytes) != 0) || (Os_Lacing_Expand(os, lacingVals) != 0))
				return -1;

			// Copy in the submitted packet. Yes, the copy is a waste; this is
			// the liability of overly clean abstraction for the time being. It
			// will actually be fairly easy to eliminate the extra copy in the
			// future
			for (i = 0; i < count; ++i)
			{
				Array.Copy(iov[i].Base, iov[i].Offset, os.BodyData, os.BodyFill, (long)iov[i].Len);
				os.BodyFill += (c_int)iov[i].Len;
			}

			// Store lacing vals for this packet
			for (i = 0; i < lacingVals - 1; i++)
			{
				os.LacingVals[os.LacingFill + i] = 255;
				os.GranuleVals[os.LacingFill + i] = os.GranulePos;
			}

			os.LacingVals[os.LacingFill + i] = bytes % 255;
			os.GranulePos = os.GranuleVals[os.LacingFill + i] = granulePos;

			// Flag the first segment as the beginning of the packet
			os.LacingVals[os.LacingFill] |= 0x100;

			os.LacingFill += lacingVals;

			// For the sake of completeness
			os.PacketNo++;

			if (eos)
				os.Eos = true;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Ogg_Stream_PacketIn(Ogg_Stream_State os, Ogg_Packet op)
		{
			Ogg_IoVec iov = new Ogg_IoVec();

			iov.Base = op.Packet;
			iov.Offset = op.Offset;
			iov.Len = (size_t)op.Bytes;

			return Ogg_Stream_IoVecIn(os, [ iov ], 1, op.Eos, op.GranulePos);
		}



		/********************************************************************/
		/// <summary>
		/// This constructs pages from buffered packet segments. The pointers
		/// returned are to static buffers; do not free. The returned buffers
		/// are good only until the next call (using the same
		/// ogg_stream_state)
		/// </summary>
		/********************************************************************/
		public static c_int Ogg_Stream_PageOut(Ogg_Stream_State os, out Ogg_Page og)
		{
			bool force = false;

			if (Ogg_Stream_Check(os) != 0)
			{
				og = null;
				return 0;
			}

			if ((os.Eos && (os.LacingFill != 0)) ||		// 'Were done, now flush' case
				((os.LacingFill != 0) && !os.Bos))		// 'initial header page' case
			{
				force = true;
			}

			return Ogg_Stream_Flush_I(os, out og, force, 4096);
		}



		/********************************************************************/
		/// <summary>
		/// Add the incoming page to the stream state; we decomposed the page
		/// into packet segments here as well
		/// </summary>
		/********************************************************************/
		public static c_int Ogg_Stream_PageIn(Ogg_Stream_State os, Ogg_Page og)
		{
			byte[] header = og.Header;
			int headerOffset = og.HeaderOffset;
			byte[] body = og.Body;
			int bodyOffset = og.BodyOffset;
			c_long bodySize = og.BodyLen;
			c_int segPtr = 0;

			c_int version = Ogg_Page_Version(og);
			bool continued = Ogg_Page_Continued(og);
			bool bos = Ogg_Page_Bos(og);
			bool eos = Ogg_Page_Eos(og);
			ogg_int64_t granulePos = Ogg_Page_GranulePos(og);
			c_int serialNo = Ogg_Page_SerialNo(og);
			c_long pageNo = Ogg_Page_PageNo(og);
			c_int segments = header[headerOffset + 26];

			if (Ogg_Stream_Check(os) != 0)
				return -1;

			// Clean up 'returned data'
			{
				c_long lr = os.LacingReturned;
				c_long br = os.BodyReturned;

				// Body data
				if (br != 0)
				{
					os.BodyFill -= br;

					if (os.BodyFill != 0)
						Array.Copy(os.BodyData, br, os.BodyData, 0, os.BodyFill);

					os.BodyReturned = 0;
				}

				if (lr != 0)
				{
					// Segment table
					if ((os.LacingFill - lr) != 0)
					{
						Array.Copy(os.LacingVals, lr, os.LacingVals, 0, os.LacingFill - lr);
						Array.Copy(os.GranuleVals, lr, os.GranuleVals, 0, os.LacingFill - lr);
					}

					os.LacingFill -= lr;
					os.LacingPacket -= lr;
					os.LacingReturned = 0;
				}
			}

			// Check the serial number
			if (serialNo != os.SerialNo)
				return -1;

			if (version > 0)
				return -1;

			if (Os_Lacing_Expand(os, segments + 1) != 0)
				return -1;

			// Are we in sequence?
			if (pageNo != os.PageNo)
			{
				// Unroll previous partial packet (if any)
				for (c_int i = os.LacingPacket; i < os.LacingFill; i++)
					os.BodyFill -= os.LacingVals[i] & 0xff;

				os.LacingFill = os.LacingPacket;

				// Make a note of dropped data in segment table
				if (os.PageNo != -1)
				{
					os.LacingVals[os.LacingFill++] = 0x400;
					os.LacingPacket++;
				}
			}

			// Are we a 'continued packet' page? If so, we may need to skip
			// some segments
			if (continued)
			{
				if ((os.LacingFill < 1) || ((os.LacingVals[os.LacingFill - 1] & 0xff) < 255) || (os.LacingVals[os.LacingFill - 1] == 0x400))
				{
					bos = false;

					for (; segPtr < segments; segPtr++)
					{
						c_int val = header[headerOffset + 27 + segPtr];
						bodyOffset += val;
						bodySize -= val;

						if (val < 255)
						{
							segPtr++;
							break;
						}
					}
				}
			}

			if (bodySize != 0)
			{
				if (Os_Body_Expand(os, bodySize) != 0)
					return -1;

				Array.Copy(body, bodyOffset, os.BodyData, os.BodyFill, bodySize);
				os.BodyFill += bodySize;
			}

			{
				c_int saved = -1;

				while (segPtr < segments)
				{
					c_int val = header[headerOffset + 27 + segPtr];
					os.LacingVals[os.LacingFill] = val;
					os.GranuleVals[os.LacingFill] = -1;

					if (bos)
					{
						os.LacingVals[os.LacingFill] |= 0x100;
						bos = false;
					}

					if (val < 255)
						saved = os.LacingFill;

					os.LacingFill++;
					segPtr++;

					if (val < 255)
						os.LacingPacket = os.LacingFill;
				}

				// Set the granulepos on the last granuleval of the last full packet
				if (saved != -1)
					os.GranuleVals[saved] = granulePos;
			}

			if (eos)
			{
				os.Eos = true;

				if (os.LacingFill > 0)
					os.LacingVals[os.LacingFill - 1] |= 0x200;
			}

			os.PageNo = pageNo + 1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Ogg_Stream_Reset(Ogg_Stream_State os)
		{
			if (Ogg_Stream_Check(os) != 0)
				return -1;

			os.BodyFill = 0;
			os.BodyReturned = 0;

			os.LacingFill = 0;
			os.LacingPacket = 0;
			os.LacingReturned = 0;

			os.HeaderFill = 0;

			os.Eos = false;
			os.Bos = false;
			os.PageNo = -1;
			os.PacketNo = 0;
			os.GranulePos = 0;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Ogg_Stream_PacketOut(Ogg_Stream_State os, out Ogg_Packet op)
		{
			if (Ogg_Stream_Check(os) != 0)
			{
				op = null;
				return 0;
			}

			return PacketOut(os, out op, true);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Ogg_Stream_PacketPeek(Ogg_Stream_State os, out Ogg_Packet op)
		{
			if (Ogg_Stream_Check(os) != 0)
			{
				op = null;
				return 0;
			}

			return PacketOut(os, out op, false);
		}
		#endregion

		#region Ogg_Sync
		/********************************************************************/
		/// <summary>
		/// Return a state in known state
		/// </summary>
		/********************************************************************/
		public static c_int Ogg_Sync_Init(out Ogg_Sync_State oy)
		{
			oy = new Ogg_Sync_State();

			oy.Storage = -1;	// Used as a readiness flag
			oy.Clear();

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Clear non-flat storage within
		/// </summary>
		/********************************************************************/
		public static c_int Ogg_Sync_Clear(Ogg_Sync_State oy)
		{
			if (oy != null)
			{
				if (oy.Data != null)
					Memory.Ogg_Free(oy.Data);

				oy.Clear();
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Ogg_Sync_Check(Ogg_Sync_State oy)
		{
			if (oy.Storage < 0)
				return -1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Memory<byte> Ogg_Sync_Buffer(Ogg_Sync_State oy, c_long size)
		{
			if (Ogg_Sync_Check(oy) != 0)
				return null;

			// First, clear out any space that has been previously returned
			if (oy.Returned != 0)
			{
				oy.Fill -= oy.Returned;

				if (oy.Fill > 0)
					Array.Copy(oy.Data, oy.Returned, oy.Data, 0, oy.Fill);

				oy.Returned = 0;
			}

			if (size > (oy.Storage - oy.Fill))
			{
				// We need to extend the internal buffer
				if (size > (c_int.MaxValue - 4096 - oy.Fill))
				{
					Ogg_Sync_Clear(oy);
					return null;
				}

				c_long newSize = size + oy.Fill + 4096;     // An extra page to be nice
				byte[] ret;

				if (oy.Data != null)
					ret = Memory.Ogg_Realloc(oy.Data, (size_t)newSize);
				else
					ret = Memory.Ogg_MAlloc<byte>((size_t)newSize);

				if (ret == null)
				{
					Ogg_Sync_Clear(oy);
					return null;
				}

				oy.Data = ret;
				oy.Storage = newSize;
			}

			// Expose a segment at least as large as requested at the fill mark
			return oy.Data.AsMemory(oy.Fill);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Ogg_Sync_Wrote(Ogg_Sync_State oy, c_long bytes)
		{
			if (Ogg_Sync_Check(oy) != 0)
				return -1;

			if ((oy.Fill + bytes) > oy.Storage)
				return -1;

			oy.Fill += bytes;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Sync the stream. This is meant to be useful for finding page
		/// boundaries.
		///
		/// Return values for this:
		/// -n) skipped n bytes
		///  0) page not ready; more data (no bytes skipped)
		///  n) page synced at current location; page length n bytes
		/// </summary>
		/********************************************************************/
		public static c_long Ogg_Sync_PageSeek(Ogg_Sync_State oy, out Ogg_Page og)
		{
			og = null;

			if (Ogg_Sync_Check(oy) != 0)
				return 0;

			byte[] page = oy.Data;
			int pageOffset = oy.Returned;
			c_long bytes = oy.Fill - oy.Returned;

			if (oy.HeaderBytes == 0)
			{
				if (bytes < 27)
					return 0;		// Not enough for a header

				// Verify capture pattern
				if (Encoding.ASCII.GetString(page, pageOffset, 4) != "OggS")
					goto SyncFail;

				c_int headerBytes = page[pageOffset + 26] + 27;
				if (bytes < headerBytes)
					return 0;		// Not enough for header + seg table

				// Count up body length in the segment table
				for (c_int i = 0; i < page[pageOffset + 26]; i++)
					oy.BodyBytes += page[pageOffset + 27 + i];

				oy.HeaderBytes = headerBytes;
			}

			if ((oy.BodyBytes + oy.HeaderBytes) > bytes)
				return 0;

			// The whole test page is buffered. Verify the checksum
			{
				// Grab the checksum bytes, set the header field to zero
				byte[] chkSum = new byte[4];
				Ogg_Page log = new Ogg_Page();

				Array.Copy(page, pageOffset + 22, chkSum, 0, 4);
				Array.Clear(page, pageOffset + 22, 4);

				// Set up a temp page struct and recompute the checksum
				log.Header = page;
				log.HeaderOffset = pageOffset;
				log.HeaderLen = oy.HeaderBytes;
				log.Body = page;
				log.BodyOffset = pageOffset + oy.HeaderBytes;
				log.BodyLen = oy.BodyBytes;
				Ogg_Page_Checksum_Set(log);

				// Compare
				if (!chkSum.SequenceEqual(page.AsSpan(pageOffset + 22, 4).ToArray()))
				{
					// D'oh. Mismatch! Corrupt page (or miscapture and not a page
					// at all)
					// Replace the computed checksum with the one actually read in
					Array.Copy(chkSum, 0, page, pageOffset + 22, 4);

					// Bad checksum. Lose sync
					goto SyncFail;
				}
			}

			// Yes, have a whole page all ready to go
			{
				og = new Ogg_Page();

				og.Header = page;
				og.HeaderOffset = pageOffset;
				og.HeaderLen = oy.HeaderBytes;
				og.Body = page;
				og.BodyOffset = pageOffset + oy.HeaderBytes;
				og.BodyLen = oy.BodyBytes;
			}

			oy.Unsynced = false;
			oy.Returned += (bytes = oy.HeaderBytes + oy.BodyBytes);
			oy.HeaderBytes = 0;
			oy.BodyBytes = 0;

			return bytes;

			SyncFail:
			oy.HeaderBytes = 0;
			oy.BodyBytes = 0;

			// Search for possible capture
			int nextOffset = Array.IndexOf(page, (byte)'O', pageOffset + 1, bytes - 1);
			if (nextOffset == -1)
				nextOffset = oy.Fill;

			oy.Returned = nextOffset;

			return -(nextOffset - pageOffset);
		}



		/********************************************************************/
		/// <summary>
		/// Sync the stream and get a page. Keep trying until we find a page.
		/// Suppress 'sync errors' after reporting the first.
		///
		/// Return values:
		/// -1) recapture (hole in data)
		///  0) need more data
		///  1) page returned
		///
		/// Returns pointers into buffered data; invalidated by next call to
		/// _stream, _clear, _init, or _buffer
		/// </summary>
		/********************************************************************/
		public static c_int Ogg_Sync_PageOut(Ogg_Sync_State oy, out Ogg_Page og)
		{
			og = null;

			if (Ogg_Sync_Check(oy) != 0)
				return 0;

			// All we need to do is verify a page at the head of the stream
			// buffer. If it doesn't verify, we look for the next potential
			// frame
			for (;;)
			{
				c_long ret = Ogg_Sync_PageSeek(oy, out og);
				if (ret > 0)
				{
					// Have a page
					return 1;
				}

				if (ret == 0)
				{
					// Need more data
					return 0;
				}

				// Head did not start a synched page... skipped some bytes
				if (!oy.Unsynced)
				{
					oy.Unsynced = true;
					return -1;
				}

				// Loop. Keep looking
			}
		}



		/********************************************************************/
		/// <summary>
		/// Clear things to an initial state. Good to call, eg, before
		/// seeking
		/// </summary>
		/********************************************************************/
		public static c_int Ogg_Sync_Reset(Ogg_Sync_State oy)
		{
			if (Ogg_Sync_Check(oy) != 0)
				return -1;

			oy.Fill = 0;
			oy.Returned = 0;
			oy.Unsynced = false;
			oy.HeaderBytes = 0;
			oy.BodyBytes = 0;

			return 0;
		}
		#endregion

		#region Ogg_Page
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Ogg_Page_Version(Ogg_Page og)
		{
			return og.Header[og.HeaderOffset + 4];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool Ogg_Page_Continued(Ogg_Page og)
		{
			return (og.Header[og.HeaderOffset + 5] & 0x01) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool Ogg_Page_Bos(Ogg_Page og)
		{
			return (og.Header[og.HeaderOffset + 5] & 0x02) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool Ogg_Page_Eos(Ogg_Page og)
		{
			return (og.Header[og.HeaderOffset + 5] & 0x04) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static ogg_int64_t Ogg_Page_GranulePos(Ogg_Page og)
		{
			byte[] page = og.Header;
			int pageOffset = og.HeaderOffset;

			ogg_uint64_t granulePos = (ogg_uint64_t)page[pageOffset + 13] & 0xff;
			granulePos = (granulePos << 8) | ((ogg_uint64_t)page[pageOffset + 12] & 0xff);
			granulePos = (granulePos << 8) | ((ogg_uint64_t)page[pageOffset + 11] & 0xff);
			granulePos = (granulePos << 8) | ((ogg_uint64_t)page[pageOffset + 10] & 0xff);
			granulePos = (granulePos << 8) | ((ogg_uint64_t)page[pageOffset + 9] & 0xff);
			granulePos = (granulePos << 8) | ((ogg_uint64_t)page[pageOffset + 8] & 0xff);
			granulePos = (granulePos << 8) | ((ogg_uint64_t)page[pageOffset + 7] & 0xff);
			granulePos = (granulePos << 8) | ((ogg_uint64_t)page[pageOffset + 6] & 0xff);

			return (ogg_int64_t)granulePos;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Ogg_Page_SerialNo(Ogg_Page og)
		{
			return (c_int)((og.Header[og.HeaderOffset + 14]) |
							((ogg_uint32_t)og.Header[og.HeaderOffset + 15] << 8) |
							((ogg_uint32_t)og.Header[og.HeaderOffset + 16] << 16) |
							((ogg_uint32_t)og.Header[og.HeaderOffset + 17] << 24));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_long Ogg_Page_PageNo(Ogg_Page og)
		{
			return (c_long)((og.Header[og.HeaderOffset + 18]) |
							((ogg_uint32_t)og.Header[og.HeaderOffset + 19] << 8) |
							((ogg_uint32_t)og.Header[og.HeaderOffset + 20] << 16) |
							((ogg_uint32_t)og.Header[og.HeaderOffset + 21] << 24));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Ogg_Page_Checksum_Set(Ogg_Page og)
		{
			if (og != null)
			{
				ogg_uint32_t crcReg = 0;

				// Safety; needed for API behavior, but not framing code
				og.Header[og.HeaderOffset + 22] = 0;
				og.Header[og.HeaderOffset + 23] = 0;
				og.Header[og.HeaderOffset + 24] = 0;
				og.Header[og.HeaderOffset + 25] = 0;

				crcReg = Os_Update_Crc(crcReg, og.Header, og.HeaderOffset, og.HeaderLen);
				crcReg = Os_Update_Crc(crcReg, og.Body, og.BodyOffset, og.BodyLen);

				og.Header[og.HeaderOffset + 22] = (byte)(crcReg & 0xff);
				og.Header[og.HeaderOffset + 23] = (byte)((crcReg >> 8) & 0xff);
				og.Header[og.HeaderOffset + 24] = (byte)((crcReg >> 16) & 0xff);
				og.Header[og.HeaderOffset + 25] = (byte)((crcReg >> 24) & 0xff);
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Os_Body_Expand(Ogg_Stream_State os, c_long needed)
		{
			if ((os.BodyStorage - needed) <= os.BodyFill)
			{
				if (os.BodyStorage > (c_long.MaxValue - needed))
				{
					Ogg_Stream_Clear(os);
					return -1;
				}

				c_long bodyStorage = os.BodyStorage + needed;

				if (bodyStorage < (c_long.MaxValue - 1024))
					bodyStorage += 1024;

				byte[] ret = Memory.Ogg_Realloc(os.BodyData, (size_t)bodyStorage);
				if (ret == null)
				{
					Ogg_Stream_Clear(os);
					return -1;
				}

				os.BodyStorage = bodyStorage;
				os.BodyData = ret;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Os_Lacing_Expand(Ogg_Stream_State os, c_long needed)
		{
			if ((os.LacingStorage - needed) <= os.LacingFill)
			{
				if (os.LacingStorage > (c_long.MaxValue - needed))
				{
					Ogg_Stream_Clear(os);
					return -1;
				}

				c_long lacingStorage = os.LacingStorage + needed;

				if (lacingStorage < (c_long.MaxValue - 32))
					lacingStorage += 32;

				int[] ret = Memory.Ogg_Realloc(os.LacingVals, (size_t)lacingStorage);
				if (ret == null)
				{
					Ogg_Stream_Clear(os);
					return -1;
				}

				os.LacingVals = ret;

				ogg_int64_t[] ret1 = Memory.Ogg_Realloc(os.GranuleVals, (size_t)lacingStorage);
				if (ret1 == null)
				{
					Ogg_Stream_Clear(os);
					return -1;
				}

				os.GranuleVals = ret1;
				os.LacingStorage = lacingStorage;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Checksum the page
		/// Direct table CRC; note that this will be faster in the future if
		/// we perform the checksum simultaneously with other copies
		/// </summary>
		/********************************************************************/
		private static ogg_uint32_t Os_Update_Crc(ogg_uint32_t crc, byte[] buffer, int offset, c_int size)
		{
			while (size >= 8)
			{
				crc ^= ((ogg_uint32_t)buffer[offset] << 24) | ((ogg_uint32_t)buffer[offset + 1] << 16) | ((ogg_uint32_t)buffer[offset + 2] << 8) | buffer[offset + 3];

				crc = Tables.CrcLookup[7][ crc >> 24        ] ^ Tables.CrcLookup[6][(crc >> 16) & 0xff] ^
					  Tables.CrcLookup[5][(crc >>  8) & 0xff] ^ Tables.CrcLookup[4][ crc        & 0xff] ^
					  Tables.CrcLookup[3][buffer[offset + 4]] ^ Tables.CrcLookup[2][buffer[offset + 5]] ^
					  Tables.CrcLookup[1][buffer[offset + 6]] ^ Tables.CrcLookup[0][buffer[offset + 7]];

				offset += 8;
				size -= 8;
			}

			while (size-- != 0)
				crc = (crc << 8) ^ Tables.CrcLookup[0][((crc >> 24) & 0xff) ^ buffer[offset++]];

			return crc;
		}



		/********************************************************************/
		/// <summary>
		/// Conditionally flush a page; force==false will only flush
		/// nominal-size pages, force==true forces us to flush a page
		/// regardless of page size so long as there's any data available at
		/// all
		/// </summary>
		/********************************************************************/
		private static c_int Ogg_Stream_Flush_I(Ogg_Stream_State os, out Ogg_Page og, bool force, c_int nFill)
		{
			c_int vals = 0;
			c_int bytes = 0;
			c_long acc = 0;
			ogg_int64_t granulePos = -1;

			og = null;

			if (Ogg_Stream_Check(os) != 0)
				return 0;

			c_int maxVals = os.LacingFill > 255 ? 255 : os.LacingFill;
			if (maxVals == 0)
				return 0;

			// Construct a page
			// Decide how many segments to include

			// If this is the initial header case, the first page must only include
			// the initial header packet
			if (!os.Bos)	// 'Initial header page' case
			{
				granulePos = 0;

				for (vals = 0; vals < maxVals; vals++)
				{
					if ((os.LacingVals[vals] & 0xff) < 255)
					{
						vals++;
						break;
					}
				}
			}
			else
			{
				// The extra packets_done, packet_just_done logic here attempts to do two things:
				// 1) Don't unnecessarily span pages.
				// 2) Unless necessary, don't flush pages if there are less than four packets on
				//    them; this expands page size to reduce unnecessary overhead if incoming packets
				//    are large.
				// These are not necessary behaviors, just 'always better than naive flushing'
				// without requiring an application to explicitly request a specific optimized
				// behavior. We'll want an explicit behavior setup pathway eventually as well
				c_int packetsDone = 0;
				c_int packetsJustDone = 0;

				for (vals = 0; vals < maxVals; vals++)
				{
					if ((acc > nFill) && (packetsJustDone >= 4))
					{
						force = true;
						break;
					}

					acc += os.LacingVals[vals] & 0xff;

					if ((os.LacingVals[vals] & 0xff) < 255)
					{
						granulePos = os.GranuleVals[vals];
						packetsJustDone = ++packetsDone;
					}
					else
						packetsJustDone = 0;
				}

				if (vals == 255)
					force = true;
			}

			if (!force)
				return 0;

			// Construct the header in temp storage
			Array.Copy("OggS"u8.ToArray(), os.Header, 4);

			// Stream structure version
			os.Header[4] = 0x00;

			// Continued packet flag?
			os.Header[5] = 0x00;

			if ((os.LacingVals[0] & 0x100) == 0)
				os.Header[5] |= 0x01;

			// First page flag?
			if (!os.Bos)
				os.Header[5] |= 0x02;

			// Last page flag?
			if (os.Eos && (os.LacingFill == vals))
				os.Header[5] |= 0x04;

			os.Bos = true;

			// 64 bits of PCM position
			for (c_int i = 6; i < 14; i++)
			{
				os.Header[i] = (byte)(granulePos & 0xff);
				granulePos >>= 8;
			}

			// 32 bits of stream serial number
			{
				c_long serialNo = os.SerialNo;

				for (c_int i = 14; i < 18; i++)
				{
					os.Header[i] = (byte)(serialNo & 0xff);
					serialNo >>= 8;
				}
			}

			// 32 bits of page counter (we have both counter and page header
			// because this val can roll over)
			if (os.PageNo == -1)		// Because someone called stream_reset; this would be a strange
				os.PageNo = 0;			// thing to do in an encode stream, but it has plausible uses

			{
				c_long pageNo = os.PageNo++;

				for (c_int i = 18; i < 22; i++)
				{
					os.Header[i] = (byte)(pageNo & 0xff);
					pageNo >>= 8;
				}
			}

			// Zero for computation; filled in later
			os.Header[22] = 0;
			os.Header[23] = 0;
			os.Header[24] = 0;
			os.Header[25] = 0;

			// Segment table
			os.Header[26] = (byte)(vals & 0xff);

			for (c_int i = 0; i < vals; i++)
				bytes += os.Header[i + 27] = (byte)(os.LacingVals[i] & 0xff);

			// Set pointers in the ogg_page struct
			og = new Ogg_Page();

			og.Header = os.Header;
			og.HeaderOffset = 0;
			og.HeaderLen = os.HeaderFill = vals + 27;
			og.Body = os.BodyData;
			og.BodyOffset = os.BodyReturned;
			og.BodyLen = bytes;

			// Advance the lacing data and set the body_returned pointer
			os.LacingFill -= vals;

			Array.Copy(os.LacingVals, vals, os.LacingVals, 0, os.LacingFill);
			Array.Copy(os.GranuleVals, vals, os.GranuleVals, 0, os.LacingFill);

			os.BodyReturned += bytes;

			// Calculate the checksum
			Ogg_Page_Checksum_Set(og);

			// Done
			return 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int PacketOut(Ogg_Stream_State os, out Ogg_Packet op, bool adv)
		{
			// The last part of decode. We have the stream broken into packet
			// segments. Now we need to group them together into packets (or return the
			// out of sync markers)
			op = null;

			c_int ptr = os.LacingReturned;

			if (os.LacingPacket <= ptr)
				return 0;

			if ((os.LacingVals[ptr] & 0x400) != 0)
			{
				// We need to tell the codec there's a gap; it might need to
				// handle previous packet dependencies
				os.LacingReturned++;
				os.PacketNo++;

				return -1;
			}

			// Gather the whole packet. We'll have no holes or a partial packet
			{
				c_int size = os.LacingVals[ptr] & 0xff;
				c_long bytes = size;
				bool eos = (os.LacingVals[ptr] & 0x0200) != 0;	// Last packet of the stream?
				bool bos = (os.LacingVals[ptr] & 0x0100) != 0;	// First packet of the stream?

				while (size == 255)
				{
					c_int val = os.LacingVals[++ptr];
					size = val & 0xff;

					if ((val & 0x200) != 0)
						eos = true;

					bytes += size;
				}

				op = new Ogg_Packet();

				op.Eos = eos;
				op.Bos = bos;
				op.Packet = os.BodyData;
				op.Offset = os.BodyReturned;
				op.PacketNo = os.PacketNo;
				op.GranulePos = os.GranuleVals[ptr];
				op.Bytes = bytes;

				if (adv)
				{
					os.BodyReturned += bytes;
					os.LacingReturned = ptr + 1;
					os.PacketNo++;
				}
			}

			return 1;
		}
		#endregion
	}
}
