/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Library.Loaders.FileDecrunchers;

namespace Polycode.NostalgicPlayer.Library.Loaders.FileLoaders
{
	/// <summary>
	/// This loader can open just standard files in a file system
	/// </summary>
	internal class NormalFileLoader : FileLoaderBase
	{
		private readonly IFileDecruncherFactory fileDecruncherFactory;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NormalFileLoader(string fileName, IFileDecruncherFactory fileDecruncherFactory) : base(fileName)
		{
			this.fileDecruncherFactory = fileDecruncherFactory;
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
				SingleFileDecruncher decruncher = fileDecruncherFactory.GetSingleFileDecruncher();
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

				SingleFileDecruncher decruncher = fileDecruncherFactory.GetSingleFileDecruncher();
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
