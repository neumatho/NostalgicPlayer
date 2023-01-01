/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.GameMusicCreator.Containers
{
	/// <summary>
	/// The different effects
	/// </summary>
	internal enum Effect : byte
	{
		None,				// 0
		SlideUp,			// 1
		SlideDown,			// 2
		SetVolume,			// 3
		PatternBreak,		// 4
		PositionJump,		// 5
		EnableFilter,		// 6
		DisableFilter,		// 7
		SetSpeed,			// 8
	}
}
