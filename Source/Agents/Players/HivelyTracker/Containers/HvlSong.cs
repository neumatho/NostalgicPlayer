/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.HivelyTracker.Containers
{
	/// <summary>
	/// Holds general song information
	/// </summary>
	internal class HvlSong
	{
		public string Name { get; set; }

		public int Restart { get; set; }
		public int PositionNr { get; set; }
		public int TrackLength { get; set; }
		public int TrackNr { get; set; }
		public int InstrumentNr { get; set; }

		public int SpeedMultiplier { get; set; }
		public int Channels { get; set; }

		public int MixGain { get; set; }
		public int DefaultStereo { get; set; }
		public int DefaultPanningLeft { get; set; }
		public int DefaultPanningRight { get; set; }

		public HvlPosition[] Positions { get; set; }
		public HvlStep[][] Tracks { get; set; }
		public HvlInstrument[] Instruments { get; set; }
	}
}
