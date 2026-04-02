/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Helpers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Library.Containers;
using Polycode.NostalgicPlayer.Library.Loaders.FileDecrunchers;

namespace Polycode.NostalgicPlayer.Library.Loaders.FileLoaders
{
	/// <summary>
	/// This loader can open files in an archive
	/// </summary>
	internal class ArchiveFileLoader : FileLoaderBase
	{
		private readonly FileDecruncherFactory fileDecruncherFactory;
		private readonly ArchiveDecruncherFactory archiveDecruncherFactory;

		private ArchiveFileDecruncher archiveFileDecruncher;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ArchiveFileLoader(string fileName, FileDecruncherFactory fileDecruncherFactory, ArchiveDecruncherFactory archiveDecruncherFactory) : base(fileName)
		{
			this.fileDecruncherFactory = fileDecruncherFactory;
			this.archiveDecruncherFactory = archiveDecruncherFactory;
		}



		/********************************************************************/
		/// <summary>
		/// Free allocated stuff
		/// </summary>
		/********************************************************************/
		public override void Dispose()
		{
			archiveFileDecruncher?.Dispose();
			archiveFileDecruncher = null;

			base.Dispose();
		}

		#region FileLoaderBase implementation
		/********************************************************************/
		/// <summary>
		/// Will try to open the main file. You need to dispose the returned
		/// stream when done
		/// </summary>
		/********************************************************************/
		public override Stream OpenFile()
		{
			archiveFileDecruncher = archiveDecruncherFactory.CreateArchiveFileDecruncher();
			archiveFileDecruncher.OpenArchive(FullPath);

			ArchiveEntryInfo entryInfo = archiveFileDecruncher.OpenArchiveEntry(ArchivePath.GetEntryName(FullPath));

			CrunchedSize = entryInfo.CrunchedSize;
			ModuleSize = entryInfo.DecrunchedSize;

			DecruncherAlgorithms = archiveFileDecruncher.DecruncherAlgorithms;

			return entryInfo.EntryStream;
		}



		/********************************************************************/
		/// <summary>
		/// Will try to open the extra file and return the stream and some
		/// info about the before and after lengths
		/// </summary>
		/********************************************************************/
		protected override ModuleStream OpenStream(string fullFileName, out StreamInfo streamInfo)
		{
			streamInfo = new StreamInfo();

			ArchiveEntryInfo entryInfo = TryOpenFile(fullFileName);
			if (entryInfo == null)
				return null;

			// Decrunch it if needed
			streamInfo.CrunchedSize = entryInfo.CrunchedSize;

			SingleFileDecruncher decruncher = fileDecruncherFactory.CreateSingleFileDecruncher();
			Stream stream = decruncher.DecrunchFileMultipleLevels(entryInfo.EntryStream);

			streamInfo.DecrunchedSize = stream.Length;

			return new ModuleStream(stream, false);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will try to open the file given and return null if it could not
		/// be opened
		/// </summary>
		/********************************************************************/
		private ArchiveEntryInfo TryOpenFile(string newFileName)
		{
			try
			{
				return archiveFileDecruncher.OpenArchiveEntry(ArchivePath.GetEntryName(newFileName));
			}
			catch (Exception)
			{
				return null;
			}
		}
		#endregion
	}
}
