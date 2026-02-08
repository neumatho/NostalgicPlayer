/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64.Cpu;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64.Banks
{
	/// <summary>
	/// Kernal ROM
	///
	/// Located at $E000-$FFFF
	/// </summary>
	internal sealed class KernalRomBank : RomBank
	{
		private uint8_t resetVectorLo;	// 0xfffc
		private uint8_t resetVectorHi;	// 0xfffd

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public KernalRomBank() : base(0x2000)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Copy content from source buffer
		/// </summary>
		/********************************************************************/
		public override void Set(uint8_t[] kernal)
		{
			base.Set(kernal);

			if (kernal == null)
			{
				Array.Fill(rom, Opcodes.RTSn);

				// IRQ routine
				SetVal(0xea31, Opcodes.JMPw);
				SetVal(0xea32, 0x7e);
				SetVal(0xea33, 0xea);

				SetVal(0xea7e, Opcodes.NOPa);	// Clear IRQ
				SetVal(0xea7f, 0x0d);
				SetVal(0xea80, 0xdc);
				SetVal(0xea81, Opcodes.PLAn);	// Restore registers
				SetVal(0xea82, Opcodes.TAYn);
				SetVal(0xea83, Opcodes.PLAn);
				SetVal(0xea84, Opcodes.TAXn);
				SetVal(0xea85, Opcodes.PLAn);
				SetVal(0xea86, Opcodes.RTIn);	// Return from interrupt

				// RESET
				SetVal(0xfce2, 0x02);			// Halt

				// NMI entry point
				SetVal(0xfe43, Opcodes.SEIn);
				SetVal(0xfe44, Opcodes.JMPi);	// Jump to NMI routine (Default: $fe47)
				SetVal(0xfe45, 0x18);
				SetVal(0xfe46, 0x03);

				// NMI routine
				SetVal(0xfe47, Opcodes.RTIn);

				// IRQ entry point
				SetVal(0xff48, Opcodes.PHAn);	// Save regs
				SetVal(0xff49, Opcodes.TXAn);
				SetVal(0xff4a, Opcodes.PHAn);
				SetVal(0xff4b, Opcodes.TYAn);
				SetVal(0xff4c, Opcodes.PHAn);
				SetVal(0xff4d, Opcodes.JMPi);	// Jump to IRQ routine (Default: $ea31)
				SetVal(0xff4e, 0x14);
				SetVal(0xff4f, 0x03);

				// Hardware vectors
				SetVal(0xfffa, 0x43);			// NMI vector $fe43
				SetVal(0xfffb, 0xfe);
				SetVal(0xfffc, 0xe2);			// RESET vector $fce2
				SetVal(0xfffd, 0xfc);
				SetVal(0xfffe, 0x48);			// IRQ/BRK vector $ff48
				SetVal(0xffff, 0xff);
			}

			// Backup reset vector
			resetVectorLo = GetVal(0xfffc);
			resetVectorHi = GetVal(0xfffd);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			// Restore original reset vector
			SetVal(0xfffc, resetVectorLo);
			SetVal(0xfffd, resetVectorHi);
		}



		/********************************************************************/
		/// <summary>
		/// Change the RESET vector
		/// </summary>
		/********************************************************************/
		public void InstallResetHook(uint_least16_t addr)
		{
			SetVal(0xfffc, SidEndian.Endian_16Lo8(addr));
			SetVal(0xfffd, SidEndian.Endian_16Hi8(addr));
		}
	}
}
