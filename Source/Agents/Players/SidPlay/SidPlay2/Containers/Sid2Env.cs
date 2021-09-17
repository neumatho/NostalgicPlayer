/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers
{
	/// <summary>
	/// Different environments
	/// </summary>
	internal enum Sid2Env
	{
		/// <summary>
		/// PlaySid
		/// </summary>
		EnvPs = 0,

		/// <summary>
		/// SidPlay - Transparent ROM
		/// </summary>
		EnvTp,

		/// <summary>
		/// SidPlay - Bank switching
		/// </summary>
		EnvBs,

		/// <summary>
		/// SidPlay2 - Real C64 environment
		/// </summary>
		EnvR,

		/// <summary>
		/// Sidusage Tracker Mode
		/// </summary>
		EnvTr
	}
}
