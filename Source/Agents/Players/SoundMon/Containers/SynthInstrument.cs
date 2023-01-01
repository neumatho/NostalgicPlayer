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
		public byte WaveTable;
		public ushort WaveLength;
		public byte AdsrControl;
		public byte AdsrTable;
		public ushort AdsrLength;
		public byte AdsrSpeed;
		public byte LfoControl;
		public byte LfoTable;
		public byte LfoDepth;
		public ushort LfoLength;
		public byte LfoDelay;
		public byte LfoSpeed;
		public byte EgControl;
		public byte EgTable;
		public ushort EgLength;
		public byte EgDelay;
		public byte EgSpeed;
		public byte FxControl;
		public byte FxSpeed;
		public byte FxDelay;
		public byte ModControl;
		public byte ModTable;
		public byte ModSpeed;
		public byte ModDelay;
		public ushort ModLength;
	}
}
