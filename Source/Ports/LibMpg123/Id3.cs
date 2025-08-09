/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Polycode.NostalgicPlayer.Ports.LibMpg123.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123
{
	/// <summary>
	/// ID3v2.3 and ID3v2.4 parsing (a relevant subset)
	/// </summary>
	internal class Id3
	{
		// We know the usual text frames plus some specifics
		private const int Known_Frames = 5;

		private static readonly c_uchar[][] frame_Type = new c_uchar[Known_Frames][]
		{
			new c_uchar[] { 0x43, 0x4f, 0x4d, 0x4d },	// COMM
			new c_uchar[] { 0x54, 0x58, 0x58, 0x58 },	// TXXX
			new c_uchar[] { 0x52, 0x56, 0x41, 0x32 },	// RVA2
			new c_uchar[] { 0x55, 0x53, 0x4c, 0x54 },	// USLT
			new c_uchar[] { 0x41, 0x50, 0x49, 0x43 }	// APIC
		};

		private enum Frame_Types
		{
			Unknown = -2,
			Text = -1,
			Comment,
			Extra,
			Rva2,
			Uslt,
			Picture
		}

		private static readonly c_uchar[][] oldTags =
		{
			new c_uchar[] { 0x43, 0x4f, 0x4d },			// COM
			new c_uchar[] { 0x54, 0x41, 0x4c },			// TAL
			new c_uchar[] { 0x54, 0x42, 0x50 },			// TBP
			new c_uchar[] { 0x54, 0x43, 0x4d },			// TCM
			new c_uchar[] { 0x54, 0x43, 0x4f },			// TCO
			new c_uchar[] { 0x54, 0x43, 0x52 },			// TCR
			new c_uchar[] { 0x54, 0x44, 0x41 },			// TDA
			new c_uchar[] { 0x54, 0x44, 0x59 },			// TDY
			new c_uchar[] { 0x54, 0x45, 0x4e },			// TEN
			new c_uchar[] { 0x54, 0x46, 0x54 },			// TFT
			new c_uchar[] { 0x54, 0x49, 0x4d },			// TIM
			new c_uchar[] { 0x54, 0x4b, 0x45 },			// TKE
			new c_uchar[] { 0x54, 0x4c, 0x41 },			// TLA
			new c_uchar[] { 0x54, 0x4c, 0x45 },			// TLE
			new c_uchar[] { 0x54, 0x4d, 0x54 },			// TMT
			new c_uchar[] { 0x54, 0x4f, 0x41 },			// TOA
			new c_uchar[] { 0x54, 0x4f, 0x46 },			// TOF
			new c_uchar[] { 0x54, 0x4f, 0x4c },			// TOL
			new c_uchar[] { 0x54, 0x4f, 0x52 },			// TOR
			new c_uchar[] { 0x54, 0x4f, 0x54 },			// TOT
			new c_uchar[] { 0x54, 0x50, 0x31 },			// TP1
			new c_uchar[] { 0x54, 0x50, 0x32 },			// TP2
			new c_uchar[] { 0x54, 0x50, 0x33 },			// TP3
			new c_uchar[] { 0x54, 0x50, 0x34 },			// TP4
			new c_uchar[] { 0x54, 0x50, 0x41 },			// TPA
			new c_uchar[] { 0x54, 0x50, 0x42 },			// TPB
			new c_uchar[] { 0x54, 0x52, 0x43 },			// TRC
			new c_uchar[] { 0x54, 0x52, 0x44 },			// TRD
			new c_uchar[] { 0x54, 0x52, 0x4b },			// TRK
			new c_uchar[] { 0x54, 0x53, 0x49 },			// TSI
			new c_uchar[] { 0x54, 0x53, 0x53 },			// TSS
			new c_uchar[] { 0x54, 0x54, 0x31 },			// TT1
			new c_uchar[] { 0x54, 0x54, 0x32 },			// TT2
			new c_uchar[] { 0x54, 0x54, 0x33 },			// TT3
			new c_uchar[] { 0x54, 0x58, 0x54 },			// TXT
			new c_uchar[] { 0x54, 0x58, 0x58 },			// TXX
			new c_uchar[] { 0x54, 0x59, 0x45 }			// TYE
		};

		private static readonly c_uchar[][] newTags =
		{
			new c_uchar[] { 0x43, 0x4f, 0x4d, 0x4d },	// COMM
			new c_uchar[] { 0x54, 0x41, 0x4c, 0x42 },	// TALB
			new c_uchar[] { 0x54, 0x42, 0x50, 0x4d },	// TBPM
			new c_uchar[] { 0x54, 0x43, 0x4f, 0x4d },	// TCOM
			new c_uchar[] { 0x54, 0x43, 0x4f, 0x4e },	// TCON
			new c_uchar[] { 0x54, 0x43, 0x4f, 0x50 },	// TCOP
			new c_uchar[] { 0x54, 0x44, 0x41, 0x54 },	// TDAT
			new c_uchar[] { 0x54, 0x44, 0x4c, 0x59 },	// TDLY
			new c_uchar[] { 0x54, 0x45, 0x4e, 0x43 },	// TENC
			new c_uchar[] { 0x54, 0x46, 0x4c, 0x54 },	// TFLT
			new c_uchar[] { 0x54, 0x49, 0x4d, 0x45 },	// TIME
			new c_uchar[] { 0x54, 0x4b, 0x45, 0x59 },	// TKEY
			new c_uchar[] { 0x54, 0x4c, 0x41, 0x4e },	// TLAN
			new c_uchar[] { 0x54, 0x4c, 0x45, 0x4e },	// TLEN
			new c_uchar[] { 0x54, 0x4d, 0x45, 0x44 },	// TMED
			new c_uchar[] { 0x54, 0x4f, 0x50, 0x45 },	// TOPE
			new c_uchar[] { 0x54, 0x4f, 0x46, 0x4e },	// TOFN
			new c_uchar[] { 0x54, 0x4f, 0x4c, 0x59 },	// TOLY
			new c_uchar[] { 0x54, 0x4f, 0x52, 0x59 },	// TORY
			new c_uchar[] { 0x54, 0x4f, 0x41, 0x4c },	// TOAL
			new c_uchar[] { 0x54, 0x50, 0x45, 0x31 },	// TPE1
			new c_uchar[] { 0x54, 0x50, 0x45, 0x32 },	// TPE2
			new c_uchar[] { 0x54, 0x50, 0x45, 0x33 },	// TPE3
			new c_uchar[] { 0x54, 0x50, 0x45, 0x34 },	// TPE4
			new c_uchar[] { 0x54, 0x50, 0x4f, 0x53 },	// TPOS
			new c_uchar[] { 0x54, 0x50, 0x55, 0x42 },	// TPUB
			new c_uchar[] { 0x54, 0x53, 0x52, 0x43 },	// TSRC
			new c_uchar[] { 0x54, 0x53, 0x44, 0x41 },	// TRDA
			new c_uchar[] { 0x54, 0x52, 0x43, 0x4b },	// TRCK
			new c_uchar[] { 0x54, 0x53, 0x49, 0x5a },	// TSIZ
			new c_uchar[] { 0x54, 0x53, 0x53, 0x45 },	// TSSE
			new c_uchar[] { 0x54, 0x49, 0x54, 0x31 },	// TIT1
			new c_uchar[] { 0x54, 0x49, 0x54, 0x32 },	// TIT2
			new c_uchar[] { 0x54, 0x49, 0x54, 0x33 },	// TIT3
			new c_uchar[] { 0x54, 0x45, 0x58, 0x54 },	// TEXT
			new c_uchar[] { 0x54, 0x58, 0x58, 0x58 },	// TXXX
			new c_uchar[] { 0x54, 0x59, 0x45, 0x52 }	// TYER
		};

		private static readonly c_uint[] encoding_Widths =
		{
			1, 2, 2, 1
		};

		private delegate void Text_Converter(Mpg123_String sb, Span<c_uchar> source, size_t len);

		private readonly Text_Converter[] text_Converters;

		private readonly LibMpg123 lib;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Id3(LibMpg123 libMpg123)
		{
			lib = libMpg123;

			text_Converters = new Text_Converter[]
			{
				Convert_Latin1,

				// We always check for (multiple) BOM in 16bit unicode. Without BOM, UTF16 BE is the default.
				// Errors in encoding are detected anyway
				Convert_Utf16Bom,
				Convert_Utf16Bom,
				Convert_Utf8
			};
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Int123_Init_Id3(Mpg123_Handle fr)
		{
			fr.Id3V2.Version = 0;		// Nothing there

			Null_Id3_Links(fr);

			fr.Id3V2.Comments = 0;
			fr.Id3V2.Comment_List = null;
			fr.Id3V2.Texts = 0;
			fr.Id3V2.Text = null;
			fr.Id3V2.Extras = 0;
			fr.Id3V2.Extra = null;
			fr.Id3V2.Pictures = 0;
			fr.Id3V2.Picture = null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Int123_Exit_Id3(Mpg123_Handle fr)
		{
			Free_Picture(fr);
			Free_Comment(fr);
			Free_Extra(fr);
			Free_Text(fr);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Int123_Reset_Id3(Mpg123_Handle fr)
		{
			Int123_Exit_Id3(fr);
			Int123_Init_Id3(fr);
		}



		/********************************************************************/
		/// <summary>
		/// Set the id3v2.artist id3v2.title ... links to elements of the
		/// array
		/// </summary>
		/********************************************************************/
		public void Int123_Id3_Link(Mpg123_Handle fr)
		{
			Mpg123_Id3V2 v2 = fr.Id3V2;

			Null_Id3_Links(fr);

			for (size_t i = 0; i < v2.Texts; ++i)
			{
				Mpg123_Text entry = v2.Text[i];

				if ((entry.Id[0] == 0x54) && (entry.Id[1] == 0x49) && (entry.Id[2] == 0x54) && (entry.Id[3] == 0x32))		// TIT2
					v2.Title = entry.Text;
				else if ((entry.Id[0] == 0x54) && (entry.Id[1] == 0x41) && (entry.Id[2] == 0x4c) && (entry.Id[3] == 0x42))	// TALB
					v2.Album = entry.Text;
				else if ((entry.Id[0] == 0x54) && (entry.Id[1] == 0x50) && (entry.Id[2] == 0x45) && (entry.Id[3] == 0x31))	// TPE1
					v2.Artist = entry.Text;
				else if ((entry.Id[0] == 0x54) && (entry.Id[1] == 0x59) && (entry.Id[2] == 0x45) && (entry.Id[3] == 0x52))	// TYER
					v2.Year = entry.Text;
				else if ((entry.Id[0] == 0x54) && (entry.Id[1] == 0x43) && (entry.Id[2] == 0x4f) && (entry.Id[3] == 0x4e))	// TCON
					v2.Genre= entry.Text;
			}

			for (size_t i = 0; i < v2.Comments; ++i)
			{
				Mpg123_Text entry = v2.Comment_List[i];

				if ((entry.Description.Fill == 0) || (entry.Description.P[0] == 0))
					v2.Comment = entry.Text;
			}

			// When no generic comment found, use the last non-generic one
			if ((v2.Comment == null) && (v2.Comments > 0))
				v2.Comment = v2.Comment_List[v2.Comments - 1].Text;
		}



		/********************************************************************/
		/// <summary>
		/// Trying to parse ID3v2.3 and ID3v2.4 tags...
		///
		/// Returns: 0: Bad or just unparseable tag
		///          1: Good, (possibly) new tag info
		///          Negative: Reader error (may need more data feed, try
		///                    again)
		/// </summary>
		/********************************************************************/
		public c_int Int123_Parse_New_Id3(Mpg123_Handle fr, c_ulong first4Bytes)
		{
			const int Unsync_Flag = 128;
			const int ExtHead_Flag = 64;	// ID3v2.3+
			const int Compress_Flag = 64;	// ID3v2.2
//			const int Exp_Flag = 32;
			const int Footer_Flag = 16;
			const int Ext_Update_Flag = 64;	// ID3v2.4 only: extended header update flag
			const int Unknown_Flags = 15;   // 00001111

			c_uchar[] buf = new c_uchar[6];
			c_ulong length = 0;
			c_uchar flags = 0;
			c_int ret = 1;
			bool storeTag = false;
			c_uint footLen = 0;
			bool skipTag = false;

			c_uchar major = (c_uchar)(first4Bytes & 0xff);

			if (major == 0xff)
				return 0;	// Invalid...

			int64_t ret2 = fr.Rd.Read_Frame_Body(fr, buf, 6);
			if (ret2 < 0)	// Read more header information
				return (c_int)ret2;

			if (buf[0] == 0xff)
				return 0;	// Revision, will never be 0xff

			if ((fr.P.Flags & Mpg123_Param_Flags.Store_Raw_Id3) != 0)
				storeTag = true;

			// Second new byte are some nice flags, if these are invalid skip the whole thing
			flags = buf[1];

			// Use 4 bytes from buf to construct 28bit uint value and return 1; return 0 if bytes are not synchsafe
			bool SynchSafe_To_Long(Span<c_uchar> buff, out c_ulong res)
			{
				if (((buff[0] | buff[1] | buff[2] | buff[3]) & 0x80) != 0)
				{
					res = 0;
					return false;
				}

				res = (((c_ulong)buff[0]) << 21)
					| (((c_ulong)buff[1]) << 14)
					| (((c_ulong)buff[2]) << 7)
					| (buff[3]);

				return true;
			}

			// id3v2.3 does not store synchsafe frame sizes, but synchsafe tag size - doh!
			bool Bytes_To_Long(Span<c_uchar> buff, out c_ulong res)
			{
				if (major == 3)
				{
					res = (((c_ulong)buff[0]) << 24)
						| (((c_ulong)buff[1]) << 16)
						| (((c_ulong)buff[2]) << 8)
						| (buff[3]);

					return true;
				}

				return SynchSafe_To_Long(buff, out res);
			}

			// For id3v2.2 only
			void ThreeBytes_To_Long(Span<c_uchar> buff, out c_ulong res)
			{
				res = (((c_ulong)buff[0]) << 16)
					| (((c_ulong)buff[1]) << 8)
					| (buff[2]);
			}

			// Length-10 or length-20 (footer present); 4 synchsafe integers == 28 bit number
			// We have already read 10 bytes, so left are length or length+10 bytes belonging to tag.
			// Note: This is an 28 bit value in 32 bit storage, plenty of space for
			// length+x for reasonable x
			if (!SynchSafe_To_Long(buf.AsSpan(2), out length))
				return 0;

			if ((flags & Footer_Flag) != 0)
				footLen = 10;

			// Skip if unknown version/scary flags, parse otherwise
			if ((fr.P.Flags & Mpg123_Param_Flags.Skip_Id3V2) != 0)
				skipTag = true;

			if (((flags & Unknown_Flags) != 0) || (major > 4) || (major < 2))
				skipTag = true;

			// Standard says that compressed tags should be ignored as there isn't an agreed
			// compression scheme
			if ((major == 2) && ((flags & Compress_Flag) != 0))
				skipTag = true;

			if (length < 10)
				skipTag = true;

			if (!skipTag)
				storeTag = true;

			if (storeTag)
			{
				// Stores the whole tag with footer and an additional trailing zero
				ret2 = Store_Id3V2(fr, first4Bytes, buf, length + footLen);
				if (ret2 <= 0)
					return (c_int)ret2;
			}

			if (skipTag)
			{
				Int123_Reset_Id3(fr);	// Old data is invalid

				if (!storeTag && (ret2 = fr.Rd.Skip_Bytes(fr, (off_t)(length + footLen))) < 0)
					ret = (c_int)ret2;
			}
			else
			{
				Span<c_uchar> tagData = fr.Id3V2_Raw.AsSpan(10);

				// Try to interpret that beast
				if (length > 0)
				{
					c_uchar extFlags = 0;
					c_ulong tagPos = 0;

					// Bytes of frame title and of framesize value
					c_uint head_Part = major > 2 ? 4U : 3U;
					c_uint flag_Part = major > 2 ? 2U : 0U;

					// The amount of bytes that are unconditionally read for each frame:
					// ID, size, flags
					c_uint frameBegin = head_Part + head_Part + flag_Part;

					if ((flags & ExtHead_Flag) != 0)
					{
						if (!Bytes_To_Long(tagData, out tagPos) || (tagPos >= length))
							ret = 0;
						else if (tagPos < 6)
							ret = 0;

						if (major == 3)
						{
							tagPos += 4;	// The size itself is not included
							if (tagPos >= length)
								ret = 0;
						}
						else if (ret != 0)	// v2.4 and at least got my 6 bytes of ext header
						{
							// Only v4 knows update frames, check for that.
							// Need to step back. Header is 4 bytes length, one byte flag size,
							// one byte flags. Flag size has to be 1!
							if ((tagData[4] == 1) && ((tagData[5] & Ext_Update_Flag) != 0))
								extFlags |= Ext_Update_Flag;
						}
					}

					if ((extFlags & Ext_Update_Flag) == 0)
						Int123_Reset_Id3(fr);

					if (ret > 0)
					{
						c_uchar[] id = new c_uchar[5];
						c_ulong frameSize;
						c_ulong fFlags;   // Need 16 bits, actually

						id[4] = 0;
						fr.Id3V2.Version = major;

						// Pos now advanced after ext head, now a frame has to follow.
						// Note: tagpos <= length, which is 28 bit integer, so both
						// far away from overflow for adding known small values.
						// I want to read at least one full header now
						while (length >= (tagPos + frameBegin))
						{
							c_int i = 0;
							c_ulong pos = tagPos;

							// Level 1,2,3 - 0 is info from lame/info tag!
							// rva tags with ascending significance, then general frames
							Frame_Types tt = Frame_Types.Unknown;

							// We may have entered the padding zone or any other
							// strangeness: check if we have valid frame id characters
							for (i = 0; i < head_Part; ++i)
							{
								if (!(((tagData[(int)tagPos + i] > 47) && (tagData[(int)tagPos + i] < 58)) || ((tagData[(int)tagPos + i] > 64) && (tagData[(int)tagPos + i] < 91))))
								{
									// This is no hard error... let's just hope that we got
									// something meaningful already (ret==1 in that case)
									goto TagParse_Cleanup;	// Need to escape two loops here
								}
							}

							if (ret > 0)
							{
								// 4 or 3 bytes id
								tagData.Slice((int)pos, (int)head_Part).CopyTo(id);
								id[head_Part] = 0;	// Terminate for 3 or 4 bytes

								pos += head_Part;
								tagPos += head_Part;

								// Size as 32 bits or 28 bits
								if (fr.Id3V2.Version == 2)
									ThreeBytes_To_Long(tagData.Slice((int)pos), out frameSize);
								else
								{
									if (!Bytes_To_Long(tagData.Slice((int)pos), out frameSize))
									{
										// Just assume that up to now there was some good data
										break;
									}
								}

								tagPos += head_Part;
								pos += head_Part;

								if (fr.Id3V2.Version > 2)
								{
									fFlags = (((c_ulong)tagData[(int)pos]) << 8) | (tagData[(int)pos + 1]);
									pos += 2;
									tagPos += 2;
								}
								else
									fFlags = 0;

								if ((length - tagPos) < frameSize)
									break;

								tagPos += frameSize;	// The important advancement in whole tag

								// For sanity, after full parsing tagPos should be == pos
								bool v3 = major == 3;
								c_ulong bad_FFlags = v3 ? 7967U : 36784;
								c_ulong pres_Tag_FFlag = v3 ? 32768U : 16384;
								c_ulong pres_File_FFlag = v3 ? 16384U : 8192;
								c_ulong read_Only_FFlag = v3 ? 8192U : 4096;
								c_ulong group_FFlag = v3 ? 32U : 64;
								c_ulong compr_FFlag = v3 ? 128U : 8;
								c_ulong encr_FFlag = v3 ? 64U : 4;
								c_ulong unsync_FFlag = v3 ? 0U : 2;
								c_ulong datLen_FFlag = v3 ? 0U : 1;

								if ((head_Part < 4) && (Promote_FrameName(fr, id) != 0))
									continue;

								// Shall not or want not handle these
								if ((fFlags & (bad_FFlags | compr_FFlag | encr_FFlag)) != 0)
									continue;

								for (i = 0; i < Known_Frames; ++i)
								{
									if (id.AsSpan(0, 4).SequenceEqual(frame_Type[i]))
									{
										tt = (Frame_Types)i;
										break;
									}
								}

								if ((id[0] == 0x54) && (tt != Frame_Types.Extra))
									tt = Frame_Types.Text;

								if (tt != Frame_Types.Unknown)
								{
									c_int rva_Mode = 1;	// Mix / album
									c_ulong realSize = frameSize;
									Span<c_uchar> realData = tagData.Slice((int)pos);
									c_uchar[] unsyncBuffer = null;

									if ((((flags & Unsync_Flag) != 0) || ((fFlags & unsync_FFlag) != 0)) && (frameSize > 0))
									{
										c_ulong iPos = 0;
										c_ulong oPos = 0;

										// De-unsync: FF00 -> FF; real FF00 is simply represented as FF0000 ...
										// Damn, that means I have to delete bytes from within the data block... thus need temporal storage
										// standard mandates that de-unsync should always be safe if flag is set
										unsyncBuffer = new c_uchar[frameSize + 1];		// Will need <= bytes, plus a safety zero
										realData = unsyncBuffer;

										if (realData == Span<c_uchar>.Empty)
											continue;

										// Now going byte per byte through the data...
										realData[0] = tagData[(int)pos];
										oPos = 1;

										for (iPos = pos + 1; iPos < pos + frameSize; ++iPos)
										{
											if (!((tagData[(int)iPos] == 0) && (tagData[(int)iPos - 1] == 0xff)))
												realData[(int)oPos++] = tagData[(int)iPos];
										}

										realSize = oPos;

										// Append a zero to keep strlen() safe
										realData[(int)realSize] = 0;
									}

									// The spec says there is a group byte, without explicitly saying that it is
									// the first thing following the header. I just assume so, because of the
									// ordering of the flags
									if ((fFlags & group_FFlag) != 0)
									{
										// Just skip group byte
										if (realSize != 0)
										{
											--realSize;
											realData = realData.Slice(1);
										}
									}

									if ((fFlags & datLen_FFlag) != 0)
									{
										// Spec says the original (without compression or unsync) data length follows,
										// so it should match de-unsynced data now
										if (realSize >= 4)
										{
											if (Bytes_To_Long(realData, out c_ulong datLen) && (datLen == (realSize - 4)))
											{
												realSize -= 4;
												realData = realData.Slice(4);
											}
											else
												realSize = 0;
										}
										else
											realSize = 0;
									}

									pos = 0;	// Now at the beginning again...

									// Avoid reading over boundary, even if there is a
									// zero byte of padding for safety
									if (realSize != 0)
									{
										switch (tt)
										{
											case Frame_Types.Comment:
											case Frame_Types.Uslt:
											{
												Process_Comment(fr, tt, realData, realSize, (c_int)Frame_Types.Comment + 1, id);
												break;
											}

											case Frame_Types.Extra:	// Perhaps foobar2000's work
											{
												Process_Extra(fr, realData, realSize, (c_int)Frame_Types.Extra + 1, id);
												break;
											}

											case Frame_Types.Rva2:	// "The" RVA tag
											{
												// Starts with null-terminated identification
												//
												// Default: some individual value, mix mode
												rva_Mode = 0;

												if ((Encoding.Latin1.GetString(realData.Slice(0, 5)).ToLower() == "album") ||
												    (Encoding.Latin1.GetString(realData.Slice(0, 10)).ToLower() == "audiophile") ||
												    (Encoding.Latin1.GetString(realData.Slice(0, 4)).ToLower() == "user"))
												{
													rva_Mode = 0;
												}

												if (fr.Rva.Level[rva_Mode] <= ((int)Frame_Types.Rva2 + 1))
												{
													pos += (c_ulong)(realData.IndexOf((c_uchar)0) + 1);

													// Channel and two bytes for RVA value
													// pos possibly just past the safety zero, so one more than realSize
													if ((pos > realSize) || ((realSize - pos) < 3))
													{
														;
													}
													else if (realData[(int)pos] == 1)
													{
														++pos;

														// Only handle master channel
														//
														// Two bytes adjustment, one byte for bits representing peak - n bytes, eh bits, for peak
														// 16 bit signed integer = dB * 512. Do not shift signed integers! Multiply instead.
														// Also no implementation-defined casting. Reinterpret the pointer to signed char, then do
														// proper casting
														fr.Rva.Gain[rva_Mode] = (c_float)(((c_char)realData[(int)pos]) * 256 + realData[(int)pos + 1]) / 512;
														pos += 2;

														// Heh, the peak value is represented by a number of bits - but in what manner? Skipping that part
														fr.Rva.Peak[rva_Mode] = 0;
														fr.Rva.Level[rva_Mode] = (int)Frame_Types.Rva2 + 1;
													}
												}
												break;
											}

											// Non-rva metainfo, simply store...
											case Frame_Types.Text:
											{
												Process_Text(fr, realData, realSize, id);
												break;
											}

											case Frame_Types.Picture:
											{
												if ((fr.P.Flags & Mpg123_Param_Flags.Picture) != 0)
													Process_Picture(fr, realData, realSize);

												break;
											}
										}
									}
								}
							}
							else
								break;
						}
					}
					else
						Int123_Reset_Id3(fr);
				}
				else	// No new data, but still there was a tag that invalidates old data
					Int123_Reset_Id3(fr);

				TagParse_Cleanup:
				// Get rid of stored raw data that should not be kept
				if ((fr.P.Flags & Mpg123_Param_Flags.Store_Raw_Id3) == 0)
				{
					fr.Id3V2_Raw = null;
					fr.Id3V2_Size = 0;
				}
			}

			return ret;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Null_Id3_Links(Mpg123_Handle fr)
		{
			fr.Id3V2.Title = null;
			fr.Id3V2.Artist = null;
			fr.Id3V2.Album = null;
			fr.Id3V2.Year = null;
			fr.Id3V2.Genre = null;
			fr.Id3V2.Comment = null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Init_Mpg123_Text(Mpg123_Text txt)
		{
			lib.Mpg123_Init_String(txt.Text);
			lib.Mpg123_Init_String(txt.Description);

			Array.Clear(txt.Id);
			Array.Clear(txt.Lang);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Init_Mpg123_Picture(Mpg123_Picture pic)
		{
			lib.Mpg123_Init_String(pic.Mime_Type);
			lib.Mpg123_Init_String(pic.Description);

			pic.Type = 0;
			pic.Size = 0;
			pic.Data = null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Free_Mpg123_Text(Mpg123_Text txt)
		{
			lib.Mpg123_Free_String(txt.Text);
			lib.Mpg123_Free_String(txt.Description);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Free_Mpg123_Picture(Mpg123_Picture pic)
		{
			lib.Mpg123_Free_String(pic.Mime_Type);
			lib.Mpg123_Free_String(pic.Description);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Free_Comment(Mpg123_Handle mh)
		{
			Free_Id3_Text(ref mh.Id3V2.Comment_List, ref mh.Id3V2.Comments);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Free_Text(Mpg123_Handle mh)
		{
			Free_Id3_Text(ref mh.Id3V2.Text, ref mh.Id3V2.Texts);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Free_Extra(Mpg123_Handle mh)
		{
			Free_Id3_Text(ref mh.Id3V2.Extra, ref mh.Id3V2.Extras);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Free_Picture(Mpg123_Handle mh)
		{
			Free_Id3_Picture(ref mh.Id3V2.Picture, ref mh.Id3V2.Pictures);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Free_Id3_Text(ref Mpg123_Text[] list, ref size_t size)
		{
			for (size_t i = 0; i < size; ++i)
				Free_Mpg123_Text(list[i]);

			list = null;
			size = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Free_Id3_Picture(ref Mpg123_Picture[] list, ref size_t size)
		{
			for (size_t i = 0; i < size; ++i)
				Free_Mpg123_Picture(list[i]);

			list = null;
			size = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Mpg123_Text Add_Comment(Mpg123_Handle mh, c_uchar[] l, Mpg123_String d)
		{
			return Add_Id3_Text(ref mh.Id3V2.Comment_List, ref mh.Id3V2.Comments, null, l, d);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Mpg123_Text Add_Text(Mpg123_Handle mh, c_uchar[] id)
		{
			return Add_Id3_Text(ref mh.Id3V2.Text, ref mh.Id3V2.Texts, id, null, null);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Mpg123_Text Add_Uslt(Mpg123_Handle mh, c_uchar[] id, c_uchar[] l, Mpg123_String d)
		{
			return Add_Id3_Text(ref mh.Id3V2.Text, ref mh.Id3V2.Texts, id, l, d);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Mpg123_Text Add_Extra(Mpg123_Handle mh, Mpg123_String d)
		{
			return Add_Id3_Text(ref mh.Id3V2.Extra, ref mh.Id3V2.Extras, null, null, d);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Mpg123_Picture Add_Picture(Mpg123_Handle mh, Mpg123_Id3_Pic_Type t, Mpg123_String d)
		{
			return Add_Id3_Picture(ref mh.Id3V2.Picture, ref mh.Id3V2.Pictures, t, d);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Mpg123_Text Add_Id3_Text(ref Mpg123_Text[] list, ref size_t size, c_uchar[] id, c_uchar[] lang, Mpg123_String description)
		{
			if ((lang != null) && (description == null))
				return null;	// No lone language intended

			if ((id != null) || (description != null))
			{
				// Look through list of existing texts and return an existing entry
				// if it should be overwritten
				for (size_t i = 0; i < size; ++i)
				{
					Mpg123_Text entry = list[i];

					if (description != null)
					{
						// Overwrite entry with same description and same ID and language
						if (((id == null) || id.SequenceEqual(entry.Id)) && ((lang == null) || lang.SequenceEqual(entry.Lang)) && (lib.Mpg123_Same_String(entry.Description, description) != 0))
							return entry;
					}
					else if ((id != null) && id.SequenceEqual(entry.Id))
						return entry;	// Just overwrite because of same ID
				}
			}

			// Nothing found, add new one
			Mpg123_Text[] x = Memory.Int123_Safe_Realloc(list, size + 1);
			if (x == null)
				return null;	// Bad

			x[size] = new Mpg123_Text();

			list = x;
			size += 1;

			Init_Mpg123_Text(list[size - 1]);

			return list[size - 1];	// Return pointer to the added text
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Mpg123_Picture Add_Id3_Picture(ref Mpg123_Picture[] list, ref size_t size, Mpg123_Id3_Pic_Type type, Mpg123_String description)
		{
			if (description == null)
				return null;

			// Return entry to overwrite, if appropriate
			for (size_t i = 0; i < size; ++i)
			{
				Mpg123_Picture entry = list[i];

				if ((type == entry.Type) && ((type == Mpg123_Id3_Pic_Type.Icon) || (type == Mpg123_Id3_Pic_Type.Other_Icon) || (lib.Mpg123_Same_String(entry.Description, description) != 0)))
					return entry;
			}

			// Append a new one
			Mpg123_Picture[] x = Memory.Int123_Safe_Realloc(list, size + 1);
			if (x == null)
				return null;	// Bad

			list = x;
			size += 1;

			list[size - 1] = new Mpg123_Picture();
			Init_Mpg123_Picture(list[size - 1]);

			return list[size - 1];	// Return pointer to the added picture
		}



		/********************************************************************/
		/// <summary>
		/// Store ID3 text data in an mpg123_string; either verbatim copy or
		/// everything translated to UTF-8 encoding.
		/// Preserve the zero string separator (I don't need strlen for the
		/// total size).
		///
		/// Since we can overwrite strings with ID3 update frames, don't free
		/// memory, just grow strings
		/// </summary>
		/********************************************************************/
		private void Store_Id3_Text(Mpg123_String sb, Mpg123_Id3_Enc encoding, Span<c_uchar> source, size_t source_Size, bool noTranslate)
		{
			if (sb != null)	// Always overwrite, even with nothing
				sb.Fill = 0;

			if (source_Size == 0)
				return;

			// We shall just copy the data. Client wants to decode itself
			if (noTranslate)
			{
				// Future: Add a path for ID3 errors
				if (lib.Mpg123_Grow_String(sb, source_Size + 1) == 0)
					return;

				sb.P[0] = (c_uchar)encoding;
				source.Slice(0, (int)source_Size).CopyTo(sb.P.AsSpan(1));
				sb.Fill = source_Size + 1;

				return;
			}

			if (encoding > Mpg123_Id3_Enc.Enc_Max)
				return;

			Int123_Id3_To_Utf8(sb, encoding, source, source_Size);
		}



		/********************************************************************/
		/// <summary>
		/// On error, sb->size is 0.
		/// Also, encoding has been checked already
		/// </summary>
		/********************************************************************/
		private void Int123_Id3_To_Utf8(Mpg123_String sb, Mpg123_Id3_Enc encoding, Span<c_uchar> source, size_t source_Size)
		{
			if (sb != null)
				sb.Fill = 0;

			// A note: ID3v2.3 uses UCS-2 non-variable 16bit encoding, v2.4 uses UTF16.
			// UTF-16 uses a reserved/private range in UCS-2 to add the magic, so we just
			// always treat it as UTF
			c_uint bWidth = encoding_Widths[(int)encoding];

			// Hack! I've seen a stray zero byte before BOM. Is that supposed to happen?
			if (encoding != Mpg123_Id3_Enc.Utf16Be)		// UTF16be _can_ begin with a null byte!
			{
				while ((source_Size > bWidth) && (source[0] == 0))
				{
					--source_Size;
					source = source.Slice(1);
				}
			}

			if ((source_Size % bWidth) != 0)
			{
				// When we need two bytes for a character, it's strange to have an uneven bytestream length
				source_Size -= source_Size % bWidth;
			}

			text_Converters[(int)encoding](sb, source, source_Size);
		}



		/********************************************************************/
		/// <summary>
		/// You have checked encoding to be in the range already
		/// </summary>
		/********************************************************************/
		private Span<c_uchar> Next_Text(Span<c_uchar> prev, Mpg123_Id3_Enc encoding, size_t limit, out size_t textOffset)
		{
			Span<c_uchar> text = prev;
			int offset = 0;
			textOffset = 0;
			size_t width = encoding_Widths[(int)encoding];

			if (limit > Constant.PtrDiff_Max)
				return null;

			// So I go lengths to find zero or double zero...
			// Remember bug 2834636: Only check for aligned NULLs!
			while (offset < (ptrdiff_t)limit)
			{
				if (text[offset] == 0)
				{
					if (width <= (limit - (size_t)offset))
					{
						size_t i = 1;
						for (; i < width; ++i)
						{
							if (text[(int)i] != 0)
								break;
						}

						if (i == width)		// Found a null wide enough!
						{
							offset += (int)width;
							break;
						}
					}
					else
						return null;	// No full character left? This text is broken
				}

				offset += (int)width;
			}

			if (offset >= (int)limit)
				return null;

			textOffset = (size_t)offset;

			return text.Slice(offset);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Process_Text(Mpg123_Handle fr, Span<c_uchar> realData, size_t realSize, c_uchar[] id)
		{
			// Text encoding          $xx
			// The text (encoded) ...
			if (realSize < 1)
				return;

			Mpg123_Text t = Add_Text(fr, id);
			if (t == null)
				return;

			Mpg123_Id3_Enc encoding = (Mpg123_Id3_Enc)realData[0];
			realData = realData.Slice(1);
			realSize--;

			Array.Copy(id, 0, t.Id, 0, 4);
			Store_Id3_Text(t.Text, encoding, realData, realSize, (fr.P.Flags & Mpg123_Param_Flags.Plain_Id3Text) != 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Process_Picture(Mpg123_Handle fr, Span<c_uchar> realData, size_t realSize)
		{
			Mpg123_Picture i = null;
			Span<c_uchar> workpoint = null;
			Mpg123_Id3_Pic_Type image_Type = Mpg123_Id3_Pic_Type.Other;
			c_uchar[] image_Data = null;

			Mpg123_String mime = new Mpg123_String();
			lib.Mpg123_Init_String(mime);

			Mpg123_String description = new Mpg123_String();
			lib.Mpg123_Init_String(description);

			if (realSize < 1)
				return;

			Mpg123_Id3_Enc encoding = (Mpg123_Id3_Enc)realData[0];
			realData = realData.Slice(1);
			realSize--;

			if (encoding > Mpg123_Id3_Enc.Enc_Max)
				return;

			// Get mime type (encoding is always latin-1)
			workpoint = Next_Text(realData, Mpg123_Id3_Enc.Latin1, realSize, out size_t workpointOffset);
			if (workpoint == Span<c_uchar>.Empty)
				return;

			Int123_Id3_To_Utf8(mime, Mpg123_Id3_Enc.Latin1, realData, workpointOffset);
			realSize -= workpointOffset;
			realData = realData.Slice((int)workpointOffset);

			// Get picture type
			image_Type = (Mpg123_Id3_Pic_Type)realData[0];
			realData = realData.Slice(1);
			realSize--;

			// Get description (encoding is encoding)
			workpoint = Next_Text(realData, encoding, realSize, out workpointOffset);
			if (workpoint == Span<c_uchar>.Empty)
			{
				lib.Mpg123_Free_String(mime);
				return;
			}

			Int123_Id3_To_Utf8(description, encoding, realData, workpointOffset);
			realSize -= workpointOffset;

			if (realSize != 0)
				image_Data = new c_uchar[realSize];

			if ((realSize == 0) || (image_Data == null))
			{
				lib.Mpg123_Free_String(description);
				lib.Mpg123_Free_String(mime);
				return;
			}

			workpoint.Slice(0, (int)realSize).CopyTo(image_Data);

			// All data ready now, append to/replace in list
			i = Add_Picture(fr, image_Type, description);
			if (i == null)
			{
				lib.Mpg123_Free_String(description);
				lib.Mpg123_Free_String(mime);
				return;
			}

			// Either this is a fresh image, or one to be replaced.
			// We hand over memory, so old storage needs to be freed
			Free_Mpg123_Picture(i);

			i.Type = image_Type;
			i.Size = realSize;
			i.Data = image_Data;

			lib.Mpg123_Move_String(mime, ref i.Mime_Type);
			lib.Mpg123_Move_String(description, ref i.Description);
		}



		/********************************************************************/
		/// <summary>
		/// Store a new comment that perhaps is a RVA / RVA_ALBUM/AUDIOPHILE/
		/// RVA_MIX/RADIO one.
		/// Special gimmik: It also stores USLT to the texts. Structure is
		/// the same as for comments
		/// </summary>
		/********************************************************************/
		private void Process_Comment(Mpg123_Handle fr, Frame_Types tt, Span<c_uchar> realData, size_t realSize, c_int rva_Level, c_uchar[] id)
		{
			// Text encoding          $xx
			// Language               $xx xx xx
			// Short description (encoded!)      <text> $00 (00)
			// Then the comment text (encoded) ...
			Mpg123_Id3_Enc encoding = (Mpg123_Id3_Enc)realData[0];
			c_uchar[] lang = new c_uchar[3];	// realData + 1
			Span<c_uchar> descr = realData.Slice(4);
			Span<c_uchar> text = null;
			Mpg123_Text xcom = null;

			Mpg123_Text localCom = new Mpg123_Text();	// UTF-8 variant for local processing, remember to clean up!
			Init_Mpg123_Text(localCom);

			if (realSize < 4)
				return;

			if (encoding > Mpg123_Id3_Enc.Enc_Max)
				return;

			lang[0] = realData[1];
			lang[1] = realData[2];
			lang[2] = realData[3];

			// Be careful with finding the end of description, I have to honor encoding here
			text = Next_Text(descr, encoding, realSize - 4, out size_t textOffset);
			if (text == Span<c_uchar>.Empty)
				return;

			{	// Just for variable scope
				Mpg123_String description = new Mpg123_String();
				lib.Mpg123_Init_String(description);

				// Store the text, with desired encoding, but for comments always a local copy in UTF-8
				Store_Id3_Text(description, encoding, descr, textOffset, (fr.P.Flags & Mpg123_Param_Flags.Plain_Id3Text) != 0);

				if (tt == Frame_Types.Comment)
					Store_Id3_Text(localCom.Description, encoding, descr, textOffset, false);

				xcom = tt == Frame_Types.Uslt ? Add_Uslt(fr, id, lang, description) : Add_Comment(fr, lang, description);
				if (xcom == null)
				{
					lib.Mpg123_Free_String(description);
					Free_Mpg123_Text(localCom);

					return;
				}

				Array.Copy(id, 0, xcom.Id, 0, 4);
				Array.Copy(lang, 0, xcom.Lang, 0, 3);

				// That takes over the description allocation
				lib.Mpg123_Move_String(description, ref xcom.Description);
			}

			Store_Id3_Text(xcom.Text, encoding, text, realSize - (textOffset + 4), (fr.P.Flags & Mpg123_Param_Flags.Plain_Id3Text) != 0);

			// Remember: I will probably decode the above (again) for rva comment checking. So no messing around, please
			//
			// Look out for RVA info only when we really deal with a straight comment
			if ((tt == Frame_Types.Comment) && (localCom.Description.Fill > 0))
			{
				int rva_Mode = -1;	// mix / album
				string descriptionStr = Encoding.UTF8.GetString(localCom.Description.P, 0, (int)localCom.Description.Fill - 1).Replace("\0", string.Empty).ToLower();

				if ((descriptionStr == "rva") || (descriptionStr == "rva_mix") || (descriptionStr == "rva_track") || (descriptionStr == "rva_radio"))
					rva_Mode = 0;
				else if ((descriptionStr == "rva_album") || (descriptionStr == "rva_audiophile") || (descriptionStr == "rva_user"))
					rva_Mode = 1;

				if ((rva_Mode > -1) && (fr.Rva.Level[rva_Mode] <= rva_Level))
				{
					// Only translate the contents in here where we really need them
					Store_Id3_Text(localCom.Text, encoding, text, realSize - (textOffset + 4), false);

					if (localCom.Text.Fill > 0)
					{
						fr.Rva.Gain[rva_Mode] = float.Parse(Encoding.UTF8.GetString(localCom.Text.P));
						fr.Rva.Peak[rva_Mode] = 0;
						fr.Rva.Level[rva_Mode] = rva_Level;
					}
				}
			}

			// Make sure to free the local memory
			Free_Mpg123_Text(localCom);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Process_Extra(Mpg123_Handle fr, Span<c_uchar> realData, size_t realSize, c_int rva_Level, c_uchar[] id)
		{
			// Text encoding          $xx
			// Description        ... $00 (00)
			// Text ...
			Mpg123_Id3_Enc encoding = (Mpg123_Id3_Enc)realData[0];
			Span<c_uchar> descr = realData.Slice(1);
			Span<c_uchar> text = null;
			Mpg123_Text xex = null;
			Mpg123_Text localEx = new Mpg123_Text();

			if ((int)realSize < 1)
				return;

			if (encoding > Mpg123_Id3_Enc.Enc_Max)
				return;

			text = Next_Text(descr, encoding, realSize - 1, out size_t textOffset);
			if (text == Span<c_uchar>.Empty)
				return;

			{	// Just for variable scope
				Mpg123_String description = new Mpg123_String();
				lib.Mpg123_Init_String(description);

				// The outside storage gets reencoded to UTF-8 only if not requested otherwise
				Store_Id3_Text(description, encoding, descr, textOffset, (fr.P.Flags & Mpg123_Param_Flags.Plain_Id3Text) != 0);

				xex = Add_Extra(fr, description);
				if (xex != null)
					lib.Mpg123_Move_String(description, ref xex.Description);
				else
					lib.Mpg123_Free_String(description);
			}

			if (xex == null)
				return;

			Array.Copy(id, 0, xex.Id, 0, 4);
			Init_Mpg123_Text(localEx);	// For our local copy

			// Out local copy is always stored in UTF-8!
			Store_Id3_Text(localEx.Description, encoding, descr, textOffset, false);

			// At first, only store the outside copy of the payload. We may not need the local copy
			Store_Id3_Text(xex.Text, encoding, text, realSize - (textOffset + 1), (fr.P.Flags & Mpg123_Param_Flags.Plain_Id3Text) != 0);

			// Now check if we would like to interpret this extra info for RVA
			if (localEx.Description.Fill > 0)
			{
				bool is_Peak = false;
				c_int rva_Mode = -1;	// mix / album
				string descriptionStr = Encoding.UTF8.GetString(localEx.Description.P, 0, (c_int)localEx.Description.Fill - 1).Replace("\0", string.Empty).ToLower();

				if (descriptionStr.StartsWith("replaygain_track_"))
				{
					rva_Mode = 0;

					if (descriptionStr == "replaygain_track_peak")
						is_Peak = true;
					else if (descriptionStr == "replaygain_track_gain")
						rva_Mode = -1;
				}
				else
				{
					if (descriptionStr.StartsWith("replaygain_album_"))
					{
						rva_Mode = 1;

						if (descriptionStr == "replaygain_album_peak")
							is_Peak = true;
						else if (descriptionStr == "replaygain_album_gain")
							rva_Mode = -1;
					}
				}

				if ((rva_Mode > -1) && (fr.Rva.Level[rva_Mode] <= rva_Level))
				{
					// Now we need the translated copy of the data
					Store_Id3_Text(localEx.Text, encoding, text, realSize - (textOffset + 1), false);

					if (localEx.Text.Fill > 0)
					{
						if (is_Peak)
							fr.Rva.Peak[rva_Mode] = float.Parse(Encoding.UTF8.GetString(localEx.Text.P));
						else
							fr.Rva.Gain[rva_Mode] = float.Parse(Encoding.UTF8.GetString(localEx.Text.P));

						fr.Rva.Level[rva_Mode] = rva_Level;
					}
				}
			}

			Free_Mpg123_Text(localEx);
		}



		/********************************************************************/
		/// <summary>
		/// Make a ID3v2.3+ 4-byte ID from a ID3v2.2 3-byte ID.
		/// Note that not all frames survived to 2.4; the mapping goes to 2.3.
		/// A notable miss is the old RVA frame, which is very unspecific
		/// anyway. This function returns -1 when a not known 3 char ID was
		/// encountered, 0 otherwise
		/// </summary>
		/********************************************************************/
		private c_int Promote_FrameName(Mpg123_Handle fr, c_uchar[] id)
		{
			for (c_int i = 0; i < oldTags.Length; ++i)
			{
				if ((id[0] == oldTags[i][0]) && (id[1] == oldTags[i][1]) && (id[2] == oldTags[i][2]))
				{
					Array.Copy(newTags[i], id, 4);
					return 0;
				}
			}

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Store_Id3V2(Mpg123_Handle fr, c_ulong first4Bytes, c_uchar[] buf, c_ulong length)
		{
			c_int ret = 1;
			int64_t ret2;
			c_ulong fulLen = 10 + length;

			fr.Id3V2_Size = 0;

			// Allocate one byte more for a closing zero as safety catch for strlen()
			fr.Id3V2_Raw = new byte[fulLen + 1];

			if (fr.Id3V2_Raw == null)
			{
				fr.Err = Mpg123_Errors.Out_Of_Mem;

				ret2 = fr.Rd.Skip_Bytes(fr, (off_t)length);
				if (ret2 < 0)
					ret = (c_int)ret2;
				else
					ret = 0;
			}
			else
			{
				fr.Id3V2_Raw[0] = (byte)((first4Bytes >> 24) & 0xff);
				fr.Id3V2_Raw[1] = (byte)((first4Bytes >> 16) & 0xff);
				fr.Id3V2_Raw[2] = (byte)((first4Bytes >> 8) & 0xff);
				fr.Id3V2_Raw[3] = (byte)(first4Bytes & 0xff);

				Array.Copy(buf, 0, fr.Id3V2_Raw, 4, 6);

				ret2 = fr.Rd.Read_Frame_Body(fr, fr.Id3V2_Raw.AsMemory(10), (int)length);
				if (ret2 < 0)
				{
					ret = (c_int)ret2;
					fr.Id3V2_Raw = null;
				}
				else
				{
					// Closing with zero for paranoia
					fr.Id3V2_Raw[fulLen] = 0;
					fr.Id3V2_Size = fulLen;
				}
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Convert_Latin1(Mpg123_String sb, Span<c_uchar> s, size_t l)
		{
			size_t length = l;

			// Determine real length, a latin1 character can at most tak 2 in UTF8
			for (size_t i = 0; i < l; ++i)
			{
				if (s[(int)i] >= 0x80)
					++length;
			}

			// One extra zero byte for paranoia
			if (lib.Mpg123_Grow_String(sb, length + 1) == 0)
				return;

			c_uchar[] p = sb.P;
			int pOffset = 0;

			for (size_t i = 0; i < l; ++i)
			{
				if (s[(int)i] < 0x80)
				{
					p[pOffset] = s[(int)i];
					++pOffset;
				}
				else	// Two-byte encoding
				{
					p[pOffset] = (c_uchar)(0xc0 | (s[(int)i] >> 6));
					p[pOffset + 1] = (c_uchar)(0x80 | (s[(int)i] & 0x3f));
					pOffset += 2;
				}
			}

			sb.P[length] = 0;
			sb.Fill = length + 1;
		}



		/********************************************************************/
		/// <summary>
		/// Check if we have a byte order mark(s) there, return:
		/// -1: little endian
		///  0: no bom
		///  1: big endian
		///
		/// This modifies source and len to indicate the data _after_ the
		/// BOM(s). Note on nasty data: The last encountered BOM determines
		/// the endianess. I have seen data with multiply BOMs, namely from
		/// "the" id3v2 program. Not nice, but what should I do?
		/// </summary>
		/********************************************************************/
		private c_int Check_Bom(ref Span<c_uchar> source, ref size_t len)
		{
			c_int last_Bom = 0;

			while (len >= 2)
			{
				c_int this_Bom = 0;

				if ((source[0] == 0xff) && (source[1] == 0xfe))
					this_Bom = -1;

				if ((source[0] == 0xfe) && (source[1] == 0xff))
					this_Bom = 1;

				if (this_Bom == 0)
					break;

				// Skip the detected BOM
				last_Bom = this_Bom;
				source = source.Slice(2);
				len -= 2;
			}

			return last_Bom;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private c_ulong FullPoint(c_ulong f, c_ulong s)
		{
			return ((f & 0x3ff) << 10) + (s & 0x3ff) + 0x10000;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private size_t Utf8Len(c_ulong x)
		{
			return (size_t)(x < 0x80 ? 1 : (x < 0x800) ? 2 : (x < 0x10000) ? 3 : 4);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Convert_Utf16Bom(Mpg123_String sb, Span<c_uchar> s, size_t l)
		{
			size_t n;	// Number of bytes that make up full pairs
			size_t length = 0;	// The resulting UTF-8 length

			// Determine real length... extreme case can be more than utf-16 length
			size_t high = 0;
			size_t low = 1;

			c_int bom_Endian = Check_Bom(ref s, ref l);

			if (bom_Endian == -1)	// Little-endian
			{
				high = 1;	// The second byte is the high byte
				low = 0;	// The first byte is the low byte
			}

			n = (l / 2) * 2;	// Number of bytes that make up full pairs

			// First: get length, check for errors -- stop at first one
			for (size_t i = 0; i < n; i += 2)
			{
				c_ulong point = ((c_ulong)s[(int)(i + high)] << 8) + s[(int)(i + low)];

				if ((point & 0xfc00) == 0xd800)		// Lead surrogate
				{
					c_ushort second = (c_ushort)((i + 3 < l) ? (s[(int)(i + 2 + high)] << 8) + s[(int)(i + 2 + low)] : 0);

					if ((second & 0xfc00) == 0xdc00)	// Good...
					{
						point = FullPoint(point, second);
						length += Utf8Len(point);	// Possible 4 bytes
						i += 2;		// We overstepped one word
					}
					else	// If no valid pair, break here
					{
						n = i;	// Forget the half pair, END!
						break;
					}
				}
				else
					length += Utf8Len(point);	// 1, 2 or 3 bytes
			}

			if (lib.Mpg123_Grow_String(sb, length + 1) == 0)
				return;

			// Now really convert, skip checks as these have been done just before
			c_uchar[] p = sb.P;
			int pOffset = 0;

			for (size_t i = 0; i < n; i += 2)
			{
				c_ulong codePoint = ((c_ulong)s[(int)(i + high)] << 8) + s[(int)(i + low)];

				if ((codePoint & 0xfc00) == 0xd800)		// Lead surrogate
				{
					c_ushort second = (c_ushort)((s[(int)(i + 2 + high)] << 8) + s[(int)(i + 2 + low)]);
					codePoint = FullPoint(codePoint, second);
					i += 2;		// We overstepped one word
				}

				if (codePoint < 0x80)
					p[pOffset++] = (c_uchar)codePoint;
				else if (codePoint < 0x800)
				{
					p[pOffset++] = (c_uchar)(0xc0 | (codePoint >> 6));
					p[pOffset++] = (c_uchar)(0x80 | (codePoint & 0x3f));
				}
				else if (codePoint < 0x10000)
				{
					p[pOffset++] = (c_uchar)(0xe0 | (codePoint >> 12));
					p[pOffset++] = (c_uchar)(0x80 | ((codePoint >> 6) & 0x3f));
					p[pOffset++] = (c_uchar)(0x80 | (codePoint & 0x3f));
				}
				else if (codePoint < 0x200000)
				{
					p[pOffset++] = (c_uchar)(0xf0 | (codePoint >> 18));
					p[pOffset++] = (c_uchar)(0x80 | ((codePoint >> 12) & 0x3f));
					p[pOffset++] = (c_uchar)(0x80 | ((codePoint >> 6) & 0x3f));
					p[pOffset++] = (c_uchar)(0x80 | (codePoint & 0x3f));
				}	// Ignore bigger ones (that are not possible here anyway)
			}

			sb.P[sb.Size - 1] = 0;	// Paranoia...
			sb.Fill = sb.Size;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Convert_Utf8(Mpg123_String sb, Span<c_uchar> source, size_t len)
		{
			if (lib.Mpg123_Grow_String(sb, len + 1) != 0)
			{
				source.Slice(0, (int)len).CopyTo(sb.P);
				sb.P[len] = 0;
				sb.Fill = len + 1;
			}
		}
		#endregion
	}
}
