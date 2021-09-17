/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Event;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Imp;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.XSid
{
	/// <summary>
	/// Support for PlaySids extended registers
	/// </summary>
	internal abstract partial class XSid : CoEmulation, ISidEmulation
	{
		#region Event class implementation
		private class MyEvent : Event.Event
		{
			private readonly XSid parent;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public MyEvent(XSid parent, string name) : base(name)
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

		private static readonly byte[] sampleConvertTable = new byte[16]
		{
			0x80, 0x94, 0xa9, 0xbc, 0xce, 0xe1, 0xf2, 0x03,
			0x1b, 0x2a, 0x3b, 0x49, 0x58, 0x66, 0x73, 0x7f
		};

		private readonly MyEvent myEvent;

		private readonly Channel ch4;
		private readonly Channel ch5;
		private bool muted;
		private bool suppressed;

		private byte sidData0x18;
		private bool sidSamples;
		private sbyte sampleOffset;
		private bool wasRunning;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected XSid(IEventContext context) : base("XSID", null)
		{
			myEvent = new MyEvent(this, "xSID");

			ch4 = new Channel("CH4", context, this, myEvent);
			ch5 = new Channel("CH5", context, this, myEvent);
			muted = false;
			suppressed = false;
			wasRunning = false;

			SidSamples(true);
		}

		#region ISidUnknown overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override bool IQuery(IId iid, out object implementation)
		{
			if (iid == ISidEmulation.IId())
				implementation = this.StaticCast<ISidEmulation>();
			else if (iid == ISidUnknown.IId())
				implementation = this.StaticCast<ISidEmulation>();
			else
			{
				implementation = null;
				return false;
			}

			return true;
		}
		#endregion

		#region ISidComponent implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual byte Read(byte addr)
		{
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual void Write(byte addr, byte data)
		{
		}
		#endregion

		#region ISidEmulation implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override void Reset(byte volume)
		{
			ch4.Reset();
			ch5.Reset();
			suppressed = false;
			wasRunning = false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual int Output(byte bits)
		{
			if (sidSamples || muted)
				return 0;

			int sample = sampleConvertTable[SampleOutput() + 8];

			return sample << (bits - 8);
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected abstract byte ReadMemByte(ushort addr);



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected abstract void WriteMemByte(byte data);
		#endregion

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SidSamples(bool enable)
		{
			sidSamples = enable;
		}



		/********************************************************************/
		/// <summary>
		/// By muting samples they will start and play them at the
		/// appropriate time but no sound is produced. Un-muting will cause
		/// sound output from the current play position
		/// </summary>
		/********************************************************************/
		public void Mute(bool enable)
		{
			if (!muted && enable && wasRunning)
				RecallSidData0x18();

			muted = enable;
		}



		/********************************************************************/
		/// <summary>
		/// Use suppress to delay the samples and start them later.
		/// Effectivly allows running samples in a frame based mode
		/// </summary>
		/********************************************************************/
		public void Suppress(bool enable)
		{
			// @FIXME@: Mute temporary hack
			suppressed = enable;

			if (!suppressed)
			{
				// Get the channels running
				ch4.CheckForInit();
				ch5.CheckForInit();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void Write(ushort addr, byte data)
		{
			// Make sure address is legal
			if (((addr & 0xfe8c) ^0x000c) != 0)
				return;

			Channel ch = ch4;
			if ((addr & 0x0100) != 0)
				ch = ch5;

			byte tempAddr = (byte)addr;
			ch.Write(tempAddr, data);

			if (tempAddr == 0x1d)
			{
				if (suppressed)
					return;

				ch.CheckForInit();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected bool StoreSidData0x18(byte data)
		{
			sidData0x18 = data;

			if (ch4.active || ch5.active)
			{
				// Force volume to be changed at next clock
				SampleOffsetCalc();

				if (sidSamples)
					return true;
			}

			WriteMemByte(sidData0x18);

			return false;
		}

		#region Event methods
		/********************************************************************/
		/// <summary>
		/// Handle the event
		/// </summary>
		/********************************************************************/
		private void DoEvent()
		{
			if (ch4.active || ch5.active)
			{
				SetSidData0x18();
				wasRunning = true;
			}
			else if (wasRunning)
			{
				RecallSidData0x18();
				wasRunning = false;
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private sbyte SampleOutput()
		{
			sbyte sample = ch4.Output();
			sample += ch5.Output();

			// Automatically compensated for by C64 code
			return sample;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SetSidData0x18()
		{
			if (!sidSamples || muted)
				return;

			byte data = (byte)(sidData0x18 & 0xf0);
			data |= (byte)((sampleOffset + SampleOutput()) & 0x0f);

			WriteMemByte(data);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void RecallSidData0x18()
		{
			// Changed to recall volume differently depending on mode.
			// Normally after samples volume should be restored to half volume,
			// however, Galway tunes sound horrible and seem to require setting back to
			// the original volume. Setting back to the original volume for normal
			// samples can have nasty pulsing effects
			if (ch4.IsGalway())
			{
				if (sidSamples && !muted)
					WriteMemByte(sidData0x18);
			}
			else
				SetSidData0x18();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SampleOffsetCalc()
		{
			// Try to determine a sensible offset between voice
			// and sample volumes
			byte lower = (byte)(ch4.Limit() + ch5.Limit());

			// Both channels seem to be off. Keep current offset!
			if (lower == 0)
				return;

			sampleOffset = (sbyte)(sidData0x18 & 0x0f);

			// Is possible to compensate for both channels
			// set to 4 bits here, but should never happen
			if (lower > 8)
				lower >>= 1;

			byte upper = (byte)(0x0f - lower + 1);

			// Check against limits
			if (sampleOffset < lower)
				sampleOffset = (sbyte)lower;
			else if (sampleOffset > upper)
				sampleOffset = (sbyte)upper;
		}
		#endregion
	}
}
