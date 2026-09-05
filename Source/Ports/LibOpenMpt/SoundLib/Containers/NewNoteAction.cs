/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// NNA types (New Note Action)
	/// </summary>
	internal enum NewNoteAction : uint8
	{
		NoteCut = 0,
		Continue = 1,
		NoteOff = 2,
		NoteFade = 3
	}
}
