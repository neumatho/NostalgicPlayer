/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Library.Loaders.FileDecrunchers
{
	/// <summary>
	/// Factory class to create archive decruncher instances
	/// </summary>
	internal interface IArchiveDecruncherFactory
	{
		/// <summary>
		/// Create a new instance of the archive file decruncher
		/// </summary>
		ArchiveFileDecruncher GetArchiveFileDecruncher();
	}
}
