/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Banks
{
	/// <summary>
	/// Area backed by RAM, including CPU port addresses 0 and 1.
	///
	/// This is bit of a fake. We know that the CPU port is an internal
	/// detail of the CPU, and therefore CPU should simply pay the price
	/// for reading/writing to $00/$01.
	///
	/// However, that would slow down all accesses, which is suboptimal. Therefore
	/// we install this little hook to the 4k 0 region to deal with this.
	///
	/// Implementation based on VICE code
	/// </summary>
	internal sealed class ZeroRamBank : IBank
	{
		#region DataBit class
		// Unused data port bits emulation, as investigated by groepaz:
		//
		// - There are 2 different unused bits, 1) the output bits, 2) the input bits
		// - The output bits can be (re)set when the data-direction is set to output
		//   for those bits and the output bits will not drop-off to 0.
		// - When the data-direction for the unused bits is set to output then the
		//   unused input bits can be (re)set by writing to them, when set to 1 the
		//   drop-off timer will start which will cause the unused input bits to drop
		//   down to 0 in a certain amount of time.
		// - When an unused input bit already had the drop-off timer running, and is
		//   set to 1 again, the drop-off timer will restart.
		// - When an unused bit changes from output to input, and the current output
		//   bit is 1, the drop-off timer will restart again
		private class DataBit
		{
			// $01 bits 6 and 7 fall-off cycles (1->0), average is about 350 msec for a 6510
			// and about 1500 msec for a 8500.
			//
			// NOTE: fall-off cycles are heavily chip- and temperature dependent. As a
			//       consequence it is very hard to find suitable realistic values that
			//       always work and we can only tweak them based on test cases. (unless we
			//       want to make it configurable or emulate temperature over time =))
			//
			//       It probably makes sense to tweak the values for a warmed up CPU, since
			//       this is likely how (old) programs were coded and tested :)
			//
			// NOTE: the unused bits of the 6510 seem to be much more temperature dependent
			//       and the fall-off time decreases quicker and more drastically than on a
			//       8500
			//
			// cpuports.prg from the lorenz test suite will fail when the falloff takes more
			// than 1373 cycles. This suggests that he tested on a well warmed up c64 :)
			// He explicitly delays by ~1280 cycles and mentions capacitance, so he probably
			// even was aware of what happens.
			private const event_clock_t C64_CPU6510_DATA_PORT_FALL_OFF_CYCLES = 350000;
			private const event_clock_t C64_CPU8500_DATA_PORT_FALL_OFF_CYCLES = 1500000; // Currently unused

			private readonly int bit;

			/// <summary>
			/// Cycle that should invalidate the bit
			/// </summary>
			private event_clock_t dataSetClk;

			/// <summary>
			/// Indicates if the bit is in the process of falling off
			/// </summary>
			private bool isFallingOff;

			/// <summary>
			/// Value of the bit
			/// </summary>
			private uint8_t dataSet;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public DataBit(int bit)
			{
				this.bit = bit;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public void Reset()
			{
				isFallingOff = false;
				dataSet = 0;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public uint8_t ReadBit(event_clock_t phi2Time)
			{
				if (isFallingOff && (dataSetClk < phi2Time))
				{
					// Discharge the "capacitor"
					Reset();
				}

				return dataSet;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public void WriteBit(event_clock_t phi2Time, uint8_t value)
			{
				dataSetClk = phi2Time + C64_CPU6510_DATA_PORT_FALL_OFF_CYCLES;
				dataSet = (uint8_t)(value & (1 << bit));
				isFallingOff = true;
			}
		}
		#endregion

		/// <summary>
		/// Not emulated
		/// </summary>
		private static readonly bool tape_sense = false;

		private readonly IPla pla;

		/// <summary>
		/// C64 RAM area
		/// </summary>
		private readonly SystemRamBank ramBank;

		// Unused bits of the data port
		private readonly DataBit dataBit6 = new DataBit(6);
		private readonly DataBit dataBit7 = new DataBit(7);

		// Value written to processor port
		private uint8_t dir;
		private uint8_t data;

		/// <summary>
		/// Value read from processor port
		/// </summary>
		private uint8_t dataRead;

		/// <summary>
		/// State of processor port pins
		/// </summary>
		private uint8_t procPortPins;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ZeroRamBank(IPla pla, SystemRamBank ramBank)
		{
			this.pla = pla;
			this.ramBank = ramBank;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			dataBit6.Reset();
			dataBit7.Reset();

			dir = 0;
			data = 0x3f;
			dataRead = 0x3f;
			procPortPins = 0x3f;

			UpdateCpuPort();
		}

		#region IBank implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint8_t Peek(uint_least16_t address)
		{
			switch (address)
			{
				case 0:
					return dir;

				case 1:
				{
					uint8_t retVal = dataRead;

					// For unused bits in input mode, the value comes from the "capacitor"

					// Set real value of bit 6
					if ((dir & 0x40) == 0)
					{
						retVal &= unchecked((uint8_t)(~0x40));
						retVal |= dataBit6.ReadBit(pla.GetPhi2Time());
					}

					// Set real value of bit 7
					if ((dir & 0x80) == 0)
					{
						retVal &= unchecked((uint8_t)(~0x80));
						retVal |= dataBit7.ReadBit(pla.GetPhi2Time());
					}

					return retVal;
				}

				default:
					return ramBank.Peek(address);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Poke(uint_least16_t address, uint8_t value)
		{
			switch (address)
			{
				case 0:
				{
					// When switching an unused bit from output (where it contained a
					// stable value) to input mode (where the input is floating), some
					// of the change is transferred to the floating input
					if (dir != value)
					{
						// Check if bit 6 has flipped from 1 to 0
						if (((dir & 0x40) != 0) && ((value & 0x40) == 0))
							dataBit6.WriteBit(pla.GetPhi2Time(), data);

						// Check if bit 7 has flipped from 1 to 0
						if (((dir & 0x80) != 0) && ((value & 0x80) == 0))
							dataBit7.WriteBit(pla.GetPhi2Time(), data);

						dir = value;
						UpdateCpuPort();
					}

					value = pla.GetLastReadByte();
					break;
				}

				case 1:
				{
					// When writing to an unused bit that is output, charge the "capacitor",
					// otherwise don't touch it
					if ((dir & 0x40) != 0)
						dataBit6.WriteBit(pla.GetPhi2Time(), value);

					if ((dir & 0x80) != 0)
						dataBit7.WriteBit(pla.GetPhi2Time(), value);

					if (data != value)
					{
						data = value;
						UpdateCpuPort();
					}

					value = pla.GetLastReadByte();
					break;
				}
			}

			ramBank.Poke(address, value);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void UpdateCpuPort()
		{
			// Update data pins for which direction is OUTPUT
			procPortPins = (uint8_t)((procPortPins & ~dir) | (data & dir));

			dataRead = (uint8_t)((data | ~dir) & (procPortPins | 0x17));

			pla.SetCpuPort((uint8_t)((data | ~dir) & 0x07));

			if ((dir & 0x20) == 0)
				dataRead &= unchecked((uint8_t)~0x20);

			if (tape_sense && ((dir & 0x10) == 0))
				dataRead &= unchecked((uint8_t)~0x10);
		}
		#endregion
	}
}
