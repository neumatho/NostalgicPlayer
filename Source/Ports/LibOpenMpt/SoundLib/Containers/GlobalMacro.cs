/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Global macro types
	/// </summary>
	internal static class GlobalMacro
	{
		public const c_int MidiOut_Start = 0;
		public const c_int MidiOut_Stop = 1;
		public const c_int MidiOut_Tick = 2;
		public const c_int MidiOut_NoteOn = 3;
		public const c_int MidiOut_NoteOff = 4;
		public const c_int MidiOut_Volume = 5;
		public const c_int MidiOut_Pan = 6;
		public const c_int MidiOut_BankSel = 7;
		public const c_int MidiOut_Program = 8;
	}
}
