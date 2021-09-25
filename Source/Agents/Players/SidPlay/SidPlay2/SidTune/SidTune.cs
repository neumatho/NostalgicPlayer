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

		// For two-file formats: holds the full path to the second file
		private string secondFileName;

		// The two files need to be swapped
		private bool swapFileData;

		// Needed for MUS/STR player installation
		private uint musDataLen;

		// Holds the C64 module
		private byte[] cache;

		private SidTuneInfo info;
		private bool status;

		private Speed[] songSpeed;
		private Clock[] clockSpeed;

		private static readonly string[] fileNameExtensions =
		{
			// Preferred default file extension for single-file sid tunes
			// or sid tune description files in SIDPLAY INFOFILE format
			"sid",

			// Common file extension for single-file sid tunes due to SIDPLAY/DOS
			// displaying files *.DAT in its file selector by default.
			// Originally this was intended to be the extension of the raw data file
			// of two-file sid tunes in SIDPLAY INFOFILE format
			"dat",

			// Extension of Amiga Workbench tool type icon info files, which
			// have been cut to MS-DOS file name length (8.3)
			"inf",

			// No extension for the raw data file of two-file sid tunes in
			// PlaySID Amiga Workbench tool type icon info format
			string.Empty,

			// File extensions used (and created) by various C64 emulators and
			// related utilities. These extensions are recommended to be used as
			// a replacement for ".dat" in conjunction with two-file sid tunes
			"c64", "prg", "p00",

			// Uncut extensions from Amiga
			"info", "data",

			// Stereo Sidplayer (.mus ought not be included because
			// these must be loaded first; it sometimes contains the first
			// credit lines of a mus/str pair)
			"str", "mus"
		};

		/********************************************************************/
		/// <summary>
		/// Will test the file to see if it's one of the known formats
		/// </summary>
		/********************************************************************/
		public bool Test(PlayerFileInfo fileInfo)
		{
			// Check for single file formats
			if (TestPSid(fileInfo.ModuleStream))
				return true;

			// ----- Support for multiple-files format

			// We cannot simply try to load additional files, if a description file
			// was specified. It would work, but is error-prone. Imagine a filename
			// mismatch or more than one description file (in another) format.
			// Any other file with an appropriate file name can be the C64 data file

			// First we see if the file could be a raw data file. In that case, we
			// have to find the corresponding description file

			// Right now, we do not have a second file. This will not hurt the file
			// support procedures

			// Make sure that the file does not contain a description file
			if (!TestSid(fileInfo.ModuleStream) && !TestInfo(fileInfo.ModuleStream))
			{
				// Assuming the file is the raw data file, we will now
				// try with different extensions to check for a description file
				foreach (string extension in fileNameExtensions)
				{
					using (ModuleStream moduleStream = fileInfo.Loader.OpenExtraFileForTest(extension, out string newFileName))
					{
						if (moduleStream != null)
						{
							// Skip check, if opened file is the same as the current one
							if (fileInfo.FileName != newFileName)
							{
								if (TestSid(moduleStream) || TestInfo(moduleStream))
								{
									secondFileName = newFileName;
									swapFileData = false;

									return true;
								}
							}
						}
					}
				}

				// Could not find a description file, try some native C64 file formats
				if (TestMus(fileInfo.ModuleStream))
				{
					sidType = SidType.Mus;

					// Try to find second file
					foreach (string extension in fileNameExtensions)
					{
						using (ModuleStream moduleStream = fileInfo.Loader.OpenExtraFileForTest(extension, out string newFileName))
						{
							if (moduleStream != null)
							{
								// Skip check, if opened file is the same as the current one
								if (fileInfo.FileName != newFileName)
								{
									// Check if tunes in wrong order and therefore swap them
									if (extension == "mus")
									{
										if (TestMus(moduleStream))
										{
											secondFileName = newFileName;
											swapFileData = true;

											return true;
										}
									}
									else
									{
										if (TestMus(moduleStream))
										{
											secondFileName = newFileName;
											swapFileData = false;

											return true;
										}
									}

									// The first tune loaded ok, so ignore errors on the
									// second tune, may find an ok one later
								}
							}
						}
					}

					// No (suitable) second file, but that's ok
					return true;
				}
			}
			else
			{
				// Seems like the description file has been selected to be loaded, so now
				// try to find the data file
				foreach (string extension in fileNameExtensions)
				{
					using (ModuleStream moduleStream = fileInfo.Loader.OpenExtraFileForTest(extension, out string newFileName))
					{
						if (moduleStream != null)
						{
							// Skip check, if opened file is the same as the current one
							if (fileInfo.FileName != newFileName)
							{
								// Found a data file, so we assume it is ok, but indicate that
								// the files need to be swapped in the loader
								secondFileName = newFileName;
								swapFileData = true;

								return true;
							}
						}
					}
				}
			}

			return false;
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
			byte[] fileBuf1 = LoadFile(fileInfo.ModuleStream);
			if (fileBuf1 == null)
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
					status = LoadPSid(fileBuf1, out errorMessage);
					if (status)
						status = AcceptSidTune(fileBuf1, out errorMessage);

					break;
				}

				case SidType.SidInfo:
				case SidType.IconInfo:
				{
					byte[] fileBuf2 = LoadSecondFile(fileInfo);
					if (fileBuf2 == null)
					{
						status = false;
						errorMessage = string.Format(Resources.IDS_SID_ERR_CANNOT_OPEN_EXTRA_FILE, secondFileName);
						return;
					}

					if (swapFileData)
						(fileBuf1, fileBuf2) = (fileBuf2, fileBuf1);

					if (sidType == SidType.SidInfo)
						status = LoadSid(fileBuf2, out errorMessage);
					else
						status = LoadInfo(fileBuf2, out errorMessage);

					if (status)
						status = AcceptSidTune(fileBuf1, out errorMessage);

					break;
				}

				case SidType.Mus:
				{
					byte[] fileBuf2 = null;
					byte[] fileBuf3 = null;

					if (!string.IsNullOrEmpty(secondFileName))
					{
						fileBuf2 = LoadSecondFile(fileInfo);
						if (fileBuf2 == null)
						{
							status = false;
							errorMessage = string.Format(Resources.IDS_SID_ERR_CANNOT_OPEN_EXTRA_FILE, secondFileName);
							return;
						}

						if (swapFileData)
							(fileBuf1, fileBuf2) = (fileBuf2, fileBuf1);
					}

					// Check if lyrics exists, then load them
					using (ModuleStream moduleStream = fileInfo.Loader.OpenExtraFile("wds"))
					{
						if (moduleStream != null)
							fileBuf3 = LoadFile(moduleStream);
					}

					status = LoadMus(fileBuf1, fileBuf2, fileBuf3, out errorMessage);
					if (status)
					{
						status = MergeParts(fileBuf1, fileBuf2, out byte[] newBuf, out errorMessage);
						if (status)
							status = AcceptSidTune(newBuf, out errorMessage);
					}
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
		/// Return the buffer holding the loaded file
		/// </summary>
		/********************************************************************/
		public byte[] GetLoadedFile()
		{
			return cache;
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

				if (info.MusPlayer)
					InstallMusPlayer(c64Buf);

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
		/// Load the whole file into memory
		/// </summary>
		/********************************************************************/
		private byte[] LoadFile(ModuleStream moduleStream)
		{
			byte[] fileBuf = new byte[moduleStream.Length];

			moduleStream.Seek(0, SeekOrigin.Begin);
			int bytesRead = moduleStream.Read(fileBuf, 0, fileBuf.Length);
			if (bytesRead != fileBuf.Length)
				return null;

			return fileBuf;
		}



		/********************************************************************/
		/// <summary>
		/// Load the whole second file into memory
		/// </summary>
		/********************************************************************/
		private byte[] LoadSecondFile(PlayerFileInfo fileInfo)
		{
			using (ModuleStream moduleStream = fileInfo.Loader.OpenExtraFileWithName(secondFileName))
			{
				if (moduleStream == null)
					return null;

				return LoadFile(moduleStream);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Accept the loaded tune and do the last fixups
		/// </summary>
		/********************************************************************/
		private bool AcceptSidTune(byte[] buf, out string errorMessage)
		{
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

			if (info.MusPlayer)
				SetPlayerAddress();

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



		/********************************************************************/
		/// <summary>
		/// Will read a hex number from the string and return it
		/// </summary>
		/********************************************************************/
		private uint ReadHex(string line, ref int nextPos)
		{
			uint result = 0;

			while (nextPos < line.Length)
			{
				char c = line[nextPos++];

				if ((c == ',') || (c == ':'))
					break;

				int b = c & 0xdf;
				if (b < 0x3a)
					b &= 0x0f;
				else
					b -= (0x41 - 0x0a);

				result <<= 4;
				result |= (uint)b;
			}

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Will read a decimal number from the string and return it
		/// </summary>
		/********************************************************************/
		private uint ReadDec(string line, ref int nextPos)
		{
			uint result = 0;

			while (nextPos < line.Length)
			{
				char c = line[nextPos++];

				if ((c == ',') || (c == ':'))
					break;

				int b = c & 0x0f;
				result *= 10;
				result += (uint)b;
			}

			return result;
		}
		#endregion
	}
}
