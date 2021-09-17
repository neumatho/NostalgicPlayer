/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Environment
{
	/// <summary>
	/// Interface to C64Environment class
	/// </summary>
	internal interface IC64Environment
	{
		/// <summary>
		/// Initialize the environment
		/// </summary>
		void SetEnvironment(C64Environment env);
	}
}
