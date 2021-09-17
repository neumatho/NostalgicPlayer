/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Environment;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.C64
{
	/// <summary>
	/// C64 VIC
	/// </summary>
	internal class C64Vic : Mos656x.Mos656x
	{
		private readonly IC64Env env;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public C64Vic(IC64Env env) : base(env.Context)
		{
			this.env = env;
		}

		#region Mos656x implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void Interrupt(bool state)
		{
			env.InterruptIrq(state);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void AddrCtrl(bool state)
		{
			env.SignalAec(state);
		}
		#endregion
	}
}
