/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.DigitalMugician.Containers
{
	/// <summary>
	/// Holds information about a single instrument
	/// </summary>
	internal class Instrument : IDeepCloneable<Instrument>
	{
		public byte WaveformNumber { get; set; }		// >= 32 -> sample number
		public ushort LoopLength { get; set; }
		public byte Finetune { get; set; }
		public byte ArpeggioNumber { get; set; }
		public byte Volume { get; set; }
		public byte VolumeSpeed { get; set; }
		public bool VolumeLoop { get; set; }
		public byte Pitch { get; set; }
		public byte PitchSpeed { get; set; }
		public byte PitchLoop { get; set; }
		public byte Delay { get; set; }
		public InstrumentEffect Effect { get; set; }
		public byte EffectSpeed { get; set; }
		public byte EffectIndex { get; set; }
		public byte SourceWave1 { get; set; }
		public byte SourceWave2 { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Instrument MakeDeepClone()
		{
			return (Instrument)MemberwiseClone();
		}
	}
}
