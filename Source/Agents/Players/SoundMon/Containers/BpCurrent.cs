/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SoundMon.Containers
{
	/// <summary>
	/// BpCurrent structure
	/// </summary>
	internal class BpCurrent : IDeepCloneable<BpCurrent>
	{
		public bool Restart { get; set; }
		public bool UseDefaultVolume { get; set; }
		public bool SynthMode { get; set; }
		public int SynthOffset { get; set; }
		public ushort Period { get; set; }
		public byte Volume { get; set; }
		public byte Instrument { get; set; }
		public byte Note { get; set; }
		public byte ArpValue { get; set; }
		public sbyte AutoSlide { get; set; }
		public byte AutoArp { get; set; }
		public ushort EgPtr { get; set; }
		public ushort LfoPtr { get; set; }
		public ushort AdsrPtr { get; set; }
		public ushort ModPtr { get; set; }
		public byte EgCount { get; set; }
		public byte LfoCount { get; set; }
		public byte AdsrCount { get; set; }
		public byte ModCount { get; set; }
		public byte FxCount { get; set; }
		public byte OldEgValue { get; set; }
		public byte EgControl { get; set; }
		public byte LfoControl { get; set; }
		public byte AdsrControl { get; set; }
		public byte ModControl { get; set; }
		public byte FxControl { get; set; }
		public sbyte Vibrato { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public BpCurrent MakeDeepClone()
		{
			return (BpCurrent)MemberwiseClone();
		}
	}
}
