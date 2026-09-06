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

namespace Polycode.NostalgicPlayer.Library.Test.Helpers
{
	/// <summary>
	/// Loader which serves the module from a byte array instead of a real file
	/// </summary>
	internal class TestLoader : ILoader
	{
		private readonly byte[] moduleData;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public TestLoader(byte[] moduleData)
		{
			this.moduleData = moduleData;
		}



		/********************************************************************/
		/// <summary>
		/// Dispose our self
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will try to open the main file. The loader opens the file more
		/// than once, so a new stream is returned every time
		/// </summary>
		/********************************************************************/
		public Stream OpenFile()
		{
			return new MemoryStream(moduleData, false);
		}



		/********************************************************************/
		/// <summary>
		/// Will return a collection of different kind of file names using
		/// the extension given
		/// </summary>
		/********************************************************************/
		public IEnumerable<string> GetPossibleFileNames(string newExtension)
		{
			throw new NotSupportedException();
		}



		/********************************************************************/
		/// <summary>
		/// Will try to open a file with the same name as the current module,
		/// but with a different extension
		/// </summary>
		/********************************************************************/
		public ModuleStream OpenExtraFileByExtension(string newExtension, bool addSize = true)
		{
			throw new NotSupportedException();
		}



		/********************************************************************/
		/// <summary>
		/// Will try to open a file with the name given as extra file
		/// </summary>
		/********************************************************************/
		public ModuleStream OpenExtraFileByFileName(string fullFileName, bool addSize = true)
		{
			throw new NotSupportedException();
		}



		/********************************************************************/
		/// <summary>
		/// Will try to open an external file in the "Instruments" directory
		/// </summary>
		/********************************************************************/
		public ModuleStream TryOpenExternalFileInInstruments(string externalFileName, out string usedDirectoryName)
		{
			throw new NotSupportedException();
		}



		/********************************************************************/
		/// <summary>
		/// Will try to open an external file in the directory given
		/// </summary>
		/********************************************************************/
		public ModuleStream TryOpenExternalFile(string externalDirectoryName, string externalFileName, out string usedDirectoryName)
		{
			throw new NotSupportedException();
		}



		/********************************************************************/
		/// <summary>
		/// Will add the sizes of the previous opened extra file to the
		/// size properties
		/// </summary>
		/********************************************************************/
		public void AddSizes()
		{
			throw new NotSupportedException();
		}



		/********************************************************************/
		/// <summary>
		/// Return the full path to the file
		/// </summary>
		/********************************************************************/
		public string FullPath => "test.tst";



		/********************************************************************/
		/// <summary>
		/// Return the size of the module loaded
		/// </summary>
		/********************************************************************/
		public long ModuleSize => moduleData.Length;



		/********************************************************************/
		/// <summary>
		/// Return the size of the module crunched. Is zero if not crunched
		/// </summary>
		/********************************************************************/
		public long CrunchedSize => 0;



		/********************************************************************/
		/// <summary>
		/// Return a list of all the algorithms used to decrunch the module.
		/// If null, no decruncher has been used
		/// </summary>
		/********************************************************************/
		public string[] DecruncherAlgorithms => null;
	}
}
