/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.GameMusicCreator.Containers
{
	/// <summary>
	/// A single pattern
	/// </summary>
	internal class Pattern
	{
		public TrackLine[,] Tracks = new TrackLine[4, 64];		// All the channel tracks
	}
}
