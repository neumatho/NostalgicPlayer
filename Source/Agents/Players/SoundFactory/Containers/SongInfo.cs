/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SoundFactory.Containers
{
	/// <summary>
	/// Holds information about a single song
	/// </summary>
	internal class SongInfo
	{
		public byte EnabledChannels { get; set; }
		public uint[] OpcodeStartOffsets { get; } = new uint[4];
	}
}
