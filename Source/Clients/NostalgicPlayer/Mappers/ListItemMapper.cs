/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow.ListItem;
using Polycode.NostalgicPlayer.Logic.MultiFiles;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Mappers
{
	/// <summary>
	/// This class can convert between a module list item and a file list item
	/// </summary>
	public static class ListItemMapper
	{
		/********************************************************************/
		/// <summary>
		/// Convert a list item to a list info
		/// </summary>
		/********************************************************************/
		public static MultiFileInfo Convert(ModuleListItem listItem)
		{
			MultiFileInfo multiFileInfo = new MultiFileInfo
			{
				Source = listItem.ListItem.Source,
				DisplayName = listItem.ListItem.DisplayName,
				PlayTime = listItem.HaveTime ? listItem.Duration : null,
				DefaultSubSong = listItem.DefaultSubSong
			};

			if (listItem.ListItem is SingleFileModuleListItem)
			{
				multiFileInfo.Type = MultiFileInfo.FileType.Plain;
			}
			else if (listItem.ListItem is ArchiveFileModuleListItem)
			{
				multiFileInfo.Type = MultiFileInfo.FileType.Archive;
			}
			else if (listItem.ListItem is UrlModuleListItem)
			{
				multiFileInfo.Type = MultiFileInfo.FileType.Url;
			}
			else if (listItem.ListItem is AudiusModuleListItem)
			{
				multiFileInfo.Type = MultiFileInfo.FileType.Audius;
			}
			else
				throw new NotImplementedException("Unknown module list implementation");

			return multiFileInfo;
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
					item = new SingleFileModuleListItem(fileInfo.Source);
					break;
				}

				case MultiFileInfo.FileType.Archive:
				{
					item = new ArchiveFileModuleListItem(fileInfo.Source);
					break;
				}

				case MultiFileInfo.FileType.Url:
				{
					item = new UrlModuleListItem(fileInfo.DisplayName, fileInfo.Source);
					break;
				}

				case MultiFileInfo.FileType.Audius:
				{
					item = new AudiusModuleListItem(fileInfo.DisplayName, fileInfo.Source);
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
