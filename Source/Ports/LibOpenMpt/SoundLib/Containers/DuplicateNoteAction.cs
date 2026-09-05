/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// DNA types (Duplicate Note Action)
	/// </summary>
	internal enum DuplicateNoteAction : uint8
	{
		NoteCut = 0,
		NoteOff = 1,
		NoteFade = 2
	}
}
