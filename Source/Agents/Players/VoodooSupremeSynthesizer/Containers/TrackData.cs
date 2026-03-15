/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.VoodooSupremeSynthesizer.Containers
{
	/// <summary>
	/// Contains the bytes for a single track
	/// </summary>
	internal class TrackData : IModuleData
	{
		/// <summary>
		/// The track data
		/// </summary>
		public byte[] Track { get; init; }
	}
}
