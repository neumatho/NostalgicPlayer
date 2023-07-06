/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64.Cia
{
	/// <summary>
	/// InterruptSource that acts like old CIA
	/// </summary>
	internal sealed class InterruptSource6526 : InterruptSource
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public InterruptSource6526(EventScheduler scheduler, Mos652x parent) : base(scheduler, parent)
		{
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void TriggerInterrupt()
		{
			idr |= INTERRUPT_REQUEST;
		}



		/********************************************************************/
		/// <summary>
		/// Trigger an interrupt
		/// </summary>
		/********************************************************************/
		public override void Trigger(uint8_t interruptMask)
		{
			if (IsTriggered(interruptMask))
			{
				// Interrupts are delayed by 1 clk on old CIAs
				Schedule(1);
			}

			// If timer B underflows during the acknowledge cycle
			// it triggers an interrupt as expected
			// but the second bit in icr is not set
			if ((interruptMask == INTERRUPT_UNDERFLOW_B) && Ack0())
			{
				idr &= unchecked((uint8_t)~INTERRUPT_UNDERFLOW_B);
				idrTemp &= unchecked((uint8_t)~INTERRUPT_UNDERFLOW_B);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Clear interrupt state
		/// </summary>
		/********************************************************************/
		public override uint8_t Clear()
		{
			uint8_t oldIdr = base.Clear();
			idr &= INTERRUPT_REQUEST;

			return oldIdr;
		}
		#endregion
	}
}
