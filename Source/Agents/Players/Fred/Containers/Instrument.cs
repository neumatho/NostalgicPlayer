/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Fred.Containers
{
	/// <summary>
	/// Holds information about a single instrument
	/// </summary>
	internal class Instrument
	{
		public short InstrumentNumber { get; set; }

		public string Name { get; set; }
		public ushort RepeatLen { get; set; }
		public ushort Length { get; set; }
		public ushort Period { get; set; }
		public byte VibDelay { get; set; }
		public sbyte VibSpeed { get; set; }
		public sbyte VibAmpl { get; set; }
		public byte EnvVol { get; set; }
		public byte AttackSpeed { get; set; }
		public byte AttackVolume { get; set; }
		public byte DecaySpeed { get; set; }
		public byte DecayVolume { get; set; }
		public byte SustainDelay { get; set; }
		public byte ReleaseSpeed { get; set; }
		public byte ReleaseVolume { get; set; }
		public sbyte[] Arpeggio { get; }= new sbyte[16];
		public byte ArpSpeed { get; set; }
		public InstrumentType InstType { get; set; }
		public sbyte PulseRateMin { get; set; }
		public sbyte PulseRatePlus { get; set; }
		public byte PulseSpeed { get; set; }
		public byte PulseStart { get; set; }
		public byte PulseEnd { get; set; }
		public byte PulseDelay { get; set; }
		public SynchronizeFlag InstSync { get; set; }
		public byte Blend { get; set; }
		public byte BlendDelay { get; set; }
		public byte PulseShotCounter { get; set; }
		public byte BlendShotCounter { get; set; }
		public byte ArpCount { get; set; }

		public sbyte[] SampleAddr { get; set; }
	}
}
