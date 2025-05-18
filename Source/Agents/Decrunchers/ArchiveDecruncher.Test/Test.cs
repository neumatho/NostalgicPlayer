/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Security.Cryptography;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Test
{
	/// <summary>
	/// Helper methods to the different tests
	/// </summary>
	public abstract class Test
	{
		/// <summary>
		/// 
		/// </summary>
		protected readonly string dataDirectory;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected Test()
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

			while (!directory.EndsWith("\\ArchiveDecruncher.Test"))
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
		/// Calculate MD5 hash
		/// </summary>
		/********************************************************************/
		protected string CalculateMd5Hash(byte[] buffer)
		{
			byte[] hash = MD5.HashData(buffer);

			return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
		}
	}
}
