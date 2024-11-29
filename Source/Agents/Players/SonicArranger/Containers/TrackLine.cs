/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SonicArranger.Containers
{
	/// <summary>
	/// A single line in a track
	/// </summary>
	internal class TrackLine
	{
		public byte Note;
		public byte Instrument;
		public bool DisableSoundTranspose;
		public bool DisableNoteTranspose;
		public byte Arpeggio;
		public Effect Effect;
		public byte EffectArg;
	}
}
