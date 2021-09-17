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
	/// CIA 1 specifics: Generates IRQs
	/// </summary>
	internal class C64Cia1 : Mos6526.Mos6526
	{
		private readonly IC64Env env;
		private byte lp;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public C64Cia1(IC64Env env) : base(env.Context)
		{
			this.env = env;
		}

		#region Mos6525 implementation
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
		protected override void PortB()
		{
			byte lp = (byte)((regs[Prb] | ~regs[Ddrb]) & 0x10);

			if (lp != this.lp)
				env.Lightpen();

			this.lp = lp;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override void Reset()
		{
			lp = 0x10;
			base.Reset();
		}
		#endregion
	}
}
