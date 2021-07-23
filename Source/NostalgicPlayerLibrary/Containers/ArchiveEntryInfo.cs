/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Containers
{
	/// <summary>
	/// Hold information about an opened entry
	/// </summary>
	internal class ArchiveEntryInfo
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ArchiveEntryInfo(Stream entryStream, int crunchedSize, int decrunchedSize)
		{
			EntryStream = entryStream;
			CrunchedSize = crunchedSize;
			DecrunchedSize = decrunchedSize;
		}



		/********************************************************************/
		/// <summary>
		/// The stream to the entry
		/// </summary>
		/********************************************************************/
		public Stream EntryStream
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// The length of the crunched data. If -1, it means the crunched
		/// length is unknown
		/// </summary>
		/********************************************************************/
		public int CrunchedSize
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// The length of the decrunched data
		/// </summary>
		/********************************************************************/
		public int DecrunchedSize
		{
			get;
		}
	}
}
