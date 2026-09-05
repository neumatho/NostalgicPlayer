/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io_Read;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;
using FileReader = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.FileReader;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpenMpt.Test.Tests
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public partial class Tests : Test
	{
		private readonly string dataDirectory;

		private static Default_Prng s_Prng = Seed.Make_Prng<Default_Prng, c_uint>(new Random_Device());

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Tests()
		{
			string solutionDirectory = GetSolutionDirectory();
			dataDirectory = Path.Combine(solutionDirectory, "Data");
		}



		/********************************************************************/
		/// <summary>
		/// Find the solution directory
		/// </summary>
		/********************************************************************/
		private string GetSolutionDirectory()
		{
			string directory = Environment.CurrentDirectory;

			while (!directory.EndsWith("\\LibOpenMpt.Test"))
			{
				int index = directory.LastIndexOf('\\');
				if (index == -1)
					throw new Exception("Could not find solution directory");

				directory = directory.Substring(0, index);
			}

			return directory;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private string GetTestFileNameBase()
		{
			return Path.Combine(dataDirectory, "Test.");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private CSoundFile CreateSoundFileContainer(string fileName)
		{
			using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
			{
				FileReader file = new Ports.LibOpenMpt.Common.FileReader(FileCursor_StdStream.Make_FileCursor<PathString>(fileStream));

				CSoundFile pSndFile = new CSoundFile();
				pSndFile.Create(file, ModLoadingFlags.LoadCompleteModule);

				return pSndFile;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private size_t strnlen(CPointer<uint8> str, size_t n)
		{
			for (size_t i = 0; i < n; ++i)
			{
				if (str[i] == '\0')
					return i;
			}

			return n;
		}
	}
}
