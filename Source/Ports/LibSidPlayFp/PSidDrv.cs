/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64.Cpu;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.SidPlayFp;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp
{
	/// <summary>
	/// The code here is use to support the PSID version 2NG
	/// (proposal B) file format for player relocation support
	/// </summary>
	internal class PSidDrv
	{
		#region PSID driver
		private static readonly byte[] psid_driver =
		{
			0x01, 0x00, 0x6f, 0x36, 0x35, 0x00, 0x00, 0x00,
			0x00, 0x10, 0xdf, 0x00, 0x00, 0x04, 0x00, 0x00,
			0x00, 0x40, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x1d, 0x10, 0xd5, 0x10, 0xde,
			0x10, 0xde, 0x10, 0x9c, 0x10, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x6c, 0x0e, 0x10, 0x6c, 0x0c, 0x10,
			0x78, 0xa9, 0x00, 0x8d, 0x1a, 0xd0, 0xad, 0x19,
			0xd0, 0x8d, 0x19, 0xd0, 0xa9, 0x7f, 0x8d, 0x0d,
			0xdc, 0x8d, 0x0d, 0xdd, 0xad, 0x0d, 0xdc, 0xad,
			0x0d, 0xdd, 0xa9, 0x0f, 0x8d, 0x18, 0xd4, 0xad,
			0x14, 0x10, 0xf0, 0x07, 0xa2, 0x25, 0xa0, 0x40,
			0x4c, 0x4c, 0x10, 0xa2, 0x95, 0xa0, 0x42, 0x8e,
			0x04, 0xdc, 0x8c, 0x05, 0xdc, 0xa2, 0x9b, 0xa0,
			0x37, 0x4d, 0x15, 0x10, 0x0d, 0x12, 0x10, 0xf0,
			0x04, 0xa2, 0x1b, 0xa0, 0x00, 0x8e, 0x11, 0xd0,
			0x8c, 0x12, 0xd0, 0xad, 0x12, 0x10, 0xf0, 0x0a,
			0xad, 0x13, 0x10, 0xf0, 0x05, 0xa2, 0xc2, 0x8e,
			0x14, 0x03, 0xae, 0x10, 0x10, 0xac, 0x11, 0x10,
			0xe8, 0xc8, 0xca, 0xd0, 0xfd, 0x88, 0xd0, 0xfa,
			0xad, 0x0b, 0x10, 0xd0, 0x08, 0xa9, 0x81, 0x8d,
			0x1a, 0xd0, 0x4c, 0x9c, 0x10, 0xa9, 0x81, 0xa2,
			0x01, 0x8d, 0x0d, 0xdc, 0x8e, 0x0e, 0xdc, 0xad,
			0x12, 0x10, 0xd0, 0x02, 0xa9, 0x37, 0x85, 0x01,
			0xad, 0x16, 0x10, 0x48, 0xad, 0x0a, 0x10, 0x28,
			0x20, 0x1a, 0x10, 0xad, 0x12, 0x10, 0xf0, 0x0a,
			0xad, 0x13, 0x10, 0xf0, 0x04, 0xa9, 0x37, 0x85,
			0x01, 0x58, 0x4c, 0xbf, 0x10, 0xa5, 0x01, 0x48,
			0xad, 0x13, 0x10, 0x85, 0x01, 0xa9, 0x00, 0x20,
			0x17, 0x10, 0x68, 0x85, 0x01, 0xce, 0x19, 0xd0,
			0xad, 0x0d, 0xdc, 0x68, 0xa8, 0x68, 0xaa, 0x68,
			0x40, 0x02, 0x00, 0x00, 0x01, 0x82, 0x02, 0x82,
			0x02, 0x82, 0x02, 0x82, 0x02, 0x82, 0x10, 0x82,
			0x03, 0x82, 0x22, 0x82, 0x09, 0x82, 0x11, 0x82,
			0x03, 0x82, 0x0f, 0x82, 0x05, 0x82, 0x05, 0x22,
			0x05, 0x82, 0x03, 0x82, 0x0b, 0x82, 0x0a, 0x82,
			0x0d, 0x82, 0x09, 0x82, 0x04, 0x82, 0x04, 0x82,
			0x03, 0x82, 0x05, 0x82, 0x0a, 0x82, 0x06, 0x82,
			0x07, 0x82, 0x00, 0x00, 0x00, 0x00
		};
		#endregion

		#region Poweron
		private static readonly byte[] powerOn =
		{
			/* addr,   off,  rle, values */
			/*$0003*/ 0x83, 0x04, 0xaa, 0xb1, 0x91, 0xb3, 0x22,
			/*$000b*/ 0x03,       0x4c,
			/*$000f*/ 0x03,       0x04,
			/*$0016*/ 0x86, 0x05, 0x19, 0x16, 0x00, 0x0a, 0x76, 0xa3,
			/*$0022*/ 0x86, 0x03, 0x40, 0xa3, 0xb3, 0xbd,
			/*$002b*/ 0x85, 0x01, 0x01, 0x08,
			/*$0034*/ 0x07,       0xa0,
			/*$0038*/ 0x03,       0xa0,
			/*$003a*/ 0x01,       0xff,
			/*$0042*/ 0x07,       0x08,
			/*$0047*/ 0x04,       0x24,
			/*$0053*/ 0x8b, 0x01, 0x03, 0x4c,
			/*$0061*/ 0x0c,       0x8d,
			/*$0063*/ 0x02,       0x10,
			/*$0069*/ 0x84, 0x02, 0x8c, 0xff, 0xa0,
			/*$0071*/ 0x85, 0x1e, 0x0a, 0xa3, 0xe6, 0x7a, 0xd0, 0x02, 0xe6, 0x7b, 0xad, 0x00, 0x08, 0xc9, 0x3a, 0xb0, 0x0a, 0xc9, 0x20, 0xf0, 0xef, 0x38, 0xe9, 0x30, 0x38, 0xe9, 0xd0, 0x60, 0x80, 0x4f, 0xc7, 0x52, 0x58,
			/*$0091*/ 0x01,       0xff,
			/*$009a*/ 0x08,       0x03,
			/*$00b2*/ 0x97, 0x01, 0x3c, 0x03,
			/*$00c2*/ 0x8e, 0x03, 0xa0, 0x30, 0xfd, 0x01,
			/*$00c8*/ 0x82, 0x82, 0x03,
			/*$00cb*/ 0x80, 0x81, 0x01,
			/*$00ce*/ 0x01,       0x20,
			/*$00d1*/ 0x82, 0x01, 0x18, 0x05,
			/*$00d5*/ 0x82, 0x02, 0x27, 0x07, 0x0d,
			/*$00d9*/ 0x81, 0x86, 0x84,
			/*$00e0*/ 0x80, 0x85, 0x85,
			/*$00e6*/ 0x80, 0x86, 0x86,
			/*$00ed*/ 0x80, 0x85, 0x87,
			/*$00f3*/ 0x80, 0x03, 0x18, 0xd9, 0x81, 0xeb,
			/*$0176*/ 0x7f,       0x00,
			/*$01f6*/ 0x7f,       0x00,
			/*$0276*/ 0x7f,       0x00,
			/*$0282*/ 0x8b, 0x0a, 0x08, 0x00, 0xa0, 0x00, 0x0e, 0x00, 0x04, 0x0a, 0x00, 0x04, 0x10,
			/*$028f*/ 0x82, 0x01, 0x48, 0xeb,
			/*$0300*/ 0xef, 0x0b, 0x8b, 0xe3, 0x83, 0xa4, 0x7c, 0xa5, 0x1a, 0xa7, 0xe4, 0xa7, 0x86, 0xae,
			/*$0310*/ 0x84, 0x02, 0x4c, 0x48, 0xb2,
			/*$0314*/ 0x81, 0x1f, 0x31, 0xea, 0x66, 0xfe, 0x47, 0xfe, 0x4a, 0xf3, 0x91, 0xf2, 0x0e, 0xf2, 0x50, 0xf2, 0x33, 0xf3, 0x57, 0xf1, 0xca, 0xf1, 0xed, 0xf6, 0x3e, 0xf1, 0x2f, 0xf3, 0x66, 0xfe, 0xa5, 0xf4, 0xed, 0xf5

			/*Total 217*/
		};
		#endregion

		private readonly SidTuneInfo tuneInfo;
		private string errorString;

		private uint8_t[] reloc_driver;
		private int reloc_size;

		private uint_least16_t driverAddr;
		private uint_least16_t driverLength;

		private uint_least16_t powerOnDelay;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PSidDrv(SidTuneInfo tuneInfo)
		{
			this.tuneInfo = tuneInfo;
			powerOnDelay = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Set the power on delay cycles
		/// </summary>
		/********************************************************************/
		public void PowerOnDelay(uint_least16_t delay)
		{
			powerOnDelay = delay;
		}



		/********************************************************************/
		/// <summary>
		/// Relocate the driver
		/// </summary>
		/********************************************************************/
		public bool DrvReloc()
		{
			int startLp = tuneInfo.LoadAddr() >> 8;
			int endLp = (int)((tuneInfo.LoadAddr() + (tuneInfo.C64DataLen() - 1)) >> 8);

			uint_least8_t relocStartPage = tuneInfo.RelocStartPage();
			uint_least8_t relocPages = tuneInfo.RelocPages();

			if (tuneInfo.Compatibility() == SidTuneInfo.compatibility_t.COMPATIBILITY_BASIC)
			{
				// The psiddrv is only used for initialization and to
				// autorun basic tunes as running the kernel falls
				// into a manual load/run mode
				relocStartPage = 0x04;
				relocPages = 0x03;
			}

			// Check for free space in tune
			if (relocStartPage == 0xff)
				relocPages = 0;
			else if (relocStartPage == 0)	// Check if we need to find the reloc addr
			{
				relocPages = 0;

				// Find area where to dump the driver in.
				// It's only 1 block long, so any free block we can find
				// between $0400 and $d000 will do
				for (int i = 4; i < 0xd0; i++)
				{
					if ((i >= startLp) && (i <= endLp))
						continue;

					if ((i >= 0xa0) && (i <= 0xbf))
						continue;

					relocStartPage = (uint8_t)i;
					relocPages = 1;
					break;
				}
			}

			if (relocPages < 1)
			{
				errorString = Resources.IDS_SID_ERR_NO_SPACE;
				return false;
			}

			// Place PSID driver into ram
			uint_least16_t relocAddr = (uint_least16_t)(relocStartPage << 8);

			reloc_driver = psid_driver;
			reloc_size = psid_driver.Length;

			Reloc65 relocator = new Reloc65(relocAddr - 10);
			if (!relocator.Reloc(ref reloc_driver, ref reloc_size))
			{
				errorString = Resources.IDS_SID_ERR_RELOC;
				return false;
			}

			// Adjust size to not include initialization data
			reloc_size -= 10;

			driverAddr = relocAddr;
			driverLength = (ushort)reloc_size;

			// Round length to end of page
			driverLength += 0xff;
			driverLength &= 0xff00;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Install the driver
		/// Must be called after the tune has been placed in memory
		/// </summary>
		/********************************************************************/
		public void Install(ISidMemory mem, uint8_t video)
		{
			mem.FillRam(0, 0, 0x3ff);

			if (tuneInfo.Compatibility() >= SidTuneInfo.compatibility_t.COMPATIBILITY_R64)
				CopyPoweronPattern(mem);

			// Set PAL/NTSC switch
			mem.WriteMemByte(0x02a6, video);

			mem.InstallResetHook(SidEndian.Endian_Little16(reloc_driver, 0));

			// If not a basic tune then the psiddrv must install
			// interrupt hooks and trap programs trying to restart basic
			if (tuneInfo.Compatibility() == SidTuneInfo.compatibility_t.COMPATIBILITY_BASIC)
			{
				// Install hook to set subtune number for basic
				mem.SetBasicSubTune((uint8_t)(tuneInfo.CurrentSong() - 1));
				mem.InstallBasicTrap(0xbf53);
			}
			else
			{
				// Only install IRQ handle for RSID tunes
				mem.FillRam(0x0314, reloc_driver, 2, tuneInfo.Compatibility() == SidTuneInfo.compatibility_t.COMPATIBILITY_R64 ? (uint)2 : 6);

				// Experimental restart basic trap
				uint_least16_t addr = SidEndian.Endian_Little16(reloc_driver, 8);
				mem.InstallBasicTrap(0xffe1);
				mem.WriteMemWord(0x0328, addr);
			}

			uint_least16_t pos = driverAddr;

			// Install driver to ram
			mem.FillRam(pos, reloc_driver, 10, (uint)reloc_size);

			// Set song number
			mem.WriteMemByte(pos, (uint8_t)(tuneInfo.CurrentSong() - 1));
			pos++;

			// Set tunes speed (VIC/CIA)
			mem.WriteMemByte(pos, (uint8_t)(tuneInfo.SongSpeed() == SidTuneInfo.SPEED_VBI ? 0 : 1));
			pos++;

			// Set init address
			mem.WriteMemWord(pos, tuneInfo.Compatibility() == SidTuneInfo.compatibility_t.COMPATIBILITY_BASIC ? (uint_least16_t)0xbf55 : tuneInfo.InitAddr());
			pos += 2;

			// Set play address
			mem.WriteMemWord(pos, tuneInfo.PlayAddr());
			pos += 2;

			mem.WriteMemWord(pos, powerOnDelay);
			pos += 2;

			// Set init address IO bank value
			mem.WriteMemByte(pos, IoMap(tuneInfo.InitAddr()));
			pos++;

			// Set play address IO bank value
			mem.WriteMemByte(pos, IoMap(tuneInfo.PlayAddr()));
			pos++;

			// Set PAL/NTSC flag
			mem.WriteMemByte(pos, video);
			pos++;

			// Set the required tune clock speed
			uint8_t clockSpeed;

			switch (tuneInfo.ClockSpeed())
			{
				case SidTuneInfo.clock_t.CLOCK_PAL:
				{
					clockSpeed = 1;
					break;
				}

				case SidTuneInfo.clock_t.CLOCK_NTSC:
				{
					clockSpeed = 0;
					break;
				}

				// UNKNOWN or ANY
				default:
				{
					clockSpeed = video;
					break;
				}
			}

			mem.WriteMemByte(pos, clockSpeed);
			pos++;

			// Set default processor register flags on calling init
			mem.WriteMemByte(pos, (uint8_t)(tuneInfo.Compatibility() >= SidTuneInfo.compatibility_t.COMPATIBILITY_R64 ? 0 : 1 << Mos6510.SR_INTERRUPT));
		}



		/********************************************************************/
		/// <summary>
		/// Get a detailed error message
		/// </summary>
		/********************************************************************/
		public string ErrorString()
		{
			return errorString;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint_least16_t DriverAddr()
		{
			return driverAddr;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint_least16_t DriverLength()
		{
			return driverLength;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Copy in power on settings. These were created by running the
		/// kernel reset routine and storing the useful values from
		/// $0000-$03ff. Format is:
		/// - offset byte (bit 7 indicates presence rle byte)
		/// - rle count byte (bit 7 indicates compression used)
		/// - data (single byte) or quantity represented by uncompressed count
		/// all counts and offsets are 1 less than they should be
		/// </summary>
		/********************************************************************/
		private void CopyPoweronPattern(ISidMemory mem)
		{
			uint_least16_t addr = 0;

			for (uint i = 0; i < powerOn.Length;)
			{
				uint8_t off = powerOn[i++];
				uint8_t count = 0;
				bool compressed = false;

				// Determine data count/compression
				if ((off & 0x80) != 0)
				{
					// Fixup offset
					off &= 0x7f;
					count = powerOn[i++];

					if ((count & 0x80) != 0)
					{
						// Fixup count
						count &= 0x7f;
						compressed = true;
					}
				}

				// Fix count off by ones (see format details)
				count++;
				addr += off;

				if (compressed)
				{
					// Extract compressed data
					byte data = powerOn[i++];

					while (count-- > 0)
						mem.WriteMemByte(addr++, data);
				}
				else
				{
					// Extract uncompressed data
					while (count-- > 0)
						mem.WriteMemByte(addr++, powerOn[i++]);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Get required I/O map to reach address
		/// </summary>
		/********************************************************************/
		private uint8_t IoMap(uint_least16_t addr)
		{
			// Force real C64 compatibility
			if ((tuneInfo.Compatibility() == SidTuneInfo.compatibility_t.COMPATIBILITY_R64) || (tuneInfo.Compatibility() == SidTuneInfo.compatibility_t.COMPATIBILITY_BASIC) || (addr == 0))
			{
				// Special case, set to 0x37 by the psid driver
				return 0;
			}

			// $34 for init/play in $d000 - $dfff
			// $35 for init/play in $e000 - $ffff
			// $36 for load end/play in $a000 - $ffff
			// $37 for the rest
			if (addr < 0xa000)
				return 0x37;	// Basic-ROM, Kernal-ROM, I/O

			if (addr < 0xd000)
				return 0x36;	// Kernal-ROM, I/O

			if (addr >= 0xe000)
				return 0x35;	// I/O only

			return 0x34;		// RAM only
		}
		#endregion
	}
}
