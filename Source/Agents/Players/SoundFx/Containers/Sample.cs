/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SoundFx.Containers
{
	/// <summary>
	/// Sample information
	/// </summary>
	internal class Sample
	{
		public string Name;
		public sbyte[] SampleAddr;
		public uint Length;
		public ushort Volume;
		public uint LoopStart;
		public uint LoopLength;
	}
}
