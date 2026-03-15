/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.VoodooSupremeSynthesizer.Containers
{
	/// <summary>
	/// Holds information about a single subsong
	/// </summary>
	internal class SongInfo
	{
		/// <summary>
		/// Track data for each voice
		/// </summary>
		public TrackData[] Tracks { get; init; }

		/// <summary>
		/// All the different structures used for this song
		/// </summary>
		public IModuleData[] Data { get; init; }
	}
}
