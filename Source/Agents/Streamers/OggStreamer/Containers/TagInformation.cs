/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Streamer.OggStreamer.Containers
{
	/// <summary>
	/// Contain information retrieved from tags in the stream
	/// </summary>
	internal class TagInformation
	{
		public string SongName { get; set; }
		public string Artist { get; set; }
		public string TrackNum { get; set; }
		public string Album { get; set; }
		public string Genre { get; set; }
		public string Organization { get; set; }
		public string Copyright { get; set; }
		public string Description { get; set; }
		public string Vendor { get; set; }
	}
}
