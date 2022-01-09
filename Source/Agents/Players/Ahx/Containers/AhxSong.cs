/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Ahx.Containers
{
	/// <summary>
	/// Holds general song information
	/// </summary>
	internal class AhxSong
	{
		public string Name;

		public int Restart;
		public int PositionNr;
		public int TrackLength;
		public int TrackNr;
		public int InstrumentNr;
		public int SubSongNr;

		public int Revision;
		public int SpeedMultiplier;

		public AhxPosition[] Positions;
		public AhxStep[][] Tracks;
		public AhxInstrument[] Instruments;
		public int[] SubSongs;
	}
}
