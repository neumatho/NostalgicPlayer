/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.QuadraComposer.Containers
{
	/// <summary>
	/// Sample structure
	/// </summary>
	internal class Sample
	{
		public string Name;			// The name of the sample
		public uint Length;			// Length
		public uint LoopStart;		// Loop start offset
		public uint LoopLength;		// Loop length
		public byte Volume;			// The volume (0-64)
		public SampleControlFlag ControlByte;
		public byte FineTune;		// Fine tune (0-15)
		public sbyte[] Data;		// Sample data
	}
}
