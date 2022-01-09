/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Banks;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64
{
	/// <summary>
	/// The C64 MMU chip
	/// </summary>
	internal sealed class Mmu : IPla, ISidMemory
	{
		private readonly EventScheduler eventScheduler;

		// CPU port signals
		private bool loRam, hiRam, charen;

		/// <summary>
		/// CPU read memory mapping in 4k chunks
		/// </summary>
		private readonly IBank[] cpuReadMap = new IBank[16];

		/// <summary>
		/// CPU write memory mapping in 4k chunks
		/// </summary>
		private readonly IBank[] cpuWriteMap = new IBank[16];

		/// <summary>
		/// IO region handler
		/// </summary>
		private readonly IOBank ioBank;

		/// <summary>
		/// Kernal ROM
		/// </summary>
		private readonly KernalRomBank kernalRomBank = new KernalRomBank();

		/// <summary>
		/// BASIC ROM
		/// </summary>
		private readonly BasicRomBank basicRomBank = new BasicRomBank();

		/// <summary>
		/// Character ROM
		/// </summary>
		private readonly CharacterRomBank characterRomBank = new CharacterRomBank();

		/// <summary>
		/// RAM
		/// </summary>
		private readonly SystemRamBank ramBank = new SystemRamBank();

		/// <summary>
		/// RAM bank 0
		/// </summary>
		private readonly ZeroRamBank zeroRamBank;

		/// <summary>
		/// Random seed
		/// </summary>
		private uint seed;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Mmu(EventScheduler scheduler, IOBank ioBank)
		{
			eventScheduler = scheduler;
			loRam = false;
			hiRam = false;
			charen = false;
			this.ioBank = ioBank;
			zeroRamBank = new ZeroRamBank(this, ramBank);
			seed = 3686734;

			cpuReadMap[0] = zeroRamBank;
			cpuWriteMap[0] = zeroRamBank;

			for (int i = 1; i < 16; i++)
			{
				cpuReadMap[i] = ramBank;
				cpuWriteMap[i] = ramBank;
			}
		}

		#region IPla implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetCpuPort(uint8_t state)
		{
			loRam = (state & 1) != 0;
			hiRam = (state & 2) != 0;
			charen = (state & 4) != 0;

			UpdateMappingPhi2();
		}



		/********************************************************************/
		/// <summary>
		/// This should actually return last byte read from VIC but since
		/// the VIC emulation currently does not fetch any values from memory
		/// we return a pseudo random value
		/// </summary>
		/********************************************************************/
		public uint8_t GetLastReadByte()
		{
			seed = Random(seed);
			return (uint8_t)seed;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public event_clock_t GetPhi2Time()
		{
			return eventScheduler.GetTime(EventScheduler.event_phase_t.EVENT_CLOCK_PHI2);
		}
		#endregion

		#region ISidMemory implementation
		/********************************************************************/
		/// <summary>
		/// Write one byte to memory
		/// </summary>
		/********************************************************************/
		public void WriteMemByte(uint_least16_t addr, uint8_t value)
		{
			ramBank.Poke(addr, value);
		}



		/********************************************************************/
		/// <summary>
		/// Write two contiguous bytes to memory
		/// </summary>
		/********************************************************************/
		public void WriteMemWord(uint_least16_t addr, uint_least16_t value)
		{
			SidEndian.Endian_Little16(ramBank.ram, addr, value);
		}



		/********************************************************************/
		/// <summary>
		/// Fill RAM area with a constant value
		/// </summary>
		/********************************************************************/
		public void FillRam(uint_least16_t start, uint8_t value, uint size)
		{
			Array.Fill(ramBank.ram, value, start, (int)size);
		}



		/********************************************************************/
		/// <summary>
		/// Copy a buffer into a RAM area
		/// </summary>
		/********************************************************************/
		public void FillRam(uint_least16_t start, uint8_t[] source, uint sourceOffset, uint size)
		{
			Array.Copy(source, sourceOffset, ramBank.ram, start, size);
		}



		/********************************************************************/
		/// <summary>
		/// Change the RESET vector
		/// </summary>
		/********************************************************************/
		public void InstallResetHook(uint_least16_t addr)
		{
			kernalRomBank.InstallResetHook(addr);
		}



		/********************************************************************/
		/// <summary>
		/// Set BASIC warm start address
		/// </summary>
		/********************************************************************/
		public void InstallBasicTrap(uint_least16_t addr)
		{
			basicRomBank.InstallTrap(addr);
		}



		/********************************************************************/
		/// <summary>
		/// Set the start tune
		/// </summary>
		/********************************************************************/
		public void SetBasicSubTune(uint8_t tune)
		{
			basicRomBank.SetSubTune(tune);
		}



		/********************************************************************/
		/// <summary>
		/// Set the kernal ROM bank
		/// </summary>
		/********************************************************************/
		public void SetKernal(uint8_t[] rom)
		{
			kernalRomBank.Set(rom);
		}



		/********************************************************************/
		/// <summary>
		/// Set the basic ROM bank
		/// </summary>
		/********************************************************************/
		public void SetBasic(uint8_t[] rom)
		{
			basicRomBank.Set(rom);
		}



		/********************************************************************/
		/// <summary>
		/// Set the character ROM bank
		/// </summary>
		/********************************************************************/
		public void SetCharGen(uint8_t[] rom)
		{
			characterRomBank.Set(rom);
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Access memory as seen by CPU
		/// </summary>
		/********************************************************************/
		public uint8_t CpuRead(uint_least16_t addr)
		{
			return cpuReadMap[addr >> 12].Peek(addr);
		}



		/********************************************************************/
		/// <summary>
		/// Access memory as seen by CPU
		/// </summary>
		/********************************************************************/
		public void CpuWrite(uint_least16_t addr, uint8_t data)
		{
			cpuWriteMap[addr >> 12].Poke(addr, data);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			ramBank.Reset();
			zeroRamBank.Reset();

			// Reset the ROMs to undo the hacks applied
			kernalRomBank.Reset();
			basicRomBank.Reset();

			loRam = false;
			hiRam = false;
			charen = false;

			UpdateMappingPhi2();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void UpdateMappingPhi2()
		{
			cpuReadMap[0x0e] = cpuReadMap[0x0f] = hiRam ? kernalRomBank : ramBank;
			cpuReadMap[0x0a] = cpuReadMap[0x0b] = loRam && hiRam ? basicRomBank : ramBank;

			if (charen && (loRam || hiRam))
				cpuReadMap[0x0d] = cpuWriteMap[0x0d] = ioBank;
			else
			{
				cpuReadMap[0x0d] = !charen && (loRam || hiRam) ? characterRomBank : ramBank;
				cpuWriteMap[0x0d] = ramBank;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint Random(uint val)
		{
			return val * 1664525 + 1013904223;
		}
		#endregion
	}
}
