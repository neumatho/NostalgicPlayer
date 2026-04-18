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
	internal class ArchiveDecruncherFactory : IArchiveDecruncherFactory
	{
		private readonly IArchiveDetector archiveDetector;
		private readonly IFileDecruncherFactory fileDecruncherFactory;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ArchiveDecruncherFactory(IArchiveDetector archiveDetector, IFileDecruncherFactory fileDecruncherFactory)
		{
			this.archiveDetector = archiveDetector;
			this.fileDecruncherFactory = fileDecruncherFactory;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new instance of the archive file decruncher
		/// </summary>
		/********************************************************************/
		public ArchiveFileDecruncher GetArchiveFileDecruncher()
		{
			return new ArchiveFileDecruncher(archiveDetector, fileDecruncherFactory);
		}
	}
}
