/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Imp;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2
{
	/// <summary>
	/// Null SID emulation
	/// </summary>
	internal class NullSid : ICoEmulation, ISidEmulation, ICoAggregate, ISidMixer
	{
		#region CoEmulation class implementation
		private class MyCoEmulation : CoEmulation
		{
			private readonly NullSid parent;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public MyCoEmulation(NullSid parent, string name, ISidUnknown builder) : base(name, builder)
			{
				this.parent = parent;
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



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public override void Reset(byte volume)
			{
				parent.Reset(volume);
			}
		}
		#endregion

		#region CoAggregate class implementation
		private class MyCoAggregate : CoAggregate
		{
			private readonly NullSid parent;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public MyCoAggregate(NullSid parent, ISidUnknown unknown) : base(unknown)
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

		private readonly MyCoEmulation myCoEmulation;
		private readonly MyCoAggregate myCoAggregate;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NullSid()
		{
			myCoEmulation = new MyCoEmulation(this, "NullSID", null);
			myCoAggregate = new MyCoAggregate(this, IUnknown());
		}

		#region SidUnknown implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ISidUnknown IUnknown()
		{
			return myCoEmulation.IUnknown();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool IQuery(IId iid, out object implementation)
		{
			if (iid == ISidEmulation.IId())
				implementation = this.StaticCast<ISidEmulation>();
			else if (iid == ISidMixer.IId())
				implementation = this.StaticCast<ISidMixer>();
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
		public void Reset()
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public byte Read(byte addr)
		{
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Write(byte addr, byte data)
		{
		}
		#endregion

		#region ISidEmulation implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ISidUnknown Builder()
		{
			return myCoEmulation.Builder();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clock(Sid2Clock clk)
		{
			myCoEmulation.Clock(clk);
		}



		/********************************************************************/
		/// <summary>
		/// Set optimization level
		/// </summary>
		/********************************************************************/
		public void Optimization(byte level)
		{
			myCoEmulation.Optimization(level);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset(byte volume)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int Output(byte bits)
		{
			return 0;
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
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Volume(byte num, byte level)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Gain(sbyte percent)
		{
		}
		#endregion
	}
}
