/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Hippel.Containers
{
	/// <summary>
	/// Holds information for a single voice position
	/// </summary>
	internal class SinglePositionInfo
	{
		public byte Track;
		public sbyte NoteTranspose;
		public sbyte EnvelopeTranspose;
		public byte Command;
	}
}
