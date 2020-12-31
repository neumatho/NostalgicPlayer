/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of RetroPlayer is keep. See the LICENSE file for more information. */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / RetroPlayer team.                         */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.RetroPlayer.RetroPlayerKit.Containers
{
	/// <summary>
	/// Results used by the agents
	/// </summary>
	public enum AgentResult
	{
		/// <summary>
		/// Everything is fine
		/// </summary>
		Ok,

		/// <summary>
		/// Some error occurred
		/// </summary>
		Error,

		/// <summary>
		/// Unknown result. Used when checking file formats
		/// </summary>
		Unknown
	}
}
