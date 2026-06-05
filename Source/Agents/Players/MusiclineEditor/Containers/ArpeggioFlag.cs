/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers
{
	/// <summary>
	/// Different arpeggio flags
	/// </summary>
	[Flags]
	internal enum ArpeggioFlag : byte
	{
		None = 0,

		Enabled = 1 << 0,
		WillSetNote = 1 << 1,
		UseTable = 1 << 2,
		UseWaveSample = 1 << 3,
		OneStepPending = 1 << 4,
		NoteIsAbsolute = 1 << 5
	}
}
