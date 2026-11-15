/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SoundMon.Containers
{
	/// <summary>
	/// BpCurrent structure
	/// </summary>
	internal class BpCurrent : IDeepCloneable<BpCurrent>
	{
		public bool Restart { get; set; }				// Bit 15 in the period in the original player
		public bool DmaRestart { get; set; }			// Voice bit in the DMA mask in the original player
		public bool SynthMode { get; set; }
		public int SynthOffset { get; set; }			// Offset to the waveform saved in the synth buffer. -1 means no buffer
		public ushort Period { get; set; }
		public byte Volume { get; set; }				// 0xff means use the default volume
		public byte Instrument { get; set; }
		public byte Note { get; set; }
		public byte ArpValue { get; set; }
		public sbyte AutoSlide { get; set; }
		public byte AutoArp { get; set; }
		public ushort PlayLength { get; set; }			// Repeat length in words of what is playing
		public ushort EgPtr { get; set; }
		public ushort LfoPtr { get; set; }
		public ushort AdsrPtr { get; set; }
		public ushort ModPtr { get; set; }
		public ushort EgCount { get; set; }
		public ushort LfoCount { get; set; }
		public ushort AdsrCount { get; set; }
		public byte ModCount { get; set; }
		public byte FxCount { get; set; }
		public byte OldEgValue { get; set; }
		public byte EgControl { get; set; }
		public byte LfoControl { get; set; }
		public byte AdsrControl { get; set; }
		public byte ModControl { get; set; }
		public byte FxControl { get; set; }
		public byte Vibrato { get; set; }

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
