﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow.ListItem;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MultiFiles
{
	/// <summary>
	/// This class can convert between a module list item and a file list item
	/// </summary>
	public static class ListItemConverter
	{
		/********************************************************************/
		/// <summary>
		/// Convert a list item to a list info
		/// </summary>
		/********************************************************************/
		public static MultiFileInfo Convert(ModuleListItem listItem)
		{
			if (listItem.ListItem is SingleFileListItem singleFile)
			{
				return new MultiFileInfo
				{
					Type = MultiFileInfo.FileType.Plain,
					FileName = singleFile.FullPath,
					PlayTime = listItem.HaveTime ? listItem.Duration : null,
					DefaultSubSong = listItem.DefaultSubSong
				};
			}

			if (listItem.ListItem is ArchiveFileListItem archiveFile)
			{
				return new MultiFileInfo
				{
					Type = MultiFileInfo.FileType.Archive,
					FileName = archiveFile.FullPath,
					PlayTime = listItem.HaveTime ? listItem.Duration : null,
					DefaultSubSong = listItem.DefaultSubSong
				};
			}

			throw new NotImplementedException("Unknown module list implementation");
		}



		/********************************************************************/
		/// <summary>
		/// Convert a list info to a list item
		/// </summary>
		/********************************************************************/
		public static ModuleListItem Convert(MultiFileInfo fileInfo)
		{
			IModuleListItem item;

			switch (fileInfo.Type)
			{
				case MultiFileInfo.FileType.Plain:
				{
					item = new SingleFileListItem(fileInfo.FileName);
					break;
				}

				case MultiFileInfo.FileType.Archive:
				{
					item = new ArchiveFileListItem(fileInfo.FileName);
					break;
				}

				default:
					throw new NotImplementedException($"File type ({fileInfo.Type}) not implemented");
			}

			ModuleListItem listItem = new ModuleListItem(item);
			if (fileInfo.PlayTime.HasValue)
				listItem.Duration = fileInfo.PlayTime.Value;

			listItem.DefaultSubSong = fileInfo.DefaultSubSong;

			return listItem;
		}
	}
}
