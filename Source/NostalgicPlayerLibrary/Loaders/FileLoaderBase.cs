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
		private readonly Manager manager;

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
		/// Will try to open the file given
		/// </summary>
		/********************************************************************/
		public Stream OpenFile()
		{
			Stream stream = OpenFile(fileName);

			// Get the original length of the file
			PackedSize = stream.Length;

			// First try to depack the file if needed
			SingleFileDepacker depacker = new SingleFileDepacker(manager);
			stream = depacker.DepackFileMultipleLevels(stream);

			// Update sizes
			ModuleSize = stream.Length;
			if (ModuleSize == PackedSize)
				PackedSize = 0;

			return stream;
		}



		/********************************************************************/
		/// <summary>
		/// Will try to open a file with the same name as the current module,
		/// but with a different extension. It will also try to use the
		/// extension as a prefix. You need to dispose the returned stream
		/// when done
		/// </summary>
		/********************************************************************/
		public ModuleStream OpenExtraFile(string newExtension)
		{
			if (string.IsNullOrEmpty(newExtension))
				return null;

			// First change the extension
			string newFileName = Path.ChangeExtension(fileName, newExtension);
			Stream stream = TryOpenFile(newFileName);
			if (stream == null)
			{
				// Now try to append the extension
				newFileName = fileName + $".{newExtension}";
				stream = TryOpenFile(newFileName);
				if (stream == null)
				{
					// Try with prefix
					string directory = Path.GetDirectoryName(fileName);
					string name = Path.GetFileName(fileName);

					int index = name.IndexOf('.');
					if (index != -1)
					{
						name = name.Substring(index + 1);

						newFileName = Path.Combine(directory, $"{newExtension}.{name}");
						stream = TryOpenFile(newFileName);
					}
				}
			}

			// If a file is opened, depack it if needed
			if (stream != null)
			{
				long fileSize = stream.Length;

				SingleFileDepacker depacker = new SingleFileDepacker(manager);
				stream = depacker.DepackFileMultipleLevels(stream);

				long newFileSize = stream.Length;
				if (newFileSize == fileSize)
				{
					// Not packed
					ModuleSize += fileSize;
					if (PackedSize != 0)
						PackedSize += fileSize;
				}
				else
				{
					// Packed
					if (PackedSize != 0)
						PackedSize += fileSize;
					else
						PackedSize = ModuleSize + fileSize;

					ModuleSize += newFileSize;
				}

				return new ModuleStream(stream, false);
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Return the full path to the file
		/// </summary>
		/********************************************************************/
		public virtual string FullPath => fileName;



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
		/// Return the size of the module packed. Is zero if not packed
		/// </summary>
		/********************************************************************/
		public long PackedSize
		{
			get; protected set;
		}



		/********************************************************************/
		/// <summary>
		/// Will try to open the file given
		/// </summary>
		/********************************************************************/
		protected abstract Stream OpenFile(string fileName);

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will try to open the file given and return null if it could not
		/// be opened
		/// </summary>
		/********************************************************************/
		private Stream TryOpenFile(string fileName)
		{
			try
			{
				return OpenFile(fileName);
			}
			catch (Exception)
			{
				return null;
			}
		}
		#endregion
	}
}
