/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DeltaMusic20.Containers
{
	/// <summary>
	/// Holds information about a single instrument
	/// </summary>
	internal class Instrument
	{
		public short Number;

		public ushort SampleLength;
		public ushort RepeatStart;
		public ushort RepeatLength;
		public readonly VolumeInfo[] VolumeTable = new VolumeInfo[5];
		public readonly VibratoInfo[] VibratoTable = new VibratoInfo[5];
		public ushort PitchBend;
		public bool IsSample;
		public byte SampleNumber;
		public readonly byte[] Table = new byte[48];

		public sbyte[] SampleData;
	}
}
