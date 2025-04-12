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
		public uint OffsetToSample;
		public uint OffsetToInfo;
		public uint Reserved;

		// This information is from the info structure
		public int StartSong;
		public string Author;
		public string Game;
		public byte Flag;
		public string Title;
	}
}
