/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Banks
{
	/// <summary>
	/// Base interface for memory and I/O banks
	/// </summary>
	internal interface IBank
	{
		/// <summary>
		/// Bank write.
		///
		/// Override this method if you expect write operations on your bank.
		/// Leave unimplemented if it's logically/operationally impossible for
		/// writes to ever arrive to bank
		/// </summary>
		void Poke(uint_least16_t address, uint8_t value);

		/// <summary>
		/// Bank read. You probably
		/// should override this method, except if the Bank is only used in
		/// write context
		/// </summary>
		uint8_t Peek(uint_least16_t address);
	}
}
