/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using System.Linq;
using System.Text;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MultiFiles
{
	/// <summary>
	/// This class can create multi file loaders which can handle module lists
	/// </summary>
	public static class ListFactory
	{
		private static readonly byte[] npmlSignature;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		static ListFactory()
		{
			npmlSignature = Encoding.UTF8.GetBytes("@*NpML*@");
		}



		/********************************************************************/
		/// <summary>
		/// Return all available extensions for list files
		/// </summary>
		/********************************************************************/
		public static string[] GetExtensions()
		{
			return new NpmlList().FileExtensions;
		}



		/********************************************************************/
		/// <summary>
		/// Try to figure out which kind of list this is and return a loader
		/// if anyone could be found
		/// </summary>
		/********************************************************************/
		public static IMultiFileLoader Create(Stream stream)
		{
			byte[] buffer = new byte[16];

			// Make sure the file position is at the beginning of the file
			stream.Seek(0, SeekOrigin.Begin);

			// Read the first line
			int bytesRead = stream.Read(buffer, 0, 16);
			if (bytesRead >= 11)
			{
				if (buffer.Skip(3).Take(8).SequenceEqual(npmlSignature))	// Skip BOM
					return new NpmlList();
			}

			return null;
		}
	}
}
