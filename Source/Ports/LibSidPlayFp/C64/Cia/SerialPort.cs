/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.C64.Cia
{
	/// <summary>
	/// Emulation of the serial port
	/// </summary>
	internal class SerialPort
	{
		#region Private implementation of Event
		private class PrivateEvent : Event
		{
			private readonly SerialPort parent;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public PrivateEvent(string name, SerialPort parent) : base(name)
			{
				this.parent = parent;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public override void DoEvent()
			{
				parent.parent.SpInterrupt();
			}
		}
		#endregion

		private readonly PrivateEvent eventObject;

		/// <summary>
		/// Pointer to the MOS6526 which this Serial Port belongs to
		/// </summary>
		private readonly Mos652x parent;

		/// <summary>
		/// Event context
		/// </summary>
		private readonly EventScheduler eventScheduler;

		private readonly EventCallback flipCntEvent;
		private readonly EventCallback flipFakeEvent;
		private readonly EventCallback startSdrEvent;

		private event_clock_t lastSync;

		private int count;

		private uint8_t cnt;
		private uint8_t cntHistory;

		private bool loaded;
		private bool pending;

		private bool forceFinish;

		private bool model4485 = false;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SerialPort(EventScheduler scheduler, Mos652x parent)
		{
			eventObject = new PrivateEvent("Serial Port interrupt", this);
			this.parent = parent;
			eventScheduler = scheduler;
			flipCntEvent = new EventCallback("flip CNT", FlipCnt);
			flipFakeEvent = new EventCallback("flip fake", FlipFake);
			startSdrEvent = new EventCallback("start SDR", DoStartSdr);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			count = 0;
			cntHistory = 0;
			cnt = 1;
			loaded = false;
			pending = false;
			forceFinish = false;

			lastSync = eventScheduler.GetTime(EventScheduler.event_phase_t.EVENT_CLOCK_PHI1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void StartSdr()
		{
			eventScheduler.Schedule(startSdrEvent, 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetModel4485(bool is4485)
		{
			model4485 = is4485;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SwitchSerialDirection(bool input)
		{
			SyncCntHistory();

			if (input)
			{
				uint8_t cntVal = (uint8_t)(model4485 ? 0x7 : 0x6);
				forceFinish = (cntHistory & cntVal) != cntVal;

				if (!forceFinish)
				{
					if ((count != 2) && (eventScheduler.Remaining(flipCntEvent) == 1))
						forceFinish = true;
				}
			}
			else
			{
				if (forceFinish)
				{
					eventScheduler.Cancel(eventObject);
					eventScheduler.Schedule(eventObject, 2);
					forceFinish = false;
				}
			}

			cnt = 1;
			cntHistory |= 1;

			eventScheduler.Cancel(flipCntEvent);
			eventScheduler.Cancel(flipFakeEvent);

			count = 0;
			loaded = false;
			pending = false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Handle()
		{
			if (loaded && (count == 0))
			{
				// Output rate 8 bits at ta / 2
				count = 16;
			}

			if (count == 0)
				return;

			if (eventScheduler.IsPending(flipFakeEvent) || eventScheduler.IsPending(flipCntEvent))
			{
				eventScheduler.Cancel(flipFakeEvent);
				eventScheduler.Schedule(flipFakeEvent, 2);
			}
			else
				eventScheduler.Schedule(flipCntEvent, 2);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SyncCntHistory()
		{
			event_clock_t time = eventScheduler.GetTime(EventScheduler.event_phase_t.EVENT_CLOCK_PHI1);
			event_clock_t clocks = time - lastSync;
			lastSync = time;

			for (int i = 0; i < clocks; i++)
				cntHistory = (uint8_t)((cntHistory << 1) | cnt);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void DoStartSdr()
		{
			if (!loaded)
				loaded = true;
			else
				pending = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FlipFake()
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FlipCnt()
		{
			if (count == 0)
				return;

			SyncCntHistory();

			cnt ^= 1;

			if (--count == 1)
			{
				eventScheduler.Cancel(eventObject);
				eventScheduler.Schedule(eventObject, 2);

				loaded = pending;
				pending = false;
			}
		}
		#endregion
	}
}
