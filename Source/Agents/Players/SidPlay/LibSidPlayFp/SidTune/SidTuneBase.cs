/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.Exceptions;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.SidPlayFp;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.SidTune
{
	/// <summary>
	/// Base class to all sid tune loaders
	/// </summary>
	internal abstract class SidTuneBase
	{
		/// <summary>
		/// Also PSID file format limit
		/// </summary>
		private const uint MAX_SONGS = 256;

		/// <summary>
		/// The Commodore 64 memory size
		/// </summary>
		private const uint_least32_t MAX_MEMORY = 65536;

		/// <summary>
		/// Minimum load address for real C64 only tunes
		/// </summary>
		private const uint_least16_t SIDTUNE_R64_MIN_LOAD_ADDR = 0x07e8;

		#region Petscii to unicode lookup table
		/// <summary>
		/// Upper case
		/// </summary>
		private static readonly ushort[] chrTabUpper =
		{
			// 0x00 - 0x1f
			0x0000, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,
			0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,
			0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,
			0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,

			// 0x20 - 0x3f
			0xee20, 0xee21, 0xee22, 0xee23, 0xee24, 0xee25, 0xee26, 0xee27,
			0xee28, 0xee29, 0xee2a, 0xee2b, 0xee2c, 0xee2d, 0xee2e, 0xee2f,
			0xee30, 0xee31, 0xee32, 0xee33, 0xee34, 0xee35, 0xee36, 0xee37,
			0xee38, 0xee39, 0xee3a, 0xee3b, 0xee3c, 0xee3d, 0xee3e, 0xee3f,

			// 0x40 - 0x5f
			0xee00, 0xee01, 0xee02, 0xee03, 0xee04, 0xee05, 0xee06, 0xee07,
			0xee08, 0xee09, 0xee0a, 0xee0b, 0xee0c, 0xee0d, 0xee0e, 0xee0f,
			0xee10, 0xee11, 0xee12, 0xee13, 0xee14, 0xee15, 0xee16, 0xee17,
			0xee18, 0xee19, 0xee1a, 0xee1b, 0xee1c, 0xee1d, 0xee1e, 0xee1f,

			// 0x60 - 0x7f
			0xee40, 0xee41, 0xee42, 0xee43, 0xee44, 0xee45, 0xee46, 0xee47,
			0xee48, 0xee49, 0xee4a, 0xee4b, 0xee4c, 0xee4d, 0xee4e, 0xee4f,
			0xee50, 0xee51, 0xee52, 0xee53, 0xee54, 0xee55, 0xee56, 0xee57,
			0xee58, 0xee59, 0xee5a, 0xee5b, 0xee5c, 0xee5d, 0xee5e, 0xee5f,

			// 0x80 - 0x9f
			0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,
			0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,
			0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,
			0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,

			// 0xa0 - 0xbf
			0xee60, 0xee61, 0xee62, 0xee63, 0xee64, 0xee65, 0xee66, 0xee67,
			0xee68, 0xee69, 0xee6a, 0xee6b, 0xee6c, 0xee6d, 0xee6e, 0xee6f,
			0xee70, 0xee71, 0xee72, 0xee73, 0xee74, 0xee75, 0xee76, 0xee77,
			0xee78, 0xee79, 0xee7a, 0xee7b, 0xee7c, 0xee7d, 0xee7e, 0xee7f,

			// 0xc0 - 0xdf
			0xee40, 0xee41, 0xee42, 0xee43, 0xee44, 0xee45, 0xee46, 0xee47,
			0xee48, 0xee49, 0xee4a, 0xee4b, 0xee4c, 0xee4d, 0xee4e, 0xee4f,
			0xee50, 0xee51, 0xee52, 0xee53, 0xee54, 0xee55, 0xee56, 0xee57,
			0xee58, 0xee59, 0xee5a, 0xee5b, 0xee5c, 0xee5d, 0xee5e, 0xee5f,

			// 0xe0 - 0xff
			0xee60, 0xee61, 0xee62, 0xee63, 0xee64, 0xee65, 0xee66, 0xee67,
			0xee68, 0xee69, 0xee6a, 0xee6b, 0xee6c, 0xee6d, 0xee6e, 0xee6f,
			0xee70, 0xee71, 0xee72, 0xee73, 0xee74, 0xee75, 0xee76, 0xee77,
			0xee78, 0xee79, 0xee7a, 0xee7b, 0xee7c, 0xee7d, 0xee7e, 0xee5e
		};

		/// <summary>
		/// Lower case
		/// </summary>
		private static readonly ushort[] chrTabLower =
		{
			// 0x00 - 0x1f
			0x0000, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,
			0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,
			0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,
			0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,

			// 0x20 - 0x3f
			0xef20, 0xef21, 0xef22, 0xef23, 0xef24, 0xef25, 0xef26, 0xef27,
			0xef28, 0xef29, 0xef2a, 0xef2b, 0xef2c, 0xef2d, 0xef2e, 0xef2f,
			0xef30, 0xef31, 0xef32, 0xef33, 0xef34, 0xef35, 0xef36, 0xef37,
			0xef38, 0xef39, 0xef3a, 0xef3b, 0xef3c, 0xef3d, 0xef3e, 0xef3f,

			// 0x40 - 0x5f
			0xef00, 0xef01, 0xef02, 0xef03, 0xef04, 0xef05, 0xef06, 0xef07,
			0xef08, 0xef09, 0xef0a, 0xef0b, 0xef0c, 0xef0d, 0xef0e, 0xef0f,
			0xef10, 0xef11, 0xef12, 0xef13, 0xef14, 0xef15, 0xef16, 0xef17,
			0xef18, 0xef19, 0xef1a, 0xef1b, 0xef1c, 0xef1d, 0xef1e, 0xef1f,

			// 0x60 - 0x7f
			0xef40, 0xef41, 0xef42, 0xef43, 0xef44, 0xef45, 0xef46, 0xef47,
			0xef48, 0xef49, 0xef4a, 0xef4b, 0xef4c, 0xef4d, 0xef4e, 0xef4f,
			0xef50, 0xef51, 0xef52, 0xef53, 0xef54, 0xef55, 0xef56, 0xef57,
			0xef58, 0xef59, 0xef5a, 0xef5b, 0xef5c, 0xef5d, 0xef5e, 0xef5f,

			// 0x80 - 0x9f
			0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,
			0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,
			0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,
			0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,

			// 0xa0 - 0xbf
			0xef60, 0xef61, 0xef62, 0xef63, 0xef64, 0xef65, 0xef66, 0xef67,
			0xef68, 0xef69, 0xef6a, 0xef6b, 0xef6c, 0xef6d, 0xef6e, 0xef6f,
			0xef70, 0xef71, 0xef72, 0xef73, 0xef74, 0xef75, 0xef76, 0xef77,
			0xef78, 0xef79, 0xef7a, 0xef7b, 0xef7c, 0xef7d, 0xef7e, 0xef7f,

			// 0xc0 - 0xdf
			0xef40, 0xef41, 0xef42, 0xef43, 0xef44, 0xef45, 0xef46, 0xef47,
			0xef48, 0xef49, 0xef4a, 0xef4b, 0xef4c, 0xef4d, 0xef4e, 0xef4f,
			0xef50, 0xef51, 0xef52, 0xef53, 0xef54, 0xef55, 0xef56, 0xef57,
			0xef58, 0xef59, 0xef5a, 0xef5b, 0xef5c, 0xef5d, 0xef5e, 0xef5f,

			// 0xe0 - 0xff
			0xef60, 0xef61, 0xef62, 0xef63, 0xef64, 0xef65, 0xef66, 0xef67,
			0xef68, 0xef69, 0xef6a, 0xef6b, 0xef6c, 0xef6d, 0xef6e, 0xef6f,
			0xef70, 0xef71, 0xef72, 0xef73, 0xef74, 0xef75, 0xef76, 0xef77,
			0xef78, 0xef79, 0xef7a, 0xef7b, 0xef7c, 0xef7d, 0xef7e, 0xef5e
		};

		/// <summary>
		/// Lower case (lyrics)
		/// </summary>
		private static readonly ushort[] chrTabLowerLyrics =
		{
			// 0x00 - 0x1f
			0x0000, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,
			0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,
			0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,
			0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,

			// 0x20 - 0x3f
			0xef20, 0xef21, 0xef22, 0xef23, 0xef24, 0xef25, 0xef26, 0xef27,
			0xef28, 0xef29, 0xef2a, 0xef2b, 0xef2c, 0xef2d, 0xef2e, 0xef2f,
			0xef30, 0xef31, 0xef32, 0xef33, 0xef34, 0xef35, 0xef36, 0xef37,
			0xef38, 0xef39, 0xef3a, 0xef3b, 0xef3c, 0xef3d, 0xef3e, 0xef3f,

			// 0x40 - 0x5f
			0xef40, 0xef41, 0xef42, 0xef43, 0xef44, 0xef45, 0xef46, 0xef47,
			0xef48, 0xef49, 0xef4a, 0xef4b, 0xef4c, 0xef4d, 0xef4e, 0xef4f,
			0xef50, 0xef51, 0xef52, 0xef53, 0xef54, 0xef55, 0xef56, 0xef57,
			0xef58, 0xef59, 0xef5a, 0xef5b, 0xef5c, 0xef5d, 0xef5e, 0xef5f,

			// 0x60 - 0x7f
			0xef00, 0xef01, 0xef02, 0xef03, 0xef04, 0xef05, 0xef06, 0xef07,
			0xef08, 0xef09, 0xef0a, 0xef0b, 0xef0c, 0xef0d, 0xef0e, 0xef0f,
			0xef10, 0xef11, 0xef12, 0xef13, 0xef14, 0xef15, 0xef16, 0xef17,
			0xef18, 0xef19, 0xef1a, 0xef1b, 0xef1c, 0xef1d, 0xef1e, 0xef1f,

			// 0x80 - 0x9f
			0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,
			0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,
			0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,
			0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,

			// 0xa0 - 0xbf
			0xef60, 0xef61, 0xef62, 0xef63, 0xef64, 0xef65, 0xef66, 0xef67,
			0xef68, 0xef69, 0xef6a, 0xef6b, 0xef6c, 0xef6d, 0xef6e, 0xef6f,
			0xef70, 0xef71, 0xef72, 0xef73, 0xef74, 0xef75, 0xef76, 0xef77,
			0xef78, 0xef79, 0xef7a, 0xef7b, 0xef7c, 0xef7d, 0xef7e, 0xef7f,

			// 0xc0 - 0xdf
			0xef00, 0xef01, 0xef02, 0xef03, 0xef04, 0xef05, 0xef06, 0xef07,
			0xef08, 0xef09, 0xef0a, 0xef0b, 0xef0c, 0xef0d, 0xef0e, 0xef0f,
			0xef10, 0xef11, 0xef12, 0xef13, 0xef14, 0xef15, 0xef16, 0xef17,
			0xef18, 0xef19, 0xef1a, 0xef1b, 0xef1c, 0xef1d, 0xef1e, 0xef1f,

			// 0xe0 - 0xff
			0xef60, 0xef61, 0xef62, 0xef63, 0xef64, 0xef65, 0xef66, 0xef67,
			0xef68, 0xef69, 0xef6a, 0xef6b, 0xef6c, 0xef6d, 0xef6e, 0xef6f,
			0xef70, 0xef71, 0xef72, 0xef73, 0xef74, 0xef75, 0xef76, 0xef77,
			0xef78, 0xef79, 0xef7a, 0xef7b, 0xef7c, 0xef7d, 0xef7e, 0xef5e
		};
		#endregion

		/// <summary></summary>
		protected readonly SidTuneInfoImpl info;

		protected readonly uint_least8_t[] songSpeed = new uint_least8_t[MAX_SONGS];
		protected readonly SidTuneInfo.clock_t[] clockSpeed = new SidTuneInfo.clock_t[MAX_SONGS];

		/// <summary>
		/// For files with header: offset to real data
		/// </summary>
		protected uint_least32_t fileOffset;

		protected byte[] cache;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected SidTuneBase()
		{
			info = new SidTuneInfoImpl();
			fileOffset = 0;

			// Initialize the object with some safe defaults
			for (int si = 0; si < MAX_SONGS; si++)
			{
				songSpeed[si] = (uint_least8_t)info.songSpeed;
				clockSpeed[si] = info.clockSpeed;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Load a sid tune from a file
		/// </summary>
		/********************************************************************/
		public static SidTuneBase Load(PlayerFileInfo fileInfo, string[] fileNameExtensions)
		{
			if (fileInfo == null)
				return null;

			return GetFromFiles(fileInfo, fileNameExtensions);
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve sub-song specific information
		/// </summary>
		/********************************************************************/
		public SidTuneInfo GetInfo()
		{
			return info;
		}



		/********************************************************************/
		/// <summary>
		/// Select sub-song (0 = default starting song) and return active
		/// song number out of [1,2,..,SIDTUNE_MAX_SONGS]
		/// </summary>
		/********************************************************************/
		public uint SelectSong(uint selectedSong)
		{
			// Check whether selected song is valid, use start song if not
			uint song = (selectedSong == 0) || (selectedSong > info.songs) ? info.startSong : selectedSong;

			// Copy any song-specific variable information
			// such a speed/clock setting to the info structure
			info.currentSong = song;

			// Retrieve song speed definition
			switch (info.compatibility)
			{
				case SidTuneInfo.compatibility_t.COMPATIBILITY_R64:
				{
					info.songSpeed = SidTuneInfo.SPEED_CIA_1A;
					break;
				}

				case SidTuneInfo.compatibility_t.COMPATIBILITY_PSID:
				{
					// This does not take into account the PlaySID bug upon evalutating the
					// SPEED field. It would most likely break compatibility to lots of
					// sidtunes, which have been converted from .SID format and vice versa.
					// The .SID format does the bit-wise/song-wise evaluation of the SPEED
					// value correctly, like it is described in the PlaySID documentation
					info.songSpeed = songSpeed[(song - 1) & 31];
					break;
				}

				default:
				{
					info.songSpeed = songSpeed[song - 1];
					break;
				}
			}

			info.clockSpeed = clockSpeed[song - 1];

			return info.currentSong;
		}



		/********************************************************************/
		/// <summary>
		/// Copy sid tune into C64 memory (64 KB)
		/// </summary>
		/********************************************************************/
		public virtual void PlaceSidTuneInC64Mem(ISidMemory mem)
		{
			// The Basic ROM sets these values on loading a file
			// Program end address
			uint_least16_t start = info.loadAddr;
			uint_least16_t end = (uint_least16_t)(start + info.c64DataLen);

			mem.WriteMemWord(0x2d, end);		// Variables start
			mem.WriteMemWord(0x2f, end);		// Arrays start
			mem.WriteMemWord(0x31, end);		// Strings start
			mem.WriteMemWord(0xac, start);
			mem.WriteMemWord(0xae, end);

			// Copy data from cache to the correct destination
			mem.FillRam(info.loadAddr, cache, fileOffset, info.c64DataLen);
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the MD5 hash of the tune
		/// </summary>
		/********************************************************************/
		public virtual byte[] CreateMD5New()
		{
			return null;
		}

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Cache the data of a single-file or two-file sid tune
		/// </summary>
		/********************************************************************/
		protected virtual void AcceptSidTune(byte[] buf)
		{
			// Fix bad sid tune setup
			if (info.songs > MAX_SONGS)
				info.songs = MAX_SONGS;
			else if (info.songs == 0)
				info.songs = 1;

			if ((info.startSong == 0) || (info.startSong > info.songs))
				info.startSong = 1;

			info.dataFileLen = (uint_least32_t)buf.Length;
			info.c64DataLen = (uint_least32_t)(buf.Length - fileOffset);

			// Calculate any remaining addresses and then
			// confirm all the file details are correct
			ResolveAddrs(buf, fileOffset);

			if (!CheckRelocInfo())
				throw new LoadErrorException(Resources.IDS_SID_ERR_BAD_RELOC);

			if (!CheckCompatibility())
				throw new LoadErrorException(Resources.IDS_SID_ERR_BAD_ADDR);

			if (info.dataFileLen >= 2)
			{
				// We only detect an offset of two. Some positions independent
				// sid tunes contain a load address of 0xE000, but are loaded
				// to 0x0FFE and call player at 0x1000
				info.fixLoad = SidEndian.Endian_Little16(buf, fileOffset) == (info.loadAddr + 2);
			}

			// Check the size of the data
			if (info.c64DataLen > MAX_MEMORY)
				throw new LoadErrorException(Resources.IDS_SID_ERR_DATA_TOO_LONG);

			if (info.c64DataLen == 0)
				throw new LoadErrorException(Resources.IDS_SID_ERR_EMPTY);

			cache = buf;
		}



		/********************************************************************/
		/// <summary>
		/// Convert 32-bit PSID-style speed word to internal tables
		/// </summary>
		/********************************************************************/
		protected void ConvertOldStyleSpeedToTables(uint_least32_t speed, SidTuneInfo.clock_t clock)
		{
			// Create the speed/clock setting tables
			//
			// This routine implements the PSIDv2NG compliant speed conversion. All tunes
			// above 32 use the same song speed as tune 32
			uint toDo = Math.Min(info.songs, MAX_SONGS);

			for (uint s = 0; s < toDo; s++)
			{
				clockSpeed[s] = clock;
				songSpeed[s] = (byte)((speed & 1) != 0 ? SidTuneInfo.SPEED_CIA_1A : SidTuneInfo.SPEED_VBI);

				if (s < 31)
					speed >>= 1;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Convert the comment to the right unicode characters
		/// </summary>
		/********************************************************************/
		protected IEnumerable<string> GetPetsciiStrings(byte[] buf, int index, int length, bool lyrics)
		{
			char[] tempBuffer = new char[length];

			ushort[] lowerLookupTable = lyrics ? chrTabUpper : chrTabLower;
			ushort[] upperLookupTable = lyrics ? chrTabLowerLyrics : chrTabUpper;

			ushort[] lookupTable = upperLookupTable;
			bool reversed = false;

			while (length > 0)
			{
				int destIndex = 0;

				for (; length > 0;)
				{
					byte b = buf[index++];
					length--;

					// Handle special characters
					if ((b == 0x00) || (b == 0x0d))
						break;

					switch (b)
					{
						// Lower case
						case 0x0e:
						{
							lookupTable = lowerLookupTable;
							break;
						}

						// Upper case
						case 0x8e:
						{
							lookupTable = upperLookupTable;
							break;
						}

						// Reverse on
						case 0x12:
						{
							reversed = true;
							break;
						}

						// Reverse off
						case 0x92:
						{
							reversed = false;
							break;
						}

						// Cursor left
						case 0x9d:
						{
							if (destIndex > 0)
								destIndex--;

							break;
						}
					}
					
					ushort v = lookupTable[b];
					if (v != 0x0001)
					{
						if (reversed)
							v += 0x80;

						tempBuffer[destIndex++] = (char)v;
					}
				}

				yield return tempBuffer.AsSpan(0, destIndex).ToString();
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Does not affect status of object, and therefore can be used to
		/// load files. Error string is put into info.statusString, though
		/// </summary>
		/********************************************************************/
		private static byte[] LoadFile(ModuleStream moduleStream)
		{
			if (moduleStream.Length == 0)
				throw new LoadErrorException(Resources.IDS_SID_ERR_EMPTY);

			byte[] fileBuf = new byte[moduleStream.Length];

			moduleStream.Seek(0, SeekOrigin.Begin);
			int bytesRead = moduleStream.Read(fileBuf, 0, fileBuf.Length);
			if (bytesRead != fileBuf.Length)
				throw new LoadErrorException(Resources.IDS_SID_ERR_CANT_LOAD_FILE);

			return fileBuf;
		}



		/********************************************************************/
		/// <summary>
		/// Initializing the object based upon what we find in the specified
		/// file
		/// </summary>
		/********************************************************************/
		private static SidTuneBase GetFromFiles(PlayerFileInfo fileInfo, string[] fileNameExtensions)
		{
			byte[] fileBuf1 = LoadFile(fileInfo.ModuleStream);

			// File loaded. Now check if it is a valid single-file-format
			SidTuneBase s = PSid.Load(fileBuf1);
			if (s == null)
			{
				// Try some native C64 file formats
				s = Mus.Load(fileBuf1, true);
				if (s != null)
				{
					// Try to load lyrics
					byte[] lyricsBuf = null;

					using (ModuleStream moduleStream = fileInfo.Loader.OpenExtraFile("wds"))
					{
						if (moduleStream != null)
						{
							lyricsBuf = LoadFile(moduleStream);
							((Mus)s).AddLyrics(lyricsBuf);
						}
					}

					// Try to find the second file
					foreach (string extension in fileNameExtensions)
					{
						foreach (string fileName2 in fileInfo.Loader.GetPossibleFileNames(extension))
						{
							// 1st data file was loaded into "fileBuf1",
							// so we load the 2nd one into "fileBuf2".
							// Do not load the first file again if names are equal
							if (fileInfo.FileName != fileName2)
							{
								try
								{
									using (ModuleStream moduleStream = fileInfo.Loader.OpenExtraFile(fileName2, false))
									{
										if (moduleStream != null)
										{
											byte[] fileBuf2 = LoadFile(moduleStream);

											// Check if tunes in wrong order and therefore swap them here
											if (extension == "mus")
											{
												SidTuneBase s2 = Mus.Load(fileBuf2, fileBuf1, lyricsBuf, 0, true, out byte[] newBuf);
												if (s2 != null)
												{
													fileInfo.Loader.AddSizes();

													s2.AcceptSidTune(newBuf);
													return s2;
												}
											}
											else
											{
												SidTuneBase s2 = Mus.Load(fileBuf1, fileBuf2, lyricsBuf, 0, true, out byte[] newBuf);
												if (s2 != null)
												{
													fileInfo.Loader.AddSizes();

													s2.AcceptSidTune(newBuf);
													return s2;
												}
											}
										}
									}
								}
								catch (LoadErrorException)
								{
									// The first tune loaded ok, so ignore errors on the
									// second tune, may find an ok one later
								}
							}
						}
					}
				}
			}

			if (s == null)
				return null;

			s.AcceptSidTune(fileBuf1);

			return s;
		}



		/********************************************************************/
		/// <summary>
		/// Check for valid relocation information
		/// </summary>
		/********************************************************************/
		private bool CheckRelocInfo()
		{
			// Fix relocation information
			if (info.relocStartPage == 0xff)
			{
				info.relocPages = 0;
				return true;
			}

			if (info.relocPages == 0)
			{
				info.relocStartPage = 0;
				return true;
			}

			// Calculate start/end page
			uint_least8_t startP = info.relocStartPage;
			uint_least8_t endP = (uint_least8_t)((startP + info.relocPages - 1) & 0xff);
			if (endP < startP)
				return false;

			{	// Check against load range
				uint_least8_t startLp = (uint_least8_t)(info.loadAddr >> 8);
				uint_least8_t endLp = (uint_least8_t)(startLp + (uint_least8_t)((info.c64DataLen - 1) >> 8));

				if (((startP <= startLp) && (endP >= startLp)) || ((startP <= endLp) && (endP >= endLp)))
					return false;
			}

			// Check that the relocation information does not use the following
			// memory areas: 0x0000-0x03FF, 0xA000-0xBFFF and 0xD000-0xFFFF
			if ((startP < 0x04) || ((0xa0 <= startP) && (startP <= 0xbf)) || (startP >= 0xd0) || ((0xa0 <= endP) && (endP <= 0xbf)) || (endP >= 0xd0))
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Common address resolution procedure
		/// </summary>
		/********************************************************************/
		private void ResolveAddrs(uint8_t[] c64Data, uint offset)
		{
			// Originally used as a first attempt at an RSID
			// style format. Now reserved for future use
			if (info.playAddr == 0xffff)
				info.playAddr = 0;

			// loadAddr = 0 means, the address is stored in front of the C64 data
			if (info.loadAddr == 0)
			{
				if (info.c64DataLen < 2)
					throw new LoadErrorException(Resources.IDS_SID_ERR_CORRUPT);

				info.loadAddr = SidEndian.Endian_16(c64Data[offset + 1], c64Data[offset]);
				fileOffset += 2;
				info.c64DataLen -= 2;
			}

			if (info.compatibility == SidTuneInfo.compatibility_t.COMPATIBILITY_BASIC)
			{
				if (info.initAddr != 0)
					throw new LoadErrorException(Resources.IDS_SID_ERR_BAD_ADDR);
			}
			else if (info.initAddr == 0)
				info.initAddr = info.loadAddr;
		}



		/********************************************************************/
		/// <summary>
		/// Check if compatibility constraints are fulfilled
		/// </summary>
		/********************************************************************/
		private bool CheckCompatibility()
		{
			if (info.compatibility == SidTuneInfo.compatibility_t.COMPATIBILITY_R64)
			{
				// Check valid init address
				switch (info.initAddr >> 12)
				{
					case 0x0a:
					case 0x0b:
					case 0x0d:
					case 0x0e:
					case 0x0f:
						return false;

					default:
					{
						if ((info.initAddr < info.loadAddr) || (info.initAddr > (info.loadAddr + info.c64DataLen - 1)))
							return false;

						break;
					}
				}

				// Check tune is loadable on a real C64
				if (info.loadAddr < SIDTUNE_R64_MIN_LOAD_ADDR)
					return false;
			}

			return true;
		}
		#endregion
	}
}
