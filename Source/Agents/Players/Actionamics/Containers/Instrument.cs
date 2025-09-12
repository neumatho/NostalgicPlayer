/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Actionamics.Containers
{
	/// <summary>
	/// Holds information about a single instrument
	/// </summary>
	internal class Instrument
	{
		public InstrumentList SampleNumberList { get; set; }
		public InstrumentList ArpeggioList { get; set; }
		public InstrumentList FrequencyList { get; set; }

		public sbyte PortamentoIncrement { get; set; }
		public byte PortamentoDelay { get; set; }

		public sbyte NoteTranspose { get; set; }

		public byte AttackEndVolume { get; set; }
		public byte AttackSpeed { get; set; }
		public byte DecayEndVolume { get; set; }
		public byte DecaySpeed { get; set; }
		public byte SustainDelay { get; set; }
		public byte ReleaseEndVolume { get; set; }
		public byte ReleaseSpeed { get; set; }
	}
}
