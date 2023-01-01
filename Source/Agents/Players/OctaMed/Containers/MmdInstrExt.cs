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
		public byte DefaultPitch;
		public InstrFlag InstrFlags;
		public ushort LongMidiPreset;
		public byte OutputDevice;
		public byte Reserved;
		public uint LongRepeat;
		public uint LongRepLen;
	}
}
