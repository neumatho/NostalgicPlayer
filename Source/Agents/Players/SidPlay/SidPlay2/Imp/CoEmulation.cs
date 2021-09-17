/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Imp
{
	/// <summary>
	/// Inherit this class to create a new SID emulation
	/// </summary>
	internal abstract class CoEmulation : CoComponent, ICoEmulation
	{
		private readonly ISidUnknown builder;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected CoEmulation(string name, ISidUnknown builder) : base(name)
		{
			this.builder = builder;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			Reset(0);
		}

		#region ICoEmulation implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ISidUnknown Builder()
		{
			return builder;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clock(Sid2Clock clk)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual void Gain(sbyte percent)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Set optimization level
		/// </summary>
		/********************************************************************/
		public virtual void Optimization(byte level)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract void Reset(byte volume);
		#endregion
	}
}
