/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SoundControl.Players.Containers
{
	/// <summary>
	/// Holds information about a module.
	/// Only used by the SoundControl 3.x player
	/// </summary>
	internal class ModuleInfo3x
	{
		public bool IsVersion32 { get; set; }
		public SongInfo3x[] SongInfoList { get; set; }
	}
}
