/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Cpu;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Banks
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
		public override void Set(uint8_t[] basic)
		{
			base.Set(basic);

			// Backup BASIC warm start
			Array.Copy(rom, GetPtr(0xa7ae), trap, 0, trap.Length);

			Array.Copy(rom, GetPtr(0xbf53), subTune, 0, subTune.Length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			// Restore original BASIC warm start
			Array.Copy(trap, 0, rom, GetPtr(0xa7ae), trap.Length);

			Array.Copy(subTune, 0, rom, GetPtr(0xbf53), subTune.Length);
		}


		/********************************************************************/
		/// <summary>
		/// Set BASIC warm start address
		/// </summary>
		/********************************************************************/
		public void InstallTrap(uint_least16_t addr)
		{
			SetVal(0xa7ae, Opcodes.JMPw);
			SetVal(0xa7af, SidEndian.Endian_16Lo8(addr));
			SetVal(0xa7b0, SidEndian.Endian_16Hi8(addr));
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
			SetVal(0xbf56, 0x0c);
			SetVal(0xbf57, 0x03);
			SetVal(0xbf58, Opcodes.JSRw);
			SetVal(0xbf59, 0x2c);
			SetVal(0xbf5a, 0xa8);
			SetVal(0xbf5b, Opcodes.JMPw);
			SetVal(0xbf5c, 0xb1);
			SetVal(0xbf5d, 0xa7);
		}
	}
}
