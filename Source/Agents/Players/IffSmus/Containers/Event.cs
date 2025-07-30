/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.IffSmus.Containers
{
	/// <summary>
	/// Holds information about a single track event
	/// </summary>
	internal class Event
	{
		public EventType Type { get; set; }
		public byte Data { get; set; }
	}
}
