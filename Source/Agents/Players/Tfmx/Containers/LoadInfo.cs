/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx.Containers
{
	/// <summary>
	/// Holds needed information to load the module
	/// </summary>
	internal class LoadInfo
	{
		public int ModuleStartOffset { get; set; }
		public int ModuleSize { get; set; }
		public int SampleStartOffset { get; set; }		// Is -1 if two files
		public int SampleSize { get; set; }

		public int StartSong { get; set; }
		public string ModuleName { get; set; }
		public string Author { get; set; }
	}
}
