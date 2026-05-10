/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
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
		private readonly uint8_t[] save_Regs =
		[
			Opcodes.PHAn,
			Opcodes.TXAn,
			Opcodes.PHAn,
			Opcodes.TYAn,
			Opcodes.PHAn
		];

		private readonly uint8_t[] restore_Regs =
		[
			Opcodes.PLAn,
			Opcodes.TAYn,
			Opcodes.PLAn,
			Opcodes.TAXn,
			Opcodes.PLAn
		];

		private readonly uint16_t[] kernal_Functions =
		[
			// Address real address
			0xFF81, 0xFF5B,		// SCINIT
			0xFF84, 0xFDA3,		// IOINIT
			0xFF87, 0xFD50,		// RAMTAS
			0xFF8A, 0xFD15,		// RESTOR
			0xFF8D, 0xFD1A,		// VECTOR
			0xFF90, 0xFE18,		// SETMSG
			0xFF93, 0xEDB9,		// LSTNSA
			0xFF96, 0xEDC7,		// TALKSA
			0xFF99, 0xFE25,		// MEMTOP
			0xFF9C, 0xFE34,		// MEMBOT
			0xFF9F, 0xEA87,		// SCNKEY
			0xFFA2, 0xFE21,		// SETTMO
			0xFFA5, 0xEE13,		// IECIN.
			0xFFA8, 0xEDDD,		// IECOUT
			0xFFAB, 0xEDEF,		// UNTALK
			0xFFAE, 0xEDFE,		// UNLSTN
			0xFFB1, 0xED0C,		// LISTEN
			0xFFB4, 0xED09,		// TALK
			0xFFB7, 0xFE07,		// READST
			0xFFBA, 0xFE00,		// SETLFS
			0xFFBD, 0xFDF9,		// SETNAM
			0xFFC0, 0xF34A,		// OPEN
			0xFFC3, 0xF291,		// CLOSE
			0xFFC6, 0xF20E,		// CHKIN
			0xFFC9, 0xF250,		// CHKOUT
			0xFFCC, 0xF333,		// CLRCHN
			0xFFCF, 0xF157,		// CHRIN
			0xFFD2, 0xF1CA,		// CHROUT
			0xFFD5, 0xF49E,		// LOAD
			0xFFD8, 0xF5DD,		// SAVE
			0xFFDB, 0xF6E4,		// SETTIM
			0xFFDE, 0xF6DD,		// RDTIM
			0xFFE1, 0xF6ED,		// STOP
			0xFFE4, 0xF13E,		// GETIN
			0xFFE7, 0xF32F,		// CLALL
			0xFFEA, 0xF69B,		// UDTIM
			0xFFED, 0xE505,		// SCREEN
			0xFFF0, 0xE50A,		// PLOT
			0xFFF3, 0xE500,		// IOBASE
		];

		private uint_least16_t resetVector;	// 0xfffc-0xfffd

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
		public override void Set(CPointer<uint8_t> kernal)
		{
			base.Set(kernal);

			if (kernal == null)
			{
				CMemory.fill(rom, Opcodes.NOPn, (size_t)rom.Length);

				// IRQ routine
				SetVal(0xea31, Opcodes.JMPw);
				SetVal16(0xea32, 0xea7e);

				SetVal(0xea7e, Opcodes.NOPa);	// Clear IRQ
				SetVal16(0xea7f, 0xdc0d);
				Fill(0xea81, restore_Regs);
				SetVal(0xea86, Opcodes.RTIn);	// Return from interrupt

				// RESET
				SetVal(0xfce2, 0x02);			// Halt

				// NMI entry point
				SetVal(0xfe43, Opcodes.SEIn);
				SetVal(0xfe44, Opcodes.JMPi);	// Jump to NMI routine (Default: $fe47)
				SetVal16(0xfe45, 0x0318);

				// NMI routine
				Fill(0xfe47, save_Regs);

				Fill(0xfebc, restore_Regs);
				SetVal(0xfec1, Opcodes.RTIn);

				// IRQ entry point
				Fill(0xff48, save_Regs);
				SetVal(0xff4d, Opcodes.JMPi);	// Jump to IRQ routine (Default: $ea31)
				SetVal16(0xff4e, 0x0314);

				// Hardware vectors
				SetVal16(0xfffa, 0xfe43);		// NMI vector $fe43
				SetVal16(0xfffc, 0xfce2);		// RESET vector $fce2
				SetVal16(0xfffe, 0xff48);		// IRQ/BRK vector $ff48

				// Standard KERNAL functions called by some unclean rips
				foreach (uint16_t addr in kernal_Functions)
					SetVal(addr, Opcodes.RTSn);
			}

			// Backup reset vector
			resetVector = GetVal16(0xfffc);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			// Restore original reset vector
			SetVal16(0xfffc, resetVector);
		}



		/********************************************************************/
		/// <summary>
		/// Change the RESET vector
		/// </summary>
		/********************************************************************/
		public void InstallResetHook(uint_least16_t addr)
		{
			SetVal16(0xfffc, addr);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Fill(uint_least16_t address, uint8_t[] data)
		{
			CMemory.memcpy(GetPtr(address), data, 5);
		}
		#endregion
	}
}
