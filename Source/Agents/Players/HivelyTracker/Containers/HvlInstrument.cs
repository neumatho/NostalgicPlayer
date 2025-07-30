/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.HivelyTracker.Containers
{
	/// <summary>
	/// Holds information about a single instrument
	/// </summary>
	internal class HvlInstrument
	{
		public string Name { get; set; }

		public int Volume { get; set; }
		public int WaveLength { get; set; }
		public HvlEnvelope Envelope { get; } = new HvlEnvelope();

		public int FilterLowerLimit { get; set; }
		public int FilterUpperLimit { get; set; }
		public int FilterSpeed { get; set; }

		public int SquareLowerLimit { get; set; }
		public int SquareUpperLimit { get; set; }
		public int SquareSpeed { get; set; }

		public int VibratoDelay { get; set; }
		public int VibratoDepth { get; set; }
		public int VibratoSpeed { get; set; }

		public int HardCutReleaseFrames { get; set; }
		public bool HardCutRelease { get; set; }

		public HvlPList PlayList { get; } = new HvlPList();
	}
}
