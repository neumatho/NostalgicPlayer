/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.JamCracker.Containers
{
	/// <summary>
	/// Pattern info structure
	/// </summary>
	internal class PattInfo
	{
		public ushort Size { get; set; }
		public NoteInfo[] Address { get; set; }
	}
}
