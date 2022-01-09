/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SoundMon.Containers
{
	/// <summary>
	/// Holds information about sample instruments
	/// </summary>
	internal class SampleInstrument : Instrument
	{
		public string Name;					// Sample name
		public ushort Length;				// Length of sample
		public ushort LoopStart;			// Offset to loop start
		public ushort LoopLength;			// Loop length
		public sbyte[] Adr;					// Sample data
	}
}
