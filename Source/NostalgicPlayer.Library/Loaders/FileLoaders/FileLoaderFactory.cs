/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Helpers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Library.Loaders.FileDecrunchers;

namespace Polycode.NostalgicPlayer.Library.Loaders.FileLoaders
{
	/// <summary>
	/// Factory to create file loaders
	/// </summary>
	internal class FileLoaderFactory : IFileLoaderFactory
	{
		private readonly FileDecruncherFactory fileDecruncherFactory;
		private readonly ArchiveDecruncherFactory archiveDecruncherFactory;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FileLoaderFactory(FileDecruncherFactory fileDecruncherFactory, ArchiveDecruncherFactory archiveDecruncherFactory)
		{
			this.fileDecruncherFactory = fileDecruncherFactory;
			this.archiveDecruncherFactory = archiveDecruncherFactory;
		}



		/********************************************************************/
		/// <summary>
		/// Will parse the file name path and return the loader to use
		/// </summary>
		/********************************************************************/
		public ILoader CreateFileLoader(string fileName)
		{
			if (ArchivePath.IsArchivePath(fileName))
				return new ArchiveFileLoader(fileName, fileDecruncherFactory, archiveDecruncherFactory);

			return new NormalFileLoader(fileName, fileDecruncherFactory);
		}
	}
}
