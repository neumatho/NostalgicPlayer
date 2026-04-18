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
		private readonly IFileDecruncherFactory fileDecruncherFactory;
		private readonly IArchiveDecruncherFactory archiveDecruncherFactory;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FileLoaderFactory(IFileDecruncherFactory fileDecruncherFactory, IArchiveDecruncherFactory archiveDecruncherFactory)
		{
			this.fileDecruncherFactory = fileDecruncherFactory;
			this.archiveDecruncherFactory = archiveDecruncherFactory;
		}



		/********************************************************************/
		/// <summary>
		/// Will parse the file name path and return the loader to use
		/// </summary>
		/********************************************************************/
		public ILoader GetFileLoader(string fileName)
		{
			if (ArchivePath.IsArchivePath(fileName))
				return new ArchiveFileLoader(fileName, fileDecruncherFactory, archiveDecruncherFactory);

			return new NormalFileLoader(fileName, fileDecruncherFactory);
		}
	}
}
