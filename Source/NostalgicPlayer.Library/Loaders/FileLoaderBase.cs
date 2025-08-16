/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Library.Agent;

namespace Polycode.NostalgicPlayer.Library.Loaders
{
	/// <summary>
	/// Helper class for loader implementations
	/// </summary>
	public abstract class FileLoaderBase : ILoader
	{
		/// <summary>
		/// Holds information about the opened extra file
		/// </summary>
		protected class StreamInfo
		{
			/// <summary>
			/// The crunched or original file size
			/// </summary>
			public long CrunchedSize { get; set; }

			/// <summary>
			/// The size after decrunching or original file size
			/// </summary>
			public long DecrunchedSize { get; set; }
		}

		private readonly string fileName;
		private StreamInfo lastExtraFileInfo;

		/// <summary></summary>
		protected Manager manager;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected FileLoaderBase(string fileName, Manager agentManager)
		{
			this.fileName = fileName;
			lastExtraFileInfo = null;

			manager = agentManager;
		}



		/********************************************************************/
		/// <summary>
		/// Free allocated stuff
		/// </summary>
		/********************************************************************/
		public virtual void Dispose()
		{
			manager = null;
		}



		/********************************************************************/
		/// <summary>
		/// Will try to open the main file.
		///
		/// You need to dispose the returned stream when done
		/// </summary>
		/********************************************************************/
		public abstract Stream OpenFile();



