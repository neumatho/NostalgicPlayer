/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Banks
{
	/// <summary>
	/// Area backed by RAM
	/// </summary>
	internal sealed class SystemRamBank : IBank
	{
		public readonly uint8_t[] ram = new uint8_t[0x10000];

		/********************************************************************/
		/// <summary>
		/// Initialize RAM with powerup pattern
		///
		/// $0000: 00 00 ff ff ff ff 00 00 00 00 ff ff ff ff 00 00
		/// ...
		/// $4000: ff ff 00 00 00 00 ff ff ff ff 00 00 00 00 ff ff
		/// ...
		/// $8000: 00 00 ff ff ff ff 00 00 00 00 ff ff ff ff 00 00
		/// ...
		/// $c000: ff ff 00 00 00 00 ff ff ff ff 00 00 00 00 ff ff
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			uint8_t @byte = 0x00;

			for (int j = 0x0000; j < 0x10000; j += 0x4000)
			{
				Array.Fill(ram, @byte, j, 0x4000);
				@byte = (uint8_t)~@byte;

				for (int i = 0x02; i < 0x4000; i += 0x08)
					Array.Fill(ram, @byte, j + i, 0x04);
			}
		}

		#region IBank implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint8_t Peek(uint_least16_t address)
		{
			return ram[address];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Poke(uint_least16_t address, uint8_t value)
		{
			ram[address] = value;
		}
		#endregion
	}
}
