/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
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
				// IRQ entry point
				SetVal(0xffa0, Opcodes.PHAn);	// Save regs
				SetVal(0xffa1, Opcodes.TXAn);
				SetVal(0xffa2, Opcodes.PHAn);
				SetVal(0xffa3, Opcodes.TYAn);
				SetVal(0xffa4, Opcodes.PHAn);
				SetVal(0xffa5, Opcodes.JMPi);	// Jump to IRQ routine
				SetVal(0xffa6, 0x14);
				SetVal(0xffa7, 0x03);

				// Halt
				SetVal(0xea39, 0x02);

				// Hardware vectors
				SetVal(0xfffa, 0x39);			// NMI vector
				SetVal(0xfffb, 0xea);
				SetVal(0xfffc, 0x39);			// RESET vector
				SetVal(0xfffd, 0xea);
				SetVal(0xfffe, 0xa0);			// IRQ/BRK vector
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
