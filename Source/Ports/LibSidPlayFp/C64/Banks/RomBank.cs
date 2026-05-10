/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64.Banks
{
	/// <summary>
	/// ROM bank base class
	/// </summary>
	internal abstract class RomBank : IBank
	{
		/// <summary>
		/// The ROM array
		/// </summary>
		protected readonly CPointer<uint8_t> rom;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected RomBank(int n)	// n must be a power of two
		{
			rom = new CPointer<uint8_t>(n);
		}



		/********************************************************************/
		/// <summary>
		/// Copy content from source buffer
		/// </summary>
		/********************************************************************/
		public virtual void Set(CPointer<uint8_t> source)
		{
			if (source != null)
				CMemory.memcpy(rom, source, (size_t)rom.Length);
		}



		/********************************************************************/
		/// <summary>
		/// Set value at memory address
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void SetVal(uint_least16_t address, uint8_t val)
		{
			rom[address & (rom.Length - 1)] = val;
		}



		/********************************************************************/
		/// <summary>
		/// Set value at memory address
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void SetVal16(uint_least16_t address, uint_least16_t val)
		{
			SidEndian.Endian_Little16(GetPtr(address), val);
		}



		/********************************************************************/
		/// <summary>
		/// Return value from memory address
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected uint8_t GetVal(uint_least16_t address)
		{
			return rom[address & (rom.Length - 1)];
		}



		/********************************************************************/
		/// <summary>
		/// Return value from memory address
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected uint_least16_t GetVal16(uint_least16_t address)
		{
			return SidEndian.Endian_Little16(GetPtr(address));
		}



		/********************************************************************/
		/// <summary>
		/// Return pointer to memory address
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected CPointer<uint8_t> GetPtr(uint_least16_t address)
		{
			return rom + (address & (rom.Length - 1));
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
