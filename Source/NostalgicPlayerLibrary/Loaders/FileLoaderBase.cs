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
		/// extension as a prefix. You need to dispose the returned stream
		/// when done
		/// </summary>
		/********************************************************************/
		public abstract ModuleStream OpenExtraFile(string newExtension);



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

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Return a collection of file names to try when opening extra files
		/// </summary>
		/********************************************************************/
		protected IEnumerable<string> GetExtraFileNames(string newExtension)
		{
			if (string.IsNullOrEmpty(newExtension))
				yield break;

			// First change the extension
			string newFileName = Path.ChangeExtension(fileName, newExtension);
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
