/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Sources.ModLand
{
	/// <summary>
	/// Represents a module service provider (e.g., ModLand, AMP Archive)
	/// </summary>
	internal class ModLandSource : IModLibrarySource
	{
		private readonly IApplicationContext applicationContext;

		private readonly List<ModEntry> files = new List<ModEntry>();

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModLandSource(IApplicationContext applicationContext)
		{
			this.applicationContext = applicationContext;
		}



		/********************************************************************/
		/// <summary>
		/// Service identifier
		/// </summary>
		/********************************************************************/
		public string Id => "modland";



		/********************************************************************/
		/// <summary>
		/// Display name of the service
		/// </summary>
		/********************************************************************/
		public string DisplayName => Resources.IDS_MODLIBRARY_SOURCE_MODLAND;



		/********************************************************************/
		/// <summary>
		/// Folder name for storing service files (filesystem-safe name)
		/// </summary>
		/********************************************************************/
		public string FolderName => "ModLand";



		/********************************************************************/
		/// <summary>
		/// Root path for this service
		/// </summary>
		/********************************************************************/
		public string RootPath => "modland://";



		/********************************************************************/
		/// <summary>
		/// Whether the service database is loaded
		/// </summary>
		/********************************************************************/
		public bool IsLoaded
		{
			get;
			set;
		} = false;



		/********************************************************************/
		/// <summary>
		/// Last update timestamp
		/// </summary>
		/********************************************************************/
		public DateTime LastUpdate
		{
			get;
			set;
		}



		/********************************************************************/
		/// <summary>
		/// Get readonly access to files
		/// </summary>
		/********************************************************************/
		public IReadOnlyList<ModEntry> Files => files;



		/********************************************************************/
		/// <summary>
		/// Add file - creates ModEntry from already sorted data
		/// </summary>
		/********************************************************************/
		public void AddFile(string nameWithPath, long size)
		{
			files.Add(new ModEntry(nameWithPath, size));
		}



		/********************************************************************/
		/// <summary>
		/// Clear file collection
		/// </summary>
		/********************************************************************/
		public void ClearFiles()
		{
			files.Clear();
		}



		/********************************************************************/
		/// <summary>
		/// Return a new instance of a downloader
		/// </summary>
		/********************************************************************/
		public IModLibraryDownloader GetDownloader()
		{
			return applicationContext.Container.GetInstance<ModLandDownloader>();
		}
	}
}
