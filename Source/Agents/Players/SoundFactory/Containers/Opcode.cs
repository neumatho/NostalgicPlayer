/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SoundFactory.Containers
{
	/// <summary>
	/// All the different opcodes
	/// </summary>
	internal enum Opcode : byte
	{
		Pause = 0x80,
		SetVolume = 0x81,
		SetFineTune = 0x82,
		UseInstrument = 0x83,
		DefineInstrument = 0x84,
		Return = 0x85,
		GoSub = 0x86,
		Goto = 0x87,
		For = 0x88,
		Next = 0x89,
		FadeOut = 0x8a,
		Nop = 0x8b,
		Request = 0x8c,
		Loop = 0x8d,
		End = 0x8e,
		FadeIn = 0x8f,
		SetAdsr = 0x90,
		OneShot = 0x91,
		Looping = 0x92,
		Vibrato = 0x93,
		Arpeggio = 0x94,
		Phasing = 0x95,
		Portamento = 0x96,
		Tremolo = 0x97,
		Filter = 0x98,
		StopAndPause = 0x99,
		Led = 0x9a,
		WaitForRequest = 0x9b,
		SetTranspose = 0x9c
	}
}
