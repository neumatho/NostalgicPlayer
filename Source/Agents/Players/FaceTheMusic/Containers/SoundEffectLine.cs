/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.FaceTheMusic.Containers
{
	/// <summary>
	/// Holds information for a single script effect line
	/// </summary>
	internal class SoundEffectLine
	{
		public SoundEffect Effect { get; set; }
		public byte Argument1 { get; set; }
		public ushort Argument2 { get; set; }
	}
}
