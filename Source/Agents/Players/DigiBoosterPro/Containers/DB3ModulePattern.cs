/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// Complete pattern
	/// </summary>
	internal class DB3ModulePattern
	{
		public uint16_t NumberOfRows;
		public DB3ModuleEntry[] Pattern;		// A table
	}
}
