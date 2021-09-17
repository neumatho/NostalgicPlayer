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
	/// CIA 2 specifics: Generates NMIs
	/// </summary>
	internal class C64Cia2 : Mos6526.Mos6526
	{
		private readonly IC64Env env;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public C64Cia2(IC64Env env) : base(env.Context)
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
			if (state)
				env.InterruptNmi();
		}
		#endregion
	}
}
