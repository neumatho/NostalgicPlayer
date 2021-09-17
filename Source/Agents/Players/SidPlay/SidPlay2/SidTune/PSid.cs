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
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.SidTune
{
	/// <summary>
	/// Handle loading of the PSID file format
	/// </summary>
	internal partial class SidTune
	{
		[Flags]
		private enum PSidFlags
		{
			Mus = 1 << 0,
			Specific = 1 << 1,						// These two are mutually exclusive
			Basic = 1 << 1,
			Clock = 3 << 2,
			SidModel = 3 << 4
		}

		[Flags]
		private enum PSidClock
		{
			Unknown = 0,
			Pal = 1 << 2,
			Ntsc = 1 << 3,
			Any = Pal | Ntsc
		}

		[Flags]
		private enum PSidSidModel
		{
			Unknown = 0,
			Model1_6581 = 1 << 4,
			Model1_8580 = 1 << 5,
			Model1_Any = Model1_6581 | Model1_8580,
			Model2_6581 = 1 << 6,
			Model2_8580 = 1 << 7,
			Model2_Any = Model2_6581 | Model2_8580
		}

		private const uint PSID_ID = 0x50534944;
		private const uint RSID_ID = 0x52534944;

		/// <summary>
		/// Header has been extended for 'RSID' format
		/// The following changes are present:
		///     Id = 'RSID'
		///     Version = 2 only
		///     Play, Load and Speed reserved 0
		///     PSidSpecific flag reserved 0
		///     Init cannot be under ROMS/IO
		///     Load cannot be less than 0x0801 (start of basic)
		/// </summary>
		private static class PSidHeader
		{
			public const int Id = 0;				// 'PSID' (ASCII)
			public const int Version = 4;			// 0x0001, 0x0002 or 0x0003
			public const int Data = 6;				// 16-bit offset to binary data in file
			public const int Load = 8;				// 16-bit C64 address to load file to
			public const int Init = 10;				// 16-bit C64 address of init subroutine
			public const int Play = 12;				// 16-bit C64 address of play subroutine
			public const int Songs = 14;			// Number of songs
			public const int Start = 16;			// Start song out of [1..256]
			public const int Speed = 18;			// 32-bit speed info
													// bit: 0=50 Hz, 1=CIA 1 Timer A (default: 60 Hz)
			public const int Name = 22;				// ASCII strings, 31 characters long and
			public const int Author = 54;			// terminated by a trailing zero
			public const int Released = 86;
			public const int Flags = 118;			// Only version >= 0x0002
			public const int RelocStartPage = 120;	// Only version >= 0x0002B
			public const int RelocPages = 121;		// Only version >= 0x0002B
			public const int SidChipBase2 = 122;	// Only version >= 0x0003
			public const int Reserved = 123;		// Only version >= 0x0002

			public const int SizeOf = 124;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will test the file to see if it's a PSID file
		/// </summary>
		/********************************************************************/
		private bool TestPSid(ModuleStream moduleStream)
		{
			// We need at least as long as version 2 plus C64 load address data
			if (moduleStream.Length < PSidHeader.SizeOf + 2)
				return false;

			// Read the ID and version
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint id = moduleStream.Read_B_UINT32();
			ushort version = moduleStream.Read_B_UINT16();

			// Check it
			if ((id == PSID_ID) && (version >= 1) && (version <= 3))
			{
				sidType = SidType.PSid;
				return true;
			}

			if ((id == RSID_ID) && (version == 2))
			{
				sidType = SidType.RSid;
				return true;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Will load the file as a PSID file
		/// </summary>
		/********************************************************************/
		private void LoadPSid(byte[] dataBuf, out string errorMessage)
		{
			errorMessage = string.Empty;

			Encoding encoder = EncoderCollection.Amiga;

			Clock clock = Clock.Unknown;
			Compatibility compatibility = Compatibility.C64;

			ushort version = Endian.EndianBig16(dataBuf, PSidHeader.Version);

			if (sidType == SidType.PSid)
			{
				info.FormatString = Resources.IDS_SID_FORMAT_PSID;

				if (version == 1)
					compatibility = Compatibility.PSid;
			}
			else if (sidType == SidType.RSid)
			{
				info.FormatString = Resources.IDS_SID_FORMAT_RSID;
				compatibility = Compatibility.R64;
			}
			else
				throw new NotImplementedException();

			fileOffset = Endian.EndianBig16(dataBuf, PSidHeader.Data);

			info.LoadAddr = Endian.EndianBig16(dataBuf, PSidHeader.Load);
			info.InitAddr = Endian.EndianBig16(dataBuf, PSidHeader.Init);
			info.PlayAddr = Endian.EndianBig16(dataBuf, PSidHeader.Play);

			info.Songs = Endian.EndianBig16(dataBuf, PSidHeader.Songs);
			info.StartSong = Endian.EndianBig16(dataBuf, PSidHeader.Start);

			info.SidChipBase1 = 0xd400;
			info.SidChipBase2 = 0;
			info.Compatibility = compatibility;

			uint speed = Endian.EndianBig32(dataBuf, PSidHeader.Speed);

			if (info.Songs > MaxSongs)
				info.Songs = MaxSongs;

			info.MusPlayer = false;
			info.SidModel1 = SidModel.Unknown;
			info.SidModel2 = SidModel.Unknown;
			info.RelocPages = 0;
			info.RelocStartPage = 0;

			if (version >= 2)
			{
				PSidFlags flags = (PSidFlags)Endian.EndianBig16(dataBuf, PSidHeader.Flags);

				if ((flags & PSidFlags.Mus) != 0)
				{
					// MUS tunes run at any speed
					clock = Clock.Any;
					info.MusPlayer = true;
				}

				// These flags is only available for the appropriate
				// file formats
				switch (compatibility)
				{
					case Compatibility.C64:
					{
						if ((flags & PSidFlags.Specific) != 0)
							info.Compatibility = Compatibility.PSid;

						break;
					}

					case Compatibility.R64:
					{
						if ((flags & PSidFlags.Basic) != 0)
							info.Compatibility = Compatibility.Basic;

						break;
					}
				}

				if ((PSidClock)(flags & PSidFlags.Clock) == PSidClock.Pal)
					clock |= Clock.Pal;

				if ((PSidClock)(flags & PSidFlags.Clock) == PSidClock.Ntsc)
					clock |= Clock.Ntsc;

				info.ClockSpeed = clock;

				info.SidModel1 = SidModel.Unknown;

				if ((PSidSidModel)(flags & PSidFlags.SidModel) == PSidSidModel.Model1_6581)
					info.SidModel1 |= SidModel._6581;

				if ((PSidSidModel)(flags & PSidFlags.SidModel) == PSidSidModel.Model1_8580)
					info.SidModel1 |= SidModel._8580;

				info.SidModel2 = SidModel.Unknown;

				info.RelocStartPage = dataBuf[PSidHeader.RelocStartPage];
				info.RelocPages = dataBuf[PSidHeader.RelocPages];

				if (version >= 3)
				{
					info.SidChipBase2 = (ushort)(0xd000 | (dataBuf[PSidHeader.SidChipBase2] << 4));

					if ((PSidSidModel)(flags & PSidFlags.SidModel) == PSidSidModel.Model2_6581)
						info.SidModel2 |= SidModel._6581;

					if ((PSidSidModel)(flags & PSidFlags.SidModel) == PSidSidModel.Model2_8580)
						info.SidModel2 |= SidModel._8580;
				}
			}

			// Check reserved fields to force real C64 compliance
			// as required by the RSID specification
			if (compatibility == Compatibility.R64)
			{
				if ((info.LoadAddr != 0) || (info.PlayAddr != 0) || (speed != 0))
				{
					errorMessage = Resources.IDS_SID_ERR_LOADING_HEADER;
					return;
				}

				// Real C64 tunes appear as CIA
				speed = uint.MaxValue;
			}

			// Create the speed/clock setting table
			ConvertOldStyleSpeedToTables(speed, clock);

			// Copy the info strings, so they will not get lost
			info.Title = encoder.GetString(dataBuf, PSidHeader.Name, 32);
			info.Author = encoder.GetString(dataBuf, PSidHeader.Author, 32);
			info.Released = encoder.GetString(dataBuf, PSidHeader.Released, 32);

			if (info.MusPlayer)//XX
				throw new NotImplementedException();
		}
		#endregion
	}
}
