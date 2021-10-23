/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Banks;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64
{
	/// <summary>
	/// SID interface
	/// </summary>
	internal abstract class C64Sid : IBank
	{
		private readonly uint8_t[] lastPoke = new uint8_t[0x20];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			Array.Clear(lastPoke, 0, 0x20);
			Reset(0);
		}

		#region IBank implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Poke(uint_least16_t address, uint8_t value)
		{
			lastPoke[address & 0x1f] = value;
			Write((uint_least8_t)(address & 0x1f), value);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint8_t Peek(uint_least16_t address)
		{
			return Read((uint_least8_t)(address & 0x1f));
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected abstract uint8_t Read(uint_least8_t addr);



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected abstract void Write(uint_least8_t addr, uint8_t data);



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract void Reset(uint8_t volume);
		#endregion
	}
}
