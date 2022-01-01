﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Ahx.Containers
{
	/// <summary>
	/// Holds information about a single position
	/// </summary>
	internal class AhxPosition
	{
		public int[] Track = new int[4];
		public int[] Transpose = new int[4];
	}
}