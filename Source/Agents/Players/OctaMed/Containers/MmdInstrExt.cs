/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers
{
	/// <summary>
	/// MMD instrument extension structure
	/// </summary>
	internal class MmdInstrExt
	{
		public byte DefaultPitch { get; set; }
		public InstrFlag InstrFlags { get; set; }
		public ushort LongMidiPreset { get; set; }
		public byte OutputDevice { get; set; }
		public byte Reserved { get; set; }
		public uint LongRepeat { get; set; }
		public uint LongRepLen { get; set; }
	}
}
