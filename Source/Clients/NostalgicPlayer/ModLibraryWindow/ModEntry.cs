/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Linq;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.ModLibraryWindow
{
	/// <summary>
	/// Represents a module file entry (not directories - only files!)
	/// </summary>
	internal class ModEntry
	{
		private List<string> pathParts;
		private string fileName;

		public long Size { get; }

		/// <summary>
		/// Full path (all directory parts concatenated)
		/// </summary>
		public string FullPath => pathParts.Count > 0 ? string.Join("/", pathParts) : "";

		/// <summary>
		/// Full name (full path + file name)
		/// </summary>
		public string FullName => string.IsNullOrEmpty(FullPath) ? fileName : FullPath + "/" + fileName;

		/// <summary>
		/// File or directory name only
		/// </summary>
		public string Name => fileName;

		/// <summary>
		/// Path parts as array for tree building
		/// </summary>
		public IReadOnlyList<string> PathParts => pathParts.AsReadOnly();



		/********************************************************************/
		/// <summary>
		/// Constructor - parses path once and stores components
		/// </summary>
		/********************************************************************/
		public ModEntry(string nameWithPath, long size)
		{
			Size = size;

			// Split path into parts
			if (string.IsNullOrEmpty(nameWithPath))
			{
				pathParts = new List<string>();
				fileName = "";
			}
			else
			{
				string[] parts = nameWithPath.Split('/');
				if (parts.Length > 1)
				{
					// Last part is the file name, rest is path
					pathParts = parts.Take(parts.Length - 1).ToList();
					fileName = parts[parts.Length - 1];
				}
				else
				{
					// No path, just filename
					pathParts = new List<string>();
					fileName = parts[0];
				}
			}
		}
	}
}
