/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers
{
	/// <summary>
	/// MMD1 block info structure
	/// </summary>
	internal class Mmd1BlockInfo
	{
		public uint HlMask;
		public uint BlockName;
		public uint BlockNameLen;
		public uint PageTable;
		public uint CmdExtTable;
	}
}
