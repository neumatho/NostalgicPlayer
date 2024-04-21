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
		public string Name;

		public int Restart;
		public int PositionNr;
		public int TrackLength;
		public int TrackNr;
		public int InstrumentNr;

		public int SpeedMultiplier;
		public int Channels;

		public int MixGain;
		public int DefaultStereo;
		public int DefaultPanningLeft;
		public int DefaultPanningRight;

		public HvlPosition[] Positions;
		public HvlStep[][] Tracks;
		public HvlInstrument[] Instruments;
	}
}
