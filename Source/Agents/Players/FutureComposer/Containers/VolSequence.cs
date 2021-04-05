/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.FutureComposer.Containers
{
	/// <summary>
	/// Volume sequence structure
	/// </summary>
	internal class VolSequence
	{
		public byte Speed;
		public byte FrqNumber;
		public sbyte VibSpeed;
		public sbyte VibDepth;
		public byte VibDelay;
		public byte[] Values = new byte[59];
	}
}
