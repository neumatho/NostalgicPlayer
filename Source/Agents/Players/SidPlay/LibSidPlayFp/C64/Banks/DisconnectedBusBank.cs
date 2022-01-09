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
	/// IO1/IO2
	///
	/// Memory mapped registers or machine code routines of optional external devices.
	///
	/// I/O Area #1 located at $DE00-$DEFF
	///
	/// I/O Area #2 located at $DF00-$DFFF
	/// </summary>
	internal sealed class DisconnectedBusBank : IBank
	{
		private readonly IPla pla;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DisconnectedBusBank(IPla pla)
		{
			this.pla = pla;
		}

		#region IBank implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Poke(uint_least16_t address, uint8_t value)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint8_t Peek(uint_least16_t address)
		{
			return pla.GetLastReadByte();
		}
		#endregion
	}
}
