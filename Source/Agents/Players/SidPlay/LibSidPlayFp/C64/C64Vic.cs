/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Banks;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Vic_II;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64
{
	/// <summary>
	/// VIC-II
	///
	/// Located at $D000-$D3FF
	/// </summary>
	internal sealed class C64Vic : Mos656x, IBank
	{
		private readonly C64Env env;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public C64Vic(C64Env env) : base(env.Scheduler())
		{
			this.env = env;
		}

		#region IBank implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Poke(uint_least16_t address, uint8_t value)
		{
			Write(SidEndian.Endian_16Lo8(address), value);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint8_t Peek(uint_least16_t address)
		{
			return Read(SidEndian.Endian_16Lo8(address));
		}
		#endregion

		#region Overrides
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
		protected override void SetBa(bool state)
		{
			env.SetBa(state);
		}
		#endregion
	}
}
