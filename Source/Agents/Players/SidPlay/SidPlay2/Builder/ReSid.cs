/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSid;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSid.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Environment;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Event;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Imp;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Builder
{
	/// <summary>
	/// ReSid wrapper for redefining the filter
	/// </summary>
	internal class ReSid : ICoEmulation, ISidEmulation, ICoAggregate, ISidMixer
	{
		#region CoEmulation class implementation
		private class MyCoEmulation : CoEmulation
		{
			private readonly ReSid parent;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public MyCoEmulation(ReSid parent, string name, ISidUnknown builder) : base(name, builder)
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
			public override void Gain(sbyte percent)
			{
				parent.Gain(percent);
			}



			/********************************************************************/
			/// <summary>
			/// Set optimization level
			/// </summary>
			/********************************************************************/
			public override void Optimization(byte level)
			{
				parent.Optimization(level);
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
			private readonly ReSid parent;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public MyCoAggregate(ReSid parent, ISidUnknown unknown) : base(unknown)
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

		private Sid sid;

		private IEventContext context;
		private EventPhase phase;

		private uint accessClk;
		private int gain;
		private byte optimization;

		private bool status;
		private string errorMessage;
		private bool locked;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ReSid(IReSidBuilder builder)
		{
			myCoEmulation = new MyCoEmulation(this, "ReSID", builder.IUnknown());
			myCoAggregate = new MyCoAggregate(this, IUnknown());

			sid = new Sid();

			context = null;
			phase = EventPhase.ClockPhi1;

			gain = 100;
			optimization = 0;

			status = true;
			locked = false;

			Reset(0);
		}

		#region ISidUnknown implementation
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
			optimization = level;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset(byte volume)
		{
			accessClk = 0;

			sid.Reset();
			sid.Write(0x18, volume);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int Output(byte bits)
		{
			uint cycles = context.GetTime(accessClk, phase);
			accessClk += cycles;

			if (optimization != 0)
			{
				if (cycles != 0)
					sid.Clock((int)cycles);
			}
			else
			{
				while (cycles-- != 0)
					sid.Clock();
			}

			return sid.Output(bits) * gain / 100;
		}
		#endregion

		#region ICoEmulation implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Gain(sbyte percent)
		{
			// 0 to 99 is loss, 101 - 200 is gain
			gain = percent;
			gain += 100;

			if (gain > 200)
				gain = 200;
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
			myCoEmulation.Reset();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public byte Read(byte addr)
		{
			uint cycles = context.GetTime(accessClk, phase);
			accessClk += cycles;

			if (optimization != 0)
			{
				if (cycles != 0)
					sid.Clock((int)cycles);
			}
			else
			{
				while (cycles-- != 0)
					sid.Clock();
			}

			return (byte)sid.Read(addr);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Write(byte addr, byte data)
		{
			uint cycles = context.GetTime(accessClk, phase);
			accessClk += cycles;

			if (optimization != 0)
			{
				if (cycles != 0)
					sid.Clock((int)cycles);
			}
			else
			{
				while (cycles-- != 0)
					sid.Clock();
			}

			sid.Write(addr, data);
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
//			sid.Mute(num, enable);	// Does not exists in ReSID
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Volume(byte num, byte level)
		{
			// Not yet supported
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool IsOk => status;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public string Error => errorMessage;



		/********************************************************************/
		/// <summary>
		/// Set execution environment and lock SID to it
		/// </summary>
		/********************************************************************/
		public bool Lock(IC64Env env)
		{
			if (env == null)
			{
				if (!locked)
					return false;

				locked = false;
				context = null;
			}
			else
			{
				if (locked)
					return false;

				locked = true;
				context = env.Context;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Set the emulated SID model
		/// </summary>
		/********************************************************************/
		public void Model(Sid2Model model)
		{
			if (model == Sid2Model.Mos8580)
				sid.SetChipModel(ChipModel.Mos8580);
			else
				sid.SetChipModel(ChipModel.Mos6581);
		}



		/********************************************************************/
		/// <summary>
		/// Enable filter
		/// </summary>
		/********************************************************************/
		public void Filter(bool enable)
		{
			sid.EnableFilter(enable);
		}



		/********************************************************************/
		/// <summary>
		/// Set new filter definition
		/// </summary>
		/********************************************************************/
		public bool Filter(Spline.FCPoint[] filter)
		{
			Spline.FCPoint[] fc = new Spline.FCPoint[0x802];
			int f0 = 0;
			int points = 0;

			if (filter == null)
			{
				// Select default filter
				sid.FcDefault(fc, out points);
			}
			else
			{
				// Make sure there are enough filter points and they are legal
				points = filter.Length;
				if ((points < 2) || (points > 0x800))
					return false;

				Spline.FCPoint[] fStart = { new Spline.FCPoint(-1, 0) };
				Spline.FCPoint[] fPrev = fStart;
				int fPrevOffset = 0;
				int fin = 0;
				int fOut = 0;

				// Last check, make sure they are list in numerical order
				// for both axis
				while (points-- > 0)
				{
					if (fPrev[fPrevOffset].X >= filter[fin].X)
						return false;

					fOut++;
					fc[fOut].X = filter[fin].X;
					fc[fOut].Y = filter[fin].Y;

					fPrev = filter;
					fPrevOffset = fin++;
				}

				// Updated ReSID interpolate requires we
				// repeat the end points
				fc[fOut + 1].X = fc[fOut].X;
				fc[fOut + 1].Y = fc[fOut].Y;

				fc[0].X = fc[1].X;
				fc[0].Y = fc[1].Y;

				points = filter.Length + 2;
			}

			// Function from ReSID
			points--;
			Spline.Interpolate(fc, f0, points, sid.FcPlotter(), 1.0f);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Sampling(uint freq)
		{
			sid.SetSamplingParameters(1000000, SamplingMethod.Fast, freq);
		}
	}
}
