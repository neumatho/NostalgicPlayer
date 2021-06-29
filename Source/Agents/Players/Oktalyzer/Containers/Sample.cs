/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Oktalyzer.Containers
{
	/// <summary>
	/// Sample structure
	/// </summary>
	internal class Sample
	{
		public string Name;
		public uint Length;
		public ushort RepeatStart;
		public ushort RepeatLength;
		public ushort Volume;
		public ushort Mode;						// 0 = 8, 1 = 4, 2 = B
		public sbyte[] SampleData;
	}
}
