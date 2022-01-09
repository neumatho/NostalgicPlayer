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
	/// ROM bank base class
	/// </summary>
	internal abstract class RomBank : IBank
	{
		/// <summary>
		/// The ROM array
		/// </summary>
		protected readonly uint8_t[] rom;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected RomBank(int n)	// n must be a power of two
		{
			rom = new uint8_t[n];
		}



		/********************************************************************/
		/// <summary>
		/// Copy content from source buffer
		/// </summary>
		/********************************************************************/
		public virtual void Set(uint8_t[] source)
		{
			if (source != null)
				Array.Copy(source, rom, rom.Length);
		}



		/********************************************************************/
		/// <summary>
		/// Set value at memory address
		/// </summary>
		/********************************************************************/
		protected void SetVal(uint_least16_t address, uint8_t val)
		{
			rom[address & (rom.Length - 1)] = val;
		}



		/********************************************************************/
		/// <summary>
		/// Return value from memory address
		/// </summary>
		/********************************************************************/
		protected uint8_t GetVal(uint_least16_t address)
		{
			return rom[address & (rom.Length - 1)];
		}



		/********************************************************************/
		/// <summary>
		/// Return pointer to memory address
		/// </summary>
		/********************************************************************/
		protected uint_least16_t GetPtr(uint_least16_t address)
		{
			return (uint_least16_t)(address & (rom.Length - 1));
		}

		#region IBank implementation
		/********************************************************************/
		/// <summary>
		/// Writing to ROM is a no-op
		/// </summary>
		/********************************************************************/
		public void Poke(uint_least16_t address, uint8_t value)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Read from ROM
		/// </summary>
		/********************************************************************/
		public uint8_t Peek(uint_least16_t address)
		{
			return rom[address & (rom.Length - 1)];
		}
		#endregion
	}
}
