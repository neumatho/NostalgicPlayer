/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Hippel.Containers
{
	/// <summary>
	/// Holds information about a single song
	/// </summary>
	internal class SongInfo
	{
		public ushort StartPosition { get; set; }
		public ushort LastPosition { get; set; }
		public ushort StartSpeed { get; set; }
	}
}
