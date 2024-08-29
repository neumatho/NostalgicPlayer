/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Security.Cryptography;
using System.Text;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.Exceptions;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.SidPlayFp;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.SidTune
{
	/// <summary>
	/// Handle loading of the PSID file format
	/// </summary>
	internal sealed class PSid : SidTuneBase
	{
		private const int PSid_MaxStrLen = 32;

		/// <summary>
		/// Header has been extended for 'RSID' format
		/// The following changes are present:
		///     Id = 'RSID'
		///     Version = 2, 3 or 4
		///     Play, Load and Speed reserved 0
		///     PSidSpecific flag is called C64BASIC flag
		///     Init cannot be under ROMS/IO memory area
		///     Load address cannot be less than 0x07E8
		///     Info strings may be 32 characters long without trailing zero
		/// </summary>
		private class PSidHeader
		{
			public uint32_t Id;									// 'PSID' or 'RSID' (ASCII)
			public uint16_t Version;							// 1, 2, 3 or 4
			public uint16_t Data;								// 16-bit offset to binary data in file
			public uint16_t Load;								// 16-bit C64 address to load file to
			public uint16_t Init;								// 16-bit C64 address of init subroutine
			public uint16_t Play;								// 16-bit C64 address of play subroutine
			public uint16_t Songs;								// Number of songs
			public uint16_t Start;								// Start song out of [1..256]
			public uint32_t Speed;								// 32-bit speed info
																// bit: 0=50 Hz, 1=CIA 1 Timer A (default: 60 Hz)
			public byte[] Name = new byte[PSid_MaxStrLen];		// ASCII strings, 31 characters long and
			public byte[] Author = new byte[PSid_MaxStrLen];	// terminated by a trailing zero
			public byte[] Released = new byte[PSid_MaxStrLen];
			public uint16_t Flags;								// Only version >= 2
			public uint8_t RelocStartPage;						// Only version >= 2ng
			public uint8_t RelocPages;							// Only version >= 2ng
			public uint8_t SidChipBase2;						// Only version >= 3
			public uint8_t SidChipBase3;						// Only version >= 4
		}

		private const uint PSID_MUS = 1 << 0;
		private const uint PSID_SPECIFIC = 1 << 1;				// These two are mutually exclusive
		private const uint PSID_BASIC = 1 << 1;
		private const uint PSID_CLOCK = 3 << 2;
		private const uint PSID_SIDMODEL = 3 << 4;

		private const uint PSID_CLOCK_UNKNOWN = 0;
		private const uint PSID_CLOCK_PAL = 1 << 2;
		private const uint PSID_CLOCK_NTSC = 1 << 3;
		private const uint PSID_CLOCK_ANY = PSID_CLOCK_PAL | PSID_CLOCK_NTSC;

		private const uint PSID_SIDMODEL_UNKNOWN = 0;
		private const uint PSID_SIDMODEL_6581 = 1;
		private const uint PSID_SIDMODEL_8580 = 2;
		private const uint PSID_SIDMODEL_ANY = PSID_SIDMODEL_6581 | PSID_SIDMODEL_8580;

		private const int PSid_HeaderSize = 118;
		private const int PSidV2_HeaderSize = PSid_HeaderSize + 6;

		// Magic fields
		private const uint PSID_ID = 0x50534944;
		private const uint RSID_ID = 0x52534944;

		/********************************************************************/
		/// <summary>
		/// Will try to load the file as a PSID file. Return an object if
		/// recognized as a PSID file, else NULL. If any errors occurred
		/// while loading, an LoadErrorException is thrown
		/// </summary>
		/********************************************************************/
		public static SidTuneBase Load(byte[] dataBuf)
		{
			// File format check
			if (dataBuf.Length < 4)
				return null;

			uint32_t magic = SidEndian.Endian_Big32(dataBuf, 0);
			if ((magic != PSID_ID) && (magic != RSID_ID))
				return null;

			PSidHeader header = ReadHeader(dataBuf);

			PSid tune = new PSid();
			tune.TryLoad(header);

			return tune;
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the MD5 hash of the tune
		/// </summary>
		/********************************************************************/
		public override byte[] CreateMD5New()
		{
			using (MD5 md5 = MD5.Create())
			{
				// The calculation is now simplified.
				// All the header + all the data
				return md5.ComputeHash(cache, 0, cache.Length);
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Decode SID model flags
		/// </summary>
		/********************************************************************/
		private SidTuneInfo.model_t GetSidModel(uint_least16_t modelFlag)
		{
			if ((modelFlag & PSID_SIDMODEL_ANY) == PSID_SIDMODEL_ANY)
				return SidTuneInfo.model_t.SIDMODEL_ANY;

			if ((modelFlag & PSID_SIDMODEL_6581) != 0)
				return SidTuneInfo.model_t.SIDMODEL_6581;

			if ((modelFlag & PSID_SIDMODEL_8580) != 0)
				return SidTuneInfo.model_t.SIDMODEL_8580;

			return SidTuneInfo.model_t.SIDMODEL_UNKNOWN;
		}



		/********************************************************************/
		/// <summary>
		/// Check if extra SID address is valid for PSID specs.
		/// </summary>
		/********************************************************************/
		private bool ValidateAddress(uint_least8_t address)
		{
			// Only even values are valid
			if ((address & 1) != 0)
				return false;

			// Ranges $00-$41 ($D000-$D410) and $80-$DF ($D800-$DDF0) are invalid.
			// Any invalid value means that no second SID is used, like $00
			if ((address <= 0x41) || ((address >= 0x80) && (address <= 0xdf)))
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read PSID file header
		/// </summary>
		/********************************************************************/
		private static PSidHeader ReadHeader(byte[] dataBuf)
		{
			// Due to security concerns, input must be at least as long as version 1
			// header plus 16-bit C64 load address. That is the area which will be
			// accessed
			if (dataBuf.Length < (PSid_HeaderSize + 2))
				throw new LoadErrorException(Resources.IDS_SID_ERR_TRUNCATED);

			PSidHeader hdr = new PSidHeader();

			// Read v1 fields
			hdr.Id = SidEndian.Endian_Big32(dataBuf, 0);
			hdr.Version = SidEndian.Endian_Big16(dataBuf, 4);
			hdr.Data = SidEndian.Endian_Big16(dataBuf, 6);
			hdr.Load = SidEndian.Endian_Big16(dataBuf, 8);
			hdr.Init = SidEndian.Endian_Big16(dataBuf, 10);
			hdr.Play = SidEndian.Endian_Big16(dataBuf, 12);
			hdr.Songs = SidEndian.Endian_Big16(dataBuf, 14);
			hdr.Start = SidEndian.Endian_Big16(dataBuf, 16);
			hdr.Speed = SidEndian.Endian_Big32(dataBuf, 18);

			Array.Copy(dataBuf, 22, hdr.Name, 0, PSid_MaxStrLen);
			Array.Copy(dataBuf, 54, hdr.Author, 0, PSid_MaxStrLen);
			Array.Copy(dataBuf, 86, hdr.Released, 0, PSid_MaxStrLen);

			if (hdr.Version >= 2)
			{
				if (dataBuf.Length < (PSidV2_HeaderSize + 2))
					throw new LoadErrorException(Resources.IDS_SID_ERR_TRUNCATED);

				// Read v2/3/4 fields
				hdr.Flags = SidEndian.Endian_Big16(dataBuf, 118);
				hdr.RelocStartPage = dataBuf[120];
				hdr.RelocPages = dataBuf[121];
				hdr.SidChipBase2 = dataBuf[122];
				hdr.SidChipBase3 = dataBuf[123];
			}

			return hdr;
		}



		/********************************************************************/
		/// <summary>
		/// Load PSID file
		/// </summary>
		/********************************************************************/
		private void TryLoad(PSidHeader header)
		{
			Encoding encoder = EncoderCollection.Amiga;

			SidTuneInfo.compatibility_t compatibility = SidTuneInfo.compatibility_t.COMPATIBILITY_C64;

			// Require a valid ID and version number
			if (header.Id == PSID_ID)
			{
				switch (header.Version)
				{
					case 1:
					{
						compatibility = SidTuneInfo.compatibility_t.COMPATIBILITY_PSID;
						break;
					}

					case 2:
					case 3:
					case 4:
						break;

					default:
						throw new LoadErrorException(Resources.IDS_SID_ERR_UNKNOWN_PSID);
				}

				info.formatString = Resources.IDS_SID_FORMAT_PSID;
			}
			else if (header.Id == RSID_ID)
			{
				switch (header.Version)
				{
					case 2:
					case 3:
					case 4:
						break;

					default:
						throw new LoadErrorException(Resources.IDS_SID_ERR_UNKNOWN_RSID);
				}

				info.formatString = Resources.IDS_SID_FORMAT_RSID;
				compatibility = SidTuneInfo.compatibility_t.COMPATIBILITY_R64;
			}

			fileOffset = header.Data;
			info.loadAddr = header.Load;
			info.initAddr = header.Init;
			info.playAddr = header.Play;
			info.songs = header.Songs;
			info.startSong = header.Start;
			info.compatibility = compatibility;
			info.relocPages = 0;
			info.relocStartPage = 0;

			uint_least32_t speed = header.Speed;
			SidTuneInfo.clock_t clock = SidTuneInfo.clock_t.CLOCK_UNKNOWN;

			bool musPlayer = false;

			if (header.Version >= 2)
			{
				uint_least16_t flags = header.Flags;

				// Check clock
				if ((flags & PSID_MUS) != 0)
				{
					// MUS tunes run at any speed
					clock = SidTuneInfo.clock_t.CLOCK_ANY;
					musPlayer = true;
				}
				else
				{
					switch (flags & PSID_CLOCK)
					{
						case PSID_CLOCK_ANY:
						{
							clock = SidTuneInfo.clock_t.CLOCK_ANY;
							break;
						}

						case PSID_CLOCK_PAL:
						{
							clock = SidTuneInfo.clock_t.CLOCK_PAL;
							break;
						}

						case PSID_CLOCK_NTSC:
						{
							clock = SidTuneInfo.clock_t.CLOCK_NTSC;
							break;
						}
					}
				}

				// These flags is only available for the appropriate
				// file formats
				switch (compatibility)
				{
					case SidTuneInfo.compatibility_t.COMPATIBILITY_C64:
					{
						if ((flags & PSID_SPECIFIC) != 0)
							info.compatibility = SidTuneInfo.compatibility_t.COMPATIBILITY_PSID;

						break;
					}

					case SidTuneInfo.compatibility_t.COMPATIBILITY_R64:
					{
						if ((flags & PSID_BASIC) != 0)
							info.compatibility = SidTuneInfo.compatibility_t.COMPATIBILITY_BASIC;

						break;
					}
				}

				info.clockSpeed = clock;

				info.sidModels[0] = GetSidModel((uint_least16_t)(flags >> 4));

				info.relocStartPage = header.RelocStartPage;
				info.relocPages = header.RelocPages;

				if (header.Version >= 3)
				{
					if (ValidateAddress(header.SidChipBase2))
					{
						info.sidChipAddresses.Add((uint_least16_t)(0xd000 | (header.SidChipBase2 << 4)));
						info.sidModels.Add(GetSidModel((uint_least16_t)(flags >> 6)));
					}

					if (header.Version >= 4)
					{
						if ((header.SidChipBase3 != header.SidChipBase2) && ValidateAddress(header.SidChipBase3))
						{
							info.sidChipAddresses.Add((uint_least16_t)(0xd000 | (header.SidChipBase3 << 4)));
							info.sidModels.Add(GetSidModel((uint_least16_t)(flags >> 8)));
						}
					}
				}
			}

			// Check reserved fields to force real C64 compliance
			// as required by the RSID specification
			if (compatibility == SidTuneInfo.compatibility_t.COMPATIBILITY_R64)
			{
				if ((info.loadAddr != 0) || (info.playAddr != 0) || (speed != 0))
					throw new LoadErrorException(Resources.IDS_SID_ERR_INVALID);

				// Real C64 tunes appear as CIA
				speed = ~(uint_least32_t)0;
			}

			// Create the speed/clock setting table
			ConvertOldStyleSpeedToTables(speed, clock);

			// Copy the info strings
			info.infoString.Add(encoder.GetString(header.Name));
			info.infoString.Add(encoder.GetString(header.Author));
			info.infoString.Add(encoder.GetString(header.Released));

			if (musPlayer)
				throw new LoadErrorException(Resources.IDS_SID_ERR_PSID_MUS);
		}
		#endregion
	}
}
