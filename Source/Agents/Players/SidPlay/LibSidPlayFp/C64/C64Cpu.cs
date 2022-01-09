/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Cpu;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64
{
	/// <summary>
	/// CPU emulator
	/// </summary>
	internal sealed class C64Cpu : Mos6510
	{
		private readonly C64Env env;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public C64Cpu(C64Env env) : base(env.Scheduler())
		{
			this.env = env;
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override uint8_t CpuRead(uint_least16_t addr)
		{
			return env.CpuRead(addr);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void CpuWrite(uint_least16_t addr, uint8_t data)
		{
			env.CpuWrite(addr, data);
		}
		#endregion
	}
}
