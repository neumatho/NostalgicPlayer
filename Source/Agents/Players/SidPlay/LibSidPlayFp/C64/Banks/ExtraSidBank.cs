/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Collections.Generic;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Banks
{
	/// <summary>
	/// Extra SID bank
	/// </summary>
	internal sealed class ExtraSidBank : IBank
	{
		/// <summary>
		/// Size of mapping table. Each 32 bytes another SID chip base address
		/// can be assigned to
		/// </summary>
		private const int MAPPER_SIZE = 8;

		/// <summary>
		/// SID mapping table.
		/// Maps a SID chip base address to a SID
		/// or to the underlying bank
		/// </summary>
		private readonly IBank[] mapper = new IBank[MAPPER_SIZE];

		private readonly List<C64Sid> sids = new List<C64Sid>();

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			foreach (C64Sid e in sids)
				e.Reset(0xf);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void ResetSidMapper(IBank bank)
		{
			for (int i = 0; i < MAPPER_SIZE; i++)
				mapper[i] = bank;
		}



		/********************************************************************/
		/// <summary>
		/// Set SID emulation
		/// </summary>
		/********************************************************************/
		public void AddSid(C64Sid s, int address)
		{
			sids.Add(s);
			mapper[MapperIndex(address)] = s;
		}

		#region IBank implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint8_t Peek(uint_least16_t addr)
		{
			return mapper[MapperIndex(addr)].Peek(addr);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Poke(uint_least16_t addr, uint8_t data)
		{
			mapper[MapperIndex(addr)].Poke(addr, data);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint MapperIndex(int address)
		{
			return (uint)(address >> 5 & (MAPPER_SIZE - 1));
		}
		#endregion
	}
}
