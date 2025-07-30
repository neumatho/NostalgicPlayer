/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx.Containers
{
	/// <summary>
	/// TFMX-MOD one file format structure
	/// </summary>
	internal class TfmxModHeader
	{
		public uint OffsetToSample { get; set; }
		public uint OffsetToInfo { get; set; }
		public uint Reserved { get; set; }

		// This information is from the info structure
		public int StartSong { get; set; }
		public string Author { get; set; }
		public string Game { get; set; }
		public byte Flag { get; set; }
		public string Title { get; set; }
	}
}
