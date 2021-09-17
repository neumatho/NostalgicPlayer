/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.SidTune
{
	/// <summary>
	/// Handling loading of a SID tune
	/// </summary>
	internal partial class SidTune
	{
		private const ushort MaxSongs = 256;
		private const uint MaxMemory = 65536;

		private const ushort R64MinLoadAddr = 0x07e8;

		private SidType sidType;

		// For files with header: offset to real data
		private uint fileOffset;

		// Needed for MUS/STR player installation
		private uint musDataLen;

		// Holds the C64 module
		private byte[] cache;

		private SidTuneInfo info;
		private bool status;

		private Speed[] songSpeed;
		private Clock[] clockSpeed;

		/********************************************************************/
		/// <summary>
		/// Will test the file to see if it's one of the known formats
		/// </summary>
		/********************************************************************/
		public bool Test(PlayerFileInfo fileInfo)
		{
			// Check for single file formats
			bool result = TestPSid(fileInfo.ModuleStream);

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Will load the module into memory
		/// </summary>
		/********************************************************************/
		public void Load(PlayerFileInfo fileInfo, out string errorMessage)
		{
			Init();

			// Load the whole file into memory
			byte[] fileBuf1 = new byte[fileInfo.ModuleStream.Length];

			fileInfo.ModuleStream.Seek(0, SeekOrigin.Begin);
			int bytesRead = fileInfo.ModuleStream.Read(fileBuf1, 0, fileBuf1.Length);
			if (bytesRead != fileBuf1.Length)
			{
				status = false;
				errorMessage = Resources.IDS_SID_ERR_CORRUPT;
				return;
			}

			// Now parse the loaded data based on the found type
			switch (sidType)
			{
				case SidType.PSid:
				case SidType.RSid:
				{
					LoadPSid(fileBuf1, out errorMessage);
					if (string.IsNullOrEmpty(errorMessage))
						status = AcceptSidTune(fileBuf1, out errorMessage);

					break;
				}

				default:
					throw new NotImplementedException();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the tune information
		/// </summary>
		/********************************************************************/
		public SidTuneInfo GetInfo()
		{
			return info;
		}



		/********************************************************************/
		/// <summary>
		/// First check, whether a song is valid. Then copy any song-specific
		/// variable information such a speed/clock setting to the info
		/// structure
		/// </summary>
		/********************************************************************/
		public ushort SelectSong(ushort selectedSong)
		{
			if (!status)
				return 0;

			ushort song = selectedSong;

			// Determine and set starting song number
			if (selectedSong == 0)
				song = info.StartSong;

			if ((selectedSong > info.Songs) || (selectedSong > MaxSongs))
				song = info.StartSong;

			info.CurrentSong = song;

			// Retrieve song speed definition
			if (info.Compatibility == Compatibility.R64)
				info.SongSpeed = Speed.Cia_1A;
			else if (info.Compatibility == Compatibility.PSid)
			{
				// This does not take into account the PlaySID bug upon evaluating the
				// SPEED field. It would most likely break compatibility to lots of
				// sid tunes, which have been converted from .SID format and vice versa.
				// The .SID format does the bit-wise/song-wise evaluation of the SPEED
				// value correctly, like it is described in the PlaySID documentation
				info.SongSpeed = songSpeed[(song - 1) & 31];
			}
			else
				info.SongSpeed = songSpeed[song - 1];

			info.ClockSpeed = clockSpeed[song - 1];

			// Assign song speed description string depending on clock speed.
			// Final speed description is available only after song init
			if (info.SongSpeed == Speed.Vbi)
				info.SpeedString = Resources.IDS_SID_SPEED_VBI;
			else
				info.SpeedString = Resources.IDS_SID_SPEED_CIA;

			return info.CurrentSong;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool PlaceSidTuneInC64Mem(byte[] c64Buf)
		{
			if (status && (c64Buf != null))
			{
				// The Basic ROM sets these values on loading a file
				// Program end address
				ushort start = info.LoadAddr;
				ushort end = (ushort)(start + info.C64DataLen);

				Endian.EndianLittle16(c64Buf, 0x2d, end);		// Variables start
				Endian.EndianLittle16(c64Buf, 0x2f, end);		// Arrays start
				Endian.EndianLittle16(c64Buf, 0x31, end);		// Strings start
				Endian.EndianLittle16(c64Buf, 0xac, start);
				Endian.EndianLittle16(c64Buf, 0xae, end);

				uint endPos = info.LoadAddr + info.C64DataLen;
				if (endPos <= MaxMemory)
				{
					// Copy data from cache to the correct destination
					Array.Copy(cache, fileOffset, c64Buf, info.LoadAddr, info.C64DataLen);
				}
				else
				{
					// Security - cut data which would exceed the end of the C64
					// memory. Array.Copy could not detect this.
					//
					// NOTE: In libsidplay1 the rest gets wrapped to the beginning
					// of the C64 memory. It is an undocumented hack most likely not
					// used by any sid tune. Here we no longer do it like that, set
					// an error message, and hope the modified behaviour will find
					// a few badly ripped sids
					Array.Copy(cache, fileOffset, c64Buf, info.LoadAddr, info.C64DataLen - (endPos - MaxMemory));
				}

//XX				if (info.MusPlayer)
//					MusInstallPlayer(c64Buf);

				return true;
			}

			return false;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Initialize the object with some safe defaults
		/// </summary>
		/********************************************************************/
		private void Init()
		{
			status = false;

			info = new SidTuneInfo();

			info.C64DataLen = 0;
			info.FormatString = Resources.IDS_SID_NA;
			info.SpeedString = Resources.IDS_SID_NA;
			info.LoadAddr = info.InitAddr = info.PlayAddr = 0;
			info.Songs = info.StartSong = info.CurrentSong = 0;
			info.SidChipBase1 = 0xd400;
			info.SidChipBase2 = 0;
			info.MusPlayer = false;
			info.FixLoad = false;
			info.SongSpeed = Speed.Vbi;
			info.ClockSpeed = Clock.Unknown;
			info.SidModel1 = SidModel.Unknown;
			info.SidModel2 = SidModel.Unknown;
			info.Compatibility = Compatibility.C64;
			info.RelocStartPage = 0;
			info.RelocPages = 0;

			songSpeed = new Speed[MaxSongs];
			clockSpeed = new Clock[MaxSongs];

			for (int si = 0; si < MaxSongs; si++)
			{
				songSpeed[si] = info.SongSpeed;
				clockSpeed[si] = info.ClockSpeed;
			}

			fileOffset = 0;
			musDataLen = 0;

			info.Title = string.Empty;
			info.Author = string.Empty;
			info.Released = string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Accept the loaded tune and do the last fixups
		/// </summary>
		/********************************************************************/
		private bool AcceptSidTune(byte[] buf, out string errorMessage)
		{
			errorMessage = string.Empty;

			// Add <?> (HVSC standard) to missing title, author and release fields
			if (string.IsNullOrEmpty(info.Title))
				info.Title = Resources.IDS_SID_UNKNOWN_INFO;

			if (string.IsNullOrEmpty(info.Author))
				info.Author = Resources.IDS_SID_UNKNOWN_INFO;

			if (string.IsNullOrEmpty(info.Released))
				info.Released = Resources.IDS_SID_UNKNOWN_INFO;

			// Fix bad sid tune setup
			if (info.Songs > MaxSongs)
				info.Songs = MaxSongs;
			else if (info.Songs == 0)
				info.Songs++;

			if (info.StartSong > info.Songs)
				info.StartSong = 1;
			else if (info.StartSong == 0)
				info.StartSong++;

//XX			if (info.MusPlayer)
//				SetMusPlayerAddress();

			info.C64DataLen = (uint)(buf.Length - fileOffset);

			// Calculate any remaining addresses and then
			// confirm all the file details are correct
			if (!ResolveAddrs(info, buf, fileOffset, out errorMessage))
				return false;

			if (!CheckRelocInfo(info, out errorMessage))
				return false;

			if (!CheckCompatibility(info, out errorMessage))
				return false;

			if (buf.Length >= 2)
			{
				// We only detect an offset of two. Some positions independent
				// sid tunes contain a load address of 0xE000, but are loaded
				// to 0x0FFE and call player at 0x1000
				info.FixLoad = Endian.EndianLittle16(buf, fileOffset) == info.LoadAddr + 2;
			}

			// Check the size of the data
			if (info.C64DataLen > MaxMemory)
			{
				errorMessage = Resources.IDS_SID_ERR_DATA_TOO_LONG;
				return false;
			}

			if (info.C64DataLen == 0)
			{
				errorMessage = Resources.IDS_SID_ERR_EMPTY;
				return false;
			}

			// Load the module data
			cache = buf;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Check and fix addresses
		/// </summary>
		/********************************************************************/
		private bool ResolveAddrs(SidTuneInfo tuneInfo, byte[] c64Data, uint offset, out string errorMessage)
		{
			errorMessage = string.Empty;

			// Originally used as a first attempt at an RSID
			// style format. Now reserved for future use
			if (tuneInfo.PlayAddr == 0xffff)
				tuneInfo.PlayAddr = 0;

			// LoadAddr = 0 means, the address is stored in front of the C64 data
			if (tuneInfo.LoadAddr == 0)
			{
				if (tuneInfo.C64DataLen < 2)
				{
					errorMessage = Resources.IDS_SID_ERR_CORRUPT;
					return false;
				}

				tuneInfo.LoadAddr = Endian.Endian16(c64Data[offset + 1], c64Data[offset]);
				fileOffset += 2;
				tuneInfo.C64DataLen -= 2;
			}

			if (tuneInfo.Compatibility == Compatibility.Basic)
			{
				if (tuneInfo.InitAddr != 0)
				{
					errorMessage = Resources.IDS_SID_ERR_BAD_ADDR;
					return false;
				}
			}
			else if (tuneInfo.InitAddr == 0)
				tuneInfo.InitAddr = tuneInfo.LoadAddr;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Check relocation information
		/// </summary>
		/********************************************************************/
		private bool CheckRelocInfo(SidTuneInfo tuneInfo, out string errorMessage)
		{
			errorMessage = string.Empty;

			// Fix relocation information
			if (tuneInfo.RelocStartPage == 0xff)
			{
				tuneInfo.RelocPages = 0;
				return true;
			}

			if (tuneInfo.RelocPages == 0)
			{
				tuneInfo.RelocStartPage = 0;
				return true;
			}

			// Calculate start/end page
			byte startP = tuneInfo.RelocStartPage;
			byte endP = (byte)((startP + tuneInfo.RelocPages - 1) & 0xff);
			if (endP < startP)
			{
				errorMessage = Resources.IDS_SID_ERR_BAD_RELOC;
				return false;
			}

			{	// Check against load range
				uint startLp = (uint)(tuneInfo.LoadAddr >> 8);
				uint endLp = startLp;
				endLp += (tuneInfo.C64DataLen - 1) >> 8;

				if (((startP <= startLp) && (endP >= startLp)) || ((startP <= endLp) && (endP >= endLp)))
				{
					errorMessage = Resources.IDS_SID_ERR_BAD_RELOC;
					return false;
				}
			}

			// Check that the relocation information does not use the following
			// memory areas: 0x0000-0x03FF, 0xA000-0xBFFF and 0xD000-0xFFFF
			if ((startP < 0x04) || ((0xa0 <= startP) && (startP <= 0xbf)) || (startP >= 0xd0) || ((0xa0 <= endP) && (endP <= 0xbf)) || (endP >= 0xd0))
			{
				errorMessage = Resources.IDS_SID_ERR_BAD_RELOC;
				return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Check compatibility mode
		/// </summary>
		/********************************************************************/
		private bool CheckCompatibility(SidTuneInfo tuneInfo, out string errorMessage)
		{
			errorMessage = string.Empty;

			switch (tuneInfo.Compatibility)
			{
				case Compatibility.R64:
				{
					// Check valid init address
					switch (tuneInfo.InitAddr >> 12)
					{
						case 0x0f:
						case 0x0e:
						case 0x0d:
						case 0x0b:
						case 0x0a:
						{
							errorMessage = Resources.IDS_SID_ERR_BAD_ADDR;
							return false;
						}

						default:
						{
							if ((tuneInfo.InitAddr < tuneInfo.LoadAddr) || (tuneInfo.InitAddr > (tuneInfo.LoadAddr + tuneInfo.C64DataLen - 1)))
							{
								errorMessage = Resources.IDS_SID_ERR_BAD_ADDR;
								return false;
							}
							break;
						}
					}

					goto case Compatibility.Basic;
				}

				case Compatibility.Basic:
				{
					// Check tune is loadable on a real C64
					if (tuneInfo.LoadAddr < R64MinLoadAddr)
					{
						errorMessage = Resources.IDS_SID_ERR_BAD_ADDR;
						return false;
					}
					break;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Create the speed/clock setting tables
		///
		/// This routine implements the PSIDv2NG compliant speed conversion
		/// </summary>
		/********************************************************************/
		private void ConvertOldStyleSpeedToTables(uint speed, Clock clock)
		{
			// All tunes above 32 use the same song speed as tune 32
			int toDo = info.Songs <= MaxSongs ? info.Songs : MaxSongs;

			for (int s = 0; s < toDo; s++)
			{
				clockSpeed[s] = clock;

				if ((speed & 1) != 0)
					songSpeed[s] = Speed.Cia_1A;
				else
					songSpeed[s] = Speed.Vbi;

				if (s < 31)
					speed >>= 1;
			}
		}
		#endregion
	}
}
