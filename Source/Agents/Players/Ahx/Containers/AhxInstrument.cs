/******************************************************************************/
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
	/// Holds information about a single instrument
	/// </summary>
	internal class AhxInstrument
	{
		public string Name;

		public int Volume;
		public int WaveLength;
		public AhxEnvelope Envelope = new AhxEnvelope();

		public int FilterLowerLimit;
		public int FilterUpperLimit;
		public int FilterSpeed;

		public int SquareLowerLimit;
		public int SquareUpperLimit;
		public int SquareSpeed;

		public int VibratoDelay;
		public int VibratoDepth;
		public int VibratoSpeed;

		public int HardCutReleaseFrames;
		public bool HardCutRelease;

		public AhxPList PlayList = new AhxPList();
	}
}