		/********************************************************************/
		/// <summary>
		/// Will return a collection of different kind of file names using
		/// the extension given
		/// </summary>
		/********************************************************************/
		public IEnumerable<string> GetPossibleFileNames(string newExtension)
		{
			string newFileName;

			if (string.IsNullOrEmpty(newExtension))
			{
				newFileName = Path.ChangeExtension(fileName, newExtension);
				if (newFileName.EndsWith('.'))
					newFileName = newFileName[0..^1];

				yield return newFileName;
			}
			else
			{
				// First change the extension
				newFileName = Path.ChangeExtension(fileName, newExtension);
				yield return newFileName;

				// Now try to append the extension
				newFileName = fileName + $".{newExtension}";
				yield return newFileName;

				// Try with prefix
				if (ArchivePath.IsArchivePath(fileName))
				{
					string archiveName = ArchivePath.GetArchiveName(fileName);
					string name = ArchivePath.GetEntryName(fileName);
					string directoryName = Path.GetDirectoryName(name);
					name = Path.GetFileName(name);

					int index = name.IndexOf('.');
					if (index != -1)
					{
						name = name.Substring(index + 1);

						newFileName = ArchivePath.CombinePathParts(archiveName, Path.Combine(directoryName, $"{newExtension}.{name}"));
						yield return newFileName;

						newFileName = Path.ChangeExtension(newFileName, string.Empty)[0..^1];
						yield return newFileName;
					}
				}
				else
				{
					string directoryName = Path.GetDirectoryName(fileName);
					string name = Path.GetFileName(fileName);

					int index = name.IndexOf('.');
					if (index != -1)
					{
						name = name.Substring(index + 1);

						newFileName = Path.Combine(directoryName, $"{newExtension}.{name}");
						yield return newFileName;

						newFileName = Path.ChangeExtension(newFileName, string.Empty)[0..^1];
						yield return newFileName;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will try to open a file with the same name as the current module,
		/// but with a different extension. It will also try to use the
		/// extension as a prefix.
		/// 
		/// If addSize is set to true, it will add the file sizes to one or
		/// both of ModuleSize and CrunchedSize. If false, you need to call
		/// AddSizes() by yourself, if you want to include the opened file
		/// as part of the collection of loaded files. This has to be done
		/// before calling this method again.
		///
		/// You need to dispose the returned stream when done
		/// </summary>
		/********************************************************************/
		public ModuleStream OpenExtraFileByExtension(string newExtension, bool addSize)
		{
			foreach (string newFileName in GetPossibleFileNames(newExtension))
			{
				ModuleStream moduleStream = OpenStream(newFileName, out lastExtraFileInfo);
				if (moduleStream != null)
				{
					if (addSize)
						AddSizes();

					return moduleStream;
				}
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Will try to open a file with the name given as extra file.
		/// 
		/// If addSize is set to true, it will add the file sizes to one or
		/// both of ModuleSize and CrunchedSize. If false, you need to call
		/// AddSizes() by yourself, if you want to include the opened file
		/// as part of the collection of loaded files. This has to be done
		/// before calling this method again.
		/// 
		/// You need to dispose the returned stream when done
		/// </summary>
		/********************************************************************/
		public ModuleStream OpenExtraFileByFileName(string fullFileName, bool addSize)
		{
			ModuleStream moduleStream = OpenStream(fullFileName, out lastExtraFileInfo);
			if ((moduleStream != null) && addSize)
				AddSizes();

			return moduleStream;
		}



		/********************************************************************/
		/// <summary>
		/// Will try to open an external file in the "Instruments" directory.
		/// The search starts from the current directory where the module is
		/// stored and then goes up to the root directory. If the file cannot
		/// be found, null is returned
		/// </summary>
		/********************************************************************/
		public ModuleStream TryOpenExternalFileInInstruments(string externalFileName, out string usedDirectoryName)
		{
			return TryOpenExternalFile("Instruments", externalFileName, out usedDirectoryName);
		}



		/********************************************************************/
		/// <summary>
		/// Will try to open an external file in the directory given. The
		/// search starts from the current directory where the module is
		/// stored and then goes up to the root directory. If the file cannot
		/// be found, null is returned
		/// </summary>
		/********************************************************************/
		public ModuleStream TryOpenExternalFile(string externalDirectoryName, string externalFileName, out string usedDirectoryName)
		{
			bool isArchive = ArchivePath.IsArchivePath(fileName);
			string moduleDirectoryName = isArchive ? Path.GetDirectoryName(ArchivePath.GetEntryName(fileName)) : Path.GetDirectoryName(fileName);

			for (;;)
			{
				string newDirectory = isArchive ? ArchivePath.CombinePathParts(ArchivePath.GetArchiveName(fileName), moduleDirectoryName) : moduleDirectoryName;
				usedDirectoryName = Path.Combine(newDirectory, externalDirectoryName);
				string fullPath = Path.Combine(usedDirectoryName, externalFileName);

				ModuleStream instrumentStream = OpenExtraFileByFileName(fullPath, true);

				// Did we get any file at all
				if (instrumentStream != null)
					return instrumentStream;

				int index = moduleDirectoryName.LastIndexOf(Path.DirectorySeparatorChar);
				if (index == -1)
					break;

				moduleDirectoryName = moduleDirectoryName.Substring(0, index);
				if (string.IsNullOrEmpty(moduleDirectoryName) || (moduleDirectoryName[^1] == ':'))
					break;
			}

			usedDirectoryName = null;

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Will add the sizes of the previous opened extra file to the
		/// size properties
		/// </summary>
		/********************************************************************/
		public void AddSizes()
		{
			if (lastExtraFileInfo != null)
			{
				if (lastExtraFileInfo.CrunchedSize == -1)
				{
					// Unknown crunched
					ModuleSize += lastExtraFileInfo.DecrunchedSize;
				}
				else if (lastExtraFileInfo.CrunchedSize == lastExtraFileInfo.DecrunchedSize)
				{
					// Not crunched
					ModuleSize += lastExtraFileInfo.CrunchedSize;
					if (CrunchedSize > 0)
						CrunchedSize += lastExtraFileInfo.CrunchedSize;
				}
				else
				{
					// Crunched
					if (CrunchedSize > 0)
						CrunchedSize += lastExtraFileInfo.CrunchedSize;
					else
						CrunchedSize = ModuleSize + lastExtraFileInfo.CrunchedSize;

					ModuleSize += lastExtraFileInfo.DecrunchedSize;
				}

				lastExtraFileInfo = null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the full path to the file
		/// </summary>
		/********************************************************************/
		public string FullPath => fileName;



		/********************************************************************/
		/// <summary>
		/// Return the size of the module loaded
		/// </summary>
		/********************************************************************/
		public long ModuleSize
		{
			get; protected set;
		}



		/********************************************************************/
		/// <summary>
		/// Return the size of the module crunched. Is zero if not crunched.
		/// If -1, it means the crunched length is unknown
		/// </summary>
		/********************************************************************/
		public long CrunchedSize
		{
			get; protected set;
		}



		/********************************************************************/
		/// <summary>
		/// Return a list of all the algorithms used to decrunch the module.
		/// If null, no decruncher has been used
		/// </summary>
		/********************************************************************/
		public string[] DecruncherAlgorithms
		{
			get; protected set;
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Will try to open the extra file and return the stream and some
		/// info about the before and after lengths
		/// </summary>
		/********************************************************************/
		protected abstract ModuleStream OpenStream(string fullFileName, out StreamInfo streamInfo);
		#endregion
	}
}
