/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp
{
	/// <summary>
	/// An interface that allows access to C64 memory
	/// for loading tunes and apply SID specific hacks
	/// </summary>
	internal interface ISidMemory
	{
		/// <summary>
		/// Write one byte to memory
		/// </summary>
		void WriteMemByte(uint_least16_t addr, uint8_t value);

		/// <summary>
		/// Write two contiguous bytes to memory
		/// </summary>
		void WriteMemWord(uint_least16_t addr, uint_least16_t value);

		/// <summary>
		/// Fill RAM area with a constant value
		/// </summary>
		void FillRam(uint_least16_t start, uint8_t value, uint size);

		/// <summary>
		/// Copy a buffer into a RAM area
		/// </summary>
		void FillRam(uint_least16_t start, uint8_t[] source, uint sourceOffset, uint size);

		/// <summary>
		/// Change the RESET vector
		/// </summary>
		void InstallResetHook(uint_least16_t addr);

		/// <summary>
		/// Set BASIC warm start address
		/// </summary>
		void InstallBasicTrap(uint_least16_t addr);

		/// <summary>
		/// Set the start tune
		/// </summary>
		void SetBasicSubTune(uint8_t tune);

		/// <summary>
		/// Set the kernal ROM bank
		/// </summary>
		void SetKernal(uint8_t[] rom);

		/// <summary>
		/// Set the basic ROM bank
		/// </summary>
		void SetBasic(uint8_t[] rom);

		/// <summary>
		/// Set the character ROM bank
		/// </summary>
		void SetCharGen(uint8_t[] rom);
	}
}
