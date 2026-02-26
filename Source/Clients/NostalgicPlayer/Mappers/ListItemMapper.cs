/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.ListItems;
using Polycode.NostalgicPlayer.Logic.Playlists;

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
		public static PlaylistFileInfo Convert(ModuleListItem listItem)
		{
			PlaylistFileInfo multiFileInfo = new PlaylistFileInfo
			{
				Source = listItem.ListItem.Source,
				DisplayName = listItem.ListItem.DisplayName,
				PlayTime = listItem.HaveTime ? listItem.Duration : null,
				DefaultSubSong = listItem.DefaultSubSong
			};

			if (listItem.ListItem is SingleFileModuleListItem)
			{
				multiFileInfo.Type = PlaylistFileInfo.FileType.Plain;
			}
			else if (listItem.ListItem is ArchiveFileModuleListItem)
			{
				multiFileInfo.Type = PlaylistFileInfo.FileType.Archive;
			}
			else if (listItem.ListItem is UrlModuleListItem)
			{
				multiFileInfo.Type = PlaylistFileInfo.FileType.Url;
			}
			else if (listItem.ListItem is AudiusModuleListItem)
			{
				multiFileInfo.Type = PlaylistFileInfo.FileType.Audius;
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
		public static ModuleListItem Convert(PlaylistFileInfo fileInfo)
		{
			IModuleListItem item;

			switch (fileInfo.Type)
			{
				case PlaylistFileInfo.FileType.Plain:
				{
					item = new SingleFileModuleListItem(fileInfo.Source);
					break;
				}

				case PlaylistFileInfo.FileType.Archive:
				{
					item = new ArchiveFileModuleListItem(fileInfo.Source);
					break;
				}

				case PlaylistFileInfo.FileType.Url:
				{
					item = new UrlModuleListItem(fileInfo.DisplayName, fileInfo.Source);
					break;
				}

				case PlaylistFileInfo.FileType.Audius:
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
