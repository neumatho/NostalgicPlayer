/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Environment;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Event;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Imp;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Mos6526
{
	/// <summary>
	/// Fake CIA timer for Sidplay1 environment modes
	/// </summary>
	internal class Sid6526 : CoComponent, ISidComponent
	{
		#region Event class implementation
		private class MyEvent : Event.Event
		{
			private readonly Sid6526 parent;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public MyEvent(Sid6526 parent) : base("CIA Timer A")
			{
				this.parent = parent;
			}



			/********************************************************************/
			/// <summary>
			/// Handle the event
			/// </summary>
			/********************************************************************/
			public override void DoEvent()
			{
				parent.DoEvent();
			}
		}
		#endregion

		private readonly MyEvent myEvent;

		private IC64Env env;
		private IEventContext eventContext;
		private uint accessClk;
		private EventPhase phase;

		private byte[] regs = new byte[0x10];
		private byte cra;			// Timer A Control Register
		private ushort taLatch;
		private ushort ta;			// Current count (reduces to zero)
		private uint rnd;
		private ushort count;
		private bool locked;		// Prevent code changing CIA

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Sid6526(IC64Env env) : base("SID6526")
		{
			myEvent = new MyEvent(this);

			this.env = env;
			eventContext = env.Context;
			phase = EventPhase.ClockPhi1;
			rnd = (uint)new Random((int)DateTime.Now.Ticks).Next();

			Clock(0xffff);
			Reset(false);
		}

		#region ISidUnknown overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override bool IQuery(IId iid, out object implementation)
		{
			implementation = null;
			return false;
		}
		#endregion

		#region ISidComponent implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			Reset(false);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public byte Read(byte addr)
		{
			if (addr > 0x0f)
				return 0;

			switch (addr)
			{
				case 0x4:
				case 0x5:
				case 0x11:
				case 0x12:
				{
					rnd = rnd * 13 + 1;
					return (byte)(rnd >> 3);
				}

				default:
					return regs[addr];
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Write(byte addr, byte data)
		{
			if (addr > 0x0f)
				return;

			regs[addr] = data;

			if (locked)
				return;		// Stop program changing time interval

			{
				// Sync up timer
				uint cycles = eventContext.GetTime(accessClk, phase);
				accessClk += cycles;
				ta -= (ushort)cycles;

				if (ta == 0)
					myEvent.DoEvent();
			}

			switch (addr)
			{
				case 0x4:
				{
					Endian.Endian16Lo8(ref taLatch, data);
					break;
				}

				case 0x5:
				{
					Endian.Endian16Hi8(ref taLatch, data);
					if ((cra & 0x01) == 0)	// Reload timer if stopped
						ta = taLatch;

					break;
				}

				case 0xe:
				{
					cra = (byte)(data | 0x01);
					if ((data & 0x10) != 0)
					{
						cra &= unchecked((byte)~0x10);
						ta = taLatch;
					}

					myEvent.Schedule(eventContext, (uint)ta + 3, phase);
					break;
				}
			}
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clock(ushort cnt)
		{
			count = cnt;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Lock()
		{
			locked = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset(bool seed)
		{
			locked = false;
			ta = taLatch = count;
			cra = 0;

			// Initialize random number generator
			if (seed)
				rnd = 0;
			else
				rnd += (byte)(DateTime.Now.Ticks & 0xff);

			accessClk = 0;

			// Remove outstanding events
			myEvent.Cancel();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Handle the event
		/// </summary>
		/********************************************************************/
		private void DoEvent()
		{
			// Timer modes
			accessClk = eventContext.GetTime(phase);
			ta = taLatch;
			myEvent.Schedule(eventContext, (uint)ta + 1, phase);
			env.InterruptIrq(true);
		}
		#endregion
	}
}
