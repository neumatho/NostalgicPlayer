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
	/// BASIC ROM
	///
	/// Located at $A000-$BFFF
	/// </summary>
	internal sealed class BasicRomBank : RomBank
	{
		private readonly uint8_t[] trap = new uint8_t[3];
		private readonly uint8_t[] subTune = new uint8_t[11];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public BasicRomBank() : base(0x2000)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Copy content from source buffer
		/// </summary>
		/********************************************************************/
		public override void Set(CPointer<uint8_t> basic)
		{
			base.Set(basic);

			// Backup BASIC warm start
			CMemory.memcpy(trap, GetPtr(0xa7ae), (size_t)trap.Length);

			CMemory.memcpy(subTune, GetPtr(0xbf53), (size_t)subTune.Length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			// Restore original BASIC warm start
			CMemory.memcpy(GetPtr(0xa7ae), trap, (size_t)trap.Length);

			CMemory.memcpy(GetPtr(0xbf53), subTune, (size_t)subTune.Length);
		}



		/********************************************************************/
		/// <summary>
		/// Set BASIC warm start address
		/// </summary>
		/********************************************************************/
		public void InstallTrap(uint_least16_t addr)
		{
			SetVal(0xa7ae, Opcodes.JMPw);
			SetVal16(0xa7af, addr);
		}



		/********************************************************************/
		/// <summary>
		/// Set the start tune
		/// </summary>
		/********************************************************************/
		public void SetSubTune(uint8_t tune)
		{
			SetVal(0xbf53, Opcodes.LDAb);
			SetVal(0xbf54, tune);
			SetVal(0xbf55, Opcodes.STAa);
			SetVal16(0xbf56, 0x030c);
			SetVal(0xbf58, Opcodes.JSRw);
			SetVal16(0xbf59, 0xa82c);
			SetVal(0xbf5b, Opcodes.JMPw);
			SetVal16(0xbf5c, 0xa7b1);
		}
	}
}
