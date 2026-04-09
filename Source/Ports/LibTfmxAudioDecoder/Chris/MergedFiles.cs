/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Text;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris
{
	/// <summary>
	/// Support for single-file TFMX files:
	///
	///  - TFMXPAK
	///  - TFHD
	///  - TFMX-MOD
	///
	/// There are multiple solutions that merge the two mdat.* and smpl.* files,
	/// prepend a header structure and sometimes append an extra structure.
	///
	/// TFHD : While it can specify the required TFMX version a player must support,
	/// that doesn't cover all variants of TFMX players.
	///
	/// TFMX-MOD : Its primary feature is to specify the (sub-)song to play by default
	/// in addition to providing Title/Author/Game for it. So, if a TFMX module contains
	/// multiple songs, TFMX-MOD usually is used to create a separate file for each song.
	/// The player could still select other songs found within the module data, but that
	/// defeats the purpose of this file format
	/// </summary>
	public partial class TfmxDecoder
	{
		private const string Tag_TfmxPak = "TFMXPAK ";
		private const string Tag_Tfhd = "TFHD";
		private const string Tag_TfmxMod = "TFMX-MOD";

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool IsMerged()
		{
			input.VersionHint = 0;
			input.StartSongHint = -1;

			// ----------------------------------------------------------------------
			// "TFMXPAK 012345 0123456 >>>" ASCII header from the early 90s
			//  01234567890123456789012345

			const c_int tfmxPakParseSize = 31;	// Large enough

			if ((input.Len > tfmxPakParseSize) && (CMemory.memcmp(input.Buf, Tag_TfmxPak, (size_t)Tag_TfmxPak.Length) == 0))
			{
				CPointer<char> probeStr = new CPointer<char>(tfmxPakParseSize + 1);
				CMemory.memcpy(probeStr, input.Buf, tfmxPakParseSize);
				probeStr[tfmxPakParseSize] = '\0';

				CSScanF scanf = new CSScanF();
				c_int r = scanf.Parse(probeStr.ToString(), "TFMXPAK %u %u >>>");
				CPointer<char> s = CString.strstr(probeStr, ">>>");

				if ((r != 2) || s.IsNull || ((c_uint)scanf.Results[0] == 0) || ((c_uint)scanf.Results[1] == 0))
					return false;

				input.MdatSize = (c_uint)scanf.Results[0];
				input.SmplSize = (c_uint)scanf.Results[1];

				offsets.Header = (c_uint)((s + 3) - probeStr);

				return true;
			}

			// ----------------------------------------------------------------------
			// "TFHD" structure. Big-endian

			const c_int tfmxTfhdParseSize = 0x12;

			if ((input.Len > tfmxTfhdParseSize) && (CMemory.memcmp(input.Buf, Tag_Tfhd, (size_t)Tag_Tfhd.Length) == 0))
			{
				// Strictly require two zero bytes here, although they belong to the
				// 32-bit header size value
				if (MyEndian.ReadBEUword(pBuf, 4) != 0)
					return false;

				offsets.Header = MyEndian.ReadBEUdword(pBuf, 4);

				// 0 = unspecified / 1 = TFMX v1.5 / 2 = TFMX Pro / 3 = TFMX 7V
				// bit 7 = forced
				// Only few TFHD files specify the version
				input.VersionHint = pBuf[8];

				input.MdatSize = MyEndian.ReadBEUdword(pBuf, 10);
				input.SmplSize = MyEndian.ReadBEUdword(pBuf, 14);

				if ((input.MdatSize == 0) || (input.SmplSize == 0))
					return false;

				return true;
			}

			// ----------------------------------------------------------------------
			// TFMX-MOD header as another single-file format modification
			// Little-endian!

			const c_int tfmxModParseSize = 16;

			if ((input.Len > tfmxModParseSize) && (CMemory.memcmp(input.Buf, Tag_TfmxMod, (size_t)Tag_TfmxMod.Length) == 0))
			{
				// Strictly require zero bytes here, although they belong to the
				// 32-bit offsets
				if ((pBuf[11] != 0) || (pBuf[15] != 0))
					return false;

				offsets.SampleData = MyEndian.MakeDword(pBuf[11], pBuf[10], pBuf[9], pBuf[8]);
				offsets.Header = 8 + 12;
				input.MdatSize = offsets.SampleData - offsets.Header;

				udword offs = MyEndian.MakeDword(pBuf[15], pBuf[14], pBuf[13], pBuf[12]);
				input.SmplSize = offs - input.MdatSize;

				c_int startSong = -1;
				Encoding encoder = EncoderCollection.Win1252;

				do
				{
					ubyte what = pBuf[offs++];
					uword len = MyEndian.MakeWord(pBuf[offs + 1], pBuf[offs]);

					if (len == 0)
						break;

					offs += 2;

					// Need this, since there is crap at the end that
					// triggers definitions accidentally
					if ((offs + len) > input.Len)
						break;

					switch (what)
					{
						case 1:
						{
							author = encoder.GetString(pBuf.AsSpan((int)offs, len));
							offs += len;
							break;
						}

						case 2:
						{
							game = encoder.GetString(pBuf.AsSpan((int)offs, len));
							offs += len;
							break;
						}

						case 6:
						{
							title = encoder.GetString(pBuf.AsSpan((int)offs, len));
							offs += len;
							break;
						}

						case 0:
						{
							startSong = MyEndian.MakeWord(pBuf[offs + 3], pBuf[offs + 2]);
							offs += len * 4U;	// One dword
							break;
						}

						case 5:
						{
							offs += len;
							break;
						}

						default:
						{
							offs += len;
							break;
						}
					}
				}
				while (offs < input.Len);

				input.StartSongHint = startSong;

				return true;
			}

			// No single-file format recognized
			return false;
		}
	}
}
