/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SoundMon.Containers
{
	/// <summary>
	/// Holds information about a synthesis instrument
	/// </summary>
	internal class SynthInstrument : Instrument
	{
		public byte WaveTable { get; set; }
		public ushort WaveLength { get; set; }
		public byte AdsrControl { get; set; }
		public byte AdsrTable { get; set; }
		public ushort AdsrLength { get; set; }
		public byte AdsrSpeed { get; set; }
		public byte LfoControl { get; set; }
		public byte LfoTable { get; set; }
		public byte LfoDepth { get; set; }
		public ushort LfoLength { get; set; }
		public byte LfoDelay { get; set; }
		public byte LfoSpeed { get; set; }
		public byte EgControl { get; set; }
		public byte EgTable { get; set; }
		public ushort EgLength { get; set; }
		public byte EgDelay { get; set; }
		public byte EgSpeed { get; set; }
		public byte FxControl { get; set; }
		public byte FxSpeed { get; set; }
		public byte FxDelay { get; set; }
		public byte ModControl { get; set; }
		public byte ModTable { get; set; }
		public byte ModSpeed { get; set; }
		public byte ModDelay { get; set; }
		public ushort ModLength { get; set; }
	}
}
