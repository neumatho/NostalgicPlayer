/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Loaders
{
	/// <summary>
	/// Helper class for loader implementations
	/// </summary>
	public abstract class FileLoaderBase : ILoader
	{
		/// <summary>
		/// Holds information about the opened extra file
		/// </summary>
		protected struct StreamInfo
		{
			/// <summary>
			/// The new file name which is opened
			/// </summary>
			public string NewFileName;

			/// <summary>
			/// The crunched or original file size
			/// </summary>
			public long CrunchedSize;

			/// <summary>
			/// The size after decrunching or original file size
			/// </summary>
			public long DecrunchedSize;
		}

		private readonly string fileName;

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
		/// Will try to open the main file. You need to dispose the returned
		/// stream when done
		/// </summary>
		/********************************************************************/
		public abstract Stream OpenFile();



		/********************************************************************/
		/// <summary>
		/// Will try to open a file with the same name as the current module,
		/// but with a different extension. It will also try to use the
		/// extension as a prefix. Use this if you need to check extra files
		/// in the Identify() method. You need to dispose the returned stream
		/// when done
		/// </summary>
		/********************************************************************/
		public virtual ModuleStream OpenExtraFileForTest(string newExtension, out string newFileName)
		{
			ModuleStream moduleStream = OpenStream(newExtension, out StreamInfo streamInfo);
			newFileName = moduleStream != null ? streamInfo.NewFileName : null;

			return moduleStream;
		}



		/********************************************************************/
		/// <summary>
		/// Will try to open a file with the same name as the current module,
		/// but with a different extension. It will also try to use the
		/// extension as a prefix. Will add the file sizes to one or both of
		/// ModuleSize and CrunchedSize. You need to dispose the returned
		/// stream when done
		/// </summary>
		/********************************************************************/
		public virtual ModuleStream OpenExtraFile(string newExtension)
		{
			ModuleStream moduleStream = OpenStream(newExtension, out StreamInfo streamInfo);
			if (moduleStream != null)
				AddSizes(streamInfo.CrunchedSize, streamInfo.DecrunchedSize);

			return moduleStream;
		}



		/********************************************************************/
		/// <summary>
		/// Will try to open a file with the name given as extra file. Will
		/// add the file sizes to one or both of ModuleSize and CrunchedSize.
		/// You need to dispose the returned stream when done
		/// </summary>
		/********************************************************************/
		public virtual ModuleStream OpenExtraFileWithName(string fullFileName)
		{
			ModuleStream moduleStream = OpenStreamWithName(fullFileName, out StreamInfo streamInfo);
			if (moduleStream != null)
				AddSizes(streamInfo.CrunchedSize, streamInfo.DecrunchedSize);

			return moduleStream;
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

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Will try to open the extra file and return the stream and some
		/// info about the before and after lengths
		/// </summary>
		/********************************************************************/
		protected abstract ModuleStream OpenStream(string newExtension, out StreamInfo streamInfo);



		/********************************************************************/
		/// <summary>
		/// Will try to open the extra file and return the stream and some
		/// info about the before and after lengths
		/// </summary>
		/********************************************************************/
		protected abstract ModuleStream OpenStreamWithName(string fullFileName, out StreamInfo streamInfo);
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Return a collection of file names to try when opening extra files
		/// </summary>
		/********************************************************************/
		protected IEnumerable<string> GetExtraFileNames(string newExtension)
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
				string directory = Path.GetDirectoryName(fileName);
				string name = Path.GetFileName(fileName);

				int index = name.IndexOf('.');
				if (index != -1)
				{
					name = name.Substring(index + 1);

					newFileName = Path.Combine(directory, $"{newExtension}.{name}");
					yield return newFileName;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will add the sizes to the size properties
		/// </summary>
		/********************************************************************/
		protected void AddSizes(long crunchedSize, long decrunchedSize)
		{
			if (crunchedSize == decrunchedSize)
			{
				// Not crunched
				ModuleSize += crunchedSize;
				if (CrunchedSize > 0)
					CrunchedSize += crunchedSize;
			}
			else
			{
				// Crunched
				if (CrunchedSize > 0)
					CrunchedSize += crunchedSize;
				else
					CrunchedSize = ModuleSize + crunchedSize;

				ModuleSize += decrunchedSize;
			}
		}
		#endregion
	}
}
