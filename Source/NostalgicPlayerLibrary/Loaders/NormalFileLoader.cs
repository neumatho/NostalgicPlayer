/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
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

		#region FileLoaderBase implementation
		/********************************************************************/
		/// <summary>
		/// Will try to open the main file.
		///
		/// You need to dispose the returned stream when done
		/// </summary>
		/********************************************************************/
		public override Stream OpenFile()
		{
			Stream stream = OpenFile(FullPath);

			try
			{
				// Get the original length of the file
				CrunchedSize = stream.Length;

				// First try to decrunch the file if needed
				SingleFileDecruncher decruncher = new SingleFileDecruncher(manager);
				stream = decruncher.DecrunchFileMultipleLevels(stream);

				// Update sizes
				ModuleSize = stream.Length;
				if (ModuleSize == CrunchedSize)
					CrunchedSize = 0;

				// Set algorithms used
				DecruncherAlgorithms = decruncher.DecruncherAlgorithms;

				return stream;
			}
			catch (Exception)
			{
				stream.Dispose();
				throw;
			}
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

			Stream stream = TryOpenFile(fullFileName);
			if (stream == null)
				return null;

			try
			{
				// Decrunch it if needed
				streamInfo.CrunchedSize = stream.Length;

				SingleFileDecruncher decruncher = new SingleFileDecruncher(manager);
				stream = decruncher.DecrunchFileMultipleLevels(stream);

				streamInfo.DecrunchedSize = stream.Length;

				return new ModuleStream(stream, false);
			}
			catch (Exception)
			{
				stream.Dispose();
				throw;
			}
		}
		#endregion

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
