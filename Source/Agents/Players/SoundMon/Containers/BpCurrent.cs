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
		public bool Restart;
		public bool UseDefaultVolume;
		public bool SynthMode;
		public int SynthOffset;
		public ushort Period;
		public byte Volume;
		public byte Instrument;
		public byte Note;
		public byte ArpValue;
		public sbyte AutoSlide;
		public byte AutoArp;
		public ushort EgPtr;
		public ushort LfoPtr;
		public ushort AdsrPtr;
		public ushort ModPtr;
		public byte EgCount;
		public byte LfoCount;
		public byte AdsrCount;
		public byte ModCount;
		public byte FxCount;
		public byte OldEgValue;
		public byte EgControl;
		public byte LfoControl;
		public byte AdsrControl;
		public byte ModControl;
		public byte FxControl;
		public sbyte Vibrato;

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
