/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.FaceTheMusic.Containers
{
	/// <summary>
	/// Holds information about a single track line
	/// </summary>
	internal class TrackLine
	{
		public TrackEffect Effect { get; set; }
		public ushort EffectArgument { get; set; }
		public byte Note { get; set; }
	}
}
