/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2
{
	/// <summary>
	/// PSID driver installation
	///
	/// The code here is use to support the PSID Version 2NG
	/// (proposal B) file format for player relocation support
	/// </summary>
	internal partial class Player
	{
		private const byte MaxPage = 0xff;

		#region PSID driver
		private static readonly byte[] psidDriver =
		{
			0x01, 0x00, 0x6f, 0x36, 0x35, 0x00, 0x00, 0x00,
			0x00, 0x10, 0xc0, 0x00, 0x00, 0x04, 0x00, 0x00,
			0x00, 0x40, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x1d, 0x10, 0xbc, 0x10, 0xbf,
			0x10, 0xbf, 0x10, 0x8e, 0x10, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x6c, 0x0e, 0x10, 0x6c, 0x0c, 0x10,
			0x78, 0xac, 0xa6, 0x02, 0xad, 0x14, 0x10, 0x8d,
			0xa6, 0x02, 0x48, 0x20, 0x84, 0xff, 0x68, 0x8c,
			0xa6, 0x02, 0xa2, 0x9b, 0xa0, 0x37, 0x4d, 0x15,
			0x10, 0x0d, 0x12, 0x10, 0xf0, 0x04, 0xa2, 0x1b,
			0xa0, 0x00, 0x8e, 0x11, 0xd0, 0x8c, 0x12, 0xd0,
			0xad, 0x12, 0x10, 0xf0, 0x0a, 0xad, 0x13, 0x10,
			0xf0, 0x05, 0xa2, 0xa0, 0x8e, 0x14, 0x03, 0xae,
			0x10, 0x10, 0xac, 0x11, 0x10, 0xe8, 0xc8, 0xca,
			0xd0, 0xfd, 0x88, 0xd0, 0xfa, 0xad, 0x0b, 0x10,
			0xd0, 0x10, 0xa9, 0x7f, 0x8d, 0x0d, 0xdc, 0xad,
			0x19, 0xd0, 0x8d, 0x19, 0xd0, 0xa9, 0x81, 0x8d,
			0x1a, 0xd0, 0xad, 0x0d, 0xdc, 0xad, 0x12, 0x10,
			0xd0, 0x02, 0xa9, 0x37, 0x85, 0x01, 0xad, 0x16,
			0x10, 0x48, 0xad, 0x0a, 0x10, 0x28, 0x20, 0x1a,
			0x10, 0xad, 0x12, 0x10, 0xf0, 0x0a, 0xad, 0x13,
			0x10, 0xf0, 0x04, 0xa9, 0x37, 0x85, 0x01, 0x58,
			0x4c, 0x9d, 0x10, 0xa5, 0x01, 0x48, 0xad, 0x13,
			0x10, 0x85, 0x01, 0xa9, 0x00, 0x20, 0x17, 0x10,
			0x68, 0x85, 0x01, 0xce, 0x19, 0xd0, 0xad, 0x0d,
			0xdc, 0x68, 0xa8, 0x68, 0xaa, 0x68, 0x40, 0x4c,
			0x31, 0xea, 0x02, 0x00, 0x00, 0x01, 0x82, 0x02,
			0x82, 0x02, 0x82, 0x02, 0x82, 0x02, 0x82, 0x10,
			0x82, 0x03, 0x82, 0x07, 0x82, 0x12, 0x82, 0x03,
			0x82, 0x0f, 0x82, 0x05, 0x82, 0x05, 0x22, 0x05,
			0x82, 0x03, 0x82, 0x0b, 0x82, 0x18, 0x82, 0x09,
			0x82, 0x04, 0x82, 0x04, 0x82, 0x03, 0x82, 0x05,
			0x82, 0x0a, 0x82, 0x06, 0x82, 0x07, 0x82, 0x00,
			0x00, 0x00, 0x00
		};
		#endregion

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PSidDrvReloc(SidTuneInfo tuneInfo, Sid2Info info)
		{
			int startLp = tuneInfo.LoadAddr >> 8;
			int endLp = (int)((tuneInfo.LoadAddr + (tuneInfo.C64DataLen - 1)) >> 8);

			if (info.Environment != Sid2Env.EnvR)
			{
				// SidPlay1 modes require no PSid driver
				info.DriverAddr = 0;
				info.DriverLength = 0;
				info.PowerOnDelay = 0;

				return;
			}

			if (tuneInfo.Compatibility == Compatibility.Basic)
			{
				// The psiddrv is only used for initialisation and to
				// autorun basic tunes as running the kernel falls
				// into a manual load/run mode
				tuneInfo.RelocStartPage = 0x04;
				tuneInfo.RelocPages = 0x03;
			}

			// Check for free space in tune
			if (tuneInfo.RelocStartPage == MaxPage)
				tuneInfo.RelocPages = 0;
			else if (tuneInfo.RelocStartPage == 0)	// Check if we need to find the reloc addr
			{
				// Tune is clean so find some free ram around the
				// load image
				PSidRelocAddr(tuneInfo, startLp, endLp);
			}
			else
			{
				// Check reloc information mode
				int startRp = tuneInfo.RelocStartPage;
				int endRp = startRp + (tuneInfo.RelocPages - 1);

				// New relocation implementation (exclude region)
				// to complement existing method rejected as being
				// unnecessary. From tests in most cases this
				// method increases memory availability
				/*************************************************
				if ((startrp <= startlp) && (endrp >= endlp))
				{   // Is describing used space so find some free
				    // ram outside this range
				    psidRelocAddr (tuneInfo, startrp, endrp);
				}
				*************************************************/
			}

			if (tuneInfo.RelocPages < 1)
				throw new Exception(Resources.IDS_SID_ERR_NO_SPACE);

			ushort relocAddr = (ushort)(tuneInfo.RelocStartPage << 8);

			{
				// Place PSID driver into ram
				byte[] relocDriver = psidDriver;
				int relocSize = psidDriver.Length;

				if (!Reloc65(ref relocDriver, ref relocSize, relocAddr - 10))
					throw new Exception(Resources.IDS_SID_ERR_RELOC);

				// Adjust size to not include initialization data
				relocSize -= 10;
				info.DriverAddr = relocAddr;
				info.DriverLength = (ushort)relocSize;

				// Round length to end of page
				info.DriverLength += 0xff;
				info.DriverLength &= 0xff00;

				// RESET
				Array.Copy(relocDriver, 0, rom, 0xfffc, 2);

				// If not a basic tune then the psiddrv must install
				// interrupt hooks and trap programs trying to restart basic
				if (tuneInfo.Compatibility == Compatibility.Basic)
				{
					// Install hook to set sub-tune number for basic
					byte[] prg =
					{
						Opcodes.LDAb, (byte)(tuneInfo.CurrentSong - 1),
						Opcodes.STAa, 0x0c, 0x03, Opcodes.JSRw, 0x2c, 0xa8,
						Opcodes.JMPw, 0xb1, 0xa7
					};

					Array.Copy(prg, 0, rom, 0xbf53, prg.Length);
					rom[0xa7ae] = Opcodes.JMPw;
					Endian.EndianLittle16(rom, 0xa7af, 0xbf53);
				}
				else
				{
					// Only install irq handle for RSID tunes
					if (tuneInfo.Compatibility == Compatibility.R64)
						Array.Copy(relocDriver, 2, ram, 0x0314, 2);
					else
						Array.Copy(relocDriver, 2, ram, 0x0314, 6);

					// Experimental restart basic trap
					ushort addr = Endian.EndianLittle16(relocDriver, 8);
					rom[0xa7ae] = Opcodes.JMPw;
					Endian.EndianLittle16(rom, 0xa7af, 0xffe1);
					Endian.EndianLittle16(rom, 0x0328, addr);
				}

				// Install driver to rom so it can be copied later into
				// ram once the tune is installed
				Array.Copy(relocDriver, 10, rom, 0, relocSize);
			}

			{
				// Setup the initial entry point
				int addr = 0;

				// Tell C64 about song
				rom[addr++] = (byte)(tuneInfo.CurrentSong - 1);

				if (tuneInfo.SongSpeed == Speed.Vbi)
					rom[addr] = 0;
				else
					rom[addr] = 1;

				addr++;

				Endian.EndianLittle16(rom, addr, tuneInfo.Compatibility == Compatibility.Basic ? (ushort)0xbf55 : tuneInfo.InitAddr);
				addr += 2;

				Endian.EndianLittle16(rom, addr, tuneInfo.PlayAddr);
				addr += 2;

				// Initialize random number generator
				info.PowerOnDelay = config.PowerOnDelay;

				// Delays above MAX result in random delays
				if (info.PowerOnDelay > MaxPowerOnDelay)
				{
					// Limit the delay to something sensible
					info.PowerOnDelay = (ushort)((rand >> 3) & MaxPowerOnDelay);
				}

				Endian.EndianLittle16(rom, addr, info.PowerOnDelay);
				addr += 2;

				rand = rand * 13 + 1;

				rom[addr++] = IoMap(tuneInfo.InitAddr);
				rom[addr++] = IoMap(tuneInfo.PlayAddr);
				rom[addr + 1] = rom[addr] = ram[0x02a6];	// PAL/NTSC flag
				addr++;

				// Add the required tune speed
				switch (tune.GetInfo().ClockSpeed)
				{
					case Clock.Pal:
					{
						rom[addr++] = 1;
						break;
					}

					case Clock.Ntsc:
					{
						rom[addr++] = 0;
						break;
					}

					default:	// Unknown or any
					{
						addr++;
						break;
					}
				}

				// Default processor register flags on calling init
				if (tuneInfo.Compatibility >= Compatibility.R64)
					rom[addr++] = 0;
				else
					rom[addr++] = (byte)Mos6510.Mos6510.StatusFlag.Interrupt;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PSidRelocAddr(SidTuneInfo tuneInfo, int startP, int endP)
		{
			// Used memory ranges
			bool[] pages = new bool[256];
			int[] used =
			{
				0x00, 0x03,
				0xa0, 0xbf,
				0xd0, 0xff,
				startP, (startP <= endP) && (endP <= 0xff) ? endP : 0xff
			};

			// Mark used pages in table
			for (int i = 0; i < used.Length; i += 2)
			{
				for (int page = used[i]; page <= used[i + 1]; page++)
					pages[page] = true;
			}

			{
				// Find largest free range
				int lastPage = 0;
				tuneInfo.RelocPages = 0;

				for (int page = 0; page < pages.Length; page++)
				{
					if (!pages[page])
						continue;

					int relocPages = page - lastPage;
					if (relocPages > tuneInfo.RelocPages)
					{
						tuneInfo.RelocStartPage = (byte)lastPage;
						tuneInfo.RelocPages = (byte)relocPages;
					}

					lastPage = page + 1;
				}
			}

			if (tuneInfo.RelocPages == 0)
				tuneInfo.RelocStartPage = MaxPage;
		}



		/********************************************************************/
		/// <summary>
		/// The driver is relocated above and here is actually installed
		/// into ram. The two operations are now split to allow the driver
		/// to be installed inside the load image
		/// </summary>
		/********************************************************************/
		private void PSidDrvInstall(Sid2Info info)
		{
			Array.Copy(rom, 0, ram, info.DriverAddr, info.DriverLength);
		}

		#region O65 handling

		private const int Buf = 9 * 2 + 8;	// 16 bit header

		#region File65 class
		private class File65
		{
			public int FSize;
			public byte[] Buf;
			public int TBase;
			public int TLen;
			public int DBase;
			public int DLen;
			public int BBase;
			public int BLen;
			public int ZBase;
			public int ZLen;
			public int TDiff;
			public int DDiff;
			public int BDiff;
			public int ZDiff;
			public int SegT;
			public int SegD;
			public int UTab;
			public int RtTab;
			public int RdTab;
			public int ExTab;
		}
		#endregion

		private static readonly byte[] cmp = { 1, 0, (byte)'o', (byte)'6', (byte)'5' };

		/********************************************************************/
		/// <summary>
		/// Relocates 'o65' files
		/// </summary>
		/********************************************************************/
		private bool Reloc65(ref byte[] buf, ref int fSize, int addr)
		{
			int tFlag = 0, dFlag = 0, bFlag = 0, zFlag = 0;
			int tBase = 0, dBase = 0, bBase = 0, zBase = 0;
			int extract = 0;

			File65 file = new File65();
			file.Buf = buf;
			file.FSize = fSize;

			tFlag = 1;
			tBase = addr;
			extract = 1;

			if (!file.Buf.AsSpan(0, cmp.Length).SequenceEqual(cmp))
				return false;

			int mode = file.Buf[7] * 256 + file.Buf[6];

			if ((mode & 0x2000) != 0)
				return false;

			if ((mode & 0x4000) != 0)
				return false;

			int hLen = Buf + ReadOptions(file.Buf, Buf);

			file.TBase = file.Buf[9] * 256 + file.Buf[8];
			file.TLen = file.Buf[11] * 256 + file.Buf[10];
			file.TDiff = tFlag != 0 ? tBase - file.TBase : 0;
			file.DBase = file.Buf[13] * 256 + file.Buf[12];
			file.DLen = file.Buf[15] * 256 + file.Buf[14];
			file.DDiff = dFlag != 0 ? dBase - file.DBase : 0;
			file.BBase = file.Buf[17] * 256 + file.Buf[16];
			file.BLen = file.Buf[19] * 256 + file.Buf[18];
			file.BDiff = bFlag != 0 ? bBase - file.BBase : 0;
			file.ZBase = file.Buf[21] * 256 + file.Buf[20];
			file.ZLen = file.Buf[23] * 256 + file.Buf[22];
			file.ZDiff = zFlag != 0 ? zBase - file.ZBase : 0;

			file.SegT = hLen;
			file.SegD = file.SegT + file.TLen;
			file.UTab = file.SegD + file.DLen;

			file.RtTab = file.UTab + ReadUndef(file.Buf, file.UTab);

			file.RdTab = RelocSeg(file.Buf, file.SegT, file.TLen, file.RtTab, file);
			file.ExTab = RelocSeg(file.Buf, file.SegD, file.DLen, file.RdTab, file);

			RelocGlobals(file.Buf, file.ExTab, file);

			if (tFlag != 0)
			{
				file.Buf[9] = (byte)((tBase >> 8) & 255);
				file.Buf[8] = (byte)(tBase & 255);
			}

			if (dFlag != 0)
			{
				file.Buf[13] = (byte)((dBase >> 8) & 255);
				file.Buf[12] = (byte)(dBase & 255);
			}

			if (bFlag != 0)
			{
				file.Buf[17] = (byte)((bBase >> 8) & 255);
				file.Buf[16] = (byte)(bBase & 255);
			}

			if (zFlag != 0)
			{
				file.Buf[21] = (byte)((zBase >> 8) & 255);
				file.Buf[20] = (byte)(zBase & 255);
			}

			switch (extract)
			{
				// Whole file
				case 0:
					return true;

				// Text segment
				case 1:
				{
					buf = file.Buf.AsSpan(file.SegT).ToArray();
					fSize = file.TLen;
					return true;
				}

				case 2:
				{
					buf = file.Buf.AsSpan(file.SegD).ToArray();
					fSize = file.DLen;
					return true;
				}

				default:
					return false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int ReadOptions(byte[] buf, int offset)
		{
			int l = 0;

			int c = buf[offset];
			while ((c != 0) && (c != -1/*EOF*/))
			{
				c &= 255;
				l += c;
				c = buf[offset + l];
			}

			return ++l;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int ReadUndef(byte[] buf, int offset)
		{
			int l = 2;

			int n = buf[offset] + 256 * buf[offset + 1];
			while (n != 0)
			{
				n--;
				while (buf[offset + l++] != 0)
				{
				}
			}

			return l;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int RelocSeg(byte[] buf, int offset, int len, int rTab, File65 fp)
		{
			int adr = -1;
			int type, seg, old, @new;

			while (buf[rTab] != 0)
			{
				if ((buf[rTab] & 255) == 255)
				{
					adr += 254;
					rTab++;
				}
				else
				{
					adr += buf[rTab] & 255;
					rTab++;
					type = buf[rTab] & 0xe0;
					seg = buf[rTab] & 0x07;
					rTab++;

					switch (type)
					{
						case 0x80:
						{
							old = buf[offset + adr] + 256 * buf[offset + adr + 1];
							@new = old + RelDiff(seg, fp);
							buf[offset + adr] = (byte)(@new & 255);
							buf[offset + adr + 1] = (byte)((@new >> 8) & 255);
							break;
						}

						case 0x40:
						{
							old = buf[offset + adr] + 256 * buf[rTab];
							@new = old + RelDiff(seg, fp);
							buf[offset + adr] = (byte)((@new >> 8) & 255);
							buf[rTab] = (byte)(@new & 255);
							rTab++;
							break;
						}

						case 0x20:
						{
							old = buf[offset + adr];
							@new = old + RelDiff(seg, fp);
							buf[offset + adr] = (byte)(@new & 255);
							break;
						}
					}

					if (seg == 0)
						rTab += 2;
				}
			}

			return ++rTab;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int RelDiff(int s, File65 fp)
		{
			return s == 2 ? fp.TDiff : s == 3 ? fp.DDiff : s == 4 ? fp.BDiff : s == 5 ? fp.ZDiff : 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int RelocGlobals(byte[] buf, int offset, File65 fp)
		{
			int n = buf[offset] + 256 * buf[offset + 1];
			offset += 2;

			while (n != 0)
			{
				while (buf[offset++] != 0)
				{
				}

				int seg = buf[offset];
				int old = buf[offset + 1] + 256 * buf[offset + 2];
				int @new = old + RelDiff(seg, fp);

				buf[offset + 1] = (byte)(@new & 255);
				buf[offset + 2] = (byte)((@new >> 8) & 255);
				offset += 3;
				n--;
			}

			return offset;
		}
		#endregion
	}
}
