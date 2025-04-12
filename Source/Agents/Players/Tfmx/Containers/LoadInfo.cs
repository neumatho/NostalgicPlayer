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
		public int ModuleStartOffset;
		public int ModuleSize;
		public int SampleStartOffset;       // Is -1 if two files
		public int SampleSize;

		public int StartSong;
		public string ModuleName;
		public string Author;
	}
}
