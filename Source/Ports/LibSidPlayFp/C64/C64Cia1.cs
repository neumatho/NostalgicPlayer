/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64.Banks;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64.Cia;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64
{
	/// <summary>
	/// CIA 1
	///
	/// Generates IRQs
	///
	/// Located at $DC00-$DCFF
	/// </summary>
	internal sealed class C64Cia1 : Mos652x, IBank
	{
		private readonly C64Env env;

		private uint_least16_t last_ta;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public C64Cia1(C64Env env) : base(env.Scheduler())
		{
			this.env = env;
		}

		#region IBank implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Poke(uint_least16_t address, uint8_t value)
		{
			uint8_t addr = SidEndian.Endian_16Lo8(address);
			Write(addr, value);

			// Save the value written to Timer A
			if ((addr == 0x04) || (addr == 0x05))
			{
				if (timerA.GetTimer() != 0)
					last_ta = timerA.GetTimer();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint8_t Peek(uint_least16_t address)
		{
			return Read(SidEndian.Endian_16Lo8(address));
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Signal interrupt
		/// </summary>
		/********************************************************************/
		public override void Interrupt(bool state)
		{
			env.InterruptIrq(state);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void PortB()
		{
			uint8_t pb = (uint8_t)(regs[PRB] | ~regs[DDRB]);

			// We should call AdjustDataPort here
			// but we're only interested in bit 4
			env.Lightpen((pb & 0x10) != 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override void Reset()
		{
			last_ta = 0;
			base.Reset();
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint_least16_t GetTimerA()
		{
			return last_ta;
		}
	}
}
