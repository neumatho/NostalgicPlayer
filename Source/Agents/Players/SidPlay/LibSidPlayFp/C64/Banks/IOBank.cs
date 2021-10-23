/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Banks
{
	/// <summary>
	/// IO region handler. 4k region, 16 chips, 256b banks
	///
	/// Located at $D000-$DFFF
	/// </summary>
	internal sealed class IOBank : IBank
	{
		private readonly IBank[] map = new IBank[16];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetBank(int num, IBank bank)
		{
			map[num] = bank;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public IBank GetBank(int num)
		{
			return map[num];
		}

		#region IBank implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint8_t Peek(uint_least16_t addr)
		{
			return map[addr >> 8 & 0xf].Peek(addr);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Poke(uint_least16_t addr, uint8_t data)
		{
			map[addr >> 8 & 0xf].Poke(addr, data);
		}
		#endregion
	}
}
