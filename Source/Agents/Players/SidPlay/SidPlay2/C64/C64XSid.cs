/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Environment;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Imp;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.C64
{
	/// <summary>
	/// ReSid wrapper
	/// </summary>
	internal class C64XSid : XSid.XSid, ICoAggregate, ISidMixer
	{
		#region CoAggregate class implementation
		private class MyCoAggregate : CoAggregate
		{
			private readonly C64XSid parent;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public MyCoAggregate(C64XSid parent, ISidUnknown unknown) : base(unknown)
			{
				this.parent = parent;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public override ISidUnknown IUnknown()
			{
				return parent.IUnknown();
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public override bool IQuery(IId iid, out object implementation)
			{
				return parent.IQuery(iid, out implementation);
			}
		}
		#endregion

		private readonly MyCoAggregate myCoAggregate;

		private readonly IC64Env env;
		private SidLazyIPtr<ISidEmulation> sid;
		private SidLazyIPtr<ISidMixer> mixer;
		private int gain;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public C64XSid(IC64Env env) : base(env.Context)
		{
			myCoAggregate = new MyCoAggregate(this, IUnknown());

			this.env = env;
			sid = null;
			mixer = null;
			gain = 100;
		}

		#region ISidUnknown implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override bool IQuery(IId iid, out object implementation)
		{
			if (iid == ISidMixer.IId())
			{
				implementation = this.StaticCast<ISidMixer>();
				return true;
			}

			return base.IQuery(iid, out implementation);
		}
		#endregion

		#region ISidComponent implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override byte Read(byte addr)
		{
			return sid.Obj.Read(addr);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override void Write(byte addr, byte data)
		{
			if (addr == 0x18)
				StoreSidData0x18(data);
			else
				sid.Obj.Write(addr, data);
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
			base.Reset(volume);
			sid.Obj.Reset(volume);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override int Output(byte bits)
		{
			return sid.Obj.Output(bits) + (base.Output(bits) * gain / 100);
		}
		#endregion

		#region ISidMixer implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Mute(byte num, bool enable)
		{
			if (num == 3)
				Mute(enable);
			else if (mixer.Obj != null)
				mixer.Obj.Mute(num, enable);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override void Gain(sbyte percent)
		{
			// 0 to 99 is loss, 101 - 200 is gain
			gain = percent;
			gain += 100;

			if (gain > 200)
				gain = 200;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Volume(byte num, byte level)
		{
			if (mixer.Obj != null)
				mixer.Obj.Volume(num, level);
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Write16(ushort addr, byte data)
		{
			Write(addr, data);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Emulation(SidIPtr<ISidEmulation> sid)
		{
			mixer = new SidLazyIPtr<ISidMixer>(sid);
			this.sid = new SidLazyIPtr<ISidEmulation>(sid);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ISidUnknown Emulation()
		{
			return sid;
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override byte ReadMemByte(ushort addr)
		{
			byte data = env.ReadMemRamByte(addr);
			env.Sid2Crc(data);

			return data;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void WriteMemByte(byte data)
		{
			sid.Obj.Write(0x18, data);
		}
		#endregion
	}
}
