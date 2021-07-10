/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Polycode.NostalgicPlayer.PlayerLibrary.Loaders;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow.ListItem
{
	/// <summary>
	/// This class holds a list item pointing to a file
	/// </summary>
	public class SingleFileListItem : IModuleListItem
	{
		private readonly Manager manager;

		#region IModuleListItem implementation
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SingleFileListItem(string fullFileName, Manager agentManager)
		{
			FullPath = fullFileName;
			manager = agentManager;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name which is shown in the list
		/// </summary>
		/********************************************************************/
		public string DisplayName
		{
			get
			{
				return Path.GetFileName(FullPath);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the full path to the file
		/// </summary>
		/********************************************************************/
		public string FullPath
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the loader which can open the files needed
		/// </summary>
		/********************************************************************/
		public ILoader GetLoader()
		{
			return new NormalFileLoader(FullPath, manager);
		}
		#endregion
	}
}
