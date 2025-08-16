/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Library.Agent;
using Polycode.NostalgicPlayer.Library.Containers;

namespace Polycode.NostalgicPlayer.Library.Loaders
{
	/// <summary>
	/// This class manage opening and detection of archive files
	/// </summary>
	internal class ArchiveFileDecruncher : ArchiveDetector, IDisposable
	{
		private readonly string fullArchivePath;

		private Stack<Stream> archiveStreams;
		private List<string> decruncherAlgorithms;
		private IArchive archive;

		/********************************************************************/
		/// <summary>
		/// Constructor
		///
		/// The path is a little bit different than usual. Here is an example
		/// of such path:
		///
		/// M:\Modules\Archive.zip|Pro\MyMods.tar|A-F\Animotion.mod
		///
		/// This means, open the archive file Archive.zip in path M:\Modules.
		/// Inside the archive, open the archive file MyMods.tar, which is
		/// stored in the directory Pro inside the first archive. Then open
		/// the file Animotion.mod in the second archive file in the directory
		/// A-F.
		///
		/// Of course, it can be done more simple, if only one archive format
		/// is used e.g:
		///
		/// M:\Modules\AllMyModules.zip|Deep Joy.it
		/// </summary>
		/********************************************************************/
		public ArchiveFileDecruncher(string fullArchivePath, Manager agentManager) : base(agentManager)
		{
			this.fullArchivePath = fullArchivePath;

			OpenArchiveStream();
		}



		/********************************************************************/
		/// <summary>
		/// Free allocated stuff
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			manager = null;
			archive = null;

			CloseStreams();
		}



		/********************************************************************/
		/// <summary>
		/// Open the file in the main archive with the name given
		/// </summary>
		/********************************************************************/
		public ArchiveEntryInfo OpenArchiveEntry(string entryName)
		{
			using (ArchiveStream entryStream = archive.OpenEntry(entryName))
			{
				// Because an entry stream use the parent archive stream to seek around,
				// we cannot open multiple entry streams at the same time. SidPlay e.g.
				// will do that when trying to identify extra files.
				//
				// Because of this, we will copy the entry stream into a memory stream
				// and return that instead. It will use more memory to do so, but modules
				// are relative small anyway, so it should not matter that much
				Stream stream = new MemoryStream((int)entryStream.Length);

				if (entryStream.Length == 0)
					entryStream.CopyTo(stream);
				else
					Helpers.CopyData(entryStream, stream, (int)entryStream.Length);

				// The entry may be crunched, so decrunch it
				SingleFileDecruncher decruncher = new SingleFileDecruncher(manager);
				stream = decruncher.DecrunchFileMultipleLevels(stream);

				List<string> algorithms = new List<string>(decruncherAlgorithms);
				if (decruncher.DecruncherAlgorithms != null)
					algorithms.AddRange(decruncher.DecruncherAlgorithms);

				DecruncherAlgorithms = algorithms.ToArray();

				return new ArchiveEntryInfo(stream, entryStream.GetCrunchedLength(), (int)stream.Length);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return a list of all the algorithms used to decrunch the module.
		/// If null, no decruncher has been used
		/// </summary>
		/********************************************************************/
		public string[] DecruncherAlgorithms
		{
			get; private set;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will try to open a stream to the archive
		/// </summary>
		/********************************************************************/
		private void OpenArchiveStream()
		{
			archiveStreams = new Stack<Stream>();
			decruncherAlgorithms = new List<string>();

			try
			{
				string[] parts = fullArchivePath.Split('|');

				// First part is the full path to the archive itself in the file system,
				// so we just open that file normally
				Stream stream = new FileStream(parts[0], FileMode.Open, FileAccess.Read);

				// Open the archive stream
				archive = OpenArchive(Path.GetFileName(parts[0]), stream, out stream, out List<string> archiveAlgorimths);
				archiveStreams.Push(stream);
				decruncherAlgorithms.AddRange(archiveAlgorimths);

				// Now take each part and open it, except for the last which contains
				// the file itself
				for (int i = 1; i < parts.Length - 1; i++)
				{
					ArchiveEntryInfo entryInfo = OpenArchiveEntry(parts[i]);

					stream = entryInfo.EntryStream;

					archive = OpenArchive(parts[i], stream, out stream, out archiveAlgorimths);
					archiveStreams.Push(stream);
					decruncherAlgorithms.AddRange(archiveAlgorimths);
				}
			}
			catch(Exception)
			{
				CloseStreams();
				archive = null;
				throw;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Close all archive streams
		/// </summary>
		/********************************************************************/
		private void CloseStreams()
		{
			if (archiveStreams != null)
			{
				while (archiveStreams.Count > 0)
					archiveStreams.Pop().Dispose();

				archiveStreams = null;
			}
		}
		#endregion
	}
}
