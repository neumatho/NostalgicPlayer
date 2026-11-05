/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Sources;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Data
{
	/// <summary>
	/// Manages module library data from online sources
	/// </summary>
	public class OnlineData : DataBase
	{
		/********************************************************************/
		/// <summary>
		/// Get all available sources
		/// </summary>
		/********************************************************************/
		public List<IModLibrarySource> Sources
		{
			get;
		} = new List<IModLibrarySource>();



		/********************************************************************/
		/// <summary>
		/// Add a source to the library
		/// </summary>
		/********************************************************************/
		public void AddSource(IModLibrarySource source)
		{
			Sources.Add(source);
		}



		/********************************************************************/
		/// <summary>
		/// Get source by ID
		/// </summary>
		/********************************************************************/
		public IModLibrarySource GetSource(string sourceId)
		{
			return Sources.FirstOrDefault(s => s.Id == sourceId);
		}



		/********************************************************************/
		/// <summary>
		/// Validate the given breadcrumb path and return a new valid path
		/// where each part is the display name
		/// </summary>
		/********************************************************************/
		public string ValidatePath(string path)
		{
			IModLibrarySource source = GetSourceFromPath(path);
			if ((source == null) || !source.IsLoaded)
				return string.Empty;

			string[] pathParts = path.Substring(source.RootPath.Length).Split('/', StringSplitOptions.RemoveEmptyEntries);
			if (pathParts.Length == 0)
				return source.DisplayName;

			// Find the longest number of leading path parts that matches
			// a directory in one of the files
			int validParts = 0;

			foreach (ModEntry entry in source.Files)
			{
				IReadOnlyList<string> entryParts = entry.PathParts;
				int count = Math.Min(pathParts.Length, entryParts.Count);

				int matched = 0;
				while ((matched < count) && (entryParts[matched] == pathParts[matched]))
					matched++;

				if (matched > validParts)
				{
					validParts = matched;
					if (validParts == pathParts.Length)
						break;
				}
			}

			if (validParts == 0)
				return source.DisplayName;

			return source.DisplayName + "/" + string.Join('/', pathParts, 0, validParts);
		}



		/********************************************************************/
		/// <summary>
		/// Convert path to internal name
		/// </summary>
		/********************************************************************/
		public string ConvertPathToInternal(string path)
		{
			string[] pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
			if (pathParts.Length == 0)
				return string.Empty;

			IModLibrarySource source = GetSourceFromDisplayName(pathParts[0]);
			if (source == null)
				return path;

			return source.RootPath + string.Join('/', pathParts.Skip(1));
		}



		/********************************************************************/
		/// <summary>
		/// Convert internal path to a path where the source is shown with
		/// its display name
		/// </summary>
		/********************************************************************/
		public string ConvertPathToDisplay(string path)
		{
			IModLibrarySource source = GetSourceFromPath(path);
			if (source == null)
				return path;

			string rest = path.Substring(source.RootPath.Length);

			return string.IsNullOrEmpty(rest) ? source.DisplayName : source.DisplayName + "/" + rest;
		}

		#region DataBase overrides
		/********************************************************************/
		/// <summary>
		/// Create new tree builder
		/// </summary>
		/********************************************************************/
		protected override FilteredTreeBuilder CreateTreeBuilder(string searchText, SearchMode searchMode)
		{
			return new OnlineFilteredTreeBuilder(Sources, searchText, searchMode);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Get source from full path
		/// </summary>
		/********************************************************************/
		private IModLibrarySource GetSourceFromPath(string path)
		{
			foreach (IModLibrarySource source in Sources)
			{
				if (path.StartsWith(source.RootPath))
					return source;
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Get source from source display name
		/// </summary>
		/********************************************************************/
		private IModLibrarySource GetSourceFromDisplayName(string displayName)
		{
			foreach (IModLibrarySource source in Sources)
			{
				if (displayName == source.DisplayName)
					return source;
			}

			return null;
		}
		#endregion
	}
}
