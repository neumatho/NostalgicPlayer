/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx.Containers
{
	/// <summary>
	/// </summary>
	internal class Pdb
	{
		public uint PAddr;
		public byte PNum;
		public sbyte PxPose;
		public ushort PLoop;
		public ushort PStep;
		public byte PWait;
		public uint PrOAddr;
		public ushort PrOStep;
		public bool Looped;
	}
}
