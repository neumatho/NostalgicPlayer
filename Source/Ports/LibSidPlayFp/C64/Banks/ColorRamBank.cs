/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64.Banks
{
	/// <summary>
	/// Color RAM.
	///
	/// 1K x 4-bit Static RAM that stores text screen color information.
	///
	/// Located at $D800-$DBFF (last 24 bytes are unused)
	/// </summary>
	internal sealed class ColorRamBank : IBank
	{
		private readonly uint8_t[] ram = new uint8_t[0x400];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			Array.Clear(ram, 0, ram.Length);
		}

		#region IBank implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Poke(uint_least16_t address, uint8_t value)
		{
			ram[address & 0x3ff] = (uint8_t)(value & 0xf);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint8_t Peek(uint_least16_t address)
		{
			return ram[address & 0x3ff];
		}
		#endregion
	}
}
