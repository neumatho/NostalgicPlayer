/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Helpers
{
	/// <summary>
	/// Helper class to handle paths to archives
	/// </summary>
	public static class ArchivePath
	{
		/********************************************************************/
		/// <summary>
		/// Will check the given path, if it is an archive path or not
		/// </summary>
		/********************************************************************/
		public static bool IsArchivePath(string fullPath)
		{
			return fullPath.Contains('|');
		}



		/********************************************************************/
		/// <summary>
		/// Return the path to the archive only
		/// </summary>
		/********************************************************************/
		public static string GetArchiveName(string fullArchivePath)
		{
			int index = fullArchivePath.IndexOf('|');
			if (index != -1)
				return fullArchivePath.Substring(0, index);

			return string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Return the path to the entry
		/// </summary>
		/********************************************************************/
		public static string GetEntryPath(string fullArchivePath)
		{
			int index = fullArchivePath.IndexOf('|');
			if (index != -1)
				return fullArchivePath.Substring(index + 1);

			return string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Return the entry name only of the path given
		/// </summary>
		/********************************************************************/
		public static string GetEntryName(string fullArchivePath)
		{
			int index = fullArchivePath.LastIndexOf('|');
			if (index != -1)
				return fullArchivePath.Substring(index + 1);

			return string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Combine two path parts
		/// </summary>
		/********************************************************************/
		public static string CombinePathParts(string path, string entryPath)
		{
			return $"{path}|{entryPath}";
		}
	}
}
