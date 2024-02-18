/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.DigitalMugician.Containers
{
	/// <summary>
	/// Holds information about a single instrument
	/// </summary>
	internal class Instrument : IDeepCloneable<Instrument>
	{
		public byte WaveformNumber;		// >= 32 -> sample number
		public ushort LoopLength;
		public byte Finetune;
		public byte ArpeggioNumber;
		public byte Volume;
		public byte VolumeSpeed;
		public bool VolumeLoop;
		public byte Pitch;
		public byte PitchSpeed;
		public byte PitchLoop;
		public byte Delay;
		public InstrumentEffect Effect;
		public byte EffectSpeed;
		public byte EffectIndex;
		public byte SourceWave1;
		public byte SourceWave2;

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
