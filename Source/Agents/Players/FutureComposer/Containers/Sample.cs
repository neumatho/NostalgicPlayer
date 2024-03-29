﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.FutureComposer.Containers
{
	/// <summary>
	/// Sample information structure
	/// </summary>
	internal class Sample
	{
		public short SampleNumber;
		public sbyte[] Address;
		public ushort Length;
		public ushort LoopStart;
		public ushort LoopLength;
		public MultiSample Multi;
	}
}
