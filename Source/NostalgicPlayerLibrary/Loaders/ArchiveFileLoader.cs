/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Loaders
{
	/// <summary>
	/// This loader can open files in an archive
	/// </summary>
	public class ArchiveFileLoader : FileLoaderBase
	{
		private ArchiveFileDecruncher archiveFileDecruncher;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ArchiveFileLoader(string fileName, Manager agentManager) : base(fileName, agentManager)
		{
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



		/********************************************************************/
		/// <summary>
		/// Will try to open the main file. You need to dispose the returned
		/// stream when done
		/// </summary>
		/********************************************************************/
		public override Stream OpenFile()
		{
			archiveFileDecruncher = new ArchiveFileDecruncher(FullPath, manager);

			ArchiveEntryInfo entryInfo = archiveFileDecruncher.OpenArchiveEntry(ArchiveDetector.GetEntryName(FullPath));

			CrunchedSize = entryInfo.CrunchedSize;
			ModuleSize = entryInfo.DecrunchedSize;

			if (ModuleSize == CrunchedSize)
				CrunchedSize = 0;

			return entryInfo.EntryStream;
		}



		/********************************************************************/
		/// <summary>
		/// Will try to open a file with the same name as the current module,
		/// but with a different extension. It will also try to use the
		/// extension as a prefix. You need to dispose the returned stream
		/// when done
		/// </summary>
		/********************************************************************/
		public override ModuleStream OpenExtraFile(string newExtension)
		{
			ArchiveEntryInfo entryInfo = null;

			foreach (string newFileName in GetExtraFileNames(newExtension))
			{
				entryInfo = TryOpenFile(newFileName);
				if (entryInfo != null)
					break;
			}

			// If a file is opened, decrunch it if needed
			if (entryInfo != null)
			{
				long fileSize = entryInfo.CrunchedSize;

				SingleFileDecruncher decruncher = new SingleFileDecruncher(manager);
				Stream stream = decruncher.DecrunchFileMultipleLevels(entryInfo.EntryStream);

				long newFileSize = stream.Length;
				AddSizes(fileSize, newFileSize);

				return new ModuleStream(stream, false);
			}

			return null;
		}

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
				return archiveFileDecruncher.OpenArchiveEntry(ArchiveDetector.GetEntryName(newFileName));
			}
			catch (Exception)
			{
				return null;
			}
		}
		#endregion
	}
}
