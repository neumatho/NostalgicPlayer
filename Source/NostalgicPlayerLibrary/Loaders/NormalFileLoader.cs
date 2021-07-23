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

namespace Polycode.NostalgicPlayer.PlayerLibrary.Loaders
{
	/// <summary>
	/// This loader can open just standard files in a file system
	/// </summary>
	public class NormalFileLoader : FileLoaderBase
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NormalFileLoader(string fileName, Manager agentManager) : base(fileName, agentManager)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will try to open the main file. You need to dispose the returned
		/// stream when done
		/// </summary>
		/********************************************************************/
		public override Stream OpenFile()
		{
			Stream stream = OpenFile(FullPath);

			// Get the original length of the file
			CrunchedSize = stream.Length;

			// First try to decrunch the file if needed
			SingleFileDecruncher decruncher = new SingleFileDecruncher(manager);
			stream = decruncher.DecrunchFileMultipleLevels(stream);

			// Update sizes
			ModuleSize = stream.Length;
			if (ModuleSize == CrunchedSize)
				CrunchedSize = 0;

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
		public override ModuleStream OpenExtraFile(string newExtension)
		{
			Stream stream = null;

			foreach (string newFileName in GetExtraFileNames(newExtension))
			{
				stream = TryOpenFile(newFileName);
				if (stream != null)
					break;
			}

			// If a file is opened, decrunch it if needed
			if (stream != null)
			{
				long fileSize = stream.Length;

				SingleFileDecruncher decruncher = new SingleFileDecruncher(manager);
				stream = decruncher.DecrunchFileMultipleLevels(stream);

				long newFileSize = stream.Length;
				AddSizes(fileSize, newFileSize);

				return new ModuleStream(stream, false);
			}

			return null;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will try to open the main file. You need to dispose the returned
		/// stream when done
		/// </summary>
		/********************************************************************/
		private Stream OpenFile(string fileName)
		{
			return new FileStream(fileName, FileMode.Open, FileAccess.Read);
		}



		/********************************************************************/
		/// <summary>
		/// Will try to open the file given and return null if it could not
		/// be opened
		/// </summary>
		/********************************************************************/
		private Stream TryOpenFile(string newFileName)
		{
			try
			{
				return OpenFile(newFileName);
			}
			catch (Exception)
			{
				return null;
			}
		}
		#endregion
	}
}
